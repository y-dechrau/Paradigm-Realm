using System;
using System.Linq;
namespace ParadigmRealm {
// Civilisation flags (owner request, 31 Oct 2026; save version 20; much extended 9 Nov 2026, save version 39): a layout of up to
// three field colours, an emblem (a drawn shape or one of the painted heraldic charges) with its colour, size, position and an
// optional outline. The player designs theirs under Empire > Flag and identity; rivals get one generated from their seed.
// Flags are drawn as 96 x 64 pixel art, with a dark rim, and fly on army banners, city plates and the diplomacy screen.
public static class FlagArt {
 public const int W=96,H=64,ChargeCount=32;
 public static readonly string[] Patterns={"Plain","Two bands","Two halves","Three bands","Tricolour","Cross","Nordic cross","Saltire","Quartered","Border","Diagonal","Chevron","Stripes","Canton",
  "Pale","Bend","Hoist triangle","Pall","Lozenge","Disc","Sun rays","Chequy","Four stripes","Per saltire","Bordered cross"};
 public static readonly string[] Emblems={"None","Star","Sun","Moon","Leaf","Crown","Tower","Cross","Diamond","Ring","Tree","Anchor","Sword","Wheel","Mountain","Waves"};
 // The painted heraldic charges (Resources/Sprites/charge-01..32), in sheet order.
 public static readonly string[] Charges={"None","Lion","Eagle","Dragon","Horse","Wolf","Bear","Stag","Bull","Serpent","Owl","Phoenix","Fish","Ship","Castle","Crossed swords","Axe",
  "Bow","Hammer","Book","Key","Scroll","Skull","Hand","Eye","Rose","Lotus","Oak","Wheat","Grapes","Lightning","Flame","Comet"};
 public static readonly string[] Sizes={"Medium","Small","Large"},Positions={"Centre","Hoist","Canton","Fly"};
 public static readonly string[] ColourNames={"Crimson","Scarlet","Orange","Gold","Yellow","Lime","Green","Forest","Teal","Sky","Royal blue","Navy","Purple","Plum","White","Black",
  "Maroon","Rose","Peach","Bronze","Brown","Sand","Olive","Mint","Turquoise","Steel","Indigo","Lavender","Magenta","Silver","Grey","Ivory"};
 static readonly int[] Palette={0x9B1B30,0xD7263D,0xF46036,0xE0A526,0xF5D547,0x9BC53D,0x3E9A47,0x1E5631,0x1B998B,0x4CA8DE,0x2448A8,0x14213D,0x6A3FA0,0x8E3B6E,0xF4F1E8,0x1A1A1A,
  0x5C0F1E,0xE87AA0,0xF6B48F,0xA8692E,0x6B4226,0xD9C38C,0x7A7D2C,0x8FD6B4,0x2EC4C4,0x5B7083,0x34287A,0xA990DD,0xC2177D,0xC0C4C8,0x6E6E6E,0xFFF7DC};
 public static int Hex(int i){return Palette[Math.Max(0,Math.Min(Palette.Length-1,i))];}
 // Owner slots to palette colours, close to each civilisation's map colour (RealmGame.nations).
 public static readonly int[] OwnerColour={6,2,9,1,12,4,8,13,5,3,10,5,14,0,11,2};
 public static uint Colour(int i){int v=Palette[Math.Max(0,Math.Min(Palette.Length-1,i))];return PixelArt.Rgb((v>>16)&255,(v>>8)&255,v&255);}
 // Which field (0, 1 or 2) a point of the flag belongs to; u, v run 0..1 across and down.
 public static int Field(int pattern,double u,double v){
  switch(pattern){
   case 1:return v>.5?1:0;
   case 2:return u>.5?1:0;
   case 3:return v<1/3.0?0:v<2/3.0?1:2;
   case 4:return u<1/3.0?0:u<2/3.0?1:2;
   case 5:return Math.Abs(u-.5)<.09||Math.Abs(v-.5)<.13?1:0;
   case 6:return Math.Abs(u-.36)<.08||Math.Abs(v-.5)<.12?1:0;
   case 7:return Math.Abs(u-v)<.12||Math.Abs(u-(1-v))<.12?1:0;
   case 8:return (u<.5)!=(v<.5)?1:0;
   case 9:return u<.1||u>.9||v<.14||v>.86?1:0;
   case 10:return v>u?1:0;
   case 11:return u<.45*(1-Math.Abs(v-.5)*2)?1:0;
   case 12:return ((int)(v*5))%2==1?1:0;
   case 13:return u<.45&&v<.5?1:0;
   case 14:return Math.Abs(u-.5)<.17?1:0;
   case 15:return u+v>1?1:0;
   case 16:return u<.5*(1-Math.Abs(v-.5)*2)?2:v>.5?1:0;
   case 17:return (u<.5&&(Math.Abs(v-u)<.09||Math.Abs(v-(1-u))<.09))||(u>=.45&&Math.Abs(v-.5)<.09)?1:(u<.5&&v>u&&v<1-u?2:0);
   case 18:return Math.Abs(u-.5)/.5+Math.Abs(v-.5)/.5<1?1:0;
   case 19:{double dx=(u-.5)*1.5,dy=v-.5;return dx*dx+dy*dy<.09?1:0;}
   case 20:{double a=Math.Atan2(v-.5,(u-.5)*1.5)+Math.PI;return ((int)(a/(Math.PI/8)))%2==1?1:0;}
   case 21:return ((int)(u*6)+(int)(v*4))%2==1?1:0;
   case 22:return ((int)(u*4))%2==1?1:0;
   case 23:return Math.Abs(v-.5)*1.5>Math.Abs(u-.5)?(v>.5?2:1):0;
   case 24:return u<.08||u>.92||v<.12||v>.88?2:Math.Abs(u-.5)<.08||Math.Abs(v-.5)<.11?1:0;
   default:return 0;
  }
 }
 // Where the emblem sits and how big it is: its centre in pixels and its scale against the 48 x 32 design size.
 public static void EmblemBox(int size,int pos,out double cx,out double cy,out double k){
  k=(W/48.0)*(size==1?.7:size==2?1.35:1.0);cx=W*(pos==1?.25:pos==2?.23:pos==3?.75:.5);cy=H*(pos==2?.3:.5);
 }
 public static PixelCanvas Draw(int pattern,int a,int b,int emblem,int ink){return Draw(pattern,a,b,a,emblem,ink,0,0,0);}
 public static PixelCanvas Draw(int pattern,int a,int b,int c,int emblem,int ink,int size,int pos,int outline,bool[] charge=null,float[] shade=null)
 {
  var cv=new PixelCanvas(W,H);var fields=new[]{Colour(a),Colour(b),Colour(c)};uint E=Colour(ink);double cx,cy,k;EmblemBox(size,pos,out cx,out cy,out k);
  var mask=new bool[W*H];
  for(int y=0;y<H;y++)for(int x=0;x<W;x++)
  {
   double u=(x+.5)/W,v=(y+.5)/H;cv.Set(x,y,fields[Field(pattern,u,v)]);
   bool on=charge!=null?charge[y*W+x]:emblem>0&&EmblemAt(emblem,(x+.5-cx)/k,(y+.5-cy)/k);
   if(on){mask[y*W+x]=true;uint col=E;if(shade!=null){float s=shade[y*W+x];col=PixelArt.Rgb(Clamp(((E>>16)&255)*s),Clamp(((E>>8)&255)*s),Clamp((E&255)*s));}cv.Set(x,y,col);}
  }
  // An outline round the emblem, dark on light emblems and light on dark ones, so it stands out on any field.
  if(outline>0){var ink3=new[]{(E>>16)&255,(E>>8)&255,E&255};uint line=ink3[0]+ink3[1]+ink3[2]>380?PixelArt.Rgb(24,20,16):PixelArt.Rgb(240,232,214);
   for(int y=1;y<H-1;y++)for(int x=1;x<W-1;x++){if(mask[y*W+x])continue;if(mask[y*W+x-1]||mask[y*W+x+1]||mask[(y-1)*W+x]||mask[(y+1)*W+x])cv.Set(x,y,line);}}
  // A dark rim and a soft fold shade so the flag reads at banner size.
  uint rim=PixelArt.Rgb(20,16,12);for(int x=0;x<W;x++){cv.Set(x,0,rim);cv.Set(x,H-1,rim);}for(int y=0;y<H;y++){cv.Set(0,y,rim);cv.Set(W-1,y,rim);}
  for(int y=1;y<H-1;y++)for(int x=1;x<W-1;x++){double fold=Math.Sin(x*.16)*.07;uint col=cv.Get(x,y);int r=(int)((col>>16)&255),g=(int)((col>>8)&255),bl=(int)(col&255);double f=1+fold;cv.Set(x,y,PixelArt.Rgb(Clamp(r*f),Clamp(g*f),Clamp(bl*f)));}
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
   if(o==0){n.flagPattern=0;n.flagA=FlagArt.OwnerColour[0];n.flagB=14;n.flagEmblem=flag==1?1:flag==2?2:4;n.flagInk=3;n.flagC=n.flagA;}
   else{var g=FlagArt.Generated(seed,o,FlagArt.OwnerColour[o%FlagArt.OwnerColour.Length]);n.flagPattern=g[0];n.flagA=g[1];n.flagB=g[2];n.flagEmblem=g[3];n.flagInk=g[4];n.flagC=(g[1]+5+o)%FlagArt.ColourNames.Length;if(n.flagC==g[2])n.flagC=g[1];}}
 }
 bool FlagsValid(){return nations.All(n=>n!=null&&n.flagPattern>=0&&n.flagPattern<FlagArt.Patterns.Length&&n.flagEmblem>=0&&n.flagEmblem<FlagArt.Emblems.Length&&new[]{n.flagA,n.flagB,n.flagC,n.flagInk}.All(c=>c>=0&&c<FlagArt.ColourNames.Length)&&n.flagSize>=0&&n.flagSize<3&&n.flagPos>=0&&n.flagPos<4&&n.flagCharge>=0&&n.flagCharge<=FlagArt.ChargeCount&&(n.flagOutline==0||n.flagOutline==1));}
}
}
