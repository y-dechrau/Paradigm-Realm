using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Every map resource in one data-driven registry (owner's resource pack, 16 Nov 2026; Design/RESOURCES-2026-11-16.md).
// A resource is saved as its number (Tile.resource), so numbers never change: 1-19 are the resources the game had before
// (rules and approved art unchanged), 20 onwards come from the owner's catalogue, appended in catalogue order. Each also has
// a stable lower-case id for data and tooling. World's resource tables (names, yields, technology, kind) are built from here.
// Resources whose draft art failed review (Art.NeedsRedraw) are registered but never placed until their art is redrawn.
public static class ResourceRegistry {
 public enum Place{Legacy,Land,Coast,Sea,Realm}
 public enum Develop{Farm,Mine,Sea}
 public enum Art{Approved,Draft,NeedsRedraw}
 // What a deposit needs beside it: fresh water (a river or lake tile, or one beside it) or the sea.
 public enum Need{None,FreshWater,Coast}
 public const int Food=0,Luxury=1,Strategic=2;
 static readonly int[] None=new int[0];
 public sealed class Def{
  public readonly int number,kind,food,shields,trade,strategicPercent,weight;public readonly string id,name,tech,artNote;public readonly int[] terrains;
  public readonly Place place;public readonly Develop develop;public readonly float scale;public readonly Art art;
  // Where it occurs on Earth (16 Nov 2026 research, Design/RESOURCES-2026-11-16.md): absolute latitude 0 (equator) to 1 (pole).
  public readonly float latMin,latMax;public readonly Need need;
  public Def(int number,string id,string name,int kind,int food,int shields,int trade,int strategicPercent,string tech,int[] terrains,int weight,Place place,Develop develop,float scale,Art art,string artNote,float latMin=0,float latMax=1,Need need=Need.None){
   this.latMin=latMin;this.latMax=latMax;this.need=need;
   this.number=number;this.id=id;this.name=name;this.kind=kind;this.food=food;this.shields=shields;this.trade=trade;this.strategicPercent=strategicPercent;this.tech=tech;this.terrains=terrains;
   this.weight=art==Art.NeedsRedraw?0:weight;this.place=place;this.develop=develop;this.scale=scale;this.art=art;this.artNote=artNote;}
  // Placed by the catalogue pass in new worlds (the first nineteen keep their own placement).
  public bool Spawns{get{return weight>0&&place!=Place.Legacy;}}
 }
 // Number, id, name, kind, food, shields, trade, strategic %, technology, terrains, weight, place, develop, sprite scale, art, note,
 // latitude band, what it needs beside it.
 public static readonly Def[] All={
   new Def(1,"wheat","Wheat",0,2,0,0,0,"agriculture",None,0,Place.Legacy,Develop.Farm,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(2,"iron","Iron",2,0,2,0,20,"iron",None,0,Place.Legacy,Develop.Mine,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(3,"gold","Gold",1,0,0,2,0,"bronze",None,0,Place.Legacy,Develop.Mine,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(4,"horses","Horses",2,0,0,0,10,"horseback",None,0,Place.Legacy,Develop.Farm,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(5,"spices","Spices",1,0,0,0,0,"currency",None,0,Place.Legacy,Develop.Farm,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(6,"fish","Fish",0,2,0,0,0,"",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(7,"whales","Whales",1,1,1,1,0,"sailing",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(8,"pearls","Pearls",1,0,0,3,0,"sailing",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(9,"crabs","Crabs",0,1,0,1,0,"",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(10,"kelp","Kelp",0,1,1,0,0,"",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(11,"oysters","Oysters",1,1,0,2,0,"",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(12,"seals","Seals",0,1,1,0,0,"sailing",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(13,"tuna","Tuna",0,2,0,1,0,"sailing",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(14,"sponges","Sponges",1,0,0,2,0,"sailing",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(15,"offshore_oil","Offshore oil",2,0,0,0,25,"refining",None,0,Place.Legacy,Develop.Sea,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(16,"sheep","Sheep",0,1,0,1,0,"m_herding",None,0,Place.Legacy,Develop.Farm,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(17,"cattle","Cattle",0,2,1,0,0,"m_herding",None,0,Place.Legacy,Develop.Farm,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(18,"pigs","Pigs",0,2,0,0,0,"m_herding",None,0,Place.Legacy,Develop.Farm,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(19,"camels","Camels",2,0,1,1,10,"horseback",None,0,Place.Legacy,Develop.Farm,1f,Art.Approved,"",0f,1f,Need.None),
   new Def(20,"rice","Rice",0,2,0,0,0,"agriculture",new[]{7,9,0,19,11,16},5,Place.Land,Develop.Farm,1f,Art.Draft,"pack 002",0.00f,0.50f,Need.FreshWater),
   new Def(21,"maize","Maize",0,2,0,0,0,"agriculture",new[]{0,16,2,1,7},5,Place.Land,Develop.Farm,1f,Art.Draft,"pack 003",0.00f,0.50f,Need.None),
   new Def(22,"barley","Barley",0,2,0,0,0,"agriculture",new[]{0,20,2,7,3},5,Place.Land,Develop.Farm,1f,Art.Draft,"pack 004",0.20f,0.75f,Need.None),
   new Def(23,"sorghum","Sorghum",0,2,0,0,0,"agriculture",new[]{16,20,0,3},4,Place.Land,Develop.Farm,1f,Art.Draft,"pack 005",0.00f,0.40f,Need.None),
   new Def(24,"potatoes","Potatoes",0,2,0,0,0,"agriculture",new[]{2,0,1,13},4,Place.Land,Develop.Farm,0.95f,Art.Draft,"pack 006",0.05f,0.72f,Need.None),
   new Def(25,"oats","Oats",0,2,0,0,0,"agriculture",new[]{0,1,2,9,13},4,Place.Land,Develop.Farm,1f,Art.Draft,"pack 007",0.45f,0.72f,Need.None),
   new Def(26,"rye","Rye",0,2,0,0,0,"agriculture",new[]{0,1,13,20,2},4,Place.Land,Develop.Farm,1f,Art.Draft,"pack 008",0.45f,0.72f,Need.None),
   new Def(27,"bananas","Bananas",0,1,0,1,0,"agriculture",new[]{11,16,7,19},4,Place.Land,Develop.Farm,1.1f,Art.Draft,"pack 009",0.00f,0.30f,Need.FreshWater),
   new Def(28,"citrus","Citrus",0,1,0,1,0,"agriculture",new[]{0,2,7,16},3,Place.Land,Develop.Farm,1.1f,Art.Draft,"pack 010",0.10f,0.45f,Need.None),
   new Def(29,"dates","Dates",0,1,0,1,0,"agriculture",new[]{3,7},3,Place.Land,Develop.Farm,1.1f,Art.Draft,"pack 011",0.17f,0.38f,Need.FreshWater),
   new Def(30,"mangoes","Mangoes",0,1,0,1,0,"agriculture",new[]{16,11,0},3,Place.Land,Develop.Farm,1.1f,Art.Draft,"pack 012",0.00f,0.30f,Need.None),
   new Def(31,"cocoa","Cocoa",1,0,0,2,0,"agriculture",new[]{11},2,Place.Land,Develop.Farm,1.1f,Art.Draft,"pack 013",0.00f,0.17f,Need.None),
   new Def(32,"coconuts","Coconuts",0,1,0,1,0,"agriculture",new[]{19,11,16},4,Place.Land,Develop.Farm,1.1f,Art.Draft,"pack 014",0.00f,0.26f,Need.Coast),
   new Def(33,"sugar_cane","Sugar Cane",1,0,0,2,0,"agriculture",new[]{11,16,7,0,19},4,Place.Land,Develop.Farm,1f,Art.Draft,"pack 015",0.00f,0.40f,Need.FreshWater),
   new Def(34,"tea","Tea",1,0,0,2,0,"agriculture",new[]{2,11,1},3,Place.Land,Develop.Farm,0.95f,Art.Draft,"pack 016",0.15f,0.40f,Need.None),
   new Def(35,"coffee","Coffee",1,0,0,2,0,"agriculture",new[]{2,11,17},3,Place.Land,Develop.Farm,1f,Art.Draft,"pack 017",0.00f,0.27f,Need.None),
   new Def(36,"grapes","Grapes",1,0,0,2,0,"agriculture",new[]{2,0,7,1},3,Place.Land,Develop.Farm,1f,Art.Draft,"pack 018",0.33f,0.56f,Need.None),
   new Def(37,"saffron","Saffron",1,0,0,2,0,"agriculture",new[]{20,2,3},1,Place.Land,Develop.Farm,0.85f,Art.Draft,"pack 020",0.36f,0.45f,Need.None),
   new Def(38,"sage","Sage",1,0,0,2,0,"agriculture",new[]{2,20,0},3,Place.Land,Develop.Farm,0.9f,Art.Draft,"pack 021",0.33f,0.50f,Need.None),
   new Def(39,"truffles","Truffles",1,0,0,2,0,"agriculture",new[]{1,2},1,Place.Land,Develop.Farm,0.85f,Art.Draft,"pack 022",0.43f,0.56f,Need.None),
   new Def(40,"goats","Goats",0,2,0,0,0,"m_herding",new[]{2,20,3,16},4,Place.Land,Develop.Farm,1f,Art.Draft,"pack 025",0.10f,0.55f,Need.None),
   new Def(41,"llamas","Llamas",0,1,1,0,0,"m_herding",new[]{2,20,3},2,Place.Land,Develop.Farm,1f,Art.Draft,"pack 029",0.08f,0.35f,Need.None),
   new Def(42,"bison","Bison",0,2,0,0,0,"toolmaking",new[]{0,20,13,1},3,Place.Land,Develop.Farm,1f,Art.Draft,"pack 030",0.28f,0.70f,Need.None),
   new Def(43,"deer","Deer",0,2,0,0,0,"toolmaking",new[]{1,13,0,2,9,8},5,Place.Land,Develop.Farm,1f,Art.Draft,"pack 031",0.05f,0.80f,Need.None),
   new Def(44,"elephants","Elephants",2,0,2,0,10,"m_herding",new[]{16,11},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"halo",0f,1f,Need.None),
   new Def(45,"wild_game","Wild Game",0,2,0,0,0,"toolmaking",new[]{1,16,0,13,11,20},5,Place.Land,Develop.Farm,1f,Art.Draft,"pack 033",0.00f,0.80f,Need.None),
   new Def(46,"wool","Wool",1,0,0,2,0,"m_herding",new[]{2,0},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"halo",0f,1f,Need.None),
   new Def(47,"furs","Furs",1,0,0,2,0,"toolmaking",new[]{13,8},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"broken",0f,1f,Need.None),
   new Def(48,"hides","Hides",1,0,0,2,0,"m_herding",new[]{20,16,0,13,8},4,Place.Land,Develop.Farm,1f,Art.Draft,"pack 050",0.00f,0.85f,Need.None),
   new Def(49,"clams","Clams",0,1,0,1,0,"sailing",new[]{4,19,12},4,Place.Sea,Develop.Sea,0.9f,Art.Draft,"pack 036",0.00f,0.75f,Need.Coast),
   new Def(50,"turtles","Turtles",1,1,0,1,0,"sailing",new[]{4,12},0,Place.Sea,Develop.Sea,1f,Art.NeedsRedraw,"neighbour",0f,1f,Need.None),
   new Def(51,"cowrie_shells","Cowrie Shells",1,0,0,2,0,"sailing",new[]{4,12},0,Place.Sea,Develop.Sea,1f,Art.NeedsRedraw,"halo",0f,1f,Need.None),
   new Def(52,"ambergris","Ambergris",1,0,0,3,0,"sailing",new[]{4},1,Place.Sea,Develop.Sea,0.9f,Art.Draft,"pack 104",0.00f,0.60f,Need.Coast),
   new Def(53,"timber","Timber",2,0,2,0,10,"toolmaking",new[]{1,13,2,9},6,Place.Land,Develop.Farm,1f,Art.Draft,"pack 042",0.30f,0.75f,Need.None),
   new Def(54,"hardwood","Hardwood",2,0,2,0,10,"toolmaking",new[]{1,11,16,2},4,Place.Land,Develop.Farm,1f,Art.Draft,"pack 043",0.00f,0.60f,Need.None),
   new Def(55,"bamboo","Bamboo",2,0,2,0,5,"toolmaking",new[]{11,1,2,7},4,Place.Land,Develop.Farm,1f,Art.Draft,"pack 044",0.00f,0.45f,Need.None),
   new Def(56,"rubber","Rubber",2,0,2,0,15,"industrialisation",new[]{11,7},2,Place.Land,Develop.Farm,1.05f,Art.Draft,"pack 045",0.00f,0.12f,Need.None),
   new Def(57,"silk","Silk",1,0,0,2,0,"currency",new[]{1,11},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"halo",0f,1f,Need.None),
   new Def(58,"honey","Honey",1,1,0,1,0,"agriculture",new[]{1,0},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"halo",0f,1f,Need.None),
   new Def(59,"aromatic_resin","Aromatic Resin",1,0,0,2,0,"currency",new[]{3,20,2},2,Place.Land,Develop.Farm,1f,Art.Draft,"pack 059",0.10f,0.40f,Need.None),
   new Def(60,"ebony","Ebony",1,0,0,2,0,"toolmaking",new[]{11,16},2,Place.Land,Develop.Farm,1f,Art.Draft,"pack 103",0.00f,0.20f,Need.None),
   new Def(61,"cotton","Cotton",1,0,0,2,0,"agriculture",new[]{16,7},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"halo",0f,1f,Need.None),
   new Def(62,"flax","Flax",1,0,0,2,0,"agriculture",new[]{0},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"halo",0f,1f,Need.None),
   new Def(63,"papyrus","Papyrus",1,0,0,2,0,"agriculture",new[]{9,7},2,Place.Land,Develop.Farm,1f,Art.Draft,"pack 053",0.00f,0.35f,Need.FreshWater),
   new Def(64,"dyes","Dyes",1,0,0,2,0,"agriculture",new[]{11,16},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"halo",0f,1f,Need.None),
   new Def(65,"lavender","Lavender",1,0,0,2,0,"agriculture",new[]{0,2},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"halo",0f,1f,Need.None),
   new Def(66,"tobacco","Tobacco",1,0,0,2,0,"agriculture",new[]{0,16,11,2},3,Place.Land,Develop.Farm,1f,Art.Draft,"pack 056",0.00f,0.45f,Need.None),
   new Def(67,"quinine","Quinine",1,0,0,2,0,"agriculture",new[]{2,11},1,Place.Land,Develop.Farm,0.95f,Art.Draft,"pack 057",0.00f,0.22f,Need.None),
   new Def(68,"medicinal_herbs","Medicinal Herbs",1,0,0,2,0,"agriculture",new[]{1,0,2,11,20},4,Place.Land,Develop.Farm,0.95f,Art.Draft,"pack 058",0.00f,0.70f,Need.None),
   new Def(69,"stone","Stone",2,0,1,0,5,"mining",new[]{2,10,3,0,1},6,Place.Land,Develop.Mine,1f,Art.Draft,"pack 060",0.00f,0.90f,Need.None),
   new Def(70,"limestone","Limestone",2,0,1,0,5,"mining",new[]{2},0,Place.Land,Develop.Mine,1f,Art.NeedsRedraw,"text",0f,1f,Need.None),
   new Def(71,"marble","Marble",1,0,0,2,0,"mining",new[]{2},0,Place.Land,Develop.Mine,1f,Art.NeedsRedraw,"text",0f,1f,Need.None),
   new Def(72,"clay","Clay",2,0,1,0,5,"mining",new[]{7,9,0,1,16},5,Place.Land,Develop.Mine,1f,Art.Draft,"pack 063",0.00f,0.70f,Need.FreshWater),
   new Def(73,"gypsum","Gypsum",2,0,1,0,5,"mining",new[]{3,20,10,2},3,Place.Land,Develop.Mine,1f,Art.Draft,"pack 064",0.10f,0.55f,Need.None),
   new Def(74,"kaolin","Kaolin",2,0,1,0,5,"mining",new[]{2},0,Place.Land,Develop.Mine,1f,Art.NeedsRedraw,"text",0f,1f,Need.None),
   new Def(75,"sand","Sand",2,0,1,0,5,"mining",new[]{3,7,0,20},5,Place.Land,Develop.Mine,1f,Art.Draft,"pack 066",0.00f,0.80f,Need.None),
   new Def(76,"gravel","Gravel",2,0,1,0,5,"mining",new[]{7,2,8,20,13},5,Place.Land,Develop.Mine,1f,Art.Draft,"pack 067",0.00f,0.90f,Need.None),
   new Def(77,"granite","Granite",2,0,1,0,5,"mining",new[]{2,10,13,8},4,Place.Land,Develop.Mine,1f,Art.Draft,"pack 068",0.00f,0.90f,Need.None),
   new Def(78,"slate","Slate",2,0,1,0,5,"mining",new[]{2,1,13},3,Place.Land,Develop.Mine,1f,Art.Draft,"pack 069",0.40f,0.75f,Need.None),
   new Def(79,"copper","Copper",2,0,2,0,10,"bronze",new[]{2,3,10,17},4,Place.Land,Develop.Mine,1f,Art.Draft,"pack 070",0.00f,0.75f,Need.None),
   new Def(80,"tin","Tin",2,0,2,0,10,"bronze",new[]{2,10,7,11},2,Place.Land,Develop.Mine,1f,Art.Draft,"pack 071",0.00f,0.60f,Need.None),
   new Def(81,"lead","Lead",2,0,2,0,5,"bronze",new[]{2,10,1},3,Place.Land,Develop.Mine,1f,Art.Draft,"pack 073",0.25f,0.70f,Need.None),
   new Def(82,"zinc","Zinc",2,0,2,0,5,"bronze",new[]{2,10,1},3,Place.Land,Develop.Mine,1f,Art.Draft,"pack 074",0.20f,0.70f,Need.None),
   new Def(83,"nickel","Nickel",2,0,2,0,10,"electricity",new[]{2,13,10,11,8},2,Place.Land,Develop.Mine,1f,Art.Draft,"pack 075",0.00f,0.80f,Need.None),
   new Def(84,"aluminium_ore","Aluminium Ore",2,0,2,0,15,"electricity",new[]{16,11,2},3,Place.Land,Develop.Mine,1f,Art.Draft,"pack 076",0.00f,0.30f,Need.None),
   new Def(85,"silver","Silver",1,0,0,2,0,"bronze",new[]{2,10,3,17},2,Place.Land,Develop.Mine,1f,Art.Draft,"pack 078",0.10f,0.60f,Need.None),
   new Def(86,"platinum","Platinum",1,0,0,3,0,"chemistry",new[]{10,2,7,13},1,Place.Land,Develop.Mine,1f,Art.Draft,"pack 079",0.25f,0.80f,Need.None),
   new Def(87,"titanium","Titanium",2,0,2,0,15,"flight",new[]{3,0,16,2},2,Place.Land,Develop.Mine,1f,Art.Draft,"pack 080",0.00f,0.70f,Need.Coast),
   new Def(88,"lithium","Lithium",2,0,2,0,15,"computers",new[]{3,10,2},1,Place.Land,Develop.Mine,1f,Art.Draft,"pack 081",0.20f,0.55f,Need.None),
   new Def(89,"cobalt","Cobalt",2,0,2,0,10,"electricity",new[]{16,11,2,10},1,Place.Land,Develop.Mine,1f,Art.Draft,"pack 082",0.00f,0.70f,Need.None),
   new Def(90,"tungsten","Tungsten",2,0,2,0,15,"military_science",new[]{2,10},1,Place.Land,Develop.Mine,1f,Art.Draft,"pack 083",0.10f,0.60f,Need.None),
   new Def(91,"rare_earth_ore","Rare Earth Ore",2,0,2,0,15,"computers",new[]{20,3,10,2},1,Place.Land,Develop.Mine,1f,Art.Draft,"pack 084",0.20f,0.55f,Need.None),
   new Def(92,"coal","Coal",2,0,2,0,20,"industrialisation",new[]{1,9,0,20,13,2},4,Place.Land,Develop.Mine,1f,Art.Draft,"pack 085",0.20f,0.80f,Need.None),
   new Def(93,"oil","Oil",2,0,2,0,25,"chemistry",new[]{3,20,9,0,8,19},3,Place.Land,Develop.Mine,1f,Art.Draft,"pack 086",0.00f,0.80f,Need.None),
   new Def(94,"natural_gas","Natural Gas",2,0,2,0,20,"chemistry",new[]{3,20,8,9,0},3,Place.Land,Develop.Mine,1f,Art.Draft,"pack 087",0.00f,0.80f,Need.None),
   new Def(95,"uranium","Uranium",2,0,2,0,25,"fission",new[]{3,20,13,10,2},1,Place.Land,Develop.Mine,1f,Art.Draft,"pack 088",0.15f,0.65f,Need.None),
   new Def(96,"geothermal","Geothermal",2,0,2,0,15,"electricity",new[]{17},0,Place.Land,Develop.Mine,1f,Art.NeedsRedraw,"faint",0f,1f,Need.None),
   new Def(97,"salt","Salt",1,1,0,1,0,"toolmaking",new[]{3,20,10,0},4,Place.Land,Develop.Mine,1f,Art.Draft,"pack 090",0.10f,0.55f,Need.None),
   new Def(98,"saltpetre","Saltpetre",2,0,1,0,10,"military_science",new[]{3,10,0},2,Place.Land,Develop.Mine,1f,Art.Draft,"pack 091",0.15f,0.45f,Need.None),
   new Def(99,"sulphur","Sulphur",2,0,1,0,10,"chemistry",new[]{17,10,3},2,Place.Land,Develop.Mine,1f,Art.Draft,"pack 092",0.00f,0.70f,Need.None),
   new Def(100,"phosphates","Phosphates",2,0,1,0,5,"chemistry",new[]{3,0},0,Place.Land,Develop.Mine,1f,Art.NeedsRedraw,"text",0f,1f,Need.None),
   new Def(101,"mercury","Mercury",2,0,1,0,5,"education",new[]{17,2,10},1,Place.Land,Develop.Mine,1f,Art.Draft,"pack 094",0.10f,0.55f,Need.None),
   new Def(102,"diamonds","Diamonds",1,0,0,3,0,"bronze",new[]{10,16,3,13,8,7},1,Place.Land,Develop.Mine,0.95f,Art.Draft,"pack 095",0.10f,0.75f,Need.None),
   new Def(103,"emeralds","Emeralds",1,0,0,3,0,"bronze",new[]{2,11},1,Place.Land,Develop.Mine,0.95f,Art.Draft,"pack 096",0.00f,0.30f,Need.None),
   new Def(104,"rubies","Rubies",1,0,0,3,0,"bronze",new[]{2,11,1},1,Place.Land,Develop.Mine,0.95f,Art.Draft,"pack 097",0.05f,0.40f,Need.None),
   new Def(105,"jade","Jade",1,0,0,3,0,"bronze",new[]{2,11},0,Place.Land,Develop.Mine,1f,Art.NeedsRedraw,"clipped",0f,1f,Need.None),
   new Def(106,"lapis_lazuli","Lapis Lazuli",1,0,0,3,0,"bronze",new[]{2},0,Place.Land,Develop.Mine,1f,Art.NeedsRedraw,"clipped",0f,1f,Need.None),
   new Def(107,"obsidian","Obsidian",2,0,1,1,5,"toolmaking",new[]{17},0,Place.Land,Develop.Mine,1f,Art.NeedsRedraw,"clipped",0f,1f,Need.None),
   new Def(108,"amber","Amber",1,0,0,3,0,"bronze",new[]{1},0,Place.Land,Develop.Mine,1f,Art.NeedsRedraw,"clipped",0f,1f,Need.None),
   new Def(109,"incense","Incense",1,0,0,2,0,"currency",new[]{3,16},0,Place.Land,Develop.Farm,1f,Art.NeedsRedraw,"clipped",0f,1f,Need.None),
   new Def(110,"aether_crystals","Aether Crystals",2,0,1,2,10,"mysticism",None,4,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 105",0f,1f,Need.None),
   new Def(111,"voidstone","Voidstone",2,0,2,0,10,"mysticism",None,4,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 106",0f,1f,Need.None),
   new Def(112,"resonance_quartz","Resonance Quartz",1,0,0,3,0,"mysticism",None,3,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 107",0f,1f,Need.None),
   new Def(113,"phase_ore","Phase Ore",2,0,2,1,10,"mysticism",None,3,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 108",0f,1f,Need.None),
   new Def(114,"rift_amber","Rift Amber",1,0,0,3,0,"mysticism",None,3,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 109",0f,1f,Need.None),
   new Def(115,"chronite","Chronite",1,0,0,3,0,"mysticism",None,2,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 110",0f,1f,Need.None),
   new Def(116,"nullstone","Nullstone",2,0,2,0,10,"mysticism",None,3,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 111",0f,1f,Need.None),
   new Def(117,"stariron","Stariron",2,0,3,0,15,"mysticism",None,2,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 112",0f,1f,Need.None),
   new Def(118,"living_timber","Living Timber",2,0,2,0,10,"mysticism",None,0,Place.Realm,Develop.Farm,1f,Art.NeedsRedraw,"background",0f,1f,Need.None),
   new Def(119,"etherbloom","Etherbloom",0,2,0,1,0,"mysticism",None,3,Place.Realm,Develop.Farm,1f,Art.Draft,"pack 114",0f,1f,Need.None),
   new Def(120,"celestial_salt","Celestial Salt",1,1,0,2,0,"mysticism",None,3,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 115",0f,1f,Need.None),
   new Def(121,"abyssal_obsidian","Abyssal Obsidian",2,0,2,1,10,"mysticism",None,3,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 116",0f,1f,Need.None),
   new Def(122,"luminous_fungi","Luminous Fungi",0,2,0,0,0,"mysticism",None,4,Place.Realm,Develop.Farm,1f,Art.Draft,"pack 117",0f,1f,Need.None),
   new Def(123,"titanbone_fossils","Titanbone Fossils",1,0,0,2,0,"mysticism",None,2,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 118",0f,1f,Need.None),
   new Def(124,"psionic_crystals","Psionic Crystals",1,0,0,3,0,"mysticism",None,2,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 119",0f,1f,Need.None),
   new Def(125,"gravity_shards","Gravity Shards",2,0,2,1,10,"mysticism",None,2,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 120",0f,1f,Need.None),
   new Def(126,"worldroot_resin","Worldroot Resin",1,0,0,3,0,"mysticism",None,0,Place.Realm,Develop.Farm,1f,Art.NeedsRedraw,"background",0f,1f,Need.None),
   new Def(127,"gateheart_crystals","Gateheart Crystals",1,0,1,4,0,"mysticism",None,1,Place.Realm,Develop.Mine,1f,Art.Draft,"pack 122",0f,1f,Need.None),
 };
 public static readonly int Count=All.Length+1;// index 0 is "no resource"
 static T[] Column<T>(T none,Func<Def,T> pick){var a=new T[Count];a[0]=none;foreach(var d in All)a[d.number]=pick(d);return a;}
 public static readonly string[] Names=Column("None",d=>d.name),Ids=Column("",d=>d.id),Techs=Column("",d=>d.tech);
 public static readonly int[] FoodYield=Column(0,d=>d.food),ShieldYield=Column(0,d=>d.shields),TradeYield=Column(0,d=>d.trade),Kind=Column(-1,d=>d.kind),StrategicPercent=Column(0,d=>d.strategicPercent);
 public static readonly bool[] Sea=Column(false,d=>d.develop==Develop.Sea),Mined=Column(false,d=>d.develop==Develop.Mine),HasArt=Column(false,d=>d.art!=Art.NeedsRedraw);
 public static readonly float[] Scale=Column(1f,d=>d.scale);
 static readonly Dictionary<string,int> byId=All.ToDictionary(d=>d.id,d=>d.number);
 public static int Number(string id){int n;return id!=null&&byId.TryGetValue(id,out n)?n:0;}
 public static Def Of(int number){return number>0&&number<Count?All[number-1]:null;}
 // Problems with the table itself (duplicate ids or numbers, a gap, a resource placed without art or terrain); empty when sound.
 public static List<string> Problems(){
  var p=new List<string>();var ids=new HashSet<string>();
  for(int i=0;i<All.Length;i++){var d=All[i];if(d.number!=i+1)p.Add(d.id+": number "+d.number+" out of order");if(string.IsNullOrEmpty(d.id)||d.id!=d.id.ToLowerInvariant()||d.id.Contains(" "))p.Add("bad id '"+d.id+"'");if(!ids.Add(d.id))p.Add("duplicate id "+d.id);
   if(d.kind<0||d.kind>2)p.Add(d.id+": no kind");if(d.Spawns&&d.place!=Place.Realm&&d.place!=Place.Sea&&d.terrains.Length==0)p.Add(d.id+": placed with no terrain");if(d.Spawns&&d.art==Art.NeedsRedraw)p.Add(d.id+": placed without usable art");}
  return p;}
}
}