using System;
using System.Linq;
namespace ParadigmRealm {
// Priests (owner request, 31 Oct 2026): once a city follows a faith and has a Temple it can train Priests (40 production) or
// buy one for faith. Ideas from Civilization V/VI missionaries and apostles (spreads with charges), Age of Empires monks
// (healing beside an army) and Civilization inquisitors (a resident priest shields a city from rival preaching):
// - Preach: in or next to a city, +200 pressure for the priest's faith there (halved if a priest of another faith is in
//   that city); two sermons, then the priest retires.
// - In one of your own cities a priest keeps 1 citizen content (once per city).
// - Each turn it heals your units on and next to its tile by 2 more.
public partial class World {
 public const int PriestUnit=15,PriestCharges=2,PriestFaithCost=50,PreachPressure=200;
 public string PriestReason(City c){if(c.religion<0)return "Requires a city that follows a faith";if(!HasBuilding(c,36))return "Requires a Temple";return "";}
 public string BuyPriestReason(int owner,City c){if(c==null||c.owner!=owner)return "Not your city.";string why=PriestReason(c);if(why!="")return why+".";if(!HasTech(owner,"mysticism"))return "Requires Mysticism.";if(!Capacity(new Unit{owner=owner,type=PriestUnit,id=-1},c.x,c.y))return "No room in the city.";return nations[owner].faith<PriestFaithCost?"Needs "+PriestFaithCost+" faith.":"";}
 public Unit BuyPriest(int owner,City c){if(BuyPriestReason(owner,c)!="")return null;nations[owner].faith-=PriestFaithCost;var p=Add(owner,PriestUnit,c.x,c.y);p.moves=0;p.homeCity=c.id;p.faith=c.religion;p.charges=PriestCharges;if(owner==0)Note("A Priest of "+ReligionCatalog.Name(p.faith)+" was ordained in "+c.name+".");return p;}
 City PreachTarget(Unit u){return cities.Where(c=>Distance(c.x,c.y,u.x,u.y)<=1).OrderBy(c=>Distance(c.x,c.y,u.x,u.y)).FirstOrDefault();}
 public string PreachReason(Unit u){if(u==null||u.type!=PriestUnit)return "Only priests preach.";if(u.moves<=0)return "The priest needs movement.";if(u.faith<0||u.charges<=0)return "This priest has no sermons left.";var c=PreachTarget(u);if(c==null)return "Stand in or next to a city.";if(AtWar(u.owner,c.owner))return "Cannot preach in an enemy city.";return "";}
 public bool Preach(Unit u){
  if(PreachReason(u)!="")return false;var c=PreachTarget(u);
  bool guarded=units.Any(p=>p.type==PriestUnit&&p.owner==c.owner&&p.faith>=0&&p.faith!=u.faith&&p.x==c.x&&p.y==c.y);
  c.faithPressure[u.faith]=Math.Min(3000,c.faithPressure[u.faith]+(guarded?PreachPressure/2:PreachPressure));UpdateMajority(c,u.owner==0);u.charges--;u.moves=0;
  if(u.owner==0)Note("Your priest preached "+ReligionCatalog.Name(u.faith)+" in "+c.name+(guarded?" (a resident priest of another faith resisted)":"")+".");
  else if(c.owner==0)Note("A priest of "+NationName(u.owner)+" preached "+ReligionCatalog.Name(u.faith)+" in "+c.name+".");
  if(u.charges<=0){units.Remove(u);if(u.owner==0)Note("The priest has given a last sermon and retires to a monastery.");}
  return true;
 }
 public int PriestContent(City c){return units.Any(p=>p.type==PriestUnit&&p.owner==c.owner&&p.x==c.x&&p.y==c.y)?1:0;}
 void PriestHealing(int owner){foreach(var p in units.Where(a=>a.owner==owner&&a.type==PriestUnit).ToList())foreach(var u in units.Where(a=>a.owner==owner&&a.type!=PriestUnit&&Distance(a.x,a.y,p.x,p.y)<=1))u.hp=Math.Min(10,u.hp+2);}
 // Rivals buy a priest when faith allows (two at most), walk it to the nearest city that does not follow its faith and preach.
 void PriestBuyingAI(int owner){var n=nations[owner];if(n.founded<0||n.faith<PriestFaithCost+20||units.Count(u=>u.owner==owner&&u.type==PriestUnit)>=2)return;var home=cities.Where(c=>c.owner==owner&&BuyPriestReason(owner,c)=="").OrderByDescending(c=>c.religion==n.founded).ThenByDescending(c=>c.pop).FirstOrDefault();if(home!=null)BuyPriest(owner,home);}
 void PriestAI(Unit u){
  if(PreachReason(u)==""&&PreachTarget(u).religion!=u.faith){Preach(u);return;}
  var goal=cities.Where(c=>c.religion!=u.faith&&!AtWar(u.owner,c.owner)&&Distance(c.x,c.y,u.x,u.y)<=15).OrderBy(c=>Distance(c.x,c.y,u.x,u.y)).FirstOrDefault();
  if(goal==null){var home=cities.Where(c=>c.owner==u.owner).OrderBy(c=>Distance(c.x,c.y,u.x,u.y)).FirstOrDefault();if(home!=null&&!(home.x==u.x&&home.y==u.y))Order(u,home.x,home.y);else u.sentry=true;return;}
  if(Distance(goal.x,goal.y,u.x,u.y)>1){var spot=NearestReachableBeside(u,At(goal.x,goal.y));if(spot!=null)Order(u,spot.x,spot.y);}
 }
}
}
