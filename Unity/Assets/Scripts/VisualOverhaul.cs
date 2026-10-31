using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
 public partial class RealmGame {
  readonly Dictionary<string,Texture2D> visualArt=new Dictionary<string,Texture2D>();
  readonly List<Texture2D> themeTextures=new List<Texture2D>();
  GUISkin visualSkin; bool artGallery; int galleryIndex,loadedGallery=-1; string[] galleryNames; Texture2D galleryImage;
  // Dark navy and gold theme (owner's mockup, 30 Oct 2026): deep navy panels framed in gold, cream text, teal for progress
  // and the active button.
  static readonly Color UiBackground=new Color(.04f,.065f,.11f),UiPanel=new Color(.065f,.1f,.165f,.95f),UiPanelEdge=new Color(.72f,.58f,.32f),
   UiButton=new Color(.11f,.165f,.25f),UiButtonHover=new Color(.16f,.24f,.35f),UiButtonActive=new Color(.1f,.4f,.46f),UiAccent=new Color(.27f,.78f,.82f),
   UiText=new Color(.94f,.9f,.8f),UiLabelBack=new Color(.065f,.1f,.165f,.93f),UiDim=new Color(0,0,0,.55f),UiTrough=new Color(.03f,.05f,.09f),
   UiGold=new Color(.86f,.7f,.39f),UiMuted=new Color(.66f,.68f,.7f);
  // Processed copies of the supplied unit pictures (the files themselves are untouched); null falls back to the original.
  Texture2D[] unitSprites;
  // Leader portraits (Resources/Leaders, scaled copies of the owner's leader art). An age-specific picture
  // "leader-NN-eE" (E = 0 Dawn ... 5 Dimensional), dressed for that age, is used when it exists; otherwise "leader-NN".
  readonly Dictionary<int,Texture2D> portraits=new Dictionary<int,Texture2D>();
  Texture2D Portrait(int i,int era=-1){
   if(i<0||i>=CivilisationCatalog.PortraitCount)return null;Texture2D t;
   // An age without its own picture borrows the nearest earlier one in its group (owner request, 31 Oct): Dawn and
   // Classical share the ancient look (e0), Industrial onwards the modern look (e3); the Medieval Age uses the base portrait.
   if(era>=0){for(int e=era;e>=(era>=3?3:era==2?2:0);e--){int key=1000+i*10+e;if(!portraits.TryGetValue(key,out t)){t=Resources.Load<Texture2D>("Leaders/leader-"+i.ToString("00")+"-e"+e);portraits[key]=t;}if(t!=null)return t;}}
   if(!portraits.TryGetValue(i,out t)){t=Resources.Load<Texture2D>("Leaders/leader-"+i.ToString("00"));portraits[i]=t;}return t;
  }
  // The portraits are 3:4 pictures, but Unity imports them padded to power-of-two sizes (300 x 400 arrives as 256 x 512), so
  // their texture size cannot be trusted for shape. Every portrait is drawn at 3:4: crop fills the box keeping the top (the
  // face), otherwise the whole picture is fitted inside the box.
  const float PortraitAspect=300f/400f;
  void DrawPortrait(Rect r,Texture2D tex,bool crop=true){
   if(tex==null)return;float ra=r.width/r.height;
   if(!crop){var fit=ra>PortraitAspect?new Rect(r.center.x-r.height*PortraitAspect/2,r.y,r.height*PortraitAspect,r.height):new Rect(r.x,r.center.y-r.width/PortraitAspect/2,r.width,r.width/PortraitAspect);GUI.DrawTexture(fit,tex,ScaleMode.StretchToFill,true);return;}
   if(ra>PortraitAspect){float vh=PortraitAspect/ra;GUI.DrawTextureWithTexCoords(r,tex,new Rect(0,1-vh,1,vh));}
   else{float uw=ra/PortraitAspect;GUI.DrawTextureWithTexCoords(r,tex,new Rect((1-uw)/2,0,uw,1));}
  }
  // Civilisations waiting for portrait art join the game as soon as their picture is in Resources/Leaders.
  void EnableCivilisationsWithArt(){for(int i=0;i<CivilisationCatalog.Count;i++)if(CivilisationCatalog.AwaitingArt[i]&&Portrait(CivilisationCatalog.Portraits[i])!=null){CivilisationCatalog.Enabled[i]=true;CivilisationCatalog.AwaitingArt[i]=false;}}
  Texture2D Art(string name){Texture2D t;if(!visualArt.TryGetValue(name,out t)){t=Resources.Load<Texture2D>("Visuals/"+name);visualArt[name]=t;}return t;}
  void LoadVisualArt(){
   unitSprites=new Texture2D[people.Length];
   for(int i=0;i<people.Length;i++){var t=Art("unit-"+i.ToString("00"));if(t!=null){Destroy(people[i]);people[i]=t;}
    try{unitSprites[i]=PixelTexture(PixelArt.UnitSprite(ReadArtPixels(people[i]),people[i].width,people[i].height));}catch(Exception){unitSprites[i]=null;}}
   // Vessels are drawn by the pixel-art generator (no supplied art); the same picture serves the build list.
   for(int i=World.BoatUnit;i<=World.WarGalleyUnit&&i<people.Length;i++){if(Art("unit-"+i.ToString("00"))!=null)continue;unitSprites[i]=PixelTexture(PixelArt.ShipSprite(i-World.BoatUnit));people[i]=unitSprites[i];}
   EnableCivilisationsWithArt();
   var city=Art("city-0-0");if(city!=null){Destroy(cityTexture);cityTexture=city;}
   var names=Resources.Load<TextAsset>("Visuals/gallery-names");galleryNames=names==null?new string[0]:names.text.Split(new[]{'\n'},StringSplitOptions.RemoveEmptyEntries);
  }
  // Copies a texture's pixels through the GPU, so the supplied art needs no Read/Write import setting and stays unchanged.
  // Returns top-down 0xAARRGGBB, the layout PixelArt uses.
  uint[] ReadArtPixels(Texture2D src){
   var rt=RenderTexture.GetTemporary(src.width,src.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
   var previous=RenderTexture.active;Graphics.Blit(src,rt);RenderTexture.active=rt;
   var copy=new Texture2D(src.width,src.height,TextureFormat.RGBA32,false);copy.ReadPixels(new Rect(0,0,src.width,src.height),0,0);copy.Apply();
   RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);
   var px=copy.GetPixels32();Destroy(copy);var result=new uint[src.width*src.height];
   for(int y=0;y<src.height;y++)for(int x=0;x<src.width;x++){var c=px[(src.height-1-y)*src.width+x];result[y*src.width+x]=((uint)c.a<<24)|((uint)c.r<<16)|((uint)c.g<<8)|c.b;}
   return result;
  }
  // 16 x 16 nine-slice panel: slightly rounded corners, a 1 px gold edge with a dark line just inside it (the framed look of
  // the mockup) and, for buttons, a gentle top-to-bottom sheen.
  Texture2D PanelTexture(Color centre,Color edge,bool shine=false){
   var t=new Texture2D(16,16,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
   for(int y=0;y<16;y++)for(int x=0;x<16;x++){
    float cx=x<3?2.5f-x:x>12?x-12.5f:0,cy=y<3?2.5f-y:y>12?y-12.5f:0,d=Mathf.Sqrt(cx*cx+cy*cy);
    Color c;if(d>2.9f)c=Color.clear;else if(d>1.9f||(cx==0&&(y==0||y==15))||(cy==0&&(x==0||x==15)))c=edge;
    else if(d>.9f||(cx==0&&(y==1||y==14))||(cy==0&&(x==1||x==14)))c=new Color(centre.r*.45f,centre.g*.45f,centre.b*.45f,1);
    else{c=centre;if(shine)c=Color.Lerp(centre*.85f,Color.Lerp(centre,Color.white,.12f),y/15f);c.a=centre.a;}
    t.SetPixel(x,y,c);}
   t.Apply();themeTextures.Add(t);return t;
  }
  void ApplyVisualSkin(){
   if(visualSkin==null){
    visualSkin=Instantiate(GUI.skin);
    var panel=PanelTexture(UiPanel,UiPanelEdge);
    var normal=PanelTexture(UiButton,UiPanelEdge,true);
    var hover=PanelTexture(UiButtonHover,UiGold,true);
    var active=PanelTexture(UiButtonActive,UiAccent,true);
    visualSkin.box.normal.background=panel;visualSkin.box.border=new RectOffset(5,5,5,5);visualSkin.box.normal.textColor=UiText;
    foreach(var s in new[]{visualSkin.button,visualSkin.textField,visualSkin.textArea}){
     s.normal.background=normal;s.hover.background=hover;s.active.background=active;s.focused.background=hover;
     s.onNormal.background=active;s.onHover.background=hover;s.onActive.background=active;
     s.border=new RectOffset(5,5,5,5);s.fontSize=14;
     foreach(var state in new[]{s.normal,s.hover,s.active,s.focused,s.onNormal,s.onHover,s.onActive})state.textColor=UiText;
    }
    foreach(var s in new[]{visualSkin.label,visualSkin.toggle})foreach(var state in new[]{s.normal,s.hover,s.active,s.onNormal,s.onHover,s.onActive})state.textColor=UiText;
    // Scrollbars and sliders: pale troughs with rounded blue-grey thumbs instead of Unity's default dark ones.
    var trough=PanelTexture(UiTrough,UiTrough);var thumb=PanelTexture(new Color(.2f,.27f,.38f),new Color(.45f,.37f,.22f));var thumbHover=PanelTexture(new Color(.26f,.35f,.48f),UiGold);
    foreach(var s in new[]{visualSkin.verticalScrollbar,visualSkin.horizontalScrollbar,visualSkin.horizontalSlider,visualSkin.verticalSlider}){s.normal.background=trough;s.border=new RectOffset(5,5,5,5);}
    foreach(var s in new[]{visualSkin.verticalScrollbarThumb,visualSkin.horizontalScrollbarThumb,visualSkin.horizontalSliderThumb,visualSkin.verticalSliderThumb}){s.normal.background=thumb;s.hover.background=thumbHover;s.active.background=thumbHover;s.border=new RectOffset(5,5,5,5);}
   }
   GUI.skin=visualSkin;
  }
  void DrawArt(string name,Rect r){var t=Art(name);if(t!=null)GUI.DrawTexture(r,t,ScaleMode.ScaleToFit,true);}
  int CityVisualEra(int owner){return world.HasTech(owner,"feudalism")?3:world.HasTech(owner,"iron")?2:world.HasTech(owner,"bronze")?1:0;}
  Texture2D CityArt(City c){return Art("city-"+CityVisualEra(c.owner)+"-"+((c.owner*2+c.id)%5))??cityTexture;}
  // Cities grow on the map (owner request): the settlement is built from its age, size and buildings (PixelArt.Settlement).
  // The supplied city pictures (CityArt) stay in the project but are no longer drawn on the map.
  readonly Dictionary<long,Texture2D> settlementTextures=new Dictionary<long,Texture2D>();
  int SettlementFlags(City c){
   int f=0;if(c.walls)f|=PixelArt.FlagWalls;
   bool capital=c.owner>=0&&c.owner<world.nations.Length&&world.nations[c.owner].capital==c.id;
   if(capital&&c.pop>=4||c.walls&&c.pop>=8)f|=PixelArt.FlagCastle;
   if(world.HasBuilding(c,36))f|=PixelArt.FlagTemple;if(world.HasBuilding(c,31))f|=PixelArt.FlagMarket;
   if(c.granary)f|=PixelArt.FlagGranary;if(c.library)f|=PixelArt.FlagLibrary;return f;
  }
  Texture2D SettlementTexture(int style,int pop,int flags,int variant){
   long key=((long)style<<24)|((long)Math.Min(16,pop)<<16)|((long)flags<<4)|(long)(variant&7);Texture2D tex;
   if(!settlementTextures.TryGetValue(key,out tex)){
    // Supplied art first: city-<age>-<size band>[-walled], size bands 0 (1-3), 1 (4-7), 2 (8-11), 3 (12 and over).
    int band=pop<=3?0:pop<=7?1:pop<=11?2:3;string name="city-"+style+"-"+band;bool walled=(flags&PixelArt.FlagWalls)!=0;
    tex=(walled?SpriteSlot(name+"-walled"):null)??SpriteSlot(name);
    if(tex==null)tex=PixelTexture(PixelArt.Settlement(style,pop,flags,variant));settlementTextures[key]=tex;}
   return tex;
  }
  // Art slots: pictures in Assets/Resources/Sprites (for example from SpriteCook) replace the drawn ones by name. Missing
  // names fall back to the procedural art, so slots can be filled one at a time. Naming: Design/ART-SLOTS.md.
  readonly Dictionary<string,Texture2D> spriteSlots=new Dictionary<string,Texture2D>();
  Texture2D SpriteSlot(string name){Texture2D t;if(!spriteSlots.TryGetValue(name,out t)){t=Resources.Load<Texture2D>("Sprites/"+name);if(t!=null)t.filterMode=FilterMode.Point;spriteSlots[name]=t;}return t;}
  void DrawCityArt(City c,Rect r){GUI.DrawTexture(r,SettlementTexture(CityVisualEra(c.owner),c.pop,SettlementFlags(c),c.id),ScaleMode.ScaleToFit,true);}
  // Outlying farmsteads: from size 5, one neighbouring land tile of the city's own territory per two citizens (up to 8)
  // gets a few houses, chosen in a fixed order per city so they do not jump about.
  readonly Dictionary<int,City> suburbCells=new Dictionary<int,City>();
  static readonly int[] SuburbDx={-1,0,1,1,1,0,-1,-1},SuburbDy={-1,-1,-1,0,1,1,1,0};
  readonly Dictionary<int,City> portCells=new Dictionary<int,City>();
  void BuildSuburbs(){
   suburbCells.Clear();portCells.Clear();
   foreach(var c in world.cities){var p=world.PortTile(c);if(p!=null&&!(p.x==c.x&&p.y==c.y))portCells[p.y*world.Width+p.x]=c;}
   foreach(var c in world.cities){
    int want=Math.Min(8,Math.Max(0,(c.pop-3)/2));if(want==0)continue;
    var order=Enumerable.Range(0,8).OrderBy(k=>(k*7+c.id*3)%8).ToList();
    foreach(int k in order){if(want==0)break;var t=world.Step(c.x,c.y,SuburbDx[k],SuburbDy[k]);if(t==null)continue;int cell=t.y*world.Width+t.x;
     if(t.owner!=c.owner||suburbCells.ContainsKey(cell)||portCells.ContainsKey(cell)||World.Barren(t.terrain)||t.terrain==1||t.terrain==11||t.terrain==13||t.terrain==17||t.terrain==19||world.cities.Any(o=>o.x==t.x&&o.y==t.y))continue;
     suburbCells[cell]=c;want--;}
   }
  }
  // City name plates under each settlement: a light rounded plate with a strip in the owner's colour (owner request).
  readonly List<string> isoCityNames=new List<string>();readonly List<Vector2> isoCityPos=new List<Vector2>();readonly List<Color> isoCityInk=new List<Color>();readonly List<City> isoCityRefs=new List<City>();

  Texture2D HamletTexture(City c,int x,int y){return IsoPicture(6,CityVisualEra(c.owner),(x*5+y*3)&7);}
  void DrawTerritory(Tile t,Rect r){
   if(t.owner<0)return;Color ink=nations[t.owner];ink.a=.7f;float w=Mathf.Max(1,r.width/40);
   int[] dx={-1,1,0,0},dy={0,0,-1,1};
   for(int i=0;i<4;i++){var adjacent=world.Step(t.x,t.y,dx[i],dy[i]);if(adjacent!=null&&adjacent.seen&&world.KnownOwner(adjacent)==t.owner)continue;
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
  // r is the unit's box on the tile. The figure is drawn 1.6 times its width (64 px at the default zoom, so pixels stay
  // whole), standing on a soft ground shadow inside a ring in its owner's colour.
  void DrawUnitArt(Unit u,Rect r,int count){
   float size=r.width*1.6f;var feet=new Vector2(r.center.x,r.yMax);
   var baseRect=new Rect(feet.x-size*.3f,feet.y-size*.09f,size*.6f,size*.225f);
   GUI.DrawTexture(baseRect,IsoPicture(4,8,0));var old=GUI.color;GUI.color=nations[u.owner];GUI.DrawTexture(baseRect,IsoPicture(4,7,0));GUI.color=old;
   var art=unitSprites!=null&&unitSprites[u.type]!=null?unitSprites[u.type]:people[u.type];
   // Armies carry a banner in their owner's colour behind them, as in the owner's mockup.
   if(World.Power[u.type]>0&&!World.IsNaval(u.type)){float px=feet.x+size*.26f,top=feet.y-size*1.02f,pole=Mathf.Max(1,size*.025f);
    Fill(new Rect(px,top,pole,size*.9f),new Color(.32f,.22f,.12f));var ink=nations[u.owner];var flag=new Rect(px+pole,top+size*.02f,size*.2f,size*.24f);
    var flagArt=FlagTexture(u.owner);if(flagArt!=null){flag.width=flag.height*1.5f;GUI.DrawTexture(flag,flagArt,ScaleMode.StretchToFill,true);}else Fill(flag,ink*.75f+new Color(0,0,0,.25f));}
   GUI.DrawTexture(new Rect(feet.x-size/2,feet.y-size*62f/64f,size,size),art,ScaleMode.ScaleToFit,true);
   // Health shows above the head, and only when the unit is hurt, so the map stays clean.
   if(u.hp<10){float h=Mathf.Max(3,r.width*.07f),bar=r.width*.8f,by=feet.y-size*60f/64f-h;
    Fill(new Rect(feet.x-bar/2-1,by-1,bar+2,h+2),new Color(.08f,.1f,.12f,.85f));
    Fill(new Rect(feet.x-bar/2,by,bar*u.hp/10f,h),u.hp>5?new Color(.37f,.8f,.47f):new Color(.95f,.47f,.28f));}
   if(count>1){var badge=new Rect(feet.x+size*.22f,feet.y-size*.92f,18,18);Fill(badge,UiLabelBack);GUI.Label(new Rect(badge.x+3,badge.y,22,18),count.ToString(),small);}
  }
  // While dragging a unit to a destination: a dotted gold line from the unit to the pointer, with a ring at the end.
  void DrawOrderLine(Vector2 from,Vector2 to){
   float length=(to-from).magnitude;if(length<4)return;var dir=(to-from)/length;
   for(float d=6;d<length-6;d+=9){var p=from+dir*d;Fill(new Rect(p.x-2.5f,p.y-2.5f,5,5),new Color(.1f,.08f,.04f,.75f));Fill(new Rect(p.x-1.5f,p.y-1.5f,3,3),new Color(1f,.84f,.3f));}
   Fill(new Rect(to.x-7,to.y-7,14,2),new Color(1f,.84f,.3f));Fill(new Rect(to.x-7,to.y+5,14,2),new Color(1f,.84f,.3f));Fill(new Rect(to.x-7,to.y-7,2,14),new Color(1f,.84f,.3f));Fill(new Rect(to.x+5,to.y-7,2,14),new Color(1f,.84f,.3f));
  }
  void DrawSelection(Rect r){
   Color c=new Color(1,.87f,.48f);float l=r.width*.25f;
   foreach(float x in new[]{r.x,r.xMax-l})foreach(float y in new[]{r.y,r.yMax-2})Fill(new Rect(x,y,l,2),c);
   foreach(float x in new[]{r.x,r.xMax-2})foreach(float y in new[]{r.y,r.yMax-l})Fill(new Rect(x,y,2,l),c);
  }
  void DrawGallery(){
   Fill(new Rect(0,0,1280,800),UiBackground);GUI.Label(new Rect(35,20,900,35),"THE ART OF PARADIGM REALM",title);
   if(galleryNames.Length==0){artGallery=false;return;}
   galleryIndex=(galleryIndex+galleryNames.Length)%galleryNames.Length;
   if(loadedGallery!=galleryIndex){if(galleryImage!=null)Resources.UnloadAsset(galleryImage);galleryImage=Resources.Load<Texture2D>("Visuals/Gallery/"+galleryIndex.ToString("000"));loadedGallery=galleryIndex;}
   if(galleryImage!=null)GUI.DrawTexture(new Rect(30,70,1220,625),galleryImage,ScaleMode.ScaleToFit,true);
   GUI.Label(new Rect(250,705,780,30),(galleryIndex+1)+" / "+galleryNames.Length+"   "+galleryNames[galleryIndex],heading);
   if(GUI.Button(new Rect(35,740,180,38),"Previous",button))galleryIndex--;
   if(GUI.Button(new Rect(230,740,180,38),"Next",button))galleryIndex++;
   if(GUI.Button(new Rect(1045,740,200,38),"Back to menu",button)){artGallery=false;if(galleryImage!=null)Resources.UnloadAsset(galleryImage);galleryImage=null;loadedGallery=-1;}
  }
  void ReleaseVisuals(){if(unitSprites!=null)foreach(var t in unitSprites)if(t!=null)Destroy(t);foreach(var t in themeTextures)Destroy(t);if(visualSkin!=null)Destroy(visualSkin);if(galleryImage!=null)Resources.UnloadAsset(galleryImage);}
 }
}
