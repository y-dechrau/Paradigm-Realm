using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The Beliefs & Traditions screens (owner briefs, 23 Nov 2026; rules in Lifeways.cs): the decision prompt for a choice put
// to the ruler, and the Empire screen with the four traditions (adherence, conviction, commitment and state policy each shown
// apart), the practices cities keep, public vows, the player's cities and the chronicle.
public partial class RealmGame {
 bool faithsView;int waysCity=-1,waysPeople=-1,waysTurns=1,waysPick;Vector2 waysCityScroll,waysChronicleScroll,waysPracticeScroll;
 static readonly Color[] WayInk={new Color(.3f,.62f,.85f),new Color(.78f,.55f,.3f),new Color(.86f,.74f,.32f),new Color(.62f,.5f,.86f)};
 static readonly int[] VowLengths={15,20,30,40};
 bool WayPromptOpen(){return world!=null&&world.lifeways&&!turnRunning&&world.result==""&&!guide&&!confirmNew&&pendingUnit==-1&&!cityScreen&&detailsScreen==""&&!researchScreen&&world.NextWayChoice()!=null;}
 void DrawWayPrompt(){
  var e=world.NextWayChoice();if(e==null)return;GUI.enabled=true;var choices=world.WayOptions(e);string text=world.WayChoiceText(e);
  var bodyStyle=new GUIStyle(body){wordWrap=true};var optionStyle=new GUIStyle(button){wordWrap=true,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(12,12,4,4)};
  float w=680,textH=bodyStyle.CalcHeight(new GUIContent(text),w-40);var heights=new float[choices.Length];float total=0;
  for(int k=0;k<choices.Length;k++){string why=world.WayOptionProblem(e,k);heights[k]=Mathf.Max(38,optionStyle.CalcHeight(new GUIContent(choices[k]+(why!=""?"  (unavailable: "+why+")":"")),w-40)+8);total+=heights[k]+8;}
  float h=64+textH+16+total+8;var box=new Rect(640-w/2,Mathf.Max(44,400-h/2),w,h);Fill(FullCanvas,new Color(0,0,0,.4f));FaithFrame(box,.97f);
  GUI.Label(new Rect(box.x+20,box.y+14,box.width-40,32),Lifeways.ChoiceNames[e.kind].ToUpperInvariant(),hudTitle);
  GUI.Label(new Rect(box.x+20,box.y+54,box.width-40,textH),text,bodyStyle);float y=box.y+64+textH+8;
  for(int k=0;k<choices.Length;k++){string why=world.WayOptionProblem(e,k);bool was=GUI.enabled;GUI.enabled=why=="";
   if(GUI.Button(new Rect(box.x+20,y,box.width-40,heights[k]),choices[k]+(why!=""?"  (unavailable: "+why+")":""),optionStyle)){world.WayResolve(e,k);status=world.wayChronicle.Count>0?world.wayChronicle[0]:"";GUIUtility.ExitGUI();}
   GUI.enabled=was;y+=heights[k]+8;}
 }
 // A label that shrinks until its wrapped text fits the box (all text visible).
 void WayLabel(Rect r,string text,GUIStyle baseStyle,int size,int least=10){var s=new GUIStyle(baseStyle){fontSize=size,wordWrap=true,alignment=TextAnchor.UpperLeft,clipping=TextClipping.Overflow};while(s.fontSize>least&&s.CalcHeight(new GUIContent(text),r.width)>r.height)s.fontSize--;GUI.Label(r,text,s);}
 bool WayButton(Rect r,string text,bool on=false,string problem=""){bool was=GUI.enabled;GUI.enabled=was&&problem=="";bool hit=CourtButton(r,"","",on?CourtGoldFace:CourtNavy);
  var s=new GUIStyle(courtText){fontSize=14,alignment=TextAnchor.MiddleCenter,wordWrap=false,clipping=TextClipping.Overflow};while(s.fontSize>9&&s.CalcSize(new GUIContent(text)).x>r.width-8)s.fontSize--;var keep=GUI.color;if(!GUI.enabled)GUI.color=new Color(1,1,1,.45f);GUI.Label(r,text,s);GUI.color=keep;
  GUI.enabled=was;if(problem!=""&&r.Contains(Event.current.mousePosition))waysHover=problem;return hit;}
 string waysHover;
 void DrawTraditionsScreen(){
  DiplomacyStyles();CourtStyles();waysHover=null;
  if(CourtChrome("Beliefs & Traditions","How your people live")){detailsScreen="";GUIUtility.ExitGUI();}
  var mine=world.cities.Where(c=>c.owner==0).ToList();var nw=world.NationWayOf(0);if(mine.Count>0&&!mine.Any(c=>c.id==waysCity))waysCity=mine[0].id;
  var known=Enumerable.Range(1,world.civCount-1).Where(o=>world.met[o]&&!world.IsNativeNation(o)&&world.cities.Any(c=>c.owner==o)).ToList();if(known.Count>0&&!known.Contains(waysPeople))waysPeople=known[0];
  var chosen=mine.FirstOrDefault(c=>c.id==waysCity);
  // ---- The four traditions ----
  var left=new Rect(8,72,624,672);GoldFrame(left);float cardH=(left.height-28-3*8)/4;
  for(int t=0;t<Lifeways.Count;t++){var card=new Rect(left.x+14,left.y+14+t*(cardH+8),left.width-28,cardH);bool pick=waysPick==t;Fill(card,new Color(.05f,.07f,.14f,.95f));Fill(new Rect(card.x,card.y,4,card.height),WayInk[t]);if(pick)Fill(new Rect(card.x,card.y,card.width,2),CourtGold);
   var followers=world.Followers(0,t).ToList();int conviction=followers.Count>0?(int)followers.Average(c=>world.WayOf(c).conviction):0;int adherence=mine.Count>0?(int)mine.Average(c=>world.WayOf(c).adherence[t]):0;
   float x=card.x+14,w=card.width-28;
   GUI.Label(new Rect(x,card.y+4,w*.62f,24),Lifeways.Names[t].ToUpperInvariant(),new GUIStyle(courtCaps){fontSize=17,alignment=TextAnchor.MiddleLeft,wordWrap=false,normal={textColor=CourtCream}});
   GUI.Label(new Rect(x+w*.62f,card.y+4,w*.38f,24),Lifeways.Kinds[t],new GUIStyle(courtItalic){fontSize=13,alignment=TextAnchor.MiddleRight,wordWrap=false});
   WayLabel(new Rect(x,card.y+27,w,30),Lifeways.Texts[t],courtItalic,12,9);
   // The four measures, each on its own.
   string measures="Adherence: "+followers.Count+" of "+mine.Count+" cities ("+adherence+"% of people)   ·   Conviction: "+(followers.Count>0?conviction.ToString():"none yet")+"   ·   Institutions: "+world.practices.Count(p=>p.owner==0&&p.status==0&&p.tradition==t)+" practices";
   WayLabel(new Rect(x,card.y+59,w,16),measures,courtText,12,9);
   WayLabel(new Rect(x,card.y+76,w,16),Lifeways.Roots[t]+"  Gift: "+Lifeways.Gifts[t],courtText,12,9);
   int held=nw.commitment[t];string cp=world.CommitmentProblem(0,t);
   WayLabel(new Rect(x,card.y+93,w-150,cardH-93-34),"Commitment: "+Lifeways.CommitmentNames[t]+(held==1?" (held)":held==2?" (broken)":" (not taken)")+". "+Lifeways.CommitmentTexts[t],courtText,12,9);
   if(held!=1&&WayButton(new Rect(card.xMax-150,card.y+94,138,26),held==2?"Renew ("+Lifeways.RenewCost+" gold)":"Take it up",false,cp)){status=world.TakeCommitment(0,t);if(status=="")status="Commitment taken up: "+Lifeways.CommitmentNames[t]+".";}
   // State policy.
   float py=card.yMax-30;GUI.Label(new Rect(x,py,92,26),"State policy:",new GUIStyle(courtText){fontSize=13,alignment=TextAnchor.MiddleLeft,wordWrap=false});
   for(int k=0;k<3;k++)if(WayButton(new Rect(x+96+k*106,py,100,26),Lifeways.PolicyNames[k]+(k==1?" (paid)":""),nw.policy[t]==k)){world.SetWayPolicy(0,t,k);}
   if(WayButton(new Rect(card.xMax-150,py,138,26),pick?"Selected":"Select",pick))waysPick=t;}
  // ---- Practices and vows ----
  var a=new Rect(640,72,632,250);GoldFrame(a);{float x=a.x+28,w=a.width-56,y=a.y+14;CourtHeading(new Rect(x,y,w,22),"Practices your cities keep");y+=26;
   var kept=world.practices.Where(p=>p.owner==0&&p.status==0).ToList();
   if(kept.Count==0){WayLabel(new Rect(x,y,w,36),"None yet. A practice begins when a city is helped through a shortage and chooses how to remember it.",courtItalic,13);y+=38;}
   else{var area=new Rect(x,y,w,60);waysPracticeScroll=GUI.BeginScrollView(area,waysPracticeScroll,new Rect(0,0,w-18,Math.Max(60,kept.Count*30)));
    for(int k=0;k<kept.Count;k++){var p=kept[k];var c=world.cities.Find(z=>z.id==p.city);var row=new Rect(0,k*30,w-20,28);Fill(row,new Color(.06f,.09f,.17f,.9f));
     WayLabel(new Rect(row.x+6,row.y+5,row.width-230,20),p.name+", "+(c==null?"":c.name)+" ("+Lifeways.PracticeKinds[p.kind].ToLowerInvariant()+"): "+world.PracticeCostText(p)+", due in "+Math.Max(0,p.nextDue-world.turn)+" turns; kept "+p.kept+", missed "+p.missed,courtText,12,9);
     if(WayButton(new Rect(row.xMax-222,row.y+1,108,26),"Pay in coin",false,world.ReformProblem(p.id,1))){status=world.ReformPractice(p.id,1);}
     if(WayButton(new Rect(row.xMax-110,row.y+1,108,26),"Keep it rarer",false,world.ReformProblem(p.id,2))){status=world.ReformPractice(p.id,2);}}
    GUI.EndScrollView();y+=64;}
   CourtHeading(new Rect(x,y,w,22),"Public vows");y+=26;var vow=world.vows.FirstOrDefault(v=>v.owner==0&&v.status==0&&v.kind!=3);var first=world.vows.FirstOrDefault(v=>v.owner==0&&v.status==0&&v.kind==3);
   if(first!=null){WayLabel(new Rect(x,y,w,20),"Your first vow: "+first.text+" ("+Math.Max(0,first.due-world.turn)+" turns left)",courtText,13,10);y+=22;}
   if(vow!=null){WayLabel(new Rect(x,y,w-150,40),"In force: "+vow.text+" Keeping it is worth "+world.VowWorth(vow)+"% of a first vow. ("+Math.Max(0,vow.due-world.turn)+" turns left)",courtText,13,10);
    if(WayButton(new Rect(a.xMax-166,y+4,138,28),"Ask release ("+Lifeways.ReleaseCost+"g)",false,world.ReleaseProblem(vow.id))){status=world.ReleaseVow(vow.id);if(status=="")status="Released from the vow.";}}
   else{int turns=VowLengths[waysTurns];
    if(WayButton(new Rect(x,y,150,26),"For "+turns+" turns ▸"))waysTurns=(waysTurns+1)%VowLengths.Length;
    if(known.Count>0&&WayButton(new Rect(x+156,y,w-156,26),"People: "+world.NationName(waysPeople)+" ▸"))waysPeople=known[(known.IndexOf(waysPeople)+1)%known.Count];y+=30;
    float bw=(w-16)/3;int city=chosen==null?-1:chosen.id;string cname=chosen==null?"a city":chosen.name;
    if(WayButton(new Rect(x,y,bw,28),"None shall starve in "+cname,false,world.VowProblem(0,0,city,-1,turns))){status=world.DeclareVow(0,0,city,-1,turns);}
    if(WayButton(new Rect(x+bw+8,y,bw,28),known.Count>0?"No war on "+world.NationName(waysPeople):"No war (no people known)",false,world.VowProblem(0,1,-1,waysPeople,turns))){status=world.DeclareVow(0,1,-1,waysPeople,turns);}
    if(WayButton(new Rect(x+2*(bw+8),y,bw,28),"Keep the practice of "+cname,false,world.VowProblem(0,2,city,-1,turns))){status=world.DeclareVow(0,2,city,-1,turns);}
    y+=32;WayLabel(new Rect(x,y,w,a.yMax-8-y),"A vow is public and binding. Kept: conviction, trust and influence. Broken: every ruler remembers. Release costs "+Lifeways.ReleaseCost+" gold. If it becomes impossible through no act of yours, no one is blamed. The same vow sworn again is worth half.",courtItalic,12,9);}}
  // ---- Your cities ----
  var b=new Rect(640,328,632,212);GoldFrame(b);{float x=b.x+28,w=b.width-56,y=b.y+14;CourtHeading(new Rect(x,y,w,22),"Your cities");y+=26;
   var area=new Rect(x,y,w,b.yMax-44-y);waysCityScroll=GUI.BeginScrollView(area,waysCityScroll,new Rect(0,0,w-18,Math.Max(area.height,mine.Count*28)));
   for(int k=0;k<mine.Count;k++){var c=mine[k];var l=world.WayOf(c);var row=new Rect(0,k*28,w-20,26);bool pick=c.id==waysCity;Fill(row,pick?new Color(CourtGold.r,CourtGold.g,CourtGold.b,.25f):new Color(.06f,.09f,.17f,.9f));if(l.tradition>=0)Fill(new Rect(row.x,row.y,4,row.height),WayInk[l.tradition]);
    string line=c.name+":  "+(l.tradition>=0?Lifeways.Names[l.tradition]+" "+l.adherence[l.tradition]+"%"+(l.split?" (its own house)":""):"no settled tradition")+"  ·  conviction "+l.conviction+(l.cause!=""?"  ·  "+l.cause:"");
    WayLabel(new Rect(row.x+10,row.y+4,row.width-14,20),line,courtText,13,9);if(GUI.Button(row,"",GUIStyle.none))waysCity=c.id;}
   GUI.EndScrollView();
   if(chosen!=null){string hp=world.HouseProblem(0,chosen.id,waysPick);if(WayButton(new Rect(x,b.yMax-38,w,28),"Found a house of the "+Lifeways.Names[waysPick]+" in "+chosen.name+" ("+Lifeways.HouseCost+" gold)",false,hp)){status=world.FoundHouse(0,chosen.id,waysPick);if(status=="")status="A house was founded in "+chosen.name+".";}}}
  // ---- The chronicle ----
  var d=new Rect(640,546,632,198);GoldFrame(d);{float x=d.x+28,w=d.width-56,y=d.y+14;CourtHeading(new Rect(x,y,w,22),"Chronicle of beliefs");y+=26;var text=new GUIStyle(courtText){fontSize=13,wordWrap=true,alignment=TextAnchor.UpperLeft};
   var lines=world.wayChronicle.Take(40).ToList();var area=new Rect(x,y,w,d.yMax-12-y);
   if(lines.Count==0)WayLabel(area,"Nothing has been written yet.",courtItalic,13);
   else{var hs=lines.Select(s=>text.CalcHeight(new GUIContent(s),w-24)+4).ToList();waysChronicleScroll=GUI.BeginScrollView(area,waysChronicleScroll,new Rect(0,0,w-18,Math.Max(area.height,hs.Sum())));float yy=0;for(int k=0;k<lines.Count;k++){GUI.Label(new Rect(2,yy,w-24,hs[k]),lines[k],text);yy+=hs[k];}GUI.EndScrollView();}}
  CourtFooter(new Rect(4,746,1272,52));
  GUI.Label(new Rect(24,758,700,34),"Faith remains the standing of the organised clergy. Conviction cannot be bought.",new GUIStyle(courtItalic){fontSize=14,alignment=TextAnchor.MiddleLeft,wordWrap=false});
  if(CourtButton(new Rect(770,758,240,34),"","Organised Faiths",CourtNavy)){faithsView=true;detailsScroll=Vector2.zero;GUIUtility.ExitGUI();}
  if(CourtButton(new Rect(1030,758,226,34),"","Return to Map",CourtNavy)){detailsScreen="";GUIUtility.ExitGUI();}
  if(waysHover!=null){var hs=new GUIStyle(courtText){fontSize=14,wordWrap=true,alignment=TextAnchor.UpperLeft};var mp=Event.current.mousePosition;float hw=320,hh=hs.CalcHeight(new GUIContent(waysHover),hw-20)+16;var hb=new Rect(Mathf.Clamp(mp.x+14,8,1272-hw),Mathf.Clamp(mp.y+18,8,740-hh),hw,hh);Fill(hb,new Color(.02f,.03f,.08f,.97f));Fill(new Rect(hb.x,hb.y,hb.width,1),CourtGold);Fill(new Rect(hb.x,hb.yMax-1,hb.width,1),CourtGold);GUI.Label(new Rect(hb.x+10,hb.y+8,hw-20,hh-16),waysHover,hs);}
 }
}
}
