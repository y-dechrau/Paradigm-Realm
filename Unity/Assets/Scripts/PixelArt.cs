using System;
using System.Collections.Generic;

namespace ParadigmRealm
{
    // Procedural 16-bit style art for the horizontal three-quarter map: square-grid tiles drawn as 64 x 48 rectangles in level
    // rows, with raised scenery standing up over the row behind. Land and water are level; biomes and coasts blend organically.
    // Pure C#, no Unity types, so the harness can run it.
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
        // TW x TH = one tile on screen at zoom 64; CW x CH = composed ground (the same size: land and water are level);
        // sprites are SW x SH with the tile centre at (AX,AY). Elev stays 0 (no step between land and sea, owner request).
        public const int TW=48,TH=48,SW=64,SH=64,AX=32,AY=40,CW=TW,CH=TH,Elev=0;
        // Diagonal (isometric) view, as in the owner's mockup (30 Oct 2026): each square grid tile of TW x TH is drawn as a DW x DH diamond.
        public const int DW=64,DH=32;
        // Screen mapping shared by the renderer and the checks: tile (i,j) centre relative to the camera, and its inverse.
        // Diagonal view: moving east (i) goes down-right on screen, moving south (j) goes down-left; tw is a diamond's screen width.
        // Staggered rows (World.mapShape 2): tile (x,y) is centred at column x + y%2/2 and half-row y; cx, cy is the camera in
        // the same units. Picking tries the rows around the point and keeps the diamond the point lies deepest inside.
        public static void StaggerScreenOf(double x,int y,double cx,double cy,double tw,out double sx,out double sy){sx=(x+(y&1)*.5-cx)*tw;sy=(y-cy)*tw*DH/DW/2;}
        public static void StaggerPickOf(double dx,double dy,double cx,double cy,double tw,out int x,out int y)
        {
            double th=tw*DH/DW,fy=cy+dy/(th/2),best=double.MaxValue;x=0;y=(int)Math.Round(fy);
            for(int r=(int)Math.Floor(fy)-1;r<=(int)Math.Floor(fy)+2;r++)
            {
                int c=(int)Math.Round(cx+dx/tw-(r&1)*.5);double ox=(c+(r&1)*.5-cx)*tw-dx,oy=(r-cy)*th/2-dy,m=Math.Abs(ox)/(tw/2)+Math.Abs(oy)/(th/2);
                if(m<best){best=m;x=c;y=r;}
            }
        }
        public static void ScreenOf(double i,double j,double camI,double camJ,double tw,out double sx,out double sy){double di=i-camI,dj=j-camJ;sx=(di-dj)*tw/2;sy=(di+dj)*tw*DH/DW/2;}
        public static void PickOf(double dx,double dy,double camI,double camJ,double tw,out int i,out int j){double a=dx/(tw/2),b=dy/(tw*DH/DW/2);i=(int)Math.Floor(camI+(a+b)/2+.5);j=(int)Math.Floor(camJ+(b-a)/2+.5);}
        // Reshapes a square grid-space tile picture (ground, roads, farms, borders) into its DW x DH diamond, nearest pixel, so
        // the pixel art stays crisp. The diamond reaches half a pixel past its edges so neighbouring tiles leave no gaps.
        public static PixelCanvas ToDiamond(PixelCanvas square)
        {
            var d=new PixelCanvas(DW,DH);double reach=.5+1.0/DH;
            for(int py=0;py<DH;py++)for(int px=0;px<DW;px++)
            {
                double x=px+.5-DW/2.0,y=py+.5-DH/2.0,fu=x/DW+y/DH,fv=y/DH-x/DW;
                if(Math.Abs(fu)>reach||Math.Abs(fv)>reach)continue;
                int sx=Math.Max(0,Math.Min(square.w-1,(int)((fu+.5)*square.w))),sy=Math.Max(0,Math.Min(square.h-1,(int)((fv+.5)*square.h)));
                d.Set(px,py,square.Get(sx,sy));
            }
            return d;
        }

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
        public static bool Inside(int x,int y){return x>=0&&y>=0&&x<TW&&y<TH;}
        static uint RampPick(uint[] r,double v){int i=(int)(Math.Max(0,Math.Min(.999,v))*r.Length);return r[i];}
        static uint[] Ramp(int a,int b,int c,int d,int e,int f,int g,int h,int i,int j,int k,int l,int m,int n,int o)
        {return new[]{Rgb(a,b,c),Rgb(d,e,f),Rgb(g,h,i),Rgb(j,k,l),Rgb(m,n,o)};}

        // ---------- palettes ----------
        static readonly uint[] Grass=Ramp(38,96,40,62,132,44,96,164,52,138,192,64,190,218,96);
        static readonly uint[] ForestGround=Ramp(22,62,36,32,84,42,46,108,48,66,130,54,96,152,64);
        static readonly uint[] Sand=Ramp(168,132,76,196,162,98,222,190,124,238,212,152,248,228,178);
        static readonly uint[] Water=Ramp(14,48,140,20,72,178,30,102,208,64,146,232,186,228,250);
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

        // ---------- ground textures (64 x 40 rectangles; noise is only slightly squashed vertically for the 3/4 view) ----------
        static uint BasePixel(int t,int x,int y,int variant,int frame)
        {
            var r=TerrainRamp[t];int s=t*131+variant*17;
            if(t==4||t==12||t==6)
            {
                double n=.6*VNoise(x/7.0,y/5.0,s)+.4*VNoise(x/2.5,y/2.0,s+3);
                double v=.2+n*.5+(Bay(x,y)-.5)*.14;
                uint c=RampPick(r,v);
                // short horizontal ripple dashes drifting with the frame
                int ph=(x+y*3+frame*3)%24;
                double wv=VNoise((x+frame*2)/9.0,y/3.0,s+9);
                if(ph<4&&wv>.5&&Hsh(x>>2,y,s+4)>.45)c=(ph>0&&ph<3)?r[3]:r[4];
                else if(ph>=4&&ph<6&&wv>.55&&Hsh(x>>2,y,s+4)>.45)c=r[0];
                // Small white wave marks scattered over open water, as in the gallery seas.
                if(Hsh(x+frame*3,y,s+11)>.99)c=Rgb(235,248,252);
                else if(VNoise(x/5.0,y/3.0,s+13)>.62&&Hsh(x>>1,y,s+14)>.86&&(y+frame)%3==0)c=Rgb(200,232,250);
                return c;
            }
            double q=.45*VNoise(x/4.0,y/3.0,s)+.3*VNoise(x/1.8,y/1.4,s+1)+.25*Hsh(x,y,s+2);
            double val=q+(Bay(x,y)-.5)*.30;
            if(t==3)val=.55*val+.45*(.5+.5*Math.Sin((x+y*1.4)*.45+VNoise(x/8.0,y/6.0,s)*4));
            uint col=RampPick(r,val);
            double h=Hsh(x,y,s+5);
            if(t==0||t==2||t==7||t==16||t==20||t==1||t==11||t==19||t==9)
            {
                if(h>.955)col=h>.978?r[4]:r[0];
                if((t==0||t==2||t==7)&&Hsh(x,y,s+21)>.994)col=Flowers[(int)(Hsh(x,y,s+22)*4)%4];
                if((t==0||t==2||t==7)&&x>0&&Hsh(x-1,y,s+5)>.955&&h<=.955&&Hsh(x,y,s+23)>.5)col=r[1];
            }
            // Rocky coast as in the gallery maps: green turf with pale rock breaking through, not bare grey stone.
            if(t==18){double st=.6*VNoise(x/5.0,y/4.0,s+31)+.4*VNoise(x/2.2,y/2.2,s+34);if(st>.6)col=Hsh(x,y,s+32)>.35?Rock[3]:Rock[4];else if(st>.56)col=Rock[1];else col=RampPick(Grass,val*.9);}
            if(t==17&&Hsh(x>>1,y,s+7)>.93&&VNoise(x/3.0,y/2.0,s+8)>.55)col=Rgb(236,120,40);
            if(t==10){double w=VNoise(x/6.0,y/3.0,s+11);if(w>.5&&Math.Abs(w-.55)<.02)col=Shade(col,.6);}
            if((t==14||t==15||t==13||t==8)&&h>.97)col=Rgb(255,255,255);
            if(t==9&&VNoise(x/5.0,y/3.0,s+12)>.68)col=MixC(Bog[4],Water[2],.4);
            return col;
        }
        static readonly uint[] Flowers={Rgb(240,226,96),Rgb(246,246,240),Rgb(230,130,170),Rgb(150,140,230)};
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
                // Spread over the rectangular footprint (taller than the old diamond, hence the 1.4 on ry), keeping clear of the edges.
                double u=Hsh(tries,seed,1)*2-1,v=Hsh(tries,seed,2)*2-1;
                if(Math.Abs(u)>.88||Math.Abs(v)>.85)continue;
                double x=AX+u*rx,y=AY+v*ry*1.4;bool ok=true;
                foreach(var q in pts)if(!(Math.Abs(x-q[0])+Math.Abs(y-q[1])*1.4>minSep)){ok=false;break;}
                if(ok)pts.Add(P(x,y));
            }
            pts.Sort((a,b)=>a[1].CompareTo(b[1]));return pts;
        }
        static readonly uint[] Tree0=Ramp(22,62,30,36,92,36,56,124,42,88,156,50,140,192,72);
        static readonly uint[] Tree1=Ramp(12,44,38,20,66,50,32,92,60,52,122,68,92,156,84);
        static readonly uint[] Tree2=Ramp(60,60,28,92,86,34,128,112,40,168,136,48,212,160,60);
        static readonly uint[] Tree3=Ramp(70,44,24,112,62,28,160,86,30,204,118,36,236,160,60);
        static readonly uint[] JungleTree=Ramp(16,52,36,26,84,48,40,116,54,62,146,60,110,176,80);
        static readonly uint[] MangroveTree=Ramp(30,66,52,44,92,62,62,116,66,88,140,74,128,170,96);
        static readonly uint[] RockBrown=Ramp(60,54,50,88,80,70,118,106,90,148,134,112,178,162,134);
        static void Broadleaf(PixelCanvas cv,double x,double y,double sz,uint[] p)
        {
            RectFill(cv,(int)x,(int)y-2,(int)x+2,(int)y+1,Rgb(64,42,28));
            double cx=x+1,cy=y-sz*1.1;
            Disk(cv,cx,cy,sz,sz*.85,p,0);
            double[][] lobes={new[]{-.45,-.35,.62},new[]{.4,-.1,.5},new[]{-.1,-.65,.5}};
            for(int k=0;k<3;k++)Disk(cv,cx+lobes[k][0]*sz,cy+lobes[k][1]*sz,sz*lobes[k][2],sz*lobes[k][2]*.85,p,.08+.06*k);
            for(int k=0;k<4;k++){int px=(int)(cx-sz*.7+Hsh((int)x,k,3)*sz*1.2),py=(int)(cy-sz*.8+Hsh(k,(int)y,4)*sz*.9);cv.Over(px,py,p[4]);}
        }
        static void Pine(PixelCanvas cv,double xd,double yd,bool snow,double sz,uint[] p)
        {
            int x=(int)xd,y=(int)yd;
            RectFill(cv,x,y-2,x+2,y+1,Rgb(58,38,26));
            int H=(int)(17*sz);double th=H/3.0;
            for(int row=0;row<H;row++)
            {
                int k=(int)(row/th);double rr=row-k*th;
                double w=1.0+(rr/th)*(2.4+k*1.3)*sz*1.25+(rr>=th-1?.6:0);
                int yy=y-H+row-1;
                for(int dx=-(int)w-1;dx<(int)w+2;dx++)
                {
                    double e=Math.Abs(dx)-w+(Hsh(dx,yy,k+3)-.5)*.9;if(e>0)continue;
                    double side=dx/Math.Max(1.0,w);
                    double v=.55+.5*(-side)-.25*(rr/th)+(Bay(x+dx,yy)-.5)*.25;
                    if(rr>=th-1.2)v-=.25;
                    uint c=RampPick(p,v);
                    if(snow&&(rr<th*.55||(Hsh(dx,yy,9)>.6&&side<.4)))c=side<.2?Rgb(232,242,248):Rgb(190,208,224);
                    cv.Over(x+1+dx,yy,c);
                }
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
            if(kind==13){foreach(var q in Spots(s,24,27,12,3.6))Pine(cv,q[0],q[1]+2,true,.6+Hsh((int)q[0],(int)q[1],1)*.3,Tree1);}
            else if(kind==1)
            {
                // Dense mixed woodland: dark conifers and round broadleaf trees, with autumn colour here and there.
                foreach(var q in Spots(s,34,31,14,3.2))
                {
                    double h=Hsh((int)q[0],(int)q[1],2);
                    if(h<.5)Pine(cv,q[0],q[1]+2,false,.6+h*.6,Tree1);
                    else Broadleaf(cv,q[0],q[1]+1,3.6+h*2.2,h<.93?Tree0:(h<.97?Tree2:Tree3));
                }
            }
            else if(kind==11)
            {
                int i=0;foreach(var q in Spots(s,17,27,12,4.5)){if(i%3==0)Palm(cv,q[0],q[1],Jungle);else Broadleaf(cv,q[0],q[1]+1,5,JungleTree);i++;}
            }
            else if(kind==19){foreach(var q in Spots(s,13,25,11,4.5))Broadleaf(cv,q[0],q[1],4,MangroveTree);}
            Outline(cv,Rgb(14,22,20));return cv;
        }
        // Open grassland in the gallery maps is dotted with small groves and lone trees; variant 2 stays open meadow.
        static PixelCanvas GroveSprite(int variant)
        {
            // Variants 0-1 a small grove, 2-3 one or two lone trees, 4-7 open meadow; each placed differently.
            if(variant>=4)return null;
            var cv=new PixelCanvas(SW,SH);int s=variant*41+5;bool grove=variant<2;
            double ox=(Hsh(s,1,3)-.5)*18,oy=(Hsh(s,2,3)-.5)*10;
            foreach(var q in Spots(s,grove?6:1+variant%2,grove?13:22,grove?5:9,4.2))
            {
                double h=Hsh((int)q[0],(int)q[1],6);
                if(h<.45)Pine(cv,q[0]+ox,q[1]+oy+2,false,.55+h*.4,Tree1);
                else Broadleaf(cv,q[0]+ox,q[1]+oy+1,3.4+h*1.6,h<.94?Tree0:Tree2);
            }
            Outline(cv,Rgb(14,22,20));return cv;
        }
        static double Bump(double x,double c,double w,double h){double d=Math.Abs(x-c);return d>=w?0.0:h*(.5+.5*Math.Cos(Math.PI*d/w));}
        static readonly uint[] Earth={Rgb(70,52,34),Rgb(98,74,46),Rgb(128,98,60),Rgb(156,124,78)};
        static readonly uint[] Shrub=Ramp(20,56,34,30,78,42,46,104,50,70,130,58,104,156,70);
        // Rolling ridge: two layers of smooth bumps, lit left slopes, shaded right slopes, earth banks, boulders and shrubs.
        static PixelCanvas HillsSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);int s=variant*7;
            int[] bases={AY+6,AY+10};double[] darks={.9,1.0};
            double[][][] layers={new[]{new[]{AX+1.0,17,19},new[]{AX-13.0,11,11},new[]{AX+16.0,10,12}},new[]{new[]{AX-7.0,12,9},new[]{AX+9.0,12,8}}};
            for(int li=0;li<2;li++)
            {
                int bas=bases[li];double dark=darks[li];var bumps=layers[li];int layer=li;
                Func<int,double> H=xx=>{double sum=0;foreach(var b in bumps)sum+=Bump(xx,b[0],b[1],b[2]);return sum+(VNoise(xx/3.2,layer*9,s)-.5)*1.6;};
                for(int x=AX-24;x<=AX+24;x++)
                {
                    double top=bas-H(x);if(bas-top<1.2)continue;
                    double sl=(H(x+1)-H(x-1))/2.0;
                    for(int y=(int)top;y<=bas;y++)
                    {
                        double u=(y-top)/Math.Max(1.0,bas-top);
                        double v=.52+Math.Max(-.3,Math.Min(.3,sl*.45))-.28*u+(Bay(x,y)-.5)*.3;
                        uint c=RampPick(Grass,v*dark);
                        if(y<top+1.5)c=sl>=-.15?Grass[4]:Grass[3];
                        else if(u>.72&&sl<.4)
                        {
                            c=Earth[Math.Min(3,(int)((1-u)*8+Hsh(x>>1,y>>1,s+3)*1.4))];
                            if(Hsh(x,y,s+8)>.9)c=Shade(c,.8);
                        }
                        cv.Over(x,y,c);
                    }
                }
            }
            foreach(var q in Spots(variant*5+3,3,16,4,9))Disk(cv,(int)q[0],(int)q[1]+AY/10+1,2.2,1.8,Shrub,0);
            foreach(var q in Spots(variant*5+9,2,16,4,12))Disk(cv,q[0],q[1]+7,3.2,2.3,RockBrown,0);
            Outline(cv,Rgb(22,44,24));return cv;
        }
        static readonly uint[] VolcanoRock={Rgb(36,30,34),Rgb(58,46,46),Rgb(84,64,56),Rgb(112,84,68),Rgb(140,104,80)};
        // Raised mountains for the three-quarter view (owner request): a cluster of peaks standing up from the rocky ground.
        // Each peak has a jagged ridged-noise silhouette, a lit north-west face and a shaded south-east face split by a
        // wandering crest line, gullies running down the slopes, snow on the upper slopes reaching down the gullies, and a
        // dithered foot so the rock melts into the relief on the ground instead of ending in a hard line.
        static PixelCanvas MountainSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);int s=variant*29+3;
            double[][][] sets={
                new[]{new[]{AX-13.0,AY+0,30,18},new[]{AX+14.0,AY+1,32,17},new[]{AX+0.0,AY+10,46,25},new[]{AX-20.0,AY+13,20,12},new[]{AX+20.0,AY+14,18,11}},
                new[]{new[]{AX-10.0,AY+4,42,22},new[]{AX+12.0,AY+6,38,20},new[]{AX-21.0,AY+12,20,11},new[]{AX+4.0,AY+14,22,15}},
                new[]{new[]{AX+9.0,AY+2,34,19},new[]{AX-7.0,AY+9,46,24},new[]{AX+21.0,AY+12,20,11},new[]{AX-21.0,AY+13,17,10}}};
            foreach(var pk in sets[variant%3])
            {
                double px=pk[0],baseY=pk[1],h=pk[2],hw=pk[3];int pid=(int)(px*7+h);
                double crest=px+(Hsh(pid,s,1)-.5)*hw*.3;
                for(int x=(int)(px-hw);x<=(int)(px+hw);x++)
                {
                    double d=Math.Abs(x-px)/hw;if(d>=1)continue;
                    double jag=(1-Math.Abs(2*VNoise(x/3.0,pid,s)-1))*3.2+(Hsh(x,pid,s+2)-.5)*1.4;
                    double top=baseY-h*Math.Pow(1-d,1.15)+jag*(1-d)*.9+(d<.08?-1:0);
                    for(int y=(int)Math.Ceiling(top);y<=(int)baseY;y++)
                    {
                        double down=(y-top)/Math.Max(1.0,baseY-top),u=(y-(baseY-h))/h;
                        // Foot: dither out over the last few rows so the peak sinks into the ground relief.
                        if(baseY-y<4&&Bay(x,y)*4>baseY-y+.3)continue;
                        double ridge=crest+(VNoise(y/4.0,pid,s+3)-.5)*4*(1-u*.5);bool lit=x<ridge;
                        double gully=VNoise((x-px)/2.2+y*.12,y/7.0,s+pid);
                        double v=(lit?.66:.28)-down*.18+(gully-.5)*.34+(Bay(x,y)-.5)*.12;
                        if(Math.Abs(x-ridge)<.9&&down>.05)v=lit?v+.18:v-.1;
                        double snow=.42+(VNoise(x/3.5,y/3.0,s+7)-.5)*.18+(gully>.6?.16:0);
                        uint c;
                        if(u<snow*1.0&&h>16)c=lit?(gully<.35?Snow[3]:Snow[4]):(gully<.4?Snow[1]:Snow[2]);
                        else c=RampPick(Rock,v);
                        cv.Set(x,y,c);
                    }
                }
            }
            // Dark outline only along the silhouette's upper edge and sides, never at the dithered foot.
            var src=(uint[])cv.p.Clone();
            for(int y=1;y<SH-1;y++)for(int x=1;x<SW-1;x++)
            {
                if((src[y*SW+x]>>24)!=0)continue;
                bool below=(src[(y+1)*SW+x]>>24)!=0,side=((src[y*SW+x-1]>>24)!=0||(src[y*SW+x+1]>>24)!=0)&&y<AY+2;
                if(below&&y<AY+4||side)cv.p[y*SW+x]=Rgb(30,30,40);
            }
            return cv;
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
        static readonly uint[] RockPal={Rgb(70,68,76),Rgb(106,102,106),Rgb(142,136,132),Rgb(178,170,160),Rgb(212,204,190)};
        static void RockForm(PixelCanvas cv,double x,double y,double w,double h,int seed)
        {
            Func<int,double> j=k=>Hsh(k,seed,7)-.5;
            var pts=new[]{P(x-w,y),P(x-w*.9+j(1)*2,y-h*.4),P(x-w*.55+j(2)*2,y-h*.75),P(x-w*.2+j(3),y-h),P(x+w*.1,y-h*.8+j(4)*2),P(x+w*.35,y-h*.95),P(x+w*.8+j(5)*2,y-h*.45),P(x+w,y),P(x+w*.4,y+2),P(x-w*.5,y+2)};
            double ridge=x-w*.1+j(6)*w*.4;
            Poly(cv,pts,(X,Y)=>
            {
                double side=(X-ridge)/w;
                double v=(side<0?.7:.26)+.2*(1-(y-Y)/Math.Max(1.0,h))+(Bay(X,Y)-.5)*.3;
                if((Y+seed)%3==0&&Hsh(X,Y,seed)>.35)v-=.2;
                if(Hsh(X,Y>>1,seed+1)>.92)v+=.28;
                if(Math.Abs(X-ridge)<1)v-=.2;
                return RampPick(RockPal,v);
            });
        }
        static PixelCanvas RocksSprite(int variant)
        {
            var cv=new PixelCanvas(SW,SH);int s=variant*3+1;int i=0;
            foreach(var q in Spots(s,5,26,10,7))
            {
                double d=1.0-Math.Abs(q[0]-AX)/30.0;
                double w=3+Hsh(i,s,1)*4+d*3,h=5+Hsh(i,s,2)*7+d*6;
                RockForm(cv,q[0],q[1]+3,w,h,s+i);i++;
            }
            foreach(var q in Spots(s+9,16,27,12,2.5)){cv.Over((int)q[0],(int)q[1]+3,RockPal[3]);cv.Over((int)q[0]+1,(int)q[1]+3,RockPal[1]);}
            Outline(cv,Rgb(20,20,26));return cv;
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
            foreach(var q in Spots(variant*13+7,6,22,9,6)){int tx=(int)q[0],ty=(int)q[1];for(int k=0;k<3;k++)RectFill(cv,tx+k-1,ty-3-k%2,tx+k,ty,Rgb(214,186,84));cv.Over(tx,ty-4,Rgb(240,220,120));}
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
        // ---------- settlements that grow with the city (owner request) ----------
        // A city is drawn from small buildings in the style of its age: round thatched huts (Stone Age), mud-brick blocks
        // (Bronze Age), whitewashed houses with terracotta roofs (Classical), timber houses with red and blue roofs (Medieval
        // onwards). More houses appear as the population grows; walls with towers appear only once Walls are built; a castle
        // keep appears for the capital from size 4 and for walled cities from size 8; temple, market, granary and library
        // appear once built. Canvas 64 x 64 with the tile centre at (32,44), so large towns rise above their tile.
        public const int CityCX=32,CityCY=44;
        public const int FlagWalls=1,FlagCastle=2,FlagTemple=4,FlagMarket=8,FlagGranary=16,FlagLibrary=32;
        static readonly uint Stone0=Rgb(96,92,96),Stone1=Rgb(140,134,130),Stone2=Rgb(178,172,162),Stone3=Rgb(208,202,190);
        static readonly uint[] RoofRed={Rgb(120,38,32),Rgb(176,58,44),Rgb(214,90,62)},RoofBlue={Rgb(30,52,120),Rgb(52,88,170),Rgb(90,130,214)},RoofTerra={Rgb(140,64,36),Rgb(190,92,52),Rgb(224,128,80)},Thatch={Rgb(110,80,40),Rgb(160,120,64),Rgb(206,166,96)};
        static void Gable(PixelCanvas cv,int x,int yBase,int w,int rows,uint[] roof)
        {
            for(int r=0;r<rows;r++){int half=Math.Max(0,(w/2)-r*(w/2)/rows);for(int dx=-half;dx<=half;dx++)cv.Over(x+dx,yBase-r,dx<0?roof[2]:dx==0?roof[1]:roof[1]);if(half>0)cv.Over(x+half,yBase-r,roof[0]);}
        }
        static void House(PixelCanvas cv,int x,int y,int style,int v)
        {
            if(style==0)
            {
                // Round hut: thatched cone over a low mud wall, dark doorway.
                RectFill(cv,x-4,y-3,x+4,y,Rgb(140,104,66));RectFill(cv,x+2,y-3,x+4,y,Rgb(112,80,50));
                for(int r=0;r<7;r++){int half=4-r*4/7+(r<2?1:0);for(int dx=-half;dx<=half;dx++)cv.Over(x+dx,y-3-r,dx<-half/3?Thatch[2]:dx>half/2?Thatch[0]:Thatch[1]);}
                RectFill(cv,x-1,y-2,x+1,y,Rgb(50,32,22));return;
            }
            if(style==1)
            {
                // Mud-brick block with a flat roof and a dark doorway.
                RectFill(cv,x-4,y-6,x+4,y,Rgb(200,152,96));RectFill(cv,x+2,y-6,x+4,y,Rgb(170,124,76));RectFill(cv,x-4,y-8,x+4,y-6,Rgb(228,190,132));
                RectFill(cv,x-2,y-3,x,y,Rgb(70,44,30));cv.Over(x+2,y-4,Rgb(70,44,30));
                if(v%3==0){RectFill(cv,x-4,y-10,x,y-8,Rgb(214,170,112));}
                return;
            }
            uint[] roof=style==2?RoofTerra:(v%3==0?RoofBlue:RoofRed);
            uint wall=style==2?Rgb(232,226,208):Rgb(226,206,160),wallDark=style==2?Rgb(196,190,174):Rgb(188,166,120);
            RectFill(cv,x-4,y-5,x+4,y,wall);RectFill(cv,x+2,y-5,x+4,y,wallDark);
            if(style>=3){uint timber=Rgb(92,62,40);for(int yy=y-5;yy<y;yy++){cv.Over(x-4,yy,timber);cv.Over(x,yy,timber);}for(int xx=x-4;xx<x+4;xx++)cv.Over(xx,y-3,timber);}
            RectFill(cv,x-2,y-3,x,y,Rgb(70,44,30));cv.Over(x+2,y-3,Rgb(70,96,130));
            Gable(cv,x,y-6,10,style==2?3:5,roof);
            if(style>=3&&v%2==0){cv.Over(x+2,y-10,Rgb(120,110,104));cv.Over(x+2,y-11,Rgb(150,150,150,170));cv.Over(x+3,y-13,Rgb(180,180,180,120));}
        }
        static void Tower(PixelCanvas cv,int x,int y,int h,bool cone)
        {
            RectFill(cv,x-3,y-h,x+3,y,Stone2);RectFill(cv,x-3,y-h,x-2,y,Stone3);RectFill(cv,x+1,y-h,x+3,y,Stone1);
            cv.Over(x,y-h+4,Rgb(40,36,40));cv.Over(x,y-h+5,Rgb(40,36,40));
            if(cone){for(int r=0;r<7;r++){int half=4-r*4/7;for(int dx=-half;dx<=half;dx++)cv.Over(x+dx,y-h-r,dx<0?RoofBlue[2]:dx==0?RoofBlue[1]:RoofBlue[0]);}cv.Over(x,y-h-7,Rgb(60,50,40));cv.Over(x+1,y-h-8,Rgb(214,60,50));cv.Over(x+2,y-h-8,Rgb(214,60,50));}
            else{RectFill(cv,x-4,y-h-2,x+4,y-h,Stone2);for(int dx=-4;dx<4;dx+=2)cv.Over(x+dx,y-h-3,Stone3);}
        }
        // Walls and towers in the material of the age: wooden palisade, mud brick, dressed stone, then castle stone.
        static readonly uint[][] WallMat={new[]{Rgb(84,56,34),Rgb(120,84,50),Rgb(156,114,70),Rgb(186,144,92)},new[]{Rgb(150,108,66),Rgb(186,140,90),Rgb(214,172,116),Rgb(234,198,146)},new[]{Rgb(150,142,128),Rgb(190,182,166),Rgb(220,214,198),Rgb(240,236,222)},new[]{Stone0,Stone1,Stone2,Stone3}};
        static void WallColumn(PixelCanvas cv,int x,int yBase,bool front,int style)
        {
            var m=WallMat[style];
            if(style==0){int h=6+(x%2);for(int k=0;k<h;k++)cv.Over(x,yBase-k,front?(x%2==0?m[2]:m[1]):m[1]);cv.Over(x,yBase-h,m[3]);return;}
            int hh=5;for(int k=0;k<hh;k++)cv.Over(x,yBase-k,front?(k==hh-1?m[3]:(x%4==0?m[1]:m[2])):(k==hh-1?m[2]:m[1]));
            if(x%2==0)cv.Over(x,yBase-hh,front?m[3]:m[2]);
        }
        static void WallTower(PixelCanvas cv,int x,int y,int h,int style)
        {
            if(style==3){Tower(cv,x,y,h,true);return;}
            var m=WallMat[style];
            if(style==0)
            {
                // Wooden watchtower: two posts, a platform and a little thatched roof.
                RectFill(cv,x-3,y-h,x-2,y,m[1]);RectFill(cv,x+2,y-h,x+3,y,m[0]);RectFill(cv,x-4,y-h-2,x+4,y-h,m[2]);
                for(int r=0;r<4;r++){int half=4-r;for(int dx=-half;dx<=half;dx++)cv.Over(x+dx,y-h-3-r,Thatch[dx<0?2:1]);}return;
            }
            RectFill(cv,x-3,y-h,x+3,y,m[2]);RectFill(cv,x-3,y-h,x-2,y,m[3]);RectFill(cv,x+1,y-h,x+3,y,m[1]);
            cv.Over(x,y-h+4,Rgb(50,40,36));RectFill(cv,x-4,y-h-2,x+4,y-h,m[2]);for(int dx=-4;dx<4;dx+=2)cv.Over(x+dx,y-h-3,m[3]);
        }
        // The seat of power: a chief's longhouse, a stepped mud-brick palace, a columned palace, then a stone castle.
        static void Keep(PixelCanvas cv,int cx,int y,int style)
        {
            if(style==0)
            {
                RectFill(cv,cx-10,y-4,cx+10,y,Rgb(140,104,66));RectFill(cv,cx+6,y-4,cx+10,y,Rgb(112,80,50));RectFill(cv,cx-1,y-3,cx+1,y,Rgb(50,32,22));
                for(int r=0;r<8;r++){int half=11-r;for(int dx=-half;dx<=half;dx++)cv.Over(cx+dx,y-4-r,dx<-half/3?Thatch[2]:dx>half/2?Thatch[0]:Thatch[1]);}
                RectFill(cv,cx-1,y-14,cx+1,y-11,Rgb(110,80,40));cv.Over(cx,y-15,Rgb(210,60,50));cv.Over(cx+1,y-15,Rgb(210,60,50));return;
            }
            if(style==1){for(int tier=0;tier<4;tier++){int half=11-tier*3;RectFill(cv,cx-half,y-4*(tier+1),cx+half,y-4*tier,tier%2==0?Rgb(200,152,96):Rgb(226,186,128));RectFill(cv,cx+half-2,y-4*(tier+1),cx+half,y-4*tier,Rgb(170,124,76));}RectFill(cv,cx-1,y-3,cx+2,y,Rgb(70,44,30));return;}
            if(style==2)
            {
                RectFill(cv,cx-11,y-3,cx+11,y,Stone3);RectFill(cv,cx-10,y-4,cx+10,y-3,Stone2);
                for(int k=-9;k<=9;k+=3)RectFill(cv,cx+k,y-12,cx+k+2,y-4,Rgb(240,236,226));
                RectFill(cv,cx-11,y-14,cx+11,y-12,Stone3);Gable(cv,cx,y-15,24,5,new[]{Rgb(170,160,150),Rgb(214,206,196),Rgb(244,240,232)});return;
            }
            RectFill(cv,cx-8,y-14,cx+8,y,Stone2);RectFill(cv,cx+3,y-14,cx+8,y,Stone1);RectFill(cv,cx-8,y-14,cx-6,y,Stone3);
            for(int x=cx-8;x<cx+8;x+=2)cv.Over(x,y-15,Stone2);RectFill(cv,cx-2,y-5,cx+2,y,Rgb(40,34,36));cv.Over(cx-5,y-10,Rgb(40,34,36));cv.Over(cx+5,y-10,Rgb(40,34,36));
            Tower(cv,cx-9,y,20,true);Tower(cv,cx+9,y,20,true);Tower(cv,cx,y-8,14,true);
        }
        static void Temple(PixelCanvas cv,int x,int y,int style)
        {
            if(style==0){for(int k=-1;k<=1;k++){RectFill(cv,x+k*4-1,y-6+Math.Abs(k),x+k*4+1,y,Stone2);cv.Over(x+k*4,y-6+Math.Abs(k),Stone3);}RectFill(cv,x-5,y-7,x+6,y-6,Stone1);return;}
            if(style==1){for(int tier=0;tier<3;tier++){int half=7-tier*2;RectFill(cv,x-half,y-3*(tier+1),x+half,y-3*tier,tier%2==0?Rgb(196,150,96):Rgb(220,180,124));}return;}
            if(style==2){RectFill(cv,x-6,y-1,x+6,y,Stone3);for(int k=-5;k<=5;k+=2)RectFill(cv,x+k,y-6,x+k+1,y-1,Rgb(240,236,226));RectFill(cv,x-6,y-7,x+6,y-6,Stone3);Gable(cv,x,y-8,13,3,new[]{Rgb(170,160,150),Rgb(214,206,196),Rgb(244,240,232)});return;}
            // Church: nave with a red roof and a tall spire.
            RectFill(cv,x-5,y-6,x+3,y,Stone3);RectFill(cv,x+1,y-6,x+3,y,Stone2);Gable(cv,x-1,y-7,10,4,RoofRed);
            Tower(cv,x+5,y,12,true);
        }
        static void Market(PixelCanvas cv,int x,int y)
        {
            for(int k=0;k<2;k++){int sx=x+k*7-3;RectFill(cv,sx-3,y-3,sx+3,y,Rgb(150,110,70));for(int dx=-3;dx<3;dx++)cv.Over(sx+dx,y-4,((dx+3)/2)%2==0?Rgb(214,60,50):Rgb(244,236,220));cv.Over(sx-1,y-2,Rgb(240,200,80));cv.Over(sx+1,y-2,Rgb(120,180,70));}
        }
        static void Granary(PixelCanvas cv,int x,int y,int style)
        {
            if(style<=1){RectFill(cv,x-3,y-7,x+3,y,Rgb(186,150,96));RectFill(cv,x+1,y-7,x+3,y,Rgb(156,120,76));for(int r=0;r<4;r++){int half=3-r;for(int dx=-half;dx<=half;dx++)cv.Over(x+dx,y-7-r,Thatch[dx<0?2:1]);}return;}
            RectFill(cv,x-5,y-6,x+5,y,Rgb(168,56,44));RectFill(cv,x+2,y-6,x+5,y,Rgb(132,42,34));RectFill(cv,x-1,y-4,x+1,y,Rgb(70,44,30));Gable(cv,x,y-7,12,4,new[]{Rgb(80,74,70),Rgb(110,104,98),Rgb(140,134,126)});
        }
        static void Library(PixelCanvas cv,int x,int y)
        {
            RectFill(cv,x-5,y-6,x+5,y,Stone3);RectFill(cv,x+2,y-6,x+5,y,Stone2);for(int k=-4;k<=4;k+=2)cv.Over(x+k,y-3,Rgb(70,90,120));
            Disk(cv,x,y-8,4,3,new[]{Rgb(60,110,110),Rgb(80,140,136),Rgb(100,166,156),Rgb(130,190,176),Rgb(170,214,200)},0);
        }
        static readonly Dictionary<long,PixelCanvas> settlementCache=new Dictionary<long,PixelCanvas>();
        public static PixelCanvas Settlement(int style,int pop,int flags,int variant)
        {
            style=Math.Max(0,Math.Min(3,style));pop=Math.Max(1,Math.Min(16,pop));variant&=7;
            long key=((long)style<<24)|((long)pop<<16)|((long)flags<<4)|(long)variant;PixelCanvas cv;
            if(settlementCache.TryGetValue(key,out cv))return cv;
            cv=new PixelCanvas(64,64);int cx=CityCX,cy=CityCY;
            bool walls=(flags&FlagWalls)!=0,castle=(flags&FlagCastle)!=0;
            var draws=new List<KeyValuePair<double,Action>>();
            // Walls: the back half is drawn first, the front half (with the gate) last.
            const double wrx=28,wry=13;int wy=cy-1;
            if(walls)for(int x=cx-(int)wrx;x<=cx+(int)wrx;x++){double f=1-Math.Pow((x-cx)/wrx,2);if(f<0)continue;int yb=(int)Math.Round(wy-wry*Math.Sqrt(f));WallColumn(cv,x,yb,false,style);}
            if(walls)foreach(int tx in new[]{cx-20,cx+20}){int ty=(int)Math.Round(wy-wry*Math.Sqrt(1-Math.Pow((tx-cx)/wrx,2)));WallTower(cv,tx,ty,9,style);}
            if(castle)
            {
                draws.Add(new KeyValuePair<double,Action>(cy-6,()=>Keep(cv,cx,cy-6,style)));
            }
            // Special buildings at fixed places; houses keep clear of them.
            var reserved=new List<double[]>();
            Action<int,int,int,Action> special=(x,y,r,a)=>{reserved.Add(new double[]{x,y,r});draws.Add(new KeyValuePair<double,Action>(y,a));};
            if(castle)reserved.Add(new double[]{cx,cy-10,12});
            if((flags&FlagTemple)!=0)special(cx-13,cy-3,8,()=>Temple(cv,cx-13,cy-3,style));
            if((flags&FlagGranary)!=0)special(cx+15,cy-4,6,()=>Granary(cv,cx+15,cy-4,style));
            if((flags&FlagLibrary)!=0)special(cx+9,cy+5,6,()=>Library(cv,cx+9,cy+5));
            if((flags&FlagMarket)!=0)special(cx-6,cy+6,7,()=>Market(cv,cx-6,cy+6));
            int houses=Math.Min(18,2+pop);double hrx=8+Math.Min(pop,14)*1.3,hry=walls?8:4+Math.Min(pop,14)*.5;
            int placed=0;
            foreach(var q in Spots(variant*37+pop*3+style,houses*3,hrx,hry,7))
            {
                if(placed>=houses)break;double qx=q[0],qy=q[1]+(cy-AY);bool clear=true;
                foreach(var r in reserved)if(Math.Abs(qx-r[0])<r[2]&&Math.Abs(qy-r[1])<r[2]*.75){clear=false;break;}
                if(!clear)continue;placed++;int hx=(int)qx,hy=(int)qy,hv=placed+variant;
                draws.Add(new KeyValuePair<double,Action>(hy,()=>House(cv,hx,hy,style,hv)));
            }
            draws.Sort((a,b)=>a.Key.CompareTo(b.Key));foreach(var d in draws)d.Value();
            if(walls)
            {
                for(int x=cx-(int)wrx;x<=cx+(int)wrx;x++){double f=1-Math.Pow((x-cx)/wrx,2);if(f<0)continue;int yb=(int)Math.Round(wy+wry*Math.Sqrt(f));if(Math.Abs(x-cx)<=2&&yb>cy)continue;WallColumn(cv,x,yb,true,style);}
                int gy=(int)Math.Round(wy+wry);RectFill(cv,cx-2,gy-4,cx+3,gy+1,Rgb(52,38,30));
                WallTower(cv,cx-5,gy+1,8,style);WallTower(cv,cx+6,gy+1,8,style);
                foreach(int tx in new[]{cx-24,cx+24}){int ty=(int)Math.Round(wy+wry*Math.Sqrt(1-Math.Pow((tx-cx)/wrx,2)));WallTower(cv,tx,ty+1,9,style);}
            }
            Outline(cv,Ink);settlementCache[key]=cv;return cv;
        }
        // ---------- interface icons (24 x 24, gold and enamel on a dark outline, for the dark navy and gold interface) ----------
        public const int IconEmpire=0,IconResearch=1,IconDiplomacy=2,IconCities=3,IconMilitary=4,IconRealms=5,IconVictory=6,IconChronicle=7,IconIdentity=8,
            IconGold=9,IconScience=10,IconAge=11,IconOrder=12,IconTurn=13,IconMenu=14,IconCrown=15,IconFood=16,IconShields=17,IconExplore=18,IconSkip=19,
            IconFortify=20,IconFound=21,IconMore=22,IconZoomIn=23,IconZoomOut=24,IconLayers=25,IconSentry=26,IconRoad=27,IconTrade=28,IconHelp=29,IconNext=30,
            IconMoodGood=31,IconMoodNeutral=32,IconMoodBad=33,IconMove=34;
        static readonly uint[] HudGold=Ramp(90,64,24,150,112,44,206,164,76,240,206,120,255,240,190),HudBlue=Ramp(20,50,100,40,90,160,70,130,210,120,180,240,190,225,255),
            HudSilver=Ramp(70,76,90,120,128,142,170,178,190,210,216,226,245,248,252),HudGreen=Ramp(20,70,30,40,110,40,70,150,60,120,190,90,180,230,150),
            HudRed=Ramp(90,20,20,150,40,30,200,70,50,235,120,90,250,180,150),HudWood=Ramp(60,36,20,92,58,32,128,86,48,164,118,70,196,152,100);
        static readonly Dictionary<int,PixelCanvas> iconCacheHud=new Dictionary<int,PixelCanvas>();
        static void Line(PixelCanvas cv,double x0,double y0,double x1,double y1,double w,uint[] ramp)
        {
            int n=(int)Math.Ceiling(Math.Max(Math.Abs(x1-x0),Math.Abs(y1-y0))*2)+1;
            for(int i=0;i<=n;i++){double t=i/(double)n,x=x0+(x1-x0)*t,y=y0+(y1-y0)*t;for(int dy=-(int)w;dy<=(int)w;dy++)for(int dx=-(int)w;dx<=(int)w;dx++)if(dx*dx+dy*dy<=w*w+.5)cv.Over((int)Math.Round(x+dx),(int)Math.Round(y+dy),RampPick(ramp,.75-(dx+dy)*.12));}
        }
        static void Shield(PixelCanvas cv,uint[] face,uint[] emblem)
        {
            Poly(cv,new[]{P(4,3),P(20,3),P(20,11),P(12,21),P(4,11)},(X,Y)=>RampPick(face,.85-(X-4)/22.0-(Y-3)/60.0));
            RectFill(cv,11,5,13,17,RampPick(emblem,.7));RectFill(cv,7,8,17,10,RampPick(emblem,.7));
        }
        public static PixelCanvas HudIcon(int k)
        {
            PixelCanvas cv;if(iconCacheHud.TryGetValue(k,out cv))return cv;cv=new PixelCanvas(24,24);
            switch(k)
            {
                case IconEmpire:Shield(cv,HudGold,HudBlue);break;
                case IconFortify:Shield(cv,HudSilver,HudBlue);break;
                case IconResearch:RectFill(cv,10,3,14,10,HudSilver[3]);RectFill(cv,9,2,15,3,HudSilver[4]);Poly(cv,new[]{P(5,21),P(19,21),P(14,10),P(10,10)},(X,Y)=>Y>14?RampPick(HudBlue,.85-(X-5)/20.0):RampPick(HudSilver,.8-(X-5)/24.0));break;
                case IconDiplomacy:Disk(cv,11,14,7,4,HudSilver,.25);Disk(cv,18,10,3,3,HudSilver,.25);Poly(cv,new[]{P(6,13),P(12,3),P(15,13)},(X,Y)=>RampPick(HudSilver,.95-(Y-3)/30.0));cv.Over(21,10,HudGold[3]);cv.Over(22,10,HudGold[2]);cv.Over(19,9,Rgb(20,24,30));Line(cv,4,16,2,19,.6,HudSilver);break;
                case IconCities:Poly(cv,new[]{P(2,9),P(12,2),P(22,9)},(X,Y)=>RampPick(HudGold,.9-(X-2)/26.0));RectFill(cv,3,19,21,22,HudGold[2]);foreach(int cx in new[]{5,10,15})RectFill(cv,cx,10,cx+3,19,cx<12?HudSilver[3]:HudSilver[2]);break;
                case IconMilitary:Line(cv,5,4,18,17,1,HudSilver);Line(cv,19,4,6,17,1,HudSilver);Line(cv,15,19,20,14,1,HudGold);Line(cv,4,14,9,19,1,HudGold);RectFill(cv,18,18,21,21,HudWood[2]);RectFill(cv,3,18,6,21,HudWood[2]);break;
                case IconRealms:Poly(cv,new[]{P(12,1),P(14.5,9.5),P(23,12),P(14.5,14.5),P(12,23),P(9.5,14.5),P(1,12),P(9.5,9.5)},(X,Y)=>RampPick(HudGold,(X<12)==(Y<12)?.85:.45));Disk(cv,12,12,2.5,2.5,HudBlue,.2);break;
                case IconVictory:Poly(cv,new[]{P(5,3),P(19,3),P(17,11),P(7,11)},(X,Y)=>RampPick(HudGold,.9-(X-5)/18.0));RectFill(cv,11,11,13,16,HudGold[2]);RectFill(cv,7,16,17,19,HudGold[1]);Line(cv,4,4,2,8,.6,HudGold);Line(cv,20,4,22,8,.6,HudGold);break;
                case IconChronicle:RectFill(cv,4,4,20,20,HudRed[1]);RectFill(cv,6,5,19,18,Rgb(236,226,198));for(int y=8;y<17;y+=3)RectFill(cv,8,y,17,y+1,Rgb(150,140,120));RectFill(cv,4,4,6,20,HudRed[0]);break;
                case IconIdentity:case IconFound:RectFill(cv,5,2,7,22,HudWood[2]);Poly(cv,new[]{P(7,3),P(20,3),P(17,8),P(20,13),P(7,13)},(X,Y)=>RampPick(k==IconFound?HudGreen:HudRed,.85-(X-7)/20.0));break;
                case IconGold:case IconTrade:Disk(cv,12,12,8,8,HudGold,.1);Disk(cv,12,12,5,5,HudGold,-.1);RectFill(cv,11,9,13,15,HudGold[4]);break;
                case IconScience:Poly(cv,new[]{P(2,6),P(11,8),P(11,20),P(2,18)},(X,Y)=>RampPick(HudBlue,.95-(11-X)/20.0));Poly(cv,new[]{P(13,8),P(22,6),P(22,18),P(13,20)},(X,Y)=>RampPick(HudBlue,.7-(X-13)/30.0));RectFill(cv,11,8,13,20,HudBlue[0]);break;
                case IconAge:Disk(cv,12,12,5,5,HudGold,.2);for(int a=0;a<8;a++){double t=a*Math.PI/4;Line(cv,12+Math.Cos(t)*7,12+Math.Sin(t)*7,12+Math.Cos(t)*10,12+Math.Sin(t)*10,.6,HudGold);}break;
                case IconOrder:Disk(cv,12,12,9,9,HudGreen,.1);cv.Set(9,9,Rgb(20,30,20));cv.Set(15,9,Rgb(20,30,20));cv.Set(9,10,Rgb(20,30,20));cv.Set(15,10,Rgb(20,30,20));for(int x=8;x<=16;x++)cv.Set(x,15+(x==8||x==16?-1:0),Rgb(20,30,20));break;
                case IconTurn:RectFill(cv,5,2,19,4,HudWood[2]);RectFill(cv,5,20,19,22,HudWood[2]);Poly(cv,new[]{P(7,4),P(17,4),P(12,12)},(X,Y)=>Y>8?HudGold[3]:HudSilver[3]);Poly(cv,new[]{P(12,12),P(17,20),P(7,20)},(X,Y)=>Y>15?HudGold[3]:HudSilver[3]);break;
                case IconMenu:foreach(int y in new[]{6,11,16})RectFill(cv,4,y,20,y+3,Rgb(236,226,198));break;
                case IconCrown:Poly(cv,new[]{P(3,18),P(3,7),P(8,12),P(12,4),P(16,12),P(21,7),P(21,18)},(X,Y)=>RampPick(HudGold,.9-(X-3)/24.0));RectFill(cv,3,17,22,20,HudGold[1]);cv.Over(12,13,HudRed[3]);break;
                case IconFood:Poly(cv,new[]{P(12,2),P(20,10),P(13,21),P(5,12)},(X,Y)=>RampPick(HudGreen,.9-(X-5)/20.0));Line(cv,7,18,17,6,.4,Ramp(20,60,20,30,80,30,40,100,40,60,120,50,80,140,60));break;
                case IconShields:Line(cv,5,20,15,8,1,HudWood);RectFill(cv,12,3,21,9,HudSilver[2]);RectFill(cv,12,3,21,5,HudSilver[4]);break;
                case IconExplore:for(int a=0;a<64;a++){double t=a*Math.PI/32;cv.Over((int)Math.Round(10+Math.Cos(t)*6),(int)Math.Round(10+Math.Sin(t)*6),HudSilver[3]);cv.Over((int)Math.Round(10+Math.Cos(t)*5),(int)Math.Round(10+Math.Sin(t)*5),HudSilver[2]);}Disk(cv,10,10,4,4,HudBlue,.5);Line(cv,15,15,21,21,1.2,HudWood);break;
                case IconSkip:case IconNext:Poly(cv,new[]{P(3,5),P(11,12),P(3,19)},(X,Y)=>HudGold[3]);Poly(cv,new[]{P(12,5),P(20,12),P(12,19)},(X,Y)=>HudGold[2]);break;
                case IconMore:foreach(int x in new[]{5,12,19})Disk(cv,x,12,2.2,2.2,HudSilver,.2);break;
                case IconZoomIn:RectFill(cv,10,4,14,20,Rgb(236,226,198));RectFill(cv,4,10,20,14,Rgb(236,226,198));break;
                case IconZoomOut:RectFill(cv,4,10,20,14,Rgb(236,226,198));break;
                case IconLayers:for(int i=0;i<3;i++){int y0=14-i*5;Poly(cv,new[]{P(12,y0-5),P(21,y0),P(12,y0+5),P(3,y0)},(X,Y)=>RampPick(i==0?HudSilver:i==1?HudBlue:HudGold,.8-(X-3)/30.0));}break;
                case IconSentry:Disk(cv,12,12,9,5,HudSilver,.3);Disk(cv,12,12,3.5,3.5,HudBlue,0);cv.Set(12,12,Rgb(10,14,20));cv.Set(11,11,Rgb(255,255,255));break;
                case IconRoad:Poly(cv,new[]{P(9,2),P(15,2),P(21,22),P(3,22)},(X,Y)=>RampPick(HudWood,.6+(Y)/60.0));for(int y=4;y<21;y+=5)RectFill(cv,11,y,13,y+2,Rgb(236,226,198));break;
                case IconMoodGood:case IconMoodNeutral:case IconMoodBad:{var face=k==IconMoodGood?HudGreen:k==IconMoodNeutral?HudGold:HudRed;Disk(cv,12,12,9,9,face,.1);uint ink=Rgb(20,24,20);cv.Set(9,9,ink);cv.Set(9,10,ink);cv.Set(15,9,ink);cv.Set(15,10,ink);for(int x=8;x<=16;x++){int y=k==IconMoodGood?(x==8||x==16?14:15):k==IconMoodBad?(x==8||x==16?16:15):15;cv.Set(x,y,ink);}break;}
                case IconMove:Poly(cv,new[]{P(3,9),P(13,9),P(13,4),P(21,12),P(13,20),P(13,15),P(3,15)},(X,Y)=>RampPick(HudSilver,.95-(Y-4)/20.0));break;
                case IconHelp:Disk(cv,12,12,9,9,HudBlue,.1);RectFill(cv,10,6,15,8,Rgb(240,236,226));RectFill(cv,13,8,15,12,Rgb(240,236,226));RectFill(cv,11,11,14,14,Rgb(240,236,226));RectFill(cv,11,16,14,19,Rgb(240,236,226));break;
            }
            Outline(cv,Rgb(12,16,24));iconCacheHud[k]=cv;return cv;
        }
        // ---------- fog of war as cloud banks (owner's mockup) ----------
        // A billowing cloud puff, 96 x 64, lit from the top left; puffs drawn over every unexplored tile overlap into banks.
        static readonly uint[] CloudRamp=Ramp(150,160,182,190,198,214,220,226,238,240,244,250,255,255,255);
        public static readonly uint FogBase=Rgb(222,228,238);
        static readonly Dictionary<int,PixelCanvas> puffCache=new Dictionary<int,PixelCanvas>();
        public static PixelCanvas CloudPuff(int variant)
        {
            variant&=3;PixelCanvas cv;if(puffCache.TryGetValue(variant,out cv))return cv;cv=new PixelCanvas(96,64);int s=variant*17+5;
            for(int k=0;k<9;k++)
            {
                double a=Hsh(k,s,1)*Math.PI*2,r=Hsh(k,s,2)*22;double cx=48+Math.Cos(a)*r*1.3,cy=34+Math.Sin(a)*r*.55,rx=13+Hsh(k,s,3)*12,ry=rx*.62;
                Disk(cv,cx,cy,rx,ry,CloudRamp,.18);
            }
            Disk(cv,48,30,24,15,CloudRamp,.3);
            puffCache[variant]=cv;return cv;
        }
        // ---------- vessels and ports ----------
        // kind 0 Boat: a small wooden boat with one sail; 1 Galley: a long hull with oars and a striped square sail;
        // 2 War Galley: a darker hull with a bronze ram, shields along the side and a red sail. Waterline near the bottom.
        static readonly uint[] HullWood={Rgb(70,44,26),Rgb(102,66,38),Rgb(136,92,52),Rgb(170,122,72)};
        public static PixelCanvas ShipSprite(int kind)
        {
            var cv=new PixelCanvas(64,64);int wl=56;int len=kind==0?16:26,depth=kind==0?5:7;int cx=32;
            // Wake and water line.
            for(int x=cx-len-4;x<=cx+len+4;x++){if((x+wl)%3!=0)cv.Over(x,wl+1,Rgb(214,238,250,200));if(x%4==0)cv.Over(x,wl+2,Rgb(160,214,240,160));}
            // Hull: curved bottom, raised bow (right) and stern (left).
            for(int x=cx-len;x<=cx+len;x++)
            {
                double u=(x-cx)/(double)len;int top=wl-depth-(int)Math.Round(Math.Pow(Math.Abs(u),3)*(kind==0?3:5));int bottom=wl-(int)Math.Round(Math.Pow(Math.Abs(u),2)*(depth-1));
                for(int y=top;y<=bottom;y++){int k=y==top?3:(y-top)<=1?2:y>=bottom-1?0:1;cv.Over(x,y,(kind==2&&k>0)?Shade(HullWood[k],.8):HullWood[k]);}
                if(kind>0&&(x-cx)%3==0&&Math.Abs(u)<.8)cv.Over(x,top+2,Rgb(52,34,20));
            }
            if(kind==2){for(int k=0;k<4;k++)cv.Over(cx+len+1+k,wl-2,Rgb(196,150,70));for(int x=cx-len+4;x<cx+len-3;x+=4){cv.Over(x,wl-depth-1,Rgb(200,200,210));cv.Over(x+1,wl-depth-1,Rgb(200,60,50));}}
            if(kind>0)for(int x=cx-len+3;x<cx+len-2;x+=3){cv.Over(x,wl+1,Rgb(120,86,50));cv.Over(x-1,wl+2,Rgb(120,86,50));}
            // Mast and sail.
            int mastTop=kind==0?wl-28:wl-40;RectFill(cv,cx-1,mastTop,cx+1,wl-depth,Rgb(84,56,32));
            int sw=kind==0?8:15,sh=kind==0?14:20,sy=mastTop+3;
            for(int y=sy;y<sy+sh;y++)for(int x=cx-sw;x<=cx+sw;x++)
            {
                double bulge=Math.Sin((y-sy)/(double)sh*Math.PI)*2;if(Math.Abs(x-cx)>sw-1+bulge*.5)continue;
                uint c=kind==2?Rgb(196,52,44):((kind==1&&((x-cx+sw)/5)%2==1)?Rgb(52,88,170):Rgb(240,232,212));
                if(x>cx+sw/2)c=Shade(c,.82);cv.Over(x+(int)bulge/2,y,c);
            }
            RectFill(cv,cx-sw,sy-1,cx+sw+1,sy,Rgb(84,56,32));cv.Over(cx,mastTop-1,Rgb(214,60,50));cv.Over(cx+1,mastTop-1,Rgb(214,60,50));cv.Over(cx+2,mastTop,Rgb(214,60,50));
            Outline(cv,Ink);return cv;
        }
        // A port on the coast: a timber jetty with a crane, a warehouse and stacked goods.
        static PixelCanvas portSprite;
        public static PixelCanvas PortSprite()
        {
            if(portSprite!=null)return portSprite;var cv=new PixelCanvas(SW,SH);
            RectFill(cv,AX-16,AY+4,AX+18,AY+7,Rgb(150,108,64));for(int x=AX-16;x<AX+18;x+=3)cv.Over(x,AY+5,Rgb(110,76,44));
            for(int x=AX-14;x<AX+18;x+=6)RectFill(cv,x,AY+7,x+1,AY+10,Rgb(84,56,32));
            RectFill(cv,AX-12,AY-4,AX+2,AY+4,Rgb(214,190,150));RectFill(cv,AX-2,AY-4,AX+2,AY+4,Rgb(180,156,118));RectFill(cv,AX-7,AY,AX-4,AY+4,Rgb(70,44,30));
            Gable(cv,AX-5,AY-5,16,5,RoofRed);
            RectFill(cv,AX+10,AY-14,AX+12,AY+4,Rgb(96,64,36));RectFill(cv,AX+10,AY-14,AX+20,AY-12,Rgb(96,64,36));RectFill(cv,AX+18,AY-12,AX+19,AY-6,Rgb(60,60,60));RectFill(cv,AX+16,AY-6,AX+21,AY-3,Rgb(150,110,60));
            RectFill(cv,AX+4,AY,AX+8,AY+4,Rgb(150,110,60));RectFill(cv,AX+5,AY-3,AX+8,AY,Rgb(176,132,76));
            Outline(cv,Ink);portSprite=cv;return cv;
        }
        // Outlying farmsteads on land next to a growing city: two or three houses and a tree.
        static readonly Dictionary<int,PixelCanvas> hamletCache=new Dictionary<int,PixelCanvas>();
        public static PixelCanvas Hamlet(int style,int variant)
        {
            style=Math.Max(0,Math.Min(3,style));variant&=7;int key=style*8+variant;PixelCanvas cv;
            if(hamletCache.TryGetValue(key,out cv))return cv;
            cv=new PixelCanvas(SW,SH);var spots=Spots(variant*13+style*5+2,2+variant%2,16,7,9);int i=0;
            foreach(var q in spots){int hx=(int)q[0],hy=(int)q[1];if(i==spots.Count-1&&variant%3!=0)Broadleaf(cv,hx,hy,4,Tree0);else House(cv,hx,hy,style,variant+i);i++;}
            Outline(cv,Ink);hamletCache[key]=cv;return cv;
        }
        static readonly Dictionary<int,PixelCanvas> spriteCache=new Dictionary<int,PixelCanvas>();
        // Returns null for terrain with no raised scenery.
        public static PixelCanvas Sprite(int t,int variant)
        {
            int key=t*10+variant;PixelCanvas cv;if(spriteCache.TryGetValue(key,out cv))return cv;
            switch(t)
            {
                case 1:case 13:case 11:case 19:cv=ForestSprite(t,variant);break;
                case 0:cv=GroveSprite(variant);break;
                case 2:cv=null;break; // hills are drawn as shaded relief in the ground (Compose), not as a sprite
                case 5:cv=MountainSprite(variant);break;
                case 17:cv=null;break; // volcanoes are relief in the ground (Compose)
                case 15:cv=GlacierSprite(variant);break;
                case 18:cv=RocksSprite(variant);break;
                case 9:cv=BogSprite(variant);break;
                case 16:cv=AcaciaSprite(variant);break;
                case 3:cv=null;break; // open desert, no cacti (owner request)
                case 10:cv=DeadTreeSprite(variant);break;
                case 20:cv=TuftSprite(variant);break;
                default:cv=null;break;
            }
            spriteCache[key]=cv;return cv;
        }

        // ---------- tile composition: organic, level terrain (64 x 40) ----------
        // Owner request 29 Oct 2026: terrain must blend far more naturally and there must be no step between land and sea.
        // Technique (noise-driven transition zones, as used for tile-map terrain transitions): every pixel looks up the terrain
        // at its own position pushed sideways by smooth world-space noise ("domain warping"), so borders between biomes and
        // coastlines meander across tile edges instead of following the grid. Textures are sampled in world coordinates, so a
        // grassland flows seamlessly from one tile into the next. Near a border the two terrains dither into each other; on
        // shores land gets a wet sand (or rock) lip and the water shallows and moving foam, all on one level.
        public static int Height(int t){return 0;}
        const int Margin=8;const double WarpBroad=13,WarpFine=4;
        static int[] distanceOffsets;static double[] distanceLengths;
        static void BuildOffsets(){if(distanceOffsets!=null)return;var list=new List<int[]>();for(int dy=-6;dy<=6;dy++)for(int dx=-6;dx<=6;dx++){if(dx==0&&dy==0)continue;double d=Math.Sqrt(dx*dx+dy*dy);if(d<=6.2)list.Add(new[]{dx,dy});}list.Sort((a,b)=>(a[0]*a[0]+a[1]*a[1]).CompareTo(b[0]*b[0]+b[1]*b[1]));distanceOffsets=new int[list.Count*2];distanceLengths=new double[list.Count];for(int i=0;i<list.Count;i++){distanceOffsets[i*2]=list[i][0];distanceOffsets[i*2+1]=list[i][1];distanceLengths[i]=Math.Sqrt(list[i][0]*list[i][0]+list[i][1]*list[i][1]);}}
        // The terrain seen at world pixel (gx,gy): its position warped by two octaves of noise, then looked up in the 3 x 3 block.
        static int WarpedTerrain(int[] n9,int wx,int wy,int lx,int ly)
        {
            double gx=wx*TW+lx,gy=wy*TH+ly;
            double ox=(VNoise(gx/26.0,gy/26.0,101)-.5)*2*WarpBroad+(VNoise(gx/7.0,gy/7.0,103)-.5)*2*WarpFine;
            double oy=(VNoise(gx/26.0,gy/26.0,107)-.5)*2*WarpBroad+(VNoise(gx/7.0,gy/7.0,109)-.5)*2*WarpFine;
            double sx=lx+ox,sy=ly+oy;int tx=sx<0?-1:sx>=TW?1:0,ty=sy<0?-1:sy>=TH?1:0;
            int t=n9[(ty+1)*3+tx+1];return t<0?n9[4]:t;
        }
        static double HillHeight(int gx,int gy){return .78*VNoise(gx/16.0,gy/11.5,301)+.22*VNoise(gx/6.5,gy/4.8,303);}
        static bool Relief(int t){return t==2||t==5||t==17;}
        // Mountains and volcanoes are relief in the ground like hills (owner request): no separate sprite sits on the tile.
        // Height comes from ridged noise (1 - |2n - 1|, the usual way to get sharp crests and valleys) over a massif centred on
        // each mountain tile, fading in from the range's edge; it is hill-shaded from the north-west and coloured by height:
        // brown-grey foothills, grey rock, then snow on the crests, blue-grey where the snow lies in shadow.
        static double Ridged(double x,double y,int s){return 1-Math.Abs(2*VNoise(x,y,s)-1);}
        static double Massif(int[] n9,int wx,int wy,int gx,int gy,int terrain,double rx,double ry)
        {
            double best=0;
            for(int k=0;k<9;k++){if(n9[k]!=terrain)continue;double cx=(wx+k%3-1)*TW+TW/2.0,cy=(wy+k/3-1)*TH+TH/2.0;
                double dx=(gx-cx)/rx,dy=(gy-cy)/ry,d=Math.Sqrt(dx*dx+dy*dy);best=Math.Max(best,1-d);}
            return Math.Max(0,best);
        }
        static double MountainHeight(int[] n9,int wx,int wy,int gx,int gy,double fade)
        {
            // Ridges are sampled on rotated axes so the value-noise grid never lines up with the map rows.
            double u=gx*.8+gy*.9,v=-gx*.6+gy*1.2;double r1=Ridged(u/24.0,v/18.0,401),r2=Ridged(u/8.0,v/6.5,403);
            return fade*fade*(3-2*fade)*(.55*Math.Pow(Massif(n9,wx,wy,gx,gy,5,32,32),1.3)+.35*r1*r1+.1*r2);
        }
        static readonly uint[] Foothill=Ramp(58,52,44,86,76,62,112,100,80,140,126,100,168,154,124);
        static readonly uint[] Snow={Rgb(118,136,168),Rgb(150,170,200),Rgb(196,212,232),Rgb(232,240,248),Rgb(252,253,255)};
        static uint MountainPixel(int[] n9,int wx,int wy,int gx,int gy,double edge)
        {
            double fade=Math.Min(1,(edge>98?9:edge)/9.0);
            double h=MountainHeight(n9,wx,wy,gx,gy,fade);
            double hl=MountainHeight(n9,wx,wy,gx-1,gy,fade),hr=MountainHeight(n9,wx,wy,gx+1,gy,fade),hu=MountainHeight(n9,wx,wy,gx,gy-1,fade),hd=MountainHeight(n9,wx,wy,gx,gy+1,fade);
            double light=.5+((hr-hl)+(hd-hu)*1.3)*5.2+(Bay(gx,gy)-.5)*.12;
            // Snow line wanders with noise so the caps follow the crests rather than a contour.
            // The raised peaks (MountainSprite) carry the snow; the ground beneath stays rock and foothill.
            double snowLine=.92+(VNoise(gx/9.0,gy/7.0,409)-.5)*.1;
            if(h>snowLine)return RampPick(Snow,light+.15);
            if(h>snowLine-.03&&Bay(gx,gy)<.5)return RampPick(Snow,light);
            double rock=Math.Min(1,Math.Max(0,(h-.22)/.25));
            uint c=MixC(RampPick(Foothill,light),RampPick(Rock,light),rock);
            if(rock>.5&&Hsh(gx,gy>>1,411)>.93)c=Shade(c,.8);
            return c;
        }
        // Volcanoes: a stratovolcano cone with concave flanks, a summit crater holding glowing lava, radial gullies cut down
        // the slopes, and one or two lava streams, on dark basalt.
        static uint VolcanoPixel(int[] n9,int wx,int wy,int gx,int gy,double edge)
        {
            double fade=Math.Min(1,(edge>98?7:edge)/5.0);
            double cx=0,cy=0,best=9;
            for(int k=0;k<9;k++){if(n9[k]!=17)continue;double x0=(wx+k%3-1)*TW+TW/2.0,y0=(wy+k/3-1)*TH+TH/2.0+3;double dx=(gx-x0)/24.0,dy=(gy-y0)/18.0,d0=Math.Sqrt(dx*dx+dy*dy);if(d0<best){best=d0;cx=x0;cy=y0;}}
            Func<int,int,double> height=(x,y)=>{double dx=(x-cx)/24.0,dy=(y-cy)/18.0,d=Math.Sqrt(dx*dx+dy*dy);double cone=Math.Pow(Math.Max(0,1-d),1.6);
                double a=Math.Atan2(dy,dx),gully=.5+.5*Math.Sin(a*13+VNoise(x/8.0,y/8.0,421)*4);
                double crater=d<.17?(.17-d)*2.2:0;return fade*(cone-crater-(d>.2?.05*gully*cone:0))+(1-fade)*.05*VNoise(x/5.0,y/5.0,423);};
            double h=height(gx,gy),light=.48+((height(gx+1,gy)-height(gx-1,gy))*1.2+(height(gx,gy+1)-height(gx,gy-1))*.35)*6+(Bay(gx,gy)-.5)*.12;
            double ddx=(gx-cx)/24.0,ddy=(gy-cy)/18.0,dist=Math.Sqrt(ddx*ddx+ddy*ddy),ang=Math.Atan2(ddy,ddx);
            if(fade>.6&&dist<.13)return Hsh(gx>>1,gy>>1,425)>.35?Rgb(244,118,38):Rgb(255,196,80);
            if(fade>.6&&dist<.17)return Rgb(40,30,32);
            // Lava streams: thin channels down two set bearings, glowing near the top and cooling to dark red lower down.
            int seed=(wx*31+wy*17)&7;
            foreach(double bearing in new[]{1.2+seed*.3,2.6+seed*.2})
            {double off=ang-bearing-.25*Math.Sin(dist*9);off=Math.Atan2(Math.Sin(off),Math.Cos(off));
             if(fade>.6&&dist>.15&&dist<.85&&Math.Abs(off)*dist*24<1.1)return dist<.45?Rgb(236,104,34):dist<.65?Rgb(176,62,30):Rgb(110,44,32);}
            // The south flank faces the viewer in this three-quarter view; beyond the foot of the cone it casts a shadow to the south-east.
            if(ddy>0&&dist<1)light+=.12*(1-dist);
            if(dist>1&&dist<1.25&&ddx+ddy>.9)light-=.18;
            return RampPick(VolcanoRock,Math.Max(.22,light+(h<.08?.1:0)));
        }
        static uint Ground(int t,int gx,int gy,int frame){return BasePixel(t,gx,gy,0,IsWater(t)?frame:0);}        // How near a pixel is to edge dir: 1 on that edge, 0 at the centre, negative towards the opposite edge.
        static double EdgeE(int dir,int px,int py)
        {
            double u=(px+.5-TW/2.0)/(TW/2.0),v=(py+.5-TH/2.0)/(TH/2.0);
            switch(dir){case 0:return -v;case 1:return u;case 2:return v;default:return -u;}
        }
        static readonly int[] MidX={TW/2,TW-1,TW/2,0},MidY={0,TH/2,TH-1,TH/2};
        static PixelCanvas RiverOverlay(int mask,int variant)
        {
            var cv=new PixelCanvas(TW,TH);uint wet=Rgb(40,110,160);
            var ends=new List<int>();for(int k=0;k<4;k++)if((mask&(1<<k))!=0)ends.Add(k);
            if(ends.Count==0)ends.Add(-1);
            foreach(int k in ends)
            {
                int ex=k<0?TW/2:MidX[k],ey=k<0?TH/2:MidY[k];bool vertical=k==0||k==2;
                // Each arm bends in a gentle S between the centre and the tile edge; both ends stay fixed so rivers join up.
                double bend=(3.5+variant*1.5)*(k%2==0?1:-1);
                for(int s=0;s<=48;s++)
                {
                    double t=s/48.0,x=TW/2+(ex-TW/2)*t,y=TH/2+(ey-TH/2)*t,wob=Math.Sin(t*Math.PI)*bend*Math.Cos(t*Math.PI*.9);
                    double bx=x+(vertical?wob:0),by=y+(vertical?0:wob*.8);double r=2.4+.8*Math.Sin(t*Math.PI);
                    for(int dy=-4;dy<=4;dy++)for(int dx=-4;dx<=4;dx++)if(dx*dx+dy*dy<=r*r)cv.Set((int)Math.Round(bx)+dx,(int)Math.Round(by)+dy,wet);
                }
            }
            if(ends.Count==1)for(int dy=-4;dy<=4;dy++)for(int dx=-5;dx<=5;dx++)if(dx*dx/25.0+dy*dy/16.0<=1)cv.Set(TW/2+dx,TH/2+dy,wet);
            var o=new PixelCanvas(TW,TH);
            for(int y=0;y<TH;y++)for(int x=0;x<TW;x++)
            {
                if((cv.Get(x,y)>>24)==0)continue;
                int nb=0;if((cv.Get(x+1,y)>>24)==0&&x<TW-1)nb++;if((cv.Get(x-1,y)>>24)==0&&x>0)nb++;if((cv.Get(x,y+1)>>24)==0&&y<TH-1)nb++;if((cv.Get(x,y-1)>>24)==0&&y>0)nb++;
                if(nb>0)o.Set(x,y,Rgb(24,70,110));
                else o.Set(x,y,Hsh(x,y,5)>.9?Rgb(86,164,200):((x+y)%7!=0?Rgb(52,128,176):Rgb(64,140,184)));
            }
            return o;
        }
        static readonly Dictionary<int,PixelCanvas> riverCache=new Dictionary<int,PixelCanvas>();
        static PixelCanvas River(int mask,int variant){int key=mask*10+variant;PixelCanvas cv;if(!riverCache.TryGetValue(key,out cv)){cv=RiverOverlay(mask,variant);riverCache[key]=cv;}return cv;}

        // n9: the known terrain of the 3 x 3 block centred on this tile (row by row, -1 = unknown); (wx,wy) the tile's map position.
        public static PixelCanvas Compose(int[] n9,int wx,int wy,int frame,int riverMask)
        {
            BuildOffsets();int W=TW+2*Margin,H=TH+2*Margin;var field=new int[W*H];
            for(int y=0;y<H;y++)for(int x=0;x<W;x++)field[y*W+x]=WarpedTerrain(n9,wx,wy,x-Margin,y-Margin);
            var cv=new PixelCanvas(CW,CH);
            for(int y=0;y<TH;y++)for(int x=0;x<TW;x++)
            {
                int fx=x+Margin,fy=y+Margin,t=field[fy*W+fx],gx=wx*TW+x,gy=wy*TH+y;bool water=IsWater(t);
                // Nearest pixel of the other class (land or water) and nearest pixel of another land group.
                double shore=99,border=99,hillEdge=Relief(t)?99:0;int other=-1;
                for(int k=0;k<distanceLengths.Length&&(shore>98||border>98||hillEdge>98);k++)
                {
                    int n=field[(fy+distanceOffsets[k*2+1])*W+fx+distanceOffsets[k*2]];double d=distanceLengths[k];
                    if(shore>98&&IsWater(n)!=water)shore=d;
                    if(border>98&&!water&&!IsWater(n)&&RampGroup[n]!=RampGroup[t]){border=d;other=n;}
                    if(hillEdge>98&&n!=t)hillEdge=d;
                    if(d>4.5&&!water&&shore>98&&border>98&&hillEdge<98)break;
                }
                uint c=Ground(t,gx,gy,frame);
                // Hills: shaded relief (hillshading, light from the north-west), fading in from the hills' edge so they rise
                // out of the surrounding grassland instead of sitting on it. Crests catch the light and hollows sit in shade.
                if(t==2){double m=Math.Min(1,(hillEdge>98?6:hillEdge)/5.5);double hl=HillHeight(gx-1,gy),hr=HillHeight(gx+1,gy),hu=HillHeight(gx,gy-1),hd=HillHeight(gx,gy+1),hc=HillHeight(gx,gy);
                    double shade=1+m*((hr-hl)+(hd-hu))*4.8-m*(hc<.36?.1:hc<.44?.04:0);if(m>.5&&hc>.64)c=MixC(c,Grass[4],hc>.72?.38:.22);c=Shade(c,Math.Max(.62,Math.Min(1.36,shade)));}
                else if(t==5)c=MountainPixel(n9,wx,wy,gx,gy,hillEdge);
                else if(t==17)c=VolcanoPixel(n9,wx,wy,gx,gy,hillEdge);
                if(!water)
                {
                    // Dithered blend into the neighbouring biome, strongest at the border.
                    if(other>=0&&border<=3.6&&Bay(gx,gy)<(3.9-border)/4.6)c=Ground(other,gx,gy,frame);
                    // Shore lip: wet sand (or rock on rocky, mountain and volcanic ground), level with the water.
                    // Coasts as in the gallery maps: mostly grey rock cliffs (a visible face where the sea lies to the south, a
                    // rocky rim elsewhere), sandy beaches in places and along deserts and savanna, ice at the poles.
                    if(shore<=6.2)
                    {
                        bool ice=t==14||t==15,beach=!ice&&(t==3||t==16||(t!=18&&t!=5&&t!=17&&VNoise(gx/22.0,gy/16.0,511)>.6));
                        if(ice||beach){if(shore<=3.2)c=MixC(c,ice?Ice[4]:Sand[3],shore<=1.5?.7:shore<=2.3?.42:.18);}
                        else
                        {
                            int below=0;for(int k=1;k<=6&&below==0;k++)if(IsWater(field[(fy+k)*W+fx]))below=k;
                            if(below>0)
                            {
                                double streak=(Hsh(gx,gy>>2,513)-.5)*.25+(gx%3==0?-.08:0);
                                c=below>=5?RampPick(Rock,below==6?.92:.8):RampPick(Rock,.6-(4-below)*.12+streak);if(below==1)c=Shade(c,.7);
                            }
                            else if(shore<=2.3)c=MixC(c,RampPick(Rock,.55+(Hsh(gx,gy,515)-.5)*.4),shore<=1.5?.75:.4);
                        }
                    }
                }
                else if(shore<=6.2)
                {
                    // Foam at the water's edge, moving with the frame; then a band of lighter shallows.
                    if(shore<=2.3&&Hsh((gx+frame)>>1,gy>>1,frame+7)>.22)c=shore<=1.5?Rgb(242,249,252):Rgb(192,229,242);
                    else c=MixC(c,Rgb(84,204,226),.55*(1-(shore-2)/4.2));
                }
                cv.Set(x,y,c);
            }
            if(n9[4]==7)
            {
                var ov=River(riverMask,(wx*7+wy*3)%3);
                for(int y=0;y<TH;y++)for(int x=0;x<TW;x++){uint c=ov.Get(x,y);if((c>>24)!=0)cv.Set(x,y,c);}
            }
            return cv;
        }
        // A tileable 128 x 128 sheet of soft cloud shadow (black, alpha only) that the renderer drifts slowly across the map.
        static PixelCanvas clouds;
        public static PixelCanvas CloudShadows()
        {
            if(clouds!=null)return clouds;clouds=new PixelCanvas(128,128);
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                // Periodic noise: sample on a torus so the sheet repeats without a seam.
                double a=x/128.0*Math.PI*2,b=y/128.0*Math.PI*2;
                double v=.6*VNoise(3+Math.Cos(a)*2.2,3+Math.Sin(a)*2.2+Math.Cos(b)*2.2,201)+.4*VNoise(9+Math.Sin(b)*4.4,9+Math.Cos(a)*4.4,203);
                int alpha=v>.55?(int)Math.Min(42,(v-.55)*260):0;
                clouds.Set(x,y,Rgb(10,20,30,alpha));
            }
            return clouds;
        }
        // ---------- small overlays: farm fields, roads, border edges, selection, fog, unit bases ----------
        public static PixelCanvas Farm(int variant)
        {
            // Fenced plots as in the gallery maps: mostly golden wheat with ripple rows, one green crop plot in rows,
            // a brown wooden fence with posts around each plot.
            var cv=new PixelCanvas(TW,TH);int split=TW/2-4+variant*4;
            for(int y=0;y<TH;y++)for(int x=0;x<TW;x++)
            {
                if(x<5||x>TW-6||y<5||y>TH-6)continue;
                bool fence=x==5||x==TW-6||y==5||y==TH-6||(x==split&&y>5&&y<TH-6);
                if(fence){cv.Set(x,y,(x+y)%4==0?Rgb(70,48,28):Rgb(132,96,56));continue;}
                bool wheat=variant==1?x>split:x<split;uint c;
                if(wheat){double r=Math.Sin(x*.9+y*.35)+(Hsh(x,y,61)-.5)*.8;c=r>.6?Rgb(250,216,96):r<-.7?Rgb(186,138,44):Rgb(226,184,64);}
                else{c=(y%3==0)?Rgb(58,104,40):Rgb(96,150,52);if(y%3!=0&&Hsh(x,y,63)>.85)c=Rgb(130,178,66);}
                cv.Set(x,y,c);
            }
            return cv;
        }
        // mask bits: 0 NW(-1,-1) 1 N(0,-1) 2 NE(1,-1) 3 E(1,0) 4 SE(1,1) 5 S(0,1) 6 SW(-1,1) 7 W(-1,0)
        static readonly int[] RoadX={0,TW/2,TW-1,TW-1,TW-1,TW/2,0,0},RoadY={0,0,0,TH/2,TH-1,TH-1,TH-1,TH/2};
        static readonly Dictionary<int,PixelCanvas> roadCache=new Dictionary<int,PixelCanvas>();
        public static PixelCanvas Road(int mask)
        {
            PixelCanvas cv;if(roadCache.TryGetValue(mask,out cv))return cv;
            cv=new PixelCanvas(TW,TH);
            for(int layer=0;layer<2;layer++)
            {
                uint col=layer==0?Rgb(70,52,32):Rgb(214,184,126);int rad=layer==0?2:1;
                for(int k=0;k<8;k++)
                {
                    if((mask&(1<<k))==0)continue;
                    for(int s=0;s<=24;s++){double t=s/24.0;int x=(int)(TW/2+(RoadX[k]-TW/2)*t),y=(int)(TH/2+(RoadY[k]-TH/2)*t);for(int dy=-rad;dy<=rad;dy++)for(int dx=-rad;dx<=rad;dx++)cv.Set(x+dx,y+dy,col);}
                }
                for(int dy=-rad;dy<=rad;dy++)for(int dx=-rad;dx<=rad;dx++)cv.Set(TW/2+dx,TH/2+dy,col);
            }
            roadCache[mask]=cv;return cv;
        }
        static readonly Dictionary<int,PixelCanvas> misc=new Dictionary<int,PixelCanvas>();
        // kind 0..3 = border edge N,E,S,W (white, tinted at draw time); 4 = selection outline; 5 = unexplored; 6 = dim overlay;
        // 7 = unit base ring (white, tinted with the owner's colour); 8 = unit ground shadow. 7 and 8 are 32 x 12.
        public static PixelCanvas Misc(int kind)
        {
            PixelCanvas cv;if(misc.TryGetValue(kind,out cv))return cv;
            if(kind==7||kind==8)
            {
                cv=new PixelCanvas(32,12);
                for(int y=0;y<12;y++)for(int x=0;x<32;x++)
                {
                    double u=(x+.5-16)/16.0,v=(y+.5-6)/6.0,r=u*u+v*v;if(r>1)continue;
                    if(kind==8)cv.Set(x,y,Rgb(10,16,20,(int)(120*(1-r*.6))));
                    else cv.Set(x,y,r>.62?Rgb(255,255,255,r>.86?255:220):Rgb(255,255,255,v<0?70:45));
                }
                misc[kind]=cv;return cv;
            }
            cv=new PixelCanvas(TW,TH);
            for(int y=0;y<TH;y++)for(int x=0;x<TW;x++)
            {
                // Borders glow like the gallery maps: a bright core that wanders gently along the edge (fixed at the corners
                // so neighbouring edges meet) and a soft halo fading inwards. White here; tinted with the owner's colour when drawn.
                if(kind<4)
                {
                    double along=kind%2==0?(x+.5)/TW:(y+.5)/TH,wob=.035*Math.Sin(along*Math.PI)*Math.Sin(along*Math.PI*3+kind);
                    double e=EdgeE(kind,x,y)+wob,core=1-e;
                    // Owner request: firmer borders. A wider bright core, a dark rim on its inner side, then the soft halo.
                    if(core<.075)cv.Set(x,y,Rgb(255,255,255));
                    else if(core<.105)cv.Set(x,y,Rgb(70,70,70,235));
                    else if(core<.24)cv.Set(x,y,Rgb(255,255,255,(int)(130*(1-(core-.105)/.135))));
                }
                else if(kind==4)
                {
                    double u=Math.Abs(x+.5-TW/2.0)/(TW/2.0),v=Math.Abs(y+.5-TH/2.0)/(TH/2.0);
                    if(Math.Max(u,v)>.92&&(u>.7&&v>.55||u>.97||v>.95))cv.Set(x,y,Rgb(255,222,120));
                }
                else if(kind==5)cv.Set(x,y,FogBase);
                else if(kind==9)cv.Set(x,y,Rgb(255,255,255));
                else cv.Set(x,y,Rgb(4,8,20,110));
            }
            misc[kind]=cv;return cv;
        }

        // ---------- sea resource icons (20 x 20, lit from the top left, outlined) ----------
        static readonly uint[] FishBlue=Ramp(28,64,110,44,96,150,70,130,186,110,170,214,170,214,240);
        static readonly uint[] Steel=Ramp(40,52,70,62,78,100,90,108,132,124,142,166,170,186,206);
        static readonly uint[] Slate=Ramp(30,38,52,46,58,76,66,80,100,90,104,126,124,138,158);
        static readonly uint[] PearlWhite=Ramp(150,150,170,190,190,206,220,220,232,238,238,246,255,255,255);
        static readonly uint[] ShellBrown=Ramp(90,64,44,126,94,62,160,124,84,192,158,110,222,192,146);
        static readonly uint[] CrabRed=Ramp(104,24,20,150,40,30,192,62,40,224,96,58,246,140,96);
        static readonly uint[] KelpGreen=Ramp(24,60,30,38,90,40,58,120,50,88,150,60,130,180,80);
        static readonly uint[] OysterGrey=Ramp(80,76,72,112,106,98,146,138,126,176,168,154,208,200,186);
        static readonly uint[] SealGrey=Ramp(52,54,60,80,82,90,110,112,120,144,146,152,184,186,190);
        static readonly uint[] SpongeOrange=Ramp(120,62,20,170,92,30,212,128,44,236,166,70,250,204,120);
        static readonly Dictionary<int,PixelCanvas> iconCache=new Dictionary<int,PixelCanvas>();
        public static PixelCanvas ResourceIcon(int r)
        {
            PixelCanvas cv;if(iconCache.TryGetValue(r,out cv))return cv;cv=new PixelCanvas(20,20);
            switch(r)
            {
                case 6: // fish
                    Poly(cv,new[]{P(2,6),P(6,10),P(2,14)},(x,y)=>FishBlue[1]);Disk(cv,11,10,6,3.4,FishBlue,0);cv.Set(14,9,Rgb(20,24,30));break;
                case 7: // whale tail rising from the sea
                    for(int x=1;x<19;x++)cv.Set(x,16,Rgb(120,190,230));Disk(cv,6,6,4.2,2.2,Slate,0);Disk(cv,14,6,4.2,2.2,Slate,0);RectFill(cv,9,7,12,16,Slate[2]);RectFill(cv,9,7,10,16,Slate[3]);break;
                case 8: // pearl in an open shell
                    Disk(cv,10,13,7,3.5,ShellBrown,0);Disk(cv,10,9,3.4,3.4,PearlWhite,.1);cv.Set(9,7,Rgb(255,255,255));break;
                case 9: // crab
                    for(int k=0;k<3;k++){cv.Set(4+k,13+k,CrabRed[1]);cv.Set(15-k,13+k,CrabRed[1]);}
                    Disk(cv,4,6,2.4,2.2,CrabRed,0);Disk(cv,16,6,2.4,2.2,CrabRed,0);Disk(cv,10,11,5.2,3.6,CrabRed,0);cv.Set(8,9,Rgb(20,24,30));cv.Set(12,9,Rgb(20,24,30));break;
                case 10: // kelp fronds
                    for(int s=0;s<3;s++)for(int y=2;y<19;y++){int x=(int)(5+s*5+Math.Sin(y*.55+s)*1.6);cv.Set(x,y,KelpGreen[2+s%2]);cv.Set(x+1,y,KelpGreen[1]);}
                    cv.Set(3,4,Rgb(200,230,250));cv.Set(16,6,Rgb(200,230,250));break;
                case 11: // oyster
                    Disk(cv,10,11,7.5,4.6,OysterGrey,0);for(int x=4;x<17;x+=3)for(int y=8;y<15;y++)if((cv.Get(x,y)>>24)!=0)cv.Set(x,y,OysterGrey[1]);break;
                case 12: // seal head
                    Disk(cv,10,10,5.6,5,SealGrey,0);cv.Set(8,8,Rgb(16,18,22));cv.Set(12,8,Rgb(16,18,22));cv.Set(10,11,Rgb(30,30,36));
                    for(int k=0;k<3;k++){cv.Set(6-k,12,SealGrey[4]);cv.Set(14+k,12,SealGrey[4]);}break;
                case 13: // tuna with yellow fins
                    Poly(cv,new[]{P(1,5),P(5,10),P(1,15)},(x,y)=>Steel[1]);Disk(cv,11,10,7,3.2,Steel,0);
                    Poly(cv,new[]{P(10,7),P(13,4),P(14,7)},(x,y)=>Rgb(240,200,60));Poly(cv,new[]{P(10,13),P(13,16),P(14,13)},(x,y)=>Rgb(240,200,60));cv.Set(15,9,Rgb(20,24,30));break;
                case 14: // sponge
                    Disk(cv,10,11,6.5,5.6,SpongeOrange,0);foreach(var h in new[]{new[]{8,8},new[]{12,10},new[]{9,13},new[]{13,14}}){cv.Set(h[0],h[1],SpongeOrange[0]);cv.Set(h[0]+1,h[1],SpongeOrange[0]);}break;
                case 15: // offshore oil rig and drop
                    for(int y=3;y<17;y++){int half=(y-3)/3;cv.Set(9-half,y,Rgb(60,62,70));cv.Set(10+half,y,Rgb(60,62,70));if(y%4==0)for(int x=9-half;x<=10+half;x++)cv.Set(x,y,Rgb(90,92,100));}
                    for(int x=2;x<18;x++)cv.Set(x,17,Rgb(120,190,230));Disk(cv,15,12,2,2.4,Ramp(10,10,14,20,20,26,34,34,40,52,52,60,90,90,100),0);break;
                default:iconCache[r]=null;return null;
            }
            Outline(cv,Rgb(20,24,30));iconCache[r]=cv;return cv;
        }
        // ---------- unit sprites ----------
        // The supplied 128 x 128 unit pictures are never changed on disk. As the game loads, a copy is reduced to a crisp
        // 64 x 64 sprite (area-averaged, hard alpha), given a rim light on edges facing the top-left light, shade on edges
        // facing away, and a dark outline, so units read as solid 16-bit figures. src is top-down 0xAARRGGBB.
        public static PixelCanvas UnitSprite(uint[] src,int sw,int sh)
        {
            const int Out=64,Fit=58;
            var cv=new PixelCanvas(Out,Out);
            double scale=Math.Max(sw,sh)/(double)Fit;int fw=(int)Math.Round(sw/scale),fh=(int)Math.Round(sh/scale);
            int ox=(Out-fw)/2,oy=Out-2-fh;
            for(int dy=0;dy<fh;dy++)for(int dx=0;dx<fw;dx++)
            {
                double x0=dx*scale,x1=x0+scale,y0=dy*scale,y1=y0+scale,a=0,r=0,g=0,b=0,area=0;
                for(int sy=(int)y0;sy<Math.Min(sh,(int)Math.Ceiling(y1));sy++)for(int sx=(int)x0;sx<Math.Min(sw,(int)Math.Ceiling(x1));sx++)
                {
                    double w=(Math.Min(x1,sx+1)-Math.Max(x0,sx))*(Math.Min(y1,sy+1)-Math.Max(y0,sy));if(w<=0)continue;
                    uint c=src[sy*sw+sx];double al=(c>>24)/255.0*w;
                    a+=al;r+=R(c)*al;g+=G(c)*al;b+=B(c)*al;area+=w;
                }
                if(area<=0||a/area<.42)continue;
                Func<double,int> contrast=v=>Clamp255(128+(v/a-128)*1.08);
                cv.Set(ox+dx,oy+dy,Rgb(contrast(r),contrast(g),contrast(b)));
            }
            var solid=(uint[])cv.p.Clone();
            Func<int,int,bool> open=(x,y)=>x<0||y<0||x>=Out||y>=Out||(solid[y*Out+x]>>24)==0;
            for(int y=0;y<Out;y++)for(int x=0;x<Out;x++)
            {
                uint c=solid[y*Out+x];if((c>>24)==0)continue;
                bool lit=open(x-1,y)||open(x,y-1)||open(x-1,y-1),dark=open(x+1,y)||open(x,y+1)||open(x+1,y+1);
                if(lit&&!dark)cv.Set(x,y,MixC(c,Rgb(255,248,230),.3));
                else if(dark&&!lit)cv.Set(x,y,Shade(c,.7));
            }
            Outline(cv,Rgb(20,18,24));
            return cv;
        }
    }
}