using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// One species to a world (owner request, 12 Nov 2026, with the owner's character artwork). Seven worlds lie beyond the gates and
// each belongs to one kind of people:
//   1 Fae Wilds: elves          2 Ashen Waste: orcs        3 Infernal Depths: demons     4 Plague Marshes: tree people
//   5 Drowned Deep: minotaurs   6 Hive Expanse: insects    7 Stonehold Deeps: dwarves (a new world)
// Ten civilisations of each of the five fantasy species, ten demon civilisations (one for each of the owner's ten demon designs)
// and fifteen new insect civilisations (so that each of the twenty insect designs has a people, with the five that existed).
// They follow the earlier peoples in the catalogue (84 on) and are written like them: see SealedPeoples.Add for the fields.
// A new game peoples each world from these; an older save keeps the peoples it was made with.
//
// art: which of the owner's pictures the people's units wear. "elf", "dwarf", "minotaur", "orc" and "treefolk" have four
// pictures each (warrior, settler, worker, scout: Resources/Visuals/Species/<art>-<role>); an "insect-..." or "demon-..." key
// is one picture worn by every other unit of that people (Resources/Visuals/Species/<art>).
// Every art key also has an archer, a mounted soldier and a siege engine with its crew (<art>-archer, <art>-cavalry, <art>-siege;
// owner's second pack, 12 Nov 2026): worn by the Archer, by the Horseman and Knight, and by the Catapult.
public static partial class SpeciesPeoples {
 public const int First=84;
 public const int Elves=1,Orcs=2,Demons=3,Treefolk=4,Minotaurs=5,Insects=6,Dwarves=7;
 public static readonly string[] SpeciesNames={"","Elves","Orcs","Demons","Tree people","Minotaurs","Insects","Dwarves"};
 public static readonly List<SealedPerson> People=new List<SealedPerson>();
 // Each world's history as its own species tells it, shown with its peoples in a game that uses them.
 public static readonly string[] WorldHistories=new string[8];
 static int adding;
 static SpeciesPeoples(){
  adding=Elves;AddElves();adding=Orcs;AddOrcs();adding=Demons;AddDemons();adding=Treefolk;AddTreefolk();adding=Minotaurs;AddMinotaurs();adding=Insects;AddInsects();adding=Dwarves;AddDwarves();
 }
 public static SealedPerson Of(int civ){int i=civ-First;return i>=0&&i<People.Count?People[i]:null;}
 // The same fields as SealedPeoples.Add, with the art key first.
 static void Add(string art,string name,string leader,string title,string greeting,int bust,int hue,int traitA,int traitB,int agenda,string[] cities,string epithet,string background,string history,string strengths,string weaknesses,
  string ability,PeopleRule abilityRule,string unit,int unitBase,int unitAttack,int unitDefence,int unitMoves,string building,int buildingBase,PeopleRule buildingRule,string weakness,PeopleRule weaknessRule){
  var p=SealedPeoples.Make(adding,name,leader,title,greeting,bust,hue,traitA,traitB,agenda,cities,epithet,background,history,strengths,weaknesses,ability,abilityRule,unit,unitBase,unitAttack,unitDefence,unitMoves,building,buildingBase,buildingRule,weakness,weaknessRule);
  p.Art=art;People.Add(p);
 }
 // The earlier peoples that are plainly one of these species wear the same pictures: the five insect peoples, the demons of the
 // Infernal Depths (the owner's earlier humanoid demon), and the first elves, dwarves and orcs.
 static readonly Dictionary<int,string> earlier=new Dictionary<int,string>{
  {79,"insect-red-wood-ant"},{80,"insect-honeybee"},{81,"insect-rhinoceros-beetle"},{82,"insect-praying-mantis"},{83,"insect-luna-moth"},
  {29,"elf"},{32,"dwarf"},{34,"orc"},{33,"demon-crimson-legion"},{55,"demon-crimson-legion"},{56,"demon-crimson-legion"},{57,"demon-crimson-legion"},{58,"demon-crimson-legion"},{59,"demon-crimson-legion"},{60,"demon-crimson-legion"},{61,"demon-crimson-legion"},{62,"demon-crimson-legion"}};
 // The art key of a civilisation's units, or null for the ordinary human pictures.
 public static string ArtOf(int civ){var p=Of(civ);if(p!=null)return p.Art;string key;return earlier.TryGetValue(civ,out key)?key:null;}
 // The picture file for one unit type of an art key.
 public static string ArtFile(string art,int unitType){if(art==null)return null;string role=unitType==4?"archer":unitType==5||unitType==8?"cavalry":unitType==9?"siege":null;if(role!=null)return art+"-"+role;if(art.StartsWith("insect-")||art.StartsWith("demon-"))return art;return art+"-"+(unitType==0?"settler":unitType==1?"worker":unitType==3?"scout":"warrior");}
 // The peoples a new game may place in a world, in the order it tries them. The species civilisations come first; the demon
 // world's earlier peoples and the Hive Expanse's first five follow or join them, and a seed turns the list so that a world with
 // more than ten candidates does not always show the same ten.
 public static List<int> Candidates(int realm,int seed)
 {
  var mine=new List<int>();for(int i=0;i<People.Count;i++)if(People[i].World==realm)mine.Add(First+i);
  if(realm==Insects){mine.InsertRange(0,new[]{79,80,81,82,83});int turn=((seed%mine.Count)+mine.Count)%mine.Count;mine=mine.Skip(turn).Concat(mine.Take(turn)).ToList();}
  else if(mine.Count>1){int turn=((seed%mine.Count)+mine.Count)%mine.Count;mine=mine.Skip(turn).Concat(mine.Take(turn)).ToList();}
  return mine;
 }
}
}
