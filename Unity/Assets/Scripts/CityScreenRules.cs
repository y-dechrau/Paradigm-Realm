using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// What the redesigned city screen asks of the rules (owner request, 16 Nov 2026): where each total comes from, when the city
// next grows, reordering a production line, and what a building would add before it is built. Nothing here changes a rule.
public partial class World {
 // The parts of a city total, in order: 0 food, 1 shields, 2 science, 3 gold. The last part is whatever the rules add on top
 // of the land (buildings, specialists, edicts, traits, ages, resources, disorder and occupation), so the parts always add up.
 public List<KeyValuePair<string,int>> YieldBreakdown(City c,int stat){
  var parts=new List<KeyValuePair<string,int>>();var v=Yield(c);
  if(stat<=1){
   var centre=CityCentreYield(c);var worked=Worked(c);int tiles=worked.Sum(t=>EffectiveTileYield(c,t)[stat]+MinorTileBonus(c.owner,t,stat));
   parts.Add(new KeyValuePair<string,int>("City centre",centre[stat]));
   parts.Add(new KeyValuePair<string,int>("Worked land ("+worked.Count+(worked.Count==1?" tile)":" tiles)"),tiles));
   int eaten=stat==0&&!HasTech(c.owner,"uploading")?c.pop*2:0;if(eaten>0)parts.Add(new KeyValuePair<string,int>("Eaten by "+c.pop+(c.pop==1?" citizen":" citizens"),-eaten));
   int rest=v[stat]-(centre[stat]+tiles-eaten);if(rest!=0)parts.Add(new KeyValuePair<string,int>(Disorder(c)&&stat==1?"Disorder: no shields":"Buildings, edicts and other bonuses"+(stat==1?StatecraftPart(c):""),rest));
  }else{
   int trade=Trade(c),share=stat==2?ScienceShare(c):GoldShare(c);
   string how=c.focus==3?(stat==2?"Science focus: all "+trade+" trade":"Science focus: no trade to gold"):c.focus==4?(stat==3?"Commerce focus: all "+trade+" trade":"Commerce focus: no trade to science"):"Trade "+trade+" at "+(stat==2?nations[c.owner].scienceRate+"% science":(100-nations[c.owner].scienceRate)+"% gold");
   parts.Add(new KeyValuePair<string,int>(how,share));
   int rest=v[stat]-share;if(rest!=0)parts.Add(new KeyValuePair<string,int>(Disorder(c)&&stat==3?"Disorder: no gold":"Specialists, buildings and other bonuses"+StatecraftPart(c),rest));
  }
  return parts;
 }
 string StatecraftPart(City c){int p=StatecraftPercent(c.owner);return p>0?" (Centralised +"+p+"%)":"";}
 // Turns until the next citizen, or -1 when the city is not growing (shortage, no surplus, or at its population limit).
 public int GrowthTurns(City c){if(Disorder(c))return -1;int net=Yield(c)[0]-c.ration;if(net<=0||c.pop>=GrowthLimit(c))return -1;return Math.Max(1,(FoodNeeded(c)-c.food+net-1)/net);}
 // Turns until each item of a production line is done: the item in progress first, then the queue in order.
 public List<int> LineTurns(City c,bool building){
  var turns=new List<int>();int current=building?c.buildingProject:c.build,progress=building?c.buildingProduction:c.production,output=Math.Max(1,LineShare(c,building)),sum=0;
  if(current>=0){sum=Math.Max(1,(CostFor(c,current)-progress+output-1)/output);turns.Add(sum);}
  foreach(int b in building?c.buildingQueue:c.queue){sum+=Math.Max(1,(CostFor(c,b)+output-1)/output);turns.Add(sum);}
  return turns;
 }
 public bool MoveQueued(City c,bool building,int index,int dir){
  if(c==null||result!="")return false;var q=building?c.buildingQueue:c.queue;int j=index+dir;if(index<0||index>=q.Count||j<0||j>=q.Count)return false;
  int keep=q[index];q[index]=q[j];q[j]=keep;return true;
 }
 public bool RemoveQueued(City c,bool building,int index){if(c==null||result!="")return false;var q=building?c.buildingQueue:c.queue;if(index<0||index>=q.Count)return false;q.RemoveAt(index);return true;}
 // What a building would add to the city as it stands now: the change in food, shields, science, gold and faith, and the tiles
 // whose yield it raises. It is tried on the city for a moment and taken away again. Null for a unit or a building it has.
 public int[] PreviewBuilding(City c,int b,List<Tile> improved){
  if(improved!=null)improved.Clear();if(c==null||BuildUnit(b)>=0||b<0||HasBuilding(c,b))return null;
  var land=Workable(c);var before=land.Select(t=>EffectiveTileYield(c,t)).ToList();var was=Yield(c);int faithWas=FaithYield(c);
  PreviewSet(c,b,true);
  try{
   if(improved!=null)for(int i=0;i<land.Count;i++){var now=EffectiveTileYield(c,land[i]);if(now[0]>before[i][0]||now[1]>before[i][1]||now[2]>before[i][2])improved.Add(land[i]);}
   var v=Yield(c);return new[]{v[0]-was[0],v[1]-was[1],v[2]-was[2],v[3]-was[3],FaithYield(c)-faithWas};
  }finally{PreviewSet(c,b,false);}
 }
 static void PreviewSet(City c,int b,bool on){if(b==7)c.granary=on;else if(b==8)c.library=on;else if(b==9)c.walls=on;else if(on)c.buildings.Add(b);else c.buildings.Remove(b);}
}
}
