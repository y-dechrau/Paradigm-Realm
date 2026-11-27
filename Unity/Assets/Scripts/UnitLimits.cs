using System;
using System.Linq;
namespace ParadigmRealm {
// Unit limits (owner report, 27 Nov 2026: auto-managed cities flooded the map with Scouts; "can you think of a mechanism that would
// limit units in general"). Soldiers and warships are already held by service capacity (national forces, Military > Forces).
// Everything else a city trains now has a national limit that grows with the number of your cities; a city cannot start another
// while the limit is reached, counting those in the field and those other cities are already training. The build list shows the
// reason ("Limit reached: 2 of 2 Scouts for 5 cities"). The same rule holds for every civilisation, and auto-managed cities follow it.
//   Settler  1 + 1 per 3 cities (at most 4)       Scout   1 + 1 per 4 cities (at most 3)     Medic  1 + 1 per 4 cities
//   Worker   2 per city                            Caravan 2 + 1 per 2 cities                 Boat   1 + 1 per 4 cities
//   Galley   1 + 1 per 2 cities                    Priest  1 + 1 per 3 cities
// New games only (modernRules); older checks and games keep their numbers.
public static class UnitLimitRules{
 public static int Limit(int type,int cities){cities=Math.Max(1,cities);switch(type){
  case 0:return Math.Min(4,1+cities/3);case 1:return 2*cities;case 3:return Math.Min(3,1+cities/4);case 10:return 1+cities/4;
  case 11:return 2+cities/2;case 12:return 1+cities/4;case 13:return 1+cities/2;case 15:return 1+cities/3;default:return -1;}}
}
public partial class World {
 public int UnitLimit(int owner,int type){if(!modernRules)return -1;return UnitLimitRules.Limit(type,cities.Count(c=>c.owner==owner));}
 // Units of this kind in the field plus those other cities are training now (the asking city's own line is not counted).
 public int UnitsCounted(int owner,int type,City asking){int b=UnitBuild(type);return units.Count(u=>u.owner==owner&&u.type==type)+cities.Count(c=>c.owner==owner&&c!=asking&&c.build==b);}
 public string UnitLimitReason(City c,int type){int lim=UnitLimit(c.owner,type);if(lim<0)return "";int have=UnitsCounted(c.owner,type,c);if(have<lim)return "";
  int n=cities.Count(a=>a.owner==c.owner);return "Limit reached: "+have+" of "+lim+" "+UnitNames[type]+(lim==1?"":"s")+" for "+n+(n==1?" city":" cities");}
}
}
