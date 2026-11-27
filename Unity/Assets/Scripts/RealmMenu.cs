using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Right-clicking Realms in the left menu opens a list of the worlds you know (owner request, 27 Nov 2026): the home world and every
// parallel world you have discovered. Choosing one shows it on the map at once (ShowRealm, as the Realms screen does); the world
// you are looking at is ticked. A click anywhere else, a second right-click or Escape closes the list.
public partial class RealmGame {
 bool realmMenuOpen;
 List<int> KnownRealms(){var list=new List<int>{0};if(world!=null)for(int r=1;r<=World.RealmCount;r++)if(world.RealmDiscovered(0,r))list.Add(r);return list;}
 const float RealmMenuRow=34;
 Rect RealmMenuRect{get{int i=System.Array.IndexOf(RailScreens,"Atlas");var rail=RailRect;float y=rail.y+5+i*RailItem;return new Rect(rail.xMax+4,y,236,KnownRealms().Count*RealmMenuRow+12);}}
 // Called by the left menu for the Realms bar, before its own button: a right-click there opens or closes the list.
 void RealmMenuInput(Rect bar){var e=Event.current;if(e.type==EventType.MouseDown&&e.button==1&&bar.Contains(e.mousePosition)&&GUI.enabled){realmMenuOpen=!realmMenuOpen;e.Use();}}
 // Called before the map takes its clicks: a press outside the open list closes it; a press inside is left for the list itself.
 bool RealmMenuBlocks(Event e){
  if(!realmMenuOpen)return false;
  if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Escape){realmMenuOpen=false;e.Use();return true;}
  if(e.type==EventType.MouseDown){if(RealmMenuRect.Contains(e.mousePosition))return true;realmMenuOpen=false;}
  return false;
 }
 void DrawRealmMenu(){
  if(!realmMenuOpen||world==null)return;Wide();bool was=GUI.enabled;GUI.enabled=true;
  var r=RealmMenuRect;Fill(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),new Color(.86f,.7f,.39f,.95f));Fill(r,new Color(.04f,.06f,.13f,.98f));
  var st=new GUIStyle(hudLabel){fontSize=15,alignment=TextAnchor.MiddleLeft};if(displayFont!=null)st.font=displayFont;
  var known=KnownRealms();
  for(int k=0;k<known.Count;k++){int realm=known[k];var row=new Rect(r.x+6,r.y+6+k*RealmMenuRow,r.width-12,RealmMenuRow-2);bool over=row.Contains(Event.current.mousePosition);
   if(over)Fill(row,new Color(.86f,.7f,.39f,.18f));if(realm==viewRealm)Fill(new Rect(row.x,row.y+4,3,row.height-8),new Color(.95f,.8f,.4f));
   st.normal.textColor=realm==viewRealm?new Color(1f,.88f,.55f):over?new Color(1f,.92f,.7f):UiText;
   FitLabel(new Rect(row.x+12,row.y,row.width-20,row.height),(realm==0?"Home world":World.RealmNames[realm])+(realm==viewRealm?"  (here)":""),st,10);
   if(GUI.Button(row,"",GUIStyle.none)){realmMenuOpen=false;detailsScreen="";if(realm!=viewRealm)ShowRealm(realm);GUIUtility.ExitGUI();}}
  GUI.enabled=was;
 }
}
}
