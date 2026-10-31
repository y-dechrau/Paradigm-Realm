using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Tutorial (owner request, 31 Oct 2026): a guide in the owner's picture (Resources/Tutor/guide.png and guide-face.png) talks
// the player through the first turns and the main systems. Steps that ask for an action finish by themselves when the player
// does it (found a city, open the city screen, open research, give a unit an order, end a turn); the rest have a Next button.
// It can be minimised to his portrait, skipped, switched on for each new game, and restarted from Settings. Progress is kept
// on this computer (PlayerPrefs), not in the save.
public partial class RealmGame {
 const string GuideName="Divus Maximus";
 bool tutorialOn,tutorialMinimised,tutorialCitySeen,tutorialResearchSeen;int tutorialStep=-1;
 Texture2D guidePicture,guideFace;
 static readonly string[] TutorialTitles={"Ave, ruler!","Found your first city","Your city","Research","Explore","End the turn","Workers","Growth and happiness","Borders and the wider world","Faith","Other civilisations","Ages and victory"};
 static readonly string[] TutorialTexts={
  "Ave! I am "+GuideName+", patron of this realm, and I have seen a thousand empires rise and fall - mostly fall, usually after dinner. Pour yourself something and let me show you how to build one that lasts.",
  "See that Settler? Left-click to select it, then press Found city (or F). Good ground has fresh water, food and a hill or two nearby. Don't dawdle: an empire with no city is just a camping trip.",
  "A city! Double-click it, or press Open city, to see inside. On the left are the tiles your citizens work; on the right is what it builds. Start with a Warrior or a Worker - the gods help those who queue things.",
  "Knowledge is power, and power is the best wine. Open Research from the left bar and pick what your scholars study next. Each era opens when you know four of its six keystone technologies.",
  "The world is mostly fog, and fog is full of opportunity. Select a Warrior or Scout with the left button, then right-click a tile (or hold the right button and drag) to send it there. Explore finds villages, ruins and neighbours for you.",
  "When your units have moved and your cities are busy, press End Turn (or Enter). Things happen between turns: cities grow, research advances, and your rivals plot. Mostly plot.",
  "Workers build the roads, farms and mines that make an empire rich. Trees must be cleared first - with an axe, Clear. Anything greyed out needs a technology you don't have yet. Too much bother? Auto develop does the thinking.",
  "Cities grow when they have spare food, and stop at size 6 without an Aqueduct. Watch the content and unhappy citizens: too many unhappy ones and the city riots. Temples, entertainers and a Golden Age all help. And if a city stops growing, check it hasn't turned all its farmers into scholars!",
  "Your borders spread as cities grow. Where they meet a rival's, culture and faith slowly pull tiles one way or the other. Tribal villages near your borders join you; ancient ruins hold gold, maps and secrets for the first to step in.",
  "Faith gathers in your capital and temples. Spend it on a pantheon, then found a real religion once you know Mysticism and have a Temple. Priests carry your faith to other cities - and they keep a city cheerful. Open the Religion button to see it all.",
  "Sooner or later you'll meet the neighbours. Open Diplomacy to talk to their leader: trade, friendship, alliances - or war. Every leader has an agenda; please it and they'll like you, ignore it and they'll sulk. Like me without wine.",
  "Great deeds - new cities, wonders, victories - earn era score, and a high score brings a Golden Age. Win by holding every rival capital, by completing Transcendence, or by keeping the peace (Concord). Now go, ruler. Make me proud - and send wine."};
 // A step that waits for the player to act returns true here once it is done.
 bool TutorialStepDone(int step){
  if(world==null)return false;
  switch(step){
   case 1:return world.cities.Any(c=>c.owner==0);
   case 2:return tutorialCitySeen;
   case 3:return tutorialResearchSeen;
   case 4:return world.units.Any(u=>u.owner==0&&(u.hasOrder||u.exploring))||world.turn>=4;
   case 5:return world.turn>=2;
  }
  return false;
 }
 static bool TutorialWaits(int step){return step>=1&&step<=5;}
 void LoadTutorialSettings(){tutorialOn=PlayerPrefs.GetInt("RealmTutorialOn",1)==1;tutorialStep=PlayerPrefs.GetInt("RealmTutorialStep",-1);}
 void SaveTutorialProgress(){PlayerPrefs.SetInt("RealmTutorialOn",tutorialOn?1:0);PlayerPrefs.SetInt("RealmTutorialStep",tutorialStep);PlayerPrefs.Save();}
 // A new game starts the tutorial from the beginning when it is switched on.
 void StartTutorialForNewGame(){if(!tutorialOn){tutorialStep=-1;return;}tutorialStep=0;tutorialMinimised=false;tutorialCitySeen=false;tutorialResearchSeen=false;SaveTutorialProgress();}
 void RestartTutorial(){tutorialOn=true;tutorialStep=0;tutorialMinimised=false;tutorialCitySeen=cityScreen;tutorialResearchSeen=false;SaveTutorialProgress();}
 void AdvanceTutorial(){tutorialStep++;if(tutorialStep>=TutorialTitles.Length)tutorialStep=-1;tutorialMinimised=false;SaveTutorialProgress();}
 void DrawTutorial(){
  if(!tutorialOn||tutorialStep<0||world==null||world.result!="")return;
  if(guideFace==null)guideFace=Resources.Load<Texture2D>("Tutor/guide-face");if(guidePicture==null)guidePicture=Resources.Load<Texture2D>("Tutor/guide");
  if(TutorialWaits(tutorialStep)&&TutorialStepDone(tutorialStep)){AdvanceTutorial();if(tutorialStep<0)return;}
  bool was=GUI.enabled;GUI.enabled=true;
  if(tutorialMinimised){var tab=TutorialRect();DrawGuideFrame(tab);if(guideFace!=null)GUI.DrawTexture(new Rect(tab.x+3,tab.y+3,50,50),guideFace,ScaleMode.ScaleAndCrop);if(GUI.Button(tab,"",GUIStyle.none))tutorialMinimised=false;GUI.enabled=was;return;}
  // The welcome is a large card with the whole picture; later steps are a compact panel under the top bar.
  if(tutorialStep==0){
   Fill(new Rect(0,0,1280,800),new Color(0,0,0,.55f));var card=new Rect(240,90,800,610);DrawGuideFrame(card);
   // The picture is 4:3; the frame is wider, so show its top part (the halo, laurels and face), not the middle.
   if(guidePicture!=null){var pic=new Rect(card.x+12,card.y+12,card.width-24,420);float vh=Mathf.Min(1f,(4f/3f)/(pic.width/pic.height));GUI.DrawTextureWithTexCoords(pic,guidePicture,new Rect(0,1-vh,1,vh));}
   GUI.Label(new Rect(card.x+24,card.y+440,card.width-48,30),GuideName.ToUpperInvariant()+"  ·  "+TutorialTitles[0],heading);
   GUI.Label(new Rect(card.x+24,card.y+474,card.width-48,80),TutorialTexts[0],body);
   if(GUI.Button(new Rect(card.x+24,card.yMax-52,360,38),"Show me how to play",button))AdvanceTutorial();
   if(GUI.Button(new Rect(card.xMax-384,card.yMax-52,360,38),"I know what I'm doing (skip)",button)){tutorialStep=-1;SaveTutorialProgress();}
   GUI.enabled=was;return;}
  var panel=TutorialRect();DrawGuideFrame(panel);
  if(guideFace!=null)GUI.DrawTexture(new Rect(panel.x+8,panel.y+8,110,110),guideFace,ScaleMode.ScaleAndCrop);
  GUI.Label(new Rect(panel.x+128,panel.y+6,400,24),GuideName+" — "+TutorialTitles[tutorialStep]+"  ("+tutorialStep+"/"+(TutorialTitles.Length-1)+")",hudTitle);
  GUI.Label(new Rect(panel.x+128,panel.y+30,panel.width-136,64),TutorialTexts[tutorialStep],new GUIStyle(small){wordWrap=true,fontSize=12});
  float by=panel.yMax-28;
  if(TutorialWaits(tutorialStep))GUI.Label(new Rect(panel.x+128,by,300,24),"Do it, and I'll carry on...",small);
  else if(GUI.Button(new Rect(panel.x+128,by,150,24),tutorialStep==TutorialTitles.Length-1?"Finish":"Next",button))AdvanceTutorial();
  if(GUI.Button(new Rect(panel.xMax-210,by,96,24),"Hide",button))tutorialMinimised=true;
  if(GUI.Button(new Rect(panel.xMax-108,by,100,24),"End tutorial",button)){tutorialStep=-1;SaveTutorialProgress();}
  GUI.enabled=was;
 }
 bool TutorialShown(){return tutorialOn&&tutorialStep>0&&world!=null&&world.result==""&&!guide;}
 Rect TutorialRect(){return tutorialMinimised?new Rect(612,44,56,56):new Rect(392,44,640,126);}
 void DrawGuideFrame(Rect r){Fill(r,new Color(.05f,.08f,.15f,.96f));var g=new Color(.86f,.7f,.39f);Fill(new Rect(r.x,r.y,r.width,2),g);Fill(new Rect(r.x,r.yMax-2,r.width,2),g);Fill(new Rect(r.x,r.y,2,r.height),g);Fill(new Rect(r.xMax-2,r.y,2,r.height),g);}
}
}
