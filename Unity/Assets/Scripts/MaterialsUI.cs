using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The timber and stone ledger (owner brief, 26 Nov 2026), opened by clicking either figure in the top bar: last turn's opening
// stock, what was harvested and by whom, what was installed in which project, what waited, and the closing stock; then every
// project that draws materials in the order it is supplied, with next turn's draw, when it finishes and an Urgent switch.
// "Produced" is the harvest; "added to storage" is the harvest less what the projects used.
public partial class RealmGame {
 // The forecasts behind the city screen's lines and build list are worked out at most twice a second per item.
 readonly System.Collections.Generic.Dictionary<string,System.Collections.Generic.KeyValuePair<float,string>> materialMemo=new System.Collections.Generic.Dictionary<string,System.Collections.Generic.KeyValuePair<float,string>>();
 string MaterialMemo(string key,Func<string> make){System.Collections.Generic.KeyValuePair<float,string> v;if(materialMemo.TryGetValue(key,out v)&&Time.realtimeSinceStartup-v.Key<.5f)return v.Value;string s=make();materialMemo[key]=new System.Collections.Generic.KeyValuePair<float,string>(Time.realtimeSinceStartup,s);return s;}
 string MaterialPreviewMemo(City c,int p){return MaterialMemo("p"+c.id+":"+p+":"+world.turn,()=>world.MaterialPreview(c,p));}
 string MaterialLineMemo(City c,bool building){return MaterialMemo("l"+c.id+":"+building+":"+world.turn,()=>world.MaterialLine(c,building));}
 // Short form for the line card: installed so far, or what it waits for.
 string MaterialBrief(City c,bool building){if(!world.buildingMaterials)return "";return MaterialMemo("s"+c.id+":"+building+":"+world.turn,()=>world.MaterialBrief(c,building));}
 bool materialsPanel;float materialsAnchor=400;Vector2 materialsScroll;int hudTimberNet,hudStoneNet;
 Rect MaterialsPanelRect{get{float w=600,h=470;return new Rect(Mathf.Clamp(materialsAnchor-40,8,MapW-w-8),42,w,h);}}
 void DrawMaterialsPanel(){
  if(!materialsPanel||!world.buildingMaterials)return;var r=MaterialsPanelRect;GUI.Box(r,"");
  var head=new GUIStyle(hudTitle){fontSize=15,alignment=TextAnchor.MiddleLeft};var text=new GUIStyle(hudLabel){fontSize=12,wordWrap=true,alignment=TextAnchor.UpperLeft,clipping=TextClipping.Overflow};
  var gold=new GUIStyle(text){normal={textColor=UiGold}};
  GUI.Label(new Rect(r.x+16,r.y+10,r.width-80,24),"Timber and stone",head);
  if(GUI.Button(new Rect(r.xMax-40,r.y+10,26,24),"x",button))materialsPanel=false;
  var n=world.nations[0];var led=world.ledger;
  var list=new System.Collections.Generic.List<string>();
  if(led!=null&&led.turn>=0){
   list.Add("#LAST TURN");
   list.Add("Timber: "+led.openTimber+" in store, "+led.madeTimber+" produced, "+(led.openTimber+led.madeTimber-led.closeTimber)+" used, "+(led.closeTimber-led.openTimber>=0?"+":"")+(led.closeTimber-led.openTimber)+" added to storage = "+led.closeTimber+".");
   list.Add("Stone: "+led.openStone+" in store, "+led.madeStone+" produced, "+(led.openStone+led.madeStone-led.closeStone)+" used, "+(led.closeStone-led.openStone>=0?"+":"")+(led.closeStone-led.openStone)+" added to storage = "+led.closeStone+".");
   list.Add("#Produced by");foreach(var s in led.made)list.Add("  "+s);
   list.Add("#Used by");if(led.used.Count==0)list.Add("  Nothing was installed.");foreach(var s in led.used)list.Add("  "+s);
   if(led.waiting.Count>0){list.Add("#Waited");foreach(var s in led.waiting)list.Add("  "+s);}}
  else list.Add("No turn has ended since timber and stone began to be drawn as work goes. This turn's harvest is added before any work, and can be used at once.");
  float x=r.x+16,w=r.width-32,y=r.y+40;
  // Upper part: the ledger.
  var top=new Rect(x,y,w,200);float th=0;foreach(var s in list)th+=(s.StartsWith("#")?20:text.CalcHeight(new GUIContent(s),w-20))+2;
  materialsScroll=GUI.BeginScrollView(top,materialsScroll,new Rect(0,0,w-18,Math.Max(top.height,th)));float yy=0;
  foreach(var s in list){if(s.StartsWith("#")){GUI.Label(new Rect(0,yy,w-20,18),s.Substring(1).ToUpperInvariant(),gold);yy+=20;}else{float h=text.CalcHeight(new GUIContent(s),w-20);GUI.Label(new Rect(0,yy,w-20,h),s,text);yy+=h+2;}}
  GUI.EndScrollView();y=top.yMax+8;Fill(new Rect(x,y,w,1),new Color(UiGold.r,UiGold.g,UiGold.b,.5f));y+=6;
  // Lower part: projects in supply order, with next turn's draw.
  GUI.Label(new Rect(x,y,w,18),"WORK THAT DRAWS MATERIALS, IN THE ORDER IT IS SUPPLIED  (stock "+n.timber+" timber, "+n.stone+" stone)",gold);y+=22;
  var jobs=world.MaterialJobs(0);
  if(jobs.Count==0){GUI.Label(new Rect(x,y,w,40),"Nothing under way needs timber or stone. Buildings, boats, galleys, catapults, bridges on rivers and forts do.",text);return;}
  var fc=world.MaterialForecast(0);var inc=world.MaterialIncome(0);int t=n.timber+inc[0],s2=n.stone+inc[1];var next=world.MaterialJobs(0);World.AllocateForUI(next,ref t,ref s2);
  foreach(var j in World.SupplyOrderForUI(jobs).Take(8)){
   var nj=next.First(q=>q.key==j.key);var f=fc.FirstOrDefault(q=>q.key==j.key);int dT=nj.instT-j.instT,dS=nj.instS-j.instS;
   string line=j.name+" · "+World.MatText(j.instT,j.instS)+" of "+World.MatText(j.totalT,j.totalS)+" in · next turn "+(dT+dS>0?"−"+World.MatText(dT,dS):"nothing")+(nj.shortT+nj.shortS>0?" (waits for "+World.MatText(nj.shortT,nj.shortS)+")":"")+(f!=null&&f.finish>0?" · done in "+f.finish:"");
   float h=Mathf.Max(22,text.CalcHeight(new GUIContent(line),w-104));GUI.Label(new Rect(x,y+2,w-104,h),line,text);
   bool urgent=world.IsUrgent(0,j.key);if(GUI.Button(new Rect(x+w-96,y,96,22),urgent?"Urgent ✓":"Make urgent",button))world.SetUrgent(0,j.key,!urgent);
   y+=h+4;if(y>r.yMax-30)break;}
  GUI.Label(new Rect(x,r.yMax-24,w,18),"Urgent work is supplied first; then whatever was started earliest.",new GUIStyle(text){fontStyle=FontStyle.Italic});
 }
}
public partial class World {
 public static void AllocateForUI(System.Collections.Generic.List<MatJob> jobs,ref int timber,ref int stone){Allocate(jobs,ref timber,ref stone);}
 public static System.Collections.Generic.IEnumerable<MatJob> SupplyOrderForUI(System.Collections.Generic.IEnumerable<MatJob> jobs){return SupplyOrder(jobs);}
 // Next turn's change in each stock: harvest less what the projects will draw. {timber, stone}.
 public int[] MaterialNetNext(int owner){var inc=MaterialIncome(owner);int t=nations[owner].timber+inc[0],s=nations[owner].stone+inc[1];Allocate(MaterialJobs(owner),ref t,ref s);return new[]{t-nations[owner].timber,s-nations[owner].stone};}
}
}
