using System;
using System.Collections.Generic;
using System.Linq;

namespace ParadigmRealm
{
    [Serializable] public class Tile { public int x,y,terrain,resource,owner=-1; public bool seen,road,farm,mine,fort,village,ruin,cleared; public int villageOwner=-1; public int swayCiv=-1,sway; public int rememberedTerrain,rememberedResource,rememberedOwner=-1; public bool rememberedRoad,rememberedFarm,rememberedMine,rememberedFort; public string rememberedCity=""; }
    [Serializable] public class Unit { public int homeCity; public bool exploring,sentry; public int id,owner,x,y,type,hp=10,moves=4,experience,promotion,job,jobTurns,workerGroup; public int victories; public bool hasOrder,fortified; public int destinationX,destinationY;
        // Save version 21: workers that develop land on their own, and road routes (build a road on every tile to roadX, roadY).
        public bool autoWork,roadOrder; public int roadX,roadY; }
    [Serializable] public class City {
        public int id,owner,x,y,pop=1,food,production,build=2,focus,occupation,age;
        public string name; public bool granary,library,walls; public int portX=-1,portY=-1; public bool portLinked;
        public List<int> buildings=new List<int>(),queue=new List<int>(),buildingQueue=new List<int>();
        public int scientists,merchants,entertainers; public int buildingProject=-1,buildingProduction;
        // Tiles (y * width + x) the player has locked a citizen onto (save version 15).
        public List<int> locked=new List<int>();
        // Religion (save version 22): pressure of each faith in ReligionCatalog, and the majority faith (-1 none).
        public int[] faithPressure=new int[ReligionCatalog.Count]; public int religion=-1;
    }
    [Serializable] public class Nation {
        // scienceRate (0-60, steps of 10) splits each city's Trade into science and gold (save version 11). tax is the legacy
        // Low/Normal/High setting, kept in step with the rate; it sets the low/high tax content slot.
        public int scienceRate=50;
        // Every civilisation starts knowing Stone Tools (save version 13: 36 keystone technologies). edicts[e] is the edict chosen
        // for era e (0-2), or -1 while still to choose or not yet reached.
        public int gold=20,tax=1,research=TechnologyCatalog.Index("agriculture"),arrears; public int[] progress=new int[TechnologyCatalog.Count];
        public bool[] tech=TechnologyCatalog.Starting(); public bool[] known=new bool[1440]; public int[] edicts=World.NoEdicts();
        // capital: id of the first city this civilisation founded (-1 before that). Transcendence project: the city building it,
        // stages completed (0-3) and turns left before the latest stage activates (save version 14).
        public int capital=-1,projectCity=-1,projectStage,projectActivation;
        // Religion (save version 22): faith stored, chosen pantheon, the faith this civilisation founded and its holy city (-1 none).
        public int faith,pantheon=-1,founded=-1,holyCity=-1;
        // Civilisation (save version 16): premade index 0-11 or -1 for custom/stock, leader name, portrait 0-14 or -1, two traits
        // (CivilisationCatalog; -1 = none). The single "trait" above is the pre-16 setting, kept only to convert old saves.
        public int civ=-1,portrait=-1; public string leader=""; public int[] traits={-1,-1};
        public int government,revolution,luxury,relations; public string name; public int trait;
        // Flag (save version 20): layout, field colours, emblem and emblem colour (FlagArt); flagPattern -1 means not yet set.
        public int flagPattern=-1,flagA,flagB,flagEmblem,flagInk;
    }
    [Serializable] public partial class World
    {
        public const int DefaultWidth=48,DefaultHeight=30;
        public static readonly string[] MapSizeNames={"Tiny","Small","Medium","Large","Huge","Massive"};
        public static readonly int[] MapWidths={32,48,80,120,180,320},MapHeights={20,30,50,75,110,200};
        // Sphere maps are 2:1 (width = 2 x height) so one tile covers roughly equal angles of longitude and latitude.
        public static readonly int[] SphereWidths={48,72,96,144,192,320},SphereHeights={24,36,48,72,96,160};
        public int mapWidth=DefaultWidth,mapHeight=DefaultHeight,mapSizeIndex=1;
        // 0 = flat map with hard edges (all saves before version 9), 1 = sphere: east-west wrap and crossing over the poles.
        // 2 = globe in staggered rows (version 19, owner request; every new globe game): rows run east-west and wrap, the poles
        // are the top and bottom rows, odd rows sit half a tile to the east. Shapes 0 and 1 keep the diagonal layout.
        public int mapShape;
        public bool Sphere{get{return mapShape==1||mapShape==2;}}
        public bool Staggered{get{return mapShape==2;}}
        public int Width{get{return mapWidth;}}
        public int Height{get{return mapHeight;}}
        public static readonly string[] TerrainNames={"Grassland","Forest","Hills","Desert","Ocean","Mountains","Lake","River","Arctic tundra","Bog","Desolate wasteland","Jungle","Coral reef","Arctic forest","Ice sheet","Glacier","Savanna","Volcanic terrain","Rocky coastline","Mangrove","Dry steppe"};
        // 0-5 land resources; 6-15 sea resources (owner request), placed by water type and latitude in SeedSeaResources.
        public static readonly string[] ResourceNames={"None","Wheat","Iron","Gold","Horses","Spices","Fish","Whales","Pearls","Crabs","Kelp","Oysters","Seals","Tuna","Sponges","Offshore oil"};
        public static readonly int[] ResourceFood={0,2,0,0,0,0,2,1,0,1,1,1,1,2,0,0},ResourceShields={0,0,2,0,0,0,0,1,0,0,1,0,1,0,0,0},ResourceTrade={0,0,0,2,0,0,0,1,3,1,0,2,0,1,2,0};
        public static bool IsSeaResource(int r){return r>=6&&r<=15;}
        // Resources stay hidden, and give nothing, until the civilisation knows the technology that uses them (owner request):
        // wheat with Agriculture, iron with Iron Working, gold with Bronze Working (mining), horses with Horseback Riding,
        // spices with Currency, whales, pearls, seals, tuna and sponges with Sailing, offshore oil with Refining.
        // Fish, crabs, kelp and oysters are known from the start (gathered from the shore).
        public static readonly string[] ResourceTech={"","agriculture","iron","bronze","horseback","currency","","sailing","sailing","","","","sailing","sailing","sailing","refining"};
        public bool ResourceKnown(int owner,int r){return r<=0||r>=ResourceTech.Length||ResourceTech[r]==""||owner<0||HasTech(owner,ResourceTech[r]);}
        public static readonly string[] UnitNames={"Settler","Worker","Warrior","Scout","Archer","Knight","Rifleman","Spearman","Horseman","Catapult","Medic","Caravan","Boat","Galley","War Galley"};
        // Stage 4 combat (CORE-DESIGN-16BIT.md): Attack and Defence per unit, health 10. Power is the attack value and doubles as
        // the "can fight" flag used across the code. Spearman, Horseman and Archer follow the design table; the rest are scaled to it.
        public static readonly int[] Attack={0,0,1,1,2,4,5,1,3,4,0,0,0,0,3},Defence={1,1,1,1,1,2,4,2,1,1,1,1,1,2,2};
        public static readonly int[] Power=Attack;
        public static int MilitaryValue(int type){return Attack[type]>0?Attack[type]+Defence[type]:0;}
        public static bool IsRanged(int type){return type==4||type==9;}
        public static bool Mounted(int type){return type==5||type==8;}
        // Damage one side deals in an exchange: max(1, 3 + own strength - enemy strength). Deterministic, no random swing.
        public static int Damage(int own,int enemy){return Math.Max(1,3+own-enemy);}
        // Added defence points: mountains and volcanic +3, hills +2, forest, jungle, arctic forest, mangrove and river +1.
        public static int TerrainDefence(int t){return t==5||t==17?3:t==2?2:Wooded(t)||t==7?1:0;}
        public static readonly int[] Moves={4,4,4,6,4,6,4,4,6,4,4,4,6,8,8};
        // Stage 3 costs from CORE-DESIGN-16BIT.md; Scout, Knight, Rifleman, Medic and Caravan are not in the design table and keep their old costs.
        public static readonly int[] UnitCosts={90,20,10,14,20,48,65,20,25,40,30,40,20,40,45};
        // Settlers come with Stone Tools, so a new city can queue Warriors then Settlers on turn 1 (design).
        public static readonly int[] UnitTech={TechnologyCatalog.Index("toolmaking"),-1,TechnologyCatalog.Index("toolmaking"),TechnologyCatalog.Index("toolmaking"),TechnologyCatalog.Index("archery"),TechnologyCatalog.Index("chivalry"),TechnologyCatalog.Index("rifling"),TechnologyCatalog.Index("bronze"),TechnologyCatalog.Index("horseback"),TechnologyCatalog.Index("maths"),TechnologyCatalog.Index("medicine"),TechnologyCatalog.Index("currency"),TechnologyCatalog.Index("sailing"),TechnologyCatalog.Index("engineering"),TechnologyCatalog.Index("engineering")};
        public static readonly int[] UnitResource={0,0,0,0,0,4,2,0,4,2,0,0,0,0,0};
        // Every unit beyond the free support (3 per city plus government) costs 1 gold.
        public static readonly int[] Upkeep={1,1,1,1,1,1,1,1,1,1,1,1,1,1,1};
        public const int FreeUnitsPerCity=3;
        public static readonly string[] TechNames=TechnologyCatalog.Names;
        public static readonly int[] TechCosts=TechnologyCatalog.Costs;
        public static readonly int[][] Requirements=TechnologyCatalog.Requirements;
        // Legacy ids 0..6 = units, 7..9 = buildings. New units use 100+type.
        public static readonly int[] Costs={36,20,18,14,28,48,65,32,40,35};
        public static readonly int[] BuildingIds={7,8,9,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,60,61,62,63,70,71,72};
        public static readonly string[] BuildingNames={"Granary","Library","Walls","Workshop","Market","Barracks","Gardens","Courthouse","Hospital","Temple","Aqueduct","University","Bank","Factory","Sewer system","Colosseum","Harbour","Port","Grand Archive","Sun Gardens","Merchant Exchange","War Academy","Transcendence: stage 1","Transcendence: stage 2","Transcendence: stage 3"};
        // Buildings come in tiers of 40, 60 and 80 shields; 60 and 80 tier buildings cost 1 gold upkeep. Wonders (0 here) cost WonderCost().
        public static readonly int[] BuildingCosts={40,40,40,60,60,40,40,60,60,40,60,80,80,80,80,60,40,60,0,0,0,0,300,400,500};
        public static readonly int[] BuildingTech={TechnologyCatalog.Index("pottery"),TechnologyCatalog.Index("writing"),TechnologyCatalog.Index("masonry"),TechnologyCatalog.Index("bronze"),TechnologyCatalog.Index("currency"),TechnologyCatalog.Index("bronze"),TechnologyCatalog.Index("agriculture"),TechnologyCatalog.Index("laws"),TechnologyCatalog.Index("medicine"),TechnologyCatalog.Index("mysticism"),TechnologyCatalog.Index("engineering"),TechnologyCatalog.Index("education"),TechnologyCatalog.Index("banking"),TechnologyCatalog.Index("industrialisation"),TechnologyCatalog.Index("medicine"),TechnologyCatalog.Index("construction"),TechnologyCatalog.Index("sailing"),TechnologyCatalog.Index("engineering"),TechnologyCatalog.Index("writing"),TechnologyCatalog.Index("calendar"),TechnologyCatalog.Index("economics"),TechnologyCatalog.Index("military_science"),TechnologyCatalog.Index("computers"),TechnologyCatalog.Index("computers"),TechnologyCatalog.Index("computers")};
        public int version=22,civCount=3,seed,turn=1,nextId=1,gold=20,science,research=TechnologyCatalog.Index("agriculture"),mapType,speed=1,difficulty=1,trait;
        public Tile[] tiles; public List<Unit> units=new List<Unit>(); public List<City> cities=new List<City>();
        public List<string> log=new List<string>(); public bool[] tech=TechnologyCatalog.Starting(),war=new bool[3],met=new[]{true,false,false},openBorders=new bool[3];
        public Nation[] nations=new[]{new Nation(),new Nation(),new Nation()};
        public string civilisationName="Verdant Union",result=""; public int flag;
        [NonSerialized] bool processing;
        [NonSerialized] HashSet<int>[] networks;
        public Tile At(int x,int y){
            if(mapShape==1){
                // Going past a pole lands on the same row, half way round the world, travelling the other way.
                if(y<0){y=-y-1;x+=Width/2;}else if(y>=Height){y=2*Height-1-y;x+=Width/2;}
                if(y<0||y>=Height)return null;
                x%=Width;if(x<0)x+=Width;return tiles[y*Width+x];
            }
            if(mapShape==2){if(y<0||y>=Height)return null;x%=Width;if(x<0)x+=Width;return tiles[y*Width+x];}
            return x<0||y<0||x>=Width||y>=Height?null:tiles[y*Width+x];
        }
        // Game rules move on a square grid of 8 neighbours, the axes of the diagonal view (i runs down-right, j down-left).
        // On staggered maps a tile's grid position is i = x + (y + y%2)/2, j = (y - y%2)/2 - x, so the same rules apply there.
        static int Parity(int y){return y&1;}
        public static void GridOf(int x,int y,out int i,out int j){int p=Parity(y);i=x+(y+p)/2;j=(y-p)/2-x;}
        public static void TileOf(int i,int j,out int x,out int y){y=i+j;x=(i-j-Parity(y))/2;}
        // The tile reached from (x,y) by di, dj grid steps (the 3 x 3 neighbours are di, dj in -1..1). Wraps like At.
        public Tile Step(int x,int y,int di,int dj){if(mapShape!=2)return At(x+di,y+dj);int i,j;GridOf(x,y,out i,out j);int nx,ny;TileOf(i+di,j+dj,out nx,out ny);return At(nx,ny);}
        // Signed shortest horizontal step from x to a (wraps on spheres).
        public int DeltaX(int x,int a){int d=a-x;if(mapShape==1||mapShape==2){int w=Width;d%=w;if(d>w/2)d-=w;else if(d<-w/2)d+=w;}return d;}
        // Number of 8-direction moves between two tiles. On a sphere the route over either pole is also considered.
        public int Distance(int x,int y,int a,int b){
            if(mapShape==2){
                // Grid distance; shifting a by a whole width moves its grid position by (+W, -W), so try the nearest copies.
                int i0,j0,i1,j1;GridOf(x,y,out i0,out j0);GridOf(a,b,out i1,out j1);int di=i1-i0,dj=j1-j0,w=Width;
                int k0=(int)Math.Round((dj-di)/(2.0*w)),best2=int.MaxValue;
                for(int k=k0-1;k<=k0+1;k++)best2=Math.Min(best2,Math.Max(Math.Abs(di+k*w),Math.Abs(dj-k*w)));
                return best2;
            }
            int best=Math.Max(Math.Abs(DeltaX(x,a)),Math.Abs(y-b));
            if(mapShape==1){
                int px=Math.Abs(DeltaX(x,a+Width/2));
                best=Math.Min(best,Math.Max(px,y+b+1));
                best=Math.Min(best,Math.Max(px,2*Height-1-y-b));
            }
            return best;
        }
        public static readonly string[] RivalNames={"Amber League","Azure Dominion","Crimson Throne","Jade Concord","Ivory Republic","Obsidian Pact","Golden Horde","Silver Compact","Emerald Court","Violet Syndicate","Saffron Empire","Cobalt Union","Ashen Realm","Verdigris Hold","Scarlet Marches"};
        public static readonly int[] MaxCivsBySize={4,6,8,12,14,16};
        public string NationName(int owner){if(owner==0)return civilisationName;if(owner>0&&owner<civCount&&nations!=null&&owner<nations.Length&&nations[owner]!=null&&!string.IsNullOrEmpty(nations[owner].name))return nations[owner].name;return owner>0&&owner<=RivalNames.Length?RivalNames[owner-1]:"Rival "+owner;}
        // Resize every per-civilisation array. Existing entries are kept; new ones start unmet and at peace.
        public void SetCivCount(int n)
        {
            n=Math.Max(2,Math.Min(16,n));civCount=n;
            var oldNations=nations;nations=new Nation[n];for(int i=0;i<n;i++)nations[i]=oldNations!=null&&i<oldNations.Length&&oldNations[i]!=null?oldNations[i]:new Nation();
            Array.Resize(ref war,n);Array.Resize(ref openBorders,n);Array.Resize(ref warWeariness,n);
            var oldMet=met;met=new bool[n];met[0]=true;if(oldMet!=null)for(int i=1;i<n&&i<oldMet.Length;i++)met[i]=oldMet[i];
            diplomacy=NewDiplomacy(n);networks=null;
        }
        // Well-spread start positions for flat maps (farthest-point sampling over a coarse grid, deterministic).
        static void FlatStarts(int n,int mw,int mh,int[] sx,int[] sy)
        {
            var cand=new List<int[]>();
            for(int y=4;y<=mh-5;y+=3)for(int x=4;x<=mw-5;x+=3)cand.Add(new[]{x,y});
            int first=0,bestD=int.MaxValue;
            for(int i=0;i<cand.Count;i++){int d=Math.Abs(cand[i][0]-mw/5)+Math.Abs(cand[i][1]-mh/2);if(d<bestD){bestD=d;first=i;}}
            var chosen=new List<int[]>{cand[first]};
            while(chosen.Count<n)
            {
                int best=-1,bestMin=-1;
                for(int i=0;i<cand.Count;i++)
                {
                    int m=int.MaxValue;foreach(var c in chosen)m=Math.Min(m,Math.Max(Math.Abs(cand[i][0]-c[0]),Math.Abs(cand[i][1]-c[1])));
                    if(m>bestMin){bestMin=m;best=i;}
                }
                chosen.Add(cand[best]);
            }
            for(int i=0;i<n;i++){sx[i]=chosen[i][0];sy[i]=chosen[i][1];}
        }
        public static string CleanName(string value){var s=new string((value??"").Where(c=>!char.IsControl(c)&&c!='<'&&c!='>').ToArray()).Trim();return s.Length==0?"Verdant Union":s.Substring(0,Math.Min(32,s.Length));}
        public void Note(string s){log.Insert(0,"Turn "+turn+": "+s);if(log.Count>150)log.RemoveAt(log.Count-1);}
        public Unit Add(int owner,int type,int x,int y){var u=new Unit{id=nextId++,owner=owner,type=type,x=x,y=y,moves=Moves[type]};units.Add(u);return u;}
        // civChoice: -2 keeps the old single-trait setup (trait), 0-11 picks a premade civilisation, -1 a custom one (name, leader,
        // portrait and traitA/traitB). Rivals then take premade civilisations the player did not choose.
        public static World New(int seed,string civilisationName="Verdant Union",int mapType=0,int speed=1,int difficulty=1,int trait=0,int mapSize=1,int mapShape=0,int civs=3,int civChoice=-2,string leader="",int portrait=0,int traitA=-1,int traitB=-1)
        {
            // New globe games use staggered rows (shape 2): the same tile count as the diagonal globe, W columns by 2W rows,
            // which shows as a 2:1 picture because rows are half a tile tall.
            mapSize=Math.Max(0,Math.Min(MapWidths.Length-1,mapSize));mapShape=mapShape==1||mapShape==2?2:0;int mw=mapShape==2?SphereHeights[mapSize]:MapWidths[mapSize],mh=mapShape==2?SphereWidths[mapSize]:MapHeights[mapSize];
            civs=Math.Max(2,Math.Min(MaxCivsBySize[mapSize],civs));
            var w=new World{seed=seed,civilisationName=CleanName(civilisationName),mapType=mapType,speed=speed,difficulty=difficulty,trait=trait,mapSizeIndex=mapSize,mapShape=mapShape,mapWidth=mw,mapHeight=mh,tiles=new Tile[mw*mh]};
            w.SetCivCount(civs);
            foreach(var nation in w.nations)nation.known=new bool[mw*mh];
            w.nations[0].trait=trait;w.nations[0].traits=TraitsFromLegacy(trait);if(civChoice>=-1)w.SetPlayerCivilisation(civChoice,civilisationName,leader,portrait,traitA,traitB);else w.AssignRivalCivilisations();w.tech=w.nations[0].tech;var r=new Random(seed);
            int[] sx=new int[civs],sy=new int[civs];
            if(civs==3){sx=new[]{9,34,31};sy=new[]{12,9,23};}
            if(mapShape==2){
                for(int y=0;y<mh;y++)for(int x=0;x<mw;x++)w.tiles[y*mw+x]=new Tile{x=x,y=y,terrain=4,owner=-1};
                w.GenerateSphere(sx,sy);
            }else{
            for(int y=0;y<mh;y++)for(int x=0;x<mw;x++){
                int n=r.Next(100),t=n<45?0:n<68?1:n<85?2:n<94?3:5;
                if(x==0||y==0||x==mw-1||y==mh-1||(mapType==0&&Math.Sin(x*.2)+Math.Cos(y*.29)*.7< -1.2))t=4;
                // Flat-map land types: archipelago, small continents and lakes use smooth noise for their seas.
                if(mapType==2&&FlatNoise(seed,x/4.5,y/4.5)<.56)t=4;
                if(mapType==3&&FlatNoise(seed,x/9.0,y/9.0)<.47)t=4;
                if(mapType==4&&FlatNoise(seed+5,x/5.0,y/5.0)<.33)t=6;
                w.tiles[y*mw+x]=new Tile{x=x,y=y,terrain=t,owner=-1};
            }
            w.GenerateBiomes();
            // Start positions keep the same proportions on every map size; Small is unchanged.
            if(civs!=3)FlatStarts(civs,mw,mh,sx,sy);
            else if(mw!=DefaultWidth||mh!=DefaultHeight)for(int i=0;i<3;i++){sx[i]=(int)Math.Round(sx[i]*mw/(double)DefaultWidth);sy[i]=(int)Math.Round(sy[i]*mh/(double)DefaultHeight);}
            // Land connections ensure conquest is possible before naval implementation.
            for(int i=0;i<civs-1;i++){int x=sx[i],y=sy[i];while(x!=sx[i+1]||y!=sy[i+1]){w.At(x,y).terrain=0;x+=Math.Sign(sx[i+1]-x);y+=Math.Sign(sy[i+1]-y);}}
            }
            for(int i=0;i<civs;i++){
                // Sphere starts: a round clearing that only softens harsh ground, so the capital does not sit in a square of plain grass.
            for(int ddy=-3;ddy<=3;ddy++)for(int ddx=-3;ddx<=3;ddx++){var clear=w.Step(sx[i],sy[i],ddx,ddy);if(clear==null)continue;
                if(mapShape==0)clear.terrain=0;else if(!Barren(clear.terrain)&&ddx*ddx+ddy*ddy<=6&&(ddx*ddx+ddy*ddy<=1||Array.IndexOf(new[]{3,8,9,10,11,18,19},clear.terrain)>=0))clear.terrain=0;}
                if(mapShape==2)foreach(var dx in new[]{-1,0,1,2}){var open=w.Step(sx[i],sy[i],dx,0);if(Barren(open.terrain))open.terrain=0;}
                if(mapShape==2){var below=w.Step(sx[i],sy[i],0,1);if(Barren(below.terrain))below.terrain=0;}
                w.Add(i,0,sx[i],sy[i]);{var east=w.Step(sx[i],sy[i],1,0);var south=w.Step(sx[i],sy[i],0,1);w.Add(i,2,east.x,east.y);w.Add(i,1,south.x,south.y);}
                if(i>0)w.Found(w.units.First(u=>u.owner==i&&u.type==0));
            }
            w.PlaceLandResources(r);
            for(int i=0;i<civs;i++){w.Step(sx[i],sy[i],-1,0).resource=1;w.Step(sx[i],sy[i],2,0).resource=4;var hill=w.Step(sx[i],sy[i],0,-2);hill.terrain=2;hill.resource=2;}
            w.HarmoniseBiomes();w.SeedSeaResources();
            w.EnsureFlags();w.SeedVillages();w.SeedRuins();w.RebuildTerritory();w.Reveal();w.Note("Found a city, choose production and research. F1 opens the guide.");return w;
        }
        [NonSerialized] bool[] visibleMask; [NonSerialized] bool visibleMaskOn;
        // The screen asks "is this tile visible to the player?" thousands of times per frame. For the length of one
        // draw the answer is read from a mask built once from the player's units and cities. Call End before changing the world.
        public void BeginVisibilityCache()
        {
            int n=tiles.Length;
            if(visibleMask==null||visibleMask.Length!=n)visibleMask=new bool[n];else Array.Clear(visibleMask,0,n);
            if(HasTech(0,"satellites")){for(int i=0;i<n;i++)visibleMask[i]=true;}
            else{
                foreach(var u in units)if(u.owner==0)SeeAround(u,t=>visibleMask[t.y*Width+t.x]=true);
                foreach(var c in cities)if(c.owner==0)MarkVisible(c.x,c.y,3);
                for(int i=0;i<n;i++)if(Developed(0,tiles[i]))visibleMask[i]=true;
            }
            visibleMaskOn=true;
        }
        public void EndVisibilityCache(){visibleMaskOn=false;}
        void MarkVisible(int cx,int cy,int radius){for(int dy=-radius;dy<=radius;dy++)for(int dx=-radius;dx<=radius;dx++){var t=Step(cx,cy,dx,dy);if(t!=null)visibleMask[t.y*Width+t.x]=true;}}
        public bool VisibleTo(int owner,int x,int y){
            if(visibleMaskOn&&owner==0){var cell=At(x,y);if(cell!=null)return visibleMask[cell.y*Width+cell.x];}
            return HasTech(owner,"satellites")||units.Any(u=>u.owner==owner&&Distance(x,y,u.x,u.y)<=SightRange(u)&&CanSee(u,At(x,y)))||cities.Any(c=>c.owner==owner&&Distance(x,y,c.x,c.y)<=3)||Developed(owner,At(x,y));
        }
        // Roads and improvements inside a civilisation's own territory are always in view, never under the clouds (owner request).
        public bool Developed(int owner,Tile t){return t!=null&&t.owner==owner&&(t.road||t.farm||t.mine||t.fort);}
        public bool Visible(int x,int y){return VisibleTo(0,x,y);}
        void LearnAround(int owner,int cx,int cy,int radius,HashSet<int> done){for(int dy=-radius;dy<=radius;dy++)for(int dx=-radius;dx<=radius;dx++){var t=Step(cx,cy,dx,dy);if(t==null)continue;if(done.Add(t.y*Width+t.x))LearnTile(owner,t);}}
        void LearnTile(int owner,Tile t){nations[owner].known[t.y*Width+t.x]=true;if(owner==0){t.seen=true;t.rememberedTerrain=t.terrain;t.rememberedResource=t.resource;t.rememberedOwner=t.owner;t.rememberedRoad=t.road;t.rememberedFarm=t.farm;t.rememberedMine=t.mine;t.rememberedFort=t.fort;var city=cities.Find(c=>c.x==t.x&&c.y==t.y);t.rememberedCity=city==null?"":city.name;}}
        // Fog is updated only around units and cities, so cost does not grow with map size.
        public void Reveal(){for(int owner=0;owner<civCount;owner++){
                if(HasTech(owner,"satellites")){foreach(var t in tiles)LearnTile(owner,t);continue;}
                var done=new HashSet<int>();
                foreach(var u in units)if(u.owner==owner)SeeAround(u,t=>{if(done.Add(t.y*Width+t.x))LearnTile(owner,t);});
                foreach(var c in cities)if(c.owner==owner)LearnAround(owner,c.x,c.y,3,done);
                foreach(var t in tiles)if(Developed(owner,t)&&done.Add(t.y*Width+t.x))LearnTile(owner,t);
            }
            for(int i=1;i<civCount;i++)if(!met[i]&&(units.Any(u=>u.owner==i&&Visible(u.x,u.y))||cities.Any(c=>c.owner==i&&Visible(c.x,c.y)))){met[i]=true;Note("First contact: "+NationName(i)+".");if(newContacts==null)newContacts=new List<int>();newContacts.Add(i);}}
        public bool AtWar(int a,int b){return a>=0&&b>=0&&a<civCount&&b<civCount&&a!=b&&(a!=0&&b!=0?RivalWar(a,b):war[a==0?b:a]);}
        bool RivalWar(int a,int b){var p=Relation(a,b);return p!=null&&p.atWar;}
        public bool CanEnter(int owner,Tile t){return t.owner<0||t.owner==owner||AtWar(owner,t.owner)||HasAccess(owner,t.owner);}
        public void RebuildTerritory()
        {
            // Each city claims the tiles inside its own radius; the nearest city wins, then the lower id. A tile already held by a
            // civilisation that still has a city in reach stays with it (border influence, SpreadInfluence, moves it slowly);
            // tiles next to a city always belong to that city's owner.
            int n=tiles.Length;var best=new int[n];var bestId=new int[n];var bestOwner=new int[n];var previous=new int[n];var kept=new bool[n];
            for(int k=0;k<n;k++){previous[k]=tiles[k].owner;tiles[k].owner=-1;best[k]=int.MaxValue;bestOwner[k]=-1;}
            foreach(var c in cities)
            {
                int radius=TerritoryRadius(c);
                for(int dy=-radius;dy<=radius;dy++)for(int dx=-radius;dx<=radius;dx++)
                {
                    var t=Step(c.x,c.y,dx,dy);if(t==null)continue;
                    int d=Distance(t.x,t.y,c.x,c.y);if(d>radius)continue;
                    int index=t.y*Width+t.x;if(c.owner==previous[index])kept[index]=true;
                    if(d<best[index]||(d==best[index]&&c.id<bestId[index])){best[index]=d;bestId[index]=c.id;bestOwner[index]=c.owner;}
                }
            }
            for(int k=0;k<n;k++)if(bestOwner[k]>=0)tiles[k].owner=best[k]>1&&kept[k]?previous[k]:bestOwner[k];
            networks=null;
        }
        HashSet<int> Network(int owner){
            if(networks==null)networks=new HashSet<int>[civCount];if(networks[owner]!=null)return networks[owner];
            var set=new HashSet<int>();var capital=cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).FirstOrDefault();if(capital==null){networks[owner]=set;return set;}
            var q=new Queue<Tile>();q.Enqueue(At(capital.x,capital.y));set.Add(capital.y*Width+capital.x);
            while(q.Count>0){var t=q.Dequeue();for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n==null||!n.road||n.owner!=owner||units.Any(u=>u.x==n.x&&u.y==n.y&&AtWar(owner,u.owner)))continue;if(set.Add(n.y*Width+n.x))q.Enqueue(n);}}
            networks[owner]=set;return set;
        }
        public bool HasDomesticResource(int owner,int resource){if(resource==0)return true;foreach(int index in Network(owner)){var t=tiles[index];if(t.owner==owner&&t.resource==resource&&(resource==2||resource==3?t.mine:t.farm))return true;}return false;}
        public bool HasResource(int owner,int resource){return HasDomesticResource(owner,resource)||ImportedResource(owner,resource);}
        public bool Connected(City c){return Network(c.owner).Contains(c.y*Width+c.x);}
        public Tile KnownTile(Tile t){if(t==null||!t.seen||Visible(t.x,t.y))return t;return new Tile{x=t.x,y=t.y,seen=true,terrain=t.rememberedTerrain,resource=t.rememberedResource,owner=t.rememberedOwner,road=t.rememberedRoad,farm=t.rememberedFarm,mine=t.rememberedMine,fort=t.rememberedFort,rememberedCity=t.rememberedCity};}
        // Allocation-free variants for the draw loop and pathfinding.
        public void CopyKnown(Tile t,Tile dest){dest.x=t.x;dest.y=t.y;dest.seen=true;dest.terrain=t.rememberedTerrain;dest.resource=t.rememberedResource;dest.owner=t.rememberedOwner;dest.road=t.rememberedRoad;dest.farm=t.rememberedFarm;dest.mine=t.rememberedMine;dest.fort=t.rememberedFort;dest.cleared=t.cleared;dest.village=false;dest.rememberedCity=t.rememberedCity;}
        public Tile KnownTileInto(Tile t,Tile dest){if(t==null||!t.seen||Visible(t.x,t.y))return t;CopyKnown(t,dest);return dest;}
        public int KnownTerrain(Tile t){return !t.seen||Visible(t.x,t.y)?t.terrain:t.rememberedTerrain;}
        public int KnownOwner(Tile t){return t.seen&&!Visible(t.x,t.y)?t.rememberedOwner:t.owner;}
        public bool RoadConnects(Tile t,int dx,int dy){var n=KnownTile(Step(t.x,t.y,dx,dy));return t.road&&n!=null&&n.seen&&n.road;}
        // Third yield is Trade: water 2, rivers 1, rocky coast and mangrove 1; roads add 1 on open flat land (grassland, desert, savanna, steppe); gold deposits add 2.
        public static readonly int[] TerrainTrade={0,0,0,0,2,0,2,1,0,0,0,0,2,0,0,0,0,0,1,1,0};
        public static bool RoadTrade(int terrain){return terrain==0||terrain==3||terrain==16||terrain==20;}
        public int[] TileYield(Tile t){int[] f={3,2,1,1,1,0,2,3,1,2,0,3,2,1,0,0,2,0,1,2,2},p={1,2,3,1,0,2,0,1,1,1,1,2,0,2,0,1,1,3,1,2,1};int res=t.resource>=0&&t.resource<ResourceFood.Length?t.resource:0;return new[]{f[t.terrain]+(t.farm?2:0)+ResourceFood[res],p[t.terrain]+(t.mine?2:0)+ResourceShields[res],TerrainTrade[t.terrain]+(t.road&&RoadTrade(t.terrain)?1:0)+ResourceTrade[res]};}
        // 60% science at most, or 80% once a civilisation adopts the Academies edict (design). MaxScienceRate is the absolute ceiling.
        public const int MaxScienceRate=80;
        public int ScienceCap(int owner){return HasEdict(owner,1,2)?80:60;}
        public static int TaxLevel(int scienceRate){return scienceRate>=60?0:scienceRate<=30?2:1;}
        public bool SetScienceRate(int owner,int rate){if(owner<0||owner>=civCount)return false;rate=Math.Max(0,Math.Min(ScienceCap(owner),rate/10*10));var n=nations[owner];n.scienceRate=rate;n.tax=TaxLevel(rate);return true;}
        // City Trade: centre plus worked tiles. (The +2 Trade per citizen bridging rule from Stage 3 was retired with the keystone
        // tree in Stage 5; with it, rivals finished the tree by turn 95.)
        public int Trade(City c){int trade=CityCentreYield(c)[2];foreach(var t in Worked(c))trade+=EffectiveTileYield(c,t)[2]+MinorTileBonus(c.owner,t,2);return trade;}
        // AI moves between 30% science in deficit and the 60% maximum once comfortably in surplus, so research never stops entirely.
        void AdjustScienceRate(int owner){var n=nations[owner];int net=Budget(owner)[2];if(net<0&&n.scienceRate>30)SetScienceRate(owner,n.scienceRate-10);else if(net>=cities.Count(c=>c.owner==owner)+2&&n.scienceRate<ScienceCap(owner))SetScienceRate(owner,n.scienceRate+10);}
        public bool HasBuilding(City c,int b){return b==7?c.granary:b==8?c.library:b==9?c.walls:c.buildings.Contains(b);}
        // Happiness model (CORE-DESIGN-16BIT.md): working citizens are content up to the city's content slots; the rest are unhappy.
        // Slots: 4 base, plus happiness buildings and the other existing bonuses and penalties, plus 2 per entertainer (in ExtraHappiness);
        // low tax (60% science) adds 1 slot and high tax (30% or less) removes 1.
        public const int BaseContent=4;
        public int ContentSlots(City c){var n=nations[c.owner];return BaseContent+EdictContent(c.owner)+MinorContent(c.owner)+ReligionContent(c)+(HasTrait(c.owner,TraitSpiritual)?1:0)+ExtraHappiness(c)-WarUnhappiness(c.owner)+(1-n.tax)+(HasBuilding(c,33)?2:0)+(HasTech(c.owner,"longevity")?2:0)+(HasResource(c.owner,5)?2:0)-(c.occupation>0?3:0)-(n.arrears>0?1:0);}
        public int Workers(City c){return Math.Max(0,c.pop-c.scientists-c.merchants-c.entertainers);}
        public int ContentCitizens(City c){return Math.Max(0,Math.Min(Workers(c),ContentSlots(c)));}
        public int UnhappyCitizens(City c){return Workers(c)-ContentCitizens(c);}
        // Net mood: content minus unhappy citizens. Below zero, unhappy outnumber content and the city is in disorder.
        public int Happiness(City c){return ContentCitizens(c)-UnhappyCitizens(c);}
        public bool Disorder(City c){return Happiness(c)<0;}
        public int[] Yield(City c){var centre=CityCentreYield(c);int f=centre[0],p=centre[1],trade=centre[2];foreach(var t in Worked(c)){var v=EffectiveTileYield(c,t);f+=v[0]+MinorTileBonus(c.owner,t,0);p+=v[1]+MinorTileBonus(c.owner,t,1);trade+=v[2]+MinorTileBonus(c.owner,t,2);}if(HasBuilding(c,30))p+=3;
            // Trade x science rate / 100 (rounded down) becomes science; the rest becomes gold, so nothing is lost.
            int s=trade*nations[c.owner].scienceRate/100,g=trade-s;s+=(c.library?2:0)+(HasTech(c.owner,"electricity")?3:0);f+=EdictFood(c.owner);p+=EdictProduction(c.owner);s+=EdictScience(c.owner);g+=EdictGold(c.owner);if(c.focus==3){s+=2;p=Math.Max(1,p-2);}ApplyResearchYields(c,ref f,ref p,ref s);ApplyEmpireYields(c,ref f,ref p,ref s,ref g);ApplyReligionYields(c,ref f,ref p,ref s,ref g);if(HasTrait(c.owner,TraitIndustrious))p++;if(HasTrait(c.owner,TraitScholarly))s++;if(HasTrait(c.owner,TraitAgrarian))f++;if(HasTrait(c.owner,TraitMercantile))g+=2;{int ms,mg,mp;MinorPercent(c.owner,out ms,out mg,out mp);s+=s*ms/100;g+=g*mg/100;p+=p*mp/100;}if(Disorder(c)){p=0;g=0;}if(c.occupation>0){p=Math.Max(1,p/2);s=Math.Max(1,s/2);}return new[]{f-(HasTech(c.owner,"uploading")?0:c.pop*2),p,s,g};}
        public int[] Budget(int owner){int income=0,expense=0;var own=cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).ToList();foreach(var c in own){income+=Yield(c)[3]+PortIncome(c)+(Disorder(c)?0:(HasBuilding(c,31)?2:0)+(Connected(c)&&c!=own[0]?3:0));int distance=Distance(c.x,c.y,own[0].x,own[0].y)/10;expense+=Math.Max(0,1+c.pop/4+distance-(HasBuilding(c,34)?2:0))+c.buildings.Sum(BuildingUpkeep);}
            int military=units.Where(u=>u.owner==owner).Sum(u=>Upkeep[u.type]);expense+=Math.Max(0,military-FreeUnitsPerCity*own.Count-GovernmentSupport(owner));expense+=own.Count*nations[owner].luxury;income+=TradeIncome(owner)+DiplomaticIncome(owner)+VillageTribute(owner)+ReligionTithe(owner);expense+=DiplomaticExpense(owner);return new[]{income,expense,income-expense};}
        // Smooth value noise for flat-map land types (0..1).
        public static double FlatNoise(int s,double x,double y){return Fbm(s,x,y,.5,3);}
        // Sea resources by zone (marine biology sources: kelp, shellfish and seals on cool coasts; reefs, pearls and sponges in
        // warm shallows; tuna and whales in open water; fish everywhere, including lakes; oil on the shelf).
        public void SeedSeaResources()
        {
            var r=new Random(seed^0x5ea);
            foreach(var t in tiles)
            {
                if(!PixelArt.IsWater(t.terrain)||t.resource!=0)continue;
                double lat=Math.Abs(t.y-(Height-1)/2.0)/(Height/2.0);int zone=lat<.35?0:lat<.68?1:2;
                bool coast=false;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n!=null&&!PixelArt.IsWater(n.terrain))coast=true;}
                int[] pick;
                if(t.terrain==6){if(r.Next(5)==0)t.resource=6;continue;}
                if(t.terrain==12){if(r.Next(3)==0)t.resource=r.Next(2)==0?8:14;continue;}
                if(coast){if(r.Next(7)!=0)continue;pick=zone==0?new[]{6,8,11,14}:zone==1?new[]{6,9,10,11}:new[]{6,10,12};}
                else{if(r.Next(16)!=0)continue;pick=zone==0?new[]{13,15}:zone==1?new[]{7,13,15}:new[]{7,15};}
                t.resource=pick[r.Next(pick.Length)];
            }
        }
        public City Found(Unit u,string name=null){if(FoundReason(u)!="")return null;if(name==null||CityNameProblem(name)!="")name=NewCityName(u.owner);else name=CleanName(name);var cnew=new City{id=nextId++,owner=u.owner,x=u.x,y=u.y,name=name};cities.Add(cnew);if(nations[u.owner].capital<0)nations[u.owner].capital=cnew.id;units.Remove(u);At(u.x,u.y).road=true;RebuildTerritory();if(u.owner==0)Note(cnew.name+" founded.");Reveal();DiplomacyOnFound(cnew);return cnew;}
        public static int UnitBuild(int type){return type<7?type:100+type;}
        public static int BuildUnit(int build){return build>=0&&build<7?build:build>=107&&build<100+UnitNames.Length?build-100:-1;}
        public static string BuildName(int b){if(b<0)return "Stockpile";int u=BuildUnit(b);if(u>=0)return UnitNames[u];int i=Array.IndexOf(BuildingIds,b);return i<0?"Unknown":BuildingNames[i];}
        // Wonders cost 100 + 50 x era, with the 14 catalogue eras folded onto the design's 6 until the technology compression in Stage 5.
        public static int WonderEra(int b){int i=Array.IndexOf(BuildingIds,b);return i<0?1:1+TechnologyCatalog.Eras[BuildingTech[i]]*6/14;}
        public static int BaseCost(int b){int u=BuildUnit(b);int i=Array.IndexOf(BuildingIds,b);return u>=0?UnitCosts[u]:i<0?0:IsWonder(b)?100+50*WonderEra(b):BuildingCosts[i];}
        public static int BuildingUpkeep(int b){return IsWonder(b)||IsProject(b)?0:BaseCost(b)>40?1:0;}
        public int Cost(int b){return BaseCost(b)*(speed==0?1:speed==2?3:2)/2;}
        public string BuildReason(City c,int b){if(b==-1)return "";int u=BuildUnit(b);if(u>=0){if(u==0&&c.pop<2)return "Requires city size 2 (a Settler costs 1 population)";if(UnitTech[u]>=0&&!nations[c.owner].tech[UnitTech[u]])return "Requires "+TechNames[UnitTech[u]];if(!HasResource(c.owner,UnitResource[u]))return "Requires improved, road-connected "+ResourceNames[UnitResource[u]];return IsNaval(u)?NavalReason(c,u):"";}int i=Array.IndexOf(BuildingIds,b);if(i<0)return "Unknown project";if(IsWonder(b)&&cities.Any(a=>HasBuilding(a,b)))return "Wonder already completed";if(HasBuilding(c,b))return "Already built";if(b==43&&!AdjacentWater(c))return "Requires a coastal city";if(b==PortBuilding&&PortSite(c)==null)return "Requires the city's borders to reach the sea";if(IsProject(b)){string why=ProjectReason(c,b);if(why!="")return why;}return !nations[c.owner].tech[BuildingTech[i]]?"Requires "+TechNames[BuildingTech[i]]:"";}
        // True when the city's owner knows the technology for this unit or building, it is not already built here, and (for a
        // wonder) nobody has finished it. Other limits such as size or resources still show as reasons in the list.
        public bool TechAllows(City c,int b){int u=BuildUnit(b);if(u>=0)return UnitTech[u]<0||nations[c.owner].tech[UnitTech[u]];int i=Array.IndexOf(BuildingIds,b);if(i<0||HasBuilding(c,b))return false;if(IsWonder(b)&&cities.Any(a=>HasBuilding(a,b)))return false;if(IsProject(b)&&ProjectStageOf(b)<=nations[c.owner].projectStage)return false;return nations[c.owner].tech[BuildingTech[i]];}
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
        // Base cost scaled by game speed; a fifth cheaper (rounded down) once two civilisations this one has met know it (design).
        // Stage 8 pacing: the design's cost (20 x e^2 + 5 x e x i) scaled per era, in percent, so a standard game meets the
        // design's targets (eras complete around turns 40, 80, 125, 170 and 210; Transcendence around 230). Measured with
        // headless rival games; see IMPLEMENTATION-STATUS.md.
        public static readonly int[] ResearchPace={150,190,280,400,420,420};
        public int ResearchCost(int t,int owner=0){int cost=TechCosts[t]*ResearchPace[TechnologyCatalog.Eras[t]]/100*(speed==0?1:speed==2?3:2)/2;if(owner>=0&&owner<civCount&&Enumerable.Range(0,civCount).Count(o=>o!=owner&&HasContact(owner,o)&&nations[o].tech[t])>=2)cost-=cost/5;return cost;}
        public void SelectResearch(int t){if(!AvailableTech().Contains(t))return;nations[0].progress[research<0?0:research]=science;research=t;nations[0].research=t;science=nations[0].progress[t];}
        public void CancelOrder(Unit u){if(u==null)return;u.exploring=false;u.sentry=false;u.hasOrder=false;u.job=0;u.jobTurns=0;}
        public int[] PeaceOwners(Unit u,int x,int y){if(u==null||!VisibleTo(u.owner,x,y))return new int[0];var owners=units.Where(a=>a.x==x&&a.y==y).Select(a=>a.owner).Concat(cities.Where(a=>a.x==x&&a.y==y).Select(a=>a.owner));var t=At(x,y);if(t!=null&&t.owner>=0&&!CanEnter(u.owner,t))owners=owners.Concat(new[]{t.owner});return owners.Where(a=>a!=u.owner&&!AtWar(u.owner,a)).Distinct().ToArray();}
        // Horsemen ignore the forest slowdown (design); Scouts already move freely.
        // Ice sheets and glaciers can be crossed, but each step takes a unit's whole move.
        public int StepCost(Unit u,int x,int y,Tile t){if(IsNaval(u.type))return 2;if(t.terrain==14||t.terrain==15)return Math.Max(4,Moves[u.type]);var from=At(x,y);
            // A road through woodland is only a trail: combat units get no road speed on it (owner request).
            bool roadStep=from.road&&t.road&&!(Power[u.type]>0&&(Forested(from.terrain)||Forested(t.terrain)));
            return roadStep?1:u.type==3?2:u.type==8&&Wooded(t.terrain)?2:(RoughTerrain(t.terrain)?3:2);}
        bool Capacity(Unit u,int x,int y){if(IsNaval(u.type))return units.Count(a=>a.id!=u.id&&a.owner==u.owner&&IsNaval(a.type)&&a.x==x&&a.y==y)<3;var at=At(x,y);if(at!=null&&IsWater(at.terrain))return true;if(u.type==1)return true;return units.Count(a=>a.type!=1&&!IsNaval(a.type)&&a.id!=u.id&&a.owner==u.owner&&a.x==x&&a.y==y&&(Power[a.type]>0)==(Power[u.type]>0))<(Power[u.type]>0?3:1);}
        [NonSerialized] int[] routeDist,routePrev,routeSeq; [NonSerialized] Tile routeScratch;
        // Regions a land unit can walk between (water, reefs, lakes and peaks separate them; polar ice joins them, as it is
        // passable). Rebuilt only when the terrain fingerprint changes, which in play means once per map.
        [NonSerialized] int[] walkRegion;[NonSerialized] long walkPrint;
        int WalkRegion(int cell)
        {
            long print=tiles.Length;for(int i=0;i<tiles.Length;i++)print=print*31+(IsImpassable(tiles[i].terrain)?1:2);
            if(walkRegion==null||walkRegion.Length!=tiles.Length||print!=walkPrint)
            {
                walkPrint=print;
                walkRegion=new int[tiles.Length];int next=1;var queue=new Queue<int>();
                for(int i=0;i<tiles.Length;i++)
                {
                    if(walkRegion[i]!=0||IsImpassable(tiles[i].terrain))continue;walkRegion[i]=next;queue.Enqueue(i);
                    while(queue.Count>0){int k=queue.Dequeue();var t=tiles[k];for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var nb=Step(t.x,t.y,dx,dy);if(nb==null)continue;int j=nb.y*Width+nb.x;if(walkRegion[j]==0&&!IsImpassable(nb.terrain)){walkRegion[j]=next;queue.Enqueue(j);}}}
                    next++;
                }
            }
            return walkRegion[cell];
        }
        static void HeapPush(List<long> h,long v){h.Add(v);int i=h.Count-1;while(i>0){int p=(i-1)/2;if(h[p]<=h[i])break;long tmp=h[p];h[p]=h[i];h[i]=tmp;i=p;}}
        static long HeapPop(List<long> h){long top=h[0];int last=h.Count-1;h[0]=h[last];h.RemoveAt(last);int i=0,n=h.Count;while(true){int l=2*i+1,r=l+1,m=i;if(l<n&&h[l]<h[m])m=l;if(r<n&&h[r]<h[m])m=r;if(m==i)break;long tmp=h[m];h[m]=h[i];h[i]=tmp;i=m;}return top;}
        // Cheapest route by movement cost: A* with the tile distance times the cheapest possible step as its estimate (never
        // more than the real remaining cost, so the route found is still the cheapest), a heap and reusable arrays.
        // ignoreUnits: plan as if no units stood in the way (used to tell a jam that will clear from a route that does not exist).
        // Stage 8 speed: rival land units give up at once when the goal lies on land they cannot reach over land.
        public List<Tile> Route(Unit u,int x,int y,bool ignoreUnits=false)
        {
            var path=new List<Tile>();var goal=At(x,y);if(u==null||goal==null)return path;
            x=goal.x;y=goal.y;
            if(nations[u.owner].known[y*Width+x]&&!Passable(u,goal,u.owner==0?KnownTerrain(goal):goal.terrain))return path;
            int n=tiles.Length,start=u.y*Width+u.x,end=y*Width+x;
            if(u.owner!=0&&!IsNaval(u.type)&&!IsWater(goal.terrain)&&WalkRegion(start)!=WalkRegion(end))return path;
            if(routeDist==null||routeDist.Length!=n){routeDist=new int[n];routePrev=new int[n];routeSeq=new int[n];for(int k=0;k<n;k++){routeDist[k]=int.MaxValue;routePrev[k]=-1;routeSeq[k]=-1;}}
            if(routeScratch==null)routeScratch=new Tile();
            var dist=routeDist;var prev=routePrev;var seqOf=routeSeq;var touched=new List<int>();var seqNode=new List<int>();var heap=new List<long>();
            int minStep=IsNaval(u.type)?2:1;Func<int,int> estimate=i=>Distance(tiles[i].x,tiles[i].y,x,y)*minStep;
            // Rivals do not chase detours longer than three times the direct cost (plus a margin); their fallbacks take over.
            int limit=u.owner==0?int.MaxValue:estimate(start)*3*2+40;
            var foreign=new HashSet<int>();var mine=new HashSet<int>();foreach(var a in units){int cell=a.y*Width+a.x;if(a.owner!=u.owner)foreign.Add(cell);else if(a.id!=u.id)mine.Add(cell);}
            try
            {
                dist[start]=0;seqOf[start]=0;seqNode.Add(start);touched.Add(start);HeapPush(heap,(long)estimate(start)<<32);
                while(heap.Count>0)
                {
                    long key=HeapPop(heap);int current=seqNode[(int)(key&0xFFFFFFFFL)];
                    if((int)(key>>32)!=dist[current]+estimate(current))continue;
                    if(current==end)break;
                    var from=tiles[current];
                    for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                    {
                        var t=Step(from.x,from.y,dx,dy);if(t==null||(dx==0&&dy==0))continue;
                        int next=t.y*Width+t.x;bool known=nations[u.owner].known[next];
                        Tile seenTile=t;if(known&&u.owner==0)seenTile=KnownTileInto(t,routeScratch);
                        // The player's unexplored tiles are assumed passable land (their real terrain is not known yet); the route is
                        // planned again before every step, so it bends or stops once the unit sees the water.
                        if(known&&!Passable(u,t,seenTile.terrain))continue;if(!known&&IsNaval(u.type))continue;if((known||u.owner!=0)&&!IsNaval(u.type)&&IsWater(seenTile.terrain)&&next!=end)continue;
                        bool visible=VisibleTo(u.owner,t.x,t.y);
                        if(visible&&next!=end&&(!CanEnter(u.owner,t)||!ignoreUnits&&foreign.Contains(next)))continue;
                        if(!ignoreUnits&&mine.Contains(next)&&!Capacity(u,t.x,t.y))continue;
                        int cost=dist[current]+(known?StepCost(u,from.x,from.y,seenTile):2);
                        if(cost>=dist[next]||cost+estimate(next)>limit)continue;
                        dist[next]=cost;prev[next]=current;
                        if(seqOf[next]<0){seqOf[next]=seqNode.Count;seqNode.Add(next);touched.Add(next);}
                        HeapPush(heap,((long)(cost+estimate(next))<<32)|(long)seqOf[next]);
                    }
                }
                if(start!=end&&prev[end]>=0){for(int i=end;i!=start;i=prev[i])path.Add(tiles[i]);path.Reverse();}
            }
            finally{foreach(int index in touched){dist[index]=int.MaxValue;prev[index]=-1;seqOf[index]=-1;}}
            return path;
        }
        // Orders find their own way: a destination that cannot be entered (a peaceful border, another civilisation's unit, a full
        // stack) is replaced by the nearest reachable tile beside it; a path jammed only by units waits for the next turn; an
        // order ends only when there is no way there at all (owner request).
        public bool Order(Unit u,int x,int y){if(u==null||result!="")return false;CancelOrder(u);u.fortified=false;if(u.x==x&&u.y==y)return true;var goal=At(x,y);if(goal==null)return false;
            if(Route(u,x,y).Count==0&&Route(u,x,y,true).Count==0){var near=NearestReachableBeside(u,goal)??(IsNaval(u.type)||u.owner!=0?null:NearestLandToward(u,goal));if(near==null)return false;x=near.x;y=near.y;if(u.x==x&&u.y==y)return true;}
            u.destinationX=x;u.destinationY=y;u.hasOrder=true;ContinueOrder(u);return true;}
        // A land unit sent out over water (owner request): head for the reachable land tile closest to the goal, usually the
        // coast facing it. Only the unit's own map knowledge is used; unexplored tiles count as land until seen.
        Tile NearestLandToward(Unit u,Tile goal)
        {
            var known=nations[u.owner].known;int tries=0;
            foreach(var t in tiles.Where(t=>Distance(t.x,t.y,goal.x,goal.y)<=16&&(!known[t.y*Width+t.x]||!IsWater(u.owner==0&&!Visible(t.x,t.y)?KnownTerrain(t):t.terrain)))
                .OrderBy(t=>Distance(t.x,t.y,goal.x,goal.y)).ThenBy(t=>Distance(t.x,t.y,u.x,u.y)))
            {
                if(t.x==u.x&&t.y==u.y)return t;
                if(++tries>40)break;
                if(Route(u,t.x,t.y).Count>0)return t;
            }
            return null;
        }
        Tile NearestReachableBeside(Unit u,Tile goal)
        {
            // Tiles touching the destination first; a ring further out only when none of those can be reached.
            for(int ring=1;ring<=2;ring++)
            {
                Tile best=null;int bestLength=int.MaxValue;
                for(int dy=-ring;dy<=ring;dy++)for(int dx=-ring;dx<=ring;dx++)
                {
                    if(Math.Max(Math.Abs(dx),Math.Abs(dy))!=ring)continue;var t=Step(goal.x,goal.y,dx,dy);if(t==null)continue;
                    int terrain=u.owner==0&&!Visible(t.x,t.y)?KnownTerrain(t):t.terrain;
                    if(!Passable(u,t,terrain)||!CanEnter(u.owner,t)||units.Any(a=>a.owner!=u.owner&&a.x==t.x&&a.y==t.y)||!Capacity(u,t.x,t.y))continue;
                    if(t.x==u.x&&t.y==u.y)return t;
                    var path=Route(u,t.x,t.y);if(path.Count==0)continue;
                    if(path.Count<bestLength){bestLength=path.Count;best=t;}
                }
                if(best!=null)return best;
            }
            return null;
        }
        public void ContinueOrder(Unit u){
            int retargets=0;
            for(int guard=0;guard<40&&u.hasOrder&&units.Contains(u)&&result=="";guard++){
                if(u.x==u.destinationX&&u.y==u.destinationY){u.hasOrder=false;break;}
                if(u.moves==0)break;
                var dest=At(u.destinationX,u.destinationY);
                if(dest!=null&&VisibleTo(u.owner,dest.x,dest.y)&&(PeaceOwners(u,dest.x,dest.y).Length>0||!CanEnter(u.owner,dest)||!Capacity(u,dest.x,dest.y)))
                {
                    var near=NearestReachableBeside(u,dest);
                    if(near==null){u.hasOrder=false;if(u.owner==0)Note("Order stopped: nowhere reachable near the destination.");break;}
                    u.destinationX=near.x;u.destinationY=near.y;if(++retargets>1)break;continue;
                }
                var p=Route(u,u.destinationX,u.destinationY);
                if(p.Count==0){if(Route(u,u.destinationX,u.destinationY,true).Count>0)break;
                    // Land units heading into unexplored water keep going to the shore facing their goal, then stop there.
                    if(u.owner==0&&!IsNaval(u.type)&&dest!=null&&retargets==0){var shore=NearestLandToward(u,dest);if(shore!=null&&!(shore.x==u.destinationX&&shore.y==u.destinationY)&&!(shore.x==u.x&&shore.y==u.y)){u.destinationX=shore.x;u.destinationY=shore.y;retargets++;continue;}
                        if(shore!=null&&shore.x==u.x&&shore.y==u.y){u.hasOrder=false;if(u.owner==0)Note(UnitNames[u.type]+" reached the coast; the way on is water.");break;}}
                    u.hasOrder=false;if(u.owner==0)Note("Order stopped: no route to the destination.");break;}
                var t=p[0];
                if(u.moves<StepCost(u,u.x,u.y,t))break;
                if(!Move(u,t.x,t.y))break;
            }
        }
        // Attack or Defence plus rank; a defender adds terrain, fortified +1, fort +1, walls +2, and a Spearman +2 against mounted
        // units. Unpaid upkeep costs 1 point.
        public int Strength(Unit u,Unit enemy,bool defending){int s=(defending?Defence[u.type]:Attack[u.type])+u.promotion+MinorStrength(u,defending);if(defending){if(u.type==7&&enemy!=null&&Mounted(enemy.type))s+=2;var t=At(u.x,u.y);s+=TerrainDefence(t.terrain);if(t.fort)s+=1;if(u.fortified)s+=1;if(cities.Any(c=>c.x==u.x&&c.y==u.y&&c.walls))s+=2;if(HasEdict(u.owner,2,0))s+=1;if(HasTrait(u.owner,TraitStalwart)&&At(u.x,u.y).owner==u.owner)s+=1;}else if(enemy!=null&&Mounted(u.type)&&HasTrait(u.owner,TraitHorseLords))s+=1;if(nations[u.owner].arrears>0)s--;return Math.Max(0,s);}
        // The defender that fights: highest defence, then most health. Only it takes damage and only it can die.
        public Unit BestDefender(Unit attacker,int x,int y){return units.Where(e=>e.owner!=attacker.owner&&e.x==x&&e.y==y).OrderByDescending(e=>Strength(e,attacker,true)).ThenByDescending(e=>e.hp).ThenBy(e=>e.id).FirstOrDefault();}
        // Exact result of one exchange: damage to the defender and to the attacker (ranged attackers take none).
        public void Exchange(Unit attacker,Unit defender,out int dealt,out int taken){int a=Strength(attacker,defender,false),d=Strength(defender,attacker,true);dealt=Damage(a,d);taken=IsRanged(attacker.type)?0:Damage(d,a);}
        public bool Move(Unit u,int x,int y){if(result!=""||u==null||!units.Contains(u))return false;var t=At(x,y);if(t==null)return false;x=t.x;y=t.y;if(Distance(u.x,u.y,x,y)!=1)return false;if(t==null||!Passable(u,t,t.terrain)||!CanEnter(u.owner,t)||!Capacity(u,x,y))return false;var cargo=IsNaval(u.type)?Cargo(u):null;int cost=StepCost(u,u.x,u.y,t);if(u.moves<cost)return false;var enemies=units.Where(a=>a.owner!=u.owner&&a.x==x&&a.y==y).ToList();var c=cities.Find(a=>a.owner!=u.owner&&a.x==x&&a.y==y);
            if(enemies.Count>0||c!=null){if(Power[u.type]==0||enemies.Any(a=>!AtWar(u.owner,a.owner))||(c!=null&&!AtWar(u.owner,c.owner)))return false;if(enemies.Count>0){var e=BestDefender(u,x,y);int dealt,taken;Exchange(u,e,out dealt,out taken);e.hp-=dealt;u.hp-=taken;u.moves=0;if(e.hp<=0&&u.hp>0)Victory(u);if(u.hp<=0&&e.hp>0)Victory(e);if(u.owner==0||e.owner==0)Note(UnitNames[u.type]+" battles "+UnitNames[e.type]+".");units.RemoveAll(a=>a.hp<=0);DrownStranded();if(u.hp<=0||enemies.Any(a=>a.hp>0)){UpdatePortLinks();CheckVictory();return true;}}
                if(c!=null){if(u.type==4||u.type==9){u.moves=0;CheckVictory();return true;}DiplomacyOnCapture(u.owner,c.owner);LoseProjectCity(c);c.owner=u.owner;c.occupation=4;c.pop=Math.Max(1,c.pop-1);NormaliseSpecialists(c);c.queue.Clear();c.build=-1;c.production=0;c.buildingQueue.Clear();c.buildingProject=-1;c.buildingProduction=0;c.walls=false;RebuildTerritory();if(u.owner==0||Visible(x,y))Note(c.name+" occupied for 4 turns.");}}
            u.x=x;u.y=y;u.moves=Math.Max(0,u.moves-cost);u.fortified=false;networks=null;if(cargo!=null)CarryCargo(cargo,x,y);VisitVillage(u);ExploreRuin(u);UpdatePortLinks();Reveal();CheckVictory();return true;}
        public List<Unit> RangedTargets(Unit u){return units.Where(e=>e.owner!=u.owner&&AtWar(u.owner,e.owner)&&Distance(u.x,u.y,e.x,e.y)<=2&&VisibleTo(u.owner,e.x,e.y)).ToList();}
        public bool Ranged(Unit u,int targetId){if(u==null||result!=""||u.moves<=0||(u.type!=4&&u.type!=9))return false;var picked=RangedTargets(u).Find(a=>a.id==targetId);if(picked==null)return false;var e=BestDefender(u,picked.x,picked.y);int dx=Math.Sign(DeltaX(u.x,e.x)),dy=Math.Sign(e.y-u.y);var mid=Step(u.x,u.y,dx,dy);if(Distance(u.x,u.y,e.x,e.y)>1&&mid!=null&&(Wooded(mid.terrain)||mid.terrain==5))return false;var city=cities.Find(c=>c.x==e.x&&c.y==e.y&&c.owner==e.owner);if(u.type==9&&city!=null&&city.walls){city.walls=false;Note("City walls breached.");}else{int dealt,taken;Exchange(u,e,out dealt,out taken);e.hp-=dealt;}u.moves=0;if(e.hp<=0)Victory(u);units.RemoveAll(a=>a.hp<=0);CheckVictory();return true;}
        // Healing (CORE-DESIGN-16BIT.md): 1 a turn in the open, 2 fortified, 3 in your own city, full with a Barracks there.
        // Hospital (+2 in its city) and an adjacent Medic (+2) are existing extras kept on top. Units heal only if they did not move.
        public int HealAmount(Unit u){var city=cities.Find(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y);if(city!=null&&HasBuilding(city,32))return 10;int heal=city!=null?3:u.fortified?2:1;if(city!=null&&HasBuilding(city,35))heal+=2;if(units.Any(a=>a.owner==u.owner&&a.type==10&&a.id!=u.id&&Distance(a.x,a.y,u.x,u.y)<=1))heal+=2;return heal;}
        // Ranks come from victories: 2 make a Veteran (+1 strength), 5 an Elite (+2). promotion holds the rank and is added to strength.
        public static readonly string[] RankNames={"Regular","Veteran","Elite"};
        public static int RankFor(int victories){return victories>=5?2:victories>=2?1:0;}
        public void Victory(Unit u){u.victories++;int rank=RankFor(u.victories);if(rank<=u.promotion)return;u.promotion=rank;if(u.owner==0)Note(UnitNames[u.type]+" is now "+RankNames[rank]+" (+"+rank+" strength).");}
        public static string RankText(Unit u){return RankNames[Math.Max(0,Math.Min(2,u.promotion))]+" | Victories "+u.victories+(u.promotion<2?" (next rank at "+(u.promotion==0?2:5)+")":"");}
        public int UpgradeTarget(Unit u){return u.type==2?7:u.type==7?6:u.type==8?5:-1;}
        public bool Upgrade(Unit u){if(u==null||u.moves<=0)return false;int t=UpgradeTarget(u);if(t<0||!cities.Any(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y)||!nations[u.owner].tech[UnitTech[t]]||!HasResource(u.owner,UnitResource[t]))return false;int price=(UnitCosts[t]-UnitCosts[u.type])*2;if(u.owner==0)nations[0].gold=gold;if(nations[u.owner].gold<price)return false;nations[u.owner].gold-=price;if(u.owner==0)gold=nations[0].gold;u.type=t;u.moves=0;CancelOrder(u);return true;}
        public bool Improve(Unit u,bool farm){return StartJob(u,farm?2:1);}
        public string JobReason(Unit u,int job){if(u==null||u.type!=1||u.moves<=0)return "Worker needs movement.";var t=At(u.x,u.y);if(Barren(t.terrain))return "Requires passable land free of polar ice.";if(job==1&&!HasTech(u.owner,"wheel"))return "Requires The Wheel.";if(job==1)return !CanEnter(u.owner,t)?"Requires access to this territory.":t.road?"Road already built.":"";if(t.owner!=u.owner)return "Requires your territory.";if(job==5){if(!Wooded(t))return "No trees to clear.";return !HasTech(u.owner,"toolmaking")?"Requires Stone Tools.":"";}if((job==2||job==3)&&Wooded(t))return "Clear the trees first.";if(job==2){if(!HasTech(u.owner,"agriculture"))return "Requires Agriculture.";return t.farm||t.mine||!Farmable(t.terrain)?"Tile cannot support a farm/pasture.":"";}if(job==3){if(!HasTech(u.owner,"mining"))return "Requires Mining.";return t.mine||t.farm||!(t.terrain==2||t.resource==2||t.resource==3)?"Requires hills or a mineral deposit.":"";}if(job==4)return !HasTech(u.owner,"engineering")?"Requires Engineering.":t.fort?"Fort already built.":"";return "Unknown job.";}
        // The technology a worker job needs, or "" when the civilisation has it: activity buttons stay listed but are greyed out
        // until then (owner request).
        public static readonly string[] JobNames={"","Road","Farm","Mine","Fort","Clear woodland"};
        public string JobTechMissing(int owner,int job){string id=job==1?"wheel":job==2?"agriculture":job==3?"mining":job==4?"engineering":job==5?"toolmaking":null;return id==null||HasTech(owner,id)?"":"Requires "+TechNames[TechnologyCatalog.Index(id)]+".";}
        public int JobWork(Unit u,int job){var t=At(u.x,u.y);return job==1?2+(t.owner==u.owner?0:t.terrain==2?2:1):job==5?ClearingWork(u.owner):3;}
        // Clearing woodland (owner request, 31 Oct): 6 worker-turns with Stone Tools, 4 with Bronze Working, 3 with Iron Working.
        public int ClearingWork(int owner){return HasTech(owner,"iron")?3:HasTech(owner,"bronze")?4:6;}
        // Woodland must be cleared (job 5, from Stone Tools; faster with Bronze and Iron Working) before a farm or mine: Forest and Jungle become
        // Grassland, Arctic forest becomes Arctic tundra, Mangrove becomes Bog. Roads can still be built through woodland.
        public static bool Forested(int terrain){return terrain==1||terrain==11||terrain==13||terrain==19;}
        public static int Cleared(int terrain){return terrain==13?8:terrain==19?9:0;}
        public List<Unit> WorkerTeam(Unit u){return u==null?new List<Unit>():u.type!=1||u.workerGroup==0?new List<Unit>{u}:units.Where(a=>a.type==1&&a.owner==u.owner&&a.workerGroup==u.workerGroup&&a.x==u.x&&a.y==u.y).ToList();}
        public bool GroupWorkers(Unit u){if(u==null||u.type!=1)return false;var team=units.Where(a=>a.type==1&&a.owner==u.owner&&a.x==u.x&&a.y==u.y).ToList();if(team.Count<2)return false;foreach(var a in team){CancelOrder(a);a.fortified=false;a.workerGroup=u.id;}return true;}
        public void UngroupWorkers(Unit u){if(u==null||u.workerGroup==0)return;int group=u.workerGroup;foreach(var a in units.Where(a=>a.owner==u.owner&&a.workerGroup==group)){CancelOrder(a);a.workerGroup=0;}}
        public void CancelActivity(Unit u){foreach(var a in WorkerTeam(u)){CancelOrder(a);a.fortified=false;a.autoWork=false;a.roadOrder=false;}}
        public bool StartJob(Unit u,int job){var team=WorkerTeam(u);if(team.Count==0||team.Any(a=>JobReason(a,job)!=""))return false;int work=JobWork(u,job);foreach(var a in team){CancelOrder(a);a.fortified=false;a.job=job;a.jobTurns=work;a.moves=0;}return true;}
        public int JobTurnsRemaining(Unit u){if(u==null||u.job==0)return 0;int count=WorkerTeam(u).Count(a=>a.job==u.job);return (u.jobTurns+Math.Max(1,count)-1)/Math.Max(1,count);}
        public string Activity(Unit u){if(u.roadOrder&&u.job==0)return "Road to "+u.roadX+", "+u.roadY;if(u.autoWork&&u.job==0)return "Auto-developing";if(u.exploring)return "Auto-exploring";if(u.sentry)return "Sentry: watching for enemies";if(u.job>0)return new[]{"","Building road","Building farm / pasture","Building mine","Building fort","Clearing forest"}[u.job];if(u.hasOrder)return "Travelling to "+u.destinationX+", "+u.destinationY;if(u.fortified)return "Fortifying / healing";return u.moves==0?"Waiting for next turn":"Idle";}
        public string Remaining(Unit u){if(u.job>0)return JobTurnsRemaining(u)+" turns remaining";if(!u.hasOrder)return "Turns remaining: —";var route=Route(u,u.destinationX,u.destinationY);if(route.Count==0)return "Route blocked";int turns=0,budget=u.moves,x=u.x,y=u.y;foreach(var t in route){int cost=StepCost(u,x,y,u.owner==0?KnownTile(t):t);if(cost>budget){turns++;budget=Moves[u.type];}budget-=cost;x=t.x;y=t.y;}return "Approx. "+turns+" turns remaining";}
        void ProcessJobs(int owner){
            var processed=new HashSet<int>();
            foreach(var u in units.Where(a=>a.owner==owner&&a.job>0).ToList()){
                if(!processed.Add(u.id))continue;
                var team=WorkerTeam(u).Where(a=>a.job==u.job).ToList();foreach(var a in team){processed.Add(a.id);a.moves=0;}
                var t=At(u.x,u.y);if(IsImpassable(t.terrain)||(u.job==1?!CanEnter(owner,t):t.owner!=owner)){foreach(var a in team)CancelOrder(a);continue;}
                int job=u.job,remaining=Math.Max(0,team.Min(a=>a.jobTurns)-team.Count);
                foreach(var a in team)a.jobTurns=remaining;
                if(remaining==0){if(job==1)t.road=true;if(job==2)t.farm=true;if(job==3)t.mine=true;if(job==4)t.fort=true;if(job==5&&Wooded(t)){if(Forested(t.terrain))t.terrain=Cleared(t.terrain);else t.cleared=true;t.farm=false;}foreach(var a in team)a.job=0;networks=null;if(owner==0)Note(job==5?"Workers cleared the trees.":"Workers completed an improvement.");}
            }
        }
        public bool Pillage(Unit u){if(u==null||Power[u.type]==0||u.moves<=0)return false;var t=At(u.x,u.y);if(t.owner<0||!AtWar(u.owner,t.owner)||!(t.road||t.farm||t.mine||t.fort))return false;Grieve(t.owner,u.owner,6);t.road=t.farm=t.mine=t.fort=false;u.moves=0;networks=null;return true;}
        void Complete(City c){int b=c.build,u=BuildUnit(b);if(u<0)return;var probe=new Unit{owner=c.owner,type=u,id=-1};var launch=IsNaval(u)?LaunchTile(c,u):At(c.x,c.y);if(launch==null||!Capacity(probe,launch.x,launch.y))return;var made=Add(c.owner,u,launch.x,launch.y);made.moves=0;made.homeCity=c.id;if(u==0){c.pop--;NormaliseSpecialists(c);}if(HasBuilding(c,32)||OwnWonder(c.owner,63)||HasEdict(c.owner,3,2)||(HasTrait(c.owner,TraitMilitaristic)&&Power[u]>0)){made.victories=2;made.promotion=1;}c.production-=CostFor(c,b);if(c.owner==0)Note(c.name+" trained "+BuildName(b)+".");c.build=c.queue.Count>0?c.queue[0]:-1;if(c.queue.Count>0)c.queue.RemoveAt(0);if(c.build<0)c.production=0;}
        void CompleteBuilding(City c){int b=c.buildingProject;if(IsProject(b)){CompleteProjectStage(c,b);c.buildingProduction-=CostFor(c,b);c.buildingProject=c.buildingQueue.Count>0?c.buildingQueue[0]:-1;if(c.buildingQueue.Count>0)c.buildingQueue.RemoveAt(0);if(c.buildingProject<0)c.buildingProduction=0;return;}if(HasBuilding(c,b))return;if(b==7)c.granary=true;else if(b==8)c.library=true;else if(b==9)c.walls=true;else c.buildings.Add(b);c.buildingProduction-=CostFor(c,b);if(IsWonder(b))CancelWonderRivals(c,b);if(c.owner==0)Note(c.name+" constructed "+BuildName(b)+".");c.buildingProject=c.buildingQueue.Count>0?c.buildingQueue[0]:-1;if(c.buildingQueue.Count>0)c.buildingQueue.RemoveAt(0);if(c.buildingProject<0)c.buildingProduction=0;}
        public void Produce(City c,int output){
            if(c.build>=0&&BuildReason(c,c.build)==""){c.production+=output;if(c.production>=CostFor(c,c.build))Complete(c);}
            if(c.buildingProject>=0&&BuildReason(c,c.buildingProject)==""){c.buildingProduction+=output;if(c.buildingProduction>=CostFor(c,c.buildingProject))CompleteBuilding(c);}
        }
        void ProcessNation(int owner){UpdatePortLinks();var n=nations[owner];var budget=Budget(owner);int balance=n.gold+budget[2]-DiplomaticNet(owner);n.gold=Math.Max(0,balance);n.arrears=balance<0?n.arrears+1:0;if(owner==0&&balance<0)Note("Treasury empty: unpaid upkeep reduces combat strength. Reduce expenses or raise taxes.");foreach(var c in cities.Where(a=>a.owner==owner).ToList()){NormaliseSpecialists(c);AutoEntertainers(c);var v=Yield(c);c.age++;ProcessCityFood(c,v[0]);if(c.occupation>0)c.occupation--;Produce(c,v[1]);if(n.research>=0&&AvailableTech(owner).Contains(n.research))n.progress[n.research]+=v[2];}
            if(n.research>=0&&AvailableTech(owner).Contains(n.research)&&n.progress[n.research]>=ResearchCost(n.research,owner)){int eraBefore=Era(owner);n.progress[n.research]-=ResearchCost(n.research,owner);n.tech[n.research]=true;if(owner==0)Note(TechNames[n.research]+" discovered.");AnnounceEra(owner,eraBefore);n.research=owner==0?AvailableTech(owner).OrderBy(t=>ResearchCost(t,owner)).DefaultIfEmpty(-1).First():PreferredResearch(owner);}
            foreach(var u in units.Where(a=>a.owner==owner).ToList()){var on=At(u.x,u.y);if(on.terrain==14||on.terrain==15)u.hp=Math.Max(1,u.hp-1);else if(u.moves==Moves[u.type]||u.fortified)u.hp=Math.Min(10,u.hp+HealAmount(u));u.moves=Moves[u.type];}
            ProcessJobs(owner);ProcessEmpireOrders(owner);if(n.revolution>0)n.revolution--;
        }
        public void EndTurn(){if(processing||result!="")return;processing=true;try{nations[0].gold=gold;nations[0].tech=tech;nations[0].research=research;if(research>=0)nations[0].progress[research]=science;
            ProcessDiplomacyPayments();for(int owner=1;owner<civCount;owner++){ProcessNation(owner);AI(owner);}RebuildTerritory();ProcessNation(0);UpdateReligion();SpreadInfluence();gold=nations[0].gold;tech=nations[0].tech;research=nations[0].research;science=research<0?0:nations[0].progress[research];turn++;AdvanceDiplomacy();UpdateConcord();UpdateProjects();Reveal();DiplomacyAI();foreach(var u in units.Where(a=>a.owner==0&&a.hasOrder).ToList())ContinueOrder(u);CheckVictory();}finally{processing=false;}}
        void AI(int owner){EmpireAI(owner);AdjustScienceRate(owner);var r=new Random(seed+turn*997+owner);foreach(var c in cities.Where(a=>a.owner==owner)){ManageCityAI(c);var own=units.Where(a=>a.owner==owner).ToList();if(c.build<0){var choices=Choices(c);int naval=AINavalBuild(c);int desired=ShouldTrainSettler(c)?0:own.Count(u=>u.type==1)<cities.Count(a=>a.owner==owner)?1:naval>=0&&!(Budget(owner)[2]<0&&nations[owner].gold<15)?naval:own.Count>=14||(Budget(owner)[2]<0&&nations[owner].gold<15)?-1:choices.Where(b=>BuildUnit(b)>=2&&!IsNaval(BuildUnit(b))).OrderByDescending(b=>MilitaryValue(BuildUnit(b))).DefaultIfEmpty(2).First();c.build=desired;}if(c.buildingProject<0){int building=PreferredBuilding(c);if(BuildingIds.Contains(building))ChooseProject(c,building,false);}}
            foreach(var u in units.Where(a=>a.owner==owner).ToList()){if(!units.Contains(u)||result!=""||u.job>0)continue;if(u.type==11){AICaravan(u);continue;}if(IsNaval(u.type)){NavalUnitAI(u);continue;}if(u.type==0){PlanAISettlement(u);continue;}if(AtSea(u)){if(!units.Any(g=>g.owner==owner&&g.type==GalleyUnit&&g.x==u.x&&g.y==u.y))GoAshore(u);continue;}
                if(u.type==1){ImproveAI(u);continue;}
                if(Power[u.type]>0&&cities.Any(c=>c.owner==owner&&c.x==u.x&&c.y==u.y)&&!units.Any(a=>a.id!=u.id&&a.owner==owner&&Power[a.type]>0&&a.x==u.x&&a.y==u.y)){u.fortified=true;continue;}
                if((u.type==4||u.type==9)&&RangedTargets(u).Count>0&&Ranged(u,RangedTargets(u)[0].id))continue;
                if(JoinInvasion(u))continue;
                var target=units.Where(a=>AtWar(owner,a.owner)&&VisibleTo(owner,a.x,a.y)).OrderBy(a=>Distance(u.x,u.y,a.x,a.y)).FirstOrDefault();if(target!=null){Order(u,target.x,target.y);continue;}
                var enemyCity=cities.Where(c=>AtWar(owner,c.owner)&&VisibleTo(owner,c.x,c.y)).OrderBy(c=>Distance(u.x,u.y,c.x,c.y)).FirstOrDefault();if(enemyCity!=null){Order(u,enemyCity.x,enemyCity.y);continue;}
                for(int attempt=0;attempt<8;attempt++){int x=u.x+r.Next(-1,2),y=u.y+r.Next(-1,2);var step=At(x,y);if(step==null||IsWater(step.terrain))continue;if(Move(u,x,y))break;}
            }
        }
        public string Calendar(){int elapsed=turn-1;int year=-4000+Math.Min(elapsed,100)*40+Math.Min(Math.Max(0,elapsed-100),200)*10+Math.Max(0,elapsed-300);return year<0?(-year)+" BCE":(year+1)+" CE";}
        // Transcendence is won in UpdateProjects; conquest is holding every rival capital or eliminating every rival (Victory.cs).
        public void CheckVictory(){if(result!="")return;if(!units.Any(u=>u.owner==0)&&!cities.Any(c=>c.owner==0)){result="Your civilisation has fallen.";return;}if(ConquestAchieved(0)){result="Conquest victory! Every rival capital is yours.";return;}for(int o=1;o<civCount;o++)if(ConquestAchieved(o)){result=NationName(o)+" has taken every capital. Conquest defeat.";return;}}
        public void UpgradeSave(){if(version<8){mapWidth=DefaultWidth;mapHeight=DefaultHeight;mapSizeIndex=1;}if(version==1){civilisationName="Verdant Union";met=new[]{true,false,false};version=2;}if(version==2){var oldTech=tech;tech=new bool[LegacyTechnologyCatalog.Count];if(oldTech!=null)Array.Copy(oldTech,tech,Math.Min(6,oldTech.Length));nations=new[]{new Nation(),new Nation(),new Nation()};foreach(var fresh in nations){fresh.tech=new bool[LegacyTechnologyCatalog.Count];fresh.progress=new int[LegacyTechnologyCatalog.Count];fresh.research=-1;}nations[0].tech=tech;nations[0].gold=gold;nations[0].research=research;if(research>=0&&research<12)nations[0].progress[research]=science;openBorders=new bool[3];foreach(var c in cities){c.buildings=new List<int>();c.queue=new List<int>();}foreach(var t in tiles){t.rememberedTerrain=t.terrain;t.rememberedResource=t.resource;t.rememberedOwner=-1;t.rememberedRoad=t.road;t.rememberedFarm=t.farm;}version=3;RebuildTerritory();Reveal();Note("Save upgraded to foundation alpha 0.2.");}if(version==3){foreach(var c in cities){int oldBuild=c.build,oldProduction=c.production;var oldQueue=c.queue??new List<int>();c.build=-1;c.production=0;c.queue=new List<int>();c.buildingProject=-1;c.buildingProduction=0;c.buildingQueue=new List<int>();foreach(int project in (oldBuild>=0?new[]{oldBuild}:new int[0]).Concat(oldQueue)){bool building=BuildUnit(project)<0;if(building){if(HasBuilding(c,project)||c.buildingProject==project||c.buildingQueue.Contains(project))continue;if(c.buildingProject<0)c.buildingProject=project;else c.buildingQueue.Add(project);}else{if(c.build<0)c.build=project;else c.queue.Add(project);}}if(oldBuild>=0){if(BuildUnit(oldBuild)>=0)c.production=oldProduction;else c.buildingProduction=oldProduction;}}version=4;}
            if(version==4)MigrateTechnologySave();
            if(version==5){UpgradeEmpireSave();version=6;}
            if(version>=6&&nations!=null&&nations.Length>=2&&nations[0]!=null)nations[0].tech=tech;UpgradeEmpireSave();if(version==6){MigrateDiplomacy();version=7;}if(version==7)version=8;if(version==8){mapShape=0;version=9;}if(version==9){civCount=3;if(diplomacy!=null)foreach(var p in diplomacy)if(p!=null&&p.a!=0)p.atWar=rivalWar;version=10;}
            // Version 11: the Low/Normal/High tax setting becomes a science rate (Low tax = 60%, Normal = 50%, High = 30%).
            if(version==10){if(nations!=null)foreach(var n in nations)if(n!=null){n.scienceRate=n.tax==0?60:n.tax==2?30:50;n.tax=TaxLevel(n.scienceRate);}version=11;}
            // Version 12: ranks come from victories. Old manual promotions (up to 3) cap at Elite; unspent experience of 5 or more
            // (for example from a Barracks) counts as Veteran; victories are set to match the rank.
            if(version==11){if(units!=null)foreach(var u in units)if(u!=null){u.promotion=Math.Max(0,Math.Min(2,Math.Max(u.promotion,u.experience>=5?1:0)));u.victories=u.promotion==2?5:u.promotion==1?2:0;}version=12;}
            if(version==12){MigrateKeystones();version=13;}
            // Version 14: capitals are recorded (each civilisation's lowest-numbered city it still holds) and every civilisation
            // gets an empty Transcendence project. Knowing the Transcendence technology no longer wins by itself.
            if(version==13){for(int o=0;o<civCount&&nations!=null&&o<nations.Length;o++){var n=nations[o];if(n==null)continue;var own=cities.Where(c=>c.owner==o).OrderBy(c=>c.id).FirstOrDefault();n.capital=own==null?-1:own.id;n.projectCity=-1;n.projectStage=0;n.projectActivation=0;}version=14;}
            // Version 15: cities gain an empty list of locked tiles.
            if(version==14){if(cities!=null)foreach(var c in cities)if(c!=null&&c.locked==null)c.locked=new List<int>();version=15;}
            // Version 16: civilisations with two traits. The player's old trait (Builders, Scholars, Traders) becomes Industrious,
            // Scholarly or Mercantile; existing rivals keep their names and get no traits, so a game in progress does not change.
            if(version==15){for(int o=0;nations!=null&&o<nations.Length;o++){var n=nations[o];if(n==null)continue;n.traits=o==0?TraitsFromLegacy(n.trait):new[]{-1,-1};n.civ=-1;n.portrait=-1;if(n.leader==null)n.leader="";}version=16;}
            // 17: resources 6-15 (sea resources), land types 2-4, and passable poles. Old worlds keep their tiles; only the rules changed.
            if(version==16)version=17;
            // 18: boats, ships and ports (City.portX/portY/portLinked) and tribal villages that join a civilisation (Tile.villageOwner).
            if(version==17){foreach(var c in cities){c.portX=-1;c.portY=-1;c.portLinked=false;}foreach(var t in tiles)t.villageOwner=-1;version=18;}
            // 19: new globe maps use staggered rows (mapShape 2). Older maps keep their diagonal layout (shape 0 or 1); worker job 5 clears woodland.
            if(version==18)version=19;
            // 20: every civilisation gets a flag (the player's keeps its old Leaf, Star or Sun as the emblem).
            if(version==19){EnsureFlags();version=20;}
            // 21: 144 minor technologies follow the 36 keystones (tech and progress arrays grow; nothing known changes); workers gain
            // auto develop and road routes; tiles gain ancient ruins (none on old maps).
            if(version==20){if(nations!=null)foreach(var n in nations)if(n!=null){n.tech=Grow(n.tech,TechnologyCatalog.Count);n.progress=Grow(n.progress,TechnologyCatalog.Count);}if(nations!=null&&nations.Length>0&&nations[0]!=null)tech=nations[0].tech;version=21;}if(version==21){UpgradeReligion();version=22;}networks=null;}
        static T[] Grow<T>(T[] a,int n){if(a==null||a.Length>=n)return a;var b=new T[n];Array.Copy(a,b,a.Length);return b;}
        public bool Valid(){if(!EmpireValid()||!DiplomacyValid()||!FlagsValid())return false;if(version!=22||!ReligionValid()||!PortValid()||tiles.Any(t=>t.villageOwner< -1||t.villageOwner>=civCount)||mapType<0||mapType>=LandTypeNames.Length||!CivilisationsValid()||!EdictsValid()||!ProjectsValid()||civCount<2||civCount>16||(mapShape<0||mapShape>2)||mapWidth<16||mapWidth>400||mapHeight<10||mapHeight>(mapShape==2?400:300)||turn<1||nextId<1||tiles==null||tiles.Length!=Width*Height||units==null||cities==null||log==null||result==null||tech==null||tech.Length!=TechnologyCatalog.Count||met==null||met.Length!=civCount||war==null||war.Length!=civCount||openBorders==null||openBorders.Length!=civCount||nations==null||nations.Length!=civCount||string.IsNullOrWhiteSpace(civilisationName)||civilisationName.Length>32||gold<0||science<0||research< -1||research>=TechnologyCatalog.Count||speed<0||speed>2||flag<0||flag>2)return false;
            if(nations.Any(n=>n==null||n.tech==null||n.tech.Length!=TechnologyCatalog.Count||n.progress==null||n.progress.Length!=TechnologyCatalog.Count||n.known==null||n.known.Length!=tiles.Length||n.research< -1||n.research>=TechnologyCatalog.Count||n.progress.Any(v=>v<0)||n.tax<0||n.tax>2||n.scienceRate<0||n.scienceRate>MaxScienceRate||n.scienceRate%10!=0||n.gold<0))return false;
            for(int i=0;i<tiles.Length;i++){var t=tiles[i];if(t==null||t.x!=i%Width||t.y!=i/Width||t.terrain<0||t.terrain>=TerrainNames.Length||t.rememberedTerrain<0||t.rememberedTerrain>=TerrainNames.Length||t.resource<0||t.resource>15||t.owner< -1||t.owner>=civCount)return false;}
            if(units.Any(u=>u==null||u.type<0||u.type>=UnitNames.Length||u.promotion<0||u.promotion>2||u.victories<0||u.owner<0||u.owner>=civCount||At(u.x,u.y)==null||u.hp<1||u.hp>10||u.moves<0||u.moves>Moves[u.type]||u.id<1||u.id>=nextId||u.job<0||u.job>5||u.jobTurns<0||u.workerGroup<0||(u.workerGroup>0&&u.type!=1)||(u.hasOrder&&At(u.destinationX,u.destinationY)==null)))return false;
            if(cities.Any(c=>c==null||c.owner<0||c.owner>=civCount||At(c.x,c.y)==null||c.pop<1||c.pop>40||c.name==null||c.buildings==null||c.queue==null||c.buildingQueue==null||c.queue.Count>8||c.buildingQueue.Count>8||c.id<1||c.id>=nextId||c.food<0||c.production<0||(c.build!=-1&&BuildUnit(c.build)<0)||c.queue.Any(b=>BuildUnit(b)<0)||c.buildingProduction<0||(c.buildingProject!=-1&&(!BuildingIds.Contains(c.buildingProject)||HasBuilding(c,c.buildingProject)))||c.buildingQueue.Any(b=>!BuildingIds.Contains(b)||HasBuilding(c,b)||b==c.buildingProject)||c.buildingQueue.Distinct().Count()!=c.buildingQueue.Count||c.locked==null||c.locked.Any(k=>k<0||k>=tiles.Length)||c.locked.Distinct().Count()!=c.locked.Count||c.buildings.Distinct().Count()!=c.buildings.Count))return false;
            var ids=units.Select(u=>u.id).Concat(cities.Select(c=>c.id)).ToList();return ids.Distinct().Count()==ids.Count;}
    }
}
