using System;
using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm {
// Animated movement (owner request, 9 Nov 2026). Units glide from tile to tile along the steps they really took (World.moveLog),
// with a small hop for each step; animals glide to where they moved. Only what you can see is animated, long walks play faster,
// and nothing about the rules changes: the unit is already on its new tile, it is only drawn on the way there.
public partial class RealmGame {
 sealed class Glide{public readonly List<Vector2Int> path=new List<Vector2Int>();public float t;}
 readonly Dictionary<int,Glide> unitGlides=new Dictionary<int,Glide>(),animalGlides=new Dictionary<int,Glide>();
 readonly Dictionary<int,Vector2Int> animalLast=new Dictionary<int,Vector2Int>();
 readonly Dictionary<int,Unit> glideUnits=new Dictionary<int,Unit>();
 float glideClock=-1;World glideWorld;
 const float StepsPerSecond=6f;
 bool Gliding(int unitId){return unitGlides.ContainsKey(unitId);}
 bool AnimalGliding(int animalId){return animalGlides.ContainsKey(animalId);}
 // Called once per repaint before the map is drawn: takes in new steps and moves every glide on.
 void UpdateGlides(){
  if(world==null)return;
  if(glideWorld!=world){unitGlides.Clear();animalGlides.Clear();animalLast.Clear();glideWorld=world;if(world.moveLog!=null)world.moveLog.Clear();}
  float now=Time.realtimeSinceStartup,dt=glideClock<0?0:Mathf.Min(.1f,now-glideClock);glideClock=now;
  if(world.moveLog!=null&&world.moveLog.Count>0){
   foreach(var m in world.moveLog){
    int id=m[0];var from=new Vector2Int(m[1],m[2]);var to=new Vector2Int(m[3],m[4]);
    bool seen=world.Visible(from.x,from.y)||world.Visible(to.x,to.y);var u=world.units.Find(a=>a.id==id);if(u==null||!(u.owner==0||seen))continue;
    var tf=world.At(from.x,from.y);if(tf==null||tf.realm!=viewRealm)continue;
    Glide g;if(!unitGlides.TryGetValue(id,out g)){g=new Glide();unitGlides[id]=g;}
    if(g.path.Count==0||g.path[g.path.Count-1]!=from){g.path.Clear();g.t=0;g.path.Add(from);}
    g.path.Add(to);if(g.path.Count>12){g.path.RemoveRange(0,g.path.Count-12);g.t=0;}
   }
   world.moveLog.Clear();
  }
  // Animals: any change of tile since the last frame becomes a straight glide.
  if(world.animals!=null){
   foreach(var a in world.animals){var now2=new Vector2Int(a.x,a.y);Vector2Int was;
    if(animalLast.TryGetValue(a.id,out was)&&was!=now2&&(world.Visible(a.x,a.y)||world.Visible(was.x,was.y))&&world.Distance(was.x,was.y,a.x,a.y)<=4){var g=new Glide();g.path.Add(was);g.path.Add(now2);animalGlides[a.id]=g;}
    animalLast[a.id]=now2;}
  }
  Advance(unitGlides,dt);Advance(animalGlides,dt);
 }
 static void Advance(Dictionary<int,Glide> glides,float dt){
  List<int> done=null;
  foreach(var kv in glides){var g=kv.Value;int steps=g.path.Count-1;if(steps<=0){(done=done??new List<int>()).Add(kv.Key);continue;}
   g.t+=dt*StepsPerSecond*Mathf.Max(1f,steps/3f);
   while(g.t>=1f&&g.path.Count>1){g.t-=1f;g.path.RemoveAt(0);}
   if(g.path.Count<=1)(done=done??new List<int>()).Add(kv.Key);}
  if(done!=null)foreach(int k in done)glides.Remove(k);
 }
 // Screen position of a tile's centre on the diagonal or staggered map, using the copy nearest the camera on a wrapping world.
 Vector2 TileScreen(int x,int y,float tw){
  float th=tw*PixelArt.DH/PixelArt.DW;float i=x;int W=world.Width;
  if(world.Sphere){while(i-camI>W/2f)i-=W;while(camI-i>W/2f)i+=W;}
  if(world.Rows){float rowH=RowPixels(tw);return new Vector2(IsoCentreX+(i+(y&1)*.5f-camI)*tw,IsoCentreY+(y-camJ)*rowH);}
  return new Vector2(IsoCentreX+((i-camI)-(y-camJ))*tw/2,IsoCentreY+((i-camI)+(y-camJ))*th/2);
 }
 bool GlidePoint(Glide g,float tw,float s,out Vector2 at){
  at=Vector2.zero;if(g.path.Count<2)return false;var a=TileScreen(g.path[0].x,g.path[0].y,tw);var b=TileScreen(g.path[1].x,g.path[1].y,tw);
  if((a-b).sqrMagnitude>tw*tw*9){g.t=1;return false;}// across the wrap seam: jump rather than sweep the whole map
  float t=Mathf.SmoothStep(0,1,g.t);at=Vector2.Lerp(a,b,t);at.y-=Mathf.Sin(g.t*Mathf.PI)*3.5f*s;return true;
 }
 // Drawn after the tiles, so a moving figure is never hidden by the tile it is entering.
 void DrawGlides(float tw,float s){
  if(unitGlides.Count==0&&animalGlides.Count==0)return;
  glideUnits.Clear();foreach(var u in world.units)if(unitGlides.ContainsKey(u.id))glideUnits[u.id]=u;
  foreach(var kv in unitGlides){Unit u;if(!glideUnits.TryGetValue(kv.Key,out u))continue;Vector2 at;if(!GlidePoint(kv.Value,tw,s,out at))continue;
   DrawUnitArt(u,new Rect(at.x-20*s,at.y-36*s,40*s,40*s),1);}
  if(world.animals!=null)foreach(var a in world.animals){Glide g;if(!animalGlides.TryGetValue(a.id,out g))continue;Vector2 at;if(!GlidePoint(g,tw,s,out at))continue;
   bool painted=SpriteSlot("animal-"+a.kind)!=null;float size=painted?(AnimalCatalog.Carnivore[a.kind]?40:48):64;
   GUI.DrawTexture(painted?new Rect(at.x-size/2*s,at.y-(size-6)*s,size*s,size*s):new Rect(at.x-32*s,at.y-42*s,64*s,64*s),IsoPicture(12,a.kind,0),ScaleMode.ScaleToFit,true);}
 }
}
}
