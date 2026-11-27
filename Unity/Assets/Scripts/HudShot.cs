using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm { public partial class RealmGame {
 // Development aid: "-hudShot <folder>" photographs the owner's requests of 14 Nov 2026 (long notes shown in full, the settler's
 // suggestion box above the unit panel, the selection outline behind the unit), then quits.
 IEnumerator HudShot(string folder){
  Directory.CreateDirectory(folder);yield return Frames(5);
  // New Game with the Zarevan Crown chosen among the Fantasy Kingdoms, and its lore page (18 Nov 2026).
  if(System.Environment.GetCommandLineArgs().Contains("-setupOnly")){yield return SetupShots(folder);Application.Quit();yield break;}
  if(System.Environment.GetCommandLineArgs().Contains("-woodOnly")){yield return WoodlandShots(folder);yield break;}
  if(System.Environment.GetCommandLineArgs().Contains("-valleyOnly")){yield return ValleyTalkShots(folder);yield break;}
  if(System.Environment.GetCommandLineArgs().Contains("-raiderOnly")){yield return RaiderShots(folder);yield break;}
  if(System.Environment.GetCommandLineArgs().Contains("-forcesOnly")){yield return ForcesShots(folder);yield break;}
  if(System.Environment.GetCommandLineArgs().Contains("-courtOnly")){yield return CourtShots(folder);yield break;}
  if(System.Environment.GetCommandLineArgs().Contains("-roadOnly")){yield return RoadShots(folder);yield break;}
  if(System.Environment.GetCommandLineArgs().Contains("-treasuryOnly")){yield return TreasuryShots(folder);yield break;}
  mainMenu=false;nameSetup=true;setupPage=0;civGroupReal=false;setupCiv=FantasyKingdoms.First;proposedName=CivilisationCatalog.Names[setupCiv];yield return Frames(15);yield return Shot(folder,"hud-newgame-zarevan.bmp");
  civLoreOpen=true;yield return Frames(10);yield return Shot(folder,"hud-lore-zarevan.bmp");civLoreOpen=false;setupCiv=0;
  seedText="421";mainMenu=false;nameSetup=false;guide=false;tutorialOn=false;tutorialStep=-1;StartGame();hasSession=true;guide=false;tutorialOn=false;tutorialStep=-1;
  yield return Frames(10);
  world.Note("A Woolly mammoth attacked your Warrior; it is getting away and will explore on.");world.Note("Azurehold grew to population 6.");
  var settler=world.units.FirstOrDefault(u=>u.owner==0&&u.type==0);
  if(settler!=null){selected=settler.id;cityId=-1;FocusTile(settler.x,settler.y);}
  // The Chronicle of Discovery fills the display with every border inside it (18 Nov 2026).
  researchScreen=true;inspectedTech=-1;researchEra=-1;researchScroll=Vector2.zero;yield return Frames(20);yield return Shot(folder,"hud-research.bmp");researchScreen=false;
  yield return Frames(30);yield return Shot(folder,"hud-settler.bmp");
  moreActions=true;yield return Frames(10);yield return Shot(folder,"hud-settler-more.bmp");moreActions=false;
  var guard=world.units.FirstOrDefault(u=>u.owner==0&&u.type!=0);
  if(guard!=null){selected=guard.id;isoWidth=192;FocusTile(guard.x,guard.y);yield return Frames(30);yield return Shot(folder,"hud-unit-outline.bmp");}
  // 14 Nov 2026 (second set): the city screen against the map, workers at work, the livestock, and the two diplomacy screens.
  City home=null;if(settler!=null&&world.units.Contains(settler))home=world.Found(settler);
  if(home!=null){
   world.Reveal();var ring=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)==1&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)).ToList();
   if(ring.Count>0){ring[0].farm=true;ring[0].terrain=0;ring[0].cleared=true;ring[0].owner=0;}
   foreach(string id in new[]{"agriculture","wheel","bronze","iron"})world.tech[TechnologyCatalog.Index(id)]=true;
   int[] herd={16,17,18,19,4};int h=0;foreach(var t in world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)==2&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)).Take(5)){t.resource=herd[h++];t.seen=true;}
   // Three workers beside the city, one at each tool.
   var spots=ring.Skip(1).Take(3).ToList();int[] jobs={5,3,2};for(int k=0;k<spots.Count;k++){var w=world.Add(0,1,spots[k].x,spots[k].y);w.job=jobs[k];w.hasOrder=false;}
   selected=-1;cityId=home.id;isoWidth=160;FocusTile(home.x,home.y);yield return Frames(30);yield return Shot(folder,"hud-map-city.bmp");
   world.NoticeForShot(home,8);yield return Frames(10);yield return Shot(folder,"hud-notice.bmp");world.buildNotices.Clear();
   yield return Frames(3);yield return Shot(folder,"hud-workers-b.bmp");
   // The redesigned city screen: a size 4 city with a scientist, both lines busy with queues, then a building hovered.
   {int popWas=home.pop;home.pop=4;home.scientists=1;home.food=9;world.gold=200;world.nations[0].gold=200;world.ChooseProject(home,World.UnitBuild(2),false);world.ChooseProject(home,World.UnitBuild(1),true);world.ChooseProject(home,World.UnitBuild(0),true);world.ChooseProject(home,7,false);world.ChooseProject(home,8,true);home.production=6;home.buildingProduction=12;
   cityScreen=true;cityBuildingTab=false;cityMapScroll=Vector2.zero;yield return Frames(20);yield return Shot(folder,"hud-city-screen.bmp");
   cityBuildingTab=true;cityShotHover=world.Workable(home).Any(t=>t.terrain==4||t.terrain==6||t.terrain==12)?43:31;yield return Frames(20);yield return Shot(folder,"hud-city-hover.bmp");cityShotHover=-1;cityBuildingTab=false;cityScreen=false;
   home.pop=popWas;home.scientists=0;home.queue.Clear();home.buildingQueue.Clear();home.build=-1;home.buildingProject=-1;home.production=0;home.buildingProduction=0;}
   // Empire > Forces (20 Nov 2026): a Spearman ordered, the Warrior picked, then the mobilisation preview.
   {bool wasForces=world.nationalForces;int popKeep=home.pop;world.nationalForces=true;world.nations[0].tech[TechnologyCatalog.Index("bronze")]=true;home.pop=6;world.Raise(0,7,home);world.Raise(0,2,home);var firstGuard=world.units.FirstOrDefault(u=>u.owner==0&&u.type==2);if(firstGuard!=null)forcesPick=firstGuard.id;
    detailsScreen="Military";militaryTab=0;yield return Frames(20);yield return Shot(folder,"hud-forces.bmp");AskMobilise(true);yield return Frames(10);yield return Shot(folder,"hud-forces-mobilise.bmp");forcesConfirm=null;forcesConfirmDo=null;
    detailsScreen="";empireTab=0;foreach(var o in world.ForceOrders(0).ToList())world.CancelForceOrder(0,o.id);home.pop=popKeep;world.nationalForces=wasForces;}
   // Empire > Statecraft (24 Nov 2026): the three ways to govern, the question, and a change settling.
   {int laws=TechnologyCatalog.Index(StatecraftRules.Tech),popKeep=home.pop;bool knew=world.nations[0].tech[laws];world.nations[0].tech[laws]=true;home.pop=7;detailsScreen="Economy";empireTab=0;yield return Frames(10);yield return Shot(folder,"hud-empire-tabs.bmp");
    empireTab=3;yield return Frames(15);yield return Shot(folder,"hud-statecraft.bmp");statecraftAsk=StatecraftRules.Centralised;yield return Frames(10);yield return Shot(folder,"hud-statecraft-ask.bmp");statecraftAsk=-1;
    world.SetStatecraft(0,StatecraftRules.Centralised);yield return Frames(10);statecraftShown=null;yield return Shot(folder,"hud-statecraft-settling.bmp");
    world.nations[0].statecraft=0;world.nations[0].statecraftSince=-99;world.nations[0].tech[laws]=knew;home.pop=popKeep;detailsScreen="";empireTab=0;}
   // Barbarians: a camp and two raiders (one hurt) beside the city on the map.
   {var barbSpots=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)>=2&&world.Distance(t.x,t.y,home.x,home.y)<=3&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&!world.units.Any(u=>u.x==t.x&&u.y==t.y)&&world.Visible(t.x,t.y)).Take(3).ToList();
    if(barbSpots.Count==3){bool was=world.barbarians;world.barbarians=true;world.camps.Add(new BarbarianCamp{id=99101,x=barbSpots[0].x,y=barbSpots[0].y,seen=true});world.raiders.Add(new Raider{id=99102,x=barbSpots[1].x,y=barbSpots[1].y});world.raiders.Add(new Raider{id=99103,x=barbSpots[2].x,y=barbSpots[2].y,hp=6,mounted=1});
     selected=-1;isoWidth=192;FocusTile(barbSpots[0].x,barbSpots[0].y);yield return Frames(30);yield return Shot(folder,"hud-barbarians.bmp");
     // The camp panel (24 Nov 2026): offers open, then hired.
     {bool[] metWas=(bool[])world.met.Clone();int goldWas=world.gold;for(int o=1;o<world.civCount&&o<=4;o++)world.met[o]=true;world.gold=world.nations[0].gold=400;world.camps.Add(new BarbarianCamp{id=99104,x=barbSpots[1].x,y=barbSpots[1].y,seen=true});OpenCampAt(barbSpots[0].x,barbSpots[0].y);yield return Frames(15);yield return Shot(folder,"hud-camp-panel.bmp");
      world.PayOff(0,world.camps[0]);yield return Frames(10);yield return Shot(folder,"hud-camp-paid.bmp");campPanel=-1;world.met=metWas;world.gold=world.nations[0].gold=goldWas;}
     world.camps.Clear();world.raiders.Clear();world.barbarians=was;}}
   // Farms and roads on a river (24 Nov 2026): the painted field on both banks; a road without and with Bridge Building.
   {var rivers=world.tiles.Where(t=>t.realm==0&&t.terrain==7&&t.gateLink==0&&t.y>3&&t.y<world.MainHeight-4&&world.RiverDown(t)>=0).ToList();
    System.Func<Tile,bool> dry=t=>t!=null&&t.terrain!=7&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&!t.village&&!t.ruin&&t.gateLink==0;int across=-1;
    var bend=rivers.FirstOrDefault(t=>{int d=world.RiverDown(t);var next=world.Step(t.x,t.y,World.HexDi[d],World.HexDj[d]);if(next==null||next.terrain!=7||world.RiverDown(next)<0||next.ruin||next.village)return false;
     var after=world.Step(next.x,next.y,World.HexDi[world.RiverDown(next)],World.HexDj[world.RiverDown(next)]);if(after==null||after.terrain!=7)return false;
     for(int k=0;k<3;k++)if(dry(world.Step(next.x,next.y,World.HexDi[k],World.HexDj[k]))&&dry(world.Step(next.x,next.y,-World.HexDi[k],-World.HexDj[k]))){across=k;return true;}return false;});
    if(bend!=null){var around=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,bend.x,bend.y)<=4).ToList();int bridgeTech=World.BridgeTechIndex;bool knew=world.nations[0].tech[bridgeTech];
     foreach(var t in around){t.seen=true;t.rememberedTerrain=t.terrain;if(t.terrain!=7&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&world.Distance(t.x,t.y,bend.x,bend.y)<=2){t.terrain=0;t.rememberedTerrain=0;t.cleared=true;t.resource=0;t.woods=0;}}
     int dd=world.RiverDown(bend);var ford=world.Step(bend.x,bend.y,World.HexDi[dd],World.HexDj[dd]);bend.farm=bend.rememberedFarm=true;
     var n=world.Step(ford.x,ford.y,World.HexDi[across],World.HexDj[across]);var o=world.Step(ford.x,ford.y,-World.HexDi[across],-World.HexDj[across]);
     foreach(var t in new[]{ford,n,o})t.road=t.rememberedRoad=true;foreach(var t in around)if(!t.road&&t.terrain==0&&world.Distance(t.x,t.y,bend.x,bend.y)==1)t.farm=t.rememberedFarm=true;
     world.units.RemoveAll(u=>u.owner!=0);var watcher=world.Add(0,3,o.x,o.y);world.Reveal();selected=-1;isoWidth=256;FocusTile(bend.x,bend.y);world.nations[0].tech[bridgeTech]=false;yield return Frames(40);yield return Shot(folder,"hud-river-farm-ford.bmp");
     world.nations[0].tech[bridgeTech]=true;yield return Frames(20);yield return Shot(folder,"hud-river-farm-bridge.bmp");world.units.Remove(watcher);researchScreen=true;world.nations[0].tech[TechnologyCatalog.Index("writing")]=true;FocusTechnology(bridgeTech);yield return Frames(20);yield return Shot(folder,"hud-bridge-tech.bmp");researchScreen=false;inspectedTech=-1;researchEra=-1;world.nations[0].tech[bridgeTech]=knew;}}
   // Soldiers: each type strikes the tile beside it (mid-blow), then takes guard (mid-clip, then held).
   var field=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)>=2&&world.Distance(t.x,t.y,home.x,home.y)<=4&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)).Take(6).ToList();
   int[] types={2,7,4,8,5,3};var soldiers=new System.Collections.Generic.List<Unit>();
   for(int k=0;k<field.Count;k++){var s=world.Add(0,types[k],field[k].x,field[k].y);soldiers.Add(s);}
   selected=-1;isoWidth=128;FocusTile(home.x,home.y);yield return Frames(10);yield return Shot(folder,"hud-soldiers-still.bmp");
   if(world.strikeLog==null)world.strikeLog=new System.Collections.Generic.List<int[]>();
   foreach(var s in soldiers){var to=world.Step(s.x,s.y,1,0)??world.Step(s.x,s.y,-1,0);if(to!=null)world.strikeLog.Add(new[]{s.id,to.x,to.y});}
   yield return new WaitForSecondsRealtime(.45f);yield return Shot(folder,"hud-strike-mid.bmp",0);
   yield return new WaitForSecondsRealtime(1f);foreach(var s in soldiers)s.fortified=true;
   yield return new WaitForSecondsRealtime(.35f);yield return Shot(folder,"hud-guard-mid.bmp",0);
   yield return new WaitForSecondsRealtime(1f);yield return Shot(folder,"hud-guard-held.bmp");
   foreach(var s in soldiers)world.units.Remove(s);
  }
  for(int o=1;o<world.civCount&&o<=8;o++)world.met[o]=true;world.newContacts.Clear();
  detailsScreen="Diplomacy";diplomacyChooser=true;councilPick=-1;yield return Frames(20);yield return Shot(folder,"hud-council.bmp");
  for(int o=1;o<world.civCount&&o<=16;o++)world.met[o]=true;
  councilPick=Enumerable.Range(1,System.Math.Min(16,world.civCount-1)).OrderByDescending(o=>world.NationName(o).Length).First();
  yield return Frames(20);yield return Shot(folder,"hud-council-16.bmp");
  // The Zarevan Crown (18 Nov 2026): the first rival wears it for two pictures, the Council and its Court, then goes back.
  {var zn=world.nations[1];int zc=zn.civ,zp=zn.portrait;string zname=zn.name,zl=zn.leader;int z=FantasyKingdoms.First;zn.civ=z;zn.name=CivilisationCatalog.Names[z];zn.leader=CivilisationCatalog.Leaders[z];zn.portrait=CivilisationCatalog.Portraits[z];
   councilPick=1;yield return Frames(20);yield return Shot(folder,"hud-council-zarevan.bmp");diplomacyPartner=1;diplomacyChooser=false;ResetDiplomacyUI();yield return Frames(20);yield return Shot(folder,"hud-court-zarevan.bmp");
   diplomacyChooser=true;councilPick=-1;zn.civ=zc;zn.portrait=zp;zn.name=zname;zn.leader=zl;}
  diplomacyPartner=councilPick;diplomacyChooser=false;ResetDiplomacyUI();yield return Frames(20);yield return Shot(folder,"hud-court-long.bmp");diplomacyChooser=true;councilPick=-1;
  for(int o=9;o<world.civCount;o++)world.met[o]=false;
  diplomacyPartner=Enumerable.Range(1,world.civCount-1).First(o=>world.met[o]);diplomacyChooser=false;ResetDiplomacyUI();yield return Frames(20);yield return Shot(folder,"hud-court.bmp");customDeal=true;yield return Frames(15);yield return Shot(folder,"hud-court-trade.bmp");customDeal=false;requestsView=true;yield return Frames(15);yield return Shot(folder,"hud-court-request.bmp");requestsView=false;
  // The negotiation screens (23 Nov 2026): a passage clause on the table, the ruler's counteroffer, then the Council's views.
  {int p=diplomacyPartner;world.nations[0].gold=System.Math.Max(world.nations[0].gold,600);world.gold=world.nations[0].gold;customDeal=true;dealAskPassage=true;yield return Frames(15);yield return Shot(folder,"hud-deal-passage.bmp");
   world.Propose(new DiplomaticOffer{from=0,to=p,kind=TreatyKind.Trade,duration=15,askPassage=true});dealAskPassage=false;yield return Frames(15);yield return Shot(folder,"hud-deal-counter.bmp");
   var counter=world.offers.Find(o=>o.from==p&&o.to==0&&o.status==0&&o.counterOf>0);if(counter!=null)world.RespondToOffer(counter.id,true);customDeal=false;
   diplomacyChooser=true;councilPick=p;councilView=0;yield return Frames(15);yield return Shot(folder,"hud-council-markers.bmp");
   councilView=1;yield return Frames(15);yield return Shot(folder,"hud-council-list.bmp");councilView=2;yield return Frames(15);yield return Shot(folder,"hud-council-agreements.bmp");
   councilView=3;yield return Frames(15);yield return Shot(folder,"hud-council-history.bmp");councilView=0;diplomacyChooser=false;}
  // With a proposal waiting from them (the "Under proposal" row and the scrolling panel).
  world.offers.Add(new DiplomaticOffer{id=99001,from=diplomacyPartner,to=0,kind=Pacts.Kind[Pacts.Commercial],created=world.turn,expires=world.turn+5,status=0});
  yield return Frames(20);yield return Shot(folder,"hud-court-proposal.bmp");world.offers.RemoveAll(o=>o.id==99001);
  detailsScreen="";
  // Minimap layers (20 Nov 2026): the home world explored, ten turns played, then each layer with its key.
  // 25 Nov 2026: idle units are shown one after another (logged), and a gateway coming into view opens its screen.
  {devRun=false;bool keepPrompt=IdlePrompt;IdlePrompt=true;selected=-1;cityId=-1;detailsScreen="";researchScreen=false;cityScreen=false;world.newContacts.Clear();world.researchPrompts.Clear();world.buildPrompts.Clear();foreach(var u in world.units)if(u.owner==0){u.fortified=true;}
   var spots=world.units.Where(u=>u.owner==0).Take(1).ToList();var made=new System.Collections.Generic.List<Unit>();if(spots.Count>0)for(int k=0;k<3;k++){var a=world.Add(0,2,spots[0].x,spots[0].y);a.moves=4;made.Add(a);}
   IdleTurnBegan();yield return new WaitForSecondsRealtime(1.2f);bool first=made.Any(a=>a.id==selected);int one=selected;Debug.Log("PROBE "+(first?"pass":"FAIL")+": the turn begins with an idle unit selected ("+status+")");yield return Shot(folder,"hud-idle-unit.bmp");
   var sel=Selected();if(sel!=null)sel.fortified=true;yield return new WaitForSecondsRealtime(1.2f);Debug.Log("PROBE "+(selected!=one&&made.Any(a=>a.id==selected)?"pass":"FAIL")+": an order brings up the next idle unit");
   int two=selected;selected=-1;yield return new WaitForSecondsRealtime(1.2f);Debug.Log("PROBE "+(selected==-1?"pass":"FAIL")+": letting go of a unit is left alone");selected=two;yield return new WaitForSecondsRealtime(.2f);foreach(var a in made)a.fortified=true;yield return new WaitForSecondsRealtime(1.2f);Debug.Log("PROBE "+(status.StartsWith("Every unit")?"pass":"FAIL")+": with none left it says so ("+status+")");
   foreach(var a in made)world.units.Remove(a);selected=-1;IdlePrompt=keepPrompt;
   world.gatesFound.Clear();world.gateHints.Clear();foreach(var g0 in world.HomeGates())g0.seen=false;var hears=world.HomeGates().FirstOrDefault();if(hears!=null){world.HearOfGateForChecks(hears,"the Sahel Empire");yield return Frames(20);yield return Shot(folder,"hud-gate-hint.bmp");}
   var door=world.HomeGates().Skip(1).FirstOrDefault();if(door!=null){foreach(var t in world.tiles)if(t.realm==0&&world.Distance(t.x,t.y,door.x,door.y)<=2)t.seen=true;yield return new WaitForSecondsRealtime(6f);Debug.Log("PROBE "+(gateScene==door?"pass":"FAIL")+": a gateway coming into view opens its screen");yield return Shot(folder,"hud-gate-discovery.bmp",0);gateScene=null;gateScenePicture=null;}
   devRun=true;}
  {for(int k=0;k<60&&world.result=="";k++)world.EndTurn();world.wayChoices.Clear();world.LearnHomeWorldForShots();for(int o=1;o<world.civCount;o++)world.met[o]=true;world.Reveal();world.buildNotices.Clear();world.nations[0].faith=0;selected=-1;cityId=-1;miniKeyAlways=true;int keep=MiniLayer;for(int k=0;k<40;k++){gateScene=null;world.newContacts.Clear();world.researchPrompts.Clear();world.buildPrompts.Clear();yield return null;}
   for(int k=0;k<MiniLayerNames.Length;k++){MiniLayer=k;miniBuilt=-99;yield return Frames(4);gateScene=null;yield return CleanShot(folder,"hud-minimap-"+k+".bmp");}
   miniLayerOpen=true;yield return Frames(3);gateScene=null;yield return CleanShot(folder,"hud-minimap-list.bmp");miniLayerOpen=false;miniKeyAlways=false;MiniLayer=keep;}
   {var woodHill=world.tiles.FirstOrDefault(t=>t.realm==0&&World.HillWoods(t)&&t.seen);if(woodHill!=null){isoWidth=192;FocusTile(woodHill.x,woodHill.y);yield return Frames(10);yield return CleanShot(folder,"hud-hill-woods.bmp");}}
   detailsScreen="Military";militaryTab=1;yield return Frames(10);yield return Shot(folder,"hud-military-units.bmp");detailsScreen="";militaryTab=0;
  // Beliefs & Traditions (23 Nov 2026): the first question, then the First Granary scenario from shortage to a lean feast.
  {detailsScreen="";cityScreen=false;selected=-1;world.wayChoices.Clear();world.NationWayOf(0).opening=-1;int keepTurn=world.turn;world.turn=1;world.LifewaysTurn();world.turn=keepTurn;yield return Frames(10);yield return Shot(folder,"hud-way-opening.bmp");
   world=World.GranaryScenario();viewRealm=0;var gc=world.cities.First(c=>c.owner==0);FocusTile(gc.x,gc.y);world.LifewaysTurn();world.turn++;world.LifewaysTurn();yield return Frames(15);yield return Shot(folder,"hud-way-shortage.bmp");
   world.WayResolve(world.NextWayChoice(),0);for(int k=0;k<Lifeways.RememberAfter;k++){world.turn++;world.LifewaysTurn();}yield return Frames(10);yield return Shot(folder,"hud-way-remember.bmp");
   world.WayResolve(world.NextWayChoice(),0);var feast=world.practices[0];gc.food=0;world.turn=feast.nextDue;world.LifewaysTurn();yield return Frames(10);yield return Shot(folder,"hud-way-lean.bmp");
   world.WayResolve(world.NextWayChoice(),1);world.met[1]=true;world.Relation(0,1).contact=true;foreach(var c in world.cities.Where(c=>c.owner==0)){var l=world.WayOf(c);l.adherence[Lifeways.Remembrance]=Mathf.Max(l.adherence[Lifeways.Remembrance],62);l.tradition=Lifeways.Remembrance;l.conviction=55;l.cause="it grew out of the relief of turn 2";}
   world.TakeCommitment(0,Lifeways.Remembrance);world.DeclareVow(0,2,gc.id,-1,20);detailsScreen="Religion";faithsView=false;religionDetail=false;yield return Frames(15);yield return Shot(folder,"hud-religion.bmp");religionDetail=true;yield return Frames(15);yield return Shot(folder,"hud-traditions.bmp");religionDetail=false;detailsScreen="";}
  Application.Quit();
 }
}}
