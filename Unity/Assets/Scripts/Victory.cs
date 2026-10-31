using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm
{
    // 16-bit plan Stage 6 (CORE-DESIGN-16BIT.md): three ways to win, shown as three bars.
    // Conquest: hold every rival capital, or eliminate every rival (rivals can win the same way).
    // Transcendence: a three-stage project (300, 400, 500 shields) built in one city of size 8 or more that knows Computing;
    // each stage then activates over 10 turns (5 with the Transcendence technology). Losing that city loses the project.
    // Concord: friendship or alliance with every surviving rival for ConcordTarget turns in a row.
    public partial class World
    {
        public const int ConcordTarget=20,ProjectSize=8,ProjectActivationBase=10;
        public int concordTurns;
        public bool Survives(int owner){return cities.Any(c=>c.owner==owner)||units.Any(u=>u.owner==owner);}
        // The home world's rivals: the native peoples of the parallel worlds are not needed for Concord or conquest.
        public List<int> SurvivingRivals(){var list=new List<int>();for(int o=1;o<civCount;o++)if(Survives(o)&&!CivilisationCatalog.IsNative(nations[o].civ))list.Add(o);return list;}
        public City Capital(int owner){int id=nations[owner].capital;return id<0?null:cities.Find(c=>c.id==id);}
        // A rival counts as conquered by owner when it is eliminated or owner holds its capital.
        public bool Conquered(int owner,int rival){if(!Survives(rival))return true;var cap=Capital(rival);return cap!=null&&cap.owner==owner;}
        public bool ConquestAchieved(int owner){if(!Survives(owner))return false;bool any=false;for(int o=0;o<civCount;o++){if(o==owner)continue;// Home civilisations need not conquer the native peoples of the parallel worlds.
if(CivilisationCatalog.IsNative(nations[o].civ)&&!CivilisationCatalog.IsNative(nations[owner].civ))continue;any=true;if(!Conquered(owner,o))return false;}return any;}
        // Called once per turn after diplomacy has advanced.
        public void UpdateConcord()
        {
            if(result!="")return;
            var alive=SurvivingRivals();
            if(alive.Count>0&&alive.All(o=>HasContact(0,o)&&Friends(0,o)))concordTurns++;else concordTurns=0;
            if(concordTurns>=ConcordTargetFor(0))result="Concord victory! Every surviving civilisation stands with you in friendship.";
        }

        // ---------- Transcendence project ----------
        public static int ProjectStageOf(int b){return b-69;}
        public int ActivationTurns(int owner){return HasTech(owner,"transcendence")?ProjectActivationBase/2:ProjectActivationBase;}
        public City ProjectCity(int owner){int id=nations[owner].projectCity;return id<0?null:cities.Find(c=>c.id==id&&c.owner==owner);}
        public int NextProjectStage(int owner){var n=nations[owner];return n.projectStage>=3?-1:70+n.projectStage;}
        string ProjectReason(City c,int b)
        {
            var n=nations[c.owner];int stage=ProjectStageOf(b);
            if(n.projectStage>=3)return "Transcendence complete";
            if(stage!=n.projectStage+1)return stage<=n.projectStage?"Stage already complete":"Complete stage "+(n.projectStage+1)+" first";
            if(n.projectActivation>0)return "Stage "+n.projectStage+" is still activating ("+n.projectActivation+" turns)";
            var home=ProjectCity(c.owner);if(n.projectStage>0&&home!=null&&home!=c)return "Must be built in "+home.name;
            if(c.pop<ProjectSize)return "Requires city size "+ProjectSize;
            return "";
        }
        void CompleteProjectStage(City c,int b)
        {
            var n=nations[c.owner];n.projectCity=c.id;n.projectStage=ProjectStageOf(b);n.projectActivation=ActivationTurns(c.owner);
            if(c.owner==0)Note(c.name+" completed Transcendence stage "+n.projectStage+" of 3. It activates in "+n.projectActivation+" turns; protect the city.");
            else if(HasContact(0,c.owner))Note("Warning: "+NationName(c.owner)+" completed Transcendence stage "+n.projectStage+" of 3 in "+c.name+".");
        }
        void ResetProject(int owner,string why){var n=nations[owner];if(n.projectStage==0&&n.projectCity<0)return;n.projectCity=-1;n.projectStage=0;n.projectActivation=0;if(owner==0)Note("Transcendence project lost: "+why);else if(HasContact(0,owner))Note(NationName(owner)+" lost its Transcendence project.");}
        // A captured city takes its owner's project with it.
        void LoseProjectCity(City c){if(c!=null&&nations[c.owner].projectCity==c.id&&nations[c.owner].projectStage>0)ResetProject(c.owner,c.name+" was captured.");}
        // Once per turn: count down activation; the third activated stage wins.
        public void UpdateProjects()
        {
            if(result!="")return;
            for(int o=0;o<civCount;o++)
            {
                var n=nations[o];if(n.projectStage==0)continue;
                if(ProjectCity(o)==null){ResetProject(o,"its city is no longer held.");continue;}
                if(n.projectActivation<=0)continue;
                n.projectActivation--;
                if(n.projectActivation>0)continue;
                if(n.projectStage>=3){result=o==0?"Transcendence victory! Your civilisation has become something new.":NationName(o)+" has achieved Transcendence. Defeat.";return;}
                if(o==0)Note("Transcendence stage "+n.projectStage+" is active. Stage "+(n.projectStage+1)+" can begin.");
            }
        }
        bool ProjectsValid(){return nations.All(n=>n!=null&&n.projectStage>=0&&n.projectStage<=3&&n.projectActivation>=0&&n.projectActivation<=ProjectActivationBase&&(n.projectStage==0||n.projectCity>=0));}
        // Fraction of the way to Transcendence: each stage is one third, half built and half activated.
        public float ProjectProgress(int owner)
        {
            var n=nations[owner];float done=n.projectStage-(n.projectActivation>0?n.projectActivation/(float)(ActivationTurns(owner)*2):0);
            var home=ProjectCity(owner)??cities.Where(c=>c.owner==owner&&c.buildingProject==NextProjectStage(owner)).OrderByDescending(c=>c.buildingProduction).FirstOrDefault();
            int next=NextProjectStage(owner);
            if(next>=0&&n.projectActivation==0&&home!=null&&home.buildingProject==next)done+=.5f*Math.Min(1f,home.buildingProduction/(float)Math.Max(1,CostFor(home,next)));
            return Math.Max(0,Math.Min(1f,done/3f));
        }
        string ProjectStatus(int owner)
        {
            var n=nations[owner];int next=NextProjectStage(owner);var home=ProjectCity(owner);
            if(n.projectStage>=3&&n.projectActivation>0)return "stage 3 activating, "+n.projectActivation+" turns left";
            if(n.projectActivation>0)return "stage "+n.projectStage+" activating, "+n.projectActivation+" turns left";
            var building=home??cities.Where(c=>c.owner==owner&&c.buildingProject==next).OrderByDescending(c=>c.buildingProduction).FirstOrDefault();
            if(building!=null&&building.buildingProject==next)return "building stage "+(n.projectStage+1)+" in "+building.name+" ("+building.buildingProduction+" / "+CostFor(building,next)+")";
            if(n.projectStage>0)return n.projectStage+" of 3 stages active; stage "+(n.projectStage+1)+" not started";
            return HasTech(owner,"computers")?"not started":"needs Computing";
        }
        // Progress figures for the Victory screen. Each is 0..1 with a one-line explanation.
        public string[] VictoryText(out float[] progress)
        {
            progress=new float[3];
            int rivals=civCount-1,held=Enumerable.Range(1,Math.Max(0,rivals)).Count(o=>Conquered(0,o));
            progress[0]=rivals==0?1f:held/(float)rivals;
            progress[1]=ProjectProgress(0);
            progress[2]=Math.Min(1f,concordTurns/(float)ConcordTargetFor(0));
            int leader=-1;float best=0;for(int o=1;o<civCount;o++){float p=ProjectProgress(o);if(p>best){best=p;leader=o;}}
            string rivalLine=leader<0?"No rival has begun.":(HasContact(0,leader)?NationName(leader):"An unknown rival")+": "+ProjectStatus(leader)+".";
            return new[]{
                "Conquest: "+held+" of "+rivals+" rivals conquered (their capital held by you, or eliminated). A rival holding every capital, yours included, wins instead.",
                "Transcendence: "+ProjectStatus(0)+". Three stages of 300, 400 and 500 shields in one city of size "+ProjectSize+" or more with Computing; each activates over "+ActivationTurns(0)+" turns. Losing the city loses the project. "+rivalLine,
                "Concord: "+concordTurns+" of "+ConcordTargetFor(0)+" turns in a row at friendship or alliance with every surviving rival. A broken treaty or a rival you have not met resets the count."};
        }
    }
}
