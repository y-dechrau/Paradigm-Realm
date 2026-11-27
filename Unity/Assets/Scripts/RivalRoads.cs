using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Rivals build roads as part of their development (owner request, 27 Nov 2026). Before, a rival worker scored a road lowest of all
// jobs and built one only to a port or an improved resource, so in a 150-turn test game no rival joined a single town to its
// capital. Now each civilisation plans, every few turns, a road from every town not yet joined to the nearest town that is (its
// capital first): the land route its own worker would walk, inside its own borders (only roads on its own land join a network).
// Its workers take those tiles before other roads, while the treasury can carry the upkeep (CityRoadNeeds in ImproveAI).
public partial class World {
 [NonSerialized] Dictionary<int,HashSet<int>> cityRoads;[NonSerialized] Dictionary<int,int> cityRoadsTurn;
 public const int CityRoadReach=10,CityRoadEvery=3;
 // A people whose towns go hungry farms before it builds roads.
 bool hungerNow(int owner){return aiHunger;}
 public HashSet<int> CityRoadNeeds(int owner){
  if(cityRoads==null){cityRoads=new Dictionary<int,HashSet<int>>();cityRoadsTurn=new Dictionary<int,int>();}
  HashSet<int> have;int when;if(cityRoads.TryGetValue(owner,out have)&&cityRoadsTurn.TryGetValue(owner,out when)&&turn-when<CityRoadEvery){have.RemoveWhere(i=>tiles[i].road);return have;}
  var needs=new HashSet<int>();cityRoads[owner]=needs;cityRoadsTurn[owner]=turn;
  if(!HasTech(owner,"wheel"))return needs;
  var own=cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).ToList();if(own.Count<2)return needs;
  var joined=own.Where(Connected).ToList();if(joined.Count==0)return needs;
  foreach(var c in own){if(joined.Contains(c))continue;
   var to=joined.OrderBy(j=>Distance(j.x,j.y,c.x,c.y)).First();if(Distance(to.x,to.y,c.x,c.y)>CityRoadReach)continue;
   var course=RoadCourse(owner,to.x,to.y,c.x,c.y);if(course==null||course.Count==0)continue;
   // Only a road wholly on its own land joins the town, so a course that leaves its borders is not worth starting.
   if(course.Any(t=>t.owner!=owner))continue;
   foreach(var t in course)if(!t.road&&!IsWater(t.terrain))needs.Add(t.y*Width+t.x);}
  return needs;
 }
}
}
