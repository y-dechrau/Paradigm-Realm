using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The negotiation screens (owner brief, 23 Nov 2026). In the private audience (the Imperial Court) the deal table is headed
// "We offer" and "We ask": gold now, gold a turn, a resource, passage through the giver's land, and use of one known gateway.
// The whole agreement is written out in plain English before it is proposed, with the ruler's answer in words. A ruler's
// counteroffer is laid on the table with the changed terms marked; the player may accept it, amend it (it is loaded into the
// table) or decline. In the Council three more views sit beside the seats: a compact contact list, the agreements in force
// with their obligations, time left and actions, and the diplomatic history.
public partial class RealmGame {
 bool dealGivePassage,dealAskPassage;int dealGiveGate,dealAskGate;
 int councilView,councilPage;Vector2 councilListScroll;string councilHover;string councilConfirm;Action councilConfirmDo;bool historyAll=true;
 static readonly Color DealChanged=new Color(1f,.84f,.4f);
 void ResetDealTable(){giveGold=askGold=givePerTurn=askPerTurn="0";giveResource=askResource=0;dealGivePassage=dealAskPassage=false;dealGiveGate=dealAskGate=0;}
 // The deal table in the audience. Returns the y below it.
 float DrawDealTable(int partner,float left,float y,float wide2,bool war,bool enabled,Color navy,Color teal,Rect side){
  // A counteroffer from this ruler lies on the table first.
  var counter=war?null:world.offers.Find(o=>o.from==partner&&o.to==0&&o.status==0&&o.counterOf>0&&o.expires>world.turn);
  if(counter!=null)y=DrawCounterOffer(partner,counter,left,y,wide2,enabled,navy,teal);
  var mine=war?new List<Tile>():world.KnownGatesOf(0,partner);var theirs=war?new List<Tile>():world.KnownGatesOf(partner,0);
  float colW=(wide2-12)/2,colH=war?150:182+(mine.Count+theirs.Count>0?34:0);
  for(int s=0;s<2;s++){
   var col=new Rect(left+s*(colW+12),y,colW,colH);DipPanel(col,.9f);GUI.Label(new Rect(col.x,col.y+4,col.width,20),s==0?"WE OFFER":"WE ASK",dipCaps);
   DrawIcon(PixelArt.IconGold,new Rect(col.x+8,col.y+32,22,22));GUI.Label(new Rect(col.x+32,col.y+30,60,24),"Gold",dipRow);
   if(s==0)giveGold=Stepper(new Rect(col.x+86,col.y+30,col.width-94,24),giveGold,25,10000);else askGold=Stepper(new Rect(col.x+86,col.y+30,col.width-94,24),askGold,25,10000);
   GUI.Label(new Rect(col.x+10,col.y+60,80,24),"Per turn",dipRow);
   if(s==0)givePerTurn=Stepper(new Rect(col.x+86,col.y+60,col.width-94,24),givePerTurn,1,20);else askPerTurn=Stepper(new Rect(col.x+86,col.y+60,col.width-94,24),askPerTurn,1,20);
   GUI.Label(new Rect(col.x+10,col.y+90,col.width-20,20),"Resource",hudSmall);
   if(s==0)giveResource=Dropdown(new Rect(col.x+8,col.y+112,col.width-16,26),"give",giveResource,World.ResourceNames);else askResource=Dropdown(new Rect(col.x+8,col.y+112,col.width-16,26),"ask",askResource,World.ResourceNames);
   if(war)continue;
   // Passage: a clause added or removed with one click.
   {bool on=s==0?dealGivePassage:dealAskPassage;var pr=new Rect(col.x+8,col.y+146,col.width-16,28);int host=s==0?0:partner,guest=s==0?partner:0;bool open=world.HasAccess(guest,host);
    bool was=GUI.enabled;GUI.enabled=was&&!open;
    if(FitDipButton(pr,(on?"✓ ":"+ ")+(s==0?"Passage through our land":"Passage through their land"),on?teal:navy)){if(s==0)dealGivePassage=!on;else dealAskPassage=!on;}
    GUI.enabled=was;if(pr.Contains(Event.current.mousePosition))Tip(pr,open?"Open borders already allow it.":(s==0?world.NationName(partner)+"'s units may cross all your land":"Your units may cross all "+world.NationName(partner)+"'s land")+" for the length of the deal. Units still inside when it ends are escorted to the border.");}
   var gates=s==0?mine:theirs;
   if(gates.Count>0){var names=new[]{"No gateway"}.Concat(gates.Select(g=>"Use of "+world.GateLabel(g.y*world.Width+g.x+1))).ToArray();int pick=Mathf.Clamp(s==0?dealGiveGate:dealAskGate,0,gates.Count);
    pick=Dropdown(new Rect(col.x+8,col.y+180,col.width-16,26),s==0?"givegate":"askgate",pick,names);if(s==0)dealGiveGate=pick;else dealAskGate=pick;}
  }
  y+=colH+8;
  var offer=new DiplomaticOffer{from=0,to=partner,kind=war?TreatyKind.Peace:TreatyKind.Trade,duration=new[]{10,15,30}[diplomacyDuration],giveGold=DealNumber(giveGold,10000),askGold=DealNumber(askGold,10000),givePerTurn=DealNumber(givePerTurn,20),askPerTurn=DealNumber(askPerTurn,20),giveResource=giveResource,askResource=askResource};
  if(!war){offer.givePassage=dealGivePassage;offer.askPassage=dealAskPassage;if(dealGiveGate>0&&dealGiveGate<=mine.Count&&!dealGivePassage){var g=mine[dealGiveGate-1];offer.giveGate=g.y*world.Width+g.x+1;}if(dealAskGate>0&&dealAskGate<=theirs.Count&&!dealAskPassage){var g=theirs[dealAskGate-1];offer.askGate=g.y*world.Width+g.x+1;}}
  if(war)y=DrawPeaceTerms(partner,side,y,offer);
  string problem=world.OfferProblem(offer);int will=problem==""?world.OfferWillingness(offer):-1;
  // The agreement in plain English, then the ruler's likely answer in words.
  if(!war&&problem==""){var sum=new GUIStyle(dipRow){fontSize=13,wordWrap=true};string text=world.DealSummary(offer);float h=sum.CalcHeight(new GUIContent(text),wide2-16)+10;var box=new Rect(left,y,wide2,h);Fill(box,new Color(.04f,.06f,.12f,.9f));Fill(new Rect(box.x,box.y,2,box.height),CourtGold);GUI.Label(new Rect(box.x+10,box.y+5,box.width-16,h-10),text,sum);y+=h+6;}
  {string verdictText=problem!=""?problem:war?world.OfferAssessment(offer):(will>=0?"They would accept. ":"They would refuse. ")+world.AnswerReasons(offer);var vs=new GUIStyle(dipRow){fontSize=13,wordWrap=true};float vh=Mathf.Max(22,vs.CalcHeight(new GUIContent(verdictText),wide2));
   var keep=GUI.color;GUI.color=problem!=""?new Color(.75f,.75f,.78f):will>=0?DipGood:DipBad;GUI.Label(new Rect(left,y,wide2,vh),verdictText,vs);GUI.color=keep;y+=vh+6;}
  float sendW=(wide2-8)*.58f;var send=new Rect(left,y,sendW,40);GUI.enabled=enabled&&problem==""&&(war||world.TalksLeft(partner)>0)&&counter==null;
  if(DipButton(send,war?"OFFER PEACE":"PROPOSE",teal)){var sent=world.Propose(offer);status=sent.status==1?"Agreed: "+world.TermsText(sent)+".":sent.answer;dipReply=world.LeaderReply(partner,sent,sent.status==1);dipReplyPartner=partner;
   if(sent.status==1){customDeal=false;ResetDealTable();ResetPeaceTerms();}
   else if(world.offers.Any(o=>o.from==partner&&o.to==0&&o.status==0&&o.counterOf==sent.id)){status=world.NationName(partner)+" answers with a counteroffer. It is on the table.";courtScroll=Vector2.zero;}}
  if(DipButton(new Rect(left+sendW+8,y,wide2-sendW-8,40),"What would it take?",navy)){int need=world.GoldToAccept(offer);if(need>0){giveGold=(DealNumber(giveGold,10000)+need).ToString();status="They would accept with "+need+" more gold at signing.";}else status=need==0?"They would already accept.":"No gold you hold would change their mind. "+world.AnswerReasons(offer);}
  GUI.enabled=enabled;if(problem=="")Tip(send,"What they weigh:",world.AcceptanceLines(offer));
  y+=44;
  if(!war){string note=counter!=null?"Answer the counteroffer above first: accept it, amend it or decline it.":world.TalksLeft(partner)>0?world.TalksLeft(partner)+" more proposal"+(world.TalksLeft(partner)==1?"":"s")+" to "+world.NationName(partner)+" this turn.":world.NationName(partner)+" will hear no more proposals this turn.";
   GUI.Label(new Rect(left,y,wide2,20),note,new GUIStyle(hudSmall){fontSize=12,wordWrap=false});y+=22;}
  return y;
 }
 // A DipButton whose label shrinks to fit (all text visible).
 bool FitDipButton(Rect r,string text,Color face){bool hit=DipButton(r,"",face);var st=new GUIStyle(dipRow){alignment=TextAnchor.MiddleCenter,wordWrap=false,clipping=TextClipping.Overflow,fontSize=14};while(st.fontSize>10&&st.CalcSize(new GUIContent(text)).x>r.width-10)st.fontSize--;var keep=GUI.color;if(!GUI.enabled)GUI.color=new Color(1,1,1,.5f);GUI.Label(r,text,st);GUI.color=keep;return hit;}
 float DrawCounterOffer(int partner,DiplomaticOffer c,float left,float y,float wide2,bool enabled,Color navy,Color teal){
  var orig=world.offers.Find(o=>o.id==c.counterOf);var row=new GUIStyle(dipRow){fontSize=13,wordWrap=true};var lines=new List<KeyValuePair<string,bool>>();
  lines.Add(new KeyValuePair<string,bool>(world.NationName(partner)+" would agree to this instead"+(c.expires>world.turn?" (answer within "+(c.expires-world.turn)+" turn"+(c.expires-world.turn==1?"":"s")+"):":":"),false));
  bool goldChanged=(c.changed&1)!=0,durChanged=(c.changed&4)!=0;
  foreach(var g in world.GivesList(c,true))lines.Add(new KeyValuePair<string,bool>("They give: "+g+(durChanged&&g.Contains(" turns")?"  (changed)":""),durChanged&&g.Contains(" turns")));
  foreach(var g in world.GivesList(c,false)){bool ch=goldChanged&&g.Contains("gold at signing")||durChanged&&g.Contains(" turns");string was=ch&&goldChanged&&g.Contains("gold at signing")&&orig!=null?"  (changed: you offered "+orig.giveGold+")":ch?"  (changed)":"";lines.Add(new KeyValuePair<string,bool>("You give: "+g+was,ch));}
  if(goldChanged&&orig!=null&&orig.askGold!=c.giveGold)lines.Add(new KeyValuePair<string,bool>("They give "+c.giveGold+" gold at signing  (changed: you asked "+orig.askGold+")",true));
  if(durChanged&&orig!=null)lines.Add(new KeyValuePair<string,bool>("For "+c.duration+" turns  (changed: you asked "+orig.duration+")",true));
  float h=30;foreach(var l in lines)h+=row.CalcHeight(new GUIContent(l.Key),wide2-24)+2;h+=44;
  var box=new Rect(left,y,wide2,h);Fill(box,new Color(.1f,.08f,.03f,.92f));Fill(new Rect(box.x,box.y,box.width,2),DealChanged);Fill(new Rect(box.x,box.yMax-2,box.width,2),DealChanged);
  GUI.Label(new Rect(box.x+12,box.y+6,box.width-24,20),"COUNTEROFFER",dipCaps);float yy=box.y+28;
  foreach(var l in lines){float lh=row.CalcHeight(new GUIContent(l.Key),wide2-24);var keep=GUI.color;if(l.Value)GUI.color=DealChanged;GUI.Label(new Rect(box.x+12,yy,wide2-24,lh),l.Key,row);GUI.color=keep;yy+=lh+2;}
  float bw=(wide2-24-16)/3;var br=new Rect(box.x+12,yy+6,bw,30);GUI.enabled=enabled;
  if(DipButton(br,"Accept",teal)){bool ok=world.RespondToOffer(c.id,true);status=ok?"Agreed: "+world.TermsText(c)+".":"It could not be agreed: "+c.answer;if(ok){customDeal=false;ResetDealTable();dipReply=world.LeaderReply(partner,c,true);dipReplyPartner=partner;}GUIUtility.ExitGUI();}
  if(DipButton(new Rect(br.x+bw+8,br.y,bw,30),"Amend",navy)){
   // Their terms are loaded into the table as your own next proposal.
   giveGold=c.askGold.ToString();askGold=c.giveGold.ToString();givePerTurn=c.askPerTurn.ToString();askPerTurn=c.givePerTurn.ToString();giveResource=c.askResource;askResource=c.giveResource;dealGivePassage=c.askPassage;dealAskPassage=c.givePassage;
   var mine=world.KnownGatesOf(0,partner);var theirs=world.KnownGatesOf(partner,0);dealGiveGate=c.askGate>0?mine.FindIndex(t=>t.y*world.Width+t.x+1==c.askGate)+1:0;dealAskGate=c.giveGate>0?theirs.FindIndex(t=>t.y*world.Width+t.x+1==c.giveGate)+1:0;
   diplomacyDuration=c.duration<=10?0:c.duration<=15?1:2;world.RespondToOffer(c.id,false,true);status="Their terms are on the table. Change what you wish and propose again.";GUIUtility.ExitGUI();}
  if(DipButton(new Rect(br.x+2*(bw+8),br.y,bw,30),"Decline",navy)){world.RespondToOffer(c.id,false);status="Counteroffer declined ("+World.DeclineCost(c)+" opinion).";GUIUtility.ExitGUI();}
  return box.yMax+10;
 }
 // ---------- the Council's other views ----------
 // What needs attention at a seat: each marker has a symbol and a sentence (shown when the pointer is on it).
 List<KeyValuePair<string,string>> SeatMarkers(int o){
  var list=new List<KeyValuePair<string,string>>();
  int waiting=world.offers.Count(x=>x.to==0&&x.from==o&&x.status==0&&x.expires>world.turn);
  if(waiting>0){var first=world.offers.First(x=>x.to==0&&x.from==o&&x.status==0&&x.expires>world.turn);list.Add(new KeyValuePair<string,string>(waiting.ToString(),world.NationName(o)+" has put "+(waiting==1?"a proposal":waiting+" proposals")+" to you"+(first.why!=""?": \""+first.why+"\"":"")+". Answer within "+(first.expires-world.turn)+" turn"+(first.expires-world.turn==1?"":"s")+"."));}
  int asked=world.requests.Count(r=>r.to==0&&r.from==o&&r.status==RequestStatus.Pending);if(asked>0)list.Add(new KeyValuePair<string,string>("?",world.NationName(o)+" has asked something of you. Open the audience to answer."));
  DiplomaticAgreement soon=null;foreach(var g in world.AgreementsBetween(0,o))if(world.AgreementActive(g)&&g.end-world.turn<=DiplomacyTuning.WarnTurns&&(soon==null||g.end<soon.end))soon=g;
  var deal=world.contracts.FirstOrDefault(c=>(c.from==0&&c.to==o||c.from==o&&c.to==0)&&c.end-world.turn<=DiplomacyTuning.WarnTurns);
  if(soon!=null)list.Add(new KeyValuePair<string,string>("⌛",(soon.gate>0?"Gate transit":"Passage")+" with "+world.NationName(o)+" ends in "+(soon.end-world.turn)+" turn"+(soon.end-world.turn==1?"":"s")+"."));
  else if(deal!=null)list.Add(new KeyValuePair<string,string>("⌛","Your trade deal with "+world.NationName(o)+" ends in "+Math.Max(0,deal.end-world.turn)+" turns."));
  int broken=world.OpinionMod(o,0,OpinionMods.Broken),wrong=world.OpinionMod(o,0,OpinionMods.Grievance);
  if(broken<0)list.Add(new KeyValuePair<string,string>("!",world.NationName(o)+" remembers a promise you broke."));else if(wrong<=-10)list.Add(new KeyValuePair<string,string>("!",world.NationName(o)+" holds a serious grievance against you."));
  return list;
 }
 void DrawSeatMarkers(Rect fig,int o){
  var marks=SeatMarkers(o);float x=fig.xMax-26;
  foreach(var m in marks){var b=new Rect(x,fig.y+6,22,22);bool hourglass=m.Key=="⌛";Fill(b,hourglass?new Color(.72f,.5f,.1f):m.Key=="!"?new Color(.45f,.08f,.3f):new Color(.75f,.18f,.14f));Fill(new Rect(b.x,b.y,b.width,1),CourtGold);
   if(hourglass)DrawIcon(PixelArt.IconTurn,new Rect(b.x+3,b.y+3,16,16));else GUI.Label(b,m.Key,dipBadge);
   if(b.Contains(Event.current.mousePosition))councilHover=m.Value;x-=25;}
 }
 void AskCouncil(string text,Action yes){councilConfirm=text;councilConfirmDo=yes;}
 void DrawCouncilConfirm(){
  if(councilConfirm==null)return;Fill(FullCanvas,new Color(0,0,0,.55f));var box=new Rect(390,290,500,230);GoldFrame(box);Fill(new Rect(box.x+10,box.y+10,box.width-20,box.height-20),new Color(.04f,.06f,.13f,.97f));
  GUI.Label(new Rect(box.x+30,box.y+26,box.width-60,130),councilConfirm,new GUIStyle(courtText){fontSize=15,wordWrap=true,alignment=TextAnchor.UpperLeft});
  if(CourtButton(new Rect(box.x+30,box.yMax-62,210,34),"","Confirm",CourtGoldFace)){var act=councilConfirmDo;councilConfirm=null;councilConfirmDo=null;if(act!=null)act();GUIUtility.ExitGUI();}
  if(CourtButton(new Rect(box.xMax-240,box.yMax-62,210,34),"","Keep as it is",CourtNavy)){councilConfirm=null;councilConfirmDo=null;GUIUtility.ExitGUI();}
 }
 // The compact contact list: every known leader in one scrolling column, for quick choice and for many contacts.
 void DrawContactList(Rect area,int[] shown){
  Fill(area,new Color(.03f,.05f,.1f,.9f));const float Row=40;
  councilListScroll=GUI.BeginScrollView(area,councilListScroll,new Rect(0,0,area.width-18,Math.Max(area.height,shown.Length*Row)));var m=Event.current.mousePosition;
  for(int k=0;k<shown.Length;k++){int o=shown[k];var r=new Rect(0,k*Row,area.width-20,Row-3);bool pick=o==councilPick;Fill(r,pick?new Color(CourtGold.r,CourtGold.g,CourtGold.b,.25f):r.Contains(m)?new Color(1,1,1,.07f):new Color(.06f,.09f,.17f,.9f));
   var flag=FlagTexture(o);if(flag!=null)GUI.DrawTexture(new Rect(r.x+6,r.y+6,38,25),flag,ScaleMode.StretchToFill,true);
   GUI.Label(new Rect(r.x+52,r.y,r.width*.42f,r.height),world.NationName(o),new GUIStyle(courtText){fontSize=15,alignment=TextAnchor.MiddleLeft,wordWrap=false,clipping=TextClipping.Clip});
   GUI.Label(new Rect(r.x+52+r.width*.42f,r.y,r.width*.3f,r.height),world.nations[o].leader??"",new GUIStyle(courtItalic){fontSize=14,alignment=TextAnchor.MiddleLeft,wordWrap=false,clipping=TextClipping.Clip});
   MoodBadge(new Rect(r.xMax-190,r.y+6,120,r.height-12),o,12);
   var marks=SeatMarkers(o);float x=r.xMax-28;foreach(var mk in marks){var b=new Rect(x,r.y+8,20,20);Fill(b,mk.Key=="⌛"?new Color(.72f,.5f,.1f):mk.Key=="!"?new Color(.45f,.08f,.3f):new Color(.75f,.18f,.14f));if(mk.Key=="⌛")DrawIcon(PixelArt.IconTurn,new Rect(b.x+2,b.y+2,16,16));else GUI.Label(b,mk.Key,dipBadge);if(b.Contains(m))councilHover=mk.Value;x-=23;}
   if(GUI.Button(r,"",GUIStyle.none)){if(pick&&Event.current.clickCount>=2){OpenCourt(o);GUIUtility.ExitGUI();}councilPick=o;}}
  GUI.EndScrollView();
 }
 // Agreements in force: who is bound, what each owes, how long is left, and what can be done.
 void DrawCouncilAgreements(Rect area,int[] known){
  Fill(area,new Color(.03f,.05f,.1f,.9f));var rows=new List<Action<Rect>>();var text=new GUIStyle(courtText){fontSize=14,wordWrap=true,alignment=TextAnchor.UpperLeft};var head=new GUIStyle(courtCaps){fontSize=14,alignment=TextAnchor.MiddleLeft,normal={textColor=CourtCream}};
  var heights=new List<float>();float w=area.width-30;
  foreach(int o in known){int who=o;
   foreach(var g in world.AgreementsBetween(0,o).Where(world.AgreementActive)){var deal=g;string t=world.AgreementText(deal)+(deal.price>0?" Paid at signing: "+deal.price+" gold.":"")+" When it ends, units inside are escorted to the border.";
    heights.Add(text.CalcHeight(new GUIContent(t),w-250)+34);rows.Add(r=>{GUI.Label(new Rect(r.x+8,r.y+2,w-250,22),(deal.gate>0?"GATE TRANSIT":"PASSAGE")+"  ·  "+world.NationName(who).ToUpperInvariant(),head);GUI.Label(new Rect(r.x+8,r.y+24,w-250,r.height-26),t,text);
     bool willing=world.WouldAgreeToCancel(deal,0);
     if(CourtButton(new Rect(r.xMax-236,r.y+6,112,28),"","Audience",CourtNavy)){OpenCourt(who);GUIUtility.ExitGUI();}
     if(CourtButton(new Rect(r.xMax-118,r.y+6,112,28),"","End early",CourtNavy)){AskCouncil(willing?"End this agreement now, by mutual consent? "+world.NationName(who)+" agrees to release you. No one is blamed; nothing is refunded."+(deal.guest==0?" Your units inside their land will be escorted to the border.":""):world.NationName(who)+" will not release you. Going back on it anyway is a broken word: they will remember it for a long time and trust your promises less."+(deal.host==0?" Their units inside your land will be escorted out.":""),()=>{string why=willing?world.CancelByAgreement(deal.id,0):world.BreakAgreement(deal.id,0);status=why==""?(willing?"Agreement ended by mutual consent.":"You went back on the agreement."):why;});}});}
   foreach(var c in world.contracts.Where(c=>c.from==0&&c.to==o||c.from==o&&c.to==0)){var con=c;bool mine=con.from==0;var give=new List<string>();var get=new List<string>();
    if((mine?con.givePerTurn:con.askPerTurn)>0)give.Add((mine?con.givePerTurn:con.askPerTurn)+" gold a turn");if((mine?con.giveResource:con.askResource)>0)give.Add(World.ResourceNames[mine?con.giveResource:con.askResource]);
    if((mine?con.askPerTurn:con.givePerTurn)>0)get.Add((mine?con.askPerTurn:con.givePerTurn)+" gold a turn");if((mine?con.askResource:con.giveResource)>0)get.Add(World.ResourceNames[mine?con.askResource:con.giveResource]);
    string t="You give "+(give.Count>0?string.Join(" and ",give.ToArray()):"nothing")+"; they give "+(get.Count>0?string.Join(" and ",get.ToArray()):"nothing")+". Runs from turn "+con.start+" until turn "+con.end+" begins ("+Math.Max(0,con.end-world.turn)+" turns left). Payments are made at the start of each turn.";
    heights.Add(text.CalcHeight(new GUIContent(t),w-130)+34);rows.Add(r=>{GUI.Label(new Rect(r.x+8,r.y+2,w-130,22),"TRADE DEAL  ·  "+world.NationName(who).ToUpperInvariant(),head);GUI.Label(new Rect(r.x+8,r.y+24,w-130,r.height-26),t,text);if(CourtButton(new Rect(r.xMax-118,r.y+6,112,28),"","Audience",CourtNavy)){OpenCourt(who);GUIUtility.ExitGUI();}});}
   var held=Enumerable.Range(0,Pacts.Count).Where(k=>world.Pact(0,o,k)).ToList();
   if(held.Count>0){string t=string.Join("  ",held.Select(k=>Pacts.Names[k]+" (since turn "+world.Relation(0,who).pactSince[k]+"): "+Pacts.Effects[k]).ToArray())+" Pacts stand until one side ends them, in the audience.";
    heights.Add(text.CalcHeight(new GUIContent(t),w-130)+34);rows.Add(r=>{GUI.Label(new Rect(r.x+8,r.y+2,w-130,22),"PACTS  ·  "+world.NationName(who).ToUpperInvariant(),head);GUI.Label(new Rect(r.x+8,r.y+24,w-130,r.height-26),t,text);if(CourtButton(new Rect(r.xMax-118,r.y+6,112,28),"","Audience",CourtNavy)){OpenCourt(who);GUIUtility.ExitGUI();}});}
  }
  if(rows.Count==0){GUI.Label(new Rect(area.x+20,area.y+20,area.width-40,60),"No agreements in force. Request an audience with a leader to propose one.",new GUIStyle(courtItalic){fontSize=16});return;}
  councilListScroll=GUI.BeginScrollView(area,councilListScroll,new Rect(0,0,area.width-18,Math.Max(area.height,heights.Sum()+8)));float y=4;
  for(int k=0;k<rows.Count;k++){var r=new Rect(4,y,w+10,heights[k]-4);Fill(r,new Color(.06f,.09f,.17f,.92f));Fill(new Rect(r.x,r.y,r.width,1),new Color(CourtGold.r,CourtGold.g,CourtGold.b,.5f));rows[k](r);y+=heights[k];}
  GUI.EndScrollView();
 }
 // The diplomatic history: what was signed, kept, cancelled or broken, and wars and peaces among those you know.
 void DrawCouncilHistory(Rect area){
  Fill(area,new Color(.03f,.05f,.1f,.9f));var top=new Rect(area.x+8,area.y+6,area.width-16,30);
  if(CourtButton(new Rect(top.x,top.y,150,28),"",historyAll?"▸ Everyone":"Everyone",historyAll?CourtGoldFace:CourtNavy)){historyAll=true;councilListScroll=Vector2.zero;}
  if(CourtButton(new Rect(top.x+156,top.y,330,28),"",(historyAll?"":"▸ ")+"With "+world.NationName(councilPick),historyAll?CourtNavy:CourtGoldFace)){historyAll=false;councilListScroll=Vector2.zero;}
  var list=world.HistoryFor(historyAll?-1:councilPick).Take(120).ToList();var body=new Rect(area.x,area.y+40,area.width,area.height-40);
  if(list.Count==0){GUI.Label(new Rect(body.x+20,body.y+14,body.width-40,40),"Nothing has passed yet.",new GUIStyle(courtItalic){fontSize=16});return;}
  var text=new GUIStyle(courtText){fontSize=14,wordWrap=true,alignment=TextAnchor.UpperLeft};float w=body.width-40;var hs=list.Select(r=>text.CalcHeight(new GUIContent(r.text),w-250)+10).ToList();
  councilListScroll=GUI.BeginScrollView(body,councilListScroll,new Rect(0,0,body.width-18,Math.Max(body.height,hs.Sum()+8)));float y=4;
  for(int k=0;k<list.Count;k++){var r=list[k];var row=new Rect(4,y,w+14,hs[k]-3);Fill(row,new Color(.06f,.09f,.17f,.92f));
   Color ink=r.kind==RecordKinds.Breach||r.kind==RecordKinds.War?new Color(1f,.55f,.45f):r.kind==RecordKinds.Fulfilled||r.kind==RecordKinds.Peace||r.kind==RecordKinds.Signed?new Color(.6f,.9f,.65f):CourtCream;
   GUI.Label(new Rect(row.x+8,row.y+4,70,20),"Turn "+r.turn,new GUIStyle(courtItalic){fontSize=13,wordWrap=false});
   GUI.Label(new Rect(row.x+80,row.y+4,166,20),RecordKinds.Names[r.kind],new GUIStyle(courtText){fontSize=13,wordWrap=false,normal={textColor=ink}});
   GUI.Label(new Rect(row.x+250,row.y+4,w-250,hs[k]-8),r.text,text);y+=hs[k];}
  GUI.EndScrollView();
 }
 // Escape closes the topmost diplomacy panel first. True when it closed something.
 bool DiplomacyEscape(){
  if(councilConfirm!=null){councilConfirm=null;councilConfirmDo=null;return true;}
  if(!diplomacyChooser){if(customDeal){customDeal=false;return true;}if(requestsView){requestsView=false;return true;}if(claimsView){claimsView=false;return true;}if(diplomacyDiscuss){diplomacyDiscuss=false;return true;}return false;}
  if(councilView!=0){councilView=0;return true;}
  return false;
 }
}
}
