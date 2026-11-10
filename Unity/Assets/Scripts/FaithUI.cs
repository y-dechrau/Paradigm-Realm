using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The Religion screen in six sections (owner design, 8 Nov 2026): Overview, Beliefs, Institutions, Expansion, Diplomacy and History,
// plus the decision panel for religious events. Older games without modern rules keep the earlier single page (DrawReligionClassic).
public partial class RealmGame {
 int faithTab,pickFaith=-1,pickCore=-1,pickSocial=-1,pickEconomic=-1,pickAfterlife=0,reformSocial=-1,reformEconomic=-1;
 static readonly string[] FaithTabs={"Overview","Faith Tree","Beliefs","More"};int faithMoreSection;
 void DrawReligion(){
  if(!world.modernRules){DrawReligionClassic();return;}
  var me=world.nations[0];
  GUILayout.Label("Faith "+me.faith+"  (+"+world.FaithPerTurn(0)+" a turn)   •   Influence "+me.influence,heading);
  faithTab=GUILayout.SelectionGrid(faithTab,FaithTabs,6);GUILayout.Space(6);
  switch(faithTab){case 0:FaithOverview();break;case 1:FaithBeliefs();break;case 2:FaithInstitutions();break;case 3:FaithExpansion();break;case 4:FaithDiplomacy();break;default:FaithHistory();break;}
 }
 string Bar(int value,int max=100){int n=Mathf.Clamp(value*20/Math.Max(1,max),0,20);return new string('█',n)+new string('·',20-n)+" "+value;}
 void FaithOverview(){
  var me=world.nations[0];int r=me.founded,state=world.StateReligion(0);
  if(world.NextFaithEvent()!=null){GUILayout.Label("A decision awaits: "+FaithCatalog.EventNames[world.NextFaithEvent().kind]+".",heading);}
  GUILayout.Label("Pantheon",heading);
  if(me.pantheon>=0)GUILayout.Label(ReligionCatalog.PantheonNames[me.pantheon]+" — "+ReligionCatalog.PantheonText[me.pantheon],body);
  else{string why=world.PantheonReason(0);GUILayout.Label(why!=""?why:"Choose a god for your people:",small);bool was=GUI.enabled;GUI.enabled=was&&why=="";for(int p=0;p<ReligionCatalog.PantheonNames.Length;p++){GUILayout.BeginHorizontal();if(GUILayout.Button(ReligionCatalog.PantheonNames[p],button,GUILayout.Width(220)))world.ChoosePantheon(0,p);GUILayout.Label(ReligionCatalog.PantheonText[p],small);GUILayout.EndHorizontal();}GUI.enabled=was;}
  GUILayout.Space(6);GUILayout.Label("Your faith",heading);
  if(state<0)GUILayout.Label("Your civilisation has no faith yet. Found one under Beliefs once you have a pantheon, Mysticism and a city with a Temple, or let one spread to your capital.",small);
  else{var f=world.Record(state);long people=world.WorldFollowers(state),all=Math.Max(1,world.cities.Where(c=>world.At(c.x,c.y).realm==0).Sum(c=>world.CityPeople(c)));
   GUILayout.Label(world.FaithName(state)+(r==state?" (founded by you)":" (your state faith)")+" — "+World.PeopleText(people)+" followers, "+(people*100/all)+"% of the world's people, majority in "+world.Followers(state)+" cities",body);
   if(f!=null){int level=world.InstitutionLevel(state);
    GUILayout.Label("Origin: "+FaithCatalog.Origins[f.origin]+".  Beliefs: "+FaithCatalog.Cores[f.core]+", "+FaithCatalog.Socials[f.social]+", "+FaithCatalog.Economics[f.economic]+", "+FaithCatalog.Afterlives[f.afterlife]+".",small);
    GUILayout.Label("Religious authority   "+Bar(f.authority)+"   (credibility and political weight of its institutions, apart from faith)",small);
    GUILayout.Label("Reform movement       "+Bar(f.reform)+"   (at 100 the founder must face the Great Schism)",small);
    GUILayout.Label("Institution: "+FaithCatalog.Institutions[level]+" (level "+level+" of 8).  Church: "+FaithCatalog.ChurchStates[me.churchState]+".  Policy: "+FaithCatalog.Policies[me.religionPolicy]+".",small);}}
  int unrest=world.cities.Where(c=>c.owner==0).Sum(c=>world.ReligiousUnrest(c));if(unrest>0)GUILayout.Label("Religious unrest costs "+unrest+" content citizens across your cities.",small);
  GUILayout.Space(6);GUILayout.Label("Faiths of the world",heading);
  var faiths=Enumerable.Range(0,ReligionCatalog.Count).Where(x=>world.Followers(x)>0||world.Record(x)!=null).OrderByDescending(x=>world.WorldFollowers(x)).ToList();
  if(faiths.Count==0)GUILayout.Label("No faith has been founded yet.",small);
  foreach(int x in faiths){var f=world.Record(x);GUILayout.Label(world.FaithName(x)+" — "+World.PeopleText(world.WorldFollowers(x))+" followers, majority in "+world.Followers(x)+" cities"+(f!=null?" • "+FaithCatalog.Cores[f.core]+" • authority "+f.authority+" • "+FaithCatalog.Institutions[world.InstitutionLevel(x)]+(f.parent>=0?" • branch of "+world.FaithName(f.parent):""):"")+" • "+(world.ReligionFounder(x)!=""?"founded by "+world.ReligionFounder(x):""),small);}
 }
 void FaithBeliefs(){
  var me=world.nations[0];var f=world.Record(me.founded);
  if(f!=null){
   GUILayout.Label(world.FaithName(f.religion)+": what it teaches",heading);
   GUILayout.Label("Origin — "+FaithCatalog.Origins[f.origin]+": "+FaithCatalog.OriginTexts[f.origin],body);
   GUILayout.Label("Core belief — "+FaithCatalog.Cores[f.core]+": "+FaithCatalog.CoreTexts[f.core],body);
   GUILayout.Label("Social doctrine — "+FaithCatalog.Socials[f.social]+": "+FaithCatalog.SocialTexts[f.social],body);
   GUILayout.Label("Economic doctrine — "+FaithCatalog.Economics[f.economic]+": "+FaithCatalog.EconomicTexts[f.economic],body);
   GUILayout.Label("Afterlife — "+FaithCatalog.Afterlives[f.afterlife]+": "+FaithCatalog.AfterlifeTexts[f.afterlife]+" (identity, no direct effect)",body);
   GUILayout.Label("Tenet — "+ReligionCatalog.TenetName(Math.Min(f.religion,ReligionCatalog.FoundableCount-1))+": "+(f.parent>=0?"inherited from "+world.FaithName(f.parent):ReligionCatalog.TenetText(f.religion)),small);
   GUILayout.Space(8);GUILayout.Label("Reformation ("+world.ReformCost(0)+" faith; authority -10, reform movement -40)",heading);
   GUILayout.Label("Change the social doctrine, the economic doctrine, or both. Needs Philosophy.",small);
   if(reformSocial<0)reformSocial=f.social;if(reformEconomic<0)reformEconomic=f.economic;
   reformSocial=GUILayout.SelectionGrid(reformSocial,FaithCatalog.Socials,5);GUILayout.Label(FaithCatalog.SocialTexts[reformSocial],small);
   reformEconomic=GUILayout.SelectionGrid(reformEconomic,FaithCatalog.Economics,6);GUILayout.Label(FaithCatalog.EconomicTexts[reformEconomic],small);
   string why=world.ReformReason(0);if(reformSocial==f.social&&reformEconomic==f.economic)why="Choose a different doctrine.";GUILayout.Label(why,small);
   bool was=GUI.enabled;GUI.enabled=was&&why=="";if(GUILayout.Button("Reform the faith",button,GUILayout.Width(260))){world.Reform(0,reformSocial,reformEconomic);status=world.FaithName(f.religion)+" has been reformed.";}GUI.enabled=was;
   return;}
  GUILayout.Label("Found a faith",heading);
  string general=world.FoundingReason(0,-1);GUILayout.Label(general!=""?general:"Choose a faith, then its beliefs. Founding costs "+world.FoundingCost()+" faith; your largest city with a Temple becomes its holy city.",small);
  var holy=world.HolyCityCandidate(0);int origin=world.OriginFor(0,holy);
  GUILayout.Label("Origin (from what your people have lived through): "+FaithCatalog.Origins[origin]+" — "+FaithCatalog.OriginTexts[origin],body);
  if(pickCore<0)pickCore=World.SuggestedCore(origin);if(pickSocial<0)pickSocial=World.SuggestedSocial(origin);if(pickEconomic<0)pickEconomic=0;
  GUILayout.Label("Core belief",heading);pickCore=GUILayout.SelectionGrid(pickCore,FaithCatalog.Cores,5);GUILayout.Label(FaithCatalog.CoreTexts[pickCore],small);
  GUILayout.Label("Social doctrine",heading);pickSocial=GUILayout.SelectionGrid(pickSocial,FaithCatalog.Socials,5);GUILayout.Label(FaithCatalog.SocialTexts[pickSocial],small);
  GUILayout.Label("Economic doctrine",heading);pickEconomic=GUILayout.SelectionGrid(pickEconomic,FaithCatalog.Economics,6);GUILayout.Label(FaithCatalog.EconomicTexts[pickEconomic],small);
  GUILayout.Label("Afterlife",heading);pickAfterlife=GUILayout.SelectionGrid(pickAfterlife,FaithCatalog.Afterlives,7);GUILayout.Label(FaithCatalog.AfterlifeTexts[pickAfterlife],small);
  // Only the faiths your people could know of by now are offered, in the order history found them (owner request, 8 Nov 2026).
  GUILayout.Space(6);GUILayout.Label("The faith",heading);
  var known=Enumerable.Range(0,ReligionCatalog.FoundableCount).Where(r=>world.HasTech(0,ReligionCatalog.Tech(r))||world.ReligionFounder(r)!="").OrderBy(r=>TechnologyCatalog.Index(ReligionCatalog.Tech(r))).ToList();
  var later=Enumerable.Range(0,ReligionCatalog.FoundableCount).Where(r=>!known.Contains(r)).OrderBy(r=>TechnologyCatalog.Index(ReligionCatalog.Tech(r))).ToList();
  if(later.Count>0)GUILayout.Label(later.Count+" more faiths will arise as your civilisation learns. Next: "+ReligionCatalog.Name(later[0])+", with "+World.TechNames[TechnologyCatalog.Index(ReligionCatalog.Tech(later[0]))]+".",small);
  foreach(int r in known){string founder=world.ReligionFounder(r);string reason=world.FoundingReason(0,r);
   GUILayout.BeginHorizontal();GUILayout.Label(ReligionCatalog.Name(r)+"  ("+ReligionCatalog.Tradition(r)+")",body,GUILayout.Width(330));
   GUILayout.Label(founder!=""?"Founded by "+founder:"Needs "+World.TechNames[TechnologyCatalog.Index(ReligionCatalog.Tech(r))],small,GUILayout.Width(330));
   if(founder==""){bool was=GUI.enabled;GUI.enabled=was&&reason=="";if(GUILayout.Button("Found with these beliefs",button,GUILayout.Width(220))){if(world.FoundReligion(0,r,pickCore,pickSocial,pickEconomic,pickAfterlife))status=ReligionCatalog.Name(r)+" founded.";}GUI.enabled=was;}
   GUILayout.EndHorizontal();GUILayout.Label(ReligionCatalog.History(r)+" Tenet — "+ReligionCatalog.TenetName(r)+": "+ReligionCatalog.TenetText(r),small);GUILayout.Space(3);}
 }
 void FaithInstitutions(){
  var me=world.nations[0];int state=world.StateReligion(0);int level=state>=0?world.InstitutionLevel(state):0;
  GUILayout.Label("Religious institutions",heading);
  for(int l=1;l<FaithCatalog.Institutions.Length;l++)GUILayout.Label((l<=level?"■ ":"□ ")+l+". "+FaithCatalog.Institutions[l]+"  — "+InstitutionNeed(l),l==level?body:small);
  GUILayout.Label("Each level makes the faith spread 5% further. A council (level 5) can meet; the highest level is needed for a religious victory.",small);
  GUILayout.Space(6);GUILayout.Label("Church and state — does the ruler control the faith, or the faith the ruler?",heading);
  for(int s=0;s<FaithCatalog.ChurchStates.Length;s++){GUILayout.BeginHorizontal();string why=world.ChurchReason(0,s);bool was=GUI.enabled;GUI.enabled=was&&why=="";if(GUILayout.Button((me.churchState==s?"▸ ":"")+FaithCatalog.ChurchStates[s],button,GUILayout.Width(240)))world.SetChurchState(0,s);GUI.enabled=was;GUILayout.Label(FaithCatalog.ChurchTexts[s]+(why!=""&&me.churchState!=s?"  ("+why+")":""),small);GUILayout.EndHorizontal();}
  GUILayout.Label("Your government: "+GovernmentFaith(),small);
  GUILayout.Space(6);GUILayout.Label("Religious policy",heading);
  for(int p=0;p<FaithCatalog.Policies.Length;p++){GUILayout.BeginHorizontal();if(GUILayout.Button((me.religionPolicy==p?"▸ ":"")+FaithCatalog.Policies[p],button,GUILayout.Width(240)))world.SetPolicy(0,p);GUILayout.Label(FaithCatalog.PolicyTexts[p],small);GUILayout.EndHorizontal();}
  GUILayout.Space(6);GUILayout.Label("Religious buildings in your cities",heading);
  foreach(int b in World.FaithBuildingIds)GUILayout.Label(World.FaithBuildingName(b)+": "+world.cities.Count(c=>c.owner==0&&world.HasBuilding(c,b))+"  ("+World.FaithBuildingTexts[Array.IndexOf(World.FaithBuildingIds,b)]+")",small);
  GUILayout.Label("They are bought with your civilisation's faith from the city screen's Buildings tab.",small);
  {var temples=world.cities.Where(c=>c.owner==0&&c.religion>=0&&world.HasBuilding(c,36)).OrderBy(c=>c.name).ToList();GUILayout.Space(6);GUILayout.Label("Priests ("+World.PriestFaithCost+" faith)",heading);
   if(temples.Count==0)GUILayout.Label("None of your cities both follows a faith and has a Temple yet.",small);
   foreach(var c in temples.Take(10)){GUILayout.BeginHorizontal();GUILayout.Label(c.name+" — "+world.FaithName(c.religion),body,GUILayout.Width(560));string why=world.BuyPriestReason(0,c);bool was=GUI.enabled;GUI.enabled=was&&why=="";if(GUILayout.Button("Buy a Priest",button,GUILayout.Width(200)))world.BuyPriest(0,c);GUI.enabled=was;GUILayout.EndHorizontal();}}
  // The faith tree has its own tab now.
 }
 static string InstitutionNeed(int l){switch(l){case 1:return "the faith is founded";case 2:return "a following city has a Shrine";case 3:return "a following city has a Temple";case 4:return "three following cities have Temples";case 5:return "six following cities and authority 40";case 6:return "followed in two civilisations";case 7:return "twelve following cities and authority 60";default:return "the capitals of half the civilisations and authority 75";}}
 string GovernmentFaith(){int g=world.nations[0].government;string[] text={"tribal ways: ancestor rites and spirits","the divine right of kings","the church stands apart from the state","religious freedom and pluralism","the state regulates organised religion"};return World.Governments[Math.Max(0,Math.Min(text.Length-1,g))]+": "+text[Math.Max(0,Math.Min(text.Length-1,g))]+" (any church and policy may be combined with it).";}
 void FaithExpansion(){
  var me=world.nations[0];int state=world.StateReligion(0);var f=world.Record(me.founded);
  GUILayout.Label("Holy city and sacred sites",heading);
  if(f==null)GUILayout.Label("Found a faith to have a holy city and sacred sites.",small);
  else{var holy=world.cities.Find(c=>c.id==f.holyCity);GUILayout.Label("Holy city: "+(holy!=null?holy.name+(holy.owner!=0?" (held by "+world.NationName(holy.owner)+")":""):"lost"),body);
   if(f.sites.Count==0)GUILayout.Label("No sacred sites yet; discoveries may bring them.",small);
   foreach(int s in f.sites){var t=world.tiles[s];GUILayout.Label(world.SiteName(s)+" — "+(t.owner==0?"yours":t.owner<0?"unclaimed":"held by "+world.NationName(t.owner)),small);}}
  GUILayout.Space(6);GUILayout.Label("Who believes what in your cities",heading);
  foreach(var c in world.cities.Where(c=>c.owner==0).OrderByDescending(c=>c.pop).Take(14)){var d=world.Demographics(c);GUILayout.Label(c.name+" ("+World.PeopleText(world.CityPeople(c))+"): "+string.Join(", ",d.Where(a=>a.Value>0).Take(4).Select(a=>(a.Key<0?"unaffiliated":world.FaithName(a.Key))+" "+a.Value+"%").ToArray())+(world.ReligiousUnrest(c)>0?"  — unrest -"+world.ReligiousUnrest(c):""),small);}
  if(me.founded>=0){GUILayout.Space(6);GUILayout.Label("Missionaries ("+world.MissionPrice(0)+" faith each, +150 pressure, within "+world.MissionRange(0)+" tiles of your faithful)",heading);
   var targets=world.cities.Where(c=>c.religion!=me.founded&&world.At(c.x,c.y).seen&&!world.MissionReason(0,c).StartsWith("Too far")).OrderBy(c=>c.owner==0?0:1).ThenBy(c=>c.name).Take(16).ToList();
   if(targets.Count==0)GUILayout.Label("No known city within reach needs converting.",small);
   foreach(var c in targets){GUILayout.BeginHorizontal();GUILayout.Label(c.name+" ("+world.NationName(c.owner)+") — "+(c.religion>=0?world.FaithName(c.religion):"no majority")+", yours "+world.ShareOf(c,me.founded)+"%",body,GUILayout.Width(560));string why=world.MissionReason(0,c);bool was=GUI.enabled;GUI.enabled=was&&why=="";if(GUILayout.Button("Send missionaries",button,GUILayout.Width(200)))world.SendMissionaries(0,c);GUI.enabled=was;if(why!="")GUILayout.Label(why,small);GUILayout.EndHorizontal();}}
  GUILayout.Label("Faith also spreads by itself: holy cities and Temples press on cities within "+(state>=0?world.PressureRange(state):10)+" tiles; trade agreements carry it a quarter further; an old majority resists newcomers; Industry leaves more people unaffiliated. Conquest brings the conqueror's faith.",small);
 }
 void FaithDiplomacy(){
  var me=world.nations[0];int state=world.StateReligion(0);
  GUILayout.Label("Religious diplomacy",heading);
  if(world.HolyWarJustified(0,me.holyWarOn))GUILayout.Label("You have just cause for holy war against "+world.NationName(me.holyWarOn)+" for "+(me.holyWarUntil-world.turn)+" more turns: declaring it will not alarm the world.",body);
  {string why=world.CouncilReason(0);GUILayout.BeginHorizontal();bool was=GUI.enabled;GUI.enabled=was&&why=="";if(GUILayout.Button("Call a religious council (60 faith)",button,GUILayout.Width(320))){world.CallCouncil(0);status="The council has met.";}GUI.enabled=was;GUILayout.Label(why!=""?why:"Authority +15, reform movement -40.",small);GUILayout.EndHorizontal();}
  foreach(int o in Enumerable.Range(1,world.civCount-1).Where(o=>world.met[o]&&world.cities.Any(c=>c.owner==o))){
   int theirs=world.StateReligion(o);GUILayout.Space(4);
   GUILayout.Label(world.NationName(o)+" — "+(theirs>=0?world.FaithName(theirs):"no faith")+" • "+FaithCatalog.Policies[world.Policy(o)]+" • "+FaithCatalog.ChurchStates[world.nations[o].churchState]+(world.FaithTreaty(0,o)?" • religious treaty":""),body);
   var reasons=world.OpinionLines(o,0).Where(l=>l.Key.Contains("faith")||l.Key.Contains("god")||l.Key.Contains("Condemned")||l.Key.Contains("Tolerant")).ToList();if(reasons.Count>0)GUILayout.Label("Their view: "+string.Join("; ",reasons.Select(l=>l.Key+" "+(l.Value>0?"+":"")+l.Value).ToArray()),small);
   GUILayout.BeginHorizontal();
   FaithAction("Fund clergy (30 faith)",world.FundReason(0,o),()=>world.FundClergy(0,o));
   FaithAction("Demand protection",world.ProtectionReason(0,o),()=>{status=world.DemandProtection(0,o)?world.NationName(o)+" will protect your faithful.":world.NationName(o)+" refused: you have just cause for holy war.";});
   FaithAction("Religious treaty",world.TreatyReason(0,o),()=>world.SignFaithTreaty(0,o));
   GUILayout.EndHorizontal();}
  GUILayout.Space(6);GUILayout.Label("Condemn a heresy (20 faith): authority +5, reform -5; civilisations of that faith resent it for 30 turns",heading);
  GUILayout.BeginHorizontal();int shown=0;foreach(int x in Enumerable.Range(0,ReligionCatalog.Count).Where(x=>x!=state&&world.Followers(x)>0)){if(shown++>=5)break;FaithAction("Condemn "+world.FaithName(x),world.CondemnReason(0,x),()=>world.Condemn(0,x));}GUILayout.EndHorizontal();
  GUILayout.Label("Sharing a faith counts +10 in every leader's opinion; persecuting their followers -15; protecting them +5. Agreements between civilisations of one faith are easier to reach.",small);
 }
 void FaithAction(string label,string why,Action act){bool was=GUI.enabled;GUI.enabled=was&&why=="";if(GUILayout.Button(new GUIContent(label,why),button,GUILayout.Width(240)))act();GUI.enabled=was;}
 // The More tab: the detail that used to fill four tabs, one section open at a time (owner request, 8 Nov 2026).
 static readonly string[] FaithMoreNames={"Institutions and policy","Missions and your cities","Faith and other leaders","History"};
 void FaithMore(){
  GUILayout.Label("Most players never need these; open one when you want the detail.",small);
  for(int k=0;k<FaithMoreNames.Length;k++){
   if(GUILayout.Button((faithMoreSection==k?"▾ ":"▸ ")+FaithMoreNames[k],button,GUILayout.Height(30)))faithMoreSection=faithMoreSection==k?-1:k;
   if(faithMoreSection!=k)continue;GUILayout.Space(4);
   switch(k){case 0:FaithInstitutions();break;case 1:FaithExpansion();break;case 2:FaithDiplomacy();break;default:FaithHistory();break;}
   GUILayout.Space(8);
  }
 }
 void FaithHistory(){
  GUILayout.Label("Religious history",heading);
  foreach(var e in world.faithEvents)GUILayout.Label("Awaiting your decision: "+FaithCatalog.EventNames[e.kind],body);
  if(world.faithHistory==null||world.faithHistory.Count==0)GUILayout.Label("Nothing recorded yet.",small);
  else foreach(var line in world.faithHistory)GUILayout.Label(line,small);
 }
 // The city screen's faith line: the two largest faiths and the unaffiliated.
 string FaithShareText(City c){var d=world.Demographics(c);var faiths=d.Where(a=>a.Key>=0&&a.Value>0).Take(2).Select(a=>world.FaithName(a.Key)+" "+a.Value+"%").ToList();int none=d.Last().Value;return (faiths.Count==0?"No faith yet":string.Join(", ",faiths.ToArray()))+(none>0&&faiths.Count>0?", none "+none+"%":"")+(world.IsHolyCity(c)?" (holy city)":"");}
 // ---------- the decision panel ----------
 bool FaithPromptOpen(){return world!=null&&world.modernRules&&!turnRunning&&world.result==""&&!guide&&!confirmNew&&pendingUnit==-1&&world.NextFaithEvent()!=null;}
 void DrawFaithPrompt(){
  var e=world.NextFaithEvent();if(e==null)return;GUI.enabled=true;var choices=FaithCatalog.EventChoices[e.kind];
  var picture=Art("faith-event-"+e.kind);float ph=picture!=null?212:0;
  var box=new Rect(330,Mathf.Max(44,400-(150+ph+choices.Length*48)/2),620,150+ph+choices.Length*48);Fill(new Rect(0,0,1280,800),new Color(0,0,0,.35f));FaithFrame(box,.97f);
  if(picture!=null){var pr=new Rect(box.x+12,box.y+12,box.width-24,ph-12);GUI.DrawTexture(pr,picture,ScaleMode.ScaleAndCrop);Fill(new Rect(pr.x,pr.yMax-1,pr.width,1),FaithGold);}
  GUI.Label(new Rect(box.x+20,box.y+ph+12,box.width-40,30),FaithCatalog.EventNames[e.kind].ToUpperInvariant(),hudTitle);
  GUI.Label(new Rect(box.x+20,box.y+ph+48,box.width-40,84),world.EventText(e),new GUIStyle(body){wordWrap=true});
  float y=box.y+ph+136;
  for(int k=0;k<choices.Length;k++){string why=world.ChoiceReason(e,k);bool was=GUI.enabled;GUI.enabled=why=="";if(GUI.Button(new Rect(box.x+20,y,box.width-40,40),choices[k]+(why!=""?"  — "+why:""),new GUIStyle(button){wordWrap=true})){world.ResolveEvent(e,k);status=FaithCatalog.EventNames[e.kind]+": decided.";GUIUtility.ExitGUI();}GUI.enabled=was;y+=48;}
 }
}
}
