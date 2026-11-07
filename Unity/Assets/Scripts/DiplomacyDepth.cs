using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Deeper diplomacy (owner request, 8 Nov 2026; modern rules, save version 35). Researched sources and what each one gave:
//  - Civilization VI and VII (Firaxis): opinion is shown as a list of named reasons that add up to the number, so the player can always
//    see why a leader feels as it does (OpinionLines); Civilization VII's Influence is a diplomatic currency spent on actions
//    (Improve relations, Incite) and earned from embassies and friendships.
//  - Crusader Kings III (Paradox): opinion comes from people, not only states. The ruler's traits and stress (LivingWorld.cs) change how
//    the ruler judges others (RulerOpinion).
//  - Europa Universalis IV (Paradox): aggressive expansion. Conquest, surprise wars and crowding settlement alarm everyone who knows
//    the culprit; alarm fades by 1 a turn (faster with an embassy or Improve relations) and three or more alarmed civilisations that do
//    not like the culprit form a coalition that defends its members against it (Alarm, Coalition).
//  - Soren Johnson (Designer Notes; GDC 2022 on Civilization III's bargaining table): leaders who simply trade value for value feel like
//    vending machines and reduce diplomacy to min-maxing. Here the same deal is worth more or less by what the leader needs and what
//    kind of leader it is, and some leaders will not be bought (Need, flavours).
//  - Utility theory in game AI (Dave Mark and Kevin Dill, GDC AI Summit): every deal item is scored by a need curve from the AI's own
//    state: a hungry rival values food far above gold, an indebted one values gold, one at war values iron and horses (GoldNeed,
//    ResourceNeed in DealValue).
//  - Finite state machines for AI personality: each rival is in one posture (Peaceful builder, Expansionist, Aggressor, Defensive) with
//    thresholds to enter and to leave, so it does not flicker; the posture shifts war ambition, deals and army size, and is shown.
//  - Modding communities (Civilization IV LeaderHeadInfos XML, Civilization V and Vox Populi leader flavours, scores 1-10 shifted by
//    up to 2 at random each game): every leader has five flavours from its agenda plus a fixed roll per game (Flavour).

public static class DiplomacyFlavours {
 public static readonly string[] Names={"Hates warmongers","Boldness","Loyalty","Greed","Forgiveness"};
 public const int WarmongerHate=0,Boldness=1,Loyalty=2,Greed=3,Forgiveness=4;
 // Rows follow CivLore.AgendaNames; values 1-10, in the manner of a leader table in a mod's XML.
 public static readonly int[,] ByAgenda={
  {5,4,7,9,6}, // Merchant Prince
  {6,3,6,5,6}, // Seeker of Wisdom
  {7,5,6,4,3}, // Defender of the Faith
  {6,4,5,6,5}, // Builder of Wonders
  {3,8,4,6,4}, // Hungry for Land
  {9,2,8,4,8}, // Keeper of the Peace
  {7,6,6,5,3}, // Warden of the Borders
  {2,9,5,5,4}};// Respects Only Strength
 public static readonly string[] PostureNames={"Peaceful builder","Expansionist","Aggressor","Defensive"};
 public static readonly string[] PostureTexts={"Wants peace and steady growth; open to research and trade.","Settling new land as fast as it can.","Looking for a war it can win.","Feels threatened and is arming."};
 public const int Builder=0,Expansionist=1,Aggressor=2,Defensive=3;
 public const int ImproveCost=20,InciteCost=30,InfluenceCap=200,CoalitionAlarm=50,CoalitionLeave=25,AlarmCap=100;
}
public partial class World {
 // ---------- flavours ----------
 public int Flavour(int owner,int k){var e=Civ(owner);int b=e!=null&&e.Agenda>=0&&e.Agenda<DiplomacyFlavours.ByAgenda.GetLength(0)?DiplomacyFlavours.ByAgenda[e.Agenda,k]:5;int roll=(int)((((uint)(seed*7919+owner*131+k*31))>>4)%5u)-2;return Math.Max(1,Math.Min(10,b+roll));}
 // ---------- alarm at expansion and coalitions ----------
 public int Alarm(int observer,int target){var p=Relation(observer,target);return p==null?0:observer==p.a?p.alarmA:p.alarmB;}
 internal void SetAlarm(int observer,int target,int v){var p=Relation(observer,target);if(p==null)return;v=Math.Max(0,Math.Min(DiplomacyFlavours.AlarmCap,v));if(observer==p.a)p.alarmA=v;else p.alarmB=v;}
 // Scaled by how much the observer hates warmongers (5 = as given).
 internal void AddAlarm(int observer,int target,int amount){if(!modernRules||observer==target||!HasContact(observer,target))return;SetAlarm(observer,target,Alarm(observer,target)+amount*Flavour(observer,DiplomacyFlavours.WarmongerHate)/5);}
 public bool InCoalition(int member,int target){var p=Relation(member,target);return p!=null&&(member==p.a?p.coalitionA:p.coalitionB);}
 internal void SetCoalition(int member,int target,bool v){var p=Relation(member,target);if(member==p.a)p.coalitionA=v;else p.coalitionB=v;}
 public List<int> CoalitionAgainst(int target){var list=new List<int>();for(int o=0;o<civCount;o++)if(o!=target&&InCoalition(o,target))list.Add(o);return list;}
 void AlarmOnCapture(int conqueror,int victim,City c){if(!modernRules)return;for(int o=0;o<civCount;o++){if(o==conqueror||!HasContact(o,conqueror))continue;int near=c!=null&&cities.Any(m=>m.owner==o&&Distance(m.x,m.y,c.x,c.y)<=10)?10:0;AddAlarm(o,conqueror,(o==victim?20:10)+near);}}
 void AlarmOnSurpriseWar(int aggressor,int victim){if(!modernRules)return;for(int o=0;o<civCount;o++)if(o!=aggressor)AddAlarm(o,aggressor,o==victim?12:6);}
 void AlarmOnFound(City city){if(!modernRules)return;int mine=cities.Count(c=>c.owner==city.owner);for(int o=0;o<civCount;o++){if(o==city.owner)continue;if(cities.Any(c=>c.owner==o&&Distance(c.x,c.y,city.x,city.y)<=8)&&mine>cities.Count(c=>c.owner==o))AddAlarm(o,city.owner,3);}}
 internal void UpdateAlarmAndCoalitions(){
  if(!modernRules)return;
  foreach(var p in diplomacy){
   if(p.alarmA>0)p.alarmA=Math.Max(0,p.alarmA-1-(p.embassyB?1:0));
   if(p.alarmB>0)p.alarmB=Math.Max(0,p.alarmB-1-(p.embassyA?1:0));
  }
  for(int t=0;t<civCount;t++){
   int alarmed=0;for(int o=0;o<civCount;o++)if(o!=t&&Alarm(o,t)>=DiplomacyFlavours.CoalitionAlarm)alarmed++;
   for(int o=0;o<civCount;o++){if(o==t||!HasContact(o,t))continue;bool member=InCoalition(o,t);
    bool join=!member&&alarmed>=3&&Alarm(o,t)>=DiplomacyFlavours.CoalitionAlarm&&!Friends(o,t)&&OpinionCore(o,t)<10;
    bool leave=member&&(Alarm(o,t)<DiplomacyFlavours.CoalitionLeave||Allied(o,t));
    if(join){SetCoalition(o,t,true);DiplomacyEvent(o,t,NationName(o)+" joins a coalition against "+NationName(t)+".");if(t==0)AddRivalCall(o,0,"has joined a coalition against you. Members defend each other if you attack one of them.");}
    else if(leave){SetCoalition(o,t,false);DiplomacyEvent(o,t,NationName(o)+" leaves the coalition against "+NationName(t)+".");}}
  }
 }
 // ---------- influence ----------
 public int InfluencePerTurn(int owner){if(!modernRules)return 0;int embassies=0,friends=0;foreach(var p in diplomacy){if(p.a!=owner&&p.b!=owner)continue;if(owner==p.a?p.embassyA:p.embassyB)embassies++;if(Friends(p.a,p.b))friends++;}return 1+embassies+Math.Min(3,friends);}
 public string ImproveReason(int a,int b){if(!modernRules)return "Not in this game's rules.";if(!HasContact(a,b))return "First contact required.";if(AtWar(a,b))return "Not while at war.";var p=Relation(a,b);int last=a==p.a?p.improvedA:p.improvedB;if(turn-last<5)return "Envoys were sent "+(turn-last)+" turns ago; wait "+(5-(turn-last))+" more.";if(nations[a].influence<DiplomacyFlavours.ImproveCost)return "Needs "+DiplomacyFlavours.ImproveCost+" influence ("+nations[a].influence+").";return "";}
 // Envoys: +8 trust and 10 less alarm at your expansion.
 public bool ImproveRelations(int a,int b){if(ImproveReason(a,b)!="")return false;nations[a].influence-=DiplomacyFlavours.ImproveCost;var p=Relation(a,b);if(a==p.a)p.improvedA=turn;else p.improvedB=turn;ChangeTrust(b,a,8);SetAlarm(b,a,Alarm(b,a)-10);DiplomacyEvent(a,b,NationName(a)+" sent envoys: trust +8, alarm -10.");return true;}
 public string InciteReason(int a,int b){if(!modernRules)return "Not in this game's rules.";if(!HasContact(a,b))return "First contact required.";if(Friends(a,b))return "Not against a friend or ally.";if(nations[a].influence<DiplomacyFlavours.InciteCost)return "Needs "+DiplomacyFlavours.InciteCost+" influence ("+nations[a].influence+").";if(InciteListeners(a,b).Count==0)return "Nobody who likes you knows them.";return "";}
 List<int> InciteListeners(int a,int b){var list=new List<int>();for(int o=0;o<civCount;o++)if(o!=a&&o!=b&&HasContact(o,a)&&HasContact(o,b)&&OpinionCore(o,a)>=10)list.Add(o);return list;}
 // Spreading word of a rival's ambition: everyone who likes you grows 10 more alarmed at them; they learn of it (10 grievances).
 public bool Incite(int a,int b){if(InciteReason(a,b)!="")return false;nations[a].influence-=DiplomacyFlavours.InciteCost;foreach(int o in InciteListeners(a,b))AddAlarm(o,b,10);Grieve(b,a,10);DiplomacyEvent(a,b,NationName(a)+" spread word of "+NationName(b)+"'s ambitions.");return true;}
 void InfluenceAI(int a){
  var n=nations[a];
  if(n.influence>=DiplomacyFlavours.ImproveCost){int best=-1,score=int.MinValue;for(int b=0;b<civCount;b++){if(b==a||ImproveReason(a,b)!="")continue;int op=OpinionCore(b,a);if(op<-40||n.posture==DiplomacyFlavours.Aggressor&&Ambition(a,b)>=40)continue;int s=(Alarm(b,a)>=30?40:0)+(InCoalition(b,a)?60:0)+(op<20?20-op:0);if(s>score){score=s;best=b;}}if(best>=0&&score>=20){ImproveRelations(a,best);return;}}
  if(n.influence>=DiplomacyFlavours.InciteCost+20){int mine=cities.Count(c=>c.owner==a);for(int b=0;b<civCount;b++){if(b==a||InciteReason(a,b)!="")continue;if(cities.Count(c=>c.owner==b)>=2*Math.Max(1,mine)&&OpinionCore(a,b)<0){Incite(a,b);return;}}}
 }
 // ---------- postures (a small finite state machine) ----------
 public int Posture(int owner){return modernRules&&owner>0&&owner<civCount?nations[owner].posture:DiplomacyFlavours.Builder;}
 int PostureAmbition(int a){switch(Posture(a)){case DiplomacyFlavours.Aggressor:return 15;case DiplomacyFlavours.Defensive:return -25;case DiplomacyFlavours.Builder:return -10;}return 0;}
 bool Threatened(int owner){if(CoalitionAgainst(owner).Count>0)return true;int mine=KnownMilitary(owner,owner);for(int b=0;b<civCount;b++)if(b!=owner&&AtWar(owner,b)&&KnownMilitary(owner,b)>mine*6/5)return true;return false;}
 internal void UpdatePosture(int a){
  if(!modernRules||!ActiveRival(a)||(turn+a)%5!=0||!cities.Any(c=>c.owner==a))return;var n=nations[a];int before=n.posture;
  int ambition=0;for(int b=0;b<civCount;b++)if(b!=a)ambition=Math.Max(ambition,AmbitionCore(a,b));
  bool threat=Threatened(a);int bold=Flavour(a,DiplomacyFlavours.Boldness);
  int enter=45-bold*2,stay=enter-15;
  if(threat)n.posture=DiplomacyFlavours.Defensive;
  else if(ambition>=enter||n.posture==DiplomacyFlavours.Aggressor&&(ambition>=stay||AtWarWithAnyone(a)))n.posture=DiplomacyFlavours.Aggressor;
  else if(cities.Count(c=>c.owner==a)<3+2*Era(a)&&turn<250)n.posture=DiplomacyFlavours.Expansionist;
  else n.posture=DiplomacyFlavours.Builder;
  if(n.posture!=before&&KnowsNation(a))Note(NationName(a)+" is now "+DiplomacyFlavours.PostureNames[n.posture].ToLower()+".");
 }
 public int PostureSoldiers(int owner){int p=Posture(owner);return p==DiplomacyFlavours.Defensive||p==DiplomacyFlavours.Aggressor?2:0;}
 int PostureWillingness(DiplomaticOffer o){if(!modernRules)return 0;int p=Posture(o.to);var k=o.kind;if(p==DiplomacyFlavours.Builder&&(k==TreatyKind.Research||k==TreatyKind.Trade||k==TreatyKind.Friendship))return 5;if(p==DiplomacyFlavours.Aggressor&&(k==TreatyKind.Friendship||k==TreatyKind.Alliance))return -10;if(p==DiplomacyFlavours.Defensive&&k==TreatyKind.Alliance)return 10;return 0;}
 // ---------- turn ----------
 void DiplomacyDepthTurn(){
  if(!modernRules)return;
  for(int o=0;o<civCount;o++){var n=nations[o];n.influence=Math.Min(DiplomacyFlavours.InfluenceCap,n.influence+InfluencePerTurn(o));}
  // Forgiving leaders (7 or more) let grievances fade twice as fast in peace.
  foreach(var p in diplomacy)if(!AtWar(p.a,p.b)){if(p.grievanceA>0&&Flavour(p.a,DiplomacyFlavours.Forgiveness)>=7)p.grievanceA--;if(p.grievanceB>0&&Flavour(p.b,DiplomacyFlavours.Forgiveness)>=7)p.grievanceB--;}
  UpdateAlarmAndCoalitions();
  for(int a=1;a<civCount;a++){UpdatePosture(a);if(ActiveRival(a)&&(turn+a)%5==0)InfluenceAI(a);}
 }
 // ---------- utility: what a deal is worth to the leader receiving it ----------
 public int GoldNeed(int owner){var n=nations[owner];int need=100+(Treasury(owner)<30?50:0)+(n.arrears>0?50:0)+(Flavour(owner,DiplomacyFlavours.Greed)-5)*10;return Math.Max(50,Math.Min(250,need));}
 public int ResourceNeed(int owner,int r){if(r<=0||r>=ResourceNames.Length)return 100;if(ResourceFood[r]>0&&nations[owner].foodState<0)return 200;if(AtWarWithAnyone(owner)&&UnitResource.Contains(r))return 180;if(ResourceTrade[r]>0&&cities.Any(c=>c.owner==owner&&Disorder(c)))return 150;return 100;}
 public string NeedText(int owner){var parts=new List<string>();int g=GoldNeed(owner);if(g>=150)parts.Add("they need gold badly (worth "+g+"% to them)");else if(g<=70)parts.Add("gold matters little to them");if(nations[owner].foodState<0)parts.Add("they are hungry: food resources are worth double");if(AtWarWithAnyone(owner))parts.Add("at war: iron and horses are worth more");return parts.Count==0?"":" Their needs: "+string.Join("; ",parts.ToArray())+".";}
 // ---------- why a leader feels as it does ----------
 [NonSerialized] Dictionary<int,int> rulerOpinionMemo;[NonSerialized] int rulerOpinionTurn=-1;
 int RulerOpinion(int o,int t,List<KeyValuePair<string,int>> lines){
  if(!livingWorld||!modernRules)return 0;var r=Ruler(o);if(r==null)return 0;
  if(lines==null){if(rulerOpinionMemo==null||rulerOpinionTurn!=turn){rulerOpinionMemo=new Dictionary<int,int>();rulerOpinionTurn=turn;}int v;if(rulerOpinionMemo.TryGetValue(o*1000+t,out v))return v;v=RulerOpinionParts(o,t,r,null);rulerOpinionMemo[o*1000+t]=v;return v;}
  return RulerOpinionParts(o,t,r,lines);
 }
 int RulerOpinionParts(int o,int t,Character r,List<KeyValuePair<string,int>> lines){
  int sum=0;string who=r.name;Func<int,bool> has=k=>r.traitA==k||r.traitB==k;
  int theirArmy=-1,myArmy=-1;Func<int> them=()=>theirArmy<0?(theirArmy=KnownMilitary(o,t)):theirArmy,me=()=>myArmy<0?(myArmy=KnownMilitary(o,o)):myArmy;
  if(has(LivingCatalog.Paranoid)&&them()>me())Part(lines,ref sum,who+" (paranoid) fears your army",-8);
  if(has(LivingCatalog.Ambitious)&&cities.Count(c=>c.owner==t)>cities.Count(c=>c.owner==o))Part(lines,ref sum,who+" (ambitious) envies your size",-6);
  if(has(LivingCatalog.Loyal)&&Friends(o,t))Part(lines,ref sum,who+" (loyal) stands by friends",8);
  if(has(LivingCatalog.Honest))Part(lines,ref sum,Grievances(o,t)>=25?who+" (honest) despises broken promises":who+" (honest) trusts plain dealing",Grievances(o,t)>=25?-6:3);
  if(has(LivingCatalog.Pious)){var mine=CapitalOf(o);var theirs=CapitalOf(t);if(mine!=null&&theirs!=null&&mine.religion>=0&&theirs.religion>=0)Part(lines,ref sum,mine.religion==theirs.religion?who+" (pious) shares your faith":who+" (pious) scorns your faith",mine.religion==theirs.religion?6:-4);}
  if(has(LivingCatalog.Brave)&&them()>=me())Part(lines,ref sum,who+" (brave) respects a strong army",4);
  if(has(LivingCatalog.Greedy)&&contracts.Any(c=>SamePair(c.from,c.to,o,t)))Part(lines,ref sum,who+" (greedy) likes your trade",4);
  if(has(LivingCatalog.Neurotic)&&AtWarWithAnyone(t))Part(lines,ref sum,who+" (neurotic) is unsettled by your wars",-4);
  if(has(LivingCatalog.Drunkard)){int mood=(int)((((uint)(seed*31+o*977+t*131+turn/10*7))>>3)%11u)-5;if(mood!=0)Part(lines,ref sum,who+" (drunkard) has moods",mood);}
  if(r.stress>=70)Part(lines,ref sum,who+" is under strain and short-tempered",-5);
  return sum;
 }
 static void Part(List<KeyValuePair<string,int>> lines,ref int sum,string text,int v){if(v==0)return;sum+=v;if(lines!=null)lines.Add(new KeyValuePair<string,int>(text,v));}
 // The opinion with the reasons behind it; the reasons add up exactly to Opinion (before the -100..100 limit).
 int OpinionSum(int o,int t,List<KeyValuePair<string,int>> lines){
  var p=Relation(o,t);int sum=0;int trust=o==p.a?p.trustA:p.trustB;
  Part(lines,ref sum,"Trust from past dealings",trust);
  Part(lines,ref sum,"Old grudges",-GrudgePenalty(o,t));
  Part(lines,ref sum,"Grievances ("+Grievances(o,t)+")",-Grievances(o,t)/2);
  if(nations[o].government==nations[t].government)Part(lines,ref sum,"Same government",5);
  if(Friends(o,t))Part(lines,ref sum,"Friendship",10);
  if(Allied(o,t))Part(lines,ref sum,"Alliance",15);
  if(AtWar(o,t))Part(lines,ref sum,"At war",-25);
  if(Embargoed(o,t))Part(lines,ref sum,"Embargo",-10);
  if(t==0&&ActiveRival(o))Part(lines,ref sum,"Wary of you (difficulty)",DifficultyTable.Attitude[Level]);
  {var e=Civ(o);int core=AgendaCore(o,t);if(core!=0&&e!=null&&e.Agenda>=0)Part(lines,ref sum,"Agenda: "+CivLore.AgendaNames[e.Agenda],core);}
  if(o!=t&&CivId(t)==33&&CivId(o)!=33)Part(lines,ref sum,"Distrusts demons",-10);
  if(modernRules){
   Part(lines,ref sum,"Alarmed by your expansion ("+Alarm(o,t)+")",-Alarm(o,t)/2);
   if(InCoalition(o,t))Part(lines,ref sum,"In a coalition against you",-10);
   bool shared=false;for(int x=0;x<civCount&&!shared;x++)if(x!=o&&x!=t&&AtWar(o,x)&&AtWar(t,x))shared=true;if(shared)Part(lines,ref sum,"Shared enemy",10);
   sum+=RulerOpinion(o,t,lines);
   FaithOpinion(o,t,lines,ref sum);
  }
  return sum;
 }
 // Opinion without the coalition rules looking at themselves (used where opinion decides coalitions and envoys).
 int OpinionCore(int o,int t){return Opinion(o,t);}
 public List<KeyValuePair<string,int>> OpinionLines(int observer,int other){var lines=new List<KeyValuePair<string,int>>();OpinionSum(observer,other,lines);return lines.OrderBy(l=>-Math.Abs(l.Value)).ToList();}
}
}
