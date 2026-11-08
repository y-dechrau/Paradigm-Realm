using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Governors with a purpose (owner choice, 8 Nov 2026: ideas 1, 2 and 4 from the research on other games; save version 38).
//  1 Traits change yields (after Old World): each of a governor's two traits raises or lowers the city's food, shields, science or
//    gold by 5-15%.
//  2 Settling in (after Civilization VI): a new governor, or one moved to another city, has no effect for SettleTurns turns.
//  3 Policy (after Imperator: Rome): each governor runs one policy. A disloyal governor will not change it, and a corrupt one
//    quietly skims part of what it gains.
public static class GovernorCatalog {
 public const int SettleTurns=5,LoyalSettleTurns=3,ObeyLoyalty=35,SkimFrom=30,ReplaceCost=25;
 // Percent change to food, shields, science and gold, by trait (LivingCatalog.Traits order).
 static readonly int[][] TraitYield={
  new[]{0,15,0,-5},   // Ambitious: drives the city hard
  new[]{5,0,0,0},     // Loyal: steady and careful
  new[]{-5,0,0,15},   // Greedy: squeezes out gold
  new[]{0,0,0,10},    // Honest: clean books
  new[]{0,5,-10,0},   // Paranoid: walls before books
  new[]{10,0,0,0},    // Stoic: patient farming
  new[]{-5,0,15,0},   // Neurotic: restless curiosity
  new[]{5,0,-5,0},    // Pious: tends the fields, distrusts scholars
  new[]{0,10,-5,0},   // Brave: workshops and armouries
  new[]{0,-5,0,-10},  // Drunkard: wastes time and money
 };
 public static int[] TraitPercent(int trait){return trait>=0&&trait<TraitYield.Length?TraitYield[trait]:new int[4];}
 public static readonly string[] Policies={"Steady","Tax hard","Pacify","Grow","Fortify","Assimilate"};
 public static readonly string[] PolicyTexts={
  "No special aim: the city runs as it is.",
  "+25% gold, but one citizen fewer content.",
  "One citizen more content, but -15% shields.",
  "+15% food, but -10% gold.",
  "Defenders in the city +1 strength and +10% shields, but -10% science.",
  "An occupied city settles twice as fast and gains a content citizen while occupied, but -10% science."};
 // Percent change to food, shields, science and gold, by policy.
 static readonly int[][] PolicyYield={new[]{0,0,0,0},new[]{0,0,0,25},new[]{0,-15,0,0},new[]{15,0,0,-10},new[]{0,10,-10,0},new[]{0,0,-10,0}};
 public static int[] PolicyPercent(int policy){return policy>=0&&policy<PolicyYield.Length?PolicyYield[policy]:new int[4];}
 public static readonly string[] YieldNames={"food","shields","science","gold"};
 public static string PercentText(int[] v){var parts=new List<string>();for(int k=0;k<4;k++)if(v[k]!=0)parts.Add((v[k]>0?"+":"")+v[k]+"% "+YieldNames[k]);return parts.Count==0?"no change":string.Join(", ",parts);}
}
public partial class World {
 public bool GovernorSettled(Character g){return g!=null&&g.settle<=0;}
 // The whole percentage a city's governor adds: both traits and the policy once settled. A corrupt governor keeps part of every gain.
 public int[] GovernorPercent(City c){
  var total=new int[4];if(!livingWorld)return total;var g=GovernorOf(c);if(g==null||!GovernorSettled(g))return total;
  var a=GovernorCatalog.TraitPercent(g.traitA);var b=GovernorCatalog.TraitPercent(g.traitB);var p=GovernorCatalog.PolicyPercent(g.policy);
  for(int k=0;k<4;k++){int gain=a[k]+b[k]+p[k];if(gain>0&&g.corruption>=GovernorCatalog.SkimFrom)gain=gain*(100-g.corruption)/100;if(g.breakTurns>0&&gain>0)gain/=2;total[k]=gain;}
  return total;
 }
 void ApplyGovernorYields(City c,ref int f,ref int p,ref int s,ref int g){
  if(!livingWorld)return;var v=GovernorPercent(c);if(v[0]==0&&v[1]==0&&v[2]==0&&v[3]==0)return;
  f+=f*v[0]/100;p+=p*v[1]/100;s+=s*v[2]/100;g+=g*v[3]/100;
 }
 // Content citizens from the policy: Tax hard -1, Pacify +1, Assimilate +1 while the city is occupied.
 public int GovernorContent(City c){if(!livingWorld)return 0;var g=GovernorOf(c);if(g==null||!GovernorSettled(g))return 0;return g.policy==1?-1:g.policy==2?1:g.policy==5&&c.occupation>0?1:0;}
 // Fortify: defenders standing in the city fight one point stronger.
 public int GovernorDefence(Unit u){if(!livingWorld)return 0;foreach(var c in cities)if(c.x==u.x&&c.y==u.y&&c.owner==u.owner){var g=GovernorOf(c);return g!=null&&GovernorSettled(g)&&g.policy==4?1:0;}return 0;}
 public string PolicyReason(City c,int policy){
  var g=GovernorOf(c);if(g==null)return "This city has no governor.";if(policy<0||policy>=GovernorCatalog.Policies.Length)return "No such policy.";if(g.policy==policy)return "Already the policy.";
  if(g.loyalty<GovernorCatalog.ObeyLoyalty)return g.name+" is not loyal enough to obey (loyalty "+g.loyalty+"; needs "+GovernorCatalog.ObeyLoyalty+").";
  return "";
 }
 public bool SetPolicy(City c,int policy){if(PolicyReason(c,policy)!="")return false;GovernorOf(c).policy=policy;return true;}
 int SettleFor(Character g){return HasTrait(g,LivingCatalog.Loyal)?GovernorCatalog.LoyalSettleTurns:GovernorCatalog.SettleTurns;}
 // Two of your governors trade places; both must settle in again.
 public bool SwapGovernors(City a,City b){
  if(a==null||b==null||a==b||a.owner!=b.owner)return false;var ga=GovernorOf(a);var gb=GovernorOf(b);if(ga==null||gb==null)return false;
  ga.city=b.id;gb.city=a.id;ga.settle=SettleFor(ga);gb.settle=SettleFor(gb);peopleIndexed=-1;governorIndex=null;
  if(a.owner==0)Note(ga.name+" now governs "+b.name+" and "+gb.name+" governs "+a.name+". Both need time to settle in.");return true;
 }
 public string ReplaceReason(City c){if(GovernorOf(c)==null)return "This city has no governor.";if(nations[c.owner].gold<GovernorCatalog.ReplaceCost)return "Needs "+GovernorCatalog.ReplaceCost+" gold.";return "";}
 // Dismiss the governor and appoint someone new: costs gold, and the other governors trust you a little less.
 public bool ReplaceGovernor(City c){
  if(ReplaceReason(c)!="")return false;var old=GovernorOf(c);var n=nations[c.owner];n.gold-=GovernorCatalog.ReplaceCost;if(c.owner==0)gold=n.gold;
  characters.Remove(old);foreach(var p in characters.Where(p=>p.owner==c.owner&&p.city>=0))p.loyalty=Math.Max(0,p.loyalty-2);peopleIndexed=-1;governorIndex=null;
  var fresh=MakePerson(c.owner,c.id,new Random(seed*41+turn*13+c.id+characters.Count));fresh.settle=SettleFor(fresh);
  if(c.owner==0)Note(old.name+" is dismissed. "+fresh.name+" ("+TraitLine(fresh)+") will govern "+c.name+" once settled in.");return true;
 }
 // Once a turn: settling in counts down, Assimilate speeds an occupation, and cities that run themselves pick a policy.
 void GovernorTurn(City c,Character g){
  if(g.settle>0){g.settle--;if(g.settle==0&&c.owner==0)Note(g.name+" has settled in as governor of "+c.name+".");}
  if(GovernorSettled(g)&&g.policy==5&&c.occupation>0)c.occupation--;
  if(c.owner!=0||c.autoManage){int want=ChoosePolicy(c);if(want!=g.policy&&PolicyReason(c,want)=="")g.policy=want;}
 }
 public int ChoosePolicy(City c){
  if(c.occupation>0)return 5;if(Disorder(c)||Happiness(c)<=0&&UnhappyCitizens(c)>0)return 2;var v=Yield(c);
  if(v[0]-c.ration<=0)return 3;if(Budget(c.owner)[2]<0)return 1;
  if(units.Any(u=>AtWar(c.owner,u.owner)&&Power[u.type]>0&&Distance(u.x,u.y,c.x,c.y)<=5))return 4;
  return c.pop<GrowthLimit(c)-1?3:0;
 }
 public string GovernorEffectText(City c){var g=GovernorOf(c);if(g==null)return "";if(!GovernorSettled(g))return "Settling in: "+g.settle+" turn"+(g.settle==1?"":"s")+" before any effect.";return GovernorCatalog.PercentText(GovernorPercent(c))+(GovernorContent(c)!=0?(GovernorContent(c)>0?", +":", ")+GovernorContent(c)+" content":"");}
}
}
