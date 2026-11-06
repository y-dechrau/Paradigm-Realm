using System;
using System.Linq;
namespace ParadigmRealm {
// Eight difficulty levels (owner request, 6 Nov 2026). Steward, the fourth, is the game's rules with no change. Above it rivals
// pay less and start with more; below it they pay more and you pay a little less. Bonuses are small and open, shown on the New
// Game screen, and never curved evenly (Soren Johnson's advice from Civilization IV: cheats should not be linear). Rivals never
// see through fog at any level, and the peoples of the sealed worlds are not changed.
public static class DifficultyTable {
 public const int Steward=3;
 public static readonly string[] Names={"Pilgrim","Novice","Apprentice","Steward","Veteran","Champion","Paragon","Ascendant"};
 public static readonly string[] Texts={
  "For learning the game. Rivals are slow and rarely look for a fight.",
  "A gentle game. Rivals fall behind unless you dawdle, and give you time to arm.",
  "A forgiving game. Rivals have a small handicap.",
  "A fair game: everyone plays by the same rules.",
  "Rivals have a small edge and a head start.",
  "Rivals are noticeably quicker, and war comes sooner.",
  "Rivals outbuild you. You will need a plan from the first turn.",
  "Rivals have every advantage short of cheating at sight. Expect to lose a few times."};
 // Percent of the normal price for rivals.
 public static readonly int[] RivalCost={160,140,120,100,95,85,75,60},RivalWonderCost={160,140,120,100,100,100,100,100},RivalFood={150,130,115,100,95,90,85,75},RivalResearch={140,125,110,100,95,90,85,80};
 public static readonly int[] RivalCities={3,3,3,4,5,6,7,8},RivalSoldiers={60,70,85,100,110,125,140,160};
 // Added to a rival's wish for war with anyone; the turn it may first come for you; its starting attitude to you.
 public static readonly int[] WarLust={-30,-20,-10,0,0,5,10,15},PlayerGrace={140,120,100,80,70,60,50,40},Attitude={15,10,5,0,0,-5,-5,-10};
 // Percent of the normal price for you.
 public static readonly int[] YourResearch={85,90,95,100,100,100,100,100},YourCost={85,90,95,100,100,100,100,100};
 // Extra units each home rival starts with: soldiers, workers, settlers; and free technologies.
 public static readonly int[] ExtraSoldiers={0,0,0,0,1,1,2,2},ExtraWorkers={0,0,0,0,0,1,1,1},ExtraSettlers={0,0,0,0,0,0,0,1},ExtraTechs={0,0,0,0,0,0,0,1};
 static string Share(int percent){return percent+"%";}
 // The whole level in words, from the same numbers the rules use.
 public static string Describe(int i){
  string start=ExtraSoldiers[i]+ExtraWorkers[i]+ExtraSettlers[i]+ExtraTechs[i]==0?"nothing extra":string.Join(", ",new[]{ExtraSoldiers[i]>0?ExtraSoldiers[i]+" soldier"+(ExtraSoldiers[i]>1?"s":""):null,ExtraWorkers[i]>0?ExtraWorkers[i]+" Worker":null,ExtraSettlers[i]>0?ExtraSettlers[i]+" Settler":null,ExtraTechs[i]>0?ExtraTechs[i]+" technology":null}.Where(s=>s!=null).ToArray());
  return Texts[i]+"\nRivals: units and buildings "+Share(RivalCost[i])+", wonders "+Share(RivalWonderCost[i])+", growth "+Share(RivalFood[i])+", research "+Share(RivalResearch[i])+"; aim for "+RivalCities[i]+" cities; armies "+Share(RivalSoldiers[i])+"; start with "+start+"; may attack you from turn "+PlayerGrace[i]+"."+
   "\nYou: research "+Share(YourResearch[i])+", units and buildings "+Share(YourCost[i])+". Rivals never see through fog.";
 }
}
public partial class World {
 // The level chosen on the New Game screen; older saves play at Steward. The older `difficulty` field was never read.
 public int difficultyLevel=DifficultyTable.Steward;
 int Level{get{return Math.Max(0,Math.Min(DifficultyTable.Names.Length-1,difficultyLevel));}}
 int DifficultyCost(City c,int b,int cost){
  if(c.owner==0)return Math.Max(1,cost*DifficultyTable.YourCost[Level]/100);if(!ActiveRival(c.owner))return cost;
  return Math.Max(1,cost*(IsWonder(b)||IsProject(b)?DifficultyTable.RivalWonderCost[Level]:DifficultyTable.RivalCost[Level])/100);
 }
 int DifficultyResearch(int owner,int cost){if(owner==0)return Math.Max(1,cost*DifficultyTable.YourResearch[Level]/100);return ActiveRival(owner)?Math.Max(1,cost*DifficultyTable.RivalResearch[Level]/100):cost;}
 int DifficultyFood(City c,int food){return ActiveRival(c.owner)?Math.Max(1,food*DifficultyTable.RivalFood[Level]/100):food;}
 public int RivalTargetCities(int owner){return ActiveRival(owner)?DifficultyTable.RivalCities[Level]:4;}
 public int PlayerGraceTurn{get{return DifficultyTable.PlayerGrace[Level];}}
 void ApplyDifficultyStart(){
  for(int o=1;o<civCount;o++){
   if(!ActiveRival(o))continue;var home=cities.FirstOrDefault(c=>c.owner==o);if(home==null)continue;
   Action<int> place=type=>{for(int r=1;r<=3;r++)for(int dj=-r;dj<=r;dj++)for(int di=-r;di<=r;di++){var t=StepIn(home.x,home.y,di,dj,r);if(t==null||IsWater(t.terrain)||IsImpassable(t.terrain)||Barren(t.terrain)||units.Any(a=>a.x==t.x&&a.y==t.y))continue;Add(o,type,t.x,t.y);return;}};
   for(int k=0;k<DifficultyTable.ExtraSoldiers[Level];k++)place(2);for(int k=0;k<DifficultyTable.ExtraWorkers[Level];k++)place(1);for(int k=0;k<DifficultyTable.ExtraSettlers[Level];k++)place(0);
   var n=nations[o];for(int k=0;k<DifficultyTable.ExtraTechs[Level];k++){var free=AvailableTech(o).OrderBy(t=>TechCosts[t]).ThenBy(t=>t).Where(t=>t!=n.research).DefaultIfEmpty(-1).First();if(free>=0)n.tech[free]=true;}
  }
 }
}
}
