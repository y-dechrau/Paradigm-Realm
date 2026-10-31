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
    // Batch-mode build for testing without the folder picker: Unity -batchmode -executeMethod RealmTools.BuildWindowsBatch -buildPath <exe>
    public static void BuildWindowsBatch()
    {
        var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-buildPath");
        string path=i>=0&&i+1<args.Length?args[i+1]:System.IO.Path.Combine("Builds","ParadigmRealm.exe");
        var report=BuildPipeline.BuildPlayer(new[]{ScenePath},path,BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
    }
    [MenuItem("Paradigm Realm/Run Engine Checks")]
    public static void Checks()
    {
        DiplomacyChecks.Run();FoundationChecks.Run();ContentChecks.Run();EmpireChecks.Run();StrategyChecks.Run();SettlementChecks.Run();
        var world=World.New(42,"Round trip");world.Found(world.units.First(u=>u.owner==0&&u.type==0));
        world.SetScienceRate(0,30);world.EndTurn();
        var worker=world.units.First(u=>u.owner==0&&u.type==1);worker.moves=4;world.Add(0,1,worker.x,worker.y);world.GroupWorkers(worker);world.tech[TechnologyCatalog.Index("wheel")]=true;world.StartJob(worker,1);
        var city=world.cities.First(c=>c.owner==0);world.tech[TechnologyCatalog.Index("agriculture")]=true;world.tech[TechnologyCatalog.Index("pottery")]=true;world.ChooseProject(city,1,false);world.QueueBuild(city,1);world.ChooseProject(city,7,false);world.Produce(city,3);
        world.met[1]=world.met[2]=true;world.gold=100;world.nations[1].gold=100;world.Propose(new DiplomaticOffer{from=1,to=0,kind=TreatyKind.Access});world.Propose(new DiplomaticOffer{from=0,to=2,kind=TreatyKind.Trade,givePerTurn=1});
        var json=JsonUtility.ToJson(world);
        var loaded=JsonUtility.FromJson<World>(json);loaded.UpgradeSave();
        Require(loaded.Valid(),"save round trip validity");
        var restoredCity=loaded.cities.First(c=>c.owner==0);Require(restoredCity.build==1&&restoredCity.queue.Count==1&&restoredCity.buildingProject==7&&restoredCity.buildingProduction==3,"both production lines survive saving");
        var restored=loaded.units.First(u=>u.id==worker.id);Require(loaded.WorkerTeam(restored).Count==2&&restored.job==1,"worker team and shared work survive saving");
        Require(JsonUtility.ToJson(loaded)==json,"save preserves complete state");
        var legacy=World.New(17,natives:false);legacy.version=2;legacy.tech=new bool[6];legacy.tech[TechnologyCatalog.Index("agriculture")]=true;
        legacy.UpgradeSave();Require(legacy.Valid()&&legacy.tech[TechnologyCatalog.Index("agriculture")],"legacy migration retains technology");
        Debug.Log("PASS: foundation engine checks, Unity JSON round trip and legacy save migration.");
    }
}
