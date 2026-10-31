using System;
using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm {
// Flag pictures and the flag designer on the Identity screen (owner request: far more customisable flags).
public partial class RealmGame {
 readonly Dictionary<long,Texture2D> flagTextures=new Dictionary<long,Texture2D>();
 Texture2D FlagTexture(int owner){
  if(world==null||owner<0||owner>=world.nations.Length)return null;var n=world.nations[owner];if(n.flagPattern<0)world.EnsureFlags();
  long key=((long)n.flagPattern<<32)|((long)n.flagA<<24)|((long)n.flagB<<16)|((long)n.flagEmblem<<8)|(long)n.flagInk;Texture2D t;
  if(!flagTextures.TryGetValue(key,out t)){t=PixelTexture(FlagArt.Draw(n.flagPattern,n.flagA,n.flagB,n.flagEmblem,n.flagInk));t.filterMode=FilterMode.Point;flagTextures[key]=t;}
  return t;}
 static Color PaletteColour(int i){int v=FlagArt.Hex(i);return new Color(((v>>16)&255)/255f,((v>>8)&255)/255f,(v&255)/255f);}
 // A row of colour swatches; the chosen one has a gold frame.
 int Swatches(string label,int value){
  GUILayout.Label(label,body);var row=GUILayoutUtility.GetRect(16*30,26);
  for(int i=0;i<16;i++){var r=new Rect(row.x+i*30,row.y,26,26);Fill(r,PaletteColour(i));if(i==value){var g=new Color(1f,.82f,.42f);Fill(new Rect(r.x-2,r.y-2,r.width+4,2),g);Fill(new Rect(r.x-2,r.yMax,r.width+4,2),g);Fill(new Rect(r.x-2,r.y,2,r.height),g);Fill(new Rect(r.xMax,r.y,2,r.height),g);}
   if(GUI.Button(r,"",GUIStyle.none))value=i;}
  return value;}
 void DrawFlagDesigner(){
  var n=world.nations[0];if(n.flagPattern<0)world.EnsureFlags();
  GUILayout.Label("Flag",heading);
  var preview=GUILayoutUtility.GetRect(240,160);GUI.DrawTexture(new Rect(preview.x,preview.y,240,160),FlagTexture(0),ScaleMode.StretchToFill,true);
  GUILayout.Label("Layout",body);n.flagPattern=GUILayout.SelectionGrid(n.flagPattern,FlagArt.Patterns,7);
  n.flagA=Swatches("Field colour",n.flagA);n.flagB=Swatches("Second colour",n.flagB);
  GUILayout.Label("Emblem",body);n.flagEmblem=GUILayout.SelectionGrid(n.flagEmblem,FlagArt.Emblems,8);
  n.flagInk=Swatches("Emblem colour",n.flagInk);
  GUILayout.BeginHorizontal();if(Button("Random flag")){var roll=new System.Random(Environment.TickCount);var g=FlagArt.Generated(roll.Next(99999),0,roll.Next(16));n.flagPattern=g[0];n.flagA=g[1];n.flagB=g[2];n.flagEmblem=g[3];n.flagInk=g[4];}GUILayout.EndHorizontal();
 }
}
}
