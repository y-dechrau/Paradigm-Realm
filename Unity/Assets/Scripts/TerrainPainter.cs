using System;

namespace ParadigmRealm
{
    // Pure CPU rasteriser. Coordinates are world pixels, so adjacent tiles share
    // exactly the same field, noise and texture phase. No live World access here.
    public struct TerrainInk
    {
        public float r,g,b;
        public TerrainInk(float r,float g,float b){this.r=r;this.g=g;this.b=b;}
        public static TerrainInk Mix(TerrainInk a,TerrainInk b,float t){return new TerrainInk(a.r+(b.r-a.r)*t,a.g+(b.g-a.g)*t,a.b+(b.b-a.b)*t);}
    }
    public static class TerrainPainter
    {
        public const int Size=64;
        public static bool Water(int t){return t==4||t==6||t==12;}
        static float Clamp(float t){return Math.Max(0,Math.Min(1,t));}
        static float Smooth(float t){t=Clamp(t);return t*t*(3-2*t);}
        static float Hash(int x,int y,int seed){unchecked{uint n=(uint)(x*374761393+y*668265263+seed*1442695041);n=(n^(n>>13))*1274126177;return (n^(n>>16))%65536/65535f;}}
        static float Noise(float x,float y,int seed){int ix=(int)Math.Floor(x),iy=(int)Math.Floor(y);float u=Smooth(x-ix),v=Smooth(y-iy);float a=Hash(ix,iy,seed),b=Hash(ix+1,iy,seed),c=Hash(ix,iy+1,seed),d=Hash(ix+1,iy+1,seed);return (a+(b-a)*u)*(1-v)+(c+(d-c)*u)*v;}
        static float Segment(float x,float y,float ax,float ay,float bx,float by){float dx=bx-ax,dy=by-ay;float t=Clamp(((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy));dx=x-ax-t*dx;dy=y-ay-t*dy;return (float)Math.Sqrt(dx*dx+dy*dy);}
        static float Level(int t){return t==5||t==15?1:t==2||t==17?.45f:0;}
        static int Ground(int t){return t==7?0:t;}
        // Palette is sampled from the project's supplied textures, never generated art.
        public static int GroundOf(int terrainId){return Ground(terrainId);}
        // Flat ground (not hills or mountains) whose whole neighbourhood is the same type looks identical on every tile.
        public static bool Raised(int ground){return Level(ground)>0;}
        static TerrainInk Quantise(TerrainInk colour){return new TerrainInk((float)Math.Round(Clamp(colour.r)*31)/31,(float)Math.Round(Clamp(colour.g)*31)/31,(float)Math.Round(Clamp(colour.b)*31)/31);}
        // Same result as Pixel when the tile and all eight neighbours are one ground type with no river, without the blending work.
        public static TerrainInk PixelUniform(float wx,float wy,int seed,int ground,Func<int,float,float,TerrainInk> sample)
        {
            TerrainInk colour=sample(ground,wx,wy);
            if(Level(ground)>0)
            {
                float light=(Noise(wx*3,wy*3,seed+51)-.5f)*.06f;
                colour=TerrainInk.Mix(colour,light>=0?new TerrainInk(.85f,.83f,.65f):new TerrainInk(.14f,.21f,.22f),Math.Abs(light));
            }
            return Quantise(colour);
        }
        public static TerrainInk Pixel(float wx,float wy,int seed,Func<int,int,int> terrain,Func<int,float,float,TerrainInk> sample){return Pixel(wx,wy,seed,terrain,sample,true);}
        // riversPossible=false lets the caller skip the river search when no river tile is within one tile.
        public static TerrainInk Pixel(float wx,float wy,int seed,Func<int,int,int> terrain,Func<int,float,float,TerrainInk> sample,bool riversPossible)
        {
            // A shared warped coordinate field produces organic boundaries without
            // choosing a different edge shape on either side of a tile seam.
            float px=wx+(Noise(wx*2,wy*2,seed)-.5f)*.25f;
            float py=wy+(Noise(wx*2+17,wy*2-11,seed+19)-.5f)*.25f;
            int ix=(int)Math.Floor(px-.5f),iy=(int)Math.Floor(py-.5f);
            float u=Smooth(px-.5f-ix),v=Smooth(py-.5f-iy);
            int a=Ground(terrain(ix,iy)),b=Ground(terrain(ix+1,iy)),c=Ground(terrain(ix,iy+1)),d=Ground(terrain(ix+1,iy+1));
            float wa=(1-u)*(1-v),wb=u*(1-v),wc=(1-u)*v,wd=u*v;
            // Aggregate repeated types; otherwise a corner with three grass samples
            // would wrongly lose to its single desert sample.
            float sa=wa+(b==a?wb:0)+(c==a?wc:0)+(d==a?wd:0);
            float sb=wb+(a==b?wa:0)+(c==b?wc:0)+(d==b?wd:0);
            float sc=wc+(a==c?wa:0)+(b==c?wb:0)+(d==c?wd:0);
            float sd=wd+(a==d?wa:0)+(b==d?wb:0)+(c==d?wc:0);
            int winner=a;float best=sa;
            if(sb>best){winner=b;best=sb;}if(sc>best){winner=c;best=sc;}if(sd>best){winner=d;best=sd;}
            float water=(Water(a)?wa:0)+(Water(b)?wb:0)+(Water(c)?wc:0)+(Water(d)?wd:0);
            int gx=(int)Math.Floor(wx*Size),gy=(int)Math.Floor(wy*Size);
            // Small ordered dither only within the transition band; no soft alpha edge.
            float grain=Hash(gx,gy,seed+73);
            TerrainInk colour=sample(winner,wx,wy);
            if(water>.30f&&water<.70f){
                bool wet=water>.5f;
                TerrainInk bank=winner==18||winner==15||winner==14?new TerrainInk(.49f,.54f,.53f):new TerrainInk(.68f,.64f,.40f);
                TerrainInk shallow=new TerrainInk(.18f,.48f,.49f);
                colour=TerrainInk.Mix(colour,wet?shallow:bank,wet?.70f:.64f);
                if(Math.Abs(water-.5f)<.035f&&grain>.4f)colour=new TerrainInk(.61f,.72f,.62f);
            }else if(best<.88f){
                TerrainInk mixed=TerrainInk.Mix(TerrainInk.Mix(sample(a,wx,wy),sample(b,wx,wy),u),TerrainInk.Mix(sample(c,wx,wy),sample(d,wx,wy),u),v);
                // Palette steps and stippling retain pixel clusters in mixed biomes.
                float amount=grain<.35f?.25f:.65f;
                colour=TerrainInk.Mix(colour,mixed,amount);
                bool arid=a==3||b==3||c==3||d==3||a==20||b==20||c==20||d==20;
                if(arid&&water<.01f)colour=TerrainInk.Mix(colour,new TerrainInk(.52f,.51f,.28f),.22f);
            }
            float elevation=Level(a)*wa+Level(b)*wb+Level(c)*wc+Level(d)*wd;
            float light=(Level(a)-Level(d))*.08f+(Noise(wx*3,wy*3,seed+51)-.5f)*.06f;
            if(elevation>0)colour=TerrainInk.Mix(colour,light>=0?new TerrainInk(.85f,.83f,.65f):new TerrainInk(.14f,.21f,.22f),Math.Abs(light));
            // River tiles retain their gameplay identity, but their water is drawn
            // as connected centre-to-centre channels over grass, including mouths.
            float river=10;int tx=(int)Math.Floor(wx),ty=(int)Math.Floor(wy);
            // A continuous meander field is shared across boundaries too.
            if(riversPossible){
            float rx=wx+(Noise(wx*2.3f,wy*2.3f,seed+113)-.5f)*.13f;
            float ry=wy+(Noise(wx*2.3f+7,wy*2.3f,seed+131)-.5f)*.13f;
            for(int yy=ty-1;yy<=ty+1;yy++)for(int xx=tx-1;xx<=tx+1;xx++){
                if(terrain(xx,yy)!=7)continue;
                int count=0;float ax=0,ay=0,bx=0,by=0;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){
                    if(dx==0&&dy==0)continue;int n=terrain(xx+dx,yy+dy);
                    if(n!=7&&!Water(n))continue;
                    if(dx!=0&&dy!=0&&(Water(n)||terrain(xx+dx,yy)==7||terrain(xx,yy+dy)==7))continue;
                    float ex=xx+.5f+dx*.5f,ey=yy+.5f+dy*.5f;
                    if(count==0){ax=ex;ay=ey;}if(count==1){bx=ex;by=ey;}count++;
                }
                if(count==2){
                    // Quadratic paths round right-angle bends and keep common edge endpoints.
                    float lastX=ax,lastY=ay;
                    for(int step=1;step<=8;step++){float t=step/8f,q=1-t;float ex=q*q*ax+2*q*t*(xx+.5f)+t*t*bx,ey=q*q*ay+2*q*t*(yy+.5f)+t*t*by;
                        river=Math.Min(river,Segment(rx,ry,lastX,lastY,ex,ey));lastX=ex;lastY=ey;}
                }else{
                    river=Math.Min(river,(float)Math.Sqrt((rx-xx-.5f)*(rx-xx-.5f)+(ry-yy-.5f)*(ry-yy-.5f)));
                    for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){
                        if(dx==0&&dy==0)continue;int n=terrain(xx+dx,yy+dy);
                        if(n!=7&&!Water(n))continue;
                        if(dx!=0&&dy!=0&&(Water(n)||terrain(xx+dx,yy)==7||terrain(xx,yy+dy)==7))continue;
                        river=Math.Min(river,Segment(rx,ry,xx+.5f,yy+.5f,xx+.5f+dx*.5f,yy+.5f+dy*.5f));
                    }
                }
            }
            }
            float channel=.085f+(Noise(wx*4,wy*4,seed+97)-.5f)*.035f;
            if(river<channel+.045f&&water<.55f){
                colour=river<channel?TerrainInk.Mix(sample(4,wx,wy),new TerrainInk(.13f,.37f,.43f),.64f):TerrainInk.Mix(colour,new TerrainInk(.43f,.49f,.29f),.55f);
                if(river<channel&&river>channel-.018f)colour=new TerrainInk(.24f,.52f,.53f);
                if(river<channel-.03f&&grain>.97f)colour=new TerrainInk(.36f,.59f,.57f);
            }
            return new TerrainInk((float)Math.Round(Clamp(colour.r)*31)/31,(float)Math.Round(Clamp(colour.g)*31)/31,(float)Math.Round(Clamp(colour.b)*31)/31);
        }
    }
}
