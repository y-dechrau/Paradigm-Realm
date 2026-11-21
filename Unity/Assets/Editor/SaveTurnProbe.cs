using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (owner report, 21 Nov 2026: turns far too long on a saved game): plays turns of a save with no drawing and
// writes a line after every piece of every turn, flushed at once, so a turn that never ends shows exactly where it stopped.
// Unity -batchmode -executeMethod ParadigmRealm.SaveTurnProbe.Run -probeSave <save.json> -probeOut <log> [-probeTurns 5]
public static class SaveTurnProbe {
 static string Arg(string[] a,string key,string fallback){int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
 public static void Run(){
  var args=Environment.GetCommandLineArgs();string save=Arg(args,"-probeSave",""),outPath=Arg(args,"-probeOut","save-turns.txt");int turns=int.Parse(Arg(args,"-probeTurns","5"));
  using(var log=new StreamWriter(outPath,false){AutoFlush=true}){
   try{
    var clock=Stopwatch.StartNew();var w=JsonUtility.FromJson<World>(File.ReadAllText(save));w.UpgradeSave();w.dormantWorlds=true;w.EnableMaterials();
    log.WriteLine("loaded turn "+w.turn+" in "+clock.ElapsedMilliseconds+" ms; nations "+w.civCount+", cities "+w.cities.Count+", units "+w.units.Count+"; valid "+w.Valid());
    World.decisionTimes=new System.Collections.Generic.Dictionary<string,double>();
    for(int t=0;t<turns;t++){
     var turnClock=Stopwatch.StartNew();var it=w.TurnSteps().GetEnumerator();var step=new Stopwatch();int pieces=0;
     while(true){step.Restart();bool more=it.MoveNext();double ms=step.Elapsed.TotalMilliseconds;pieces++;
      int who=more?it.Current%10000:-1,part=more?it.Current/10000:0;
      if(ms>50)log.WriteLine("   slow piece "+ms.ToString("0")+" ms: after "+who+(who>=0&&who<w.civCount?" ("+w.NationName(who)+")":" (world)")+" part "+part);
      if(!more)break;
      if(pieces%200==0)log.WriteLine("   ... "+pieces+" pieces, last after "+who+" part "+part+", "+turnClock.ElapsedMilliseconds+" ms so far");}
     log.WriteLine("turn "+w.turn+" done in "+turnClock.ElapsedMilliseconds+" ms; decisions: "+string.Join(", ",World.decisionTimes.OrderByDescending(p=>p.Value).Take(12).Select(p=>p.Key+" "+p.Value.ToString("0")).ToArray()));World.decisionTimes.Clear();
    }
   }catch(Exception e){log.WriteLine("FAILED: "+e);}
  }
  EditorApplication.Exit(0);
 }
}
}
