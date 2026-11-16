using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (owner's resource pack, 16 Nov 2026): "-resourceGallery <folder>" draws every resource miniature at its real
// map size on real ground of four kinds (grassland, desert, mountains, sea) and saves one picture of each, then quits.
// Pack drafts (first-pass art) are labelled in amber, the game's earlier art in white.
public partial class RealmGame {
 Tile galleryGround;string galleryTitle;
 void DrawResourceGallery(){
  Fill(FullCanvas,new Color(.05f,.07f,.12f));
  GUI.Label(new Rect(20,8,1240,30),"RESOURCE GALLERY  —  "+galleryTitle+"   (white: earlier art, amber: owner's pack)",new GUIStyle(hudTitle){fontSize=18});
  var list=Enumerable.Range(1,ResourceRegistry.Count-1).Where(r=>ResourceRegistry.HasArt[r]).ToList();
  const int cols=13;float s=84f/64f,cellW=96,cellH=91;var small=new GUIStyle(hudSmall){fontSize=9,alignment=TextAnchor.UpperCenter,wordWrap=false,clipping=TextClipping.Overflow};
  for(int k=0;k<list.Count;k++){int r=list[k];float sx=16+(k%cols)*cellW+cellW/2,sy=60+(k/cols)*cellH+30;
   GUI.DrawTexture(TileRect(sx,sy,s),IsoGround(galleryGround,galleryGround.x,galleryGround.y,0));
   DrawResourceArt(r,new Rect(sx-36*s,sy+15*s-61*s,61*s,61*s),4);
   var d=ResourceRegistry.Of(r);small.normal.textColor=d!=null&&d.art==ResourceRegistry.Art.Draft?new Color(1f,.78f,.35f):Color.white;
   GUI.Label(new Rect(sx-cellW/2,sy+28,cellW,14),r+" "+(d!=null?d.id:"?"),small);}
 }
 IEnumerator ResourceGalleryRun(string folder){
  Directory.CreateDirectory(folder);yield return Frames(5);
  seedText="421";mainMenu=false;nameSetup=false;guide=false;tutorialOn=false;tutorialStep=-1;StartGame();hasSession=true;guide=false;tutorialOn=false;tutorialStep=-1;yield return Frames(10);
  foreach(var page in new[]{new{t=0,n="grassland"},new{t=3,n="desert"},new{t=5,n="mountains"},new{t=4,n="sea"}}){
   galleryGround=world.tiles.FirstOrDefault(x=>x.realm==0&&x.terrain==page.t)??world.tiles[0];galleryTitle=page.n.ToUpperInvariant();
   yield return Frames(30);yield return Shot(folder,"resources-"+page.n+".bmp");}
  galleryGround=null;
  // In play: every technology known so all resources show, the land around the densest patch of catalogue resources revealed.
  for(int k=0;k<world.nations[0].tech.Length;k++)world.nations[0].tech[k]=true;world.tech=world.nations[0].tech;
  var spots=world.tiles.Where(t=>t.realm==0&&t.resource>=20).ToList();
  var best=spots.OrderByDescending(a=>spots.Count(b=>world.Distance(a.x,a.y,b.x,b.y)<=4)).FirstOrDefault();
  if(best!=null){foreach(var t in world.tiles)if(t.realm==0&&world.Distance(t.x,t.y,best.x,best.y)<=12)t.seen=true;world.Reveal();selected=-1;cityId=-1;
   // Gateways in the revealed land count as already found, so their scene does not cover the picture.
   if(world.gatesFound==null)world.gatesFound=new System.Collections.Generic.List<int>();foreach(var g in world.tiles.Where(t=>t.gateLink>0)){int cell=g.y*world.Width+g.x;if(!world.gatesFound.Contains(cell))world.gatesFound.Add(cell);}
   foreach(int zoom in new[]{128,64}){isoWidth=zoom;FocusTile(best.x,best.y);yield return Frames(10);
    // Revealing the land may meet a rival, which opens the talks; close them for a picture of the map.
    for(int k=0;k<5;k++){world.newContacts.Clear();detailsScreen="";diplomacyChooser=false;gateScene=null;gatePanel=null;yield return null;}
    yield return Frames(30);yield return Shot(folder,"map-resources-"+zoom+".bmp");}}
  Application.Quit();
 }
}
}
