using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// The living countryside (26 Nov 2026): the lines things travel along lie on the painted roads and rivers, neighbouring tiles' lines
// meet, and the demonstration valley is a sound game with everything the scene needs.
public static class AmbientChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Living countryside: "+what);}
 public static void Run(){
  // Roads: each arm starts at its own edge and ends where the others end, on the painted road.
  foreach(int mask in new[]{9,18,36,1|8|16,2|16|4,1,63})for(int look=0;look<4;look++){var paint=PixelArt.HexRoad(mask,2,look);double[] middle=null;for(int k=0;k<6;k++){if((mask&(1<<k))==0)continue;var arm=PixelArt.HexRoadArm(mask,k,look,0,0,0);Check(arm.Count>3,"an arm has a line");int off=0,seen=0;foreach(var p in arm){int x=(int)p[0],y=(int)p[1];if(x<2||y<2||x>=PixelArt.HW-2||y>=PixelArt.HH-2)continue;seen++;bool on=false;for(int dy=-1;dy<=1&&!on;dy++)for(int dx=-1;dx<=1;dx++)if((paint.Get(x+dx,y+dy)>>24)!=0){on=true;break;}if(!on)off++;}Check(off*8<=seen,"the line lies on the painted road (mask "+mask+", arm "+k+", "+off+" of "+seen+" off it)");
    var end=arm[arm.Count-1];if(middle==null)middle=end;else Check(Math.Abs(end[0]-middle[0])<1.5&&Math.Abs(end[1]-middle[1])<1.5,"arms meet in the middle (mask "+mask+")");}}
  // Rivers: the trunk runs through its own water, from an inflow to the outflow.
  for(int down=0;down<6;down++)for(int up=0;up<6;up++){if(up==down||(up+1)%6==down||(down+1)%6==up)continue;int mask=1<<down|1<<up|(down+1)<<PixelArt.RiverDownShift;for(int k=0;k<6;k++)if((mask&(1<<k))!=0)mask|=1<<(PixelArt.RiverWidthShift+2*k);var line=PixelArt.HexRiverLine(mask,1);Check(line!=null&&line.Count>20,"a river with an outflow has a line");int dry=0;foreach(var p in line){int x=(int)(p[0]*2),y=(int)(p[1]*2);if(x<4||y<4||x>=PixelArt.HW*2-4||y>=PixelArt.HH*2-4)continue;bool wet=false;for(int dy=-2;dy<=2&&!wet;dy+=2)for(int dx=-2;dx<=2&&!wet;dx+=2)wet=PixelArt.HexRiverWet(mask,1,x+dx,y+dy);if(!wet)dry++;}Check(dry<=3,"and it runs in the water ("+dry+" points dry)");Check(PixelArt.LineLength(line)>20,"all the way across");}
  Check(PixelArt.HexRiverLine(9,1)==null,"a river whose flow is not known has none, so no mark runs the wrong way");
  // The valley.
  {var w=World.ValleyScenario();Check(w.valleyMill>=0&&w.Valid(),"the valley is a sound game");var mill=w.tiles[w.valleyMill];var bridge=w.ValleyCentre();Check(mill.terrain==7&&mill.road&&bridge!=mill&&bridge.road&&w.Bridged(bridge)&&w.Distance(mill.x,mill.y,bridge.x,bridge.y)==2,"a bridge above a mill on one river");
   var city=w.cities.First(c=>c.owner==0);Check(w.Worked(city).Contains(mill)&&w.tiles.Count(t=>t.farm&&t.owner==0)>=4,"a town that works the mill and has fields");Check(w.HomeGates().Any(g=>w.gatesFound.Contains(g.y*w.Width+g.x)&&w.Distance(g.x,g.y,bridge.x,bridge.y)<=5&&!w.GateOpen(g)),"and a sealed gateway near");
   var again=World.ValleyScenario();Check(again.valleyMill==w.valleyMill&&again.seed==w.seed,"it is the same valley every time");int gold=w.gold;for(int t=0;t<6;t++)w.EndTurn();Check(w.Valid(),"and it plays on");
   var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));Check(copy.valleyMill==w.valleyMill,"and saves");}
  Check(World.New(77).valleyMill==-1,"no other game has the mill");
 }
}
}
