using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Checks for gateways, dimensional rules, colonies, local research, rivals and accords (Design/DIMENSIONS-PLAN-2026-11-11.md).
// The scenario at the end is the owner's: discover a gateway, divert a city's production to investigate it, explore beyond,
// found a colony, make a local discovery, export to a home city, have a rival compete, and interrupt the gateway without
// losing a unit or a colony. It is run with an early civilisation and an advanced one.
public static class DimensionChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Dimensions: "+what);}
 static World Fresh(){var w=World.New(306,"Keys",0,1,1,0,1,2,4,hex:true,rations:true);w.Found(w.units.First(u=>u.owner==0&&u.type==0));var other=w.units.FirstOrDefault(u=>u.owner==1&&u.type==0);if(other!=null)w.Found(other);w.EnableDimensions();return w;}
 static void Learn(World w,int owner,params string[] ids){foreach(string id in ids){int k=TechnologyCatalog.Index(id);if(k>=0)w.nations[owner].tech[k]=true;}if(owner==0)w.tech=w.nations[0].tech;}
 static void Rich(World w,int owner){w.nations[owner].gold=2000;if(owner==0)w.gold=2000;w.nations[owner].faith=500;w.nations[owner].influence=300;}
 static Tile Gate(World w,int condition,int family=-1){return w.HomeGates().FirstOrDefault(g=>w.ConditionOf(g)==condition&&(family<0||DimensionFamily.OfRealm(w.GateRealm(g))==family));}
 // A land tile beside a tile, made passable grassland for the test.
 static Tile Beside(World w,Tile t){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=w.Step(t.x,t.y,dx,dy);if(n==null||n==t||n.realm!=t.realm||n.gateLink>0||w.cities.Any(c=>c.x==n.x&&c.y==n.y))continue;n.terrain=0;n.resource=0;n.ruin=false;n.village=false;return n;}return null;}
 static City MoveCityNear(World w,Tile gate,int owner=0){var c=w.cities.First(x=>x.owner==owner);var spot=w.tiles.Where(t=>t.realm==gate.realm&&t.gateLink==0&&w.Distance(t.x,t.y,gate.x,gate.y)>=2&&w.Distance(t.x,t.y,gate.x,gate.y)<=3&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&!w.cities.Any(o=>o!=c&&o.x==t.x&&o.y==t.y)).OrderBy(t=>t.y*w.Width+t.x).First();c.x=spot.x;c.y=spot.y;w.RebuildTerritory();if(owner==0){if(w.gatesFound==null)w.gatesFound=new List<int>();int cell=gate.y*w.Width+gate.x;if(!w.gatesFound.Contains(cell))w.gatesFound.Add(cell);}else w.nations[owner].known[gate.y*w.Width+gate.x]=true;return c;}
 public static void Run()
 {
  Foundation();Cycling();Investigation();Compatibility();Colonies();Research();Accords();
  var early=Scenario(false);var late=Scenario(true);
  Check(late[0]<early[0],"an advanced civilisation investigates with less production ("+early[0]+" against "+late[0]+")");
  Check(late[1]>early[1],"and its gateways carry more ("+early[1]+" against "+late[1]+" loads)");
  Check(early[2]>0&&late[2]>=early[2],"both export to a home city ("+early[2]+" and "+late[2]+" production)");
  LongGame();
  Console.WriteLine("PASS Dimensions (gateway conditions, investigation, rules, colonies, research, accords, the full scenario early and late)");
 }
 static void Foundation()
 {
  var w=Fresh();var gates=w.HomeGates();Check(gates.Count>=5&&gates.All(g=>w.StateOf(g)!=null)&&w.gateStates.Count==gates.Count,"every gateway has a condition ("+gates.Count+")");
  Check(Gate(w,GateCondition.Cycling)!=null&&Gate(w,GateCondition.Dormant)!=null&&Gate(w,GateCondition.Damaged)!=null&&Gate(w,GateCondition.Sealed)!=null,"a new game has cycling, dormant, damaged and sealed gateways");
  Check(DimensionFamily.OfRealm(w.GateRealm(Gate(w,GateCondition.Cycling)))==DimensionFamily.Ember&&Gate(w,GateCondition.Dormant,DimensionFamily.Resonant)!=null&&Gate(w,GateCondition.Dormant,DimensionFamily.Ember)!=null&&gates.Where(g=>DimensionFamily.OfRealm(w.GateRealm(g))==DimensionFamily.Home).All(g=>w.ConditionOf(g)==GateCondition.Sealed),"the cycling gateway leads to an Ember world, each family has a dormant one, and worlds with home physics stay sealed");
  Check(w.version==World.SaveVersion&&w.Valid(),"a new game is valid at save version "+World.SaveVersion);
  // An older save: no gateway data at all. Everything is sealed, as it was.
  var old=World.New(307,natives:false);old.version=41;old.gateStates=null;old.colonies=null;old.adaptations=null;old.accords=null;old.gatePriority=null;old.UpgradeSave();
  Check(old.version==World.SaveVersion&&old.Valid()&&old.HomeGates().All(g=>old.ConditionOf(g)==GateCondition.Sealed)&&!old.dimensionRules&&old.colonies.Count==0,"a version 41 save upgrades with every gateway sealed and no colonies");
  old.EndTurn();Check(old.Valid(),"and still plays a turn");
 }
 static void Cycling()
 {
  var w=Fresh();var g=Gate(w,GateCondition.Cycling);var s=w.StateOf(g);int open=0;for(int k=0;k<s.period;k++){if(w.GateOpen(g))open++;w.turn++;}
  Check(open==w.CycleOpenTurns(s)&&open>0&&open<s.period,"a cycling gateway is open "+open+" turns in every "+s.period);
  while(w.GateOpen(g))w.turn++;Check(w.GateSchedule(g).StartsWith("Shut now")&&!w.GateSteady(g),"its schedule can be read: "+w.GateSchedule(g));
  var side=Beside(w,g);var scout=w.Add(0,3,side.x,side.y);scout.moves=9;w.Move(scout,g.x,g.y);
  Check(w.units.Contains(scout)&&scout.x==g.x&&scout.y==g.y,"a unit that reaches a shut gateway waits on it; it is not lost");
  while(!w.GateOpen(g))w.turn++;Check(w.GateSchedule(g).StartsWith("Open now"),"and when its time comes it is open");
  w.TravelForCheck(scout);Check(w.units.Contains(scout)&&w.At(scout.x,scout.y).realm==w.GateRealm(g),"an early civilisation can use a cycling gateway without investigating or building anything");
  while(w.GateOpen(g))w.turn++;Check(w.units.Contains(scout)&&w.At(scout.x,scout.y).realm>0,"a unit beyond a gateway that shuts stays where it is");
 }
 static void Investigation()
 {
  var w=Fresh();var g=Gate(w,GateCondition.Dormant,DimensionFamily.Resonant);var c=MoveCityNear(w,g);
  Check(w.BuildReason(c,World.InvestigateProject)==""&&w.TechAllows(c,World.InvestigateProject),"a city within reach can investigate a gateway its people have found ("+w.BuildReason(c,World.InvestigateProject)+")");
  Check(w.BuildReason(c,World.RepairProject)!="","there is nothing to repair there");
  int plain=w.CostFor(c,World.InvestigateProject);Check(w.InvestigationMethod(0)==0&&w.BuildingGold(World.InvestigateProject)==0,"an early people investigates by observation and trial, with production alone");
  Learn(w,0,"writing");int scholarly=w.CostFor(c,World.InvestigateProject);Learn(w,0,"printing");int instruments=w.CostFor(c,World.InvestigateProject);
  Check(scholarly<plain&&instruments<scholarly&&w.InvestigationMethod(0)==2,"knowledge makes the same work lighter ("+plain+", "+scholarly+", "+instruments+")");
  Check(w.ApproachReason(0,g,GateApproach.Stabilise).Contains("not been read"),"an unread gateway cannot be worked");
  Check(w.ChooseProject(c,World.InvestigateProject,false)&&c.buildingProject==World.InvestigateProject,"the project goes on the city's ordinary building line");
  c.buildingProduction=w.CostFor(c,World.InvestigateProject);w.CompleteBuildingForCheck(c);
  var study=w.StudyOf(0,g);Check(study!=null&&study.known&&study.method==2&&c.buildingProject!=World.InvestigateProject&&!w.HasBuilding(c,World.InvestigateProject),"finishing it reads the seal, and leaves no building behind");
  Check(w.BuildReason(c,World.InvestigateProject)!=""&&w.gateNotices!=null&&w.gateNotices.Count==1,"it is not offered again, and the player is told once");
  Check(w.DangerText(g,0).Contains("measured")&&w.DangerText(g).Contains(" to "),"instruments measure the danger exactly; without them it is a range");
  var report=w.CompatibilityReport(0,g);Check(report.Any(l=>l.StartsWith("Confirmed"))&&report.Any(l=>l.Contains("Resonant")),"the investigation reports the far side's rules");
  // Postponing costs nothing and says nothing.
  Rich(w,0);for(int k=0;k<4;k++)w.GateExpeditionTurn();Check(w.StudyOf(0,g).known&&!w.GateSteady(g),"a read gateway can be left alone for as long as the player likes");
  // Dormant: only a small activation, no seal's demand.
  var need=w.GateNeed(g,0);Check(need.gold>0&&need.gold<=DimensionSettings.ActivateGold&&need.faith==0&&need.techs.Count==0&&need.resource==0,"a dormant gateway asks only a small activation ("+need.gold+" gold)");
  w.Add(0,2,g.x,g.y);Check(w.BeginApproach(0,g,GateApproach.Stabilise),"it can be woken");w.turn+=w.StabiliseTurnsNow+6;w.GateExpeditionTurn();Check(w.GateSteady(g),"and stays open");
  // Damaged: repair first, with metal.
  var w2=Fresh();var broken=Gate(w2,GateCondition.Damaged);var c2=MoveCityNear(w2,broken);w2.StudyOfForCheck(0,broken).known=true;
  Check(w2.ApproachReason(0,broken,GateApproach.Stabilise).Contains("damaged"),"a damaged gateway must be repaired first");
  w2.nations[0].tech[TechnologyCatalog.Index("bronze")]=false;w2.tech=w2.nations[0].tech;Check(w2.BuildReason(c2,World.RepairProject).Contains("Bronze"),"repairs need Bronze Working");Learn(w2,0,"bronze");
  Check(w2.BuildReason(c2,World.RepairProject)==""&&w2.ChooseProject(c2,World.RepairProject,false),"a city within reach can repair it");c2.buildingProduction=w2.CostFor(c2,World.RepairProject);w2.CompleteBuildingForCheck(c2);
  Check(w2.ConditionOf(broken)==GateCondition.Dormant&&w2.StateOf(broken).repair==0&&w2.ApproachReason(0,broken,GateApproach.Stabilise)!="This gateway is damaged","once repaired it is dormant");
  Check(w.Valid()&&w2.Valid(),"valid with investigations recorded");
 }
 static void Compatibility()
 {
  var w=Fresh();var ember=Gate(w,GateCondition.Cycling);var far=w.GateTwin(ember);var ground=Beside(w,far);var home=Beside(w,ember);
  var foe=w.Add(1,2,ground.x,ground.y);var rifle=w.Add(0,6,home.x,home.y);var club=w.Add(0,2,home.x,home.y);
  int rifleHome=w.Strength(rifle,foe,false),rifleHomeDef=w.Strength(rifle,foe,true),clubHome=w.Strength(club,foe,false);
  var there=Beside(w,far);rifle.x=there.x;rifle.y=there.y;club.x=there.x;club.y=there.y;
  Check(rifleHome-w.Strength(rifle,foe,false)==World.Attack[6]-CapabilityCatalog.FallbackAttack,"a Rifleman's firearm fails in an Ember world: attack "+rifleHome+" to "+w.Strength(rifle,foe,false));
  Check(w.Strength(club,foe,false)==clubHome,"a Warrior is unaffected");
  Check(w.DimensionStrength(rifle,true)==0&&w.DimensionNote(rifle).Contains("fail"),"only the weapon is lost: the Rifleman still defends, and the unit says why");
  var report=w.CompatibilityReport(0,ember);Check(report.Count==1&&report[0].StartsWith("Unknown"),"before investigation the far side is listed as unknown, not as safe");
  w.StudyOfForCheck(0,ember).known=true;report=w.CompatibilityReport(0,ember);
  Check(report.Any(l=>l.Contains("Riflemen lose"))&&report.Any(l=>l.Contains("hazard"))&&report.Any(l=>l.Contains("Knowledge is not affected")),"after it, equipment failures and survival hazards are confirmed");
  // The same rule for a rival.
  var theirs=w.Add(1,6,ground.x,ground.y);Check(w.DimensionStrength(theirs,false)==w.DimensionStrength(rifle,false),"the rule is the same for a rival's Rifleman");
  w.AdaptationOf(0,DimensionFamily.Ember,true).known.Add(AdaptationCatalog.PressureArms);Check(w.DimensionStrength(rifle,false)==0&&w.DimensionStrength(theirs,false)<0,"Pressure Arms give the firearm back to those who learned it, and only to them");
  // A Resonant world leaves powder alone.
  var res=w.GateTwin(Gate(w,GateCondition.Dormant,DimensionFamily.Resonant));var rt=Beside(w,res);rifle.x=rt.x;rifle.y=rt.y;Check(w.DimensionStrength(rifle,false)==0,"combustion works in a Resonant world");
 }
 // A colony in the Ember world behind the cycling gateway, which is steadied so supply can be watched turn by turn.
 static Colony Planted(World w,out Tile gate,out City home)
 {
  gate=Gate(w,GateCondition.Cycling);home=MoveCityNear(w,gate);w.OpenGateForCheck(gate);var far=w.GateTwin(gate);var site=w.ColonySites(0,far.realm).First();
  var settler=w.Add(0,0,site.x,site.y);settler.moves=4;Check(w.ColonyGround(settler)&&w.ColonyReason(settler)=="","a Settler beyond a gateway can found a colony ("+w.ColonyReason(settler)+")");
  int cities=w.cities.Count;var made=w.Found(settler);var c=w.ColonyAt(site.x,site.y);Check(made==null&&c!=null&&w.cities.Count==cities&&!w.units.Contains(settler),"it founds a colony, not a city");return c;
 }
 static void Colonies()
 {
  var w=Fresh();Tile gate;City home;var c=Planted(w,out gate,out home);Rich(w,0);
  Check(c.level==1&&c.specialisation==ColonyCatalog.Extraction&&w.ColonyHome(c)==home&&c.gate>=0,"it starts as an extraction colony tied to the nearest home city and a gateway");
  int before=w.Yield(home)[1];w.DimensionsTurnForCheck(0);
  Check(c.shipShields>0&&w.Yield(home)[1]==before+c.shipShields&&c.isolated==0,"its exports add production in that home city (+"+c.shipShields+")");
  Check(w.Yield(w.cities.First(x=>x.owner==1))[1]>=0&&w.cities.Where(x=>x.owner==0&&x!=home).All(x=>true),"and only there");
  // A foothold: a Scout plants one; it grows once supplies reach it.
  var far=w.GateTwin(gate);var spot=w.ColonySites(0,far.realm).First();var scout=w.Add(0,3,spot.x,spot.y);scout.moves=4;var foothold=w.FoundColony(scout);
  Check(foothold!=null&&foothold.level==0,"a Scout plants a foothold");int ore,goods;w.ColonyOutput(foothold,out ore,out goods);Check(ore+goods==0,"a foothold exports nothing");
  for(int k=0;k<4&&foothold.level==0;k++)w.DimensionsTurnForCheck(0);Check(foothold.level==1,"supplies from home grow it into a colony");
  // Capacity: with room for one load, the priority decides what moves.
  w.StateOf(gate).capacity=1;foreach(string id in new[]{"engineering","industrialisation","computers"})w.nations[0].tech[TechnologyCatalog.Index(id)]=false;w.tech=w.nations[0].tech;
  Check(w.GateCapacityFor(0,gate)==1,"a narrow gateway carries one load");c.stock=0;foothold.stock=0;c.selfSufficiency=0;foothold.selfSufficiency=0;
  w.SetGatePriority(0,ColonyCatalog.PrioritySupply);w.DimensionsTurnForCheck(0);int shipSupply=c.shipShields+foothold.shipShields,sentSupply=c.supplied+foothold.supplied;
  c.stock=0;foothold.stock=0;w.SetGatePriority(0,ColonyCatalog.PriorityTrade);w.DimensionsTurnForCheck(0);
  Check(sentSupply==1&&shipSupply==0,"Supply first: the one load carries supplies ("+sentSupply+" supply, "+shipSupply+" export)");
  Check(c.supplied+foothold.supplied==1&&w.PriorityPreview(0,gate,ColonyCatalog.PriorityTrade).Contains("load"),"Trade first still sends a short colony one load before exports, and the trade-off is shown");
  // Units share the same gateway: when it has carried its loads, the next unit waits and is not lost.
  var wait=Beside(w,gate);var a=w.Add(0,2,wait.x,wait.y);var b=w.Add(0,2,wait.x,wait.y);var third=w.Add(0,2,wait.x,wait.y);foreach(var u in new[]{a,b,third}){u.x=gate.x;u.y=gate.y;w.TravelForCheck(u);}
  Check(w.At(a.x,a.y).realm>0&&w.At(b.x,b.y).realm>0&&w.At(third.x,third.y).realm==0&&w.units.Contains(third),"two units cross a narrow gateway in a turn; the third waits on it");
  // Interruption: the gateway is taken. Supply stops; nothing is deleted and nothing changes hands.
  w.StateOf(gate).capacity=DimensionSettings.GateCapacity;w.SetGatePriority(0,ColonyCatalog.PrioritySupply);c.stock=12;w.DimensionsTurnForCheck(0);int unitsBeyond=w.units.Count(u=>u.owner==0&&w.At(u.x,u.y).realm>0);
  foreach(var u in w.units.Where(u=>u.x==gate.x&&u.y==gate.y).ToList())w.units.Remove(u);w.met[1]=true;if(!w.AtWar(0,1))w.DeclareWar(0,1);var holder=w.Add(1,2,gate.x,gate.y);
  Check(!w.GateUsable(0,gate)&&w.GateBlock(0,gate).Contains(w.NationName(1)),"an enemy standing on the gateway cuts it: "+w.GateBlock(0,gate));
  int stock=c.stock;w.DimensionsTurnForCheck(0);
  Check(c.isolated==1&&c.shipShields==0&&c.stock<stock&&c.owner==0&&w.colonies.Contains(c)&&w.units.Count(u=>u.owner==0&&w.At(u.x,u.y).realm>0)==unitsBeyond,"the colony is cut off: exports stop, it lives on its stock, it stays yours and no unit vanishes");
  Check(w.DimensionProblems(0).Any(p=>p.Contains(c.name)&&p.Contains("cut off")),"the overview names the problem");
  for(int k=0;k<12;k++)w.DimensionsTurnForCheck(0);Check(w.colonies.Contains(c)&&c.owner==0&&c.stock==0&&c.level>=1,"a long isolation empties its stores and does not destroy it");
  c.selfSufficiency=100;Check(w.SupplyNeed(c)==0,"an established, self-sufficient colony needs nothing from home");
  w.units.Remove(holder);w.DimensionsTurnForCheck(0);Check(c.isolated==0&&c.shipShields>0,"when the gateway is free again the exports resume");
  // Buildings, policy, second specialisation.
  Check(w.ColonyBuild(c,ColonyCatalog.Storehouse)&&w.ColonyStorage(c)==ColonyCatalog.BaseStorage+20&&w.ColonyBuildReason(c,ColonyCatalog.FamilyWorkshop).Contains("Thermal Forging"),"buildings are bought from home; the Thermal Forge needs its discovery");
  Check(w.SecondSpecReason(c,ColonyCatalog.Research)!=""&&w.SetColonyPolicy(c,ColonyCatalog.SelfSufficiency),"a second specialisation waits until the colony is established");
  c.level=2;Check(w.SetSpecialisation(c,ColonyCatalog.Research,true)&&c.second==ColonyCatalog.Research,"then it takes one");
  Check(w.Valid(),"valid with colonies");
 }
 static void Research()
 {
  var w=Fresh();Tile gate;City home;var c=Planted(w,out gate,out home);Rich(w,0);
  Check(w.AdaptationRate(0,DimensionFamily.Ember)>=1&&w.AdaptationRate(0,DimensionFamily.Resonant)==0,"a colony learns about its own family of worlds, and nothing about one nobody has visited");
  int slow=w.AdaptationRate(0,DimensionFamily.Ember);w.SetAdaptationFunding(0,DimensionFamily.Ember,2);Check(w.AdaptationRate(0,DimensionFamily.Ember)==slow+2&&w.AdaptationFundingCost(0)==2*AdaptationCatalog.FundingStep,"gold from home speeds it");
  Check(w.DiscoveryReason(0,DimensionFamily.Ember,AdaptationCatalog.ThermalForging).Contains("Banked Embers"),"discoveries have prerequisites");
  w.nations[0].tech[TechnologyCatalog.Index("toolmaking")]=false;int dear=w.DiscoveryCost(0,DimensionFamily.Ember,AdaptationCatalog.BankedEmbers);Learn(w,0,"toolmaking");int cheap=w.DiscoveryCost(0,DimensionFamily.Ember,AdaptationCatalog.BankedEmbers);
  Check(cheap<dear,"related universal knowledge makes a discovery cheaper ("+dear+" to "+cheap+")");
  Check(w.StudyDiscovery(0,DimensionFamily.Ember,AdaptationCatalog.BankedEmbers),"a discovery is chosen");int turns=0;while(!w.Knows(0,DimensionFamily.Ember,AdaptationCatalog.BankedEmbers)&&turns<20){w.DimensionsTurnForCheck(0);turns++;}
  Check(w.Knows(0,DimensionFamily.Ember,AdaptationCatalog.BankedEmbers),"and learned in a few turns ("+turns+")");
  Check(w.AttritionRelief(0,2)==1&&w.AttritionRelief(0,3)==1&&w.AttritionRelief(0,4)==0&&w.AttritionRelief(1,2)==0,"it works in every world of the family, for the one who learned it");
  // Effects: a workshop only where its rules hold, more exports, and an old-fashioned unit that outdoes a modern one.
  w.AdaptationOf(0,DimensionFamily.Ember).known.Add(AdaptationCatalog.ThermalForging);int ore,goods,ore2;w.ColonyOutput(c,out ore,out goods);Check(w.ColonyBuild(c,ColonyCatalog.FamilyWorkshop)&&w.WorkshopName(c)=="Thermal Forge","Thermal Forging lets the colony build a Thermal Forge");
  w.ColonyOutput(c,out ore2,out goods);Check(ore2==ore+1,"which adds a load of exports");
  var far=w.GateTwin(gate);var spot=Beside(w,far);var foe=w.Add(1,2,spot.x,spot.y);var knight=w.Add(0,5,spot.x,spot.y);var rifle=w.Add(0,6,spot.x,spot.y);
  Check(w.Strength(knight,foe,false)>w.Strength(rifle,foe,false),"where powder fails, a Knight strikes harder than a Rifleman");
  // A native people already lives by its world's rules.
  int native=-1;for(int o=0;o<w.civCount;o++)if(w.IsNativeNation(o)&&DimensionFamily.OfRealm(CivilisationCatalog.NativeRealm(w.nations[o].civ))==DimensionFamily.Ember)native=o;
  if(native>=0)Check(w.Knows(native,DimensionFamily.Ember,AdaptationCatalog.DeepTapping)&&!w.Knows(native,DimensionFamily.Resonant,0),"a people of an Ember world has its family's craft and no other's");
  // Synthesis: needs both families and a universal technology, and is paid for out of ordinary research.
  Check(w.DiscoveryReason(0,AdaptationCatalog.Synthesis,AdaptationCatalog.FlamelessFoundry).Contains("Resonant"),"synthesis needs knowledge from both families");
  w.AdaptationOf(0,DimensionFamily.Resonant,true).known.Add(AdaptationCatalog.CrystalLathe);w.nations[0].tech[TechnologyCatalog.Index("engineering")]=false;Check(w.DiscoveryReason(0,AdaptationCatalog.Synthesis,AdaptationCatalog.FlamelessFoundry).Contains("Engineering"),"and a universal technology");
  Learn(w,0,"engineering");Check(w.StudyDiscovery(0,AdaptationCatalog.Synthesis,AdaptationCatalog.FlamelessFoundry),"it can then be studied");
  var n=w.nations[0];n.research=w.AvailableTech(0).First();n.progress[n.research]=50;home.scientists=0;int share=w.SynthesisShare(0);w.DimensionsTurnForCheck(0);var syn=w.AdaptationOf(0,AdaptationCatalog.Synthesis);
  Check(share>=0&&n.progress[n.research]==50-Math.Min(50,share)&&syn.points==Math.Min(50,share),"half of the home cities' science goes to it instead of the technology in hand ("+share+")");
  if(!home.buildings.Contains(30))home.buildings.Add(30);int p=w.Yield(home)[1];syn.known.Add(AdaptationCatalog.FlamelessFoundry);Check(w.Yield(home)[1]==p+1,"the Flameless Foundry adds production in a home city with a Workshop");
  int cap=w.GateCapacityFor(0,gate);syn.known.Add(AdaptationCatalog.GateHarmonics);Check(w.GateCapacityFor(0,gate)==cap+3,"Gate Harmonics widens every gateway");
  Check(w.Valid(),"valid with discoveries");
 }
 static void Accords()
 {
  var w=Fresh();Tile gate;City home;var mine=Planted(w,out gate,out home);Rich(w,0);Rich(w,1);w.met[1]=true;
  // Recognition of colonial claims: a complete exchange with a rival.
  var far=w.GateTwin(gate);var theirSite=w.ColonySites(1,far.realm).First();var theirSettler=w.Add(1,0,theirSite.x,theirSite.y);theirSettler.moves=4;var theirs=w.FoundColony(theirSettler);Check(theirs!=null&&theirs.owner==1,"a rival founds a colony by the same rule");
  w.AddOpinion(1,0,OpinionMods.Dealings,30);int influence=w.nations[0].influence,opinion=w.Opinion(1,0);
  Check(w.ClaimsReason(0,1)==""&&w.RecogniseClaims(0,1)&&w.HasAccord(0,1,AccordCatalog.Claims)&&w.nations[0].influence==influence-AccordCatalog.ClaimInfluence&&w.Opinion(1,0)>opinion,"colonial claims are recognised for influence, and both think better of each other");
  var near=w.tiles.Where(t=>t.realm==far.realm&&t.gateLink==0&&w.Distance(t.x,t.y,theirs.x,theirs.y)==ColonyCatalog.MinSpacing&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&w.Distance(t.x,t.y,mine.x,mine.y)>=ColonyCatalog.MinSpacing).FirstOrDefault();
  if(near!=null){var squatter=w.Add(0,0,near.x,near.y);squatter.moves=4;Check(w.ColonyReason(squatter).Contains("recognised"),"a recognised claim is kept: no colony beside theirs ("+w.ColonyReason(squatter)+")");w.units.Remove(squatter);}
  w.DeclareWar(0,1);w.DimensionsTurnForCheck(0);Check(!w.HasAccord(0,1,AccordCatalog.Claims),"war ends the recognition");w.MakePeaceForCheck(0,1);
  // Joint investigation.
  var other=Gate(w,GateCondition.Sealed);w.StudyOfForCheck(1,other).known=true;w.AddOpinion(1,0,OpinionMods.Dealings,200);Check(!w.AtWar(0,1)&&w.JointStudyReason(0,1,other)=="","joint investigation is open to those on terms: "+w.JointStudyReason(0,1,other));int gold=w.gold,theirGold=w.nations[1].gold;
  Check(w.JointStudyPartners(0,other).Contains(1)&&w.JointStudy(0,1,other)&&w.StudyOf(0,other).known&&w.gold==gold-AccordCatalog.JointGold&&w.nations[1].gold==theirGold+AccordCatalog.JointGold,"a rival that has read a seal shares the reading for gold");
  // Passage: a gateway on a rival's land needs open borders.
  var st=Gate(w,GateCondition.Dormant);w.OpenGateForCheck(st);st.owner=1;Check(w.GateBlock(0,st).Contains("open borders"),"a gateway on another civilisation's land needs an open borders agreement");
  w.SetPactForCheck(0,1,Pacts.Borders,true);Check(w.GateUsable(0,st),"with one, it carries your loads");w.SetPactForCheck(0,1,Pacts.Borders,false);Check(!w.GateUsable(0,st),"cancel the agreement and it stops");st.owner=-1;
  // The people of that world: lessons and supplies.
  int native=-1;for(int o=0;o<w.civCount;o++)if(w.IsNativeNation(o)&&CivilisationCatalog.NativeRealm(w.nations[o].civ)==far.realm&&w.cities.Any(x=>x.owner==o))native=o;
  if(native>=0)
  {
   w.met[native]=true;Check(w.LessonReason(0,native).Contains("trust"),"a local people teaches only those it trusts");w.AddOpinion(native,0,OpinionMods.Dealings,40);
   int pts=w.AdaptationOf(0,DimensionFamily.Ember,true).points;Check(w.LocalPeoples(0,far.realm).Contains(native)&&w.LocalLesson(0,native)&&w.AdaptationOf(0,DimensionFamily.Ember).points==pts+World.LocalLessonPoints,"then its lessons count toward local discoveries");
   mine.stock=0;Check(w.LocalSupply(0,native,mine)&&mine.stock==AccordCatalog.SupplyAmount,"and it will sell a colony supplies with no gateway at all");
  }
  Check(w.Valid(),"valid with accords");
 }
 // The whole loop. Returns {production to investigate, gateway loads, production gained at home}.
 static int[] Scenario(bool advanced)
 {
  var w=Fresh();Rich(w,0);Rich(w,1);string stage=advanced?"advanced":"early";
  if(advanced)Learn(w,0,"agriculture","bronze","wheel","writing","maths","engineering","printing","education","industrialisation","electricity","computers","military_science");
  // 1. Discover: an ordinary unit sees the gateway.
  var gate=Gate(w,GateCondition.Cycling);var home=MoveCityNear(w,gate);Check(w.gatesFound.Contains(gate.y*w.Width+gate.x),stage+": the gateway is found by exploring");
  // 2. Investigate with a city's production.
  int cost=w.CostFor(home,World.InvestigateProject);Check(w.ChooseProject(home,World.InvestigateProject,false),stage+": the city turns its production to the gateway");
  home.buildingProduction=cost;w.CompleteBuildingForCheck(home);Check(w.StudyOf(0,gate).known&&w.CompatibilityReport(0,gate).Any(l=>l.StartsWith("Confirmed")),stage+": the investigation confirms what lies beyond");
  // 3. Explore: a scout crosses when the gateway opens.
  var side=Beside(w,gate);var scout=w.Add(0,3,side.x,side.y);scout.x=gate.x;scout.y=gate.y;while(!w.GateOpen(gate))w.turn++;w.TravelForCheck(scout);Check(w.At(scout.x,scout.y).realm==w.GateRealm(gate),stage+": a scout explores the far side");
  // 4. Colony.
  var far=w.GateTwin(gate);var site=w.ColonySites(0,far.realm).First();var settler=w.Add(0,0,site.x,site.y);settler.moves=4;w.Found(settler);var colony=w.ColonyAt(site.x,site.y);Check(colony!=null,stage+": a colony is founded");
  // Steady the gateway so that supply runs every turn.
  w.Add(0,2,gate.x,gate.y);Check(w.BeginApproach(0,gate,GateApproach.Stabilise),stage+": the cycling gateway is steadied ("+w.ApproachReason(0,gate,GateApproach.Stabilise)+")");w.turn+=w.StabiliseTurnsNow;w.GateExpeditionTurn();Check(w.GateSteady(gate),stage+": and stays open");
  foreach(var u in w.units.Where(u=>u.x==gate.x&&u.y==gate.y).ToList())w.units.Remove(u);
  // 5. A local adaptation.
  w.StudyDiscovery(0,DimensionFamily.Ember,AdaptationCatalog.BankedEmbers);int before=w.Yield(home)[1];for(int k=0;k<16&&!w.Knows(0,DimensionFamily.Ember,AdaptationCatalog.BankedEmbers);k++)w.DimensionsTurnForCheck(0);
  Check(w.Knows(0,DimensionFamily.Ember,AdaptationCatalog.BankedEmbers),stage+": a local adaptation is developed");
  // 6. Exports reach the home city.
  w.DimensionsTurnForCheck(0);int gained=w.Yield(home)[1]-before;Check(gained>0&&gained>=colony.shipShields,stage+": exports benefit the home city (+"+gained+")");
  // 7. A rival competes: it founds beside, and takes the gateway by force.
  var rivalSite=w.ColonySites(1,far.realm).First();var rs=w.Add(1,0,rivalSite.x,rivalSite.y);rs.moves=4;Check(w.FoundColony(rs)!=null,stage+": a rival founds a colony in the same world");
  w.met[1]=true;w.DeclareWar(1,0);var raider=w.Add(1,2,gate.x,gate.y);int units=w.units.Count(u=>u.owner==0),cols=w.ColoniesOf(0).Count;
  // 8. The interruption.
  w.DimensionsTurnForCheck(0);w.DimensionsTurnForCheck(0);
  Check(colony.isolated>0&&colony.shipShields==0&&w.Yield(home)[1]==before,stage+": the interruption stops supply and exports");
  Check(w.units.Count(u=>u.owner==0)==units&&w.ColoniesOf(0).Count==cols&&colony.owner==0,stage+": no unit is deleted and no colony changes hands");
  w.units.Remove(raider);w.DimensionsTurnForCheck(0);Check(colony.isolated==0&&colony.shipShields>0,stage+": supply resumes when the gateway is retaken");
  Check(w.Valid(),stage+": the game is valid at the end");
  return new[]{cost,w.GateCapacityFor(0,gate),gained};
 }
 // Rivals left to themselves for a long game: the world stays valid, and what they did beyond the gates is reported.
 static void LongGame()
 {
  var w=Fresh();int turns=160;for(int k=0;k<turns;k++){w.EndTurn();if(k%40==39)Check(w.Valid(),"valid after "+(k+1)+" turns with rivals using gateways");if(w.result!="")break;}
  int studied=w.gateStudies.Count(s=>s.owner!=0&&s.known),colonies=w.colonies.Count(c=>c.owner!=0),steady=w.HomeGates().Count(g=>w.GateSteady(g)),cities=w.cities.Count(c=>c.owner!=0&&!w.IsNativeNation(c.owner));
  for(int o=1;o<w.civCount;o++){if(w.IsNativeNation(o))continue;var own=w.cities.Where(c=>c.owner==o).ToList();var known=w.HomeGates().Where(g=>w.nations[o].known[g.y*w.Width+g.x]).ToList();var cyc=Gate(w,GateCondition.Cycling);
   Console.WriteLine("MEASURE   rival "+o+": "+own.Count+" cities, gold "+w.nations[o].gold+", era "+w.Era(o)+", knows "+known.Count+" of "+w.HomeGates().Count+" gateways, nearest known "+(known.Count==0||own.Count==0?-1:known.Min(g=>own.Min(c=>w.Distance(c.x,c.y,g.x,g.y))))+" tiles from a city, cycling gateway "+(own.Count==0?-1:own.Min(c=>w.Distance(c.x,c.y,cyc.x,cyc.y)))+" tiles away, at war "+Enumerable.Range(0,w.civCount).Any(x=>x!=o&&w.AtWar(o,x))+", settlers "+w.units.Count(x=>x.owner==o&&x.type==0));}
  Console.WriteLine("MEASURE Dimensions: after "+w.turn+" turns on a Small map, home rivals hold "+cities+" cities, have read "+studied+" seals, steadied or opened "+steady+" gateways and founded "+colonies+" colonies; discoveries "+w.adaptations.Where(a=>a.owner!=0).Sum(a=>a.known.Count)+".");
 }
}
}
