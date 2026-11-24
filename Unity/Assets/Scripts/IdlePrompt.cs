using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Idle units are shown one after another (owner request, 25 Nov 2026): when a turn begins, and whenever the unit on show has been
// given something to do (or has used its movement, been skipped, founded a city or died), the next idle unit is selected and the
// map moves to it. Nothing is taken out of the player's hands: pressing Esc, choosing a city or choosing another unit stops it until
// that unit is dealt with or the next turn begins, and it waits for every prompt, screen and moving unit. It can be turned off in
// Settings. A stack is shown once, by its first unit.
public partial class RealmGame {
 static bool? idlePrompt;static bool devRun=Environment.GetCommandLineArgs().Any(a=>a.EndsWith("Shot")||a.EndsWith("Probe")||a=="-spriteCompare"||a=="-resourceGallery");
 bool IdlePrompt{get{if(!idlePrompt.HasValue){try{idlePrompt=PlayerPrefs.GetInt("RealmIdlePrompt",1)!=0;}catch{idlePrompt=true;}}return idlePrompt.Value;}set{if(idlePrompt.HasValue&&idlePrompt.Value==value)return;idlePrompt=value;try{PlayerPrefs.SetInt("RealmIdlePrompt",value?1:0);}catch{}}}
 bool idleDue;int idleWatched=-1;float idleAt;World idleWorld;Vector2Int idleLast=new Vector2Int(-1,-1);
 bool NeedsOrders(Unit u){return u!=null&&u.owner==0&&u.moves>0&&u.hp>0&&!u.unfed&&!u.refitting&&world.UnitIdle(u);}
 // Your idle units with movement, a stack by its first unit only.
 List<Unit> IdleUnits(){
  var list=new List<Unit>();var stacks=new HashSet<int>();
  foreach(var u in world.units){if(!NeedsOrders(u))continue;if(u.stack!=0&&!stacks.Add(u.stack))continue;list.Add(u);}
  return list;}
 int idleCount,idleCountFrame=-1;
 int IdleCount(){if(idleCountFrame<0||Time.frameCount-idleCountFrame>=20||Time.frameCount<idleCountFrame){idleCountFrame=Time.frameCount;idleCount=IdleUnits().Count;}return idleCount;}
 void IdleTurnBegan(){idleDue=true;idleWatched=-1;idleAt=Time.realtimeSinceStartup+.35f;}
 // Called each frame from Update once no menu, screen or turn is in the way.
 void UpdateIdlePrompt(){
  if(idleWorld!=world){idleWorld=world;idleDue=false;idleWatched=-1;}
  if(!IdlePrompt||devRun||PromptOpen()||moveMode||roadMode||dragUnit!=-1||dragging)return;
  var u=Selected();
  if(u!=null&&NeedsOrders(u)){
   idleLast=new Vector2Int(u.x,u.y);idleWatched=u.id;
   if(idleDue&&Time.realtimeSinceStartup>=idleAt){idleDue=false;Center();}
   return;}
  if(idleWatched>=0){
   var was=world.units.Find(a=>a.id==idleWatched);
   // Let go or looking elsewhere, with that unit still waiting: the player's choice.
   if(was!=null&&NeedsOrders(was))idleWatched=-1;
   else if(u==null||u.id==idleWatched||!world.units.Contains(u)){idleWatched=-1;idleDue=true;idleAt=Time.realtimeSinceStartup+.45f;}
   else idleWatched=-1;}
  if(!idleDue||Time.realtimeSinceStartup<idleAt)return;
  foreach(var id in unitGlides.Keys)if(world.units.Any(a=>a.id==id&&a.owner==0))return;
  if(strikes.Count>0)return;
  idleDue=false;ShowIdle(false);
 }
 // The idle unit nearest the last one shown; false when there is none. From the Next unit button it steps on past the one on show.
 bool ShowIdle(bool step){
  var list=IdleUnits();if(list.Count==0){status="Every unit has its orders. End the turn when you are ready.";return false;}
  Unit next;int at=step?list.FindIndex(a=>a.id==selected):-1;
  if(at>=0)next=list[(at+1)%list.Count];
  else{var from=idleLast.x>=0?idleLast:new Vector2Int(list[0].x,list[0].y);int realm=world.RealmOf(from.x,from.y);
   next=list.OrderBy(a=>world.RealmOf(a.x,a.y)==realm?0:1).ThenBy(a=>world.Distance(a.x,a.y,from.x,from.y)).ThenBy(a=>a.id).First();}
  selected=next.id;cityId=-1;idleWatched=next.id;idleLast=new Vector2Int(next.x,next.y);Center();
  status=world.UnitName(next)+" is waiting for orders"+(list.Count>1?" ("+list.Count+" idle).":".");return true;}
}
}
