using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm {
// The owner's redesigned Imperial Courts (27 Nov 2026, Paradigm-Realm-Redesigned-Imperial-Courts.zip): each leader painted in their
// own hall, filling the left frame of the Court. The paintings were cut from the owner's screens (Resources/Leaders/Court/
// court-NNN, NNN the civilisation's number; originals in Design/Imperial-Courts-2026-11-27). Each picture had the name plate and
// sometimes a flag painted on it; those are drawn live in the same places, so the painted ones never show (the flags were painted
// out of the cut, the plate is covered by the live plate). Numbers are in the cut picture's own pixels:
//   width, height, plate x, y, w, h, flag x, y, w, h.
public static class CourtPaintings {
 static readonly Dictionary<int,int[]> Data=new Dictionary<int,int[]>{
  {0,  new[]{840,572,123,476,586,88, 20,12,110,74}},
  {1,  new[]{840,557,153,470,545,93, 13, 4,113,85}},
  {3,  new[]{840,572,157,482,555,83, 11, 1,140,95}},
  {4,  new[]{840,539,143,456,563,89,  7, 1,128,87}},
  {5,  new[]{840,570,130,454,590,90,  7, 2,139,95}},
  {8,  new[]{840,546,156,448,541,88, 24, 6,124,88}},
  {9,  new[]{840,564,155,481,543,85, 20,12,110,74}},
  {10, new[]{840,562,142,458,568,90, 17, 7,128,92}},
  {11, new[]{840,552,157,460,541,85, 20,12,110,74}},
  {12, new[]{840,534,159,451,555,85, 20,12,110,74}},
  {13, new[]{840,559,140,469,584,92, 20,12,110,74}},
  {14, new[]{840,563,157,476,542,89, 20,12,110,74}},
  {15, new[]{840,559,152,468,551,93, 20,12,110,74}},
  {16, new[]{840,541,152,454,557,89, 17, 0,128,88}},
  {159,new[]{840,557,152,479,560,84, 35,21,131,88}},
 };
 public static int[] Of(int civ){int[] d;return Data.TryGetValue(civ,out d)?d:null;}
 public static bool Has(int civ){return Data.ContainsKey(civ);}
}
public partial class RealmGame {
 readonly Dictionary<int,Texture2D> courtPaintings=new Dictionary<int,Texture2D>();
 Texture2D CourtPainting(int civ){if(!CourtPaintings.Has(civ))return null;Texture2D t;if(!courtPaintings.TryGetValue(civ,out t)){t=Resources.Load<Texture2D>("Leaders/Court/court-"+civ.ToString("000"));if(t!=null)t.filterMode=FilterMode.Trilinear;courtPaintings[civ]=t;}return t;}
 // Draws the painting to cover the area (centred, cut evenly at the sides or top and bottom) and gives back where its plate and
 // flag landed on screen.
 bool DrawCourtPainting(int civ,Rect area,out Rect plate,out Rect flag){
  plate=flag=default(Rect);var t=CourtPainting(civ);var d=CourtPaintings.Of(civ);if(t==null||d==null)return false;
  float W=d[0],H=d[1],k=Mathf.Max(area.width/W,area.height/H);float dw=W*k,dh=H*k,ox=area.x+(area.width-dw)/2,oy=area.y+(area.height-dh)/2;
  float u0=(area.x-ox)/dw,v0=(area.y-oy)/dh;GUI.DrawTextureWithTexCoords(area,t,new Rect(u0,1-v0-area.height/dh,area.width/dw,area.height/dh));
  plate=new Rect(ox+d[2]*k,oy+d[3]*k,d[4]*k,d[5]*k);flag=new Rect(ox+d[6]*k,oy+d[7]*k,d[8]*k,d[9]*k);
  // A flag cut off at the side is moved in, so the whole of the live flag shows.
  if(flag.x<area.x+4)flag.x=area.x+4;if(flag.y<area.y+4)flag.y=area.y+4;
  return true;
 }
}
}
