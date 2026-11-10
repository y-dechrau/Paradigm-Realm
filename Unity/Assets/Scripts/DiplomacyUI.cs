using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The "More" page of the diplomacy screen: the inbox of proposals waiting for an answer, every reason behind the leader's opinion,
// the leader's character, trade deals in force, the buying of technology and the record of what has passed between the two.
public partial class RealmGame {
 int diplomacyPartner=1,diplomacyDuration=1,giveResource,askResource;
 string giveGold="0",askGold="0",givePerTurn="0",askPerTurn="0";
 void ResetDiplomacyUI(){giveGold=askGold=givePerTurn=askPerTurn="0";giveResource=askResource=0;customDeal=false;claimsView=false;pendingUnit=-1;pendingWars=new int[0];}
 static int DealNumber(string text,int maximum){int value;return int.TryParse(text,out value)?Math.Max(0,Math.Min(maximum,value)):0;}
 void DrawAdvancedDiplomacy(){
  GUILayout.Label("Proposals awaiting your answer",heading);
  var pending=world.offers.Where(o=>o.to==0&&o.status==0&&o.expires>world.turn).ToList();
  if(pending.Count==0)GUILayout.Label("None.",small);
  foreach(var o in pending){GUILayout.Label(world.DescribeOffer(o),body);GUILayout.Label("Answer within "+(o.expires-world.turn)+" turns.",small);GUILayout.BeginHorizontal();
   if(Button("Accept #"+o.id))status=world.RespondToOffer(o.id,true)?"Agreement accepted.":"Agreement unavailable: terms changed or expired.";
   if(Button("Decline #"+o.id))world.RespondToOffer(o.id,false);GUILayout.EndHorizontal();}
  var known=Enumerable.Range(1,world.civCount-1).Where(i=>world.met[i]).ToArray();if(known.Length==0){GUILayout.Label("Explore to meet other civilisations.",body);return;}
  if(!world.met[diplomacyPartner])diplomacyPartner=known[0];int partner=diplomacyPartner;var p=world.Relation(0,partner);var pn=world.nations[partner];int attitude=world.Attitude(partner,0);
  GUILayout.Label(world.NationName(partner)+" — "+Attitudes.Names[attitude],heading);
  GUILayout.Label(Attitudes.Texts[attitude]+"  "+(string.IsNullOrEmpty(pn.leader)?"":"Led by "+pn.leader+".  ")+(pn.traits!=null&&pn.traits.Any(t=>t>=0)?CivilisationCatalog.TraitLine(pn.traits):""),body);
  GUILayout.Label(world.RelationshipBreakdown(partner,0),body);
  GUILayout.Label("Their strength beside yours: "+Attitudes.Power[world.PowerLevel(0,partner)]+". Their fear of you: "+world.Threat(partner,0)+"; yours of them: "+world.Threat(0,partner)+". Posture: "+DiplomacyFlavours.PostureNames[world.Posture(partner)]+" — "+DiplomacyFlavours.PostureTexts[world.Posture(partner)],small);
  {var flavours=Enumerable.Range(0,DiplomacyFlavours.Names.Length).Select(k=>DiplomacyFlavours.Names[k]+" "+world.Flavour(partner,k)).ToArray();GUILayout.Label("Character (1-10): "+string.Join(", ",flavours)+".  "+world.Personality(partner),small);}
  {string agenda=world.AgendaReason(partner,0);if(agenda!="")GUILayout.Label(agenda,small);}
  GUILayout.Label("Agreements and war",heading);
  if(world.AtWar(0,partner))GUILayout.Label("At war since turn "+p.warStarted+". Your exhaustion "+world.Exhaustion(0,partner)+", theirs "+world.Exhaustion(partner,0)+" (of 100). Your cities each lose "+world.WarUnhappiness(0)+" content citizens to your worst war.",body);
  else{var held=Enumerable.Range(0,Pacts.Count).Where(k=>world.Pact(0,partner,k)).Select(k=>Pacts.Names[k]+" ("+world.PactAge(0,partner,k)+" turns)").ToArray();GUILayout.Label(held.Length==0?"No agreements.":string.Join("  •  ",held),body);
   if(p.peaceUntil>world.turn)GUILayout.Label("Truce: no war for "+(p.peaceUntil-world.turn)+" more turns.",small);
   if(world.Rival(0,partner))GUILayout.Label("You have named them a rival.",small);if(world.Rival(partner,0))GUILayout.Label("They have named you a rival.",small);
   string cause=world.CasusBelli(0,partner),theirs=world.CasusBelli(partner,0);GUILayout.Label("Your just cause for war: "+(cause==""?"none":cause)+".  Theirs: "+(theirs==""?"none":theirs)+".",small);}
  foreach(var c in world.contracts.Where(c=>c.from==0&&c.to==partner||c.to==0&&c.from==partner)){GUILayout.BeginHorizontal();GUILayout.Label("Trade deal #"+c.id+": "+(c.end-world.turn)+" turns left; "+(world.ContractActive(c)?"active":"suspended")+". "+world.NationName(c.from)+" gives "+c.givePerTurn+" gold a turn and "+World.ResourceNames[c.giveResource]+"; "+world.NationName(c.to)+" gives "+c.askPerTurn+" and "+World.ResourceNames[c.askResource]+".",body);GUILayout.EndHorizontal();
   if(Button("Break trade deal #"+c.id+" (remembered as a broken word)")){world.EndTreaty(0,partner,TreatyKind.Trade);break;}}
  GUILayout.Label("Technology",heading);
  DrawEmpireDiplomacy(partner);
  GUILayout.Label("What has passed between you",heading);if(p.events.Count==0)GUILayout.Label("Nothing yet.",small);foreach(string line in p.events)GUILayout.Label(line,small);
 }
}
}
