using System;
using System.Linq;
namespace ParadigmRealm {
public static class EmpireChecks {
 static void Check(bool ok,string name){if(!ok)throw new Exception("Empire: "+name);}
 static World Setup(){var w=World.New(917);foreach(var t in w.tiles){t.terrain=0;t.village=false;}var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.gold=w.nations[0].gold=1000;return w;}
 static void Grant(World w,int owner,string id){w.nations[owner].tech[TechnologyCatalog.Index(id)]=true;}
 public static void Run(){
 Check(World.UnitNames.Length==World.Power.Length&&World.Power.Length==World.Moves.Length&&World.Moves.Length==World.UnitCosts.Length&&World.UnitCosts.Length==World.UnitTech.Length&&World.UnitTech.Length==World.UnitResource.Length&&World.UnitResource.Length==World.Upkeep.Length,"unit definition alignment");
 Check(World.BuildingIds.Length==World.BuildingNames.Length&&World.BuildingIds.Length==World.BuildingCosts.Length&&World.BuildingIds.Length==World.BuildingTech.Length,"building definition alignment");
 var w=Setup();var c=w.cities.First(a=>a.owner==0);c.pop=3;int worked=w.Worked(c).Count;Check(w.AdjustSpecialist(c,0,1)&&w.Worked(c).Count==worked-1,"specialists replace workers");Check(w.AdjustSpecialist(c,1,1)&&w.AdjustSpecialist(c,2,1)&&!w.AdjustSpecialist(c,0,1),"specialist population cap");c.pop=1;w.NormaliseSpecialists(c);Check(c.scientists+c.merchants+c.entertainers==1,"starvation specialist clamp");
 Check(!w.ChangeGovernment(0,2),"government research gate");Grant(w,0,"philosophy");Check(w.ChangeGovernment(0,2)&&w.nations[0].revolution==2&&!w.ChangeGovernment(0,0),"revolution blocks repeated switch");w.EndTurn();w.EndTurn();Check(w.nations[0].revolution==0,"revolution expires");
 Check(w.GrowthLimit(c)==6,"default growth cap");c.buildings.Add(37);Check(w.GrowthLimit(c)==12,"aqueduct cap");c.buildings.Add(41);Check(w.GrowthLimit(c)==20,"sewer cap");
 w=Setup();c=w.cities.First(a=>a.owner==0);Grant(w,0,"mysticism");Check(w.ChooseProject(c,36,false),"new building selectable");int money=w.gold;Check(w.CostFor(c,36)==w.Cost(36)*(w.Era(0)==0?2:1),"Dawn Age buildings cost twice as much");Check(w.Rush(c,true)&&w.gold==money-w.CostFor(c,36)*4,"rush price: 2 gold per shield, doubled when nothing is built yet");w.Produce(c,1);Check(w.HasBuilding(c,36),"rushed building completes");
 var rival=w.cities.First(a=>a.owner==1);Grant(w,0,"writing");Grant(w,1,"writing");w.ChooseProject(c,60,false);w.ChooseProject(rival,60,false);rival.buildingProduction=20;int rivalGold=w.nations[1].gold;w.Produce(c,w.Cost(60));Check(w.OwnWonder(0,60)&&rival.buildingProject==-1&&w.nations[1].gold==rivalGold+10&&w.BuildReason(rival,60)!="","world wonder uniqueness and refund");
 var settler=w.Add(0,0,c.x+6,c.y);var city2=w.Found(settler);Check(city2!=null,"second city");var caravan=w.Add(0,11,city2.x,city2.y);caravan.homeCity=c.id;money=w.gold;Check(w.DeliverCaravan(caravan,city2)&&!w.units.Contains(caravan)&&w.gold>money&&w.TradeIncome(0)==6,"domestic caravan delivery and both endpoint income");caravan=w.Add(0,11,city2.x,city2.y);caravan.homeCity=c.id;Check(!w.DeliverCaravan(caravan,city2),"duplicate route blocked");
 w.met[1]=true;w.SetBorders(1);caravan=w.Add(0,11,rival.x-1,rival.y);caravan.homeCity=c.id;Check(w.DeliverCaravan(caravan,rival)&&w.TradeIncome(0)==9,"foreign route");w.SetWar(1,true);Check(w.TradeIncome(0)==6,"war suspends trade");w.SetWar(1,false);Check(w.TradeIncome(0)==9,"peace restores trade");
 w=Setup();w.met[1]=true;Grant(w,1,"agriculture");Check(w.BuyTechnology(1,TechnologyCatalog.Index("agriculture"))&&w.HasTech(0,"agriculture"),"technology purchase");Check(!w.BuyTechnology(1,TechnologyCatalog.Index("agriculture")),"no repeat technology purchase");money=w.gold;Check(w.GiftGold(1)&&w.gold==money-25&&w.nations[1].relations==10,"diplomatic gift");
 var scout=w.Add(0,3,12,15);w.At(13,15).village=true;int goldBefore=w.nations[0].gold;Check(w.Move(scout,13,15)&&w.At(13,15).village&&w.At(13,15).villageOwner==0,"occupied village joins the civilisation");
 Check(w.nations[0].known.Count(k=>k)>0&&Enumerable.Range(-4,9).All(d=>w.At(13+d,15)==null||w.nations[0].known[15*w.Width+w.At(13+d,15).x]),"village reveals the land within 4 tiles");
 var villageTile=w.At(13,15);villageTile.owner=-1;foreach(var tt in w.tiles)if(tt.owner==0&&w.Distance(tt.x,tt.y,13,15)<=4)tt.owner=-1;Check(w.VillageTribute(0)==0,"no tax from a village far from your borders");
 w.At(14,15).owner=0;Check(w.VillageTribute(0)==World.VillageTax,"village within 4 tiles of your borders pays tax");
 scout.x=13;scout.y=17;var rivalScout=w.Add(1,3,12,15);Check(w.Move(rivalScout,13,15)&&villageTile.villageOwner==1&&w.VillageTribute(0)==0,"another civilisation can win the village over");w.AutoExplore(scout);w.CancelActivity(scout);Check(!scout.exploring&&!scout.hasOrder,"X stops exploration");
 StageThreeEconomy();StageThreeTrade();StageThreeHappiness();StageThreeUnits();StageFourCombat();StageSixVictory();StageSevenCity();CivilisationChecks();PathfindingChecks();NavalChecks();NavalAIChecks();StageEightChecks();
 Check(w.Valid(),"expanded state validates");w.tradeRoutes=null;w.UpgradeSave();Check(w.tradeRoutes!=null&&w.Valid(),"legacy absent routes migrated");
 for(int seed=0;seed<3;seed++){w=World.New(seed);w.Found(w.units.First(u=>u.owner==0&&u.type==0));for(int turn=0;turn<50;turn++){w.EndTurn();Check(w.Valid(),"campaign state, seed "+seed+" turn "+turn);}}
 }
 // 16-bit plan Stage 3: production costs, Settler population cost, building tiers and upkeep (CORE-DESIGN-16BIT.md).
 static void StageThreeEconomy(){
  int[] units={2,1,7,4,8,9,0},costs={10,20,20,20,25,40,30};
  for(int i=0;i<units.Length;i++)Check(World.BaseCost(World.UnitBuild(units[i]))==costs[i],World.UnitNames[units[i]]+" costs "+costs[i]);
  foreach(int b in World.BuildingIds.Where(id=>!World.IsProject(id))){int cost=World.BaseCost(b);if(World.IsWonder(b))Check(World.WonderEra(b)>=1&&World.WonderEra(b)<=6&&cost==100+50*World.WonderEra(b)&&World.BuildingUpkeep(b)==0,"wonder cost and free upkeep: "+World.BuildName(b));else Check((cost==40||cost==60||cost==80)&&World.BuildingUpkeep(b)==(cost>40?1:0),"building tier and upkeep: "+World.BuildName(b));}
  var w=Setup();var c=w.cities.First(a=>a.owner==0);w.speed=1;Grant(w,0,"agriculture");
  c.pop=1;Check(w.BuildReason(c,0)!="","Settler needs city size 2");
  c.pop=2;Check(w.BuildReason(c,0)==""&&w.ChooseProject(c,0,false),"Settler buildable at size 2");
  int settlers=w.units.Count(u=>u.owner==0&&u.type==0);w.Produce(c,w.Cost(0));Check(c.pop==1&&w.units.Count(u=>u.owner==0&&u.type==0)==settlers+1,"Settler costs 1 population");
  w.units.RemoveAll(u=>u.owner==0);int free=World.FreeUnitsPerCity+w.GovernmentSupport(0);
  for(int i=0;i<free;i++)w.Add(0,2,c.x,c.y);int expense=w.Budget(0)[1];
  w.Add(0,2,c.x,c.y);Check(w.Budget(0)[1]==expense+1,"each unit beyond 3 per city plus government support costs 1 gold");
  expense=w.Budget(0)[1];c.buildings.Add(36);Check(w.Budget(0)[1]==expense,"40-tier building is free");
  c.buildings.Add(31);Check(w.Budget(0)[1]==expense+1,"60-tier building costs 1 gold");
  c.buildings.Add(60);Check(w.Budget(0)[1]==expense+1,"wonders are free");
 }
 // Stage 3: Trade as the third tile yield, split into science and gold by the science rate (save version 11).
 static void StageThreeTrade(){
  var w=Setup();var c=w.cities.First(a=>a.owner==0);var t=w.At(c.x+1,c.y);t.resource=0;t.road=false;
  int[] terrain={0,4,6,7,2},trade={0,2,2,1,0};for(int i=0;i<terrain.Length;i++){t.terrain=terrain[i];Check(w.TileYield(t)[2]==trade[i],World.TerrainNames[terrain[i]]+" trade "+trade[i]);}
  t.terrain=0;t.road=true;Check(w.TileYield(t)[2]==1,"road adds 1 trade on grassland");t.terrain=2;Check(w.TileYield(t)[2]==0,"road adds no trade on hills");
  t.resource=3;Check(w.TileYield(t)[2]==2,"gold deposit adds 2 trade");t.resource=0;t.terrain=0;
  Check(World.CentreYield(0)[2]==4&&World.CentreYield(2)[2]==3,"city centre trade: 3 base plus road on open land");
  Check(w.SetScienceRate(0,70)&&w.nations[0].scienceRate==w.ScienceCap(0)&&w.ScienceCap(0)==60&&w.SetScienceRate(0,-10)&&w.nations[0].scienceRate==0&&w.SetScienceRate(0,35)&&w.nations[0].scienceRate==30,"science rate clamps to 0-60 in steps of 10");
  c.pop=2;foreach(var a in w.Workable(c)){a.terrain=0;a.resource=0;a.road=false;}Check(w.Trade(c)==World.CentreYield(0)[2],"citizens add no Trade of their own (bridging rule retired in Stage 5)");
  c.pop=3;foreach(var a in w.Workable(c)){a.terrain=4;a.resource=0;}
  int total=w.Trade(c);Check(total>=7,"worked ocean adds trade");
  for(int rate=0;rate<=60;rate+=10){w.SetScienceRate(0,rate);Check(w.Yield(c)[3]==total-total*rate/100,"gold is the trade left after science at "+rate+"%");}
  w.SetScienceRate(0,0);int science0=w.Yield(c)[2];w.SetScienceRate(0,60);Check(w.Yield(c)[2]==science0+total*60/100,"science share is trade x rate / 100, rounded down");
  int noLibrary=w.Yield(c)[2];c.library=true;Check(w.Yield(c)[2]==noLibrary+2,"Library adds 2 science");c.library=false;
  int income=w.Budget(0)[0];c.buildings.Add(31);Check(w.Budget(0)[0]==income+2,"Market adds 2 gold");
  w.SetScienceRate(0,50);int normal=w.ContentSlots(c);w.SetScienceRate(0,60);Check(w.ContentSlots(c)==normal+1,"60% science (low tax) adds a content slot");w.SetScienceRate(0,30);Check(w.ContentSlots(c)==normal-1,"30% science (high tax) removes a content slot");w.SetScienceRate(0,50);
  w.version=10;w.nations[0].tax=0;w.nations[1].tax=2;w.nations[2].tax=1;w.UpgradeSave();
  Check(w.version==21&&w.nations[0].scienceRate==60&&w.nations[1].scienceRate==30&&w.nations[2].scienceRate==50&&w.Valid(),"version 10 tax setting migrates to a science rate");
  w.nations[0].scienceRate=55;Check(!w.Valid(),"off-step science rate rejected");
 }
 // Stage 3: content and unhappy citizens, disorder (CORE-DESIGN-16BIT.md).
 static void StageThreeHappiness(){
  var w=Setup();var c=w.cities.First(a=>a.owner==0);c.pop=3;
  Check(w.ContentSlots(c)==World.BaseContent,"a plain city has 4 content slots");
  Check(w.ContentCitizens(c)==3&&w.UnhappyCitizens(c)==0&&!w.Disorder(c),"citizens up to the slots are content");
  c.pop=6;Check(w.ContentCitizens(c)==4&&w.UnhappyCitizens(c)==2&&w.Happiness(c)==2&&!w.Disorder(c),"workers beyond the slots are unhappy");
  c.pop=8;Check(w.ContentCitizens(c)==4&&w.UnhappyCitizens(c)==4&&!w.Disorder(c),"equal content and unhappy is not disorder");
  c.pop=9;Check(w.Disorder(c)&&w.Yield(c)[1]==0&&w.Yield(c)[3]==0,"disorder when unhappy outnumber content: no production, no gold");
  int income=w.Budget(0)[0];c.buildings.Add(31);Check(w.Budget(0)[0]==income,"no Market gold during disorder");c.buildings.Remove(31);
  Check(w.GrowthStatus(c).Contains("Disorder"),"growth report names disorder");
  int slots=w.ContentSlots(c),workers=w.Workers(c);Check(w.AdjustSpecialist(c,2,1)&&w.ContentSlots(c)==slots+2&&w.Workers(c)==workers-1&&!w.Disorder(c),"an entertainer adds 2 content slots and ends disorder");
  c.entertainers=0;slots=w.ContentSlots(c);Grant(w,0,"mysticism");c.buildings.Add(36);Check(w.ContentSlots(c)==slots+2&&!w.Disorder(c),"Temple adds 2 content slots");
  c.pop=1;c.scientists=1;Check(w.Workers(c)==0&&w.ContentCitizens(c)==0&&w.UnhappyCitizens(c)==0,"specialists are neither content nor unhappy");
 }
 // Stage 3: healing rates, victories and automatic Veteran / Elite ranks (save version 12).
 static void StageThreeUnits(){
  var w=Setup();var c=w.cities.First(a=>a.owner==0);w.units.RemoveAll(u=>u.owner==0&&u.type==10);
  var open=w.Add(0,2,c.x+4,c.y);open.fortified=false;Check(w.HealAmount(open)==1,"open ground heals 1");
  open.fortified=true;Check(w.HealAmount(open)==2,"fortified heals 2");
  var inCity=w.Add(0,2,c.x,c.y);Check(w.HealAmount(inCity)==3,"own city heals 3");
  c.buildings.Add(35);Check(w.HealAmount(inCity)==5,"Hospital adds 2 in its city");c.buildings.Remove(35);
  c.buildings.Add(32);Check(w.HealAmount(inCity)==10,"Barracks heals fully");
  inCity.hp=3;inCity.moves=World.Moves[inCity.type];open.hp=4;open.moves=World.Moves[open.type];w.EndTurn();Check(inCity.hp==10&&open.hp==6,"healing applied at end of turn");
  var medic=w.Add(0,10,open.x+1,open.y);Check(w.HealAmount(open)==4,"adjacent Medic adds 2");w.units.Remove(medic);
  Check(World.RankFor(0)==0&&World.RankFor(1)==0&&World.RankFor(2)==1&&World.RankFor(4)==1&&World.RankFor(5)==2&&World.RankFor(9)==2,"2 victories Veteran, 5 Elite");
  var u=w.Add(0,2,c.x+5,c.y);for(int i=0;i<5;i++)w.Victory(u);Check(u.victories==5&&u.promotion==2,"victories raise rank automatically");
  w.Victory(u);Check(u.promotion==2,"Elite is the top rank");
  Grant(w,0,"toolmaking");w.ChooseProject(c,2,false);w.Produce(c,w.Cost(2));var trained=w.units.Last(a=>a.owner==0&&a.type==2);Check(trained.promotion==1&&trained.victories==2,"Barracks trains Veterans");
  // Combat victories: a strong attacker kills a weak defender.
  w.met[1]=true;w.SetWar(1,true);var attacker=w.Add(0,6,c.x+6,c.y);var target=w.Add(1,3,c.x+7,c.y);target.hp=1;w.Reveal();attacker.moves=World.Moves[attacker.type];
  Check(w.Move(attacker,target.x,target.y)&&!w.units.Contains(target)&&attacker.victories==1,"killing a defender counts as a victory");
  // Version 11 saves: manual promotions cap at Elite and spare experience counts as Veteran.
  var a3=w.Add(0,2,c.x+2,c.y);a3.promotion=3;a3.victories=0;var a1=w.Add(0,2,c.x+2,c.y+1);a1.promotion=0;a1.experience=5;a1.victories=0;
  w.version=11;w.UpgradeSave();Check(w.version==21&&a3.promotion==2&&a3.victories==5&&a1.promotion==1&&a1.victories==2&&w.Valid(),"version 11 ranks migrate");
  a3.promotion=3;Check(!w.Valid(),"rank above Elite rejected");
 }
 // Civilisations (owner request 29 Oct 2026): 12 premade with two traits each, custom civilisations, trait effects, save 16;
 // plus the tech-filtered build list and auto-explore for combat units.
 // Orders reroute instead of stopping, and auto-explore finds unexplored ground round obstacles (owner request).
 static void PathfindingChecks(){
  var w=Setup();foreach(var t in w.tiles){t.owner=-1;}foreach(var u in w.units.Where(a=>a.owner!=0).ToList())w.units.Remove(u);foreach(var c in w.cities.Where(a=>a.owner!=0).ToList())w.cities.Remove(c);
  var home=w.cities.First(a=>a.owner==0);int y=home.y+3;var walker=w.Add(0,2,home.x,y);w.met[1]=true;var blocker=w.Add(1,2,home.x+5,y);
  Check(w.Order(walker,home.x+5,y)&&walker.hasOrder,"order onto a foreign unit accepted");
  for(int i=0;i<8&&walker.hasOrder;i++){walker.moves=World.Moves[walker.type];w.Reveal();w.ContinueOrder(walker);}Check(!walker.hasOrder&&w.Distance(walker.x,walker.y,blocker.x,blocker.y)==1,"unit arrives next to the occupied destination (at "+walker.x+","+walker.y+", order "+walker.hasOrder+")");
  // A wall of mountains with a gap: the order walks round it rather than stopping.
  w=Setup();foreach(var t in w.tiles)t.owner=-1;foreach(var u in w.units.Where(a=>a.owner!=0).ToList())w.units.Remove(u);home=w.cities.First(a=>a.owner==0);
  int wallX=home.x+3;for(int dy=-6;dy<=3;dy++){var t=w.At(wallX,home.y+dy);if(t!=null)t.terrain=5;}
  walker=w.Add(0,2,home.x+1,home.y);Check(w.Order(walker,home.x+5,home.y),"order behind a wall accepted");for(int i=0;i<16&&walker.hasOrder;i++){walker.moves=World.Moves[walker.type];w.Reveal();w.ContinueOrder(walker);}Check(walker.x==home.x+5&&walker.y==home.y,"unit walks round the wall to its destination");
  // A temporary jam keeps the order instead of cancelling it.
  w=Setup();foreach(var t in w.tiles)t.owner=-1;foreach(var u in w.units.Where(a=>a.owner!=0).ToList())w.units.Remove(u);home=w.cities.First(a=>a.owner==0);
  for(int dy=-8;dy<=8;dy++)if(dy!=0){var t=w.Step(home.x,home.y,3,dy);if(t!=null)t.terrain=5;}
  w.met[1]=true;var jam=w.Add(1,2,home.x+3,home.y);walker=w.Add(0,2,home.x+1,home.y);w.Order(walker,home.x+5,home.y);Check(walker.hasOrder,"order through a gap blocked by a unit waits");for(int i=0;i<3;i++){walker.moves=World.Moves[walker.type];w.Reveal();w.ContinueOrder(walker);}Check(walker.hasOrder,"jammed order is kept over several turns");w.units.Remove(jam);for(int i=0;i<8&&walker.hasOrder;i++){walker.moves=World.Moves[walker.type];w.Reveal();w.ContinueOrder(walker);}Check(walker.x==home.x+5,"order resumes once the gap clears");
  // Auto-explore heads for unexplored ground and finishes when nothing reachable is left.
  w=World.New(95);var first=w.units.First(a=>a.owner==0);var scout=w.Add(0,3,first.x,first.y);int known=w.nations[0].known.Count(k=>k);w.AutoExplore(scout);for(int i=0;i<40&&scout.exploring&&w.result=="";i++)w.EndTurn();
  Check(w.nations[0].known.Count(k=>k)>known*2,"auto-explore uncovers the map ("+known+" to "+w.nations[0].known.Count(k=>k)+")");
 }
 // Boats, ships and ports (owner request, save version 18).
 static void NavalChecks(){
  var w=Setup();foreach(var u in w.units.Where(a=>a.owner!=0).ToList())w.units.Remove(u);var c=w.cities.First(a=>a.owner==0);
  foreach(var t in w.tiles){if(t.x>=c.x+3)t.terrain=4;t.owner=w.Distance(t.x,t.y,c.x,c.y)<=3?0:-1;t.road=t.x==c.x&&t.y==c.y;}
  Grant(w,0,"sailing");Grant(w,0,"engineering");
  Check(w.BuildReason(c,World.UnitBuild(World.BoatUnit))!="","boats need a city beside the sea or a lake");
  Check(w.BuildReason(c,World.PortBuilding)=="","port allowed when the city's borders reach the sea");
  c.buildings.Add(World.PortBuilding);var port=w.PortTile(c);Check(port!=null&&!(port.x==c.x&&port.y==c.y)&&w.Distance(c.x,c.y,port.x,port.y)==2,"port stands on the coast of the city's territory");
  w.UpdatePortLinks();Check(!c.portLinked&&w.PortIncome(c)==0&&w.BuildReason(c,World.UnitBuild(World.GalleyUnit))!="","port without a road gives nothing and cannot launch ships");
  var mid=w.At((c.x+port.x)/2,(c.y+port.y)/2);mid.road=true;w.UpdatePortLinks();Check(c.portLinked&&w.PortIncome(c)==2&&w.BuildReason(c,World.UnitBuild(World.GalleyUnit))=="","road-linked port pays and builds ships");
  w.met[1]=true;w.SetWar(1,true);var raider=w.Add(1,2,mid.x,mid.y);w.UpdatePortLinks();Check(!c.portLinked,"an enemy on the road cuts the port off");
  w.units.Remove(raider);w.UpdatePortLinks();Check(c.portLinked,"the link returns when the enemy leaves");
  mid.road=false;w.UpdatePortLinks();Check(!c.portLinked,"a broken road cuts the port off");mid.road=true;w.UpdatePortLinks();
  c.build=World.UnitBuild(World.GalleyUnit);c.production=0;w.Produce(c,500);var galley=w.units.FirstOrDefault(a=>a.owner==0&&a.type==World.GalleyUnit);
  Check(galley!=null&&w.At(galley.x,galley.y).terrain==4&&w.Distance(galley.x,galley.y,port.x,port.y)==1,"galley is launched beside the port");
  var soldier=w.Add(0,2,port.x,port.y);var dry=w.Add(0,2,port.x,port.y);var open=w.tiles.First(t=>t.terrain==4&&w.Distance(t.x,t.y,port.x,port.y)==1&&!(t.x==galley.x&&t.y==galley.y));
  Check(!w.Move(dry,open.x,open.y),"land units cannot walk into the sea");
  Check(w.Move(soldier,galley.x,galley.y)&&w.Cargo(galley).Contains(soldier),"a land unit boards the galley");
  var next=w.tiles.First(t=>t.terrain==4&&w.Distance(t.x,t.y,galley.x,galley.y)==1&&t.x>galley.x);galley.moves=8;Check(w.Move(galley,next.x,next.y)&&soldier.x==galley.x&&soldier.y==galley.y,"the galley carries its cargo");
  var enemyShip=w.Add(1,World.WarGalleyUnit,next.x+1,next.y);galley.hp=1;enemyShip.moves=8;Check(w.Move(enemyShip,galley.x,galley.y)&&!w.units.Contains(galley)&&!w.units.Contains(soldier),"a sunk galley takes its cargo down with it");
  var land=w.Add(0,2,c.x,c.y);Check(!w.Passable(land,w.At(next.x,next.y),4),"open sea is impassable to land units without a ship");
  w.At(c.x-1,c.y).terrain=6;Check(w.BuildReason(c,World.UnitBuild(World.BoatUnit))=="","a lakeside city can build boats");
  Check(w.Valid(),"naval state validates");
 }
 // Stage 8 (AI and balance): flat science buildings, research pacts once per civilisation, the research pace, and a
 // whole standard game in which no rival wins before turn 200 (design target: earliest Transcendence around 230).
 static void StageEightChecks(){
  var w=Setup();var c=w.cities.First(a=>a.owner==0);c.pop=6;int before=w.Yield(c)[2];c.buildings.Add(38);Check(w.Yield(c)[2]==before+3,"University adds a flat +3 science");
  var second=w.Found(w.Add(0,0,c.x+6,c.y));second.pop=4;w.met[1]=true;w.Relation(0,1).researchUntil=w.turn+10;
  int pact=w.DiplomaticScience(0);Check(pact==2,"one research pact gives 2 science");
  int capitalScience=w.Yield(c)[2],otherScience=w.Yield(second)[2];w.Relation(0,1).researchUntil=0;
  Check(w.Yield(c)[2]==capitalScience-2&&w.Yield(second)[2]==otherScience,"a research pact adds its science once, at the capital");
  int first=TechnologyCatalog.Index("agriculture"),late=TechnologyCatalog.Index("fusion");w.speed=1;
  Check(w.ResearchCost(first,-1)==World.TechCosts[first]*World.ResearchPace[0]/100&&w.ResearchCost(late,-1)==World.TechCosts[late]*World.ResearchPace[TechnologyCatalog.Eras[late]]/100,"research cost is the design cost times the era's pace");
  var g=World.New(701,"Pace",0,1,1,0,1,1,4);g.Found(g.units.First(u=>u.owner==0&&u.type==0));var eraOne=new int[g.civCount];
  for(int t=0;t<200&&g.result=="";t++){g.EndTurn();for(int o=1;o<g.civCount;o++)if(eraOne[o]==0&&g.Era(o)>=1)eraOne[o]=g.turn;}
  Check(g.result=="","no rival wins before turn 200 in a standard Small game ("+g.result+")");
  Check(Enumerable.Range(1,g.civCount-1).All(o=>eraOne[o]>=25&&eraOne[o]<=60),"rivals complete the first era between turns 25 and 60 ("+string.Join(", ",Enumerable.Range(1,g.civCount-1).Select(o=>eraOne[o]))+")");
 }
 // Rivals use the sea: they link ports by road, hunt enemy ships at war, and ferry stranded settlers to other land.
 static World Islands(out City rival,out int bx){
  var w=Setup();var rc=w.cities.First(a=>a.owner==1);rival=rc;var mine=w.cities.First(a=>a.owner==0);
  foreach(var u in w.units.ToList())if(u.owner!=0||w.Distance(u.x,u.y,mine.x,mine.y)>1)w.units.Remove(u);
  foreach(var c in w.cities.Where(a=>a.owner!=0&&a!=rc).ToList())w.cities.Remove(c);
  int dir=mine.x>rc.x?-1:1;bx=rc.x+dir*10;int bxx=bx;
  foreach(var t in w.tiles){bool islandA=w.Distance(t.x,t.y,rc.x,rc.y)<=1,islandB=Math.Abs(w.DeltaX(t.x,bxx))<=2&&Math.Abs(t.y-rc.y)<=2,home=w.Distance(t.x,t.y,mine.x,mine.y)<=1;
   t.terrain=islandA||islandB||home?0:4;t.road=t.x==rc.x&&t.y==rc.y||t.x==mine.x&&t.y==mine.y;t.village=false;t.resource=0;}
  for(int o=0;o<w.civCount;o++)for(int i=0;i<w.tiles.Length;i++)w.nations[o].known[i]=true;
  Grant(w,1,"sailing");Grant(w,1,"engineering");w.RebuildTerritory();return w;
 }
 static void NavalAIChecks(){
  // Ferry: a settler on a small island with no room is carried to the other island and founds a city there.
  City rc;int bx;var w=Islands(out rc,out bx);
  rc.buildings.Add(World.PortBuilding);w.UpdatePortLinks();Check(rc.portLinked,"coastal rival city has a working port");
  var landing=w.tiles.First(t=>t.terrain==0&&w.Distance(t.x,t.y,rc.x,rc.y)==1);var settler=w.Add(1,0,landing.x,landing.y);
  var berth=w.tiles.First(t=>t.terrain==4&&w.Distance(t.x,t.y,landing.x,landing.y)==1);w.Add(1,World.GalleyUnit,berth.x,berth.y);
  int before=w.cities.Count(a=>a.owner==1);
  for(int turn=0;turn<14&&w.cities.Count(a=>a.owner==1)==before&&w.result=="";turn++)w.EndTurn();
  Check(w.cities.Count(a=>a.owner==1)==before+1&&w.cities.Any(a=>a.owner==1&&Math.Abs(w.DeltaX(a.x,bx))<=2),"a rival galley ferries its settler to the other island, which founds a city ("+string.Join("; ",w.units.Where(a=>a.owner==1).Select(a=>World.UnitNames[a.type]+"@"+a.x+","+a.y+" t"+w.At(a.x,a.y).terrain+(a.hasOrder?" ->"+a.destinationX+","+a.destinationY:"")))+" | rc "+rc.x+","+rc.y+" bx "+bx+" settler alive "+w.units.Contains(settler)+")");
  // War: a rival War Galley attacks an enemy ship it can see.
  w=Islands(out rc,out bx);rc.buildings.Add(World.PortBuilding);w.UpdatePortLinks();
  var sea=w.tiles.First(t=>t.terrain==4&&w.Distance(t.x,t.y,rc.x,rc.y)==2);var guard=w.Add(1,World.WarGalleyUnit,sea.x,sea.y);
  var near=w.tiles.First(t=>t.terrain==4&&w.Distance(t.x,t.y,sea.x,sea.y)==2&&w.Distance(t.x,t.y,rc.x,rc.y)>=2);var boat=w.Add(0,World.BoatUnit,near.x,near.y);
  w.met[1]=true;w.SetWar(1,true);w.EndTurn();Check(!w.units.Contains(boat)||boat.hp<10,"a rival war galley attacks an enemy boat at war");
  // Invasion: at war with a civilisation overseas, a rival builds a galley, embarks spare troops and lands them by the enemy city.
  w=Islands(out rc,out bx);rc.buildings.Add(World.PortBuilding);w.UpdatePortLinks();w.met[1]=true;w.SetWar(1,true);
  var mineCity=w.cities.First(a=>a.owner==0);Check(w.InvasionTarget(1)==mineCity,"the overseas enemy city is the invasion target");
  Check(w.AINavalBuild(rc)==World.UnitBuild(World.GalleyUnit),"a rival at war overseas builds an invasion galley");
  w.Add(1,2,rc.x,rc.y);var islandTiles=w.tiles.Where(t=>t.terrain==0&&w.Distance(t.x,t.y,rc.x,rc.y)==1).ToList();
  for(int k=0;k<3;k++)w.Add(1,k==0?7:2,islandTiles[k].x,islandTiles[k].y);
  var dock=w.tiles.First(t=>t.terrain==4&&w.Distance(t.x,t.y,rc.x,rc.y)==2);w.Add(1,World.GalleyUnit,dock.x,dock.y);
  bool landed=false;for(int turn=0;turn<16&&!landed&&w.result=="";turn++){w.EndTurn();landed=w.units.Any(a=>a.owner==1&&!World.IsNaval(a.type)&&w.Distance(a.x,a.y,mineCity.x,mineCity.y)<=1&&w.At(a.x,a.y).terrain==0)||mineCity.owner==1;}
  Check(landed,"rival troops are carried across the sea and land beside the enemy city");
  // Ports: a rival port away from the city gets its road from a rival worker.
  w=Setup();rc=w.cities.First(a=>a.owner==1);foreach(var u in w.units.Where(a=>a.owner==1).ToList())w.units.Remove(u);
  foreach(var t in w.tiles){if(t.x>=rc.x+3&&Math.Abs(t.y-rc.y)<=6)t.terrain=4;if(w.Distance(t.x,t.y,rc.x,rc.y)<=3)t.owner=1;t.road=t.x==rc.x&&t.y==rc.y;}
  Grant(w,1,"sailing");Grant(w,1,"engineering");Grant(w,1,"wheel");rc.buildings.Add(World.PortBuilding);w.UpdatePortLinks();Check(!rc.portLinked,"new rival port starts unlinked");
  w.Add(1,1,rc.x,rc.y);for(int turn=0;turn<16&&!rc.portLinked;turn++){foreach(var t in w.tiles)if(w.Distance(t.x,t.y,rc.x,rc.y)<=3&&!World.IsImpassable(t.terrain))t.owner=1;w.EndTurn();}
  Check(rc.portLinked,"a rival worker builds the road that links its port (port "+w.PortTile(rc).x+","+w.PortTile(rc).y+" city "+rc.x+","+rc.y+"; workers "+string.Join(" ",w.units.Where(a=>a.owner==1&&a.type==1).Select(a=>a.x+","+a.y+" job"+a.job+(a.hasOrder?" ->"+a.destinationX+","+a.destinationY:"")))+")");
 }
 static void CivilisationChecks(){
  int n=CivilisationCatalog.Count;Check(n==17&&CivilisationCatalog.Names.Distinct().Count()==n&&CivilisationCatalog.Portraits.Distinct().Count()==n&&CivilisationCatalog.Portraits.All(p=>p>=0&&p<CivilisationCatalog.PortraitCount),"17 civilisations with their own names and portraits");
  Check(CivilisationCatalog.Traits.All(t=>t.Length==2&&t[0]!=t[1])&&CivilisationCatalog.Traits.Select(t=>Math.Min(t[0],t[1])*100+Math.Max(t[0],t[1])).Distinct().Count()==n,"two different traits each, no two civilisations alike");
  for(int t=0;t<CivilisationCatalog.TraitCount;t++)Check(CivilisationCatalog.Traits.Count(p=>p.Contains(t))>=2,"trait "+CivilisationCatalog.TraitNames[t]+" belongs to at least two civilisations");
  // Crystal Dominion (2) and Sylvan Court (6) are set aside; Jade Dynasty (15) and Serpent League (16) wait for portraits.
  var offered=CivilisationCatalog.Offered().ToList();Check(offered.Count==13&&!offered.Contains(2)&&!offered.Contains(6)&&!offered.Contains(15)&&!offered.Contains(16),"13 civilisations offered; set-aside and art-less ones hidden");
  for(int s=0;s<4;s++){var many=World.New(200+s,"",0,1,1,0,3,1,8,0);Check(Enumerable.Range(1,many.civCount-1).All(o=>offered.Contains(many.nations[o].civ))&&many.Valid(),"rivals only come from offered civilisations, seed "+s);}
  Check(!World.New(93).SetPlayerCivilisation(2,"","",0,-1,-1),"a set-aside civilisation cannot be chosen");
  CivilisationCatalog.Enabled[15]=true;Check(CivilisationCatalog.Offered().Contains(15),"art arriving enables a waiting civilisation");CivilisationCatalog.Enabled[15]=false;
  // First contact queues the diplomacy screen, and the other leader greets according to relations.
  var contact=World.New(94);for(int o=1;o<contact.civCount;o++)contact.met[o]=false;contact.newContacts.Clear();var mine=contact.units.First(u=>u.owner==0);contact.Add(1,2,mine.x+1,mine.y);contact.Reveal();
  Check(contact.met[1]&&contact.newContacts.Contains(1),"first contact queues diplomacy");string hello=contact.Greeting(1);contact.SetWar(1,true);Check(hello!=""&&contact.Greeting(1)!=""&&contact.Greeting(1)!=hello,"greeting changes with war");
  // Premade choice: the player takes it, rivals take others, no duplicates, and their cities carry their names.
  var w=World.New(90,"",0,1,1,0,1,0,6,4);
  Check(w.civilisationName=="Sahel Empire"&&w.nations[0].civ==4&&w.HasTrait(0,World.TraitMonumental)&&w.HasTrait(0,World.TraitDiplomatic),"premade civilisation chosen with its traits");
  var rivals=Enumerable.Range(1,w.civCount-1).Select(o=>w.nations[o].civ).ToList();Check(rivals.All(c=>c>=0&&c!=4)&&rivals.Distinct().Count()==rivals.Count,"rivals are other premade civilisations, no repeats");
  Check(w.cities.Select(c=>c.name.ToLowerInvariant()).Distinct().Count()==w.cities.Count&&w.CityNameProblem(w.cities[0].name)!=""&&w.CityNameProblem(" ")!="","every city has its own name; repeats and blanks are refused");
  Check(w.ConcordTargetFor(0)==15,"Diplomatic: Concord in 15 turns");
  // Custom civilisation: own name, leader, portrait and two different traits.
  w=World.New(91);Check(!w.SetPlayerCivilisation(-1,"Ember Clans","Chief Oru",13,3,3),"a custom civilisation needs two different traits");
  Check(w.SetPlayerCivilisation(-1,"Ember Clans","Chief Oru",13,2,8)&&w.civilisationName=="Ember Clans"&&w.nations[0].leader=="Chief Oru"&&w.nations[0].portrait==13&&w.HasTrait(0,World.TraitAgrarian)&&w.HasTrait(0,World.TraitIndustrious)&&w.Valid(),"custom civilisation with chosen traits");
  // Every trait's rule.
  w=Setup();var c=w.cities.First(a=>a.owner==0);c.pop=2;w.nations[0].traits=new[]{-1,-1};int[] plain=w.Yield(c);int slots=w.ContentSlots(c);
  w.nations[0].traits=new[]{World.TraitAgrarian,World.TraitIndustrious};var y=w.Yield(c);Check(y[0]==plain[0]+1&&y[1]==plain[1]+1,"Agrarian +1 food, Industrious +1 production");
  w.nations[0].traits=new[]{World.TraitScholarly,World.TraitMercantile};y=w.Yield(c);Check(y[2]==plain[2]+1&&y[3]==plain[3]+2,"Scholarly +1 science, Mercantile +2 gold");
  w.nations[0].traits=new[]{World.TraitSpiritual,-1};Check(w.ContentSlots(c)==slots+1,"Spiritual +1 content citizen");
  var sea=w.Workable(c)[0];sea.terrain=4;w.nations[0].traits=new[]{-1,-1};int food=w.EffectiveTileYield(c,sea)[0];w.nations[0].traits=new[]{World.TraitSeafaring,-1};Check(w.EffectiveTileYield(c,sea)[0]==food+1,"Seafaring +1 food on water");
  w.speed=1;w.nations[0].traits=new[]{World.TraitMonumental,World.TraitExpansionist};Check(w.CostFor(c,60)==w.Cost(60)-w.Cost(60)/5&&w.CostFor(c,70)==240&&w.CostFor(c,0)==20&&w.CostFor(c,2)==w.Cost(2),"Monumental: wonders and stages a fifth less; Expansionist: Settlers a third less");
  w.met[1]=true;w.SetWar(1,true);var guard=w.Add(0,7,c.x+1,c.y);w.At(c.x+1,c.y).owner=0;var rider=w.Add(1,8,c.x+2,c.y);
  w.nations[0].traits=new[]{-1,-1};int d=w.Strength(guard,rider,true);w.nations[0].traits=new[]{World.TraitStalwart,-1};Check(w.Strength(guard,rider,true)==d+1,"Stalwart +1 defence inside your borders");
  var horse=w.Add(0,8,c.x+3,c.y+1);w.nations[0].traits=new[]{-1,-1};int a=w.Strength(horse,guard,false);w.nations[0].traits=new[]{World.TraitHorseLords,-1};Check(w.Strength(horse,guard,false)==a+1,"Horse Lords +1 attack for mounted units");
  w.nations[0].traits=new[]{World.TraitMilitaristic,-1};Grant(w,0,"toolmaking");c.buildings.Remove(32);w.ChooseProject(c,2,false);w.Produce(c,w.CostFor(c,2));var made=w.units.Last(u=>u.owner==0&&u.type==2);Check(made.promotion==1,"Militaristic: new military units start as Veterans");
  // Version 15 saves: the old trait becomes a matching new trait; rivals keep their names.
  w=World.New(92);string rivalName=w.NationName(1);w.version=15;w.nations[0].trait=2;foreach(var nn in w.nations){nn.traits=null;nn.civ=7;}w.UpgradeSave();
  Check(w.version==21&&w.HasTrait(0,World.TraitScholarly)&&w.nations.All(nn=>nn.civ==-1&&nn.traits!=null)&&w.NationName(1)==rivalName&&w.Valid(),"version 15 saves convert traits and keep rival names");
  // Build lists show only what known technology allows; combat units can explore.
  w=Setup();c=w.cities.First(a=>a.owner==0);Check(!w.TechAllows(c,World.UnitBuild(5))&&w.TechAllows(c,World.UnitBuild(2))&&!w.TechAllows(c,8),"build list hides what technology does not yet allow");
  Grant(w,0,"writing");Check(w.TechAllows(c,8),"a newly known technology shows its building");c.library=true;Check(!w.TechAllows(c,8),"a building already built is not listed");
  Check(World.CanExplore(2)&&World.CanExplore(3)&&!World.CanExplore(0)&&!World.CanExplore(1),"combat units and scouts can auto-explore; settlers and workers cannot");
  var warrior=w.units.First(u=>u.owner==0&&u.type==2);warrior.moves=World.Moves[2];Check(w.AutoExplore(warrior)||!warrior.exploring,"a warrior can be sent exploring");
 }
 // Stage 7: tile locks over the Governor, automatic entertainers, one-click specialists, save version 15.
 static void StageSevenCity(){
  var w=Setup();var c=w.cities.First(a=>a.owner==0);c.pop=2;c.focus=1;
  var tiles=w.Workable(c);foreach(var t in tiles){t.terrain=0;t.resource=0;t.farm=false;t.mine=false;}
  var hill=tiles[0];hill.terrain=2;hill.mine=true;var farm=tiles[1];farm.farm=true;
  Check(!w.Worked(c).Contains(hill),"the growth Governor skips a low-food mine");
  Check(w.ToggleLock(c,hill)&&w.IsLocked(c,hill)&&w.Worked(c).Contains(hill),"a locked tile is worked whatever the Governor prefers");
  Check(w.ToggleLock(c,tiles[2])&&!w.ToggleLock(c,tiles[3]),"locks are capped at the number of working citizens");
  Check(w.Worked(c).Contains(tiles[2])&&!w.Worked(c).Contains(farm),"both locks are kept even over a better farm");
  Check(w.ToggleLock(c,tiles[2])&&!w.IsLocked(c,tiles[2]),"clicking a locked tile releases it");
  Check(!w.ToggleLock(c,w.At(c.x+4,c.y)),"tiles outside the 5 x 5 ring cannot be locked");
  // Starvation swaps never move a locked citizen.
  hill.mine=false;hill.terrain=5;c.pop=1;Check(w.Worked(c).Contains(hill),"a lock survives even when it cannot feed the city");
  c.locked.Clear();c.pop=3;
  // Citizens with no tile to work become entertainers.
  foreach(var t in tiles)t.owner=-1;w.Workable(c).ForEach(t=>{});
  int before=c.entertainers;w.AutoEntertainers(c);Check(c.entertainers==before+3&&w.Workers(c)==0,"surplus citizens become entertainers automatically");
  Check(w.ConvertSpecialist(c,2,0)&&c.scientists==1&&c.entertainers==2,"one click: entertainer to scientist");
  Check(w.ConvertSpecialist(c,2,1)&&c.merchants==1&&c.entertainers==1,"one click: entertainer to tax collector");
  Check(w.ConvertSpecialist(c,0,2)&&c.scientists==0&&c.entertainers==2,"and back again");
  c.entertainers=0;c.scientists=0;c.merchants=0;Check(!w.ConvertSpecialist(c,2,0)&&c.scientists==0,"no conversion without an entertainer");
  // Version 14 saves gain empty lock lists.
  w=World.New(81);w.version=14;foreach(var city in w.cities)city.locked=null;w.UpgradeSave();Check(w.version==21&&w.cities.All(a=>a.locked!=null&&a.locked.Count==0)&&w.Valid(),"version 14 saves gain empty lock lists");
  w.cities[0].locked.Add(-3);Check(!w.Valid(),"invalid lock rejected");
 }
 // Stage 6: victory by capitals, Transcendence as a three-stage project, save version 14 (CORE-DESIGN-16BIT.md).
 static void StageSixVictory(){
  var w=World.New(77);Check(w.nations[0].capital<0&&Enumerable.Range(1,w.civCount-1).All(o=>w.Capital(o)!=null&&w.Capital(o).owner==o),"rivals' first cities are their capitals; the player has none yet");
  var home=w.Found(w.units.First(u=>u.owner==0&&u.type==0));Check(w.Capital(0)==home,"the player's first city becomes the capital");
  var second=w.Add(0,0,home.x+5,home.y);w.At(second.x,second.y).owner=-1;var other=w.Found(second);Check(other==null||w.Capital(0)==home,"later cities never replace the capital");
  // Conquest: holding every rival capital wins even while rivals survive elsewhere.
  for(int o=1;o<w.civCount;o++)w.Capital(o).owner=0;w.Add(1,2,home.x+9,home.y+2);w.CheckVictory();
  Check(w.result.StartsWith("Conquest victory")&&w.Survives(1),"holding every rival capital wins");
  w=World.New(78);home=w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.Capital(2).owner=1;home.owner=1;w.Add(0,2,home.x+3,home.y);w.CheckVictory();
  Check(w.result.Contains("Conquest defeat"),"a rival holding every other capital wins");
  w=World.New(79);w.units.RemoveAll(u=>u.owner>0);w.cities.RemoveAll(c=>c.owner>0);w.CheckVictory();Check(w.result.StartsWith("Conquest victory"),"eliminating every rival still wins");
  // Transcendence project gates.
  w=Setup();var c=w.cities.First(a=>a.owner==0);c.pop=8;w.speed=1;
  Check(World.BaseCost(70)==300&&World.BaseCost(71)==400&&World.BaseCost(72)==500&&World.BuildingUpkeep(70)==0,"stages cost 300, 400 and 500 shields, no upkeep");
  Check(w.BuildReason(c,70).Contains("Computing"),"needs Computing");Grant(w,0,"computers");
  c.pop=7;Check(w.BuildReason(c,70).Contains("size 8"),"needs a city of size 8");c.pop=8;
  Check(w.BuildReason(c,70)==""&&w.BuildReason(c,71).Contains("stage 1 first"),"stages go in order");
  Check(w.ChooseProject(c,70,false)&&!w.Rush(c,true),"projects cannot be bought with gold");
  w.Produce(c,w.Cost(70));var n=w.nations[0];
  Check(n.projectStage==1&&n.projectCity==c.id&&n.projectActivation==10&&!c.buildings.Contains(70),"stage 1 complete, activating for 10 turns");
  Check(w.BuildReason(c,71).Contains("activating"),"the next stage waits for activation");
  for(int i=0;i<10;i++)w.UpdateProjects();Check(n.projectActivation==0&&w.result=="","stage 1 activates after 10 turns");
  var c2=w.Found(w.Add(0,0,c.x+6,c.y));if(c2!=null){c2.pop=8;Check(w.BuildReason(c2,71).StartsWith("Must be built in"),"later stages stay in the same city");}
  float before=w.ProjectProgress(0);Check(before>0.3f&&before<0.34f,"one active stage is a third of the bar");
  // Losing the city loses the project.
  c.owner=1;w.UpdateProjects();c.owner=0;Check(n.projectStage==0&&n.projectCity<0,"losing the project city resets the project");
  // Three activated stages win; the Transcendence technology halves activation.
  Grant(w,0,"transcendence");Check(w.ActivationTurns(0)==5,"Transcendence technology: stages activate in 5 turns");
  foreach(int b in new[]{70,71,72}){w.ChooseProject(c,b,false);w.Produce(c,w.Cost(b));Check(n.projectStage==b-69,"stage "+(b-69)+" built");for(int i=0;i<5&&w.result=="";i++)w.UpdateProjects();}
  Check(w.result.StartsWith("Transcendence victory"),"three activated stages win");
  // A rival's project ends the game in defeat; the AI pursues it in its best city.
  w=Setup();var rc=w.cities.First(a=>a.owner==1);rc.pop=8;Grant(w,1,"computers");
  Check(w.PreferredBuilding(rc)==70,"AI starts Transcendence in its best eligible city");
  var rn=w.nations[1];rn.projectCity=rc.id;rn.projectStage=3;rn.projectActivation=1;w.UpdateProjects();Check(w.result.Contains("has achieved Transcendence"),"a rival's third stage is a defeat");
  // Version 13 saves gain capitals and empty projects.
  w=World.New(80);var keep=w.cities.First(a=>a.owner==1);w.version=13;foreach(var nn in w.nations){nn.capital=-5;nn.projectStage=2;nn.projectActivation=3;}w.UpgradeSave();
  Check(w.version==21&&w.nations[1].capital==keep.id&&w.nations.All(nn=>nn.projectStage==0&&nn.projectActivation==0)&&w.nations[0].capital==-1&&w.Valid(),"version 13 saves gain capitals and empty projects");
  float[] bars;var text=w.VictoryText(out bars);Check(text.Length==3&&bars.All(v=>v>=0&&v<=1)&&text[1].Contains("Computing"),"three victory bars");
 }
 // Stage 4: deterministic combat (CORE-DESIGN-16BIT.md).
 static World War(){var w=Setup();w.met[1]=true;w.SetWar(1,true);foreach(var t in w.tiles){t.terrain=0;t.fort=false;t.road=false;}w.units.RemoveAll(u=>u.owner==1&&w.cities.All(c=>c.x!=u.x||c.y!=u.y));return w;}
 static void StageFourCombat(){
  Check(World.Damage(1,1)==3&&World.Damage(5,1)==7&&World.Damage(1,5)==1&&World.Damage(0,9)==1,"damage = max(1, 3 + own - enemy)");
  Check(World.Attack[7]==1&&World.Defence[7]==2&&World.Attack[8]==3&&World.Defence[8]==1&&World.Attack[4]==2&&World.Defence[4]==1,"design table: Spearman 1/2, Horseman 3/1, Archer 2/1");
  Check(World.Moves[8]>World.Moves[7]&&World.IsRanged(4)&&!World.IsRanged(7),"Horseman is faster; Archer is ranged");
  Check(World.TerrainDefence(0)==0&&World.TerrainDefence(1)==1&&World.TerrainDefence(7)==1&&World.TerrainDefence(2)==2&&World.TerrainDefence(5)==3,"terrain defence: forest +1, river +1, hills +2, mountains +3");
  var w=War();var c=w.cities.First(a=>a.owner==0);int x=c.x+6,y=c.y;
  var spear=w.Add(1,7,x+1,y);var horse=w.Add(0,8,x,y);
  Check(w.Strength(spear,horse,true)==4,"Spearman defends 2 + 2 against mounted");
  w.At(x+1,y).terrain=2;Check(w.Strength(spear,horse,true)==6,"hills add 2");w.At(x+1,y).terrain=0;
  spear.fortified=true;Check(w.Strength(spear,horse,true)==5,"fortified adds 1");spear.fortified=false;
  int dealt,taken;w.Exchange(horse,spear,out dealt,out taken);Check(dealt==2&&taken==4,"Spearman beats Horseman");
  var archer=w.Add(1,4,x+1,y+1);w.Exchange(horse,archer,out dealt,out taken);Check(dealt==5&&taken==1,"Horseman beats Archer");
  var bow=w.Add(0,4,x,y+2);var spear2=w.Add(1,7,x+1,y+3);w.Exchange(bow,spear2,out dealt,out taken);Check(dealt==3&&taken==0,"Archer beats Spearman without retaliation");
  // The preview is exactly what happens.
  w.Reveal();horse.moves=World.Moves[horse.type];string preview=w.CombatPreview(horse,spear.x,spear.y);
  Check(preview.Contains("You deal 2")&&preview.Contains("you take 4"),"preview names the exact damage: "+preview);
  w.Move(horse,spear.x,spear.y);Check(spear.hp==8&&horse.hp==6,"attack resolves exactly as previewed");
  // Only the best defender fights.
  w=War();c=w.cities.First(a=>a.owner==0);x=c.x+6;y=c.y;
  var weak=w.Add(1,2,x+1,y);var strong=w.Add(1,6,x+1,y);var attacker=w.Add(0,5,x,y);w.Reveal();
  Check(w.BestDefender(attacker,x+1,y)==strong,"best defender is chosen");
  w.Move(attacker,x+1,y);Check(weak.hp==10&&strong.hp<10,"only the best defender takes damage");
  // An archer attacking in melee takes no retaliation either.
  var archer2=w.Add(0,4,x,y+2);var target=w.Add(1,2,x+1,y+2);w.Reveal();archer2.moves=World.Moves[4];w.Move(archer2,x+1,y+2);Check(archer2.hp==10&&target.hp<10,"ranged unit takes no retaliation");
  // Walls add 2 to units in the city.
  var rival=w.cities.First(a=>a.owner==1);var guard=w.units.FirstOrDefault(u=>u.owner==1&&u.x==rival.x&&u.y==rival.y)??w.Add(1,2,rival.x,rival.y);
  int open=w.Strength(guard,attacker,true);rival.walls=true;Check(w.Strength(guard,attacker,true)==open+2,"walls add 2");
  // Horsemen ignore the forest slowdown.
  var forest=w.At(x,y+4);forest.terrain=1;var rider=w.Add(0,8,x,y+5);var foot=w.Add(0,2,x+1,y+5);
  Check(w.StepCost(rider,rider.x,rider.y,forest)==2&&w.StepCost(foot,foot.x,foot.y,forest)==3,"Horseman ignores forest slowdown");
  // Stack limit of 3 military units.
  var s0=w.Add(0,2,x+3,y+6);w.Add(0,2,x+3,y+6);w.Add(0,2,x+3,y+6);var fourth=w.Add(0,2,x+2,y+6);fourth.moves=World.Moves[2];
  Check(!w.Move(fourth,x+3,y+6),"stack limit 3");
  Check(w.Valid(),"combat state stays valid");
 }
}
}
