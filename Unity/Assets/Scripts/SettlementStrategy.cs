using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
 public sealed class SettlementReport {
  public int x,y,score,food,production,gold,knownTiles,unknownTiles,housing,upkeep,distance;
  public bool freshWater,coastal,threat;
  public string reason="",diplomacyWarning="";
  public List<int> resources=new List<int>();
  public bool Suitable {get{return reason=="";}}
 }
 public partial class World {
  // The centre always has a road; it also gives 3 base Trade so every new city makes some science and gold. Measured in
  // Stage 5: with this, AI civilisations complete the Dawn around turn 40, the design's target.
  public static int[] CentreYield(int terrain){return new[]{terrain==7?3:2,terrain==2?3:2,TerrainTrade[terrain]+(RoadTrade(terrain)?1:0)+3};}
  public int[] CityCentreYield(City c){return CentreYield(At(c.x,c.y).terrain);}
  public bool FreshWater(int x,int y){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var t=At(x+dx,y+dy);if(t!=null&&(t.terrain==6||t.terrain==7))return true;}return false;}
  // Player reports use remembered information. Rivals conservatively evaluate only
  // currently visible terrain: they have no per-tile memory record in save v6.
  Tile SettlementTile(int owner,int x,int y){var t=At(x,y);if(t==null)return null;x=t.x;y=t.y;if(owner==0)return t.seen?KnownTile(t):null;return nations[owner].known[y*Width+x]&&VisibleTo(owner,x,y)?t:null;}
  string SiteReason(int owner,int x,int y,bool known){
   if(owner<0||owner>=nations.Length||At(x,y)==null)return "Outside the map.";
   var t=known?SettlementTile(owner,x,y):At(x,y);if(t==null)return "Explore this tile first.";
   if(IsImpassable(t.terrain))return "Cities require passable land.";
   if(t.terrain==14||t.terrain==15)return "Nothing grows on the polar ice; a city cannot survive here.";
   if((t.terrain==8||t.terrain==13)&&!HasTech(owner,"fire"))return "Research Fire before settling cold terrain.";
   if(t.owner>=0&&t.owner!=owner)return "Cannot found a city in another civilisation's territory.";
   if((!known||VisibleTo(owner,x,y))&&units.Any(u=>u.owner!=owner&&u.x==x&&u.y==y))return "The site is occupied by another civilisation.";
   if(!known){if(cities.Any(c=>Distance(x,y,c.x,c.y)<4))return "Keep at least 4 tiles between city centres.";}
   else {
    if(cities.Any(c=>c.owner==owner&&Distance(x,y,c.x,c.y)<4))return "Keep at least 4 tiles between city centres.";
    for(int dy=-3;dy<=3;dy++)for(int dx=-3;dx<=3;dx++){
     var k=SettlementTile(owner,x+dx,y+dy);if(k==null)continue;
     bool visible=VisibleTo(owner,k.x,k.y);
     if(visible?cities.Any(c=>c.x==k.x&&c.y==k.y):owner==0&&!string.IsNullOrEmpty(k.rememberedCity))return "Too close to a known city; scout again if the information is old.";
    }
   }
   return "";
  }
  int UnknownAround(int owner,Tile centre){int count=0;for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){var a=At(centre.x+dx,centre.y+dy);if(a!=null&&Distance(centre.x,centre.y,a.x,a.y)<=2&&!nations[owner].known[a.y*Width+a.x])count++;}return count;}
  public string FoundReason(Unit u){
   if(result!="")return "The game has ended.";
   if(u==null||!units.Contains(u)||u.type!=0)return "Select a Settler.";
   if(u.moves<=0)return "The Settler needs movement remaining.";
   return SiteReason(u.owner,u.x,u.y,false);
  }
  public SettlementReport AssessSettlement(int owner,int x,int y,int fromX,int fromY){
   var report=new SettlementReport{x=x,y=y,distance=Distance(fromX,fromY,x,y)};
   report.reason=SiteReason(owner,x,y,true);if(!report.Suitable)return report;
   var centre=SettlementTile(owner,x,y);var central=CentreYield(centre.terrain);
   var yields=new List<int[]>();var resources=new HashSet<int>();
   for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){
    var actual=At(x+dx,y+dy);if(actual==null)continue;
    var t=SettlementTile(owner,actual.x,actual.y);if(t==null){report.unknownTiles++;continue;}report.knownTiles++;
    if(Math.Abs(dx)<=1&&Math.Abs(dy)<=1){if(t.terrain==6||t.terrain==7)report.freshWater=true;if(t.terrain==4||t.terrain==12)report.coastal=true;}
    if(t.owner>=0&&t.owner!=owner)continue;
    // Existing cities get first claim on tiles at the same distance (older id).
    bool shared=cities.Any(c=>c.owner==owner&&Distance(c.x,c.y,t.x,t.y)<=Distance(x,y,t.x,t.y));
    if(shared||dx==0&&dy==0)continue;
    yields.Add(TileYield(t));if(t.resource>0)resources.Add(t.resource);
   }
   var best=yields.OrderByDescending(v=>v[0]*3+v[1]*2+v[2]).Take(3).ToList();
   report.food=central[0]+best.Sum(v=>v[0])-6;report.production=central[1]+best.Sum(v=>v[1]);report.gold=best.Sum(v=>v[2]);
   report.resources=resources.OrderBy(r=>r).ToList();report.housing=report.freshWater?8:6;
   var capital=cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).FirstOrDefault();report.upkeep=1+(capital==null?0:Distance(x,y,capital.x,capital.y)/10);
   report.threat=SettlementThreat(owner,x,y);report.diplomacyWarning=SettlementDiplomacyWarning(owner,x,y);
   report.score=report.food*4+report.production*2+report.gold*2+report.resources.Count*4+(report.freshWater?8:0)+(report.coastal?2:0)-report.upkeep*3-report.distance*2-(report.threat?60:0)-(report.diplomacyWarning!=""?100:0);
   return report;
  }
  bool SettlementThreat(int owner,int x,int y){return units.Any(u=>Power[u.type]>0&&AtWar(owner,u.owner)&&VisibleTo(owner,u.x,u.y)&&Distance(u.x,u.y,x,y)<=3);}
  bool SafeSettlerPath(Unit u,List<Tile> route){return route.All(t=>!SettlementThreat(u.owner,t.x,t.y));}
  public List<SettlementReport> SettlementOptions(Unit u,int limit=5){
   if(u==null||u.type!=0||!units.Contains(u))return new List<SettlementReport>();
   var reports=new List<SettlementReport>();
   // Local planning is bounded. The adviser can be reopened after moving/exploring.
   var scanned=new HashSet<int>();
   for(int y=u.y-12;y<=u.y+12;y++)for(int x=u.x-12;x<=u.x+12;x++){
    var cell=At(x,y);if(cell==null||!scanned.Add(cell.y*Width+cell.x))continue;
    var t=SettlementTile(u.owner,cell.x,cell.y);if(t==null||IsImpassable(t.terrain))continue;
    var r=AssessSettlement(u.owner,cell.x,cell.y,u.x,u.y);if(r.Suitable&&!r.threat&&(u.owner==0||r.diplomacyWarning==""))reports.Add(r);
   }
   var result=new List<SettlementReport>();
   foreach(var r in reports.OrderByDescending(r=>r.score).ThenBy(r=>r.distance).ThenBy(r=>r.y*Width+r.x).Take(24)){
    if(units.Any(a=>a.id!=u.id&&a.owner==u.owner&&a.type==0&&a.hasOrder&&Distance(a.destinationX,a.destinationY,r.x,r.y)<4))continue;
    if(r.x!=u.x||r.y!=u.y){var path=Route(u,r.x,r.y);if(path.Count==0||!SafeSettlerPath(u,path))continue;}
    result.Add(r);if(result.Count>=limit)break;
   }
   return result;
  }
  public string SettlementSummary(SettlementReport r){
   if(!r.Suitable)return r.reason;
   return "Site ("+r.x+", "+r.y+") | early population limit "+r.housing+(r.freshWater?" — fresh water":" — no known fresh water")+
    "\nPotential at 3 citizens: net food "+r.food+", production "+r.production+", tile trade "+r.gold+". Uses the best 3 known tiles; excludes future improvements, buildings and policies."+
    "\nKnown nearby resources: "+(r.resources.Count==0?"none":string.Join(", ",r.resources.Select(i=>ResourceNames[i])))+
    ". Initial city upkeep about "+r.upkeep+" gold/turn. Road connections still require workers."+
    "\n"+r.knownTiles+" tiles assessed; "+r.unknownTiles+" unexplored. "+(r.threat?"Enemy troops nearby!":"No currently visible military threat at this site.")+"\n"+r.diplomacyWarning;
  }
  public bool ShouldTrainSettler(City c){
   int owner=c.owner;return c.pop>=2&&Yield(c)[0]>0&&BuildReason(c,0)==""&&cities.Count(a=>a.owner==owner)<4&&!units.Any(u=>u.owner==owner&&u.type==0)&&nations[owner].gold>=Math.Max(20,12-Budget(owner)[2]*6);
  }
  void PlanAISettlement(Unit u){
   if(u.moves<=0)return;
   if(AtSea(u))return; // aboard a galley: the galley decides where it lands
   if(u.hasOrder){
    var r=AssessSettlement(u.owner,u.destinationX,u.destinationY,u.x,u.y);
    if(r.Suitable&&!r.threat&&r.diplomacyWarning==""&&SafeSettlerPath(u,Route(u,u.destinationX,u.destinationY))){ContinueOrder(u);if(u.x==r.x&&u.y==r.y)Found(u);if(u.hasOrder||!units.Contains(u))return;}
    else CancelOrder(u);
   }
   var options=SettlementOptions(u,1);if(options.Count>0){
    FerryWanted.Remove(u.owner);var r=options[0];if(r.x==u.x&&r.y==u.y){Found(u);return;}
    Order(u,r.x,r.y);if(u.x==r.x&&u.y==r.y)Found(u);return;
   }
   // No known legal site: explore a safe, reachable local frontier rather than stall.
   var frontierTiles=new List<Tile>();var frontierSeen=new HashSet<int>();
   for(int fy=u.y-4;fy<=u.y+4;fy++)for(int fx=u.x-4;fx<=u.x+4;fx++){
    var ft=At(fx,fy);if(ft==null||!frontierSeen.Add(ft.y*Width+ft.x))continue;
    int fd=Distance(u.x,u.y,ft.x,ft.y);
    if(fd>0&&fd<=4&&SettlementTile(u.owner,ft.x,ft.y)!=null&&!IsImpassable(ft.terrain)&&CanEnter(u.owner,ft)&&!SettlementThreat(u.owner,ft.x,ft.y))frontierTiles.Add(ft);
   }
   var frontier=frontierTiles.Select(t=>new{Tile=t,Unknown=UnknownAround(u.owner,t)}).Where(a=>a.Unknown>0)
    .OrderByDescending(a=>a.Unknown).ThenBy(a=>Distance(u.x,u.y,a.Tile.x,a.Tile.y)).ThenBy(a=>a.Tile.y*Width+a.Tile.x);
   foreach(var candidate in frontier.Take(8)){var route=Route(u,candidate.Tile.x,candidate.Tile.y);if(route.Count>0&&SafeSettlerPath(u,route)){Order(u,candidate.Tile.x,candidate.Tile.y);return;}}
   // Nowhere left to settle or explore on foot: wait on the shore for a galley.
   SettlerAwaitsFerry(u);
  }
 }
}
