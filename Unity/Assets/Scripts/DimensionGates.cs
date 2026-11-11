using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Gateway conditions in play (plan stage 2, Design/DIMENSIONS-PLAN-2026-11-11.md).
//  - A naturally cycling gateway is open for part of every cycle, on a schedule anyone can read once it is found. It can be
//    used while open with no investigation at all, and steadied later (the Stabilise approach) so that it stays open.
//  - A dormant gateway needs its seal read (the investigation) and then only a small activation.
//  - A damaged gateway must first be repaired by a city (the Repair Gateway project), and is then dormant.
//  - A sealed gateway keeps the requirement of GateExpedition.cs.
//  - Constructed gateways are a later capability; the condition exists so later stages can use it.
// Investigation has two dependable routes and no dice: a Scout, Medic or Priest standing on the gateway for two turns (as
// before), or a city within reach running the Investigate Gateway project with its ordinary production. The method, its cost
// and how much it tells depend on what the civilisation knows.
public static class GateMethod {
 public static readonly string[] Names={"Observation and trial","Scholarly study","Instruments","Probes and sensing"};
 public static readonly string[] Tech={"","writing","printing","computers"};
 public static readonly int[] CostPercent={100,80,60,40};
 public static readonly string[] Texts={
  "Watchers camp by the arch, note when it stirs, and try its stones and levers until something answers.",
  "Scribes copy its inscriptions and compare them with every record your people keep.",
  "Surveyors measure it with instruments made for the purpose, and can tell exactly how dangerous the far side is.",
  "Machines are sent through and report back, and name the best sites beyond before anyone crosses."};
}
public partial class World {
 public const int InvestigateProject=65,RepairProject=66;
 public static bool IsGateProject(int b){return b==InvestigateProject||b==RepairProject;}
 GateState FindState(Tile gate){if(gate==null||gate.gateLink<=0||gateStates==null)return null;int pair=GatePair(gate);foreach(var s in gateStates)if(s.pair==pair)return s;return null;}
 public int ConditionOf(Tile gate){var s=FindState(gate);return s==null?GateCondition.Sealed:s.condition;}
 // ---------- cycling ----------
 int CyclePhase(GateState s){int p=Math.Max(1,s.period);return ((turn+s.offset)%p+p)%p;}
 public int CycleOpenTurns(GateState s){return Math.Min(Math.Max(1,s.period)-1,s.openFor+(SynthesisKnownByAny(1)?2:0));}
 public bool CycleOpen(GateState s){return s!=null&&s.condition==GateCondition.Cycling&&CyclePhase(s)<CycleOpenTurns(s);}
 // When a cycling gateway next changes: turns left open, or turns until it opens.
 public string GateSchedule(Tile gate)
 {
  var s=FindState(gate);if(s==null)return "";bool steady=openedGates!=null&&openedGates.Contains(GatePair(gate))||!gatesLocked;
  if(steady)return "Open.";
  if(s.condition!=GateCondition.Cycling)return GateCondition.Names[s.condition]+". "+GateCondition.Texts[s.condition];
  int phase=CyclePhase(s),open=CycleOpenTurns(s);
  return phase<open?"Open now, for "+(open-phase)+" more turn"+(open-phase==1?"":"s")+"; then shut for "+(s.period-open)+". It opens for "+open+" turns in every "+s.period+".":"Shut now; it opens in "+(s.period-phase)+" turn"+(s.period-phase==1?"":"s")+", for "+open+" turns in every "+s.period+".";
 }
 // ---------- investigation by a city ----------
 public int InvestigationMethod(int owner){int m=0;for(int k=1;k<GateMethod.Tech.Length;k++)if(HasTech(owner,GateMethod.Tech[k]))m=k;return m;}
 bool GateKnownTo(int owner,Tile gate){int cell=gate.y*Width+gate.x;if(owner==0)return gatesFound!=null&&gatesFound.Contains(cell)||gate.seen&&!gatesLocked;var known=nations[owner].known;return known!=null&&cell<known.Length&&known[cell];}
 public Tile GateToInvestigate(City c){Tile best=null;int least=int.MaxValue;var here=At(c.x,c.y);foreach(var g in AllGates()){if(g.gateLink<=0||g.realm!=here.realm||!GateKnownTo(c.owner,g))continue;var s=StudyOf(c.owner,g);if(s!=null&&s.known)continue;if(openedGates!=null&&openedGates.Contains(GatePair(g)))continue;int d=Distance(c.x,c.y,g.x,g.y);if(d<=DimensionSettings.InvestigateRange&&d<least){least=d;best=g;}}return best;}
 public Tile GateToRepair(City c){Tile best=null;int least=int.MaxValue;var here=At(c.x,c.y);foreach(var g in AllGates()){if(g.gateLink<=0||g.realm!=here.realm||!GateKnownTo(c.owner,g))continue;var st=FindState(g);if(st==null||st.condition!=GateCondition.Damaged||st.repair<=0)continue;var s=StudyOf(c.owner,g);if(s==null||!s.known)continue;int d=Distance(c.x,c.y,g.x,g.y);if(d<=DimensionSettings.InvestigateRange&&d<least){least=d;best=g;}}return best;}
 // A rival weighs a gateway against its home: not in its first years, not at war, not while poor or small.
 public bool GateInvestWanted(int owner)
 {
  if(owner==0)return true;if(IsNativeNation(owner))return false;
  if(turn<30&&Era(owner)==0)return false;if(cities.Count(c=>c.owner==owner)<2||nations[owner].gold<15)return false;
  for(int o=0;o<civCount;o++)if(o!=owner&&AtWar(owner,o))return false;return true;
 }
 public string GateProjectReason(City c,int b)
 {
  if(!HasRealms)return "This world has no gateways.";
  if(b==InvestigateProject){if(GateToInvestigate(c)==null)return "No unread gateway your people know of lies within "+DimensionSettings.InvestigateRange+" tiles.";}
  else{if(GateToRepair(c)==null)return "No damaged gateway whose seal you have read lies within "+DimensionSettings.InvestigateRange+" tiles.";if(!HasTech(c.owner,"bronze"))return "Requires Bronze Working: its works are metal.";}
  if(cities.Any(o=>o!=c&&o.owner==c.owner&&o.buildingProject==b&&(b==InvestigateProject?GateToInvestigate(o)==GateToInvestigate(c):GateToRepair(o)==GateToRepair(c))))return "Another of your cities is already doing this.";
  return GateInvestWanted(c.owner)?"":"Home comes first.";
 }
 // Knowledge makes the work lighter: the better the method, the less production it takes.
 public int GateProjectCost(City c,int b,int cost){if(!IsGateProject(b)||c==null)return cost;return Math.Max(10,cost*GateMethod.CostPercent[InvestigationMethod(c.owner)]/100);}
 void CompleteGateProject(City c,int b)
 {
  if(b==InvestigateProject)
  {
   var gate=GateToInvestigate(c);if(gate==null)return;var s=StudyFor(c.owner,HomeSide(gate));s.known=true;s.turns=GateApproach.StudyTurns;s.method=InvestigationMethod(c.owner);
   if(c.owner==0){Note(c.name+" has finished investigating the "+GateStyleNames[GateStyle(gate)].ToLower()+" by "+GateMethod.Names[s.method].ToLower()+". It leads to the "+RealmNames[GateRealm(gate)]+".");AddGateNotice(gate);}
  }
  else
  {
   var gate=GateToRepair(c);if(gate==null)return;var st=StateOf(gate);st.repair=0;st.condition=GateCondition.Dormant;
   if(c.owner==0){Note(c.name+" has repaired the "+GateStyleNames[GateStyle(gate)].ToLower()+". It is whole again, and dormant.");AddGateNotice(gate);}
   else if(KnowsNation(c.owner))Note(NationName(c.owner)+" has repaired a damaged gateway.");
  }
 }
 // ---------- what a civilisation knows about the far side ----------
 // Confirmed lines come from an investigation; without one the same things are listed as unknown, never as safe.
 public List<string> CompatibilityReport(int owner,Tile gate)
 {
  var lines=new List<string>();int realm=GateRealm(gate),family=DimensionFamily.OfRealm(realm);var s=StudyOf(owner,gate);bool read=s!=null&&s.known||RealmDiscovered(owner,realm)&&owner==0&&GateOpenFor(gate);
  if(!read){lines.Add("Unknown: what the far side does to people and equipment. Investigate before sending an army.");return lines;}
  lines.Add("Confirmed: "+DimensionFamily.Names[family]+(family==DimensionFamily.Home?" (your equipment works as at home).":" world. "+DimensionFamily.Rules[family]));
  if(realm==2||realm==4)lines.Add("Confirmed hazard: units lose 1 health a turn outside a city or colony"+(AttritionRelief(owner,realm)>0?" (your people have learned to live with it)":"")+".");
  if(realm==3)lines.Add("Confirmed hazard: units lose 2 health a turn outside a city or colony"+(AttritionRelief(owner,realm)>0?" (1 less for your people)":"")+".");
  if(realm==4)lines.Add("Confirmed hazard: units carry plague home for 5 turns.");
  for(int type=0;type<UnitNames.Length;type++)foreach(var need in CapabilityCatalog.OfUnit(type))
  {
   if(!DimensionFamily.Blocks(family,need.tag))continue;bool restored=TagRestored(owner,family,need.tag);
   string many=UnitNames[type].EndsWith("man")?UnitNames[type].Substring(0,UnitNames[type].Length-3)+"men":UnitNames[type]+"s";lines.Add(restored?"Adapted: "+many+" keep "+need.what+" there, thanks to what your people have learned.":"Confirmed: "+many+" lose "+need.what+" there (attack "+Attack[type]+" falls to "+CapabilityCatalog.FallbackAttack+"); they still march and defend.");
  }
  if(family!=DimensionFamily.Home)lines.Add("Knowledge is not affected: what your scholars know stays true there.");
  int method=s!=null?s.method:0;if(method>=3)lines.Add("Probes found "+ColonySites(owner,realm).Count+" good sites for a colony near the far gate.");
  return lines;
 }
 bool GateOpenFor(Tile gate){return GateOpen(gate);}
 // ---------- who may send through ----------
 // A gateway carries a civilisation's supplies and people when it is open, the land it stands on does not belong to someone who
 // refuses passage, and no foreign unit stands on either end of it. Capturing an end cuts the crossing; it changes nothing beyond.
 public string GateBlock(int owner,Tile gate)
 {
  if(gate==null||gate.gateLink<=0)return "Not a gateway.";if(!GateOpen(gate))return ConditionOf(gate)==GateCondition.Cycling?"The gateway is in the shut part of its cycle.":"The gateway is shut.";
  foreach(var side in new[]{gate,GateTwin(gate)})
  {
   if(side==null)continue;
   if(side.owner>=0&&side.owner!=owner&&side.owner<civCount){if(AtWar(owner,side.owner))return NationName(side.owner)+" holds the gateway and is at war with "+(owner==0?"you":NationName(owner))+".";if(!HasAccess(owner,side.owner))return NationName(side.owner)+" holds the land the gateway stands on, and there is no open borders agreement.";}
   foreach(var u in units)if(u.x==side.x&&u.y==side.y&&u.owner!=owner&&u.owner<civCount&&(AtWar(owner,u.owner)||!HasAccess(owner,u.owner)))return "Units of "+NationName(u.owner)+" stand on the gateway.";
  }
  return "";
 }
 public bool GateUsable(int owner,Tile gate){return GateBlock(owner,gate)=="";}
 // Loads a turn for one civilisation: the gateway's own capacity, plus what that civilisation's knowledge adds.
 public int GateCapacityFor(int owner,Tile gate)
 {
  var s=FindState(gate);int cap=s==null?DimensionSettings.GateCapacity:s.capacity;
  if(HasTech(owner,"engineering"))cap+=2;if(HasTech(owner,"industrialisation"))cap+=2;if(HasTech(owner,"computers"))cap+=4;
  cap+=CapacityFromAdaptation(owner,DimensionFamily.OfRealm(GateRealm(gate)));if(SynthesisKnown(owner,1))cap+=3;return cap;
 }
}
}
