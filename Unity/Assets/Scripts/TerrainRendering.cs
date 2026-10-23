using System;
using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm { public partial class RealmGame {
 readonly Dictionary<string,Texture2D> terrainComposites=new Dictionary<string,Texture2D>();
 readonly Queue<string> terrainCacheOrder=new Queue<string>();
 static Color MixTerrain(Color a,Color b,float t){return new Color(a.r+(b.r-a.r)*t,a.g+(b.g-a.g)*t,a.b+(b.b-a.b)*t,1);}
 static bool WaterArt(int t){return t==4||t==6||t==12;}
 static float Relief(int t,float u,float v){float a=t==5?.85f:t==2?.5f:t==15||t==17?.6f:t==1||t==11||t==13?.22f:t==3?.13f:.035f;double x=Math.Sin(Math.PI*u),y=Math.Sin(Math.PI*v);return a*(float)(x*x*y*y);}
 int KnownTerrain(int x,int y,int fallback){var t=world.At(x,y);return t==null||!t.seen?fallback:world.KnownTile(t).terrain;}
 Color TerrainPixel(int id,float u,float v){var t=terrain[id];return t.GetPixel(Math.Max(0,Math.Min(t.width-1,(int)(u*t.width))),Math.Max(0,Math.Min(t.height-1,(int)((1-v)*t.height))));}
 static float Quantise(float v){return (float)Math.Round(Math.Max(0,Math.Min(1,v))*31)/31;}
 Texture2D TerrainAppearance(Tile t){int[] ids=new int[9];int i=0;for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)ids[i++]=KnownTerrain(t.x+x,t.y+y,t.terrain);string key=string.Join(",",ids);Texture2D output;if(terrainComposites.TryGetValue(key,out output))return output;
 output=new Texture2D(64,64,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
 for(int y=0;y<64;y++)for(int x=0;x<64;x++){float u=(x+.5f)/64,v=(y+.5f)/64;int dx=u<.5f?-1:1,dy=v<.5f?-1:1,h=ids[4+dx],vertical=ids[4+dy*3],d=ids[4+dx+dy*3];float bx=WaterArt(t.terrain)!=WaterArt(h)?.1f:.22f,by=WaterArt(t.terrain)!=WaterArt(vertical)?.1f:.22f;float wx=Math.Max(0,(bx-Math.Min(u,1-u))/(2*bx)),wy=Math.Max(0,(by-Math.Min(v,1-v))/(2*by));Color colour=MixTerrain(MixTerrain(TerrainPixel(t.terrain,u,v),TerrainPixel(h,1-u,v),wx),MixTerrain(TerrainPixel(vertical,u,1-v),TerrainPixel(d,1-u,1-v),wx),wy);float slope=(Relief(t.terrain,u+.015f,v)-Relief(t.terrain,u-.015f,v)+Relief(t.terrain,u,v+.015f)-Relief(t.terrain,u,v-.015f))/.03f;float shade=1+(float)Math.Round(Math.Max(-.3f,Math.Min(.25f,slope*.16f))*16)/16;output.SetPixel(x,63-y,new Color(Quantise(colour.r*shade),Quantise(colour.g*shade),Quantise(colour.b*shade),1));}
 output.Apply();if(terrainComposites.Count>=2048){string oldest=terrainCacheOrder.Dequeue();Destroy(terrainComposites[oldest]);terrainComposites.Remove(oldest);}terrainComposites.Add(key,output);terrainCacheOrder.Enqueue(key);return output;
 }
 void ClearTerrainComposites(){foreach(var t in terrainComposites.Values)Destroy(t);terrainComposites.Clear();terrainCacheOrder.Clear();}
}}
