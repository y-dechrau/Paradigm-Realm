using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (owner report, 21 Nov 2026: turns slow down after discovering another world): "-turnProbe <save> <folder>"
// loads a saved game in the real build, plays three End Turns as the button does, and writes <folder>/turns.txt: each turn's
// length, its slowest frames and pieces, the frames after it (animations), and separately the autosave (validation, JSON,
// writing), the minimap and atlas pictures and a full sight update. Autosaves go to <folder>, never beside the player's saves.
public partial class RealmGame {
 IEnumerator TurnProbe(string save,string folder){
  var log=new StringBuilder();var clock=new Stopwatch();
  yield return Frames(5);tutorialOn=false;tutorialStep=-1;clock.Start();LoadFile(save);activeSavePath=null;autoSaveFolder=folder;log.AppendLine("load "+clock.ElapsedMilliseconds+" ms: "+status);
  if(world==null){File.WriteAllText(Path.Combine(folder,"turns.txt"),log.ToString());Application.Quit();yield break;}
  QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;mainMenu=false;nameSetup=false;
  int natives=Enumerable.Range(1,world.civCount-1).Count(o=>world.IsNativeNation(o)),metNatives=Enumerable.Range(1,world.civCount-1).Count(o=>world.IsNativeNation(o)&&world.met[o]);
  log.AppendLine("turn "+world.turn+", "+world.tiles.Length+" tiles, "+world.civCount+" nations ("+natives+" parallel-world, "+metNatives+" met), "+world.cities.Count+" cities, "+world.units.Count+" units; gates open "+(world.openedGates!=null?world.openedGates.Count:0)+", locked "+world.gatesLocked+"; viewing world "+viewRealm+"; your units in other worlds "+world.units.Count(u=>u.owner==0&&world.At(u.x,u.y).realm>0));
  {var sb2=new StringBuilder("worlds: ");for(int r=1;r<=World.RealmCount;r++){int peoples=Enumerable.Range(1,world.civCount-1).Count(o=>world.IsNativeNation(o)&&CivilisationCatalog.NativeRealm(world.nations[o].civ)==r);bool home=world.units.Any(u=>!world.IsNativeNation(u.owner)&&world.At(u.x,u.y).realm==r);string seenBy=string.Join(",",Enumerable.Range(0,world.civCount).Where(o=>!world.IsNativeNation(o)&&world.RealmDiscovered(o,r)).Select(o=>o.ToString()).ToArray());sb2.Append(r+(world.RealmAwake(r)?" awake":" dormant")+" ("+peoples+" peoples"+(home?", home unit there":"")+(seenBy!=""?", seen by "+seenBy:"")+"); ");}log.AppendLine(sb2.ToString());log.AppendLine("dormant peoples: "+Enumerable.Range(1,world.civCount-1).Count(o=>world.Dormant(o))+", dormancy on: "+world.dormantWorlds);}
  yield return Frames(30);
  {var one=new Stopwatch();var idle=new System.Collections.Generic.List<double>();one.Start();for(int k=0;k<60;k++){yield return null;idle.Add(one.Elapsed.TotalMilliseconds);one.Reset();one.Start();}log.AppendLine("idle frames: average "+idle.Average().ToString("0.0")+" ms, slowest "+idle.Max().ToString("0")+" ms");}
  // The pieces outside the turn itself.
  clock.Restart();bool valid=world.Valid();long tValid=clock.ElapsedMilliseconds;clock.Restart();string json=JsonUtility.ToJson(world,false);long tJson=clock.ElapsedMilliseconds;clock.Restart();File.WriteAllText(Path.Combine(folder,"probe-save.tmp"),json);long tWrite=clock.ElapsedMilliseconds;
  log.AppendLine("autosave pieces: validation "+tValid+" ms ("+valid+"), JSON "+tJson+" ms ("+(json.Length/1048576.0).ToString("0.0")+" MB), writing "+tWrite+" ms; autosave every "+(world.tiles.Length>60000?"10":world.tiles.Length<=9000?"1":"5")+" turns");json=null;
  clock.Restart();BuildMiniTexture();long tMini=clock.ElapsedMilliseconds;clock.Restart();BuildAtlasTexture();long tAtlas=clock.ElapsedMilliseconds;clock.Restart();world.Reveal();long tReveal=clock.ElapsedMilliseconds;
  log.AppendLine("minimap picture "+tMini+" ms, atlas picture "+tAtlas+" ms (each rebuilt every 3 s), full sight update "+tReveal+" ms");
  // Which tile-sized pieces of the save are largest.
  {long tilesBytes=JsonUtility.ToJson(new TileProbeHolder{tiles=world.tiles}).Length,unitsBytes=JsonUtility.ToJson(new UnitProbeHolder{units=world.units}).Length;long nationsBytes=0;foreach(var n in world.nations)nationsBytes+=JsonUtility.ToJson(n).Length;
   log.AppendLine("save make-up: tiles "+(tilesBytes/1048576.0).ToString("0.0")+" MB, units "+(unitsBytes/1048576.0).ToString("0.0")+" MB, nations "+(nationsBytes/1048576.0).ToString("0.0")+" MB (largest nation "+(world.nations.Max(n=>JsonUtility.ToJson(n).Length)/1048576.0).ToString("0.00")+" MB)");}
  World.decisionTimes=new System.Collections.Generic.Dictionary<string,double>();
  for(int run=0;run<9;run++){
   world.researchPrompts.Clear();world.buildPrompts.Clear();if(world.newContacts!=null)world.newContacts.Clear();if(world.rivalCalls!=null)world.rivalCalls.Clear();gateScene=null;detailsScreen="";cityScreen=false;
   turnStepLog=new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string,double>>();clock.Restart();EndTurn();
   var one=new Stopwatch();one.Start();var frames=new System.Collections.Generic.List<double>();int glides=0;
   while(turnRunning){yield return null;frames.Add(one.Elapsed.TotalMilliseconds);one.Reset();one.Start();glides=Math.Max(glides,unitGlides.Count);}
   long total=clock.ElapsedMilliseconds;double sim=turnStepLog.Sum(s=>s.Value);
   var after=new System.Collections.Generic.List<double>();for(int k=0;k<40;k++){yield return null;after.Add(one.Elapsed.TotalMilliseconds);one.Reset();one.Start();glides=Math.Max(glides,unitGlides.Count);}
   log.AppendLine("   minimap repaints during and after: "+miniRepaints);miniRepaints=0;
  log.AppendLine("End Turn "+(run+1)+" (now turn "+world.turn+"): "+total+" ms on screen, of which rules "+sim.ToString("0")+" ms; "+frames.Count+" frames, slowest "+(frames.Count>0?frames.Max():0).ToString("0")+" ms, over 50 ms: "+frames.Count(f=>f>50)+"; units animating (most at once) "+glides);
   log.AppendLine("   next 40 frames: average "+after.Average().ToString("0.0")+" ms, slowest "+after.Max().ToString("0")+" ms");
   foreach(var g in turnStepLog.GroupBy(s=>s.Key.Contains("(world)")?s.Key:s.Key.Contains("(")?(world.civCount>0&&IsNativeLabel(s.Key)?"parallel-world peoples":"home nations"):s.Key).Select(g=>new{g.Key,ms=g.Sum(x=>x.Value)}).OrderByDescending(g=>g.ms).Take(6))
    log.AppendLine("   "+g.ms.ToString("0")+" ms  "+g.Key);
   {var per=turnStepLog.Where(s=>IsNativeLabel(s.Key)).GroupBy(s=>{int a=s.Key.IndexOf("after ")+6;return int.Parse(s.Key.Substring(a,s.Key.IndexOf(' ',a)-a));}).Select(g=>new{o=g.Key,ms=g.Sum(x=>x.Value),n=g.Count()}).OrderByDescending(g=>g.ms).Take(6);
    log.AppendLine("   slowest peoples: "+string.Join("; ",per.Select(p=>world.NationName(p.o)+" (world "+CivilisationCatalog.NativeRealm(world.nations[p.o].civ)+(world.Dormant(p.o)?", dormant":"")+", "+world.cities.Count(c=>c.owner==p.o)+" cities, "+world.units.Count(u=>u.owner==p.o)+" units) "+p.ms.ToString("0")+" ms").ToArray()));}
   log.AppendLine("   decisions: "+string.Join(", ",World.decisionTimes.OrderByDescending(p=>p.Value).Take(6).Select(p=>p.Key+" "+p.Value.ToString("0")+" ms").ToArray()));World.decisionTimes.Clear();
   log.AppendLine("   slowest single steps: "+string.Join("; ",turnStepLog.OrderByDescending(s=>s.Value).Take(8).Select(s=>s.Key.Replace("step after ","")+" "+s.Value.ToString("0")+" ms").ToArray()));
   turnStepLog=null;
  }
  File.WriteAllText(Path.Combine(folder,"turns.txt"),log.ToString());try{File.Delete(Path.Combine(folder,"probe-save.tmp"));}catch{}Application.Quit();
 }
 bool IsNativeLabel(string key){int open=key.IndexOf("after ")+6,space=key.IndexOf(' ',open);int who;return space>open&&int.TryParse(key.Substring(open,space-open),out who)&&who>0&&who<world.civCount&&world.IsNativeNation(who);}
 [Serializable] class TileProbeHolder{public Tile[] tiles;}
 [Serializable] class UnitProbeHolder{public System.Collections.Generic.List<Unit> units;}
}
}
