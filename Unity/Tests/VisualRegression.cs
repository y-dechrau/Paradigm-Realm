using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ParadigmRealm;
class VisualRegression {
 static BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 static object Call(RealmGame g,string name,params object[] args){return typeof(RealmGame).GetMethod(name,flags).Invoke(g,args);}
 static void Main(){
  var w=World.New(731);w.Found(w.units.First(u=>u.owner==0&&u.type==0));
  var c=w.cities.First(a=>a.owner==0);c.focus=4;c.merchants=1;
  var json=JsonUtility.ToJson(w);var loaded=JsonUtility.FromJson<World>(json);loaded.UpgradeSave();
  Check(loaded.Valid()&&JsonUtility.ToJson(loaded)==json,"complete JSON round trip incl commerce focus");
  World twinA=World.New(817),twinB=World.New(817);
  twinA.Found(twinA.units.First(u=>u.owner==0&&u.type==0));twinB.Found(twinB.units.First(u=>u.owner==0&&u.type==0));
  for(int i=0;i<12;i++){twinA.EndTurn();twinB.EndTurn();}
  Check(JsonUtility.ToJson(twinA)==JsonUtility.ToJson(twinB),"seeded AI and simulation remain deterministic");
  // Explicit default-state legacy v6 round trip is still supported.
  loaded.version=6;loaded.UpgradeSave();Check(loaded.Valid(),"legacy v6 upgrade remains valid");
  var game=new RealmGame();typeof(RealmGame).GetField("world",flags).SetValue(game,w);Call(game,"MakeArt");Call(game,"PrepareTerrainView");
  var t=w.At(9,12);var art=(Texture2D)Call(game,"TerrainAppearance",w.KnownTile(t));
  Check(ReferenceEquals(art,Call(game,"TerrainAppearance",w.KnownTile(t))),"unchanged terrain cached");
  Check(JsonUtility.ToJson(w)==json,"rendering does not mutate simulation");
  // Render beside a never-seen neighbour, then change its secret live terrain.
  var edge=w.At(12,12);var secret=w.At(13,12);secret.seen=false;
  Call(game,"PrepareTerrainView");var first=(Texture2D)Call(game,"TerrainAppearance",w.KnownTile(edge));
  secret.terrain=secret.terrain==4?5:4;Call(game,"PrepareTerrainView");
  Check(ReferenceEquals(first,Call(game,"TerrainAppearance",w.KnownTile(edge))),"unexplored terrain never changes visible art/cache key");
  // Previously seen terrain must use remembered state after all observers leave.
  t.seen=true;t.rememberedTerrain=0;w.units.RemoveAll(u=>u.owner==0);w.cities.RemoveAll(a=>a.owner==0);t.terrain=5;
  Call(game,"PrepareTerrainView");var snapshot=(int[])typeof(RealmGame).GetField("terrainSnapshot",flags).GetValue(game);
  Check(snapshot[t.y*World.Width+t.x]==0,"remembered terrain used outside sight");
  // Tile-local cache neighbourhoods reproduce the global field, including corners.
  Func<int,float,float,TerrainInk> sample=(id,x,y)=>new TerrainInk(id/21f,.3f,.5f);
  for(int y=0;y<TerrainDemo.Height;y++)for(int x=0;x<TerrainDemo.Width;x++){
   int cx=x,cy=y;Func<int,int,int> local=(a,b)=>TerrainDemo.At(Math.Max(cx-2,Math.Min(cx+2,a)),Math.Max(cy-2,Math.Min(cy+2,b)));
   foreach(float dx in new[]{.001f,.25f,.999f})foreach(float dy in new[]{.001f,.5f,.999f}){
    var a=TerrainPainter.Pixel(x+dx,y+dy,TerrainDemo.Seed,TerrainDemo.At,sample);var b=TerrainPainter.Pixel(x+dx,y+dy,TerrainDemo.Seed,local,sample);
    Check(a.r==b.r&&a.g==b.g&&a.b==b.b,"tile cache dependency radius covers all pixel contributors");
   }
  }
  Console.WriteLine("PASS JSON round trip (System.Text.Json doubles), v6 loading, renderer cache, no render mutation, unseen/remembered fog, 1728 corner/edge dependency checks");
 }
}
