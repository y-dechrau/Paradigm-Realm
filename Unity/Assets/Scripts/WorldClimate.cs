using System;
using System.Collections.Generic;
using System.Linq;

namespace ParadigmRealm
{
    // Climate-driven world generation for sphere maps (mapShape 1).
    // Pipeline: seeded 3D noise on a unit sphere -> elevation and sea level -> temperature and moisture bands (0-7)
    // -> Whittaker-style biome table onto the 21 terrain types -> coasts, lakes, rivers, coral and polar ice
    // -> start sites on the largest landmass. Everything gameplay-relevant (bands, terrain ids, starts) is integer;
    // floating point is used only inside the generator and never saved.
    public partial class World
    {
        static double Lattice(int s,int x,int y,int z)
        {
            unchecked
            {
                uint h=(uint)s*374761393u+(uint)x*668265263u+(uint)y*2147483647u+(uint)z*1274126177u;
                h=(h^(h>>13))*1274126177u;h^=h>>16;
                return (h&0xFFFFFF)/16777216.0;
            }
        }
        static double SmoothStep(double t){return t*t*(3-2*t);}
        static double Noise3(int s,double x,double y,double z)
        {
            int x0=(int)Math.Floor(x),y0=(int)Math.Floor(y),z0=(int)Math.Floor(z);
            double fx=SmoothStep(x-x0),fy=SmoothStep(y-y0),fz=SmoothStep(z-z0);
            double c000=Lattice(s,x0,y0,z0),c100=Lattice(s,x0+1,y0,z0),c010=Lattice(s,x0,y0+1,z0),c110=Lattice(s,x0+1,y0+1,z0);
            double c001=Lattice(s,x0,y0,z0+1),c101=Lattice(s,x0+1,y0,z0+1),c011=Lattice(s,x0,y0+1,z0+1),c111=Lattice(s,x0+1,y0+1,z0+1);
            double a=c000+(c100-c000)*fx,b=c010+(c110-c010)*fx,c=c001+(c101-c001)*fx,d=c011+(c111-c011)*fx;
            double e=a+(b-a)*fy,f=c+(d-c)*fy;
            return e+(f-e)*fz;
        }
        static double Fbm(int s,double x,double y,double z,int octaves)
        {
            double sum=0,amp=1,norm=0,freq=1;
            for(int i=0;i<octaves;i++){sum+=amp*Noise3(s+i*101,x*freq,y*freq,z*freq);norm+=amp;amp*=.5;freq*=2;}
            return sum/norm;
        }

        // Biome table: temperature band 0 (polar) to 7 (hot) against moisture band 0 (arid) to 7 (soaked).
        public static int BiomeFor(int t,int m)
        {
            if(t<=0)return 8;
            if(t==1)return m<=3?8:13;
            if(t==2)return m<=1?20:m<=3?0:13;
            if(t==3)return m<=1?20:m<=3?0:1;
            if(t==4)return m==0?3:m==1?20:m<=3?0:1;
            if(t==5)return m<=1?3:m==2?20:m<=4?0:1;
            if(t==6)return m<=1?3:m<=4?16:11;
            return m<=2?3:m<=4?16:11;
        }

        // Last generated climate, kept for the harness and the globe view. Not saved.
        [NonSerialized] public int[] climateTemperature,climateMoisture;
        [NonSerialized] double[] elevation;

        int[] SphereNeighbours(int i)
        {
            int x=i%Width,y=i/Width,count=0;var buffer=new int[8];
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
            {
                if(dx==0&&dy==0)continue;
                var t=Step(x,y,dx,dy);if(t==null)continue;
                buffer[count++]=t.y*Width+t.x;
            }
            var result=new int[count];Array.Copy(buffer,result,count);return result;
        }

        double[] BoxSmooth(double[] a,int passes)
        {
            int n=a.Length;
            for(int pass=0;pass<passes;pass++)
            {
                var b=new double[n];
                for(int i=0;i<n;i++){double sum=a[i];int count=1;foreach(int j in SphereNeighbours(i)){sum+=a[j];count++;}b[i]=sum/count;}
                a=b;
            }
            return a;
        }

        // Point on the unit sphere for a tile centre, so noise is continuous across the east-west seam.
        // Staggered rows: odd rows sit half a tile east, so their longitude is half a column further on.
        double RowShift(int y){return Staggered&&(y&1)==1?.5:0;}
        void SpherePoint(int x,int y,out double px,out double py,out double pz)
        {
            double lat=Math.PI*(0.5-(y+.5)/Height),lon=2*Math.PI*(x+.5+RowShift(y))/Width,cl=Math.Cos(lat);
            px=cl*Math.Cos(lon);py=Math.Sin(lat);pz=cl*Math.Sin(lon);
        }

        // Fills tiles (already allocated with coordinates) with terrain for one seed variant.
        double pangaeaX,pangaeaY,pangaeaZ;
        public static readonly string[] LandTypeNames={"Continents","One landmass","Archipelago","Small continents","Inland seas"};
        public static readonly string[] LandTypeTexts={"A few large continents with open oceans between them.","A single great landmass surrounded by ocean.","Many islands; most of the world is sea. Ships matter early, and neighbours may be far apart.","Several mid-sized continents.","More land, broken by lakes and enclosed inland seas."};
        // Water bodies too small to be ocean become lakes (used for the lakes-and-inland-seas land type).
        void SmallSeasToLakes(int limit)
        {
            int n=tiles.Length;var seen=new bool[n];
            for(int i=0;i<n;i++){if(seen[i]||tiles[i].terrain!=4)continue;var part=new List<int>();var q=new Queue<int>();q.Enqueue(i);seen[i]=true;
                while(q.Count>0){int k=q.Dequeue();part.Add(k);foreach(int j in SphereNeighbours(k))if(!seen[j]&&tiles[j].terrain==4){seen[j]=true;q.Enqueue(j);}}
                if(part.Count<limit)foreach(int k in part)tiles[k].terrain=6;}
        }
        // Realistic neighbours (Whittaker biome model): neighbouring land tiles may differ by at most one temperature step
        // (polar, cold, temperate, hot) and dry land may not touch wet land. Where they clash, the hotter tile is cooled one step
        // (desert to steppe, jungle to forest, grassland to tundra next to ice) and dry tiles are moistened one step (desert to
        // savanna, steppe and wasteland to grassland). Water, rivers, mountains, volcanoes and rocky coasts are neutral.
        public static int TemperatureClass(int t){switch(t){case 14:case 15:return 0;case 8:case 13:return 1;case 3:case 16:case 11:case 19:return 3;case 0:case 1:case 2:case 9:case 10:case 20:return 2;default:return -1;}}
        public static int MoistureClass(int t){switch(t){case 3:case 10:case 20:return 0;case 9:case 11:case 19:return 2;case 0:case 1:case 2:case 8:case 13:case 16:return 1;default:return -1;}}
        static int Cooler(int t){switch(t){case 3:return 20;case 16:return 0;case 11:case 19:return 1;case 1:return 13;case 0:case 2:case 9:case 10:case 20:return 8;default:return t;}}
        static int Wetter(int t){switch(t){case 3:return 16;case 10:case 20:return 0;default:return t;}}
        public int HarmoniseBiomes()
        {
            int changes=0;
            for(int pass=0;pass<8;pass++)
            {
                bool any=false;
                foreach(var t in tiles)
                {
                    int tc=TemperatureClass(t.terrain),wc=MoistureClass(t.terrain);if(tc<0)continue;
                    for(int dy=-1;dy<=1&&tc>=0;dy++)for(int dx=-1;dx<=1;dx++)
                    {
                        if(dx==0&&dy==0)continue;var n=Step(t.x,t.y,dx,dy);if(n==null)continue;int nc=TemperatureClass(n.terrain),nw=MoistureClass(n.terrain);
                        if(nc>=0&&tc-nc>=2){int to=Cooler(t.terrain);if(to!=t.terrain){t.terrain=to;changes++;any=true;}break;}
                        if(nw>=0&&wc==0&&nw==2){int to=Wetter(t.terrain);if(to!=t.terrain){t.terrain=to;changes++;any=true;}break;}
                    }
                }
                if(!any)break;
            }
            return changes;
        }
        public bool BiomeClash(Tile a,Tile b){int ta=TemperatureClass(a.terrain),tb=TemperatureClass(b.terrain),wa=MoistureClass(a.terrain),wb=MoistureClass(b.terrain);return ta>=0&&tb>=0&&Math.Abs(ta-tb)>=2||wa>=0&&wb>=0&&Math.Abs(wa-wb)>=2;}
        void BuildClimateWorld(int s)
        {
            int W=Width,H=Height,n=W*H;
            var elev=new double[n];var ridge=new double[n];var lat=new double[H];
            {var pr=new Random(s^0x2a);double a=pr.NextDouble()*Math.PI*2,b=(pr.NextDouble()-.5)*.8;pangaeaX=Math.Cos(b)*Math.Cos(a);pangaeaY=Math.Sin(b);pangaeaZ=Math.Cos(b)*Math.Sin(a);}
            for(int y=0;y<H;y++)
            {
                lat[y]=Math.PI*(0.5-(y+.5)/H);
                for(int x=0;x<W;x++)
                {
                    double lon=2*Math.PI*(x+.5+RowShift(y))/W,cl=Math.Cos(lat[y]);
                    double px=cl*Math.Cos(lon),py=Math.Sin(lat[y]),pz=cl*Math.Sin(lon);
                    // Land types (New Game): 0 continents, 1 one landmass, 2 archipelago, 3 small continents, 4 lakes and inland seas.
                    double broad=mapType==3?2.6:1.4;
                    double c=Fbm(s,px*broad+7,py*broad+7,pz*broad+7,2),d=Fbm(s+17,px*4+3,py*4+3,pz*4+3,4);
                    double e=c*.62+d*.38;
                    if(mapType==1){double dot=px*pangaeaX+py*pangaeaY+pz*pangaeaZ;e=e*.55+Math.Max(0,dot)*.45;}
                    if(mapType==2)e=c*.28+Fbm(s+71,px*7+5,py*7+5,pz*7+5,3)*.72;
                    elev[y*W+x]=e-.10*Math.Pow(Math.Abs(Math.Sin(lat[y])),4);
                    ridge[y*W+x]=1-Math.Abs(2*Fbm(s+31,px*5+1,py*5+1,pz*5+1,3)-1);
                }
            }
            // Colliding plates lift land into ranges; spreading ones open rifts and seas.
            double[] rift;var uplift=TectonicUplift(s,out rift);for(int i=0;i<n;i++)elev[i]+=.22*uplift[i]-.10*rift[i];
            var sorted=(double[])elev.Clone();Array.Sort(sorted);
            double sea=sorted[(int)(n*(mapType==2?.78:mapType==3?.64:mapType==4?.47:mapType==1?.6:.62))],top=sorted[n-1];
            var land=new bool[n];var hn=new double[n];
            for(int i=0;i<n;i++){land[i]=elev[i]>sea;hn[i]=land[i]?(elev[i]-sea)/(top-sea):0;}
            elevation=elev;
            // Distance to the nearest ocean tile (capped), for continental dryness.
            var dist=new int[n];var queue=new Queue<int>();
            for(int i=0;i<n;i++){if(land[i])dist[i]=99;else{dist[i]=0;queue.Enqueue(i);}}
            while(queue.Count>0){int i=queue.Dequeue();foreach(int j in SphereNeighbours(i))if(dist[j]>dist[i]+1){dist[j]=dist[i]+1;queue.Enqueue(j);}}
            // Temperature falls with latitude and altitude; moisture follows the global wind belts, the distance
            // from the sea and rain shadows behind high ground.
            var temp=new double[n];var wet=new double[n];
            for(int y=0;y<H;y++)
            {
                double al=Math.Abs(lat[y]);
                int wind=(al<Math.PI/6||al>Math.PI/3)?-1:1; // +1 blows towards the east (westerlies), -1 towards the west (trade winds, polar easterlies)
                for(int x=0;x<W;x++)
                {
                    int i=y*W+x;
                    double t=Math.Pow(Math.Cos(lat[y]),1.5);
                    if(land[i])t-=hn[i]*.45;
                    double qx,qy,qz;SpherePoint(x,y,out qx,out qy,out qz);t+=(Fbm(s+53,qx*4+9,qy*4+9,qz*4+9,2)-.5)*.12;
                    temp[i]=t;
                    double m=.62+.33*Math.Cos(6*lat[y]);
                    if(land[i])m-=Math.Min(dist[i],14)/14.0*.30;
                    m+=(Fbm(s+71,qx*3+5,qy*3+5,qz*3+5,3)-.5)*.5;
                    if(land[i])
                    {
                        for(int k=1;k<8;k++)
                        {
                            var up=At(x-wind*k,y);if(up==null)break;
                            int j=up.y*W+up.x;
                            if(land[j]&&hn[j]>.5){m-=.28*(1-k/9.0);break;}
                        }
                        if(hn[i]>.5)m+=.15;
                    }
                    wet[i]=m;
                }
            }
            temp=BoxSmooth(temp,2);wet=BoxSmooth(wet,2);
            var tb=new int[n];var mb=new int[n];
            for(int i=0;i<n;i++){tb[i]=Math.Max(0,Math.Min(7,(int)(temp[i]*8)));mb[i]=Math.Max(0,Math.Min(7,(int)(wet[i]*8)));}
            climateTemperature=tb;climateMoisture=mb;
            for(int i=0;i<n;i++)
            {
                var tile=tiles[i];tile.resource=0;tile.road=false;tile.farm=false;tile.mine=false;tile.fort=false;tile.village=false;
                if(!land[i]){tile.terrain=(tb[i]==0&&Math.Abs(lat[i/W])>1.2)?14:4;continue;}
                double h=hn[i];int t2=tb[i],m2=mb[i];
                // Mountains stand along converging plate boundaries (broken up by the ridge noise) or on the very highest ground;
                // hills fringe the ranges and cover high plateaus.
                if(h>.86||(uplift[i]>.55&&h>.22&&ridge[i]>.5))tile.terrain=t2<=1?15:5;
                else if(h>.55||(uplift[i]>.3&&h>.12&&ridge[i]>.35))tile.terrain=t2>0?2:8;
                else
                {
                    int b=BiomeFor(t2,m2);
                    if(b==3&&t2>=4&&m2==0&&WastelandNoise(s,i%W,i/W)>.62)b=10;
                    if(h<.06&&m2>=6&&t2>=2)b=9;
                    tile.terrain=b;
                }
            }
            CapMountainRanges(hn,tb);
            // Volcanic ground on warm high hills, with a deterministic hash choosing a few clusters.
            for(int i=0;i<n;i++){var t=tiles[i];if(t.terrain==2&&tb[i]>=3&&hn[i]>.6&&Lattice(s+5,i%W/3,i/W/3,0)>.9)t.terrain=17;}
            // Coasts: low ground beside the sea becomes rocky shoreline where hills are near, mangrove in the wet tropics.
            for(int i=0;i<n;i++)
            {
                var t=tiles[i];if(Barren(t.terrain))continue;
                bool coast=false,highNear=false;
                foreach(int j in SphereNeighbours(i)){int tj=tiles[j].terrain;if(tj==4||tj==12||tj==14)coast=true;if(tj==2||tj==5||tj==17)highNear=true;}
                if(!coast)continue;
                if(t.terrain==11||(tb[i]>=6&&mb[i]>=5&&t.terrain!=2))t.terrain=19;
                // Rocky shores only where high ground meets the sea (cliffs), or on some cold coasts (owner request: fewer).
                else if(highNear&&Lattice(s+13,i%W,i/W,1)<.55||tb[i]<=1&&Lattice(s+17,i%W,i/W,1)<.3)t.terrain=18;
            }
            // Coral reefs grow in shallow warm water; ice sheets cap cold seas (already set above).
            for(int i=0;i<n;i++)
            {
                var t=tiles[i];if(t.terrain!=4||tb[i]<6||dist[i]>2||dist[i]==0)continue;
                if(Lattice(s+29,i%W,i/W,2)>.45)t.terrain=12;
            }
            MakeLakes(s,land,dist,hn,tb);
            MakeRivers(s,land,hn,mb);
        }

        // Land resources where they occur on Earth (owner request, 31 Oct): metal ores in hills and volcanic ground, strongest
        // in the mountain belts; gold also in river gravels and old desert shields; horses on steppe, grassland and savanna
        // (domesticated on the Eurasian steppe); wheat on temperate grassland and steppe, twice as likely beside fresh water;
        // spices in the tropics (jungle, mangrove, wet savanna). Seeded, so a world is repeatable.
        void PlaceLandResources(Random r)
        {
            foreach(var t in tiles)
            {
                if(t.resource!=0||IsWater(t.terrain)||Barren(t.terrain))continue;
                bool mountainsNear=false,water=false;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n==null)continue;if(n.terrain==5||n.terrain==15)mountainsNear=true;if(n.terrain==6||n.terrain==7)water=true;}
                int roll=r.Next(1000);
                switch(t.terrain)
                {
                    case 2:  t.resource=roll<(mountainsNear?330:220)?2:roll<(mountainsNear?420:265)?3:0;break;      // hills: iron, gold near ranges
                    case 17: t.resource=roll<180?2:roll<230?3:0;break;                                              // volcanic: iron, some gold
                    case 7:  t.resource=roll<45?3:roll<150?1:0;break;                                               // river: placer gold, wheat
                    case 3:  t.resource=roll<40?3:0;break;                                                           // desert shields: rare gold
                    case 20: t.resource=roll<150?4:roll<(water?250:200)?1:0;break;                                  // dry steppe: horses, wheat
                    case 0:  t.resource=roll<(water?140:70)?1:roll<(water?200:130)?4:0;break;                       // grassland: wheat, horses
                    case 16: t.resource=roll<70?4:roll<130?5:0;break;                                               // savanna: horses, spices
                    case 11: t.resource=roll<160?5:0;break;                                                          // jungle: spices
                    case 19: t.resource=roll<90?5:0;break;                                                           // mangrove: spices
                }
            }
        }
        // Plate tectonics (owner request, 31 Oct): 8-14 plates seeded on the globe, each turning about its own axis. A tile
        // belongs to the nearest plate centre (with a little noise so borders wander). Where two plates move towards each other
        // the crust is pushed up into long ranges, as along the Andes or the Himalaya; where they move apart it sinks into rifts
        // and seas. Returns uplift 0..1 per tile (convergence, fading over three tiles from the boundary) and rift 0..1.
        double[] TectonicUplift(int s,out double[] rift)
        {
            int n=tiles.Length;var rng=new Random(s^0x5eed);int plates=8+rng.Next(7);
            var cx=new double[plates];var cy=new double[plates];var cz=new double[plates];var ax=new double[plates];var ay=new double[plates];var az=new double[plates];
            for(int k=0;k<plates;k++){double u=rng.NextDouble()*2-1,a=rng.NextDouble()*Math.PI*2,r=Math.Sqrt(1-u*u);cx[k]=r*Math.Cos(a);cy[k]=u;cz[k]=r*Math.Sin(a);
                double v=rng.NextDouble()*2-1,b=rng.NextDouble()*Math.PI*2,q=Math.Sqrt(1-v*v),speed=.5+rng.NextDouble();ax[k]=q*Math.Cos(b)*speed;ay[k]=v*speed;az[k]=q*Math.Sin(b)*speed;}
            var plate=new int[n];var px=new double[n];var py=new double[n];var pz=new double[n];
            for(int i=0;i<n;i++){double x,y,z;SpherePoint(i%Width,i/Width,out x,out y,out z);px[i]=x;py[i]=y;pz[i]=z;
                double wx=x+(Fbm(s+211,x*3+2,y*3+2,z*3+2,2)-.5)*.35,wy=y+(Fbm(s+223,x*3+5,y*3+5,z*3+5,2)-.5)*.35,wz=z+(Fbm(s+227,x*3+8,y*3+8,z*3+8,2)-.5)*.35;
                int best=0;double bd=double.MinValue;for(int k=0;k<plates;k++){double d=wx*cx[k]+wy*cy[k]+wz*cz[k];if(d>bd){bd=d;best=k;}}plate[i]=best;}
            var conv=new double[n];var apart=new double[n];
            for(int i=0;i<n;i++)foreach(int j in SphereNeighbours(i))
            {
                int a=plate[i],b=plate[j];if(a==b)continue;
                // Surface velocity of each plate at the boundary (rotation axis x position); approaching when the relative
                // velocity points from this tile towards the neighbour.
                double vax=ay[a]*pz[i]-az[a]*py[i],vay=az[a]*px[i]-ax[a]*pz[i],vaz=ax[a]*py[i]-ay[a]*px[i];
                double vbx=ay[b]*pz[j]-az[b]*py[j],vby=az[b]*px[j]-ax[b]*pz[j],vbz=ax[b]*py[j]-ay[b]*px[j];
                double dx=px[j]-px[i],dy=py[j]-py[i],dz=pz[j]-pz[i],len=Math.Sqrt(dx*dx+dy*dy+dz*dz)+1e-9;
                double c=((vax-vbx)*dx+(vay-vby)*dy+(vaz-vbz)*dz)/len;
                if(c>0)conv[i]=Math.Max(conv[i],Math.Min(1,c));else apart[i]=Math.Max(apart[i],Math.Min(1,-c));
            }
            // Spread outwards from the boundary: full strength on it, then .7, .45, .25.
            var uplift=new double[n];rift=new double[n];double[] fall={1,.7,.45,.25};
            for(int i=0;i<n;i++){uplift[i]=conv[i];rift[i]=apart[i];}
            for(int ring=1;ring<fall.Length;ring++){double m=fall[ring]/fall[ring-1];var nextU=(double[])uplift.Clone();var nextR=(double[])rift.Clone();
                for(int i=0;i<n;i++)foreach(int j in SphereNeighbours(i)){nextU[i]=Math.Max(nextU[i],uplift[j]*m);nextR[i]=Math.Max(nextR[i],rift[j]*m*.8);}
                uplift=nextU;rift=nextR;}
            return uplift;
        }
        // The relief noise is laid on the globe, so a range covers the same share of any map and dominates a small one (owner
        // request: massive ranges less likely on small maps). A connected range keeps at most about sqrt(tiles)/4 peaks (8 on
        // Tiny, more on bigger maps), the highest; the lower rest become hills (tundra where it is too cold for hills).
        void CapMountainRanges(double[] hn,int[] tb)
        {
            int n=tiles.Length,cap=Math.Max(4,(int)Math.Round(Math.Sqrt(n)/4));var seen=new bool[n];
            for(int i=0;i<n;i++)
            {
                if(seen[i]||!IsMountain(tiles[i].terrain))continue;
                var range=new List<int>();var stack=new Stack<int>();stack.Push(i);seen[i]=true;
                while(stack.Count>0){int k=stack.Pop();range.Add(k);foreach(int j in SphereNeighbours(k))if(!seen[j]&&IsMountain(tiles[j].terrain)){seen[j]=true;stack.Push(j);}}
                if(range.Count<=cap)continue;
                foreach(int k in range.OrderByDescending(k=>hn[k]).ThenBy(k=>k).Skip(cap))tiles[k].terrain=tb[k]>0?2:8;
            }
        }
        static bool IsMountain(int terrain){return terrain==5||terrain==15;}
        double WastelandNoise(int s,int x,int y){double px,py,pz;SpherePoint(x,y,out px,out py,out pz);return Fbm(s+9,px*6+2,py*6+2,pz*6+2,2);}

        void MakeLakes(int s,bool[] land,int[] dist,double[] hn,int[] tb)
        {
            int n=tiles.Length,wanted=Math.Max(2,land.Count(l=>l)/170);
            var candidates=new List<int>();
            for(int i=0;i<n;i++)if(land[i]&&dist[i]>=3&&hn[i]<.3&&!Barren(tiles[i].terrain)&&tb[i]>=2)candidates.Add(i);
            candidates.Sort((a,b)=>{int c=Lattice(s+43,a%Width,a/Width,3).CompareTo(Lattice(s+43,b%Width,b/Width,3));return c!=0?c:a.CompareTo(b);});
            int made=0;
            foreach(int i in candidates)
            {
                if(made>=wanted)break;
                if(SphereNeighbours(i).Any(j=>tiles[j].terrain==6))continue;
                tiles[i].terrain=6;made++;
                if(Lattice(s+47,i%Width,i/Width,4)>.5)
                {
                    foreach(int j in SphereNeighbours(i)){if(land[j]&&!Barren(tiles[j].terrain)&&tiles[j].terrain!=7){tiles[j].terrain=6;break;}}
                }
            }
        }

        // Rivers start on high ground in wet country and run downhill until they meet the sea, a lake or another river.
        void MakeRivers(int s,bool[] land,double[] hn,int[] mb)
        {
            int n=tiles.Length,wanted=Math.Max(2,land.Count(l=>l)/90);
            var sources=new List<int>();
            for(int i=0;i<n;i++)if(land[i]&&hn[i]>.4&&hn[i]<.9&&mb[i]>=3&&tiles[i].terrain!=15)sources.Add(i);
            sources.Sort((a,b)=>{int c=Lattice(s+61,a%Width,a/Width,5).CompareTo(Lattice(s+61,b%Width,b/Width,5));return c!=0?c:a.CompareTo(b);});
            int made=0;
            foreach(int source in sources)
            {
                if(made>=wanted)break;
                if(SphereNeighbours(source).Any(j=>tiles[j].terrain==7))continue;
                var path=new List<int>();int current=source;bool reached=false;
                for(int step=0;step<80;step++)
                {
                    int next=-1;double best=elevation[current];
                    foreach(int j in SphereNeighbours(current))if(elevation[j]<best){best=elevation[j];next=j;}
                    if(next<0)break;
                    int tt=tiles[next].terrain;
                    if(tt==4||tt==12||tt==14||tt==6||tt==7){reached=true;break;}
                    path.Add(next);current=next;
                }
                if(!reached||path.Count<4)continue;
                foreach(int i in path){int tt=tiles[i].terrain;if(tt==5||tt==15||tt==17)continue;tiles[i].terrain=7;}
                made++;
            }
        }

        // Landmass labelling on tiles a unit can walk across (8 directions, wrapping).
        List<List<int>> Landmasses()
        {
            int n=tiles.Length;var seen=new bool[n];var result=new List<List<int>>();
            for(int i=0;i<n;i++)
            {
                if(seen[i]||Barren(tiles[i].terrain))continue;
                var group=new List<int>();var q=new Queue<int>();q.Enqueue(i);seen[i]=true;
                while(q.Count>0)
                {
                    int c=q.Dequeue();group.Add(c);
                    foreach(int j in SphereNeighbours(c))if(!seen[j]&&!Barren(tiles[j].terrain)){seen[j]=true;q.Enqueue(j);}
                }
                result.Add(group);
            }
            return result;
        }

        int SiteScore(int i)
        {
            int score=0;var t=tiles[i];
            if(Barren(t.terrain)||t.terrain==2||t.terrain==17)return -1;
            for(int dy=-3;dy<=3;dy++)for(int dx=-3;dx<=3;dx++)
            {
                var n=Step(t.x,t.y,dx,dy);if(n==null)continue;
                int r=Math.Max(Math.Abs(dx),Math.Abs(dy));int w=r<=1?3:r==2?2:1;
                switch(n.terrain)
                {
                    case 0:case 16:score+=3*w;break;
                    case 1:case 20:case 7:score+=2*w;break;
                    case 2:case 11:case 13:case 18:case 19:score+=1*w;break;
                    case 4:case 12:score+=1*w;break;
                    case 3:case 8:case 9:case 10:score-=1*w;break;
                    case 5:case 15:case 14:score-=2*w;break;
                }
            }
            if(climateTemperature!=null&&(climateTemperature[i]<3||climateTemperature[i]>6))score-=40;
            return score;
        }

        // Picks one well-spaced start site per civilisation on the largest landmass. Returns false if the world is unsuitable.
        bool ChooseSphereStarts(int[] sx,int[] sy,bool relaxed)
        {
            var masses=Landmasses();if(masses.Count==0)return false;
            var main=masses.OrderByDescending(m=>m.Count).First();
            if(!relaxed&&main.Count<tiles.Length*8/100)return false;
            int minGap=sx.Length<=3?Math.Max(8,Width/9):Math.Max(5,Math.Min(Width/9,(int)(Math.Sqrt(main.Count/(double)sx.Length)*.8)));
            var picked=new List<int>();
            for(int k=0;k<sx.Length;k++)
            {
                int bestTile=-1,bestScore=int.MinValue;
                foreach(int i in main)
                {
                    int y=i/Width;if(y<Height*2/10||y>=Height*8/10)continue;
                    if(picked.Any(p=>Distance(i%Width,y,p%Width,p/Width)<minGap))continue;
                    int sc=SiteScore(i);
                    if(sc>bestScore||(sc==bestScore&&i<bestTile)){bestScore=sc;bestTile=i;}
                }
                if(bestTile<0||bestScore<20&&!relaxed)
                {
                    if(!relaxed)return false;
                    if(bestTile<0)
                    {
                        foreach(int i in main)if(!picked.Contains(i)&&picked.All(p=>Distance(i%Width,i/Width,p%Width,p/Width)>=3)){bestTile=i;break;}
                        if(bestTile<0)return false;
                    }
                }
                picked.Add(bestTile);
            }
            for(int k=0;k<sx.Length;k++){sx[k]=picked[k]%Width;sy[k]=picked[k]/Width;}
            return true;
        }

        void GenerateSphere(int[] sx,int[] sy)
        {
            for(int attempt=0;attempt<8;attempt++)
            {
                int s=seed*7919+attempt*104729+12345;
                BuildClimateWorld(s);if(mapType==4)SmallSeasToLakes(Math.Max(6,tiles.Length/60));
                if(ChooseSphereStarts(sx,sy,attempt==7))return;
            }
        }
    }
}
