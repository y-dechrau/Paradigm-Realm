using System;
using System.Linq;
namespace ParadigmRealm {
// Religious buildings bought with faith (owner request, 8 Nov 2026; modern rules, save version 35).
// Every city gathers its own faith each turn (the same amount also goes to the national pool used for pantheons, faiths, missions,
// priests and doctrines, as before). A religious building can be raised only when the city itself has gathered enough faith; it costs no
// production. After Civilization VI's faith purchases and Civilization V's Shrine, Temple and religious buildings.
// Every city now makes at least 1 faith, so a new town can save for a Shrine.
public partial class World {
 public const int ShrineBuilding=45,MonasteryBuilding=46,CathedralBuilding=47,CityFaithCap=999;
 public static readonly int[] FaithBuildingIds={ShrineBuilding,36,MonasteryBuilding,CathedralBuilding};
 public static readonly string[] FaithBuildingNames={"Shrine","Temple","Monastery","Cathedral"};
 // Faith on Standard speed; Quick costs two thirds of this and Long half as much again, like production.
 public static readonly int[] FaithBuildingCosts={25,60,100,160};
 // Technology needed (keystone id, "" none) and the building needed first (-1 none).
 public static readonly string[] FaithBuildingTech={"","mysticism","philosophy","education"};
 public static readonly int[] FaithBuildingNeeds={-1,ShrineBuilding,36,36};
 public static readonly string[] FaithBuildingTexts={"+1 faith","+2 faith, +2 content; a holy city needs one; Priests are ordained here","+2 faith, +2 science","+3 faith, +3 content"};
 public static bool IsFaithBuilding(int b){return Array.IndexOf(FaithBuildingIds,b)>=0;}
 public static string FaithBuildingName(int b){int i=Array.IndexOf(FaithBuildingIds,b);return i<0?"Unknown":FaithBuildingNames[i];}
 public int FaithBuildingCost(int b){int i=Array.IndexOf(FaithBuildingIds,b);if(i<0)return 0;int cost=FaithBuildingCosts[i];return speed==0?cost*2/3:speed==2?cost*3/2:cost;}
 int ShrineFaith(City c){return (HasBuilding(c,ShrineBuilding)?1:0)+(HasBuilding(c,MonasteryBuilding)?2:0)+(HasBuilding(c,CathedralBuilding)?3:0);}
 // Listed once the technology is known and the building is not yet there.
 public bool FaithAllows(City c,int b){int i=Array.IndexOf(FaithBuildingIds,b);if(!modernRules||i<0||c==null||HasBuilding(c,b))return false;return FaithBuildingTech[i]==""||HasTech(c.owner,FaithBuildingTech[i]);}
 public string FaithBuildReason(City c,int b){
  int i=Array.IndexOf(FaithBuildingIds,b);if(!modernRules)return "Not in this game's rules";if(i<0||c==null)return "Unknown building";if(HasBuilding(c,b))return "Already built";
  if(FaithBuildingTech[i]!=""&&!HasTech(c.owner,FaithBuildingTech[i]))return "Requires "+TechNames[TechnologyCatalog.Index(FaithBuildingTech[i])];
  if(FaithBuildingNeeds[i]>=0&&!HasBuilding(c,FaithBuildingNeeds[i]))return "Needs a "+FaithBuildingName(FaithBuildingNeeds[i])+" first";
  if(Disorder(c))return "The city is in disorder";
  // Paid from the civilisation's faith total (owner request, 9 Nov 2026), not from the city's own store.
  int cost=FaithBuildingCost(b),have=nations[c.owner].faith;if(have<cost)return "Needs "+cost+" faith ("+have+" in your treasury of faith, +"+FaithPerTurn(c.owner)+" a turn)";
  return "";
 }
 public bool BuildWithFaith(City c,int b){if(FaithBuildReason(c,b)!="")return false;nations[c.owner].faith-=FaithBuildingCost(b);c.buildings.Add(b);if(c.owner==0)Note(FaithBuildingName(b)+" consecrated in "+c.name+".");return true;}
 public int FaithTurns(City c,int b){int left=FaithBuildingCost(b)-nations[c.owner].faith;if(left<=0)return 0;int per=FaithPerTurn(c.owner);return per<=0?-1:(left+per-1)/per;}
 internal void GatherCityFaith(){if(!modernRules)return;foreach(var c in cities)c.faith=Math.Min(CityFaithCap,Math.Max(0,c.faith)+FaithYield(c));}
 // Rivals raise the cheapest religious building their city can afford.
 void FaithBuildingAI(int owner){if(!modernRules)return;foreach(var c in cities){if(c.owner!=owner)continue;foreach(int b in FaithBuildingIds)if(FaithBuildReason(c,b)==""){BuildWithFaith(c,b);break;}}}
}
}
