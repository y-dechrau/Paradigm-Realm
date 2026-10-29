using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
public partial class RealmGame {
 int diplomacyPartner=1,diplomacyKind,diplomacyDuration=1,giveResource,askResource;
 string giveGold="0",askGold="0",givePerTurn="0",askPerTurn="0",diplomacyConfirmation="";
 DiplomaticOffer confirmingOffer; int confirmingInbox=-1;
 void ResetDiplomacyUI(){confirmingOffer=null;confirmingInbox=-1;diplomacyConfirmation="";giveGold=askGold=givePerTurn=askPerTurn="0";giveResource=askResource=0;pendingUnit=-1;pendingWars=new int[0];}
 static int DealNumber(string text,int maximum){int value;return int.TryParse(text,out value)?Math.Max(0,Math.Min(maximum,value)):0;}
 void DealField(string label,ref string value){GUILayout.BeginHorizontal();GUILayout.Label(label);value=GUILayout.TextField(value,5);GUILayout.EndHorizontal();}
 void DrawAdvancedDiplomacy(){
  GUILayout.Label("Diplomatic inbox",heading);
  var pending=world.offers.Where(o=>o.to==0&&o.status==0&&o.expires>world.turn).ToList();
  if(pending.Count==0)GUILayout.Label("No pending proposals.",small);
  foreach(var o in pending){GUILayout.Label(world.DescribeOffer(o),body);GUILayout.Label("Respond within "+(o.expires-world.turn)+" turns. "+o.answer,small);GUILayout.BeginHorizontal();if(Button("Review acceptance #"+o.id)){confirmingInbox=o.id;confirmingOffer=null;diplomacyConfirmation="Accept proposal #"+o.id+"?";}if(Button("Decline #"+o.id))world.RespondToOffer(o.id,false);GUILayout.EndHorizontal();}
  if(diplomacyConfirmation!=""){
   GUILayout.Label(diplomacyConfirmation,heading);if(confirmingOffer!=null)GUILayout.Label(world.DescribeOffer(confirmingOffer),body);
   if(Button("Confirm diplomatic action")){if(confirmingInbox>=0)status=world.RespondToOffer(confirmingInbox,true)?"Agreement accepted.":"Agreement unavailable: terms changed or expired.";else if(confirmingOffer!=null)status=world.Propose(confirmingOffer).answer;else if(diplomacyConfirmation.StartsWith("Embargo"))status=world.ImposeEmbargo(0,diplomacyPartner)?"Embargo imposed.":"Embargo unavailable.";else status=world.EndTreaty(0,diplomacyPartner,(TreatyKind)diplomacyKind)?"Agreement ended.":"No such agreement to end.";diplomacyConfirmation="";confirmingInbox=-1;confirmingOffer=null;}
   if(Button("Cancel diplomatic action")){diplomacyConfirmation="";confirmingInbox=-1;confirmingOffer=null;}
   GUILayout.Label(status,body);return;
  }
  var known=Enumerable.Range(1,world.civCount-1).Where(i=>world.met[i]).ToArray();if(known.Length==0){GUILayout.Label("Explore to meet other civilisations.",body);return;}
  for(int row=0;row<known.Length;row+=4){GUILayout.BeginHorizontal();foreach(int other in known.Skip(row).Take(4))if(Button(world.NationName(other)))diplomacyPartner=other;GUILayout.EndHorizontal();}if(!world.met[diplomacyPartner])diplomacyPartner=known[0];int partner=diplomacyPartner;var p=world.Relation(0,partner);
  GUILayout.Label(world.NationName(partner)+" — "+(world.AtWar(0,partner)?"WAR":world.Allied(0,partner)?"ALLIED":world.Friends(0,partner)?"FRIENDS":"PEACE"),heading);
  GUILayout.Label(world.Personality(partner),small);GUILayout.Label(world.RelationshipBreakdown(partner,0),body);
  GUILayout.Label("War weariness: "+world.warWeariness[0]+"; your cities lose "+world.WarUnhappiness(0)+" happiness. Peace recovers 2 weariness/turn.",small);
  GUILayout.Label("Turns remaining — Borders "+Math.Max(0,p.accessUntil-world.turn)+" | Friendship "+Math.Max(0,p.friendUntil-world.turn)+" | Alliance "+Math.Max(0,p.allianceUntil-world.turn)+" | Research "+Math.Max(0,p.researchUntil-world.turn)+" | Truce "+Math.Max(0,p.peaceUntil-world.turn)+" | Embargo "+Math.Max(0,p.embargoUntil-world.turn),body);
  GUILayout.Label("Border promises — You: "+Math.Max(0,p.promiseAUntil-world.turn)+" turns; partner: "+Math.Max(0,p.promiseBUntil-world.turn)+" turns.",small);
  foreach(var c in world.contracts.Where(c=>c.from==0&&c.to==partner||c.to==0&&c.from==partner))GUILayout.Label("Trade #"+c.id+": "+(c.end-world.turn)+" turns remaining; "+(world.ContractActive(c)?"active":"suspended; ends at next payment check")+". "+world.NationName(c.from)+" supplies "+c.givePerTurn+" gold/turn and "+World.ResourceNames[c.giveResource]+"; "+world.NationName(c.to)+" supplies "+c.askPerTurn+" and "+World.ResourceNames[c.askResource]+".",body);
  GUILayout.Label("Negotiate an agreement",heading);
  diplomacyKind=GUILayout.SelectionGrid(diplomacyKind,new[]{"Open borders","Friendship","Defensive alliance","Research pact","Trade","Peace","Border promise","Joint war"},4);
  diplomacyDuration=GUILayout.SelectionGrid(diplomacyDuration,new[]{"10 turns","15 turns","30 turns"},3);
  DealField("You offer gold now",ref giveGold);DealField("You request gold now",ref askGold);
  if(diplomacyKind==4||diplomacyKind==5){DealField("You pay gold/turn (0–20)",ref givePerTurn);DealField("Partner pays gold/turn (0–20)",ref askPerTurn);GUILayout.Label("You share resource access",small);giveResource=GUILayout.SelectionGrid(giveResource,World.ResourceNames,6);GUILayout.Label("You request resource access",small);askResource=GUILayout.SelectionGrid(askResource,World.ResourceNames,6);}
  bool recurring=diplomacyKind==4||diplomacyKind==5;int target=Enumerable.Range(1,world.civCount-1).FirstOrDefault(i=>i!=partner&&world.met[i]);
  var offer=new DiplomaticOffer{from=0,to=partner,kind=(TreatyKind)diplomacyKind,duration=new[]{10,15,30}[diplomacyDuration],giveGold=DealNumber(giveGold,10000),askGold=DealNumber(askGold,10000),givePerTurn=recurring?DealNumber(givePerTurn,20):0,askPerTurn=recurring?DealNumber(askPerTurn,20):0,giveResource=recurring?giveResource:0,askResource=recurring?askResource:0,target=target==0?-1:target};
  GUILayout.Label(world.DescribeOffer(offer),body);GUILayout.Label(world.OfferAssessment(offer),small);
  bool enabled=GUI.enabled;GUI.enabled=enabled&&world.OfferProblem(offer)=="";if(Button("Review proposal")){confirmingOffer=offer;confirmingInbox=-1;diplomacyConfirmation="Send this proposal?";}GUI.enabled=enabled;
  GUILayout.Label("Diplomatic actions",heading);
  if(Button("Establish embassy — 25 gold, once per partner"))status=world.SendEmbassy(0,partner)?"Embassy established; trust +8.":"Requires peace, 25 gold and no existing embassy.";
  if(Button("Denounce — formal war after 3 turns; 8 grievances"))status=world.Denounce(0,partner)?"Denounced.":"Cannot denounce a friend, active war or already-denounced leader.";
  if(Button("Embargo — 10 turns; cancels trade, 12 grievances")){diplomacyConfirmation="Embargo this partner and end economic contracts?";confirmingInbox=-1;confirmingOffer=null;}
  if(Button("End selected agreement — trust -10, grievances +8 or +25")){diplomacyConfirmation="End "+(TreatyKind)diplomacyKind+"? Friendship/alliance also ends its research pact.";confirmingInbox=-1;confirmingOffer=null;}
  string warReason=world.WarReason(0,partner);GUILayout.Label(warReason==""?(world.FormalWarAvailable(0,partner)?"Formal war causes 25 grievances.":"Surprise war causes 45 grievances."):warReason,small);
  GUI.enabled=enabled&&warReason=="";if(Button("Review declaration of war")){pendingUnit=-2;pendingWars=new[]{partner};detailsScreen="";cityScreen=false;GUIUtility.ExitGUI();}GUI.enabled=enabled;
  DrawEmpireDiplomacy(partner);
  GUILayout.Label("Recent diplomatic events",heading);foreach(string line in p.events)GUILayout.Label(line,small);
 }
}
}
