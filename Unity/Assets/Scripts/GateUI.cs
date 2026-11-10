using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The gateway panel (owner design, 10 Nov 2026; GateExpedition.cs): opened from the Gateway action of any of your units standing
// on a sealed gateway, and by itself when one of your investigators finishes reading a seal. It shows what has been learned
// (where the gateway leads, how dangerous the far side probably is, what the seal asks for and what you still lack) and the
// three approaches, each with its exact cost and consequence written beside its button before you commit.
public partial class RealmGame {
 Tile gatePanel;int gatePartner;
 void OpenGatePanel(Tile gate){gatePanel=gate;gatePartner=0;ResetMapGesture();}
 void DrawGatePanel()
 {
  GUI.enabled=true;DiplomacyStyles();HudStyles();var gate=gatePanel;if(gate==null||world.GateOpen(gate)){gatePanel=null;return;}
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
   GUI.Label(new Rect(x,y,w,60),"The gateway is sealed, and nobody living knows how. Its seal can be read.",text);y+=56;
   DipHeading(new Rect(x,y,w,22),"Investigate");y+=28;
   GUI.Label(new Rect(x,y,w,90),"Keep a Scout, Medic or Priest on the gateway for "+GateApproach.StudyTurns+" turns. They will learn where it leads, what lies there, what the seal asks for and roughly how dangerous the far side is.",text);y+=92;
   GUI.Label(new Rect(x,y,w,24),"Read so far: "+done+" of "+GateApproach.StudyTurns+" turns."+(here?"  An investigator is at work.":"  No investigator is on the gateway."),new GUIStyle(text){normal={textColor=here?DipGood:DipPlain}});y+=40;
   GUI.Label(new Rect(x,y,w,60),"It can still open in the older ways: the first civilisation to learn its world's key technology, the Gatekeeper's Key wonder, or a priest's rite of unsealing.",small2);
  }
  else
  {
   var need=world.GateNeed(gate);int danger=world.GateDanger(gate);
   GUI.Label(new Rect(x,y,w,24),"Leads to the "+World.RealmNames[realm]+".   Danger beyond: probably "+GateApproach.Danger[danger].ToLower()+".",new GUIStyle(text){fontStyle=FontStyle.Bold});y+=26;
   if(realm>0&&realm<World.RealmTexts.Length){var rt=new GUIContent(World.RealmTexts[realm]);float rh=Mathf.Min(74,small2.CalcHeight(rt,w));GUI.Label(new Rect(x,y,w,rh),rt,small2);y+=rh+6;}
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
    DipHeading(new Rect(x,y,w,22),"Choose how to open it");y+=28;
    var partners=world.GatePartners(0,gate);if(gatePartner>=partners.Count)gatePartner=0;int partner=partners.Count>0?partners[gatePartner]:-1;
    for(int approach=1;approach<=3;approach++)
    {
     string reason=world.ApproachReason(0,gate,approach,partner),outlook=world.GateOutlook(0,gate,approach,partner);var row=new Rect(x,y,w,approach==GateApproach.Negotiate?92:74);
     bool enabled=GUI.enabled;GUI.enabled=reason=="";
     if(DipButton(new Rect(row.x,row.y,170,36),GateApproach.Names[approach],approach==GateApproach.Force?red:approach==GateApproach.Stabilise?teal:blue))
     {
      if(world.BeginApproach(0,gate,approach,partner)){status=approach==GateApproach.Force?"The seal is forced.":GateApproach.Names[approach]+" has begun.";if(world.GateOpen(gate)){gatePanel=null;StartGateScene(gate);GUI.enabled=enabled;return;}}
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
  GUI.Label(new Rect(box.x+24,box.y+312,230,120),"Gold "+world.gold+"\nFaith "+world.nations[0].faith+"\nInfluence "+world.nations[0].influence+"\n\nOne of your units must be on the gateway to begin, and again when it opens.",small2);
 }
}
}
