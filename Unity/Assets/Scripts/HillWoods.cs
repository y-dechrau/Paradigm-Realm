using System;
using UnityEngine;
namespace ParadigmRealm {
// Wooded hills (owner request, 21 Nov 2026): hills can carry forest or jungle (Tile.woods 1 forest, 2 jungle), drawn as a few
// smaller, sparser trees over the hill. Wooded hills give +1 shield; a Worker's Clear job removes the trees (Tile.cleared, as for
// groves), and a farm or mine needs them cleared first. New worlds wood the hills that border forest or jungle; older saves have
// none (woods loads as 0).
public partial class World {
 public const int HillWoodsShields=1;
 public static bool HillWoods(Tile t){return t!=null&&t.terrain==2&&t.woods>0&&!t.cleared;}
 internal void PlaceHillWoods(){
  var r=new System.Random(seed*61+17);
  foreach(var t in tiles){
   if(t.terrain!=2)continue;int forest=0,jungle=0;
   for(int k=0;k<6;k++){var n=Step(t.x,t.y,HexOr(k,true),HexOr(k,false));if(n==null)continue;if(n.terrain==1||n.terrain==13)forest++;else if(n.terrain==11||n.terrain==19)jungle++;}
   int near=forest+jungle;if(near>=2||near==1&&r.Next(100)<35)t.woods=jungle>forest?2:1;
  }
 }
}
public partial class RealmGame {
 // Three small stands of trees on the hill's lower slopes: the forest or jungle picture at about a third of its size.
 void DrawHillWoods(Tile source,float cx,float gy,float s){
  if(!World.HillWoods(source))return;int kind=source.woods==2?11:1;int v=(source.x*7919+source.y*104729)&0x7fffffff;
  float[] ox={-15,14,-2},oy={3,5,9};float k=.34f;
  for(int i=0;i<3;i++){if(i==2&&v%3==0)continue;var pic=IsoPicture(1,kind,(v+i)&3);if(pic==null)continue;float x=cx+ox[i]*s,y=gy+oy[i]*s;
   GUI.DrawTexture(new Rect(x-PixelArt.AX*s*k,y-PixelArt.AY*s*k,64*s*k,64*s*k),pic);}
 }
}
}
