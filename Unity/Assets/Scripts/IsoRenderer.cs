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
        // Hex games always use the pixel-art view; the classic view only draws square tiles.
        bool classicView;
        bool diamondView{get{return world==null||world.Hex||!classicView;}set{classicView=!value;}}
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

        Color32[] pixelBuffer=new Color32[0];
        void FillTexture(Texture2D tex,PixelCanvas cv,bool mips,bool release)
        {
            if(pixelBuffer.Length!=cv.w*cv.h)pixelBuffer=new Color32[cv.w*cv.h];var pixels=pixelBuffer;var p=cv.p;int w=cv.w,h=cv.h;
            for(int y=0;y<h;y++){int to=(h-1-y)*w,from=y*w;for(int x=0;x<w;x++){uint c=p[from+x];pixels[to+x]=new Color32((byte)(c>>16),(byte)(c>>8),(byte)c,(byte)(c>>24));}}
            tex.SetPixels32(pixels);tex.Apply(mips,release);
        }
        Texture2D PixelTexture(PixelCanvas cv)
        {
            var tex=new Texture2D(cv.w,cv.h,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            FillTexture(tex,cv,false,false);return tex;
        }
        // Art that may be drawn at any size: smooth scaling, with smaller copies for when the map is zoomed out.
        Texture2D SoftTexture(PixelCanvas cv)
        {
            var tex=new Texture2D(cv.w,cv.h,TextureFormat.RGBA32,true){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            FillTexture(tex,PixelArt.Bleed(cv),true,false);return tex;
        }
        // Ground and tile shapes keep their crisp edges so neighbouring tiles meet exactly; the memory copy is dropped once uploaded.
        Texture2D TileTexture(PixelCanvas cv)
        {
            var tex=new Texture2D(cv.w,cv.h,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            FillTexture(tex,cv,false,true);return tex;
        }
        // kind: 1 sprite, 2 farm, 3 road, 4 misc, 5 sea resource icon. Small shared pictures, never evicted.
        Texture2D IsoPicture(int kind,int a,int b)
        {
            bool hexShape=world!=null&&world.Hex;long key=((long)(kind+(hexShape?64:0))<<40)|((long)a<<8)|(long)b;Texture2D tex;
            if(isoPictures.TryGetValue(key,out tex))return tex;
            // Supplied art (Resources/Sprites, e.g. from SpriteCook) replaces the drawn pictures; see Design/ART-SLOTS.md.
            // Cloud puffs carry their variant in a (cloud-V); the other slots in b.
            // Hills use the drawn mounds rather than the supplied boulder picture (owner request); the file is kept.
            // Hills use the painted rounded hills (owner request, 8 Nov 2026: "hills-V"); the old boulder file stays unused.
            string slot=kind==1&&a==2?"hills":kind==1?"terrain-"+a.ToString("00"):kind==6?"hamlet-"+a:kind==7?"port":kind==9?"cloud":null;
            // The third hill picture ("hills-2") stands on a steeper base with a grey rim that cannot be rounded off cleanly, so hills use the
            // other two (owner report, 10 Nov 2026: square bases); the file is kept.
            if(slot!=null){var supplied=SpriteSlot(slot+"-"+(kind==9?a:slot=="hills"?b%2:b))??SpriteSlot(slot);if(supplied!=null){isoPictures[key]=supplied;return supplied;}}
            // Painted wild animals (SpriteCook, 8 Nov 2026: Sprites/animal-0..6 in AnimalCatalog order); the drawn ones remain the fallback.
            if(kind==12){var beastArt=SpriteSlot("animal-"+a);if(beastArt!=null){isoPictures[key]=beastArt;return beastArt;}}
            PixelCanvas cv=null;
            if(kind==1)cv=PixelArt.Sprite(a,b);else if(kind==6)cv=PixelArt.Hamlet(a,b);else if(kind==7)cv=PixelArt.PortSprite();else if(kind==8)cv=PixelArt.HudIcon(a);else if(kind==9)cv=PixelArt.CloudPuff(a);else if(kind==2)cv=PixelArt.Farm(a);else if(kind==3)cv=PixelArt.Road(a);else if(kind==5)cv=PixelArt.ResourceIcon(a);else cv=PixelArt.Misc(a);
            // Tile-shaped overlays (farms, roads, borders, selection, unexplored) are drawn as diamonds in the diagonal view.
            if(kind==10)cv=PixelArt.HexEdge(a);else if(kind==11)cv=PixelArt.HexRoad(a);else if(kind==12)cv=PixelArt.AnimalSprite(a);
            else if(cv!=null&&(kind==2||kind==3||kind==4&&(a<=6||a==9)))cv=hexShape?PixelArt.ToHex(cv):PixelArt.ToDiamond(cv);
            // Icons are drawn at four times their design size and sprites at twice, so they stay sharp on large screens.
            tex=cv==null?null:kind==8?SoftTexture(PixelArt.Scale2x(PixelArt.Scale2x(cv))):kind==1||kind==6||kind==7||kind==9||kind==12||kind==5?SoftTexture(PixelArt.Scale2x(cv)):kind==2||kind==3||kind==4||kind==10||kind==11?TileTexture(PixelArt.Scale2x(cv)):PixelTexture(cv);isoPictures[key]=tex;return tex;
        }
        bool AnimalNearPlayer(Animal a){foreach(var u in world.units)if(u.owner==0&&world.Distance(u.x,u.y,a.x,a.y)<=2)return true;return false;}
        void ClearIso()
        {
            // Supplied art (spriteSlots) is a loaded asset shared by the cache, so only the drawn pictures are destroyed.
            foreach(var t in isoTiles.Values)Destroy(t);foreach(var t in isoPictures.Values)if(t!=null&&!spriteSlots.ContainsValue(t))Destroy(t);
            isoTiles.Clear();isoTileOrder.Clear();isoPictures.Clear();isoShown.Clear();
        }
        // The ground rectangle of a tile centred at (sx,sy), the selection outline, and the road picture for a tile.
        Rect TileRect(float sx,float sy,float s){if(!world.Hex)return new Rect(sx-32*s,sy-PixelArt.DH/2*s,64*s,PixelArt.DH*s);float h=PixelArt.HH*s;return new Rect(sx-32*s-.5f,sy-h/2-.5f,64*s+1,h+1);}// a pixel of overlap, so rounding leaves no dark seam between hexes
        Texture2D SelectionPicture(){return world.Hex?IsoPicture(10,6,0):IsoPicture(4,4,0);}
        Texture2D RoadPicture(Tile t)
        {
            int mask=0;
            if(world.Hex){for(int k=0;k<6;k++)if(world.RoadConnects(t,World.HexDi[k],World.HexDj[k]))mask|=1<<k;return IsoPicture(11,mask,0);}
            for(int k=0;k<8;k++)if(world.RoadConnects(t,RoadDx[k],RoadDy[k]))mask|=1<<k;
            return IsoPicture(3,mask,0);
        }
        // Screen pixels from one row of tiles to the next at width tw.
        float RowPixels(float tw){return world.Hex?tw*PixelArt.HR/PixelArt.HW:tw*PixelArt.DH/PixelArt.DW/2;}
        static int IsoVariant(int x,int y){unchecked{int h=x*73856093^y*19349663;return (h&0x7fffffff)%3;}}
        static int GroveVariant(int x,int y){unchecked{int h=x*83492791^y*1640531527;return (h&0x7fffffff)%8;}}

        // ---------- camera ----------
        void ToggleView(){if(world!=null&&world.Hex){status="The classic view draws square tiles, so it is not available on a hex map.";return;}diamondView=!diamondView;var u=Selected();if(u!=null)FocusTile(u.x,u.y);ResetMapGesture();}
        void ClampCamera()
        {
            if(world.Sphere){float w=world.Width;camI%=w;if(camI<0)camI+=w;}
            else camI=Mathf.Clamp(camI,0,world.Width-1);
            camJ=Mathf.Clamp(camJ,world.RealmTop(viewRealm),world.RealmBottom(viewRealm)-1);
        }
        void FocusTile(int x,int y)
        {
            // The map shows one world at a time; looking at a tile shows the world it belongs to.
            {var ft=world.At(x,y);if(ft!=null&&ft.realm>=0&&ft.realm!=viewRealm){viewRealm=ft.realm;atlasTexture=null;if(viewRealm!=0)globeZoom=false;}}
            if(!diamondView){if(world.Staggered){mapScroll=new Vector2((x+(y&1)*.5f)*tilePixels-MapW/2,y*tilePixels/2f-MapH/2);ClampMapScroll();return;}mapScroll=new Vector2(x*tilePixels-MapW/2,y*tilePixels-MapH/2);ClampMapScroll();return;}
            // Staggered rows: the camera is in columns (with the odd-row half step) and rows.
            float i=x+(world.Rows?(y&1)*.5f:0);if(world.Sphere){float w=world.Width;i=i+w*(float)Math.Round((camI-i)/w);}
            camI=i;camJ=y;ClampCamera();
        }
        void IsoPan(float dx,float dy)
        {
            float tw=isoWidth,th=tw*PixelArt.DH/PixelArt.DW;
            // Staggered rows pan straight: right is east, down is south.
            if(world.Rows){camI+=dx/tw;camJ+=dy/RowPixels(tw);ClampCamera();return;}
            // Screen movement of (dx,dy) pixels moves the camera by whole and part tiles, through the diagonal projection.
            float a=dx/(tw/2),b=dy/(th/2);
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
            double dx,dy;
            if(world.Hex)PixelArt.HexScreenOf(i,y,camI,camJ,isoWidth,out dx,out dy);else if(world.Staggered)PixelArt.StaggerScreenOf(i,y,camI,camJ,isoWidth,out dx,out dy);else PixelArt.ScreenOf(i,y,camI,camJ,isoWidth,out dx,out dy);
            sx=IsoCentreX+(float)dx;sy=IsoCentreY+(float)dy;
        }
        // Which map tile is under a point given in map-view coordinates (0,0 = top left of the view).
        bool IsoPick(Vector2 local,out int x,out int y)
        {
            // Land and water are level, so the tile under the pointer is simply the diamond it falls in.
            int i,j;
            if(world.Hex)PixelArt.HexPickOf(local.x-IsoCentreX,local.y-IsoCentreY,camI,camJ,isoWidth,out i,out j);else if(world.Staggered)PixelArt.StaggerPickOf(local.x-IsoCentreX,local.y-IsoCentreY,camI,camJ,isoWidth,out i,out j);
            else PixelArt.PickOf(local.x-IsoCentreX,local.y-IsoCentreY,camI,camJ,isoWidth,out i,out j);
            var t=world.At(i,j);if(t==null){x=-1;y=-1;return false;}
            x=t.x;y=t.y;return true;
        }
        // Tile under the mouse in either view. Mouse position is in screen space.
        bool MapTileAt(Vector2 mouse,out int x,out int y)
        {
            if(diamondView)return IsoPick(mouse-new Vector2(MapX,MapY),out x,out y);
            return ClassicPick(mouse.x-MapX+mapScroll.x,mouse.y-MapY+mapScroll.y,out x,out y);
            var t=world.At(x,y);if(t==null)return false;x=t.x;y=t.y;return true;
        }

        // ---------- terrain pictures ----------
        int IsoKnownTerrain(Tile n){return n==null||!n.seen?-1:world.KnownTerrain(n);}
        readonly int[] isoN9=new int[9];
        // Each tile's picture depends on its own position (world-space noise) and on the 3 x 3 block of terrain the player
        // knows, so the cache key is the tile plus a hash of that block, its river mask and the water animation frame.
        Texture2D IsoGround(Tile t,int i,int j,int frame)
        {
            int mask=0;bool anyWater=false;
            // Neighbours by grid step from the tile itself, so the same picture serves diagonal and staggered maps.
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int n=IsoKnownTerrain(world.Step(t.x,t.y,dx,dy));if(dx==0&&dy==0)n=t.terrain;isoN9[(dy+1)*3+dx+1]=n;if(n>=0&&PixelArt.IsWater(n))anyWater=true;}
            mask=RiverMaskOf(t);
            int fr=anyWater?frame:0;int sig=world.Hex?19:17;unchecked{foreach(int n in isoN9)sig=sig*31+n+1;sig=sig*31+mask;}
            // Zoomed in, the ground is drawn at twice the detail; zoomed out the plain picture looks better and costs less.
            bool sharp=cityScreen||isoWidth>=64;int index=t.y*world.Width+t.x;long key=((long)(uint)sig<<32)|(uint)(index*8+fr+(sharp?4:0));
            Texture2D tex;
            if(isoTiles.TryGetValue(key,out tex)){Texture2D cur;if(!isoShown.TryGetValue(index,out cur)||cur!=tex)isoShown[index]=tex;return tex;}
            if(compositeBudgetOn&&compositeClock.ElapsedMilliseconds>=CompositeBudgetMs)
            {
                Texture2D previous;
                if(isoShown.TryGetValue(index,out previous)&&previous!=null)return previous;
            }
            {var composed=world.Hex?PixelArt.ComposeHex(isoN9,t.x,t.y,fr,mask):PixelArt.ToDiamond(PixelArt.Compose(isoN9,t.x,t.y,fr,mask));tex=TileTexture(sharp?PixelArt.Scale2x(composed):composed);}
            if(isoTiles.Count>=4000){long oldest=isoTileOrder.Dequeue();Texture2D gone;if(isoTiles.TryGetValue(oldest,out gone)){Destroy(gone);isoTiles.Remove(oldest);}}
            isoTiles[key]=tex;isoTileOrder.Enqueue(key);isoShown[index]=tex;return tex;
        }
        // On a river tile: the sides its water runs to (another river tile, the sea or a lake), as the player knows them. On a sea or
        // lake tile: the sides a river flows in from, so the mouth is drawn on both tiles (PixelArt.RiverMouth).
        int RiverMaskOf(Tile t)
        {
            int mask=0;int[] offI=world.Hex?World.HexDi:new[]{0,1,0,-1},offJ=world.Hex?World.HexDj:new[]{-1,0,1,0};int dirs=offI.Length;
            if(t.terrain==7){int flow=world.RiverLinks(world.At(t.x,t.y));for(int k=0;k<dirs;k++){int n=IsoKnownTerrain(world.Step(t.x,t.y,offI[k],offJ[k]));if(n>=0&&(n==7||PixelArt.IsWater(n))&&(flow<0||(flow&(1<<k))!=0))mask|=1<<k;}}
            else if(PixelArt.IsWater(t.terrain))for(int k=0;k<dirs;k++){var n=world.Step(t.x,t.y,offI[k],offJ[k]);if(IsoKnownTerrain(n)!=7)continue;int flow=world.RiverLinks(world.At(n.x,n.y));if(flow<0||(flow&(1<<((k+dirs/2)%dirs)))!=0)mask|=1<<k;}
            return mask;
        }
        // A farm on a river tile leaves the river clear (owner request, 10 Nov 2026): the field picture is cut away over the water
        // and a little either side of it, so the furrows stop at the banks. One picture is kept for each field, river shape and layout.
        readonly Dictionary<long,Texture2D> riverFields=new Dictionary<long,Texture2D>();
        Texture2D FieldAroundRiver(Texture2D field,int fieldVariant,Tile t,float spanX,float spanY)
        {
            int mask=RiverMaskOf(t),variant=(t.x*7+t.y*3)%3;long key=fieldVariant|((long)mask<<4)|((long)variant<<12)|(world.Hex?1L<<16:0);
            Texture2D made;if(riverFields.TryGetValue(key,out made)&&made!=null)return made;
            try
            {
                var px=ReadArtPixels(field);int w=field.width,h=field.height;var river=PixelArt.RiverShape(world.Hex,mask,variant);var cv=new PixelCanvas(w,h);
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    uint c=px[y*w+x];if((c>>24)==0)continue;
                    // Where this field pixel falls on the tile picture, and how near the river is there.
                    double rx=(.5+((x+.5)/w-.5)*spanX)*river.w,ry=(.5+((y+.5)/h-.5)*spanY)*river.h;double near=99;
                    for(int dy=-4;dy<=4;dy++)for(int dx=-4;dx<=4;dx++){int qx=(int)Math.Floor(rx)+dx,qy=(int)Math.Floor(ry)+dy;if(qx<0||qy<0||qx>=river.w||qy>=river.h||(river.Get(qx,qy)>>24)==0)continue;double d=Math.Sqrt(dx*dx+dy*dy);if(d<near)near=d;}
                    if(near<=2.2)continue;                         // the water and its grassy bank
                    if(near<=3.4){uint a=(uint)((c>>24)*.55);c=(a<<24)|(c&0xFFFFFFu);} // the field thins out toward the bank
                    cv.Set(x,y,c);
                }
                made=SoftTexture(cv);themeTextures.Add(made);
            }
            catch(Exception){made=field;}
            riverFields[key]=made;return made;
        }
        // Which side of a river tile is freer of water: -1 west, +1 east. Mines and forts stand there, beside the river, not on it.
        int DrySide(Tile t){int mask=RiverMaskOf(t),east=0,west=0;int[] offI=world.Hex?World.HexDi:new[]{0,1,0,-1};for(int k=0;k<offI.Length;k++)if((mask&(1<<k))!=0){if(offI[k]>0)east++;else if(offI[k]<0)west++;}return east<west?1:east>west?-1:((t.x+t.y)&1)==0?1:-1;}
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
            if(dragging){var m=Event.current.mousePosition;var u=world.units.Find(a=>a.id==dragUnit);if(u!=null){float sx,sy;IsoScreen(u.x,u.y,out sx,out sy);DrawOrderLine(new Vector2(MapX+sx,MapY+sy-6*isoWidth/64f),m);}GUI.Label(new Rect(m.x+15,m.y+12,220,30),roadMode?"Release to build a road there":"Release to send the unit here",body);}
        }
        readonly Dictionary<int,Animal> viewAnimals=new Dictionary<int,Animal>();readonly List<Vector2> isoGates=new List<Vector2>();readonly List<int> isoGateStyles=new List<int>();
        // Painted gateways (owner pictures, 8 Nov 2026): Sprites/gate-open-N or gate-sealed-N by the gateway's design, standing on the
        // tile with their ground patch on the tile's lower tip. False when the art is missing, so the drawn gateway is used instead.
        // Gateways are drawn tile-sized, their ground patch ending at the tile's lower tip (owner request, 9 Nov 2026).
        bool DrawGateArt(int style,bool open,float sx,float gy,float s){var art=SpriteSlot((open?"gate-open-":"gate-sealed-")+style);if(art==null)return false;
            // Map-aligned gate pictures (owner's pack, 10 Nov 2026): 256 x 256 with the ground line 16 pixels above the bottom edge and the
            // platform about 87% of the width. Drawn one tile wide, so the platform fits inside the tile, its front corner just above
            // the tile's lower tip; open and sealed pictures of a gate share this placement.
            float w=64*s,h=w*art.height/Mathf.Max(1,art.width);GUI.DrawTexture(new Rect(sx-w/2,gy+TileHalf()*s-2*s+h*GateBaseline-h,w,h),art,ScaleMode.ScaleToFit,true);return true;}
        const float GateBaseline=16f/256f;
        float TileHalf(){return (world.Hex?PixelArt.HH:PixelArt.DH)/2f;}
        // Ruins in five architectural styles (owner request, 9 Nov 2026), suited to the land: sandstone in desert and steppe, a stepped
        // pyramid in jungle and mangrove, otherwise classical columns, a stone circle or an eastern temple chosen by place.
        static int RuinStyle(Tile t){if(t.terrain==3||t.terrain==10||t.terrain==20)return 1;if(t.terrain==11||t.terrain==19)return 2;int[] rest={0,3,4};return rest[((t.x*37+t.y*11)&0x7fffffff)%3];}
        // A settlement fits inside its own tile (owner request, 9 Nov 2026: no overlap onto the next tile): 54 pixels, its lower tip on the tile's.
        Rect SettlementRect(float sx,float gy,float s){float size=54*s;return new Rect(sx-size/2,gy+TileHalf()*s-size,size,size);}
        Texture2D ruinTexture;
        readonly Dictionary<int,Texture2D> gateTextures=new Dictionary<int,Texture2D>();
        readonly List<KeyValuePair<Unit,Rect>> isoUnitDraws=new List<KeyValuePair<Unit,Rect>>();readonly List<int> isoUnitCounts=new List<int>();
        readonly List<Vector3> isoFog=new List<Vector3>(),isoMist=new List<Vector3>();readonly List<Vector4> isoCells=new List<Vector4>();
        // Dark mist colours: the void and unexplored ground, the cloud banks over it, the veil over remembered land and its tint.
        // Unexplored land is black mist (owner request, 6 Nov 2026): soft black banks with faint grey wisps moving through them.
        static readonly Color ResourceRemembered=new Color(.36f,.39f,.47f,.72f);readonly List<bool> isoResSeen=new List<bool>();
        static readonly Color MistDeep=new Color(.006f,.006f,.01f),MistWisp=new Color(1,1,1,.8f),MistThin=new Color(.14f,.16f,.21f,.3f),Remembered=new Color(.58f,.61f,.68f,1);
        Texture2D mistBank,mistWisp;
        // Glass-smooth texture for mist: bilinear, unlike the crisp pixel art.
        Texture2D SoftPicture(PixelCanvas cv){var tex=PixelTexture(cv);tex.filterMode=FilterMode.Bilinear;themeTextures.Add(tex);return tex;}
        static readonly int[] RoadDx={-1,0,1,1,1,0,-1,-1},RoadDy={-1,-1,-1,0,1,1,1,0};
        void DrawIsoContents()
        {
            compositeBudgetOn=true;compositeClock.Reset();compositeClock.Start();
            ClampCamera();
            float tw=isoWidth,th=tw*PixelArt.DH/PixelArt.DW,s=tw/64f;int width=world.Width;
            int frame=(int)(Time.realtimeSinceStartup*2.5f)%3;
            Fill(new Rect(0,0,MapW,MapH),MistDeep);isoFog.Clear();isoMist.Clear();
            viewStacks.Clear();
            foreach(var unit in world.units){List<Unit> stackAt;int cell=unit.y*width+unit.x;if(!viewStacks.TryGetValue(cell,out stackAt)){stackAt=new List<Unit>(2);viewStacks[cell]=stackAt;}stackAt.Add(unit);}
            viewCities.Clear();foreach(var city in world.cities)viewCities[city.y*width+city.x]=city;
            viewAnimals.Clear();if(world.animals!=null)foreach(var a in world.animals)viewAnimals[a.y*width+a.x]=a;
            if(Event.current.type==EventType.Repaint)UpdateGlides();
            if(Event.current.type==EventType.Repaint)BuildSuburbs();
            // Diagonal view: tiles go back to front by diagonal (i + j), so raised scenery overlaps the tiles behind it.
            // R covers every tile whose diamond can touch the view, with a margin for tall scenery below the bottom edge.
            int R=(int)Math.Ceiling(MapW/tw+MapH/th)+3,ci=(int)Math.Round(camI),cj=(int)Math.Round(camJ);
            int pooled=0;isoUnitDraws.Clear();isoUnitCounts.Clear();isoResTiles.Clear();isoResPos.Clear();isoResSeen.Clear();isoCityNames.Clear();isoCityPos.Clear();isoCityInk.Clear();isoCityRefs.Clear();
            // Staggered rows: draw row by row from the back (north); every tile of a row is level, so order within it is free.
            isoCells.Clear();
            if(world.Rows)
            {
                float rowH=RowPixels(tw);int top=(int)Math.Floor(camJ-MapH/2/rowH)-2,bottom=(int)Math.Ceiling(camJ+MapH/2/rowH+64*s/rowH)+1;int left=(int)Math.Floor(camI-MapW/2/tw)-2,right=(int)Math.Ceiling(camI+MapW/2/tw)+2;
                for(int j=Math.Max(world.RealmTop(viewRealm),top);j<=Math.Min(world.RealmBottom(viewRealm)-1,bottom);j++)for(int i=left;i<=right;i++)
                {float sx=IsoCentreX+(i+(j&1)*.5f-camI)*tw,sy=IsoCentreY+(j-camJ)*rowH;if(sx<-tw||sx>MapW+tw||sy<-th||sy>MapH+64*s)continue;isoCells.Add(new Vector4(i,j,sx,sy));}
            }
            else for(int sum=-2*R;sum<=2*R;sum++)for(int di=-R;di<=R;di++)
            {
                // Diagonal view: tiles go back to front by diagonal (i + j).
                int dj=sum-di;if(dj<-R||dj>R)continue;int i=ci+di,j=cj+dj;
                if(j<world.RealmTop(viewRealm)||j>=world.RealmBottom(viewRealm)||!world.Sphere&&(i<0||i>=width))continue;
                float sx=IsoCentreX+((i-camI)-(j-camJ))*tw/2,sy=IsoCentreY+((i-camI)+(j-camJ))*th/2;
                if(sx<-tw||sx>MapW+tw||sy<-th||sy>MapH+64*s)continue;isoCells.Add(new Vector4(i,j,sx,sy));
            }
            foreach(var cellAt in isoCells)
            {
                int i=(int)cellAt.x,j=(int)cellAt.y;float sx=cellAt.z,sy=cellAt.w;
                var source=world.At(i,j);if(source==null||source.realm!=viewRealm)continue;
                bool visible=world.Visible(source.x,source.y);Tile t=source;
                if(source.seen&&!visible){if(pooled==knownPool.Count)knownPool.Add(new Tile());t=knownPool[pooled++];world.CopyKnown(source,t);}
                if(!source.seen){var keep=GUI.color;GUI.color=MistDeep;GUI.DrawTexture(TileRect(sx,sy,s),IsoPicture(4,5,0));GUI.color=keep;isoFog.Add(new Vector3(sx,sy,(i*7+j*13)&3));continue;}
                bool water=PixelArt.IsWater(t.terrain);float gy=sy;
                if(!visible)isoMist.Add(new Vector3(sx,sy,(i*5+j*11)&3));
                var old=GUI.color;if(!visible)GUI.color=Remembered;
                var ground=TileRect(sx,sy,s);
                GUI.DrawTexture(ground,IsoGround(t,i,j,frame));
                if(!water&&t.farm){var field=SpriteSlot("field-"+(IsoVariant(t.x,t.y)%3));if(field!=null){float fw=ground.width*.86f,fh=fw*field.height/Mathf.Max(1,field.width);if(t.terrain==7)field=FieldAroundRiver(field,IsoVariant(t.x,t.y)%3,t,.86f,fh/Mathf.Max(1,ground.height));GUI.DrawTexture(new Rect(ground.center.x-fw/2,ground.center.y-fh/2,fw,fh),field,ScaleMode.StretchToFill,true);}else{var farm=IsoPicture(2,IsoVariant(t.x,t.y),0);if(farm!=null)GUI.DrawTexture(ground,farm);}}
                if(t.road)
                {
                    var road=RoadPicture(t);if(road!=null)GUI.DrawTexture(ground,road);
                }
                // Territory outline along edges that touch another owner or unknown ground.
                if(t.owner>=0)
                {
                    // A faint wash of the owner's colour over every owned tile makes the whole territory readable, not just its edge.
                    Color ink=nations[t.owner];ink.a=1f;GUI.color=new Color(ink.r,ink.g,ink.b,.13f);GUI.DrawTexture(ground,IsoPicture(4,9,0));int[] offI=world.Hex?World.HexDi:new[]{0,1,0,-1},offJ=world.Hex?World.HexDj:new[]{-1,0,1,0};
                    for(int k=0;k<offI.Length;k++)
                    {
                        var n=world.Step(source.x,source.y,offI[k],offJ[k]);
                        if(n!=null&&n.seen&&world.KnownOwner(n)==t.owner)continue;
                        GUI.color=ink;GUI.DrawTexture(ground,world.Hex?IsoPicture(10,k,0):IsoPicture(4,k,0));
                    }
                    GUI.color=visible?old:Remembered;
                }
                // Grassland groves use eight variants so lone trees do not repeat in a pattern; none on farms, roads or cities.
                // A farm replaces whatever stood on the tile (owner request), and cleared savanna or grassland has no trees left.
                bool meadow=t.terrain==0;bool built=t.farm||t.road||viewCities.ContainsKey(t.y*width+t.x);bool bare=t.farm||t.cleared&&(meadow||t.terrain==16);
                // Dry steppe shows its own ground and no picture (its supplied picture left straight lines; owner request, 9 Nov 2026).
                var sprite=water||bare||meadow&&built||t.terrain==20?null:IsoPicture(1,t.terrain,meadow?GroveVariant(t.x,t.y):IsoVariant(t.x,t.y));
                City suburbOf;bool hamlet=false;if(!water&&suburbCells.TryGetValue(t.y*width+t.x,out suburbOf)&&(visible||suburbOf.owner==0)){sprite=HamletTexture(suburbOf,t.x,t.y);hamlet=true;}
                City portOf=null;if(!water&&portCells.TryGetValue(t.y*width+t.x,out portOf)&&(visible||portOf.owner==0))sprite=IsoPicture(7,0,0);else portOf=null;
                // Savanna acacias are drawn at 60% so they read as scattered trees on grassland, not woodland (owner request).
                // Grassland grove pictures hold two or three large trees; at 55% they match the trees in the forest pictures
                // (owner request, 8 Nov 2026). Only the supplied variants 0-3; meadows (4-7) are drawn tufts.
                float treeScale=t.terrain==16&&!hamlet&&portOf==null?.6f:meadow&&!hamlet&&portOf==null&&GroveVariant(t.x,t.y)<4?.55f:1f;
                // Groves are drawn as a small cluster of three (owner request, 9 Nov 2026: denser), the back two first; farmsteads and ports
                // are drawn a little smaller so they stay on their own tile.
                if(sprite!=null&&meadow&&!hamlet&&portOf==null&&GroveVariant(t.x,t.y)<4){int v=GroveVariant(t.x,t.y);var back1=IsoPicture(1,0,(v+1)&3);var back2=IsoPicture(1,0,(v+2)&3);float bs=.42f;
                    if(back1!=null)GUI.DrawTexture(new Rect(sx-13*s-PixelArt.AX*s*bs,gy-6*s-PixelArt.AY*s*bs,64*s*bs,64*s*bs),back1);if(back2!=null)GUI.DrawTexture(new Rect(sx+13*s-PixelArt.AX*s*bs,gy-5*s-PixelArt.AY*s*bs,64*s*bs,64*s*bs),back2);
                    GUI.DrawTexture(new Rect(sx-PixelArt.AX*s*treeScale,gy+4*s-PixelArt.AY*s*treeScale,64*s*treeScale,64*s*treeScale),sprite);}
                else if(sprite!=null){if(portOf!=null)treeScale=.82f;else if(hamlet)treeScale=.5f;/* farmsteads at half size: their houses match the city's buildings (owner request, 9 Nov 2026) */
                    // No two hills are the same size or sit in quite the same place on their tile, so a range does not line up in rows
                    // (owner report, 10 Nov 2026).
                    float hillX=0,hillY=0;if(t.terrain==2&&!hamlet&&portOf==null){int hv=(t.x*92821+t.y*68917)&0x7fffffff;treeScale=.9f+(hv%19)/100f;hillX=((hv/19%9)-4)*.9f*s;hillY=((hv/171%7)-3)*.7f*s;}
                    GUI.DrawTexture(new Rect(sx-PixelArt.AX*s*treeScale+hillX,gy-PixelArt.AY*s*treeScale+hillY,64*s*treeScale,64*s*treeScale),sprite);}
                if(portOf!=null&&!portOf.portLinked&&portOf.owner==0){var warn=new Rect(sx+8*s,gy-22*s,9*s,9*s);Fill(warn,new Color(.85f,.2f,.15f,.92f));GUI.Label(warn,"!",small);}
                var improvement=new Rect(sx-18*s,gy-22*s,36*s,36*s);
                // Mines and forts are drawn at the map's angle and sized to their tile (owner request, 9 Nov 2026); the old upright pictures are the fallback.
                // On a river tile they are drawn smaller and to the drier side, so the river still runs past them (owner request, 10 Nov 2026).
                float besideRiver=t.terrain==7&&(t.mine||t.fort)?DrySide(t)*13*s:0,riverScale=t.terrain==7?.72f:1f;
                if(t.mine){var art=SpriteSlot("improve-mine");if(art!=null)GUI.DrawTexture(new Rect(sx-21*s*riverScale+besideRiver,gy+TileHalf()*s-44*s*riverScale-(1-riverScale)*10*s,42*s*riverScale,42*s*riverScale),art,ScaleMode.ScaleToFit,true);else DrawArt("mine",improvement);}
                if(t.fort){var art=SpriteSlot("improve-fort");if(art!=null)GUI.DrawTexture(new Rect(sx-23*s*riverScale+besideRiver,gy+TileHalf()*s-48*s*riverScale-(1-riverScale)*10*s,46*s*riverScale,46*s*riverScale),art,ScaleMode.ScaleToFit,true);else DrawArt("fort",improvement);}
                if(source.ruin){var ruinArt=SpriteSlot("ruin-"+RuinStyle(t));if(ruinArt!=null)GUI.DrawTexture(new Rect(sx-24*s,gy+TileHalf()*s-50*s,48*s,48*s),ruinArt,ScaleMode.ScaleToFit,true);else{if(ruinTexture==null)ruinTexture=PixelTexture(PixelArt.Ruins());GUI.DrawTexture(new Rect(sx-PixelArt.AX*s,gy-PixelArt.AY*s,64*s,64*s),ruinTexture);}}
                // A sealed gateway the player has found keeps its name on the map (owner request).
                {var story=world.livingWorld?world.StoryAt(source.x,source.y):null;if(story!=null&&story.deaths>=LivingCatalog.GraveyardAt){isoCityNames.Add("Graveyard");isoCityPos.Add(new Vector2(sx,gy));isoCityInk.Add(new Color(.7f,.7f,.75f));isoCityRefs.Add(null);}}
                if(source.gateLink>0&&source.realm==0&&world.gatesLocked&&!world.GateOpen(source)&&world.gatesFound!=null&&world.gatesFound.Contains(source.y*width+source.x)){isoGates.Add(new Vector2(sx,gy));isoGateStyles.Add(world.GateStyle(source));isoCityNames.Add("Sealed gateway");isoCityPos.Add(new Vector2(sx,gy));isoCityInk.Add(new Color(1f,.82f,.25f));isoCityRefs.Add(null);}
                if(source.gateLink>0&&DrawGateArt(world.GateStyle(source),world.GateOpen(source),sx,gy,s)){}
                else if(source.gateLink>0){var twin=world.GateTwin(source);int kind=world.gatesLocked?PixelArt.SealedGate:source.realm>0?0:twin!=null&&world.RealmDiscovered(0,twin.realm)?twin.realm:0;Texture2D gateTex;if(!gateTextures.TryGetValue(kind,out gateTex)){gateTex=PixelTexture(PixelArt.GatewaySprite(kind));gateTextures[kind]=gateTex;}GUI.DrawTexture(new Rect(sx-PixelArt.AX*s,gy-PixelArt.AY*s,64*s,64*s),gateTex);}
                if(t.village&&visible){DrawArt("village",improvement);if(t.villageOwner>=0){var pole=new Rect(sx+10*s,gy-26*s,Mathf.Max(1,1.5f*s),12*s);Fill(pole,new Color(.3f,.22f,.14f));Fill(new Rect(pole.xMax,pole.y,7*s,5*s),nations[t.villageOwner]);}}
                if(!visible&&!string.IsNullOrEmpty(t.rememberedCity)){GUI.DrawTexture(SettlementRect(sx,gy,s),SettlementTexture(CityVisualEra(0),3,0,t.x+t.y),ScaleMode.ScaleToFit,true);isoCityNames.Add(t.rememberedCity);isoCityPos.Add(new Vector2(sx,gy));isoCityInk.Add(new Color(.6f,.62f,.66f));isoCityRefs.Add(null);}
                GUI.color=old;
                int cell=t.y*width+t.x;City c;viewCities.TryGetValue(cell,out c);if(c!=null&&!(c.owner==0||visible))c=null;
                if(t.resource>0){isoResTiles.Add(t);isoResPos.Add(new Vector2(sx,gy));isoResSeen.Add(visible);}
                if(c!=null){DrawCityArt(c,SettlementRect(sx,gy,s));isoCityNames.Add(c.name);isoCityPos.Add(new Vector2(sx,gy));isoCityInk.Add(nations[c.owner]);isoCityRefs.Add(c);}
                Unit first=null,picked=null;int count=0;List<Unit> stackAt;
                if(viewStacks.TryGetValue(cell,out stackAt))foreach(var a in stackAt){if(!(a.owner==0||visible)||Gliding(a.id))continue;count++;if(first==null)first=a;if(a.id==selected)picked=a;}
                // At sea the ship is drawn, not the units it carries.
                if(picked==null&&water&&stackAt!=null){var ship=stackAt.Find(a=>World.IsNaval(a.type)&&(a.owner==0||visible));if(ship!=null)first=ship;}
                var shown=picked??first;
                if(visible&&world.wildlife){Animal beast;viewAnimals.TryGetValue(cell,out beast);if(beast!=null&&!AnimalGliding(beast.id)){var feet=new Rect(sx-20*s,gy-8*s,40*s,15*s);GUI.DrawTexture(feet,IsoPicture(4,8,0));// The painted animals are cropped tight, so they are drawn smaller than the old 64-pixel cells: about unit size, the
                    // mammoth, sloth and rhinoceros a little larger.
                    {bool painted=SpriteSlot("animal-"+beast.kind)!=null;float size=painted?(AnimalCatalog.Carnivore[beast.kind]?40:48):64;
                     GUI.DrawTexture(painted?new Rect(sx-size/2*s,gy-(size-6)*s,size*s,size*s):new Rect(sx-32*s,gy-42*s,64*s,64*s),IsoPicture(12,beast.kind,0),ScaleMode.ScaleToFit,true);}
                    // An animal's health shows during a fight (owner request, 8 Nov 2026): whenever it is hurt or one of your
                    // units stands within two tiles, a bar over its back, red-edged for a hunter.
                    if(beast.hp<10||AnimalNearPlayer(beast)){float bh=Mathf.Max(3,2.8f*s),bw=34*s,by=gy-(AnimalCatalog.Carnivore[beast.kind]?38:46)*s;bool hunter=AnimalCatalog.Carnivore[beast.kind];
                        Fill(new Rect(sx-bw/2-1,by-1,bw+2,bh+2),hunter?new Color(.45f,.08f,.06f,.9f):new Color(.08f,.1f,.12f,.85f));
                        Fill(new Rect(sx-bw/2,by,bw*Mathf.Clamp01(beast.hp/10f),bh),beast.hp>5?new Color(.37f,.8f,.47f):beast.hp>2?new Color(.95f,.75f,.3f):new Color(.9f,.3f,.25f));}}}
                if(shown!=null){var unitRect=new Rect(sx-20*s,gy-36*s,40*s,40*s);DrawUnitArt(shown,unitRect,count);isoUnitDraws.Add(new KeyValuePair<Unit,Rect>(shown,unitRect));isoUnitCounts.Add(count);}
            }
            // Fog of war as dark mist (owner request): a thin drifting veil over land seen before but not watched now, and
            // thick dark banks (the cloud puffs, darkened) over unexplored land, spilling a little over the known world's edge.
            DrawGlides(tw,s);
            DrawCloudShadows(s);
            float clock=Time.realtimeSinceStartup;var before=GUI.color;
            if(mistBank==null){mistBank=SoftPicture(PixelArt.MistBank());mistWisp=SoftPicture(PixelArt.MistWisp());}
            foreach(var f in isoMist){float drift=Mathf.Sin(clock*.35f+f.z*1.7f+f.x*.013f)*5*s;GUI.color=MistThin;GUI.DrawTexture(new Rect(f.x-64*s+drift,f.y-44*s,128*s,88*s),mistBank);}
            GUI.color=Color.white;foreach(var f in isoFog){float drift=Mathf.Sin(clock*.25f+f.z*2.1f+f.y*.011f)*5*s;GUI.DrawTexture(new Rect(f.x-64*s+drift,f.y-44*s,128*s,88*s),mistBank);}
            GUI.color=MistWisp;foreach(var f in isoFog){if(f.z!=0)continue;float drift=clock*2.2f%64f,sway=Mathf.Sin(clock*.2f+f.x*.02f)*6*s;GUI.DrawTexture(new Rect(f.x-64*s+drift*s-32*s,f.y-30*s+sway,128*s,60*s),mistWisp);}
            GUI.color=before;
            // A found gateway stands clear of the mist, so it stays on the map however far the fog closes in.
            for(int gi=0;gi<isoGates.Count;gi++){var g=isoGates[gi];if(gi<isoGateStyles.Count&&DrawGateArt(isoGateStyles[gi],false,g.x,g.y,s))continue;Texture2D gateTex;if(!gateTextures.TryGetValue(PixelArt.SealedGate,out gateTex)){gateTex=PixelTexture(PixelArt.GatewaySprite(PixelArt.SealedGate));gateTextures[PixelArt.SealedGate]=gateTex;}GUI.DrawTexture(new Rect(g.x-PixelArt.AX*s,g.y-PixelArt.AY*s,64*s,64*s),gateTex);}isoGates.Clear();isoGateStyles.Clear();
            // Units stand in front of the cloud banks of unexplored land next to them (owner request): drawn again over the mist.
            for(int n=0;n<isoUnitDraws.Count;n++)DrawUnitArt(isoUnitDraws[n].Key,isoUnitDraws[n].Value,isoUnitCounts[n]);
            // Resource badges go on after every tile so the tiles in front can never cover them.
            // A resource on land that is remembered but not watched now is shadowed like the land under it (owner request, 10 Nov 2026).
            {var resInk=GUI.color;for(int n=0;n<isoResTiles.Count;n++){var p=isoResPos[n];GUI.color=isoResSeen[n]?resInk:ResourceRemembered;DrawResource(isoResTiles[n],new Rect(p.x-36*s,p.y+15*s-61*s,61*s,61*s));}GUI.color=resInk;}
            // City plates go on after the tiles (so no ground covers them), then any unit standing over a plate is drawn again on
            // top, so plates never hide units (owner request).
            var plates=new List<Rect>();
            for(int n=0;n<isoCityNames.Count;n++){var at=new Vector2(isoCityPos[n].x,isoCityPos[n].y+24*s);DrawCityPlate(isoCityRefs[n],isoCityNames[n],at,isoCityInk[n]);plates.Add(new Rect(at.x-120,at.y,240,36));}
            for(int n=0;n<isoUnitDraws.Count;n++){var ur=isoUnitDraws[n].Value;if(plates.Any(p=>p.x<ur.x+ur.width&&ur.x<p.x+p.width&&p.y<ur.y+ur.height&&ur.y<p.y+p.height))DrawUnitArt(isoUnitDraws[n].Key,ur,isoUnitCounts[n]);}
            DrawIsoOverlay(s);
            compositeBudgetOn=false;
        }
        // Soft cloud shadows drift slowly across the map so the world feels alive. They move with the map as it scrolls.
        Texture2D cloudTexture;
        void DrawCloudShadows(float s)
        {
            // One smooth sheet scrolled across the view. It used to be hard-edged blocks in separate rectangles, and as they drifted
            // their rows of blocks and joins crossed the screen as flashing horizontal lines (owner report, 6 Nov 2026).
            if(cloudTexture==null){cloudTexture=PixelTexture(PixelArt.CloudShadows());cloudTexture.wrapMode=TextureWrapMode.Repeat;cloudTexture.filterMode=FilterMode.Bilinear;}
            float tile=128*3*s,t=Time.realtimeSinceStartup;
            float offX=(-(world.Rows?camI*2:camI-camJ)*isoWidth/2+t*9f)%tile,offY=(-(world.Rows?camJ:camI+camJ)*RowPixels(isoWidth)+t*4f)%tile;
            GUI.DrawTextureWithTexCoords(new Rect(0,0,MapW,MapH),cloudTexture,new Rect(-offX/tile,1-(MapH-offY)/tile,MapW/tile,MapH/tile));
        }
        // How far a tile's surface is raised on screen: land (as the player knows it) stands Elev pixels above water.
        float Lift(int x,int y,float s){return 0;}
        // ---------- plotting a road (owner request, 6 Nov 2026) ----------
        // Road mode shows the course from the worker to the pointer as it moves, with what it will cost. Click to build, Shift-click to
        // fix a bend and carry on from there, right-click to cancel.
        readonly List<Tile> roadBends=new List<Tile>();List<Tile> roadPlotted=new List<Tile>();long roadPlotKey=-1;
        List<Tile> RoadCourseTo(Unit u,int x,int y)
        {
            var course=new List<Tile>();int fx=u.x,fy=u.y;
            foreach(var bend in roadBends){course.AddRange(world.RoadCourse(u.owner,fx,fy,bend.x,bend.y));fx=bend.x;fy=bend.y;}
            if(!(x==fx&&y==fy))course.AddRange(world.RoadCourse(u.owner,fx,fy,x,y));return course;
        }
        bool PlotRoadInput(Event e,bool inside,int x,int y)
        {
            var u=Selected();if(u==null||u.type!=1){roadMode=false;roadBends.Clear();return false;}
            if(e.type==EventType.MouseDown&&e.button==1){roadMode=false;roadBends.Clear();status="Road plan cancelled.";e.Use();return true;}
            if(e.type==EventType.MouseDown&&e.button==0&&inside)
            {
                e.Use();var picked=world.At(x,y);if(picked==null)return true;
                if(e.shift){roadBends.Add(picked);roadPlotKey=-1;status="Bend added. Keep plotting.";return true;}
                var course=RoadCourseTo(u,x,y);roadMode=false;roadBends.Clear();
                if(course.Count==0&&u.x==x&&u.y==y){status=world.StartJob(u,1)?"Building a road here.":world.JobReason(u,1);return true;}
                status=world.PlanRoad(u,course);return true;
            }
            return false;
        }
        void DrawRoadPlot(Unit u,float s)
        {
            if(u.type!=1)return;var mouse=Event.current.mousePosition;int x,y;bool over=MapView.Contains(mouse)&&!OverHud(mouse)&&IsoPick(mouse,out x,out y);if(!over){x=u.x;y=u.y;}IsoPick(mouse,out x,out y);
            long key;unchecked{key=x*100000L+y;key=key*31+u.x;key=key*31+u.y;key=key*31+roadBends.Count;}
            if(key!=roadPlotKey){roadPlotKey=key;roadPlotted=world.At(x,y)!=null?RoadCourseTo(u,x,y):new List<Tile>();}
            var gold=new Color(1f,.82f,.3f,.95f);float ax,ay;RouteAnchor(u.x,u.y,out ax,out ay);var at=new Vector2(ax,ay);
            foreach(var t in roadPlotted){RouteAnchor(t.x,t.y,out ax,out ay);var to=new Vector2(ax,ay);float length=(to-at).magnitude;if(length<=2*isoWidth&&length>0)for(float d=0;d<length;d+=3){var p=at+(to-at)*(d/length);Fill(new Rect(p.x-2.5f,p.y-2.5f,5,5),new Color(.1f,.07f,.02f,.7f));Fill(new Rect(p.x-1.5f,p.y-1.5f,3,3),gold);}at=to;
                if(!t.road){var old=GUI.color;GUI.color=new Color(1f,.82f,.3f,.8f);GUI.DrawTexture(TileRect(ax,ay,s),SelectionPicture());GUI.color=old;}}
            foreach(var bend in roadBends){RouteAnchor(bend.x,bend.y,out ax,out ay);Fill(new Rect(ax-6,ay-6,12,12),new Color(.1f,.07f,.02f,.9f));Fill(new Rect(ax-4,ay-4,8,8),gold);}
            int fresh=world.NewRoadTiles(roadPlotted),team=Math.Max(1,world.WorkerTeam(u).Count),turns=Math.Max(1,(fresh*3+team-1)/team+roadPlotted.Count/2),upkeep=(world.RoadTiles(u.owner)+fresh+World.RoadTilesPerGold-1)/World.RoadTilesPerGold-world.RoadUpkeep(u.owner);
            var panel=new Rect(mouse.x+18,mouse.y+14,280,70);if(panel.xMax>MapW-4)panel.x=mouse.x-298;if(panel.yMax>MapH-4)panel.y=mouse.y-84;DrawGuideFrame(panel);
            GUI.Label(new Rect(panel.x+8,panel.y+5,panel.width-16,20),roadPlotted.Count==0?"No way there":fresh+" new road tiles  •  about "+turns+" turns",hudLabel);
            var plotNote=new GUIStyle(hudSmall){wordWrap=false,clipping=TextClipping.Overflow};GUI.Label(new Rect(panel.x+8,panel.y+26,panel.width-16,18),"Upkeep +"+upkeep+" gold a turn",plotNote);GUI.Label(new Rect(panel.x+8,panel.y+45,panel.width-16,18),"Click: build  •  Shift-click: bend  •  Right-click: cancel",new GUIStyle(plotNote){fontSize=11});
        }
        // A faint dotted line along the way a selected unit's order will go (owner request, 6 Nov 2026). The route is only worked
        // out again when something it depends on changes.
        List<Tile> routeTiles=new List<Tile>();long routeKey=-1;
        void DrawRouteLine(Unit u,float s)
        {
            if(!u.hasOrder&&!u.roadOrder)return;
            int dx=u.roadOrder?u.roadX:u.destinationX,dy=u.roadOrder?u.roadY:u.destinationY;
            unchecked{long key=world.turn;key=key*31+u.id;key=key*31+u.x;key=key*31+u.y;key=key*31+dx;key=key*31+dy;key=key*31+u.moves;key=key*31+world.units.Count;if(key!=routeKey){routeKey=key;routeTiles=world.PlannedRoute(u);}}
            float wide=(world.Hex?2:4)*(diamondView?isoWidth:tilePixels),step=Mathf.Max(3,4*s);
            // A soft dark edge first, then the pale line over it, so it stays readable over forest and snow alike.
            for(int pass=0;pass<2;pass++)
            {
                float half=pass==0?3:1.5f,ax,ay,gap=0;var ink=pass==0?new Color(.05f,.04f,.02f,.22f):new Color(1f,.94f,.72f,.5f);
                RouteAnchor(u.x,u.y,out ax,out ay);var at=new Vector2(ax,ay);
                foreach(var t in routeTiles)
                {
                    RouteAnchor(t.x,t.y,out ax,out ay);var to=new Vector2(ax,ay);float length=(to-at).magnitude;
                    // Each step is one tile; a longer jump is the seam of a round world, so the line picks up again on the far side.
                    if(length<=wide)for(float d=gap;d<length;d+=step){var p=at+(to-at)*(d/length);if(diamondView&&(p.x<-4||p.y<-4||p.x>MapW+4||p.y>MapH+4))continue;Fill(new Rect(p.x-half,p.y-half,half*2,half*2),ink);}
                    gap=length<=wide?Mathf.Repeat(gap-length,step):0;at=to;
                }
            }
        }
        void RouteAnchor(int x,int y,out float sx,out float sy)
        {
            if(diamondView){IsoScreen(x,y,out sx,out sy);return;}
            var r=ClassicRect(x,y);sx=r.center.x;sy=r.center.y;
        }
        // Selection outline, destination marker and stack labels go on top so scenery never hides them.
        void DrawIsoOverlay(float s)
        {
            int width=world.Width;float sx,sy;
            var chosen=Selected();
            if(chosen!=null)
            {
                IsoScreen(chosen.x,chosen.y,out sx,out sy);sy-=Lift(chosen.x,chosen.y,s);
                GUI.DrawTexture(TileRect(sx,sy,s),SelectionPicture());
                DrawRouteLine(chosen,s);if(roadMode)DrawRoadPlot(chosen,s);
                if(chosen.hasOrder)
                {
                    IsoScreen(chosen.destinationX,chosen.destinationY,out sx,out sy);
                    sy-=Lift(chosen.destinationX,chosen.destinationY,s);var marker=new Rect(sx-10,sy-10,20,20);Fill(marker,new Color(1,.8f,0,.55f));GUI.Label(marker,"X",small);
                }
            }
            else if(cityId>=0)
            {
                var city=world.cities.Find(a=>a.id==cityId);
                if(city!=null){IsoScreen(city.x,city.y,out sx,out sy);sy-=Lift(city.x,city.y,s);GUI.DrawTexture(TileRect(sx,sy,s),SelectionPicture());}
            }
        }
    }
}
