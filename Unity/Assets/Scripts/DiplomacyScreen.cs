using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The diplomacy screen for the rebuilt diplomacy (owner request, 10 Nov 2026; Diplomacy.cs), in the frame of the owner's mockup
// (Design/Mockups/diplomacy-mockup.png): the partner's leader before a painted coast, and on the right everything that can be
// done with them on one page. Each leader shows one attitude word, the reasons for its opinion, and for every agreement whether
// it would sign and exactly why (hover a row). Claims and trade deals open in place; More holds the inbox, the record and details.
public partial class RealmGame {
 bool diplomacyDiscuss,diplomacyChooser,diplomacyDirect,customDeal,claimsView;string dipReply="";int dipReplyPartner=-1;
 // The Diplomacy button first asks which leader to contact; the talk that follows is with that leader alone.
 void DrawLeaderChooser(int[] known){
  DipPanel(new Rect(400,84,480,44),.9f);GUI.Label(new Rect(400,88,480,34),"Choose a leader to contact",dipBig);
  var backdrop=Art("diplomacy-backdrop");
  // Up to 15 leaders (with the native peoples): two rows of up to eight cards, smaller as more are known.
  int cols=known.Length<=5?known.Length:Math.Min(8,(known.Length+1)/2);int rows=(known.Length+cols-1)/cols;float gap=14,cw=Math.Min(220,(1220-(cols-1)*gap)/cols),ch=Math.Min(300,(580-(rows-1)*gap)/rows),x0=640-(cols*cw+(cols-1)*gap)/2;
  for(int k=0;k<known.Length;k++){
   int other=known[k];var card=new Rect(x0+(k%cols)*(cw+gap),140+(k/cols)*(ch+gap),cw,ch);DipPanel(card,1);var pic=new Rect(card.x+4,card.y+4,card.width-8,card.height-60);
   if(backdrop!=null)DrawCropped(pic,backdrop,BackdropAspect);var face=Portrait(world.nations[other].portrait,world.Era(other));if(face!=null)DrawPortrait(pic,face);
   Fill(new Rect(card.x+4,card.yMax-56,card.width-8,52),new Color(.04f,.06f,.12f,.96f));FitLabel(new Rect(card.x+2,card.yMax-54,card.width-4,24),world.NationName(other).ToUpperInvariant(),new GUIStyle(dipCaps){fontSize=14},9);
   Color ink;GUI.Label(new Rect(card.x,card.yMax-30,card.width,22),MoodText(other,out ink),new GUIStyle(dipBadge){normal={textColor=ink}});
   int waiting=world.offers.Count(o=>o.to==0&&o.from==other&&o.status==0&&o.expires>world.turn);if(waiting>0){var b=new Rect(card.xMax-30,card.y+8,22,22);Fill(b,new Color(.75f,.18f,.14f));GUI.Label(b,waiting.ToString(),dipBadge);}
   if(card.Contains(Event.current.mousePosition))Fill(new Rect(card.x,card.y,card.width,2),new Color(1f,.82f,.42f));
   if(GUI.Button(card,"",GUIStyle.none)){diplomacyPartner=other;diplomacyChooser=false;ResetDiplomacyUI();}
  }
  if(DipButton(new Rect(560,730,160,44),"Leave",new Color(.1f,.16f,.3f))){detailsScreen="";GUIUtility.ExitGUI();}
 }GUIStyle dipTitle,dipName,dipQuote,dipRow,dipBadge,dipCaps,dipBig;
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
 // Hover notes: the row under the pointer explains itself in a panel beside the list (the reasons for an answer, or what a button does).
 string tipTitle,tipFoot;List<KeyValuePair<string,int>> tipLines;float tipY;
 void Tip(Rect r,string title,List<KeyValuePair<string,int>> lines=null,string foot=""){if(!r.Contains(hoverProbe??Event.current.mousePosition))return;tipTitle=title;tipLines=lines;tipFoot=foot;tipY=r.y;}
 void DrawTip(Rect side){
  if(string.IsNullOrEmpty(tipTitle))return;var ts=new GUIStyle(dipRow){fontSize=13,wordWrap=true};float w=340,th=ts.CalcHeight(new GUIContent(tipTitle),w-24);
  int n=tipLines==null?0:Math.Min(11,tipLines.Count);float fh=string.IsNullOrEmpty(tipFoot)?0:ts.CalcHeight(new GUIContent(tipFoot),w-24)+6;float h=16+th+(n>0?n*19+30:0)+fh;
  var box=new Rect(side.x-w-8,Mathf.Clamp(tipY-8,80,782-h),w,h);DipPanel(box,.98f);float y=box.y+8;GUI.Label(new Rect(box.x+12,y,w-24,th),tipTitle,ts);y+=th+4;
  if(n>0){int total=tipLines.Sum(l=>l.Value);
   for(int k=0;k<n;k++){var l=tipLines[k];GUI.Label(new Rect(box.x+14,y,w-80,19),l.Key,new GUIStyle(dipRow){fontSize=12});GUI.Label(new Rect(box.xMax-64,y,52,19),(l.Value>0?"+":"")+l.Value,new GUIStyle(dipRow){fontSize=12,alignment=TextAnchor.MiddleRight,normal={textColor=l.Value>0?new Color(.55f,.9f,.55f):new Color(1f,.55f,.45f)}});y+=19;}
   Fill(new Rect(box.x+12,y+3,w-24,1),new Color(.78f,.62f,.32f,.7f));GUI.Label(new Rect(box.x+14,y+6,w-80,20),total>=0?"They would agree":"They would refuse",new GUIStyle(dipRow){fontSize=13,fontStyle=FontStyle.Bold});
   GUI.Label(new Rect(box.xMax-64,y+6,52,20),(total>0?"+":"")+total,new GUIStyle(dipRow){fontSize=13,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleRight,normal={textColor=total>=0?new Color(.55f,.9f,.55f):new Color(1f,.55f,.45f)}});y+=30;}
  if(fh>0)GUI.Label(new Rect(box.x+12,y,w-24,fh),tipFoot,new GUIStyle(ts){fontSize=12});}
 static readonly Color DipGood=new Color(.55f,.9f,.55f),DipBad=new Color(1f,.55f,.45f),DipPlain=new Color(.9f,.76f,.38f);
 static Color AttitudeInk(int attitude){return attitude==Attitudes.Friendly||attitude==Attitudes.Cordial||attitude==Attitudes.Loyal?new Color(.4f,.85f,.45f):attitude==Attitudes.Neutral||attitude==Attitudes.Wary?new Color(.9f,.76f,.38f):new Color(.9f,.4f,.3f);}
 // The leader's one-word attitude toward you with its opinion beside it.
 string MoodText(int partner,out Color ink){int a=world.Attitude(partner,0);ink=AttitudeInk(a);if(a==Attitudes.AtWar)return "AT WAR";int o=world.Opinion(partner,0);return Attitudes.Names[a].ToUpperInvariant()+" "+(o>=0?"+"+o:o.ToString());}
 // Why the leader feels as it does: every named reason with its value, then what the attitude means and what you have to spend.
 void DrawOpinionPanel(int partner,Rect inner){
  var lines=world.OpinionLines(partner,0);int shown=Math.Min(7,lines.Count);int attitude=world.Attitude(partner,0);
  var box=new Rect(inner.x+10,inner.yMax-(88+shown*20),inner.width-20,78+shown*20);DipPanel(box,.9f);float y=box.y+6;
  GUI.Label(new Rect(box.x+12,y,box.width-24,22),"WHY "+world.NationName(partner).ToUpperInvariant()+" FEELS THIS WAY: "+world.Opinion(partner,0),new GUIStyle(dipCaps){alignment=TextAnchor.MiddleLeft});y+=24;
  var fade=world.MemoryLines(partner,0).Where(m=>m.turnsLeft>0).ToDictionary(m=>m.text,m=>m.turnsLeft);
  for(int k=0;k<shown;k++){var l=lines[k];int left;GUI.Label(new Rect(box.x+16,y,box.width-110,20),l.Key+(fade.TryGetValue(l.Key,out left)?"  (fades in ~"+left+" turns)":""),new GUIStyle(dipRow){fontSize=13});GUI.Label(new Rect(box.xMax-90,y,76,20),(l.Value>0?"+":"")+l.Value,new GUIStyle(dipRow){fontSize=13,alignment=TextAnchor.MiddleRight,normal={textColor=l.Value>0?DipGood:DipBad}});y+=20;}
  if(lines.Count>shown)GUI.Label(new Rect(box.x+16,y-2,box.width-32,18),"+"+(lines.Count-shown)+" smaller reasons under More",hudSmall);
  var ink=AttitudeInk(attitude);GUI.Label(new Rect(box.x+12,box.yMax-46,box.width-24,20),Attitudes.Names[attitude]+": "+Attitudes.Texts[attitude],new GUIStyle(dipRow){fontSize=13,normal={textColor=ink}});
  GUI.Label(new Rect(box.x+12,box.yMax-25,box.width-24,20),"Your influence "+world.nations[0].influence+" (+"+world.InfluencePerTurn(0)+" a turn, spent on claims)   •   Envoys away "+world.EnvoysOut(0)+" of "+world.EnvoyLimit(0)+"   •   Rivals "+world.RivalCount(0)+" of "+Pacts.MaxRivals,new GUIStyle(dipRow){fontSize=12});
 }
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

 void DipBar(Rect r,string label,int value,Color fill){Fill(r,new Color(.03f,.05f,.1f,.95f));Fill(new Rect(r.x+1,r.y+1,(r.width-2)*Mathf.Clamp01(value/100f),r.height-2),fill);GUI.Label(r,label+"  "+value+" / 100",new GUIStyle(dipBadge){fontSize=12});}
 // One row of a two-button line. Returns true when clicked; reason greys it and is shown on hover with what the button does.
 bool DipAction(Rect r,string text,string reason,string what,Color face){bool enabled=GUI.enabled;GUI.enabled=enabled&&reason=="";bool hit=DipButton(r,text,face);GUI.enabled=enabled;Tip(r,what,null,reason);return hit;}
 void DrawDiplomacyScreen(){
  DiplomacyStyles();
  DropdownClicks();
  tipTitle=null;
  // A painted world behind the talks instead of plain black (owner request): the title landscape under a navy veil, darker
  // towards the top and bottom edges so the panels and cards stand out.
  {var full=new Rect(0,0,1280,800);var painting=Art("title-world");Fill(full,new Color(.03f,.05f,.1f,1f));if(painting!=null)GUI.DrawTexture(full,painting,ScaleMode.ScaleAndCrop);
   Fill(full,new Color(.03f,.05f,.11f,diplomacyChooser?.5f:.62f));for(int band=0;band<8;band++){float a=.09f*(8-band)/8f;Fill(new Rect(0,band*14,1280,14),new Color(.02f,.03f,.07f,a));Fill(new Rect(0,800-(band+1)*14,1280,14),new Color(.02f,.03f,.07f,a));}}
  DipPanel(new Rect(450,6,380,60));GUI.Label(new Rect(450,8,380,34),"PARADIGM REALM",dipTitle);GUI.Label(new Rect(450,40,380,22),"—  D I P L O M A C Y  —",dipCaps);
  var known=Enumerable.Range(1,world.civCount-1).Where(i=>world.met[i]).ToArray();
  if(known.Length==0){DipPanel(new Rect(340,300,600,120));GUI.Label(new Rect(340,320,600,40),"No civilisations met yet",dipBig);GUI.Label(new Rect(340,360,600,30),"Explore to meet other civilisations.",new GUIStyle(dipRow){alignment=TextAnchor.MiddleCenter});
   if(DipButton(new Rect(560,440,160,44),"Leave",new Color(.1f,.16f,.3f))){detailsScreen="";GUIUtility.ExitGUI();}return;}
  if(diplomacyChooser){DrawLeaderChooser(known);return;}
  if(!known.Contains(diplomacyPartner))diplomacyPartner=known[0];int partner=diplomacyPartner;var pn=world.nations[partner];bool war=world.AtWar(0,partner);
  // ---- Scene: the leader before the painted coast ----
  var scene=new Rect(14,74,800,712);DipPanel(scene,1);var inner=new Rect(scene.x+4,scene.y+4,scene.width-8,scene.height-8);
  var backdrop=Art("diplomacy-backdrop");
  if(backdrop!=null)DrawCropped(inner,backdrop,BackdropAspect);else{Fill(inner,new Color(.35f,.3f,.42f));Fill(new Rect(inner.x,inner.y+inner.height*.6f,inner.width,inner.height*.4f),new Color(.15f,.25f,.4f));}
  {var banner=FlagTexture(partner);var br=new Rect(inner.x+18,inner.y+10,66,44);if(banner!=null){Fill(new Rect(br.x-2,br.y-6,2,br.height+120),new Color(.32f,.22f,.12f));GUI.DrawTexture(br,banner,ScaleMode.StretchToFill,true);}}
  var face=Portrait(pn.portrait,world.Era(partner));
  if(face!=null){float h=inner.height-36,w=h*.9f;var frame=new Rect(inner.x+86,inner.y+18,w,h);DipPanel(new Rect(frame.x-4,frame.y-4,frame.width+8,frame.height+8),1);DrawPortrait(frame,face);}
  if(!diplomacyDiscuss)DrawOpinionPanel(partner,inner);
  // ---- Right panel ----
  var side=new Rect(828,74,440,712);DipPanel(side);
  if(diplomacyDiscuss){
   // The inbox, the full record and the finer details, in a scrolling page across the whole width.
   var wide=new Rect(14,74,1254,712);DipPanel(wide);side=wide;
   GUILayout.BeginArea(new Rect(wide.x+18,wide.y+12,wide.width-36,wide.height-70));detailsScroll=GUILayout.BeginScrollView(detailsScroll,false,false,GUIStyle.none,GUIStyle.none);detailsScroll.x=0;DrawAdvancedDiplomacy();GUILayout.EndScrollView();GUILayout.EndArea();
   if(DipButton(new Rect(side.x+14,side.yMax-52,side.width-28,40),"Back to the talks",new Color(.1f,.16f,.3f)))diplomacyDiscuss=false;
   return;}
  float y=side.y+14;float left=side.x+16,wide2=side.width-32;bool enabled=GUI.enabled;
  Color navy=new Color(.1f,.16f,.3f),teal=new Color(.07f,.38f,.4f),blue=new Color(.08f,.2f,.3f),red=new Color(.45f,.08f,.07f);
  // Flag, name, leader and the attitude badge.
  {var theirFlag=FlagTexture(partner);var fr=new Rect(side.x+14,y+12,72,48);DipPanel(new Rect(fr.x-3,fr.y-3,fr.width+6,fr.height+6),1);if(theirFlag!=null)GUI.DrawTexture(fr,theirFlag,ScaleMode.StretchToFill,true);}
  FitLabel(new Rect(side.x+92,y,side.width-250,40),world.NationName(partner),dipName,14);
  GUI.Label(new Rect(side.x+92,y+40,330,22),(string.IsNullOrEmpty(pn.leader)?"":pn.leader+" — ")+DiplomacyFlavours.PostureNames[world.Posture(partner)],new GUIStyle(dipRow){fontSize=14});
  Color moodInk;string mood=MoodText(partner,out moodInk);var badge=new Rect(side.xMax-154,y+6,138,26);Fill(badge,new Color(moodInk.r*.25f,moodInk.g*.25f,moodInk.b*.25f,.95f));Fill(new Rect(badge.x,badge.y,badge.width,1),moodInk);Fill(new Rect(badge.x,badge.yMax-1,badge.width,1),moodInk);var bs=new GUIStyle(dipBadge);bs.normal.textColor=moodInk;GUI.Label(badge,mood,bs);
  Tip(badge,Attitudes.Names[world.Attitude(partner,0)]+": "+Attitudes.Texts[world.Attitude(partner,0)]);
  Fill(new Rect(side.x+92,y+68,side.width-108,1),new Color(.78f,.62f,.32f,.7f));
  y+=84;
  var quote=new Rect(side.x+14,y,side.width-28,72);DipPanel(quote,.9f);{string said=dipReplyPartner==partner&&!string.IsNullOrEmpty(dipReply)?dipReply:world.Greeting(partner);var qs=new GUIStyle(dipQuote);var qr=new Rect(quote.x+14,quote.y+4,quote.width-28,quote.height-8);while(qs.fontSize>11&&qs.CalcHeight(new GUIContent(said),qr.width)>qr.height)qs.fontSize--;GUI.Label(qr,said,qs);}y+=80;
  // Three numbers: their strength beside yours, their trust in you (and how high it can go), their fear of you.
  {var row=new Rect(left,y,wide2,22);int power=world.PowerLevel(0,partner);
   GUI.Label(row,"Their strength: "+Attitudes.Power[power]+"   •   Trust in you: "+world.Trust(partner,0)+" of "+world.TrustCap(partner,0)+"   •   Fear of you: "+world.Threat(partner,0),new GUIStyle(dipRow){fontSize=13});
   Tip(row,"Strength is what you can see of their army and cities beside your own. Trust grows by 1 every second turn while agreements stand, up to the ceiling of the strongest one (an envoy alone allows 20); each 2 trust is 1 opinion. Fear comes from conquest and war without cause; at 50 they become Threatened.");y+=26;}
  var pending=world.offers.Where(o=>o.to==0&&o.status==0&&o.expires>world.turn).ToList();int asked=world.requests.Count(r=>r.to==0&&r.status==RequestStatus.Pending);
  if(pending.Count+asked>0){var strip=new Rect(side.x+14,y,side.width-28,28);Fill(strip,new Color(.5f,.38f,.12f,.9f));GUI.Label(new Rect(strip.x+10,strip.y,strip.width-110,28),(pending.Count+asked)+" proposal"+(pending.Count+asked==1?"":"s")+" and request"+(pending.Count+asked==1?"":"s")+" await your answer",dipRow);if(GUI.Button(new Rect(strip.xMax-96,strip.y+2,90,24),"Review")){diplomacyDiscuss=true;detailsScroll=Vector2.zero;}y+=34;}
  if(claimsView){
   // ---- Claims on their cities ----
   if(DipButton(new Rect(left,y,wide2,26),"◂ Back",navy))claimsView=false;y+=32;
   DipHeading(new Rect(left,y,wide2,22),"Claims");y+=24;
   GUI.Label(new Rect(left,y,wide2,52),"A claim costs influence, is announced to them, and after "+Pacts.CauseDelay+" turns is a just cause for war. Taking a claimed city alarms the world far less. Your influence: "+world.nations[0].influence+" (+"+world.InfluencePerTurn(0)+" a turn).",new GUIStyle(dipRow){fontSize=12,wordWrap=true});y+=56;
   var theirs=world.cities.Where(c=>c.owner==partner&&world.At(c.x,c.y).seen).OrderByDescending(c=>world.HasClaim(0,c.id)).ThenByDescending(c=>c.pop).ThenBy(c=>c.id).ToList();
   if(theirs.Count==0){GUI.Label(new Rect(left,y,wide2,22),"You have not found any of their cities.",new GUIStyle(dipRow){fontSize=13});y+=26;}
   foreach(var c in theirs.Take(10)){var row=new Rect(left,y,wide2,28);bool mine=world.HasClaim(0,c.id);string reason=mine?"":world.ClaimReason(0,c);
    GUI.Label(new Rect(row.x,row.y+3,200,22),c.name+"  (size "+c.pop+(pn.capital==c.id?", capital":"")+")",new GUIStyle(dipRow){fontSize=13,normal={textColor=mine?DipGood:dipRow.normal.textColor}});
    if(mine){if(DipButton(new Rect(row.xMax-150,row.y,150,26),"Drop claim",navy))world.DropClaim(0,c.id);}
    else if(DipAction(new Rect(row.xMax-150,row.y,150,26),"Claim: "+world.ClaimCost(c)+" influence",reason,"Lay claim to "+c.name+".",red)){if(world.MakeClaim(0,c)){dipReply=(string.IsNullOrEmpty(pn.leader)?world.NationName(partner):pn.leader)+": \"You lay claim to "+c.name+"? We will remember this.\"";dipReplyPartner=partner;}}
    y+=30;}
   if(theirs.Count>10){GUI.Label(new Rect(left,y,wide2,20),"+"+(theirs.Count-10)+" smaller cities",hudSmall);y+=22;}
   var onMe=world.ClaimsOn(partner,0);if(onMe.Count>0){GUI.Label(new Rect(left,y+4,wide2,22),"They claim: "+string.Join(", ",onMe.Select(c=>c.name).ToArray()),new GUIStyle(dipRow){fontSize=13,normal={textColor=DipBad}});y+=28;}
  }else if(requestsView){
   // ---- Requests and guarantees (DiplomacyRequestsUI.cs) ----
   DrawRequestsPanel(partner,side,y);
  }else if(customDeal){
   // ---- A trade deal, or the terms of a peace ----
   if(DipButton(new Rect(left,y,wide2,26),"◂ Back",navy))customDeal=false;y+=32;
   DipHeading(new Rect(left,y,wide2,22),war?"Peace terms":"Trade deal");y+=28;
   GUI.Label(new Rect(left,y+3,150,22),"Payments run for",new GUIStyle(dipRow){fontSize=13});diplomacyDuration=Dropdown(new Rect(left+150,y,150,26),"length",diplomacyDuration,new[]{"10 turns","15 turns","30 turns"});y+=32;
   float colW=(side.width-44)/2;
   for(int s=0;s<2;s++){
    var col=new Rect(left+s*(colW+12),y,colW,150);DipPanel(col,.9f);GUI.Label(new Rect(col.x,col.y+4,col.width,20),s==0?"YOU GIVE":"THEY GIVE",dipCaps);
    DrawIcon(PixelArt.IconGold,new Rect(col.x+8,col.y+32,22,22));GUI.Label(new Rect(col.x+32,col.y+30,60,24),"Gold",dipRow);
    if(s==0)giveGold=Stepper(new Rect(col.x+86,col.y+30,col.width-94,24),giveGold,25,10000);else askGold=Stepper(new Rect(col.x+86,col.y+30,col.width-94,24),askGold,25,10000);
    GUI.Label(new Rect(col.x+10,col.y+60,80,24),"Per turn",dipRow);
    if(s==0)givePerTurn=Stepper(new Rect(col.x+86,col.y+60,col.width-94,24),givePerTurn,1,20);else askPerTurn=Stepper(new Rect(col.x+86,col.y+60,col.width-94,24),askPerTurn,1,20);
    GUI.Label(new Rect(col.x+10,col.y+90,col.width-20,20),"Resource",hudSmall);
    if(s==0)giveResource=Dropdown(new Rect(col.x+8,col.y+112,col.width-16,26),"give",giveResource,World.ResourceNames);else askResource=Dropdown(new Rect(col.x+8,col.y+112,col.width-16,26),"ask",askResource,World.ResourceNames);
   }
   y+=158;
   var offer=new DiplomaticOffer{from=0,to=partner,kind=war?TreatyKind.Peace:TreatyKind.Trade,duration=new[]{10,15,30}[diplomacyDuration],giveGold=DealNumber(giveGold,10000),askGold=DealNumber(askGold,10000),givePerTurn=DealNumber(givePerTurn,20),askPerTurn=DealNumber(askPerTurn,20),giveResource=giveResource,askResource=askResource};
   if(war)y=DrawPeaceTerms(partner,side,y,offer);
   string problem=world.OfferProblem(offer);int will=problem==""?world.OfferWillingness(offer):-1;
   var verdict=new Rect(left,y,wide2,40);var keep=GUI.color;GUI.color=problem!=""?new Color(.75f,.75f,.78f):will>=0?DipGood:DipBad;GUI.Label(verdict,problem!=""?problem:world.OfferAssessment(offer),new GUIStyle(dipRow){fontSize=13,wordWrap=true});GUI.color=keep;y+=44;
   float sendW=(wide2-8)*.58f;var send=new Rect(left,y,sendW,40);GUI.enabled=enabled&&problem=="";
   if(DipButton(send,war?"OFFER PEACE":"PROPOSE TRADE",teal)){var sent=world.Propose(offer);status=sent.answer;dipReply=world.LeaderReply(partner,sent,sent.status==1);dipReplyPartner=partner;if(sent.status==1){customDeal=false;giveGold=askGold=givePerTurn=askPerTurn="0";giveResource=askResource=0;ResetPeaceTerms();}}
   // The least gold now that would turn their answer; nothing else in the deal is touched.
   if(DipButton(new Rect(left+sendW+8,y,wide2-sendW-8,40),"What would it take?",navy)){int need=world.GoldToAccept(offer);if(need>0){giveGold=(DealNumber(giveGold,10000)+need).ToString();status="They would accept with "+need+" more gold now.";}else status=need==0?"They would already accept.":"No gold you hold would change their mind: "+string.Join("; ",world.AcceptanceLines(offer).Where(l=>l.Value<0).OrderBy(l=>l.Value).Take(2).Select(l=>l.Key).ToArray())+".";}
   GUI.enabled=enabled;if(problem=="")Tip(send,"Why they would answer so:",world.AcceptanceLines(offer));
  }else{
   if(war){
    // ---- War: exhaustion on both sides, and peace ----
    var p=world.Relation(0,partner);DipHeading(new Rect(left,y,wide2,22),"At war since turn "+p.warStarted);y+=28;
    DipBar(new Rect(left,y,wide2,22),"Your war exhaustion",world.Exhaustion(0,partner),new Color(.6f,.25f,.2f));y+=26;
    DipBar(new Rect(left,y,wide2,22),"Their war exhaustion",world.Exhaustion(partner,0),new Color(.6f,.25f,.2f));y+=28;
    GUI.Label(new Rect(left,y,wide2,54),"Exhaustion rises 1 a turn, 2 for each unit lost and 10 for each city lost. Every 25 points of yours costs each of your cities a content citizen. A rival at 100 always accepts peace.",new GUIStyle(dipRow){fontSize=12,wordWrap=true});y+=58;
    var o=new DiplomaticOffer{from=0,to=partner,kind=TreatyKind.Peace};string problem=world.OfferProblem(o);int will=problem==""?world.OfferWillingness(o):-1;var row=new Rect(left,y,wide2,34);
    GUI.enabled=enabled&&problem=="";if(DipButton(new Rect(row.x,row.y,190,row.height),"Offer peace",teal)){var sent=world.Propose(o);status=sent.answer;dipReply=world.LeaderReply(partner,sent,sent.status==1);dipReplyPartner=partner;}GUI.enabled=enabled;
    var keep=GUI.color;GUI.color=problem!=""?new Color(.75f,.75f,.78f):will>=0?DipGood:DipBad;GUI.Label(new Rect(row.x+198,row.y+2,row.width-198,row.height),problem!=""?problem:will>=0?"They would accept (+"+will+")":"They would refuse ("+will+")",new GUIStyle(dipRow){fontSize=13,wordWrap=true});GUI.color=keep;
    if(problem=="")Tip(row,"Peace: each side keeps what it holds, with "+Pacts.Truce+" turns of truce.",world.AcceptanceLines(o));y+=40;
    if(DipButton(new Rect(left,y,wide2,28),"Peace with terms ▸ (gold, resources)",navy))customDeal=true;y+=36;
   }else{
    // ---- The five standing agreements ----
    DipHeading(new Rect(left,y,wide2,22),"Agreements");y+=26;
    for(int k=0;k<Pacts.Count;k++){
     var row=new Rect(left,y,wide2,29);
     if(world.Pact(0,partner,k)){
      Fill(row,new Color(.1f,.3f,.16f,.55f));GUI.Label(new Rect(row.x+8,row.y+3,210,22),"✔ "+Pacts.Names[k],new GUIStyle(dipRow){fontSize=14,normal={textColor=DipGood}});
      GUI.Label(new Rect(row.x+214,row.y+4,110,22),"for "+world.PactAge(0,partner,k)+" turns",new GUIStyle(dipRow){fontSize=12});
      string held=world.PactEndReason(0,partner,k);GUI.enabled=enabled&&held=="";
      if(DipButton(new Rect(row.xMax-76,row.y+2,76,25),"End",red)){if(world.EndTreaty(0,partner,Pacts.Kind[k])){dipReply=(string.IsNullOrEmpty(pn.leader)?world.NationName(partner):pn.leader)+": \"So your word lasts only this long. We will not forget.\"";dipReplyPartner=partner;}}
      GUI.enabled=enabled;Tip(row,Pacts.Names[k]+": "+Pacts.Effects[k],null,held!=""?held:"Ending it is remembered as a broken word (opinion and trust fall)."+(k==Pacts.NonAggression||k==Pacts.Defensive?" Neither side may declare war for "+Pacts.Notice+" turns afterwards.":""));
     }else{
      var o=new DiplomaticOffer{from=0,to=partner,kind=Pacts.Kind[k]};string problem=world.OfferProblem(o);int will=problem==""?world.OfferWillingness(o):-1;
      GUI.enabled=enabled&&problem=="";
      if(DipButton(new Rect(row.x,row.y,196,row.height),Pacts.Names[k],blue)){var sent=world.Propose(o);status=sent.answer;dipReply=world.LeaderReply(partner,sent,sent.status==1);dipReplyPartner=partner;}
      GUI.enabled=enabled;var keep=GUI.color;GUI.color=problem!=""?new Color(.75f,.75f,.78f):will>=0?DipGood:DipBad;
      GUI.Label(new Rect(row.x+204,row.y+1,row.width-204,row.height),problem!=""?problem:will>=0?"Would sign (+"+will+")":"Would refuse ("+will+")",new GUIStyle(dipRow){fontSize=12,wordWrap=true});GUI.color=keep;
      Tip(row,Pacts.Names[k]+": "+Pacts.Effects[k]+" It lasts until one side ends it.",problem==""?world.AcceptanceLines(o):null,problem);
     }
     y+=32;}
    y+=4;
   }
   // ---- Actions ----
   DipHeading(new Rect(left,y,wide2,22),"Actions");y+=26;float half=(wide2-8)/2;
   {bool has=world.Envoy(0,partner);string reason=world.EnvoyReason(0,partner);
    if(DipAction(new Rect(left,y,half,29),has?"Recall envoy":"Send envoy",has?"":reason,"An envoy at their court adds 1 to their opinion each turn up to +"+Pacts.EnvoyCap+", makes their fear of you fade twice as fast, and lets trust reach "+Pacts.EnvoyTrust+" without any agreement. You have "+world.EnvoyLimit(0)+" envoy"+(world.EnvoyLimit(0)==1?"":"s")+" (more in later ages); "+(world.EnvoyLimit(0)-world.EnvoysOut(0))+" free.",teal))world.SetEnvoy(0,partner,!has);
    bool rival=world.Rival(0,partner);string rr=rival?world.EndRivalReason(0,partner):world.RivalReason(0,partner);
    if(DipAction(new Rect(left+half+8,y,half,29),rival?"End rivalry":"Name as rival",rr,rival?"Stop naming them a rival; trade can resume.":"A public declaration: all trade with them stops, you gain 1 influence a turn, both opinions fall by 30, and after "+Pacts.CauseDelay+" turns you have a just cause for war. It stands for at least "+Pacts.MinTurns+" turns.",red)){if(rival)world.EndRivalry(0,partner);else if(world.DeclareRival(0,partner)){dipReply=(string.IsNullOrEmpty(pn.leader)?world.NationName(partner):pn.leader)+": \"A rival? Then we know where we stand.\"";dipReplyPartner=partner;}}
    y+=33;}
   {int mine=world.ClaimsOn(0,partner).Count,theirs=world.ClaimsOn(partner,0).Count;
    if(DipAction(new Rect(left,y,half,29),"Claims ("+mine+")  ▸","","Claim their cities with influence. A claim is a just cause for war after "+Pacts.CauseDelay+" turns, and a claimed city can be taken without alarming the world."+(theirs>0?" They claim "+theirs+" of yours.":""),blue))claimsView=true;
    bool canGift=!war&&world.gold>=25;
    if(DipAction(new Rect(left+half+8,y,half,29),"Gift 25 gold",canGift?"":"Needs 25 gold and peace.","A gift adds 10 to their opinion of you; it fades by 1 every third turn.",blue))status=world.GiftGold(partner)?"Gift sent to "+world.NationName(partner)+".":"Gift not possible.";
    y+=33;}
   if(!war){if(DipAction(new Rect(left,y,wide2,29),"Trade deal  ▸  (gold, payments, resources)",world.Rivals(0,partner)?"No dealings between rivals.":"","Exchange gold now, gold by the turn and access to resources for a set number of turns.",blue))customDeal=true;y+=33;}
   {int open=world.requests.Count(x=>(x.from==0&&x.to==partner||x.to==0&&x.from==partner)&&(x.status==RequestStatus.Active||x.status==RequestStatus.Pending));
    if(DipAction(new Rect(left,y,wide2,29),"Requests and guarantees  ▸"+(open>0?"  ("+open+" open)":""),"","Ask them to stop a war, keep troops from your cities, join a war, guard your gateway or send aid: with a deadline, a payment on completion and, if you like, a threat. See every commitment between you.",blue)){requestsView=true;customDeal=false;claimsView=false;}y+=33;}
   // Their leader's agenda and how it judges you.
   {string agenda=world.AgendaReason(partner,0);if(agenda!=""&&y<side.yMax-110){GUI.Label(new Rect(left,y+6,wide2,40),agenda,new GUIStyle(dipRow){fontSize=12,wordWrap=true});y+=46;}}
  }
  float bw=(side.width-48)/3;
  if(DipButton(new Rect(side.x+16,side.yMax-54,bw,42),"More",navy)){diplomacyDiscuss=true;detailsScroll=Vector2.zero;}
  if(DipButton(new Rect(side.x+24+bw,side.yMax-54,bw,42),"Leave",navy)){detailsScreen="";diplomacyDiscuss=false;claimsView=false;customDeal=false;requestsView=false;GUIUtility.ExitGUI();}
  {string warReason=world.WarReason(0,partner);string cause=world.CasusBelli(0,partner);var wr=new Rect(side.x+32+2*bw,side.yMax-54,bw,42);
   if(DipAction(wr,"Declare War",warReason,warReason!=""?"War is not possible now.":cause!=""?"You have a just cause: "+cause+". Only they will hold it against you.":"You have no just cause. "+(world.CausePending(0,partner)!=""?world.CausePending(0,partner)+" ":"Name them a rival or claim a city, then wait "+Pacts.CauseDelay+" turns. ")+"War without cause makes every civilisation that knows you fear and dislike you.",red)){pendingUnit=-2;pendingWars=new[]{partner};detailsScreen="";cityScreen=false;GUIUtility.ExitGUI();}}
  DrawTip(side);
  DrawOpenDropdown();
 }
}
}
