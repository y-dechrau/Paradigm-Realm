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
            w.tech[0]=true;var worker=w.units.First(a=>a.owner==0&&a.type==1);
            Require(w.StartJob(worker,1),"worker can start road");w.EndTurn();Require(!w.At(worker.x,worker.y).road,"road not instant");w.EndTurn();Require(w.At(worker.x,worker.y).road&&worker.job==0,"job finishes after two turns");
            w.tech[6]=true;var ore=w.At(c.x+1,c.y);ore.resource=2;ore.mine=true;ore.road=true;w.RebuildTerritory();
            Require(w.HasResource(0,2),"improved road-connected iron");ore.road=false;w.RebuildTerritory();Require(!w.HasResource(0,2),"cut road removes access");
            c.build=-1;Require(w.QueueBuild(c,7)&&w.QueueBuild(c,2),"queue accepts valid projects");Require(!w.QueueBuild(c,7),"duplicate building rejected");c.buildingProduction=w.Cost(7);w.EndTurn();Require(c.granary&&c.build==2,"completion advances queue");
            c.pop=8;w.nations[0].tax=0;int low=w.Happiness(c),lowTax=w.Budget(0)[0];w.nations[0].tax=2;Require(w.Happiness(c)==low-4&&w.Budget(0)[0]>lowTax,"taxes trade happiness for revenue");
            u.experience=5;Require(w.Promote(u)&&u.promotion==1&&u.experience==0,"promotion consumes experience");
            var neutral=w.Add(1,2,u.x,u.y+1);w.Reveal();u.moves=World.Moves[u.type];Require(!w.Move(u,neutral.x,neutral.y),"peace blocks combat");Require(w.SetWar(1,true),"contact enables declaration");Require(w.Move(u,neutral.x,neutral.y)&&neutral.hp<10,"war enables combat");
            var research=World.New(81);research.tech[TechnologyCatalog.Index("toolmaking")]=true;int fire=TechnologyCatalog.Index("fire"),hunting=TechnologyCatalog.Index("hunting");research.SelectResearch(fire);research.science=12;research.SelectResearch(hunting);research.science=7;research.SelectResearch(fire);Require(research.science==12&&research.nations[0].progress[hunting]==7,"switching preserves individual research progress");
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
            Require(old.version==9&&oc.build==1&&oc.queue.SequenceEqual(new[]{1})&&oc.production==0&&oc.buildingProject==8&&oc.buildingProduction==17&&oc.buildingQueue.SequenceEqual(new[]{7,9})&&old.Valid(),"version-3 mixed queue migration preserves both category orders and active progress");
            old.UpgradeSave();Require(oc.buildingProduction==17&&oc.queue.Count==1,"migration is idempotent");
            var upgrade=Flat();var uc=upgrade.Found(upgrade.units.First(v=>v.owner==0&&v.type==0));var troop=upgrade.units.First(v=>v.owner==0&&v.type==2);upgrade.tech[6]=true;upgrade.gold=100;
            Require(!upgrade.Upgrade(troop),"owned territory alone does not permit upgrades");troop.x=uc.x;troop.y=uc.y;Require(upgrade.Upgrade(troop)&&troop.type==7,"owned city permits eligible upgrade");
            var other=upgrade.units.First(v=>v.owner==0&&v.type==1);int otherMoves=other.moves;upgrade.SkipTurn(troop);Require(troop.moves==0&&other.moves==otherMoves,"skip affects only selected unit");
            Require(World.StackLabel(new[]{new Unit{type=1}})=="Worker"&&World.StackLabel(new[]{new Unit{type=1},new Unit{type=1}})=="Stack"&&World.StackLabel(new[]{new Unit{type=1},new Unit{type=2}})=="","unit label rules");
            MapSizeChecks();
            SphereChecks();
            SpeedChecks();
            PixelChecks();
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
            int[] none={-1,-1,-1,-1};
            for(int t=0;t<21;t++){
                var a=PixelArt.Compose(t,0,none,0,0);var b=PixelArt.Compose(t,0,none,0,0);
                Require(a.w==PixelArt.CW&&a.h==PixelArt.CH,"terrain "+t+" composite has the expected size");
                bool same=true;for(int i=0;i<a.p.Length;i++)if(a.p[i]!=b.p[i]){same=false;break;}
                Require(same,"terrain "+t+" art is deterministic");
                int opaque=0;foreach(uint c in a.p)if((c>>24)>0)opaque++;
                Require(opaque>400,"terrain "+t+" composite is not empty");
                var sp=PixelArt.Sprite(t,1);Require(sp==null||(sp.w==PixelArt.SW&&sp.h==PixelArt.SH),"terrain "+t+" sprite size");
            }
            Require(PixelArt.NeighbourCode(0,1,-1)==-1&&PixelArt.NeighbourCode(0,1,4)==4,"neighbour codes for unknown and water");
            Require(PixelArt.Farm(0).w==PixelArt.TW&&PixelArt.Road(5).w==PixelArt.TW,"farm and road overlays are tile sized");
            // The diamond screen mapping must invert exactly: tile centres map back to the same lattice point.
            float tw=64,th=32;
            for(int i=-3;i<=3;i++)for(int j=-3;j<=3;j++){
                float dx=(i-j)*tw/2,dy=(i+j)*th/2,u=dx/(tw/2),v=dy/(th/2);
                Require((int)Math.Round((u+v)/2)==i&&(int)Math.Round((v-u)/2)==j,"diamond pick round trip "+i+","+j);
            }
        }
        static void SphereChecks()
        {
            for(int size=0;size<World.SphereWidths.Length;size++){
                string name=World.MapSizeNames[size];
                var w=World.New(70+size,"Sphere Test",0,1,1,0,size,1);
                Require(w.Sphere&&w.Width==World.SphereWidths[size]&&w.Height==World.SphereHeights[size]&&w.Width==2*w.Height&&w.tiles.Length==w.Width*w.Height,name+" sphere is a 2:1 grid");
                Require(w.Valid(),"new "+name+" sphere is valid");
                Require(w.cities.Count==2&&w.units.Count(u=>u.owner==0)==3,name+" sphere has the usual starting civilisations and units");
                Require(w.units.Where(u=>u.owner==0).All(u=>!World.IsImpassable(w.At(u.x,u.y).terrain)),"player starts on passable land on the "+name+" sphere");
                if(w.tiles.Length<=10000)for(int owner=0;owner<3;owner++)for(int i=0;i<w.tiles.Length;i++)if(w.nations[owner].known[i]!=w.VisibleTo(owner,i%w.Width,i/w.Width))throw new Exception("FAIL: incremental fog differs from full visibility on the "+name+" sphere");
                // All three civilisations must share one landmass so conquest is possible without ships.
                var start=w.cities.First(c=>c.owner==1);var reached=new HashSet<int>{start.y*w.Width+start.x};var queue=new Queue<Tile>();queue.Enqueue(w.At(start.x,start.y));
                while(queue.Count>0){var t=queue.Dequeue();for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=w.At(t.x+dx,t.y+dy);if(n==null||World.IsImpassable(n.terrain)||!reached.Add(n.y*w.Width+n.x))continue;queue.Enqueue(n);}}
                Require(w.cities.All(c=>reached.Contains(c.y*w.Width+c.x))&&w.units.Where(u=>u.owner==0).All(u=>reached.Contains(u.y*w.Width+u.x)),"all "+name+" sphere starts are connected by land");
                int land=w.tiles.Count(t=>t.terrain!=4&&t.terrain!=12&&t.terrain!=14);
                Require(land*100>=w.tiles.Length*28&&land*100<=w.tiles.Length*50,name+" sphere has a believable land share");
                Require(w.tiles.Count(t=>t.terrain==14)>0&&w.tiles.Any(t=>t.terrain==8||t.terrain==13)&&w.tiles.Any(t=>t.terrain==0)&&w.tiles.Any(t=>t.terrain==1),name+" sphere has polar ice, cold land, grass and forest");
                int incoherent=0,maxStep=0;
                for(int i=0;i<w.tiles.Length;i++){
                    var a=w.tiles[i];
                    for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){
                        var b=w.At(a.x+dx,a.y+dy);if(b==null||(dx==0&&dy==0))continue;
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
            Require(s.At(4,-1)==s.At(4+s.Width/2,0)&&s.At(4,s.Height)==s.At(4+s.Width/2,s.Height-1)&&s.At(4,-s.Height-1)==null,"sphere crosses both poles");
            Require(s.Distance(0,5,s.Width-1,5)==1&&s.Distance(2,5,s.Width-2,5)==4,"distance wraps east-west");
            Require(s.Distance(4,0,4+s.Width/2,0)==1&&s.Distance(s.Width/4,0,s.Width/4+s.Width/2,3)==4&&s.Distance(4,s.Height-1,4+s.Width/2,s.Height-1)==1,"distance crosses the poles");
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
            Require(s.Move(climber,4,-1)&&climber.x==4+w2/2&&climber.y==0,"a unit steps over the north pole");
            // Saves migrate: version 8 becomes version 9 and stays flat.
            var old=World.New(8);old.version=8;old.UpgradeSave();
            Require(old.version==9&&old.mapShape==0&&old.Valid(),"version 8 saves upgrade to flat version 9");
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
            Require(legacy.Width==48&&legacy.Height==30&&legacy.version==9&&legacy.Valid(),"version 7 saves upgrade to Small flat version 9");
        }
    }
}
