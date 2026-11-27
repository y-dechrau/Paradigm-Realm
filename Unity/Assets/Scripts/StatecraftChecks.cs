using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Statecraft (Statecraft.cs, 24 Nov 2026): off unless switched on, what each choice does, the settling time, the wait, the dearer
// technology of a large realm, the preview, rivals choosing, and saving.
public static class StatecraftChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Statecraft: "+what);}
 static World Setup(int seed=421){var w=World.New(seed,"Realm",0,1,1,0,1,2,4,hex:true,natives:false,rations:true);w.nationalForces=true;w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.turn=60;return w;}
 static void Learn(World w,int o){w.nations[o].tech[TechnologyCatalog.Index(StatecraftRules.Tech)]=true;if(o==0)w.tech=w.nations[0].tech;}
 // Paper cities beyond the first, for rules that only count them.
 static void Grow(World w,int total){var home=w.cities.First(c=>c.owner==0);foreach(var t in w.tiles){if(w.cities.Count(c=>c.owner==0)>=total)break;if(t.realm!=0||PixelArt.IsWater(t.terrain)||World.IsImpassable(t.terrain)||w.cities.Any(c=>w.Distance(c.x,c.y,t.x,t.y)<4))continue;w.cities.Add(new City{id=w.nextId++,owner=0,x=t.x,y=t.y,name="Town "+w.nextId});}Check(w.cities.Count(c=>c.owner==0)==total,"room for "+total+" towns");w.RebuildTerritory();}
 public static void Run(){
  Check(StatecraftRules.CentralPercent(3)==40&&StatecraftRules.CentralPercent(1)==40&&StatecraftRules.CentralPercent(4)==32&&StatecraftRules.CentralPercent(8)==0&&StatecraftRules.CentralPercent(20)==0&&StatecraftRules.CentralPercent(0)==0,"the Centralised bonus fades a step at a time");
  Check(StatecraftRules.TechPercent(4)==0&&StatecraftRules.TechPercent(5)==3&&StatecraftRules.TechPercent(40)==45,"technology is dearer past the fourth city, up to a limit");
  // Off unless switched on.
  {var w=Setup();Learn(w,0);int cost=w.ResearchCost(5,0);Grow(w,7);Check(!w.SetStatecraft(0,1)&&w.StatecraftOf(0)==0&&w.ResearchCost(5,0)==cost,"nothing changes when off");}
  // Choosing: the technology, the settling time, the wait.
  {var w=Setup();w.EnableStatecraft();Check(w.StatecraftReason(0,1).Contains("Requires"),"needs Code of Laws ("+w.StatecraftReason(0,1)+")");Learn(w,0);var c=w.cities.First(a=>a.owner==0);c.pop=6;int content=w.ContentSlots(c);var before=w.Yield(c);
   Check(w.StatecraftReason(0,0)!=""&&w.SetStatecraft(0,1),"Centralised rule can be chosen");Check(w.StatecraftUnsettled(0)&&w.StatecraftOf(0)==0&&w.ContentSlots(c)==content-StatecraftRules.Unrest&&w.Yield(c)[1]==before[1],"nothing counts while it settles, and the cities are less content");
   Check(!w.SetStatecraft(0,2)&&w.StatecraftReason(0,2).Contains("must stand"),"and it must stand");
   w.turn+=StatecraftRules.SettleTurns;Check(w.StatecraftOf(0)==1&&w.ContentSlots(c)==content,"then it counts");var after=w.Yield(c);
   Check(after[1]==before[1]+World.ShareOf(before[1],40)&&after[2]==before[2]+World.ShareOf(before[2],40)&&after[3]==before[3]+World.ShareOf(Math.Max(0,before[3]),40)&&after[0]==before[0],"one city: +40% shields, science and gold, and no more food ("+before[1]+" to "+after[1]+")");
   foreach(int stat in new[]{0,1,2,3})Check(w.YieldBreakdown(c,stat).Sum(p=>p.Value)==after[stat],"the city's sums still add up");
   int settler=w.CostFor(c,0);w.nations[0].statecraft=0;int plain=w.CostFor(c,0);Check(settler==plain+plain*StatecraftRules.SettlerDearer/100||settler>plain,"Settlers cost more ("+plain+" to "+settler+")");
   w.nations[0].statecraft=1;Grow(w,8);Check(w.StatecraftPercent(0)==0,"eight cities leave no bonus");
   w.turn+=StatecraftRules.MinTurns;Check(w.SetStatecraft(0,2),"after the wait it may change");w.turn+=StatecraftRules.SettleTurns;
   Check(w.ServiceCapacity(0,false)==ForcesRules.BaseService+w.Citizens(0)*StatecraftRules.ServicePercent/100&&w.ServiceCapacity(0,true)==ForcesRules.BaseService+w.Citizens(0),"Expansive: more serve in peacetime, mobilising is unchanged");
   Check(w.CostFor(c,0)<plain&&w.StatecraftExpansion(0,30)==30-30*StatecraftRules.ExpansionCheaper/100,"Expansive: cheaper Settlers and cities");Check(w.Valid(),"the game stays valid");}
  // Size and technology, for every civilisation of this world.
  {var w=Setup();int cost=w.ResearchCost(5,0);w.EnableStatecraft();Check(w.ResearchCost(5,0)==cost,"four cities or fewer cost nothing");Grow(w,9);Check(w.ResearchCost(5,0)==cost+cost*15/100,"nine cities make technology 15% dearer ("+cost+" to "+w.ResearchCost(5,0)+")");}
  // Soldiers raised as Veterans; influence from pacts.
  {var w=Setup();w.EnableStatecraft();Learn(w,0);w.SetStatecraft(0,1);w.turn+=StatecraftRules.SettleTurns;int plain=w.InfluencePerTurn(0);w.met[1]=true;w.SetPactForChecks(0,1,Pacts.Borders);w.SetPactForChecks(0,1,Pacts.NonAggression);Check(w.InfluencePerTurn(0)==plain+2,"influence for each pact");
   var c=w.cities.First(a=>a.owner==0);c.pop=8;w.units.RemoveAll(u=>u.owner==0&&u.type==2);Check(w.Raise(0,2,c)=="","a Warrior is ordered");for(int k=0;k<40&&!w.units.Any(u=>u.owner==0&&u.type==2);k++)w.ForcesTurnForChecks(0);var made=w.units.FirstOrDefault(u=>u.owner==0&&u.type==2);Check(made!=null&&made.promotion==1,"it is raised a Veteran");}
  // The preview agrees with what happens, and leaves nothing behind.
  {var w=Setup();w.EnableStatecraft();Learn(w,0);var f=w.PreviewStatecraft(0,1);Check(w.nations[0].statecraft==0&&w.nations[0].statecraftSince==-99,"the preview changes nothing");w.SetStatecraft(0,1);w.turn+=StatecraftRules.SettleTurns;int s=w.cities.Where(c=>c.owner==0).Sum(c=>w.Yield(c)[2]);Check(f.science==s&&f.service==w.ServiceCapacity(0)&&f.bonus==40,"and it is right ("+f.science+", "+s+")");}
  // Rivals choose, change seldom, and a game with it saves.
  {var g=World.New(422,"Whole",0,1,1,0,1,2,5,hex:true,natives:false,wild:true,rations:true);g.nationalForces=true;g.EnableStatecraft();g.Found(g.units.First(u=>u.owner==0&&u.type==0));
   for(int o=1;o<g.civCount;o++)Learn(g,o);int changes=0;var last=new int[g.civCount];
   for(int t=0;t<120&&g.result=="";t++){for(int a=0;a<g.civCount;a++)for(int o=a+1;o<g.civCount;o++){var rel=g.Relation(a,o);if(rel!=null)rel.peaceUntil=g.turn+10;}/* everyone at peace: a rival at war never changes how it governs (27 Nov 2026: with roads the rivals fought among themselves in this game) */g.EndTurn();for(int o=1;o<g.civCount;o++){if(g.nations[o].statecraft!=last[o])changes++;last[o]=g.nations[o].statecraft;}}
   Check(Enumerable.Range(1,g.civCount-1).Any(o=>g.nations[o].statecraft!=0),"some rival chooses (turn "+g.turn+(g.result!=""?", ended: "+g.result:"")+"; "+string.Join("; ",Enumerable.Range(1,g.civCount-1).Select(o=>g.NationName(o)+" towns "+g.cities.Count(c=>c.owner==o)+" posture "+g.Posture(o)+" wants "+g.StatecraftWanted(o)).ToArray())+")");Check(changes<=g.civCount*5,"and they change seldom ("+changes+" times)");Check(g.Valid(),"the game stays valid");
   var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(g));Check(copy.statecraft&&Enumerable.Range(0,g.civCount).All(o=>copy.nations[o].statecraft==g.nations[o].statecraft&&copy.nations[o].statecraftSince==g.nations[o].statecraftSince),"choices are saved");}
 }
}
public partial class World {
 internal void ForcesTurnForChecks(int o){ForcesTurn(o);}
 internal void SetPactForChecks(int a,int b,int k){SetPact(a,b,k,true);}
}
}
