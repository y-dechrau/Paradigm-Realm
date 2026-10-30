using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    public partial class RealmGame : MonoBehaviour
    {
        int dragUnit=-1,pressX,pressY,pendingUnit=-1,pendingX,pendingY; bool dragging,mapPointerDown; Vector2 pressMouse; int[] pendingWars=new int[0];
        World world; int selected=-1,cityId=-1; Vector2 mapScroll,sideScroll,logScroll;
        Texture2D[] terrain,people; Texture2D cityTexture; GUIStyle title,heading,body,small,button;
        bool nameSetup; string proposedName="Verdant Union";
        bool guide=false,confirmNew; bool mainMenu=true,hasSession,confirmQuit; GUIStyle menuTitle,menuSubtitle,menuButton; string status="Select your Settler, then Found city.";
        float tilePixels=56; readonly Color[] nations={new Color(.55f,.86f,.61f),new Color(.9f,.65f,.35f),new Color(.4f,.7f,.88f),new Color(.9f,.4f,.4f),new Color(.78f,.6f,.92f),new Color(.95f,.88f,.4f),new Color(.4f,.85f,.8f),new Color(.9f,.55f,.75f),new Color(.7f,.75f,.4f),new Color(.95f,.75f,.55f),new Color(.55f,.6f,.95f),new Color(.6f,.9f,.5f),new Color(.85f,.85f,.85f),new Color(.75f,.5f,.4f),new Color(.4f,.55f,.55f),new Color(.95f,.5f,.2f)};
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(FindFirstObjectByType<RealmGame>()==null)new GameObject("Paradigm Realm").AddComponent<RealmGame>();}
        void Awake(){zoomSensitivity=PlayerPrefs.GetFloat("RealmZoomSensitivity",2f);tooltips=PlayerPrefs.GetInt("RealmTooltips",1)!=0;var cameraObject=new GameObject("World Camera");var camera=cameraObject.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=UiBackground;camera.orthographic=true;Application.targetFrameRate=30;world=World.New(Environment.TickCount&int.MaxValue);selected=world.units.First(u=>u.owner==0).id;MakeArt();Center();}
        Unit Selected(){return world.units.Find(u=>u.owner==0&&u.id==selected);}
        void Center(){var u=Selected();if(u!=null&&diamondView){FocusTile(u.x,u.y);return;}if(u!=null)FocusTile(u.x,u.y);}
        string SavePath {get{return Path.Combine(Application.persistentDataPath,"earth-save.json");}}
        void Save(){SaveTo(activeSavePath??SavePath);}
        bool HasSavedGame(){return File.Exists(SavePath)||NamedSaves().Length>0;}
        string[] NamedSaves(){try{return Directory.GetFiles(Application.persistentDataPath,"earth-named-*.json").OrderByDescending(File.GetLastWriteTimeUtc).ToArray();}catch{return new string[0];}}
        void Load(){var candidates=NamedSaves().Concat(File.Exists(SavePath)?new[]{SavePath}:new string[0]);LoadFile(activeSavePath??candidates.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault()??SavePath);}
        void LoadFile(string path){try{var next=JsonUtility.FromJson<World>(File.ReadAllText(path));if(next!=null)next.UpgradeSave();if(next==null||!next.Valid())throw new Exception("Invalid or incompatible save.");world=next;ResetDiplomacyUI();renameText=world.civilisationName;selected=world.units.Where(u=>u.owner==0).Select(u=>u.id).DefaultIfEmpty(-1).First();cityId=-1;cityScreen=false;researchScreen=false;detailsScreen="";pauseMenu=false;loadScreen=false;Center();hasSession=true;mainMenu=false;activeSavePath=Path.GetFileName(path).StartsWith("earth-auto-")?null:path;status="Game loaded.";}catch(Exception e){status="Could not load: "+e.Message;}}
        void NextUnit(){var list=world.units.Where(u=>u.owner==0&&u.moves>0&&!u.fortified&&!u.sentry&&!u.exploring&&!u.hasOrder&&u.job==0).ToList();if(list.Count==0){status="No units with movement. End the turn.";return;}int i=list.FindIndex(u=>u.id==selected);selected=list[(i+1)%list.Count].id;cityId=-1;Center();}
        void Found(){var u=Selected();string reason=world.FoundReason(u);var c=world.Found(u);if(c==null)status=reason;else{selected=-1;cityId=c.id;status=c.name+" founded.";}}
        void EndTurn(){world.EndTurn();AutoSave();status=world.result!=""?world.result:"Turn "+world.turn+". Choose your orders.";}
        void Styles(){if(title!=null)return;title=new GUIStyle(GUI.skin.label){fontSize=26,fontStyle=FontStyle.Bold};heading=new GUIStyle(GUI.skin.label){fontSize=17,fontStyle=FontStyle.Bold};body=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true};small=new GUIStyle(body){fontSize=12};button=new GUIStyle(GUI.skin.button){fontSize=13,wordWrap=true};}
        bool Button(string text){return GUILayout.Button(text,button,GUILayout.Height(30));}
        void OnGUI()
        {
            if(world!=null)world.EndVisibilityCache();compositeBudgetOn=false;
            ApplyVisualSkin();Styles();HudStyles();float scale=Mathf.Min(Screen.width/1280f,Screen.height/800f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-800*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
            GUI.DrawTexture(new Rect(0,0,1280,800),Texture2D.whiteTexture,ScaleMode.StretchToFill,false,0,UiBackground,0,0);
            if(artGallery){DrawGallery();return;}
            if(nameSetup){DrawNameSetup();return;}
            if(loadScreen){DrawLoadScreen();return;}
            if(mainMenu){DrawMainMenu();return;}
            HandleGlobalInput();
            if(pauseMenu){GUI.enabled=false;DrawMap();GUI.enabled=true;DrawPauseMenu();DrawSaveToast();return;}
            MapKeyboardInput();
            BuildHudRects();
            if(researchScreen){DrawTopBar();DrawResearchScreen();DrawSaveToast();return;}
            if(detailsScreen!=""){DrawTopBar();DrawDetailsScreen();DrawSaveToast();return;}
            if(cityScreen){DrawTopBar();DrawCityScreen();DrawSaveToast();return;}
            NavigationInput();
            GUI.enabled=!guide&&!confirmNew&&pendingUnit==-1;
            DrawMap();DrawHud();
            if(guide)DrawGuide();if(confirmNew){GUI.Box(new Rect(420,270,440,190),"");GUILayout.BeginArea(new Rect(445,290,390,145));GUILayout.Label("Start a new world?",heading);GUILayout.Label("Save first if you want to keep your current game.",body);if(Button("Start new game")){confirmNew=false;nameSetup=true;GUIUtility.ExitGUI();}if(Button("Cancel"))confirmNew=false;GUILayout.EndArea();}
            if(pendingUnit!=-1){DrawWarPrompt();return;}
            DrawSaveToast();
        }
        // Meeting a civilisation opens Diplomacy with them straight away, one meeting at a time. This runs between frames, not
        // inside OnGUI, so the screen never changes between Unity's layout and repaint passes.
        void Update()
        {
            if(world==null||mainMenu||nameSetup||pauseMenu||guide||confirmNew||pendingUnit!=-1||cityScreen||researchScreen||detailsScreen!=""||world.result!="")return;
            if(world.newContacts!=null&&world.newContacts.Count>0){int met=world.newContacts[0];world.newContacts.RemoveAt(0);if(met>0&&met<world.civCount){diplomacyPartner=met;detailsScreen="Diplomacy";researchScreen=false;detailsScroll=Vector2.zero;ResetMapGesture();status="First contact: "+world.NationName(met)+".";}}
        }
        void MapKeyboardInput(){
            if(cityScreen||researchScreen||detailsScreen!=""||pauseMenu||mainMenu||loadScreen)return;
            var ev=Event.current;if(GUI.GetNameOfFocusedControl()!="rename"&&GUI.GetNameOfFocusedControl()!="cityrename"&&ev.type==EventType.KeyDown&&!ev.control&&!ev.alt&&!ev.command&&world.result==""&&!guide&&!confirmNew&&pendingUnit==-1){var u=Selected();if(ev.keyCode==KeyCode.X&&u!=null){world.CancelActivity(u);ResetMapGesture();status="Activity and route cancelled.";ev.Use();}else if(ev.keyCode==KeyCode.Tab){NextUnit();ev.Use();}else if(ev.keyCode==KeyCode.Space){if(u!=null){world.SkipTurn(u);status=World.UnitNames[u.type]+" skips this turn.";}ev.Use();}else if(ev.keyCode==KeyCode.Return||ev.keyCode==KeyCode.KeypadEnter){EndTurn();ev.Use();}else if(ev.keyCode==KeyCode.B){Found();ev.Use();}else if(u!=null){int dx=ev.keyCode==KeyCode.LeftArrow?-1:ev.keyCode==KeyCode.RightArrow?1:0,dy=ev.keyCode==KeyCode.UpArrow?-1:ev.keyCode==KeyCode.DownArrow?1:0;if(dx!=0||dy!=0){IssueOrder(u,u.x+dx,u.y+dy);ev.Use();}}}
        }

        void StartGame()
        {
            int parsed; int chosenSeed=int.TryParse(seedText,out parsed)?parsed:Environment.TickCount&int.MaxValue; bool custom=setupCiv>=CivilisationCatalog.Count;world=World.New(chosenSeed,custom?proposedName:CivilisationCatalog.Names[setupCiv],setupMap,setupSpeed,1,0,setupSize,setupShape,setupCivs,custom?-1:setupCiv,customLeader,customPortrait,customTraitA,customTraitB);setupPage=0;ResetDiplomacyUI();renameText=world.civilisationName;nameSetup=false;
            selected=world.units.First(u=>u.owner==0).id;cityId=-1;
            sideScroll=Vector2.zero;researchScreen=false;detailsScreen="";cityScreen=false;pauseMenu=false;activeSavePath=null;Center();hasSession=true;mainMenu=false;
            guide=true;confirmNew=false;status="Select your Settler, then Found city.";
        }
        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        void DrawMainMenu()
        {
            if(menuTitle==null){
                menuTitle=new GUIStyle(title){fontSize=62,alignment=TextAnchor.MiddleCenter};
                menuTitle.normal.textColor=UiGold;
                menuSubtitle=new GUIStyle(body){fontSize=16,alignment=TextAnchor.MiddleCenter};
                menuSubtitle.normal.textColor=UiText;
                menuButton=new GUIStyle(button){fontSize=19,fontStyle=FontStyle.Bold};
            }
            // Original pixel landscape, drawn in integer-sized blocks.
            var backdrop=Art("title-world");if(backdrop!=null)GUI.DrawTexture(new Rect(0,0,1280,800),backdrop,ScaleMode.ScaleAndCrop);
            Fill(new Rect(0,0,1280,800),new Color(0,0,0,.18f));
            Fill(new Rect(390,85,500,637),new Color(UiPanel.r,UiPanel.g,UiPanel.b,.93f));
            Fill(new Rect(390,85,500,3),new Color(.68f,.57f,.35f));
            Fill(new Rect(390,719,500,3),new Color(.68f,.57f,.35f));
            GUI.Label(new Rect(390,110,500,78),"PARADIGM",menuTitle);
            GUI.Label(new Rect(390,181,500,78),"REALM",menuTitle);
            GUI.Label(new Rect(420,264,440,30),"EARTH  /  THE FIRST AGE",menuSubtitle);
            GUI.enabled=!guide&&!confirmNew&&pendingUnit==-1&&!confirmQuit;
            if(GUI.Button(new Rect(465,317,350,48),"NEW GAME",menuButton)){
                if(hasSession)confirmNew=true;else nameSetup=true;GUIUtility.ExitGUI();
            }
            bool enabled=GUI.enabled;GUI.enabled=enabled&&(hasSession||HasSavedGame());
            if(GUI.Button(new Rect(465,377,350,48),hasSession?"RESUME GAME":"CONTINUE SAVED GAME",menuButton)){
                if(hasSession)mainMenu=false;else Load();GUIUtility.ExitGUI();
            }
            GUI.enabled=enabled;
            if(GUI.Button(new Rect(465,437,350,42),"LOAD GAME",menuButton)){OpenLoadScreen();}
            if(GUI.Button(new Rect(465,487,350,42),"HOW TO PLAY",menuButton)){guide=true;GUIUtility.ExitGUI();}
            if(GUI.Button(new Rect(465,537,350,42),"QUIT",menuButton)){
                if(hasSession)confirmQuit=true;else QuitGame();GUIUtility.ExitGUI();
            }
            GUI.enabled=true;
            GUI.Label(new Rect(430,595,420,52),hasSession?"Your current game is paused. Save in-game to keep it after quitting.":HasSavedGame()?"A saved civilisation awaits your return.":"Found a settlement. Shape a civilisation.\nYour story begins on Earth.",menuSubtitle);
            if(status.StartsWith("Could not load"))GUI.Label(new Rect(430,637,420,55),status,small);
            if(GUI.Button(new Rect(465,663,350,36),"ARTWORK GALLERY",button)){artGallery=true;GUIUtility.ExitGUI();}
            GUI.Label(new Rect(390,735,500,30),"EARTH  |  PIXEL REALMS EDITION",menuSubtitle);
            if(guide)DrawGuide();
            if(confirmNew||confirmQuit){
                Fill(new Rect(0,0,1280,800),UiDim);
                GUI.Box(new Rect(390,275,500,230),"");
                GUI.Label(new Rect(420,295,440,35),confirmQuit?"Quit Paradigm Realm?":"Start a new civilisation?",heading);
                GUI.Label(new Rect(420,340,440,55),"Unsaved progress will be lost. Cancel and use Save in the game to keep your progress.",body);
                if(GUI.Button(new Rect(420,422,210,44),confirmQuit?"Quit without saving":"Start new game",button)){
                    if(confirmQuit)QuitGame();else {confirmNew=false;nameSetup=true;}GUIUtility.ExitGUI();
                }
                if(GUI.Button(new Rect(650,422,210,44),"Cancel",button)){confirmNew=false;confirmQuit=false;GUIUtility.ExitGUI();}
            }
        }
        // Stage 7: a clearer New Game screen. Two columns (your civilisation, your world), each choice explained, and a one-line
        // summary of the game about to start.
        // Two pages: choose a civilisation (12 premade or custom), then the world.
        int setupPage,setupCiv,customPortrait=7,customTraitA=6,customTraitB=8;string customLeader="";
        GUIStyle cardLabel;
        // Step through the portraits that exist (pictures still to be supplied are skipped).
        int NextPortrait(int from,int step){for(int k=1;k<=CivilisationCatalog.PortraitCount;k++){int i=((from+step*k)%CivilisationCatalog.PortraitCount+CivilisationCatalog.PortraitCount)%CivilisationCatalog.PortraitCount;if(Portrait(i)!=null)return i;}return from;}
        bool CustomValid(){return setupCiv<CivilisationCatalog.Count||(!string.IsNullOrWhiteSpace(proposedName)&&customTraitA!=customTraitB);}
        void DrawNameSetup()
        {
            GUI.Box(new Rect(160,60,960,680),"");
            if(setupPage==0){DrawCivilisationChoice();return;}
            GUI.Label(new Rect(190,74,900,44),"New game — the world",title);
            GUI.Box(new Rect(185,125,430,500),"");GUI.Box(new Rect(665,125,430,500),"");
            DrawSetupOptions();
            GUI.Label(new Rect(185,636,910,40),SetupSummary(),body);
            if(GUI.Button(new Rect(185,680,450,46),"Begin civilisation",button)){StartGame();GUIUtility.ExitGUI();}
            if(GUI.Button(new Rect(645,680,450,46),"Back: civilisation",button)){setupPage=0;GUIUtility.ExitGUI();}
        }
        void DrawCivilisationChoice()
        {
            if(cardLabel==null)cardLabel=new GUIStyle(small){alignment=TextAnchor.UpperCenter,wordWrap=true};
            GUI.Label(new Rect(190,74,900,44),"Choose your civilisation",title);
            var e=Event.current;
            // Every offered civilisation, then Custom, eight to a row.
            var cards=CivilisationCatalog.Offered().Concat(new[]{CivilisationCatalog.Count}).ToList();
            if(setupCiv<CivilisationCatalog.Count&&!CivilisationCatalog.Enabled[setupCiv])setupCiv=cards[0];
            for(int k=0;k<cards.Count;k++)
            {
                int i=cards[k],col=k%8,row=k/8;var r=new Rect(185+col*114,124+row*206,108,200);
                if(setupCiv==i)Fill(new Rect(r.x-4,r.y-4,r.width+8,r.height+8),UiAccent);
                GUI.Box(r,"");
                var tex=Portrait(i<CivilisationCatalog.Count?CivilisationCatalog.Portraits[i]:customPortrait);
                if(tex!=null)GUI.DrawTexture(new Rect(r.x+4,r.y+4,100,155),tex,ScaleMode.ScaleAndCrop);
                GUI.Label(new Rect(r.x+3,r.y+161,102,38),i<CivilisationCatalog.Count?CivilisationCatalog.Names[i]:"Custom civilisation",cardLabel);
                if(e.type==EventType.MouseDown&&e.button==0&&r.Contains(e.mousePosition)){setupCiv=i;if(i<CivilisationCatalog.Count)proposedName=CivilisationCatalog.Names[i];e.Use();}
            }
            if(setupCiv<CivilisationCatalog.Count)
            {
                GUI.Label(new Rect(185,544,910,30),CivilisationCatalog.Names[setupCiv]+" — led by "+CivilisationCatalog.Leaders[setupCiv],heading);
                var traits=CivilisationCatalog.Traits[setupCiv];
                for(int k=0;k<2;k++)GUI.Label(new Rect(185,578+k*28,910,26),CivilisationCatalog.TraitNames[traits[k]]+" — "+CivilisationCatalog.TraitTexts[traits[k]],body);
                GUI.Label(new Rect(185,638,910,24),"Rivals are drawn from the other civilisations, each with its own leader and traits.",small);
            }
            else
            {
                GUI.Label(new Rect(185,532,260,20),"Civilisation name",small);GUI.SetNextControlName("customname");proposedName=GUI.TextField(new Rect(185,552,260,28),proposedName,32);
                GUI.Label(new Rect(455,532,260,20),"Leader",small);customLeader=GUI.TextField(new Rect(455,552,260,28),customLeader,32);
                GUI.Label(new Rect(725,532,370,20),"Portrait: "+CivilisationCatalog.PortraitTitles[customPortrait],small);
                if(GUI.Button(new Rect(725,552,44,28),"<",button))customPortrait=NextPortrait(customPortrait,-1);
                GUI.Label(new Rect(775,556,70,24),(customPortrait+1)+" / "+CivilisationCatalog.PortraitCount,body);
                if(GUI.Button(new Rect(845,552,44,28),">",button))customPortrait=NextPortrait(customPortrait,1);
                GUI.Label(new Rect(185,584,455,20),"First trait",small);customTraitA=GUI.SelectionGrid(new Rect(185,603,450,69),customTraitA,CivilisationCatalog.TraitNames,4);
                GUI.Label(new Rect(645,584,450,20),"Second trait",small);customTraitB=GUI.SelectionGrid(new Rect(645,603,450,69),customTraitB,CivilisationCatalog.TraitNames,4);
                GUI.Label(new Rect(185,673,910,20),customTraitA==customTraitB?"Choose two different traits.":CivilisationCatalog.TraitLine(new[]{customTraitA,customTraitB}),small);
            }
            bool enabled=GUI.enabled;GUI.enabled=enabled&&CustomValid();
            if(GUI.Button(new Rect(185,696,450,38),"Next: choose the world",button)){setupPage=1;GUIUtility.ExitGUI();}
            GUI.enabled=enabled;
            if(GUI.Button(new Rect(645,696,450,38),"Back",button)){nameSetup=false;setupPage=0;GUIUtility.ExitGUI();}
        }
        void DrawRoad(Tile t,Rect rect)
        {
            Color edge=new Color(.27f,.2f,.12f),sand=new Color(.82f,.68f,.43f);
            float half=rect.width/2,cx=rect.x+half,cy=rect.y+half;int reach=(int)(half/2)*2;
            // Straight arms are one long strip each; diagonal arms are short chains of squares. Colours are set once per layer.
            var old=GUI.color;
            for(int layer=0;layer<2;layer++){
                GUI.color=layer==0?edge:sand;float pad=layer==0?2:1;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)if((dx!=0||dy!=0)&&world.RoadConnects(t,dx,dy)){
                    if(dx==0||dy==0){
                        float x0=dx<0?cx-reach-pad:cx-pad,y0=dy<0?cy-reach-pad:cy-pad;
                        float w=dx==0?pad*2:reach+pad*2,h=dy==0?pad*2:reach+pad*2;
                        GUI.DrawTexture(new Rect(x0,y0,w,h),Texture2D.whiteTexture);
                    }else for(int step=0;step<=half;step+=3){GUI.DrawTexture(new Rect(cx+dx*step-pad,cy+dy*step-pad,pad*2,pad*2),Texture2D.whiteTexture);}
                }
            }
            Fill(new Rect(cx-2,cy-2,4,4),sand);
            GUI.color=old;
        }
        static readonly string[] ResourceArtNames={"resource-00","resource-01","resource-02","resource-03","resource-04","resource-05"};
        void DrawResource(Tile t,Rect rect)
        {
            if(t.resource<=0||!world.ResourceKnown(0,t.resource))return;
            float size=rect.width*.36f;var badge=new Rect(rect.xMax-size,rect.yMax-size,size,size);
            Fill(badge,new Color(1,1,1,.5f));
            if(World.IsSeaResource(t.resource)){var icon=IsoPicture(5,t.resource,0);if(icon!=null)GUI.DrawTexture(badge,icon);}
            else DrawArt(ResourceArtNames[Math.Max(0,Math.Min(5,t.resource))],badge);

        }
        void DrawTileTooltip()
        {
            Vector2 mouse=Event.current.mousePosition;
            if(!InMap(mouse))return;
            int x,y;if(!MapTileAt(mouse,out x,out y))return;
            var t=world.KnownTile(world.At(x,y));if(t==null)return;
            string text="Unexplored territory";
            if(t.seen){var v=world.TileYield(t);if(t.resource>0&&!world.ResourceKnown(0,t.resource)){v[0]-=World.ResourceFood[t.resource];v[1]-=World.ResourceShields[t.resource];v[2]-=World.ResourceTrade[t.resource];}text=World.TerrainNames[t.terrain]+"  ("+x+", "+y+")";
                text+="\n"+(t.owner<0?"Unclaimed":world.met[t.owner]?world.NationName(t.owner):"Unknown territory");text+="\n"+(World.IsImpassable(t.terrain)?"Impassable to land units":"Movement: "+(World.RoughTerrain(t.terrain)?3:2));
                text+="\nFood "+v[0]+" | Production "+v[1]+" | Trade "+v[2];
                if(t.resource>0&&world.ResourceKnown(0,t.resource))text+="\nResource: "+World.ResourceNames[t.resource]+" (+"+World.ResourceFood[t.resource]+" food, +"+World.ResourceShields[t.resource]+" shields, +"+World.ResourceTrade[t.resource]+" trade when worked"+(t.resource==15?"; +3 shields more with Chemistry":"")+")";
                if(t.road)text+="\nRoad: 1 movement between connected roads"+(World.RoadTrade(t.terrain)?"; +1 trade":"");if(t.farm)text+="\nFarm / pasture: +2 food";if(t.mine)text+="\nMine: +2 production";if(t.fort)text+="\nFort: +2 defence";
                if(world.Visible(x,y)){var city=world.cities.Find(c=>c.x==x&&c.y==y);if(city!=null)text+="\n"+city.name+" - "+world.NationName(city.owner);var units=world.units.Where(u=>u.x==x&&u.y==y);foreach(var unit in units.OrderByDescending(a=>a.id==selected).Take(3))text+="\n\n"+world.NationName(unit.owner)+": "+World.UnitNames[unit.type]+" — "+UnitDescription(unit)+"\nAttack "+(World.Attack[unit.type]+unit.promotion)+" / Defence "+(World.Defence[unit.type]+unit.promotion)+" ("+World.RankNames[Math.Max(0,Math.Min(2,unit.promotion))]+") | Health "+unit.hp+"/10 | Movement "+unit.moves+"/"+World.Moves[unit.type]+"\n"+world.Activity(unit)+" | "+world.Remaining(unit);if(units.Count()>3)text+="\n+ "+(units.Count()-3)+" more units; select to inspect.";}
                else text+="\nOutside current sight; last-known information"+(string.IsNullOrEmpty(t.rememberedCity)?"":"\nLast seen city: "+t.rememberedCity);
            }
            var settler=Selected();if(t.seen&&settler!=null&&settler.type==0){
                var site=world.AssessSettlement(settler.owner,x,y,settler.x,settler.y);
                if(!site.Suitable)text+="\n\nSettlement: "+site.reason;
                else{var centre=World.CentreYield(t.terrain);text+="\n\nSettlement: centre "+centre[0]+" food / "+centre[1]+" production; early population limit "+site.housing+(site.freshWater?" (fresh water)":" (no known fresh water)")+". Estimated initial city upkeep "+site.upkeep+" gold/turn.";}
            }
            string preview=world.CombatPreview(Selected(),x,y);if(preview!="")text+="\n\n"+preview;
            float height=body.CalcHeight(new GUIContent(text),392)+20;
            var box=new Rect(Mathf.Min(mouse.x+18,854),Mathf.Max(4,Mathf.Min(mouse.y+18,790-height)),416,height);
            Fill(box,UiPanel);GUI.Box(box,"");GUI.Label(new Rect(box.x+10,box.y+8,396,height-16),text,body);
        }
        void DrawMap()
        {
            // Pictures are only painted on Repaint events; Layout and mouse events skip all the drawing work.
            if(globeZoom){DrawGlobeZoom();return;}
            if(diamondView){DrawIsoMap();return;}
            bool repaint=Event.current.type==EventType.Repaint;
            mapScroll=new Vector2(Mathf.Round(mapScroll.x),Mathf.Round(mapScroll.y));
            if(repaint){world.BeginVisibilityCache();PrepareTerrainView();}
            // A sphere gets one extra screen of content to the east, filled with the western edge of the world again, so panning
            // past the date line shows the same land coming round; ClampMapScroll then wraps the position back.
            float worldPixels=world.Width*tilePixels;if(world.Sphere)ClampMapScroll();
            mapScroll=GUI.BeginScrollView(MapView,mapScroll,new Rect(0,0,worldPixels+(world.Sphere?MapW:0),world.Height*tilePixels),GUIStyle.none,GUIStyle.none);
            if(repaint){DrawMapContents();if(world.Sphere&&mapScroll.x+MapW>worldPixels){var keep=mapScroll;GUI.BeginGroup(new Rect(worldPixels,0,MapW,world.Height*tilePixels));mapScroll.x-=worldPixels;PrepareTerrainView();DrawMapContents();mapScroll=keep;GUI.EndGroup();}}
            GUI.EndScrollView();
            world.EndVisibilityCache();
            HandleMapInput();
            if(dragging){var m=Event.current.mousePosition;var u=world.units.Find(a=>a.id==dragUnit);if(u!=null)DrawOrderLine(new Vector2(MapX+u.x*tilePixels+tilePixels/2-mapScroll.x,MapY+u.y*tilePixels+tilePixels/2-mapScroll.y),m);GUI.Label(new Rect(m.x+15,m.y+12,220,30),"Release to issue destination",body);}
        }
        void DrawMapContents()
        {
            compositeBudgetOn=true;compositeClock.Reset();compositeClock.Start();
            int mapWidth=world.Width;viewTiles.Clear();viewVisible.Clear();int pooled=0;
            // Work out what the player knows about each visible tile once, instead of once per drawing pass.
            foreach(var source in TilesInView()){
                bool seenNow=world.Visible(source.x,source.y);Tile known=source;
                if(source.seen&&!seenNow){if(pooled==knownPool.Count)knownPool.Add(new Tile());known=knownPool[pooled++];world.CopyKnown(source,known);}
                viewTiles.Add(known);viewVisible.Add(seenNow);
            }
            viewStacks.Clear();
            foreach(var unit in world.units){List<Unit> stackAt;int cell=unit.y*mapWidth+unit.x;if(!viewStacks.TryGetValue(cell,out stackAt)){stackAt=new List<Unit>(2);viewStacks[cell]=stackAt;}stackAt.Add(unit);}
            viewCities.Clear();foreach(var city in world.cities)viewCities[city.y*mapWidth+city.x]=city;
            float right=mapScroll.x+MapW,bottom=mapScroll.y+MapH;
            // Draw terrain before raised scenery. Use remembered tiles, never unseen live state.
            for(int n=0;n<viewTiles.Count;n++){var t=viewTiles[n];var r=new Rect(t.x*tilePixels,t.y*tilePixels,tilePixels,tilePixels);
                if(r.xMax<mapScroll.x||r.x>right||r.yMax<mapScroll.y||r.y>bottom)continue;
                if(!t.seen){Fill(r,new Color(.2f,.25f,.31f));continue;}
                GUI.DrawTexture(r,TerrainAppearance(t));
            }
            for(int n=0;n<viewTiles.Count;n++){var t=viewTiles[n];var rect=new Rect(t.x*tilePixels,t.y*tilePixels,tilePixels,tilePixels);
                if(!t.seen||rect.xMax<mapScroll.x||rect.x>right||rect.yMax<mapScroll.y||rect.y>mapScroll.y+tilePixels+MapH)continue;
                bool visible=viewVisible[n];var old=GUI.color;if(!visible)GUI.color=new Color(.48f,.51f,.6f,1);
                DrawLandscape(t,rect);DrawTerritory(t,rect);
                if(!visible&&!string.IsNullOrEmpty(t.rememberedCity))GUI.DrawTexture(rect,cityTexture,ScaleMode.ScaleToFit,true);
                GUI.color=old;
                City c;if(viewCities.TryGetValue(t.y*mapWidth+t.x,out c)&&(c.owner==0||visible)){DrawCityArt(c,new Rect(rect.x-rect.width*.08f,rect.y-rect.height*.3f,rect.width*1.16f,rect.height*1.3f));Fill(new Rect(rect.x+rect.width*.35f,rect.y+2,rect.width*.3f,3),nations[c.owner]);}
            }
            for(int n=0;n<viewTiles.Count;n++){var t=viewTiles[n];var rect=new Rect(t.x*tilePixels,t.y*tilePixels,tilePixels,tilePixels);
                if(rect.xMax<mapScroll.x||rect.x>right||rect.yMax<mapScroll.y||rect.y>bottom)continue;
                if(!t.seen){Fill(rect,new Color(.2f,.25f,.31f));continue;}
                bool visible=viewVisible[n];if(!visible)Fill(rect,new Color(.015f,.025f,.055f,.4f));DrawResource(t,rect);
                int cell=t.y*mapWidth+t.x;Unit first=null,picked=null;int count=0;List<Unit> stackAt;
                if(viewStacks.TryGetValue(cell,out stackAt))foreach(var a in stackAt){if(!(a.owner==0||visible))continue;count++;if(first==null)first=a;if(a.id==selected)picked=a;}
                var u=picked??first;
                if(u!=null)DrawUnitArt(u,new Rect(rect.x+rect.width*.1875f,rect.y+rect.height*.08f,rect.width*.625f,rect.height*.625f),count);
                City c;viewCities.TryGetValue(cell,out c);if(c!=null&&!(c.owner==0||visible))c=null;
                if((u!=null&&u.id==selected)||(c!=null&&c.id==cityId))DrawSelection(rect);
            }
            DrawUnitLabels();
            foreach(var city in world.cities){if(!(city.owner==0||world.Visible(city.x,city.y)))continue;float cx=city.x*tilePixels+tilePixels/2f,cy=city.y*tilePixels+tilePixels*.82f;
                if(cx<mapScroll.x-80||cx>right+80||cy<mapScroll.y-20||cy>bottom)continue;DrawCityPlate(city,city.name,new Vector2(cx,cy),nations[city.owner]);}
            var chosen=Selected();if(chosen!=null&&chosen.hasOrder){
                var marker=new Rect(chosen.destinationX*tilePixels+6,chosen.destinationY*tilePixels+6,20,20);
                Fill(marker,new Color(1,.8f,0,.55f));GUI.Label(marker,"X",small);
            }
            compositeBudgetOn=false;
        }
        void HandleMapInput()
        {
            var e=Event.current;bool inside=InMap(e.mousePosition);
            int x,y;if(!MapTileAt(e.mousePosition,out x,out y)){x=-1;y=-1;inside=false;}
            if(!GUI.enabled||world.result!=""){dragUnit=-1;dragging=false;mapPointerDown=false;return;}
            if(e.type==EventType.MouseDown&&e.button==0&&inside){
                var city=world.cities.Find(c=>c.owner==0&&c.x==x&&c.y==y);
                if(e.clickCount==2&&city!=null){e.Use();OpenCity(city);return;}
                mapPointerDown=true;pressX=x;pressY=y;pressMouse=e.mousePosition;dragging=false;
                var own=world.units.Where(u=>u.owner==0&&u.x==x&&u.y==y).ToList();
                var unit=own.Find(u=>u.id==selected)??own.FirstOrDefault();dragUnit=unit==null?-1:unit.id;e.Use();
            }
            if(e.type==EventType.MouseDrag&&e.button==0&&mapPointerDown&&dragUnit>=0){
                if((e.mousePosition-pressMouse).sqrMagnitude>16){dragging=true;selected=dragUnit;cityId=-1;}e.Use();
            }
            if(e.type==EventType.MouseUp&&e.button==0&&mapPointerDown){
                if(inside){if(dragging&&dragUnit>=0)IssueOrder(world.units.Find(u=>u.id==dragUnit),x,y);else if(x==pressX&&y==pressY)Click(x,y);}
                dragUnit=-1;dragging=false;mapPointerDown=false;e.Use();
            }
        }
        void IssueOrder(Unit u,int x,int y)
        {
            if(u==null)return;world.CancelActivity(u);
            if(u.x==x&&u.y==y){status="Movement order cancelled.";return;}
            if(world.At(x,y)==null||world.Route(u,x,y).Count==0){status="No land route to that destination.";return;}
            var rivals=world.PeaceOwners(u,x,y);
            if(rivals.Length>0){pendingUnit=u.id;pendingX=x;pendingY=y;pendingWars=rivals;return;}
            OrderTeam(u,x,y);
        }
        void DrawWarPrompt()
        {
            Fill(new Rect(0,0,1280,800),UiDim);GUI.Box(new Rect(340,200,600,390),"");
            GUILayout.BeginArea(new Rect(365,220,550,345));GUILayout.Label("Declare war?",heading);
            GUILayout.Label("This attack will declare war on "+string.Join(" and ",pendingWars.Select(i=>world.NationName(i)).ToArray())+". Proceed?",body);
            string blocked=string.Join(" ",pendingWars.Where(i=>!world.AtWar(0,i)).Select(i=>world.WarReason(0,i)).Where(r=>r!="").ToArray());
            GUILayout.Label(blocked!=""?blocked:"War ends trade and access; defensive allies may join. Surprise war causes 45 grievances, formal war 25.",body);
            bool prior=GUI.enabled;GUI.enabled=prior&&blocked=="";
            if(Button(pendingUnit==-2?"Yes — declare war":"Yes — declare war and attack")){
                bool attack=pendingUnit>=0;var u=world.units.Find(a=>a.id==pendingUnit);foreach(int owner in pendingWars)if(!world.AtWar(0,owner))world.SetWar(owner,true);
                pendingUnit=-1;status=!attack?"War declared.":OrderTeam(u,pendingX,pendingY)?"War declared. Attack order issued.":"Attack order could not proceed.";
            }
            GUI.enabled=prior;
            if(Button("No — cancel attack")){pendingUnit=-1;status="Attack cancelled. Peace maintained.";}
            GUILayout.EndArea();
        }
        void Click(int x,int y)
        {
            var own=world.units.Where(u=>u.owner==0&&u.x==x&&u.y==y).ToList();var c=world.cities.Find(a=>a.owner==0&&a.x==x&&a.y==y);
            if(c!=null&&cityId!=c.id&&(own.Count==0||own.Any(u=>u.id==selected))){cityId=c.id;selected=-1;return;}
            if(own.Count>0){int i=own.FindIndex(u=>u.id==selected);selected=own[(i+1)%own.Count].id;cityId=-1;return;}
            IssueOrder(Selected(),x,y);
        }
        void DrawGuide()
        {
            Fill(new Rect(0,0,1280,800),UiDim);GUI.Box(new Rect(330,150,620,485),"");GUILayout.BeginArea(new Rect(358,175,564,435));GUILayout.Label("Welcome to Paradigm Realm",title);
            logScroll=GUILayout.BeginScrollView(logScroll);GUILayout.Label("1. Found a city with your Settler. Double-click your city to open its screen. Units and buildings have separate simultaneous production lines. Click once for a single project; Ctrl+left-click appends, Ctrl+right-click removes the last item from that line. Each building type may exist once per city.\n\n2. Drag units to destinations. Orders continue each turn. Peaceful borders and attacks require permission or a war confirmation. Three troops plus one civilian/support fit per tile, with any number of workers.\n\n3. Workers build roads in 2 turns at home, 3 outside your borders (4 on hills). Farms, mines and forts take 3 worker-turns. Group workers on one tile to share construction; each contributes one work per turn. Research Agriculture for farms, Mining for mines, and The Wheel for roads. Connect improved resources to your capital by owned roads.\n\n4. Open Economy to set taxes and inspect upkeep. A city in disorder (more unhappy than content citizens) makes no production or gold and cannot grow. New projects have costs and locked-requirement explanations.\n\n5. Archers and catapults can fire at visible enemies using their panel. Infantry capture cities. Promotions and upgrades are available when eligible.\n\nWASD / middle drag pan. Mouse wheel zooms at the pointer. Zoom buttons also change scale. X cancels the selected unit or worker team activity / route. Escape opens the game menu; Ctrl+S quick saves. Research opens the technology tree from the top bar. Upgrades require an owned city. Arrows move the selected unit; Tab selects an idle unit. B founds a city. Enter (including keypad Enter) ends a turn. Space skips only the selected unit. F1 opens help. Manual saves keep a backup; three autosaves rotate.\n\nFoundation alpha: Earth, 3 civilisations, 124 technologies across 14 eras. The research screen distinguishes active effects from planned content. Naval units, aircraft and realms remain planned. See Design/MASTER-DESIGN.md for the full specification.",body);
            GUILayout.EndScrollView();GUILayout.FlexibleSpace();if(Button(mainMenu?"Back to menu":"Begin your story")){guide=false;GUIUtility.ExitGUI();}GUILayout.EndArea();
        }
        static void Fill(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        static Texture2D Texture(Color background){var t=new Texture2D(16,16,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};for(int y=0;y<16;y++)for(int x=0;x<16;x++)t.SetPixel(x,y,background);return t;}
        static void Paint(Texture2D t,int x,int y,int w,int h,Color c){for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)t.SetPixel(xx,15-yy,c);}
        void MakeArt()
        {
            Color[] baseColors={new Color(.4f,.5f,.27f),new Color(.23f,.38f,.25f),new Color(.55f,.51f,.36f),new Color(.71f,.64f,.43f),new Color(.14f,.3f,.42f),new Color(.42f,.46f,.44f)};terrain=new Texture2D[World.TerrainNames.Length];
            for(int i=0;i<6;i++){var t=Texture(baseColors[i]);var r=new System.Random(i+15);for(int n=0;n<14;n++)Paint(t,r.Next(15),r.Next(15),1,1,baseColors[i]*1.15f);if(i==1)foreach(int x in new[]{2,8}){Paint(t,x+2,8,1,5,new Color(.4f,.3f,.2f));Paint(t,x,5,5,5,new Color(.13f,.25f,.18f));Paint(t,x+1,3,3,3,new Color(.2f,.4f,.23f));}if(i==5){Paint(t,2,10,12,4,new Color(.3f,.34f,.29f));Paint(t,4,7,8,4,baseColors[i]*1.15f);Paint(t,6,4,4,4,baseColors[i]*1.4f);if(i==5)Paint(t,7,2,2,4,new Color(.88f,.89f,.8f));}if(i==0){for(int x=2;x<15;x+=5){Paint(t,x,11,1,3,new Color(.25f,.43f,.14f));Paint(t,x+1,10,1,2,new Color(.66f,.77f,.34f));}}
                if(i==2){Paint(t,1,11,14,4,new Color(.38f,.34f,.2f));Paint(t,3,9,10,3,new Color(.62f,.58f,.32f));Paint(t,5,7,6,3,new Color(.76f,.7f,.44f));}
                if(i==3){for(int y=4;y<15;y+=5){Paint(t,2,y,8,1,new Color(.9f,.78f,.49f));Paint(t,7,y+1,7,1,new Color(.56f,.4f,.2f));}}
                if(i==4){for(int y=3;y<15;y+=5){Paint(t,2,y,5,1,new Color(.37f,.66f,.83f));Paint(t,9,y+2,5,1,new Color(.37f,.66f,.83f));}}
                t.Apply();terrain[i]=t;}
            LoadTerrainArt();
            people=new Texture2D[World.UnitNames.Length];for(int i=0;i<World.UnitNames.Length;i++){var t=Texture(Color.clear);Paint(t,5,5,6,7,new Color(.15f,.2f,.2f));Paint(t,6,2,4,4,new Color(.92f,.8f,.59f));Paint(t,5,11,2,3,new Color(.75f,.68f,.5f));Paint(t,9,11,2,3,new Color(.75f,.68f,.5f));if(i==0){Paint(t,4,1,8,2,new Color(.7f,.5f,.25f));Paint(t,2,6,3,5,new Color(.6f,.4f,.2f));}else{Paint(t,12,3,1,10,Color.gray);if(i==1)Paint(t,10,3,5,2,Color.gray);if(i==4)Paint(t,13,4,1,6,new Color(.6f,.35f,.2f));if(i==6)Paint(t,11,5,3,2,Color.black);}if(i==11){for(int yy=0;yy<16;yy++)for(int xx=0;xx<16;xx++)t.SetPixel(xx,yy,Color.clear);Paint(t,2,6,12,6,new Color(.6f,.37f,.18f));Paint(t,3,3,10,5,new Color(.9f,.82f,.6f));Paint(t,3,12,3,3,new Color(.16f,.17f,.16f));Paint(t,10,12,3,3,new Color(.16f,.17f,.16f));Paint(t,4,4,8,1,new Color(1,.94f,.75f));}t.Apply();people[i]=t;}
            cityTexture=Texture(Color.clear);Paint(cityTexture,2,7,12,7,new Color(.83f,.76f,.58f));Paint(cityTexture,1,5,14,3,new Color(.5f,.3f,.2f));Paint(cityTexture,6,10,3,4,new Color(.2f,.22f,.19f));Paint(cityTexture,3,1,2,6,new Color(.8f,.8f,.65f));cityTexture.Apply();LoadVisualArt();
        }
        void OnDestroy(){ReleaseVisuals();ClearTerrainComposites();if(terrain!=null)foreach(var t in terrain)if(t!=null&&!loadedTerrain.Contains(t))Destroy(t);if(people!=null)foreach(var t in people)if(t!=null&&!visualArt.ContainsValue(t))Destroy(t);if(cityTexture!=null&&!visualArt.ContainsValue(cityTexture))Destroy(cityTexture);}
    }
}
