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
  Bargains(w,open,side,clear);
  // Camps appear, only on land nobody owns, and send raiders.
  {clear();int raidersSeen=0;for(int k=0;k<200;k++){w.turn++;w.BarbariansTurnForChecks();raidersSeen=Math.Max(raidersSeen,w.raiders.Count);}
   Check(w.camps.Count>0&&w.camps.Count<=w.CampLimit(),"camps appear up to the limit ("+w.camps.Count+" of "+w.CampLimit()+")");Check(raidersSeen>0,"camps send out raiders");
   Check(w.camps.All(c=>{var t=w.At(c.x,c.y);return t.realm==0&&!PixelArt.IsWater(t.terrain);}),"camps stand on home-world land");}
  // A whole game with barbarians stays valid and saves.
  {var g=World.New(173,"Whole",0,1,1,0,0,2,4,hex:true,natives:false,wild:true);g.barbarians=true;g.Found(g.units.First(u=>u.owner==0&&u.type==0));int most=0;
   for(int t=0;t<120&&g.result=="";t++){foreach(int o in Enumerable.Range(1,g.civCount-1))g.Relation(0,o).peaceUntil=g.turn+10;g.EndTurn();most=Math.Max(most,g.camps.Count);}
   Check(g.Valid(),"a 120-turn game stays valid (most camps "+most+")");Check(most>0,"camps appear in a whole game");
   var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(g));Check(copy.barbarians&&copy.camps.Count==g.camps.Count&&copy.raiders.Count==g.raiders.Count,"barbarians are saved");
   if(copy.camps.Count>0){copy.camps[0].deals.Add(new BarbarianDeal{payer=0,target=1,until=copy.turn+9});var again=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(copy));Check(again.camps[0].deals.Count>0&&again.Contract(again.camps[0]).target==1&&again.Contract(again.camps[0]).payer==0,"bargains are saved");}}
 }
 // Bargains (24 Nov 2026): pay a camp off, or hire it against another civilisation like a privateer.
 static void Bargains(World w,Tile open,Tile side,Action clear){
  Func<BarbarianCamp> camp=()=>{clear();var c=new BarbarianCamp{id=9201,x=open.x,y=open.y,seen=true,nextRaid=9999};w.camps.Add(c);return c;};
  Action<int> purse=v=>{w.gold=v;w.nations[0].gold=v;};
  // Paid off: the price is taken, and a raider beside your worker lets it be until the bargain ends.
  {var c=camp();purse(0);Check(!w.PayOff(0,c)&&w.PayOffReason(0,c).Contains("gold"),"no gold, no bargain");int price=w.PayOffPrice(0,c);purse(price+7);Check(w.PayOff(0,c)&&w.gold==7&&w.nations[0].gold==7,"the price is paid");
   Check(w.PayOffPrice(0,c)>price&&w.PayOffReason(0,c)!="","the next payment is dearer, and they are not paid twice at once");
   var r=new Raider{id=9202,x=side.x,y=side.y,camp=c.id};w.raiders.Add(r);var near=w.Step(side.x,side.y,World.HexDi[0],World.HexDj[0]);var worker=w.Add(0,1,near.x,near.y);w.BarbariansTurnForChecks();
   Check(worker.hp==10&&w.units.Contains(worker)&&!w.BarbarianNear(0,worker.x,worker.y,3),"a raider whose camp is paid leaves your worker alone");
   w.turn+=BarbarianTerms.TruceTurns+1;r.x=side.x;r.y=side.y;w.BarbariansTurnForChecks();Check(!w.CampSpares(c,0)&&(worker.hp<10||!w.units.Contains(worker)),"and strikes again once the gold is spent");}
  // Attacking them breaks the bargain.
  {var c=camp();purse(9999);w.PayOff(0,c);var soldier=w.Add(0,6,side.x,side.y);soldier.moves=4;w.Move(soldier,c.x,c.y);Check(!w.CampSpares(c,0),"attacking a camp you paid ends the bargain");}
  // Hired: raiders march on the people named, spare you, and nobody is blamed for it.
  {var c=camp();City target=null;foreach(var t in w.tiles.Where(t=>t.realm==0&&w.Distance(t.x,t.y,open.x,open.y)>=5&&w.Distance(t.x,t.y,open.x,open.y)<=14&&w.cities.All(x=>w.Distance(x.x,x.y,t.x,t.y)>=5)).OrderBy(t=>w.Distance(t.x,t.y,open.x,open.y))){if(PixelArt.IsWater(t.terrain)||World.IsImpassable(t.terrain)||World.Barren(t.terrain))continue;var settler=w.Add(1,0,t.x,t.y);settler.moves=4;target=w.Found(settler);if(target!=null){w.met[1]=true;if(w.HireReason(0,c,1).Contains("reach")){w.cities.Remove(target);target=null;}}w.units.Remove(settler);if(target!=null)break;}
   Check(target!=null,"a rival city stands near the camp");w.met[1]=false;purse(9999);Check(w.HireReason(0,c,1)!="","they cannot be sent against people you have not met");w.met[1]=true;
   int opinion=w.Opinion(1,0),price=w.HirePrice(c,1);bool war=w.AtWar(0,1);purse(price+50);Check(w.Hire(0,c,1)&&w.gold==50,"hiring takes the price ("+price+")");Check(w.HireReason(0,c,2)!=""&&w.CampSpares(c,0)&&!w.CampSpares(c,1),"one employer at a time, and the employer is spared");
   w.nations[1].gold=500;var r=new Raider{id=9203,x=side.x,y=side.y,camp=c.id};w.raiders.Add(r);int far=w.Distance(r.x,r.y,target.x,target.y),least=far;int treasury=w.gold;
   for(int k=0;k<BarbarianTerms.HireTurns-1&&w.gold==treasury;k++){w.turn++;r.hp=10;w.BarbariansTurnForChecks();if(w.raiders.Contains(r))least=Math.Min(least,w.Distance(r.x,r.y,target.x,target.y));}
   Check(least<far&&least<=2,"hired raiders march on their target ("+far+" tiles to "+least+")");Check(w.gold>treasury,"and half of what they plunder is yours ("+(w.gold-treasury)+" gold)");
   Check(w.Opinion(1,0)==opinion&&w.AtWar(0,1)==war,"nobody learns who sent them");
   int full=w.PayOffPrice(1,c);w.nations[1].gold=full+10;Check(w.PayOff(1,c)&&w.Contract(c)==null&&w.CampSpares(c,1),"the victim may buy them off at double the price");
   w.cities.Remove(target);w.RebuildTerritory();}
  // A line in the Chronicle never names a civilisation the player has not met.
  {w.met[2]=false;w.Note(w.NationName(2)+" did something far away.");Check(!w.log[0].Contains(w.NationName(2))&&w.log[0].Contains("unknown people"),"unmet civilisations are not named ("+w.log[0]+")");w.met[2]=true;w.Note(w.NationName(2)+" did something.");Check(w.log[0].Contains(w.NationName(2)),"met ones are");}
  clear();w.turn=30;
 }
}
}
