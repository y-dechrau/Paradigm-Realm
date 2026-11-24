using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Rival civilisations use Scouts when they can afford to (owner request, 25 Nov 2026). Until now a rival trained none and any it
// began with stood guard or wandered. A home rival at peace, with land left to find and gold coming in, trains one Scout (two from
// four cities) and sends it along the frontier exactly as the player's automatic exploration does, keeping clear of wild animals and
// enemies. Errands at the gateways still come first. A Scout that has found everything it can reach rests, and no more are trained
// for a while.
public static class RivalScoutRules{
 // Gold a turn the empire must still be making, or a treasury that can carry one; and what a second Scout asks.
 public const int Income=2,Treasury=60,SecondIncome=5,SecondCities=4,Rest=12,NoMore=40,Home=4;
}
public partial class World {
 [NonSerialized] Dictionary<int,int> scoutRest,scoutsDone;[NonSerialized] public int scoutsTrainedForProbe;
 bool ScoutingRival(int owner){return owner>0&&owner<civCount&&!IsNativeNation(owner)&&ActiveRival(owner);}
 // Whether this rival should train a Scout now, and can afford one.
 public bool ScoutWanted(int owner){
  if(!modernRules||!ScoutingRival(owner)||AtWarWithAnyone(owner)||Threatened(owner)||nations[owner].rationShort>0)return false;
  int until;if(scoutsDone!=null&&scoutsDone.TryGetValue(owner,out until)&&turn<until)return false;
  int net=Budget(owner)[2],own=CityTotal(owner),scouts=units.Count(u=>u.owner==owner&&u.type==3)+cities.Count(c=>c.owner==owner&&c.build>=0&&BuildUnit(c.build)==3);
  if(own==0)return false;bool affordable=net>=RivalScoutRules.Income||net>=0&&nations[owner].gold>=RivalScoutRules.Treasury;if(!affordable)return false;
  return scouts<(own>=RivalScoutRules.SecondCities&&net>=RivalScoutRules.SecondIncome?2:1);}
 // One turn of a rival's Scout. True when it had something to do.
 bool ScoutAI(Unit u){
  if(u.type!=3||!modernRules||!ScoutingRival(u.owner))return false;
  // Someone at the door: the Scout serves as a soldier, as before.
  if(AtWarWithAnyone(u.owner)&&VisibleThreat(u))return false;
  // Hurt: home to heal, and stay until mended.
  if(u.hp<=RivalScoutRules.Home||u.hp<9&&cities.Any(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y)){
   var home=cities.Where(c=>c.owner==u.owner&&At(c.x,c.y).realm==At(u.x,u.y).realm).OrderBy(c=>Distance(u.x,u.y,c.x,c.y)).ThenBy(c=>c.id).FirstOrDefault();
   if(home==null||home.x==u.x&&home.y==u.y)return true;if(Order(u,home.x,home.y))return true;return true;}
  if(scoutRest==null){scoutRest=new Dictionary<int,int>();scoutsDone=new Dictionary<int,int>();}
  int until;if(scoutRest.TryGetValue(u.id,out until)&&turn<until)return true;
  if(u.hasOrder)CancelOrder(u);u.fortified=false;int moves=u.moves;u.exploring=true;ExploreStep(u);if(!units.Contains(u))return true;
  // ExploreStep lets go of a unit that can reach nothing new.
  if(!u.exploring&&u.moves==moves&&!VisibleThreat(u)){scoutRest[u.id]=turn+RivalScoutRules.Rest;scoutsDone[u.owner]=turn+RivalScoutRules.NoMore;}
  u.exploring=false;return true;}
}
}
