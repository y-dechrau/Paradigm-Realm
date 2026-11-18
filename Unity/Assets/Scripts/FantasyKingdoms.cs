using System.Collections.Generic;
namespace ParadigmRealm {
// Fantasy kingdoms added after the catalogue was full (owner request, 18 Nov 2026: the Zarevan Crown). They are numbered after
// the species civilisations (from First) so that no existing civilisation, portrait or save changes number, but they belong
// to the Fantasy Kingdoms group of the New Game screen and are home-world rivals like the Azure Crown or the Holy Imperium.
// They are written like the peoples of the other worlds (SealedPeoples.Make), so their rules are the plain PeopleRule numbers.
public static class FantasyKingdoms {
 public const int First=159;
 public static readonly List<SealedPerson> People=new List<SealedPerson>();
 public static SealedPerson Of(int civ){int i=civ-First;return i>=0&&i<People.Count?People[i]:null;}
 public static bool Is(int civ){return Of(civ)!=null;}
 static FantasyKingdoms(){
  People.Add(SealedPeoples.Make(0,"Zarevan Crown","Sovereign Hamzah","Sovereign of the Sun Throne",
   "Hamzah, Sovereign of the Sun Throne, greets you. The sun rises first over the gardens of Zarevan; come as a friend and you will share its light.",
   0,30,4,1,3,
   new[]{"Zarevan","Shamsira","Anarbad","Azarvin","Golrida","Khorvand","Narenjan","Firuzkand","Tavrin","Rudhaven","Sunhold","Kesh-e-Nur","Mirasht","Delavan","Toranbad","Sepidar"},
   "Keepers of the Sun Throne",
   "A kingdom of terraced garden cities climbing the foothills of a great mountain range. Turquoise domes rise over cypress walks, rose beds and tiled fountains fed by channels cut deep under the mountains; every city is built around a garden, and every garden around water. Its banner is the golden lion beneath the rising sun.",
   "The first Zarevan were mountain clans who learned to carry snowmelt to the plains through long tunnels under the earth, and the engineer who joined their channels into one became the first to sit on the Sun Throne. Their golden age came when the water reached the dry valleys: orchards, rose gardens and walled paradises spread from city to city, and poets, tile-makers and architects flocked to the court. Then came the Year Without Sun, when ash from a far volcano darkened the sky, the harvests failed and a usurper seized the capital; the garden-keepers hid the royal heir among the channel-diggers until the lion banner rose again. Sovereign Hamzah rules from a throne guarded by golden lions and means to see a garden city flourish under every sun in the world.",
   "Content, well-fed cities; rich trade from its markets of silk and dye; wonders built faster than anyone's; disciplined Lion Guards.",
   "Its lavish court and endless building drain every city's workshops.",
   "Paradise Gardens",new PeopleRule{Food=1,Content=1},
   "Lion Guard",7,1,1,0,
   "Paradise Garden",33,new PeopleRule{Content=1,Gold=2},
   "Lavish Court",new PeopleRule{Production=-1}));
 }
}
}
