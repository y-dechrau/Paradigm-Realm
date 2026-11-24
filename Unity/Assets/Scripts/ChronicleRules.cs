using System;
namespace ParadigmRealm {
// What the Chronicle may tell (owner request, 24 Nov 2026): only what the player can see happen, or what happens to a civilisation
// the player knows. World.Note writes the line; anything that is not the player's own affair goes through NoteEvent, which drops it
// when neither holds. Whatever is written, the name of a civilisation not yet met is never given away (ConcealUnmet).
public partial class World {
 const string UnknownPeople="an unknown people";
 void NoteEvent(string text,Tile where,params int[] who){
  bool tell=where!=null&&Visible(where.x,where.y);if(!tell)foreach(int o in who)if(Known(o)){tell=true;break;}
  if(tell)Note(text);}
 // The first civilisation the player has not met whose name is in the line, from a starting number, or -1. A name that is part of
 // the player's own or of a met civilisation's name is left alone.
 int NamedUnmet(string s){
  if(string.IsNullOrEmpty(s)||met==null||nations==null)return -1;
  for(int o=1;o<civCount&&o<nations.Length;o++){if(Known(o)||nations[o]==null)continue;string name=NationName(o);if(string.IsNullOrEmpty(name)||name.Length<4||s.IndexOf(name,StringComparison.Ordinal)<0)continue;
   bool shared=false;for(int k=0;k<civCount&&!shared;k++)if(Known(k)&&NationName(k).IndexOf(name,StringComparison.Ordinal)>=0)shared=true;if(!shared)return o;}
  return -1;}
 public bool NamesUnmet(string s){return NamedUnmet(s)>=0;}
 public string ConcealUnmet(string s){
  for(int guard=0;guard<8;guard++){int o=NamedUnmet(s);if(o<0)return s;string name=NationName(o);
   for(int at=s.IndexOf(name,StringComparison.Ordinal);at>=0;at=s.IndexOf(name,at+1,StringComparison.Ordinal)){int from=at,length=name.Length;if(at>=4&&string.CompareOrdinal(s,at-4,"The ",0,4)==0||at>=4&&string.CompareOrdinal(s,at-4,"the ",0,4)==0){from=at-4;length+=4;}
    bool opens=from==0||from>=2&&(s[from-1]==' '&&".!?:/".IndexOf(s[from-2])>=0);s=s.Substring(0,from)+(opens?"An"+UnknownPeople.Substring(2):UnknownPeople)+s.Substring(from+length);at=from;}}
  return s;}
}
}
