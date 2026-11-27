using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (27 Nov 2026): "-hudShot <folder> -raiderOnly" places a raider in sight, walks it one tile east and one west and
// photographs it mid-stride, with PROBE lines for the glide, the walking frames and the facing.
public partial class RealmGame {
 IEnumerator RaiderShots(string folder){
  Directory.CreateDirectory(folder);tutorialOn=false;tutorialStep=-1;guide=false;
  string scenario=Path.Combine(Application.temporaryCachePath,"raider-shot.json");File.WriteAllText(scenario,JsonUtility.ToJson(World.ValleyTalkScenario("plenty")));LoadFile(scenario);activeSavePath=null;mainMenu=false;nameSetup=false;viewRealm=0;
  selected=-1;cityId=-1;isoWidth=192;world.barbarians=true;var home=world.cities.First(c=>c.owner==0);
  var spot=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)==2&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&!world.units.Any(u=>u.x==t.x&&u.y==t.y)&&world.Visible(t.x,t.y)).FirstOrDefault();
  Probe("raider: a visible tile beside the city",spot!=null);if(spot==null){Application.Quit();yield break;}
  var r=new Raider{id=99201,x=spot.x,y=spot.y,camp=-1};world.raiders.Add(r);FocusTile(spot.x,spot.y);yield return Frames(60);yield return Shot(folder,"raider-standing.bmp",30);
  Probe("raider: walking frames load",RaiderWalkFrame(0)!=null);
  foreach(int dir in new[]{1,-1}){
   var next=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,r.x,r.y)==1&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&world.Visible(t.x,t.y)&&!world.units.Any(u=>u.x==t.x&&u.y==t.y)&&(t.x-r.x)*dir>0).FirstOrDefault();
   if(next==null){Probe("raider: a step "+(dir>0?"east":"west"),false);continue;}
   r.x=next.x;r.y=next.y;yield return Frames(4);Probe("raider: glides "+(dir>0?"east":"west"),RaiderGliding(r));Probe("raider: faces "+(dir>0?"right":"left"),raiderFacingLeft.Contains(r.id)==(dir<0));
   yield return Shot(folder,"raider-walk-"+(dir>0?"east":"west")+".bmp",0);yield return Frames(40);Probe("raider: stops gliding on arrival",!RaiderGliding(r));
  }
  yield return Shot(folder,"raider-arrived.bmp",10);Application.Quit();
 }
}
}
namespace ParadigmRealm {
// Development aid (27 Nov 2026): "-hudShot <folder> -forcesOnly" photographs Military > Forces in the owner's new look: an empty
// programme, five orders (as in the picture), a chosen formation, the mobilise question, and presses Units and Back for real.
public partial class RealmGame {
 System.Collections.IEnumerator ForcesShots(string folder){
  System.IO.Directory.CreateDirectory(folder);tutorialOn=false;tutorialStep=-1;guide=false;
  string scenario=System.IO.Path.Combine(Application.temporaryCachePath,"forces-shot.json");System.IO.File.WriteAllText(scenario,JsonUtility.ToJson(World.ValleyTalkScenario("plenty")));LoadFile(scenario);activeSavePath=null;mainMenu=false;nameSetup=false;viewRealm=0;
  world.nationalForces=true;var home=world.cities.First(c=>c.owner==0);home.pop=Math.Max(home.pop,8);world.nations[0].tech[TechnologyCatalog.Index("bronze")]=true;
  detailsScreen="Military";militaryTab=0;yield return Frames(20);yield return Shot(folder,"forces-empty.bmp",10);
  for(int k=0;k<5;k++)world.Raise(0,k%2==0?2:7,home);world.Add(0,2,home.x,home.y);world.Add(0,7,home.x,home.y);
  yield return Frames(10);yield return Shot(folder,"forces-orders.bmp",10);
  var f=world.units.First(u=>u.owner==0&&ForcesRules.Formation(u.type));forcesPick=f.id;yield return Frames(5);yield return Shot(folder,"forces-picked.bmp",10);
  AskMobilise(true);yield return Frames(5);yield return Shot(folder,"forces-mobilise.bmp",10);forcesConfirm=null;forcesConfirmDo=null;
  {var mtx=MilMatrix();var p=mtx.MultiplyPoint3x4(new Vector3(517,161,0));yield return ClickAt(p.x,p.y);yield return Frames(5);Probe("forces: the Units tab opens the list",militaryTab==1);yield return Shot(folder,"forces-units.bmp",10);}
  militaryTab=0;yield return Frames(3);{var p=MilMatrix().MultiplyPoint3x4(new Vector3(1485,106,0));yield return ClickAt(p.x,p.y);yield return Frames(5);Probe("forces: Back closes the screen",detailsScreen=="");}
  Application.Quit();
 }
}
}
namespace ParadigmRealm {
// Development aid (owner request, 27 Nov 2026): "-hudShot <folder> -courtOnly" photographs the Imperial Court as the game draws
// it for every civilisation that can appear in a game (CivilisationCatalog.Offered), one at a time worn by the first rival, as
// court-NNN.bmp with NNN the catalogue number. Frostpeak Realm, the owner's example, is left out.
public partial class RealmGame {
 System.Collections.IEnumerator CourtShots(string folder){
  System.IO.Directory.CreateDirectory(folder);tutorialOn=false;tutorialStep=-1;guide=false;
  string scenario=System.IO.Path.Combine(Application.temporaryCachePath,"court-shot.json");System.IO.File.WriteAllText(scenario,JsonUtility.ToJson(World.ValleyTalkScenario("plenty")));LoadFile(scenario);activeSavePath=null;mainMenu=false;nameSetup=false;viewRealm=0;
  world.valleyIssues.Clear();world.rivalCalls.Clear();world.met[1]=true;var n=world.nations[1];int frost=Array.IndexOf(CivilisationCatalog.Names,"Frostpeak Realm");
  var list=CivilisationCatalog.Offered().Where(c=>c!=frost).ToList();var names=new System.Text.StringBuilder();
  foreach(int c in list){
   n.civ=c;n.name=CivilisationCatalog.Names[c];n.leader=CivilisationCatalog.Leaders[c];n.portrait=CivilisationCatalog.Portraits[c];n.traits=(int[])CivilisationCatalog.Traits[c].Clone();
   // A flag of its own for each picture, made as the game makes a rival's flag (FlagArt.Generated), so no two leaders share one.
   {int o=c+1;var g=FlagArt.Generated(world.seed,o,FlagArt.OwnerColour[o%FlagArt.OwnerColour.Length]);n.flagPattern=g[0];n.flagA=g[1];n.flagB=g[2];n.flagEmblem=g[3];n.flagInk=g[4];n.flagC=(g[1]+5+o)%FlagArt.ColourNames.Length;if(n.flagC==g[2])n.flagC=g[1];}
   detailsScreen="Diplomacy";diplomacyPartner=1;diplomacyChooser=false;ResetDiplomacyUI();yield return Frames(6);
   yield return Shot(folder,"court-"+c.ToString("000")+".bmp",8);names.AppendLine(c.ToString("000")+"\t"+CivilisationCatalog.Names[c]+"\t"+CivilisationCatalog.Leaders[c]);}
  System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"names.txt"),names.ToString());Debug.Log("PROBE court shots: "+list.Count);
  Application.Quit();
 }
}
}
namespace ParadigmRealm {
// Development aid (27 Nov 2026): "-hudShot <folder> -roadOnly" lays roads through and around your first city and photographs them in
// every age, close up and at the usual zoom: the narrower road, and the town's own tile where roads stop at its edge.
public partial class RealmGame {
 System.Collections.IEnumerator RoadShots(string folder){
  System.IO.Directory.CreateDirectory(folder);tutorialOn=false;tutorialStep=-1;guide=false;
  string scenario=System.IO.Path.Combine(Application.temporaryCachePath,"road-shot.json");System.IO.File.WriteAllText(scenario,JsonUtility.ToJson(World.ValleyTalkScenario("plenty")));LoadFile(scenario);activeSavePath=null;mainMenu=false;nameSetup=false;viewRealm=0;
  world.valleyIssues.Clear();world.rivalCalls.Clear();var home=world.cities.First(c=>c.owner==0);
  foreach(var t in world.KeepTiles(home.y*world.Width+home.x,3))if(!PixelArt.IsWater(t.terrain)&&(t.y==home.y||t.x==home.x||world.Distance(t.x,t.y,home.x,home.y)<=1))t.road=true;
  selected=-1;cityId=-1;
  foreach(int zoom in new[]{192,96}){isoWidth=zoom;FocusTile(home.x,home.y);yield return Frames(60);yield return Shot(folder,"roads-"+zoom+".bmp",30);}
  Application.Quit();
 }
}
}
