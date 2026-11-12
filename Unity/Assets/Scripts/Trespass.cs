using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Units left inside borders that grew over them (owner request, 12 Nov 2026). When a tile a unit stands on becomes another
// civilisation's land and there is no agreement that lets it be there (the same test as walking in: World.CanEnter, so open
// borders, a defensive pact or war all let it stay), the unit is put down on the nearest ground just outside those borders:
// the nearest tile of the same world it may stand on. Nothing is lost; its orders are cancelled. Ships are left alone.
// The rule is the same for every civilisation.
public partial class World {
 public const int EscortReach=14;
 // Where a trespassing unit would be set down, or null if it may stay or there is nowhere within reach.
 public Tile EscortTile(Unit u)
 {
  if(u==null||IsNaval(u.type))return null;var here=At(u.x,u.y);if(here==null||CanEnter(u.owner,here))return null;
  if(cities.Any(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y))return null;
  Tile best=null;int least=int.MaxValue;
  for(int dy=-EscortReach;dy<=EscortReach;dy++)for(int dx=-EscortReach;dx<=EscortReach;dx++)
  {
   var t=StepIn(u.x,u.y,dx,dy,EscortReach);if(t==null||t==here||t.realm!=here.realm||IsWater(t.terrain)||IsImpassable(t.terrain)||!CanEnter(u.owner,t))continue;
   int d=Distance(u.x,u.y,t.x,t.y);if(d>least||d==least&&best!=null&&t.y*Width+t.x>=best.y*Width+best.x)continue;
   if(units.Any(a=>a.owner!=u.owner&&a.x==t.x&&a.y==t.y)||cities.Any(c=>c.owner!=u.owner&&c.x==t.x&&c.y==t.y)||!Capacity(u,t.x,t.y))continue;
   least=d;best=t;
  }
  return best;
 }
 // Called for each civilisation in its turn, after borders have been redrawn.
 internal void ExpelTrespassers(int owner)
 {
  List<Unit> moved=null;string whose="";
  foreach(var u in units)
  {
   if(u.owner!=owner||IsNaval(u.type))continue;var here=At(u.x,u.y);if(here==null||here.owner<0||here.owner==owner||CanEnter(owner,here))continue;
   var to=EscortTile(u);if(to==null)continue;whose=NationName(here.owner);CancelOrder(u);u.fortified=false;u.x=to.x;u.y=to.y;if(moved==null)moved=new List<Unit>();moved.Add(u);
  }
  if(moved==null)return;networks=null;
  if(owner==0){Note(moved.Count==1?"Your "+UnitName(moved[0])+" stood on land that is now "+whose+"'s, and you have no agreement with them: it has been escorted to the edge of their borders.":moved.Count+" of your units stood on land that is now "+whose+"'s, and you have no agreement with them: they have been escorted to the edge of their borders.");Reveal();}
 }
}
}
