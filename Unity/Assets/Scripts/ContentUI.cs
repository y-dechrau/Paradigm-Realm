using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm
{
    public partial class RealmGame
    {
        readonly HashSet<Texture2D> loadedTerrain=new HashSet<Texture2D>();
        int inspectedTech=-1;
        Vector2 techDetailScroll;
        int[] techDepth,techRow;
        void LoadTerrainArt(){
            for(int i=0;i<terrain.Length;i++){
                var asset=Resources.Load<Texture2D>("Terrain/terrain-"+i.ToString("00"));
                if(asset!=null){if(terrain[i]!=null)Destroy(terrain[i]);terrain[i]=asset;asset.filterMode=FilterMode.Point;loadedTerrain.Add(asset);}
                else if(terrain[i]==null){var fallback=Texture(new Color(.3f,.4f,.3f));fallback.Apply();terrain[i]=fallback;}
            }
        }
        // One era at a time (owner request, 31 Oct): columns follow prerequisites inside the era; technologies whose
        // prerequisites lie in earlier eras start the first column. Laid out again when the shown era changes.
        int researchEra=-1,techLayoutEra=-2;float techZoom=1f;
        int TechDepth(int id){if(techDepth[id]>=0)return techDepth[id];var inside=World.Requirements[id].Where(p=>TechnologyCatalog.Eras[p]==TechnologyCatalog.Eras[id]).ToArray();return techDepth[id]=inside.Length==0?0:inside.Max(TechDepth)+1;}
        void LayoutTechnologyTree(){
            if(researchEra<0)researchEra=world.Era(0);if(techDepth!=null&&techLayoutEra==researchEra)return;techLayoutEra=researchEra;
            techDepth=Enumerable.Repeat(-1,TechnologyCatalog.Count).ToArray();techRow=new int[TechnologyCatalog.Count];
            var rows=new Dictionary<int,int>();foreach(int i in EraTechs(researchEra).OrderBy(i=>TechnologyCatalog.IsKeystone(i)?0:1).ThenBy(i=>i)){int depth=TechDepth(i);if(!rows.ContainsKey(depth))rows[depth]=0;techRow[i]=rows[depth]++;}
        }
        IEnumerable<int> EraTechs(int era){return Enumerable.Range(0,TechnologyCatalog.Count).Where(i=>TechnologyCatalog.Active[i]&&TechnologyCatalog.Eras[i]==era);}
        // An era can be opened once it is reached, or once a known technology leads into it.
        bool EraViewable(int era){return era<=world.Era(0)||EraTechs(era).Any(t=>World.Requirements[t].Any(p=>world.tech[p]));}
        void FocusTechnology(int i){if(!EraViewable(TechnologyCatalog.Eras[i]))return;researchEra=TechnologyCatalog.Eras[i];techUnlocksView=false;ChronicleLayout();inspectedTech=i;var r=ExpandedTechRect(i);researchScroll=new Vector2(Math.Max(0,r.x*techZoom-20),Math.Max(0,r.y*techZoom-20));techDetailScroll=Vector2.zero;}
        void DrawTechnologyGlyph(Rect r,int id){
            // The owner's painted icon for the technology (owner request, 13 Nov 2026: Resources/UI/Tech/tech-<id>, 128 x 128,
            // cut from the six age sheets); the coded glyph below is kept for any technology without one.
            if(id>=0&&id<TechnologyCatalog.Count){var art=UiTex("Tech/tech-"+TechnologyCatalog.Ids[id]);if(art!=null){GUI.DrawTexture(r,art,ScaleMode.ScaleToFit,true);return;}}
            // An era-coloured glyph with a unique seven-bit centre pattern for each technology.
            Fill(r,new Color(.10f,.18f,.22f));float cell=r.width/8;int bits=id+1;
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)if(x==0||x==7||y==0||y==7||(y==3&&(bits&(1<<(x-1)))!=0)||(y==4&&x==2&&(bits&64)!=0)||(y==5&&x==1+TechnologyCatalog.Eras[id]%6))Fill(new Rect(r.x+x*cell,r.y+y*cell,cell,cell),new Color(.85f,.55f+TechnologyCatalog.Eras[id]*.02f,.35f));
        }
        // The research screen itself is TechTreeScreen.cs ("The Chronicle of Discovery", owner mockup 13 Nov 2026).
    }
}
