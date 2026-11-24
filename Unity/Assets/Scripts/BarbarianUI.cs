using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The barbarian camp panel (owner request, 24 Nov 2026; rules in BarbarianDeals.cs): left-click a camp you have found, or one of its
// raiders, to pay it off or hire it against a civilisation you have met. Every price and the reason a button is greyed is written
// beside it. The arrows step through every camp you can bargain with.
public partial class RealmGame {
 int campPanel=-1,campTarget;
 BarbarianCamp CampPanelCamp(){return campPanel<0||world==null||world.camps==null?null:world.camps.Find(c=>c.id==campPanel&&c.hp>0);}
 bool CampPanelOpen(){if(campPanel<0)return false;if(CampPanelCamp()!=null)return true;campPanel=-1;return false;}
 // Opens the panel if a camp or raider you can bargain with stands on the tile. True when the click was used.
 bool OpenCampAt(int x,int y){
  if(!world.barbarians)return false;var camp=world.CampAt(x,y);if(camp!=null&&!camp.seen&&!world.Visible(x,y))camp=null;
  if(camp==null){var raider=world.Visible(x,y)?world.RaiderAt(x,y):null;if(raider==null)return false;camp=world.camps.Find(c=>c.id==raider.camp&&c.hp>0);if(camp==null){status="These raiders have no camp left to bargain for.";return true;}}
  if(!world.CanParley(0,camp))return false;campPanel=camp.id;ResetMapGesture();return true;}
 void DrawCampPanel(){
  GUI.enabled=true;DiplomacyStyles();HudStyles();var camp=CampPanelCamp();if(camp==null){campPanel=-1;return;}
  Fill(FullCanvas,new Color(0,0,0,.72f));var box=new Rect(190,120,900,540);DipPanel(box,.98f);
  var known=world.camps.Where(c=>world.CanParley(0,c)).OrderBy(c=>c.id).ToList();int at=Mathf.Max(0,known.FindIndex(c=>c.id==camp.id));
  GUI.Label(new Rect(box.x,box.y+14,box.width,40),"BARBARIAN CAMP",dipTitle);
  var text=new GUIStyle(dipRow){fontSize=14,wordWrap=true};Color navy=new Color(.1f,.16f,.3f),teal=new Color(.07f,.38f,.4f),red=new Color(.45f,.08f,.07f);
  var pic=new Rect(box.x+28,box.y+74,230,200);DipPanel(new Rect(pic.x-4,pic.y-4,pic.width+8,pic.height+8),1);var art=Art("barbarian-camp");if(art!=null)GUI.DrawTexture(new Rect(pic.x+10,pic.y+10,pic.width-20,pic.height-20),art,ScaleMode.ScaleToFit,true);
  int outNow=world.raiders.Count(r=>r.camp==camp.id&&r.hp>0);
  Paragraph(new Rect(box.x+28,pic.yMax+16,230,0),"Defence "+world.CampDefence(camp)+"   Health "+camp.hp+" of 10\nRaiders abroad: "+outNow+"\nReward for burning it: "+world.CampReward()+" gold\n\nYour gold: "+world.gold,text);
  float x=box.x+288,w=box.width-316,y=box.y+68;
  string bargain=world.BargainText(0,camp);
  y+=Paragraph(new Rect(x,y,w,0),bargain!=""?"This camp is "+bargain+".":"These people serve nobody. Gold will turn their raids away from you, or turn them on someone else.",new GUIStyle(text){fontStyle=FontStyle.Bold})+14;
  // ---- Pay them off ----
  DipHeading(new Rect(x,y,w,22),"Pay them off");y+=28;
  y+=Paragraph(new Rect(x,y,w,0),"For "+BarbarianTerms.TruceTurns+" turns this camp's raiders leave your units, cities and land alone. Each payment makes the next one dearer.",text)+8;
  y+=CampDeal(new Rect(x,y,w,36),"Pay "+world.PayOffPrice(0,camp)+" gold",teal,world.PayOffReason(0,camp),()=>{if(world.PayOff(0,camp))status="The barbarians have been paid off.";})+16;
  // ---- Hire them ----
  DipHeading(new Rect(x,y,w,22),"Hire them as raiders");y+=28;
  y+=Paragraph(new Rect(x,y,w,0),"For "+BarbarianTerms.HireTurns+" turns they march on the civilisation you name, and leave you alone. Like privateers they carry no flag, so nobody learns who sent them. Half of any gold they plunder is yours.",text)+8;
  var targets=world.HireTargets(0);
  if(targets.Count==0)Paragraph(new Rect(x,y,w,0),"You have met no other civilisation of this world.",text);
  else{campTarget=Mathf.Clamp(campTarget,0,targets.Count-1);GUI.Label(new Rect(x,y,70,28),"Against",new GUIStyle(text){wordWrap=false,alignment=TextAnchor.MiddleLeft});campTarget=Cycle(new Rect(x+74,y,Mathf.Min(360,w-74),28),campTarget,targets.Select(t=>world.NationName(t)).ToArray());y+=38;int target=targets[campTarget];
   CampDeal(new Rect(x,y,w,36),"Hire for "+world.HirePrice(camp,target)+" gold",red,world.HireReason(0,camp,target),()=>{if(world.Hire(0,camp,target))status="The barbarians will raid "+world.NationName(target)+".";});}
  // ---- foot ----
  float fy=box.yMax-58;
  if(DipButton(new Rect(box.x+28,fy,230,38),"Close",navy)){campPanel=-1;GUIUtility.ExitGUI();}
  if(known.Count>1){if(DipButton(new Rect(x,fy,130,38),"Previous",navy)){campPanel=known[(at+known.Count-1)%known.Count].id;GUIUtility.ExitGUI();}GUI.Label(new Rect(x+136,fy,150,38),"Camp "+(at+1)+" of "+known.Count,new GUIStyle(text){alignment=TextAnchor.MiddleCenter,wordWrap=false});if(DipButton(new Rect(x+292,fy,130,38),"Next",navy)){campPanel=known[(at+1)%known.Count].id;GUIUtility.ExitGUI();}}
  if(DipButton(new Rect(box.xMax-28-140,fy,140,38),"Show on map",navy)){campPanel=-1;FocusTile(camp.x,camp.y);GUIUtility.ExitGUI();}
 }
 // Text as tall as it needs; returns its height.
 float Paragraph(Rect r,string s,GUIStyle style){var c=new GUIContent(s);float h=style.CalcHeight(c,r.width);GUI.Label(new Rect(r.x,r.y,r.width,h),c,style);return h;}
 // A bargain button with the reason it is greyed written beside it; returns the height used.
 float CampDeal(Rect r,string label,Color ink,string reason,Action act){
  bool was=GUI.enabled;GUI.enabled=reason=="";if(DipButton(new Rect(r.x,r.y,220,r.height),label,ink)){act();GUIUtility.ExitGUI();}GUI.enabled=was;
  if(reason!="")FitLabel(new Rect(r.x+232,r.y,r.width-232,r.height),reason,new GUIStyle(dipRow){fontSize=14,wordWrap=false,alignment=TextAnchor.MiddleLeft,normal={textColor=DipBad}},10);
  return r.height;}
}
}
