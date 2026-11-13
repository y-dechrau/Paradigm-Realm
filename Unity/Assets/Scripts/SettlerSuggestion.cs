using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm { public partial class RealmGame {
 // Owner request (13 Nov 2026): selecting a Settler suggests the best place nearby for a city. The best site carries a pulsing
 // green outline and a plate saying why; the next two are outlined faintly. The unit panel offers to send the Settler there.
 // The search is the settlement adviser's (SettlementOptions) and runs again only when the Settler, the turn or the units change.
 List<SettlementReport> suggestedSites=new List<SettlementReport>();long suggestionKey=long.MinValue;
 List<SettlementReport> SuggestedSites(Unit u){
  if(u==null||u.owner!=0||u.type!=0||world.result!=""){suggestedSites.Clear();suggestionKey=long.MinValue;return suggestedSites;}
  long key;unchecked{key=world.turn;key=key*31+u.id;key=key*31+u.x;key=key*31+u.y;key=key*31+world.units.Count;key=key*31+world.cities.Count;}
  if(key!=suggestionKey){suggestionKey=key;suggestedSites=world.SettlementOptions(u,3);}
  return suggestedSites;
 }
 void DrawSiteSuggestion(Unit u,float s){
  if(u==null||u.type!=0||u.owner!=0||roadMode)return;
  var sites=SuggestedSites(u);if(sites.Count==0)return;
  float pulse=.55f+.45f*Mathf.Sin(Time.realtimeSinceStartup*4f);var old=GUI.color;
  for(int i=sites.Count-1;i>=0;i--){
   var r=sites[i];Rect cell;float cx,top;
   if(diamondView){float sx,sy;IsoScreen(r.x,r.y,out sx,out sy);sy-=Lift(r.x,r.y,s);cell=TileRect(sx,sy,s);cx=sx;top=cell.y;}
   else {cell=ClassicRect(r.x,r.y);cx=cell.center.x;top=cell.y;}
   if(i==0){
    GUI.color=new Color(.25f,1f,.45f,.5f+.5f*pulse);GUI.DrawTexture(cell,SelectionPicture());GUI.DrawTexture(new Rect(cell.x+1,cell.y+1,cell.width-2,cell.height-2),SelectionPicture());GUI.color=old;
    string head=r.x==u.x&&r.y==u.y?"Best site nearby: found the city here [B]":"Suggested city site";
    string line=world.SiteHighlights(r);var style=new GUIStyle(hudSmall){wordWrap=false,clipping=TextClipping.Overflow,alignment=TextAnchor.MiddleCenter};
    float w=Mathf.Max(style.CalcSize(new GUIContent(head)).x,style.CalcSize(new GUIContent(line)).x)+20;
    var plate=new Rect(cx-w/2,top-46,w,40);DrawGuideFrame(plate);
    GUI.color=new Color(.55f,1f,.6f);GUI.Label(new Rect(plate.x,plate.y+3,plate.width,18),head,style);GUI.color=old;
    GUI.Label(new Rect(plate.x,plate.y+20,plate.width,18),line,style);
   } else {GUI.color=new Color(.6f,1f,.7f,.35f);GUI.DrawTexture(cell,SelectionPicture());GUI.color=old;}
  }
 }
 // The unit panel line, above "Compare settlement sites".
 void DrawSettlerPrompt(Unit u){
  var sites=SuggestedSites(u);
  if(sites.Count==0){GUILayout.Label("No good site found within 12 tiles yet. Explore farther, or found a city by hand.",small);return;}
  var best=sites[0];
  if(best.x==u.x&&best.y==u.y){GUILayout.Label("Suggested: this is the best site nearby ("+world.SiteHighlights(best)+").",small);return;}
  GUILayout.Label("Suggested site "+best.distance+" tiles away: "+world.SiteHighlights(best)+".",small);
  bool enabled=GUI.enabled;GUI.enabled=enabled&&u.moves>0;
  if(Button("Go to the suggested site")){IssueOrder(u,best.x,best.y);status="Settler on its way to the suggested site. Found the city there with [B].";}
  GUI.enabled=enabled;
 }
}}
