using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Empire > Statecraft (24 Nov 2026; rules in Statecraft.cs): the three ways to govern side by side, what each does, and your own
// realm's figures under each, so the choice can be weighed before it is made. Choosing asks once more.
public partial class RealmGame {
 int statecraftAsk=-1;World.StatecraftFigures[] statecraftShown;float statecraftShownAt=-9;int statecraftShownTurn=-1;World statecraftShownWorld;
 World.StatecraftFigures[] StatecraftNumbers(){
  if(statecraftShown==null||statecraftShownWorld!=world||statecraftShownTurn!=world.turn||Time.realtimeSinceStartup-statecraftShownAt>2){statecraftShown=Enumerable.Range(0,StatecraftRules.Count).Select(k=>world.PreviewStatecraft(0,k)).ToArray();statecraftShownAt=Time.realtimeSinceStartup;statecraftShownTurn=world.turn;statecraftShownWorld=world;}
  return statecraftShown;}
 static string[] StatecraftEffects(int k){
  if(k==StatecraftRules.Centralised)return new[]{"+"+StatecraftRules.PercentPerCity+"% science, shields and gold for each city fewer than "+StatecraftRules.FewCities+" (at most +"+StatecraftRules.MostPercent+"%)","New soldiers start as Veterans","+"+StatecraftRules.InfluencePerPact+" influence a turn for each pact (at most +"+StatecraftRules.MostPactInfluence+")","Settlers cost "+StatecraftRules.SettlerDearer+"% more"};
  if(k==StatecraftRules.Expansive)return new[]{StatecraftRules.ServicePercent+"% of citizens can serve in peacetime, not "+ForcesRules.PeacePercent+"%","Settlers cost "+StatecraftRules.SettlerCheaper+"% less","Extra cities cost a third less gold"};
  return new[]{"No bonus and no drawback"};}
 void DrawStatecraftScreen(){
  ForcesStyles();GUI.Box(new Rect(24,100,1232,630),"");GUI.Label(new Rect(50,115,800,40),"Empire",title);
  if(GUI.Button(new Rect(1040,115,190,34),"Back",button)){detailsScreen="";empireTab=0;GUIUtility.ExitGUI();}
  if(GUI.Button(new Rect(55,158,220,30),"Overview",button)){empireTab=0;statecraftAsk=-1;GUIUtility.ExitGUI();}
  if(GUI.Button(new Rect(281,158,220,30),"Flag and identity",button)){empireTab=1;flagUndo=null;statecraftAsk=-1;GUIUtility.ExitGUI();}
  GUI.Button(new Rect(507,158,220,30),"▸ Statecraft",button);
  bool modal=statecraftAsk>=0,was=GUI.enabled;if(modal)GUI.enabled=false;
  var n=world.nations[0];var f=StatecraftNumbers();int now=n.statecraft,cities=world.cities.Count(c=>c.owner==0);bool settling=world.StatecraftUnsettled(0);
  var text=new GUIStyle(fSmall){fontSize=14};var head=new GUIStyle(fHead){fontSize=18};
  GUI.Label(new Rect(58,196,1160,24),"Govern few cities well, or many cities in strength. You may change your mind, but a change takes "+StatecraftRules.SettleTurns+" turns to settle and must stand for "+StatecraftRules.MinTurns+".",text);
  float w=(1170-24)/3f;var order=new[]{StatecraftRules.Centralised,StatecraftRules.Balanced,StatecraftRules.Expansive};
  for(int i=0;i<3;i++){
   int k=order[i];var r=new Rect(55+i*(w+12),226,w,418);ForcesFrame(r);if(k==now)Fill(new Rect(r.x,r.y,r.width,3),UiGold);
   float x=r.x+16,iw=r.width-32,y=r.y+12;
   FitLabel(new Rect(x,y,iw,26),StatecraftRules.Names[k]+(k==now?settling?"  (taking effect)":"  (now)":""),head,12);y+=28;
   y+=Paragraph(new Rect(x,y,iw,0),StatecraftRules.Mottos[k],new GUIStyle(text){fontStyle=FontStyle.Italic})+8;
   foreach(string line in StatecraftEffects(k))y+=Paragraph(new Rect(x,y,iw,0),"•  "+line,text)+3;
   // Your own figures, coloured against how the realm stands now.
   float fy=r.y+200;Fill(new Rect(x,fy-6,iw,1),new Color(UiGold.r,UiGold.g,UiGold.b,.45f));GUI.Label(new Rect(x,fy,iw,24),"Your realm"+(k==StatecraftRules.Centralised?" ("+cities+(cities==1?" city":" cities")+": +"+f[k].bonus+"%)":""),text);fy+=22;
   var mine=f[world.StatecraftOf(0)];// what counts today: Balanced while a change settles
   Figure(x,ref fy,iw,"Science a turn",f[k].science,mine.science,true);Figure(x,ref fy,iw,"Shields a turn",f[k].shields,mine.shields,true);Figure(x,ref fy,iw,"Gold a turn, net",f[k].gold,mine.gold,true);
   Figure(x,ref fy,iw,"Service capacity",f[k].service,mine.service,true);Figure(x,ref fy,iw,"Influence a turn",f[k].influence,mine.influence,true);Figure(x,ref fy,iw,"Cost of a Settler",f[k].settler,mine.settler,false);
   string reason=world.StatecraftReason(0,k);bool on=GUI.enabled;GUI.enabled=on&&reason=="";
   if(GUI.Button(new Rect(x,r.yMax-70,iw,32),"Choose "+StatecraftRules.Names[k].ToLower()+" rule",button))statecraftAsk=k;GUI.enabled=on;
   if(reason!="")FitLabel(new Rect(x,r.yMax-34,iw,26),reason,new GUIStyle(text){alignment=TextAnchor.MiddleCenter,wordWrap=false},10);
  }
  int extra=Mathf.Max(0,cities-World.FreeCities),tech=world.SizeTechPercent(0);
  GUI.Label(new Rect(58,652,1160,44),"Whatever you choose, each city beyond the "+Ordinal(World.FreeCities)+" makes technology "+StatecraftRules.TechPerCity+"% dearer for you and for every rival of this world, up to "+StatecraftRules.TechMost+"%. "+(extra==0?"Your "+cities+(cities==1?" city costs":" cities cost")+" you nothing.":"Your "+cities+" cities make it "+tech+"% dearer."),text);
  GUI.enabled=was;if(modal)DrawStatecraftAsk(f);
 }
 static string Ordinal(int n){return n==4?"fourth":n+"th";}
 void Figure(float x,ref float y,float w,string label,int value,int now,bool moreIsBetter){
  var s=new GUIStyle(fSmall){fontSize=14,wordWrap=false,clipping=TextClipping.Overflow,padding=new RectOffset(0,0,0,0),margin=new RectOffset(0,0,0,0)};GUI.Label(new Rect(x,y,w*.6f,22),label,s);
  var v=new GUIStyle(s){alignment=TextAnchor.UpperRight};int d=value-now;if(d!=0)v.normal.textColor=d>0==moreIsBetter?ForceGood:ForceBad;
  GUI.Label(new Rect(x+w*.5f,y,w*.5f,22),value+(d!=0?"  ("+(d>0?"+":"")+d+")":""),v);y+=20;}
 void DrawStatecraftAsk(World.StatecraftFigures[] f){
  Fill(new Rect(24,100,1232,630),new Color(0,0,0,.6f));var box=new Rect(390,290,500,220);GUI.Box(box,"");ForcesFrame(box);int k=statecraftAsk;
  Paragraph(new Rect(box.x+20,box.y+16,box.width-40,0),"Turn to "+StatecraftRules.Names[k].ToLower()+" rule?\n\nFor "+StatecraftRules.SettleTurns+" turns neither the old way nor the new one counts, and every city has one citizen fewer content. Then it takes effect. You cannot change again for "+StatecraftRules.MinTurns+" turns.",new GUIStyle(fSmall){fontSize=14});
  if(GUI.Button(new Rect(box.x+20,box.yMax-50,220,34),"Yes, change",button)){int choice=statecraftAsk;statecraftAsk=-1;statecraftShown=null;status=world.SetStatecraft(0,choice)?"Your realm turns to "+StatecraftRules.Names[choice].ToLower()+" rule.":world.StatecraftReason(0,choice);GUIUtility.ExitGUI();}
  if(GUI.Button(new Rect(box.xMax-240,box.yMax-50,220,34),"Keep as it is",button)){statecraftAsk=-1;GUIUtility.ExitGUI();}
 }
}
}
