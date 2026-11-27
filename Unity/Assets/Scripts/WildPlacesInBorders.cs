using System;
using System.Linq;
namespace ParadigmRealm {
// Owner request (27 Nov 2026): when a civilisation's borders come to cover a barbarian camp or a tribal village, it disappears.
// A camp inside settled land breaks up, and the raiders it had sent out go with it. A village is taken in: its people join the
// nearest town of the land's owner (one citizen) and it pays no tribute any longer, since it is no longer a village of its own.
public partial class World {
 public void ClearWildPlacesInBorders(){
  if(tiles==null)return;
  if(camps!=null&&camps.Count>0){
   foreach(var c in camps.ToList()){var t=At(c.x,c.y);if(t==null||t.owner<0)continue;
    camps.Remove(c);if(raiders!=null)raiders.RemoveAll(r=>r.camp==c.id);
    if(t.owner==0||c.seen)Note("The barbarian camp "+(t.owner==0?"inside your borders":"inside "+NationName(t.owner)+"'s borders")+" has broken up; its raiders have scattered.");}}
  for(int k=0;k<tiles.Length;k++){var t=tiles[k];if(!t.village||t.owner<0)continue;
   t.village=false;int was=t.villageOwner;t.villageOwner=-1;
   var town=cities.Where(c=>c.owner==t.owner).OrderBy(c=>Distance(c.x,c.y,t.x,t.y)).FirstOrDefault();if(town!=null)town.pop++;
   if(t.owner==0)Note("A tribal village inside your borders has settled among your people"+(town!=null?": "+town.name+" grows to size "+town.pop:"")+".");
   else if(was==0)Note("A tribal village that paid you tribute is now part of "+NationName(t.owner)+".");}
 }
}
}
