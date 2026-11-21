using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Building costs (owner request, 21 Nov 2026): constructing a building costs gold, timber and stone as well as shields, paid in
// full the moment the building starts (when it is chosen with the line idle, or when it reaches the front of the queue).
// Without enough the building waits at the front of its line and makes no progress; cancelling or replacing an unfinished
// building refunds what was paid. Timber and stone are national stockpiles filled each turn by every city (a little from local
// woodcutters and quarries), by worked woodland and hills, and most by worked Timber and Stone resources. Faith buildings,
// Transcendence stages and gateway projects cost none of this. Games from before this rule pay on loading as new games do
// (each nation is given a starting stockpile, and buildings already under way count as paid).
public static class MaterialRules {
 // Timber and stone per building: about a sixth of its shield cost, split by what it is built of (timber thirds, the rest stone).
 public const int CostDivisor=6,WonderMultiple=2,Minimum=3;
 public const int CityTimber=1,CityStone=1,WoodedTile=1,HillTile=1,ResourceTile=2,DevelopedBonus=1;
 public const int TimberResource=53,StoneResource=69;
 public const int StartStock=10,StartStockPerCity=5;
 static readonly Dictionary<int,int> TimberThirds=new Dictionary<int,int>{{7,2},{8,1},{9,0},{30,2},{31,2},{32,1},{33,1},{34,1},{35,1},{37,0},{38,1},{39,0},{40,1},{41,0},{42,0},{43,3},{44,2},{60,1},{61,1},{62,1},{63,1},{64,1}};
 public static int Timber(int b,int total){int t;if(!TimberThirds.TryGetValue(b,out t))t=1;return total*t/3;}
}
public partial class World {
 public bool buildingMaterials;
 public int BuildingMaterialTotal(int b){if(!buildingMaterials||BuildingGold(b)<=0)return 0;return Math.Max(MaterialRules.Minimum,Cost(b)/MaterialRules.CostDivisor)*(IsWonder(b)?MaterialRules.WonderMultiple:1);}
 public int BuildingTimber(int b){return MaterialRules.Timber(b,BuildingMaterialTotal(b));}
 public int BuildingStone(int b){return BuildingMaterialTotal(b)-BuildingTimber(b);}
 public string BuildingPriceText(int b){var parts=new List<string>();int g=BuildingGold(b),t=BuildingTimber(b),s=BuildingStone(b);if(g>0)parts.Add(g+" gold");if(t>0)parts.Add(t+" timber");if(s>0)parts.Add(s+" stone");return string.Join(", ",parts.ToArray());}
 // What is missing to start building b in city c now ("" when it can be paid, or is already paid for).
 public string PayReason(City c,int b){
  if(!buildingMaterials||BuildUnit(b)>=0||c.paidFor==b)return "";var n=nations[c.owner];var miss=new List<string>();
  int g=BuildingGold(b),t=BuildingTimber(b),s=BuildingStone(b);if(n.gold<g)miss.Add((g-n.gold)+" more gold");if(n.timber<t)miss.Add((t-n.timber)+" more timber");if(n.stone<s)miss.Add((s-n.stone)+" more stone");
  return miss.Count==0?"":"Needs "+string.Join(", ",miss.ToArray())+" to start";
 }
 // Pays for the building at the front of the line if it is not paid for yet. False when the nation cannot afford it.
 public bool TryPayBuilding(City c){
  if(!buildingMaterials)return true;int b=c.buildingProject;if(b<0||c.paidFor==b)return true;RefundBuilding(c);if(PayReason(c,b)!="")return false;
  var n=nations[c.owner];int g=BuildingGold(b),t=BuildingTimber(b),s=BuildingStone(b);n.gold-=g;n.timber-=t;n.stone-=s;if(c.owner==0)gold=n.gold;
  c.paidFor=b;c.paidGold=g;c.paidTimber=t;c.paidStone=s;return true;
 }
 // An unfinished building that was paid for and is no longer the one in progress gives its payment back.
 void RefundBuilding(City c){
  if(c.paidFor<0||c.paidFor==c.buildingProject)return;var n=nations[c.owner];n.gold+=c.paidGold;n.timber+=c.paidTimber;n.stone+=c.paidStone;if(c.owner==0)gold=n.gold;
  c.paidFor=-1;c.paidGold=c.paidTimber=c.paidStone=0;
 }
 // After the player changes a production line: refund what was replaced and pay for what now starts.
 public void SettleBuilding(City c){if(!buildingMaterials||c==null)return;RefundBuilding(c);TryPayBuilding(c);}
 void ConsumeBuildingPayment(City c){c.paidFor=-1;c.paidGold=c.paidTimber=c.paidStone=0;}
 // Timber and stone a nation gathers in a turn: {timber, stone}.
 public int[] MaterialIncome(int owner){
  int timber=0,stone=0;
  foreach(var c in cities){if(c.owner!=owner)continue;timber+=MaterialRules.CityTimber;stone+=MaterialRules.CityStone;
   foreach(var t in Worked(c)){
    if(Wooded(t))timber+=MaterialRules.WoodedTile;if(t.terrain==2)stone+=MaterialRules.HillTile;
    if(t.resource==MaterialRules.TimberResource&&ResourceKnown(owner,t.resource))timber+=MaterialRules.ResourceTile+(t.road?MaterialRules.DevelopedBonus:0);
    if(t.resource==MaterialRules.StoneResource&&ResourceKnown(owner,t.resource))stone+=MaterialRules.ResourceTile+(t.mine?MaterialRules.DevelopedBonus:0);}}
  return new[]{timber,stone};
 }
 void MaterialsTurn(int owner,int turns){if(!buildingMaterials)return;var inc=MaterialIncome(owner);var n=nations[owner];n.timber+=inc[0]*turns;n.stone+=inc[1]*turns;}
 // Turns materials on for a game: a starting stockpile for every nation, and buildings already under way count as paid.
 public void EnableMaterials(){
  if(buildingMaterials)return;buildingMaterials=true;
  for(int o=0;o<civCount;o++){int own=cities.Count(c=>c.owner==o);nations[o].timber+=MaterialRules.StartStock+MaterialRules.StartStockPerCity*own;nations[o].stone+=MaterialRules.StartStock+MaterialRules.StartStockPerCity*own;}
  foreach(var c in cities)if(c.buildingProject>=0){c.paidFor=c.buildingProject;c.paidGold=c.paidTimber=c.paidStone=0;}
 }
 bool MaterialsValid(){foreach(var n in nations)if(n!=null&&(n.timber<0||n.stone<0))return false;foreach(var c in cities)if(c.paidGold<0||c.paidTimber<0||c.paidStone<0)return false;return true;}
}
}
