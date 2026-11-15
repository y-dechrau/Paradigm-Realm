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
 GUIStyle dipTitle,dipName,dipQuote,dipRow,dipBadge,dipCaps,dipBig;
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
 // ---------- The Council of Realms and the Imperial Court (owner request, 14 Nov 2026) ----------
 // The owner's two mockups: the Diplomacy button opens the Council of Realms, every known leader in a painted hall (more cards,
 // smaller, as more civilisations are met) with filters, a search box and the chosen leader's summary beside the hall; talking to
 // one leader opens the Court, the leader's scene and relations on the left and agreements and actions on the right. Gold icons
 // and the hall were made in SpriteCook (Resources/UI/Diplomacy, Visuals/council-hall).
 readonly Dictionary<string,Texture2D> dipIcons=new Dictionary<string,Texture2D>();
 Texture2D DipIcon(string name){Texture2D t;if(!dipIcons.TryGetValue(name,out t)){t=Resources.Load<Texture2D>("UI/Diplomacy/dip-"+name);if(t!=null)t.filterMode=FilterMode.Trilinear;dipIcons[name]=t;}return t;}
 void DrawDipIcon(string name,Rect r,Color? tint=null){var t=DipIcon(name);if(t==null)return;var old=GUI.color;if(tint.HasValue)GUI.color=tint.Value;GUI.DrawTexture(r,t,ScaleMode.ScaleToFit,true);GUI.color=old;}
 static readonly Color CourtCream=new Color(.95f,.9f,.78f),CourtGold=new Color(.86f,.7f,.39f),CourtNavy=new Color(.08f,.12f,.22f),CourtGoldFace=new Color(.52f,.4f,.17f),CourtRed=new Color(.45f,.08f,.07f);
 GUIStyle courtTitle,courtSub,courtCaps,courtText,courtItalic;
 void CourtStyles(){
  if(courtTitle!=null)return;DiplomacyStyles();
  courtTitle=new GUIStyle(hudTitle){fontSize=30,alignment=TextAnchor.MiddleCenter,clipping=TextClipping.Overflow};courtTitle.normal.textColor=CourtCream;
  courtSub=new GUIStyle(hudLabel){fontSize=17,fontStyle=FontStyle.Italic,alignment=TextAnchor.MiddleCenter,clipping=TextClipping.Overflow};courtSub.normal.textColor=CourtCream;
  courtCaps=new GUIStyle(hudTitle){fontSize=16,alignment=TextAnchor.MiddleLeft,clipping=TextClipping.Overflow};
  courtText=new GUIStyle(hudLabel){fontSize=14,clipping=TextClipping.Overflow};courtText.normal.textColor=CourtCream;
  courtItalic=new GUIStyle(courtText){fontStyle=FontStyle.Italic,wordWrap=true};
 }
 // A heading in gold capitals with a fine rule and a small diamond after it.
 void CourtHeading(Rect r,string text){var s=new GUIStyle(courtCaps);float w=s.CalcSize(new GUIContent(text.ToUpperInvariant())).x;GUI.Label(new Rect(r.x,r.y-2,w+4,r.height+4),text.ToUpperInvariant(),s);
  float x=r.x+w+12,cy=Mathf.Round(r.center.y);if(x<r.xMax-20){Fill(new Rect(x,cy,r.xMax-x,1),new Color(CourtGold.r,CourtGold.g,CourtGold.b,.6f));float mx=x+(r.xMax-x)*.4f;Fill(new Rect(mx-3,cy-3,7,7),CourtGold);Fill(new Rect(mx-2,cy-2,5,5),new Color(.05f,.08f,.15f));}}
 // The top banner (emblem, title and subtitle, close cross) and the strip of figures along the bottom.
 bool CourtChrome(string titleText,string subtitle){
  var full=FullCanvas;Fill(full,new Color(.03f,.05f,.1f));
  {var painting=Art("title-world");if(painting!=null){GUI.DrawTexture(full,painting,ScaleMode.ScaleAndCrop);Fill(full,new Color(.03f,.05f,.11f,.86f));}}
  // A sprinkling of faint stars over the navy, fixed so it does not shimmer.
  for(int k=0;k<70;k++){int h=(k*7919+13)%100003;float sx=h%1280,sy=(h/1280*37)%800;Fill(new Rect(sx,sy,1,1),new Color(1,1,1,.12f+(k%3)*.06f));}
  GoldFrame(new Rect(-14,-14,1308,84));
  DrawDipIcon("compass",new Rect(30,10,44,44));GUI.Label(new Rect(82,12,260,26),"PARADIGM REALM",new GUIStyle(courtCaps){fontSize=17,normal={textColor=CourtCream}});
  Fill(new Rect(82,42,200,1),new Color(CourtGold.r,CourtGold.g,CourtGold.b,.6f));
  // Title and subtitle stay inside the banner's rails (owner request, 15 Nov 2026: all text fully visible).
  GUI.Label(new Rect(340,0,600,36),titleText.ToUpperInvariant(),new GUIStyle(courtTitle){fontSize=27});
  GUI.Label(new Rect(340,32,600,22),subtitle,new GUIStyle(courtSub){fontSize=15});
  {var lineInk=new Color(CourtGold.r,CourtGold.g,CourtGold.b,.6f);float tw=courtTitle.CalcSize(new GUIContent(titleText.ToUpperInvariant())).x;Fill(new Rect(640-tw/2-150,22,130,1),lineInk);Fill(new Rect(640+tw/2+20,22,130,1),lineInk);}
  var close=new Rect(1226,12,40,40);Fill(close,CourtNavy);Fill(new Rect(close.x,close.y,close.width,1),CourtGold);Fill(new Rect(close.x,close.yMax-1,close.width,1),CourtGold);Fill(new Rect(close.x,close.y,1,close.height),CourtGold);Fill(new Rect(close.xMax-1,close.y,1,close.height),CourtGold);
  GUI.Label(close,"✕",new GUIStyle(courtTitle){fontSize=22});
  return GUI.Button(close,"",GUIStyle.none);
 }
 void CourtFooter(Rect bar){GoldFrame(bar);}
 void FooterFigure(float cx,string icon,string text,string extra){var s=new GUIStyle(courtText){fontSize=16,alignment=TextAnchor.MiddleLeft};var it=new GUIStyle(courtItalic){fontSize=14,wordWrap=false,alignment=TextAnchor.MiddleLeft};
  float w=s.CalcSize(new GUIContent(text)).x+(extra==""?0:8+it.CalcSize(new GUIContent(extra)).x)+34,x=cx-w/2;DrawDipIcon(icon,new Rect(x,763,26,26));GUI.Label(new Rect(x+34,760,w,30),text,s);if(extra!="")GUI.Label(new Rect(x+40+s.CalcSize(new GUIContent(text)).x,761,200,30),extra,it);}
 void FooterRule(float x){Fill(new Rect(x,762,1,28),new Color(CourtGold.r,CourtGold.g,CourtGold.b,.5f));}
 // A court button: a face colour, a gold edge, an optional icon at the left, the caption, an optional italic note and arrow.
 bool CourtButton(Rect r,string icon,string text,Color face,string note="",bool arrow=false){
  bool hover=r.Contains(Event.current.mousePosition)&&GUI.enabled;
  Fill(r,face);Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height*.45f),new Color(1,1,1,.06f));var edge=GUI.enabled?CourtGold:new Color(.45f,.4f,.32f);Fill(new Rect(r.x,r.y,r.width,1),edge);Fill(new Rect(r.x,r.yMax-1,r.width,1),edge);Fill(new Rect(r.x,r.y,1,r.height),edge);Fill(new Rect(r.xMax-1,r.y,1,r.height),edge);
  if(hover)Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),new Color(1,1,1,.08f));
  var keep=GUI.color;if(!GUI.enabled)GUI.color=new Color(1,1,1,.45f);
  float iconW=icon==""?0:r.height-8;if(icon!="")DrawDipIcon(icon,new Rect(r.x+12,r.y+4,iconW,iconW));
  float noteW=0;if(note!=""){var ns=new GUIStyle(courtItalic){fontSize=13,wordWrap=false,alignment=TextAnchor.MiddleRight};noteW=Mathf.Min(r.width*.5f,ns.CalcSize(new GUIContent(note)).x+8);GUI.Label(new Rect(r.xMax-noteW-(arrow?30:12),r.y,noteW,r.height),note,ns);}
  var ts=new GUIStyle(courtText){fontSize=18,alignment=note==""?TextAnchor.MiddleCenter:TextAnchor.MiddleLeft};if(displayFont!=null)ts.font=displayFont;
  var tr=note==""?new Rect(r.x+iconW+12,r.y,r.width-iconW-24-(arrow?20:0),r.height):new Rect(r.x+iconW+28,r.y,r.width-iconW-noteW-60,r.height);
  while(ts.fontSize>11&&ts.CalcSize(new GUIContent(text)).x>tr.width)ts.fontSize--;GUI.Label(tr,text,ts);
  if(arrow)GUI.Label(new Rect(r.xMax-26,r.y,20,r.height),"▸",new GUIStyle(courtText){fontSize=16,alignment=TextAnchor.MiddleCenter});
  GUI.color=keep;return GUI.Button(r,"",GUIStyle.none)&&GUI.enabled;}
 bool CourtAction(Rect r,string icon,string text,string reason,string what,Color face,string note="",bool arrow=false){bool enabled=GUI.enabled;GUI.enabled=enabled&&reason=="";bool hit=CourtButton(r,icon,text,face,note,arrow);GUI.enabled=enabled;Tip(r,what,null,reason);return hit;}
 // The attitude badge: a framed plate in the attitude's colour.
 void MoodBadge(Rect r,int partner,int fontSize=15){Color ink;string mood=MoodText(partner,out ink);Fill(r,new Color(ink.r*.22f,ink.g*.22f,ink.b*.22f,.95f));var e=new Color(ink.r,ink.g,ink.b,.9f);Fill(new Rect(r.x,r.y,r.width,1),e);Fill(new Rect(r.x,r.yMax-1,r.width,1),e);Fill(new Rect(r.x,r.y,1,r.height),e);Fill(new Rect(r.xMax-1,r.y,1,r.height),e);
  var s=new GUIStyle(courtCaps){fontSize=fontSize,alignment=TextAnchor.MiddleCenter};s.normal.textColor=ink;GUI.Label(r,mood,s);}
 // Which group of the Council a leader falls in: 0 allies, 1 neutral, 2 rivals, 3 at war.
 int CouncilGroup(int o){if(world.AtWar(0,o))return 3;int a=world.Attitude(o,0);if(a==Attitudes.Cordial||a==Attitudes.Friendly||a==Attitudes.Loyal)return 0;if(a==Attitudes.Rival||a==Attitudes.Hostile||a==Attitudes.Unfriendly||world.Rival(0,o))return 2;return 1;}
 static readonly string[] CouncilGroupIcons={"laurel","scales","swords","swords"};
 int councilFilter,councilPick=-1;string councilSearch="";
 void DrawCouncil(int[] known){
  if(CourtChrome("The Council of Realms","Known Civilisations")){detailsScreen="";GUIUtility.ExitGUI();}
  // Filters and the search box.
  string[] tabs={"All Contacts ("+known.Length+")","Allies","Neutral","Rivals","At War"};string[] tabIcons={"","laurel","scales","swords","swords"};
  GoldFrame(new Rect(8,72,940,46));
  for(int k=0;k<tabs.Length;k++){var tr=new Rect(22+k*184,80,174,30);bool on=councilFilter==k;
   if(CourtButton(tr,tabIcons[k],tabs[k],on?CourtGoldFace:CourtNavy))councilFilter=k;
   if(on){Fill(new Rect(tr.x,tr.y,tr.width,2),new Color(1f,.86f,.5f));}}
  GoldFrame(new Rect(956,72,316,46));
  {var box=new Rect(972,80,284,30);Fill(box,new Color(.04f,.06f,.12f));DrawDipIcon("compass",new Rect(box.x+6,box.y+5,20,20));
   var fs=new GUIStyle(courtText){fontSize=15,alignment=TextAnchor.MiddleLeft};fs.normal.background=null;fs.focused.background=null;fs.focused.textColor=CourtCream;fs.hover.textColor=CourtCream;
   GUI.SetNextControlName("councilSearch");councilSearch=GUI.TextField(new Rect(box.x+32,box.y,box.width-38,box.height),councilSearch??"",40,fs);
   if(string.IsNullOrEmpty(councilSearch)&&GUI.GetNameOfFocusedControl()!="councilSearch")GUI.Label(new Rect(box.x+34,box.y,box.width-40,box.height),"Search contacts...",new GUIStyle(courtItalic){fontSize=15,wordWrap=false,alignment=TextAnchor.MiddleLeft,normal={textColor=new Color(.7f,.66f,.58f)}});}
  string q=(councilSearch??"").Trim().ToLowerInvariant();
  var shown=known.Where(o=>(councilFilter==0||CouncilGroup(o)==councilFilter-1)&&(q==""||world.NationName(o).ToLowerInvariant().Contains(q)||(world.nations[o].leader??"").ToLowerInvariant().Contains(q))).ToArray();
  if(councilPick<0||!known.Contains(councilPick))councilPick=shown.Length>0?shown[0]:known[0];
  // ---- The hall ----
  var hall=new Rect(8,124,940,620);GoldFrame(hall);var inner=new Rect(hall.x+14,hall.y+14,hall.width-28,hall.height-28);
  {var art=Art("council-hall");if(art!=null)DrawCropped(inner,art,art.width/(float)art.height);else Fill(inner,new Color(.2f,.18f,.16f));Fill(new Rect(inner.x,inner.yMax-34,inner.width,34),new Color(.03f,.05f,.1f,.82f));}
  var area=new Rect(inner.x+10,inner.y+12,inner.width-20,inner.height-58);
  if(shown.Length==0)GUI.Label(area,"No known civilisation in this group.",new GUIStyle(courtTitle){fontSize=20});
  else{
   // The cards grow fewer per row only while they still fit: one row up to five, two up to twelve, then three, so the hall
   // fills as more civilisations are met.
   int rows=shown.Length<=5?1:shown.Length<=12?2:3,cols=(shown.Length+rows-1)/rows;float gap=12;
   // A card holds a seated leader on its throne (wider than a portrait), so it may be up to about as wide as it is tall.
   float ch=Mathf.Min(rows==1?430:300,(area.height-(rows-1)*gap)/rows),cw=Mathf.Min((area.width-(cols-1)*gap)/cols,ch*.92f);
   for(int k=0;k<shown.Length;k++){int o=shown[k];int row=k/cols,inRow=Math.Min(cols,shown.Length-row*cols);float x0=area.center.x-(inRow*cw+(inRow-1)*gap)/2,y0=area.center.y-(rows*ch+(rows-1)*gap)/2;
    var card=new Rect(x0+(k%cols)*(cw+gap),y0+row*(ch+gap),cw,ch);DrawCouncilCard(card,o,o==councilPick);
    if(GUI.Button(card,"",GUIStyle.none)){if(councilPick==o&&Event.current.clickCount>=2){OpenCourt(o);GUIUtility.ExitGUI();}councilPick=o;}}}
  {var lg=new Rect(inner.x+12,inner.yMax-32,inner.width-24,30);GUI.Label(lg,"Select a leader, then Open Diplomacy (or double-click)",new GUIStyle(courtItalic){fontSize=14,wordWrap=false,alignment=TextAnchor.MiddleLeft});
   string[] names={"Allied","Neutral","Rival","At War"};float x=lg.xMax-4;
   for(int k=3;k>=0;k--){var s=new GUIStyle(courtItalic){fontSize=13,wordWrap=false,alignment=TextAnchor.MiddleLeft};string t="= "+names[k];float w=s.CalcSize(new GUIContent(t)).x;x-=w;GUI.Label(new Rect(x,lg.y,w+4,lg.height),t,s);x-=24;DrawDipIcon(CouncilGroupIcons[k],new Rect(x,lg.y+5,20,20),k==3?new Color(1f,.45f,.4f):(Color?)null);x-=18;}}
  // ---- The chosen leader ----
  DrawCouncilSide(new Rect(956,124,316,620),councilPick);
  // ---- Figures along the bottom ----
  CourtFooter(new Rect(-14,748,1308,66));
  FooterFigure(150,"people",known.Length+" known civilisation"+(known.Length==1?"":"s"),"");FooterRule(330);
  FooterFigure(520,"compass","Influence "+world.nations[0].influence,"(+"+world.InfluencePerTurn(0)+" / turn)");FooterRule(720);
  FooterFigure(860,"people","Envoys "+world.EnvoysOut(0)+" / "+world.EnvoyLimit(0),"");FooterRule(990);
  if(CourtButton(new Rect(1030,758,226,34),"","Return to Map",CourtNavy)){detailsScreen="";GUIUtility.ExitGUI();}
  DrawTip(new Rect(956,124,316,620));
 }
 void OpenCourt(int o){diplomacyPartner=o;diplomacyChooser=false;ResetDiplomacyUI();}
 // The seated leaders (owner's art, 15 Nov 2026; Resources/Leaders/Seated/seated-NNN by civilisation number): each sits on its
 // throne in the hall, as in the owner's blueprint, its banner hanging behind, a gold name plate across the foot of the throne
 // and its attitude beneath. The chosen leader glows gold. A civilisation without a seated picture (a custom one) keeps its card.
 readonly Dictionary<int,Texture2D> seatedArt=new Dictionary<int,Texture2D>();
 Texture2D SeatedLeader(int o){int civ=world.nations[o].civ;if(civ<0)return null;Texture2D t;if(!seatedArt.TryGetValue(civ,out t)){t=Resources.Load<Texture2D>("Leaders/Seated/seated-"+civ.ToString("000"));if(t!=null)t.filterMode=FilterMode.Trilinear;seatedArt[civ]=t;}return t;}
 void DrawCouncilCard(Rect card,int o,bool chosen){
  var seated=SeatedLeader(o);if(seated==null){DrawCouncilPortraitCard(card,o,chosen);return;}
  float plateH=Mathf.Clamp(card.height*.1f,20,30),badgeH=Mathf.Clamp(card.height*.09f,18,26);
  // The figure stands on the plates' line, as large as the card allows without cropping crown, feet or seat.
  var box=new Rect(card.x,card.y+card.height*.13f,card.width,card.height*.87f-badgeH*.7f);
  float aspect=seated.width/(float)seated.height,fh=box.height,fw=fh*aspect;if(fw>box.width){fw=box.width;fh=fw/aspect;}
  var fig=new Rect(card.center.x-fw/2,box.yMax-fh,fw,fh);
  DrawCouncilBanner(new Rect(card.center.x-fw*.16f,card.y,fw*.32f,Mathf.Max(fh*.42f,card.height*.3f)),o);
  var old=GUI.color;
  if(chosen){
   // A pool of light on the floor, then the leader's own outline traced in gold, widest and faintest first.
   GUI.color=new Color(1f,.8f,.35f,.35f);GUI.DrawTexture(new Rect(fig.center.x-fig.width*.6f,fig.yMax-fig.height*.16f,fig.width*1.2f,fig.height*.24f),IsoPicture(4,8,0),ScaleMode.StretchToFill,true);
   var glow=new Color(1f,.84f,.42f);foreach(float grow in new[]{14f,9f,5f,2f}){float a=grow>12?.28f:grow>8?.42f:grow>4?.62f:.9f;GUI.color=new Color(glow.r,glow.g,glow.b,a);GUI.DrawTexture(new Rect(fig.x-grow,fig.y-grow,fig.width+grow*2,fig.height+grow*2),seated,ScaleMode.StretchToFill,true);}GUI.color=old;}
  GUI.DrawTexture(fig,seated,ScaleMode.StretchToFill,true);
  int waiting=world.offers.Count(x=>x.to==0&&x.from==o&&x.status==0&&x.expires>world.turn);if(waiting>0){var b=new Rect(fig.xMax-26,fig.y+6,22,22);Fill(b,new Color(.75f,.18f,.14f));GUI.Label(b,waiting.ToString(),dipBadge);}
  // Name plate across the foot of the throne.
  // The full name: one line if it fits at a readable size, otherwise two, and the plate grows to hold it.
  float nameH;var nameStyle=FitName(world.NationName(o).ToUpperInvariant(),card.width*.94f-10,new GUIStyle(courtCaps){alignment=TextAnchor.MiddleCenter,normal={textColor=CourtCream}},15,11,9,out nameH);plateH=Mathf.Max(plateH,nameH+6);
  var plate=new Rect(card.center.x-card.width*.47f,card.yMax-plateH-badgeH+2,card.width*.94f,plateH);
  Fill(new Rect(plate.x-2,plate.y-2,plate.width+4,plate.height+4),new Color(.18f,.12f,.05f,.95f));Fill(plate,new Color(.05f,.08f,.15f,.97f));
  {var e=chosen?new Color(1f,.86f,.5f):CourtGold;Fill(new Rect(plate.x,plate.y,plate.width,1),e);Fill(new Rect(plate.x,plate.yMax-1,plate.width,1),e);Fill(new Rect(plate.x,plate.y,1,plate.height),e);Fill(new Rect(plate.xMax-1,plate.y,1,plate.height),e);}
  GUI.Label(new Rect(plate.x+5,plate.y,plate.width-10,plate.height),world.NationName(o).ToUpperInvariant(),nameStyle);
  // The attitude beneath, framed in its own colour.
  Color ink;string mood=MoodText(o,out ink);float badgeW=card.width*.76f;{var probe=new GUIStyle(courtCaps){fontSize=10};float need=probe.CalcSize(new GUIContent(mood)).x+badgeH+14;badgeW=Mathf.Min(card.width,Mathf.Max(badgeW,need));}var badge=new Rect(card.center.x-badgeW/2,plate.yMax-1,badgeW,badgeH);
  Fill(badge,new Color(ink.r*.2f,ink.g*.2f,ink.b*.2f,.97f));{var e=new Color(ink.r,ink.g,ink.b,.85f);Fill(new Rect(badge.x,badge.y,badge.width,1),e);Fill(new Rect(badge.x,badge.yMax-1,badge.width,1),e);Fill(new Rect(badge.x,badge.y,1,badge.height),e);Fill(new Rect(badge.xMax-1,badge.y,1,badge.height),e);}
  var ms=new GUIStyle(courtCaps){fontSize=13,alignment=TextAnchor.MiddleLeft};ms.normal.textColor=ink;float iconS=badge.height-6;while(ms.fontSize>10&&ms.CalcSize(new GUIContent(mood)).x>badge.width-iconS-14)ms.fontSize--;
  float mw=ms.CalcSize(new GUIContent(mood)).x,mx=badge.center.x-(mw+iconS+6)/2;int g=CouncilGroup(o);DrawDipIcon(CouncilGroupIcons[g],new Rect(mx,badge.y+3,iconS,iconS),g==3?new Color(1f,.45f,.4f):(Color?)null);GUI.Label(new Rect(mx+iconS+6,badge.y,mw+6,badge.height),mood,ms);
 }
 // A banner on a gilded bar: the nation's colour, its flag at the top, a swallow-tail at the foot.
 void DrawCouncilBanner(Rect r,int o){
  var gold=CourtGold;Fill(new Rect(r.x-r.width*.18f,r.y,r.width*1.36f,Mathf.Max(3,r.height*.03f)),gold);
  float tail=Mathf.Max(6,r.height*.1f);var cloth=nations[o];cloth=new Color(cloth.r*.55f,cloth.g*.55f,cloth.b*.55f,1);
  var body=new Rect(r.x,r.y+3,r.width,r.height-tail-3);Fill(body,cloth);
  Fill(new Rect(r.x,body.yMax,r.width*.42f,tail),cloth);Fill(new Rect(r.xMax-r.width*.42f,body.yMax,r.width*.42f,tail),cloth);
  Fill(new Rect(r.x,body.y,2,body.height+tail),gold);Fill(new Rect(r.xMax-2,body.y,2,body.height+tail),gold);
  var flag=FlagTexture(o);if(flag!=null){float fw=r.width-10,fh=fw/1.5f;GUI.DrawTexture(new Rect(r.x+5,body.y+6,fw,fh),flag,ScaleMode.StretchToFill,true);}
 }
 // A name at the largest size that shows it whole: one line down to minOne, otherwise two lines down to minTwo, never cut off
 // (owner request, 15 Nov 2026). Returns the style and the height it needs at the given width.
 GUIStyle FitName(string text,float width,GUIStyle look,int maxSize,int minOne,int minTwo,out float height){
  var gc=new GUIContent(text);var s=new GUIStyle(look){fontSize=maxSize,wordWrap=false,clipping=TextClipping.Overflow};
  while(s.fontSize>minOne&&s.CalcSize(gc).x>width)s.fontSize--;
  if(s.CalcSize(gc).x<=width){height=s.CalcSize(gc).y;return s;}
  s.wordWrap=true;s.fontSize=maxSize;var words=text.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
  while(s.fontSize>minTwo){var one=new GUIStyle(s){wordWrap=false};float widest=words.Length==0?0:words.Max(word=>one.CalcSize(new GUIContent(word)).x);float line=one.CalcSize(new GUIContent("Ag")).y;if(widest<=width&&s.CalcHeight(gc,width)<=line*2.2f)break;s.fontSize--;}
  height=s.CalcHeight(gc,width);return s;}
 // Portraits in a gold-ringed circle (owner request, 15 Nov 2026): a round copy of each picture, the top of it (the face), made once.
 readonly Dictionary<Texture2D,Texture2D> roundPortraits=new Dictionary<Texture2D,Texture2D>();Texture2D discTexture;
 Texture2D RoundCopy(Texture2D src){
  if(src==null)return null;Texture2D made;if(roundPortraits.TryGetValue(src,out made))return made;
  try{int side=Math.Min(src.width,src.height),x0=(src.width-side)/2,y0=src.height-side;var px=src.GetPixels32();var outPx=new Color32[side*side];float r=side/2f,edge=Mathf.Max(1.5f,side/160f);
   for(int y=0;y<side;y++)for(int x=0;x<side;x++){var c=px[(y0+y)*src.width+x0+x];float d=Mathf.Sqrt((x+.5f-r)*(x+.5f-r)+(y+.5f-r)*(y+.5f-r));float a=Mathf.Clamp01((r-d)/edge);c.a=(byte)(c.a*a);outPx[y*side+x]=c;}
   made=new Texture2D(side,side,TextureFormat.RGBA32,true);made.SetPixels32(outPx);made.Apply(true);made.filterMode=FilterMode.Trilinear;made.wrapMode=TextureWrapMode.Clamp;}
  catch(Exception){made=src;}
  roundPortraits[src]=made;return made;}
 Texture2D Disc(){if(discTexture!=null)return discTexture;int n=128;var px=new Color32[n*n];float r=n/2f;for(int y=0;y<n;y++)for(int x=0;x<n;x++){float d=Mathf.Sqrt((x+.5f-r)*(x+.5f-r)+(y+.5f-r)*(y+.5f-r));px[y*n+x]=new Color32(255,255,255,(byte)(255*Mathf.Clamp01(r-d)));}
  discTexture=new Texture2D(n,n,TextureFormat.RGBA32,true);discTexture.SetPixels32(px);discTexture.Apply(true);discTexture.filterMode=FilterMode.Bilinear;return discTexture;}
 void DrawRoundPortrait(Rect r,Texture2D face){
  var old=GUI.color;GUI.color=new Color(.18f,.12f,.05f);GUI.DrawTexture(new Rect(r.x-5,r.y-5,r.width+10,r.height+10),Disc());GUI.color=CourtGold;GUI.DrawTexture(new Rect(r.x-4,r.y-4,r.width+8,r.height+8),Disc());
  GUI.color=new Color(.12f,.15f,.24f);GUI.DrawTexture(new Rect(r.x-1,r.y-1,r.width+2,r.height+2),Disc());GUI.color=old;
  var round=RoundCopy(face);if(round!=null)GUI.DrawTexture(r,round,ScaleMode.StretchToFill,true);}
 void DrawCouncilPortraitCard(Rect card,int o,bool chosen){
  var pn=world.nations[o];var pic=new Rect(card.x,card.y,card.width,card.height-58);
  if(chosen){Fill(new Rect(card.x-6,card.y-6,card.width+12,card.height+12),new Color(1f,.82f,.4f,.18f));Fill(new Rect(card.x-3,card.y-3,card.width+6,card.height+6),new Color(1f,.82f,.4f,.35f));}
  Fill(new Rect(pic.x-2,pic.y-2,pic.width+4,pic.height+4),chosen?new Color(1f,.86f,.5f):CourtGold);
  {var bd=Art("diplomacy-backdrop");if(bd!=null)DrawCropped(pic,bd,BackdropAspect);else Fill(pic,new Color(.15f,.2f,.3f));}
  var face=Portrait(pn.portrait,world.Era(o));if(face!=null)DrawPortrait(pic,face);
  {var flag=FlagTexture(o);if(flag!=null){var fr=new Rect(pic.x+6,pic.y+6,Mathf.Min(42,pic.width*.28f),Mathf.Min(28,pic.width*.19f));Fill(new Rect(fr.x-1,fr.y-1,fr.width+2,fr.height+2),CourtGold);GUI.DrawTexture(fr,flag,ScaleMode.StretchToFill,true);}}
  int waiting=world.offers.Count(x=>x.to==0&&x.from==o&&x.status==0&&x.expires>world.turn);if(waiting>0){var b=new Rect(pic.xMax-28,pic.y+6,22,22);Fill(b,new Color(.75f,.18f,.14f));GUI.Label(b,waiting.ToString(),dipBadge);}
  var plate=new Rect(card.x-4,card.yMax-56,card.width+8,56);Fill(plate,new Color(.04f,.06f,.12f,.97f));Fill(new Rect(plate.x,plate.y,plate.width,1),CourtGold);Fill(new Rect(plate.x,plate.yMax-1,plate.width,1),CourtGold);Fill(new Rect(plate.x,plate.y,1,plate.height),CourtGold);Fill(new Rect(plate.xMax-1,plate.y,1,plate.height),CourtGold);
  FitLabel(new Rect(plate.x+4,plate.y+3,plate.width-8,24),world.NationName(o).ToUpperInvariant(),new GUIStyle(courtCaps){fontSize=15,alignment=TextAnchor.MiddleCenter,normal={textColor=CourtCream}},9);
  Color ink;string mood=MoodText(o,out ink);var ms=new GUIStyle(courtCaps){fontSize=13,alignment=TextAnchor.MiddleLeft};ms.normal.textColor=ink;while(ms.fontSize>9&&ms.CalcSize(new GUIContent(mood)).x>plate.width-40)ms.fontSize--;
  float mw=ms.CalcSize(new GUIContent(mood)).x,mx=plate.center.x-(mw+24)/2;int g=CouncilGroup(o);DrawDipIcon(CouncilGroupIcons[g],new Rect(mx,plate.y+30,18,18),g==3?new Color(1f,.45f,.4f):(Color?)null);GUI.Label(new Rect(mx+24,plate.y+27,mw+6,24),mood,ms);
 }
 void DrawCouncilSide(Rect side,int o){
  GoldFrame(side);var pn=world.nations[o];float x=side.x+18,w=side.width-36,y=side.y+18;
  {var flag=FlagTexture(o);var fr=new Rect(x,y+2,60,40);Fill(new Rect(fr.x-2,fr.y-2,fr.width+4,fr.height+4),CourtGold);if(flag!=null)GUI.DrawTexture(fr,flag,ScaleMode.StretchToFill,true);}
  {float h;var st=FitName(world.NationName(o).ToUpperInvariant(),w-70,new GUIStyle(courtCaps){alignment=TextAnchor.MiddleLeft,normal={textColor=CourtCream}},21,15,12,out h);GUI.Label(new Rect(x+70,y,w-70,Mathf.Max(44,h)),world.NationName(o).ToUpperInvariant(),st);y+=Mathf.Max(50,h+6);}
  DrawRoundPortrait(new Rect(x,y,96,96),Portrait(pn.portrait,world.Era(o)));
  {var it=new GUIStyle(courtItalic){fontSize=16,alignment=TextAnchor.UpperLeft};GUI.Label(new Rect(x+108,y,w-108,44),string.IsNullOrEmpty(pn.leader)?world.NationName(o):pn.leader,it);
   GUI.Label(new Rect(x+108,y+40,w-108,22),DiplomacyFlavours.PostureNames[world.Posture(o)],new GUIStyle(it){fontSize=14});MoodBadge(new Rect(x+108,y+66,w-108,28),o,14);}
  y+=108;
  CourtHeading(new Rect(x,y,w,22),"Relations");y+=26;
  {string[] icons={"handshake","mask","crown"};string[] text={"Trust: "+world.Trust(o,0),"Fear: "+world.Threat(o,0),"Strength: "+Attitudes.Power[world.PowerLevel(0,o)]};
   for(int k=0;k<3;k++){var row=new Rect(x,y,w,27);Fill(row,new Color(.06f,.09f,.17f,.9f));Fill(new Rect(row.x,row.yMax-1,row.width,1),new Color(CourtGold.r,CourtGold.g,CourtGold.b,.35f));DrawDipIcon(icons[k],new Rect(row.x+8,row.y+3,22,22));GUI.Label(new Rect(row.x+40,row.y,row.width-44,row.height),text[k],new GUIStyle(courtText){fontSize=15,alignment=TextAnchor.MiddleLeft});y+=29;}}
  y+=4;CourtHeading(new Rect(x,y,w,22),"Agreements");y+=26;
  {var held=Enumerable.Range(0,Pacts.Count).Where(k=>world.Pact(0,o,k)).Select(k=>Pacts.Names[k]).ToList();DrawDipIcon("book",new Rect(x+4,y+2,22,22));
   GUI.Label(new Rect(x+34,y,w-34,44),held.Count==0?"No active agreements.":string.Join(", ",held.ToArray())+".",new GUIStyle(courtItalic){fontSize=14});y+=40;}
  CourtHeading(new Rect(x,y,w,22),"Latest Contact");y+=26;
  {string said=world.Greeting(o);var qs=new GUIStyle(courtItalic){fontSize=14,alignment=TextAnchor.UpperCenter};var qr=new Rect(x,y,w,side.yMax-16-4*38-6-y);while(qs.fontSize>10&&qs.CalcHeight(new GUIContent("“"+said+"”"),qr.width)>qr.height)qs.fontSize--;GUI.Label(qr,"“"+said+"”",qs);y+=66;}
  float by=side.yMax-16-4*38;bool war=world.AtWar(0,o);
  if(CourtButton(new Rect(x,by,w,34),"handshake","Open Diplomacy",CourtGoldFace)){OpenCourt(o);GUIUtility.ExitGUI();}
  if(CourtAction(new Rect(x,by+38,w,34),"coins",war?"Offer Peace Terms":"Propose Trade",!war&&world.Rivals(0,o)?"No dealings between rivals.":"",war?"Offer peace with gold and resources.":"Exchange gold, payments and resources.",CourtNavy)){OpenCourt(o);customDeal=true;GUIUtility.ExitGUI();}
  if(CourtButton(new Rect(x,by+76,w,34),"scroll","View Agreements",CourtNavy)){OpenCourt(o);GUIUtility.ExitGUI();}
  if(CourtButton(new Rect(x,by+114,w,34),"gateway","View Civilisation",CourtNavy)){OpenCourt(o);diplomacyDiscuss=true;detailsScroll=Vector2.zero;GUIUtility.ExitGUI();}
 }
 // ---- The Court: one leader ----
 static readonly string[] PactIcons={"handshake","dove","coins","book","shield","gateway"};
 void DrawCourtScene(int partner,Rect scene){
  var pn=world.nations[partner];GoldFrame(scene);var inner=new Rect(scene.x+16,scene.y+16,scene.width-32,scene.height-32);
  var backdrop=Art("diplomacy-backdrop");if(backdrop!=null)DrawCropped(inner,backdrop,BackdropAspect);else Fill(inner,new Color(.3f,.28f,.36f));
  var face=Portrait(pn.portrait,world.Era(partner));if(face!=null){float h=inner.height-6,w2=Mathf.Min(inner.width-40,h*.9f);DrawPortrait(new Rect(inner.center.x-w2/2,inner.y+6,w2,h),face);}
  {var flag=FlagTexture(partner);if(flag!=null){var fr=new Rect(inner.x+12,inner.y+12,84,56);Fill(new Rect(fr.x-3,fr.y-3,fr.width+6,fr.height+6),new Color(.16f,.11f,.05f));Fill(new Rect(fr.x-2,fr.y-2,fr.width+4,fr.height+4),CourtGold);GUI.DrawTexture(fr,flag,ScaleMode.StretchToFill,true);}}
  // The name plate across the foot of the picture, laurels either side.
  // The name plate across the foot of the picture, laurels either side; it grows upward when the leader's name needs two lines.
  string who=(string.IsNullOrEmpty(pn.leader)?world.NationName(partner):pn.leader).ToUpperInvariant();float whoH;var whoStyle=FitName(who,460-132,new GUIStyle(courtCaps){alignment=TextAnchor.MiddleCenter,normal={textColor=CourtCream}},24,16,12,out whoH);float nameRow=Mathf.Max(38,whoH+4);
  var plate=new Rect(inner.center.x-230,inner.yMax-78-(nameRow-38),460,70+(nameRow-38));Fill(new Rect(plate.x-4,plate.y-4,plate.width+8,plate.height+8),new Color(.16f,.11f,.05f,.9f));Fill(plate,new Color(.05f,.08f,.15f,.97f));
  var e=CourtGold;Fill(new Rect(plate.x,plate.y,plate.width,2),e);Fill(new Rect(plate.x,plate.yMax-2,plate.width,2),e);Fill(new Rect(plate.x,plate.y,2,plate.height),e);Fill(new Rect(plate.xMax-2,plate.y,2,plate.height),e);
  DrawDipIcon("laurel",new Rect(plate.x+8,plate.center.y-26,52,52));DrawDipIcon("laurel",new Rect(plate.xMax-60,plate.center.y-26,52,52));
  GUI.Label(new Rect(plate.x+66,plate.y+2,plate.width-132,nameRow),who,whoStyle);
  FitLabel(new Rect(plate.x+66,plate.y+nameRow,plate.width-132,26),world.NationName(partner)+"  •  "+DiplomacyFlavours.PostureNames[world.Posture(partner)],new GUIStyle(courtText){fontSize=16,alignment=TextAnchor.MiddleCenter},10);
 }
 void DrawCourtRelations(int partner,Rect box){
  GoldFrame(box);float x=box.x+24,w=box.width-48,y=box.y+16;
  GUI.Label(new Rect(x,y,140,30),"RELATIONS  •",new GUIStyle(courtCaps){fontSize=19});MoodBadge(new Rect(x+150,y+1,170,28),partner,14);
  Tip(new Rect(x+150,y+1,170,28),Attitudes.Names[world.Attitude(partner,0)]+": "+Attitudes.Texts[world.Attitude(partner,0)]);y+=38;
  var lines=world.OpinionLines(partner,0);var fade=world.MemoryLines(partner,0).Where(m=>m.turnsLeft>0).GroupBy(m=>m.text).ToDictionary(g=>g.Key,g=>g.First().turnsLeft);
  int room=(int)((box.yMax-16-y)/22)-1,shown=Math.Max(0,Math.Min(room,lines.Count));
  for(int k=0;k<shown;k++){var l=lines[k];int left;GUI.Label(new Rect(x+4,y,16,22),"•",courtText);FitLabel(new Rect(x+20,y,w-90,22),l.Key+(fade.TryGetValue(l.Key,out left)?"  (fades in ~"+left+" turns)":""),new GUIStyle(courtText){fontSize=14},10);
   GUI.Label(new Rect(x+w-70,y,70,22),(l.Value>0?"+":"")+l.Value,new GUIStyle(courtText){fontSize=14,alignment=TextAnchor.MiddleRight,normal={textColor=l.Value>0?DipGood:DipBad}});y+=22;}
  GUI.Label(new Rect(x+4,y,16,22),"•",courtText);FitLabel(new Rect(x+20,y,w-20,22),Attitudes.Texts[world.Attitude(partner,0)]+(lines.Count>shown?"   (+"+(lines.Count-shown)+" more under More)":""),new GUIStyle(courtText){fontSize=14},10);
 }
 void DrawDiplomacyScreen(){
  DiplomacyStyles();CourtStyles();
  DropdownClicks();
  tipTitle=null;
  var known=Enumerable.Range(1,world.civCount-1).Where(i=>world.met[i]).ToArray();
  if(known.Length==0){
   if(CourtChrome("The Council of Realms","Known Civilisations")){detailsScreen="";GUIUtility.ExitGUI();}
   var box=new Rect(340,290,600,170);GoldFrame(box);GUI.Label(new Rect(340,318,600,40),"No civilisations met yet",new GUIStyle(courtTitle){fontSize=22});
   GUI.Label(new Rect(340,356,600,30),"Explore: seeing another people's borders, units or cities opens the talks.",new GUIStyle(courtText){alignment=TextAnchor.MiddleCenter});
   if(CourtButton(new Rect(560,398,160,40),"","Leave",CourtNavy)){detailsScreen="";GUIUtility.ExitGUI();}return;}
  if(diplomacyChooser){DrawCouncil(known);DrawOpenDropdown();return;}
  if(!known.Contains(diplomacyPartner))diplomacyPartner=known[0];int partner=diplomacyPartner;var pn=world.nations[partner];bool war=world.AtWar(0,partner);
  if(CourtChrome("The Imperial Court","Diplomacy & Agreements")){detailsScreen="";diplomacyDiscuss=false;claimsView=false;customDeal=false;requestsView=false;GUIUtility.ExitGUI();}
  CourtFooter(new Rect(-14,748,1308,66));
  FooterFigure(260,"compass","Influence "+world.nations[0].influence,"(+"+world.InfluencePerTurn(0)+" / turn)");FooterRule(470);
  FooterFigure(640,"people","Envoys "+world.EnvoysOut(0)+" / "+world.EnvoyLimit(0),"");FooterRule(820);
  FooterFigure(1010,"swords","Rivals "+world.RivalCount(0)+" / "+Pacts.MaxRivals,"");
  if(diplomacyDiscuss){
   // The inbox, the full record and the finer details, in a scrolling page across the whole width.
   var wide=new Rect(8,72,1264,672);GoldFrame(wide);
   GUILayout.BeginArea(new Rect(wide.x+28,wide.y+22,wide.width-56,wide.height-90));detailsScroll=GUILayout.BeginScrollView(detailsScroll,false,false,GUIStyle.none,GUIStyle.none);detailsScroll.x=0;DrawAdvancedDiplomacy();GUILayout.EndScrollView();GUILayout.EndArea();
   if(CourtButton(new Rect(wide.x+28,wide.yMax-60,wide.width-56,40),"scroll","Back to the talks",CourtNavy))diplomacyDiscuss=false;
   if(CourtButton(new Rect(wide.x+28,wide.y+18,180,30),"","◂ All leaders",CourtNavy)){diplomacyDiscuss=false;diplomacyChooser=true;}
   DrawOpenDropdown();return;}
  // ---- Left: the leader's scene, then why they feel as they do ----
  DrawCourtScene(partner,new Rect(8,72,644,500));
  DrawCourtRelations(partner,new Rect(8,578,644,166));
  // ---- Right: everything that can be done with them ----
  var side=new Rect(660,72,612,672);GoldFrame(side);
  float y=side.y+22;float left=side.x+24,wide2=side.width-48;bool enabled=GUI.enabled;
  Color navy=CourtNavy,teal=new Color(.07f,.38f,.4f),blue=CourtNavy,red=CourtRed;
  {var theirFlag=FlagTexture(partner);var fr=new Rect(left,y,80,52);Fill(new Rect(fr.x-2,fr.y-2,fr.width+4,fr.height+4),CourtGold);if(theirFlag!=null)GUI.DrawTexture(fr,theirFlag,ScaleMode.StretchToFill,true);}
  {string nm=world.NationName(partner).ToUpperInvariant();float h;var st=FitName(nm,wide2-96-180,new GUIStyle(courtCaps){alignment=TextAnchor.MiddleLeft,normal={textColor=CourtCream}},27,17,13,out h);GUI.Label(new Rect(left+96,y-4,wide2-96-180,38),nm,st);}
  GUI.Label(new Rect(left+96,y+32,wide2-96,24),(string.IsNullOrEmpty(pn.leader)?"":pn.leader+" — ")+DiplomacyFlavours.PostureNames[world.Posture(partner)],new GUIStyle(courtItalic){fontSize=16,wordWrap=false});
  {var badge=new Rect(side.xMax-24-170,y+2,170,30);MoodBadge(badge,partner,15);Tip(badge,Attitudes.Names[world.Attitude(partner,0)]+": "+Attitudes.Texts[world.Attitude(partner,0)]);}
  y+=66;
  {var quote=new Rect(left,y,wide2,58);Fill(quote,new Color(.03f,.05f,.1f,.92f));Fill(new Rect(quote.x,quote.y,quote.width,1),CourtGold);Fill(new Rect(quote.x,quote.yMax-1,quote.width,1),CourtGold);Fill(new Rect(quote.x,quote.y,1,quote.height),CourtGold);Fill(new Rect(quote.xMax-1,quote.y,1,quote.height),CourtGold);
   string said=dipReplyPartner==partner&&!string.IsNullOrEmpty(dipReply)?dipReply:"“"+world.Greeting(partner)+"”";var qs=new GUIStyle(courtItalic){fontSize=17,alignment=TextAnchor.MiddleCenter};var qr=new Rect(quote.x+14,quote.y+3,quote.width-28,quote.height-6);while(qs.fontSize>11&&qs.CalcHeight(new GUIContent(said),qr.width)>qr.height)qs.fontSize--;GUI.Label(qr,said,qs);}
  y+=66;
  // Their strength beside yours, their trust in you, their fear of you.
  {float third=wide2/3;string[] icons={"crown","handshake","mask"};string[] text={"Strength: "+Attitudes.Power[world.PowerLevel(0,partner)],"Trust: "+world.Trust(partner,0),"Fear: "+world.Threat(partner,0)};
   for(int k=0;k<3;k++){var cell=new Rect(left+k*third,y,third,26);var s=new GUIStyle(courtText){fontSize=15,alignment=TextAnchor.MiddleLeft};float tw=s.CalcSize(new GUIContent(text[k])).x+30;float cx=cell.center.x-tw/2;DrawDipIcon(icons[k],new Rect(cx,cell.y+1,24,24));GUI.Label(new Rect(cx+30,cell.y,tw,26),text[k],s);if(k>0)Fill(new Rect(cell.x,cell.y+3,1,20),new Color(CourtGold.r,CourtGold.g,CourtGold.b,.5f));}
   Tip(new Rect(left,y,wide2,26),"Strength is what you can see of their army and cities beside your own. Trust grows by 1 every second turn while agreements stand, up to the ceiling of the strongest one (an envoy alone allows 20); each 2 trust is 1 opinion. Fear comes from conquest and war without cause; at 50 they become Threatened.");y+=32;}
  var pending=world.offers.Where(o=>o.to==0&&o.status==0&&o.expires>world.turn).ToList();int asked=world.requests.Count(r=>r.to==0&&r.status==RequestStatus.Pending);
  if(pending.Count+asked>0){var strip=new Rect(left,y,wide2,28);Fill(strip,new Color(.5f,.38f,.12f,.9f));GUI.Label(new Rect(strip.x+10,strip.y,strip.width-110,28),(pending.Count+asked)+" proposal"+(pending.Count+asked==1?"":"s")+" and request"+(pending.Count+asked==1?"":"s")+" await your answer",dipRow);if(GUI.Button(new Rect(strip.xMax-96,strip.y+2,90,24),"Review")){diplomacyDiscuss=true;detailsScroll=Vector2.zero;}y+=34;}
  if(claimsView||requestsView||customDeal){
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
  }}else{
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
    // ---- The six standing agreements: icon, name, and whether they would sign ----
    CourtHeading(new Rect(left,y,wide2,22),"Agreements");y+=26;
    for(int k=0;k<Pacts.Count;k++){
     var row=new Rect(left,y,wide2,29);Fill(row,new Color(.06f,.09f,.17f,.92f));var edge=new Color(CourtGold.r,CourtGold.g,CourtGold.b,.45f);Fill(new Rect(row.x,row.y,row.width,1),edge);Fill(new Rect(row.x,row.yMax-1,row.width,1),edge);Fill(new Rect(row.x,row.y,1,row.height),edge);Fill(new Rect(row.xMax-1,row.y,1,row.height),edge);
     var nameStyle=new GUIStyle(courtText){fontSize=16,alignment=TextAnchor.MiddleLeft};if(displayFont!=null)nameStyle.font=displayFont;var cell=new Rect(row.x+290,row.y,row.width-298,row.height);
     if(world.Pact(0,partner,k)){
      Fill(new Rect(row.x+1,row.y+1,row.width-2,row.height-2),new Color(.1f,.3f,.16f,.5f));DrawDipIcon(PactIcons[k],new Rect(row.x+8,row.y+2,25,25));GUI.Label(new Rect(row.x+44,row.y,240,row.height),Pacts.Names[k],new GUIStyle(nameStyle){normal={textColor=DipGood}});
      GUI.Label(new Rect(cell.x,cell.y,cell.width-78,cell.height),"✔ In force, "+world.PactAge(0,partner,k)+" turns",new GUIStyle(courtText){fontSize=14,alignment=TextAnchor.MiddleLeft,normal={textColor=DipGood}});
      string held=world.PactEndReason(0,partner,k);GUI.enabled=enabled&&held=="";
      if(DipButton(new Rect(row.xMax-72,row.y+3,66,row.height-6),"End",red)){if(world.EndTreaty(0,partner,Pacts.Kind[k])){dipReply=(string.IsNullOrEmpty(pn.leader)?world.NationName(partner):pn.leader)+": \"So your word lasts only this long. We will not forget.\"";dipReplyPartner=partner;}}
      GUI.enabled=enabled;Tip(row,Pacts.Names[k]+": "+Pacts.Effects[k],null,held!=""?held:"Ending it is remembered as a broken word (opinion and trust fall)."+(k==Pacts.NonAggression||k==Pacts.Defensive?" Neither side may declare war for "+Pacts.Notice+" turns afterwards.":""));
     }else{
      var o=new DiplomaticOffer{from=0,to=partner,kind=Pacts.Kind[k]};string problem=world.OfferProblem(o);int will=problem==""?world.OfferWillingness(o):-1;
      var keep=GUI.color;if(problem!="")GUI.color=new Color(1,1,1,.55f);DrawDipIcon(PactIcons[k],new Rect(row.x+8,row.y+2,25,25));GUI.Label(new Rect(row.x+44,row.y,240,row.height),Pacts.Names[k],nameStyle);GUI.color=keep;
      if(problem!=""){DrawDipIcon("lock",new Rect(cell.x,cell.y+5,19,19),new Color(.85f,.85f,.88f));FitLabel(new Rect(cell.x+26,cell.y,cell.width-26,cell.height),problem,new GUIStyle(courtText){fontSize=14,alignment=TextAnchor.MiddleLeft,normal={textColor=new Color(.75f,.75f,.78f)}},9);}
      else{var ink=will>=0?DipGood:DipPlain;GUI.Label(new Rect(cell.x,cell.y,24,cell.height),will>=0?"✓":"~",new GUIStyle(courtText){fontSize=18,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,normal={textColor=ink}});
       GUI.Label(new Rect(cell.x+28,cell.y,cell.width-28,cell.height),will>=0?"Would sign (+"+will+")":"Would refuse ("+will+")",new GUIStyle(courtText){fontSize=15,alignment=TextAnchor.MiddleLeft,normal={textColor=ink}});
       if(row.Contains(Event.current.mousePosition)&&GUI.enabled)Fill(new Rect(row.x+1,row.y+1,row.width-2,row.height-2),new Color(1,1,1,.06f));
       if(GUI.Button(row,"",GUIStyle.none)&&GUI.enabled){var sent=world.Propose(o);status=sent.answer;dipReply=world.LeaderReply(partner,sent,sent.status==1);dipReplyPartner=partner;}}
      Tip(row,Pacts.Names[k]+": "+Pacts.Effects[k]+" It lasts until one side ends it. Click to propose it.",problem==""?world.AcceptanceLines(o):null,problem);
     }
     y+=32;}
    y+=4;
   }
   // ---- Diplomatic actions ----
   CourtHeading(new Rect(left,y,wide2,22),"Diplomatic Actions");y+=28;float half=(wide2-10)/2;
   {bool has=world.Envoy(0,partner);string reason=world.EnvoyReason(0,partner);
    if(CourtAction(new Rect(left,y,half,34),"envoy",has?"Recall Envoy":"Send Envoy",has?"":reason,"An envoy at their court adds 1 to their opinion each turn up to +"+Pacts.EnvoyCap+", makes their fear of you fade twice as fast, and lets trust reach "+Pacts.EnvoyTrust+" without any agreement. You have "+world.EnvoyLimit(0)+" envoy"+(world.EnvoyLimit(0)==1?"":"s")+" (more in later ages); "+(world.EnvoyLimit(0)-world.EnvoysOut(0))+" free.",CourtGoldFace))world.SetEnvoy(0,partner,!has);
    bool rival=world.Rival(0,partner);string rr=rival?world.EndRivalReason(0,partner):world.RivalReason(0,partner);
    if(CourtAction(new Rect(left+half+10,y,half,34),"laurel",rival?"End Rivalry":"Name as Rival",rr,rival?"Stop naming them a rival; trade can resume.":"A public declaration: all trade with them stops, you gain 1 influence a turn, both opinions fall by 30, and after "+Pacts.CauseDelay+" turns you have a just cause for war. It stands for at least "+Pacts.MinTurns+" turns.",red)){if(rival)world.EndRivalry(0,partner);else if(world.DeclareRival(0,partner)){dipReply=(string.IsNullOrEmpty(pn.leader)?world.NationName(partner):pn.leader)+": \"A rival? Then we know where we stand.\"";dipReplyPartner=partner;}}
    y+=38;}
   {int mine=world.ClaimsOn(0,partner).Count,theirs=world.ClaimsOn(partner,0).Count;
    if(CourtAction(new Rect(left,y,half,34),"scales","Claims ("+mine+")","","Claim their cities with influence. A claim is a just cause for war after "+Pacts.CauseDelay+" turns, and a claimed city can be taken without alarming the world."+(theirs>0?" They claim "+theirs+" of yours.":""),navy,"",true))claimsView=true;
    bool canGift=!war&&world.gold>=25;
    if(CourtAction(new Rect(left+half+10,y,half,34),"coins","Gift 25 Gold",canGift?"":"Needs 25 gold and peace.","A gift adds 10 to their opinion of you; it fades by 1 every third turn.",navy))status=world.GiftGold(partner)?"Gift sent to "+world.NationName(partner)+".":"Gift not possible.";
    y+=38;}
   if(!war){if(CourtAction(new Rect(left,y,wide2,34),"handshake","Trade Deal",world.Rivals(0,partner)?"No dealings between rivals.":"","Exchange gold now, gold by the turn and access to resources for a set number of turns.",navy,"Gold, payments & resources",true))customDeal=true;y+=38;}
   {int open=world.requests.Count(x=>(x.from==0&&x.to==partner||x.to==0&&x.from==partner)&&(x.status==RequestStatus.Active||x.status==RequestStatus.Pending));
    if(CourtAction(new Rect(left,y,wide2,34),"scroll","Requests & Guarantees"+(open>0?"  ("+open+" open)":""),"","Ask them to stop a war, keep troops from your cities, join a war, guard your gateway or send aid: with a deadline, a payment on completion and, if you like, a threat. See every commitment between you.",navy,"",true)){requestsView=true;customDeal=false;claimsView=false;}y+=38;}
   // Their leader's agenda and how it judges you.
   {string agenda=world.AgendaReason(partner,0);if(agenda!=""&&y<side.yMax-92){var s=new GUIStyle(courtItalic){fontSize=14,alignment=TextAnchor.MiddleCenter};GUI.Label(new Rect(left,y,wide2,side.yMax-70-y),agenda,s);}}
  }
  float bw=(wide2-20)/3,by=side.yMax-62;
  if(CourtButton(new Rect(left,by,bw,42),"book","More",navy)){diplomacyDiscuss=true;detailsScroll=Vector2.zero;}
  if(CourtButton(new Rect(left+bw+10,by,bw,42),"gateway","Leave",navy)){diplomacyChooser=true;diplomacyDiscuss=false;claimsView=false;customDeal=false;requestsView=false;}
  {string warReason=world.WarReason(0,partner);string cause=world.CasusBelli(0,partner);var wr=new Rect(left+2*bw+20,by,bw,42);
   if(CourtAction(wr,"swords","Declare War",warReason,warReason!=""?"War is not possible now.":cause!=""?"You have a just cause: "+cause+". Only they will hold it against you.":"You have no just cause. "+(world.CausePending(0,partner)!=""?world.CausePending(0,partner)+" ":"Name them a rival or claim a city, then wait "+Pacts.CauseDelay+" turns. ")+"War without cause makes every civilisation that knows you fear and dislike you.",red)){pendingUnit=-2;pendingWars=new[]{partner};detailsScreen="";cityScreen=false;GUIUtility.ExitGUI();}}
  DrawTip(side);
  DrawOpenDropdown();
 }
}
}
