using System;
using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm { public partial class RealmGame {
 readonly Dictionary<long,Texture2D> terrainComposites=new Dictionary<long,Texture2D>();
 readonly Queue<long> terrainCacheOrder=new Queue<long>();
 readonly Dictionary<int,Texture2D> uniformTerrain=new Dictionary<int,Texture2D>();
 readonly int[] compositeIds=new int[25];
 readonly System.Diagnostics.Stopwatch compositeClock=new System.Diagnostics.Stopwatch();
 bool compositeBudgetOn; const int CompositeBudgetMs=6;
 // Reused every frame so drawing the map allocates almost nothing.
 readonly List<Tile> viewTiles=new List<Tile>(),knownPool=new List<Tile>();
 readonly List<bool> viewVisible=new List<bool>();
 readonly Dictionary<int,List<Unit>> viewStacks=new Dictionary<int,List<Unit>>();
 readonly Dictionary<int,City> viewCities=new Dictionary<int,City>();
 readonly Dictionary<int,TerrainInk[]> terrainSourcePixels=new Dictionary<int,TerrainInk[]>();
 int[] terrainSnapshot; World terrainWorld;
 // Snapshot known information once per draw, not for every pixel or neighbour.
 void PrepareTerrainView(){
  // Small maps refresh everything; large maps refresh only the visible window plus a margin for neighbour lookups.
  if(world.tiles.Length<=4000)PrepareTerrainRegion(0,0,world.Width-1,world.Height-1);
  else PrepareTerrainRegion((int)(mapScroll.x/tilePixels)-4,(int)(mapScroll.y/ClassicRowHeight())-4,(int)((mapScroll.x+MapW)/tilePixels)+4,(int)((mapScroll.y+MapH)/ClassicRowHeight())+4);
 }
 void PrepareTerrainRegion(int x0,int y0,int x1,int y1){
  if(terrainWorld!=world){ClearTerrainComposites();terrainWorld=world;}
  if(terrainSnapshot==null||terrainSnapshot.Length!=world.tiles.Length){terrainSnapshot=new int[world.tiles.Length];for(int i=0;i<terrainSnapshot.Length;i++)terrainSnapshot[i]=-1;}
  x0=Math.Max(0,x0);y0=Math.Max(0,y0);x1=Math.Min(world.Width-1,x1);y1=Math.Min(world.Height-1,y1);
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++){var t=world.tiles[y*world.Width+x];terrainSnapshot[y*world.Width+x]=t.seen?world.KnownTerrain(t):-1;}
 }
 // Only tiles on screen are drawn, in the same row-by-row order as the full tile list.
 IEnumerable<Tile> TilesInView(){
  int x0=Math.Max(0,(int)(mapScroll.x/tilePixels)-1),y0=Math.Max(0,(int)(mapScroll.y/ClassicRowHeight())-1);
  int x1=Math.Min(world.Width-1,(int)((mapScroll.x+MapW)/tilePixels)+1),y1=Math.Min(world.Height-1,(int)((mapScroll.y+MapH)/ClassicRowHeight())+1);
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)yield return world.tiles[y*world.Width+x];
 }
 int StaggeredViewTerrain(Tile t,int dx,int dy){int y=t.y+dy;int x=(int)Math.Floor(t.x+(t.y&1)*.5+.5+dx-(y&1)*.5);return ViewTerrain(x,y);}
 int ViewTerrain(int x,int y){x=Math.Max(0,Math.Min(world.Width-1,x));y=Math.Max(0,Math.Min(world.Height-1,y));return terrainSnapshot[y*world.Width+x];}
 // Unknown neighbours are neutral grass; their hidden live type never affects art.
 int PaintTerrain(int x,int y){int t=ViewTerrain(x,y);return t<0?0:t;}
 TerrainInk SourceTerrain(int id,float u,float v){
  if(id==6)id=4;
  TerrainInk[] pixels;
  if(!terrainSourcePixels.TryGetValue(id,out pixels)){
   var t=terrain[id];var original=t.GetPixels();int sourceId=id;
   pixels=TerrainPalette.Build((x,y)=>{
    if(sourceId==18)y=.12f+y*.3f;
    var c=original[Math.Min(t.height-1,(int)((1-y)*t.height))*t.width+Math.Min(t.width-1,(int)(x*t.width))];return new TerrainInk(c.r,c.g,c.b);
   });terrainSourcePixels[id]=pixels;
  }
  return TerrainPalette.Sample(pixels,u,v);
 }
 // Flat ground with the same type all around looks the same on every tile, so one shared texture per type is enough.
 Texture2D UniformTexture(int ground){
  Texture2D shared;if(uniformTerrain.TryGetValue(ground,out shared))return shared;
  int size=TerrainPainter.Size;shared=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
  var pixels=new Color[size*size];Func<int,float,float,TerrainInk> source=SourceTerrain;
  for(int y=0;y<size;y++)for(int x=0;x<size;x++){var c=TerrainPainter.PixelUniform((x+.5f)/size,(y+.5f)/size,0,ground,source);pixels[(size-1-y)*size+x]=new Color(c.r,c.g,c.b,1);}
  shared.SetPixels(pixels);shared.Apply();uniformTerrain[ground]=shared;return shared;
 }
 // While the map is being drawn only a few milliseconds a frame go on painting new tiles; the rest show the plain
 // supplied texture for a moment and are upgraded on later frames.
 // Remembers what each tile last showed. A tile that has to be repainted (for example because its neighbours were just
 // explored) keeps showing its old picture until the new one is ready, so nothing flickers back to a placeholder.
 readonly Dictionary<int,Texture2D> lastShown=new Dictionary<int,Texture2D>();
 Texture2D Shown(int index,Texture2D picture){Texture2D current;if(!lastShown.TryGetValue(index,out current)||current!=picture)lastShown[index]=picture;return picture;}
 Texture2D WaitingPicture(int index,int ground){Texture2D previous;if(lastShown.TryGetValue(index,out previous)&&previous!=null)return previous;return UniformTexture(ground);}
 Texture2D TerrainAppearance(Tile t){
  if(terrainSnapshot==null||terrainWorld!=world)PrepareTerrainView();
  // 5x5 includes the river segments and warped corners influencing this tile.
  // Staggered maps: the neighbour on screen is the tile under the point dx tiles across and dy rows down.
  var ids=compositeIds;int i=0;for(int y=-2;y<=2;y++)for(int x=-2;x<=2;x++)ids[i++]=world.Staggered?StaggeredViewTerrain(t,x,y):ViewTerrain(t.x+x,t.y+y);
  int ground0=-1;bool uniform=true,river=false;
  for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int id=ids[(dy+2)*5+dx+2];if(id<0)id=0;if(id==7)river=true;int g=TerrainPainter.GroundOf(id);if(ground0<0)ground0=g;else if(g!=ground0)uniform=false;}
  uniform=uniform&&!river;
  int tileIndex=t.y*world.Width+t.x;
  if(uniform&&!TerrainPainter.Raised(ground0))return Shown(tileIndex,UniformTexture(ground0));
  ulong hash=14695981039346656037UL;
  unchecked{
   for(int k=0;k<25;k++)hash=(hash^(ulong)(ids[k]+1))*1099511628211UL;
   hash=(hash^(ulong)(uint)t.x)*1099511628211UL;hash=(hash^(ulong)(uint)t.y)*1099511628211UL;hash=(hash^(ulong)(uint)world.seed)*1099511628211UL;
  }
  long key=(long)hash;Texture2D output;if(terrainComposites.TryGetValue(key,out output))return Shown(tileIndex,output);
  if(compositeBudgetOn&&compositeClock.ElapsedMilliseconds>=CompositeBudgetMs){int centre=ids[12];if(centre<0)centre=0;return WaitingPicture(tileIndex,TerrainPainter.GroundOf(centre));}
  int size=TerrainPainter.Size;output=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
  var pixels=new Color[size*size];Func<int,int,int> paint=PaintTerrain;Func<int,float,float,TerrainInk> source=SourceTerrain;
  for(int y=0;y<size;y++)for(int x=0;x<size;x++){
   float wx=t.x+(x+.5f)/size,wy=t.y+(y+.5f)/size;
   var c=uniform?TerrainPainter.PixelUniform(wx,wy,world.seed,ground0,source):TerrainPainter.Pixel(wx,wy,world.seed,paint,source,river);
   pixels[(size-1-y)*size+x]=new Color(c.r,c.g,c.b,1);
  }
  output.SetPixels(pixels);output.Apply();
  // Enough for the full fixed-size world plus changed edges; no pan-induced churn.
  if(terrainComposites.Count>=2048){long oldest=terrainCacheOrder.Dequeue();Destroy(terrainComposites[oldest]);terrainComposites.Remove(oldest);}
  terrainComposites.Add(key,output);terrainCacheOrder.Enqueue(key);return Shown(tileIndex,output);
 }
 void ClearTerrainComposites(){ClearIso();foreach(var t in terrainComposites.Values)Destroy(t);foreach(var t in uniformTerrain.Values)Destroy(t);terrainComposites.Clear();terrainCacheOrder.Clear();uniformTerrain.Clear();lastShown.Clear();terrainSourcePixels.Clear();terrainSnapshot=null;}
}}
