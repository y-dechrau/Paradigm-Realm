using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The mini-map, redrawn after Stellaris's galaxy map (owner request, 25 Nov 2026: "needs to be clearer"). Dark, quiet ground; each
// civilisation's land one flat field of its colour with a bright, rounded border and a thin dark line outside it; unexplored land near
// black; coasts a fine pale line. Cities are crisp dots drawn over the picture at their own size on screen, capitals stars, gateways
// you have found gold diamonds, and a gateway you have only heard of a gold arrow from your capital pointing the way. The other layers
// use the same shapes with their own colours. Each tile is a point on the map's real rows, and every pixel belongs to the group of
// tiles that outweighs the others round it, which is what rounds the borders. The atlas texture (one pixel a tile) is still used by
// the globe and the atlas screen.
public partial class RealmGame {
 Texture2D miniTexture;World miniWorld;float miniBuilt=-99;int miniRealm=-1;Color32[] miniPixels;int[] miniKey;
 static readonly Color MiniUnknown=new Color(.016f,.024f,.043f),MiniSea=new Color(.045f,.075f,.14f),MiniLand=new Color(.2f,.225f,.25f),MiniCoast=new Color(.36f,.44f,.54f);
 // Rows as tall as they are on the map: the share of a tile's width.
 float MiniRowShare(){return world.Hex?PixelArt.HR/(float)PixelArt.HW:world.Staggered?.25f:1f;}
 float MiniShift(int y){return world.Rows&&(y&1)!=0?.5f:0f;}
 void BuildMiniTexture(){float began=Time.realtimeSinceStartup;
  int W=world.Width,H=ViewRows,top=ViewTop;float share=MiniRowShare();int scale=Mathf.Clamp(640/Math.Max(1,W),3,9);int tw=W*scale,th=Mathf.Max(8,Mathf.RoundToInt(H*scale*share));
  if(miniTexture==null||miniTexture.width!=tw||miniTexture.height!=th){if(miniTexture!=null)Destroy(miniTexture);miniTexture=new Texture2D(tw,th,TextureFormat.RGBA32,true){filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp,anisoLevel=2};miniPixels=new Color32[tw*th];miniKey=new int[tw*th];}
  PrepareMiniLayer();
  // What every tile shows: its kind (0 unknown, 1 water, 2 land), the group its land belongs to, and its colour on this layer.
  var key=miniTileKey=new int[W*H];var ink=miniTileInk=new Color[W*H];
  for(int y=0;y<H;y++)for(int x=0;x<W;x++){int at=y*W+x,cell=(y+top)*W+x;var t=world.KnownTile(world.tiles[cell]);if(!t.seen){key[at]=0;ink[at]=MiniUnknown;continue;}
   bool water=PixelArt.IsWater(t.terrain)&&t.terrain!=7;key[at]=t.owner>=0?10+t.owner:water?1:2;ink[at]=MiniTileColour(cell,t,water);}
  bool wrap=world.Sphere;for(int py=0;py<th;py++)PaintMiniRow(py,tw,th,scale,scale*share,0,0,miniPixels,miniKey);
  miniPixels=EdgeMini(miniPixels,miniKey,tw,th,wrap);if(devRun)Debug.Log("PROBEINFO minimap "+tw+"x"+th+" painted in "+((Time.realtimeSinceStartup-began)*1000).ToString("0")+" ms");
  PaintMiniOverlay(miniPixels,tw,th,scale,scale*share,top,0,0);miniSig=MiniSignature();miniRepaints++;
  miniTexture.SetPixels32(miniPixels);miniTexture.Apply(true);miniWorld=world;miniBuilt=Time.realtimeSinceStartup;miniRealm=viewRealm;miniLayerBuilt=MiniLayer;
 }
 // One row of pixels of a picture whose corner is at column x0, row y0 of the world, scale pixels a column and rowPixels a row.
 void PaintMiniRow(int py,int tw,int th,float scale,float rowPixels,float x0,float y0,Color32[] pixels,int[] keys){
  int W=world.Width,H=ViewRows,top=ViewTop;float share=rowPixels/scale,reach=1.18f*scale;bool wrap=world.Sphere;var key=miniTileKey;var ink=miniTileInk;
  {float fy=(py+.5f)/rowPixels-.5f+y0;int r0=Mathf.FloorToInt(fy-1.2f/share*.75f),r1=Mathf.CeilToInt(fy+1.2f/share*.75f);
   for(int px=0;px<tw;px++){
    // The tiles round this pixel vote for their group; near tiles count most.
    int n=0;float nearest=float.MaxValue;int nearAt=-1;
    for(int ry=Math.Max(0,r0);ry<=Math.Min(H-1,r1);ry++){float shift=MiniShift(ry+top),dy=(ry-fy)*rowPixels;if(dy>reach||dy<-reach)continue;float fx=(px+.5f)/scale-.5f-shift+x0;int c=Mathf.RoundToInt(fx);
     for(int k=c-1;k<=c+1;k++){int qx=k;if(qx<0||qx>=W){if(!wrap)continue;qx=(qx%W+W)%W;}float dx=(k-fx)*scale,d=dx*dx+dy*dy;if(d>reach*reach)continue;
      int at=ry*W+qx;if(d<nearest){nearest=d;nearAt=at;}float weight=reach-Mathf.Sqrt(d);weight*=weight;int g=key[at],slot=0;while(slot<n&&voteKey[slot]!=g)slot++;if(slot==n){if(n>=voteKey.Length)continue;n++;voteKey[slot]=g;voteSum[slot]=0;voteAt[slot]=at;voteNear[slot]=float.MaxValue;}
      voteSum[slot]+=weight;if(d<voteNear[slot]){voteNear[slot]=d;voteAt[slot]=at;}}}
    int best=-1;for(int s=0;s<n;s++)if(best<0||voteSum[s]>voteSum[best])best=s;
    int to=(th-1-py)*tw+px;if(best<0){keys[to]=0;pixels[to]=MiniUnknown;continue;}
    keys[to]=voteKey[best];pixels[to]=ink[voteAt[best]];}}
 }
 static readonly int[] voteKey=new int[12],voteAt=new int[12];static readonly float[] voteSum=new float[12],voteNear=new float[12];
 // Borders: two bright pixels inside a civilisation's land, a dark one outside it, and a fine line where known land meets water.
 Color32[] EdgeMini(Color32[] pixels,int[] keys,int tw,int th,bool wrap){
  var made=(Color32[])pixels.Clone();
  for(int y=0;y<th;y++)for(int x=0;x<tw;x++){int at=y*tw+x,k=keys[at];int near=0;bool otherOwner=false,land=false;
   for(int d=1;d<=2&&near==0;d++)for(int q=0;q<4;q++){int nx=x+(q==0?d:q==1?-d:0),ny=y+(q==2?d:q==3?-d:0);if(ny<0||ny>=th)continue;if(nx<0||nx>=tw){if(!wrap)continue;nx=(nx+tw)%tw;}int o=keys[ny*tw+nx];if(o==k)continue;
     if(k>=10){near=d;break;}
     if(o>=10&&d==1)otherOwner=true;if(d==1&&(k==1&&o==2||k==2&&o==1))land=true;}
   if(k>=10&&near>0){var ink=BorderInk(k-10);made[at]=Color.Lerp(ink,Color.white,near==1?.38f:.12f);}
   else if(otherOwner)made[at]=Color.Lerp(pixels[at],Color.black,.7f);
   else if(land)made[at]=Color.Lerp(pixels[at],MiniCoast,.75f);}
  return made;
 }
 // On the Diplomacy, Religion and other layers the border keeps the colour the layer gives that people.
 Color BorderInk(int owner){return MiniLayer==4?DiplomacyInk(owner):NationInk(owner);}
 Color MiniTileColour(int cell,Tile t,bool water){
  Color c;
  if(MiniLayer==0){c=water?Color.Lerp(AtlasColour(t.terrain),MiniSea,.45f):Color.Lerp(AtlasColour(t.terrain),Color.black,.06f);if(t.owner>=0)c=Color.Lerp(c,NationInk(t.owner),.3f);c.a=1;return c;}
  if(MiniLayer==1){if(t.owner>=0){c=Color.Lerp(MiniUnknown,NationInk(t.owner),.66f);c.a=1;return c;}if(water)return MiniSea;
   // Unclaimed land is one quiet field; only polar ice stands out a little.
   c=t.terrain==14||t.terrain==15?Color.Lerp(MiniLand,new Color(.5f,.62f,.72f),.3f):MiniLand;c.a=1;return c;}
  return MiniLayerColour(cell,t,AtlasColour(t.terrain));
 }
 // ---------- marks drawn over the picture, at their own size on screen ----------
 readonly Dictionary<string,Texture2D> miniMarks=new Dictionary<string,Texture2D>();
 // Small white shapes, tinted when drawn: dot, ring, star, diamond, arrow0 to arrow7 (north, then clockwise by eighths).
 Texture2D MiniMark(string kind){
  Texture2D t;if(miniMarks.TryGetValue(kind,out t)&&t!=null)return t;const int n=32;t=new Texture2D(n,n,TextureFormat.RGBA32,true){filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp};var px=new Color32[n*n];
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){float cover=0;for(int s=0;s<4;s++){float fx=(x+.25f+.5f*(s&1)-n/2f)/(n/2f),fy=(y+.25f+.5f*(s>>1)-n/2f)/(n/2f);if(kind.StartsWith("arrow")){float turn=(kind[5]-'0')*Mathf.PI/4,lx=fx*Mathf.Cos(turn)-fy*Mathf.Sin(turn),ly=fx*Mathf.Sin(turn)+fy*Mathf.Cos(turn);fx=lx;fy=ly;}float d=Mathf.Sqrt(fx*fx+fy*fy);bool on=false;
    switch(kind.StartsWith("arrow")?"arrow":kind){case "dot":on=d<=.95f;break;case "ring":on=d<=.95f&&d>=.68f;break;case "diamond":on=Mathf.Abs(fx)+Mathf.Abs(fy)<=.95f;break;
     case "star":{float a=Mathf.Atan2(fx,fy);float w=Mathf.Abs(Mathf.Cos(a*2.5f));on=d<=Mathf.Lerp(.42f,.98f,w*w*w);break;}
     case "arrow":on=fy<=.95f&&fy>=-.1f&&Mathf.Abs(fx)<=(.95f-fy)*.75f||fy<-.1f&&fy>=-.95f&&Mathf.Abs(fx)<=.2f;break;}
    if(on)cover+=.25f;}
   px[y*n+x]=new Color32(255,255,255,(byte)(cover*255));}
  t.SetPixels32(px);t.Apply(true);themeTextures.Add(t);miniMarks[kind]=t;return t;}
 void Mark(string kind,Vector2 at,float size,Color ink){var old=GUI.color;GUI.color=ink;GUI.DrawTexture(new Rect(at.x-size/2,at.y-size/2,size,size),MiniMark(kind),ScaleMode.StretchToFill,true);GUI.color=old;}
 Vector2 MiniPoint(Rect drawn,int x,int y){float sx=drawn.width/world.Width,sy=drawn.height/ViewRows;return new Vector2(drawn.x+(x+.5f+MiniShift(y))*sx,drawn.y+(y-ViewTop+.5f)*sy);}
 // drawn: the whole world in the current group; seen: the part of the group that shows.
 void DrawMiniMarks(Rect drawn,Rect seen){
  if(Event.current.type!=EventType.Repaint)return;
  var dark=new Color(0,0,0,.85f);
  // The Resources view: each known resource a dot at screen size, so it reads at any zoom (one pixel per tile does not).
  if(MiniLayer==7){float size=Mathf.Clamp(seen.width/world.Width*1.6f,4f,9f);
   foreach(var t in world.tiles){if(t.realm!=viewRealm||!t.seen||t.resource<=0)continue;var ink=ResourceInk(t);if(ink.a<=0)continue;var at=MiniPoint(drawn,t.x,t.y);if(!seen.Contains(at))continue;Mark("dot",at,size+2,dark);Mark("dot",at,size,ink);}}
  foreach(var c in world.cities){var t=world.At(c.x,c.y);if(t==null||t.realm!=viewRealm||!(c.owner==0||t.seen))continue;var at=MiniPoint(drawn,c.x,c.y);bool capital=world.nations[c.owner].capital==c.id&&(c.owner==0||world.met[c.owner]);var ink=c.owner==0?Color.white:Color.Lerp(NationInk(c.owner),Color.white,.55f);
   if(capital){Mark("star",at,c.owner==0?14.5f:11.5f,dark);Mark("star",at,c.owner==0?11.5f:9f,c.owner==0?new Color(1f,.9f,.5f):ink);}
   else{Mark("dot",at,c.owner==0?6.5f:5.5f,dark);Mark("dot",at,c.owner==0?4.4f:3.5f,ink);}}
  if(viewRealm==0&&world.gatesFound!=null)foreach(int cell in world.gatesFound){var g=world.tiles[cell];var at=MiniPoint(drawn,g.x,g.y);Mark("diamond",at,10,dark);Mark("diamond",at,7.5f,world.GateOpen(g)?new Color(.5f,.9f,1f):new Color(1f,.82f,.25f));}
  // Where to look for a gateway you have only heard of: the way, not the place.
  if(viewRealm==0){var home=world.CapitalCity(0);if(home!=null&&world.At(home.x,home.y).realm==0){var from=MiniPoint(drawn,home.x,home.y);if(seen.Contains(from)){var told=new HashSet<int>();
    foreach(var h in world.OpenHints()){if(!told.Add(h.direction))continue;float a=h.direction*Mathf.PI/4;var way=new Vector2(Mathf.Sin(a),-Mathf.Cos(a));float pulse=.65f+.35f*Mathf.Sin(Time.realtimeSinceStartup*2.5f);var at=from+way*17;
     if(!seen.Contains(at))at=new Vector2(Mathf.Clamp(at.x,seen.x+5,seen.xMax-5),Mathf.Clamp(at.y,seen.y+5,seen.yMax-5));
     string arrow="arrow"+(h.direction&7);Mark(arrow,at,16,dark);Mark(arrow,at,12.5f,new Color(1f,.82f,.25f,pulse));}}}}
 }
 // Whose land the pointer is over, beside it.
 void DrawMiniTip(Rect drawn){
  var m=Event.current.mousePosition;if(!drawn.Contains(m)||!MiniInner.Contains(m)||miniLayerOpen)return;int row=Mathf.Clamp((int)((m.y-drawn.y)/(drawn.height/ViewRows)),0,ViewRows-1),y=ViewTop+row;int x=Mathf.FloorToInt((m.x-drawn.x)/(drawn.width/world.Width)-MiniShift(y));if(x<0||x>=world.Width)return;
  var t=world.KnownTile(world.tiles[y*world.Width+x]);string s=!t.seen?"Unexplored":t.owner<0?World.TerrainNames[t.terrain]:t.owner==0||world.met[t.owner]?world.NationName(t.owner):"An unknown people";
  var city=t.seen?world.cities.Find(c=>(c.owner==0||world.At(c.x,c.y).seen)&&world.At(c.x,c.y).realm==viewRealm&&(MiniPoint(drawn,c.x,c.y)-m).sqrMagnitude<30):null;if(city!=null)s=city.name+" • "+(city.owner==0||world.met[city.owner]?world.NationName(city.owner):"An unknown people");
  var st=new GUIStyle(hudSmall){fontSize=12,wordWrap=false,alignment=TextAnchor.MiddleLeft};var size=st.CalcSize(new GUIContent(s));var r=new Rect(Mathf.Min(m.x-size.x/2-6,MiniMap.xMax-size.x-12),MiniMap.yMax-23,size.x+12,20);r.x=Mathf.Max(MiniMap.x,r.x);
  Fill(r,new Color(.03f,.05f,.1f,.92f));GUI.Label(new Rect(r.x+6,r.y,size.x+4,r.height),s,st);
 }
 // The gold hover lists where the money goes (owner request, 9 Nov 2026: road upkeep must be visible).
 string goldTip="";float goldTipAt=-9;
 string GoldTooltipDetail(){if(Time.realtimeSinceStartup-goldTipAt<.5f)return goldTip;goldTipAt=Time.realtimeSinceStartup;var b=world.BudgetDetail(0);return goldTip=" (income "+b.Income+"; units "+(b.army+b.civilians)+", buildings "+(b.cityUpkeep+b.buildingUpkeep)+", roads "+b.roads+" for "+world.RoadTiles(0)+" road tiles)";}
 static float Hash01(int x,int y){unchecked{int h=x*374761393+y*668265263;h=(h^(h>>13))*1274126177;return ((h^(h>>16))&1023)/1023f;}}
}
}
namespace ParadigmRealm {
public partial class RealmGame {
 // The minimap is repainted only when something it shows can have changed (owner report, 21 Nov 2026: a 154 ms repaint every
 // three seconds on a large map was a steady stutter): a new turn, a city founded or lost, or one of your units moving (which
 // explores and changes what you see). The Military layer also follows other units, which only move during a turn.
 long miniSig;int miniRepaints;
 long MiniSignature(){unchecked{long h=world.turn*1000003L+world.cities.Count*7919L+viewRealm*131L;foreach(var u in world.units)if(u.owner==0)h=h*31+u.x*977+u.y*13+u.id;return h;}}
}
}
