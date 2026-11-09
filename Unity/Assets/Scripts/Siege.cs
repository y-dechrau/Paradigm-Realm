using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Sieges and stacks (owner request, 9 Nov 2026).
//  - A defender in its own city counts three times its strength (3:1 in its favour). An attacker standing on a hill brings that
//    down to 2:1, unless the city itself stands on a hill, when it stays 3:1.
//  - A stack attacks at the combined strength of its soldiers (ranged units fire on their own) and the damage it takes is shared
//    across them. Your stacks are the units you joined (G); rivals' soldiers on one tile attack together.
//  - The advantage of attacking alone: Skirmish. A lone attacker keeps half its remaining movement, so it can pull back or press on,
//    and the rest of its stack can still act; a stack attack spends every member's move. (A flat +1 was tried first, but it upset
//    the designed matchups such as Spearman against Horseman.)
public partial class World {
 public const int CityDefenceRatio=3,HillAttackRatio=2,MaxAttackGroup=6;
 public static bool IsHill(Tile t){return t!=null&&t.terrain==2;}
 // How many times the defender's strength counts: 3 in its city (2 if the attacker is on a hill, 3 again if the city is on one).
 public int CityRatio(Unit attacker,Unit defender){
  if(attacker==null||defender==null)return 1;var city=cities.Find(c=>c.x==defender.x&&c.y==defender.y&&c.owner==defender.owner);if(city==null)return 1;
  if(IsHill(At(city.x,city.y)))return CityDefenceRatio;return IsHill(At(attacker.x,attacker.y))?HillAttackRatio:CityDefenceRatio;
 }
 // The soldiers that attack together with this one.
 public List<Unit> AttackGroup(Unit u){
  var group=new List<Unit>{u};if(u==null||Power[u.type]==0)return group;
  IEnumerable<Unit> mates=u.owner==0?(u.stack!=0?StackOf(u):Enumerable.Empty<Unit>()):units.Where(a=>a.owner==u.owner);
  foreach(var m in mates){if(group.Count>=MaxAttackGroup)break;if(m==u||m.x!=u.x||m.y!=u.y||m.hp<=0||m.moves<=0||Power[m.type]==0||IsRanged(m.type)||IsNaval(m.type)!=IsNaval(u.type))continue;group.Add(m);}
  return group;
 }
 public int GroupAttack(List<Unit> group,Unit defender){int a=0;foreach(var m in group)a+=Strength(m,defender,false);return a;}
 // Skirmish: a lone attacker keeps half the movement it had left.
 public static int SkirmishMoves(int movesBefore){return movesBefore/2;}
 // Damage shared as evenly as it goes, the strongest taking any remainder first.
 public void ShareDamage(List<Unit> group,int taken){
  if(group.Count==0||taken<=0)return;int each=taken/group.Count,rest=taken%group.Count;
  foreach(var m in group.OrderByDescending(m=>m.hp).ThenBy(m=>m.id)){m.hp-=each+(rest>0?1:0);if(rest>0)rest--;}
 }
 public string SiegeNote(Unit attacker,int x,int y){
  var group=AttackGroup(attacker);string s=group.Count>1?" Your stack of "+group.Count+" attacks together and shares the damage.":" Alone: Skirmish, it keeps half its movement after the attack to pull back or press on.";
  var d=BestDefender(attacker,x,y);int r=CityRatio(attacker,d);if(r>1)s+=" The city's defenders count "+r+" times over"+(r==HillAttackRatio?" (you attack from a hill)":IsHill(At(x,y))?" (the city stands on a hill)":"")+".";
  return s;
 }
}
}
