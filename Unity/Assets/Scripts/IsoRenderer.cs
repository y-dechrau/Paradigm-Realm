using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    // Horizontal three-quarter map view (the file keeps its old name so Unity references stay intact). Tile (x,y) is drawn
    // as a 64 x 48 rectangle in level rows; on a sphere x wraps, so the world scrolls east and west without an edge. Rows are
    // drawn from the back (north) to the front so raised scenery overlaps the row behind it. Land stands PixelArt.Elev pixels
    // above water and shows a bank face where water lies in front of it.
    public partial class RealmGame
    {
        bool diamondView=true;
        float camI=20,camJ=15;
        int isoWidth=64;
        static readonly int[] IsoWidths={32,48,64,96,128};
        const float IsoCentreX=MapW/2,IsoCentreY=MapH/2;
        readonly Dictionary<long,Texture2D> isoTiles=new Dictionary<long,Texture2D>();
        readonly Queue<long> isoTileOrder=new Queue<long>();
        readonly Dictionary<long,Texture2D> isoPictures=new Dictionary<long,Texture2D>();
        readonly Dictionary<int,Texture2D> isoShown=new Dictionary<int,Texture2D>();
        readonly int[] isoNb=new int[4];
        readonly List<Tile> isoResTiles=new List<Tile>();
        readonly List<Vector2> isoResPos=new List<Vector2>();

        Texture2D PixelTexture(PixelCanvas cv)
        {
            var tex=new Texture2D(cv.w,cv.h,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[cv.w*cv.h];
            for(int y=0;y<cv.h;y++)for(int x=0;x<cv.w;x++)
            {
                uint c=cv.p[y*cv.w+x];
                pixels[(cv.h-1-y)*cv.w+x]=new Color(((c>>16)&255)/255f,((c>>8)&255)/255f,(c&255)/255f,(c>>24)/255f);
            }
            tex.SetPixels(pixels);tex.Apply();return tex;
        }
        // kind: 1 sprite, 2 farm, 3 road, 4 misc, 5 sea resource icon. Small shared pictures, never evicted.
        Texture2D IsoPicture(int kind,int a,int b)
        {
            long key=((long)kind<<40)|((long)a<<8)|(long)b;Texture2D tex;
            if(isoPictures.TryGetValue(key,out tex))return tex;
            // Supplied art (Resources/Sprites, e.g. from SpriteCook) replaces the drawn pictures; see Design/ART-SLOTS.md.
            // Cloud puffs carry their variant in a (cloud-V); the other slots in b.
            string slot=kind==1?"terrain-"+a.ToString("00"):kind==6?"hamlet-"+a:kind==7?"port":kind==9?"cloud":null;
            if(slot!=null){var supplied=SpriteSlot(slot+"-"+(kind==9?a:b))??SpriteSlot(slot);if(supplied!=null){isoPictures[key]=supplied;return supplied;}}
            PixelCanvas cv=null;
            if(kind==1)cv=PixelArt.Sprite(a,b);else if(kind==6)cv=PixelArt.Hamlet(a,b);else if(kind==7)cv=PixelArt.PortSprite();else if(kind==8)cv=PixelArt.HudIcon(a);else if(kind==9)cv=PixelArt.CloudPuff(a);else if(kind==2)cv=PixelArt.Farm(a);else if(kind==3)cv=PixelArt.Road(a);else if(kind==5)cv=PixelArt.ResourceIcon(a);else cv=PixelArt.Misc(a);
            // Tile-shaped overlays (farms, roads, borders, selection, unexplored) are drawn as diamonds in the diagonal view.
            if(cv!=null&&(kind==2||kind==3||kind==4&&a<=6))cv=PixelArt.ToDiamond(cv);
            tex=cv==null?null:PixelTexture(cv);isoPictures[key]=tex;return tex;
        }
        void ClearIso()
        {
            // Supplied art (spriteSlots) is a loaded asset shared by the cache, so only the drawn pictures are destroyed.
            foreach(var t in isoTiles.Values)Destroy(t);foreach(var t in isoPictures.Values)if(t!=null&&!spriteSlots.ContainsValue(t))Destroy(t);
            isoTiles.Clear();isoTileOrder.Clear();isoPictures.Clear();isoShown.Clear();
        }
        static int IsoVariant(int x,int y){unchecked{int h=x*73856093^y*19349663;return (h&0x7fffffff)%3;}}
        static int GroveVariant(int x,int y){unchecked{int h=x*83492791^y*1640531527;return (h&0x7fffffff)%8;}}

        // ---------- camera ----------
        void ToggleView(){diamondView=!diamondView;var u=Selected();if(u!=null)FocusTile(u.x,u.y);ResetMapGesture();}
        void ClampCamera()
        {
            if(world.Sphere){float w=world.Width;camI%=w;if(camI<0)camI+=w;}
            else camI=Mathf.Clamp(camI,0,world.Width-1);
            camJ=Mathf.Clamp(camJ,0,world.Height-1);
        }
        void FocusTile(int x,int y)
        {
            if(!diamondView){mapScroll=new Vector2(x*tilePixels-MapW/2,y*tilePixels-MapH/2);ClampMapScroll();return;}
            float i=x;if(world.Sphere){float w=world.Width;i=x+w*(float)Math.Round((camI-x)/w);}
            camI=i;camJ=y;ClampCamera();
        }
        void IsoPan(float dx,float dy)
        {
            // Screen movement of (dx,dy) pixels moves the camera by whole and part tiles, through the diagonal projection.
            float tw=isoWidth,th=tw*PixelArt.DH/PixelArt.DW,a=dx/(tw/2),b=dy/(th/2);
            camI+=(a+b)/2;camJ+=(b-a)/2;ClampCamera();
        }
        void IsoZoom(int direction)
        {
            if(globeZoom){if(direction>0)globeZoom=false;return;}
            if(direction<0&&isoWidth==IsoWidths[0]&&GlobeZoomAllowed()){EnterGlobeZoom(WrapColumn((int)Math.Round(camI)),(int)camJ);return;}
            int index=Array.IndexOf(IsoWidths,isoWidth);if(index<0)index=2;
            isoWidth=IsoWidths[Math.Max(0,Math.Min(IsoWidths.Length-1,index+direction))];
        }
        void IsoScreen(int x,int y,out float sx,out float sy)
        {
            float i=x;
            if(world.Sphere){float w=world.Width;i=x+w*(float)Math.Round((camI-x)/w);}
            double dx,dy;PixelArt.ScreenOf(i,y,camI,camJ,isoWidth,out dx,out dy);sx=IsoCentreX+(float)dx;sy=IsoCentreY+(float)dy;
        }
        // Which map tile is under a point given in map-view coordinates (0,0 = top left of the view).
        bool IsoPick(Vector2 local,out int x,out int y)
        {
            // Land and water are level, so the tile under the pointer is simply the rectangle it falls in.
            int i,j;PixelArt.PickOf(local.x-IsoCentreX,local.y-IsoCentreY,camI,camJ,isoWidth,out i,out j);
            var t=world.At(i,j);if(t==null){x=-1;y=-1;return false;}
            x=t.x;y=t.y;return true;
        }
        // Tile under the mouse in either view. Mouse position is in screen space.
        bool MapTileAt(Vector2 mouse,out int x,out int y)
        {
            if(diamondView)return IsoPick(mouse-new Vector2(MapX,MapY),out x,out y);
            x=Mathf.FloorToInt((mouse.x-MapX+mapScroll.x)/tilePixels);y=Mathf.FloorToInt((mouse.y-MapY+mapScroll.y)/tilePixels);
            var t=world.At(x,y);if(t==null)return false;x=t.x;y=t.y;return true;
        }

        // ---------- terrain pictures ----------
        int IsoKnownTerrain(int i,int j)
        {
            var n=world.At(i,j);
            return n==null||!n.seen?-1:world.KnownTerrain(n);
        }
        readonly int[] isoN9=new int[9];
        // Each tile's picture depends on its own position (world-space noise) and on the 3 x 3 block of terrain the player
        // knows, so the cache key is the tile plus a hash of that block, its river mask and the water animation frame.
        Texture2D IsoGround(Tile t,int i,int j,int frame)
        {
            int mask=0;bool anyWater=false;
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int n=IsoKnownTerrain(i+dx,j+dy);if(dx==0&&dy==0)n=t.terrain;isoN9[(dy+1)*3+dx+1]=n;if(n>=0&&PixelArt.IsWater(n))anyWater=true;}
            if(t.terrain==7){int[] offI={0,1,0,-1},offJ={-1,0,1,0};for(int k=0;k<4;k++){int n=IsoKnownTerrain(i+offI[k],j+offJ[k]);if(n>=0&&(n==7||PixelArt.IsWater(n)))mask|=1<<k;}}
            int fr=anyWater?frame:0;int sig=17;unchecked{foreach(int n in isoN9)sig=sig*31+n+1;sig=sig*31+mask;}
            int index=t.y*world.Width+t.x;long key=((long)(uint)sig<<32)|(uint)(index*4+fr);
            Texture2D tex;
            if(isoTiles.TryGetValue(key,out tex)){Texture2D cur;if(!isoShown.TryGetValue(index,out cur)||cur!=tex)isoShown[index]=tex;return tex;}
            if(compositeBudgetOn&&compositeClock.ElapsedMilliseconds>=CompositeBudgetMs)
            {
                Texture2D previous;
                if(isoShown.TryGetValue(index,out previous)&&previous!=null)return previous;
            }
            tex=PixelTexture(PixelArt.ToDiamond(PixelArt.Compose(isoN9,t.x,t.y,fr,mask)));
            if(isoTiles.Count>=6000){long oldest=isoTileOrder.Dequeue();Texture2D gone;if(isoTiles.TryGetValue(oldest,out gone)){Destroy(gone);isoTiles.Remove(oldest);}}
            isoTiles[key]=tex;isoTileOrder.Enqueue(key);isoShown[index]=tex;return tex;
        }
        // ---------- drawing ----------
        void DrawIsoMap()
        {
            if(terrainWorld!=world){ClearTerrainComposites();terrainWorld=world;}
            bool repaint=Event.current.type==EventType.Repaint;
            if(repaint)world.BeginVisibilityCache();

            GUI.BeginGroup(MapView);
            if(repaint)DrawIsoContents();
            GUI.EndGroup();
            world.EndVisibilityCache();
            HandleMapInput();
            if(dragging){var m=Event.current.mousePosition;var u=world.units.Find(a=>a.id==dragUnit);if(u!=null){float sx,sy;IsoScreen(u.x,u.y,out sx,out sy);DrawOrderLine(new Vector2(MapX+sx,MapY+sy-6*isoWidth/64f),m);}GUI.Label(new Rect(m.x+15,m.y+12,220,30),"Release to issue destination",body);}
        }
        readonly List<Vector3> isoFog=new List<Vector3>();
        static readonly int[] RoadDx={-1,0,1,1,1,0,-1,-1},RoadDy={-1,-1,-1,0,1,1,1,0};
        void DrawIsoContents()
        {
            compositeBudgetOn=true;compositeClock.Reset();compositeClock.Start();
            ClampCamera();
            float tw=isoWidth,th=tw*PixelArt.DH/PixelArt.DW,s=tw/64f;int width=world.Width;
            int frame=(int)(Time.realtimeSinceStartup*2.5f)%3;
            Fill(new Rect(0,0,MapW,MapH),new Color(.87f,.89f,.93f));isoFog.Clear();
            viewStacks.Clear();
            foreach(var unit in world.units){List<Unit> stackAt;int cell=unit.y*width+unit.x;if(!viewStacks.TryGetValue(cell,out stackAt)){stackAt=new List<Unit>(2);viewStacks[cell]=stackAt;}stackAt.Add(unit);}
            viewCities.Clear();foreach(var city in world.cities)viewCities[city.y*width+city.x]=city;
            if(Event.current.type==EventType.Repaint)BuildSuburbs();
            // Diagonal view: tiles go back to front by diagonal (i + j), so raised scenery overlaps the tiles behind it.
            // R covers every tile whose diamond can touch the view, with a margin for tall scenery below the bottom edge.
            int R=(int)Math.Ceiling(MapW/tw+MapH/th)+3,ci=(int)Math.Round(camI),cj=(int)Math.Round(camJ);
            int pooled=0;isoResTiles.Clear();isoResPos.Clear();isoCityNames.Clear();isoCityPos.Clear();isoCityInk.Clear();isoCityRefs.Clear();
            for(int sum=-2*R;sum<=2*R;sum++)for(int di=-R;di<=R;di++)
            {
                int dj=sum-di;if(dj<-R||dj>R)continue;int i=ci+di,j=cj+dj;
                if(j<0||j>=world.Height||!world.Sphere&&(i<0||i>=width))continue;
                float sx=IsoCentreX+((i-camI)-(j-camJ))*tw/2,sy=IsoCentreY+((i-camI)+(j-camJ))*th/2;
                if(sx<-tw||sx>MapW+tw||sy<-th||sy>MapH+64*s)continue;
                var source=world.At(i,j);if(source==null)continue;
                bool visible=world.Visible(source.x,source.y);Tile t=source;
                if(source.seen&&!visible){if(pooled==knownPool.Count)knownPool.Add(new Tile());t=knownPool[pooled++];world.CopyKnown(source,t);}
                if(!source.seen){GUI.DrawTexture(new Rect(sx-32*s,sy-PixelArt.DH/2*s,64*s,PixelArt.DH*s),IsoPicture(4,5,0));isoFog.Add(new Vector3(sx,sy,(i*7+j*13)&3));continue;}
                bool water=PixelArt.IsWater(t.terrain);float gy=sy;
                var old=GUI.color;if(!visible)GUI.color=new Color(.62f,.66f,.74f,1);
                var ground=new Rect(sx-32*s,sy-PixelArt.DH/2*s,64*s,PixelArt.DH*s);
                GUI.DrawTexture(ground,IsoGround(t,i,j,frame));
                if(!water&&t.farm){var farm=IsoPicture(2,IsoVariant(t.x,t.y),0);if(farm!=null)GUI.DrawTexture(ground,farm);}
                if(t.road)
                {
                    int roadMask=0;for(int k=0;k<8;k++)if(world.RoadConnects(t,RoadDx[k],RoadDy[k]))roadMask|=1<<k;
                    var road=IsoPicture(3,roadMask,0);if(road!=null)GUI.DrawTexture(ground,road);
                }
                // Territory outline along edges that touch another owner or unknown ground.
                if(t.owner>=0)
                {
                    Color ink=nations[t.owner];ink.a=.85f;int[] offI={0,1,0,-1},offJ={-1,0,1,0};
                    for(int k=0;k<4;k++)
                    {
                        var n=world.At(i+offI[k],j+offJ[k]);
                        if(n!=null&&n.seen&&world.KnownOwner(n)==t.owner)continue;
                        GUI.color=ink;GUI.DrawTexture(ground,IsoPicture(4,k,0));
                    }
                    GUI.color=visible?old:new Color(.62f,.66f,.74f,1);
                }
                // Grassland groves use eight variants so lone trees do not repeat in a pattern; none on farms, roads or cities.
                bool meadow=t.terrain==0;bool built=t.farm||t.road||viewCities.ContainsKey(t.y*width+t.x);
                var sprite=water||meadow&&built?null:IsoPicture(1,t.terrain,meadow?GroveVariant(t.x,t.y):IsoVariant(t.x,t.y));
                City suburbOf;if(!water&&suburbCells.TryGetValue(t.y*width+t.x,out suburbOf)&&(visible||suburbOf.owner==0))sprite=HamletTexture(suburbOf,t.x,t.y);
                City portOf=null;if(!water&&portCells.TryGetValue(t.y*width+t.x,out portOf)&&(visible||portOf.owner==0))sprite=IsoPicture(7,0,0);else portOf=null;
                if(sprite!=null)GUI.DrawTexture(new Rect(sx-PixelArt.AX*s,gy-PixelArt.AY*s,64*s,64*s),sprite);
                if(portOf!=null&&!portOf.portLinked&&portOf.owner==0){var warn=new Rect(sx+8*s,gy-22*s,9*s,9*s);Fill(warn,new Color(.85f,.2f,.15f,.92f));GUI.Label(warn,"!",small);}
                var improvement=new Rect(sx-18*s,gy-22*s,36*s,36*s);
                if(t.mine)DrawArt("mine",improvement);if(t.fort)DrawArt("fort",improvement);
                if(t.village&&visible){DrawArt("village",improvement);if(t.villageOwner>=0){var pole=new Rect(sx+10*s,gy-26*s,Mathf.Max(1,1.5f*s),12*s);Fill(pole,new Color(.3f,.22f,.14f));Fill(new Rect(pole.xMax,pole.y,7*s,5*s),nations[t.villageOwner]);}}
                if(!visible&&!string.IsNullOrEmpty(t.rememberedCity)){GUI.DrawTexture(new Rect(sx-32*s,gy-44*s,64*s,64*s),SettlementTexture(CityVisualEra(0),3,0,t.x+t.y),ScaleMode.ScaleToFit,true);isoCityNames.Add(t.rememberedCity);isoCityPos.Add(new Vector2(sx,gy));isoCityInk.Add(new Color(.6f,.62f,.66f));isoCityRefs.Add(null);}
                GUI.color=old;
                int cell=t.y*width+t.x;City c;viewCities.TryGetValue(cell,out c);if(c!=null&&!(c.owner==0||visible))c=null;
                if(t.resource>0){isoResTiles.Add(t);isoResPos.Add(new Vector2(sx,gy));}
                if(c!=null){DrawCityArt(c,new Rect(sx-32*s,gy-44*s,64*s,64*s));isoCityNames.Add(c.name);isoCityPos.Add(new Vector2(sx,gy));isoCityInk.Add(nations[c.owner]);isoCityRefs.Add(c);}
                Unit first=null,picked=null;int count=0;List<Unit> stackAt;
                if(viewStacks.TryGetValue(cell,out stackAt))foreach(var a in stackAt){if(!(a.owner==0||visible))continue;count++;if(first==null)first=a;if(a.id==selected)picked=a;}
                // At sea the ship is drawn, not the units it carries.
                if(picked==null&&water&&stackAt!=null){var ship=stackAt.Find(a=>World.IsNaval(a.type)&&(a.owner==0||visible));if(ship!=null)first=ship;}
                var shown=picked??first;
                if(shown!=null){DrawUnitArt(shown,new Rect(sx-20*s,gy-36*s,40*s,40*s),count);}
            }
            // Unexplored land lies under banks of cloud that spill a little over the edge of the known world.
            DrawCloudShadows(s);
            foreach(var f in isoFog)GUI.DrawTexture(new Rect(f.x-48*s*1.1f,f.y-38*s*1.1f,96*s*1.1f,64*s*1.1f),IsoPicture(9,(int)f.z,0));
            // Resource badges go on after every tile so the tiles in front can never cover them.
            for(int n=0;n<isoResTiles.Count;n++){var p=isoResPos[n];DrawResource(isoResTiles[n],new Rect(p.x-36*s,p.y+15*s-61*s,61*s,61*s));}
            // City names sit just below each settlement, drawn last so neither tiles nor clouds cover them.
            for(int n=0;n<isoCityNames.Count;n++)DrawCityPlate(isoCityRefs[n],isoCityNames[n],new Vector2(isoCityPos[n].x,isoCityPos[n].y+24*s),isoCityInk[n]);
            DrawIsoOverlay(s);
            compositeBudgetOn=false;
        }
        // Soft cloud shadows drift slowly across the map so the world feels alive. They move with the map as it scrolls.
        Texture2D cloudTexture;
        void DrawCloudShadows(float s)
        {
            if(cloudTexture==null){cloudTexture=PixelTexture(PixelArt.CloudShadows());cloudTexture.wrapMode=TextureWrapMode.Repeat;}
            float tile=128*3*s,t=Time.realtimeSinceStartup;
            float offX=(-(camI-camJ)*isoWidth/2+t*9f)%tile,offY=(-(camI+camJ)*isoWidth*PixelArt.DH/PixelArt.DW/2+t*4f)%tile;if(offX>0)offX-=tile;if(offY>0)offY-=tile;
            for(float y=offY;y<MapH;y+=tile)for(float x=offX;x<MapW;x+=tile)GUI.DrawTexture(new Rect(x,y,tile,tile),cloudTexture);
        }
        // How far a tile's surface is raised on screen: land (as the player knows it) stands Elev pixels above water.
        float Lift(int x,int y,float s){return 0;}
        // Selection outline, destination marker and stack labels go on top so scenery never hides them.
        void DrawIsoOverlay(float s)
        {
            int width=world.Width;float sx,sy;
            var chosen=Selected();
            if(chosen!=null)
            {
                IsoScreen(chosen.x,chosen.y,out sx,out sy);sy-=Lift(chosen.x,chosen.y,s);
                GUI.DrawTexture(new Rect(sx-32*s,sy-PixelArt.DH/2*s,64*s,PixelArt.DH*s),IsoPicture(4,4,0));
                if(chosen.hasOrder)
                {
                    IsoScreen(chosen.destinationX,chosen.destinationY,out sx,out sy);
                    sy-=Lift(chosen.destinationX,chosen.destinationY,s);var marker=new Rect(sx-10,sy-10,20,20);Fill(marker,new Color(1,.8f,0,.55f));GUI.Label(marker,"X",small);
                }
            }
            else if(cityId>=0)
            {
                var city=world.cities.Find(a=>a.id==cityId);
                if(city!=null){IsoScreen(city.x,city.y,out sx,out sy);sy-=Lift(city.x,city.y,s);GUI.DrawTexture(new Rect(sx-32*s,sy-PixelArt.DH/2*s,64*s,PixelArt.DH*s),IsoPicture(4,4,0));}
            }
        }
    }
}
