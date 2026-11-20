using System;
using System.Linq;
namespace ParadigmRealm {
// Rivals use the national forces rules too (owner brief, 20 Nov 2026). Once a turn, before its cities and units act, a rival:
//  - mobilises when at war and short of personnel, and stands down after the policy's minimum once at peace;
//  - keeps at most three orders, and only orders it can really fill (RaiseReason must pass);
//  - raises towards its old soldier limit (SoldierLimit: 3 + 2 per city, more at war), choosing the strongest formation it can
//    field, every fourth one ranged, mounted only with free horse support, deployed at the city nearest danger;
//  - refits one formation when nothing new is wanted.
public partial class World {
 static readonly int[] LandFormations={2,7,6,4,8,5,9};
 void ForcesAI(int owner){
  if(!nationalForces||owner==0)return;var n=nations[owner];var list=Orders(owner);bool war=AtWarWithAnyone(owner);
  // Orders it can no longer deploy or afford in personnel are given up (they would sit in the programme for ever).
  list.RemoveAll(o=>o.kind==0&&!o.held&&o.paid==0&&!FormationKnown(owner,o.type));
  int want=SoldierLimit(owner)-SoldierCount(owner)-list.Count(o=>o.kind==0&&!IsNaval(o.type));
  if(war&&!n.mobilised&&want>0&&ServiceFree(owner)<2)SetMobilised(owner,true);
  else if(!war&&n.mobilised)SetMobilised(owner,false);
  if(list.Count>=3)return;
  if(want>0&&list.Count(o=>o.kind==0)<2){int type=FormationChoice(owner);if(type>=0){var at=ThreatenedCity(owner,type);Raise(owner,type,at);}return;}
  if(list.Count==0){var u=units.FirstOrDefault(x=>x.owner==owner&&!x.refitting&&Formation(x)&&UpgradeOf(x.type)>=0&&RefitReason(owner,x)=="");if(u!=null)Refit(owner,u);}
 }
 int FormationChoice(int owner){
  int soldiers=SoldierCount(owner),ranged=units.Count(u=>u.owner==owner&&IsRanged(u.type));bool wantRanged=soldiers>=3&&ranged*4<soldiers;
  int best=-1,score=int.MinValue;
  foreach(int t in LandFormations){if(IsObsolete(owner,t)||RaiseReason(owner,t,null)!="")continue;int s=MilitaryValue(t)*10-ForcesRules.Service[t]*3+(wantRanged&&IsRanged(t)?40:0);if(s>score){score=s;best=t;}}
  return best;
 }
 // The city with an enemy soldier nearest it, else the capital.
 City ThreatenedCity(int owner,int type){
  City best=null;int least=int.MaxValue;
  foreach(var c in cities){if(c.owner!=owner||!CanDeployAt(c,type))continue;foreach(var u in units){if(Power[u.type]==0||u.owner==owner||!AtWar(owner,u.owner))continue;int d=Distance(u.x,u.y,c.x,c.y);if(d<least){least=d;best=c;}}}
  return least<=8?best:DeployCity(owner,type);
 }
}
}
