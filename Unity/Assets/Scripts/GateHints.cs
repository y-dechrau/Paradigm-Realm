using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Word of a gateway you have not seen (owner request, 25 Nov 2026): it no longer marks the gateway on the map, which gave the place
// away and, because the gateway then counted as found, kept the discovery screen from ever showing. The player hears only which way
// it lies from the capital and roughly how far, and the gateway is found by exploring, with its screen, as it always was. Rivals are
// told as before: they have no map to read.
[Serializable] public class GateHint{public int cell,turn,direction,far;public string from="",place="";}
public partial class World {
 public List<GateHint> gateHints=new List<GateHint>();
 public static readonly string[] CompassNames={"north","north-east","east","south-east","south","south-west","west","north-west"};
 static readonly string[] FarNames={"not far","a long way","very far"};
 // Which way one tile lies from another as the map shows it: 0 north, then clockwise by eighths.
 public int Compass(int x,int y,int tx,int ty){
  double wide=1,tall=hexGrid?30.0/64:mapShape==2?.25:1,dx=DeltaX(x,tx)+(Rows?((ty&1)-(y&1))*.5:0),dy=ty-y;
  double angle=Math.Atan2(dx*wide,-dy*tall);int k=(int)Math.Round(angle/(Math.PI/4));return ((k%8)+8)%8;}
 bool GateHinted(Tile g){int cell=g.y*Width+g.x;if(gateHints!=null)foreach(var h in gateHints)if(h.cell==cell)return true;return false;}
 // The player hears of a gateway. True when it is news.
 bool HearOfGate(Tile g,string from){
  if(gateHints==null)gateHints=new List<GateHint>();if(g==null||g.seen||GateHinted(g))return false;
  var capital=cities.Find(c=>c.id==nations[0].capital&&c.owner==0)??cities.FirstOrDefault(c=>c.owner==0&&At(c.x,c.y).realm==0);if(capital==null)return false;
  int d=Distance(capital.x,capital.y,g.x,g.y);var h=new GateHint{cell=g.y*Width+g.x,turn=turn,direction=Compass(capital.x,capital.y,g.x,g.y),far=d<=12?0:d<=28?1:2,from=from,place=capital.name};gateHints.Add(h);
  Note(HintText(h));return true;}
 public string HintText(GateHint h){return (h.from==""?"Your people's oldest stories tell":"Travellers from "+h.from+" speak")+" of an ancient, ruined gateway "+FarNames[Math.Max(0,Math.Min(2,h.far))]+" to the "+CompassNames[((h.direction%8)+8)%8]+" of "+h.place+". Nobody can say just where; it must be looked for.";}
 // Hints of gateways not yet found, for the gateways list and the mini-map.
 public List<GateHint> OpenHints(){var list=new List<GateHint>();if(gateHints==null)return list;foreach(var h in gateHints)if(h.cell>=0&&h.cell<tiles.Length&&!tiles[h.cell].seen)list.Add(h);return list;}
 // Saves from before 25 Nov 2026: a gateway marked only by rumour (none of the land round it has been seen) is unmarked and becomes
 // a hint, so that it can still be discovered. Safe to run on every load.
 public void ForgetRumouredGates(){
  if(gateHints==null)gateHints=new List<GateHint>();if(gatesFound==null||tiles==null||!gatesLocked)return;
  foreach(int cell in gatesFound.ToArray()){
   if(cell<0||cell>=tiles.Length)continue;var g=tiles[cell];if(g.realm!=0||g.gateLink<=0||GateOpen(g))continue;bool around=false;
   for(int dy=-1;dy<=1&&!around;dy++)for(int dx=-1;dx<=1;dx++){if(dx==0&&dy==0)continue;var n=Step(g.x,g.y,dx,dy);if(n!=null&&n.seen){around=true;break;}}
   var study=gateStudies!=null?gateStudies.Find(s=>s.owner==0&&s.pair==GatePair(g)):null;
   if(around||study!=null||units.Any(u=>u.owner==0&&Distance(u.x,u.y,g.x,g.y)<=2))continue;
   gatesFound.Remove(cell);g.seen=false;if(nations[0].known!=null&&cell<nations[0].known.Length)nations[0].known[cell]=false;HearOfGate(g,"");}
 }
}
}
