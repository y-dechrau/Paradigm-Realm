using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm {
// Barbarian raiders walk (owner request, 27 Nov 2026: "animate movement for barbarians"). A raider that changed tile since the last
// frame glides there as units and animals do (MotionUI's Glide, hop and pace), stepping through a six-frame walk made in SpriteCook
// from the raider's own picture (Visuals/Movement/raider-move-00..05, matched to the standing raider like every clip), and faces
// the way it last walked. Drawing only; the rules are unchanged. With frames missing it slides its standing picture.
public partial class RealmGame {
 readonly Dictionary<int,Glide> raiderGlides=new Dictionary<int,Glide>();
 readonly Dictionary<int,Vector2Int> raiderLast=new Dictionary<int,Vector2Int>();
 readonly HashSet<int> raiderFacingLeft=new HashSet<int>();
 Texture2D[] raiderWalk;bool raiderWalkTried;
 bool RaiderGliding(Raider r){return r!=null&&raiderGlides.ContainsKey(r.id);}
 Texture2D RaiderWalkFrame(float walk){
  if(raiderWalk!=null)foreach(var t in raiderWalk)if(t==null){raiderWalk=null;raiderWalkTried=false;break;}
  if(raiderWalk==null){if(raiderWalkTried)return null;raiderWalkTried=true;
   var raw=new List<Texture2D>();for(int f=0;f<WalkFrameCount;f++){var t=Resources.Load<Texture2D>("Visuals/Movement/raider-move-"+f.ToString("00"));if(t==null)return null;raw.Add(t);}
   var set=MatchStanding(2,raw);foreach(var t in set)if(t==null)return null;raiderWalk=set;}
  return raiderWalk[(int)(walk*WalkFramesPerTile)%raiderWalk.Length];
 }
 // Called from UpdateGlides: a change of tile since the last frame becomes a glide (two steps for a horse raider show as one).
 void UpdateRaiderGlides(float dt){
  if(world.raiders==null||!world.barbarians){raiderGlides.Clear();return;}
  foreach(var r in world.raiders){if(r.hp<=0)continue;var now=new Vector2Int(r.x,r.y);Vector2Int was;
   if(raiderLast.TryGetValue(r.id,out was)&&was!=now&&(world.Visible(r.x,r.y)||world.Visible(was.x,was.y))&&world.Distance(was.x,was.y,r.x,r.y)<=4){
    var tw=world.At(was.x,was.y);if(tw!=null&&tw.realm==viewRealm){var g=new Glide();g.path.Add(was);g.path.Add(now);raiderGlides[r.id]=g;}}
   raiderLast[r.id]=now;}
  Advance(raiderGlides,dt);
 }
 void DrawRaiderGlides(float tw,float s){
  if(raiderGlides.Count==0||world.raiders==null)return;
  foreach(var r in world.raiders){Glide g;if(r.hp<=0||!raiderGlides.TryGetValue(r.id,out g))continue;
   if(g.path.Count>=2){float dx=TileScreen(g.path[1].x,g.path[1].y,tw).x-TileScreen(g.path[0].x,g.path[0].y,tw).x;if(dx<-1&&Mathf.Abs(dx)<tw*3)raiderFacingLeft.Add(r.id);else if(dx>1&&Mathf.Abs(dx)<tw*3)raiderFacingLeft.Remove(r.id);}
   Vector2 at;if(!GlidePoint(g,tw,s,out at))continue;
   DrawRaiderFigure(r,new Vector2(at.x,at.y+4*s),s,RaiderWalkFrame(g.walk)??RaiderPicture());}
 }
 // Drawn as DrawUnitArt draws every unit: 1.6 times its 40-unit box, feet on the box's floor, mirrored when facing left.
 void DrawRaiderFigure(Raider r,Vector2 feet,float s,Texture2D pic){
  float size=40*s*1.6f;var at=new Rect(feet.x-size/2,feet.y-size*62f/64f,size,size);
  if(pic==null){Fill(new Rect(feet.x-5*s,feet.y-24*s,10*s,18*s),new Color(.55f,.2f,.12f));return;}
  if(raiderFacingLeft.Contains(r.id))GUI.DrawTextureWithTexCoords(at,pic,new Rect(1,0,-1,1));else GUI.DrawTexture(at,pic,ScaleMode.ScaleToFit,true);
 }
}
}
