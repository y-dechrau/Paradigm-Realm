using System;
using UnityEditor;
using UnityEngine;
using ParadigmRealm;
// Uses the exact runtime rasteriser. Opens independently of the current game.
public class TerrainReview : EditorWindow {
 Texture2D preview; Vector2 scroll;
 [MenuItem("Paradigm Realm/Terrain Transition Review")]
 static void Open(){GetWindow<TerrainReview>("Terrain Review");}
 void OnGUI(){
  GUILayout.Label("Terrain transition test map: all 21 terrains, corners, island, land bridge, river bends, junction and lake/ocean mouths.");
  if(GUILayout.Button("Render diagnostic map")){Build();}
  if(preview==null)return;
  if(GUILayout.Button("Export PNG")){
   string path=EditorUtility.SaveFilePanel("Export terrain review","","Paradigm-Terrain-Review","png");
   if(!string.IsNullOrEmpty(path))System.IO.File.WriteAllBytes(path,preview.EncodeToPNG());
  }
  scroll=GUILayout.BeginScrollView(scroll);
  GUILayout.Label(preview,GUILayout.Width(TerrainDemo.Width*56),GUILayout.Height(TerrainDemo.Height*56));
  GUILayout.EndScrollView();
 }
 void Build(){
  if(preview!=null)DestroyImmediate(preview);
  var sources=new Texture2D[21];var pixels=new Color[21][];
  for(int i=0;i<21;i++){sources[i]=Resources.Load<Texture2D>("Terrain/terrain-"+i.ToString("00"));pixels[i]=sources[i].GetPixels();}
  var palette=new TerrainInk[21][];
  for(int i=0;i<21;i++){int id=i==6?4:i;palette[i]=TerrainPalette.Build((x,y)=>{
   if(id==18)y=.12f+y*.3f;var t=sources[id];var c=pixels[id][Math.Min(t.height-1,(int)((1-y)*t.height))*t.width+Math.Min(t.width-1,(int)(x*t.width))];return new TerrainInk(c.r,c.g,c.b);
  });}
  Func<int,float,float,TerrainInk> sample=(id,x,y)=>TerrainPalette.Sample(palette[id],x,y);
  int w=TerrainDemo.Width*TerrainPainter.Size,h=TerrainDemo.Height*TerrainPainter.Size;
  preview=new Texture2D(w,h,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};var output=new Color[w*h];
  for(int y=0;y<h;y++)for(int x=0;x<w;x++){var c=TerrainPainter.Pixel((x+.5f)/TerrainPainter.Size,(y+.5f)/TerrainPainter.Size,TerrainDemo.Seed,TerrainDemo.At,sample);output[(h-1-y)*w+x]=new Color(c.r,c.g,c.b);}
  preview.SetPixels(output);preview.Apply();
 }
 void OnDisable(){if(preview!=null)DestroyImmediate(preview);}
}
