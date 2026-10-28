using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    public partial class RealmGame
    {
        bool pauseMenu,cityScreen,saveAsScreen,settingsScreen,pauseQuit,overwriteSave;
        bool tooltips=true;
        float zoomSensitivity=2,toastUntil;
        string toastText="",saveName="",saveError="",activeSavePath;
        Vector2 cityScroll,saveScroll;
        void ResetMapGesture(){dragUnit=-1;dragging=false;mapPointerDown=false;}
        void ClampMapScroll(){mapScroll.x=Mathf.Clamp(mapScroll.x,0,Mathf.Max(0,world.Width*tilePixels-836));mapScroll.y=Mathf.Clamp(mapScroll.y,0,Mathf.Max(0,world.Height*tilePixels-608));}
        void OpenCity(City c){cityId=c.id;selected=-1;cityScreen=true;cityScroll=Vector2.zero;cityMapScroll=new Vector2(Mathf.Max(0,(Math.Min(4,2+c.age/25)*2+1)*40-207),Mathf.Max(0,(Math.Min(4,2+c.age/25)*2+1)*40-147));builtScroll=Vector2.zero;unitQueueScroll=Vector2.zero;buildingQueueScroll=Vector2.zero;researchScreen=false;detailsScreen="";GUI.FocusControl(null);ResetMapGesture();GUIUtility.ExitGUI();}
        bool OrderTeam(Unit u,int x,int y){var team=world.WorkerTeam(u).ToList();bool ok=false;foreach(var member in team)ok=world.Order(member,x,y)||ok;status=ok?"Order issued. Travel continues each turn; new orders replace it.":"Cannot reach that destination.";return ok;}
        string UnitDescription(Unit u){return new[]{"Founds new cities","Builds roads and improvements","Basic close-combat infantry","Explores distant territory","Ranged infantry","Armoured mounted troops","Gunpowder infantry","Counters mounted troops","Fast mounted troops","Siege artillery","Helps nearby units heal","Establishes trade routes and supplies wonders"}[u.type];}
        void HandleGlobalInput(){var e=Event.current;if(e.type!=EventType.KeyDown)return;
            if(e.keyCode==KeyCode.S&&e.control){Save();e.Use();return;}
            if(e.keyCode!=KeyCode.Escape)return;
            if(pauseQuit)pauseQuit=false;
            else if(overwriteSave)overwriteSave=false;
            else if(saveAsScreen||settingsScreen){PersistSettings();saveAsScreen=false;settingsScreen=false;}
            else if(pauseMenu)pauseMenu=false;
            else if(researchScreen||detailsScreen!=""){researchScreen=false;detailsScreen="";}
            else {guide=false;confirmNew=false;pendingUnit=-1;pauseMenu=true;}
            ResetMapGesture();GUI.FocusControl(null);e.Use();GUIUtility.ExitGUI();
        }
        bool SaveTo(string path){try{AtomicSave(path);activeSavePath=path;status="";toastText="Game Saved";toastUntil=Time.realtimeSinceStartup+2f;saveError="";return true;}
            catch(Exception e){status="Could not save: "+e.Message;saveError=status;toastText="Save failed";toastUntil=Time.realtimeSinceStartup+4f;return false;}}
        void DrawSaveToast(){float remaining=toastUntil-Time.realtimeSinceStartup;if(remaining<=0)return;var old=GUI.color;GUI.color=new Color(1,1,1,Mathf.Clamp01(remaining));GUI.Box(new Rect(490,745,300,42),toastText);GUI.color=old;}
        string NamedSavePath(){var safe=new string(saveName.Where(c=>char.IsLetterOrDigit(c)||c==' '||c=='-'||c=='_').ToArray()).Trim();return safe.Length==0?null:Path.Combine(Application.persistentDataPath,"earth-named-"+safe+".json");}
        void PersistSettings(){PlayerPrefs.SetFloat("RealmZoomSensitivity",zoomSensitivity);PlayerPrefs.SetInt("RealmTooltips",tooltips?1:0);PlayerPrefs.Save();}
        void DrawPauseMenu(){Fill(new Rect(0,0,1280,800),new Color(0,0,0,.7f));
            if(pauseQuit){GUI.Box(new Rect(420,280,440,220),"");GUILayout.BeginArea(new Rect(450,300,380,180));GUILayout.Label("Are you sure?",heading);GUILayout.Label("Quit Paradigm Realm? Unsaved progress will be lost.",body);if(Button("Yes"))QuitGame();if(Button("No"))pauseQuit=false;GUILayout.EndArea();return;}
            if(saveAsScreen){DrawSaveAs();return;}
            if(settingsScreen){GUI.Box(new Rect(410,230,460,330),"");GUILayout.BeginArea(new Rect(440,250,400,280));GUILayout.Label("Settings",heading);GUILayout.Label("Mouse-wheel zoom sensitivity",body);zoomSensitivity=GUILayout.HorizontalSlider(zoomSensitivity,.5f,5f);tooltips=GUILayout.Toggle(tooltips,"Show map and unit tooltips");GUILayout.Label("Settings are saved on this computer.",small);if(Button("Back")){PersistSettings();settingsScreen=false;}GUILayout.EndArea();return;}
            GUI.Box(new Rect(440,205,400,390),"");GUILayout.BeginArea(new Rect(470,225,340,350));GUILayout.Label("Game menu",heading);
            if(Button("Quick Save [Ctrl+S]"))Save();
            if(Button("Save As")){saveName=world.civilisationName+" - Turn "+world.turn;saveError="";overwriteSave=false;saveAsScreen=true;}
            if(Button("Continue")){pauseMenu=false;GUI.FocusControl(null);}
            if(Button("Settings"))settingsScreen=true;
            if(Button("Quit"))pauseQuit=true;
            if(status.StartsWith("Could not save"))GUILayout.Label(status,small);
            GUILayout.EndArea();
        }
        void DrawSaveAs(){GUI.Box(new Rect(330,140,620,520),"");GUILayout.BeginArea(new Rect(360,165,560,470));GUILayout.Label("Save As",heading);
            if(overwriteSave){GUILayout.Label("Replace the existing save named "+saveName+"?",body);if(Button("Yes, replace")){if(SaveTo(NamedSavePath())){saveAsScreen=false;overwriteSave=false;}}if(Button("No"))overwriteSave=false;}
            else {GUILayout.Label("Save name (letters, numbers, spaces, hyphens and underscores)",small);GUI.SetNextControlName("saveName");saveName=GUILayout.TextField(saveName,64);
                if(Button("Save")){string path=NamedSavePath();if(path==null)saveError="Enter a save name.";else if(File.Exists(path))overwriteSave=true;else if(SaveTo(path))saveAsScreen=false;}
                GUILayout.Label("Saved games — select one to load",body);saveScroll=GUILayout.BeginScrollView(saveScroll,GUILayout.Height(220));
                string[] files=new string[0];try{files=Directory.GetFiles(Application.persistentDataPath,"earth-named-*.json").OrderByDescending(File.GetLastWriteTimeUtc).ToArray();}catch(Exception e){saveError=e.Message;}
                foreach(var file in files)if(Button(Path.GetFileNameWithoutExtension(file).Substring("earth-named-".Length))){LoadFile(file);if(!status.StartsWith("Could not load")){saveAsScreen=false;pauseMenu=false;}}
                GUILayout.EndScrollView();if(Button("Back"))saveAsScreen=false;
            }
            if(saveError!="")GUILayout.Label(saveError,small);GUILayout.EndArea();
        }
        string renameText="Verdant Union",seedText="";
        int setupMap,setupSpeed=1,setupTrait,setupSize=1,setupShape=1;
        
        void AtomicSave(string path)
        {
            if(!world.Valid())throw new Exception("Game state did not pass validation; existing save preserved.");
            string temporary=path+".tmp";
            File.WriteAllText(temporary,JsonUtility.ToJson(world,true));
            if(File.Exists(path))File.Replace(temporary,path,path+".bak");else File.Move(temporary,path);
        }
        void AutoSave()
        {
            if(world.tiles.Length>9000&&world.turn%5!=0)return;
            try{AtomicSave(Path.Combine(Application.persistentDataPath,"earth-auto-"+(world.turn%3)+".json"));}
            catch(Exception e){world.Note("Autosave failed: "+e.Message);}
        }
        void LoadAuto()
        {
            try{
                var files=Directory.GetFiles(Application.persistentDataPath,"earth-auto-*.json").OrderByDescending(File.GetLastWriteTimeUtc);
                foreach(var file in files){try{var next=JsonUtility.FromJson<World>(File.ReadAllText(file));next.UpgradeSave();if(!next.Valid())continue;world=next;renameText=world.civilisationName;selected=world.units.Where(u=>u.owner==0).Select(u=>u.id).DefaultIfEmpty(-1).First();cityId=-1;cityScreen=false;hasSession=true;mainMenu=false;Center();status="Autosave restored.";return;}catch{}}
                status="No valid autosave available.";
            }catch(Exception e){status="Could not load autosave: "+e.Message;}
        }
        void DrawSetupOptions()
        {
            GUILayout.BeginArea(new Rect(402,292,470,346));
            GUILayout.Label("World: "+(setupShape==1?"Sphere (wraps east-west, routes cross the poles)":"Flat map with edges (old style)"),body);
            setupShape=GUILayout.SelectionGrid(setupShape,new[]{"Flat","Sphere"},2);
            GUILayout.Label("Map size: "+World.MapSizeNames[setupSize]+" ("+(setupShape==1?World.SphereWidths[setupSize]:World.MapWidths[setupSize])+" x "+(setupShape==1?World.SphereHeights[setupSize]:World.MapHeights[setupSize])+" tiles)",body);
            setupSize=GUILayout.SelectionGrid(setupSize,World.MapSizeNames,6);
            GUILayout.Label("Map shape",body);
            setupMap=GUILayout.SelectionGrid(setupMap,new[]{"Continents","Inland landmass"},2);
            GUILayout.Label("Game speed: research and construction",body);
            setupSpeed=GUILayout.SelectionGrid(setupSpeed,new[]{"Quick","Standard","Long"},3);
            GUILayout.Label("Trait",body);
            setupTrait=GUILayout.SelectionGrid(setupTrait,new[]{"Balanced","Builders +1 production","Scholars +1 science","Traders +1 gold/city"},2);
            GUILayout.Label("Seed (optional number for repeatable worlds)",body);seedText=GUILayout.TextField(seedText,11);
            GUILayout.Label("Earth / conquest / standard AI. Larger maps still have three civilisations for now; more are planned.",small);
            GUILayout.EndArea();
        }
        void DrawFoundationPanel(Unit u)
        {
            if(u!=null){
                DrawEmpireUnit(u);
                GUILayout.Label("Upkeep "+World.Upkeep[u.type]+" gold | Experience "+u.experience+" | Rank "+u.promotion,small);
                GUILayout.Label("Activity: "+world.Activity(u)+" | "+world.Remaining(u),body);
                if(u.type==1){GUILayout.Label("Worker team: "+world.WorkerTeam(u).Count,small);
                    if(u.workerGroup>0){if(Button("Ungroup workers (stop current work)")){world.UngroupWorkers(u);status="Workers ungrouped; current work cancelled.";}}
                    if(world.units.Count(a=>a.owner==u.owner&&a.type==1&&a.x==u.x&&a.y==u.y)>1&&Button("Group all workers here (restart work)")){world.GroupWorkers(u);status="Workers grouped. Choose their shared activity.";}}
                if((u.job>0||u.hasOrder||u.fortified)&&Button("Cancel activity / route [X]")){world.CancelActivity(u);status="Activity and route cancelled.";}
                if(Button(u.fortified?"Wake unit":"Fortify / heal")){bool fortify=!u.fortified;world.CancelActivity(u);foreach(var member in world.WorkerTeam(u))member.fortified=fortify;}
                if(u.experience>=(u.promotion+1)*5&&u.promotion<3&&Button("Promote (+1 combat strength)"))world.Promote(u);
                int target=world.UpgradeTarget(u);if(target>=0&&Button("Upgrade to "+World.UnitNames[target]+" ("+((World.UnitCosts[target]-World.UnitCosts[u.type])*2)+" gold)"))status=world.Upgrade(u)?"Unit upgraded.":"Requires technology, connected resources, gold, movement and a city owned by your civilisation.";
                if(Powerful(u)&&Button("Pillage enemy improvement"))status=world.Pillage(u)?"Enemy infrastructure pillaged.":"Requires movement and an improved enemy tile at war.";
                if(u.type==4||u.type==9)foreach(var e in world.RangedTargets(u)){if(Button("Fire at "+World.UnitNames[e.type]+" ["+e.x+","+e.y+"]"))status=world.Ranged(u,e.id)?"Ranged attack resolved.":"No movement or line of sight blocked.";}
                if(Button("Disband this unit permanently")){world.units.Remove(u);selected=-1;world.CheckVictory();GUIUtility.ExitGUI();}
            }
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();if(GUILayout.Button("Zoom -"))tilePixels=Mathf.Max(24,tilePixels-8);if(GUILayout.Button("Zoom +"))tilePixels=Mathf.Min(96,tilePixels+8);GUILayout.EndHorizontal();
        }
        bool Powerful(Unit u){return World.Power[u.type]>0;}
        void NavigationInput()
        {
            if(guide||confirmNew||pendingUnit!=-1||mainMenu||pauseMenu||cityScreen||researchScreen||detailsScreen!=""||loadScreen)return;
            var e=Event.current;
            // Consume the wheel before BeginScrollView can turn it into vertical panning.
            if(e.type==EventType.ScrollWheel&&new Rect(24,100,836,608).Contains(e.mousePosition)){
                float old=tilePixels;tilePixels=Mathf.Clamp(tilePixels-e.delta.y*zoomSensitivity,24,96);
                var point=e.mousePosition-new Vector2(24,100);
                mapScroll=(mapScroll+point)*(tilePixels/old)-point;ClampMapScroll();ResetMapGesture();e.Use();
            }
            string focus=GUI.GetNameOfFocusedControl();if(focus=="rename"||focus=="cityrename")return;
            if(e.type==EventType.KeyDown&&!e.control&&!e.alt&&!e.command){if(e.keyCode==KeyCode.W){mapScroll.y-=64;e.Use();}else if(e.keyCode==KeyCode.S){mapScroll.y+=64;e.Use();}else if(e.keyCode==KeyCode.A){mapScroll.x-=64;e.Use();}else if(e.keyCode==KeyCode.D){mapScroll.x+=64;e.Use();}else if(e.keyCode==KeyCode.F1){guide=true;e.Use();}}
            if(e.type==EventType.MouseDrag&&e.button==2&&new Rect(24,100,836,608).Contains(e.mousePosition)){mapScroll-=e.delta;e.Use();}
            ClampMapScroll();
        }
    }
}
