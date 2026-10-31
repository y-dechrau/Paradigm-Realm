using System;
using System.Linq;
namespace ParadigmRealm {
// Era score and Golden, Dark and Heroic Ages (owner request, 31 Oct 2026: improve the mechanics after comparing similar games;
// from Civilization VI's era score and ages, with Humankind's fame as a model for counting deeds). During each era a
// civilisation earns era score for notable deeds: founding a city +2, a keystone technology +1, a wonder +4, a battle won +1
// (at most 6 a era), a city converted to its own faith +2, founding a faith +3, meeting another civilisation +1. When it
// enters the next era the score decides the age that follows, for 15 turns:
// - Golden Age (score at least 14 + 2 x era): +1 production, +2 gold and +1 content citizen in every city.
// - Heroic Age (a Golden Age straight after a Dark Age): the Golden Age plus +1 science in every city.
// - Dark Age (score below 6 + era): -1 content citizen in every city, but +2 faith in every city as people turn to their gods.
// - otherwise a Normal Age. The score then starts again from 0.
public partial class World {
 public const int AgeLength=15,AgeNormal=0,AgeGolden=1,AgeDark=2,AgeHeroic=3;
 public static readonly string[] AgeNames={"Normal Age","Golden Age","Dark Age","Heroic Age"};
 public int GoldenThreshold(int owner){return 14+2*Era(owner);}
 public int DarkThreshold(int owner){return 6+Era(owner);}
 public void AddEraScore(int owner,int points,string why){if(owner<0||owner>=civCount||points<=0)return;var n=nations[owner];n.eraScore+=points;if(owner==0&&points>=2)Note("Era score +"+points+": "+why+" ("+n.eraScore+").");}
 public int AgeOf(int owner){var n=nations[owner];return n.ageTurns>0?n.age:AgeNormal;}
 void BeginAge(int owner){
  var n=nations[owner];int before=n.age;int score=n.eraScore;int kind=score>=GoldenThreshold(owner)?(before==AgeDark&&n.ageTurns>=0?AgeHeroic:AgeGolden):score<DarkThreshold(owner)?AgeDark:AgeNormal;
  n.age=kind;n.ageTurns=kind==AgeNormal?0:AgeLength;n.eraScore=0;n.battleScore=0;
  if(owner==0)Note(TechnologyCatalog.EraNames[Era(owner)]+" begins with era score "+score+": a "+AgeNames[kind]+(kind==AgeNormal?".":" for "+AgeLength+" turns.")+(kind==AgeGolden?" +1 production, +2 gold and +1 content in every city.":kind==AgeHeroic?" Out of the darkness: Golden Age bonuses plus +1 science in every city.":kind==AgeDark?" -1 content in every city, but +2 faith in every city.":""));
  else if(KnowsNation(owner)&&kind!=AgeNormal)Note(NationName(owner)+" enters a "+AgeNames[kind]+".");
 }
 void AgeTurn(int owner){var n=nations[owner];if(n.ageTurns>0){n.ageTurns--;if(n.ageTurns==0&&owner==0)Note("The "+AgeNames[n.age]+" has ended.");}}
 void ApplyAgeYields(City c,ref int p,ref int s,ref int g){int a=AgeOf(c.owner);if(a==AgeGolden||a==AgeHeroic){p++;g+=2;}if(a==AgeHeroic)s++;}
 public int AgeContent(City c){int a=AgeOf(c.owner);return a==AgeGolden||a==AgeHeroic?1:a==AgeDark?-1:0;}
 public int AgeFaith(City c){return AgeOf(c.owner)==AgeDark?2:0;}
 void TechEraScore(int owner,int tech,int eraBefore){if(TechnologyCatalog.IsKeystone(tech))AddEraScore(owner,1,TechNames[tech]+" discovered");if(Era(owner)>eraBefore)BeginAge(owner);}
 void BattleEraScore(int owner){var n=nations[owner];if(n.battleScore<6){n.battleScore++;AddEraScore(owner,1,"a battle won");}}
 public string AgeLine(int owner){var n=nations[owner];return "Era score "+n.eraScore+" (Golden Age at "+GoldenThreshold(owner)+", Dark Age below "+DarkThreshold(owner)+")"+(n.ageTurns>0?" · "+AgeNames[n.age]+", "+n.ageTurns+" turns left":"");}
}
}
