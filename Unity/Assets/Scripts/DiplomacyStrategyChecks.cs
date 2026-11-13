using System;
using System.Linq;
namespace ParadigmRealm {
// Checks of the 13 Nov 2026 diplomacy brief (DiplomacyMemory.cs, DiplomacyPeace.cs, DiplomacyRequests.cs): the acceptance
// scenarios that the rules can show without a human playing. Run from DiplomacyChecks.Run (harness and Unity).
public static class DiplomacyStrategyChecks {
 static void Check(bool yes,string why){if(!yes)throw new Exception("Diplomacy strategy: "+why);}
 static World Setup(int seed=613){var w=World.New(seed,natives:false);w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.met[1]=w.met[2]=true;w.Relation(1,2).contact=true;w.gold=500;foreach(var n in w.nations)n.gold=500;for(int o=1;o<w.civCount;o++)w.nations[o].civ=-1;return w;}
 static void Next(World w,int turns=1){for(int i=0;i<turns;i++){w.turn++;w.AdvanceDiplomacy();}}
 static void Bond(World w,int a,int b,int k,int age){var p=w.Relation(a,b);p.pacts|=1<<k;p.pactSince[k]=w.turn-age;if(a==0||b==0)w.openBorders[a==0?b:a]=(p.pacts&(1<<Pacts.Borders|1<<Pacts.Defensive))!=0;}
 // A second city for owner on free land, or null.
 // An army for owner in its capital, in sight: the military success that gives leverage.
 // The loser's soldiers are gone: the military defeat that gives the winner leverage.
 // and the winner's army stands at the gates of the city in question, where the loser can see it.
 static void Beaten(World w,int loser,int winner,City at){w.units.RemoveAll(u=>u.owner==loser&&World.Power[u.type]>0);var spot=w.tiles.First(t=>t.realm==0&&w.Distance(t.x,t.y,at.x,at.y)==2&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));for(int i=0;i<6;i++)w.Add(winner,2,spot.x,spot.y);}
 // A road from owner's capital to a city, on owner's land, as its workers would build.
 static void Connect(World w,int owner,City to){var home=w.cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).First();var walker=w.Add(owner,1,home.x,home.y);var path=w.Route(walker,to.x,to.y,true);w.units.Remove(walker);w.At(home.x,home.y).road=true;foreach(var t in path){t.road=true;t.owner=owner;}w.RebuildTerritory();foreach(var t in path)t.owner=owner;}
 static City SecondCity(World w,int owner){var home=w.cities.First(c=>c.owner==owner);
  foreach(var t in w.tiles.Where(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&t.terrain!=8&&t.terrain!=13&&t.terrain!=14&&t.terrain!=15&&t.owner<0&&t.gateLink==0).OrderBy(t=>w.Distance(t.x,t.y,home.x,home.y))){
   if(w.Distance(t.x,t.y,home.x,home.y)<5||w.cities.Any(c=>w.Distance(c.x,c.y,t.x,t.y)<5)||w.units.Any(u=>u.x==t.x&&u.y==t.y))continue;var s=w.Add(owner,0,t.x,t.y);var c=w.Found(s);if(c!=null)return c;w.units.Remove(s);}
  return null;}
 public static void Run(){
  // ---- Memory: gifts cannot wash out a betrayal; the betrayal fades in the end; a long friendship steadies opinion. ----
  {var w=Setup();w.turn=50;Bond(w,0,1,Pacts.Defensive,40);int before=w.Opinion(1,0);Check(w.LongFriendship(1,0)==5&&w.OpinionLines(1,0).Any(l=>l.Key=="A long friendship"&&l.Value==5),"forty turns of a pact are worth +5 as a long friendship");
   Check(w.EndTreaty(0,1,TreatyKind.Defensive)&&w.OpinionMod(1,0,OpinionMods.Broken)==-25,"ending a defensive pact is remembered as a broken word");
   for(int i=0;i<6;i++){w.gold=500;w.GiftGold(1);}Check(w.OpinionMod(1,0,OpinionMods.Gifts)<=OpinionMods.GiftCap,"gifts never add more than +"+OpinionMods.GiftCap);
   Check(w.Opinion(1,0)<before,"six gifts do not undo a broken defensive pact");
   var mem=w.MemoryLines(1,0);Check(mem.Any(l=>l.text=="Broke an agreement"&&l.turnsLeft==100),"the screen can say how long the broken word will be remembered (25 points at 1 every 4 turns)");
   Next(w,101);Check(w.OpinionMod(1,0,OpinionMods.Broken)==0,"and in time it is forgiven: betrayal is lasting but recoverable");
   Check(w.ImproveTips(1,0).Count>0,"the player is told how to mend relations");}
  // ---- Scenario 1: a defensive war ends with reparations and a withdrawal, not annexation. ----
  {var w=Setup(614);int cities=w.cities.Count;Check(w.DeclareWar(1,0)&&w.AtWar(0,1)&&w.Relation(0,1).lastDeclarer==1,"rival 1 attacks the player");
   w.turn+=8;w.AddExhaustion(1,0,70);
   var early=new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Peace,askCity=w.nations[1].capital};Check(w.OfferProblem(early).Contains("capital"),"a capital is never handed over at the peace table");
   var greedy=new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Peace,duration=30,askGold=400,askPerTurn=20};Check(w.OfferProblem(greedy)==""&&w.OfferWillingness(greedy)<0&&w.AcceptanceLines(greedy).Any(l=>l.Key.StartsWith("Demands beyond")),"absurd demands are refused as beyond what was won");
   var terms=new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Peace,duration=10,askPerTurn=3,withdraw=true,truce=20};
   Check(w.Leverage(0,1)>=30&&w.OfferProblem(terms)==""&&w.OfferWillingness(terms)>=0,"the beaten attacker accepts reparations and a withdrawal ("+w.OfferWillingness(terms)+")");
   var sent=w.Propose(terms);Check(sent.status==1&&!w.AtWar(0,1),"peace is made on those terms");
   Check(w.contracts.Any(c=>c.from==0&&c.to==1&&c.askPerTurn==3)&&w.requests.Any(r=>r.kind==RequestKinds.Withdraw&&r.from==0&&r.to==1&&r.status==RequestStatus.Active)&&w.Relation(0,1).peaceUntil>=w.turn+20,"reparations run as a contract, the withdrawal is a commitment, and the truce is 20 turns");
   Check(w.cities.Count==cities&&w.cities.Count(c=>c.owner==0)==1,"no city changed hands");Check(w.Valid(),"the game stays valid with terms in force");
   int g=w.gold;w.ProcessDiplomacyPayments();Check(w.gold==g+3,"reparations are paid each turn");}
  // ---- Leverage lets a victor take a city, but not from an undefeated enemy. ----
  {var w=Setup(615);var second=SecondCity(w,1);Check(second!=null,"room for a second rival city");{w.DeclareWar(1,0);w.turn+=8;
    var ask=new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Peace,askCity=second.id};Check(w.OfferProblem(ask)==""&&w.AcceptanceLines(ask).Any(l=>l.Key.StartsWith("Undefeated")),"an undefeated enemy will not cede a city");
    w.AddExhaustion(1,0,90);Beaten(w,1,0,second);var p=w.Propose(ask);Check(p.status==1&&second.owner==0&&second.occupation==2,"a beaten enemy hands the city over at the peace table: "+p.answer+" / "+string.Join("; ",w.AcceptanceLines(ask).Select(l=>l.Key+" "+l.Value)));}}
  // ---- Scenario 2: a long-standing ally provides feasible help. ----
  {var w=Setup(616);w.turn=60;Bond(w,0,1,Pacts.Defensive,50);w.SetTrust(1,0,60);w.DeclareWar(0,2);
   int trust=w.Trust(1,0);var aid=w.ProposeRequest(new DiplomaticRequest{from=0,to=1,kind=RequestKinds.Aid,aid=40,duration=5});
   Check(aid.status==RequestStatus.Fulfilled&&w.OpinionMod(1,0,OpinionMods.KeptWord)==0,"an ally sends the aid asked for (the recipient's word is kept, the asker owes nothing)");
   var join=w.ProposeRequest(new DiplomaticRequest{from=0,to=1,kind=RequestKinds.JoinWar,target=2,duration=5,gold=30});
   Check(join.status==RequestStatus.Fulfilled&&w.AtWar(1,2)||join.status==RequestStatus.Refused&&join.answer.Contains("instead"),"an ally either joins the war or, if it cannot, sends gold instead and says why ("+join.answer+")");
   Check(w.Valid(),"valid after allied help");}
  // ---- The player helps an ally at a gateway and is trusted more for it. ----
  {var w=Setup(617);var gate=w.HomeGates().FirstOrDefault();Check(gate!=null,"the test map has a gateway");{
    w.turn=40;Bond(w,0,1,Pacts.Defensive,20);foreach(var t in w.tiles)if(t.realm==gate.realm&&w.Distance(t.x,t.y,gate.x,gate.y)<=3)t.owner=1;
    var r=w.ProposeRequest(new DiplomaticRequest{from=1,to=0,kind=RequestKinds.DefendGate,gate=gate.y*w.Width+gate.x,duration=6,gold=30});Check(r.status==RequestStatus.Pending,"an ally's request for help at its gateway reaches the player ("+r.answer+")");
    Check(w.RespondToRequest(r.id,true)&&r.status==RequestStatus.Active,"the player accepts a specific commitment");
    var post=w.tiles.First(t=>t.realm==gate.realm&&w.Distance(t.x,t.y,gate.x,gate.y)==1&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));var guard=w.Add(0,2,post.x,post.y);
    int trust=w.Trust(1,0),gold=w.gold;Next(w,6);Check(r.status==RequestStatus.Fulfilled&&w.gold==gold+30&&w.OpinionMod(1,0,OpinionMods.KeptWord)>0&&w.TrustCap(1,0)==Pacts.TrustCap[Pacts.Defensive],"guarding the gateway fulfils the commitment, pays the incentive and is remembered");
    var r2=w.ProposeRequest(new DiplomaticRequest{from=1,to=0,kind=RequestKinds.DefendGate,gate=gate.y*w.Width+gate.x,duration=6});w.RespondToRequest(r2.id,true);w.units.Remove(guard);int broken=w.OpinionMod(1,0,OpinionMods.Broken);Next(w,6);
    Check(r2.status==RequestStatus.Failed&&w.OpinionMod(1,0,OpinionMods.Broken)<broken&&w.OpinionMod(1,0,OpinionMods.Abandoned)<0,"leaving the gateway unguarded breaks the word and abandons the ally");}}
  // ---- Scenario 3: a request protects a weaker nation. ----
  {var w=Setup(618);w.turn=60;w.DeclareWar(1,2);var home=w.cities.First(c=>c.owner==0);for(int i=0;i<12;i++)w.Add(0,2,home.x,home.y);foreach(var u in w.units.Where(u=>u.owner==0))w.At(u.x,u.y).seen=true;
   w.AddOpinion(1,0,OpinionMods.Dealings,30);w.AddExhaustion(1,2,30);
   var stop=new DiplomaticRequest{from=0,to=1,kind=RequestKinds.StopWar,target=2,duration=15,gold=50,consequence=RequestConsequence.Rivalry};
   Check(w.RequestProblem(stop)=="","the request is valid");int score=w.RequestScore(stop,null);Check(w.RequestLines(stop).Any(l=>l.Key.StartsWith("Fear of your anger")||l.Key.StartsWith("Your threat")),"the threat's credibility is weighed");
   var sent=w.ProposeRequest(stop);Check(score>=0==(sent.status==RequestStatus.Fulfilled),"the answer follows the shown reckoning");
   if(sent.status==RequestStatus.Fulfilled)Check(!w.AtWar(1,2)&&w.Relation(1,2).peaceUntil>=w.turn+15&&w.gold==450,"the attacker makes peace, the guarantee becomes a truce, and the incentive is paid");
   else Check(sent.status==RequestStatus.Refused&&sent.answer.StartsWith("Refused"),"a refusal says why");
   // Repeated identical requests are blocked, so a refusal cannot be farmed.
   Check(w.RequestProblem(stop)!="","the same request cannot be sent again at once");}
  // ---- Scenario 5: a scarce resource can be had by trade, or at the peace table. ----
  {var w=Setup(619);var second=SecondCity(w,1);Check(second!=null,"room for a second rival city");{var t=w.At(second.x,second.y);t.resource=2;t.mine=true;t.road=true;w.nations[0].tech[TechnologyCatalog.Index("iron")]=true;w.nations[1].tech[TechnologyCatalog.Index("iron")]=true;w.tech=w.nations[0].tech;Connect(w,1,second);
    Check(!w.HasResource(0,2)&&w.HasDomesticResource(1,2),"only the rival has Iron");
    w.AddOpinion(1,0,OpinionMods.Dealings,40);var buy=w.Propose(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,duration=10,givePerTurn=6,askResource=2});Check(buy.status==1&&w.HasResource(0,2),"by trade: Iron imported for gold: "+buy.answer+" "+w.HasResource(0,2));
    var w2=Setup(619);var second2=SecondCity(w2,1);var t2=w2.At(second2.x,second2.y);t2.resource=2;t2.mine=true;t2.road=true;foreach(int o in new[]{0,1})w2.nations[o].tech[TechnologyCatalog.Index("iron")]=true;w2.tech=w2.nations[0].tech;w2.DeclareWar(1,0);w2.turn+=8;w2.AddExhaustion(1,0,90);Beaten(w2,1,0,second2);
    var cede=w2.Propose(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Peace,askCity=second2.id});Check(cede.status==1&&second2.owner==0,"or by peace terms: the city with the Iron is handed over");Connect(w2,0,second2);Check(w2.HasDomesticResource(0,2),"and once joined by road its Iron is the player's own");}}
  // ---- Scenario 7: gate access opens the way to a gateway, and nothing else. ----
  {var w=Setup(620);var gate=w.HomeGates().FirstOrDefault();Check(gate!=null,"the test map has a gateway");{
    foreach(var t in w.tiles)if(t.realm==gate.realm&&w.Distance(t.x,t.y,gate.x,gate.y)<=4)t.owner=1;
    var far=w.tiles.First(t=>t.owner==1&&w.Distance(t.x,t.y,gate.x,gate.y)==4);
    Check(!w.CanEnter(0,gate)&&!w.CanEnter(0,far),"without an agreement the gateway is closed to you");
    w.AddOpinion(1,0,OpinionMods.Dealings,60);var o=w.Propose(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.GateAccess});
    Check(o.status==1&&w.Pact(0,1,Pacts.GateAccess),"gate access is signed ("+o.answer+")");
    Check(w.CanEnter(0,gate)&&!w.CanEnter(0,far)&&!w.HasAccess(0,1),"with gate access your units may reach the gateway, but the rest of their land stays closed");
    Check(w.AcceptanceLines(new DiplomaticOffer{from=2,to=1,kind=TreatyKind.GateAccess}).Count>0,"rivals weigh gate access by their interest in it");}}
  // ---- Behaviour profile: competitive rivals turn on whoever is close to winning; world simulation does not. ----
  {var w=Setup(621);w.nations[0].projectStage=2;Check(w.VictoryThreat(0)>=60,"a Transcendence two stages in is a threat to win");Check(w.CompetitiveAmbition(1,0)==0,"world-simulation rivals do not gang up for it");w.aiProfile=AiProfiles.Competitive;Check(w.CompetitiveAmbition(1,0)>=25,"competitive rivals do");}
  // ---- Rivals use the same requests among themselves. ----
  {var w=Setup(622);w.turn=60;Bond(w,1,2,Pacts.Defensive,30);w.DeclareWar(0,2);
   w.AddExhaustion(2,0,60);for(int i=0;i<6;i++)w.DiplomacyAI();w.turn++;for(int i=0;i<5;i++){w.turn++;w.DiplomacyAI();}
   Check(w.requests.Any(r=>r.from==2&&r.to==1)||w.AtWar(1,0),"an attacked rival calls on its ally (or the ally is already at war beside it)");Check(w.Valid(),"valid after rival requests");}
  // ---- Older saves: version 42 diplomacy gains the new slots and stays valid. ----
  {var w=Setup(623);foreach(var p in w.diplomacy){p.pactSince=new int[5];p.modsA=new int[8];p.modsB=new int[8];}w.requests=null;w.version=42;w.UpgradeSave();Check(w.version==World.SaveVersion&&w.requests!=null&&w.diplomacy.All(p=>p.pactSince.Length==Pacts.Count&&p.modsA.Length==OpinionMods.Count&&p.lastDeclarer==-1)&&w.Valid(),"version 42 saves upgrade to 43");}
 }
}
}
