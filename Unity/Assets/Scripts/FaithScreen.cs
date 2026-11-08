using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The Religion screen in the style of the owner's mockup (8 Nov 2026): a framed header with the faith's emblem, name, motto,
// followers, faith and authority; six gold tabs; on the Overview the holy city, doctrine medallions and a map of the world's faiths
// with its legend; along the bottom the faiths of one city, the key institutions and faith actions; on the right a religious
// summary, recent events and the faiths of the world. The other five tabs keep their content in the same frame.
public partial class RealmGame {
 Texture2D sunEmblem,medallion,faithMap;int faithMapTurn=-1,faithMapCount=-1,faithCityIndex;Vector2 faithTabScroll;
 static readonly Color FaithGold=new Color(.86f,.7f,.39f),FaithNavy=new Color(.04f,.07f,.14f,.95f),FaithInk=new Color(.96f,.92f,.82f),FaithMuted=new Color(.72f,.68f,.6f);
 // A colour for each faith: your state faith is always gold, others spread round the colour wheel.
 Color FaithColour(int r){if(r<0)return new Color(.55f,.55f,.58f);if(r==world.StateReligion(0))return new Color(.93f,.68f,.18f);float h=(r*0.381966f+.58f)%1f;if(Mathf.Abs(h-.11f)<.06f)h+=.12f;return Color.HSVToRGB(h%1f,.62f,.85f);}
 void FaithFrame(Rect r,float alpha=.95f){Fill(r,new Color(FaithNavy.r,FaithNavy.g,FaithNavy.b,alpha));var g=FaithGold;Fill(new Rect(r.x,r.y,r.width,2),g);Fill(new Rect(r.x,r.yMax-2,r.width,2),g);Fill(new Rect(r.x,r.y,2,r.height),g);Fill(new Rect(r.xMax-2,r.y,2,r.height),g);var dark=new Color(.32f,.24f,.1f);Fill(new Rect(r.x+4,r.y+4,r.width-8,1),dark);Fill(new Rect(r.x+4,r.yMax-5,r.width-8,1),dark);Fill(new Rect(r.x+4,r.y+4,1,r.height-8),dark);Fill(new Rect(r.xMax-5,r.y+4,1,r.height-8),dark);foreach(var c in new[]{new Vector2(r.x,r.y),new Vector2(r.xMax-8,r.y),new Vector2(r.x,r.yMax-8),new Vector2(r.xMax-8,r.yMax-8)})Fill(new Rect(c.x,c.y,8,8),g);}
 void FaithTitle(Rect r,string text){GUI.Label(new Rect(r.x+14,r.y+8,r.width-28,26),text,new GUIStyle(hudTitle){fontSize=17,alignment=TextAnchor.MiddleLeft});Fill(new Rect(r.x+14,r.y+36,r.width-28,1),new Color(FaithGold.r,FaithGold.g,FaithGold.b,.6f));}
 GUIStyle FaithText(int size,TextAnchor anchor=TextAnchor.MiddleLeft,bool italic=false,Color? ink=null){var s=new GUIStyle(hudLabel){fontSize=size,alignment=anchor,wordWrap=true,fontStyle=italic?FontStyle.Italic:FontStyle.Normal};s.normal.textColor=ink??FaithInk;return s;}
 // ---------- procedural art: a radiant sun and a round medallion ----------
 Texture2D SunEmblem(){if(sunEmblem!=null)return sunEmblem;const int n=96;var t=new Texture2D(n,n,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};var px=new Color32[n*n];float c=(n-1)/2f;
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){float dx=x-c,dy=y-c,d=Mathf.Sqrt(dx*dx+dy*dy),a=Mathf.Atan2(dy,dx);float ray=Mathf.Pow(Mathf.Abs(Mathf.Cos(a*8)),6f);Color col=new Color(0,0,0,0);
   if(d<20)col=Color.Lerp(new Color(1f,.93f,.55f),new Color(.95f,.62f,.12f),d/20f);else if(d<23)col=new Color(.55f,.32f,.06f);else if(d<44*(.55f+.45f*ray)&&d<46)col=Color.Lerp(new Color(1f,.8f,.3f),new Color(.78f,.45f,.08f),(d-23)/23f);
   else if(d>=44&&d<46)col=new Color(.86f,.7f,.39f,.8f);px[y*n+x]=col;}
  t.SetPixels32(px);t.Apply();themeTextures.Add(t);sunEmblem=t;return t;}
 Texture2D Medallion(){if(medallion!=null)return medallion;const int n=64;var t=new Texture2D(n,n,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};var px=new Color32[n*n];float c=(n-1)/2f;
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){float d=Mathf.Sqrt((x-c)*(x-c)+(y-c)*(y-c));Color col=new Color(0,0,0,0);if(d<27)col=Color.Lerp(new Color(.16f,.2f,.32f),new Color(.06f,.08f,.15f),d/27f);else if(d<31)col=Color.Lerp(new Color(1f,.85f,.5f),new Color(.6f,.42f,.15f),(d-27)/4f);px[y*n+x]=col;}
  t.SetPixels32(px);t.Apply();themeTextures.Add(t);medallion=t;return t;}
 // The home world coloured by the faith of the nearest city (unexplored land stays dark); rebuilt once a turn.
 Texture2D FaithMap(){
  if(faithMap!=null&&faithMapTurn==world.turn&&faithMapCount==world.cities.Count)return faithMap;int w=world.Width,h=world.MainHeight;
  if(faithMap==null||faithMap.width!=w||faithMap.height!=h){if(faithMap!=null)Destroy(faithMap);faithMap=new Texture2D(w,h,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};}
  var px=new Color32[w*h];var home=world.cities.Where(c=>world.At(c.x,c.y).realm==0).ToList();var colours=home.Select(c=>FaithColour(c.religion>=0?c.religion:-1)).ToList();
  for(int y=0;y<h;y++)for(int x=0;x<w;x++){var t=world.At(x,y);Color col;
   if(t==null||!t.seen)col=new Color(.05f,.06f,.09f);else if(PixelArt.IsWater(t.terrain))col=new Color(.1f,.17f,.3f);
   else if(t.owner<0)col=new Color(.42f,.45f,.36f);else{int best=-1,least=int.MaxValue;for(int k=0;k<home.Count;k++){if(home[k].owner!=t.owner)continue;int d=world.Distance(x,y,home[k].x,home[k].y);if(d<least){least=d;best=k;}}col=best<0?new Color(.42f,.45f,.36f):colours[best];}
   px[(h-1-y)*w+x]=col;}
  faithMap.SetPixels32(px);faithMap.Apply();faithMapTurn=world.turn;faithMapCount=world.cities.Count;return faithMap;}
 // The explored part of the world, fitted to the panel (never smaller than a quarter of the map either way).
 Rect FaithMapView(Rect panel){int w=world.Width,h=world.MainHeight,x0=w,x1=-1,y0=h,y1=-1;for(int y=0;y<h;y++)for(int x=0;x<w;x++){var t=world.At(x,y);if(t==null||!t.seen)continue;if(x<x0)x0=x;if(x>x1)x1=x;if(y<y0)y0=y;if(y>y1)y1=y;}if(x1<0)return new Rect(0,0,1,1);
  float bw=Mathf.Max(w/4f,x1-x0+5),bh=Mathf.Max(h/4f,y1-y0+5);float aspect=panel.width/panel.height*(world.Hex?1f:.5f);if(bw/bh<aspect)bw=Mathf.Min(w,bh*aspect);else bh=Mathf.Min(h,bw/aspect);float cx=(x0+x1)/2f,cy=(y0+y1)/2f;float left=Mathf.Clamp(cx-bw/2,0,w-bw),top=Mathf.Clamp(cy-bh/2,0,h-bh);return new Rect(left/w,1-(top+bh)/h,bw/w,bh/h);}
 void DrawFaithMedallion(Rect r,int icon){GUI.DrawTexture(r,Medallion());DrawIcon(icon,new Rect(r.x+r.width*.22f,r.y+r.height*.22f,r.width*.56f,r.height*.56f));}
 bool FaithTabButton(Rect r,string text,bool on){if(on){Fill(r,new Color(.78f,.55f,.18f));Fill(new Rect(r.x+2,r.y+2,r.width-4,r.height/2-2),new Color(.93f,.74f,.32f));}else Fill(r,new Color(.08f,.12f,.22f));var g=FaithGold;Fill(new Rect(r.x,r.y,r.width,1),g);Fill(new Rect(r.x,r.yMax-1,r.width,1),g);Fill(new Rect(r.x,r.y,1,r.height),g);Fill(new Rect(r.xMax-1,r.y,1,r.height),g);
  bool hover=r.Contains(Event.current.mousePosition)&&!on;if(hover)Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),new Color(1,1,1,.07f));GUI.Label(r,text,new GUIStyle(hudTitle){fontSize=16,alignment=TextAnchor.MiddleCenter,normal={textColor=on?new Color(.18f,.1f,.02f):FaithInk}});return GUI.Button(r,"",GUIStyle.none);}
 bool FaithActionButton(Rect r,string text,int icon,string why){bool ok=why=="";Fill(r,ok?new Color(.1f,.15f,.27f):new Color(.07f,.09f,.14f));var g=ok?FaithGold:new Color(.45f,.38f,.25f);Fill(new Rect(r.x,r.y,r.width,1),g);Fill(new Rect(r.x,r.yMax-1,r.width,1),g);Fill(new Rect(r.x,r.y,1,r.height),g);Fill(new Rect(r.xMax-1,r.y,1,r.height),g);
  DrawIcon(icon,new Rect(r.x+8,r.y+(r.height-24)/2,24,24));GUI.Label(new Rect(r.x+38,r.y,r.width-44,r.height),text,FaithText(13,TextAnchor.MiddleCenter,false,ok?FaithInk:FaithMuted));
  if(r.Contains(Event.current.mousePosition)){if(ok)Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),new Color(1,1,1,.07f));else faithHint=why;}
  return ok&&GUI.Button(r,"",GUIStyle.none);}
 string faithHint="";
 // ---------- the screen ----------
 void DrawFaithScreen(){
  var full=new Rect(0,38,1280,762);var painting=Art("title-world");Fill(full,new Color(.03f,.05f,.1f,1f));if(painting!=null)GUI.DrawTexture(full,painting,ScaleMode.ScaleAndCrop);Fill(full,new Color(.02f,.03f,.07f,.45f));
  DrawRail();faithHint="";var me=world.nations[0];int state=world.StateReligion(0);var rec=world.Record(state);
  // Header.
  var head=new Rect(80,46,900,88);FaithFrame(head);GUI.DrawTexture(new Rect(head.x+12,head.y+6,76,76),SunEmblem());Fill(new Rect(head.x+98,head.y+12,1,64),new Color(FaithGold.r,FaithGold.g,FaithGold.b,.6f));
  GUI.Label(new Rect(head.x+110,head.y+8,420,42),state>=0?world.FaithName(state):"No faith yet",new GUIStyle(hudTitle){fontSize=30,alignment=TextAnchor.MiddleLeft});
  GUI.Label(new Rect(head.x+112,head.y+50,420,26),state>=0?world.Motto(state):"Faith "+me.faith+" (+"+world.FaithPerTurn(0)+" a turn)",FaithText(16,TextAnchor.MiddleLeft,true));
  {float x=head.x+470;var stats=new[]{new KeyValuePair<int,string>(PixelArt.IconCities,"Followers\n"+(state>=0?(world.WorldShare(state)/10f).ToString("0.0")+"% ("+World.PeopleText(world.WorldFollowers(state))+")":"—")),new KeyValuePair<int,string>(PixelArt.IconFaith,"Faith\n"+me.faith+" (+"+world.FaithPerTurn(0)+")"),new KeyValuePair<int,string>(PixelArt.IconEmpire,"Authority\n"+(rec!=null?rec.authority+" / 100":"—"))};
   foreach(var s in stats){Fill(new Rect(x-8,head.y+16,1,56),new Color(FaithGold.r,FaithGold.g,FaithGold.b,.5f));DrawIcon(s.Key,new Rect(x,head.y+26,34,34));GUI.Label(new Rect(x+40,head.y+14,104,60),s.Value,FaithText(15));x+=142;}}
  {var close=new Rect(head.xMax-30,head.y+6,24,24);Fill(close,new Color(.12f,.16f,.28f));GUI.Label(close,"×",FaithText(18,TextAnchor.MiddleCenter));if(GUI.Button(close,"",GUIStyle.none)){detailsScreen="";GUIUtility.ExitGUI();}}
  // Tabs.
  for(int k=0;k<FaithTabs.Length;k++){var r=new Rect(84+k*149,142,143,32);if(FaithTabButton(r,FaithTabs[k],faithTab==k)){faithTab=k;faithTabScroll=Vector2.zero;}}
  if(faithTab==0)DrawFaithOverview(state,rec);
  else{var body=new Rect(80,182,900,610);FaithFrame(body);GUILayout.BeginArea(new Rect(body.x+16,body.y+12,body.width-26,body.height-22));faithTabScroll=GUILayout.BeginScrollView(faithTabScroll);
   switch(faithTab){case 1:DrawFaithTree();break;case 2:FaithBeliefs();break;default:FaithMore();break;}
   GUILayout.EndScrollView();GUILayout.EndArea();}
  DrawFaithSide(state);
  if(faithHint!="")GUI.Label(new Rect(80,776,900,20),faithHint,FaithText(12,TextAnchor.MiddleCenter,false,new Color(1f,.8f,.5f)));
 }
 void DrawFaithOverview(int state,FaithRecord rec){
  var me=world.nations[0];
  // The holy city before a sunrise.
  {var pic=new Rect(80,182,360,318);FaithFrame(pic);var inner=new Rect(pic.x+6,pic.y+6,pic.width-12,pic.height-12);
   for(int b=0;b<20;b++){float t=b/19f;Fill(new Rect(inner.x,inner.y+t*inner.height*.7f,inner.width,inner.height*.7f/20+1),Color.Lerp(new Color(.32f,.36f,.62f),new Color(.98f,.72f,.42f),t));}
   Fill(new Rect(inner.x,inner.y+inner.height*.7f,inner.width,inner.height*.3f),new Color(.22f,.36f,.18f));
   GUI.DrawTexture(new Rect(inner.center.x-90,inner.y+20,180,180),SunEmblem());
   var holy=rec!=null?world.cities.Find(c=>c.id==rec.holyCity):world.cities.Where(c=>c.owner==0).OrderByDescending(c=>c.pop).FirstOrDefault();
   // A painted holy place for the faith.s core belief (SpriteCook, 8 Nov 2026); the city.s own sprite before a faith exists.
   var painting=rec!=null?Art("faith-holy-"+rec.core):null;if(painting!=null)GUI.DrawTexture(inner,painting,ScaleMode.ScaleAndCrop);
   if(holy!=null){var art=painting!=null?null:CityArt(holy);if(art!=null)GUI.DrawTexture(new Rect(inner.center.x-130,inner.yMax-232,260,260),art,ScaleMode.ScaleToFit,true);
    Fill(new Rect(inner.x,inner.yMax-30,inner.width,30),new Color(.03f,.05f,.1f,.75f));GUI.Label(new Rect(inner.x,inner.yMax-30,inner.width,30),(rec!=null?"Holy city: ":"")+holy.name+(holy.owner!=0?" (held by "+world.NationName(holy.owner)+")":""),FaithText(14,TextAnchor.MiddleCenter));}}
  // Streamlined (owner request, 8 Nov 2026: "too much information"): one plain next step, the faith tree's progress and three
  // actions. Institutions, policy, missions in detail, diplomacy and history moved to the More tab.
  {var box=new Rect(446,182,534,318);FaithFrame(box);FaithTitle(box,"What to do next");var step=world.FaithNextStep(0);
   GUI.Label(new Rect(box.x+18,box.y+52,box.width-36,150),step,FaithText(17,TextAnchor.UpperLeft));
   if(me.founded<0&&world.FoundingReason(0,-1)==""&&FaithActionButton(new Rect(box.x+18,box.yMax-60,box.width-36,40),"Choose your faith's beliefs →",PixelArt.IconFaith,"")){faithTab=2;faithTabScroll=Vector2.zero;}
   if(rec!=null)GUI.Label(new Rect(box.x+18,box.yMax-58,box.width-36,44),"Your faith teaches "+FaithCatalog.Cores[rec.core]+" and "+FaithCatalog.Socials[rec.social]+". Followers: "+(world.WorldShare(state)/10f).ToString("0.0")+"% of the world.",FaithText(13,TextAnchor.UpperLeft,false,FaithMuted));}
  // The faith tree at a glance.
  {var box=new Rect(80,506,628,286);FaithFrame(box);FaithTitle(box,"Faith Tree");
   if(!world.modernRules)GUI.Label(new Rect(box.x+18,box.y+52,box.width-36,60),"The faith tree belongs to games with the modern rules.",FaithText(14,TextAnchor.UpperLeft,false,FaithMuted));
   else{int adopted=Enumerable.Range(0,DoctrineCatalog.Count).Count(d=>world.DoctrineKnown(0,d));
    GUI.Label(new Rect(box.x+18,box.y+50,box.width-36,26),adopted+" of "+DoctrineCatalog.Count+" doctrines adopted. Devotion comes from your faith each turn without spending it.",FaithText(14,TextAnchor.UpperLeft));
    if(me.doctrine>=0){GUI.Label(new Rect(box.x+18,box.y+86,box.width-36,26),"Learning: "+DoctrineCatalog.Names[me.doctrine]+" — "+DoctrineCatalog.EffectText(me.doctrine),FaithText(15,TextAnchor.UpperLeft));
     var bar=new Rect(box.x+18,box.y+120,box.width-36,14);Fill(bar,new Color(.1f,.12f,.18f));Fill(new Rect(bar.x,bar.y,bar.width*Mathf.Clamp01(me.devotion/(float)Mathf.Max(1,DoctrineCatalog.Cost(me.doctrine))),bar.height),FaithGold);
     GUI.Label(new Rect(box.x+18,box.y+138,box.width-36,22),me.devotion+" / "+DoctrineCatalog.Cost(me.doctrine)+" devotion"+(world.DoctrineTurns(0)>0?", about "+world.DoctrineTurns(0)+" turns":""),FaithText(12,TextAnchor.UpperLeft,false,FaithMuted));}
    else GUI.Label(new Rect(box.x+18,box.y+86,box.width-36,40),"No doctrine chosen: devotion is waiting. Pick one in the Faith Tree.",FaithText(15,TextAnchor.UpperLeft,false,new Color(1f,.8f,.5f)));
    if(FaithActionButton(new Rect(box.x+18,box.yMax-60,260,40),"Open the Faith Tree →",PixelArt.IconCivics,"")){faithTab=1;faithTabScroll=Vector2.zero;}}}
  // Faith actions.
  {var box=new Rect(714,506,266,286);FaithFrame(box);FaithTitle(box,"Faith Actions");float y=box.y+48;var r=new Rect(box.x+14,y,box.width-28,44);
   var target=world.cities.Where(c=>c.religion!=me.founded&&world.At(c.x,c.y).seen&&world.MissionReason(0,c)=="").OrderBy(c=>c.owner==0?0:1).ThenByDescending(c=>c.pop).FirstOrDefault();
   string mission=me.founded<0?"Found a faith first.":target==null?"No city within reach needs your missionaries.":"";
   if(FaithActionButton(r,"Send Missionary"+(target!=null?" to "+target.name:""),PixelArt.IconDiplomacy,mission)){world.SendMissionaries(0,target);status="Missionaries set out for "+target.name+".";}r.y+=52;
   var temple=world.cities.Where(c=>c.owner==0&&!world.HasBuilding(c,36)).OrderByDescending(c=>c.faith).FirstOrDefault();string build=temple==null?"Every city has a Temple.":world.FaithBuildReason(temple,world.HasBuilding(temple,World.ShrineBuilding)?36:World.ShrineBuilding);
   if(FaithActionButton(r,temple==null?"Build Temple":(world.HasBuilding(temple,World.ShrineBuilding)?"Build Temple in ":"Build Shrine in ")+temple.name,PixelArt.IconCities,build)){int b=world.HasBuilding(temple,World.ShrineBuilding)?36:World.ShrineBuilding;world.BuildWithFaith(temple,b);status=World.FaithBuildingName(b)+" consecrated in "+temple.name+".";}r.y+=52;
   if(FaithActionButton(r,"Enact Religious Reform",PixelArt.IconChronicle,me.founded<0?"Found a faith first.":"")){faithTab=2;faithTabScroll=Vector2.zero;}r.y+=52;
   if(FaithActionButton(r,"Fund Pilgrimage (40 faith)",PixelArt.IconMove,world.PilgrimageReason(0))){world.FundPilgrimage(0);status="Pilgrims set out for the holy city.";}}
 }
 void DrawFaithSide(int state){
  {var box=new Rect(988,46,284,454);FaithFrame(box);FaithTitle(box,"Religious Summary");GUI.Label(new Rect(box.x+14,box.y+44,box.width-28,box.height-52),world.FaithSummary(state),FaithText(14,TextAnchor.UpperLeft));}
  {var box=new Rect(988,506,284,286);FaithFrame(box);FaithTitle(box,"Faiths of the World");var faiths=Enumerable.Range(0,ReligionCatalog.Count).Where(x=>world.Followers(x)>0||world.Record(x)!=null).OrderByDescending(x=>world.WorldShare(x)).Take(7).ToList();float y=box.y+48;
   if(faiths.Count==0)GUI.Label(new Rect(box.x+14,y,box.width-28,40),"No faith has been founded yet.",FaithText(13,TextAnchor.UpperLeft,false,FaithMuted));
   foreach(int x in faiths){var f=world.Record(x);Fill(new Rect(box.x+14,y+5,12,12),FaithColour(x));GUI.Label(new Rect(box.x+32,y,box.width-90,20),world.FaithName(x),FaithText(13));GUI.Label(new Rect(box.xMax-60,y,48,20),(world.WorldShare(x)/10f).ToString("0.0")+"%",FaithText(13,TextAnchor.MiddleRight));
    var bar=new Rect(box.x+32,y+21,box.width-46,5);Fill(bar,new Color(.1f,.12f,.18f));Fill(new Rect(bar.x,bar.y,bar.width*Mathf.Clamp01(world.WorldShare(x)/1000f),bar.height),FaithColour(x));
    GUI.Label(new Rect(box.x+32,y+26,box.width-46,16),(f!=null?FaithCatalog.Cores[f.core]+" • authority "+f.authority:"")+(world.ReligionFounder(x)!=""?" • "+world.ReligionFounder(x):""),FaithText(10,TextAnchor.UpperLeft,false,FaithMuted));y+=36;}}
 }
}
}
