using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm {
// Barbarian camps and raiders on the map (owner request, 16 Nov 2026). The raider is SpriteCook art in the warrior's style
// (Visuals/barbarian-raider), matched to the standing warrior's height and footing and finished like every unit picture;
// the camp (Visuals/barbarian-camp) stands on its tile like a tribal village. A camp once seen stays on the map; raiders are
// drawn only where you can see them, with a red-edged health bar when hurt.
public partial class RealmGame {
 Texture2D raiderPicture;bool raiderTried;
 Texture2D RaiderPicture(){
  if(raiderPicture!=null)return raiderPicture;if(raiderTried&&ReferenceEquals(raiderPicture,null))return null;raiderTried=true;
  var raw=Art("barbarian-raider");if(raw==null)return null;
  var made=MatchStanding(2,new List<Texture2D>{raw});raiderPicture=made.Length>0?made[0]:null;return raiderPicture;
 }
 void DrawBarbarianArt(int x,int y,float sx,float gy,float s,bool visible){
  var camp=world.CampAt(x,y);
  if(camp!=null&&(visible||camp.seen)){var tex=Art("barbarian-camp");var r=new Rect(sx-30*s,gy-40*s,60*s,52*s);if(tex!=null)GUI.DrawTexture(r,tex,ScaleMode.ScaleToFit,true);else Fill(new Rect(sx-8*s,gy-12*s,16*s,12*s),new Color(.5f,.15f,.1f));
   if(visible&&camp.hp<10)barbarianBars.Add(new Vector4(sx,gy-42*s,s,camp.hp));}
  if(!visible)return;var raider=world.RaiderAt(x,y);if(raider==null||RaiderGliding(raider))return;
  // Drawn as DrawUnitArt draws every unit: 1.6 times its 40-unit box, feet on the box's floor (owner report, 21 Nov 2026: too small).
  DrawRaiderFigure(raider,new Vector2(sx,gy+4*s),s,RaiderPicture());

  if(raider.hp<10)barbarianBars.Add(new Vector4(sx,gy-47*s,s,raider.hp));
 }
 // Health bars go on after the tiles, so taller ground drawn later never hides them.
 readonly List<Vector4> barbarianBars=new List<Vector4>();
 void DrawBarbarianBars(){foreach(var b in barbarianBars)HealthBar(b.x,b.y,b.z,(int)b.w);}
 void HealthBar(float sx,float by,float s,int hp){float bh=Mathf.Max(3,2.8f*s),bw=30*s;
  Fill(new Rect(sx-bw/2-1,by-1,bw+2,bh+2),new Color(.45f,.08f,.06f,.9f));
  Fill(new Rect(sx-bw/2,by,bw*Mathf.Clamp01(hp/10f),bh),hp>5?new Color(.37f,.8f,.47f):hp>2?new Color(.95f,.75f,.3f):new Color(.9f,.3f,.25f));}
}
}
