using System;
using System.Linq;
namespace ParadigmRealm {
// Civilisation flags (owner request, 31 Oct 2026; save version 20): a layout, two field colours, an emblem and its colour,
// chosen from the palette below. The player designs theirs on the Identity screen; rivals get one generated from their seed.
// Flags are drawn as 48 x 32 pixel art, with a dark rim, and fly on army banners, city plates and the diplomacy screen.
public static class FlagArt {
 public const int W=48,H=32;
 public static readonly string[] Patterns={"Plain","Two bands","Two halves","Three bands","Tricolour","Cross","Nordic cross","Saltire","Quartered","Border","Diagonal","Chevron","Stripes","Canton"};
 public static readonly string[] Emblems={"None","Star","Sun","Moon","Leaf","Crown","Tower","Cross","Diamond","Ring","Tree","Anchor","Sword","Wheel","Mountain","Waves"};
 public static readonly string[] ColourNames={"Crimson","Scarlet","Orange","Gold","Yellow","Lime","Green","Forest","Teal","Sky","Royal blue","Navy","Purple","Plum","White","Black"};
 static readonly int[] Palette={0x9B1B30,0xD7263D,0xF46036,0xE0A526,0xF5D547,0x9BC53D,0x3E9A47,0x1E5631,0x1B998B,0x4CA8DE,0x2448A8,0x14213D,0x6A3FA0,0x8E3B6E,0xF4F1E8,0x1A1A1A};
 public static int Hex(int i){return Palette[Math.Max(0,Math.Min(Palette.Length-1,i))];}
 // Owner slots to palette colours, close to each civilisation's map colour (RealmGame.nations).
 public static readonly int[] OwnerColour={6,2,9,1,12,4,8,13,5,3,10,5,14,0,11,2};
 public static uint Colour(int i){int v=Palette[Math.Max(0,Math.Min(Palette.Length-1,i))];return PixelArt.Rgb((v>>16)&255,(v>>8)&255,v&255);}
 public static PixelCanvas Draw(int pattern,int a,int b,int emblem,int ink)
 {
  var cv=new PixelCanvas(W,H);uint A=Colour(a),B=Colour(b),E=Colour(ink);
  for(int y=0;y<H;y++)for(int x=0;x<W;x++)
  {
   double u=(x+.5)/W,v=(y+.5)/H;bool second;
   switch(pattern)
   {
    case 1:second=v>.5;break;
    case 2:second=u>.5;break;
    case 3:second=v>1/3.0&&v<2/3.0;break;
    case 4:second=u>1/3.0&&u<2/3.0;break;
    case 5:second=Math.Abs(u-.5)<.09||Math.Abs(v-.5)<.13;break;
    case 6:second=Math.Abs(u-.36)<.08||Math.Abs(v-.5)<.12;break;
    case 7:second=Math.Abs(u-v)<.12||Math.Abs(u-(1-v))<.12;break;
    case 8:second=(u<.5)!=(v<.5);break;
    case 9:second=u<.1||u>.9||v<.14||v>.86;break;
    case 10:second=v>u;break;
    case 11:second=u<.45*(1-Math.Abs(v-.5)*2);break;
    case 12:second=((int)(v*5))%2==1;break;
    case 13:second=u<.45&&v<.5;break;
    default:second=false;break;
   }
   cv.Set(x,y,second?B:A);
   if(emblem>0&&EmblemAt(emblem,x+.5-W/2.0,y+.5-H/2.0))cv.Set(x,y,E);
  }
  // A dark rim and a soft fold shade so the flag reads at banner size.
  uint rim=PixelArt.Rgb(20,16,12);for(int x=0;x<W;x++){cv.Set(x,0,rim);cv.Set(x,H-1,rim);}for(int y=0;y<H;y++){cv.Set(0,y,rim);cv.Set(W-1,y,rim);}
  for(int y=1;y<H-1;y++)for(int x=1;x<W-1;x++){double fold=Math.Sin(x*.32)*.07;uint c=cv.Get(x,y);int r=(int)((c>>16)&255),g=(int)((c>>8)&255),bl=(int)(c&255);double k=1+fold;cv.Set(x,y,PixelArt.Rgb(Clamp(r*k),Clamp(g*k),Clamp(bl*k)));}
  return cv;
 }
 static int Clamp(double v){return Math.Max(0,Math.Min(255,(int)v));}
 // Emblems are shapes about 20 pixels across, centred on the flag (x, y measured from the centre).
 static bool EmblemAt(int e,double x,double y)
 {
  double r=Math.Sqrt(x*x+y*y),a=Math.Atan2(y,x);
  switch(e)
  {
   case 1:{double ang=((a+Math.PI/2)%(2*Math.PI/5)+2*Math.PI/5)%(2*Math.PI/5);return r<9*Star(ang);}
   case 2:return r<4.5||r<9&&Math.Abs(Math.Sin(a*4))>.82;
   case 3:return r<8&&Math.Sqrt((x-3.5)*(x-3.5)+(y+1)*(y+1))>6.2;
   case 4:{double rx=x*.8+y*.6,ry=-x*.6+y*.8;return rx*rx/64+ry*ry/16<1||Math.Abs(ry)<.8&&rx>-10&&rx<0;}
   case 5:return y>-1&&y<5&&Math.Abs(x)<8||y<=-1&&y>-7&&Math.Abs(x)<8&&(Math.Abs(x)>5.5||Math.Abs(x)<1.5);
   case 6:return Math.Abs(x)<5&&y>-4&&y<8||y>-8&&y<=-4&&Math.Abs(x)<6.5&&((int)Math.Floor((x+6.5)/2.6))%2==0;
   case 7:return Math.Abs(x)<2&&Math.Abs(y)<8||Math.Abs(y+1.5)<2&&Math.Abs(x)<6;
   case 8:return Math.Abs(x)/7+Math.Abs(y)/9<1;
   case 9:return r<8.5&&r>5.2;
   case 10:return y<4&&y>-9&&Math.Abs(x)<(y+9)*.55||Math.Abs(x)<1.5&&y>=4&&y<8;
   case 11:return Math.Abs(x)<1.2&&y>-7&&y<6||Math.Abs(y+4)<1&&Math.Abs(x)<4.5||r>5.5&&r<7.3&&y>1.5||Math.Sqrt(x*x+(y+8)*(y+8))<2.2&&Math.Sqrt(x*x+(y+8)*(y+8))>1;
   case 12:return Math.Abs(x)<1.5&&y>-9&&y<4||Math.Abs(y-4)<1.2&&Math.Abs(x)<5||Math.Abs(x)<1&&y>=4&&y<8;
   case 13:return r<8.5&&r>6.8||r<2.2||r<7&&Math.Abs(Math.Sin(a*3))<.18;
   case 14:return y<7&&y>-7&&Math.Abs(x+2)<(y+7)*.6||y<7&&y>-3&&Math.Abs(x-5)<(y+3)*.6;
   case 15:return Math.Abs(y-Math.Sin(x*.55)*2.2)<1.6&&Math.Abs(x)<10||Math.Abs(y-5-Math.Sin(x*.55)*2.2)<1.6&&Math.Abs(x)<10||Math.Abs(y+5-Math.Sin(x*.55)*2.2)<1.6&&Math.Abs(x)<10;
  }
  return false;
 }
 // Radius of a five-pointed star at angle ang inside one point's sector (0..72 degrees), 1 at the tips, about .4 between.
 static double Star(double ang){double t=Math.Abs(ang-Math.PI/5)/(Math.PI/5);return .4+.6*t;}
 // Rival flags: layout, colours and emblem from the seed, with the civilisation's map colour as the first field colour.
 public static int[] Generated(int seed,int owner,int nearestColour)
 {
  var r=new Random(seed*31+owner*977+7);int a=nearestColour,b=(a+3+r.Next(Palette.Length-4))%Palette.Length;if(b==a)b=(a+8)%Palette.Length;
  int ink=new[]{14,3,15,4}.FirstOrDefault(c=>c!=a&&c!=b);return new[]{r.Next(Patterns.Length),a,b,1+r.Next(Emblems.Length-1),ink};
 }
}
}
namespace ParadigmRealm {
public partial class World {
 public void EnsureFlags(){
  if(nations==null)return;
  for(int o=0;o<nations.Length;o++){var n=nations[o];if(n==null||n.flagPattern>=0)continue;
   if(o==0){n.flagPattern=0;n.flagA=FlagArt.OwnerColour[0];n.flagB=14;n.flagEmblem=flag==1?1:flag==2?2:4;n.flagInk=3;}
   else{var g=FlagArt.Generated(seed,o,FlagArt.OwnerColour[o%FlagArt.OwnerColour.Length]);n.flagPattern=g[0];n.flagA=g[1];n.flagB=g[2];n.flagEmblem=g[3];n.flagInk=g[4];}}
 }
 bool FlagsValid(){return nations.All(n=>n!=null&&n.flagPattern>=0&&n.flagPattern<FlagArt.Patterns.Length&&n.flagEmblem>=0&&n.flagEmblem<FlagArt.Emblems.Length&&new[]{n.flagA,n.flagB,n.flagInk}.All(c=>c>=0&&c<FlagArt.ColourNames.Length));}
}
}
