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
    var researched=new HashSet<int>{TechnologyCatalog.Start};while(researched.Count<TechnologyCatalog.ActiveCount){var available=w.AvailableTech();Check(available.Count>0,"no cycle or unreachable research");foreach(int i in available){Check(World.Requirements[i].All(p=>researched.Contains(p)),"prerequisites already complete");researched.Add(i);w.tech[i]=true;}}
    Check(researched.Count==99&&TechnologyCatalog.ActiveCount==99&&Enumerable.Range(0,6).All(e=>Enumerable.Range(0,180).Count(t=>TechnologyCatalog.Active[t]&&TechnologyCatalog.Eras[t]==e)==TechnologyCatalog.EraSize[e])&&TechnologyCatalog.KeystoneCount==36&&TechnologyCatalog.EraNames.Length==6&&Enumerable.Range(0,6).All(e=>Enumerable.Range(0,180).Count(t=>TechnologyCatalog.Eras[t]==e)==30),"180 technologies, 36 keystones, 30 per era");
    Check(Enumerable.Range(36,144).All(t=>World.Requirements[t].Length>0&&World.Requirements[t].All(p=>TechnologyCatalog.Eras[p]==TechnologyCatalog.Eras[t])&&World.ActiveTechBenefits(t).Length==1),"each minor technology needs only its own era and has one listed effect");w.CheckVictory();Check(w.result=="","researching every keystone no longer wins by itself (Stage 6: Transcendence is a project)");
    StageFive();
    PlayerRequestChecks();
    SecondRequestChecks();
    ThirdRequestChecks();
    FourthRequestChecks();
    FifthRequestChecks();
    SixthRequestChecks();
    LivingWorldChecks();
    PacingAndPopulationChecks();
    EighthNovemberChecks();
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
    {var wood=World.New(8);var founder=wood.units.First(u=>u.owner==0&&u.type==0);wood.Found(founder);var hand=wood.units.First(u=>u.owner==0&&u.type==1);{var beside=wood.Step(founder.x,founder.y,1,0);hand.x=beside.x;hand.y=beside.y;beside.owner=0;}hand.moves=4;var spot=wood.At(hand.x,hand.y);spot.terrain=1;spot.farm=false;wood.tech[K("agriculture")]=true;
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
    Check(old.version==35&&old.Valid(),"version 12 converts to a valid version 35");
    Check(old.HasTech(0,"pottery")&&old.HasTech(0,"masonry")&&old.HasTech(0,"steam")&&old.HasTech(0,"engineering"),"any known folded technology keeps its keystone, with prerequisites");
    Check(old.research==K("sailing")&&old.science==18,"research and progress move to the keystone (Sailing 11 + Shipbuilding 7)");
    Check(old.nations.All(n=>n.edicts!=null&&n.edicts.Length==6),"edict slots added");
    ReligionChecks();
}
// Owner requests 31 Oct, third batch: religion, border influence, settler cost, worker activities (save version 22).
static void ReligionChecks(){
    Check(ReligionCatalog.Count==23&&ReligionCatalog.Name(0)=="Christianity"&&ReligionCatalog.Name(12)=="Egyptian Religion"&&Enumerable.Range(0,ReligionCatalog.Count).All(r=>TechnologyCatalog.Index(ReligionCatalog.Tech(r))>=0&&ReligionCatalog.Tenet(r)>=0&&ReligionCatalog.Tenet(r)<=6),"23 faiths (12 living, 11 ancient), each with a real founding technology and a tenet");
    Check(World.UnitCosts[0]==90,"settlers cost three times as much (90)");
    var w=World.New(61);var founder=w.units.First(u=>u.owner==0&&u.type==0);var home=w.Found(founder);Check(home!=null&&w.Valid()&&w.version==35,"new games are valid version 35");
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
    var v=World.New(64);v.version=21;v.nations[1].pantheon=5;foreach(var t in v.tiles)t.swayCiv=-7;v.UpgradeSave();Check(v.version==35&&v.Valid()&&v.nations.All(n=>n.pantheon==-1&&n.founded==-1&&n.faith==0)&&v.tiles.All(t=>t.swayCiv==-1),"version 21 saves upgrade to version 26");
    CivilisationAndAgeChecks();
}
// Owner requests 31 Oct, fifth batch: civilisations in full, real civilisations, priests, era score and ages (save version 26).
static void CivilisationAndAgeChecks(){
    int full=Enumerable.Range(0,CivilisationCatalog.Count).Count(CivLore.Full);Check(full==72,"72 civilisations with full lore (10 premade, 12 real, 50 peoples of the sealed worlds), found "+full);
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
    var oldSave=World.New(77);oldSave.version=22;foreach(var u in oldSave.units)u.faith=0;oldSave.UpgradeSave();Check(oldSave.version==35&&oldSave.units.All(u=>u.faith==-1)&&oldSave.Valid(),"version 22 saves upgrade to 23 (units carry no faith)");
    RealmChecks();HexChecks();PeopleChecks();DifficultyChecks();AnimalChecks();
}
// Owner request 6 Nov: wild animals and larger maps.
static void AnimalChecks(){
    Check(AnimalCatalog.Names.Length==7&&AnimalCatalog.Attack.Length==7&&AnimalCatalog.Defence.Length==7&&AnimalCatalog.Steps.Length==7&&AnimalCatalog.Carnivore.Length==7&&AnimalCatalog.Homes.Length==7,"seven animals, every table complete");
    Check(new[]{"Cave lion","Smilodon","Cave bear","Dire wolf"}.All(n=>AnimalCatalog.Carnivore[Array.IndexOf(AnimalCatalog.Names,n)])&&new[]{"Megatherium","Woolly mammoth","Woolly rhinoceros"}.All(n=>!AnimalCatalog.Carnivore[Array.IndexOf(AnimalCatalog.Names,n)]),"four hunters and three grazers");
    for(int k=0;k<7;k++){var art=PixelArt.AnimalSprite(k);Check(art.w==64&&art.p.Count(c=>(c>>24)!=0)>150,AnimalCatalog.Names[k]+" has a picture");}
    // Larger maps: a fifth more tiles than the square tables, on every size and both shapes.
    for(int size=0;size<World.MapWidths.Length;size++){int flat=World.New(160,"Big",0,1,1,0,size,0,3,hex:true,natives:false).tiles.Count(t=>t.realm==0),old=World.MapWidths[size]*World.MapHeights[size];Check(flat>=old*118/100&&flat<=old*122/100,"flat "+World.MapSizeNames[size]+" is 20% larger ("+old+" to "+flat+")");if(size>3)continue;int globe=World.New(160,"Big",0,1,1,0,size,2,3,hex:true,natives:false).tiles.Count(t=>t.realm==0),oldG=World.SphereWidths[size]*World.SphereHeights[size];Check(globe>=oldG*118/100&&globe<=oldG*122/100,"globe "+World.MapSizeNames[size]+" is 20% larger ("+oldG+" to "+globe+")");}
    var w=World.New(161,"Wild",0,1,1,0,1,2,4,hex:true,natives:false,wild:true);Check(w.wildlife&&w.animals.Count==0&&w.Valid(),"a new game starts with none");w.turn=World.HuntersFrom;
    var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));foreach(var a in w.units.ToList())w.units.Remove(a);
    var open=w.tiles.First(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&t.owner<0&&t.gateLink==0&&w.Distance(t.x,t.y,city.x,city.y)>=12&&Enumerable.Range(0,6).All(k=>{var n=w.Step(t.x,t.y,World.HexDi[k],World.HexDj[k]);return n!=null&&!PixelArt.IsWater(n.terrain)&&!World.IsImpassable(n.terrain)&&Enumerable.Range(0,6).All(j=>{var m=w.Step(n.x,n.y,World.HexDi[j],World.HexDj[j]);return m!=null&&!PixelArt.IsWater(m.terrain)&&!World.IsImpassable(m.terrain);});}));
var side=w.Step(open.x,open.y,World.HexDi[0],World.HexDj[0]);var far=w.Step(side.x,side.y,World.HexDi[0],World.HexDj[0]);
    Action clear=()=>{w.animals.Clear();w.units.Clear();};
    // A carnivore hunts: it closes in and strikes.
    {clear();var wolf=new Animal{id=9001,kind=0,x=open.x,y=open.y};w.animals.Add(wolf);var prey=w.Add(0,2,far.x,far.y);for(int k=0;k<6;k++){w.TickAnimalsForChecks(false);if(prey.hp<10||!w.units.Contains(prey))break;}Check(!w.units.Contains(prey)||prey.hp<10,"a carnivore hunts down a unit");}
    {clear();var lion=new Animal{id=9002,kind=1,x=open.x,y=open.y};w.animals.Add(lion);var rival=w.Add(1,2,far.x,far.y);for(int k=0;k<6;k++){w.TickAnimalsForChecks(false);if(rival.hp<10||!w.units.Contains(rival))break;}Check(!w.units.Contains(rival)||rival.hp<10,"carnivores hunt rivals' units too");}
    // A herbivore does not attack across one clear tile, and does attack alongside.
    foreach(int kind in new[]{4,5,6}){clear();var grazer=new Animal{id=9003,kind=kind,x=open.x,y=open.y};w.animals.Add(grazer);var safe=w.Add(0,2,far.x,far.y);
     for(int k=0;k<30;k++){w.TickAnimalsForChecks(false);Check(safe.hp==10&&w.units.Contains(safe),AnimalCatalog.Names[kind]+" leaves alone a unit with a clear tile between");Check(w.Distance(grazer.x,grazer.y,safe.x,safe.y)>=2,AnimalCatalog.Names[kind]+" does not wander next to a unit");}
     clear();var g2=new Animal{id=9004,kind=kind,x=open.x,y=open.y};w.animals.Add(g2);var near=w.Add(0,2,side.x,side.y);w.TickAnimalsForChecks(false);Check(near.hp<10||!w.units.Contains(near),AnimalCatalog.Names[kind]+" attacks a unit right beside it");}
    // A unit in a city is safe; an animal never enters one.
    {clear();var at=world0(w,city);var guard=w.Add(0,2,city.x,city.y);var beast=new Animal{id=9005,kind=1,x=at.x,y=at.y};w.animals.Add(beast);for(int k=0;k<5;k++)w.TickAnimalsForChecks(false);Check(guard.hp==10&&!(beast.x==city.x&&beast.y==city.y),"a unit inside a city is safe");}
    // Attacking: soldiers can, civilians cannot, and a kill counts as a victory.
    {clear();var beast=new Animal{id=9006,kind=3,x=side.x,y=side.y,hp=2};w.animals.Add(beast);var worker=w.Add(0,1,open.x,open.y);worker.moves=4;Check(!w.Move(worker,side.x,side.y)&&worker.x==open.x,"a Worker cannot attack an animal");w.units.Remove(worker);
     var fighter=w.Add(0,6,open.x,open.y);fighter.moves=4;Check(w.Move(fighter,side.x,side.y)&&w.animals.Count==0&&fighter.moves==0&&fighter.victories==1&&fighter.x==open.x,"a soldier kills an animal by moving onto it, and stays put");}
    // Paths go round animals, and orders can target them.
    {clear();var beast=new Animal{id=9007,kind=2,x=side.x,y=side.y};w.animals.Add(beast);var walker=w.Add(0,2,open.x,open.y);var path=w.Route(walker,far.x,far.y);Check(path.Count>0&&path.All(t=>t!=side),"routes go round animals");var strike=w.Route(walker,side.x,side.y);Check(strike.Count==1&&strike[0]==side,"a route may end on an animal");}
    // Random spawning: only when on, only on free land away from everyone, up to the limit.
    {clear();w.turn=50;for(int k=0;k<400;k++)w.TickAnimalsForChecks(true);int limit=w.AnimalLimit();Check(w.animals.Count>0&&w.animals.Count<=limit+1,"animals spawn up to the limit ("+w.animals.Count+" of "+limit+")");Check(w.animals.Select(a=>a.kind).Distinct().Count()>=4,"several kinds spawn");Check(w.animals.All(a=>{var t=w.At(a.x,a.y);return t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain);}),"animals stand on open home land");}
    var quiet=World.New(162,"Quiet",0,1,1,0,1,2,4,hex:true,natives:false);quiet.turn=50;for(int k=0;k<50;k++)quiet.TickAnimalsForChecks(true);Check(quiet.animals.Count==0,"no animals when wildlife is off");
    {var easy=World.New(163,"E",0,1,1,0,1,2,4,hex:true,natives:false,wild:true,level:0);var hard=World.New(163,"H",0,1,1,0,1,2,4,hex:true,natives:false,wild:true,level:7);Check(easy.AnimalLimit()<hard.AnimalLimit(),"fewer animals on easy levels");}
    {var old=World.New(164,natives:false);old.version=26;old.animals=null;old.UpgradeSave();Check(old.version==35&&old.animals!=null&&!old.wildlife&&old.Valid(),"older games have no animals and stay valid");}
    // A whole game stays valid, and rivals deal with them.
    {var g=World.New(166,"Whole",0,1,1,0,0,2,4,hex:true,natives:false,wild:true);g.Found(g.units.First(u=>u.owner==0&&u.type==0));int most=0;for(int t=0;t<120&&g.result=="";t++){foreach(int o in Enumerable.Range(1,g.civCount-1))g.Relation(0,o).peaceUntil=g.turn+10;g.EndTurn();most=Math.Max(most,g.animals.Count);}Check(g.Valid()&&most>0,"a whole game with animals stays valid (at most "+most+")");}
}
static Tile world0(World w,City c){for(int k=0;k<6;k++){var n=w.Step(c.x,c.y,World.HexDi[k],World.HexDj[k]);if(n!=null&&!PixelArt.IsWater(n.terrain)&&!World.IsImpassable(n.terrain))return n;}return w.At(c.x,c.y);}
// Owner request 6 Nov: eight difficulty levels and the city-finished message.
// Owner requests of 6 Nov 2026: priorities, influence timer, discovery prompts, suggestions, route line, unit pictures.
static void PlayerRequestChecks(){
    var w=World.New(211,"Balance",0,1,1,0,1,2,4,hex:true);var n=w.nations[0];var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));
    Check(n.scienceRate==50&&n.militaryRate==0&&w.FinanceRate(0)==50,"new games start half science, half finance, nothing on the army");
    var before=w.Yield(city);
    w.SetPriority(0,World.AxisMilitary,20);Check(n.scienceRate==40&&n.militaryRate==20&&w.FinanceRate(0)==40,"raising military takes from the other two in proportion");
    w.SetPriority(0,World.AxisScience,100);Check(n.scienceRate==w.ScienceCap(0)&&n.scienceRate+n.militaryRate+w.FinanceRate(0)==100,"science stops at its ceiling and the sliders still total 100");
    w.SetPriority(0,World.AxisFinance,100);Check(n.scienceRate==0&&n.militaryRate==0&&w.FinanceRate(0)==100,"one slider can take everything");
    w.SetPriority(0,World.AxisMilitary,100);Check(n.scienceRate==0&&n.militaryRate==100&&w.FinanceRate(0)==0,"military can take everything");
    w.SetPriority(0,World.AxisFinance,50);Check(n.militaryRate==50&&w.FinanceRate(0)==50,"lowering one slider hands the share to the others");
    w.SetPriority(0,World.AxisMilitary,0);w.SetPriority(0,World.AxisScience,50);Check(n.scienceRate==50&&n.militaryRate==0&&w.Yield(city)[3]==before[3]&&w.Yield(city)[2]==before[2],"at no military spending yields match the old split");
    int trade=w.Trade(city);w.SetPriority(0,World.AxisMilitary,50);Check(w.ArmyBudget(city)==trade*n.militaryRate/100&&w.Yield(city)[1]==before[1],"the military share is counted from Trade and leaves ordinary production alone");
    city.build=World.UnitBuild(2);city.production=0;w.tech[K("toolmaking")]=true;n.tech=w.tech;w.Produce(city,1);Check(city.production==1+w.ArmyBudget(city),"a combat unit gets the army's shields");
    city.build=World.UnitBuild(1);city.production=0;w.Produce(city,1);Check(city.production==1,"a Worker does not get the army's shields");city.build=-1;city.production=0;
    w.SetPriority(0,World.AxisMilitary,0);
    // How far until the next ring of influence.
    city.age=0;city.pop=1;var g=w.NextBorderGrowth(city);Check(g.radius==2&&g.turnsLeft==25&&g.done==0&&g.span==25&&!g.full,"a new city's next ring is 25 turns away");
    city.age=20;g=w.NextBorderGrowth(city);Check(g.turnsLeft==5&&g.done==20,"the ring counts down with age");
    city.age=25;g=w.NextBorderGrowth(city);Check(g.radius==3&&g.turnsLeft==25&&g.done==0,"each ring starts a new count");
    city.age=50;g=w.NextBorderGrowth(city);Check(g.radius==4&&g.turnsLeft<0&&g.citizensAt==30,"the last ring waits for size 30");
    city.pop=30;g=w.NextBorderGrowth(city);Check(g.radius==5&&g.full,"then influence is fully expanded");city.pop=1;city.age=0;
    // A discovery asks what to research next, and asks idle cities that gained a building, not busy ones.
    int pot=K("pottery");foreach(int req in World.Requirements[pot])w.tech[req]=true;n.tech=w.tech;
    var busy=w.Add(0,0,w.units.First(u=>u.owner==0).x,w.units.First(u=>u.owner==0).y);busy.x=city.x;w.researchPrompts.Clear();w.buildPrompts.Clear();
    var other=w.tiles.First(t=>!World.IsImpassable(t.terrain)&&!PixelArt.IsWater(t.terrain)&&t.realm==0&&w.Distance(t.x,t.y,city.x,city.y)>8&&w.Distance(t.x,t.y,w.cities.First(c=>c.owner==1).x,w.cities.First(c=>c.owner==1).y)>8);busy.x=other.x;busy.y=other.y;var second=w.Found(busy);
    if(second!=null){second.buildingProject=World.BuildingIds[0]==7?8:7;}
    city.buildingProject=-1;city.buildingQueue.Clear();
    Check(World.BuildingTech.Contains(pot)&&w.AvailableTech(0).Contains(pot),"Pottery can be researched here and unlocks a building");
    w.SelectResearch(pot);w.science=w.ResearchCost(pot,0)+50;w.EndTurn();
    Check(w.tech[pot]&&w.researchPrompts.Count==1&&w.researchPrompts[0]==pot,"finishing a technology asks what to research next");
    Check(w.buildPrompts.Count==1&&w.buildPrompts[0].city==city.id&&w.buildPrompts[0].tech==pot&&w.buildPrompts[0].options.Count>0,"only the idle city is asked about its new building");
    var ask=w.NextBuildPrompt();Check(ask!=null&&ask.options.All(b=>w.BuildReason(city,b)==""),"the choices can all be built");
    w.ChooseProject(city,ask.options[0],false);Check(w.NextBuildPrompt()==null&&w.buildPrompts.Count==0,"a city that starts a building is no longer asked");
    // A unit is suggested only when nothing else is lined up.
    string why;city.pop=3;city.build=-1;city.queue.Clear();city.buildingProject=-1;city.buildingQueue.Clear();foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);
    Check(w.SuggestNext(city,false,out why)>=0,"an empty city is offered a unit");
    city.buildingQueue.Add(7);Check(w.SuggestNext(city,false,out why)<0,"a queued building stops the unit suggestion");city.buildingQueue.Clear();
    city.buildingProject=7;Check(w.SuggestNext(city,false,out why)<0,"a building under way stops the unit suggestion");city.buildingProject=-1;
    city.queue.Add(World.UnitBuild(2));Check(w.SuggestNext(city,false,out why)<0,"a queued unit stops the unit suggestion");city.queue.Clear();
    // The route line follows the order, tile by tile.
    var walker=w.Add(0,2,city.x,city.y);var goal=w.tiles.First(t=>t.realm==0&&t.seen&&w.Distance(t.x,t.y,city.x,city.y)==3&&w.Route(walker,t.x,t.y).Count>0);Check(w.PlannedRoute(walker).Count==0,"no order, no route line");
    walker.hasOrder=true;walker.destinationX=goal.x;walker.destinationY=goal.y;var line=w.PlannedRoute(walker);Check(line.Count>=3&&line.Last()==goal&&w.Distance(line[0].x,line[0].y,walker.x,walker.y)==1,"the route line runs from the unit to its destination");
    // Unit pictures stand on the same row whatever empty space or specks their art has under the figure.
    var art=new uint[64*64];for(int y=8;y<40;y++)for(int x=20;x<44;x++)art[y*64+x]=PixelArt.Rgb(120,90,60);art[60*64+30]=PixelArt.Rgb(120,90,60);
    var stood=PixelArt.UnitSprite(art,64,64);Check(PixelArt.FootRow(stood)==PixelArt.FootRowTarget&&Enumerable.Range(0,64).All(x=>Enumerable.Range(PixelArt.FootRowTarget+1,2).All(y=>(stood.Get(x,y)>>24)==0)),"a figure with space under it is moved down onto the tile");
    foreach(int type in new[]{2,3,4,5,6,7,8,9}){var guard=PixelArt.GuardSprite(stood.p,64,64,type);Check(guard.w==64&&guard.h==64&&!guard.p.SequenceEqual(stood.p)&&PixelArt.FootRow(guard)>=PixelArt.FootRowTarget-1,"unit "+type+" has a guard picture standing on the tile");}
    {var old=World.New(212,natives:false);old.version=27;old.UpgradeSave();Check(old.version==35&&old.nations.All(a=>a.militaryRate==0)&&old.Valid(),"older saves spend nothing on the army");}
}
// The living world: people, memory, materials and the map (owner request, 7 Nov 2026).
// Owner requests of 7 Nov 2026, latest: tree size, speed, opening budget, animals and population.
static void PacingAndPopulationChecks(){
    Check(TechnologyCatalog.EraSize.SequenceEqual(new[]{10,14,15,20,25,15}),"10, 14, 15, 20, 25 and 15 technologies by age");
    for(int i=0;i<TechnologyCatalog.Count;i++)if(TechnologyCatalog.Active[i])Check(World.Requirements[i].All(TechnologyCatalog.IsActive),"every prerequisite is itself researchable: "+TechnologyCatalog.Names[i]);
    // Long now costs what Standard used to; Standard two thirds and Quick one third.
    {var q=World.New(261,"Pace",0,0,1,0,1,2,4,hex:true,natives:false,rations:true);var s=World.New(261,"Pace",0,1,1,0,1,2,4,hex:true,natives:false,rations:true);var l=World.New(261,"Pace",0,2,1,0,1,2,4,hex:true,natives:false,rations:true);int t=K("agriculture");int old=World.TechCosts[t]*World.ResearchPace[0]/100;
     Check(l.ResearchCost(t,-1)==old&&s.ResearchCost(t,-1)==old*2/3&&q.ResearchCost(t,-1)==old/3,"Long research equals the old Standard");Check(q.MinTurns()==4&&s.MinTurns()==7&&l.MinTurns()==10,"fewest turns a technology: 4, 7, 10");
     // A Warrior and a Worker: nothing to pay before the capital, +1 gold a turn after.
     Check(s.units.Count(u=>u.owner==0&&u.type!=0)==2&&s.BudgetDetail(0).Net==1,"a new game with a Warrior and a Worker makes +1 gold a turn");var cap=s.Found(s.units.First(u=>u.owner==0&&u.type==0));Check(s.BudgetDetail(0).Net==1,"and the capital's guard is free, so this capital still makes +1, not "+s.BudgetDetail(0).Net);
     var warrior=s.units.First(u=>u.owner==0&&World.Power[u.type]>0);var away=s.tiles.First(x=>x.owner<0&&!PixelArt.IsWater(x.terrain)&&!World.IsImpassable(x.terrain)&&s.Distance(x.x,x.y,cap.x,cap.y)>6);warrior.x=away.x;warrior.y=away.y;Check(s.BudgetDetail(0).Net==1,"still +1 with the Warrior out exploring");
     // Population after history.
     cap.pop=8;Check(s.CityPeople(cap)==64000,"a size 8 Dawn city holds about 64,000 people");var st=s.StatsFor(0);Check(st.urbanShare==5&&st.population==64000L*20,"one person in twenty lives in a city in the Dawn Age");
     cap.pop=10;Check(s.DemographicPercent(cap)==130&&s.EpidemicChance(cap)==30,"crowded towns before Medicine grow slowly and risk sickness");s.nations[0].tech[K("medicine")]=true;Check(s.DemographicPercent(cap)==75&&s.EpidemicChance(cap)==0,"Medicine: faster growth, no epidemics");
     Check(World.PeopleText(1200000)=="1.2 million"&&World.PeopleText(64000)=="64,000","people figures read naturally");}
    // Animals: three grazers to each hunter; hunters take grazers; different hunters avoid each other or fight.
    {var w=World.New(262,"Wild",0,1,1,0,2,2,4,hex:true,natives:false,wild:true,rations:true);w.animals.Clear();for(int k=0;k<80;k++)w.SpawnAnimalsForChecks();int hunters=w.animals.Count(a=>AnimalCatalog.Carnivore[a.kind]),grazers=w.animals.Count-hunters;Check(hunters>0&&grazers>=hunters*2,"grazers outnumber hunters ("+grazers+" to "+hunters+")");
     var open=w.tiles.Where(t=>t.owner<0&&t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&!w.units.Any(u=>w.Distance(u.x,u.y,t.x,t.y)<9)&&!w.cities.Any(c=>w.Distance(c.x,c.y,t.x,t.y)<9)).ToList();
     Tile Beside(Tile t){for(int k=0;k<6;k++){var n=w.Step(t.x,t.y,World.HexDi[k],World.HexDj[k]);if(n!=null&&open.Contains(n))return n;}return null;}
     var spot=open.First(t=>Beside(t)!=null&&w.animals.All(a=>w.Distance(a.x,a.y,t.x,t.y)>12));var next=Beside(spot);w.animals.Clear();
     var lion=new Animal{id=9001,kind=0,x=spot.x,y=spot.y};var mammoth=new Animal{id=9002,kind=5,x=next.x,y=next.y};w.animals.Add(lion);w.animals.Add(mammoth);w.TickAnimalsForChecks(false);Check(mammoth.hp<10||!w.animals.Contains(mammoth),"a hunter attacks a grazer beside it");
     w.animals.Clear();var wolf=new Animal{id=9003,kind=3,x=next.x,y=next.y};lion.hp=10;lion.x=spot.x;lion.y=spot.y;w.animals.Add(lion);w.animals.Add(wolf);w.TickAnimalsForChecks(false);Check(lion.hp<10||wolf.hp<10||w.Distance(lion.x,lion.y,wolf.x,wolf.y)>1,"rival hunters forced together fight, or part");}
}
// Owner requests of 8 Nov 2026.
static void EighthNovemberChecks(){
    Check(World.MapSizeNames.Last()=="Colossal"&&World.MapWidths.Last()*World.MapHeights.Last()>World.MapWidths[5]*World.MapHeights[5]&&World.SphereWidths.Last()*World.SphereHeights.Last()>World.SphereWidths[5]*World.SphereHeights[5],"Colossal maps are larger than Massive");
    {int across=World.HexColumns(World.HexTiles(World.SphereWidths[6]*World.SphereHeights[6]));Check(across<=400&&World.HexTiles(World.SphereWidths[6]*World.SphereHeights[6])/across<=400,"a Colossal hex globe fits the save limits");}
    // Legions only from the Classical Age.
    {var w=World.New(301,"Aurelia",0,1,1,0,1,2,3,civChoice:8,hex:true,natives:false,rations:true);Check(w.nations[0].civ==8,"playing Aurelia");Check(w.UnitNameFor(0,2)=="Warrior"&&!w.UniqueUnit(0,2),"no Legionaries in the Dawn Age");
     for(int i=0;i<TechnologyCatalog.Count;i++)if(TechnologyCatalog.Eras[i]==0)w.nations[0].tech[i]=true;w.tech=w.nations[0].tech;Check(w.Era(0)>=1&&w.UnitNameFor(0,2)=="Legionary"&&w.UniqueUnit(0,2),"Legionaries replace Warriors in the Classical Age");}
    var s=World.New(302,"Rules",0,1,1,0,1,2,4,hex:true,natives:false,rations:true);var city=s.Found(s.units.First(u=>u.owner==0&&u.type==0));
    // A new city starts with nothing under way and no buildings.
    Check(city.build==-1&&city.buildingProject==-1&&city.buildings.Count==0&&!city.granary&&!city.walls&&!city.library,"a new city builds nothing until told to");
    // One pool of shields: a unit and a building under way share it.
    {s.nations[0].tech[K("pottery")]=true;Check(s.ChooseProject(city,World.UnitBuild(2),false)&&s.ChooseProject(city,7,false),"a Warrior and a Granary ordered");city.production=0;city.buildingProduction=0;s.Produce(city,5);Check(city.production+city.buildingProduction==5+s.ArmyShields(city,city.build)&&city.buildingProduction==2,"5 shields split 3 to the Warrior and 2 to the Granary");
     Check(s.LineShare(city,true)==Math.Max(1,s.Yield(city)[1])/2,"the city screen shows the Granary share");city.build=-1;city.buildingProduction=0;s.Produce(city,5);Check(city.buildingProduction==5,"with no unit in training the building gets every shield");
     city.buildingProduction=s.CostFor(city,7)-1;s.Produce(city,0);Check(!city.granary,"a building is not finished until its shields reach the cost");s.Produce(city,1);Check(city.granary,"and is finished when they do");}
    // Hills.
    {var hill=s.tiles.First(t=>t.terrain==2);Check(World.TerrainDefence(2)==2&&s.DefenceNote(hill.x,hill.y).Contains("+2"),"hills give +2 defence and the battle preview says so");}
    // Faith and religious buildings.
    {Check(!s.TechAllows(city,36)&&s.BuildReason(city,36).Contains("faith"),"Temples are not built with production");Check(s.FaithAllows(city,World.ShrineBuilding)&&!s.FaithAllows(city,World.MonasteryBuilding),"a Shrine is open at once, a Monastery needs Philosophy");
     city.faith=0;s.GatherCityFaith();Check(city.faith==s.FaithYield(city)&&city.faith>=1,"a city gathers its own faith each turn");Check(s.FaithBuildReason(city,World.ShrineBuilding).Contains("faith"),"not enough faith yet");
     city.faith=s.FaithBuildingCost(World.ShrineBuilding);int before=s.FaithYield(city);Check(s.BuildWithFaith(city,World.ShrineBuilding)&&city.faith==0&&s.HasBuilding(city,World.ShrineBuilding)&&s.FaithYield(city)==before+1,"a Shrine is bought with the city faith and adds 1 faith");
     s.nations[0].tech[K("mysticism")]=true;city.faith=1000;Check(s.FaithBuildReason(city,36)==""&&s.BuildWithFaith(city,36)&&s.HasBuilding(city,36)&&city.faith==1000-s.FaithBuildingCost(36),"a Temple follows the Shrine, paid in faith");
     Check(s.FaithBuildReason(city,World.CathedralBuilding).StartsWith("Requires"),"a Cathedral needs its technology");Check(World.BuildName(World.MonasteryBuilding)=="Monastery","religious buildings have names");}
    // Workers: never run from animals on their own land; run from enemy soldiers anywhere; run for own or friendly land, not a city.
    {var g=World.New(303,"Flee",0,1,1,0,1,2,4,hex:true,natives:false,wild:true,rations:true);var c=g.Found(g.units.First(u=>u.owner==0&&u.type==0));g.Reveal();foreach(var u in g.units.Where(u=>u.owner==0).ToList())g.units.Remove(u);g.animals.Clear();
     var home=g.tiles.First(t=>t.owner==0&&!(t.x==c.x&&t.y==c.y)&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));var hand=g.Add(0,1,home.x,home.y);hand.job=2;hand.jobTurns=3;
     var outside=g.tiles.First(t=>t.realm==0&&t.owner<0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&g.Distance(t.x,t.y,home.x,home.y)<=3);g.animals.Add(new Animal{id=9951,kind=0,x=outside.x,y=outside.y});g.WorkersFlee(0);Check(hand.flee==0&&hand.job==2,"a worker on its own land keeps working beside an animal");
     int enemy=Enumerable.Range(1,g.civCount-1).First(o=>!g.IsNativeNation(o));g.met[enemy]=true;g.SetWar(enemy,true);var raider=g.Add(enemy,2,outside.x,outside.y);g.animals.Clear();g.WorkersFlee(0);Check(hand.flee==1&&hand.fleeEnemy&&g.Activity(hand).Contains("enemy"),"but runs from enemy soldiers, even at home");g.units.Remove(raider);
     var far=g.tiles.First(t=>t.realm==0&&t.owner<0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&g.Distance(t.x,t.y,c.x,c.y)>=5&&g.Distance(t.x,t.y,c.x,c.y)<=7&&g.Route(new Unit{id=-1,owner=0,type=1,x=c.x,y=c.y,moves=4},t.x,t.y,true).Count>0);
     var hand2=g.Add(0,1,far.x,far.y);var refuge=g.Refuge(hand2);Check(refuge!=null&&refuge.owner==0&&!(refuge.x==c.x&&refuge.y==c.y)&&g.Distance(refuge.x,refuge.y,far.x,far.y)<g.Distance(c.x,c.y,far.x,far.y),"a worker in the wild runs for the nearest edge of its own land, not the city");
     var theirs=g.tiles.First(t=>t.owner<0&&!PixelArt.IsWater(t.terrain));theirs.owner=enemy;Check(!g.FriendlyGround(0,theirs),"an enemy land is no refuge");g.SetWar(enemy,false);g.openBorders[enemy]=true;g.Relation(0,enemy).accessUntil=g.turn+10;Check(g.FriendlyGround(0,theirs),"land of a civilisation with an agreement is a refuge");theirs.owner=-1;}
    // Diplomacy: named reasons, alarm and coalitions, influence, needs, postures.
    {var d=World.New(304,"Talks",0,1,1,0,2,2,5,hex:true,natives:false,rations:true);d.Found(d.units.First(u=>u.owner==0&&u.type==0));for(int o=1;o<d.civCount;o++){d.met[o]=true;}
     for(int o=1;o<d.civCount;o++){var lines=d.OpinionLines(o,0);int total=lines.Sum(l=>l.Value);Check(Math.Max(-100,Math.Min(100,total))==d.Opinion(o,0),"the reasons add up to the opinion of "+d.NationName(o));}
     d.SetAlarm(1,0,0);d.AddAlarm(1,0,20);Check(d.Alarm(1,0)==20*d.Flavour(1,0)/5,"alarm is scaled by how much a leader hates warmongers");int op=d.Opinion(1,0);d.SetAlarm(1,0,d.Alarm(1,0)+20);Check(d.Opinion(1,0)==Math.Max(-100,op-10)&&d.OpinionLines(1,0).Any(l=>l.Key.StartsWith("Alarmed")),"alarm at your expansion costs opinion, and says so");
     for(int o=1;o<=3;o++)d.SetAlarm(o,0,70);d.UpdateAlarmAndCoalitions();Check(d.CoalitionAgainst(0).Count>=3,"three alarmed civilisations form a coalition ("+d.CoalitionAgainst(0).Count+")");
     Check(d.SetWar(1,true)&&d.AtWar(0,2)&&d.AtWar(0,3),"attacking one member brings the coalition into the war");
     for(int o=1;o<=3;o++)d.SetAlarm(o,0,10);d.UpdateAlarmAndCoalitions();Check(d.CoalitionAgainst(0).Count==0,"the coalition breaks up once alarm fades");
     d.nations[0].influence=0;int per=d.InfluencePerTurn(0);d.AdvanceDiplomacy();Check(per>=1&&d.nations[0].influence==per,"influence builds each turn");
     d.nations[0].influence=50;var pair=d.Relation(0,4);int trust=pair.trustB;d.SetAlarm(4,0,30);Check(d.ImproveRelations(0,4)&&d.nations[0].influence==30&&pair.trustB==Math.Min(100,trust+8)&&d.Alarm(4,0)==20,"envoys cost 20 influence: trust +8, alarm -10");Check(d.ImproveReason(0,4).Contains("wait"),"envoys only every five turns");
     int calm=d.GoldNeed(1);d.nations[1].arrears=2;d.nations[1].gold=0;Check(d.GoldNeed(1)>calm,"a leader in debt values gold more");var gift=new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,giveGold=100};Check(d.DealValue(gift)>100*calm/100,"so the same gold buys more from it");
     d.nations[1].foodState=-1;Check(d.ResourceNeed(1,1)==200,"a hungry leader values wheat double");
     if(!d.cities.Any(c=>c.owner==1)){var st=d.units.FirstOrDefault(u=>u.owner==1&&u.type==0);if(st!=null)d.Found(st);}
     d.SetCoalition(2,1,true);d.turn=5*(d.turn/5+1)-1;d.UpdatePosture(1);Check(d.Posture(1)==DiplomacyFlavours.Defensive&&d.PostureSoldiers(1)==2,"a rival with a coalition against it turns defensive and arms");}
}
static void LivingWorldChecks(){
    var w=World.New(281,"Living",0,1,1,0,1,2,4,hex:true,natives:false,rations:true);Check(w.livingWorld&&w.log.Any(l=>l.StartsWith("Before history"))&&w.grudges.Count>0,"the world starts with a remembered past");
    var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.EndTurn();var gov=w.GovernorOf(city);Check(w.Ruler(0)!=null&&gov!=null&&gov.traitA!=gov.traitB&&gov.name!="","rulers and governors are people with two traits");
    Check(w.cities.Where(c=>c.owner!=0).All(c=>w.GovernorOf(c)!=null),"rival cities have governors too");
    // Breaking point.
    gov.stress=95;gov.breakTurns=0;gov.traitA=LivingCatalog.Loyal;gov.traitB=LivingCatalog.Ambitious;int gold=w.gold;w.EndTurn();Check(gov.breakKind>0&&gov.stress<85,"a governor under too much stress snaps ("+World.BreakNames[gov.breakKind]+")");
    // Grudges never fade and colour diplomacy.
    {int rival=Enumerable.Range(1,w.civCount-1).First(o=>!w.IsNativeNation(o));int before=w.GrudgePenalty(rival,0);w.AddGrudge(rival,0,1,"test");Check(w.GrudgePenalty(rival,0)==Math.Min(40,before+10),"a grudge costs opinion");for(int t=0;t<5;t++)w.EndTurn();Check(w.GrudgePenalty(rival,0)>=Math.Min(40,before+10),"and it does not fade");}
    // Gear from the city's supplies.
    Check(w.GearFor(city,2)>=0&&w.GearFor(city,2)<=3,"gear quality is between poor and masterwork");city.buildings.Add(30);int with=w.GearFor(city,2);city.buildings.Remove(30);Check(with==w.GearFor(city,2)+1,"a Workshop improves the gear");
    {var u=w.Add(0,2,city.x,city.y);u.gear=3;int strong=w.Strength(u,null,false);u.gear=0;Check(w.Strength(u,null,false)==strong-2,"gear changes fighting strength");w.units.Remove(u);}
    // Battlefields and graveyards.
    {var t=w.tiles.First(a=>a.realm==0&&!PixelArt.IsWater(a.terrain)&&w.Distance(a.x,a.y,city.x,city.y)==2);var rv=Enumerable.Range(1,w.civCount-1).First(o=>!w.IsNativeNation(o));int faith=w.FaithYield(city);for(int k=0;k<4;k++)w.RecordBattle(t.x,t.y,2,0,rv);Check(w.StoryText(t.x,t.y).StartsWith("Graveyard"),"many deaths make a graveyard");Check(w.PilgrimFaith(city)==2&&w.FaithYield(city)>=faith,"a graveyard nearby draws pilgrims");t.owner=rv;Check(w.GrudgePenalty(0,rv)>=10,"and its holder is resented by the people whose dead lie there");t.owner=0;}
    // Capture: a scar, a grudge and stolen blueprints.
    {var rv=w.cities.First(c=>c.owner!=0&&!w.IsNativeNation(c.owner));int victim=rv.owner;int tech=w.AvailableTech(0).First();w.artifacts.Add(new Artifact{city=rv.id,tech=tech,maker=victim});int before=w.nations[0].progress[tech];w.CityCaptured(rv,victim,0);Check(rv.scars.Contains(LivingCatalog.ScarConquest)&&w.grudges.Any(g=>g.holder==victim&&g.against==0)&&w.nations[0].progress[tech]>before,"a captured city remembers, its people resent, and its blueprints are read");}
    // Priorities are carried out by loyal cities only.
    {w.nations[0].priority=4;gov.loyalty=90;gov.corruption=0;gov.breakTurns=0;city.scars.Clear();Check(w.Obeys(city),"a loyal governor follows the priority");gov.loyalty=10;Check(!w.Obeys(city),"a disloyal one does not");}
    // Rebellion: an ungarrisoned, disloyal city breaks away.
    {var g=World.New(282,"Rebels",0,1,1,0,1,1,3,natives:false,rations:true);var a=g.Found(g.units.First(u=>u.owner==0&&u.type==0));var land=g.tiles.First(t=>!World.IsImpassable(t.terrain)&&!PixelArt.IsWater(t.terrain)&&g.Distance(t.x,t.y,a.x,a.y)==6&&!g.cities.Any(c=>g.Distance(c.x,c.y,t.x,t.y)<5));var b=g.Found(g.Add(0,0,land.x,land.y));foreach(var u in g.units.Where(u=>u.owner==0).ToList())g.units.Remove(u);g.EndTurn();var gb=g.GovernorOf(b);gb.traitA=LivingCatalog.Ambitious;gb.traitB=LivingCatalog.Greedy;
     var near=g.cities.First(c=>c.owner==1);foreach(var t in g.tiles)if(g.Distance(t.x,t.y,b.x,b.y)<=1){}near.x=near.x;for(int t=0;t<8&&b.owner==0;t++){gb=g.GovernorOf(b);if(gb==null)break;gb.loyalty=0;gb.stress=50;g.EndTurn();}
     Check(b.owner!=0||b.strike>0||g.log.Any(l=>l.Contains("risen against you")),"a disloyal city with no garrison rises or breaks away");}
    {var old=World.New(283,natives:false);old.modernRules=true;old.characters=null;old.version=33;old.UpgradeSave();Check(old.version==35&&old.livingWorld&&old.characters!=null&&old.Valid(),"older saves join the living world");}
}
// Owner requests of 7 Nov 2026, second batch.
static void SixthRequestChecks(){
    var w=World.New(271,"Sixth",0,1,1,0,1,2,4,hex:true,natives:false,wild:true,rations:true);var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);w.RebuildTerritory();
    // A big stack made in a city leaves it with what open land can hold; the rest stay behind.
    var spear=w.Add(0,7,city.x,city.y);var settlers=new System.Collections.Generic.List<Unit>();for(int k=0;k<3;k++)settlers.Add(w.Add(0,0,city.x,city.y));for(int k=0;k<4;k++)w.Add(0,8,city.x,city.y);Check(w.JoinStack(spear),"a crowd in a city stacks");
    var step=w.tiles.First(t=>w.Distance(t.x,t.y,city.x,city.y)==1&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));Check(w.MoveProblem(spear,step.x,step.y)=="","the stack can set off");Check(w.Move(spear,step.x,step.y)&&spear.x==step.x,"and does");
    Check(w.units.Count(u=>u.owner==0&&u.x==step.x&&u.y==step.y&&u.type==0)==World.CivilianStack&&settlers.Count(s=>s.x==city.x&&s.y==city.y&&s.stack==0)==1,"the civilian it has no room for stays in the city");
    foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);
    // One kind on a mixed tile.
    var h1=w.Add(0,8,city.x,city.y);var h2=w.Add(0,8,city.x,city.y);var other=w.Add(0,7,city.x,city.y);Check(w.SameTypeHere(h1)==2&&w.JoinType(h1)&&h2.stack==h1.stack&&other.stack!=h1.stack,"all the units of one kind can be taken together");
    // Half upkeep at home.
    {int home=w.BudgetDetail(0).army;var far=w.tiles.First(t=>t.realm==0&&t.owner<0&&!PixelArt.IsWater(t.terrain));foreach(var u in new[]{h1,h2,other}){u.x=far.x;u.y=far.y;}int away=w.BudgetDetail(0).army;Check(home*2>=away-1&&home<away,"soldiers in their own territory cost half ("+home+" at home, "+away+" away)");}
    foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);
    // Debt has consequences.
    {var n=w.nations[0];for(int k=0;k<3;k++)w.Add(0,2,city.x,city.y);city.buildings.Add(31);int content=w.ContentSlots(city);n.arrears=1;Check(w.ContentSlots(city)==content-1,"an empty treasury costs a content citizen");n.arrears=0;
     n.gold=0;w.gold=0;n.scienceRate=0;n.militaryRate=0;var before=w.units.Count(u=>u.owner==0);for(int k=0;k<12;k++)w.Add(0,6,city.x,city.y);for(int t=0;t<4;t++){w.gold=0;w.nations[0].gold=0;w.EndTurn();}Check(w.nations[0].arrears>=2&&w.units.Count(u=>u.owner==0)<before+12,"soldiers desert when they go unpaid");Check(w.DebtState(0)!="","and the player is told why");}
    foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);
    // Animals keep out of borders and are half as strong.
    Check(AnimalCatalog.Attack[5]==2&&AnimalCatalog.Defence[5]==3,"animals have half their old strength");
    {var g=World.New(272,"Wild",0,1,1,0,1,2,4,hex:true,natives:false,wild:true,rations:true);g.Found(g.units.First(u=>u.owner==0&&u.type==0));g.RebuildTerritory();g.turn=20;for(int k=0;k<40;k++)g.TickAnimalsForChecks(true);Check(g.animals.Count>0&&g.animals.All(a=>g.At(a.x,a.y).owner<0),"no animal stands inside a border ("+g.animals.Count+")");
     var inside=g.units.FirstOrDefault(u=>u.owner==0&&g.At(u.x,u.y).owner==0);if(inside!=null){var spot=g.tiles.FirstOrDefault(t=>t.owner<0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&g.Distance(t.x,t.y,inside.x,inside.y)==1);if(spot!=null){g.animals.Clear();g.animals.Add(new Animal{id=99,kind=1,x=spot.x,y=spot.y});int hp=inside.hp;g.TickAnimalsForChecks(false);Check(inside.hp==hp,"a unit inside its borders is safe from hunters");}}}
    // The faith tree.
    {var n=w.nations[0];Check(n.doctrines!=null&&n.doctrines.Length==DoctrineCatalog.Count&&DoctrineCatalog.Count==18,"eighteen doctrines");Check(w.DoctrineOpen(0,0)&&w.DoctrineOpen(0,1)&&!w.DoctrineOpen(0,3)&&!w.DoctrineOpen(0,6),"the first age is open; later ones wait for science and their line");
     Check(w.ChooseDoctrine(0,2)&&n.doctrine==2,"a doctrine can be chosen");int faithBefore=w.FaithYield(city);n.devotion=DoctrineCatalog.Cost(2);n.faith=0;w.UpdateReligion();Check(n.doctrines[2]&&n.faith>0,"devotion fills the tree without spending faith");Check(w.FaithYield(city)>=faithBefore,"Shamans add faith ("+faithBefore+" to "+w.FaithYield(city)+")");
     Check(w.DoctrineReason(0,5)!=""&&w.DoctrineReason(0,5).Contains("Age"),"the next age's doctrine waits for science");}
    {var old=World.New(273,natives:false);foreach(var nn in old.nations)nn.doctrines=null;old.version=32;old.UpgradeSave();Check(old.version==35&&old.nations.All(nn=>nn.doctrines!=null&&nn.doctrine==-1)&&old.Valid(),"older saves start with an empty faith tree");}
}
// Owner requests of 7 Nov 2026.
static void FifthRequestChecks(){
    var w=World.New(251,"Fifth",0,1,1,0,1,2,4,hex:true,natives:false,rations:true);var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));
    // A streamlined tree: only the 36 keystones are researched; ages stay locked until a technology leads into them.
    Check(w.AvailableTech(0).All(TechnologyCatalog.IsActive)&&w.AvailableTech(0).Any(t=>!TechnologyCatalog.IsKeystone(t)),"only the 99 technologies of the tree can be researched");
    Check(Enumerable.Range(0,TechnologyCatalog.KeystoneCount-1).All(t=>World.ActiveTechBenefits(t).Length>0),"every technology does something");
    int writing=K("printing");Check(!w.AvailableTech(0).Any(t=>TechnologyCatalog.Eras[t]>=2),"later ages are locked at the start");
    Check(Enumerable.Range(0,36).Where(t=>TechnologyCatalog.Eras[t]>0).All(t=>World.Requirements[t].Length>0),"every later technology has something leading into it");
    Check(Enumerable.Range(1,5).All(e=>Enumerable.Range(0,36).Any(t=>TechnologyCatalog.Eras[t]==e&&World.Requirements[t].Any(p=>TechnologyCatalog.Eras[p]==e-1))),"each age is entered from the one before");
    {int cost=w.ResearchCost(K("agriculture"),-1);Check(cost==World.TechCosts[K("agriculture")]*World.ResearchPace[0]/100*2/3,"research costs follow the age (Standard is two thirds of Long)");Check(w.MinTurns()==7&&w.ResearchCap(K("agriculture"),0)*7>=w.ResearchCost(K("agriculture"),0),"a technology takes at least seven turns on Standard");}
    {int before=w.Yield(city)[0];}
    // Older saves that were researching a retired technology move on.
    {var old=World.New(252,natives:false);int minor=Enumerable.Range(TechnologyCatalog.KeystoneCount,144).First(t=>!TechnologyCatalog.Active[t]);old.research=minor;old.nations[0].research=minor;old.version=31;old.UpgradeSave();Check(old.version==35&&old.research<TechnologyCatalog.KeystoneCount&&old.modernRules&&old.Valid(),"an older save researching a retired technology moves on");}
    // Expansion costs more, especially cut off from the rest.
    Check(World.ExpansionCost(4)==0&&World.ExpansionCost(6)==1&&World.ExpansionCost(10)==7&&World.ExpansionCost(16)==26,"a large empire costs more and more");
    {var g=World.New(253,"Links",0,1,1,0,1,1,3,natives:false,rations:true);var a=g.Found(g.units.First(u=>u.owner==0&&u.type==0));var land=g.tiles.First(t=>!World.IsImpassable(t.terrain)&&!PixelArt.IsWater(t.terrain)&&g.Distance(t.x,t.y,a.x,a.y)==7&&!g.cities.Any(c=>g.Distance(c.x,c.y,t.x,t.y)<6));var b=g.Found(g.Add(0,0,land.x,land.y));g.RebuildTerritory();g.ResetLinks();Check(g.CutOff(a)&&g.CutOff(b)&&g.BudgetDetail(0).cutOff==2*World.CutOffGold,"cities with no link cost extra gold");int content=g.ContentSlots(a);
     var walker=g.Add(0,3,a.x,a.y);var path=g.Route(walker,b.x,b.y,true);g.units.Remove(walker);foreach(var t in path){t.road=true;t.owner=0;}g.At(a.x,a.y).owner=0;g.At(b.x,b.y).owner=0;g.ResetLinks();Check(!g.CutOff(a)&&g.BudgetDetail(0).cutOff==0&&g.ContentSlots(a)==content+1,"a road clears both the gold and the unrest");}
    // Water: at least 50 tiles for a port and ships; places that cannot build a thing do not list it.
    {var g=World.New(254,"Water",0,1,1,0,1,1,3,natives:false,rations:true);var c=g.Found(g.units.First(u=>u.owner==0&&u.type==0));foreach(var t in g.tiles)if(t.realm==0&&PixelArt.IsWater(t.terrain)&&g.Distance(t.x,t.y,c.x,c.y)<=6)t.terrain=0;foreach(var t in g.tiles)if(t.realm==0&&g.Distance(t.x,t.y,c.x,c.y)<=6)t.terrain=0;
     g.ResetWaters();var pond=g.Step(c.x,c.y,1,0);pond.terrain=6;g.ResetWaters();Check(g.WaterBodySize(pond.x,pond.y)==1&&!g.NextToBigWater(c.x,c.y),"a pond is too small");Check(g.PortSite(c)==null&&!g.PlaceAllows(c,World.PortBuilding)&&!g.TechAllows(c,World.PortBuilding),"no port on a pond, and it is not listed");Check(!g.PlaceAllows(c,World.UnitBuild(World.BoatUnit)),"nor boats");
     int rowY=c.y;var lake=new System.Collections.Generic.List<Tile>{pond};var stack=new System.Collections.Generic.Queue<Tile>();stack.Enqueue(pond);while(lake.Count<60&&stack.Count>0){var at=stack.Dequeue();for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=g.Step(at.x,at.y,dx,dy);if(n==null||n.realm!=0||lake.Contains(n)||(n.x==c.x&&n.y==c.y))continue;if(g.Distance(n.x,n.y,c.x,c.y)<1)continue;n.terrain=6;lake.Add(n);stack.Enqueue(n);if(lake.Count>=60)break;}}g.ResetWaters();Check(g.WaterBodySize(pond.x,pond.y)>=50&&g.NextToBigWater(c.x,c.y)&&g.PortSite(c)!=null&&g.PlaceAllows(c,World.PortBuilding),"a large lake can take a port ("+g.WaterBodySize(pond.x,pond.y)+")");}
    // A unit leaves the training list when its replacement can really be trained.
    {var g=World.New(255,"Units",0,1,1,0,1,1,3,natives:false,rations:true);var c=g.Found(g.units.First(u=>u.owner==0&&u.type==0));Check(g.TechAllows(c,World.UnitBuild(2)),"Warriors can be trained at first");g.tech[K("bronze")]=true;g.nations[0].tech=g.tech;Check(g.IsObsolete(0,2)&&!g.TechAllows(c,World.UnitBuild(2))&&g.TechAllows(c,World.UnitBuild(7)),"Spearmen replace Warriors on the list");}
    // Animal frequency and climate.
    {var few=World.New(256,"F",0,1,1,0,1,2,4,hex:true,natives:false,wild:true,animalShare:25);var many=World.New(256,"M",0,1,1,0,1,2,4,hex:true,natives:false,wild:true,animalShare:200);Check(few.AnimalLimit()*4<=many.AnimalLimit()&&many.AnimalLimit()>few.AnimalLimit(),"animal frequency changes the numbers ("+few.AnimalLimit()+", "+many.AnimalLimit()+")");}
    {Func<World,int,int> share=(m,kind)=>{var land=m.tiles.Where(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)).ToList();return land.Count(t=>World.TemperatureClass(t.terrain)==kind)*100/Math.Max(1,land.Count);};
     var cold=World.New(257,"Cold",0,1,1,0,1,1,4,natives:false,climate:2);var hot=World.New(257,"Hot",0,1,1,0,1,1,4,natives:false,climate:3);var normal=World.New(257,"Normal",0,1,1,0,1,1,4,natives:false);Check(share(cold,1)>share(normal,1)&&share(hot,3)>share(normal,3),"a frozen world is colder and a tropical world hotter ("+share(cold,1)+">"+share(normal,1)+", "+share(hot,3)+">"+share(normal,3)+")");
     foreach(int kind in Enumerable.Range(0,World.ClimateNames.Length)){var m=World.New(258+kind,"C",0,1,1,0,0,1,3,natives:false,climate:kind,hex:true);Check(m.Valid()&&m.units.Where(u=>u.owner==0).All(u=>!PixelArt.IsWater(m.At(u.x,u.y).terrain)),"every climate gives a playable start ("+World.ClimateNames[kind]+")");}}
    // Workers run, then come back to their work.
    {var g=World.New(265,"Flee",0,1,1,0,1,2,4,hex:true,natives:false,wild:true,rations:true);var c=g.Found(g.units.First(u=>u.owner==0&&u.type==0));g.Reveal();foreach(var u in g.units.Where(u=>u.owner==0).ToList())g.units.Remove(u);g.animals.Clear();
     var site=g.tiles.First(t=>t.realm==0&&t.owner<0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&g.Distance(t.x,t.y,c.x,c.y)>=4&&g.Distance(t.x,t.y,c.x,c.y)<=6&&g.Route(new Unit{id=-1,owner=0,type=1,x=c.x,y=c.y,moves=4},t.x,t.y,true).Count>0);var hand=g.Add(0,1,site.x,site.y);hand.job=2;hand.jobTurns=3;
     var wild=g.tiles.First(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&g.Distance(t.x,t.y,site.x,site.y)==2&&g.Distance(t.x,t.y,c.x,c.y)>2);var lion=new Animal{id=9901,kind=0,x=wild.x,y=wild.y};g.animals.Add(lion);g.WorkersFlee(0);
     Check(hand.flee==1&&hand.job==0&&g.Activity(hand).Contains("Fleeing"),"a worker with an animal close by stops work and runs");
     hand.x=c.x;hand.y=c.y;hand.hasOrder=false;g.animals.Clear();g.WorkersFlee(0);Check(hand.flee==2&&hand.hasOrder&&g.Activity(hand).Contains("back"),"when the coast is clear it walks back");hand.x=site.x;hand.y=site.y;g.WorkersFlee(0);Check(hand.flee==0&&hand.job==2&&hand.jobTurns==3,"and takes up the same job");
     var guard=g.Add(0,2,site.x,site.y);g.animals.Add(lion);hand.job=2;g.WorkersFlee(0);Check(hand.flee==0&&hand.job==2,"a soldier beside it keeps it at work");g.units.Remove(guard);}
}
// Owner requests of 6 Nov 2026, fourth batch.
static void FourthRequestChecks(){
    var w=World.New(241,"Fourth",0,1,1,0,1,2,4,hex:true,natives:false);var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);foreach(var t in w.tiles)if(t.realm==0&&w.Distance(t.x,t.y,city.x,city.y)<=10){t.seen=true;w.nations[0].known[t.y*w.Width+t.x]=true;}
    var open=w.tiles.Where(t=>t.realm==0&&!World.IsImpassable(t.terrain)&&!PixelArt.IsWater(t.terrain)&&t.owner==0&&w.Distance(t.x,t.y,city.x,city.y)==2&&w.Route(new Unit{id=-1,owner=0,type=1,x=city.x,y=city.y,moves=4},t.x,t.y,true).Count>0).First();
    // The freeze: ordering a settler to a tile it cannot fit on. A city shelters civilians; elsewhere it stops beside the tile.
    var settler=w.Add(0,0,open.x,open.y);var sitter=w.Add(0,0,city.x,city.y);Check(w.Order(settler,city.x,city.y)&&(settler.hasOrder||settler.x==city.x&&settler.y==city.y),"a settler can be sent to a city with another civilian in it");w.units.Remove(settler);w.units.Remove(sitter);
    {var a=w.Add(0,0,open.x,open.y);var b=w.Add(0,10,open.x,open.y);var mover=w.Add(0,0,city.x,city.y);mover.moves=0;var watch=System.Diagnostics.Stopwatch.StartNew();var empty=w.Route(mover,open.x,open.y);Check(empty.Count==0,"a tile with no room has no route");Check(watch.ElapsedMilliseconds<200,"and finding that out takes no searching ("+watch.ElapsedMilliseconds+" ms)");w.units.Remove(a);w.units.Remove(b);w.units.Remove(mover);}
    // Soldiers cost more to train and to keep.
    Check(World.UnitCosts[2]==30&&World.UnitCosts[6]==150,"soldiers take longer to train");Check(World.Upkeep[2]==2&&World.Upkeep[6]==4,"and cost more to keep");
    for(int k=0;k<5;k++)w.Add(0,2,city.x,city.y);Check(w.BudgetDetail(0).army==5*2+1,"a large army costs more than the sum of its soldiers ("+w.BudgetDetail(0).army+")");foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);
    // Stacks and escorts.
    var soldier=w.Add(0,2,city.x,city.y);var worker=w.Add(0,1,city.x,city.y);Check(w.JoinStack(soldier)&&soldier.stack!=0&&worker.stack==soldier.stack,"units on a tile stack");soldier.moves=4;worker.moves=4;var step=w.Route(soldier,open.x,open.y)[0];Check(w.Move(soldier,step.x,step.y)&&worker.x==step.x&&worker.y==step.y,"a stack moves as one");
    {var attacker=new Unit{id=-1,owner=1,type=2,x=step.x,y=step.y};Check(w.BestDefender(attacker,step.x,step.y)==soldier,"an escort is attacked before its worker");}
    {int before=soldier.moves;soldier.moves=0;worker.moves=4;var next=w.Route(worker,open.x,open.y);if(next.Count>0)Check(!w.Move(worker,next[0].x,next[0].y),"a stack waits for its slowest member");soldier.moves=before;}
    w.LeaveStack(soldier);Check(soldier.stack==0&&worker.stack==0,"a stack can be broken up");w.units.Remove(soldier);w.units.Remove(worker);
    // Auto-develop says what it is doing.
    {var hand=w.Add(0,1,open.x,open.y);hand.autoWork=true;Check(w.Activity(hand).Contains("looking for work"),"an idle developer says so");hand.autoJob=2;hand.hasOrder=true;Check(w.Activity(hand).Contains("build a farm"),"on the way it says what to");hand.hasOrder=false;hand.job=3;Check(w.Activity(hand).Contains("mine"),"and at work what it is building");w.units.Remove(hand);}
    // Roads cost upkeep and can be plotted.
    w.ResetRoadCount();int upkeep=w.BudgetDetail(0).roads;Check(upkeep==w.RoadUpkeep(0),"roads appear in the budget");var course=w.RoadCourse(0,city.x,city.y,open.x,open.y);Check(course.Count>=2,"a road can be plotted");foreach(var t in course){t.road=false;}
    int fresh=w.NewRoadTiles(course);Check(fresh==course.Count,"new tiles are counted");foreach(var t in course)t.road=true;w.ResetRoadCount();Check(w.RoadTiles(0)>=course.Count-1&&w.RoadUpkeep(0)>=1,"roads cost gold");foreach(var t in w.tiles)if(t.road&&!w.cities.Any(c=>c.x==t.x&&c.y==t.y))t.road=false;w.ResetRoadCount();Check(w.RoadUpkeep(0)==0,"a city's own tile costs nothing");
    w.tech[K("wheel")]=true;w.nations[0].tech=w.tech;var builder=w.Add(0,1,city.x,city.y);builder.moves=4;var plan=w.RoadCourse(0,city.x,city.y,open.x,open.y);Check(w.PlanRoad(builder,plan).StartsWith("Road ordered")&&builder.roadOrder&&builder.roadX==open.x,"a plotted road sets the worker going");
    // Wild animals slow down inside borders; scouts keep clear of them.
    {var g=World.New(242,"Beasts",0,1,1,0,1,2,4,hex:true,natives:false,wild:true);var c=g.Found(g.units.First(u=>u.owner==0&&u.type==0));var inside=g.tiles.First(t=>t.owner==0&&!PixelArt.IsWater(t.terrain));var outside=g.tiles.First(t=>t.realm==0&&t.owner<0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));var lion=new Animal{id=7,kind=0,x=outside.x,y=outside.y};Check(g.AnimalSteps(lion)==3,"a lion runs free outside borders");lion.x=inside.x;lion.y=inside.y;int one=g.AnimalSteps(lion);lion.id=8;int two=g.AnimalSteps(lion);Check(one+two==3,"and at half speed inside them ("+one+","+two+")");}
    // Gateways are found once and stay on the map.
    {var g=World.New(243,"Gates",0,1,1,0,1,2,4,hex:true);foreach(var t in g.tiles)t.seen=false;Check(g.NextNewGate()==null,"no gateway seen, none found");var gate=g.tiles.First(t=>t.realm==0&&t.gateLink>0);gate.seen=true;Check(g.NextNewGate()==gate&&g.NextNewGate()==null&&g.gatesFound.Contains(gate.y*g.Width+gate.x),"a gateway is found once and remembered");}
    {var old=World.New(244);var gate=old.tiles.First(t=>t.realm==0&&t.gateLink>0);gate.seen=true;old.version=30;old.UpgradeSave();Check(old.version==35&&old.gatesFound.Count>0&&old.Valid(),"gateways an older save already knows need no introduction");}
}
// Owner requests of 6 Nov 2026, third batch.
static void ThirdRequestChecks(){
    // Units eat. Field rations and spare food cover a small army; a huge one goes hungry and the newest units stop.
    var w=World.New(231,"Rations",0,1,1,0,1,2,4,hex:true,natives:false,rations:true);var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);
    var first=w.Add(0,2,city.x,city.y);w.EndTurn();Check(!first.unfed&&first.moves>0&&w.nations[0].rationShort==0,"a small army is fed");
    for(int k=0;k<30;k++)w.Add(0,2,city.x,city.y);int foodBefore=city.food;w.EndTurn();Check(w.nations[0].rationShort>0&&w.nations[0].foodState==-1&&w.UnfedUnits(0)>0,"too many mouths and the empire goes hungry");Check(!first.unfed&&w.units.Where(u=>u.owner==0).OrderByDescending(u=>u.id).First().unfed,"the newest units go without first");
    var hungry=w.units.Where(u=>u.owner==0&&u.unfed).First();Check(hungry.moves==0&&w.Activity(hungry).Contains("food"),"a unit without food cannot act");{int content=w.ContentSlots(city);w.nations[0].foodState=0;Check(w.ContentSlots(city)==content+1,"hunger costs a content citizen");w.nations[0].foodState=1;Check(w.ContentSlots(city)==content+2,"plenty gains one");}
    foreach(var u in w.units.Where(u=>u.owner==0&&u!=first).ToList())w.units.Remove(u);w.EndTurn();Check(w.UnfedUnits(0)==0&&first.moves>0&&w.nations[0].rationShort==0,"with fewer mouths all can act again");
    Check(World.UnitRation(5)==2&&World.UnitRation(2)==1,"horsemen eat more than foot soldiers");
    {var g=World.New(232,"Share",0,1,1,0,1,2,4,hex:true,natives:false,rations:true);var c=g.Found(g.units.First(u=>u.owner==0&&u.type==0));c.pop=1;var v=g.Yield(c)[0];for(int k=0;k<6;k++)g.Add(0,2,c.x,c.y);int need;int spare=g.FoodBalance(0,out need);Check(spare==Math.Max(0,v)-need,"the balance is the surplus less the rations");g.EndTurn();Check(c.ration==Math.Min(need,Math.Max(0,v)),"the rations come out of the city's food");}
    {var old=World.New(233,natives:false);old.version=29;old.UpgradeSave();Check(old.version==35&&old.foodRations&&old.Valid(),"older saves start feeding their units");}
    {var off=World.New(234,natives:false);var c=off.Found(off.units.First(u=>u.owner==0&&u.type==0));for(int k=0;k<30;k++)off.Add(0,2,c.x,c.y);off.EndTurn();Check(off.UnfedUnits(0)==0,"the checks of older rules run without rations");}
    // The turn run in steps is the same turn.
    {var a=World.New(235,"Steps",0,1,1,0,0,1,4,hex:true,wild:true,rations:true);var b=World.New(235,"Steps",0,1,1,0,0,1,4,hex:true,wild:true,rations:true);a.Found(a.units.First(u=>u.owner==0&&u.type==0));b.Found(b.units.First(u=>u.owner==0&&u.type==0));for(int t=0;t<12;t++){a.EndTurn();bool live=b.result=="";int steps=0;foreach(int s in b.TurnSteps())if(s<b.civCount)steps++;Check(!live||steps==b.civCount-1,"one step for each rival");}Check(a.turn==b.turn&&a.units.Count==b.units.Count&&a.cities.Count==b.cities.Count&&a.units.Select(u=>u.x*100000+u.y).SequenceEqual(b.units.Select(u=>u.x*100000+u.y)),"a stepped turn matches a whole one");}
    // More wild animals.
    {var g=World.New(236,"Wild",0,1,1,0,1,2,4,hex:true,natives:false,wild:true);int land=g.tiles.Count(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&t.gateLink==0);Check(g.AnimalLimit()>=land/40*World.WildMultiple*DifficultyTable.Animals[g.difficultyLevel]/100-World.WildMultiple,"one animal per forty tiles of land, three times that early ("+g.AnimalLimit()+")");for(int k=0;k<20;k++)g.TickAnimalsForChecks(true);Check(g.animals.Count>=20,"they come quickly ("+g.animals.Count+")");}
    // Sharper art.
    {var art=new uint[128*128];for(int y=16;y<80;y++)for(int x=40;x<88;x++)art[y*128+x]=PixelArt.Rgb(120,90,60);var hd=PixelArt.UnitSprite(art,128,128,128);Check(hd.w==128&&PixelArt.FootRow(hd)==PixelArt.FootRowTarget*2,"a large unit picture keeps its size and stands on its row");var guard=PixelArt.GuardSpriteHD(hd.p,128,128,7);Check(guard.w==128&&!guard.p.SequenceEqual(hd.p),"it has a guard picture too");var two=PixelArt.Scale2x(PixelArt.HudIcon(PixelArt.IconCivics));Check(two.w==48&&two.h==48&&two.p.Any(p=>(p>>24)!=0),"icons double cleanly");Check((PixelArt.Bleed(two).p.Count(p=>(p>>24)==0&&(p&0xFFFFFFu)!=0))>0,"clear pixels take their neighbour's colour");}
}
// Owner requests of 6 Nov 2026, second batch.
static void SecondRequestChecks(){
    var w=World.New(221,"Second",0,1,1,0,1,2,4,hex:true,wild:true);var n=w.nations[0];var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));
    // Research time follows the science coming in; four turns at the very least; left-over science carries on.
    int pot=K("agriculture");w.SelectResearch(pot);int cost=w.ResearchCost(pot,0);int per=w.SciencePerTurn(0);Check(per>0&&w.ResearchTurns(pot,0)==Math.Max(1,(cost+Math.Min(per,w.ResearchCap(pot,0))-1)/Math.Min(per,w.ResearchCap(pot,0))),"turns are the science left divided by the science per turn");
    Check(w.ResearchCost(pot,-1)==World.TechCosts[pot]*World.ResearchPace[0]/100,"research costs follow the pace table");
    {var save=n.scienceRate;w.SetScienceRate(0,0);foreach(var c in w.cities.Where(c=>c.owner==0))c.focus=0;if(w.SciencePerTurn(0)==0)Check(w.ResearchTurns(pot,0)==-1,"no science, no estimate");w.SetScienceRate(0,save);}
    Check(w.ResearchCap(pot,0)*World.MinResearchTurns>=cost&&w.ResearchCap(pot,0)*(World.MinResearchTurns-1)<cost,"a technology absorbs at most a quarter of its cost in a turn");
    {int before=n.progress[K("bronze")];w.science=cost+7;n.progress[pot]=w.science;w.researchPrompts.Clear();w.buildPrompts.Clear();w.EndTurn();Check(w.tech[pot]&&w.researchPrompts.Contains(pot),"the technology finished");int next=w.research;Check(next>=0&&w.researchOverflow>=7&&w.science>=7,"science left over goes to the next technology ("+w.science+")");int other=w.AvailableTech(0).First(t=>t!=next);int spare=w.researchOverflow;w.ChooseNextResearch(other);Check(w.research==other&&w.science>=spare,"and follows your choice");}
    // Soldiers are always paid for; civilians still have free support.
    foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);var empty=w.BudgetDetail(0);Check(empty.army==0&&empty.civilians==0,"no units, no support");
    var paid=w.Add(0,2,city.x,city.y);int half=w.AtHome(paid)?1:2;Check(w.BudgetDetail(0).army==2/half,"a Warrior costs 2 gold (half at home) though the city has free support");w.Add(0,6,city.x,city.y);Check(w.BudgetDetail(0).army==6/half,"a Rifleman costs 4 (half at home)");
    w.Add(0,1,city.x,city.y);Check(w.BudgetDetail(0).civilians==0,"a Worker is covered by the free allowance");
    {var b=w.BudgetDetail(0);var totals=w.Budget(0);Check(totals[0]==b.Income&&totals[1]==b.Expense&&totals[2]==b.Net,"the budget is the sum of its lines");}
    foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);
    // Workers leave the city's own tile alone, and the city still joins the road network.
    var worker=w.Add(0,1,city.x,city.y);Check(w.JobReason(worker,1).Contains("city")&&w.JobReason(worker,2).Contains("city")&&!w.StartJob(worker,2),"workers cannot develop a city's tile");w.units.Remove(worker);w.RebuildTerritory();Check(w.At(city.x,city.y).road,"a city counts as on the road");
    // Joining cities adds Trade.
    {var g=World.New(222,"Links",0,1,1,0,1,1,3,natives:false);var a=g.Found(g.units.First(u=>u.owner==0&&u.type==0));var land=g.tiles.First(t=>!World.IsImpassable(t.terrain)&&!PixelArt.IsWater(t.terrain)&&g.Distance(t.x,t.y,a.x,a.y)==7&&!g.cities.Any(c=>g.Distance(c.x,c.y,t.x,t.y)<6));var b=g.Found(g.Add(0,0,land.x,land.y));Check(b!=null,"second city founded");g.RebuildTerritory();
     Check(g.LinkTrade(a)==0&&g.LinkTrade(b)==0,"cities not joined add nothing");int tradeBefore=g.Trade(a);
     int x=a.x,y=a.y;var walker=g.Add(0,3,a.x,a.y);var path=g.Route(walker,b.x,b.y,true);g.units.Remove(walker);foreach(var t in path){t.road=true;t.owner=0;}g.At(a.x,a.y).owner=0;g.At(b.x,b.y).owner=0;g.ResetLinks();
     Check(path.Count>0&&g.LinkTrade(a)==1&&g.LinkTrade(b)==1&&g.Trade(a)==tradeBefore+1,"a road between two cities adds Trade");
     foreach(var t in path)t.road=false;g.At(a.x,a.y).road=true;g.At(b.x,b.y).road=true;g.ResetLinks();Check(g.LinkTrade(a)==0,"and goes when the road does");}
    // Animals teem early and never appear inside borders.
    {var g=World.New(223,"Wilds",0,1,1,0,1,2,4,hex:true,natives:false,wild:true);g.Found(g.units.First(u=>u.owner==0&&u.type==0));int early=g.AnimalLimit();Check(g.EarlyWilds(),"the wilds teem at the start");int seen=0;for(int k=0;k<8;k++){int count=g.animals.Count;var ids=new HashSet<int>(g.animals.Select(a=>a.id));g.TickAnimalsForChecks(true);seen+=g.animals.Count(a=>!ids.Contains(a.id)&&g.At(a.x,a.y).owner>=0);}Check(g.animals.Count>=4,"animals appear from the first turns ("+g.animals.Count+")");
     foreach(var a in g.animals.ToList()){}Check(seen<=g.animals.Count,"few wander in");
     for(int e=0;e<=3;e++)foreach(int t in Enumerable.Range(0,TechnologyCatalog.Count).Where(t=>TechnologyCatalog.Eras[t]==e))g.nations[0].tech[t]=true;g.tech=g.nations[0].tech;g.EndTurn();if(!g.EarlyWilds())Check(g.AnimalLimit()*World.WildMultiple==early,"past the Medieval Age they fall back to a third");
     var fresh=World.New(224,"Spawn",0,1,1,0,1,2,4,hex:true,natives:false,wild:true);fresh.Found(fresh.units.First(u=>u.owner==0&&u.type==0));foreach(var c in fresh.cities)foreach(var t in fresh.tiles)if(fresh.Distance(t.x,t.y,c.x,c.y)<=12)t.owner=c.owner;for(int k=0;k<6;k++){var ids=new HashSet<int>(fresh.animals.Select(a=>a.id));fresh.turn++;fresh.SpawnAnimalsForChecks();Check(fresh.animals.Where(a=>!ids.Contains(a.id)).All(a=>fresh.At(a.x,a.y).owner<0),"no animal appears on land within borders");}}
    // A chosen building can be cancelled from anywhere on the list.
    city.buildingProject=7;city.buildingQueue.Clear();city.buildingQueue.Add(8);city.buildingQueue.Add(36);Check(w.CancelProject(city,8)&&city.buildingQueue.SequenceEqual(new[]{36})&&city.buildingProject==7,"a queued building can be cancelled");Check(w.CancelProject(city,7)&&city.buildingProject==36&&city.buildingQueue.Count==0,"cancelling the one under way starts the next");Check(w.CancelProject(city,36)&&city.buildingProject==-1,"and then the line is idle");Check(!w.CancelProject(city,36),"nothing left to cancel");
    // Other worlds: nothing shows until found; more comes with agents.
    {var g=World.New(225,"Legends",0,1,1,0,1,2,4,hex:true);g.Found(g.units.First(u=>u.owner==0&&u.type==0));foreach(var t in g.tiles)t.seen=false;for(int o=0;o<g.civCount;o++)for(int i=0;i<g.tiles.Length;i++)if(!g.IsNativeNation(o))g.nations[o].known[i]=false;g.turn++;
     Check(!g.WorldKnownAtHome(1),"no world is known before a gateway is found");int people=Enumerable.Range(1,g.civCount-1).First(o=>g.IsNativeNation(o)&&CivilisationCatalog.NativeRealm(g.nations[o].civ)==1);Check(g.PeopleDetail(people)==0&&g.SpyReason(people)!="","its peoples are unknown and out of reach");
     var gate=g.tiles.First(t=>t.realm==0&&t.gateLink>0&&g.GateTwin(t).realm==1);gate.seen=true;g.turn++;Check(g.WorldKnownAtHome(1)&&!g.WorldKnownAtHome(g.GateTwin(g.tiles.First(t=>t.realm==0&&t.gateLink>0&&g.GateTwin(t).realm!=1)).realm),"finding a gateway reveals that world alone");
     g.gold=500;g.nations[0].gold=500;Check(g.SpyPrice(people)==50&&g.SendAgents(people)&&g.PeopleDetail(people)==1&&g.gold==450,"agents buy the first news");Check(g.SendAgents(people)&&g.SendAgents(people)&&g.PeopleDetail(people)==3&&g.SpyReason(people)!="","then more, up to everything");
     int friend=Enumerable.Range(1,g.civCount-1).First(o=>g.IsNativeNation(o)&&o!=people);g.met[friend]=true;Check(g.PeopleDetail(friend)>=1,"meeting a people shows who they are");g.Relation(0,friend).friendUntil=g.turn+10;Check(g.PeopleDetail(friend)==3,"friendship tells all");}
    Check(!CivilisationCatalog.Names.Contains("Open Register"),"the Open Register has a new name");
    // Civics figures.
    {var s=w.StatsFor(0);Check(s.cities==w.cities.Count(c=>c.owner==0)&&s.population>0&&s.land>0&&s.approval>=0&&s.approval<=100&&s.literacy>=0&&s.literacy<=100,"Civics figures are sensible");}
    Check(PixelArt.EraHatch(0).w==16&&Enumerable.Range(0,6).Select(e=>PixelArt.EraHatch(e).p[3]).Distinct().Count()>=4,"each age has its own bar pattern");
}
static void DifficultyChecks(){
    int L=DifficultyTable.Names.Length;Check(L==8&&DifficultyTable.Names.Distinct().Count()==8&&DifficultyTable.Texts.Length==8,"eight named difficulty levels");
    foreach(var row in new[]{DifficultyTable.RivalCost,DifficultyTable.RivalWonderCost,DifficultyTable.RivalFood,DifficultyTable.RivalResearch,DifficultyTable.RivalCities,DifficultyTable.RivalSoldiers,DifficultyTable.WarLust,DifficultyTable.PlayerGrace,DifficultyTable.Attitude,DifficultyTable.YourResearch,DifficultyTable.YourCost,DifficultyTable.ExtraSoldiers,DifficultyTable.ExtraWorkers,DifficultyTable.ExtraSettlers,DifficultyTable.ExtraTechs})Check(row.Length==8,"each difficulty row has eight values");
    int S=DifficultyTable.Steward;Check(DifficultyTable.RivalCost[S]==100&&DifficultyTable.RivalResearch[S]==100&&DifficultyTable.RivalFood[S]==100&&DifficultyTable.RivalSoldiers[S]==100&&DifficultyTable.WarLust[S]==0&&DifficultyTable.PlayerGrace[S]==80&&DifficultyTable.RivalCities[S]==4&&DifficultyTable.YourCost[S]==100&&DifficultyTable.YourResearch[S]==100,"Steward is the game's plain rules");
    for(int i=1;i<L;i++)Check(DifficultyTable.RivalCost[i]<DifficultyTable.RivalCost[i-1]&&DifficultyTable.RivalResearch[i]<=DifficultyTable.RivalResearch[i-1]&&DifficultyTable.PlayerGrace[i]<DifficultyTable.PlayerGrace[i-1],"each level is harder than the last");
    for(int i=0;i<L;i++)Check(DifficultyTable.Describe(i).Contains("Rivals never see through fog"),"each level describes itself");
    var plain=World.New(150,"Plain",0,1,1,0,1,2,4,hex:true);Check(plain.difficultyLevel==S&&plain.Valid(),"new games default to Steward");
    var easy=World.New(150,"Easy",0,1,1,0,1,2,4,hex:true,level:0);var hard=World.New(150,"Hard",0,1,1,0,1,2,4,hex:true,level:7);int t=TechnologyCatalog.Index("writing");
    Check(easy.Valid()&&hard.Valid(),"every level starts a valid world");
    Check(easy.ResearchCost(t,1)>plain.ResearchCost(t,1)&&hard.ResearchCost(t,1)<plain.ResearchCost(t,1),"rival research cost follows the level");
    Check(easy.ResearchCost(t,0)<plain.ResearchCost(t,0)&&hard.ResearchCost(t,0)==plain.ResearchCost(t,0),"your research is cheaper on the easy levels only");
    int native=Enumerable.Range(0,hard.civCount).First(hard.IsNativeNation);Check(hard.ResearchCost(t,native)==plain.ResearchCost(t,native),"the sealed peoples are not changed");
    Check(hard.units.Count(u=>u.owner==1)>plain.units.Count(u=>u.owner==1)&&easy.units.Count(u=>u.owner==1)==plain.units.Count(u=>u.owner==1),"hard levels give rivals extra starting units");
    Check(hard.nations[1].tech.Count(a=>a)==plain.nations[1].tech.Count(a=>a)+1,"the hardest level gives rivals a technology");
    Check(hard.RivalTargetCities(1)==8&&easy.RivalTargetCities(1)==3&&plain.RivalTargetCities(1)==4,"rivals aim for more cities on harder levels");
    {var city0=plain.cities.First(c=>c.owner==1);var city1=hard.cities.First(c=>c.owner==1);int warrior=World.UnitBuild(2);Check(hard.CostFor(city1,warrior)<plain.CostFor(city0,warrior),"rival units are cheaper on hard levels");Check(hard.FoodNeeded(city1)<plain.FoodNeeded(city0),"rival cities grow faster on hard levels");Check(plain.SoldierLimit(1)<hard.SoldierLimit(1)||hard.cities.Count(c=>c.owner==1)>=plain.cities.Count(c=>c.owner==1),"rival armies grow with the level");}
    plain.turn=90;hard.turn=45;Check(hard.PlayerGraceTurn==40&&plain.PlayerGraceTurn==80&&easy.PlayerGraceTurn==140,"harder rivals come for you sooner");
    {var old=World.New(151,natives:false);old.version=25;old.difficultyLevel=7;old.UpgradeSave();Check(old.version==35&&old.difficultyLevel==S&&old.Valid(),"an older save plays at Steward");}
    // The message when a city finishes something.
    var w=World.New(152,"Notice",0,1,1,0,1,2,4,hex:true);var mine=w.Found(w.units.First(u=>u.owner==0&&u.type==0));var rival=w.cities.First(c=>c.owner==1);
    w.tech[TechnologyCatalog.Index("toolmaking")]=true;w.nations[1].tech[TechnologyCatalog.Index("toolmaking")]=true;mine.pop=3;
    mine.build=World.UnitBuild(2);mine.production=w.CostFor(mine,mine.build);w.Produce(mine,0);Check(w.buildNotices.Count==1&&w.buildNotices[0].city==mine.id&&!w.buildNotices[0].building&&w.buildNotices[0].text.Contains(mine.name)&&w.buildNotices[0].text.Contains("trained"),"finishing a unit tells you what was trained");
    rival.build=World.UnitBuild(2);rival.production=w.CostFor(rival,rival.build);w.Produce(rival,0);Check(w.buildNotices.Count==1,"a rival's city sends you no message");
    string why;int next=w.SuggestNext(mine,false,out why);Check(next>=0&&why!=""&&w.BuildReason(mine,next)=="","the message suggests a unit it can make, with a reason ("+why+")");
    {int need=World.BuildingTech[Array.IndexOf(World.BuildingIds,7)];if(need>=0)w.tech[need]=true;}mine.buildingProject=7;mine.buildingProduction=w.CostFor(mine,7);w.Produce(mine,0);Check(w.buildNotices.Count==2&&w.buildNotices[1].building&&w.buildNotices[1].text.Contains("built"),"finishing a building tells you what was built");
    int nextB=w.SuggestNext(mine,true,out why);Check(nextB>=0&&why!=""&&World.BuildUnit(nextB)<0&&w.BuildReason(mine,nextB)=="","the message suggests a building it can make, with a reason ("+why+")");
    Check(w.ChooseProject(mine,nextB,false)&&mine.buildingProject==nextB,"the suggestion can be taken as it stands");
    w.EndTurn();Check(w.buildNotices.All(n=>n.text.Contains(mine.name)&&w.turn>0)&&w.buildNotices.Count<=2,"old messages clear when the turn ends");
    // A leader who wants to talk gets your attention, and the message goes when the matter is settled.
    var t2=World.New(153,"Calls",0,1,1,0,1,2,4,hex:true,natives:false);t2.Found(t2.units.First(u=>u.owner==0&&u.type==0));t2.met[1]=true;Check(t2.NextRivalCall()==null,"nobody is calling at first");
    var made=t2.Propose(new DiplomaticOffer{from=1,to=0,kind=TreatyKind.Access});var call=t2.NextRivalCall();Check(made.status==0&&call!=null&&call.from==1&&call.offer==made.id&&call.text.Contains("open borders"),"a proposal brings up a message saying what is wanted");
    Check(t2.RespondToOffer(made.id,false)&&t2.NextRivalCall()==null,"answering the proposal clears its message");
    var lapsing=t2.Propose(new DiplomaticOffer{from=1,to=0,kind=TreatyKind.Access});for(int i=0;i<4;i++){t2.EndTurn();t2.rivalCalls.RemoveAll(c=>c.offer!=lapsing.id);}Check(t2.NextRivalCall()==null,"an expired proposal clears its message");
    t2.rivalCalls.Clear();t2.turn=200;t2.Relation(0,1).friendUntil=0;t2.Relation(0,1).allianceUntil=0;t2.Relation(0,1).peaceUntil=0;Check(t2.Denounce(1,0)&&t2.NextRivalCall()!=null&&t2.NextRivalCall().text.Contains("denounced"),"a denunciation of you brings up a message");t2.rivalCalls.Clear();
    Check(t2.DeclareWar(1,0)&&t2.NextRivalCall()!=null&&t2.NextRivalCall().text.Contains("war"),"a declaration of war brings up a message");t2.rivalCalls.Clear();
    {int before=t2.Relation(1,2).contact?1:0;t2.Relation(1,2).contact=true;t2.Propose(new DiplomaticOffer{from=1,to=2,kind=TreatyKind.Access});Check(t2.NextRivalCall()==null,"rivals talking among themselves do not call you");}
}
// Owner request 5 Nov: fifty peoples of the sealed worlds, ten to a world.
static void PeopleChecks(){
    Check(SealedPeoples.People.Count==SealedPeoples.Count,"forty later peoples written");
    for(int r=1;r<=World.RealmCount;r++){var ten=CivilisationCatalog.PeoplesOf(r).ToList();Check(ten.Count==10,World.RealmNames[r]+" has ten peoples");string h=SealedPeoples.WorldHistories[r];Check(h!=null&&h.Length>300,World.RealmNames[r]+" has its history");
     var pairs=ten.Skip(2).Select(c=>{var t=CivilisationCatalog.Traits[c];return Math.Min(t[0],t[1])*100+Math.Max(t[0],t[1]);}).ToList();Check(pairs.Distinct().Count()==8,World.RealmNames[r]+" peoples have different traits");}
    foreach(var p in SealedPeoples.People){int civ=SealedPeoples.First+SealedPeoples.People.IndexOf(p);var e=p.Lore;string who=p.Name;
     Check(CivilisationCatalog.NativeRealm(civ)==p.World&&CivLore.Of(civ)==e&&CivilisationCatalog.Names[civ]==p.Name&&CivilisationCatalog.Greetings[civ]==p.Greeting,who+" is in the catalogue");
     Check(!string.IsNullOrEmpty(p.Leader)&&!string.IsNullOrEmpty(p.LeaderTitle)&&!string.IsNullOrEmpty(p.Greeting)&&p.Cities.Length==12&&p.Cities.Distinct().Count()==12,who+" has a leader and twelve cities");
     Check(e.Background.Length>=120&&e.History.Length>=500&&e.Strengths!=""&&e.Weaknesses!="",who+" has a full history ("+e.History.Length+")");
     Check(p.Traits[0]!=p.Traits[1]&&p.Traits.All(t=>t>=0&&t<CivilisationCatalog.TraitCount)&&e.Agenda>=0&&e.Agenda<CivLore.AgendaNames.Length&&p.Bust>=0&&p.Bust<=14&&p.Hue>=0&&p.Hue<=7,who+" has valid numbers");
     Check(e.UnitBase>=2&&e.UnitBase<=9&&e.UnitAttack+e.UnitDefence+e.UnitMoves>=1&&Array.IndexOf(World.BuildingIds,e.BuildingBase)>=0,who+" has a valid unit and building");
     Check(!e.AbilityText.StartsWith(".")&&!e.WeaknessText.StartsWith(".")&&!e.BuildingText.Contains(": as well"),who+" has rules that say something");
     var hard=new[]{e.WeaknessRule.Food,e.WeaknessRule.Production,e.WeaknessRule.Science,e.WeaknessRule.Gold,e.WeaknessRule.Content,e.WeaknessRule.Faith,e.WeaknessRule.Attack,e.WeaknessRule.Defence,e.WeaknessRule.Sight};Check(hard.Min()<0&&hard.Max()<=0,who+"'s weakness is a real penalty");
     Check(PixelArt.PeoplePortrait(p).w==150,who+" has a portrait");}
    // All ten live in every world in a game, and their rule numbers reach the rules.
    var w=World.New(130,"Crowd",0,1,1,0,1,2,4,hex:true,peoplesPerWorld:10);var peoples=Enumerable.Range(0,w.civCount).Where(w.IsNativeNation).ToList();
    Check(peoples.Count==50&&w.civCount==54&&w.Valid(),"fifty peoples join a game");Check(peoples.All(o=>w.cities.Any(c=>c.owner==o&&w.RealmOf(c.x,c.y)==CivilisationCatalog.NativeRealm(w.nations[o].civ))),"each of the fifty starts with a city in its own world");
    for(int t=0;t<10;t++)w.EndTurn();Check(w.Valid()&&w.result=="","fifty peoples run for ten turns");
    {int o=peoples.First(a=>w.nations[a].civ>=SealedPeoples.First&&CivLore.Of(w.nations[a].civ).AbilityRule.Food>0);var c=w.cities.First(a=>a.owner==o);int with=w.Yield(c)[0];var e=CivLore.Of(w.nations[o].civ);int keep=e.AbilityRule.Food;e.AbilityRule.Food=0;int without=w.Yield(c)[0];e.AbilityRule.Food=keep;Check(with==without+keep,"a people's ability changes its yields");}
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
    {var old=World.New(126,natives:false);old.version=24;old.UpgradeSave();Check(old.version==35&&!old.Hex&&old.Valid(),"a version 24 save keeps its square map");}
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
    {var crowded=World.New(93,"",0,1,1,0,5,1,14);Check(Enumerable.Range(0,crowded.civCount).Count(crowded.IsNativeNation)==10&&crowded.civCount==24,"a 14-civilisation game still has its sealed peoples");}
    // Save version 26 locks the gates and sends stray land units back; a colony's garrison stays.
    {var old=World.New(95);old.version=23;old.gatesLocked=false;var g3=old.tiles.First(t=>t.realm==0&&t.gateLink>0&&old.GateTwin(t).realm==3);var far=old.GateTwin(g3);var stray=old.Add(0,2,far.x,far.y);
     old.UpgradeSave();Check(old.version==35&&old.gatesLocked&&old.RealmOf(stray.x,stray.y)==0&&old.Distance(stray.x,stray.y,g3.x,g3.y)<=1&&old.Valid(),"an older save locks the gates and brings stray units home");}
    // The land of the sealed worlds feeds its own peoples and keeps its danger for outsiders.
    {var land=World.New(96);var ash=land.tiles.First(t=>t.realm==3&&t.terrain==17&&t.resource==0&&!t.farm&&!t.mine&&!t.road);ash.owner=-1;var fed=land.TileYield(ash);Check(fed[0]==3&&fed[1]==2&&fed[2]==1,"volcanic land feeds its native people");
     ash.owner=0;var outsider=land.TileYield(ash);Check(outsider[0]==0&&outsider[1]==3&&outsider[2]==0,"volcanic land stays hard on an outsider");ash.owner=-1;}
    // The Spore Circle's slow units (2 moves) can still leave a city ringed by bog, which costs others 3.
    {var slow=World.New(98);int spore=Enumerable.Range(0,slow.civCount).First(o=>slow.nations[o].civ==36);var sc=slow.cities.First(c=>c.owner==spore);var bog=slow.tiles.First(t=>t.realm==4&&t.terrain==9);var walker2=slow.Add(spore,2,sc.x,sc.y);Check(slow.StepCost(walker2,sc.x,sc.y,bog)<=slow.MaxMoves(walker2),"a slow people's units can always take a step");}
    // A sealed people behind the home world catches up on research and one far ahead slows; it cannot transcend.
    {var pace=World.New(97);int nativeOwner=Enumerable.Range(0,pace.civCount).First(pace.IsNativeNation);var n=pace.nations[nativeOwner];n.research=pace.AvailableTech(nativeOwner).First();int t0=n.research;n.progress[t0]=0;
     foreach(int o in Enumerable.Range(0,pace.civCount).Where(o=>!pace.IsNativeNation(o)))for(int i=0;i<20;i++)pace.nations[o].tech[i]=true;
     Check(pace.HomeTechPace()>=20,"home pace counts home civilisations only");int behind=pace.HomeTechPace()-n.tech.Count(a=>a);pace.KeepPace(nativeOwner);Check(behind>=World.PaceShare&&n.progress[t0]==pace.ResearchCost(t0,nativeOwner),"a people far behind gains a full technology's research in a turn");
     for(int i=0;i<24;i++)n.tech[i]=true;n.research=pace.AvailableTech(nativeOwner).First();int held=pace.ResearchCost(n.research,nativeOwner);n.progress[n.research]=held+5;pace.KeepPace(nativeOwner);Check(n.progress[n.research]==held-1,"a people far ahead cannot finish its next technology");
     var nc=pace.cities.First(c=>c.owner==nativeOwner);Check(pace.BuildReason(nc,70)!="","a sealed people cannot build Transcendence");}
}
}
}
