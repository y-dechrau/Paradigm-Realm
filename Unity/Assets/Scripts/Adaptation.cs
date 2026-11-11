using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Three layers of knowledge (plan stage 5, Design/DIMENSIONS-PLAN-2026-11-11.md).
//  1. Universal knowledge: the main research tree, untouched. It is true everywhere.
//  2. Local adaptation: what a civilisation works out about living in one FAMILY of worlds. It comes slowly from being there
//     (colonies, units on the ground), faster from colony facilities, and faster again with money from home. Related universal
//     knowledge makes each discovery cheaper. Worlds of one family share it. A native people already has its own family's.
//  3. Cross-dimensional synthesis: discoveries that need local knowledge from BOTH families and are paid for with the science
//     of the home cities, half of each turn's science while one is being studied, so they compete with ordinary research.
// Every discovery does something; AdaptationCatalog.Effect names it and the queries at the bottom of this file apply it.
public class Discovery {
 public string name,text,effect;public int cost,need=-1,need2=-1;public string[] related;
 public Discovery(string name,int cost,int need,string[] related,string effect,string text,int need2=-1){this.name=name;this.cost=cost;this.need=need;this.need2=need2;this.related=related;this.effect=effect;this.text=text;}
}
public static class AdaptationCatalog {
 public const int Synthesis=3;
 // Ember: fire will not sustain itself; heat and pressure come from the ground.
 public const int BankedEmbers=0,VentReading=1,ThermalForging=2,PressureArms=3,Ashcloth=4,GeothermalKilns=5,PressureHaulage=6,DeepTapping=7,FlamelessFurnace=8;
 // Resonant: current will not hold; certain crystals ring in sympathy.
 public const int ListeningStones=0,SympatheticChimes=1,CrystalLathe=2,TunedMail=3,HarmonicFarming=4,ResonanceRelay=5,SingingBlades=6,CrystalEngines=7,FarSpeakingLattice=8;
 public const int FlamelessFoundry=0,GateHarmonics=1;
 static readonly string[] none=new string[0];
 public static readonly Discovery[][] ByFamily={
  new Discovery[0],
  new[]{
   new Discovery("Banked Embers",8,-1,new[]{"toolmaking"},"Your units lose 1 less health a turn in Ember worlds.","Fires die here, so travellers carry heat instead: stones warmed at the vents, wrapped and buried each night."),
   new Discovery("Vent Reading",10,0,new[]{"agriculture"},"Ember colonies grow 2 points more self-sufficient each turn.","Where the steam comes up, things grow. Learning the vents' moods tells a colony where to plant and when to move."),
   new Discovery("Thermal Forging",16,0,new[]{"bronze"},"Ember colonies may build a Thermal Forge.","Metal worked on the hot rock itself, hammered where the ground glows, with no fire needed."),
   new Discovery("Pressure Arms",22,2,new[]{"military_science","engineering"},"Riflemen keep their firearms in Ember worlds.","Shot driven by trapped steam instead of powder. Slower to load, and as deadly."),
   new Discovery("Ashcloth",12,0,new[]{"toolmaking"},"Ember colonies defend with +2 strength.","Cloth felted from the ash itself, which neither heat nor falling cinders mark."),
   new Discovery("Geothermal Kilns",14,1,new[]{"bronze","maths"},"Ember colonies need 1 less supply a turn.","Pots, bricks and bread baked in the ground. The colony stops asking home for fuel it cannot burn anyway."),
   new Discovery("Pressure Haulage",20,2,new[]{"wheel","engineering"},"Gateways to Ember worlds carry 2 more loads a turn for you.","Sledges driven by vented steam bring loads to the arch faster than any team of animals."),
   new Discovery("Deep Tapping",24,2,new[]{"engineering","industrialisation"},"Extraction colonies in Ember worlds export 1 more load.","Shafts sunk to where the metal runs soft, drawn up by its own pressure."),
   new Discovery("Flameless Furnace",30,3,new[]{"industrialisation","chemistry"},"A home city supplied from an Ember world gains 1 more production.","Stone that holds Ember heat for a season, carried home through the gate and set under the workshops.",2),
  },
  new[]{
   new Discovery("Listening Stones",8,-1,new[]{"mysticism"},"Resonant colonies see 4 tiles round them instead of 2.","Some crystals hum when anything large moves nearby. Sentries learn each stone's voice."),
   new Discovery("Sympathetic Chimes",12,0,new[]{"writing"},"Resonant colonies need 1 less supply a turn.","Two crystals cut from one seed ring together however far apart. A colony can ask for exactly what it lacks."),
   new Discovery("Crystal Lathe",16,0,new[]{"bronze","maths"},"Resonant colonies may build a Crystal Workshop.","Cutting a crystal so that it rings one pure note is a craft learned by ear, a hundred broken stones at a time."),
   new Discovery("Tuned Mail",12,0,new[]{"bronze"},"Resonant colonies defend with +2 strength.","Armour hung with small crystals that shiver before a blow lands."),
   new Discovery("Harmonic Farming",10,0,new[]{"agriculture"},"Resonant colonies grow 2 points more self-sufficient each turn.","Crops here lean toward certain notes. Fields are planted round a ringing stone."),
   new Discovery("Resonance Relay",20,1,new[]{"maths","engineering"},"Gateways to Resonant worlds carry 2 more loads a turn for you.","A chain of chimes from colony to arch: every load is ready the moment the way is clear."),
   new Discovery("Singing Blades",18,2,new[]{"bronze","feudalism"},"Your units have +1 attack in Resonant worlds.","Crystal edges that ring in the hand and cut what steel only dents."),
   new Discovery("Crystal Engines",24,2,new[]{"engineering","industrialisation"},"Trade colonies in Resonant worlds export 1 more load.","Wheels turned by a struck crystal's long note. They do the work a current cannot do here."),
   new Discovery("Far-Speaking Lattice",30,5,new[]{"printing","electricity"},"A home city supplied from a Resonant world gains 1 more science.","Paired crystals carried home ring across the gate. Its scholars talk to the colony's every day.",1),
  },
  new[]{
   new Discovery("Flameless Foundry",60,AdaptationCatalog.ThermalForging,new[]{"engineering"},"Every home city with a Workshop gains 1 production.","Ember heat-stone rung to an even warmth by a Resonant crystal: a forge that needs no fuel and never cools.",AdaptationCatalog.CrystalLathe),
   new Discovery("Gate Harmonics",90,AdaptationCatalog.PressureHaulage,new[]{"maths"},"Every gateway carries 3 more loads a turn for you, and cycling gateways stay open 2 turns longer.","The arches answer both pressure and tone. Understood together, they can be held open and widened.",AdaptationCatalog.ResonanceRelay),
  },
 };
 public static Discovery Of(int family,int index){if(family<0||family>=ByFamily.Length||index<0||index>=ByFamily[family].Length)return null;return ByFamily[family][index];}
 public static readonly string[] FamilyTitles={"","Ember adaptation","Resonant adaptation","Cross-dimensional synthesis"};
 public const int FundingStep=2,MaxFunding=3;   // gold a turn per funding level, each level one more point a turn
}
public partial class World {
 public Adaptation AdaptationOf(int owner,int family,bool create=false)
 {
  if(adaptations==null)adaptations=new List<Adaptation>();foreach(var a in adaptations)if(a.owner==owner&&a.family==family)return a;
  if(!create)return null;var made=new Adaptation{owner=owner,family=family};adaptations.Add(made);return made;
 }
 // A native people has always lived by its own world's rules.
 bool NativeTo(int owner,int family){if(!IsNativeNation(owner))return false;return family!=DimensionFamily.Home&&DimensionFamily.OfRealm(CivilisationCatalog.NativeRealm(nations[owner].civ))==family;}
 public bool Knows(int owner,int family,int index){if(owner<0||owner>=civCount)return false;if(family!=AdaptationCatalog.Synthesis&&NativeTo(owner,family))return true;var a=AdaptationOf(owner,family);return a!=null&&a.known.Contains(index);}
 public bool SynthesisKnown(int owner,int index){return Knows(owner,AdaptationCatalog.Synthesis,index);}
 public bool SynthesisKnownByAny(int index){if(adaptations==null)return false;foreach(var a in adaptations)if(a.family==AdaptationCatalog.Synthesis&&a.known.Contains(index))return true;return false;}
 // Related universal knowledge takes a tenth off for each technology known, to half at most.
 public int DiscoveryCost(int owner,int family,int index){var d=AdaptationCatalog.Of(family,index);if(d==null)return 0;int known=0;foreach(string id in d.related)if(HasTech(owner,id))known++;return Math.Max(d.cost/2,d.cost*(100-10*known)/100);}
 public string DiscoveryReason(int owner,int family,int index)
 {
  var d=AdaptationCatalog.Of(family,index);if(d==null)return "No such discovery.";if(Knows(owner,family,index))return "Already known.";
  if(family==AdaptationCatalog.Synthesis)
  {
   if(!Knows(owner,DimensionFamily.Ember,d.need))return "Needs "+AdaptationCatalog.Of(DimensionFamily.Ember,d.need).name+" (Ember).";
   if(!Knows(owner,DimensionFamily.Resonant,d.need2))return "Needs "+AdaptationCatalog.Of(DimensionFamily.Resonant,d.need2).name+" (Resonant).";
   foreach(string id in d.related)if(!HasTech(owner,id)){int t=TechnologyCatalog.Index(id);return "Needs "+(t>=0?TechNames[t]:id)+".";}
   return "";
  }
  if(d.need>=0&&!Knows(owner,family,d.need))return "Needs "+AdaptationCatalog.Of(family,d.need).name+".";
  if(d.need2>=0&&!Knows(owner,family,d.need2))return "Needs "+AdaptationCatalog.Of(family,d.need2).name+".";
  return "";
 }
 public bool StudyDiscovery(int owner,int family,int index){if(index>=0&&DiscoveryReason(owner,family,index)!="")return false;var a=AdaptationOf(owner,family,true);a.current=index;return true;}
 public bool SetAdaptationFunding(int owner,int family,int level){if(family<=0||family>=AdaptationCatalog.Synthesis)return false;AdaptationOf(owner,family,true).funding=Math.Max(0,Math.Min(AdaptationCatalog.MaxFunding,level));return true;}
 // Points a turn toward a family's discoveries: presence, facilities and funding. Nothing without someone there.
 public int AdaptationRate(int owner,int family)
 {
  if(family<=0||family>=AdaptationCatalog.Synthesis)return 0;int rate=0;bool present=false;
  if(colonies!=null)foreach(var c in colonies){if(c.owner!=owner||FamilyAt(At(c.x,c.y))!=family)continue;present=true;if(c.level<1)continue;rate+=1+(c.specialisation==ColonyCatalog.Research||c.second==ColonyCatalog.Research?2:0)+(c.buildings.Contains(ColonyCatalog.StudyHall)?1:0)+(c.buildings.Contains(ColonyCatalog.FamilyWorkshop)?1:0);}
  if(!present)foreach(var u in units)if(u.owner==owner&&FamilyAt(At(u.x,u.y))==family){present=true;break;}
  if(!present)return 0;var a=AdaptationOf(owner,family);int funding=a==null?0:a.funding;
  return Math.Max(1,rate)+funding;
 }
 public int AdaptationFundingCost(int owner){int g=0;if(adaptations!=null)foreach(var a in adaptations)if(a.owner==owner&&a.family<AdaptationCatalog.Synthesis&&AdaptationRate(owner,a.family)>0)g+=a.funding*AdaptationCatalog.FundingStep;return g;}
 void AdaptationTurn(int owner)
 {
  if(IsNativeNation(owner))return;
  for(int family=1;family<AdaptationCatalog.Synthesis;family++)
  {
   int rate=AdaptationRate(owner,family);if(rate<=0)continue;var a=AdaptationOf(owner,family,true);
   int cost=a.funding*AdaptationCatalog.FundingStep;if(cost>0){if(nations[owner].gold>=cost){nations[owner].gold-=cost;if(owner==0)gold=nations[0].gold;}else{rate-=a.funding;a.funding=0;if(owner==0)Note("There is no gold left to fund "+AdaptationCatalog.FamilyTitles[family].ToLower()+"; the funding has stopped.");}}
   a.points+=rate;
   // A rival, or a player who has not chosen, takes the cheapest discovery open to it.
   if((a.current<0||Knows(owner,family,a.current))&&(owner!=0||a.current<0&&a.known.Count==0)){a.current=-1;int best=int.MaxValue;for(int i=0;i<AdaptationCatalog.ByFamily[family].Length;i++)if(DiscoveryReason(owner,family,i)==""&&DiscoveryCost(owner,family,i)<best){best=DiscoveryCost(owner,family,i);a.current=i;}}
   if(a.current>=0&&DiscoveryReason(owner,family,a.current)==""&&a.points>=DiscoveryCost(owner,family,a.current)){a.points-=DiscoveryCost(owner,family,a.current);Learn(owner,family,a.current);a.current=-1;}
  }
  // Synthesis: half of the home cities' science goes to it while one is being studied.
  var syn=AdaptationOf(owner,AdaptationCatalog.Synthesis);
  if(syn==null&&owner!=0){for(int i=0;i<AdaptationCatalog.ByFamily[AdaptationCatalog.Synthesis].Length;i++)if(DiscoveryReason(owner,AdaptationCatalog.Synthesis,i)==""){syn=AdaptationOf(owner,AdaptationCatalog.Synthesis,true);syn.current=i;break;}}
  if(syn!=null&&syn.current>=0)
  {
   if(DiscoveryReason(owner,AdaptationCatalog.Synthesis,syn.current)!=""){syn.current=-1;return;}
   int share=SynthesisShare(owner);var n=nations[owner];
   if(share>0&&n.research>=0&&n.research<n.progress.Length){int take=Math.Min(share,n.progress[n.research]);n.progress[n.research]-=take;syn.points+=take;if(owner==0)science=n.progress[n.research];}
   int cost=DiscoveryCost(owner,AdaptationCatalog.Synthesis,syn.current);
   if(syn.points>=cost){syn.points-=cost;Learn(owner,AdaptationCatalog.Synthesis,syn.current);syn.current=-1;}
  }
 }
 public int SynthesisShare(int owner){int s=0;foreach(var c in cities)if(c.owner==owner)s+=Math.Max(0,Yield(c)[2]);return s/2;}
 void Learn(int owner,int family,int index)
 {
  var a=AdaptationOf(owner,family,true);if(a.known.Contains(index))return;a.known.Add(index);var d=AdaptationCatalog.Of(family,index);
  if(owner==0){Note("Discovery: "+d.name+". "+d.effect);dimensionNews.Add("Discovered "+d.name);}
  else if(family==AdaptationCatalog.Synthesis&&KnowsNation(owner))Note(NationName(owner)+" has made a cross-dimensional discovery: "+d.name+".");
 }
 // What a native people will teach: points toward its family's discoveries, for gold, when it thinks well of the buyer.
 public const int LocalLessonGold=30,LocalLessonPoints=8,LocalLessonOpinion=10;
 // ---------- effects ----------
 public int AttritionRelief(int owner,int realm){return DimensionFamily.OfRealm(realm)==DimensionFamily.Ember&&Knows(owner,DimensionFamily.Ember,AdaptationCatalog.BankedEmbers)?1:0;}
 public bool TagRestored(int owner,int family,string tag){return family==DimensionFamily.Ember&&tag==DimensionFamily.Combustion&&Knows(owner,DimensionFamily.Ember,AdaptationCatalog.PressureArms);}
 public int CapacityFromAdaptation(int owner,int family){return family==DimensionFamily.Ember&&Knows(owner,family,AdaptationCatalog.PressureHaulage)||family==DimensionFamily.Resonant&&Knows(owner,family,AdaptationCatalog.ResonanceRelay)?2:0;}
 // What local physics does to a unit's strength where it stands: a failed weapon falls back to hand arms, and local craft can add.
 public int DimensionStrength(Unit u,bool defending)
 {
  if(u==null)return 0;var t=At(u.x,u.y);int family=FamilyAt(t);if(family==DimensionFamily.Home)return 0;int change=0;
  foreach(var need in CapabilityCatalog.OfUnit(u.type))
  {
   if(!DimensionFamily.Blocks(family,need.tag)||TagRestored(u.owner,family,need.tag))continue;
   if(!defending&&(need.function==CapabilityNeed.Attack||need.function==CapabilityNeed.Everything))change-=Math.Max(0,Attack[u.type]-CapabilityCatalog.FallbackAttack);
   if(defending&&(need.function==CapabilityNeed.Defence||need.function==CapabilityNeed.Everything))change-=Math.Max(0,Defence[u.type]-1);
  }
  if(!defending&&family==DimensionFamily.Resonant&&Attack[u.type]>0&&Knows(u.owner,family,AdaptationCatalog.SingingBlades))change++;
  return change;
 }
 // Why a unit is weaker (or stronger) here, for the tile card and the battle preview.
 public string DimensionNote(Unit u)
 {
  if(u==null)return "";var t=At(u.x,u.y);int family=FamilyAt(t);if(family==DimensionFamily.Home)return "";
  foreach(var need in CapabilityCatalog.OfUnit(u.type))if(DimensionFamily.Blocks(family,need.tag)&&!TagRestored(u.owner,family,need.tag))return UnitNames[u.type]+": "+need.what+" fail in "+DimensionFamily.Names[family]+" worlds (attack "+Attack[u.type]+" to "+CapabilityCatalog.FallbackAttack+").";
  if(family==DimensionFamily.Resonant&&Attack[u.type]>0&&Knows(u.owner,family,AdaptationCatalog.SingingBlades))return "Singing Blades: +1 attack here.";
  return "";
 }
}
}
