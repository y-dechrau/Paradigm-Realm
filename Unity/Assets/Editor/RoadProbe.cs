using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
namespace ParadigmRealm {
// Development aid (owner request, 27 Nov 2026: "other civilisations should build roads as part of their development"): plays a new
// Small game headless and reports, every 25 turns, each home rival's cities, road tiles, and how many of its cities are joined to its
// capital by road. Unity -batchmode -executeMethod ParadigmRealm.RoadProbe.Run -probeOut <file> [-probeTurns 150]
public static class RoadProbe {
 static string Arg(string[] a,string k,string d){int i=Array.IndexOf(a,k);return i>=0&&i+1<a.Length?a[i+1]:d;}
 static string TryOne(World w,int o){var u=w.units.FirstOrDefault(x=>x.owner==o&&x.type==1&&x.job==0);if(u==null)return "none";u.moves=4;var near=w.CityRoadNeeds(o).OrderBy(i=>w.Distance(w.tiles[i].x,w.tiles[i].y,u.x,u.y)).Select(i=>w.tiles[i]).FirstOrDefault();string r=near==null?"":" nearest planned "+near.x+","+near.y+" reason '"+w.JobReason(u,1)+"' start "+(u.x==near.x&&u.y==near.y?w.StartJob(u,1).ToString():"away")+" job "+u.job+" team "+w.WorkerTeam(u).Count+" autoWork "+u.autoWork+" roadOrder "+u.roadOrder+" flee "+u.flee;return "worker @"+u.x+","+u.y+r;}
 static string diag(World w,int o){var own=w.cities.Where(c=>c.owner==o).ToList();var cap=own.OrderBy(c=>c.id).First();return string.Join("",own.Where(c=>c!=cap).Select(c=>{var r=w.RoadCourse(o,cap.x,cap.y,c.x,c.y);return " ["+c.name+" d"+w.Distance(c.x,c.y,cap.x,cap.y)+" route "+(r==null?-1:r.Count)+" off-land "+(r==null?0:r.Count(t=>t.owner!=o))+"]";}).ToArray());}
 public static void Run(){
  var args=Environment.GetCommandLineArgs();var sb=new StringBuilder();int turns=int.Parse(Arg(args,"-probeTurns","150"));
  try{
   var w=World.New(int.Parse(Arg(args,"-probeSeed","4242")),"Roads",1,1,1,0,1,2,4,hex:true,natives:false,rations:true,wild:true);w.EnableMaterials();w.EnableStatecraft();w.nationalForces=true;w.lifeways=true;
   for(int t=1;t<=turns;t++){w.EndTurn();if(w.result!=""){sb.AppendLine("game over at turn "+w.turn+": "+w.result);break;}if(t%25==0){sb.AppendLine("turn "+w.turn);
    for(int o=1;o<w.civCount;o++){var own=w.cities.Where(c=>c.owner==o).ToList();if(own.Count==0)continue;int roads=w.tiles.Count(x=>x.road&&x.owner==o&&!own.Any(c=>c.x==x.x&&c.y==x.y));int joined=own.Count(c=>w.Connected(c));
     sb.AppendLine("  "+w.NationName(o)+": "+own.Count+" cities, "+roads+" road tiles, "+joined+" joined by road, workers "+w.units.Count(u=>u.owner==o&&u.type==1)+", net gold "+w.Budget(o)[2]+", wheel "+w.HasTech(o,"wheel")+", planned "+w.CityRoadNeeds(o).Count+" why: "+string.Join("; ",w.CityRoadNeeds(o).Select(i=>w.JobReason(new Unit{owner=o,type=1,x=w.tiles[i].x,y=w.tiles[i].y,moves=2},1)).GroupBy(s=>s).Select(g=>(g.Key==""?"ok":g.Key)+" x"+g.Count()).ToArray())+" | try: "+TryOne(w,o)+" | worker jobs: "+string.Join(",",w.units.Where(u=>u.owner==o&&u.type==1).Select(u=>w.Activity(u)+(u.unfed?" UNFED":"")+" @"+u.x+","+u.y+" mv"+u.moves).ToArray()));}}}
  }catch(Exception e){sb.AppendLine("FAILED: "+e);}
  File.WriteAllText(Arg(args,"-probeOut","roads.txt"),sb.ToString());EditorApplication.Exit(0);
 }
}
}
