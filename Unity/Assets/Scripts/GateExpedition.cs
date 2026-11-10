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
 public int pair,owner,turns,approach,ready,partner=-1; public bool known;
}
public class GateCost {
 public int gold,faith,influence,resource,techCount; public List<string> techs=new List<string>(); public string main="",activation="";
}
public static class GateApproach {
 public const int None=0,Stabilise=1,Force=2,Negotiate=3;
 public static readonly string[] Names={"","Stabilise","Force open","Negotiate access"};
 public const int StudyTurns=2,StabiliseTurns=6,NegotiateTurns=3,StabiliseGold=30,LandingSight=3,PartnerOpinion=10;
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
 public GateCost GateNeed(Tile gate)
 {
  var c=new GateCost();int r=GateRealm(gate);int far=RealmGateStyle[Math.Max(0,Math.Min(RealmGateStyle.Length-1,r))],near=GateStyle(HomeSide(gate));
  switch(far)
  {
   case 0:c.techs.Add("writing");c.gold+=40;c.main="Frost seal: read its frozen inscription (Writing) and bring fuel to thaw it (40 gold).";break;
   case 1:c.faith+=40;c.main="Grove seal: restore the shrine that stands before it (40 faith).";break;
   case 2:c.resource=2;c.main="Obsidian seal: break it with iron (a source of Iron), and expect its guardians to fight harder.";break;
   case 4:c.gold+=80;c.main="Sun seal: set a sunstone in its arch, bought from traders (80 gold).";break;
   case 8:c.faith+=30;c.gold+=30;c.main="Rift seal: feed both of its halves, the living side and the burning side (30 faith and 30 gold).";break;
   default:c.gold+=50;c.main="An old seal: pay for the work of breaking it (50 gold).";break;
  }
  switch(near)
  {
   case 5:c.influence+=20;c.activation="Arcane gate: its custodians must give leave (20 influence).";break;
   case 7:c.techs.Add("bronze");c.gold+=30;c.activation="Steam gate: mend its works with metal (Bronze Working and 30 gold).";break;
   case 6:c.techCount=8;c.activation="Sky gate: charge its crystals with learning (know 8 technologies).";break;
   case 3:c.faith+=20;c.activation="Void gate: quiet what whispers in it (20 faith).";break;
   default:c.activation="";break;
  }
  return c;
 }
 static int Half(int v){return (v+1)/2;}
 // What of the requirement the owner still lacks, or "" when it can pay. With a partner the gold, faith and influence are halved.
 public string GateLacks(int owner,Tile gate,bool shared=false,int extraGold=0)
 {
  var c=GateNeed(gate);var n=nations[owner];var lacks=new List<string>();int gold=(shared?Half(c.gold):c.gold)+extraGold,faith=shared?Half(c.faith):c.faith,influence=shared?Half(c.influence):c.influence;
  foreach(string id in c.techs)if(!HasTech(owner,id)){int t=TechnologyCatalog.Index(id);lacks.Add(t>=0?TechNames[t]:id);}
  if(c.techCount>0&&KnownTechs(owner)<c.techCount)lacks.Add(c.techCount+" technologies (you know "+KnownTechs(owner)+")");
  if(c.resource>0&&!HasResource(owner,c.resource))lacks.Add("a source of "+ResourceNames[c.resource]);
  if(Treasury(owner)<gold)lacks.Add(gold+" gold ("+Treasury(owner)+")");
  if(n.faith<faith)lacks.Add(faith+" faith ("+n.faith+")");
  if(n.influence<influence)lacks.Add(influence+" influence ("+n.influence+")");
  return lacks.Count==0?"":"Needs "+string.Join(", ",lacks.ToArray())+".";
 }
 int KnownTechs(int owner){int k=0;var t=nations[owner].tech;for(int i=0;i<t.Length;i++)if(t[i])k++;return k;}
 public string GatePrice(Tile gate,bool shared=false,int extraGold=0){var c=GateNeed(gate);var parts=new List<string>();int gold=(shared?Half(c.gold):c.gold)+extraGold,faith=shared?Half(c.faith):c.faith,influence=shared?Half(c.influence):c.influence;if(gold>0)parts.Add(gold+" gold");if(faith>0)parts.Add(faith+" faith");if(influence>0)parts.Add(influence+" influence");return parts.Count==0?"nothing more":string.Join(", ",parts.ToArray());}
 void PayGate(int owner,Tile gate,bool shared,int extraGold){var c=GateNeed(gate);AddDiplomaticGold(owner,-((shared?Half(c.gold):c.gold)+extraGold));nations[owner].faith-=shared?Half(c.faith):c.faith;nations[owner].influence-=shared?Half(c.influence):c.influence;}
 List<Unit> Holders(int owner,Tile gate){var list=new List<Unit>();foreach(var u in units)if(u.owner==owner&&u.x==gate.x&&u.y==gate.y)list.Add(u);return list;}
 // The guardians of a seal: stronger with the danger beyond, with the age of whoever wakes them, and half as strong again
 // behind an obsidian seal.
 public int GuardianPower(int owner,Tile gate){int p=4+4*GateDanger(gate)+2*Era(owner);int r=GateRealm(gate);if(RealmGateStyle[Math.Max(0,Math.Min(RealmGateStyle.Length-1,r))]==2)p=p*3/2;return p;}
 // The fight a forced seal brings, exactly: each soldier holding the tile takes the same loss, smaller the stronger the garrison.
 public int ForceLoss(int owner,Tile gate){int guard=GuardianPower(owner,gate),garrison=0;foreach(var u in Holders(owner,gate))if(Power[u.type]>0&&!IsNaval(u.type))garrison+=MilitaryValue(u.type)*u.hp/10;if(garrison<=0)return 10;return Math.Max(1,Math.Min(9,(int)Math.Round(10.0*guard/(guard+2.0*garrison))));}
 public int ForceDeaths(int owner,Tile gate){int loss=ForceLoss(owner,gate),n=0;foreach(var u in Holders(owner,gate))if(Power[u.type]>0&&!IsNaval(u.type)&&u.hp<=loss)n++;return n;}
 // Who would share the cost: a civilisation the owner knows, at peace and not a rival, that thinks well enough of it and can pay.
 public List<int> GatePartners(int owner,Tile gate){var list=new List<int>();var c=GateNeed(gate);for(int o=0;o<civCount;o++){if(o==owner||!HasContact(owner,o)||AtWar(owner,o)||Rivals(owner,o)||IsNativeNation(o)&&gatesLocked)continue;if(!cities.Any(x=>x.owner==o))continue;if(Opinion(o,owner)<GateApproach.PartnerOpinion||Treasury(o)<Half(c.gold))continue;list.Add(o);}return list;}
 public string ApproachReason(int owner,Tile gate,int approach,int partner=-1)
 {
  if(gate==null||gate.gateLink<=0)return "Not a gateway.";if(GateOpen(gate))return "This gateway is already open.";if(result!="")return "The game has ended.";
  var s=StudyOf(owner,gate);if(s==null||!s.known)return "Its seal has not been read: keep a Scout, Medic or Priest on it for "+GateApproach.StudyTurns+" turns.";
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
   case GateApproach.Stabilise:return "Costs "+GatePrice(gate,false,GateApproach.StabiliseGold)+". The gateway opens in "+GateApproach.StabiliseTurns+" turns if one of your units is still on it. No guardians wake.";
   case GateApproach.Force:{int loss=ForceLoss(owner,gate),dead=ForceDeaths(owner,gate);bool any=Holders(owner,gate).Any(u=>Power[u.type]>0&&!IsNaval(u.type));return "Costs "+GatePrice(gate)+". The gateway opens at once and its guardians (strength "+GuardianPower(owner,gate)+") attack"+(any?": each of your soldiers here loses "+loss+" health"+(dead>0?", and "+dead+" would fall.":", and none would fall."):" whoever holds it.");}
   case GateApproach.Negotiate:return "Costs "+GatePrice(gate,true)+": "+(partner>=0?NationName(partner):"a partner")+" pays half the gold and gains open borders with you. The gateway opens in "+GateApproach.NegotiateTurns+" turns if one of your units is still on it. No guardians wake.";
  }
  return "";
 }
 public bool BeginApproach(int owner,Tile gate,int approach,int partner=-1)
 {
  if(ApproachReason(owner,gate,approach,partner)!="")return false;var s=StudyOf(owner,gate);var c=GateNeed(gate);string who=owner==0?"Your people":NationName(owner);
  if(approach==GateApproach.Stabilise){PayGate(owner,gate,false,GateApproach.StabiliseGold);s.approach=approach;s.ready=turn+GateApproach.StabiliseTurns;if(owner==0)Note("Work to stabilise the gateway has begun: "+GateApproach.StabiliseTurns+" turns.");return true;}
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
  OpenGate(gate,how);gateStudies.RemoveAll(x=>x.pair==s.pair);
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
   if(!Investigator(u.type))continue;var t=At(u.x,u.y);if(t==null||t.gateLink<=0||GateOpen(t))continue;int pair=GatePair(t);if(!counted.Add((long)pair*100+u.owner))continue;
   var s=StudyFor(u.owner,t);if(s.known)continue;s.turns++;
   if(s.turns>=GateApproach.StudyTurns){s.known=true;if(u.owner==0){Note("Your "+UnitName(u)+" has read the seal of the "+GateStyleNames[GateStyle(t)].ToLower()+": it leads to the "+RealmNames[GateRealm(t)]+". Choose how to open it.");AddGateNotice(t);}}
  }
  foreach(var s in gateStudies.ToArray())
  {
   var gate=GatePairTile(s.pair);if(gate==null||GateOpen(gate)){gateStudies.Remove(s);continue;}
   // A rival that has read a seal and can pay stabilises it; rivals never force.
   if(s.owner!=0&&s.known&&s.approach==GateApproach.None&&ApproachReason(s.owner,gate,GateApproach.Stabilise)=="")BeginApproach(s.owner,gate,GateApproach.Stabilise);
   if(s.approach==GateApproach.None||s.ready>turn)continue;
   if(Holders(s.owner,gate).Count==0&&Holders(s.owner,GateTwin(gate)??gate).Count==0){if(s.owner==0&&s.ready==turn)Note("The gateway is ready, but none of your units stands on it. It will open when one does.");continue;}
   FinishApproach(s,gate,(s.owner==0?"Your people":NationName(s.owner))+(s.approach==GateApproach.Negotiate&&s.partner>=0?" and "+NationName(s.partner)+" opened the seal together.":" stabilised the seal."));
  }
 }
 // The gateway tile of a pair: the home side when there is one.
 Tile GatePairTile(int pair){if(pair<0||pair>=tiles.Length)return null;var t=tiles[pair];if(t.gateLink<=0)return null;return HomeSide(t);}
 // Seals just read by the player, for the screen to open the gateway panel on.
 [NonSerialized] public List<int> gateNotices;
 void AddGateNotice(Tile t){if(gateNotices==null)gateNotices=new List<int>();gateNotices.Add(t.y*Width+t.x);}
 bool GateStudiesValid(){if(gateStudies==null)return false;foreach(var s in gateStudies)if(s==null||s.owner<0||s.owner>=civCount||s.pair<0||s.pair>=tiles.Length||s.turns<0||s.approach<0||s.approach>3||s.partner< -1||s.partner>=civCount)return false;return true;}
}
}
