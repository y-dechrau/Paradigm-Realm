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
        // On a sphere the classic view pans east or west for ever: the scroll position wraps round the world width (owner request).
        // The classic view scrolls only over the rows of the world being viewed (home or a parallel world).
        void ClampViewRows(){float top=world.RealmTop(viewRealm)*ClassicRowHeight(),bottom=world.RealmBottom(viewRealm)*ClassicRowHeight()+(world.Staggered?tilePixels*.5f:0);mapScroll.y=Mathf.Clamp(mapScroll.y,top,Mathf.Max(top,bottom-MapH));}
        void ClampMapScroll(){if(world.Sphere){float wpx=world.Width*tilePixels;mapScroll.x=((mapScroll.x%wpx)+wpx)%wpx;ClampViewRows();return;}mapScroll.x=Mathf.Clamp(mapScroll.x,0,Mathf.Max(0,world.Width*tilePixels-MapW));ClampViewRows();}
        void OpenCity(City c){if(!cityScreen)cityOrigin=c.id;tutorialCitySeen=true;cityId=c.id;selected=-1;cityScreen=true;cityScroll=Vector2.zero;cityMapScroll=Vector2.zero;builtScroll=Vector2.zero;unitQueueScroll=Vector2.zero;buildingQueueScroll=Vector2.zero;researchScreen=false;detailsScreen="";GUI.FocusControl(null);ResetMapGesture();GUIUtility.ExitGUI();}
        bool OrderTeam(Unit u,int x,int y){var team=world.WorkerTeam(u).ToList();int sx=u.x,sy=u.y;bool ok=false;foreach(var member in team)ok=world.Order(member,x,y)||ok;status=ok?"Order issued. Travel continues each turn; new orders replace it.":"Cannot reach that destination.";
            // An order that cannot take its first step says why, rather than leaving the unit standing there in silence.
            if(ok&&u.hasOrder&&u.x==sx&&u.y==sy&&u.moves>0){var path=world.PlannedRoute(u);if(path.Count>0){string why=world.MoveProblem(u,path[0].x,path[0].y);if(why!="")status="Order given, but it cannot move yet: "+why;}}
            return ok;}
        string UnitDescription(Unit u){return new[]{"Founds new cities","Builds roads and improvements","Basic close-combat infantry","Explores distant territory","Ranged infantry","Armoured mounted troops","Gunpowder infantry","Counters mounted troops","Fast mounted troops","Siege artillery","Helps nearby units heal","Establishes trade routes and supplies wonders","Explores coasts and ferries one land unit","Carries land units across the sea","Fights at sea and bombards coasts","Preaches its faith in cities (2 sermons), keeps a city content and heals nearby units"}[u.type];}
        void HandleGlobalInput(){var e=Event.current;if(e.type!=EventType.KeyDown||turnRunning)return;
            if(e.keyCode==KeyCode.S&&e.control){Save();e.Use();return;}
            if(e.keyCode!=KeyCode.Escape)return;
            if(foundStage>0)foundStage=0;
            else if(tilePicker)tilePicker=false;
            else if(pauseQuit)pauseQuit=false;
            else if(overwriteSave)overwriteSave=false;
            else if(saveAsScreen||settingsScreen){PersistSettings();saveAsScreen=false;settingsScreen=false;}
            else if(pauseMenu)pauseMenu=false;
            else if(legendsOpen){legendsOpen=false;legendsPeople=-1;civLoreOpen=false;}else if(researchScreen||detailsScreen!=""){researchScreen=false;detailsScreen="";}
            // Esc first lets go of the selected unit (and any pending move); with nothing selected it opens the pause menu.
            else if(gateScene!=null)gateScene=null;else if(gatePanel!=null)gatePanel=null;
            else if(roadMode){roadMode=false;roadBends.Clear();status="Road plan cancelled.";}
            else if(!cityScreen&&(selected>=0||moveMode)){selected=-1;moveMode=false;moreActions=false;status="";}
            else {guide=false;confirmNew=false;pendingUnit=-1;pauseMenu=true;}
            ResetMapGesture();GUI.FocusControl(null);e.Use();GUIUtility.ExitGUI();
        }
        bool SaveTo(string path){try{AtomicSave(path);activeSavePath=path;status="";toastText="Game saved";toastUntil=Time.realtimeSinceStartup+2f;saveError="";return true;}
            catch(Exception e){status="Could not save: "+e.Message;saveError=status;toastText="Save failed";toastUntil=Time.realtimeSinceStartup+4f;return false;}}
        void DrawSaveToast(){float remaining=toastUntil-Time.realtimeSinceStartup;if(remaining<=0)return;var old=GUI.color;GUI.color=new Color(1,1,1,Mathf.Clamp01(remaining));var plate=new Rect(530,50,220,34);DrawGuideFrame(plate);GUI.Label(plate,toastText,hudCentre);GUI.color=old;}
        string NamedSavePath(){var safe=new string(saveName.Where(c=>char.IsLetterOrDigit(c)||c==' '||c=='-'||c=='_').ToArray()).Trim();return safe.Length==0?null:Path.Combine(Application.persistentDataPath,"earth-named-"+safe+".json");}
        void PersistSettings(){SaveTutorialProgress();PlayerPrefs.SetFloat("RealmZoomSensitivity",zoomSensitivity);PlayerPrefs.SetInt("RealmTooltips",tooltips?1:0);PlayerPrefs.Save();}
        void DrawPauseMenu(){Fill(new Rect(0,0,1280,800),UiDim);
            if(pauseQuit){GUI.Box(new Rect(420,280,440,220),"");GUILayout.BeginArea(new Rect(450,300,380,180));GUILayout.Label("Are you sure?",heading);GUILayout.Label("Quit Paradigm Realm? Unsaved progress will be lost.",body);if(Button("Yes"))QuitGame();if(Button("No"))pauseQuit=false;GUILayout.EndArea();return;}
            if(saveAsScreen){DrawSaveAs();return;}
            if(settingsScreen){GUI.Box(new Rect(410,200,460,400),"");GUILayout.BeginArea(new Rect(440,220,400,350));GUILayout.Label("Settings",heading);GUILayout.Label("Mouse-wheel zoom sensitivity",body);zoomSensitivity=GUILayout.HorizontalSlider(zoomSensitivity,.5f,5f);tooltips=GUILayout.Toggle(tooltips,"Show map and unit tooltips");tutorialOn=GUILayout.Toggle(tutorialOn,"Tutorial with "+GuideName);if(world!=null&&Button("Restart the tutorial")){RestartTutorial();settingsScreen=false;pauseMenu=false;GUIUtility.ExitGUI();}GUILayout.Label("Settings are saved on this computer.",small);if(Button("Back")){PersistSettings();settingsScreen=false;}GUILayout.EndArea();return;}
            GUI.Box(new Rect(440,180,400,440),"");GUILayout.BeginArea(new Rect(470,200,340,400));GUILayout.Label("Game menu",heading);
            if(Button("Continue")){pauseMenu=false;GUI.FocusControl(null);}
            if(Button("Quick Save [Ctrl+S]"))Save();
            if(Button("Save As")){saveName=world.civilisationName+" - Turn "+world.turn;saveError="";overwriteSave=false;saveAsScreen=true;}
            if(Button("Load Game"))OpenLoadScreen();
            if(Button("Settings"))settingsScreen=true;
            // Back to the title screen; the game stays open there under Resume Game (owner request).
            if(Button("Return to Main Menu")){pauseMenu=false;mainMenu=true;GUI.FocusControl(null);GUIUtility.ExitGUI();}
            if(Button("Quit"))pauseQuit=true;
            if(status.StartsWith("Could not save"))GUILayout.Label(status,small);
            GUILayout.EndArea();
        }
        void DrawSaveAs(){GUI.Box(new Rect(330,140,620,520),"");GUILayout.BeginArea(new Rect(360,165,560,470));GUILayout.Label("Save As",heading);
            if(overwriteSave){GUILayout.Label("Are you sure you want to save over "+saveName+"?",body);if(Button("Yes, save over it")){if(SaveTo(NamedSavePath())){saveAsScreen=false;overwriteSave=false;}}if(Button("No"))overwriteSave=false;}
            else {GUILayout.Label("Save name (letters, numbers, spaces, hyphens and underscores)",small);GUI.SetNextControlName("saveName");saveName=GUILayout.TextField(saveName,64);
                if(Button("Save")){string path=NamedSavePath();if(path==null)saveError="Enter a save name.";else if(File.Exists(path))overwriteSave=true;else if(SaveTo(path))saveAsScreen=false;}
                GUILayout.Label("Saved games — select one to save over it",body);saveScroll=GUILayout.BeginScrollView(saveScroll,GUILayout.Height(220));
                string[] files=new string[0];try{files=Directory.GetFiles(Application.persistentDataPath,"earth-named-*.json").OrderByDescending(File.GetLastWriteTimeUtc).ToArray();}catch(Exception e){saveError=e.Message;}
                foreach(var file in files)if(Button(Path.GetFileNameWithoutExtension(file).Substring("earth-named-".Length))){saveName=Path.GetFileNameWithoutExtension(file).Substring("earth-named-".Length);saveError="";overwriteSave=true;}
                GUILayout.EndScrollView();if(Button("Back"))saveAsScreen=false;
            }
            if(saveError!="")GUILayout.Label(saveError,small);GUILayout.EndArea();
        }
        string renameText="Verdant Union",seedText="";
        int setupClimate,setupAnimals=3,setupMap,setupSpeed=1,setupTrait,setupSize=1,setupShape=1,setupCivs=3;
        
        void AtomicSave(string path)
        {
            if(!world.Valid())throw new Exception("Game state did not pass validation; existing save preserved.");
            string temporary=path+".tmp";
            // Compact JSON: with the parallel worlds a map is six times the size, so saves are written without indentation.
            File.WriteAllText(temporary,JsonUtility.ToJson(world,false));
            if(File.Exists(path))File.Replace(temporary,path,path+".bak");else File.Move(temporary,path);
        }
        // Autosave runs on another thread, so the map keeps moving; the turn is not handed back until it has finished.
        bool AutoSaveDue(){return world.tiles.Length>60000?world.turn%10==0:world.tiles.Length<=9000||world.turn%5==0;}
        void LoadAuto()
        {
            try{
                var files=Directory.GetFiles(Application.persistentDataPath,"earth-auto-*.json").OrderByDescending(File.GetLastWriteTimeUtc);
                foreach(var file in files){try{var next=JsonUtility.FromJson<World>(File.ReadAllText(file));next.UpgradeSave();if(!next.Valid())continue;world=next;renameText=world.civilisationName;selected=world.units.Where(u=>u.owner==0).Select(u=>u.id).DefaultIfEmpty(-1).First();cityId=-1;cityScreen=false;hasSession=true;mainMenu=false;Center();status="Autosave restored.";return;}catch{}}
                status="No valid autosave available.";
            }catch(Exception e){status="Could not load autosave: "+e.Message;}
        }
        static readonly string[] SpeedTexts={"Quick: research takes a third of a Long game (4 turns or more a technology); building costs half.","Standard: research takes two thirds of a Long game (7 turns or more).","Long: the slowest research (10 turns or more a technology); building costs half as much again."};
        void DrawSetupOptions()
        {
            GUILayout.BeginArea(new Rect(88,128,342,496));
            GUILayout.Label("Your game",heading);
            GUILayout.Label("Playing as "+(setupCiv<CivilisationCatalog.Count?CivilisationCatalog.Names[setupCiv]:proposedName)+": "+CivilisationCatalog.TraitLine(setupCiv<CivilisationCatalog.Count?CivilisationCatalog.Traits[setupCiv]:new[]{customTraitA,customTraitB}),small);
            GUILayout.Space(10);GUILayout.Label("Game speed",heading);
            setupSpeed=GUILayout.SelectionGrid(setupSpeed,new[]{"Quick","Standard","Long"},3);
            GUILayout.Label(SpeedTexts[Mathf.Clamp(setupSpeed,0,2)],small);
            GUILayout.Space(10);GUILayout.Label("Seed (optional)",heading);seedText=GUILayout.TextField(seedText,11);
            GUILayout.Label("The same number and settings make the same world again.",small);
            GUILayout.Space(10);GUILayout.Label("Difficulty: "+DifficultyTable.Names[setupLevel],heading);setupLevel=GUILayout.SelectionGrid(setupLevel,DifficultyTable.Names,4);GUILayout.Label(DifficultyTable.Describe(setupLevel),small);
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(469,128,342,496));
            GUILayout.Label("Your world",heading);
            GUILayout.Label("Shape",body);setupShape=GUILayout.SelectionGrid(setupShape,new[]{"Flat","Sphere"},2);
            GUILayout.Label(setupShape==1?"A globe: travel wraps east to west and routes can cross the poles.":"A flat map with edges, as in older versions.",small);
            GUILayout.Space(6);GUILayout.Label("Size",body);setupSize=GUILayout.SelectionGrid(setupSize,World.MapSizeNames,3);
            {int hexes=World.HexTiles(setupShape==1?World.SphereWidths[setupSize]*World.SphereHeights[setupSize]:World.MapWidths[setupSize]*World.MapHeights[setupSize]),across=World.HexColumns(hexes);GUILayout.Label(across+" x "+hexes/across+" hexes. Bigger worlds take longer turns.",small);}
            GUILayout.Space(6);GUILayout.Label("Land",body);setupMap=GUILayout.SelectionGrid(setupMap,World.LandTypeNames,3);GUILayout.Label(World.LandTypeTexts[setupMap],small);
            GUILayout.Space(6);GUILayout.Label("Climate",body);setupClimate=GUILayout.SelectionGrid(setupClimate,World.ClimateNames,3);GUILayout.Label(World.ClimateTexts[setupClimate],small);
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(850,128,342,496));
            GUILayout.Label("Who shares it",heading);
            int maxCivs=World.MaxCivsBySize[setupSize];if(setupCivs>maxCivs)setupCivs=maxCivs;if(setupCivs<2)setupCivs=2;
            GUILayout.Space(6);GUILayout.Label("Civilisations: "+setupCivs+" (you and "+(setupCivs-1)+" rival"+(setupCivs==2?"":"s")+"; up to "+maxCivs+" on this size), plus "+(setupPeoples*(World.RealmCount-1)+Math.Min(setupPeoples,SealedPeoples.HivePeoples))+" peoples in the sealed worlds",body);
            setupCivs=Mathf.RoundToInt(GUILayout.HorizontalSlider(setupCivs,2,maxCivs));
            GUILayout.Space(6);GUILayout.Label("Peoples in each sealed world: "+setupPeoples+(setupPeoples>4?" (slower turns on large maps)":""),body);setupPeoples=Mathf.RoundToInt(GUILayout.HorizontalSlider(setupPeoples,1,10));
            GUILayout.Space(10);GUILayout.Label("Wild animals: "+World.AnimalChoices[setupAnimals],body);setupAnimals=GUILayout.SelectionGrid(setupAnimals,World.AnimalChoices,5);GUILayout.Label(setupAnimals==0?"No wild animals at all.":"Three times as many until a civilisation passes the Medieval Age.",small);
            GUILayout.Space(10);tutorialOn=GUILayout.Toggle(tutorialOn," Play with the tutorial");GUILayout.Label(GuideName+" talks you through the first turns.",small);
            GUILayout.EndArea();
        }
        string SetupSummary(){return (setupCiv<CivilisationCatalog.Count?CivilisationCatalog.Names[setupCiv]+" ("+CivilisationCatalog.Leaders[setupCiv]+")":(string.IsNullOrWhiteSpace(proposedName)?"Your civilisation":proposedName.Trim()))+" on a "+World.MapSizeNames[setupSize].ToLower()+" "+(setupShape==1?"sphere":"flat map")+" with "+(setupCivs-1)+" rival"+(setupCivs==2?"":"s")+", "+new[]{"quick","standard","long"}[Mathf.Clamp(setupSpeed,0,2)]+" speed.";}        void DrawFoundationPanel(Unit u)
        {
            if(u!=null){
                DrawEmpireUnit(u);
                GUILayout.Label((World.Power[u.type]>0?"Support "+World.SupportCost(u.type)+" gold a turn":"Support 1 gold beyond the free allowance")+" | Eats "+(World.UnitRation(u.type)==2?"1 food":"half a food")+" a turn | "+World.RankText(u),small);
                bool cityWorker=world.WorkerInCity(u),wasOn=GUI.enabled;if(cityWorker){GUILayout.Label("In a city a worker cannot develop the tile itself: it can auto develop the land around, cancel, fortify or heal, keep sentry, move out or be disbanded.",small);GUI.enabled=false;}
                if(world.SameTypeHere(u)>=2&&world.UnitsToStack(u).Count>world.SameTypeHere(u)&&Button("Take all"+world.SameTypeHere(u)+" "+World.UnitNames[u.type]+" units here as one stack")){world.JoinType(u);status="All the "+World.UnitNames[u.type]+" units here now move together.";}
                if(!World.IsNaval(u.type)&&world.UnitsToStack(u).Count>1){if(u.stack==0&&Button("Stack the units on this tile [G]"))world.JoinStack(u);else if(u.stack!=0&&Button("Break up this stack [G]"))world.LeaveStack(u);}
                GUILayout.Label("Activity: "+world.Activity(u)+" | "+world.Remaining(u),body);
                if(u.type==1){GUILayout.Label("Worker team: "+world.WorkerTeam(u).Count,small);
                    if(u.workerGroup>0){if(Button("Ungroup workers (stop current work)")){world.UngroupWorkers(u);status="Workers ungrouped; current work cancelled.";}}
                    if(world.units.Count(a=>a.owner==u.owner&&a.type==1&&a.x==u.x&&a.y==u.y)>1&&Button("Group all workers here (restart work)")){world.GroupWorkers(u);status="Workers grouped. Choose their shared activity.";}}
                GUI.enabled=wasOn;
                if((u.job>0||u.hasOrder||u.fortified||u.sentry||u.roadOrder||u.autoWork)&&Button("Cancel activity / route [X]")){world.CancelActivity(u);status="Activity and route cancelled.";}
                if(Button(u.fortified?"Wake unit":"Fortify / heal")){bool fortify=!u.fortified;world.CancelActivity(u);foreach(var member in world.WorkerTeam(u))member.fortified=fortify;}
                if(World.Power[u.type]==0&&Button(u.sentry?"Wake from sentry":"Sentry until an enemy approaches")){bool watch=!u.sentry;world.CancelActivity(u);u.sentry=watch;}
                if(cityWorker)GUI.enabled=false;
                
                int target=world.UpgradeTarget(u);if(target>=0&&(World.UnitTech[target]<0||world.nations[0].tech[World.UnitTech[target]])&&Button("Upgrade to "+World.UnitNames[target]+" ("+((World.UnitCosts[target]-World.UnitCosts[u.type])*2)+" gold)"))status=world.Upgrade(u)?"Unit upgraded.":"Requires technology, connected resources, gold, movement and a city owned by your civilisation.";
                if(Powerful(u)&&Button("Pillage enemy improvement"))status=world.Pillage(u)?"Enemy infrastructure pillaged.":"Requires movement and an improved enemy tile at war.";
                if(u.type==4||u.type==9)foreach(var e in world.RangedTargets(u)){if(Button("Fire at "+World.UnitNames[e.type]+" ["+e.x+","+e.y+"]"))status=world.Ranged(u,e.id)?"Ranged attack resolved.":"No movement or line of sight blocked.";GUILayout.Label(world.RangedPreview(u,e),small);}
                GUI.enabled=wasOn;
                if(Button("Disband this unit permanently")){world.units.Remove(u);selected=-1;world.CheckVictory();GUIUtility.ExitGUI();}
            }
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();if(GUILayout.Button("Zoom -")){if(diamondView)IsoZoom(-1);else ClassicZoom(-8);}if(GUILayout.Button("Zoom +")){if(diamondView)IsoZoom(1);else ClassicZoom(8);}GUILayout.EndHorizontal();
            if(GUILayout.Button(diamondView?"View: pixel art (V)":"View: classic textures (V)"))ToggleView();
        }
        void ClassicZoom(int step){
            if(globeZoom){if(step>0)globeZoom=false;return;}
            if(step<0&&tilePixels<=24&&GlobeZoomAllowed()){EnterGlobeZoom(WrapColumn((int)((mapScroll.x+MapW/2)/tilePixels)),Mathf.Clamp((int)((mapScroll.y+MapH/2)/tilePixels),0,world.Height-1));return;}
            tilePixels=Mathf.Clamp(tilePixels+step,24,96);ClampMapScroll();
        }
        bool Powerful(Unit u){return World.Power[u.type]>0;}
        void NavigationInput()
        {
            if(guide||confirmNew||pendingUnit!=-1||mainMenu||pauseMenu||cityScreen||researchScreen||detailsScreen!=""||loadScreen)return;
            var e=Event.current;
            // While a prompt waits for an answer the map stays still (owner request).
            if(PromptOpen()){if(e.type==EventType.ScrollWheel)e.Use();return;}
            // Consume the wheel before BeginScrollView can turn it into vertical panning.
            if(e.type==EventType.ScrollWheel&&diamondView&&MapView.Contains(e.mousePosition)){IsoZoom(e.delta.y>0?-1:1);ResetMapGesture();e.Use();}
            else if(e.type==EventType.ScrollWheel&&MapView.Contains(e.mousePosition)&&(globeZoom||e.delta.y>0&&tilePixels<=24&&GlobeZoomAllowed())){ClassicZoom(e.delta.y>0?-8:8);ResetMapGesture();e.Use();}
            else if(e.type==EventType.ScrollWheel&&MapView.Contains(e.mousePosition)){
                float old=tilePixels;tilePixels=Mathf.Clamp(tilePixels-e.delta.y*zoomSensitivity,24,96);
                var point=e.mousePosition-new Vector2(MapX,MapY);
                mapScroll=(mapScroll+point)*(tilePixels/old)-point;ClampMapScroll();ResetMapGesture();e.Use();
            }
            string focus=GUI.GetNameOfFocusedControl();if(focus=="rename"||focus=="cityrename")return;
            if(e.type==EventType.KeyDown&&!e.control&&!e.alt&&!e.command){if(e.keyCode==KeyCode.V){ToggleView();e.Use();}else if(e.keyCode==KeyCode.W){if(diamondView)IsoPan(0,-64);else mapScroll.y-=64;e.Use();}else if(e.keyCode==KeyCode.S){if(diamondView)IsoPan(0,64);else mapScroll.y+=64;e.Use();}else if(e.keyCode==KeyCode.A){if(diamondView)IsoPan(-64,0);else mapScroll.x-=64;e.Use();}else if(e.keyCode==KeyCode.D){if(diamondView)IsoPan(64,0);else mapScroll.x+=64;e.Use();}else if(e.keyCode==KeyCode.F1){guide=true;e.Use();}}
            if(e.type==EventType.MouseDrag&&e.button==2&&MapView.Contains(e.mousePosition)){if(diamondView)IsoPan(-e.delta.x,-e.delta.y);else mapScroll-=e.delta;e.Use();}
            if(diamondView)ClampCamera();else ClampMapScroll();
        }
    }
}
