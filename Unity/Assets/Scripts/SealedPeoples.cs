using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// The peoples of the sealed worlds (owner request, 5 Nov 2026): ten to a world, the two first written (CivLore entries 29-38)
// and eight more each (catalogue 39-78, eight in a row per world, in world order). Inspired by biblical and apocryphal
// writings, other religions and myths, science fiction and fantasy, but all invented: no figure sacred to a living faith
// is depicted, and no name was knowingly taken from anyone's fiction (names not legally cleared). Their stories and rule numbers are in SealedPeoples<World>.cs.
// A rule is a few small integers; the text the player reads is written from the numbers, so the two cannot disagree.
public sealed class PeopleRule {
 // In every city.
 public int Food,Production,Science,Gold,Content,Faith;
 // For each unit in combat, attacking and defending, and how far it sees.
 public int Attack,Defence,Sight;
 // On each worked tile of these terrains (World.TerrainNames).
 public int[] Terrains=new int[0];public int TileFood,TileProduction,TileScience,TileGold;
 static string Signed(int v,string what){return (v>0?"+":"")+v+" "+what;}
 public string Describe(){
  var cityParts=new List<string>();
  if(Food!=0)cityParts.Add(Signed(Food,"food"));if(Production!=0)cityParts.Add(Signed(Production,"production"));if(Science!=0)cityParts.Add(Signed(Science,"science"));if(Gold!=0)cityParts.Add(Signed(Gold,"gold"));if(Content!=0)cityParts.Add(Signed(Content,"content citizen"));if(Faith!=0)cityParts.Add(Signed(Faith,"faith"));
  var parts=new List<string>();if(cityParts.Count>0)parts.Add(string.Join(", ",cityParts.ToArray())+" in every city");
  if(Attack!=0)parts.Add(Signed(Attack,"attack")+" for all units");if(Defence!=0)parts.Add(Signed(Defence,"defence")+" for all units");if(Sight!=0)parts.Add("all units see "+Math.Abs(Sight)+(Sight>0?" tile further":" tile less"));
  var tileParts=new List<string>();if(TileFood!=0)tileParts.Add(Signed(TileFood,"food"));if(TileProduction!=0)tileParts.Add(Signed(TileProduction,"production"));if(TileScience!=0)tileParts.Add(Signed(TileScience,"science"));if(TileGold!=0)tileParts.Add(Signed(TileGold,"gold"));
  if(tileParts.Count>0)parts.Add(string.Join(", ",tileParts.ToArray())+" on each worked "+string.Join(" or ",Terrains.Select(t=>World.TerrainNames[t]).ToArray())+" tile");
  return string.Join("; ",parts.ToArray())+".";
 }
 // The building's bonus reads as "+2 science as well", without "in every city".
 public string DescribeBuilding(){return Describe().Replace(" in every city","").TrimEnd('.')+" as well.";}
}
public sealed class SealedPerson {
 public string Name,Leader,LeaderTitle,Greeting;public int World,Bust,Hue;public int[] Traits;public string[] Cities;public CivEntry Lore;
}
public static partial class SealedPeoples {
 public const int First=39,PerWorld=8,Count=45;
 // The Hive Expanse (world 6, 9 Nov 2026) has five peoples, catalogue 79-83, after the forty of the first five worlds.
 public const int HivePeoples=5;
 // Each world's own history, shown with its peoples.
 public static readonly string[] WorldHistories=new string[7];
 public static readonly List<SealedPerson> People=new List<SealedPerson>();
 static int adding;
 static SealedPeoples(){
  adding=1;AddFaeWilds();adding=2;AddAshenWaste();adding=3;AddInfernalDepths();adding=4;AddPlagueMarshes();adding=5;AddDrownedDeep();adding=6;AddHiveExpanse();
 }
 public static SealedPerson Of(int civ){int i=civ-First;return i>=0&&i<People.Count?People[i]:null;}
 // bust: the portrait's shape. 0 elf, 1 avian, 2 machine, 3 dwarf, 4 demon, 5 orc, 6 ratkin, 7 mushroom folk, 8 merfolk, 9 lizardfolk,
 // 10 robed human, 11 skeleton, 12 masked spirit, 13 giant, 14 radiant winged. hue: 0-7, colours its robes.
 // unitBase: World.UnitNames index; buildingBase: World.BuildingIds value.
 static void Add(string name,string leader,string title,string greeting,int bust,int hue,int traitA,int traitB,int agenda,string[] cities,string epithet,string background,string history,string strengths,string weaknesses,
  string ability,PeopleRule abilityRule,string unit,int unitBase,int unitAttack,int unitDefence,int unitMoves,string building,int buildingBase,PeopleRule buildingRule,string weakness,PeopleRule weaknessRule){
  string unitParts=string.Join(", ",new[]{unitAttack!=0?"+"+unitAttack+" attack":null,unitDefence!=0?"+"+unitDefence+" defence":null,unitMoves!=0?"+"+unitMoves+" movement":null}.Where(s=>s!=null).ToArray());
  People.Add(new SealedPerson{Name=name,Leader=leader,LeaderTitle=title,Greeting=greeting,World=adding,Bust=bust,Hue=hue,Traits=new[]{traitA,traitB},Cities=cities,Lore=new CivEntry{
   Epithet=epithet,Background=background,History=history,Strengths=strengths,Weaknesses=weaknesses,
   Ability=ability,AbilityText=abilityRule.Describe(),AbilityRule=abilityRule,
   Unit=unit,UnitBase=unitBase,UnitAttack=unitAttack,UnitDefence=unitDefence,UnitMoves=unitMoves,UnitText="Replaces the "+World.UnitNames[unitBase]+": "+unitParts+".",
   Building=building,BuildingBase=buildingBase,BuildingText="Replaces the "+World.BuildingNames[Array.IndexOf(World.BuildingIds,buildingBase)]+": "+buildingRule.DescribeBuilding(),BuildingRule=buildingRule,
   Weakness=weakness,WeaknessText=weaknessRule.Describe(),WeaknessRule=weaknessRule,Agenda=agenda}});
 }
}
public partial class World {
 // The rule numbers of the peoples written in SealedPeoples: their ability and weakness apply everywhere, the building's bonus where it stands.
 void ApplyPeopleYields(City c,CivEntry e,List<Tile> worked,ref int f,ref int p,ref int s,ref int g){
  foreach(var rule in new[]{e.AbilityRule,e.WeaknessRule,UniqueBuilding(c,e.BuildingBase)?e.BuildingRule:null}){
   if(rule==null)continue;f+=rule.Food;p+=rule.Production;s+=rule.Science;g+=rule.Gold;
   if(rule.Terrains.Length>0){int on=worked.Count(t=>rule.Terrains.Contains(t.terrain));f+=on*rule.TileFood;p+=on*rule.TileProduction;s+=on*rule.TileScience;g+=on*rule.TileGold;}
  }
  p=Math.Max(1,p);s=Math.Max(0,s);
 }
 int PeopleContent(City c,CivEntry e){return (e.AbilityRule!=null?e.AbilityRule.Content:0)+(e.WeaknessRule!=null?e.WeaknessRule.Content:0)+(e.BuildingRule!=null&&UniqueBuilding(c,e.BuildingBase)?e.BuildingRule.Content:0);}
 int PeopleFaith(City c,CivEntry e){return Math.Max(0,(e.AbilityRule!=null?e.AbilityRule.Faith:0)+(e.WeaknessRule!=null?e.WeaknessRule.Faith:0)+(e.BuildingRule!=null&&UniqueBuilding(c,e.BuildingBase)?e.BuildingRule.Faith:0));}
 int PeopleStrength(CivEntry e,bool defending){int s=0;foreach(var rule in new[]{e.AbilityRule,e.WeaknessRule})if(rule!=null)s+=defending?rule.Defence:rule.Attack;return s;}
 int PeopleSight(CivEntry e){int s=0;foreach(var rule in new[]{e.AbilityRule,e.WeaknessRule})if(rule!=null)s+=rule.Sight;return s;}
}
}
