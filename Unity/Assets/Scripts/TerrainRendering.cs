using System;
using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm { public partial class RealmGame {
 readonly Dictionary<string,Texture2D> terrainComposites=new Dictionary<string,Texture2D>();
 readonly Queue<string> terrainCacheOrder=new Queue<string>();
 readonly Dictionary<int,TerrainInk[]> terrainSourcePixels=new Dictionary<int,TerrainInk[]>();
 int[] terrainSnapshot; World terrainWorld;
 // Snapshot known information once per draw, not for every pixel or neighbour.
 void PrepareTerrainView(){
  // Small maps refresh everything; large maps refresh only the visible window plus a margin for neighbour lookups.
  if(world.tiles.Length<=4000)PrepareTerrainRegion(0,0,world.Width-1,world.Height-1);
  else PrepareTerrainRegion((int)(mapScroll.x/tilePixels)-4,(int)(mapScroll.y/tilePixels)-4,(int)((mapScroll.x+852)/tilePixels)+4,(int)((mapScroll.y+624)/tilePixels)+4);
 }
 void PrepareTerrainRegion(int x0,int y0,int x1,int y1){
  if(terrainWorld!=world){ClearTerrainComposites();terrainWorld=world;}
  if(terrainSnapshot==null||terrainSnapshot.Length!=world.tiles.Length){terrainSnapshot=new int[world.tiles.Length];for(int i=0;i<terrainSnapshot.Length;i++)terrainSnapshot[i]=-1;}
  x0=Math.Max(0,x0);y0=Math.Max(0,y0);x1=Math.Min(world.Width-1,x1);y1=Math.Min(world.Height-1,y1);
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++){var t=world.tiles[y*world.Width+x];terrainSnapshot[y*world.Width+x]=t.seen?world.KnownTile(t).terrain:-1;}
 }
 // Only tiles on screen are drawn, in the same row-by-row order as the full tile list.
 IEnumerable<Tile> TilesInView(){
  int x0=Math.Max(0,(int)(mapScroll.x/tilePixels)-1),y0=Math.Max(0,(int)(mapScroll.y/tilePixels)-1);
  int x1=Math.Min(world.Width-1,(int)((mapScroll.x+852)/tilePixels)+1),y1=Math.Min(world.Height-1,(int)((mapScroll.y+624)/tilePixels)+1);
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)yield return world.tiles[y*world.Width+x];
 }
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
 Texture2D TerrainAppearance(Tile t){
  if(terrainSnapshot==null||terrainWorld!=world)PrepareTerrainView();
  // 5x5 includes the river segments and warped corners influencing this tile.
  var ids=new int[25];int i=0;for(int y=-2;y<=2;y++)for(int x=-2;x<=2;x++)ids[i++]=ViewTerrain(t.x+x,t.y+y);
  string key=world.seed+":"+t.x+":"+t.y+":"+string.Join(",",ids);Texture2D output;if(terrainComposites.TryGetValue(key,out output))return output;
  int size=TerrainPainter.Size;output=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
  var pixels=new Color[size*size];
  for(int y=0;y<size;y++)for(int x=0;x<size;x++){
   var c=TerrainPainter.Pixel(t.x+(x+.5f)/size,t.y+(y+.5f)/size,world.seed,PaintTerrain,SourceTerrain);
   pixels[(size-1-y)*size+x]=new Color(c.r,c.g,c.b,1);
  }
  output.SetPixels(pixels);output.Apply();
  // Enough for the full fixed-size world plus changed edges; no pan-induced churn.
  if(terrainComposites.Count>=2048){string oldest=terrainCacheOrder.Dequeue();Destroy(terrainComposites[oldest]);terrainComposites.Remove(oldest);}
  terrainComposites.Add(key,output);terrainCacheOrder.Enqueue(key);return output;
 }
 void ClearTerrainComposites(){foreach(var t in terrainComposites.Values)Destroy(t);terrainComposites.Clear();terrainCacheOrder.Clear();terrainSourcePixels.Clear();terrainSnapshot=null;}
}}
