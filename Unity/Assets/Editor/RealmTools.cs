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
        FoundationChecks.Run();
        var world=World.New(42,"Round trip");world.Found(world.units.First(u=>u.owner==0&&u.type==0));
        world.nations[0].tax=2;world.EndTurn();
        var worker=world.units.First(u=>u.owner==0&&u.type==1);worker.moves=4;world.Add(0,1,worker.x,worker.y);world.GroupWorkers(worker);world.tech[TechnologyCatalog.Index("wheel")]=true;world.StartJob(worker,1);
        var city=world.cities.First(c=>c.owner==0);world.tech[0]=true;world.tech[TechnologyCatalog.Index("pottery")]=true;world.ChooseProject(city,1,false);world.QueueBuild(city,1);world.ChooseProject(city,7,false);world.Produce(city,3);
        var json=JsonUtility.ToJson(world);
        var loaded=JsonUtility.FromJson<World>(json);loaded.UpgradeSave();
        Require(loaded.Valid(),"save round trip validity");
        var restoredCity=loaded.cities.First(c=>c.owner==0);Require(restoredCity.build==1&&restoredCity.queue.Count==1&&restoredCity.buildingProject==7&&restoredCity.buildingProduction==3,"both production lines survive saving");
        var restored=loaded.units.First(u=>u.id==worker.id);Require(loaded.WorkerTeam(restored).Count==2&&restored.job==1,"worker team and shared work survive saving");
        Require(JsonUtility.ToJson(loaded)==json,"save preserves complete state");
        var legacy=World.New(17);legacy.version=2;legacy.tech=new bool[6];legacy.tech[0]=true;
        legacy.UpgradeSave();Require(legacy.Valid()&&legacy.tech[0],"legacy migration retains technology");
        Debug.Log("PASS: foundation engine checks, Unity JSON round trip and legacy save migration.");
    }
}
