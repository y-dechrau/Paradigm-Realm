using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The Civics tab (owner request, 6 Nov 2026): the state of your civilisation on one page. The ranked table follows the
// Demographics screens of the Civilization series, measured only against the home civilisations you have met; the rest is what
// you need to run the empire: where the money goes, what the army costs, and the state of society.
public partial class RealmGame {
 static readonly string[] CivicsRows={"Population","Cities","Land (tiles)","Literacy","Approval","Life expectancy","Trade (GNP)","Goods (shields)","Crop yield (food)","Science","Soldiers","Army strength"};
 List<CivicStats> civicsRivals;CivicStats civicsMine;int civicsTurn=-1;float civicsAt;
 void RefreshCivics(){
  if(civicsMine!=null&&civicsTurn==world.turn&&Time.realtimeSinceStartup-civicsAt<1f)return;civicsTurn=world.turn;civicsAt=Time.realtimeSinceStartup;civicsMine=world.StatsFor(0);
  civicsRivals=Enumerable.Range(1,world.civCount-1).Where(o=>!world.IsNativeNation(o)&&world.met[o]&&world.cities.Any(c=>c.owner==o)).Select(world.StatsFor).ToList();
 }
 static long CivicValue(CivicStats s,int row){switch(row){case 0:return s.population;case 1:return s.cities;case 2:return s.land;case 3:return s.literacy;case 4:return s.approval;case 5:return s.lifeExpectancy;case 6:return s.gnp;case 7:return s.goods;case 8:return s.crops;case 9:return s.science;case 10:return s.soldiers;default:return s.strength;}}
 static string CivicText(int row,long v){return row==0?v.ToString("N0"):row==3||row==4?v+"%":row==5?v+" years":v.ToString("N0");}
 void DrawCivics(){
  RefreshCivics();var me=civicsMine;var n=world.nations[0];
  // Demographics, ranked against the civilisations you know.
  GUILayout.BeginHorizontal();GUILayout.BeginVertical(GUILayout.Width(610));
  GUILayout.Label("Demographics",heading);
  GUILayout.BeginHorizontal();GUILayout.Label("",small,GUILayout.Width(170));GUILayout.Label("You",small,GUILayout.Width(120));GUILayout.Label("Rank",small,GUILayout.Width(70));GUILayout.Label("Best known",small,GUILayout.Width(120));GUILayout.Label("Average",small,GUILayout.Width(100));GUILayout.EndHorizontal();
  for(int r=0;r<CivicsRows.Length;r++){
   long mine=CivicValue(me,r);var others=civicsRivals.Select(s=>CivicValue(s,r)).ToList();int rank=1+others.Count(v=>v>mine);long best=others.Count==0?mine:Math.Max(mine,others.Max());long mean=others.Count==0?mine:(mine+others.Sum())/(others.Count+1);
   GUILayout.BeginHorizontal();GUILayout.Label(CivicsRows[r],body,GUILayout.Width(170));GUILayout.Label(CivicText(r,mine),body,GUILayout.Width(120));GUILayout.Label(rank+" of "+(others.Count+1),body,GUILayout.Width(70));GUILayout.Label(others.Count==0?"—":CivicText(r,best),body,GUILayout.Width(120));GUILayout.Label(others.Count==0?"—":CivicText(r,mean),body,GUILayout.Width(100));GUILayout.EndHorizontal();
  }
  GUILayout.Label(civicsRivals.Count==0?"Meet other civilisations to see how you compare.":"Compared with the "+civicsRivals.Count+" home "+(civicsRivals.Count==1?"civilisation":"civilisations")+" you have met.",small);
  GUILayout.Space(8);GUILayout.Label("Society",heading);
  var own=world.cities.Where(c=>c.owner==0).ToList();var largest=own.OrderByDescending(c=>c.pop).ThenBy(c=>c.id).FirstOrDefault();int unrest=own.Count(world.Disorder);
  GUILayout.Label("Government: "+World.Governments[n.government]+"   •   "+TechnologyCatalog.EraNames[world.Era(0)]+(world.AgeOf(0)!=World.AgeNormal?" ("+World.AgeNames[world.AgeOf(0)]+")":""),body);
  GUILayout.Label("Faith: "+(n.founded>=0?ReligionCatalog.Name(n.founded)+" (founder)":own.Any(c=>c.religion>=0)?ReligionCatalog.Name(own.Where(c=>c.religion>=0).GroupBy(c=>c.religion).OrderByDescending(g=>g.Count()).First().Key):"None yet"),body);
  GUILayout.Label("Largest city: "+(largest==null?"none":largest.name+" (size "+largest.pop+")")+"   •   Cities in disorder: "+unrest,body);
  GUILayout.Label("Technologies known: "+n.tech.Count(a=>a)+"   •   Wonders: "+own.Sum(c=>c.buildings.Count(World.IsWonder))+"   •   Roads: "+me.roads+" tiles",body);
  GUILayout.EndVertical();
  GUILayout.Space(30);GUILayout.BeginVertical(GUILayout.Width(480));
  var b=world.BudgetDetail(0);
  GUILayout.Label("Finances",heading);
  GUILayout.Label("Treasury "+world.gold+" gold   •   Net "+(b.Net>=0?"+":"")+b.Net+" a turn"+(b.Net<0?"   •   empty in "+Math.Max(1,world.gold/Math.Max(1,-b.Net))+" turns":""),body);
  CivicsMoney("City trade and commerce",b.cityGold);CivicsMoney("Buildings and connected cities",b.cityBonus);CivicsMoney("Ports",b.ports);CivicsMoney("Trade routes",b.routes);CivicsMoney("Treaties",b.treaties);CivicsMoney("Villages and tithes",b.tribute);
  CivicsMoney("City upkeep",-b.cityUpkeep);CivicsMoney("Building upkeep",-b.buildingUpkeep);CivicsMoney("Army support",-b.army);CivicsMoney("Civilian support",-b.civilians);CivicsMoney("Luxuries",-b.luxury);CivicsMoney("Treaty payments",-b.treatyCost);
  GUILayout.Space(8);GUILayout.Label("Economy",heading);
  GUILayout.Label("Priorities: Science "+n.scienceRate+"%   Military "+n.militaryRate+"%   Finance "+world.FinanceRate(0)+"%",body);
  int links=own.Sum(world.LinkTrade);GUILayout.Label("Cities joined by road, river or sea add "+links+" Trade   •   Trade per citizen: "+(own.Sum(c=>c.pop)==0?"0":(me.gnp/(float)own.Sum(c=>c.pop)).ToString("0.0")),body);
  {string line="Next technology: ";if(world.research<0)line+="none";else{int turns=world.ResearchTurns(world.research,0);line+=World.TechNames[world.research]+(turns<0?" (no science coming in)":" in "+turns+(turns==1?" turn":" turns"));}GUILayout.Label(line,body);}
  {int need;int spare=world.FoodBalance(0,out need);GUILayout.Label("Food: "+world.FoodStateName(0)+"   •   Units eat "+need+" a turn beyond what the fields give   •   "+(spare>=0?"+"+spare+" left for growth":"short by "+(-spare))+(world.UnfedUnits(0)>0?"   •   "+world.UnfedUnits(0)+" units cannot act":""),body);}
  GUILayout.Space(8);GUILayout.Label("Military",heading);
  var army=world.units.Where(u=>u.owner==0&&World.Power[u.type]>0).GroupBy(u=>u.type).OrderByDescending(g=>World.MilitaryValue(g.Key)).Select(g=>g.Count()+" "+World.UnitNames[g.Key]);
  GUILayout.Label(me.soldiers==0?"No soldiers.":string.Join(", ",army.ToArray()),body);
  var wars=Enumerable.Range(1,world.civCount-1).Where(o=>world.met[o]&&world.AtWar(0,o)).Select(world.NationName).ToArray();GUILayout.Label("Support "+b.army+" gold a turn   •   "+(wars.Length==0?"At peace":"At war with "+string.Join(", ",wars)),body);
  GUILayout.EndVertical();GUILayout.EndHorizontal();
 }
 void CivicsMoney(string name,int amount){if(amount==0)return;GUILayout.BeginHorizontal();GUILayout.Label(name,body,GUILayout.Width(300));GUILayout.Label((amount>0?"+":"")+amount,body,GUILayout.Width(80));GUILayout.EndHorizontal();}
 // Progress bars carry a pattern of the age you are in (owner request, 6 Nov 2026).
 readonly Texture2D[] eraHatches=new Texture2D[TechnologyCatalog.EraCount];
 void EraBar(Rect r,float fraction){
  Fill(r,UiTrough);int era=Mathf.Clamp(world.Era(0),0,eraHatches.Length-1);
  if(eraHatches[era]==null){var tex=PixelTexture(PixelArt.EraHatch(era));tex.wrapMode=TextureWrapMode.Repeat;tex.filterMode=FilterMode.Point;themeTextures.Add(tex);eraHatches[era]=tex;}
  float w=r.width*Mathf.Clamp01(fraction);if(w<=0)return;
  GUI.DrawTextureWithTexCoords(new Rect(r.x,r.y,w,r.height),eraHatches[era],new Rect(0,0,w/16f,r.height/16f));
 }
}
}
