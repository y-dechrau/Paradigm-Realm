using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Timber and stone drawn as work goes (owner brief, 26 Nov 2026): the owner's acceptance scenario on the allocation itself, then
// the same rules in a real game: ledger against stockpile, waiting, cancelling, completion, bridges, rivals, saving and loading.
public static class MaterialEconomyChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Materials: "+what);}
 static MatJob Harbour(int started){return new MatJob{key="harbour",name="Harbour",kind=0,totalT=30,totalS=40,work=100,rate=10,started=started};}
 static MatJob Bridge(int left,int started){return new MatJob{key="bridge",name="Frontier bridge",kind=2,totalT=4*(left+2),work=left+2,done=2,instT=8,rate=1,started=started};}
 static MatJob Galley(int started){return new MatJob{key="galley",name="Galley",kind=1,totalT=16,work=4,rate=1,started=started};}
 public static void Run(){
  var inc=new[]{8,5};
  // 1. The ledger before the galley: 8 produced - 3 harbour - 4 bridge = 1 timber added; 5 - 4 = 1 stone.
  {var jobs=new List<MatJob>{Harbour(1),Bridge(3,2)};int t=12+8,s=20+5;World.AllocateForUI(jobs,ref t,ref s);
   Check(t-12==1&&s-20==1,"before the galley one timber and one stone are added to storage ("+(t-12)+", "+(s-20)+")");}
  // 2. With the galley: 12, 9, 6, 3, then the bridge is done and stock rises to 4 as the galley finishes.
  {var trace=new List<int[]>();var f=World.ForecastJobs(new List<MatJob>{Harbour(1),Bridge(3,2),Galley(3)},12,20,inc,4,0,trace);
   Check(trace.Select(x=>x[0]).SequenceEqual(new[]{9,6,3,4}),"timber goes 12, 9, 6, 3, 4 ("+string.Join(", ",trace.Select(x=>x[0].ToString()).ToArray())+")");
   Check(f.First(j=>j.key=="bridge").finish==3&&f.First(j=>j.key=="galley").finish==4&&f.All(j=>j.firstWait<0),"the bridge finishes on turn 3 and the galley on turn 4, nothing waits");}
  // 3. A real shortage: 3 timber stored, six turns left on the bridge. Turn 1 is supplied in full, turn 2 has 8 against 11.
  Func<bool,List<MatJob>> shortage=galleyUrgent=>{var js=new List<MatJob>{Harbour(1),Bridge(6,2),Galley(3)};js[2].urgent=galleyUrgent;return js;};
  {var js=shortage(false);int t=3+8,s=20+5;World.AllocateForUI(js,ref t,ref s);Check(t==0&&js.All(j=>j.shortT==0),"turn one is fully supplied and leaves nothing");
   foreach(var j in js)j.done=j.reach;t+=8;s+=5;World.AllocateForUI(js,ref t,ref s);
   var g=js.First(j=>j.key=="galley");var b=js.First(j=>j.key=="bridge");Check(g.reach==g.done&&g.shortT>0&&b.reach>b.done,"by the rule (earliest started first) the newest work, the galley, waits on turn two");
   var pre=World.ForecastJobs(shortage(false),3,20,inc,6,0,null);Check(pre.First(j=>j.key=="galley").firstWait==2&&pre.First(j=>j.key=="bridge").firstWait<0,"the preview says the same: the galley waits from turn two, the bridge never");}
  {var js=shortage(true);int t=3+8,s=20+5;World.AllocateForUI(js,ref t,ref s);foreach(var j in js)j.done=j.reach;t+=8;s+=5;World.AllocateForUI(js,ref t,ref s);
   var g=js.First(j=>j.key=="galley");var b=js.First(j=>j.key=="bridge");Check(g.reach>g.done&&b.reach==b.done&&b.shortT>0,"marked urgent, the galley is supplied and the bridge waits instead");
   var pre=World.ForecastJobs(shortage(true),3,20,inc,6,0,null);Check(pre.First(j=>j.key=="bridge").firstWait==2&&pre.First(j=>j.key=="galley").firstWait<0,"and the preview agrees");}
  // 4. Exact amounts: an uneven split never over- or under-charges, and a mixed project stops at whichever runs out first.
  {var j=new MatJob{key="odd",totalT=7,totalS=5,work=3,rate=1};int t=100,s=100,used=0;for(int k=0;k<3;k++){int before=t;World.AllocateForUI(new List<MatJob>{j},ref t,ref s);used+=before-t;j.done=j.reach;}
   Check(j.done==3&&j.instT==7&&j.instS==5&&used==7,"7 timber over 3 turns is charged exactly 7");
   var m=new MatJob{key="mixed",totalT=10,totalS=10,work=10,rate=10};int t2=10,s2=3;World.AllocateForUI(new List<MatJob>{m},ref t2,ref s2);Check(m.reach==3&&m.instT==3&&m.instS==3&&t2==7,"short of stone, timber is not taken for work that did not happen");}
  // 5. In a game: ledger against stock, waiting, completion with every piece installed, refunds, bridges, saving.
  var w=World.New(353,"Ledger",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.nations[0].tech[TechnologyCatalog.Index("pottery")]=true;w.EnableMaterials();
  var n=w.nations[0];n.gold=900;w.gold=900;foreach(var u in w.units.Where(u=>u.owner==0).ToList())w.units.Remove(u);
  w.ChooseProject(c,7,false);w.SettleBuilding(c);int needT=w.BuildingTimber(7),needS=w.BuildingStone(7);Check(c.paidFor==7&&c.paidTimber==0&&c.paidStone==0,"starting a Granary pays its gold only");
  n.timber=0;n.stone=0;w.incomeForChecks=new[]{1,1};int steps=0,usedT=0,usedS=0;
  while(!c.granary&&steps<80){int t0=n.timber,s0=n.stone;w.EndTurn();steps++;var led=w.ledger;usedT+=led.usedTimber;usedS+=led.usedStone;
   Check(led.turn>=0&&led.openTimber==t0&&led.closeTimber==n.timber&&led.closeTimber==led.openTimber+led.madeTimber-led.usedTimber&&led.closeStone==led.openStone+led.madeStone-led.usedStone,"the ledger matches the stockpile (turn "+w.turn+")");
   Check(n.timber>=0&&n.stone>=0&&c.paidTimber<=needT&&c.paidStone<=needS,"stocks never go below nothing and nothing is over-installed");}
  Check(c.granary&&usedT==needT&&usedS==needS,"a Granary built on a trickle of materials finishes having drawn exactly its "+needT+" timber and "+needS+" stone ("+usedT+", "+usedS+"; "+steps+" turns)");
  // Cancelling returns what is installed; the same work is never charged twice.
  w.incomeForChecks=new[]{0,0};n.timber=50;n.stone=50;n.gold=900;w.gold=900;int wb=World.BuildingIds.Where(b=>!w.HasBuilding(c,b)&&w.BuildReason(c,b)==""&&w.BuildingTimber(b)+w.BuildingStone(b)>0&&!World.IsWonder(b)).OrderBy(b=>w.CostFor(c,b)).First();w.ChooseProject(c,wb,false);w.SettleBuilding(c);for(int k=0;k<12&&c.paidTimber+c.paidStone==0;k++)w.EndTurn();int inT=c.paidTimber,inS=c.paidStone,haveT=n.timber,haveS=n.stone;
  Check(inT+inS>0,World.BuildName(wb)+" draws materials as it goes");w.CancelProject(c,wb);w.SettleBuilding(c);Check(n.timber==haveT+inT&&n.stone==haveS+inS&&c.paidTimber==0,"cancelling returns exactly what was installed");
  w.SettleBuilding(c);Check(n.timber==haveT+inT,"and only once");
  // A bridge: a road on a river tile takes timber as the work goes and is not finished without it.
  var river=w.tiles.FirstOrDefault(t=>t.terrain==7&&t.owner==0&&!t.road);
  if(river!=null){var worker=w.Add(0,1,river.x,river.y);n.timber=0;worker.moves=1;string why=w.JobReason(worker,1);if(why==""){w.StartJob(worker,1);w.EndTurn();Check(!river.road&&worker.job==1,"a bridge with no timber waits");
    n.timber=World.BridgeTimber;for(int k=0;k<8&&!river.road;k++)w.EndTurn();Check(river.road&&w.siteStocks.Count(s=>s.cell==river.y*w.Width+river.x)==0,"with "+World.BridgeTimber+" timber the bridge is finished");}}
  // Saving and loading keeps installed materials, urgency and the ledger; an older save keeps what it paid.
  n.gold=900;w.gold=900;w.ChooseProject(c,wb,false);w.SettleBuilding(c);n.timber=30;n.stone=30;w.EndTurn();w.SetUrgent(0,"b"+c.id+":"+wb,true);
  var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));var cc=copy.cities.First(x=>x.id==c.id);
  Check(cc.paidTimber==c.paidTimber&&cc.paidStone==c.paidStone&&copy.IsUrgent(0,"b"+c.id+":"+wb)&&copy.ledger.closeTimber==w.ledger.closeTimber,"installed materials, urgency and the ledger are saved");
  {var old=World.New(354,"Old",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);var oc=old.Found(old.units.First(u=>u.owner==0&&u.type==0));old.nations[0].tech[TechnologyCatalog.Index("pottery")]=true;old.EnableMaterials();old.nations[0].gold=500;old.gold=500;
   old.ChooseProject(oc,7,false);old.SettleBuilding(oc);oc.paidTimber=old.BuildingTimber(7);oc.paidStone=old.BuildingStone(7);old.siteStocks=null;old.materialCommits=null;old.ledger=null;
   int t0=old.nations[0].timber;old.incomeForChecks=new[]{0,0};old.EndTurn();Check(old.nations[0].timber==t0&&old.Valid(),"an older save's paid-up building draws nothing more");}
  // Rivals keep building, and every stock stays whole.
  var r=World.New(355,"Rivals",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);r.EnableMaterials();for(int k=0;k<60;k++)r.EndTurn();
  Check(r.Valid()&&r.nations.All(x=>x.timber>=0&&x.stone>=0)&&r.cities.Where(x=>x.owner>0).Sum(x=>x.buildings.Count+(x.granary?1:0)+(x.library?1:0)+(x.walls?1:0))>0,"rivals still build in a 60-turn game");
 }
}
}
