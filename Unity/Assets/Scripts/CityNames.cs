using System;
using System.Linq;
namespace ParadigmRealm {
// Every city in a world has its own name (owner request). Each premade civilisation (CivilisationCatalog order) draws from its
// own list in order; when that runs out, or for custom and stock civilisations, names come from a shared pool, then from
// "New ..." forms of names already used. The player is offered the next name and may type another when founding.
public partial class World {
 static readonly string[][] CivCityNames={
  new[]{"Azurehold","Kingsport","Bluewater","Crownmere","Seawatch","Gullhaven","Tidecross","Saltmarsh","Stormquay","Brightcliff","Wavecrest","Harbourne"},
  new[]{"Qasr al-Nur","Zahirah","Palmwell","Sandgate","Mirajah","Oasis Spring","Duneharbour","Saffronia","Jinnwater","Amberdune","Khaled's Rest","Sultan's Well"},
  new[]{"Elderglade","Moonwood","Silverleaf","Thornbower","Mistvale","Fernhollow","Willowmere","Starbloom","Oakenheart","Dewbrook","Ivythorne","Lanternwood"},
  new[]{"Sanctum Aurea","Basilica","Gloria","Holyreach","Cathedra","Seraphim","Templum","Divinia","Cruxford","Laudis","Altaris","Vespera"},
  new[]{"Kanto Ba","Goldriver","Niani Sun","Sahelia","Baobab Hill","Djenara","Kora Falls","Saltroad","Mansa's Rest","Timbara","Riverbend","Griot's Gate"},
  new[]{"Kleonia","Thalassa","Olympeion","Argyros","Delphira","Korinthea","Ithakos","Marathos","Pelion","Lykaion","Agora Nova","Kallistos"},
  new[]{"Nyxspire","Shardfall","Voidcrest","Prismgate","Glimmerdeep","Amethia","Crystalis","Shimmerhold","Duskfacet","Geodeum","Lumenreach","Obscura"},
  new[]{"Frostpeak","Snowhelm","Icewater","Brynjaheim","Winterhold","Glacierford","Rimehall","Coldharbour","Whitecrest","Hoarfell","Skaldvik","Northwatch"},
  new[]{"Aurelia","Novum Forum","Senatus","Capitolia","Via Aurea","Concordia","Lucentum","Valeria","Portus Magnus","Fortuna","Civitas Nova","Ostia Aurelia"},
  new[]{"Lutetia","Belle Rive","Fleurmont","Lucienne","Sainte-Lumiere","Montclair","Rivedor","Beauval","Chateau Noir","Vermeil","Clairbourg","Roseval"},
  new[]{"Kaito Bay","Sakura Isle","Shiraumi","Tsukihama","Kazegawa","Ishigaki Point","Hoshizora","Akashima","Nagihama","Ryugu","Matsuoka","Kumokura"},
  new[]{"Temur's Camp","Grassyurt","Windplain","Khanbalik","Horsewater","Blue Sky Ordu","Felt Hall","Saddlecross","Eagle Steppe","Arrowford","Tengri Rise","Long Grass"},
  new[]{"Northmarch","Halvardsby","Stonewatch","Ravenshold","Greyford","Ironmoor","Wolfden","Highmarch","Fellgate","Thornwick","Kingsbarrow","Cragmere"},
  new[]{"Sunspire","Rashidabad","Goldminaret","Dawnspire","Lanternsouk","Brasswell","Sunhaven","Zephyria","Minaret Hill","Crescent Bay","Topaz Gate","Silkhaven"},
  new[]{"Lionmarch","Aldousgate","Pridehold","Goldmane","Roarwick","Lionsford","Manehaven","Sunlion","Valiant Keep","Heraldry","Crestfield","Brightmane"},
  new[]{"Jade Palace","Meiling","Lotus Gate","Silkriver","Pearlharbour","Dragonwell","Bamboo Grove","Celadon","Moon Terrace","Phoenix Hill","Lantern City","Plum Blossom"},
  new[]{"Itzelan","Serpent Mound","Quetzalpan","Jaguar Rock","Obsidian Steps","Feather Lake","Sunstone","Cenote Deep","Maizefield","Skypyramid","Coatlan","Tlalocan"},
  // Real civilisations (owner request, 31 Oct 2026): historical cities, capital first.
  new[]{"Xianyang","Chang'an","Luoyang","Kaifeng","Nanjing","Hangzhou","Beijing","Chengdu","Guangzhou","Suzhou","Yangzhou","Handan"},
  new[]{"Thebes","Memphis","Pi-Ramesses","Heliopolis","Abydos","Akhetaten","Elephantine","Avaris","Tanis","Sais","Hermopolis","Buto"},
  new[]{"Roma","Ostia","Capua","Ravenna","Mediolanum","Neapolis","Londinium","Lugdunum","Tarraco","Carthago Nova","Aquileia","Antiochia"},
  new[]{"Athens","Sparta","Corinth","Argos","Delphi","Olympia","Miletus","Syracuse","Rhodes","Megara","Ephesus","Eretria"},
  new[]{"Tenochtitlan","Texcoco","Tlacopan","Tlatelolco","Cholula","Xochimilco","Coyoacan","Azcapotzalco","Tula","Teotihuacan","Malinalco","Chalco"},
  new[]{"Cusco","Machu Picchu","Quito","Ollantaytambo","Pisac","Cajamarca","Vilcabamba","Tumebamba","Huanuco Pampa","Vitcos","Choquequirao","Hatun Xauxa"},
  new[]{"Lakamha","Mutal","Calakmul","Copan","Chichen Itza","Uxmal","Mayapan","Caracol","Yaxchilan","Piedras Negras","Tulum","Coba"},
  new[]{"Aksum","Adulis","Lalibela","Gondar","Addis Ababa","Harar","Adwa","Debre Berhan","Magdala","Yeha","Ankober","Dessie"},
  new[]{"Baghdad","Mecca","Medina","Damascus","Basra","Kufa","Samarra","Raqqa","Mosul","Aleppo","Sana'a","Muscat"},
  new[]{"Pasargadae","Persepolis","Susa","Ecbatana","Sardis","Bactra","Ctesiphon","Isfahan","Shiraz","Tabriz","Nishapur","Merv"},
  new[]{"Pataliputra","Ujjain","Taxila","Varanasi","Sanchi","Mathura","Kaushambi","Tosali","Suvarnagiri","Ayodhya","Kanchipuram","Vidisha"},
  new[]{"Babylon","Kish","Nippur","Sippar","Borsippa","Larsa","Ur","Uruk","Eridu","Lagash","Isin","Mari"},
  // Native peoples of the parallel worlds.
  new[]{"Silverbough","Moonglade","Elarion","Thistlewood","Starfall Bower","Lirien","Dawnspring","Willowhall","Aelmere","Fernsong","Glimmerdell","Eventide"},
  new[]{"High Aerie","Cloudnest","Stormperch","Windspire","Skyreach","Featherfall","Talon Rock","Gale Roost","Updraft","Eyrie Crown","Zephyr Ledge","Thunderwing"},
  new[]{"Node Prime","Foundry Alpha","Core 7","Relay Theta","Gearhold","Array Sigma","Unit Cradle","Logic Vault","Spindle","Cog Nexus","Circuit Deep","Iron Lattice"},
  new[]{"Ashforge","Stonedeep","Emberhold","Cinderdeep","Grimstone","Hammerfall","Coalgate","Anvilhome","Deepdelve","Slagmount","Runehall","Ironvein"},
  new[]{"Throne of Cinders","Brimstone Gate","Malgoroth's Maw","Pyre Citadel","Ashen Spire","Searing Pit","Obsidian Keep","Hellmouth","Scorchreach","Flayed Tower","Sulphur Court","Wrathfire"},
  new[]{"Gorefort","Bloodtusk Camp","Skullpile","Grukka's Hold","Bonecrag","Ironjaw","Redmaw","Smashgate","Wolfden Rock","Gutrot","Spikewall","Burnt Tusk"},
  new[]{"Gnawhole","Rotden","Skritt's Nest","Burrowmire","Plaguewarren","Squeakvault","Gnawgate","Bilepit","Tunnelrot","Scurryhold","Verminhole","Ratspire"},
  new[]{"Sporeheart","Mycel Ring","Capgrove","Gillhollow","Rootmeld","Moldmere","Puffball Rise","Hyphae Deep","Lichenveil","Mossbloom","Stalkhall","Fruiting Glade"},
  new[]{"Coral Throne","Tidehaven","Pearlspire","Kelpmoor","Wavecrown","Shellreach","Abyssal Gate","Seafoam Court","Brinewell","Reefdeep","Undertow","Nautilus Hall"},
  new[]{"Xoth's Pyramid","Sunscale","Coatlpan","Serpent Steps","Jadeclaw","Reef Temple","Venomgrove","Starscale","Old Sun","Basking Rock","Fangwater","Itzamal"}};
 static readonly string[] SharedCityNames={"Ashford","Brindlemoor","Cinderfall","Dunmere","Embervale","Farrowdale","Glenhallow","Hollowmere","Ironwood","Juniper Hill","Kestrel Point","Larkspur","Maplecross","Northwind","Oakridge","Pinehurst","Quarrystone","Redcliff","Stonebridge","Thistledown","Umberlee","Vinehold","Westmarch","Yarrowfield","Ambergate","Bramblewick","Coppervale","Deepwell","Eastmere","Foxhollow","Goldfurrow","Highcairn","Isleby","Jasper Ford","Knollwood","Lindenhall","Marrowby","Nettlebank","Orchard End","Pebblebrook","Riverwatch","Sablecrest","Tallowmere","Upperbrook","Valewood","Wrenfield","Yewbarrow","Zephyr Rise"};
 public string NewCityName(int owner){
  var used=new System.Collections.Generic.HashSet<string>(cities.Select(c=>c.name),StringComparer.OrdinalIgnoreCase);
  int civ=owner>=0&&nations!=null&&owner<nations.Length&&nations[owner]!=null?nations[owner].civ:-1;
  if(civ>=0&&civ<CivCityNames.Length)foreach(var n in CivCityNames[civ])if(!used.Contains(n))return n;
  // The shared pool starts at a different place for each civilisation so neighbours do not take alternate names.
  int start=Math.Abs(owner*7+seed)%SharedCityNames.Length;
  for(int k=0;k<SharedCityNames.Length;k++){var n=SharedCityNames[(start+k)%SharedCityNames.Length];if(!used.Contains(n))return n;}
  foreach(var c in cities.OrderBy(c=>c.id)){var n="New "+c.name;if(!used.Contains(n))return n;}
  int i=cities.Count+1;while(used.Contains("Settlement "+i))i++;return "Settlement "+i;
 }
 // A typed name is tidied and must not repeat another city's name.
 public string CityNameProblem(string name){if(string.IsNullOrWhiteSpace(name))return "Enter a name.";var clean=CleanName(name);return cities.Any(c=>string.Equals(c.name,clean,StringComparison.OrdinalIgnoreCase))?"Another city already has that name.":"";}
}
}
namespace ParadigmRealm {
public partial class World {
 // What a leader says when answering a proposal (owner request: the leader states whether they agree).
 public string LeaderReply(int leader,DiplomaticOffer o,bool accepted){
  var n=nations[leader];string who=string.IsNullOrEmpty(n.leader)?NationName(leader):n.leader;string deal=o.kind.ToString().ToLowerInvariant();
  string[] yes={"We agree. Let this "+deal+" bind our peoples for "+o.duration+" turns.","Accepted. "+NationName(leader)+" keeps its word.","These terms are fair. We have an agreement.","So be it. May the "+deal+" serve us both."};
  string[] no={"No. These terms do not serve "+NationName(leader)+".","We decline. Offer more, and we may listen.","Not today. Our council will not agree to this.","You ask too much and give too little. We refuse."};
  var pick=accepted?yes:no;return who+": \""+pick[Math.Abs(o.id*7+turn+leader)%pick.Length]+"\"";
 }
}
}
