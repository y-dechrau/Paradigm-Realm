using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The river valley (owner brief, 26 Nov 2026): the first scene of the living countryside, found and dressed on a generated map. Start
// the game with -valley. A river bends past a bridge, then turns a mill; a road runs from the mill to a small town; wheat and groves
// lie downwind of each other; a sealed gateway stands upstream by the water; the stretch above the bridge is left as quiet pasture.
// The mill is decoration (the game has no mill): World.valleyMill only says where it stands.
public partial class World {
 public static World ValleyScenario(bool awaken=false){
  // Seeds whose river makes the best valley first (measured with ValleyScore, 26 Nov 2026).
  foreach(int seed in ValleySeeds){var w=New(seed,"Rivervale",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);if(w.DressValley(awaken))return w;}
  return New(26011,"Rivervale",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);}
 static readonly int[] ValleySeeds={26026,26035,26033,26032,26060,26020};
 // How good the best valley of a seed would be, for choosing (-1 none).
 public static int ValleyScore(int seed){var w=New(seed,"Rivervale",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);Tile b;int arm,score;return w.FindValley(out b,out arm,out score)?score:-1;}
 static int FieldLook(int x,int y){unchecked{int h=x*73856093^y*19349663;return (h&0x7fffffff)%3;}}
 bool DressValley(bool awaken){
  Func<Tile,bool> dry=t=>t!=null&&t.realm==0&&t.terrain!=7&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&!Barren(t.terrain);
  Func<Tile,Tile> down=t=>{int d=RiverDown(t);return d<0?null:Step(t.x,t.y,HexDi[d],HexDj[d]);};
  Tile best;int bestScore,bestArm;if(!FindValley(out best,out bestArm,out bestScore))return false;
  // ---- the ground ----
  Tile up=null;foreach(var t in TilesAround(best.x,best.y,1))if(t.terrain==7&&down(t)==best)up=t;var midTile=down(best);var millTile=down(midTile);
  DressGround(best,midTile,millTile,up);
  return DressRest(awaken,best,bestArm,midTile,millTile,up,dry,down);}
 // The best stretch of river: pasture, bridge, a tile between, the mill, and river on beyond, bending, with land on both banks.
 bool FindValley(out Tile best,out int bestArm,out int bestScore){
  Func<Tile,bool> dry=t=>t!=null&&t.realm==0&&t.terrain!=7&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&!Barren(t.terrain);
  Func<Tile,Tile> down=t=>{int d=RiverDown(t);return d<0?null:Step(t.x,t.y,HexDi[d],HexDj[d]);};best=null;bestScore=-1;bestArm=0;
  foreach(var a in tiles){
   if(a.realm!=0||a.terrain!=7||a.y<MainHeight/5||a.y>MainHeight*4/5||a.x<9||a.x>Width-10)continue;
   // a (upstream pasture) -> bridge -> mill -> on, with a bend between.
   var bridge=down(a);if(bridge==null||bridge.terrain!=7)continue;var mid=down(bridge);if(mid==null||mid.terrain!=7)continue;var mill=down(mid);if(mill==null||mill.terrain!=7||Distance(mill.x,mill.y,bridge.x,bridge.y)<2)continue;var on=down(mill);if(on==null||on.terrain!=7)continue;
   int beyond=0;for(var r=on;r!=null&&r.terrain==7&&beyond<6;r=down(r))beyond++;int d0=RiverDown(a),d1=RiverDown(bridge),d2=RiverDown(mid),d3=RiverDown(mill);int into=(d0+3)%6;if(gateAt(a)||gateAt(bridge)||gateAt(mill))continue;
   for(int k=0;k<6;k++){if(k==into||k==d1||(k+3)%6==into||(k+3)%6==d1)continue;var n1=Step(bridge.x,bridge.y,HexDi[k],HexDj[k]);var n2=Step(bridge.x,bridge.y,-HexDi[k],-HexDj[k]);if(!dry(n1)||!dry(n2))continue;
    // The town's side: from it the mill's bank must be reachable over dry land.
    int near=0;for(int q=0;q<6;q++){var m=Step(mill.x,mill.y,HexDi[q],HexDj[q]);if(dry(m)&&Distance(m.x,m.y,n2.x,n2.y)<=2&&Distance(m.x,m.y,n1.x,n1.y)>1)near++;}if(near==0)continue;
    int land=0,wet=0;foreach(var t in TilesAround(bridge.x,bridge.y,5)){if(dry(t))land++;else if(IsWater(t.terrain))wet++;}
    int score=land*2-wet*3+(d0!=d1?12:0)+(d1!=d2?10:0)+(d2!=d3?6:0)+(HexDi[k]!=0&&HexDj[k]==0?8:0)+Math.Min(8,RiverFlow(a))*4+beyond*4+near*2;if(land<50||score<=bestScore)continue;bestScore=score;best=bridge;bestArm=k;}}
  return best!=null;}
 void DressGround(Tile best,Tile midTile,Tile millTile,Tile up){
  Func<Tile,bool> dry=t=>t!=null&&t.realm==0&&t.terrain!=7&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&!Barren(t.terrain);
  foreach(var t in TilesAround(best.x,best.y,7)){t.village=false;t.ruin=false;t.resource=0;t.farm=t.mine=t.fort=false;t.road=false;t.woods=0;if(!dry(t))continue;if(Distance(t.x,t.y,best.x,best.y)<=5){t.terrain=0;t.cleared=true;}}
  units.RemoveAll(u=>u.owner!=0&&Distance(u.x,u.y,best.x,best.y)<=12);cities.RemoveAll(c=>Distance(c.x,c.y,best.x,best.y)<=10);animals=new List<Animal>();}
 bool DressRest(bool awaken,Tile best,int bestArm,Tile midTile,Tile millTile,Tile up,Func<Tile,bool> dry,Func<Tile,Tile> down){
  Tile farSide=Step(best.x,best.y,HexDi[bestArm],HexDj[bestArm]),nearSide=Step(best.x,best.y,-HexDi[bestArm],-HexDj[bestArm]);
  // ---- the town: two steps from the mill on the bridge's near side, off the water ----
  Tile town=null;int far=-1;foreach(var t in TilesAround(millTile.x,millTile.y,3)){if(!dry(t)||Distance(t.x,t.y,millTile.x,millTile.y)<2||Distance(t.x,t.y,nearSide.x,nearSide.y)>3||Distance(t.x,t.y,nearSide.x,nearSide.y)<1||Distance(t.x,t.y,best.x,best.y)<2||Distance(t.x,t.y,farSide.x,farSide.y)<=Distance(t.x,t.y,nearSide.x,nearSide.y))continue;bool bank=TilesAround(t.x,t.y,1).Any(n=>n.terrain==7);int score=(bank?0:6)-Math.Abs(Distance(t.x,t.y,millTile.x,millTile.y)-2)*4+(Step(t.x,t.y,0,0)!=null?0:0)-Math.Abs(t.y-best.y);if(score>far){far=score;town=t;}}
  if(town==null)return false;
  var lead=units.FirstOrDefault(u=>u.owner==0&&u.type==0);if(lead==null)return false;lead.x=town.x;lead.y=town.y;lead.moves=4;turn=Math.Max(turn,1);foreach(var id in new[]{"agriculture","wheel","bronze","archery","writing","maths","mysticism","laws","sailing",BridgeTech}){int i=TechnologyCatalog.Index(id);if(i>=0)nations[0].tech[i]=true;}tech=nations[0].tech;
  EnableDimensions();EnableMaterials();EnableStatecraft();nationalForces=true;dormantWorlds=true;
  var city=Found(lead,"Millbrook");if(city==null)return false;city.pop=6;city.food=10;
  // ---- the roads: mill to town, and town by the bridge to the far bank and beyond ----
  Func<Tile,Tile,List<Tile>> way=(from,to)=>{var prev=new Dictionary<Tile,Tile>{{from,null}};var q=new Queue<Tile>();q.Enqueue(from);while(q.Count>0){var t=q.Dequeue();if(t==to){var path=new List<Tile>();for(var p=t;p!=null;p=prev[p])path.Add(p);path.Reverse();return path;}foreach(var n in TilesAround(t.x,t.y,1)){if(prev.ContainsKey(n)||n!=to&&!dry(n))continue;prev[n]=t;q.Enqueue(n);}}return null;};
  var mr=way(millTile,town);var br=way(nearSide,town);if(mr==null||br==null||mr.Count>5)return false;foreach(var t in mr.Concat(br))if(t!=midTile)t.road=true;best.road=true;farSide.road=true;var beyond=Step(farSide.x,farSide.y,HexDi[bestArm],HexDj[bestArm]);if(dry(beyond))beyond.road=true;
  // ---- fields near the town and mill (the ripe ones first), never upstream of the bridge ----
  foreach(var t in TilesAround(town.x,town.y,2).Concat(TilesAround(millTile.x,millTile.y,1)).Distinct().Where(t=>dry(t)&&!t.road&&t!=town&&Distance(t.x,t.y,up.x,up.y)>1&&t.terrain==0).OrderBy(t=>FieldLook(t.x,t.y)==0?0:1).ThenBy(t=>Distance(t.x,t.y,town.x,town.y)).ThenBy(t=>t.y*Width+t.x).Take(6))t.farm=true;
  // ---- groves: downwind (east) of the fields and round the valley's edge; the pasture above the bridge stays open ----
  foreach(var t in TilesAround(best.x,best.y,5)){if(!dry(t)||t.farm||t.road||t==town)continue;int d=Distance(t.x,t.y,best.x,best.y);bool pasture=Distance(t.x,t.y,up.x,up.y)<=1;bool besideField=TilesAround(t.x,t.y,1).Any(n=>n.farm);if(!pasture&&(besideField||d>=4)&&GroveTrees(t.x,t.y))t.cleared=false;}
  // ---- the gateway: a sealed one moved to dry ground beside the water upstream ----
  Func<Tile,bool> mirror=t=>{bool wet=false;foreach(int k in new[]{1,2}){var n=Step(t.x,t.y,HexDi[k],HexDj[k]);if(n==null||n.road||n.farm||n==town||n.terrain==7||n.gateLink>0||units.Any(u=>u.x==n.x&&u.y==n.y))return false;if(IsWater(n.terrain))wet=true;}return wet;};
  Tile gate=null;foreach(var t in TilesAround(best.x,best.y,5).OrderBy(t=>mirror(t)?0:1).ThenBy(t=>Distance(t.x,t.y,up.x,up.y)).ThenBy(t=>t.y*Width+t.x)){if(!dry(t)||t.road||t.farm||t==town||Distance(t.x,t.y,best.x,best.y)<2)continue;bool water=false;foreach(int k in new[]{0,1,2,3}){var n=Step(t.x,t.y,HexDi[k],HexDj[k]);if(n!=null&&(n.terrain==7||IsWater(n.terrain)))water=true;}if(water){gate=t;break;}}
  // Both tiles below it are lake, so that it stands over its own reflection.
  if(gate!=null&&mirror(gate))foreach(int k in new[]{1,2}){var n=Step(gate.x,gate.y,HexDi[k],HexDj[k]);if(!IsWater(n.terrain)){n.terrain=6;n.farm=false;n.cleared=false;}}
  var old=tiles.FirstOrDefault(t=>t.realm==0&&t.gateLink>0);
  if(gate!=null&&old!=null&&old!=gate){var twin=GateTwin(old);if(twin!=null){gate.gateLink=old.gateLink;old.gateLink=0;twin.gateLink=gate.y*Width+gate.x+1;gateCells=null;gate.cleared=true;gate.seen=true;gatesFound.Add(gate.y*Width+gate.x);
    if(awaken){var s=new GateStudy{owner=0,pair=GatePair(gate),known=true,turns=GateApproach.StudyTurns,approach=GateApproach.Stabilise,ready=turn+20};gateStudies.Add(s);}}}
  valleyMill=millTile.y*Width+millTile.x;
  // ---- people to see by, and one to select ----
  units.RemoveAll(u=>u.owner==0);
  foreach(var t in new[]{farSide,up}){var post=TilesAround(t.x,t.y,2).Where(n=>dry(n)&&!n.road&&!n.farm&&n!=gate&&n!=town).OrderByDescending(n=>Distance(n.x,n.y,best.x,best.y)).ThenBy(n=>n.y*Width+n.x).FirstOrDefault();if(post!=null){var u=Add(0,3,post.x,post.y);u.fortified=true;}}
  var parade=TilesAround(farSide.x,farSide.y,1).Where(n=>dry(n)&&!n.road&&!n.farm&&n!=gate&&n!=town&&!units.Any(u=>u.x==n.x&&u.y==n.y)).OrderByDescending(n=>Distance(n.x,n.y,millTile.x,millTile.y)).FirstOrDefault()??farSide;var guard=Add(0,2,parade.x,parade.y);guard.fortified=true;
  gold=nations[0].gold=200;RebuildTerritory();networks=null;Reveal();if(!Worked(city).Contains(millTile))ToggleLock(city,millTile);foreach(var t in tiles)if(t.realm==0&&Distance(t.x,t.y,best.x,best.y)<=6)LearnTile(0,t);
  city.build=-1;city.buildingProject=-1;for(int o=1;o<civCount;o++)Relation(0,o).peaceUntil=turn+200;return Worked(city).Contains(millTile);}
 bool gateAt(Tile t){return t.gateLink>0;}
 // The bridge of the valley, for the camera: the river tile with a road just above the mill.
 public Tile ValleyCentre(){if(valleyMill<0)return null;var mill=tiles[valleyMill];foreach(var t in TilesAround(mill.x,mill.y,2))if(t.terrain==7&&t.road&&t!=mill){int d=RiverDown(t);var n=d<0?null:Step(t.x,t.y,HexDi[d],HexDj[d]);if(n==null)continue;d=RiverDown(n);n=d<0?null:Step(n.x,n.y,HexDi[d],HexDj[d]);if(n==mill)return t;}return mill;}
}
public partial class RealmGame {
 void OpenValley(bool awaken){
  string file=Path.Combine(Application.temporaryCachePath,"river-valley.json");File.WriteAllText(file,JsonUtility.ToJson(World.ValleyScenario(awaken)));LoadFile(file);activeSavePath=null;guide=false;tutorialOn=false;tutorialStep=-1;world.newContacts.Clear();selected=-1;cityId=-1;isoWidth=96;var c=world.ValleyCentre();if(c!=null)FocusTile(c.x,c.y);status="The river valley. Watch the bridge, the mill and the road.";}
 // Development aid: "-valleyShot <folder>" photographs the valley with and without the living countryside, checks what the brief asks to
 // be checked (logged as PROBE lines), measures both, and saves a fixed-camera run of frames for a clip; then quits.
 IEnumerator ValleyShot(string folder){
  Directory.CreateDirectory(folder);devRun=true;yield return Frames(5);OpenValley(false);yield return Frames(10);
  bool was=AmbientOn;Action quiet=()=>{world.newContacts.Clear();world.researchPrompts.Clear();world.buildPrompts.Clear();world.buildNotices.Clear();if(world.rivalCalls!=null)world.rivalCalls.Clear();if(world.wayChoices!=null)world.wayChoices.Clear();gateScene=null;detailsScreen="";};quiet();
  var centre=world.ValleyCentre();Action aim=()=>{if(centre!=null)FocusTile(centre.x,centre.y);};
  Action<bool,string> probe=(ok,what)=>Debug.Log("PROBE "+(ok?"pass":"FAIL")+": "+what);Func<string,int> seen=what=>{int n;return ambientCounts.TryGetValue(what,out n)?n:0;};
  Debug.Log("PROBEINFO valley seed "+world.seed);foreach(int zoom in new[]{64,96,128}){isoWidth=zoom;aim();AmbientOn=false;yield return Frames(60);yield return Shot(folder,"valley-"+zoom+"-without.bmp",20);AmbientOn=true;yield return new WaitForSecondsRealtime(4f);yield return Shot(folder,"valley-"+zoom+"-with.bmp",0);}
  // Everything of the brief is seen within a minute at the closest zoom.
  isoWidth=96;aim();var found=new Dictionary<string,int>();float until=Time.realtimeSinceStartup+40;while(Time.realtimeSinceStartup<until){foreach(var kv in ambientCounts){int n;found.TryGetValue(kv.Key,out n);found[kv.Key]=Math.Max(n,kv.Value);}yield return null;}
  foreach(var what in new[]{"wheat","canopy","current","cart","smoke","leaf"})probe(found.ContainsKey(what),what+" is seen in the valley ("+(found.ContainsKey(what)?found[what]:0)+" at most)");probe(world.valleyMill>=0&&wheelSpeed>.9f,"the mill turns while its city works the land");
  // A clip: fixed camera, ten frames a second, long enough for a cart's journey and a gust.
  isoWidth=128;aim();yield return Frames(30);{float start=Time.realtimeSinceStartup;for(int f=0;f<230;f++){while(Time.realtimeSinceStartup<start+f*.1f)yield return null;yield return Shot2(folder,"clip-"+f.ToString("000")+".bmp");}}
  // Zoom: smoke alone at 48, nothing at 32.
  isoWidth=48;aim();yield return Frames(5);int at48=0,smoke48=0;for(int f=0;f<400;f++){at48=Math.Max(at48,ambientDraws);smoke48=Math.Max(smoke48,seen("smoke"));yield return null;}probe(at48==smoke48,"at strategic zoom only smoke is left ("+at48+" marks)");isoWidth=32;aim();yield return Frames(10);probe(ambientDraws==0,"and at the widest nothing moves");
  // A road cut while a cart is on it.
  isoWidth=96;aim();yield return Frames(40);until=Time.realtimeSinceStartup+30;while(carts.Count==0||seen("cart")==0){if(Time.realtimeSinceStartup>until)break;yield return null;}
  if(carts.Count>0){var cart=carts[0];var cut=cart.tiles[cart.tiles.Count/2];bool cutIsMill=cut.y*world.Width+cut.x==world.valleyMill;cut.road=false;world.Reveal();yield return new WaitForSecondsRealtime(1.6f);probe(!carts.Contains(cart),"a cart whose road is cut fades away");probe(carts.Count(c=>c.key==cart.key)==0,"and none takes its place while it is cut");cut.road=true;world.Reveal();yield return new WaitForSecondsRealtime(1.2f);probe(carts.Count(c=>!c.dying)>0,"one comes back when the road does");}else probe(false,"a cart is on the road");
  // Production stops.
  {var city=world.cities.First(c=>c.owner==0);int pop=city.pop;city.occupation=3;yield return new WaitForSecondsRealtime(2.2f);probe(carts.Count(c=>!c.dying)==0&&wheelSpeed<.2f,"with work stopped the carts go and the wheel runs down");city.occupation=0;yield return new WaitForSecondsRealtime(2.2f);probe(wheelSpeed>.8f,"and starts again");}
  // Fog: the same land remembered but not in sight shows no movement.
  {var watchers=world.units.Where(u=>u.owner==0).ToList();var city=world.cities.First(c=>c.owner==0);world.units.RemoveAll(u=>u.owner==0);int owner=city.owner;var lost=world.cities.ToList();world.cities.Clear();foreach(var t in world.tiles)if(t.owner==0)t.owner=-1;var keepRoads=world.tiles.Where(t=>t.road).ToList();world.Reveal();yield return Frames(20);int hid=0;for(int f=0;f<500;f++){hid=Math.Max(hid,ambientDraws);yield return null;}probe(hid==0,"nothing moves on land that is remembered but not in sight ("+hid+" marks)");yield return Shot(folder,"valley-remembered.bmp",10);world.cities.AddRange(lost);world.units.AddRange(watchers);world.RebuildTerritory();world.Reveal();}
  // End a turn, then save and load: no doubled carts, no second first wisp.
  yield return Frames(30);quiet();int before=carts.Count(c=>!c.dying);EndTurn();while(turnRunning)yield return null;quiet();yield return new WaitForSecondsRealtime(1.5f);probe(carts.Select(c=>c.key).Distinct().Count()==carts.Count,"ending a turn doubles no cart ("+before+" before, "+carts.Count+" after)");
  {string file=Path.Combine(folder,"valley.json");File.WriteAllText(file,JsonUtility.ToJson(world));int mill=world.valleyMill;LoadFile(file);activeSavePath=null;quiet();selected=-1;isoWidth=96;centre=world.ValleyCentre();aim();yield return new WaitForSecondsRealtime(2f);probe(world.valleyMill==mill&&carts.Select(c=>c.key).Distinct().Count()==carts.Count&&hearthBorn.Count==0,"saving and loading keeps the mill, doubles nothing and lights no hearth anew");}
  // A new settlement's first wisp.
  {var spot=world.tiles.FirstOrDefault(t=>t.realm==0&&world.Visible(t.x,t.y)&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&t.terrain!=7&&world.cities.All(c=>world.Distance(c.x,c.y,t.x,t.y)>=4));if(spot!=null){var settler=world.Add(0,0,spot.x,spot.y);settler.moves=4;var made=world.Found(settler,"Newhearth");if(made!=null){FocusTile(made.x,made.y);world.Reveal();int wisp=0;float end=Time.realtimeSinceStartup+2f;while(Time.realtimeSinceStartup<end){wisp=Math.Max(wisp,hearthBorn.ContainsKey(made.id)?seen("smoke"):0);yield return null;}probe(wisp>0,"a new city's first wisp rises within two seconds");yield return Shot(folder,"valley-first-wisp.bmp",0);}else probe(false,"a second city could be founded ("+world.FoundReason(settler)+")");}else probe(false,"there is room for a second city");}
  // A unit selected in the busiest part.
  aim();{var u=world.units.FirstOrDefault(a=>a.owner==0&&a.type==2);var busy=world.valleyMill>=0&&carts.Count>0?carts[0].tiles[Math.Min(1,carts[0].tiles.Count-1)]:null;if(u!=null&&busy!=null){u.x=busy.x;u.y=busy.y;u.fortified=false;u.moves=4;world.Reveal();}if(u!=null){selected=u.id;yield return new WaitForSecondsRealtime(2f);yield return Shot(folder,"valley-unit.bmp",0);selected=-1;}}
  // Speed: the map's own drawing time, the same view with and without.
  foreach(int zoom in new[]{64,128}){isoWidth=zoom;aim();yield return Frames(60);var ms=new double[2];foreach(int on in new[]{0,1,0,1}){AmbientOn=on==1;yield return Frames(20);mapClock.Reset();int frames=0;for(int f=0;f<240;f++){yield return null;frames++;}ms[on]+=mapClock.Elapsed.TotalMilliseconds/frames/2;}Debug.Log("PROBEINFO map drawing at "+zoom+": "+ms[0].ToString("0.00")+" ms a frame without, "+ms[1].ToString("0.00")+" ms with ("+ambientDraws+" marks)");}
  AmbientOn=true;
  // The gateway stirring.
  OpenValley(true);quiet();yield return Frames(10);quiet();isoWidth=128;var door=world.HomeGates().FirstOrDefault(g=>world.gatesFound.Contains(g.y*world.Width+g.x));if(door!=null){FocusTile(door.x,door.y);bool glow=false,mirror=false;float firstMirror=-1,firstStone=-1,end=Time.realtimeSinceStartup+20;yield return Frames(30);while(Time.realtimeSinceStartup<end){if(seen("reflection")>0){mirror=true;if(firstMirror<0)firstMirror=ambientNow;}if(seen("stone")>0){glow=true;if(firstStone<0){firstStone=ambientNow;yield return new WaitForSecondsRealtime(.9f);yield return Shot(folder,"valley-gate-stirs.bmp",0);}}yield return null;}probe(glow,"a gateway being opened shows colour on its stone");probe(!mirror||firstMirror<=firstStone||firstStone<0,"its reflection, where there is water, comes first ("+mirror+")");}else probe(false,"the valley has a gateway");
  AmbientOn=was;Application.Quit();}
 // A frame for the clip: no settling time.
 IEnumerator Shot2(string folder,string name){yield return new WaitForEndOfFrame();int w=Screen.width,h=Screen.height;var frame=new Texture2D(w,h,TextureFormat.RGBA32,false);frame.ReadPixels(new Rect(0,0,w,h),0,0);frame.Apply();var px=frame.GetPixels32();Destroy(frame);int row=(w*3+3)&~3;var bytes=new byte[54+row*h];Action<int,int> put=(at,v)=>{bytes[at]=(byte)v;bytes[at+1]=(byte)(v>>8);bytes[at+2]=(byte)(v>>16);bytes[at+3]=(byte)(v>>24);};bytes[0]=(byte)'B';bytes[1]=(byte)'M';put(2,bytes.Length);put(10,54);put(14,40);put(18,w);put(22,h);bytes[26]=1;bytes[28]=24;put(34,row*h);for(int y=0;y<h;y++)for(int x=0;x<w;x++){var c=px[y*w+x];int at=54+y*row+x*3;bytes[at]=c.b;bytes[at+1]=c.g;bytes[at+2]=c.r;}File.WriteAllBytes(Path.Combine(folder,name),bytes);}
}
}
