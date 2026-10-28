using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ParadigmRealm;
class SettlementUIChecks {
 static BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 static void Set(RealmGame g,string name,object v){typeof(RealmGame).GetField(name,flags).SetValue(g,v);}
 static T Get<T>(RealmGame g,string name){return (T)typeof(RealmGame).GetField(name,flags).GetValue(g);}
 static void Call(RealmGame g,string name,params object[] args){try{typeof(RealmGame).GetMethod(name,flags).Invoke(g,args);}catch(TargetInvocationException e)when(e.InnerException is ExitGUIException){}}
 static void Main(){
  var w=World.New(213);foreach(var t in w.tiles){t.terrain=t.rememberedTerrain=0;t.resource=t.rememberedResource=0;t.seen=true;t.rememberedOwner=t.owner;for(int n=0;n<3;n++)w.nations[n].known[t.y*World.Width+t.x]=true;}
  var u=w.units.First(a=>a.owner==0&&a.type==0);var game=new RealmGame();Set(game,"world",w);Set(game,"selected",u.id);Set(game,"mainMenu",false);Set(game,"hasSession",true);Call(game,"Styles");Call(game,"MakeArt");
  var before=JsonUtility.ToJson(w);Call(game,"OpenSettlementAdviser",u);
  Check(Get<string>(game,"detailsScreen")=="Settlement adviser","adviser opens");
  Check(JsonUtility.ToJson(w)==before,"recommendation calculation does not mutate save state");
  GUI.Labels.Clear();Call(game,"DrawSettlementAdviser");Check(GUI.Labels.Any(s=>s.Contains("population limit"))&&GUI.Labels.Any(s=>s.Contains("upkeep")),"adviser explains housing and upkeep");
  var chosen=Get<List<SettlementReport>>(game,"settlementReports").First(r=>r.x!=u.x||r.y!=u.y);int cityCount=w.cities.Count;
  GUI.ClickText="Travel to "+chosen.x+", "+chosen.y;Call(game,"DrawSettlementAdviser");
  Check(Get<string>(game,"detailsScreen")==""&&Get<int>(game,"selected")==u.id,"travel returns to selected unit on map");
  Check(w.cities.Count==cityCount&&w.units.Contains(u),"travel never automatically founds a player city");
  Check(u.hasOrder||u.x==chosen.x&&u.y==chosen.y,"travel uses ordinary persistent movement orders");
  var loaded=JsonUtility.FromJson<World>(JsonUtility.ToJson(w));loaded.UpgradeSave();Check(loaded.Valid(),"v7 save remains valid after adviser order");
  var restored=loaded.units.First(a=>a.id==u.id);Check(restored.destinationX==u.destinationX&&restored.destinationY==u.destinationY&&restored.hasOrder==u.hasOrder,"movement plan survives save load");
  Console.WriteLine("PASS settlement adviser display/input, no simulation mutation, manual founding, persistent travel and v7 round trip (API doubles)");
 }
}
