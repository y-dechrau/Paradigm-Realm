using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Proposals, the reasons a leader accepts or refuses, trade deals, the turn, the rivals' diplomacy and the upgrade of older saves
// (see Diplomacy.cs for the design).
public partial class World {
 bool Recurring(DiplomaticOffer o){return o.givePerTurn+o.askPerTurn+o.giveResource+o.askResource>0;}
 static string PactTech(int k){return k==Pacts.Research?"writing":k==Pacts.Defensive?"laws":null;}
 public string OfferProblem(DiplomaticOffer o){
  if(o==null||!HasContact(o.from,o.to))return "First contact required.";
  if(result!="")return "The game has ended.";
  if(!ValidKind(o.kind)||o.duration<5||o.duration>30||o.giveGold<0||o.askGold<0||o.giveGold>10000||o.askGold>10000||o.givePerTurn<0||o.askPerTurn<0||o.givePerTurn>20||o.askPerTurn>20||o.giveResource<0||o.giveResource>5||o.askResource<0||o.askResource>5)return "Invalid deal terms.";
  bool warNow=AtWar(o.from,o.to);
  if(warNow&&o.kind!=TreatyKind.Peace)return "Negotiate peace first.";
  if(!warNow&&o.kind==TreatyKind.Peace)return "Already at peace.";
  if(Recurring(o)&&o.kind!=TreatyKind.Trade&&o.kind!=TreatyKind.Peace)return "Payments by the turn and resources belong in a trade deal.";
  if(Rivals(o.from,o.to)&&o.kind!=TreatyKind.Peace)return "No dealings between rivals.";
  if(Treasury(o.from)<o.giveGold||Treasury(o.to)<o.askGold)return "Insufficient treasury for upfront terms.";
  if(o.giveResource>0&&!HasDomesticResource(o.from,o.giveResource)||o.askResource>0&&!HasDomesticResource(o.to,o.askResource))return "Resources must be domestically improved and connected to the capital; imports cannot be exported.";
  if(o.giveResource>0&&o.giveResource==o.askResource)return "Choose different resources.";
  if(Recurring(o)&&contracts.Any(c=>SamePair(c.from,c.to,o.from,o.to)))return "End the existing trade deal before replacing it.";
  if(o.kind==TreatyKind.Trade&&!Recurring(o)&&o.giveGold+o.askGold==0)return "Add a payment or resource to the trade.";
  int k=Pacts.Of(o.kind);
  if(k>=0){if(Pact(o.from,o.to,k))return Pacts.Names[k]+" already in force.";string tech=PactTech(k);if(tech!=null&&(!HasTech(o.from,tech)||!HasTech(o.to,tech)))return "Both sides need "+(tech=="writing"?"Writing":"Code of Laws")+".";
   if(k==Pacts.GateAccess&&!OwnsGate(o.from)&&!OwnsGate(o.to))return "Neither of you holds a gateway.";}
  {string terms=PeaceTermsProblem(o);if(terms!="")return terms;}
  return "";
 }
 public int DealValue(DiplomaticOffer o){
  if(!modernRules)return o.giveGold-o.askGold+(o.givePerTurn-o.askPerTurn)*o.duration*3/4+(o.giveResource>0?(HasResource(o.to,o.giveResource)?12:60):0)-(o.askResource>0?40:0);
  // Utility: gold and resources are weighed by what the receiving leader needs now (DiplomacyDepth.cs).
  int g=GoldNeed(o.to);int gold=(o.giveGold+o.givePerTurn*o.duration*3/4-o.askGold-o.askPerTurn*o.duration*3/4)*g/100;
  int given=o.giveResource>0?(HasResource(o.to,o.giveResource)?12:60)*ResourceNeed(o.to,o.giveResource)/100:0,asked=o.askResource>0?40*ResourceNeed(o.to,o.askResource)/100:0;
  return gold+given-asked;}
 // How the decider weighs a standing agreement asked for by the other: every reason, by name. The same reckoning decides whether
 // a rival wants to propose one itself.
 int PactScore(int decider,int asker,int k,List<KeyValuePair<string,int>> lines){
  int sum=0;int att=AttitudeNow(decider,asker);bool bond=k==Pacts.NonAggression||k==Pacts.Defensive;
  Part(lines,ref sum,k==Pacts.Borders?"Borders are easily opened":"Reluctance to sign a "+Pacts.Names[k].ToLower(),Pacts.Reluctance[k]);
  Part(lines,ref sum,"Opinion of you",Opinion(decider,asker));
  Part(lines,ref sum,"Trust in you",Trust(decider,asker)/2);
  int a=0;
  switch(att){case Attitudes.Hostile:case Attitudes.Rival:a=-100;break;case Attitudes.Unfriendly:a=-20;break;case Attitudes.Wary:a=-10;break;case Attitudes.Domineering:a=-30;break;case Attitudes.Friendly:case Attitudes.Loyal:a=10;break;
   case Attitudes.Threatened:a=k==Pacts.NonAggression?25:k==Pacts.Defensive?-100:0;break;}
  Part(lines,ref sum,"Attitude: "+Attitudes.Names[att],a);
  if(Rival(asker,decider))Part(lines,ref sum,"You named them a rival",-100);
  if(bond){
   bool shared=false;for(int x=0;x<civCount&&!shared;x++)if(x!=decider&&x!=asker&&(Threat(decider,x)>=40&&(Threat(asker,x)>=40||AtWar(asker,x))||AtWar(decider,x)&&AtWar(asker,x)))shared=true;
   if(shared)Part(lines,ref sum,"We face the same danger",20);
   if(decider!=0&&Ambition(decider,asker)>=50)Part(lines,ref sum,"Has designs on your land",-60);}
  if(k==Pacts.GateAccess)sum+=GateInterest(decider,asker,lines);
  if(k==Pacts.Defensive){int power=PowerLevel(decider,asker);Part(lines,ref sum,"Your strength: "+Attitudes.Power[power],power==0?-20:power==1?-10:power==3?10:power==4?15:0);}
  if(decider!=0){int p=Posture(decider);Part(lines,ref sum,"Posture: "+DiplomacyFlavours.PostureNames[p],p==DiplomacyFlavours.Builder&&!bond&&k!=Pacts.Borders||p==DiplomacyFlavours.Builder&&k==Pacts.NonAggression?5:p==DiplomacyFlavours.Aggressor&&bond?-10:p==DiplomacyFlavours.Defensive&&k==Pacts.Defensive?10:0);}
  return sum;}
 // Every reason the recipient weighs, by name; the offer is accepted when they add up to 0 or more.
 public List<KeyValuePair<string,int>> AcceptanceLines(DiplomaticOffer o){var lines=new List<KeyValuePair<string,int>>();Acceptance(o,lines);return lines;}
 public int OfferWillingness(DiplomaticOffer o){return Acceptance(o,null);}
 int Acceptance(DiplomaticOffer o,List<KeyValuePair<string,int>> lines){
  int sum=0,value=DealValue(o);
  if(o.kind==TreatyKind.Trade){Part(lines,ref sum,"What the terms are worth to them",value);Part(lines,ref sum,"Goodwill",Math.Max(0,Opinion(o.to,o.from))/5);return sum;}
  if(o.kind==TreatyKind.Peace){
   var p=Relation(o.from,o.to);int age=turn-p.warStarted,mine=Exhaustion(o.to,o.from),yours=Exhaustion(o.from,o.to);
   Part(lines,ref sum,"Will to fight on",-40);
   Part(lines,ref sum,"Their war exhaustion ("+mine+")",mine);
   Part(lines,ref sum,"Your war exhaustion ("+yours+")",-yours/2);
   if(age<6)Part(lines,ref sum,"The war has only begun",-100);
   if(mine>=100)Part(lines,ref sum,"Their people demand peace",200);
   else if(ActiveRival(o.to)&&Strength(o.to,o.to)*2>=Strength(o.to,o.from)*3)Part(lines,ref sum,"They believe they are winning",-30);
   else if(Strength(o.to,o.from)*2>=Strength(o.to,o.to)*3)Part(lines,ref sum,"They fear they are losing",30);
   if(!ActiveRival(o.to)&&o.to!=0)Part(lines,ref sum,"No wish for this war",60);
   Part(lines,ref sum,"The terms offered",Clamp(value/3,-60,60));PeaceTermLines(o,lines,ref sum);return sum;}
  int k=Pacts.Of(o.kind);if(k<0)return -1000;
  sum=PactScore(o.to,o.from,k,lines);
  Part(lines,ref sum,"The terms offered",Clamp(value/5,-40,40));
  return sum;}
 public string OfferAssessment(DiplomaticOffer o){string problem=OfferProblem(o);if(problem!="")return problem;int w=OfferWillingness(o);return (w>=0?"They would accept (+"+w+").":"They would refuse ("+w+").")+(modernRules?NeedText(o.to):"");}
 public DiplomaticOffer Propose(DiplomaticOffer draft){var o=draft.Copy();o.status=2;o.answer=OfferProblem(o);if(o.answer!="")return o;var p=Relation(o.from,o.to);if((o.from==p.a?p.lastA:p.lastB)==turn||offers.Any(x=>x.status==0&&SamePair(x.from,x.to,o.from,o.to))){o.answer="One proposal per turn, with only one pending proposal per pair.";return o;}if(o.from==p.a)p.lastA=turn;else p.lastB=turn;o.id=nextDiplomaticId++;o.created=turn;o.expires=turn+3;o.status=0;offers.Add(o);
  if(o.to==0){AddRivalCall(o.from,o.id,CallText(o));o.answer="Awaiting your response.";DiplomacyEvent(o.from,o.to,"New diplomatic proposal. Respond within three turns.");}
  else if(OfferWillingness(o)>=0)AcceptOffer(o);
  else{int w=OfferWillingness(o);o.status=2;o.answer="Declined ("+w+").";DiplomacyEvent(o.from,o.to,OfferName(o.kind)+" declined.");if(o.kind==TreatyKind.Trade&&o.from==0){var counter=new DiplomaticOffer{from=o.to,to=0,kind=o.kind,duration=o.duration,giveGold=o.askGold,askGold=o.giveGold-w+5,givePerTurn=o.askPerTurn,askPerTurn=o.givePerTurn,giveResource=o.askResource,askResource=o.giveResource};if(OfferProblem(counter)==""){counter.id=nextDiplomaticId++;counter.created=turn;counter.expires=turn+3;counter.answer="Counteroffer: revised upfront payment.";offers.Add(counter);AddRivalCall(counter.from,counter.id,"makes a counteroffer.");DiplomacyEvent(o.to,0,"A counteroffer is waiting in your inbox.");}}}
  offers.RemoveAll(x=>x.status!=0&&x.created<turn-10);return o;
 }
 public static string OfferName(TreatyKind kind){int k=Pacts.Of(kind);return k>=0?Pacts.Names[k]:kind==TreatyKind.Trade?"Trade deal":kind==TreatyKind.Peace?"Peace":"Proposal";}
 bool AcceptOffer(DiplomaticOffer o){string reason=OfferProblem(o);if(reason!=""){o.status=2;o.answer=reason;return false;}AddDiplomaticGold(o.from,o.askGold-o.giveGold);AddDiplomaticGold(o.to,o.giveGold-o.askGold);int until=turn+o.duration;
  int k=Pacts.Of(o.kind);if(k>=0)SetPact(o.from,o.to,k,true);else if(o.kind==TreatyKind.Peace){MakePeace(o.from,o.to);ApplyPeaceTerms(o);}
  if(Recurring(o))contracts.Add(new DiplomaticContract{id=nextDiplomaticId++,from=o.from,to=o.to,start=turn,end=until,givePerTurn=o.givePerTurn,askPerTurn=o.askPerTurn,giveResource=o.giveResource,askResource=o.askResource});
  if(k>=0){AddOpinion(o.from,o.to,OpinionMods.Dealings,3);AddOpinion(o.to,o.from,OpinionMods.Dealings,3);}o.status=1;o.answer="Accepted.";DiplomacyEvent(o.from,o.to,OfferName(o.kind)+" agreed.");return true;
 }
 public bool RespondToOffer(int id,bool accept){var o=offers.Find(x=>x.id==id&&x.to==0&&x.status==0);if(o==null||result!="")return false;if(turn>=o.expires){o.status=3;o.answer="Expired.";return false;}if(accept)return AcceptOffer(o);o.status=2;o.answer="Declined by player.";DiplomacyEvent(o.from,o.to,"Proposal declined.");return true;}
 public string DescribeOffer(DiplomaticOffer o){
  int k=Pacts.Of(o.kind);string s=NationName(o.from)+" proposes "+(k>=0?"a "+Pacts.Names[k].ToLower():o.kind==TreatyKind.Peace?"peace":"a trade deal")+" to "+NationName(o.to)+".";
  if(k>=0)s+=" "+Pacts.Effects[k]+" It lasts until one side ends it.";
  if(o.kind==TreatyKind.Peace)s+=" Terms: "+PeaceTermsText(o);
  var mine=new List<string>();var theirs=new List<string>();
  if(o.giveGold>0)mine.Add(o.giveGold+" gold now");if(o.givePerTurn>0)mine.Add(o.givePerTurn+" gold a turn");if(o.giveResource>0)mine.Add(ResourceNames[o.giveResource]);
  if(o.askGold>0)theirs.Add(o.askGold+" gold now");if(o.askPerTurn>0)theirs.Add(o.askPerTurn+" gold a turn");if(o.askResource>0)theirs.Add(ResourceNames[o.askResource]);
  if(mine.Count>0)s+=" "+NationName(o.from)+" gives "+string.Join(", ",mine.ToArray())+".";
  if(theirs.Count>0)s+=" "+NationName(o.to)+" gives "+string.Join(", ",theirs.ToArray())+".";
  if(Recurring(o))s+=" Payments and resources run for "+o.duration+" turns; a shared resource is not lost by its supplier.";
  return s;}
 public bool ContractActive(DiplomaticContract c){return c.start<=turn&&turn<c.end&&!AtWar(c.from,c.to)&&!Embargoed(c.from,c.to)&&(c.giveResource==0||HasDomesticResource(c.from,c.giveResource))&&(c.askResource==0||HasDomesticResource(c.to,c.askResource));}
 public bool ImportedResource(int owner,int resource){return resource>0&&contracts.Any(c=>ContractActive(c)&&(c.to==owner&&c.giveResource==resource||c.from==owner&&c.askResource==resource));}
 public int DiplomaticIncome(int owner){return contracts.Where(ContractActive).Sum(c=>c.to==owner?c.givePerTurn:c.from==owner?c.askPerTurn:0)+CommercialGold(owner);}
 public int DiplomaticExpense(int owner){return contracts.Where(ContractActive).Sum(c=>c.from==owner?c.givePerTurn:c.to==owner?c.askPerTurn:0);}
 public int DiplomaticNet(int owner){return DiplomaticIncome(owner)-DiplomaticExpense(owner);}
 // Budget each payer before incoming money. No cyclic credit or negative treasury creates gold.
 public void ProcessDiplomacyPayments(){foreach(var c in contracts.ToArray())if(!ContractActive(c)){contracts.Remove(c);DiplomacyEvent(c.from,c.to,"Trade ended: duration, war, rivalry or resource connection changed.");}var insolvent=Enumerable.Range(0,civCount).Where(i=>Treasury(i)<DiplomaticExpense(i)).ToArray();foreach(var c in contracts.ToArray()){int offender=insolvent.Contains(c.from)&&c.givePerTurn>0?c.from:insolvent.Contains(c.to)&&c.askPerTurn>0?c.to:-1;if(offender<0)continue;contracts.Remove(c);int victim=offender==c.from?c.to:c.from;AddOpinion(victim,offender,OpinionMods.Broken,-10);SetTrust(victim,offender,Trust(victim,offender)-10);DiplomacyEvent(c.from,c.to,"Payment default: the trade deal is cancelled and trust damaged.");}int[] net=Enumerable.Range(0,civCount).Select(DiplomaticNet).ToArray();for(int i=0;i<civCount;i++)AddDiplomaticGold(i,net[i]);}

 // ---------- the turn ----------
 // Marks fade: wrongs by 1 every second turn (conquest and broken words every fourth; twice as fast for a forgiving leader),
 // favours by 1 every third turn. An envoy's work is kept up while the envoy stays.
 void FadeMarks(int[] m,int observer,bool envoyPresent){
  bool forgiving=Flavour(observer,DiplomacyFlavours.Forgiveness)>=7;
  for(int k=0;k<m.Length;k++){
   if(k==OpinionMods.Envoy){if(envoyPresent){if(m[k]<Pacts.EnvoyCap)m[k]++;}else if(m[k]>0)m[k]--;else if(m[k]<0)m[k]++;continue;}
   int every=OpinionMods.FadeEvery[k];
   if(m[k]<0){if(forgiving)every/=2;if(every<=1||turn%every==0)m[k]++;}
   else if(m[k]>0&&turn%every==0)m[k]--;}}
 public void AdvanceDiplomacy(){
  DiplomacyDepthTurn();
  foreach(var p in diplomacy){
   bool war=AtWar(p.a,p.b);
   if(war){p.exhaustA=Math.Min(100,p.exhaustA+1);p.exhaustB=Math.Min(100,p.exhaustB+1);p.trustA=Math.Max(0,p.trustA-2);p.trustB=Math.Max(0,p.trustB-2);p.envoyA=p.envoyB=0;}
   else{
    FadeMarks(p.modsA,p.a,p.envoyB==1);FadeMarks(p.modsB,p.b,p.envoyA==1);
    // Fear fades by 1 a turn, by 2 where the feared side keeps an envoy.
    if(p.alarmA>0)p.alarmA=Math.Max(0,p.alarmA-1-(p.envoyB==1?1:0));if(p.alarmB>0)p.alarmB=Math.Max(0,p.alarmB-1-(p.envoyA==1?1:0));
    int capA=TrustCap(p.a,p.b),capB=TrustCap(p.b,p.a);bool grow=(turn+p.a+p.b)%2==0;
    if(p.trustA>capA)p.trustA--;else if(p.trustA<capA&&grow)p.trustA++;
    if(p.trustB>capB)p.trustB--;else if(p.trustB<capB&&grow)p.trustB++;
   }}
  // A claim ends when its city is gone or belongs to the claimant.
  claims.RemoveAll(c=>{var city=CityById(c.city);return city==null||city.owner==c.owner;});
  foreach(var c in contracts.Where(c=>c.end<=turn).ToArray()){contracts.Remove(c);DiplomacyEvent(c.from,c.to,"Trade deal completed.");}
  foreach(var o in offers.Where(o=>o.status==0&&o.expires<=turn)){o.status=3;o.answer="Expired.";}
  RequestsTurn();
 }

 // ---------- the rivals' diplomacy ----------
 // The city of b that a would claim first: the nearest one it knows to any city of its own.
 City ClaimTarget(int a,int b){City best=null;int bestD=int.MaxValue;var mine=cities.Where(c=>c.owner==a).ToList();if(mine.Count==0)return null;foreach(var c in cities){if(c.owner!=b||HasClaim(a,c.id)||!KnowsCity(a,c))continue;int d=mine.Min(m=>Distance(m.x,m.y,c.x,c.y));if(d<bestD||d==bestD&&best!=null&&c.id<best.id){bestD=d;best=c;}}return best;}
 public void DiplomacyAI(){
  // Each pair looks for the other one turn in three, through lists sorted by owner: with sixty peoples this was a tenth of a turn.
  {var unitsOf=new List<Unit>[civCount];var citiesOf=new List<City>[civCount];for(int o=0;o<civCount;o++){unitsOf[o]=new List<Unit>();citiesOf[o]=new List<City>();}foreach(var u in units)if(u.owner>=0&&u.owner<civCount)unitsOf[u.owner].Add(u);foreach(var c in cities)citiesOf[c.owner].Add(c);
   foreach(var pair in diplomacy){if(pair.a==0||pair.contact||(pair.a+pair.b+turn)%3!=0||SealedApart(pair.a,pair.b))continue;bool seen=false;foreach(var u in unitsOf[pair.b])if(VisibleTo(pair.a,u.x,u.y)){seen=true;break;}if(!seen)foreach(var c in citiesOf[pair.b])if(VisibleTo(pair.a,c.x,c.y)){seen=true;break;}if(!seen)foreach(var u in unitsOf[pair.a])if(VisibleTo(pair.b,u.x,u.y)){seen=true;break;}if(seen)pair.contact=true;}}
  for(int a=1;a<civCount;a++){if(turn%5!=a%5||!cities.Any(c=>c.owner==a))continue;
   for(int b=0;b<civCount;b++){if(!HasContact(a,b)||offers.Any(o=>o.status==0&&SamePair(o.from,o.to,a,b)))continue;var p=Relation(a,b);DiplomaticOffer offer=null;
    if(AtWar(a,b)){if(RivalWantsPeace(a,b))offer=AIPeaceOffer(a,b);}
    else{
     int opinion=Opinion(a,b);bool covets=Ambition(a,b)>=50,hates=opinion<=-50;
     if(covets||hates){
      // The road to war is walked in the open: end what binds them, name the rival, claim a city, then strike with a cause.
      bool bound=false;for(int k=Pacts.Count-1;k>=0;k--)if(Pact(a,b,k)){bound=true;if(PactEndReason(a,b,k)=="")EndTreaty(a,b,Pacts.Kind[k]);}
      if(bound||Friends(a,b))continue;
      if(!Rival(a,b)&&RivalReason(a,b)=="")DeclareRival(a,b);
      if(covets){var target=ClaimTarget(a,b);if(target!=null&&ClaimReason(a,target)=="")MakeClaim(a,target);}
      bool strong=covets||Strength(a,a)*2>Strength(a,b)*3;
      if(strong&&p.peaceUntil<=turn&&CasusBelli(a,b)!=""){if(DeclareWar(a,b))DiplomacyEvent(a,b,nations[b].projectStage>=1||nations[b].projectCity>=0?"War to stop the Transcendence project.":covets?"War for land and power.":"War between old enemies.");}
      continue;}
     if(Rival(a,b)&&opinion>=-10&&EndRivalReason(a,b)=="")EndRivalry(a,b);
     if(Rivals(a,b))continue;
     // Otherwise the next agreement this leader itself wants, lowest first.
     for(int k=0;k<Pacts.Count&&offer==null;k++){if(Pact(a,b,k))continue;var o=new DiplomaticOffer{from=a,to=b,kind=Pacts.Kind[k]};if(OfferProblem(o)!="")continue;if(PactScore(a,b,k,null)<0)break;if(b!=0&&OfferWillingness(o)<0)break;offer=o;}
     if(offer==null){int resource=Enumerable.Range(1,5).FirstOrDefault(r=>HasDomesticResource(a,r)&&!HasResource(b,r));if(resource>0)offer=new DiplomaticOffer{from=a,to=b,kind=TreatyKind.Trade,giveResource=resource,askPerTurn=1};}
    }
    if(offer!=null&&OfferProblem(offer)==""&&(offer.to==0||OfferWillingness(offer)>=0))Propose(offer);
   }
   EnvoyAI(a);
   RequestsAI(a);
  }
 }
 // Envoys go where they do most good: to calm a power that fears this one, then to a leader nearly ready to sign.
 void EnvoyAI(int a){
  foreach(var p in diplomacy){if(p.a!=a&&p.b!=a)continue;int b=p.a==a?p.b:p.a;if(Envoy(a,b)&&(AtWar(a,b)||Rivals(a,b)||Ambition(a,b)>=50))SetEnvoy(a,b,false);}
  int free=EnvoyLimit(a)-EnvoysOut(a);
  while(free>0){int best=-1,score=0;for(int b=0;b<civCount;b++){if(b==a||Envoy(a,b)||EnvoyReason(a,b)!=""||Ambition(a,b)>=50)continue;int op=Opinion(b,a);if(op<=-50)continue;int s=(Threat(b,a)>=30?40:0)+(op<40?20+(40-op)/4:0)+(Strength(a,b)>Strength(a,a)?15:0);if(s>score){score=s;best=b;}}
   if(best<0||score<20||!SetEnvoy(a,best,true))break;free--;}
 }

 // ---------- validity and older saves ----------
 bool DiplomacyValid(){if(diplomacy==null||diplomacy.Count!=civCount*(civCount-1)/2||offers==null||contracts==null||claims==null||warWeariness==null||warWeariness.Length!=civCount||nextDiplomaticId<1)return false;
  var keys=new HashSet<int>();foreach(var p in diplomacy){if(p==null||p.a<0||p.b>=civCount||p.a>=p.b||!keys.Add(p.a*1000+p.b)||p.events==null||p.events.Count>12||p.events.Any(x=>x==null)||p.trustA<0||p.trustB<0||p.trustA>100||p.trustB>100||p.alarmA<0||p.alarmB<0||p.alarmA>100||p.alarmB>100||p.exhaustA<0||p.exhaustB<0||p.exhaustA>100||p.exhaustB>100||p.pacts<0||p.pacts>=1<<Pacts.Count||p.pactSince==null||p.pactSince.Length!=Pacts.Count||p.modsA==null||p.modsB==null||p.modsA.Length!=OpinionMods.Count||p.modsB.Length!=OpinionMods.Count||p.modsA.Any(v=>v<-100||v>100)||p.modsB.Any(v=>v<-100||v>100)||p.rivalA< -1||p.rivalB< -1||p.envoyA<0||p.envoyA>1||p.envoyB<0||p.envoyB>1||p.peaceUntil<0||p.warStarted<0)return false;}
  var ids=new HashSet<int>();foreach(var o in offers){if(o==null||o.id<1||o.id>=nextDiplomaticId||!ids.Add(o.id)||o.from<0||o.to<0||o.from>=civCount||o.to>=civCount||o.from==o.to||o.status<0||o.status>3||o.created<1||o.expires<=o.created||o.answer==null||!ValidKind(o.kind)||o.duration<5||o.duration>30||o.giveGold<0||o.askGold<0||o.giveGold>10000||o.askGold>10000||!ValidRecurring(o.givePerTurn,o.askPerTurn,o.giveResource,o.askResource))return false;}
  keys.Clear();foreach(var c in contracts)if(c==null||c.id<1||c.id>=nextDiplomaticId||!ids.Add(c.id)||c.from<0||c.to<0||c.from>=civCount||c.to>=civCount||c.from==c.to||c.start<1||c.end<=c.start||!keys.Add(Math.Min(c.from,c.to)*1000+Math.Max(c.from,c.to))||!ValidRecurring(c.givePerTurn,c.askPerTurn,c.giveResource,c.askResource))return false;
  foreach(var c in claims)if(c==null||c.owner<0||c.owner>=civCount||c.turn<0)return false;return RequestsValid();
 }
 static bool ValidRecurring(int give,int ask,int resource,int requested){return give>=0&&ask>=0&&give<=20&&ask<=20&&resource>=0&&resource<=5&&requested>=0&&requested<=5;}
 // Save version 40: a game saved under the old treaties keeps what it had, in the new terms. Timed open borders, friendship,
 // alliance and research pacts become standing agreements (friendship a non-aggression pact, alliance a defensive pact); old
 // trust and grievances become marks on opinion; an embargo or a recent denunciation becomes a rivalry; war weariness becomes
 // exhaustion of each war under way. Border promises, joint wars, embassies and coalitions are gone.
 internal void UpgradeDiplomacy40(){
  if(claims==null)claims=new List<Claim>();if(diplomacy==null)return;
  foreach(var p in diplomacy){if(p==null)continue;
   if(p.pactSince==null||p.pactSince.Length!=Pacts.Count)p.pactSince=new int[Pacts.Count];
   if(p.modsA==null||p.modsA.Length!=OpinionMods.Count)p.modsA=new int[OpinionMods.Count];if(p.modsB==null||p.modsB.Length!=OpinionMods.Count)p.modsB=new int[OpinionMods.Count];
   p.pacts=0;Action<int,bool> keep=(k,on)=>{if(on){p.pacts|=1<<k;p.pactSince[k]=Math.Max(0,turn-Pacts.MinTurns);}};
   bool war=AtWar(p.a,p.b);
   if(!war){keep(Pacts.Borders,p.accessUntil>turn||p.a==0&&openBorders!=null&&p.b<openBorders.Length&&openBorders[p.b]);keep(Pacts.NonAggression,p.friendUntil>turn||p.allianceUntil>turn);keep(Pacts.Defensive,p.allianceUntil>turn);keep(Pacts.Research,p.researchUntil>turn);}
   p.modsA[OpinionMods.Dealings]=Clamp(p.trustA+(p.embassyB?5:0),-100,100);p.modsB[OpinionMods.Dealings]=Clamp(p.trustB+(p.embassyA?5:0),-100,100);
   p.modsA[OpinionMods.Grievance]=-Math.Min(100,Math.Max(0,p.grievanceA)/2);p.modsB[OpinionMods.Grievance]=-Math.Min(100,Math.Max(0,p.grievanceB)/2);
   int cap=0;for(int k=0;k<Pacts.Count;k++)if((p.pacts&(1<<k))!=0)cap=Math.Max(cap,Pacts.TrustCap[k]);
   p.trustA=Clamp(p.trustA,0,cap);p.trustB=Clamp(p.trustB,0,cap);
   p.alarmA=Clamp(p.alarmA,0,100);p.alarmB=Clamp(p.alarmB,0,100);
   bool embargo=p.embargoUntil>turn;p.rivalA=p.pacts==0&&(embargo||p.denounceA>=0&&turn-p.denounceA<15)?Math.Max(0,turn-Pacts.CauseDelay):-1;p.rivalB=p.pacts==0&&(p.denounceB>=0&&turn-p.denounceB<15)?Math.Max(0,turn-Pacts.CauseDelay):-1;
   p.envoyA=p.envoyB=0;p.exhaustA=war&&warWeariness!=null&&p.a<warWeariness.Length?Clamp(warWeariness[p.a]*2,0,100):0;p.exhaustB=war&&warWeariness!=null&&p.b<warWeariness.Length?Clamp(warWeariness[p.b]*2,0,100):0;
   p.grievanceA=p.grievanceB=p.accessUntil=p.friendUntil=p.allianceUntil=p.researchUntil=p.promiseAUntil=p.promiseBUntil=p.embargoUntil=0;p.denounceA=p.denounceB=-1;p.improvedA=p.improvedB=-10;p.embassyA=p.embassyB=p.coalitionA=p.coalitionB=false;
   if(p.peaceUntil<0)p.peaceUntil=0;if(p.warStarted<0)p.warStarted=0;
   if(p.a==0&&openBorders!=null&&p.b<openBorders.Length)openBorders[p.b]=(p.pacts&(1<<Pacts.Borders|1<<Pacts.Defensive))!=0;
  }
  // Old kinds 1 and 2 keep their meaning under new names; 6 (border promise) and 7 (joint war) no longer exist.
  if(offers!=null)offers.RemoveAll(o=>o==null||(int)o.kind>=6);
  if(warWeariness!=null)for(int i=0;i<warWeariness.Length;i++)warWeariness[i]=0;
 }
}
}
