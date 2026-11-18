using System;
using System.Linq;
namespace ParadigmRealm {
// The Zarevan Crown (owner request, 18 Nov 2026): a fantasy kingdom numbered after everyone else, so nothing else moves.
public static class FantasyKingdomChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Fantasy kingdoms: "+what);}
 public static void Run(){
  int z=FantasyKingdoms.First;Check(CivilisationCatalog.Count==z+FantasyKingdoms.People.Count&&CivilisationCatalog.Names[z]=="Zarevan Crown"&&CivilisationCatalog.Leaders[z]=="Sovereign Hamzah","the Zarevan Crown is civilisation "+z);
  Check(CivilisationCatalog.Names[84]==SpeciesPeoples.People[0].Name&&CivilisationCatalog.Names[17]=="China"&&CivilisationCatalog.Names[39]==SealedPeoples.People[0].Name,"no other civilisation changed number");
  Check(!CivilisationCatalog.IsReal(z)&&!CivilisationCatalog.IsNative(z)&&CivilisationCatalog.NativeRealm(z)==0,"it is a home-world fantasy kingdom");
  Check(CivilisationCatalog.Portraits[z]==z&&CivilisationCatalog.PortraitTitles[z]=="Sovereign of the Sun Throne"&&CivilisationCatalog.Greetings[z].Contains("Sun Throne"),"portrait, title and greeting");
  var e=CivLore.Of(z);Check(e!=null&&e.Unit=="Lion Guard"&&e.UnitBase==7&&e.Building=="Paradise Garden"&&e.BuildingBase==33&&e.Agenda==3&&e.History.Length>200,"full lore with its unique unit, building and agenda");
  Check(CivilisationCatalog.Traits[z].SequenceEqual(new[]{4,1}),"Monumental and Mercantile");
  // Played: its rules apply and its cities take its names.
  bool was=CivilisationCatalog.Enabled[z];CivilisationCatalog.Enabled[z]=true;
  try{
  var w=World.New(191,"Zarevan Crown",0,1,1,0,1,2,3,civChoice:z,hex:true,natives:false);
  Check(w.nations[0].civ==z&&w.civilisationName=="Zarevan Crown","a game can be started as the Zarevan Crown");
  var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));Check(c!=null&&c.name=="Zarevan","its capital is Zarevan");
  Check(w.CivContent(c)==1,"Paradise Gardens: +1 content");
  Check(Enumerable.Range(1,w.civCount-1).All(o=>w.nations[o].civ!=z),"no rival is a second Zarevan Crown");
  }finally{CivilisationCatalog.Enabled[z]=was;}
 }
}
}
