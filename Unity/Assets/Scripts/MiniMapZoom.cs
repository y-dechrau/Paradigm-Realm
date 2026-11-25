using System;
using UnityEngine;
namespace ParadigmRealm {
// The mini-map's own zoom (owner request, 25 Nov 2026: "when hovering the cursor over the mini map and zooming in and out this should
// apply to the mini map only"). The wheel over the mini-map zooms it round the pointer and leaves the map alone; a middle-drag over it
// moves its window. Zoomed in, the window is painted again at full sharpness a few rows a frame, over the stretched whole picture. When
// the map's view leaves the window, the window follows it.
public partial class RealmGame {
 float miniZoom=1,miniU=.5f,miniV=.5f,miniTouched=-9;bool miniPan;Vector2 miniCamera;World miniZoomWorld;int miniZoomRealm=-1;
 int[] miniTileKey;Color[] miniTileInk;
 static Rect MiniInner{get{return new Rect(MiniMap.x+4,MiniMap.y+4,MiniMap.width-8,MiniMap.height-8);}}
 // The whole world's rectangle at the present zoom; larger than the panel when zoomed in, and then clipped to it.
 Rect MiniDrawn(){
  if(miniZoomWorld!=world||miniZoomRealm!=viewRealm){miniZoomWorld=world;miniZoomRealm=viewRealm;miniZoom=1;miniU=miniV=.5f;}
  var inner=MiniInner;float w,h;MiniBase(out w,out h);w*=miniZoom;h*=miniZoom;
  float x=w<=inner.width?inner.center.x-w/2:Mathf.Clamp(inner.center.x-miniU*w,inner.xMax-w,inner.x),y=h<=inner.height?inner.center.y-h/2:Mathf.Clamp(inner.center.y-miniV*h,inner.yMax-h,inner.y);
  miniU=(inner.center.x-x)/w;miniV=(inner.center.y-y)/h;return new Rect(x,y,w,h);}
 // Staggered rows are a quarter of a tile width apart (16 of 64 pixels), so a row is a quarter as tall as a column is wide.
 void MiniBase(out float w,out float h){var inner=MiniInner;float aspect=world.Width/(world.Hex?ViewRows*PixelArt.HR/(float)PixelArt.HW:world.Staggered?ViewRows*.25f:(float)ViewRows);w=inner.width;h=w/aspect;if(h>inner.height){h=inner.height;w=h*aspect;}}
 // As close as a tile 14 units wide, ten times at most.
 float MiniZoomMost(){float w,h;MiniBase(out w,out h);return Mathf.Clamp(14f*world.Width/w,4f,10f);}
 void MiniWheel(Vector2 at,float delta){
  var was=MiniDrawn();var inner=MiniInner;float u=Mathf.Clamp01((at.x-was.x)/was.width),v=Mathf.Clamp01((at.y-was.y)/was.height);
  float most=MiniZoomMost();miniZoom=Mathf.Clamp(miniZoom*(delta<0?1.25f:.8f),1f,most);if(miniZoom<1.02f)miniZoom=1;if(miniZoom>most*.98f)miniZoom=most;
  float w,h;MiniBase(out w,out h);w*=miniZoom;h*=miniZoom;miniU=(inner.center.x-at.x)/w+u;miniV=(inner.center.y-at.y)/h+v;MiniDrawn();miniTouched=Time.realtimeSinceStartup;}
 void MiniDrag(Vector2 by){var d=MiniDrawn();miniU-=by.x/d.width;miniV-=by.y/d.height;MiniDrawn();miniTouched=Time.realtimeSinceStartup;}
 // The wheel and the middle button over the mini-map belong to it alone. True: the map must not have this event.
 bool MiniInput(Event e){
  if(world==null||!MiniMap.Contains(e.mousePosition)&&!(miniPan&&e.type==EventType.MouseDrag)){if(e.type==EventType.MouseUp)miniPan=false;return false;}
  if(e.type==EventType.ScrollWheel){if(!turnRunning)MiniWheel(e.mousePosition,e.delta.y);e.Use();return true;}
  if(e.type==EventType.MouseDown&&e.button==2){miniPan=true;e.Use();return true;}
  if(e.type==EventType.MouseDrag&&e.button==2&&miniPan){MiniDrag(e.delta);e.Use();return true;}
  if(e.type==EventType.MouseUp)miniPan=false;return false;}
 // The map's view has moved: keep its middle inside the window.
 void MiniFollow(Rect view){
  var c=view.center;if((c-miniCamera).sqrMagnitude<.0000001f)return;miniCamera=c;if(miniZoom<=1)return;
  var d=MiniDrawn();var inner=MiniInner;var p=new Vector2(d.x+c.x*d.width,d.y+c.y*d.height);if(inner.Contains(p))return;
  miniU+=(p.x<inner.x?p.x-inner.x-inner.width*.25f:p.x>inner.xMax?p.x-inner.xMax+inner.width*.25f:0)/d.width;miniV+=(p.y<inner.y?p.y-inner.y-inner.height*.25f:p.y>inner.yMax?p.y-inner.yMax+inner.height*.25f:0)/d.height;MiniDrawn();}
 // ---------- the sharp picture of the window ----------
 Texture2D miniDetail;Rect miniDetailAt;int miniDetailFrom=-1,miniDetailLayer=-1;World miniDetailWorld;int miniDetailRealm=-1;float miniDetailScale;
 Color32[] detailPixels;int[] detailKey;int detailRow=-1,detailW,detailH,detailFrom;float detailScale;Rect detailAt;int detailFrame=-1,detailLayer,detailRealm;World detailWorld;
 // The window in tiles (columns, rows).
 Rect MiniWindow(Rect drawn){var inner=MiniInner;float sx=drawn.width/world.Width,sy=drawn.height/ViewRows;return new Rect((inner.x-drawn.x)/sx,(inner.y-drawn.y)/sy,inner.width/sx,inner.height/sy);}
 void MiniDetailStep(Rect drawn){
  if(Event.current.type!=EventType.Repaint||detailFrame==Time.frameCount)return;detailFrame=Time.frameCount;
  if(miniZoom<1.5f||miniTileKey==null||miniWorld!=world||turnRunning){detailRow=-1;return;}
  var win=MiniWindow(drawn);float tile=drawn.width/world.Width,want=tile*1.6f;
  bool good=miniDetail!=null&&miniDetailWorld==world&&miniDetailRealm==viewRealm&&miniDetailLayer==miniLayerBuilt&&miniDetailFrom==miniRepaints&&Mathf.Abs(miniDetailScale/want-1)<.3f&&Covers(miniDetailAt,win);
  if(detailRow>=0&&(detailFrom!=miniRepaints||detailWorld!=world||detailRealm!=viewRealm||detailLayer!=miniLayerBuilt||Mathf.Abs(detailScale/want-1)>.3f||!Covers(detailAt,win)))detailRow=-1;
  if(good){detailRow=-1;return;}
  if(detailRow<0){
   if(Time.realtimeSinceStartup-miniTouched<.15f)return;
   float mx=win.width*.1f,my=win.height*.1f,x0=Mathf.Max(0,win.x-mx),y0=Mathf.Max(0,win.y-my),x1=Mathf.Min(world.Width,win.xMax+mx),y1=Mathf.Min(ViewRows,win.yMax+my);
   detailAt=Rect.MinMaxRect(x0,y0,x1,y1);detailScale=want;detailW=Mathf.Clamp(Mathf.RoundToInt(detailAt.width*want),16,900);detailH=Mathf.Clamp(Mathf.RoundToInt(detailAt.height*want*MiniRowShare()),16,600);
   detailPixels=new Color32[detailW*detailH];detailKey=new int[detailW*detailH];detailFrom=miniRepaints;detailWorld=world;detailRealm=viewRealm;detailLayer=miniLayerBuilt;detailRow=0;}
  float until=Time.realtimeSinceStartup+.005f,sx=detailW/detailAt.width,sy=detailH/detailAt.height;
  while(detailRow<detailH&&Time.realtimeSinceStartup<until)PaintMiniRow(detailRow++,detailW,detailH,sx,sy,detailAt.x,detailAt.y,detailPixels,detailKey);
  if(detailRow<detailH)return;
  var made=EdgeMini(detailPixels,detailKey,detailW,detailH,false);PaintMiniOverlay(made,detailW,detailH,sx,sy,ViewTop,detailAt.x,detailAt.y);
  if(miniDetail==null||miniDetail.width!=detailW||miniDetail.height!=detailH){if(miniDetail!=null)Destroy(miniDetail);miniDetail=new Texture2D(detailW,detailH,TextureFormat.RGBA32,true){filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp,anisoLevel=2};}
  miniDetail.SetPixels32(made);miniDetail.Apply(true);miniDetailAt=detailAt;miniDetailFrom=detailFrom;miniDetailWorld=world;miniDetailRealm=viewRealm;miniDetailLayer=miniLayerBuilt;miniDetailScale=detailScale;detailRow=-1;detailPixels=null;detailKey=null;}
 // A window may reach past the edge of the world where the world is narrower than the panel.
 bool Covers(Rect a,Rect win){return a.x<=Mathf.Max(0,win.x)+.01f&&a.y<=Mathf.Max(0,win.y)+.01f&&a.xMax>=Mathf.Min(world.Width,win.xMax)-.01f&&a.yMax>=Mathf.Min(ViewRows,win.yMax)-.01f;}
 void DrawMiniDetail(Rect drawn){
  if(miniZoom<1.5f||miniDetail==null||miniDetailWorld!=world||miniDetailRealm!=viewRealm||miniDetailLayer!=miniLayerBuilt||miniDetailFrom!=miniRepaints)return;
  float sx=drawn.width/world.Width,sy=drawn.height/ViewRows;GUI.DrawTexture(new Rect(drawn.x+miniDetailAt.x*sx,drawn.y+miniDetailAt.y*sy,miniDetailAt.width*sx,miniDetailAt.height*sy),miniDetail,ScaleMode.StretchToFill);}
}
}
