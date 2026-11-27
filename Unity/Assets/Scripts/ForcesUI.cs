using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The screen itself is drawn in MilitaryScreenUI.cs after the owner's picture of 27 Nov 2026; this file keeps its shared pieces.
// Military > Forces (owner brief, 20 Nov 2026; moved from the Empire screen to its own Military screen on 21 Nov). Answers four questions at a glance:
//  1. what forces you have and what they cost each turn (the meters and the Formations column);
//  2. what you can raise now and exactly what stops the rest (the Raise column; every blocked card says why);
//  3. what military industry is working on and when it finishes (the Programme column, top priority first);
//  4. what mobilisation or a refit would change (the policy preview and each formation's refit line).
public partial class RealmGame {
 int militaryTab;// Military screen: 0 Forces, 1 Units (the old units list)
 int forcesPick=-1;Vector2 forcesUnitScroll,forcesRaiseScroll,forcesOrderScroll;string forcesConfirm;Action forcesConfirmDo;string forcesTip;
 static readonly Color ForceCell=new Color(.06f,.09f,.17f,.9f),ForceBad=new Color(1f,.6f,.45f),ForceGood=new Color(.55f,.88f,.6f);
 GUIStyle fSmall,fTiny,fBig,fHead;
 void ForcesStyles(){if(fSmall!=null)return;fSmall=new GUIStyle(small){wordWrap=true};fTiny=new GUIStyle(small){fontSize=11,wordWrap=true};fBig=new GUIStyle(heading){fontSize=20,wordWrap=false,clipping=TextClipping.Overflow};fHead=new GUIStyle(heading){wordWrap=false};}
 static string FormationRole(int t){return World.IsNaval(t)?"Warship":ForcesRules.Mounted(t)?"Mounted":t==9?"Siege":World.IsRanged(t)?"Ranged":"Infantry";}
 void ForcesFrame(Rect r){Fill(r,ForceCell);Fill(new Rect(r.x,r.y,r.width,1),new Color(UiGold.r,UiGold.g,UiGold.b,.7f));}
 string OrderTitle(ForceOrder o){if(o.kind==0)return "Raise "+World.UnitNames[o.type];var u=world.units.Find(x=>x.id==o.unit);string who=u!=null?world.UnitName(u):"formation";return o.kind==1?"Refit "+who+" to "+World.UnitNames[o.type]:"Replace losses: "+who;}
 void Ask(string text,Action yes){forcesConfirm=text;forcesConfirmDo=yes;}
 void AskMobilise(bool on){
  var n=world.nations[0];int capNow=world.ServiceCapacity(0,n.mobilised),capThen=world.ServiceCapacity(0,on);
  int shieldsNow=world.cities.Where(c=>c.owner==0).Sum(c=>world.Yield(c)[1]);n.mobilised=on;int shieldsThen=world.cities.Where(c=>c.owner==0).Sum(c=>world.Yield(c)[1]);n.mobilised=!on;
  int over=Math.Max(0,world.ServiceServing(0)+world.ServiceReserved(0)-capThen);
  Ask((on?"Mobilise?":"Demobilise?")+"\nService capacity "+capNow+" → "+capThen+".\nShields in all your cities "+shieldsNow+" → "+shieldsThen+" a turn.\n"+(on?"The policy must stand at least "+ForcesRules.PolicyMinTurns+" turns.":"You cannot mobilise again for "+ForcesRules.PolicyMinTurns+" turns."+(over>0?"\nYou would be "+over+" service over capacity: every formation stays, but that costs "+over*ForcesRules.ShortfallGold+" gold a turn and blocks orders that add personnel until you are back within it.":"")),()=>{var why=world.SetMobilised(0,on);status=why==""?(on?"Mobilised.":"Demobilised."):why;});
 }
}
}
