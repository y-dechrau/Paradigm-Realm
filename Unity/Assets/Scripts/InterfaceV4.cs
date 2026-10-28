using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    public partial class RealmGame
    {
        bool loadScreen,researchScreen;
        string detailsScreen="",loadError="";
        Vector2 loadScroll,researchScroll,detailsScroll,cityMapScroll,builtScroll,unitQueueScroll,buildingQueueScroll;
        GUIStyle tileLabel;
        void OpenLoadScreen(){loadScreen=true;loadError="";loadScroll=Vector2.zero;ResetMapGesture();GUI.FocusControl(null);GUIUtility.ExitGUI();}
        void DrawLoadScreen()
        {
            GUI.Box(new Rect(250,100,780,600),"");GUI.Label(new Rect(280,125,600,40),"Load game",title);
            GUI.Label(new Rect(280,170,700,32),"Choose a manual save or autosave. Loading replaces the current session.",body);
            string[] files=new string[0];try{files=Directory.GetFiles(Application.persistentDataPath,"earth-*.json").OrderByDescending(File.GetLastWriteTimeUtc).ToArray();}catch(Exception e){loadError=e.Message;}
            GUILayout.BeginArea(new Rect(280,215,720,365));loadScroll=GUILayout.BeginScrollView(loadScroll);
            if(files.Length==0)GUILayout.Label("No saved games found.",body);
            foreach(var file in files){string label=Path.GetFileNameWithoutExtension(file)+" — "+File.GetLastWriteTime(file).ToString("dd MMM yyyy HH:mm");
                if(Button(label)){LoadFile(file);if(status.StartsWith("Could not load"))loadError=status;else{loadScreen=false;pauseMenu=false;GUIUtility.ExitGUI();}}}
            GUILayout.EndScrollView();GUILayout.EndArea();GUI.Label(new Rect(280,590,720,55),loadError,small);
            if(GUI.Button(new Rect(780,645,220,35),"Back",button)||(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.Escape)){loadScreen=false;GUI.FocusControl(null);GUIUtility.ExitGUI();}
        }
        void OpenDetails(string name){detailsScreen=name;researchScreen=false;detailsScroll=Vector2.zero;ResetMapGesture();GUI.FocusControl(null);GUIUtility.ExitGUI();}
        void DrawGameHeader()
        {
            bool enabled=GUI.enabled;GUI.enabled=!guide&&!confirmNew&&pendingUnit==-1;
            GUI.Label(new Rect(24,12,630,32),world.civilisationName,title);
            GUI.Label(new Rect(24,51,490,37),world.Calendar()+" | Turn "+world.turn+" | Gold "+world.gold+" ("+world.Budget(0)[2]+"/turn)",body);
            if(GUI.Button(new Rect(680,15,100,30),"Menu",button)){mainMenu=true;GUIUtility.ExitGUI();}
            if(GUI.Button(new Rect(795,15,90,30),"Save",button))Save();
            if(GUI.Button(new Rect(895,15,90,30),"Load",button))OpenLoadScreen();
            if(GUI.Button(new Rect(995,15,110,30),"New world",button)){mainMenu=true;confirmNew=true;GUIUtility.ExitGUI();}
            if(GUI.Button(new Rect(1115,15,110,30),"Guide",button)){cityScreen=false;researchScreen=false;detailsScreen="";guide=true;GUIUtility.ExitGUI();}
            if(GUI.Button(new Rect(510,54,140,32),"Research",button)){researchScreen=true;detailsScreen="";ResetMapGesture();GUI.FocusControl(null);GUIUtility.ExitGUI();}
            string[] names={"Economy","Diplomacy","Identity","Cities","Chronicle"};
            for(int i=0;i<names.Length;i++)if(GUI.Button(new Rect(660+i*116,54,110,32),names[i]+(names[i]=="Diplomacy"&&world.offers.Any(o=>o.to==0&&o.status==0&&o.expires>world.turn)?" ("+world.offers.Count(o=>o.to==0&&o.status==0&&o.expires>world.turn)+")":""),button))OpenDetails(names[i]);
            GUI.enabled=enabled;
        }
        void DrawDetailsScreen()
        {
            GUI.Box(new Rect(24,100,1232,630),"");GUI.Label(new Rect(50,115,800,40),detailsScreen,title);
            if(GUI.Button(new Rect(1040,115,190,34),"Back",button)){detailsScreen="";GUI.FocusControl(null);GUIUtility.ExitGUI();}
            GUILayout.BeginArea(new Rect(55,175,1160,520));detailsScroll=GUILayout.BeginScrollView(detailsScroll);
            if(detailsScreen=="Economy"){
                var b=world.Budget(0);GUILayout.Label("Income +"+b[0]+" | Upkeep -"+b[1]+" | Net "+b[2],heading);
                GUILayout.Label("Tax policy",body);world.nations[0].tax=GUILayout.SelectionGrid(world.nations[0].tax,new[]{"Low","Normal","High"},3);
                GUILayout.Label("Low: less tax, +2 happiness. High: more tax, -2 happiness. Government determines free unit upkeep.",body);
                for(int r=2;r<World.ResourceNames.Length;r++)GUILayout.Label(World.ResourceNames[r]+": "+(world.HasResource(0,r)?"connected":"not connected"),body);
            DrawEmpireEconomy();
            }else if(detailsScreen=="Settlement adviser"){DrawSettlementAdviser();
            }else if(detailsScreen=="Citizens"){DrawCitizenManagement();
            }else if(detailsScreen=="Identity"){
                GUI.SetNextControlName("rename");renameText=GUILayout.TextField(renameText,32);if(Button("Apply civilisation name"))world.civilisationName=World.CleanName(renameText);
                world.flag=GUILayout.SelectionGrid(world.flag,new[]{"Leaf","Star","Sun"},3);
            }else if(detailsScreen=="Diplomacy"){
                DrawAdvancedDiplomacy();
            }else if(detailsScreen=="Atlas"){DrawAtlas();
            }else if(detailsScreen=="Military"){DrawMilitaryAdviser();
            }else if(detailsScreen=="Cities"){
                DrawEmpireAdvisors();
                foreach(var c in world.cities.Where(c=>c.owner==0))if(Button(c.name+" — Population "+c.pop))OpenCity(c);
            }else {GUILayout.Label(world.result==""?"Your civilisation's history":world.result,heading);foreach(var line in world.log)GUILayout.Label(line,body);}
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
        void DrawUnitLabels()
        {
            if(tileLabel==null)tileLabel=new GUIStyle(small){fontSize=10,alignment=TextAnchor.MiddleCenter,wordWrap=false};
            foreach(var stack in world.units.Where(u=>u.owner==0||world.Visible(u.x,u.y)).GroupBy(u=>new {u.x,u.y})){
                string label=World.StackLabel(stack);if(label=="")continue;var unit=stack.First();float x=unit.x*tilePixels,y=unit.y*tilePixels;
                if(x<mapScroll.x-tilePixels||x>mapScroll.x+852||y<mapScroll.y-tilePixels||y>mapScroll.y+624)continue;
                float width=Mathf.Max(tilePixels,label.Length*6+6);var r=new Rect(x+(tilePixels-width)/2,y+tilePixels-2,width,14);
                Fill(r,new Color(.025f,.045f,.055f,.92f));GUI.Label(r,label,tileLabel);
            }
        }
        void DrawCityScreen()
        {
            var c=world.cities.Find(a=>a.owner==0&&a.id==cityId);if(c==null){cityScreen=false;return;}
            GUI.Box(new Rect(24,100,1232,630),"");GUI.Label(new Rect(40,110,800,34),c.name+" — Population "+c.pop,title);
            if(GUI.Button(new Rect(1055,110,180,34),"Back to map",button)){cityScreen=false;GUI.FocusControl(null);GUIUtility.ExitGUI();}
            var yield=world.Yield(c);GUI.Label(new Rect(40,150,1170,30),"Food "+c.food+" ("+yield[0]+"/turn) | Production "+yield[1]+" per line | Science "+yield[2]+" | Happiness "+world.Happiness(c)+" | "+(world.Connected(c)?"Connected":"Isolated")+(c.occupation>0?" | Occupied "+c.occupation+" turns":""),body);
            PrepareTerrainView();DrawCityTiles(c);DrawBuiltBuildings(c);
            GUI.SetNextControlName("cityrename");c.name=GUI.TextField(new Rect(490,185,330,30),c.name,32);
            c.focus=GUI.SelectionGrid(new Rect(840,185,390,30),c.focus,new[]{"Balanced","Growth","Production","Science","Commerce"},5);
            if(GUI.Button(new Rect(490,220,180,30),"Citizens / specialists",button)){managedCity=c.id;cityScreen=false;OpenDetails("Citizens");}
            GUI.Label(new Rect(680,220,550,42),"Click: select one. Ctrl+left-click: queue. Ctrl+right-click anywhere in a line: remove last. One of each building per city.",small);
            DrawProductionLine(c,false,new Rect(485,270,365,438));
            DrawProductionLine(c,true,new Rect(870,270,365,438));
            GUI.Label(new Rect(24,744,1232,42),status,body);
        }
        void DrawCityTiles(City c)
        {
            GUI.Label(new Rect(40,184,430,28),"City influence — tile contributions",heading);
            int radius=Math.Min(4,2+c.age/25),side=radius*2+1;float size=80;
            var worked=world.Worked(c);cityMapScroll=GUI.BeginScrollView(new Rect(40,218,430,310),cityMapScroll,new Rect(0,0,side*size,side*size));
            for(int dy=-radius;dy<=radius;dy++)for(int dx=-radius;dx<=radius;dx++){
                var t=world.At(c.x+dx,c.y+dy);var r=new Rect((dx+radius)*size,(dy+radius)*size,size-2,size-2);if(t==null)continue;
                bool centre=dx==0&&dy==0,active=worked.Contains(t),influence=t.owner==c.owner;
                if(!t.seen){Fill(r,new Color(.04f,.07f,.09f));GUI.Label(r,"Unknown",small);continue;}
                var known=world.KnownTile(t);GUI.DrawTexture(r,TerrainAppearance(known));if(centre)DrawCityArt(c,new Rect(r.x+8,r.y-3,62,62));
                if(!influence)Fill(r,new Color(0,0,0,.6f));
                if(influence){int[] v=centre?world.CityCentreYield(c):active?world.EffectiveTileYield(c,t):new[]{0,0,0};
                    Fill(new Rect(r.x,r.y+39,r.width,39),new Color(.02f,.04f,.05f,.9f));
                    GUI.Label(new Rect(r.x+3,r.y+39,r.width-4,38),"F "+v[0]+"  P "+v[1]+"\nG "+v[2]+(centre?"  City":active?"  Worked":"  Unworked"),small);
                    Fill(new Rect(r.x,r.y,r.width,2),active||centre?Color.yellow:new Color(.5f,.6f,.5f));
                }
            }
            GUI.EndScrollView();GUI.Label(new Rect(40,533,430,44),world.GrowthStatus(c),small);
        }
        void DrawBuiltBuildings(City c)
        {
            GUI.Box(new Rect(40,580,430,128),"");GUI.Label(new Rect(50,584,400,24),"Constructed buildings",heading);
            var buildings=World.BuildingIds.Where(b=>world.HasBuilding(c,b)).ToArray();builtScroll=GUI.BeginScrollView(new Rect(50,614,410,85),builtScroll,new Rect(0,0,390,Math.Max(75,buildings.Length*34)));
            if(buildings.Length==0)GUI.Label(new Rect(0,0,380,25),"None constructed yet.",body);
            for(int i=0;i<buildings.Length;i++){DrawIcon(new Rect(2,i*34,26,26),12+Array.IndexOf(World.BuildingIds,buildings[i]));GUI.Label(new Rect(40,i*34,340,30),World.BuildName(buildings[i]),body);}
            GUI.EndScrollView();
        }
        void DrawProductionLine(City c,bool building,Rect area)
        {
            GUI.Box(area,"");int current=building?c.buildingProject:c.build,progress=building?c.buildingProduction:c.production;var queue=building?c.buildingQueue:c.queue;
            GUI.Label(new Rect(area.x+10,area.y+8,area.width-20,25),(building?"BUILDINGS":"UNIT TRAINING")+" — "+queue.Count+" queued",heading);
            string message=current<0?"Idle":World.BuildName(current)+"  "+progress+" / "+world.Cost(current);
            if(current>=0){int output=world.Yield(c)[1];message+=" — "+Math.Max(0,(world.Cost(current)-progress+output-1)/output)+" turns";var reason=world.BuildReason(c,current);if(reason!="")message+="\nPaused: "+reason;}
            GUI.Label(new Rect(area.x+10,area.y+37,area.width-20,48),message,small);
            if(current>=0&&GUI.Button(new Rect(area.x+10,area.y+105,area.width-20,24),"Rush: "+world.RushPrice(c,building)+" gold",button))status=world.Rush(c,building)?"Paid; completes next turn if space permits.":"Cannot rush: insufficient gold, blocked, or already funded.";
            var e=Event.current;if(e.type==EventType.MouseDown&&e.button==1&&e.control&&GUI.enabled&&world.result==""&&area.Contains(e.mousePosition)){world.RemoveLastProject(c,building);status="Removed the last "+(building?"building":"unit")+" order.";e.Use();GUIUtility.ExitGUI();}
            GUI.Label(new Rect(area.x+10,area.y+82,area.width-20,22),"Next: "+(queue.Count==0?"none":string.Join(" → ",queue.Take(3).Select(World.BuildName).ToArray())+(queue.Count>3?" +"+(queue.Count-3):"")),small);
            int[] options=building?World.BuildingIds:Enumerable.Range(0,World.UnitNames.Length).Select(World.UnitBuild).ToArray();
            Vector2 scroll=building?buildingQueueScroll:unitQueueScroll;
            scroll=GUI.BeginScrollView(new Rect(area.x+8,area.y+130,area.width-16,area.height-140),scroll,new Rect(0,0,area.width-34,options.Length*67+queue.Count*26+62));
            for(int i=0;i<options.Length;i++){
                int project=options[i];float y=i*67;DrawIcon(new Rect(4,y+5,26,26),building?12+i:-1);
                if(!building)GUI.DrawTexture(new Rect(4,y+5,26,26),people[World.BuildUnit(project)]);
                var r=new Rect(37,y,area.width-78,32);string reason=world.BuildReason(c,project);bool enabled=GUI.enabled;GUI.enabled=enabled&&reason==""&&world.result=="";
                bool append=e.control;if(GUI.Button(r,World.BuildName(project),button)){bool ok=world.ChooseProject(c,project,append);status=ok?(append?"Added to queue.":"Selected one "+World.BuildName(project)+"."):"Already built/queued or queue full.";GUIUtility.ExitGUI();}
                GUI.enabled=enabled;GUI.Label(new Rect(37,y+34,area.width-78,31),reason!=""?reason:world.Cost(project)+" production"+(building?" | "+World.BuildingEffect(project):"")+(building&&(current==project||queue.Contains(project))?" | Ordered":""),small);
            }
            float qy=options.Length*67;GUI.Label(new Rect(5,qy,area.width-40,25),"Waiting queue ("+queue.Count+" / 8)",heading);
            if(queue.Count==0)GUI.Label(new Rect(5,qy+28,area.width-40,24),"Empty",small);
            for(int i=0;i<queue.Count;i++)GUI.Label(new Rect(5,qy+28+i*26,area.width-40,25),(i+1)+". "+World.BuildName(queue[i]),body);
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
