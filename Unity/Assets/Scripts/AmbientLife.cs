using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The living countryside (owner brief, 26 Nov 2026; first pass, judged on a river valley: run the game with -valley). Small painted
// gestures that follow from what the map already shows, drawn in the same pass as the tiles so roofs and trees cover them:
//  - The broken-silver current: tapered marks just downstream of a bridge pier (or the mill wheel), along the river's own painted line.
//  - The travelling gust: one wind, worked out from the place on the map, that leans ripe wheat, then the next canopy a little later.
//  - The miller's road: a flour cart between a worked field (or the valley's mill) and its city, along the road as it is painted.
//  - The occupied hearth: thin smoke from a settlement's roofs; a new city's first wisp rises at once.
//  - The wrong-way leaves: loose leaves drifting into a shut gateway against the wind; beside water, a gateway your people are opening
//    shows its reflection a beat before colour reaches the stone.
// It changes nothing in the game. It is drawn only on tiles in sight (nothing moves on remembered land), carts only for your own
// cities, gateways only once found. Zoom decides how much: 128 and 96 everything, 64 the readable gestures, 48 smoke alone, 32 nothing.
// Settings > Living countryside turns it off; -noAmbient too. No object is kept for a tile: everything is worked out from the place
// and the clock while the tile is drawn, except the few carts on the road. Art: Visuals/Ambient, SpriteCook (Design/SpriteCook-River-Valley-2026-11-26).
public partial class RealmGame {
 static bool? ambientPref;static readonly bool ambientBarred=Environment.GetCommandLineArgs().Contains("-noAmbient");
 internal bool AmbientOn{get{if(ambientBarred)return false;if(!ambientPref.HasValue){try{ambientPref=PlayerPrefs.GetInt("RealmAmbient",1)!=0;}catch{ambientPref=true;}}return ambientPref.Value;}set{if(ambientPref.HasValue&&ambientPref.Value==value)return;ambientPref=value;if(devRun)return;try{PlayerPrefs.SetInt("RealmAmbient",value?1:0);}catch{}}}
 // 2 close, 1 ordinary play, 0 strategic (smoke only), -1 nothing.
 int AmbientDetail(){return isoWidth>=96?2:isoWidth>=64?1:isoWidth>=48?0:-1;}
 bool ambientPass;int ambientLevel=-1;float ambientNow;internal int ambientDraws;internal readonly Dictionary<string,int> ambientCounts=new Dictionary<string,int>();
 bool ambientTally=devRun0;static readonly bool devRun0=Environment.GetCommandLineArgs().Any(a=>a.EndsWith("Shot"));
 void Count(string what){ambientDraws++;if(!ambientTally)return;int n;ambientCounts.TryGetValue(what,out n);ambientCounts[what]=n+1;}
 readonly Dictionary<string,Texture2D> ambientArt=new Dictionary<string,Texture2D>();
 Texture2D Amb(string name){Texture2D t;if(ambientArt.TryGetValue(name,out t))return t;var raw=Resources.Load<Texture2D>("Visuals/Ambient/"+name);if(raw!=null){try{var cv=new PixelCanvas(raw.width,raw.height);Array.Copy(ReadArtPixels(raw),cv.p,cv.p.Length);t=SoftTexture(cv);themeTextures.Add(t);}catch(Exception){t=raw;}}ambientArt[name]=t;return t;}
 static float H01(int a,int b,int c=0){unchecked{int h=a*374761393+b*668265263+c*982451653;h=(h^(h>>13))*1274126177;h^=h>>16;return (h&0xFFFF)/65535f;}}
 // Called before the tiles are drawn.
 void BeginAmbient(){
  ambientNow=Time.realtimeSinceStartup;ambientDraws=0;ambientCounts.Clear();ambientLevel=AmbientOn&&world.Hex&&viewRealm>=0?AmbientDetail():-1;ambientPass=ambientLevel>=0;ambientGates.Clear();rowAir.Clear();highAir.Clear();
  if(ambientWorld!=world){ambientWorld=world;carts.Clear();hearthBorn.Clear();hearthKnown.Clear();foreach(var c in world.cities)hearthKnown.Add(c.id);cartsChecked=-9;wheelTurn=0;wheelSpeed=0;}
  if(!ambientPass){if(!AmbientOn)carts.Clear();return;}
  foreach(var c in world.cities)if(hearthKnown.Add(c.id))hearthBorn[c.id]=ambientNow+.25f;
  UpdateCarts();UpdateWheel();}
 World ambientWorld;
 // Where a tile stands on the map in tile widths, whatever its row: the wind knows nothing of hexagons.
 static Vector2 AmbientPlace(int i,int j){return new Vector2(i+(j&1)*.5f,j*PixelArt.HR/(float)PixelArt.HW);}
 // ---------- the wind ----------
 const float GustSpeed=.9f,GustGap=7.6f,GustWidth=1.15f,CanopyWidth=2f;// wheat answers in a narrow band, crowns take longer to swing and settle
 static readonly Vector2 WindWay=new Vector2(.95f,.31f);
 // How far through a passing gust this place is (0 to 1), or -1 in still air. delay: how long after the front this thing answers.
 float GustPhase(float x,float y,float delay,out float strength,float width=GustWidth){
  float p=x*WindWay.x+y*WindWay.y+.42f*Mathf.Sin(y*1.9f+x*.45f)+.2f*Mathf.Sin(x*1.3f-y*2.7f);float q=(ambientNow-delay)*GustSpeed-p;int k=Mathf.FloorToInt(q/GustGap);float f=q-k*GustGap;
  strength=H01(k,17);strength=strength<.22f?0:.55f+.45f*strength;if(strength<=0||f>width)return -1;return f/width;}
 // The same gust as a lean that rises, holds and settles with a little give.
 float GustLean(float x,float y,float delay){float s,u=GustPhase(x,y,delay,out s,CanopyWidth);if(u<0)return 0;float lean=u<.32f?Mathf.SmoothStep(0,1,u/.32f):u<.55f?1:Mathf.SmoothStep(1,0,(u-.55f)/.45f)-.1f*Mathf.Sin((u-.55f)/.45f*Mathf.PI*2)*(u>.75f?1:0);return lean*s;}
 static int GustFrame(float u,int frames){if(u<0)return 0;float v=u<.32f?u/.32f*.5f:u<.55f?.5f+.07f*Mathf.Sin((u-.32f)*20):.5f+(u-.55f)/.45f*.5f;return Mathf.Clamp(Mathf.RoundToInt(v*(frames-1)),0,frames-1);}
 // Ripe wheat on a worked-looking field: at rest it is the painted field alone; as a gust passes, clumps at different depths rise out of
 // the furrows, lean, pause and settle back into them, pale-tipped, so the gust is a pale band crossing the field. Roots stay still.
 void DrawWheat(Tile t,int i,int j,Rect ground,float s){
  if(!ambientPass||ambientLevel<1)return;int crop=IsoVariant(t.x,t.y)%3;int want=ambientLevel==2?23:13;var place=AmbientPlace(i,j);float hw=.36f*ground.width,hh=hw*.5f;bool river=t.terrain==7;int mask=river?RiverMaskOf(t):0,variant=(t.x*7+t.y*3)%3;
  for(int row=0;row<5;row++)for(int n=0;n<(want+2)/5;n++){
   // Rows run with the painted furrows: places on the field's own slanted grid, nudged.
   float a=-.8f+row*.4f+(H01(t.x*31+row,t.y,n)-.5f)*.2f,b=-.8f+(n+.5f)*1.6f/((want+2)/5)+(H01(t.x,t.y*17+row,n+9)-.5f)*.34f;float fx=(a+b)*.5f*hw,fy=(b-a)*.5f*hh;if(Mathf.Abs(fx)/hw+Mathf.Abs(fy)/hh>.9f)continue;
   float px=ground.center.x+fx,py=ground.center.y+fy;
   if(river){int x2=(int)((px-ground.x)/ground.width*PixelArt.HW*2),y2=(int)((py-ground.y)/ground.height*PixelArt.HH*2);bool wet=false;for(int q=-4;q<=4&&!wet;q+=4)for(int w=-4;w<=4&&!wet;w+=4)wet=x2+q>=0&&y2+w>=0&&x2+q<PixelArt.HW*2&&y2+w<PixelArt.HH*2&&PixelArt.HexRiverWet(mask,variant,x2+q,y2+w);if(wet)continue;}
   float strength,u=GustPhase(place.x+fx/ground.width,place.y+fy/ground.width,H01(t.x+n,t.y+row,3)*.22f,out strength);if(u<0)continue;int frame=GustFrame(u,8);if(strength<.7f&&frame==4)frame=3;
   var art=crop==0?Amb("wheat-"+frame):AmbientMark(Tufts[Mathf.Clamp((int)(Swell(u)*5),0,4)]);if(art==null)return;float size=(7.2f+1.3f*H01(t.x,t.y+row,n))*s;var ink=GUI.color;var dye=crop==0?Color.white:crop==1?new Color(.42f,.66f,.28f):new Color(.84f,.86f,.5f);GUI.color=new Color(ink.r*dye.r,ink.g*dye.g,ink.b*dye.b,ink.a*Mathf.Clamp01(Mathf.Min(u,1-u)*5.5f));GUI.DrawTexture(new Rect(px-size/2,py-size*.97f,size,size),art,ScaleMode.StretchToFill,true);GUI.color=ink;Count("wheat");}}
 // A crown leaning a pixel or two: the trunk and roots are drawn as they are, the upper part in slices that lean more towards the top.
 void DrawSway(Rect r,Texture2D tex,float lean,float amount){
  if(lean<=.03f){GUI.DrawTexture(r,tex);return;}
  const int slices=5;const float top=.62f;GUI.DrawTextureWithTexCoords(new Rect(r.x,r.y+r.height*top,r.width,r.height*(1-top)),tex,new Rect(0,0,1,1-top));
  for(int k=0;k<slices;k++){float v0=top*k/slices,v1=top*(k+1)/slices,up=1-(k+.5f)/slices;float dx=lean*amount*up*up;GUI.DrawTextureWithTexCoords(new Rect(r.x+dx,r.y+r.height*v0,r.width,r.height*(v1-v0)+.6f),tex,new Rect(0,1-v1,1,v1-v0));}
  Count("canopy");}
 // Crown edges on the grove pictures, in their own 64 pixels, where a tuft of leaves catches the gust last.
 static readonly Vector2[][] CrownTips={new[]{new Vector2(31,10),new Vector2(61,18)},new[]{new Vector2(57,17),new Vector2(44,5)},new[]{new Vector2(60,21)},new[]{new Vector2(60,14)}};
 void DrawGrove(Rect r,Texture2D tex,int variant,int i,int j,float s,float sx,bool visible){
  if(!ambientPass||!visible||ambientLevel<1){GUI.DrawTexture(r,tex);return;}
  var place=AmbientPlace(i,j);float ox=(r.center.x-sx)/(64*s);float lean=GustLean(place.x+ox,place.y,.55f);DrawSway(r,tex,lean,1.9f*s);
  if(ambientLevel<2||lean<=.02f||variant>=CrownTips.Length)return;float strength,u=GustPhase(place.x,place.y,.8f,out strength,CanopyWidth);var art=Amb("branch-"+GustFrame(u,6));if(art==null||u<0)return;
  float size=r.width*.15f,fade=Mathf.Clamp01(Mathf.Min(u,1-u)*6);var old=GUI.color;GUI.color=new Color(old.r,old.g,old.b,old.a*fade);foreach(var tip in CrownTips[variant]){GUI.DrawTexture(new Rect(r.x+tip.x/64f*r.width-size*.45f+lean*s*1.5f,r.y+tip.y/64f*r.height-size*.55f,size,size),art,ScaleMode.StretchToFill,true);Count("tuft");}GUI.color=old;}
 // ---------- the broken-silver current ----------
 readonly Dictionary<int,Texture2D> strokeArt=new Dictionary<int,Texture2D>();
 // A tapered mark pointing one of sixteen ways (half a turn is enough: the taper is at both ends, thicker behind the leading edge).
 Texture2D Stroke(int way,bool small){int key=way*2+(small?1:0);Texture2D t;if(strokeArt.TryGetValue(key,out t)&&t!=null)return t;const int n=24;var cv=new PixelCanvas(n,n);double a=way*Math.PI*2/16,ca=Math.Cos(a),sa=Math.Sin(a),half=small?4.2:9.5,thick=small?1.35:1.75;
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){double dx=x+.5-n/2.0,dy=y+.5-n/2.0,al=dx*ca+dy*sa,ac=-dx*sa+dy*ca;double f=(al+half)/(2*half);if(f<0||f>1)continue;double w=thick*Math.Sin(Math.PI*Math.Pow(f,.62));double d=Math.Abs(ac+.35*Math.Sin(al*.8));if(d>w)continue;int alpha=(int)(255*Math.Min(1,(w-d)/.7)*(.55+.45*f));cv.Set(x,y,(uint)alpha<<24|0xFFFFFF);}
  t=SoftTexture(cv);themeTextures.Add(t);strokeArt[key]=t;return t;}
 // The pier of a bridge: how far down the river's line the road crosses it, or -1.
 readonly Dictionary<string,float> pierAt=new Dictionary<string,float>();
 void RoadOnRiver(Tile t,out int roadMask,out int riverMask,out int variant,out int shifted){
  roadMask=0;for(int k=0;k<6;k++)if(world.RoadConnects(t,World.HexDi[k],World.HexDj[k]))roadMask|=1<<k;riverMask=RiverMaskOf(t);variant=(t.x*7+t.y*3)%3;shifted=0;
  for(int r=0;r<6;r++){if((roadMask&(1<<r))==0||(riverMask&(1<<r))==0)continue;if(IsoKnownTerrain(world.Step(t.x,t.y,World.HexDi[r],World.HexDj[r]))==7){shifted|=1<<r;if(((riverMask>>PixelArt.RiverDownShift)&7)-1==r)shifted|=256<<r;}}}
 float PierOf(Tile t,out List<double[]> line){
  int road,river,variant,shifted;RoadOnRiver(t,out road,out river,out variant,out shifted);line=PixelArt.HexRiverLine(river,variant);if(line==null||!world.Bridged(t))return -1;int arms=0;for(int k=0;k<6;k++)if((road&(1<<k))!=0)arms++;if(arms<2)return -1;
  string key=road+"."+river+"."+variant+"."+shifted;float at;if(pierAt.TryGetValue(key,out at))return at;at=-1;double best=2.6;double along=0;
  for(int n=0;n<line.Count;n++){if(n>0){double sx=line[n][0]-line[n-1][0],sy=line[n][1]-line[n-1][1];along+=Math.Sqrt(sx*sx+sy*sy);}for(int k=0;k<6;k++){if((road&(1<<k))==0)continue;foreach(var p in PixelArt.HexRoadArm(road,k,0,river,variant,shifted)){double dx=p[0]-line[n][0],dy=(p[1]-line[n][1])*1.25,d=Math.Sqrt(dx*dx+dy*dy);if(d<best){best=d;at=(float)along;}}}}
  pierAt[key]=at;return at;}
 void DrawCurrent(Tile t,Rect ground,float s){
  if(!ambientPass||ambientLevel<1||t.terrain!=7)return;List<double[]> line=null;float from=-1;
  if(t.road)from=PierOf(t,out line);
  bool mill=world.valleyMill==t.y*world.Width+t.x;if(mill){int mask=RiverMaskOf(t);line=PixelArt.HexRiverLine(mask,(t.x*7+t.y*3)%3);from=line==null?-1:(float)(PixelArt.LineLength(line)*MillShare)+3;if(wheelSpeed<.1f)from=-1;}
  if(from<0||line==null)return;float length=(float)PixelArt.LineLength(line);
  var old=GUI.color;for(int m=0;m<3;m++){
   float period=3.3f+m*.55f,clock=ambientNow/period+m*.37f+H01(t.x,t.y,m);int turn=Mathf.FloorToInt(clock);float p=clock-turn;if(H01(turn,t.x*7+t.y,m)<.1f)continue;// a missed beat now and then
   float lane=(m-1)*.5f+(H01(turn,m,t.x)-.5f)*.3f,run=from+2.5f+m*.8f+p*p*(10f+3*H01(turn,m,5));if(run>length-1)continue;double dx,dy;var at=PixelArt.AlongLine(line,run,out dx,out dy);float radius=(float)PixelArt.HexRiverRadius(RiverMaskOf(t));
   float px=ground.x+(float)((at[0]-dy*lane*radius)/PixelArt.HW)*ground.width,py=ground.y+(float)((at[1]+dx*lane*radius*.8)/PixelArt.HH)*ground.height;int way=((int)Math.Round(Math.Atan2(dy,dx)/(Math.PI*2)*16)%16+16)%16;
   float alpha=Mathf.Clamp01(p*7)*(p<.62f?1:Mathf.Clamp01((1-p)/.38f))*.92f;
   if(p<.62f){float size=(9f+6f*p)*s;GUI.color=new Color(.95f,.99f,1f,alpha);GUI.DrawTexture(new Rect(px-size/2,py-size/2,size,size),Stroke(way,false),ScaleMode.StretchToFill,true);Count("current");}
   else{float gap=(p-.62f)*10f*s,size=8f*s;GUI.color=new Color(.82f,.93f,1f,alpha*.85f);for(int h=-1;h<=1;h+=2){float qx=px+(float)dx*gap*h,qy=py+(float)dy*gap*h+h*(p-.62f)*2f*s;GUI.DrawTexture(new Rect(qx-size/2,qy-size/2,size,size),Stroke(way,true),ScaleMode.StretchToFill,true);}Count("current");}}
  GUI.color=old;}
 // ---------- the valley's mill (decoration of the demonstration; World.valleyMill) ----------
 const float MillShare=.52f;float wheelTurn,wheelSpeed,wheelAt;
 // It works while its city works the land it stands on and is not in disorder.
 bool MillWorking(){int cell=world.valleyMill;if(cell<0||cell>=world.tiles.Length)return false;var t=world.tiles[cell];foreach(var c in world.cities)if(c.owner==0&&!world.Disorder(c)&&c.occupation==0&&world.Distance(c.x,c.y,t.x,t.y)<=5&&world.Worked(c).Contains(t))return true;return false;}
 bool millWorks;float millChecked=-9;
 void UpdateWheel(){if(world.valleyMill<0){wheelSpeed=0;return;}if(ambientNow-millChecked>.5f){millChecked=ambientNow;millWorks=MillWorking();}float dt=Mathf.Clamp(ambientNow-wheelAt,0,.1f);wheelAt=ambientNow;wheelSpeed=Mathf.MoveTowards(wheelSpeed,millWorks?1:0,dt*.8f);wheelTurn+=dt*wheelSpeed*6.5f;}
 void DrawMill(Tile t,Rect ground,float s){
  if(world.valleyMill!=t.y*world.Width+t.x||t.terrain!=7)return;var line=PixelArt.HexRiverLine(RiverMaskOf(t),(t.x*7+t.y*3)%3);var art=Amb("mill");if(line==null||art==null)return;
  double dx,dy;var at=PixelArt.AlongLine(line,PixelArt.LineLength(line)*MillShare,out dx,out dy);float ax=ground.x+(float)(at[0]/PixelArt.HW)*ground.width-1.2f*s,ay=ground.y+(float)(at[1]/PixelArt.HH)*ground.height-4.6f*s;
  // The picture is 64 pixels, two to a map unit; its axle hole is at (43.8, 49.3).
  const float big=1.14f;GUI.DrawTexture(new Rect(ax-21.9f*big*s,ay-24.65f*big*s,32*big*s,32*big*s),art,ScaleMode.StretchToFill,true);
  var wheel=Amb("mill-wheel-"+(((int)Mathf.Floor(wheelTurn)%8+8)%8));if(wheel!=null)GUI.DrawTexture(new Rect(ax-8f*big*s+.9f*s,ay-8f*big*s+.6f*s,16*big*s,16*big*s),wheel,ScaleMode.StretchToFill,true);}
 // ---------- the miller's road ----------
 sealed class AmbientCart{public long key;public City city;public List<Tile> tiles;public List<Vector2> path;public List<int> at;public float length,start,fade=1;public bool dying;public int seed;}
 readonly List<AmbientCart> carts=new List<AmbientCart>();float cartsChecked=-9;
 const float CartSpeed=.30f,CartStop=.9f,CartUnload=3.4f;const int CartReach=4,CartsMost=6;
 // The road from a worked field to its city: tiles joined by road you know, all in sight, on your land or nobody's.
 List<Tile> CartRoute(Tile from,City to){
  if(!from.road||!world.Visible(from.x,from.y))return null;var seen=new Dictionary<int,int>{{from.y*world.Width+from.x,-1}};var q=new Queue<Tile>();q.Enqueue(from);int goal=to.y*world.Width+to.x;var depth=new Dictionary<int,int>{{from.y*world.Width+from.x,0}};
  while(q.Count>0){var t=q.Dequeue();int cell=t.y*world.Width+t.x;if(cell==goal){var path=new List<Tile>();for(int c=cell;c>=0;c=seen[c])path.Add(world.tiles[c]);path.Reverse();return path.Count>=2?path:null;}
   if(depth[cell]>=CartReach)continue;
   for(int k=0;k<6;k++){if(!world.RoadConnects(t,World.HexDi[k],World.HexDj[k]))continue;var n=world.Step(t.x,t.y,World.HexDi[k],World.HexDj[k]);if(n==null||!n.road||n.owner>0||!world.Visible(n.x,n.y))continue;int at=n.y*world.Width+n.x;if(seen.ContainsKey(at))continue;seen[at]=cell;depth[at]=depth[cell]+1;q.Enqueue(n);}}
  return null;}
 static int WayBetween(World w,Tile a,Tile b){for(int k=0;k<6;k++){var n=w.Step(a.x,a.y,World.HexDi[k],World.HexDj[k]);if(n!=null&&n.x==b.x&&n.y==b.y)return k;}return -1;}
 List<double[]> ArmOf(Tile t,int k){int mask=0;for(int q=0;q<6;q++)if(world.RoadConnects(t,World.HexDi[q],World.HexDj[q]))mask|=1<<q;if(t.terrain==7){int road,river,variant,shifted;RoadOnRiver(t,out road,out river,out variant,out shifted);if((river&63)!=0)return PixelArt.HexRoadArm(road,k,0,river,variant,shifted);}return PixelArt.HexRoadArm(mask,k,RoadLook(t)&3,0,0,0);}
 // The route as points on the map (tile widths), each with the tile of the route it lies on.
 void LayCart(AmbientCart c){
  c.path=new List<Vector2>();c.at=new List<int>();float ux=c.tiles[0].x;
  for(int n=0;n<c.tiles.Count;n++){var t=c.tiles[n];if(n>0)ux+=world.DeltaX(c.tiles[n-1].x,t.x);
   Action<List<double[]>,bool> lay=(pts,back)=>{for(int q=0;q<pts.Count;q++){var p=pts[back?pts.Count-1-q:q];c.path.Add(new Vector2(ux+(t.y&1)*.5f+(float)(p[0]-PixelArt.HW/2.0)/PixelArt.HW,t.y*PixelArt.HR/(float)PixelArt.HW+(float)(p[1]-PixelArt.HH/2.0)/PixelArt.HW));c.at.Add(n);}};
   if(n>0){int k=WayBetween(world,t,c.tiles[n-1]);if(k>=0)lay(ArmOf(t,k),false);}
   if(n<c.tiles.Count-1){int k=WayBetween(world,t,c.tiles[n+1]);if(k>=0)lay(ArmOf(t,k),true);}}
  c.length=0;for(int q=1;q<c.path.Count;q++)c.length+=(c.path[q]-c.path[q-1]).magnitude;
  c.length=Mathf.Max(.2f,c.length-.33f);}// the cart stops at the edge of the town, not in the middle of it
 long CartKey(List<Tile> tiles){unchecked{long h=17;foreach(var t in tiles)h=h*1000003+t.y*world.Width+t.x;return h;}}
 void UpdateCarts(){
  if(ambientLevel<1){return;}
  if(ambientNow-cartsChecked>=.5f){cartsChecked=ambientNow;var wanted=new Dictionary<long,KeyValuePair<City,List<Tile>>>();
   foreach(var city in world.cities){if(city.owner!=0||world.Disorder(city)||city.occupation>0||wanted.Count>=CartsMost)continue;var ct=world.At(city.x,city.y);if(ct==null||ct.realm!=viewRealm||Mathf.Abs(world.DeltaX((int)camI,city.x))>MapW/isoWidth+6||Mathf.Abs(city.y-camJ)>MapH/RowPixels(isoWidth)+8)continue;
    int most=city.pop>=8?2:1,made=0;foreach(var t in world.Worked(city).Where(t=>t.road&&(t.farm||world.valleyMill==t.y*world.Width+t.x)&&!(t.x==city.x&&t.y==city.y)).OrderByDescending(t=>world.valleyMill==t.y*world.Width+t.x).ThenBy(t=>t.y*world.Width+t.x)){if(made>=most)break;var route=CartRoute(t,city);if(route==null)continue;long key=CartKey(route)*31+city.id;if(wanted.ContainsKey(key))continue;wanted[key]=new KeyValuePair<City,List<Tile>>(city,route);made++;}}
   foreach(var c in carts)if(!wanted.ContainsKey(c.key))c.dying=true;
   foreach(var kv in wanted){if(carts.Any(c=>c.key==kv.Key&&!c.dying))continue;var c2=new AmbientCart{key=kv.Key,city=kv.Value.Key,tiles=kv.Value.Value,seed=(int)(kv.Key&0x7fff),start=ambientNow+.6f};LayCart(c2);if(c2.path.Count>=2&&c2.length>.2f)carts.Add(c2);}}
  for(int n=carts.Count-1;n>=0;n--){var c=carts[n];if(c.dying){c.fade-=Time.unscaledDeltaTime/.5f*.5f;if(c.fade<=0)carts.RemoveAt(n);}}}
 // Where a cart is now: how far along (0 at the field), which way it faces, whether it is loaded, and how solid it is. False while it is
 // out of sight behind the mill or the houses.
 bool CartNow(AmbientCart c,out float along,out bool outward,out bool loaded,out float solid,out bool rolling){
  float travel=c.length/CartSpeed,rest=3.5f+4f*H01(c.seed,3),total=travel*2+CartStop+CartUnload+rest,t=(ambientNow-c.start)%total;if(t<0)t=0;along=0;outward=true;loaded=true;solid=1;rolling=true;
  if(t<travel){along=Mathf.SmoothStep(0,c.length,Mathf.Clamp01(t/travel))*.25f+c.length*(t/travel)*.75f;solid=Mathf.Clamp01(t/.5f);return true;}
  t-=travel;if(t<CartStop){along=c.length;rolling=false;solid=Mathf.Clamp01((CartStop-t)/.35f);return true;}
  t-=CartStop;if(t<CartUnload)return false;t-=CartUnload;
  if(t<travel){along=c.length-(Mathf.SmoothStep(0,c.length,Mathf.Clamp01(t/travel))*.25f+c.length*(t/travel)*.75f);outward=false;loaded=false;solid=Mathf.Clamp01(Mathf.Min(t/.5f,(travel-t)/.4f));return true;}
  return false;}
 // Drawn once every tile of a row is down, so no neighbour's ground cuts it; the next row's trees and roofs still cover it, and the
 // houses of the city it has reached are laid over it again.
 void FlushAmbientRow(int row){
  FlushRowAir();
  if(!ambientPass||ambientLevel<1||carts.Count==0)return;
  foreach(var c in carts){float along,solid;bool outward,loaded,rolling;if(!CartNow(c,out along,out outward,out loaded,out solid,out rolling))continue;
   Vector2 p=c.path[0],way=Vector2.right;int tile=0;float left=along;for(int q=1;q<c.path.Count;q++){var d=c.path[q]-c.path[q-1];float l=d.magnitude;tile=c.at[q];if(l>0&&left<=l){p=c.path[q-1]+d*(left/l);way=d/l;break;}left-=l;p=c.path[q];if(l>0)way=d/l;}
   var on=c.tiles[tile];if(on.y!=row||!world.Visible(on.x,on.y))continue;if(!outward)way=-way;
   float s=isoWidth/64f,w=world.Width,X=p.x;if(world.Sphere)X+=w*Mathf.Round((camI-X)/w);float sx=IsoCentreX+(X-camI)*isoWidth,sy=IsoCentreY+p.y*isoWidth-camJ*RowPixels(isoWidth);
   int wheel=rolling?((int)(along*64/1.7f)%4+4)%4:0;var art=Amb("cart-"+(loaded?"loaded-":"empty-")+(outward?wheel:(4-wheel)%4));if(art==null)return;
   float size=15.5f*s,jolt=rolling?Mathf.Abs(Mathf.Sin(along*46))*.28f*s:0;var r=new Rect(sx-size/2,sy-size*.89f-jolt,size,size);var old=GUI.color;GUI.color=new Color(1,1,1,solid*Mathf.Clamp01(c.fade));
   var shade=new Color(0,0,0,.2f*GUI.color.a);{var keep=GUI.color;GUI.color=shade;GUI.DrawTexture(new Rect(sx-size*.38f,sy-size*.1f,size*.76f,size*.2f),IsoPicture(4,8,0));GUI.color=keep;}
   if(way.x<-.05f)GUI.DrawTextureWithTexCoords(r,art,new Rect(1,0,-1,1));else GUI.DrawTexture(r,art,ScaleMode.StretchToFill,true);GUI.color=old;Count("cart");
   if(tile==c.tiles.Count-1){float csx=IsoCentreX+(c.path[c.path.Count-1].x-p.x+X-camI)*isoWidth;var cityRect=ambientCity.ContainsKey(c.city.id)?ambientCity[c.city.id]:default(Rect);if(cityRect.width>0&&sy<cityRect.y+cityRect.height*.82f)DrawCityArt(c.city,cityRect);}
   if(tile==0&&world.valleyMill==on.y*world.Width+on.x&&ambientMill.width>0&&sy<ambientMill.y+ambientMill.height*.62f)DrawMill(on,ambientMill,s);}}
 readonly Dictionary<int,Rect> ambientCity=new Dictionary<int,Rect>();Rect ambientMill;
 // ---------- the occupied hearth ----------
 readonly Dictionary<int,float> hearthBorn=new Dictionary<int,float>();readonly HashSet<int> hearthKnown=new HashSet<int>();
 readonly Dictionary<Texture2D,List<Vector2>> roofs=new Dictionary<Texture2D,List<Vector2>>();
 // Roof peaks of a settlement picture: columns where its top stands above both sides, not leaves, spaced apart. Shares of the picture.
 List<Vector2> RoofPeaks(Texture2D tex){
  List<Vector2> found;if(roofs.TryGetValue(tex,out found))return found;found=new List<Vector2>();try{var px=ReadArtPixels(tex);int w=tex.width,h=tex.height;var top=new int[w];for(int x=0;x<w;x++){top[x]=h;for(int y=0;y<h;y++){uint p=px[y*w+x];if((p>>24)<160)continue;top[x]=y;break;}}
   var peaks=new List<int>();int reach=Math.Max(2,w/16);for(int x=reach;x<w-reach;x++){if(top[x]>=h*.75f)continue;bool peak=true;for(int d=1;d<=reach&&peak;d++)if(top[x-d]<top[x]||top[x+d]<top[x])peak=false;if(!peak)continue;int green=0,all=0;for(int y=top[x]+1;y<Math.Min(h,top[x]+Math.Max(4,h/12));y++)for(int q=-1;q<=1;q++){uint p=px[y*w+x+q];int r=(int)(p>>16&255),g=(int)(p>>8&255),b=(int)(p&255);all++;if(g>r+6&&g>b+6)green++;}if(green*2>all)continue;peaks.Add(x);}
   foreach(int x in peaks.OrderBy(x=>top[x])){if(found.Any(f=>Mathf.Abs(f.x*w-x)<w*.16f))continue;found.Add(new Vector2((x+.5f)/w,(top[x]+1.5f)/h));if(found.Count>=4)break;}}catch(Exception){}
  if(found.Count==0)found.Add(new Vector2(.5f,.3f));roofs[tex]=found;return found;}
 void DrawHearths(City c,Rect r,float s,int i,int j){
  if(!ambientPass||c==null||c.pop<=0)return;ambientCity[c.id]=r;var tex=SettlementTexture(CityVisualEra(c.owner),c.pop,SettlementFlags(c),c.id);if(tex==null)return;var peaks=RoofPeaks(tex);int hearths=ambientLevel==0?1:Math.Min(peaks.Count,c.pop>=12?4:c.pop>=8?3:c.pop>=4?2:1);float born;bool isNew=hearthBorn.TryGetValue(c.id,out born);var place=AmbientPlace(i,j);
  for(int h=0;h<hearths;h++){
   float period=6.2f+3.6f*H01(c.id,h,1)+(ambientLevel==0?4:0),clock=isNew&&h==0?ambientNow-born:ambientNow+H01(c.id,h,2)*period;if(clock<0)continue;int turn=Mathf.FloorToInt(clock/period);float into=clock-turn*period,gap=isNew&&h==0&&turn==0?0:H01(turn,c.id,h)*(period-3.8f);float life=(into-gap)/3.6f;if(life<0||life>1)continue;if(turn>0&&H01(turn,h,c.id+7)<.14f)continue;
   var at=new Vector2(r.x+peaks[h].x*r.width,r.y+peaks[h].y*r.height);float lean=GustLean(place.x,place.y,.35f);bool first=isNew&&h==0&&turn==0;
   float size=(first?16.5f:13.5f)*s,grow=life<.25f?Mathf.Lerp(.35f,1,life/.25f):1;int frame=life<.25f?0:Mathf.Clamp((int)((life-.25f)/.75f*8),0,7);var art=Amb("smoke-"+frame);if(art==null)return;
   float alpha=(first?.95f:.72f)*Mathf.Clamp01(life*6)*(life>.6f?(1-life)/.4f:1);var old=GUI.color;GUI.color=new Color(1,.985f,.95f,alpha);float w=size*Mathf.Lerp(.6f,1,grow),tall=size*grow,drift=(.6f+lean*3.2f)*life*life*s,up=life*3.5f*s;
   // The chimney's mouth stays put: the picture's foot is at its bottom middle, and the wind takes the top.
   const int slices=4;for(int k=0;k<slices;k++){float v0=k/(float)slices,v1=(k+1)/(float)slices,share=1-(v0+v1)/2;GUI.DrawTextureWithTexCoords(new Rect(at.x-w*.51f+drift*share*share*2.2f,at.y-tall-up*(life>.55f?(life-.55f)/.45f:0)+tall*v0,w,tall/slices+.4f),art,new Rect(0,1-v1,1,v1-v0));}
   GUI.color=old;Count("smoke");}}
 // ---------- the wrong-way leaves ----------
 struct AmbientGate{public Tile tile;public Vector2 at;public float fit;}
 readonly List<AmbientGate> ambientGates=new List<AmbientGate>();
 // A gateway in sight that the player knows and that is shut.
 void NoteAmbientGate(Tile source,bool visible,float x,float y,float fit){
  if(!ambientPass||ambientLevel<1||!visible||source.gateLink<=0||world.GateOpen(source))return;if(source.realm==0&&(world.gatesFound==null||!world.gatesFound.Contains(source.y*world.Width+source.x)))return;ambientGates.Add(new AmbientGate{tile=source,at=new Vector2(x,y),fit=fit});}
 bool GateOverWater(Tile g){if(!world.Hex)return false;foreach(int k in new[]{1,2}){var n=world.Step(g.x,g.y,World.HexDi[k],World.HexDj[k]);if(n!=null&&n.seen&&PixelArt.IsWater(n.terrain)&&n.terrain!=7)return true;}return false;}
 // Your own people are opening it: the only awakening you can know of.
 bool GateWakes(Tile g){var study=world.StudyOf(0,g);return study!=null&&study.known&&study.approach!=GateApproach.None;}
 static float Beat(float t,float rise,float hold,float fall){return t<0?0:t<rise?Mathf.SmoothStep(0,1,t/rise):t<rise+hold?1:t<rise+hold+fall?Mathf.SmoothStep(1,0,(t-rise-hold)/fall):0;}
 void DrawAmbientGates(float s){
  FlushHighAir();
  if(!ambientPass||ambientLevel<1)return;var old=GUI.color;
  foreach(var g in ambientGates){
   bool wakes=GateWakes(g.tile);float w=64*s*GateScale*g.fit,foot=g.fit>=1?g.at.y+TileHalf()*s*.5f:g.at.y+w*.16f;var open=SpriteSlot("gate-open-"+world.GateStyle(g.tile));
   bool water=false;
   if(wakes&&open!=null){float t=ambientNow%8.5f,shine=Beat(t,.9f,2.2f,1.6f),stone=Beat(t-.85f,.9f,1.6f,1.8f);
    // The reflection comes a beat before the colour, on water in sight below the gateway.
    for(int k=-1;k<6;k++){if(k>=0&&(k==4||k==5))continue;var n=k<0?g.tile:world.Step(g.tile.x,g.tile.y,World.HexDi[k],World.HexDj[k]);if(n==null||!world.Visible(n.x,n.y))continue;bool river=n.terrain==7;if(!river&&!PixelArt.IsWater(n.terrain)||k<0&&!river)continue;
     var tex=Reflection(g,n,k,river);if(tex==null)continue;water=true;if(shine<=.01f)continue;int ddx=k<0?0:k==0?64:k==1?32:k==2?-32:-64,ddy=k<0?0:k==1||k==2?PixelArt.HR:0;var centre=GateTileCentre(g,s);var rect=TileRect(centre.x+ddx*s,centre.y+ddy*s,s);GUI.color=new Color(1,1,1,.86f*shine*(.88f+.12f*Mathf.Sin(ambientNow*5)));GUI.DrawTexture(rect,tex);Count("reflection");}
    if(stone>.01f){float h=w*open.height/Mathf.Max(1,open.width);GUI.color=new Color(1,1,1,.55f*stone);GUI.DrawTexture(new Rect(g.at.x-w/2,foot+h*GateBaseline-h,w,h),open,ScaleMode.ScaleToFit,true);Count("stone");}}
   // Two or three leaves go in against the wind, now and then; a waking gateway inland draws them more often.
   float period=wakes&&!water?8f:21f+6*H01(g.tile.x,g.tile.y),clock=ambientNow/period+H01(g.tile.y,g.tile.x);int turn=Mathf.FloorToInt(clock);float into=(clock-turn)*period;int leaves=2+(H01(turn,g.tile.x)>.5f?1:0);
   for(int n=0;n<leaves;n++){float life=(into-n*.55f-H01(turn,n)*.4f)/(3.6f+n*.35f);if(life<0||life>1)continue;float pull=life*life*(1.6f-.6f*life);
    float sx0=g.at.x+(30+9*H01(turn,n,1))*s,sy0=foot-(1+10*H01(turn,n,2))*s,ex=g.at.x+(n-1)*1.1f*s,ey=foot-w*.34f;float x=Mathf.Lerp(sx0,ex,pull),y=Mathf.Lerp(sy0,ey,pull)-Mathf.Sin(life*Mathf.PI)*5*s+Mathf.Sin(life*13+n*2)*1.5f*s*(1-life);
    float size=(3.5f+.7f*H01(turn,n,4))*s,alpha=Mathf.Clamp01(life*6)*Mathf.Clamp01((1-life)*7);var art=Amb("leaf-"+((n+turn)&1));if(art==null)break;float flip=Mathf.Cos(life*11+n);GUI.color=new Color(1,1,1,alpha);GUI.DrawTextureWithTexCoords(new Rect(x-size*Mathf.Abs(flip)/2,y-size/2,Mathf.Max(1,size*Mathf.Abs(flip)),size),art,flip>=0?new Rect(0,0,1,1):new Rect(1,0,-1,1));Count("leaf");}}
  GUI.color=old;}
 Vector2 GateTileCentre(AmbientGate g,float s){float sx,sy;IsoScreen(g.tile.x,g.tile.y,out sx,out sy);return new Vector2(sx,sy);}
 readonly Dictionary<string,Texture2D> reflections=new Dictionary<string,Texture2D>();
 // The open gateway upside down, only where that tile is water, broken by level bands.
 Texture2D Reflection(AmbientGate g,Tile n,int k,bool river){
  int style=world.GateStyle(g.tile),mask=river?RiverMaskOf(n):0,variant=(n.x*7+n.y*3)%3;string key=style+"."+k+"."+mask+"."+variant+"."+(int)(g.fit*100)+"."+(g.tile.x*7919+g.tile.y);Texture2D t;if(reflections.TryGetValue(key,out t))return t;if(reflections.Count>40)reflections.Clear();
  var art=SpriteSlot("gate-open-"+style);t=null;if(art!=null){try{var px=ReadArtPixels(art);float w=64*GateScale*g.fit,h=w*art.height/Mathf.Max(1,art.width);float sx,sy;IsoScreen(g.tile.x,g.tile.y,out sx,out sy);float scale=isoWidth/64f,ox=(g.at.x-sx)/scale,oy=(g.at.y-sy)/scale,foot=g.fit>=1?oy+TileHalf()*.5f:oy+w*.16f;float ddx=k<0?0:k==0?64:k==1?32:k==2?-32:-64,ddy=k<0?0:k==1||k==2?PixelArt.HR:0;
   var cv=new PixelCanvas(PixelArt.HW*2,PixelArt.HH*2);int drawn=0;for(int y=0;y<cv.h;y++)for(int x=0;x<cv.w;x++){if(river&&!PixelArt.HexRiverWet(mask,variant,x,y))continue;if(!river&&!(PixelArt.HexInside(x/2,y/2)&&PixelArt.HexInside(x/2,y/2-6)))continue;float px0=x/2f-PixelArt.HW/2f+ddx-ox,py0=y/2f-PixelArt.HH/2f+ddy-foot-(river?0:7);float wave=Mathf.Sin(y*.55f)*1.1f,u=.5f+(px0+wave)/w,v=1-GateBaseline*1.6f-py0/(h*.8f);if(u<0||u>=1||v<0||v>=1||py0<0)continue;uint c=px[(int)(v*art.height)*art.width+(int)(u*art.width)];if((c>>24)<90)continue;float depth=1.15f-py0/(h*.8f);float band=((y/3)&3)==0?.35f:1f;int al=(int)(255*Mathf.Clamp01(depth*1.1f)*band);uint r=c>>16&255,gg=c>>8&255,b=c&255;r=(r*2+60)/3;gg=(gg*2+150)/3;b=(b*2+215)/3;cv.Set(x,y,(uint)al<<24|r<<16|gg<<8|b);drawn++;}
   if(drawn>12){t=SoftTexture(cv);themeTextures.Add(t);}}catch(Exception){t=null;}}
  reflections[key]=t;return t;}
}
}
