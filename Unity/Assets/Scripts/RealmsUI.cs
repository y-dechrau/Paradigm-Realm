using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Parallel worlds in the interface (owner request, 31 Oct 2026): the Realms screen switches the map between the home world
// and each parallel world you have found; a banner on the map names the world you are looking at and returns you home.
public partial class RealmGame {
 void ShowRealm(int r){
  if(r!=0)globeZoom=false;
  if(r!=0&&!world.RealmDiscovered(0,r))return;
  // Look at one of your units there, else your capital or a gateway you know, else the middle of that world.
  var mine=world.units.FirstOrDefault(u=>u.owner==0&&world.RealmOf(u.x,u.y)==r);var city=world.cities.Where(c=>c.owner==0&&world.RealmOf(c.x,c.y)==r).OrderBy(c=>c.id).FirstOrDefault();
  var gate=world.tiles.FirstOrDefault(t=>t.realm==r&&t.gateLink>0&&t.seen);
  int x,y;if(city!=null){x=city.x;y=city.y;}else if(mine!=null){x=mine.x;y=mine.y;}else if(gate!=null){x=gate.x;y=gate.y;}else{x=world.Width/2;y=(world.RealmTop(r)+world.RealmBottom(r))/2;}
  // The home world is left and found again where the player was looking (the camera's place is kept).
  if(viewRealm==0&&r!=0){homeCamera=new Vector2(camI,camJ);homeCameraSaved=true;}
  if(r==0&&viewRealm!=0&&homeCameraSaved){viewRealm=0;atlasTexture=null;camI=homeCamera.x;camJ=homeCamera.y;ClampCamera();return;}
  viewRealm=r;atlasTexture=null;FocusTile(x,y);
 }
 // Legends of the sealed worlds: their histories and all ten peoples of each, to read while the gates stay shut.
 bool legendsOpen;int legendsWorld=1,legendsPeople=-1;Vector2 legendsScroll;
 // Only what the home world's civilisations have discovered shows here (owner request, 6 Nov 2026): a world once a gateway to it has
 // been found, a people as contact, friendship or agents reveal them.
 void DrawLegends(){
  var panel=new Rect(60,60,1160,690);Fill(new Rect(0,0,1280,800),UiDim);GUI.Box(panel,"");
  if(legendsPeople>=0){var who=world.NationOfPeople(legendsPeople);if(who<0||world.PeopleDetail(who)<1)legendsPeople=-1;else{DrawCivilisationLore(legendsPeople,world.PeopleDetail(who));return;}}
  GUI.Label(new Rect(84,74,800,34),"Legends of the sealed worlds",title);
  if(GUI.Button(new Rect(1020,76,176,32),"Close",button)){legendsOpen=false;GUIUtility.ExitGUI();}
  var knownWorlds=Enumerable.Range(1,World.RealmCount).Where(world.WorldKnownAtHome).ToList();
  if(knownWorlds.Count==0){GUI.Label(new Rect(84,170,1100,120),"Nothing is known of any other world. No civilisation of your world has yet found an ancient gateway. Explore: the first gateway found, by you or any civilisation of your world, will begin this book.",body);return;}
  if(!knownWorlds.Contains(legendsWorld))legendsWorld=knownWorlds[0];
  for(int r=1;r<=World.RealmCount;r++){var tab=new Rect(84+(r-1)*188,118,182,32);bool known=knownWorlds.Contains(r);if(r==legendsWorld)Fill(new Rect(tab.x-2,tab.y-2,tab.width+4,tab.height+4),UiAccent);bool was=GUI.enabled;GUI.enabled=was&&known;if(GUI.Button(tab,known?World.RealmNames[r]:"Unknown world",button)){legendsWorld=r;legendsScroll=Vector2.zero;GUI.enabled=was;GUIUtility.ExitGUI();}GUI.enabled=was;}
  var peoples=CivilisationCatalog.PeoplesOf(legendsWorld).Where(c=>world.NationOfPeople(c)>=0).ToList();bool told=peoples.Any(c=>world.PeopleDetail(world.NationOfPeople(c))>=2);
  GUILayout.BeginArea(new Rect(84,162,560,570));legendsScroll=GUILayout.BeginScrollView(legendsScroll);
  GUILayout.Label(World.RealmTexts[legendsWorld],body);GUILayout.Space(8);GUILayout.Label("History",heading);GUILayout.Label(told?SealedPeoples.WorldHistories[legendsWorld]:"Its history is unknown. Learn the customs of one of its peoples, and the story of the world comes with it.",body);
  GUILayout.EndScrollView();GUILayout.EndArea();
  float y=162,step=peoples.Count==0?57:Mathf.Min(57,570f/peoples.Count);
  foreach(int civ in peoples){int o=world.NationOfPeople(civ),detail=world.PeopleDetail(o);var row=new Rect(664,y,532,step-5);bool canOpen=detail>=1;
   if(canOpen&&GUI.Button(new Rect(row.x,row.y,row.width-132,row.height),"",button)){legendsPeople=civ;civLoreScroll=Vector2.zero;GUIUtility.ExitGUI();}else if(!canOpen)GUI.Box(new Rect(row.x,row.y,row.width-132,row.height),"");
   float faceH=row.height-8;var face=canOpen?Portrait(CivilisationCatalog.Portraits[civ]):null;var at=new Rect(row.x+6,row.y+4,faceH*.75f,faceH);if(face!=null)DrawPortrait(at,face);else Fill(at,new Color(.02f,.02f,.04f));var e=CivLore.Of(civ);
   GUI.Label(new Rect(row.x+48,row.y+3,350,22),canOpen?CivilisationCatalog.Names[civ]:"An unknown people",hudLabel);GUI.Label(new Rect(row.x+48,row.y+24,350,20),detail>=2&&e!=null?e.Epithet:detail==1?"Little is known of them.":"Only rumour.",small);
   if(detail<3){string reason=world.SpyReason(o);bool was=GUI.enabled;GUI.enabled=was&&reason=="";if(GUI.Button(new Rect(row.xMax-126,row.y,126,row.height),"Send agents\n"+world.SpyPrice(o)+" gold",button)){world.SendAgents(o);status="Your agents have returned.";GUI.enabled=was;GUIUtility.ExitGUI();}GUI.enabled=was;}
   y+=step;}
 }
 void DrawWorldSwitcher(){
  if(!world.HasRealms)return;
  var panel=new Rect(800,0,350,500);GUI.Box(panel,"");float y=10;
  GUI.Label(new Rect(panel.x+14,y,panel.width-28,28),"Worlds",heading);y+=32;
  for(int r=0;r<=World.RealmCount;r++){
   bool known=world.RealmDiscovered(0,r);if(!known)continue;
   var row=new Rect(panel.x+12,y,panel.width-24,30);if(r==viewRealm)Fill(new Rect(row.x-4,row.y-2,row.width+8,row.height+4),UiAccent);
   if(GUI.Button(row,(r==viewRealm?"▸ ":"")+World.RealmNames[r]+(r==viewRealm?" (on the map)":""),button)){ShowRealm(r);GUIUtility.ExitGUI();}
   y+=36;
  }
  // What the world on the map is like.
  y+=4;GUI.Label(new Rect(panel.x+14,y,panel.width-28,22),World.RealmNames[viewRealm],hudTitle);y+=24;GUI.Label(new Rect(panel.x+14,y,panel.width-28,70),World.RealmTexts[viewRealm],new GUIStyle(small){wordWrap=true});y+=74;
  int hidden=Enumerable.Range(1,world.RealmsHere).Count(r=>!world.RealmDiscovered(0,r));
  if(hidden>0){GUI.Label(new Rect(panel.x+14,y,panel.width-28,54),hidden+" parallel world"+(hidden==1?" is":"s are")+(world.gatesLocked?" sealed behind the ancient gateways.":" still unknown. Find an ancient gateway and move a land unit onto it to cross over."),new GUIStyle(small){wordWrap=true});y+=58;}
  int gates=world.tiles.Count(t=>t.realm==0&&t.gateLink>0&&t.seen);GUI.Label(new Rect(panel.x+14,y,panel.width-28,22),"Ancient gateways found: "+gates,small);y+=28;
  if(GUI.Button(new Rect(panel.x+12,y,panel.width-24,34),"Legends of the sealed worlds",button)){legendsOpen=true;legendsScroll=Vector2.zero;GUIUtility.ExitGUI();}
 }
 // A banner on the map while you look at a parallel world.
 void DrawRealmBanner(){
  if(world==null||viewRealm==0)return;var r=new Rect(472,708-tileCardGrow,336,30);
  Fill(r,new Color(.18f,.06f,.2f,.92f));Fill(new Rect(r.x,r.y,r.width,2),UiAccent);
  GUI.Label(new Rect(r.x+8,r.y+4,200,22),"In the "+World.RealmNames[viewRealm],hudLabel);
  if(GUI.Button(new Rect(r.xMax-118,r.y+3,112,24),"Home world",button)){ShowRealm(0);}
 }
 // When one of your units comes through a gateway the map follows it into the other world.
 void FollowGateArrival(){
  if(world==null||world.lastGateArrival<0)return;int r=world.lastGateArrival;world.lastGateArrival=-1;
  var u=Selected();if(u!=null&&world.RealmOf(u.x,u.y)==r)FocusTile(u.x,u.y);else ShowRealm(r);
 }
}
}
