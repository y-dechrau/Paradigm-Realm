using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The gateway panel (owner design, 10 Nov 2026; GateExpedition.cs): opened from the Gateway action of any of your units standing
// on a sealed gateway, and by itself when one of your investigators finishes reading a seal. It shows what has been learned
// (where the gateway leads, how dangerous the far side probably is, what the seal asks for and what you still lack) and the
// three approaches, each with its exact cost and consequence written beside its button before you commit.
public partial class RealmGame {
 Tile gatePanel;int gatePartner,gateSide;
 void OpenGatePanel(Tile gate){gatePanel=gate;gatePartner=0;ResetMapGesture();}
 // An open gateway: who holds its tile holds the crossing, and may close it (owner design, 11 Nov 2026).
 void DrawOpenGatePanel(Tile gate)
 {
  Fill(new Rect(0,0,1280,800),new Color(0,0,0,.72f));var box=new Rect(190,170,900,420);DipPanel(box,.98f);int style=world.GateStyle(gate),realm=world.GateRealm(gate);
  var art=SpriteSlot("gate-open-"+style);var pic=new Rect(box.x+24,box.y+70,230,230);DipPanel(new Rect(pic.x-4,pic.y-4,pic.width+8,pic.height+8),1);if(art!=null)GUI.DrawTexture(pic,art,ScaleMode.ScaleToFit,true);
  GUI.Label(new Rect(box.x,box.y+12,box.width,40),"THE "+World.GateStyleNames[style].ToUpperInvariant(),dipTitle);
  var text=new GUIStyle(dipRow){fontSize=14,wordWrap=true};float x=box.x+280,w=box.width-304,y=box.y+66;
  GUI.Label(new Rect(x,y,w,70),"This gateway stands open to the "+World.RealmNames[realm]+", in both directions. Whoever holds its tile holds the crossing: no one can pass a tile that another civilisation's units stand on, short of fighting for it.",text);y+=78;
  DipHeading(new Rect(x,y,w,22),"Close the gateway");y+=28;
  GUI.Label(new Rect(x,y,w,70),"Closing takes a full turn, and one of your units must still hold the gateway when the turn ends. It cannot be closed while an enemy stands beside it. Once closed it is sealed for everyone, and opening it again means meeting its seal again.",text);y+=78;
  bool closing=world.ClosingGate(gate);string reason=closing?"":world.CloseReason(0,gate);
  if(closing)GUI.Label(new Rect(x,y,w,24),"Closing: it seals at the start of next turn if you still hold it.",new GUIStyle(text){normal={textColor=DipGood}});
  else{bool was=GUI.enabled;GUI.enabled=reason=="";if(DipButton(new Rect(x,y,200,36),"Close the gateway",new Color(.45f,.08f,.07f))){if(world.BeginClose(0,gate))status="The gateway is closing.";}GUI.enabled=was;if(reason!="")GUI.Label(new Rect(x+212,y+6,w-212,24),reason,new GUIStyle(text){normal={textColor=DipBad}});}
  if(DipButton(new Rect(box.x+24,box.yMax-60,230,40),"Close this panel",new Color(.1f,.16f,.3f))){gatePanel=null;GUIUtility.ExitGUI();}
 }
 void DrawGatePanel()
 {
  GUI.enabled=true;DiplomacyStyles();HudStyles();var gate=gatePanel;if(gate==null){gatePanel=null;return;}if(world.GateSteady(gate)){DrawOpenGatePanel(gate);return;}
  Fill(new Rect(0,0,1280,800),new Color(0,0,0,.72f));var box=new Rect(190,90,900,620);DipPanel(box,.98f);
  int style=world.GateStyle(gate),realm=world.GateRealm(gate);var study=world.StudyOf(0,gate);bool known=study!=null&&study.known;
  // The gateway's sealed picture on the left.
  var art=SpriteSlot("gate-sealed-"+style);var pic=new Rect(box.x+24,box.y+70,230,230);DipPanel(new Rect(pic.x-4,pic.y-4,pic.width+8,pic.height+8),1);if(art!=null)GUI.DrawTexture(pic,art,ScaleMode.ScaleToFit,true);
  GUI.Label(new Rect(box.x,box.y+12,box.width,40),"THE "+World.GateStyleNames[style].ToUpperInvariant(),dipTitle);
  var text=new GUIStyle(dipRow){fontSize=14,wordWrap=true};var small2=new GUIStyle(dipRow){fontSize=12,wordWrap=true};float x=box.x+280,w=box.width-304,y=box.y+66;
  Color navy=new Color(.1f,.16f,.3f),teal=new Color(.07f,.38f,.4f),red=new Color(.45f,.08f,.07f),blue=new Color(.08f,.2f,.3f);
  if(!known)
  {
   int done=study==null?0:study.turns;bool here=world.units.Any(u=>u.owner==0&&u.x==gate.x&&u.y==gate.y&&World.Investigator(u.type));
   GUI.Label(new Rect(x,y,w,60),(world.ConditionOf(gate)==GateCondition.Cycling?"This gateway opens and shuts by itself, and can be used whenever it is open. Reading its seal shows how to steady it so that it stays open.":world.ConditionOf(gate)==GateCondition.Sealed?"The gateway is sealed, and nobody living knows how. Its seal can be read.":"The gateway is shut. Reading its seal shows what it needs."),text);y+=56;
   DipHeading(new Rect(x,y,w,22),"Investigate");y+=28;
   GUI.Label(new Rect(x,y,w,90),"Keep a Scout, Medic or Priest on the gateway for "+GateApproach.StudyTurns+" turns. They will learn where it leads, what lies there, what the seal asks for and roughly how dangerous the far side is.",text);y+=92;
   GUI.Label(new Rect(x,y,w,24),"Read so far: "+done+" of "+GateApproach.StudyTurns+" turns."+(here?"  An investigator is at work.":"  No investigator is on the gateway."),new GUIStyle(text){normal={textColor=here?DipGood:DipPlain}});y+=40;
   {var unknown=new GUIContent(string.Join(" ",world.CompatibilityReport(0,gate).ToArray())+"  A city within "+DimensionSettings.InvestigateRange+" tiles can also investigate it with its production (Investigate Gateway).");GUI.Label(new Rect(x,y,w,48),unknown,small2);y+=52;}
   GUI.Label(new Rect(x,y,w,60),"It can still open in the older ways: the first civilisation to learn its world's key technology, the Gatekeeper's Key wonder, or a priest's rite of unsealing.",small2);
  }
  else
  {
   var need=world.GateNeed(gate,0);
   GUI.Label(new Rect(x,y,w,24),"Leads to the "+World.RealmNames[realm]+".   Danger beyond: "+world.DangerText(gate,0)+".",new GUIStyle(text){fontStyle=FontStyle.Bold});y+=26;
   // What the investigation confirmed about the far side: its physical rules, hazards and what fails there (CompatibilityReport).
   if(realm>0&&realm<World.RealmTexts.Length){var rt=new GUIContent(string.Join(" ",world.CompatibilityReport(0,gate).ToArray()));var tiny=new GUIStyle(small2){fontSize=11};float rh=Mathf.Min(86,tiny.CalcHeight(rt,w));GUI.Label(new Rect(x,y,w,rh),rt,tiny);y+=rh+4;}
   DipHeading(new Rect(x,y,w,22),"What the seal asks");y+=26;
   {var mc=new GUIContent(need.main);float mh=text.CalcHeight(mc,w);GUI.Label(new Rect(x,y,w,mh),mc,text);y+=mh+2;}
   if(need.activation!=""){var ac=new GUIContent(need.activation);float ah=text.CalcHeight(ac,w);GUI.Label(new Rect(x,y,w,ah),ac,text);y+=ah+2;}
   {string lacks=world.GateLacks(0,gate);GUI.Label(new Rect(x,y,w,22),lacks==""?"You have everything it asks for.":lacks,new GUIStyle(text){normal={textColor=lacks==""?DipGood:DipBad}});y+=30;}
   if(study.approach!=GateApproach.None)
   {
    DipHeading(new Rect(x,y,w,22),GateApproach.Names[study.approach]+" under way");y+=28;int left=Math.Max(0,study.ready-world.turn);
    GUI.Label(new Rect(x,y,w,60),(left>0?"The gateway opens in "+left+" turn"+(left==1?"":"s"):"The gateway is ready")+", so long as one of your units stands on it then."+(study.partner>=0?"  "+world.NationName(study.partner)+" is paying half.":""),text);
   }
   else
   {
    DipHeading(new Rect(x,y,w,22),"Choose how to open it");if(world.FarStyle(gate)==8){GUI.Label(new Rect(x+w-300,y-2,70,24),"Empower",small2);gateSide=Cycle(new Rect(x+w-230,y-2,230,24),gateSide,GateApproach.RiftSides);}y+=28;
    var partners=world.GatePartners(0,gate);if(gatePartner>=partners.Count)gatePartner=0;int partner=partners.Count>0?partners[gatePartner]:-1;
    for(int approach=1;approach<=3;approach++)
    {
     string reason=world.ApproachReason(0,gate,approach,partner),outlook=world.GateOutlook(0,gate,approach,partner);var row=new Rect(x,y,w,approach==GateApproach.Negotiate?92:74);
     bool enabled=GUI.enabled;GUI.enabled=reason=="";
     if(DipButton(new Rect(row.x,row.y,170,36),GateApproach.Names[approach],approach==GateApproach.Force?red:approach==GateApproach.Stabilise?teal:blue))
     {
      if(world.BeginApproach(0,gate,approach,partner,gateSide)){status=approach==GateApproach.Force?"The seal is forced.":GateApproach.Names[approach]+" has begun.";if(world.GateOpen(gate)){gatePanel=null;StartGateScene(gate);GUI.enabled=enabled;return;}}
     }
     GUI.enabled=enabled;
     GUI.Label(new Rect(row.x+180,row.y-2,row.width-180,approach==GateApproach.Negotiate?52:54),outlook,small2);
     if(reason!="")GUI.Label(new Rect(row.x+180,row.y+(approach==GateApproach.Negotiate?50:52),row.width-180,20),reason,new GUIStyle(small2){wordWrap=false,clipping=TextClipping.Clip,normal={textColor=DipBad}});
     if(approach==GateApproach.Negotiate&&partners.Count>0){GUI.Label(new Rect(row.x,row.y+42,60,24),"Partner",small2);gatePartner=Cycle(new Rect(row.x,row.y+62,170,24),gatePartner,partners.Select(p=>world.NationName(p)).ToArray());}
     y+=row.height+4;
    }
   }
  }
  if(DipButton(new Rect(box.x+24,box.yMax-60,230,40),"Close",navy)){gatePanel=null;GUIUtility.ExitGUI();}
  GUI.Label(new Rect(box.x+24,box.y+312,230,230),(world.ConditionOf(gate)==GateCondition.Cycling?GateCondition.Names[GateCondition.Cycling]+". ":"")+world.GateSchedule(gate)+"\n\nGold "+world.gold+"\nFaith "+world.nations[0].faith+"\nInfluence "+world.nations[0].influence+"\n\nOne of your units must be on the gateway to begin, and again when it opens.",small2);
 }
}
}
