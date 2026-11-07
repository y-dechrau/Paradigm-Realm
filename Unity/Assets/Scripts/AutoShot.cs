using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    // Development aid only. Launching the game with "-autoShot <folder>" starts a fixed-seed new game, founds the capital,
    // saves screenshots of the map, a selected unit, the city screen and the economy screen, then quits.
    // Without that argument nothing here runs.
    public partial class RealmGame
    {
        void Start()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-autoShot");
            if(i>=0&&i+1<args.Length)StartCoroutine(AutoShot(args[i+1]));
            i=Array.IndexOf(args,"-frameProbe");if(i>=0&&i+1<args.Length)StartCoroutine(FrameProbe(args[i+1],i+2<args.Length?int.Parse(args[i+2]):4,i+3<args.Length?int.Parse(args[i+3]):120));
            i=Array.IndexOf(args,"-unitProbe");if(i>=0&&i+2<args.Length)StartCoroutine(UnitProbe(args[i+1],args[i+2]));
            i=Array.IndexOf(args,"-routeProbe");if(i>=0&&i+1<args.Length)StartCoroutine(RouteProbe(args[i+1]));
            i=Array.IndexOf(args,"-inputProbe");if(i>=0&&i+1<args.Length)StartCoroutine(InputProbeRun(args[i+1]));
            i=Array.IndexOf(args,"-flickerProbe");if(i>=0&&i+1<args.Length)StartCoroutine(FlickerProbe(args[i+1]));
        }
        IEnumerator Frames(int n){for(int k=0;k<n;k++)yield return null;}
        // Real input for the autoShot run: the next OnGUI handles this event as if the player had done it.
        Event probeEvent;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);
        [System.Runtime.InteropServices.DllImport("user32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode)] static extern IntPtr FindWindow(string c,string t);
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
        // Mouse events cannot be injected into OnGUI (groups forget the position), so they go to this game's own window as Windows messages.
        void PostMouse(Event e){var h=GetActiveWindow();if(h==IntPtr.Zero)h=FindWindow(null,Application.productName);var at=new IntPtr(((int)e.mousePosition.y<<16)|((int)e.mousePosition.x&0xffff));PostMessage(h,0x0200,IntPtr.Zero,at);bool left=e.button==0;uint msg=e.type==EventType.MouseDown?(left?0x0201u:0x0204u):(left?0x0202u:0x0205u);PostMessage(h,msg,new IntPtr(e.type==EventType.MouseDown?(left?1:2):0),at);}
#else
        void PostMouse(Event e){probeEvent=e;}
#endif
        IEnumerator Send(Event e){if(e.isMouse)PostMouse(e);else probeEvent=e;yield return Frames(3);}
        static Event Mouse(EventType type,float x,float y,int button=0,int clicks=1){return new Event{type=type,mousePosition=new Vector2(x,y),button=button,clickCount=clicks};}
        IEnumerator ClickAt(float x,float y,int button=0,int clicks=1){yield return Send(Mouse(EventType.MouseDown,x,y,button,clicks));yield return Send(Mouse(EventType.MouseUp,x,y,button,clicks));}
        void Probe(string name,bool ok){Debug.Log("PROBE "+(ok?"pass":"FAIL")+": "+name);}
        // Development aid: "-frameProbe <folder> <size> <turns>" plays the rivals forward, then measures frames and turns uncapped.
        IEnumerator FrameProbe(string folder,int size,int turns)
        {
            Directory.CreateDirectory(folder);yield return Frames(5);var log=new System.Text.StringBuilder();
            QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
            seedText="4242";setupSize=size;setupShape=1;setupCivs=World.MaxCivsBySize[size];setupPeoples=10;mainMenu=false;nameSetup=false;guide=false;tutorialOn=false;tutorialStep=-1;StartGame();hasSession=true;guide=false;tutorialOn=false;tutorialStep=-1;
            var founder=world.units.FirstOrDefault(u=>u.owner==0&&u.type==0);var made=world.Found(founder);log.AppendLine("screen "+Screen.width+"x"+Screen.height+" tiles "+world.tiles.Length+" nations "+world.civCount+" "+System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            var clock=new System.Diagnostics.Stopwatch();double sum=0,worst=0;
            for(int t=1;t<=turns&&world.result=="";t++){if(world.research<0||!world.AvailableTech(0).Contains(world.research))world.SelectResearch(world.AvailableTech(0).DefaultIfEmpty(0).First());for(int o=1;o<world.civCount;o++)world.Relation(0,o).friendUntil=world.turn+10;clock.Restart();world.EndTurn();double ms=clock.Elapsed.TotalMilliseconds;sum+=ms;worst=Math.Max(worst,ms);if(t%20==0){var homes=Enumerable.Range(1,world.civCount-1).Where(o=>!world.IsNativeNation(o)&&world.cities.Any(c=>c.owner==o)).ToList();log.AppendLine("turn "+t+": average "+(sum/20).ToString("0")+" ms, worst "+worst.ToString("0")+" ms, cities "+world.cities.Count+", units "+world.units.Count+", animals "+world.animals.Count+", rival ages "+(homes.Count==0?"-":homes.Min(o=>world.Era(o))+"-"+homes.Max(o=>world.Era(o)))+", techs "+(homes.Count==0?"-":homes.Max(o=>world.nations[o].tech.Take(36).Count(a=>a))+"")+", science "+(homes.Count==0?"-":homes.Max(o=>world.SciencePerTurn(o))+"")+", broke "+homes.Count(o=>world.nations[o].gold==0)+", net "+(homes.Count==0?"-":homes.Sum(o=>world.Budget(o)[2])/homes.Count+"")+", people "+world.characters.Count+", grudges "+world.grudges.Count+", graveyards "+world.stories.Count(s=>s.deaths>=LivingCatalog.GraveyardAt)+", breaks "+world.characters.Count(p=>p.breakTurns>0)+", disloyal "+world.characters.Count(p=>p.city>=0&&p.loyalty<15)+", obey "+world.cities.Count(c=>c.owner!=0&&world.Obeys(c))+"/"+world.cities.Count+", rebellions "+world.log.Count(l=>l.Contains("broken away"))+", result '"+world.result+"'");File.WriteAllText(Path.Combine(folder,"frames.txt"),log.ToString());sum=0;worst=0;}if(t%5==0)yield return null;}
            world.researchPrompts.Clear();world.buildPrompts.Clear();world.buildNotices.Clear();world.rivalCalls.Clear();world.newContacts.Clear();for(int o=1;o<world.civCount;o++)world.met[o]=o<6;
            foreach(var t in world.tiles)if(t.realm==0&&world.Distance(t.x,t.y,made.x,made.y)<=24)t.seen=true;world.Reveal();selected=world.units.Where(u=>u.owner==0).Select(u=>u.id).DefaultIfEmpty(-1).First();
            foreach(int zoom in new[]{64,32,128})
            {
                isoWidth=zoom;FocusTile(made.x,made.y);yield return Frames(90);guiClock.Reset();mapClock.Reset();hudClock.Reset();guiPasses=0;long before=GC.GetTotalMemory(false);int gcs=GC.CollectionCount(0);clock.Restart();const int N=120;for(int n=0;n<N;n++)yield return null;double total=clock.Elapsed.TotalMilliseconds;
                log.AppendLine("zoom "+zoom+": frame "+(total/N).ToString("0.0")+" ms; OnGUI "+(guiClock.Elapsed.TotalMilliseconds/N).ToString("0.0")+" (map "+(mapClock.Elapsed.TotalMilliseconds/N).ToString("0.0")+", HUD "+(hudClock.Elapsed.TotalMilliseconds/N).ToString("0.0")+"), passes/frame "+(guiPasses/(float)N).ToString("0.0")+", GC gen0 "+(GC.CollectionCount(0)-gcs));
            }
            isoWidth=64;FocusTile(made.x,made.y);yield return Frames(30);{clock.Restart();const int N=60;for(int n=0;n<N;n++){IsoPan(6,3);yield return null;}log.AppendLine("panning: frame "+(clock.Elapsed.TotalMilliseconds/N).ToString("0.0")+" ms");}
            clock.Restart();AutoSaveMeasure();log.AppendLine("save "+clock.Elapsed.TotalMilliseconds.ToString("0")+" ms");
            autoSaveFolder=folder;for(int run=0;run<3;run++){world.researchPrompts.Clear();world.buildPrompts.Clear();world.nations[0].pantheon=0;if(run==2)while((world.turn+1)%10!=0)world.turn++;clock.Restart();EndTurn();double slowest=0;var one=new System.Diagnostics.Stopwatch();one.Start();int frames=0;while(turnRunning){yield return null;slowest=Math.Max(slowest,one.Elapsed.TotalMilliseconds);one.Reset();one.Start();frames++;}log.AppendLine("End Turn"+(run==2?" with autosave":"")+": "+clock.Elapsed.TotalMilliseconds.ToString("0")+" ms, "+frames+" frames drawn meanwhile, slowest frame "+slowest.ToString("0")+" ms");}log.AppendLine("autosave file written: "+(Directory.GetFiles(folder,"earth-auto-*.json").Length>0)+"; status "+status);autoSaveFolder=null;
            File.WriteAllText(Path.Combine(folder,"frames.txt"),log.ToString());Application.Quit();
        }
        void AutoSaveMeasure(){JsonUtility.ToJson(world,false);}
        // Development aid: "-routeProbe <save>" loads a save and times orders from a settler to each of the player's cities.
        IEnumerator RouteProbe(string save)
        {
            yield return Frames(5);tutorialOn=false;tutorialStep=-1;LoadFile(save);activeSavePath=null;autoSaveFolder=Path.GetDirectoryName(save);Debug.Log("PROBE loaded: "+status+" turn "+world.turn+" tiles "+world.tiles.Length+" nations "+world.civCount+" cities "+world.cities.Count+" units "+world.units.Count);
            foreach(var line in world.units.Where(u=>u.owner==0).GroupBy(u=>u.type).Select(g=>g.Count()+" "+World.UnitNames[g.Key]))Debug.Log("PROBE own "+line);
            yield return Frames(30);var own=world.cities.Where(c=>c.owner==0).ToList();var clock=new System.Diagnostics.Stopwatch();
            var settler=world.units.FirstOrDefault(u=>u.owner==0&&u.type==0);if(settler==null&&own.Count>0){var near=own[0];settler=world.Add(0,0,near.x,near.y);Debug.Log("PROBE added a settler at "+near.name);}
            if(settler==null){Debug.Log("PROBE no settler or city");Application.Quit();yield break;}
            selected=settler.id;cityId=-1;FocusTile(settler.x,settler.y);int sx=settler.x,sy=settler.y;
            foreach(var c in own.Take(12)){
             settler.x=sx;settler.y=sy;settler.moves=World.Moves[0];settler.hasOrder=false;clock.Restart();string error="";bool ok=false;Debug.Log("PROBE ordering to "+c.name+" at "+c.x+","+c.y+" from "+sx+","+sy+" distance "+world.Distance(sx,sy,c.x,c.y)+" civilians there "+world.units.Count(a=>a.owner==0&&a.x==c.x&&a.y==c.y&&World.Power[a.type]==0));
             try{ok=world.Order(settler,c.x,c.y);}catch(Exception e){error=e.ToString();}
             Debug.Log("PROBE order to "+c.name+": "+ok+" "+clock.ElapsedMilliseconds+" ms; order "+settler.hasOrder+" to "+settler.destinationX+","+settler.destinationY+" now at "+settler.x+","+settler.y+" "+error);
             clock.Restart();yield return Frames(3);Debug.Log("PROBE frames after: "+clock.ElapsedMilliseconds+" ms");
            }
            settler.x=sx;settler.y=sy;settler.hasOrder=false;Application.Quit();
        }
        // Development aid: "-unitProbe <save> <city>" explains why the player's units in or beside that city cannot move.
        IEnumerator UnitProbe(string save,string cityName)
        {
            yield return Frames(5);tutorialOn=false;tutorialStep=-1;LoadFile(save);activeSavePath=null;Debug.Log("PROBE loaded: "+status+" turn "+world.turn+" gold "+world.gold+" food state "+world.FoodStateName(0)+" short "+world.nations[0].rationShort+" unfed "+world.UnfedUnits(0));
            var c=world.cities.Find(a=>a.name==cityName);if(c==null){Debug.Log("PROBE no city "+cityName);Application.Quit();yield break;}
            Debug.Log("PROBE city "+c.name+" owner "+c.owner+" at "+c.x+","+c.y);
            foreach(var u in world.units.Where(a=>a.owner==0&&world.Distance(a.x,a.y,c.x,c.y)<=1)){
             Debug.Log("PROBE unit "+u.id+" "+World.UnitNames[u.type]+" at "+u.x+","+u.y+" moves "+u.moves+"/"+world.MaxMoves(u)+" unfed "+u.unfed+" stack "+u.stack+" order "+u.hasOrder+" to "+u.destinationX+","+u.destinationY+" job "+u.job+" flee "+u.flee+" fortified "+u.fortified+" sentry "+u.sentry+" exploring "+u.exploring+" auto "+u.autoWork+" road "+u.roadOrder+" hp "+u.hp);
             for(int k=0;k<6;k++){var n=world.Step(u.x,u.y,World.HexDi[k],World.HexDj[k]);if(n==null)continue;Debug.Log("PROBE   to "+n.x+","+n.y+" "+World.TerrainNames[n.terrain]+" owner "+n.owner+": "+(world.MoveProblem(u,n.x,n.y) is var why&&why==""?"can move":why));}
            }
            {var mover=world.units.FirstOrDefault(a=>a.owner==0&&a.x==c.x&&a.y==c.y&&World.Power[a.type]>0);if(mover!=null){var land=Enumerable.Range(0,6).Select(k=>world.Step(c.x,c.y,World.HexDi[k],World.HexDj[k])).FirstOrDefault(n=>n!=null&&world.MoveProblem(mover,n.x,n.y)=="");Debug.Log("PROBE move test: "+(land!=null&&world.Move(mover,land.x,land.y)?"moved to "+mover.x+","+mover.y+" with "+world.StackOf(mover).Count+" in its stack":"could not move"));}}
            Application.Quit();
        }
        IEnumerator InputProbeRun(string folder)
        {
            Directory.CreateDirectory(folder);yield return Frames(5);
            seedText="421";mainMenu=false;nameSetup=false;guide=false;tutorialOn=false;tutorialStep=-1;StartGame();hasSession=true;guide=false;tutorialOn=false;tutorialStep=-1;
            var founder=world.units.FirstOrDefault(u=>u.owner==0&&u.type==0);var made=world.Found(founder);world.Reveal();selected=-1;cityId=made.id;yield return Frames(60);
            yield return InputProbes(folder,made);Application.Quit();
        }
        IEnumerator InputProbes(string folder,City city)
        {
            // Saves go to the screenshot folder, so a real saved game is never touched.
            activeSavePath=Path.Combine(folder,"probe-save.json");toastText="";yield return Send(Event.KeyboardEvent("^s"));Probe("Ctrl+S saves ("+toastText+")",toastText=="Game saved"&&File.Exists(activeSavePath));
            yield return Shot(folder,"10a-saved-toast.bmp");
            // Double click on a city with a unit on it.
            cityScreen=false;detailsScreen="";isoWidth=64;FocusTile(city.x,city.y);var guard=world.Add(0,2,city.x,city.y);yield return Frames(5);float sx,sy;IsoScreen(city.x,city.y,out sx,out sy);
            yield return Send(Mouse(EventType.MouseDown,sx,sy));Debug.Log("PROBEINFO after down pointer "+mapPointerDown+" press "+pressX+","+pressY+" city "+city.x+","+city.y+" result '"+world.result+"' screens "+cityScreen+detailsScreen+researchScreen+" prompt "+PromptOpen());yield return Send(Mouse(EventType.MouseUp,sx,sy));Probe("first click on a city with a unit opens the chooser",tilePicker);yield return Send(Mouse(EventType.MouseDown,sx,sy,0,2));Probe("double click opens the city whatever stands there",cityScreen&&!tilePicker);
            yield return Send(Mouse(EventType.MouseUp,sx,sy,0,2));
            // Right-click cancels a chosen building, though greyed out; then the building list.
            cityBuildingTab=true;int pot=TechnologyCatalog.Index("pottery");bool hadPot=world.tech[pot];world.tech[pot]=true;world.tech[TechnologyCatalog.Index("writing")]=true;var opts=World.BuildingIds.Where(b=>world.TechAllows(city,b)).ToList();city.buildingProject=opts[0];city.buildingQueue.Clear();if(opts.Count>1)city.buildingQueue.Add(opts[1]);buildingQueueScroll=Vector2.zero;
            foreach(var u in new[]{2,3,4,7,2,3,2,3})city.queue.Add(World.UnitBuild(u));city.build=World.UnitBuild(2);yield return Shot(folder,"10b-building-greyed.bmp");
            yield return ClickAt(1000,336,1);Probe("right click cancels a chosen building",city.buildingProject!=opts[0]);
            cityBuildingTab=false;yield return Shot(folder,"10c-unit-queue.bmp");city.queue.Clear();city.build=-1;
            // Zoom to city.
            yield return ClickAt(955,127);Probe("Zoom to city returns to the map on the city",!cityScreen&&isoWidth==128);yield return Shot(folder,"10d-zoomed-city.bmp");isoWidth=64;world.units.Remove(guard);
            // The wheel does nothing while the research prompt waits.
            world.researchPrompts.Add(TechnologyCatalog.Index("agriculture"));yield return Frames(3);int zoom=isoWidth;var wheel=new Event{type=EventType.ScrollWheel,mousePosition=new Vector2(200,500),delta=new Vector2(0,3)};yield return Send(wheel);Probe("the wheel is still while the research prompt waits",isoWidth==zoom);world.researchPrompts.Clear();yield return Frames(3);yield return Send(new Event{type=EventType.ScrollWheel,mousePosition=new Vector2(200,500),delta=new Vector2(0,3)});Probe("and works again afterwards",isoWidth!=zoom);isoWidth=64;
            // Realms: the Legends button, and the Close button of a people's page.
            detailsScreen="Atlas";viewRealm=0;legendsOpen=false;legendsPeople=-1;yield return Shot(folder,"10e-realms.bmp");yield return Send(Mouse(EventType.MouseDown,1030,458));Debug.Log("PROBEINFO hot "+GUIUtility.hotControl);yield return Send(Mouse(EventType.MouseUp,1030,458));Probe("the Legends button opens the book",legendsOpen);yield return Shot(folder,"10f-legends-empty.bmp");
            var gate=world.tiles.FirstOrDefault(t=>t.realm==0&&t.gateLink>0);if(gate!=null){gate.seen=true;int far=world.GateTwin(gate).realm;world.turn++;legendsWorld=far;var people=CivilisationCatalog.PeoplesOf(far).Where(c=>world.NationOfPeople(c)>=0).ToList();if(people.Count>1){world.nations[world.NationOfPeople(people[0])].intel=2;world.nations[world.NationOfPeople(people[1])].intel=3;}yield return Shot(folder,"10g-legends-known.bmp");
                if(people.Count>0){legendsPeople=people[0];yield return Shot(folder,"10h-people-partial.bmp");yield return ClickAt(234,710);Probe("Close leaves a people's page",legendsPeople==-1&&legendsOpen);}
                world.turn--;gate.seen=false;}
            legendsOpen=false;detailsScreen="Civics";yield return Shot(folder,"10i-civics.bmp");detailsScroll=new Vector2(0,520);world.nations[0].priority=4;yield return Shot(folder,"10r-court.bmp");world.nations[0].priority=0;detailsScroll=Vector2.zero;detailsScreen="";
            {detailsScreen="Cities";citiesPick=city.id;citiesPickBuilding=false;yield return Shot(folder,"10m-cities.bmp");citiesPick=-1;detailsScreen="";}
            {var door=world.tiles.FirstOrDefault(t=>t.realm==0&&t.gateLink>0);if(door!=null){door.seen=true;StartGateScene(door);if(world.gatesFound==null)world.gatesFound=new System.Collections.Generic.List<int>();world.gatesFound.Add(door.y*world.Width+door.x);yield return new WaitForSeconds(5f);yield return Shot(folder,"10n-gate-scene.bmp");yield return ClickAt(680,610);Probe("Continue closes the gateway scene",gateScene==null);FocusTile(door.x,door.y);isoWidth=64;BuildAtlasTexture();yield return Shot(folder,"10o-gate-on-map.bmp");door.seen=false;world.gatesFound.Clear();FocusTile(city.x,city.y);}}
            {world.tech[TechnologyCatalog.Index("wheel")]=true;var hand=world.Add(0,1,city.x,city.y);selected=hand.id;cityId=-1;isoWidth=96;FocusTile(city.x,city.y);RoadKey(hand);Probe("R starts road plotting",roadMode);float rx,ry;var aim=world.tiles.First(t=>t.realm==0&&t.seen&&world.Distance(t.x,t.y,city.x,city.y)==3&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));IsoScreen(aim.x,aim.y,out rx,out ry);yield return Send(Mouse(EventType.MouseUp,rx,ry));yield return Shot(folder,"10p-road-plot.bmp");yield return ClickAt(rx,ry);Probe("a click orders the plotted road",!roadMode&&hand.roadOrder&&hand.roadX==aim.x&&hand.roadY==aim.y);world.units.Remove(hand);selected=-1;isoWidth=64;}
            {detailsScreen="Religion";detailsScroll=Vector2.zero;yield return Shot(folder,"10q-faith-tree.bmp");detailsScreen="";}
            {var me=world.nations[0];int keepFaith=me.faith;me.faith=World.PantheonCost;me.pantheon=-1;yield return Shot(folder,"10l-god-prompt.bmp");Probe("the god prompt is waiting",PromptOpen());me.faith=keepFaith;}
            {// A crowded tile: the box must show every word.
             var spot=world.tiles.FirstOrDefault(t=>t.realm==0&&t.owner==0&&!PixelArt.IsWater(t.terrain)&&world.Distance(t.x,t.y,city.x,city.y)==1);if(spot!=null){spot.road=true;spot.farm=true;spot.village=true;var a=world.Add(0,2,spot.x,spot.y);var b=world.Add(0,4,spot.x,spot.y);var beast=new Animal{id=9901,kind=5,x=spot.x,y=spot.y};world.animals.Add(beast);selected=a.id;cityId=-1;isoWidth=64;FocusTile(spot.x,spot.y);yield return Send(Mouse(EventType.MouseUp,640,20));yield return Shot(folder,"10k-tile-card.bmp");world.animals.Remove(beast);world.units.Remove(a);world.units.Remove(b);selected=-1;}}
            world.tech[pot]=hadPot;
            // Esc menu.
            pauseMenu=true;yield return Shot(folder,"10j-pause.bmp");pauseMenu=false;activeSavePath=null;
        }
        // Development aid: "-flickerProbe <folder>" watches the idle map frame by frame and reports thin horizontal bands that change
        // while the rows beside them do not, then saves the worst pair of frames and their difference.
        void WriteBmp(string path,Color32[] px,int w,int h)
        {
            int row=(w*3+3)&~3;var bytes=new byte[54+row*h];Action<int,int> put=(at,v)=>{bytes[at]=(byte)v;bytes[at+1]=(byte)(v>>8);bytes[at+2]=(byte)(v>>16);bytes[at+3]=(byte)(v>>24);};
            bytes[0]=(byte)'B';bytes[1]=(byte)'M';put(2,bytes.Length);put(10,54);put(14,40);put(18,w);put(22,h);bytes[26]=1;bytes[28]=24;put(34,row*h);
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){var c=px[y*w+x];int at=54+y*row+x*3;bytes[at]=c.b;bytes[at+1]=c.g;bytes[at+2]=c.r;}
            File.WriteAllBytes(path,bytes);
        }
        IEnumerator FlickerProbe(string folder)
        {
            Directory.CreateDirectory(folder);yield return Frames(5);
            seedText="421";mainMenu=false;nameSetup=false;guide=false;tutorialOn=false;tutorialStep=-1;StartGame();hasSession=true;guide=false;tutorialOn=false;tutorialStep=-1;
            var founder=world.units.FirstOrDefault(u=>u.owner==0&&u.type==0);if(founder!=null)world.Found(founder);
            foreach(var t in world.tiles)if(t.realm==0&&world.Distance(t.x,t.y,founder.x,founder.y)<=9)t.seen=true;
            var log=new System.Text.StringBuilder();log.AppendLine("screen "+Screen.width+"x"+Screen.height+" vsync "+QualitySettings.vSyncCount+" zoom "+isoWidth);
            foreach(int zoom in new[]{64,48,96})
            {
                isoWidth=zoom;yield return Frames(120);int w=Screen.width,h=Screen.height;Color32[] last=null,worstA=null,worstB=null;float worst=0;int worstRow=-1,hits=0;var seen=new int[h];var frame=new Texture2D(w,h,TextureFormat.RGBA32,false);
                for(int n=0;n<180;n++)
                {
                    yield return new WaitForEndOfFrame();frame.ReadPixels(new Rect(0,0,w,h),0,0);var px=frame.GetPixels32();
                    if(last!=null)
                    {
                        var changed=new int[h];for(int y=0;y<h;y++){int at=y*w,count=0;for(int x=0;x<w;x++){var a=px[at+x];var b=last[at+x];if(Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b)>9)count++;}changed[y]=count;}
                        for(int y=4;y<h-4;y++){float beside=(changed[y-4]+changed[y+4])/2f,score=(changed[y]-beside)/w;if(score>.03f){seen[y]++;hits++;if(score>worst){worst=score;worstRow=y;worstA=last;worstB=px;}}}
                    }
                    last=px;yield return null;
                }
                Destroy(frame);log.AppendLine("zoom "+zoom+": thin-band hits "+hits+", worst score "+worst.ToString("0.00")+" at row "+(worstRow<0?-1:h-1-worstRow)+" (from top); rows: "+string.Join(",",Enumerable.Range(0,h).Where(y=>seen[y]>0).OrderByDescending(y=>seen[y]).Take(20).Select(y=>(h-1-y)+"x"+seen[y]).ToArray()));
                if(worstA!=null){WriteBmp(Path.Combine(folder,"z"+zoom+"-a.bmp"),worstA,w,h);WriteBmp(Path.Combine(folder,"z"+zoom+"-b.bmp"),worstB,w,h);var d=new Color32[w*h];for(int k=0;k<d.Length;k++){int v=Math.Min(255,(Math.Abs(worstA[k].r-worstB[k].r)+Math.Abs(worstA[k].g-worstB[k].g)+Math.Abs(worstA[k].b-worstB[k].b))*2);d[k]=new Color32((byte)v,(byte)v,(byte)v,255);}WriteBmp(Path.Combine(folder,"z"+zoom+"-diff.bmp"),d,w,h);}
            }
            // Then pan slowly: a straight one or two pixel line across the view, unlike the rows beside it, is a seam between tiles.
            foreach(int zoom in new[]{64,48,96,32})
            {
                isoWidth=zoom;FocusTile(founder.x,founder.y);yield return Frames(60);int w=Screen.width,h=Screen.height;var frame=new Texture2D(w,h,TextureFormat.RGBA32,false);int lines=0,linesRow=-1;float top=0;Color32[] topFrame=null;var rows=new System.Collections.Generic.Dictionary<int,int>();
                for(int n=0;n<240;n++)
                {
                    camI+=.013f;camJ+=.019f;compositeBudgetOn=false;
                    yield return new WaitForEndOfFrame();frame.ReadPixels(new Rect(0,0,w,h),0,0);var px=frame.GetPixels32();
                    for(int y=2;y<h-2;y++){int count=0;for(int x=0;x<w;x+=2){var a=px[y*w+x];var u=px[(y+1)*w+x];var d=px[(y-1)*w+x];int au=Math.Abs(a.r-u.r)+Math.Abs(a.g-u.g)+Math.Abs(a.b-u.b),ad=Math.Abs(a.r-d.r)+Math.Abs(a.g-d.g)+Math.Abs(a.b-d.b),ud=Math.Abs(u.r-d.r)+Math.Abs(u.g-d.g)+Math.Abs(u.b-d.b);if(au>30&&ad>30&&ud<au/2&&ud<ad/2)count++;}
                        float share=count*2f/w;if(share>.12f){lines++;int key=h-1-y;int was;rows.TryGetValue(key,out was);rows[key]=was+1;if(share>top){top=share;linesRow=key;topFrame=px;}}}
                    yield return null;
                }
                Destroy(frame);log.AppendLine("pan zoom "+zoom+": line rows "+lines+", widest "+top.ToString("0.00")+" of screen at row "+linesRow+"; common rows: "+string.Join(",",rows.OrderByDescending(a=>a.Value).Take(12).Select(a=>a.Key+"x"+a.Value).ToArray()));
                if(topFrame!=null)WriteBmp(Path.Combine(folder,"pan"+zoom+".bmp"),topFrame,w,h);
            }
            File.WriteAllText(Path.Combine(folder,"flicker.txt"),log.ToString());Application.Quit();
        }
        IEnumerator Shot(string folder,string name)
        {
            // Terrain pictures are composed under a per-frame budget, so give the view time to fill in first.
            yield return Frames(90);
            yield return new WaitForEndOfFrame();
            // The project has no screenshot or image-encoding modules, so read the frame back and write a 24-bit BMP.
            int w=Screen.width,h=Screen.height;var frame=new Texture2D(w,h,TextureFormat.RGBA32,false);
            frame.ReadPixels(new Rect(0,0,w,h),0,0);frame.Apply();var px=frame.GetPixels32();Destroy(frame);
            int row=(w*3+3)&~3;var bytes=new byte[54+row*h];
            Action<int,int> put=(at,v)=>{bytes[at]=(byte)v;bytes[at+1]=(byte)(v>>8);bytes[at+2]=(byte)(v>>16);bytes[at+3]=(byte)(v>>24);};
            bytes[0]=(byte)'B';bytes[1]=(byte)'M';put(2,bytes.Length);put(10,54);put(14,40);put(18,w);put(22,h);bytes[26]=1;bytes[28]=24;put(34,row*h);
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){var c=px[y*w+x];int at=54+y*row+x*3;bytes[at]=c.b;bytes[at+1]=c.g;bytes[at+2]=c.r;}
            File.WriteAllBytes(Path.Combine(folder,name),bytes);
            yield return Frames(5);
        }
        IEnumerator AutoShot(string folder)
        {
            Directory.CreateDirectory(folder);
            // The player's own tutorial setting and progress are restored at the end (screenshots must not change them).
            int keepTutorialOn=PlayerPrefs.GetInt("RealmTutorialOn",1),keepTutorialStep=PlayerPrefs.GetInt("RealmTutorialStep",-1);
            yield return Frames(5);
            mainMenu=false;nameSetup=true;setupPage=0;setupCiv=4;
            yield return Shot(folder,"0-new-game.bmp");
            civGroupReal=true;setupCiv=17;yield return Shot(folder,"0a-real-civilisation.bmp");civLoreOpen=true;yield return Shot(folder,"0a2-civilisation-lore.bmp");setupCiv=26;yield return Shot(folder,"0a3-lore-persia.bmp");civLoreOpen=false;
            setupCiv=CivilisationCatalog.Count;proposedName="Free Cantons";customLeader="Mayor Tell";customPortrait=18;yield return Shot(folder,"0a4-real-custom.bmp");civGroupReal=false;setupCiv=4;
            setupCiv=CivilisationCatalog.Count;proposedName="Ember Clans";customLeader="Chief Oru";customPortrait=13;customTraitA=2;customTraitB=9;
            yield return Shot(folder,"0b-custom.bmp");
            setupCiv=0;setupPage=1;
            yield return Shot(folder,"0c-world.bmp");
            seedText="421";nameSetup=false;guide=false;StartGame();hasSession=true;guide=false;
            tutorialOn=true;tutorialStep=0;tutorialMinimised=false;yield return Shot(folder,"1a-tutorial-welcome.bmp");tutorialStep=1;yield return Shot(folder,"1b-tutorial-step.bmp");tutorialStep=6;yield return Shot(folder,"1c-tutorial-workers.bmp");tutorialOn=false;tutorialStep=-1;
            yield return Shot(folder,"1-map.bmp");
            var settler=world.units.FirstOrDefault(u=>u.owner==0&&u.type==0);
            if(settler!=null)world.Found(settler);
            var unit=world.units.FirstOrDefault(u=>u.owner==0&&World.Power[u.type]>0)??world.units.FirstOrDefault(u=>u.owner==0);
            if(unit!=null){selected=unit.id;Center();}
            yield return Shot(folder,"2-unit.bmp");{var fc=world.cities.FirstOrDefault(a=>a.owner==0);if(fc!=null){world.buildNotices.Add(new BuildNotice{city=fc.id,text=fc.name+" trained Warrior.",building=false});world.met[1]=true;world.rivalCalls.Add(new RivalCall{from=1,offer=0,text="proposes a trade."});yield return Shot(folder,"2a-messages.bmp");{var hc=world.cities.FirstOrDefault(a=>a.owner==0);if(hc!=null&&world.wildlife){world.buildNotices.Clear();world.rivalCalls.Clear();int placed=0;foreach(var ring in world.tiles.Where(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain)&&world.Visible(t.x,t.y)&&world.Distance(t.x,t.y,hc.x,hc.y)>=1&&world.Distance(t.x,t.y,hc.x,hc.y)<=4&&world.AnimalAt(t.x,t.y)==null&&!world.units.Any(u=>u.x==t.x&&u.y==t.y)&&!world.cities.Any(u=>u.x==t.x&&u.y==t.y)).OrderBy(t=>(t.x*37+t.y*11)%13).ToList()){if(placed>=7)break;world.animals.Add(new Animal{id=world.nextId++,kind=placed,x=ring.x,y=ring.y});placed++;}FocusTile(hc.x,hc.y);int keepZoom=isoWidth;isoWidth=128;yield return Shot(folder,"2b-animals.bmp");isoWidth=keepZoom;world.animals.Clear();}}world.buildNotices.Clear();world.rivalCalls.Clear();world.met[1]=false;}}
            var city=world.cities.FirstOrDefault(c=>c.owner==0);
            if(city!=null){selected=-1;cityId=city.id;cityScreen=true;var spot=world.Workable(city).FirstOrDefault();if(spot!=null)world.ToggleLock(city,spot);}
            yield return Shot(folder,"3-city.bmp");
            cityScreen=false;detailsScreen="Economy";
            yield return Shot(folder,"4-economy.bmp");
            // Owner requests of 6 Nov 2026, on this throwaway game: Chronicle, guard pictures and route line, guide prompts, wide city view.
            detailsScreen="Chronicle";yield return Shot(folder,"4c-chronicle.bmp");detailsScreen="";
            if(city!=null){
                var extras=new System.Collections.Generic.List<Unit>();isoWidth=96;int slot=0;
                foreach(int kind in new[]{2,3,4,5,6,7,8,9}){var at=world.tiles.Where(t=>t.realm==0&&!World.IsImpassable(t.terrain)&&!PixelArt.IsWater(t.terrain)&&world.Distance(t.x,t.y,city.x,city.y)==2&&!world.units.Any(a=>a.x==t.x&&a.y==t.y)).Skip(slot++%2).FirstOrDefault();if(at==null)continue;var g=world.Add(0,kind,at.x,at.y);g.fortified=true;extras.Add(g);}
                world.Reveal();FocusTile(city.x,city.y);var walker=extras.FirstOrDefault(a=>a.type==2);if(walker!=null){walker.fortified=false;var far=world.tiles.Where(t=>t.realm==0&&t.seen&&world.Distance(t.x,t.y,walker.x,walker.y)==5&&world.Route(walker,t.x,t.y).Count>3).FirstOrDefault();if(far!=null){walker.hasOrder=true;walker.destinationX=far.x;walker.destinationY=far.y;selected=walker.id;}}
                yield return Shot(folder,"4d-guard-and-route.bmp");selected=-1;
                foreach(var g in extras)world.units.Remove(g);
                world.researchPrompts.Add(TechnologyCatalog.Index("agriculture"));yield return Shot(folder,"4e-research-prompt.bmp");world.researchPrompts.Clear();
                {int keepB=city.buildingProject;var keepQ=city.buildingQueue.ToList();city.buildingProject=-1;city.buildingQueue.Clear();int pot=TechnologyCatalog.Index("pottery");bool hadPot=world.tech[pot],hadWrite=world.tech[TechnologyCatalog.Index("writing")];world.tech[pot]=true;world.tech[TechnologyCatalog.Index("writing")]=true;
                 world.buildPrompts.Add(new BuildPrompt{city=city.id,tech=pot,options=World.BuildingIds.Where(b=>world.BuildReason(city,b)=="").Take(3).ToList()});yield return Shot(folder,"4f-build-prompt.bmp");world.buildPrompts.Clear();world.tech[pot]=hadPot;world.tech[TechnologyCatalog.Index("writing")]=hadWrite;city.buildingProject=keepB;city.buildingQueue=keepQ;}
                int keepAge=city.age;city.age=60;isoWidth=64;world.RebuildTerritory();world.Reveal();selected=-1;cityId=city.id;cityScreen=true;cityMapScroll=new Vector2(30,20);yield return Shot(folder,"4g-city-scroll.bmp");cityScreen=false;city.age=keepAge;world.RebuildTerritory();
                world.SetPriority(0,World.AxisMilitary,20);detailsScreen="Economy";yield return Shot(folder,"4h-priorities.bmp");detailsScreen="";world.SetPriority(0,World.AxisMilitary,0);world.SetPriority(0,World.AxisScience,50);
                yield return InputProbes(folder,city);
            }
            // Research screen with the Dawn completed (this throwaway game only), so the edict choice shows.
            detailsScreen="";foreach(var id in new[]{"agriculture","archery","bronze"})world.tech[TechnologyCatalog.Index(id)]=true;
            world.research=TechnologyCatalog.Index("writing");world.nations[0].research=world.research;inspectedTech=-1;researchScreen=true;
            yield return Shot(folder,"4b-research.bmp");{int keepEra=researchEra,keepResearch=world.research,keepInspect=inspectedTech;researchEra=0;inspectedTech=-1;researchScroll=Vector2.zero;var dawn=world.AvailableTech(0).Where(t=>TechnologyCatalog.Eras[t]==0).DefaultIfEmpty(-1).First();if(dawn>=0){world.research=dawn;world.nations[0].research=dawn;}LayoutTechnologyTree();yield return Shot(folder,"4b1-research-dawn.bmp");researchEra=keepEra;inspectedTech=keepInspect;world.research=keepResearch;world.nations[0].research=keepResearch;LayoutTechnologyTree();}
            techZoom=.6f;researchScroll=Vector2.zero;yield return Shot(folder,"4b2-research-zoomed-out.bmp");techZoom=1.4f;yield return Shot(folder,"4b3-research-zoomed-in.bmp");techZoom=1f;
            researchScreen=false;
            // Religion screen with a pantheon and a founded faith (this throwaway game only).
            if(city!=null){var me=world.nations[0];foreach(var id in new[]{"mysticism","laws","philosophy"})world.tech[TechnologyCatalog.Index(id)]=true;if(!city.buildings.Contains(36))city.buildings.Add(36);me.faith=200;world.ChoosePantheon(0,0);world.FoundReligion(0,3);}
            {var pr=world.BuyPriest(0,city);if(pr==null&&city!=null){pr=world.Add(0,World.PriestUnit,city.x,city.y);pr.faith=city.religion;pr.charges=2;}}detailsScreen="Religion";yield return Shot(folder,"4b4-religion.bmp");religionAncient=true;yield return Shot(folder,"4b5-religion-ancient.bmp");religionAncient=false;detailsScreen="";
            // Victory screen with a stage-1 project activating (this throwaway game only).
            if(city!=null){world.tech[TechnologyCatalog.Index("computers")]=true;city.pop=8;var n=world.nations[0];n.projectCity=city.id;n.projectStage=1;n.projectActivation=6;}
            detailsScreen="Victory";
            yield return Shot(folder,"4c-victory.bmp");
            detailsScreen="";
            // First contact opens Diplomacy by itself, with both leaders and a greeting.
            world.newContacts.Add(1);world.met[1]=true;
            yield return Shot(folder,"4d-first-contact.bmp");
            diplomacyChooser=true;yield return Shot(folder,"4e-diplomacy-chooser.bmp");diplomacyChooser=false;
            // The native peoples of the parallel worlds in diplomacy (this throwaway game only).
            {int demons=Enumerable.Range(1,world.civCount-1).FirstOrDefault(o=>world.nations[o].civ==33);if(demons>0){for(int o=1;o<world.civCount;o++)if(world.IsNativeNation(o))world.met[o]=true;detailsScreen="Diplomacy";diplomacyChooser=true;yield return Shot(folder,"4f-natives-chooser.bmp");diplomacyChooser=false;diplomacyPartner=demons;ResetDiplomacyUI();yield return Shot(folder,"4g-demon-talks.bmp");for(int o=1;o<world.civCount;o++)if(world.IsNativeNation(o))world.met[o]=false;}}
            detailsScreen="";
            // A revealed, zoomed-out view so coastlines and banks are visible (this throwaway game only).
            detailsScreen="";foreach(var t in world.tiles){t.seen=true;t.rememberedTerrain=t.terrain;t.rememberedResource=t.resource;t.rememberedOwner=t.owner;t.rememberedRoad=t.road;t.rememberedFarm=t.farm;t.rememberedMine=t.mine;t.rememberedFort=t.fort;}
            isoWidth=48;ClearIso();
            yield return Shot(folder,"5-world.bmp");
            var coast=world.tiles.FirstOrDefault(t=>!PixelArt.IsWater(t.terrain)&&world.At(t.x,t.y+1)!=null&&PixelArt.IsWater(world.At(t.x,t.y+1).terrain)&&world.At(t.x-1,t.y)!=null&&!PixelArt.IsWater(world.At(t.x-1,t.y).terrain));
            if(coast!=null){isoWidth=96;FocusTile(coast.x,coast.y);}
            yield return Shot(folder,"6-coast-close.bmp");
            // Close views of hills and dry steppe in full daylight (this throwaway game only; Satellites shows every tile).
            {int sat=TechnologyCatalog.Index("satellites");bool had=world.tech[sat];world.tech[sat]=true;
             var hill=world.tiles.Where(t=>t.terrain==2).OrderByDescending(t=>Enumerable.Range(0,9).Count(k=>{var n=world.Step(t.x,t.y,k%3-1,k/3-1);return n!=null&&n.terrain==2;})).FirstOrDefault();
             if(hill!=null){isoWidth=128;FocusTile(hill.x,hill.y);yield return Shot(folder,"6a-hills.bmp");}
             var steppe=world.tiles.Where(t=>t.terrain==20||t.terrain==0).OrderByDescending(t=>Enumerable.Range(0,9).Count(k=>{var n=world.Step(t.x,t.y,k%3-1,k/3-1);return n!=null&&n.terrain==t.terrain;})*10+(t.terrain==20?5:0)).FirstOrDefault();
             if(steppe!=null){isoWidth=128;FocusTile(steppe.x,steppe.y);yield return Shot(folder,"6a2-open-land.bmp");}
             world.tech[sat]=had;}
            // A grown, walled medieval capital with farmsteads around it (this throwaway game only).
            if(city!=null){city.pop=11;city.walls=true;city.granary=true;city.library=true;if(!city.buildings.Contains(36))city.buildings.Add(36);if(!city.buildings.Contains(31))city.buildings.Add(31);
                world.tech[TechnologyCatalog.Index("feudalism")]=true;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=world.Step(city.x,city.y,dx,dy);if(n!=null)n.owner=0;}
                isoWidth=128;FocusTile(city.x,city.y);}
            yield return Shot(folder,"6b-grown-city.bmp");
            // A port with ships (this throwaway game only): the nearest stretch of sea to the capital gets a port, a road, a Galley and a Boat.
            if(city!=null){var sea=world.tiles.Where(t=>t.terrain==4).OrderBy(t=>world.Distance(t.x,t.y,city.x,city.y)).FirstOrDefault();
                if(sea!=null){foreach(var t in world.tiles)if(world.Distance(t.x,t.y,city.x,city.y)<=3&&!World.IsImpassable(t.terrain))t.owner=0;
                    world.tech[TechnologyCatalog.Index("engineering")]=true;world.tech[TechnologyCatalog.Index("sailing")]=true;
                    if(!city.buildings.Contains(World.PortBuilding))city.buildings.Add(World.PortBuilding);var port=world.PortTile(city);
                    if(port!=null){foreach(var t in world.tiles)if(world.Distance(t.x,t.y,city.x,city.y)<=world.Distance(port.x,port.y,city.x,city.y)&&world.Distance(t.x,t.y,port.x,port.y)<=world.Distance(city.x,city.y,port.x,port.y)&&!World.IsImpassable(t.terrain))t.road=true;
                        world.UpdatePortLinks();var g=world.LaunchTile(city,World.GalleyUnit);if(g!=null)world.Add(0,World.GalleyUnit,g.x,g.y);var b=world.LaunchTile(city,World.WarGalleyUnit);if(b!=null)world.Add(0,World.BoatUnit,b.x,b.y);
                        isoWidth=96;FocusTile(port.x,port.y);}}}
            yield return Shot(folder,"6c-harbour.bmp");
            // Zoomed all the way out once the world is known to be round (Mysticism folds in astronomy).
            world.tech[TechnologyCatalog.Index("mysticism")]=true;isoWidth=32;IsoZoom(-1);
            yield return Shot(folder,"7-globe.bmp");
            // An ancient gateway, each parallel world beyond it and the Realms screen switcher (this throwaway game only).
            {var g=world.tiles.FirstOrDefault(t=>t.realm==0&&t.gateLink>0);if(g!=null){globeZoom=false;diamondView=true;isoWidth=128;FocusTile(g.x,g.y);yield return Shot(folder,"9a-gateway.bmp");
              foreach(int r in new[]{3,1,2,4,5}){var gate=world.tiles.FirstOrDefault(t=>t.realm==r&&t.gateLink>0);if(gate==null)continue;var visitor=world.Add(0,2,gate.x,gate.y);world.Reveal();selected=visitor.id;isoWidth=96;FocusTile(gate.x,gate.y);yield return Shot(folder,"9b-realm-"+r+".bmp");}
              detailsScreen="Atlas";yield return Shot(folder,"9c-realms-screen.bmp");legendsOpen=true;legendsWorld=3;yield return Shot(folder,"9d-legends.bmp");legendsPeople=CivilisationCatalog.PeoplesOf(3).Last();yield return Shot(folder,"9e-legend-people.bmp");legendsPeople=-1;legendsOpen=false;civLoreOpen=false;detailsScreen="";ShowRealm(0);isoWidth=96;}}
            // Classic view scrolled across the east-west seam.
            globeZoom=false;diamondView=false;tilePixels=48;mapScroll=new Vector2(world.Width*tilePixels-420,Mathf.Max(0,world.Height*tilePixels/2-300));
            yield return Shot(folder,"8-classic-wrap.bmp");
            PlayerPrefs.SetInt("RealmTutorialOn",keepTutorialOn);PlayerPrefs.SetInt("RealmTutorialStep",keepTutorialStep);PlayerPrefs.Save();
            Application.Quit();
        }
    }
}
