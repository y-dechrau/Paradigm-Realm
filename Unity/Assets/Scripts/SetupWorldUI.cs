using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// New Game, second page, exactly after the owner's picture (25 Nov 2026; Design/Mockups/new-game-world.png). The frame, paintings, crest
// and column headings are cut from that picture with the words and controls that change cleared away (Resources/UI/Setup); the controls
// are cut from it too. Everything is laid out in the picture's own pixels (1615 x 974), so each piece stands where it was drawn, and the
// whole is scaled to fill the display (up to a tenth wider than drawn; beyond that it is centred).
public partial class RealmGame {
 const float SetupW=1615,SetupH=974;
 Font setupFont,setupCaps;GUIStyle setText,setBody,setLabel,setOption,setBig,setField,setCap;
 readonly Dictionary<string,Texture2D> setupArt=new Dictionary<string,Texture2D>();
 Texture2D SetupTex(string name){Texture2D t;if(!setupArt.TryGetValue(name,out t)){t=Resources.Load<Texture2D>("UI/Setup/"+name);if(t!=null)t.filterMode=FilterMode.Trilinear;setupArt[name]=t;}return t;}
 void SetupStyles(){
  if(setText!=null)return;
  // The picture's lettering: Cambria where the system has it, the game's own Garamond elsewhere.
  try{if(Font.GetOSInstalledFontNames().Contains("Cambria"))setupFont=Font.CreateDynamicFontFromOSFont("Cambria",18);}catch(Exception){}
  if(setupFont==null)setupFont=Resources.Load<Font>("Fonts/EBGaramond-Regular");setupCaps=Resources.Load<Font>("Fonts/EBGaramond-SemiBold");
  GUIStyle Make(Font f,int size,Color c,TextAnchor a){var s=new GUIStyle(GUI.skin.label){font=f,fontSize=size,alignment=a,wordWrap=true,clipping=TextClipping.Overflow,padding=new RectOffset(0,0,0,0),margin=new RectOffset(0,0,0,0),richText=false};s.normal.textColor=s.hover.textColor=c;return s;}
  var white=new Color(.93f,.94f,.95f);var cream=new Color(.97f,.94f,.84f);
  setText=Make(setupFont,17,white,TextAnchor.UpperLeft);setBody=Make(setupFont,18,white,TextAnchor.UpperLeft);setLabel=Make(setupFont,21,cream,TextAnchor.UpperLeft);
  setOption=Make(setupFont,20,new Color(.96f,.95f,.9f),TextAnchor.MiddleCenter);setOption.wordWrap=false;
  setBig=Make(setupFont,31,new Color(1f,.95f,.78f),TextAnchor.MiddleCenter);setBig.wordWrap=false;
  setField=new GUIStyle(GUIStyle.none){font=setupFont,fontSize=20,alignment=TextAnchor.MiddleLeft,clipping=TextClipping.Clip,padding=new RectOffset(12,12,0,0)};setField.normal.textColor=setField.focused.textColor=setField.hover.textColor=setField.active.textColor=white;
  setCap=Make(setupCaps,30,Color.white,TextAnchor.UpperLeft);setCap.wordWrap=false;
 }
 // Text that steps its size down until all of it fits its place.
 void SetupWrite(Rect r,string text,GUIStyle style,int minimum=11){var s=style;while(s.fontSize>minimum&&(s.wordWrap?s.CalcHeight(new GUIContent(text),r.width)>r.height:s.CalcSize(new GUIContent(text)).x>r.width))s=new GUIStyle(s){fontSize=s.fontSize-1};
  var shade=new GUIStyle(s);shade.normal.textColor=shade.hover.textColor=new Color(0,0,0,.55f);GUI.Label(new Rect(r.x+1,r.y+1.2f,r.width,r.height),text,shade);GUI.Label(r,text,s);}
 // Gold small capitals, brighter above and darker below, as the picture's headings.
 void SetupHeading(float x,float y,string text){
  // A large capital begins the heading and whatever follows a colon; the rest are small.
  float at=x;bool large=true;foreach(var word in text.ToUpperInvariant().Split(' ')){
   int i=0;while(i<word.Length){string run;int size;if(i==0&&large){run=word.Substring(0,1);size=30;large=false;}else{run=word.Substring(i);size=25;if(word.EndsWith(":"))large=true;}
    var s=new GUIStyle(setCap){fontSize=size};float w=s.CalcSize(new GUIContent(run)).x,h=s.CalcHeight(new GUIContent(run),999);float drop=size==30?0:3.9f;var r=new Rect(at,y+drop,w+4,h);
    s.normal.textColor=s.hover.textColor=new Color(0,0,0,.6f);GUI.Label(new Rect(r.x+1f,r.y+1.3f,r.width,r.height),run,s);
    s.normal.textColor=s.hover.textColor=new Color(.88f,.71f,.32f);GUI.Label(r,run,s);
    float cut=h*.56f;GUI.BeginGroup(new Rect(r.x,r.y,r.width,cut));s.normal.textColor=s.hover.textColor=new Color(.97f,.86f,.5f);GUI.Label(new Rect(0,0,r.width,r.height),run,s);GUI.EndGroup();
    at+=w;i+=run.Length;}
   at+=8;}
 }
 // Three pieces across: the ends keep their shape, the middle stretches.
 void SetupBar(Rect r,string art,float cap){
  var t=SetupTex(art);if(t==null){GUI.Box(r,"");return;}if(Event.current.type!=EventType.Repaint)return;
  float u=cap/t.width,c=Mathf.Min(cap,r.width*.34f);
  // The middle runs a little under both ends, so no hairline shows between the pieces at any scale.
  float over=Mathf.Min(3,c*.3f),du=over/c*u;GUI.DrawTextureWithTexCoords(new Rect(r.x+c-over,r.y,r.width-2*(c-over),r.height),t,new Rect(u-du,0,1-2*(u-du),1));
  GUI.DrawTextureWithTexCoords(new Rect(r.x,r.y,c,r.height),t,new Rect(0,0,u,1));GUI.DrawTextureWithTexCoords(new Rect(r.xMax-c,r.y,c,r.height),t,new Rect(1-u,0,u,1));}
 bool SetupOption(Rect r,string text,bool on){
  SetupBar(new Rect(r.x-3,r.y-3,r.width+6,r.height+6),on?"setup-button-on":"setup-button",on?42:20);
  if(r.Contains(Event.current.mousePosition)&&!on&&GUI.enabled)GUI.DrawTexture(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,new Color(.55f,.75f,1f,.13f),0,0);
  SetupWrite(new Rect(r.x+5,r.y-1,r.width-10,r.height),text,setOption,10);
  return GUI.Button(r,"",GUIStyle.none);}
 int SetupGrid(int chosen,string[] names,int columns,float x,float width,float y,float h,float row,float gap=5){
  float w=(width-gap*(columns-1))/columns;for(int i=0;i<names.Length;i++)if(SetupOption(new Rect(x+i%columns*(w+gap),y+i/columns*row,w,h),names[i],chosen==i))chosen=i;return chosen;}
 int SetupSlider(float x,float y,float width,int value,int low,int high){
  const float Knob=32;var r=new Rect(x,y-16,width,32);var thumb=new GUIStyle(GUIStyle.none){fixedWidth=Knob,fixedHeight=32};var rail=new GUIStyle(GUIStyle.none){fixedHeight=32};
  value=Mathf.RoundToInt(GUI.HorizontalSlider(r,value,low,high,rail,thumb));
  if(Event.current.type==EventType.Repaint){var t=SetupTex("setup-rail");float h=22,top=y-h/2;
   if(t!=null){float u=20f/t.width;
    // The worked middle repeats, every other piece mirrored so that the joins match, and is never stretched.
    float mid=t.width-40,at=x+15;for(int n=0;at<x+width-15;n++){float w=Mathf.Min(mid,x+width-15-at);if((n&1)==0)GUI.DrawTextureWithTexCoords(new Rect(at,top,w+.6f,h),t,new Rect(u,0,w/t.width,1));else GUI.DrawTextureWithTexCoords(new Rect(at,top,w+.6f,h),t,new Rect(u+mid/t.width,0,-w/t.width,1));at+=w;}
    GUI.DrawTextureWithTexCoords(new Rect(x-3,top,20,h),t,new Rect(0,0,u,1));
    GUI.DrawTextureWithTexCoords(new Rect(x+width-17,top,20,h),t,new Rect(u,0,-u,1));}
   float f=high>low?(value-low)/(float)(high-low):0;var k=SetupTex("setup-knob");if(k!=null)GUI.DrawTexture(new Rect(x+f*(width-Knob),y-16,32,32),k,ScaleMode.StretchToFill,true);}
  return value;}
 bool SetupTick(float x,float y,string text,bool on){
  var t=SetupTex(on?"setup-tick-on":"setup-tick-off");if(t!=null)GUI.DrawTexture(new Rect(x,y,26,26),t,ScaleMode.StretchToFill,true);SetupWrite(new Rect(x+33,y+1,420,26),text,new GUIStyle(setBody){fontSize=20});
  return GUI.Button(new Rect(x,y,33+setBody.CalcSize(new GUIContent(text)).x+30,27),"",GUIStyle.none)?!on:on;}
 bool SetupBig(Rect r,string text){
  if(r.Contains(Event.current.mousePosition))GUI.DrawTexture(new Rect(r.x+14,r.y+10,r.width-28,r.height-20),Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,new Color(1f,.95f,.7f,.09f),0,0);
  SetupWrite(r,text,setBig,16);return GUI.Button(r,"",GUIStyle.none);}

 // The picture's pixels on the display.
 Matrix4x4 SetupMatrix(){
  float sy=800/SetupH,sx=MapW/SetupW;sx=sx>=sy*.95f?Mathf.Min(sx,sy*1.1f):sx;if(sx<sy*.95f)sy=sx;
  return wideMatrix*Matrix4x4.TRS(new Vector3((MapW-SetupW*sx)/2,(800-SetupH*sy)/2,0),Quaternion.identity,new Vector3(sx,sy,1));}
 void DrawWorldSetup(){
  SetupStyles();Wide();Fill(FullCanvas,new Color(.012f,.03f,.085f));
  GUI.matrix=SetupMatrix();var frame=SetupTex("setup-world");if(frame!=null)GUI.DrawTexture(new Rect(0,0,SetupW,SetupH),frame,ScaleMode.StretchToFill,false);
  string hint="";
  // ---------- your game ----------
  string civ=setupCiv<CivilisationCatalog.Count?CivilisationCatalog.Names[setupCiv]:proposedName;
  SetupWrite(new Rect(69,176,468,64),"Playing as "+civ+": "+CivilisationCatalog.TraitLine(setupCiv<CivilisationCatalog.Count?CivilisationCatalog.Traits[setupCiv]:new[]{customTraitA,customTraitB}),setText);
  SetupHeading(69,250,"Game speed");
  setupSpeed=SetupGrid(setupSpeed,new[]{"Quick","Standard","Long"},3,69,459,292,35,35);
  SetupWrite(new Rect(70,332,466,44),SpeedTexts[Mathf.Clamp(setupSpeed,0,2)],setText);
  SetupHeading(69,381,"Seed (optional)");
  SetupBar(new Rect(64,420,466,40),"setup-field",24);GUI.SetNextControlName("setupseed");seedText=GUI.TextField(new Rect(68,425,458,29),seedText,11,setField);
  SetupWrite(new Rect(69,460,466,24),"The same number and settings make the same world again.",setText);
  SetupHeading(69,488,"Difficulty: "+DifficultyTable.Names[setupLevel]);
  setupLevel=SetupGrid(setupLevel,DifficultyTable.Names,4,69,459,534,32,35);
  SetupWrite(new Rect(70,607,466,136),DifficultyTable.Describe(setupLevel),setText);
  // ---------- your world ----------
  SetupWrite(new Rect(576,177,300,26),"Shape",setLabel);
  setupShape=SetupGrid(setupShape,new[]{"Flat","Sphere"},2,575,459,208,33,35);
  SetupWrite(new Rect(577,247,462,44),setupShape==1?"A globe: travel wraps east to west and routes can cross the poles.":"A flat map with edges, as in older versions.",setText);
  SetupWrite(new Rect(577,295,300,26),"Size",setLabel);
  setupSize=SetupGrid(setupSize,World.MapSizeNames,3,575,459,325,31,35);
  {int hexes=World.HexTiles(setupShape==1?World.SphereWidths[setupSize]*World.SphereHeights[setupSize]:World.MapWidths[setupSize]*World.MapHeights[setupSize]),across=World.HexColumns(hexes);SetupWrite(new Rect(576,431,462,24),across+" x "+hexes/across+" hexes. Bigger worlds take longer turns.",setText);}
  SetupWrite(new Rect(577,462,300,26),"Land",setLabel);
  setupMap=SetupGrid(setupMap,World.LandTypeNames,3,575,459,492,31,35);
  SetupWrite(new Rect(577,562,462,26),World.LandTypeTexts[setupMap],setText);
  SetupWrite(new Rect(577,593,300,26),"Climate",setLabel);
  setupClimate=SetupGrid(setupClimate,World.ClimateNames,3,575,459,624,31,35);
  SetupWrite(new Rect(576,695,462,44),World.ClimateTexts[setupClimate],setText);
  // ---------- who shares it ----------
  int maxCivs=World.MaxCivsBySize[setupSize];if(setupCivs>maxCivs)setupCivs=maxCivs;if(setupCivs<2)setupCivs=2;
  SetupWrite(new Rect(1084,177,474,48),"Civilisations: "+setupCivs+" (you and "+(setupCivs-1)+" rival"+(setupCivs==2?"":"s")+"; up to "+maxCivs+" on this size), plus "+(setupPeoples*World.RealmCount)+" peoples in the sealed worlds",setBody);
  setupCivs=SetupSlider(1088,240,459,setupCivs,2,maxCivs);
  SetupWrite(new Rect(1085,262,470,48),"Peoples in each sealed world: "+setupPeoples+(setupPeoples>4?" (slower turns on large maps)":""),setBody);
  setupPeoples=SetupSlider(1088,320,459,setupPeoples,1,10);
  SetupWrite(new Rect(1085,344,470,26),"Wild animals: "+World.AnimalChoices[setupAnimals],new GUIStyle(setBody){fontSize=19});
  setupAnimals=SetupGrid(setupAnimals,World.AnimalChoices,5,1085,470,375,31,35);
  SetupWrite(new Rect(1086,411,472,44),setupAnimals==0?"No wild animals at all.":"Three times as many until a civilisation passes the Medieval Age.",setText);
  setupBarbarians=SetupTick(1084,460,"Barbarians",setupBarbarians);
  SetupWrite(new Rect(1086,488,474,44),setupBarbarians?"War camps appear in unclaimed land from turn "+BarbarianRules.CampsFrom+" and send out raiders. Burn a camp for gold.":"No barbarians.",setText);
  tutorialOn=SetupTick(1084,539,"Play with the tutorial",tutorialOn);
  SetupWrite(new Rect(1086,568,400,24),GuideName+" talks you through the first turns.",setText);
  // ---------- the summary, and the one setting the picture has no place for (how rivals behave), in the free space beside it ----------
  SetupWrite(new Rect(950,781,150,26),"Rival behaviour",new GUIStyle(setText){fontSize=19,wordWrap=false});
  for(int i=0;i<AiProfiles.Names.Length;i++){var r=new Rect(1098+i*228,777,222,31);if(SetupOption(r,AiProfiles.Names[i],setupProfile==i))setupProfile=i;if(r.Contains(Event.current.mousePosition))hint=AiProfiles.Texts[i]+" Difficulty bonuses are set separately.";}
  SetupWrite(new Rect(63,hint==""?780:776,870,hint==""?30:38),hint==""?SetupSummary():hint,hint==""?new GUIStyle(setText){fontSize=22,wordWrap=false}:setText,10);
  // ---------- begin, or go back ----------
  bool begin=SetupBig(new Rect(168,847,604,78),"Begin civilisation"),back=SetupBig(new Rect(846,847,604,78),"Back: civilisation");
  Centred();
  if(begin){StartGame();GUIUtility.ExitGUI();}
  if(back){setupPage=0;GUIUtility.ExitGUI();}
 }
}
}
