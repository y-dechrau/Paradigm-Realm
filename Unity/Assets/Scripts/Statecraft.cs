using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Statecraft (owner request, 24 Nov 2026): one national choice that lets a civilisation be strong by staying small or by growing
// large, and be changed during the game. From Code of Laws:
//  - Centralised: more science, shields and gold for every city fewer than StatecraftRules.FewCities, soldiers raised as Veterans and
//    influence from pacts; Settlers cost more.
//  - Balanced: the rules as they were. Every civilisation starts here.
//  - Expansive: more citizens serve in peacetime, Settlers are cheaper and extra cities cost less gold.
// A change takes SettleTurns, in which neither the old choice nor the new one counts and every city has one citizen fewer content;
// then it must stand for MinTurns from the day it was made. Apart from the choice, every city beyond the fourth makes technology
// dearer for everyone of the home world, so a small civilisation can keep up. Off in World.New, so older checks are unchanged;
// new games and loaded ones switch it on (EnableStatecraft). Every number is here.
public static class StatecraftRules {
 public const int Balanced=0,Centralised=1,Expansive=2,Count=3;
 public static readonly string[] Names={"Balanced","Centralised","Expansive"};
 public static readonly string[] Mottos={"Neither large nor small: the realm as it is.","Few cities, each one made great.","Many cities, and the weight of numbers."};
 public const string Tech="laws";
 public const int MinTurns=15,SettleTurns=5,Unrest=1;
 // Centralised: this many percent more science, shields and gold for each city fewer than FewCities, up to MostPercent.
 public const int FewCities=8,PercentPerCity=8,MostPercent=40,SettlerDearer=50,InfluencePerPact=1,MostPactInfluence=3;
 // Expansive.
 public const int ServicePercent=60,SettlerCheaper=25,ExpansionCheaper=33;
 // Everyone: technology costs this much more for each city beyond World.FreeCities, up to the most.
 public const int TechPerCity=3,TechMost=45;
 // Rivals: a Centralised one stops settling at AiSmall cities; none changes again within AiMinTurns.
 public const int AiSmall=5,AiLarge=8,AiMinTurns=25;
 public static int CentralPercent(int cities){return cities<1?0:Math.Max(0,Math.Min(MostPercent,(FewCities-cities)*PercentPerCity));}
 public static int TechPercent(int cities){return Math.Max(0,Math.Min(TechMost,(cities-World.FreeCities)*TechPerCity));}
}
public partial class World {
 public bool statecraft;
 // Development only: what a probe makes each owner choose, and whether it settles as a small realm.
 [NonSerialized] public Dictionary<int,int> statecraftForced;[NonSerialized] public bool statecraftRulesOff;
 public void EnableStatecraft(){statecraft=true;foreach(var n in nations)if(n!=null&&(n.statecraft<0||n.statecraft>=StatecraftRules.Count))n.statecraft=0;}
 int CityTotal(int owner){int k=0;foreach(var c in cities)if(c.owner==owner)k++;return k;}
 // A change still taking effect.
 public bool StatecraftUnsettled(int owner){return statecraft&&owner>=0&&owner<civCount&&turn-nations[owner].statecraftSince<StatecraftRules.SettleTurns;}
 public int StatecraftSettles(int owner){return Math.Max(0,nations[owner].statecraftSince+StatecraftRules.SettleTurns-turn);}
 // What counts today: the choice made, or Balanced while it settles.
 public int StatecraftOf(int owner){if(!statecraft||statecraftRulesOff||owner<0||owner>=civCount||nations[owner].statecraft==0||StatecraftUnsettled(owner))return 0;return nations[owner].statecraft;}
 public int StatecraftPercent(int owner){return StatecraftOf(owner)==StatecraftRules.Centralised?StatecraftRules.CentralPercent(CityTotal(owner)):0;}
 public int StatecraftContent(int owner){return StatecraftUnsettled(owner)?-StatecraftRules.Unrest:0;}
 // Percent more that technology costs this civilisation for its size. The peoples behind the gates keep pace by their own rule.
 public int SizeTechPercent(int owner){return !statecraft||statecraftRulesOff||owner<0||owner>=civCount||IsNativeNation(owner)?0:StatecraftRules.TechPercent(CityTotal(owner));}
 public int StatecraftSettlerCost(int owner,int cost){int s=StatecraftOf(owner);return s==StatecraftRules.Centralised?cost+cost*StatecraftRules.SettlerDearer/100:s==StatecraftRules.Expansive?cost-cost*StatecraftRules.SettlerCheaper/100:cost;}
 public int StatecraftExpansion(int owner,int cost){return StatecraftOf(owner)==StatecraftRules.Expansive?cost-cost*StatecraftRules.ExpansionCheaper/100:cost;}
 public int PeaceServicePercent(int owner){return StatecraftOf(owner)==StatecraftRules.Expansive?StatecraftRules.ServicePercent:ForcesRules.PeacePercent;}
 public int StatecraftInfluence(int owner){
  if(StatecraftOf(owner)!=StatecraftRules.Centralised)return 0;int k=0;foreach(var p in diplomacy)if((p.a==owner||p.b==owner)&&p.pacts!=0)for(int b=0;b<Pacts.Count;b++)if((p.pacts&(1<<b))!=0)k++;
  return Math.Min(StatecraftRules.MostPactInfluence,k*StatecraftRules.InfluencePerPact);}
 public string StatecraftReason(int owner,int choice){
  if(!statecraft)return "Not available in this game.";if(result!="")return "The game has ended.";if(choice<0||choice>=StatecraftRules.Count)return "Unknown choice.";
  var n=nations[owner];if(n.statecraft==choice)return StatecraftUnsettled(owner)?"Taking effect in "+StatecraftSettles(owner)+(StatecraftSettles(owner)==1?" turn.":" turns."):"Your realm is governed this way.";
  if(!HasTech(owner,StatecraftRules.Tech))return "Requires "+TechNames[TechnologyCatalog.Index(StatecraftRules.Tech)]+".";
  if(!cities.Any(c=>c.owner==owner))return "Found a city first.";
  int since=turn-n.statecraftSince;return since<StatecraftRules.MinTurns?"The last choice must stand "+(StatecraftRules.MinTurns-since)+" more turns.":"";}
 public bool SetStatecraft(int owner,int choice){
  if(StatecraftReason(owner,choice)!="")return false;nations[owner].statecraft=choice;nations[owner].statecraftSince=turn;budgetMemo=null;
  if(owner==0)Note("Your realm turns to "+StatecraftRules.Names[choice].ToLower()+" rule. It takes effect in "+StatecraftRules.SettleTurns+" turns.");
  else if(KnowsNation(owner))Note(NationName(owner)+" turns to "+StatecraftRules.Names[choice].ToLower()+" rule.");
  return true;}
 void StatecraftTurn(int owner){
  if(!statecraft||IsNativeNation(owner))return;var n=nations[owner];
  if(owner==0){
   if(!n.statecraftTold&&HasTech(0,StatecraftRules.Tech)){n.statecraftTold=true;Note("Your laws let you choose how the realm is governed: see Empire, Statecraft.");}
   if(n.statecraftSince>0&&turn-n.statecraftSince==StatecraftRules.SettleTurns)Note("Your "+StatecraftRules.Names[n.statecraft].ToLower()+" rule is in force.");
   return;}
  if((turn+owner)%5!=0||AtWarWithAnyone(owner)&&statecraftForced==null||turn-n.statecraftSince<StatecraftRules.AiMinTurns)return;
  int want=StatecraftWanted(owner);if(want!=n.statecraft)SetStatecraft(owner,want);}
 // Rivals: aggressors and settlers want numbers, builders and the threatened keep few cities well; a large realm is never centralised.
 public int StatecraftWanted(int owner){
  int forced;if(statecraftForced!=null&&statecraftForced.TryGetValue(owner,out forced))return forced;
  int k=CityTotal(owner),posture=Posture(owner);
  if(k>=StatecraftRules.AiLarge)return StatecraftRules.Expansive;
  bool room=k<RivalTargetCities(owner);
  if(posture==DiplomacyFlavours.Aggressor||posture==DiplomacyFlavours.Expansionist&&room)return k>=3?StatecraftRules.Expansive:StatecraftRules.Balanced;
  return k<=StatecraftRules.AiSmall?StatecraftRules.Centralised:StatecraftRules.Balanced;}
 // A Centralised rival that has the cities it wants stops settling.
 bool StatecraftStopsSettling(int owner){return statecraft&&owner!=0&&nations[owner].statecraft==StatecraftRules.Centralised&&CityTotal(owner)>=(statecraftForced!=null?4:StatecraftRules.AiSmall);}
 // ---------- the preview ----------
 public sealed class StatecraftFigures {public int science,shields,gold,service,expansion,settler,influence,techPercent,bonus;}
 // Your realm's figures as they would stand under a choice once it had settled.
 public StatecraftFigures PreviewStatecraft(int owner,int choice){
  var n=nations[owner];int was=n.statecraft,since=n.statecraftSince;var memo=budgetMemo;
  try{n.statecraft=choice;n.statecraftSince=turn-1000;budgetMemo=null;var f=new StatecraftFigures();
   foreach(var c in cities)if(c.owner==owner){var v=Yield(c);f.science+=v[2];f.shields+=v[1];}
   var b=BudgetDetail(owner);f.gold=b.Net;f.expansion=b.expansion;f.service=ServiceCapacity(owner);f.influence=InfluencePerTurn(owner);f.techPercent=SizeTechPercent(owner);f.bonus=StatecraftPercent(owner);
   var home=cities.FirstOrDefault(c=>c.owner==owner);f.settler=home==null?0:CostFor(home,UnitBuild(0));return f;}
  finally{n.statecraft=was;n.statecraftSince=since;budgetMemo=memo;}}
}
}
