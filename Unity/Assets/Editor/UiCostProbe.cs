using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (owner report 26 Nov 2026: the game froze): times every calculation the new screens make on a save.
// Unity -batchmode -executeMethod ParadigmRealm.UiCostProbe.Run -probeSave <save.json> -probeOut <log>
public static class UiCostProbe {
 static string Arg(string[] a,string k,string d){int i=Array.IndexOf(a,k);return i>=0&&i+1<a.Length?a[i+1]:d;}
 public static void Run(){
  var args=Environment.GetCommandLineArgs();var sb=new StringBuilder();string outPath=Arg(args,"-probeOut","ui-cost.txt");
  try{
   var w=JsonUtility.FromJson<World>(File.ReadAllText(Arg(args,"-probeSave","")));w.UpgradeSave();w.dormantWorlds=true;w.lifeways=true;w.EnableStatecraft();w.EnableMaterials();
   var clock=new Stopwatch();Func<Action,double> T=a=>{clock.Restart();a();return clock.Elapsed.TotalMilliseconds;};
   sb.AppendLine("turn "+w.turn+", cities "+w.cities.Count+", own "+w.cities.Count(c=>c.owner==0)+", units "+w.units.Count+", outposts "+(w.outposts==null?0:w.outposts.Count));
   sb.AppendLine("MaterialJobs(0) "+T(()=>w.MaterialJobs(0)).ToString("0.0")+" ms; forecast "+T(()=>w.MaterialForecast(0)).ToString("0.0")+" ms; net "+T(()=>w.MaterialNetNext(0)).ToString("0.0")+" ms; income "+T(()=>w.MaterialIncome(0)).ToString("0.0")+" ms");
   foreach(var c in w.cities.Where(c=>c.owner==0)){double line=T(()=>{w.MaterialLine(c,true);w.MaterialLine(c,false);w.MaterialBrief(c,true);w.MaterialBrief(c,false);});double worst=0;int wb=-1;
    foreach(var b in World.BuildingIds.Concat(Enumerable.Range(0,World.UnitNames.Length).Select(World.UnitBuild))){double ms=T(()=>w.MaterialPreview(c,b));if(ms>worst){worst=ms;wb=b;}}
    sb.AppendLine("city "+c.name+": lines "+line.ToString("0.0")+" ms, slowest preview "+worst.ToString("0.0")+" ms ("+World.BuildName(wb)+")");}
   double worstSite=0,total=0;int n=0;Tile ws=null;
   foreach(var t in w.tiles.Where(t=>t.seen&&t.owner!=0&&(World.Wooded(t)||t.terrain==2))){double ms=T(()=>w.AssessSite(0,t));total+=ms;n++;if(ms>worstSite){worstSite=ms;ws=t;}if(clock.ElapsedMilliseconds>60000)break;}
   sb.AppendLine("site panels: "+n+" tiles, average "+(n>0?total/n:0).ToString("0.0")+" ms, slowest "+worstSite.ToString("0.0")+" ms"+(ws!=null?" at "+ws.x+","+ws.y:""));
   File.WriteAllText(outPath,sb.ToString());
   // A rival worker's camp search, the costliest new rival step.
   double ai=0;int workers=0;foreach(var u in w.units.Where(u=>u.owner>0&&u.type==1&&!w.IsNativeNation(u.owner)).Take(20)){workers++;ai+=T(()=>w.CampAIForProbe(u));}
   sb.AppendLine("rival camp search: "+workers+" workers, "+ai.ToString("0")+" ms in all");
  }catch(Exception e){sb.AppendLine("FAILED: "+e);}
  File.WriteAllText(outPath,sb.ToString());EditorApplication.Exit(0);
 }
}
}
