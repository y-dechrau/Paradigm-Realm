using System.Collections;
using System.IO;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (16 Nov 2026): "-spriteCompare <folder>" draws every unit type standing, then its walking frames 0 and 3,
// its attack mid-way and its held guard, all at the map's unit size on one ground line, and saves the sheet, then quits.
// A row whose pictures differ in size or footing shows a clip that is not matched to the standing picture.
public partial class RealmGame {
 bool spriteCompare;
 void DrawSpriteCompare(){
  Fill(FullCanvas,new Color(.24f,.34f,.2f));
  string[] heads={"standing","walk 0","walk 3","attack","guard"};var cap=new GUIStyle(hudSmall){fontSize=11,alignment=TextAnchor.MiddleCenter,wordWrap=false,clipping=TextClipping.Overflow};
  for(int c=0;c<5;c++)GUI.Label(new Rect(150+c*86,4,86,16),heads[c],cap);
  int rows=Mathf.Min(16,people!=null?people.Length:0);
  for(int type=0;type<rows;type++){
   int col=type<8?0:1;float y=24+(type%8)*96,x0=col*640;
   GUI.Label(new Rect(x0+6,y+40,140,16),World.UnitNames[type],new GUIStyle(cap){alignment=TextAnchor.MiddleLeft});
   var pics=new Texture2D[5];
   pics[0]=unitSprites!=null&&unitSprites[type]!=null?unitSprites[type]:people[type];
   pics[1]=WalkFrame(type,0f);pics[2]=WalkFrame(type,3f/WalkFramesPerTile);
   var a=CombatFrames(type,"attack");pics[3]=a.Length>0?a[a.Length/2]:null;var g=CombatFrames(type,"guard");pics[4]=g.Length>0?g[g.Length-1]:null;
   for(int c=0;c<5;c++){var box=new Rect(x0+150+c*86,y,80,80);Fill(new Rect(box.x,box.yMax-2,box.width,1),new Color(1,1,1,.35f));
    if(pics[c]!=null)GUI.DrawTexture(new Rect(box.x,box.y,80,80),pics[c],ScaleMode.ScaleToFit,true);}
  }
 }
 IEnumerator SpriteCompareRun(string folder){
  Directory.CreateDirectory(folder);yield return Frames(5);
  seedText="421";mainMenu=false;nameSetup=false;guide=false;tutorialOn=false;tutorialStep=-1;StartGame();hasSession=true;guide=false;tutorialOn=false;tutorialStep=-1;yield return Frames(10);
  spriteCompare=true;yield return Frames(30);yield return Shot(folder,"sprite-compare.bmp");spriteCompare=false;Application.Quit();
 }
}
}
