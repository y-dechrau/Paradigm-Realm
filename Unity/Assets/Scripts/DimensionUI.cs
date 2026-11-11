using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The Gates and colonies screen (plan stage 7, Design/DIMENSIONS-PLAN-2026-11-11.md): one place for gateways, colonies, supply,
// local knowledge and agreements, opened from the Empire screen. The first tab lists only what the player can act on now.
// Every figure shown is the one the rules use (World.GateCapacityFor, PriorityPreview, SupplyNeed, DiscoveryCost and so on).
public partial class RealmGame {
 int dimensionTab;Vector2 homeCamera;bool homeCameraSaved;
 static readonly string[] DimensionTabs={"Overview","Gateways","Colonies","Local knowledge","Agreements"};
 void Line(string text){GUILayout.Label(text,body);}
 void Small(string text){GUILayout.Label(text,small);}
 bool Small(string text,float width,bool enabled=true){bool was=GUI.enabled;GUI.enabled=was&&enabled;bool hit=GUILayout.Button(text,button,GUILayout.Width(width),GUILayout.Height(26));GUI.enabled=was;return hit;}
 void ShowOnMap(int x,int y){var t=world.At(x,y);if(t==null)return;if(t.realm!=viewRealm&&viewRealm==0){homeCamera=new Vector2(camI,camJ);homeCameraSaved=true;}detailsScreen="";FocusTile(x,y);ResetMapGesture();}
 void DrawDimensions()
 {
  GUILayout.BeginHorizontal();for(int i=0;i<DimensionTabs.Length;i++)if(GUILayout.Button(i==dimensionTab?"[ "+DimensionTabs[i]+" ]":DimensionTabs[i],button,GUILayout.Width(190),GUILayout.Height(30)))dimensionTab=i;GUILayout.EndHorizontal();GUILayout.Space(8);
  if(!world.HasRealms){Line("This world has no gateways.");return;}
  switch(dimensionTab){case 1:DrawGatewayList();break;case 2:DrawColonyList();break;case 3:DrawLocalKnowledge();break;case 4:DrawAccords();break;default:DrawDimensionOverview();break;}
 }
 void DrawDimensionOverview()
 {
  GUILayout.Label("Needs your attention",heading);var problems=world.DimensionProblems(0);
  if(problems.Count==0)Line("Nothing here needs you now.");else foreach(string p in problems)Line("-  "+p);
  GUILayout.Space(10);GUILayout.Label("At a glance",heading);
  var gates=world.HomeGates().Where(g=>world.gatesFound!=null&&world.gatesFound.Contains(g.y*world.Width+g.x)).ToList();var mine=world.ColoniesOf(0);
  Line("Gateways found: "+gates.Count+" of "+world.HomeGates().Count+".   Open for good: "+gates.Count(g=>world.GateSteady(g))+".   Seals read: "+gates.Count(g=>{var s=world.StudyOf(0,g);return s!=null&&s.known;})+".");
  Line("Colonies: "+mine.Count+(mine.Count>0?".   Production sent home last turn: "+mine.Sum(c=>c.shipShields)+".   Gold: "+mine.Sum(c=>c.shipGold)+".":". A Settler founds one beyond a gateway; a Scout can plant a foothold."));
  if(!world.dimensionRules)Small("This game was begun before gateways differed: all of its gateways are sealed, and open as they always did.");
  GUILayout.Space(10);GUILayout.Label("What goes through first",heading);int now=world.GatePriorityOf(0);
  GUILayout.BeginHorizontal();for(int p=0;p<3;p++)if(GUILayout.Button(p==now?"[ "+ColonyCatalog.Priorities[p]+" ]":ColonyCatalog.Priorities[p],button,GUILayout.Width(220),GUILayout.Height(30)))world.SetGatePriority(0,p);GUILayout.EndHorizontal();
  Line(ColonyCatalog.PriorityTexts[now]);
  Small("A gateway carries a number of loads a turn. One load is 3 supplies out, or one load of exports home, or one unit crossing.");
  foreach(var g in gates.Where(g=>mine.Any(c=>c.gate>=0&&world.StateOf(g).pair==c.gate)))
  {
   GUILayout.Space(4);Line("The "+World.GateStyleNames[world.GateStyle(g)].ToLower()+" to the "+World.RealmNames[world.GateRealm(g)]+(world.GateUsable(0,g)?"":"   (shut to you: "+world.GateBlock(0,g)+")"));
   for(int p=0;p<3;p++)Small((p==now?"Now, ":"With ")+ColonyCatalog.Priorities[p].ToLower()+": "+world.PriorityPreview(0,g,p));
  }
 }
 void DrawGatewayList()
 {
  var gates=world.HomeGates().Where(g=>world.gatesFound!=null&&world.gatesFound.Contains(g.y*world.Width+g.x)).ToList();
  if(gates.Count==0){Line("Your people have found no gateway yet. They are found by exploring, like anything else on the map.");return;}
  Small("Method of investigation your people would use now: "+GateMethod.Names[world.InvestigationMethod(0)]+". "+GateMethod.Texts[world.InvestigationMethod(0)]);
  foreach(var g in gates)
  {
   var study=world.StudyOf(0,g);bool read=study!=null&&study.known;int cond=world.ConditionOf(g);GUILayout.Space(8);
   GUILayout.BeginHorizontal();GUILayout.Label("The "+World.GateStyleNames[world.GateStyle(g)].ToLower()+(read||world.GateSteady(g)?" to the "+World.RealmNames[world.GateRealm(g)]:" (destination not yet known)"),heading,GUILayout.Width(640));
   if(Small("Show on map",130))ShowOnMap(g.x,g.y);
   var partners=read?new List<int>():world.JointStudyPartners(0,g);if(partners.Count>0&&Small("Share "+world.NationName(partners[0])+"'s reading ("+AccordCatalog.JointGold+" gold)",330)){if(world.JointStudy(0,partners[0],g))status=world.NationName(partners[0])+" shares its reading of the seal.";}
   GUILayout.EndHorizontal();
   Line(world.GateSteady(g)?"Open.":(cond==GateCondition.Cycling?GateCondition.Names[cond]+".  ":"")+world.GateSchedule(g));
   Line(read?"Seal read"+(study.method>0||study.turns>0?" by "+GateMethod.Names[study.method].ToLower():"")+".   Danger beyond: "+world.DangerText(g,0)+".   Carries "+world.GateCapacityFor(0,g)+" loads a turn for you.":"Seal not read. Keep a Scout, Medic or Priest on it for "+GateApproach.StudyTurns+" turns, or have a city within "+DimensionSettings.InvestigateRange+" tiles run the Investigate Gateway project.");
   if(!world.GateSteady(g)&&read&&world.ApproachReason(0,g,GateApproach.Stabilise)!="")Small("To open it for good: "+world.ApproachReason(0,g,GateApproach.Stabilise));
   foreach(string l in world.CompatibilityReport(0,g))Small(l);
  }
 }
 void DrawColonyList()
 {
  var mine=world.ColoniesOf(0);
  if(mine.Count==0){Line("You have no colonies. Beyond a gateway a Settler founds a colony, not a city: a compact settlement that works by itself and sends what it makes to one of your home cities. A Scout can plant a foothold that grows into one once supplies reach it.");return;}
  foreach(var c in mine)
  {
   var here=world.At(c.x,c.y);int family=world.FamilyAt(here),need=world.SupplyNeed(c),ore,goods;world.ColonyOutput(c,out ore,out goods);var home=world.ColonyHome(c);GUILayout.Space(8);
   GUILayout.BeginHorizontal();GUILayout.Label(c.name+", "+ColonyCatalog.Levels[Mathf.Clamp(c.level,0,2)].ToLower()+" in the "+World.RealmNames[here.realm],heading,GUILayout.Width(640));if(Small("Show on map",130))ShowOnMap(c.x,c.y);GUILayout.EndHorizontal();
   Line("Supplies "+c.stock+" of "+world.ColonyStorage(c)+", uses "+need+" a turn"+(need>0?" ("+(c.stock/Math.Max(1,need))+" turns in store)":"")+".   Self-sufficiency "+c.selfSufficiency+"% (+"+world.SelfGrowth(c)+" a turn).   Defence "+world.ColonyDefence(c)+"."+(c.isolated>0?"   CUT OFF for "+c.isolated+" turn"+(c.isolated==1?"":"s")+".":""));
   Line("Sends home each turn: "+ore+" load"+(ore==1?"":"s")+" of materials, "+goods+" of goods.   Last turn "+(home==null?"it had no home city":home.name+" gained "+c.shipShields+" production and "+c.shipGold+" gold")+".   Family: "+DimensionFamily.Names[family]+".");
   GUILayout.BeginHorizontal();
   if(Small("Does: "+ColonyCatalog.Specialisations[c.specialisation],250,c.level>=1))world.SetSpecialisation(c,(c.specialisation+1)%4);
   if(c.level>=2&&Small("Also: "+(c.second<0?"nothing yet":ColonyCatalog.Specialisations[c.second]),250)){int next=c.second;for(int k=0;k<4;k++){next=(next+1)%4;if(world.SecondSpecReason(c,next)=="")break;}world.SetSpecialisation(c,next,true);}
   if(Small("Policy: "+ColonyCatalog.Policies[c.policy],250))world.SetColonyPolicy(c,(c.policy+1)%3);
   GUILayout.EndHorizontal();
   Small(ColonyCatalog.SpecialisationTexts[c.specialisation]+"  "+ColonyCatalog.PolicyTexts[c.policy]);
   GUILayout.BeginHorizontal();
   for(int b=0;b<ColonyCatalog.Buildings.Length;b++)
   {
    string name=b==ColonyCatalog.FamilyWorkshop?world.WorkshopName(c):ColonyCatalog.Buildings[b];if(c.buildings.Contains(b)){GUILayout.Label(name+" (built)",small,GUILayout.Width(180));continue;}
    string why=world.ColonyBuildReason(c,b);if(Small(name+"  "+ColonyCatalog.BuildingGold[b]+" gold",180,why=="")){if(world.ColonyBuild(c,b))status=c.name+" builds "+name.ToLower()+".";}
   }
   GUILayout.EndHorizontal();
   {string first="";for(int b=0;b<ColonyCatalog.Buildings.Length&&first=="";b++)if(!c.buildings.Contains(b)&&world.ColonyBuildReason(c,b)!="")first=(b==ColonyCatalog.FamilyWorkshop?world.WorkshopName(c):ColonyCatalog.Buildings[b])+": "+world.ColonyBuildReason(c,b);if(first!="")Small(first);}
   // One offer: the people of that world who think best of you.
   {var locals=world.LocalPeoples(0,here.realm).OrderByDescending(o=>world.Opinion(o,0)).ToList();if(locals.Count>0){int native=locals[0];string who=world.NationName(native);if(who.StartsWith("The "))who=who.Substring(4);
    GUILayout.BeginHorizontal();string why=world.LocalSupplyReason(0,native,c);
    if(Small("Buy "+AccordCatalog.SupplyAmount+" supplies from the "+who+" ("+AccordCatalog.SupplyGold+" gold)",460,why=="")){if(world.LocalSupply(0,native,c))status="Supplies bought.";}
    if(why!="")GUILayout.Label(why,small);GUILayout.EndHorizontal();}}
  }
 }
 void DrawLocalKnowledge()
 {
  Small("What your scholars know is true everywhere. What follows is how to live and work where the physics differ: learned by being there, faster with colony facilities and with gold from home, and shared by every world of the same family.");
  for(int family=1;family<=AdaptationCatalog.Synthesis;family++)
  {
   GUILayout.Space(10);var a=world.AdaptationOf(0,family);bool synthesis=family==AdaptationCatalog.Synthesis;
   GUILayout.Label(AdaptationCatalog.FamilyTitles[family],heading);
   if(synthesis)Small("Discoveries that need local knowledge of both families. While one is being studied, half of your cities' science ("+world.SynthesisShare(0)+" a turn now) goes to it instead of the technology in hand."+(a!=null&&a.current>=0?"":"  None is being studied."));
   else
   {
    Small(DimensionFamily.Rules[family]);int rate=world.AdaptationRate(0,family);
    GUILayout.BeginHorizontal();GUILayout.Label(rate>0?"Learning "+rate+" point"+(rate==1?"":"s")+" a turn.   In hand: "+(a==null?0:a.points)+".":"Nobody of yours is there: nothing is being learned."+(a!=null&&a.points>0?"   In hand: "+a.points+".":""),body,GUILayout.Width(520));
    int funding=a==null?0:a.funding;if(Small("-",34,funding>0))world.SetAdaptationFunding(0,family,funding-1);GUILayout.Label("Funding "+funding+" ("+funding*AdaptationCatalog.FundingStep+" gold a turn)",body,GUILayout.Width(250));if(Small("+",34,funding<AdaptationCatalog.MaxFunding))world.SetAdaptationFunding(0,family,funding+1);
    GUILayout.EndHorizontal();
   }
   var list=AdaptationCatalog.ByFamily[family];
   for(int i=0;i<list.Length;i++)
   {
    var d=list[i];bool known=world.Knows(0,family,i);string why=known?"":world.DiscoveryReason(0,family,i);int cost=world.DiscoveryCost(0,family,i);bool current=a!=null&&a.current==i;
    GUILayout.BeginHorizontal();
    GUILayout.Label(d.name,body,GUILayout.Width(210));GUILayout.Label(d.effect,small,GUILayout.Width(520));
    GUILayout.Label(known?"Known":current?"Studying: "+(a==null?0:a.points)+" of "+cost:why!=""?why:cost+(synthesis?" science":" points")+(cost<d.cost?" (was "+d.cost+")":""),small,GUILayout.Width(250));
    if(!known&&!current&&why==""&&Small("Study",90)){world.StudyDiscovery(0,family,i);status="Studying "+d.name+".";}
    if(current&&Small("Stop",90))world.StudyDiscovery(0,family,-1);
    GUILayout.EndHorizontal();
   }
  }
 }
 void DrawAccords()
 {
  GUILayout.Label("Passage",heading);Line("A gateway that stands on another civilisation's land carries your units and loads only under an open borders agreement, made on the Diplomacy screen. If the agreement ends or war comes, the crossing shuts to you; what you have beyond it stays yours and lives on its stores.");
  Line("Resources are traded on the Diplomacy screen as before.");
  GUILayout.Space(10);GUILayout.Label("Colonial claims",heading);Small("Costs "+AccordCatalog.ClaimInfluence+" influence. Neither side founds within "+AccordCatalog.ClaimReach+" tiles of the other's colonies, and each thinks better of the other. War ends it.");
  bool any=false;
  for(int o=1;o<world.civCount;o++)
  {
   if(world.IsNativeNation(o)||!world.HasContact(0,o))continue;any=true;GUILayout.BeginHorizontal();GUILayout.Label(world.NationName(o)+"   ("+world.ColoniesOf(o).Count+" colon"+(world.ColoniesOf(o).Count==1?"y":"ies")+")",body,GUILayout.Width(360));
   if(world.HasAccord(0,o,AccordCatalog.Claims))GUILayout.Label("Claims recognised.",body);
   else{string why=world.ClaimsReason(0,o);if(Small("Recognise each other's claims",280,why=="")){if(world.RecogniseClaims(0,o))status="Colonial claims recognised with "+world.NationName(o)+".";}if(why!="")GUILayout.Label(why,small);}
   GUILayout.EndHorizontal();
  }
  if(!any)Line("You have met no other civilisation of the home world.");
  GUILayout.Space(10);GUILayout.Label("The peoples beyond the gates",heading);Small("A people that lives in another world already knows how to live there. For "+World.LocalLessonGold+" gold, one that trusts you (opinion "+World.LocalLessonOpinion+") teaches "+World.LocalLessonPoints+" points toward that family's local discoveries. They also sell supplies to a colony in their world (Colonies tab).");
  any=false;
  for(int o=1;o<world.civCount;o++)
  {
   if(!world.IsNativeNation(o)||!world.HasContact(0,o))continue;any=true;int family=DimensionFamily.OfRealm(CivilisationCatalog.NativeRealm(world.nations[o].civ));string why=world.LessonReason(0,o);
   GUILayout.BeginHorizontal();GUILayout.Label(world.NationName(o)+"   ("+World.RealmNames[CivilisationCatalog.NativeRealm(world.nations[o].civ)]+", "+DimensionFamily.Names[family]+")",body,GUILayout.Width(460));
   if(Small("Ask for lessons",180,why=="")){if(world.LocalLesson(0,o))status="Lessons learned.";}if(why!="")GUILayout.Label(why,small);GUILayout.EndHorizontal();
  }
  if(!any)Line("You have met none of them yet.");
 }
}
}
