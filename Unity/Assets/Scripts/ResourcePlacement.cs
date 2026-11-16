using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Placing the catalogue's resources (owner's resource pack, 16 Nov 2026; ResourceRegistry, Design/RESOURCES-2026-11-16.md).
// New worlds only: it runs after the original placement, fills only tiles that still have no resource, and uses its own
// random stream from the world seed, so everything else about world generation (and every older save) is unchanged.
// Each resource goes only on the terrains listed for it, weighted by rarity; coastal ones need water beside them; sea ones go
// on water beside land; dimensional ones only in the parallel worlds. A tile never holds more than one resource.
public partial class World {
 public const double CatalogueLandShare=.055,CatalogueSeaShare=.02,CatalogueRealmShare=.05;
 // Candidates by terrain, built once from the registry.
 static Dictionary<int,List<ResourceRegistry.Def>> landPicks,seaPicks;static List<ResourceRegistry.Def> realmPicks;
 static void BuildPicks(){
  if(landPicks!=null)return;landPicks=new Dictionary<int,List<ResourceRegistry.Def>>();seaPicks=new Dictionary<int,List<ResourceRegistry.Def>>();realmPicks=new List<ResourceRegistry.Def>();
  foreach(var d in ResourceRegistry.All){if(!d.Spawns)continue;
   if(d.place==ResourceRegistry.Place.Realm){realmPicks.Add(d);continue;}
   var table=d.place==ResourceRegistry.Place.Sea?seaPicks:landPicks;
   foreach(int t in d.terrains){List<ResourceRegistry.Def> list;if(!table.TryGetValue(t,out list)){list=new List<ResourceRegistry.Def>();table[t]=list;}list.Add(d);}}
 }
 static ResourceRegistry.Def Weighted(List<ResourceRegistry.Def> options,Random r){
  int total=0;foreach(var d in options)total+=d.weight;if(total<=0)return null;int roll=r.Next(total);
  foreach(var d in options){roll-=d.weight;if(roll<0)return d;}return options[options.Count-1];
 }
 bool BesideWater(Tile t){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n!=null&&n!=t&&IsWater(n.terrain))return true;}return false;}
 bool BesideLand(Tile t){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n!=null&&n!=t&&!IsWater(n.terrain))return true;}return false;}
 // The home world's land and seas.
 public void PlaceCatalogueResources(){if(Environment.GetCommandLineArgs().Contains("-noCatalogue"))return;
  BuildPicks();var r=new Random(unchecked(seed*31+0x5EED));
  foreach(var t in tiles){
   if(t.realm!=0||t.resource!=0||t.gateLink>0)continue;
   if(IsWater(t.terrain)){
    if(r.NextDouble()>=CatalogueSeaShare||!BesideLand(t))continue;List<ResourceRegistry.Def> sea;if(!seaPicks.TryGetValue(t.terrain,out sea))continue;
    var pick=Weighted(sea,r);if(pick!=null)t.resource=pick.number;continue;}
   if(Barren(t.terrain)||IsImpassable(t.terrain))continue;
   if(r.NextDouble()>=CatalogueLandShare)continue;List<ResourceRegistry.Def> land;if(!landPicks.TryGetValue(t.terrain,out land))continue;
   bool coast=BesideWater(t);var fit=land.Where(d=>d.place!=ResourceRegistry.Place.Coast||coast).ToList();if(fit.Count==0)continue;
   var chosen=Weighted(fit,r);if(chosen!=null)t.resource=chosen.number;
  }
 }
 // A parallel world's land: dimensional resources, a late-game prize behind the gateways.
 void PlaceRealmResources(int realm){if(Environment.GetCommandLineArgs().Contains("-noCatalogue"))return;
  BuildPicks();if(realmPicks.Count==0)return;var r=new Random(unchecked(seed*131+realm*7919+0xD1));
  foreach(var t in tiles){if(t.realm!=realm||t.resource!=0||t.gateLink>0||IsWater(t.terrain)||Barren(t.terrain)||IsImpassable(t.terrain))continue;
   if(r.NextDouble()>=CatalogueRealmShare)continue;var pick=Weighted(realmPicks,r);if(pick!=null)t.resource=pick.number;}
 }
}
}
