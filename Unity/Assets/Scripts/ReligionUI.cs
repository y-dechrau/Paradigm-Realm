using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The Religion screen (owner request, 31 Oct 2026): faith, the pantheon, founding a faith from the real living and ancient
// religions, the faiths of the world, and missionaries.
public partial class RealmGame {
 bool religionAncient;
 void DrawReligion(){
  var me=world.nations[0];
  GUILayout.Label("Faith "+me.faith+"  (+"+world.FaithPerTurn(0)+" a turn)",heading);
  GUILayout.Label("Faith comes from your capital (+1), Temples (+2), a holy city (+3) and some beliefs. A pantheon costs "+World.PantheonCost+" faith; founding a faith costs "+world.FoundingCost()+" and needs Mysticism and a city with a Temple, which becomes the holy city. Cities press their majority faith on every city within ten tiles; the stronger pressure wins the city. This world can hold "+world.MaxReligions()+" faiths.",small);
  GUILayout.Space(6);GUILayout.Label("Pantheon",heading);
  if(me.pantheon>=0)GUILayout.Label(ReligionCatalog.PantheonNames[me.pantheon]+" — "+ReligionCatalog.PantheonText[me.pantheon],body);
  else{string why=world.PantheonReason(0);if(why!="")GUILayout.Label(why,small);bool was=GUI.enabled;GUI.enabled=was&&why=="";
   for(int p=0;p<ReligionCatalog.PantheonNames.Length;p++){GUILayout.BeginHorizontal();if(GUILayout.Button(ReligionCatalog.PantheonNames[p],button,GUILayout.Width(220)))world.ChoosePantheon(0,p);GUILayout.Label(ReligionCatalog.PantheonText[p],small);GUILayout.EndHorizontal();}GUI.enabled=was;}
  GUILayout.Space(6);GUILayout.Label("Your faith",heading);
  if(me.founded>=0){int r=me.founded;var holy=world.cities.Find(c=>c.id==me.holyCity);
   GUILayout.Label(ReligionCatalog.Name(r)+" — holy city "+(holy!=null?holy.name+(holy.owner!=0?" (held by "+world.NationName(holy.owner)+")":""):"lost")+" — followed in "+world.Followers(r)+(world.Followers(r)==1?" city":" cities")+" — tithe +"+world.ReligionTithe(0)+" gold",body);
   GUILayout.Label(ReligionCatalog.TenetName(r)+": "+ReligionCatalog.TenetText(r)+" Your own cities that follow it also gain +1 content citizen.",small);}
  else GUILayout.Label(world.FoundingReason(0,-1) is var general&&general!=""?general:"Choose a faith below to found it.",small);
  // The catalogue of faiths, living and ancient, with what history records of each.
  GUILayout.Space(6);GUILayout.BeginHorizontal();GUILayout.Label("Faiths of the world",heading,GUILayout.Width(300));
  if(GUILayout.Button((religionAncient?"":"▸ ")+"Living faiths",button,GUILayout.Width(170)))religionAncient=false;if(GUILayout.Button((religionAncient?"▸ ":"")+"Faiths of the ancient world",button,GUILayout.Width(240)))religionAncient=true;GUILayout.EndHorizontal();
  for(int r=0;r<ReligionCatalog.Count;r++){bool ancient=r>=12;if(ancient!=religionAncient)continue;
   string founder=world.ReligionFounder(r);string reason=world.FoundingReason(0,r);
   GUILayout.BeginHorizontal();GUILayout.Label(ReligionCatalog.Name(r)+"  ("+ReligionCatalog.Tradition(r)+")",body,GUILayout.Width(330));
   GUILayout.Label(founder!=""?"Founded by "+founder+" • "+world.Followers(r)+(world.Followers(r)==1?" city":" cities"):"Not yet founded • needs "+World.TechNames[TechnologyCatalog.Index(ReligionCatalog.Tech(r))],small,GUILayout.Width(380));
   if(founder==""&&me.founded<0){bool was=GUI.enabled;GUI.enabled=was&&reason=="";if(GUILayout.Button("Found",button,GUILayout.Width(110)))world.FoundReligion(0,r);GUI.enabled=was;}
   GUILayout.EndHorizontal();
   GUILayout.Label(ReligionCatalog.History(r)+" Followers today: "+ReligionCatalog.Adherents(r)+". Tenet — "+ReligionCatalog.TenetName(r)+": "+ReligionCatalog.TenetText(r),small);GUILayout.Space(4);}
  // Priests: any of your cities that follows a faith and has a Temple can train one, or buy one here for faith.
  {var temples=world.cities.Where(c=>c.owner==0&&c.religion>=0&&world.HasBuilding(c,36)).OrderBy(c=>c.name).ToList();GUILayout.Space(6);GUILayout.Label("Priests ("+World.PriestFaithCost+" faith, or 40 production in the city)",heading);
   GUILayout.Label("A priest carries its city's faith. Preach in or next to a city for +"+World.PreachPressure+" pressure (two sermons, halved where a priest of another faith resides). In one of your cities it keeps a citizen content, and it heals your units beside it by 2 a turn.",small);
   if(temples.Count==0)GUILayout.Label("None of your cities both follows a faith and has a Temple yet.",small);
   foreach(var c in temples.Take(10)){GUILayout.BeginHorizontal();GUILayout.Label(c.name+" — "+ReligionCatalog.Name(c.religion),body,GUILayout.Width(560));string why=world.BuyPriestReason(0,c);bool was=GUI.enabled;GUI.enabled=was&&why=="";if(GUILayout.Button("Buy a Priest",button,GUILayout.Width(200)))world.BuyPriest(0,c);GUI.enabled=was;GUILayout.EndHorizontal();}}
  if(me.founded>=0){GUILayout.Space(6);GUILayout.Label("Missionaries ("+World.MissionCost+" faith each, +150 pressure)",heading);
   var targets=world.cities.Where(c=>c.religion!=me.founded&&world.At(c.x,c.y).seen&&world.MissionReason(0,c)!="Too far from your faithful cities (12 tiles).").OrderBy(c=>c.owner==0?0:1).ThenBy(c=>c.name).Take(16).ToList();
   if(targets.Count==0)GUILayout.Label("No known city within 12 tiles of your faithful cities needs converting.",small);
   foreach(var c in targets){GUILayout.BeginHorizontal();GUILayout.Label(c.name+" ("+world.NationName(c.owner)+") — "+(c.religion>=0?"follows "+ReligionCatalog.Name(c.religion):"no majority faith"),body,GUILayout.Width(560));bool was=GUI.enabled;GUI.enabled=was&&world.MissionReason(0,c)=="";if(GUILayout.Button("Send missionaries",button,GUILayout.Width(200)))world.SendMissionaries(0,c);GUI.enabled=was;GUILayout.EndHorizontal();}}
 }
}
}
