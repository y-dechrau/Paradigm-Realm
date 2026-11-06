using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// When one of your cities finishes a unit or building a small message says so and offers what to make next (owner request,
// 6 Nov 2026). It waits on the map without stopping play, and any left unanswered go when you end the next turn.
public sealed class BuildNotice {public int city;public string text;public bool building;}
// Another leader wants your attention (owner request, 6 Nov 2026): a proposal waiting for your answer, a denunciation or a
// declaration of war. offer is the proposal's id, or 0 when there is nothing to answer.
public sealed class RivalCall {public int from,offer;public string text;}
public partial class World {
 [NonSerialized] public List<BuildNotice> buildNotices=new List<BuildNotice>();
 void AddBuildNotice(City c,int b,bool building,string verb){if(c.owner!=0)return;if(buildNotices==null)buildNotices=new List<BuildNotice>();buildNotices.Add(new BuildNotice{city=c.id,building=building,text=c.name+" "+verb+" "+BuildNameFor(0,b)+"."});}
 [NonSerialized] public List<RivalCall> rivalCalls=new List<RivalCall>();
 void AddRivalCall(int from,int offer,string text){if(from<=0||from>=civCount)return;if(rivalCalls==null)rivalCalls=new List<RivalCall>();rivalCalls.Add(new RivalCall{from=from,offer=offer,text=text});}
 // A call about a proposal lapses when the proposal has been answered or has run out.
 public RivalCall NextRivalCall(){if(rivalCalls==null)return null;rivalCalls.RemoveAll(c=>c.offer>0&&!offers.Any(o=>o.id==c.offer&&o.status==0));return rivalCalls.Count>0?rivalCalls[0]:null;}
 public static string CallText(DiplomaticOffer o){return o.kind==TreatyKind.Peace?"offers peace.":o.kind==TreatyKind.Trade?"proposes a trade.":o.kind==TreatyKind.Access?"asks for open borders.":o.kind==TreatyKind.Friendship?"offers friendship.":o.kind==TreatyKind.Alliance?"proposes an alliance.":o.kind==TreatyKind.Research?"proposes a research pact.":o.kind==TreatyKind.BorderPromise?"asks for a border promise.":o.kind==TreatyKind.JointWar?"proposes a joint war.":"wants to talk.";}
 // What this city might make next on that line, and why; -1 if nothing is called for.
 public int SuggestNext(City c,bool building,out string why){
  why="";if(c==null)return -1;
  // Another unit is only suggested when the city has nothing else lined up on either line (owner request, 6 Nov 2026).
  if(!building&&(c.build>=0||c.queue.Count>0||c.buildingProject>=0||c.buildingQueue.Count>0))return -1;
  if(building){
   int b=PreferredBuilding(c);if(b<0)return -1;var v=Yield(c);
   why=(b==36||b==33||b==42)&&Happiness(c)<1?"Its citizens are close to unrest.":(b==37||b==41)&&c.pop>=GrowthLimit(c)-1?"It is near its growth limit.":b==7?"It stores food, so the city grows faster.":(b==31||b==34||b==39)&&Budget(0)[2]<2?"Your treasury is tight.":b==9?"Enemies are close.":b==43&&v[0]<=0?"The city is short of food.":b==PortBuilding?"It opens the sea to you.":b==30||b==40?"It raises production here.":b==8||b==38?"It raises science here.":"It is the next useful building here.";
   return b;
  }
  var options=Choices(c);int mine=cities.Count(a=>a.owner==0),soldiers=SoldierCount(0);
  if(AtWarWithAnyone(0)||soldiers<mine){var best=options.Where(b=>BuildUnit(b)>=2&&!IsNaval(BuildUnit(b))&&!IsRanged(BuildUnit(b))&&MilitaryValue(BuildUnit(b))>0).OrderByDescending(b=>MilitaryValue(BuildUnit(b))).DefaultIfEmpty(-1).First();if(best>=0){why=AtWarWithAnyone(0)?"You are at war.":"Your cities need defending.";return best;}}
  if(units.Count(u=>u.owner==0&&u.type==1)<mine&&options.Contains(UnitBuild(1))){why="You have fewer Workers than cities.";return UnitBuild(1);}
  if(mine<5&&!units.Any(u=>u.owner==0&&u.type==0)&&options.Contains(UnitBuild(0))&&c.pop>=3){why="There may be room for another city.";return UnitBuild(0);}
  return -1;
 }
}
}
