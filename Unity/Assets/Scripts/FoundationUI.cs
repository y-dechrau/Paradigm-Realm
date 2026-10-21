using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    public partial class RealmGame
    {
        string renameText="Verdant Union",seedText="";
        int setupMap,setupSpeed=1,setupTrait;
        bool showEconomy,showIdentity,showRoster;
        void AtomicSave(string path)
        {
            if(!world.Valid())throw new Exception("Game state did not pass validation; existing save preserved.");
            string temporary=path+".tmp";
            File.WriteAllText(temporary,JsonUtility.ToJson(world,true));
            if(File.Exists(path))File.Replace(temporary,path,path+".bak");else File.Move(temporary,path);
        }
        void AutoSave()
        {
            try{AtomicSave(Path.Combine(Application.persistentDataPath,"earth-auto-"+(world.turn%3)+".json"));}
            catch(Exception e){world.Note("Autosave failed: "+e.Message);}
        }
        void LoadAuto()
        {
            try{
                var files=Directory.GetFiles(Application.persistentDataPath,"earth-auto-*.json").OrderByDescending(File.GetLastWriteTimeUtc);
                foreach(var file in files){try{var next=JsonUtility.FromJson<World>(File.ReadAllText(file));next.UpgradeSave();if(!next.Valid())continue;world=next;renameText=world.civilisationName;selected=world.units.Where(u=>u.owner==0).Select(u=>u.id).DefaultIfEmpty(-1).First();cityId=-1;hasSession=true;mainMenu=false;Center();status="Autosave restored.";return;}catch{}}
                status="No valid autosave available.";
            }catch(Exception e){status="Could not load autosave: "+e.Message;}
        }
        void DrawSetupOptions()
        {
            GUILayout.BeginArea(new Rect(402,300,470,320));
            GUILayout.Label("Map (48 x 30; three civilisations)",body);
            setupMap=GUILayout.SelectionGrid(setupMap,new[]{"Continents","Inland landmass"},2);
            GUILayout.Label("Game speed: research and construction",body);
            setupSpeed=GUILayout.SelectionGrid(setupSpeed,new[]{"Quick","Standard","Long"},3);
            GUILayout.Label("Trait",body);
            setupTrait=GUILayout.SelectionGrid(setupTrait,new[]{"Balanced","Builders +1 production","Scholars +1 science","Traders +1 gold/city"},2);
            GUILayout.Label("Seed (optional number for repeatable worlds)",body);seedText=GUILayout.TextField(seedText,11);
            GUILayout.Label("Earth / conquest / standard AI. Other realms and difficulty presets are planned.",small);
            GUILayout.EndArea();
        }
        void DrawCityManagement(City c)
        {
            GUILayout.Label("Happiness "+world.Happiness(c)+" | "+(world.Connected(c)?"Capital connected":"Isolated")+(c.occupation>0?" | Occupied: "+c.occupation:""),small);
            GUILayout.Label("Food stored "+c.food+" / "+(12+c.pop*6),small);
            c.focus=GUILayout.SelectionGrid(c.focus,new[]{"Balanced","Growth","Production","Science"},2);
            GUI.SetNextControlName("cityrename");c.name=GUILayout.TextField(c.name,32);
            if(c.build>=0){string reason=world.BuildReason(c,c.build);if(reason!="")GUILayout.Label("Paused: "+reason,small);
                int output=world.Yield(c)[1];GUILayout.Label("Approx. "+Math.Max(0,(world.Cost(c.build)-c.production+output-1)/output)+" turns; output may change.",small);}
            GUILayout.Label("Queue: "+(c.queue.Count==0?"empty":string.Join(" > ",c.queue.Select(World.BuildName).ToArray())),small);
            if(c.queue.Count>0&&Button("Remove last queued project"))c.queue.RemoveAt(c.queue.Count-1);
            if(c.build>=0&&Button("Cancel current project (lose its progress)")){c.production=0;c.build=c.queue.Count>0?c.queue[0]:-1;if(c.queue.Count>0)c.queue.RemoveAt(0);}
            GUILayout.Label("Add construction (maximum 8 queued)",body);
            foreach(int b in world.Choices(c))if(Button(World.BuildName(b)+" - "+world.Cost(b)+" production"))status=world.QueueBuild(c,b)?"Project queued.":"Already queued or queue full.";
            if(Button(showRoster?"Hide locked projects":"Show locked projects"))showRoster=!showRoster;
            if(showRoster)foreach(int b in Enumerable.Range(0,World.UnitNames.Length).Select(World.UnitBuild).Concat(World.BuildingIds)){string reason=world.BuildReason(c,b);if(reason!="")GUILayout.Label(World.BuildName(b)+": "+reason,small);}
        }
        void DrawFoundationPanel(Unit u)
        {
            if(u!=null){
                GUILayout.Label("Upkeep "+World.Upkeep[u.type]+" gold | Experience "+u.experience+" | Rank "+u.promotion,small);
                if(u.job>0)GUILayout.Label("Worker job: "+u.jobTurns+" turns remaining",body);
                if(Button(u.fortified?"Wake unit":"Fortify / heal")){world.CancelOrder(u);u.fortified=!u.fortified;}
                if(u.experience>=(u.promotion+1)*5&&u.promotion<3&&Button("Promote (+1 combat strength)"))world.Promote(u);
                int target=world.UpgradeTarget(u);if(target>=0&&Button("Upgrade to "+World.UnitNames[target]+" ("+((World.UnitCosts[target]-World.UnitCosts[u.type])*2)+" gold)"))status=world.Upgrade(u)?"Unit upgraded.":"Requires technology, connected resources, gold, movement and friendly territory.";
                if(Powerful(u)&&Button("Pillage enemy improvement"))status=world.Pillage(u)?"Enemy infrastructure pillaged.":"Requires movement and an improved enemy tile at war.";
                if(u.type==4||u.type==9)foreach(var e in world.RangedTargets(u)){if(Button("Fire at "+World.UnitNames[e.type]+" ["+e.x+","+e.y+"]"))status=world.Ranged(u,e.id)?"Ranged attack resolved.":"No movement or line of sight blocked.";}
                if(Button("Disband this unit permanently")){world.units.Remove(u);selected=-1;world.CheckVictory();GUIUtility.ExitGUI();}
            }
            GUILayout.Space(10);
            if(Button(showEconomy?"Hide economy":"Economy and resources"))showEconomy=!showEconomy;
            if(showEconomy){var b=world.Budget(0);GUILayout.Label("Income +"+b[0]+" | Upkeep -"+b[1]+" | Net "+b[2],body);if(b[2]<0)GUILayout.Label("Reserve covers approximately "+world.gold/(-b[2])+" turns.",small);
                GUILayout.Label("Tax policy",body);world.nations[0].tax=GUILayout.SelectionGrid(world.nations[0].tax,new[]{"Low","Normal","High"},3);
                GUILayout.Label("Low: less tax, +2 happiness. High: more tax, -2 happiness. First 4 gold of unit upkeep supported.",small);
                for(int r=2;r<World.ResourceNames.Length;r++)GUILayout.Label(World.ResourceNames[r]+": "+(world.HasResource(0,r)?"connected":"not connected"),small);
            }
            if(Button(showIdentity?"Hide identity":"Civilisation identity"))showIdentity=!showIdentity;
            if(showIdentity){GUI.SetNextControlName("rename");renameText=GUILayout.TextField(renameText,32);if(Button("Apply civilisation name"))world.civilisationName=World.CleanName(renameText);
                world.flag=GUILayout.SelectionGrid(world.flag,new[]{"Leaf","Star","Sun"},3);GUILayout.Label("Flag: "+new[]{"Leaf","Star","Sun"}[Mathf.Clamp(world.flag,0,2)],small);}
            if(Button("Restore latest valid autosave"))LoadAuto();
            GUILayout.BeginHorizontal();if(GUILayout.Button("Zoom -"))tilePixels=Mathf.Max(24,tilePixels-4);if(GUILayout.Button("Zoom +"))tilePixels=Mathf.Min(48,tilePixels+4);GUILayout.EndHorizontal();
        }
        bool Powerful(Unit u){return World.Power[u.type]>0;}
        void NavigationInput()
        {
            if(guide||confirmNew||pendingUnit!=-1||mainMenu)return;
            var e=Event.current;string focus=GUI.GetNameOfFocusedControl();if(focus=="rename"||focus=="cityrename")return;
            if(e.type==EventType.KeyDown){if(e.keyCode==KeyCode.W){mapScroll.y-=64;e.Use();}else if(e.keyCode==KeyCode.S){mapScroll.y+=64;e.Use();}else if(e.keyCode==KeyCode.A){mapScroll.x-=64;e.Use();}else if(e.keyCode==KeyCode.D){mapScroll.x+=64;e.Use();}else if(e.keyCode==KeyCode.F1){guide=true;e.Use();}}
            if(e.type==EventType.MouseDrag&&e.button==2&&new Rect(24,100,836,608).Contains(e.mousePosition)){mapScroll-=e.delta;e.Use();}
            mapScroll.x=Mathf.Clamp(mapScroll.x,0,Mathf.Max(0,World.Width*tilePixels-836));mapScroll.y=Mathf.Clamp(mapScroll.y,0,Mathf.Max(0,World.Height*tilePixels-608));
        }
    }
}
