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
  mainMenu=false;nameSetup=true;setupPage=0;civGroupReal=false;setupCiv=FantasyKingdoms.First;proposedName=CivilisationCatalog.Names[setupCiv];yield return Frames(15);yield return Shot(folder,"hud-newgame-zarevan.bmp");
  civLoreOpen=true;yield return Frames(10);yield return Shot(folder,"hud-lore-zarevan.bmp");civLoreOpen=false;setupCiv=0;
  seedText="421";mainMenu=false;nameSetup=false;guide=false;tutorialOn=false;tutorialStep=-1;StartGame();hasSession=true;guide=false;tutorialOn=false;tutorialStep=-1;
  yield return Frames(10);
  world.Note("A Megatherium attacked your Warrior; it is getting away and will explore on.");world.Note("Azurehold grew to population 6.");
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
   // Barbarians: a camp and two raiders (one hurt) beside the city on the map.
   {var barbSpots=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)>=2&&world.Distance(t.x,t.y,home.x,home.y)<=3&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&!world.units.Any(u=>u.x==t.x&&u.y==t.y)&&world.Visible(t.x,t.y)).Take(3).ToList();
    if(barbSpots.Count==3){bool was=world.barbarians;world.barbarians=true;world.camps.Add(new BarbarianCamp{id=99101,x=barbSpots[0].x,y=barbSpots[0].y,seen=true});world.raiders.Add(new Raider{id=99102,x=barbSpots[1].x,y=barbSpots[1].y});world.raiders.Add(new Raider{id=99103,x=barbSpots[2].x,y=barbSpots[2].y,hp=6,mounted=1});
     selected=-1;isoWidth=192;FocusTile(barbSpots[0].x,barbSpots[0].y);yield return Frames(30);yield return Shot(folder,"hud-barbarians.bmp");world.camps.Clear();world.raiders.Clear();world.barbarians=was;}}
   // Soldiers: each type strikes the tile beside it (mid-blow), then takes guard (mid-clip, then held).
   var field=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)>=2&&world.Distance(t.x,t.y,home.x,home.y)<=4&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)).Take(5).ToList();
   int[] types={2,7,4,8,5};var soldiers=new System.Collections.Generic.List<Unit>();
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
  diplomacyPartner=Enumerable.Range(1,world.civCount-1).First(o=>world.met[o]);diplomacyChooser=false;ResetDiplomacyUI();yield return Frames(20);yield return Shot(folder,"hud-court.bmp");customDeal=true;yield return Frames(15);yield return Shot(folder,"hud-court-trade.bmp");customDeal=false;
  // With a proposal waiting from them (the "Under proposal" row and the scrolling panel).
  world.offers.Add(new DiplomaticOffer{id=99001,from=diplomacyPartner,to=0,kind=Pacts.Kind[Pacts.Commercial],created=world.turn,expires=world.turn+5,status=0});
  yield return Frames(20);yield return Shot(folder,"hud-court-proposal.bmp");world.offers.RemoveAll(o=>o.id==99001);
  detailsScreen="";
  // Minimap layers (20 Nov 2026): the home world explored, ten turns played, then each layer with its key.
  {for(int k=0;k<60&&world.result=="";k++)world.EndTurn();world.LearnHomeWorldForShots();for(int o=1;o<world.civCount;o++)world.met[o]=true;world.Reveal();world.buildNotices.Clear();world.nations[0].faith=0;selected=-1;cityId=-1;miniKeyAlways=true;int keep=MiniLayer;for(int k=0;k<40;k++){gateScene=null;world.newContacts.Clear();world.researchPrompts.Clear();world.buildPrompts.Clear();yield return null;}
   for(int k=0;k<MiniLayerNames.Length;k++){MiniLayer=k;miniBuilt=-99;yield return Frames(4);gateScene=null;yield return CleanShot(folder,"hud-minimap-"+k+".bmp");}
   miniLayerOpen=true;yield return Frames(3);gateScene=null;yield return CleanShot(folder,"hud-minimap-list.bmp");miniLayerOpen=false;miniKeyAlways=false;MiniLayer=keep;}
   {var woodHill=world.tiles.FirstOrDefault(t=>t.realm==0&&World.HillWoods(t)&&t.seen);if(woodHill!=null){isoWidth=192;FocusTile(woodHill.x,woodHill.y);yield return Frames(10);yield return CleanShot(folder,"hud-hill-woods.bmp");}}
   detailsScreen="Military";militaryTab=1;yield return Frames(10);yield return Shot(folder,"hud-military-units.bmp");detailsScreen="";militaryTab=0;
  Application.Quit();
 }
}}
