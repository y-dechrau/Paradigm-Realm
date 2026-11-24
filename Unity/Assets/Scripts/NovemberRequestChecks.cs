using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Owner requests of 25 Nov 2026: gateways heard of but not marked, sites that share no land, rival Scouts.
public static class NovemberRequestChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("25 Nov requests: "+what);}
 public static void Run(){
  // The compass is the map's own: up is north, right is east.
  {var w=World.New(431,"Way",0,1,1,0,1,1,3,hex:true,natives:false);int x=w.Width/2,y=w.MainHeight/2;y-=y&1;
   Check(World.CompassNames[w.Compass(x,y,x,y-8)]=="north"&&World.CompassNames[w.Compass(x,y,x,y+8)]=="south"&&World.CompassNames[w.Compass(x,y,x+6,y)]=="east"&&World.CompassNames[w.Compass(x,y,x-6,y)]=="west","north, south, east and west");
   Check(World.CompassNames[w.Compass(x,y,x+4,y-8)]=="north-east"&&World.CompassNames[w.Compass(x,y,x-4,y+8)]=="south-west","and the quarters between");}
  // Hearing of a gateway marks nothing; seeing it is what finds it, once, so its screen shows.
  {var w=World.New(432,"Gates",0,1,1,0,1,2,4,hex:true);w.EnableDimensions();var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));foreach(var t in w.tiles)if(t.gateLink>0)t.seen=false;w.gatesFound.Clear();
   var gate=w.HomeGates().OrderByDescending(g=>w.Distance(g.x,g.y,city.x,city.y)).First();int cell=gate.y*w.Width+gate.x;
   Check(w.HearOfGateForChecks(gate,"Sahel Empire")&&!w.HearOfGateForChecks(gate,"Sahel Empire"),"word of a gateway is news once");
   Check(!gate.seen&&!w.gatesFound.Contains(cell)&&w.OpenHints().Count==1&&w.NextNewGate()==null,"and marks nothing on the map");
   string text=w.HintText(w.OpenHints()[0]);Check(text.Contains(World.CompassNames[w.Compass(city.x,city.y,gate.x,gate.y)])&&text.Contains(city.name)&&!text.Contains(gate.x+", "+gate.y)&&w.log[0].Contains("gateway"),"it says only which way ("+text+")");
   gate.seen=true;Check(w.NextNewGate()==gate&&w.NextNewGate()==null&&w.OpenHints().Count==0,"seeing it discovers it, once");
   // Through the turn's own rules: nothing is marked by legend or rumour.
   foreach(var g in w.HomeGates())g.seen=false;w.gatesFound.Clear();w.gateHints.Clear();w.turn=DimensionSettings.LegendTurn;while(w.turn%10!=0)w.turn++;w.LegendsForChecks(0);Check(w.gatesFound.Count==0&&w.HomeGates().All(g=>!g.seen),"a legend marks no gateway");
   // A save in which a rumour had marked one.
   w.gateHints.Clear();foreach(var t in w.tiles)if(w.Distance(t.x,t.y,gate.x,gate.y)<=2&&t.realm==0)t.seen=false;w.units.RemoveAll(u=>u.owner==0&&w.Distance(u.x,u.y,gate.x,gate.y)<=3);gate.seen=true;w.gatesFound.Add(cell);w.ForgetRumouredGates();Check(!gate.seen&&!w.gatesFound.Contains(cell)&&w.OpenHints().Count==1,"an older save's rumoured gateway becomes a hint");
   var near=w.Step(gate.x,gate.y,1,0);gate.seen=true;near.seen=true;w.gatesFound.Add(cell);w.ForgetRumouredGates();Check(gate.seen&&w.gatesFound.Contains(cell),"one really explored stays found");
   var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));Check(copy.gateHints.Count==w.gateHints.Count,"hints are saved");}
  // Suggested sites: the best one shares less land than a near twin, and says what it shares.
  {var w=World.New(433,"Sites",0,1,1,0,1,1,3,hex:true,natives:false,rations:true);var settler=w.units.First(u=>u.owner==0&&u.type==0);var city=w.Found(settler);foreach(var t in w.tiles){t.seen=true;t.rememberedTerrain=t.terrain;}for(int k=0;k<w.nations[0].known.Length;k++)w.nations[0].known[k]=true;
   var at=w.tiles.First(t=>t.realm==0&&w.Distance(t.x,t.y,city.x,city.y)==3&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));var next=w.Add(0,0,at.x,at.y);next.moves=4;var sites=w.SettlementOptions(next,5);Check(sites.Count>0,"sites are found");
   foreach(var r in sites){int least=w.Distance(r.x,r.y,city.x,city.y);Check(r.shared>0==(least<=World.SharedReach*2)||r.shared==0,"land is shared only within reach");if(r.shared>0)Check(r.sharedWith==city.name&&w.SiteHighlights(r).Contains("shares"),"and it is said with whom");}
   var flat=World.New(434,"Flat",0,1,1,0,1,1,3,hex:true,natives:false,rations:true);Check(flat.SharedForChecks(5,5)==0,"nothing is shared before the first city");
   var reports=new List<SettlementReport>();foreach(var t in w.tiles.Where(t=>t.realm==0&&w.Distance(t.x,t.y,city.x,city.y)>=4&&w.Distance(t.x,t.y,city.x,city.y)<=8)){var r=w.AssessSettlement(0,t.x,t.y,at.x,at.y);if(r.Suitable)reports.Add(r);}
   var close=reports.Where(r=>w.Distance(r.x,r.y,city.x,city.y)==4).ToList();var clear=reports.Where(r=>w.Distance(r.x,r.y,city.x,city.y)>=7).ToList();if(close.Count>0&&clear.Count>0)Check(close.Min(r=>r.shared)>0&&clear.All(r=>r.shared==0),"four tiles apart shares land, seven does not");}
  // Rival Scouts: trained when affordable, they uncover ground, and not when poor, hungry or at war.
  {var w=World.New(435,"Scouts",0,1,1,0,2,2,4,hex:true,natives:false,wild:true,rations:true);w.nationalForces=true;w.Found(w.units.First(u=>u.owner==0&&u.type==0));foreach(var u in w.units.Where(u=>u.owner==1&&u.type==0).ToList())w.Found(u);
   var c=w.cities.FirstOrDefault(a=>a.owner==1);Check(c!=null,"a rival has a city");w.nations[1].gold=200;bool rich=w.ScoutWanted(1);w.units.RemoveAll(u=>u.owner==1&&u.type==3);Check(w.ScoutWanted(1)||w.Budget(1)[2]<0,"a rival with gold wants a Scout");
   w.nations[1].gold=0;if(w.Budget(1)[2]<RivalScoutRules.Income)Check(!w.ScoutWanted(1),"a poor one does not");w.nations[1].gold=200;w.Add(1,3,c.x,c.y);w.Add(1,3,c.x,c.y);Check(!w.ScoutWanted(1),"nor one that has enough");
   var scout=w.units.First(u=>u.owner==1&&u.type==3);int known=w.nations[1].known.Count(k=>k);for(int t=0;t<12;t++){scout.moves=w.MaxMoves(scout);if(!w.units.Contains(scout))break;w.ScoutForChecks(scout);w.turn++;}
   Check(w.nations[1].known.Count(k=>k)>known+20,"a Scout uncovers ground ("+known+" to "+w.nations[1].known.Count(k=>k)+" tiles)");
   // A whole game.
   var g=World.New(436,"Whole",0,1,1,0,2,2,5,hex:true,natives:false,wild:true,rations:true);g.nationalForces=true;g.Found(g.units.First(u=>u.owner==0&&u.type==0));int most=0;for(int t=0;t<60&&g.result=="";t++){for(int o=1;o<g.civCount;o++)g.Relation(0,o).peaceUntil=g.turn+10;g.EndTurn();most=Math.Max(most,g.units.Count(u=>u.owner>0&&u.type==3));}
   Check(most>0&&most<=(g.civCount-1)*3,"rivals keep a few Scouts ("+most+" at most)");Check(g.Valid(),"and the game stays valid");}
 }
}
public partial class World {
 internal bool HearOfGateForChecks(Tile g,string from){return HearOfGate(g,from);}
 internal void LegendsForChecks(int owner){GateLegends(owner);GateRumours(owner);}
 internal int SharedForChecks(int x,int y){string with;return SharedLand(x,y,out with);}
 internal bool ScoutForChecks(Unit u){return ScoutAI(u);}
}
}
