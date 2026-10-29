using System;
using System.Linq;
namespace ParadigmRealm {
// 16-bit plan Stage 5 (CORE-DESIGN-16BIT.md): knowing 4 of an era's 6 keystones completes that era and offers a choice of
// 1 of 3 permanent edicts. The design names no edicts; these use levers the game already has (proposal, tune in play).
// Academies is the design's "80% science after an edict".
public partial class World {
 public const int EraThreshold=4;
 public static readonly string[][] EdictNames={
  new[]{"Common Granaries","Stone Masons","Sky Watchers"},
  new[]{"Coinage","Civic Festivals","Academies"},
  new[]{"Standing Army","Guild Charters","Royal Charters"},
  new[]{"Rail Network","Free Press","Conscription"},
  new[]{"Global Markets","Research Grants","Welfare State"},
  new[]{"Planetary Grid","Open Knowledge","Harmony"}};
 public static readonly string[][] EdictTexts={
  new[]{"+1 food in every city","+1 production in every city","+1 science in every city"},
  new[]{"+2 gold in every city","+1 content citizen in every city","Science rate may rise to 80%"},
  new[]{"+1 defence for all your units","+2 production in every city","+2 science in every city"},
  new[]{"+3 production in every city","+1 content citizen and +1 science in every city","New units start as Veterans"},
  new[]{"+4 gold in every city","+4 science in every city","+2 content citizens in every city"},
  new[]{"+5 production in every city","+5 science in every city","+3 content citizens in every city"}};
 // What each edict does, for the AI: F food, P production, S science, G gold, C content, D defence, A academies, V veterans.
 static readonly string[] EdictKinds={"FPS","GCA","DPS","PCV","GSC","PSC"};
 public static int[] NoEdicts(){var e=new int[TechnologyCatalog.EraCount];for(int i=0;i<e.Length;i++)e[i]=-1;return e;}
 public int KnownInEra(int owner,int era){int n=0;for(int t=era*TechnologyCatalog.PerEra;t<(era+1)*TechnologyCatalog.PerEra;t++)if(nations[owner].tech[t])n++;return n;}
 public bool EraComplete(int owner,int era){return era>=0&&era<TechnologyCatalog.EraCount&&nations[owner].tech.Length==TechnologyCatalog.Count&&KnownInEra(owner,era)>=EraThreshold;}
 // The age a civilisation is in: one past the run of completed eras from the Dawn (capped at the last era).
 public int Era(int owner){int e=0;while(e<TechnologyCatalog.EraCount-1&&EraComplete(owner,e))e++;return e;}
 public bool HasEdict(int owner,int era,int choice){if(owner<0||owner>=civCount)return false;var e=nations[owner].edicts;return e!=null&&era<e.Length&&e[era]==choice;}
 // The earliest completed era whose edict is still to be chosen, or -1.
 public int PendingEdict(int owner){var e=nations[owner].edicts;if(e==null)return -1;for(int era=0;era<e.Length;era++)if(e[era]<0&&EraComplete(owner,era))return era;return -1;}
 public bool ChooseEdict(int owner,int era,int choice){if(owner<0||owner>=civCount||result!=""||choice<0||choice>2||era<0||era>=TechnologyCatalog.EraCount)return false;var e=nations[owner].edicts;if(e==null||e[era]>=0||!EraComplete(owner,era))return false;e[era]=choice;if(owner==0)Note("Edict adopted: "+EdictNames[era][choice]+" ("+EdictTexts[era][choice]+").");return true;}
 void AnnounceEra(int owner,int eraBefore){if(owner!=0)return;int now=Era(owner);if(now>eraBefore)Note("A new age: "+TechnologyCatalog.EraNames[now]+". Choose an edict on the Research screen.");else if(PendingEdict(owner)>=0&&KnownInEra(owner,PendingEdict(owner))==EraThreshold)Note(TechnologyCatalog.EraNames[PendingEdict(owner)]+" complete. Choose an edict on the Research screen.");}
 // AI: content when cities are unhappy, gold in deficit, otherwise science, then production.
 void AIChooseEdicts(int owner){
  // Choosing fails once the game has a result (for example a conquest earlier in the same turn), so stop rather than retry.
  if(result!="")return;
  for(int era=PendingEdict(owner);era>=0;era=PendingEdict(owner)){
   string kinds=EdictKinds[era];bool unhappy=cities.Any(c=>c.owner==owner&&Happiness(c)<1),deficit=Budget(owner)[2]<0;
   string order=(unhappy?"C":"")+(deficit?"G":"")+"SAPGCFDV";int pick=0;
   foreach(char k in order){int i=kinds.IndexOf(k);if(i>=0){pick=i;break;}}
   if(!ChooseEdict(owner,era,pick))return;
  }
 }
 public void EmpireAIForChecks(int owner){AIChooseEdicts(owner);}
 public int EdictFood(int owner){return HasEdict(owner,0,0)?1:0;}
 public int EdictProduction(int owner){return (HasEdict(owner,0,1)?1:0)+(HasEdict(owner,2,1)?2:0)+(HasEdict(owner,3,0)?3:0)+(HasEdict(owner,5,0)?5:0);}
 public int EdictScience(int owner){return (HasEdict(owner,0,2)?1:0)+(HasEdict(owner,2,2)?2:0)+(HasEdict(owner,3,1)?1:0)+(HasEdict(owner,4,1)?4:0)+(HasEdict(owner,5,1)?5:0);}
 public int EdictGold(int owner){return (HasEdict(owner,1,0)?2:0)+(HasEdict(owner,4,0)?4:0);}
 public int EdictContent(int owner){return (HasEdict(owner,1,1)?1:0)+(HasEdict(owner,3,1)?1:0)+(HasEdict(owner,4,2)?2:0)+(HasEdict(owner,5,2)?3:0);}
 bool EdictsValid(){return nations.All(n=>n!=null&&n.edicts!=null&&n.edicts.Length==TechnologyCatalog.EraCount&&n.edicts.All(v=>v>=-1&&v<=2));}
}
}
