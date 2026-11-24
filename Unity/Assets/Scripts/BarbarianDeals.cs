using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Bargains with barbarians (owner request, 24 Nov 2026: pay them off, or pay them to attack another civilisation like a privateer).
// A bargain is struck with one camp and binds that camp's raiders, wherever they are:
//  - Paid off: for TruceTurns they leave the payer's units, cities and land alone. Other civilisations are still fair game.
//  - Hired: for HireTurns they also march on one civilisation, beyond their usual range, with more raiders and more often. Like a
//    privateer they carry no flag: the victim sees barbarians and never learns who paid, so it costs no peace and no reputation.
//    The employer takes half of any gold they plunder, and is left alone as if it had paid them off.
// A camp serves one employer at a time. The victim of a hired camp, or anyone else, may still buy it off, at double the price while
// it is under contract; if the victim does, the contract ends. Each payment makes that camp dearer (they learn who pays). Attacking a
// camp or its raiders ends your own bargain with it. Rival home civilisations bargain too. Every number is a first guess.
[Serializable] public class BarbarianDeal{public int payer,target=-1,until;}
public static class BarbarianTerms{
 public const int TruceTurns=15,HireTurns=15,TruceBase=30,TrucePerAge=20,HireBase=80,HirePerAge=40,HirePerTile=2,DearerPercent=25,HireRange=30,ExtraRaiders=2,HiredRaidEvery=4,EmployerShare=2,MaxMarch=6000;
}
public partial class World {
 BarbarianCamp CampOf(Raider r){if(camps==null||r==null)return null;foreach(var c in camps)if(c.id==r.camp&&c.hp>0)return c;return null;}
 static List<BarbarianDeal> DealsOf(BarbarianCamp c){if(c.deals==null)c.deals=new List<BarbarianDeal>();return c.deals;}
 public BarbarianDeal DealWith(int owner,BarbarianCamp c){if(c==null)return null;foreach(var d in DealsOf(c))if(d.payer==owner&&d.until>turn)return d;return null;}
 // The contract a hired camp is serving, or null.
 public BarbarianDeal Contract(BarbarianCamp c){if(c==null)return null;foreach(var d in DealsOf(c))if(d.target>=0&&d.until>turn)return d;return null;}
 public bool CampSpares(BarbarianCamp c,int owner){return owner>=0&&DealWith(owner,c)!=null;}
 public bool RaiderSpares(Raider r,int owner){return owner>=0&&CampSpares(CampOf(r),owner);}
 // You can bargain with a camp you have found, or with raiders standing in sight of you.
 public bool CanParley(int owner,BarbarianCamp c){
  if(!barbarians||c==null||c.hp<=0||owner<0||owner>=civCount||IsNativeNation(owner))return false;
  if(owner==0?c.seen:nations[owner].known[c.y*Width+c.x])return true;
  return raiders!=null&&raiders.Any(r=>r.camp==c.id&&r.hp>0&&VisibleTo(owner,r.x,r.y));}
 int Dearer(BarbarianCamp c,int price){return price*(100+BarbarianTerms.DearerPercent*Math.Min(12,c.paid))/100;}
 public int PayOffPrice(int owner,BarbarianCamp c){int p=Dearer(c,BarbarianTerms.TruceBase+BarbarianTerms.TrucePerAge*BarbarianEra());var k=Contract(c);return k!=null&&k.payer!=owner?p*2:p;}
 public int HirePrice(BarbarianCamp c,int target){var t=NearestCityOf(target,c);return Dearer(c,BarbarianTerms.HireBase+BarbarianTerms.HirePerAge*BarbarianEra()+(t==null?0:BarbarianTerms.HirePerTile*Distance(c.x,c.y,t.x,t.y)));}
 // The target's nearest home-world city the raiders could walk to.
 City NearestCityOf(int target,BarbarianCamp c){City best=null;int least=int.MaxValue,region=WalkRegion(c.y*Width+c.x);foreach(var t in cities){if(t.owner!=target||At(t.x,t.y).realm!=0)continue;int d=Distance(c.x,c.y,t.x,t.y);if(d>=least||!BesideRegion(t,region))continue;least=d;best=t;}return best;}
 bool BesideRegion(City t,int region){if(WalkRegion(t.y*Width+t.x)==region)return true;for(int k=0;k<6;k++){var n=Step(t.x,t.y,HexOr(k,true),HexOr(k,false));if(n!=null&&BarbarianGround(n)&&WalkRegion(n.y*Width+n.x)==region)return true;}return false;}
 public string PayOffReason(int owner,BarbarianCamp c){
  if(result!="")return "The game has ended.";if(!CanParley(owner,c))return "You have not found this camp.";
  var mine=DealWith(owner,c);if(mine!=null)return mine.target>=0?"They already serve you.":"Already paid off until turn "+mine.until+".";
  int price=PayOffPrice(owner,c);return Treasury(owner)<price?"You need "+price+" gold.":"";}
 public bool PayOff(int owner,BarbarianCamp c){
  if(PayOffReason(owner,c)!="")return false;AddDiplomaticGold(owner,-PayOffPrice(owner,c));c.paid++;
  DealsOf(c).RemoveAll(d=>d.target==owner);// the victim has outbid whoever sent them
  DealsOf(c).RemoveAll(d=>d.payer==owner);c.deals.Add(new BarbarianDeal{payer=owner,until=turn+BarbarianTerms.TruceTurns});
  if(owner==0)Note("The barbarians take your gold. Their raiders will leave you alone until turn "+(turn+BarbarianTerms.TruceTurns)+".");return true;}
 public string HireReason(int owner,BarbarianCamp c,int target){
  if(result!="")return "The game has ended.";if(!CanParley(owner,c))return "You have not found this camp.";
  if(target<0||target>=civCount||target==owner||IsNativeNation(target)||!Survives(target))return "Choose a civilisation of the home world.";
  if(!HasContact(owner,target))return "You have not met them.";
  var k=Contract(c);if(k!=null)return k.payer==owner?"They already serve you until turn "+k.until+".":"They are already in someone's pay.";
  var t=NearestCityOf(target,c);if(t==null)return "They cannot reach that people by land.";
  if(Distance(c.x,c.y,t.x,t.y)>BarbarianTerms.HireRange)return "That people lives too far from this camp.";
  int price=HirePrice(c,target);return Treasury(owner)<price?"You need "+price+" gold.":"";}
 public bool Hire(int owner,BarbarianCamp c,int target){
  if(HireReason(owner,c,target)!="")return false;AddDiplomaticGold(owner,-HirePrice(c,target));c.paid++;
  DealsOf(c).RemoveAll(d=>d.payer==owner||d.payer==target);c.deals.Add(new BarbarianDeal{payer=owner,target=target,until=turn+BarbarianTerms.HireTurns});c.nextRaid=Math.Min(c.nextRaid,turn+1);
  if(owner==0)Note("The barbarians take your gold and will raid "+NationName(target)+" until turn "+(turn+BarbarianTerms.HireTurns)+". Nobody will know who sent them.");return true;}
 // The civilisations this camp could be set on, for the panel: everyone of the home world you have met.
 public List<int> HireTargets(int owner){var list=new List<int>();for(int o=0;o<civCount;o++)if(o!=owner&&!IsNativeNation(o)&&Survives(o)&&HasContact(owner,o))list.Add(o);return list;}
 // Attacking them breaks your own bargain.
 void BreakBargain(int owner,BarbarianCamp c){if(c==null||!CampSpares(c,owner))return;DealsOf(c).RemoveAll(d=>d.payer==owner);if(owner==0)Note("You attacked barbarians you had paid: your bargain with that camp is over.");}
 void BargainsTurn(){
  foreach(var c in camps){var done=DealsOf(c).Where(d=>d.until<=turn||d.target>=0&&!Survives(d.target)||!Survives(d.payer)).ToList();
   foreach(var d in done){c.deals.Remove(d);if(d.payer==0)Note(d.target>=0?"The barbarians you hired have done their time and no longer serve you.":"The barbarians you paid off have spent your gold. Their raiders will strike you again.");}}
  for(int o=1;o<civCount;o++)if(!IsNativeNation(o))BargainAI(o);}
 // Rivals pay off raiders who are troubling their land, and now and then send a camp against someone they are at war with or have
 // named a rival.
 void BargainAI(int o){
  if((turn+o)%4!=0||Treasury(o)<60||!cities.Any(c=>c.owner==o))return;
  foreach(var r in raiders){if(r.hp<=0)continue;var t=At(r.x,r.y);if(t==null||t.owner!=o)continue;var c=CampOf(r);if(c==null||CampSpares(c,o)||!CanParley(o,c))continue;if(Treasury(o)>=PayOffPrice(o,c)*2&&PayOff(o,c))return;}
  if((turn+o)%12!=0||camps.Any(c=>Contract(c)!=null&&Contract(c).payer==o))return;
  foreach(var c in camps){if(!CanParley(o,c)||Contract(c)!=null)continue;foreach(int target in HireTargets(o)){if(!AtWar(o,target)&&!Rival(o,target)||HireReason(o,c,target)!="")continue;if(Treasury(o)>=HirePrice(c,target)*3&&Hire(o,c,target))return;}}}
 // One step of a hired raider's march on its target: the way round water and mountains to the nearest of its cities.
 Tile MarchStep(Raider r,int target){
  City goal=null;int least=int.MaxValue;foreach(var c in cities){if(c.owner!=target||At(c.x,c.y).realm!=0)continue;int d=Distance(r.x,r.y,c.x,c.y);if(d<least){least=d;goal=c;}}
  if(goal==null||least<=1)return null;int start=r.y*Width+r.x;var from=new Dictionary<int,int>{{start,-1}};var q=new Queue<int>();q.Enqueue(start);int end=-1;
  while(q.Count>0&&end<0&&from.Count<BarbarianTerms.MaxMarch){int cell=q.Dequeue();var t=tiles[cell];if(Distance(t.x,t.y,goal.x,goal.y)==1){end=cell;break;}
   for(int k=0;k<6;k++){var n=Step(t.x,t.y,HexOr(k,true),HexOr(k,false));if(n==null||!BarbarianGround(n))continue;int at=n.y*Width+n.x;if(from.ContainsKey(at))continue;if(cell==start&&!OpenToRaider(n))continue;from[at]=cell;q.Enqueue(at);}}
  if(end<0||end==start)return null;while(from[end]!=start)end=from[end];return tiles[end];}
 public string BargainText(int owner,BarbarianCamp c){var d=DealWith(owner,c);return d==null?"":d.target>=0?"hired by you against "+NationName(d.target)+" until turn "+d.until:"paid off by you until turn "+d.until;}
}
}
