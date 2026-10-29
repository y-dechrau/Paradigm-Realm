using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// 16-bit plan Stage 5 (CORE-DESIGN-16BIT.md): 36 keystone technologies, 6 eras of 6. Every one of the 124 earlier technologies
// (LegacyTechnologyCatalog) is folded into exactly one keystone, so Index("chemistry") finds Chemistry and Index("steam")
// finds Industry; existing unlocks and effects therefore keep working. Knowing 4 of an era's 6 keystones advances the era
// (see Edicts.cs). Cost = 20 x era^2 + 5 x era x position, era and position counted from 1.
public static class TechnologyCatalog {
public const int Count=36,EraCount=6,PerEra=6,Start=0;
public static readonly string[] EraNames={"Dawn Age","Classical Age","Medieval Age","Industrial Age","Modern Age","Dimensional Age"};
// id (the earlier technology it is named after), name, earlier technologies folded in, prerequisites (keystone ids).
static readonly string[][] Table={
 new[]{"toolmaking","Stone Tools","fire hunting foraging cordage tailoring cave_art rafting",""},
 new[]{"agriculture","Agriculture","husbandry pottery","toolmaking"},
 new[]{"archery","Archery","","toolmaking"},
 new[]{"bronze","Bronze Working","mining masonry","toolmaking"},
 new[]{"wheel","The Wheel","horseback","agriculture"},
 new[]{"writing","Writing","alphabet","agriculture"},
 new[]{"currency","Currency","","bronze writing"},
 new[]{"laws","Code of Laws","","writing"},
 new[]{"maths","Mathematics","iron construction irrigation","writing bronze"},
 new[]{"mysticism","Mysticism","calendar astronomy","writing"},
 new[]{"sailing","Sailing","shipbuilding","wheel"},
 new[]{"philosophy","Philosophy","drama","laws mysticism"},
 new[]{"engineering","Engineering","architecture","maths wheel"},
 new[]{"feudalism","Feudalism","chivalry steel","laws maths"},
 new[]{"education","Education","theology alchemy","philosophy"},
 new[]{"banking","Banking","guilds economics compass","currency laws"},
 new[]{"printing","Printing Press","invention physics navigation","education"},
 new[]{"military_science","Military Science","gunpowder metallurgy machinery","feudalism engineering"},
 new[]{"industrialisation","Industry","steam railways mass_production corporations","engineering banking"},
 new[]{"electricity","Electricity","radio","printing industrialisation"},
 new[]{"rifling","Rifling","ballistics sci_method","military_science"},
 new[]{"chemistry","Chemistry","refining combustion plastics archaeology","education engineering"},
 new[]{"medicine","Medicine","biology antibiotics","education"},
 new[]{"flight","Flight","radar combined_arms","industrialisation electricity"},
 new[]{"computers","Computing","microchips electronics telecoms internet ai","electricity"},
 new[]{"satellites","Satellites","rocketry reusable space_station","flight computers"},
 new[]{"fission","Nuclear Fission","atomic_theory","chemistry electricity"},
 new[]{"robotics","Robotics","lasers stealth","computers industrialisation"},
 new[]{"genetics","Genetic Engineering","renewables climate","medicine chemistry"},
 new[]{"globalisation","Globalisation","","computers banking"},
 new[]{"nanotech","Nanotechnology","materials replicators","robotics"},
 new[]{"quantum","Quantum Computing","quantum_net neural","computers fission"},
 new[]{"longevity","Longevity Medicine","arcologies suspended","genetics"},
 new[]{"fusion","Fusion Power","plasma fusion_drive antimatter antimatter_drive solar_array orbital_industry asteroid_mining space_elevator lunar_base mars mars_colony terraforming outer_colonies dyson stellar matrioshka","fission satellites"},
 new[]{"uploading","Mind Uploading","","quantum longevity"},
 new[]{"transcendence","Transcendence","galactic spacetime exoplanets generation_ship","uploading fusion nanotech"},
};
public static readonly string[] Ids=new string[Count],Names=new string[Count],Descriptions=new string[Count];
public static readonly int[] Eras=new int[Count],Costs=new int[Count];
public static readonly int[][] Requirements=new int[Count][];
public static readonly string[][] Unlocks=new string[Count][];
// Folded[i] = the keystone's own id followed by the earlier technology ids folded into it.
public static readonly string[][] Folded=new string[Count][];
// LegacyMap[i] = the keystone that earlier technology i was folded into.
public static readonly int[] LegacyMap=new int[LegacyTechnologyCatalog.Count];
static readonly Dictionary<string,int> indexMap=new Dictionary<string,int>();
static TechnologyCatalog(){
 for(int i=0;i<Count;i++){var row=Table[i];Ids[i]=row[0];Names[i]=row[1];indexMap[row[0]]=i;}
 for(int i=0;i<LegacyMap.Length;i++)LegacyMap[i]=-1;
 for(int i=0;i<Count;i++){
  var row=Table[i];Eras[i]=i/PerEra;int e=Eras[i]+1,pos=i%PerEra+1;Costs[i]=20*e*e+5*e*pos;
  var folded=new[]{row[0]}.Concat(row[2].Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries)).ToArray();
  Folded[i]=folded;var unlocks=new List<string>();
  foreach(var id in folded){int legacy=LegacyTechnologyCatalog.Index(id);if(legacy<0)continue;LegacyMap[legacy]=i;indexMap[id]=i;unlocks.AddRange(LegacyTechnologyCatalog.Unlocks[legacy]);}
  int primary=LegacyTechnologyCatalog.Index(row[0]);
  Descriptions[i]=(primary>=0?LegacyTechnologyCatalog.Descriptions[primary]:"")+(folded.Length>1?" Includes "+string.Join(", ",folded.Skip(1).Select(id=>{int l=LegacyTechnologyCatalog.Index(id);return l<0?id:LegacyTechnologyCatalog.Names[l];}).ToArray())+".":"");
  Unlocks[i]=unlocks.ToArray();
  Requirements[i]=row[3].Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries).Select(id=>indexMap[id]).ToArray();
 }
}
// Keystone index for a keystone id or for any earlier technology id folded into it; -1 if unknown.
public static int Index(string id){int found;return id!=null&&indexMap.TryGetValue(id,out found)?found:-1;}
public static bool[] Starting(){var t=new bool[Count];t[Start]=true;return t;}
} }
