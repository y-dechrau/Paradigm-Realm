using System;
using System.Linq;
namespace ParadigmRealm {
// The faith tree (owner request, 7 Nov 2026): eighteen doctrines in three lines through the six ages, learned with devotion, which is
// gathered from faith every turn alongside science. It follows the civics tree of Civilization VI, which runs beside the technology
// tree on a second currency: here faith plays the part culture plays there. A doctrine of a later age opens only once the civilisation
// has reached that age through science, and it needs the doctrine before it in its line. Spending faith on pantheons, founding and
// missionaries is unchanged: devotion is counted separately, so the tree never competes with them.
public static class DoctrineCatalog {
 public static readonly string[] Names={
  "Ancestor Rites","Sacred Groves","Shamans",
  "Temple Festivals","Scribes of the Faith","Temple Tithes",
  "Holy Orders","Monastic Schools","Pilgrim Roads",
  "Cathedral Builders","Mission Societies","Charity Hospitals",
  "Interfaith Councils","Sacred Science","Faith Banks",
  "Ascended Mind","Unity of Souls","Cosmic Peace"};
 // Effects use the technology effect codes: H content, F/T terrain yields, S science, G gold, W production, D defence, and Y faith.
 public static readonly string[] Effects={"H:1","F:1","Y:20","H:1","S:5","G:10","D:melee","S:8","T:0","W:8","Y:30","F:0","H:1","S:8","G:10","S:10","Y:50","H:1"};
 public static readonly string[] Lines={"Rites","Learning","Community"};
 public const int Count=18,PerAge=3;
 public static int Age(int d){return d/PerAge;}
 public static int Cost(int d){int a=Age(d)+1;return 40*a*a;}
 public static int Before(int d){return d>=PerAge?d-PerAge:-1;}
 public static string EffectText(int d){var e=Effects[d];return e.StartsWith("Y:")?"+"+e.Substring(2)+"% faith in every city":TechnologyCatalog.EffectText(e);}
}
public partial class World {
 public bool DoctrineKnown(int owner,int d){var n=nations[owner];return n.doctrines!=null&&d>=0&&d<n.doctrines.Length&&n.doctrines[d];}
 public string DoctrineReason(int owner,int d){if(d<0||d>=DoctrineCatalog.Count)return "No such doctrine.";if(DoctrineKnown(owner,d))return "Already adopted.";if(DoctrineCatalog.Age(d)>Era(owner))return "Opens in the "+TechnologyCatalog.EraNames[DoctrineCatalog.Age(d)]+".";int before=DoctrineCatalog.Before(d);if(before>=0&&!DoctrineKnown(owner,before))return "Needs "+DoctrineCatalog.Names[before]+" first.";return "";}
 public bool DoctrineOpen(int owner,int d){return DoctrineReason(owner,d)=="";}
 public bool ChooseDoctrine(int owner,int d){if(!DoctrineOpen(owner,d))return false;nations[owner].doctrine=d;return true;}
 int NextDoctrine(int owner){return Enumerable.Range(0,DoctrineCatalog.Count).Where(d=>DoctrineOpen(owner,d)).OrderBy(DoctrineCatalog.Cost).ThenBy(d=>d).DefaultIfEmpty(-1).First();}
 // Turns until the chosen doctrine is adopted, or -1 with no faith coming in.
 public int DoctrineTurns(int owner){var n=nations[owner];if(n.doctrine<0)return -1;int per=FaithPerTurn(owner);if(per<=0)return -1;return Math.Max(1,(DoctrineCatalog.Cost(n.doctrine)-n.devotion+per-1)/per);}
 // Once a turn, after faith is gathered.
 void AdvanceDoctrines(int owner){
  if(!modernRules)return;var n=nations[owner];if(n.doctrines==null||n.doctrines.Length!=DoctrineCatalog.Count)n.doctrines=new bool[DoctrineCatalog.Count];
  if(n.doctrine<0||!DoctrineOpen(owner,n.doctrine))n.doctrine=NextDoctrine(owner);if(n.doctrine<0)return;
  n.devotion+=FaithPerTurn(owner);int cost=DoctrineCatalog.Cost(n.doctrine);
  if(n.devotion>=cost){n.devotion-=cost;n.doctrines[n.doctrine]=true;if(owner==0)Note("Doctrine adopted: "+DoctrineCatalog.Names[n.doctrine]+" ("+DoctrineCatalog.EffectText(n.doctrine)+").");n.doctrine=NextDoctrine(owner);}
 }
 public int DoctrineCount(int owner){var d=nations[owner].doctrines;return d==null?0:d.Count(a=>a);}
}
}
