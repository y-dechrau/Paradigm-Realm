using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The site panel (owner brief, 26 Nov 2026): left-click a woodland, hills or a timber or stone resource you have seen beyond your
// borders. It answers, from your own knowledge: what we could use it for now, who has a known claim, how we could connect it,
// which work would benefit and by how much; then the choices side by side, with what each commits.
public partial class RealmGame {
 int sitePanel=-1;
 bool SitePanelOpen(){return sitePanel>=0&&world!=null&&sitePanel<world.tiles.Length;}
 bool OpenSiteAt(int x,int y){
  if(!world.buildingMaterials)return false;var t=world.At(x,y);if(t==null||!t.seen||t.owner==0||PixelArt.IsWater(t.terrain))return false;
  bool site=World.Wooded(t)||t.terrain==2||t.resource==MaterialRules.TimberResource||t.resource==MaterialRules.StoneResource;if(!site)return false;
  sitePanel=y*world.Width+x;ResetMapGesture();return true;}
 void DrawSitePanel(){
  GUI.enabled=true;DiplomacyStyles();HudStyles();var t=world.tiles[sitePanel];
  Fill(FullCanvas,new Color(0,0,0,.6f));var box=new Rect(190,90,900,610);DipPanel(box,.98f);
  var rep=world.AssessSite(0,t);string what=World.Wooded(t)?"WOODLAND":t.terrain==2?"HILLS":"RESOURCE";
  GUI.Label(new Rect(box.x,box.y+14,box.width,40),what+" BEYOND YOUR BORDERS",dipTitle);
  var text=new GUIStyle(dipRow){fontSize=14,wordWrap=true,alignment=TextAnchor.UpperLeft};var bold=new GUIStyle(text){fontStyle=FontStyle.Bold};
  float x=box.x+36,w=box.width-72,y=box.y+62;
  foreach(var line in rep.lines){float h=text.CalcHeight(new GUIContent(line),w);GUI.Label(new Rect(x,y,w,h),line,text);y+=h+4;}
  y+=6;Fill(new Rect(x,y,w,1),new Color(.86f,.7f,.39f,.6f));y+=10;
  Color navy=new Color(.1f,.16f,.3f),teal=new Color(.07f,.38f,.4f);
  foreach(var o in rep.options){
   float h=Mathf.Max(22,text.CalcHeight(new GUIContent(o.detail),w-200));GUI.Label(new Rect(x,y,w-200,22),o.title,bold);GUI.Label(new Rect(x,y+22,w-200,h),o.detail,text);
   if(o.action=="camp"&&DipButton(new Rect(x+w-184,y+4,184,34),"Send a Worker",teal)){string why=world.SendWorkerToCamp(0,t);status=why==""?"A Worker is on the way to set up the camp.":why;sitePanel=-1;GUIUtility.ExitGUI();}
   if(o.action.StartsWith("buy:")&&DipButton(new Rect(x+w-184,y+4,184,34),"Buy it",teal)){var p=o.action.Split(':');string why=world.BuyShipment(0,int.Parse(p[1]),rep.kind,int.Parse(p[2]));status=why==""?"Bought.":why;GUIUtility.ExitGUI();}
   y+=22+h+12;if(y>box.yMax-70)break;}
  float fy=box.yMax-56;
  if(DipButton(new Rect(box.x+28,fy,200,38),"Close",navy)){sitePanel=-1;GUIUtility.ExitGUI();}
  if(t.owner>0&&world.met[t.owner]&&DipButton(new Rect(box.xMax-28-260,fy,260,38),"Talk to "+world.NationName(t.owner),navy)){sitePanel=-1;diplomacyPartner=t.owner;diplomacyDirect=true;OpenDetails("Diplomacy");}
 }
}
}
namespace ParadigmRealm {
public partial class RealmGame {
 // A camp on the map, drawn in code: a log pile or cut stone, a lean-to and a pennant in the owner's colour.
 void DrawOutpost(Outpost p,float sx,float foot,float s){
  float bx=sx+8*s,by=foot-12*s;var flag=p.owner<nations.Length?nations[p.owner]:Color.white;
  if(p.kind==0){Color bark=new Color(.42f,.27f,.14f),cut=new Color(.86f,.7f,.45f);for(int r=0;r<3;r++)for(int k=0;k<3-r;k++){var log=new Rect(bx+k*5*s+r*2.5f*s,by-r*3.4f*s,5*s,3.4f*s);Fill(log,bark);Fill(new Rect(log.xMax-1.6f*s,log.y+.6f*s,1.4f*s,2.2f*s),cut);}}
  else{Color grey=new Color(.62f,.6f,.56f),dark=new Color(.38f,.36f,.34f);for(int r=0;r<2;r++)for(int k=0;k<3-r;k++){var block=new Rect(bx+k*5.4f*s+r*2.7f*s,by-r*4f*s,5*s,3.8f*s);Fill(block,grey);Fill(new Rect(block.x,block.yMax-.8f*s,block.width,.8f*s),dark);}}
  // Lean-to and pennant.
  var hide=new Color(.55f,.44f,.3f);for(int i=0;i<6;i++)Fill(new Rect(sx-12*s+i*1.2f*s,foot-14*s+i*1.6f*s,(8-i*1.1f)*s,1.6f*s),hide);
  Fill(new Rect(sx-13*s,foot-26*s,1.2f*s,16*s),new Color(.3f,.22f,.14f));Fill(new Rect(sx-11.8f*s,foot-26*s,7*s,4*s),flag);
 }
}
}
