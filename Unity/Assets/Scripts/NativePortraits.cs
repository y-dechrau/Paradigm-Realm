using System;
namespace ParadigmRealm {
// Drawn portraits for the native peoples of the parallel worlds (owner request, 31 Oct 2026), used until painted ones are
// supplied as Resources/Leaders/leader-29.png ... leader-38.png. Each is a 150 x 200 bust against a sky of its world, with the
// features of its people. k: 0 elf, 1 avian, 2 machine, 3 dwarf, 4 demon, 5 orc, 6 ratkin, 7 myconid, 8 merfolk, 9 lizardfolk.
public static partial class PixelArt {
 static void Ellipse(PixelCanvas cv,double cx,double cy,double rx,double ry,Func<double,double,uint> paint){for(int y=(int)(cy-ry);y<=cy+ry;y++)for(int x=(int)(cx-rx);x<=cx+rx;x++){double dx=(x-cx)/rx,dy=(y-cy)/ry;if(dx*dx+dy*dy<=1)cv.Set(x,y,paint(dx,dy));}}
 static uint Lit(uint[] ramp,double dx,double dy){return RampPick(ramp,.78-.3*dx-.25*dy);}
 public static PixelCanvas NativePortrait(int k)
 {
  int W=150,H=200;var cv=new PixelCanvas(W,H);
  uint[][] skies={Ramp(20,60,40,40,110,80,90,170,120,170,220,170,230,250,220),Ramp(40,70,120,70,120,180,120,170,220,190,215,240,240,245,250),Ramp(40,36,34,80,72,66,130,118,104,180,166,148,225,215,200),Ramp(40,34,30,90,70,52,150,110,70,210,160,100,245,215,170),
   Ramp(30,5,5,90,15,10,170,40,15,235,100,30,255,190,90),Ramp(40,12,8,100,30,15,170,70,25,220,130,50,250,200,120),Ramp(30,40,20,60,80,35,100,120,50,150,160,80,200,200,140),Ramp(35,30,45,70,60,85,120,100,130,170,150,170,215,200,215),
   Ramp(5,30,60,10,60,110,30,110,170,90,170,220,180,225,250),Ramp(15,50,35,30,95,60,60,145,90,140,200,130,220,240,190)};
  uint[][] skins={Ramp(150,120,100,200,170,150,230,205,185,245,228,212,255,245,235),Ramp(80,55,35,130,95,60,175,140,95,215,190,150,245,235,215),Ramp(50,55,60,90,96,104,135,142,150,180,186,194,225,230,236),Ramp(110,70,50,160,110,80,200,150,115,230,185,150,250,215,185),
   Ramp(90,10,10,150,25,20,200,50,35,235,90,60,255,150,120),Ramp(30,60,25,55,100,40,85,140,55,125,175,80,175,210,120),Ramp(55,52,55,95,90,92,140,134,134,185,178,176,225,220,215),Ramp(150,140,120,195,185,165,225,215,195,240,235,220,252,250,240),
   Ramp(20,70,80,40,115,125,70,160,165,120,200,195,185,235,225),Ramp(25,70,35,45,110,50,70,150,70,120,190,100,180,225,150)};
  uint[][] cloth={Ramp(20,60,30,30,95,45,45,130,60,80,170,90,150,210,150),Ramp(70,50,30,110,80,45,150,115,65,190,155,95,225,200,150),Ramp(40,44,50,70,76,84,105,112,122,150,156,166,200,205,212),Ramp(60,40,25,100,65,40,140,95,55,180,130,80,215,175,120),
   Ramp(20,10,15,45,20,25,75,30,35,110,45,45,150,70,60),Ramp(45,35,25,80,60,40,115,90,55,155,125,80,195,165,115),Ramp(40,35,30,70,60,50,100,85,70,135,115,95,170,150,125),Ramp(80,30,30,140,45,40,190,60,50,225,110,90,245,190,170),
   Ramp(10,50,70,20,85,110,40,125,150,90,170,190,170,215,230),Ramp(70,55,20,120,95,30,170,140,40,215,185,80,245,225,150)};
  var sky=skies[k];var skin=skins[k];var robe=cloth[k];uint gold=Rgb(230,190,90),dark=Rgb(20,16,22);
  for(int y=0;y<H;y++)for(int x=0;x<W;x++)cv.Set(x,y,RampPick(sky,.95-y/(double)H*.7+(VNoise(x/16.0,y/12.0,700+k)-.5)*.2));
  // Avian wings behind the shoulders.
  if(k==1){Ellipse(cv,35,150,30,48,(dx,dy)=>Lit(skin,dx,dy));Ellipse(cv,115,150,30,48,(dx,dy)=>Lit(skin,dx,dy));}
  // Shoulders and robe.
  Ellipse(cv,75,200,64,62,(dx,dy)=>Lit(robe,dx,dy));Ellipse(cv,75,148,14,14,(dx,dy)=>Lit(skin,dx,dy));
  // Head (the dwarf wider, the machine square, the myconid a pale stalk).
  double hx=75,hy=k==7?110:98,rx=k==3?30:k==5?30:26,ry=k==3?28:32;
  if(k==2){RectFill(cv,49,66,101,128,Lit(skin,-.3,-.2));RectFill(cv,49,66,101,70,skin[4]);for(int y=70;y<128;y+=12){cv.Set(52,y,dark);cv.Set(98,y,dark);}}
  else Ellipse(cv,hx,hy,rx,ry,(dx,dy)=>Lit(skin,dx,dy));
  switch(k){
   case 0:Poly(cv,new[]{P(50,95),P(28,70),P(52,88)},(X,Y)=>skin[3]);Poly(cv,new[]{P(100,95),P(122,70),P(98,88)},(X,Y)=>skin[3]);for(int x=50;x<=100;x++){cv.Set(x,78,Rgb(220,225,235));cv.Set(x,79,Rgb(180,185,200));}
     for(int y=70;y<150;y++)for(int x=44;x<=50;x++)cv.Set(x,y,Rgb(235,230,190));for(int y=70;y<150;y++)for(int x=100;x<=106;x++)cv.Set(x,y,Rgb(235,230,190));break;
   case 1:Poly(cv,new[]{P(68,100),P(75,128),P(82,100)},(X,Y)=>Rgb(225,175,40));for(int i=0;i<5;i++)Poly(cv,new[]{P(60+i*8,70),P(64+i*8,40-i%2*8),P(68+i*8,70)},(X,Y)=>RampPick(robe,.6+i*.05));break;
   case 2:Ellipse(cv,75,92,9,9,(dx,dy)=>dx*dx+dy*dy<.3?Rgb(255,250,200):Rgb(255,120,40));RectFill(cv,60,112,90,116,dark);break;
   case 3:Ellipse(cv,75,128,32,40,(dx,dy)=>RampPick(Ramp(120,50,20,170,80,30,210,110,40,235,150,70,250,195,120),.7-.2*dx+(Math.Abs(dx*10)%2<1?.1:0)));Ellipse(cv,75,74,31,16,(dx,dy)=>Lit(Ramp(60,62,66,100,104,110,145,150,156,190,195,200,230,232,236),dx,dy));
     Poly(cv,new[]{P(44,74),P(26,50),P(50,66)},(X,Y)=>Rgb(235,225,200));Poly(cv,new[]{P(106,74),P(124,50),P(100,66)},(X,Y)=>Rgb(235,225,200));break;
   case 4:Poly(cv,new[]{P(54,72),P(30,30),P(44,40),P(62,68)},(X,Y)=>Rgb(40,30,30));Poly(cv,new[]{P(96,72),P(120,30),P(106,40),P(88,68)},(X,Y)=>Rgb(40,30,30));break;
   case 5:Poly(cv,new[]{P(62,112),P(60,96),P(66,110)},(X,Y)=>Rgb(240,235,210));Poly(cv,new[]{P(88,112),P(90,96),P(84,110)},(X,Y)=>Rgb(240,235,210));for(int y=56;y<72;y++)for(int x=70;x<=80;x++)cv.Set(x,y,dark);break;
   case 6:Ellipse(cv,52,66,13,15,(dx,dy)=>Rgb(230,150,160));Ellipse(cv,98,66,13,15,(dx,dy)=>Rgb(230,150,160));Ellipse(cv,75,116,12,14,(dx,dy)=>Lit(skin,dx,dy));cv.Set(75,128,Rgb(30,20,20));cv.Set(76,128,Rgb(30,20,20));break;
   case 7:Ellipse(cv,75,72,52,30,(dx,dy)=>dy>.55?Rgb(240,225,205):Lit(Ramp(110,20,20,170,35,30,210,55,45,235,95,80,250,160,150),dx,dy));foreach(var s in new[]{P(52,62),P(75,52),P(98,64),P(64,80),P(88,80)})Ellipse(cv,s[0],s[1],6,4,(dx,dy)=>Rgb(250,245,235));break;
   case 8:for(int i=0;i<5;i++)Poly(cv,new[]{P(56+i*10,70),P(61+i*10,46+i%2*8),P(66+i*10,70)},(X,Y)=>Rgb(240,120,130));Poly(cv,new[]{P(50,92),P(30,80),P(48,104)},(X,Y)=>RampPick(skin,.5));Poly(cv,new[]{P(100,92),P(120,80),P(102,104)},(X,Y)=>RampPick(skin,.5));break;
   case 9:Ellipse(cv,75,116,16,12,(dx,dy)=>Lit(skin,dx,dy));for(int i=0;i<6;i++)Poly(cv,new[]{P(48+i*10,72),P(53+i*10,42+i%2*10),P(58+i*10,72)},(X,Y)=>i%2==0?gold:Rgb(40,140,120));break;
  }
  // Eyes: glowing for demon and ratkin, plain for the rest; the machine has its one lens.
  if(k!=2){uint eye=k==4?Rgb(255,220,60):k==6?Rgb(230,40,40):k==7?Rgb(30,25,25):Rgb(30,30,40);int ey=k==7?108:94;for(int d=-1;d<=0;d++){cv.Set(65,ey+d,eye);cv.Set(66,ey+d,eye);cv.Set(84,ey+d,eye);cv.Set(85,ey+d,eye);}}
  // A gold clasp, then a gold frame.
  Ellipse(cv,75,170,6,6,(dx,dy)=>gold);
  for(int x=0;x<W;x++){cv.Set(x,0,gold);cv.Set(x,H-1,gold);}for(int y=0;y<H;y++){cv.Set(0,y,gold);cv.Set(W-1,y,gold);}
  return cv;
 }
}
}
