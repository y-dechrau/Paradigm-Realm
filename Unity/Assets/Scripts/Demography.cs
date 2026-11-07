using System;
using System.Linq;
namespace ParadigmRealm {
// Population after real history (owner request, 7 Nov 2026). Sources: the largest-city estimates of Chandler, Modelski and Morris
// (Uruk in the low tens of thousands, Rome about a million around 200 CE, Baghdad, Kaifeng and Constantinople under or near a million,
// London about 6.5 million in 1900, Tokyo about 37 million today); HYDE and UN urban shares (about 4% of people in towns in 1500,
// 10% in 1800, 16% in 1900, over half today); and the demographic transition (growth well under 1% a year before 1800, a peak of about
// 2.1% around 1963, slowing since).
//   People in a city: a size grows by the square of the size, scaled by the owner's age, so a size 8 Dawn city is about 64,000
//   people and a size 14 Classical city about a million.
//   Countryside: everyone else lives on the land; the share in cities is 5% in the Dawn Age rising to 85% in the Dimensional Age.
//   Growth: before Medicine, cities above size 8 need more food to grow (the "urban graveyard": towns died faster than they were
//   born and grew by people moving in) and can lose a citizen to an epidemic, less often with an Aqueduct. Medicine cuts the food
//   needed by a quarter (death rates fall first); in the Modern Age and beyond large cities grow more slowly again (birth rates fall).
public partial class World {
 public static readonly int[] PeoplePerSize={1000,5000,6000,12000,25000,40000};
 public static readonly int[] UrbanShare={5,8,10,40,75,85};
 public static readonly int[] LifeByAge={25,28,31,40,72,90};
 public const int GraveyardSize=8,EpidemicPerSize=10,LateTransitionSize=12;
 public long CityPeople(City c){
  if(c==null)return 0;if(!modernRules)return (long)(1000*Math.Pow(c.pop,2.8));
  int era=Math.Max(0,Math.Min(5,Era(c.owner)));return (long)PeoplePerSize[era]*c.pop*c.pop;
 }
 public long RuralPeople(int owner,long urban){int share=UrbanShare[Math.Max(0,Math.Min(5,Era(owner)))];return urban*(100-share)/share;}
 public int LifeExpectancy(int owner){int era=Math.Max(0,Math.Min(5,Era(owner)));int years=LifeByAge[era];if(era<4&&HasTech(owner,"medicine"))years+=15;return years;}
 // Food needed to grow, as a percentage of the usual amount.
 public int DemographicPercent(City c){
  if(!modernRules||c==null)return 100;int percent=100;
  if(HasTech(c.owner,"medicine")){percent-=25;if(Era(c.owner)>=4&&c.pop>LateTransitionSize)percent+=5*(c.pop-LateTransitionSize);}
  else if(c.pop>GraveyardSize)percent+=15*(c.pop-GraveyardSize);
  return Math.Max(50,Math.Min(250,percent));
 }
 // Chance in a thousand each turn that sickness takes a citizen.
 public int EpidemicChance(City c){if(!modernRules||c==null||c.pop<GraveyardSize||HasTech(c.owner,"medicine"))return 0;int chance=(c.pop-GraveyardSize+1)*EpidemicPerSize;return HasBuilding(c,37)?chance/2:chance;}
 void Epidemic(City c){
  int chance=EpidemicChance(c);if(chance<=0)return;
  if(new Random(seed*7919+turn*104729+c.id*31).Next(1000)>=chance)return;
  c.pop--;c.food=0;NormaliseSpecialists(c);
  if(c.owner==0)Note("Sickness sweeps through "+c.name+": it falls to size "+c.pop+(HasBuilding(c,37)?".":". An Aqueduct would halve the risk; Medicine ends it."));
 }
 public static string PeopleText(long people){
  if(people>=10000000)return (people/1000000)+" million";if(people>=1000000)return (people/100000/10.0).ToString("0.0")+" million";
  if(people>=10000)return (people/1000).ToString("N0")+",000";return people.ToString("N0");
 }
}
}
