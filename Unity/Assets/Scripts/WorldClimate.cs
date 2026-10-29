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
                var t=At(x+dx,y+dy);if(t==null)continue;
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
        void SpherePoint(int x,int y,out double px,out double py,out double pz)
        {
            double lat=Math.PI*(0.5-(y+.5)/Height),lon=2*Math.PI*(x+.5)/Width,cl=Math.Cos(lat);
            px=cl*Math.Cos(lon);py=Math.Sin(lat);pz=cl*Math.Sin(lon);
        }

        // Fills tiles (already allocated with coordinates) with terrain for one seed variant.
        void BuildClimateWorld(int s)
        {
            int W=Width,H=Height,n=W*H;
            var elev=new double[n];var ridge=new double[n];var lat=new double[H];
            for(int y=0;y<H;y++)
            {
                lat[y]=Math.PI*(0.5-(y+.5)/H);
                for(int x=0;x<W;x++)
                {
                    double lon=2*Math.PI*(x+.5)/W,cl=Math.Cos(lat[y]);
                    double px=cl*Math.Cos(lon),py=Math.Sin(lat[y]),pz=cl*Math.Sin(lon);
                    double c=Fbm(s,px*1.4+7,py*1.4+7,pz*1.4+7,2),d=Fbm(s+17,px*4+3,py*4+3,pz*4+3,4);
                    elev[y*W+x]=c*.62+d*.38-.10*Math.Pow(Math.Abs(Math.Sin(lat[y])),4);
                    ridge[y*W+x]=1-Math.Abs(2*Fbm(s+31,px*5+1,py*5+1,pz*5+1,3)-1);
                }
            }
            var sorted=(double[])elev.Clone();Array.Sort(sorted);
            double sea=sorted[(int)(n*.62)],top=sorted[n-1];
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
                if(h>.82||(h>.58&&ridge[i]>.72))tile.terrain=t2<=1?15:5;
                else if(h>.5||(h>.36&&ridge[i]>.78))tile.terrain=t2>0?2:8;
                else
                {
                    int b=BiomeFor(t2,m2);
                    if(b==3&&t2>=4&&m2==0&&WastelandNoise(s,i%W,i/W)>.62)b=10;
                    if(h<.06&&m2>=6&&t2>=2)b=9;
                    tile.terrain=b;
                }
            }
            // Volcanic ground on warm high hills, with a deterministic hash choosing a few clusters.
            for(int i=0;i<n;i++){var t=tiles[i];if(t.terrain==2&&tb[i]>=3&&hn[i]>.6&&Lattice(s+5,i%W/3,i/W/3,0)>.9)t.terrain=17;}
            // Coasts: low ground beside the sea becomes rocky shoreline where hills are near, mangrove in the wet tropics.
            for(int i=0;i<n;i++)
            {
                var t=tiles[i];if(IsImpassable(t.terrain))continue;
                bool coast=false,highNear=false;
                foreach(int j in SphereNeighbours(i)){int tj=tiles[j].terrain;if(tj==4||tj==12||tj==14)coast=true;if(tj==2||tj==5||tj==17)highNear=true;}
                if(!coast)continue;
                if(t.terrain==11||(tb[i]>=6&&mb[i]>=5&&t.terrain!=2))t.terrain=19;
                else if(highNear||tb[i]<=2||Lattice(s+13,i%W,i/W,1)<.3)t.terrain=18;
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

        double WastelandNoise(int s,int x,int y){double px,py,pz;SpherePoint(x,y,out px,out py,out pz);return Fbm(s+9,px*6+2,py*6+2,pz*6+2,2);}

        void MakeLakes(int s,bool[] land,int[] dist,double[] hn,int[] tb)
        {
            int n=tiles.Length,wanted=Math.Max(2,land.Count(l=>l)/170);
            var candidates=new List<int>();
            for(int i=0;i<n;i++)if(land[i]&&dist[i]>=3&&hn[i]<.3&&!IsImpassable(tiles[i].terrain)&&tb[i]>=2)candidates.Add(i);
            candidates.Sort((a,b)=>{int c=Lattice(s+43,a%Width,a/Width,3).CompareTo(Lattice(s+43,b%Width,b/Width,3));return c!=0?c:a.CompareTo(b);});
            int made=0;
            foreach(int i in candidates)
            {
                if(made>=wanted)break;
                if(SphereNeighbours(i).Any(j=>tiles[j].terrain==6))continue;
                tiles[i].terrain=6;made++;
                if(Lattice(s+47,i%Width,i/Width,4)>.5)
                {
                    foreach(int j in SphereNeighbours(i)){if(land[j]&&!IsImpassable(tiles[j].terrain)&&tiles[j].terrain!=7){tiles[j].terrain=6;break;}}
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
                if(seen[i]||IsImpassable(tiles[i].terrain))continue;
                var group=new List<int>();var q=new Queue<int>();q.Enqueue(i);seen[i]=true;
                while(q.Count>0)
                {
                    int c=q.Dequeue();group.Add(c);
                    foreach(int j in SphereNeighbours(c))if(!seen[j]&&!IsImpassable(tiles[j].terrain)){seen[j]=true;q.Enqueue(j);}
                }
                result.Add(group);
            }
            return result;
        }

        int SiteScore(int i)
        {
            int score=0;var t=tiles[i];
            if(IsImpassable(t.terrain)||t.terrain==2||t.terrain==17)return -1;
            for(int dy=-3;dy<=3;dy++)for(int dx=-3;dx<=3;dx++)
            {
                var n=At(t.x+dx,t.y+dy);if(n==null)continue;
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
                BuildClimateWorld(s);
                if(ChooseSphereStarts(sx,sy,attempt==7))return;
            }
        }
    }
}
