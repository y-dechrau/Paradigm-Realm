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
        IEnumerable<int> EraTechs(int era){return Enumerable.Range(0,TechnologyCatalog.Count).Where(i=>TechnologyCatalog.Eras[i]==era);}
        // An era can be opened once it is reached, or once a known technology leads into it.
        bool EraViewable(int era){return era<=world.Era(0)||EraTechs(era).Any(t=>World.Requirements[t].Any(p=>world.tech[p]));}
        Rect ExpandedTechRect(int i){return new Rect(20+techDepth[i]*252,20+techRow[i]*230,220,200);}
        void FocusTechnology(int i){if(!EraViewable(TechnologyCatalog.Eras[i]))return;researchEra=TechnologyCatalog.Eras[i];LayoutTechnologyTree();inspectedTech=i;var r=ExpandedTechRect(i);researchScroll=new Vector2(Math.Max(0,r.x*techZoom-20),Math.Max(0,r.y*techZoom-20));techDetailScroll=Vector2.zero;}
        void DrawTechnologyGlyph(Rect r,int id){
            // An era-coloured glyph with a unique seven-bit centre pattern for each technology.
            Fill(r,new Color(.10f,.18f,.22f));float cell=r.width/8;int bits=id+1;
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)if(x==0||x==7||y==0||y==7||(y==3&&(bits&(1<<(x-1)))!=0)||(y==4&&x==2&&(bits&64)!=0)||(y==5&&x==1+TechnologyCatalog.Eras[id]%6))Fill(new Rect(r.x+x*cell,r.y+y*cell,cell,cell),new Color(.85f,.55f+TechnologyCatalog.Eras[id]*.02f,.35f));
        }
        void DrawExpandedResearch(){
            LayoutTechnologyTree();if(inspectedTech<0||TechnologyCatalog.Eras[inspectedTech]!=researchEra){inspectedTech=world.research>=0&&TechnologyCatalog.Eras[world.research]==researchEra?world.research:EraTechs(researchEra).First();}
            GUI.Box(new Rect(24,100,1232,630),"");GUI.Label(new Rect(42,110,960,32),"Research — "+(world.research<0?"Complete":World.TechNames[world.research]+" ("+world.science+" / "+world.ResearchCost(world.research)+")"),heading);
            if(GUI.Button(new Rect(1080,110,155,30),"Back",button)){researchScreen=false;GUIUtility.ExitGUI();}
            // Era tabs: one era is shown at a time; a later era opens once a known technology leads into it.
            for(int e=0;e<TechnologyCatalog.EraNames.Length;e++){
                int era=e;bool open=EraViewable(era);var tab=new Rect(42+e%7*171,150+e/7*29,166,26);bool was=GUI.enabled;GUI.enabled=was&&open;
                if(GUI.Button(tab,(era==researchEra?"▸ ":"")+TechnologyCatalog.EraNames[e].Replace(" Age","")+(open?"":" (locked)"),button)&&era!=researchEra){researchEra=era;LayoutTechnologyTree();inspectedTech=-1;researchScroll=Vector2.zero;GUI.enabled=was;GUIUtility.ExitGUI();}
                GUI.enabled=was;if(era==researchEra)Fill(new Rect(tab.x,tab.yMax-2,tab.width,2),new Color(.86f,.7f,.39f));
            }
            var header=new GUIStyle(small){clipping=TextClipping.Clip,wordWrap=false};
            GUI.Label(new Rect(42,211,1190,26),TechnologyCatalog.Count+" technologies, "+(TechnologyCatalog.Count/6)+" per era • You are in the "+TechnologyCatalog.EraNames[world.Era(0)]+" • "+World.EraThreshold+" of an era's "+TechnologyCatalog.PerEra+" keystones complete it • Ctrl + wheel zooms, hold the wheel to pan",header);
            // Hold the mouse wheel (middle button) and drag to pan around the tree.
            var shown=EraTechs(researchEra).ToList();var treeView=new Rect(40,243,790,465);
            // Hold Ctrl and turn the wheel to zoom the tree around the pointer (owner request). The cards are drawn at the zoomed
            // size rather than through GUI.matrix, because Unity clips scaled content inside a scroll view in the wrong place.
            var wheel=Event.current;if(wheel.type==EventType.ScrollWheel&&wheel.control&&treeView.Contains(wheel.mousePosition)){float was=techZoom;techZoom=Mathf.Clamp(techZoom*(wheel.delta.y<0?1.12f:1/1.12f),.35f,1.6f);var at=new Vector2(wheel.mousePosition.x-treeView.x,wheel.mousePosition.y-treeView.y);researchScroll=new Vector2(Math.Max(0,(researchScroll.x+at.x)*techZoom/was-at.x),Math.Max(0,(researchScroll.y+at.y)*techZoom/was-at.y));wheel.Use();}
            float z=techZoom;Rect Z(Rect q)=>new Rect(q.x*z,q.y*z,q.width*z,q.height*z);
            GUIStyle Zoomed(GUIStyle o,int normal){return new GUIStyle(o){fontSize=Math.Max(6,(int)Math.Round((o.fontSize>0?o.fontSize:normal)*z)),clipping=TextClipping.Clip};}
            var zBody=Zoomed(body,15);var zSmall=Zoomed(small,12);var zButton=Zoomed(button,13);
            var treeSize=new Vector2(((shown.Max(i=>techDepth[i])+1)*252+20)*techZoom,((shown.Max(i=>techRow[i])+1)*230+20)*techZoom);var pan=Event.current;
            if(pan.button==2&&treeView.Contains(pan.mousePosition)&&(pan.type==EventType.MouseDown||pan.type==EventType.MouseDrag)){
                if(pan.type==EventType.MouseDrag)researchScroll=new Vector2(Mathf.Clamp(researchScroll.x-pan.delta.x,0,Mathf.Max(0,treeSize.x-treeView.width+16)),Mathf.Clamp(researchScroll.y-pan.delta.y,0,Mathf.Max(0,treeSize.y-treeView.height+16)));
                pan.Use();}
            researchScroll=GUI.BeginScrollView(treeView,researchScroll,new Rect(0,0,treeSize.x,treeSize.y));
            // Draw only selected incoming/outgoing links so the large graph stays readable.
            foreach(int i in shown)foreach(int pre in World.Requirements[i])if(TechnologyCatalog.Eras[pre]==researchEra&&(i==inspectedTech||pre==inspectedTech)){
                Rect from=ExpandedTechRect(pre),to=ExpandedTechRect(i);float x=from.xMax+10,y=from.y+100,ty=to.y+100;
                var colour=new Color(.8f,.65f,.3f);Fill(Z(new Rect(from.xMax,y,10,2)),colour);Fill(Z(new Rect(x,Math.Min(y,ty),2,Math.Abs(ty-y))),colour);Fill(Z(new Rect(x,ty,to.x-x,2)),colour);GUI.Label(Z(new Rect(to.x-12,ty-9,12,20)),">",zBody);
            }
            var available=world.AvailableTech();int perTurn=Math.Max(1,world.cities.Where(c=>c.owner==0).Sum(c=>world.Yield(c)[2]));
            foreach(int i in shown){
                Rect r=ExpandedTechRect(i);Fill(Z(r),world.tech[i]?new Color(.12f,.3f,.21f):inspectedTech==i?new Color(.25f,.29f,.4f):new Color(.10f,.15f,.18f));GUI.Box(Z(r),"");DrawTechnologyGlyph(Z(new Rect(r.x+8,r.y+10,24,24)),i);
                GUI.Label(Z(new Rect(r.x+40,r.y+8,173,44)),World.TechNames[i],zBody);
                GUI.Label(Z(new Rect(r.x+10,r.y+51,200,24)),TechnologyCatalog.EraNames[TechnologyCatalog.Eras[i]],zSmall);
                var benefits=World.ActiveTechBenefits(i);string summary=benefits.Length>0?string.Join("\n",benefits.Take(2).Select(v=>"• "+v).ToArray()):"• Advances the research tree\n• Content unlocks planned";
                GUI.Label(Z(new Rect(r.x+10,r.y+78,200,53)),summary,zSmall);
                GUI.Label(Z(new Rect(r.x+10,r.y+132,200,26)),world.tech[i]?"Discovered":world.research==i?"Researching":available.Contains(i)?"Available • "+world.ResearchCost(i):"Locked • "+World.Requirements[i].Length+" prerequisites",zSmall);
                // Research straight from the card, but only when the technology can be researched now.
                if(available.Contains(i)&&world.research!=i&&world.result==""){int left=Math.Max(0,world.ResearchCost(i)-world.nations[0].progress[i]);if(GUI.Button(Z(new Rect(r.x+10,r.y+165,140,27)),"Research • "+Math.Max(1,(left+perTurn-1)/perTurn)+" turns",zButton)){world.SelectResearch(i);inspectedTech=i;}if(GUI.Button(Z(new Rect(r.x+154,r.y+165,56,27)),"Info",zButton)){inspectedTech=i;techDetailScroll=Vector2.zero;}}
                else if(GUI.Button(Z(new Rect(r.x+10,r.y+165,200,27)),(world.research==i?"Researching — details":"Inspect "+World.TechNames[i]),zButton)){inspectedTech=i;techDetailScroll=Vector2.zero;}
            }
            GUI.EndScrollView();DrawTechnologyDetails(available);
        }
        void DrawTechnologyDetails(List<int> available){
            int i=inspectedTech;GUI.Box(new Rect(845,243,390,465),"");GUILayout.BeginArea(new Rect(858,252,363,444));techDetailScroll=GUILayout.BeginScrollView(techDetailScroll);
            // Edicts come first: a completed era waits here until one of its three is chosen.
            int pending=world.PendingEdict(0);
            if(pending>=0){
                GUILayout.Label(TechnologyCatalog.EraNames[pending]+" complete — choose an edict",heading);GUILayout.Label("Permanent; one of three.",small);
                for(int k=0;k<3;k++)if(Button(World.EdictNames[pending][k]+": "+World.EdictTexts[pending][k])){world.ChooseEdict(0,pending,k);GUIUtility.ExitGUI();}
                GUILayout.Space(8);
            }
            var adopted=Enumerable.Range(0,TechnologyCatalog.EraCount).Where(e=>world.nations[0].edicts!=null&&world.nations[0].edicts[e]>=0).Select(e=>World.EdictNames[e][world.nations[0].edicts[e]]).ToArray();
            if(adopted.Length>0)GUILayout.Label("Edicts: "+string.Join(", ",adopted),small);
            GUILayout.Label(World.TechNames[i],heading);GUILayout.Label(TechnologyCatalog.EraNames[TechnologyCatalog.Eras[i]]+" • "+world.ResearchCost(i)+" science"+(world.ResearchCost(i)<world.ResearchCost(i,-1)?" (a fifth off: two civilisations you know have it)":""),small);
            GUILayout.Label(TechnologyCatalog.Descriptions[i],body);
            // The research button appears only when the technology can be researched now.
            if(world.research==i)GUILayout.Label("Currently researching.",heading);
            else if(available.Contains(i)&&world.result==""){if(Button("Research this technology"))world.SelectResearch(i);}
            else if(!world.tech[i])GUILayout.Label("Not yet available: research its prerequisites first.",small);
            GUILayout.Label("Active game effects",heading);var benefits=World.ActiveTechBenefits(i);
            foreach(string benefit in benefits)GUILayout.Label("• "+benefit,body);
            if(benefits.Length==0)GUILayout.Label("Opens the next research steps. Its listed content is planned.",body);
            GUILayout.Label("Requires all",heading);
            if(World.Requirements[i].Length==0)GUILayout.Label("No prerequisites",body);
            foreach(int pre in World.Requirements[i])if(Button((world.tech[pre]?"✓ ":"→ ")+World.TechNames[pre]))FocusTechnology(pre);
            GUILayout.Label("Leads to",heading);
            foreach(int next in Enumerable.Range(0,TechnologyCatalog.Count).Where(t=>World.Requirements[t].Contains(i)))if(Button(World.TechNames[next]))FocusTechnology(next);
            GUILayout.Label("Original design unlocks",heading);GUILayout.Label("This preserves the attached design. Only effects listed under Active game effects above are implemented; other content remains planned.",small);
            foreach(string unlock in TechnologyCatalog.Unlocks[i])GUILayout.Label("• "+unlock.Substring(2),body);
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
    }
}
