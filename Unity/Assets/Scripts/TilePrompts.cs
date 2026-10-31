using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Owner requests (31 Oct 2026): founding a city asks "Are you sure you want to settle here?", then for the city's name; and a
// click on your city's tile while your units stand on it asks whether you mean the city or one of the units.
public partial class RealmGame {
 int foundStage,foundUnit=-1;string foundName="",foundProblem="";
 bool tilePicker;int pickerX,pickerY;Vector2 pickerAt;
 bool PromptOpen(){return foundStage>0||tilePicker;}
 void Found(){
  var u=Selected();string reason=world.FoundReason(u);
  if(u==null||reason!=""){status=reason==""?"Select a Settler first.":reason;return;}
  foundUnit=u.id;foundStage=1;foundProblem="";ResetMapGesture();
 }
 void DrawFoundPrompt(){
  var u=world.units.Find(a=>a.id==foundUnit);if(u==null||world.FoundReason(u)!=""){foundStage=0;return;}
  GUI.enabled=true;Fill(new Rect(0,0,1280,800),new Color(0,0,0,.45f));var box=new Rect(420,280,440,foundStage==1?190:220);GUI.Box(box,"");
  GUILayout.BeginArea(new Rect(box.x+24,box.y+18,box.width-48,box.height-30));
  if(foundStage==1){
   GUILayout.Label("Are you sure you want to settle here?",heading);
   var t=world.At(u.x,u.y);GUILayout.Label(World.TerrainNames[t.terrain]+" at "+u.x+", "+u.y+". The Settler becomes the new city.",body);GUILayout.Space(10);
   GUILayout.BeginHorizontal();if(Button("Yes, settle here")){foundStage=2;foundName=world.NewCityName(0);foundProblem="";GUI.FocusControl(null);}if(Button("No")){foundStage=0;status="Settling cancelled.";}GUILayout.EndHorizontal();
  }else{
   GUILayout.Label("Name your city",heading);
   GUI.SetNextControlName("cityfound");foundName=GUILayout.TextField(foundName,32);GUI.FocusControl("cityfound");
   GUILayout.Label(foundProblem,small);GUILayout.Space(6);
   var e=Event.current;bool enter=e.type==EventType.KeyDown&&(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.KeypadEnter);
   GUILayout.BeginHorizontal();
   if(Button("Found city")||enter){foundProblem=world.CityNameProblem(foundName);if(foundProblem==""){var c=world.Found(u,foundName);foundStage=0;if(c!=null){selected=-1;cityId=c.id;status=c.name+" founded.";}else status=world.FoundReason(u);if(enter)e.Use();}}
   if(Button("Back"))foundStage=1;
   GUILayout.EndHorizontal();
  }
  GUILayout.EndArea();
 }
 // Arrow keys: on staggered maps up and down are straight north and south (two rows), left and right the next column;
 // on diagonal maps they keep their grid steps.
 Tile ArrowTarget(Unit u,int dx,int dy){if(!world.Staggered)return world.At(u.x+dx,u.y+dy);return dy!=0?world.At(u.x,u.y+2*dy):world.At(u.x+dx,u.y);}
 // City or unit: one button for the city and one per unit on the tile, beside the pointer.
 void OpenTilePicker(int x,int y){tilePicker=true;pickerX=x;pickerY=y;pickerAt=Event.current.mousePosition;}
 void DrawTilePicker(){
  var c=world.cities.Find(a=>a.owner==0&&a.x==pickerX&&a.y==pickerY);var own=world.units.Where(u=>u.owner==0&&u.x==pickerX&&u.y==pickerY).ToList();
  if(c==null&&own.Count==0){tilePicker=false;return;}
  GUI.enabled=true;int rows=(c!=null?1:0)+own.Count;var box=new Rect(Mathf.Min(pickerAt.x+12,1280-270),Mathf.Min(pickerAt.y+12,800-(rows*34+44)),258,rows*34+44);
  var e=Event.current;if(e.type==EventType.MouseDown&&!box.Contains(e.mousePosition)){tilePicker=false;e.Use();return;}
  GUI.Box(box,"");GUI.Label(new Rect(box.x+12,box.y+6,box.width-24,22),"Select",heading);float y=box.y+34;
  if(c!=null){if(GUI.Button(new Rect(box.x+10,y,box.width-20,30),"City: "+c.name,button)){cityId=c.id;selected=-1;tilePicker=false;}y+=34;}
  foreach(var u in own){if(GUI.Button(new Rect(box.x+10,y,box.width-20,30),World.UnitNames[u.type]+" — "+world.Activity(u),button)){selected=u.id;cityId=-1;tilePicker=false;}y+=34;}
 }
}
}
namespace ParadigmRealm {
// Classic view geometry. Diagonal-layout maps keep square tiles; staggered maps (shape 2) draw each row as half-height
// tiles with odd rows half a tile to the east, so the classic view shows the same geography as the pixel-art view.
public partial class RealmGame {
 UnityEngine.Rect ClassicRect(int x,int y){return world.Staggered?new UnityEngine.Rect((x+(y&1)*.5f)*tilePixels,y*tilePixels*.5f,tilePixels,tilePixels*.5f):new UnityEngine.Rect(x*tilePixels,y*tilePixels,tilePixels,tilePixels);}
 float ClassicHeight(){return world.Staggered?world.Height*tilePixels*.5f+tilePixels*.5f:world.Height*tilePixels;}
 float ClassicRowHeight(){return world.Staggered?tilePixels*.5f:tilePixels;}
 bool ClassicPick(float px,float py,out int x,out int y){
  if(!world.Staggered){x=UnityEngine.Mathf.FloorToInt(px/tilePixels);y=UnityEngine.Mathf.FloorToInt(py/tilePixels);}
  else{y=UnityEngine.Mathf.FloorToInt(py/(tilePixels*.5f));x=UnityEngine.Mathf.FloorToInt(px/tilePixels-(y&1)*.5f);}
  var t=world.At(x,y);if(t==null)return false;x=t.x;y=t.y;return true;}
}
}
namespace ParadigmRealm {
public partial class RealmGame {
 void OrderRoad(Unit u,int x,int y){status=world.RoadRoute(u,x,y);}
 // R: the first press arms road mode (then right-drag to a destination builds a road all the way); a second press, or R
 // with no drag, builds a road on the worker's own tile.
 void RoadKey(Unit u){
  if(u==null||u.type!=1){status="Select a worker to build roads.";return;}
  if(!roadMode){roadMode=true;status="Road mode: right-drag to a destination to build a road there, or press R again to build here.";return;}
  roadMode=false;string reason=world.WorkerTeam(u).Select(m=>world.JobReason(m,1)).FirstOrDefault(s=>s!="")??"";status=world.StartJob(u,1)?"Building a road here.":reason;
 }
}
}
