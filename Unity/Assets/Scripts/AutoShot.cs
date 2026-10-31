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
        }
        IEnumerator Frames(int n){for(int k=0;k<n;k++)yield return null;}
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
            yield return Frames(5);
            mainMenu=false;nameSetup=true;setupPage=0;setupCiv=4;
            yield return Shot(folder,"0-new-game.bmp");
            setupCiv=CivilisationCatalog.Count;proposedName="Ember Clans";customLeader="Chief Oru";customPortrait=13;customTraitA=2;customTraitB=9;
            yield return Shot(folder,"0b-custom.bmp");
            setupCiv=0;setupPage=1;
            yield return Shot(folder,"0c-world.bmp");
            seedText="421";nameSetup=false;guide=false;StartGame();hasSession=true;guide=false;
            yield return Shot(folder,"1-map.bmp");
            var settler=world.units.FirstOrDefault(u=>u.owner==0&&u.type==0);
            if(settler!=null)world.Found(settler);
            var unit=world.units.FirstOrDefault(u=>u.owner==0&&World.Power[u.type]>0)??world.units.FirstOrDefault(u=>u.owner==0);
            if(unit!=null){selected=unit.id;Center();}
            yield return Shot(folder,"2-unit.bmp");
            var city=world.cities.FirstOrDefault(c=>c.owner==0);
            if(city!=null){selected=-1;cityId=city.id;cityScreen=true;var spot=world.Workable(city).FirstOrDefault();if(spot!=null)world.ToggleLock(city,spot);}
            yield return Shot(folder,"3-city.bmp");
            cityScreen=false;detailsScreen="Economy";
            yield return Shot(folder,"4-economy.bmp");
            // Research screen with the Dawn completed (this throwaway game only), so the edict choice shows.
            detailsScreen="";foreach(var id in new[]{"agriculture","archery","bronze"})world.tech[TechnologyCatalog.Index(id)]=true;
            world.research=TechnologyCatalog.Index("writing");world.nations[0].research=world.research;inspectedTech=-1;researchScreen=true;
            yield return Shot(folder,"4b-research.bmp");
            techZoom=.6f;researchScroll=Vector2.zero;yield return Shot(folder,"4b2-research-zoomed-out.bmp");techZoom=1.4f;yield return Shot(folder,"4b3-research-zoomed-in.bmp");techZoom=1f;
            researchScreen=false;
            // Religion screen with a pantheon and a founded faith (this throwaway game only).
            if(city!=null){var me=world.nations[0];foreach(var id in new[]{"mysticism","laws","philosophy"})world.tech[TechnologyCatalog.Index(id)]=true;if(!city.buildings.Contains(39))city.buildings.Add(39);me.faith=200;world.ChoosePantheon(0,0);world.FoundReligion(0,3);}
            detailsScreen="Religion";yield return Shot(folder,"4b4-religion.bmp");religionAncient=true;yield return Shot(folder,"4b5-religion-ancient.bmp");religionAncient=false;detailsScreen="";
            // Victory screen with a stage-1 project activating (this throwaway game only).
            if(city!=null){world.tech[TechnologyCatalog.Index("computers")]=true;city.pop=8;var n=world.nations[0];n.projectCity=city.id;n.projectStage=1;n.projectActivation=6;}
            detailsScreen="Victory";
            yield return Shot(folder,"4c-victory.bmp");
            detailsScreen="";
            // First contact opens Diplomacy by itself, with both leaders and a greeting.
            world.newContacts.Add(1);world.met[1]=true;
            yield return Shot(folder,"4d-first-contact.bmp");
            diplomacyChooser=true;yield return Shot(folder,"4e-diplomacy-chooser.bmp");diplomacyChooser=false;
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
            // Classic view scrolled across the east-west seam.
            globeZoom=false;diamondView=false;tilePixels=48;mapScroll=new Vector2(world.Width*tilePixels-420,Mathf.Max(0,world.Height*tilePixels/2-300));
            yield return Shot(folder,"8-classic-wrap.bmp");
            Application.Quit();
        }
    }
}
