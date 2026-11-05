using System;
using System.Linq;
using System.Collections.Generic;
namespace ParadigmRealm {
public static class ContentChecks {
static void Check(bool ok,string why){if(!ok)throw new Exception("Content: "+why);}
static int K(string id){return TechnologyCatalog.Index(id);}
public static void Run(){
    var w=World.New(137);Check(w.tech[TechnologyCatalog.Start]&&w.nations.All(n=>n.tech[TechnologyCatalog.Start]),"every civilisation starts with Stone Tools");
    Check(w.AvailableTech().Where(TechnologyCatalog.IsKeystone).OrderBy(i=>i).SequenceEqual(new[]{K("agriculture"),K("archery"),K("bronze")}.OrderBy(i=>i)),"Stone Tools opens Agriculture, Archery and Bronze Working (and Dawn minors)");
    var researched=new HashSet<int>{TechnologyCatalog.Start};while(researched.Count<TechnologyCatalog.Count){var available=w.AvailableTech();Check(available.Count>0,"no cycle or unreachable research");foreach(int i in available){Check(World.Requirements[i].All(p=>researched.Contains(p)),"prerequisites already complete");researched.Add(i);w.tech[i]=true;}}
    Check(researched.Count==180&&TechnologyCatalog.KeystoneCount==36&&TechnologyCatalog.EraNames.Length==6&&Enumerable.Range(0,6).All(e=>Enumerable.Range(0,180).Count(t=>TechnologyCatalog.Eras[t]==e)==30),"180 technologies, 36 keystones, 30 per era");
    Check(Enumerable.Range(36,144).All(t=>World.Requirements[t].Length>0&&World.Requirements[t].All(p=>TechnologyCatalog.Eras[p]==TechnologyCatalog.Eras[t])&&World.ActiveTechBenefits(t).Length==1),"each minor technology needs only its own era and has one listed effect");w.CheckVictory();Check(w.result=="","researching every keystone no longer wins by itself (Stage 6: Transcendence is a project)");
    StageFive();
    var cold=World.New(3);var settler=cold.units.First(u=>u.owner==0&&u.type==0);cold.At(settler.x,settler.y).terrain=8;cold.tech[K("fire")]=false;Check(cold.Found(settler)==null,"cold settlement needs Fire");cold.tech[K("fire")]=true;Check(cold.Found(settler)!=null,"Fire (Stone Tools) permits cold settlement");
    var terrainTypes=new HashSet<int>();for(int seed=0;seed<6;seed++){var map=World.New(seed);Check(map.Valid(),"biome map validates");foreach(var tile in map.tiles){terrainTypes.Add(tile.terrain);Check(map.TileYield(tile).Length==3,"all terrain has yields");}}
    Check(terrainTypes.Count==21,"all texture terrain types generated");
    var movement=World.New(3);var unit=movement.units.First(u=>u.owner==0&&u.type==2);int x=unit.x+1,y=unit.y;
    foreach(int t in new[]{4,5,6,12}){movement.At(x,y).terrain=t;unit.moves=4;Check(!movement.Move(unit,x,y),"water/mountain impassable");}
    // Poles: crossable at great cost (all movement), wearing units down, and never settled.
    foreach(int t in new[]{14,15}){movement.At(x,y).terrain=t;unit.moves=4;Check(movement.StepCost(unit,unit.x,unit.y,movement.At(x,y))>=4,"ice costs a whole turn");}
    {var ice=World.New(5);var walker=ice.units.First(u=>u.owner==0&&u.type==2);ice.At(walker.x+1,walker.y).terrain=14;walker.moves=World.Moves[walker.type]*3;Check(ice.Move(walker,walker.x+1,walker.y),"ice sheet can be crossed");walker.hp=10;ice.EndTurn();Check(!ice.units.Contains(walker)||walker.hp<10,"units on polar ice lose strength");
     var polarSettler=ice.units.First(u=>u.owner==0&&u.type==0);ice.At(polarSettler.x,polarSettler.y).terrain=15;Check(ice.Found(polarSettler)==null,"glaciers cannot be settled");}
    Check(TechnologyCatalog.Index("astronomy")==TechnologyCatalog.Index("mysticism"),"astronomy (globe zoom) is part of Mysticism");
    // Land types (New Game): every type makes a valid world; archipelago has the most sea, lakes and inland seas the least.
    {int[] share=new int[World.LandTypeNames.Length];for(int m=0;m<share.Length;m++){var lw=World.New(300+m,"Lands",m,1,1,0,1,1);Check(lw.Valid()&&lw.mapType==m,"land type "+World.LandTypeNames[m]+" makes a valid world");var lhome=lw.tiles.Where(t=>t.realm==0).ToArray();share[m]=lhome.Count(t=>t.terrain==4||t.terrain==12)*100/lhome.Length;
      int clash=0;foreach(var a in lw.tiles.Where(t=>t.realm==0))for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var b=lw.Step(a.x,a.y,dx,dy);if(b!=null&&b!=a&&b.realm==0&&lw.BiomeClash(a,b))clash++;}
      Check(clash==0,"no desert beside ice or jungle beside tundra on "+World.LandTypeNames[m]+" ("+clash+" clashes)");}
     Check(share[2]>share[0]&&share[4]<share[0],"archipelago is mostly sea; lakes type has the most land");}
    // Sea resources: ten of them, on water only, with real yields.
    {int sea=0;for(int s=0;s<3;s++){var sw=World.New(310+s,"Seas",0,1,1,0,1,1);foreach(var t in sw.tiles.Where(t=>t.resource>=6)){Check(World.IsSeaResource(t.resource)&&(t.terrain==4||t.terrain==6||t.terrain==12),"sea resource "+World.ResourceNames[t.resource]+" sits on water");sea++;}}
     Check(sea>0&&Enumerable.Range(6,9).All(r=>World.ResourceFood[r]+World.ResourceShields[r]+World.ResourceTrade[r]>0),"sea resources placed and each yields something (offshore oil needs Refining)");}
    movement.At(x,y).terrain=11;unit.moves=4;Check(movement.Move(unit,x,y)&&unit.moves==1,"jungle movement cost");
    var road=World.New(7);var worker=road.units.First(u=>u.owner==0&&u.type==1);Check(!road.StartJob(worker,1),"road gated by Wheel");road.tech[K("wheel")]=true;Check(road.StartJob(worker,1),"Wheel enables road");
    // Woodland must be cleared (Bronze Working, for axes) before a farm; clearing turns Forest into Grassland.
    {var wood=World.New(8);var founder=wood.units.First(u=>u.owner==0&&u.type==0);wood.Found(founder);var hand=wood.units.First(u=>u.owner==0&&u.type==1);hand.x=founder.x;hand.y=founder.y;hand.moves=4;var spot=wood.At(hand.x,hand.y);spot.terrain=1;spot.farm=false;wood.tech[K("agriculture")]=true;
     Check(wood.JobReason(hand,2)=="Clear the trees first.","farm blocked by forest");Check(wood.JobWork(hand,5)==6,"Stone Tools clear woodland in six worker-turns");wood.tech[K("bronze")]=true;Check(wood.JobWork(hand,5)==4,"Bronze Working clears in four");wood.tech[K("iron")]=true;Check(wood.StartJob(hand,5)&&hand.jobTurns==3,"Iron Working clears in three");
     for(int k=0;k<5&&spot.terrain==1;k++)wood.EndTurn();Check(spot.terrain==0&&hand.job==0,"cleared forest becomes grassland");Check(World.Cleared(13)==8&&World.Cleared(19)==9&&World.Cleared(11)==0,"arctic forest, mangrove and jungle clear to tundra, bog and grassland");}
    // Owner requests 31 Oct: city reach grows with size; overlaps go to the larger city unless the other locks the tile; the first
    // settler sees 4 tiles until it moves; ruins appear and are explored once; worker road routes and auto develop.
    {Check(World.WorkRadius(new City{pop=9})==2&&World.WorkRadius(new City{pop=10})==3&&World.WorkRadius(new City{pop=20})==4&&World.WorkRadius(new City{pop=30})==5&&World.WorkRadius(new City{pop=40})==5,"reach 2, +1 at size 10, 20 and 30, then no more");
     var two=World.New(12,"Overlap",0,1,1,0,1,1);var founder=two.units.First(u=>u.owner==0&&u.type==0);var big=two.Found(founder);big.pop=6;
     var spot=two.Step(big.x,big.y,3,0);if(spot!=null&&!World.IsImpassable(spot.terrain)&&!PixelArt.IsWater(spot.terrain)){var small=new City{id=two.nextId++,owner=0,x=spot.x,y=spot.y,name="Overlap Two",pop=2};two.cities.Add(small);two.RebuildTerritory();
      var shared=two.tiles.FirstOrDefault(t=>two.InReach(big,t)&&two.InReach(small,t));
      if(shared!=null){Check(two.TileController(shared,0)==big,"the larger city works a shared tile");small.locked=new List<int>();Check(two.ToggleLock(small,shared)&&two.TileController(shared,0)==small,"locking it from the smaller city takes it over");}}
     var start=World.New(13);var firstSettler=start.units.First(u=>u.owner==0&&u.type==0);Check(start.SightRange(firstSettler)>=4,"the first settler sees 4 tiles from its start");firstSettler.moves--;Check(start.SightRange(firstSettler)<=3,"after moving it sees the usual range");
     var ruined=World.New(14,"Ruins",0,1,1,0,1,1);var ruin=ruined.tiles.FirstOrDefault(t=>t.ruin);Check(ruin!=null,"ruins appear on new maps");
     if(ruin!=null){var walker=ruined.Add(0,3,ruin.x,ruin.y);var from=new[]{-1,0,1}.SelectMany(dj=>new[]{-1,0,1}.Select(di=>ruined.Step(ruin.x,ruin.y,di,dj))).FirstOrDefault(t=>t!=null&&!(t.x==ruin.x&&t.y==ruin.y)&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));
      if(from!=null){walker.x=from.x;walker.y=from.y;walker.moves=6;ruin.owner=-1;if(ruined.Move(walker,ruin.x,ruin.y))Check(!ruin.ruin,"a unit explores the ruins once");}}}
    // Mountain ranges are capped at about sqrt(tiles)/4 connected peaks, so small maps are not walled off.
    {var peaks=World.New(9);int cap=Math.Max(4,(int)Math.Round(Math.Sqrt(peaks.tiles.Count(t=>t.realm==0))/4)),largest=0;var seen=new HashSet<Tile>();
     foreach(var t in peaks.tiles.Where(t=>t.realm==0&&(t.terrain==5||t.terrain==15))){if(!seen.Add(t))continue;int size=0;var stack=new Stack<Tile>();stack.Push(t);while(stack.Count>0){var a=stack.Pop();size++;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var b=peaks.Step(a.x,a.y,dx,dy);if(b!=null&&b.realm==0&&(b.terrain==5||b.terrain==15)&&seen.Add(b))stack.Push(b);}}largest=Math.Max(largest,size);}
     Check(largest<=cap,"largest mountain range "+largest+" within cap "+cap);}
    // A version 4 save (12 original technologies) still upgrades all the way to keystones.
    var legacy=World.New(4,natives:false);legacy.version=4;legacy.tech=new bool[12];legacy.tech[LegacyTechnologyCatalog.Index("engineering")]=true;legacy.research=4;legacy.science=23;
    for(int i=0;i<3;i++){legacy.nations[i].tech=new bool[12];legacy.nations[i].progress=new int[12];legacy.nations[i].research=0;}
    legacy.UpgradeSave();Check(legacy.Valid()&&legacy.HasTech(0,"engineering")&&legacy.HasTech(0,"chivalry")&&legacy.HasTech(0,"wheel")&&legacy.nations[0].progress[K("industrialisation")]==23,"legacy progress and unlocks preserved");
    var saved=legacy.tech.ToArray();legacy.UpgradeSave();Check(saved.SequenceEqual(legacy.tech),"migration idempotent");
    var production=World.New(41);var city=production.Found(production.units.First(u=>u.owner==0&&u.type==0));
    Check(production.BuildReason(city,7).Contains("Agriculture"),"Granary requires Agriculture (Pottery)");production.tech[K("pottery")]=true;Check(production.BuildReason(city,7)=="","Agriculture unlocks Granary");
    int before=production.Yield(city)[2];production.tech[K("electricity")]=true;Check(production.Yield(city)[2]==before+3,"science bonus wired");
    production.tech[K("uploading")]=true;int food=production.Yield(city)[0];production.tech[K("uploading")]=false;Check(food-production.Yield(city)[0]==city.pop*2,"uploading removes food consumption");
    production.tech[K("satellites")]=true;production.Reveal();Check(production.tiles.Where(t=>t.realm==0).All(t=>t.seen)&&production.tiles.Where(t=>t.realm!=0).All(t=>!t.seen),"Satellites reveal the home world, not the parallel worlds");
}
// 16-bit plan Stage 5: keystone technologies, costs, eras and edicts, and the version 13 conversion.
static void StageFive(){
    Check(TechnologyCatalog.LegacyMap.All(k=>k>=0&&k<TechnologyCatalog.Count),"every one of the 124 earlier technologies is folded into a keystone");
    Check(Enumerable.Range(0,LegacyTechnologyCatalog.Count).All(i=>K(LegacyTechnologyCatalog.Ids[i])==TechnologyCatalog.LegacyMap[i]),"earlier ids resolve to their keystone");
    for(int e=0;e<6;e++)Check(Enumerable.Range(0,TechnologyCatalog.KeystoneCount).Count(i=>TechnologyCatalog.Eras[i]==e)==6,"six keystones in era "+e);
    foreach(var name in new[]{"Bronze Working","Writing","Currency","Code of Laws","Engineering","Industry","Electricity","Computing"})Check(TechnologyCatalog.Names.Contains(name),"design milestone "+name);
    for(int i=0;i<TechnologyCatalog.Count;i++){int e=TechnologyCatalog.Eras[i]+1,p=i%6+1;if(TechnologyCatalog.IsKeystone(i))Check(TechnologyCatalog.Costs[i]==20*e*e+5*e*p,"cost formula "+TechnologyCatalog.Names[i]);foreach(int pre in TechnologyCatalog.Requirements[i])Check(TechnologyCatalog.Eras[pre]<=TechnologyCatalog.Eras[i],"no prerequisite from a later era");}
    Check(TechnologyCatalog.Costs[1]==30&&TechnologyCatalog.Costs[5]==50&&TechnologyCatalog.Costs[35]==900,"cost range: era 1 25-50, era 6 up to 900");
    foreach(int u in new[]{0,2,3})Check(World.UnitTech[u]==TechnologyCatalog.Start,"Warriors, Scouts and Settlers from turn 1");
    // A fifth off once two civilisations you have met know it.
    var w=World.New(5);w.speed=1;int t=K("currency");int full=w.ResearchCost(t);
    w.nations[1].tech[t]=true;w.nations[2].tech[t]=true;Check(w.ResearchCost(t)==full,"no discount from civilisations not met");
    w.met[1]=w.met[2]=true;
    Check(w.ResearchCost(t)==full-full/5,"a fifth off when two met civilisations know it");
    // Eras and edicts.
    w=World.New(6);Check(w.Era(0)==0&&w.PendingEdict(0)==-1,"start in the Dawn with no edict");
    foreach(var id in new[]{"agriculture","archery"})w.tech[K(id)]=true;Check(w.Era(0)==0,"3 of 6 is not enough");
    w.tech[K("bronze")]=true;Check(w.Era(0)==1&&w.PendingEdict(0)==0,"4 of 6 completes the Dawn and offers an edict");
    Check(!w.ChooseEdict(0,1,0),"no edict for an era not yet complete");
    var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));int[] y=w.Yield(c);
    Check(w.ChooseEdict(0,0,2)&&w.PendingEdict(0)==-1&&w.Yield(c)[2]==y[2]+1,"Sky Watchers adds 1 science");
    Check(!w.ChooseEdict(0,0,1),"an edict is permanent");
    Check(w.ScienceCap(0)==60&&w.SetScienceRate(0,80)&&w.nations[0].scienceRate==60,"science capped at 60% without Academies");
    foreach(var id in new[]{"currency","laws","maths","mysticism"})w.tech[K(id)]=true;Check(w.Era(0)==2&&w.ChooseEdict(0,1,2),"Classical complete; Academies chosen");
    Check(w.ScienceCap(0)==80&&w.SetScienceRate(0,80)&&w.nations[0].scienceRate==80&&w.Valid(),"Academies lifts the science cap to 80%");
    // The AI chooses its own edicts.
    foreach(var id in new[]{"agriculture","archery","bronze","wheel"})w.nations[1].tech[K(id)]=true;w.EndTurn();Check(w.nations[1].edicts[0]>=0,"AI adopts an edict for a completed era");
    // Once the game has a result, edicts cannot be chosen; the AI must not keep retrying (this used to loop forever).
    foreach(var id in new[]{"agriculture","archery","bronze","wheel"})w.nations[2].tech[K(id)]=true;w.result="Test over.";w.EmpireAIForChecks(2);Check(w.nations[2].edicts[0]<0,"no edict and no endless retry after the game ends");
    // Version 12 saves (124 technologies) convert to keystones.
    var old=World.New(8,natives:false);old.version=12;
    foreach(var n in old.nations){n.tech=new bool[LegacyTechnologyCatalog.Count];n.progress=new int[LegacyTechnologyCatalog.Count];n.edicts=null;}
    old.tech=old.nations[0].tech;foreach(var id in new[]{"toolmaking","agriculture","pottery","masonry","steam"})old.tech[LegacyTechnologyCatalog.Index(id)]=true;
    old.nations[0].progress[LegacyTechnologyCatalog.Index("sailing")]=11;old.nations[0].progress[LegacyTechnologyCatalog.Index("shipbuilding")]=7;old.research=LegacyTechnologyCatalog.Index("sailing");old.science=11;
    old.UpgradeSave();
    Check(old.version==25&&old.Valid(),"version 12 converts to a valid version 25");
    Check(old.HasTech(0,"pottery")&&old.HasTech(0,"masonry")&&old.HasTech(0,"steam")&&old.HasTech(0,"engineering"),"any known folded technology keeps its keystone, with prerequisites");
    Check(old.research==K("sailing")&&old.science==18,"research and progress move to the keystone (Sailing 11 + Shipbuilding 7)");
    Check(old.nations.All(n=>n.edicts!=null&&n.edicts.Length==6),"edict slots added");
    ReligionChecks();
}
// Owner requests 31 Oct, third batch: religion, border influence, settler cost, worker activities (save version 22).
static void ReligionChecks(){
    Check(ReligionCatalog.Count==23&&ReligionCatalog.Name(0)=="Christianity"&&ReligionCatalog.Name(12)=="Egyptian Religion"&&Enumerable.Range(0,ReligionCatalog.Count).All(r=>TechnologyCatalog.Index(ReligionCatalog.Tech(r))>=0&&ReligionCatalog.Tenet(r)>=0&&ReligionCatalog.Tenet(r)<=6),"23 faiths (12 living, 11 ancient), each with a real founding technology and a tenet");
    Check(World.UnitCosts[0]==90,"settlers cost three times as much (90)");
    var w=World.New(61);var founder=w.units.First(u=>u.owner==0&&u.type==0);var home=w.Found(founder);Check(home!=null&&w.Valid()&&w.version==25,"new games are valid version 25");
    Check(w.JobTechMissing(0,1)=="Requires The Wheel."&&w.JobTechMissing(0,5)=="","worker jobs report a missing technology (Road needs The Wheel; clearing has Stone Tools)");
    Check(w.FaithYield(home)==1&&w.ChoosePantheon(0,4)==false,"a capital gives 1 faith; a pantheon needs 25 faith");
    w.nations[0].faith=300;Check(w.ChoosePantheon(0,4)&&w.nations[0].pantheon==4&&w.nations[0].faith==275,"pantheon adopted for 25 faith");
    Check(w.FoundingReason(0,3)=="Requires Mysticism.","founding needs Mysticism");
    foreach(var id in new[]{"agriculture","writing","mysticism","laws","philosophy"})w.nations[0].tech[K(id)]=true;
    Check(w.FoundingReason(0,3)=="Needs a city with a Temple to be the holy city.","founding needs a Temple");home.buildings.Add(36);
    Check(w.FoundingReason(0,5)=="Requires Printing Press."&&w.FoundReligion(0,3)&&home.religion==3&&w.IsHolyCity(home)&&!w.FoundReligion(0,4),"Buddhism founded in the capital, which becomes its holy city; only one faith each");
    Check(w.FaithYield(home)==7&&w.ReligionContent(home)==2,"holy capital with a Temple and Ancestor Spirits: 7 faith; Middle Way and own faith: +2 content");
    var far=w.tiles.First(t=>w.Distance(t.x,t.y,home.x,home.y)==6);var rival=new City{id=w.nextId++,owner=1,x=far.x,y=far.y,name="Testholm",pop=3};w.cities.Add(rival);
    for(int i=0;i<20;i++)w.UpdateReligion();Check(rival.religion==3&&w.Followers(3)>=2&&w.ReligionTithe(0)>=1,"the holy city's pressure converts a rival city 6 tiles away; the founder gains a tithe");
    var other=new City{id=w.nextId++,owner=2,x=w.tiles.First(t=>w.Distance(t.x,t.y,home.x,home.y)==11).x,y=w.tiles.First(t=>w.Distance(t.x,t.y,home.x,home.y)==11).y,name="Farpost",pop=2};w.cities.Add(other);w.At(other.x,other.y).seen=true;
    int before=w.nations[0].faith;Check(w.MissionReason(0,other)==""&&w.SendMissionaries(0,other)&&w.nations[0].faith==before-World.MissionCost&&other.religion==3,"missionaries convert a known city within 12 tiles of a faithful city");
    Check(w.Valid(),"world with religion stays valid");
    // Border influence: a much stronger rival city slowly wins a contested tile; it takes many turns and flips at 90%.
    var b=World.New(62);var a=new City{id=b.nextId++,owner=0,x=b.Width/2,y=b.MainHeight/2,name="Weakton",pop=1};b.cities.Add(a);
    var spot=b.tiles.First(t=>b.Distance(t.x,t.y,a.x,a.y)==4);var strong=new City{id=b.nextId++,owner=1,x=spot.x,y=spot.y,name="Grandcity",pop=20,age=200};strong.buildings.AddRange(new[]{36,42,60,61,62});b.cities.Add(strong);
    b.RebuildTerritory();var mid=b.tiles.First(t=>b.Distance(t.x,t.y,a.x,a.y)==2&&b.Distance(t.x,t.y,strong.x,strong.y)==2);var touch=b.tiles.First(t=>b.Distance(t.x,t.y,a.x,a.y)==1&&b.Distance(t.x,t.y,strong.x,strong.y)<=3);
    int mine=b.cities.Count(c=>c.owner==0);if(mid.owner!=0)mid.owner=0;int turns=0;while(mid.owner==0&&turns<100){b.SpreadInfluence();b.RebuildTerritory();turns++;}
    Check(mid.owner==1&&turns>=15&&turns<=45,"a contested tile changes hands slowly (took "+turns+" turns) to the stronger culture and faith");
    Check(touch.owner==0,"tiles next to a city never change hands");
    var same=World.New(63);var c1=new City{id=same.nextId++,owner=0,x=same.Width/2,y=same.MainHeight/2,name="Oneton",pop=4};same.cities.Add(c1);var s2=same.tiles.First(t=>same.Distance(t.x,t.y,c1.x,c1.y)==4);same.cities.Add(new City{id=same.nextId++,owner=0,x=s2.x,y=s2.y,name="Twoton",pop=4});same.RebuildTerritory();
    Check(same.tiles.Where(t=>same.Distance(t.x,t.y,c1.x,c1.y)<=2||same.Distance(t.x,t.y,s2.x,s2.y)<=2).All(t=>t.owner==0),"two cities of one civilisation share one territory (their borders merge)");
    // Version 21 saves gain religion and influence fields.
    var v=World.New(64);v.version=21;v.nations[1].pantheon=5;foreach(var t in v.tiles)t.swayCiv=-7;v.UpgradeSave();Check(v.version==25&&v.Valid()&&v.nations.All(n=>n.pantheon==-1&&n.founded==-1&&n.faith==0)&&v.tiles.All(t=>t.swayCiv==-1),"version 21 saves upgrade to version 25");
    CivilisationAndAgeChecks();
}
// Owner requests 31 Oct, fifth batch: civilisations in full, real civilisations, priests, era score and ages (save version 25).
static void CivilisationAndAgeChecks(){
    int full=Enumerable.Range(0,CivilisationCatalog.Count).Count(CivLore.Full);Check(full==32,"32 civilisations with full lore (10 premade, 12 real, 10 native peoples), found "+full);
    foreach(int civ in Enumerable.Range(0,CivilisationCatalog.Count).Where(CivLore.Full)){var e=CivLore.Of(civ);Check(e.Background.Length>100&&e.History.Length>200&&e.Strengths!=""&&e.Weaknesses!=""&&e.Ability!=""&&e.Unit!=""&&e.Building!=""&&e.Weakness!=""&&e.UnitBase>=0&&e.UnitBase<World.UnitNames.Length&&Array.IndexOf(World.BuildingIds,e.BuildingBase)>=0&&e.Agenda>=0&&e.Agenda<CivLore.AgendaNames.Length&&e.Faiths.All(f=>f>=0&&f<ReligionCatalog.Count),CivilisationCatalog.Names[civ]+" is complete");}
    Check(CivilisationCatalog.Names[17]=="China"&&CivilisationCatalog.Leaders[26]=="Cyrus the Great"&&CivilisationCatalog.Enabled[23],"real civilisations offered (China ... Persia, Maya)");
    // Steppe Horde: Keshig name and +2 movement for mounted units; Egypt: river cities +1 food and production.
    var w=World.New(71);Check(w.SetPlayerCivilisation(11,"","",0,-1,-1),"play as the Steppe Horde");var rider=w.Add(0,8,w.units.First(u=>u.owner==0).x,w.units.First(u=>u.owner==0).y);
    Check(w.UnitName(rider)=="Keshig"&&w.MaxMoves(rider)==World.Moves[8]+2&&w.Strength(rider,null,false)==World.Attack[8]+1,"Keshig: unique name, +1 attack, +2 movement for mounted units");
    var e2=World.New(72);Check(e2.SetPlayerCivilisation(18,"","",0,-1,-1),"play as Egypt");var pharaoh=e2.units.First(u=>u.owner==0&&u.type==0);var nile=e2.Found(pharaoh);
    if(nile!=null){int before=e2.Yield(nile)[0];bool fresh=e2.FreshWater(nile.x,nile.y);Check(e2.BuildNameFor(0,World.UnitBuild(8))=="Maryannu Chariot","Egyptian chariot name in build lists");
     nile.buildings.Add(36);Check(e2.FaithYield(nile)==1+2+2,"Mortuary Temple: capital 1 + Temple 2 + 2 more faith");}
    // Agendas: Keeper of the Peace dislikes a civilisation at war.
    var a=World.New(73);a.nations[1].civ=5;int calm=a.AgendaOpinion(1,0);a.war[1]=true;a.nations[2].civ=5;Check(calm==5&&a.AgendaOpinion(2,0)==-12,"Keeper of the Peace agenda: +5 at peace, -12 when at war");
    // Priests: need a city following a faith with a Temple; bought for faith; preach converts; content and healing.
    var p=World.New(74);var founder=p.units.First(u=>u.owner==0&&u.type==0);var holy=p.Found(founder);Check(holy!=null,"city for priests");
    Check(p.BuildReason(holy,World.UnitBuild(World.PriestUnit))!="","priests need a faith and a Temple");
    foreach(var id in new[]{"agriculture","writing","mysticism","laws","philosophy"})p.nations[0].tech[K(id)]=true;holy.buildings.Add(36);p.nations[0].faith=500;p.ChoosePantheon(0,0);Check(p.FoundReligion(0,3),"Buddhism founded");
    Check(p.BuildReason(holy,World.UnitBuild(World.PriestUnit))=="","a Temple city following a faith can train priests");
    var priest=p.BuyPriest(0,holy);Check(priest!=null&&priest.faith==3&&priest.charges==World.PriestCharges&&p.nations[0].faith==500-World.PantheonCost-60-World.PriestFaithCost,"a priest is bought for faith and carries the city's faith");
    Check(p.PriestContent(holy)==1,"a priest in its city keeps a citizen content");
    var spot=p.tiles.First(t=>p.Distance(t.x,t.y,holy.x,holy.y)==7&&!PixelArt.IsWater(t.terrain));var heathen=new City{id=p.nextId++,owner=1,x=spot.x,y=spot.y,name="Faraway",pop=3};p.cities.Add(heathen);
    priest.x=spot.x;priest.y=spot.y;priest.moves=4;Check(p.Preach(priest)&&heathen.faithPressure[3]>=World.PreachPressure&&heathen.religion==3&&priest.charges==1,"preaching converts a city and uses a sermon");
    priest.moves=4;Check(p.PreachReason(priest)=="",  "a second sermon is possible");p.Preach(priest);Check(!p.units.Contains(priest),"after two sermons the priest retires");
    var wounded=p.Add(0,2,holy.x,holy.y);wounded.hp=4;var healer=p.BuyPriest(0,holy)??p.Add(0,World.PriestUnit,holy.x,holy.y);healer.faith=3;p.EndTurn();Check(wounded.hp>=6,"a priest heals units beside it by 2 a turn (hp "+wounded.hp+")");
    Check(p.Valid(),"world with priests stays valid");
    // Era score and ages: deeds add score; the next era starts a Golden Age when the score is high, a Dark Age when low.
    var g=World.New(75);var settler=g.units.First(u=>u.owner==0&&u.type==0);g.Found(settler);Check(g.nations[0].eraScore>=2,"founding a city earns era score");
    g.nations[0].eraScore=40;foreach(var id in new[]{"agriculture","archery"})g.nations[0].tech[K(id)]=true;g.nations[0].research=K("bronze");g.research=g.nations[0].research;g.nations[0].progress[K("bronze")]=100000;g.science=100000;g.EndTurn();
    Check(g.Era(0)==1&&g.AgeOf(0)==World.AgeGolden&&g.nations[0].ageTurns>0,"a high era score starts a Golden Age in the next era");
    var city=g.cities.First(c=>c.owner==0);int golden=g.ContentSlots(city);g.nations[0].ageTurns=0;Check(g.ContentSlots(city)==golden-1,"the Golden Age gives +1 content");
    var d=World.New(76);d.Found(d.units.First(u=>u.owner==0&&u.type==0));d.nations[0].eraScore=0;foreach(var id in new[]{"agriculture","archery"})d.nations[0].tech[K(id)]=true;d.nations[0].research=K("bronze");d.research=d.nations[0].research;d.nations[0].progress[K("bronze")]=100000;d.science=100000;d.EndTurn();
    Check(d.AgeOf(0)==World.AgeDark,"a low era score brings a Dark Age");
    // Real World games draw rivals from the real civilisations, Fantasy games from the fantasy kingdoms (custom civilisations too).
    var realGame=World.New(78,"",0,1,1,0,3,1,8,17);Check(realGame.realWorld&&Enumerable.Range(1,realGame.civCount-1).Where(o=>!realGame.IsNativeNation(o)).All(o=>CivilisationCatalog.IsReal(realGame.nations[o].civ)),"a Real World game has real rivals");
    var fantasyGame=World.New(79,"",0,1,1,0,3,1,8,0);Check(!fantasyGame.realWorld&&Enumerable.Range(1,fantasyGame.civCount-1).Where(o=>!fantasyGame.IsNativeNation(o)).All(o=>!CivilisationCatalog.IsReal(fantasyGame.nations[o].civ)),"a Fantasy game has fantasy rivals");
    var customReal=World.New(80,"Free Cantons",0,1,1,0,3,1,6,-1,"Mayor Tell",18,1,2,true);Check(customReal.realWorld&&customReal.nations[0].civ==-1&&Enumerable.Range(1,customReal.civCount-1).Where(o=>!customReal.IsNativeNation(o)).All(o=>CivilisationCatalog.IsReal(customReal.nations[o].civ))&&customReal.Valid(),"a custom civilisation in the Real World group meets real rivals");
    var oldSave=World.New(77);oldSave.version=22;foreach(var u in oldSave.units)u.faith=0;oldSave.UpgradeSave();Check(oldSave.version==25&&oldSave.units.All(u=>u.faith==-1)&&oldSave.Valid(),"version 22 saves upgrade to 23 (units carry no faith)");
    RealmChecks();HexChecks();
}
// Owner request 5 Nov: the hex grid. Geometry, picture mapping and whole games on both map shapes.
static void HexChecks(){
    foreach(int shape in new[]{0,2}){
     var w=World.New(120+shape,"Hexes",0,1,1,0,1,shape,4,hex:true);string name=shape==2?"globe":"flat";
     Check(w.Hex&&w.Valid()&&w.tiles.Length==w.Width*w.Height,name+" hex world is valid");
     // Every tile away from the edge has exactly six neighbours, each one step away, and each neighbour leads back.
     foreach(var t in w.tiles.Where(t=>t.realm==0&&t.y>1&&t.y<w.MainHeight-2&&t.x>1&&t.x<w.Width-2)){
      var around=new List<Tile>();for(int dj=-1;dj<=1;dj++)for(int di=-1;di<=1;di++){if(di==0&&dj==0)continue;var n=w.Step(t.x,t.y,di,dj);if(n!=null)around.Add(n);}
      if(around.Count!=6||around.Distinct().Count()!=6)Check(false,name+" tile "+t.x+","+t.y+" has "+around.Count+" neighbours");
      foreach(var n in around)if(w.Distance(t.x,t.y,n.x,n.y)!=1||w.Distance(n.x,n.y,t.x,t.y)!=1)Check(false,name+" neighbour distance at "+t.x+","+t.y);
      for(int k=0;k<6;k++){var n=w.Step(t.x,t.y,World.HexDi[k],World.HexDj[k]);if(n==null||w.Step(n.x,n.y,World.HexDi[(k+3)%6],World.HexDj[(k+3)%6])!=t)Check(false,name+" opposite directions at "+t.x+","+t.y);}
     }
     // Distance agrees with walking the grid, and a wide loop is exactly the hexagon of that radius.
     {var o=w.tiles.First(t=>t.realm==0&&t.x==w.Width/2&&t.y==w.MainHeight/2);var depth=new Dictionary<Tile,int>{{o,0}};var q=new Queue<Tile>();q.Enqueue(o);
      while(q.Count>0){var a=q.Dequeue();if(depth[a]>=8)continue;for(int k=0;k<6;k++){var n=w.Step(a.x,a.y,World.HexDi[k],World.HexDj[k]);if(n!=null&&!depth.ContainsKey(n)){depth[n]=depth[a]+1;q.Enqueue(n);}}}
      foreach(var kv in depth)if(w.Distance(o.x,o.y,kv.Key.x,kv.Key.y)!=kv.Value)Check(false,name+" distance differs from walking at "+kv.Key.x+","+kv.Key.y);
      for(int r=1;r<=5;r++){var seen=new HashSet<Tile>();for(int dj=-r;dj<=r;dj++)for(int di=-r;di<=r;di++){var t=w.StepIn(o.x,o.y,di,dj,r);if(t!=null)seen.Add(t);}Check(seen.Count==3*r*(r+1)+1&&seen.All(t=>w.Distance(o.x,o.y,t.x,t.y)<=r),name+" radius "+r+" hexagon has "+seen.Count+" tiles");}}
     // A whole game keeps every rule valid; rivals expand and every unit stands on a real tile.
     for(int turn=0;turn<40;turn++)w.EndTurn();
     Check(w.Valid()&&w.result==""&&w.units.All(u=>w.At(u.x,u.y)!=null),name+" hex world stays valid for 40 turns");
     Check(Enumerable.Range(1,w.civCount-1).All(o=>w.cities.Count(c=>c.owner==o)>=2),name+" every people has expanded by turn 40 ("+string.Join(",",Enumerable.Range(1,w.civCount-1).Select(o=>w.cities.Count(c=>c.owner==o)))+")");
    }
    // East-west wrap on a globe: the last column touches the first.
    {var g=World.New(123,"Round",0,1,1,0,1,2,3,hex:true);var e=g.At(g.Width-1,5);var n=g.Step(e.x,e.y,1,0);Check(n.x==0&&n.y==5&&g.Distance(e.x,e.y,n.x,n.y)==1,"a hex globe wraps east to west");Check(g.Distance(0,4,g.Width/2,4)==g.Width/2,"the way round is the short way");}
    // Edges of a flat map stop.
    {var f=World.New(124,"Edge",0,1,1,0,1,0,3,hex:true);Check(f.Step(0,4,-1,0)==null&&f.Step(0,0,0,-1)==null,"a flat hex map has edges");}
    // Every point on screen picks a tile whose hexagon holds it, and neighbours sit where the art expects.
    {double tw=64,row=tw*PixelArt.HR/PixelArt.HW;
     for(int y=4;y<8;y++)for(int x=4;x<8;x++){double sx,sy;PixelArt.HexScreenOf(x,y,5.3,5.4,tw,out sx,out sy);foreach(var off in new[]{new[]{0.0,0},new[]{30.0,0},new[]{-30.0,0},new[]{0,18.0},new[]{0,-18.0},new[]{15.0,10},new[]{-15.0,-10}}){int px,py;PixelArt.HexPickOf(sx+off[0],sy+off[1],5.3,5.4,tw,out px,out py);Check(px==x&&py==y,"hex pick round trip "+x+","+y+" got "+px+","+py);}}
     var w=World.New(125,"Art",0,1,1,0,1,2,3,hex:true);double x0,y0;PixelArt.HexScreenOf(6,6,0,0,tw,out x0,out y0);for(int k=0;k<6;k++){var n=w.Step(6,6,World.HexDi[k],World.HexDj[k]);double x1,y1;PixelArt.HexScreenOf(n.x,n.y,0,0,tw,out x1,out y1);Check(Math.Abs(Math.Abs(x1-x0)-(World.HexDj[k]==0?tw:tw/2))<1e-9&&Math.Abs(Math.Abs(y1-y0)-(World.HexDj[k]==0?0:row))<1e-9,"neighbour "+k+" is one hexagon away on screen");}}
    // Pictures fill the hexagon, leave its corners clear and leave no gaps between neighbours.
    {var n9=Enumerable.Repeat(0,9).ToArray();n9[0]=-1;n9[8]=-1;var hex=PixelArt.ComposeHex(n9,10,5,0,0);Check(hex.w==PixelArt.HW&&hex.h==PixelArt.HH&&(hex.Get(32,20)>>24)==255&&(hex.Get(0,0)>>24)==0&&(hex.Get(63,39)>>24)==0&&(hex.Get(0,20)>>24)==255&&(hex.Get(32,0)>>24)==255,"a hex tile picture is hexagon shaped");
     for(int y=-10;y<=10;y++)for(int x=-32;x<=32;x++){bool any=false;foreach(var c in new[]{new[]{0,0},new[]{64,0},new[]{-64,0},new[]{32,30},new[]{-32,30},new[]{32,-30},new[]{-32,-30}}){int px=x-c[0]+32,py=y-c[1]+20;if(px>=0&&py>=0&&px<64&&py<40&&(hex.Get(px,py)>>24)!=0)any=true;}Check(any,"no gap at "+x+","+y);}
     Check((PixelArt.HexEdge(0).Get(62,20)>>24)!=0&&(PixelArt.HexEdge(3).Get(62,20)>>24)==0&&(PixelArt.HexRoad(1).Get(32,20)>>24)!=0,"hex borders and roads sit on the right edges");}
    // An old square save keeps its layout.
    {var old=World.New(126,natives:false);old.version=24;old.UpgradeSave();Check(old.version==25&&!old.Hex&&old.Valid(),"a version 24 save keeps its square map");}
}
// Owner request 31 Oct: ancient gateways and five parallel worlds below the home world.
static void RealmChecks(){
    var w=World.New(91);Check(w.HasRealms&&w.Height==w.MainHeight+World.RealmCount*(w.RealmHeight+World.RealmGap)&&w.RealmHeight==w.MainHeight&&w.tiles.Length==w.Width*w.Height&&w.Valid(),"new worlds carry five parallel worlds below the home world");
    var homeGates=w.tiles.Where(t=>t.realm==0&&t.gateLink>0).ToList();Check(homeGates.Count==World.RealmCount&&homeGates.All(g=>w.GateTwin(w.GateTwin(g))==g)&&homeGates.Select(g=>w.GateTwin(g).realm).OrderBy(r=>r).SequenceEqual(new[]{1,2,3,4,5}),"one gateway pair per parallel world, linked both ways");
    // The worlds are walled off: no tile of one world touches a tile of another, only impassable separator rows.
    bool sealedOff=true;for(int r=1;r<=World.RealmCount&&sealedOff;r++)for(int y=w.RealmTop(r);y<w.RealmBottom(r)&&sealedOff;y++)for(int x=0;x<w.Width&&sealedOff;x++)for(int dj=-1;dj<=1;dj++)for(int di=-1;di<=1;di++){var n=w.Step(x,y,di,dj);if(n!=null&&n.realm!=r&&n.realm!=-1)sealedOff=false;}
    Check(sealedOff&&w.tiles.Where(t=>t.realm==-1).All(t=>World.IsImpassable(t.terrain)),"parallel worlds are sealed off by impassable rows");
    Check(w.RealmDiscovered(0,0)&&Enumerable.Range(1,5).All(r=>!w.RealmDiscovered(0,r)),"only the home world is known at the start");
    // The gates are locked: a land unit on a gateway stays on the home world and learns nothing of the world beyond.
    var gate=homeGates.First(g=>w.GateTwin(g).realm==3);var side=w.tiles.First(t=>t.realm==0&&w.Distance(t.x,t.y,gate.x,gate.y)==1&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));
    var walker=w.Add(0,2,side.x,side.y);w.Reveal();Check(w.gatesLocked&&w.Move(walker,gate.x,gate.y)&&walker.x==gate.x&&walker.y==gate.y&&w.RealmOf(walker.x,walker.y)==0&&!w.RealmDiscovered(0,3)&&w.GateName(gate).Contains("Sealed"),"a locked gateway does not carry a warrior across");
    walker.x=side.x;walker.y=side.y;walker.moves=World.Moves[2];
    {var rival=w.Add(1,2,side.x,side.y);w.Move(rival,gate.x,gate.y);Check(w.RealmOf(rival.x,rival.y)==0,"a locked gateway stops rivals too");w.units.Remove(rival);}
    // Unlocked (kept for a later key): stepping onto a gateway carries a land unit to the twin.
    w.gatesLocked=false;
    Check(w.Move(walker,gate.x,gate.y)&&w.RealmOf(walker.x,walker.y)==3&&walker.moves==0&&w.RealmDiscovered(0,3),"a warrior crosses into the Infernal Depths");
    {var settler=w.Add(0,0,gate.x,gate.y);Check(w.FoundReason(settler).Contains("gateway"),"gateway tiles cannot be settled");w.units.Remove(settler);}
    walker.hp=10;w.EndTurn();Check(walker.hp<=8,"the Infernal Depths burn units outside cities (hp "+walker.hp+")");
    var marsh=homeGates.First(g=>w.GateTwin(g).realm==4);var twin=w.GateTwin(marsh);var sick=w.Add(0,2,twin.x,twin.y);w.EndTurn();Check(sick.plague==5,"units in the Plague Marshes catch plague");
    var home=w.tiles.First(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&w.units.All(u=>w.Distance(u.x,u.y,t.x,t.y)>3));var hc=new City{id=w.nextId++,owner=0,x=home.x,y=home.y,name="Sickbay",pop=3};w.cities.Add(hc);
    sick.x=home.x;sick.y=home.y;w.EndTurn();int sickFood=w.Yield(hc)[0],keep=hc.plague;hc.plague=0;int wellFood=w.Yield(hc)[0];hc.plague=keep;Check(keep>0&&sickFood==wellFood-2,"plague carried home costs a city 2 food a turn");
    Check(w.Valid(),"world with travellers between worlds stays valid");
    // Native peoples: up to ten (within 16 civilisations), each founded in its own world, never required for a home conquest.
    var natives=Enumerable.Range(0,w.civCount).Where(w.IsNativeNation).ToList();Check(natives.Count==Math.Min(10,16-3),"ten native peoples join a three-civilisation game (found "+natives.Count+")");
    Check(natives.All(o=>w.cities.Any(c=>c.owner==o&&w.RealmOf(c.x,c.y)==CivilisationCatalog.NativeRealm(w.nations[o].civ))),"each native people starts with a city in its own world");
    Check(Enumerable.Range(1,World.RealmCount).All(r=>natives.Count(o=>CivilisationCatalog.NativeRealm(w.nations[o].civ)==r)==2),"two native peoples in every parallel world");
    {var crowded=World.New(93,"",0,1,1,0,5,1,14);Check(Enumerable.Range(0,crowded.civCount).Count(crowded.IsNativeNation)==2&&crowded.civCount==16,"a 14-civilisation game has room for two native peoples");}
    // Save version 25 locks the gates and sends stray land units back; a colony's garrison stays.
    {var old=World.New(95);old.version=23;old.gatesLocked=false;var g3=old.tiles.First(t=>t.realm==0&&t.gateLink>0&&old.GateTwin(t).realm==3);var far=old.GateTwin(g3);var stray=old.Add(0,2,far.x,far.y);
     old.UpgradeSave();Check(old.version==25&&old.gatesLocked&&old.RealmOf(stray.x,stray.y)==0&&old.Distance(stray.x,stray.y,g3.x,g3.y)<=1&&old.Valid(),"an older save locks the gates and brings stray units home");}
    // The land of the sealed worlds feeds its own peoples and keeps its danger for outsiders.
    {var land=World.New(96);var ash=land.tiles.First(t=>t.realm==3&&t.terrain==17&&t.resource==0&&!t.farm&&!t.mine&&!t.road);ash.owner=-1;var fed=land.TileYield(ash);Check(fed[0]==3&&fed[1]==2&&fed[2]==1,"volcanic land feeds its native people");
     ash.owner=0;var outsider=land.TileYield(ash);Check(outsider[0]==0&&outsider[1]==3&&outsider[2]==0,"volcanic land stays hard on an outsider");ash.owner=-1;}
    // The Spore Circle's slow units (2 moves) can still leave a city ringed by bog, which costs others 3.
    {var slow=World.New(98);int spore=Enumerable.Range(0,slow.civCount).First(o=>slow.nations[o].civ==36);var sc=slow.cities.First(c=>c.owner==spore);var bog=slow.tiles.First(t=>t.realm==4&&t.terrain==9);var walker2=slow.Add(spore,2,sc.x,sc.y);Check(slow.StepCost(walker2,sc.x,sc.y,bog)<=slow.MaxMoves(walker2),"a slow people's units can always take a step");}
    // A sealed people behind the home world catches up on research and one far ahead slows; it cannot transcend.
    {var pace=World.New(97);int nativeOwner=Enumerable.Range(0,pace.civCount).First(pace.IsNativeNation);var n=pace.nations[nativeOwner];n.research=pace.AvailableTech(nativeOwner).First();int t0=n.research;n.progress[t0]=0;
     foreach(int o in Enumerable.Range(0,pace.civCount).Where(o=>!pace.IsNativeNation(o)))for(int i=0;i<20;i++)pace.nations[o].tech[i]=true;
     Check(pace.HomeTechPace()>=20,"home pace counts home civilisations only");int behind=pace.HomeTechPace()-n.tech.Count(a=>a);pace.KeepPace(nativeOwner);Check(behind>=World.PaceShare&&n.progress[t0]==pace.ResearchCost(t0,nativeOwner),"a people far behind gains a full technology's research in a turn");
     for(int i=0;i<60;i++)n.tech[i]=true;n.research=pace.AvailableTech(nativeOwner).First();int held=pace.ResearchCost(n.research,nativeOwner);n.progress[n.research]=held+5;pace.KeepPace(nativeOwner);Check(n.progress[n.research]==held-1,"a people far ahead cannot finish its next technology");
     var nc=pace.cities.First(c=>c.owner==nativeOwner);Check(pace.BuildReason(nc,70)!="","a sealed people cannot build Transcendence");}
}
}
}
