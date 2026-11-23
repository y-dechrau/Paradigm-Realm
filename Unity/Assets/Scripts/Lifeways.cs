using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Beliefs & Traditions (owner briefs, 23 Nov 2026): how people live, built around four separate things.
//   Adherence   how much of a city follows each tradition (0-100 each; the largest at 40 or more is the city's tradition).
//   Conviction  how firmly that city holds its way of life (0-100): kept practices raise it, refused ones break it.
//   Institutions the practices a city keeps (a feast, a civic rule, a custom), each with a cost that falls due.
//   State policy what the ruler does about each tradition: tolerate, favour (paid) or discourage.
// The Faith currency keeps one meaning: the standing of the organised clergy of the historical religions (Religion.cs,
// FaithWorld.cs). Nothing here spends Faith; conviction is not a currency and cannot be bought.
// Traditions rise from the ground and from conduct, spread only along real connections, and bind their followers to
// commitments with triggers. A public vow is the ruler's own time-bound promise about a named place, people or practice.
[Serializable] public class Practice {public int id,city,owner,kind,tradition=-1,founded,nextDue,kept,missed,status,form,lastBent=-99,reserveUsed=-99;public string name="";}
[Serializable] public class WayChoice {public int id,kind,owner,city=-1,turn,a=-1,b;}
[Serializable] public class Vow {public int id,owner,kind,city=-1,target=-1,start,due,status,endedTurn;public string text="",ending="";}
[Serializable] public class CityWay {public int city;public int[] adherence=new int[Lifeways.Count];public int tradition=-1,conviction=20,shortage,reliefTurn=-99,reliefKind,enduredTurn=-99,sour,since;public bool split,starved;public string cause="";}
[Serializable] public class NationWay {public int owner;public int[] policy=new int[Lifeways.Count],commitment=new int[Lifeways.Count],breaches=new int[Lifeways.Count],renewTurn=new int[Lifeways.Count];public int lastVowEnd=-99,opening=-1,openingAnswer=-1,openingDue;public bool openingMet,openingDone,voiceHeard;public List<int> vowKeys=new List<int>();}
public static class Lifeways {
 public const int Ford=0,Remembrance=1,OpenHand=2,MeasuredSky=3,Count=4;
 public static readonly string[] Names={"Keeper of the Ford","Houses of Remembrance","School of the Open Hand","Society of the Measured Sky"};
 public static readonly string[] Kinds={"A deity of the crossing","Ancestral houses","Ethical philosophy","Natural philosophy"};
 public static readonly string[] Texts={
  "Where a river can be crossed, something keeps the crossing. Those who live by the water leave the first bread of every harvest at the ford and feed whoever comes hungry over it.",
  "The dead are not gone; they are the oldest members of the household. Each house keeps its names, and a city that has suffered keeps them hardest.",
  "No gods are asked. A person is measured by the open hand: what was given when giving cost something, and whether a word once spoken was kept.",
  "The sky keeps perfect time, so it can be measured, and what can be measured can be known. Its members watch, count and write down, and trust nothing they have not checked."};
 public static readonly string[] Roots={"Rises in cities beside a river.","Rises in cities that carry a scar, or that have stood for forty turns.","Rises in cities that gave or received relief in a shortage, or that hold a trade route.","Rises in cities with a library."};
 public static readonly string[] Gifts={"+1 food in a following city beside a river.","+1 production in a following city.","+1 gold in a following city.","+1 science in a following city (+2 while the commitment is held)."};
 public static readonly string[] CommitmentNames={"Feed the stranger at the ford","Keep the feast days","Break no word","Fund the observers"};
 public static readonly string[] CommitmentTexts={
  "When one of your cities runs short of food, relief is sent. Leaving a city to endure breaks it.",
  "Every practice your cities keep is honoured when it falls due. Refusing one breaks it.",
  "No agreement with another ruler is gone back on. Breaking one breaks this too. Rulers weigh your offers a little more kindly while it holds.",
  "Each discovery is written up and the observers paid (10 gold at every technology). Failing to pay breaks it."};
 public static readonly string[] PracticeKinds={"Religious observance","Civic rule","Local custom"};
 public static readonly string[] PolicyNames={"Tolerated","Favoured","Discouraged"};
 public static readonly string[] VowKinds={"None shall go hungry","No war upon them","The practice shall be kept","The first question"};
 public static readonly string[] VowEndings={"In force","Kept","Broken","Released","Made impossible"};
 public static readonly string[] ChoiceNames={"The stores are failing","How shall it be remembered?","A lean year","A voice beyond the gate","","A house divided","The first question"};
 public static readonly string[] OpeningQuestions={
  "A river runs past your first fires. Will it feed your people, or will they have to feed it?",
  "The soil here is thin. When there is not enough, who eats?",
  "A ring of standing stone hums beyond the hills. What is on the other side?",
  "This is good land, and others will want it. Can one city become a people?"};
 public static readonly string[] OpeningGoals={"Raise your capital to 4 citizens","Build a granary in your capital","Stand one of your units beside the stone ring","Found a second city"};
 // ---- Tuning: every number of the system in one place ----
 public const int LeanTurns=3,ReliefGap=15,ReliefFood=6,GoldPerFood=2,RememberAfter=3,Period=12,LongPeriod=20;
 public const int EmergeEvery=5,EmergeStep=3,EmergeAt=40,FadeAt=25,SpreadEvery=4,SpreadBase=4,SpreadFrom=50,SpreadReach=8;
 public const int KeptConviction=5,FulfilConviction=10,BentConviction=-3,RefusedConviction=-15,BreachConviction=-20,SourTurns=8,LapseAfter=2;
 public const int HouseCost=40,FavourCost=1,RenewCost=30,RenewWait=10,ObserverFee=10,ReconcileCost=40,GiftConviction=40,ProudConviction=60;
 public const int VowMin=15,VowMax=40,VowCooldown=8,ReleaseCost=25,VowConviction=10,VowTrust=5,VowInfluence=5,VowBreachTrust=-5,VowBreachMark=-6,ChronicleMax=80,OpeningGold=30;
}
public partial class World {
 public bool lifeways;
 public List<CityWay> ways=new List<CityWay>();public List<NationWay> nationWays=new List<NationWay>();public List<Practice> practices=new List<Practice>();
 public List<WayChoice> wayChoices=new List<WayChoice>();public List<Vow> vows=new List<Vow>();public List<string> wayChronicle=new List<string>();public int nextWayId=1;
 [NonSerialized] Dictionary<int,CityWay> wayIndex;[NonSerialized] int wayIndexCount=-1;
 void WayGuard(){if(ways==null)ways=new List<CityWay>();if(nationWays==null)nationWays=new List<NationWay>();if(practices==null)practices=new List<Practice>();if(wayChoices==null)wayChoices=new List<WayChoice>();if(vows==null)vows=new List<Vow>();if(wayChronicle==null)wayChronicle=new List<string>();}
 public CityWay WayOf(City c){WayGuard();if(wayIndex==null||wayIndexCount!=ways.Count){wayIndex=new Dictionary<int,CityWay>();foreach(var w in ways)wayIndex[w.city]=w;wayIndexCount=ways.Count;}CityWay l;if(!wayIndex.TryGetValue(c.id,out l)){l=new CityWay{city=c.id};ways.Add(l);wayIndex[c.id]=l;wayIndexCount=ways.Count;}return l;}
 public NationWay NationWayOf(int owner){WayGuard();var n=nationWays.Find(x=>x.owner==owner);if(n==null){n=new NationWay{owner=owner};nationWays.Add(n);}return n;}
 void WayPay(int owner,int amount){nations[owner].gold-=amount;if(owner==0)gold=nations[0].gold;}
 void Sway(CityWay l,int by){l.conviction=Math.Max(0,Math.Min(100,l.conviction+by));}
 void WayTell(string text,int owner=-1){WayGuard();wayChronicle.Insert(0,"Turn "+turn+": "+text);if(wayChronicle.Count>Lifeways.ChronicleMax)wayChronicle.RemoveAt(wayChronicle.Count-1);if(owner==0)Note(text);}
 public IEnumerable<City> Followers(int owner,int tradition){return cities.Where(c=>c.owner==owner&&WayOf(c).tradition==tradition);}
 public bool RootOf(City c,int t){var l=WayOf(c);var tile=At(c.x,c.y);
  switch(t){case Lifeways.Ford:return tile!=null&&NearRiver(tile);case Lifeways.Remembrance:return c.scars!=null&&c.scars.Count>0||c.age>=40;
   case Lifeways.OpenHand:return l.reliefKind>0||tradeRoutes!=null&&tradeRoutes.Any(r=>r.origin==c.id||r.destination==c.id);default:return c.library;}}
 // ---- Effects on a city ----
 public int WayContent(City c){if(!lifeways)return 0;var l=WayOf(c);int v=0;if(practices.Any(p=>p.city==c.id&&p.status==0&&p.kind==0))v++;if(l.tradition>=0&&l.conviction>=Lifeways.ProudConviction)v++;if(l.sour>0)v--;
  if(l.tradition>=0&&NationWayOf(c.owner).policy[l.tradition]==2)v--;return v;}
 void WayYields(City c,ref int f,ref int p,ref int s,ref int g){if(!lifeways)return;var l=WayOf(c);if(l.tradition<0||l.split||l.conviction<Lifeways.GiftConviction)return;
  switch(l.tradition){case Lifeways.Ford:{var t=At(c.x,c.y);if(t!=null&&NearRiver(t))f++;break;}case Lifeways.Remembrance:p++;break;case Lifeways.OpenHand:g++;break;default:s+=NationWayOf(c.owner).commitment[Lifeways.MeasuredSky]==1?2:1;break;}}
 // A city under a civic rule keeps a public reserve: once between observances it carries the city through an empty store.
 bool WayReserve(City c){if(!lifeways)return false;var p=practices.Find(x=>x.city==c.id&&x.status==0&&x.kind==1&&x.reserveUsed<x.nextDue-PeriodOf(x));if(p==null)return false;p.reserveUsed=turn;WayTell("The public reserve of "+c.name+" was opened; no one starved.",c.owner);return true;}
 void WayStarved(City c){if(lifeways)WayOf(c).starved=true;}
 public string WayLine(City c){if(!lifeways)return "";var l=WayOf(c);var p=practices.Find(x=>x.city==c.id&&x.status==0);
  return (l.tradition>=0?Lifeways.Names[l.tradition]+" ("+l.adherence[l.tradition]+"% follow"+(l.split?", its own house":"")+")":"No settled tradition")+" · conviction "+l.conviction+(p!=null?" · "+p.name+" due in "+Math.Max(0,p.nextDue-turn)+" turns":"");}
 // ---- Choices put to a ruler ----
 public WayChoice NextWayChoice(){return lifeways&&wayChoices!=null&&wayChoices.Count>0?wayChoices[0]:null;}
 void WayRaise(WayChoice e){if(wayChoices.Any(x=>x.kind==e.kind&&x.owner==e.owner&&x.city==e.city&&x.a==e.a))return;e.id=nextWayId++;e.turn=turn;wayChoices.Add(e);
  if(e.owner==0)Note(Lifeways.ChoiceNames[e.kind]+": a decision awaits.");else WayResolve(e,WayAIChoice(e));}
 internal void WayRaiseForChecks(WayChoice e){WayRaise(e);}
 int ReliefNeed(City c){return Math.Max(4,-Yield(c)[0]*Lifeways.ReliefFood);}
 City ReliefDonor(City c){int need=ReliefNeed(c);return cities.Where(x=>x.owner==c.owner&&x.id!=c.id&&x.food>=need).OrderByDescending(x=>x.food).ThenBy(x=>x.id).FirstOrDefault();}
 int PeriodOf(Practice p){return p.form==2?Lifeways.LongPeriod:Lifeways.Period;}
 // What a practice costs when it falls due: food from the city's store, or coin.
 public int PracticeCost(Practice p,out bool inGold){var c=cities.Find(x=>x.id==p.city);int pop=c==null?1:c.pop;inGold=p.kind==1||p.form==1;return p.kind==0?(inGold?pop*3:Math.Max(4,pop*2)):p.kind==1?pop*3:inGold?pop*2:Math.Max(2,pop);}
 public string PracticeCostText(Practice p){bool g;int v=PracticeCost(p,out g);return v+(g?" gold":" food from the city's store");}
 bool CanPayPractice(Practice p,int share=1){bool g;int v=PracticeCost(p,out g)/share;var c=cities.Find(x=>x.id==p.city);return c!=null&&(g?nations[p.owner].gold>=v:c.food>=v);}
 void PayPractice(Practice p,int share=1){bool g;int v=PracticeCost(p,out g)/share;var c=cities.Find(x=>x.id==p.city);if(g)WayPay(p.owner,v);else c.food-=v;}
 public string WayChoiceText(WayChoice e){var c=cities.Find(x=>x.id==e.city);string name=c==null?"the city":c.name;
  switch(e.kind){
   case 0:return "The stores of "+name+" will be empty within "+Lifeways.LeanTurns+" turns, and the fields cannot make it up ("+Yield(c)[0]+" food a turn). People are already going without.";
   case 1:return name+" was fed when it could not feed itself. People are asking how that should be remembered, and whatever is chosen will be owed again every "+Lifeways.Period+" turns.";
   case 2:{var p=practices.Find(x=>x.id==e.a);return p==null?"":p.name+" falls due in "+name+", and it is a lean year: keeping it in full costs "+PracticeCostText(p)+". It has been kept "+p.kept+" time"+(p.kept==1?"":"s")+" and missed "+p.missed+".";}
   case 3:{int t=DominantWay(e.owner);string reading=t==Lifeways.Ford?"The keepers of the ford say every crossing has its keeper, and this one has not been fed.":t==Lifeways.Remembrance?"The houses of remembrance say it is the voice of someone's dead, and should be answered as kin.":t==Lifeways.OpenHand?"The teachers of the Open Hand say a stranger has spoken first, and courtesy is owed.":t==Lifeways.MeasuredSky?"The Society says it is a sound like any other: it should be timed, written down and repeated.":"Nobody at home agrees what it is.";
    return "The gateway is open, and something on the far side is speaking. No one can make out words. "+reading;}
   case 5:return "The followers of "+Lifeways.Names[e.a]+" in "+name+" say the realm has broken faith with its own teaching. They will keep the commitment themselves, with or without you.";
   case 6:return Lifeways.OpeningQuestions[e.a]+" Your people are waiting to hear what you intend: "+Lifeways.OpeningGoals[e.a].ToLowerInvariant()+" before turn "+e.b+".";
  }return "";}
 public string[] WayOptions(WayChoice e){var c=cities.Find(x=>x.id==e.city);
  switch(e.kind){
   case 0:{var d=ReliefDonor(c);int need=ReliefNeed(c);return new[]{"Send "+need+" food from "+(d!=null?d.name:"another city")+" (its own growth is set back)","Buy "+need+" food for "+need*Lifeways.GoldPerFood+" gold","Let them endure (no cost; "+c.name+" will remember it)"};}
   case 1:return new[]{"A religious observance: the Feast of the First Granary ("+Math.Max(4,c.pop*2)+" food each time; +1 content)","A civic rule: the Granary Rule ("+c.pop*3+" gold each time; a public reserve against starvation)","A local custom: the Shared Loaf ("+Math.Max(2,c.pop)+" food each time; the city's own, and firmly held)","Let it pass unmarked"};
   case 2:{var p=practices.Find(x=>x.id==e.a);return new[]{"Keep it in full ("+(p==null?"":PracticeCostText(p))+")","Ask the elders for a smaller observance (half the cost)","Refuse it this year"};}
   case 3:return new[]{"Listen, and write down what is heard","Ward the gateway with rites (20 gold)","Answer it as a guest would be answered"};
   case 5:return new[]{"Reconcile: renew the commitment ("+Lifeways.ReconcileCost+" gold)","Let them keep their own house","Suppress them"};
   case 6:return new[]{"Make it a public vow (a greater reward if kept; a broken word if not)","Keep it as a private aim (a smaller reward; nothing lost)","Set it aside"};
  }return new string[0];}
 // Why an option cannot be taken ("" when it can).
 public string WayOptionProblem(WayChoice e,int k){var c=cities.Find(x=>x.id==e.city);var n=nations[e.owner];
  switch(e.kind){
   case 0:if(k==0&&ReliefDonor(c)==null)return "no other city has that much stored";if(k==1&&n.gold<ReliefNeed(c)*Lifeways.GoldPerFood)return "not enough gold";break;
   case 2:{var p=practices.Find(x=>x.id==e.a);if(p==null)return "";bool g;int v=PracticeCost(p,out g);if(k==0&&!CanPayPractice(p)&&n.gold<v*2)return "neither the store nor the treasury can cover it";if(k==1&&p.kept+p.missed>0&&p.lastBent==p.kept+p.missed-1)return "the elders bent last time and will not again";if(k==1&&!CanPayPractice(p,2)&&n.gold<v)return "even half cannot be found";break;}
   case 3:if(k==1&&n.gold<20)return "not enough gold";break;
   case 5:if(k==0&&n.gold<Lifeways.ReconcileCost)return "not enough gold";break;
  }return "";}
 int WayAIChoice(WayChoice e){int n=WayOptions(e).Length;for(int k=0;k<n;k++)if(WayOptionProblem(e,k)=="")return k;return n-1;}
 public bool WayResolve(WayChoice e,int k){
  if(e==null||!wayChoices.Contains(e)||k<0||k>=WayOptions(e).Length||WayOptionProblem(e,k)!="")return false;wayChoices.Remove(e);
  var c=cities.Find(x=>x.id==e.city);var nw=NationWayOf(e.owner);if(c==null&&e.kind!=3&&e.kind!=6)return true;var l=c!=null?WayOf(c):null;
  switch(e.kind){
   case 0:{int need=ReliefNeed(c);
    if(k==0){var d=ReliefDonor(c);d.food-=need;c.food+=need;l.reliefTurn=turn;l.reliefKind=1;WayOf(d).reliefKind=Math.Max(WayOf(d).reliefKind,1);Sway(l,5);WayTell(d.name+" sent "+need+" food to "+c.name+" when its stores failed.",e.owner);}
    else if(k==1){WayPay(e.owner,need*Lifeways.GoldPerFood);c.food+=need;l.reliefTurn=turn;l.reliefKind=2;Sway(l,3);WayTell("Grain was bought for "+c.name+" ("+need*Lifeways.GoldPerFood+" gold) when its stores failed.",e.owner);}
    else{l.enduredTurn=turn;l.sour=Lifeways.SourTurns;Sway(l,-5);WayTell(c.name+" was left to endure its shortage.",e.owner);if(nw.commitment[Lifeways.Ford]==1)WayBreach(e.owner,Lifeways.Ford,c.name+" was left hungry");}
    break;}
   case 1:{if(k==3){WayTell("The relief of "+c.name+" passed unmarked.",e.owner);break;}
    var p=new Practice{id=nextWayId++,city=c.id,owner=e.owner,kind=k,founded=turn,nextDue=turn+Lifeways.Period};bool first=!practices.Any(x=>x.owner==e.owner);var tile=At(c.x,c.y);
    if(k==0){p.name=first?"The Feast of the First Granary":"The Feast of Thanks";p.tradition=tile!=null&&NearRiver(tile)?Lifeways.Ford:Lifeways.Remembrance;}else if(k==1){p.name="The Granary Rule";p.tradition=Lifeways.OpenHand;}else{p.name="The Shared Loaf";Sway(l,10);}
    if(p.tradition>=0)l.adherence[p.tradition]=Math.Min(100,l.adherence[p.tradition]+30);practices.Add(p);Sway(l,5);
    WayTell(c.name+" keeps "+p.name+" ("+Lifeways.PracticeKinds[k].ToLowerInvariant()+") in memory of its relief. It falls due every "+Lifeways.Period+" turns.",e.owner);SettleWay(c,"it grew out of the relief of turn "+l.reliefTurn);break;}
   case 2:{var p=practices.Find(x=>x.id==e.a);if(p==null||p.status!=0)break;bool g;int v=PracticeCost(p,out g);
    if(k==0){if(CanPayPractice(p))PayPractice(p);else WayPay(e.owner,v*2);KeepPractice(p,Lifeways.FulfilConviction," in a lean year, at full cost");}
    else if(k==1){if(CanPayPractice(p,2))PayPractice(p,2);else WayPay(e.owner,v);p.lastBent=p.kept+p.missed;KeepPractice(p,Lifeways.BentConviction,", made smaller by agreement with the elders");}
    else RefusePractice(p);break;}
   case 3:{nw.voiceHeard=true;var n=nations[e.owner];int[] moved=k==0?new[]{-5,0,0,10}:k==1?new[]{10,10,0,-5}:new[]{0,5,10,0};
    if(k==0){if(n.research>=0)n.progress[n.research]+=15;}else if(k==1)WayPay(e.owner,20);else n.influence+=5;
    foreach(var x in cities.Where(x=>x.owner==e.owner)){var w=WayOf(x);if(w.tradition>=0)Sway(w,moved[w.tradition]);}
    WayTell(NationName(e.owner)+(k==0?" listened at the gateway and wrote down what was heard (+15 research).":k==1?" warded the gateway with rites.":" answered the voice beyond the gateway as a guest (+5 influence)."),e.owner);break;}
   case 5:{int t=e.a;
    if(k==0){WayPay(e.owner,Lifeways.ReconcileCost);nw.commitment[t]=1;nw.breaches[t]=0;Sway(l,10);WayTell("The realm renewed "+Lifeways.CommitmentNames[t]+"; "+c.name+" is reconciled.",e.owner);}
    else if(k==1){l.split=true;Sway(l,15);WayTell("The followers of "+Lifeways.Names[t]+" in "+c.name+" keep their own house now, apart from the realm's.",e.owner);}
    else{l.adherence[t]=Math.Max(0,l.adherence[t]-30);l.sour=12;Sway(l,-20);foreach(var x in Followers(e.owner,t))if(x.id!=c.id)Sway(WayOf(x),-5);WayTell("The strict followers of "+Lifeways.Names[t]+" in "+c.name+" were suppressed.",e.owner);SettleWay(c,"");}
    break;}
   case 6:{nw.openingAnswer=k;if(k==0){var v=new Vow{id=nextWayId++,owner=e.owner,kind=3,target=e.a,start=turn,due=e.b,text="We will "+Lifeways.OpeningGoals[e.a].ToLowerInvariant()+" before turn "+e.b+"."};vows.Add(v);WayTell(NationName(e.owner)+" vowed in public: "+v.text,e.owner);}
    else if(k==2)nw.openingDone=true;break;}
  }return true;}
 void KeepPractice(Practice p,int conviction,string how=""){var c=cities.Find(x=>x.id==p.city);p.kept++;p.nextDue=turn+PeriodOf(p);if(c==null)return;var l=WayOf(c);Sway(l,conviction);if(p.tradition>=0)l.adherence[p.tradition]=Math.Min(100,l.adherence[p.tradition]+5);WayTell(c.name+" kept "+p.name+how+".",p.owner);}
 void RefusePractice(Practice p){var c=cities.Find(x=>x.id==p.city);p.missed++;p.nextDue=turn+PeriodOf(p);var l=WayOf(c);Sway(l,Lifeways.RefusedConviction);l.sour=Lifeways.SourTurns;
  if(p.missed>=Lifeways.LapseAfter){p.status=1;WayTell(p.name+" of "+c.name+" has lapsed: refused twice, it is no longer kept.",p.owner);}else WayTell(c.name+" was refused "+p.name+" this year.",p.owner);
  if(NationWayOf(p.owner).commitment[Lifeways.Remembrance]==1)WayBreach(p.owner,Lifeways.Remembrance,p.name+" was refused");
  foreach(var v in vows.Where(v=>v.status==0&&v.kind==2&&v.owner==p.owner&&v.city==p.city).ToList())EndVow(v,2,p.name+" was refused");}
 // A reform changes how a practice is kept: paid in coin, or kept less often. It costs conviction now.
 public string ReformProblem(int id,int form){var p=practices.Find(x=>x.id==id);if(p==null||p.status!=0)return "No such practice.";if(form!=1&&form!=2)return "Choose a new form.";if(p.form!=0)return "It has been reformed once already.";if(p.kept+p.missed<1)return "It must fall due once before it can be reformed.";if(form==1&&p.kind==1)return "It is already paid in coin.";return "";}
 public string ReformPractice(int id,int form){string why=ReformProblem(id,form);if(why!="")return why;var p=practices.Find(x=>x.id==id);var c=cities.Find(x=>x.id==p.city);p.form=form;p.missed=0;Sway(WayOf(c),-10);
  WayTell(p.name+" of "+c.name+" was reformed: "+(form==1?"it is now paid in coin, not grain.":"it is now kept every "+Lifeways.LongPeriod+" turns."),p.owner);return "";}
 // ---- Traditions: emergence, founding, spread, policy ----
 public int DominantWay(int owner){int best=-1,most=0;for(int t=0;t<Lifeways.Count;t++){int n=Followers(owner,t).Count();if(n>most){most=n;best=t;}}return best;}
 void SettleWay(City c,string cause){var l=WayOf(c);int top=0;for(int t=1;t<Lifeways.Count;t++)if(l.adherence[t]>l.adherence[top])top=t;
  if(l.adherence[top]>=Lifeways.EmergeAt&&l.tradition!=top){bool first=!ways.Any(w=>w.tradition==top);l.tradition=top;l.since=turn;l.cause=cause;l.split=false;
   WayTell((first?Lifeways.Names[top]+" is first honoured at "+c.name:Lifeways.Names[top]+" took hold in "+c.name)+(cause!=""?": "+cause:"")+".",c.owner);}
  else if(l.tradition>=0&&l.adherence[l.tradition]<Lifeways.FadeAt){WayTell(Lifeways.Names[l.tradition]+" faded from "+c.name+".",c.owner);l.tradition=-1;l.split=false;l.cause="";}}
 public string HouseProblem(int owner,int cityId,int t){var c=cities.Find(x=>x.id==cityId&&x.owner==owner);if(c==null)return "Choose one of your cities.";if(t<0||t>=Lifeways.Count)return "Choose a tradition.";var l=WayOf(c);
  if(l.tradition==t)return c.name+" already follows it.";if(!RootOf(c,t))return "It has no root there. "+Lifeways.Roots[t];if(NationWayOf(owner).policy[t]==2)return "You discourage this tradition.";if(nations[owner].gold<Lifeways.HouseCost)return "It costs "+Lifeways.HouseCost+" gold.";return "";}
 public string FoundHouse(int owner,int cityId,int t){string why=HouseProblem(owner,cityId,t);if(why!="")return why;var c=cities.Find(x=>x.id==cityId);var l=WayOf(c);WayPay(owner,Lifeways.HouseCost);for(int k=0;k<Lifeways.Count;k++)if(k!=t)l.adherence[k]=Math.Min(l.adherence[k],35);l.adherence[t]=Math.Max(l.adherence[t],60);Sway(l,10);SettleWay(c,"a house was founded there by the ruler");return "";}
 public string SetWayPolicy(int owner,int t,int policy){if(t<0||t>=Lifeways.Count||policy<0||policy>2)return "No such policy.";var nw=NationWayOf(owner);if(nw.policy[t]==policy)return "";nw.policy[t]=policy;
  WayTell(NationName(owner)+" now "+(policy==0?"tolerates":policy==1?"favours":"discourages")+" "+Lifeways.Names[t]+".",owner);if(policy==2)foreach(var c in Followers(owner,t))Sway(WayOf(c),-5);return "";}
 // The cities a tradition can travel to from here, each with the road it takes.
 IEnumerable<KeyValuePair<City,string>> WayLinks(City a){
  if(tradeRoutes!=null)foreach(var r in tradeRoutes){int other=r.origin==a.id?r.destination:r.destination==a.id?r.origin:-1;if(other<0)continue;var b=cities.Find(x=>x.id==other);if(b!=null)yield return new KeyValuePair<City,string>(b,"carried by traders on the route from "+a.name);}
  bool linked=Connected(a);int realm=At(a.x,a.y).realm;
  foreach(var b in cities){if(b.id==a.id||Distance(a.x,a.y,b.x,b.y)>Lifeways.SpreadReach||realm!=At(b.x,b.y).realm)continue;
   if(b.owner==a.owner){if(linked&&Connected(b))yield return new KeyValuePair<City,string>(b,"brought along the road from "+a.name);}
   else if(!IsNativeNation(b.owner)&&!IsNativeNation(a.owner)&&HasAccess(a.owner,b.owner))yield return new KeyValuePair<City,string>(b,"brought over the open border from "+a.name);}}
 void WaysEmerge(City c){var l=WayOf(c);var nw=NationWayOf(c.owner);
  for(int t=0;t<Lifeways.Count;t++){int step=RootOf(c,t)?Lifeways.EmergeStep:l.adherence[t]>0&&l.tradition!=t?-1:0;if(nw.policy[t]==1&&step>0)step+=2;if(nw.policy[t]==2)step=-4;l.adherence[t]=Math.Max(0,Math.Min(100,l.adherence[t]+step));}
  int top=0;for(int t=1;t<Lifeways.Count;t++)if(l.adherence[t]>l.adherence[top])top=t;
  string cause=top==Lifeways.Ford?"its people live by the river":top==Lifeways.Remembrance?(c.scars!=null&&c.scars.Count>0?"it has not forgotten what it suffered":"it is an old city with many dead"):top==Lifeways.OpenHand?"it has known what it is to be helped":"its library keeps the count of the sky";
  SettleWay(c,cause);}
 void WaysSpread(City a){var l=WayOf(a);int t=l.tradition;if(t<0||l.adherence[t]<Lifeways.SpreadFrom||l.conviction<30||NationWayOf(a.owner).policy[t]==2)return;
  foreach(var link in WayLinks(a).ToList()){var b=link.Key;var lb=WayOf(b);if(lb.tradition==t||NationWayOf(b.owner).policy[t]==2)continue;lb.adherence[t]=Math.Min(100,lb.adherence[t]+Lifeways.SpreadBase+l.conviction/20);
   int top=0;for(int k=1;k<Lifeways.Count;k++)if(lb.adherence[k]>lb.adherence[top])top=k;if(top==t)SettleWay(b,link.Value);}}
 internal void WaysSpreadForChecks(City a){WaysSpread(a);}
 internal void WaysEmergeForChecks(City c){WaysEmerge(c);}
 // ---- Commitments: what a tradition binds its realm to ----
 public string CommitmentProblem(int owner,int t){var nw=NationWayOf(owner);if(nw.commitment[t]==1)return "Already held.";if(Followers(owner,t).Count()<2)return "Two of your cities must follow it first.";if(nw.commitment[t]==2&&turn<nw.renewTurn[t])return "Broken too lately: it can be renewed from turn "+nw.renewTurn[t]+".";if(nw.commitment[t]==2&&nations[owner].gold<Lifeways.RenewCost)return "Renewing it costs "+Lifeways.RenewCost+" gold.";return "";}
 public string TakeCommitment(int owner,int t){string why=CommitmentProblem(owner,t);if(why!="")return why;var nw=NationWayOf(owner);if(nw.commitment[t]==2)WayPay(owner,Lifeways.RenewCost);nw.commitment[t]=1;foreach(var c in Followers(owner,t))Sway(WayOf(c),5);WayTell(NationName(owner)+" took up the commitment of "+Lifeways.Names[t]+": "+Lifeways.CommitmentNames[t]+".",owner);return "";}
 void WayBreach(int owner,int t,string how){var nw=NationWayOf(owner);if(nw.commitment[t]!=1)return;nw.commitment[t]=2;nw.breaches[t]++;nw.renewTurn[t]=turn+Lifeways.RenewWait;var hit=Followers(owner,t).ToList();foreach(var c in hit)Sway(WayOf(c),Lifeways.BreachConviction);
  WayTell(NationName(owner)+" broke "+Lifeways.CommitmentNames[t]+": "+how+".",owner);
  // A realm that breaks its own teaching twice, or breaks it while a city still holds firm, divides that city from it.
  var firm=hit.Where(c=>!WayOf(c).split).OrderByDescending(c=>WayOf(c).conviction).ThenBy(c=>c.id).FirstOrDefault();if(firm!=null&&(nw.breaches[t]>=2||WayOf(firm).conviction>=Lifeways.GiftConviction))WayRaise(new WayChoice{kind=5,owner=owner,city=firm.id,a=t});}
 internal void WayBrokenWord(int by){if(lifeways&&by>=0)WayBreach(by,Lifeways.OpenHand,"an agreement was gone back on");}
 internal void WayDiscovery(int owner){if(!lifeways)return;var nw=NationWayOf(owner);if(nw.commitment[Lifeways.MeasuredSky]!=1)return;if(nations[owner].gold>=Lifeways.ObserverFee){WayPay(owner,Lifeways.ObserverFee);foreach(var c in Followers(owner,Lifeways.MeasuredSky))Sway(WayOf(c),3);}else WayBreach(owner,Lifeways.MeasuredSky,"the observers went unpaid");}
 public int WayDealInterest(int asker){return lifeways&&NationWayOf(asker).commitment[Lifeways.OpenHand]==1?6:0;}
 // ---- Public vows ----
 int VowKey(int kind,int city,int target){return kind*1000000+(city>=0?city:target+500000);}
 public int VowsMadeOn(int owner,int kind,int city,int target){int key=VowKey(kind,city,target);return NationWayOf(owner).vowKeys.Count(k=>k==key);}
 public string VowProblem(int owner,int kind,int cityId,int target,int turns){var nw=NationWayOf(owner);
  if(vows.Any(v=>v.owner==owner&&v.status==0&&v.kind!=3))return "One vow at a time: the last is still in force.";if(turn-nw.lastVowEnd<Lifeways.VowCooldown)return "Too soon after the last: wait until turn "+(nw.lastVowEnd+Lifeways.VowCooldown)+".";
  if(turns<Lifeways.VowMin||turns>Lifeways.VowMax)return "A vow runs "+Lifeways.VowMin+" to "+Lifeways.VowMax+" turns.";var c=cities.Find(x=>x.id==cityId&&x.owner==owner);
  if(kind==0){if(c==null)return "Name one of your cities.";}
  else if(kind==1){if(target<0||target>=civCount||target==owner||IsNativeNation(target)||!HasContact(owner,target))return "Name a people you know.";if(AtWar(owner,target))return "You are at war with them.";}
  else if(kind==2){if(c==null||!practices.Any(p=>p.city==cityId&&p.status==0))return "Name a city that keeps a practice.";}
  else return "No such vow.";return "";}
 public string VowText(int owner,int kind,int cityId,int target,int turns){var c=cities.Find(x=>x.id==cityId);var p=practices.Find(x=>x.city==cityId&&x.status==0);
  return kind==0?"No one in "+(c==null?"the city":c.name)+" will starve before turn "+(turn+turns)+".":kind==1?"We will not make war on "+NationName(target)+" before turn "+(turn+turns)+".":(p==null?"The practice":p.name)+" of "+(c==null?"the city":c.name)+" will be kept, whatever the year, until turn "+(turn+turns)+".";}
 public string DeclareVow(int owner,int kind,int cityId,int target,int turns){string why=VowProblem(owner,kind,cityId,target,turns);if(why!="")return why;if(kind==1)cityId=-1;else target=-1;
  var v=new Vow{id=nextWayId++,owner=owner,kind=kind,city=cityId,target=target,start=turn,due=turn+turns,text=VowText(owner,kind,cityId,target,turns)};vows.Add(v);NationWayOf(owner).vowKeys.Add(VowKey(kind,cityId,target));
  WayTell(NationName(owner)+" vowed in public: "+v.text,owner);if(owner!=0&&met[owner])Note(NationName(owner)+" has vowed in public: "+v.text);return "";}
 // What keeping it will be worth (percent): each repeat of the same vow about the same place or people is worth half the last.
 public int VowWorth(Vow v){if(v.kind==3)return 100;int repeats=Math.Max(0,VowsMadeOn(v.owner,v.kind,v.city,v.target)-1);return Math.Max(0,(100*Math.Min(2*Lifeways.VowMin,v.due-v.start)/Lifeways.VowMin)>>repeats);}
 void EndVow(Vow v,int status,string how){if(v.status!=0)return;v.status=status;v.endedTurn=turn;v.ending=how;var nw=NationWayOf(v.owner);if(v.kind!=3)nw.lastVowEnd=turn;var c=cities.Find(x=>x.id==v.city);
  var mine=cities.Where(x=>x.owner==v.owner).ToList();var witnesses=Enumerable.Range(0,civCount).Where(o=>o!=v.owner&&!IsNativeNation(o)&&HasContact(o,v.owner)).ToList();
  if(status==1){int worth=VowWorth(v);
   foreach(var x in v.kind==1||v.kind==3?mine:mine.Where(x=>x.id==v.city))Sway(WayOf(x),Lifeways.VowConviction*worth/100);nations[v.owner].influence+=Lifeways.VowInfluence*worth/100;
   foreach(int o in witnesses)SetTrust(o,v.owner,Trust(o,v.owner)+(v.kind==1&&o==v.target?2:1)*Lifeways.VowTrust*worth/100);if(v.kind==1&&worth>0&&HasContact(v.target,v.owner))AddOpinion(v.target,v.owner,OpinionMods.KeptWord,DiplomacyTuning.KeptMark);
   if(v.kind==3){WayPay(v.owner,-Lifeways.OpeningGold);nw.openingDone=true;}
   WayTell(NationName(v.owner)+" kept its vow: "+v.text+(v.kind==3?" (+"+Lifeways.OpeningGold+" gold)":worth<100?" (the same vow made again counts for less)":""),v.owner);}
  else if(status==2){foreach(var x in mine)Sway(WayOf(x),-15);foreach(int o in witnesses){SetTrust(o,v.owner,Trust(o,v.owner)+Lifeways.VowBreachTrust);AddOpinion(o,v.owner,OpinionMods.Broken,v.kind==1&&o==v.target?Lifeways.VowBreachMark:Lifeways.VowBreachMark/2);}
   if(v.kind==3)nw.openingDone=true;WayTell(NationName(v.owner)+" broke its vow ("+how+"): "+v.text,v.owner);}
  else if(status==3){WayPay(v.owner,Lifeways.ReleaseCost);if(c!=null)Sway(WayOf(c),-5);WayTell(NationName(v.owner)+" asked to be released from its vow and paid "+Lifeways.ReleaseCost+" gold: "+v.text,v.owner);}
  else WayTell("A vow of "+NationName(v.owner)+" can no longer be kept or broken ("+how+"): "+v.text,v.owner);}
 public string ReleaseProblem(int id){var v=vows.Find(x=>x.id==id&&x.status==0);if(v==null)return "No such vow in force.";if(v.kind==3)return "The first vow cannot be handed back.";if(nations[v.owner].gold<Lifeways.ReleaseCost)return "Release costs "+Lifeways.ReleaseCost+" gold.";return "";}
 public string ReleaseVow(int id){string why=ReleaseProblem(id);if(why!="")return why;EndVow(vows.Find(x=>x.id==id),3,"released");return "";}
 // War and vows: the attacker breaks a vow of peace; a vow of peace toward an attacker is made impossible by the attack.
 internal void WayWar(int attacker,int defender){if(!lifeways||vows==null)return;foreach(var v in vows.Where(v=>v.status==0&&v.kind==1).ToList()){if(v.owner==attacker&&v.target==defender)EndVow(v,2,"war was declared on "+NationName(defender));else if(v.owner==defender&&v.target==attacker)EndVow(v,4,NationName(attacker)+" attacked first");}}
 public bool VowForbidsWar(int a,int b){return lifeways&&vows!=null&&vows.Any(v=>v.status==0&&v.kind==1&&v.owner==a&&v.target==b);}
 bool OpeningMet(int owner,int goal){var cap=cities.Find(x=>x.id==nations[owner].capital)??cities.FirstOrDefault(x=>x.owner==owner);if(cap==null)return false;
  switch(goal){case 0:return cap.pop>=4;case 1:return cap.granary;case 2:return units.Any(u=>u.owner==owner&&TilesAround(u.x,u.y,1).Any(t=>t.gateLink>0));default:return cities.Count(x=>x.owner==owner)>=2;}}
 void VowsTurn(){
  foreach(var v in vows.Where(v=>v.status==0).ToList()){var c=cities.Find(x=>x.id==v.city);
   if(v.kind==3){var nw=NationWayOf(v.owner);if(nw.openingMet||OpeningMet(v.owner,v.target)){nw.openingMet=true;EndVow(v,1,"");}else if(turn>=v.due)EndVow(v,2,"the turn came and it was not done");continue;}
   if(v.kind!=1&&(c==null||c.owner!=v.owner)){EndVow(v,4,"the city was lost");continue;}
   if(v.kind==1&&!cities.Any(x=>x.owner==v.target)){EndVow(v,4,NationName(v.target)+" is no more");continue;}
   if(v.kind==0&&WayOf(c).starved){EndVow(v,2,"a citizen of "+c.name+" starved");continue;}
   if(v.kind==2&&!practices.Any(p=>p.city==v.city&&p.status==0)){EndVow(v,2,"the practice lapsed");continue;}
   if(turn>=v.due)EndVow(v,1,"");}
  foreach(var w in ways)w.starved=false;}
 internal void VowsTurnForChecks(){VowsTurn();}
 // The first question: read from the land around the capital on the first turns.
 void OpeningTurn(){var nw=NationWayOf(0);if(nw.opening>=0||turn>3)return;var cap=cities.FirstOrDefault(x=>x.owner==0);if(cap==null)return;var tile=At(cap.x,cap.y);
  bool gate=TilesAround(cap.x,cap.y,7).Any(t=>t.gateLink>0&&t.realm==tile.realm);int kind=NearRiver(tile)?0:Yield(cap)[0]<=1?1:gate?2:3;nw.opening=kind;nw.openingDue=turn+(kind==2?40:kind==1?35:30);
  WayRaise(new WayChoice{kind=6,owner=0,a=kind,b=nw.openingDue});}
 // ---- The turn ----
 internal void LifewaysTurn(){
  if(!lifeways)return;WayGuard();OpeningTurn();
  foreach(var c in cities.ToList()){if(IsNativeNation(c.owner)&&Dormant(c.owner))continue;bool player=c.owner==0;if(!player&&(turn+c.id)%3!=0)continue;var l=WayOf(c);
   int net=Yield(c)[0];bool lean=net<0&&c.food+net*Lifeways.LeanTurns<0;l.shortage=lean?l.shortage+1:0;if(l.sour>0)l.sour=Math.Max(0,l.sour-(player?1:3));
   if(lean&&l.shortage>=(player?2:1)&&turn-l.reliefTurn>=Lifeways.ReliefGap&&turn-l.enduredTurn>=Lifeways.ReliefGap)WayRaise(new WayChoice{kind=0,owner=c.owner,city=c.id});
   if(l.reliefTurn>=0&&turn-l.reliefTurn>=Lifeways.RememberAfter&&turn-l.reliefTurn<Lifeways.RememberAfter+3&&!practices.Any(p=>p.city==c.id&&(p.status==0||p.founded>=l.reliefTurn)))WayRaise(new WayChoice{kind=1,owner=c.owner,city=c.id,a=l.reliefTurn});
   if((turn+c.id)%Lifeways.EmergeEvery==0)WaysEmerge(c);if((turn+c.id)%Lifeways.SpreadEvery==0)WaysSpread(c);}
  foreach(var p in practices.Where(p=>p.status==0&&turn>=p.nextDue).ToList()){var c=cities.Find(x=>x.id==p.city);
   if(c==null||c.owner!=p.owner){p.status=1;continue;}bool lean=WayOf(c).shortage>0||!CanPayPractice(p);
   if(!lean){PayPractice(p);KeepPractice(p,Lifeways.KeptConviction);}else WayRaise(new WayChoice{kind=2,owner=p.owner,city=c.id,a=p.id});}
  for(int o=0;o<civCount;o++){if(IsNativeNation(o))continue;var nw=NationWayOf(o);
   for(int t=0;t<Lifeways.Count;t++)if(nw.policy[t]==1){int cost=Followers(o,t).Count()*Lifeways.FavourCost;if(cost==0)continue;if(nations[o].gold>=cost){WayPay(o,cost);foreach(var c in Followers(o,t))Sway(WayOf(c),1);}else{nw.policy[t]=0;WayTell(NationName(o)+" can no longer pay to favour "+Lifeways.Names[t]+".",o);}}
   if(o==0&&!nw.voiceHeard&&openedGates!=null&&openedGates.Count>0)WayRaise(new WayChoice{kind=3,owner=0});
   if(o==0&&nw.opening>=0&&nw.openingAnswer==1&&!nw.openingDone){if(OpeningMet(0,nw.opening)){nw.openingDone=true;WayPay(0,-Lifeways.OpeningGold/2);WayTell("Your people saw the first question answered: "+Lifeways.OpeningGoals[nw.opening].ToLowerInvariant()+" (+"+Lifeways.OpeningGold/2+" gold).",0);}else if(turn>=nw.openingDue)nw.openingDone=true;}
   // Other rulers vow on the same terms: now and then one swears peace toward the neighbour it thinks best of.
   if(o>0&&(turn+o*7)%20==0&&cities.Any(c=>c.owner==o)){int best=-1,high=10;for(int k=0;k<civCount;k++)if(k!=o&&!IsNativeNation(k)&&HasContact(o,k)&&!AtWar(o,k)&&Opinion(o,k)>high&&cities.Any(c=>c.owner==k)){high=Opinion(o,k);best=k;}if(best>=0)DeclareVow(o,1,-1,best,20);}}
  VowsTurn();}
 // A scenario for trying the First Granary at once: a capital that cannot feed itself and a second city with full stores.
 public static World GranaryScenario(int seed=2311){var w=World.New(seed,"Granary",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);w.lifeways=true;var cap=w.Found(w.units.First(u=>u.owner==0&&u.type==0));
  var spot=w.tiles.Where(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.owner<=0&&w.Distance(t.x,t.y,cap.x,cap.y)>=4&&w.Distance(t.x,t.y,cap.x,cap.y)<=6&&!w.cities.Any(c=>c.id!=cap.id&&w.Distance(c.x,c.y,t.x,t.y)<4)).OrderBy(t=>w.Distance(t.x,t.y,cap.x,cap.y)).ThenBy(t=>t.y*w.Width+t.x).ToList();
  foreach(var t in spot){var s=w.Add(0,0,t.x,t.y);var second=w.Found(s);if(second!=null){second.food=40;break;}w.units.Remove(s);}
  cap.pop=6;cap.entertainers=5;cap.food=3;w.nations[0].gold=300;w.gold=300;var nw=w.NationWayOf(0);nw.opening=3;nw.openingDone=true;return w;}
 bool LifewaysValid(){WayGuard();if(ways.Any(w=>w==null||w.adherence==null||w.adherence.Length!=Lifeways.Count||w.tradition<-1||w.tradition>=Lifeways.Count||w.conviction<0||w.conviction>100))return false;
  if(nationWays.Any(n=>n==null||n.policy==null||n.policy.Length!=Lifeways.Count||n.commitment==null||n.commitment.Length!=Lifeways.Count||n.breaches==null||n.renewTurn==null||n.vowKeys==null))return false;
  if(practices.Any(p=>p==null||p.kind<0||p.kind>2||p.status<0||p.status>1||p.name==null)||vows.Any(v=>v==null||v.kind<0||v.kind>3||v.status<0||v.status>4||v.text==null))return false;
  return wayChoices.All(e=>e!=null&&e.kind>=0&&e.kind<Lifeways.ChoiceNames.Length)&&wayChronicle.Count<=Lifeways.ChronicleMax&&wayChronicle.All(s=>s!=null);}
}
}
