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
        int TechDepth(int id){if(techDepth[id]>=0)return techDepth[id];return techDepth[id]=World.Requirements[id].Length==0?0:World.Requirements[id].Max(TechDepth)+1;}
        void LayoutTechnologyTree(){
            if(techDepth!=null)return;techDepth=Enumerable.Repeat(-1,TechnologyCatalog.Count).ToArray();techRow=new int[TechnologyCatalog.Count];
            for(int i=0;i<techDepth.Length;i++)TechDepth(i);
            var rows=new Dictionary<int,int>();foreach(int i in Enumerable.Range(0,techDepth.Length).OrderBy(i=>TechnologyCatalog.Eras[i]).ThenBy(i=>i)){
                int depth=techDepth[i];if(!rows.ContainsKey(depth))rows[depth]=0;techRow[i]=rows[depth]++;
            }
        }
        Rect ExpandedTechRect(int i){return new Rect(20+techDepth[i]*252,20+techRow[i]*230,220,200);}
        void FocusTechnology(int i){inspectedTech=i;var r=ExpandedTechRect(i);researchScroll=new Vector2(Math.Max(0,r.x-20),Math.Max(0,r.y-20));techDetailScroll=Vector2.zero;}
        void DrawTechnologyGlyph(Rect r,int id){
            // An era-coloured glyph with a unique seven-bit centre pattern for each technology.
            Fill(r,new Color(.10f,.18f,.22f));float cell=r.width/8;int bits=id+1;
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)if(x==0||x==7||y==0||y==7||(y==3&&(bits&(1<<(x-1)))!=0)||(y==4&&x==2&&(bits&64)!=0)||(y==5&&x==1+TechnologyCatalog.Eras[id]%6))Fill(new Rect(r.x+x*cell,r.y+y*cell,cell,cell),new Color(.85f,.55f+TechnologyCatalog.Eras[id]*.02f,.35f));
        }
        void DrawExpandedResearch(){
            LayoutTechnologyTree();if(inspectedTech<0)FocusTechnology(world.research>=0?world.research:0);
            GUI.Box(new Rect(24,100,1232,630),"");GUI.Label(new Rect(42,110,960,32),"Research — "+(world.research<0?"Complete":World.TechNames[world.research]+" ("+world.science+" / "+world.ResearchCost(world.research)+")"),heading);
            if(GUI.Button(new Rect(1080,110,155,30),"Back",button)){researchScreen=false;GUIUtility.ExitGUI();}
            for(int e=0;e<TechnologyCatalog.EraNames.Length;e++){
                int era=e;if(GUI.Button(new Rect(42+e%7*171,150+e/7*29,166,26),TechnologyCatalog.EraNames[e].Replace(" Age",""),button)){FocusTechnology(Enumerable.Range(0,TechnologyCatalog.Count).Where(i=>TechnologyCatalog.Eras[i]==era).OrderBy(i=>techDepth[i]).First());}
            }
            GUI.Label(new Rect(42,211,1165,26),TechnologyCatalog.Count+" technologies in "+TechnologyCatalog.EraCount+" eras • You are in the "+TechnologyCatalog.EraNames[world.Era(0)]+" • Knowing "+World.EraThreshold+" of an era's "+TechnologyCatalog.PerEra+" completes it and offers an edict • Every prerequisite is required.",small);
            researchScroll=GUI.BeginScrollView(new Rect(40,243,790,465),researchScroll,new Rect(0,0,(techDepth.Max()+1)*252+20,(techRow.Max()+1)*230+20));
            // Draw only selected incoming/outgoing links so the large graph stays readable.
            for(int i=0;i<TechnologyCatalog.Count;i++)foreach(int pre in World.Requirements[i])if(i==inspectedTech||pre==inspectedTech){
                Rect from=ExpandedTechRect(pre),to=ExpandedTechRect(i);float x=from.xMax+10,y=from.y+100,ty=to.y+100;
                var colour=new Color(.8f,.65f,.3f);Fill(new Rect(from.xMax,y,10,2),colour);Fill(new Rect(x,Math.Min(y,ty),2,Math.Abs(ty-y)),colour);Fill(new Rect(x,ty,to.x-x,2),colour);GUI.Label(new Rect(to.x-12,ty-9,12,20),">",body);
            }
            var available=world.AvailableTech();int perTurn=Math.Max(1,world.cities.Where(c=>c.owner==0).Sum(c=>world.Yield(c)[2]));
            for(int i=0;i<TechnologyCatalog.Count;i++){
                Rect r=ExpandedTechRect(i);Fill(r,world.tech[i]?new Color(.12f,.3f,.21f):inspectedTech==i?new Color(.25f,.29f,.4f):new Color(.10f,.15f,.18f));GUI.Box(r,"");DrawTechnologyGlyph(new Rect(r.x+8,r.y+10,24,24),i);
                GUI.Label(new Rect(r.x+40,r.y+8,173,44),World.TechNames[i],body);
                GUI.Label(new Rect(r.x+10,r.y+51,200,24),TechnologyCatalog.EraNames[TechnologyCatalog.Eras[i]],small);
                var benefits=World.ActiveTechBenefits(i);string summary=benefits.Length>0?string.Join("\n",benefits.Take(2).Select(v=>"• "+v).ToArray()):"• Advances the research tree\n• Content unlocks planned";
                GUI.Label(new Rect(r.x+10,r.y+78,200,53),summary,small);
                GUI.Label(new Rect(r.x+10,r.y+132,200,26),world.tech[i]?"Discovered":world.research==i?"Researching":available.Contains(i)?"Available • "+world.ResearchCost(i):"Locked • "+World.Requirements[i].Length+" prerequisites",small);
                // Research straight from the card, but only when the technology can be researched now.
                if(available.Contains(i)&&world.research!=i&&world.result==""){int left=Math.Max(0,world.ResearchCost(i)-world.nations[0].progress[i]);if(GUI.Button(new Rect(r.x+10,r.y+165,140,27),"Research • "+Math.Max(1,(left+perTurn-1)/perTurn)+" turns",button)){world.SelectResearch(i);inspectedTech=i;}if(GUI.Button(new Rect(r.x+154,r.y+165,56,27),"Info",button)){inspectedTech=i;techDetailScroll=Vector2.zero;}}
                else if(GUI.Button(new Rect(r.x+10,r.y+165,200,27),(world.research==i?"Researching — details":"Inspect "+World.TechNames[i]),button)){inspectedTech=i;techDetailScroll=Vector2.zero;}
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
