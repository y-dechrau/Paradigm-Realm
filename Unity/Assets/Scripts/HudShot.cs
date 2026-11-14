using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm { public partial class RealmGame {
 // Development aid: "-hudShot <folder>" photographs the owner's requests of 14 Nov 2026 (long notes shown in full, the settler's
 // suggestion box above the unit panel, the selection outline behind the unit), then quits.
 IEnumerator HudShot(string folder){
  Directory.CreateDirectory(folder);yield return Frames(5);
  seedText="421";mainMenu=false;nameSetup=false;guide=false;tutorialOn=false;tutorialStep=-1;StartGame();hasSession=true;guide=false;tutorialOn=false;tutorialStep=-1;
  yield return Frames(10);
  world.Note("A Megatherium attacked your Warrior; it is getting away and will explore on.");world.Note("Azurehold grew to population 6.");
  var settler=world.units.FirstOrDefault(u=>u.owner==0&&u.type==0);
  if(settler!=null){selected=settler.id;cityId=-1;FocusTile(settler.x,settler.y);}
  yield return Frames(30);yield return Shot(folder,"hud-settler.bmp");
  moreActions=true;yield return Frames(10);yield return Shot(folder,"hud-settler-more.bmp");moreActions=false;
  var guard=world.units.FirstOrDefault(u=>u.owner==0&&u.type!=0);
  if(guard!=null){selected=guard.id;isoWidth=192;FocusTile(guard.x,guard.y);yield return Frames(30);yield return Shot(folder,"hud-unit-outline.bmp");}
  // 14 Nov 2026 (second set): the city screen against the map, workers at work, the livestock, and the two diplomacy screens.
  City home=null;if(settler!=null&&world.units.Contains(settler))home=world.Found(settler);
  if(home!=null){
   world.Reveal();var ring=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)==1&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)).ToList();
   if(ring.Count>0){ring[0].farm=true;ring[0].terrain=0;ring[0].cleared=true;ring[0].owner=0;}
   foreach(string id in new[]{"agriculture","wheel","bronze","iron"})world.tech[TechnologyCatalog.Index(id)]=true;
   int[] herd={16,17,18,19,4};int h=0;foreach(var t in world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)==2&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)).Take(5)){t.resource=herd[h++];t.seen=true;}
   // Three workers beside the city, one at each tool.
   var spots=ring.Skip(1).Take(3).ToList();int[] jobs={5,3,2};for(int k=0;k<spots.Count;k++){var w=world.Add(0,1,spots[k].x,spots[k].y);w.job=jobs[k];w.hasOrder=false;}
   selected=-1;cityId=home.id;isoWidth=160;FocusTile(home.x,home.y);yield return Frames(30);yield return Shot(folder,"hud-map-city.bmp");
   yield return Frames(3);yield return Shot(folder,"hud-workers-b.bmp");
   cityScreen=true;cityMapScroll=Vector2.zero;yield return Frames(20);yield return Shot(folder,"hud-city-screen.bmp");cityScreen=false;
   // Soldiers: each type strikes the tile beside it (mid-blow), then takes guard (mid-clip, then held).
   var field=world.tiles.Where(t=>t.realm==0&&world.Distance(t.x,t.y,home.x,home.y)>=2&&world.Distance(t.x,t.y,home.x,home.y)<=4&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)).Take(5).ToList();
   int[] types={2,7,4,8,5};var soldiers=new System.Collections.Generic.List<Unit>();
   for(int k=0;k<field.Count;k++){var s=world.Add(0,types[k],field[k].x,field[k].y);soldiers.Add(s);}
   selected=-1;isoWidth=128;FocusTile(home.x,home.y);yield return Frames(10);
   if(world.strikeLog==null)world.strikeLog=new System.Collections.Generic.List<int[]>();
   foreach(var s in soldiers){var to=world.Step(s.x,s.y,1,0)??world.Step(s.x,s.y,-1,0);if(to!=null)world.strikeLog.Add(new[]{s.id,to.x,to.y});}
   yield return new WaitForSecondsRealtime(.45f);yield return Shot(folder,"hud-strike-mid.bmp",0);
   yield return new WaitForSecondsRealtime(1f);foreach(var s in soldiers)s.fortified=true;
   yield return new WaitForSecondsRealtime(.35f);yield return Shot(folder,"hud-guard-mid.bmp",0);
   yield return new WaitForSecondsRealtime(1f);yield return Shot(folder,"hud-guard-held.bmp");
   foreach(var s in soldiers)world.units.Remove(s);
  }
  for(int o=1;o<world.civCount;o++){if(world.IsNativeNation(o))continue;world.met[o]=true;}world.newContacts.Clear();
  detailsScreen="Diplomacy";diplomacyChooser=true;yield return Frames(20);yield return Shot(folder,"hud-council.bmp");
  diplomacyPartner=Enumerable.Range(1,world.civCount-1).First(o=>world.met[o]);diplomacyChooser=false;ResetDiplomacyUI();yield return Frames(20);yield return Shot(folder,"hud-court.bmp");
  detailsScreen="";
  Application.Quit();
 }
}}
