using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// The empire's priorities (owner request, 6 Nov 2026, after Galactic Civilizations II): three linked sliders share each city's
// Trade between science, the army and the treasury. They always total 100; moving one moves the other two in proportion.
// The military share becomes extra shields for any combat unit a city is training. Rivals keep it at 0 for now.
public sealed class BorderGrowth {public int radius,done,span,turnsLeft=-1,citizensAt=-1;public bool full;}
// What a discovery asks of the player (owner request, 6 Nov 2026): what to research next, then each idle city that gained a building.
public sealed class BuildPrompt {public int city,tech;public List<int> options=new List<int>();}
public partial class World {
 public const int AxisScience=0,AxisMilitary=1,AxisFinance=2;
 public int FinanceRate(int owner){return 100-nations[owner].scienceRate-nations[owner].militaryRate;}
 static int RoundTen(int v){return (v+5)/10*10;}
 public bool SetPriority(int owner,int axis,int value){
  if(owner<0||owner>=civCount||axis<0||axis>2)return false;var n=nations[owner];int cap=ScienceCap(owner);
  value=Math.Max(0,Math.Min(axis==AxisScience?cap:100,RoundTen(value)));
  var rates=new[]{n.scienceRate,n.militaryRate,FinanceRate(owner)};int a=(axis+1)%3,b=(axis+2)%3,rest=100-value,total=rates[a]+rates[b];
  int first=total>0?RoundTen(rest*rates[a]/total):RoundTen(rest/2);first=Math.Max(0,Math.Min(rest,first));rates[axis]=value;rates[a]=first;rates[b]=rest-first;
  // Science has a ceiling (60%, or 80% with the Academies edict); what it cannot take goes to the treasury.
  if(rates[AxisScience]>cap){int spill=rates[AxisScience]-cap;rates[AxisScience]=cap;rates[axis==AxisFinance?AxisMilitary:AxisFinance]+=spill;}
  n.scienceRate=rates[AxisScience];n.militaryRate=rates[AxisMilitary];n.tax=TaxLevel(n.scienceRate+n.militaryRate);return true;
 }
 // Shields this city's military share adds per turn, whether or not it is training anything.
 public int ArmyBudget(City c){int rate=nations[c.owner].militaryRate;if(rate<=0||Disorder(c))return 0;int shields=Trade(c)*rate/100;return c.occupation>0?shields/2:shields;}
 public static bool IsCombatBuild(int project){int u=BuildUnit(project);return u>=0&&Power[u]>0;}
 public int ArmyShields(City c,int project){return IsCombatBuild(project)?ArmyBudget(c):0;}
 // Shields per turn on the line that holds this project.
 public int LineOutput(City c,int project){return Math.Max(1,Yield(c)[1])+ArmyShields(c,project);}
 // When the city's influence next spreads a ring: with age (one ring per 25 turns up to 4) or with size (ring 5 at size 30).
 public BorderGrowth NextBorderGrowth(City c){
  int r=TerritoryRadius(c);var g=new BorderGrowth{radius=r};
  if(r>=5){g.full=true;return g;}
  g.citizensAt=(r-1)*10;
  if(r<4){int from=(r-2)*25,to=(r-1)*25;g.span=to-from;g.done=Math.Max(0,Math.Min(g.span,c.age-from));g.turnsLeft=Math.Max(1,to-c.age);}
  else{g.span=g.citizensAt;g.done=Math.Min(g.span,c.pop);}
  return g;
 }
 // The path a unit's current order will take, for the route line; it goes through other units if they block the way today.
 public List<Tile> PlannedRoute(Unit u){
  if(u==null||!(u.hasOrder||u.roadOrder))return new List<Tile>();int x=u.roadOrder?u.roadX:u.destinationX,y=u.roadOrder?u.roadY:u.destinationY;
  var path=Route(u,x,y);return path.Count>0?path:Route(u,x,y,true);
 }
 [NonSerialized] public List<int> researchPrompts=new List<int>();
 [NonSerialized] public List<BuildPrompt> buildPrompts=new List<BuildPrompt>();
 [NonSerialized] Dictionary<int,List<int>> buildableBefore;
 bool BuildingLineIdle(City c){return c.buildingProject<0&&c.buildingQueue.Count==0;}
 void BeginDiscovery(){buildableBefore=new Dictionary<int,List<int>>();foreach(var c in cities.Where(a=>a.owner==0&&BuildingLineIdle(a)))buildableBefore[c.id]=BuildingIds.Where(b=>TechAllows(c,b)).ToList();}
 void EndDiscovery(int found){
  if(researchPrompts==null)researchPrompts=new List<int>();if(buildPrompts==null)buildPrompts=new List<BuildPrompt>();
  researchPrompts.Add(found);
  foreach(var c in cities.Where(a=>a.owner==0&&buildableBefore.ContainsKey(a.id))){var before=buildableBefore[c.id];var fresh=BuildingIds.Where(b=>!before.Contains(b)&&TechAllows(c,b)&&BuildReason(c,b)=="").ToList();if(fresh.Count>0)buildPrompts.Add(new BuildPrompt{city=c.id,tech=found,options=fresh});}
  buildableBefore=null;
 }
 // The next city still waiting for an answer; one that has since started a building, or is gone, no longer needs asking.
 public BuildPrompt NextBuildPrompt(){
  if(buildPrompts==null)return null;
  while(buildPrompts.Count>0){var p=buildPrompts[0];var c=cities.Find(a=>a.id==p.city&&a.owner==0);if(c!=null&&BuildingLineIdle(c)){p.options=p.options.Where(b=>BuildReason(c,b)=="").ToList();if(p.options.Count>0)return p;}buildPrompts.RemoveAt(0);}
  return null;
 }
}
}
