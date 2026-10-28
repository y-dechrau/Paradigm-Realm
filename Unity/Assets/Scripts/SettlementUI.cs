using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm { public partial class RealmGame {
 List<SettlementReport> settlementReports=new List<SettlementReport>();
 int settlementUnit=-1,settlementTurn=-1;
 void OpenSettlementAdviser(Unit u){settlementUnit=u.id;settlementTurn=world.turn;settlementReports=world.SettlementOptions(u);OpenDetails("Settlement adviser");}
 void DrawSettlementUnit(Unit u){
  var centre=World.CentreYield(world.At(u.x,u.y).terrain);
  GUILayout.Label("City centre here: "+centre[0]+" food, "+centre[1]+" production. Early population limit "+(world.FreshWater(u.x,u.y)?8:6)+".",small);
  string warning=world.SettlementDiplomacyWarning(u.owner,u.x,u.y);if(warning!="")GUILayout.Label(warning,small);
  string reason=world.FoundReason(u);if(reason!="")GUILayout.Label(reason,small);
  if(Button("Compare settlement sites"))OpenSettlementAdviser(u);
 }
 void DrawSettlementAdviser(){
  var u=world.units.Find(a=>a.id==settlementUnit&&a.owner==0&&a.type==0);if(u==null){GUILayout.Label("Select a Settler to compare sites.",body);return;}
  GUILayout.Label("Explored sites within 12 tiles. Suggestions use known information and current routes. You choose where to settle; travel never founds a city automatically.",body);
  if(settlementTurn!=world.turn||Button("Refresh site suggestions")){settlementReports=world.SettlementOptions(u);settlementTurn=world.turn;}
  if(settlementReports.Count==0)GUILayout.Label("No reachable safe site found in the local search. Explore farther or move away from existing cities. You can still inspect and found a legal site manually.",body);
  foreach(var r in settlementReports){
   GUILayout.Space(12);GUILayout.Label("Site "+r.x+", "+r.y+" | comparative score "+r.score+" | "+r.distance+" tiles away",heading);
   GUILayout.Label(world.SettlementSummary(r),body);
   bool enabled=GUI.enabled;GUI.enabled=enabled&&world.result=="";
   if(Button(r.x==u.x&&r.y==u.y?"Return to Settler here":"Travel to "+r.x+", "+r.y)){
    selected=u.id;cityId=-1;detailsScreen="";cityScreen=false;
    if(r.x!=u.x||r.y!=u.y)IssueOrder(u,r.x,r.y);else status="Press Found city [B] when ready.";
    GUI.enabled=enabled;GUIUtility.ExitGUI();
   }
   GUI.enabled=enabled;
  }
 }
}}
