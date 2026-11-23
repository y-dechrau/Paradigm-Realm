using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Negotiated, time-limited agreements (owner brief, 23 Nov 2026; Design/DIPLOMACY-NEGOTIATION-2026-11-23.md).
// A trade deal (DiplomaticOffer, kind Trade) can now carry two more clauses each way:
//  - passage: the giver lets the other side's units cross all its land for the deal's duration;
//  - gate transit: the giver lets them reach and use one named gateway in its land.
// On acceptance each becomes a DiplomaticAgreement with a start and an end turn. It is in force while turn < end; it expires
// when turn `end` begins (AgreementsTurn, in AdvanceDiplomacy), exactly once. CanEnter reads it (PassageTile), so movement,
// routes and the rival AI all obey it, and EnterReason says why a border is closed. When it ends, the guest's units still
// inside are escorted out at once by the existing trespass rule. Endings are told apart: expired (fulfilled), cancelled by
// agreement, broken by one side, torn up by war, or lapsed because the host lost the land or gateway. Each is written to the
// saved diplomatic history (DiplomaticRecord), and fulfilment and breach leave different marks that later answers weigh.
[Serializable] public class DiplomaticAgreement {
 // gate: 0 = passage through all the host's land; otherwise the gateway's cell + 1.
 // status: 0 in force, 1 expired, 2 cancelled by agreement, 3 broken by the guest, 4 broken by the host, 5 ended by war, 6 lapsed.
 public int id,guest,host,start,end,gate,status,endedTurn,price;public bool warned;
}
[Serializable] public class DiplomaticRecord {public int turn,a,b,kind;public string text="";}
public static class RecordKinds {
 public const int FirstContact=0,Signed=1,Fulfilled=2,Cancelled=3,Breach=4,War=5,Peace=6,Refused=7,Counter=8,Other=9;
 public static readonly string[] Names={"First contact","Agreement signed","Agreement fulfilled","Cancelled by agreement","Agreement broken","War","Peace","Refused","Counteroffer","Dealings"};
 // What a line of the older event text means (DiplomacyEvent writes plain sentences).
 public static int Of(string text){string s=text.ToLowerInvariant();
  if(s.Contains("counteroffer"))return Counter;if(s.Contains("default")||s.Contains("broken")||s.Contains("broke "))return Breach;
  if(s.Contains("declined")||s.Contains("refus"))return Refused;if(s.Contains("completed"))return Fulfilled;
  if(s.Contains("peace"))return Peace;if(s.Contains("war"))return War;if(s.Contains("agreed")||s.Contains("signed"))return Signed;return Other;}
}
// Everything a designer may want to tune about negotiation, in one place.
public static class DiplomacyTuning {
 // What a host asks for letting soldiers cross: a base, plus so much a turn (in points, about gold at normal need).
 public const int PassageBase=10,PassagePerTurn=2,GateBase=15,GatePerTurn=1;
 // An army near the host's cities: points for each formation the host can see within FrontierReach of one, up to FrontierMax.
 public const int FrontierReach=5,FrontierEach=12,FrontierMax=60,FrontierFrom=2;
 // Marks left by how an agreement ends, and what they do to trust.
 public const int KeptMark=3,KeptTrust=3,KeptMinTurns=10,BrokenMark=-12,BrokenTrust=-15,WarMark=-10;
 // A warning this many turns before an agreement ends; proposals the player may make to one leader in a turn; the turns
 // between a rival's requests for passage to the same neighbour; how long a rival's request waits for an answer.
 public const int WarnTurns=3,TalksPerTurn=3,AskCooldown=20,AnswerTurns=3,HistoryMax=300;
 // Counteroffers: the most gold a leader will name, and the step it searches in.
 public const int CounterMaxGold=600,CounterStep=5;
 // Negotiating personality by agenda (CivLore.AgendaNames order): how much a leader dislikes foreign soldiers on its land
 // (percent), how far it trusts a kept record (percent of trust counted), and how much it wants passage for itself (percent).
 // Merchant Prince, Seeker of Wisdom, Defender of the Faith, Builder of Wonders, Hungry for Land, Keeper of the Peace,
 // Warden of the Borders, Respects Only Strength.
 public static readonly int[] Caution={70,90,110,100,130,90,200,120};
 public static readonly int[] TrustWeight={60,50,50,40,30,70,40,30};
 public static readonly int[] Reach={120,100,90,80,150,70,60,130};
 public static int Pick(int[] table,int agenda){return agenda>=0&&agenda<table.Length?table[agenda]:100;}
}
public partial class World {
 public List<DiplomaticAgreement> agreements=new List<DiplomaticAgreement>();
 public List<DiplomaticRecord> diplomaticHistory=new List<DiplomaticRecord>();
 [NonSerialized] HashSet<int> passageKeys;[NonSerialized] int talksTurn=-1;[NonSerialized] Dictionary<int,int> talks;
 void EnsureAgreements(){if(agreements==null)agreements=new List<DiplomaticAgreement>();if(diplomaticHistory==null)diplomaticHistory=new List<DiplomaticRecord>();}
 // ---- history ----
 public void Record(int kind,int a,int b,string text){EnsureAgreements();diplomaticHistory.Add(new DiplomaticRecord{turn=turn,a=a,b=b,kind=kind,text=text});if(diplomaticHistory.Count>DiplomacyTuning.HistoryMax)diplomaticHistory.RemoveAt(0);}
 // What the player may read: its own dealings, and public events between civilisations it has met.
 public IEnumerable<DiplomaticRecord> HistoryFor(int partner=-1){EnsureAgreements();for(int i=diplomaticHistory.Count-1;i>=0;i--){var r=diplomaticHistory[i];bool mine=r.a==0||r.b==0;
   if(partner>=0?(r.a==partner||r.b==partner)&&(mine||Known(r.a)&&Known(r.b)&&(r.kind==RecordKinds.War||r.kind==RecordKinds.Peace)):mine||Known(r.a)&&Known(r.b)&&(r.kind==RecordKinds.War||r.kind==RecordKinds.Peace))yield return r;}}
 bool Known(int o){return o==0||o>0&&o<civCount&&met[o];}
 // ---- agreements in force ----
 public bool AgreementActive(DiplomaticAgreement g){return g!=null&&g.status==0&&turn<g.end;}
 public IEnumerable<DiplomaticAgreement> AgreementsBetween(int a,int b){EnsureAgreements();return agreements.Where(g=>SamePair(g.guest,g.host,a,b));}
 public DiplomaticAgreement PassageAgreement(int guest,int host,int gate=0){EnsureAgreements();foreach(var g in agreements)if(g.guest==guest&&g.host==host&&g.gate==gate&&AgreementActive(g))return g;return null;}
 void ResetPassage(){passageKeys=null;networks=null;}
 // May this owner's units stand on this tile because of an agreement? Asked on every step of every route, so the whole-land
 // agreements are kept as a set of guest/host keys.
 public bool PassageTile(int owner,Tile t){
  if(agreements==null||agreements.Count==0||t.owner<0||t.owner==owner)return false;
  if(passageKeys==null){passageKeys=new HashSet<int>();foreach(var g in agreements)if(AgreementActive(g)&&g.gate==0)passageKeys.Add(g.guest*1000+g.host);}
  if(passageKeys.Contains(owner*1000+t.owner))return true;
  foreach(var g in agreements){if(g.gate==0||g.guest!=owner||g.host!=t.owner||!AgreementActive(g))continue;var gate=tiles[g.gate-1];if(Distance(gate.x,gate.y,t.x,t.y)<=GateRoadReach)return true;}
  return false;
 }
 public const int GateRoadReach=2;
 // Why a tile cannot be entered, in the player's words ("" when it can).
 public string EnterReason(int owner,Tile t){
  if(t==null||CanEnter(owner,t))return "";string host=NationName(t.owner);
  var last=agreements==null?null:agreements.Where(g=>g.guest==owner&&g.host==t.owner&&g.status!=0).OrderByDescending(g=>g.endedTurn).FirstOrDefault();
  string was=last==null?"":" Your "+(last.gate>0?"gate transit":"passage")+" there "+EndWord(last)+" on turn "+last.endedTurn+".";
  if(t.gateLink>0)return "This gateway stands in "+host+"'s land: crossing it needs open borders, a gate access pact or a gate transit agreement."+was;
  var gateDeal=agreements==null?null:agreements.FirstOrDefault(g=>g.guest==owner&&g.host==t.owner&&g.gate>0&&AgreementActive(g));
  if(gateDeal!=null)return host+"'s borders are closed: your agreement covers only the road to "+GateLabel(gateDeal.gate)+" (within "+GateRoadReach+" tiles of it).";
  return host+"'s borders are closed to you: there is no open borders pact or passage agreement."+was;
 }
 static string EndWord(DiplomaticAgreement g){return g.status==1?"expired":g.status==2?"was cancelled by agreement":g.status==3||g.status==4?"was broken":g.status==5?"ended in war":"lapsed";}
 public string GateLabel(int gatePlusOne){if(gatePlusOne<=0||gatePlusOne>tiles.Length)return "the gateway";var t=tiles[gatePlusOne-1];return "the "+GateStyleNames[GateStyle(t)].ToLower()+" at "+NearestCityName(t);}
 string NearestCityName(Tile t){City best=null;int least=int.MaxValue;foreach(var c in cities){if(At(c.x,c.y).realm!=t.realm)continue;int d=Distance(c.x,c.y,t.x,t.y);if(d<least){least=d;best=c;}}return best==null?"the frontier":least==0?best.name:"near "+best.name;}
 // Gateways in a host's land that a guest knows of (never one it has not found).
 public List<Tile> KnownGatesOf(int host,int guest){var list=new List<Tile>();foreach(int cell in GateCells()){var t=tiles[cell];if(t.owner!=host)continue;bool known=guest==0?t.seen:nations[guest].known!=null&&cell<nations[guest].known.Length&&nations[guest].known[cell];if(known)list.Add(t);}return list;}
 public string AgreementText(DiplomaticAgreement g){
  string what=g.gate>0?"may reach and use "+GateLabel(g.gate)+" in "+NationName(g.host)+"'s land":"may cross "+NationName(g.host)+"'s land";
  return NationName(g.guest)+"'s units "+what+" from turn "+g.start+" until turn "+g.end+" begins"+(g.status==0?" ("+Math.Max(0,g.end-turn)+" turns left)":" — "+EndWord(g)+" on turn "+g.endedTurn)+".";
 }
 // ---- clauses on an offer ----
 public bool HasPassageTerms(DiplomaticOffer o){return o.givePassage||o.askPassage||o.giveGate>0||o.askGate>0;}
 string PassageProblem(DiplomaticOffer o){
  if(!HasPassageTerms(o))return "";if(o.kind!=TreatyKind.Trade)return "Passage belongs in a trade deal.";
  foreach(bool give in new[]{true,false}){int host=give?o.from:o.to,guest=give?o.to:o.from;bool passage=give?o.givePassage:o.askPassage;int gate=give?o.giveGate:o.askGate;
   if(!passage&&gate==0)continue;
   if(HasAccess(guest,host))return NationName(guest)+" already has open borders with "+NationName(host)+".";
   if(!cities.Any(c=>c.owner==host))return NationName(host)+" holds no land to cross.";
   if(passage&&PassageAgreement(guest,host)!=null)return "A passage agreement is already in force.";
   if(gate>0){if(gate>tiles.Length||tiles[gate-1].gateLink<=0||tiles[gate-1].owner!=host)return NationName(host)+" does not hold that gateway.";
    if(!KnownGatesOf(host,guest).Contains(tiles[gate-1]))return "That gateway has not been found.";
    if(passage)return "Passage through all their land already reaches the gateway.";if(Pact(guest,host,Pacts.GateAccess))return "A gate access pact already covers it.";if(PassageAgreement(guest,host,gate)!=null)return "A transit agreement for that gateway is already in force.";}}
  return "";
 }
 // What the recipient makes of the passage clauses: every reason, by name (they join the reckoning in Acceptance).
 void PassageLines(DiplomaticOffer o,List<KeyValuePair<string,int>> lines,ref int sum){
  if(!HasPassageTerms(o))return;int r=o.to,p=o.from;int agenda=AgendaOf(r);{int openHand=WayDealInterest(p);if(openHand>0)Part(lines,ref sum,"They hold to the Open Hand: a word given is a word kept",openHand);}
  if(o.askPassage||o.askGate>0){
   bool gate=!o.askPassage;int cost=((gate?DiplomacyTuning.GateBase:DiplomacyTuning.PassageBase)+o.duration*(gate?DiplomacyTuning.GatePerTurn:DiplomacyTuning.PassagePerTurn))*DiplomacyTuning.Pick(DiplomacyTuning.Caution,agenda)/100;
   Part(lines,ref sum,gate?"Letting your people use their gateway for "+o.duration+" turns":"Letting your soldiers cross their land for "+o.duration+" turns",-cost);
   int massed=FrontierForce(r,p);if(massed>=DiplomacyTuning.FrontierFrom)Part(lines,ref sum,"Your army is massed at their frontier ("+massed+" formations)",-Math.Min(DiplomacyTuning.FrontierMax,massed*DiplomacyTuning.FrontierEach*DiplomacyTuning.Pick(DiplomacyTuning.Caution,agenda)/100));
   int trust=Trust(r,p)*DiplomacyTuning.Pick(DiplomacyTuning.TrustWeight,agenda)/100;if(trust>0)Part(lines,ref sum,"Trust in your word",trust);
   int kept=OpinionMod(r,p,OpinionMods.KeptWord);if(kept>0)Part(lines,ref sum,"You kept your word before",kept*2);
   int broke=OpinionMod(r,p,OpinionMods.Broken);if(broke<0)Part(lines,ref sum,"A promise you broke",broke*3);
   int att=Attitude(r,p);if(att==Attitudes.Hostile||att==Attitudes.Unfriendly||att==Attitudes.Rival)Part(lines,ref sum,"They do not want you near",-30);else if(att==Attitudes.Friendly||att==Attitudes.Loyal)Part(lines,ref sum,"Friendship",15);
   for(int x=0;x<civCount;x++)if(x!=r&&x!=p&&AtWar(r,x)&&AtWar(p,x)){Part(lines,ref sum,"You fight the same enemy ("+NationName(x)+")",25);break;}
  }
  if(o.givePassage||o.giveGate>0){int want=(AtWarWithAnyone(r)?20:5)*DiplomacyTuning.Pick(DiplomacyTuning.Reach,agenda)/100;Part(lines,ref sum,o.givePassage?"Passage through your land":"Use of your gateway",want);}
 }
 int AgendaOf(int owner){var e=Civ(owner);return e!=null?e.Agenda:-1;}
 // The asker's soldiers that the host can see within reach of one of its cities.
 public int FrontierForce(int host,int asker){int n=0;var own=cities.Where(c=>c.owner==host).ToList();if(own.Count==0)return 0;
  foreach(var u in units){if(u.owner!=asker||Power[u.type]==0||IsNaval(u.type))continue;var t=At(u.x,u.y);if(t==null||t.owner==asker||!VisibleTo(host,u.x,u.y))continue;
   foreach(var c in own)if(Distance(c.x,c.y,u.x,u.y)<=TerritoryRadius(c)+DiplomacyTuning.FrontierReach){n++;break;}}
  return n;}
 // The strongest reason or two for an answer, in the leader's own words. Only reasons that were really weighed are named.
 public string AnswerReasons(DiplomaticOffer o){
  var lines=AcceptanceLines(o).Where(l=>l.Value!=0).ToList();if(lines.Count==0)return "";bool yes=lines.Sum(l=>l.Value)>=0;
  var best=lines.Where(l=>l.Value>0).OrderByDescending(l=>l.Value).FirstOrDefault();var worst=lines.Where(l=>l.Value<0).OrderBy(l=>l.Value).FirstOrDefault();
  string good=best.Key==null?null:Prose(best.Key,true),bad=worst.Key==null?null:Prose(worst.Key,false);
  if(yes)return good!=null?(bad!=null&&-worst.Value*2>best.Value?Cap(good)+", though "+bad+".":Cap(good)+"."):"We see no harm in it.";
  return bad!=null?(good!=null&&best.Value*2>-worst.Value?Cap(good)+", but "+bad+".":Cap(bad)+"."):"It does not serve us.";
 }
 static string Cap(string s){return s.Length==0?s:char.ToUpper(s[0])+s.Substring(1);}
 static string Prose(string label,bool good){
  if(label.StartsWith("What the terms are worth"))return good?"your offer is generous":"the terms give us too little";
  if(label.StartsWith("Goodwill"))return "we think well of you";
  if(label.StartsWith("Letting your soldiers"))return "foreign soldiers on our roads are a heavy thing to ask";
  if(label.StartsWith("Letting your people use"))return "our gateway is not lightly opened to others";
  if(label.StartsWith("Your army is massed"))return "your army is massed at our frontier";
  if(label.StartsWith("Trust in your word"))return "you have earned our trust";
  if(label.StartsWith("You kept your word"))return "you kept your word to us before";
  if(label.StartsWith("A promise you broke"))return "we remember the promise you broke";
  if(label.StartsWith("They do not want you near"))return "we do not want you near our cities";
  if(label.StartsWith("Friendship"))return "we count you a friend";
  if(label.StartsWith("You fight the same enemy"))return "we fight the same enemy";
  if(label.StartsWith("Passage through your land")||label.StartsWith("Use of your gateway"))return "the way through your land is useful to us";
  if(label.StartsWith("The terms offered"))return good?"the terms are fair":"the terms are poor";
  return (good?"in its favour: ":"against it: ")+label.ToLowerInvariant();
 }
 // ---- counteroffers ----
 // A leader who refuses the player's trade names what would change its mind: more gold, or the same for fewer turns. The
 // counter is an offer from the leader with the changed terms marked (changed: 1 gold now, 4 duration). Null when nothing
 // within reach would do. Leaders never counter each other, so talks cannot go round for ever.
 DiplomaticOffer CounterFor(DiplomaticOffer o){
  if(o.from!=0||o.kind!=TreatyKind.Trade)return null;
  foreach(int duration in o.duration>10&&HasPassageTerms(o)?new[]{o.duration,10}:new[]{o.duration}){
   for(int extra=DiplomacyTuning.CounterStep;extra<=DiplomacyTuning.CounterMaxGold;extra+=DiplomacyTuning.CounterStep){
    var t=o.Copy();t.duration=duration;int less=Math.Min(extra,t.askGold);t.askGold-=less;t.giveGold+=extra-less;
    if(OfferWillingness(t)<0)continue;if(Treasury(0)<t.giveGold)break;
    var c=new DiplomaticOffer{from=o.to,to=0,kind=o.kind,duration=duration,giveGold=t.askGold,askGold=t.giveGold,givePerTurn=o.askPerTurn,askPerTurn=o.givePerTurn,giveResource=o.askResource,askResource=o.giveResource,
     givePassage=o.askPassage,askPassage=o.givePassage,giveGate=o.askGate,askGate=o.giveGate,counterOf=o.id,changed=(t.giveGold!=o.giveGold||t.askGold!=o.askGold?1:0)|(duration!=o.duration?4:0)};
    return OfferProblem(c)==""?c:null;}}
  return null;
 }
 // The player may put a few proposals to one leader in a turn (a counter, then an amendment), not an endless stream.
 bool TalkAllowed(int partner,bool spend){if(talksTurn!=turn||talks==null){talksTurn=turn;talks=new Dictionary<int,int>();}int n;talks.TryGetValue(partner,out n);if(n>=DiplomacyTuning.TalksPerTurn)return false;if(spend)talks[partner]=n+1;return true;}
 public int TalksLeft(int partner){if(talksTurn!=turn||talks==null)return DiplomacyTuning.TalksPerTurn;int n;talks.TryGetValue(partner,out n);return Math.Max(0,DiplomacyTuning.TalksPerTurn-n);}
 // ---- signing ----
 void SignPassage(DiplomaticOffer o){
  if(!HasPassageTerms(o))return;EnsureAgreements();int until=turn+o.duration;
  foreach(bool give in new[]{true,false}){int host=give?o.from:o.to,guest=give?o.to:o.from;bool passage=give?o.givePassage:o.askPassage;int gate=give?o.giveGate:o.askGate;if(!passage&&gate==0)continue;
   var g=new DiplomaticAgreement{id=nextDiplomaticId++,guest=guest,host=host,start=turn,end=until,gate=passage?0:gate,price=give?o.askGold:o.giveGold};agreements.Add(g);
   Record(RecordKinds.Signed,guest,host,AgreementText(g)+(g.price>0?" Paid: "+g.price+" gold at signing.":""));}
  ResetPassage();
 }
 // ---- ending ----
 void EndAgreement(DiplomaticAgreement g,int status,string why){
  if(g.status!=0)return;g.status=status;g.endedTurn=turn;ResetPassage();
  string what=(g.gate>0?"Gate transit":"Passage")+" for "+NationName(g.guest)+" through "+NationName(g.host)+"'s land";
  int kind=status==1?RecordKinds.Fulfilled:status==2?RecordKinds.Cancelled:status==6?RecordKinds.Other:RecordKinds.Breach;
  Record(kind,g.guest,g.host,what+" "+EndWord(g)+(why!=""?": "+why:"")+".");
  if(g.guest==0||g.host==0)Note(what+" "+EndWord(g)+(why!=""?": "+why:"")+".");
  if(status==1&&g.end-g.start>=DiplomacyTuning.KeptMinTurns){AddOpinion(g.host,g.guest,OpinionMods.KeptWord,DiplomacyTuning.KeptMark);AddOpinion(g.guest,g.host,OpinionMods.KeptWord,DiplomacyTuning.KeptMark);SetTrust(g.host,g.guest,Trust(g.host,g.guest)+DiplomacyTuning.KeptTrust);SetTrust(g.guest,g.host,Trust(g.guest,g.host)+DiplomacyTuning.KeptTrust);}
  if(status==3||status==4){int breaker=status==3?g.guest:g.host,victim=status==3?g.host:g.guest;AddOpinion(victim,breaker,OpinionMods.Broken,DiplomacyTuning.BrokenMark);SetTrust(victim,breaker,Trust(victim,breaker)+DiplomacyTuning.BrokenTrust);}
  // The guest's units still inside are set down outside at once (Trespass.cs), unless war has opened the border anyway.
  if(!AtWar(g.guest,g.host))ExpelTrespassers(g.guest);
 }
 // Each turn, after the turn number moves on: expiry on the stated turn, a warning a few turns before, and the endings that
 // circumstances force.
 void AgreementsTurn(){
  EnsureAgreements();
  foreach(var g in agreements.ToArray()){
   if(g.status!=0)continue;
   if(AtWar(g.guest,g.host)){var p=Relation(g.guest,g.host);int declarer=p!=null?p.lastDeclarer:-1;EndAgreement(g,declarer==g.host?4:declarer==g.guest?3:5,"war was declared");continue;}
   if(!cities.Any(c=>c.owner==g.host)){EndAgreement(g,6,NationName(g.host)+" holds no land any more");continue;}
   if(g.gate>0&&tiles[g.gate-1].owner!=g.host){EndAgreement(g,6,NationName(g.host)+" no longer holds the gateway");continue;}
   if(g.end<=turn){EndAgreement(g,1,"");continue;}
   if(!g.warned&&g.end-turn<=DiplomacyTuning.WarnTurns){g.warned=true;if(g.guest==0){int inside=units.Count(u=>u.owner==0&&At(u.x,u.y).owner==g.host);Note((g.gate>0?"Gate transit":"Passage")+" through "+NationName(g.host)+"'s land ends in "+(g.end-turn)+" turns."+(inside>0?" "+inside+" of your units are inside; any still there will be escorted to the border."+(g.gate>0?" Units beyond the gateway stay on the far side.":""):""));}
    else if(g.host==0)Note(NationName(g.guest)+"'s "+(g.gate>0?"gate transit":"passage")+" through your land ends in "+(g.end-turn)+" turns.");}
  }
  agreements.RemoveAll(g=>g.status!=0&&g.endedTurn<turn-200);
 }
 // Ending early. By agreement leaves no mark; a rival agrees when it loses nothing by it. Otherwise it is a broken word.
 public string CancelReason(int id,int by){var g=agreements.Find(x=>x.id==id);if(g==null||g.status!=0)return "No such agreement.";if(by!=g.guest&&by!=g.host)return "Not yours to end.";if(result!="")return "The game has ended.";return "";}
 public bool WouldAgreeToCancel(DiplomaticAgreement g,int asker){int other=asker==g.guest?g.host:g.guest;if(other==0)return false;
  // A host that has been paid loses nothing; a guest gives up its way through only if none of its units are using it.
  return other==g.host||!units.Any(u=>u.owner==g.guest&&At(u.x,u.y).owner==g.host);}
 public string CancelByAgreement(int id,int by){string why=CancelReason(id,by);if(why!="")return why;var g=agreements.Find(x=>x.id==id);if(!WouldAgreeToCancel(g,by))return NationName(by==g.guest?g.host:g.guest)+" will not release you: its units are using the passage.";EndAgreement(g,2,"both sides agreed");return "";}
 public string BreakAgreement(int id,int by){string why=CancelReason(id,by);if(why!="")return why;WayBrokenWord(by);var g=agreements.Find(x=>x.id==id);EndAgreement(g,by==g.guest?3:4,NationName(by)+" went back on it");return "";}
 // ---- rivals ask for passage ----
 // A rival at war asks a neighbour whose land lies on its way to the enemy, offering gold; at most once in AskCooldown turns
 // per neighbour, and never while another proposal between the two is waiting.
 void PassageAI(int a){
  if(!ActiveRival(a)||!AtWarWithAnyone(a))return;var home=CapitalOf(a);if(home==null)return;
  for(int x=1;x<civCount;x++){if(!AtWar(a,x))continue;var foe=CapitalOf(x);if(foe==null)continue;int direct=Distance(home.x,home.y,foe.x,foe.y);
   for(int b=0;b<civCount;b++){if(b==a||b==x||IsNativeNation(b)||!HasContact(a,b)||AtWar(a,b)||HasAccess(a,b)||PassageAgreement(a,b)!=null)continue;var mid=CapitalOf(b);if(mid==null)continue;
    if(Distance(home.x,home.y,mid.x,mid.y)>=direct||Distance(mid.x,mid.y,foe.x,foe.y)>=direct)continue;
    var p=Relation(a,b);if(p==null||turn-p.lastPassageAsk<DiplomacyTuning.AskCooldown||offers.Any(o=>o.status==0&&SamePair(o.from,o.to,a,b)))continue;
    var offer=new DiplomaticOffer{from=a,to=b,kind=TreatyKind.Trade,duration=15,askPassage=true,why="We are at war with "+NationName(x)+", and the road to them runs through your land."};
    int pay=DiplomacyTuning.PassageBase+offer.duration*DiplomacyTuning.PassagePerTurn;
    if(b!=0){for(pay=0;pay<=200;pay+=10){offer.giveGold=pay;if(OfferWillingness(offer)>=0)break;}if(pay>200)continue;}
    offer.giveGold=Math.Min(pay,Treasury(a)/2);if(offer.giveGold<5&&b==0)continue;
    p.lastPassageAsk=turn;Propose(offer);return;}}
 }
 bool AgreementsValid(){if(!LifewaysValid())return false;
  if(agreements==null||diplomaticHistory==null)return true;
  foreach(var g in agreements)if(g==null||g.guest<0||g.host<0||g.guest>=civCount||g.host>=civCount||g.guest==g.host||g.end<g.start||g.status<0||g.status>6||g.gate<0||g.gate>tiles.Length)return false;
  return diplomaticHistory.All(r=>r!=null&&r.kind>=0&&r.kind<RecordKinds.Names.Length);
 }
}
}
namespace ParadigmRealm {
public partial class World {
 // What each side gives, in plain words ("" for a side that gives nothing).
 public List<string> GivesList(DiplomaticOffer o,bool fromSide){
  var list=new List<string>();int gold=fromSide?o.giveGold:o.askGold,per=fromSide?o.givePerTurn:o.askPerTurn,res=fromSide?o.giveResource:o.askResource,gate=fromSide?o.giveGate:o.askGate;bool passage=fromSide?o.givePassage:o.askPassage;
  int giver=fromSide?o.from:o.to,taker=fromSide?o.to:o.from;
  if(gold>0)list.Add(gold+" gold at signing");if(per>0)list.Add(per+" gold a turn for "+o.duration+" turns");if(res>0)list.Add(ResourceNames[res]+" for "+o.duration+" turns");
  if(passage)list.Add("passage for "+NationName(taker)+"'s units through all "+NationName(giver)+"'s land for "+o.duration+" turns");
  if(gate>0)list.Add("use of "+GateLabel(gate)+" for "+o.duration+" turns");
  return list;
 }
 public string TermsText(DiplomaticOffer o){var a=GivesList(o,true);var b=GivesList(o,false);return NationName(o.from)+" gives "+(a.Count>0?string.Join(", ",a.ToArray()):"nothing")+"; "+NationName(o.to)+" gives "+(b.Count>0?string.Join(", ",b.ToArray()):"nothing");}
 // The whole agreement in plain English before anyone commits: who owes what, when it starts and ends, and what ending means.
 public string DealSummary(DiplomaticOffer o){
  string s=TermsText(o)+".";
  if(o.giveGold+o.askGold>0)s+=" Gold at signing changes hands once, the moment the deal is accepted.";
  if(HasPassageTerms(o)){s+=" It takes effect at once (turn "+turn+") and ends when turn "+(turn+o.duration)+" begins. Units still inside when it ends are escorted to the border";
   if(o.giveGate>0||o.askGate>0)s+="; units beyond the gateway stay on the far side and must find another way home or a new agreement";
   s+=". Ending it early by agreement leaves no mark; going back on it is remembered as a broken word.";}
  return s;
 }
}
}
