using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The talk about a place (owner brief, 27 Nov 2026): opened by the leader's call, or by "Land and timber" in the Court. The
// leader says what they want; you can ask why, see the land on the map, accept, answer with your own terms (you settle the
// place and supply timber for a number of turns; or sell timber now), or decline. Every term is shown before anyone commits,
// and a promise to leave land unsettled is never a transfer of ownership.
public partial class RealmGame {
 int valleyTalk=-1,valleyMarkId=-1,counterPer=2,counterTurns=8;bool valleyWhy,valleyMarked;string valleyReply="";
 bool ValleyTalkOpen(){return valleyTalk>=0&&world!=null&&world.IssueById(valleyTalk)!=null;}
 void OpenValleyTalk(int partner){var issue=world.AskAboutValley(partner);if(issue==null){dipReplyPartner=partner;dipReply=world.ValleyNothingToSay(partner);status=dipReply;return;}
  valleyTalk=issue.id;valleyWhy=false;valleyReply="";valleyMarked=false;detailsScreen="";var n=world.AssessValley(partner);counterPer=Mathf.Clamp((n.shortfall+5)/6,1,6);counterTurns=8;ResetMapGesture();}
 // The land in question, outlined on the map while the talk is open or marked.
 void DrawValleyTiles(float s){
  if(!ValleyTalkOpen()&&!valleyMarked)return;var issue=world.IssueById(ValleyTalkOpen()?valleyTalk:valleyMarkId);if(issue==null||issue.site<0)return;var old=GUI.color;float pulse=.6f+.4f*Mathf.Sin(Time.realtimeSinceStartup*3f);
  foreach(var t in world.KeepTiles(issue.site,ValleyTuning.KeepRadius)){float sx,sy;IsoScreen(t.x,t.y,out sx,out sy);sy-=Lift(t.x,t.y,s);GUI.color=t.y*world.Width+t.x==issue.site?new Color(1f,.85f,.3f,.55f+.45f*pulse):new Color(1f,.85f,.3f,.35f);GUI.DrawTexture(TileRect(sx,sy,s),SelectionPicture());}
  GUI.color=old;}
 void DrawValleyTalk(){
  GUI.enabled=true;DiplomacyStyles();HudStyles();var issue=world.IssueById(valleyTalk);if(issue==null){valleyTalk=-1;return;}int o=issue.owner;var nation=world.nations[o];
  Fill(FullCanvas,new Color(0,0,0,.45f));var box=new Rect(150,70,980,660);DipPanel(box,.98f);
  string place=world.PlaceName(o,issue.site);
  GUI.Label(new Rect(box.x,box.y+12,box.width,36),(string.IsNullOrEmpty(nation.leader)?world.NationName(o):nation.leader+" of "+world.NationName(o)).ToUpperInvariant(),dipTitle);
  var face=nation.portrait>=0?Portrait(nation.portrait):null;if(face!=null)DrawPortrait(new Rect(box.x+28,box.y+60,150,200),face);
  var text=new GUIStyle(dipRow){fontSize=15,wordWrap=true,alignment=TextAnchor.UpperLeft};var said=new GUIStyle(text){fontStyle=FontStyle.Italic};var you=new GUIStyle(text){normal={textColor=new Color(.86f,.7f,.39f)}};
  float x=box.x+200,w=box.width-228,y=box.y+62;
  Action<string,GUIStyle> line=(s,st)=>{float h=st.CalcHeight(new GUIContent(s),w);GUI.Label(new Rect(x,y,w,h),s,st);y+=h+8;};
  line("“"+issue.opening+"”",said);
  if(valleyWhy){line("You: “"+(issue.kind==1?"What do you need it for?":"Is it the land you want, or the timber?")+"”",you);line("“"+world.ValleyWhy(issue)+"”",said);}
  if(valleyReply!="")line(valleyReply,said);
  // The terms on the table, exactly.
  y+=4;Fill(new Rect(x,y,w,1),new Color(.86f,.7f,.39f,.6f));y+=8;
  var tiles=world.KeepTiles(issue.site,ValleyTuning.KeepRadius);
  if(issue.status!=0)line("This matter is "+ValleyTuning.Status[issue.status]+".",text);
  else if(issue.kind==1)line("Their terms: you sell "+issue.amount+" timber now (you have "+world.nations[0].timber+"); they pay "+issue.gold+" gold. Nothing else changes.",text);
  else line("Their terms: you promise to leave "+place+" unsettled: "+tiles.Count+" tiles within "+ValleyTuning.KeepRadius+" of it, marked on the map. They pay "+issue.gold+" gold now. It is a promise, not a transfer: the land stays nobody's, and if you settle there you break your word.",text);
  // Buttons.
  float by=box.yMax-214;Color navy=new Color(.1f,.16f,.3f),teal=new Color(.07f,.38f,.4f),red=new Color(.45f,.08f,.07f);float bw=(w-20)/3;
  if(issue.status==0){
   if(!valleyWhy&&DipButton(new Rect(x,by,bw,36),issue.kind==1?"What is it for?":"Land or timber?",navy)){valleyWhy=true;GUIUtility.ExitGUI();}
   if(issue.site>=0&&DipButton(new Rect(x+bw+10,by,bw,36),"Show the land",navy)){valleyMarked=true;valleyMarkId=issue.id;var s=world.tiles[issue.site];FocusTile(s.x,s.y);valleyTalk=-1;status="The land "+world.NationName(o)+" means is marked on the map. Open the talk again from Diplomacy > Land and timber.";GUIUtility.ExitGUI();}
   if(DipButton(new Rect(x+2*(bw+10),by,bw,36),issue.kind==1?"Sell it":"Promise (+"+issue.gold+" gold)",teal)){string why=world.AcceptValley(issue);valleyReply=why==""?"Agreed.":why;GUIUtility.ExitGUI();}
   // Your own terms: settle it yourself and supply timber instead.
   if(issue.kind==2){float cy=by+46;GUI.Label(new Rect(x,cy,186,30),"Or: we settle it and supply",new GUIStyle(text){fontSize=13,alignment=TextAnchor.MiddleLeft,wordWrap=false});
    if(DipButton(new Rect(x+190,cy,30,30),"-",navy))counterPer=Mathf.Max(1,counterPer-1);GUI.Label(new Rect(x+224,cy,80,30),counterPer+" timber",new GUIStyle(text){alignment=TextAnchor.MiddleCenter});if(DipButton(new Rect(x+306,cy,30,30),"+",navy))counterPer=Mathf.Min(10,counterPer+1);
    GUI.Label(new Rect(x+340,cy,68,30),"a turn, for",new GUIStyle(text){fontSize=13,alignment=TextAnchor.MiddleCenter,wordWrap=false});
    if(DipButton(new Rect(x+408,cy,30,30),"-",navy))counterTurns=Mathf.Max(2,counterTurns-1);GUI.Label(new Rect(x+442,cy,70,30),counterTurns+" turns",new GUIStyle(text){alignment=TextAnchor.MiddleCenter});if(DipButton(new Rect(x+514,cy,30,30),"+",navy))counterTurns=Mathf.Min(30,counterTurns+1);
    int each;bool ok;string judged=world.JudgeSupplyCounter(issue,counterPer,counterTurns,out each,out ok);
    var jr=new Rect(x,cy+40,w,52);var js=new GUIStyle(said){fontSize=13};while(js.fontSize>10&&js.CalcHeight(new GUIContent(judged),jr.width)>jr.height)js.fontSize--;GUI.Label(jr,judged,js);
    if(DipButton(new Rect(x+w-200,cy,200,36),"Propose these terms",teal)){valleyReply=world.AgreeSupplyCounter(issue,counterPer,counterTurns);GUIUtility.ExitGUI();}}
   if(DipButton(new Rect(box.x+28,box.yMax-56,180,38),"Decline",red)){world.DeclineValley(issue);valleyReply=world.NationName(o)+": “Then we will do as we must.”";GUIUtility.ExitGUI();}
  }
  if(DipButton(new Rect(box.xMax-28-180,box.yMax-56,180,38),"Close",navy)){valleyTalk=-1;GUIUtility.ExitGUI();}
 }
}
}
namespace ParadigmRealm {
public partial class RealmGame {
 static bool? devReasoning;
 // "-devReasoning" on the command line: the Court shows each leader's reckoning about land and timber (ValleyReasoningText).
 static bool DevReasoning{get{if(devReasoning==null)devReasoning=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-devReasoning")>=0;return devReasoning.Value;}}
}
}
