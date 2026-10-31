using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Religion (owner request, 31 Oct 2026; save version 22). Twelve living faiths and eleven of the ancient world, each with its
// real origin, a short accurate account and a tenet drawn from its practice. Sources: Pew Research Center, "How the Global
// Religious Landscape Changed From 2010 to 2020" (2025) and "Other Religions" (2015); Encyclopaedia Iranica on Manichaeism;
// general histories. The mechanic follows the genre (Civilization V and VI, Old World): faith builds up, a pantheon of folk
// beliefs comes first, a faith is founded in a city with a Temple (its holy city), cities press their majority faith on cities
// within ten tiles, and faith buys missionaries. Here the tenet belongs to the faith itself rather than being picked.
public static class ReligionCatalog {
 // name | tradition | origin and history | adherents today | technology needed to found | tenet kind | tenet name | tenet text
 // Tenet kinds: 0 +1 food, 1 +1 production, 2 +2 gold, 3 +2 science, 4 +1 content, 5 +2 faith, 6 spreads half again as fast
 // (all in the cities that follow the faith, whoever owns them).
 static readonly string[][] Table={
  new[]{"Christianity","Abrahamic","Founded on the teaching of Jesus of Nazareth in Roman Judea in the 1st century CE; spread by Paul and other missionaries and made the Roman state religion in 380.","About 2.3 billion (Pew, 2020)","philosophy","6","Great Commission","Missionary zeal: spreads half again as fast."},
  new[]{"Islam","Abrahamic","Revealed to the Prophet Muhammad in Mecca from 610 CE, recorded in the Qur'an; within a century it reached from Iberia to Central Asia.","About 2.0 billion (Pew, 2020)","education","2","Zakat","Almsgiving: +2 gold in following cities."},
  new[]{"Hinduism","Dharmic","Grew from the Vedic religion of north India, its oldest texts from about 1500 BCE; no single founder, many traditions of devotion and philosophy.","About 1.2 billion (Pew, 2020)","philosophy","4","Dharma","Duty and order: +1 content citizen in following cities."},
  new[]{"Buddhism","Dharmic","Founded by Siddhartha Gautama, the Buddha, in north India around the 5th century BCE; carried across Asia, helped by the emperor Ashoka.","About 300-500 million (estimates vary)","philosophy","4","Middle Way","Calm of the sangha: +1 content citizen in following cities."},
  new[]{"Judaism","Abrahamic","The covenant faith of the Israelites in the Iron Age Levant, centred on the Torah; it survived exile and dispersion through study and law.","About 15-17 million (Pew, 2020)","philosophy","3","Torah Study","Learning as worship: +2 science in following cities."},
  new[]{"Sikhism","Dharmic","Founded by Guru Nanak in Punjab around 1500 CE and shaped by nine later Gurus; scripture is the Guru Granth Sahib.","About 25-30 million","printing","0","Langar","The free community kitchen: +1 food in following cities."},
  new[]{"Taoism","Taoic","Chinese tradition traced to Laozi and the Tao Te Ching (6th-4th century BCE); harmony with the Tao, the way of nature.","Tens of millions, mainly China and Taiwan (estimates vary widely)","philosophy","0","Wu Wei","Harmony with nature: +1 food in following cities."},
  new[]{"Confucianism","Taoic","The ethical and ritual teaching of Confucius (551-479 BCE); state orthodoxy of imperial China and the basis of its examinations.","Practised widely in East Asia; rarely counted as a separate faith","philosophy","3","Civil Examinations","Learned officials: +2 science in following cities."},
  new[]{"Shinto","Japanese","The way of the kami, the spirits of Japan's land and ancestors, with shrines from prehistory; first written down in the 8th century.","About 3 million counted alone; far more take part in its rites","philosophy","5","Kami Shrines","Sacred places everywhere: +2 faith in following cities."},
  new[]{"Jainism","Dharmic","Taught in its present form by Mahavira, a contemporary of the Buddha, in the 6th-5th century BCE; non-violence (ahimsa) above all.","About 4-5 million","philosophy","4","Ahimsa","Non-violence: +1 content citizen in following cities."},
  new[]{"Zoroastrianism","Iranian","Founded by the prophet Zarathustra (dated between the 15th and 6th centuries BCE); faith of the Achaemenid and Sasanian empires of Persia.","About 100,000-200,000","philosophy","5","Fire Temples","Eternal flames: +2 faith in following cities."},
  new[]{"Baha'i Faith","Abrahamic","Founded by Baha'u'llah in Persia in 1863; teaches the oneness of God, of religion and of humanity.","About 5-7 million","industrialisation","6","Unity of Humanity","Open to all: spreads half again as fast."},
  new[]{"Egyptian Religion","Ancient","The gods of the Nile - Ra, Osiris, Isis, Horus - from before 3000 BCE; the last temple, at Philae, was closed in the 6th century CE.","No longer practised (modern revivals only)","mysticism","1","Monument Builders","Temples and tombs: +1 production in following cities."},
  new[]{"Mesopotamian Religion","Ancient","Gods of Sumer, Akkad, Babylon and Assyria such as Anu, Enlil, Inanna and Marduk, worshipped in ziggurat temples from the 4th millennium BCE.","No longer practised","mysticism","0","Temple Granaries","Temple estates: +1 food in following cities."},
  new[]{"Greco-Roman Religion","Ancient","The Olympian gods of Greece and their Roman counterparts, honoured with civic festivals and oracles until the Theodosian decrees of the 390s CE.","No longer practised (modern revivals only)","mysticism","4","Civic Festivals","Games and feasts: +1 content citizen in following cities."},
  new[]{"Norse Religion","Ancient","Odin, Thor, Freyja and the other gods of Scandinavia, known from the Eddas; replaced by Christianity around 1000-1100 CE.","No longer practised (modern revivals only)","mysticism","1","Valhalla","Warrior's honour: +1 production in following cities."},
  new[]{"Manichaeism","Iranian","Founded by the prophet Mani in Sasanian Persia in the 3rd century CE; at its height it reached from North Africa to China before dying out.","Extinct","philosophy","2","Silk Road Merchants","Carried by traders: +2 gold in following cities."},
  new[]{"Mithraism","Ancient","A Roman mystery cult of the god Mithras, popular with soldiers from the 1st to the 4th century CE and worshipped in underground temples.","Extinct","philosophy","1","Soldiers' Mysteries","Disciplined brotherhood: +1 production in following cities."},
  new[]{"Tengrism","Steppe","Worship of Tengri, the Eternal Blue Sky, by Turkic and Mongol peoples of the steppe, including the empire of Genghis Khan.","Small modern revival","mysticism","5","Eternal Blue Sky","Sky and spirits: +2 faith in following cities."},
  new[]{"Aztec Religion","Ancient","The gods of the Mexica - Huitzilopochtli, Tlaloc, Quetzalcoatl - worshipped at the Templo Mayor of Tenochtitlan until 1521.","No longer practised","mysticism","1","The Fifth Sun","Great temple works: +1 production in following cities."},
  new[]{"Inca Religion","Ancient","Worship of Inti the sun, Pachamama the earth and the ancestors across the Andes, centred on Cusco until the Spanish conquest of the 1530s.","No longer practised","mysticism","0","Ayni","Shared labour on terraces: +1 food in following cities."},
  new[]{"Celtic Druidism","Ancient","The religion of the Celtic peoples of Gaul and the British Isles, kept by druid priests; suppressed under Rome and gone by early medieval times.","No longer practised (modern revivals only)","mysticism","3","Druidic Lore","Keepers of knowledge: +2 science in following cities."},
  new[]{"Canaanite Religion","Ancient","Gods of the Levant and Phoenicia such as El, Baal and Asherah, carried by Phoenician sailors across the Mediterranean.","No longer practised","mysticism","2","Sea Traders","Phoenician commerce: +2 gold in following cities."},
 };
 public static readonly int Count=Table.Length;
 public static string Name(int r){return r>=0&&r<Count?Table[r][0]:"None";}
 public static string Tradition(int r){return Table[r][1];}
 public static string History(int r){return Table[r][2];}
 public static string Adherents(int r){return Table[r][3];}
 public static string Tech(int r){return Table[r][4];}
 public static int Tenet(int r){return r>=0&&r<Count?int.Parse(Table[r][5]):-1;}
 public static string TenetName(int r){return Table[r][6];}
 public static string TenetText(int r){return Table[r][7];}
 // Pantheons: folk beliefs a civilisation adopts first, applying in every city it owns (the capital only for Festival Gods).
 public static readonly string[] PantheonNames={"Harvest Mother","Forge God","God of Trade","Oracle","Ancestor Spirits","Festival Gods"};
 public static readonly string[] PantheonText={"Fertility rites, like those of Demeter or Isis: +1 food in every city.","Smith gods such as Hephaestus or Ptah: +1 production in every city.","Patrons of markets such as Hermes or Mercury: +2 gold in every city.","Seers like the oracles of Delphi or Siwa: +1 science in every city.","Honouring the dead at household shrines: +1 faith in every city.","Feast days for the gods: +2 content citizens in the capital."};
}
public partial class World {
 public const int PantheonCost=25,MissionCost=40;
 public int FoundingCost(){return 60+20*nations.Count(n=>n.founded>=0);}
 public int MaxReligions(){return Math.Min(8,Math.Max(3,civCount));}
 public bool IsHolyCity(City c){return nations.Any(n=>n.holyCity==c.id&&n.founded>=0);}
 public int FaithYield(City c){if(Disorder(c))return 0;var n=nations[c.owner];int f=n.capital==c.id?1:0;if(HasBuilding(c,36))f+=2;if(IsHolyCity(c))f+=3;if(n.pantheon==4)f++;if(ReligionCatalog.Tenet(c.religion)==5)f+=2;return f+CivFaith(c)+AgeFaith(c);}
 public int FaithPerTurn(int owner){return cities.Where(c=>c.owner==owner).Sum(FaithYield);}
 // Tenets help the cities that follow the faith; the pantheon helps every city of its civilisation.
 void ApplyReligionYields(City c,ref int f,ref int p,ref int s,ref int g){int pan=nations[c.owner].pantheon;if(pan==0)f++;else if(pan==1)p++;else if(pan==2)g+=2;else if(pan==3)s++;switch(ReligionCatalog.Tenet(c.religion)){case 0:f++;break;case 1:p++;break;case 2:g+=2;break;case 3:s+=2;break;}}
 public int ReligionContent(City c){var n=nations[c.owner];return (ReligionCatalog.Tenet(c.religion)==4?1:0)+(n.pantheon==5&&n.capital==c.id?2:0)+(c.religion>=0&&c.religion==n.founded?1:0);}
 // The founder's tithe: +1 gold for every two cities in the world that follow its faith.
 public int ReligionTithe(int owner){int r=nations[owner].founded;return r<0?0:cities.Count(c=>c.religion==r)/2;}
 public int Followers(int religion){return cities.Count(c=>c.religion==religion);}
 public string ReligionFounder(int religion){for(int o=0;o<civCount;o++)if(nations[o].founded==religion)return NationName(o);return "";}
 public string PantheonReason(int owner){var n=nations[owner];if(n.pantheon>=0)return "A pantheon is already chosen.";return n.faith<PantheonCost?"Needs "+PantheonCost+" faith ("+n.faith+" so far).":"";}
 public bool ChoosePantheon(int owner,int p){if(p<0||p>=ReligionCatalog.PantheonNames.Length||PantheonReason(owner)!="")return false;var n=nations[owner];n.faith-=PantheonCost;n.pantheon=p;if(owner==0)Note("Pantheon adopted: "+ReligionCatalog.PantheonNames[p]+".");return true;}
 public City HolyCityCandidate(int owner){return cities.Where(c=>c.owner==owner&&HasBuilding(c,36)&&!IsHolyCity(c)).OrderByDescending(c=>c.pop).ThenBy(c=>c.id).FirstOrDefault();}
 public bool ReligionTaken(int r){return nations.Any(n=>n.founded==r);}
 public string FoundingReason(int owner,int r){var n=nations[owner];if(n.founded>=0)return "You have already founded a faith.";if(n.pantheon<0)return "Adopt a pantheon first.";if(nations.Count(a=>a.founded>=0)>=MaxReligions())return "Every faith this world can hold has been founded.";if(!HasTech(owner,"mysticism"))return "Requires Mysticism.";if(r>=0){if(ReligionTaken(r))return ReligionCatalog.Name(r)+" has already been founded.";if(!HasTech(owner,ReligionCatalog.Tech(r)))return "Requires "+TechNames[TechnologyCatalog.Index(ReligionCatalog.Tech(r))]+".";}if(HolyCityCandidate(owner)==null)return "Needs a city with a Temple to be the holy city.";return n.faith<FoundingCost()?"Needs "+FoundingCost()+" faith ("+n.faith+" so far).":"";}
 public bool FoundReligion(int owner,int r){if(r<0||r>=ReligionCatalog.Count||FoundingReason(owner,r)!="")return false;var n=nations[owner];var holy=HolyCityCandidate(owner);n.faith-=FoundingCost();n.founded=r;n.holyCity=holy.id;holy.faithPressure[r]+=300;holy.religion=r;AddEraScore(owner,3,ReligionCatalog.Name(r)+" founded");
  if(owner==0)Note(ReligionCatalog.Name(r)+" founded, with "+holy.name+" as its holy city.");else if(KnowsNation(owner))Note(NationName(owner)+" founded "+ReligionCatalog.Name(r)+".");return true;}
 bool KnowsNation(int owner){return met!=null&&owner<met.Length&&met[owner];}
 // Missionaries carry the founder's faith to a city within 12 tiles of one of its following cities (+150 pressure).
 public string MissionReason(int owner,City target){var n=nations[owner];if(n.founded<0)return "Found a faith first.";if(target==null)return "No city chosen.";if(owner==0&&!At(target.x,target.y).seen)return "That city is unknown.";if(!cities.Any(c=>c.owner==owner&&c.religion==n.founded&&Distance(c.x,c.y,target.x,target.y)<=12))return "Too far from your faithful cities (12 tiles).";return n.faith<MissionCost?"Needs "+MissionCost+" faith.":"";}
 public bool SendMissionaries(int owner,City target){if(MissionReason(owner,target)!="")return false;var n=nations[owner];n.faith-=MissionCost;target.faithPressure[n.founded]+=150;UpdateMajority(target,owner==0);if(target.owner==0&&owner!=0)Note(NationName(owner)+" sent missionaries of "+ReligionCatalog.Name(n.founded)+" to "+target.name+".");return true;}
 void UpdateMajority(City c,bool tell){if(c.faithPressure==null||c.faithPressure.Length!=ReligionCatalog.Count)c.faithPressure=Grow(c.faithPressure??new int[0],ReligionCatalog.Count);int best=-1,top=59;for(int r=0;r<ReligionCatalog.Count;r++)if(c.faithPressure[r]>top){top=c.faithPressure[r];best=r;}if(best!=c.religion){if(c.owner==0&&(tell||best>=0))Note(c.name+(best>=0?" now follows "+ReligionCatalog.Name(best)+".":" no longer has a majority faith."));c.religion=best;if(best>=0)for(int o=0;o<civCount;o++)if(nations[o].founded==best&&!(nations[o].holyCity==c.id))AddEraScore(o,2,c.name+" converted to "+ReligionCatalog.Name(best));}}
 // Once a turn: faith is gathered, rivals act, then every city presses its majority faith on cities within ten tiles.
 public void UpdateReligion(){
  for(int o=0;o<civCount;o++){nations[o].faith+=FaithPerTurn(o);if(o>0)ReligionAI(o);}
  var adds=new Dictionary<City,int[]>();
  foreach(var s in cities){if(s.religion<0)continue;int r=s.religion,power=(IsHolyCity(s)&&nations.Any(n=>n.holyCity==s.id&&n.founded==r)?8:4)+(HasBuilding(s,36)?2:0);if(ReligionCatalog.Tenet(r)==6)power=power*3/2;power=power*CivPressure(s.owner)/100;
   foreach(var t in cities){int d=Distance(s.x,s.y,t.x,t.y);if(d>10)continue;int gain=t==s?power:Math.Max(1,power*(11-d)/10);int[] a;if(!adds.TryGetValue(t,out a)){a=new int[ReligionCatalog.Count];adds[t]=a;}a[r]+=gain;}}
  foreach(var c in cities){if(c.faithPressure==null||c.faithPressure.Length!=ReligionCatalog.Count)c.faithPressure=Grow(c.faithPressure??new int[0],ReligionCatalog.Count);int[] a;adds.TryGetValue(c,out a);for(int r=0;r<ReligionCatalog.Count;r++){c.faithPressure[r]-=c.faithPressure[r]/50;if(a!=null)c.faithPressure[r]=Math.Min(3000,c.faithPressure[r]+a[r]);}UpdateMajority(c,false);}
 }
 void ReligionAI(int owner){var n=nations[owner];var r=new Random(seed+turn*131+owner*17);
  if(n.pantheon<0&&n.faith>=PantheonCost)ChoosePantheon(owner,r.Next(ReligionCatalog.PantheonNames.Length));
  if(n.founded<0&&n.pantheon>=0){int wanted=PreferredFaith(owner);var options=Enumerable.Range(0,ReligionCatalog.Count).Where(f=>FoundingReason(owner,f)=="").ToList();if(wanted>=0)FoundReligion(owner,wanted);else if(options.Count>0)FoundReligion(owner,options[r.Next(options.Count)]);}
  PriestBuyingAI(owner);
  if(n.founded>=0&&n.faith>=MissionCost+10){var home=cities.Where(c=>c.owner==owner&&c.religion==n.founded).ToList();var target=cities.Where(c=>c.religion!=n.founded&&home.Any(h=>Distance(h.x,h.y,c.x,c.y)<=12)).OrderBy(c=>c.owner==owner?0:1).ThenBy(c=>home.Min(h=>Distance(h.x,h.y,c.x,c.y))).FirstOrDefault();if(target!=null)SendMissionaries(owner,target);}
 }
 bool ReligionValid(){if(nations.Any(n=>n.faith<0||n.pantheon< -1||n.pantheon>=ReligionCatalog.PantheonNames.Length||n.founded< -1||n.founded>=ReligionCatalog.Count))return false;var founded=nations.Where(n=>n.founded>=0).Select(n=>n.founded).ToList();if(founded.Distinct().Count()!=founded.Count)return false;
  return cities.All(c=>c.faithPressure!=null&&c.faithPressure.Length==ReligionCatalog.Count&&c.faithPressure.All(v=>v>=0)&&c.religion>=-1&&c.religion<ReligionCatalog.Count)&&tiles.All(t=>t.swayCiv>=-1&&t.swayCiv<civCount&&t.sway>=0&&t.sway<=100);}
 void UpgradeReligion(){foreach(var n in nations){n.faith=0;n.pantheon=-1;n.founded=-1;n.holyCity=-1;}foreach(var c in cities){c.faithPressure=new int[ReligionCatalog.Count];c.religion=-1;}foreach(var t in tiles){t.swayCiv=-1;t.sway=0;}}
}
}
namespace ParadigmRealm {
// Border influence (owner request, 31 Oct 2026). Cities of one civilisation share a single territory, so their borders merge.
// Where two civilisations' borders meet, each tile in reach of both feels the culture and piety of the nearby cities
// (CityInfluence, weakened with distance). When a rival's influence is stronger, the tile slowly swings towards it, a few
// percent a turn; at 90% it changes hands and joins that civilisation's territory. If the owner is stronger again the swing
// fades. Tiles next to a city never change hands.
public partial class World {
 public int TerritoryRadius(City c){return Math.Max(Math.Min(4,2+c.age/25),WorkRadius(c));}
 public int CityInfluence(City c){return 2+c.pop/2+(HasBuilding(c,36)?2:0)+(HasBuilding(c,42)?1:0)+c.buildings.Count(IsWonder)*2+FaithYield(c)+(c.religion>=0&&c.religion==nations[c.owner].founded?1:0)+CivInfluence(c);}
 public void SpreadInfluence(){
  int n=tiles.Length;var own=new float[n];var near=new bool[n];var rivals=new Dictionary<int,Dictionary<int,float>>();
  foreach(var c in cities){float s=CityInfluence(c);int radius=TerritoryRadius(c);
   for(int dy=-radius;dy<=radius;dy++)for(int dx=-radius;dx<=radius;dx++){var t=Step(c.x,c.y,dx,dy);if(t==null)continue;int d=Distance(t.x,t.y,c.x,c.y);if(d>radius)continue;int i=t.y*Width+t.x;if(d<=1){near[i]=true;continue;}float p=s/(1+d);
    if(t.owner==c.owner)own[i]+=p;else{Dictionary<int,float> m;if(!rivals.TryGetValue(i,out m)){m=new Dictionary<int,float>();rivals[i]=m;}float was;m.TryGetValue(c.owner,out was);m[c.owner]=was+p;}}}
  bool changed=false;int lost=0,gained=0;string taker="";
  for(int i=0;i<n;i++){var t=tiles[i];Dictionary<int,float> m;if(t.owner<0||near[i]||!rivals.TryGetValue(i,out m)){if(t.sway>0){t.sway=Math.Max(0,t.sway-3);if(t.sway==0)t.swayCiv=-1;}continue;}
   var best=m.OrderByDescending(kv=>kv.Value).First();float pa=own[i],pb=best.Value;
   if(pb>pa){if(t.swayCiv!=best.Key){t.swayCiv=best.Key;t.sway=0;}t.sway=Math.Min(100,t.sway+Math.Max(2,Math.Min(6,(int)Math.Round(12*(pb-pa)/(pb+pa)))));
    if(t.sway>=90){int old=t.owner;t.owner=best.Key;t.swayCiv=-1;t.sway=0;changed=true;if(old==0){lost++;taker=NationName(best.Key);}if(best.Key==0)gained++;}}
   else if(t.sway>0){t.sway=Math.Max(0,t.sway-3);if(t.sway==0)t.swayCiv=-1;}}
  if(lost>0)Note(lost+(lost==1?" border tile":" border tiles")+" passed to "+taker+", whose culture and faith there are stronger.");
  if(gained>0)Note(gained+(gained==1?" border tile":" border tiles")+" joined your territory, drawn by your culture and faith.");
  if(changed)networks=null;
 }
}
}
