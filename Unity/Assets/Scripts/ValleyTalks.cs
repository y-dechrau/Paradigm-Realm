using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Early diplomacy over a place (owner brief, 27 Nov 2026): a neighbour raises a valley only when the outcome changes its plans.
// Each home rival, every few turns, answers four questions from its own knowledge (ValleyNeed):
//   1. Use       what it is building or about to build that needs timber, from its own production lines and next choice
//   2. Shortfall how much it lacks by when: demand over ValleyTuning.Horizon against stock, income and promised deliveries
//   3. Alternative its best other source: an unclaimed woodland it can reach with a camp, other settlement sites, or waiting
//   4. Gain      how many turns of work the deal saves against the best alternative; and, for a site, how much better the site
//                is than its next best (farmland and timber reported separately, so it speaks of what moves it)
// From these it either says nothing (and the reason is kept for developers), quietly takes its alternative, offers to buy a
// small amount, or asks you to leave a place unsettled, paying for the promise. It only knows what it has seen: your units and
// cities in sight of its own, the woodland it can see on your land. You can ask why, accept, decline, or answer with your own
// terms (supply timber for some turns and settle the place yourself, or sell timber now); it judges your terms by whether they
// arrive in time, whether it trusts you (your record of deliveries) and what else it could do. Agreements run every turn:
// deliveries through the stockpiles, promises through every settler's choice of site. Broken promises are remembered with the
// place and what was paid. A settled issue is not raised again unless the need has changed a good deal.
[Serializable] public class ValleyIssue{public int id,owner,other=0,site=-1,motive,kind,amount,gold,raised,status,need,shortfall,answered=-1;public string why="",opening="";}
// keepTurns: how long a promise to leave the place unsettled lasts from the start; supplyDone: every delivery made.
[Serializable] public class ValleyAccord{public int id,a,b,site=-1,radius=ValleyTuning.KeepRadius,keeper=-1,supplier=-1,receiver=-1,perTurn,turns,start,delivered,missed,goldEach,goldOnce,status,lastTurn=-1,issue,keepTurns;public bool supplyDone;public string terms="",place="";
 // The exact tiles of a promise you painted on the map (27 Nov 2026); empty in older saves and for the leaders' own talks, which
 // mean every tile within radius of site.
 public List<int> area=new List<int>();}
[Serializable] public class SupplyRecord{public int receiver,supplier,delivered,missed,broken;}
public static class ValleyTuning{
 public const int Horizon=12,LongHorizon=24,SmallShort=8,KeepRadius=3,EvidenceReach=3,CityEvidence=5,LandMargin=20,FoodMargin=2,RaiseEvery=3,RaiseAgain=15,GoldPerTurnSaved=6,PricePerTimber=3,MissLimit=2,MinGain=1;
 public static readonly string[] Motives={"timber","farmland"};
 public static readonly string[] Kinds={"nothing","buy timber","leave the place unsettled","supply timber"};
 public static readonly string[] Status={"open","agreed","refused","withdrawn","replaced"};
}
public class ValleyNeed{public int owner,site=-1,motive=-1,kind,demand,expected,shortfall,longDemand,longShort,worstTurn,usedBy,haveBy,shortAt=-1,alternativeGain,dealGain,landGain,altSite=-1,offerGold,amount;public bool use,supplierSeen,evidence;public string useText="",alt="",decision="",reason="";}
public partial class World {
 public List<ValleyIssue> valleyIssues=new List<ValleyIssue>();public List<ValleyAccord> valleyAccords=new List<ValleyAccord>();public List<SupplyRecord> supplyRecords=new List<SupplyRecord>();
 [NonSerialized] public Dictionary<int,ValleyNeed> valleyReasoning=new Dictionary<int,ValleyNeed>();
 List<ValleyIssue> Issues(){if(valleyIssues==null)valleyIssues=new List<ValleyIssue>();return valleyIssues;}
 List<ValleyAccord> Accords(){if(valleyAccords==null)valleyAccords=new List<ValleyAccord>();return valleyAccords;}
 SupplyRecord Record(int receiver,int supplier){if(supplyRecords==null)supplyRecords=new List<SupplyRecord>();var r=supplyRecords.Find(x=>x.receiver==receiver&&x.supplier==supplier);if(r==null){r=new SupplyRecord{receiver=receiver,supplier=supplier};supplyRecords.Add(r);}return r;}
 public ValleyIssue OpenIssue(int owner){return Issues().Find(i=>i.owner==owner&&i.status==0);}
 // ---- Where a place is, in words a leader would use ----
 // Named for you, the reader (owner report, 27 Nov 2026: "the north-eastern woods" said nothing about where): what the place is,
 // then how far and which way from your nearest town, e.g. "the woods 3 tiles north-east of Shamsira".
 public string PlaceName(int owner,int cell){if(cell<0)return "that land";var t=tiles[cell];
  {var mine=cities.Where(c=>c.owner==0).OrderBy(c=>Distance(c.x,c.y,t.x,t.y)).FirstOrDefault();if(mine!=null&&Distance(mine.x,mine.y,t.x,t.y)<=14){bool f=TilesAround(t.x,t.y,1).Any(n=>n.terrain==7);int wd=TilesAround(t.x,t.y,1).Count(Wooded);int d=Distance(mine.x,mine.y,t.x,t.y);if(d==0)return mine.name;
   return "the "+(f?"ford":wd>=3?"woods":"land")+" "+d+(d==1?" tile ":" tiles ")+CompassWord(mine.x,mine.y,t.x,t.y)+" of "+mine.name;}}
  var cap=CapitalOf(owner)??cities.FirstOrDefault(c=>c.owner==owner);
  string dir="";if(cap!=null){int dx=DeltaX(cap.x,t.x),dy=t.y-cap.y;dir=Math.Abs(dx)>=Math.Abs(dy)*2?(dx>0?"eastern ":"western "):Math.Abs(dy)>=Math.Abs(dx)*2?(dy>0?"southern ":"northern "):(dy>0?"south":"north")+(dx>0?"-eastern ":"-western ");}
  bool ford=TilesAround(t.x,t.y,1).Any(n=>n.terrain==7);int woods=TilesAround(t.x,t.y,1).Count(Wooded);return "the "+dir+(ford?"ford":woods>=3?"woods":"valley");}
 string CompassWord(int fx,int fy,int tx,int ty){int dx=DeltaX(fx,tx),dy=ty-fy;double a=Math.Atan2(-dy,dx*1.0)*180/Math.PI;string[] names={"east","north-east","north","north-west","west","south-west","south","south-east"};int k=(int)Math.Round(((a%360)+360)%360/45.0)%8;return names[k];}
 public List<Tile> KeepTiles(int site,int radius){if(site<0)return new List<Tile>();var s=tiles[site];return TilesAround(s.x,s.y,radius).Where(t=>Distance(t.x,t.y,s.x,s.y)<=radius).ToList();}
 // ---- 1-4: the rival's own reckoning ----
 public ValleyNeed AssessValley(int owner,int other=0){
  var n=new ValleyNeed{owner=owner};if(!buildingMaterials||owner<=0||IsNativeNation(owner)||!HasContact(owner,other)||AtWar(owner,other)){n.decision="silent";n.reason="no standing to talk (no contact, at war, or not a home civilisation)";return n;}
  // 1. Use: work under way and queued, then the building it would choose next.
  // Demand is what its work can actually use within the horizon: a forecast with unlimited timber, so slow shields count.
  var jobs=JobsWithQueue(owner);var inc0=MaterialIncome(owner);var trace=new List<int[]>();var unlimited=ForecastJobs(jobs.Select(j=>j.Copy()).ToList(),99999,99999,inc0,ValleyTuning.LongHorizon,turn,trace);
  var before=new Dictionary<string,int>();foreach(var j in jobs){var x=j;while(x!=null){before[x.key]=x.instT;x=x.next;}}
  var horizonJobs=unlimited.Where(j=>j.instT>(before.ContainsKey(j.key)?before[j.key]:0)).ToList();n.demand=horizonJobs.Sum(j=>j.instT-(before.ContainsKey(j.key)?before[j.key]:0));
  if(n.demand==0){var city=cities.Where(c=>c.owner==owner&&c.buildingProject<0).OrderByDescending(c=>c.pop).FirstOrDefault();if(city!=null){int b=PreferredBuilding(city);int tt=b>=0?BuildingTimber(b):0;if(tt>0){n.demand=tt;horizonJobs.Add(new MatJob{name=BuildName(b)+" (planned) in "+city.name});}}}
  n.use=n.demand>0;n.useText=n.use?string.Join(", ",horizonJobs.Take(3).Select(j=>j.name).ToArray()):"nothing it is building or planning needs timber";
  // 2. Shortfall by the horizon, with promised deliveries counted.
  var inc=MaterialIncome(owner);int promised=Accords().Where(a=>a.status==0&&a.receiver==owner).Sum(a=>a.perTurn*Math.Min(ValleyTuning.Horizon,a.turns-a.delivered));
  n.expected=nations[owner].timber+inc[0]*ValleyTuning.Horizon+promised;
  // The shortfall is the worst gap at any turn: timber its work would have used by then, against store, income and deliveries by
  // then (a forecast with unlimited timber; slow shields count). Within the horizon, and over the longer view.
  {int stock=nations[owner].timber;n.shortfall=0;n.longShort=0;n.longDemand=0;
   for(int k=1;k<=trace.Count;k++){int used=99999+inc0[0]*k-trace[k-1][0];int got=stock+inc[0]*k+Accords().Where(a=>a.status==0&&a.receiver==owner).Sum(a=>a.perTurn*Math.Min(k,a.turns-a.delivered));int gap=Math.Max(0,used-got);
    if(k<=ValleyTuning.Horizon){if(gap>n.shortfall){n.shortfall=gap;n.worstTurn=k;n.usedBy=used;n.haveBy=got;}if(k==Math.Min(ValleyTuning.Horizon,trace.Count))n.demand=used;}if(gap>n.longShort)n.longShort=gap;n.longDemand=used;}}
  if(jobs.Count>0){var f=ForecastJobs(jobs.Select(j=>j.Copy()).ToList(),nations[owner].timber,nations[owner].stone,inc,ValleyTuning.Horizon,turn,null);var w=f.Where(j=>j.firstWait>0).OrderBy(j=>j.firstWait).FirstOrDefault();if(w!=null)n.shortAt=w.firstWait;}
  // The place it has in mind: its settler's destination, else its best known woodland or site, near which it has seen you.
  int site=ValleySite(owner,other,out n.evidence);n.site=site;
  // 3. Alternative: the best woodland camp it could reach that is not the contested place.
  Tile bestAlt=null;int altGain=0;if(n.shortfall>0||n.longShort>0)foreach(var t in CampCandidates(owner)){if(site>=0&&Distance(t.x,t.y,tiles[site].x,tiles[site].y)<=ValleyTuning.KeepRadius+1)continue;int g=CampBenefit(owner,t,0,0);if(g>altGain){altGain=g;bestAlt=t;}}
  n.alternativeGain=altGain;n.altSite=bestAlt==null?-1:bestAlt.y*Width+bestAlt.x;n.alt=bestAlt!=null?"a woodland camp near "+NearestCityName(owner,bestAlt.x,bestAlt.y)+" ("+altGain+" turns sooner)":n.shortfall>0?"no reachable woodland of its own: waiting longer":"none needed";
  // 4. Gain from the contested place (a camp there) or from deliveries covering the shortfall.
  if(site>=0&&(n.shortfall>0||n.longShort>0)){n.dealGain=CampBenefit(owner,tiles[site],0,0);}
  int deliver=n.shortfall>0?Math.Max(1,(n.shortfall+ValleyTuning.Horizon/2-1)/(ValleyTuning.Horizon/2)):0;n.amount=n.shortfall;
  n.supplierSeen=tiles.Count(t=>t.owner==other&&Wooded(t)&&nations[owner].known!=null&&nations[owner].known[t.y*Width+t.x])>=3||Outposts().Any(p=>p.owner==other&&p.kind==0&&VisibleTo(owner,tiles[p.cell].x,tiles[p.cell].y));
  // Farmland: how much better the place is as a town than its next best site.
  if(site>=0){var best=AssessSettlement(owner,tiles[site].x,tiles[site].y,tiles[site].x,tiles[site].y);var alt=BestOtherSite(owner,site);n.landGain=alt==null?best.score:best.score-alt.score;if(alt!=null&&best.food-alt.food<ValleyTuning.FoodMargin&&n.landGain<ValleyTuning.LandMargin*2)n.landGain=Math.Min(n.landGain,ValleyTuning.LandMargin-1);}
  // The decision.
  int greed=Flavour(owner,DiplomacyFlavours.Greed);
  if(site>=0&&n.evidence&&n.landGain>=ValleyTuning.LandMargin&&(n.shortfall==0||n.dealGain<=n.alternativeGain)){n.motive=1;n.kind=2;n.offerGold=Math.Min(Treasury(owner)/2,10+n.landGain);n.decision="approach";n.reason="the place is the best town site it knows by "+n.landGain+" points (food), and it has seen your people near it";return n;}
  if(!n.use){n.decision="silent";n.reason="no use for timber: "+n.useText;return n;}
  if(n.shortfall==0&&n.longShort==0){n.decision="silent";n.reason="enough timber: needs "+n.demand+" within "+ValleyTuning.Horizon+" turns and expects "+n.expected+" ("+n.longDemand+" within "+ValleyTuning.LongHorizon+")";return n;}
  if(n.alternativeGain>0&&n.alternativeGain+ValleyTuning.MinGain>=n.dealGain&&Math.Max(n.shortfall,n.longShort)>ValleyTuning.SmallShort){n.decision="alternative";n.reason="short of "+n.shortfall+" timber, but "+n.alt+" serves as well as the contested place";return n;}
  if(n.shortfall<=ValleyTuning.SmallShort&&n.longShort<=ValleyTuning.SmallShort){if(n.supplierSeen){n.motive=0;n.kind=1;n.offerGold=n.amount*ValleyTuning.PricePerTimber*(10+greed)/15;n.decision="approach";n.reason="a small shortfall of "+n.shortfall+" timber"+(n.shortAt>0?" from turn "+(turn+n.shortAt):"")+"; buying it from you solves it";return n;}
   n.decision="silent";n.reason="a small shortfall of "+n.shortfall+" timber, but it has seen no timber of yours to buy";return n;}
  if(site>=0&&n.evidence&&n.dealGain>Math.Max(n.alternativeGain,0)+ValleyTuning.MinGain-1){n.motive=0;n.kind=2;n.offerGold=Math.Min(Treasury(owner)/2,(n.dealGain-n.alternativeGain)*ValleyTuning.GoldPerTurnSaved+10);n.decision="approach";n.reason="a lasting shortfall ("+n.shortfall+" timber within "+ValleyTuning.Horizon+" turns, "+n.longShort+" within "+ValleyTuning.LongHorizon+"); "+PlaceName(owner,site)+" is its only practical source ("+n.dealGain+" turns sooner against "+n.alternativeGain+") and it has seen your people there";return n;}
  if(site>=0&&!n.evidence){n.decision="quiet claim";n.reason="short of "+n.shortfall+" timber and means to take "+PlaceName(owner,site)+" itself; it has seen no one else there";return n;}
  n.decision="silent";n.reason="short of "+n.shortfall+" timber with no place in mind and no supplier seen";return n;
 }
 // Its settler's destination if it has one, else its best known woodland within reach of a future town; "evidence" when it has
 // seen your settler or soldiers within reach of the place, or your city close by.
 int ValleySite(int owner,int other,out bool evidence){
  evidence=false;int site=-1;var settler=units.FirstOrDefault(u=>u.owner==owner&&u.type==0&&u.hasOrder);
  if(settler!=null)site=settler.destinationY*Width+settler.destinationX;
  else{var cap=CapitalOf(owner);if(cap!=null){Tile best=null;int most=0;foreach(var t in TilesAround(cap.x,cap.y,7)){if(t.owner>=0||Distance(t.x,t.y,cap.x,cap.y)<4||nations[owner].known==null||!nations[owner].known[t.y*Width+t.x]||IsWater(t.terrain)||IsImpassable(t.terrain))continue;int w=TilesAround(t.x,t.y,1).Count(Wooded);if(w>most){most=w;best=t;}}if(best!=null&&most>=3)site=best.y*Width+best.x;}}
  if(site<0)return -1;var s=tiles[site];
  evidence=units.Any(u=>u.owner==other&&(u.type==0||Power[u.type]>0)&&Distance(u.x,u.y,s.x,s.y)<=ValleyTuning.EvidenceReach&&VisibleTo(owner,u.x,u.y))||cities.Any(c=>c.owner==other&&Distance(c.x,c.y,s.x,s.y)<=ValleyTuning.CityEvidence&&nations[owner].known!=null&&nations[owner].known[c.y*Width+c.x]);
  return site;
 }
 IEnumerable<Tile> CampCandidates(int owner){var known=nations[owner].known;foreach(var c in cities.Where(c=>c.owner==owner).ToList())foreach(var t in TilesAround(c.x,c.y,5)){if(t.owner==owner||known!=null&&!known[t.y*Width+t.x]||!Wooded(t))continue;if(CampSiteReason(owner,t)!="")continue;yield return t;}}
 // Its best town site away from the place (clear: how far from it counts as away; 3 when judging land, more when redirecting).
 public SettlementReport BestOtherSite(int owner,int site,int clear=3){var s=tiles[site];var cap=CapitalOf(owner);if(cap==null)return null;SettlementReport best=null;
  foreach(var t in TilesAround(cap.x,cap.y,10)){if(Distance(t.x,t.y,s.x,s.y)<=clear)continue;if(SettlementDiplomacyWarning(owner,t.x,t.y)!="")continue;if(SettlementTile(owner,t.x,t.y)==null)continue;var r=AssessSettlement(owner,t.x,t.y,cap.x,cap.y);if(!r.Suitable||r.threat)continue;if(best==null||r.score>best.score)best=r;}return best;}
 public string ValleyReasoningText(int owner){ValleyNeed n;if(valleyReasoning==null||!valleyReasoning.TryGetValue(owner,out n))n=AssessValley(owner);
  return NationName(owner)+": "+n.decision+" — "+n.reason+". Use: "+n.useText+"; demand "+n.demand+", expects "+n.expected+", short "+n.shortfall+(n.shortAt>0?" (work waits from turn "+(turn+n.shortAt)+")":"")+"; over "+ValleyTuning.LongHorizon+" turns needs "+n.longDemand+", short "+n.longShort+"; alternative: "+n.alt+"; place "+(n.site>=0?PlaceName(owner,n.site)+(n.evidence?" (has seen you there)":" (has not seen you there)"):"none")+"; deal gain "+n.dealGain+", land gain "+n.landGain+"."+(n.site>=0?" Forecast with a camp there: "+string.Join(", ",CampForecast(owner,tiles[n.site],0,CampWork+1).Select(p=>p.Key+" "+p.Value[0]+">"+p.Value[1]).ToArray()):"");}
 // ---- Each turn: reassess, raise, withdraw, run agreements ----
 void ValleyTurn(){
  if(!buildingMaterials)return;if(valleyReasoning==null)valleyReasoning=new Dictionary<int,ValleyNeed>();
  RunValleyAccords();
  for(int o=1;o<civCount;o++){if(IsNativeNation(o)||!met[o]||!cities.Any(c=>c.owner==o))continue;var open=OpenIssue(o);
   if(open==null&&(turn+o)%ValleyTuning.RaiseEvery!=0)continue;var n=AssessValley(o);valleyReasoning[o]=n;
   if(open!=null){bool still=n.decision=="approach"&&n.kind==open.kind&&n.motive==open.motive;
    if(!still){open.status=3;string why=n.decision=="alternative"?"they have found another source: "+n.alt:n.shortfall==0&&open.motive==0?"they no longer lack timber (they need "+n.demand+" and expect "+n.expected+")":"their plans have changed ("+n.reason+")";open.why=why;
     AddRivalCall(o,0,"withdraws the offer about "+PlaceName(o,open.site>=0?open.site:-1)+": "+why+".");Note(NationName(o)+" withdraws its offer: "+why+".");}
    continue;}
   if(n.decision!="approach")continue;
   // Not again about the same thing unless it has changed a good deal.
   var last=Issues().Where(i=>i.owner==o&&i.kind==n.kind&&(n.site<0||i.site<0||Distance(tiles[i.site].x,tiles[i.site].y,tiles[n.site].x,tiles[n.site].y)<=2)).OrderByDescending(i=>i.raised).FirstOrDefault();
   if(last!=null&&(turn-last.raised<ValleyTuning.RaiseAgain||last.status==1)&&n.shortfall<last.shortfall*2+4)continue;
   RaiseIssue(o,n);}
 }
 ValleyIssue RaiseIssue(int o,ValleyNeed n){
  var issue=new ValleyIssue{id=nextDiplomaticId++,owner=o,site=n.site,motive=n.motive,kind=n.kind,amount=n.amount,gold=n.offerGold,raised=turn,need=n.demand,shortfall=n.shortfall,why=n.reason};
  string place=PlaceName(o,n.site);
  issue.opening=n.kind==1?"Our builders are short of timber: "+n.amount+" by turn "+(turn+(n.shortAt>0?n.shortAt:ValleyTuning.Horizon))+". You have woods to spare. Will you sell us "+n.amount+" timber for "+n.offerGold+" gold?":
   (n.evidence&&units.Any(u=>u.owner==0&&u.type==0&&Distance(u.x,u.y,tiles[n.site].x,tiles[n.site].y)<=ValleyTuning.EvidenceReach&&VisibleTo(o,u.x,u.y))?"Your settlers are near "+place+". Do you mean to stay? ":"Your people are close to "+place+". ")+
   "We had hoped to found a town there. Leave it to us and we will pay "+n.offerGold+" gold.";
  Issues().Add(issue);AddRivalCall(o,0,"wants to talk about "+place+".");DiplomacyEvent(o,0,n.kind==1?"Asked to buy "+n.amount+" timber for "+issue.gold+" gold.":"Asked you to leave "+place+" unsettled, for "+issue.gold+" gold"+(n.motive==1?" (for its farmland).":" (for its timber)."));return issue;
 }
 // ---- Your answers ----
 // "Why?" The answer reveals what moves them; how much depends on the leader's candour (loyal, ungreedy leaders say more).
 public string ValleyWhy(ValleyIssue issue){
  var n=AssessValley(issue.owner);int candour=(Flavour(issue.owner,DiplomacyFlavours.Loyalty)+10-Flavour(issue.owner,DiplomacyFlavours.Greed))/2;
  if(issue.motive==1){string s="The land. "+Cap(PlaceName(issue.owner,issue.site))+" can feed a town; our other sites are poorer.";if(candour>=5)s+=" Timber we have"+(n.shortfall==0?" enough of.":" a use for, but that is not why.");return s;}
  string s2="The timber. "+(n.use?"We are building "+n.useText+". ":"");
  if(candour>=4)s2+=n.shortfall>0?"By turn "+(turn+n.worstTurn)+" our builders need "+n.usedBy+" timber and we will have "+n.haveBy+"."+(n.longShort>n.shortfall?" It does not get better: "+n.longShort+" short over "+ValleyTuning.LongHorizon+" turns.":""):"For now we have enough.";
  if(candour>=6)s2+=" "+(n.alternativeGain>0?"There is "+n.alt+", but it is poorer.":"Our own lands have little to fell.");
  if(issue.kind==2&&n.landGain<ValleyTuning.LandMargin)s2+=" Our hills can feed another town; it is the wood we lack.";
  return s2;
 }
 // Accepting their terms as they stand.
 public string AcceptValley(ValleyIssue issue){
  if(issue.status!=0)return "That offer is no longer open.";int o=issue.owner;
  if(issue.kind==1){if(nations[0].timber<issue.amount)return "You have only "+nations[0].timber+" timber.";if(Treasury(o)<issue.gold)return NationName(o)+" can no longer pay "+issue.gold+" gold.";
   nations[0].timber-=issue.amount;nations[o].timber+=issue.amount;AddDiplomaticGold(o,-issue.gold);AddDiplomaticGold(0,issue.gold);Record(o,0).delivered++;AddOpinion(o,0,OpinionMods.Dealings,3);
   issue.status=1;DiplomacyEvent(0,o,"Sold "+issue.amount+" timber to "+NationName(o)+" for "+issue.gold+" gold.");return "";}
  if(Treasury(o)<issue.gold)return NationName(o)+" can no longer pay "+issue.gold+" gold.";AddDiplomaticGold(o,-issue.gold);AddDiplomaticGold(0,issue.gold);
  Accords().Add(new ValleyAccord{id=nextDiplomaticId++,a=o,b=0,site=issue.site,keeper=0,goldOnce=issue.gold,start=turn,turns=0,keepTurns=ValleyTuning.LongHorizon*2,issue=issue.id,place=PlaceName(o,issue.site),terms="You leave "+PlaceName(o,issue.site)+" (within "+ValleyTuning.KeepRadius+" tiles) unsettled; "+NationName(o)+" paid "+issue.gold+" gold."});
  issue.status=1;AddOpinion(o,0,OpinionMods.Dealings,4);DiplomacyEvent(0,o,"Promised to leave "+PlaceName(o,issue.site)+" unsettled for "+issue.gold+" gold.");return "";
 }
 public void DeclineValley(ValleyIssue issue){if(issue.status!=0)return;issue.status=2;issue.answered=turn;AddOpinion(issue.owner,0,OpinionMods.Declined,-2);DiplomacyEvent(0,issue.owner,"Refused "+NationName(issue.owner)+" about "+PlaceName(issue.owner,issue.site)+".");}
 // Your counter: you supply perTurn timber for turns, and in return they leave the place to you (and pay per delivery).
 // Their answer weighs: does it cover the shortfall in time, do they trust you to deliver, and what else could they do.
 public string JudgeSupplyCounter(ValleyIssue issue,int perTurn,int turns,out int goldEach,out bool accept){
  goldEach=0;accept=false;int o=issue.owner;var n=AssessValley(o);
  if(issue.motive==1){return NationName(o)+": \"Timber is welcome, but it is the farmland we need. "+Cap(PlaceName(o,issue.site))+" would feed a town; your wood cannot.\"";}
  int total=perTurn*turns;int within=perTurn*Math.Min(turns,ValleyTuning.Horizon);
  var rec=Record(o,0);int trust=Trust(o,0)+rec.delivered*2-rec.missed*5-rec.broken*15+Flavour(o,DiplomacyFlavours.Loyalty);
  // Would the deliveries get its work done as soon as the place would?
  var jobs=JobsWithQueue(o);var inc=MaterialIncome(o);int saved=0;
  if(jobs.Count>0){var a=ForecastJobs(jobs.Select(j=>j.Copy()).ToList(),nations[o].timber,nations[o].stone,inc,40,turn,null);var b=ForecastJobsFrom(jobs.Select(j=>j.Copy()).ToList(),nations[o].timber,nations[o].stone,inc,new[]{perTurn,0},1,40);
   foreach(var j in a){var k=b.FirstOrDefault(x=>x.key==j.key);if(k!=null)saved+=Math.Max(0,(j.finish<0?40:j.finish)-(k.finish<0?40:k.finish));}}
  goldEach=Math.Max(1,perTurn*ValleyTuning.PricePerTimber*(10+Flavour(o,DiplomacyFlavours.Greed))/15-1);
  if(within<n.shortfall)return NationName(o)+": \""+within+" timber within "+ValleyTuning.Horizon+" turns does not cover what we lack ("+n.shortfall+"). Offer more, or sooner.\"";
  if(trust<0)return NationName(o)+": \"Promises of wood are easy. We have seen too little delivered by you to rely on it.\"";
  if(n.dealGain>saved+2&&n.alternativeGain<=0&&turns<ValleyTuning.Horizon)return NationName(o)+": \"That helps for "+turns+" turns, but the woods would serve us for good. Make it longer.\"";
  accept=true;return NationName(o)+": \"If the wood comes, we have no need of "+PlaceName(o,issue.site)+". We will settle elsewhere and pay "+goldEach+" gold for each delivery of "+perTurn+".\"";
 }
 public string AgreeSupplyCounter(ValleyIssue issue,int perTurn,int turns){
  int goldEach;bool accept;string said=JudgeSupplyCounter(issue,perTurn,turns,out goldEach,out accept);if(!accept){issue.answered=turn;DiplomacyEvent(0,issue.owner,"Counteroffer about "+PlaceName(issue.owner,issue.site)+" refused.");return said;}
  int o=issue.owner;string place=PlaceName(o,issue.site);
  Accords().Add(new ValleyAccord{id=nextDiplomaticId++,a=o,b=0,site=issue.site,keeper=o,supplier=0,receiver=o,perTurn=perTurn,turns=turns,keepTurns=turns+ValleyTuning.LongHorizon*2,goldEach=goldEach,start=turn,issue=issue.id,place=place,terms="You deliver "+perTurn+" timber a turn for "+turns+" turns; "+NationName(o)+" pays "+goldEach+" gold each delivery and leaves "+place+" (within "+ValleyTuning.KeepRadius+" tiles) unsettled."});
  issue.status=4;AddOpinion(o,0,OpinionMods.Dealings,3);DiplomacyEvent(0,o,"Agreed to supply "+perTurn+" timber a turn for "+turns+" turns; "+NationName(o)+" leaves "+place+" to you.");
  // The rival gives up the place now: its settler bound there turns to its best other site, well clear of the place.
  var other=BestOtherSite(o,issue.site,ValleyTuning.KeepRadius+3);
  foreach(var u in units.Where(u=>u.owner==o&&u.type==0&&u.hasOrder&&Distance(u.destinationX,u.destinationY,tiles[issue.site].x,tiles[issue.site].y)<=ValleyTuning.KeepRadius+1).ToList()){CancelOrder(u);var opts=SettlementOptions(u,1);if(opts.Count>0)Order(u,opts[0].x,opts[0].y);else if(other!=null)Order(u,other.x,other.y);}
  if(other!=null)DiplomacyEvent(o,0,NationName(o)+" turns its settlers from "+place+" towards other land.");
  return said;
 }
 // ---- Agreements at work ----
 void RunValleyAccords(){
  foreach(var a in Accords()){if(a.status!=0||a.lastTurn==turn)continue;a.lastTurn=turn;
   if(a.supplier>=0&&a.perTurn>0&&!a.supplyDone){var s=nations[a.supplier];
    if(s.timber>=a.perTurn){s.timber-=a.perTurn;nations[a.receiver].timber+=a.perTurn;a.delivered++;Record(a.receiver,a.supplier).delivered++;if(a.goldEach>0&&Treasury(a.receiver)>=a.goldEach){AddDiplomaticGold(a.receiver,-a.goldEach);AddDiplomaticGold(a.supplier,a.goldEach);}
     if(a.supplier==0&&a.delivered==a.turns)Note("You have delivered all "+a.perTurn*a.turns+" timber to "+NationName(a.receiver)+". They remember it.");}
    else{a.missed++;Record(a.receiver,a.supplier).missed++;if(a.supplier==0)Note("You could not deliver "+a.perTurn+" timber to "+NationName(a.receiver)+" this turn ("+s.timber+" in store). Missing "+ValleyTuning.MissLimit+" deliveries breaks the agreement.");
     if(a.missed>=ValleyTuning.MissLimit){BreakAccord(a,a.supplier,"missed "+a.missed+" deliveries of timber");continue;}}
    if(a.delivered>=a.turns){a.supplyDone=true;if(a.keeper<0)a.status=1;AddOpinion(a.receiver,a.supplier,OpinionMods.KeptWord,Math.Min(OpinionTuning.FulfilledOpinion,2+a.delivered/2));if(a.receiver>0)SetTrust(a.receiver,a.supplier,Math.Min(TrustCap(a.receiver,a.supplier),Trust(a.receiver,a.supplier)+OpinionTuning.FulfilledTrust));DiplomacyEvent(a.supplier,a.receiver,"Timber agreement fulfilled: "+a.delivered+" deliveries.");}}
   // A promise to leave the place unsettled runs its own term, after the deliveries.
   if(a.status==0&&a.keeper>=0&&a.keepTurns>0&&turn-a.start>=a.keepTurns){a.status=1;DiplomacyEvent(a.a,a.b,"The promise over "+a.place+" has run its term.");}
  }
 }
 void BreakAccord(ValleyAccord a,int breaker,string what){
  a.status=2;int victim=breaker==a.a?a.b:a.a;int paid=a.goldOnce+a.goldEach*a.delivered;int forgive=Flavour(victim,DiplomacyFlavours.Forgiveness);
  int hurt=Math.Min(30,8+paid/4+(a.keeper==breaker?6:0))*(12-forgive)/8;if(victim>0){AddOpinion(victim,breaker,OpinionMods.Broken,-hurt);SetTrust(victim,breaker,Trust(victim,breaker)-Math.Min(25,6+paid/5));}
  Record(victim,breaker).broken++;DiplomacyEvent(breaker,victim,NationName(breaker)+" broke the agreement over "+a.place+": "+what+(paid>0?" after "+paid+" gold was paid":"")+".");
 }
 // A promise to leave a place unsettled, as a warning on any site within it (every settler, yours and theirs, reads this).
 public ValleyAccord KeepPromiseAt(int owner,int x,int y){foreach(var a in Accords())if(a.status==0&&a.keeper==owner&&AccordCovers(a,x,y))return a;return null;}
 public bool AccordCovers(ValleyAccord a,int x,int y){if(a.area!=null&&a.area.Count>0)return a.area.Contains(y*Width+x);return a.site>=0&&Distance(tiles[a.site].x,tiles[a.site].y,x,y)<=a.radius;}
 public string KeepPromiseWarning(int owner,int x,int y){var a=KeepPromiseAt(owner,x,y);if(a==null)return "";int other=a.keeper==a.a?a.b:a.a;return "You promised "+NationName(other)+" to leave "+a.place+" unsettled ("+a.terms+") Settling here breaks that promise.";}
 // Founding inside a promise breaks it, scaled to what the other side gave for it.
 void ValleyOnFound(City c){var a=KeepPromiseAt(c.owner,c.x,c.y);if(a==null)return;BreakAccord(a,c.owner,"founded "+c.name+" inside it");if(c.owner==0)Note("You broke your promise to "+NationName(a.keeper==a.a?a.b:a.a)+" by founding "+c.name+" at "+a.place+". They will remember.");}
 public ValleyIssue IssueById(int id){return Issues().Find(i=>i.id==id);}
 // You start the talk: their honest answer now, and an issue to work with if they have one.
 public ValleyIssue AskAboutValley(int o){var open=OpenIssue(o);if(open!=null)return open;var n=AssessValley(o);if(valleyReasoning==null)valleyReasoning=new Dictionary<int,ValleyNeed>();valleyReasoning[o]=n;
  if(n.decision=="approach")return RaiseIssue(o,n);return null;}
 public string ValleyNothingToSay(int o){var n=AssessValley(o);return n.decision=="alternative"?NationName(o)+": \"We have our own woods in mind; there is nothing to settle between us.\"":n.decision=="quiet claim"?NationName(o)+": \"We have our own plans. They need not concern you.\"":
  !n.use?NationName(o)+": \"We have no great need of timber.\"":n.shortfall==0?NationName(o)+": \"Our woodcutters keep up with our builders. We have no quarrel with you over land.\"":NationName(o)+": \"Nothing that need trouble you.\"";}
 bool ValleyValid(){return Issues().All(i=>i!=null&&i.status>=0&&i.status<=4)&&Accords().All(a=>a!=null&&a.status>=0&&a.status<=3&&a.delivered>=0&&a.missed>=0);}
}
}
