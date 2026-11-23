using System;
using System.Linq;
namespace ParadigmRealm {
// The negotiation brief's scenarios (23 Nov 2026): a counteroffer accepted once, stale offers failing cleanly, a passage
// agreement that really opens the border, survives saving, expires on its stated turn and escorts units out, gate transit,
// and fulfilment and breach leaving different memories that change a later answer.
public static class NegotiationChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Negotiation: "+what);}
 static World Fresh(int seed){var w=World.New(seed,"Envoys",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.met[1]=true;w.Relation(0,1).contact=true;w.nations[0].gold=500;w.gold=500;return w;}
 static Tile TheirLand(World w){var c=w.cities.First(x=>x.owner==1);return Enumerable.Range(0,6).Select(k=>w.Step(c.x,c.y,World.HexDi[k],World.HexDj[k])).First(t=>t!=null&&t.owner==1&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));}
 static void Quiet(World w){foreach(int o in Enumerable.Range(1,w.civCount-1))w.Relation(0,o).peaceUntil=w.turn+10;}
 static DiplomaticOffer Ask(int gold=0){return new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,duration=15,askPassage=true,giveGold=gold};}
 public static void Run(){
  // 1-5. Counteroffer, acceptance once, a real open border, save and load, expiry on the stated turn.
  {var w=Fresh(401);var land=TheirLand(w);
   Check(!w.CanEnter(0,land)&&w.EnterReason(0,land).Contains("closed"),"their border is closed, and says why ("+w.EnterReason(0,land)+")");
   var ask=Ask();
   Check(w.OfferProblem(ask)==""&&w.OfferWillingness(ask)<0,"passage for nothing is refused ("+w.OfferProblem(ask)+" "+w.OfferWillingness(ask)+")");
   Check(w.OfferWillingness(ask)==w.OfferWillingness(ask.Copy()),"the same offer gets the same answer");
   var sent=w.Propose(ask);Check(sent.status==2&&sent.answer!=""&&!sent.answer.Contains("("),"the refusal gives a reason in words, with no hidden number ("+sent.answer+")");
   var counter=w.offers.FirstOrDefault(o=>o.from==1&&o.to==0&&o.status==0&&o.counterOf==sent.id);
   Check(counter!=null&&counter.askGold>0&&(counter.changed&1)!=0&&counter.givePassage,"the ruler names a price: a counteroffer with the gold marked as changed");
   Check(w.HistoryFor(1).Any(r=>r.kind==RecordKinds.Counter),"the counteroffer is in the history");
   int gold=w.nations[0].gold,theirs=w.nations[1].gold,price=counter.askGold;
   Check(w.RespondToOffer(counter.id,true),"the counteroffer can be accepted");
   Check(w.nations[0].gold==gold-price&&w.nations[1].gold==theirs+price,"the gold changes hands ("+price+")");
   Check(!w.RespondToOffer(counter.id,true)&&w.nations[0].gold==gold-price,"accepting twice pays once");
   var deal=w.PassageAgreement(0,1);Check(deal!=null&&deal.start==w.turn&&deal.end==w.turn+15,"a passage agreement is in force until the stated turn");
   Check(w.CanEnter(0,land)&&w.EnterReason(0,land)=="","the border is open to the guest");
   var mine=w.cities.First(c=>c.owner==0);Check(!w.PassageTile(1,w.At(mine.x,mine.y)),"it opens one way only");
   var scout=w.Add(0,3,land.x,land.y);
   var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));Check(copy.Valid(),"a save with an agreement is valid");
   Check(copy.PassageAgreement(0,1)!=null&&copy.PassageAgreement(0,1).end==deal.end&&copy.CanEnter(0,copy.At(land.x,land.y)),"the agreement survives saving and loading");
   int end=deal.end;while(w.turn<end-1&&w.result==""){Quiet(w);w.EndTurn();}
   Check(w.turn==end-1&&w.AgreementActive(deal)&&w.CanEnter(0,land),"it holds until the turn before it ends (turn "+w.turn+" of "+end+")");
   if(!w.units.Contains(scout))scout=w.Add(0,3,land.x,land.y);scout.x=land.x;scout.y=land.y;Quiet(w);w.EndTurn();
   Check(w.turn==end&&deal.status==1&&!w.CanEnter(0,land),"it expires when the stated turn begins, and the border closes again");
   Check(!w.units.Contains(scout)||w.At(scout.x,scout.y).owner!=1,"the unit inside is escorted out");
   Check(w.HistoryFor(1).Any(r=>r.kind==RecordKinds.Fulfilled)&&w.OpinionMod(1,0,OpinionMods.KeptWord)>0,"a fulfilled agreement is recorded and remembered");
   Check(w.EnterReason(0,land).Contains("expired"),"the closed border names the agreement that ended ("+w.EnterReason(0,land)+")");
   while(copy.turn<end&&copy.result==""){Quiet(copy);copy.EndTurn();}
   Check(copy.agreements.Count(g=>g.status==1)==1&&copy.diplomaticHistory.Count(r=>r.kind==RecordKinds.Fulfilled)==1,"the loaded game expires it once, on the same turn");}
  // 6. Unaffordable and stale proposals fail cleanly.
  {var w=Fresh(402);int gold=w.nations[0].gold;
   var sent=w.Propose(Ask(5000));Check(sent.status==2&&sent.answer.Contains("treasury")&&w.nations[0].gold==gold&&w.agreements.Count==0,"an unaffordable offer changes nothing");
   w.Propose(Ask());var counter=w.offers.FirstOrDefault(o=>o.from==1&&o.status==0);
   Check(counter!=null,"a counteroffer to go stale");w.nations[0].gold=0;w.gold=0;int theirs=w.nations[1].gold;
   Check(!w.RespondToOffer(counter.id,true)&&w.nations[1].gold==theirs&&w.agreements.Count==0&&counter.status==2,"a counteroffer that can no longer be paid fails with nothing transferred");}
  // 7. Fulfilment and breach leave different memories that change the next answer.
  {var w=Fresh(403);int before=w.OfferWillingness(Ask());w.Propose(Ask(400));var g=w.PassageAgreement(0,1);
   Check(g!=null,"a generous offer is accepted outright");Check(w.BreakAgreement(g.id,0)==""&&g.status==3,"the guest can go back on it");
   Check(w.HistoryFor(1).Any(r=>r.kind==RecordKinds.Breach)&&w.OpinionMod(1,0,OpinionMods.Broken)<0,"a breach is recorded as a breach");
   int after=w.OfferWillingness(Ask());Check(after<before,"a broken word makes the next answer harder ("+before+" to "+after+")");
   var w2=Fresh(403);w2.Propose(Ask(400));var g2=w2.PassageAgreement(0,1);Check(g2!=null,"the same deal in a twin world");
   Check(w2.CancelByAgreement(g2.id,0)==""&&g2.status==2&&w2.OpinionMod(1,0,OpinionMods.Broken)==0,"ending it by agreement leaves no mark");
   Check(w2.HistoryFor(1).Any(r=>r.kind==RecordKinds.Cancelled),"and is recorded as cancelled, not broken");
   var w3=Fresh(403);w3.Propose(Ask(400));var g3=w3.PassageAgreement(0,1);
   while(w3.turn<g3.end&&w3.result==""){Quiet(w3);w3.EndTurn();}int kept=w3.OfferWillingness(Ask());
   Check(g3.status==1&&w3.OpinionMod(1,0,OpinionMods.KeptWord)>0,"a kept agreement is remembered as kept");Check(kept>after,"a kept word serves better than a broken one ("+kept+" against "+after+")");}
  // 8. Gate transit: only the road to the gateway opens, and it lapses when the host loses the gateway.
  {var w=World.New(404,"Gates",0,1,1,0,1,2,3,hex:true,rations:true);w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.met[1]=true;w.Relation(0,1).contact=true;w.nations[0].gold=900;w.gold=900;
   var gate=w.tiles.FirstOrDefault(t=>t.realm==0&&t.gateLink>0);
   if(gate!=null){int cell=gate.y*w.Width+gate.x;var their=w.cities.First(c=>c.owner==1);var far=w.At(their.x,their.y);gate.seen=false;
    Check(w.OfferProblem(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,askGate=cell+1,giveGold=10})!="","a gateway they do not hold cannot be asked for");
    gate.owner=1;Check(w.OfferProblem(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,askGate=cell+1,giveGold=10}).Contains("found"),"nor one you have not found");
    gate.seen=true;Check(!w.CanEnter(0,gate)&&w.EnterReason(0,gate).Contains("gateway"),"their gateway is closed and says what is needed");
    var sent=w.Propose(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,duration=10,askGate=cell+1,giveGold=600});var g=w.PassageAgreement(0,1,cell+1);
    Check(g!=null,"gate transit can be bought ("+sent.answer+")");Check(w.CanEnter(0,gate),"the gateway can be reached");
    if(w.Distance(far.x,far.y,gate.x,gate.y)>World.GateRoadReach)Check(!w.CanEnter(0,far)&&w.EnterReason(0,far).Contains("only the road"),"the rest of their land stays closed, and says so");
    gate.owner=-1;w.AgreementsTurnForChecks();Check(g.status==6&&w.OpinionMod(1,0,OpinionMods.Broken)==0,"it lapses, with no blame, when they no longer hold the gateway");}}
  // 9. History shows only civilisations you have met; older saves load.
  {var w=Fresh(405);w.Record(RecordKinds.War,1,2,"A war between strangers.");Check(!w.HistoryFor().Any(r=>r.text.Contains("strangers")),"dealings of a civilisation you have not met stay hidden");
   w.met[2]=true;Check(w.HistoryFor().Any(r=>r.text.Contains("strangers")),"a war between two you know is public");
   var old=World.New(406,"Old",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);old.agreements=null;old.diplomaticHistory=null;Check(old.Valid()&&old.HistoryFor().Count()==0&&old.PassageAgreement(0,1)==null,"a game from before agreements loads safely");}
 }
}
public partial class World {internal void AgreementsTurnForChecks(){AgreementsTurn();}}
}
