using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm
{
    // Victory tracking: Conquest and Science (Transcendence) already ended the game in CheckVictory;
    // Concord is new: friendship or alliance with every surviving rival for ConcordTarget turns in a row.
    public partial class World
    {
        public const int ConcordTarget=20;
        public int concordTurns;
        public bool Survives(int owner){return cities.Any(c=>c.owner==owner)||units.Any(u=>u.owner==owner);}
        public List<int> SurvivingRivals(){var list=new List<int>();for(int o=1;o<civCount;o++)if(Survives(o))list.Add(o);return list;}
        // Called once per turn after diplomacy has advanced.
        public void UpdateConcord()
        {
            if(result!="")return;
            var alive=SurvivingRivals();
            if(alive.Count>0&&alive.All(o=>HasContact(0,o)&&Friends(0,o)))concordTurns++;else concordTurns=0;
            if(concordTurns>=ConcordTarget)result="Concord victory! Every surviving civilisation stands with you in friendship.";
        }
        // Progress figures for the Victory screen. Each is 0..1 with a one-line explanation.
        public string[] VictoryText(out float[] progress)
        {
            progress=new float[3];
            int rivals=civCount-1,alive=SurvivingRivals().Count;
            progress[0]=rivals==0?1f:(rivals-alive)/(float)rivals;
            int known=tech.Count(t=>t),total=TechnologyCatalog.Count;
            progress[1]=HasTech(0,"transcendence")?1f:known/(float)total;
            progress[2]=Math.Min(1f,concordTurns/(float)ConcordTarget);
            int bestRival=0;for(int o=1;o<civCount;o++)bestRival=Math.Max(bestRival,nations[o].tech.Count(t=>t));
            return new[]{
                "Conquest: "+(rivals-alive)+" of "+rivals+" rival civilisations eliminated. Remove every rival city and unit to win.",
                "Transcendence: "+known+" of "+total+" technologies known (strongest rival: "+bestRival+"). Researching Transcendence wins the game, and so does a rival reaching it first.",
                "Concord: "+concordTurns+" of "+ConcordTarget+" turns in a row at friendship or alliance with every surviving rival. A broken treaty or a rival you have not met resets the count."};
        }
    }
}
