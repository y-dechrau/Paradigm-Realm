using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    public partial class RealmGame
    {
        bool loadScreen,researchScreen;
        string detailsScreen="",loadError="",deleteCandidate;
        Vector2 buildingListScroll,unitListScroll,loadScroll,researchScroll,detailsScroll,cityMapScroll,builtScroll,unitQueueScroll,buildingQueueScroll;
        GUIStyle tileLabel;
        void OpenLoadScreen(){loadScreen=true;loadError="";deleteCandidate=null;loadScroll=Vector2.zero;ResetMapGesture();GUI.FocusControl(null);GUIUtility.ExitGUI();}
        void DrawLoadScreen()
        {
            GUI.Box(new Rect(250,100,780,600),"");GUI.Label(new Rect(280,125,600,40),"Load game",title);
            GUI.Label(new Rect(280,170,700,32),"Choose a manual save or autosave. Loading replaces the current session.",body);
            string[] files=new string[0];try{files=Directory.GetFiles(Application.persistentDataPath,"earth-*.json").OrderByDescending(File.GetLastWriteTimeUtc).ToArray();}catch(Exception e){loadError=e.Message;}
            GUILayout.BeginArea(new Rect(280,215,720,365));loadScroll=GUILayout.BeginScrollView(loadScroll);
            if(files.Length==0)GUILayout.Label("No saved games found.",body);
            foreach(var file in files){
                string shown=Path.GetFileNameWithoutExtension(file);
                if(deleteCandidate==file){
                    GUILayout.BeginHorizontal();GUILayout.Label("Delete "+shown+"? This cannot be undone.",body);
                    if(GUILayout.Button("Yes, delete",button,GUILayout.Width(120),GUILayout.Height(30))){DeleteSaveFile(file);GUIUtility.ExitGUI();}
                    if(GUILayout.Button("Cancel",button,GUILayout.Width(90),GUILayout.Height(30)))deleteCandidate=null;
                    GUILayout.EndHorizontal();continue;
                }
                string label=shown+" — "+File.GetLastWriteTime(file).ToString("dd MMM yyyy HH:mm");
                GUILayout.BeginHorizontal();
                if(Button(label)){LoadFile(file);if(status.StartsWith("Could not load"))loadError=status;else{loadScreen=false;pauseMenu=false;GUIUtility.ExitGUI();}}
                if(GUILayout.Button("Delete",button,GUILayout.Width(90),GUILayout.Height(30)))deleteCandidate=file;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();GUILayout.EndArea();GUI.Label(new Rect(280,590,720,55),loadError,small);
            if(GUI.Button(new Rect(780,645,220,35),"Back",button)||(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.Escape)){loadScreen=false;GUI.FocusControl(null);GUIUtility.ExitGUI();}
        }
        // Deletes one saved game (and its backup copy). Only files in the saves folder named earth-*.json are accepted.
        void DeleteSaveFile(string file){
            try{
                string name=Path.GetFileName(file);
                if(!name.StartsWith("earth-")||!name.EndsWith(".json"))throw new Exception("Only saved games can be deleted.");
                if(File.Exists(file))File.Delete(file);
                if(File.Exists(file+".bak"))File.Delete(file+".bak");
                if(File.Exists(file+".tmp"))File.Delete(file+".tmp");
                loadError="Deleted "+Path.GetFileNameWithoutExtension(file)+".";
            }catch(Exception e){loadError="Could not delete: "+e.Message;}
            deleteCandidate=null;
        }
        void OpenDetails(string name){detailsScreen=name;diplomacyDiscuss=false;diplomacyChooser=name=="Diplomacy"&&!diplomacyDirect;diplomacyDirect=false;researchScreen=false;detailsScroll=Vector2.zero;ResetMapGesture();GUI.FocusControl(null);GUIUtility.ExitGUI();}
        void DrawDetailsScreen()
        {
            if(detailsScreen=="Diplomacy"){DrawDiplomacyScreen();return;}
            ChronicleStyles();GUI.Box(new Rect(24,100,1232,630),"");GUI.Label(new Rect(50,detailsScreen=="Chronicle"?107:115,800,detailsScreen=="Chronicle"?56:40),detailsScreen=="Atlas"?"Realms":detailsScreen=="Economy"?"Empire":detailsScreen,detailsScreen=="Chronicle"?chronicleTitle:title);
            if(GUI.Button(new Rect(1040,115,190,34),"Back",button)){detailsScreen="";GUI.FocusControl(null);GUIUtility.ExitGUI();}
            GUILayout.BeginArea(new Rect(55,175,1160,520));detailsScroll=GUILayout.BeginScrollView(detailsScroll);
            if(detailsScreen=="Economy"){
                var b=world.Budget(0);GUILayout.Label("Income +"+b[0]+" | Upkeep -"+b[1]+" | Net "+b[2],heading);
                DrawEmpireBalance();
                for(int r=2;r<World.ResourceNames.Length;r++)GUILayout.Label(World.ResourceNames[r]+": "+(world.HasResource(0,r)?"connected":"not connected"),body);
            GUILayout.Label(world.AgeLine(0),body);
            DrawEmpireEconomy();
            }else if(detailsScreen=="Civics"){DrawCivics();
            }else if(detailsScreen=="Settlement adviser"){DrawSettlementAdviser();
            }else if(detailsScreen=="Citizens"){DrawCitizenManagement();
            }else if(detailsScreen=="Religion"){DrawReligion();
            }else if(detailsScreen=="Victory"){DrawVictoryStatus();
            }else if(detailsScreen=="Identity"){
                GUI.SetNextControlName("rename");renameText=GUILayout.TextField(renameText,32);if(Button("Apply civilisation name"))world.civilisationName=World.CleanName(renameText);
                DrawFlagDesigner();
                {var me=world.nations[0];var face=Portrait(me.portrait);if(face!=null)DrawPortrait(GUILayoutUtility.GetRect(150,200),face);if(!string.IsNullOrEmpty(me.leader))GUILayout.Label("Led by "+me.leader,heading);GUILayout.Label(me.traits!=null&&me.traits.Any(t=>t>=0)?CivilisationCatalog.TraitLine(me.traits):"No traits.",body);}
                GUILayout.Label("Difficulty: "+DifficultyTable.Names[Mathf.Clamp(world.difficultyLevel,0,DifficultyTable.Names.Length-1)],heading);GUILayout.Label(DifficultyTable.Describe(Mathf.Clamp(world.difficultyLevel,0,DifficultyTable.Names.Length-1)),small);
                DrawOwnCivilisationLore();
            }else if(detailsScreen=="Diplomacy"){
                DrawAdvancedDiplomacy();
            }else if(detailsScreen=="Atlas"){DrawAtlas();
            }else if(detailsScreen=="Military"){DrawMilitaryAdviser();
            }else if(detailsScreen=="Cities"){
                DrawEmpireAdvisors();
                foreach(var c in world.cities.Where(c=>c.owner==0))if(Button(c.name+" — Population "+c.pop))OpenCity(c);
            }else {GUILayout.Label(world.result==""?"Your civilisation's history":world.result,chronicleHeading);foreach(var line in world.log)GUILayout.Label(line,chronicleText);}
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
        void DrawUnitLabels()
        {
            if(tileLabel==null)tileLabel=new GUIStyle(small){fontSize=10,alignment=TextAnchor.MiddleCenter,wordWrap=false};
            foreach(var stack in world.units.Where(u=>u.owner==0||world.Visible(u.x,u.y)).GroupBy(u=>new {u.x,u.y})){
                string label=World.StackLabel(stack);if(label=="")continue;var unit=stack.First();var cell=ClassicRect(unit.x,unit.y);float x=cell.x,y=cell.y;
                if(x<mapScroll.x-tilePixels||x>mapScroll.x+MapW||y<mapScroll.y-tilePixels||y>mapScroll.y+MapH)continue;
                float width=Mathf.Max(tilePixels,label.Length*6+6);var r=new Rect(x+(tilePixels-width)/2,y+cell.height-2,width,14);
                Fill(r,UiLabelBack);GUI.Label(r,label,tileLabel);
            }
        }
        // Stage 7 (CORE-DESIGN-16BIT.md): one city screen in three panels. Left, the 5 x 5 tile ring (click to lock a citizen);
        // middle, totals, growth, happiness and citizens; right, the build queue. The Governor sits along the bottom.
        bool cityBuildingTab;
        static readonly string[] GovernorNames={"Balanced","Growth (food)","Build (shields)","Science","Commerce (gold)"};
        void DrawCityScreen()
        {
            var c=world.cities.Find(a=>a.owner==0&&a.id==cityId);if(c==null){cityScreen=false;return;}
            GUI.Box(new Rect(24,100,1232,630),"");if(GUI.Button(new Rect(865,110,180,34),"Zoom to city",button)){cityScreen=false;GUI.FocusControl(null);if(world.Hex||diamondView)isoWidth=128;else tilePixels=96;FocusTile(c.x,c.y);GUIUtility.ExitGUI();}GUI.Label(new Rect(40,106,700,38),c.name+" — size "+c.pop+(world.Disorder(c)?"   DISORDER":"")+(c.occupation>0?"   occupied "+c.occupation+" turns":""),title);
            if(GUI.Button(new Rect(1055,110,180,34),"Back to map",button)){cityScreen=false;GUI.FocusControl(null);GUIUtility.ExitGUI();}
            DrawCityTiles(c,new Rect(40,150,440,520));
            DrawCityTotals(c,new Rect(495,150,370,520));
            GUI.Box(new Rect(880,150,355,520),"");
            if(GUI.Button(new Rect(890,158,165,28),(cityBuildingTab?"":"● ")+"Units",button))cityBuildingTab=false;
            if(GUI.Button(new Rect(1060,158,165,28),(cityBuildingTab?"● ":"")+"Buildings",button))cityBuildingTab=true;
            DrawProductionLine(c,cityBuildingTab,new Rect(880,190,355,480));
            GUI.Label(new Rect(40,682,110,30),"Governor",heading);
            c.focus=GUI.SelectionGrid(new Rect(150,678,1085,34),c.focus,GovernorNames,5);
            GUI.Label(new Rect(24,744,1232,42),status,body);
        }
        // The land around the city as one continuous piece of the pixel-art map (owner request: organic, no rigid cells with
        // text). Worked tiles carry small pips (green food, orange shields, gold trade); locked tiles a blue marker. Hover shows
        // a soft outline and a one-line description; click locks or releases a citizen. Land beyond the city's reach is dimmed.
        void DrawCityTiles(City c,Rect area)
        {
            GUI.Box(area,"");GUI.Label(new Rect(area.x+12,area.y+6,area.width-24,26),"The land around "+c.name,heading);
            var view=new Rect(area.x+8,area.y+36,area.width-16,374);
            // The tiles stay one size (fitted to a new city's land). As the city's influence spreads past the window, scroll bars
            // appear; the wheel (with Shift for sideways) and dragging with the right or middle button also move it (owner request).
            int reach=World.WorkRadius(c),show=world.TerritoryRadius(c)+1;const int FitRings=3;float tw=world.Hex?Mathf.Min(96f,Mathf.Floor(Mathf.Min(view.width/(2*FitRings+1),374f/(2*FitRings*PixelArt.HR/(float)PixelArt.HW+PixelArt.HH/(float)PixelArt.HW)))):Mathf.Min(96f,Mathf.Floor(2*374f/(2*FitRings+1))),th=world.Hex?tw*PixelArt.HH/PixelArt.HW:tw*PixelArt.DH/PixelArt.DW,s=tw/64f;var worked=world.Worked(c);var workable=world.Workable(c);var e=Event.current;
            float landW=world.Hex?(2*show+1)*tw:(2*show+1)*tw,landH=world.Hex?2*show*tw*PixelArt.HR/PixelArt.HW+th:(2*show+1)*th;
            bool scrollX=landW>view.width,scrollY=landH>view.height;if(scrollX||scrollY){scrollX=scrollY=true;view.width-=14;view.height-=14;}
            float maxX=Mathf.Max(0,(landW-view.width)/2),maxY=Mathf.Max(0,(landH-view.height)/2);
            if(view.Contains(e.mousePosition)&&(scrollX||scrollY)){
             if(e.type==EventType.ScrollWheel){if(e.shift)cityMapScroll.x+=e.delta.y*12;else cityMapScroll.y+=e.delta.y*12;e.Use();}
             if(e.type==EventType.MouseDrag&&(e.button==1||e.button==2)){cityMapScroll-=e.delta;e.Use();}
            }
            cityMapScroll.x=Mathf.Clamp(cityMapScroll.x,-maxX,maxX);cityMapScroll.y=Mathf.Clamp(cityMapScroll.y,-maxY,maxY);
            if(scrollX){cityMapScroll.x=GUI.HorizontalScrollbar(new Rect(view.x,view.yMax,view.width,14),cityMapScroll.x,view.width,-maxX,maxX+view.width);cityMapScroll.y=GUI.VerticalScrollbar(new Rect(view.xMax,view.y,14,view.height),cityMapScroll.y,view.height,-maxY,maxY+view.height);}
            int frame=(int)(Time.realtimeSinceStartup*2.5f)%3;string hover=null;Rect hoverRect=new Rect();
            GUI.BeginGroup(view);
            Fill(new Rect(0,0,view.width,view.height),scrollX?MistDeep:new Color(.2f,.25f,.31f));
            float cx=Mathf.Round(view.width/2-cityMapScroll.x),cy=Mathf.Round(view.height/2-cityMapScroll.y);var viewLocal=new Rect(0,0,view.width,view.height);
            for(int pass=0;pass<2;pass++)
            // Diagonal layout, back to front by diagonal so scenery overlaps the tiles behind it.
            // Hex maps go row by row instead.
            for(int sum=world.Hex?-show:-2*show;sum<=(world.Hex?show:2*show);sum++)for(int dx=-show;dx<=show;dx++)
            {
                int dy=world.Hex?sum:sum-dx;if(dy<-show||dy>show)continue;
                var t=world.StepIn(c.x,c.y,dx,dy,show);if(t==null||world.Distance(c.x,c.y,t.x,t.y)>show)continue;
                float sx=world.Hex?cx+(dx+dy*.5f)*tw:cx+(dx-dy)*tw/2,sy=world.Hex?cy+dy*tw*PixelArt.HR/PixelArt.HW:cy+(dx+dy)*th/2;var r=new Rect(sx-tw/2,sy-th/2,tw,th);
                if(r.xMax<0||r.x>view.width||r.yMax<0||r.y>view.height+th)continue;
                bool ring=world.Distance(c.x,c.y,t.x,t.y)<=reach&&(dx!=0||dy!=0?workable.Contains(t):true),mine=t.owner==c.owner;
                var known=t.seen?world.KnownTile(t):null;var old=GUI.color;
                if(pass==0)
                {
                    if(known==null){GUI.color=MistDeep;GUI.DrawTexture(r,IsoPicture(4,5,0));GUI.color=old;continue;}
                    GUI.DrawTexture(r,IsoGround(known,c.x+dx,c.y+dy,frame));
                    if(known.road){var road=RoadPicture(known);if(road!=null)GUI.DrawTexture(r,road);}
                    // Once the view scrolls there is no soft edge to fade the land out, so land outside the territory is shaded,
                    // and the territory carries the same bold edge as on the map.
                    if(known.owner!=c.owner&&scrollX){GUI.color=new Color(.04f,.05f,.08f,.55f);GUI.DrawTexture(r,IsoPicture(4,9,0));}
                    if(known.owner>=0){Color edgeInk=nations[known.owner];edgeInk.a=1;int[] oi=world.Hex?World.HexDi:new[]{0,1,0,-1},oj=world.Hex?World.HexDj:new[]{-1,0,1,0};for(int k=0;k<oi.Length;k++){var nb=world.Step(t.x,t.y,oi[k],oj[k]);if(nb!=null&&nb.seen&&world.KnownOwner(nb)==known.owner)continue;GUI.color=edgeInk;GUI.DrawTexture(r,world.Hex?IsoPicture(10,k,0):IsoPicture(4,k,0));}}
                    GUI.color=old;continue;
                }
                // Second pass: scenery, the city, and markers, so they stand over the row behind.
                if(known==null)continue;
                bool centre=dx==0&&dy==0;
                var sprite=PixelArt.IsWater(known.terrain)||centre?null:IsoPicture(1,known.terrain,IsoVariant(t.x,t.y));
                if(sprite!=null)GUI.DrawTexture(new Rect(sx-PixelArt.AX*s,sy-PixelArt.AY*s,64*s,64*s),sprite);
                if(centre)DrawCityArt(c,new Rect(sx-36,sy-52,72,72));
                GUI.color=old;
                // Resources around the city, with their own icons.
                if(known.resource>0)DrawResource(known,new Rect(sx-tw/2+tw*.55f,sy-th/2+2,tw*.45f,tw*.45f));
                // Tiles in reach that another of your cities works can be taken over by locking them here (owner request).
                bool reachable=centre||world.InReach(c,t);if(!reachable||!mine)continue;
                int[] v=centre?world.CityCentreYield(c):world.EffectiveTileYield(c,t);bool active=centre||worked.Contains(t),locked=world.IsLocked(c,t);
                if(active)DrawYieldPips(new Rect(sx-tw/2+5,sy+th/2-14,tw-10,10),v);
                if(locked){Fill(new Rect(sx+tw/2-16,sy-th/2+5,11,11),new Color(.08f,.16f,.3f,.9f));Fill(new Rect(sx+tw/2-14,sy-th/2+7,7,7),UiAccent);}
                if((world.Hex?PixelArt.InHex(e.mousePosition.x-sx,e.mousePosition.y-sy,tw):Mathf.Abs(e.mousePosition.x-sx)/(tw/2)+Mathf.Abs(e.mousePosition.y-sy)/(th/2)<=1)&&viewLocal.Contains(e.mousePosition)){
                    hoverRect=r;hover=World.TerrainNames[known.terrain]+(known.resource>0?" with "+World.ResourceNames[known.resource]:"")+": "+v[0]+" food, "+v[1]+" shields, "+v[2]+" trade — "+(centre?"city centre":locked?"locked citizen (click to release)":active?"worked (click to lock)":"not worked (click to lock a citizen here)");
                    if(!ring&&!centre){var ctl=world.TileController(t,c.owner);if(ctl!=null&&ctl!=c)hover+=" — worked by "+ctl.name+"; click to take it over";}
                    if(!centre&&e.type==EventType.MouseDown&&e.button==0&&GUI.enabled){bool ok=world.ToggleLock(c,t);status=ok?(world.IsLocked(c,t)?"Citizen locked on "+World.TerrainNames[known.terrain]+".":"Tile released to the Governor."):"Cannot lock: every working citizen is already locked here.";e.Use();}
                }
            }
            // A soft round vignette, rather than dimmed squares, fades the land beyond the city's reach.
            if(!scrollX)GUI.DrawTexture(new Rect(0,0,view.width,view.height),CityVignette());
            if(hover!=null){var h=hoverRect;Color glow=new Color(1,1,1,.85f);Fill(new Rect(h.x+3,h.y,h.width-6,2),glow);Fill(new Rect(h.x+3,h.yMax-2,h.width-6,2),glow);Fill(new Rect(h.x,h.y+3,2,h.height-6),glow);Fill(new Rect(h.xMax-2,h.y+3,2,h.height-6),glow);}
            GUI.EndGroup();
            GUI.Label(new Rect(area.x+12,area.y+414,area.width-24,40),hover??"Hover over the land to see what it yields. Click to lock a citizen on a tile; the Governor works the rest.",small);
            float lx=area.x+12,ly=area.y+466;
            DrawYieldPips(new Rect(lx,ly+6,30,10),new[]{1,0,0});GUI.Label(new Rect(lx+12,ly,60,22),"food",small);
            DrawYieldPips(new Rect(lx+70,ly+6,30,10),new[]{0,1,0});GUI.Label(new Rect(lx+82,ly,70,22),"shields",small);
            DrawYieldPips(new Rect(lx+150,ly+6,30,10),new[]{0,0,1});GUI.Label(new Rect(lx+162,ly,60,22),"trade",small);
            Fill(new Rect(lx+222,ly+6,9,9),UiAccent);GUI.Label(new Rect(lx+236,ly,140,22),"locked citizen",small);
            // How far until the city's influence next spreads (owner request): a bar with the numbers on it.
            {var g=world.NextBorderGrowth(c);var bar=new Rect(lx,area.y+503,area.width-24,14);
             string growthLine=g.full?"Influence "+g.radius+" tiles: fully expanded":"Influence "+g.radius+" tiles: next ring "+(g.turnsLeft>=0?"in "+g.turnsLeft+(g.turnsLeft==1?" turn":" turns")+", or at size "+g.citizensAt+" if sooner":"at size "+g.citizensAt);
             GUI.Label(new Rect(lx,area.y+484,area.width-24,20),growthLine,small);
             EraBar(bar,g.full?1f:g.done/(float)Math.Max(1,g.span));
             if(!g.full){string count=g.done+" / "+g.span+(g.turnsLeft>=0?" turns":" citizens");var countStyle=new GUIStyle(hudCentre){fontSize=11};float cw=countStyle.CalcSize(new GUIContent(count)).x+10;var plate=new Rect(bar.center.x-cw/2,bar.y,cw,bar.height);Fill(plate,new Color(.04f,.06f,.12f,.72f));GUI.Label(plate,count,countStyle);}}
        }
        Texture2D cityVignette;
        // Clear in the middle, shading smoothly to the panel's slate colour towards the edges (an ellipse, not a square).
        Texture2D CityVignette(){
            if(cityVignette!=null)return cityVignette;cityVignette=new Texture2D(64,48,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            for(int y=0;y<48;y++)for(int x=0;x<64;x++){float u=(x+.5f-32)/32f,v=(y+.5f-24)/24f,d=Mathf.Sqrt(u*u+v*v),t=Mathf.Clamp01((d-.95f)/.4f);cityVignette.SetPixel(x,y,new Color(.2f,.25f,.31f,t*t*(3-2*t)*.82f));}
            cityVignette.Apply();themeTextures.Add(cityVignette);return cityVignette;
        }
        // Small square pips in a row: green food, orange shields, gold trade (at most six of each).
        void DrawYieldPips(Rect r,int[] v)
        {
            Color[] colours={new Color(.42f,.8f,.32f),new Color(.93f,.55f,.22f),new Color(1f,.83f,.25f)};float x=r.x;
            for(int k=0;k<3;k++){for(int n=0;n<Math.Min(6,v[k]);n++){Fill(new Rect(x,r.y,8,8),new Color(.1f,.1f,.1f,.85f));Fill(new Rect(x+1,r.y+1,6,6),colours[k]);x+=8;}if(v[k]>0)x+=4;}
        }        void DrawBar(Rect r,float fraction,Color colour){Fill(r,UiTrough);Fill(new Rect(r.x,r.y,r.width*Mathf.Clamp01(fraction),r.height),colour);}
        void DrawCityTotals(City c,Rect area)
        {
            GUI.Box(area,"");float x=area.x+12,w=area.width-24,y=area.y+8;
            GUI.SetNextControlName("cityrename");c.name=GUI.TextField(new Rect(x,y,w,28),c.name,32);y+=38;
            var v=world.Yield(c);int trade=world.Trade(c),need=world.FoodNeeded(c);
            GUI.Label(new Rect(x,y,w,24),"Food "+(v[0]>=0?"+":"")+v[0]+"   Shields "+v[1]+"   Trade "+trade,heading);y+=28;
            GUI.Label(new Rect(x,y,w,22),"Trade at "+world.nations[0].scienceRate+"% science: Science "+v[2]+"  Gold "+v[3]+(world.nations[0].militaryRate>0?"  Army +"+world.ArmyBudget(c)+" shields":"")+(world.LinkTrade(c)>0?"  (joined cities +"+world.LinkTrade(c)+" Trade)":""),small);y+=26;
            GUI.Label(new Rect(x,y,w,22),"Faith +"+world.FaithYield(c)+"  •  "+(c.religion>=0?"Follows "+ReligionCatalog.Name(c.religion)+(world.IsHolyCity(c)?" (holy city)":""):"No majority faith")+"  •  Influence "+world.CityInfluence(c),small);y+=22;
            GUI.Label(new Rect(x,y,w,22),"Growth  "+c.food+" / "+need+" food",body);y+=22;DrawBar(new Rect(x,y,w,12),c.food/(float)Math.Max(1,need),new Color(.45f,.75f,.35f));y+=16;
            GUI.Label(new Rect(x,y,w,40),world.GrowthStatus(c),small);y+=42;
            int content=world.ContentCitizens(c),unhappy=world.UnhappyCitizens(c),workers=Math.Max(1,world.Workers(c));
            GUI.Label(new Rect(x,y,w,22),"Content "+content+"   Unhappy "+unhappy+(world.Disorder(c)?"   — disorder: no shields or gold":""),body);y+=22;
            Fill(new Rect(x,y,w,12),UiTrough);Fill(new Rect(x,y,w*content/workers,12),new Color(.4f,.72f,.5f));Fill(new Rect(x+w*content/workers,y,w*unhappy/workers,12),new Color(.9f,.45f,.35f));y+=22;
            GUI.Label(new Rect(x,y,w,24),"Citizens",heading);y+=26;
            GUI.Label(new Rect(x,y,w,22),"Working "+world.Workers(c)+"  Entertainers "+c.entertainers+"  Scientists "+c.scientists+"  Tax collectors "+c.merchants,small);y+=24;
            bool enabled=GUI.enabled;float half=(w-6)/2;
            GUI.enabled=enabled&&c.entertainers>0;
            if(GUI.Button(new Rect(x,y,half,26),"Entertainer → Scientist",button))world.ConvertSpecialist(c,2,0);
            if(GUI.Button(new Rect(x+half+6,y,half,26),"Entertainer → Tax collector",button))world.ConvertSpecialist(c,2,1);y+=30;
            GUI.enabled=enabled&&c.scientists+c.merchants>0;
            if(GUI.Button(new Rect(x,y,half,26),"Scientist → Entertainer",button))world.ConvertSpecialist(c,0,2);
            if(GUI.Button(new Rect(x+half+6,y,half,26),"Tax collector → Entertainer",button))world.ConvertSpecialist(c,1,2);y+=30;
            GUI.enabled=enabled;
            // Specialists can always be sent back to work the land (owner request: a size 1 city with its only citizen a
            // specialist works no tile and cannot grow).
            GUI.enabled=enabled&&world.Workers(c)>0;if(GUI.Button(new Rect(x,y,half,26),"Worker → Entertainer",button))world.AdjustSpecialist(c,2,1);
            GUI.enabled=enabled&&c.entertainers+c.scientists+c.merchants>0;if(GUI.Button(new Rect(x+half+6,y,half,26),"Specialist → back to work",button))world.ReturnToWork(c);GUI.enabled=enabled;y+=32;
            GUI.Label(new Rect(x,y,w,36),"Entertainers +2 content • Scientists +3 science\nTax collectors +3 gold",small);y+=38;
            GUI.Label(new Rect(x,y,w,24),"Buildings",heading);y+=26;
            var buildings=World.BuildingIds.Where(b=>world.HasBuilding(c,b)).Select(World.BuildName).ToArray();
            GUI.Label(new Rect(x,y,w,area.yMax-y-6),buildings.Length==0?"None yet.":string.Join(", ",buildings),small);
        }        void DrawProductionLine(City c,bool building,Rect area)
        {
            GUI.Box(area,"");int current=building?c.buildingProject:c.build,progress=building?c.buildingProduction:c.production;var queue=building?c.buildingQueue:c.queue;
            GUI.Label(new Rect(area.x+10,area.y+8,area.width-20,25),(building?"BUILDINGS":"UNIT TRAINING")+" — "+queue.Count+" queued",heading);
            // The next three items with their cost and turns left (turns count up through the queue).
            int output=building||current<0?Math.Max(1,world.Yield(c)[1]):world.LineOutput(c,current),turns=0;var lines=new System.Collections.Generic.List<string>();
            if(current<0)lines.Add("Idle — choose below");
            else{turns=Math.Max(1,(world.CostFor(c,current)-progress+output-1)/output);lines.Add("1. "+world.BuildNameFor(c.owner,current)+"  "+progress+" / "+world.CostFor(c,current)+" — "+turns+" turns");var reason=world.BuildReason(c,current);if(reason!="")lines.Add("   Paused: "+reason);}
            for(int i=0;i<queue.Count;i++){turns+=Math.Max(1,(world.CostFor(c,queue[i])+output-1)/output);lines.Add((i+2)+". "+world.BuildNameFor(c.owner,queue[i])+"  "+world.CostFor(c,queue[i])+" — "+turns+" turns");}
            // The list stays in the space above Rush; a longer one gets a scroll bar there (owner request).
            {var listRect=new Rect(area.x+10,area.y+32,area.width-20,70);string listText=string.Join("\n",lines.ToArray());float listH=small.CalcHeight(new GUIContent(listText),listRect.width-18);
             if(listH<=listRect.height)GUI.Label(listRect,listText,small);
             else{var listScroll=building?buildingListScroll:unitListScroll;listScroll=GUI.BeginScrollView(listRect,listScroll,new Rect(0,0,listRect.width-18,listH));GUI.Label(new Rect(0,0,listRect.width-18,listH),listText,small);GUI.EndScrollView();if(building)buildingListScroll=listScroll;else unitListScroll=listScroll;}}
            if(current>=0&&GUI.Button(new Rect(area.x+10,area.y+105,area.width-20,24),"Rush: "+world.RushPrice(c,building)+" gold",button))status=world.Rush(c,building)?"Paid; completes next turn if space permits.":"Cannot rush: insufficient gold, blocked, or already funded.";
            var e=Event.current;if(e.type==EventType.MouseDown&&e.button==1&&e.control&&GUI.enabled&&world.result==""&&area.Contains(e.mousePosition)){world.RemoveLastProject(c,building);status="Removed the last "+(building?"building":"unit")+" order.";e.Use();GUIUtility.ExitGUI();}

            // Only what the civilisation's current technology allows (and is not already built) is listed (owner request).
            int[] options=(building?World.BuildingIds:Enumerable.Range(0,World.UnitNames.Length).Select(World.UnitBuild).ToArray()).Where(p=>world.TechAllows(c,p)).ToArray();
            Vector2 scroll=building?buildingQueueScroll:unitQueueScroll;
            scroll=GUI.BeginScrollView(new Rect(area.x+8,area.y+130,area.width-16,area.height-140),scroll,new Rect(0,0,area.width-34,options.Length*67+4));
            for(int i=0;i<options.Length;i++){
                int project=options[i];float y=i*67;DrawIcon(new Rect(4,y+5,26,26),building?12+Array.IndexOf(World.BuildingIds,project):-1);
                if(!building)GUI.DrawTexture(new Rect(4,y+5,26,26),people[World.BuildUnit(project)]);
                var r=new Rect(37,y,area.width-78,32);string reason=world.BuildReason(c,project);bool enabled=GUI.enabled;bool ordered=building&&(current==project||queue.Contains(project));
                // A building already chosen is greyed out until it is built; right-clicking it still cancels it (owner request).
                if(ordered&&e.type==EventType.MouseDown&&e.button==1&&!e.control&&r.Contains(e.mousePosition)&&world.result==""){world.CancelProject(c,project);status="Cancelled "+world.BuildNameFor(c.owner,project)+".";e.Use();GUIUtility.ExitGUI();}
                GUI.enabled=enabled&&reason==""&&world.result==""&&!ordered;
                bool append=e.control;if(GUI.Button(r,world.BuildNameFor(c.owner,project),button)){bool ok=world.ChooseProject(c,project,append);status=ok?(append?"Added to queue.":"Selected one "+world.BuildNameFor(c.owner,project)+"."):"Already built/queued or queue full.";GUIUtility.ExitGUI();}
                GUI.enabled=enabled;GUI.Label(new Rect(37,y+34,area.width-78,31),reason!=""?reason:world.CostFor(c,project)+" production"+(building?" | "+World.BuildingEffect(project):"")+(ordered?" | Ordered: right-click to cancel":""),small);
            }
            GUI.EndScrollView();if(building)buildingQueueScroll=scroll;else unitQueueScroll=scroll;
        }
        void DrawResearchScreen(){DrawExpandedResearch();}
        // Distinct original 8x8 pixel glyphs, drawn with the existing code-based art system.
        static readonly string[] IconPatterns={
            "...#..../.#.#.#../..###.../.#.##.#./..###.../...#..../...#..../..###...", // wheat
            "..##..../.#..#.../....#.../#####.../....#.../.#..#.../..##..../........", // bow
            "######../#.#..#../#.##.#../#.#..#../#.##.#../#.#..#../######../........", // book
            "..##..../.#..#.../##..##../#....#../########/.#..#.../..##..../........", // gear
            "#.#.#.../#####.../#...#.../#.###.../#.#.#.../#####.../########/........", // factory
            "....##../...##.../..##..../.#####../...##.../..##..../.##...../........", // lightning
            "...#..../..###.../.#.#.#../...#..../...#..../..###.../.#####../........", // spear
            "..###.../.#..##../##...#../..####../..#..#../.#...#../##...##./........", // horse
            "...##.../..##..../.##...../#####.../...#..../#######./.#...#../........", // catapult
            "..##..../..##..../######../######../..##..../..##..../........ /........", // cross
            "..###.../.#...#../#..#..#./#.#.#.#./#..#..#./.#...#../..###.../........", // coin
            "######../#....#../#.##.#../#....#../#.##.#../#....#../######../...##...", // charter
            "...#..../..###.../.#####../#######./#.#.#.#./#.#.#.#./#######./........", // granary
            "#..#..#./##.##.#./#..#..#./#######./#..#..#./##.##.#./#######./........", // library
            "##.##.##/########/#..#..#./########/..#..#../########/#..#..#./........", // walls
            ".###..../..#...../..#..#../..####../..#..#../.###.#../.....#../........", // workshop
            "########/.#.#.#.#/########/.#....#./.#.##.#./.#.##.#./.######./........", // market
            "#.....#./.#...#../..#.#.../...#..../..#.#.../.#...#../#.....#./........", // barracks
            "..#..#../.###.##./..#..#../....#.../.#..#.../###.#.../.#..#.../########", // gardens
            "...#..../#######./.#.#.#../.#.#.#../#..#..#./#######./...#..../.#####..", // courthouse
            "########/#..##..#/#..##..#/#.####.#/#..##..#/#..##..#/########/........" // hospital
        };
        void DrawIcon(Rect rect,int id){if(id>=IconPatterns.Length){DrawEmpireGlyph(rect,id);return;}if(id<0)return;Fill(rect,new Color(.12f,.19f,.22f));string pixels=IconPatterns[id].Replace("/","").Replace(" ","");float size=rect.width/8;for(int i=0;i<Math.Min(64,pixels.Length);i++)if(pixels[i]=='#')Fill(new Rect(rect.x+i%8*size,rect.y+i/8*size,size,size),new Color(.87f,.76f,.43f));}
    }
}
