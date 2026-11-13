using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
 // Owner request (13 Nov 2026): resources play a pivotal part in a city's success. With modern rules (every new game):
 //  - a worked resource tile yields one more of its kind (food resources +1 food, luxuries +1 trade, iron/horses/oil +1 shield);
 //  - a developed resource (farm or mine on land; at sea a Harbour or a road-linked port) also gives the city a lasting effect:
 //    food resources +2 to the population limit, luxuries +1 content place, Iron +20% shields, Horses +10% shields,
 //    Offshore oil +25% shields (production bonuses capped at +40%);
 //  - variety: three different resources in use add 1 content place and 10% gold, five add 2 and 20%;
 //  - a city of size 6 or more that uses no resource at all has nothing to trade and loses 1 content place.
 // Nothing is stored: everything follows from the tiles, so saves are unchanged.
 public partial class World {
  public const int ResourceFoodKind=0,ResourceLuxuryKind=1,ResourceStrategicKind=2;
  // Index by resource id: None, Wheat, Iron, Gold, Horses, Spices, Fish, Whales, Pearls, Crabs, Kelp, Oysters, Seals, Tuna, Sponges, Offshore oil.
  public static readonly int[] ResourceKind={-1,0,2,1,2,1,0,1,1,0,0,1,0,0,1,2};
  public static readonly string[] ResourceKindNames={"food","luxury","strategic"};
  static readonly int[] StrategicPercent={0,0,20,0,10,0,0,0,0,0,0,0,0,0,0,25};
  public sealed class CityResourceReport {
   public List<Tile> inUse=new List<Tile>(),idle=new List<Tile>();
   public int growth,content,productionPercent,goldPercent,variety;
   public bool nothingToTrade;
  }
  static int KindOf(int r){return r>0&&r<ResourceKind.Length?ResourceKind[r]:-1;}
  // A resource is developed when its land is improved, or at sea once the city has a Harbour or a road-linked port.
  public bool ResourceDeveloped(City c,Tile t){
   if(t==null||t.resource<=0)return false;
   if(IsSeaResource(t.resource))return HasBuilding(c,43)||c.portLinked;
   return t.resource==2||t.resource==3?t.mine:t.farm;
  }
  // The extra yield a worked resource gives (added to EffectiveTileYield so the Governor favours resource tiles).
  void ResourceTileBonus(City c,Tile t,int[] v){
   if(!modernRules||t.resource<=0||!ResourceKnown(c.owner,t.resource))return;
   int k=KindOf(t.resource);if(k==ResourceFoodKind)v[0]++;else if(k==ResourceLuxuryKind)v[2]++;else if(k==ResourceStrategicKind)v[1]++;
  }
  public CityResourceReport CityResources(City c){
   var rep=new CityResourceReport();if(c==null||!modernRules)return rep;
   var worked=new HashSet<Tile>(Worked(c));var kinds=new HashSet<int>();int production=0;
   foreach(var t in Workable(c)){
    if(t.resource<=0||!ResourceKnown(c.owner,t.resource))continue;
    if(!worked.Contains(t)){rep.idle.Add(t);continue;}
    rep.inUse.Add(t);kinds.Add(t.resource);
    if(!ResourceDeveloped(c,t))continue;
    int k=KindOf(t.resource);
    if(k==ResourceFoodKind)rep.growth+=2;else if(k==ResourceLuxuryKind)rep.content++;else production+=StrategicPercent[t.resource];
   }
   rep.productionPercent=Math.Min(40,production);rep.variety=kinds.Count;
   if(kinds.Count>=5){rep.content+=2;rep.goldPercent=20;}else if(kinds.Count>=3){rep.content++;rep.goldPercent=10;}
   if(c.pop>=6&&kinds.Count==0){rep.nothingToTrade=true;rep.content--;}
   return rep;
  }
  public int ResourceGrowth(City c){return modernRules?CityResources(c).growth:0;}
  public int ResourceContent(City c){return modernRules?CityResources(c).content:0;}
  void ApplyResourceYields(City c,ref int p,ref int g){
   if(!modernRules)return;var rep=CityResources(c);p+=p*rep.productionPercent/100;g+=g*rep.goldPercent/100;
  }
  // One plain line per effect, for the city screen.
  public List<string> ResourceLines(City c){
   var lines=new List<string>();if(!modernRules)return lines;var rep=CityResources(c);
   foreach(var t in rep.inUse){
    int k=KindOf(t.resource);bool dev=ResourceDeveloped(c,t);string name=ResourceNames[t.resource];
    string worked=k==ResourceFoodKind?"+1 food":k==ResourceLuxuryKind?"+1 trade":"+1 shield";
    string lasting=k==ResourceFoodKind?"+2 population limit":k==ResourceLuxuryKind?"+1 content":"+"+StrategicPercent[t.resource]+"% shields";
    lines.Add(name+": "+worked+(dev?", "+lasting:" (develop it for "+lasting+(IsSeaResource(t.resource)?": build a Harbour or a linked port":"")+")"));
   }
   foreach(var t in rep.idle)lines.Add(ResourceNames[t.resource]+": in reach but not worked");
   if(rep.variety>=3)lines.Add(rep.variety+" different resources in use: +"+(rep.variety>=5?2:1)+" content, +"+rep.goldPercent+"% gold");
   else if(rep.inUse.Count>0)lines.Add("Use "+(3-rep.variety)+" more kind"+(3-rep.variety==1?"":"s")+" of resource for +1 content and +10% gold");
   if(rep.nothingToTrade)lines.Add("No resource in use: nothing to trade (-1 content)");
   if(lines.Count==0)lines.Add("No known resources in reach. Cities beside resources grow larger, happier and richer.");
   return lines;
  }
  // Settlement value of a resource in a prospective city's land (the adviser and rival settlers weigh these).
  public static int ResourceSiteValue(int r){int k=KindOf(r);return k==ResourceFoodKind?12:k==ResourceLuxuryKind?10:k==ResourceStrategicKind?(r==2?14:r==4?11:9):0;}
 }
}
