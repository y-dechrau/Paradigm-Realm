using System;
using System.Collections.Generic;
using System.Linq;

namespace ParadigmRealm
{
    // Pure C# checks, callable in Unity from Paradigm Realm > Run Engine Checks.
    public static class FoundationChecks
    {
        static void Require(bool condition,string message){if(!condition)throw new Exception("FAIL: "+message);}
        static World Flat(){var w=World.New(73);foreach(string id in new[]{"toolmaking","wheel","pottery"})w.tech[TechnologyCatalog.Index(id)]=true;foreach(var t in w.tiles){t.terrain=0;t.road=false;t.resource=0;t.seen=true;for(int n=0;n<3;n++)w.nations[n].known[t.y*w.Width+t.x]=true;}return w;}
        public static void Run()
        {
            var w=Flat();Require(w.Valid(),"new state validates");
            var u=w.units.First(a=>a.owner==0&&a.type==2);int x=u.x,y=u.y;
            Require(w.Order(u,x+6,y)&&u.moves==0&&u.hasOrder,"movement allowance respected");
            w.EndTurn();w.EndTurn();Require(u.x==x+6&&u.y==y&&!u.hasOrder,"queued route completes over turns");
            w.Order(u,x,y);w.Order(u,x+6,y+2);Require(u.destinationY==y+2,"new order replaces old");
            var c=w.Found(w.units.First(a=>a.owner==0&&a.type==0));Require(c!=null,"founding");
            w.tech[TechnologyCatalog.Index("agriculture")]=true;var worker=w.units.First(a=>a.owner==0&&a.type==1);
            Require(w.StartJob(worker,1),"worker can start road");w.EndTurn();Require(!w.At(worker.x,worker.y).road,"road not instant");w.EndTurn();Require(w.At(worker.x,worker.y).road&&worker.job==0,"job finishes after two turns");
            w.tech[TechnologyCatalog.Index("bronze")]=true;var ore=w.At(c.x+1,c.y);ore.resource=2;ore.mine=true;ore.road=true;w.RebuildTerritory();
            Require(w.HasResource(0,2),"improved road-connected iron");ore.road=false;w.RebuildTerritory();Require(!w.HasResource(0,2),"cut road removes access");
            c.build=-1;c.production=0;Require(w.QueueBuild(c,7)&&w.QueueBuild(c,2),"queue accepts valid projects");Require(!w.QueueBuild(c,7),"duplicate building rejected");c.buildingProduction=w.Cost(7);w.EndTurn();Require(c.granary&&c.build==2,"completion advances queue");
            c.pop=5;w.SetScienceRate(0,60);int low=w.ContentSlots(c),lowTax=w.Budget(0)[0];w.SetScienceRate(0,30);Require(w.ContentSlots(c)==low-2&&w.Budget(0)[0]>lowTax,"taxes trade happiness for revenue");
            w.Victory(u);Require(u.promotion==0,"one victory is not a rank");w.Victory(u);Require(u.promotion==1,"two victories make a Veteran");u.victories=0;u.promotion=0;
            var neutral=w.Add(1,2,u.x,u.y+1);w.Reveal();u.moves=World.Moves[u.type];Require(!w.Move(u,neutral.x,neutral.y),"peace blocks combat");Require(w.SetWar(1,true),"contact enables declaration");Require(w.Move(u,neutral.x,neutral.y)&&neutral.hp<10,"war enables combat");
            var research=World.New(81);research.tech[TechnologyCatalog.Index("toolmaking")]=true;int fire=TechnologyCatalog.Index("agriculture"),hunting=TechnologyCatalog.Index("archery");research.SelectResearch(fire);research.science=12;research.SelectResearch(hunting);research.science=7;research.SelectResearch(fire);Require(research.science==12&&research.nations[0].progress[hunting]==7,"switching preserves individual research progress");
            var campaign=World.New(91);campaign.Found(campaign.units.First(a=>a.owner==0&&a.type==0));
            for(int i=0;i<45&&campaign.result=="";i++){campaign.EndTurn();Require(campaign.Valid(),"campaign state valid after turn "+campaign.turn);}
            campaign.units[0].type=99;Require(!campaign.Valid(),"malformed unit rejected");
            var memory=World.New(67);var spot=memory.At(9,12);memory.Reveal();int oldTerrain=spot.terrain;memory.units.RemoveAll(a=>a.owner==0);spot.terrain=3;Require(memory.KnownTile(spot).terrain==oldTerrain,"fog preserves last-seen terrain");
            var siege=Flat();siege.met[1]=true;siege.SetWar(1,true);var targetCity=siege.cities.First(a=>a.owner==1);siege.units.RemoveAll(a=>a.x==targetCity.x&&a.y==targetCity.y);var archer=siege.Add(0,4,targetCity.x-1,targetCity.y);siege.Reveal();siege.Order(archer,targetCity.x,targetCity.y);Require(archer.moves==0&&targetCity.owner==1,"ranged unit cannot capture or loop on empty city");
            var stack=Flat();var mover=stack.units.First(a=>a.owner==0&&a.type==2);int tx=mover.x+1,ty=mover.y;for(int i=0;i<3;i++)stack.Add(0,2,tx,ty);Require(!stack.Move(mover,tx,ty),"military stack limit");
            var work=Flat();var a=work.units.First(v=>v.owner==0&&v.type==1);a.x=15;a.y=15;
            Require(work.At(15,15).owner==-1&&work.StartJob(a,1)&&a.jobTurns==3,"unclaimed flat road costs three worker-turns");
            work.EndTurn();work.EndTurn();Require(!work.At(15,15).road,"outside flat road not complete early");work.EndTurn();Require(work.At(15,15).road,"outside flat road completes");
            a.x=16;a.moves=4;work.At(16,15).terrain=2;Require(work.StartJob(a,1)&&a.jobTurns==4,"outside hill road costs four worker-turns");
            work.CancelActivity(a);Require(a.job==0&&!a.hasOrder,"cancel clears job and route");
            a.moves=4;var b=work.Add(0,1,15,15);Require(work.Move(b,16,15),"workers can share a tile");
            Require(work.GroupWorkers(a)&&a.workerGroup==b.workerGroup,"workers group by tile");
            Require(work.StartJob(a,1)&&b.job==1&&work.JobTurnsRemaining(a)==2,"group job assigns both workers and calculates ETA");
            work.EndTurn();Require(!work.At(16,15).road&&a.jobTurns==2&&b.jobTurns==2,"team contributes once per member per turn");
            work.EndTurn();Require(work.At(16,15).road&&a.job==0&&b.job==0,"two workers finish four-work road in two turns");
            a.moves=b.moves=4;work.At(16,15).road=false;work.StartJob(a,1);work.CancelActivity(b);Require(a.job==0&&b.job==0,"cancel applies to selected team");
            work.UngroupWorkers(a);Require(a.workerGroup==0&&b.workerGroup==0,"ungroup resets all members");
            a.moves=4;work.StartJob(a,1);work.units.Remove(b);work.EndTurn();Require(a.jobTurns==3,"ungrouped worker contributes one work");
            work.CancelActivity(a);a.moves=4;work.At(a.x,a.y).owner=1;Require(work.JobReason(a,1)!="","closed foreign borders block road job");
            work.openBorders[1]=true;Require(work.StartJob(a,1),"open borders permit foreign road work");Require(work.JobReason(a,2)!="","farms still require own territory");
            Require(work.Valid(),"worker group state validates");
            var dual=Flat();var dc=dual.Found(dual.units.First(v=>v.owner==0&&v.type==0));for(int i=0;i<dual.tech.Length;i++)dual.tech[i]=true;
            Require(dual.ChooseProject(dc,1,false)&&dual.ChooseProject(dc,7,false),"unit and building can be selected independently");
            dual.Produce(dc,5);Require(dc.production==5&&dc.buildingProduction==5,"both production lines advance simultaneously");
            Require(dual.QueueBuild(dc,1)&&dual.QueueBuild(dc,3)&&dual.QueueBuild(dc,8)&&dual.QueueBuild(dc,9),"both queues accept mixed projects");
            Require(!dual.QueueBuild(dc,7)&&!dual.QueueBuild(dc,8),"duplicate buildings rejected in active and queued orders");
            dual.RemoveLastProject(dc,false);Require(dc.queue.SequenceEqual(new[]{1})&&dc.buildingQueue.SequenceEqual(new[]{8,9}),"unit removal is last-first and leaves buildings alone");
            dual.RemoveLastProject(dc,true);Require(dc.buildingQueue.SequenceEqual(new[]{8}),"building removal is last-first");
            dual.ChooseProject(dc,3,false);Require(dc.build==3&&dc.queue.Count==0&&dc.production==0&&dc.buildingProject==7&&dc.buildingProduction==5,"single click selects one and clears only its line");
            dc.production=dual.Cost(3)-1;dc.buildingProduction=dual.Cost(7)-1;int unitCount=dual.units.Count;dual.Produce(dc,1);
            Require(dual.units.Count==unitCount+1&&dc.granary&&dc.build<0&&dc.buildingProject==8,"unit and building complete in same production step");
            Require(!dual.ChooseProject(dc,7,false),"constructed building cannot be built twice");
            dual.ChooseProject(dc,2,false);for(int i=0;i<2;i++)dual.Add(0,2,dc.x,dc.y);dc.production=dual.Cost(2);dc.buildingProduction=dual.Cost(8);dual.Produce(dc,1);
            Require(dc.build==2&&dc.library,"blocked unit stack does not block building construction");
            var old=Flat();var oc=old.Found(old.units.First(v=>v.owner==0&&v.type==0));old.version=3;oc.build=8;oc.production=17;oc.queue=new System.Collections.Generic.List<int>{1,7,1,9};old.UpgradeSave();
            Require(old.version==19&&oc.build==1&&oc.queue.SequenceEqual(new[]{1})&&oc.production==0&&oc.buildingProject==8&&oc.buildingProduction==17&&oc.buildingQueue.SequenceEqual(new[]{7,9})&&old.Valid(),"version-3 mixed queue migration preserves both category orders and active progress");
            old.UpgradeSave();Require(oc.buildingProduction==17&&oc.queue.Count==1,"migration is idempotent");
            var upgrade=Flat();var uc=upgrade.Found(upgrade.units.First(v=>v.owner==0&&v.type==0));var troop=upgrade.units.First(v=>v.owner==0&&v.type==2);upgrade.tech[TechnologyCatalog.Index("bronze")]=true;upgrade.gold=100;
            Require(!upgrade.Upgrade(troop),"owned territory alone does not permit upgrades");troop.x=uc.x;troop.y=uc.y;Require(upgrade.Upgrade(troop)&&troop.type==7,"owned city permits eligible upgrade");
            var other=upgrade.units.First(v=>v.owner==0&&v.type==1);int otherMoves=other.moves;upgrade.SkipTurn(troop);Require(troop.moves==0&&other.moves==otherMoves,"skip affects only selected unit");
            Require(World.StackLabel(new[]{new Unit{type=1}})=="Worker"&&World.StackLabel(new[]{new Unit{type=1},new Unit{type=1}})=="Stack"&&World.StackLabel(new[]{new Unit{type=1},new Unit{type=2}})=="","unit label rules");
            MapSizeChecks();
            SphereChecks();
            SpeedChecks();
            PixelChecks();
            CivChecks();
            VictoryChecks();
            ContentChecks.Run();
            var victory=World.New(22);victory.units.RemoveAll(a=>a.owner>0);victory.cities.RemoveAll(a=>a.owner>0);victory.CheckVictory();Require(victory.result.StartsWith("Conquest"),"conquest detected");
            var defeat=World.New(23);defeat.units.RemoveAll(a=>a.owner==0);defeat.CheckVictory();Require(defeat.result.Contains("fallen"),"defeat detected");
        }
        // The speed-ups must not change behaviour: each fast routine is compared with the plain definition it replaced.
        static void SpeedChecks()
        {
            foreach(int shape in new[]{0,1})
            {
                var w=World.New(60+shape,"Speed",0,1,1,0,shape==1?1:1,shape);
                // Extra cities so territory, workable tiles and ties are exercised.
                var sites=w.tiles.Where(site=>!World.IsImpassable(site.terrain)).Where((site,k)=>k%97==0).Take(30).ToList();
                foreach(var site in sites)
                    if(w.cities.All(c=>w.Distance(c.x,c.y,site.x,site.y)>2))w.cities.Add(new City{id=w.nextId++,owner=w.cities.Count%3,x=site.x,y=site.y,name="Speed "+site.x+"-"+site.y,age=(site.x*7)%90});
                w.RebuildTerritory();
                var expected=w.tiles.Select(t=>{var c=w.cities.Where(a=>w.Distance(t.x,t.y,a.x,a.y)<=Math.Min(4,2+a.age/25)).OrderBy(a=>w.Distance(t.x,t.y,a.x,a.y)).ThenBy(a=>a.id).FirstOrDefault();return c==null?-1:c.owner;}).ToArray();
                Require(w.tiles.Select(t=>t.owner).SequenceEqual(expected),"territory matches the plain nearest-city rule on shape "+shape);
                foreach(var c in w.cities)
                {
                    var plain=w.tiles.Where(t=>t.owner==c.owner&&w.Distance(t.x,t.y,c.x,c.y)<=2&&!(t.x==c.x&&t.y==c.y)&&!w.cities.Any(a=>a.x==t.x&&a.y==t.y)&&w.cities.Where(a=>a.owner==c.owner).OrderBy(a=>w.Distance(a.x,a.y,t.x,t.y)).ThenBy(a=>a.id).First()==c).ToList();
                    Require(w.Workable(c).SequenceEqual(plain),"workable tiles match the plain rule for "+c.name);
                    bool water=w.tiles.Any(t=>w.Distance(t.x,t.y,c.x,c.y)==1&&(t.terrain==4||t.terrain==6||t.terrain==12));
                    Require(w.AdjacentWater(c)==water,"adjacent water matches for "+c.name);
                }
                // Visibility cache gives the same answers as asking directly.
                var direct=new List<bool>();for(int i=0;i<w.tiles.Length;i++)direct.Add(w.Visible(w.tiles[i].x,w.tiles[i].y));
                w.BeginVisibilityCache();
                for(int i=0;i<w.tiles.Length;i++)if(w.Visible(w.tiles[i].x,w.tiles[i].y)!=direct[i])throw new Exception("FAIL: visibility cache differs on shape "+shape);
                w.EndVisibilityCache();
                // Routes are valid walks that end where asked, and repeat exactly.
                var u=w.units.First(a=>a.owner==0&&a.type==2);int routes=0;
                foreach(var goal in w.tiles.Where(g=>!World.IsImpassable(g.terrain)&&w.nations[0].known[g.y*w.Width+g.x]).Where((g,k)=>k%3==0).Take(40).ToList())
                {
                    var path=w.Route(u,goal.x,goal.y);var again=w.Route(u,goal.x,goal.y);
                    Require(path.SequenceEqual(again),"routes repeat exactly");
                    if(path.Count==0)continue;
                    routes++;int px=u.x,py=u.y;
                    foreach(var step in path){Require(w.Distance(px,py,step.x,step.y)==1,"each route step is one move");px=step.x;py=step.y;}
                    Require(px==goal.x&&py==goal.y,"routes end at the goal");
                }
                Require(routes>0,"at least one route was found on shape "+shape);
            }
            for(int i=0;i<TechnologyCatalog.Ids.Length;i++)Require(TechnologyCatalog.Index(TechnologyCatalog.Ids[i])==Array.IndexOf(TechnologyCatalog.Ids,TechnologyCatalog.Ids[i]),"technology index lookup matches");
            Require(TechnologyCatalog.Index("no-such-technology")==-1&&TechnologyCatalog.Index(null)==-1,"unknown technologies are not found");
            // Rough speed guard on the biggest map (generous limit; real timings come from Unity).
            var clock=System.Diagnostics.Stopwatch.StartNew();
            var big=World.New(3,"Big",0,1,1,0,5,1);
            long generated=clock.ElapsedMilliseconds;clock.Restart();
            var scout=big.units.First(a=>a.owner==0&&a.type==2);int found=0;
            foreach(var target in big.cities)if(big.Route(scout,target.x,target.y).Count>0)found++;
            Require(found>0,"a route crosses the Massive sphere");
            Require(generated<60000&&clock.ElapsedMilliseconds<20000,"Massive sphere builds and routes within a generous limit (built in "+generated+" ms, routes in "+clock.ElapsedMilliseconds+" ms)");
        }
        static void PixelChecks()
        {
            // Settlements grow with the city: more built-up pixels as population rises, walls and a castle only when flagged.
            Func<PixelCanvas,int> filled=cv=>{int n=0;for(int i=0;i<cv.p.Length;i++)if((cv.p[i]>>24)!=0)n++;return n;};
            for(int style=0;style<4;style++)
            {
                int small=filled(PixelArt.Settlement(style,1,0,0)),large=filled(PixelArt.Settlement(style,10,0,0)),walled=filled(PixelArt.Settlement(style,10,PixelArt.FlagWalls,0));
                Require(small>0&&large>small&&walled>large,"settlement in age "+style+" grows with size and gains walls ("+small+", "+large+", "+walled+")");
                Require(filled(PixelArt.Settlement(style,10,PixelArt.FlagWalls|PixelArt.FlagCastle,0))>walled,"castle adds to a walled settlement in age "+style);
                Require(filled(PixelArt.Hamlet(style,1))>0,"farmsteads drawn in age "+style);
            }
            // Organic, level terrain (owner request 29 Oct 2026): every terrain composes to a full, deterministic, opaque tile.
            for(int t=0;t<21;t++){
                var n9=Enumerable.Repeat(t,9).ToArray();
                var a=PixelArt.Compose(n9,5,7,0,0);var b=PixelArt.Compose(n9,5,7,0,0);
                Require(a.w==PixelArt.CW&&a.h==PixelArt.CH&&PixelArt.CH==PixelArt.TH,"terrain "+t+" composite is exactly one level tile");
                Require(a.p.SequenceEqual(b.p),"terrain "+t+" art is deterministic");
                Require(a.p.All(c=>(c>>24)==255),"terrain "+t+" composite is fully opaque (no step, no gap)");
                Require(PixelArt.Height(t)==0,"no terrain is raised: "+t);
                var sp=PixelArt.Sprite(t,1);Require(sp==null||(sp.w==PixelArt.SW&&sp.h==PixelArt.SH),"terrain "+t+" sprite size");
            }
            // Borders meander: grassland with desert to the east blends in near the east edge, unevenly from row to row, and the
            // west half is untouched.
            int[] grass=Enumerable.Repeat(0,9).ToArray(),eastDesert=(int[])grass.Clone();eastDesert[2]=eastDesert[5]=eastDesert[8]=3;
            // Measured at ten map positions, since at any one position the noise may push the border away from this tile.
            var reach=new List<int>();bool westUntouched=true;int touched=0;
            for(int wx=0;wx<10;wx++){var pureAt=PixelArt.Compose(grass,wx,7,0,0);var mixedAt=PixelArt.Compose(eastDesert,wx,7,0,0);bool any=false;
                for(int y=0;y<PixelArt.TH;y++){int first=PixelArt.TW;for(int x=0;x<PixelArt.TW;x++)if(mixedAt.Get(x,y)!=pureAt.Get(x,y)){if(x<20)westUntouched=false;first=Math.Min(first,x);}if(first<PixelArt.TW){reach.Add(first);any=true;}}if(any)touched++;}
            Require(touched>=5&&westUntouched,"neighbouring desert blends into the east of the tile only, at most positions");
            Require(reach.Distinct().Count()>10,"the border wanders from row to row instead of a straight line");
            var pure=PixelArt.Compose(grass,5,7,0,0);
            // Coasts: sea to the north changes only the northern part of a land tile (lip and meander), never the south. The
            // wavy coastline may lie wholly in the sea tile at some positions, so it is measured at ten positions along a row.
            int[] northSea=(int[])grass.Clone();northSea[0]=northSea[1]=northSea[2]=4;int coastsSeen=0;bool southSame=true;
            for(int wx=0;wx<10;wx++){var plain=PixelArt.Compose(grass,wx,7,0,0);var coast=PixelArt.Compose(northSea,wx,7,0,0);bool northChanged=false;
                for(int x=0;x<PixelArt.TW;x++){for(int y=PixelArt.TH*2/3;y<PixelArt.TH;y++)if(coast.Get(x,y)!=plain.Get(x,y))southSame=false;for(int y=0;y<PixelArt.TH/3;y++)if(coast.Get(x,y)!=plain.Get(x,y))northChanged=true;}
                if(northChanged)coastsSeen++;}
            Require(southSame&&coastsSeen>=5,"a northern coast shapes only the north of the tile (seen at "+coastsSeen+" of 10 positions)");
            var clouds=PixelArt.CloudShadows();Require(clouds.w==128&&clouds.p.Any(c=>(c>>24)>0)&&clouds.p.Any(c=>(c>>24)==0),"cloud shadows have clear sky and shade");
            Require(PixelArt.Farm(0).w==PixelArt.TW&&PixelArt.Road(5).w==PixelArt.TW,"farm and road overlays are tile sized");
            // The diagonal screen mapping must invert exactly at every zoom: diamond centres and points just inside a diamond's
            // four tips map back to that tile; a step east goes half a diamond down-right, a step south half a diamond down-left.
            foreach(double tw in new[]{32.0,48,64,96,128})for(int i=-3;i<=3;i++)for(int j=-3;j<=3;j++){
                double sx,sy;PixelArt.ScreenOf(i+10,j+5,10.25,5.5,tw,out sx,out sy);double th=tw*PixelArt.DH/PixelArt.DW;
                foreach(var tip in new[]{new[]{0.0,0.0},new[]{-.85,0},new[]{.85,0},new[]{0,-.85},new[]{0,.85},new[]{.4,.4}}){
                    int pi,pj;PixelArt.PickOf(sx+tip[0]*tw/2,sy+tip[1]*th/2,10.25,5.5,tw,out pi,out pj);
                    Require(pi==i+10&&pj==j+5,"diagonal pick round trip "+i+","+j+" at zoom "+tw);
                }
                double ex,ey,s2x,s2y;PixelArt.ScreenOf(i+11,j+5,10.25,5.5,tw,out ex,out ey);PixelArt.ScreenOf(i+10,j+6,10.25,5.5,tw,out s2x,out s2y);
                Require(Math.Abs(ex-sx-tw/2)<1e-9&&Math.Abs(ey-sy-th/2)<1e-9&&Math.Abs(s2x-sx+tw/2)<1e-9&&Math.Abs(s2y-sy-th/2)<1e-9,"east steps down-right and south steps down-left by half a diamond");
                // Staggered rows: centres and points just inside each tip pick back to the tile; east is a whole diamond to
                // the right, and the grid step (0,+1) of World.Step lands half a diamond down-left or down-right on screen.
                {int x=i+10,y=j+6;double cx=10.3,cy=6.4,tx,ty;PixelArt.StaggerScreenOf(x,y,cx,cy,tw,out tx,out ty);
                 foreach(var tip in new[]{new[]{0.0,0.0},new[]{-.85,0},new[]{.85,0},new[]{0,-.85},new[]{0,.85},new[]{.4,.4}}){int px,py;PixelArt.StaggerPickOf(tx+tip[0]*tw/2,ty+tip[1]*th/2,cx,cy,tw,out px,out py);Require(px==x&&py==y,"staggered pick round trip "+x+","+y+" at zoom "+tw);}
                 double rx,ry;PixelArt.StaggerScreenOf(x+1,y,cx,cy,tw,out rx,out ry);Require(Math.Abs(rx-tx-tw)<1e-9&&Math.Abs(ry-ty)<1e-9,"staggered east is one diamond to the right");
                 int gi,gj,nx,ny;World.GridOf(x,y,out gi,out gj);World.TileOf(gi,gj+1,out nx,out ny);double sx2,sy2;PixelArt.StaggerScreenOf(nx,ny,cx,cy,tw,out sx2,out sy2);
                 World.TileOf(gi+1,gj,out nx,out ny);double ex2,ey2;PixelArt.StaggerScreenOf(nx,ny,cx,cy,tw,out ex2,out ey2);
                 Require(Math.Abs(sx2-tx+tw/2)<1e-9&&Math.Abs(sy2-ty-th/2)<1e-9&&Math.Abs(ex2-tx-tw/2)<1e-9&&Math.Abs(ey2-ty-th/2)<1e-9,"grid steps on staggered rows match the diagonal view's directions");}
                var diamond=PixelArt.ToDiamond(PixelArt.Compose(Enumerable.Repeat(0,9).ToArray(),i+10,j+5,0,0));
                Require(diamond.w==PixelArt.DW&&diamond.h==PixelArt.DH&&(diamond.Get(PixelArt.DW/2,PixelArt.DH/2)>>24)==255&&(diamond.Get(0,0)>>24)==0,"ground is reshaped into an opaque diamond with clear corners");
            }
            // Unit sprites: processed copies are 64 x 64 with an outline and keep their feet on the bottom rows.
            var src=new uint[32*32];for(int y=4;y<32;y++)for(int x=10;x<22;x++)src[y*32+x]=PixelArt.Rgb(180,120,80);
            var unit=PixelArt.UnitSprite(src,32,32);Require(unit.w==64&&unit.h==64,"unit sprite size");
            bool outlined=false,feet=false;for(int x=0;x<64;x++){if(unit.Get(x,62)==PixelArt.Rgb(20,18,24))outlined=true;if((unit.Get(x,61)>>24)!=0)feet=true;}
            Require(outlined&&feet,"unit sprite is outlined and stands on the bottom of its box");
        }
        static void CivChecks()
        {
            foreach(var plan in new[]{new[]{1,1,2},new[]{1,1,5},new[]{0,0,4},new[]{1,2,8},new[]{0,1,6}})
            {
                int shape=plan[0],size=plan[1],civs=plan[2];
                var w=World.New(900+civs,"Civ Test",0,1,1,0,size,shape,civs);
                string label=(shape==1?"sphere ":"flat ")+World.MapSizeNames[size]+" with "+civs+" civilisations";
                Require(w.civCount==civs&&w.nations.Length==civs&&w.met.Length==civs&&w.war.Length==civs&&w.openBorders.Length==civs,label+": per-civilisation arrays match");
                Require(w.diplomacy.Count==civs*(civs-1)/2&&w.Valid(),label+": diplomacy pairs and validity");
                Require(w.cities.Count==civs-1&&w.units.Count(u=>u.owner==0)==3,label+": every rival has a city, the player has a start party");
                var starts=Enumerable.Range(0,civs).Select(o=>w.units.Where(u=>u.owner==o).Select(u=>u.y*w.Width+u.x).First()).ToList();
                Require(starts.Distinct().Count()==civs,label+": distinct start tiles");
                for(int o=0;o<civs;o++)Require(w.units.Any(u=>u.owner==o&&u.type==0||w.cities.Any(c=>c.owner==o)),label+": civilisation "+o+" has a start");
                for(int a=0;a<civs;a++)for(int b=a+1;b<civs;b++)Require(w.Relation(a,b)!=null&&w.Relation(b,a)==w.Relation(a,b)&&w.Relation(a,b).a==a&&w.Relation(a,b).b==b,label+": pair "+a+","+b+" lookup");
                for(int i=0;i<6;i++)w.EndTurn();
                Require(w.Valid(),label+": still valid after six turns");
            }
            var clamped=World.New(5,"Clamp",0,1,1,0,0,1,16);
            Require(clamped.civCount==World.MaxCivsBySize[0],"requests above the size limit are clamped");
            var old=World.New(6,"Old",0,1,1,0,1,0,3);old.version=9;old.rivalWar=true;foreach(var p in old.diplomacy)p.atWar=false;old.UpgradeSave();
            Require(old.version==19&&old.civCount==3&&old.AtWar(1,2)&&!old.AtWar(0,1)&&old.Valid(),"version 9 saves upgrade with the old rival war flag preserved");
            var peace=World.New(7,"Peace",0,1,1,0,1,0,3);peace.version=9;peace.rivalWar=false;peace.UpgradeSave();
            Require(!peace.AtWar(1,2)&&peace.Valid(),"version 9 peace between rivals preserved");
        }
        static void VictoryChecks()
        {
            var w=World.New(31,"Victory Test",0,1,1,0,1,1,4);
            var rivals=w.SurvivingRivals();
            Require(rivals.Count==3&&w.concordTurns==0,"four civilisations start with three surviving rivals");
            foreach(int o in rivals){w.Relation(0,o).contact=true;w.met[o]=true;w.Relation(0,o).friendUntil=w.turn+100;}
            for(int i=0;i<World.ConcordTarget-1;i++){w.turn++;w.UpdateConcord();}
            Require(w.concordTurns==World.ConcordTarget-1&&w.result=="","concord counts consecutive turns without winning early");
            w.Relation(0,rivals[0]).friendUntil=0;w.turn++;w.UpdateConcord();
            Require(w.concordTurns==0,"a lapsed friendship resets the count");
            w.Relation(0,rivals[0]).friendUntil=w.turn+100;
            for(int i=0;i<World.ConcordTarget;i++){w.turn++;w.UpdateConcord();}
            Require(w.result.StartsWith("Concord"),"twenty consecutive turns of friendship win");
            float[] progress;var text=w.VictoryText(out progress);
            Require(text.Length==3&&progress.Length==3&&progress[2]==1f,"victory screen reports three bars");
            var lone=World.New(32,"Lone",0,1,1,0,1,1,3);lone.units.RemoveAll(a=>a.owner>0);lone.cities.RemoveAll(a=>a.owner>0);
            Require(lone.SurvivingRivals().Count==0,"eliminated rivals no longer count");
            var growth=World.New(33,"Growth",0,1,1,0,1,1,3);var city=growth.Found(growth.units.First(a=>a.owner==0&&a.type==0));
            Require(growth.FoodNeeded(city)==10+4*city.pop,"growth needs 10 + 4 x population food");
        }
        static void SphereChecks()
        {
            for(int size=0;size<World.SphereWidths.Length;size++){
                string name=World.MapSizeNames[size];
                var w=World.New(70+size,"Sphere Test",0,1,1,0,size,1);
                Require(w.Sphere&&w.Staggered&&w.Width==World.SphereHeights[size]&&w.Height==World.SphereWidths[size]&&w.Height==2*w.Width&&w.tiles.Length==w.Width*w.Height,name+" globe is W columns by 2W half-height rows (a 2:1 picture)");
                Require(w.Valid(),"new "+name+" sphere is valid");
                Require(w.cities.Count==2&&w.units.Count(u=>u.owner==0)==3,name+" sphere has the usual starting civilisations and units");
                Require(w.units.Where(u=>u.owner==0).All(u=>!World.IsImpassable(w.At(u.x,u.y).terrain)),"player starts on passable land on the "+name+" sphere");
                if(w.tiles.Length<=10000)for(int owner=0;owner<3;owner++)for(int i=0;i<w.tiles.Length;i++)if(w.nations[owner].known[i]!=w.VisibleTo(owner,i%w.Width,i/w.Width))throw new Exception("FAIL: incremental fog differs from full visibility on the "+name+" sphere");
                // All three civilisations must share one landmass so conquest is possible without ships.
                var start=w.cities.First(c=>c.owner==1);var reached=new HashSet<int>{start.y*w.Width+start.x};var queue=new Queue<Tile>();queue.Enqueue(w.At(start.x,start.y));
                while(queue.Count>0){var t=queue.Dequeue();for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=w.Step(t.x,t.y,dx,dy);if(n==null||World.IsImpassable(n.terrain)||!reached.Add(n.y*w.Width+n.x))continue;queue.Enqueue(n);}}
                Require(w.cities.All(c=>reached.Contains(c.y*w.Width+c.x))&&w.units.Where(u=>u.owner==0).All(u=>reached.Contains(u.y*w.Width+u.x)),"all "+name+" sphere starts are connected by land");
                int land=w.tiles.Count(t=>t.terrain!=4&&t.terrain!=12&&t.terrain!=14);
                Require(land*100>=w.tiles.Length*28&&land*100<=w.tiles.Length*50,name+" sphere has a believable land share ("+(land*100/w.tiles.Length)+"%)");
                Require(w.tiles.Count(t=>t.terrain==14)>0&&w.tiles.Any(t=>t.terrain==8||t.terrain==13)&&w.tiles.Any(t=>t.terrain==0)&&w.tiles.Any(t=>t.terrain==1),name+" sphere has polar ice, cold land, grass and forest");
                int incoherent=0,maxStep=0;
                for(int i=0;i<w.tiles.Length;i++){
                    var a=w.tiles[i];
                    for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){
                        var b=w.Step(a.x,a.y,dx,dy);if(b==null||(dx==0&&dy==0))continue;
                        maxStep=Math.Max(maxStep,Math.Abs(w.climateTemperature[i]-w.climateTemperature[b.y*w.Width+b.x]));
                        bool hot=a.terrain==11||a.terrain==3||a.terrain==16,cold=b.terrain==8||b.terrain==13||b.terrain==14;
                        if(hot&&cold)incoherent++;
                    }
                }
                Require(maxStep<=3,name+" sphere temperature changes gently between neighbours");
                Require(incoherent*200<=w.tiles.Length,name+" sphere never puts jungle or desert beside tundra or ice");
                if(w.tiles.Length<=10000)Require(World.New(70+size,"Sphere Test",0,1,1,0,size,1).tiles.Select(t=>t.terrain).SequenceEqual(w.tiles.Select(t=>t.terrain)),name+" sphere is repeatable by seed");
            }
            var s=World.New(5,"Sphere",0,1,1,0,1,1);
            Require(s.At(-1,3)==s.At(s.Width-1,3)&&s.At(s.Width,3)==s.At(0,3),"sphere wraps east-west");
            Require(s.At(4,-1)==null&&s.At(4,s.Height)==null,"staggered globe ends at the polar rows");
            Require(s.Distance(0,5,s.Width-1,5)==1&&s.Distance(2,5,s.Width-2,5)==4,"distance wraps east-west");
            Require(s.Distance(4,0,4,2)==1&&s.Distance(4,0,4,1)==1&&s.Distance(4,1,4,0)==1&&s.Distance(4,1,5,0)==1&&s.Distance(4,0,4,4)==2,"staggered neighbours: two rows down, the half-step rows and the next column are one move");
            {bool ok=true;var rng=new Random(3);for(int k=0;k<300&&ok;k++){int x=rng.Next(s.Width),y=1+rng.Next(s.Height-2);for(int dj=-1;dj<=1;dj++)for(int di=-1;di<=1;di++){if(di==0&&dj==0)continue;var n=s.Step(x,y,di,dj);if(n==null)continue;var back=s.Step(n.x,n.y,-di,-dj);if(back==null||back.x!=x||back.y!=y||s.Distance(x,y,n.x,n.y)!=1)ok=false;}
              var far=s.Step(x,y,rng.Next(-6,7),rng.Next(-6,7));if(far!=null&&s.Distance(x,y,far.x,far.y)!=s.Distance(far.x,far.y,x,y))ok=false;}
             Require(ok,"staggered steps are reversible, neighbours are one move apart and distance is symmetric");}
            Require(s.Distance(3,7,12,9)==s.Distance(12,9,3,7),"distance is symmetric");
            var flat=World.New(5);
            Require(!flat.Sphere&&flat.At(-1,3)==null&&flat.Distance(0,5,flat.Width-1,5)==flat.Width-1,"flat maps keep hard edges");
            // Units can walk across the seam and over a pole.
            s.units.RemoveAll(a=>(a.x==0||a.x==s.Width-1||a.y==0)&&a.owner>0);
            int w2=s.Width;
            foreach(var cell in new[]{s.At(0,10),s.At(w2-1,10),s.At(4,0),s.At(4+w2/2,0)}){cell.terrain=0;cell.owner=-1;s.units.RemoveAll(a=>a.x==cell.x&&a.y==cell.y);}
            var walker=s.Add(0,2,0,10);walker.moves=4;
            Require(s.Move(walker,-1,10)&&walker.x==w2-1&&walker.y==10,"a unit steps west across the seam");
            var climber=s.Add(0,2,4,0);climber.moves=4;
            Require(!s.Move(climber,4,-1)&&climber.x==4&&climber.y==0,"no step beyond the north pole on a staggered globe");
            // Saves migrate: version 8 becomes version 9 and stays flat.
            var old=World.New(8);old.version=8;old.UpgradeSave();
            Require(old.version==19&&old.mapShape==0&&old.Valid(),"version 8 saves upgrade to flat version 19");
        }
        static void MapSizeChecks()
        {
            for(int size=0;size<World.MapWidths.Length;size++){
                string name=World.MapSizeNames[size];
                var w=World.New(40+size,"Map Test",0,1,1,0,size);
                Require(w.Width==World.MapWidths[size]&&w.Height==World.MapHeights[size]&&w.tiles.Length==w.Width*w.Height,name+" map has the right dimensions");
                Require(w.Valid(),"new "+name+" world is valid");
                Require(w.nations.All(n=>n.known.Length==w.tiles.Length),"known-tile arrays match the "+name+" map");
                Require(w.cities.Count==2&&w.units.Count(u=>u.owner==0)==3,name+" map has the usual starting civilisations and units");
                Require(w.units.Where(u=>u.owner==0).All(u=>!World.IsImpassable(w.At(u.x,u.y).terrain)),"player starts on passable land on the "+name+" map");
                if(w.tiles.Length<=10000)for(int owner=0;owner<3;owner++)for(int i=0;i<w.tiles.Length;i++)if(w.nations[owner].known[i]!=w.VisibleTo(owner,i%w.Width,i/w.Width))throw new Exception("FAIL: incremental fog differs from full visibility on the "+name+" map");
            }
            Require(World.New(9).Width==48&&World.New(9).Height==30,"default map stays Small 48 x 30");
            Require(World.New(9,"A",0,1,1,0,1).tiles.Select(t=>t.terrain).SequenceEqual(World.New(9).tiles.Select(t=>t.terrain)),"Small is the default size and repeatable by seed");
            var legacy=World.New(3);legacy.version=7;legacy.mapWidth=0;legacy.mapHeight=0;legacy.UpgradeSave();
            Require(legacy.Width==48&&legacy.Height==30&&legacy.version==19&&legacy.Valid(),"version 7 saves upgrade to Small flat version 19");
        }
    }
}
