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
            i=Array.IndexOf(args,"-flickerProbe");if(i>=0&&i+1<args.Length)StartCoroutine(FlickerProbe(args[i+1]));
        }
        IEnumerator Frames(int n){for(int k=0;k<n;k++)yield return null;}
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
