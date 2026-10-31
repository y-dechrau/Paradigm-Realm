using System;
using System.Linq;
namespace ParadigmRealm {
// Worker orders (owner request, 31 Oct 2026): a road route builds a road on every tile of the cheapest way to a destination
// (R, then right-drag), and auto develop hands the worker to the same planner the rival workers use (ImproveAI).
public partial class World {
 public string RoadRoute(Unit u,int x,int y){
  if(u==null||u.type!=1)return "Only workers build roads.";if(!HasTech(u.owner,"wheel"))return "Requires The Wheel.";
  var goal=At(x,y);if(goal==null)return "That is beyond the edge of the world.";
  if(!(u.x==goal.x&&u.y==goal.y)&&Route(u,goal.x,goal.y).Count==0)return "No land route to that destination.";
  foreach(var a in WorkerTeam(u)){CancelOrder(a);a.autoWork=false;a.roadOrder=true;a.roadX=goal.x;a.roadY=goal.y;}
  foreach(var a in WorkerTeam(u))RoadStep(a);
  return "Road ordered: the worker builds along the way.";
 }
 // One turn of a road route: build here if there is no road yet, otherwise take one step on the way and start building.
 public void RoadStep(Unit u){
  if(!u.roadOrder||u.job>0)return;
  var here=At(u.x,u.y);
  if(!here.road){if(JobReason(u,1)=="")StartJob(u,1);else u.roadOrder=false;return;}
  if(u.x==u.roadX&&u.y==u.roadY){u.roadOrder=false;if(u.owner==0)Note("Road finished.");return;}
  var path=Route(u,u.roadX,u.roadY);if(path.Count==0){u.roadOrder=false;if(u.owner==0)Note("Road stopped: no route.");return;}
  var next=path[0];if(u.moves<StepCost(u,u.x,u.y,next)||!Move(u,next.x,next.y))return;
  if(!At(u.x,u.y).road&&u.moves>0&&JobReason(u,1)=="")StartJob(u,1);
 }
 public void SetAutoWork(Unit u,bool on){if(u==null||u.type!=1)return;foreach(var a in WorkerTeam(u)){CancelOrder(a);a.roadOrder=false;a.autoWork=on;}if(on&&u.moves>0)ImproveAI(u);}
}
}
namespace ParadigmRealm {
// Sight (owner request, 31 Oct 2026): units see 2 tiles (scouts 4). The first Settler sees 4 from its standing start until it
// moves. A unit on a hill sees one tile further, except where the next tile towards the target is itself a hill or mountain:
// then it sees only that tile in that direction.
public partial class World {
 public int SightRange(Unit u){int r=u.type==3?4:2;if(u.type==0&&turn==1&&u.moves==Moves[0]&&!u.hasOrder)r=Math.Max(r,4);var here=At(u.x,u.y);if(here!=null&&here.terrain==2)r++;return r;}
 static bool HighGround(int terrain){return terrain==2||terrain==5||terrain==15||terrain==17;}
 public bool CanSee(Unit u,Tile t){
  if(t==null)return false;int d=Distance(u.x,u.y,t.x,t.y);if(d>SightRange(u))return false;if(d<=1)return true;
  var here=At(u.x,u.y);if(here==null||here.terrain!=2)return true;
  Tile next=null;int best=int.MaxValue;for(int dj=-1;dj<=1;dj++)for(int di=-1;di<=1;di++){if(di==0&&dj==0)continue;var n=Step(u.x,u.y,di,dj);if(n==null)continue;int nd=Distance(n.x,n.y,t.x,t.y);if(nd<best){best=nd;next=n;}}
  return next==null||!HighGround(next.terrain);
 }
 void SeeAround(Unit u,Action<Tile> seen){int r=SightRange(u);for(int dj=-r;dj<=r;dj++)for(int di=-r;di<=r;di++){var t=Step(u.x,u.y,di,dj);if(t!=null&&CanSee(u,t))seen(t);}}
}
}
namespace ParadigmRealm {
// Ancient ruins (owner request, 31 Oct 2026): rare, scattered at random on open land away from the starts. The first land
// unit to step in explores them once: gold, research, a map of the surroundings, or battle-hardened veterans.
public partial class World {
 public void SeedRuins(){
  var r=new Random(seed^0x51ad);int wanted=Math.Max(2,tiles.Length/320),made=0;
  foreach(var t in tiles.OrderBy(t=>Lattice(seed+77,t.x,t.y,9))){
   if(made>=wanted)break;if(IsWater(t.terrain)||IsImpassable(t.terrain)||Barren(t.terrain)||t.village||t.ruin)continue;
   if(cities.Any(c=>Distance(c.x,c.y,t.x,t.y)<5)||units.Any(u=>Distance(u.x,u.y,t.x,t.y)<4)||tiles.Any(o=>o.ruin&&Distance(o.x,o.y,t.x,t.y)<8))continue;
   if(r.Next(3)==0)continue;t.ruin=true;made++;
  }
 }
 void ExploreRuin(Unit u){
  var t=At(u.x,u.y);if(t==null||!t.ruin||IsNaval(u.type))return;t.ruin=false;var n=nations[u.owner];int roll=Math.Abs(seed*31+t.x*17+t.y*13+turn)%4;string what;
  if(roll==0){int g=25+Math.Abs(t.x*7+t.y)%4*10;n.gold+=g;if(u.owner==0)gold=n.gold;what="a buried treasury: "+g+" gold";}
  else if(roll==1&&n.research>=0){int gain=Math.Max(10,ResearchCost(n.research,u.owner)/2);n.progress[n.research]+=gain;what="old scrolls: +"+gain+" towards "+TechNames[n.research];}
  else if(roll==2){for(int dj=-6;dj<=6;dj++)for(int di=-6;di<=6;di++){var s=Step(u.x,u.y,di,dj);if(s!=null)LearnTile(u.owner,s);}what="an ancient map of the land around";}
  else{if(Power[u.type]>0){u.victories=Math.Max(u.victories,2);u.promotion=Math.Max(u.promotion,1);what="a war shrine: the "+UnitNames[u.type]+" becomes a Veteran";}else{n.gold+=30;if(u.owner==0)gold=n.gold;what="old trade goods: 30 gold";}}
  if(u.owner==0)Note(UnitNames[u.type]+" explored ancient ruins and found "+what+".");
 }
}
}
