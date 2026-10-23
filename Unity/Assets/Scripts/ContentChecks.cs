using System;
using System.Linq;
using System.Collections.Generic;
namespace ParadigmRealm {
public static class ContentChecks {
static void Check(bool ok,string why){if(!ok)throw new Exception("Content: "+why);}
public static void Run(){
    var w=World.New(137);Check(w.AvailableTech().SequenceEqual(new[]{TechnologyCatalog.Index("toolmaking")}),"only Toolmaking starts available");
    var researched=new HashSet<int>();while(researched.Count<TechnologyCatalog.Count){var available=w.AvailableTech();Check(available.Count>0,"no cycle or unreachable research");foreach(int i in available){Check(World.Requirements[i].All(p=>researched.Contains(p)),"prerequisites already complete");researched.Add(i);w.tech[i]=true;}}
    Check(researched.Count==124&&TechnologyCatalog.EraNames.Length==14,"complete source tree imported");w.CheckVictory();Check(w.result.StartsWith("Science victory"),"Transcendence victory");
    var cold=World.New(3);var settler=cold.units.First(u=>u.owner==0&&u.type==0);cold.At(settler.x,settler.y).terrain=8;Check(cold.Found(settler)==null,"cold settlement needs Fire");cold.tech[TechnologyCatalog.Index("fire")]=true;Check(cold.Found(settler)!=null,"Fire permits cold settlement");
    var terrainTypes=new HashSet<int>();for(int seed=0;seed<6;seed++){var map=World.New(seed);Check(map.Valid(),"biome map validates");foreach(var tile in map.tiles){terrainTypes.Add(tile.terrain);Check(map.TileYield(tile).Length==3,"all terrain has yields");}}
    Check(terrainTypes.Count==21,"all texture terrain types generated");
    var movement=World.New(3);var unit=movement.units.First(u=>u.owner==0&&u.type==2);int x=unit.x+1,y=unit.y;
    foreach(int t in new[]{4,5,6,12,14,15}){movement.At(x,y).terrain=t;unit.moves=4;Check(!movement.Move(unit,x,y),"water/mountain/ice impassable");}
    movement.At(x,y).terrain=11;unit.moves=4;Check(movement.Move(unit,x,y)&&unit.moves==1,"jungle movement cost");
    var road=World.New(7);var worker=road.units.First(u=>u.owner==0&&u.type==1);Check(!road.StartJob(worker,1),"road gated by Wheel");road.tech[TechnologyCatalog.Index("wheel")]=true;Check(road.StartJob(worker,1),"Wheel enables road");
    var legacy=World.New(4);legacy.version=4;legacy.tech=new bool[12];legacy.tech[3]=true;legacy.research=4;legacy.science=23;
    for(int i=0;i<3;i++){legacy.nations[i].tech=new bool[12];legacy.nations[i].progress=new int[12];legacy.nations[i].research=0;}
    legacy.UpgradeSave();Check(legacy.Valid()&&legacy.HasTech(0,"engineering")&&legacy.HasTech(0,"chivalry")&&legacy.HasTech(0,"wheel")&&legacy.nations[0].progress[4]==23,"legacy progress and unlocks preserved");
    var saved=legacy.tech.ToArray();legacy.UpgradeSave();Check(saved.SequenceEqual(legacy.tech),"migration idempotent");
    var production=World.New(41);var city=production.Found(production.units.First(u=>u.owner==0&&u.type==0));
    Check(production.BuildReason(city,7).Contains("Pottery"),"Granary requires Pottery");production.tech[TechnologyCatalog.Index("pottery")]=true;Check(production.BuildReason(city,7)=="","Pottery unlocks Granary");
    int before=production.Yield(city)[2];production.tech[TechnologyCatalog.Index("electricity")]=true;Check(production.Yield(city)[2]==before+3,"science bonus wired");
    production.tech[TechnologyCatalog.Index("uploading")]=true;int food=production.Yield(city)[0];production.tech[TechnologyCatalog.Index("uploading")]=false;Check(food-production.Yield(city)[0]==city.pop*2,"uploading removes food consumption");
    production.tech[TechnologyCatalog.Index("satellites")]=true;production.Reveal();Check(production.tiles.All(t=>t.seen),"Satellites reveals world");
}
}
}
