using System;
using System.Linq;
namespace ParadigmRealm {
// Barbarians (owner request, 16 Nov 2026): off unless chosen, camps on unclaimed land, raiders that strike, pillage and plunder,
// soldiers that clear them for gold, and wild animals that hunt them as they hunt everyone.
public static class BarbarianChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Barbarians: "+what);}
 public static void Run(){
  var off=World.New(171,"Calm",0,1,1,0,1,2,4,hex:true,natives:false);Check(!off.barbarians&&off.camps.Count==0&&off.raiders.Count==0,"off unless chosen");off.turn=40;for(int k=0;k<30;k++){off.turn++;off.BarbariansTurnForChecks();}Check(off.camps.Count==0,"none appear when off");
  var w=World.New(172,"Raid",0,1,1,0,1,2,4,hex:true,natives:false);w.barbarians=true;
  var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));foreach(var a in w.units.ToList())w.units.Remove(a);
  Func<int,int,bool> land=(x,y)=>{var t=w.At(x,y);return t!=null&&t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&!World.Barren(t.terrain)&&t.gateLink==0;};
  var open=w.tiles.First(t=>land(t.x,t.y)&&t.owner<0&&w.Distance(t.x,t.y,city.x,city.y)>=12&&Enumerable.Range(0,6).All(k=>{var n=w.Step(t.x,t.y,World.HexDi[k],World.HexDj[k]);return n!=null&&land(n.x,n.y)&&n.owner<0;}));
  var side=w.Step(open.x,open.y,World.HexDi[0],World.HexDj[0]);var far=w.Step(side.x,side.y,World.HexDi[0],World.HexDj[0]);
  Action clear=()=>{w.camps.Clear();w.raiders.Clear();w.units.Clear();w.animals.Clear();};w.turn=30;
  // A raider strikes a unit beside it.
  {clear();var r=new Raider{id=9101,x=open.x,y=open.y};w.raiders.Add(r);var worker=w.Add(0,1,side.x,side.y);w.BarbariansTurnForChecks();Check(worker.hp<10||!w.units.Contains(worker),"a raider strikes a unit beside it");}
  // A civilian cannot attack; a soldier kills a raider by moving onto it.
  {clear();var r=new Raider{id=9102,x=side.x,y=side.y,hp=1};w.raiders.Add(r);var worker=w.Add(0,1,open.x,open.y);worker.moves=4;Check(!w.Move(worker,side.x,side.y),"a Worker cannot attack a raider");w.units.Remove(worker);
   var soldier=w.Add(0,6,open.x,open.y);soldier.moves=4;Check(w.Move(soldier,side.x,side.y)&&w.raiders.Count==0&&soldier.victories==1,"a soldier kills a raider by moving onto it");}
  // Burning a camp pays gold.
  {clear();var c=new BarbarianCamp{id=9103,x=side.x,y=side.y,hp=1};w.camps.Add(c);var soldier=w.Add(0,6,open.x,open.y);soldier.moves=4;int before=w.nations[0].gold,reward=w.CampReward();
   Check(w.Move(soldier,side.x,side.y)&&w.camps.Count==0&&w.nations[0].gold==before+reward,"burning a camp pays "+reward+" gold");}
  // A raider wrecks a farm it stands on.
  {clear();var t=w.At(open.x,open.y);t.owner=0;t.farm=true;var r=new Raider{id=9104,x=open.x,y=open.y};w.raiders.Add(r);w.BarbariansTurnForChecks();Check(!t.farm,"a raider pillages a farm");t.owner=-1;}
  // Routes go round raiders.
  {clear();var r=new Raider{id=9105,x=side.x,y=side.y};w.raiders.Add(r);var walker=w.Add(0,2,open.x,open.y);var path=w.Route(walker,far.x,far.y);Check(path.Count>0&&path.All(t=>t!=side),"routes go round raiders");}
  // Wild animals hunt raiders.
  {clear();w.wildlife=true;w.turn=Math.Max(30,World.HuntersFrom);var r=new Raider{id=9106,x=far.x,y=far.y};w.raiders.Add(r);w.animals.Add(new Animal{id=9107,kind=0,x=open.x,y=open.y});
   for(int k=0;k<4&&r.hp>=10&&w.raiders.Contains(r);k++)w.TickAnimalsForChecks(false);Check(r.hp<10||!w.raiders.Contains(r),"a hunter attacks a raider");w.wildlife=false;}
  // Camps appear, only on land nobody owns, and send raiders.
  {clear();int raidersSeen=0;for(int k=0;k<200;k++){w.turn++;w.BarbariansTurnForChecks();raidersSeen=Math.Max(raidersSeen,w.raiders.Count);}
   Check(w.camps.Count>0&&w.camps.Count<=w.CampLimit(),"camps appear up to the limit ("+w.camps.Count+" of "+w.CampLimit()+")");Check(raidersSeen>0,"camps send out raiders");
   Check(w.camps.All(c=>{var t=w.At(c.x,c.y);return t.realm==0&&!PixelArt.IsWater(t.terrain);}),"camps stand on home-world land");}
  // A whole game with barbarians stays valid and saves.
  {var g=World.New(173,"Whole",0,1,1,0,0,2,4,hex:true,natives:false,wild:true);g.barbarians=true;g.Found(g.units.First(u=>u.owner==0&&u.type==0));int most=0;
   for(int t=0;t<120&&g.result=="";t++){foreach(int o in Enumerable.Range(1,g.civCount-1))g.Relation(0,o).peaceUntil=g.turn+10;g.EndTurn();most=Math.Max(most,g.camps.Count);}
   Check(g.Valid(),"a 120-turn game stays valid (most camps "+most+")");Check(most>0,"camps appear in a whole game");
   var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(g));Check(copy.barbarians&&copy.camps.Count==g.camps.Count&&copy.raiders.Count==g.raiders.Count,"barbarians are saved");}
 }
}
}
