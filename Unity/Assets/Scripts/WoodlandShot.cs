using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (26 Nov 2026): "-hudShot <folder> -woodOnly" plays the woodland scenario in the built game at 1280 x 800 and
// photographs the owner's requests of that day: the site panel opened by a real click on the woods, the timber and stone ledger
// opened by a real click on the top bar, the city line waiting for timber, farms in each age, the simplified Religion page and
// the Court page under "All leaders". PROBE lines in the player log say what was tested.
public partial class RealmGame {
 int farmAgeForShots=-1;
 IEnumerator WoodlandShots(string folder){
  Directory.CreateDirectory(folder);tutorialOn=false;tutorialStep=-1;guide=false;
  string scenario=Path.Combine(Application.temporaryCachePath,"woodland-shot.json");File.WriteAllText(scenario,JsonUtility.ToJson(World.WoodlandScenario()));LoadFile(scenario);activeSavePath=null;autoSaveFolder=folder;
  mainMenu=false;nameSetup=false;viewRealm=0;isoWidth=96;var wood=world.WoodlandSite();Probe("woodland scenario loads with its woods in sight",wood!=null);if(wood==null){Application.Quit();yield break;}
  selected=-1;cityId=-1;FocusTile(wood.x,wood.y);yield return Frames(30);
  // The site panel by a real left click on the woods.
  var p=TileScreen(wood.x,wood.y,isoWidth);yield return ClickAt(p.x,p.y+4);yield return Frames(20);
  Probe("a left click on the woods opens the site panel",SitePanelOpen());var rep=world.AssessSite(0,wood);
  Debug.Log("PROBE site lines: "+string.Join(" | ",rep.lines.ToArray()));foreach(var o in rep.options)Debug.Log("PROBE site option: "+o.title+" — "+o.detail);
  yield return Shot(folder,"wood-site-panel.bmp",20);
  // Send the Worker from the panel, then play until the camp stands.
  {string why=world.SendWorkerToCamp(0,wood);sitePanel=-1;Probe("the Worker is sent ("+why+")",why=="");}
  var cap=world.cities.First(c=>c.owner==0);int turns=0;
  while(world.OutpostAt(wood.x,wood.y)==null&&turns<12){EndTurn();while(turnRunning)yield return null;yield return Frames(3);ClosePrompts();turns++;}
  Probe("the camp is set up after "+turns+" turns",world.OutpostAt(wood.x,wood.y)!=null);
  EndTurn();while(turnRunning)yield return null;yield return Frames(5);ClosePrompts();FocusTile(wood.x,wood.y);yield return Frames(30);
  yield return Shot(folder,"wood-camp-on-map.bmp",20);
  // The ledger by a real click on the timber figure.
  ClosePrompts();yield return ClickAt(materialsAnchor+30,18);yield return Frames(15);Probe("a click on the timber figure opens the ledger",materialsPanel);
  Debug.Log("PROBE ledger made: "+string.Join(" | ",world.ledger.made.ToArray()));Debug.Log("PROBE ledger used: "+string.Join(" | ",world.ledger.used.ToArray()));
  Probe("ledger: closing = opening + produced - used",world.ledger.closeTimber==world.ledger.openTimber+world.ledger.madeTimber-world.ledger.usedTimber);
  yield return Shot(folder,"wood-ledger.bmp",20);materialsPanel=false;yield return Send(Mouse(EventType.MouseMove,640,520,0,0));
  // A city line waiting for timber, with the build list's preview.
  world.nations[0].timber=0;world.nations[0].stone=0;cap.buildingQueue.Clear();if(!cap.granary)cap.buildingProduction=world.CostFor(cap,7);else{int nb=World.BuildingIds.Where(b=>!world.HasBuilding(cap,b)&&world.BuildReason(cap,b)==""&&world.BuildingTimber(b)>0&&!World.IsWonder(b)).OrderBy(b=>world.CostFor(cap,b)).FirstOrDefault();world.nations[0].gold=500;world.gold=500;world.ChooseProject(cap,nb,false);world.SettleBuilding(cap);cap.buildingProduction=world.CostFor(cap,nb);}
  cityId=cap.id;cityScreen=true;yield return Frames(20);Debug.Log("PROBE city line: "+world.MaterialLine(cap,true));Probe("the line says it waits for timber",world.MaterialBrief(cap,true).StartsWith("Waiting"));
  yield return Shot(folder,"wood-city-line.bmp",20);cityScreen=false;
  // Farms in each age, on the tiles round the capital.
  foreach(var t in world.tiles.Where(q=>world.Distance(q.x,q.y,cap.x,cap.y)<=2))if(t.owner==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&!(t.x==cap.x&&t.y==cap.y)&&t.terrain!=7&&!World.Wooded(t)&&t.terrain!=2)t.farm=true;
  ClosePrompts();isoWidth=128;FocusTile(cap.x,cap.y);for(int age=0;age<FarmDesigns.Length;age++){farmAgeForShots=age;yield return Frames(20);yield return Shot(folder,"wood-farms-age-"+age+".bmp",20);}farmAgeForShots=-1;isoWidth=96;
  // Religion, simplified, and its full details.
  OpenDetailsQuiet("Religion");yield return Frames(15);yield return Shot(folder,"wood-religion.bmp",20);religionDetail=true;yield return Frames(10);yield return Shot(folder,"wood-religion-details.bmp",20);detailsScreen="";religionDetail=false;
  // The Court's full page: "All leaders" sits above the text.
  diplomacyPartner=1;diplomacyDirect=true;OpenDetailsQuiet("Diplomacy");diplomacyChooser=false;diplomacyDiscuss=true;yield return Frames(20);yield return Shot(folder,"wood-court-discuss.bmp",20);detailsScreen="";diplomacyDiscuss=false;
  Application.Quit();
 }
 void OpenDetailsQuiet(string name){detailsScreen=name;faithsView=false;religionDetail=false;diplomacyDiscuss=false;diplomacyChooser=name=="Diplomacy"&&!diplomacyDirect;diplomacyDirect=false;researchScreen=false;detailsScroll=Vector2.zero;}
 void ClosePrompts(){if(world.researchPrompts!=null)world.researchPrompts.Clear();if(world.buildPrompts!=null)world.buildPrompts.Clear();if(world.buildNotices!=null)world.buildNotices.Clear();if(world.rivalCalls!=null)world.rivalCalls.Clear();if(world.newContacts!=null)world.newContacts.Clear();if(world.wayChoices!=null)world.wayChoices.Clear();gateScene=null;}
}
}
