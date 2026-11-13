using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Requests and guarantees (owner brief, 13 Nov 2026; save version 43). One civilisation asks another to do something specific by a
// deadline: stop a war on a third party, keep troops away from its cities, join a war, defend a gateway, or send aid. A request
// names its target, its deadline, what the asker pays when it is done (incentive) and what the asker will do if it is refused or
// broken (consequence). Humans and rivals use the same rules and the same reckoning (RequestLines).
//
// Life of a request: Pending (the recipient decides; three turns for the player) -> Active (accepted, being carried out) ->
// Fulfilled (incentive paid, "Kept their word" and trust for the recipient) or Failed (broken word, "Abandoned us" between allies,
// and the threatened consequence). Refused requests carry the consequence only when the threat is credible. A request whose
// target vanishes, or whose gateway changes hands, is Cancelled with no blame (an unavoidable interruption).
public static class RequestKinds {
 public const int StopWar=0,Withdraw=1,JoinWar=2,DefendGate=3,Aid=4,Resource=5,Count=6;
 public static readonly string[] Names={"Stop a war","Withdraw troops","Join a war","Defend a gateway","Send aid","Share a resource"};
 public static readonly string[] Texts={
  "Make peace with the named civilisation before the deadline.",
  "Keep every soldier at least 4 tiles from the asker's cities for the whole term (a 2-turn grace to march out).",
  "Declare war on the named civilisation before the deadline (it must be an enemy of the asker).",
  "Keep a soldier within 2 tiles of the asker's gateway on at least half the turns of the term.",
  "Send the gold asked at once.",
  "Share an improved, road-connected resource for the term (it arrives as an import, as in a trade deal; a cut road ends it with no blame)."};
 // Resources that can be shared: Wheat, Iron, Gold, Horses, Spices (the ones trade deals carry).
 public const int FirstShared=1,LastShared=5;
 // Distance from the asker's cities that troops must keep under a withdrawal.
 public const int WithdrawRange=3,GateGuardRange=2,Grace=2,Cooldown=10,AnswerTurns=3,MaxActive=3;
}
public static class RequestStatus {
 public const int Pending=0,Active=1,Fulfilled=2,Failed=3,Refused=4,Cancelled=5;
 public static readonly string[] Names={"Awaiting an answer","In force","Fulfilled","Broken","Refused","Cancelled"};
}
public static class RequestConsequence {
 public const int None=0,Remember=1,Rivalry=2;
 public static readonly string[] Names={"No threat","\"We will remember this\"","\"We will name you a rival\""};
}
[Serializable] public class DiplomaticRequest {
 // gate is a tile index (Width*y+x). gold is the incentive the asker pays on success; aid is the gold asked for under Send aid.
 // unit is the soldier a rival recipient has posted to a gateway.
 public int id,from,to,kind,target=-1,gate=-1,created,accepted,deadline,duration=10,status,gold,aid,consequence,served,unit=-1;
 public bool endRivalry;public string answer="";
 // Share a resource (added 13 Nov 2026, after the save version 43 change; older version 43 saves read it as 0): the resource id.
 public int resource;
 public DiplomaticRequest Copy(){return (DiplomaticRequest)MemberwiseClone();}
}
public partial class World {
 public List<DiplomaticRequest> requests=new List<DiplomaticRequest>();
 bool Gone(int owner){return owner<0||owner>=civCount||!Survives(owner);}
 Tile GateTileOf(DiplomaticRequest r){return r.gate>=0&&r.gate<tiles.Length?tiles[r.gate]:null;}
 // Soldiers of owner within range of any city of other.
 public int TroopsNear(int owner,int other,int range){int n=0;var theirs=cities.Where(c=>c.owner==other).ToList();if(theirs.Count==0)return 0;foreach(var u in units)if(u.owner==owner&&Power[u.type]>0&&!IsNaval(u.type)&&theirs.Any(c=>Distance(c.x,c.y,u.x,u.y)<=range))n++;return n;}
 bool GuardAt(int owner,Tile gate){return gate!=null&&units.Any(u=>u.owner==owner&&Power[u.type]>0&&!IsNaval(u.type)&&Distance(u.x,u.y,gate.x,gate.y)<=RequestKinds.GateGuardRange);}
 public string RequestProblem(DiplomaticRequest r){
  if(r==null||r.from==r.to||!HasContact(r.from,r.to))return "First contact required.";if(result!="")return "The game has ended.";
  if(r.kind<0||r.kind>=RequestKinds.Count||r.duration<5||r.duration>30||r.gold<0||r.gold>2000||r.aid<0||r.aid>2000||r.consequence<0||r.consequence>2)return "Invalid request.";
  if(AtWar(r.from,r.to))return "Negotiate peace first.";
  if(Treasury(r.from)<r.gold)return "You cannot cover the incentive ("+r.gold+" gold).";
  if(r.endRivalry&&!Rival(r.from,r.to))return "You have not named them a rival, so there is no rivalry to end.";
  bool third=r.kind==RequestKinds.StopWar||r.kind==RequestKinds.JoinWar;
  if(third){if(r.target<0||r.target>=civCount||r.target==r.from||r.target==r.to||Gone(r.target))return "Choose another civilisation.";if(!HasContact(r.from,r.target)||!HasContact(r.to,r.target))return "Both of you must know "+NationName(r.target)+".";}
  switch(r.kind){
   case RequestKinds.StopWar:if(!AtWar(r.to,r.target))return NationName(r.to)+" is not at war with "+NationName(r.target)+".";break;
   case RequestKinds.Withdraw:if(TroopsNear(r.to,r.from,RequestKinds.WithdrawRange)==0)return "None of their soldiers are near your cities.";break;
   case RequestKinds.JoinWar:if(!AtWar(r.from,r.target))return "You are not at war with "+NationName(r.target)+".";if(AtWar(r.to,r.target))return "They are already at war with "+NationName(r.target)+".";if(Friends(r.to,r.target))return "A pact binds them to "+NationName(r.target)+".";break;
   case RequestKinds.DefendGate:{var g=GateTileOf(r);if(g==null||g.gateLink<=0||g.owner!=r.from)return "Choose a gateway on your own land.";if(!HasAccess(r.to,r.from)&&!Pact(r.to,r.from,Pacts.GateAccess))return "They need open borders or gate access to reach it.";break;}
   case RequestKinds.Aid:if(r.aid<=0)return "Ask for some gold.";if(Treasury(r.to)<r.aid)return "They do not have "+r.aid+" gold.";break;
   case RequestKinds.Resource:if(r.resource<RequestKinds.FirstShared||r.resource>RequestKinds.LastShared)return "Choose Wheat, Iron, Gold, Horses or Spices.";if(!HasDomesticResource(r.to,r.resource))return "They have no improved, road-connected "+ResourceNames[r.resource]+".";if(HasDomesticResource(r.from,r.resource))return "You already have your own "+ResourceNames[r.resource]+".";if(Embargoed(r.from,r.to))return "No dealings between rivals.";if(contracts.Any(c=>SamePair(c.from,c.to,r.from,r.to)))return "End the trade deal between you first (one deal per pair).";break;}
  if(requests.Any(x=>x.kind==r.kind&&x.from==r.from&&x.to==r.to&&(x.status==RequestStatus.Pending||x.status==RequestStatus.Active)))return "That request is already open.";
  if(requests.Any(x=>x.kind==r.kind&&x.from==r.from&&x.to==r.to&&x.status==RequestStatus.Refused&&turn-x.created<RequestKinds.Cooldown))return "Asked too recently; wait "+RequestKinds.Cooldown+" turns after a refusal.";
  if(requests.Count(x=>SamePair(x.from,x.to,r.from,r.to)&&x.status==RequestStatus.Active)>=RequestKinds.MaxActive)return "At most "+RequestKinds.MaxActive+" commitments between two civilisations at once.";
  return "";}
 // Whether the recipient can actually do it: soldiers to spare, a front within reach, gold.
 int Feasibility(DiplomaticRequest r,List<KeyValuePair<string,int>> lines){
  int sum=0;int to=r.to;
  if(r.kind==RequestKinds.JoinWar){
   var mine=cities.Where(c=>c.owner==to).ToList();var theirs=cities.Where(c=>c.owner==r.target).ToList();
   int close=mine.Count==0||theirs.Count==0?999:mine.Min(c=>theirs.Min(o=>Distance(c.x,c.y,o.x,o.y)));
   if(close>24)Part(lines,ref sum,"Too far away to fight there",-100);
   int ratio=Strength(to,to)*100/Math.Max(1,Strength(to,r.target));if(ratio<70)Part(lines,ref sum,"Too weak to take on "+NationName(r.target),-100);else if(ratio<100)Part(lines,ref sum,NationName(r.target)+" is stronger",-20);
   if(AtWarWithAnyone(to))Part(lines,ref sum,"Already fighting another war",-30);}
  if(r.kind==RequestKinds.DefendGate){var g=GateTileOf(r);int spare=units.Count(u=>u.owner==to&&Power[u.type]>0&&!IsNaval(u.type))-cities.Count(c=>c.owner==to);if(spare<1)Part(lines,ref sum,"No soldier to spare",-100);
   else if(g!=null&&!units.Any(u=>u.owner==to&&Power[u.type]>0&&!IsNaval(u.type)&&Distance(u.x,u.y,g.x,g.y)<=20))Part(lines,ref sum,"No soldier within reach",to==0?-10:-60);}
  if(r.kind==RequestKinds.Aid)Part(lines,ref sum,"Cost of "+r.aid+" gold",-(r.aid*GoldNeed(to)/100)/2);
  return sum;}
 // Every reason the recipient weighs, by name; it accepts when they add up to 0 or more.
 public List<KeyValuePair<string,int>> RequestLines(DiplomaticRequest r){var l=new List<KeyValuePair<string,int>>();RequestScore(r,l);return l;}
 public int RequestScore(DiplomaticRequest r,List<KeyValuePair<string,int>> lines){
  int sum=0,to=r.to,from=r.from;
  switch(r.kind){
   case RequestKinds.StopWar:{Part(lines,ref sum,"Will to finish their war",-25);int ex=Exhaustion(to,r.target);Part(lines,ref sum,"Their war exhaustion ("+ex+")",ex/2);if(to!=0&&Ambition(to,r.target)>=50)Part(lines,ref sum,"They want "+NationName(r.target)+"'s land",-30);break;}
   case RequestKinds.Withdraw:Part(lines,ref sum,"Troops to pull back ("+TroopsNear(to,from,RequestKinds.WithdrawRange)+")",-5-3*TroopsNear(to,from,RequestKinds.WithdrawRange));if(to!=0&&Ambition(to,from)>=50)Part(lines,ref sum,"They have designs on your land",-40);break;
   case RequestKinds.JoinWar:{Part(lines,ref sum,"Reluctance to go to war",-40);if(Threat(to,r.target)>=30||Rival(to,r.target)||Opinion(to,r.target)<=-30)Part(lines,ref sum,"They have their own quarrel with "+NationName(r.target),30);if(Opinion(to,r.target)>=40)Part(lines,ref sum,"They like "+NationName(r.target),-30);break;}
   case RequestKinds.DefendGate:Part(lines,ref sum,"A soldier kept far from home",-15);break;
   case RequestKinds.Aid:Part(lines,ref sum,"Reluctance to give",-5);break;
   case RequestKinds.Resource:{Part(lines,ref sum,"Reluctance to share "+ResourceNames[r.resource],-10);int need=ResourceNeed(to,r.resource);if(UnitResource.Contains(r.resource)&&Threat(to,from)>=30)Part(lines,ref sum,"It would arm a power they fear",-30);if(AtWar(from,to)==false&&r.to!=0&&Ambition(to,from)>=50)Part(lines,ref sum,"They have designs on your land",-40);if(need>=180)Part(lines,ref sum,"They need it for their own war",-15);break;}}
  sum+=Feasibility(r,lines);
  Part(lines,ref sum,"Opinion of you",Opinion(to,from)/3);
  Part(lines,ref sum,"Trust in you",Trust(to,from)/3);
  if(Allied(to,from)&&r.kind!=RequestKinds.Withdraw)Part(lines,ref sum,"Defensive pact: they stand by allies",25);
  else if(Friends(to,from))Part(lines,ref sum,"Non-aggression pact",5);
  if(r.kind==RequestKinds.JoinWar&&Allied(to,from)&&WarStartedBy(r.target,from))Part(lines,ref sum,"You were attacked: the pact's purpose",20);
  if(r.gold>0)Part(lines,ref sum,"Incentive: "+r.gold+" gold when done",Math.Min(40,r.gold*GoldNeed(to)/100/3));
  if(r.endRivalry)Part(lines,ref sum,"You end your rivalry when done",15);
  if(r.consequence>0){int power=PowerLevel(to,from);bool repeated=requests.Any(x=>x!=r&&x.from==from&&x.to==to&&x.consequence>0&&turn-x.created<RequestKinds.Cooldown);
   int fear=repeated?0:power>=4?30:power==3?20:power==2?(r.consequence==RequestConsequence.Rivalry?10:5):-10;
   Part(lines,ref sum,repeated?"Threats repeated too often are ignored":fear<0?"Your threat is not credible":"Fear of your anger ("+Attitudes.Power[power]+")",fear);}
  if(to!=0&&Posture(to)==DiplomacyFlavours.Aggressor&&r.kind==RequestKinds.StopWar)Part(lines,ref sum,"Posture: aggressor",-15);
  return sum;}
 // Who opened the war between a and b, read from the war's record (the first line naming the declaration).
 bool WarStartedBy(int a,int b){var p=Relation(a,b);if(p==null||!AtWar(a,b))return false;return p.lastDeclarer==a;}
 public string DescribeRequest(DiplomaticRequest r){
  string what=r.kind==RequestKinds.Resource?"share its "+(r.resource>0&&r.resource<ResourceNames.Length?ResourceNames[r.resource]:"resource"):r.kind==RequestKinds.StopWar?"make peace with "+NationName(r.target):r.kind==RequestKinds.Withdraw?"keep troops "+(RequestKinds.WithdrawRange+1)+" tiles from "+NationName(r.from)+"'s cities":r.kind==RequestKinds.JoinWar?"declare war on "+NationName(r.target):r.kind==RequestKinds.DefendGate?"defend the gateway at "+GateLabel(r):"send "+r.aid+" gold";
  string s=NationName(r.from)+" asks "+NationName(r.to)+" to "+what+(r.kind==RequestKinds.Aid?"":r.kind==RequestKinds.Resource?" for "+r.duration+" turns":" within "+r.duration+" turns");
  var gives=new List<string>();if(r.gold>0)gives.Add(r.gold+" gold");if(r.endRivalry)gives.Add("an end to the rivalry");
  if(gives.Count>0)s+=", for "+string.Join(" and ",gives.ToArray());if(r.consequence>0)s+=". If not: "+RequestConsequence.Names[r.consequence];return s+".";}
 string GateLabel(DiplomaticRequest r){var g=GateTileOf(r);if(g==null)return "?";var near=cities.Where(c=>At(c.x,c.y).realm==g.realm).OrderBy(c=>Distance(c.x,c.y,g.x,g.y)).FirstOrDefault();return near==null?"("+g.x+", "+g.y+")":"near "+near.name;}
 public DiplomaticRequest ProposeRequest(DiplomaticRequest draft){
  var r=draft.Copy();r.status=RequestStatus.Refused;r.answer=RequestProblem(r);if(r.answer!="")return r;
  r.id=nextDiplomaticId++;r.created=turn;r.status=RequestStatus.Pending;r.served=0;r.unit=-1;requests.Add(r);
  if(r.to==0){r.answer="Awaiting your answer.";AddRivalCall(r.from,0,"asks you to "+RequestKinds.Names[r.kind].ToLower()+" ("+DescribeRequest(r)+")");DiplomacyEvent(r.from,r.to,"Request: "+DescribeRequest(r));return r;}
  int score=RequestScore(r,null);
  if(score>=0)AcceptRequest(r);else RefuseRequest(r,score);
  PruneRequests();return r;}
 void PruneRequests(){if(requests.Count>60)requests.RemoveAll(x=>x.status>=RequestStatus.Fulfilled&&x.created<turn-20);}
 public bool RespondToRequest(int id,bool accept){var r=requests.Find(x=>x.id==id&&x.to==0&&x.status==RequestStatus.Pending);if(r==null||result!="")return false;{int keep=r.status;r.status=RequestStatus.Cancelled;string why=RequestProblem(r);r.status=keep;if(why!=""){Cancel(r,why);return false;}}if(accept)AcceptRequest(r);else RefuseRequest(r,0);return true;}
 void AcceptRequest(DiplomaticRequest r){
  r.status=RequestStatus.Active;r.accepted=turn;r.deadline=turn+r.duration;r.answer="Accepted.";
  DiplomacyEvent(r.from,r.to,"Agreed: "+DescribeRequest(r));
  if(r.from==0)AddRivalCall(r.to,0,"agrees: "+RequestKinds.Names[r.kind].ToLower()+".");
  if(r.kind==RequestKinds.Aid){AddDiplomaticGold(r.to,-r.aid);AddDiplomaticGold(r.from,r.aid);Fulfil(r);return;}
  // Shared through the trade-deal contract, so a lost road or improvement ends it with no blame, like any resource deal.
  if(r.kind==RequestKinds.Resource){contracts.Add(new DiplomaticContract{id=nextDiplomaticId++,from=r.to,to=r.from,start=turn,end=turn+r.duration,giveResource=r.resource});networks=null;Fulfil(r);return;}
  if(r.to!=0)CarryOut(r);
  CheckRequest(r);}
 void RefuseRequest(DiplomaticRequest r,int score){
  // An ally that cannot fight where asked offers what it can instead: a share of its treasury.
  if(r.to!=0&&(r.kind==RequestKinds.JoinWar||r.kind==RequestKinds.DefendGate)&&(Allied(r.to,r.from)||Opinion(r.to,r.from)>=40)){
   int spare=Math.Min(60,Treasury(r.to)/4);if(spare>=10){AddDiplomaticGold(r.to,-spare);AddDiplomaticGold(r.from,spare);AddOpinion(r.from,r.to,OpinionMods.Aid,6);r.status=RequestStatus.Refused;r.answer="Cannot do it: "+string.Join("; ",RequestLines(r).Where(l=>l.Value<0).Select(l=>l.Key).Take(2).ToArray())+". Sent "+spare+" gold instead.";
    DiplomacyEvent(r.from,r.to,NationName(r.to)+" could not "+RequestKinds.Names[r.kind].ToLower()+" and sent "+spare+" gold instead.");return;}}
  r.status=RequestStatus.Refused;var worst=RequestLines(r).Where(l=>l.Value<0).OrderBy(l=>l.Value).Select(l=>l.Key).Take(2).ToArray();r.answer="Refused"+(r.to==0?" by you":worst.Length>0?": "+string.Join("; ",worst):"")+".";
  DiplomacyEvent(r.from,r.to,"Refused: "+DescribeRequest(r));
  if(Allied(r.to,r.from)&&(r.kind==RequestKinds.JoinWar||r.kind==RequestKinds.DefendGate)&&WarStartedBy(r.target,r.from))AddOpinion(r.from,r.to,OpinionMods.Abandoned,-10);
  if(r.consequence>0)Threaten(r,false);}
 // The asker carries out its threat only when it is credible (at least as strong) and only once; otherwise it is remembered.
 void Threaten(DiplomaticRequest r,bool broken){
  if(r.consequence==RequestConsequence.Rivalry&&PowerLevel(r.to,r.from)>=2&&RivalReason(r.from,r.to)=="")DeclareRival(r.from,r.to);
  else AddOpinion(r.from,r.to,OpinionMods.Grievance,broken?-10:-5);}
 void Fulfil(DiplomaticRequest r){
  r.status=RequestStatus.Fulfilled;r.answer="Fulfilled.";
  if(r.gold>0){if(Treasury(r.from)>=r.gold){AddDiplomaticGold(r.from,-r.gold);AddDiplomaticGold(r.to,r.gold);}else{r.answer="Fulfilled, but the promised gold could not be paid.";AddOpinion(r.to,r.from,OpinionMods.Broken,-12);SetTrust(r.to,r.from,Trust(r.to,r.from)-12);}}
  if(r.endRivalry&&Rival(r.from,r.to)){var p=Relation(r.from,r.to);if(r.from==p.a)p.rivalA=-1;else p.rivalB=-1;networks=null;}
  bool help=r.kind==RequestKinds.JoinWar||r.kind==RequestKinds.DefendGate||r.kind==RequestKinds.Aid||r.kind==RequestKinds.Resource;
  AddOpinion(r.from,r.to,OpinionMods.KeptWord,help?15:10);if(help)AddOpinion(r.from,r.to,OpinionMods.Aid,10);
  SetTrust(r.from,r.to,Math.Min(TrustCap(r.from,r.to),Trust(r.from,r.to)+8));
  DiplomacyEvent(r.from,r.to,"Kept their word: "+DescribeRequest(r));
  if(r.to==0)Note(NationName(r.from)+" thanks you: request fulfilled"+(r.gold>0?" (+"+r.gold+" gold)":"")+".");}
 void Fail(DiplomaticRequest r,string why){
  r.status=RequestStatus.Failed;r.answer="Broken: "+why;
  AddOpinion(r.from,r.to,OpinionMods.Broken,-15);SetTrust(r.from,r.to,Trust(r.from,r.to)-15);
  if(Allied(r.from,r.to)&&r.kind!=RequestKinds.Withdraw)AddOpinion(r.from,r.to,OpinionMods.Abandoned,-15);
  if(r.consequence>0)Threaten(r,true);
  DiplomacyEvent(r.from,r.to,"Word broken ("+why+"): "+DescribeRequest(r));
  if(r.to==0)AddRivalCall(r.from,0,"says you broke your word: "+why);}
 void Cancel(DiplomaticRequest r,string why){r.status=RequestStatus.Cancelled;r.answer="Cancelled: "+why;DiplomacyEvent(r.from,r.to,"Request cancelled ("+why+"), no blame on either side.");}
 // A rival recipient carries out what it agreed to at once where it can (peace, war); troop duties are done by DutyErrand.
 void CarryOut(DiplomaticRequest r){
  if(r.kind==RequestKinds.StopWar&&AtWar(r.to,r.target)){MakePeace(r.to,r.target);}
  else if(r.kind==RequestKinds.JoinWar&&WarReason(r.to,r.target)==""){DeclareWar(r.to,r.target,true);}
  else if(r.kind==RequestKinds.DefendGate){var g=GateTileOf(r);var pick=units.Where(u=>u.owner==r.to&&Power[u.type]>0&&!IsNaval(u.type)&&!cities.Any(c=>c.x==u.x&&c.y==u.y&&!units.Any(o=>o!=u&&o.owner==u.owner&&Power[o.type]>0&&o.x==u.x&&o.y==u.y))).OrderBy(u=>g==null?0:Distance(u.x,u.y,g.x,g.y)).FirstOrDefault();if(pick!=null)r.unit=pick.id;}}
 // Checked every turn for each request in force, and once when it is accepted.
 void CheckRequest(DiplomaticRequest r){
  if(r.status!=RequestStatus.Active)return;
  if(Gone(r.from)||Gone(r.to)){Cancel(r,"a party has fallen");return;}
  if(AtWar(r.from,r.to)){Fail(r,"war between the two");return;}
  switch(r.kind){
   case RequestKinds.StopWar:if(Gone(r.target)){Cancel(r,NationName(r.target)+" has fallen");return;}if(!AtWar(r.to,r.target)){Fulfil(r);GuaranteePeace(r);return;}if(turn>=r.deadline)Fail(r,"still at war with "+NationName(r.target));return;
   case RequestKinds.JoinWar:if(Gone(r.target)||!AtWar(r.from,r.target)&&!AtWar(r.to,r.target)){Cancel(r,"the war is over");return;}if(AtWar(r.to,r.target)){Fulfil(r);return;}if(turn>=r.deadline)Fail(r,"did not join the war");return;
   case RequestKinds.Withdraw:{int near=TroopsNear(r.to,r.from,RequestKinds.WithdrawRange);if(near>0&&turn-r.accepted>=RequestKinds.Grace){Fail(r,near+" soldier"+(near==1?"":"s")+" still near the cities");return;}if(turn>=r.deadline)Fulfil(r);return;}
   case RequestKinds.DefendGate:{var g=GateTileOf(r);if(g==null||g.owner!=r.from){Cancel(r,"the gateway changed hands");return;}if(turn>r.accepted&&GuardAt(r.to,g))r.served++;if(turn>=r.deadline){if(r.served*2>=r.duration)Fulfil(r);else Fail(r,"the gateway was left unguarded");}return;}}
 }
 // Peace made at a third party's request comes with that party's guarantee: a truce as long as the request's term.
 void GuaranteePeace(DiplomaticRequest r){var p=Relation(r.to,r.target);if(p!=null)p.peaceUntil=Math.Max(p.peaceUntil,turn+r.duration);}
 internal void RequestsTurn(){
  if(requests==null)requests=new List<DiplomaticRequest>();
  foreach(var r in requests.ToArray()){
   if(r.status==RequestStatus.Pending){if(Gone(r.from)||Gone(r.to))Cancel(r,"a party has fallen");else if(turn-r.created>=RequestKinds.AnswerTurns){RefuseRequest(r,0);r.answer="No answer given: treated as refused.";}continue;}
   if(r.status==RequestStatus.Active){CheckRequest(r);if(r.status==RequestStatus.Active&&r.to==0&&r.deadline-turn==2)Note("Commitment to "+NationName(r.from)+" ends in 2 turns: "+DescribeRequest(r));}}
  PruneRequests();}
 // Attacking a civilisation another has guaranteed (a StopWar it brokered, or a defensive pact) is remembered by the guarantor.
 internal void ProtectedAttacked(int attacker,int victim){
  for(int g=0;g<civCount;g++){if(g==attacker||g==victim||!HasContact(g,attacker))continue;
   bool guards=Allied(g,victim)||requests!=null&&requests.Any(r=>r.from==g&&r.kind==RequestKinds.StopWar&&r.target==victim&&r.to==attacker&&r.status==RequestStatus.Fulfilled&&turn-r.accepted<=r.duration+20);
   if(guards)AddOpinion(g,attacker,OpinionMods.Protected,-20);}}
 // Rival soldiers on duty: keep away from a city they promised to leave alone, or stand guard at an ally's gateway.
 bool DutyErrand(Unit u){
  if(requests==null||Power[u.type]<=0||IsNaval(u.type))return false;
  foreach(var r in requests){if(r.to!=u.owner||r.status!=RequestStatus.Active)continue;
   if(r.kind==RequestKinds.Withdraw&&cities.Any(c=>c.owner==r.from&&Distance(c.x,c.y,u.x,u.y)<=RequestKinds.WithdrawRange)){var home=cities.Where(c=>c.owner==u.owner&&At(c.x,c.y).realm==At(u.x,u.y).realm).OrderBy(c=>Distance(c.x,c.y,u.x,u.y)).FirstOrDefault();if(home!=null){Order(u,home.x,home.y);return true;}}
   if(r.kind==RequestKinds.DefendGate&&r.unit==u.id){var g=GateTileOf(r);if(g==null)continue;if(Distance(u.x,u.y,g.x,g.y)<=1&&!(u.x==g.x&&u.y==g.y)){u.fortified=true;return true;}
    Tile post=null;for(int dy=-1;dy<=1&&post==null;dy++)for(int dx=-1;dx<=1&&post==null;dx++){var t=Step(g.x,g.y,dx,dy);if(t!=null&&t!=g&&!IsImpassable(t.terrain)&&!PixelArt.IsWater(t.terrain)&&CanEnter(u.owner,t))post=t;}
    if(post!=null){Order(u,post.x,post.y);return true;}}}
  return false;}
 // ---------- the rivals ask ----------
 void RequestsAI(int a){
  if(requests==null)return;
  for(int b=0;b<civCount;b++){if(b==a||!HasContact(a,b)||AtWar(a,b)||Gone(b))continue;
   // 1. At war and not winning: ask friends to join, or for gold.
   // Allies and friends are asked first; anyone with its own quarrel with the enemy may be asked to join.
   for(int e=0;e<civCount;e++){if(e==a||e==b||!AtWar(a,e)||Gone(e))continue;bool friend=Allied(b,a)||Opinion(b,a)>=20,quarrel=Threat(b,e)>=30||Rival(b,e)||Opinion(b,e)<=-30;if(!friend&&!quarrel)continue;
    bool losing=Exhaustion(a,e)>=Exhaustion(e,a)||Strength(a,e)>Strength(a,a);if(!losing&&!quarrel)continue;
    var join=new DiplomaticRequest{from=a,to=b,kind=RequestKinds.JoinWar,target=e,duration=5,gold=Math.Min(60,Treasury(a)/3)};if(RequestProblem(join)==""&&(b==0||RequestScore(join,null)>-40)){Ask(join);break;}
    if(!friend||!losing)continue;var aid=new DiplomaticRequest{from=a,to=b,kind=RequestKinds.Aid,aid=Math.Min(50,Math.Max(10,Treasury(b)/5)),duration=5};if(RequestProblem(aid)==""){Ask(aid);break;}}
   // 2. An enemy near one of our gateways: ask an ally to stand guard.
   if(Allied(b,a)||Opinion(b,a)>=50)foreach(var g in AllGates()){if(g.owner!=a)continue;bool menace=units.Any(u=>Power[u.type]>0&&AtWar(a,u.owner)&&Distance(u.x,u.y,g.x,g.y)<=4);if(!menace)continue;var guard=new DiplomaticRequest{from=a,to=b,kind=RequestKinds.DefendGate,gate=g.y*Width+g.x,duration=10,gold=Math.Min(40,Treasury(a)/4)};if(RequestProblem(guard)==""){Ask(guard);break;}}
   // 3. b attacks a friend of ours: tell it to stop.
   for(int f=0;f<civCount;f++){if(f==a||f==b||!AtWar(b,f)||!(Friends(a,f)||Opinion(a,f)>=40)||Friends(a,b))continue;if(!WarStartedBy(b,f))continue;
    bool strong=Strength(a,a)>=Strength(a,b);var stop=new DiplomaticRequest{from=a,to=b,kind=RequestKinds.StopWar,target=f,duration=10,consequence=strong?RequestConsequence.Rivalry:RequestConsequence.Remember,endRivalry=Rival(a,b)};if(RequestProblem(stop)==""){Ask(stop);break;}}
   // 4. A resource our next soldiers need (Iron, Horses) that b has and we lack: ask to share it, paying on delivery.
   if(b!=a&&(Opinion(b,a)>=0||Friends(a,b)))foreach(int res in new[]{2,4}){if(HasResource(a,res)||!HasTech(a,ResourceTech[res])||!HasDomesticResource(b,res))continue;var share=new DiplomaticRequest{from=a,to=b,kind=RequestKinds.Resource,resource=res,duration=15,gold=Math.Min(40,Treasury(a)/4)};if(RequestProblem(share)==""){Ask(share);break;}}
   // 5. b's soldiers crowd our cities in peacetime and we are uneasy.
   if(Threat(a,b)>=20&&TroopsNear(b,a,RequestKinds.WithdrawRange)>=2){var w=new DiplomaticRequest{from=a,to=b,kind=RequestKinds.Withdraw,duration=10,consequence=RequestConsequence.Remember};if(RequestProblem(w)=="")Ask(w);}
  }}
 // A rival only asks what the recipient might plausibly grant (the player is always asked; the answer is theirs).
 void Ask(DiplomaticRequest r){if(r.to!=0&&RequestScore(r,null)<-60)return;ProposeRequest(r);}
 // A leader refused a resource in the last 20 turns values it more in deals (DiplomacyDepth.ResourceNeed).
 internal bool RefusedResource(int owner,int r){return requests!=null&&requests.Any(x=>x.from==owner&&x.kind==RequestKinds.Resource&&x.resource==r&&x.status==RequestStatus.Refused&&turn-x.created<=20);}
 internal bool RequestsValid(){if(requests==null)return false;var ids=new HashSet<int>();foreach(var r in requests)if(r==null||r.id<1||r.id>=nextDiplomaticId||!ids.Add(r.id)||r.from<0||r.to<0||r.from>=civCount||r.to>=civCount||r.from==r.to||r.kind<0||r.kind>=RequestKinds.Count||r.status<0||r.status>RequestStatus.Cancelled||r.duration<5||r.duration>30||r.answer==null||r.resource<0||r.resource>=ResourceNames.Length)return false;return true;}
}
}
