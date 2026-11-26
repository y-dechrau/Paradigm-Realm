param([string]$Src,[string]$Out)
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @"
using System;using System.Drawing;using System.Drawing.Imaging;
public static class HexWarp{
 // The farm's ground is a flat diamond; the map's tile is a pointy-topped hexagon 64 x 40 (|dy| + |dx| x 10/32 <= 20, |dx| <= 32).
 // Each output point is sent back along its own direction from the centre to the source point with the same "distance to the
 // edge": the diamond's rim lands on the hexagon's rim, straight up and straight across are unchanged, the hexagon's corners take
 // the diamond's edge. Pixels above the ground (buildings, trees) are carried along; a thin hashed fringe keeps the edge ragged.
 public static string Run(string src,string dst){
  var b=new Bitmap(src);int W=b.Width,H=b.Height;var a=new Color[W*H];for(int y=0;y<H;y++)for(int x=0;x<W;x++)a[y*W+x]=b.GetPixel(x,y);b.Dispose();
  int xl=W,xr=-1,yb=-1;for(int y=0;y<H;y++)for(int x=0;x<W;x++)if(a[y*W+x].A>40){if(x<xl)xl=x;if(x>xr)xr=x;if(y>yb)yb=y;}
  // Centre height: the middle of the opaque run in the leftmost and rightmost few columns.
  Func<int,double> mid=col=>{int lo=-1,hi=-1;for(int y=0;y<H;y++)if(a[y*W+col].A>40){if(lo<0)lo=y;hi=y;}return (lo+hi)/2.0;};
  double cyl=0,cyr=0;int n=0;for(int k=0;k<4;k++){if(xl+k<W){cyl+=mid(xl+k);n++;}}cyl/=Math.Max(1,n);n=0;for(int k=0;k<4;k++){if(xr-k>=0){cyr+=mid(xr-k);n++;}}cyr/=Math.Max(1,n);
  double cx=(xl+xr)/2.0,cy=(cyl+cyr)/2.0,ha=(xr-xl)/2.0,hb=Math.Max(4,yb-cy);int top=H;for(int y=0;y<H&&top==H;y++)for(int x=0;x<W;x++)if(a[y*W+x].A>40){top=y;break;}
  double A=Math.Sqrt(ha*hb*32.0/20.0),B=A*20.0/32.0;/* the hexagon's size splits the change of shape evenly between width and height */double above=Math.Max(0,(cy-top)*B/hb-B);int OW=(int)Math.Round(2*A),OH=(int)Math.Round(above+2*B);double ocx=A,ocy=above+B;
  var o=new Bitmap(OW,OH,PixelFormat.Format32bppArgb);
  for(int y=0;y<OH;y++)for(int x=0;x<OW;x++){
   double dx=x+.5-ocx,dy=y+.5-ocy;double nh=Math.Max(Math.Abs(dx)/A,(Math.Abs(dy)+Math.Abs(dx)*(B/2)/A)/B);
   double ux=dx*ha/A,uy=dy*hb/B;double nd=Math.Abs(dx)/A+Math.Abs(dy)/B;double f=nd<1e-6?1:nh/nd;
   // Above the ground the warp fades out over a short distance, so buildings on the back edge keep their shape.
   if(nh>1){double w=Math.Max(0,1-(nh-1)/.35);f=1+(f-1)*w;}
   int sx=(int)Math.Floor(cx+ux*f),sy=(int)Math.Floor(cy+uy*f);if(sx<0||sy<0||sx>=W||sy>=H){continue;}
   var c=a[sy*W+sx];if(c.A<=40)continue;double sd=Math.Abs(sx-cx)/ha+Math.Abs(sy-cy)/hb;
   // Ground beyond the hexagon is dropped; a hashed fringe just inside the rim leaves the edge ragged like the old fields.
   if(nh>1&&(dy>=0||sd<=1.02))continue;
   uint h=(uint)(x*73856093^y*19349663);h^=h>>13;h*=0x5bd1e995;h^=h>>15;double r=(h&1023)/1023.0;
   if(nh>.9&&nh<=1&&sd<=1.05&&r<(nh-.9)/.1*.7)continue;
   o.SetPixel(x,y,Color.FromArgb(255,c.R,c.G,c.B));}
  /* Fill pinholes left where edge ground was dropped under a roof: a clear pixel with at least five filled neighbours takes their average. */
  for(int pass=0;pass<2;pass++){var fill=new System.Collections.Generic.List<int[]>();for(int y=1;y<OH-1;y++)for(int x=1;x<OW-1;x++){if(o.GetPixel(x,y).A!=0)continue;int k=0,r2=0,g2=0,b2=0;for(int j=-1;j<=1;j++)for(int i=-1;i<=1;i++){if(i==0&&j==0)continue;var q=o.GetPixel(x+i,y+j);if(q.A==0)continue;k++;r2+=q.R;g2+=q.G;b2+=q.B;}if(k>=5)fill.Add(new[]{x,y,r2/k,g2/k,b2/k});}foreach(var p in fill)o.SetPixel(p[0],p[1],Color.FromArgb(255,p[2],p[3],p[4]));}
  o.Save(dst,ImageFormat.Png);string info=OW+"x"+OH+" (ground "+(int)(2*ha)+"x"+(int)(2*hb)+", above "+(int)above+")";o.Dispose();return info;}
}
"@
[HexWarp]::Run($Src,$Out)
