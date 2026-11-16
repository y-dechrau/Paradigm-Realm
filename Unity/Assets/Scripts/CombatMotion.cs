using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm {
// Soldiers fight and take guard on screen (owner request, 14 Nov 2026). Clips made in SpriteCook from each unit's picture, in
// Resources/Visuals/Combat: unit-NN-attack-00..07 (the Warrior swings his axe, the Spearman thrusts, the Archer looses an arrow,
// Horseman and Knight drive a lance) and unit-NN-guard-00..07 (into a defensive posture; the last frame is then held while the
// unit stays fortified). An attack is played when World.strikeLog records one the player can see: the figure lunges toward the
// tile it struck, facing it, and an archer's arrow flies across. Nothing about the rules changes. A type without clips, and
// peoples with their own artwork, look as before.
public partial class RealmGame {
 sealed class Strike{public Vector2Int from,target;public float t;}
 readonly Dictionary<int,Strike> strikes=new Dictionary<int,Strike>();
 readonly Dictionary<int,float> guardClips=new Dictionary<int,float>();
 readonly Dictionary<int,bool> wasFortified=new Dictionary<int,bool>();
 readonly Dictionary<string,Texture2D[]> combatFrames=new Dictionary<string,Texture2D[]>();
 World strikeWorld;
 const float StrikeSeconds=.85f,GuardSeconds=.8f;
 Texture2D[] CombatFrames(int type,string clip){
  string key=type.ToString("00")+"-"+clip;Texture2D[] set;if(combatFrames.TryGetValue(key,out set))return set;
  var raw=new List<Texture2D>();for(int k=0;k<16;k++){var f=Resources.Load<Texture2D>("Visuals/Combat/unit-"+key+"-"+k.ToString("00"));if(f==null)break;raw.Add(f);}
  set=MatchStanding(type,raw);combatFrames[key]=set;return set;
 }
 // Clip frames drawn the way the standing picture is (owner report, 16 Nov 2026: the guard looked larger and softer than the
 // unit at rest). The clips come at another size, and their figure fills more of the frame, so each frame is first rescaled
 // until the figure in frame 0 stands exactly as tall, as wide-centred and on the same foot row as the figure in the unit's
 // own picture; then it goes through the same finishing as that picture (UnitSprite: contrast, rim light, outline; SoftTexture).
 Texture2D[] MatchStanding(int type,List<Texture2D> raw){
  var made=new Texture2D[raw.Count];if(raw.Count==0)return made;
  Texture2D still=people!=null&&type>=0&&type<people.Length?people[type]:null;
  try{
   int S=still!=null?still.width:128;int sx0,sy0,sx1,sy1;
   if(still==null||!Box(ReadArtPixels(still),still.width,still.height,out sx0,out sy0,out sx1,out sy1)){sx0=0;sy0=0;sx1=S-1;sy1=S-1;}
   int W0=raw[0].width,H0=raw[0].height;var first=ReadArtPixels(raw[0]);int fx0,fy0,fx1,fy1;if(!Box(first,W0,H0,out fx0,out fy0,out fx1,out fy1)){fx0=0;fy0=0;fx1=W0-1;fy1=H0-1;}
   float k=(sy1-sy0+1)/(float)(fy1-fy0+1);float cxS=(sx0+sx1)/2f,cxF=(fx0+fx1)/2f;
   for(int f=0;f<raw.Count;f++){
    int W=raw[f].width,H=raw[f].height;var src=f==0?first:ReadArtPixels(raw[f]);var canvas=new uint[S*S];
    // Each canvas pixel samples the frame where it sits once frame 0's figure is laid over the standing figure.
    for(int y=0;y<S;y++)for(int x=0;x<S;x++){float u=cxF+(x+.5f-cxS)/k,v=fy1+(y+.5f-sy1-1)/k;int ix=(int)Mathf.Floor(u),iy=(int)Mathf.Floor(v);if(ix<0||iy<0||ix>=W||iy>=H)continue;canvas[y*S+x]=src[iy*W+ix];}
    made[f]=SoftTexture(PixelArt.UnitSprite(canvas,S,S,128));themeTextures.Add(made[f]);
   }
  }catch(System.Exception){for(int f=0;f<raw.Count;f++){raw[f].filterMode=FilterMode.Bilinear;made[f]=raw[f];}}
  return made;
 }
 // The box round a picture's solid pixels (alpha over 100). False when there are none.
 static bool Box(uint[] px,int w,int h,out int x0,out int y0,out int x1,out int y1){
  x0=w;y0=h;x1=-1;y1=-1;for(int y=0;y<h;y++)for(int x=0;x<w;x++){if((px[y*w+x]>>24)<=100)continue;if(x<x0)x0=x;if(x>x1)x1=x;if(y<y0)y0=y;if(y>y1)y1=y;}
  return x1>=0;}
 bool GuardStance(Unit u){return u.fortified&&!u.hasOrder&&u.job<=0&&World.Power[u.type]>0&&!World.IsNaval(u.type);}
 // Called with the glides once per repaint: new blows start clips, fortifying starts the guard clip, clips move on.
 void UpdateStrikes(float dt){
  if(world==null)return;
  if(strikeWorld!=world){strikes.Clear();guardClips.Clear();wasFortified.Clear();strikeWorld=world;if(world.strikeLog!=null)world.strikeLog.Clear();foreach(var u in world.units)wasFortified[u.id]=GuardStance(u);return;}
  if(world.strikeLog!=null&&world.strikeLog.Count>0){
   foreach(var s in world.strikeLog){var u=world.units.Find(a=>a.id==s[0]);if(u==null)continue;
    if(!(u.owner==0||world.Visible(u.x,u.y)||world.Visible(s[1],s[2])))continue;
    strikes[u.id]=new Strike{from=new Vector2Int(u.x,u.y),target=new Vector2Int(s[1],s[2])};guardClips.Remove(u.id);}
   world.strikeLog.Clear();}
  foreach(var u in world.units){bool now=GuardStance(u);bool before;
   if(wasFortified.TryGetValue(u.id,out before)&&now&&!before&&(u.owner==0||world.Visible(u.x,u.y)))guardClips[u.id]=0;
   wasFortified[u.id]=now;}
  List<int> done=null;
  foreach(var kv in strikes){kv.Value.t+=dt/StrikeSeconds;if(kv.Value.t>=1)(done=done??new List<int>()).Add(kv.Key);}
  if(done!=null)foreach(int k in done)strikes.Remove(k);
  done=null;var keys=new List<int>(guardClips.Keys);
  foreach(int k in keys){guardClips[k]+=dt/GuardSeconds;if(guardClips[k]>=1)(done=done??new List<int>()).Add(k);}
  if(done!=null)foreach(int k in done)guardClips.Remove(k);
 }
 // The attack frame for a unit now striking, or null.
 Texture2D StrikePose(Unit u,out Strike strike){
  strike=null;if(u==null||!strikes.TryGetValue(u.id,out strike)||SpeciesSprite(u)!=null)return null;
  var set=CombatFrames(u.type,"attack");if(set.Length==0)return null;return set[Mathf.Min(set.Length-1,(int)(strike.t*set.Length))];
 }
 // While fortified: the guard clip as it plays, then its last frame held.
 Texture2D GuardPose(Unit u){
  if(u==null||!GuardStance(u)||SpeciesSprite(u)!=null)return null;var set=CombatFrames(u.type,"guard");if(set.Length==0)return null;
  float t;return guardClips.TryGetValue(u.id,out t)?set[Mathf.Min(set.Length-1,(int)(t*set.Length))]:set[set.Length-1];
 }
 // Where on screen a tile is, for the lunge and the arrow (the diagonal and staggered views; the classic view does not lunge).
 bool StrikeVector(Strike s,out Vector2 dir,out float length){
  dir=Vector2.right;length=0;if(!diamondView)return false;
  var a=TileScreen(s.from.x,s.from.y,isoWidth);var b=TileScreen(s.target.x,s.target.y,isoWidth);var d=b-a;length=d.magnitude;if(length<1||length>isoWidth*4)return false;dir=d/length;return true;
 }
 // An arrow in flight from the archer to its target, in the middle of the clip.
 void DrawArrowFlight(Strike s,Vector2 feet,float size){
  Vector2 dir;float length;if(!StrikeVector(s,out dir,out length))return;float k=Mathf.InverseLerp(.42f,.92f,s.t);if(k<=0||k>=1)return;
  var start=feet+new Vector2(dir.x*size*.2f,-size*.55f);var end=start+dir*length;var p=Vector2.Lerp(start,end,k);p.y-=Mathf.Sin(k*Mathf.PI)*size*.25f;
  var tangent=(end-start).normalized;tangent.y-=Mathf.Cos(k*Mathf.PI)*.5f;tangent.Normalize();float shaft=size*.22f,w=Mathf.Max(1.5f,size*.025f);
  for(float d=0;d<shaft;d+=w){var q=p-tangent*d;Fill(new Rect(q.x-w/2,q.y-w/2,w,w),d<w*2?new Color(.75f,.78f,.82f):d>shaft-w*3?new Color(.92f,.92f,.88f):new Color(.45f,.3f,.16f));}
 }
}
}
