using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// The timber and stone economy, turn by turn (owner brief, 26 Nov 2026). Before: a building's timber and stone were taken in
// full the moment it started (BuildingMaterials.cs, 21 Nov), nothing else used them, and the top bar showed only a total.
// Now materials are drawn as work is done:
//   * Each project has a total timber and stone need. Buildings keep their totals (MaterialRules); boats, galleys, war galleys
//     and catapults trained in a city need timber (UnitTimber); a bridge (a road on a river tile) needs timber and a fort stone
//     (JobTimber, JobStone). The gold for a building is still paid when it starts.
//   * Owed(progress) = total x progress / work, rounded down; each turn a project is charged Owed(new progress) minus what is
//     already installed, and at completion Owed is exactly the total. A project needing both advances only as far as both allow.
//   * One national stockpile. This turn's harvest is added first and can be used at once (the ledger shows it as produced).
//   * Projects are supplied in one fixed order: Urgent first, then the earliest started, then buildings, units, works, then id.
//     Nothing depends on which city Unity happens to update first.
//   * Short of materials, a project keeps its work and installed materials and waits; its spare shields go to the city's other
//     line. Pausing stops the draw. Finished work has no upkeep.
//   * Cancelling or replacing a building or unit returns everything installed in it. Materials installed in a worker's work stay
//     on that tile until the work resumes there; if a different work is begun there they go back to the stockpile.
// The same Allocate runs the turn and every forecast (city line, build preview, ledger, End Turn warnings), so a preview and
// the turn that follows agree.
[Serializable] public class MaterialCommitment{public int owner,started;public string key="";public bool urgent;}
[Serializable] public class SiteStock{public int owner,cell,job,timber,stone;}
[Serializable] public class MaterialLedger{public int turn=-1,openTimber,openStone,closeTimber,closeStone,madeTimber,madeStone,usedTimber,usedStone;public List<string> made=new List<string>(),used=new List<string>(),waiting=new List<string>();}
public class MatJob{public string key="",name="";public int kind,city=-1,cell=-1,project=-1,totalT,totalS,work,done,instT,instS,rate,started,reach,finish=-1,firstWait=-1,shortT,shortS;public bool urgent,waited;public MatJob next;
 public MatJob Copy(){var j=(MatJob)MemberwiseClone();j.next=next==null?null:next.Copy();return j;}}
public partial class World {
 public List<MaterialCommitment> materialCommits=new List<MaterialCommitment>();public List<SiteStock> siteStocks=new List<SiteStock>();public MaterialLedger ledger=new MaterialLedger();
 [NonSerialized] Dictionary<int,int> buildReach,unitReach,jobReach;[NonSerialized] HashSet<string> warnedWaits;
 public const int BridgeTimber=20,FortStone=12,FortTimber=4;
 public int UnitTimber(int type){if(!buildingMaterials)return 0;return type==12?6:type==13?16:type==14?20:type==9?12:type==11?4:0;}
 public int JobTimber(int owner,int cell,int job){if(!buildingMaterials)return 0;var t=tiles[cell];return job==1&&t.terrain==7&&!t.road?BridgeTimber:job==4?FortTimber:0;}
 public int JobStone(int owner,int cell,int job){if(!buildingMaterials)return 0;return job==4?FortStone:0;}
 static int Owed(int total,int done,int work){if(total<=0)return 0;if(work<=0||done>=work)return total;return (int)((long)total*Math.Max(0,done)/work);}
 List<MaterialCommitment> Commits(){if(materialCommits==null)materialCommits=new List<MaterialCommitment>();return materialCommits;}
 List<SiteStock> Sites(){if(siteStocks==null)siteStocks=new List<SiteStock>();return siteStocks;}
 MaterialCommitment Commit(int owner,string key,bool create){var c=Commits().Find(x=>x.owner==owner&&x.key==key);if(c==null&&create){c=new MaterialCommitment{owner=owner,key=key,started=turn};materialCommits.Add(c);}return c;}
 public bool IsUrgent(int owner,string key){var c=Commit(owner,key,false);return c!=null&&c.urgent;}
 public void SetUrgent(int owner,string key,bool on){var c=Commit(owner,key,true);c.urgent=on;}
 // ---- What each nation is building that needs materials, at the rate it is worked now ----
 public List<MatJob> MaterialJobs(int owner,Func<City,int> output=null,int turns=1){
  var jobs=new List<MatJob>();if(!buildingMaterials)return jobs;
  foreach(var c in cities){if(c.owner!=owner)continue;
   bool unitLine=c.build>=0&&BuildReason(c,c.build)=="",buildingLine=c.buildingProject>=0&&BuildReason(c,c.buildingProject)==""&&c.paidFor==c.buildingProject;
   int o=output!=null?output(c):Math.Max(0,Yield(c)[1]);int toB=o;if(modernRules&&unitLine&&buildingLine){toB=o/2;o-=toB;}
   if(buildingLine){int b=c.buildingProject,tt=BuildingTimber(b),ss=BuildingStone(b);
    if(tt+ss>0)jobs.Add(new MatJob{key="b"+c.id+":"+b,name=BuildName(b)+" in "+c.name,kind=0,city=c.id,project=b,totalT=tt,totalS=ss,work=CostFor(c,b),done=c.buildingProduction,instT=c.paidTimber,instS=c.paidStone,rate=toB*turns});}
   if(unitLine){int u=BuildUnit(c.build),tt=u>=0?UnitTimber(u):0;
    if(tt>0)jobs.Add(new MatJob{key="u"+c.id+":"+c.build,name=BuildName(c.build)+" in "+c.name,kind=1,city=c.id,project=c.build,totalT=tt,work=CostFor(c,c.build),done=c.production,instT=c.unitMatFor==c.build+1?c.unitTimber:0,rate=(o+ArmyShields(c,c.build))*turns});}
  }
  var seen=new HashSet<int>();
  foreach(var u in units){if(u.owner!=owner||u.job==0||u.unfed)continue;int cell=u.y*Width+u.x;if(!seen.Add(cell*8+u.job))continue;int tt=JobTimber(owner,cell,u.job),ss=JobStone(owner,cell,u.job);if(tt+ss<=0)continue;
   var team=WorkerTeam(u).Where(a=>a.job==u.job).ToList();int work=Math.Max(1,JobWork(u,u.job)),left=team.Min(a=>a.jobTurns);var site=Sites().Find(x=>x.owner==owner&&x.cell==cell&&x.job==u.job);
   jobs.Add(new MatJob{key="j"+cell+":"+u.job,name=(u.job==1?"Bridge":"Fort")+" near "+NearestCityName(owner,u.x,u.y),kind=2,cell=cell,project=u.job,totalT=tt,totalS=ss,work=work,done=Math.Max(0,work-left),instT=site!=null?site.timber:0,instS=site!=null?site.stone:0,rate=team.Count*turns});}
  foreach(var j in jobs){var m=Commit(owner,j.key,false);j.started=m!=null?m.started:turn;j.urgent=m!=null&&m.urgent;}
  return jobs;
 }
 string NearestCityName(int owner,int x,int y){City best=null;int least=int.MaxValue;foreach(var c in cities){if(c.owner!=owner)continue;int d=Distance(c.x,c.y,x,y);if(d<least){least=d;best=c;}}return best==null?"your land":best.name;}
 static IEnumerable<MatJob> SupplyOrder(IEnumerable<MatJob> jobs){return jobs.OrderByDescending(j=>j.urgent).ThenBy(j=>j.started).ThenBy(j=>j.kind).ThenBy(j=>j.key,StringComparer.Ordinal);}
 // ---- The one allocation: supplies each job in order as far as the stockpile allows; returns what was used ----
 static void Allocate(List<MatJob> jobs,ref int timber,ref int stone){
  foreach(var j in SupplyOrder(jobs).ToList()){
   int target=Math.Min(j.work,j.done+Math.Max(0,j.rate));j.shortT=j.shortS=0;j.waited=false;if(target<=j.done&&j.done<j.work){j.reach=j.done;continue;}
   int t0=timber,s0=stone;Func<int,bool> fits=p=>Math.Max(0,Owed(j.totalT,p,j.work)-j.instT)<=t0&&Math.Max(0,Owed(j.totalS,p,j.work)-j.instS)<=s0;
   int lo=j.done,hi=target;if(!fits(lo)){lo=j.done-1;}
   while(lo<hi){int mid=(lo+hi+1)/2;if(fits(mid))lo=mid;else hi=mid-1;}
   int p2=Math.Max(j.done,lo);int needT=Math.Max(0,Owed(j.totalT,p2,j.work)-j.instT),needS=Math.Max(0,Owed(j.totalS,p2,j.work)-j.instS);
   if(needT>timber||needS>stone){p2=j.done;needT=needS=0;}
   timber-=needT;stone-=needS;j.instT+=needT;j.instS+=needS;j.reach=p2;
   // It waits if its work could not go as far as its shields allow, or (its work done) its last materials are not all in.
   if(p2<target||j.instT<Owed(j.totalT,p2,j.work)||j.instS<Owed(j.totalS,p2,j.work)){j.waited=true;j.shortT=Math.Max(0,Owed(j.totalT,target,j.work)-j.instT-timber);j.shortS=Math.Max(0,Owed(j.totalS,target,j.work)-j.instS-stone);}
  }
 }
 // ---- The turn: harvest, then supply every project before any city works ----
 void MaterialsBegin(int owner,List<City> feeding,int[][] fed,int catchup){
  if(!buildingMaterials)return;var n=nations[owner];if(buildReach==null){buildReach=new Dictionary<int,int>();unitReach=new Dictionary<int,int>();jobReach=new Dictionary<int,int>();}
  foreach(var c in feeding){buildReach.Remove(c.id);unitReach.Remove(c.id);if(c.buildingProject>=0)TryPayBuilding(c);SettleUnitMaterials(c);}
  var rec=owner==0?new MaterialLedger{turn=turn,openTimber=n.timber,openStone=n.stone}:null;
  var inc=MaterialIncome(owner);n.timber+=inc[0]*catchup;n.stone+=inc[1]*catchup;
  if(rec!=null){rec.madeTimber=inc[0];rec.madeStone=inc[1];foreach(var line in MaterialSources(owner))rec.made.Add(line);}
  var outputs=new Dictionary<int,int>();for(int k=0;k<feeding.Count;k++)outputs[feeding[k].id]=fed[k][1];
  var jobs=MaterialJobs(owner,c=>{int v;return outputs.TryGetValue(c.id,out v)?v:0;},catchup);
  foreach(var j in jobs)Commit(owner,j.key,true);
  Commits().RemoveAll(m=>m.owner==owner&&!jobs.Any(j=>j.key==m.key));
  int timber=n.timber,stone=n.stone;Allocate(jobs,ref timber,ref stone);
  foreach(var j in jobs){int dT=j.instT-InstalledT(owner,j),dS=j.instS-InstalledS(owner,j);
   if(j.kind==0){var c=cities.Find(x=>x.id==j.city);c.paidTimber=j.instT;c.paidStone=j.instS;buildReach[c.id]=j.reach;}
   else if(j.kind==1){var c=cities.Find(x=>x.id==j.city);c.unitMatFor=c.build+1;c.unitTimber=j.instT;unitReach[c.id]=j.reach;}
   else{var site=Sites().Find(x=>x.owner==owner&&x.cell==j.cell&&x.job==j.project);if(site==null){foreach(var old in Sites().Where(x=>x.owner==owner&&x.cell==j.cell&&x.job!=j.project).ToList()){timber+=old.timber;stone+=old.stone;siteStocks.Remove(old);}site=new SiteStock{owner=owner,cell=j.cell,job=j.project};siteStocks.Add(site);}site.timber=j.instT;site.stone=j.instS;jobReach[j.cell*8+j.project]=j.reach;}
   if(rec!=null&&(dT>0||dS>0))rec.used.Add(j.name+": "+MatText(dT,dS));
   if(rec!=null&&j.waited)rec.waiting.Add(j.name+" waits for "+MatText(j.shortT,j.shortS));}
  n.timber=timber;n.stone=stone;
  if(rec!=null){rec.usedTimber=rec.openTimber+inc[0]-timber;rec.usedStone=rec.openStone+inc[1]-stone;rec.closeTimber=timber;rec.closeStone=stone;ledger=rec;}
 }
 int InstalledT(int owner,MatJob j){if(j.kind==0){var c=cities.Find(x=>x.id==j.city);return c.paidTimber;}if(j.kind==1){var c=cities.Find(x=>x.id==j.city);return c.unitMatFor==c.build+1?c.unitTimber:0;}var s=Sites().Find(x=>x.owner==owner&&x.cell==j.cell&&x.job==j.project);return s==null?0:s.timber;}
 int InstalledS(int owner,MatJob j){if(j.kind==0){var c=cities.Find(x=>x.id==j.city);return c.paidStone;}if(j.kind==1)return 0;var s=Sites().Find(x=>x.owner==owner&&x.cell==j.cell&&x.job==j.project);return s==null?0:s.stone;}
 public static string MatText(int t,int s){var p=new List<string>();if(t>0)p.Add(t+" timber");if(s>0)p.Add(s+" stone");return p.Count==0?"nothing":string.Join(" and ",p.ToArray());}
 // A unit line that changed returns what was installed in the unit it replaced.
 void SettleUnitMaterials(City c){if(c.unitMatFor!=0&&c.unitMatFor!=c.build+1){var n=nations[c.owner];n.timber+=c.unitTimber;c.unitTimber=c.unitStone=0;c.unitMatFor=0;}}
 // How far the building and unit lines may go this turn ( -1: no limit).
 int BuildingReach(City c){int v;return buildingMaterials&&buildReach!=null&&buildReach.TryGetValue(c.id,out v)?v:-1;}
 int UnitReach(City c){int v;return buildingMaterials&&unitReach!=null&&unitReach.TryGetValue(c.id,out v)?v:-1;}
 int JobReach(int cell,int job){int v;return buildingMaterials&&jobReach!=null&&jobReach.TryGetValue(cell*8+job,out v)?v:-1;}
 bool BuildingMaterialsIn(City c){return !buildingMaterials||c.paidFor!=c.buildingProject||c.paidTimber>=BuildingTimber(c.buildingProject)&&c.paidStone>=BuildingStone(c.buildingProject);}
 bool UnitMaterialsIn(City c){int u=BuildUnit(c.build);int need=u>=0?UnitTimber(u):0;return need<=0||c.unitMatFor==c.build+1&&c.unitTimber>=need;}
 bool JobMaterialsIn(int owner,int cell,int job){int tt=JobTimber(owner,cell,job),ss=JobStone(owner,cell,job);if(tt+ss<=0)return true;var s=Sites().Find(x=>x.owner==owner&&x.cell==cell&&x.job==job);return s!=null&&s.timber>=tt&&s.stone>=ss;}
 void JobFinished(int owner,int cell,int job){Sites().RemoveAll(x=>x.owner==owner&&x.cell==cell&&x.job==job);}
 // Every place timber and stone came from this turn, named.
 public List<string> MaterialSources(int owner){
  var lines=new List<string>();
  foreach(var c in cities){if(c.owner!=owner)continue;int t=MaterialRules.CityTimber,s=MaterialRules.CityStone,wt=0,hs=0,rt=0,rs=0;
   foreach(var tile in Worked(c)){if(Wooded(tile))wt+=MaterialRules.WoodedTile;if(tile.terrain==2)hs+=MaterialRules.HillTile;
    if(tile.resource==MaterialRules.TimberResource&&ResourceKnown(owner,tile.resource))rt+=MaterialRules.ResourceTile+(tile.road?MaterialRules.DevelopedBonus:0);
    if(tile.resource==MaterialRules.StoneResource&&ResourceKnown(owner,tile.resource))rs+=MaterialRules.ResourceTile+(tile.mine?MaterialRules.DevelopedBonus:0);}
   var parts=new List<string>{"town "+MatText(t,s)};if(wt>0)parts.Add("woodland "+wt+" timber");if(hs>0)parts.Add("hills "+hs+" stone");if(rt>0)parts.Add("timber stands "+rt);if(rs>0)parts.Add("quarries "+rs);
   lines.Add(c.name+": "+MatText(t+wt+rt,s+hs+rs)+" ("+string.Join(", ",parts.ToArray())+")");}
  OutpostIncome(owner,lines);
  return lines;
 }
 // ---- Forecast: the same allocation, turn after turn, with today's output and income; queued items start as lines free up ----
 // Work under way with each line's queued items behind it (they start as the line frees up).
 public List<MatJob> JobsWithQueue(int owner){
  var jobs=MaterialJobs(owner);
  foreach(var j in jobs){if(j.kind==2)continue;var c=cities.Find(x=>x.id==j.city);var q=j.kind==0?c.buildingQueue:c.queue;MatJob tail=j;
   foreach(int b in q){int tt=j.kind==0?BuildingTimber(b):BuildUnit(b)>=0?UnitTimber(BuildUnit(b)):0,ss=j.kind==0?BuildingStone(b):0;var nx=new MatJob{key=(j.kind==0?"b":"u")+c.id+":"+b+":q",name=BuildName(b)+" in "+c.name,kind=j.kind,city=c.id,project=b,totalT=tt,totalS=ss,work=CostFor(c,b),rate=j.rate,started=turn+1};tail.next=nx;tail=nx;}}
  return jobs;
 }
 public List<MatJob> MaterialForecast(int owner,MatJob extra=null,int horizon=40){
  var jobs=MaterialJobs(owner);
  foreach(var j in jobs){if(j.kind==2)continue;var c=cities.Find(x=>x.id==j.city);var q=j.kind==0?c.buildingQueue:c.queue;MatJob tail=j;
   foreach(int b in q){int tt=j.kind==0?BuildingTimber(b):BuildUnit(b)>=0?UnitTimber(BuildUnit(b)):0,ss=j.kind==0?BuildingStone(b):0;var nx=new MatJob{key=(j.kind==0?"b":"u")+c.id+":"+b+":q",name=BuildName(b)+" in "+c.name,kind=j.kind,city=c.id,project=b,totalT=tt,totalS=ss,work=CostFor(c,b),rate=j.rate,started=turn+1};tail.next=nx;tail=nx;}}
  if(extra!=null)jobs.Add(extra);
  return ForecastJobs(jobs,nations[owner].timber,nations[owner].stone,MaterialIncome(owner),horizon,turn,null);
 }
 // The forecast itself, on given work, stock and income: each turn the harvest is added, Allocate supplies the work in order,
 // and finished work hands its line to the next queued item. trace (if given) gets the stock after each turn.
 public static List<MatJob> ForecastJobs(List<MatJob> jobs,int timber,int stone,int[] inc,int horizon,int now,List<int[]> trace){
  var all=new List<MatJob>(jobs);foreach(var j in jobs){var x=j.next;while(x!=null){all.Add(x);x=x.next;}}
  var active=new List<MatJob>(jobs);
  for(int k=1;k<=horizon&&active.Count>0;k++){
   timber+=inc[0];stone+=inc[1];var live=active.Where(j=>j.totalT+j.totalS>0).ToList();Allocate(live,ref timber,ref stone);
   foreach(var j in active.ToList()){if(j.totalT+j.totalS<=0){j.reach=Math.Min(j.work,j.done+j.rate);j.waited=false;}if(j.waited&&j.firstWait<0)j.firstWait=k;j.done=j.reach;
    if(j.done>=j.work&&j.instT>=j.totalT&&j.instS>=j.totalS){j.finish=k;active.Remove(j);if(j.next!=null){j.next.started=now+k;active.Add(j.next);}}}
   if(trace!=null)trace.Add(new[]{timber,stone});}
  return all;
 }
 // What starting one more project would do, in one line for the build list.
 public string MaterialPreview(City c,int b){
  if(!buildingMaterials||c==null)return "";bool unit=BuildUnit(b)>=0;int tt=unit?UnitTimber(BuildUnit(b)):BuildingTimber(b),ss=unit?0:BuildingStone(b);if(tt+ss<=0)return "";
  int rate=Math.Max(1,LineShare(c,!unit));int work=CostFor(c,b);int turns=Math.Max(1,(work+rate-1)/rate);
  var extra=new MatJob{key="preview",name=BuildName(b),kind=unit?1:0,city=c.id,project=b,totalT=tt,totalS=ss,work=work,rate=rate,started=turn+1};
  var before=MaterialForecast(c.owner).Where(j=>!j.key.EndsWith(":q")).ToDictionary(j=>j.key,j=>j.finish);var after=MaterialForecast(c.owner,extra);
  string head="Start "+BuildName(b)+" · "+MatText(tt,ss)+" over "+turns+(turns==1?" turn":" turns");
  var hurt=after.Where(j=>before.ContainsKey(j.key)&&(before[j.key]<0?false:j.finish<0||j.finish>before[j.key])).ToList();
  var mine=after.FirstOrDefault(j=>j.key=="preview");string self=mine==null||mine.finish<0?" · cannot be supplied within 40 turns":mine.firstWait>0?" · would wait for materials from turn "+mine.firstWait+" (done in "+mine.finish+")":"";
  if(hurt.Count==0)return head+self+(before.Count>0?" · work already under way still finishes on schedule.":".");
  var h=hurt[0];return head+self+" · delays "+h.name+(h.finish<0?" beyond 40 turns":" by "+(h.finish-before[h.key])+(h.finish-before[h.key]==1?" turn":" turns"))+(hurt.Count>1?" and "+(hurt.Count-1)+" more":"")+".";
 }
 // For a city line: installed, next draw, when it finishes, and why it waits.
 public string MaterialLine(City c,bool building){
  if(!buildingMaterials||c==null)return "";string key=building?"b"+c.id+":"+c.buildingProject:"u"+c.id+":"+c.build;var f=MaterialForecast(c.owner);var j=f.FirstOrDefault(x=>x.key==key);if(j==null)return "";
  var now=MaterialJobs(c.owner).FirstOrDefault(x=>x.key==key);if(now==null)return "";
  string inst="installed "+MatText(now.instT,now.instS)+" of "+MatText(now.totalT,now.totalS);
  if(j.firstWait==1){var first=SupplyOrder(f.Where(x=>x.finish!=0&&x.key!=key&&x.next!=j)).FirstOrDefault(x=>(now.totalT>0&&x.totalT>0||now.totalS>0&&x.totalS>0)&&x.started<=now.started);int st,ss2;MaterialShortNext(c.owner,key,out st,out ss2);
   return "Waiting for "+MatText(st,ss2)+(first!=null?" ("+first.name+" is supplied first)":"")+" · "+inst;}
  return inst+(j.finish>0?" · done in "+j.finish+(j.finish==1?" turn":" turns"):" · not within 40 turns")+(j.firstWait>0?" · short from turn "+j.firstWait:"");
 }
 public string MaterialBrief(City c,bool building){
  string key=building?"b"+c.id+":"+c.buildingProject:"u"+c.id+":"+c.build;var j=MaterialJobs(c.owner).FirstOrDefault(x=>x.key==key);if(j==null)return "";
  int t,s;MaterialShortNext(c.owner,key,out t,out s);if(t+s>0)return "Waiting for "+MatText(t,s);
  var p=new List<string>();if(j.totalT>0)p.Add("timber "+j.instT+"/"+j.totalT);if(j.totalS>0)p.Add("stone "+j.instS+"/"+j.totalS);return string.Join(", ",p.ToArray());
 }
 public void MaterialShortNext(int owner,string key,out int timber,out int stone){var jobs=MaterialJobs(owner);var inc=MaterialIncome(owner);int t=nations[owner].timber+inc[0],s=nations[owner].stone+inc[1];Allocate(jobs,ref t,ref s);var j=jobs.FirstOrDefault(x=>x.key==key);timber=j==null?0:j.shortT;stone=j==null?0:j.shortS;}
 // After your turn: a project newly predicted to wait next turn is named once.
 void WarnMaterialWaits(){
  if(!buildingMaterials)return;if(warnedWaits==null)warnedWaits=new HashSet<string>();var f=MaterialForecast(0,null,3);var now=new HashSet<string>();
  foreach(var j in f.Where(x=>x.firstWait==1&&!x.key.EndsWith(":q"))){now.Add(j.key);if(warnedWaits.Contains(j.key))continue;int t,s;MaterialShortNext(0,j.key,out t,out s);Note("Materials: "+j.name+" will wait next turn, short of "+MatText(t,s)+". Open the timber or stone figure in the top bar to see who is supplied first, or mark it urgent.");}
  warnedWaits=now;
 }
 // Rivals do not start what they cannot supply: within 15 turns of income, after what is already promised.
 public bool MaterialsInReach(int owner,int b){
  if(!buildingMaterials)return true;int tt=BuildingTimber(b),ss=BuildingStone(b);if(tt+ss==0)return true;var inc=MaterialIncome(owner);var jobs=MaterialJobs(owner);
  int needT=jobs.Sum(j=>Math.Max(0,j.totalT-j.instT)),needS=jobs.Sum(j=>Math.Max(0,j.totalS-j.instS));
  return nations[owner].timber+inc[0]*15-needT>=tt&&nations[owner].stone+inc[1]*15-needS>=ss;
 }
}
}
