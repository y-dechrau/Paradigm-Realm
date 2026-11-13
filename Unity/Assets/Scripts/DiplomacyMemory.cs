using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Diplomatic memory, gate access, the rivals' behaviour profile and the explanation view (owner brief, 13 Nov 2026; save
// version 43; Design/DIPLOMACY-STRATEGY-2026-11-13.md).
//  - Memory: four more marks (helped us, kept their word, abandoned us, attacked those we protect), each fading at its own pace
//    (OpinionMods.FadeEvery). Gifts never add more than +20 in all. A long friendship adds up to +15 and keeps a relationship
//    steady; a partner who has kept its word is trusted ten points beyond the ceiling of its agreements.
//  - Gate access: a sixth agreement. Units may cross land within 2 tiles of the partner's gateways and pass through them.
//  - Behaviour profile: World simulation (rivals seek survival, prosperity and their own grudges) or Competitive (they also gang
//    up on whoever is close to winning). Profiles change motives only; difficulty bonuses are separate (Difficulty.cs).
public static class AiProfiles {
 public const int WorldSimulation=0,Competitive=1;
 public static readonly string[] Names={"World simulation","Competitive"};
 public static readonly string[] Texts={
  "Rivals pursue survival, prosperity, their agendas and their grudges. They judge you as they judge each other.",
  "As above, and rivals also turn on whoever is close to winning (a Transcendence stage, a faith near victory or many capitals held), you included."};
}
public partial class World {
 // Save version 43: 0 world simulation (older saves too), 1 competitive. Chosen on the New Game screen.
 public int aiProfile;

 // Save version 43: agreements and marks gained a slot each kind; requests are new; the profile is world simulation.
 internal void UpgradeDiplomacy43(){
  if(requests==null)requests=new List<DiplomaticRequest>();if(diplomacy==null)return;
  foreach(var p in diplomacy){if(p==null)continue;p.pactSince=Grow(p.pactSince==null?new int[0]:p.pactSince,Pacts.Count);p.modsA=Grow(p.modsA==null?new int[0]:p.modsA,OpinionMods.Count);p.modsB=Grow(p.modsB==null?new int[0]:p.modsB,OpinionMods.Count);p.lastDeclarer=-1;}
  if(offers!=null)foreach(var o in offers)if(o!=null&&o.giveCity==0&&o.askCity==0){o.giveCity=-1;o.askCity=-1;}}
 public static bool ValidKind(TreatyKind k){int i=(int)k;return i>=0&&i<(int)TreatyKind.Retired||k==TreatyKind.GateAccess;}

 // ---------- memory ----------
 // Years of standing agreements make a relationship steady: +1 for every 8 turns of the oldest agreement, at most +15.
 public int LongFriendship(int o,int t){var p=Relation(o,t);if(p==null||AtWar(o,t)||p.pacts==0)return 0;int age=0;for(int k=0;k<Pacts.Count;k++)if((p.pacts&(1<<k))!=0)age=Math.Max(age,turn-p.pactSince[k]);return Math.Min(15,age/8);}
 public sealed class MemoryLine { public string text; public int value,turnsLeft; }
 // Every mark the observer holds of the other, with how long it will take to fade away (assuming nothing new happens).
 public List<MemoryLine> MemoryLines(int observer,int other){
  var list=new List<MemoryLine>();var p=Relation(observer,other);if(p==null)return list;var m=Mods(observer,other);bool forgiving=Flavour(observer,DiplomacyFlavours.Forgiveness)>=7;
  for(int k=0;k<m.Length;k++){if(m[k]==0)continue;int every=OpinionMods.FadeEvery[k];if(m[k]<0&&forgiving)every=Math.Max(1,every/2);
   int left=k==OpinionMods.Envoy&&Envoy(other,observer)?-1:Math.Abs(m[k])*every;list.Add(new MemoryLine{text=OpinionMods.Names[k],value=m[k],turnsLeft=left});}
  return list.OrderBy(l=>l.value).ToList();}
 // What the player could do now to improve the other leader's view of them, in order of how much it would help.
 public List<string> ImproveTips(int leader,int you){
  var tips=new List<string>();if(!HasContact(leader,you))return tips;var m=Mods(leader,you);
  if(AtWar(leader,you)){tips.Add("Offer peace with terms they can accept: reparations or a withdrawal make it easier.");return tips;}
  if(Rival(you,leader))tips.Add("End your rivalry with them ("+(EndRivalReason(you,leader)==""?"possible now":EndRivalReason(you,leader))+").");
  if(ClaimsOn(you,leader).Count>0)tips.Add("Drop your claims on their cities (-10 each while they stand).");
  if(!Envoy(you,leader)&&EnvoyReason(you,leader)=="")tips.Add("Send an envoy: +1 a turn up to +"+Pacts.EnvoyCap+", and their fear of you fades faster.");
  if(Threat(leader,you)>=30)tips.Add("Their fear of you ("+Threat(leader,you)+") costs "+Threat(leader,you)/2+": keep troops away from their cities and avoid war without cause.");
  if(m[OpinionMods.Broken]<0||m[OpinionMods.Abandoned]<0)tips.Add("Keep your word: fulfil a request of theirs to earn \"Kept their word\" (+10, fades slowly) and raise their trust ceiling.");
  if(Relation(leader,you).pacts==0&&!Rivals(leader,you))tips.Add("Open borders is the first step: it lets trust grow to "+Pacts.TrustCap[Pacts.Borders]+".");
  if(m[OpinionMods.Gifts]<OpinionMods.GiftCap&&Opinion(leader,you)<20)tips.Add("A gift helps a little: gifts add at most +"+OpinionMods.GiftCap+" in all, each worth less than the last.");
  tips.Add("Help them when asked: accepting and fulfilling their requests for aid or defence is remembered longest.");
  return tips;}
 // Called whenever a defensive pact is ended: unfinished defence commitments between the two are abandoned.
 void AbandonCommitmentsOnBreak(int a,int b){if(requests==null)return;foreach(var r in requests)if(r.status==RequestStatus.Active&&SamePair(r.from,r.to,a,b)&&(r.kind==RequestKinds.DefendGate||r.kind==RequestKinds.JoinWar)){if(r.to==a){r.status=RequestStatus.Failed;r.answer="Broken: abandoned when "+NationName(a)+" ended the defensive pact.";AddOpinion(r.from,a,OpinionMods.Abandoned,-15);}else Cancel(r,NationName(a)+" ended the defensive pact");}}

 // ---------- gate access ----------
 [NonSerialized] HashSet<int> gateApproaches;[NonSerialized] int gateApproachTurn=-1;
 // Land within 2 tiles of any gateway (either end). Gateways never move once placed; kept for the turn.
 bool NearAnyGate(Tile t){
  if(gateApproaches==null||gateApproachTurn!=turn){gateApproachTurn=turn;gateApproaches=new HashSet<int>();foreach(var g in AllGates())for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){var a=StepIn(g.x,g.y,dx,dy,2);if(a!=null&&Distance(g.x,g.y,a.x,a.y)<=2)gateApproaches.Add(a.y*Width+a.x);}}
  return gateApproaches.Contains(t.y*Width+t.x);}
 public bool GateAccessTile(int owner,Tile t){if(t==null||t.owner<0||owner<0||owner>=civCount||t.owner>=civCount||t.owner==owner)return false;if(!Pact(owner,t.owner,Pacts.GateAccess))return false;return NearAnyGate(t);}
 public bool OwnsGate(int owner){foreach(var g in AllGates())if(g.owner==owner)return true;return false;}
 // Part of PactScore: what gate access is worth to the decider.
 int GateInterest(int decider,int asker,List<KeyValuePair<string,int>> lines){
  int sum=0;bool theirs=OwnsGate(asker),mine=OwnsGate(decider);
  if(theirs&&!mine)Part(lines,ref sum,"Passage to a gateway we lack",20);else if(theirs)Part(lines,ref sum,"Passage to more gateways",5);
  if(mine&&Threat(decider,asker)>=30)Part(lines,ref sum,"Wary of your troops at our gateways",-20);
  if(dimensionRules&&mine&&colonies!=null&&colonies.Any(c=>c.owner==asker))Part(lines,ref sum,"You already hold land beyond the gates",-5);
  return sum;}

 // ---------- behaviour profile ----------
 // How close a civilisation looks to winning, 0..100: its Transcendence project, its faith's progress, or capitals held.
 public int VictoryThreat(int b){
  if(b<0||b>=civCount)return 0;int v=(int)(ProjectProgress(b)*100);
  if(modernRules){string line;v=Math.Max(v,(int)(ReligiousProgress(b,out line)*100));}
  int rivals=0,held=0;for(int o=0;o<civCount;o++){if(o==b||IsNativeNation(o))continue;rivals++;if(Conquered(b,o))held++;}
  if(rivals>0)v=Math.Max(v,held*100/rivals);return Math.Min(100,v);}
 // Competitive rivals turn on a civilisation once it is halfway to a win; world-simulation rivals do not.
 public int CompetitiveAmbition(int a,int b){if(aiProfile!=AiProfiles.Competitive||a==b||IsNativeNation(a))return 0;int v=VictoryThreat(b);return v>=50?25+(v-50)/2:0;}

 // ---------- "What would make this acceptable?" ----------
 // The least gold now (in steps of 25, up to what the proposer holds) that turns a refusal into acceptance, or -1 if none does.
 // Only gold is added: nothing else of the deal changes, so the answer cannot be gamed one unit at a time.
 public int GoldToAccept(DiplomaticOffer o){if(OfferProblem(o)!="")return -1;if(OfferWillingness(o)>=0)return 0;var c=o.Copy();int start=c.giveGold;
  for(int add=25;start+add<=Treasury(o.from)&&add<=1000;add+=25){c.giveGold=start+add;if(OfferWillingness(c)>=0)return add;}return -1;}
 public int IncentiveToAccept(DiplomaticRequest r){if(RequestProblem(r)!="")return -1;if(RequestScore(r,null)>=0)return 0;var c=r.Copy();int start=c.gold;
  for(int add=25;start+add<=Treasury(r.from)&&add<=1000;add+=25){c.gold=start+add;if(RequestScore(c,null)>=0)return add;}return -1;}
 // Everything the player has promised and been promised, soonest first (the Diplomacy > More summary).
 public List<string> CommitmentSummary(int owner){
  var list=new List<KeyValuePair<int,string>>();
  if(requests!=null)foreach(var r in requests)if(r.status==RequestStatus.Active&&(r.to==owner||r.from==owner))list.Add(new KeyValuePair<int,string>(r.deadline-turn,(r.to==owner?"You promised: ":"Promised to you: ")+DescribeRequest(r)+" "+(r.deadline-turn)+" turns left"+(r.kind==RequestKinds.DefendGate?" (guarded "+r.served+" of "+r.duration+" turns so far)":"")+"."));
  foreach(var c in contracts)if(c.from==owner||c.to==owner)list.Add(new KeyValuePair<int,string>(c.end-turn,"Trade deal with "+NationName(c.from==owner?c.to:c.from)+": "+(c.end-turn)+" turns left"+(ContractActive(c)?"":" (suspended)")+"."));
  foreach(var p in diplomacy){if(p.a!=owner&&p.b!=owner)continue;int other=p.a==owner?p.b:p.a;if(p.peaceUntil>turn&&!AtWar(owner,other))list.Add(new KeyValuePair<int,string>(p.peaceUntil-turn,"Truce with "+NationName(other)+": no war for "+(p.peaceUntil-turn)+" turns."));}
  return list.OrderBy(k=>k.Key).Select(k=>k.Value).ToList();}
 // ---------- the explanation view (Diplomacy > More > How they see you) ----------
 public List<string> AiExplanation(int leader,int you){
  var lines=new List<string>();if(!HasContact(leader,you))return lines;
  int amb=Ambition(leader,you);lines.Add("Designs on your land: "+amb+(amb>=50?" (they want it: expect rivalry, claims and war)":amb>=35?" (they are tempted)":" (none worth acting on)")+".");
  lines.Add("Posture: "+DiplomacyFlavours.PostureNames[Posture(leader)]+". Relative strength as they see it: you are "+Attitudes.Power[PowerLevel(leader,you)]+".");
  int comp=CompetitiveAmbition(leader,you);if(comp>0)lines.Add("Competitive profile: you are close to winning (+"+comp+" to their designs).");
  for(int k=0;k<Pacts.Count;k++){if(Pact(leader,you,k))continue;var o=new DiplomaticOffer{from=you,to=leader,kind=Pacts.Kind[k]};string why=OfferProblem(o);if(why!="")continue;int w=OfferWillingness(o);if(w>=0){lines.Add("They would sign a "+Pacts.Names[k].ToLower()+" now (+"+w+").");break;}}
  {var pend=requests==null?null:requests.Where(r=>r.to==you&&r.from==leader&&r.status==RequestStatus.Active).ToList();if(pend!=null)foreach(var r in pend)lines.Add("They hold you to: "+DescribeRequest(r)+" ("+(r.deadline-turn)+" turns left).");}
  if(AtWar(leader,you)){var peace=new DiplomaticOffer{from=you,to=leader,kind=TreatyKind.Peace};lines.Add("Peace now: "+(OfferWillingness(peace)>=0?"they would accept":"they would refuse")+" ("+OfferWillingness(peace)+"); your leverage "+Leverage(you,leader)+".");}
  return lines;}
}
}
