using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    public class RealmGame : MonoBehaviour
    {
        World world; int selected=-1,cityId=-1; Vector2 mapScroll,sideScroll,logScroll;
        Texture2D[] terrain,people; Texture2D cityTexture; GUIStyle title,heading,body,small,button;
        bool nameSetup; string proposedName="Verdant Union";
        bool guide=false,confirmNew; bool mainMenu=true,hasSession,confirmQuit; GUIStyle menuTitle,menuSubtitle,menuButton; string status="Select your Settler, then Found city.";
        const float TileSize=32; readonly Color[] nations={new Color(.55f,.86f,.61f),new Color(.9f,.65f,.35f),new Color(.4f,.7f,.88f)};
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(FindFirstObjectByType<RealmGame>()==null)new GameObject("Paradigm Realm").AddComponent<RealmGame>();}
        void Awake(){var cameraObject=new GameObject("World Camera");var camera=cameraObject.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.09f,.11f);camera.orthographic=true;Application.targetFrameRate=30;world=World.New(Environment.TickCount&int.MaxValue);selected=world.units.First(u=>u.owner==0).id;MakeArt();Center();}
        Unit Selected(){return world.units.Find(u=>u.owner==0&&u.id==selected);}
        void Center(){var u=Selected();if(u!=null)mapScroll=new Vector2(Mathf.Max(0,u.x*32-350),Mathf.Max(0,u.y*32-250));}
        string SavePath {get{return Path.Combine(Application.persistentDataPath,"earth-save.json");}}
        void Save(){try{File.WriteAllText(SavePath,JsonUtility.ToJson(world,true));status="Game saved.";}catch(Exception e){status="Could not save: "+e.Message;}}
        void Load(){try{var next=JsonUtility.FromJson<World>(File.ReadAllText(SavePath));if(next!=null)next.UpgradeSave();if(next==null||!next.Valid())throw new Exception("Invalid or incompatible save.");world=next;selected=world.units.Where(u=>u.owner==0).Select(u=>u.id).DefaultIfEmpty(-1).First();cityId=-1;Center();hasSession=true;mainMenu=false;status="Game loaded.";}catch(Exception e){status="Could not load: "+e.Message;}}
        void NextUnit(){var list=world.units.Where(u=>u.owner==0&&u.moves>0).ToList();if(list.Count==0){status="No units with movement. End the turn.";return;}int i=list.FindIndex(u=>u.id==selected);selected=list[(i+1)%list.Count].id;cityId=-1;Center();}
        void Found(){var c=world.Found(Selected());if(c==null)status="Use a Settler with movement, at least 4 tiles from another city.";else{selected=-1;cityId=c.id;status=c.name+" founded.";}}
        void EndTurn(){world.EndTurn();status=world.result!=""?world.result:"Turn "+world.turn+". Choose your orders.";}
        void Styles(){if(title!=null)return;title=new GUIStyle(GUI.skin.label){fontSize=26,fontStyle=FontStyle.Bold};heading=new GUIStyle(GUI.skin.label){fontSize=17,fontStyle=FontStyle.Bold};body=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true};small=new GUIStyle(body){fontSize=12};button=new GUIStyle(GUI.skin.button){fontSize=13,wordWrap=true};}
        bool Button(string text){return GUILayout.Button(text,button,GUILayout.Height(30));}
        void OnGUI()
        {
            Styles();float scale=Mathf.Min(Screen.width/1280f,Screen.height/800f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-800*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
            GUI.DrawTexture(new Rect(0,0,1280,800),Texture2D.whiteTexture,ScaleMode.StretchToFill,false,0,new Color(.055f,.09f,.11f),0,0);
            if(nameSetup){DrawNameSetup();return;}
            if(mainMenu){DrawMainMenu();return;}
            GUI.Label(new Rect(24,15,420,38),"PARADIGM REALM",title);GUI.Label(new Rect(25,55,500,25),"EARTH  |  Turn "+world.turn+"  |  Gold "+world.gold+"  |  "+world.cities.Count(c=>c.owner==0)+" cities",body);
            GUI.enabled=!guide&&!confirmNew;
            if(GUI.Button(new Rect(680,26,100,32),"Menu",button)){mainMenu=true;GUIUtility.ExitGUI();}
            if(GUI.Button(new Rect(795,26,90,32),"Save",button))Save();if(GUI.Button(new Rect(895,26,90,32),"Load",button))Load();if(GUI.Button(new Rect(995,26,110,32),"New world",button))confirmNew=true;if(GUI.Button(new Rect(1115,26,100,32),"Guide",button))guide=true;
            GUI.enabled=!guide&&!confirmNew;
            DrawMap();DrawPanel();GUI.Label(new Rect(24,744,865,42),status,body);
            GUI.enabled=!guide&&!confirmNew&&world.result=="";if(GUI.Button(new Rect(912,744,130,38),"Next unit [Tab]",button))NextUnit();if(GUI.Button(new Rect(1055,744,195,38),"END TURN [Enter]",button))EndTurn();GUI.enabled=true;
            if(!guide&&!confirmNew)DrawTileTooltip();
            if(guide)DrawGuide();if(confirmNew){GUI.Box(new Rect(420,270,440,190),"");GUILayout.BeginArea(new Rect(445,290,390,145));GUILayout.Label("Start a new world?",heading);GUILayout.Label("Save first if you want to keep your current game.",body);if(Button("Start new game")){confirmNew=false;nameSetup=true;GUIUtility.ExitGUI();}if(Button("Cancel"))confirmNew=false;GUILayout.EndArea();}
            var ev=Event.current;if(ev.type==EventType.KeyDown&&!guide&&!confirmNew){var u=Selected();if(ev.keyCode==KeyCode.Tab){NextUnit();ev.Use();}else if(ev.keyCode==KeyCode.Return){EndTurn();ev.Use();}else if(ev.keyCode==KeyCode.B){Found();ev.Use();}else if(u!=null){int dx=ev.keyCode==KeyCode.LeftArrow?-1:ev.keyCode==KeyCode.RightArrow?1:0,dy=ev.keyCode==KeyCode.UpArrow?-1:ev.keyCode==KeyCode.DownArrow?1:0;if(dx!=0||dy!=0){world.Move(u,u.x+dx,u.y+dy);ev.Use();}}}
        }
        void StartGame()
        {
            world=World.New(Environment.TickCount&int.MaxValue,proposedName);nameSetup=false;
            selected=world.units.First(u=>u.owner==0).id;cityId=-1;
            sideScroll=Vector2.zero;Center();hasSession=true;mainMenu=false;
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
                menuTitle.normal.textColor=new Color(.93f,.83f,.58f);
                menuSubtitle=new GUIStyle(body){fontSize=16,alignment=TextAnchor.MiddleCenter};
                menuSubtitle.normal.textColor=new Color(.7f,.79f,.73f);
                menuButton=new GUIStyle(button){fontSize=19,fontStyle=FontStyle.Bold};
            }
            // Original pixel landscape, drawn in integer-sized blocks.
            Fill(new Rect(0,0,1280,800),new Color(.055f,.105f,.15f));
            for(int i=0;i<58;i++){int x=(i*193+31)%1280,y=(i*79+17)%270;Fill(new Rect(x,y,2,2),new Color(.65f,.73f,.72f));}
            Fill(new Rect(985,90,56,56),new Color(.85f,.82f,.61f));
            Fill(new Rect(975,103,76,32),new Color(.85f,.82f,.61f));
            for(int x=0;x<1280;x+=16){
                float ridge=370+Mathf.Round((Mathf.Sin(x*.009f)*80+Mathf.Sin(x*.022f)*30)/16)*16;
                Fill(new Rect(x,ridge,16,800-ridge),new Color(.12f,.22f,.25f));
                float front=570+Mathf.Round(Mathf.Sin(x*.012f)*42/16)*16;
                Fill(new Rect(x,front,16,800-front),new Color(.17f,.29f,.25f));
            }
            for(int i=0;i<16;i++){
                float x=i*91-20,y=625+(i%3)*20;
                Fill(new Rect(x+20,y+26,8,66),new Color(.18f,.22f,.18f));
                Fill(new Rect(x,y+26,48,26),new Color(.075f,.17f,.15f));
                Fill(new Rect(x+8,y+10,32,26),new Color(.075f,.17f,.15f));
                Fill(new Rect(x+16,y,16,20),new Color(.075f,.17f,.15f));
            }
            GUI.DrawTexture(new Rect(102,565,128,128),cityTexture);
            Fill(new Rect(105,696,250,8),new Color(.42f,.39f,.27f));
            Fill(new Rect(390,85,500,637),new Color(.025f,.055f,.07f,.94f));
            Fill(new Rect(390,85,500,3),new Color(.68f,.57f,.35f));
            Fill(new Rect(390,719,500,3),new Color(.68f,.57f,.35f));
            GUI.Label(new Rect(390,110,500,78),"PARADIGM",menuTitle);
            GUI.Label(new Rect(390,181,500,78),"REALM",menuTitle);
            GUI.Label(new Rect(420,264,440,30),"EARTH  /  THE FIRST AGE",menuSubtitle);
            GUI.enabled=!guide&&!confirmNew&&!confirmQuit;
            if(GUI.Button(new Rect(465,317,350,48),"NEW GAME",menuButton)){
                if(hasSession)confirmNew=true;else nameSetup=true;GUIUtility.ExitGUI();
            }
            bool enabled=GUI.enabled;GUI.enabled=enabled&&(hasSession||File.Exists(SavePath));
            if(GUI.Button(new Rect(465,377,350,48),hasSession?"RESUME GAME":"CONTINUE SAVED GAME",menuButton)){
                if(hasSession)mainMenu=false;else Load();GUIUtility.ExitGUI();
            }
            GUI.enabled=enabled;
            if(GUI.Button(new Rect(465,437,350,48),"HOW TO PLAY",menuButton)){guide=true;GUIUtility.ExitGUI();}
            if(GUI.Button(new Rect(465,497,350,48),"QUIT",menuButton)){
                if(hasSession)confirmQuit=true;else QuitGame();GUIUtility.ExitGUI();
            }
            GUI.enabled=true;
            GUI.Label(new Rect(430,568,420,68),hasSession?"Your current game is paused. Save in-game to keep it after quitting.":File.Exists(SavePath)?"A saved civilisation awaits your return.":"Found a settlement. Shape a civilisation.\nYour story begins on Earth.",menuSubtitle);
            if(status.StartsWith("Could not load"))GUI.Label(new Rect(430,637,420,55),status,small);
            GUI.Label(new Rect(390,735,500,30),"EARTH ALPHA  |  ORIGINAL PIXEL ART",menuSubtitle);
            if(guide)DrawGuide();
            if(confirmNew||confirmQuit){
                Fill(new Rect(0,0,1280,800),new Color(0,0,0,.82f));
                GUI.Box(new Rect(390,275,500,230),"");
                GUI.Label(new Rect(420,295,440,35),confirmQuit?"Quit Paradigm Realm?":"Start a new civilisation?",heading);
                GUI.Label(new Rect(420,340,440,55),"Unsaved progress will be lost. Cancel and use Save in the game to keep your progress.",body);
                if(GUI.Button(new Rect(420,422,210,44),confirmQuit?"Quit without saving":"Start new game",button)){
                    if(confirmQuit)QuitGame();else {confirmNew=false;nameSetup=true;}GUIUtility.ExitGUI();
                }
                if(GUI.Button(new Rect(650,422,210,44),"Cancel",button)){confirmNew=false;confirmQuit=false;GUIUtility.ExitGUI();}
            }
        }
        void DrawNameSetup()
        {
            GUI.Box(new Rect(370,225,540,340),"");
            GUI.Label(new Rect(402,252,470,45),"Name your civilisation",title);
            GUI.Label(new Rect(402,309,470,40),"What will your people be known as?",body);
            proposedName=GUI.TextField(new Rect(402,363,470,42),proposedName,32);
            GUI.Label(new Rect(402,415,470,35),"Up to 32 characters. You can explore before meeting rivals.",small);
            GUI.enabled=!string.IsNullOrWhiteSpace(proposedName);
            if(GUI.Button(new Rect(402,476,225,45),"Begin civilisation",button)){StartGame();GUIUtility.ExitGUI();}
            GUI.enabled=true;
            if(GUI.Button(new Rect(647,476,225,45),"Back",button)){nameSetup=false;GUIUtility.ExitGUI();}
        }
        void DrawRoad(Tile t,Rect rect)
        {
            Color edge=new Color(.27f,.2f,.12f),sand=new Color(.82f,.68f,.43f);
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)if((dx!=0||dy!=0)&&world.RoadConnects(t,dx,dy)){
                for(int step=0;step<=16;step+=2)Fill(new Rect(rect.x+Mathf.Clamp(14+dx*step,0,28),rect.y+Mathf.Clamp(14+dy*step,0,28),4,4),edge);
                for(int step=0;step<=16;step+=2)Fill(new Rect(rect.x+Mathf.Clamp(15+dx*step,0,30),rect.y+Mathf.Clamp(15+dy*step,0,30),2,2),sand);
            }
            Fill(new Rect(rect.x+13,rect.y+13,6,6),edge);Fill(new Rect(rect.x+14,rect.y+14,4,4),sand);
        }
        void DrawResource(Tile t,Rect rect)
        {
            if(t.resource==0)return;
            Rect badge=new Rect(rect.x+19,rect.y+1,12,12);Fill(badge,new Color(.08f,.1f,.1f));
            Color c=t.resource==1?new Color(1,.84f,.24f):t.resource==2?new Color(.75f,.84f,.89f):new Color(1,.65f,.05f);
            if(t.resource==1){Fill(new Rect(badge.x+5,badge.y+2,2,9),c);Fill(new Rect(badge.x+2,badge.y+3,8,2),c);Fill(new Rect(badge.x+3,badge.y+6,6,2),c);}
            else {Fill(new Rect(badge.x+2,badge.y+5,8,5),c);Fill(new Rect(badge.x+4,badge.y+3,4,2),c);Fill(new Rect(badge.x+3,badge.y+6,2,2),Color.white);}
        }
        void DrawTileTooltip()
        {
            Vector2 mouse=Event.current.mousePosition;
            if(!new Rect(24,100,835,607).Contains(mouse))return;
            int x=Mathf.FloorToInt((mouse.x-24+mapScroll.x)/32),y=Mathf.FloorToInt((mouse.y-100+mapScroll.y)/32);
            var t=world.At(x,y);if(t==null)return;
            string text="Unexplored territory";
            if(t.seen){var v=world.TileYield(t);text=World.TerrainNames[t.terrain]+"  ("+x+", "+y+")";
                text+="\n"+(t.terrain>=4?"Impassable to land units":"Movement: "+(t.terrain==1||t.terrain==2?3:2));
                text+="\nFood "+v[0]+" | Production "+v[1]+" | Gold "+v[2];
                if(t.resource>0)text+="\nResource: "+World.ResourceNames[t.resource]+" (bonus when worked)";
                if(t.road)text+="\nRoad: 1 movement between connected roads";if(t.farm)text+="\nFarm: +2 food";
                if(world.Visible(x,y)){var city=world.cities.Find(c=>c.x==x&&c.y==y);if(city!=null)text+="\n"+city.name+" - "+world.NationName(city.owner);var units=world.units.Where(u=>u.x==x&&u.y==y);foreach(var unit in units.Take(3))text+="\n"+world.NationName(unit.owner)+": "+World.UnitNames[unit.type];}
                else text+="\nOutside current sight";
            }
            float height=body.CalcHeight(new GUIContent(text),292)+20;
            var box=new Rect(Mathf.Min(mouse.x+18,954),Mathf.Max(4,Mathf.Min(mouse.y+18,790-height)),316,height);
            Fill(box,new Color(.025f,.04f,.045f,.98f));GUI.Box(box,"");GUI.Label(new Rect(box.x+10,box.y+8,296,height-16),text,body);
        }
        void DrawMap()
        {
            GUI.Box(new Rect(18,94,865,637),"");mapScroll=GUI.BeginScrollView(new Rect(24,100,852,624),mapScroll,new Rect(0,0,World.Width*32,World.Height*32));
            foreach(var t in world.tiles){var rect=new Rect(t.x*32,t.y*32,32,32);if(rect.xMax<mapScroll.x||rect.x>mapScroll.x+852||rect.yMax<mapScroll.y||rect.y>mapScroll.y+624)continue;
                if(!t.seen){GUI.color=new Color(.07f,.12f,.16f);GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=Color.white;continue;}
                GUI.DrawTexture(rect,terrain[t.terrain]);if(t.farm)for(int i=4;i<29;i+=6)Fill(new Rect(rect.x+i,rect.y+4,2,24),new Color(.8f,.75f,.35f));
                if(t.road)DrawRoad(t,rect);DrawResource(t,rect);
                Fill(new Rect(rect.x,rect.y,32,1),new Color(0,0,0,.22f));Fill(new Rect(rect.x,rect.y,1,32),new Color(0,0,0,.22f));
                bool visible=world.Visible(t.x,t.y);if(!visible)Fill(rect,new Color(0,0,0,.45f));
                var c=world.cities.Find(a=>a.x==t.x&&a.y==t.y&&(a.owner==0||visible));if(c!=null){GUI.DrawTexture(rect,cityTexture);Fill(new Rect(rect.x+10,rect.y+2,12,4),nations[c.owner]);}
                var stack=world.units.Where(a=>a.x==t.x&&a.y==t.y&&(a.owner==0||visible)).ToList();var u=stack.Find(a=>a.id==selected)??stack.FirstOrDefault();if(u!=null){GUI.DrawTexture(rect,people[u.type]);Fill(new Rect(rect.x+10,rect.y+14,12,8),nations[u.owner]);Fill(new Rect(rect.x+4,rect.y+29,24*u.hp/10f,2),Color.green);if(stack.Count>1)GUI.Label(new Rect(rect.x+22,rect.y-3,20,18),stack.Count.ToString(),small);}
                if(u!=null&&u.id==selected||c!=null&&c.id==cityId){Fill(new Rect(rect.x,rect.y,32,2),Color.yellow);Fill(new Rect(rect.x,rect.y+30,32,2),Color.yellow);Fill(new Rect(rect.x,rect.y,2,32),Color.yellow);Fill(new Rect(rect.x+30,rect.y,2,32),Color.yellow);}
                if(GUI.Button(rect,new GUIContent("",World.TerrainNames[t.terrain]),GUIStyle.none))Click(t.x,t.y);
            }GUI.EndScrollView();
        }
        void Click(int x,int y)
        {
            var own=world.units.Where(u=>u.owner==0&&u.x==x&&u.y==y).ToList();var c=world.cities.Find(a=>a.owner==0&&a.x==x&&a.y==y);
            if(c!=null&&cityId!=c.id&&(own.Count==0||own.Any(u=>u.id==selected))){cityId=c.id;selected=-1;return;}
            if(own.Count>0){int i=own.FindIndex(u=>u.id==selected);selected=own[(i+1)%own.Count].id;cityId=-1;return;}
            if(!world.Move(Selected(),x,y))status="Move to an adjacent land tile. Check movement points and peace/war.";
        }
        void DrawPanel()
        {
            GUI.Box(new Rect(898,94,364,637),"");GUILayout.BeginArea(new Rect(914,108,330,610));sideScroll=GUILayout.BeginScrollView(sideScroll);
            GUILayout.Label(world.civilisationName,heading);GUILayout.Label(world.result==""?"Explore Earth. Build your civilisation.":world.result,body);GUILayout.Space(10);
            var c=world.cities.Find(a=>a.owner==0&&a.id==cityId);var u=Selected();
            if(c!=null){var v=world.Yield(c);GUILayout.Label(c.name+"  |  Population "+c.pop,heading);GUILayout.Label("Food "+v[0]+"   Production +"+v[1]+"   Science +"+v[2],small);GUILayout.Label("Building: "+World.BuildName(c.build)+" ("+c.production+"/"+(c.build<0?0:World.Costs[c.build])+")",body);foreach(int b in world.Choices(c))if(Button(World.BuildName(b)+" - "+World.Costs[b]))c.build=b;}
            else if(u!=null){GUILayout.Label(World.UnitNames[u.type],heading);GUILayout.Label("Moves "+u.moves+"/"+World.Moves[u.type]+"   Health "+u.hp+"/10",body);GUI.enabled=world.result==""&&!guide&&!confirmNew;if(u.type==0&&Button("Found city [B]"))Found();if(u.type==1){if(Button("Build road")&&!world.Improve(u,false))status="Road already built or no movement left.";if(Button("Build farm")&&!world.Improve(u,true))status="Needs Agriculture, open land and movement.";}if(Button("Skip unit")){u.moves=0;NextUnit();}GUI.enabled=!guide&&!confirmNew;}
            else GUILayout.Label("Select a unit or city on the map.",body);
            GUILayout.Space(12);GUILayout.Label("RESEARCH",heading);GUILayout.Label(world.research<0?"All technologies discovered":World.TechNames[world.research]+"  "+world.science+"/"+World.TechCosts[world.research],body);foreach(int t in world.AvailableTech())if(Button("Research "+World.TechNames[t]))world.research=t;
            GUILayout.Space(12);GUILayout.Label("DIPLOMACY",heading);if(!world.met.Skip(1).Any(v=>v))GUILayout.Label("No other civilisations encountered.",body);for(int i=1;i<3;i++)if(world.met[i]&&Button(world.NationName(i)+": "+(world.war[i]?"offer peace":"declare war")))world.SetWar(i,!world.war[i]);
            GUILayout.Space(12);GUILayout.Label("CITIES",heading);foreach(var a in world.cities.Where(a=>a.owner==0))if(Button(a.name+"  ("+a.pop+")")){cityId=a.id;selected=-1;mapScroll=new Vector2(Mathf.Max(0,a.x*32-350),Mathf.Max(0,a.y*32-250));}
            GUILayout.Space(12);GUILayout.Label("CHRONICLE",heading);foreach(var line in world.log.Take(8))GUILayout.Label(line,small);GUILayout.EndScrollView();GUILayout.EndArea();
        }
        void DrawGuide()
        {
            Fill(new Rect(0,0,1280,800),new Color(0,0,0,.8f));GUI.Box(new Rect(330,150,620,485),"");GUILayout.BeginArea(new Rect(358,175,564,435));GUILayout.Label("Welcome to Paradigm Realm",title);
            GUILayout.Label("1. Select your Settler and choose Found city.\n\n2. Click a unit, then an adjacent land tile to move. Arrow keys work too. Hills and forest cost 3 movement; open ground 2; connected roads 1.\n\n3. Click a city to choose what it builds. Click again to select its units. Repeated clicks cycle a stack. Workers build roads and, after Agriculture, farms.\n\n4. Choose research and end turns. Declare war before attacking rivals. Eliminate their cities and units to win.\n\nScroll to explore the map and the orders panel. Tab selects your next unit. B founds a city. Enter ends the turn. Save and Load use one local save slot.\n\nEarth-only alpha: generated geography, simple rivals, six technologies. No naval travel or parallel worlds yet.",body);
            GUILayout.FlexibleSpace();if(Button(mainMenu?"Back to menu":"Begin your story")){guide=false;GUIUtility.ExitGUI();}GUILayout.EndArea();
        }
        static void Fill(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        static Texture2D Texture(Color background){var t=new Texture2D(16,16,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};for(int y=0;y<16;y++)for(int x=0;x<16;x++)t.SetPixel(x,y,background);return t;}
        static void Paint(Texture2D t,int x,int y,int w,int h,Color c){for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)t.SetPixel(xx,15-yy,c);}
        void MakeArt()
        {
            Color[] baseColors={new Color(.4f,.5f,.27f),new Color(.23f,.38f,.25f),new Color(.55f,.51f,.36f),new Color(.71f,.64f,.43f),new Color(.14f,.3f,.42f),new Color(.42f,.46f,.44f)};terrain=new Texture2D[6];
            for(int i=0;i<6;i++){var t=Texture(baseColors[i]);var r=new System.Random(i+15);for(int n=0;n<14;n++)Paint(t,r.Next(15),r.Next(15),1,1,baseColors[i]*1.15f);if(i==1)foreach(int x in new[]{2,8}){Paint(t,x+2,8,1,5,new Color(.4f,.3f,.2f));Paint(t,x,5,5,5,new Color(.13f,.25f,.18f));Paint(t,x+1,3,3,3,new Color(.2f,.4f,.23f));}if(i==5){Paint(t,2,10,12,4,new Color(.3f,.34f,.29f));Paint(t,4,7,8,4,baseColors[i]*1.15f);Paint(t,6,4,4,4,baseColors[i]*1.4f);if(i==5)Paint(t,7,2,2,4,new Color(.88f,.89f,.8f));}if(i==0){for(int x=2;x<15;x+=5){Paint(t,x,11,1,3,new Color(.25f,.43f,.14f));Paint(t,x+1,10,1,2,new Color(.66f,.77f,.34f));}}
                if(i==2){Paint(t,1,11,14,4,new Color(.38f,.34f,.2f));Paint(t,3,9,10,3,new Color(.62f,.58f,.32f));Paint(t,5,7,6,3,new Color(.76f,.7f,.44f));}
                if(i==3){for(int y=4;y<15;y+=5){Paint(t,2,y,8,1,new Color(.9f,.78f,.49f));Paint(t,7,y+1,7,1,new Color(.56f,.4f,.2f));}}
                if(i==4){for(int y=3;y<15;y+=5){Paint(t,2,y,5,1,new Color(.37f,.66f,.83f));Paint(t,9,y+2,5,1,new Color(.37f,.66f,.83f));}}
                t.Apply();terrain[i]=t;}
            people=new Texture2D[7];for(int i=0;i<7;i++){var t=Texture(Color.clear);Paint(t,5,5,6,7,new Color(.15f,.2f,.2f));Paint(t,6,2,4,4,new Color(.92f,.8f,.59f));Paint(t,5,11,2,3,new Color(.75f,.68f,.5f));Paint(t,9,11,2,3,new Color(.75f,.68f,.5f));if(i==0){Paint(t,4,1,8,2,new Color(.7f,.5f,.25f));Paint(t,2,6,3,5,new Color(.6f,.4f,.2f));}else{Paint(t,12,3,1,10,Color.gray);if(i==1)Paint(t,10,3,5,2,Color.gray);if(i==4)Paint(t,13,4,1,6,new Color(.6f,.35f,.2f));if(i==6)Paint(t,11,5,3,2,Color.black);}t.Apply();people[i]=t;}
            cityTexture=Texture(Color.clear);Paint(cityTexture,2,7,12,7,new Color(.83f,.76f,.58f));Paint(cityTexture,1,5,14,3,new Color(.5f,.3f,.2f));Paint(cityTexture,6,10,3,4,new Color(.2f,.22f,.19f));Paint(cityTexture,3,1,2,6,new Color(.8f,.8f,.65f));cityTexture.Apply();
        }
        void OnDestroy(){if(terrain!=null)foreach(var t in terrain)Destroy(t);if(people!=null)foreach(var t in people)Destroy(t);if(cityTexture!=null)Destroy(cityTexture);}
    }
}
