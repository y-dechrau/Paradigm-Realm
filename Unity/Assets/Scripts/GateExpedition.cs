using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Opening a gateway as a small expedition with a choice at the end (owner design, 10 Nov 2026; save version 41).
//  1. Investigate: a Scout, Medic or Priest that ends two turns on a sealed gateway reads its seal. That tells its civilisation
//     where the gateway leads, what lies there, what the seal asks for and roughly how dangerous the far side is.
//  2. Requirement: every gateway asks for one thing set by the world beyond it (the design of its far side) and a smaller
//     activation cost set by its own design on this side, so the nine designs ask for different things.
//  3. Approach: Stabilise (dearer and slower, no danger), Force (at once, but its guardians fight the soldiers holding the tile)
//     or Negotiate (another civilisation pays half and gains open borders; a little slower than forcing, no fight).
//  4. The gateway opens for everyone, and the opener sees the land around the far side.
// Every cost and consequence is worked out before the choice and shown (GateOutlook), and nothing is left to chance.
// The older ways still stand: a world's key technology, the Gatekeeper's Key wonder and the priest's rite (Realms.cs).
[Serializable] public class GateStudy {
 public int pair,owner,turns,approach,ready,partner=-1,side,method; public bool known,sunstone;// side: which half of a Rift seal is empowered (0 living, 1 burning); sunstone: recovered from a ruin for a Sun seal
}
public class GateCost {
 public int gold,faith,influence,resource,techCount; public List<string> techs=new List<string>(); public string main="",activation="";
}
public static class GateApproach {
 public const int None=0,Stabilise=1,Force=2,Negotiate=3,Closing=4;
 public static readonly string[] Names={"","Stabilise","Force open","Negotiate access","Closing"};
 public const int StudyTurns=2,StabiliseTurns=6,NegotiateTurns=3,StabiliseGold=30,LandingSight=3,PartnerOpinion=10;
 // Added 11 Nov 2026 from the rest of the owner's design: the first gateway to open anywhere asks half and stabilises in 3 turns; a
 // Sun seal takes a sunstone from a ruin within 6 tiles; a Grove seal is satisfied by a Shrine within 10; a Rift seal's opener
 // chooses which half to empower; a held gateway can be closed in a full turn.
 public const int FirstStabiliseTurns=3,RuinReach=6,ShrineReach=10,RiftReach=2,FireSight=5;
 public static readonly string[] RiftSides={"The living side","The burning side"};
 public static readonly string[] Danger={"","Low","Moderate","High"};
}
public partial class World {
 public List<GateStudy> gateStudies=new List<GateStudy>();
 // Units that can read a seal: the Scout from the first turn, so the first gateway is within early reach; the Medic and the Priest later.
 public static bool Investigator(int type){return type==3||type==10||type==PriestUnit;}
 public GateStudy StudyOf(int owner,Tile gate){if(gate==null||gate.gateLink<=0||gateStudies==null)return null;int pair=GatePair(gate);foreach(var s in gateStudies)if(s.pair==pair&&s.owner==owner)return s;return null;}
 GateStudy StudyFor(int owner,Tile gate){var s=StudyOf(owner,gate);if(s==null){s=new GateStudy{pair=GatePair(gate),owner=owner};gateStudies.Add(s);}return s;}
 Tile HomeSide(Tile gate){if(gate==null)return null;if(gate.realm==0)return gate;var twin=GateTwin(gate);return twin!=null&&twin.realm==0?twin:gate;}
 public int GateDanger(Tile gate){int r=GateRealm(gate);int[] d={1,1,2,3,2,1,3};return d[Math.Max(0,Math.Min(d.Length-1,r))];}
 // What the seal asks for: the main requirement from the design of the far side, the activation from the design of the near side.
 public bool FirstGateway{get{return openedGates==null||openedGates.Count==0;}}
 public int StabiliseTurnsNow{get{return FirstGateway?GateApproach.FirstStabiliseTurns:GateApproach.StabiliseTurns;}}
 public int FarStyle(Tile gate){int r=GateRealm(gate);return RealmGateStyle[Math.Max(0,Math.Min(RealmGateStyle.Length-1,r))];}
 public GateCost GateNeed(Tile gate,int owner=-1)
 {
  var c=new GateCost();int r=GateRealm(gate);int far=FarStyle(gate),near=GateStyle(HomeSide(gate));bool first=FirstGateway;Func<int,int> h=v=>first?Half(v):v;
  bool stone=owner>=0&&far==4&&HasSunstone(owner,gate),shrine=owner>=0&&far==1&&ShrineNear(owner,gate);
  switch(far)
  {
   case 0:c.techs.Add("writing");c.gold+=h(40);c.main="Frost seal: read its frozen inscription (Writing) and bring fuel to thaw it ("+h(40)+" gold).";break;
   case 1:if(shrine)c.main="Grove seal: the Shrine in your city nearby tends the shrine before the gate. Nothing more is asked.";else{c.faith+=h(40);c.main="Grove seal: restore the shrine that stands before it ("+h(40)+" faith), or build a Shrine in a city of yours within "+GateApproach.ShrineReach+" tiles.";}break;
   case 2:c.resource=2;c.main="Obsidian seal: break it with iron (a source of Iron), and expect its guardians to fight harder.";break;
   case 4:if(stone)c.main="Sun seal: the sunstone your people recovered from the ruin is ready to set in its arch.";else{c.gold+=h(80);c.main="Sun seal: set a sunstone in its arch, bought from traders ("+h(80)+" gold)"+(RuinNear(gate)?", or recovered by exploring the ruin within "+GateApproach.RuinReach+" tiles.":".");}break;
   case 8:c.faith+=h(30);c.gold+=h(30);c.main="Rift seal: feed both of its halves ("+h(30)+" faith and "+h(30)+" gold) and choose which to empower: the living side makes the landing fertile, the burning side dries it and shows more of the land beyond.";break;
   default:c.gold+=h(50);c.main="An old seal: pay for the work of breaking it ("+h(50)+" gold).";break;
  }
  switch(near)
  {
   case 5:c.influence+=h(20);c.activation="Arcane gate: its custodians must give leave ("+h(20)+" influence).";break;
   case 7:c.techs.Add("bronze");c.gold+=h(30);c.activation="Steam gate: mend its works with metal (Bronze Working and "+h(30)+" gold).";break;
   case 6:c.techCount=8;c.activation="Sky gate: charge its crystals with learning (know 8 technologies).";break;
   case 3:c.faith+=h(20);c.activation="Void gate: quiet what whispers in it ("+h(20)+" faith).";break;
   default:c.activation="";break;
  }
  // The gateway's condition decides what kind of thing is asked (DimensionGates.cs): only a sealed gateway keeps its seal's demand.
  {var st=FindState(gate);int cond=st==null?GateCondition.Sealed:st.condition;
   if(cond==GateCondition.Dormant||cond==GateCondition.Constructed||cond==GateCondition.Damaged){c=new GateCost();c.gold=h(DimensionSettings.ActivateGold);c.main=(cond==GateCondition.Damaged&&st.repair>0?"Damaged gate: once repaired, ":"Dormant gate: ")+"its own mechanism wakes when it is worked in the right order ("+h(DimensionSettings.ActivateGold)+" gold for the attempt).";}
   else if(cond==GateCondition.Cycling){c=new GateCost();c.gold=h(30);c.main="Cycling gate: it opens by itself. Steady its rhythm and it stays open ("+h(30)+" gold).";}}
  if(first)c.activation+=(c.activation==""?"":" ")+"No gateway has opened yet, so this one asks half and stabilises in "+GateApproach.FirstStabiliseTurns+" turns.";
  return c;
 }
 static int Half(int v){return (v+1)/2;}
 // What of the requirement the owner still lacks, or "" when it can pay. With a partner the gold, faith and influence are halved.
 public string GateLacks(int owner,Tile gate,bool shared=false,int extraGold=0)
 {
  var c=GateNeed(gate,owner);var n=nations[owner];var lacks=new List<string>();int gold=(shared?Half(c.gold):c.gold)+extraGold,faith=shared?Half(c.faith):c.faith,influence=shared?Half(c.influence):c.influence;
  foreach(string id in c.techs)if(!HasTech(owner,id)){int t=TechnologyCatalog.Index(id);lacks.Add(t>=0?TechNames[t]:id);}
  if(c.techCount>0&&KnownTechs(owner)<c.techCount)lacks.Add(c.techCount+" technologies (you know "+KnownTechs(owner)+")");
  if(c.resource>0&&!HasResource(owner,c.resource))lacks.Add("a source of "+ResourceNames[c.resource]);
  if(Treasury(owner)<gold)lacks.Add(gold+" gold ("+Treasury(owner)+")");
  if(n.faith<faith)lacks.Add(faith+" faith ("+n.faith+")");
  if(n.influence<influence)lacks.Add(influence+" influence ("+n.influence+")");
  return lacks.Count==0?"":"Needs "+string.Join(", ",lacks.ToArray())+".";
 }
 int KnownTechs(int owner){int k=0;var t=nations[owner].tech;for(int i=0;i<t.Length;i++)if(t[i])k++;return k;}
 public string GatePrice(Tile gate,bool shared=false,int extraGold=0,int owner=-1){var c=GateNeed(gate,owner);var parts=new List<string>();int gold=(shared?Half(c.gold):c.gold)+extraGold,faith=shared?Half(c.faith):c.faith,influence=shared?Half(c.influence):c.influence;if(gold>0)parts.Add(gold+" gold");if(faith>0)parts.Add(faith+" faith");if(influence>0)parts.Add(influence+" influence");return parts.Count==0?"nothing more":string.Join(", ",parts.ToArray());}
 void PayGate(int owner,Tile gate,bool shared,int extraGold){var c=GateNeed(gate,owner);AddDiplomaticGold(owner,-((shared?Half(c.gold):c.gold)+extraGold));nations[owner].faith-=shared?Half(c.faith):c.faith;nations[owner].influence-=shared?Half(c.influence):c.influence;}
 List<Unit> Holders(int owner,Tile gate){var list=new List<Unit>();foreach(var u in units)if(u.owner==owner&&u.x==gate.x&&u.y==gate.y)list.Add(u);return list;}
 // The guardians of a seal: stronger with the danger beyond, with the age of whoever wakes them, and half as strong again
 // behind an obsidian seal.
 public int GuardianPower(int owner,Tile gate,int danger=0){int p=4+4*(danger>0?danger:GateDanger(gate))+2*Era(owner);int r=GateRealm(gate);if(RealmGateStyle[Math.Max(0,Math.Min(RealmGateStyle.Length-1,r))]==2)p=p*3/2;return p;}
 // The fight a forced seal brings, exactly: each soldier holding the tile takes the same loss, smaller the stronger the garrison.
 public int ForceLoss(int owner,Tile gate,int danger=0){int guard=GuardianPower(owner,gate,danger),garrison=0;foreach(var u in Holders(owner,gate))if(Power[u.type]>0&&!IsNaval(u.type))garrison+=MilitaryValue(u.type)*u.hp/10;if(garrison<=0)return 10;return Math.Max(1,Math.Min(9,(int)Math.Round(10.0*guard/(guard+2.0*garrison))));}
 public int ForceDeaths(int owner,Tile gate,int danger=0){int loss=ForceLoss(owner,gate,danger),n=0;foreach(var u in Holders(owner,gate))if(Power[u.type]>0&&!IsNaval(u.type)&&u.hp<=loss)n++;return n;}
 // Who would share the cost: a civilisation the owner knows, at peace and not a rival, that thinks well enough of it and can pay.
 public List<int> GatePartners(int owner,Tile gate){var list=new List<int>();var c=GateNeed(gate,owner);for(int o=0;o<civCount;o++){if(o==owner||!HasContact(owner,o)||AtWar(owner,o)||Rivals(owner,o)||IsNativeNation(o)&&gatesLocked)continue;if(!cities.Any(x=>x.owner==o))continue;if(Opinion(o,owner)<GateApproach.PartnerOpinion||Treasury(o)<Half(c.gold))continue;list.Add(o);}return list;}
 public string ApproachReason(int owner,Tile gate,int approach,int partner=-1)
 {
  if(gate==null||gate.gateLink<=0)return "Not a gateway.";if(GateSteady(gate))return "This gateway is already open.";if(result!="")return "The game has ended.";
  var s=StudyOf(owner,gate);if(s==null||!s.known)return "Its seal has not been read: keep a Scout, Medic or Priest on it for "+GateApproach.StudyTurns+" turns, or have a city within "+DimensionSettings.InvestigateRange+" tiles investigate it.";
  {var st=FindState(gate);if(st!=null&&st.condition==GateCondition.Damaged&&st.repair>0)return "This gateway is damaged. A city within "+DimensionSettings.InvestigateRange+" tiles must repair it first (the Repair Gateway project, Bronze Working).";}
  if(s.approach!=GateApproach.None)return GateApproach.Names[s.approach]+" is already under way.";
  if(Holders(owner,gate).Count==0)return "One of your units must stand on the gateway.";
  if(approach==GateApproach.Stabilise)return GateLacks(owner,gate,false,GateApproach.StabiliseGold);
  if(approach==GateApproach.Force){if(!Holders(owner,gate).Any(u=>Power[u.type]>0&&!IsNaval(u.type)))return "Its guardians will fight: a soldier must hold the gateway.";return GateLacks(owner,gate);}
  if(approach==GateApproach.Negotiate){if(partner<0||!GatePartners(owner,gate).Contains(partner))return GatePartners(owner,gate).Count==0?"No civilisation you know both thinks well enough of you (opinion "+GateApproach.PartnerOpinion+") and can pay its half.":"Choose a partner.";return GateLacks(owner,gate,true);}
  return "Choose an approach.";
 }
 // What the choice will cost and bring, in words, before it is made.
 public string GateOutlook(int owner,Tile gate,int approach,int partner=-1)
 {
  switch(approach)
  {
   case GateApproach.Stabilise:return "Costs "+GatePrice(gate,false,GateApproach.StabiliseGold,owner)+". The gateway opens in "+StabiliseTurnsNow+" turns if one of your units is still on it. No guardians wake.";
   case GateApproach.Force:{int lo,hi;DangerRange(gate,out lo,out hi);int least=ForceLoss(owner,gate,lo),most=ForceLoss(owner,gate,hi),dead=ForceDeaths(owner,gate,hi);bool any=Holders(owner,gate).Any(u=>Power[u.type]>0&&!IsNaval(u.type));return "Costs "+GatePrice(gate,false,0,owner)+". The gateway opens at once and its guardians (strength "+GuardianPower(owner,gate,lo)+" to "+GuardianPower(owner,gate,hi)+") attack"+(any?": each of your soldiers here loses "+(least==most?most.ToString():least+" to "+most)+" health"+(dead>0?", and at worst "+dead+" would fall.":", and none would fall."):" whoever holds it.");}
   case GateApproach.Negotiate:return "Costs "+GatePrice(gate,true,0,owner)+": "+(partner>=0?NationName(partner):"a partner")+" pays half the gold and gains open borders with you. The gateway opens in "+GateApproach.NegotiateTurns+" turns if one of your units is still on it. No guardians wake.";
  }
  return "";
 }
 public bool BeginApproach(int owner,Tile gate,int approach,int partner=-1,int side=0)
 {
  if(ApproachReason(owner,gate,approach,partner)!="")return false;var s=StudyOf(owner,gate);var c=GateNeed(gate,owner);s.side=side==1?1:0;string who=owner==0?"Your people":NationName(owner);
  if(approach==GateApproach.Stabilise){PayGate(owner,gate,false,GateApproach.StabiliseGold);int wait=StabiliseTurnsNow;s.approach=approach;s.ready=turn+wait;if(owner==0)Note("Work to stabilise the gateway has begun: "+wait+" turns.");return true;}
  if(approach==GateApproach.Negotiate)
  {
   PayGate(owner,gate,true,0);AddDiplomaticGold(partner,-Half(c.gold));s.approach=approach;s.partner=partner;s.ready=turn+GateApproach.NegotiateTurns;
   if(!Pact(owner,partner,Pacts.Borders))SetPact(owner,partner,Pacts.Borders,true);AddOpinion(owner,partner,OpinionMods.Dealings,5);AddOpinion(partner,owner,OpinionMods.Dealings,5);
   DiplomacyEvent(owner,partner,NationName(partner)+" shares the cost of opening a gateway and gains open borders.");return true;
  }
  // Force: pay, fight, open.
  PayGate(owner,gate,false,0);int loss=ForceLoss(owner,gate),fallen=0;
  foreach(var u in Holders(owner,gate)){if(Power[u.type]<=0||IsNaval(u.type))continue;u.hp-=loss;if(u.hp<=0)fallen++;}
  units.RemoveAll(u=>u.hp<=0);
  if(owner==0)Note("The seal is forced and its guardians attack: each soldier on the gateway lost "+loss+" health"+(fallen>0?", and "+fallen+" fell.":"."));
  s.approach=approach;s.ready=turn;FinishApproach(s,gate,who+" forced the seal.");return true;
 }
 void FinishApproach(GateStudy s,Tile gate,string how)
 {
  OpenGate(gate,how);gateStudies.RemoveAll(x=>x.pair==s.pair);ShapeLanding(s,gate);
  // The opener sees the land around the far side, not the whole world beyond.
  var far=GateTwin(gate);if(far!=null){var known=nations[s.owner].known;for(int k=0;k<tiles.Length;k++){var t=tiles[k];if(t.realm!=far.realm||Distance(t.x,t.y,far.x,far.y)>GateApproach.LandingSight)continue;if(known!=null&&k<known.Length)known[k]=true;if(s.owner==0)t.seen=true;}}
 }
 // Each turn: investigators on a sealed gateway read a little more of its seal, and work under way finishes when its time comes.
 internal void GateExpeditionTurn()
 {
  if(gateStudies==null)gateStudies=new List<GateStudy>();if(!gatesLocked)return;
  var counted=new HashSet<long>();
  foreach(var u in units)
  {
   if(!Investigator(u.type))continue;var t=At(u.x,u.y);if(t==null||t.gateLink<=0||GateSteady(t))continue;int pair=GatePair(t);if(!counted.Add((long)pair*100+u.owner))continue;
   var s=StudyFor(u.owner,t);if(s.known)continue;s.turns++;
   if(s.turns>=GateApproach.StudyTurns){s.known=true;if(u.owner==0){Note("Your "+UnitName(u)+" has read the seal of the "+GateStyleNames[GateStyle(t)].ToLower()+": it leads to the "+RealmNames[GateRealm(t)]+". Choose how to open it.");AddGateNotice(t);}}
  }
  foreach(var s in gateStudies.ToArray())
  {
   var gate=GatePairTile(s.pair);
   if(gate!=null&&s.approach==GateApproach.Closing){if(s.ready<=turn)FinishClosing(s,gate);continue;}
   if(gate==null||GateSteady(gate)){gateStudies.Remove(s);continue;}
   // A rival that has read a seal and can pay stabilises it; rivals never force.
   if(s.owner!=0&&s.known&&s.approach==GateApproach.None&&ApproachReason(s.owner,gate,GateApproach.Stabilise)=="")BeginApproach(s.owner,gate,GateApproach.Stabilise);
   if(s.approach==GateApproach.None||s.ready>turn)continue;
   if(Holders(s.owner,gate).Count==0&&Holders(s.owner,GateTwin(gate)??gate).Count==0){if(s.owner==0&&s.ready==turn)Note("The gateway is ready, but none of your units stands on it. It will open when one does.");continue;}
   FinishApproach(s,gate,(s.owner==0?"Your people":NationName(s.owner))+(s.approach==GateApproach.Negotiate&&s.partner>=0?" and "+NationName(s.partner)+" opened the seal together.":" stabilised the seal."));
  }
 }
 // ---------- the rest of the owner's design (11 Nov 2026) ----------
 // The danger beyond is only roughly known until the gateway opens: two neighbouring levels, one of them the truth.
 public void DangerRange(Tile gate,out int lo,out int hi){int d=GateDanger(gate);bool up=GatePair(gate)%2==0;lo=up?d:d-1;hi=up?d+1:d;if(lo<1){lo=1;hi=2;}if(hi>3){hi=3;lo=2;}}
 public string DangerText(Tile gate,int owner=-1){if(owner>=0){var read=StudyOf(owner,gate);if(read!=null&&read.method>=2)return GateApproach.Danger[GateDanger(gate)].ToLower()+" (measured)";}int lo,hi;DangerRange(gate,out lo,out hi);return GateApproach.Danger[lo].ToLower()+" to "+GateApproach.Danger[hi].ToLower();}
 // A Sun seal's sunstone can be recovered instead of bought: any unit of a civilisation that explores a ruin within reach of the gate.
 public bool RuinNear(Tile gate){var home=HomeSide(gate);if(home==null)return false;foreach(var t in tiles)if(t.ruin&&t.realm==home.realm&&Distance(t.x,t.y,home.x,home.y)<=GateApproach.RuinReach)return true;return false;}
 public bool HasSunstone(int owner,Tile gate){var s=StudyOf(owner,gate);return s!=null&&s.sunstone;}
 internal void GateRuinFound(int owner,Tile ruin)
 {
  if(gateStudies==null||!gatesLocked||ruin==null)return;
  foreach(var t in tiles){if(t.gateLink<=0||t.realm!=ruin.realm||GateOpen(t)||FarStyle(t)!=4||HomeSide(t)!=t||Distance(t.x,t.y,ruin.x,ruin.y)>GateApproach.RuinReach)continue;
   var s=StudyFor(owner,t);if(s.sunstone)continue;s.sunstone=true;if(owner==0)Note("Among the ruin's stones your people found a sunstone. The sun gate nearby will take it in place of gold.");}
 }
 // A Grove seal asks for its shrine to be tended: a Shrine in a city of the owner's near the gate does it.
 public bool ShrineNear(int owner,Tile gate){var home=HomeSide(gate);if(home==null)return false;foreach(var c in cities)if(c.owner==owner&&Distance(c.x,c.y,home.x,home.y)<=GateApproach.ShrineReach&&HasBuilding(c,ShrineBuilding))return true;return false;}
 // A Rift seal's opener chose a half. The living side turns the landing to grassland; the burning side dries it to waste and
 // shows the opener the land for five tiles round instead of three.
 void ShapeLanding(GateStudy s,Tile gate)
 {
  if(FarStyle(gate)!=8)return;var far=GateTwin(gate);if(far==null||far.realm==0)far=gate.realm!=0?gate:far;if(far==null)return;var known=nations[s.owner].known;
  for(int k=0;k<tiles.Length;k++){var t=tiles[k];if(t.realm!=far.realm)continue;int away=Distance(t.x,t.y,far.x,far.y);
   if(away<=GateApproach.RiftReach&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.terrain!=7&&!cities.Any(c=>c.x==t.x&&c.y==t.y)){t.terrain=s.side==1?10:0;if(s.side==1){t.farm=false;}}
   if(s.side==1&&away<=GateApproach.FireSight){if(known!=null&&k<known.Length)known[k]=true;if(s.owner==0)t.seen=true;}}
  if(s.owner==0)Note(s.side==1?"The burning side of the rift is fed: the marsh round the far gate dries to ash, and the smoke clears from the land beyond.":"The living side of the rift is fed: the marsh round the far gate grows over into meadow.");
 }
 // Closing a gateway: whoever holds its tile may close it. It takes a full turn, the tile must still be held then, and it cannot
 // be done while an enemy stands beside either side of it (a battle at the gate). A closed gateway is sealed as it was; the
 // civilisation that closed it keeps its reading of the seal.
 bool EnemyAtGate(int owner,Tile gate){foreach(var side in new[]{gate,GateTwin(gate)}){if(side==null)continue;foreach(var u in units)if(u.owner!=owner&&u.owner<civCount&&AtWar(owner,u.owner)&&Distance(u.x,u.y,side.x,side.y)<=1&&At(u.x,u.y).realm==side.realm)return true;}return false;}
 bool HoldsGate(int owner,Tile gate){return Holders(owner,gate).Count>0||Holders(owner,GateTwin(gate)??gate).Count>0;}
 public bool ClosingGate(Tile gate){if(gateStudies==null||gate==null||gate.gateLink<=0)return false;int pair=GatePair(gate);return gateStudies.Any(s=>s.pair==pair&&s.approach==GateApproach.Closing);}
 public string CloseReason(int owner,Tile gate)
 {
  if(gate==null||gate.gateLink<=0)return "Not a gateway.";if(!GateOpen(gate))return "This gateway is sealed.";if(result!="")return "The game has ended.";
  if(!GateSteady(gate))return "It opens and shuts by itself. Steady it before trying to close it.";if(ClosingGate(gate))return "The gateway is already closing.";if(!HoldsGate(owner,gate))return "One of your units must stand on the gateway.";
  if(EnemyAtGate(owner,gate))return "It cannot be closed during a battle at the gate: an enemy stands beside it.";return "";
 }
 public bool BeginClose(int owner,Tile gate)
 {
  if(CloseReason(owner,gate)!="")return false;var home=HomeSide(gate);var s=StudyFor(owner,home);s.known=true;s.approach=GateApproach.Closing;s.ready=turn+1;s.partner=-1;
  if(owner==0)Note("Your people begin to close the gateway. It seals next turn if they still hold it.");return true;
 }
 void FinishClosing(GateStudy s,Tile gate)
 {
  s.approach=GateApproach.None;
  if(!GateSteady(gate))return;
  if(!HoldsGate(s.owner,gate)||EnemyAtGate(s.owner,gate)){if(s.owner==0)Note("The gateway could not be closed: "+(EnemyAtGate(s.owner,gate)?"an enemy stands beside it.":"none of your units holds it."));return;}
  openedGates.Remove(GatePair(gate));gatesLocked=true;
  Note((s.owner==0?"Your people":NationName(s.owner))+" closed the "+GateStyleNames[GateStyle(gate)].ToLower()+" to the "+RealmNames[GateRealm(gate)]+". It is sealed again.");
 }
 // For the checks.
 public void StudyOfForShot(int owner,Tile gate){var s=StudyFor(owner,HomeSide(gate));s.known=true;s.turns=GateApproach.StudyTurns;}
 internal GateStudy StudyOfForCheck(int owner,Tile gate){return StudyFor(owner,HomeSide(gate));}
 internal void SetPactForCheck(int a,int b,int k,bool on){SetPact(a,b,k,on);}
 internal void MakePeaceForCheck(int a,int b){MakePeace(a,b);}
 internal void CompleteBuildingForCheck(City c){CompleteBuilding(c);}
 internal void DimensionsTurnForCheck(int owner){DimensionsTurn(owner);}
 internal void TravelForCheck(Unit u){GatewayTravel(u);}
 internal void OpenGateForCheck(Tile gate){if(!GateOpen(gate))OpenGate(gate);}
 internal void ExploreRuinForCheck(Unit u){ExploreRuin(u);}
 internal void GiveBuildingForCheck(City c,int b){if(c.buildings==null)c.buildings=new List<int>();if(!c.buildings.Contains(b))c.buildings.Add(b);}
 // The gateway tile of a pair: the home side when there is one.
 Tile GatePairTile(int pair){if(pair<0||pair>=tiles.Length)return null;var t=tiles[pair];if(t.gateLink<=0)return null;return HomeSide(t);}
 // Seals just read by the player, for the screen to open the gateway panel on.
 [NonSerialized] public List<int> gateNotices;
 void AddGateNotice(Tile t){if(gateNotices==null)gateNotices=new List<int>();gateNotices.Add(t.y*Width+t.x);}
 bool GateStudiesValid(){if(gateStudies==null)return false;foreach(var s in gateStudies)if(s==null||s.owner<0||s.owner>=civCount||s.pair<0||s.pair>=tiles.Length||s.turns<0||s.approach<0||s.approach>4||s.partner< -1||s.partner>=civCount)return false;return true;}
}
}
