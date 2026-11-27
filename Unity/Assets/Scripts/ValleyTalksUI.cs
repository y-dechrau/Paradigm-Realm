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
 void OpenValleyTalk(int partner){var issue=world.AskAboutValley(partner);if(issue==null){OpenCompose(partner);composeReply=world.ValleyNothingToSay(partner);return;}
  valleyTalk=issue.id;valleyWhy=false;valleyReply="";valleyMarked=false;detailsScreen="";var n=world.AssessValley(partner);counterPer=Mathf.Clamp((n.shortfall+5)/6,1,6);counterTurns=8;ResetMapGesture();}
 // The land in question, outlined on the map while the talk is open or marked.
 void DrawValleyTiles(float s){
  if(composeWith>0&&composeArea.Count>0&&(composePicking||ComposeOpen())){var old2=GUI.color;float p2=.6f+.4f*Mathf.Sin(Time.realtimeSinceStartup*3f);
   foreach(int c in composeArea){var t=world.tiles[c];if(t.realm!=viewRealm)continue;float sx,sy;IsoScreen(t.x,t.y,out sx,out sy);sy-=Lift(t.x,t.y,s);GUI.color=new Color(1f,.85f,.3f,composePicking?.45f+.4f*p2:.6f);GUI.DrawTexture(TileRect(sx,sy,s),SelectionPicture());}
   GUI.color=old2;return;}
  int markSite=composeWith>0&&composeSite>=0?composeSite:-1;
  if(markSite<0){if(!ValleyTalkOpen()&&!valleyMarked)return;var issue=world.IssueById(ValleyTalkOpen()?valleyTalk:valleyMarkId);if(issue==null||issue.site<0)return;markSite=issue.site;}var old=GUI.color;float pulse=.6f+.4f*Mathf.Sin(Time.realtimeSinceStartup*3f);
  foreach(var t in world.KeepTiles(markSite,ValleyTuning.KeepRadius)){float sx,sy;IsoScreen(t.x,t.y,out sx,out sy);sy-=Lift(t.x,t.y,s);GUI.color=t.y*world.Width+t.x==markSite?new Color(1f,.85f,.3f,.55f+.45f*pulse):new Color(1f,.85f,.3f,.35f);GUI.DrawTexture(TileRect(sx,sy,s),SelectionPicture());}
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
  if(DipButton(new Rect(box.xMax-28-180-200,box.yMax-56,190,38),"Your own terms",navy)){OpenCompose(o);composeSite=issue.site;composeArea=world.ProposalArea(issue.site,null);GUIUtility.ExitGUI();}
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
namespace ParadigmRealm {
// Your own proposal (owner request, 27 Nov 2026): choose what you propose, the place (picked on the map), the amounts and the gold,
// read the leader's answer before committing, then propose. Opened from Court > Land and timber, or from a talk in progress.
public partial class RealmGame {
 int composeWith=-1,composeKind,composeSite=-1,composeAmount=3,composeTurns=8,composeGold=20;bool composePicking;string composeReply="";
 // The land you deal for is painted on the map (owner request, 27 Nov 2026): press on a tile and drag across the land you want;
 // starting on a chosen tile and dragging removes tiles instead. Land only, at most ProposalKinds.MaxArea tiles. Right-click,
 // Done or Escape finishes; the middle button still pans the map and the wheel still zooms.
 System.Collections.Generic.List<int> composeArea=new System.Collections.Generic.List<int>();bool composePainting,composeErase;Vector2 composeLastMouse;
 string composeJudgeKey,composeJudged;bool composeJudgedOk;
 Rect ComposeBarRect{get{return new Rect(MapW/2-300,140,600,54);}}
 bool ComposeAreaInput(Event e,bool inside,int x,int y){
  if(!composePicking||composeWith<=0)return false;
  if(!composePainting&&ComposeBarRect.Contains(e.mousePosition))return true;// the bar's own buttons take it
  if(e.type==EventType.MouseDown&&e.button==1){FinishComposeArea();e.Use();return true;}
  if(e.type==EventType.MouseDown&&e.button==0){if(inside){composeErase=composeArea.Contains(y*world.Width+x);composePainting=true;composeLastMouse=e.mousePosition;PaintCompose(x,y);}e.Use();return true;}
  if(composePainting&&e.type==EventType.MouseDrag&&e.button==0){
   // Sampled along the stroke, so a quick drag leaves no gaps.
   var from=composeLastMouse;var to=e.mousePosition;int steps=Mathf.Max(1,Mathf.CeilToInt((to-from).magnitude/5f));
   for(int k=1;k<=steps;k++){int tx,ty;if(MapTileAt(Vector2.Lerp(from,to,k/(float)steps),out tx,out ty))PaintCompose(tx,ty);}composeLastMouse=to;e.Use();return true;}
  if(e.type==EventType.MouseUp&&e.button==0){composePainting=false;e.Use();return true;}
  return false;
 }
 void PaintCompose(int x,int y){
  var t=world.At(x,y);if(t==null||t.realm!=viewRealm)return;int c=y*world.Width+x;
  if(composeErase){composeArea.Remove(c);return;}
  if(composeArea.Contains(c))return;if(PixelArt.IsWater(t.terrain)){status="Only land can be marked.";return;}
  if(world.nations[0].known!=null&&!world.nations[0].known[c]){status="You have not seen that land.";return;}
  if(composeArea.Count>=ProposalKinds.MaxArea){status="At most "+ProposalKinds.MaxArea+" tiles in one proposal.";return;}
  composeArea.Add(c);}
 void FinishComposeArea(){composePicking=false;composePainting=false;composeSite=world.AreaCentre(composeArea);composeJudgeKey=null;status=composeArea.Count==0?"No land marked.":"Marked: "+world.AreaName(composeWith,composeArea)+".";}
 // Shown over the map while you paint.
 void DrawComposePickBar(){
  if(!composePicking||composeWith<=0)return;DiplomacyStyles();Wide();var r=ComposeBarRect;bool was=GUI.enabled;GUI.enabled=true;
  Fill(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),new Color(.86f,.7f,.39f,.9f));Fill(r,new Color(.04f,.06f,.13f,.97f));
  var st=new GUIStyle(dipRow){fontSize=14,wordWrap=true,alignment=TextAnchor.MiddleLeft};
  GUI.Label(new Rect(r.x+12,r.y+4,r.width-230,r.height-8),"Drag across the land you want: "+composeArea.Count+" of "+ProposalKinds.MaxArea+" tiles. Drag over marked tiles to remove them.",st);
  Color navy=new Color(.1f,.16f,.3f),teal=new Color(.07f,.38f,.4f);
  if(DipButton(new Rect(r.xMax-212,r.y+10,96,34),"Clear",navy)){composeArea.Clear();composeJudgeKey=null;}
  if(DipButton(new Rect(r.xMax-108,r.y+10,96,34),"Done",teal)){FinishComposeArea();GUIUtility.ExitGUI();}
  GUI.enabled=was;}
 bool ComposeOpen(){return composeWith>0&&world!=null&&!composePicking;}
 void OpenCompose(int partner){composeWith=partner;composePicking=false;composePainting=false;composeArea.Clear();composeSite=-1;composeJudgeKey=null;composeReply="";valleyTalk=-1;detailsScreen="";ResetMapGesture();}
 // A left click on the map while choosing a place names it (Click calls this first).
 bool ComposePick(int x,int y){return composePicking;}// painting is handled by ComposeAreaInput; a stray click while marking does nothing else
 void DrawCompose(){
  GUI.enabled=true;DiplomacyStyles();HudStyles();int o=composeWith;var nation=world.nations[o];
  Fill(FullCanvas,new Color(0,0,0,.45f));var box=new Rect(150,70,980,660);DipPanel(box,.98f);
  GUI.Label(new Rect(box.x,box.y+12,box.width,36),("Your proposal to "+(string.IsNullOrEmpty(nation.leader)?world.NationName(o):nation.leader+" of "+world.NationName(o))).ToUpperInvariant(),dipTitle);
  var face=nation.portrait>=0?Portrait(nation.portrait):null;if(face!=null)DrawPortrait(new Rect(box.x+28,box.y+60,150,200),face);
  var text=new GUIStyle(dipRow){fontSize=15,wordWrap=true,alignment=TextAnchor.UpperLeft};var said=new GUIStyle(text){fontStyle=FontStyle.Italic};
  Color navy=new Color(.1f,.16f,.3f),teal=new Color(.07f,.38f,.4f),gold=new Color(.45f,.36f,.12f);float x=box.x+200,w=box.width-228,y=box.y+62;
  // What kind.
  float kw=(w-30)/4;for(int k=0;k<4;k++){var kr=new Rect(x+k*(kw+10),y,kw,40);if(DipButton(kr,"",composeKind==k?teal:navy)){composeKind=k;composeReply="";}var ks=new GUIStyle(text){fontSize=13,alignment=TextAnchor.MiddleCenter};GUI.Label(kr,ProposalKinds.Names[k],ks);}
  y+=52;
  if(composeKind<=1){{var pr=new Rect(x,y,w-200,32);var ps=new GUIStyle(text);string pt="Land: "+(composeArea.Count>0?world.AreaName(o,composeArea)+", marked on the map":"none marked yet");while(ps.fontSize>11&&ps.CalcHeight(new GUIContent(pt),pr.width)>pr.height)ps.fontSize--;GUI.Label(pr,pt,ps);}
   if(DipButton(new Rect(x+w-190,y,190,32),composeArea.Count>0?"Change on map":"Mark on map",navy)){composePicking=true;composePainting=false;status="Drag across the land you want.";var c0=composeArea.Count>0?world.tiles[world.AreaCentre(composeArea)]:null;if(c0!=null)FocusTile(c0.x,c0.y);GUIUtility.ExitGUI();}y+=40;
   GUI.Label(new Rect(x,y,w,40),composeKind==0?"They promise to leave the place unsettled for 48 turns; you pay the gold now. It does not make the land yours.":"You promise to leave the place unsettled for 48 turns; they pay the gold now. Settling there would break your word.",new GUIStyle(text){fontSize=13});y+=44;}
  else{GUI.Label(new Rect(x,y,120,32),composeKind==2?"Timber now":"Timber a turn",text);
   if(DipButton(new Rect(x+130,y,32,32),"-",navy))composeAmount=Mathf.Max(1,composeAmount-1);GUI.Label(new Rect(x+166,y,60,32),composeAmount.ToString(),new GUIStyle(text){alignment=TextAnchor.MiddleCenter});if(DipButton(new Rect(x+230,y,32,32),"+",navy))composeAmount=Mathf.Min(60,composeAmount+(composeKind==2?5:1));
   if(composeKind==3){GUI.Label(new Rect(x+290,y,80,32),"for",new GUIStyle(text){alignment=TextAnchor.MiddleCenter});if(DipButton(new Rect(x+370,y,32,32),"-",navy))composeTurns=Mathf.Max(2,composeTurns-1);GUI.Label(new Rect(x+406,y,80,32),composeTurns+" turns",new GUIStyle(text){alignment=TextAnchor.MiddleCenter});if(DipButton(new Rect(x+490,y,32,32),"+",navy))composeTurns=Mathf.Min(30,composeTurns+1);}
   y+=44;}
  GUI.Label(new Rect(x,y,140,32),composeKind==1||composeKind==2?"They pay":composeKind==3?"You pay each":"You pay",text);
  if(DipButton(new Rect(x+150,y,32,32),"-",navy))composeGold=Mathf.Max(0,composeGold-5);GUI.Label(new Rect(x+186,y,80,32),composeGold+" gold",new GUIStyle(text){alignment=TextAnchor.MiddleCenter});if(DipButton(new Rect(x+270,y,32,32),"+",navy))composeGold=Mathf.Min(500,composeGold+5);
  y+=46;Fill(new Rect(x,y,w,1),new Color(.86f,.7f,.39f,.6f));y+=10;
  // Judged again only when a term changes (a painted area can hold many town sites to weigh).
  string key=o+"|"+composeKind+"|"+string.Join(",",composeArea.Select(c=>c.ToString()).ToArray())+"|"+composeAmount+"|"+composeTurns+"|"+composeGold+"|"+world.turn+"|"+world.gold;
  if(key!=composeJudgeKey){composeJudged=world.JudgeProposal(o,composeKind,composeArea,composeAmount,composeTurns,composeGold,out composeJudgedOk);composeJudgeKey=key;}
  bool ok=composeJudgedOk;string answer=composeJudged;
  {var jr=new Rect(x,y,w,80);var js=new GUIStyle(said);while(js.fontSize>11&&js.CalcHeight(new GUIContent(answer),jr.width)>jr.height)js.fontSize--;GUI.Label(jr,(ok?"They would agree. ":"")+answer,js);y+=88;}
  if(composeReply!=""){var rr=new Rect(x,y,w,60);GUI.Label(rr,composeReply,new GUIStyle(text){fontSize=14});}
  if(DipButton(new Rect(x+w-220,box.yMax-56,220,38),"Propose",ok?teal:navy)&&ok){composeReply=world.MakeProposal(o,composeKind,composeArea,composeAmount,composeTurns,composeGold);composeJudgeKey=null;status=composeReply;GUIUtility.ExitGUI();}
  if(DipButton(new Rect(box.x+28,box.yMax-56,180,38),"Close",navy)){composeWith=-1;GUIUtility.ExitGUI();}
 }
}
}
