using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Paradigm Realms Live interface after the owner's direction paper ("Proposed main interface"; layout and wording only, no
// third-party pictures were used): three levels of view (the map; the eight management categories; each category's detailed
// screen), a World events panel with dated, coloured entries and an "N new" badge, and a top bar of Year and Season, Gold, Food,
// Iron, Faith and Stability.
public partial class RealmGame {
 // The eight management categories, in the paper's order. Each opens the existing detailed screen that covers it.
 static readonly string[] CategoryNames={"Realm","Population","Economy","Politics","Diplomacy","Military","Research","Religion"};
 static readonly string[] CategoryScreens={"Economy","Cities","Citizens","Civics","Diplomacy","Military","Research","Religion"};
 static readonly string[] CategoryArt={"crown","population","trade","politics","diplomacy","military","research","religion"};
 static readonly string[] CategoryTips={
  "Realm: your empire at a glance, priorities, budget, flag and identity",
  "Population: every city, its people, growth and advisers",
  "Economy: luxuries, citizens at work and specialists",
  "Politics: the court, society, finances and what is remembered",
  "Diplomacy: leaders, treaties, opinion and deals",
  "Military: armies, active and idle units",
  "Research: the technology tree",
  "Religion: your faith, beliefs and the faiths of the world"};

 // ---------- World events ----------
 static readonly Color[] EventColours={new Color(.86f,.78f,.55f),new Color(.92f,.32f,.26f),new Color(.95f,.62f,.25f),new Color(.72f,.55f,.95f),new Color(.45f,.7f,.98f),new Color(.45f,.85f,.42f)};
 WorldEvent eventsSeen;World eventsSeenWorld;
 const int EventRows=5;
 Rect EventsRect{get{float top=MetRivals().Count>0?150:46;int n=world.events==null?0:Math.Min(EventRows,world.events.Count);return new Rect(1040,top,234,40+Math.Max(1,n)*34+(string.IsNullOrEmpty(status)?0:30));}}
 int NewEvents(){
  if(world.events==null||world.events.Count==0)return 0;
  if(eventsSeenWorld!=world){eventsSeenWorld=world;eventsSeen=world.events[0];}
  int n=world.events.IndexOf(eventsSeen);return n<0?world.events.Count:n;
 }
 void MarkEventsSeen(){if(world.events!=null&&world.events.Count>0){eventsSeen=world.events[0];eventsSeenWorld=world;}}
 void DrawEventsPanel(){
  var r=EventsRect;GUI.Box(r,"");var e=Event.current;
  var title=new GUIStyle(hudLabel){fontSize=14};if(displayFont!=null)title.font=displayFont;title.normal.textColor=UiGold;
  GUI.Label(new Rect(r.x+12,r.y+7,140,22),"WORLD EVENTS",title);
  int fresh=NewEvents();
  if(fresh>0){string badge=fresh+" new";var bs=new GUIStyle(hudCentre){fontSize=11};float w=bs.CalcSize(new GUIContent(badge)).x+12;var b=new Rect(r.xMax-w-10,r.y+9,w,18);Fill(b,new Color(.72f,.17f,.13f));GUI.Label(b,badge,bs);}
  Fill(new Rect(r.x+10,r.y+31,r.width-20,1),new Color(UiGold.r,UiGold.g,UiGold.b,.5f));
  float y=r.y+35;
  // What just happened or why an order failed stays on top, as before.
  if(!string.IsNullOrEmpty(status)){DrawIcon(PixelArt.IconHelp,new Rect(r.x+10,y+4,18,18));GUI.Label(new Rect(r.x+32,y,r.width-40,28),status,hudSmall);y+=30;}
  var list=world.events??new List<WorldEvent>();
  var date=new GUIStyle(hudSmall){fontSize=11,wordWrap=false,fontStyle=FontStyle.Bold};
  for(int i=0;i<EventRows&&i<list.Count;i++){
   var ev=list[i];var row=new Rect(r.x+6,y,r.width-12,32);if(row.Contains(e.mousePosition))Fill(row,new Color(1,1,1,.05f));
   var colour=EventColours[Mathf.Clamp(ev.kind,0,EventColours.Length-1)];Fill(new Rect(row.x+4,row.y+4,3,row.height-8),colour);
   date.normal.textColor=colour;GUI.Label(new Rect(row.x+12,row.y,60,14),world.ShortDay(ev.day)+(i<fresh?"  •":""),date);
   GUI.Label(new Rect(row.x+12,row.y+12,row.width-16,20),ev.text,new GUIStyle(hudSmall){fontSize=11,wordWrap=false,clipping=TextClipping.Clip});
   if(row.Contains(e.mousePosition))eventTip=ev.text;
   y+=34;
  }
  if(list.Count==0)GUI.Label(new Rect(r.x+12,y,r.width-24,40),"Nothing has happened yet. Start the clock with Space.",hudSmall);
  if(GUI.Button(r,"",GUIStyle.none)){MarkEventsSeen();OpenDetails("Chronicle");}
 }
 string eventTip;
 void DrawEventTip(){
  if(eventTip==null)return;var e=Event.current;var st=new GUIStyle(hudSmall){wordWrap=true};float w=300,h=st.CalcHeight(new GUIContent(eventTip),w-12)+8;
  var tr=new Rect(EventsRect.x-w-6,Mathf.Min(e.mousePosition.y-h/2,790-h),w,h);Fill(tr,new Color(.05f,.08f,.16f,.96f));Fill(new Rect(tr.x,tr.yMax-1,tr.width,1),UiGold);GUI.Label(new Rect(tr.x+6,tr.y+4,w-12,h-8),eventTip,st);eventTip=null;
 }

 // ---------- the eight categories (the left menu) ----------
 static Rect CategoryRect{get{return new Rect(0,44,166,CategoryNames.Length*44+8);}}
 void DrawCategories(){
  var rail=CategoryRect;GUI.Box(rail,"");var e=Event.current;string tip=null;
  int offers=world.offers.Count(o=>o.to==0&&o.status==0&&o.expires>world.turn);
  var label=new GUIStyle(hudLabel){fontSize=14,alignment=TextAnchor.MiddleLeft};if(displayFont!=null)label.font=displayFont;
  for(int i=0;i<CategoryNames.Length;i++){
   var r=new Rect(rail.x+5,rail.y+5+i*44,rail.width-10,41);bool over=r.Contains(e.mousePosition)&&GUI.enabled;
   DrawButtonFace(r,over);var art=UiTex("icon-"+CategoryArt[i]);var ir=new Rect(r.x+14,r.y+5,r.height-10,r.height-10);
   if(art!=null)GUI.DrawTexture(ir,art,ScaleMode.ScaleToFit,true);else DrawIcon(PixelArt.IconCivics,ir);
   label.normal.textColor=over?new Color(1f,.9f,.6f):UiText;FitLabel(new Rect(ir.xMax+6,r.y,r.xMax-ir.xMax-30,r.height),CategoryNames[i],label,10);
   if(over)tip=CategoryTips[i];
   if(GUI.Button(r,"",GUIStyle.none)){if(CategoryScreens[i]=="Economy")empireTab=0;OpenRailScreen(CategoryScreens[i]);}
   if(CategoryScreens[i]=="Diplomacy"&&offers>0){var b=new Rect(r.xMax-24,r.y+4,16,16);Fill(b,new Color(.75f,.18f,.14f));GUI.Label(b,offers.ToString(),hudCentre);}
  }
  if(tip!=null){var st=new GUIStyle(hudSmall){wordWrap=true};float w=240,h=st.CalcHeight(new GUIContent(tip),w-12)+8;var tr=new Rect(rail.xMax+6,Mathf.Clamp(e.mousePosition.y-h/2,44,790-h),w,h);Fill(tr,new Color(.05f,.08f,.16f,.96f));Fill(new Rect(tr.x,tr.yMax-1,tr.width,1),UiGold);GUI.Label(new Rect(tr.x+6,tr.y+4,w-12,h-8),tip,st);}
 }

 // ---------- top bar figures ----------
 // Iron: improved, road-linked iron sources in your lands. Stability: the share of your cities in order (100% with none).
 int hudIron;
 int IronSources(){int n=0;foreach(var t in world.tiles)if(t.owner==0&&t.resource==2&&t.mine)n++;return n;}
 int Stability(){return hudOwnCities==0?100:hudCalm*100/hudOwnCities;}
 string YearAndSeason(){return world.Calendar()+" · "+world.Season;}
}
}
