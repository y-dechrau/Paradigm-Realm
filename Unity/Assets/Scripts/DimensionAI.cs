using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Rivals and diplomacy beyond the gates (plan stage 6, Design/DIMENSIONS-PLAN-2026-11-11.md).
// Rivals use exactly the player's rules: the same investigation project and cost (World.GateProjectReason), the same gateway
// schedule and capacity (they cross through World.GatewayTravel like anyone), the same colony rules, and they know of a gateway
// only once their own map shows it. They put home first: no gateway work at war, while small or while poor, and one colony
// (two from the Industrial Age) at most.
//
// Diplomacy added here is a small set of exchanges with real effects. Passage and military transit through a gateway on
// someone's land is the existing open borders agreement; resource trade is the existing deal screen.
//  - Recognise colonial claims (15 influence): neither side founds within 4 tiles of the other's colonies or attacks them;
//    both think better of each other. War ends it.
//  - Joint investigation (20 gold to the other side): a civilisation that has read a seal shares the reading.
//  - Lessons from a local people (30 gold): 8 points toward that family's discoveries.
//  - Supplies from a local people (20 gold): 9 supplies delivered to a colony in their world, gateway or no gateway.
[Serializable] public class DimensionAccord { public int a,b,kind,since; }
public static class AccordCatalog {
 public const int Claims=0;
 public const int ClaimInfluence=15,ClaimReach=4,ClaimOpinion=8,JointGold=20,SupplyGold=20,SupplyAmount=9;
}
public partial class World {
 public List<DimensionAccord> accords=new List<DimensionAccord>();
 public bool HasAccord(int a,int b,int kind){if(accords==null)return false;foreach(var x in accords)if(x.kind==kind&&(x.a==a&&x.b==b||x.a==b&&x.b==a))return true;return false;}
 void AccordTurn(int owner)
 {
  if(accords==null){accords=new List<DimensionAccord>();return;}
  foreach(var x in accords.ToArray()){if(x.a!=owner&&x.b!=owner)continue;if(!AtWar(x.a,x.b))continue;accords.Remove(x);if(x.a==0||x.b==0)dimensionNews.Add("War has ended the recognition of colonial claims with "+NationName(x.a==0?x.b:x.a)+".");}
 }
 // ---------- colonial claims ----------
 public string ClaimsReason(int owner,int other)
 {
  if(other==owner||other<0||other>=civCount)return "Choose a civilisation.";if(IsNativeNation(other)||IsNativeNation(owner))return "Claims are recognised between civilisations of the home world.";
  if(!HasContact(owner,other))return "You have not met.";if(AtWar(owner,other))return "You are at war.";if(Rivals(owner,other))return "You are rivals.";
  if(HasAccord(owner,other,AccordCatalog.Claims))return "Already recognised.";
  if(ColoniesOf(owner).Count==0&&ColoniesOf(other).Count==0)return "Neither of you has a colony to recognise.";
  if(Opinion(other,owner)<0)return NationName(other)+" thinks too little of "+(owner==0?"you":NationName(owner))+" (opinion "+Opinion(other,owner)+").";
  if(nations[owner].influence<AccordCatalog.ClaimInfluence)return "Needs "+AccordCatalog.ClaimInfluence+" influence.";return "";
 }
 public bool RecogniseClaims(int owner,int other)
 {
  if(ClaimsReason(owner,other)!="")return false;nations[owner].influence-=AccordCatalog.ClaimInfluence;accords.Add(new DimensionAccord{a=owner,b=other,kind=AccordCatalog.Claims,since=turn});
  AddOpinion(other,owner,OpinionMods.Dealings,AccordCatalog.ClaimOpinion);AddOpinion(owner,other,OpinionMods.Dealings,AccordCatalog.ClaimOpinion);
  DiplomacyEvent(owner,other,NationName(owner)+" and "+NationName(other)+" recognise each other's colonial claims.");return true;
 }
 // ---------- joint investigation ----------
 public string JointStudyReason(int owner,int other,Tile gate)
 {
  if(gate==null||gate.gateLink<=0)return "Not a gateway.";if(other==owner||other<0||other>=civCount||!HasContact(owner,other))return "Choose a civilisation you have met.";if(AtWar(owner,other))return "You are at war.";
  var mine=StudyOf(owner,gate);if(mine!=null&&mine.known)return "You have already read this seal.";var theirs=StudyOf(other,gate);if(theirs==null||!theirs.known)return NationName(other)+" has not read this seal.";
  if(Opinion(other,owner)<0)return NationName(other)+" will not share with "+(owner==0?"you":NationName(owner))+" (opinion "+Opinion(other,owner)+").";
  if(Treasury(owner)<AccordCatalog.JointGold)return "Needs "+AccordCatalog.JointGold+" gold.";return "";
 }
 public bool JointStudy(int owner,int other,Tile gate)
 {
  if(JointStudyReason(owner,other,gate)!="")return false;AddDiplomaticGold(owner,-AccordCatalog.JointGold);AddDiplomaticGold(other,AccordCatalog.JointGold);
  var s=StudyFor(owner,HomeSide(gate));s.known=true;s.turns=GateApproach.StudyTurns;s.method=StudyOf(other,gate).method;AddOpinion(other,owner,OpinionMods.Dealings,3);AddOpinion(owner,other,OpinionMods.Dealings,3);
  DiplomacyEvent(owner,other,NationName(other)+" shares its reading of a gateway's seal with "+NationName(owner)+".");if(owner==0)AddGateNotice(gate);return true;
 }
 public List<int> JointStudyPartners(int owner,Tile gate){var list=new List<int>();for(int o=0;o<civCount;o++)if(o!=owner&&JointStudyReason(owner,o,gate)=="")list.Add(o);return list;}
 // ---------- local peoples ----------
 public List<int> LocalPeoples(int owner,int realm){var list=new List<int>();for(int o=0;o<civCount;o++)if(o!=owner&&IsNativeNation(o)&&CivilisationCatalog.NativeRealm(nations[o].civ)==realm&&HasContact(owner,o)&&cities.Any(c=>c.owner==o))list.Add(o);return list;}
 public string LessonReason(int owner,int native)
 {
  if(native<0||native>=civCount||!IsNativeNation(native))return "Choose a people of that world.";int realm=CivilisationCatalog.NativeRealm(nations[native].civ),family=DimensionFamily.OfRealm(realm);
  if(family==DimensionFamily.Home)return "Their world works as yours does: there is nothing of that kind to learn.";if(!HasContact(owner,native))return "You have not met.";if(AtWar(owner,native))return "You are at war.";
  if(Opinion(native,owner)<LocalLessonOpinion)return "They do not trust "+(owner==0?"you":NationName(owner))+" enough yet (opinion "+Opinion(native,owner)+", needs "+LocalLessonOpinion+").";
  if(Treasury(owner)<LocalLessonGold)return "Needs "+LocalLessonGold+" gold.";return "";
 }
 public bool LocalLesson(int owner,int native)
 {
  if(LessonReason(owner,native)!="")return false;int family=DimensionFamily.OfRealm(CivilisationCatalog.NativeRealm(nations[native].civ));AddDiplomaticGold(owner,-LocalLessonGold);AddDiplomaticGold(native,LocalLessonGold);
  AdaptationOf(owner,family,true).points+=LocalLessonPoints;AddOpinion(native,owner,OpinionMods.Dealings,2);if(owner==0)Note("The "+NationName(native)+" show your people how they live there: "+LocalLessonPoints+" points toward "+AdaptationCatalog.FamilyTitles[family].ToLower()+".");return true;
 }
 public string LocalSupplyReason(int owner,int native,Colony c)
 {
  if(c==null||c.owner!=owner)return "Choose one of your colonies.";if(native<0||native>=civCount||!IsNativeNation(native)||CivilisationCatalog.NativeRealm(nations[native].civ)!=At(c.x,c.y).realm)return "Choose a people of that colony's world.";
  if(!HasContact(owner,native))return "You have not met.";if(AtWar(owner,native))return "You are at war.";if(Opinion(native,owner)<0)return "They will not deal with "+(owner==0?"you":NationName(owner))+" (opinion "+Opinion(native,owner)+").";
  if(c.stock>=ColonyStorage(c))return "Its stores are full.";if(Treasury(owner)<AccordCatalog.SupplyGold)return "Needs "+AccordCatalog.SupplyGold+" gold.";return "";
 }
 public bool LocalSupply(int owner,int native,Colony c)
 {
  if(LocalSupplyReason(owner,native,c)!="")return false;AddDiplomaticGold(owner,-AccordCatalog.SupplyGold);AddDiplomaticGold(native,AccordCatalog.SupplyGold);c.stock=Math.Min(ColonyStorage(c),c.stock+AccordCatalog.SupplyAmount);
  AddOpinion(native,owner,OpinionMods.Dealings,1);if(owner==0)Note("The "+NationName(native)+" sell "+c.name+" "+AccordCatalog.SupplyAmount+" supplies.");return true;
 }
 // ---------- rivals ----------
 public int ColonyLimit(int owner){return Era(owner)>=3?2:1;}
 // Does this rival want a colony now, and through which gateway? Null when home comes first.
 Tile ColonyGateFor(int owner)
 {
  if(owner==0||IsNativeNation(owner)||!dimensionRules||!GateInvestWanted(owner))return null;if(cities.Count(c=>c.owner==owner)<3||ColoniesOf(owner).Count>=ColonyLimit(owner)||nations[owner].gold<20)return null;
  Tile best=null;int least=int.MaxValue;var capital=cities.Find(c=>c.id==nations[owner].capital)??cities.FirstOrDefault(c=>c.owner==owner);if(capital==null)return null;
  foreach(var g in HomeGates()){if(!GateKnownTo(owner,g)||ConditionOf(g)!=GateCondition.Cycling&&!GateOpen(g))continue;if(GateBlock(owner,g)!=""&&!(ConditionOf(g)==GateCondition.Cycling&&!GateOpen(g)))continue;
   if(ColonySites(owner,GateRealm(g)).Count==0)continue;int d=Distance(capital.x,capital.y,g.x,g.y);if(d<least){least=d;best=g;}}
  return best;
 }
 // A city trains the Settler for a colony when it is the rival's nearest city to the chosen gateway.
 bool ColonySettlerWanted(City c)
 {
  if(c.owner==0||At(c.x,c.y).realm!=0)return false;var gate=ColonyGateFor(c.owner);if(gate==null||c.pop<2||BuildReason(c,0)!=""||units.Any(u=>u.owner==c.owner&&u.type==0))return false;
  var nearest=cities.Where(x=>x.owner==c.owner&&At(x.x,x.y).realm==0).OrderBy(x=>Distance(x.x,x.y,gate.x,gate.y)).ThenBy(x=>x.id).FirstOrDefault();return nearest==c&&Distance(c.x,c.y,gate.x,gate.y)<=20;
 }
 // A rival Settler with a colony to found: walk to the gateway, wait for it to open, cross, walk to a site, found.
 bool ColonyErrand(Unit u)
 {
  if(u.owner==0||IsNativeNation(u.owner)||!dimensionRules)return false;var here=At(u.x,u.y);
  if(here.realm>0)
  {
   var sites=ColonySites(u.owner,here.realm);if(sites.Any(s=>s.x==u.x&&s.y==u.y)&&ColonyReason(u)==""){FoundColony(u);return true;}
   foreach(var s in sites){var route=Route(u,s.x,s.y);if(route.Count==0)continue;Order(u,s.x,s.y);if(units.Contains(u)&&u.x==s.x&&u.y==s.y&&ColonyReason(u)=="")FoundColony(u);return true;}
   if(ColonyReason(u)==""){FoundColony(u);return true;}return false;
  }
  // At home a city site comes first; the gateway is for a civilisation that has filled its land.
  if(cities.Count(c=>c.owner==u.owner)<RivalTargetCities(u.owner)&&SettlementOptions(u,1).Count>0)return false;
  var gate=ColonyGateFor(u.owner);if(gate==null)return false;
  if(u.x==gate.x&&u.y==gate.y){if(GateOpen(gate))GatewayTravel(u);return true;}
  if(Route(u,gate.x,gate.y).Count==0)return false;Order(u,gate.x,gate.y);return true;
 }
 // A rival that has read a seal it can afford sends its nearest spare soldier to stand on the gateway, since the work needs
 // someone there to begin it and again when it opens (the same rule as the player's).
 bool GateErrand(Unit u)
 {
  if(u.owner==0||IsNativeNation(u.owner)||!dimensionRules||Power[u.type]<=0||IsNaval(u.type)||gateStudies==null||!GateInvestWanted(u.owner))return false;
  foreach(var s in gateStudies)
  {
   if(s.owner!=u.owner||!s.known)continue;var gate=GatePairTile(s.pair);if(gate==null||GateSteady(gate)||gate.realm!=At(u.x,u.y).realm)continue;
   if(s.approach==GateApproach.None&&GateLacks(u.owner,gate,false,GateApproach.StabiliseGold)!="")continue;{var st=FindState(gate);if(st!=null&&st.condition==GateCondition.Damaged&&st.repair>0)continue;}
   if(u.x==gate.x&&u.y==gate.y){u.fortified=true;return true;}
   int mine=Distance(u.x,u.y,gate.x,gate.y);if(mine>16)continue;bool nearest=true;
   foreach(var o in units){if(o==u||o.owner!=u.owner||Power[o.type]<=0||IsNaval(o.type)||At(o.x,o.y).realm!=gate.realm)continue;int d=Distance(o.x,o.y,gate.x,gate.y);if(d<mine||d==mine&&o.id<u.id){nearest=false;break;}}
   if(!nearest)continue;
   // Never the last soldier in a city.
   if(cities.Any(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y)&&!units.Any(a=>a.id!=u.id&&a.owner==u.owner&&Power[a.type]>0&&a.x==u.x&&a.y==u.y))continue;
   if(Route(u,gate.x,gate.y).Count==0)continue;Order(u,gate.x,gate.y);return true;
  }
  return false;
 }
 // A rival looks after its colonies with what it can spare, and deals with its neighbours beyond the gates.
 void ColonyAI(int owner)
 {
  if(IsNativeNation(owner))return;var mine=ColoniesOf(owner);if(mine.Count==0)return;bool war=false;for(int o=0;o<civCount;o++)if(o!=owner&&AtWar(owner,o))war=true;
  foreach(var c in mine)
  {
   if(war&&c.specialisation!=ColonyCatalog.Military&&c.level>=1)c.specialisation=ColonyCatalog.Military;else if(!war&&c.specialisation==ColonyCatalog.Military)c.specialisation=ChooseSpecialisation(owner);
   if(c.level>=2&&c.second<0)c.second=c.specialisation==ColonyCatalog.Extraction?ColonyCatalog.Research:ColonyCatalog.Extraction;
   c.policy=c.stock<SupplyNeed(c)*2?ColonyCatalog.SelfSufficiency:LocalPeoples(owner,At(c.x,c.y).realm).Count>0?ColonyCatalog.Cooperation:ColonyCatalog.Exploitation;
   // Buildings only from a full purse: home upkeep is paid first.
   if(!war)foreach(int b in new[]{c.stock<SupplyNeed(c)*3?ColonyCatalog.Storehouse:ColonyCatalog.Workings,ColonyCatalog.GardenPlots,ColonyCatalog.FamilyWorkshop,ColonyCatalog.Palisade}){if(Treasury(owner)<ColonyCatalog.BuildingGold[b]+90)break;if(ColonyBuildReason(c,b)==""){ColonyBuild(c,b);break;}}
   // Short of supplies with the gateway shut: buy from the people of that world.
   if(c.isolated>0&&c.stock<SupplyNeed(c)*2)foreach(int native in LocalPeoples(owner,At(c.x,c.y).realm))if(LocalSupply(owner,native,c))break;
  }
  if(war||turn%8!=owner%8)return;
  foreach(var c in mine)foreach(int native in LocalPeoples(owner,At(c.x,c.y).realm)){if(Treasury(owner)>=LocalLessonGold+80&&LocalLesson(owner,native))break;}
  // Rivals with colonies recognise each other's claims when they get on, and offer the same to a player they like.
  for(int o=0;o<civCount;o++)
  {
   if(o==owner||IsNativeNation(o)||ColoniesOf(o).Count==0||Opinion(owner,o)<20||ClaimsReason(owner,o)!="")continue;
   if(o==0&&Opinion(0,owner)<0)continue;if(RecogniseClaims(owner,o)&&o==0)Note(NationName(owner)+" recognises your colonial claims, and asks the same of you. Neither of you will found beside the other's colonies.");
  }
 }
 bool AccordsValid(){if(accords==null)return false;foreach(var x in accords)if(x==null||x.a<0||x.b<0||x.a>=civCount||x.b>=civCount||x.a==x.b||x.kind!=AccordCatalog.Claims)return false;return true;}
}
}
