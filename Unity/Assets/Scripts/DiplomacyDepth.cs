using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// The people behind diplomacy (see Diplomacy.cs for the system itself): each leader's five flavours, taken from its agenda and a
// roll fixed for each game (after the leader tables of Civilization IV and V mods); a posture held as a small state machine so a
// rival does not flicker between moods; what a leader needs, which decides what gold and resources are worth to it in a trade
// (utility curves, after Dave Mark and Kevin Dill); and the ruler's own traits and strain colouring its opinion (after Crusader
// Kings III).

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
}
public partial class World {
 // ---------- flavours ----------
 public int Flavour(int owner,int k){var e=Civ(owner);int b=e!=null&&e.Agenda>=0&&e.Agenda<DiplomacyFlavours.ByAgenda.GetLength(0)?DiplomacyFlavours.ByAgenda[e.Agenda,k]:5;int roll=(int)((((uint)(seed*7919+owner*131+k*31))>>4)%5u)-2;return Math.Max(1,Math.Min(10,b+roll));}
 // ---------- influence ----------
 // Influence buys claims on foreign cities. 2 a turn, +1 for each rival named (naming an enemy rallies the court).
 public int InfluencePerTurn(int owner){return 2+Math.Min(Pacts.MaxRivals,RivalCount(owner));}
 // ---------- postures (a small finite state machine) ----------
 public int Posture(int owner){return modernRules&&owner>0&&owner<civCount?nations[owner].posture:DiplomacyFlavours.Builder;}
 int PostureAmbition(int a){switch(Posture(a)){case DiplomacyFlavours.Aggressor:return 15;case DiplomacyFlavours.Defensive:return -25;case DiplomacyFlavours.Builder:return -10;}return 0;}
 bool Threatened(int owner){for(int b=0;b<civCount;b++)if(b!=owner&&Threat(owner,b)>=50)return true;int mine=KnownMilitary(owner,owner);for(int b=0;b<civCount;b++)if(b!=owner&&AtWar(owner,b)&&KnownMilitary(owner,b)>mine*6/5)return true;return false;}
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
 // ---------- turn ----------
 void DiplomacyDepthTurn(){
  for(int o=0;o<civCount;o++){var n=nations[o];n.influence=Math.Min(Pacts.InfluenceCap,n.influence+InfluencePerTurn(o));}
  for(int a=1;a<civCount;a++)UpdatePosture(a);
 }
 // ---------- utility: what a deal is worth to the leader receiving it ----------
 public int GoldNeed(int owner){var n=nations[owner];int need=100+(Treasury(owner)<30?50:0)+(n.arrears>0?50:0)+(Flavour(owner,DiplomacyFlavours.Greed)-5)*10;return Math.Max(50,Math.Min(250,need));}
 public int ResourceNeed(int owner,int r){if(r<=0||r>=ResourceNames.Length)return 100;if(ResourceFood[r]>0&&nations[owner].foodState<0)return 200;if(AtWarWithAnyone(owner)&&UnitResource.Contains(r))return 180;if(RefusedResource(owner,r))return 160;if(ResourceTrade[r]>0&&cities.Any(c=>c.owner==owner&&Disorder(c)))return 150;return 100;}
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
  if(has(LivingCatalog.Honest))Part(lines,ref sum,Grievances(o,t)>=12?who+" (honest) despises broken promises":who+" (honest) trusts plain dealing",Grievances(o,t)>=12?-6:3);
  if(has(LivingCatalog.Pious)){var mine=CapitalOf(o);var theirs=CapitalOf(t);if(mine!=null&&theirs!=null&&mine.religion>=0&&theirs.religion>=0)Part(lines,ref sum,mine.religion==theirs.religion?who+" (pious) shares your faith":who+" (pious) scorns your faith",mine.religion==theirs.religion?6:-4);}
  if(has(LivingCatalog.Brave)&&them()>=me())Part(lines,ref sum,who+" (brave) respects a strong army",4);
  if(has(LivingCatalog.Greedy)&&contracts.Any(c=>SamePair(c.from,c.to,o,t)))Part(lines,ref sum,who+" (greedy) likes your trade",4);
  if(has(LivingCatalog.Neurotic)&&AtWarWithAnyone(t))Part(lines,ref sum,who+" (neurotic) is unsettled by your wars",-4);
  if(has(LivingCatalog.Drunkard)){int mood=(int)((((uint)(seed*31+o*977+t*131+turn/10*7))>>3)%11u)-5;if(mood!=0)Part(lines,ref sum,who+" (drunkard) has moods",mood);}
  if(r.stress>=70)Part(lines,ref sum,who+" is under strain and short-tempered",-5);
  return sum;
 }
}
}
