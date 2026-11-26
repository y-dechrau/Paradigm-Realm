using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Barbarians (owner request, 16 Nov 2026). Like the wild animals they belong to nobody and are kept in their own lists, so they
// take no part in diplomacy, victory or rival planning. War camps appear on unclaimed land that no civilisation can see, from
// turn CampsFrom, and each sends out raiders. Raiders hunt units outside cities, pillage farms, mines and roads, and plunder
// gold from a city left without a soldier. They must contend with the wild animals as everyone does: hunters attack them, they
// fight back, and they hunt grazers for food. Any civilisation's soldiers can attack raiders and camps by moving onto them;
// destroying a camp pays gold. Their strength grows with the most advanced home civilisation's age. Off in older saves.
[Serializable] public class BarbarianCamp{public int id,x,y,hp=10,nextRaid=4,raiders,paid;public bool seen;public List<BarbarianDeal> deals=new List<BarbarianDeal>();}
[Serializable] public class Raider{public int id,x,y,hp=10,camp,mounted,pillaged;}
public static class BarbarianRules{
 public const int CampsFrom=12,LandPerCamp=260,Sense=6,Leash=9,BaseRaidEvery=7;
 public static readonly string[] Names={"Barbarian raider","Barbarian horse raider"};
}
public partial class World {
 public bool barbarians;public List<BarbarianCamp> camps=new List<BarbarianCamp>();public List<Raider> raiders=new List<Raider>();
 internal void BarbariansTurnForChecks(){BarbariansTurn();}
 void EnsureBarbarianLists(){if(camps==null)camps=new List<BarbarianCamp>();if(raiders==null)raiders=new List<Raider>();}
 public Raider RaiderAt(int x,int y){if(raiders==null)return null;foreach(var r in raiders)if(r.x==x&&r.y==y&&r.hp>0)return r;return null;}
 public BarbarianCamp CampAt(int x,int y){if(camps==null)return null;foreach(var c in camps)if(c.x==x&&c.y==y&&c.hp>0)return c;return null;}
 // Their strength follows the home civilisations' average age, rounded down (owner request, 26 Nov 2026: barbarians were far
 // stronger than everyone's soldiers; they followed the most advanced civilisation, struck at 2 and camps defended at 3). A Dawn
 // raider is now a Warrior's match (attack 1, defence 1; horse raiders attack 2), +1 each every two ages; a camp defends at 2.
 [NonSerialized] int barbarianEraTurn=-1,barbarianEraMemo;
 int BarbarianEra(){if(barbarianEraTurn==turn)return barbarianEraMemo;int sum=0,n=0;var living=new HashSet<int>(cities.Select(c=>c.owner));for(int o=0;o<civCount;o++)if(!IsNativeNation(o)&&(o==0||living.Contains(o))){sum+=Era(o);n++;}barbarianEraMemo=n==0?0:sum/n;barbarianEraTurn=turn;return barbarianEraMemo;}
 public int RaiderAttack(Raider r){return 1+BarbarianEra()/2+(r.mounted>0?1:0);}
 public int RaiderDefence(Raider r){var t=At(r.x,r.y);return 1+BarbarianEra()/2+(t!=null?TerrainDefence(t.terrain):0);}
 public int CampDefence(BarbarianCamp c){var t=At(c.x,c.y);return 2+BarbarianEra()/2+(t!=null?TerrainDefence(t.terrain):0);}
 public int CampReward(){return 25+15*BarbarianEra();}
 bool BarbarianGround(Tile t){return t!=null&&t.realm==0&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&!Barren(t.terrain)&&t.gateLink==0&&!CityCentres().Contains(t.y*Width+t.x);}
 bool OpenToRaider(Tile t){return BarbarianGround(t)&&RaiderAt(t.x,t.y)==null&&(animals==null||AnimalAt(t.x,t.y)==null)&&!units.Any(u=>u.x==t.x&&u.y==t.y);}
 bool SeenByAnyone(int x,int y){for(int o=0;o<civCount;o++)if(VisibleTo(o,x,y))return true;return false;}
 public int CampLimit(){if(!barbarians)return 0;int land=tiles.Count(t=>t.realm==0&&!IsWater(t.terrain)&&!IsImpassable(t.terrain));return Math.Max(2,land/BarbarianRules.LandPerCamp*DifficultyTable.Animals[Level]/100);}
 void SpawnCamp(Random r){
  for(int attempt=0;attempt<60;attempt++){
   var t=tiles[r.Next(mapHeight*Width)];if(!BarbarianGround(t)||t.owner>=0||t.resource>0||t.village||t.ruin||CampAt(t.x,t.y)!=null||!OpenToRaider(t))continue;
   if(SeenByAnyone(t.x,t.y))continue;
   if(cities.Any(c=>Distance(c.x,c.y,t.x,t.y)<7)||units.Any(u=>Distance(u.x,u.y,t.x,t.y)<5)||camps.Any(c=>Distance(c.x,c.y,t.x,t.y)<10))continue;
   camps.Add(new BarbarianCamp{id=nextId++,x=t.x,y=t.y,nextRaid=turn+2});return;}
 }
 void BarbariansTurn(){
  if(!barbarians)return;EnsureBarbarianLists();BargainsTurn();var r=new Random(seed*53+turn*11);
  if(turn>=BarbarianRules.CampsFrom&&camps.Count<CampLimit()&&r.Next(3)==0)SpawnCamp(r);
  int era=BarbarianEra();
  foreach(var c in camps){
   c.hp=Math.Min(10,c.hp+1);if(!c.seen&&VisibleTo(0,c.x,c.y)){c.seen=true;Note("Your scouts have found a barbarian camp. Destroy it before its raiders grow bold ("+CampReward()+" gold to whoever burns it). Click the camp to pay it off or hire it.");}
   c.raiders=raiders.Count(x=>x.camp==c.id&&x.hp>0);
   bool hired=Contract(c)!=null;
   if(turn>=c.nextRaid&&c.raiders<2+(era>=2?1:0)+(hired?BarbarianTerms.ExtraRaiders:0)){
    for(int k=0;k<6;k++){var n=Step(c.x,c.y,HexOr(k,true),HexOr(k,false));if(!OpenToRaider(n))continue;raiders.Add(new Raider{id=nextId++,x=n.x,y=n.y,camp=c.id,mounted=era>=1&&r.Next(3)==0?1:0});break;}
    c.nextRaid=turn+(hired?BarbarianTerms.HiredRaidEvery:BarbarianRules.BaseRaidEvery+r.Next(4));}
  }
  foreach(var raider in raiders.ToList()){if(raider.hp<=0)continue;raider.hp=Math.Min(10,raider.hp+1);RaiderActs(raider,r);}
  raiders.RemoveAll(x=>x.hp<=0);camps.RemoveAll(x=>x.hp<=0);
  CheckVictory();
 }
 // A unit a raider may strike: on land, outside every city; soldiers on the tile defend first.
 // Nobody who has paid the raider's camp is prey (BarbarianDeals.cs).
 Unit RaiderPrey(int x,int y,BarbarianCamp home){var t=At(x,y);if(t==null||IsWater(t.terrain)||cities.Any(c=>c.x==x&&c.y==y))return null;return units.Where(u=>u.x==x&&u.y==y&&!CampSpares(home,u.owner)).OrderByDescending(u=>Power[u.type]>0).ThenByDescending(u=>Strength(u,null,true)).ThenByDescending(u=>u.hp).ThenBy(u=>u.id).FirstOrDefault();}
 void RaiderActs(Raider raider,Random r){
  var home=camps.FirstOrDefault(c=>c.id==raider.camp&&c.hp>0);int steps=raider.mounted>0?2:1;var contract=Contract(home);int mark=contract==null?-1:contract.target;
  for(int step=0;step<steps&&raider.hp>0;step++){
   // 1. Something to fight right beside it: a unit outside a city, or a hunting animal.
   Unit prey=null;int least=int.MaxValue;foreach(var u in units){if(IsNaval(u.type))continue;int d=Distance(u.x,u.y,raider.x,raider.y);if(d>BarbarianRules.Sense||d>=least||CampSpares(home,u.owner))continue;var found=RaiderPrey(u.x,u.y,home);if(found==null)continue;least=d;prey=found;}
   if(prey!=null&&least==1){RaiderStrikes(raider,prey);return;}
   // 2. A city with no soldier beside it: plunder it, then head home.
   var town=cities.FirstOrDefault(c=>Distance(c.x,c.y,raider.x,raider.y)==1&&!CampSpares(home,c.owner)&&!units.Any(u=>u.owner==c.owner&&u.x==c.x&&u.y==c.y&&Power[u.type]>0));
   if(town!=null){Plunder(raider,town);return;}
   // 3. Standing on someone's improvement: wreck it.
   var here=At(raider.x,raider.y);if(here!=null&&here.owner>=0&&!CampSpares(home,here.owner)&&(here.farm||here.mine||here.road)&&raider.pillaged<turn-1){Pillage(raider,here);return;}
   // 4. Choose where to go: a unit in sight, else an undefended town or improvement within reach, else a grazer to hunt, else roam near home.
   Tile goal=null;
   if(prey!=null&&(prey.hp<=raider.hp||Power[prey.type]==0||Strength(prey,null,true)<=RaiderAttack(raider)))goal=At(prey.x,prey.y);
   if(goal==null){var bare=cities.Where(c=>Distance(c.x,c.y,raider.x,raider.y)<=BarbarianRules.Sense&&!CampSpares(home,c.owner)&&!units.Any(u=>u.owner==c.owner&&u.x==c.x&&u.y==c.y&&Power[u.type]>0)).OrderBy(c=>Distance(c.x,c.y,raider.x,raider.y)).FirstOrDefault();if(bare!=null)goal=At(bare.x,bare.y);}
   if(goal==null){Tile best=null;int bd=5;for(int dy=-4;dy<=4;dy++)for(int dx=-4;dx<=4;dx++){var t=Step(raider.x,raider.y,dx,dy);if(t==null||t.owner<0||CampSpares(home,t.owner)||!(t.farm||t.mine||t.road)||!OpenToRaider(t))continue;int d=Distance(t.x,t.y,raider.x,raider.y);if(d<bd){bd=d;best=t;}}goal=best;}
   if(goal==null&&animals!=null&&raider.hp>=7){var meal=animals.Where(a=>a.hp>0&&!AnimalCatalog.Carnivore[a.kind]&&Distance(a.x,a.y,raider.x,raider.y)<=3).OrderBy(a=>Distance(a.x,a.y,raider.x,raider.y)).FirstOrDefault();
    if(meal!=null){if(Distance(meal.x,meal.y,raider.x,raider.y)==1){RaiderHunts(raider,meal);return;}goal=At(meal.x,meal.y);}}
   // Hired: whatever is in reach of the people they were paid to raid comes first, and otherwise they march on it. No leash.
   if(mark>=0){bool theirs=goal!=null&&(goal.owner==mark||units.Any(u=>u.owner==mark&&u.x==goal.x&&u.y==goal.y));if(!theirs){var march=MarchStep(raider,mark);if(march!=null){raider.x=march.x;raider.y=march.y;continue;}}}
   if(goal==null||mark<0&&home!=null&&Distance(goal.x,goal.y,home.x,home.y)>BarbarianRules.Leash+3){
    // Roam within reach of the camp; a raider whose camp has gone wanders.
    Tile pick=null;for(int attempt=0;attempt<6&&pick==null;attempt++){int k=r.Next(6);var n=Step(raider.x,raider.y,HexOr(k,true),HexOr(k,false));if(!OpenToRaider(n))continue;if(mark<0&&home!=null&&Distance(n.x,n.y,home.x,home.y)>BarbarianRules.Leash)continue;pick=n;}
    if(pick!=null){raider.x=pick.x;raider.y=pick.y;}continue;}
   Tile next=null;int nd=Distance(raider.x,raider.y,goal.x,goal.y);
   for(int k=0;k<6;k++){var n=Step(raider.x,raider.y,HexOr(k,true),HexOr(k,false));if(n==null||!OpenToRaider(n))continue;int d=Distance(n.x,n.y,goal.x,goal.y);if(d<nd){nd=d;next=n;}}
   if(next==null)return;raider.x=next.x;raider.y=next.y;
  }
 }
 void RaiderStrikes(Raider raider,Unit prey){
  int atk=RaiderAttack(raider),def=Strength(prey,null,true);prey.hp-=Damage(atk,def);if(Power[prey.type]>0)raider.hp-=Damage(def,atk);
  prey.fortified=false;if(!prey.hasOrder&&!prey.exploring)CancelOrder(prey);
  string name=BarbarianRules.Names[raider.mounted];
  if(prey.owner==0)Note(prey.hp<=0?"A "+name+" killed your "+UnitName(prey)+".":"A "+name+" attacked your "+UnitName(prey)+".");
  else{var paid=Contract(CampOf(raider));if(paid!=null&&paid.payer==0&&prey.owner==paid.target)Note("Your hired barbarians "+(prey.hp<=0?"killed":"attacked")+" a "+UnitName(prey)+" of "+NationName(prey.owner)+".");}
  if(prey.hp<=0){units.Remove(prey);DrownStranded();}else if(raider.hp<=0){Victory(prey);if(prey.owner==0)Note("Your "+UnitName(prey)+" cut down the "+name+".");}
 }
 void Plunder(Raider raider,City c){
  var n=nations[c.owner];int take=Math.Min(n.gold,15+5*BarbarianEra());n.gold-=take;if(c.owner==0)gold=n.gold;
  {var paid=Contract(CampOf(raider));if(paid!=null&&take>0&&paid.payer!=c.owner){int share=take/BarbarianTerms.EmployerShare;AddDiplomaticGold(paid.payer,share);if(paid.payer==0)Note("Your hired barbarians plundered "+c.name+" of "+NationName(c.owner)+". Your share is "+share+" gold.");}}
  if(c.owner==0)Note("Barbarians plundered "+c.name+" for "+take+" gold. A soldier in the city keeps them out.");
  // Loaded with spoil, the raider makes for home.
  var home=camps.FirstOrDefault(x=>x.id==raider.camp);if(home!=null){for(int k=0;k<6;k++){var nb=Step(raider.x,raider.y,HexOr(k,true),HexOr(k,false));if(nb!=null&&OpenToRaider(nb)&&Distance(nb.x,nb.y,home.x,home.y)<Distance(raider.x,raider.y,home.x,home.y)){raider.x=nb.x;raider.y=nb.y;break;}}}
 }
 void Pillage(Raider raider,Tile t){
  string what=t.farm?"farm":t.mine?"mine":"road";if(t.farm)t.farm=false;else if(t.mine)t.mine=false;else t.road=false;raider.pillaged=turn;networks=null;
  if(t.owner==0)Note("Barbarians wrecked a "+what+" on your land. Workers can rebuild it.");
  else{var paid=Contract(CampOf(raider));if(paid!=null&&paid.payer==0&&t.owner==paid.target)Note("Your hired barbarians wrecked a "+what+" of "+NationName(t.owner)+".");}
 }
 void RaiderHunts(Raider raider,Animal g){
  g.hp-=Damage(RaiderAttack(raider),AnimalCatalog.Defence[g.kind]);raider.hp-=Damage(AnimalCatalog.Attack[g.kind],RaiderDefence(raider));
  if(g.hp<=0){animals.Remove(g);if(raider.hp>0)raider.hp=Math.Min(10,raider.hp+3);}
 }
 // Wild animals deal with barbarians as they do with anyone: a hunter attacks a raider in reach, a grazer one standing beside it.
 Raider RaiderNearAnimal(Animal a,int range){if(!barbarians||raiders==null)return null;Raider best=null;int least=range+1;foreach(var x in raiders){if(x.hp<=0)continue;int d=Distance(a.x,a.y,x.x,x.y);if(d<least){least=d;best=x;}}return best;}
 void AnimalAttacksRaider(Animal a,Raider raider){
  raider.hp-=Damage(AnimalCatalog.Attack[a.kind],RaiderDefence(raider));a.hp-=Damage(RaiderAttack(raider),AnimalCatalog.Defence[a.kind]);
  if(VisibleTo(0,raider.x,raider.y))Note("A "+AnimalCatalog.Names[a.kind]+" fell on a barbarian raider.");
  if(raider.hp<=0)raiders.Remove(raider);
 }
 // A soldier attacks a raider or a camp by moving onto it. Combat has no dice, so the result can be known in advance.
 public void RaiderExchange(Unit u,Raider raider,out int dealt,out int taken){int own=Strength(u,null,false);dealt=Damage(own,RaiderDefence(raider));taken=IsRanged(u.type)?0:Damage(RaiderDefence(raider),own);}
 public void CampExchange(Unit u,BarbarianCamp c,out int dealt,out int taken){int own=Strength(u,null,false);dealt=Damage(own,CampDefence(c));taken=IsRanged(u.type)?0:Damage(CampDefence(c),own);}
 bool FightRaider(Unit u,Raider raider){
  if(Power[u.type]==0)return false;BreakBargain(u.owner,CampOf(raider));int dealt,taken;RaiderExchange(u,raider,out dealt,out taken);LogStrike(u,raider.x,raider.y);raider.hp-=dealt;u.hp-=taken;u.moves=0;
  string name=BarbarianRules.Names[raider.mounted];if(u.owner==0)Note(UnitName(u)+" attacks a "+name+".");
  if(raider.hp<=0){raiders.Remove(raider);if(u.hp>0)Victory(u);if(u.owner==0)Note("The "+name+" is dead.");}
  if(u.hp<=0){if(u.owner==0)Note("The "+name+" killed your "+UnitName(u)+".");units.Remove(u);DrownStranded();}
  CheckVictory();return true;
 }
 bool AttackCamp(Unit u,BarbarianCamp c){
  if(Power[u.type]==0)return false;BreakBargain(u.owner,c);int dealt,taken;CampExchange(u,c,out dealt,out taken);LogStrike(u,c.x,c.y);c.hp-=dealt;u.hp-=taken;u.moves=0;
  if(c.hp<=0){if(u.owner!=0&&CampSpares(c,0))Note("A barbarian camp you had paid has been burned by "+NationName(u.owner)+".");camps.Remove(c);int reward=CampReward();nations[u.owner].gold+=reward;if(u.owner==0){gold=nations[0].gold;Note("Your "+UnitName(u)+" burned the barbarian camp: "+reward+" gold.");}
   if(u.hp>0){Victory(u);if(At(c.x,c.y)!=null&&!units.Any(o=>o.owner!=u.owner&&o.x==c.x&&o.y==c.y)){LogStep(u,c.x,c.y);u.x=c.x;u.y=c.y;}}}
  else if(u.owner==0)Note(UnitName(u)+" attacks the barbarian camp ("+c.hp+" of 10 left).");
  if(u.hp<=0){if(u.owner==0)Note("Your "+UnitName(u)+" fell attacking the barbarian camp.");units.Remove(u);DrownStranded();}
  CheckVictory();return true;
 }
 public bool GoodRaid(Unit u,int dealt,int taken,int theirHp){if(dealt>=theirHp)return true;return taken<u.hp&&dealt>=taken;}
 // Rival soldiers clear barbarians close to their land when the fight favours them.
 bool HuntBarbarianAI(Unit u){
  if(!barbarians||IsNaval(u.type)||Power[u.type]==0)return false;EnsureBarbarianLists();Tile best=null;int least=7;
  foreach(var x in raiders){int d=Distance(u.x,u.y,x.x,x.y);if(d>=least||x.hp<=0||!VisibleTo(u.owner,x.x,x.y)||RaiderSpares(x,u.owner))continue;int dealt,taken;RaiderExchange(u,x,out dealt,out taken);if(!GoodRaid(u,dealt,taken,x.hp))continue;least=d;best=At(x.x,x.y);}
  foreach(var c in camps){int d=Distance(u.x,u.y,c.x,c.y);if(d>=least||!VisibleTo(u.owner,c.x,c.y)||CampSpares(c,u.owner))continue;int dealt,taken;CampExchange(u,c,out dealt,out taken);if(!GoodRaid(u,dealt,taken,c.hp))continue;least=d;best=At(c.x,c.y);}
  return best!=null&&Order(u,best.x,best.y);
 }
 public bool BarbarianNear(int owner,int x,int y,int range){if(!barbarians||raiders==null)return false;foreach(var r in raiders)if(r.hp>0&&Distance(r.x,r.y,x,y)<=range&&VisibleTo(owner,r.x,r.y)&&!RaiderSpares(r,owner))return true;return false;}
 public string RaiderLabel(Raider r){return BarbarianRules.Names[r.mounted]+" (attack "+RaiderAttack(r)+", defence "+RaiderDefence(r)+", health "+r.hp+(RaiderSpares(r,0)?"; "+BargainText(0,CampOf(r)):"")+")";}
 public string CampLabel(BarbarianCamp c){return "Barbarian camp (defence "+CampDefence(c)+", health "+c.hp+"; "+CampReward()+" gold to whoever burns it; "+(CampSpares(c,0)?BargainText(0,c):"click to bargain")+")";}
}
}
