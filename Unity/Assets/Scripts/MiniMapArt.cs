using System;
using UnityEngine;
namespace ParadigmRealm {
// A sharper minimap (owner request, 9 Nov 2026). The atlas texture keeps one pixel per tile for the globe and the atlas screen;
// the minimap gets its own picture with several pixels per tile: the terrain colour lightly mottled, shallows along coasts,
// a tint and a bright edge for each civilisation's land, rivers as blue threads and cities as framed dots. It is drawn smooth
// with mipmaps, so a large world shrinks cleanly into the small panel.
public partial class RealmGame {
 Texture2D miniTexture;World miniWorld;float miniBuilt=-99;int miniRealm=-1;Color32[] miniPixels;
 void BuildMiniTexture(){
  int W=world.Width,H=ViewRows,top=ViewTop;int scale=Mathf.Clamp(560/Math.Max(1,W),3,8);int tw=W*scale,th=H*scale;
  if(miniTexture==null||miniTexture.width!=tw||miniTexture.height!=th){if(miniTexture!=null)Destroy(miniTexture);miniTexture=new Texture2D(tw,th,TextureFormat.RGBA32,true){filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp,anisoLevel=2};miniPixels=new Color32[tw*th];}
  var dark=new Color32(6,10,16,255);
  for(int y=0;y<H;y++)for(int x=0;x<W;x++){
   var source=world.tiles[(y+top)*W+x];var t=world.KnownTile(source);
   Color c;bool seen=t.seen;
   if(!seen)c=new Color(.025f,.04f,.06f,1);else{c=AtlasColour(t.terrain);if(t.owner>=0){var n=nations[t.owner];c=Color.Lerp(c,new Color(n.r,n.g,n.b,1),.32f);}}
   bool water=seen&&PixelArt.IsWater(t.terrain),river=seen&&t.terrain==7;
   // Neighbours for coasts and borders (north, east, south, west in grid steps).
   int ownN=OwnerAt(x,y-1,top),ownE=OwnerAt(x+1,y,top),ownS=OwnerAt(x,y+1,top),ownW=OwnerAt(x-1,y,top);
   bool landN=LandAt(x,y-1,top),landE=LandAt(x+1,y,top),landS=LandAt(x,y+1,top),landW=LandAt(x-1,y,top);
   for(int py=0;py<scale;py++)for(int px=0;px<scale;px++){
    Color p=c;
    if(seen){
     // Gentle mottling so land does not read as flat squares.
     float n=Hash01(x*scale+px,y*scale+py);p=Color.Lerp(p,p*(water?1.08f:1.12f),n*.6f);p=Color.Lerp(p,p*.88f,(1-n)*.35f);
     // Shallows: water pixels next to land are lighter.
     if(water&&!river){bool near=(py==0&&landN)||(py==scale-1&&landS)||(px==0&&landW)||(px==scale-1&&landE);if(near)p=Color.Lerp(p,new Color(.45f,.7f,.8f,1),.45f);}
     // Rivers: a blue thread through the middle of the tile on a land-coloured bank.
     if(river){p=AtlasColour(0);int mid=scale/2;if(Math.Abs(px-mid)<=Math.Max(0,scale/4)||Math.Abs(py-mid)<=Math.Max(0,scale/4))p=new Color(.2f,.45f,.75f,1);}
     // Borders: a bright edge in the owner's colour where the owner changes.
     if(t.owner>=0){var ink=nations[t.owner];bool edge=(py==0&&ownN!=t.owner)||(py==scale-1&&ownS!=t.owner)||(px==0&&ownW!=t.owner)||(px==scale-1&&ownE!=t.owner);if(edge)p=Color.Lerp(new Color(ink.r,ink.g,ink.b,1),Color.white,.25f);}
    }
    // Texture rows run bottom-up.
    miniPixels[((H-1-y)*scale+(scale-1-py))*tw+x*scale+px]=p;}
  }
  // Cities: a dot in the owner's colour with a light frame, about two tiles across; your own a little larger.
  foreach(var city in world.cities){var ct=world.tiles[city.y*W+city.x];if(ct.realm!=viewRealm||!(city.owner==0||ct.seen))continue;int r=Math.Max(2,scale*(city.owner==0?5:4)/4);
   int cx=city.x*scale+scale/2,cy=(H-1-(city.y-top))*scale+scale/2;var ink=nations[city.owner];
   for(int dy=-r;dy<=r;dy++)for(int dx=-r;dx<=r;dx++){int d2=dx*dx+dy*dy;if(d2>r*r)continue;int qx=cx+dx,qy=cy+dy;if(qx<0||qy<0||qx>=tw||qy>=th)continue;
    miniPixels[qy*tw+qx]=d2>=(r-1)*(r-1)?new Color32(240,232,210,255):new Color32((byte)(ink.r*255),(byte)(ink.g*255),(byte)(ink.b*255),255);}}
  // Found sealed gateways stay marked in gold.
  if(viewRealm==0&&world.gatesFound!=null)foreach(int cell in world.gatesFound){var g=world.tiles[cell];int cx=g.x*scale+scale/2,cy=(H-1-(g.y-top))*scale+scale/2,r=Math.Max(2,scale);
   for(int dy=-r;dy<=r;dy++)for(int dx=-r;dx<=r;dx++){if(Math.Abs(dx)+Math.Abs(dy)>r)continue;int qx=cx+dx,qy=cy+dy;if(qx<0||qy<0||qx>=tw||qy>=th)continue;miniPixels[qy*tw+qx]=new Color32(255,210,64,255);}}
  miniTexture.SetPixels32(miniPixels);miniTexture.Apply(true);miniWorld=world;miniBuilt=Time.realtimeSinceStartup;miniRealm=viewRealm;
 }
 int OwnerAt(int x,int y,int top){if(y<0||y>=ViewRows)return -2;x=((x%world.Width)+world.Width)%world.Width;var t=world.KnownTile(world.tiles[(y+top)*world.Width+x]);return t.seen?t.owner:-2;}
 bool LandAt(int x,int y,int top){if(y<0||y>=ViewRows)return false;x=((x%world.Width)+world.Width)%world.Width;var t=world.KnownTile(world.tiles[(y+top)*world.Width+x]);return t.seen&&!PixelArt.IsWater(t.terrain);}
 // The gold hover lists where the money goes (owner request, 9 Nov 2026: road upkeep must be visible).
 string goldTip="";float goldTipAt=-9;
 string GoldTooltipDetail(){if(Time.realtimeSinceStartup-goldTipAt<.5f)return goldTip;goldTipAt=Time.realtimeSinceStartup;var b=world.BudgetDetail(0);return goldTip=" (income "+b.Income+"; units "+(b.army+b.civilians)+", buildings "+(b.cityUpkeep+b.buildingUpkeep)+", roads "+b.roads+" for "+world.RoadTiles(0)+" road tiles)";}
 static float Hash01(int x,int y){unchecked{int h=x*374761393+y*668265263;h=(h^(h>>13))*1274126177;return ((h^(h>>16))&1023)/1023f;}}
}
}
