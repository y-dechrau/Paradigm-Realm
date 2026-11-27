using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// New Game, second page, after the owner's picture of 27 Nov 2026 (it replaces the ornate picture of 25 Nov): plain navy, thin gold
// frames, a landscape over the title, the city painting under "Who shares it". The picture with its changing words and controls
// cleared away is Resources/UI/Setup/setup2-world (1672 x 941); the buttons, rail, knob and ticks are cut from it (setup2-*). Every
// piece stands in the picture's own pixels and the whole is scaled to fill the display (up to a tenth wider than drawn; beyond that it
// is centred). Labels that never change (Shape, Size, Barbarians...) are the picture's own lettering.
public partial class RealmGame {
 const float SetupW=1672,SetupH=941;
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
  SetupBar(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),on?"setup2-button-on":"setup2-button",18);
  if(r.Contains(Event.current.mousePosition)&&!on&&GUI.enabled)GUI.DrawTexture(new Rect(r.x+4,r.y+4,r.width-8,r.height-8),Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,new Color(.55f,.75f,1f,.13f),0,0);
  SetupWrite(new Rect(r.x+5,r.y-1,r.width-10,r.height),text,setOption,10);
  return GUI.Button(r,"",GUIStyle.none);}
 int SetupGrid(int chosen,string[] names,int columns,float x,float width,float y,float h,float row,float gap=5){
  float w=(width-gap*(columns-1))/columns;for(int i=0;i<names.Length;i++)if(SetupOption(new Rect(x+i%columns*(w+gap),y+i/columns*row,w,h),names[i],chosen==i))chosen=i;return chosen;}
 // The rail keeps its rounded ends and stretches between them; the jewelled knob slides along it.
 int SetupSlider(float x,float y,float width,int value,int low,int high){
  const float Knob=28;var r=new Rect(x,y-14,width,28);var thumb=new GUIStyle(GUIStyle.none){fixedWidth=Knob,fixedHeight=28};var rail=new GUIStyle(GUIStyle.none){fixedHeight=28};
  value=Mathf.RoundToInt(GUI.HorizontalSlider(r,value,low,high,rail,thumb));
  if(Event.current.type==EventType.Repaint){var t=SetupTex("setup2-rail");float h=26,top=y-h/2;
   if(t!=null){float cap=14,u=cap/t.width;GUI.DrawTextureWithTexCoords(new Rect(x,top,cap,h),t,new Rect(0,0,u,1));GUI.DrawTextureWithTexCoords(new Rect(x+cap-1,top,width-2*cap+2,h),t,new Rect(u,0,1-2*u,1));GUI.DrawTextureWithTexCoords(new Rect(x+width-cap,top,cap,h),t,new Rect(1-u,0,u,1));}
   float f=high>low?(value-low)/(float)(high-low):0;var k=SetupTex("setup2-knob");if(k!=null)GUI.DrawTexture(new Rect(x+f*(width-Knob),y-14,Knob,Knob),k,ScaleMode.StretchToFill,true);}
  return value;}
 // The box only: its words are the picture's own; the whole line answers a click.
 bool SetupTick(float x,float y,float labelWidth,bool on){
  var t=SetupTex(on?"setup2-tick-on":"setup2-tick-off");if(t!=null)GUI.DrawTexture(new Rect(x,y,26,27),t,ScaleMode.StretchToFill,true);
  return GUI.Button(new Rect(x,y-2,36+labelWidth,31),"",GUIStyle.none)?!on:on;}
 // Gold capitals, brighter above and darker below, as the picture's headings.
 void SetupCaps(float x,float y,string text,int size){
  var s=new GUIStyle(setCap){fontSize=size};string run=text.ToUpperInvariant();var c=new GUIContent(run);float w=s.CalcSize(c).x+4,h=s.CalcHeight(c,9999);var r=new Rect(x,y,w,h);
  s.normal.textColor=s.hover.textColor=new Color(0,0,0,.6f);GUI.Label(new Rect(r.x+1f,r.y+1.3f,r.width,r.height),run,s);
  s.normal.textColor=s.hover.textColor=new Color(.86f,.68f,.36f);GUI.Label(r,run,s);
  GUI.BeginGroup(new Rect(r.x,r.y,r.width,h*.55f));s.normal.textColor=s.hover.textColor=new Color(.96f,.84f,.56f);GUI.Label(new Rect(0,0,r.width,r.height),run,s);GUI.EndGroup();}
 bool SetupBig(Rect r,string text){
  if(r.Contains(Event.current.mousePosition))GUI.DrawTexture(new Rect(r.x+14,r.y+10,r.width-28,r.height-20),Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,new Color(1f,.95f,.7f,.09f),0,0);
  SetupWrite(r,text,setBig,16);return GUI.Button(r,"",GUIStyle.none);}

 // The picture's pixels on the display.
 Matrix4x4 SetupMatrix(){
  float sy=800/SetupH,sx=MapW/SetupW;sx=sx>=sy*.95f?Mathf.Min(sx,sy*1.1f):sx;if(sx<sy*.95f)sy=sx;
  return wideMatrix*Matrix4x4.TRS(new Vector3((MapW-SetupW*sx)/2,(800-SetupH*sy)/2,0),Quaternion.identity,new Vector3(sx,sy,1));}
 void DrawWorldSetup(){
  SetupStyles();Wide();Fill(FullCanvas,new Color(.035f,.06f,.12f));
  GUI.matrix=SetupMatrix();var frame=SetupTex("setup2-world");if(frame!=null)GUI.DrawTexture(new Rect(0,0,SetupW,SetupH),frame,ScaleMode.StretchToFill,false);
  string hint="";var text=new GUIStyle(setText){fontSize=17};
  // ---------- your game ----------
  string civ=setupCiv<CivilisationCatalog.Count?CivilisationCatalog.Names[setupCiv]:proposedName;
  SetupWrite(new Rect(55,153,492,46),"Playing as "+civ+": "+CivilisationCatalog.TraitLine(setupCiv<CivilisationCatalog.Count?CivilisationCatalog.Traits[setupCiv]:new[]{customTraitA,customTraitB}),text);
  setupSpeed=SetupGrid(setupSpeed,new[]{"Quick","Standard","Long"},3,55,484,259,31,35,7);
  SetupWrite(new Rect(56,300,490,44),SpeedTexts[Mathf.Clamp(setupSpeed,0,2)],text);
  GUI.SetNextControlName("setupseed");seedText=GUI.TextField(new Rect(62,392,472,24),seedText,11,new GUIStyle(setField){fontSize=18,padding=new RectOffset(6,6,0,0)});
  SetupCaps(56,457,"Difficulty: "+DifficultyTable.Names[setupLevel],25);
  setupLevel=SetupGrid(setupLevel,DifficultyTable.Names,4,57,485,497,30,36,7);
  SetupWrite(new Rect(57,576,490,126),DifficultyTable.Describe(setupLevel),text);
  // ---------- your world ----------
  setupShape=SetupGrid(setupShape,new[]{"Flat","Sphere"},2,595,476,187,32,35,7);
  SetupWrite(new Rect(596,226,478,40),setupShape==1?"A globe: travel wraps east to west and routes can cross the poles.":"A flat map with edges, as in older versions.",text);
  setupSize=SetupGrid(setupSize,World.MapSizeNames,3,595,476,303,30,35,7);
  {int hexes=World.HexTiles(setupShape==1?World.SphereWidths[setupSize]*World.SphereHeights[setupSize]:World.MapWidths[setupSize]*World.MapHeights[setupSize]),across=World.HexColumns(hexes);SetupWrite(new Rect(596,407,478,22),across+" x "+hexes/across+" hexes. Bigger worlds take longer turns.",text);}
  setupMap=SetupGrid(setupMap,World.LandTypeNames,3,595,476,462,30,35,7);
  SetupWrite(new Rect(596,533,478,22),World.LandTypeTexts[setupMap],text);
  setupClimate=SetupGrid(setupClimate,World.ClimateNames,3,595,476,590,30,35,7);
  SetupWrite(new Rect(596,662,478,22),World.ClimateTexts[setupClimate],text);
  // ---------- who shares it ----------
  int maxCivs=World.MaxCivsBySize[setupSize];if(setupCivs>maxCivs)setupCivs=maxCivs;if(setupCivs<2)setupCivs=2;
  SetupWrite(new Rect(1122,153,500,46),"Civilisations: "+setupCivs+" (you and "+(setupCivs-1)+" rival"+(setupCivs==2?"":"s")+"; up to "+maxCivs+" on this size), plus "+(setupPeoples*World.RealmCount)+" peoples in the sealed worlds",setBody);
  setupCivs=SetupSlider(1125,215,492,setupCivs,2,maxCivs);
  SetupWrite(new Rect(1122,239,500,46),"Peoples in each sealed world: "+setupPeoples+(setupPeoples>4?" (slower turns on large maps)":""),setBody);
  setupPeoples=SetupSlider(1125,295,492,setupPeoples,1,10);
  SetupWrite(new Rect(1122,317,500,24),"Wild animals: "+World.AnimalChoices[setupAnimals],new GUIStyle(setBody){fontSize=19});
  setupAnimals=SetupGrid(setupAnimals,World.AnimalChoices,5,1122,500,345,32,35,6);
  SetupWrite(new Rect(1123,381,500,22),setupAnimals==0?"No wild animals at all.":"Three times as many until a civilisation passes the Medieval Age.",text);
  setupBarbarians=SetupTick(1124,422,110,setupBarbarians);
  SetupWrite(new Rect(1124,446,500,44),setupBarbarians?"War camps appear in unclaimed land from turn "+BarbarianRules.CampsFrom+" and send out raiders. Burn a camp for gold.":"No barbarians.",text);
  tutorialOn=SetupTick(1124,497,200,tutorialOn);
  // ---------- the summary, and how rivals behave, in the bar ----------
  for(int i=0;i<AiProfiles.Names.Length;i++){var r=i==0?new Rect(1169,750,223,33):new Rect(1400,750,216,33);if(SetupOption(r,AiProfiles.Names[i],setupProfile==i))setupProfile=i;if(r.Contains(Event.current.mousePosition))hint=AiProfiles.Texts[i]+" Difficulty bonuses are set separately.";}
  SetupWrite(new Rect(62,hint==""?752:749,880,hint==""?30:38),hint==""?SetupSummary():hint,hint==""?new GUIStyle(setText){fontSize=21,wordWrap=false}:text,10);
  // ---------- begin, or go back ----------
  bool begin=SetupBig(new Rect(148,827,674,60),"Begin civilisation"),back=SetupBig(new Rect(849,827,674,60),"Back: civilisation");
  Centred();
  if(begin){StartGame();GUIUtility.ExitGUI();}
  if(back){setupPage=0;GUIUtility.ExitGUI();}
 }
}
}
