using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Wild animals of the Ice Age (owner request, 6 Nov 2026), on the home world of new games. They belong to nobody. Carnivores hunt
// any civilisation's units they can sense; herbivores graze and strike only a unit standing right beside them, so one clear tile
// between you and a herbivore is safe. They never attack a unit inside a city, and they appear at random on open land away from
// everyone. They are kept in their own list, not as a nation, so they take no part in diplomacy or victory.
[Serializable] public class Animal {public int id,kind,x,y,hp=10;}
public static class AnimalCatalog {
 public static readonly string[] Names={"Cave lion","Smilodon","Cave bear","Dire wolf","Megatherium","Woolly mammoth","Woolly rhinoceros"};
 // Half the strength they had (owner request, 7 Nov 2026; was attack 3,4,3,2,2,4,5 and defence 2,2,3,1,4,5,4).
 public static readonly int[] Attack={2,2,2,1,1,2,3},Defence={1,1,2,1,2,3,2},Steps={3,3,2,3,1,2,2};
 public static readonly bool[] Carnivore={true,true,true,true,false,false,false};
 public const int Sense=7;
 // Where each is most at home; any open land will do at a pinch.
 public static readonly int[][] Homes={new[]{0,16,20},new[]{1,11,16},new[]{1,2,13},new[]{8,13,20},new[]{1,11,0},new[]{8,20,13},new[]{8,20,2}};
}
public partial class World {
 public bool wildlife;public List<Animal> animals=new List<Animal>();
 internal void SpawnAnimalsForChecks(){SpawnAnimals(new Random(seed+turn));}
 internal void TickAnimalsForChecks(bool spawn){if(!spawn)keepAnimalsFromSpawning=true;turn++;AnimalsTurn();keepAnimalsFromSpawning=false;}
 bool keepAnimalsFromSpawning;
 public Animal AnimalAt(int x,int y){if(animals==null)return null;foreach(var a in animals)if(a.x==x&&a.y==y)return a;return null;}
 bool AnimalGround(Tile t){return t!=null&&t.realm==0&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.gateLink==0&&!CityCentres().Contains(t.y*Width+t.x);}
 // Animals never step onto land inside any civilisation's borders (owner request, 7 Nov 2026).
 bool OpenToAnimal(Tile t){return AnimalGround(t)&&t.owner<0&&AnimalAt(t.x,t.y)==null&&!(preyCells!=null?preyCells.Contains(t.y*Width+t.x):units.Any(u=>u.x==t.x&&u.y==t.y));}
 // Units sorted into blocks of 8 x 8 tiles for the animals' turn, so a beast looks only at what is near it.
 [NonSerialized] Dictionary<int,List<Unit>> preyGrid;[NonSerialized] HashSet<int> preyCells;
 void IndexPrey(){preyGrid=new Dictionary<int,List<Unit>>();preyCells=new HashSet<int>();foreach(var u in units){int key=(u.y/8)*100000+u.x/8;List<Unit> list;if(!preyGrid.TryGetValue(key,out list)){list=new List<Unit>();preyGrid[key]=list;}list.Add(u);preyCells.Add(u.y*Width+u.x);}}
 IEnumerable<Unit> PreyNear(int x,int y){if(preyGrid==null){foreach(var u in units)yield return u;yield break;}int columns=(Width+7)/8;for(int r=y/8-1;r<=y/8+1;r++)for(int k=-1;k<=1;k++){int col=((x/8+k)%columns+columns)%columns;List<Unit> list;if(r>=0&&preyGrid.TryGetValue(r*100000+col,out list))foreach(var u in list)if(u.hp>0)yield return u;}}
 // How many may roam: one per hundred tiles of open land, more or fewer with the difficulty.
 // Until the first civilisation has passed the Medieval Age the wilds teem (owner request, 6 Nov 2026): three times as many,
 // arriving faster and from the very start. Animals never appear on land within any civilisation's borders.
 public const int LastWildEra=2,WildMultiple=3,HuntersFrom=8;
 public bool EarlyWilds(){foreach(var o in Enumerable.Range(0,civCount))if(!IsNativeNation(o)&&Era(o)>LastWildEra)return false;return true;}
 [NonSerialized] int openLandCount=-1;
 public int AnimalLimit(){if(!wildlife)return 0;if(openLandCount<0)openLandCount=tiles.Count(t=>t.realm==0&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.gateLink==0);return Math.Max(2,openLandCount/40*DifficultyTable.Animals[Level]/100*animalShare/100)*(EarlyWilds()?WildMultiple:1);}
 void SpawnAnimals(Random r){
  if(keepAnimalsFromSpawning)return;bool early=EarlyWilds();if(!early&&turn<8)return;int limit=AnimalLimit();
  for(int n=early?Math.Max(3,limit/30):Math.Max(1,limit/100);n>0;n--)if(animals.Count<limit&&(early||r.Next(3)==0))SpawnAnimal(r);
 }
 void SpawnAnimal(Random r){
  int kind=r.Next(AnimalCatalog.Names.Length);var homes=AnimalCatalog.Homes[kind];
  for(int attempt=0;attempt<40;attempt++){
   var t=tiles[r.Next(mapHeight*Width)];if(!OpenToAnimal(t)||t.owner>=0)continue;if(attempt<25&&!homes.Contains(t.terrain))continue;
   if(units.Any(u=>Distance(u.x,u.y,t.x,t.y)<6)||cities.Any(c=>Distance(c.x,c.y,t.x,t.y)<6))continue;
   animals.Add(new Animal{id=nextId++,kind=kind,x=t.x,y=t.y});
   // Dire wolves come in pairs.
   if(kind==3)for(int k=0;k<6;k++){var n=Step(t.x,t.y,HexOr(k,true),HexOr(k,false));if(OpenToAnimal(n)){animals.Add(new Animal{id=nextId++,kind=kind,x=n.x,y=n.y});break;}}
   return;
  }
 }
 int HexOr(int k,bool first){if(hexGrid)return first?HexDi[k]:HexDj[k];return first?new[]{1,0,-1,0,1,-1}[k]:new[]{0,1,0,-1,1,-1}[k];}
 // The strongest defender on the tile takes the blow, as in any battle. A unit in a city is safe, and so is one on water.
 Unit AnimalPrey(int x,int y){var t=At(x,y);if(t==null||IsWater(t.terrain)||t.owner>=0||cities.Any(c=>c.x==x&&c.y==y))return null;return units.Where(u=>u.x==x&&u.y==y).OrderByDescending(u=>Power[u.type]>0).ThenByDescending(u=>Strength(u,null,true)).ThenByDescending(u=>u.hp).ThenBy(u=>u.id).FirstOrDefault();}
 void AnimalAttacks(Animal a,Unit prey){
  int atk=AnimalCatalog.Attack[a.kind],def=Strength(prey,null,true);prey.hp-=Damage(atk,def);if(Power[prey.type]>0)a.hp-=Damage(def,atk);string name=AnimalCatalog.Names[a.kind];CancelOrder(prey);
  if(prey.owner==0)Note(prey.hp<=0?"A "+name+" killed your "+UnitName(prey)+".":"A "+name+" attacked your "+UnitName(prey)+".");
  if(prey.hp<=0){units.Remove(prey);DrownStranded();}else if(a.hp<=0){Victory(prey);if(prey.owner==0)Note("Your "+UnitName(prey)+" killed the "+name+".");}
 }
 Unit NearestPrey(Animal a,int range){Unit best=null;int least=int.MaxValue;foreach(var u in PreyNear(a.x,a.y)){int d=Distance(a.x,a.y,u.x,u.y);if(d>range||d>=least||AnimalPrey(u.x,u.y)==null)continue;best=u;least=d;}return best==null?null:AnimalPrey(best.x,best.y);}
 void AnimalsTurn(){
  if(!wildlife)return;if(animals==null)animals=new List<Animal>();var r=new Random(seed*31+turn*7);IndexPrey();SpawnAnimals(r);
  foreach(var a in animals.ToList()){
   if(!animals.Contains(a)||a.hp<=0)continue;a.hp=Math.Min(10,a.hp+1);
   if(AnimalCatalog.Carnivore[a.kind]){
    for(int step=0;step<AnimalSteps(a)&&a.hp>0;step++){
     // The first turns give every people the chance to settle before the hunters find them.
     var prey=turn<HuntersFrom?null:NearestPrey(a,AnimalCatalog.Sense);if(prey==null){AnimalWander(a,r,false);continue;}
     if(Distance(a.x,a.y,prey.x,prey.y)==1){AnimalAttacks(a,prey);break;}
     Tile best=null;int least=Distance(a.x,a.y,prey.x,prey.y);for(int k=0;k<6;k++){var n=Step(a.x,a.y,HexOr(k,true),HexOr(k,false));if(!OpenToAnimal(n))continue;int d=Distance(n.x,n.y,prey.x,prey.y);if(d<least){least=d;best=n;}}
     if(best==null)break;a.x=best.x;a.y=best.y;
    }
   }else{
    var close=NearestPrey(a,1);if(close!=null)AnimalAttacks(a,close);
    else if(r.Next(2)==0)for(int step=0;step<AnimalSteps(a);step++)AnimalWander(a,r,true);
   }
  }
  animals.RemoveAll(a=>a.hp<=0);preyGrid=null;preyCells=null;
  CheckVictory();
 }
 // Inside any civilisation's borders an animal moves at half speed (owner request, 6 Nov 2026); odd steps round up on alternate turns.
 public int AnimalSteps(Animal a){int steps=AnimalCatalog.Steps[a.kind];var t=At(a.x,a.y);if(t==null||t.owner<0)return steps;return (steps+((turn+a.id)&1))/2;}
 // A herbivore keeps a clear tile between itself and every unit, so it never wanders into a fight.
 void AnimalWander(Animal a,Random r,bool keepClear){
  for(int attempt=0;attempt<6;attempt++){int k=r.Next(6);var n=Step(a.x,a.y,HexOr(k,true),HexOr(k,false));if(!OpenToAnimal(n))continue;if(keepClear&&PreyNear(n.x,n.y).Any(u=>Distance(u.x,u.y,n.x,n.y)<2))continue;a.x=n.x;a.y=n.y;return;}
 }
 // A unit attacks an animal by moving onto it. Civilians cannot. Combat has no dice, so the result can be known in advance.
 public void AnimalExchange(Unit u,Animal a,out int dealt,out int taken){int own=Strength(u,null,false);dealt=Damage(own,AnimalCatalog.Defence[a.kind]);taken=IsRanged(u.type)?0:Damage(AnimalCatalog.Defence[a.kind],own);}
 bool FightAnimal(Unit u,Animal a){
  if(Power[u.type]==0)return false;int dealt,taken;AnimalExchange(u,a,out dealt,out taken);a.hp-=dealt;u.hp-=taken;u.moves=0;string name=AnimalCatalog.Names[a.kind];
  if(u.owner==0)Note(UnitName(u)+" fights a "+name+".");
  if(a.hp<=0){animals.Remove(a);if(u.hp>0)Victory(u);if(u.owner==0)Note("The "+name+" is dead.");}
  if(u.hp<=0){if(u.owner==0)Note("The "+name+" killed your "+UnitName(u)+".");units.Remove(u);DrownStranded();}
  CheckVictory();return true;
 }
 public bool GoodHunt(Unit u,Animal a){int dealt,taken;AnimalExchange(u,a,out dealt,out taken);if(dealt>=a.hp)return true;return taken<u.hp&&dealt>=taken;}
 // Rival soldiers deal with animals close by when the fight favours them.
 bool HuntAnimalAI(Unit u){
  if(!wildlife||animals==null||animals.Count==0||IsNaval(u.type))return false;Animal best=null;int least=6;
  foreach(var a in animals){int d=Distance(u.x,u.y,a.x,a.y);if(d>=least||!GoodHunt(u,a)||!VisibleTo(u.owner,a.x,a.y))continue;best=a;least=d;}
  return best!=null&&Order(u,best.x,best.y);
 }
 bool AnimalThreat(int owner,int x,int y){if(!wildlife||animals==null)return false;foreach(var a in animals)if(AnimalCatalog.Carnivore[a.kind]&&Distance(a.x,a.y,x,y)<=3&&VisibleTo(owner,a.x,a.y))return true;return false;}
 public string AnimalLabel(Animal a){return AnimalCatalog.Names[a.kind]+" ("+(AnimalCatalog.Carnivore[a.kind]?"hunter":"keep one tile away")+"; attack "+AnimalCatalog.Attack[a.kind]+", defence "+AnimalCatalog.Defence[a.kind]+", health "+a.hp+")";}
}
public static partial class PixelArt {
 static readonly Dictionary<int,PixelCanvas> animalCache=new Dictionary<int,PixelCanvas>();
 // Side-on pictures drawn in code, 64 x 64, feet at about y=46 so they stand like the unit figures.
 public static PixelCanvas AnimalSprite(int kind){
  PixelCanvas cv;if(animalCache.TryGetValue(kind,out cv))return cv;cv=new PixelCanvas(64,64);
  uint[][] coats={Ramp(120,84,40,160,118,60,196,152,84,222,184,116,240,212,150),Ramp(110,66,30,150,96,44,190,130,62,216,164,92,236,198,130),Ramp(50,34,26,78,54,38,106,76,50,134,100,68,162,128,90),Ramp(60,62,68,92,94,100,126,128,132,160,162,164,198,198,198),Ramp(70,48,30,102,72,44,134,98,60,162,126,82,190,156,108),Ramp(58,34,22,92,56,32,124,80,44,154,108,62,182,138,88),Ramp(64,50,40,96,76,60,128,104,82,158,132,106,186,162,134)};
  var coat=coats[kind];uint dark=Rgb(22,18,16),bone=Rgb(236,226,200);
  Func<double,double,uint> fur=(dx,dy)=>Lit(coat,dx,dy);
  Action<int,int,int> leg=(x,w,bottom)=>RectFill(cv,x,34,x+w,bottom,RampPick(coat,.35));
  switch(kind){
   case 0:// cave lion: long low cat, no mane, tufted tail
    leg(20,4,47);leg(25,4,47);leg(40,4,47);leg(45,4,47);Ellipse(cv,32,32,17,8,fur);Ellipse(cv,50,27,7,7,fur);Poly(cv,new[]{P(52,29),P(60,30),P(52,33)},(X,Y)=>RampPick(coat,.6));cv.Set(52,25,dark);
    Poly(cv,new[]{P(46,21),P(48,17),P(50,21)},(X,Y)=>RampPick(coat,.5));for(int i=0;i<9;i++)cv.Set(15-i,30-i/2,RampPick(coat,.4));RectFill(cv,4,23,8,28,dark);break;
   case 1:// smilodon: heavy shoulders, short tail, long sabre teeth
    leg(20,5,47);leg(26,5,47);leg(39,5,47);leg(45,5,47);Ellipse(cv,31,31,16,9,fur);Ellipse(cv,42,28,9,10,fur);Ellipse(cv,52,26,7,7,fur);for(int y=30;y<40;y++){cv.Set(53,y,bone);cv.Set(56,y,bone);}cv.Set(53,24,dark);
    for(int i=0;i<4;i++)cv.Set(15-i,29,RampPick(coat,.4));for(int s=0;s<5;s++)cv.Set(24+s*4,27+s%2,RampPick(coat,.15));break;
   case 2:// cave bear: great humped bulk, domed brow
    leg(18,7,47);leg(26,6,47);leg(38,6,47);leg(45,7,47);Ellipse(cv,31,30,18,13,fur);Ellipse(cv,40,23,8,6,fur);Ellipse(cv,51,28,8,7,fur);Ellipse(cv,57,30,4,3,(dx,dy)=>RampPick(coat,.75));Ellipse(cv,47,21,2,2,fur);cv.Set(52,26,dark);cv.Set(60,30,dark);break;
   case 3:// dire wolf: lean, long legs, raised tail, grey
    leg(21,3,47);leg(26,3,47);leg(40,3,47);leg(44,3,47);Ellipse(cv,32,29,14,7,fur);Ellipse(cv,47,23,6,6,fur);Poly(cv,new[]{P(49,21),P(60,24),P(50,27)},(X,Y)=>RampPick(coat,.7));Poly(cv,new[]{P(44,19),P(46,12),P(49,18)},(X,Y)=>RampPick(coat,.5));Poly(cv,new[]{P(19,27),P(6,20),P(10,30)},(X,Y)=>RampPick(coat,.45));cv.Set(49,22,dark);cv.Set(60,24,dark);break;
   case 4:// megatherium: giant ground sloth rearing on thick hind legs, long claws
    RectFill(cv,20,34,29,47,RampPick(coat,.35));RectFill(cv,33,34,41,47,RampPick(coat,.35));Ellipse(cv,28,29,14,16,fur);Ellipse(cv,20,40,8,6,fur);Ellipse(cv,38,16,8,8,fur);Ellipse(cv,44,16,4,3,(dx,dy)=>RampPick(coat,.75));cv.Set(40,14,dark);RectFill(cv,38,25,46,28,RampPick(coat,.5));
    for(int k=0;k<3;k++)for(int i=0;i<5;i++)cv.Set(46+i,24+k*2+i/2,bone);break;
   case 5:// woolly mammoth: domed head, sloping back, curved tusks, trunk, shaggy skirt
    leg(16,7,47);leg(25,6,47);leg(35,6,47);leg(42,7,47);Ellipse(cv,28,28,16,12,fur);Ellipse(cv,40,20,10,11,fur);Ellipse(cv,48,23,6,7,fur);for(int y=26;y<44;y++){cv.Set(52+(y-26)/6,y,RampPick(coat,.45));cv.Set(53+(y-26)/6,y,RampPick(coat,.55));}
    for(int i=0;i<14;i++){int y=32+(int)(Math.Sin(i/14.0*Math.PI)*7);cv.Set(46+i,y,bone);cv.Set(46+i,y+1,bone);}cv.Set(46,21,dark);for(int x=14;x<45;x+=3)RectFill(cv,x,36,x+2,40+(x%2),RampPick(coat,.2));break;
   default:// woolly rhinoceros: low head, shoulder hump, two horns
    leg(17,6,47);leg(25,5,47);leg(38,5,47);leg(45,6,47);Ellipse(cv,30,30,17,11,fur);Ellipse(cv,38,23,7,6,fur);Ellipse(cv,51,32,8,7,fur);Poly(cv,new[]{P(55,27),P(60,14),P(59,29)},(X,Y)=>bone);Poly(cv,new[]{P(50,26),P(52,20),P(53,27)},(X,Y)=>bone);cv.Set(49,30,dark);for(int x=14;x<46;x+=3)RectFill(cv,x,37,x+2,41,RampPick(coat,.2));break;
  }
  Outline(cv,Ink);animalCache[kind]=cv;return cv;
 }
}
}
