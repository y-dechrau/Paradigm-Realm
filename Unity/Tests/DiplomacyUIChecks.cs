using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ParadigmRealm;
class DiplomacyUIChecks {
 static BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 static void Set(RealmGame g,string name,object value){typeof(RealmGame).GetField(name,flags).SetValue(g,value);}
 static T Get<T>(RealmGame g,string name){return (T)typeof(RealmGame).GetField(name,flags).GetValue(g);}
 static void Call(RealmGame g,string name){try{typeof(RealmGame).GetMethod(name,flags).Invoke(g,null);}catch(TargetInvocationException e)when(e.InnerException is ExitGUIException){}}
 static void Click(RealmGame g,string method,string text){GUI.enabled=true;GUI.ClickText=text;Call(g,method);GUI.ClickText="";}
 static void Main(){
  var w=World.New(63);w.met[1]=w.met[2]=true;w.Relation(1,2).contact=true;w.gold=500;w.nations[1].gold=500;w.nations[2].gold=500;w.Found(w.units.First(u=>u.owner==0&&u.type==0));
  var game=new RealmGame();Set(game,"world",w);Call(game,"Styles");
  GUI.Labels.Clear();Call(game,"DrawAdvancedDiplomacy");Check(GUI.Labels.Any(s=>s.Contains("Acceptance margin"))&&GUI.Labels.Any(s=>s.Contains("Grievances")),"composer shows costs, acceptance and relationship reasons");
  Click(game,"DrawAdvancedDiplomacy","Review proposal");Check(!w.HasAccess(0,1),"review never commits offer");Click(game,"DrawAdvancedDiplomacy","Confirm diplomatic action");Check(w.HasAccess(0,1),"confirmed composer negotiates borders");
  var offer=w.Propose(new DiplomaticOffer{from=2,to=0,kind=TreatyKind.Trade,givePerTurn=2,askPerTurn=1});Click(game,"DrawAdvancedDiplomacy","Review acceptance #"+offer.id);Check(offer.status==0,"inbox review shows terms before acceptance");Click(game,"DrawAdvancedDiplomacy","Confirm diplomatic action");Check(offer.status==1&&w.contracts.Count==1,"inbox accepts contract");
  Call(game,"HudStyles");GUI.ClickText="(no click)";GUI.Labels.Clear();Call(game,"DrawRail");Check(GUI.Labels.Contains("Diplomacy"),"rail shows Diplomacy");var inbound=w.Propose(new DiplomaticOffer{from=1,to=0,kind=TreatyKind.BorderPromise});GUI.Labels.Clear();Call(game,"DrawRail");GUI.ClickText="";Check(GUI.Labels.Contains(w.offers.Count(o=>o.to==0&&o.status==0&&o.expires>w.turn).ToString()),"rail badge shows the pending inbox count");
  var json=JsonUtility.ToJson(w);var loaded=JsonUtility.FromJson<World>(json);loaded.UpgradeSave();Check(loaded.Valid()&&JsonUtility.ToJson(loaded)==json,"v7 round trip preserves contracts, pending offers, pair events and IDs");Check(loaded.RespondToOffer(inbound.id,true),"loaded inbox remains actionable");
  Set(game,"pendingUnit",-2);Set(game,"pendingWars",new[]{1,2});w.Relation(0,2).peaceUntil=w.turn+8;Click(game,"DrawWarPrompt","Yes — declare war");Check(!w.AtWar(0,1)&&!w.AtWar(0,2)&&Get<int>(game,"pendingUnit")==-2,"multi-target confirmation preflights all treaties before any war");
  w.Relation(0,2).peaceUntil=0;w.Relation(0,1).friendUntil=w.turn+5;var u=w.units.First(x=>x.owner==0&&x.type==2);Set(game,"pendingUnit",u.id);Set(game,"pendingWars",new[]{1});int x=u.x,y=u.y;Click(game,"DrawWarPrompt","Yes — declare war and attack");Check(!w.AtWar(0,1)&&u.x==x&&u.y==y,"map attacks cannot bypass friendship");
  w.Relation(0,1).friendUntil=0;Set(game,"pendingUnit",-2);Click(game,"DrawWarPrompt","Yes — declare war");Check(w.AtWar(0,1),"unprotected confirmed war works");
  World a=World.New(900),b=World.New(900);foreach(var twin in new[]{a,b}){twin.Found(twin.units.First(v=>v.owner==0&&v.type==0));twin.met[1]=twin.met[2]=true;twin.Relation(1,2).contact=true;}
  for(int i=0;i<20;i++){a.EndTurn();b.EndTurn();foreach(var twin in new[]{a,b})foreach(var o in twin.offers.Where(o=>o.status==0&&o.to==0).ToArray())twin.RespondToOffer(o.id,true);Check(a.Valid()&&b.Valid(),"diplomatic campaign valid");}
  Check(JsonUtility.ToJson(a)==JsonUtility.ToJson(b),"AI diplomacy is deterministic across same-seed campaigns");
  Console.WriteLine("PASS diplomacy composer/inbox confirmation, header notice, v7 full-state round trip, treaty-protected multi-target map attacks and 20-turn deterministic diplomacy (API doubles)");
 }
}
