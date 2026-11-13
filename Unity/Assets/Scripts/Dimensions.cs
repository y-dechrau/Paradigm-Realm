using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Dimensions: the shared data of the gate-and-colony design (owner brief, 11 Nov 2026; plan and decisions in
// Design/DIMENSIONS-PLAN-2026-11-11.md; save version 42). The parallel worlds, their gateways and the gateway expedition already
// existed (Realms.cs, GateExpedition.cs). This file adds what the later stages share:
//  - a gateway's CONDITION (sealed, cycling, dormant, damaged, constructed), kept apart from what anyone has learned about it;
//  - dimensional FAMILIES with consistent physical rules, expressed as capability tags that fail there;
//  - compact COLONIES and per-family local ADAPTATION records.
// Everything here is data and small queries; the rules that use it are in DimensionGates.cs, DimensionRules.cs, Colonies.cs,
// Adaptation.cs and DimensionAI.cs.
public static class GateCondition {
 public const int Sealed=0,Cycling=1,Dormant=2,Damaged=3,Constructed=4;
 public static readonly string[] Names={"Sealed","Naturally cycling","Dormant","Damaged","Constructed"};
 public static readonly string[] Texts={
  "Held shut by a seal. It asks for something before it will open.",
  "Opens and closes by itself at regular intervals. It can be used while open, and steadied so that it stays open.",
  "Whole but asleep. Its own mechanism will wake it once it is understood.",
  "Broken. It must be repaired before it can be woken.",
  "Built by a civilisation that understands gateways."};
}
// One per gateway pair. period, openFor and offset are in turns; capacity is in loads a turn (Colonies.cs); repair is the
// production still needed to mend a damaged gateway.
[Serializable] public class GateState {
 public int pair,condition,period=DimensionSettings.CyclePeriod,openFor=DimensionSettings.CycleOpen,offset,capacity=DimensionSettings.GateCapacity,repair,priority,builder=-1;
}
// Balance values in one place. All are first guesses to be tuned by play.
public static class DimensionSettings {
 public const int CyclePeriod=10,CycleOpen=4;          // a cycling gateway is open 4 turns in every 10
 public const int GateCapacity=6;                       // loads a gateway carries each turn it is open
 public const int InvestigateRange=12,InvestigateCost=40,RepairCost=90,ActivateGold=20;
 public const int LegendTurn=40,LegendReach=40;           // from this turn a civilisation that knows no gateway hears of the nearest one within reach of its capital
 public const int ColonyLoad=1;                         // loads a turn one colony's supplies take
 public const int HomeGateLimit=6;                      // gateways placed on the home world (one per world today)
}
// Physical rules shared by the worlds of a family. A capability tag names something equipment depends on; a family lists the
// tags that fail there. Knowledge is never blocked, only what depends on local physics.
public static class DimensionFamily {
 public const int Home=0,Ember=1,Resonant=2;
 public const string Combustion="combustion",Electrical="electrical";
 public static readonly string[] Names={"Home physics","Ember","Resonant"};
 public static readonly string[] Rules={
  "The physics your people know.",
  "Fire will not keep itself alight here: nothing burns for long, so powder, furnaces and engines fail. The ground is hot and full of steam and pressure.",
  "Electric current will not hold steady here: it surges and dies. Certain crystals ring in sympathy over great distances and can be made to do work."};
 public static readonly string[][] Fails={new string[0],new[]{Combustion},new[]{Electrical}};
 // Which family each world belongs to (worlds in Realms.RealmNames order). Two families to begin with; the Plague Marshes and
 // the Hive Expanse keep home physics until the first two play well.
 public static readonly int[] OfRealmTable={Home,Resonant,Ember,Ember,Home,Resonant,Home,Home};
 public static int OfRealm(int realm){return realm<0||realm>=OfRealmTable.Length?Home:OfRealmTable[realm];}
 public static bool Blocks(int family,string tag){if(family<0||family>=Fails.Length)return false;return Array.IndexOf(Fails[family],tag)>=0;}
}
// What a thing depends on, and which of its functions stops when that fails.
public class CapabilityNeed {
 public const int Attack=0,Defence=1,Movement=2,Everything=3;
 public string tag;public int function;public string what;
 public CapabilityNeed(string tag,int function,string what){this.tag=tag;this.function=function;this.what=what;}
}
public static class CapabilityCatalog {
 // By unit type (World.UnitNames). Only the Rifleman carries equipment that depends on local physics today; the table is where
 // later units declare theirs.
 static readonly Dictionary<int,CapabilityNeed[]> units=new Dictionary<int,CapabilityNeed[]>{
  {6,new[]{new CapabilityNeed(DimensionFamily.Combustion,CapabilityNeed.Attack,"its firearms")}},
 };
 static readonly CapabilityNeed[] none=new CapabilityNeed[0];
 public static CapabilityNeed[] OfUnit(int type){CapabilityNeed[] found;return units.TryGetValue(type,out found)?found:none;}
 // Attack a unit falls back on when its weapon fails: bayonets and rifle butts.
 public const int FallbackAttack=2;
}
// A compact settlement beyond a gateway (Colonies.cs).
[Serializable] public class Colony {
 public int id,owner,x,y,specialisation,second=-1,policy,stock,selfSufficiency,founded,gate=-1,supplied,isolated,level=1,homeCity=-1,shipShields,shipGold,siege;// supplied: loads of supplies received last turn; shipShields and shipGold: what its exports brought its home city last turn
 public string name="";public List<int> buildings=new List<int>();
}
// What one civilisation has worked out about living in one family of worlds (Adaptation.cs).
[Serializable] public class Adaptation {
 public int owner,family,points,current=-1,funding;public List<int> known=new List<int>();
}
public partial class World {
 public const int SaveVersion=43;
 public static int DimensionFamilyTableLength{get{return DimensionFamily.OfRealmTable.Length;}}
 public List<GateState> gateStates=new List<GateState>();
 public List<Colony> colonies=new List<Colony>();
 public List<Adaptation> adaptations=new List<Adaptation>();
 // True once this game uses gateway conditions other than sealed (new games from the menu; older saves stay as they were).
 public bool dimensionRules;
 public int FamilyAt(Tile t){return t==null?DimensionFamily.Home:DimensionFamily.OfRealm(t.realm);}
 // The home-side tile of every gateway pair.
 public List<Tile> HomeGates(){var list=new List<Tile>();foreach(int i in GateCells())list.Add(tiles[i]);return list;}
 // Both ends of every gateway.
 public List<Tile> AllGates(){var list=new List<Tile>();foreach(int i in GateCells()){list.Add(tiles[i]);var twin=GateTwin(tiles[i]);if(twin!=null)list.Add(twin);}return list;}
 public GateState StateOf(Tile gate)
 {
  if(gate==null||gate.gateLink<=0)return null;if(gateStates==null)gateStates=new List<GateState>();int pair=GatePair(gate);
  foreach(var s in gateStates)if(s.pair==pair)return s;
  var made=new GateState{pair=pair,condition=GateCondition.Sealed,offset=pair%DimensionSettings.CyclePeriod};gateStates.Add(made);return made;
 }
 // Every gateway has a state; lists exist. Called for older saves and whenever gateways are placed.
 internal void EnsureDimensionData()
 {
  if(gateStates==null)gateStates=new List<GateState>();if(colonies==null)colonies=new List<Colony>();if(adaptations==null)adaptations=new List<Adaptation>();if(accords==null)accords=new List<DimensionAccord>();if(gatePriority==null||gatePriority.Length<MaxNations)gatePriority=new int[MaxNations];
  foreach(var g in HomeGates())StateOf(g);
  gateStates.RemoveAll(s=>s==null||s.pair<0||s.pair>=tiles.Length||tiles[s.pair].gateLink<=0);
 }
 // New games: the gateways are not all alike. Of the two Ember worlds' gateways one cycles by itself and the other is dormant; of
 // the two Resonant worlds' one is dormant and the other damaged; gateways to worlds with home physics stay sealed. So four of
 // six can be opened by investigation and ordinary means, wherever a civilisation happens to start.
 public void EnableDimensions()
 {
  EnsureDimensionData();dimensionRules=true;bool cycling=false,dormant=false,damaged=false;
  foreach(var g in HomeGates().OrderBy(t=>GateRealm(t)))
  {
   var s=StateOf(g);int family=DimensionFamily.OfRealm(GateRealm(g));
   if(family==DimensionFamily.Ember&&!cycling){s.condition=GateCondition.Cycling;cycling=true;}
   else if(family==DimensionFamily.Resonant&&!dormant){s.condition=GateCondition.Dormant;dormant=true;}
   else if(family==DimensionFamily.Resonant&&!damaged){s.condition=GateCondition.Damaged;s.repair=DimensionSettings.RepairCost;damaged=true;}
   else if(family!=DimensionFamily.Home)s.condition=GateCondition.Dormant;
   else s.condition=GateCondition.Sealed;
  }
 }
 bool DimensionDataValid()
 {
  if(gateStates==null||colonies==null||adaptations==null||!AccordsValid())return false;
  foreach(var s in gateStates)if(s==null||s.pair<0||s.pair>=tiles.Length||s.condition<0||s.condition>GateCondition.Constructed||s.period<1||s.openFor<0||s.capacity<0||s.repair<0)return false;
  foreach(var c in colonies)if(c==null||c.owner<0||c.owner>=civCount||At(c.x,c.y)==null||c.buildings==null||c.stock<0)return false;
  foreach(var a in adaptations)if(a==null||a.owner<0||a.owner>=civCount||a.family<0||a.family>AdaptationCatalog.Synthesis||a.known==null||a.points<0)return false;
  return true;
 }
}
}
