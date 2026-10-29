using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    // Diamond (three-quarter) map view. Map tile (x,y) sits at lattice point (i,j)=(x,y); on a sphere i wraps, so the
    // world scrolls east and west without an edge. Tiles are drawn back to front, each with its own scenery.
    public partial class RealmGame
    {
        bool diamondView=true;
        float camI=20,camJ=15;
        int isoWidth=64;
        static readonly int[] IsoWidths={32,48,64,96,128};
        const float IsoCentreX=426,IsoCentreY=312;
        readonly Dictionary<long,Texture2D> isoTiles=new Dictionary<long,Texture2D>();
        readonly Queue<long> isoTileOrder=new Queue<long>();
        readonly Dictionary<long,Texture2D> isoPictures=new Dictionary<long,Texture2D>();
        readonly Dictionary<int,Texture2D> isoShown=new Dictionary<int,Texture2D>();
        readonly List<int> isoLabelCells=new List<int>();
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
        // kind: 1 sprite, 2 farm, 3 road, 4 misc. Small shared pictures, never evicted.
        Texture2D IsoPicture(int kind,int a,int b)
        {
            long key=((long)kind<<40)|((long)a<<8)|(long)b;Texture2D tex;
            if(isoPictures.TryGetValue(key,out tex))return tex;
            PixelCanvas cv=null;
            if(kind==1)cv=PixelArt.Sprite(a,b);else if(kind==2)cv=PixelArt.Farm(a);else if(kind==3)cv=PixelArt.Road(a);else cv=PixelArt.Misc(a);
            tex=cv==null?null:PixelTexture(cv);isoPictures[key]=tex;return tex;
        }
        void ClearIso()
        {
            foreach(var t in isoTiles.Values)Destroy(t);foreach(var t in isoPictures.Values)if(t!=null)Destroy(t);
            isoTiles.Clear();isoTileOrder.Clear();isoPictures.Clear();isoShown.Clear();
        }
        static int IsoVariant(int x,int y){unchecked{int h=x*73856093^y*19349663;return (h&0x7fffffff)%3;}}

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
            if(!diamondView){mapScroll=new Vector2(Mathf.Max(0,x*tilePixels-350),Mathf.Max(0,y*tilePixels-250));return;}
            float i=x;if(world.Sphere){float w=world.Width;i=x+w*(float)Math.Round((camI-x)/w);}
            camI=i;camJ=y;ClampCamera();
        }
        void IsoPan(float dx,float dy)
        {
            // Screen movement of (dx,dy) pixels moves the camera over the lattice.
            float tw=isoWidth,th=tw/2,u=dx/(tw/2),v=dy/(th/2);
            camI+=(u+v)/2;camJ+=(v-u)/2;ClampCamera();
        }
        void IsoZoom(int direction)
        {
            int index=Array.IndexOf(IsoWidths,isoWidth);if(index<0)index=2;
            isoWidth=IsoWidths[Math.Max(0,Math.Min(IsoWidths.Length-1,index+direction))];
        }
        void IsoScreen(int x,int y,out float sx,out float sy)
        {
            float tw=isoWidth,th=tw/2,i=x;
            if(world.Sphere){float w=world.Width;i=x+w*(float)Math.Round((camI-x)/w);}
            sx=IsoCentreX+((i-camI)-(y-camJ))*tw/2;sy=IsoCentreY+((i-camI)+(y-camJ))*th/2;
        }
        // Which map tile is under a point given in map-view coordinates (0,0 = top left of the view).
        bool IsoPick(Vector2 local,out int x,out int y)
        {
            float tw=isoWidth,th=tw/2,s=tw/64f;
            for(int pass=0;pass<2;pass++)
            {
                // Land is drawn Elev pixels higher than water, so try the raised surface first.
                float dy=local.y-IsoCentreY+(pass==0?PixelArt.Elev*s:0),dx=local.x-IsoCentreX;
                float u=dx/(tw/2),v=dy/(th/2);
                int i=(int)Math.Round(camI+(u+v)/2),j=(int)Math.Round(camJ+(v-u)/2);
                var t=world.At(i,j);
                if(t==null)continue;
                if(pass==0&&t.seen&&PixelArt.IsWater(world.KnownTerrain(t)))continue;
                x=t.x;y=t.y;return true;
            }
            x=-1;y=-1;return false;
        }
        // Tile under the mouse in either view. Mouse position is in screen space.
        bool MapTileAt(Vector2 mouse,out int x,out int y)
        {
            if(diamondView)return IsoPick(mouse-new Vector2(24,100),out x,out y);
            x=Mathf.FloorToInt((mouse.x-24+mapScroll.x)/tilePixels);y=Mathf.FloorToInt((mouse.y-100+mapScroll.y)/tilePixels);
            return world.At(x,y)!=null;
        }

        // ---------- terrain pictures ----------
        int IsoKnownTerrain(int i,int j)
        {
            var n=world.At(i,j);
            return n==null||!n.seen?-1:world.KnownTerrain(n);
        }
        Texture2D IsoGround(Tile t,int i,int j,int frame)
        {
            int terrain=t.terrain;
            int[] offI={0,1,0,-1},offJ={-1,0,1,0};
            int mask=0;
            for(int k=0;k<4;k++)
            {
                int n=IsoKnownTerrain(i+offI[k],j+offJ[k]);
                isoNb[k]=PixelArt.NeighbourCode(terrain,k,n);
                if(terrain==7&&n>=0&&(n==7||PixelArt.IsWater(n)))mask|=1<<k;
            }
            int variant=IsoVariant(t.x,t.y);
            int fr=PixelArt.IsWater(terrain)?frame:0;
            long key=(long)terrain|((long)variant<<5)|((long)fr<<7)|((long)mask<<9)|((long)(isoNb[0]+1)<<13)|((long)(isoNb[1]+1)<<20)|((long)(isoNb[2]+1)<<27)|((long)(isoNb[3]+1)<<34);
            int index=t.y*world.Width+t.x;
            Texture2D tex;
            if(isoTiles.TryGetValue(key,out tex)){Texture2D cur;if(!isoShown.TryGetValue(index,out cur)||cur!=tex)isoShown[index]=tex;return tex;}
            if(compositeBudgetOn&&compositeClock.ElapsedMilliseconds>=CompositeBudgetMs)
            {
                Texture2D previous;
                if(isoShown.TryGetValue(index,out previous)&&previous!=null)return previous;
            }
            tex=PixelTexture(PixelArt.Compose(terrain,variant,isoNb,fr,mask));
            if(isoTiles.Count>=4000){long oldest=isoTileOrder.Dequeue();Destroy(isoTiles[oldest]);isoTiles.Remove(oldest);}
            isoTiles[key]=tex;isoTileOrder.Enqueue(key);isoShown[index]=tex;return tex;
        }

        // ---------- drawing ----------
        void DrawIsoMap()
        {
            if(terrainWorld!=world){ClearTerrainComposites();terrainWorld=world;}
            bool repaint=Event.current.type==EventType.Repaint;
            if(repaint)world.BeginVisibilityCache();
            GUI.Box(new Rect(18,94,865,637),"");
            GUI.BeginGroup(new Rect(24,100,852,624));
            if(repaint)DrawIsoContents();
            GUI.EndGroup();
            world.EndVisibilityCache();
            HandleMapInput();
            if(dragging){var m=Event.current.mousePosition;GUI.Label(new Rect(m.x+15,m.y+12,220,30),"Release to issue destination",body);}
        }
        static readonly int[] RoadDx={-1,0,1,1,1,0,-1,-1},RoadDy={-1,-1,-1,0,1,1,1,0};
        void DrawIsoContents()
        {
            compositeBudgetOn=true;compositeClock.Reset();compositeClock.Start();
            ClampCamera();
            float tw=isoWidth,th=tw/2,s=tw/64f;int width=world.Width;
            int frame=(int)(Time.realtimeSinceStartup*2.5f)%3;
            Fill(new Rect(0,0,852,624),new Color(.035f,.065f,.085f));
            viewStacks.Clear();
            foreach(var unit in world.units){List<Unit> stackAt;int cell=unit.y*width+unit.x;if(!viewStacks.TryGetValue(cell,out stackAt)){stackAt=new List<Unit>(2);viewStacks[cell]=stackAt;}stackAt.Add(unit);}
            viewCities.Clear();foreach(var city in world.cities)viewCities[city.y*width+city.x]=city;
            // Range of lattice points that can touch the view (extra room above for tall scenery, below for cliffs).
            float dxMax=IsoCentreX+tw,dyUp=IsoCentreY+64*s,dyDown=624-IsoCentreY+24*s;
            float[] cu={-dxMax/(tw/2),dxMax/(tw/2),-dxMax/(tw/2),dxMax/(tw/2)};
            float[] cv={-dyUp/(th/2),-dyUp/(th/2),dyDown/(th/2),dyDown/(th/2)};
            float minI=1e9f,maxI=-1e9f,minJ=1e9f,maxJ=-1e9f;
            for(int k=0;k<4;k++){float di=(cu[k]+cv[k])/2,dj=(cv[k]-cu[k])/2;minI=Math.Min(minI,di);maxI=Math.Max(maxI,di);minJ=Math.Min(minJ,dj);maxJ=Math.Max(maxJ,dj);}
            int iLo=(int)Math.Floor(camI+minI)-1,iHi=(int)Math.Ceiling(camI+maxI)+1,jLo=(int)Math.Floor(camJ+minJ)-1,jHi=(int)Math.Ceiling(camJ+maxJ)+1;
            if(!world.Sphere){iLo=Math.Max(iLo,0);iHi=Math.Min(iHi,width-1);}
            jLo=Math.Max(jLo,0);jHi=Math.Min(jHi,world.Height-1);
            int pooled=0;isoLabelCells.Clear();isoResTiles.Clear();isoResPos.Clear();
            for(int diag=iLo+jLo;diag<=iHi+jHi;diag++)for(int j=jLo;j<=jHi;j++)
            {
                int i=diag-j;if(i<iLo||i>iHi)continue;
                float sx=IsoCentreX+((i-camI)-(j-camJ))*tw/2,sy=IsoCentreY+((i-camI)+(j-camJ))*th/2;
                if(sx<-tw||sx>852+tw||sy<-64*s||sy>624+24*s)continue;
                var source=world.At(i,j);if(source==null)continue;
                bool visible=world.Visible(source.x,source.y);Tile t=source;
                if(source.seen&&!visible){if(pooled==knownPool.Count)knownPool.Add(new Tile());t=knownPool[pooled++];world.CopyKnown(source,t);}
                if(!source.seen){GUI.DrawTexture(new Rect(sx-32*s,sy-16*s,64*s,32*s),IsoPicture(4,5,0));continue;}
                bool water=PixelArt.IsWater(t.terrain);float lift=water?0:PixelArt.Elev*s,gy=sy-lift;
                var old=GUI.color;if(!visible)GUI.color=new Color(.55f,.58f,.68f,1);
                GUI.DrawTexture(new Rect(sx-32*s,sy-(16+PixelArt.Elev)*s,64*s,PixelArt.CH*s),IsoGround(t,i,j,frame));
                var ground=new Rect(sx-32*s,gy-16*s,64*s,32*s);
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
                    GUI.color=visible?old:new Color(.55f,.58f,.68f,1);
                }
                var sprite=water?null:IsoPicture(1,t.terrain,IsoVariant(t.x,t.y));
                if(sprite!=null)GUI.DrawTexture(new Rect(sx-PixelArt.AX*s,gy-PixelArt.AY*s,64*s,64*s),sprite);
                var improvement=new Rect(sx-18*s,gy-26*s,36*s,36*s);
                if(t.mine)DrawArt("mine",improvement);if(t.fort)DrawArt("fort",improvement);
                if(t.village&&visible)DrawArt("village",improvement);
                if(!visible&&!string.IsNullOrEmpty(t.rememberedCity))GUI.DrawTexture(new Rect(sx-24*s,gy-40*s,48*s,48*s),cityTexture,ScaleMode.ScaleToFit,true);
                GUI.color=old;
                int cell=t.y*width+t.x;City c;viewCities.TryGetValue(cell,out c);if(c!=null&&!(c.owner==0||visible))c=null;
                if(t.resource>0){isoResTiles.Add(t);isoResPos.Add(new Vector2(sx,gy));}
                if(c!=null){DrawCityArt(c,new Rect(sx-32*s,gy-50*s,64*s,64*s));Fill(new Rect(sx-10*s,gy+14*s,20*s,3),nations[c.owner]);}
                Unit first=null,picked=null;int count=0;List<Unit> stackAt;
                if(viewStacks.TryGetValue(cell,out stackAt))foreach(var a in stackAt){if(!(a.owner==0||visible))continue;count++;if(first==null)first=a;if(a.id==selected)picked=a;}
                var shown=picked??first;
                if(shown!=null){DrawUnitArt(shown,new Rect(sx-20*s,gy-26*s,40*s,40*s),count);isoLabelCells.Add(cell);}
            }
            // Resource badges go on after every tile so the tiles in front can never cover them.
            for(int n=0;n<isoResTiles.Count;n++){var p=isoResPos[n];DrawResource(isoResTiles[n],new Rect(p.x-36*s,p.y+15*s-61*s,61*s,61*s));}
            DrawIsoOverlay(s);
            compositeBudgetOn=false;
        }
        // Selection outline, destination marker and stack labels go on top so scenery never hides them.
        void DrawIsoOverlay(float s)
        {
            int width=world.Width;float sx,sy;
            var chosen=Selected();
            if(chosen!=null)
            {
                IsoScreen(chosen.x,chosen.y,out sx,out sy);var tile=world.At(chosen.x,chosen.y);
                float gy=sy-(tile!=null&&PixelArt.IsWater(tile.terrain)?0:PixelArt.Elev*s);
                GUI.DrawTexture(new Rect(sx-32*s,gy-16*s,64*s,32*s),IsoPicture(4,4,0));
                if(chosen.hasOrder)
                {
                    IsoScreen(chosen.destinationX,chosen.destinationY,out sx,out sy);
                    var marker=new Rect(sx-10,sy-10-PixelArt.Elev*s,20,20);Fill(marker,new Color(1,.8f,0,.55f));GUI.Label(marker,"X",small);
                }
            }
            else if(cityId>=0)
            {
                var city=world.cities.Find(a=>a.id==cityId);
                if(city!=null){IsoScreen(city.x,city.y,out sx,out sy);GUI.DrawTexture(new Rect(sx-32*s,sy-(16+PixelArt.Elev)*s,64*s,32*s),IsoPicture(4,4,0));}
            }
            if(tileLabel==null)tileLabel=new GUIStyle(small){fontSize=10,alignment=TextAnchor.MiddleCenter,wordWrap=false};
            foreach(int cell in isoLabelCells)
            {
                List<Unit> stack;if(!viewStacks.TryGetValue(cell,out stack))continue;
                var visibleStack=stack.Where(u=>u.owner==0||world.Visible(u.x,u.y)).ToList();
                string label=World.StackLabel(visibleStack);if(label=="")continue;
                var unit=visibleStack[0];IsoScreen(unit.x,unit.y,out sx,out sy);
                float width2=Mathf.Max(48,label.Length*6+6);var r=new Rect(sx-width2/2,sy+6*s,width2,14);
                Fill(r,new Color(.025f,.045f,.055f,.92f));GUI.Label(r,label,tileLabel);
            }
        }
    }
}
