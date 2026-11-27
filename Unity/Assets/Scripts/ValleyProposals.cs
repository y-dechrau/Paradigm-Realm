using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Your own land and timber proposals (owner request, 27 Nov 2026: "the player must be able to propose their own Land or timber
// deal, specifying an area of land it wants, just like the computer civs can"). Four kinds, each judged by the leader from its own
// stake, as its own proposals are:
//   0 ask them to leave a place unsettled, for gold      their stake: a settler bound there, the best town site in it over their
//                                                         next best, the timber it would give them; nothing at stake costs a token
//   1 promise you will leave a place unsettled, for gold  they pay only what the place is worth to them
//   2 sell them timber now                                they buy only what they lack, at most their price
//   3 buy timber from them, delivered each turn           they sell only what they can spare
// The place is the exact set of tiles you paint on the map by clicking and dragging (owner request the same evening: "the land tile
// selection should be dynamic, dictated by the player's cursor click and drag"). A call with a single cell and no area means that
// cell and every tile within ValleyTuning.KeepRadius, as before.
public static class ProposalKinds{public static readonly string[] Names={"They leave a place unsettled","We leave a place unsettled","We sell timber now","We buy timber each turn"};public const int MaxArea=37;}
public partial class World {
 public List<int> ProposalArea(int cell,List<int> area){if(area!=null&&area.Count>0)return area;return KeepTiles(cell,ValleyTuning.KeepRadius).Select(t=>t.y*Width+t.x).ToList();}
 // The cell nearest the middle of an area, used to name it ("the 9 tiles around the ford north of Northwind").
 public int AreaCentre(List<int> area){if(area==null||area.Count==0)return -1;int best=area[0],least=int.MaxValue;foreach(var c in area){int sum=0;foreach(var d in area)sum+=Distance(tiles[c].x,tiles[c].y,tiles[d].x,tiles[d].y);if(sum<least){least=sum;best=c;}}return best;}
 public string AreaName(int o,List<int> area){int c=AreaCentre(area);if(c<0)return "nowhere";return area.Count==1?PlaceName(o,c):"the "+area.Count+" tiles around "+PlaceName(o,c);}
 // What a place is worth to a leader, in gold, from its own knowledge; with the reasons in its voice.
 public int PlaceStake(int o,int cell,out string why){return PlaceStake(o,cell<0?null:ProposalArea(cell,null),out why);}
 public int PlaceStake(int o,List<int> area,out string why){
  why="";if(area==null||area.Count==0){why="Name a place first.";return 0;}var known=nations[o].known;
  var seen=area.Where(c=>c>=0&&c<tiles.Length&&(known==null||known[c])).ToList();if(seen.Count==0){why="We have not seen that land; it is nothing to us yet.";return 0;}
  int stake=0;var parts=new List<string>();
  if(units.Any(u=>u.owner==o&&u.type==0&&u.hasOrder&&seen.Contains(u.destinationY*Width+u.destinationX))){stake+=25;parts.Add("our settlers are bound there");}
  // The best town site inside it, against the best they know elsewhere.
  SettlementReport best=null;int bestCell=-1;foreach(var c in seen){var t=tiles[c];if(IsWater(t.terrain))continue;var r=AssessSettlement(o,t.x,t.y,t.x,t.y);if(r.Suitable&&(best==null||r.score>best.score)){best=r;bestCell=c;}}
  if(best!=null){var alt=BestOtherSite(o,bestCell);int gain=alt==null?best.score:best.score-alt.score;if(gain>0){stake+=gain;parts.Add("it holds a better town site than any other we know");}}
  // Its woods, if they are short of timber.
  var n=AssessValley(o);var woods=seen.Where(c=>Wooded(tiles[c])).ToList();
  if((n.shortfall>0||n.longShort>0)&&woods.Count>=2){int g=woods.Max(c=>CampBenefit(o,tiles[c],0,0));if(g>0){stake+=g*ValleyTuning.GoldPerTurnSaved;parts.Add("its woods would end our timber shortage "+g+" turns sooner");}}
  why=parts.Count==0?"We had no plans for that land.":Cap(string.Join(", and ",parts.ToArray()))+".";return stake;
 }
 public string JudgeProposal(int o,int kind,int cell,int amount,int turns,int gold,out bool accept){return JudgeProposal(o,kind,cell<0?null:ProposalArea(cell,null),amount,turns,gold,out accept);}
 public string JudgeProposal(int o,int kind,List<int> area,int amount,int turns,int gold,out bool accept){
  accept=false;if(!HasContact(0,o))return "You have not met them.";if(AtWar(0,o))return NationName(o)+": \"We do not bargain with an enemy.\"";
  int greed=Flavour(o,DiplomacyFlavours.Greed);int warmth=Opinion(o,0);var rec=Record(o,0);int trust=Trust(o,0)+rec.delivered*2-rec.missed*5-rec.broken*15;
  if(kind==0||kind==1){
   if(area==null||area.Count==0)return "Mark the land on the map first.";if(area.All(c=>IsWater(tiles[c].terrain)))return "Choose land, not water.";
   int third=area.Select(c=>tiles[c].owner).FirstOrDefault(w=>w>=0&&w!=0&&w!=o);if(area.Any(c=>tiles[c].owner>=0&&tiles[c].owner!=0&&tiles[c].owner!=o))return "Part of that land belongs to "+NationName(third)+".";}
  string place=kind<=1?AreaName(o,area):"";
  if(kind==0){if(area.Any(c=>tiles[c].owner==o))return NationName(o)+": \"Part of that is our land. We will not promise to stay off it.\"";
   string why;int stake=PlaceStake(o,area,out why);int price=stake*(10+greed)/12-Math.Max(0,warmth)/5;
   if(stake<=0){accept=gold>=0;return NationName(o)+": \""+why+" We will leave "+place+" alone"+(gold>0?" and take your gold.":".")+"\"";}
   if(gold<price){return NationName(o)+": \""+why+" For "+gold+" gold, no. It is worth "+price+" to us.\"";}
   accept=true;return NationName(o)+": \""+why+" Still, "+gold+" gold is fair. We will leave "+place+" unsettled.\"";}
  if(kind==1){string why;int stake=PlaceStake(o,area,out why);if(stake<=0)return NationName(o)+": \""+why+" Your promise is worth nothing to us.\"";
   int worth=stake*(20-greed)/15;if(trust<0)worth/=2;if(gold>worth)return NationName(o)+": \""+why+" We would pay "+worth+" gold for your promise, not "+gold+".\""+(trust<0?" (They doubt your word.)":"");
   accept=true;return NationName(o)+": \""+why+" "+gold+" gold for your word that you will not settle "+place+". Agreed.\"";}
  if(kind==2){var n=AssessValley(o);int need=Math.Max(n.shortfall,n.longShort);if(need<=0)return NationName(o)+": \"We have timber enough: "+n.reason+".\"";
   if(amount>nations[0].timber)return "You have only "+nations[0].timber+" timber.";int useful=Math.Min(amount,need);int max=useful*ValleyTuning.PricePerTimber*(15-greed/2)/10;
   if(gold>max)return NationName(o)+": \"We can use "+useful+" of it, and would pay "+max+" gold, not "+gold+".\"";accept=true;return NationName(o)+": \"Yes: we lack "+need+" timber. "+amount+" for "+gold+" gold.\"";}
  if(kind==3){int spare=Spare(o,0),total=amount*turns;if(spare<total)return NationName(o)+": \"We can spare "+spare+" timber in all, not "+total+".\"";
   int price=total*ValleyTuning.PricePerTimber*(10+greed)/12;if(gold*turns<price)return NationName(o)+": \"For "+amount+" a turn over "+turns+" turns we want "+((price+turns-1)/turns)+" gold a delivery.\"";
   accept=true;return NationName(o)+": \"Agreed: "+amount+" timber a turn for "+turns+" turns, "+gold+" gold each delivery.\"";}
  return "";
 }
 public string MakeProposal(int o,int kind,int cell,int amount,int turns,int gold){return MakeProposal(o,kind,cell<0?null:ProposalArea(cell,null),amount,turns,gold);}
 public string MakeProposal(int o,int kind,List<int> area,int amount,int turns,int gold){
  bool ok;string said=JudgeProposal(o,kind,area,amount,turns,gold,out ok);if(!ok){DiplomacyEvent(0,o,"Proposal refused: "+ProposalKinds.Names[kind].ToLowerInvariant()+".");return said;}
  string place=kind<=1?AreaName(o,area):"";int centre=kind<=1?AreaCentre(area):-1;var marked=kind<=1?new List<int>(area):new List<int>();
  if(kind==0){if(Treasury(0)<gold)return "You have only "+gold+" gold.";AddDiplomaticGold(0,-gold);AddDiplomaticGold(o,gold);
   Accords().Add(new ValleyAccord{id=nextDiplomaticId++,a=o,b=0,site=centre,area=marked,keeper=o,goldOnce=gold,start=turn,keepTurns=ValleyTuning.LongHorizon*2,place=place,terms=NationName(o)+" leaves "+place+" unsettled; you paid "+gold+" gold."});
   foreach(var u in units.Where(u=>u.owner==o&&u.type==0&&u.hasOrder&&marked.Contains(u.destinationY*Width+u.destinationX)).ToList()){CancelOrder(u);var opts=SettlementOptions(u,1).Where(s=>!marked.Contains(s.y*Width+s.x)).ToList();if(opts.Count>0)Order(u,opts[0].x,opts[0].y);}}
  else if(kind==1){if(Treasury(o)<gold)return NationName(o)+" cannot pay "+gold+" gold.";AddDiplomaticGold(o,-gold);AddDiplomaticGold(0,gold);
   Accords().Add(new ValleyAccord{id=nextDiplomaticId++,a=o,b=0,site=centre,area=marked,keeper=0,goldOnce=gold,start=turn,keepTurns=ValleyTuning.LongHorizon*2,place=place,terms="You leave "+place+" unsettled; "+NationName(o)+" paid "+gold+" gold."});}
  else if(kind==2){if(Treasury(o)<gold)return NationName(o)+" cannot pay "+gold+" gold.";nations[0].timber-=amount;nations[o].timber+=amount;AddDiplomaticGold(o,-gold);AddDiplomaticGold(0,gold);Record(o,0).delivered++;}
  else if(kind==3){Accords().Add(new ValleyAccord{id=nextDiplomaticId++,a=o,b=0,supplier=o,receiver=0,perTurn=amount,turns=turns,goldEach=gold,start=turn,place="",terms=NationName(o)+" delivers "+amount+" timber a turn for "+turns+" turns; you pay "+gold+" gold each delivery."});}
  AddOpinion(o,0,OpinionMods.Dealings,2);DiplomacyEvent(0,o,"Agreed: "+ProposalKinds.Names[kind].ToLowerInvariant()+(kind<=1?" ("+place+")":"")+".");return said;
 }
}
}
