using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Dormant worlds (owner request, 21 Nov 2026: turns slowed down; measured on the owner's game at turn 60, the 70 peoples of
// the parallel worlds took 0.85-1.2 s of a 1.6-2.1 s End Turn while every gate was still sealed). A parallel world that no
// home civilisation has discovered, opened a gate to or set foot in is dormant: each of its peoples takes its whole turn
// (economy, research, plans, sight) only once every DormantEvery turns, in rotation, and that turn counts for the turns it
// skipped (food, shields, science and gold multiplied). Technology already keeps pace with the home world (KeepPace). When
// any home civilisation discovers the world, opens a gate to it or has a unit there, it wakes and every people there runs
// every turn again with everything it built while dormant. Nothing is saved: dormancy is worked out each turn.
public partial class World {
 public const int DormantEvery=3;
 public bool dormantWorlds;
 [NonSerialized] bool[] realmAwake;[NonSerialized] int realmAwakeTurn=-1;
 public bool RealmAwake(int r){
  if(r<=0||!dormantWorlds)return true;
  if(realmAwake==null||realmAwakeTurn!=turn||realmAwake.Length<=r){
   realmAwakeTurn=turn;int count=Math.Max(RealmCount,r)+1;realmAwake=new bool[count];realmAwake[0]=true;
   if(!gatesLocked){for(int k=0;k<count;k++)realmAwake[k]=true;return true;}
   // A gate open (or cycling open) to a world, or a home civilisation's unit in it.
   foreach(int i in GateCells()){var t=tiles[i];if(t.realm!=0||!GateOpen(t))continue;var twin=GateTwin(t);if(twin!=null&&twin.realm<count)realmAwake[twin.realm]=true;}
   foreach(var u in units){if(IsNativeNation(u.owner))continue;var t=At(u.x,u.y);if(t!=null&&t.realm>0&&t.realm<count)realmAwake[t.realm]=true;}
   foreach(var c in colonies??new List<Colony>()){var t=At(c.x,c.y);if(t!=null&&t.realm>0&&t.realm<count)realmAwake[t.realm]=true;}
   // A world a home civilisation has seen any of.
   for(int rr=1;rr<count;rr++){if(realmAwake[rr])continue;for(int o=0;o<civCount&&!realmAwake[rr];o++)if(!IsNativeNation(o)&&RealmDiscovered(o,rr))realmAwake[rr]=true;}
  }
  return r<realmAwake.Length&&realmAwake[r];
 }
 public bool Dormant(int owner){if(!dormantWorlds||!IsNativeNation(owner))return false;int r=CivilisationCatalog.NativeRealm(nations[owner].civ);return !RealmAwake(r)&&!HomeWarWith(owner);}
 bool HomeWarWith(int owner){for(int o=0;o<civCount;o++)if(!IsNativeNation(o)&&AtWar(o,owner))return true;return false;}
 // A dormant people acts on one turn in DormantEvery, staggered so the work is spread evenly.
 public bool DormantSkips(int owner){return Dormant(owner)&&(turn+owner)%DormantEvery!=0;}
 int Catchup(int owner){return Dormant(owner)?DormantEvery:1;}
}
}
