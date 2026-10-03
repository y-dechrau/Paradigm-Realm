using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ParadigmRealm;

public static class RealmTools
{
    const string ScenePath="Assets/Scenes/Earth.unity";
    [MenuItem("Paradigm Realm/Open Earth Scene")]
    public static void OpenScene(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ScenePath);}
    [MenuItem("Paradigm Realm/Build Windows Game")]
    public static void BuildWindows()
    {
        string folder=EditorUtility.OpenFolderPanel("Choose build folder", "", "");if(string.IsNullOrEmpty(folder))return;
        var report=BuildPipeline.BuildPlayer(new[]{ScenePath},System.IO.Path.Combine(folder,"ParadigmRealm.exe"),BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed. Check Console and install Windows Build Support in Unity Hub.");
        EditorUtility.RevealInFinder(System.IO.Path.Combine(folder,"ParadigmRealm.exe"));
    }
    static void Require(bool condition,string name){if(!condition)throw new Exception("FAIL: "+name);}
    [MenuItem("Paradigm Realm/Run Engine Checks")]
    public static void Checks()
    {
        var w=World.New(42);Require(w.Valid(),"new world valid");Require(w.tiles.Length==1440,"square map dimensions");
        Require(JsonUtility.ToJson(w)==JsonUtility.ToJson(World.New(42)),"deterministic seed");
        var settler=w.units.First(u=>u.owner==0&&u.type==0);var city=w.Found(settler);Require(city!=null&&!w.units.Contains(settler),"founding consumes settler");
        var worker=w.units.First(u=>u.owner==0&&u.type==1);Require(w.Improve(worker,false)&&worker.moves==0,"road consumes movement");Require(!w.Improve(worker,true),"farm locked");
        var soldier=w.units.First(u=>u.owner==0&&u.type==2);int x=soldier.x+1,y=soldier.y;w.At(x,y).terrain=0;Require(w.Move(soldier,x,y)&&soldier.moves==2,"terrain movement cost");Require(!w.Move(soldier,x+3,y),"reject teleport");
        var enemy=w.Add(1,2,x,y+1);w.At(x,y+1).terrain=0;Require(!w.Move(soldier,x,y+1),"peace blocks combat");w.war[1]=true;Require(w.Move(soldier,x,y+1)&&enemy.hp<10,"combat at war");w.war[1]=false;
        for(int i=0;i<130;i++)w.EndTurn();Require(w.tech[5],"research reaches Electricity");Require(w.Valid(),"state valid after 130 turns");
        var roundtrip=JsonUtility.FromJson<World>(JsonUtility.ToJson(w));Require(roundtrip.Valid()&&JsonUtility.ToJson(w)==JsonUtility.ToJson(roundtrip),"save round trip");
        w.units.RemoveAll(u=>u.owner>0);w.cities.RemoveAll(c=>c.owner>0);w.CheckVictory();Require(w.result.StartsWith("Conquest"),"conquest victory");
        roundtrip.units[0].type=99;Require(!roundtrip.Valid(),"reject malformed unit");
        Debug.Log("PASS: world generation, founding, movement, improvements, combat, 130 turns, research, save round trip, victory, malformed save checks.");
    }
}
