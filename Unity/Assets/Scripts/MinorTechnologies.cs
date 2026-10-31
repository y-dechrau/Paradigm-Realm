using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Owner request (31 Oct 2026): five times as many technologies. The 36 keystones keep indices 0-35 and their rules (an era
// completes at 4 of its 6 keystones); 144 minor technologies follow (indices 36-179), 24 per era. Each needs one keystone of
// its era (and sometimes another minor) and gives a small lasting effect:
//   F/P/T:terrains   +1 food / shields / trade on worked tiles of those terrains
//   R:res:F|P|T      +1 food / shields / trade on that resource
//   S:n G:n W:n      +n% science / gold / production in every city (each total capped at +40%)
//   H:1              +1 content citizen per city (capped at +3 in all)
//   A:class D:class  +1 attack / defence for melee, ranged, mounted, siege or naval units (each capped at +3)
// Format: id|Name|keystone (0-5 within the era)|minor prerequisite (index within the era's list, - for none)|effect
public static partial class TechnologyCatalog {
 public const int KeystoneCount=36,MinorPerEra=24;
 static readonly string[][] MinorTable={
  new[]{"fire|Fire Keeping|0|-|W:3","flint|Flint Knapping|0|-|P:2","hunting|Big Game Hunting|0|-|F:16|20","fishing|Shore Fishing|0|-|R:6:F","basketry|Basketry|1|-|F:0","ditches|Irrigation Ditches|1|4|F:7","herding|Herding|1|-|R:4:P","granaries|Grain Storage|1|4|F:20","fletching|Fletching|2|-|A:ranged","slings|Slings|2|-|D:ranged","copper|Copper Smelting|3|-|P:2","casting|Lost-Wax Casting|3|10|G:3","alloys|Tin Alloys|3|10|A:melee","quarrying|Quarrying|3|-|P:5|17","sledges|Sledges|4|-|W:3","carts|Ox Carts|4|14|T:0|20","tally|Tally Sticks|5|-|G:3","pictograms|Pictograms|5|-|S:3","lunar|Lunar Calendar|5|16|F:16","kilns|Pottery Kilns|1|-|W:3","weaving|Weaving|0|-|T:16","rafts|Reed Rafts|0|-|T:4|6","shamanism|Shamanism|0|-|H:1","drystone|Dry-Stone Walls|3|-|D:melee"},
  new[]{"coinage|Coinage|0|-|G:4","marketdays|Market Days|0|0|T:0","changers|Money Changers|0|0|G:3","census|Census|1|-|S:2","magistrates|Magistrates|1|-|H:1","paved|Paved Roads|1|-|W:3","geometry|Geometry|2|-|P:2","surveying|Surveying|2|6|F:7","arches|Arches|2|-|W:4","starcharts|Star Charts|3|-|S:3","oracles|Oracles|3|-|H:1","rites|Temple Rites|3|10|G:2","triremes|Trireme Design|4|-|A:naval","harbourworks|Harbour Works|4|-|F:4","celestial|Celestial Navigation|4|12|T:4","salting|Fish Salting|4|-|R:6:P","rhetoric|Rhetoric|5|-|S:3","logic|Logic|5|16|S:2","ethics|Ethics|5|-|H:1","phalanx|Phalanx Drill|1|-|D:melee","saddles|Saddles|2|-|A:mounted","olives|Olive Presses|0|-|F:16","terraces|Terraced Vineyards|2|-|F:2","ramps|Siege Ramps|2|-|A:siege"},
  new[]{"windmills|Windmills|0|-|P:0","watermills|Watermills|0|0|P:7","keeps|Castle Keeps|1|-|D:melee","plough|Heavy Plough|1|-|F:0|9","rotation|Crop Rotation|1|3|F:20","knighthood|Knighthood|1|-|A:mounted","scholastic|Scholasticism|2|-|S:4","monastic|Monastic Orders|2|-|H:1","manuscripts|Illuminated Manuscripts|2|6|S:2","guildhalls|Guild Halls|3|-|W:4","exchange|Bills of Exchange|3|-|G:4","fairs|Trade Fairs|3|10|T:7","movable|Movable Type|4|-|S:3","charts|Cartography|4|-|T:4","caravels|Caravels|4|13|A:naval","crossbows|Crossbows|5|-|A:ranged","pikes|Pike Squares|5|-|D:ranged","powdermills|Powder Mills|5|-|A:siege","forestry|Forestry|0|-|P:1|13","shafts|Mine Shafts|0|-|R:2:P","goldsmiths|Goldsmiths|3|-|R:3:T","apothecaries|Apothecaries|2|-|H:1","dykes|Dykes and Polders|0|-|F:9|19","spiceroutes|Spice Routes|3|-|R:5:T"},
  new[]{"steam|Steam Engines|0|-|W:5","railnet|Rail Networks|0|0|T:0|20","assembly|Assembly Lines|0|0|W:5","coal|Coal Mining|0|-|P:2","telegraph|Telegraph|1|-|G:3","lighting|Street Lighting|1|-|H:1","dynamos|Dynamos|1|5|W:4","breech|Breech Loaders|2|-|A:melee","trenches|Trench Works|2|-|D:melee","artillery|Field Artillery|2|-|A:siege","fertilisers|Fertilisers|3|-|F:0|16|20","dyes|Synthetic Dyes|3|-|G:3","refineries|Refineries|3|-|R:15:P","vaccination|Vaccination|4|-|H:1","sanitation|Sanitation|4|13|F:7","anaesthesia|Anaesthesia|4|-|S:2","aerial|Aerial Survey|5|-|S:3","airmail|Airmail|5|-|G:3","ironclads|Ironclads|2|-|A:naval","canning|Canning|3|-|R:13:F","bridges|Steel Bridges|0|-|T:7","mills|Cotton Mills|0|-|P:16","schools|Public Schools|1|-|S:4","stocks|Stock Exchange|1|-|G:4"},
  new[]{"microchips|Microprocessors|0|-|S:4","networks|Computer Networks|0|0|G:3","software|Software Engineering|0|0|W:4","gps|Satellite Navigation|1|-|T:4","weather|Weather Satellites|1|-|F:0","reactors|Reactor Design|2|-|W:5","isotopes|Medical Isotopes|2|-|H:1","automation|Factory Automation|3|-|W:5","drones|Drones|3|-|A:ranged","hybrids|Hybrid Crops|4|-|F:16|20","biotech|Biotechnology|4|-|H:1","aquaculture|Aquaculture|4|-|F:4","containers|Container Shipping|5|-|T:4","freetrade|Free Trade Zones|5|-|G:5","tourism|Tourism|5|-|G:3","composite|Composite Armour|3|-|D:melee","jets|Jet Engines|1|-|A:mounted","desalination|Desalination|2|-|F:3","offshore|Offshore Drilling|2|-|R:15:T","fibre|Fibre Optics|0|-|S:3","recycling|Recycling|3|-|P:10","vertical|Vertical Farms|4|-|F:8","stealth|Stealth Hulls|1|-|A:naval","cyber|Cyber Defence|0|-|D:ranged"},
  new[]{"nanofabs|Nanofabricators|0|-|W:6","smartmatter|Smart Matter|0|0|P:5|15","qcomms|Quantum Communication|1|-|S:5","qsensors|Quantum Sensors|1|-|D:ranged","regen|Regenerative Medicine|2|-|H:1","geneforge|Gene Forging|2|-|F:10|14","fusiongrid|Fusion Grid|3|-|W:6","orbitalfarms|Orbital Farms|3|-|F:0|3","plasma|Plasma Weapons|3|-|A:melee","mindnet|Mind Network|4|-|S:5","virtualstates|Virtual States|4|-|G:5","sharedmind|Shared Consciousness|4|10|H:1","starships|Starship Yards|3|-|A:naval","forcefields|Force Fields|0|-|D:melee","terraformers|Terraforming Crews|3|-|F:10|8","weathercontrol|Weather Control|1|-|F:16|20","antigravity|Antigravity|1|-|A:mounted","printers|Matter Printers|0|-|G:4","rifts|Dimensional Rifts|5|-|S:4","timecrystals|Time Crystals|5|-|W:5","harmony|Planetary Harmony|2|-|H:1","deepcore|Deep Core Mining|3|-|P:2|5","oceancities|Ocean Arcologies|2|-|F:4|12","singularity|Singularity Ethics|5|-|G:4"}};
 public static readonly string[] Effects=new string[Count];
 public static bool IsKeystone(int t){return t>=0&&t<KeystoneCount;}
 // Adds the minors after the keystones (called at the end of the static constructor).
 static void AddMinors(){
  string[] yields={"food","shields","trade"};
  for(int era=0;era<EraCount;era++)for(int k=0;k<MinorPerEra;k++){
   var f=MinorTable[era][k].Split('|');int i=KeystoneCount+era*MinorPerEra+k;
   Ids[i]="m_"+f[0];Names[i]=f[1];Eras[i]=era;indexMap[Ids[i]]=i;
   int e=era+1;Costs[i]=Math.Max(10,(20*e*e+5*e*3)*2/5);
   var req=new List<int>{era*PerEra+int.Parse(f[2])};if(f[3]!="-")req.Add(KeystoneCount+era*MinorPerEra+int.Parse(f[3]));Requirements[i]=req.ToArray();
   Effects[i]=string.Join("|",f.Skip(4).ToArray());Unlocks[i]=new string[0];Folded[i]=new[]{Ids[i]};
   Descriptions[i]=f[1]+": "+EffectText(Effects[i])+".";
  }
 }
 public static string EffectText(string effect){
  if(string.IsNullOrEmpty(effect))return "";var parts=effect.Split('|');var head=parts[0].Split(':');
  string Terrains(){return string.Join(", ",new[]{head[1]}.Concat(parts.Skip(1)).Select(s=>World.TerrainNames[int.Parse(s)]).ToArray());}
  switch(head[0]){
   case "F":return "+1 food on worked "+Terrains();case "P":return "+1 shields on worked "+Terrains();case "T":return "+1 trade on worked "+Terrains();
   case "R":return "+1 "+(head[2]=="F"?"food":head[2]=="P"?"shields":"trade")+" on "+World.ResourceNames[int.Parse(head[1])];
   case "S":return "+"+head[1]+"% science in every city";case "G":return "+"+head[1]+"% gold in every city";case "W":return "+"+head[1]+"% production in every city";
   case "H":return "+1 content citizen in every city";case "A":return "+1 attack for "+head[1]+" units";case "D":return "+1 defence for "+head[1]+" units";
  }
  return effect;
 }
}
// The combined effect of the minor technologies a civilisation knows, cached until it learns another.
public partial class World {
 [NonSerialized] Dictionary<int,MinorTotals> minorCache;
 class MinorTotals{public int known=-1;public int[,] terrain=new int[3,21];public int[,] resource=new int[3,16];public int science,gold,production,content;public Dictionary<string,int> attack=new Dictionary<string,int>(),defence=new Dictionary<string,int>();}
 MinorTotals Minors(int owner){
  if(minorCache==null)minorCache=new Dictionary<int,MinorTotals>();MinorTotals m;
  var tech=owner>=0&&owner<civCount?nations[owner].tech:null;int known=0;if(tech!=null)for(int i=TechnologyCatalog.KeystoneCount;i<tech.Length;i++)if(tech[i])known++;
  if(minorCache.TryGetValue(owner,out m)&&m.known==known)return m;
  m=new MinorTotals{known=known};minorCache[owner]=m;if(tech==null)return m;
  for(int i=TechnologyCatalog.KeystoneCount;i<tech.Length&&i<TechnologyCatalog.Count;i++){if(!tech[i])continue;var parts=TechnologyCatalog.Effects[i].Split('|');var head=parts[0].Split(':');
   switch(head[0]){
    case "F":case "P":case "T":{int y=head[0]=="F"?0:head[0]=="P"?1:2;foreach(var s in new[]{head[1]}.Concat(parts.Skip(1)))m.terrain[y,int.Parse(s)]++;break;}
    case "R":{int y=head[2]=="F"?0:head[2]=="P"?1:2;m.resource[y,int.Parse(head[1])]++;break;}
    case "S":m.science+=int.Parse(head[1]);break;case "G":m.gold+=int.Parse(head[1]);break;case "W":m.production+=int.Parse(head[1]);break;
    case "H":m.content++;break;
    case "A":{int v;m.attack.TryGetValue(head[1],out v);m.attack[head[1]]=v+1;break;}
    case "D":{int v;m.defence.TryGetValue(head[1],out v);m.defence[head[1]]=v+1;break;}
   }}
  m.science=Math.Min(40,m.science);m.gold=Math.Min(40,m.gold);m.production=Math.Min(40,m.production);m.content=Math.Min(3,m.content);
  foreach(var k in m.attack.Keys.ToList())m.attack[k]=Math.Min(3,m.attack[k]);foreach(var k in m.defence.Keys.ToList())m.defence[k]=Math.Min(3,m.defence[k]);
  return m;
 }
 // Extra food, shields and trade a worked tile gives its owner from minor technologies.
 public int MinorTileBonus(int owner,Tile t,int yield){if(owner<0||t==null)return 0;var m=Minors(owner);int b=t.terrain>=0&&t.terrain<21?m.terrain[yield,t.terrain]:0;if(t.resource>0&&t.resource<16&&ResourceKnown(owner,t.resource))b+=m.resource[yield,t.resource];return b;}
 public static string UnitClass(int type){return IsNaval(type)?"naval":IsRanged(type)?(type==9?"siege":"ranged"):Mounted(type)?"mounted":"melee";}
 public int MinorStrength(Unit u,bool defending){if(u==null||Power[u.type]==0&&!defending)return 0;var m=Minors(u.owner);int v;return (defending?m.defence:m.attack).TryGetValue(UnitClass(u.type),out v)?v:0;}
 public int MinorContent(int owner){return owner<0?0:Minors(owner).content;}
 public void MinorPercent(int owner,out int science,out int gold,out int production){var m=Minors(owner);science=m.science;gold=m.gold;production=m.production;}
}
}
