using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
namespace ParadigmRealm {
// Development aid (owner report, 27 Nov 2026: a city making a Settler did not seem to lose population): plays a city through a
// Settler with real End Turns and prints its size, food and people each turn.
// Unity -batchmode -executeMethod ParadigmRealm.SettlerProbe.Run -probeOut <file>
public static class SettlerProbe {
 public static void Run(){
  var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-probeOut");string outFile=i>=0&&i+1<args.Length?args[i+1]:"settler.txt";var sb=new StringBuilder();
  try{
   var w=World.ValleyTalkScenario("plenty");var c=w.cities.First(x=>x.owner==0);c.pop=5;w.ChooseProject(c,World.UnitBuild(0),false);
   for(int t=0;t<25;t++){int settlers=w.units.Count(u=>u.owner==0&&u.type==0);sb.AppendLine("turn "+w.turn+": size "+c.pop+", food "+c.food+", build "+World.BuildName(c.build)+" "+c.production+"/"+w.CostFor(c,c.build)+", settlers "+settlers);
    w.EndTurn();int after=w.units.Count(u=>u.owner==0&&u.type==0);if(after>settlers)sb.AppendLine("   -> a Settler was trained; size now "+c.pop);if(c.build!=World.UnitBuild(0))w.ChooseProject(c,World.UnitBuild(0),false);}
  }catch(Exception e){sb.AppendLine("FAILED: "+e);}
  File.WriteAllText(outFile,sb.ToString());EditorApplication.Exit(0);
 }
}
}
