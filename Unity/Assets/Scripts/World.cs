using System;
using System.Collections.Generic;
using System.Linq;

namespace ParadigmRealm
{
    [Serializable] public class Tile { public int x,y,terrain,resource,owner=-1; public bool seen,road,farm,mine,fort; public int rememberedTerrain,rememberedResource,rememberedOwner=-1; public bool rememberedRoad,rememberedFarm,rememberedMine,rememberedFort; public string rememberedCity=""; }
    [Serializable] public class Unit { public int id,owner,x,y,type,hp=10,moves=4,experience,promotion,job,jobTurns,workerGroup; public bool hasOrder,fortified; public int destinationX,destinationY; }
    [Serializable] public class City {
        public int id,owner,x,y,pop=1,food,production,build=2,focus,occupation,age;
        public string name; public bool granary,library,walls;
        public List<int> buildings=new List<int>(),queue=new List<int>(),buildingQueue=new List<int>();
        public int buildingProject=-1,buildingProduction;
    }
    [Serializable] public class Nation {
        public int gold=20,tax=1,research=TechnologyCatalog.Index("toolmaking"),arrears; public int[] progress=new int[TechnologyCatalog.Count];
        public bool[] tech=new bool[TechnologyCatalog.Count]; public bool[] known=new bool[1440];
        public string name; public int trait;
    }
    [Serializable] public partial class World
    {
        public const int Width=48,Height=30;
        public static readonly string[] TerrainNames={"Grassland","Forest","Hills","Desert","Ocean","Mountains","Lake","River","Arctic tundra","Bog","Desolate wasteland","Jungle","Coral reef","Arctic forest","Ice sheet","Glacier","Savanna","Volcanic terrain","Rocky coastline","Mangrove","Dry steppe"};
        public static readonly string[] ResourceNames={"None","Wheat","Iron","Gold","Horses","Spices"};
        public static readonly string[] UnitNames={"Settler","Worker","Warrior","Scout","Archer","Knight","Rifleman","Spearman","Horseman","Catapult","Medic"};
        public static readonly int[] Power={0,0,3,1,4,8,13,4,5,2,0};
        public static readonly int[] Moves={4,4,4,6,4,6,4,4,6,4,4};
        public static readonly int[] UnitCosts={36,20,18,14,28,48,65,24,36,45,30};
        public static readonly int[] UnitTech={0,-1,TechnologyCatalog.Index("toolmaking"),TechnologyCatalog.Index("toolmaking"),1,TechnologyCatalog.Index("chivalry"),TechnologyCatalog.Index("rifling"),6,TechnologyCatalog.Index("horseback"),8,9};
        public static readonly int[] UnitResource={0,0,0,0,0,4,2,0,4,2,0};
        public static readonly int[] Upkeep={1,1,1,1,2,3,4,1,2,3,1};
        public static readonly string[] TechNames=TechnologyCatalog.Names;
        public static readonly int[] TechCosts=TechnologyCatalog.Costs;
        public static readonly int[][] Requirements=TechnologyCatalog.Requirements;
        // Legacy ids 0..6 = units, 7..9 = buildings. New units use 100+type.
        public static readonly int[] Costs={36,20,18,14,28,48,65,32,40,35};
        public static readonly int[] BuildingIds={7,8,9,30,31,32,33,34,35};
        public static readonly string[] BuildingNames={"Granary","Library","Walls","Workshop","Market","Barracks","Gardens","Courthouse","Hospital"};
        public static readonly int[] BuildingCosts={32,40,35,45,45,35,35,55,65};
        public static readonly int[] BuildingTech={TechnologyCatalog.Index("pottery"),2,TechnologyCatalog.Index("masonry"),6,10,6,0,11,9};
        public int version=5,seed,turn=1,nextId=1,gold=20,science,research=TechnologyCatalog.Index("toolmaking"),mapType,speed=1,difficulty=1,trait;
        public Tile[] tiles; public List<Unit> units=new List<Unit>(); public List<City> cities=new List<City>();
        public List<string> log=new List<string>(); public bool[] tech=new bool[TechnologyCatalog.Count],war=new bool[3],met=new[]{true,false,false},openBorders=new bool[3];
        public Nation[] nations=new[]{new Nation(),new Nation(),new Nation()};
        public string civilisationName="Verdant Union",result=""; public int flag;
        [NonSerialized] bool processing;
        [NonSerialized] HashSet<int>[] networks;
        public Tile At(int x,int y){return x<0||y<0||x>=Width||y>=Height?null:tiles[y*Width+x];}
        public static int Distance(int x,int y,int a,int b){return Math.Max(Math.Abs(x-a),Math.Abs(y-b));}
        public string NationName(int owner){return owner==0?civilisationName:owner==1?"Amber League":"Azure Dominion";}
        public static string CleanName(string value){var s=new string((value??"").Where(c=>!char.IsControl(c)&&c!='<'&&c!='>').ToArray()).Trim();return s.Length==0?"Verdant Union":s.Substring(0,Math.Min(32,s.Length));}
        public void Note(string s){log.Insert(0,"Turn "+turn+": "+s);if(log.Count>150)log.RemoveAt(log.Count-1);}
        public Unit Add(int owner,int type,int x,int y){var u=new Unit{id=nextId++,owner=owner,type=type,x=x,y=y,moves=Moves[type]};units.Add(u);return u;}
        public static World New(int seed,string civilisationName="Verdant Union",int mapType=0,int speed=1,int difficulty=1,int trait=0)
        {
            var w=new World{seed=seed,civilisationName=CleanName(civilisationName),mapType=mapType,speed=speed,difficulty=difficulty,trait=trait,tiles=new Tile[Width*Height]};
            w.nations[0].trait=trait;w.tech=w.nations[0].tech;var r=new Random(seed);
            for(int y=0;y<Height;y++)for(int x=0;x<Width;x++){
                int n=r.Next(100),t=n<45?0:n<68?1:n<85?2:n<94?3:5;
                if(x==0||y==0||x==Width-1||y==Height-1||(mapType==0&&Math.Sin(x*.2)+Math.Cos(y*.29)*.7< -1.2))t=4;
                w.tiles[y*Width+x]=new Tile{x=x,y=y,terrain=t,owner=-1};
            }
            w.GenerateBiomes();
            int[] sx={9,34,31},sy={12,9,23};
            // Land connections ensure conquest is possible before naval implementation.
            for(int i=0;i<2;i++){int x=sx[i],y=sy[i];while(x!=sx[i+1]||y!=sy[i+1]){w.At(x,y).terrain=0;x+=Math.Sign(sx[i+1]-x);y+=Math.Sign(sy[i+1]-y);}}
            for(int i=0;i<3;i++){
                for(int y=sy[i]-3;y<=sy[i]+3;y++)for(int x=sx[i]-3;x<=sx[i]+3;x++)w.At(x,y).terrain=0;
                w.Add(i,0,sx[i],sy[i]);w.Add(i,2,sx[i]+1,sy[i]);w.Add(i,1,sx[i],sy[i]+1);
                if(i>0)w.Found(w.units.First(u=>u.owner==i&&u.type==0));
            }
            foreach(var t in w.tiles){if(t.terrain==0&&r.Next(7)==0)t.resource=1;else if(t.terrain==2&&r.Next(3)==0)t.resource=2;else if(t.terrain==3&&r.Next(5)==0)t.resource=3;else if(t.terrain==0&&r.Next(12)==0)t.resource=4;else if(t.terrain==1&&r.Next(12)==0)t.resource=5;}
            for(int i=0;i<3;i++){w.At(sx[i]-1,sy[i]).resource=1;w.At(sx[i]+2,sy[i]).resource=4;w.At(sx[i],sy[i]-2).terrain=2;w.At(sx[i],sy[i]-2).resource=2;}
            w.RebuildTerritory();w.Reveal();w.Note("Found a city, choose production and research. F1 opens the guide.");return w;
        }
        public bool VisibleTo(int owner,int x,int y){return HasTech(owner,"satellites")||units.Any(u=>u.owner==owner&&Distance(x,y,u.x,u.y)<=(u.type==3?4:2))||cities.Any(c=>c.owner==owner&&Distance(x,y,c.x,c.y)<=3);}
        public bool Visible(int x,int y){return VisibleTo(0,x,y);}
        public void Reveal(){for(int owner=0;owner<3;owner++)foreach(var t in tiles)if(VisibleTo(owner,t.x,t.y)){nations[owner].known[t.y*Width+t.x]=true;if(owner==0){t.seen=true;t.rememberedTerrain=t.terrain;t.rememberedResource=t.resource;t.rememberedOwner=t.owner;t.rememberedRoad=t.road;t.rememberedFarm=t.farm;t.rememberedMine=t.mine;t.rememberedFort=t.fort;var city=cities.Find(c=>c.x==t.x&&c.y==t.y);t.rememberedCity=city==null?"":city.name;}}
            for(int i=1;i<3;i++)if(!met[i]&&(units.Any(u=>u.owner==i&&Visible(u.x,u.y))||cities.Any(c=>c.owner==i&&Visible(c.x,c.y)))){met[i]=true;Note("First contact: "+NationName(i)+".");}}
        public bool AtWar(int a,int b){return a!=b&&(a!=0&&b!=0||war[a==0?b:a]);}
        public bool SetWar(int owner,bool value){if(owner<1||owner>2||!met[owner])return false;war[owner]=value;if(value)openBorders[owner]=false;Note((value?"War declared on ":"Peace agreed with ")+NationName(owner)+".");networks=null;return true;}
        public bool SetBorders(int owner){if(owner<1||owner>2||!met[owner]||war[owner])return false;openBorders[owner]=!openBorders[owner];Note("Open borders with "+NationName(owner)+(openBorders[owner]?" agreed.":" ended; units may withdraw."));return true;}
        public bool CanEnter(int owner,Tile t){return t.owner<0||t.owner==owner||AtWar(owner,t.owner)||(owner==0?openBorders[t.owner]:t.owner==0&&openBorders[owner]);}
        public void RebuildTerritory(){foreach(var t in tiles){var c=cities.Where(a=>Distance(t.x,t.y,a.x,a.y)<=Math.Min(4,2+a.age/25)).OrderBy(a=>Distance(t.x,t.y,a.x,a.y)).ThenBy(a=>a.id).FirstOrDefault();t.owner=c==null?-1:c.owner;}networks=null;}
        HashSet<int> Network(int owner){
            if(networks==null)networks=new HashSet<int>[3];if(networks[owner]!=null)return networks[owner];
            var set=new HashSet<int>();var capital=cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).FirstOrDefault();if(capital==null){networks[owner]=set;return set;}
            var q=new Queue<Tile>();q.Enqueue(At(capital.x,capital.y));set.Add(capital.y*Width+capital.x);
            while(q.Count>0){var t=q.Dequeue();for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=At(t.x+dx,t.y+dy);if(n==null||!n.road||n.owner!=owner||units.Any(u=>u.x==n.x&&u.y==n.y&&AtWar(owner,u.owner)))continue;if(set.Add(n.y*Width+n.x))q.Enqueue(n);}}
            networks[owner]=set;return set;
        }
        public bool HasResource(int owner,int resource){if(resource==0)return true;return tiles.Any(t=>t.owner==owner&&t.resource==resource&&Network(owner).Contains(t.y*Width+t.x)&&(resource==2||resource==3?t.mine:t.farm));}
        public bool Connected(City c){return Network(c.owner).Contains(c.y*Width+c.x);}
        public Tile KnownTile(Tile t){if(t==null||!t.seen||Visible(t.x,t.y))return t;return new Tile{x=t.x,y=t.y,seen=true,terrain=t.rememberedTerrain,resource=t.rememberedResource,owner=t.rememberedOwner,road=t.rememberedRoad,farm=t.rememberedFarm,mine=t.rememberedMine,fort=t.rememberedFort,rememberedCity=t.rememberedCity};}
        public bool RoadConnects(Tile t,int dx,int dy){var n=KnownTile(At(t.x+dx,t.y+dy));return t.road&&n!=null&&n.seen&&n.road;}
        public int[] TileYield(Tile t){int[] f={3,2,1,1,1,0,2,3,1,2,0,3,2,1,0,0,2,0,1,2,2},p={1,2,3,1,0,2,0,1,1,1,1,2,0,2,0,1,1,3,1,2,1};return new[]{f[t.terrain]+(t.farm?2:0)+(t.resource==1?2:0),p[t.terrain]+(t.mine?2:0)+(t.resource==2?2:0),t.resource==3?2:0};}
        public bool HasBuilding(City c,int b){return b==7?c.granary:b==8?c.library:b==9?c.walls:c.buildings.Contains(b);}
        public int Happiness(City c){return 5-c.pop/2+(1-nations[c.owner].tax)*2+(HasBuilding(c,33)?2:0)+(HasTech(c.owner,"longevity")?2:0)+(HasResource(c.owner,5)?2:0)-(c.occupation>0?3:0)-(nations[c.owner].arrears>0?1:0);}
        public List<Tile> Worked(City c){return tiles.Where(t=>t.owner==c.owner&&Distance(t.x,t.y,c.x,c.y)<=2&&!(t.x==c.x&&t.y==c.y)&&!cities.Any(a=>a.x==t.x&&a.y==t.y)&&cities.Where(a=>a.owner==c.owner).OrderBy(a=>Distance(a.x,a.y,t.x,t.y)).ThenBy(a=>a.id).First()==c).OrderByDescending(t=>{var v=TileYield(t);return c.focus==1?v[0]*4+v[1]:c.focus==2?v[1]*4+v[0]:v[0]*2+v[1]+v[2];}).Take(c.pop).ToList();}
        public int[] Yield(City c){int f=2,p=2,g=0;foreach(var t in Worked(c)){var v=TileYield(t);f+=v[0];p+=v[1];g+=v[2];}if(HasBuilding(c,30))p+=3;int s=2+c.pop+(c.library?4:0)+(nations[c.owner].tech[5]?3:0);if(c.focus==3){s+=2;p=Math.Max(1,p-2);}ApplyResearchYields(c,ref f,ref p,ref s);if(nations[c.owner].trait==1)p++;if(nations[c.owner].trait==2)s++;if(Happiness(c)<0){p=Math.Max(1,p/2);s=Math.Max(1,s/2);}if(c.occupation>0){p=Math.Max(1,p/2);s=Math.Max(1,s/2);}return new[]{f-(HasTech(c.owner,"uploading")?0:c.pop*2),p,s,g};}
        public int[] Budget(int owner){int income=0,expense=0;var own=cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).ToList();foreach(var c in own){int tax=c.pop*(nations[owner].tax==0?3:nations[owner].tax==2?5:4)/4;income+=tax+Yield(c)[3]+(HasBuilding(c,31)?3:0)+(Connected(c)&&c!=own[0]?3:0);int distance=Distance(c.x,c.y,own[0].x,own[0].y)/10;expense+=Math.Max(0,1+c.pop/4+distance-(HasBuilding(c,34)?2:0))+c.buildings.Count+(c.library?1:0);}
            int military=units.Where(u=>u.owner==owner).Sum(u=>Upkeep[u.type]);expense+=Math.Max(0,military-4);if(nations[owner].trait==3)income+=own.Count;return new[]{income,expense,income-expense};}
        public City Found(Unit u){if(result!=""||u==null||!units.Contains(u)||((At(u.x,u.y).terrain==8||At(u.x,u.y).terrain==13)&&!HasTech(u.owner,"fire"))||u.type!=0||u.moves==0||IsImpassable(At(u.x,u.y).terrain)||(At(u.x,u.y).owner>=0&&At(u.x,u.y).owner!=u.owner)||cities.Any(c=>Distance(u.x,u.y,c.x,c.y)<4))return null;int count=cities.Count(c=>c.owner==u.owner)+1;var cnew=new City{id=nextId++,owner=u.owner,x=u.x,y=u.y,name=(u.owner==0?"Hearth ":NationName(u.owner)+" ")+count};cities.Add(cnew);units.Remove(u);At(u.x,u.y).road=true;RebuildTerritory();if(u.owner==0)Note(cnew.name+" founded.");Reveal();return cnew;}
        public static int UnitBuild(int type){return type<7?type:100+type;}
        public static int BuildUnit(int build){return build>=0&&build<7?build:build>=107&&build<=110?build-100:-1;}
        public static string BuildName(int b){if(b<0)return "Stockpile";int u=BuildUnit(b);if(u>=0)return UnitNames[u];int i=Array.IndexOf(BuildingIds,b);return i<0?"Unknown":BuildingNames[i];}
        public int Cost(int b){int u=BuildUnit(b);int i=Array.IndexOf(BuildingIds,b);int cost=u>=0?UnitCosts[u]:i>=0?BuildingCosts[i]:0;return cost*(speed==0?1:speed==2?3:2)/2;}
        public string BuildReason(City c,int b){if(b==-1)return "";int u=BuildUnit(b);if(u>=0){if(UnitTech[u]>=0&&!nations[c.owner].tech[UnitTech[u]])return "Requires "+TechNames[UnitTech[u]];if(!HasResource(c.owner,UnitResource[u]))return "Requires improved, road-connected "+ResourceNames[UnitResource[u]];return "";}int i=Array.IndexOf(BuildingIds,b);if(i<0)return "Unknown project";if(HasBuilding(c,b))return "Already built";return !nations[c.owner].tech[BuildingTech[i]]?"Requires "+TechNames[BuildingTech[i]]:"";}
        public List<int> Choices(City c){return Enumerable.Range(0,UnitNames.Length).Select(UnitBuild).Concat(BuildingIds).Where(b=>BuildReason(c,b)=="").ToList();}
        // Training and construction each receive the city's production output every turn.
        public bool QueueBuild(City c,int b){return ChooseProject(c,b,true);}
        public bool ChooseProject(City c,int b,bool append){
            if(c==null||b<0||BuildReason(c,b)!="")return false;
            bool building=BuildUnit(b)<0;var queue=building?c.buildingQueue:c.queue;int current=building?c.buildingProject:c.build;
            if(building&&append&&(current==b||queue.Contains(b)))return false;
            if(append&&current>=0&&queue.Count>=8)return false;
            if(!append){queue.Clear();if(building){if(c.buildingProject!=b)c.buildingProduction=0;c.buildingProject=b;}else{if(c.build!=b)c.production=0;c.build=b;}return true;}
            if(current<0){if(building)c.buildingProject=b;else c.build=b;}else queue.Add(b);return true;
        }
        public bool RemoveLastProject(City c,bool building){if(c==null)return false;var queue=building?c.buildingQueue:c.queue;
            if(queue.Count>0){queue.RemoveAt(queue.Count-1);return true;}
            if(building){if(c.buildingProject<0)return false;c.buildingProject=-1;c.buildingProduction=0;}else{if(c.build<0)return false;c.build=-1;c.production=0;}return true;}
        public void SkipTurn(Unit u){if(u==null||!units.Contains(u)||u.owner!=0||result!="")return;CancelOrder(u);u.fortified=false;u.moves=0;}
        public static string StackLabel(IEnumerable<Unit> source){var stack=source.ToList();if(stack.Count==0)return "";if(stack.GroupBy(u=>u.owner).Any(g=>g.Select(u=>u.type).Distinct().Count()>1))return "";return stack.Count>1?"Stack":UnitNames[stack[0].type];}
        public List<int> AvailableTech(int owner=0){return Enumerable.Range(0,TechNames.Length).Where(i=>!nations[owner].tech[i]&&Requirements[i].All(j=>nations[owner].tech[j])).ToList();}
        public int ResearchCost(int t){return TechCosts[t]*(speed==0?1:speed==2?3:2)/2;}
        public void SelectResearch(int t){if(!AvailableTech().Contains(t))return;nations[0].progress[research<0?0:research]=science;research=t;nations[0].research=t;science=nations[0].progress[t];}
        public void CancelOrder(Unit u){if(u==null)return;u.hasOrder=false;u.job=0;u.jobTurns=0;}
        public int[] PeaceOwners(Unit u,int x,int y){if(u==null||!VisibleTo(u.owner,x,y))return new int[0];var owners=units.Where(a=>a.x==x&&a.y==y).Select(a=>a.owner).Concat(cities.Where(a=>a.x==x&&a.y==y).Select(a=>a.owner));var t=At(x,y);if(t!=null&&t.owner>=0&&!CanEnter(u.owner,t))owners=owners.Concat(new[]{t.owner});return owners.Where(a=>a!=u.owner&&!AtWar(u.owner,a)).Distinct().ToArray();}
        public int StepCost(Unit u,int x,int y,Tile t){return At(x,y).road&&t.road?1:u.type==3?2:(RoughTerrain(t.terrain)?3:2);}
        bool Capacity(Unit u,int x,int y){if(u.type==1)return true;return units.Count(a=>a.type!=1&&a.id!=u.id&&a.owner==u.owner&&a.x==x&&a.y==y&&(Power[a.type]>0)==(Power[u.type]>0))<(Power[u.type]>0?3:1);}
        public List<Tile> Route(Unit u,int x,int y){var path=new List<Tile>();var goal=At(x,y);if(u==null||goal==null)return path;if(nations[u.owner].known[y*Width+x]&&IsImpassable(u.owner==0?KnownTile(goal).terrain:goal.terrain))return path;int start=u.y*Width+u.x,end=y*Width+x;var dist=Enumerable.Repeat(int.MaxValue,tiles.Length).ToArray();var prev=Enumerable.Repeat(-1,tiles.Length).ToArray();var open=new List<int>{start};dist[start]=0;
            while(open.Count>0){int current=open.OrderBy(i=>dist[i]).First();open.Remove(current);if(current==end)break;var from=tiles[current];for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var t=At(from.x+dx,from.y+dy);if(t==null||(dx==0&&dy==0))continue;int next=t.y*Width+t.x;bool known=nations[u.owner].known[next];if(known&&IsImpassable(u.owner==0?KnownTile(t).terrain:t.terrain))continue;bool visible=VisibleTo(u.owner,t.x,t.y);if(visible&&next!=end&&(!CanEnter(u.owner,t)||units.Any(a=>a.owner!=u.owner&&a.x==t.x&&a.y==t.y)))continue;if(!Capacity(u,t.x,t.y))continue;int cost=dist[current]+(known?StepCost(u,from.x,from.y,u.owner==0?KnownTile(t):t):2);if(cost>=dist[next])continue;dist[next]=cost;prev[next]=current;if(!open.Contains(next))open.Add(next);}}
            if(start==end||prev[end]<0)return path;for(int i=end;i!=start;i=prev[i])path.Add(tiles[i]);path.Reverse();return path;}
        public bool Order(Unit u,int x,int y){if(u==null||result!="")return false;CancelOrder(u);u.fortified=false;if(u.x==x&&u.y==y)return true;if(Route(u,x,y).Count==0)return false;u.destinationX=x;u.destinationY=y;u.hasOrder=true;ContinueOrder(u);return true;}
        public void ContinueOrder(Unit u){while(u.hasOrder&&units.Contains(u)&&result==""){if(u.x==u.destinationX&&u.y==u.destinationY){u.hasOrder=false;break;}if(u.moves==0)break;var p=Route(u,u.destinationX,u.destinationY);if(p.Count==0){u.hasOrder=false;if(u.owner==0)Note("Order stopped: route blocked.");break;}var t=p[0];if(PeaceOwners(u,t.x,t.y).Length>0||!CanEnter(u.owner,t)){u.hasOrder=false;if(u.owner==0)Note("Order stopped at a peaceful border or unit.");break;}if(u.moves<StepCost(u,u.x,u.y,t))break;if(!Move(u,t.x,t.y)){u.hasOrder=false;break;}}}
        int Strength(Unit u,Unit enemy,bool defending){int s=Power[u.type]+u.promotion;if(u.type==7&&(enemy.type==5||enemy.type==8))s+=5;if(defending){var t=At(u.x,u.y);if(RoughTerrain(t.terrain))s+=2;if(t.fort)s+=2;if(u.fortified)s+=2;if(cities.Any(c=>c.x==u.x&&c.y==u.y&&c.walls))s+=3;}if(nations[u.owner].arrears>0)s=Math.Max(1,s-2);return Math.Max(1,s);}
        public bool Move(Unit u,int x,int y){if(result!=""||u==null||!units.Contains(u)||Distance(u.x,u.y,x,y)!=1)return false;var t=At(x,y);if(t==null||IsImpassable(t.terrain)||!CanEnter(u.owner,t)||!Capacity(u,x,y))return false;int cost=StepCost(u,u.x,u.y,t);if(u.moves<cost)return false;var enemies=units.Where(a=>a.owner!=u.owner&&a.x==x&&a.y==y).ToList();var c=cities.Find(a=>a.owner!=u.owner&&a.x==x&&a.y==y);
            if(enemies.Count>0||c!=null){if(Power[u.type]==0||enemies.Any(a=>!AtWar(u.owner,a.owner))||(c!=null&&!AtWar(u.owner,c.owner)))return false;if(enemies.Count>0){var e=enemies.OrderByDescending(a=>Strength(a,u,true)).First();int attack=Strength(u,e,false),defence=Strength(e,u,true);e.hp-=Math.Max(1,attack*5/defence);u.hp-=Math.Max(1,defence*4/attack);u.moves=0;u.experience+=2;e.experience++;if(u.owner==0||e.owner==0)Note(UnitNames[u.type]+" battles "+UnitNames[e.type]+".");units.RemoveAll(a=>a.hp<=0);if(u.hp<=0||enemies.Any(a=>a.hp>0)){CheckVictory();return true;}}
                if(c!=null){if(u.type==4||u.type==9){u.moves=0;CheckVictory();return true;}c.owner=u.owner;c.occupation=4;c.pop=Math.Max(1,c.pop-1);c.queue.Clear();c.build=-1;c.production=0;c.buildingQueue.Clear();c.buildingProject=-1;c.buildingProduction=0;c.walls=false;RebuildTerritory();if(u.owner==0||Visible(x,y))Note(c.name+" occupied for 4 turns.");}}
            u.x=x;u.y=y;u.moves=Math.Max(0,u.moves-cost);u.fortified=false;networks=null;Reveal();CheckVictory();return true;}
        public List<Unit> RangedTargets(Unit u){return units.Where(e=>e.owner!=u.owner&&AtWar(u.owner,e.owner)&&Distance(u.x,u.y,e.x,e.y)<=2&&VisibleTo(u.owner,e.x,e.y)).ToList();}
        public bool Ranged(Unit u,int targetId){if(u==null||result!=""||u.moves<=0||(u.type!=4&&u.type!=9))return false;var e=RangedTargets(u).Find(a=>a.id==targetId);if(e==null)return false;int dx=Math.Sign(e.x-u.x),dy=Math.Sign(e.y-u.y);var mid=At(u.x+dx,u.y+dy);if(Distance(u.x,u.y,e.x,e.y)>1&&mid!=null&&(Wooded(mid.terrain)||mid.terrain==5))return false;var city=cities.Find(c=>c.x==e.x&&c.y==e.y&&c.owner==e.owner);if(u.type==9&&city!=null&&city.walls){city.walls=false;Note("City walls breached.");}else e.hp-=Math.Max(1,(u.type==9?6:4)*4/Strength(e,u,true));u.moves=0;u.experience++;units.RemoveAll(a=>a.hp<=0);CheckVictory();return true;}
        public bool Promote(Unit u){if(u==null||u.experience<(u.promotion+1)*5||u.promotion>=3)return false;u.experience-=(u.promotion+1)*5;u.promotion++;return true;}
        public int UpgradeTarget(Unit u){return u.type==2?7:u.type==7?6:u.type==8?5:-1;}
        public bool Upgrade(Unit u){if(u==null||u.moves<=0)return false;int t=UpgradeTarget(u);if(t<0||!cities.Any(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y)||!nations[u.owner].tech[UnitTech[t]]||!HasResource(u.owner,UnitResource[t]))return false;int price=(UnitCosts[t]-UnitCosts[u.type])*2;if(u.owner==0)nations[0].gold=gold;if(nations[u.owner].gold<price)return false;nations[u.owner].gold-=price;if(u.owner==0)gold=nations[0].gold;u.type=t;u.moves=0;CancelOrder(u);return true;}
        public bool Improve(Unit u,bool farm){return StartJob(u,farm?2:1);}
        public string JobReason(Unit u,int job){if(u==null||u.type!=1||u.moves<=0)return "Worker needs movement.";var t=At(u.x,u.y);if(IsImpassable(t.terrain))return "Requires passable land.";if(job==1&&!HasTech(u.owner,"wheel"))return "Requires The Wheel.";if(job==1)return !CanEnter(u.owner,t)?"Requires access to this territory.":t.road?"Road already built.":"";if(t.owner!=u.owner)return "Requires your territory.";if(job==2){if(!nations[u.owner].tech[0])return "Requires Agriculture.";return t.farm||t.mine||!Farmable(t.terrain)?"Tile cannot support a farm/pasture.":"";}if(job==3){if(!HasTech(u.owner,"mining"))return "Requires Mining.";return t.mine||t.farm||!(t.terrain==2||t.resource==2||t.resource==3)?"Requires hills or a mineral deposit.":"";}if(job==4)return !nations[u.owner].tech[3]?"Requires Engineering.":t.fort?"Fort already built.":"";return "Unknown job.";}
        public int JobWork(Unit u,int job){var t=At(u.x,u.y);return job==1?2+(t.owner==u.owner?0:t.terrain==2?2:1):3;}
        public List<Unit> WorkerTeam(Unit u){return u==null?new List<Unit>():u.type!=1||u.workerGroup==0?new List<Unit>{u}:units.Where(a=>a.type==1&&a.owner==u.owner&&a.workerGroup==u.workerGroup&&a.x==u.x&&a.y==u.y).ToList();}
        public bool GroupWorkers(Unit u){if(u==null||u.type!=1)return false;var team=units.Where(a=>a.type==1&&a.owner==u.owner&&a.x==u.x&&a.y==u.y).ToList();if(team.Count<2)return false;foreach(var a in team){CancelOrder(a);a.fortified=false;a.workerGroup=u.id;}return true;}
        public void UngroupWorkers(Unit u){if(u==null||u.workerGroup==0)return;int group=u.workerGroup;foreach(var a in units.Where(a=>a.owner==u.owner&&a.workerGroup==group)){CancelOrder(a);a.workerGroup=0;}}
        public void CancelActivity(Unit u){foreach(var a in WorkerTeam(u)){CancelOrder(a);a.fortified=false;}}
        public bool StartJob(Unit u,int job){var team=WorkerTeam(u);if(team.Count==0||team.Any(a=>JobReason(a,job)!=""))return false;int work=JobWork(u,job);foreach(var a in team){CancelOrder(a);a.fortified=false;a.job=job;a.jobTurns=work;a.moves=0;}return true;}
        public int JobTurnsRemaining(Unit u){if(u==null||u.job==0)return 0;int count=WorkerTeam(u).Count(a=>a.job==u.job);return (u.jobTurns+Math.Max(1,count)-1)/Math.Max(1,count);}
        public string Activity(Unit u){if(u.job>0)return new[]{"","Building road","Building farm / pasture","Building mine","Building fort"}[u.job];if(u.hasOrder)return "Travelling to "+u.destinationX+", "+u.destinationY;if(u.fortified)return "Fortifying / healing";return u.moves==0?"Waiting for next turn":"Idle";}
        public string Remaining(Unit u){if(u.job>0)return JobTurnsRemaining(u)+" turns remaining";if(!u.hasOrder)return "Turns remaining: —";var route=Route(u,u.destinationX,u.destinationY);if(route.Count==0)return "Route blocked";int turns=0,budget=u.moves,x=u.x,y=u.y;foreach(var t in route){int cost=StepCost(u,x,y,u.owner==0?KnownTile(t):t);if(cost>budget){turns++;budget=Moves[u.type];}budget-=cost;x=t.x;y=t.y;}return "Approx. "+turns+" turns remaining";}
        void ProcessJobs(int owner){
            var processed=new HashSet<int>();
            foreach(var u in units.Where(a=>a.owner==owner&&a.job>0).ToList()){
                if(!processed.Add(u.id))continue;
                var team=WorkerTeam(u).Where(a=>a.job==u.job).ToList();foreach(var a in team){processed.Add(a.id);a.moves=0;}
                var t=At(u.x,u.y);if(IsImpassable(t.terrain)||(u.job==1?!CanEnter(owner,t):t.owner!=owner)){foreach(var a in team)CancelOrder(a);continue;}
                int job=u.job,remaining=Math.Max(0,team.Min(a=>a.jobTurns)-team.Count);
                foreach(var a in team)a.jobTurns=remaining;
                if(remaining==0){if(job==1)t.road=true;if(job==2)t.farm=true;if(job==3)t.mine=true;if(job==4)t.fort=true;foreach(var a in team)a.job=0;networks=null;if(owner==0)Note("Workers completed an improvement.");}
            }
        }
        public bool Pillage(Unit u){if(u==null||Power[u.type]==0||u.moves<=0)return false;var t=At(u.x,u.y);if(t.owner<0||!AtWar(u.owner,t.owner)||!(t.road||t.farm||t.mine||t.fort))return false;t.road=t.farm=t.mine=t.fort=false;u.moves=0;networks=null;return true;}
        void Complete(City c){int b=c.build,u=BuildUnit(b);if(u<0)return;var probe=new Unit{owner=c.owner,type=u,id=-1};if(!Capacity(probe,c.x,c.y))return;var made=Add(c.owner,u,c.x,c.y);made.moves=0;if(HasBuilding(c,32))made.experience=5;c.production-=Cost(b);if(c.owner==0)Note(c.name+" trained "+BuildName(b)+".");c.build=c.queue.Count>0?c.queue[0]:-1;if(c.queue.Count>0)c.queue.RemoveAt(0);if(c.build<0)c.production=0;}
        void CompleteBuilding(City c){int b=c.buildingProject;if(HasBuilding(c,b))return;if(b==7)c.granary=true;else if(b==8)c.library=true;else if(b==9)c.walls=true;else c.buildings.Add(b);c.buildingProduction-=Cost(b);if(c.owner==0)Note(c.name+" constructed "+BuildName(b)+".");c.buildingProject=c.buildingQueue.Count>0?c.buildingQueue[0]:-1;if(c.buildingQueue.Count>0)c.buildingQueue.RemoveAt(0);if(c.buildingProject<0)c.buildingProduction=0;}
        public void Produce(City c,int output){
            if(c.build>=0&&BuildReason(c,c.build)==""){c.production+=output;if(c.production>=Cost(c.build))Complete(c);}
            if(c.buildingProject>=0&&BuildReason(c,c.buildingProject)==""){c.buildingProduction+=output;if(c.buildingProduction>=Cost(c.buildingProject))CompleteBuilding(c);}
        }
        void ProcessNation(int owner){var n=nations[owner];var budget=Budget(owner);int balance=n.gold+budget[2];n.gold=Math.Max(0,balance);n.arrears=balance<0?n.arrears+1:0;if(owner==0&&balance<0)Note("Treasury empty: unpaid upkeep reduces combat strength. Reduce expenses or raise taxes.");foreach(var c in cities.Where(a=>a.owner==owner).ToList()){var v=Yield(c);c.age++;c.food+=v[0];if(c.food>=12+c.pop*6&&c.pop<20&&Happiness(c)>=0){c.food=c.granary?c.food/2:0;c.pop++;}if(c.food<0){c.pop=Math.Max(1,c.pop-1);c.food=0;if(owner==0)Note(c.name+" suffers food shortages.");}if(c.occupation>0)c.occupation--;Produce(c,v[1]);if(n.research>=0&&AvailableTech(owner).Contains(n.research))n.progress[n.research]+=v[2];}
            if(n.research>=0&&AvailableTech(owner).Contains(n.research)&&n.progress[n.research]>=ResearchCost(n.research)){n.progress[n.research]-=ResearchCost(n.research);n.tech[n.research]=true;if(owner==0)Note(TechNames[n.research]+" discovered.");n.research=AvailableTech(owner).OrderBy(t=>ResearchCost(t)).DefaultIfEmpty(-1).First();}
            foreach(var u in units.Where(a=>a.owner==owner).ToList()){if(u.moves==Moves[u.type]||u.fortified)u.hp=Math.Min(10,u.hp+1+(At(u.x,u.y).owner==owner?1:0)+(cities.Any(c=>c.owner==owner&&c.x==u.x&&c.y==u.y&&HasBuilding(c,35))?2:0)+(units.Any(a=>a.owner==owner&&a.type==10&&Distance(a.x,a.y,u.x,u.y)<=1)?2:0));u.moves=Moves[u.type];}
            ProcessJobs(owner);
        }
        public void EndTurn(){if(processing||result!="")return;processing=true;try{nations[0].gold=gold;nations[0].tech=tech;nations[0].research=research;if(research>=0)nations[0].progress[research]=science;
            for(int owner=1;owner<3;owner++){ProcessNation(owner);AI(owner);}turn++;RebuildTerritory();ProcessNation(0);gold=nations[0].gold;tech=nations[0].tech;research=nations[0].research;science=research<0?0:nations[0].progress[research];Reveal();foreach(var u in units.Where(a=>a.owner==0&&a.hasOrder).ToList())ContinueOrder(u);CheckVictory();}finally{processing=false;}}
        void AI(int owner){nations[owner].tax=Budget(owner)[2]<0?2:1;var r=new Random(seed+turn*997+owner);foreach(var c in cities.Where(a=>a.owner==owner)){var own=units.Where(a=>a.owner==owner).ToList();if(c.build<0){var choices=Choices(c);int desired=own.Count(u=>u.type==0)==0&&cities.Count(a=>a.owner==owner)<4?0:own.Count(u=>u.type==1)<cities.Count(a=>a.owner==owner)?1:own.Count>=14||(Budget(owner)[2]<0&&nations[owner].gold<15)?-1:choices.Where(b=>BuildUnit(b)>=2).OrderByDescending(b=>Power[BuildUnit(b)]).DefaultIfEmpty(2).First();c.build=desired;}if(c.buildingProject<0){int building=Choices(c).FirstOrDefault(b=>BuildUnit(b)<0);if(BuildingIds.Contains(building))ChooseProject(c,building,false);}}
            foreach(var u in units.Where(a=>a.owner==owner).ToList()){if(!units.Contains(u)||result!=""||u.job>0)continue;if(u.type==0){if(Found(u)!=null)continue;var site=tiles.Where(t=>nations[owner].known[t.y*Width+t.x]&&!IsImpassable(t.terrain)&&(t.owner<0||t.owner==owner)&&cities.All(c=>Distance(c.x,c.y,t.x,t.y)>=4)).OrderBy(t=>Distance(u.x,u.y,t.x,t.y)).FirstOrDefault();if(site!=null)Order(u,site.x,site.y);continue;}
                if(u.type==1){var t=At(u.x,u.y);int job=!t.road?1:!t.mine&&(t.resource==2||t.terrain==2)?3:!t.farm&&t.terrain==0?2:0;if(job>0&&StartJob(u,job))continue;var tile=tiles.Where(a=>a.owner==owner&&!IsImpassable(a.terrain)&&!a.road).OrderBy(a=>Distance(u.x,u.y,a.x,a.y)).FirstOrDefault();if(tile!=null)Order(u,tile.x,tile.y);continue;}
                if((u.type==4||u.type==9)&&RangedTargets(u).Count>0&&Ranged(u,RangedTargets(u)[0].id))continue;
                var target=units.Where(a=>AtWar(owner,a.owner)&&VisibleTo(owner,a.x,a.y)).OrderBy(a=>Distance(u.x,u.y,a.x,a.y)).FirstOrDefault();if(target!=null){Order(u,target.x,target.y);continue;}
                var enemyCity=cities.Where(c=>AtWar(owner,c.owner)&&VisibleTo(owner,c.x,c.y)).OrderBy(c=>Distance(u.x,u.y,c.x,c.y)).FirstOrDefault();if(enemyCity!=null){Order(u,enemyCity.x,enemyCity.y);continue;}
                for(int attempt=0;attempt<8;attempt++){int x=u.x+r.Next(-1,2),y=u.y+r.Next(-1,2);if(Move(u,x,y))break;}
            }
        }
        public string Calendar(){int elapsed=turn-1;int year=-4000+Math.Min(elapsed,100)*40+Math.Min(Math.Max(0,elapsed-100),200)*10+Math.Max(0,elapsed-300);return year<0?(-year)+" BCE":(year+1)+" CE";}
        public void CheckVictory(){if(result!="")return;if(HasTech(0,"transcendence")){result="Science victory! Your civilisation has achieved Transcendence.";return;}if(nations.Skip(1).Any(n=>n.tech[TechnologyCatalog.Index("transcendence")])){result="A rival civilisation has achieved a science victory.";return;}if(!units.Any(u=>u.owner==0)&&!cities.Any(c=>c.owner==0))result="Your civilisation has fallen.";else if(!units.Any(u=>u.owner>0)&&!cities.Any(c=>c.owner>0))result="Conquest victory! Earth is united.";}
        public void UpgradeSave(){if(version==1){civilisationName="Verdant Union";met=new[]{true,false,false};version=2;}if(version==2){var oldTech=tech;tech=new bool[TechnologyCatalog.Count];if(oldTech!=null)Array.Copy(oldTech,tech,Math.Min(6,oldTech.Length));nations=new[]{new Nation(),new Nation(),new Nation()};nations[0].tech=tech;nations[0].gold=gold;nations[0].research=research;if(research>=0&&research<12)nations[0].progress[research]=science;openBorders=new bool[3];foreach(var c in cities){c.buildings=new List<int>();c.queue=new List<int>();}foreach(var t in tiles){t.rememberedTerrain=t.terrain;t.rememberedResource=t.resource;t.rememberedOwner=-1;t.rememberedRoad=t.road;t.rememberedFarm=t.farm;}version=3;RebuildTerritory();Reveal();Note("Save upgraded to foundation alpha 0.2.");}if(version==3){foreach(var c in cities){int oldBuild=c.build,oldProduction=c.production;var oldQueue=c.queue??new List<int>();c.build=-1;c.production=0;c.queue=new List<int>();c.buildingProject=-1;c.buildingProduction=0;c.buildingQueue=new List<int>();foreach(int project in (oldBuild>=0?new[]{oldBuild}:new int[0]).Concat(oldQueue)){bool building=BuildUnit(project)<0;if(building){if(HasBuilding(c,project)||c.buildingProject==project||c.buildingQueue.Contains(project))continue;if(c.buildingProject<0)c.buildingProject=project;else c.buildingQueue.Add(project);}else{if(c.build<0)c.build=project;else c.queue.Add(project);}}if(oldBuild>=0){if(BuildUnit(oldBuild)>=0)c.production=oldProduction;else c.buildingProduction=oldProduction;}}version=4;}
            if(version==4)MigrateTechnologySave();
            if(version==5&&nations!=null&&nations.Length==3&&nations[0]!=null)nations[0].tech=tech;networks=null;}
        public bool Valid(){if(version!=5||turn<1||nextId<1||tiles==null||tiles.Length!=Width*Height||units==null||cities==null||log==null||result==null||tech==null||tech.Length!=TechnologyCatalog.Count||met==null||met.Length!=3||war==null||war.Length!=3||openBorders==null||openBorders.Length!=3||nations==null||nations.Length!=3||string.IsNullOrWhiteSpace(civilisationName)||civilisationName.Length>32||gold<0||science<0||research< -1||research>=TechnologyCatalog.Count||speed<0||speed>2||flag<0||flag>2)return false;
            if(nations.Any(n=>n==null||n.tech==null||n.tech.Length!=TechnologyCatalog.Count||n.progress==null||n.progress.Length!=TechnologyCatalog.Count||n.known==null||n.known.Length!=tiles.Length||n.research< -1||n.research>=TechnologyCatalog.Count||n.progress.Any(v=>v<0)||n.tax<0||n.tax>2||n.gold<0))return false;
            for(int i=0;i<tiles.Length;i++){var t=tiles[i];if(t==null||t.x!=i%Width||t.y!=i/Width||t.terrain<0||t.terrain>=TerrainNames.Length||t.rememberedTerrain<0||t.rememberedTerrain>=TerrainNames.Length||t.resource<0||t.resource>5||t.owner< -1||t.owner>2)return false;}
            if(units.Any(u=>u==null||u.type<0||u.type>=UnitNames.Length||u.owner<0||u.owner>2||At(u.x,u.y)==null||u.hp<1||u.hp>10||u.moves<0||u.moves>Moves[u.type]||u.id<1||u.id>=nextId||u.job<0||u.job>4||u.jobTurns<0||u.workerGroup<0||(u.workerGroup>0&&u.type!=1)||(u.hasOrder&&At(u.destinationX,u.destinationY)==null)))return false;
            if(cities.Any(c=>c==null||c.owner<0||c.owner>2||At(c.x,c.y)==null||c.pop<1||c.pop>20||c.name==null||c.buildings==null||c.queue==null||c.buildingQueue==null||c.queue.Count>8||c.buildingQueue.Count>8||c.id<1||c.id>=nextId||c.food<0||c.production<0||(c.build!=-1&&BuildUnit(c.build)<0)||c.queue.Any(b=>BuildUnit(b)<0)||c.buildingProduction<0||(c.buildingProject!=-1&&(!BuildingIds.Contains(c.buildingProject)||HasBuilding(c,c.buildingProject)))||c.buildingQueue.Any(b=>!BuildingIds.Contains(b)||HasBuilding(c,b)||b==c.buildingProject)||c.buildingQueue.Distinct().Count()!=c.buildingQueue.Count||c.buildings.Distinct().Count()!=c.buildings.Count))return false;
            var ids=units.Select(u=>u.id).Concat(cities.Select(c=>c.id)).ToList();return ids.Distinct().Count()==ids.Count;}
    }
}
