using System;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine;
using ParadigmRealm;
class UIProgram{
static BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
static RealmGame game=new RealmGame();
static void Set(string n,object v)=>typeof(RealmGame).GetField(n,flags).SetValue(game,v);
static T Get<T>(string n)=>(T)typeof(RealmGame).GetField(n,flags).GetValue(game);
static object Call(string n,params object[] a){try{return typeof(RealmGame).GetMethod(n,flags).Invoke(game,a);}catch(TargetInvocationException e)when(e.InnerException is ExitGUIException){return null;}}
static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
static void Key(KeyCode k){Event.current=new Event{type=EventType.KeyDown,keyCode=k};Call("MapKeyboardInput");}
static void Main(){
Directory.CreateDirectory(Application.persistentDataPath);var w=World.New(73);Set("world",w);Set("mainMenu",false);Set("hasSession",true);Call("Styles");Call("MakeArt");Check(Get<System.Collections.Generic.HashSet<Texture2D>>("loadedTerrain").Count==21,"all texture paths resolve to packaged resources");
var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));var u=w.units.First(a=>a.owner==0&&a.type==2);Set("selected",-1);int moves=u.moves;Key(KeyCode.Space);Check(u.moves==moves&&Event.current.type==EventType.Used,"unselected Space has no gameplay effect and is consumed");
Set("selected",u.id);Key(KeyCode.Space);Check(u.moves==0,"Space skips selected unit");int turn=w.turn;Key(KeyCode.Return);Check(w.turn==turn+1,"Return ends exactly one turn");turn=w.turn;Key(KeyCode.KeypadEnter);Check(w.turn==turn+1,"keypad Enter ends exactly one turn");
Set("cityScreen",true);turn=w.turn;Key(KeyCode.Return);Check(w.turn==turn,"city screen blocks map keyboard");Set("cityScreen",false);
Set("pauseMenu",true);Key(KeyCode.Return);Check(w.turn==turn,"pause blocks map keyboard");Set("pauseMenu",false);
for(int i=0;i<w.tech.Length;i++)w.tech[i]=true;
Event.current=new Event{type=EventType.MouseUp};GUI.ClickText="Worker";Call("DrawProductionLine",c,false,new Rect(485,270,365,438));Check(c.build==1&&c.queue.Count==0,"plain click selects one unit");
GUI.enabled=true;Event.current=new Event{type=EventType.MouseUp,control=true};GUI.ClickText="Worker";Call("DrawProductionLine",c,false,new Rect(485,270,365,438));Check(c.queue.SequenceEqual(new[]{1}),"Ctrl click adds one unit");
GUI.enabled=true;Event.current=new Event{type=EventType.MouseUp,control=true};GUI.ClickText="Scout";Call("DrawProductionLine",c,false,new Rect(485,270,365,438));Check(c.queue.SequenceEqual(new[]{1,3}),"Ctrl click adds varying units");
GUI.enabled=true;Event.current=new Event{type=EventType.MouseDown,control=true,button=1,mousePosition=new Vector2(500,300)};Call("DrawProductionLine",c,false,new Rect(485,270,365,438));Check(c.queue.SequenceEqual(new[]{1}),"Ctrl right click removes last unit");
GUI.enabled=true;Event.current=new Event{type=EventType.MouseUp};GUI.ClickText="Granary";Call("DrawProductionLine",c,true,new Rect(870,270,365,438));Check(c.buildingProject==7&&c.build==1,"building click leaves unit line intact");
GUI.enabled=true;Event.current=new Event{type=EventType.MouseUp,control=true};GUI.ClickText="Library";Call("DrawProductionLine",c,true,new Rect(870,270,365,438));Check(c.buildingQueue.SequenceEqual(new[]{8}),"Ctrl click queues building");
GUI.enabled=true;Event.current=new Event{type=EventType.MouseDown,control=true,button=1,mousePosition=new Vector2(900,300)};Call("DrawProductionLine",c,true,new Rect(870,270,365,438));Check(c.buildingQueue.Count==0&&c.buildingProject==7&&c.queue.Count==1,"building removal leaves training queue intact");
Event.current=new Event{type=EventType.Repaint};GUI.enabled=true;GUI.Labels.Clear();Call("DrawResearchScreen");Check(World.TechNames.All(t=>GUI.Labels.Contains(t)),"all tech cards drawn");Check(GUI.Labels.Contains("Requires all")&&GUI.Labels.Contains("Original design unlocks"),"prerequisites and planned content labelled");
for(int i=0;i<TechnologyCatalog.Count;i++)Call("FocusTechnology",i);Call("DrawResearchScreen");Check(Get<int>("inspectedTech")==TechnologyCatalog.Count-1,"last technology can be focused");
var patterns=(string[])typeof(RealmGame).GetField("IconPatterns",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);Check(patterns.Length==21&&patterns.Distinct().Count()==21&&patterns.All(p=>p.Replace("/","").Replace(" ","").Length==64),"21 distinct valid pixel icons");
Set("cityId",c.id);GUI.Labels.Clear();Call("DrawCityScreen");Check(GUI.Labels.Any(s=>s.StartsWith("The land around"))&&GUI.Labels.Contains("Buildings")&&GUI.Labels.Any(s=>s.StartsWith("Hover over the land")),"three-panel city screen: organic land view, totals and building list drawn");
GUI.Labels.Clear();Call("DrawMainMenu");Check(GUI.Labels.Contains("LOAD GAME"),"main menu contains Load");
File.WriteAllText(Path.Combine(Application.persistentDataPath,"earth-named-roundtrip.json"),JsonUtility.ToJson(w));Set("loadScreen",true);Call("LoadFile",Path.Combine(Application.persistentDataPath,"earth-named-roundtrip.json"));Check(Get<World>("world").Valid()&&!Get<bool>("loadScreen"),"valid dual production save loads");
var keep=Get<World>("world");File.WriteAllText(Path.Combine(Application.persistentDataPath,"earth-broken.json"),"broken");Call("LoadFile",Path.Combine(Application.persistentDataPath,"earth-broken.json"));Check(ReferenceEquals(keep,Get<World>("world"))&&Get<string>("status").StartsWith("Could not load"),"bad save preserves current game");
var doomed=Path.Combine(Application.persistentDataPath,"earth-named-delete-me.json");File.WriteAllText(doomed,"x");File.WriteAllText(doomed+".bak","x");Set("deleteCandidate",doomed);Call("DeleteSaveFile",doomed);Check(!File.Exists(doomed)&&!File.Exists(doomed+".bak")&&Get<string>("deleteCandidate")==null&&Get<string>("loadError").StartsWith("Deleted"),"saved game and its backup can be deleted");
var outside=Path.Combine(Application.persistentDataPath,"notes.txt");File.WriteAllText(outside,"keep");Call("DeleteSaveFile",outside);Check(File.Exists(outside)&&Get<string>("loadError").StartsWith("Could not delete"),"only saved games can be deleted");File.Delete(outside);
Console.WriteLine("PASS: simulated input, two production lanes, research and city drawing calls, unique icons, Load, save failure and save deletion handling (Unity API doubles)");
}}
