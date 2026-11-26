using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Religion, simplified (owner request, 26 Nov 2026: "Religion is too complicated, simplify it further"). The Religion button
// opens one page with three things to know and at most one decision each:
//   Your faith        faith gathered, the one next step, and the choice that step needs (a pantheon, then a faith to found).
//   Your cities       which faith and tradition each city follows, and whether it has a temple.
//   Your tradition    the four traditions with their one gift; you may favour one of them (it costs gold a turn per follower).
// Commitments, conviction, practices, houses, vows, church and state, doctrines and the faith map are still in the game and are
// reached with "Full details" (the earlier Beliefs & Traditions and Organised Faiths screens); nothing on this page needs them.
// Doctrines are already learned automatically; a founded faith takes its usual beliefs (DefaultBeliefs) when founded here.
public partial class RealmGame {
 bool religionDetail;Vector2 religionCityScroll;
 void DrawSimpleReligion(){
  DiplomacyStyles();CourtStyles();waysHover=null;
  if(CourtChrome("Religion","Faith and the ways of your people")){detailsScreen="";GUIUtility.ExitGUI();}
  var me=world.nations[0];var mine=world.cities.Where(c=>c.owner==0).ToList();var nw=world.NationWayOf(0);
  // ---- Your faith ----
  var a=new Rect(8,72,624,372);GoldFrame(a);{float x=a.x+44,w=a.width-88,y=a.y+26;
   CourtHeading(new Rect(x,y,w,22),"Your faith");y+=30;
   int state=world.StateReligion(0);
   WayLabel(new Rect(x,y,w,22),"Faith "+me.faith+"  (+"+world.FaithPerTurn(0)+" a turn)"+(state>=0?"   ·   Your people follow "+world.FaithName(state):me.pantheon>=0?"   ·   Pantheon: "+ReligionCatalog.PantheonNames[me.pantheon]:""),courtText,15,11);y+=28;
   var step=new Rect(x,y,w,52);Fill(step,new Color(.03f,.05f,.1f,.92f));Fill(new Rect(step.x,step.y,3,step.height),CourtGold);
   WayLabel(new Rect(step.x+12,step.y+6,step.width-20,step.height-10),"Next: "+world.FaithNextStep(0),courtItalic,15,11);y+=62;
   if(me.pantheon<0){
    string why=world.PantheonReason(0);WayLabel(new Rect(x,y,w,20),why==""?"Choose the gods of your people:":"Pantheons cost "+World.PantheonCost+" faith.",courtText,14,11);y+=24;
    for(int p=0;p<ReligionCatalog.PantheonNames.Length;p++){var row=new Rect(x,y,w,32);
     if(WayButton(new Rect(row.x,row.y,190,28),ReligionCatalog.PantheonNames[p],false,why))world.ChoosePantheon(0,p);
     WayLabel(new Rect(row.x+200,row.y+2,row.width-200,28),ReligionCatalog.PantheonText[p],courtText,12,9);y+=32;}}
   else if(me.founded<0&&state<0){
    var open=Enumerable.Range(0,ReligionCatalog.FoundableCount).Where(r=>world.FoundingReason(0,r)=="").ToList();
    if(open.Count==0){WayLabel(new Rect(x,y,w,60),"Pantheon: "+ReligionCatalog.PantheonNames[me.pantheon]+" — "+ReligionCatalog.PantheonText[me.pantheon],courtText,13,10);}
    else{WayLabel(new Rect(x,y,w,20),"Found a faith for "+world.FoundingCost()+" faith (its holy city will be your best temple city):",courtText,14,11);y+=24;
     float bw=(w-16)/3;for(int k=0;k<open.Count&&k<9;k++){int r=open[k];if(WayButton(new Rect(x+(k%3)*(bw+8),y+(k/3)*34,bw,28),ReligionCatalog.Name(r))){if(world.FoundReligion(0,r))status=ReligionCatalog.Name(r)+" founded.";}}}}
   else{
    WayLabel(new Rect(x,y,w,a.yMax-12-y),(me.pantheon>=0?"Pantheon: "+ReligionCatalog.PantheonNames[me.pantheon]+" — "+ReligionCatalog.PantheonText[me.pantheon]+"\n":"")+
     "Doctrines are learned for you as faith comes in ("+world.DoctrineCount(0)+" so far). Temples raise faith and spread it to nearby cities; Missionaries carry it further.",courtText,14,10);}
  }
  // ---- Your cities ----
  var b=new Rect(8,450,624,294);GoldFrame(b);{float x=b.x+44,w=b.width-88,y=b.y+26;CourtHeading(new Rect(x,y,w,22),"Your cities");y+=30;
   var area=new Rect(x,y,w,b.yMax-14-y);religionCityScroll=GUI.BeginScrollView(area,religionCityScroll,new Rect(0,0,w-18,Math.Max(area.height,mine.Count*28)));
   for(int k=0;k<mine.Count;k++){var c=mine[k];var l=world.WayOf(c);var row=new Rect(0,k*28,w-20,26);Fill(row,new Color(.06f,.09f,.17f,.9f));if(l.tradition>=0)Fill(new Rect(row.x,row.y,4,row.height),WayInk[l.tradition]);
    string line=c.name+":  "+(c.religion>=0?world.FaithName(c.religion):"no faith yet")+"  ·  "+(l.tradition>=0?Lifeways.Names[l.tradition]:"no tradition yet")+"  ·  "+(world.HasBuilding(c,36)?"temple":"no temple");
    WayLabel(new Rect(row.x+10,row.y+4,row.width-14,20),line,courtText,13,9);}
   if(mine.Count==0)WayLabel(new Rect(0,0,w-18,24),"No cities yet.",courtItalic,13);
   GUI.EndScrollView();}
  // ---- Your tradition ----
  var d=new Rect(640,72,632,672);GoldFrame(d);{float x=d.x+44,w=d.width-88,y=d.y+26;CourtHeading(new Rect(x,y,w,22),"Traditions of your people");y+=30;
   WayLabel(new Rect(x,y,w,40),"Traditions grow in your cities on their own. Each gives its followers one gift. You may favour one: it spreads faster, for "+Lifeways.FavourCost+" gold a turn per city that follows it.",courtItalic,14,10);y+=50;
   float cardH=(d.yMax-16-y-3*10)/4;
   for(int t=0;t<Lifeways.Count;t++){var card=new Rect(x,y+t*(cardH+10),w,cardH);Fill(card,new Color(.05f,.07f,.14f,.95f));Fill(new Rect(card.x,card.y,4,card.height),WayInk[t]);
    bool favoured=nw.policy[t]==1;int follow=world.Followers(0,t).Count();
    GUI.Label(new Rect(card.x+16,card.y+8,card.width-200,26),Lifeways.Names[t].ToUpperInvariant(),new GUIStyle(courtCaps){fontSize=17,alignment=TextAnchor.MiddleLeft,wordWrap=false,normal={textColor=CourtCream}});
    WayLabel(new Rect(card.x+16,card.y+38,card.width-200,24),"Gift: "+Lifeways.Gifts[t].Replace(" (+2 while the commitment is held)",""),courtText,14,10);
    WayLabel(new Rect(card.x+16,card.y+64,card.width-200,card.height-70),(mine.Count==1?(follow==1?"Your city follows it.  ":"Your city does not follow it yet.  "):follow+" of your "+mine.Count+" cities follow it.  ")+Lifeways.Roots[t],courtItalic,13,9);
    if(WayButton(new Rect(card.xMax-170,card.y+card.height/2-16,154,32),favoured?"Favoured":"Favour",favoured)){
     if(favoured)world.SetWayPolicy(0,t,0);else{for(int k=0;k<Lifeways.Count;k++)if(k!=t&&nw.policy[k]==1)world.SetWayPolicy(0,k,0);world.SetWayPolicy(0,t,1);}}}
  }
  CourtFooter(new Rect(4,746,1272,52));
  GUI.Label(new Rect(44,758,700,34),"Everything else about belief is still there under Full details.",new GUIStyle(courtItalic){fontSize=14,alignment=TextAnchor.MiddleLeft,wordWrap=false});
  if(CourtButton(new Rect(770,758,240,34),"","Full details",CourtNavy)){religionDetail=true;faithsView=false;detailsScroll=Vector2.zero;GUIUtility.ExitGUI();}
  if(CourtButton(new Rect(1030,758,226,34),"","Return to Map",CourtNavy)){detailsScreen="";GUIUtility.ExitGUI();}
 }
}
}
