using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Owner requests, 8 Nov 2026: a city says why citizens are unhappy and what can be done (MoodReport), and a city can be left to
// manage itself (City.autoManage, AutoManageCities): it sets its own focus, keeps order with entertainers, picks buildings, and
// trains a unit only when the unit's upkeep and rations leave gold and food at zero or above.
public partial class World {
 // Each line is a reason with its effect on the number of content citizens the city can hold (ContentSlots).
 public List<KeyValuePair<string,int>> MoodReasons(City c){
  var n=nations[c.owner];var list=new List<KeyValuePair<string,int>>();
  Action<string,int> add=(text,v)=>{if(v!=0)list.Add(new KeyValuePair<string,int>(text,v));};
  add("Every city holds this many content citizens",BaseContent);
  add("Empire's food: "+(n.foodState>0?"well fed":"hungry"),n.foodState>0?1:n.foodState<0?-1:0);
  add("Not joined to your other cities by road, river or sea",modernRules&&CutOff(c)?-1:0);
  add("Governor's policy, the ruler and the city's memories",LivingContent(c));
  add("Edicts",EdictContent(c.owner));add("Technologies",MinorContent(c.owner));add("Faith",ReligionContent(c));add("Your civilisation",CivContent(c));
  add("Priests nearby",PriestContent(c));add("The age (golden or dark)",AgeContent(c));add("Spiritual trait",HasTrait(c.owner,TraitSpiritual)?1:0);
  add("Entertainers (2 each)",c.entertainers*2);add("Luxury spending",n.luxury*2);add("Government",n.government==1?1:0);
  add("Temple",HasBuilding(c,36)?2:0);add("Cathedral",HasBuilding(c,CathedralBuilding)?3:0);add("Colosseum",HasBuilding(c,42)?3:0);add("Gardens",HasBuilding(c,33)?2:0);
  add("A wonder of yours",OwnWonder(c.owner,61)?2:0);add("Revolution under way",n.revolution>0?-2:0);
  add("Soldiers fighting abroad under this government",n.government==3&&units.Any(u=>u.owner==c.owner&&Power[u.type]>0&&At(u.x,u.y).owner>=0&&At(u.x,u.y).owner!=c.owner&&AtWar(c.owner,At(u.x,u.y).owner))?-2:0);
  add("War exhaustion",-WarUnhappiness(c.owner));add(n.tax==0?"Low taxes (60% science)":n.tax==2?"High taxes (30% science or less)":"Taxes",1-n.tax);
  add("Longevity",HasTech(c.owner,"longevity")?2:0);add("A luxury resource",HasResource(c.owner,5)?2:0);
  add("Occupied after capture",c.occupation>0?-3:0);add("Unpaid upkeep (treasury empty)",n.arrears>0?-1:0);
  return list;
 }
 // Plain advice: only what the player can actually do now (a building appears once its technology is known).
 public List<string> MoodRemedies(City c){
  var n=nations[c.owner];var tips=new List<string>();int unhappy=UnhappyCitizens(c);
  if(unhappy==0){tips.Add("Nobody is unhappy. Each extra citizen beyond "+ContentSlots(c)+" content places will be unhappy.");return tips;}
  if(Workers(c)>0)tips.Add("Turn a working citizen into an entertainer (Worker → Entertainer): each adds 2 content places.");
  if(!HasBuilding(c,36)&&KnowsBuildingTech(c.owner,36))tips.Add("Build a Temple (+2).");
  if(!HasBuilding(c,42)&&KnowsBuildingTech(c.owner,42))tips.Add("Build a Colosseum (+3).");
  if(!HasBuilding(c,33)&&KnowsBuildingTech(c.owner,33))tips.Add("Build Gardens (+2).");
  if(n.tax>0)tips.Add("Lower taxes: a science rate of 60% adds a content place in every city.");
  if(n.luxury<2&&n.gold>20)tips.Add("Spend on luxuries (Economy) for +2 per level.");
  if(WarUnhappiness(c.owner)>0)tips.Add("Make peace: the war costs "+WarUnhappiness(c.owner)+" content places.");
  if(n.arrears>0)tips.Add("Pay your upkeep: an empty treasury costs a content place.");
  if(modernRules&&CutOff(c))tips.Add("Join the city to your others by road.");
  if(!HasResource(c.owner,5))tips.Add("Connect a luxury resource by road (+2).");
  tips.Add("Or let it shrink to a size it can keep happy: stop growth by working fewer food tiles.");
  return tips;
 }
 public string MoodSummary(City c){int u=UnhappyCitizens(c);return Workers(c)+" working citizens, "+ContentSlots(c)+" content places: "+(u>0?u+" unhappy.":"all content.");}

 // ---------- auto-manage ----------
 public void SetAutoManage(City c,bool on){if(c==null)return;c.autoManage=on;if(on&&c.owner==0)AutoManageCity(c);}
 void AutoManageCities(){foreach(var c in cities.Where(a=>a.owner==0&&a.autoManage).ToList())AutoManageCity(c);}
 // Would training this unit keep the key totals at zero or above? Gold per turn after its support, the city's food after its
 // ration, the city's size for a Settler, and the empire's food rations.
 public bool UnitAffordable(City c,int type){
  int support=Power[type]>0||IsNaval(type)?SupportCost(type):1;if(Budget(c.owner)[2]-support<0)return false;
  int ration=foodRations?UnitRation(type):0;if(Yield(c)[0]-c.ration-ration<0)return false;
  if(type==0&&(c.pop<2||Yield(c)[0]-c.ration-ration-2<0))return false;
  if(nations[c.owner].rationShort>0)return false;
  return true;
 }
 public void AutoManageCity(City c){
  var n=nations[c.owner];int balance=Budget(c.owner)[2];
  c.focus=Yield(c)[0]<=0||n.rationShort>0?1:balance<0?4:c.pop>=GrowthLimit(c)?2:0;
  // Order first: entertainers until the city is out of disorder, without starving it.
  NormaliseSpecialists(c);
  while(Happiness(c)<0&&Workers(c)>0){c.entertainers++;if(Yield(c)[0]<0&&c.entertainers>0){c.entertainers--;break;}}
  if(Happiness(c)>0&&c.entertainers>0){while(c.entertainers>0){c.entertainers--;if(Happiness(c)<0){c.entertainers++;break;}}}
  // Buildings: the rivals' choice, skipping any whose upkeep would put the treasury into the red.
  if(c.buildingProject<0){int b=PreferredBuilding(c);if(b>=0&&BuildingIds.Contains(b)&&(BuildingUpkeep(b)==0||balance-BuildingUpkeep(b)>=0))ChooseProject(c,b,false);}
  // Units: a Worker while there are fewer than cities, a Settler when a good site waits, otherwise one defender for an empty city.
  if(c.build<0||(BuildUnit(c.build)>=0&&!UnitAffordable(c,BuildUnit(c.build))&&c.production==0)){
   int want=-1;var choices=Choices(c).Where(b=>BuildUnit(b)>=0&&BuildReason(c,b)=="").ToList();
   int own=cities.Count(a=>a.owner==c.owner),workers=units.Count(u=>u.owner==c.owner&&u.type==1);
   if(workers<own&&choices.Contains(UnitBuild(1)))want=UnitBuild(1);
   else if(ShouldTrainSettler(c)&&choices.Contains(UnitBuild(0)))want=UnitBuild(0);
   else if(!units.Any(u=>u.owner==c.owner&&Power[u.type]>0&&u.x==c.x&&u.y==c.y)){
    var guards=choices.Where(b=>Power[BuildUnit(b)]>0&&!IsNaval(BuildUnit(b))).OrderByDescending(b=>Defence[BuildUnit(b)]).ThenBy(b=>UnitCosts[BuildUnit(b)]).ToList();if(guards.Count>0)want=guards[0];}
   if(want>=0&&!UnitAffordable(c,BuildUnit(want)))want=-1;
   c.build=want;
  }
 }
}
}
namespace ParadigmRealm {
public partial class World {
 // Gold a building costs on top of its shields (owner request, 9 Nov 2026): a quarter of its shield cost, paid on completion.
 // Projects (Transcendence) and the faith buildings bought with faith cost no gold.
 public int BuildingGold(int b){if(b<0||BuildUnit(b)>=0||IsProject(b)||IsGateProject(b)||IsFaithBuilding(b))return 0;return Math.Max(5,Cost(b)/4);}
}
}
