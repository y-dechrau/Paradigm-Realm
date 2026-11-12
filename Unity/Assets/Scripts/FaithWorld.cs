using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Faith, Belief and Religious Power (owner design, 8 Nov 2026; modern rules, save version 36). Built on the existing religion
// (Religion.cs: pantheons, 23 historical faiths, pressure between cities, missionaries, priests, the faith tree, faith buildings).
// The design's six elements and where they live here:
//  Origins      - what was happening when a faith was founded decides its origin (drought and famine, plague, fire mountains,
//                 victories, a great ruler, sacred landscape, philosophy, ancestors) and suggests its beliefs (OriginFor).
//  Beliefs      - the founder picks a core belief, a social doctrine, an economic doctrine and an afterlife (FoundReligion).
//                 Each has one clear effect; afterlives are identity only. Reformation changes a doctrine later.
//  Institutions - eight levels from a sacred gathering to a supreme institution (InstitutionLevel); Religious Authority (0-100)
//                 is the credibility of the institution, separate from faith; a civilisation chooses independent church,
//                 state religion or theocracy.
//  Influence    - every city holds a share of each faith and the unaffiliated (CityShares); conversion is gradual pressure,
//                 helped by trade, holy cities, authority and institutions, resisted by an established faith, Traditionalism,
//                 Monotheism and government policy (permit, restrict missionaries, ban foreign faiths, forced conversion).
//  Power        - religion enters diplomacy (shared faith, persecution of followers, condemnation, holy war, religious treaty,
//                 funding clergy abroad, demanding protection), government and victory.
//  Evolution    - wealth and lost holy cities feed a reform movement; at its height the founder must call a council, accept
//                 reform, suppress it, or let the faith split into a new branch with its own name and doctrine. Faiths outlive
//                 the civilisations that founded them.
//  Events: a great prophet, a great schism, a sacred discovery and religious persecution arise from the state of the world.
//  Religious victory: majority of the world's people, the capitals of half the other civilisations, the holy city and two sacred
//  sites, the highest institution, all held for 20 turns.
[Serializable] public class FaithRecord {
 public int religion,founder=-1,holyCity=-1,turn,origin,core,social,economic,afterlife,authority=50,parent=-1,reform,tenet=-1,lastCouncil=-100,lastProphet=-100;
 public bool holyHeld=true; public string name=""; public List<int> sites=new List<int>();
}
[Serializable] public class FaithEvent {public int kind,owner,city=-1,religion=-1,cell=-1,turn,other=-1;}
public static class FaithCatalog {
 public const int BranchSlots=8;
 public static readonly string[] Origins={"Ancestor worship","Rain and fertility","Fire and destruction","Divine ruler","Sacred landscape","Philosophical school","Healing and the afterlife","Warrior faith"};
 public static readonly string[] OriginTexts={
  "Grew from honouring the founders of the city: +1 faith in the holy city.",
  "Born in drought and famine, praying for rain: +1 food in the holy city.",
  "Born beneath a fire mountain: +1 production in the holy city.",
  "A great ruler was raised among the gods: +1 content citizen in the capital.",
  "A mountain, lake or river was held sacred: +2 faith in the holy city.",
  "A great thinker gathered pupils: +2 science in the holy city.",
  "Born of plague and death: +1 content citizen in the holy city.",
  "Born of victory in battle: +1 production in the holy city."};
 public static readonly string[] Cores={"Polytheism","Monotheism","Philosophical","Animism","Ancestor worship"};
 public static readonly string[] CoreTexts={
  "Many gods, each with a temple: +1 production in following cities; reformation costs half.",
  "One supreme god: spreads a quarter faster; its cities resist other faiths; minorities of 35% or more cause unrest unless Tolerant.",
  "Enlightenment and wisdom: +2 science in following cities; spreads a quarter slower; never causes religious unrest.",
  "Spirits of land and beast: +1 food in following cities; sacred sites count double for authority.",
  "Reverence for the dead and the founders: +1 content citizen in following cities with a Shrine."};
 public static readonly string[] Socials={"Equality","Hierarchy","Charity","Asceticism","Scholarship","Militarism","Pacifism","Expansionism","Traditionalism","Tolerance"};
 public static readonly string[] SocialTexts={
  "+1 content citizen in following cities.",
  "+1 production in following cities; -1 content in following cities of size 8 or more.",
  "+1 content citizen in following cities of size 8 or more; slows the reform movement.",
  "+2 faith and -1 gold in following cities; slows the reform movement.",
  "+2 science in following cities.",
  "Soldiers of civilisations whose state faith this is attack at +1.",
  "War weariness builds half as fast for civilisations whose state faith this is.",
  "Spreads half again as fast; missionaries cost 30 faith instead of 40.",
  "Cities that follow it resist other faiths twice as well.",
  "No religious unrest from minorities; +5 opinion with civilisations of other faiths."};
 public static readonly string[] Economics={"Tithes","Pilgrimage","Sacred trade","Monastic economy","Charitable giving","Religious taxation"};
 public static readonly string[] EconomicTexts={
  "The founder's tithe doubles; feeds the reform movement.",
  "+1 gold in the holy city for every 3 cities in the world that follow the faith.",
  "+1 gold in following cities joined to another city by road, river or sea.",
  "Monasteries give +2 gold as well.",
  "+1 food in following cities of size 4 or less.",
  "The state takes a tenth of its faith as gold each turn; authority falls and the reform movement grows."};
 public static readonly string[] Afterlives={"Reincarnation","Eternal paradise","Ancestral afterlife","Spiritual enlightenment","Resurrection","No afterlife","Divine judgement"};
 public static readonly string[] AfterlifeTexts={"The soul returns in another life.","The faithful rest in paradise.","The dead join the ancestors and watch over the living.","Release from the cycle of desire.","The dead will rise again.","This life is all there is.","Every soul is weighed and judged."};
 public static readonly string[] Institutions={"None","Sacred gathering","Shrine","Temple","Priesthood","Religious council","High clergy","Religious authority","Supreme religious institution"};
 public static readonly string[] Policies={"Permit all faiths","Restrict foreign missionaries","Ban foreign faiths","Forced conversion"};
 public static readonly string[] PolicyTexts={"Foreign missionaries and priests are welcome; no unrest.","Foreign missionaries cannot enter; other faiths spread three quarters as fast in your cities.","Other faiths spread half as fast; minorities of 15% or more worship in secret (-1 content).","Your state faith spreads half again as fast at home, others a quarter as fast; every city with a 10% minority has -1 content; persecution events follow."};
 public static readonly string[] ChurchStates={"Independent church","State religion","Theocracy"};
 public static readonly string[] ChurchTexts={"The church keeps its own counsel: authority rises slowly.","The ruler appoints the clergy: +1 content in cities that follow the state faith; authority cannot pass 80.","The priests rule: +2 faith and -1 science in every city; needs a faith you founded."};
 public static readonly string[] EventNames={"The Great Prophet","The Great Schism","The Sacred Discovery","Religious Persecution"};
 public static readonly string[][] EventChoices={
  new[]{"Fund their teachings (40 gold): authority +10","Invite them to court: +5 authority, the ruler's stress falls","Establish an institution (50 faith): reform movement -20, authority +5","Reject their teachings: reform movement +10"},
  new[]{"Convene a council (60 faith): reform -50, authority -10","Accept the reformers: change the social doctrine, reform 0, authority -5","Suppress the movement: reform 0, authority +5, unrest for 10 turns","Allow the faith to divide: a new branch is born"},
  new[]{"Restore the temple (40 gold): a sacred site, +1 faith","Make it a national pilgrimage site: a sacred site, +1 gold","Study its inscriptions: research progress +30","Incorporate its traditions: authority +10, reform -10"},
  new[]{"Grant religious freedom: policy becomes Permit all faiths","Negotiate with their leaders (30 gold): calm for 10 turns","Increase restrictions: the minority shrinks by a third, unrest for 10 turns","Use force (needs a soldier in the city): the minority is broken, the city loses 1 size, their founder holds a grievance"}};
 public const int ProphetEvent=0,SchismEvent=1,DiscoveryEvent=2,PersecutionEvent=3;
 public static readonly string[] BranchNames={"Reformed {0}","{0} of the New Dawn","Old {0}","Free {0}","Orthodox {0}","{0} Fellowship","Mystic {0}","Southern {0}"};
}
public partial class World {
 public List<FaithRecord> faithRecords=new List<FaithRecord>();
 public List<FaithEvent> faithEvents=new List<FaithEvent>();
 public List<string> faithHistory=new List<string>();
 // ---------- records and names ----------
 public FaithRecord Record(int r){if(faithRecords==null)return null;foreach(var f in faithRecords)if(f.religion==r)return f;return null;}
 public string FaithName(int r){var f=Record(r);if(f!=null&&f.name!="")return f.name;return ReligionCatalog.Name(r);}
 public int FaithTenet(int r){var f=Record(r);return f!=null&&f.tenet>=0?f.tenet:ReligionCatalog.Tenet(r);}
 public static bool IsBranchSlot(int r){return r>=ReligionCatalog.FoundableCount&&r<ReligionCatalog.Count;}
 void History(string line){if(faithHistory==null)faithHistory=new List<string>();faithHistory.Insert(0,"Turn "+turn+": "+line);if(faithHistory.Count>60)faithHistory.RemoveAt(60);}
 public int StateReligion(int owner){if(owner<0||owner>=civCount)return -1;var n=nations[owner];if(n.founded>=0)return n.founded;var cap=CapitalOf(owner);return cap!=null?cap.religion:-1;}
 bool StateHas(int owner,int social){int r=StateReligion(owner);var f=Record(r);return modernRules&&f!=null&&f.social==social;}
 bool FaithIs(int r,Func<FaithRecord,bool> test){var f=Record(r);return modernRules&&f!=null&&test(f);}
 // ---------- origins ----------
 public int OriginFor(int owner,City holy){
  if(holy==null)return 0;Func<Func<Tile,bool>,int,bool> near=(test,range)=>{for(int dj=-range;dj<=range;dj++)for(int di=-range;di<=range;di++){var t=StepIn(holy.x,holy.y,di,dj,range);if(t!=null&&test(t))return true;}return false;};
  var ruler=livingWorld?Ruler(owner):null;
  if(cities.Any(c=>c.owner==owner&&c.scars!=null&&c.scars.Contains(LivingCatalog.ScarFamine))||holy.plague>0)return holy.plague>0?6:1;
  if(stories!=null&&stories.Any(s=>s.deaths>=LivingCatalog.GraveyardAt&&Distance(s.cell%Width,s.cell/Width,holy.x,holy.y)<=5))return 6;
  if(near(t=>t.terrain==17,3))return 2;
  if(units.Where(u=>u.owner==owner).Sum(u=>u.victories)>=5)return 7;
  if(ruler!=null&&(ruler.traitA==LivingCatalog.Ambitious||ruler.traitB==LivingCatalog.Ambitious)&&cities.Count(c=>c.owner==owner)>=3)return 3;
  if(HasTech(owner,"philosophy"))return 5;
  if(near(t=>t.terrain==5||t.terrain==6||t.terrain==15,2))return 4;
  return 0;
 }
 public static int SuggestedCore(int origin){return new[]{4,0,0,1,3,2,1,0}[Math.Max(0,Math.Min(7,origin))];}
 public static int SuggestedSocial(int origin){return new[]{8,2,3,1,9,4,2,5}[Math.Max(0,Math.Min(7,origin))];}
 // ---------- founding with beliefs ----------
 public bool FoundReligion(int owner,int r,int core,int social,int economic,int afterlife){
  choosingBeliefs=true;bool ok;try{ok=FoundReligion(owner,r);}finally{choosingBeliefs=false;}if(!ok)return false;if(!modernRules)return true;City holy;
  var f=Record(r);if(f==null){f=new FaithRecord{religion=r};faithRecords.Add(f);}
  holy=cities.Find(c=>c.id==nations[owner].holyCity);
  f.founder=owner;f.holyCity=nations[owner].holyCity;f.turn=turn;f.origin=OriginFor(owner,holy);f.core=Clamp(core,FaithCatalog.Cores.Length);f.social=Clamp(social,FaithCatalog.Socials.Length);f.economic=Clamp(economic,FaithCatalog.Economics.Length);f.afterlife=Clamp(afterlife,FaithCatalog.Afterlives.Length);f.authority=50;f.holyHeld=true;
  ChooseSacredSites(f,holy,2);
  History(NationName(owner)+" founded "+FaithName(r)+" in "+(holy!=null?holy.name:"its capital")+": "+FaithCatalog.Origins[f.origin].ToLower()+", "+FaithCatalog.Cores[f.core].ToLower()+", "+FaithCatalog.Socials[f.social].ToLower()+", "+FaithCatalog.Economics[f.economic].ToLower()+", "+FaithCatalog.Afterlives[f.afterlife].ToLower()+".");
  return true;
 }
 static int Clamp(int v,int count){return Math.Max(0,Math.Min(count-1,v));}
 // Called by FoundReligion(owner,r) for rivals and old callers: beliefs follow the origin and the leader's agenda.
 void DefaultBeliefs(int owner,int r){
  if(!modernRules||Record(r)!=null)return;var holy=cities.Find(c=>c.id==nations[owner].holyCity);int origin=OriginFor(owner,holy);var e=Civ(owner);int agenda=e!=null?e.Agenda:-1;
  int social=agenda==7?5:agenda==5?6:agenda==1?4:agenda==2?7:agenda==0?9:SuggestedSocial(origin);int economic=agenda==0?2:agenda==3?1:agenda==2?0:(owner+seed)%FaithCatalog.Economics.Length==5?3:(owner+seed)%FaithCatalog.Economics.Length;
  var f=new FaithRecord{religion=r,founder=owner,holyCity=nations[owner].holyCity,turn=turn,origin=origin,core=SuggestedCore(origin),social=social,economic=economic,afterlife=(seed+owner*3)%FaithCatalog.Afterlives.Length};
  faithRecords.Add(f);ChooseSacredSites(f,holy,2);History(NationName(owner)+" founded "+FaithName(r)+" ("+FaithCatalog.Cores[f.core].ToLower()+", "+FaithCatalog.Socials[f.social].ToLower()+").");
 }
 void ChooseSacredSites(FaithRecord f,City holy,int count){
  if(holy==null)return;var found=new List<KeyValuePair<int,int>>();
  for(int dj=-3;dj<=3;dj++)for(int di=-3;di<=3;di++){var t=StepIn(holy.x,holy.y,di,dj,3);if(t==null||t.x==holy.x&&t.y==holy.y)continue;int score=t.ruin?5:t.terrain==17?4:t.terrain==5||t.terrain==15?3:t.terrain==6?3:t.terrain==7?2:0;if(score>0&&!faithRecords.Any(o=>o.sites.Contains(t.y*Width+t.x)))found.Add(new KeyValuePair<int,int>(t.y*Width+t.x,score*10-Distance(t.x,t.y,holy.x,holy.y)));}
  foreach(var s in found.OrderByDescending(a=>a.Value).Take(count))f.sites.Add(s.Key);
 }
 public string SiteName(int cell){var t=tiles[cell];return (t.ruin?"Ancient temple":t.terrain==17?"Fire mountain":t.terrain==5||t.terrain==15?"Sacred mountain":t.terrain==6?"Holy lake":t.terrain==7?"Holy river":"Sacred grove")+" ("+t.x+", "+t.y+")";}
 public int SitesHeld(int r,int owner){var f=Record(r);if(f==null)return 0;return f.sites.Count(s=>s>=0&&s<tiles.Length&&tiles[s].owner==owner);}
 // ---------- institutions and authority ----------
 public int InstitutionLevel(int r){
  var f=Record(r);if(f==null)return 0;var follow=cities.Where(c=>c.religion==r).ToList();int civs=follow.Select(c=>c.owner).Distinct().Count();
  int level=1;if(follow.Any(c=>HasBuilding(c,ShrineBuilding)))level=2;if(follow.Any(c=>HasBuilding(c,36)))level=3;if(follow.Count(c=>HasBuilding(c,36))>=3)level=4;
  if(level>=4&&follow.Count>=6&&f.authority>=40)level=5;if(level>=5&&civs>=2)level=6;if(level>=6&&follow.Count>=12&&f.authority>=60)level=7;
  int homeCivs=Enumerable.Range(0,civCount).Count(o=>!IsNativeNation(o)&&cities.Any(c=>c.owner==o));
  if(level>=7&&f.authority>=75&&Enumerable.Range(0,civCount).Count(o=>{var cap=CapitalOf(o);return cap!=null&&cap.religion==r;})>=Math.Max(2,(homeCivs+1)/2))level=8;
  return level;
 }
 int ChurchState(int owner){return modernRules&&owner>=0&&owner<civCount?nations[owner].churchState:0;}
 public int Policy(int owner){return modernRules&&owner>=0&&owner<civCount?nations[owner].religionPolicy:0;}
 public string ChurchReason(int owner,int state){if(!modernRules)return "Not in this game's rules.";var n=nations[owner];if(n.churchState==state)return "Already chosen.";if(turn-n.churchChanged<10)return "Changed "+(turn-n.churchChanged)+" turns ago; wait "+(10-(turn-n.churchChanged))+".";if(state==2&&n.founded<0)return "A theocracy needs a faith you founded.";return "";}
 public bool SetChurchState(int owner,int state){if(ChurchReason(owner,state)!="")return false;var n=nations[owner];n.churchState=state;n.churchChanged=turn;var f=Record(StateReligion(owner));if(f!=null)f.authority=Math.Max(0,f.authority-10);if(owner==0)Note("The church is now: "+FaithCatalog.ChurchStates[state]+".");History(NationName(owner)+" chose "+FaithCatalog.ChurchStates[state].ToLower()+".");return true;}
 public bool SetPolicy(int owner,int policy){if(!modernRules||policy<0||policy>=FaithCatalog.Policies.Length)return false;nations[owner].religionPolicy=policy;if(owner==0)Note("Religious policy: "+FaithCatalog.Policies[policy]+".");return true;}
 void AuthorityTurn(){
  foreach(var f in faithRecords){
   var holy=cities.Find(c=>c.id==f.holyCity);bool held=holy!=null&&f.founder>=0&&holy.owner==f.founder&&holy.religion==f.religion;
   if(f.holyHeld&&!held&&holy!=null&&f.founder>=0&&holy.owner!=f.founder){f.authority-=25;f.reform+=20;History(FaithName(f.religion)+" lost its holy city "+holy.name+" to "+NationName(holy.owner)+".");}
   f.holyHeld=held;int drift=0;
   if(held&&turn%2==0)drift++;else if(!held&&turn%3==0)drift--;
   int sites=f.founder>=0?SitesHeld(f.religion,f.founder)*(f.core==3?2:1):0;if(turn%5==0)drift+=sites/2;
   if(f.founder>=0&&ChurchState(f.founder)==0&&turn%5==0)drift++;if(f.founder>=0&&ChurchState(f.founder)==2&&turn%2==0)drift++;
   if(f.economic==5&&turn%3==0)drift--;if(f.reform>=70)drift--;
   int cap=f.founder>=0&&ChurchState(f.founder)==1?80:100;f.authority=Math.Max(0,Math.Min(cap,f.authority+drift));
   int follow=Followers(f.religion);int grow=(f.economic==0||f.economic==5?1:0)+(follow>=10?1:0)+(f.authority<40?1:0)+(held?0:2)-((f.social==2||f.social==3)&&turn%2==0?1:0);
   f.reform=Math.Max(0,Math.Min(100,f.reform+grow));
   if(f.reform>=100&&f.founder>=0&&!faithEvents.Any(e=>e.kind==FaithCatalog.SchismEvent&&e.religion==f.religion))RaiseEvent(new FaithEvent{kind=FaithCatalog.SchismEvent,owner=f.founder,religion=f.religion});
  }
 }
 // ---------- pressure factors, used by UpdateReligion ----------
 // How strongly a city sends its faith (percent).
 internal int SendFactor(City s,int r){
  if(!modernRules)return 100;var f=Record(r);int p=100;if(f!=null){p=p*(50+f.authority)/100;if(f.core==1)p=p*5/4;else if(f.core==2)p=p*3/4;if(f.social==7)p=p*3/2;p=p*(100+5*InstitutionLevel(r))/100;}
  if(Policy(s.owner)==3&&r==StateReligion(s.owner))p=p*3/2;if(f!=null&&f.founder>=0&&HasTech(f.founder,"printing"))p=p*5/4;
  return p;
 }
 // How much of that reaches a target city (percent).
 internal int ReceiveFactor(City s,City t,int r){
  if(!modernRules)return 100;int p=100;int state=StateReligion(t.owner);
  if(r!=state){int pol=Policy(t.owner);if(pol==1)p=p*3/4;else if(pol==2)p/=2;else if(pol==3)p/=4;}
  if(t.religion>=0&&t.religion!=r){var mine=Record(t.religion);if(mine!=null){if(mine.social==8)p/=2;if(mine.core==1)p=p*2/3;}int age=turn-t.religionSince;p=p*Math.Max(50,100-age/2)/100;}
  if(s.owner!=t.owner&&contracts.Any(c=>SamePair(c.from,c.to,s.owner,t.owner)))p=p*5/4;
  if(t.owner>=0&&HasTech(t.owner,"industrialisation"))p=p*4/5;
  return p;
 }
 public int PressureRange(int r){var f=Record(r);return modernRules&&f!=null&&f.founder>=0&&HasTech(f.founder,"computers")?15:10;}
 public int MissionRange(int owner){return modernRules&&HasTech(owner,"sailing")?20:12;}
 public int MissionPrice(int owner){return StateHas(owner,7)?30:MissionCost;}
 // ---------- demographics ----------
 // Share of each faith in a city in percent, with the unaffiliated last; the unaffiliated weigh 100 (200 after Industry).
 public int[] CityShares(City c){
  int n=ReligionCatalog.Count;var shares=new int[n+1];if(c.faithPressure==null)return shares;long total=0;int none=c.owner>=0&&HasTech(c.owner,"industrialisation")?200:100;
  for(int r=0;r<n&&r<c.faithPressure.Length;r++)total+=Math.Max(0,c.faithPressure[r]);total+=none;
  int used=0;for(int r=0;r<n&&r<c.faithPressure.Length;r++){shares[r]=(int)(Math.Max(0,c.faithPressure[r])*100/total);used+=shares[r];}shares[n]=100-used;return shares;
 }
 public int ShareOf(City c,int r){if(r<0)return 0;var s=CityShares(c);return r<s.Length?s[r]:0;}
 public List<KeyValuePair<int,int>> Demographics(City c){var s=CityShares(c);var list=new List<KeyValuePair<int,int>>();for(int r=0;r<s.Length-1;r++)if(s[r]>0)list.Add(new KeyValuePair<int,int>(r,s[r]));list=list.OrderByDescending(a=>a.Value).ToList();list.Add(new KeyValuePair<int,int>(-1,s[s.Length-1]));return list;}
 public int LargestMinority(City c,int except){var s=CityShares(c);int best=0;for(int r=0;r<s.Length-1;r++)if(r!=except&&s[r]>best)best=s[r];return best;}
 public long WorldFollowers(int r){long n=0;foreach(var c in cities)if(At(c.x,c.y).realm==0)n+=CityPeople(c)*ShareOf(c,r)/100;return n;}
 // ---------- effects on cities and units ----------
 internal void BeliefYields(City c,ref int f,ref int p,ref int s,ref int g){
  if(!modernRules)return;var rec=Record(c.religion);
  if(rec!=null){
   switch(rec.core){case 0:p++;break;case 2:s+=2;break;case 3:f++;break;}
   switch(rec.social){case 1:p++;break;case 3:g--;break;case 4:s+=2;break;}
   switch(rec.economic){case 2:if(LinkTrade(c)>0)g++;break;case 3:if(HasBuilding(c,MonasteryBuilding))g+=2;break;case 4:if(c.pop<=4)f++;break;}
  }
  if(c.pilgrims>0)g+=c.pilgrims;
  foreach(var o in faithRecords){if(o.holyCity!=c.id||o.founder!=c.owner)continue;switch(o.origin){case 1:f++;break;case 2:case 7:p++;break;case 5:s+=2;break;}if(o.economic==1)g+=Followers(o.religion)/3;}
  if(ChurchState(c.owner)==2&&c.pop>=3)s--;
 }
 internal int BeliefFaith(City c){
  if(!modernRules)return 0;int v=0;var rec=Record(c.religion);if(rec!=null&&rec.social==3)v+=2;if(ChurchState(c.owner)==2)v+=2;
  // The holy city: its origin, and +1 for each restored sacred site its civilisation holds.
  foreach(var o in faithRecords){if(o.holyCity!=c.id||o.founder!=c.owner)continue;if(o.origin==0)v++;if(o.origin==4)v+=2;if(siteRestored!=null)v+=o.sites.Count(s=>s>=0&&s<tiles.Length&&tiles[s].owner==c.owner&&siteRestored.Contains(s));}
  return v;
 }
 internal int BeliefContent(City c){
  if(!modernRules)return 0;int v=0;var rec=Record(c.religion);
  if(rec!=null){if(rec.social==0)v++;if(rec.social==1&&c.pop>=8)v--;if(rec.social==2&&c.pop>=8)v++;if(rec.core==4&&HasBuilding(c,ShrineBuilding))v++;}
  if(ChurchState(c.owner)==1&&c.religion>=0&&c.religion==StateReligion(c.owner))v++;
  foreach(var o in faithRecords){if(o.founder!=c.owner)continue;if(o.origin==3&&nations[c.owner].capital==c.id)v++;if(o.origin==6&&o.holyCity==c.id)v++;}
  v-=ReligiousUnrest(c);return v;
 }
 // Unrest from faiths the state does not hold (shown on the city screen).
 public int ReligiousUnrest(City c){
  if(!modernRules)return 0;int state=StateReligion(c.owner);var rec=Record(state);if(rec!=null&&(rec.social==9||rec.core==2)&&Policy(c.owner)<2)return c.suppressedUntil>turn?1:0;
  int minority=LargestMinority(c,state);int v=0;int pol=Policy(c.owner);
  if(pol==2&&minority>=15)v++;else if(pol==3&&minority>=10)v++;else if(rec!=null&&rec.core==1&&minority>=35)v++;
  if(c.suppressedUntil>turn)v++;if(c.calmUntil>turn)v=0;return Math.Min(2,v);
 }
 public int FaithStrength(Unit u,bool defending){return !defending&&modernRules&&Power[u.type]>0&&StateHas(u.owner,5)?1:0;}
 public int TitheMultiplier(int owner){var f=Record(nations[owner].founded);return modernRules&&f!=null&&f.economic==0?2:1;}
 int TaxationGold(int owner){var f=Record(StateReligion(owner));return modernRules&&f!=null&&f.economic==5&&f.founder==owner?FaithPerTurn(owner)/10:0;}
 // ---------- religious diplomacy ----------
 internal void FaithOpinion(int o,int t,List<KeyValuePair<string,int>> lines,ref int sum){
  if(!modernRules)return;int mine=StateReligion(o),theirs=StateReligion(t);var rec=Record(mine);
  if(mine>=0&&mine==theirs)Part(lines,ref sum,"Shares our faith ("+FaithName(mine)+")",10);
  else if(mine>=0&&theirs>=0){if(rec!=null&&rec.social==9)Part(lines,ref sum,"Tolerant of your faith",5);else if(rec!=null&&rec.core==1)Part(lines,ref sum,"Follows another god ("+FaithName(theirs)+")",-5);}
  if(mine>=0&&cities.Any(c=>c.owner==t&&ShareOf(c,mine)>=10)){int pol=Policy(t);if(pol>=2)Part(lines,ref sum,"Persecutes our faithful",-15);else if(pol==0)Part(lines,ref sum,"Protects our faithful",5);}
  var n=nations[o];if(n.condemnedBy!=null&&n.condemnedBy.Contains(t))Part(lines,ref sum,"Condemned our faith",-15);
 }
 public bool HolyWarJustified(int a,int b){var n=nations[a];return modernRules&&n.holyWarOn==b&&n.holyWarUntil>turn;}
 public string FundReason(int a,int b){if(!modernRules)return "Not in this game's rules.";int r=StateReligion(a);if(r<0)return "You have no state faith.";if(a==b||!HasContact(a,b))return "No contact.";if(Policy(b)>=1)return NationName(b)+" bars foreign missionaries.";if(FaithTreaty(a,b))return "Your religious treaty forbids it.";if(nations[a].faith<30)return "Needs 30 faith.";if(!cities.Any(c=>c.owner==b&&c.religion!=r))return "Their cities already follow it.";return "";}
 // Funding clergy abroad: +80 pressure of your state faith in their two largest cities that do not follow it.
 public bool FundClergy(int a,int b){if(FundReason(a,b)!="")return false;int r=StateReligion(a);nations[a].faith-=30;foreach(var c in cities.Where(c=>c.owner==b&&c.religion!=r).OrderByDescending(c=>c.pop).Take(2).ToList()){Grow(c);c.faithPressure[r]+=80;UpdateMajority(c,false);}DiplomacyEvent(a,b,NationName(a)+" funds clergy of "+FaithName(r)+" in "+NationName(b)+".");return true;}
 public string ProtectionReason(int a,int b){if(!modernRules)return "Not in this game's rules.";int r=StateReligion(a);if(r<0)return "You have no state faith.";if(a==b||!HasContact(a,b))return "No contact.";if(Policy(b)<2)return NationName(b)+" does not persecute other faiths.";if(!cities.Any(c=>c.owner==b&&ShareOf(c,r)>=10))return "None of your faithful live there.";return "";}
 // Demanding protection: a friendly or weaker civilisation lets other faiths be; otherwise you gain a just cause for holy war.
 public bool DemandProtection(int a,int b){if(ProtectionReason(a,b)!="")return false;bool yields=Opinion(b,a)>=20||KnownMilitary(b,a)*2>=KnownMilitary(b,b)*3;if(yields){nations[b].religionPolicy=0;AddOpinion(b,a,OpinionMods.Dealings,-3);DiplomacyEvent(a,b,NationName(b)+" agreed to protect the faithful of "+FaithName(StateReligion(a))+".");}else{nations[a].holyWarOn=b;nations[a].holyWarUntil=turn+20;Grieve(b,a,5);DiplomacyEvent(a,b,NationName(b)+" refused to protect the faithful: just cause for holy war for 20 turns.");}return yields;}
 public string CondemnReason(int a,int target){if(!modernRules)return "Not in this game's rules.";int mine=StateReligion(a);if(mine<0)return "You have no state faith.";if(target<0||target==mine)return "Choose another faith.";if(Followers(target)==0)return "Nobody follows it.";if(nations[a].faith<20)return "Needs 20 faith.";return "";}
 // Condemning a heresy: +5 authority for your faith and -5 reform; civilisations of that faith resent it for 30 turns.
 public bool Condemn(int a,int target){if(CondemnReason(a,target)!="")return false;nations[a].faith-=20;var mine=Record(StateReligion(a));if(mine!=null){mine.authority=Math.Min(100,mine.authority+5);mine.reform=Math.Max(0,mine.reform-5);}for(int o=0;o<civCount;o++)if(o!=a&&StateReligion(o)==target){if(nations[o].condemnedBy==null)nations[o].condemnedBy=new List<int>();if(!nations[o].condemnedBy.Contains(a))nations[o].condemnedBy.Add(a);nations[o].condemnedUntil=turn+30;}History(NationName(a)+" condemned "+FaithName(target)+" as heresy.");return true;}
 public string CouncilReason(int a){if(!modernRules)return "Not in this game's rules.";var f=Record(nations[a].founded);if(f==null)return "Found a faith first.";if(InstitutionLevel(f.religion)<5)return "Needs a religious council (institution level 5).";if(turn-f.lastCouncil<20)return "A council met "+(turn-f.lastCouncil)+" turns ago.";if(nations[a].faith<60)return "Needs 60 faith.";return "";}
 public bool CallCouncil(int a){if(CouncilReason(a)!="")return false;var f=Record(nations[a].founded);nations[a].faith-=60;f.lastCouncil=turn;f.authority=Math.Min(100,f.authority+15);f.reform=Math.Max(0,f.reform-40);History("A council of "+FaithName(f.religion)+" met: authority +15, reform movement -40.");return true;}
 public bool FaithTreaty(int a,int b){var p=Relation(a,b);return p!=null&&p.faithTreatyUntil>turn;}
 public string TreatyReason(int a,int b){if(!modernRules)return "Not in this game's rules.";if(a==b||!HasContact(a,b))return "No contact.";if(FaithTreaty(a,b))return "A treaty is already in force.";if(AtWar(a,b))return "Not at war.";if(Opinion(b,a)<0)return NationName(b)+" will not sign (opinion below 0).";return "";}
 public bool SignFaithTreaty(int a,int b){if(TreatyReason(a,b)!="")return false;Relation(a,b).faithTreatyUntil=turn+30;AddOpinion(b,a,OpinionMods.Dealings,3);DiplomacyEvent(a,b,"Religious treaty: no missionaries or funded clergy between them for 30 turns.");return true;}
 // ---------- reformation and schism ----------
 public string ReformReason(int owner){if(!modernRules)return "Not in this game's rules.";var f=Record(nations[owner].founded);if(f==null)return "Found a faith first.";if(!HasTech(owner,"philosophy"))return "Requires Philosophy.";int cost=ReformCost(owner);if(nations[owner].faith<cost)return "Needs "+cost+" faith.";return "";}
 public int ReformCost(int owner){var f=Record(nations[owner].founded);return f!=null&&f.core==0?40:80;}
 public bool Reform(int owner,int social,int economic){if(ReformReason(owner)!="")return false;var f=Record(nations[owner].founded);nations[owner].faith-=ReformCost(owner);if(social>=0)f.social=Clamp(social,FaithCatalog.Socials.Length);if(economic>=0)f.economic=Clamp(economic,FaithCatalog.Economics.Length);f.authority=Math.Max(0,f.authority-10);f.reform=Math.Max(0,f.reform-40);History(FaithName(f.religion)+" reformed: now "+FaithCatalog.Socials[f.social].ToLower()+", "+FaithCatalog.Economics[f.economic].ToLower()+".");return true;}
 // A branch takes a free slot; the cities that follow the parent far from its holy city, or under another civilisation, go over.
 public int Schism(int r){
  var parent=Record(r);int slot=-1;for(int s=ReligionCatalog.FoundableCount;s<ReligionCatalog.Count;s++)if(Record(s)==null&&Followers(s)==0){slot=s;break;}
  if(parent==null||slot<0){if(parent!=null){parent.reform=0;parent.authority=Math.Max(0,parent.authority-10);}return -1;}
  var holy=cities.Find(c=>c.id==parent.holyCity);var follow=cities.Where(c=>c.religion==r).ToList();
  var leaving=follow.Where(c=>holy==null||c.owner!=parent.founder||Distance(c.x,c.y,holy.x,holy.y)>8).ToList();if(leaving.Count==0)leaving=follow.Where(c=>holy==null||c.id!=holy.id).Take(Math.Max(1,follow.Count/3)).ToList();
  int leader=leaving.Count>0?leaving.GroupBy(c=>c.owner).OrderByDescending(g=>g.Count()).First().Key:-1;var seat=leaving.Where(c=>c.owner==leader).OrderByDescending(c=>c.pop).FirstOrDefault();
  string pattern=FaithCatalog.BranchNames[(slot+turn)%FaithCatalog.BranchNames.Length];
  var b=new FaithRecord{religion=slot,founder=leader>=0&&nations[leader].founded<0?leader:-1,holyCity=seat!=null?seat.id:-1,turn=turn,origin=parent.origin,core=parent.core,social=(parent.social+3)%FaithCatalog.Socials.Length,economic=(parent.economic+1)%FaithCatalog.Economics.Length,afterlife=parent.afterlife,authority=45,parent=r,tenet=FaithTenet(r),name=string.Format(pattern,FaithName(r))};
  faithRecords.Add(b);if(b.founder>=0){nations[b.founder].founded=slot;nations[b.founder].holyCity=b.holyCity;}
  foreach(var c in leaving){Grow(c);int moved=c.faithPressure[r]*2/3;c.faithPressure[r]-=moved;c.faithPressure[slot]+=moved+40;UpdateMajority(c,c.owner==0);c.religionSince=turn;}
  parent.reform=0;parent.authority=Math.Max(0,parent.authority-20);
  History("The Great Schism: "+b.name+" broke away from "+FaithName(r)+(seat!=null?", centred on "+seat.name:"")+".");if(KnowsFaith(r)||parent.founder==0)Note("Schism: "+b.name+" has broken away from "+FaithName(r)+".");
  return slot;
 }
 bool KnowsFaith(int r){return cities.Any(c=>c.owner==0&&ShareOf(c,r)>0);}
 internal static void Grow(City c){if(c.faithPressure==null||c.faithPressure.Length!=ReligionCatalog.Count)c.faithPressure=Grow(c.faithPressure??new int[0],ReligionCatalog.Count);}
 // ---------- events ----------
 internal void RaiseEventForChecks(FaithEvent e){RaiseEvent(e);}
 internal void UpdateMajorityForChecks(City c){UpdateMajority(c,false);}
 void RaiseEvent(FaithEvent e){e.turn=turn;if(e.owner==0){faithEvents.Add(e);Note(FaithCatalog.EventNames[e.kind]+": a decision awaits on the Religion screen.");}else ResolveEvent(e,AIChoice(e));}
 public FaithEvent NextFaithEvent(){return faithEvents!=null&&faithEvents.Count>0?faithEvents[0]:null;}
 int AIChoice(FaithEvent e){var r=new Random(seed+turn*13+e.owner*7+e.kind);switch(e.kind){case FaithCatalog.ProphetEvent:return Treasury(e.owner)>=60?0:1;case FaithCatalog.SchismEvent:return nations[e.owner].faith>=60?0:r.Next(2)==0?2:3;case FaithCatalog.DiscoveryEvent:return r.Next(4);default:return Treasury(e.owner)>=30?1:0;}}
 public string EventText(FaithEvent e){
  var c=cities.Find(x=>x.id==e.city);string faith=FaithName(e.religion);
  switch(e.kind){
   case FaithCatalog.ProphetEvent:return "A charismatic teacher of "+faith+" has appeared in "+(c!=null?c.name:"your capital")+". Crowds gather wherever they speak.";
   case FaithCatalog.SchismEvent:return "The clergy of "+faith+" are divided over doctrine. Reformers say the faith has grown rich and forgotten its first teachings. The movement has reached its height: you must decide.";
   case FaithCatalog.DiscoveryEvent:return "Workers near "+(c!=null?c.name:"your lands")+" have uncovered "+(e.cell>=0?SiteName(e.cell).ToLower():"an ancient temple")+".";
   default:return "Followers of "+faith+" in "+(c!=null?c.name:"one of your cities")+" ("+(c!=null?ShareOf(c,e.religion):0)+"% of its people) protest against the restrictions on their worship.";}
 }
 public string ChoiceReason(FaithEvent e,int choice){
  var n=nations[e.owner];var c=cities.Find(x=>x.id==e.city);
  if(e.kind==FaithCatalog.ProphetEvent&&choice==0&&Treasury(e.owner)<40)return "Needs 40 gold.";if(e.kind==FaithCatalog.ProphetEvent&&choice==2&&n.faith<50)return "Needs 50 faith.";
  if(e.kind==FaithCatalog.SchismEvent&&choice==0&&n.faith<60)return "Needs 60 faith.";
  if(e.kind==FaithCatalog.DiscoveryEvent&&choice==0&&Treasury(e.owner)<40)return "Needs 40 gold.";
  if(e.kind==FaithCatalog.PersecutionEvent&&choice==1&&Treasury(e.owner)<30)return "Needs 30 gold.";if(e.kind==FaithCatalog.PersecutionEvent&&choice==3&&(c==null||!units.Any(u=>u.owner==e.owner&&Power[u.type]>0&&u.x==c.x&&u.y==c.y)))return "Needs a soldier in the city.";
  return "";
 }
 public bool ResolveEvent(FaithEvent e,int choice){
  if(ChoiceReason(e,choice)!=""){if(e.owner!=0)choice=e.kind==FaithCatalog.SchismEvent?3:e.kind==FaithCatalog.PersecutionEvent?0:1;else return false;if(ChoiceReason(e,choice)!="")choice=e.kind==FaithCatalog.SchismEvent?3:e.kind==FaithCatalog.PersecutionEvent?0:3;}
  faithEvents.Remove(e);var n=nations[e.owner];var f=Record(e.religion);var c=cities.Find(x=>x.id==e.city);string what=FaithCatalog.EventChoices[e.kind][choice].Split(':')[0].Split('(')[0].Trim();
  switch(e.kind){
   case FaithCatalog.ProphetEvent:if(f==null)break;if(choice==0){AddDiplomaticGold(e.owner,-40);f.authority=Math.Min(100,f.authority+10);}else if(choice==1){f.authority=Math.Min(100,f.authority+5);var ruler=livingWorld?Ruler(e.owner):null;if(ruler!=null)ruler.stress=Math.Max(0,ruler.stress-15);}else if(choice==2){n.faith-=50;f.reform=Math.Max(0,f.reform-20);f.authority=Math.Min(100,f.authority+5);}else f.reform=Math.Min(100,f.reform+10);f.lastProphet=turn;break;
   case FaithCatalog.SchismEvent:if(f==null)break;if(choice==0){n.faith-=60;f.reform=Math.Max(0,f.reform-50);f.authority=Math.Max(0,f.authority-10);}else if(choice==1){f.social=(f.social+1)%FaithCatalog.Socials.Length;f.reform=0;f.authority=Math.Max(0,f.authority-5);}else if(choice==2){f.reform=0;f.authority=Math.Min(100,f.authority+5);foreach(var x in cities.Where(x=>x.owner==e.owner&&x.religion==e.religion))x.suppressedUntil=turn+10;}else Schism(e.religion);break;
   case FaithCatalog.DiscoveryEvent:{int r=StateReligion(e.owner);var g=Record(r);if(choice==0){AddDiplomaticGold(e.owner,-40);if(g!=null&&e.cell>=0&&!g.sites.Contains(e.cell))g.sites.Add(e.cell);if(siteRestored==null)siteRestored=new List<int>();if(e.cell>=0)siteRestored.Add(e.cell);}else if(choice==1){if(g!=null&&e.cell>=0&&!g.sites.Contains(e.cell))g.sites.Add(e.cell);if(c!=null)c.pilgrims++;}else if(choice==2){if(n.research>=0)n.progress[n.research]+=30;if(e.owner==0&&research>=0)science+=30;}else if(g!=null){g.authority=Math.Min(100,g.authority+10);g.reform=Math.Max(0,g.reform-10);}if(e.cell>=0){if(discoveredSites==null)discoveredSites=new List<int>();discoveredSites.Add(e.cell);}break;}
   default:if(c==null)break;if(choice==0)n.religionPolicy=0;else if(choice==1){AddDiplomaticGold(e.owner,-30);c.calmUntil=turn+10;}else if(choice==2){Grow(c);c.faithPressure[e.religion]=c.faithPressure[e.religion]*2/3;c.suppressedUntil=turn+10;UpdateMajority(c,false);}else{Grow(c);c.faithPressure[e.religion]=c.faithPressure[e.religion]*3/10;c.pop=Math.Max(1,c.pop-1);NormaliseSpecialists(c);UpdateMajority(c,false);var lead=Record(e.religion);if(lead!=null&&lead.founder>=0&&lead.founder!=e.owner)Grieve(lead.founder,e.owner,20);if(livingWorld)AddScar(c,LivingCatalog.ScarConquest);}c.lastPersecution=turn;break;
  }
  History(NationName(e.owner)+" — "+FaithCatalog.EventNames[e.kind]+": "+what+".");return true;
 }
 void FaithEventsTurn(){
  for(int o=0;o<civCount;o++){
   if(IsNativeNation(o)||!cities.Any(c=>c.owner==o))continue;var rnd=new Random(seed*17+turn*131+o*7);var n=nations[o];if(o==0&&faithEvents.Count>=2)continue;
   var f=Record(n.founded);var holy=f==null?null:cities.Find(c=>c.id==f.holyCity&&c.owner==o);
   if(f!=null&&holy!=null&&holy.pop>=5&&turn-f.lastProphet>=30&&rnd.Next(40)==0)RaiseEvent(new FaithEvent{kind=FaithCatalog.ProphetEvent,owner=o,religion=f.religion,city=holy.id});
   int state=StateReligion(o);
   if(state>=0&&rnd.Next(60)==0){var spot=tiles.FirstOrDefault(t=>t.owner==o&&t.realm==0&&(t.ruin||t.terrain==17||t.terrain==6)&&(discoveredSites==null||!discoveredSites.Contains(t.y*Width+t.x))&&!faithRecords.Any(r=>r.sites.Contains(t.y*Width+t.x)));if(spot!=null){var near=cities.Where(c=>c.owner==o).OrderBy(c=>Distance(c.x,c.y,spot.x,spot.y)).FirstOrDefault();RaiseEvent(new FaithEvent{kind=FaithCatalog.DiscoveryEvent,owner=o,religion=state,cell=spot.y*Width+spot.x,city=near!=null?near.id:-1});}}
   if(Policy(o)>=2||Record(state)!=null&&Record(state).core==1){foreach(var c in cities.Where(c=>c.owner==o&&turn-c.lastPersecution>=25)){int minority=-1,best=24;var s=CityShares(c);for(int r=0;r<s.Length-1;r++)if(r!=state&&s[r]>best){best=s[r];minority=r;}if(minority>=0&&rnd.Next(12)==0){RaiseEvent(new FaithEvent{kind=FaithCatalog.PersecutionEvent,owner=o,religion=minority,city=c.id});c.lastPersecution=turn;break;}}}
  }
 }
 // ---------- rivals ----------
 void FaithPolicyAI(int o){
  if(!modernRules||!ActiveRival(o)||(turn+o)%10!=0)return;var n=nations[o];var e=Civ(o);int agenda=e!=null?e.Agenda:-1;var f=Record(n.founded);
  int want=agenda==2&&f!=null?2:f!=null?1:0;if(n.churchState!=want&&ChurchReason(o,want)=="")SetChurchState(o,want);
  int policy=agenda==2&&Posture(o)==DiplomacyFlavours.Aggressor?2:agenda==6?1:0;if(f!=null&&f.core==1&&agenda==2)policy=Math.Max(policy,1);n.religionPolicy=policy;
  if(f!=null&&CouncilReason(o)==""&&f.reform>=60)CallCouncil(o);
  if(f!=null&&n.faith>=80){var targets=Enumerable.Range(0,civCount).Where(b=>b!=o&&FundReason(o,b)=="").OrderByDescending(b=>Opinion(o,b)).ToList();if(targets.Count>0)FundClergy(o,targets[0]);}
 }
 // ---------- for the Religion screen (owner mockup, 8 Nov 2026) ----------
 static readonly string[] Mottos={"Many gods, one people.","One god. One truth. One people.","Wisdom. Balance. Peace.","Earth, river and sky.","We honour those who came before."};
 public string Motto(int r){var f=Record(r);return f==null?"":Mottos[Math.Max(0,Math.Min(Mottos.Length-1,f.core))];}
 public int WorldShare(int r){long all=cities.Where(c=>At(c.x,c.y).realm==0).Sum(c=>CityPeople(c));return all<=0?0:(int)(WorldFollowers(r)*1000/all);}
 public string FaithSummary(int r){
  if(r<0)return "Your people have no faith of their own yet. Choose a pantheon, learn Mysticism and raise a Temple, then found one under Beliefs.";
  var f=Record(r);int share=WorldShare(r);string standing=share>=500?"A dominant faith":share>=200?"A great faith":share>=50?"A growing faith":"A young faith";
  string core=f==null?"":new[]{"many gods","one god","the way of wisdom","the spirits of nature","reverence for the ancestors"}[Math.Max(0,Math.Min(4,f.core))];int civs=cities.Where(c=>c.religion==r).Select(c=>c.owner).Distinct().Count();
  return standing+", "+FaithName(r)+(core!=""?" teaches "+core:"")+" in "+Followers(r)+(Followers(r)==1?" city":" cities")+" of "+civs+(civs==1?" civilisation":" civilisations")+"."+(f!=null&&f.reform>=60?" A reform movement is stirring.":f!=null&&f.authority>=75?" Its clergy speak with great authority.":"");
 }
 // How content a city's believers are with their faith's place in it (percent): minorities, suppression and unrest lower it.
 public int ReligiousSatisfaction(City c){int state=StateReligion(c.owner);int v=100-LargestMinority(c,state)/2-20*ReligiousUnrest(c);if(state>=0&&c.religion!=state)v-=15;if(c.suppressedUntil>turn)v-=15;return Math.Max(0,Math.Min(100,v));}
 // Pilgrimage programme: 40 faith brings pilgrims to the holy city (+1 gold there, up to 3) and +3 authority.
 public string PilgrimageReason(int owner){if(!modernRules)return "Not in this game's rules.";var f=Record(nations[owner].founded);if(f==null)return "Found a faith first.";var holy=cities.Find(c=>c.id==f.holyCity&&c.owner==owner);if(holy==null)return "You do not hold the holy city.";if(holy.pilgrims>=3)return "Pilgrims already crowd "+holy.name+".";if(nations[owner].faith<40)return "Needs 40 faith.";return "";}
 public bool FundPilgrimage(int owner){if(PilgrimageReason(owner)!="")return false;var f=Record(nations[owner].founded);var holy=cities.Find(c=>c.id==f.holyCity);nations[owner].faith-=40;holy.pilgrims++;f.authority=Math.Min(100,f.authority+3);History("Pilgrimage to "+holy.name+": +1 gold there, authority +3.");return true;}
 // Recent religious history as (text, turns ago).
 public List<KeyValuePair<string,int>> RecentFaith(int count){var list=new List<KeyValuePair<string,int>>();if(faithHistory==null)return list;foreach(var h in faithHistory){if(list.Count>=count)break;int colon=h.IndexOf(':');int t;if(h.StartsWith("Turn ")&&colon>5&&int.TryParse(h.Substring(5,colon-5),out t))list.Add(new KeyValuePair<string,int>(h.Substring(colon+1).Trim(),turn-t));else list.Add(new KeyValuePair<string,int>(h,0));}return list;}
 // ---------- the turn ----------
 internal void FaithTurn(){
  if(!modernRules)return;if(faithRecords==null)faithRecords=new List<FaithRecord>();if(faithEvents==null)faithEvents=new List<FaithEvent>();
  for(int o=0;o<civCount;o++){var n=nations[o];if(n.founded>=0&&Record(n.founded)==null)DefaultBeliefs(o,n.founded);int tax=TaxationGold(o);if(tax>0)AddDiplomaticGold(o,tax);if(n.condemnedBy!=null&&n.condemnedUntil<=turn)n.condemnedBy.Clear();FaithPolicyAI(o);}
  AuthorityTurn();FaithEventsTurn();
  // Forced conversion wears minorities down; Pacifism softens war weariness.
  foreach(var c in cities){if(Policy(c.owner)!=3||c.faithPressure==null)continue;int state=StateReligion(c.owner);for(int r=0;r<c.faithPressure.Length;r++)if(r!=state)c.faithPressure[r]-=c.faithPressure[r]/20;}
  for(int o=0;o<civCount;o++)if(StateHas(o,6)&&turn%2==0)for(int e=0;e<civCount;e++)if(e!=o&&AtWar(o,e))AddExhaustion(o,e,-1);
 }
 // ---------- victory ----------
 public float ReligiousProgress(int owner,out string line){
  line="";int r=nations[owner].founded;if(!modernRules||r<0){line="Found a faith to compete.";return 0f;}
  long world=cities.Where(c=>At(c.x,c.y).realm==0).Sum(c=>CityPeople(c)),mine=WorldFollowers(r);float people=world==0?0:Math.Min(1f,mine*2f/world);
  var others=Enumerable.Range(0,civCount).Where(o=>o!=owner&&!IsNativeNation(o)&&Survives(o)).ToList();int capitals=others.Count(o=>{var cap=CapitalOf(o);return cap!=null&&cap.religion==r;});int needCaps=Math.Max(1,(others.Count+1)/2);float caps=Math.Min(1f,capitals/(float)needCaps);
  var f=Record(r);var holy=f==null?null:cities.Find(c=>c.id==f.holyCity);bool holyHeld=holy!=null&&holy.owner==owner;int sites=SitesHeld(r,owner);float places=((holyHeld?1:0)+Math.Min(2,sites))/3f;
  int level=InstitutionLevel(r);float inst=level/8f;int hold=nations[owner].faithHold;
  line=FaithName(r)+": "+(world==0?0:mine*100/world)+"% of the world's people (50% needed), the faith of "+capitals+" of "+needCaps+" rival capitals needed, holy city "+(holyHeld?"held":"not held")+" and "+sites+" of 2 sacred sites, institution "+FaithCatalog.Institutions[level].ToLower()+" (level "+level+" of 8). All together for "+hold+" of "+ReligiousHoldTurns+" turns.";
  float all=(people+caps+places+inst)/4f;return all>=1f?Math.Min(1f,.9f+.1f*hold/ReligiousHoldTurns):all*.9f;
 }
 public const int ReligiousHoldTurns=20;
 public void UpdateReligiousVictory(){
  if(!modernRules||result!="")return;
  for(int o=0;o<civCount;o++){if(IsNativeNation(o)||!Survives(o))continue;string line;float p=ReligiousProgress(o,out line);if(p>=.9f)nations[o].faithHold++;else nations[o].faithHold=0;
   if(nations[o].faithHold>=ReligiousHoldTurns){result=o==0?"Religious victory! "+FaithName(nations[0].founded)+" has become the faith of the world.":NationName(o)+"'s faith, "+FaithName(nations[o].founded)+", has won the world. Defeat.";return;}}
 }
 internal void UpgradeFaith(){if(faithRecords==null)faithRecords=new List<FaithRecord>();if(faithEvents==null)faithEvents=new List<FaithEvent>();if(faithHistory==null)faithHistory=new List<string>();foreach(var c in cities){Grow(c);if(c.religionSince==0)c.religionSince=turn;}foreach(var n in nations)if(n.condemnedBy==null)n.condemnedBy=new List<int>();}
 public List<int> siteRestored=new List<int>(),discoveredSites=new List<int>();
}
}
