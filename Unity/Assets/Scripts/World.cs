using System;
using System.Collections.Generic;
using System.Linq;

namespace ParadigmRealm
{
    [Serializable] public class Tile { public int x,y,terrain,resource; public bool seen,road,farm; }
    [Serializable] public class Unit { public int id,owner,x,y,type,hp=10,moves=4; }
    [Serializable] public class City { public int id,owner,x,y,pop=1,food,production,build=2; public string name; public bool granary,library,walls; }
    [Serializable] public class World
    {
        public const int Width=48, Height=30;
        public static readonly string[] TerrainNames={"Grassland","Forest","Hills","Desert","Ocean","Mountains"};
        public static readonly string[] UnitNames={"Settler","Worker","Warrior","Scout","Archer","Knight","Rifleman"};
        public static readonly string[] TechNames={"Agriculture","Archery","Writing","Engineering","Industry","Electricity"};
        public static readonly int[] Costs={36,20,18,14,28,42,58,32,40,35};
        public static readonly int[] Power={0,0,3,1,5,8,13};
        public static readonly int[] Moves={4,4,4,6,4,6,4};
        public static readonly int[] TechCosts={20,25,35,60,90,120};
        public static readonly int[][] Requirements={new int[0],new int[0],new[]{0},new[]{1,2},new[]{3},new[]{4}};
        public int version=2,seed,turn=1,nextId=1,gold=20,science,research;
        public Tile[] tiles;
        public List<Unit> units=new List<Unit>(); public List<City> cities=new List<City>();
        public List<string> log=new List<string>(); public bool[] tech=new bool[6],war=new bool[3];
        public string civilisationName="Verdant Union"; public bool[] met=new[]{true,false,false};
        public static readonly string[] ResourceNames={"None","Wheat","Iron","Gold"};
        public string NationName(int owner){return owner==0?civilisationName:owner==1?"Amber League":"Azure Dominion";}
        public string result="";
        public Tile At(int x,int y) { return x<0||y<0||x>=Width||y>=Height?null:tiles[y*Width+x]; }
        public static int Distance(int x,int y,int a,int b) {return Math.Max(Math.Abs(x-a),Math.Abs(y-b));}
        public void Note(string message) {log.Insert(0,"Turn "+turn+": "+message);if(log.Count>35)log.RemoveAt(35);}
        public Unit Add(int owner,int type,int x,int y) {var u=new Unit{id=nextId++,owner=owner,type=type,x=x,y=y,moves=Moves[type]};units.Add(u);return u;}
        public static World New(int seed,string civilisationName="Verdant Union")
        {
            var w=new World{civilisationName=CleanName(civilisationName),seed=seed,tiles=new Tile[Width*Height]};var r=new Random(seed);
            for(int y=0;y<Height;y++)for(int x=0;x<Width;x++){
                int n=r.Next(100),t=n<45?0:n<68?1:n<85?2:n<94?3:5;
                if(x==0||y==0||x==Width-1||y==Height-1||Math.Sin(x*.2)+Math.Cos(y*.29)*.7< -1.2)t=4;
                w.tiles[y*Width+x]=new Tile{x=x,y=y,terrain=t};
            }
            int[] sx={9,34,31},sy={12,9,23};
            // Connected starting regions while naval travel is deferred.
            for(int i=0;i<2;i++){int x=sx[i],y=sy[i];while(x!=sx[i+1]||y!=sy[i+1]){w.At(x,y).terrain=0;x+=Math.Sign(sx[i+1]-x);y+=Math.Sign(sy[i+1]-y);}}
            for(int i=0;i<3;i++){
                for(int y=sy[i]-3;y<=sy[i]+3;y++)for(int x=sx[i]-3;x<=sx[i]+3;x++)w.At(x,y).terrain=0;
                var u=w.Add(i,0,sx[i],sy[i]);w.Add(i,2,sx[i]+1,sy[i]);w.Add(i,1,sx[i],sy[i]+1);if(i>0)w.Found(u);
            }
            foreach(var tile in w.tiles){if(tile.terrain==0&&r.Next(7)==0)tile.resource=1;else if(tile.terrain==2&&r.Next(3)==0)tile.resource=2;else if(tile.terrain==3&&r.Next(5)==0)tile.resource=3;}
            w.Reveal();w.Note("Found your first city with your Settler.");return w;
        }
        public bool Visible(int x,int y) {return units.Any(u=>u.owner==0&&Distance(x,y,u.x,u.y)<=3)||cities.Any(c=>c.owner==0&&Distance(x,y,c.x,c.y)<=3);}
        public void Reveal(){foreach(var t in tiles)if(Visible(t.x,t.y))t.seen=true;
            for(int i=1;i<3;i++)if(!met[i]&&(units.Any(u=>u.owner==i&&Visible(u.x,u.y))||cities.Any(c=>c.owner==i&&Visible(c.x,c.y)))){met[i]=true;Note("First contact: "+NationName(i)+".");}}
        public static string CleanName(string value){var name=new string((value??"").Where(c=>!char.IsControl(c)&&c!='<'&&c!='>').ToArray()).Trim();return name.Length==0?"Verdant Union":name.Substring(0,Math.Min(32,name.Length));}
        public bool SetWar(int owner,bool value){if(owner<1||owner>2||!met[owner])return false;war[owner]=value;return true;}
        public int[] TileYield(Tile t){int[] f={3,2,1,1,1,0},p={1,2,3,1,0,2};return new[]{f[t.terrain]+(t.farm?2:0)+(t.resource==1?2:0),p[t.terrain]+(t.resource==2?2:0),t.resource==3?2:0};}
        public bool RoadConnects(Tile t,int dx,int dy){var n=At(t.x+dx,t.y+dy);return t.road&&n!=null&&n.seen&&n.road;}
        public void UpgradeSave(){if(version==1){civilisationName="Verdant Union";met=new[]{true,false,false};log.Clear();version=2;Reveal();for(int i=1;i<3;i++)if(war[i])met[i]=true;}}
        public City Found(Unit u)
        {
            if(result!=""||u==null||!units.Contains(u)||u.type!=0||u.moves==0||cities.Any(c=>Distance(u.x,u.y,c.x,c.y)<4))return null;
            string[] names={"Hearth","Dawnwatch","Stonehaven","Westmarch","New Hope"};int n=cities.Count(c=>c.owner==u.owner);
            var city=new City{id=nextId++,owner=u.owner,x=u.x,y=u.y,name=(u.owner==1?"Amber ":u.owner==2?"Azure ":"")+(n<names.Length?names[n]:"Outpost "+(n+1))};
            cities.Add(city);units.Remove(u);At(u.x,u.y).road=true;if(u.owner==0)Note(city.name+" founded.");Reveal();return city;
        }
        public bool AtWar(int a,int b){return a!=b&&(a!=0&&b!=0||war[a==0?b:a]);}
        public bool Move(Unit u,int x,int y)
        {
            if(result!=""||u==null||!units.Contains(u)||Distance(u.x,u.y,x,y)!=1)return false;
            var t=At(x,y);if(t==null||t.terrain>=4)return false;
            int cost=t.road&&At(u.x,u.y).road?1:(t.terrain==1||t.terrain==2?3:2);if(u.moves<cost)return false;
            var enemies=units.Where(a=>a.owner!=u.owner&&a.x==x&&a.y==y).ToList();var c=cities.Find(a=>a.owner!=u.owner&&a.x==x&&a.y==y);
            if(enemies.Count>0||c!=null){
                if(Power[u.type]==0||enemies.Any(a=>!AtWar(u.owner,a.owner))||(c!=null&&!AtWar(u.owner,c.owner)))return false;
                if(enemies.Count>0){
                    var e=enemies.OrderByDescending(a=>Power[a.type]).First();int defense=Power[e.type]+(t.terrain==1||t.terrain==2?2:0)+(c!=null&&c.walls?3:0);
                    e.hp-=Math.Max(2,Power[u.type]*6/Math.Max(1,defense));u.hp-=Math.Max(1,defense*4/Power[u.type]);u.moves=0;
                    if(u.owner==0||e.owner==0)Note(UnitNames[u.type]+" battles "+UnitNames[e.type]+".");units.RemoveAll(a=>a.hp<=0);
                    if(u.hp<=0||enemies.Any(a=>a.hp>0)){CheckVictory();return true;}
                }
                if(c!=null){c.owner=u.owner;c.pop=Math.Max(1,c.pop-1);c.build=2;if(u.owner==0||Visible(x,y))Note(c.name+" captured.");}
            }
            u.x=x;u.y=y;u.moves=Math.Max(0,u.moves-cost);Reveal();CheckVictory();return true;
        }
        public bool Improve(Unit u,bool farm)
        {
            if(result!=""||u==null||u.type!=1||u.moves==0)return false;var t=At(u.x,u.y);
            if(farm){if(!tech[0]||t.farm||(t.terrain!=0&&t.terrain!=3))return false;t.farm=true;}else{if(t.road)return false;t.road=true;}u.moves=0;return true;
        }
        public List<int> AvailableTech(){return Enumerable.Range(0,6).Where(i=>!tech[i]&&Requirements[i].All(j=>tech[j])).ToList();}
        public List<int> Choices(City c){var a=new List<int>{0,1,2,3};if(tech[1])a.Add(4);if(tech[3])a.Add(5);if(tech[4])a.Add(6);if(tech[0]&&!c.granary)a.Add(7);if(tech[2]&&!c.library)a.Add(8);if(tech[3]&&!c.walls)a.Add(9);return a;}
        public static string BuildName(int n){return n<0?"Stockpile":n<7?UnitNames[n]:n==7?"Granary":n==8?"Library":"Walls";}
        public int[] Yield(City c)
        {
            int[] food={3,2,1,1,1,0},prod={1,2,3,1,0,2};var nearby=new List<Tile>();
            for(int y=c.y-1;y<=c.y+1;y++)for(int x=c.x-1;x<=c.x+1;x++){var t=At(x,y);if(t!=null)nearby.Add(t);}
            int f=2,p=2,g=0;foreach(var t in nearby.OrderByDescending(t=>TileYield(t).Sum()).Take(c.pop)){var v=TileYield(t);f+=v[0];p+=v[1];g+=v[2];}
            return new[]{f-c.pop*2,p,2+c.pop+(c.library?4:0),g};
        }
        public void EndTurn()
        {
            if(result!="")return;turn++;
            foreach(var c in cities){var v=Yield(c);c.food+=v[0];if(c.food>=12+c.pop*6&&c.pop<9){c.food=c.granary?c.food/2:0;c.pop++;}if(c.food<0){c.pop=Math.Max(1,c.pop-1);c.food=0;}
                if(c.owner==0){science+=v[2];gold+=c.pop+v[3];}if(c.build<0)continue;c.production+=v[1];if(c.production>=Costs[c.build]){c.production-=Costs[c.build];int b=c.build;if(b<7)Add(c.owner,b,c.x,c.y);else{if(b==7)c.granary=true;if(b==8)c.library=true;if(b==9)c.walls=true;c.build=2;}if(c.owner==0)Note(c.name+" completes "+BuildName(b)+".");}}
            if(research>=0&&science>=TechCosts[research]){science-=TechCosts[research];tech[research]=true;Note(TechNames[research]+" discovered.");research=AvailableTech().DefaultIfEmpty(-1).First();}
            foreach(var u in units){if(u.moves==Moves[u.type])u.hp=Math.Min(10,u.hp+2);u.moves=Moves[u.type];}
            AI();Reveal();CheckVictory();
        }
        void AI()
        {
            var r=new Random(seed+turn*997);
            foreach(var u in units.Where(a=>a.owner>0).ToList()){
                if(!units.Contains(u)||result!="")continue;if(u.type==0&&Found(u)!=null)continue;
                if(u.type==1)At(u.x,u.y).road=true;
                var target=cities.Where(c=>AtWar(u.owner,c.owner)).OrderBy(c=>Distance(u.x,u.y,c.x,c.y)).FirstOrDefault();
                var steps=new List<int[]>();for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)if(dx!=0||dy!=0)steps.Add(new[]{u.x+dx,u.y+dy});
                steps=steps.OrderBy(a=>r.Next()).ToList();if(target!=null&&u.type>=2)steps=steps.OrderBy(a=>Distance(a[0],a[1],target.x,target.y)).ToList();foreach(var a in steps)if(Move(u,a[0],a[1]))break;
            }
            foreach(var c in cities.Where(a=>a.owner>0)){var own=units.Where(a=>a.owner==c.owner).ToList();c.build=own.Count>=18?-1:!own.Any(a=>a.type==0)&&cities.Count(a=>a.owner==c.owner)<5?0:turn>90?6:turn>55?5:turn>25?4:2;}
        }
        public void CheckVictory(){if(!units.Any(u=>u.owner==0)&&!cities.Any(c=>c.owner==0))result="Your civilisation has fallen.";else if(!units.Any(u=>u.owner>0)&&!cities.Any(c=>c.owner>0))result="Conquest victory! Earth is united.";}
        public bool Valid()
        {
            if(version!=2||met==null||met.Length!=3||!met[0]||string.IsNullOrWhiteSpace(civilisationName)||civilisationName.Length>32||turn<1||nextId<1||tiles==null||tiles.Length!=Width*Height||units==null||cities==null||tech==null||tech.Length!=6||war==null||war.Length!=3||log==null||result==null||research< -1||research>5)return false;
            for(int i=0;i<tiles.Length;i++){var t=tiles[i];if(t==null||t.x!=i%Width||t.y!=i/Width||t.terrain<0||t.terrain>5||t.resource<0||t.resource>3)return false;}
            if(units.Any(u=>u==null||u.type<0||u.type>6||u.owner<0||u.owner>2||At(u.x,u.y)==null||u.hp<1||u.hp>10||u.moves<0||u.moves>Moves[u.type]||u.id<1||u.id>=nextId))return false;
            if(cities.Any(c=>c==null||c.owner<0||c.owner>2||At(c.x,c.y)==null||c.pop<1||c.pop>9||c.build< -1||c.build>9||c.name==null||c.id<1||c.id>=nextId||c.food<0||c.production<0))return false;
            var ids=units.Select(u=>u.id).Concat(cities.Select(c=>c.id)).ToList();return ids.Distinct().Count()==ids.Count&&gold>=0&&science>=0&&(research<0||AvailableTech().Contains(research));
        }
    }
}
