using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
namespace ParadigmRealm {
// Development aid (owner question, 20 Nov 2026: would the game run faster if the parallel worlds were only simulated once
// discovered?). Plays whole games with no drawing, as New Game sets them up, and times every piece of every End Turn by whose
// it is: the home rivals, the peoples of the parallel worlds, or the world-wide steps everyone shares (religion, influence,
// diplomacy, animals, sight). Run: Unity -batchmode -executeMethod ParadigmRealm.RealmProbe.Run -probeOut <file>
// [-probeTurns 250]. Writes a table per map size every 50 turns.
public static class RealmProbe {
 static string Arg(string[] a,string key,string fallback){int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
 static readonly string[] Shared={"","territory","your economy","religion","influence","diplomacy","victory checks","gateways and projects","animals and barbarians","sickness","sight","rival diplomacy"};
 public static void Run(){
  var args=Environment.GetCommandLineArgs();string outPath=Arg(args,"-probeOut","realm-probe.txt");int turns=int.Parse(Arg(args,"-probeTurns","250"));
  var sb=new StringBuilder();
  try{
   bool big=Arg(args,"-probeLarge","0")=="1";World.decisionTimes=new Dictionary<string,double>();
   foreach(var setup in big?new[]{new[]{1,3},new[]{3,World.MaxCivsBySize[3]}}:new[]{new[]{1,3}}){Probe(setup[0],setup[1],turns,sb);File.WriteAllText(outPath,sb.ToString());}
  }catch(Exception e){sb.AppendLine("FAILED: "+e);File.WriteAllText(outPath,sb.ToString());}
  EditorApplication.Exit(0);
 }
 static void Probe(int size,int civs,int turns,StringBuilder sb){
  var w=World.New(4242,"Probe",0,1,1,0,size,1,civs,hex:true,peoplesPerWorld:10,level:DifficultyTable.Steward,wild:true,rations:true,animalShare:World.AnimalShares[3]);
  w.EnableDimensions();w.barbarians=true;
  var founder=w.units.FirstOrDefault(u=>u.owner==0&&u.type==0);if(founder!=null)w.Found(founder);
  int home=Enumerable.Range(1,w.civCount-1).Count(o=>!w.IsNativeNation(o)),native=Enumerable.Range(1,w.civCount-1).Count(o=>w.IsNativeNation(o));
  sb.AppendLine("=== "+World.MapSizeNames[size]+" map: "+w.tiles.Length+" tiles, you + "+home+" home rivals + "+native+" parallel-world peoples ===");
  var homeParts=new Dictionary<string,double>();double bHome=0,bNative=0,bShared=0,bAll=0;var shared=new double[Shared.Length+1];int bTurns=0;var clock=new Stopwatch();
  for(int t=1;t<=turns&&w.result=="";t++){
   if(w.research<0||!w.AvailableTech(0).Contains(w.research))w.SelectResearch(w.AvailableTech(0).DefaultIfEmpty(0).First());
   w.researchPrompts.Clear();w.buildPrompts.Clear();w.buildNotices.Clear();w.rivalCalls.Clear();w.newContacts.Clear();
   var it=w.TurnSteps().GetEnumerator();
   while(true){clock.Restart();bool more=it.MoveNext();double ms=clock.Elapsed.TotalMilliseconds;bAll+=ms;
    int who=more?it.Current%10000:-1,part=more?it.Current/10000:0;
    if(who>=1&&who<w.civCount){if(w.IsNativeNation(who))bNative+=ms;else{bHome+=ms;string k=part>=100?"unit "+World.UnitNames[Math.Min(World.UnitNames.Length-1,part-100)]:part>=10?"economy":part==1?"empire plan":part==2?"city choices":part==3||part==4?"before units":"last unit";double had;homeParts.TryGetValue(k,out had);homeParts[k]=had+ms;}}
    else{bShared+=ms;int k=who>=w.civCount?who-w.civCount:Shared.Length;shared[Math.Min(Shared.Length,Math.Max(0,k))]+=ms;}
    if(!more)break;}
   bTurns++;
   if(t%50==0||t==turns||w.result!=""){
    int nativeCities=w.cities.Count(c=>w.IsNativeNation(c.owner)),homeCities=w.cities.Count(c=>!w.IsNativeNation(c.owner));
    int nativeUnits=w.units.Count(u=>w.IsNativeNation(u.owner)),homeUnits=w.units.Count(u=>!w.IsNativeNation(u.owner));
    sb.AppendLine("turns "+(t-bTurns+1)+"-"+t+": average End Turn "+(bAll/bTurns).ToString("0")+" ms");
    sb.AppendLine("   parallel-world peoples "+Pct(bNative,bAll)+"  ("+(bNative/bTurns).ToString("0")+" ms; "+nativeCities+" cities, "+nativeUnits+" units)");
    sb.AppendLine("   home rivals            "+Pct(bHome,bAll)+"  ("+(bHome/bTurns).ToString("0")+" ms; "+homeCities+" cities incl. yours, "+homeUnits+" units)");
    sb.AppendLine("   shared world steps     "+Pct(bShared,bAll)+"  ("+(bShared/bTurns).ToString("0")+" ms): "+string.Join(", ",Enumerable.Range(0,shared.Length).Where(k=>shared[k]>bAll*.02).OrderByDescending(k=>shared[k]).Select(k=>(k<Shared.Length?Shared[k]:"other")+" "+Pct(shared[k],bAll)).ToArray()));
    sb.AppendLine("   home rival pieces: "+string.Join(", ",homeParts.OrderByDescending(p=>p.Value).Take(6).Select(p=>p.Key+" "+(p.Value/bTurns).ToString("0")+" ms").ToArray()));
    sb.AppendLine("   unit decisions, all nations: "+string.Join(", ",World.decisionTimes.OrderByDescending(p=>p.Value).Take(8).Select(p=>p.Key+" "+(p.Value/bTurns).ToString("0")+" ms").ToArray()));homeParts.Clear();World.decisionTimes.Clear();
    sb.AppendLine("   gateways open: "+(!w.gatesLocked?"all":(w.openedGates!=null?w.openedGates.Count:0)+"")+"; colonies "+(w.colonies!=null?w.colonies.Count:0));
    bHome=bNative=bShared=bAll=0;bTurns=0;Array.Clear(shared,0,shared.Length);
   }
  }
  sb.AppendLine();
 }
 static string Pct(double part,double all){return (all<=0?0:100*part/all).ToString("0").PadLeft(3)+"%";}
}
}
