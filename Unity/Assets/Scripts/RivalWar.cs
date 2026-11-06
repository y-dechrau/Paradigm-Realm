using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Rival armies and war (owner request, 6 Nov 2026). Measured games had no wars: rivals befriended everyone they met, a friend
// cannot be attacked, armies stayed Warriors at a low unit limit and gold piled up. Here home rivals keep only soldiers
// against a limit that grows with their cities, upgrade and buy with the same prices as the player, sometimes want a
// neighbour's land, say so before attacking, fight only when the exact combat result favours them, fall back when hurt, and
// do not make peace merely because time has passed. Rivals get no hidden sight or bonus: they judge what they can see.
public partial class World {
 // Home rivals only: the sealed peoples cannot reach anyone else, and leaving them alone keeps a crowded turn short.
 bool ActiveRival(int owner){return owner>0&&!(gatesLocked&&IsNativeNation(owner));}

 public int SoldierCount(int owner){return units.Count(u=>u.owner==owner&&Power[u.type]>0&&!IsNaval(u.type));}
 // Three soldiers and two a city, half as many again at war.
 public int SoldierLimit(int owner){int limit=(3+2*cities.Count(c=>c.owner==owner))*DifficultyTable.RivalSoldiers[Level]/100;return AtWarWithAnyone(owner)?limit*3/2:limit;}
 // What a city trains when it has no settler, worker or ship to make: the best soldier it can, every fourth one a ranged unit.
 int AIMilitaryBuild(City c,List<int> choices,List<Unit> own){
  int owner=c.owner;var soldiers=choices.Where(b=>BuildUnit(b)>=2&&!IsNaval(BuildUnit(b))&&MilitaryValue(BuildUnit(b))>0).OrderByDescending(b=>MilitaryValue(BuildUnit(b))).ToList();
  if(!ActiveRival(owner)){if(own.Count>=14)return -1;return soldiers.DefaultIfEmpty(2).First();}
  int have=SoldierCount(owner);
  if(have>=SoldierLimit(owner)){
   // At the limit, an outdated soldier the empire cannot upgrade makes way for a much better one.
   if(soldiers.Count==0||nations[owner].gold<100)return -1;int best=MilitaryValue(BuildUnit(soldiers[0]));
   var worst=own.Where(u=>SpareTroop(u)&&UpgradeTarget(u)<0&&MilitaryValue(u.type)+3<=best).OrderBy(u=>MilitaryValue(u.type)).ThenBy(u=>u.id).FirstOrDefault();
   if(worst==null)return -1;units.Remove(worst);
  }
  // Only line troops take cities, so ranged units, however strong on paper, are one in four.
  var line=soldiers.Where(b=>!IsRanged(BuildUnit(b))).ToList();var ranged=soldiers.Where(b=>IsRanged(BuildUnit(b))).ToList();
  if(have%4==3&&ranged.Count>0)return ranged[0];
  return line.Count>0?line[0]:ranged.Count>0?ranged[0]:2;
 }
 // An outdated soldier upgrades in a city when the treasury can spare it, walking to one close by first.
 internal bool AIUpgrade(Unit u){
  if(!ActiveRival(u.owner)||Power[u.type]==0||IsNaval(u.type)||u.moves<=0)return false;int t=UpgradeTarget(u);
  if(t<0||!nations[u.owner].tech[UnitTech[t]]||!HasResource(u.owner,UnitResource[t]))return false;
  int price=(UnitCosts[t]-UnitCosts[u.type])*2;if(nations[u.owner].gold<price+20)return false;
  if(cities.Any(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y))return Upgrade(u);
  if(AtWarWithAnyone(u.owner))return false;
  var home=cities.Where(c=>c.owner==u.owner&&Distance(u.x,u.y,c.x,c.y)<=8).OrderBy(c=>Distance(u.x,u.y,c.x,c.y)).ThenBy(c=>c.id).FirstOrDefault();
  return home!=null&&Order(u,home.x,home.y);
 }
 // A rival with money to spare finishes what its cities are making, at the player's prices. Projects cannot be bought.
 internal void AIRush(City c){
  if(!ActiveRival(c.owner))return;var n=nations[c.owner];int reserve=(AtWarWithAnyone(c.owner)?40:100)+10*cities.Count(a=>a.owner==c.owner);
  foreach(bool building in new[]{false,true}){
   int b=building?c.buildingProject:c.build;if(b<0||IsProject(b)||BuildReason(c,b)!="")continue;
   int price=RushPrice(c,building);if(price<=0||n.gold-price<reserve)continue;
   n.gold-=price;if(building)c.buildingProduction=CostFor(c,b);else c.production=CostFor(c,b);
  }
 }
 // Combat here has no dice, so a rival can know the result: it fights when it kills, or when it hurts more than it is hurt
 // and survives.
 public bool GoodFight(Unit u,Unit target){
  var e=BestDefender(u,target.x,target.y);if(e==null)return true;int dealt,taken;Exchange(u,e,out dealt,out taken);
  if(dealt>=e.hp)return true;if(taken>=u.hp)return false;return dealt>=taken;
 }
 int aiFoeOwner=-1,aiFoeTurn=-1;List<Unit> aiFoes;List<City> aiFoeCities;
 void LookForFoes(int owner){
  if(aiFoeOwner==owner&&aiFoeTurn==turn&&aiFoes!=null)return;aiFoeOwner=owner;aiFoeTurn=turn;
  aiFoes=units.Where(a=>AtWar(owner,a.owner)&&VisibleTo(owner,a.x,a.y)).ToList();aiFoeCities=cities.Where(c=>AtWar(owner,c.owner)&&VisibleTo(owner,c.x,c.y)).ToList();
 }
 // A soldier at war: fall back when hurt, take the nearest fight it can win, and march on a city only in enough strength.
 // Returns false when it has nothing to do with the war.
 internal bool WarUnitAI(Unit u){
  int owner=u.owner;if(!AtWarWithAnyone(owner))return false;LookForFoes(owner);
  bool inCity=cities.Any(c=>c.owner==owner&&c.x==u.x&&c.y==u.y);
  if(u.hp<=3){if(inCity){u.fortified=true;return true;}var refuge=cities.Where(c=>c.owner==owner).OrderBy(c=>Distance(u.x,u.y,c.x,c.y)).ThenBy(c=>c.id).FirstOrDefault();if(refuge!=null&&Order(u,refuge.x,refuge.y))return true;}
  foreach(var foe in aiFoes.Where(a=>a.hp>0&&units.Contains(a)&&AtWar(owner,a.owner)).OrderBy(a=>Distance(u.x,u.y,a.x,a.y)).ThenBy(a=>a.id).Take(4)){
   if(Distance(u.x,u.y,foe.x,foe.y)>12)break;if(!GoodFight(u,foe))continue;if(Order(u,foe.x,foe.y))return true;
  }
  var city=aiFoeCities.Where(c=>c.owner!=owner&&AtWar(owner,c.owner)).OrderBy(c=>Distance(u.x,u.y,c.x,c.y)).ThenBy(c=>c.id).FirstOrDefault();
  if(city!=null){
   int defenders=units.Count(a=>a.owner==city.owner&&a.x==city.x&&a.y==city.y&&Power[a.type]>0),near=units.Count(a=>a.owner==owner&&Power[a.type]>0&&!IsNaval(a.type)&&Distance(a.x,a.y,city.x,city.y)<=3),d=Distance(u.x,u.y,city.x,city.y);
   if(d<=3&&defenders>0&&near<defenders+2){u.fortified=true;return true;}
   var guard=units.FirstOrDefault(a=>a.owner==city.owner&&a.x==city.x&&a.y==city.y&&Power[a.type]>0);
   if(guard!=null&&d<=1&&!GoodFight(u,guard)){u.fortified=true;return true;}
   if(Order(u,city.x,city.y))return true;
  }
  return false;
 }
 // How much a home rival wants to attack another home civilisation, from what it can see; 50 or more starts it down the road
 // to war. Agendas and traits make leaders differ, and a small roll fixed for each game means they differ between games.
 public int Ambition(int a,int b){
  // The player gets a longer grace, to build a first army before anyone comes for them.
  if(turn<(b==0?PlayerGraceTurn:40)||!ActiveRival(a)||a==b||(gatesLocked&&IsNativeNation(b))||!HasContact(a,b))return 0;
  var mine=cities.Where(c=>c.owner==a).ToList();var theirs=cities.Where(c=>c.owner==b&&nations[a].known[c.y*Width+c.x]).ToList();if(mine.Count==0||theirs.Count==0)return 0;
  int score=0,agenda=CivLore.Of(nations[a].civ)!=null?CivLore.Of(nations[a].civ).Agenda:-1;
  score+=agenda==4||agenda==7?20:agenda==6?10:agenda==0?-5:agenda==5?-20:0;
  if(HasTrait(a,9))score+=10;if(HasTrait(a,11))score+=5;if(HasTrait(a,5))score-=10;
  score+=(int)(((uint)(seed*31+a*17)>>3)&15)+DifficultyTable.WarLust[Level];
  int close=mine.Min(c=>theirs.Min(o=>Distance(c.x,c.y,o.x,o.y)));
  int reach=close<=10?20:close<=16?10:-30;
  if(reach>0&&Landmass(mine[0].x,mine[0].y)!=Landmass(theirs[0].x,theirs[0].y)&&!cities.Any(c=>c.owner==a&&c.portLinked))reach=-30;
  score+=reach;
  int strength=KnownMilitary(a,a)*100/(KnownMilitary(a,b)+2*theirs.Count);
  score+=strength>=200?30:strength>=150?20:strength>=120?10:strength<100?-30:0;
  // A neighbour pulling far ahead draws enemies, which slows a runaway.
  int theirCities=cities.Count(c=>c.owner==b);score+=theirCities>=2*mine.Count?20:theirCities>mine.Count?5:0;
  score-=Opinion(a,b)/10;
  // A civilisation building toward Transcendence is everyone's problem.
  if(nations[b].projectStage>=1||nations[b].projectCity>=0)score+=40;
  if(AtWarWithAnyone(a))score-=40;
  var pair=Relation(a,b);if(pair.warStarted>0&&turn-pair.warStarted<30)score-=20;
  return score;
 }
 // Peace: a rival asks when it is losing or the war has dragged on, and a rival that is clearly winning will not stop early.
 internal bool RivalWantsPeace(int a,int b){var p=Relation(a,b);int age=turn-p.warStarted;if(age<6)return false;if(!ActiveRival(a))return true;return age>=40||KnownMilitary(a,a)*3<KnownMilitary(a,b)*2;}
 internal int PeaceRefusal(DiplomaticOffer o){if(!ActiveRival(o.to))return 0;int age=turn-Relation(o.from,o.to).warStarted;return age<40&&KnownMilitary(o.to,o.to)*2>=KnownMilitary(o.to,o.from)*3?100:0;}
}
}
