using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// "The Chronicle of Discovery": the research screen after the owner's mockup (13 Nov 2026). A full-screen page: a header with
// the compass logo and title; gold age tabs; the technologies of one age as framed cards on a starry field, joined by arrows
// (solid gold from a known technology, dashed amber otherwise); on the right the chosen technology in a medallion with its
// benefits, progress and actions; along the bottom the research queue, "Add technology" and the zoom. Card art: the owner's
// painted icons (Resources/UI/Tech); frames: Resources/UI/panel and frame; medallion and logo: Resources/UI. Tabs, pills,
// buttons, bars, the star field and arrowheads are drawn here at first use. Fonts: Cinzel for titles, EB Garamond (OFL) for
// names and text, with its italic.
public partial class RealmGame {
 bool techUnlocksView;Vector2 techUnlockScroll;
 const float CardW=272,CardH=158,CardDoneH=112,ColPitch=322,RowPitch=180,TreeLeft=22,TreeTop=18;
 static readonly string[] EraLines={
  "From scattered tribes, the first harvests give rise to civilisation.",
  "Law, coin and philosophy bind cities into empires.",
  "Faith, feud and the printed word reshape the realm.",
  "Steam and steel turn workshops into nations.",
  "Wires and atoms carry thought around the world.",
  "Beyond the gates, the bounds of the possible dissolve."};
 // ---------- fonts and styles ----------
 Font garamond,garamondBold,garamondItalic;GUIStyle chTitle,chSub,chBrand,chTab,chName,chPill,chBullet,chItalic,chPanelTitle,chHead,chBody,chGoldButton,chNavyButton,chBarText,chSmallCaps;
 void ChronicleOfDiscoveryStyles(){
  if(chTitle!=null)return;
  garamond=Resources.Load<Font>("Fonts/EBGaramond-Regular");garamondBold=Resources.Load<Font>("Fonts/EBGaramond-SemiBold");garamondItalic=Resources.Load<Font>("Fonts/EBGaramond-Italic");
  var cream=new Color(.96f,.92f,.82f);var gold=new Color(.93f,.77f,.44f);var muted=new Color(.84f,.82f,.76f);
  GUIStyle Make(Font f,int size,Color c,TextAnchor a=TextAnchor.MiddleLeft,FontStyle fs=FontStyle.Normal){var s=new GUIStyle(GUI.skin.label){font=f,fontSize=size,alignment=a,fontStyle=fs,wordWrap=false,clipping=TextClipping.Overflow,padding=new RectOffset(0,0,0,0),margin=new RectOffset(0,0,0,0)};s.normal.textColor=c;return s;}
  // Without a supplied italic the default font is slanted instead.
  var italicFont=garamondItalic;FontStyle italic=italicFont==null?FontStyle.Italic:FontStyle.Normal;
  chTitle=Make(displayFont,29,gold,TextAnchor.MiddleCenter);chSub=Make(italicFont,19,cream,TextAnchor.MiddleCenter,italic);chBrand=Make(displayFont,17,cream);
  chTab=Make(garamond,19,cream,TextAnchor.MiddleCenter);chName=Make(garamondBold??garamond,22,cream);chPill=Make(displayFont,10,cream,TextAnchor.MiddleCenter);
  chBullet=Make(garamond,13,muted);chBullet.wordWrap=true;chBullet.clipping=TextClipping.Clip;chBullet.alignment=TextAnchor.UpperLeft;
  chItalic=Make(italicFont,14,muted,TextAnchor.MiddleCenter,italic);chPanelTitle=Make(displayFont,24,cream,TextAnchor.MiddleCenter);chHead=Make(displayFont,13,gold);
  chBody=Make(garamond,15,cream);chBody.wordWrap=true;chSmallCaps=Make(displayFont,13,new Color(.2f,.12f,.04f),TextAnchor.MiddleCenter);
  chGoldButton=Make(garamond,17,cream,TextAnchor.MiddleCenter);chNavyButton=Make(garamond,17,cream,TextAnchor.MiddleCenter);chBarText=Make(garamond,13,cream,TextAnchor.MiddleCenter);
 }
 static GUIStyle Sized(GUIStyle s,float z){return new GUIStyle(s){fontSize=Math.Max(6,Mathf.RoundToInt(s.fontSize*z))};}
 void ShadowLabel(Rect r,string text,GUIStyle s){var keep=s.normal.textColor;s.normal.textColor=new Color(0,0,0,.6f);GUI.Label(new Rect(r.x+1,r.y+1,r.width,r.height),text,s);s.normal.textColor=keep;GUI.Label(r,text,s);}
 // ---------- drawn pieces (made once) ----------
 readonly Dictionary<string,Texture2D> chronicleArt=new Dictionary<string,Texture2D>();
 Texture2D Drawn(string key,int w,int h,Func<float,float,Color> pixel){
  Texture2D t;if(chronicleArt.TryGetValue(key,out t)&&t!=null)return t;
  t=new Texture2D(w,h,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};var px=new Color[w*h];
  // Four samples a pixel for smooth edges.
  for(int y=0;y<h;y++)for(int x=0;x<w;x++){Color c=Color.clear;for(int s=0;s<4;s++){var v=pixel(x+.25f+.5f*(s&1),y+.25f+.5f*(s>>1));c+=v;}px[(h-1-y)*w+x]=c/4f;}
  t.SetPixels(px);t.Apply();chronicleArt[key]=t;return t;}
 static Color Mix(Color a,Color b,float t){return Color.Lerp(a,b,Mathf.Clamp01(t));}
 // Distance outside a rounded rectangle (negative inside).
 static float RoundDist(float x,float y,float w,float h,float r){float dx=Mathf.Max(Mathf.Abs(x-w/2)-(w/2-r),0),dy=Mathf.Max(Mathf.Abs(y-h/2)-(h/2-r),0);return Mathf.Sqrt(dx*dx+dy*dy)-r;}
 static readonly Color GoldTop=new Color(.98f,.84f,.5f),GoldBottom=new Color(.66f,.46f,.17f),GoldEdge=new Color(.42f,.27f,.08f),NavyFill=new Color(.06f,.1f,.19f),NavyEdge=new Color(.78f,.62f,.32f);
 Texture2D RoundBox(string key,int w,int h,float r,bool gold){
  return Drawn("box-"+key,w,h,(x,y)=>{float d=RoundDist(x,y,w,h,r);if(d>0)return Color.clear;
   if(gold){if(d>-2)return GoldEdge;var c=Mix(GoldTop,GoldBottom,y/h);if(y<4)c=Mix(c,Color.white,.25f);return c;}
   return d>-2?NavyEdge:Mix(NavyFill,new Color(.04f,.07f,.14f),y/h);});}
 // An age tab: a bar with pointed ends.
 Texture2D TabArt(bool chosen){const int w=256,h=48;const float slant=20;
  return Drawn(chosen?"tab-on":"tab-off",w,h,(x,y)=>{float k=Mathf.Abs(2*y/h-1);float left=slant*k,right=w-slant*k;if(x<left||x>right||y<0||y>h)return Color.clear;
   float edge=Mathf.Min(Mathf.Min(x-left,right-x)*.7f,Mathf.Min(y,h-y));
   if(chosen){if(edge<2.2f)return GoldEdge;var c=Mix(GoldTop,GoldBottom,y/h);return y<6?Mix(c,Color.white,.2f):c;}
   return edge<2f?NavyEdge:Mix(new Color(.07f,.11f,.21f),new Color(.04f,.07f,.14f),y/h);});}
 Texture2D Diamond(){return Drawn("diamond",16,16,(x,y)=>Mathf.Abs(x-8)+Mathf.Abs(y-8)<=7?(Mathf.Abs(x-8)+Mathf.Abs(y-8)>5?GoldEdge:new Color(.95f,.8f,.48f)):Color.clear);}
 Texture2D Arrowhead(){return Drawn("arrow",16,16,(x,y)=>x>=2&&x<=14&&Mathf.Abs(y-8)<=(14-x)*.55f?Color.white:Color.clear);}
 Texture2D CheckArt(){return Drawn("check",32,32,(x,y)=>{float d=Mathf.Sqrt((x-16)*(x-16)+(y-16)*(y-16));if(d>15)return Color.clear;
   bool tick=SegDist(x,y,8,17,13.5f,22.5f)<2.2f||SegDist(x,y,13.5f,22.5f,24,10)<2.2f;if(tick)return Color.white;return d>13.5f?new Color(.75f,.93f,.72f):new Color(.16f,.55f,.27f);});}
 Texture2D LockArt(){return Drawn("lock",32,32,(x,y)=>{bool body=x>=8&&x<=24&&y>=14&&y<=28&&RoundDist(x-8,y-14,16,14,3)<=0;float r=Mathf.Sqrt((x-16)*(x-16)+(y-14)*(y-14));bool shackle=y<=14&&r>=5&&r<=8;
   if(body){bool hole=Mathf.Abs(x-16)<1.6f&&y>=19&&y<=24;return hole?new Color(.15f,.12f,.08f):new Color(.93f,.87f,.72f);}return shackle?new Color(.93f,.87f,.72f):Color.clear;});}
 static float SegDist(float px,float py,float ax,float ay,float bx,float by){float vx=bx-ax,vy=by-ay,t=Mathf.Clamp01(((px-ax)*vx+(py-ay)*vy)/(vx*vx+vy*vy));float dx=px-(ax+t*vx),dy=py-(ay+t*vy);return Mathf.Sqrt(dx*dx+dy*dy);}
 // A navy night with stars and a few faint constellations.
 Texture2D StarField(){const int w=640,h=384;var rng=new System.Random(7);var stars=new List<Vector3>();for(int i=0;i<260;i++)stars.Add(new Vector3((float)rng.NextDouble()*w,(float)rng.NextDouble()*h,(float)rng.NextDouble()));
  var lines=new List<Vector4>();for(int c=0;c<6;c++){float cx=(float)rng.NextDouble()*w,cy=(float)rng.NextDouble()*h;for(int k=0;k<4;k++){float nx=cx+((float)rng.NextDouble()-.5f)*110,ny=cy+((float)rng.NextDouble()-.5f)*80;lines.Add(new Vector4(cx,cy,nx,ny));stars.Add(new Vector3(nx,ny,.9f));cx=nx;cy=ny;}}
  var px=new Color[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++){float t=y/(float)h;var c=Mix(new Color(.055f,.085f,.17f),new Color(.035f,.055f,.12f),t);float vign=Mathf.Clamp01(Mathf.Sqrt((x-w/2f)*(x-w/2f)/(w*w*.3f)+(y-h/2f)*(y-h/2f)/(h*h*.3f)));c=Mix(c,new Color(.025f,.04f,.09f),vign*.5f);px[(h-1-y)*w+x]=c;}
  foreach(var l in lines){int n=(int)(new Vector2(l.z-l.x,l.w-l.y).magnitude);for(int s=0;s<n;s++){int x=(int)(l.x+(l.z-l.x)*s/n),y=(int)(l.y+(l.w-l.y)*s/n);if(x<0||y<0||x>=w||y>=h)continue;int i=(h-1-y)*w+x;px[i]=Mix(px[i],new Color(.45f,.5f,.65f),.12f);}}
  foreach(var s in stars){int x=(int)s.x,y=(int)s.y;if(x<1||y<1||x>=w-1||y>=h-1)continue;float b=.25f+.6f*s.z;px[(h-1-y)*w+x]=Mix(px[(h-1-y)*w+x],new Color(.85f,.88f,1f),b);if(s.z>.85f){foreach(var o in new[]{-1,1}){px[(h-1-y)*w+x+o]=Mix(px[(h-1-y)*w+x+o],new Color(.8f,.84f,1f),b*.4f);px[(h-1-y-o)*w+x]=Mix(px[(h-1-y-o)*w+x],new Color(.8f,.84f,1f),b*.4f);}}}
  Texture2D sky;if(chronicleArt.TryGetValue("stars",out sky)&&sky!=null)return sky;sky=new Texture2D(w,h,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};sky.SetPixels(px);sky.Apply();chronicleArt["stars"]=sky;return sky;}
 Texture2D StarFieldCached(){Texture2D t;return chronicleArt.TryGetValue("stars",out t)&&t!=null?t:StarField();}
 // A painted frame drawn in nine slices (border in pixels of the picture).
 void Framed(Rect r,string art,int border){var tex=UiTex(art);if(tex==null){GUI.Box(r,"");return;}var s=new GUIStyle{border=new RectOffset(border,border,border,border)};s.normal.background=tex;if(Event.current.type==EventType.Repaint)s.Draw(r,false,false,false,false);}
 bool GoldButton(Rect r,string text,bool enabled=true,GUIStyle style=null){var was=GUI.color;if(!enabled)GUI.color=new Color(.75f,.75f,.75f,.75f);GUI.DrawTexture(r,RoundBox("gold",256,48,9,true));GUI.color=was;ShadowLabel(r,text,style??chGoldButton);return enabled&&GUI.Button(r,GUIContent.none,GUIStyle.none);}
 bool NavyButton(Rect r,string text,bool enabled=true){var was=GUI.color;if(!enabled)GUI.color=new Color(.7f,.7f,.7f,.6f);GUI.DrawTexture(r,RoundBox("navy",256,48,6,false));GUI.Label(r,text,chNavyButton);GUI.color=was;return enabled&&GUI.Button(r,GUIContent.none,GUIStyle.none);}
 void GoldRule(float x,float y,float w,bool diamond=true){Fill(new Rect(x,y,w,1),new Color(.78f,.62f,.32f,.8f));if(diamond)GUI.DrawTexture(new Rect(x+w/2-6,y-5,12,12),Diamond());}
 void ProgressBar(Rect r,float share,string text){GUI.DrawTexture(r,RoundBox("trough",256,32,8,false));if(share>0){float w=Mathf.Max(r.height,r.width*Mathf.Clamp01(share));GUI.BeginGroup(new Rect(r.x,r.y,w,r.height));GUI.DrawTexture(new Rect(0,0,r.width,r.height),RoundBox("bar",256,32,8,true));GUI.EndGroup();}if(!string.IsNullOrEmpty(text))ShadowLabel(r,text,chBarText);}
 // ---------- layout ----------
 int[] techColumnCount;int techMaxRows;
 void ChronicleLayout(){LayoutTechnologyTree();techColumnCount=new int[TechnologyCatalog.Count+1];techMaxRows=1;foreach(int i in EraTechs(researchEra)){techColumnCount[techDepth[i]]++;techMaxRows=Math.Max(techMaxRows,techColumnCount[techDepth[i]]);}}
 // A card's place in the tree, before zoom: columns follow prerequisites, each column centred on the tallest.
 Rect ExpandedTechRect(int i){if(techColumnCount==null)ChronicleLayout();int d=techDepth[i];float y=TreeTop+(techMaxRows-techColumnCount[d])*RowPitch/2+techRow[i]*RowPitch;bool done=world.tech[i];return new Rect(TreeLeft+d*ColPitch,y+(done?(CardH-CardDoneH)/2:0),CardW,done?CardDoneH:CardH);}
 Vector2 TreeSize(){int cols=EraTechs(researchEra).Select(i=>techDepth[i]).DefaultIfEmpty(0).Max()+1;return new Vector2(TreeLeft*2+(cols-1)*ColPitch+CardW,TreeTop*2+(techMaxRows-1)*RowPitch+CardH);}
 int TechProgress(int i){return i==world.research?world.science:world.nations[0].progress[i];}
 int TurnsLeft(int i){int per=Math.Max(1,SciencePerTurn());return Math.Max(1,(Math.Max(0,world.ResearchCost(i)-TechProgress(i))+per-1)/per);}
 string Requires(int i){var missing=World.Requirements[i].Where(p=>!world.tech[p]).Select(p=>World.TechNames[p]).ToArray();return missing.Length==0?"":"Requires "+string.Join(", ",missing);}
 // ---------- the screen ----------
 void DrawExpandedResearch(){
  ChronicleOfDiscoveryStyles();
  if(researchEra<0)researchEra=world.Era(0);ChronicleLayout();
  if(inspectedTech<0||TechnologyCatalog.Eras[inspectedTech]!=researchEra){inspectedTech=world.research>=0&&TechnologyCatalog.Eras[world.research]==researchEra?world.research:EraTechs(researchEra).First();}
  var available=world.AvailableTech();
  Fill(new Rect(0,0,1280,800),new Color(.03f,.05f,.1f));
  // ---- Header ----
  Framed(new Rect(-14,-14,1308,92),"frame",26);
  {var logo=UiTex("logo");if(logo!=null)GUI.DrawTexture(new Rect(46,4,56,56),logo,ScaleMode.ScaleToFit);}
  GUI.Label(new Rect(110,12,260,26),"PARADIGM REALM",chBrand);GoldRule(110,42,190);
  ShadowLabel(new Rect(340,0,600,36),"THE CHRONICLE OF DISCOVERY",chTitle);GUI.Label(new Rect(340,31,600,24),"Technology & Civilisation",chSub);
  Fill(new Rect(370,43,110,1),NavyEdge);Fill(new Rect(800,43,110,1),NavyEdge);GUI.DrawTexture(new Rect(364,38,11,11),Diamond());GUI.DrawTexture(new Rect(905,38,11,11),Diamond());
  {var close=new Rect(1222,12,40,40);GUI.DrawTexture(close,RoundBox("navy",256,48,6,false));GUI.Label(close,"X",new GUIStyle(chTitle){fontSize=20});if(GUI.Button(close,GUIContent.none,GUIStyle.none)){researchScreen=false;techUnlocksView=false;GUIUtility.ExitGUI();}}
  // ---- Age tabs ----
  for(int e=0;e<TechnologyCatalog.EraNames.Length;e++){
   var tab=new Rect(40+e*152,79,142,38);bool open=EraViewable(e),chosen=e==researchEra;
   GUI.DrawTexture(tab,TabArt(chosen));var st=new GUIStyle(chTab);st.normal.textColor=chosen?new Color(.22f,.12f,.03f):open?new Color(.96f,.92f,.82f):new Color(.6f,.6f,.62f);
   GUI.Label(tab,e==0?TechnologyCatalog.EraNames[e]:TechnologyCatalog.EraNames[e].Replace(" Age",""),st);
   if(e<TechnologyCatalog.EraNames.Length-1)GUI.DrawTexture(new Rect(tab.xMax+2,tab.center.y-4,8,8),Diamond());
   if(open&&!chosen&&GUI.Button(tab,GUIContent.none,GUIStyle.none)){researchEra=e;techLayoutEra=-2;ChronicleLayout();inspectedTech=-1;researchScroll=Vector2.zero;techUnlocksView=false;GUIUtility.ExitGUI();}
   if(!open&&tab.Contains(Event.current.mousePosition))GUI.Label(new Rect(tab.x,tab.yMax,240,18),"Opens once a known technology leads into it",new GUIStyle(chItalic){alignment=TextAnchor.MiddleLeft,fontSize=12});
  }
  GUI.DrawTexture(new Rect(14,91,14,14),Diamond());GUI.DrawTexture(new Rect(946,91,14,14),Diamond());
  Fill(new Rect(0,121,958,1),new Color(.78f,.62f,.32f,.6f));
  // ---- The tree ----
  var view=new Rect(0,124,958,578);GUI.DrawTexture(view,StarFieldCached(),ScaleMode.ScaleAndCrop);
  {int pending=world.PendingEdict(0);if(pending>=0){var strip=new Rect(view.x+16,view.y+8,view.width-32,30);if(GoldButton(strip,TechnologyCatalog.EraNames[pending]+" complete: choose an edict ▸")){techUnlocksView=true;techUnlockScroll=Vector2.zero;}view=new Rect(view.x,view.y+44,view.width,view.height-44);}}
  var wheel=Event.current;var size=TreeSize();
  if(wheel.type==EventType.ScrollWheel&&wheel.control&&view.Contains(wheel.mousePosition)){float was=techZoom;techZoom=Mathf.Clamp(techZoom*(wheel.delta.y<0?1.12f:1/1.12f),.35f,1.6f);var at=wheel.mousePosition-view.position;researchScroll=new Vector2(Math.Max(0,(researchScroll.x+at.x)*techZoom/was-at.x),Math.Max(0,(researchScroll.y+at.y)*techZoom/was-at.y));wheel.Use();}
  if(wheel.button==2&&view.Contains(wheel.mousePosition)&&wheel.type==EventType.MouseDrag){researchScroll-=wheel.delta;wheel.Use();}
  float z=techZoom;var content=new Rect(0,0,Math.Max(view.width-16,size.x*z),Math.Max(view.height-16,size.y*z));
  float offX=Math.Max(0,(view.width-16-size.x*z)/2),offY=Math.Max(0,(view.height-16-size.y*z)/2);
  researchScroll=GUI.BeginScrollView(view,researchScroll,content);
  Rect Z(Rect q){return new Rect(offX+q.x*z,offY+q.y*z,q.width*z,q.height*z);}
  var shown=EraTechs(researchEra).ToList();
  // Arrows first, under the cards: from the right of a prerequisite to the left of what it opens.
  foreach(int i in shown)foreach(int pre in World.Requirements[i]){if(TechnologyCatalog.Eras[pre]!=researchEra)continue;
   var a=Z(ExpandedTechRect(pre));var b=Z(ExpandedTechRect(i));bool solid=world.tech[pre];var ink=solid?new Color(.95f,.78f,.36f):new Color(.96f,.62f,.22f,.9f);float mid=a.xMax+Math.Max(10,(b.x-a.xMax)*.35f);float th=Math.Max(1.5f,2*z);
   Line(new Vector2(a.xMax,a.center.y),new Vector2(mid,a.center.y),ink,th,!solid);Line(new Vector2(mid,a.center.y),new Vector2(mid,b.center.y),ink,th,!solid);Line(new Vector2(mid,b.center.y),new Vector2(b.x-8*z,b.center.y),ink,th,!solid);
   var keep=GUI.color;GUI.color=ink;GUI.DrawTexture(new Rect(b.x-11*z,b.center.y-7*z,14*z,14*z),Arrowhead());GUI.color=keep;}
  var sName=Sized(chName,z);var sPill=Sized(chPill,z);var sBullet=Sized(chBullet,z);var sItalic=Sized(chItalic,z);var sButton=Sized(chGoldButton,z);var sBar=Sized(chBarText,z);
  foreach(int i in shown)DrawChronicleCard(i,Z(ExpandedTechRect(i)),z,available,sName,sPill,sBullet,sItalic,sButton,sBar);
  GUI.EndScrollView();
  // ---- The chosen technology ----
  DrawChronicleDetail(new Rect(962,74,312,628),available);
  // ---- Queue and zoom ----
  DrawChronicleQueue(new Rect(-8,700,1296,108));
 }
 void Line(Vector2 a,Vector2 b,Color ink,float th,bool dashed){
  if(Mathf.Abs(a.y-b.y)<.5f){float x0=Math.Min(a.x,b.x),x1=Math.Max(a.x,b.x);if(!dashed){Fill(new Rect(x0,a.y-th/2,x1-x0,th),ink);return;}for(float x=x0;x<x1;x+=10)Fill(new Rect(x,a.y-th/2,Math.Min(6,x1-x),th),ink);}
  else{float y0=Math.Min(a.y,b.y),y1=Math.Max(a.y,b.y);if(!dashed){Fill(new Rect(a.x-th/2,y0,th,y1-y0),ink);return;}for(float y=y0;y<y1;y+=10)Fill(new Rect(a.x-th/2,y,th,Math.Min(6,y1-y)),ink);}}
 void DrawChronicleCard(int i,Rect r,float z,List<int> available,GUIStyle sName,GUIStyle sPill,GUIStyle sBullet,GUIStyle sItalic,GUIStyle sButton,GUIStyle sBar){
  bool known=world.tech[i],now=world.research==i,open=available.Contains(i),chosen=inspectedTech==i;
  // A glow round the technology being researched.
  if(now)for(int k=4;k>=1;k--)Fill(new Rect(r.x-k*2*z,r.y-k*2*z,r.width+k*4*z,r.height+k*4*z),new Color(.98f,.78f,.35f,.07f*k));
  GUI.Box(r,"");
  if(known)Fill(new Rect(r.x+4*z,r.y+4*z,r.width-8*z,r.height-8*z),new Color(.12f,.32f,.2f,.35f));
  if(now||chosen){var g=now?new Color(1f,.84f,.45f):new Color(.86f,.72f,.45f,.8f);float t=Math.Max(1,(now?2.5f:1.5f)*z);Fill(new Rect(r.x,r.y,r.width,t),g);Fill(new Rect(r.x,r.yMax-t,r.width,t),g);Fill(new Rect(r.x,r.y,t,r.height),g);Fill(new Rect(r.xMax-t,r.y,t,r.height),g);}
  float icon=(known?84:78)*z;DrawTechnologyGlyph(new Rect(r.x+12*z,r.y+(known?(r.height-icon)/2:12*z),icon,icon),i);
  float tx=r.x+(known?106:100)*z,tw=r.xMax-tx-8*z;
  FitLabel(new Rect(tx,r.y+(known?16:8)*z,tw,30*z),World.TechNames[i],sName,Math.Max(7,(int)(13*z)));
  // The status pill.
  string word=known?"RESEARCHED":now?"RESEARCHING":open?"AVAILABLE":"LOCKED";bool mark=known||(!now&&!open);
  float pw=sPill.CalcSize(new GUIContent(word)).x+(mark?34:22)*z;var pill=new Rect(tx,r.y+(known?52:42)*z,pw,20*z);
  GUI.DrawTexture(pill,RoundBox(known?"pillgreen":"pill",128,28,10,false));
  if(known)GUI.DrawTexture(new Rect(pill.x-4*z,pill.y-3*z,26*z,26*z),CheckArt());else if(mark)GUI.DrawTexture(new Rect(pill.x+6*z,pill.y+2*z,16*z,16*z),LockArt());
  GUI.Label(new Rect(pill.x+(mark?22:0)*z,pill.y,pill.width-(mark?22:0)*z,pill.height),word,sPill);
  if(!known){
   var benefits=World.ActiveTechBenefits(i);string lines=benefits.Length>0?string.Join("\n",benefits.Take(2).Select(v=>"• "+v).ToArray()):"• Opens the next steps of research";
   GUI.Label(new Rect(tx,r.y+66*z,tw,44*z),lines,sBullet);
   var low=new Rect(r.x+12*z,r.y+114*z,r.width-24*z,30*z);
   if(now)ProgressBar(new Rect(low.x,low.y+3*z,low.width,24*z),TechProgress(i)/(float)Math.Max(1,world.ResearchCost(i)),TechProgress(i)+" / "+world.ResearchCost(i)+" Science");
   else if(open&&world.result==""){if(GoldButton(low,"Research  ·  "+TurnsLeft(i)+" turns",true,sButton)){world.SelectResearch(i);inspectedTech=i;GUIUtility.ExitGUI();}}
   else{Fill(new Rect(low.x,low.y,low.width,1),new Color(.78f,.62f,.32f,.5f));int q=world.ResearchQueue().IndexOf(i);GUI.Label(new Rect(low.x,low.y+3*z,low.width,low.height-3*z),q>=0?"In the queue ("+(q+1)+")":Requires(i),sItalic);}
  }
  // The whole card chooses the technology for the panel on the right (after the buttons, which take the click first).
  if(GUI.Button(r,GUIContent.none,GUIStyle.none)){inspectedTech=i;techUnlocksView=false;techDetailScroll=Vector2.zero;}
 }
 void DrawChronicleDetail(Rect panel,List<int> available){
  Framed(panel,"frame",26);int i=inspectedTech;if(i<0)return;float x=panel.x+22,w=panel.width-44,cx=panel.center.x;
  FitLabel(new Rect(x,panel.y+16,w,36),World.TechNames[i].ToUpperInvariant(),chPanelTitle,14);
  var titleStyle=new GUIStyle(chPanelTitle);GoldRule(x+20,panel.y+56,w-40);
  if(techUnlocksView){DrawChronicleUnlocks(new Rect(x,panel.y+66,w,panel.height-130),available);if(NavyButton(new Rect(x,panel.yMax-56,w,36),"Back to overview")){techUnlocksView=false;}return;}
  // Medallion with the icon, and the age on a gold ribbon.
  {var med=UiTex("medallion");var m=new Rect(cx-64,panel.y+66,128,132);if(med!=null)GUI.DrawTexture(m,med,ScaleMode.ScaleToFit);DrawTechnologyGlyph(new Rect(cx-38,m.y+28,76,76),i);
   var ribbon=new Rect(cx-74,m.yMax-12,148,26);GUI.DrawTexture(ribbon,RoundBox("gold",256,48,9,true));GUI.Label(ribbon,TechnologyCatalog.EraNames[TechnologyCatalog.Eras[i]].ToUpperInvariant(),chSmallCaps);}
  float y=panel.y+226;
  {string desc=TechnologyCatalog.Descriptions[i];string line=TechnologyCatalog.IsKeystone(i)&&!string.IsNullOrEmpty(desc)?desc.Split(new[]{". "},StringSplitOptions.None)[0].TrimEnd('.')+".":EraLines[Mathf.Clamp(TechnologyCatalog.Eras[i],0,EraLines.Length-1)];
   var st=new GUIStyle(chItalic){fontSize=15,wordWrap=true};GUI.Label(new Rect(x,y,w,46),line,st);}
  y+=52;GoldRule(x,y,w);y+=10;
  GUI.Label(new Rect(x,y,w,20),"BENEFITS",chHead);Fill(new Rect(x,y+22,w,1),new Color(.78f,.62f,.32f,.35f));y+=28;
  {var benefits=World.ActiveTechBenefits(i);if(benefits.Length==0)benefits=new[]{"Opens the next steps of research"};foreach(var b in benefits.Take(4)){GUI.Label(new Rect(x+6,y,w-6,22),"•  "+b,new GUIStyle(chBody){wordWrap=false,clipping=TextClipping.Clip});y+=22;}}
  y=Math.Max(y+6,panel.y+412);GoldRule(x,y,w);y+=10;
  GUI.Label(new Rect(x,y,w,20),"RESEARCH PROGRESS",chHead);Fill(new Rect(x,y+22,w,1),new Color(.78f,.62f,.32f,.35f));y+=28;
  bool known=world.tech[i],now=world.research==i,open=available.Contains(i);int cost=world.ResearchCost(i),have=known?cost:TechProgress(i);
  GUI.Label(new Rect(x,y,w/2,22),have+" / "+cost+" Science",chBody);GUI.Label(new Rect(x+w/2,y,w/2,22),known?"Discovered":TurnsLeft(i)+" turns "+(now?"remaining":"at your rate"),new GUIStyle(chItalic){alignment=TextAnchor.MiddleRight});y+=28;
  ProgressBar(new Rect(x,y,w,16),have/(float)Math.Max(1,cost),null);y+=30;
  var main=new Rect(x,panel.yMax-104,w,42);var q=world.ResearchQueue();int at=q.IndexOf(i);
  var italic=new GUIStyle(chGoldButton){font=garamondItalic??chGoldButton.font,fontSize=19};
  if(known)GoldButton(main,"Discovered",false,italic);
  else if(now)GoldButton(main,"Currently researching",false,italic);
  else if(open&&world.result==""){if(GoldButton(main,"Research  ·  "+TurnsLeft(i)+" turns",true,italic)){world.SelectResearch(i);GUIUtility.ExitGUI();}}
  else if(at>=0){if(GoldButton(main,"In the queue ("+(at+1)+")  ·  remove",true,italic))world.Unqueue(i);}
  else if(GoldButton(main,"Add to the research queue",world.QueueReason(i)=="",italic))world.QueueTechnology(i);
  if(NavyButton(new Rect(x,panel.yMax-54,w,36),"View unlocks")){techUnlocksView=true;techUnlockScroll=Vector2.zero;}
 }
 // Everything else about the technology: edicts waiting, what it needs and opens, and the original design's unlocks.
 void DrawChronicleUnlocks(Rect area,List<int> available){
  int i=inspectedTech;GUILayout.BeginArea(area);techUnlockScroll=GUILayout.BeginScrollView(techUnlockScroll,false,false,GUIStyle.none,GUI.skin.verticalScrollbar);
  var head=new GUIStyle(chHead){fixedHeight=26};var text=new GUIStyle(chBody){wordWrap=true};var note=new GUIStyle(chItalic){wordWrap=true,alignment=TextAnchor.UpperLeft};
  int pending=world.PendingEdict(0);
  if(pending>=0){GUILayout.Label(TechnologyCatalog.EraNames[pending].ToUpperInvariant()+" COMPLETE",head);GUILayout.Label("Choose one edict. It is permanent.",note);
   for(int k=0;k<3;k++)if(Button(World.EdictNames[pending][k]+": "+World.EdictTexts[pending][k])){world.ChooseEdict(0,pending,k);GUIUtility.ExitGUI();}GUILayout.Space(8);}
  var adopted=Enumerable.Range(0,TechnologyCatalog.EraCount).Where(e=>world.nations[0].edicts!=null&&world.nations[0].edicts[e]>=0).Select(e=>World.EdictNames[e][world.nations[0].edicts[e]]).ToArray();
  if(adopted.Length>0){GUILayout.Label("YOUR EDICTS",head);GUILayout.Label(string.Join(", ",adopted),text);}
  GUILayout.Label("ABOUT",head);GUILayout.Label(TechnologyCatalog.Descriptions[i],text);
  GUILayout.Label(world.ResearchCost(i)+" science"+(world.ResearchCost(i)<world.ResearchCost(i,-1)?" (a fifth off: two civilisations you know have it)":""),note);
  GUILayout.Label("REQUIRES",head);if(World.Requirements[i].Length==0)GUILayout.Label("Nothing.",text);
  foreach(int pre in World.Requirements[i])if(Button((world.tech[pre]?"✓ ":"→ ")+World.TechNames[pre]))FocusTechnology(pre);
  GUILayout.Label("LEADS TO",head);foreach(int next in Enumerable.Range(0,TechnologyCatalog.Count).Where(t=>TechnologyCatalog.Active[t]&&World.Requirements[t].Contains(i)))if(Button(World.TechNames[next]))FocusTechnology(next);
  GUILayout.Label("ORIGINAL DESIGN UNLOCKS",head);GUILayout.Label("Only the benefits on the overview are in play; the rest is planned.",note);
  foreach(string unlock in TechnologyCatalog.Unlocks[i])GUILayout.Label("• "+unlock.Substring(2),text);
  GUILayout.EndScrollView();GUILayout.EndArea();}
 void DrawChronicleQueue(Rect bar){
  Framed(bar,"frame",26);float y=bar.y+20;
  {var books=UiTex("icon-chronicle")??UiTex("icon-research");if(books!=null)GUI.DrawTexture(new Rect(44,y+6,40,40),books,ScaleMode.ScaleToFit);}
  GUI.Label(new Rect(94,y+6,180,40),"Research queue",new GUIStyle(chName){fontSize=22,font=garamond});
  Fill(new Rect(282,y+8,1,36),new Color(.78f,.62f,.32f,.6f));
  int r=world.research;
  if(r>=0){DrawTechnologyGlyph(new Rect(298,y,52,52),r);GUI.Label(new Rect(360,y+2,200,26),World.TechNames[r],new GUIStyle(chName){fontSize=20});GUI.Label(new Rect(360,y+28,220,22),world.science+" / "+world.ResearchCost(r)+" Science ("+TurnsLeft(r)+" turns)",new GUIStyle(chItalic){alignment=TextAnchor.MiddleLeft,fontSize=15});
   if(GUI.Button(new Rect(298,y,262,52),GUIContent.none,GUIStyle.none))FocusTechnology(r);}
  else GUI.Label(new Rect(298,y+8,260,36),"Nothing being researched",new GUIStyle(chItalic){alignment=TextAnchor.MiddleLeft,fontSize=16});
  // The queued technologies after it, smallest first in line; a click shows one, the panel can remove it.
  var q=world.ResearchQueue();float qx=574;
  for(int k=0;k<q.Count&&k<5;k++){var cell=new Rect(qx+k*46,y+8,40,40);DrawTechnologyGlyph(cell,q[k]);GUI.Label(new Rect(cell.xMax-12,cell.yMax-14,16,16),(k+1).ToString(),new GUIStyle(chPill){fontSize=11});if(GUI.Button(cell,new GUIContent("",World.TechNames[q[k]]),GUIStyle.none))FocusTechnology(q[k]);}
  if(q.Count>5)GUI.Label(new Rect(qx+5*46,y+8,40,40),"+"+(q.Count-5),chBody);
  float addX=qx+Math.Min(5,q.Count)*46+(q.Count>5?40:0)+(q.Count>0?14:0);
  Fill(new Rect(addX-8,y+8,1,36),new Color(.78f,.62f,.32f,.6f));
  {string why=inspectedTech<0?"Choose a technology first.":world.QueueReason(inspectedTech);var add=new Rect(addX+6,y+8,200,38);
   if(NavyButton(add,"+  Add technology",why=="")){int n=world.QueueTechnology(inspectedTech);status=n>1?"Queued "+World.TechNames[inspectedTech]+" with "+(n-1)+" prerequisite"+(n==2?"":"s")+" before it.":"Queued "+World.TechNames[inspectedTech]+".";}
   if(why!=""&&add.Contains(Event.current.mousePosition))GUI.Label(new Rect(add.x,add.y-22,320,20),why,new GUIStyle(chItalic){alignment=TextAnchor.MiddleLeft,fontSize=13});}
  // Zoom.
  Fill(new Rect(1100,y+8,1,36),new Color(.78f,.62f,.32f,.6f));
  if(NavyButton(new Rect(1116,y+10,36,34),"−")){techZoom=Mathf.Clamp(techZoom/1.12f,.35f,1.6f);}
  GUI.Label(new Rect(1154,y+10,64,34),Mathf.RoundToInt(techZoom*100)+"%",new GUIStyle(chBody){alignment=TextAnchor.MiddleCenter,fontSize=18});
  if(NavyButton(new Rect(1220,y+10,36,34),"+")){techZoom=Mathf.Clamp(techZoom*1.12f,.35f,1.6f);}
  if(!string.IsNullOrEmpty(GUI.tooltip)){var tip=new Rect(Event.current.mousePosition.x+12,Event.current.mousePosition.y-26,220,22);GUI.Label(tip,GUI.tooltip,new GUIStyle(chItalic){alignment=TextAnchor.MiddleLeft});}
 }
}
}
