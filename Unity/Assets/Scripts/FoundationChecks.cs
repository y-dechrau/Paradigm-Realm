using System;
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
            Require(old.version==8&&oc.build==1&&oc.queue.SequenceEqual(new[]{1})&&oc.production==0&&oc.buildingProject==8&&oc.buildingProduction==17&&oc.buildingQueue.SequenceEqual(new[]{7,9})&&old.Valid(),"version-3 mixed queue migration preserves both category orders and active progress");
            old.UpgradeSave();Require(oc.buildingProduction==17&&oc.queue.Count==1,"migration is idempotent");
            var upgrade=Flat();var uc=upgrade.Found(upgrade.units.First(v=>v.owner==0&&v.type==0));var troop=upgrade.units.First(v=>v.owner==0&&v.type==2);upgrade.tech[6]=true;upgrade.gold=100;
            Require(!upgrade.Upgrade(troop),"owned territory alone does not permit upgrades");troop.x=uc.x;troop.y=uc.y;Require(upgrade.Upgrade(troop)&&troop.type==7,"owned city permits eligible upgrade");
            var other=upgrade.units.First(v=>v.owner==0&&v.type==1);int otherMoves=other.moves;upgrade.SkipTurn(troop);Require(troop.moves==0&&other.moves==otherMoves,"skip affects only selected unit");
            Require(World.StackLabel(new[]{new Unit{type=1}})=="Worker"&&World.StackLabel(new[]{new Unit{type=1},new Unit{type=1}})=="Stack"&&World.StackLabel(new[]{new Unit{type=1},new Unit{type=2}})=="","unit label rules");
            MapSizeChecks();
            ContentChecks.Run();
            var victory=World.New(22);victory.units.RemoveAll(a=>a.owner>0);victory.cities.RemoveAll(a=>a.owner>0);victory.CheckVictory();Require(victory.result.StartsWith("Conquest"),"conquest detected");
            var defeat=World.New(23);defeat.units.RemoveAll(a=>a.owner==0);defeat.CheckVictory();Require(defeat.result.Contains("fallen"),"defeat detected");
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
                for(int owner=0;owner<3;owner++)for(int i=0;i<w.tiles.Length;i++)if(w.nations[owner].known[i]!=w.VisibleTo(owner,i%w.Width,i/w.Width))throw new Exception("FAIL: incremental fog differs from full visibility on the "+name+" map");
            }
            Require(World.New(9).Width==48&&World.New(9).Height==30,"default map stays Small 48 x 30");
            Require(World.New(9,"A",0,1,1,0,1).tiles.Select(t=>t.terrain).SequenceEqual(World.New(9).tiles.Select(t=>t.terrain)),"Small is the default size and repeatable by seed");
            var legacy=World.New(3);legacy.version=7;legacy.mapWidth=0;legacy.mapHeight=0;legacy.UpgradeSave();
            Require(legacy.Width==48&&legacy.Height==30&&legacy.version==8&&legacy.Valid(),"version 7 saves upgrade to Small version 8");
        }
    }
}
