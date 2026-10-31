using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Civilisation summary and lore (owner request, 31 Oct 2026): on the New Game screen each civilisation shows its traits and
// four unique rules; "History and lore" opens its background, history, strengths and weaknesses, leader agenda and faiths.
public partial class RealmGame {
 bool civLoreOpen;Vector2 civLoreScroll;
 static string CivRules(CivEntry e){return "Ability — "+e.Ability+": "+e.AbilityText+"\nUnique unit — "+e.Unit+": "+e.UnitText+"\nUnique building — "+e.Building+": "+e.BuildingText+"\nWeakness — "+e.Weakness+": "+e.WeaknessText;}
 static string CivFaiths(CivEntry e){return e.Faiths.Length==0?"No traditional faith: it adopts whatever faith it meets.":string.Join(", ",e.Faiths.Select(ReligionCatalog.Name).ToArray());}
 void DrawCivilisationSummary(int civ){
  var e=CivLore.Of(civ);var traits=CivilisationCatalog.Traits[civ];
  GUI.Label(new Rect(68,440,880,30),CivilisationCatalog.Names[civ]+" — led by "+CivilisationCatalog.Leaders[civ]+(e!=null?"  ·  "+e.Epithet:""),heading);
  if(e!=null&&GUI.Button(new Rect(960,440,252,32),"History and lore",button)){civLoreOpen=true;civLoreScroll=Vector2.zero;GUIUtility.ExitGUI();}
  for(int k=0;k<2;k++)GUI.Label(new Rect(68,474+k*22,1144,22),"Trait — "+CivilisationCatalog.TraitNames[traits[k]]+": "+CivilisationCatalog.TraitTexts[traits[k]],small);
  if(e!=null){GUI.Label(new Rect(68,522,1144,92),CivRules(e),small);GUI.Label(new Rect(68,616,1144,40),"Strengths: "+e.Strengths+"   Weaknesses: "+e.Weaknesses+"\nAs a rival, "+CivilisationCatalog.Leaders[civ]+" is a "+CivLore.AgendaNames[e.Agenda]+": "+CivLore.AgendaTexts[e.Agenda],small);}
  else GUI.Label(new Rect(68,522,1144,24),"Rivals are drawn from the other civilisations, each with its own leader and traits.",small);
 }
 void DrawCivilisationLore(int civ){
  var e=CivLore.Of(civ);if(e==null){civLoreOpen=false;return;}
  var panel=new Rect(60,60,1160,690);Fill(new Rect(0,0,1280,800),UiDim);GUI.Box(panel,"");
  var face=Portrait(CivilisationCatalog.Portraits[civ]);if(face!=null)DrawPortrait(new Rect(84,84,300,400),face);
  GUI.Label(new Rect(84,494,300,30),CivilisationCatalog.Leaders[civ],heading);GUI.Label(new Rect(84,524,300,44),CivilisationCatalog.PortraitTitles[CivilisationCatalog.Portraits[civ]],small);
  GUILayout.BeginArea(new Rect(408,80,792,600));civLoreScroll=GUILayout.BeginScrollView(civLoreScroll);
  GUILayout.Label(CivilisationCatalog.Names[civ],title);GUILayout.Label(e.Epithet,heading);
  GUILayout.Label("Background",heading);GUILayout.Label(e.Background,body);
  GUILayout.Label("History",heading);GUILayout.Label(e.History,body);
  GUILayout.Label("Strengths",heading);GUILayout.Label(e.Strengths,body);GUILayout.Label("Weaknesses",heading);GUILayout.Label(e.Weaknesses,body);
  GUILayout.Label("In play",heading);var traits=CivilisationCatalog.Traits[civ];GUILayout.Label("Traits — "+CivilisationCatalog.TraitLine(traits)+"\n"+CivRules(e),body);
  GUILayout.Label("As a rival",heading);GUILayout.Label(CivLore.AgendaNames[e.Agenda]+" — "+CivLore.AgendaTexts[e.Agenda],body);
  GUILayout.Label("Faiths",heading);GUILayout.Label(CivFaiths(e),body);
  if(civ>=17)GUILayout.Label("The portrait is a placeholder until a painted one is added (Resources/Leaders/leader-"+CivilisationCatalog.Portraits[civ].ToString("00")+".png).",small);
  GUILayout.EndScrollView();GUILayout.EndArea();
  if(GUI.Button(new Rect(84,690,300,40),"Close",button)){civLoreOpen=false;GUIUtility.ExitGUI();}
 }
 // The player's own civilisation, for the Identity screen.
 void DrawOwnCivilisationLore(){
  var me=world.nations[0];var e=CivLore.Of(me.civ);if(e==null)return;
  GUILayout.Label(e.Epithet,heading);GUILayout.Label(e.Background,body);GUILayout.Label(e.History,body);GUILayout.Label(CivRules(e),small);
  GUILayout.Label("Strengths: "+e.Strengths+"   Weaknesses: "+e.Weaknesses,small);
 }
}
}
