using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Prompts from the guide after a discovery (owner request, 6 Nov 2026): first what to research next, then, one at a time, each
// idle city that can now build something new. A small picture of Divus Maximus sits beside each. The private picture may be
// absent (the public backup leaves it out), in which case the panel simply has no face.
public partial class RealmGame {
 Texture2D GuideFace(){if(guideFace==null)guideFace=Resources.Load<Texture2D>("Tutor/guide-face");return guideFace;}
 // Divus Maximus raises his cup beside every guide prompt (owner request, 11 Nov 2026): 18 frames in Resources/Tutor/Cheers, made
 // from his picture. Like the picture they are private and absent from the public backup, and then nothing is drawn.
 const int CheersFrames=18;const float CheersFrameTime=.07f,CheersHold=.45f,CheersRest=2.2f;Texture2D[] cheers;bool cheersLooked;float cheersStart=-1,cheersSeen=-1;
 Texture2D CheersFrame(){
  if(!cheersLooked){cheersLooked=true;var found=new Texture2D[CheersFrames];for(int i=0;i<CheersFrames;i++){found[i]=Resources.Load<Texture2D>("Tutor/Cheers/guide-cheers-"+i.ToString("00"));if(found[i]==null){found=null;break;}}cheers=found;}
  if(cheers==null)return null;float now=Time.realtimeSinceStartup;
  // A prompt that was not on screen a moment ago starts the toast from the beginning.
  if(cheersStart<0||now-cheersSeen>.5f)cheersStart=now;cheersSeen=now;
  int top=CheersFrames/3+2;float up=top*CheersFrameTime,t=(now-cheersStart)%(CheersFrames*CheersFrameTime+CheersHold+CheersRest);
  int frame=t<up?(int)(t/CheersFrameTime):t<up+CheersHold?top:Mathf.Min(CheersFrames-1,top+(int)((t-up-CheersHold)/CheersFrameTime));
  return cheers[Mathf.Clamp(frame,0,CheersFrames-1)];
 }
 bool PantheonDue(){var n=world.nations[0];return n.pantheon<0&&n.faith>=World.PantheonCost&&world.PantheonReason(0)=="";}
 bool GuidePromptOpen(){return world!=null&&!turnRunning&&world.result==""&&!guide&&!confirmNew&&pendingUnit==-1&&((world.researchPrompts!=null&&world.researchPrompts.Count>0)||PantheonDue()||world.NextBuildPrompt()!=null);}
 // The End Turn button runs the turn a little at a time, so the map keeps moving and never freezes.
 bool turnRunning;int turnStep;const long TurnSliceMs=20;string autoSaveFolder;
 System.Collections.IEnumerator RunTurn(){
  var clock=System.Diagnostics.Stopwatch.StartNew();var steps=world.TurnSteps().GetEnumerator();turnStep=0;
  while(true){
   bool more;try{more=steps.MoveNext();}catch(Exception e){Debug.LogException(e);status="The turn could not finish: "+e.Message;more=false;}
   if(!more)break;turnStep=steps.Current;if(clock.ElapsedMilliseconds>=TurnSliceMs){yield return null;clock.Reset();clock.Start();}
  }
  steps.Dispose();turnStep=world.civCount;
  if(AutoSaveDue()){string path=System.IO.Path.Combine(autoSaveFolder??Application.persistentDataPath,"earth-auto-"+(world.turn%3)+".json");var job=System.Threading.Tasks.Task.Run(()=>AtomicSave(path));while(!job.IsCompleted)yield return null;if(job.Exception!=null)world.Note("Autosave failed: "+job.Exception.GetBaseException().Message);}
  status=world.result!=""?world.result:"Turn "+world.turn+". Choose your orders.";turnRunning=false;
 }
 Rect GuidePanel(string line,float height,out float y,float textHeight=62){
  var box=new Rect(370,Mathf.Max(60,170-Mathf.Max(0,height-430)/2),540,height);Fill(new Rect(0,0,1280,800),new Color(0,0,0,.35f));DrawGuideFrame(box);
  var face=GuideFace();if(face!=null)GUI.DrawTexture(new Rect(box.x+14,box.y+14,84,84),face,ScaleMode.ScaleAndCrop);
  {var toast=CheersFrame();if(toast!=null){var pic=new Rect(box.x-312,box.y,300,225);DrawGuideFrame(new Rect(pic.x-6,pic.y-6,pic.width+12,pic.height+12));GUI.DrawTexture(pic,toast,ScaleMode.StretchToFill);}}
  float tx=box.x+112,tw=box.width-126;GUI.Label(new Rect(tx,box.y+10,tw,24),GuideName,hudTitle);
  GUI.Label(new Rect(tx,box.y+36,tw,textHeight),line,new GUIStyle(body){wordWrap=true});y=box.y+44+textHeight;return box;
 }
 void DrawGuidePrompt(){
  GUI.enabled=true;
  if(world.researchPrompts!=null&&world.researchPrompts.Count>0){DrawResearchPrompt();return;}
  if(PantheonDue()){DrawPantheonPrompt();return;}
  var p=world.NextBuildPrompt();if(p!=null)DrawBuildPrompt(p);
 }
 // Enough faith has gathered: the player must name a god (owner request, 6 Nov 2026). There is no putting it off.
 const string PantheonPlea="Your people are leaving offerings at every shrine in sight, but each village prays to a different power and they squabble over whose feast comes first. *hic* All that devotion, going to waste, like good wine left out in the sun. Pick ONE god as patron of your whole civilisation: its priests put your cities on one calendar of rites, and its blessing reaches every one of them. It's forever, mind, so choose what your people really need. Not just the one with the best parties.";
 void DrawPantheonPrompt(){
  float y;var names=ReligionCatalog.PantheonNames;var box=GuidePanel(PantheonPlea,44+156+names.Length*40+14,out y,156);
  var text=new GUIStyle(hudSmall){wordWrap=true,alignment=TextAnchor.MiddleLeft};text.normal.textColor=UiText;
  for(int p=0;p<names.Length;p++){if(GUI.Button(new Rect(box.x+14,y,box.width-28,36),"",button)){world.ChoosePantheon(0,p);status="Your people now follow the "+names[p]+".";GUIUtility.ExitGUI();}GUI.Label(new Rect(box.x+24,y,150,36),names[p],new GUIStyle(hudCaption){alignment=TextAnchor.MiddleLeft});GUI.Label(new Rect(box.x+176,y,box.width-200,36),ReligionCatalog.PantheonText[p],text);y+=40;}
 }
 void DrawResearchPrompt(){
  int found=world.researchPrompts[0];var choices=world.AvailableTech(0).OrderBy(t=>world.ResearchCost(t,0)).ToList();
  if(choices.Count==0){world.researchPrompts.RemoveAt(0);return;}
  // The cheapest few, always including the one research has already moved on to.
  var shown=choices.Take(6).ToList();if(world.research>=0&&!shown.Contains(world.research)){shown[shown.Count-1]=world.research;}
  float y;var box=GuidePanel("*hic* "+World.TechNames[found]+"! They've only gone and discovered "+World.TechNames[found]+". Marvellous. Now what d'you want them to study next? Pick one before I nod off.",106+shown.Count*34+50,out y);
  foreach(int t in shown){
   int turns=world.ResearchTurns(t,0);
   string[] gains=World.ActiveTechBenefits(t);string gain=gains.Length>0?"  —  "+gains[0]:"";
   if(GUI.Button(new Rect(box.x+14,y,box.width-28,30),"",button)){world.ChooseNextResearch(t);status="Researching "+World.TechNames[t]+".";world.researchPrompts.RemoveAt(0);GUIUtility.ExitGUI();}
   var was=new GUIStyle(hudSmall){wordWrap=false,alignment=TextAnchor.MiddleLeft};was.normal.textColor=t==world.research?UiGold:UiText;
   FitLabel(new Rect(box.x+24,y,box.width-120,30),World.TechNames[t]+gain,was,9);
   GUI.Label(new Rect(box.xMax-96,y,72,30),turns<0?"no science":turns+(turns==1?" turn":" turns"),hudNumber);
   y+=34;
  }
  y+=6;
  if(GUI.Button(new Rect(box.x+14,y,252,32),"See every technology",button)){world.researchPrompts.RemoveAt(0);researchScreen=true;detailsScreen="";ResetMapGesture();GUI.FocusControl(null);GUIUtility.ExitGUI();}
  if(world.research>=0&&GUI.Button(new Rect(box.x+274,y,252,32),"Carry on with "+World.TechNames[world.research],button)){world.researchPrompts.RemoveAt(0);GUIUtility.ExitGUI();}
 }
 void DrawBuildPrompt(BuildPrompt p){
  var c=world.cities.Find(a=>a.id==p.city);var shown=p.options.Take(5).ToList();int more=world.buildPrompts.Count-1;
  float y;var box=GuidePanel(World.TechNames[p.tech]+" lets "+c.name+" build something new, and "+c.name+"'s just sat there doing nothing. Like me. *burp* What should it build?"+(more>0?"  ("+more+" more "+(more==1?"city":"cities")+" after this)":""),106+shown.Count*34+50,out y);
  int output=Math.Max(1,world.Yield(c)[1]);
  foreach(int b in shown){
   int turns=Math.Max(1,(world.CostFor(c,b)+output-1)/output);
   if(GUI.Button(new Rect(box.x+14,y,box.width-28,30),"",button)){world.ChooseProject(c,b,false);status=c.name+" will build "+world.BuildNameFor(0,b)+".";world.buildPrompts.RemoveAt(0);GUIUtility.ExitGUI();}
   var line=new GUIStyle(hudSmall){wordWrap=false,alignment=TextAnchor.MiddleLeft};line.normal.textColor=UiText;
   FitLabel(new Rect(box.x+24,y,box.width-120,30),world.BuildNameFor(0,b)+"  —  "+World.BuildingEffect(b),line,9);
   GUI.Label(new Rect(box.xMax-96,y,72,30),turns+(turns==1?" turn":" turns"),hudNumber);
   y+=34;
  }
  y+=6;
  if(GUI.Button(new Rect(box.x+14,y,252,32),"Open "+c.name,button)){world.buildPrompts.RemoveAt(0);OpenCity(c);GUIUtility.ExitGUI();}
  if(GUI.Button(new Rect(box.x+274,y,252,32),"Not now",button)){world.buildPrompts.RemoveAt(0);GUIUtility.ExitGUI();}
 }
 // ---------- the empire's priorities (Empire screen) ----------
 static readonly string[] PriorityNames={"Science","Military","Finance"};
 void DrawEmpireBalance(){
  var n=world.nations[0];int[] rates={n.scienceRate,n.militaryRate,world.FinanceRate(0)};
  var own=world.cities.Where(c=>c.owner==0).ToList();int trade=own.Sum(c=>world.Trade(c));
  bool training=own.Any(c=>World.IsCombatBuild(c.build));
  int[] flow={own.Sum(c=>world.ScienceShare(c)),own.Where(c=>World.IsCombatBuild(c.build)).Sum(c=>world.ArmyBudget(c)),own.Sum(c=>world.GoldShare(c))};
  string[] unit={" science"," shields"," gold"};
  GUILayout.Label("Empire priorities",heading);
  for(int axis=0;axis<3;axis++){
   GUILayout.BeginHorizontal();GUILayout.Label(PriorityNames[axis]+"  "+rates[axis]+"%",body,GUILayout.Width(150));
   int moved=Mathf.RoundToInt(GUILayout.HorizontalSlider(rates[axis],0,100,GUILayout.Width(620),GUILayout.Height(22))/10f)*10;
   GUILayout.Space(16);GUILayout.Label("+"+flow[axis]+unit[axis]+" a turn from Trade",body,GUILayout.Width(300));GUILayout.EndHorizontal();
   if(moved!=rates[axis]){world.SetPriority(0,axis,moved);rates=new[]{n.scienceRate,n.militaryRate,world.FinanceRate(0)};}
  }
  GUILayout.Label("Your cities' Trade is shared between these three, always totalling 100%. Military adds shields to any combat unit a city is training"+(n.militaryRate>0&&!training?" (none is training one now, so that share stays in the treasury as gold)":"; a city not training one keeps that share as gold")+". Science stops at "+world.ScienceCap(0)+"%. Finance of 40% or less counts as low tax (+1 content citizen per city); 70% or more is high tax (-1). Each city supports 3 units free; government adds more.",body);
 }
 // ---------- the Chronicle's lettering ----------
 GUIStyle chronicleText,chronicleHeading,chronicleTitle;
 void ChronicleStyles(){
  if(chronicleText!=null)return;
  // A calligraphic face that comes with Windows. It is not shipped with the game, so other systems fall back to the next name.
  Font hand=null;try{hand=Font.CreateDynamicFontFromOSFont(new[]{"Gabriola","Lucida Calligraphy","Monotype Corsiva","Palatino Linotype"},26);}catch(Exception){}
  var ink=new Color(.96f,.9f,.76f);
  chronicleText=new GUIStyle(body){fontSize=25,wordWrap=true,richText=false,padding=new RectOffset(4,4,0,0),margin=new RectOffset(0,0,0,2)};chronicleText.normal.textColor=ink;
  chronicleHeading=new GUIStyle(chronicleText){fontSize=33};chronicleHeading.normal.textColor=UiGold;
  chronicleTitle=new GUIStyle(title){fontSize=44,fontStyle=FontStyle.Normal};chronicleTitle.normal.textColor=UiGold;
  if(hand!=null){chronicleText.font=hand;chronicleHeading.font=hand;chronicleTitle.font=hand;}
 }
}
}
