using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// The living world (owner request, 7 Nov 2026), after RimWorld and Dwarf Fortress, in four parts; save version 34.
//  (Governors were removed on 12 Nov 2026 at the owner's request; only rulers remain, and every city follows the priority.)
//  1 People: every ruler and city governor is a person with two traits, stress, loyalty and corruption. Stress builds from unrest,
//    hunger, war, debt and old wounds; at breaking point they snap (tantrums, binges, purges, melancholy). Rival governors feud.
//    A city whose governor stays disloyal rebels and may defect to a neighbour.
//  2 Memory: cities carry permanent scars (famine, conquest) and civilisations keep generational grudges that never fade, some of
//    them from a history written before the game begins.
//  3 Materials: soldiers are equipped from what the city can really supply (smithies, iron, bronze), gear wears in battle, and the
//    city that makes a breakthrough holds its blueprint: whoever captures that city learns from it.
//  4 Living map: tiles remember their battles; a battlefield becomes a graveyard that draws pilgrims, breeds disease and draws
//    scavengers, and a people resents whoever holds the graves of its dead.
//  Plus imperial priorities: the ruler sets one aim, and each city carries it out only if its governor is loyal and honest enough.
[Serializable] public class Character {public int id,owner,city=-1,traitA,traitB,stress,loyalty=60,corruption,rival=-1,breakTurns,breakKind;public string name="";
 // Save version 38 (Governors.cs): turns left to settle in (0 = settled) and the policy the governor runs.
 public int settle,policy;}
[Serializable] public class Grudge {public int holder,against,reason,turn;public string text="";}
[Serializable] public class TileStory {public int cell,battles,deaths,lastTurn;public List<int> fallen=new List<int>();}
[Serializable] public class Artifact {public int city,tech,maker;}
public static class LivingCatalog {
 public static readonly string[] Traits={"Ambitious","Loyal","Greedy","Honest","Paranoid","Stoic","Neurotic","Pious","Brave","Drunkard"};
 public static readonly string[] TraitTexts={"less loyal, but drives the city hard","far more loyal","takes a cut: more corruption","keeps the books clean: less corruption","stress builds every turn","stress builds half as fast","stress builds half again as fast","calmer in a city that follows a faith","war does not trouble them","more corrupt, and given to binges"};
 public const int Ambitious=0,Loyal=1,Greedy=2,Honest=3,Paranoid=4,Stoic=5,Neurotic=6,Pious=7,Brave=8,Drunkard=9;
 static readonly string[] First={"Aldric","Berenice","Cassius","Darya","Eamon","Fenna","Gaius","Halla","Iskander","Jorunn","Kasimir","Livia","Marek","Nerys","Orsolya","Piran","Quintus","Runa","Sabine","Taro","Ulfric","Vesna","Wendel","Ximena","Yorick","Zofia","Anselm","Brigid","Corvin","Dagny","Emeric","Faustina","Gerolt","Hesper","Ilian","Junia"};
 static readonly string[] Epithets={"the Bold","the Quiet","the Fat","of the Long Road","the Younger","the Elder","Ironhand","the Wise","the Unlucky","Silvertongue","the Lame","the Fair","Oathkeeper","the Pale","the Glutton","Stormborn"};
 public static string Name(Random r){return First[r.Next(First.Length)]+" "+Epithets[r.Next(Epithets.Length)];}
 public static readonly string[] Scars={"","Famine","Conquest","Sacked"};
 public const int ScarFamine=1,ScarConquest=2;
 public static readonly string[] Priorities={"None","Fortify","Feed the people","Expand","Learning","Commerce"};
 public static readonly string[] PriorityTexts={"Cities follow their own needs.","Walls and soldiers first.","Farms and food first.","Settlers and growth.","Science first.","Gold and trade first."};
 public static readonly string[] Gear={"Poor","Common","Fine","Masterwork"};
 public const int BattlefieldAt=2,GraveyardAt=6;
}
public partial class World {
 public bool livingWorld;
 public List<Character> characters=new List<Character>();
 public List<Grudge> grudges=new List<Grudge>();
 public List<TileStory> stories=new List<TileStory>();
 public List<Artifact> artifacts=new List<Artifact>();
 // ---------- people ----------
 bool HasPeople(int owner){return owner>=0&&owner<civCount&&!IsNativeNation(owner);}
 // Looked up constantly (every content check asks), so kept in an index rebuilt whenever the list of people changes.
 [NonSerialized] Dictionary<int,Character> governorIndex,rulerIndex;[NonSerialized] int peopleIndexed=-1;[NonSerialized] Character peopleLast;
 void IndexPeople(){if(characters==null)return;var last=characters.Count>0?characters[characters.Count-1]:null;if(governorIndex!=null&&peopleIndexed==characters.Count&&peopleLast==last)return;governorIndex=new Dictionary<int,Character>();rulerIndex=new Dictionary<int,Character>();foreach(var p in characters){if(p.city>=0)governorIndex[p.city]=p;else if(!rulerIndex.ContainsKey(p.owner))rulerIndex[p.owner]=p;}peopleIndexed=characters.Count;peopleLast=last;}
 public Character Ruler(int owner){if(characters==null)return null;IndexPeople();Character p;return rulerIndex.TryGetValue(owner,out p)&&p.owner==owner&&p.city<0?p:null;}
 public static bool HasTrait(Character c,int t){return c!=null&&(c.traitA==t||c.traitB==t);}
 public string TraitLine(Character c){return c==null?"":LivingCatalog.Traits[c.traitA]+", "+LivingCatalog.Traits[c.traitB];}
 Character MakePerson(int owner,int city,Random r){int a=r.Next(LivingCatalog.Traits.Length),b=r.Next(LivingCatalog.Traits.Length-1);if(b>=a)b++;var p=new Character{id=nextId++,owner=owner,city=city,traitA=a,traitB=b,stress=r.Next(10,30),loyalty=50+r.Next(25)+(a==LivingCatalog.Loyal||b==LivingCatalog.Loyal?20:0)-(a==LivingCatalog.Ambitious||b==LivingCatalog.Ambitious?10:0),name=LivingCatalog.Name(r)};characters.Add(p);return p;}
 // Governors were removed (owner request, 12 Nov 2026: too unpredictable, no value). Any left in an older save are dismissed, and the
 // corruption, strikes and brewing rebellions they caused are cleared. Called on load and every turn; it does nothing once they are gone.
 internal void DismissGovernors(){
  if(characters==null)return;if(characters.RemoveAll(p=>p==null||p.city>=0)>0){peopleIndexed=-1;governorIndex=null;}
  foreach(var p in characters)p.rival=-1;if(cities!=null)foreach(var c in cities){c.corruption=0;c.strike=0;c.unrestTurns=0;}
 }
 // A ruler for each home civilisation that has a city.
 void EnsurePeople(){
  if(characters==null)characters=new List<Character>();DismissGovernors();var r=new Random(seed*13+turn*7+characters.Count);
  for(int o=0;o<civCount;o++)if(HasPeople(o)&&cities.Any(c=>c.owner==o)&&Ruler(o)==null)MakePerson(o,-1,r);
 }
 // Once a turn for each home civilisation, before its cities work.
 void LivingTurn(int owner){
  if(!livingWorld||!HasPeople(owner))return;EnsurePeople();MendGear(owner);var n=nations[owner];var ruler=Ruler(owner);var own=cities.Where(c=>c.owner==owner).ToList();if(own.Count==0)return;var r=new Random(seed*31+turn*101+owner);
  int war=Enumerable.Range(0,civCount).Count(o=>o!=owner&&AtWar(owner,o));
  // The ruler.
  if(ruler!=null){int load=(n.arrears>0?6:0)+(war>0?3:0)+own.Count(Disorder)*2+(n.rationShort>0?4:0);Stress(ruler,load,war);if(ruler.breakTurns>0)ruler.breakTurns--;else if(ruler.stress>=85)RulerBreak(owner,ruler,r);}
  ApplyPriority(owner);
 }
 void Stress(Character p,int load,int war){
  if(war>0&&HasTrait(p,LivingCatalog.Brave))load-=3;if(HasTrait(p,LivingCatalog.Paranoid))load+=2;
  if(load>0){if(HasTrait(p,LivingCatalog.Stoic))load=(load+1)/2;if(HasTrait(p,LivingCatalog.Neurotic))load=load*3/2;}
  p.stress=Math.Max(0,Math.Min(100,p.stress+load-2));
 }
 public static readonly string[] BreakNames={"","Tantrum","Binge","Withdrawn","Purge","Melancholy","Folly"};
 void RulerBreak(int owner,Character ruler,Random r){
  int kind=5+r.Next(2);ruler.breakKind=kind;ruler.breakTurns=6;ruler.stress=45;var n=nations[owner];
  if(kind==5){if(owner==0)Note(ruler.name+" has fallen into a black melancholy. For six turns every city has one citizen fewer content.");}
  else{int lost=n.gold/5;n.gold-=lost;if(owner==0){gold=n.gold;Note(ruler.name+" has spent "+lost+" gold on a palace of follies.");}}
 }
 // ---------- memory ----------
 public void AddScar(City c,int scar){if(c.scars==null)c.scars=new List<int>();if(c.scars.Count<6)c.scars.Add(scar);}
 public void AddGrudge(int holder,int against,int reason,string text){if(!livingWorld||holder<0||against<0||holder==against)return;if(grudges==null)grudges=new List<Grudge>();grudges.Add(new Grudge{holder=holder,against=against,reason=reason,turn=turn,text=text});}
 // Grudges never fade: each costs 10 opinion, up to 40.
 public int GrudgePenalty(int observer,int other){if(!livingWorld||grudges==null)return 0;int n=0;foreach(var g in grudges)if(g.holder==observer&&g.against==other)n++;return Math.Min(40,n*10)+GraveLeverage(observer,other);}
 internal void CityCaptured(City c,int from,int to){if(!livingWorld)return;AddScar(c,LivingCatalog.ScarConquest);AddGrudge(from,to,1,"captured "+c.name);StealArtifacts(c,to);}
 // Before the first turn: a few ancient wrongs between the home civilisations, remembered in the Chronicle and in every later dealing.
 static readonly string[] AncientWrongs={"burned the ancestral city of {0}","broke the sacred oath sworn at {0}","carried off the treasure of {0}","murdered the envoys of {0}","drowned the harvest of {0}"};
 void WriteAncientHistory(){
  var r=new Random(seed*17+5);var homes=Enumerable.Range(0,civCount).Where(HasPeople).ToList();if(homes.Count<2)return;int events=Math.Min(6,2+homes.Count/2);
  for(int k=0;k<events;k++){int a=homes[r.Next(homes.Count)],b=homes[r.Next(homes.Count)];if(a==b)continue;string place=SharedCityNames[r.Next(SharedCityNames.Length)];int years=50+r.Next(16)*25;string what=string.Format(AncientWrongs[r.Next(AncientWrongs.Length)],place);
   AddGrudge(b,a,2,what+", "+years+" years ago");log.Insert(0,"Before history: "+years+" years ago "+NationName(a)+" "+what+". "+NationName(b)+" has not forgotten.");}
 }
 // ---------- materials ----------
 // Gear from what the city can really supply: a Workshop or Barracks, and iron (or bronze) reaching it by road.
 public int GearFor(City c,int type){if(!livingWorld||Power[type]<=0)return 1;int q=0;if(HasBuilding(c,30)||HasBuilding(c,32))q++;if(HasResource(c.owner,2))q++;else if(HasResource(c.owner,3))q++;if(HasTech(c.owner,"military_science"))q++;return Math.Max(0,Math.Min(3,q));}
 public int GearBonus(Unit u){if(!livingWorld||Power[u.type]<=0)return 0;int q=u.gear-(u.wear>=4?1:0);return q<=0?-1:q>=2?1:0;}
 public string GearText(Unit u){if(!livingWorld||Power[u.type]<=0)return "";return " | "+LivingCatalog.Gear[Math.Max(0,Math.Min(3,u.gear))]+" gear"+(u.wear>=4?", worn":"");}
 void Wear(Unit u){if(u!=null&&u.hp>0&&Power[u.type]>0){u.wear++;if(u.wear>=6&&u.gear>0){u.gear--;u.wear=0;}}}
 // Repairs: a unit resting in its own city with a Barracks or Workshop has its gear mended.
 void MendGear(int owner){foreach(var u in units)if(u.owner==owner&&u.wear>0&&cities.Any(c=>c.owner==owner&&c.x==u.x&&c.y==u.y&&(HasBuilding(c,30)||HasBuilding(c,32))))u.wear=0;}
 // A breakthrough is written down in the city with the most science; capture the city and you can read it.
 void MakeArtifact(int owner,int tech){if(!livingWorld||!HasPeople(owner))return;var home=cities.Where(c=>c.owner==owner).OrderByDescending(c=>Yield(c)[2]).ThenBy(c=>c.id).FirstOrDefault();if(home==null)return;if(artifacts==null)artifacts=new List<Artifact>();artifacts.Add(new Artifact{city=home.id,tech=tech,maker=owner});}
 void StealArtifacts(City c,int to){if(artifacts==null)return;foreach(var a in artifacts.Where(a=>a.city==c.id).ToList()){var n=nations[to];if(n.tech[a.tech]){artifacts.Remove(a);continue;}n.progress[a.tech]+=ResearchCost(a.tech,to)/2;if(to==0){if(research==a.tech)science=n.progress[a.tech];Note("In "+c.name+" your scholars found the blueprints of "+TechNames[a.tech]+": half of it is learned already.");}a.maker=to;}}
 public List<Artifact> ArtifactsIn(City c){return artifacts==null?new List<Artifact>():artifacts.Where(a=>a.city==c.id).ToList();}
 // ---------- the living map ----------
 public TileStory StoryAt(int x,int y){if(stories==null)return null;int cell=y*Width+x;return stories.Find(s=>s.cell==cell);}
 internal void RecordBattle(int x,int y,int deaths,int ownerA,int ownerB){
  if(!livingWorld)return;var t=At(x,y);if(t==null)return;if(stories==null)stories=new List<TileStory>();var s=StoryAt(t.x,t.y);if(s==null){s=new TileStory{cell=t.y*Width+t.x};stories.Add(s);}
  s.battles++;s.deaths+=deaths;s.lastTurn=turn;foreach(int o in new[]{ownerA,ownerB})if(o>=0&&deaths>0&&!s.fallen.Contains(o))s.fallen.Add(o);
  if(s.deaths>=LivingCatalog.GraveyardAt&&s.deaths-deaths<LivingCatalog.GraveyardAt&&(ownerA==0||ownerB==0||Visible(t.x,t.y)))Note("So many have fallen at "+t.x+","+t.y+" that it is now a graveyard. Pilgrims will come, and so will sickness.");
 }
 public string StoryText(int x,int y){var s=StoryAt(x,y);if(s==null)return "";if(s.deaths>=LivingCatalog.GraveyardAt)return "Graveyard: "+s.deaths+" fallen in "+s.battles+" battles";if(s.battles>=LivingCatalog.BattlefieldAt)return "Battlefield ("+s.battles+" battles)";return "";}
 bool Graveyard(TileStory s){return s!=null&&s.deaths>=LivingCatalog.GraveyardAt;}
 // Pilgrims come to a city within three tiles of a graveyard: +2 faith. Sickness may spread from it to a city within two.
 public int PilgrimFaith(City c){if(!livingWorld||stories==null)return 0;foreach(var s in stories)if(Graveyard(s)&&Distance(c.x,c.y,tiles[s.cell].x,tiles[s.cell].y)<=3)return 2;return 0;}
 void GraveSickness(){if(!livingWorld||stories==null)return;var r=new Random(seed+turn*61);foreach(var s in stories.Where(Graveyard)){var t=tiles[s.cell];foreach(var c in cities)if(c.plague==0&&Distance(c.x,c.y,t.x,t.y)<=2&&r.Next(40)==0){c.plague=3;if(c.owner==0)Note("Sickness from the graveyard near "+c.name+" has reached the city.");}}}
 // A people resents whoever holds the graves of its dead.
 int GraveLeverage(int observer,int other){if(stories==null)return 0;int n=0;foreach(var s in stories)if(Graveyard(s)&&s.fallen.Contains(observer)&&tiles[s.cell].owner==other)n++;return Math.Min(20,n*10);}
 public Tile GraveyardFor(Random r){if(stories==null)return null;var graves=stories.Where(s=>Graveyard(s)&&tiles[s.cell].owner<0).ToList();return graves.Count==0?null:tiles[graves[r.Next(graves.Count)].cell];}
 // ---------- imperial priorities ----------
 // Every city carries out the ruler's priority (governors, who could refuse, were removed on 12 Nov 2026).
 void ApplyPriority(int owner){
  int p=nations[owner].priority;if(p<=0)return;
  foreach(var c in cities.Where(a=>a.owner==owner)){
   c.focus=p==2?1:p==4?3:p==5?4:p==3?1:2;
   if(c.buildingProject<0&&c.buildingQueue.Count==0){int want=p==1?9:p==2?7:p==4?8:p==5?31:-1;if(want>=0&&BuildReason(c,want)==""&&TechAllows(c,want))ChooseProject(c,want,false);}
   if(c.build<0&&c.queue.Count==0){int unit=p==1?CityDefender(c):p==3&&c.pop>=3&&!units.Any(u=>u.owner==owner&&u.type==0)?UnitBuild(0):-1;if(unit>=0&&BuildReason(c,unit)=="")ChooseProject(c,unit,false);}
  }
 }
 int CityDefender(City c){return Choices(c).Where(b=>BuildUnit(b)>=2&&!IsNaval(BuildUnit(b))&&Power[BuildUnit(b)]>0).OrderByDescending(b=>Defence[BuildUnit(b)]).DefaultIfEmpty(-1).First();}
 // The content citizens a city loses to its past and its ruler: old wounds, and a ruler's melancholy.
 public int LivingContent(City c){if(!livingWorld)return 0;int v=-(c.scars==null?0:Math.Min(2,c.scars.Count));var ruler=Ruler(c.owner);if(ruler!=null&&ruler.breakKind==5&&ruler.breakTurns>0)v--;return v;}
 void StartLivingWorld(){livingWorld=true;EnsurePeople();WriteAncientHistory();}
}
}
