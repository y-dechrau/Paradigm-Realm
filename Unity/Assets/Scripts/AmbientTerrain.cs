using System;
using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm {
// Every kind of land and water moves (owner request, 25 Nov 2026: "as you have done with the road tiles, and grassland tiles all other
// tiles must have some form of natural dynamic movement"). The same rules as the rest of the living countryside (AmbientLife.cs): worked
// out from the place and the clock while the tile is drawn, nothing kept for a tile, only tiles in sight, nothing in the rules reads it,
// the one wind for everything the wind moves, and detail by zoom (64 and closer; at 48 only smoke).
//   Grassland, hills, savanna, steppe, tundra   grass bends as the gust passes; butterflies, a circling hawk, a tumbling bush
//   Forest, jungle, arctic forest, mangrove     the trees lean; leaves fall, birds cross, mist rises, snow slips from the branches
//   Desert, wasteland                           sand and dust run before the wind; now and then a dust devil
//   Ice sheet, glacier, tundra                  snow drifts over the ice and the ice glints
//   Mountains                                   snow blows from the peak and a cloud passes below it
//   Volcano                                     smoke, embers and a breathing glow
//   Ocean, lake, reef, river                    crests, ripples, cat's-paws, fish, glints going down the stream
//   Bog, rocky coast, villages, other fields    bubbles and mist, spray, the camp's fire, the gust over every crop
// The marks are drawn in code (AmbientMark); the trees are the map's own pictures, and leaves and smoke the SpriteCook art of the valley.
public partial class RealmGame {
 readonly Dictionary<string,Texture2D> lifeArt=new Dictionary<string,Texture2D>();
 // Small white shapes, tinted when drawn.
 Texture2D AmbientMark(string kind){
  Texture2D t;if(lifeArt.TryGetValue(kind,out t)&&t!=null)return t;int w=32,h=32;Func<double,double,double> ink=null;
  if(kind=="wisp"){w=64;h=20;ink=(x,y)=>{double u=x/w,v=(y-h*.5-(u-.5)*7-Math.Sin(u*6)*1.2)/(4.6*Math.Sin(Math.PI*Math.Pow(u,.7))+.01);return Math.Abs(v)>1?0:(1-v*v*v*v)*Math.Min(1,1.5*Math.Sin(Math.PI*u));};}
  else if(kind=="grains"){w=32;h=14;var pt=new[]{3.0,9,9,6,14,9.5,20,4.5,26,8};ink=(x,y)=>{double m=0;for(int k=0;k<5;k++){double dx=x-pt[k*2],dy=y-pt[k*2+1];m=Math.Max(m,1-(dx*dx+dy*dy)/1.9);}return m;};}
  else if(kind=="ring"){w=48;h=30;ink=(x,y)=>{double dx=(x-w/2.0)/(w/2.0-2),dy=(y-h/2.0)/(h/2.0-2),d=Math.Sqrt(dx*dx+dy*dy);return 1.25-Math.Abs(d-.88)/.11;};}
  else if(kind=="arc"){w=40;h=16;ink=(x,y)=>{double u=(x-w/2.0)/(w/2.0-1);if(Math.Abs(u)>=1)return 0;double mid=h-4-(1-u*u)*(h-7),th=3.1*(1-u*u)+.3;return 1.4-1.4*Math.Abs(y-mid)/th;};}
  else if(kind=="star"){w=h=24;ink=(x,y)=>{double dx=Math.Abs(x-12)/11,dy=Math.Abs(y-12)/11;return 1-(Math.Sqrt(dx)+Math.Sqrt(dy))*1.0;};}
  else if(kind=="dot"){w=h=16;ink=(x,y)=>{double dx=(x-8)/7,dy=(y-8)/7;return 1.2-(dx*dx+dy*dy)*1.2;};}
  else if(kind=="mist"){w=64;h=28;ink=(x,y)=>{double m=0;for(int k=0;k<4;k++){double cx=14+k*12,cy=14+((k&1)==0?2:-2),rx=15-Math.Abs(k-1.5)*2,ry=9;double dx=(x-cx)/rx,dy=(y-cy)/ry;m+=Math.Max(0,1-dx*dx-dy*dy)*.6;}return Math.Min(1,m);};}
  else if(kind=="fish"){w=20;h=10;ink=(x,y)=>{double dx=(x-8)/7,dy=(y-5)/2.6;double body=1-(dx*dx+dy*dy);double tail=x>13&&x<19?1-Math.Abs(y-5)/((x-13)*.6+.2):0;return Math.Max(body,tail);};}
  else if(kind.StartsWith("bird")){w=32;h=20;int f=kind[4]-'0';double lift=f==0?-7:f==2?5:-1;ink=(x,y)=>{double u=(x-16)/14,a=Math.Abs(u);if(a>1)return 0;double mid=11+lift*Math.Pow(a,.75)*(1-.25*a)+(a>.72?(a-.72)*5:0),th=2.1*(1-a)+.75;double body=1-((x-16)*(x-16)/9+(y-11)*(y-11)/5);return Math.Max(1-Math.Abs(y-mid)/th,body);};}
  else if(kind.StartsWith("tuft")){w=28;h=28;double lean=(kind[4]-'0')*2.6;ink=(x,y)=>{double m=0;for(int k=0;k<6;k++){double bx=8+k*2.4,tall=13+((k*7)%5)*2.3,v=(h-1-y)/tall;if(v<0||v>1)continue;double mid=bx+(k-2.5)*1.5*v+lean*v*v;m=Math.Max(m,1-Math.Abs(x-mid)/(1.25*(1-v)+.45));}return m;};}
  else if(kind.StartsWith("weed")){w=h=28;int f=kind[4]-'0';ink=(x,y)=>{double dx=x-14,dy=y-14,d=Math.Sqrt(dx*dx+dy*dy);if(d>12.5)return 0;double a=Math.Atan2(dy,dx)+f*Math.PI/8,m=0;m=Math.Max(m,1-Math.Abs(d-10.5-1.2*Math.Sin(a*5))/1.1);m=Math.Max(m,1-Math.Abs(d-6-2*Math.Sin(a*3+1))/1.0);m=Math.Max(m,.9-Math.Abs(Math.Sin(a*4+d*.25))*d/3);return m;};}
  if(ink==null)return null;var cv=new PixelCanvas(w,h);for(int y=0;y<h;y++)for(int x=0;x<w;x++){double sum=0;for(int s=0;s<4;s++)sum+=Math.Max(0,Math.Min(1,ink(x+.25+.5*(s&1),y+.25+.5*(s>>1))));int a=(int)(sum/4*255);if(a>3)cv.Set(x,y,(uint)a<<24|0xFFFFFF);}
  t=SoftTexture(cv);themeTextures.Add(t);lifeArt[kind]=t;return t;}
 static readonly string[] Tufts={"tuft0","tuft1","tuft2","tuft3","tuft4"},Birds={"bird0","bird1","bird2"},Weeds={"weed0","weed1","weed2","weed3"};
 void CountLife(int terrain){if(!ambientTally)ambientDraws++;else Count("life"+terrain);}
 // ---------- drawn at once, with the row, or above everything ----------
 struct AirMark{public Texture2D tex;public Rect r;public Color ink;}
 readonly List<AirMark> rowAir=new List<AirMark>(),highAir=new List<AirMark>();
 // How large and how solid the marks are, all together: seen at ordinary zoom, quiet beside units and towns.
 const float LifeSize=1.5f,LifeInk=1.3f;
 void Put(string mark,float x,float y,float w,float h,Color c,float alpha){w*=LifeSize;h*=LifeSize;alpha=Mathf.Min(1,alpha*LifeInk);if(alpha<=.01f)return;var old=GUI.color;c.a=alpha;GUI.color=c;GUI.DrawTexture(new Rect(x-w/2,y-h/2,w,h),AmbientMark(mark),ScaleMode.StretchToFill,true);GUI.color=old;}
 // With the row: no neighbour's ground cuts it, and the trees of the next row still cover it.
 void PutRow(string mark,float x,float y,float w,float h,Color c,float alpha){w*=LifeSize;h*=LifeSize;alpha=Mathf.Min(1,alpha*LifeInk);if(alpha<=.01f)return;c.a=alpha;rowAir.Add(new AirMark{tex=AmbientMark(mark),r=new Rect(x-w/2,y-h/2,w,h),ink=c});}
 void PutHigh(Texture2D tex,float x,float y,float w,float h,Color c,float alpha){w*=LifeSize;h*=LifeSize;alpha=Mathf.Min(1,alpha*LifeInk);if(alpha<=.01f||tex==null)return;c.a=alpha;highAir.Add(new AirMark{tex=tex,r=new Rect(x-w/2,y-h/2,w,h),ink=c});}
 void FlushRowAir(){if(rowAir.Count==0)return;var old=GUI.color;foreach(var m in rowAir){GUI.color=m.ink;GUI.DrawTexture(m.r,m.tex,ScaleMode.StretchToFill,true);}GUI.color=old;rowAir.Clear();}
 // Birds and smoke, after every tile; a bird shows only over land you can see.
 void FlushHighAir(){if(highAir.Count==0)return;var old=GUI.color;foreach(var m in highAir){GUI.color=m.ink;GUI.DrawTexture(m.r,m.tex,ScaleMode.StretchToFill,true);}GUI.color=old;highAir.Clear();}
 // ---------- timing and places ----------
 // One thing a tile does every so often: how far through it is (0 to 1), and which time round. False between times.
 bool Cycle(Tile t,int what,float period,float duty,out float life,out int turn){float clock=ambientNow/(period*(.85f+.3f*H01(t.y,what,t.x)))+H01(t.x,t.y,what*7+1);turn=Mathf.FloorToInt(clock);float f=clock-turn;life=f/duty;return f<duty;}
 static float Swell(float life){return Mathf.Sin(Mathf.PI*Mathf.Clamp01(life));}
 Vector2 Spot(Tile t,int turn,int what,Rect g,float reach=1){float a=H01(t.x+turn*31,t.y,what)*Mathf.PI*2,r=Mathf.Sqrt(H01(t.y+turn*17,t.x,what+3))*reach;return new Vector2(g.center.x+Mathf.Cos(a)*r*g.width*.36f,g.center.y+Mathf.Sin(a)*r*g.height*.3f);}
 static readonly Color LifeWhite=new Color(.97f,.99f,1f),LifeSand=new Color(1f,.97f,.84f),LifeDust=new Color(.82f,.76f,.68f),LifeFoam=new Color(.9f,.97f,1f);
 // Blown sand, dust or snow: low streaks that run before the wind, faster in a gust.
 // The streak lies over its own shadow, so pale sand shows on sand and snow on snow.
 void Drifts(Tile t,int i,int j,Rect g,float s,Color ink,float strength,int n){
  var shade=t.terrain==3||t.terrain==20?new Color(.5f,.34f,.14f):t.terrain==10?new Color(.2f,.16f,.13f):new Color(.36f,.52f,.72f);var place=AmbientPlace(i,j);float gust=GustLean(place.x,place.y,0);
  for(int m=0;m<n;m++){float life;int turn;if(!Cycle(t,11+m,4.6f,.62f,out life,out turn))continue;var p=Spot(t,turn,m,g,.75f);float run=(life-.5f)*(20+8*gust)*s,x=p.x+run,y=p.y+run*.18f;float wide=(13+7*H01(turn,m,t.x))*s*(.8f+.4f*Swell(life));
   PutRow("wisp",x+1.2f*s,y+1.7f*s,wide,wide*.28f,shade,Swell(life)*(strength+.12f*gust)*.75f);PutRow("wisp",x,y,wide,wide*.28f,ink,Swell(life)*(strength+.28f+.2f*gust));if(ambientLevel==2)PutRow("grains",x+4*s,y+1.5f*s,9*s,4*s,ink,Swell(life)*(strength+.3f*gust));CountLife(t.terrain);}}
 // Grass that bends as the gust passes and settles again: tufts rise out of the painted ground, so still grass is the picture alone.
 void Grass(Tile t,int i,int j,Rect g,float s,Color ink,int n,float lift=0){
  var place=AmbientPlace(i,j);float near;if(GustPhase(place.x,place.y,-.8f,out near,3.4f)<0)return;for(int m=0;m<n;m++){float fx=(H01(t.x,t.y,m+40)-.5f)*.74f,fy=(H01(t.y,t.x,m+60)-.5f)*.6f;float strength,u=GustPhase(place.x+fx,place.y+fy*.5f,.1f+H01(t.x+m,t.y,5)*.25f,out strength,1.5f);if(u<0)continue;
   float fade=Mathf.Clamp01(Mathf.Min(u,1-u)*4.5f);int frame=Mathf.Clamp((int)(Swell(u)*strength*5),0,4);float size=(5.2f+2.2f*H01(t.x,m,t.y))*s;float shade=.82f+.36f*H01(m,t.x,t.y);var c=new Color(Mathf.Min(1,ink.r*shade),Mathf.Min(1,ink.g*shade),Mathf.Min(1,ink.b*shade));
   float hump=lift*(1-Mathf.Abs(fx)*2.1f);Put(Tufts[frame],g.center.x+fx*g.width,g.center.y+fy*g.height-hump*s-size*.6f,size,size,c,fade*.85f);CountLife(t.terrain);}}
 void Glints(Tile t,Rect g,float s,Color ink,int n,float alpha){for(int m=0;m<n;m++){float life;int turn;if(!Cycle(t,23+m,3.4f,.24f,out life,out turn))continue;var p=Spot(t,turn,m+20,g);float size=(3.2f+2.6f*Swell(life))*s;Put("star",p.x,p.y,size,size,ink,Swell(life)*alpha);CountLife(t.terrain);}}
 void Ripples(Tile t,Rect g,float s,Color ink,float period,float alpha){float life;int turn;if(!Cycle(t,31,period,.5f,out life,out turn))return;var p=Spot(t,turn,33,g,.8f);for(int k=0;k<2;k++){float l=life-k*.22f;if(l<=0)continue;float w=(2.5f+13*l)*s;Put("ring",p.x,p.y,w,w*.58f,ink,(1-l)*alpha*Mathf.Clamp01(l*8));}CountLife(t.terrain);}
 // A whirl of dust that wanders a little way and falls apart.
 void Devil(Tile t,float s,Rect g,Color ink){float life;int turn;if(H01(t.x,t.y,71)>.2f||!Cycle(t,73,15,.36f,out life,out turn))return;float k=Swell(life),x=g.center.x+(life-.5f)*26*s+Mathf.Sin(life*9)*3*s,y=g.center.y+(life-.5f)*5*s;
  for(int n=0;n<6;n++){float up=n/5f,w=(2.6f+6.5f*up*up)*s*(.6f+.4f*k),spin=Mathf.Sin(ambientNow*9+n*1.3f)*(.6f+1.4f*up)*s;PutRow("ring",x+spin,y-up*15*s*k,w,w*.5f,ink,k*(.62f-.3f*up));}CountLife(t.terrain);}
 // ---------- birds ----------
 void Flock(Tile t,float x,float y,float s,Color ink,float share,float size=5.2f){
  float life;int turn;if(H01(t.x,t.y,81)>share||!Cycle(t,83,24,.42f,out life,out turn))return;float a=H01(turn,t.x,t.y)*Mathf.PI*2;float dx=Mathf.Cos(a),dy=Mathf.Sin(a)*.6f;int n=3+(int)(H01(t.y,turn,9)*3);float far=(life-.12f)*300*s,high=(14+10*Mathf.Clamp01(life*5))*s;
  for(int k=0;k<n;k++){int rank=(k+1)/2,side=(k&1)==0?1:-1;float bx=x+dx*(far-rank*9*s)-dy*rank*7*s*side+Mathf.Sin(ambientNow*1.3f+k)*1.5f*s,by=y+dy*(far-rank*9*s)+dx*rank*4.5f*s*side-high+Mathf.Sin(ambientNow*1.7f+k*2)*1.2f*s;
   int mx,my;if(!MapTileAt(new Vector2(bx,by+high),out mx,out my)||!world.Visible(mx,my))continue;bool glide=Mathf.Repeat(ambientNow*.4f+k*.21f,1)<.35f;int f=glide?1:(int)(ambientNow*8+k*1.7f)%4;if(f==3)f=1;float w=size*s;float fade=Mathf.Clamp01(Mathf.Min(life*9,(1-life)*6));
   PutHigh(AmbientMark(Birds[f]),bx+3*s,by+high,w*.8f,w*.3f,Color.black,fade*.14f);PutHigh(AmbientMark(Birds[f]),bx,by,w,w*.62f,ink,fade*.92f);}CountLife(t.terrain);Count("bird");}
 void Hawk(Tile t,float x,float y,float s,float share){
  float life;int turn;if(H01(t.x,t.y,87)>share||!Cycle(t,89,22,.6f,out life,out turn))return;float a=ambientNow*.9f*(H01(t.x,t.y,5)>.5f?1:-1),fade=Mathf.Clamp01(Mathf.Min(life,1-life)*9);float bx=x+Mathf.Cos(a)*15*s,by=y-26*s+Mathf.Sin(a)*6*s,w=7.5f*s;PutHigh(AmbientMark("bird1"),bx+2*s,by+24*s,w*.8f,w*.28f,Color.black,fade*.13f);PutHigh(AmbientMark(Mathf.Repeat(ambientNow*.17f+t.x,1)<.12f?"bird"+((int)(ambientNow*8)%2*2):"bird1"),bx,by,w,w*.5f*(.8f+.2f*Mathf.Sin(a)),new Color(.2f,.14f,.09f),fade*.95f);CountLife(t.terrain);Count("bird");}
 // ---------- each kind of tile: what lies on the ground ----------
 void DrawGroundLife(Tile t,int i,int j,Rect g,float sx,float sy,float s,bool built){
  if(!ambientPass||ambientLevel<1)return;bool close=ambientLevel==2;var place=AmbientPlace(i,j);
  switch(t.terrain){
   case 0:if(!t.farm){Grass(t,i,j,g,s,new Color(.6f,.8f,.32f),close?9:5);if(close&&!built)Butterfly(t,g,s);}break;
   case 3:if(!t.farm){Drifts(t,i,j,g,s,LifeSand,.42f,close?3:2);Devil(t,s,g,LifeSand);}break;
   case 10:if(!t.farm){Drifts(t,i,j,g,s,LifeDust,.36f,close?3:2);Devil(t,s,g,LifeDust);}break;
   case 8:Grass(t,i,j,g,s,new Color(.74f,.76f,.58f),close?5:3);Drifts(t,i,j,g,s,LifeWhite,.36f,2);break;
   case 14:Drifts(t,i,j,g,s,LifeWhite,.5f,close?3:2);Glints(t,g,s,LifeWhite,2,.9f);break;
   case 15:Drifts(t,i,j,g,s,LifeWhite,.46f,2);break;
   case 16:if(!t.farm)Grass(t,i,j,g,s,new Color(.88f,.8f,.4f),close?9:5);break;
   case 20:if(!t.farm){Grass(t,i,j,g,s,new Color(.86f,.76f,.46f),close?7:4);Drifts(t,i,j,g,s,LifeSand,.2f,1);Tumble(t,g,s);}break;
   case 4:{// Crests rise, run a little before the wind and fall back; gulls near land.
    int n=close?3:2;for(int m=0;m<n;m++){float life;int turn;if(!Cycle(t,41+m,5.4f,.5f,out life,out turn))continue;var p=Spot(t,turn,m,g,.85f);float k=Swell(life),w=(4.5f+5.5f*k)*s,run=(life-.5f)*7*s;Put("arc",p.x+run,p.y+run*.3f-k*s,w,w*.36f,LifeFoam,k*.6f);if(close&&life>.45f)Put("wisp",p.x+run-2*s,p.y+run*.3f+1.4f*s,w*1.1f,w*.22f,LifeFoam,Swell((life-.45f)/.55f)*.3f);CountLife(4);}
    Glints(t,g,s,LifeWhite,1,.55f);Flock(t,sx,sy,s,LifeWhite,.035f,6f);break;}
   case 6:Ripples(t,g,s,LifeFoam,6.5f,.5f);Glints(t,g,s,LifeWhite,close?2:1,.6f);CatsPaw(t,place,g,s);break;
   case 12:Glints(t,g,s,new Color(.75f,1f,.93f),close?3:2,.75f);Ripples(t,g,s,new Color(.8f,1f,.95f),8f,.34f);Fish(t,g,s);break;
   case 7:StreamMarks(t,g,s);if(!t.farm)Grass(t,i,j,g,s,new Color(.6f,.8f,.32f),close?4:2);break;
   case 9:Bubbles(t,g,s);break;
   case 19:Ripples(t,g,s,LifeFoam,7f,.4f);break;
   case 18:Spray(t,g,s);break;}}
 // Small runs of light wind over still water.
 void CatsPaw(Tile t,Vector2 place,Rect g,float s){for(int m=0;m<4;m++){float fx=(H01(t.x,m,t.y+7)-.5f)*.6f,fy=(H01(t.y,m,t.x+9)-.5f)*.5f;float strength,u=GustPhase(place.x+fx,place.y+fy*.5f,0,out strength,1.3f);if(u<0)continue;float w=(5+3*H01(m,t.x,1))*s;Put("arc",g.center.x+fx*g.width+u*3*s,g.center.y+fy*g.height,w,w*.22f,LifeFoam,Swell(u)*.42f*strength);CountLife(t.terrain);}}
 // Light going down the stream, on the river's own line.
 void StreamMarks(Tile t,Rect g,float s){
  int mask=RiverMaskOf(t);var line=PixelArt.HexRiverLine(mask,(t.x*7+t.y*3)%3);if(line==null){Glints(t,g,s,LifeWhite,1,.35f);return;}float length=(float)PixelArt.LineLength(line);if(length<6)return;
  for(int m=0;m<(ambientLevel==2?3:2);m++){float clock=ambientNow/(length/7f)+m*.37f+H01(t.x,t.y,m+50);int turn=Mathf.FloorToInt(clock);float p=clock-turn;if(H01(turn,t.x,t.y+m)<.35f)continue;double dx,dy;var at=PixelArt.AlongLine(line,p*length,out dx,out dy);float radius=(float)PixelArt.HexRiverRadius(mask),lane=(H01(turn,m,t.y)-.5f)*.8f;
   float px=g.x+(float)((at[0]-dy*lane*radius)/PixelArt.HW)*g.width,py=g.y+(float)((at[1]+dx*lane*radius*.8)/PixelArt.HH)*g.height;int way=((int)Math.Round(Math.Atan2(dy,dx)/(Math.PI*2)*16)%16+16)%16;float size=10f*s;var old=GUI.color;GUI.color=new Color(.92f,.98f,1f,Swell(p)*.85f);GUI.DrawTexture(new Rect(px-size/2,py-size/2,size,size),Stroke(way,true),ScaleMode.StretchToFill,true);GUI.color=old;CountLife(7);}}
 void Fish(Tile t,Rect g,float s){float life;int turn;if(!Cycle(t,45,7.5f,.4f,out life,out turn))return;var p=Spot(t,turn,46,g,.45f);float way=H01(turn,t.x,t.y)>.5f?1:-1;for(int k=0;k<3;k++){float l=life-k*.06f;float x=p.x+way*((l-.5f)*22+Mathf.Sin(k*2.1f)*2)*s,y=p.y+(Mathf.Sin(l*7+k)*2.2f+k*2.2f)*s,w=7f*s;var old=GUI.color;GUI.color=new Color(k==1?1f:.98f,k==1?.72f:.55f,.3f,Swell(life)*.75f);GUI.DrawTextureWithTexCoords(new Rect(x-w/2,y-w*.25f,w,w*.5f),AmbientMark("fish"),way<0?new Rect(0,0,1,1):new Rect(1,0,-1,1));GUI.color=old;}CountLife(t.terrain);}
 void Butterfly(Tile t,Rect g,float s){float life;int turn;if(H01(t.x,t.y,61)>.3f||!Cycle(t,63,9,.6f,out life,out turn))return;var p=Spot(t,turn,64,g,.5f);float x=p.x+Mathf.Sin(life*9+t.x)*9*s+life*8*s,y=p.y+Mathf.Cos(life*13+t.y)*4*s-4*s,w=Mathf.Abs(Mathf.Sin(ambientNow*11+t.x))*1.6f*s+.35f*s;var ink=H01(t.y,turn)>.5f?new Color(1f,.95f,.65f):new Color(.85f,.9f,1f);PutRow("dot",x-w*.5f,y,w,2.1f*s,ink,Swell(life)*.95f);PutRow("dot",x+w*.5f,y,w,2.1f*s,ink,Swell(life)*.95f);CountLife(0);}
 void Tumble(Tile t,Rect g,float s){float life;int turn;if(H01(t.x,t.y,65)>.3f||!Cycle(t,67,13,.34f,out life,out turn))return;float x=g.center.x+(life-.5f)*46*s,hop=Mathf.Abs(Mathf.Sin(life*12))*4*s,y=g.center.y+(H01(turn,t.y,t.x)-.5f)*10*s+(life-.5f)*8*s,w=6.2f*s;PutRow("dot",x,y+2.6f*s,w*.9f,w*.3f,Color.black,Swell(life)*.22f);PutRow(Weeds[(int)(life*40)%4],x,y-hop,w,w,new Color(.5f,.37f,.18f),Mathf.Clamp01(Swell(life)*3));CountLife(20);}
 void Bubbles(Tile t,Rect g,float s){for(int m=0;m<2;m++){float life;int turn;if(!Cycle(t,47+m,5.2f,.5f,out life,out turn))continue;var p=Spot(t,turn,m+48,g,.5f);if(life<.45f){float w=(1+3.2f*life/.45f)*s;Put("dot",p.x,p.y,w,w*.8f,new Color(.72f,.84f,.66f),.75f);Put("dot",p.x-w*.15f,p.y-w*.15f,w*.35f,w*.3f,LifeWhite,.8f);}else{float l=(life-.45f)/.55f,w=(3+8*l)*s;Put("ring",p.x,p.y,w,w*.58f,new Color(.82f,.92f,.76f),(1-l)*.55f);}CountLife(9);}}
 // Spray where the rocks meet the sea.
 void Spray(Tile t,Rect g,float s){bool any=false;for(int k=0;k<6;k++){var n=world.Step(t.x,t.y,World.HexDi[k],World.HexDj[k]);if(n==null||!n.seen||!PixelArt.IsWater(n.terrain)||n.terrain==7)continue;any=true;float life;int turn;if(!Cycle(t,51+k,4.4f,.36f,out life,out turn))continue;float ang=k*-Mathf.PI/3f,ex=g.center.x+Mathf.Cos(ang)*g.width*.4f,ey=g.center.y+Mathf.Sin(ang)*g.height*.4f;
   PutRow("arc",ex,ey-Swell(life)*2*s,(6+5*life)*s,(2.6f+1.5f*life)*s,LifeFoam,(1-life)*.75f);for(int d=0;d<5;d++){float vx=(H01(turn,d,k)-.5f)*13*s,vy=(7+7*H01(d,turn,k))*s;float l=life*1.15f;PutRow("dot",ex+vx*l,ey-vy*l*(1-l)*2.4f,1.7f*s,1.7f*s,LifeWhite,Mathf.Clamp01((1-life)*1.6f)*.85f);}CountLife(18);}
  if(!any)Grass(t,0,t.y,g,s,new Color(.62f,.7f,.42f),3);}
 // ---------- the trees and peaks themselves ----------
 // A stand of trees leaning row by row: the farther rows more, the foot of the picture not at all.
 void DrawStand(Rect r,Texture2D tex,float lean,float amount){
  if(Mathf.Abs(lean)<=.03f){GUI.DrawTexture(r,tex);return;}const int n=8;for(int k=0;k<n;k++){float v0=k/(float)n,v1=(k+1)/(float)n,up=1-(k+.5f)/n;float share=up<.14f?0:Mathf.Pow((up-.14f)/.86f,.8f);GUI.DrawTextureWithTexCoords(new Rect(r.x+lean*amount*share,r.y+r.height*v0,r.width,r.height/n+.5f),tex,new Rect(0,1-v1,1,v1-v0));}Count("canopy");}
 static float SwayOf(int terrain){return terrain==1?2.6f:terrain==11?2.8f:terrain==13?1.8f:terrain==16?3.2f:terrain==19?2.4f:terrain==9?1.6f:0;}
 // wild: the tile's own scenery, not a farmstead or a port.
 void DrawScenery(Rect r,Texture2D tex,Tile t,int i,int j,float s,bool visible,bool wild){
  float sway=wild&&visible&&ambientPass&&ambientLevel>=1?SwayOf(t.terrain):0;
  if(sway>0){var place=AmbientPlace(i,j);float lean=GustLean(place.x,place.y,.5f);DrawStand(r,tex,lean,sway*s);if(lean>.03f)CountLife(t.terrain);}else GUI.DrawTexture(r,tex);
  if(wild&&visible&&ambientPass)DrawAirLife(r,t,i,j,s);}
 void DrawAirLife(Rect r,Tile t,int i,int j,float s){
  float cx=r.center.x,foot=r.y+r.height*.78f;var place=AmbientPlace(i,j);
  if(t.terrain==17){Volcano(r,t,s);return;}if(ambientLevel<1)return;var g=new Rect(cx-32*s,foot-26*s,64*s,40*s);
  switch(t.terrain){
   case 1:{float life;int turn;if(Cycle(t,91,6.5f,.55f,out life,out turn)){var art=Amb("leaf-"+(turn&1));if(art!=null){float x=r.x+r.width*(.2f+.6f*H01(turn,t.x,t.y))+Mathf.Sin(life*9)*3*s+life*6*s,y=r.y+r.height*(.25f+.3f*H01(turn,t.y,1))+life*20*s,w=2.9f*s,flip=Mathf.Abs(Mathf.Cos(life*10));PutHigh(art,x,y,Mathf.Max(1,w*flip),w,Color.white,Swell(life));CountLife(1);}}Flock(t,cx,foot,s,new Color(.14f,.12f,.12f),.1f);break;}
   case 11:{float life;int turn;if(Cycle(t,93,10,.6f,out life,out turn)){float w=(16+12*life)*s;PutRow("mist",cx+(H01(turn,t.x,t.y)-.5f)*18*s+life*6*s,r.y+r.height*(.52f-.22f*life),w,w*.4f,LifeWhite,Swell(life)*.5f);CountLife(11);}Flock(t,cx,foot,s,H01(t.x,t.y,2)>.5f?new Color(.9f,.24f,.16f):new Color(.2f,.62f,.9f),.12f);break;}
   case 13:{float life;int turn;if(Cycle(t,95,6.2f,.34f,out life,out turn)){float x=r.x+r.width*(.2f+.6f*H01(turn,t.x,t.y)),y=r.y+r.height*(.3f+.25f*H01(turn,t.y,1));for(int d=0;d<6;d++)PutRow("dot",x+(H01(turn,d,1)-.5f)*7*s+life*3*s,y+(life*13+H01(d,turn,2)*4)*s,1.6f*s,1.6f*s,LifeWhite,(1-life)*.95f);PutRow("mist",x,y+life*8*s,9*s,4*s,LifeWhite,(1-life)*.4f);CountLife(13);}Glints(t,g,s,LifeWhite,1,.7f);break;}
   case 2:Grass(t,i,j,new Rect(g.x+6*s,g.y+2*s,g.width-12*s,g.height-8*s),s,new Color(.62f,.8f,.32f),ambientLevel==2?8:5,9);Hawk(t,cx,foot,s,.16f);break;
   case 5:{// Snow streams off the peak; a cloud goes by beneath it.
    for(int m=0;m<3;m++){float clock=ambientNow/3.6f+m/3f+H01(t.x,t.y,m);float life=clock-Mathf.Floor(clock);float gust=.6f+.4f*GustLean(place.x,place.y,0);float w=(6+11*life)*s;PutHigh(AmbientMark("wisp"),cx+(1.5f+13*life)*s,r.y+(3.5f+2.4f*life)*s,w,w*.34f,LifeWhite,Mathf.Clamp01(life*6)*(1-life)*.85f*gust);}CountLife(5);
    float l2;int turn2;if(Cycle(t,97,15,.6f,out l2,out turn2)){float w=26*s;PutRow("mist",cx+(l2-.5f)*40*s,r.y+r.height*(.46f+.16f*H01(turn2,t.x,t.y)),w,w*.36f,LifeWhite,Swell(l2)*.42f);}Hawk(t,cx,foot-8*s,s,.08f);break;}
   case 15:Glints(t,g,s,LifeWhite,ambientLevel==2?3:2,.95f);{float life;int turn;if(Cycle(t,99,5,.5f,out life,out turn)){float w=(7+9*life)*s;PutRow("wisp",cx+(2+14*life)*s,r.y+(5+3*life)*s,w,w*.33f,LifeWhite,Swell(life)*.6f);CountLife(15);}}break;
   case 9:{float life;int turn;if(Cycle(t,101,11,.7f,out life,out turn)){float w=30*s;PutRow("mist",cx+(life-.5f)*16*s,foot-4*s,w,w*.34f,new Color(.82f,.9f,.8f),Swell(life)*.34f);CountLife(9);}break;}
   case 19:Flock(t,cx,foot,s,LifeWhite,.07f);break;
   case 16:Flock(t,cx,foot,s,new Color(.14f,.12f,.12f),.05f);break;
   case 18:Flock(t,cx,foot,s,LifeWhite,.12f,6f);break;}}
 void Volcano(Rect r,Tile t,float s){
  float x=r.x+r.width*.49f,y=r.y+r.height*.2f;
  if(ambientLevel>=1){float glow=.2f+.14f*Mathf.Sin(ambientNow*1.9f+t.x)+.06f*Mathf.Sin(ambientNow*5.3f+t.y);PutRow("dot",x,y+5*s,13*s,9*s,new Color(1f,.5f,.12f),glow);}
  for(int m=0;m<4;m++){float clock=ambientNow/5.2f+m/4f+H01(t.x,t.y,m);int turn=Mathf.FloorToInt(clock);float life=clock-turn;var art=Amb("smoke-"+Mathf.Clamp((int)(life*8),0,7));if(art==null)break;float w=(7+15*life)*s;PutHigh(AmbientMark("mist"),x+(Mathf.Sin(turn+m)*2+life*13)*s,y-(2+life*27)*s,w*1.15f,w*.8f,Color.Lerp(new Color(.3f,.27f,.27f),new Color(.78f,.76f,.75f),life),Mathf.Clamp01(life*5)*(1-life)*.95f);}
  if(ambientLevel>=1)for(int m=0;m<(ambientLevel==2?5:3);m++){float clock=ambientNow/(1.7f+.5f*H01(m,t.x))+H01(t.x,t.y,m+9);int turn=Mathf.FloorToInt(clock);float life=clock-turn;if(H01(turn,m,t.y)<.35f)continue;float vx=(H01(turn,m,1)-.5f)*18*s,h=(10+9*H01(m,turn,2))*s;PutHigh(AmbientMark("dot"),x+vx*life,y-h*life*(1.7f-life*1.25f),1.5f*s,1.5f*s,Color.Lerp(new Color(1f,.82f,.35f),new Color(1f,.3f,.08f),life),1-life);}
  CountLife(17);}
 // Development aid: "-lifeShot <folder>" lays out a patch of every kind of land and water, photographs it, saves the frames of a clip,
 // checks that each kind moves and that nothing does where it should not, and measures the cost.
 System.Collections.IEnumerator LifeShot(string folder){
  System.IO.Directory.CreateDirectory(folder);yield return Frames(5);OpenValley(false);yield return Frames(10);Action quiet=()=>{world.newContacts.Clear();world.researchPrompts.Clear();world.buildPrompts.Clear();world.buildNotices.Clear();if(world.rivalCalls!=null)world.rivalCalls.Clear();if(world.wayChoices!=null)world.wayChoices.Clear();gateScene=null;detailsScreen="";};
  world.gatesLocked=false;foreach(var gate in world.tiles)if(gate.realm==0){gate.gateLink=0;gate.village=false;}int[] kinds={0,1,2,3,4,5,6,8,9,10,11,12,13,14,15,16,17,18,19,20};var home=world.CapitalCity(0);int ox=(home.x+12)%(world.Width-24)+2,oy=Mathf.Clamp(home.y-10,3,world.MainHeight-26);
  var centres=new List<Vector2Int>();for(int n=0;n<kinds.Length;n++){int cx=ox+(n%5)*4+2,cy=oy+(n/5)*5+2;centres.Add(new Vector2Int(cx,cy));for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){var t=world.At(cx+dx,cy+dy);if(t==null)continue;t.farm=t.road=t.mine=t.fort=t.village=t.ruin=false;t.resource=0;t.owner=-1;t.gateLink=0;t.woods=0;t.cleared=false;t.terrain=kinds[n];}}
  world.units.RemoveAll(u=>{foreach(var c in centres)if(Mathf.Abs(u.x-c.x)<=3&&Mathf.Abs(u.y-c.y)<=3)return true;return false;});foreach(var c in centres)world.Add(0,3,c.x,c.y);
  foreach(var t in world.tiles)if(t.realm==0)t.seen=true;world.Reveal();yield return Frames(5);quiet();yield return Frames(5);quiet();ClearIso();selected=-1;cityId=-1;var mid=new Vector2Int(ox+9,oy+9);
  quiet();yield return Frames(3);quiet();foreach(int zoom in new[]{64,96,128}){isoWidth=zoom;FocusTile(zoom==64?mid.x:centres[6].x,zoom==64?mid.y:centres[6].y+1);yield return Frames(20);AmbientOn=false;yield return Frames(3);yield return Shot(folder,"life-"+zoom+"-without.bmp");AmbientOn=true;yield return Frames(40);yield return Shot(folder,"life-"+zoom+"-with.bmp");}
  isoWidth=64;FocusTile(mid.x,mid.y);var most=new Dictionary<string,int>();float until=Time.realtimeSinceStartup+32;while(Time.realtimeSinceStartup<until){yield return null;foreach(var kv in ambientCounts){int had;most.TryGetValue(kv.Key,out had);most[kv.Key]=Mathf.Max(had,kv.Value);}}
  foreach(int k in kinds){int had;most.TryGetValue("life"+k,out had);Probe(World.TerrainNames[k]+" moves ("+had+")",had>0);}{int b;most.TryGetValue("bird",out b);Probe("birds cross ("+b+")",b>0);}
  for(int f=0;f<160;f++){yield return Shot(folder,"clip-"+f.ToString("000")+".bmp");float next=Time.realtimeSinceStartup+.02f;while(Time.realtimeSinceStartup<next)yield return null;}
  foreach(int n in new[]{4,9,14,19}){isoWidth=128;FocusTile(centres[n].x-2,centres[n].y);yield return Frames(60);yield return Shot(folder,"life-close-"+n+".bmp");}
  isoWidth=48;FocusTile(mid.x,mid.y);yield return Frames(20);int odd=0;foreach(var kv in ambientCounts)if(kv.Key!="life17"&&kv.Key!="smoke"&&kv.Key!="camp")odd+=kv.Value;Probe("at the strategic zoom only smoke is left ("+odd+")",odd==0);isoWidth=32;yield return Frames(20);Probe("and at the widest nothing",ambientDraws==0);
  isoWidth=64;FocusTile(mid.x,mid.y);foreach(int zoom in new[]{64,128}){isoWidth=zoom;FocusTile(mid.x,mid.y);yield return Frames(30);float[] ms=new float[2];ambientTally=false;int marks=0;for(int pass=0;pass<2;pass++){AmbientOn=pass==1;yield return Frames(20);int n=0;mapClock.Reset();for(int f=0;f<300;f++){yield return null;n++;marks=Mathf.Max(marks,ambientDraws);}ms[pass]=(float)(mapClock.Elapsed.TotalMilliseconds/n);}Debug.Log("PROBEINFO map drawing at "+zoom+": "+ms[0].ToString("0.00")+" ms without, "+ms[1].ToString("0.00")+" ms with ("+marks+" marks)");}
  ambientTally=true;AmbientOn=true;world.units.Clear();world.cities.Clear();foreach(var t in world.tiles)t.owner=-1;world.Reveal();quiet();isoWidth=64;FocusTile(mid.x,mid.y);yield return Frames(30);int still=0;foreach(var kv in ambientCounts)if(kv.Key.StartsWith("life"))still+=kv.Value;Probe("nothing moves on land that is remembered but not in sight ("+still+")",still==0);
  Application.Quit();}
 // The fire of a village camp.
 void DrawCampSmoke(Tile t,Rect r,float s){if(!ambientPass)return;float clock=ambientNow/6.5f+H01(t.x,t.y,3);int turn=Mathf.FloorToInt(clock);float life=(clock-turn)/.62f;if(life>1)return;var art=Amb("smoke-"+Mathf.Clamp((int)(life*8),0,7));if(art==null)return;float w=12*s;PutHigh(art,r.center.x+life*2*s,r.y+r.height*.45f-w*.5f-life*4*s,w,w,new Color(1,.985f,.95f),Mathf.Clamp01(life*5)*(1-life)*.95f);Count("camp");}
}
}
