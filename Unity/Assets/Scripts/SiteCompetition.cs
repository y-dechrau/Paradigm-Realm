using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Resource competition, first part (owner brief, 26 Nov 2026): a woodland or quarry beyond your borders matters because of what
// it does for your own work now, judged by each civilisation from its own knowledge:
//   1. Do we know what is here?          the civilisation has seen the tile (Nation.known)
//   2. Do we know how to use it?         timber needs Stone Tools, stone needs Mining; and something it is building needs it
//   3. Can we get it and bring it home?  a worker can build a camp: unclaimed land, or land of a people that lets us pass, within
//                                        CampReach steps of our own land over ground we may cross (a river without Bridge
//                                        Building counts as two steps; another people's land without passage blocks the way)
//   4. Is it worth it now?               the same forecast as the city lines (MaterialEconomy.cs), with the camp's output added
//                                        once the worker could have it running, compared with no camp
// A camp (worker job 6) gives a right to work the site, not the land: borders do not change, and if the land becomes another
// people's without passage, or the way home is cut, it delivers nothing until that changes. The alternatives are weighed in
// the same terms: a camp, a settlement nearby, buying a shipment from a people that can spare it (judged by the seller's own
// forecast), passage through the land in the way, or carrying on with what you have.
// Not built: gateway trade beyond the gates, special timber grades, finished components (planks, ship timbers); see the status
// document.
[Serializable] public class Outpost{public int owner,cell,kind,built;}
public class SiteOption{public string title="",detail="";public int delivery=-1;public string action="";}
public class SiteReport{public bool known,understood,reachable;public int kind=-1;public List<string> lines=new List<string>();public List<SiteOption> options=new List<SiteOption>();}
public partial class World {
 public List<Outpost> outposts=new List<Outpost>();
 public const int CampJob=6,CampReach=6,CampWork=3,MaterialPrice=2;
 List<Outpost> Outposts(){if(outposts==null)outposts=new List<Outpost>();return outposts;}
 public Outpost OutpostAt(int x,int y){int cell=y*Width+x;return Outposts().Find(o=>o.cell==cell);}
 // What a camp here would gather: 0 timber, 1 stone, -1 nothing (for this civilisation's knowledge).
 public int CampKind(int owner,Tile t){if(t==null||IsWater(t.terrain)||IsImpassable(t.terrain))return -1;
  if(t.resource==MaterialRules.StoneResource&&ResourceKnown(owner,t.resource)||t.terrain==2&&!Wooded(t))return HasTech(owner,"mining")?1:-1;
  if(Wooded(t)||t.resource==MaterialRules.TimberResource&&ResourceKnown(owner,t.resource))return HasTech(owner,"toolmaking")?0:-1;return -1;}
 public int CampYield(int owner,Tile t,int kind){int y=2;if(kind==0&&t.resource==MaterialRules.TimberResource&&ResourceKnown(owner,t.resource))y++;if(kind==1&&t.resource==MaterialRules.StoneResource&&ResourceKnown(owner,t.resource))y++;if(t.road)y++;return y;}
 // The way home: steps to our own land, a river without Bridge Building counting double; who blocks it if nobody lets us pass.
 public int HomeSteps(int owner,Tile from,bool ignoreBorders,out int blocker){
  blocker=-1;var dist=new Dictionary<int,int>{{from.y*Width+from.x,0}};var open=new List<Tile>{from};int realm=from.realm;
  while(open.Count>0){var t=open.OrderBy(a=>dist[a.y*Width+a.x]).First();open.Remove(t);int d=dist[t.y*Width+t.x];if(t.owner==owner)return d;
   for(int k=0;k<6;k++){var n=Step(t.x,t.y,HexOr(k,true),HexOr(k,false));if(n==null||n.realm!=realm||IsWater(n.terrain)||IsImpassable(n.terrain))continue;
    if(!ignoreBorders&&n.owner>=0&&n.owner!=owner&&!CanEnter(owner,n)){if(blocker<0)blocker=n.owner;continue;}
    int step=n.terrain==7&&!HasBridges(owner)&&!n.road?2:1;int nd=d+step;if(nd>CampReach)continue;int cell=n.y*Width+n.x;int old;if(dist.TryGetValue(cell,out old)&&old<=nd)continue;dist[cell]=nd;if(!open.Contains(n))open.Add(n);}}
  return -1;}
 // Why a camp cannot be set up here by this worker ("" when it can).
 public string CampReason(Unit u){if(u==null||u.type!=1||u.moves<=0)return "Worker needs movement.";return CampSiteReason(u.owner,At(u.x,u.y));}
 public string CampSiteReason(int owner,Tile t){
  if(t==null)return "No land here.";if(CityCentres().Contains(t.y*Width+t.x))return "A city stands here.";int kind=CampKind(owner,t);
  if(kind<0)return Wooded(t)||t.terrain==2||t.resource==MaterialRules.TimberResource||t.resource==MaterialRules.StoneResource?(Wooded(t)?"Requires Stone Tools.":"Requires Mining."):"Only woodland, hills or a timber or stone resource can have a camp.";
  if(t.owner==owner)return "Inside your borders your cities work it directly.";if(OutpostAt(t.x,t.y)!=null)return "There is already a camp here.";
  if(t.owner>=0&&!CanEnter(owner,t))return NationName(t.owner)+"'s land: needs an open borders agreement with them.";
  int blocker;if(HomeSteps(owner,t,false,out blocker)<0){int b2;bool through=HomeSteps(owner,t,true,out b2)>=0;return through&&blocker>=0?"The way home runs through "+NationName(blocker)+"'s land: open borders with them would open it.":"Too far from your land to bring anything home (more than "+CampReach+" steps).";}
  return "";}
 void BuildCamp(int owner,Tile t){int kind=CampKind(owner,t);if(kind<0)return;deliverMemo=null;Outposts().Add(new Outpost{owner=owner,cell=t.y*Width+t.x,kind=kind,built=turn});if(owner==0)Note("Workers set up a "+(kind==0?"timber":"stone")+" camp. It sends "+CampYield(owner,t,kind)+" "+(kind==0?"timber":"stone")+" home each turn while the way is open.");
  // A people that wanted the site minds; the more it needed it, the more (only if it can see the camp).
  for(int o=1;o<civCount;o++){if(o==owner||!met[o]&&owner==0||!VisibleTo(o,t.x,t.y)||IsNativeNation(o))continue;int gain=CampBenefit(o,t,kind,0);if(gain>0)AddOpinion(o,owner,OpinionMods.Declined,-Math.Min(4,1+gain/3));}}
 // A camp delivers while its land is open to us and the way home is open.
 [NonSerialized] Dictionary<int,bool> deliverMemo;[NonSerialized] int deliverTurn=-1;
 public void ForgetCampRoutes(){deliverMemo=null;}
 public bool OutpostDelivers(Outpost p){if(deliverTurn!=turn||deliverMemo==null){deliverMemo=new Dictionary<int,bool>();deliverTurn=turn;}bool v;if(deliverMemo.TryGetValue(p.cell*8+p.owner,out v))return v;v=OutpostDeliversNow(p);deliverMemo[p.cell*8+p.owner]=v;return v;}
 bool OutpostDeliversNow(Outpost p){var t=tiles[p.cell];if(t.owner>=0&&t.owner!=p.owner&&!CanEnter(p.owner,t))return false;if(t.owner==p.owner)return true;int b;return HomeSteps(p.owner,t,false,out b)>=0;}
 // {timber, stone} from camps this turn, and named lines for the ledger.
 public int[] OutpostIncome(int owner,List<string> lines){var r=new int[2];foreach(var p in Outposts()){if(p.owner!=owner)continue;var t=tiles[p.cell];bool ok=OutpostDelivers(p);int y=ok?CampYield(owner,t,p.kind):0;r[p.kind]+=y;if(lines!=null)lines.Add((p.kind==0?"Timber":"Stone")+" camp near "+NearestCityName(owner,t.x,t.y)+": "+(ok?y+" "+(p.kind==0?"timber":"stone"):"nothing (the way home or the land is closed)"));}return r;}
 // How many turns sooner the work under way would finish with a camp here running from turn `from` (0: as soon as it could be).
 public int CampBenefit(int owner,Tile t,int kind,int from){var b=CampForecast(owner,t,kind,from<=0?CampWork+1:from);return b.Sum(x=>Math.Max(0,x.Value[0]-x.Value[1]));}
 // For each project: {finish without, finish with} the camp (turns from now; 99 beyond the forecast).
 public Dictionary<string,int[]> CampForecast(int owner,Tile t,int kind,int from){
  var res=new Dictionary<string,int[]>();if(!buildingMaterials)return res;var jobs=MaterialJobs(owner);if(jobs.Count==0)return res;
  var inc=MaterialIncome(owner);var bonus=new int[2];bonus[kind]=CampYield(owner,t,kind);
  var a=ForecastJobs(jobs.Select(j=>j.Copy()).ToList(),nations[owner].timber,nations[owner].stone,inc,40,turn,null);
  var b=ForecastJobsFrom(jobs.Select(j=>j.Copy()).ToList(),nations[owner].timber,nations[owner].stone,inc,bonus,from,40);
  foreach(var j in a){var k=b.FirstOrDefault(x=>x.key==j.key);if(k==null)continue;res[j.name]=new[]{j.finish<0?99:j.finish,k.finish<0?99:k.finish};}
  return res;}
 static List<MatJob> ForecastJobsFrom(List<MatJob> jobs,int timber,int stone,int[] inc,int[] bonus,int from,int horizon){
  var all=new List<MatJob>(jobs);var active=new List<MatJob>(jobs);
  for(int k=1;k<=horizon&&active.Count>0;k++){timber+=inc[0]+(k>=from?bonus[0]:0);stone+=inc[1]+(k>=from?bonus[1]:0);Allocate(active.Where(j=>j.totalT+j.totalS>0).ToList(),ref timber,ref stone);
   foreach(var j in active.ToList()){j.done=j.reach;if(j.done>=j.work&&j.instT>=j.totalT&&j.instS>=j.totalS){j.finish=k;active.Remove(j);}}}
  return all;}
 // ---- Buying a shipment ----
 // What a people can spare of one material: its stock less what its own work will want in the next ten turns, after its income.
 public int Spare(int seller,int kind){var inc=MaterialIncome(seller);var jobs=MaterialJobs(seller);int need=jobs.Sum(j=>Math.Max(0,(kind==0?j.totalT-j.instT:j.totalS-j.instS)));int have=(kind==0?nations[seller].timber:nations[seller].stone)+inc[kind]*10;return Math.Max(0,have-need-5);}
 public int ShipmentPrice(int seller,int kind,int amount){int spare=Spare(seller,kind);return amount*MaterialPrice+(spare<amount*2?amount:0);}
 public string ShipmentReason(int buyer,int seller,int kind,int amount){if(seller<=0&&buyer==0||seller==buyer)return "Choose a people.";if(!met[seller]&&buyer==0)return "You have not met them.";if(AtWar(buyer,seller))return "You are at war.";
  if(!cities.Any(c=>c.owner==seller&&At(c.x,c.y).realm==0)&&!cities.Any(c=>c.owner==seller))return "They have no cities.";if(!ShipmentRoute(buyer,seller))return "No way to carry it: no city of theirs within 12 tiles of yours on the same land, and no commercial pact.";
  if(Spare(seller,kind)<amount)return "They cannot spare "+amount+" "+(kind==0?"timber":"stone")+" (they need it themselves).";if(nations[buyer].gold<ShipmentPrice(seller,kind,amount))return "Costs "+ShipmentPrice(seller,kind,amount)+" gold.";return "";}
 bool ShipmentRoute(int a,int b){if(HasPact(a,b,Pacts.Commercial))return true;foreach(var x in cities){if(x.owner!=a)continue;foreach(var y in cities)if(y.owner==b&&At(x.x,x.y).realm==At(y.x,y.y).realm&&Distance(x.x,x.y,y.x,y.y)<=12)return true;}return false;}
 public string BuyShipment(int buyer,int seller,int kind,int amount){string why=ShipmentReason(buyer,seller,kind,amount);if(why!="")return why;int price=ShipmentPrice(seller,kind,amount);
  nations[buyer].gold-=price;nations[seller].gold+=price;if(buyer==0)gold=nations[0].gold;if(seller==0)gold=nations[0].gold;
  if(kind==0){nations[seller].timber-=amount;nations[buyer].timber+=amount;}else{nations[seller].stone-=amount;nations[buyer].stone+=amount;}
  if(buyer==0)Note("Bought "+amount+" "+(kind==0?"timber":"stone")+" from "+NationName(seller)+" for "+price+" gold. It is in your stockpile now.");return "";}
 bool HasPact(int a,int b,int pact){return a!=b&&(Relation(a,b).pacts&(1<<pact))!=0&&!AtWar(a,b);}
 // ---- The assessment, from one civilisation's knowledge ----
 public SiteReport AssessSite(int owner,Tile t){
  var r=new SiteReport();if(t==null)return r;int cell=t.y*Width+t.x;r.known=owner==0?t.seen:nations[owner].known!=null&&nations[owner].known[cell];
  if(!r.known){r.lines.Add("You have not seen this place.");return r;}
  bool wood=Wooded(t)||t.resource==MaterialRules.TimberResource&&ResourceKnown(owner,t.resource),stoneSite=t.terrain==2&&!Wooded(t)||t.resource==MaterialRules.StoneResource&&ResourceKnown(owner,t.resource);
  if(!wood&&!stoneSite){r.lines.Add("Nothing here we know how to gather.");return r;}
  r.kind=CampKind(owner,t);string mat=wood?"timber":"stone";
  if(r.kind<0){r.lines.Add("Here is "+(wood?"woodland":"stone")+", but we do not yet know how to gather it ("+(wood?"Stone Tools":"Mining")+").");return r;}
  var jobs=MaterialJobs(owner).Where(j=>r.kind==0?j.totalT>j.instT:j.totalS>j.instS).ToList();
  r.understood=jobs.Count>0;
  r.lines.Add(r.understood?"We could use its "+mat+" now for: "+string.Join(", ",jobs.Take(3).Select(j=>j.name).ToArray())+(jobs.Count>3?" and more":"")+".":"We know how to use its "+mat+", but nothing we are building needs more of it now.");
  // Claims we know of: a camp, a border, a city close by.
  var camp=OutpostAt(t.x,t.y);if(camp!=null)r.lines.Add(camp.owner==owner?"Our camp works it"+(OutpostDelivers(camp)?".":", but nothing reaches home now."):NationName(camp.owner)+" has a camp here (since turn "+camp.built+").");
  if(t.owner>=0&&t.owner!=owner)r.lines.Add("It lies in "+NationName(t.owner)+"'s land"+(CanEnter(owner,t)?"; they let us pass.":"."));
  {var near=cities.Where(c=>c.owner!=owner&&(owner!=0||met[c.owner])&&Distance(c.x,c.y,t.x,t.y)<=4&&(owner!=0||At(c.x,c.y).seen)).OrderBy(c=>Distance(c.x,c.y,t.x,t.y)).FirstOrDefault();if(near!=null)r.lines.Add(NationName(near.owner)+"'s "+near.name+" is "+Distance(near.x,near.y,t.x,t.y)+" tiles away.");}
  if(owner==0)foreach(var u in units.Where(u=>u.owner>0&&u.type==1&&Distance(u.x,u.y,t.x,t.y)<=3&&VisibleTo(0,u.x,u.y)).Select(u=>u.owner).Distinct())r.lines.Add(NationName(u)+" workers are close by.");
  if(t.owner==owner){r.lines.Add("It is inside our borders: a city works it directly.");r.reachable=true;return r;}
  string why=CampSiteReason(owner,t);r.reachable=why=="";
  // The choices, in the same terms.
  var worker=units.Where(u=>u.owner==owner&&u.type==1).OrderBy(u=>u.job>0?1:0).ThenBy(u=>Distance(u.x,u.y,t.x,t.y)).FirstOrDefault();
  int travel=worker==null?-1:(Distance(worker.x,worker.y,t.x,t.y)+Math.Max(1,MaxMoves(worker))-1)/Math.Max(1,MaxMoves(worker));
  var baseline=CampForecast(owner,t,Math.Max(0,r.kind),99);
  if(r.reachable&&worker!=null){int from=travel+CampWork+1;var f=CampForecast(owner,t,r.kind,from);var best=f.Where(x=>x.Value[1]<x.Value[0]).OrderByDescending(x=>x.Value[0]-x.Value[1]).FirstOrDefault();
   r.options.Add(new SiteOption{title="Set up a "+mat+" camp",delivery=from,action="camp",detail=(best.Key!=null?"Would move "+best.Key+" from turn "+(turn+best.Value[0])+" to turn "+(turn+best.Value[1])+". ":"No time saved: "+(r.understood?"the work is held back by shields, not "+mat+".":"nothing needs it now."))+"First "+mat+" in "+from+" turns ("+CampYield(owner,t,r.kind)+" a turn). Commits "+(worker.job>0?"a busy ":"a ")+"Worker"+(travel>0?" "+travel+(travel==1?" turn":" turns")+" away":"")+" for "+CampWork+" turns."});}
  else r.options.Add(new SiteOption{title="A camp is not possible now",detail=why});
  if(units.Any(u=>u.owner==owner&&u.type==0))r.options.Add(new SiteOption{title="Found a settlement nearby",detail="Lasting access, its own production and a stronger hold on the way; commits your Settler and needs a good site within two tiles."});
  {int need=jobs.Sum(j=>r.kind==0?j.totalT-j.instT:j.totalS-j.instS);int stock=r.kind==0?nations[owner].timber:nations[owner].stone;int amount=Math.Max(0,Math.Min(30,need-stock));
   if(amount>0){var sellers=Enumerable.Range(1,civCount-1).Where(o=>o!=owner&&(owner!=0||met[o])&&!IsNativeNation(o)&&ShipmentReason(owner,o,r.kind,amount)=="").OrderBy(o=>ShipmentPrice(o,r.kind,amount)).ToList();
    if(sellers.Count>0)r.options.Add(new SiteOption{title="Buy "+amount+" "+mat+" from "+NationName(sellers[0]),delivery=0,action="buy:"+sellers[0]+":"+amount,detail="For "+ShipmentPrice(sellers[0],r.kind,amount)+" gold, in your stockpile at once. The woodland stays theirs to work or nobody's; they decide whether they can spare it."});
    else{var refuse=Enumerable.Range(1,civCount-1).Where(o=>o!=owner&&met[o]&&!IsNativeNation(o)).Select(o=>NationName(o)+": "+ShipmentReason(owner,o,r.kind,amount)).Take(2).ToList();r.options.Add(new SiteOption{title="No one can sell "+amount+" "+mat+" now",detail=refuse.Count==0?"You have met no people who could.":string.Join(" ",refuse.ToArray())});}}}
  {var slow=baseline.OrderByDescending(x=>x.Value[0]).FirstOrDefault();r.options.Add(new SiteOption{title="Carry on with what we have",delivery=0,detail=slow.Key!=null?"Everything under way finishes by turn "+(turn+baseline.Max(x=>x.Value[0]==99?0:x.Value[0]))+(baseline.Any(x=>x.Value[0]==99)?" (some not within 40 turns)":"")+"; the worker stays free.":"Nothing under way needs it."});}
  return r;
 }
 // Rivals: a worker with nothing better builds a camp where the forecast says it saves the most, at most one at a time.
 bool CampAI(Unit u){
  if(!buildingMaterials||u.type!=1||u.owner==0||IsNativeNation(u.owner)||Outposts().Count(p=>p.owner==u.owner)>=1+cities.Count(c=>c.owner==u.owner)/3)return false;
  var jobs=MaterialJobs(u.owner);if(jobs.Count==0)return false;var f=ForecastJobs(jobs.Select(j=>j.Copy()).ToList(),nations[u.owner].timber,nations[u.owner].stone,MaterialIncome(u.owner),20,turn,null);if(!f.Any(j=>j.firstWait>0))return false;
  Tile best=null;int gain=0;var known=nations[u.owner].known;
  foreach(var t in TilesAround(u.x,u.y,5)){if(t.owner==u.owner||known!=null&&!known[t.y*Width+t.x]||CampSiteReason(u.owner,t)!="")continue;int k=CampKind(u.owner,t);int g=CampBenefit(u.owner,t,k,Distance(u.x,u.y,t.x,t.y)+CampWork+1);if(g>gain){gain=g;best=t;}}
  if(best==null)return false;if(best.x==u.x&&best.y==u.y)return StartJob(u,CampJob);bool ok=Order(u,best.x,best.y);if(ok)u.nextJob=CampJob;return ok;
 }
 public bool CampAIForProbe(Unit u){return CampAI(u);}
 // A worker ordered to a camp site starts the camp when it gets there.
 void StartPendingCamps(int owner){foreach(var u in units.Where(a=>a.owner==owner&&a.type==1&&a.job==0&&a.nextJob==CampJob&&!a.hasOrder&&a.moves>0).ToList()){u.nextJob=0;if(CampReason(u)=="")StartJob(u,CampJob);}}
 public string SendWorkerToCamp(int owner,Tile t){var worker=units.Where(u=>u.owner==owner&&u.type==1).OrderBy(u=>u.job>0?1:0).ThenBy(u=>Distance(u.x,u.y,t.x,t.y)).FirstOrDefault();if(worker==null)return "You have no Worker.";string why=CampSiteReason(owner,t);if(why!="")return why;
  if(worker.x==t.x&&worker.y==t.y){worker.moves=Math.Max(worker.moves,1);return StartJob(worker,CampJob)?"":CampReason(worker);}if(worker.job>0)CancelOrder(worker);if(!Order(worker,t.x,t.y))return "No route for the Worker.";worker.nextJob=CampJob;return "";}
}
}
namespace ParadigmRealm {
public partial class World {
 // The woodland scenario (owner brief, 26 Nov 2026; "-woodland" on the command line). Seed 2612: you and one neighbour, early
 // in the Dawn Age, both knowing Stone Tools and Pottery. Your capital is building a Granary with no timber in store; your Worker
 // is farming at home. A woodland lies unclaimed between you, with the way home crossing a river (Bridge Building unknown).
 // The neighbour (who also knows Bronze Working) is building a Workshop, also short of timber, and has a Worker of its own. Neither of you knows Sailing.
 public static World WoodlandScenario(int seed=2612){
  for(int attempt=0;attempt<12;attempt++){var w=World.New(seed+attempt,"Woodland",0,1,1,0,1,2,2,hex:true,natives:false,rations:true);w.EnableMaterials();
   var cap=w.Found(w.units.First(u=>u.owner==0&&u.type==0));if(cap==null)continue;
   Tile wood=null,rivalSpot=null;
   foreach(var t in w.tiles.Where(t=>t.realm==0&&Wooded(t)&&t.owner<0&&w.Distance(t.x,t.y,cap.x,cap.y)>=4&&w.Distance(t.x,t.y,cap.x,cap.y)<=5).OrderBy(t=>t.y*w.Width+t.x)){
    var spot=w.tiles.Where(s=>s.realm==0&&s.owner<0&&!IsWater(s.terrain)&&!IsImpassable(s.terrain)&&!Barren(s.terrain)&&s.terrain!=7&&w.Distance(s.x,s.y,t.x,t.y)>=4&&w.Distance(s.x,s.y,t.x,t.y)<=5&&w.Distance(s.x,s.y,cap.x,cap.y)>=8).OrderBy(s=>s.y*w.Width+s.x).FirstOrDefault();
    if(spot!=null){wood=t;rivalSpot=spot;if(w.TilesAround(t.x,t.y,2).Any(r=>r.terrain==7))break;}}
   if(wood==null)continue;
   foreach(var u in w.units.Where(u=>u.owner==1).ToList())w.units.Remove(u);foreach(var c in w.cities.Where(c=>c.owner==1).ToList())w.cities.Remove(c);
   var settler=w.Add(1,0,rivalSpot.x,rivalSpot.y);var rival=w.Found(settler);if(rival==null)continue;
   foreach(int o in new[]{0,1})foreach(var id in new[]{"toolmaking","pottery"}){int k=TechnologyCatalog.Index(id);if(k>=0)w.nations[o].tech[k]=true;}
   w.tech=w.nations[0].tech;w.met[1]=true;
   foreach(var n in w.nations){n.timber=0;n.stone=20;n.gold=200;}w.gold=200;
   w.ChooseProject(cap,7,false);w.SettleBuilding(cap);{int b=TechnologyCatalog.Index("bronze");if(b>=0)w.nations[1].tech[b]=true;}w.ChooseProject(rival,30,false);w.SettleBuilding(rival);// The work on both Granaries is done; only their timber is missing.
   cap.buildingProduction=w.CostFor(cap,7);rival.buildingProduction=w.CostFor(rival,30);
   foreach(var u in w.units.Where(u=>u.owner==0&&u.type!=0).ToList())w.units.Remove(u);
   var home=w.TilesAround(cap.x,cap.y,1).FirstOrDefault(t=>t.owner==0&&!(t.x==cap.x&&t.y==cap.y)&&Farmable(t.terrain)&&!Wooded(t))??w.At(cap.x,cap.y);
   var worker=w.Add(0,1,home.x,home.y);if(w.JobReason(worker,2)=="")w.StartJob(worker,2);
   var look=w.TilesAround(wood.x,wood.y,1).Where(t=>t!=wood&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.owner<0).OrderBy(t=>t.y*w.Width+t.x).FirstOrDefault()??wood;var scout=w.Add(0,3,look.x,look.y);w.Add(1,1,rival.x,rival.y);{var near=w.TilesAround(wood.x,wood.y,1).FirstOrDefault(t=>t!=wood&&t!=look&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.owner<0);if(near!=null)w.Add(1,3,near.x,near.y);}
   w.Reveal();return w;}
  return World.New(seed,"Woodland",0,1,1,0,1,2,2,hex:true,natives:false,rations:true);
 }
 public Tile WoodlandSite(){var cap=cities.FirstOrDefault(c=>c.owner==0);if(cap==null)return null;return tiles.Where(t=>t.realm==0&&Wooded(t)&&t.owner<0&&Distance(t.x,t.y,cap.x,cap.y)>=4&&Distance(t.x,t.y,cap.x,cap.y)<=5&&units.Any(u=>u.owner==0&&u.type==3&&Distance(u.x,u.y,t.x,t.y)<=1)).OrderBy(t=>units.Any(u=>u.owner==0&&u.type==3&&u.x==t.x&&u.y==t.y)?1:0).ThenBy(t=>t.y*Width+t.x).FirstOrDefault();}
}
}
