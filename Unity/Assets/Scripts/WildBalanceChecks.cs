using System;
using System.Linq;
namespace ParadigmRealm {
// Owner request 26 Nov 2026: barbarians and wild animals were far stronger than a civilisation's soldiers, a Warrior attacking a
// Megatherium barely hurt it, and the Megatherium is retired. A Warrior on open ground now wins or draws against every beast and
// matches a Dawn raider; a camp can be burned by Warriors.
public static class WildBalanceChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Wild balance: "+what);}
 public static void Run(){
  var w=World.New(181,"Balance",0,1,1,0,1,2,4,hex:true,natives:false,wild:true);w.barbarians=true;
  var city=w.Found(w.units.First(u=>u.owner==0&&u.type==0));foreach(var a in w.units.ToList())w.units.Remove(a);
  var open=w.tiles.First(t=>t.realm==0&&t.terrain==0&&t.owner<0&&t.gateLink==0&&w.Distance(t.x,t.y,city.x,city.y)>=12);
  var warrior=w.Add(0,2,open.x,open.y);
  for(int kind=0;kind<AnimalCatalog.Names.Length;kind++){
   int dealt,taken;w.AnimalExchange(warrior,new Animal{kind=kind,x=open.x,y=open.y},out dealt,out taken);
   Check(dealt>=2,"a Warrior hurts a "+AnimalCatalog.Names[kind]+" (deals "+dealt+")");
   Check(dealt>=taken,"a Warrior attacking a "+AnimalCatalog.Names[kind]+" deals at least what it takes ("+dealt+" against "+taken+")");
   int theirs=World.Damage(AnimalCatalog.Attack[kind],w.Strength(warrior,null,true)+AnimalCatalog.HuntBonus);
   Check(theirs<=4,"a "+AnimalCatalog.Names[kind]+" attacking a Warrior deals at most 4 ("+theirs+")");
  }
  {int d,t;w.AnimalExchange(warrior,new Animal{kind=AnimalCatalog.Megatherium},out d,out t);Check(d>=3,"a Warrior's blow on an old save's Megatherium does real damage ("+d+")");}
  w.animals.Add(new Animal{id=9100,kind=AnimalCatalog.Megatherium,x=open.x,y=open.y});w.RetireMegatheriums();Check(w.animals.All(a=>a.kind!=AnimalCatalog.Megatherium),"loading an older save removes its Megatheriums");
  // A Dawn raider is a Warrior's match; a horse raider strikes harder; Warriors can burn a camp.
  var raider=new Raider{x=open.x,y=open.y};var horse=new Raider{x=open.x,y=open.y,mounted=1};
  Check(w.RaiderAttack(raider)==1&&w.RaiderAttack(horse)==2,"a Dawn raider attacks at 1, a horse raider at 2");
  {int d,t;w.RaiderExchange(warrior,raider,out d,out t);Check(d==t&&d==3,"a Warrior and a Dawn raider trade even blows ("+d+" against "+t+")");}
  {int d,t;var camp=new BarbarianCamp{x=open.x,y=open.y};w.CampExchange(warrior,camp,out d,out t);Check(d>=2&&t<=4,"Warriors can burn a Dawn camp (deal "+d+", take "+t+")");}
 }
}
}
