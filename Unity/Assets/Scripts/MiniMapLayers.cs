using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Minimap layers (owner request, 20 Nov 2026: "an option to display certain features on the mini map, like primary trade
// routes, prominent religion"). A tab on top of the minimap opens the list; the choice is remembered between games.
//  Terrain    - the land as before.
//  Political  - plain nation colours.
//  Religion   - each city's land in the colour of the city's main faith (grey: none yet).
//  Trade      - roads and river links that join a civilisation's cities to its capital in gold, other roads dim; dotted gold
//               lines from your capital to the capitals of your commercial-pact partners.
//  Diplomacy  - everyone's land coloured by how they regard you.
//  Military   - the units you can see: yours, theirs, enemies at war in red, barbarian raiders and camps.
// Only what you know is shown: unexplored land stays dark and foreign units only where you can see them.
public partial class RealmGame {
 static readonly string[] MiniLayerNames={"Terrain","Political","Religion","Trade routes","Diplomacy","Military","Traditions"};
 int miniLayer=-1,miniLayerBuilt=-1;bool miniLayerOpen,miniKeyAlways;// miniKeyAlways: -hudShot shows the key without a pointer
 int MiniLayer{get{if(miniLayer<0){try{miniLayer=Mathf.Clamp(PlayerPrefs.GetInt("MiniMapLayer2",1),0,MiniLayerNames.Length-1);}catch{miniLayer=0;}}return miniLayer;}
  set{miniLayer=value;try{PlayerPrefs.SetInt("MiniMapLayer2",value);}catch{}}}
 static Rect MiniLayerTab{get{return new Rect(MiniMap.x,MiniMap.y-26,MiniMap.width,24);}}
 static Rect MiniLayerList{get{return new Rect(MiniMap.x,MiniMap.y-26-MiniLayerNames.Length*24-4,MiniMap.width,MiniLayerNames.Length*24+4);}}
 readonly Dictionary<int,City> miniCityLand=new Dictionary<int,City>();
 static Color MiniWater{get{return MiniSea;}}static Color MiniWild{get{return MiniLand;}}
 // Before the minimap is painted: which city each owned tile belongs to (the nearest of its owner's), for the religion layer.
 void PrepareMiniLayer(){
  miniCityLand.Clear();if(MiniLayer!=2&&MiniLayer!=6)return;var best=new Dictionary<int,int>();
  foreach(var c in world.cities){var ct=world.At(c.x,c.y);if(ct==null||ct.realm!=viewRealm)continue;int r=world.TerritoryRadius(c)+1;
   for(int dy=-r;dy<=r;dy++)for(int dx=-r;dx<=r;dx++){var t=world.StepIn(c.x,c.y,dx,dy,r);if(t==null||t.owner!=c.owner)continue;int d=world.Distance(c.x,c.y,t.x,t.y);if(d>r)continue;int cell=t.y*world.Width+t.x;int had;
    if(!best.TryGetValue(cell,out had)||d<had){best[cell]=d;miniCityLand[cell]=c;}}}
 }
 static Color Dim(Color c,float k){return new Color(c.r*k,c.g*k,c.b*k,1);}
 Color NationInk(int o){var n=nations[o];return new Color(n.r,n.g,n.b,1);}
 // The colour of one known tile on the chosen layer; terrain is what the Terrain layer would show there.
 Color MiniLayerColour(int cell,Tile t,Color terrain){
  bool water=PixelArt.IsWater(t.terrain)&&t.terrain!=7;
  switch(MiniLayer){
   case 2:{if(t.owner<0)return water?MiniWater:MiniWild;City c;return miniCityLand.TryGetValue(cell,out c)?FaithColour(c.religion>=0?c.religion:-1):new Color(.55f,.55f,.58f);}
   case 3:{var basic=water?Dim(MiniWater,.8f):t.owner>=0?Dim(Color.Lerp(NationInk(t.owner),terrain,.5f),.55f):Dim(terrain,.45f);
    if(t.road||t.terrain==7&&t.owner>=0){bool linked=t.owner>=0&&world.OnTradeNetwork(t.owner,cell);return linked?new Color(1f,.82f,.3f):Color.Lerp(basic,new Color(.75f,.62f,.42f),.6f);}
    return basic;}
   case 4:{if(t.owner<0)return water?MiniWater:MiniWild;return Color.Lerp(MiniUnknown,DiplomacyInk(t.owner),.7f);}
   case 5:return water?Dim(MiniWater,.8f):t.owner>=0?Dim(Color.Lerp(NationInk(t.owner),terrain,.6f),.5f):Dim(terrain,.45f);
   case 6:{if(t.owner<0)return water?MiniWater:MiniWild;City c;if(!miniCityLand.TryGetValue(cell,out c)||!world.lifeways)return new Color(.55f,.55f,.58f);int way=world.WayOf(c).tradition;return way>=0?WayInk[way]:new Color(.55f,.55f,.58f);}
  }
  return terrain;
 }
 static readonly Color InkYou=new Color(.3f,.6f,1f),InkFriend=new Color(.35f,.78f,.4f),InkNeutral=new Color(.62f,.62f,.6f),InkWary=new Color(.9f,.78f,.3f),InkCold=new Color(.95f,.52f,.22f),InkWar=new Color(.88f,.2f,.18f),InkUnmet=new Color(.42f,.42f,.46f);
 Color DiplomacyInk(int o){
  if(o==0)return InkYou;if(o>=world.civCount||!world.met[o])return InkUnmet;
  int a=world.Attitude(o,0);
  if(a==Attitudes.AtWar)return InkWar;if(a==Attitudes.Rival||a==Attitudes.Hostile||a==Attitudes.Unfriendly)return InkCold;
  if(a==Attitudes.Loyal||a==Attitudes.Friendly||a==Attitudes.Cordial)return InkFriend;
  if(a==Attitudes.Wary||a==Attitudes.Threatened||a==Attitudes.Domineering)return InkWary;return InkNeutral;
 }
 // Marks painted after the cities: units on the Military layer.
 void PaintMiniOverlay(int tw,int th,int scale,int top){
  if(MiniLayer!=5)return;float rowPixels=scale*MiniRowShare();
  Action<int,int,int,Color32> dot=(x,y,r,ink)=>{int cx=(int)((x+.5f+MiniShift(y))*scale),cy=th-1-(int)((y-top+.5f)*rowPixels);for(int dy=-r;dy<=r;dy++)for(int dx=-r;dx<=r;dx++){if(dx*dx+dy*dy>r*r)continue;int qx=cx+dx,qy=cy+dy;if(qx<0||qy<0||qx>=tw||qy>=th)continue;miniPixels[qy*tw+qx]=ink;}};
  int r0=Math.Max(1,scale/2+1);
  foreach(var u in world.units){var t=world.At(u.x,u.y);if(t==null||t.realm!=viewRealm||!(u.owner==0||world.Visible(u.x,u.y)))continue;
   Color ink=u.owner==0?InkYou:world.AtWar(0,u.owner)?InkWar:NationInk(u.owner);if(World.Power[u.type]==0)ink=Color.Lerp(ink,Color.white,.45f);dot(u.x,u.y,r0,ink);}
  if(world.barbarians&&viewRealm==0){
   if(world.camps!=null)foreach(var c in world.camps)if(c.seen||world.Visible(c.x,c.y)){dot(c.x,c.y,r0+1,new Color32(30,10,8,255));dot(c.x,c.y,r0,new Color32(150,30,24,255));}
   if(world.raiders!=null)foreach(var r in world.raiders)if(world.Visible(r.x,r.y))dot(r.x,r.y,r0,new Color32(150,30,24,255));}
 }
 // The tab, the list and the key, drawn with the minimap.
 void DrawMiniLayerControls(Rect drawn,float sx,float sy){
  var e=Event.current;var tab=MiniLayerTab;
  // Trade: dotted gold lines from your capital to your commercial-pact partners' capitals that you have seen.
  if(MiniLayer==3){var home=world.CapitalCity(0);if(home!=null&&world.At(home.x,home.y).realm==viewRealm)for(int o=1;o<world.civCount;o++){if(!world.met[o]||!world.Pact(0,o,Pacts.Commercial))continue;var cap=world.CapitalCity(o);if(cap==null)continue;var ct=world.At(cap.x,cap.y);if(ct.realm!=viewRealm||!ct.seen)continue;
   Vector2 a=MiniPoint(drawn,home.x,home.y),b=MiniPoint(drawn,cap.x,cap.y);float len=(b-a).magnitude;
   for(float d=0;d<len;d+=5){var p=Vector2.Lerp(a,b,d/Math.Max(1,len));Fill(new Rect(p.x-1,p.y-1,3,3),new Color(1f,.85f,.35f,.95f));}}}
  // The tab.
  Fill(tab,new Color(.05f,.08f,.15f,.95f));Fill(new Rect(tab.x,tab.y,tab.width,1),UiGold);Fill(new Rect(tab.x,tab.yMax-1,tab.width,1),new Color(UiGold.r,UiGold.g,UiGold.b,.5f));
  GUI.Label(new Rect(tab.x+8,tab.y,tab.width-30,tab.height),"Map: "+MiniLayerNames[MiniLayer],new GUIStyle(hudSmall){fontSize=13,alignment=TextAnchor.MiddleLeft,wordWrap=false});
  {float cx=tab.xMax-14,cy=tab.center.y;for(int i=0;i<4;i++){float row=miniLayerOpen?cy+1-i:cy-2+i;Fill(new Rect(cx-(miniLayerOpen?i:3-i)-.5f,row,2*(miniLayerOpen?i:3-i)+1,1),UiGold);}}
  if(e.type==EventType.MouseDown&&e.button==0&&tab.Contains(e.mousePosition)&&GUI.enabled){miniLayerOpen=!miniLayerOpen;e.Use();}
  if(miniLayerOpen){var list=MiniLayerList;Fill(list,new Color(.04f,.06f,.12f,.97f));Fill(new Rect(list.x,list.y,list.width,1),UiGold);
   for(int k=0;k<MiniLayerNames.Length;k++){var row=new Rect(list.x+2,list.y+2+k*24,list.width-4,23);bool hover=row.Contains(e.mousePosition);
    if(k==MiniLayer||hover)Fill(row,k==MiniLayer?new Color(UiGold.r,UiGold.g,UiGold.b,.25f):new Color(1,1,1,.08f));
    GUI.Label(new Rect(row.x+10,row.y,row.width-12,row.height),MiniLayerNames[k],new GUIStyle(hudSmall){fontSize=13,alignment=TextAnchor.MiddleLeft,wordWrap=false});
    if(e.type==EventType.MouseDown&&e.button==0&&hover&&GUI.enabled){MiniLayer=k;miniLayerOpen=false;miniBuilt=-99;e.Use();}}
   if(e.type==EventType.MouseDown&&!list.Contains(e.mousePosition)&&!tab.Contains(e.mousePosition))miniLayerOpen=false;}
  // The key, to the left of the minimap while the pointer is over it or its tab.
  if(MiniLayer>0&&!miniLayerOpen&&(miniKeyAlways||MiniMap.Contains(e.mousePosition)||tab.Contains(e.mousePosition)))DrawMiniKey();
 }
 void DrawMiniKey(){
  var rows=new List<KeyValuePair<Color,string>>();
  switch(MiniLayer){
   case 1:foreach(int o in world.cities.Where(c=>{var ct=world.At(c.x,c.y);return ct!=null&&ct.realm==viewRealm&&(c.owner==0||ct.seen);}).GroupBy(c=>c.owner).OrderBy(g=>g.Key==0?0:1).ThenByDescending(g=>g.Count()).Take(8).Select(g=>g.Key))rows.Add(new KeyValuePair<Color,string>(NationInk(o),o==0?world.NationName(0)+" (you)":world.met[o]?world.NationName(o):"An unknown people"));rows.Add(new KeyValuePair<Color,string>(MiniLand,"Unclaimed land"));break;
   case 2:{var seen=world.cities.Where(c=>{var t=world.At(c.x,c.y);return t!=null&&t.realm==viewRealm&&(c.owner==0||t.seen);}).GroupBy(c=>c.religion).OrderByDescending(g=>g.Count()).Take(6);
    foreach(var g in seen)rows.Add(new KeyValuePair<Color,string>(FaithColour(g.Key>=0?g.Key:-1),(g.Key>=0?world.FaithName(g.Key):"No main faith yet")+" ("+g.Count()+(g.Count()==1?" city)":" cities)")));break;}
   case 3:rows.Add(new KeyValuePair<Color,string>(new Color(1f,.82f,.3f),"Roads and rivers joining cities to their capital"));rows.Add(new KeyValuePair<Color,string>(new Color(.75f,.62f,.42f),"Other roads"));rows.Add(new KeyValuePair<Color,string>(new Color(1f,.85f,.35f),"Dotted: your commercial pacts"));break;
   case 4:rows.Add(new KeyValuePair<Color,string>(InkYou,"You"));rows.Add(new KeyValuePair<Color,string>(InkFriend,"Friendly"));rows.Add(new KeyValuePair<Color,string>(InkNeutral,"Neutral"));rows.Add(new KeyValuePair<Color,string>(InkWary,"Wary or threatened"));rows.Add(new KeyValuePair<Color,string>(InkCold,"Unfriendly or rival"));rows.Add(new KeyValuePair<Color,string>(InkWar,"At war with you"));rows.Add(new KeyValuePair<Color,string>(InkUnmet,"Not yet met"));break;
   case 6:for(int way=0;way<Lifeways.Count;way++)rows.Add(new KeyValuePair<Color,string>(WayInk[way],Lifeways.Names[way]));rows.Add(new KeyValuePair<Color,string>(new Color(.55f,.55f,.58f),"No settled tradition"));break;
   case 5:rows.Add(new KeyValuePair<Color,string>(InkYou,"Your units"));rows.Add(new KeyValuePair<Color,string>(InkWar,"Enemies at war, barbarians"));rows.Add(new KeyValuePair<Color,string>(new Color(.85f,.85f,.85f),"Others' units (pale: civilians)"));break;
  }
  if(rows.Count==0)return;var st=new GUIStyle(hudSmall){fontSize=12,alignment=TextAnchor.MiddleLeft,wordWrap=false,clipping=TextClipping.Overflow};float w=24+rows.Max(r=>st.CalcSize(new GUIContent(r.Value)).x)+12,h=rows.Count*20+10;
  var box=new Rect(MiniMap.xMax-w,MiniLayerTab.y-h-6,w,h);Fill(box,new Color(.04f,.06f,.12f,.95f));Fill(new Rect(box.x,box.y,box.width,1),UiGold);
  for(int i=0;i<rows.Count;i++){Fill(new Rect(box.x+8,box.y+10+i*20,12,10),rows[i].Key);GUI.Label(new Rect(box.x+26,box.y+5+i*20,w-28,20),rows[i].Value,st);}
 }
}
public partial class World {
 public bool OnTradeNetwork(int owner,int cell){return owner>=0&&owner<civCount&&Network(owner).Contains(cell);}
 public City CapitalCity(int owner){return owner>=0&&owner<civCount?CapitalOf(owner):null;}
 // -hudShot: the home world explored as if walked, so remembered tiles show what is really there.
 internal void LearnHomeWorldForShots(){foreach(var t in tiles)if(t.realm==0)LearnTile(0,t);}
}
}
