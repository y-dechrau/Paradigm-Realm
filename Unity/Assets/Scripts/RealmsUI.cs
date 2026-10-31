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
  viewRealm=r;atlasTexture=null;FocusTile(x,y);
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
  int hidden=Enumerable.Range(1,World.RealmCount).Count(r=>!world.RealmDiscovered(0,r));
  if(hidden>0){GUI.Label(new Rect(panel.x+14,y,panel.width-28,54),hidden+" parallel world"+(hidden==1?" is":"s are")+" still unknown. Find an ancient gateway and move a land unit onto it to cross over.",new GUIStyle(small){wordWrap=true});y+=58;}
  int gates=world.tiles.Count(t=>t.realm==0&&t.gateLink>0&&t.seen);GUI.Label(new Rect(panel.x+14,y,panel.width-28,22),"Ancient gateways found: "+gates,small);
 }
 // A banner on the map while you look at a parallel world.
 void DrawRealmBanner(){
  if(world==null||viewRealm==0)return;var r=new Rect(472,708,336,30);
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
