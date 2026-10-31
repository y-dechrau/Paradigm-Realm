using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The diplomacy screen in the layout of the owner's mockup (Design/Mockups/diplomacy-mockup.png, 30 Oct 2026): the partner's
// leader large in front of a painted coast (Resources/Visuals/diplomacy-backdrop, cut from the mockup), a card for each met
// civilisation along the bottom, and on the right their name and mood, greeting, active treaties, a two-sided offer and
// Propose / Discuss / Leave / Declare War. Discuss opens the full detail panel (DrawAdvancedDiplomacy) with every action.
public partial class RealmGame {
 bool diplomacyDiscuss,diplomacyChooser,diplomacyDirect;string dipReply="";int dipReplyPartner=-1;
 // The Diplomacy button first asks which leader to contact; the talk that follows is with that leader alone.
 void DrawLeaderChooser(int[] known){
  GUI.Label(new Rect(240,90,800,34),"Choose a leader to contact",dipBig);
  var backdrop=Art("diplomacy-backdrop");int cols=Math.Min(5,known.Length);float cw=220,ch=300,gap=18,x0=640-(cols*cw+(cols-1)*gap)/2;
  for(int k=0;k<known.Length;k++){
   int other=known[k];var card=new Rect(x0+(k%5)*(cw+gap),140+(k/5)*(ch+gap),cw,ch);DipPanel(card,1);var pic=new Rect(card.x+4,card.y+4,card.width-8,card.height-60);
   if(backdrop!=null)DrawCropped(pic,backdrop,BackdropAspect);var face=Portrait(world.nations[other].portrait,world.Era(other));if(face!=null)DrawPortrait(pic,face);
   Fill(new Rect(card.x+4,card.yMax-56,card.width-8,52),new Color(.04f,.06f,.12f,.96f));GUI.Label(new Rect(card.x,card.yMax-54,card.width,24),world.NationName(other).ToUpperInvariant(),new GUIStyle(dipCaps){fontSize=14});
   Color ink;GUI.Label(new Rect(card.x,card.yMax-30,card.width,22),MoodText(other,out ink),new GUIStyle(dipBadge){normal={textColor=ink}});
   int waiting=world.offers.Count(o=>o.to==0&&o.from==other&&o.status==0&&o.expires>world.turn);if(waiting>0){var b=new Rect(card.xMax-30,card.y+8,22,22);Fill(b,new Color(.75f,.18f,.14f));GUI.Label(b,waiting.ToString(),dipBadge);}
   if(card.Contains(Event.current.mousePosition))Fill(new Rect(card.x,card.y,card.width,2),new Color(1f,.82f,.42f));
   if(GUI.Button(card,"",GUIStyle.none)){diplomacyPartner=other;diplomacyChooser=false;ResetDiplomacyUI();}
  }
  if(DipButton(new Rect(560,730,160,44),"Leave",new Color(.1f,.16f,.3f))){detailsScreen="";GUIUtility.ExitGUI();}
 }GUIStyle dipTitle,dipName,dipQuote,dipRow,dipBadge,dipCaps,dipBig;
 static readonly string[] DealKinds={"Open borders","Friendship","Defensive alliance","Research pact","Trade","Peace","Border promise","Joint war"};
 void DiplomacyStyles(){
  if(dipTitle!=null)return;
  dipTitle=new GUIStyle(hudTitle){fontSize=26,alignment=TextAnchor.MiddleCenter};
  dipCaps=new GUIStyle(hudTitle){fontSize=13,alignment=TextAnchor.MiddleCenter};
  dipName=new GUIStyle(hudTitle){fontSize=26,alignment=TextAnchor.MiddleLeft};
  dipBig=new GUIStyle(hudTitle){fontSize=20,alignment=TextAnchor.MiddleCenter};foreach(var s in new[]{dipBig.normal,dipBig.hover})s.textColor=new Color(.98f,.95f,.86f);
  dipQuote=new GUIStyle(hudLabel){fontSize=16,wordWrap=true,alignment=TextAnchor.MiddleLeft};
  dipRow=new GUIStyle(hudLabel){fontSize=15};
  dipBadge=new GUIStyle(hudLabel){fontSize=13,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
 }
 // A navy panel with a gold rule round it, as in the mockup.
 void DipPanel(Rect r,float alpha=.94f){Fill(r,new Color(.05f,.08f,.15f,alpha));var g=new Color(.78f,.62f,.32f);Fill(new Rect(r.x,r.y,r.width,1),g);Fill(new Rect(r.x,r.yMax-1,r.width,1),g);Fill(new Rect(r.x,r.y,1,r.height),g);Fill(new Rect(r.xMax-1,r.y,1,r.height),g);}
 void DipHeading(Rect r,string text){GUI.Label(r,text.ToUpperInvariant(),new GUIStyle(dipCaps){alignment=TextAnchor.MiddleLeft,fontSize=15});Fill(new Rect(r.x+text.Length*11+14,r.center.y,r.xMax-(r.x+text.Length*11+14),1),new Color(.78f,.62f,.32f,.7f));}
 bool DipButton(Rect r,string text,Color face){Fill(r,face);var edge=new Color(.86f,.7f,.39f);Fill(new Rect(r.x,r.y,r.width,1),edge);Fill(new Rect(r.x,r.yMax-1,r.width,1),edge);Fill(new Rect(r.x,r.y,1,r.height),edge);Fill(new Rect(r.xMax-1,r.y,1,r.height),edge);
  bool hover=r.Contains(Event.current.mousePosition)&&GUI.enabled;if(hover)Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),new Color(1,1,1,.08f));
  var style=new GUIStyle(dipBig){fontSize=19};while(style.fontSize>11&&style.CalcSize(new GUIContent(text)).x>r.width-14)style.fontSize--;if(!GUI.enabled)style.normal.textColor=new Color(.6f,.6f,.6f);GUI.Label(r,text,style);
  return GUI.Button(r,"",GUIStyle.none)&&GUI.enabled;}
 string MoodText(int partner,out Color ink){
  if(world.AtWar(0,partner)){ink=new Color(.9f,.3f,.25f);return "AT WAR";}
  int o=world.Opinion(partner,0);string sign=o>=0?"+"+o:o.ToString();
  if(world.Allied(0,partner)){ink=new Color(.4f,.85f,.45f);return "ALLIED "+sign;}
  if(o>=25||world.Friends(0,partner)){ink=new Color(.4f,.85f,.45f);return "FRIENDLY "+sign;}
  if(o>=-10){ink=new Color(.9f,.76f,.38f);return "NEUTRAL "+sign;}
  ink=new Color(.9f,.42f,.3f);return "HOSTILE "+sign;}
 List<KeyValuePair<string,int>> ActiveTreaties(int partner){
  var p=world.Relation(0,partner);int now=world.turn;var list=new List<KeyValuePair<string,int>>();
  Action<string,int> add=(n,until)=>{if(until>now)list.Add(new KeyValuePair<string,int>(n,until-now));};
  add("Open Borders",p.accessUntil);add("Friendship",p.friendUntil);add("Defensive Alliance",p.allianceUntil);add("Research Pact",p.researchUntil);add("Truce",p.peaceUntil);add("Embargo",p.embargoUntil);
  add("Your border promise",p.promiseAUntil);add("Their border promise",p.promiseBUntil);
  foreach(var c in world.contracts.Where(c=>c.from==0&&c.to==partner||c.to==0&&c.from==partner))list.Add(new KeyValuePair<string,int>("Trade #"+c.id+(world.ContractActive(c)?"":" (suspended)"),c.end-now));
  return list;}
 // Number field with − / + steppers so offers can be built with the mouse, as in the mockup's offer slots.
 string Stepper(Rect r,string value,int step,int max){int v=DealNumber(value,max);
  if(GUI.Button(new Rect(r.x,r.y,24,r.height),"−"))v=Math.Max(0,v-step);
  var field=GUI.TextField(new Rect(r.x+26,r.y,r.width-52,r.height),v.ToString(),5);int typed;if(int.TryParse(field,out typed))v=Math.Max(0,Math.Min(max,typed));else if(field=="")v=0;
  if(GUI.Button(new Rect(r.xMax-24,r.y,24,r.height),"+"))v=Math.Min(max,v+step);return v.ToString();}
 // Drop-down boxes: the box shows the choice; clicking opens a list drawn over everything else. Clicks on an open list are
 // taken at the start of the next screen draw, before the controls underneath can see them.
 string dropOpen,dropPicked;int dropIndex;string[] dropNames;Rect dropBox,dropList;
 int Dropdown(Rect r,string id,int value,string[] names){
  if(dropPicked==id){dropPicked=null;value=dropIndex;}
  value=Math.Max(0,Math.Min(names.Length-1,value));
  Fill(r,new Color(.08f,.12f,.22f));var edge=new Color(.78f,.62f,.32f);Fill(new Rect(r.x,r.y,r.width,1),edge);Fill(new Rect(r.x,r.yMax-1,r.width,1),edge);Fill(new Rect(r.x,r.y,1,r.height),edge);Fill(new Rect(r.xMax-1,r.y,1,r.height),edge);
  GUI.Label(new Rect(r.x+8,r.y,r.width-30,r.height),names[value],new GUIStyle(dipRow){fontSize=13,alignment=TextAnchor.MiddleLeft});GUI.Label(new Rect(r.xMax-22,r.y,18,r.height),dropOpen==id?"▲":"▼",new GUIStyle(dipRow){fontSize=11,alignment=TextAnchor.MiddleCenter});
  if(dropOpen==null&&GUI.Button(r,"",GUIStyle.none)){dropOpen=id;dropNames=names;dropBox=r;float h=Math.Min(names.Length*24,24*9);dropList=r.yMax+h>790?new Rect(r.x,r.y-h,r.width,h):new Rect(r.x,r.yMax,r.width,h);}
  return value;}
 void DropdownClicks(){
  var e=Event.current;if(dropOpen==null||e.type!=EventType.MouseDown)return;
  if(dropList.Contains(e.mousePosition)){int k=(int)((e.mousePosition.y-dropList.y)/24)+dropScroll;if(k>=0&&k<dropNames.Length){dropPicked=dropOpen;dropIndex=k;}}
  dropOpen=null;dropScroll=0;e.Use();}
 int dropScroll;
 void DrawOpenDropdown(){
  if(dropOpen==null)return;var e=Event.current;int rows=(int)(dropList.height/24);
  if(e.type==EventType.ScrollWheel&&dropList.Contains(e.mousePosition)){dropScroll=Math.Max(0,Math.Min(dropNames.Length-rows,dropScroll+(e.delta.y>0?1:-1)));e.Use();}
  DipPanel(dropList,1);
  for(int k=0;k<rows&&k+dropScroll<dropNames.Length;k++){var row=new Rect(dropList.x+1,dropList.y+k*24,dropList.width-2,24);if(row.Contains(e.mousePosition))Fill(row,new Color(.07f,.38f,.4f,.9f));GUI.Label(new Rect(row.x+8,row.y,row.width-16,24),dropNames[k+dropScroll],new GUIStyle(dipRow){fontSize=13});}}
 // A picture whose true shape is aspect (width / height), filling the box and cropping the overflow evenly.
 const float BackdropAspect=410f/452f;
 void DrawCropped(Rect r,Texture2D tex,float aspect){float ra=r.width/r.height;if(ra>aspect){float vh=aspect/ra;GUI.DrawTextureWithTexCoords(r,tex,new Rect(0,(1-vh)/2,1,vh));}else{float uw=ra/aspect;GUI.DrawTextureWithTexCoords(r,tex,new Rect((1-uw)/2,0,uw,1));}}
 int Cycle(Rect r,int value,string[] names){if(GUI.Button(new Rect(r.x,r.y,24,r.height),"◀"))value=(value+names.Length-1)%names.Length;GUI.Label(new Rect(r.x+26,r.y,r.width-52,r.height),names[value],new GUIStyle(dipRow){alignment=TextAnchor.MiddleCenter,fontSize=13});if(GUI.Button(new Rect(r.xMax-24,r.y,24,r.height),"▶"))value=(value+1)%names.Length;return value;}

 void DrawDiplomacyScreen(){
  DiplomacyStyles();
  DropdownClicks();
  Fill(new Rect(0,0,1280,800),new Color(.03f,.05f,.1f,.97f));
  // Title plaque.
  DipPanel(new Rect(450,6,380,60));GUI.Label(new Rect(450,8,380,34),"PARADIGM REALM",dipTitle);GUI.Label(new Rect(450,40,380,22),"—  D I P L O M A C Y  —",dipCaps);
  var known=Enumerable.Range(1,world.civCount-1).Where(i=>world.met[i]).ToArray();
  if(known.Length==0){DipPanel(new Rect(340,300,600,120));GUI.Label(new Rect(340,320,600,40),"No civilisations met yet",dipBig);GUI.Label(new Rect(340,360,600,30),"Explore to meet other civilisations.",new GUIStyle(dipRow){alignment=TextAnchor.MiddleCenter});
   if(DipButton(new Rect(560,440,160,44),"Leave",new Color(.1f,.16f,.3f))){detailsScreen="";GUIUtility.ExitGUI();}return;}
  if(diplomacyChooser){DrawLeaderChooser(known);return;}
  if(!known.Contains(diplomacyPartner))diplomacyPartner=known[0];int partner=diplomacyPartner;var pn=world.nations[partner];
  // ---- Scene: the leader before the painted coast ----
  var scene=new Rect(14,74,800,712);DipPanel(scene,1);var inner=new Rect(scene.x+4,scene.y+4,scene.width-8,scene.height-8);
  var backdrop=Art("diplomacy-backdrop");
  if(backdrop!=null)DrawCropped(inner,backdrop,BackdropAspect);else{Fill(inner,new Color(.35f,.3f,.42f));Fill(new Rect(inner.x,inner.y+inner.height*.6f,inner.width,inner.height*.4f),new Color(.15f,.25f,.4f));}
  // The partner's banner hangs at the left edge in their colour.
  Color ink=nations[partner];
  {var banner=FlagTexture(partner);var br=new Rect(inner.x+18,inner.y+10,66,44);if(banner!=null){Fill(new Rect(br.x-2,br.y-6,2,br.height+120),new Color(.32f,.22f,.12f));GUI.DrawTexture(br,banner,ScaleMode.StretchToFill,true);}}
  var face=Portrait(pn.portrait,world.Era(partner));
  if(face!=null){float h=inner.height-36,w=h*.9f;var frame=new Rect(inner.x+86,inner.y+18,w,h);DipPanel(new Rect(frame.x-4,frame.y-4,frame.width+8,frame.height+8),1);DrawPortrait(frame,face);}
  // Talks are one on one (owner request): leader cards appear only on the chooser (DrawLeaderChooser), not here.
  // ---- Right panel ----
  var side=new Rect(828,74,440,712);DipPanel(side);
  if(diplomacyDiscuss||diplomacyConfirmation!=""){
   // Full detail (every action, the inbox and confirmations) in the familiar scrolling layout.
   // Full width so every line fits; no scroll bars, the mouse wheel scrolls up and down.
   var wide=new Rect(14,74,1254,712);DipPanel(wide);side=wide;
   GUILayout.BeginArea(new Rect(wide.x+18,wide.y+12,wide.width-36,wide.height-70));detailsScroll=GUILayout.BeginScrollView(detailsScroll,false,false,GUIStyle.none,GUIStyle.none);detailsScroll.x=0;DrawAdvancedDiplomacy();GUILayout.EndScrollView();GUILayout.EndArea();
   if(DipButton(new Rect(side.x+14,side.yMax-52,side.width-28,40),"Back to negotiation",new Color(.1f,.16f,.3f))){diplomacyDiscuss=false;diplomacyConfirmation="";confirmingOffer=null;confirmingInbox=-1;}
   return;}
  float y=side.y+14;
  // Shield, name, subtitle and mood badge.
  // Their flag in place of a shield.
  {var theirFlag=FlagTexture(partner);var fr=new Rect(side.x+14,y+12,72,48);DipPanel(new Rect(fr.x-3,fr.y-3,fr.width+6,fr.height+6),1);if(theirFlag!=null)GUI.DrawTexture(fr,theirFlag,ScaleMode.StretchToFill,true);}
  GUI.Label(new Rect(side.x+92,y,240,40),world.NationName(partner),dipName);
  GUI.Label(new Rect(side.x+92,y+40,330,22),(string.IsNullOrEmpty(pn.leader)?"":pn.leader+" — ")+world.Personality(partner).Split(':')[0],new GUIStyle(dipRow){fontSize=14});
  Color moodInk;string mood=MoodText(partner,out moodInk);var badge=new Rect(side.xMax-140,y+6,124,26);Fill(badge,new Color(moodInk.r*.25f,moodInk.g*.25f,moodInk.b*.25f,.95f));Fill(new Rect(badge.x,badge.y,badge.width,1),moodInk);Fill(new Rect(badge.x,badge.yMax-1,badge.width,1),moodInk);var bs=new GUIStyle(dipBadge);bs.normal.textColor=moodInk;GUI.Label(badge,mood,bs);
  Fill(new Rect(side.x+92,y+68,side.width-108,1),new Color(.78f,.62f,.32f,.7f));
  y+=90;
  // Greeting.
  var quote=new Rect(side.x+14,y,side.width-28,62);DipPanel(quote,.9f);GUI.Label(new Rect(quote.x+14,quote.y+4,quote.width-28,quote.height-8),dipReplyPartner==partner&&!string.IsNullOrEmpty(dipReply)?dipReply:world.Greeting(partner),dipQuote);y+=74;
  // Pending proposals from this partner.
  var pending=world.offers.Where(o=>o.to==0&&o.status==0&&o.expires>world.turn).ToList();
  if(pending.Count>0){var strip=new Rect(side.x+14,y,side.width-28,30);Fill(strip,new Color(.5f,.38f,.12f,.9f));GUI.Label(new Rect(strip.x+10,strip.y,strip.width-110,30),pending.Count+" proposal"+(pending.Count==1?"":"s")+" awaiting your answer",dipRow);if(GUI.Button(new Rect(strip.xMax-96,strip.y+3,90,24),"Review")){diplomacyDiscuss=true;detailsScroll=Vector2.zero;}y+=38;}
  // Active treaties.
  DipHeading(new Rect(side.x+16,y,side.width-32,22),"Active treaties");y+=26;
  var treaties=ActiveTreaties(partner);
  if(treaties.Count==0){GUI.Label(new Rect(side.x+22,y,side.width-40,26),world.AtWar(0,partner)?"None — your nations are at war.":"None yet.",dipRow);y+=30;}
  foreach(var tr in treaties.Take(4)){var row=new Rect(side.x+16,y,side.width-32,30);DipPanel(row,.85f);GUI.Label(new Rect(row.x+12,row.y+2,row.width-120,26),tr.Key,dipRow);GUI.Label(new Rect(row.xMax-110,row.y+2,98,26),tr.Value+" turns",new GUIStyle(dipRow){alignment=TextAnchor.MiddleRight});y+=34;}
  if(treaties.Count>4){GUI.Label(new Rect(side.x+22,y,side.width-40,20),"+"+(treaties.Count-4)+" more under Discuss",hudSmall);y+=22;}
  y+=6;
  // Negotiate: agreement, length, and the two sides of the offer.
  DipHeading(new Rect(side.x+16,y,side.width-32,22),"Negotiate");y+=28;
  diplomacyKind=Dropdown(new Rect(side.x+16,y,side.width/2-22,26),"kind",diplomacyKind,DealKinds);
  diplomacyDuration=Dropdown(new Rect(side.x+side.width/2+6,y,side.width/2-22,26),"length",diplomacyDuration,new[]{"10 turns","15 turns","30 turns"});y+=32;
  bool recurring=diplomacyKind==4||diplomacyKind==5;float colW=(side.width-44)/2;
  for(int s=0;s<2;s++){
   var col=new Rect(side.x+16+s*(colW+12),y,colW,recurring?150:92);DipPanel(col,.9f);GUI.Label(new Rect(col.x,col.y+4,col.width,20),s==0?"YOUR OFFER":"THEIR OFFER",dipCaps);
   DrawIcon(PixelArt.IconGold,new Rect(col.x+8,col.y+32,22,22));GUI.Label(new Rect(col.x+32,col.y+30,60,24),"Gold",dipRow);
   if(s==0)giveGold=Stepper(new Rect(col.x+86,col.y+30,col.width-94,24),giveGold,25,10000);else askGold=Stepper(new Rect(col.x+86,col.y+30,col.width-94,24),askGold,25,10000);
   if(recurring){GUI.Label(new Rect(col.x+10,col.y+60,80,24),"Per turn",dipRow);
    if(s==0)givePerTurn=Stepper(new Rect(col.x+86,col.y+60,col.width-94,24),givePerTurn,1,20);else askPerTurn=Stepper(new Rect(col.x+86,col.y+60,col.width-94,24),askPerTurn,1,20);
    GUI.Label(new Rect(col.x+10,col.y+90,col.width-20,20),"Resource access",hudSmall);
    if(s==0)giveResource=Dropdown(new Rect(col.x+8,col.y+112,col.width-16,26),"give",giveResource,World.ResourceNames);else askResource=Dropdown(new Rect(col.x+8,col.y+112,col.width-16,26),"ask",askResource,World.ResourceNames);}
   else GUI.Label(new Rect(col.x+10,col.y+60,col.width-20,24),s==0?"plus the agreement":"in return",hudSmall);
  }
  y+=(recurring?150:92)+8;
  int target=Enumerable.Range(1,world.civCount-1).FirstOrDefault(i=>i!=partner&&world.met[i]);
  var offer=new DiplomaticOffer{from=0,to=partner,kind=(TreatyKind)diplomacyKind,duration=new[]{10,15,30}[diplomacyDuration],giveGold=DealNumber(giveGold,10000),askGold=DealNumber(askGold,10000),givePerTurn=recurring?DealNumber(givePerTurn,20):0,askPerTurn=recurring?DealNumber(askPerTurn,20):0,giveResource=recurring?giveResource:0,askResource=recurring?askResource:0,target=target==0?-1:target};
  string problem=world.OfferProblem(offer);
  GUI.Label(new Rect(side.x+18,y,side.width-36,38),problem!=""?problem:world.OfferAssessment(offer),hudSmall);
  // Buttons.
  bool enabled=GUI.enabled;GUI.enabled=enabled&&problem=="";
  if(DipButton(new Rect(side.x+16,side.yMax-108,side.width-32,46),"PROPOSE AGREEMENT",new Color(.07f,.38f,.4f))){confirmingOffer=offer;confirmingInbox=-1;diplomacyConfirmation="Send this proposal?";detailsScroll=Vector2.zero;}
  GUI.enabled=enabled;
  float bw=(side.width-48)/3;
  if(DipButton(new Rect(side.x+16,side.yMax-54,bw,42),"Discuss",new Color(.1f,.16f,.3f))){diplomacyDiscuss=true;detailsScroll=Vector2.zero;}
  if(DipButton(new Rect(side.x+24+bw,side.yMax-54,bw,42),"Leave",new Color(.1f,.16f,.3f))){detailsScreen="";diplomacyDiscuss=false;GUIUtility.ExitGUI();}
  string warReason=world.WarReason(0,partner);GUI.enabled=enabled&&warReason=="";
  if(DipButton(new Rect(side.x+32+2*bw,side.yMax-54,bw,42),"Declare War",new Color(.45f,.08f,.07f))){pendingUnit=-2;pendingWars=new[]{partner};detailsScreen="";cityScreen=false;GUIUtility.ExitGUI();}
  GUI.enabled=enabled;
  if(warReason!=""&&new Rect(side.x+32+2*bw,side.yMax-54,bw,42).Contains(Event.current.mousePosition))GUI.Label(new Rect(side.x+16,side.yMax-128,side.width-32,20),warReason,hudSmall);
  DrawOpenDropdown();
 }
}
}
