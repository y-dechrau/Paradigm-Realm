using System;
using System.Linq;
namespace ParadigmRealm
{
    public partial class World
    {
        public bool HasTech(int owner,string id){int i=TechnologyCatalog.Index(id);return i>=0&&nations[owner].tech.Length>i&&nations[owner].tech[i];}
        public static bool IsImpassable(int t){return t==4||t==5||t==6||t==12||t==14||t==15;}
        public static bool Wooded(int t){return t==1||t==11||t==13||t==19;}
        public static bool RoughTerrain(int t){return Wooded(t)||t==2||t==7||t==9||t==17||t==18;}
        public static bool Farmable(int t){return t==0||t==1||t==2||t==7||t==8||t==11||t==16||t==20;}
        // Climate bands and broad seeded patches; starting clearings and land links are restored afterwards.
        void GenerateBiomes(){
            var random=new Random(seed^17071);
            foreach(var t in tiles){
                double latitude=Math.Abs(t.y-(Height-1)/2.0)/(Height/2.0);
                double patch=Math.Sin((t.x+seed%31)*.39)+Math.Cos((t.y+seed%17)*.55);
                if(t.terrain==4){if(latitude>.83)t.terrain=14;else if(latitude<.42&&patch>.6)t.terrain=12;continue;}
                if(t.terrain==5){if(latitude>.65)t.terrain=15;continue;}
                if(latitude>.75){t.terrain=t.terrain==1?13:8;continue;}
                if(t.terrain==1&&latitude<.38)t.terrain=11;
                else if(t.terrain==0&&patch>1.1)t.terrain=9;
                else if(t.terrain==0&&latitude<.4&&patch<-.5)t.terrain=16;
                else if(t.terrain==0&&latitude>.45&&patch<-.5)t.terrain=20;
                else if(t.terrain==3&&patch<-.4)t.terrain=10;
                else if(t.terrain==2&&patch>1.4)t.terrain=17;
            }
            // Coast and mangrove tiles are traversable shoreline land; reefs remain water.
            foreach(var t in tiles.Where(t=>!IsImpassable(t.terrain)).ToList()){
                bool coast=new[]{At(t.x-1,t.y),At(t.x+1,t.y),At(t.x,t.y-1),At(t.x,t.y+1)}.Any(a=>a!=null&&(a.terrain==4||a.terrain==12));
                if(coast)t.terrain=t.terrain==11?19:18;
            }
            for(int i=0;i<4;i++){
                int x=random.Next(5,Width-5),y=random.Next(5,Height-5);var lake=At(x,y);
                if(!IsImpassable(lake.terrain))lake.terrain=6;
            }
            // The supplied river artwork runs north/south, so short connected streams follow that axis.
            for(int i=0;i<3;i++){
                int x=5+i*14+random.Next(4),start=random.Next(6,12);
                for(int y=start;y<Math.Min(Height-4,start+9);y++)if(!IsImpassable(At(x,y).terrain))At(x,y).terrain=7;
            }
        }
        void ApplyResearchYields(City c,ref int food,ref int production,ref int science){
            int owner=c.owner;
            if(HasTech(owner,"alphabet")&&c.library)science++;
            int sciencePercent=(HasTech(owner,"printing")?10:0)+(HasTech(owner,"computers")?10:0)+(HasTech(owner,"quantum")?20:0);
            science=science*(100+sciencePercent)/100;
            if(HasTech(owner,"nanotech"))production=production*115/100;
        }
        void GrantAncestors(bool[] values,int id){foreach(int pre in Requirements[id]){if(values[pre])continue;values[pre]=true;GrantAncestors(values,pre);}}
        void MigrateTechnologySave(){
            if(nations==null||nations.Length!=3||nations.Any(n=>n==null)||tech==null)return;
            for(int owner=0;owner<3;owner++){
                var n=nations[owner];if(owner==0)n.research=research;var old=owner==0?tech:n.tech;var progress=n.progress;
                if(old==null||progress==null)return;
                var upgraded=new bool[TechnologyCatalog.Count];var upgradedProgress=new int[TechnologyCatalog.Count];
                Array.Copy(old,upgraded,Math.Min(12,old.Length));Array.Copy(progress,upgradedProgress,Math.Min(12,progress.Length));
                if(owner==0&&research>=0&&research<12)upgradedProgress[research]=science;
                // Existing unlocks remain usable, while all newly introduced research starts undiscovered.
                if(old.Length>3&&old[3])upgraded[TechnologyCatalog.Index("chivalry")]=true;
                if(old.Length>4&&old[4])upgraded[TechnologyCatalog.Index("rifling")]=true;
                if(old.Length>7&&old[7])upgraded[TechnologyCatalog.Index("horseback")]=true;
                if(old.Length>0&&old[0])upgraded[TechnologyCatalog.Index("pottery")]=true;
                upgraded[TechnologyCatalog.Index("wheel")]=true; // roads were unrestricted in earlier saves
                for(int i=0;i<upgraded.Length;i++)if(upgraded[i])GrantAncestors(upgraded,i);
                n.tech=upgraded;n.progress=upgradedProgress;
                if(n.research<0||n.research>=12||n.tech[n.research]||!Requirements[n.research].All(j=>n.tech[j]))n.research=AvailableTech(owner).OrderBy(t=>ResearchCost(t)).DefaultIfEmpty(-1).First();
            }
            tech=nations[0].tech;research=nations[0].research;science=research<0?0:nations[0].progress[research];version=5;
            Note("Save upgraded: full technology tree. Existing technology prerequisites and road access preserved.");
        }
        // Only active game effects are advertised as playable. Source unlocks are preserved separately.
        public static string[] ActiveTechBenefits(int id){
            var benefits=new System.Collections.Generic.List<string>();
            for(int i=0;i<UnitNames.Length;i++)if(UnitTech[i]==id)benefits.Add("Train "+UnitNames[i]);
            for(int i=0;i<BuildingIds.Length;i++)if(BuildingTech[i]==id)benefits.Add("Build "+BuildingNames[i]);
            for(int g=1;g<Governments.Length;g++)if(TechnologyCatalog.Index(GovernmentTech[g])==id)benefits.Add("Government: "+Governments[g]);
            string key=TechnologyCatalog.Ids[id];
            switch(key){
                case "writing":benefits.Add("Negotiate research pacts with friends: 25 gold each, +2 base science/city");break;
                case "laws":benefits.Add("Negotiate defensive alliances with friends; both sides require Code of Laws");break;
                case "agriculture":benefits.Add("Workers can build farms / pastures");break;
                case "wheel":benefits.Add("Workers can build roads");break;
                case "mining":benefits.Add("Workers can build mines");break;
                case "engineering":benefits.Add("Workers can build forts");break;
                case "fire":benefits.Add("Found cities in tundra / arctic forest");break;
                case "electricity":benefits.Add("+3 science per city");break;
                case "irrigation":benefits.Add("+1 food per worked farm beside a river");break;
                case "chemistry":benefits.Add("+1 food per worked farm");break;
                case "steam":benefits.Add("+1 production per worked mine");break;
                case "alphabet":benefits.Add("+1 science per Library");break;
                case "printing":case "computers":benefits.Add("+10% city science");break;
                case "quantum":benefits.Add("+20% city science");break;
                case "nanotech":benefits.Add("+15% city production");break;
                case "satellites":benefits.Add("Reveal the whole map");break;
                case "longevity":benefits.Add("+2 happiness in every city");break;
                case "uploading":benefits.Add("Population no longer consumes food");break;
                case "transcendence":benefits.Add("Science victory");break;
            }
            return benefits.ToArray();
        }
    }
}
