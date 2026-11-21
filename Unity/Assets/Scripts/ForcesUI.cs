using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Military > Forces (owner brief, 20 Nov 2026; moved from the Empire screen to its own Military screen on 21 Nov). Answers four questions at a glance:
//  1. what forces you have and what they cost each turn (the meters and the Formations column);
//  2. what you can raise now and exactly what stops the rest (the Raise column; every blocked card says why);
//  3. what military industry is working on and when it finishes (the Programme column, top priority first);
//  4. what mobilisation or a refit would change (the policy preview and each formation's refit line).
public partial class RealmGame {
 int militaryTab;// Military screen: 0 Forces, 1 Units (the old units list)
 int forcesPick=-1;Vector2 forcesUnitScroll,forcesRaiseScroll,forcesOrderScroll;string forcesConfirm;Action forcesConfirmDo;string forcesTip;
 static readonly Color ForceCell=new Color(.06f,.09f,.17f,.9f),ForceBad=new Color(1f,.6f,.45f),ForceGood=new Color(.55f,.88f,.6f);
 GUIStyle fSmall,fTiny,fBig,fHead;
 void ForcesStyles(){if(fSmall!=null)return;fSmall=new GUIStyle(small){wordWrap=true};fTiny=new GUIStyle(small){fontSize=11,wordWrap=true};fBig=new GUIStyle(heading){fontSize=20,wordWrap=false,clipping=TextClipping.Overflow};fHead=new GUIStyle(heading){wordWrap=false};}
 static string FormationRole(int t){return World.IsNaval(t)?"Warship":ForcesRules.Mounted(t)?"Mounted":t==9?"Siege":World.IsRanged(t)?"Ranged":"Infantry";}
 void ForcesFrame(Rect r){Fill(r,ForceCell);Fill(new Rect(r.x,r.y,r.width,1),new Color(UiGold.r,UiGold.g,UiGold.b,.7f));}
 void DrawForcesScreen(){
  ForcesStyles();forcesTip=null;var e=Event.current;var m=e.mousePosition;
  GUI.Box(new Rect(24,100,1232,630),"");GUI.Label(new Rect(50,115,800,40),"Military",title);
  if(GUI.Button(new Rect(1040,115,190,34),"Back",button)){detailsScreen="";GUIUtility.ExitGUI();}
  GUI.Button(new Rect(55,158,220,30),"▸ Forces",button);
  if(GUI.Button(new Rect(281,158,220,30),"Units",button)){militaryTab=1;detailsScroll=Vector2.zero;GUIUtility.ExitGUI();}
  if(!world.nationalForces){GUI.Label(new Rect(60,210,1100,60),"This game began before national forces: soldiers are trained in cities. Start a new game to raise them nationally.",body);return;}
  bool modal=forcesConfirm!=null;bool was=GUI.enabled;if(modal)GUI.enabled=false;
  // ---- meters ----
  int cap=world.ServiceCapacity(0),serving=world.ServiceServing(0),reserved=world.ServiceReserved(0),free=cap-serving-reserved,short_=world.ServiceShortfall(0);
  int arms=world.ArmsOutput(0),support=world.HorseSupport(0),horses=world.HorsesCommitted(0);var budget=world.BudgetDetail(0);
  float mw=(1170-18)/4f;var meters=new[]{new Rect(55,198,mw,80),new Rect(55+mw+6,198,mw,80),new Rect(55+2*(mw+6),198,mw,80),new Rect(55+3*(mw+6),198,mw,80)};
  foreach(var r in meters)ForcesFrame(r);
  {var r=meters[0];GUI.Label(new Rect(r.x+10,r.y+4,r.width-20,22),"Service capacity",fHead);GUI.Label(new Rect(r.x+10,r.y+24,r.width-20,26),(serving+reserved)+" / "+cap,fBig);
   var bar=new Rect(r.x+10,r.y+52,r.width-20,8);Fill(bar,UiTrough);float k=bar.width/Math.Max(1,Math.Max(cap,serving+reserved));Fill(new Rect(bar.x,bar.y,serving*k,8),new Color(.4f,.6f,.95f));Fill(new Rect(bar.x+serving*k,bar.y,reserved*k,8),new Color(.6f,.75f,1f,.6f));
   GUI.Label(new Rect(r.x+10,r.y+58,r.width-20,20),serving+" serving · "+reserved+" reserved · "+(short_>0?short_+" OVER":Math.Max(0,free)+" free"),new GUIStyle(fTiny){normal={textColor=short_>0?ForceBad:fTiny.normal.textColor}});
   if(r.Contains(m))forcesTip=world.ServiceText(0)+".\nServing: formations on the map. Reserved: approved orders, held until they finish."+(short_>0?"\nOver capacity by "+short_+": costs "+short_*ForcesRules.ShortfallGold+" gold a turn, and orders that add personnel are blocked. Disband or grow your cities to recover.":"");}
  {var r=meters[1];GUI.Label(new Rect(r.x+10,r.y+4,r.width-20,22),"Arms output",fHead);GUI.Label(new Rect(r.x+10,r.y+24,r.width-20,26),"+"+arms+" a turn",fBig);
   var list=world.ForceOrders(0);GUI.Label(new Rect(r.x+10,r.y+52,r.width-20,20),list.Count==0?"Idle: no orders":"Working on: "+OrderTitle(list[0]),new GUIStyle(fTiny){wordWrap=false,clipping=TextClipping.Clip});
   if(r.Contains(m))forcesTip="Arms output each turn:\n"+string.Join("\n",world.ArmsLines(0).Select(l=>"  "+l.Key+"   +"+l.Value).ToArray())+"\nWorkshops, Factories and Barracks add to it; the Empire screen's military priority turns trade into arms while the programme has work.";}
  {var r=meters[2];GUI.Label(new Rect(r.x+10,r.y+4,r.width-20,22),"Horse support",fHead);GUI.Label(new Rect(r.x+10,r.y+24,r.width-20,26),horses+" / "+support,fBig);int src=world.HorseSources(0);
   GUI.Label(new Rect(r.x+10,r.y+52,r.width-20,20),src==0?"No connected horse pasture":src+" source"+(src==1?"":"s")+" x "+ForcesRules.HorsesPerSource+" mounted each",fTiny);
   if(r.Contains(m))forcesTip="Each Horses resource with a pasture (farm), joined to your capital by road, or bought by treaty, keeps "+ForcesRules.HorsesPerSource+" mounted formations in the field. Horsemen and Knights use it; a refit from one to the other keeps its slot."+(horses>support?"\nYou have lost support: "+(horses-support)+" mounted formations stay, but no more can be raised.":"");}
  {var r=meters[3];int upkeep=budget.army+budget.forceShortfall;GUI.Label(new Rect(r.x+10,r.y+4,r.width-20,22),"Military upkeep",fHead);GUI.Label(new Rect(r.x+10,r.y+24,r.width-20,26),upkeep==0?"No upkeep":"-"+upkeep+" gold a turn",fBig);
   GUI.Label(new Rect(r.x+10,r.y+52,r.width-20,20),"Net income "+(budget.Net>=0?"+":"")+budget.Net+" a turn",new GUIStyle(fTiny){normal={textColor=budget.Net<0?ForceBad:fTiny.normal.textColor}});
   if(r.Contains(m))forcesTip="Soldiers and warships "+budget.army+" (half price inside your borders; your capital's best guard is free)"+(budget.forceShortfall>0?"\nPersonnel over capacity "+budget.forceShortfall:"")+".\nUpkeep starts the turn a formation stands on the map, not while it is being equipped.";}
  // ---- policy ----
  {var n=world.nations[0];var r=new Rect(55,284,1170,36);ForcesFrame(r);
   GUI.Label(new Rect(r.x+10,r.y+7,700,22),n.mobilised?"Mobilised: service capacity "+cap+" (all citizens), every city gives up "+ForcesRules.MobiliseShieldPercent+"% of its shields":"Peacetime service: capacity "+cap+" ("+ForcesRules.PeacePercent+"% of citizens)",fSmall);
   string why=world.MobiliseReason(0,!n.mobilised);bool w2=GUI.enabled;GUI.enabled=w2&&why=="";
   if(GUI.Button(new Rect(r.xMax-210,r.y+4,200,28),n.mobilised?"Demobilise…":"Mobilise…",button))AskMobilise(!n.mobilised);GUI.enabled=w2;
   if(why!=""&&new Rect(r.xMax-210,r.y+4,200,28).Contains(m))forcesTip=why;}
  // ---- columns ----
  var colA=new Rect(55,328,384,394);var colB=new Rect(447,328,384,394);var colC=new Rect(839,328,386,394);
  DrawFormationColumn(colA);DrawRaiseColumn(colB);DrawProgrammeColumn(colC);
  GUI.enabled=was;
  if(modal)DrawForcesConfirm();
  if(forcesTip!=null&&!modal&&e.type==EventType.Repaint){var ts=new GUIStyle(small){wordWrap=true};float w=360,h=ts.CalcHeight(new GUIContent(forcesTip),w-20)+12;var box=new Rect(Mathf.Clamp(m.x+16,30,1250-w),Mathf.Clamp(m.y+16,110,725-h),w,h);Fill(box,new Color(.04f,.06f,.13f,.97f));Fill(new Rect(box.x,box.y,box.width,1),UiGold);GUI.Label(new Rect(box.x+10,box.y+6,w-20,h-12),forcesTip,ts);}
 }
 string OrderTitle(ForceOrder o){if(o.kind==0)return "Raise "+World.UnitNames[o.type];var u=world.units.Find(x=>x.id==o.unit);string who=u!=null?world.UnitName(u):"formation";return o.kind==1?"Refit "+who+" to "+World.UnitNames[o.type]:"Replace losses: "+who;}
 void DrawFormationColumn(Rect col){
  var m=Event.current.mousePosition;GUI.Label(new Rect(col.x,col.y,col.width,22),"Formations",fHead);
  var own=world.units.Where(u=>u.owner==0&&ForcesRules.Formation(u.type)).OrderBy(u=>u.type).ThenBy(u=>u.id).ToList();
  GUI.Label(new Rect(col.x,col.y,col.width,22),own.Count+" in service",new GUIStyle(fTiny){alignment=TextAnchor.MiddleRight});
  var view=new Rect(col.x,col.y+26,col.width,230);const float Row=40;
  forcesUnitScroll=GUI.BeginScrollView(view,forcesUnitScroll,new Rect(0,0,view.width-18,Math.Max(view.height,own.Count*Row)));var lm=Event.current.mousePosition;
  for(int i=0;i<own.Count;i++){var u=own[i];var r=new Rect(0,i*Row,view.width-20,Row-3);bool pick=u.id==forcesPick;Fill(r,pick?new Color(UiGold.r,UiGold.g,UiGold.b,.22f):r.Contains(lm)?new Color(1,1,1,.06f):ForceCell);
   GUI.DrawTexture(new Rect(r.x+2,r.y+1,34,34),people[u.type],ScaleMode.ScaleToFit,true);
   var city=world.cities.Find(c=>c.x==u.x&&c.y==u.y);var ord=world.ForceOrders(0).FirstOrDefault(o=>o.unit==u.id);
   GUI.Label(new Rect(r.x+40,r.y+1,r.width-44,18),world.UnitName(u)+(u.promotion>0?(u.promotion>1?"  ★★":"  ★"):""),new GUIStyle(fSmall){wordWrap=false,clipping=TextClipping.Clip});
   GUI.Label(new Rect(r.x+40,r.y+18,r.width-44,18),FormationRole(u.type)+" · "+(ord!=null?(ord.kind==1?"refitting":"taking replacements"):city!=null?"in "+city.name:"in the field")+" · "+u.hp+"/10 · upkeep "+World.Upkeep[u.type],new GUIStyle(fTiny){wordWrap=false,clipping=TextClipping.Clip});
   if(GUI.Button(r,GUIContent.none,GUIStyle.none))forcesPick=u.id;}
  GUI.EndScrollView();
  // The chosen formation's actions.
  var d=new Rect(col.x,col.y+262,col.width,138);ForcesFrame(d);var pu=own.Find(u=>u.id==forcesPick);
  if(pu==null){GUI.Label(new Rect(d.x+10,d.y+8,d.width-20,60),own.Count==0?"No formations yet. Raise one from the middle column.":"Choose a formation to go to it, refit it, replace its losses or disband it.",fSmall);return;}
  GUI.Label(new Rect(d.x+10,d.y+4,d.width-20,20),world.UnitName(pu)+" ("+World.UnitNames[pu.type]+", "+(pu.victories)+" victories)",new GUIStyle(fSmall){wordWrap=false,clipping=TextClipping.Clip});
  int target=World.UpgradeOf(pu.type);string refitWhy=world.RefitReason(0,pu),repWhy=world.ReplaceReason(0,pu);
  string refitLine=target<0?"No refit available for a "+World.UnitNames[pu.type]+".":"Refit to "+World.UnitNames[target]+": attack "+World.Attack[pu.type]+"→"+World.Attack[target]+", defence "+World.Defence[pu.type]+"→"+World.Defence[target]+", moves "+World.Moves[pu.type]+"→"+World.Moves[target]+(World.IsRanged(pu.type)&&!World.IsRanged(target)?"; gives up ranged attack":"")+(World.Moves[target]<World.Moves[pu.type]?"; slower":"")+". "+world.RefitCost(pu,target)+" arms"+(ForcesRules.Service[target]>ForcesRules.Service[pu.type]?", +"+(ForcesRules.Service[target]-ForcesRules.Service[pu.type])+" service":"")+", upkeep "+World.Upkeep[pu.type]+"→"+World.Upkeep[target]+"."+(refitWhy!=""?"  "+refitWhy:"");
  GUI.Label(new Rect(d.x+10,d.y+24,d.width-20,50),refitLine,fTiny);
  float bw=(d.width-26)/4;bool w=GUI.enabled;
  if(GUI.Button(new Rect(d.x+8,d.y+100,bw,30),"Go to",button)){selected=pu.id;cityId=-1;detailsScreen="";cityScreen=false;Center();GUIUtility.ExitGUI();}
  GUI.enabled=w&&refitWhy=="";if(GUI.Button(new Rect(d.x+12+bw,d.y+100,bw,30),"Refit",button)){var why=world.Refit(0,pu);status=why==""?World.UnitNames[pu.type]+" refit ordered; it cannot move or fight until it is done.":why;}
  GUI.enabled=w&&repWhy=="";if(GUI.Button(new Rect(d.x+16+2*bw,d.y+100,bw,30),"Replace",button)){var why=world.Replace(0,pu);status=why==""?"Replacements ordered: "+world.ReplaceCost(pu)+" arms; it stays in the city until they arrive.":why;}
  GUI.enabled=w;if(GUI.Button(new Rect(d.x+20+3*bw,d.y+100,bw,30),"Disband",button)){var victim=pu;Ask("Disband "+world.UnitName(victim)+"? It frees "+ForcesRules.Service[victim.type]+" service"+(ForcesRules.Mounted(victim.type)?" and 1 horse support":"")+" and saves "+World.Upkeep[victim.type]+" upkeep. Its equipment and its "+victim.victories+" victories are lost; nothing is refunded.",()=>{world.Disband(0,victim);forcesPick=-1;status="Disbanded.";});}
  var hover=new Rect(d.x+16+2*bw,d.y+100,bw,30);if(hover.Contains(m))forcesTip=repWhy!=""?repWhy:"Restore it to full strength for "+world.ReplaceCost(pu)+" arms (half the new price per lost point). It also heals as usual while it waits; replacements simply make it whole at once.";
  var hover2=new Rect(d.x+12+bw,d.y+100,bw,30);if(hover2.Contains(m)&&refitWhy!="")forcesTip=refitWhy;
 }
 void DrawRaiseColumn(Rect col){
  var m=Event.current.mousePosition;GUI.Label(new Rect(col.x,col.y,col.width,22),"Raise",fHead);
  // Default deployment cities: land and sea, cycled through your cities.
  var mine=world.cities.Where(c=>c.owner==0).OrderBy(c=>c.id).ToList();var land=world.DeployCity(0,2);var sea=world.DeployCity(0,14);
  GUI.Label(new Rect(col.x+60,col.y,col.width-60,22),"Deploy at: "+(land!=null?land.name:"no city")+(sea!=null?"; warships: "+sea.name:""),new GUIStyle(fTiny){alignment=TextAnchor.MiddleRight,wordWrap=false});
  if(mine.Count>1&&GUI.Button(new Rect(col.xMax-150,col.y+22,150,22),"Change city ▸",button)){int at=land!=null?mine.IndexOf(land):-1;world.nations[0].landDeploy=mine[(at+1)%mine.Count].id;}
  var types=Enumerable.Range(0,World.UnitNames.Length).Where(t=>ForcesRules.Formation(t)&&world.FormationKnown(0,t)&&!world.IsObsolete(0,t)).ToList();
  var view=new Rect(col.x,col.y+48,col.width,346);const float Card=92;
  forcesRaiseScroll=GUI.BeginScrollView(view,forcesRaiseScroll,new Rect(0,0,view.width-18,Math.Max(view.height,types.Count*Card+40)));
  int arms=world.ArmsOutput(0);
  for(int i=0;i<types.Count;i++){int t=types[i];var r=new Rect(0,i*Card,view.width-20,Card-4);ForcesFrame(r);var at=world.DeployCity(0,t);string why=world.RaiseReason(0,t,at);
   GUI.DrawTexture(new Rect(r.x+4,r.y+6,44,44),people[t],ScaleMode.ScaleToFit,true);
   GUI.Label(new Rect(r.x+54,r.y+2,r.width-150,20),World.UnitNames[t]+"  ·  "+FormationRole(t),new GUIStyle(fSmall){wordWrap=false});
   int cost=at!=null?world.CostFor(at,World.UnitBuild(t)):World.UnitCosts[t];int ahead=world.ForceOrders(0).Count,list=world.ForceOrders(0).Sum(o=>Math.Max(0,o.cost-o.paid));int turns=arms>0?(list+cost+arms-1)/arms:-1;
   GUI.Label(new Rect(r.x+54,r.y+20,r.width-150,18),"Attack "+World.Attack[t]+" · Defence "+World.Defence[t]+" · Moves "+World.Moves[t],new GUIStyle(fTiny){wordWrap=false,clipping=TextClipping.Clip});
   GUI.Label(new Rect(r.x+54,r.y+36,r.width-60,18),ForcesRules.Service[t]+" service · "+cost+" arms · upkeep "+World.Upkeep[t]+(ForcesRules.Mounted(t)?" · 1 horse":"")+" · "+(turns>0?"~"+turns+" turns":"no arms output"),new GUIStyle(fTiny){wordWrap=false,clipping=TextClipping.Clip});
   if(why!="")GUI.Label(new Rect(r.x+54,r.y+52,r.width-60,34),why,new GUIStyle(fTiny){normal={textColor=ForceBad},wordWrap=true});
   bool w=GUI.enabled;GUI.enabled=w&&why=="";
   if(GUI.Button(new Rect(r.xMax-92,r.y+4,86,26),"Raise",button)){var res=world.Raise(0,t,at);status=res==""?World.UnitNames[t]+" ordered for "+at.name+": "+ForcesRules.Service[t]+" service reserved now; upkeep starts when it stands on the map.":res;}
   GUI.enabled=w;
   if(r.Contains(Event.current.mousePosition))forcesTip=World.UnitNames[t]+" ("+FormationRole(t)+").\nPersonnel: "+ForcesRules.Service[t]+" service point"+(ForcesRules.Service[t]==1?"":"s")+", reserved when you approve.\nEquipment: "+cost+" arms from national output (+"+arms+" a turn), after the orders above it.\nUpkeep: about "+(World.Upkeep[t]+1)/2+"-"+World.Upkeep[t]+" gold a turn (less at home) from the turn it is deployed; national income "+(world.BudgetDetail(0).Net>=0?"+":"")+world.BudgetDetail(0).Net+" now, about "+(world.BudgetDetail(0).Net-(World.Upkeep[t]+1)/2)+" after."+(ForcesRules.Mounted(t)?"\nHorse support: 1 slot while it serves.":"")+(why!=""?"\n"+why:"");
  }
  GUI.EndScrollView();
 }
 void DrawProgrammeColumn(Rect col){
  GUI.Label(new Rect(col.x,col.y,col.width,22),"Programme",fHead);var list=world.ForceOrders(0);var turns=world.OrderTurns(0);int arms=world.ArmsOutput(0);
  GUI.Label(new Rect(col.x,col.y,col.width,22),list.Count+" / "+ForcesRules.MaxOrders+" orders",new GUIStyle(fTiny){alignment=TextAnchor.MiddleRight});
  if(list.Count==0){GUI.Label(new Rect(col.x,col.y+30,col.width,80),"Nothing ordered. Arms output (+"+arms+" a turn) waits for orders: raise a formation, or refit or reinforce one from the left.",fSmall);return;}
  if(arms<=0)GUI.Label(new Rect(col.x,col.y+24,col.width,36),"Arms output is 0, so nothing progresses: build a Workshop, connect Iron or raise the military priority on the Overview tab.",new GUIStyle(fTiny){normal={textColor=ForceBad}});
  var view=new Rect(col.x,col.y+(arms<=0?60:28),col.width,col.height-(arms<=0?60:28));const float Row=74;
  forcesOrderScroll=GUI.BeginScrollView(view,forcesOrderScroll,new Rect(0,0,view.width-18,Math.Max(view.height,list.Count*Row)));
  for(int i=0;i<list.Count;i++){var o=list[i];var r=new Rect(0,i*Row,view.width-20,Row-4);ForcesFrame(r);
   GUI.Label(new Rect(r.x+8,r.y+2,r.width-90,20),(i==0?"TOP PRIORITY  ":(i+1)+".  ")+OrderTitle(o),new GUIStyle(fSmall){wordWrap=false,clipping=TextClipping.Clip});
   var city=world.cities.Find(c=>c.id==o.city);
   DrawBar(new Rect(r.x+8,r.y+24,r.width-100,8),o.paid/(float)Math.Max(1,o.cost),o.kind==0?new Color(.9f,.6f,.3f):new Color(.55f,.62f,.85f));
   string when=o.held?"Ready: held until "+(city==null||city.owner!=0?"you choose a city":city.name+" has room"):turns[i]>0?(turns[i]==1?"next turn":"in "+turns[i]+" turns"):"stopped (no arms output)";
   GUI.Label(new Rect(r.x+8,r.y+34,r.width-16,18),o.paid+" / "+o.cost+" arms · "+when,new GUIStyle(fTiny){wordWrap=false,clipping=TextClipping.Clip,normal={textColor=o.held?ForceBad:fTiny.normal.textColor}});
   GUI.Label(new Rect(r.x+8,r.y+50,r.width-150,18),(o.kind==0?"to "+(city!=null?city.name:"?"):"")+(o.service>0?" · "+o.service+" service reserved":"")+(o.horses>0?" · 1 horse":""),new GUIStyle(fTiny){wordWrap=false,clipping=TextClipping.Clip});
   bool w=GUI.enabled;
   if(o.kind==0&&GUI.Button(new Rect(r.xMax-142,r.y+48,64,20),"City ▸",button)){var mine=world.cities.Where(c=>c.owner==0&&world.DeployReason(c,o.type)=="").OrderBy(c=>c.id).ToList();if(mine.Count>0){int at=mine.FindIndex(c=>c.id==o.city);var next=mine[(at+1)%mine.Count];var res=world.Redeploy(0,o.id,next);status=res==""?"Will deploy at "+next.name+".":res;}}
   GUI.enabled=w&&i>0;if(GlyphButton(new Rect(r.xMax-74,r.y+4,20,20),0))world.MoveForceOrder(0,i,-1);
   GUI.enabled=w&&i<list.Count-1;if(GlyphButton(new Rect(r.xMax-50,r.y+4,20,20),1))world.MoveForceOrder(0,i,1);
   GUI.enabled=w;if(GlyphButton(new Rect(r.xMax-26,r.y+4,20,20),2)){var cancel=o;Ask("Cancel "+OrderTitle(cancel)+"? Its "+cancel.service+" reserved service"+(cancel.horses>0?" and horse support are":" is")+" released. The "+cancel.paid+" arms already spent on it are lost.",()=>{world.CancelForceOrder(0,cancel.id);status="Order cancelled.";});}
  }
  GUI.EndScrollView();
 }
 void Ask(string text,Action yes){forcesConfirm=text;forcesConfirmDo=yes;}
 void AskMobilise(bool on){
  var n=world.nations[0];int capNow=world.ServiceCapacity(0,n.mobilised),capThen=world.ServiceCapacity(0,on);
  int shieldsNow=world.cities.Where(c=>c.owner==0).Sum(c=>world.Yield(c)[1]);n.mobilised=on;int shieldsThen=world.cities.Where(c=>c.owner==0).Sum(c=>world.Yield(c)[1]);n.mobilised=!on;
  int over=Math.Max(0,world.ServiceServing(0)+world.ServiceReserved(0)-capThen);
  Ask((on?"Mobilise?":"Demobilise?")+"\nService capacity "+capNow+" → "+capThen+".\nShields in all your cities "+shieldsNow+" → "+shieldsThen+" a turn.\n"+(on?"The policy must stand at least "+ForcesRules.PolicyMinTurns+" turns.":"You cannot mobilise again for "+ForcesRules.PolicyMinTurns+" turns."+(over>0?"\nYou would be "+over+" service over capacity: every formation stays, but that costs "+over*ForcesRules.ShortfallGold+" gold a turn and blocks orders that add personnel until you are back within it.":"")),()=>{var why=world.SetMobilised(0,on);status=why==""?(on?"Mobilised.":"Demobilised."):why;});
 }
 void DrawForcesConfirm(){
  Fill(new Rect(24,100,1232,630),new Color(0,0,0,.55f));var box=new Rect(390,300,500,220);GUI.Box(box,"");ForcesFrame(box);
  GUI.Label(new Rect(box.x+20,box.y+16,box.width-40,140),forcesConfirm,fSmall);
  if(GUI.Button(new Rect(box.x+20,box.yMax-48,220,32),"Confirm",button)){var act=forcesConfirmDo;forcesConfirm=null;forcesConfirmDo=null;if(act!=null)act();GUIUtility.ExitGUI();}
  if(GUI.Button(new Rect(box.xMax-240,box.yMax-48,220,32),"Keep as it is",button)){forcesConfirm=null;forcesConfirmDo=null;GUIUtility.ExitGUI();}
 }
}
}
