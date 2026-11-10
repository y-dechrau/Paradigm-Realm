using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Diplomacy, rebuilt from the ground up (owner request, 10 Nov 2026; save version 40) after the system in Stellaris (Paradox).
// The whole of it is six ideas, each one number or one word the player can read on the screen:
//  1. OPINION (-100..100): a list of named reasons that add up (OpinionLines). Deeds leave marks that fade with time.
//  2. TRUST (0..100): grows slowly while agreements stand, up to a ceiling set by the strongest agreement, and is half an opinion
//     point each. Friendship therefore has to be built in steps; it cannot be bought in one turn.
//  3. ATTITUDE: one word a leader holds toward another (Friendly, Wary, Threatened, Domineering...), worked out from opinion,
//     relative power and fear. It says at a glance what that leader will and will not agree to.
//  4. AGREEMENTS: five standing pacts (open borders, non-aggression, commercial, research, defensive). They have no end date:
//     they last until one side ends them, which needs ten turns in force and gives ten turns' notice before any war.
//     Every proposal shows the exact reasons for its answer before it is sent (AcceptanceLines).
//  5. RIVALS AND CLAIMS: naming a rival or claiming a city (with influence) is a public warning and, three turns later, a just
//     cause for war. War without a cause is allowed, and frightens everyone who hears of it.
//  6. WAR EXHAUSTION AND THREAT: each side of a war tires (0..100) with time and losses, and peace is judged on it. Conquest
//     makes the conqueror a threat to everyone watching, and leaders who fear the same power seek each other out.
public enum TreatyKind { Access, NonAggression, Defensive, Research, Trade, Peace, Commercial, Retired }
public static class Pacts {
 public const int Borders=0,NonAggression=1,Commercial=2,Research=3,Defensive=4,Count=5;
 public static readonly string[] Names={"Open borders","Non-aggression pact","Commercial pact","Research agreement","Defensive pact"};
 public static readonly string[] Effects={
  "Units of both sides may cross each other's land.",
  "Neither side may declare war on the other. Ending it gives ten turns' notice.",
  "Both treasuries gain gold every turn, more as the smaller partner grows.",
  "Both gain +2 science a turn, +1 more for a partner who knows more.",
  "If either is attacked, the other joins the war. Includes open borders."};
 // How high trust can grow under each agreement, and how reluctant a leader is to sign it before opinion is counted.
 public static readonly int[] TrustCap={20,50,40,40,100};
 public static readonly int[] Reluctance={10,-15,-20,-25,-50};
 public static readonly TreatyKind[] Kind={TreatyKind.Access,TreatyKind.NonAggression,TreatyKind.Commercial,TreatyKind.Research,TreatyKind.Defensive};
 public static int Of(TreatyKind k){for(int i=0;i<Count;i++)if(Kind[i]==k)return i;return -1;}
 public const int MinTurns=10,Notice=10,Truce=10,CauseDelay=3,EnvoyCap=30,EnvoyTrust=20,MaxRivals=2,MaxClaims=3,InfluenceCap=200;
}
// Marks that deeds leave on one leader's opinion of another. Each fades toward nothing.
public static class OpinionMods {
 public const int Dealings=0,Gifts=1,Grievance=2,War=3,Conquest=4,Borders=5,Envoy=6,Broken=7,Count=8;
 public static readonly string[] Names={"Past dealings","Gifts","Grievances","Declared war on us","Took our cities","Settled against our borders","Their envoy at our court","Broke an agreement"};
}
public static class Attitudes {
 public const int Hostile=0,Unfriendly=1,Wary=2,Neutral=3,Cordial=4,Friendly=5,Loyal=6,Threatened=7,Domineering=8,Rival=9,AtWar=10;
 public static readonly string[] Names={"Hostile","Unfriendly","Wary","Neutral","Cordial","Friendly","Loyal","Threatened","Domineering","Rival","At war"};
 public static readonly string[] Texts={
  "Will sign nothing. Expect rivalry, claims and war.",
  "Refuses most agreements. Mend relations first.",
  "Watching you. Will open borders and trade; little more yet.",
  "Open to borders and trade. Pacts need a better opinion.",
  "Ready for non-aggression, commerce and research.",
  "Will sign almost anything, and may defend you.",
  "Bound to you by a defensive pact.",
  "Fears you. Wants non-aggression, and allies against you.",
  "Thinks you weak. Refuses pacts and may press claims.",
  "Has named you a rival. No agreements are possible.",
  "Only peace can be discussed."};
 public static readonly string[] Power={"Pathetic","Inferior","Equivalent","Superior","Overwhelming"};
}
[Serializable] public class Claim { public int owner,city,turn; }
[Serializable] public class DiplomaticPair {
 public bool atWar; public int a,b,warStarted,peaceUntil,lastA=-10,lastB=-10; public bool contact;
 // trustA is a's trust in b (0..100); alarmA is the threat a feels from b (0..100).
 public int trustA,trustB,alarmA,alarmB;
 // Save version 40. pacts holds one bit per agreement (Pacts) and pactSince the turn each was signed. modsA are the marks on
 // a's opinion of b (OpinionMods). rivalA is the turn a named b a rival, or -1. envoyA is 1 while a keeps an envoy at b's court.
 // exhaustA is a's exhaustion in the war between them.
 public int pacts; public int[] pactSince=new int[Pacts.Count];
 public int[] modsA=new int[OpinionMods.Count],modsB=new int[OpinionMods.Count];
 public int rivalA=-1,rivalB=-1,envoyA,envoyB,exhaustA,exhaustB;
 // Save version 36: a religious treaty (no missionaries or funded clergy between the two) lasts until this turn.
 public int faithTreatyUntil;
 public List<string> events=new List<string>();
 // Read only when a game saved before version 40 is upgraded (World.UpgradeDiplomacy40), then left at zero.
 public int grievanceA,grievanceB,accessUntil,friendUntil,allianceUntil,researchUntil,promiseAUntil,promiseBUntil,denounceA=-1,denounceB=-1,embargoUntil,improvedA=-10,improvedB=-10;
 public bool embassyA,embassyB,coalitionA,coalitionB;
}
[Serializable] public class DiplomaticOffer {
 public int id,from,to,duration=15,created,expires,status,target=-1,giveGold,askGold,givePerTurn,askPerTurn,giveResource,askResource;
 public TreatyKind kind; public string answer="";
 public DiplomaticOffer Copy(){return (DiplomaticOffer)MemberwiseClone();}
}
[Serializable] public class DiplomaticContract {
 public int id,from,to,start,end,givePerTurn,askPerTurn,giveResource,askResource;
}
public partial class World {
 public List<DiplomaticPair> diplomacy=NewDiplomacy(3);
 public List<DiplomaticOffer> offers=new List<DiplomaticOffer>();
 public List<DiplomaticContract> contracts=new List<DiplomaticContract>();
 public List<Claim> claims=new List<Claim>();
 // warWeariness is no longer used (war exhaustion is kept for each war, on the pair); it stays so older saves load.
 public int nextDiplomaticId=1; public int[] warWeariness=new int[3]; public bool rivalWar;
 // Pairs are stored in order (0,1),(0,2)...(1,2)... so Relation can find one by index.
 static List<DiplomaticPair> NewDiplomacy(int n){var list=new List<DiplomaticPair>();for(int a=0;a<n-1;a++)for(int b=a+1;b<n;b++)list.Add(new DiplomaticPair{a=a,b=b});return list;}
 public void MigrateDiplomacy(){diplomacy=NewDiplomacy(3);offers=new List<DiplomaticOffer>();contracts=new List<DiplomaticContract>();warWeariness=new int[3];nextDiplomaticId=1;rivalWar=true;foreach(var p in diplomacy){p.atWar=p.a!=0&&rivalWar;p.contact=p.a!=0||met[p.b];p.trustB=p.a==0?nations[p.b].relations:0;p.warStarted=AtWar(p.a,p.b)?turn:0;}}
 public DiplomaticPair Relation(int a,int b){int lo=Math.Min(a,b),hi=Math.Max(a,b);if(lo>=0&&hi<civCount&&lo!=hi&&diplomacy!=null&&diplomacy.Count==civCount*(civCount-1)/2){var q=diplomacy[lo*(2*civCount-lo-1)/2+(hi-lo-1)];if(q.a==lo&&q.b==hi)return q;}return diplomacy.Find(p=>p.a==lo&&p.b==hi);}
 public bool HasContact(int a,int b){return a>=0&&b>=0&&a<civCount&&b<civCount&&a!=b&&(a==0||b==0?met[a==0?b:a]:Relation(a,b).contact);}
 static bool SamePair(int a,int b,int c,int d){return a==c&&b==d||a==d&&b==c;}
 static int Clamp(int v,int lo,int hi){return v<lo?lo:v>hi?hi:v;}
 int Treasury(int owner){return owner==0?gold:nations[owner].gold;}
 void AddDiplomaticGold(int owner,int amount){nations[owner].gold=Treasury(owner)+amount;if(owner==0)gold=nations[0].gold;}
 void DiplomacyEvent(int a,int b,string message){var p=Relation(a,b);p.events.Insert(0,"Turn "+turn+": "+message);if(p.events.Count>12)p.events.RemoveAt(12);if(a==0||b==0||met[a]&&met[b])Note(NationName(a)+" / "+NationName(b)+": "+message);}
 public string Personality(int owner){return owner==0?"Your decisions shape your reputation.":new[]{"Mercantile: values trade and reliable payments.","Scholarly: favours research cooperation.","Martial: respects strength and keeps its word to allies.","Builder: prefers peace and steady growth.","Expansionist: watches its neighbours closely."}[(owner-1)%5];}

 // ---------- agreements ----------
 public bool Pact(int a,int b,int k){var p=Relation(a,b);return p!=null&&(p.pacts&(1<<k))!=0;}
 void SetPact(int a,int b,int k,bool on){var p=Relation(a,b);if(on){p.pacts|=1<<k;p.pactSince[k]=turn;}else p.pacts&=~(1<<k);if(a==0||b==0)openBorders[a==0?b:a]=(p.pacts&(1<<Pacts.Borders|1<<Pacts.Defensive))!=0;networks=null;}
 public int PactAge(int a,int b,int k){return Pact(a,b,k)?turn-Relation(a,b).pactSince[k]:0;}
 // A standing bond: a non-aggression or defensive pact. War between the two is impossible while it lasts.
 public bool Friends(int a,int b){return Pact(a,b,Pacts.NonAggression)||Pact(a,b,Pacts.Defensive);}
 public bool Allied(int a,int b){return Pact(a,b,Pacts.Defensive);}
 public bool HasAccess(int a,int b){var p=Relation(a,b);return p!=null&&!AtWar(a,b)&&(Pact(a,b,Pacts.Borders)||Pact(a,b,Pacts.Defensive)||(a==0||b==0)&&openBorders[a==0?b:a]);}
 public string PactEndReason(int a,int b,int k){if(!HasContact(a,b)||result!="")return "Not possible.";if(!Pact(a,b,k))return "No such agreement.";int age=PactAge(a,b,k);if(age<Pacts.MinTurns)return "Agreements hold for at least "+Pacts.MinTurns+" turns ("+(Pacts.MinTurns-age)+" to go).";return "";}
 // Ending an agreement is remembered as a broken word; ending a bond that kept the peace also starts ten turns' notice.
 public bool EndTreaty(int a,int b,TreatyKind kind){
  if(kind==TreatyKind.Trade){if(!HasContact(a,b)||result!=""||contracts.RemoveAll(c=>SamePair(c.from,c.to,a,b))==0)return false;AddOpinion(b,a,OpinionMods.Broken,-8);DiplomacyEvent(a,b,"Trade deal broken by "+NationName(a)+".");return true;}
  int k=Pacts.Of(kind);if(k<0||PactEndReason(a,b,k)!="")return false;var p=Relation(a,b);bool bond=k==Pacts.NonAggression||k==Pacts.Defensive;
  SetPact(a,b,k,false);AddOpinion(b,a,OpinionMods.Broken,k==Pacts.Defensive?-25:bond?-15:-8);SetTrust(b,a,Trust(b,a)-(bond?20:10));
  if(bond&&!Friends(a,b))p.peaceUntil=Math.Max(p.peaceUntil,turn+Pacts.Notice);
  if(b==0)AddRivalCall(a,0,"has ended our "+Pacts.Names[k].ToLower()+".");
  DiplomacyEvent(a,b,Pacts.Names[k]+" ended by "+NationName(a)+(bond&&!Friends(a,b)?": ten turns' notice before any war.":"."));return true;}
 public int DiplomaticScience(int owner){int s=0;foreach(var p in diplomacy){if(p.a!=owner&&p.b!=owner||(p.pacts&(1<<Pacts.Research))==0||AtWar(p.a,p.b))continue;int other=p.a==owner?p.b:p.a;s+=2+(TechCount(other)>TechCount(owner)?1:0);}return s;}
 int TechCount(int owner){int n=0;var t=nations[owner].tech;if(t!=null)for(int i=0;i<t.Length;i++)if(t[i])n++;return n;}
 // A commercial pact pays both sides: 2 gold, +1 for every two cities of the smaller partner, at most 8.
 public int CommercialGold(int owner){int g=0;int[] count=null;foreach(var p in diplomacy){if(p.a!=owner&&p.b!=owner||(p.pacts&(1<<Pacts.Commercial))==0||AtWar(p.a,p.b))continue;if(count==null){count=new int[civCount];foreach(var c in cities)count[c.owner]++;}g+=Math.Min(8,2+Math.Min(count[p.a],count[p.b])/2);}return g;}

 // ---------- opinion ----------
 int[] Mods(int observer,int other){var p=Relation(observer,other);return observer==p.a?p.modsA:p.modsB;}
 public int OpinionMod(int observer,int other,int kind){return Mods(observer,other)[kind];}
 // Leaves a mark on the observer's opinion of the other. Positive marks cannot go below nothing, nor negative ones above it.
 public void AddOpinion(int observer,int other,int kind,int amount){
  if(observer==other||Relation(observer,other)==null)return;var m=Mods(observer,other);bool either=kind==OpinionMods.Dealings||kind==OpinionMods.Envoy;
  m[kind]=Clamp(m[kind]+amount,either||kind!=OpinionMods.Gifts?-100:0,either||kind==OpinionMods.Gifts?100:0);}
 // An offence against the victim (the old grievance scale: two points of grievance cost one of opinion).
 void Grieve(int victim,int offender,int amount){AddOpinion(victim,offender,OpinionMods.Grievance,-(amount+1)/2);}
 public int Grievances(int observer,int other){var m=Mods(observer,other);int s=0;for(int k=0;k<m.Length;k++)if(m[k]<0)s-=m[k];return s;}
 public int Trust(int observer,int other){var p=Relation(observer,other);return p==null?0:observer==p.a?p.trustA:p.trustB;}
 internal void SetTrust(int observer,int other,int v){var p=Relation(observer,other);if(p==null)return;v=Clamp(v,0,100);if(observer==p.a)p.trustA=v;else p.trustB=v;}
 // The highest trust the observer can come to feel: set by the strongest agreement, or by the other's envoy at its court.
 public int TrustCap(int observer,int other){var p=Relation(observer,other);if(p==null||AtWar(observer,other))return 0;int cap=0;for(int k=0;k<Pacts.Count;k++)if((p.pacts&(1<<k))!=0)cap=Math.Max(cap,Pacts.TrustCap[k]);if(Envoy(other,observer))cap=Math.Max(cap,Pacts.EnvoyTrust);return cap;}
 public int Threat(int observer,int target){var p=Relation(observer,target);return p==null?0:observer==p.a?p.alarmA:p.alarmB;}
 internal void SetThreat(int observer,int target,int v){var p=Relation(observer,target);if(p==null)return;v=Clamp(v,0,100);if(observer==p.a)p.alarmA=v;else p.alarmB=v;}
 // Scaled by how much the observer hates warmongers (5 = as given).
 internal void AddThreat(int observer,int target,int amount){if(observer==target||!HasContact(observer,target))return;SetThreat(observer,target,Threat(observer,target)+amount*Flavour(observer,DiplomacyFlavours.WarmongerHate)/5);}
 public bool Rival(int a,int b){var p=Relation(a,b);return p!=null&&(a==p.a?p.rivalA:p.rivalB)>=0;}
 public bool Rivals(int a,int b){var p=Relation(a,b);return p!=null&&(p.rivalA>=0||p.rivalB>=0);}
 // Rivalry closes trade between the two, in both directions.
 public bool Embargoed(int a,int b){return Rivals(a,b);}
 public bool Envoy(int a,int b){var p=Relation(a,b);return p!=null&&(a==p.a?p.envoyA:p.envoyB)==1;}
 static void Part(List<KeyValuePair<string,int>> lines,ref int sum,string text,int v){if(v==0)return;sum+=v;if(lines!=null)lines.Add(new KeyValuePair<string,int>(text,v));}
 // The observer's opinion of the other with the reasons behind it; the reasons add up exactly to Opinion (before the limit).
 int OpinionSum(int o,int t,List<KeyValuePair<string,int>> lines){
  var p=Relation(o,t);if(p==null)return 0;int sum=0;var m=o==p.a?p.modsA:p.modsB;
  for(int k=0;k<OpinionMods.Count;k++)Part(lines,ref sum,OpinionMods.Names[k],m[k]);
  Part(lines,ref sum,"Trust ("+Trust(o,t)+")",Trust(o,t)/2);
  {int bonds=0,n=0;int[] worth={5,10,5,5,20};for(int k=0;k<Pacts.Count;k++)if((p.pacts&(1<<k))!=0){bonds+=worth[k];n++;}if(n>0)Part(lines,ref sum,n==1?"An agreement between us":n+" agreements between us",bonds);}
  if(AtWar(o,t))Part(lines,ref sum,"At war",-50);
  if(Rival(t,o))Part(lines,ref sum,"Named us a rival",-30);
  if(Rival(o,t))Part(lines,ref sum,"We named them a rival",-30);
  {int n=0;foreach(var c in claims)if(c.owner==t){var city=CityById(c.city);if(city!=null&&city.owner==o)n++;}if(n>0)Part(lines,ref sum,n==1?"Claims one of our cities":"Claims "+n+" of our cities",-Math.Min(30,10*n));}
  Part(lines,ref sum,"Fears their conquests (threat "+Threat(o,t)+")",-Threat(o,t)/2);
  {bool war=false,rival=false,fear=false;for(int x=0;x<civCount;x++){if(x==o||x==t)continue;if(AtWar(o,x)&&AtWar(t,x))war=true;if(Rival(o,x)&&Rival(t,x))rival=true;if(Threat(o,x)>=40&&Threat(t,x)>=40)fear=true;}
   if(war)Part(lines,ref sum,"Fighting the same enemy",10);if(rival)Part(lines,ref sum,"Rival of our rival",15);if(fear)Part(lines,ref sum,"We fear the same power",15);}
  Part(lines,ref sum,"Old grudges",-GrudgePenalty(o,t));
  if(nations[o].government==nations[t].government)Part(lines,ref sum,"Same government",5);
  if(t==0&&ActiveRival(o))Part(lines,ref sum,"Wary of you (difficulty)",DifficultyTable.Attitude[Level]);
  {var e=Civ(o);int core=AgendaCore(o,t);if(core!=0&&e!=null&&e.Agenda>=0)Part(lines,ref sum,"Agenda: "+CivLore.AgendaNames[e.Agenda],core);}
  if(o!=t&&CivId(t)==33&&CivId(o)!=33)Part(lines,ref sum,"Distrusts demons",-10);
  sum+=RulerOpinion(o,t,lines);
  if(modernRules)FaithOpinion(o,t,lines,ref sum);
  return sum;
 }
 public int Opinion(int observer,int other){return Clamp(OpinionSum(observer,other,null),-100,100);}
 public List<KeyValuePair<string,int>> OpinionLines(int observer,int other){var lines=new List<KeyValuePair<string,int>>();OpinionSum(observer,other,lines);return lines.OrderBy(l=>-Math.Abs(l.Value)).ToList();}
 public string RelationshipBreakdown(int observer,int other){return "Opinion "+Opinion(observer,other)+" | Trust "+Trust(observer,other)+" of "+TrustCap(observer,other)+" | Attitude "+Attitudes.Names[Attitude(observer,other)]+". "+string.Join("; ",OpinionLines(observer,other).Select(l=>l.Key+" "+(l.Value>0?"+":"")+l.Value).ToArray())+".";}

 // ---------- relative power and attitude ----------
 int KnownMilitary(int observer,int other){return Math.Max(1,units.Where(u=>u.owner==other&&(other==observer||VisibleTo(observer,u.x,u.y))).Sum(u=>MilitaryValue(u.type)));}
 // What the observer can count of the other's strength: the soldiers it sees and four for each city.
 int Strength(int observer,int other){return KnownMilitary(observer,other)+4*cities.Count(c=>c.owner==other);}
 // The other's strength beside the observer's own: 0 Pathetic, 1 Inferior, 2 Equivalent, 3 Superior, 4 Overwhelming.
 public int PowerLevel(int observer,int other){int r=Strength(observer,other)*100/Math.Max(1,Strength(observer,observer));return r<50?0:r<80?1:r<=125?2:r<=200?3:4;}
 [NonSerialized] Dictionary<int,int> attitudeMemo;[NonSerialized] int attitudeTurn=-1;
 // The one word the observer holds toward the other. Remembered for the rest of the turn when asked through AttitudeNow.
 public int Attitude(int o,int t){
  if(AtWar(o,t))return Attitudes.AtWar;if(Rival(o,t))return Attitudes.Rival;if(Allied(o,t))return Attitudes.Loyal;
  int op=Opinion(o,t),power=PowerLevel(o,t);
  if(Threat(o,t)>=50||power>=4&&op<10)return Attitudes.Threatened;
  if(op<=-50)return Attitudes.Hostile;
  if(o!=0&&power==0&&op<30&&Flavour(o,DiplomacyFlavours.Boldness)>=6)return Attitudes.Domineering;
  if(op<=-20)return Attitudes.Unfriendly;if(op>=50)return Attitudes.Friendly;if(op>=20)return Attitudes.Cordial;
  if(power>=3&&op<10||o!=0&&Ambition(o,t)>=35)return Attitudes.Wary;
  return Attitudes.Neutral;}
 int AttitudeNow(int o,int t){if(attitudeMemo==null||attitudeTurn!=turn){attitudeMemo=new Dictionary<int,int>();attitudeTurn=turn;}int v;int key=o*1000+t;if(!attitudeMemo.TryGetValue(key,out v)){v=Attitude(o,t);attitudeMemo[key]=v;}return v;}

 // ---------- envoys ----------
 public int EnvoyLimit(int owner){return Math.Min(3,1+(Era(owner)+1)/2);}
 public int EnvoysOut(int owner){int n=0;foreach(var p in diplomacy)if(p.a==owner&&p.envoyA==1||p.b==owner&&p.envoyB==1)n++;return n;}
 public string EnvoyReason(int a,int b){if(!HasContact(a,b))return "First contact required.";if(result!="")return "The game has ended.";if(Envoy(a,b))return "";if(AtWar(a,b))return "Not while at war.";if(Rivals(a,b))return "Not to a rival's court.";if(EnvoysOut(a)>=EnvoyLimit(a))return "All "+EnvoyLimit(a)+" of your envoys are away. Recall one first.";return "";}
 // An envoy at a court: +1 opinion a turn up to +30, their fear of you fades faster, and trust can reach 20 without any pact.
 public bool SetEnvoy(int a,int b,bool on){if(!HasContact(a,b)||result!="")return false;if(on&&(Envoy(a,b)||EnvoyReason(a,b)!=""))return false;if(!on&&!Envoy(a,b))return false;var p=Relation(a,b);if(a==p.a)p.envoyA=on?1:0;else p.envoyB=on?1:0;DiplomacyEvent(a,b,on?NationName(a)+" sent an envoy to "+NationName(b)+".":NationName(a)+" recalled its envoy from "+NationName(b)+".");return true;}

 // ---------- rivals ----------
 public int RivalCount(int a){int n=0;foreach(var p in diplomacy)if(p.a==a&&p.rivalA>=0||p.b==a&&p.rivalB>=0)n++;return n;}
 public string RivalReason(int a,int b){if(!HasContact(a,b))return "First contact required.";if(result!="")return "The game has ended.";if(Rival(a,b))return "Already your rival.";if(Relation(a,b).pacts!=0)return "End your agreements with them first.";if(RivalCount(a)>=Pacts.MaxRivals)return "You may name "+Pacts.MaxRivals+" rivals at most.";return "";}
 // A public declaration: trade between the two stops, the namer gains 1 influence a turn, and after three turns it is a just cause for war.
 public bool DeclareRival(int a,int b){if(RivalReason(a,b)!="")return false;var p=Relation(a,b);if(a==p.a)p.rivalA=turn;else p.rivalB=turn;contracts.RemoveAll(c=>SamePair(c.from,c.to,a,b));if(a==p.a)p.envoyA=p.envoyB=0;else p.envoyA=p.envoyB=0;if(b==0)AddRivalCall(a,0,"has named you a rival. War may follow.");networks=null;DiplomacyEvent(a,b,NationName(a)+" named "+NationName(b)+" a rival: trade between them stops.");return true;}
 public string EndRivalReason(int a,int b){var p=Relation(a,b);if(p==null||!Rival(a,b))return "Not your rival.";if(AtWar(a,b))return "Not while at war.";int age=turn-(a==p.a?p.rivalA:p.rivalB);if(age<Pacts.MinTurns)return "A rivalry stands for at least "+Pacts.MinTurns+" turns ("+(Pacts.MinTurns-age)+" to go).";return "";}
 public bool EndRivalry(int a,int b){if(EndRivalReason(a,b)!="")return false;var p=Relation(a,b);if(a==p.a)p.rivalA=-1;else p.rivalB=-1;networks=null;DiplomacyEvent(a,b,NationName(a)+" no longer names "+NationName(b)+" a rival.");return true;}

 // ---------- claims ----------
 City CityById(int id){foreach(var c in cities)if(c.id==id)return c;return null;}
 public bool HasClaim(int a,int cityId){foreach(var c in claims)if(c.owner==a&&c.city==cityId)return true;return false;}
 public List<City> ClaimsOn(int a,int b){var list=new List<City>();foreach(var c in claims)if(c.owner==a){var city=CityById(c.city);if(city!=null&&city.owner==b)list.Add(city);}return list;}
 public int ClaimCost(City c){return Math.Min(60,20+2*c.pop)+(nations[c.owner].capital==c.id?20:0);}
 bool KnowsCity(int a,City c){return a==0?At(c.x,c.y).seen:nations[a].known!=null&&nations[a].known[c.y*Width+c.x];}
 public string ClaimReason(int a,City c){
  if(c==null||c.owner==a)return "Not a foreign city.";int b=c.owner;if(!HasContact(a,b))return "First contact required.";if(result!="")return "The game has ended.";
  if(HasClaim(a,c.id))return "Already claimed.";if(!KnowsCity(a,c))return "You have not found this city.";if(Friends(a,b))return "Not while a pact keeps the peace between you.";
  if(ClaimsOn(a,b).Count>=Pacts.MaxClaims)return "At most "+Pacts.MaxClaims+" claims against one civilisation.";
  if(nations[a].influence<ClaimCost(c))return "Needs "+ClaimCost(c)+" influence ("+nations[a].influence+").";return "";}
 // A claim costs influence, is announced to its owner, and after three turns is a just cause for war. Taking a claimed city
 // alarms the world far less than taking one without a claim.
 public bool MakeClaim(int a,City c){if(ClaimReason(a,c)!="")return false;nations[a].influence-=ClaimCost(c);claims.Add(new Claim{owner=a,city=c.id,turn=turn});if(c.owner==0)AddRivalCall(a,0,"has laid claim to "+c.name+". War may follow.");DiplomacyEvent(a,c.owner,NationName(a)+" laid claim to "+c.name+".");return true;}
 public bool DropClaim(int a,int cityId){return claims.RemoveAll(c=>c.owner==a&&c.city==cityId)>0;}

 // ---------- war and peace ----------
 public string WarReason(int a,int b){if(!HasContact(a,b))return "First contact required.";if(result!="")return "The game has ended.";if(AtWar(a,b))return "Already at war.";var p=Relation(a,b);if(Friends(a,b))return "End the "+(Allied(a,b)?"defensive":"non-aggression")+" pact first; that gives ten turns' notice.";if(p.peaceUntil>turn)return "A truce protects this civilisation for "+(p.peaceUntil-turn)+" turns.";return "";}
 // The just cause a has for war on b, or "" when it has none.
 public string CasusBelli(int a,int b){
  var p=Relation(a,b);if(p==null)return "";
  foreach(var c in claims)if(c.owner==a&&turn-c.turn>=Pacts.CauseDelay){var city=CityById(c.city);if(city!=null&&city.owner==b)return "Claim on "+city.name;}
  int since=a==p.a?p.rivalA:p.rivalB;if(since>=0&&turn-since>=Pacts.CauseDelay)return "Rivalry";
  if(HolyWarJustified(a,b))return "Holy war";
  if(Threat(a,b)>=50)return "Containing a threat";
  return "";}
 // Kept for the war prompt: true when war now would have a just cause.
 public bool FormalWarAvailable(int a,int b){return CasusBelli(a,b)!="";}
 public string CausePending(int a,int b){var p=Relation(a,b);if(p==null||CasusBelli(a,b)!="")return "";int best=-1;foreach(var c in claims)if(c.owner==a){var city=CityById(c.city);if(city!=null&&city.owner==b)best=Math.Max(best,c.turn);}int since=a==p.a?p.rivalA:p.rivalB;best=Math.Max(best,since);return best<0?"":"Just cause in "+Math.Max(1,Pacts.CauseDelay-(turn-best))+" turns.";}
 public bool DeclareWar(int a,int b,bool defensive=false){
  if(WarReason(a,b)!="")return false;string cause=defensive?"Defence of an ally":CasusBelli(a,b);bool just=cause!="";
  if(b==0)AddRivalCall(a,0,"has declared war on you!"+(just?" ("+cause+")":""));
  var defenders=Enumerable.Range(0,civCount).Where(c=>c!=a&&c!=b&&Allied(c,b)).ToArray();
  var p=Relation(a,b);if(a==0||b==0){war[a==0?b:a]=true;openBorders[a==0?b:a]=false;}else p.atWar=true;
  p.warStarted=turn;p.pacts=0;p.envoyA=p.envoyB=0;p.exhaustA=p.exhaustB=0;contracts.RemoveAll(c=>SamePair(c.from,c.to,a,b));networks=null;
  if(!defensive){
   AddOpinion(b,a,OpinionMods.War,just?-20:-40);
   // War without a cause frightens everyone who hears of it; a just war worries only the victim.
   if(just)AddThreat(b,a,4);else for(int o=0;o<civCount;o++)if(o!=a){AddThreat(o,a,o==b?12:6);if(o!=b&&HasContact(o,a))AddOpinion(o,a,OpinionMods.Grievance,-5);}
  }
  if(a==0)nations[b].relations=Math.Max(-100,nations[b].relations-25);
  DiplomacyEvent(a,b,just?"War declared ("+cause+").":"War declared without cause: the world takes fright.");
  if(!defensive)foreach(int c in defenders){if(c==0||a==0)met[c==0?a:c]=true;else Relation(c,a).contact=true;if(WarReason(c,a)=="")DeclareWar(c,a,true);else if(!AtWar(c,a)){AddOpinion(b,c,OpinionMods.Broken,-15);SetTrust(b,c,Trust(b,c)-20);DiplomacyEvent(c,b,"Defence blocked by another pact or a truce; trust damaged.");}}
  return true;}
 bool MakePeace(int a,int b){if(!HasContact(a,b)||!AtWar(a,b))return false;var p=Relation(a,b);if(a==0||b==0)war[a==0?b:a]=false;else p.atWar=false;p.peaceUntil=turn+Pacts.Truce;p.exhaustA=p.exhaustB=0;networks=null;DiplomacyEvent(a,b,"Peace agreed: each side keeps what it holds. Ten turns of truce.");return true;}
 public bool SetWar(int other,bool value){return value?DeclareWar(0,other):MakePeace(0,other);}
 public bool SetBorders(int other){if(!HasContact(0,other)||AtWar(0,other))return false;if(Pact(0,other,Pacts.Borders))return EndTreaty(0,other,TreatyKind.Access);return Propose(new DiplomaticOffer{from=0,to=other,kind=TreatyKind.Access}).status==1;}
 public bool OfferPeace(int other){return Propose(new DiplomaticOffer{from=0,to=other,kind=TreatyKind.Peace}).status==1;}
 // War exhaustion: one side's weariness of one war, 0..100. It rises 1 a turn, 2 for each unit lost and 10 for each city lost.
 public int Exhaustion(int a,int b){var p=Relation(a,b);return p==null||!AtWar(a,b)?0:a==p.a?p.exhaustA:p.exhaustB;}
 internal void AddExhaustion(int a,int b,int amount){var p=Relation(a,b);if(p==null||!AtWar(a,b))return;if(a==p.a)p.exhaustA=Clamp(p.exhaustA+amount,0,100);else p.exhaustB=Clamp(p.exhaustB+amount,0,100);}
 public int WarExhaustion(int owner){int worst=0;foreach(var p in diplomacy){if(p.a!=owner&&p.b!=owner||!AtWar(p.a,p.b))continue;worst=Math.Max(worst,p.a==owner?p.exhaustA:p.exhaustB);}return worst;}
 // One content place lost in every city for each 25 points of the owner's worst war.
 public int WarUnhappiness(int owner){return Math.Min(4,WarExhaustion(owner)/25);}
 // Called from combat: units each side lost in one fight.
 internal void WarLosses(int a,int b,int lostByA,int lostByB){if(a==b||!AtWar(a,b))return;if(lostByA>0)AddExhaustion(a,b,2*lostByA);if(lostByB>0)AddExhaustion(b,a,2*lostByB);}

 // ---------- what the map does to diplomacy ----------
 // Shown before a settler founds a city: a new city within six tiles of a pact partner's city strains the pact.
 public string SettlementDiplomacyWarning(int owner,int x,int y){
  for(int other=0;other<civCount;other++){if(other==owner||!HasContact(owner,other)||!Friends(owner,other))continue;
   if(cities.Any(c=>c.owner==other&&Distance(c.x,c.y,x,y)<=6&&(VisibleTo(owner,c.x,c.y)||owner==0&&At(c.x,c.y).seen&&At(c.x,c.y).rememberedOwner==other&&!string.IsNullOrEmpty(At(c.x,c.y).rememberedCity))))return "Warning: settling this close to "+NationName(other)+" strains your pact (opinion -8, trust -5).";
  }return "";}
 void DiplomacyOnFound(City city){
  int mine=cities.Count(c=>c.owner==city.owner);
  for(int other=0;other<civCount;other++){if(other==city.owner)continue;
   if(cities.Any(c=>c.owner==other&&Distance(c.x,c.y,city.x,city.y)<=8)&&mine>cities.Count(c=>c.owner==other))AddThreat(other,city.owner,3);
   if(!HasContact(city.owner,other)||!cities.Any(c=>c.owner==other&&Distance(c.x,c.y,city.x,city.y)<=6&&VisibleTo(other,city.x,city.y)))continue;
   bool pact=Friends(city.owner,other);AddOpinion(other,city.owner,OpinionMods.Borders,pact?-8:-3);if(pact)SetTrust(other,city.owner,Trust(other,city.owner)-5);
   DiplomacyEvent(city.owner,other,pact?"A settlement against a partner's border strains the pact.":"A nearby settlement causes border friction.");}}
 internal void DiplomacyOnCapture(int conqueror,int victim,City taken=null){
  bool claimed=taken!=null&&HasClaim(conqueror,taken.id);
  for(int o=0;o<civCount;o++){if(o==conqueror||!HasContact(o,conqueror))continue;int near=taken!=null&&cities.Any(m=>m.owner==o&&Distance(m.x,m.y,taken.x,taken.y)<=10)?claimed?4:10:0;AddThreat(o,conqueror,(claimed?o==victim?10:3:o==victim?24:12)+near);}
  if(modernRules&&taken!=null){int faith=StateReligion(conqueror);if(faith>=0){Grow(taken);taken.faithPressure[faith]+=150;}}
  AddOpinion(victim,conqueror,OpinionMods.Conquest,claimed?-10:-20);AddExhaustion(victim,conqueror,10);AddExhaustion(conqueror,victim,-5);
  foreach(int other in Enumerable.Range(0,civCount).Where(o=>o!=victim&&o!=conqueror&&HasContact(o,conqueror)&&Friends(o,victim)))Grieve(other,conqueror,10);
  if(taken!=null)claims.RemoveAll(c=>c.city==taken.id&&c.owner==conqueror);
  DiplomacyEvent(conqueror,victim,claimed?"A claimed city was taken.":"A city was taken without a claim: the world takes note.");}
}
}
