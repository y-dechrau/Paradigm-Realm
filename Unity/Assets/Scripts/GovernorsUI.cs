using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The governor panel on the city screen (owner choice, 8 Nov 2026; Governors.cs): who governs, what their traits and policy do,
// how long until they have settled in, and the choices: set the policy, swap with another city's governor, or replace them.
public partial class RealmGame {
 bool governorPanel;Vector2 governorScroll;
 void DrawGovernorPanel(City c,Rect area){
  GUI.Box(area,"");GUI.Box(area,"");float x=area.x+14,w=area.width-28,y=area.y+10;var g=world.GovernorOf(c);
  GUI.Label(new Rect(x,y,w-70,26),"Governor of "+c.name,heading);if(GUI.Button(new Rect(area.xMax-74,y,60,24),"Close",button))governorPanel=false;y+=30;
  if(g==null){GUI.Label(new Rect(x,y,w,40),"This city has no governor.",body);return;}
  float inner=760+world.cities.Count(a=>a.owner==0)*30;
  governorScroll=GUI.BeginScrollView(new Rect(x,y,w,area.yMax-y-8),governorScroll,new Rect(0,0,w-18,inner));float iy=0,iw=w-22;
  GUI.Label(new Rect(0,iy,iw,22),g.name,body);iy+=22;
  GUI.Label(new Rect(0,iy,iw,20),"Stress "+g.stress+"  •  Loyalty "+g.loyalty+"  •  Corruption "+g.corruption+"%",small);iy+=22;
  {var keep=GUI.color;GUI.color=world.GovernorSettled(g)?new Color(.6f,.9f,.6f):new Color(1f,.8f,.45f);GUI.Label(new Rect(0,iy,iw,22),world.GovernorSettled(g)?"Settled in. Effect now: "+world.GovernorEffectText(c):world.GovernorEffectText(c),small);GUI.color=keep;iy+=26;}
  GUI.Label(new Rect(0,iy,iw,22),"Traits",heading);iy+=24;
  foreach(int t in new[]{g.traitA,g.traitB}){GUI.Label(new Rect(0,iy,iw,34),LivingCatalog.Traits[t]+": "+GovernorCatalog.PercentText(GovernorCatalog.TraitPercent(t))+". Also "+LivingCatalog.TraitTexts[t]+".",small);iy+=36;}
  if(g.corruption>=GovernorCatalog.SkimFrom){GUI.Label(new Rect(0,iy,iw,34),"Corrupt: keeps "+g.corruption+"% of every gain for themselves. A Courthouse or an Honest governor helps.",small);iy+=36;}
  iy+=4;GUI.Label(new Rect(0,iy,iw,22),"Policy",heading);iy+=24;
  if(c.autoManage){GUI.Label(new Rect(0,iy,iw,22),"Auto-manage chooses the policy for this city.",small);iy+=22;}
  bool was=GUI.enabled;
  for(int k=0;k<GovernorCatalog.Policies.Length;k++){
   string why=g.policy==k?"":world.PolicyReason(c,k);GUI.enabled=was&&!c.autoManage&&(g.policy==k||why=="");
   if(GUI.Button(new Rect(0,iy,130,26),(g.policy==k?"● ":"")+GovernorCatalog.Policies[k],button)&&g.policy!=k){if(world.SetPolicy(c,k))status=g.name+" now runs "+c.name+" on "+GovernorCatalog.Policies[k]+".";}
   GUI.enabled=was;GUI.Label(new Rect(136,iy,iw-136,30),GovernorCatalog.PolicyTexts[k],small);iy+=32;}
  {string refuse=world.PolicyReason(c,g.policy==0?1:0);if(refuse.Contains("loyal")){GUI.Label(new Rect(0,iy,iw,34),refuse,small);iy+=34;}}
  iy+=4;GUI.Label(new Rect(0,iy,iw,22),"Swap with another governor",heading);iy+=24;
  GUI.Label(new Rect(0,iy,iw,34),"Both governors move and must settle in again ("+GovernorCatalog.SettleTurns+" turns, "+GovernorCatalog.LoyalSettleTurns+" if Loyal).",small);iy+=36;
  foreach(var o in world.cities.Where(a=>a.owner==0&&a!=c).OrderBy(a=>a.name)){var og=world.GovernorOf(o);if(og==null)continue;
   if(GUI.Button(new Rect(0,iy,70,26),"Swap",button)){world.SwapGovernors(c,o);status=og.name+" now governs "+c.name+".";}
   GUI.Label(new Rect(76,iy,iw-76,28),o.name+": "+og.name+" ("+world.TraitLine(og)+")",small);iy+=30;}
  iy+=6;{string why=world.ReplaceReason(c);GUI.enabled=was&&why=="";if(GUI.Button(new Rect(0,iy,260,28),"Replace ("+GovernorCatalog.ReplaceCost+" gold)",button)){world.ReplaceGovernor(c);}GUI.enabled=was;
   GUI.Label(new Rect(0,iy+30,iw,34),why!=""?why:"A new governor with new traits. Your other governors lose 2 loyalty.",small);}
  GUI.EndScrollView();
 }
}
}
