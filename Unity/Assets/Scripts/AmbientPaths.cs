using System;
using System.Collections.Generic;
namespace ParadigmRealm {
// The centre lines of the roads and rivers as they are painted on a tile, for things that travel along them (the living
// countryside, 26 Nov 2026; AmbientLife.cs). The same curves PaintRoad and RiverChannel lay their paint on, so a cart keeps to the
// road and a mark on the water follows the bend. Points are in tile-picture pixels (hexagon: HW x HH; others: TW x TH before the
// diamond squeeze, so only hexagon tiles use them).
public static partial class PixelArt {
 // From the edge of arm k to the middle of the road on this tile. look: the road's wandering (bits 0-1). river: null on dry land.
 static readonly Dictionary<string,List<double[]>> roadArms=new Dictionary<string,List<double[]>>();
 public static List<double[]> HexRoadArm(int mask,int arm,int look,int riverMask,int variant,int shifted){
  string key=mask+"."+arm+"."+look+"."+riverMask+"."+variant+"."+shifted;List<double[]> pts;if(roadArms.TryGetValue(key,out pts))return pts;
  if(roadArms.Count>600)roadArms.Clear();
  double cx=HW/2.0,cy=HH/2.0,squash=1.25;var ex=new double[6];var ey=new double[6];for(int k=0;k<6;k++){ex[k]=cx+HexEdgeX[k];ey[k]=cy+HexEdgeY[k];}
  var arms=new List<int>();for(int k=0;k<6;k++)if((mask&(1<<k))!=0)arms.Add(k);pts=new List<double[]>();
  if(riverMask!=0){
   bool flowKnown=(riverMask>>RiverDownShift&7)!=0;
   for(int k=0;k<6;k++)if((shifted&(1<<k))!=0){double vx=ex[k]-cx,vy=ey[k]-cy,vl=Math.Sqrt(vx*vx+vy*vy),sign=flowKnown?((shifted&(256<<k))!=0?1:-1):k<3?1:-1;ex[k]+=-vy/vl*RoadBankShift*sign;ey[k]+=vx/vl*RoadBankShift*sign/squash;}
   var river=HexRiver(riverMask,variant);var dist=RiverDistance(river);double fx,fy,room;DrySpot(true,riverMask,variant,out fx,out fy,out room);
   Func<int,int,bool> on=(x,y)=>InHex(x+.5-HW/2.0,y+.5-HH/2.0,HW,-1.5);
   if(arms.Count==1)pts=BankPath(dist,on,HW,HH,ex[arm],ey[arm],fx*HW,fy*HH,squash);
   else{int a0,a1;ThroughPair(arms,ex,ey,cx,cy,out a0,out a1);var main=BankPath(dist,on,HW,HH,ex[a0],ey[a0],ex[a1],ey[a1],squash);int mid=main.Count/2;
    if(arm==a0)pts=main.GetRange(0,mid+1);else if(arm==a1){pts=main.GetRange(mid,main.Count-mid);pts.Reverse();}
    else pts=BankPath(dist,on,HW,HH,ex[arm],ey[arm],main[mid][0],main[mid][1],squash);}
  }else{
   double sway=((look&1)==0?1:-1)*((look&2)==0?2.3:3.3);
   Action<double,double,double,double,double,double,double,double> curve=(x0,y0,x1,y1,x2,y2,from,to)=>{for(int i=0;i<=20;i++){double t=from+(to-from)*i/20.0,a=(1-t)*(1-t),b=2*t*(1-t),c=t*t,x=a*x0+b*x1+c*x2,y=a*y0+b*y1+c*y2,tx=2*(1-t)*(x1-x0)+2*t*(x2-x1),ty=2*(1-t)*(y1-y0)+2*t*(y2-y1),tl=Math.Sqrt(tx*tx+ty*ty),sw=Math.Sin(t*Math.PI);sw=sw*sw*Math.Sin(t*Math.PI*2)*sway;if(tl>0){x+=-ty/tl*sw;y+=tx/tl*sw/squash;}pts.Add(new[]{x,y});}};
   if(arms.Count<=1)curve(ex[arm],ey[arm],(ex[arm]+cx)/2,(ey[arm]+cy)/2,cx,cy,0,1);
   else{int a0,a1;ThroughPair(arms,ex,ey,cx,cy,out a0,out a1);double mx=.25*ex[a0]+.5*cx+.25*ex[a1],my=.25*ey[a0]+.5*cy+.25*ey[a1];
    if(arm==a0)curve(ex[a0],ey[a0],cx,cy,ex[a1],ey[a1],0,.5);else if(arm==a1)curve(ex[a0],ey[a0],cx,cy,ex[a1],ey[a1],1,.5);
    else curve(ex[arm],ey[arm],(ex[arm]+cx)/2,(ey[arm]+cy)/2,mx,my,0,1);}
  }
  roadArms[key]=pts;return pts;}
 static void ThroughPair(List<int> arms,double[] ex,double[] ey,double cx,double cy,out int a0,out int a1){
  a0=arms[0];a1=arms[Math.Min(1,arms.Count-1)];double best=9;
  foreach(int i in arms)foreach(int j in arms){if(j<=i)continue;double ix=ex[i]-cx,iy=ey[i]-cy,jx=ex[j]-cx,jy=ey[j]-cy,dot=(ix*jx+iy*jy)/Math.Sqrt((ix*ix+iy*iy)*(jx*jx+jy*jy));if(dot<best){best=dot;a0=i;a1=j;}}}
 // The trunk of a river on a hexagon tile, upstream to downstream, or null when its outflow is not known.
 static readonly Dictionary<int,List<double[]>> riverLines=new Dictionary<int,List<double[]>>();
 public static List<double[]> HexRiverLine(int mask,int variant){
  int key=mask*10+variant;List<double[]> pts;if(riverLines.TryGetValue(key,out pts))return pts;if(riverLines.Count>600)riverLines.Clear();
  if(!RiverTree(mask,6)){riverLines[key]=null;return null;}
  double cx=HW/2.0,cy=HH/2.0,squash=1.22;var ex=new double[6];var ey=new double[6];for(int k=0;k<6;k++){ex[k]=cx+HexEdgeX[k];ey[k]=cy+HexEdgeY[k];}
  int down=((mask>>RiverDownShift)&7)-1;var ups=new List<int>();for(int k=0;k<6;k++)if(k!=down&&(mask&(1<<k))!=0)ups.Add(k);
  Func<int,double> rad=k=>RiverRadius[(mask>>(RiverWidthShift+2*k))&3];ups.Sort((a,b)=>{int c=rad(b).CompareTo(rad(a));return c!=0?c:a.CompareTo(b);});
  double ox=ex[down],oy=ey[down],odx=cx-ox,ody=cy-oy,ol=Math.Sqrt(odx*odx+ody*ody);odx/=ol;ody/=ol;double sx0,sy0,c1x,c1y;
  if(ups.Count==0){sx0=cx+odx*5;sy0=cy+ody*5;c1x=sx0;c1y=sy0;}else{int m=ups[0];sx0=ex[m];sy0=ey[m];c1x=sx0+(cx-sx0)*.55;c1y=sy0+(cy-sy0)*.55;}
  double c2x=ox+odx*ol*.55,c2y=oy+ody*ol*.55;const int N=64;var bx=new double[N+1];var by=new double[N+1];
  for(int i=0;i<=N;i++){double t=i/(double)N,a=(1-t)*(1-t)*(1-t),b=3*t*(1-t)*(1-t),c=3*t*t*(1-t),d=t*t*t;bx[i]=a*sx0+b*c1x+c*c2x+d*ox;by[i]=a*sy0+b*c1y+c*c2y+d*oy;}
  double bump=(1.6+variant*.9)*(((down+variant)&1)==0?1:-1);pts=new List<double[]>();
  for(int i=0;i<=N;i++){double t=i/(double)N,gx=bx[Math.Min(N,i+1)]-bx[Math.Max(0,i-1)],gy=by[Math.Min(N,i+1)]-by[Math.Max(0,i-1)],gl=Math.Sqrt(gx*gx+gy*gy),w=Math.Sin(t*Math.PI);w=w*w*Math.Sin(t*Math.PI*2)*bump;pts.Add(new[]{bx[i]+(gl>0?-gy/gl*w:0),by[i]+(gl>0?gx/gl*w/squash:0)});}
  riverLines[key]=pts;return pts;}
 // The radius of the water at a river tile's outflow, in tile pixels.
 public static double HexRiverRadius(int mask){int down=((mask>>RiverDownShift)&7)-1;return down<0?1:RiverRadius[(mask>>(RiverWidthShift+2*down))&3];}
 public static bool HexInside(int x,int y){return InHex(x+.5-HW/2.0,y+.5-HH/2.0,HW,0);}
 // Where the water is on a hexagon tile, at twice the tile's pixels: true for the river's own pixels.
 public static bool HexRiverWet(int mask,int variant,int x2,int y2){var river=HexRiver(mask,variant);return (river.Get(x2/2,y2/2)>>24)!=0;}
 // A point of a line a given distance along it, and the way it runs there.
 public static double[] AlongLine(List<double[]> line,double distance,out double dx,out double dy){
  dx=1;dy=0;if(line==null||line.Count==0)return new double[]{0,0};double left=Math.Max(0,distance);
  for(int i=1;i<line.Count;i++){double sx=line[i][0]-line[i-1][0],sy=line[i][1]-line[i-1][1],l=Math.Sqrt(sx*sx+sy*sy);if(l<=0)continue;dx=sx/l;dy=sy/l;if(left<=l)return new[]{line[i-1][0]+dx*left,line[i-1][1]+dy*left};left-=l;}
  var end=line[line.Count-1];return new[]{end[0],end[1]};}
 public static double LineLength(List<double[]> line){double n=0;if(line!=null)for(int i=1;i<line.Count;i++){double sx=line[i][0]-line[i-1][0],sy=line[i][1]-line[i-1][1];n+=Math.Sqrt(sx*sx+sy*sy);}return n;}
}
}
