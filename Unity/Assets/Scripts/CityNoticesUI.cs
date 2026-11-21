using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The message on the map when a city finishes something, and the glowing technology borders.
public partial class RealmGame {
 static Rect NoticeRect{get{return new Rect(452+MidShift,46,380,118);}}
 bool NoticeShown(){return world!=null&&world.buildNotices!=null&&world.buildNotices.Count>0&&world.result=="";}
 void DrawBuildNotice(){
  if(!NoticeShown())return;var n=world.buildNotices[0];var c=world.cities.Find(a=>a.id==n.city&&a.owner==0);if(c==null){world.buildNotices.RemoveAt(0);return;}
  var r=NoticeRect;GUI.Box(r,"");Fill(new Rect(r.x,r.y,r.width,2),UiAccent);var wrap=new GUIStyle(small){wordWrap=true};
  string more=world.buildNotices.Count>1?"  (+"+(world.buildNotices.Count-1)+" more)":"";GUI.Label(new Rect(r.x+12,r.y+6,r.width-24,24),n.text+more,hudCaption);
  int current=n.building?c.buildingProject:c.build;string why;int next=current>=0?-1:world.SuggestNext(c,n.building,out why);why="";if(current<0)world.SuggestNext(c,n.building,out why);
  int other=n.building?c.build:c.buildingProject;
  string line=current>=0?"Now making "+world.BuildNameFor(0,current)+". Open the city to change it.":next>=0?"Next: "+world.BuildNameFor(0,next)+"? "+why:other>=0?"Still making "+world.BuildNameFor(0,other)+". Open the city to add more.":"Nothing is queued on this line. Open the city to choose.";
  GUI.Label(new Rect(r.x+12,r.y+32,r.width-24,40),line,wrap);
  float y=r.yMax-36;
  // The buttons share the panel width and the first one's text shrinks to fit (owner report, 21 Nov 2026: it wrapped and was cut).
  float okW=80,openW=104,makeW=r.width-20-okW-openW-12;
  if(next>=0&&FitButton(new Rect(r.x+10,y,makeW,28),"Make "+world.BuildNameFor(0,next))){world.ChooseProject(c,next,false);world.SettleBuilding(c);status=c.name+" will make "+world.BuildNameFor(0,next)+".";world.buildNotices.RemoveAt(0);GUIUtility.ExitGUI();}
  if(FitButton(new Rect(r.xMax-10-okW-6-openW,y,openW,28),"Open city")){world.buildNotices.RemoveAt(0);OpenCity(c);GUIUtility.ExitGUI();}
  if(FitButton(new Rect(r.xMax-10-okW,y,okW,28),"OK")){world.buildNotices.RemoveAt(0);GUIUtility.ExitGUI();}
 }
 // Another leader calling: the portrait, what they want, and the way to answer.
 static Rect CallRect{get{return new Rect(452+MidShift,170,380,76);}}
 bool CallShown(){return world!=null&&world.result==""&&world.NextRivalCall()!=null;}
 void DrawRivalCall(){
  var call=world.NextRivalCall();if(call==null||world.result!="")return;var r=CallRect;GUI.Box(r,"");Fill(new Rect(r.x,r.y,r.width,2),GlowGold);
  var n=world.nations[call.from];var face=n.portrait>=0?Portrait(n.portrait):null;if(face!=null)DrawPortrait(new Rect(r.x+8,r.y+8,45,60),face);
  int more=world.rivalCalls.Count-1;GUI.Label(new Rect(r.x+62,r.y+6,r.width-70,24),world.NationName(call.from)+(more>0?"  (+"+more+" more)":""),hudCaption);
  GUI.Label(new Rect(r.x+62,r.y+26,r.width-70,20),(string.IsNullOrEmpty(n.leader)?"Their leader":n.leader)+" "+call.text,small);
  if(GUI.Button(new Rect(r.x+62,r.y+46,150,24),call.offer>0?"Answer":"Open Diplomacy",button)){world.rivalCalls.Remove(call);diplomacyPartner=call.from;diplomacyChooser=false;diplomacyDiscuss=false;detailsScreen="Diplomacy";researchScreen=false;detailsScroll=Vector2.zero;ResetMapGesture();GUIUtility.ExitGUI();}
  if(GUI.Button(new Rect(r.x+218,r.y+46,90,24),"Later",button)){world.rivalCalls.Remove(call);GUIUtility.ExitGUI();}
 }
 // A glowing outline: gold on a technology you know, silver on the one you are researching. It breathes slowly.
 static readonly Color GlowGold=new Color(1f,.82f,.32f),GlowSilver=new Color(.84f,.88f,.96f);
 void GlowBorder(Rect r,Color ink){
  float pulse=.78f+.22f*Mathf.Sin(Time.realtimeSinceStartup*2.4f);
  for(int k=4;k>=0;k--){
   float grow=k*1.5f,a=(k==0?1f:.62f/(k*.8f))*pulse;var g=new Rect(r.x-grow,r.y-grow,r.width+2*grow,r.height+2*grow);var c=new Color(ink.r,ink.g,ink.b,a);float t=k==0?2f:1.5f;
   Fill(new Rect(g.x,g.y,g.width,t),c);Fill(new Rect(g.x,g.yMax-t,g.width,t),c);Fill(new Rect(g.x,g.y+t,t,g.height-2*t),c);Fill(new Rect(g.xMax-t,g.y+t,t,g.height-2*t),c);
  }
 }
}
}
namespace ParadigmRealm {
public partial class RealmGame {
 // A button whose label is never cut (owner request, 21 Nov 2026: all text must be visible): the font shrinks, down to 10, until
 // the label fits on one line; only then does it wrap onto two smaller lines.
 readonly System.Collections.Generic.Dictionary<string,GUIStyle> fitButtonStyles=new System.Collections.Generic.Dictionary<string,GUIStyle>();
 bool FitButton(Rect r,string text,GUIStyle style=null){
  style=style??button;string key=style.GetHashCode()+"|"+(int)r.width+"|"+(int)r.height+"|"+text;GUIStyle s;
  if(!fitButtonStyles.TryGetValue(key,out s)){s=new GUIStyle(style){wordWrap=false,clipping=TextClipping.Overflow};var content=new GUIContent(text);
   while(s.fontSize==0?false:s.fontSize>10&&s.CalcSize(content).x>r.width-12)s.fontSize--;
   if(s.fontSize==0){s.fontSize=style.font!=null?style.font.fontSize:14;while(s.fontSize>10&&s.CalcSize(content).x>r.width-12)s.fontSize--;}
   if(s.CalcSize(content).x>r.width-12){s.wordWrap=true;while(s.fontSize>9&&s.CalcHeight(content,r.width-12)>r.height-2)s.fontSize--;}
   if(fitButtonStyles.Count>400)fitButtonStyles.Clear();fitButtonStyles[key]=s;}
  return GUI.Button(r,text,s);
 }
}
}
namespace ParadigmRealm {
public partial class World {internal void NoticeForShot(City c,int b){AddBuildNotice(c,b,true,"built");}}
}
