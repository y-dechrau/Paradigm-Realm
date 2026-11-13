using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Peace with terms (owner brief, 13 Nov 2026; save version 43). A peace can now carry, besides gold now, gold by the turn
// (reparations, through the trade-deal contract) and resources:
//  - a city handed over by either side (never a capital);
//  - a withdrawal pledge: the side receiving the offer keeps its soldiers 4 tiles from the other's cities for the truce
//    (it becomes a Withdraw commitment, DiplomacyRequests.cs, so breaking it is a broken word);
//  - a longer truce (10, 20 or 30 turns).
// Demands are judged against the leverage the war has given (Leverage): exhaustion on both sides and the strength each can see.
// An undefeated opponent will not give up a city, and demands beyond what has been won are refused outright. Reparations that
// stop because the payer is bankrupt are a broken word; a resource lost to a cut road ends the deal with no blame.
public partial class World {
 // a's leverage over b in their war, -100..100: how much more tired b is than a, and whether a looks the stronger.
 public int Leverage(int a,int b){if(!AtWar(a,b))return 0;int v=Exhaustion(b,a)-Exhaustion(a,b);int sa=Strength(b,a),sb=Strength(b,b);if(sa*2>=sb*3)v+=15;else if(sb*2>=sa*3)v-=15;return Clamp(v,-100,100);}
 public int CityWorth(City c){return c==null?0:40+10*c.pop;}
 string PeaceTermsProblem(DiplomaticOffer o){
  bool terms=o.giveCity>=0||o.askCity>=0||o.withdraw||o.truce!=0;
  if(o.kind!=TreatyKind.Peace)return terms?"Cities, withdrawals and truces belong in peace terms.":"";
  if(o.truce!=0&&(o.truce<10||o.truce>30))return "A truce lasts 10 to 30 turns.";
  if(o.askCity>=0){var c=CityById(o.askCity);if(c==null||c.owner!=o.to)return "That city is not theirs to give.";if(nations[o.to].capital==c.id)return "A capital cannot be handed over at the peace table.";}
  if(o.giveCity>=0){var c=CityById(o.giveCity);if(c==null||c.owner!=o.from)return "That city is not yours to give.";if(nations[o.from].capital==c.id)return "A capital cannot be handed over at the peace table.";}
  return "";}
 // The demands in an offer, as the recipient values them (what it would lose).
 int DemandValue(DiplomaticOffer o){int v=o.askGold+o.askPerTurn*o.duration*3/4+(o.askResource>0?40:0);if(o.askCity>=0)v+=CityWorth(CityById(o.askCity));if(o.withdraw)v+=20;return v;}
 // Added to the reasons a leader weighs a peace (DiplomacyDeals.Acceptance).
 void PeaceTermLines(DiplomaticOffer o,List<KeyValuePair<string,int>> lines,ref int sum){
  int lev=Leverage(o.from,o.to),theirs=Exhaustion(o.to,o.from);
  if(o.askCity>=0){var c=CityById(o.askCity);Part(lines,ref sum,"Giving up "+(c==null?"a city":c.name),-CityWorth(c)/2);if(theirs<50&&lev<30)Part(lines,ref sum,"Undefeated: they will not cede a city",-100);}
  if(o.giveCity>=0){var c=CityById(o.giveCity);Part(lines,ref sum,"Gaining "+(c==null?"a city":c.name),CityWorth(c)/2);}
  if(o.withdraw)Part(lines,ref sum,"Pledge to keep troops from your cities",lev>=20?-5:-15);
  if(o.truce>Pacts.Truce)Part(lines,ref sum,"A long truce ("+o.truce+" turns)",lev<0?10:-5);
  int demand=DemandValue(o),allowed=Math.Max(0,lev)*4+theirs*2;
  if(demand>0)Part(lines,ref sum,demand>allowed?"Demands beyond what you have won (leverage "+lev+")":"Demands within what you have won (leverage "+lev+")",demand>allowed?-60:0);
  // A peace brokered by a third party's request: the recipient has promised to end this war.
  if(requests!=null&&requests.Any(r=>r.status==RequestStatus.Active&&r.kind==RequestKinds.StopWar&&r.to==o.from&&r.target==o.to))Part(lines,ref sum,"They know you have promised a third party to end this war",20);
  if(requests!=null&&requests.Any(r=>r.status==RequestStatus.Active&&r.kind==RequestKinds.StopWar&&r.to==o.to&&r.target==o.from))Part(lines,ref sum,"They promised a third party to end this war",80);}
 void ApplyPeaceTerms(DiplomaticOffer o){
  var p=Relation(o.from,o.to);if(o.truce>0)p.peaceUntil=Math.Max(p.peaceUntil,turn+o.truce);
  if(o.askCity>=0)CedeCity(CityById(o.askCity),o.from);
  if(o.giveCity>=0)CedeCity(CityById(o.giveCity),o.to);
  if(o.withdraw){if(requests==null)requests=new List<DiplomaticRequest>();var r=new DiplomaticRequest{id=nextDiplomaticId++,from=o.from,to=o.to,kind=RequestKinds.Withdraw,duration=Math.Max(Pacts.Truce,o.truce),created=turn,accepted=turn,deadline=turn+Math.Max(Pacts.Truce,o.truce),status=RequestStatus.Active,answer="Pledged in the peace."};requests.Add(r);DiplomacyEvent(o.from,o.to,"Peace terms: "+NationName(o.to)+" pledges to keep its soldiers from "+NationName(o.from)+"'s cities for "+r.duration+" turns.");}}
 // A city handed over at the peace table: the people stay, its work under way is lost, and it settles in over 2 turns.
 public void CedeCity(City c,int to){
  if(c==null||c.owner==to||to<0||to>=civCount)return;int from=c.owner;LoseProjectCity(c);c.owner=to;c.occupation=2;NormaliseSpecialists(c);
  c.queue.Clear();c.build=-1;c.production=0;c.buildingQueue.Clear();c.buildingProject=-1;c.buildingProduction=0;c.locked=null;
  claims.RemoveAll(x=>x.city==c.id);RebuildTerritory();networks=null;
  DiplomacyEvent(from,to,c.name+" handed over to "+NationName(to)+" at the peace table.");if(from==0||to==0)Note(c.name+" now belongs to "+NationName(to)+".");}
 public string PeaceTermsText(DiplomaticOffer o){
  var parts=new List<string>();
  if(o.askCity>=0){var c=CityById(o.askCity);if(c!=null)parts.Add(NationName(o.to)+" hands over "+c.name);}
  if(o.giveCity>=0){var c=CityById(o.giveCity);if(c!=null)parts.Add(NationName(o.from)+" hands over "+c.name);}
  if(o.withdraw)parts.Add(NationName(o.to)+" keeps its soldiers "+(RequestKinds.WithdrawRange+1)+" tiles from "+NationName(o.from)+"'s cities for the truce");
  parts.Add("truce of "+(o.truce>0?o.truce:Pacts.Truce)+" turns");
  return string.Join("; ",parts.ToArray())+".";}
 // What a rival asks or offers to end a war: reparations and a withdrawal when it has the upper hand, reparations of its own when
 // it is losing badly, a plain peace otherwise. Whatever it sends must be acceptable by the same reckoning the player faces.
 DiplomaticOffer AIPeaceOffer(int a,int b){
  int lev=Leverage(a,b);var plain=new DiplomaticOffer{from=a,to=b,kind=TreatyKind.Peace};
  var tries=new List<DiplomaticOffer>();
  if(lev>=30){int pay=Clamp(Treasury(b)/20,2,5);tries.Add(new DiplomaticOffer{from=a,to=b,kind=TreatyKind.Peace,duration=10,askPerTurn=pay,withdraw=true});tries.Add(new DiplomaticOffer{from=a,to=b,kind=TreatyKind.Peace,duration=10,askPerTurn=Math.Max(1,pay/2)});}
  else if(lev<=-30&&Treasury(a)>=40){tries.Add(new DiplomaticOffer{from=a,to=b,kind=TreatyKind.Peace,duration=10,givePerTurn=Clamp(Treasury(a)/40,1,4),truce=20});}
  foreach(var o in tries)if(OfferProblem(o)==""&&(b==0||OfferWillingness(o)>=0))return o;
  return plain;}
}
}
