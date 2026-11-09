using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Flag pictures and the flag designer (owner request, 9 Nov 2026: much more comprehensive, and a part of the Empire screen rather
// than its own menu entry). Layout thumbnails in your own colours, three field colours from 32, simple shapes or 32 painted
// heraldic charges (SpriteCook, Resources/Sprites/charge-01..32, drawn as white silhouettes and tinted here), emblem size,
// position and outline, live previews at banner and plate size, random and undo.
public partial class RealmGame {
 readonly Dictionary<string,Texture2D> flagTextures=new Dictionary<string,Texture2D>();
 readonly Dictionary<string,KeyValuePair<bool[],float[]>> chargeMasks=new Dictionary<string,KeyValuePair<bool[],float[]>>();
 int[] flagUndo;int flagEmblemTab;
 Texture2D FlagTexture(int owner){
  if(world==null||owner<0||owner>=world.nations.Length)return null;var n=world.nations[owner];if(n.flagPattern<0)world.EnsureFlags();
  return FlagPicture(n.flagPattern,n.flagA,n.flagB,n.flagC,n.flagEmblem,n.flagInk,n.flagSize,n.flagPos,n.flagCharge,n.flagOutline);}
 Texture2D FlagPicture(int pattern,int a,int b,int c,int emblem,int ink,int size,int pos,int charge,int outline){
  string key=pattern+"."+a+"."+b+"."+c+"."+emblem+"."+ink+"."+size+"."+pos+"."+charge+"."+outline;Texture2D t;
  if(flagTextures.TryGetValue(key,out t))return t;
  bool[] mask=null;float[] shade=null;if(charge>0){var m=ChargeMask(charge,size,pos);if(m.Key!=null){mask=m.Key;shade=m.Value;}}
  t=PixelTexture(FlagArt.Draw(pattern,a,b,c,mask==null?emblem:0,ink,size,pos,outline,mask,shade));t.filterMode=FilterMode.Point;
  if(flagTextures.Count>400){foreach(var old in flagTextures.Values)Destroy(old);flagTextures.Clear();}
  flagTextures[key]=t;return t;}
 // A painted charge scaled into the emblem box: which flag pixels it covers, and how light each is (its inner detail lines).
 KeyValuePair<bool[],float[]> ChargeMask(int charge,int size,int pos){
  string key=charge+"."+size+"."+pos;KeyValuePair<bool[],float[]> hit;if(chargeMasks.TryGetValue(key,out hit))return hit;
  var art=SpriteSlot("charge-"+charge.ToString("00"));if(art==null){chargeMasks[key]=new KeyValuePair<bool[],float[]>(null,null);return chargeMasks[key];}
  uint[] px;try{px=ReadArtPixels(art);}catch(Exception){chargeMasks[key]=new KeyValuePair<bool[],float[]>(null,null);return chargeMasks[key];}
  int W=FlagArt.W,H=FlagArt.H;double cx,cy,k;FlagArt.EmblemBox(size,pos,out cx,out cy,out k);double box=22*k;
  float aspect=art.width/(float)Mathf.Max(1,art.height);double bw=aspect>=1?box:box*aspect,bh=aspect>=1?box/aspect:box;
  var mask=new bool[W*H];var shade=new float[W*H];
  for(int y=0;y<H;y++)for(int x=0;x<W;x++){
   double u=(x+.5-(cx-bw/2))/bw,v=(y+.5-(cy-bh/2))/bh;if(u<0||v<0||u>=1||v>=1)continue;
   int sx=Mathf.Clamp((int)(u*art.width),0,art.width-1),sy=Mathf.Clamp((int)(v*art.height),0,art.height-1);uint p=px[sy*art.width+sx];
   if((p>>24)<128)continue;float lum=(((p>>16)&255)+((p>>8)&255)+(p&255))/765f;mask[y*W+x]=true;shade[y*W+x]=Mathf.Lerp(.62f,1.05f,lum);}
  hit=new KeyValuePair<bool[],float[]>(mask,shade);chargeMasks[key]=hit;return hit;}
 static Color PaletteColour(int i){int v=FlagArt.Hex(i);return new Color(((v>>16)&255)/255f,((v>>8)&255)/255f,(v&255)/255f);}
 // Two rows of sixteen colour swatches; the chosen one has a gold frame.
 int Swatches(string label,int value){
  GUILayout.Label(label+": "+FlagArt.ColourNames[Mathf.Clamp(value,0,FlagArt.ColourNames.Length-1)],body);var row=GUILayoutUtility.GetRect(16*30,56);
  for(int i=0;i<FlagArt.ColourNames.Length;i++){var r=new Rect(row.x+(i%16)*30,row.y+(i/16)*30,26,26);Fill(r,PaletteColour(i));if(i==value){var g=new Color(1f,.82f,.42f);Fill(new Rect(r.x-2,r.y-2,r.width+4,2),g);Fill(new Rect(r.x-2,r.yMax,r.width+4,2),g);Fill(new Rect(r.x-2,r.y,2,r.height),g);Fill(new Rect(r.xMax,r.y,2,r.height),g);}
   if(GUI.Button(r,new GUIContent("",FlagArt.ColourNames[i]),GUIStyle.none))value=i;}
  return value;}
 static readonly HashSet<int> ThreeColourLayouts=new HashSet<int>{3,4,16,17,23,24};
 // The identity page: name, flag designer, leader and lore (owner request, 9 Nov 2026: a part of the Empire screen).
 int empireTab;
 void DrawIdentityPage(){
                GUI.SetNextControlName("rename");renameText=GUILayout.TextField(renameText,32);if(Button("Apply civilisation name"))world.civilisationName=World.CleanName(renameText);
                DrawFlagDesigner();
                {var me=world.nations[0];var face=Portrait(me.portrait);if(face!=null)DrawPortrait(GUILayoutUtility.GetRect(150,200),face);if(!string.IsNullOrEmpty(me.leader))GUILayout.Label("Led by "+me.leader,heading);GUILayout.Label(me.traits!=null&&me.traits.Any(t=>t>=0)?CivilisationCatalog.TraitLine(me.traits):"No traits.",body);}
                GUILayout.Label("Difficulty: "+DifficultyTable.Names[Mathf.Clamp(world.difficultyLevel,0,DifficultyTable.Names.Length-1)],heading);GUILayout.Label(DifficultyTable.Describe(Mathf.Clamp(world.difficultyLevel,0,DifficultyTable.Names.Length-1)),small);
                DrawOwnCivilisationLore();
 }
 void DrawFlagDesigner(){
  var n=world.nations[0];if(n.flagPattern<0)world.EnsureFlags();
  if(flagUndo==null)flagUndo=new[]{n.flagPattern,n.flagA,n.flagB,n.flagC,n.flagEmblem,n.flagInk,n.flagSize,n.flagPos,n.flagCharge,n.flagOutline};
  GUILayout.Label("Flag",heading);
  GUILayout.BeginHorizontal();
  {var big=GUILayoutUtility.GetRect(288,192,GUILayout.Width(288));GUI.DrawTexture(big,FlagTexture(0),ScaleMode.StretchToFill,true);}
  GUILayout.Space(16);GUILayout.BeginVertical();
  GUILayout.Label("As it flies on a banner and a city plate:",small);
  {var r=GUILayoutUtility.GetRect(150,70,GUILayout.Width(150));Fill(new Rect(r.x+8,r.y,2,70),new Color(.32f,.22f,.12f));GUI.DrawTexture(new Rect(r.x+10,r.y+2,72,48),FlagTexture(0),ScaleMode.StretchToFill,true);GUI.DrawTexture(new Rect(r.x+96,r.y+20,36,24),FlagTexture(0),ScaleMode.StretchToFill,true);}
  if(Button("Random flag")){var roll=new System.Random(Environment.TickCount);var g=FlagArt.Generated(roll.Next(99999),0,roll.Next(FlagArt.ColourNames.Length));n.flagPattern=g[0];n.flagA=g[1];n.flagB=g[2];n.flagC=roll.Next(FlagArt.ColourNames.Length);n.flagEmblem=g[3];n.flagInk=g[4];n.flagCharge=roll.Next(3)==0?1+roll.Next(FlagArt.ChargeCount):0;n.flagSize=roll.Next(3);n.flagPos=roll.Next(5)==0?2:0;n.flagOutline=roll.Next(2);}
  if(Button("Undo changes")){var u=flagUndo;n.flagPattern=u[0];n.flagA=u[1];n.flagB=u[2];n.flagC=u[3];n.flagEmblem=u[4];n.flagInk=u[5];n.flagSize=u[6];n.flagPos=u[7];n.flagCharge=u[8];n.flagOutline=u[9];}
  GUILayout.EndVertical();GUILayout.EndHorizontal();
  // Layouts as thumbnails in your own colours.
  GUILayout.Space(8);GUILayout.Label("Layout: "+FlagArt.Patterns[n.flagPattern],body);
  {int per=9;for(int row=0;row*per<FlagArt.Patterns.Length;row++){var line=GUILayoutUtility.GetRect(per*64,44);
    for(int k=0;k<per&&row*per+k<FlagArt.Patterns.Length;k++){int p=row*per+k;var r=new Rect(line.x+k*64,line.y,58,39);if(p==n.flagPattern)Fill(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),new Color(1f,.82f,.42f));
     GUI.DrawTexture(r,FlagPicture(p,n.flagA,n.flagB,n.flagC,0,n.flagInk,0,0,0,0),ScaleMode.StretchToFill,true);if(GUI.Button(r,new GUIContent("",FlagArt.Patterns[p]),GUIStyle.none))n.flagPattern=p;}}}
  // Colours.
  n.flagA=Swatches("First colour",n.flagA);n.flagB=Swatches("Second colour",n.flagB);
  n.flagC=Swatches("Third colour"+(ThreeColourLayouts.Contains(n.flagPattern)?"":" (used by three bands, tricolour, hoist triangle, pall, per saltire and bordered cross)"),n.flagC);
  // Emblem.
  GUILayout.Space(6);GUILayout.Label("Emblem",heading);
  GUILayout.BeginHorizontal();if(GUILayout.Button((flagEmblemTab==0?"▸ ":"")+"Simple shapes",button,GUILayout.Width(200)))flagEmblemTab=0;if(GUILayout.Button((flagEmblemTab==1?"▸ ":"")+"Heraldic charges",button,GUILayout.Width(200)))flagEmblemTab=1;GUILayout.EndHorizontal();
  if(flagEmblemTab==0){int pick=GUILayout.SelectionGrid(n.flagCharge>0?-1:n.flagEmblem,FlagArt.Emblems,8);if(pick>=0&&(pick!=n.flagEmblem||n.flagCharge>0)){n.flagEmblem=pick;n.flagCharge=0;}}
  else{int per=11;for(int row=0;row*per<=FlagArt.ChargeCount;row++){var line=GUILayoutUtility.GetRect(per*58,58);
    for(int k=0;k<per&&row*per+k<=FlagArt.ChargeCount;k++){int ch=row*per+k;var r=new Rect(line.x+k*58,line.y,52,52);Fill(r,ch==n.flagCharge?new Color(.5f,.38f,.12f):new Color(.1f,.13f,.2f));
     var art=ch>0?SpriteSlot("charge-"+ch.ToString("00")):null;if(art!=null)GUI.DrawTexture(new Rect(r.x+5,r.y+5,42,42),art,ScaleMode.ScaleToFit,true);else GUI.Label(r,ch==0?"None":"?",hudCentre);
     if(GUI.Button(r,new GUIContent("",FlagArt.Charges[ch]),GUIStyle.none)){n.flagCharge=ch;if(ch>0)n.flagEmblem=0;}}}
   GUILayout.Label(n.flagCharge>0?"Charge: "+FlagArt.Charges[n.flagCharge]:"No charge chosen.",small);}
  n.flagInk=Swatches("Emblem colour",n.flagInk);
  GUILayout.BeginHorizontal();GUILayout.Label("Size",body,GUILayout.Width(70));n.flagSize=GUILayout.SelectionGrid(n.flagSize,FlagArt.Sizes,3,GUILayout.Width(330));GUILayout.Space(20);
  GUILayout.Label("Position",body,GUILayout.Width(80));n.flagPos=GUILayout.SelectionGrid(n.flagPos,FlagArt.Positions,4,GUILayout.Width(420));GUILayout.EndHorizontal();
  n.flagOutline=GUILayout.Toggle(n.flagOutline==1,"  Outline the emblem")?1:0;
 }
}
}
