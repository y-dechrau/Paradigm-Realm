using System;
using System.Collections.Generic;
using UnityEngine;
namespace ParadigmRealm {
 public partial class RealmGame {
  readonly Dictionary<string,Texture2D> visualArt=new Dictionary<string,Texture2D>();
  readonly List<Texture2D> themeTextures=new List<Texture2D>();
  GUISkin visualSkin; bool artGallery; int galleryIndex,loadedGallery=-1; string[] galleryNames; Texture2D galleryImage;
  Texture2D Art(string name){Texture2D t;if(!visualArt.TryGetValue(name,out t)){t=Resources.Load<Texture2D>("Visuals/"+name);visualArt[name]=t;}return t;}
  void LoadVisualArt(){
   for(int i=0;i<people.Length;i++){var t=Art("unit-"+i.ToString("00"));if(t!=null){Destroy(people[i]);people[i]=t;}}
   var city=Art("city-0-0");if(city!=null){Destroy(cityTexture);cityTexture=city;}
   var names=Resources.Load<TextAsset>("Visuals/gallery-names");galleryNames=names==null?new string[0]:names.text.Split(new[]{'\n'},StringSplitOptions.RemoveEmptyEntries);
  }
  Texture2D PanelTexture(Color centre,Color edge){
   var t=new Texture2D(16,16,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
   for(int y=0;y<16;y++)for(int x=0;x<16;x++)t.SetPixel(x,y,x==0||y==0||x==15||y==15?edge:centre);
   t.Apply();themeTextures.Add(t);return t;
  }
  void ApplyVisualSkin(){
   if(visualSkin==null){
    visualSkin=Instantiate(GUI.skin);
    var panel=PanelTexture(new Color(.045f,.075f,.085f,.98f),new Color(.48f,.43f,.29f));
    var normal=PanelTexture(new Color(.11f,.18f,.19f),new Color(.44f,.41f,.28f));
    var hover=PanelTexture(new Color(.19f,.28f,.27f),new Color(.85f,.71f,.42f));
    var active=PanelTexture(new Color(.31f,.29f,.17f),new Color(1,.84f,.5f));
    visualSkin.box.normal.background=panel;visualSkin.box.border=new RectOffset(2,2,2,2);
    foreach(var s in new[]{visualSkin.button,visualSkin.textField,visualSkin.textArea}){
     s.normal.background=normal;s.hover.background=hover;s.active.background=active;s.focused.background=hover;
     s.onNormal.background=active;s.onHover.background=hover;s.onActive.background=active;
     s.border=new RectOffset(2,2,2,2);s.normal.textColor=new Color(.93f,.9f,.78f);s.hover.textColor=Color.white;s.active.textColor=Color.white;s.fontSize=14;
    }
    visualSkin.label.normal.textColor=new Color(.9f,.91f,.83f);
   }
   GUI.skin=visualSkin;
  }
  void DrawArt(string name,Rect r){var t=Art(name);if(t!=null)GUI.DrawTexture(r,t,ScaleMode.ScaleToFit,true);}
  int CityVisualEra(int owner){return world.HasTech(owner,"feudalism")?3:world.HasTech(owner,"iron")?2:world.HasTech(owner,"bronze")?1:0;}
  Texture2D CityArt(City c){return Art("city-"+CityVisualEra(c.owner)+"-"+((c.owner*2+c.id)%5))??cityTexture;}
  void DrawCityArt(City c,Rect r){GUI.DrawTexture(r,CityArt(c),ScaleMode.ScaleToFit,true);}
  void DrawTerritory(Tile t,Rect r){
   if(t.owner<0)return;Color ink=nations[t.owner];ink.a=.7f;float w=Mathf.Max(1,r.width/40);
   int[] dx={-1,1,0,0},dy={0,0,-1,1};
   for(int i=0;i<4;i++){var adjacent=world.At(t.x+dx[i],t.y+dy[i]);if(adjacent!=null&&adjacent.seen&&world.KnownTile(adjacent).owner==t.owner)continue;
    var edge=i==0?new Rect(r.x,r.y,w,r.height):i==1?new Rect(r.xMax-w,r.y,w,r.height):i==2?new Rect(r.x,r.y,r.width,w):new Rect(r.x,r.yMax-w,r.width,w);Fill(edge,ink);
   }
  }
  void DrawLandscape(Tile t,Rect r){
   string item=t.terrain==1?"trees":t.terrain==13?"pines":t.terrain==5?"mountain":t.terrain==11||t.terrain==19?"palms":null;
   if(item!=null)DrawArt(item,new Rect(r.x,r.y-r.height*.22f,r.width,r.height*1.22f));
   if(t.farm)DrawArt("farm",r);if(t.road)DrawRoad(t,r);
   if(t.mine)DrawArt("mine",r);if(t.fort)DrawArt("fort",r);
   if(t.village&&world.Visible(t.x,t.y))DrawArt("village",r);
  }
  void DrawUnitArt(Unit u,Rect r,int count){
   GUI.DrawTexture(new Rect(r.x+r.width*.08f,r.y-r.height*.16f,r.width*.84f,r.height*1.12f),people[u.type],ScaleMode.ScaleToFit,true);
   float h=Mathf.Max(3,r.width*.06f);Fill(new Rect(r.x+r.width*.12f,r.yMax-h-1,r.width*.76f,h),new Color(.03f,.04f,.04f,.95f));
   Fill(new Rect(r.x+r.width*.12f,r.yMax-h-1,r.width*.76f*u.hp/10f,h),u.hp>5?new Color(.37f,.78f,.45f):new Color(.92f,.45f,.27f));
   Fill(new Rect(r.x+2,r.y+2,Mathf.Max(4,r.width*.1f),Mathf.Max(4,r.width*.1f)),nations[u.owner]);
   if(count>1){Fill(new Rect(r.xMax-18,r.y,18,18),new Color(.03f,.05f,.06f,.9f));GUI.Label(new Rect(r.xMax-16,r.y-1,22,18),count.ToString(),small);}
  }
  void DrawSelection(Rect r){
   Color c=new Color(1,.87f,.48f);float l=r.width*.25f;
   foreach(float x in new[]{r.x,r.xMax-l})foreach(float y in new[]{r.y,r.yMax-2})Fill(new Rect(x,y,l,2),c);
   foreach(float x in new[]{r.x,r.xMax-2})foreach(float y in new[]{r.y,r.yMax-l})Fill(new Rect(x,y,2,l),c);
  }
  void DrawGallery(){
   Fill(new Rect(0,0,1280,800),new Color(.025f,.04f,.05f));GUI.Label(new Rect(35,20,900,35),"THE ART OF PARADIGM REALM",title);
   if(galleryNames.Length==0){artGallery=false;return;}
   galleryIndex=(galleryIndex+galleryNames.Length)%galleryNames.Length;
   if(loadedGallery!=galleryIndex){if(galleryImage!=null)Resources.UnloadAsset(galleryImage);galleryImage=Resources.Load<Texture2D>("Visuals/Gallery/"+galleryIndex.ToString("000"));loadedGallery=galleryIndex;}
   if(galleryImage!=null)GUI.DrawTexture(new Rect(30,70,1220,625),galleryImage,ScaleMode.ScaleToFit,true);
   GUI.Label(new Rect(250,705,780,30),(galleryIndex+1)+" / "+galleryNames.Length+"   "+galleryNames[galleryIndex],heading);
   if(GUI.Button(new Rect(35,740,180,38),"Previous",button))galleryIndex--;
   if(GUI.Button(new Rect(230,740,180,38),"Next",button))galleryIndex++;
   if(GUI.Button(new Rect(1045,740,200,38),"Back to menu",button)){artGallery=false;if(galleryImage!=null)Resources.UnloadAsset(galleryImage);galleryImage=null;loadedGallery=-1;}
  }
  void ReleaseVisuals(){foreach(var t in themeTextures)Destroy(t);if(visualSkin!=null)Destroy(visualSkin);if(galleryImage!=null)Resources.UnloadAsset(galleryImage);}
 }
}
