using System;
using System.Linq;
namespace ParadigmRealm {
// Checks of the rebuilt diplomacy (Diplomacy.cs, save version 40): opinion and its reasons, trust, attitudes, standing agreements,
// envoys, rivals, claims, just and unjust wars, war exhaustion, trade deals, the inbox, the rivals' own diplomacy and older saves.
public static class DiplomacyChecks {
 static void Check(bool yes,string why){if(!yes)throw new Exception("Diplomacy: "+why);}
 static World Setup(){var w=World.New(613,natives:false);w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.met[1]=w.met[2]=true;w.Relation(1,2).contact=true;w.gold=500;foreach(var n in w.nations)n.gold=500;
  // Rival leader agendas (CivLore) move opinion; these checks measure the rules alone, so the rivals here have none.
  for(int o=1;o<w.civCount;o++)w.nations[o].civ=-1;return w;}
 static void Next(World w,int turns=1){for(int i=0;i<turns;i++){w.turn++;w.AdvanceDiplomacy();}}
 static DiplomaticOffer Offer(World w,TreatyKind kind,int a=0,int b=1){return w.Propose(new DiplomaticOffer{from=a,to=b,kind=kind,duration=10});}
 static void Resource(World w,int owner,int res){var c=w.cities.First(x=>x.owner==owner);var t=w.At(c.x,c.y);t.resource=res;t.farm=t.mine=t.road=true;w.RebuildTerritory();}
 static void Tech(World w,string id,params int[] owners){foreach(int o in owners)w.nations[o].tech[TechnologyCatalog.Index(id)]=true;}
 static void Like(World w,int observer,int other,int amount){w.AddOpinion(observer,other,OpinionMods.Dealings,amount);}
 public static void Run(){
  // Older saves.
  var w=Setup();Check(!w.AtWar(1,2),"new rivals begin at peace");w.version=6;w.war[1]=true;w.openBorders[2]=true;w.nations[2].relations=17;w.UpgradeSave();
  Check(w.version==41&&w.AtWar(1,2)&&w.AtWar(0,1)&&w.HasAccess(0,2)&&w.Pact(0,2,Pacts.Borders)&&w.OpinionMod(2,0,OpinionMods.Dealings)==17&&w.Valid(),"v6 migration preserves old wars, borders and goodwill");int kept=w.Opinion(2,0);w.UpgradeSave();Check(w.Opinion(2,0)==kept,"migration is idempotent");
  w=Setup();w.turn=20;{var p=w.Relation(0,1);p.friendUntil=w.turn+10;p.researchUntil=w.turn+10;p.trustB=30;p.grievanceB=40;p.embassyA=true;w.Relation(0,2).allianceUntil=w.turn+5;w.Relation(1,2).embargoUntil=w.turn+5;w.warWeariness[0]=9;
   w.offers.Add(new DiplomaticOffer{id=w.nextDiplomaticId++,from=1,to=0,kind=(TreatyKind)6,created=1,expires=4});w.version=39;w.UpgradeSave();
   Check(w.version==41&&w.Pact(0,1,Pacts.NonAggression)&&w.Pact(0,1,Pacts.Research)&&!w.Pact(0,1,Pacts.Defensive),"an old friendship and research pact become standing agreements");
   Check(w.Allied(0,2)&&w.Friends(0,2)&&w.HasAccess(0,2),"an old alliance becomes a defensive pact");
   Check(w.Rival(1,2)&&w.Embargoed(1,2)&&w.CasusBelli(1,2)=="Rivalry","an old embargo becomes a rivalry");
   Check(w.OpinionMod(1,0,OpinionMods.Dealings)==35&&w.OpinionMod(1,0,OpinionMods.Grievance)==-20&&w.Trust(1,0)==30,"old trust, the embassy and grievances become marks on opinion");
   Check(w.offers.Count==0&&p.friendUntil==0&&p.grievanceB==0&&!p.embassyA&&w.warWeariness[0]==0&&w.Valid(),"retired treaties and fields are cleared, and the game is valid");}
  // Contact, opinion and its reasons.
  w=Setup();w.met[1]=false;Check(w.EnvoyReason(0,1)!=""&&w.WarReason(0,1)!=""&&w.RivalReason(0,1)!=""&&Offer(w,TreatyKind.Access).status==2,"first contact gates every action");w.met[1]=true;
  int start=w.Opinion(1,0);Check(w.OpinionLines(1,0).Sum(l=>l.Value)==start,"the reasons add up to the opinion");
  Check(w.GiftGold(1)&&w.Opinion(1,0)==start+10&&w.OpinionLines(1,0).Any(l=>l.Key=="Gifts"&&l.Value==10),"a gift is a named reason worth +10");Next(w,30);Check(w.Opinion(1,0)==start,"a gift fades, one point every third turn");
  // Proposals show their reasons; the lowest agreements come first.
  w=Setup();var borders=new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Access};Check(w.AcceptanceLines(borders).Sum(l=>l.Value)==w.OfferWillingness(borders)&&w.AcceptanceLines(borders).Any(l=>l.Key=="Opinion of you"),"the reasons for an answer add up to it");
  Check(w.Attitude(1,0)==Attitudes.Neutral&&Offer(w,TreatyKind.NonAggression).status==2,"a neutral leader refuses a non-aggression pact");Check(Offer(w,TreatyKind.Access).status==2,"one proposal per pair each turn");Next(w);
  Check(Offer(w,TreatyKind.Access).status==1&&w.HasAccess(0,1)&&w.openBorders[1],"a neutral leader opens borders");Next(w,40);Check(w.HasAccess(0,1)&&w.Trust(1,0)==Pacts.TrustCap[Pacts.Borders],"agreements have no end date, and trust grows to the agreement's ceiling");
  Check(w.OpinionLines(1,0).Any(l=>l.Key.StartsWith("Trust")&&l.Value==10),"trust is half an opinion point each");
  // Envoys.
  w=Setup();Check(w.EnvoyLimit(0)==1&&w.SetEnvoy(0,1,true)&&w.Envoy(0,1)&&w.EnvoyReason(0,2).Contains("Recall"),"one envoy in the first age, at one court");w.SetThreat(1,0,30);Next(w,10);
  Check(w.OpinionMod(1,0,OpinionMods.Envoy)==10&&w.Threat(1,0)==10,"an envoy adds 1 opinion a turn and halves the time fear takes to fade");Next(w,40);Check(w.OpinionMod(1,0,OpinionMods.Envoy)==Pacts.EnvoyCap&&w.Trust(1,0)==Pacts.EnvoyTrust,"an envoy's work stops at +30, and trust reaches 20 without any pact");
  Check(w.SetEnvoy(0,1,false)&&w.EnvoyReason(0,2)=="","a recalled envoy is free again");Next(w,5);Check(w.OpinionMod(1,0,OpinionMods.Envoy)==Pacts.EnvoyCap-5&&w.Trust(1,0)==Pacts.EnvoyTrust-5,"without the envoy its work fades");
  // Non-aggression: the peace it keeps, the ten turns it must stand and the notice it gives.
  w=Setup();Like(w,1,0,40);Check(w.Attitude(1,0)==Attitudes.Cordial&&Offer(w,TreatyKind.NonAggression).status==1&&w.Friends(0,1)&&!w.DeclareWar(0,1)&&w.WarReason(0,1).Contains("pact"),"a cordial leader signs non-aggression, and it keeps the peace");
  Check(!w.EndTreaty(0,1,TreatyKind.NonAggression)&&w.PactEndReason(0,1,Pacts.NonAggression).Contains("10"),"an agreement holds for at least ten turns");Next(w,10);int trusted=w.Trust(1,0);
  Check(w.EndTreaty(0,1,TreatyKind.NonAggression)&&!w.Friends(0,1)&&w.OpinionMod(1,0,OpinionMods.Broken)==-15&&w.Trust(1,0)==Math.Max(0,trusted-20),"ending it is remembered as a broken word");
  Check(!w.DeclareWar(0,1)&&w.WarReason(0,1).Contains("truce"),"ending it gives notice");Next(w,Pacts.Notice);Check(w.DeclareWar(0,1),"after ten turns' notice war is possible");
  // Research, commerce and defence.
  w=Setup();Like(w,1,0,60);Check(Offer(w,TreatyKind.Research).status==2,"research needs Writing");Tech(w,"writing",0,1);Next(w);var city=w.cities.First(c=>c.owner==0);int science=w.Yield(city)[2];
  Check(Offer(w,TreatyKind.Research).status==1&&w.gold==500&&w.Yield(city)[2]==science+2,"a research agreement costs nothing and adds 2 science");Next(w);
  int total=w.gold+w.nations[1].gold;Check(Offer(w,TreatyKind.Commercial).status==1&&w.DiplomaticIncome(0)==w.CommercialGold(0)&&w.CommercialGold(0)>=2&&w.CommercialGold(1)==w.CommercialGold(0),"a commercial pact pays both sides");
  int pay=w.CommercialGold(0);w.ProcessDiplomacyPayments();Check(w.gold+w.nations[1].gold==total+2*pay,"the pact's gold reaches both treasuries");
  w=Setup();Like(w,1,0,80);Check(Offer(w,TreatyKind.Defensive).status==2,"a defensive pact needs Code of Laws");Tech(w,"laws",0,1);Next(w);Check(w.Attitude(1,0)==Attitudes.Friendly&&Offer(w,TreatyKind.Defensive).status==1&&w.Allied(0,1)&&w.HasAccess(0,1)&&w.Attitude(1,0)==Attitudes.Loyal,"a friendly leader signs a defensive pact, with open borders");
  Check(w.DeclareWar(2,1)&&w.AtWar(0,2),"a defensive pact brings the partner into the war, the player included");
  w=Setup();w.Relation(0,1).pacts=1<<Pacts.Defensive;w.Relation(0,2).peaceUntil=w.turn+8;Check(w.DeclareWar(2,1)&&!w.AtWar(0,2)&&w.OpinionMod(1,0,OpinionMods.Broken)==-15,"a defensive pact cannot break a truce, and the failure is remembered");
  // Rivals.
  w=Setup();w.Relation(0,1).pacts=1<<Pacts.Borders;Check(w.RivalReason(0,1).Contains("agreements"),"no rivalry while agreements stand");w.Relation(0,1).pacts=0;
  var ca=w.cities.First(c=>c.owner==0);var cb=w.cities.First(c=>c.owner==1);w.tradeRoutes.Add(new TradeRoute{origin=ca.id,destination=cb.id,originOwner=0,destinationOwner=1});int influence=w.InfluencePerTurn(0);
  Check(w.TradeIncome(0)==3&&w.DeclareRival(0,1)&&w.Rival(0,1)&&!w.Rival(1,0)&&w.TradeIncome(0)==0&&w.InfluencePerTurn(0)==influence+1,"naming a rival stops trade and adds 1 influence a turn");
  Check(Offer(w,TreatyKind.Access).answer.Contains("rivals")&&w.Attitude(0,1)==Attitudes.Rival&&w.OpinionLines(1,0).Any(l=>l.Key=="Named us a rival"&&l.Value==-30),"rivals sign nothing, and the insult is felt");
  Check(w.CasusBelli(0,1)==""&&w.CausePending(0,1)!=""&&w.EndRivalReason(0,1)!="","a rivalry is not a cause for war at once, and cannot be dropped at once");Next(w,Pacts.CauseDelay);Check(w.CasusBelli(0,1)=="Rivalry","after three turns it is a just cause");
  Next(w,Pacts.MinTurns);Check(w.EndRivalry(0,1)&&w.TradeIncome(0)==3,"a rivalry can end after ten turns, and trade resumes");
  // Claims.
  w=Setup();cb=w.cities.First(c=>c.owner==1);w.At(cb.x,cb.y).seen=true;w.nations[0].influence=0;Check(w.ClaimReason(0,cb).Contains("influence"),"a claim costs influence");w.nations[0].influence=100;int cost=w.ClaimCost(cb);int before=w.Opinion(1,0);
  Check(w.MakeClaim(0,cb)&&w.nations[0].influence==100-cost&&w.HasClaim(0,cb.id)&&w.ClaimReason(0,cb)=="Already claimed."&&w.Opinion(1,0)==before-10,"a claim is paid for, recorded and resented");
  Check(w.CasusBelli(0,1)=="",w.CasusBelli(0,1)+": a claim is not a cause at once");Next(w,Pacts.CauseDelay);Check(w.CasusBelli(0,1)=="Claim on "+cb.name&&w.FormalWarAvailable(0,1),"after three turns a claim is a just cause");
  // Just and unjust wars.
  w.SetThreat(2,0,0);w.SetThreat(1,0,0);Check(w.DeclareWar(0,1)&&w.OpinionMod(1,0,OpinionMods.War)==-20&&w.Threat(2,0)==0&&w.Threat(1,0)>0,"a just war angers only its victim");
  w.DiplomacyOnCapture(0,1,cb);int claimedFear=w.Threat(2,0);Check(!w.HasClaim(0,cb.id)&&w.OpinionMod(1,0,OpinionMods.Conquest)==-10,"taking a claimed city settles the claim");
  var w2=Setup();cb=w2.cities.First(c=>c.owner==1);Check(w2.DeclareWar(0,1)&&w2.OpinionMod(1,0,OpinionMods.War)==-40&&w2.Threat(2,0)>0&&w2.OpinionMod(2,0,OpinionMods.Grievance)<0,"a war without cause frightens the world");
  int fear=w2.Threat(2,0);w2.DiplomacyOnCapture(0,1,cb);Check(w2.Threat(2,0)-fear>claimedFear&&w2.OpinionMod(1,0,OpinionMods.Conquest)==-20,"taking a city without a claim alarms the world far more ("+(w2.Threat(2,0)-fear)+" against "+claimedFear+")");
  w2.SetThreat(2,0,60);Check(w2.Attitude(2,0)==Attitudes.Threatened&&w2.CasusBelli(2,0)=="Containing a threat","a feared power faces threatened leaders with a cause against it");
  var appease=new DiplomaticOffer{from=0,to=2,kind=TreatyKind.NonAggression};Check(w2.AcceptanceLines(appease).Any(l=>l.Key=="Attitude: Threatened"&&l.Value==25),"a threatened leader is readier to sign non-aggression");
  // War exhaustion and peace.
  w=Setup();Check(w.DeclareWar(0,1),"war begins");city=w.cities.First(c=>c.owner==0);int happy=w.ContentSlots(city);var peace=new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Peace};
  Check(w.OfferWillingness(peace)<0&&w.AcceptanceLines(peace).Any(l=>l.Key=="The war has only begun"),"nobody makes peace at once");Next(w,25);
  Check(w.Exhaustion(0,1)==25&&w.Exhaustion(1,0)==25&&w.WarUnhappiness(0)==1&&w.ContentSlots(city)==happy-1,"exhaustion rises 1 a turn and costs content citizens");
  w.WarLosses(0,1,2,1);Check(w.Exhaustion(0,1)==29&&w.Exhaustion(1,0)==27,"each unit lost adds 2");Check(w.AcceptanceLines(peace).Sum(l=>l.Value)==w.OfferWillingness(peace),"the reasons for peace add up");
  w.AddExhaustion(1,0,100);Check(w.Exhaustion(1,0)==100&&w.OfferWillingness(peace)>=0&&Offer(w,TreatyKind.Peace).status==1&&!w.AtWar(0,1)&&w.Exhaustion(0,1)==0&&w.ContentSlots(city)==happy,"an exhausted enemy makes peace, and the exhaustion ends with the war");
  Check(!w.DeclareWar(0,1)&&w.WarReason(0,1).Contains("truce"),"a truce follows");Next(w,Pacts.Truce);Check(w.DeclareWar(0,1),"the truce ends after ten turns");
  // Attitudes.
  w=Setup();Like(w,1,0,-70);Check(w.Attitude(1,0)==Attitudes.Hostile&&w.OfferWillingness(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Access})<0,"a hostile leader signs nothing");
  // The inbox.
  w=Setup();var inbound=Offer(w,TreatyKind.Access,1,0);Check(inbound.status==0&&!w.HasAccess(0,1),"a rival's proposal never accepts for the player");Check(w.RespondToOffer(inbound.id,true)&&w.HasAccess(0,1)&&!w.RespondToOffer(inbound.id,true),"the inbox accepts exactly once");Next(w);var stale=w.Propose(new DiplomaticOffer{from=2,to=0,kind=TreatyKind.Trade,askGold=450});w.gold=20;Check(!w.RespondToOffer(stale.id,true)&&stale.status==2,"a stale treasury is checked again");
  w=Setup();inbound=Offer(w,TreatyKind.Access,1,0);Next(w,3);Check(inbound.status==3&&!w.RespondToOffer(inbound.id,true),"the inbox expires");
  // Trade deals.
  w=Setup();var refused=w.Propose(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,askGold=40});Check(refused.status==2&&w.offers.Any(o=>o.to==0&&o.status==0),"a rival answers a poor trade with a priced counteroffer");var counter=w.offers.Last();total=w.gold+w.nations[1].gold;Check(w.RespondToOffer(counter.id,true)&&w.gold+w.nations[1].gold==total,"counteroffer money is conserved");
  w=Setup();Resource(w,1,2);var trade=w.Propose(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,duration=5,giveGold=50,givePerTurn=2,askResource=2});Check(trade.status==1&&w.HasResource(0,2)&&!w.HasDomesticResource(0,2),"resource imports unlock supply");Check(w.OfferProblem(new DiplomaticOffer{from=0,to=2,kind=TreatyKind.Trade,giveResource=2})!="","imports cannot be re-exported");before=w.gold;total=w.gold+w.nations[1].gold;for(int i=0;i<5;i++){w.ProcessDiplomacyPayments();Next(w);}Check(w.gold==before-10&&w.gold+w.nations[1].gold==total&&w.contracts.Count==0&&!w.HasResource(0,2),"a five-turn deal pays exactly five times and ends");
  w=Setup();Resource(w,1,2);trade=w.Propose(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,giveGold=50,givePerTurn=2,askResource=2});w.gold=0;total=w.nations[1].gold;w.ProcessDiplomacyPayments();Check(w.gold==0&&w.nations[1].gold==total&&w.contracts.Count==0&&w.OpinionMod(1,0,OpinionMods.Broken)<0,"a default cancels the deal without creating money");
  w=Setup();Resource(w,1,2);trade=w.Propose(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Trade,giveGold=50,askResource=2});w.At(w.cities.First(c=>c.owner==1).x,w.cities.First(c=>c.owner==1).y).mine=false;Check(!w.HasResource(0,2),"a lost improvement removes the imported supply at once");w.ProcessDiplomacyPayments();Check(w.contracts.Count==0,"a disconnected deal is cancelled");
  // Settling against a partner's border.
  w=Setup();w.Relation(0,1).pacts=1<<Pacts.NonAggression;ca=w.cities.First(c=>c.owner==0);for(int y=ca.y-1;y<=ca.y+1;y++)for(int x=ca.x+3;x<=ca.x+5;x++)w.At(x,y).terrain=0;w.Add(0,3,ca.x+3,ca.y);var settler=w.Add(1,0,ca.x+4,ca.y);w.Add(1,3,ca.x+1,ca.y);w.Reveal();
  Check(w.SettlementDiplomacyWarning(1,settler.x,settler.y)!=""&&w.SettlementOptions(settler).All(r=>r.diplomacyWarning==""),"rivals avoid sites that would strain a pact");Check(w.Found(settler)!=null&&w.OpinionMod(0,1,OpinionMods.Borders)==-8,"a settlement against a partner's border is resented");
  // The rivals' own diplomacy.
  w=Setup();w.turn=6;w.DiplomacyAI();Check(w.offers.Any(o=>o.from==1&&o.to==0&&o.status==0)&&w.HasAccess(1,2),"rivals propose to the player and agree with each other");Check(w.Valid(),"valid active diplomacy state");
  w=Setup();Like(w,1,2,-90);w.turn=6;w.DiplomacyAI();Check(w.Rival(1,2),"a leader that hates another names it a rival first");var home=w.cities.First(c=>c.owner==1);for(int i=0;i<6;i++)w.Add(1,2,home.x,home.y);
  w.turn=11;w.DiplomacyAI();Check(w.AtWar(1,2)&&w.Relation(1,2).events.Any(e=>e.Contains("Rivalry")),"and goes to war only once it has a cause and the strength");
  w=Setup();w.SetEnvoy(1,0,false);w.SetThreat(0,1,40);w.turn=6;w.DiplomacyAI();Check(w.Envoy(1,0),"a rival sends its envoy to calm a power that fears it");
  Check(w.Valid(),"valid after the rivals' turn");w.Relation(0,1).modsA[0]=500;Check(!w.Valid(),"malformed marks rejected");w.Relation(0,1).modsA[0]=0;w.offers.Add(new DiplomaticOffer{id=w.nextDiplomaticId++,from=1,to=0,kind=TreatyKind.Access,created=1,expires=4,givePerTurn=-1});Check(!w.Valid(),"malformed offers rejected");
 }
}
}
