using System;
using System.Collections.Generic;

namespace ParadigmRealm
{
    // Procedural 16-bit style art for the diamond (isometric) map. Pure C#, no Unity types, so the harness can run it.
    // Colours are packed 0xAARRGGBB. Light comes from the top left; every ramp runs dark to light; shapes get a dark outline.
    public sealed class PixelCanvas
    {
        public readonly int w,h;public readonly uint[] p;
        public PixelCanvas(int w,int h){this.w=w;this.h=h;p=new uint[w*h];}
        public uint Get(int x,int y){return x>=0&&y>=0&&x<w&&y<h?p[y*w+x]:0u;}
        public void Set(int x,int y,uint c){if(x>=0&&y>=0&&x<w&&y<h)p[y*w+x]=c;}
        public void Over(int x,int y,uint c){if(x>=0&&y>=0&&x<w&&y<h&&(c>>24)>0)p[y*w+x]=c;}
    }

    public static class PixelArt
    {
        public const int TW=64,TH=32,SW=64,SH=64,AX=32,AY=40,CW=64,CH=48,Elev=8;

        // ---------- colour and noise helpers ----------
        public static uint Rgb(int r,int g,int b,int a=255){return (uint)((a<<24)|(r<<16)|(g<<8)|b);}
        static int R(uint c){return (int)((c>>16)&255);}
        static int G(uint c){return (int)((c>>8)&255);}
        static int B(uint c){return (int)(c&255);}
        static int Clamp255(double v){return v<0?0:v>255?255:(int)v;}
        public static uint Shade(uint c,double f){return Rgb(Clamp255(R(c)*f),Clamp255(G(c)*f),Clamp255(B(c)*f));}
        static uint MixC(uint a,uint b,double t){return Rgb((int)(R(a)+(R(b)-R(a))*t),(int)(G(a)+(G(b)-G(a))*t),(int)(B(a)+(B(b)-B(a))*t));}
        static double Hsh(int x,int y,int s)
        {
            unchecked
            {
                uint h=(uint)(x*374761393+y*668265263+s*1442695041);
                h=(h^(h>>13))*1274126177u;h^=h>>16;
                return (h&0xFFFF)/65535.0;
            }
        }
        static double Smooth(double t){return t*t*(3-2*t);}
        static double VNoise(double x,double y,int s)
        {
            int ix=(int)Math.Floor(x),iy=(int)Math.Floor(y);double fx=Smooth(x-ix),fy=Smooth(y-iy);
            double a=Hsh(ix,iy,s),b=Hsh(ix+1,iy,s),c=Hsh(ix,iy+1,s),d=Hsh(ix+1,iy+1,s);
            return (a+(b-a)*fx)*(1-fy)+(c+(d-c)*fx)*fy;
        }
        static readonly int[,] Bayer={{0,8,2,10},{12,4,14,6},{3,11,1,9},{15,7,13,5}};
        static double Bay(int x,int y){return (Bayer[y&3,x&3]+.5)/16.0;}
        public static bool Inside(int x,int y){return Math.Abs(x+.5-32)/32.0+Math.Abs(y+.5-16)/16.0<=1.0;}
        static uint RampPick(uint[] r,double v){int i=(int)(Math.Max(0,Math.Min(.999,v))*r.Length);return r[i];}
        static uint[] Ramp(int a,int b,int c,int d,int e,int f,int g,int h,int i,int j,int k,int l,int m,int n,int o)
        {return new[]{Rgb(a,b,c),Rgb(d,e,f),Rgb(g,h,i),Rgb(j,k,l),Rgb(m,n,o)};}

        // ---------- palettes ----------
        static readonly uint[] Grass=Ramp(38,84,52,54,112,58,80,142,64,112,172,74,148,198,92);
        static readonly uint[] ForestGround=Ramp(24,58,40,34,78,46,46,98,52,62,118,58,84,138,66);
        static readonly uint[] Sand=Ramp(168,132,76,196,162,98,222,190,124,238,212,152,248,228,178);
        static readonly uint[] Water=Ramp(18,48,96,26,72,128,36,98,152,52,126,172,104,172,204);
        static readonly uint[] Lake=Ramp(20,70,100,28,96,124,40,122,142,66,150,160,116,186,182);
        static readonly uint[] Rock=Ramp(66,62,70,92,86,92,120,112,112,150,140,130,180,170,156);
        static readonly uint[] Mount=Ramp(84,78,64,110,102,80,134,124,96,158,146,112,184,170,130);
        static readonly uint[] Tundra=Ramp(96,110,100,122,136,122,150,162,146,184,194,180,222,230,224);
        static readonly uint[] Bog=Ramp(40,52,36,56,70,42,76,88,50,98,108,60,60,92,88);
        static readonly uint[] Waste=Ramp(70,58,50,94,80,66,120,104,84,148,130,104,176,156,126);
        static readonly uint[] Jungle=Ramp(16,52,36,24,76,44,34,100,52,50,126,56,76,150,66);
        static readonly uint[] SnowForest=Ramp(120,140,150,150,170,176,182,198,200,210,222,224,240,246,246);
        static readonly uint[] Ice=Ramp(150,184,206,176,204,222,204,226,238,230,242,248,250,254,255);
        static readonly uint[] Glacier=Ramp(110,150,184,140,178,208,176,208,230,210,230,244,240,250,255);
        static readonly uint[] Savanna=Ramp(150,126,56,178,152,66,202,176,80,222,198,104,238,218,134);
        static readonly uint[] Volcanic=Ramp(30,26,30,46,38,40,64,50,46,88,64,50,190,70,30);
        static readonly uint[] Mangrove=Ramp(30,66,52,42,88,60,60,110,64,84,132,72,60,120,120);
        static readonly uint[] Steppe=Ramp(122,128,64,148,154,76,172,176,92,196,198,112,218,214,138);
        static readonly uint[][] TerrainRamp={Grass,ForestGround,Grass,Sand,Water,Mount,Lake,Grass,Tundra,Bog,Waste,Jungle,Water,SnowForest,Ice,Glacier,Savanna,Volcanic,Rock,Mangrove,Steppe};
        static readonly int[] RampGroup={0,1,0,2,3,4,5,0,6,7,8,9,3,10,11,12,13,14,15,16,17}; // terrains sharing a group blend into each other without a seam
        public static bool IsWater(int t){return t==4||t==6||t==12;}
        public static int Group(int t){return RampGroup[t];}

        // ---------- ground textures (64 x 32 diamonds) ----------
        static uint BasePixel(int t,int x,int y,int variant,int frame)
        {
            var r=TerrainRamp[t];int s=t*131+variant*17;
            if(t==4||t==12||t==6)
            {
                double n=.55*VNoise(x/6.0,y/3.0,s)+.45*Hsh(x>>1,y,s+3);
                double v=.18+n*.52+(Bay(x,y)-.5)*.2;
                uint c=RampPick(r,v);
                int wave=(x*3+y*7+frame*10)%40;
                if(wave<3&&Hsh(x,y>>1,s+9)>.55)c=r[4];
                if((x*5+y*3+frame*14)%131==0)c=Rgb(235,246,250);
                return c;
            }
            double q=.5*VNoise(x/5.0,y/2.5,s)+.3*VNoise(x/2.2,y/1.2,s+1)+.2*Hsh(x,y,s+2);
            double val=q+(Bay(x,y)-.5)*.34;
            if(t==3)val=.55*val+.45*(.5+.5*Math.Sin((x+y*2)*.45+VNoise(x/8.0,y/4.0,s)*4));
            uint col=RampPick(r,val);
            double h=Hsh(x,y,s+5);
            if((t==0||t==2||t==7||t==16||t==20||t==1||t==11||t==19||t==9)&&h>.965)col=h>.985?r[4]:r[0];
            if(t==17&&Hsh(x>>1,y,s+7)>.93&&VNoise(x/3.0,y/2.0,s+8)>.55)col=Rgb(236,120,40);
            if(t==10){double w=VNoise(x/6.0,y/3.0,s+11);if(w>.5&&Math.Abs(w-.55)<.02)col=Shade(col,.6);}
            if((t==14||t==15||t==13||t==8)&&h>.97)col=Rgb(255,255,255);
            if(t==9&&VNoise(x/5.0,y/3.0,s+12)>.68)col=MixC(Bog[4],Water[2],.4);
            return col;
        }
        static readonly Dictionary<int,PixelCanvas> baseCache=new Dictionary<int,PixelCanvas>();
        public static PixelCanvas BaseTex(int t,int variant,int frame)
        {
            int key=t*1000+variant*10+frame;PixelCanvas cv;
            if(baseCache.TryGetValue(key,out cv))return cv;
            cv=new PixelCanvas(TW,TH);
            for(int y=0;y<TH;y++)for(int x=0;x<TW;x++)if(Inside(x,y))cv.Set(x,y,BasePixel(t,x,y,variant,frame));
            baseCache[key]=cv;return cv;
        }

        // ---------- raised sprites (64 x 64, anchored at the tile centre AX,AY) ----------
        static void Outline(PixelCanvas cv,uint col)
        {
            var src=(uint[])cv.p.Clone();int w=cv.w,h=cv.h;
            int[] dx={1,-1,0,0},dy={0,0,1,-1};
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                if((src[y*w+x]>>24)!=0)continue;
                for(int k=0;k<4;k++){int nx=x+dx[k],ny=y+dy[k];if(nx>=0&&ny>=0&&nx<w&&ny<h&&(src[ny*w+nx]>>24)>0){cv.p[y*w+x]=col;break;}}
            }
        }
        static readonly uint Ink=Rgb(18,22,26);
        static uint Lit(uint[] r,double nx,double ny,int x,int y,double bias)
        {
            double v=.5+.5*(-.55*nx-.8*ny)+bias+(Bay(x,y)-.5)*.28;return RampPick(r,v);
        }
        static void Disk(PixelCanvas cv,double cx,double cy,double rx,double ry,uint[] r,double bias)
        {
            for(int y=(int)(cy-ry)-1;y<(int)(cy+ry)+2;y++)for(int x=(int)(cx-rx)-1;x<(int)(cx+rx)+2;x++)
            {
                double nx=(x+.5-cx)/rx,ny=(y+.5-cy)/ry;
                if(nx*nx+ny*ny<=1)cv.Over(x,y,Lit(r,nx,ny,x,y,bias));
            }
        }
        static bool PIn(double[][] pts,double px,double py)
        {
            bool c=false;int n=pts.Length,j=n-1;
            for(int i=0;i<n;i++)
            {
                double xi=pts[i][0],yi=pts[i][1],xj=pts[j][0],yj=pts[j][1];
                if((yi>py)!=(yj>py)&&px<(xj-xi)*(py-yi)/(yj-yi+1e-9)+xi)c=!c;
                j=i;
            }
            return c;
        }
        static void Poly(PixelCanvas cv,double[][] pts,Func<int,int,uint> fn)
        {
            double minX=1e9,maxX=-1e9,minY=1e9,maxY=-1e9;
            foreach(var p in pts){minX=Math.Min(minX,p[0]);maxX=Math.Max(maxX,p[0]);minY=Math.Min(minY,p[1]);maxY=Math.Max(maxY,p[1]);}
            for(int y=(int)minY;y<=(int)maxY;y++)for(int x=(int)minX;x<=(int)maxX;x++)if(PIn(pts,x+.5,y+.5))cv.Over(x,y,fn(x,y));
        }
        static double[] P(double x,double y){return new[]{x,y};}
        static void RectFill(PixelCanvas cv,int x0,int y0,int x1,int y1,uint c){for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)cv.Over(x,y,c);}
        static List<double[]> Spots(int seed,int n,double rx,double ry,double minSep)
        {
            var pts=new List<double[]>();int tries=0;
            while(pts.Count<n&&tries<200)
            {
                tries++;
                double u=Hsh(tries,seed,1)*2-1,v=Hsh(tries,seed,2)*2-1;
                if(Math.Abs(u)+Math.Abs(v)>.85)continue;
                double x=AX+u*rx,y=AY+v*ry;bool ok=true;
                foreach(var q in pts)if(!(Math.Abs(x-q[0])+Math.Abs(y-q[1])*2>minSep)){ok=false;break;}
                if(ok)pts.Add(P(x,y));
            }
            pts.Sort((a,b)=>a[1].CompareTo(b[1]));return pts;
        }
        static void Broadleaf(PixelCanvas cv,double x,double y,uint[] r,double sz)
        {
            RectFill(cv,(int)x-1,(int)y-1,(int)x+1,(int)y+3,Rgb(70,46,30));
            Disk(cv,x,y-sz*.9,sz,sz*.85,r,0);
            Disk(cv,x-sz*.35,y-sz*1.3,sz*.55,sz*.45,r,.18);
        }
        static void Pine(PixelCanvas cv,double x,double y,uint[] r,bool snow,double sz)
        {
            RectFill(cv,(int)x,(int)y-1,(int)x+1,(int)y+2,Rgb(60,40,28));
            double[] widths={7*sz,5.5*sz,4*sz},tops={-9*sz,-15*sz,-20*sz};
            for(int k=0;k<3;k++)
            {
                double w=widths[k],top=tops[k],bas=y-(k*5.5*sz);
                var pts=new[]{P(x-w,bas+1),P(x+w,bas+1),P(x,bas+top*.55-4*sz)};
                double cx=x,cw=w;
                Poly(cv,pts,(X,Y)=>Lit(r,(X-cx)/cw,.3,X,Y,-.05));
                if(snow)Poly(cv,new[]{P(x-w*.55,bas-3*sz),P(x+w*.3,bas-3*sz),P(x,bas+top*.55-4*sz)},(X,Y)=>Rgb(236,244,248));
            }
        }
        static void Palm(PixelCanvas cv,double x,double y,uint[] r)
        {
            for(int k=0;k<3;k++)RectFill(cv,(int)(x+k),(int)(y-k*4)-6,(int)(x+k)+2,(int)(y-k*4),Rgb(112,84,52));
            double tx=x+4,ty=y-16;
            for(int ang=0;ang<360;ang+=45)
            {
                double a=ang*Math.PI/180.0;
                for(int t=1;t<9;t++)
                {
                    double px=tx+Math.Cos(a)*t*1.15,py=ty+Math.Sin(a)*t*.5+t*t*.04;
                    cv.Over((int)px,(int)py,Shade(r[3],t<5?1.0:.8));cv.Over((int)px,(int)py+1,r[1]);
                }
            }
        }
        static PixelCanvas ForestSprite(int kind,int variant)
        {
            var cv=new PixelCanvas(SW,SH);int s=variant*31+kind;
            if(kind==13){foreach(var q in Spots(s,7,22,9,7))Pine(cv,q[0],q[1],ForestGround,true,.9);}
            else if(kind==1)
            {
                var r=Ramp(30,70,44,44,100,54,66,130,60,96,160,70,140,190,84);
                foreach(var q in Spots(s,7,22,9,8))Broadleaf(cv,q[0],q[1],r,6.5);
            }
            else if(kind==11)
            {
                int i=0;foreach(var q in Spots(s,7,22,9,8)){if(i%2==0)Palm(cv,q[0],q[1],Jungle);else Broadleaf(cv,q[0],q[1],Jungle,8);i++;}
            }
            else if(kind==19){foreach(var q in Spots(s,6,20,8,8))Broadleaf(cv,q[0],q[1],Mangrove,5);}
            Outline(cv,Ink);return cv;
        }
        static PixelCanvas HillsSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);
            double[][] mounds={new[]{AX-8.0,22,15},new[]{AX+9.0,24,19},new[]{(double)AX,18,11}};
            foreach(var m in mounds)
            {
                double cx=m[0],w=m[1],h=m[2];
                for(int x=(int)(cx-w);x<(int)(cx+w);x++)
                {
                    double u=(x+.5-cx)/w;if(Math.Abs(u)>=1)continue;
                    double top=AY+7-h*Math.Pow(1-u*u,.8);
                    for(int y=(int)top;y<AY+9;y++){double ny=(y-top)/(h+2);cv.Over(x,y,Lit(Grass,u,-.6+ny*.8,x,y,-.02));}
                }
            }
            Outline(cv,Ink);return cv;
        }
        static readonly uint[] MountRock={Rgb(58,56,64),Rgb(84,80,88),Rgb(112,104,106),Rgb(142,130,122),Rgb(172,160,144)};
        static PixelCanvas MountainSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);int s=variant*13;double snowline=.52;
            var peaks=new[]{new[]{AX-9.0,AY+6.0,26+(int)(Hsh(variant,1,2)*5),15.0},new[]{AX+10.0,AY+7.0,22+(int)(Hsh(variant,2,2)*5),14.0},new[]{AX+1.0,AY+10.0,34.0,13.0}};
            foreach(var pk in peaks)
            {
                double px=pk[0],py=pk[1],h=pk[2],w=pk[3];int top=(int)(py-h);
                for(int y=top;y<=(int)py;y++)
                {
                    double f=(y-top)/h,half=w*f;
                    for(int x=(int)(px-half);x<=(int)(px+half);x++)
                    {
                        double side=(x-px)/Math.Max(1.0,half);uint c;
                        if(f<=snowline*.9+Hsh(x,y,s)*.06&&(side<.55||f<.3))c=side<.1?Rgb(240,246,250):Rgb(196,212,226);
                        else{double v=.5+.5*(-side*.9)-.08*f+(Bay(x,y)-.5)*.3;c=RampPick(MountRock,v+(side<-.3?.1:0));}
                        cv.Over(x,y,c);
                    }
                }
            }
            Outline(cv,Ink);return cv;
        }
        static readonly uint[] VolcanoRock={Rgb(36,30,34),Rgb(58,46,46),Rgb(84,64,56),Rgb(112,84,68),Rgb(140,104,80)};
        static PixelCanvas VolcanoSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);int px=AX,py=AY+8,h=26,w=20,top=py-h;
            for(int y=top;y<=py;y++)
            {
                double f=(y-top)/(double)h,half=4+(w-4)*f;
                for(int x=(int)(px-half);x<=(int)(px+half);x++)
                {
                    double side=(x-px)/Math.Max(1.0,half);
                    uint c=RampPick(VolcanoRock,.5+.5*(-side)-.1*f+(Bay(x,y)-.5)*.3);
                    if(f<.18)c=Hsh(x,y,3)>.4?Rgb(240,110,36):Rgb(255,190,70);
                    if(f>.25&&f<.8&&Math.Abs(side-.25*Math.Sin(f*9))<.07)c=Rgb(226,96,32);
                    cv.Over(x,y,c);
                }
            }
            for(int a=0;a<5;a++)cv.Over(px+(a-2)*2,top-3-(a%2)*3,Rgb(120,110,110,200));
            Outline(cv,Ink);return cv;
        }
        static PixelCanvas GlacierSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);
            double[][] blocks={new[]{AX-10.0,16,8},new[]{AX+8.0,22,9},new[]{AX-1.0,28,7},new[]{AX+16.0,12,6}};
            for(int i=0;i<blocks.Length;i++)
            {
                double cx=blocks[i][0],h=blocks[i][1],w=blocks[i][2],bas=AY+8-(i%2)*2;
                var pts=new[]{P(cx-w,bas),P(cx-w*.6,bas-h*.7),P(cx-1,bas-h),P(cx+w*.7,bas-h*.6),P(cx+w,bas)};
                Poly(cv,pts,(X,Y)=>RampPick(Glacier,.55+.55*(-(X-cx)/w)*.6+(Bay(X,Y)-.5)*.25));
            }
            Outline(cv,Rgb(40,70,110));return cv;
        }
        static PixelCanvas RocksSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);
            foreach(var q in Spots(variant*3+1,4,20,8,10))Disk(cv,q[0],q[1]-3,6,5,Rock,0);
            Outline(cv,Ink);return cv;
        }
        static PixelCanvas BogSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);
            foreach(var q in Spots(variant*5+2,9,22,9,5))
            {
                int x=(int)q[0],y=(int)q[1];
                for(int k=0;k<3;k++){RectFill(cv,x+k-1,y-6-k%2*2,x+k,y,Rgb(96,112,56));cv.Over(x+k-1,y-7-k%2*2,Rgb(150,110,60));}
            }
            Outline(cv,Rgb(30,40,24));return cv;
        }
        static readonly uint[] AcaciaRamp={Rgb(70,90,40),Rgb(100,120,50),Rgb(128,146,60),Rgb(156,170,76),Rgb(190,196,100)};
        static PixelCanvas AcaciaSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);int x=AX+(variant%3-1)*8,y=AY+2;
            RectFill(cv,x,y-12,x+2,y,Rgb(110,84,56));
            for(int dx=-8;dx<=8;dx+=4)Disk(cv,x+1+dx,y-14+Math.Abs(dx)/4,5,2.6,AcaciaRamp,0);
            Outline(cv,Ink);return cv;
        }
        static PixelCanvas CactusSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);uint g=Rgb(64,128,70);
            foreach(var q in Spots(variant*9+4,2,16,6,14))
            {
                int x=(int)q[0],y=(int)q[1];
                RectFill(cv,x,y-10,x+3,y,g);RectFill(cv,x-3,y-7,x,y-5,g);RectFill(cv,x-3,y-10,x-1,y-7,g);RectFill(cv,x+3,y-8,x+6,y-6,g);RectFill(cv,x+5,y-11,x+7,y-8,g);
                for(int k=0;k<10;k+=2)cv.Over(x+1,y-10+k,Rgb(110,176,100));
            }
            Outline(cv,Ink);return cv;
        }
        static PixelCanvas DeadTreeSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);int x=AX-6+variant*4,y=AY+3;uint c=Rgb(84,64,52);
            RectFill(cv,x,y-14,x+2,y,c);RectFill(cv,x-4,y-12,x,y-10,c);RectFill(cv,x+2,y-9,x+6,y-7,c);RectFill(cv,x+5,y-12,x+7,y-8,c);
            Outline(cv,Ink);return cv;
        }
        static PixelCanvas TuftSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);uint c=Rgb(150,150,70);
            foreach(var q in Spots(variant*11+6,7,22,9,6)){int x=(int)q[0],y=(int)q[1];for(int k=0;k<3;k++)RectFill(cv,x+k-1,y-3-k%2,x+k,y,c);}
            return cv;
        }
        static readonly Dictionary<int,PixelCanvas> spriteCache=new Dictionary<int,PixelCanvas>();
        // Returns null for terrain with no raised scenery.
        public static PixelCanvas Sprite(int t,int variant)
        {
            int key=t*10+variant;PixelCanvas cv;if(spriteCache.TryGetValue(key,out cv))return cv;
            switch(t)
            {
                case 1:case 13:case 11:case 19:cv=ForestSprite(t,variant);break;
                case 2:cv=HillsSprite(variant);break;
                case 5:cv=MountainSprite(variant);break;
                case 17:cv=VolcanoSprite(variant);break;
                case 15:cv=GlacierSprite(variant);break;
                case 18:cv=RocksSprite(variant);break;
                case 9:cv=BogSprite(variant);break;
                case 16:cv=AcaciaSprite(variant);break;
                case 3:cv=CactusSprite(variant);break;
                case 10:cv=DeadTreeSprite(variant);break;
                case 20:cv=TuftSprite(variant);break;
                default:cv=null;break;
            }
            spriteCache[key]=cv;return cv;
        }

        // ---------- tile composition: ground, blends, rivers, foam, cliffs (64 x 48) ----------
        // Neighbour order everywhere: 0 NE (map y-1), 1 SE (x+1), 2 SW (y+1), 3 NW (x-1).
        public static int Height(int t){return IsWater(t)?0:Elev;}
        static int SkirtFamily(int t)
        {
            switch(t){case 3:return 1;case 5:case 10:case 17:case 18:return 2;case 8:case 13:case 14:case 15:return 3;default:return 0;}
        }
        static readonly uint[][] SkirtColours={new[]{Rgb(88,62,40),Rgb(112,80,50),Rgb(136,98,62)},new[]{Rgb(176,138,84),Rgb(204,168,104),Rgb(226,192,128)},new[]{Rgb(70,66,72),Rgb(98,92,98),Rgb(128,120,120)},new[]{Rgb(150,184,206),Rgb(184,212,230),Rgb(214,232,244)}};
        static readonly uint[] Lip={Rgb(112,172,74),Rgb(62,118,58),Rgb(112,172,74),Rgb(238,212,152),0,Rgb(150,140,130),0,Rgb(112,172,74),Rgb(184,194,180),Rgb(98,108,60),Rgb(148,130,104),Rgb(50,126,56),0,Rgb(210,222,224),Rgb(230,242,248),Rgb(210,230,244),Rgb(222,198,104),Rgb(88,64,50),Rgb(150,140,130),Rgb(84,132,72),Rgb(196,198,112)};
        static uint SkirtPixel(int t,bool left,int x,int y,int ytop)
        {
            var r=SkirtColours[SkirtFamily(t)];int d=y-ytop;
            if(d<2)return Shade(Lip[t]==0?r[2]:Lip[t],left?.92:.7);
            uint baseC=left?r[1]:r[0];double v=.9+.2*(Hsh(x,y>>2,t)-.5);uint c=Shade(baseC,v);
            if((x/3)%5==0&&Hsh(x,y,3)>.5)c=Shade(c,.85);
            return c;
        }
        static double EdgeE(int dir,int px,int py)
        {
            double u=(px+.5-32)/32.0,v=(py+.5-16)/16.0;
            switch(dir){case 0:return u-v;case 1:return u+v;case 2:return v-u;default:return -(u+v);}
        }
        static readonly int[] MidX={48,48,16,16},MidY={8,24,24,8};
        static PixelCanvas RiverOverlay(int mask,int variant)
        {
            var cv=new PixelCanvas(64,32);uint wet=Rgb(40,110,160);
            var ends=new List<int>();for(int k=0;k<4;k++)if((mask&(1<<k))!=0)ends.Add(k);
            if(ends.Count==0)ends.Add(-1);
            foreach(int k in ends)
            {
                int ex=k<0?32:MidX[k],ey=k<0?16:MidY[k];
                for(int s=0;s<=20;s++)
                {
                    double t=s/20.0,x=32+(ex-32)*t,y=16+(ey-16)*t,wob=Math.Sin(t*6+variant)*1.2;
                    double bx=x+wob*.5,by=y+wob*.25;
                    for(int dy=-1;dy<=1;dy++)for(int dx=-2;dx<=2;dx++)if(dx*dx/4.0+dy*dy<=1.2)cv.Set((int)bx+dx,(int)by+dy,wet);
                }
            }
            if(ends.Count==1)for(int dy=-2;dy<=2;dy++)for(int dx=-4;dx<=4;dx++)if(dx*dx/16.0+dy*dy/4.0<=1)cv.Set(32+dx,16+dy,wet);
            var o=new PixelCanvas(64,32);
            for(int y=0;y<32;y++)for(int x=0;x<64;x++)
            {
                if((cv.Get(x,y)>>24)==0)continue;
                int nb=0;if((cv.Get(x+1,y)>>24)==0)nb++;if((cv.Get(x-1,y)>>24)==0)nb++;if((cv.Get(x,y+1)>>24)==0)nb++;if((cv.Get(x,y-1)>>24)==0)nb++;
                if(nb>0)o.Set(x,y,Rgb(24,70,110));
                else o.Set(x,y,Hsh(x,y,5)>.9?Rgb(86,164,200):((x+y)%7!=0?Rgb(52,128,176):Rgb(64,140,184)));
            }
            return o;
        }
        static readonly Dictionary<int,PixelCanvas> riverCache=new Dictionary<int,PixelCanvas>();
        static PixelCanvas River(int mask,int variant){int key=mask*10+variant;PixelCanvas cv;if(!riverCache.TryGetValue(key,out cv)){cv=RiverOverlay(mask,variant);riverCache[key]=cv;}return cv;}

        // nb[k] is the neighbour terrain, already reduced by NeighbourCode (-1 means "no effect").
        public static int NeighbourCode(int t,int dir,int n)
        {
            if(n<0)return -1;
            if(IsWater(t))return IsWater(n)?-1:(dir==0||dir==3?99:-1);
            if(IsWater(n))return 4;
            return RampGroup[n]==RampGroup[t]?-1:n;
        }
        public static PixelCanvas Compose(int t,int variant,int[] nb,int frame,int riverMask)
        {
            var cv=new PixelCanvas(CW,CH);int y0=IsWater(t)?Elev:0;int fr=IsWater(t)?frame:0;
            var bt=BaseTex(t,variant,fr);
            for(int y=0;y<32;y++)for(int x=0;x<64;x++){uint p=bt.Get(x,y);if((p>>24)!=0)cv.Set(x,y+y0,p);}
            if(!IsWater(t))
            {
                for(int d=0;d<4;d++)
                {
                    int n=nb[d];if(n<0||n==99||IsWater(n))continue;
                    var nbase=BaseTex(n,variant,0);
                    for(int y=0;y<32;y++)for(int x=0;x<64;x++)
                    {
                        if(!Inside(x,y))continue;
                        double e=EdgeE(d,x,y);
                        if(e>.62){double pr=.5*(e-.62)/.38;if(Bay(x,y)<pr)cv.Set(x,y+y0,nbase.Get(x,y));}
                    }
                }
                if(t==7)
                {
                    var ov=River(riverMask,variant);
                    for(int y=0;y<32;y++)for(int x=0;x<64;x++){uint c=ov.Get(x,y);if((c>>24)!=0)cv.Set(x,y+y0,c);}
                }
            }
            for(int d=0;d<4;d++)
            {
                int n=nb[d];if(n<0)continue;
                for(int y=0;y<32;y++)for(int x=0;x<64;x++)
                {
                    if(!Inside(x,y))continue;
                    double e=EdgeE(d,x,y);
                    if(!IsWater(t)&&n==4&&e>.9)cv.Set(x,y+y0,Shade(cv.Get(x,y+y0),1.25));
                    if(IsWater(t)&&n==99&&(d==0||d==3)&&e>.86&&((x+frame*2+y)%6)<4)cv.Set(x,y+y0,e>.93?Rgb(226,240,244):Rgb(170,210,224));
                }
            }
            if(!IsWater(t))
            {
                for(int side=0;side<2;side++)
                {
                    bool left=side==0;int dir=left?2:1;
                    if(nb[dir]!=4)continue;
                    for(int x=left?0:32;x<(left?32:64);x++)
                    {
                        double ytop=left?16+(x+.5)*.5:32-(x-32+.5)*.5;int yt=(int)(ytop+.5);
                        for(int d=0;d<Elev;d++){int y=yt+d;if(y<CH)cv.Set(x,y,SkirtPixel(t,left,x,y,yt));}
                    }
                }
            }
            return cv;
        }

        // ---------- small overlays: farm fields, roads, border edges, selection, fog ----------
        public static PixelCanvas Farm(int variant)
        {
            var cv=new PixelCanvas(64,32);
            for(int y=0;y<32;y++)for(int x=0;x<64;x++)
            {
                double u=(x+.5-32)/32.0,v=(y+.5-16)/16.0;
                if(Math.Abs(u)+Math.Abs(v)>.78)continue;
                double a=(u+v)*8+variant;int band=(int)Math.Floor(a);bool green=(band&1)==0;
                uint c=green?Rgb(104,158,60):Rgb(206,176,84);
                if(Math.Abs(a-Math.Round(a))<.1)c=Shade(c,.78);
                cv.Set(x,y,c);
            }
            Outline(cv,Rgb(70,56,30));return cv;
        }
        // mask bits: 0 N(-1,-1 up) 1 NE(0,-1) 2 E(1,-1 right) 3 SE(1,0) 4 S(1,1 down) 5 SW(0,1) 6 W(-1,1 left) 7 NW(-1,0)
        static readonly int[] RoadX={32,48,60,48,32,16,4,16},RoadY={2,8,16,24,30,24,16,8};
        static readonly Dictionary<int,PixelCanvas> roadCache=new Dictionary<int,PixelCanvas>();
        public static PixelCanvas Road(int mask)
        {
            PixelCanvas cv;if(roadCache.TryGetValue(mask,out cv))return cv;
            cv=new PixelCanvas(64,32);
            for(int layer=0;layer<2;layer++)
            {
                uint col=layer==0?Rgb(70,52,32):Rgb(214,184,126);int rad=layer==0?2:1;
                for(int k=0;k<8;k++)
                {
                    if((mask&(1<<k))==0)continue;
                    for(int s=0;s<=16;s++){double t=s/16.0;int x=(int)(32+(RoadX[k]-32)*t),y=(int)(16+(RoadY[k]-16)*t);for(int dy=-rad+1;dy<=rad-1+layer;dy++)for(int dx=-rad;dx<=rad;dx++)cv.Set(x+dx,y+dy,col);}
                }
                for(int dy=-rad+1;dy<=rad-1+layer;dy++)for(int dx=-rad;dx<=rad;dx++)cv.Set(32+dx,16+dy,col);
            }
            roadCache[mask]=cv;return cv;
        }
        static readonly Dictionary<int,PixelCanvas> misc=new Dictionary<int,PixelCanvas>();
        // kind 0..3 = border edge NE,SE,SW,NW (white, tinted at draw time); 4 = selection outline; 5 = dark fog diamond; 6 = dim overlay
        public static PixelCanvas Misc(int kind)
        {
            PixelCanvas cv;if(misc.TryGetValue(kind,out cv))return cv;
            cv=new PixelCanvas(64,32);
            for(int y=0;y<32;y++)for(int x=0;x<64;x++)
            {
                if(!Inside(x,y))continue;
                if(kind<4){double e=EdgeE(kind,x,y);if(e>.9)cv.Set(x,y,Rgb(255,255,255));}
                else if(kind==4)
                {
                    double e=Math.Abs(x+.5-32)/32.0+Math.Abs(y+.5-16)/16.0;
                    if(e>.88)cv.Set(x,y,Rgb(255,222,120));
                }
                else if(kind==5)cv.Set(x,y,(x+y)%5==0?Rgb(14,26,36):Rgb(9,17,24));
                else cv.Set(x,y,Rgb(4,8,20,110));
            }
            misc[kind]=cv;return cv;
        }
    }
}
