using System;
using UnityEngine;
namespace ParadigmRealm {
// Stage 1 of Paradigm Realms Live: the world runs on its own clock (LIVE-ARCHITECTURE-SPEC.md §4.4). The End Turn button becomes a
// date plate with Pause, three speeds and "Next month". Space pauses, 1-3 set the speed, Enter runs to the next month.
// The game starts paused; it pauses itself when a leader calls, a decision or discovery prompt opens, or another screen asks for
// an answer. A month's work (the old turn) runs over a few frames while the clock waits.
public partial class RealmGame {
 int clockSpeed;float clockAccum;bool clockToMonth;int lastCalls=-1;
 static readonly float[] SecondsPerDay={0f,.6f,.25f,.08f};
 static readonly string[] SpeedNames={"Paused","Slow","Normal","Fast"};
 bool ClockBlocked(){
  return world==null||mainMenu||nameSetup||pauseMenu||guide||confirmNew||loadScreen||pendingUnit!=-1||turnRunning||world.result!=""||gateScene!=null||PromptOpen()||FaithPromptOpen();
 }
 // Called every frame from Update, before anything else.
 void ClockUpdate(){
  if(world==null)return;if(!world.liveClock)world.liveClock=true;
  // Auto-pause: a leader calls, or a decision waits.
  int calls=world.rivalCalls==null?0:world.rivalCalls.Count;if(lastCalls>=0&&calls>lastCalls){clockSpeed=0;clockToMonth=false;}lastCalls=calls;
  if(world.newContacts!=null&&world.newContacts.Count>0){clockSpeed=0;clockToMonth=false;}
  if(ClockBlocked()){clockAccum=0;return;}
  if(clockSpeed==0&&!clockToMonth)return;
  float perDay=clockToMonth?.03f:SecondsPerDay[clockSpeed];clockAccum+=Mathf.Min(.25f,Time.unscaledDeltaTime);int ticks=0;
  while(clockAccum>=perDay&&ticks<3){
   clockAccum-=perDay;ticks++;
   if(world.TickDay()){clockAccum=0;if(clockToMonth){clockToMonth=false;clockSpeed=0;}turnRunning=true;StartCoroutine(RunTurn());break;}
  }
 }
 void NextMonth(){if(world==null||world.result!=""||turnRunning)return;clockToMonth=true;}
 void SetSpeed(int s){clockSpeed=Mathf.Clamp(s,0,3);clockToMonth=false;}
 void ClockKeys(Event ev){
  if(ev.type!=EventType.KeyDown)return;
  if(ev.keyCode==KeyCode.Space){SetSpeed(clockSpeed==0?2:0);ev.Use();}
  else if(ev.keyCode==KeyCode.Alpha1){SetSpeed(1);ev.Use();}else if(ev.keyCode==KeyCode.Alpha2){SetSpeed(2);ev.Use();}else if(ev.keyCode==KeyCode.Alpha3){SetSpeed(3);ev.Use();}
  else if(ev.keyCode==KeyCode.Return||ev.keyCode==KeyCode.KeypadEnter){NextMonth();ev.Use();}
 }
 // The date plate and controls, drawn where End Turn used to be.
 void DrawClockControls(Rect r){
  GUI.Box(r,"");string date=world.DateText();
  GUI.Label(new Rect(r.x+8,r.y+3,r.width-16,20),date,hudCentre);
  float sw=30,by=r.y+r.height-27,mw=r.width-16-4*sw-4*3;string[] glyph={"II",">",">>",">>>"};var small2=new GUIStyle(button){fontSize=12,wordWrap=false,clipping=TextClipping.Overflow,alignment=TextAnchor.MiddleCenter};
  for(int k=0;k<4;k++){var b=new Rect(r.x+8+k*(sw+3),by,sw,22);bool on=clockSpeed==k&&!clockToMonth;if(on)Fill(new Rect(b.x-1,b.y-1,b.width+2,b.height+2),new Color(1f,.82f,.42f));
   if(GUI.Button(b,new GUIContent(glyph[k],SpeedNames[k]+(k==0?" (Space)":" ("+k+")")),small2))SetSpeed(k);}
  var next=new Rect(r.x+8+4*(sw+3)+1,by,mw,22);if(clockToMonth)Fill(new Rect(next.x-1,next.y-1,next.width+2,next.height+2),new Color(1f,.82f,.42f));
  if(GUI.Button(next,new GUIContent("Next month","Run to the next month, then pause (Enter)"),small2))NextMonth();
  // How far through the month: a thin bar under the date.
  Fill(new Rect(r.x+10,r.y+22,r.width-20,2),new Color(.1f,.12f,.18f));Fill(new Rect(r.x+10,r.y+22,(r.width-20)*world.DayOfMonth/(float)World.DaysPerMonth,2),new Color(1f,.82f,.42f));
 }
}
}
