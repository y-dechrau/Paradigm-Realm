using System;
using System.Linq;

namespace ParadigmRealm
{
    // Pure C# checks, callable in Unity from Paradigm Realm > Run Engine Checks.
    public static class FoundationChecks
    {
        static void Require(bool condition,string message){if(!condition)throw new Exception("FAIL: "+message);}
        static World Flat(){var w=World.New(73);foreach(var t in w.tiles){t.terrain=0;t.road=false;t.resource=0;t.seen=true;for(int n=0;n<3;n++)w.nations[n].known[t.y*World.Width+t.x]=true;}return w;}
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
            c.build=-1;Require(w.QueueBuild(c,7)&&w.QueueBuild(c,2),"queue accepts valid projects");Require(!w.QueueBuild(c,7),"duplicate building rejected");c.production=w.Cost(7);w.EndTurn();Require(c.granary&&c.build==2,"completion advances queue");
            c.pop=8;w.nations[0].tax=0;int low=w.Happiness(c),lowTax=w.Budget(0)[0];w.nations[0].tax=2;Require(w.Happiness(c)==low-4&&w.Budget(0)[0]>lowTax,"taxes trade happiness for revenue");
            u.experience=5;Require(w.Promote(u)&&u.promotion==1&&u.experience==0,"promotion consumes experience");
            var neutral=w.Add(1,2,u.x,u.y+1);w.Reveal();u.moves=World.Moves[u.type];Require(!w.Move(u,neutral.x,neutral.y),"peace blocks combat");Require(w.SetWar(1,true),"contact enables declaration");Require(w.Move(u,neutral.x,neutral.y)&&neutral.hp<10,"war enables combat");
            var research=World.New(81);research.science=12;research.SelectResearch(1);research.science=7;research.SelectResearch(0);Require(research.science==12&&research.nations[0].progress[1]==7,"switching preserves individual research progress");
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
            var victory=World.New(22);victory.units.RemoveAll(a=>a.owner>0);victory.cities.RemoveAll(a=>a.owner>0);victory.CheckVictory();Require(victory.result.StartsWith("Conquest"),"conquest detected");
            var defeat=World.New(23);defeat.units.RemoveAll(a=>a.owner==0);defeat.CheckVictory();Require(defeat.result.Contains("fallen"),"defeat detected");
        }
    }
}
