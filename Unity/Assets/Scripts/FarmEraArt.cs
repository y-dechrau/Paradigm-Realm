using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm {
// Farms through the ages (owner's art pack, 26 Nov 2026: "Paradigm-Realm-Farm-Art", six sheets of six farms, originals in
// Design/Farm-Art-2026-11-26). Each sheet was cut into its six farms (the blurred backdrop removed) as Sprites/farm-<age>-<n>:
// age 0 Dawn (the Ancient sheet), 1 Classical, 2 Medieval (the Medieval and Renaissance sheets, twelve designs), 3 Industrial,
// 4 Modern, which the Dimensional Age also uses. A farm is drawn in its owner's age, so a people's farms change as it advances;
// unowned farms use your age. Picture only: the farm's yield, cost and rules are unchanged. Farms on river tiles keep the
// patchwork cut round the river (RiverQuilt). Without the pictures the earlier field pictures are drawn.
public partial class RealmGame {
 public static readonly int[] FarmDesigns={6,6,12,6,6};
 int farmEraFrame=-1;readonly Dictionary<int,int> farmEraMemo=new Dictionary<int,int>();
 int FarmAge(int owner){if(farmAgeForShots>=0)return farmAgeForShots;if(Time.frameCount!=farmEraFrame){farmEraMemo.Clear();farmEraFrame=Time.frameCount;}if(owner<0)owner=0;int e;if(!farmEraMemo.TryGetValue(owner,out e)){e=Mathf.Clamp(world.Era(owner),0,FarmDesigns.Length-1);farmEraMemo[owner]=e;}return e;}
 Texture2D FarmPicture(Tile t){int age=FarmAge(t.owner);int n=FarmDesigns[age];int pick;unchecked{pick=((t.x*92821)^(t.y*68917)^(t.x*t.y*131))&0x7fffffff;}return SpriteSlot("farm-"+age+"-"+(pick%n));}
 // The farm stands on the tile: as wide as the tile, its foot on the tile's lower edge, buildings rising above it.
 bool DrawFarmPicture(Tile t,Rect ground){var pic=FarmPicture(t);if(pic==null)return false;float w=ground.width*1.04f,h=w*pic.height/Mathf.Max(1,pic.width);
  GUI.DrawTexture(new Rect(ground.center.x-w/2,ground.yMax+ground.height*.06f-h,w,h),pic,ScaleMode.StretchToFill,true);return true;}
}
}
