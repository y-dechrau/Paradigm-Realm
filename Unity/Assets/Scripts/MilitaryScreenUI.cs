using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Military > Forces drawn after the owner's picture (27 Nov 2026): a painting of an army camp under the title with the Forces / Units
// tabs and Back, four stat panels (service capacity, arms output, horse support, military upkeep), the peacetime or mobilised line with
// Mobilise…, and three framed columns: Formations (list, then the painting of soldiers and the help or the chosen formation's
// actions), Raise (deploy city, unit cards with Raise and the reason in red when blocked) and Programme (n / 8 orders, each with its
// progress bar, City ▸ and up, down, cancel). The picture with the changing words cleared is Resources/UI/Military/military-screen
// (picture rows 48-941; the game's own top bar stands above it). Everything is placed in the picture's 1672 x 941 pixels (MilMatrix).
// The rules, reasons and tips are those of ForcesUI.cs; only the look changed.
public partial class RealmGame {
 const float MilW=1672,MilH=941;
 GUIStyle milText,milSmall,milRed,milRight;Texture2D milTri;
 Matrix4x4 MilMatrix(){float s=Mathf.Min(MapW/MilW,800/MilH);return wideMatrix*Matrix4x4.TRS(new Vector3((MapW-MilW*s)/2,0,0),Quaternion.identity,new Vector3(s,s,1));}
 void MilStyles(){
  SetupStyles();if(milText!=null)return;
  milText=new GUIStyle(setText){fontSize=18,wordWrap=false,clipping=TextClipping.Clip};milSmall=new GUIStyle(setText){fontSize=16,wordWrap=false,clipping=TextClipping.Clip};
  milRed=new GUIStyle(milSmall){wordWrap=true};milRed.normal.textColor=milRed.hover.textColor=new Color(1f,.5f,.36f);
  milRight=new GUIStyle(milSmall){alignment=TextAnchor.UpperRight};
  milTri=new Texture2D(7,9,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};for(int y=0;y<9;y++)for(int x=0;x<7;x++)milTri.SetPixel(x,y,x<=(4-Mathf.Abs(y-4))*1.5f?Color.white:new Color(1,1,1,0));milTri.Apply();
 }
 void MilFrame(Rect r,bool picked=false){Line(r,picked?new Color(.9f,.74f,.42f):new Color(.62f,.48f,.27f));Line(new Rect(r.x+1.5f,r.y+1.5f,r.width-3,r.height-3),picked?new Color(.1f,.13f,.2f):new Color(.04f,.07f,.13f));}
 // A button from the picture: navy with a gold edge, teal when it is the one to press; a small right-pointing triangle after its word.
 bool MilButton(Rect r,string text,bool teal,bool arrow=false){
  bool hit=SetupOption(r,arrow?text+"    ":text,teal);
  if(arrow&&Event.current.type==EventType.Repaint){var s=new GUIStyle(setOption){fontSize=setOption.fontSize};float tw=s.CalcSize(new GUIContent(text+"    ")).x;var c=GUI.color;GUI.color=GUI.enabled?new Color(.96f,.95f,.9f):new Color(.6f,.6f,.62f);GUI.DrawTexture(new Rect(r.center.x+tw/2-12,r.center.y-4.5f,7,9),milTri);GUI.color=c;}
  if(!GUI.enabled)Line(new Rect(r.x+2,r.y+2,r.width-4,r.height-4),new Color(.02f,.04f,.09f,.45f));
  return hit;}
 bool MilGlyph(Rect r,int kind){
  bool hit=SetupOption(r,"",false);var ink=GUI.enabled?new Color(.97f,.95f,.9f):new Color(.5f,.5f,.52f);float cx=Mathf.Round(r.center.x),cy=Mathf.Round(r.center.y);
  if(kind<2)for(int i=0;i<5;i++){float row=kind==0?cy-2.5f+i:cy+2.5f-i;Line(new Rect(cx-i-.5f,row,2*i+1,1.2f),ink);}
  else for(int i=-4;i<=4;i++){Line(new Rect(cx+i-1,cy+i-1,2.4f,2.4f),ink);Line(new Rect(cx+i-1,cy-i-1,2.4f,2.4f),ink);}
  return hit;}
 void DrawForcesScreen(){
  ForcesStyles();MilStyles();forcesTip=null;
  Wide();float sc=Mathf.Min(MapW/MilW,800/MilH);Fill(new Rect(0,46*sc,MapW,800-46*sc),new Color(.03f,.05f,.1f));
  GUI.matrix=MilMatrix();var e=Event.current;var m=e.mousePosition;
  var bg=Resources.Load<Texture2D>("UI/Military/military-screen");if(bg!=null)GUI.DrawTexture(new Rect(0,48,MilW,893),bg,ScaleMode.StretchToFill,false);
  // Back and the Units tab are the picture's own buttons.
  bool backHit=SetupBig(new Rect(1378,88,214,36),""),unitsHit=SetupBig(new Rect(384,144,266,34),"");
  if(backHit){Centred();detailsScreen="";GUIUtility.ExitGUI();}
  if(unitsHit){Centred();militaryTab=1;detailsScroll=Vector2.zero;GUIUtility.ExitGUI();}
  if(!world.nationalForces){SetupWrite(new Rect(76,236,1500,60),"This game began before national forces: soldiers are trained in cities. Start a new game to raise them nationally.",new GUIStyle(setText){fontSize=19});Centred();return;}
  bool modal=forcesConfirm!=null;bool was=GUI.enabled;if(modal)GUI.enabled=false;
  // ---- the four panels ----
  int cap=world.ServiceCapacity(0),serving=world.ServiceServing(0),reserved=world.ServiceReserved(0),free=cap-serving-reserved,short_=world.ServiceShortfall(0);
  int arms=world.ArmsOutput(0),support=world.HorseSupport(0),horses=world.HorsesCommitted(0);var budget=world.BudgetDetail(0);
  var panels=new[]{new Rect(60,200,400,108),new Rect(470,200,375,108),new Rect(852,200,388,108),new Rect(1247,200,383,108)};
  {var r=panels[0];SetupCapsFit(76,234,(serving+reserved)+" / "+cap,28,370);
   var bar=new Rect(76,268,372,11);Line(bar,new Color(.08f,.11f,.2f));float k=bar.width/Math.Max(1,Math.Max(cap,serving+reserved));Line(new Rect(bar.x,bar.y,serving*k,bar.height),new Color(.47f,.62f,.95f));Line(new Rect(bar.x+serving*k,bar.y,reserved*k,bar.height),new Color(.62f,.75f,1f));
   SetupWrite(new Rect(76,282,380,20),serving+" serving · "+reserved+" reserved · "+(short_>0?short_+" OVER":Math.Max(0,free)+" free"),short_>0?new GUIStyle(milSmall){normal={textColor=ForceBad}}:milSmall,11);
   if(r.Contains(m))forcesTip=world.ServiceText(0)+".\nServing: formations on the map. Reserved: approved orders, held until they finish."+(short_>0?"\nOver capacity by "+short_+": costs "+short_*ForcesRules.ShortfallGold+" gold a turn, and orders that add personnel are blocked. Disband or grow your cities to recover.":"");}
  {var r=panels[1];var list=world.ForceOrders(0);SetupCapsFit(493,234,"+"+arms+" a turn",28,340);
   SetupWrite(new Rect(493,276,340,20),list.Count==0?"Idle: no orders":"Working on: "+OrderTitle(list[0]),milSmall,11);
   if(r.Contains(m))forcesTip="Arms output each turn:\n"+string.Join("\n",world.ArmsLines(0).Select(l=>"  "+l.Key+"   +"+l.Value).ToArray())+"\nWorkshops, Factories and Barracks add to it; the Empire screen's military priority turns trade into arms while the programme has work.";}
  {var r=panels[2];int src=world.HorseSources(0);SetupCapsFit(875,234,horses+" / "+support,28,350);
   SetupWrite(new Rect(875,276,355,20),src==0?"No connected horse pasture":src+" source"+(src==1?"":"s")+" x "+ForcesRules.HorsesPerSource+" mounted each",milSmall,11);
   if(r.Contains(m))forcesTip="Each Horses resource with a pasture (farm), joined to your capital by road, or bought by treaty, keeps "+ForcesRules.HorsesPerSource+" mounted formations in the field. Horsemen and Knights use it; a refit from one to the other keeps its slot."+(horses>support?"\nYou have lost support: "+(horses-support)+" mounted formations stay, but no more can be raised.":"");}
  {var r=panels[3];int upkeep=budget.army+budget.forceShortfall;SetupCapsFit(1270,234,upkeep==0?"No upkeep":"-"+upkeep+" gold a turn",28,350);
   SetupWrite(new Rect(1271,276,350,20),"Net income "+(budget.Net>=0?"+":"")+budget.Net+" a turn",budget.Net<0?new GUIStyle(milSmall){normal={textColor=ForceBad}}:milSmall,11);
   if(r.Contains(m))forcesTip="Soldiers and warships "+budget.army+" (half price inside your borders; your capital's best guard is free)"+(budget.forceShortfall>0?"\nPersonnel over capacity "+budget.forceShortfall:"")+".\nUpkeep starts the turn a formation stands on the map, not while it is being equipped.";}
  // ---- policy ----
  {var n=world.nations[0];
   SetupWrite(new Rect(76,327,1250,26),n.mobilised?"Mobilised: service capacity "+cap+" (all citizens), every city gives up "+ForcesRules.MobiliseShieldPercent+"% of its shields":"Peacetime service: capacity "+cap+" ("+ForcesRules.PeacePercent+"% of citizens)",new GUIStyle(setText){fontSize=19,wordWrap=false},12);
   string why=world.MobiliseReason(0,!n.mobilised);bool w2=GUI.enabled;GUI.enabled=w2&&why=="";var mb=new Rect(1355,322,260,35);
   if(MilButton(mb,n.mobilised?"Demobilise…":"Mobilise…",true))AskMobilise(!n.mobilised);GUI.enabled=w2;
   if(why!=""&&mb.Contains(m))forcesTip=why;}
  MilFormations();MilRaise();MilProgramme();
  GUI.enabled=was;
  if(modal)MilConfirm();
  if(forcesTip!=null&&!modal&&e.type==EventType.Repaint){var ts=new GUIStyle(setText){fontSize=17,wordWrap=true};float w=430,h=ts.CalcHeight(new GUIContent(forcesTip),w-24)+16;var box=new Rect(Mathf.Clamp(m.x+18,30,MilW-30-w),Mathf.Clamp(m.y+18,60,MilH-30-h),w,h);
   Line(box,new Color(.72f,.56f,.3f));Line(new Rect(box.x+1.5f,box.y+1.5f,box.width-3,box.height-3),new Color(.03f,.05f,.11f,.98f));GUI.Label(new Rect(box.x+12,box.y+8,w-24,h-16),forcesTip,ts);}
  Centred();
 }
 // Formations: the list (one row as in the picture, more push down over the painting), then the help or the chosen formation's actions.
 void MilFormations(){
  var m=Event.current.mousePosition;
  var own=world.units.Where(u=>u.owner==0&&ForcesRules.Formation(u.type)).OrderBy(u=>u.type).ThenBy(u=>u.id).ToList();
  SetupWrite(new Rect(400,388,142,20),own.Count+" in service",milRight,11);
  const float Row=64;float shown=Mathf.Clamp(own.Count,1,5)*Row-4;var view=new Rect(55,430,496,shown);
  if(own.Count>1)Line(new Rect(50,428,503,shown+6),new Color(.035f,.06f,.11f));
  if(own.Count==0)SetupWrite(new Rect(66,446,470,26),"No formations yet. Raise one from the middle column.",new GUIStyle(setText){fontSize=17});
  forcesUnitScroll=GUI.BeginScrollView(view,forcesUnitScroll,new Rect(0,0,view.width-(own.Count>5?18:0),Math.Max(view.height,own.Count*Row-4)));var lm=Event.current.mousePosition;
  for(int i=0;i<own.Count;i++){var u=own[i];var r=new Rect(2,i*Row,view.width-(own.Count>5?22:4),Row-6);bool pick=u.id==forcesPick;MilFrame(r,pick||r.Contains(lm));
   GUI.DrawTexture(new Rect(r.x+12,r.y+6,44,44),people[u.type],ScaleMode.ScaleToFit,true);
   var city=world.cities.Find(c=>c.x==u.x&&c.y==u.y);var ord=world.ForceOrders(0).FirstOrDefault(o=>o.unit==u.id);
   SetupWrite(new Rect(r.x+72,r.y+6,r.width-80,22),world.UnitName(u)+(u.promotion>0?(u.promotion>1?"  ★★":"  ★"):""),milText,11);
   SetupWrite(new Rect(r.x+72,r.y+30,r.width-80,20),FormationRole(u.type)+" · "+(ord!=null?(ord.kind==1?"refitting":"taking replacements"):city!=null?"in "+city.name:"in the field")+" · "+u.hp+"/10 · upkeep "+World.Upkeep[u.type],milSmall,11);
   if(GUI.Button(r,GUIContent.none,GUIStyle.none))forcesPick=u.id;}
  GUI.EndScrollView();
  // Under the painting: what to do, or the chosen formation's actions.
  var pu=own.Find(u=>u.id==forcesPick);
  if(pu==null){SetupWrite(new Rect(66,780,476,60),own.Count==0?"Raise a formation from the middle column; it appears here once it stands on the map.":"Choose a formation to go to it, refit it, replace its losses or disband it.",new GUIStyle(setText){fontSize=18});return;}
  SetupWrite(new Rect(66,775,476,22),world.UnitName(pu)+" ("+World.UnitNames[pu.type]+", "+pu.victories+" victories)",milText,11);
  int target=World.UpgradeOf(pu.type);string refitWhy=world.RefitReason(0,pu),repWhy=world.ReplaceReason(0,pu);
  string refitLine=target<0?"No refit available for a "+World.UnitNames[pu.type]+".":"Refit to "+World.UnitNames[target]+": attack "+World.Attack[pu.type]+"→"+World.Attack[target]+", defence "+World.Defence[pu.type]+"→"+World.Defence[target]+", moves "+World.Moves[pu.type]+"→"+World.Moves[target]+(World.IsRanged(pu.type)&&!World.IsRanged(target)?"; gives up ranged attack":"")+(World.Moves[target]<World.Moves[pu.type]?"; slower":"")+". "+world.RefitCost(pu,target)+" arms"+(ForcesRules.Service[target]>ForcesRules.Service[pu.type]?", +"+(ForcesRules.Service[target]-ForcesRules.Service[pu.type])+" service":"")+", upkeep "+World.Upkeep[pu.type]+"→"+World.Upkeep[target]+"."+(refitWhy!=""?"  "+refitWhy:"");
  SetupWrite(new Rect(66,798,476,38),refitLine,new GUIStyle(setText){fontSize=15},10);
  float bw=(476-18)/4f;bool w=GUI.enabled;var bGo=new Rect(66,840,bw,30);var bRefit=new Rect(66+bw+6,840,bw,30);var bRep=new Rect(66+2*(bw+6),840,bw,30);var bDis=new Rect(66+3*(bw+6),840,bw,30);
  if(MilButton(bGo,"Go to",false)){Centred();selected=pu.id;cityId=-1;detailsScreen="";cityScreen=false;Center();GUIUtility.ExitGUI();}
  GUI.enabled=w&&refitWhy=="";if(MilButton(bRefit,"Refit",false)){var why=world.Refit(0,pu);status=why==""?World.UnitNames[pu.type]+" refit ordered; it cannot move or fight until it is done.":why;}
  GUI.enabled=w&&repWhy=="";if(MilButton(bRep,"Replace",false)){var why=world.Replace(0,pu);status=why==""?"Replacements ordered: "+world.ReplaceCost(pu)+" arms; it stays in the city until they arrive.":why;}
  GUI.enabled=w;if(MilButton(bDis,"Disband",false)){var victim=pu;Ask("Disband "+world.UnitName(victim)+"? It frees "+ForcesRules.Service[victim.type]+" service"+(ForcesRules.Mounted(victim.type)?" and 1 horse support":"")+" and saves "+World.Upkeep[victim.type]+" upkeep. Its equipment and its "+victim.victories+" victories are lost; nothing is refunded.",()=>{world.Disband(0,victim);forcesPick=-1;status="Disbanded.";});}
  if(bRep.Contains(m))forcesTip=repWhy!=""?repWhy:"Restore it to full strength for "+world.ReplaceCost(pu)+" arms (half the new price per lost point). It also heals as usual while it waits; replacements simply make it whole at once.";
  if(bRefit.Contains(m)&&refitWhy!="")forcesTip=refitWhy;
 }
 void MilRaise(){
  var mine=world.cities.Where(c=>c.owner==0).OrderBy(c=>c.id).ToList();var land=world.DeployCity(0,2);var sea=world.DeployCity(0,14);
  SetupWrite(new Rect(760,388,318,18),"Deploy at: "+(land!=null?land.name:"no city")+(sea!=null?"; warships: "+sea.name:""),new GUIStyle(milRight){fontSize=15},10);
  {bool w=GUI.enabled;GUI.enabled=w&&mine.Count>1;if(MilButton(new Rect(883,412,194,28),"Change city",false,true)){int at=land!=null?mine.IndexOf(land):-1;world.nations[0].landDeploy=mine[(at+1)%mine.Count].id;}GUI.enabled=w;}
  var types=Enumerable.Range(0,World.UnitNames.Length).Where(t=>ForcesRules.Formation(t)&&world.FormationKnown(0,t)&&!world.IsObsolete(0,t)).ToList();
  var view=new Rect(588,448,494,430);const float Card=112;bool scroll=types.Count*Card>view.height;
  forcesRaiseScroll=GUI.BeginScrollView(view,forcesRaiseScroll,new Rect(0,0,view.width-(scroll?18:0),Math.Max(view.height,types.Count*Card)));
  int arms=world.ArmsOutput(0);
  for(int i=0;i<types.Count;i++){int t=types[i];var r=new Rect(2,i*Card+2,view.width-(scroll?22:4),Card-8);MilFrame(r);var at=world.DeployCity(0,t);string why=world.RaiseReason(0,t,at);
   GUI.DrawTexture(new Rect(r.x+8,r.y+10,52,52),people[t],ScaleMode.ScaleToFit,true);
   SetupWrite(new Rect(r.x+72,r.y+6,r.width-210,22),World.UnitNames[t]+"  ·  "+FormationRole(t),milText,11);
   int cost=at!=null?world.CostFor(at,World.UnitBuild(t)):World.UnitCosts[t];int list=world.ForceOrders(0).Sum(o=>Math.Max(0,o.cost-o.paid));int turns=arms>0?(list+cost+arms-1)/arms:-1;
   SetupWrite(new Rect(r.x+72,r.y+29,r.width-210,20),"Attack "+World.Attack[t]+" · Defence "+World.Defence[t]+" · Moves "+World.Moves[t],milSmall,11);
   SetupWrite(new Rect(r.x+72,r.y+51,r.width-80,20),ForcesRules.Service[t]+" service · "+cost+" arms · upkeep "+World.Upkeep[t]+(ForcesRules.Mounted(t)?" · 1 horse":"")+" · "+(turns>0?"~"+turns+" turns":"no arms output"),milSmall,11);
   if(why!="")SetupWrite(new Rect(r.x+72,r.y+72,r.width-80,30),why,new GUIStyle(milRed){fontSize=15},10);
   bool w=GUI.enabled;GUI.enabled=w&&why=="";
   if(MilButton(new Rect(r.xMax-127,r.y+6,115,33),"Raise",false)){var res=world.Raise(0,t,at);status=res==""?World.UnitNames[t]+" ordered for "+at.name+": "+ForcesRules.Service[t]+" service reserved now; upkeep starts when it stands on the map.":res;}
   GUI.enabled=w;
   if(r.Contains(Event.current.mousePosition))forcesTip=World.UnitNames[t]+" ("+FormationRole(t)+").\nPersonnel: "+ForcesRules.Service[t]+" service point"+(ForcesRules.Service[t]==1?"":"s")+", reserved when you approve.\nEquipment: "+cost+" arms from national output (+"+arms+" a turn), after the orders above it.\nUpkeep: about "+(World.Upkeep[t]+1)/2+"-"+World.Upkeep[t]+" gold a turn (less at home) from the turn it is deployed; national income "+(world.BudgetDetail(0).Net>=0?"+":"")+world.BudgetDetail(0).Net+" now, about "+(world.BudgetDetail(0).Net-(World.Upkeep[t]+1)/2)+" after."+(ForcesRules.Mounted(t)?"\nHorse support: 1 slot while it serves.":"")+(why!=""?"\n"+why:"");
  }
  GUI.EndScrollView();
 }
 void MilProgramme(){
  var list=world.ForceOrders(0);var turns=world.OrderTurns(0);int arms=world.ArmsOutput(0);
  SetupWrite(new Rect(1470,388,140,20),list.Count+" / "+ForcesRules.MaxOrders+" orders",milRight,11);
  if(list.Count==0){SetupWrite(new Rect(1127,426,480,80),"Nothing ordered. Arms output (+"+arms+" a turn) waits for orders: raise a formation, or refit or reinforce one from the left.",new GUIStyle(setText){fontSize=18});return;}
  float top=416;if(arms<=0){SetupWrite(new Rect(1127,418,480,40),"Arms output is 0, so nothing progresses: build a Workshop, connect Iron or raise the military priority on the Overview tab.",new GUIStyle(milRed){fontSize=15},10);top=460;}
  var view=new Rect(1112,top,510,878-top);const float Row=90;bool scroll=list.Count*Row>view.height;
  forcesOrderScroll=GUI.BeginScrollView(view,forcesOrderScroll,new Rect(0,0,view.width-(scroll?18:0),Math.Max(view.height,list.Count*Row)));
  float rw=view.width-(scroll?22:8);
  for(int i=0;i<list.Count;i++){var o=list[i];float y=i*Row;
   if(i<list.Count-1)Line(new Rect(4,y+Row-3,rw-4,1.2f),new Color(.62f,.48f,.27f,.8f));
   SetupWrite(new Rect(15,y+6,rw-120,22),(i==0?"TOP PRIORITY  ":(i+1)+".  ")+OrderTitle(o),milText,11);
   var city=world.cities.Find(c=>c.id==o.city);
   {var bar=new Rect(15,y+33,353,10);Line(bar,new Color(.02f,.03f,.07f));Line(new Rect(bar.x,bar.y,bar.width*Mathf.Clamp01(o.paid/(float)Math.Max(1,o.cost)),bar.height),o.kind==0?new Color(.97f,.62f,.3f):new Color(.55f,.62f,.85f));}
   string when=o.held?"Ready: held until "+(city==null||city.owner!=0?"you choose a city":city.name+" has room"):turns[i]>0?(turns[i]==1?"next turn":"in "+turns[i]+" turns"):"stopped (no arms output)";
   SetupWrite(new Rect(15,y+49,280,20),o.paid+" / "+o.cost+" arms · "+when,o.held?new GUIStyle(milSmall){normal={textColor=ForceBad}}:milSmall,11);
   SetupWrite(new Rect(15,y+69,280,20),(o.kind==0?"to "+(city!=null?city.name:"?"):"")+(o.service>0?" · "+o.service+" service reserved":"")+(o.horses>0?" · 1 horse":""),milSmall,11);
   bool w=GUI.enabled;
   if(o.kind==0&&MilButton(new Rect(300,y+57,85,27),"City",false,true)){var mine=world.cities.Where(c=>c.owner==0&&world.DeployReason(c,o.type)=="").OrderBy(c=>c.id).ToList();if(mine.Count>0){int at=mine.FindIndex(c=>c.id==o.city);var next=mine[(at+1)%mine.Count];var res=world.Redeploy(0,o.id,next);status=res==""?"Will deploy at "+next.name+".":res;}}
   GUI.enabled=w&&i>0;if(MilGlyph(new Rect(388,y+7,26,24),0))world.MoveForceOrder(0,i,-1);
   GUI.enabled=w&&i<list.Count-1;if(MilGlyph(new Rect(420,y+7,26,24),1))world.MoveForceOrder(0,i,1);
   GUI.enabled=w;if(MilGlyph(new Rect(453,y+7,26,24),2)){var cancel=o;Ask("Cancel "+OrderTitle(cancel)+"? Its "+cancel.service+" reserved service"+(cancel.horses>0?" and horse support are":" is")+" released. The "+cancel.paid+" arms already spent on it are lost.",()=>{world.CancelForceOrder(0,cancel.id);status="Order cancelled.";});}
  }
  GUI.EndScrollView();
 }
 void MilConfirm(){
  Line(new Rect(0,48,MilW,893),new Color(0,0,0,.55f));var box=new Rect(536,330,600,260);MilFrame(box,true);
  SetupWrite(new Rect(box.x+24,box.y+20,box.width-48,170),forcesConfirm,new GUIStyle(setText){fontSize=18},11);
  if(MilButton(new Rect(box.x+24,box.yMax-56,260,36),"Confirm",true)){var act=forcesConfirmDo;forcesConfirm=null;forcesConfirmDo=null;if(act!=null)act();Centred();GUIUtility.ExitGUI();}
  if(MilButton(new Rect(box.xMax-284,box.yMax-56,260,36),"Keep as it is",false)){forcesConfirm=null;forcesConfirmDo=null;Centred();GUIUtility.ExitGUI();}
 }
}
}
