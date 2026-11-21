using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The city screen, redesigned (owner request, 16 Nov 2026, after looking at the city managers of other 4X games):
//  - totals as five tiles (food, shields, science, gold, faith); hovering one shows where it comes from;
//  - growth as "size N -> N+1 in T turns" with the food bar;
//  - citizens as a bar of tokens in four groups (working, entertainers, scientists, tax collectors): drag a token to another
//    group to change its job, or onto the land to lock a citizen on that tile;
//  - buildings as icons with their effect on hover;
//  - one production panel holding both lines (units and buildings share the city's shields as before), each with its item in
//    progress, a progress bar, turns left, a Buy button and its queue (move up or down, remove);
//  - a picker below: clicking adds to the line (starts it when the line is idle); hovering a building shows what it would add
//    to this city now and lights up the tiles it would improve on the land view.
public partial class RealmGame {
 int cityShotHover=-1;// set by -hudShot to show a building's lit tiles without a mouse
 int cityDrag=-1;Tile cityHoverTile;int cityHoverProject=-1,cityHoverProjectNext=-1;
 readonly HashSet<Tile> cityHighlight=new HashSet<Tile>();int[] cityPreview;long cityPreviewKey=long.MinValue;
 string cityTip;Vector2 cityPickScroll;readonly Rect[] cityGroupRects=new Rect[4];
 static readonly string[] CitizenGroups={"Working","Entertainers","Scientists","Tax collectors"},CitizenSingular={"worker","entertainer","scientist","tax collector"};
 static readonly string[] CitizenEffect={"work the land","+2 content","+3 science","+3 gold"};
 static readonly int[] CitizenType={-1,2,0,1};
 static readonly int[] CitizenIcon={PixelArt.IconAxe,PixelArt.IconMoodGood,PixelArt.IconScience,PixelArt.IconGold};
 static readonly Color CityCell=new Color(.06f,.09f,.17f,.88f);
 int CitizenCount(City c,int g){return g==0?world.Workers(c):g==1?c.entertainers:g==2?c.scientists:c.merchants;}
 GUIStyle cityBig,cityTiny,cityTinyCentre,cityRight;
 void CityStyles(){
  if(cityBig!=null)return;
  cityBig=new GUIStyle(heading){fontSize=20,alignment=TextAnchor.MiddleLeft,wordWrap=false,clipping=TextClipping.Overflow};
  cityTiny=new GUIStyle(small){fontSize=11,wordWrap=true};cityTinyCentre=new GUIStyle(cityTiny){alignment=TextAnchor.MiddleCenter,wordWrap=false};
  cityRight=new GUIStyle(small){alignment=TextAnchor.MiddleRight,wordWrap=false};
 }
 void SetCityTip(string text){cityTip=text;}
 // Called by DrawCityScreen at the start of each pass and at its end.
 void BeginCityFrame(City c){
  CityStyles();cityTip=null;
  // The building hovered in the picker last frame lights its tiles on the land view, which is drawn first.
  long key=cityHoverProject<0?-1:((long)c.id*100000+cityHoverProject)*100000+world.turn*10+c.pop%10;
  if(key!=cityPreviewKey){cityPreviewKey=key;cityHighlight.Clear();cityPreview=null;if(cityHoverProject>=0){var list=new List<Tile>();cityPreview=world.PreviewBuilding(c,cityHoverProject,list);foreach(var t in list)cityHighlight.Add(t);}}
 }
 void EndCityFrame(City c){
  var e=Event.current;var m=e.mousePosition;
  if(cityDrag>=0){
   if(e.type==EventType.Repaint){Fill(new Rect(m.x-12,m.y-12,24,24),new Color(.05f,.08f,.16f,.9f));DrawIcon(CitizenIcon[cityDrag],new Rect(m.x-10,m.y-10,20,20));
    cityTip=cityHoverTile!=null?"Drop to have a citizen work this tile.":"Drop on another group to change this citizen's job, or on the land to work a tile.";}
   if(e.type==EventType.MouseUp){
    int from=cityDrag,to=-1;cityDrag=-1;for(int g=0;g<4;g++)if(cityGroupRects[g].Contains(m))to=g;
    if(to>=0&&to!=from)status=MoveCitizen(c,from,to);else if(to<0&&cityHoverTile!=null)status=CitizenToTile(c,from,cityHoverTile);
    e.Use();}
  }
  if(cityTip!=null&&e.type==EventType.Repaint){
   var ts=new GUIStyle(small){wordWrap=true};float w=330,h=ts.CalcHeight(new GUIContent(cityTip),w-20)+12;
   var box=new Rect(Mathf.Clamp(m.x+18,8,1272-w),Mathf.Clamp(m.y+18,108,792-h),w,h);if(box.Contains(m))box.y=Mathf.Max(108,m.y-h-12);
   Fill(box,new Color(.04f,.06f,.13f,.97f));Fill(new Rect(box.x,box.y,box.width,1),UiGold);Fill(new Rect(box.x,box.yMax-1,box.width,1),UiGold);
   GUI.Label(new Rect(box.x+10,box.y+6,box.width-20,h-12),cityTip,ts);}
  if(e.type==EventType.Repaint){cityHoverProject=cityShotHover>=0?cityShotHover:cityHoverProjectNext;cityHoverProjectNext=-1;}
 }
 string MoveCitizen(City c,int from,int to){
  bool ok=from==0?world.AdjustSpecialist(c,CitizenType[to],1):to==0?world.AdjustSpecialist(c,CitizenType[from],-1):world.ConvertSpecialist(c,CitizenType[from],CitizenType[to]);
  return ok?(to==0?"A "+CitizenSingular[from]+" goes back to work the land.":"A "+CitizenSingular[from]+" becomes a "+CitizenSingular[to]+"."):"That citizen cannot change job.";
 }
 string CitizenToTile(City c,int from,Tile t){
  if(world.IsLocked(c,t))return "A citizen already works that tile.";
  if(from>0&&!world.AdjustSpecialist(c,CitizenType[from],-1))return "That citizen cannot change job.";
  bool ok=world.ToggleLock(c,t);string land=World.TerrainNames[t.terrain];
  return ok?"A citizen now works the "+land+" tile.":from>0?"The "+CitizenSingular[from]+" went back to the land, but every working citizen is already placed by hand; click a tile to release one.":"Every working citizen is already placed by hand; click a tile to release one first.";
 }
 // The middle column: name, totals, growth, mood, citizens, buildings.
 void DrawCityOverview(City c,Rect area){
  var e=Event.current;var m=e.mousePosition;GUI.Box(area,"");float x=area.x+12,w=area.width-24,y=area.y+8;
  GUI.SetNextControlName("cityrename");c.name=GUI.TextField(new Rect(x,y,w,28),c.name,32);y+=36;
  // Totals.
  var v=world.Yield(c);int[] vals={v[0],v[1],v[2],v[3],world.FaithYield(c)};int[] icons={PixelArt.IconFood,PixelArt.IconShields,PixelArt.IconScience,PixelArt.IconGold,PixelArt.IconFaith};
  string[] names={"Food","Shields","Science","Gold","Faith"};float cw=(w-8)/5f;
  for(int k=0;k<5;k++){var cell=new Rect(x+k*(cw+2),y,cw,56);Fill(cell,CityCell);Fill(new Rect(cell.x,cell.yMax-2,cell.width,2),new Color(UiGold.r,UiGold.g,UiGold.b,.55f));
   DrawIcon(icons[k],new Rect(cell.x+5,cell.y+7,20,20));var keep=GUI.color;if(k==0&&vals[k]<0)GUI.color=new Color(1f,.55f,.45f);GUI.Label(new Rect(cell.x+27,cell.y+4,cell.width-28,26),(k==0&&vals[k]>0?"+":"")+vals[k],cityBig);GUI.color=keep;
   GUI.Label(new Rect(cell.x,cell.y+33,cell.width,18),names[k],cityTinyCentre);
   if(cell.Contains(m))SetCityTip(TotalTip(c,k,vals[k]));}
  y+=64;
  // Growth.
  int need=world.FoodNeeded(c),turns=world.GrowthTurns(c);
  GUI.Label(new Rect(x,y,w-120,24),"Size "+c.pop+(turns>0?"   →   "+(c.pop+1)+" in "+turns+(turns==1?" turn":" turns"):""),heading);
  GUI.Label(new Rect(x+w-130,y,130,24),c.food+" / "+need+" food",cityRight);y+=26;
  DrawBar(new Rect(x,y,w,10),c.food/(float)Math.Max(1,need),new Color(.45f,.75f,.35f));y+=14;
  GUI.Label(new Rect(x,y,w,32),turns>0?World.PeopleText(world.CityPeople(c))+" people.  "+world.GrowthStatus(c):world.GrowthStatus(c),cityTiny);y+=34;
  // Mood.
  int content=world.ContentCitizens(c),unhappy=world.UnhappyCitizens(c),workers=Math.Max(1,world.Workers(c));
  GUI.Label(new Rect(x,y,w-64,22),"Content "+content+"   Unhappy "+unhappy+(world.Disorder(c)?"   — disorder":"")+(world.ReligiousUnrest(c)>0?"   (religious unrest -"+world.ReligiousUnrest(c)+")":""),body);
  if(GUI.Button(new Rect(x+w-60,y,60,22),moodPanel?"Close":"Why?",button)){moodPanel=!moodPanel;resourcePanel=false;moodScroll=Vector2.zero;}y+=24;
  Fill(new Rect(x,y,w,8),UiTrough);Fill(new Rect(x,y,w*content/workers,8),new Color(.4f,.72f,.5f));Fill(new Rect(x+w*content/workers,y,w*unhappy/workers,8),new Color(.9f,.45f,.35f));y+=16;
  // Citizens.
  GUI.Label(new Rect(x,y,w,22),"Citizens",heading);GUI.Label(new Rect(x+90,y+2,w-90,20),"drag one to change its job or work a tile",new GUIStyle(cityRight){fontSize=11});y+=24;
  float gw=(w-9)/4f;
  for(int g=0;g<4;g++){
   var box=new Rect(x+g*(gw+3),y,gw,92);cityGroupRects[g]=box;int n=CitizenCount(c,g);
   bool target=cityDrag>=0&&cityDrag!=g&&box.Contains(m);Fill(box,target?new Color(.16f,.26f,.2f,.95f):CityCell);if(target){Fill(new Rect(box.x,box.y,box.width,1),UiGold);Fill(new Rect(box.x,box.yMax-1,box.width,1),UiGold);}
   GUI.Label(new Rect(box.x,box.y+2,box.width,16),CitizenGroups[g]+" "+n,cityTinyCentre);
   int cols=Mathf.Max(1,(int)((box.width-6)/25)),most=cols*2,shown=Math.Min(n,n>most?most-1:most);
   for(int i=0;i<shown;i++){var tok=new Rect(box.x+5+(i%cols)*25,box.y+22+(i/cols)*25,23,23);Fill(tok,new Color(.12f,.17f,.26f));Fill(new Rect(tok.x,tok.yMax-1,tok.width,1),new Color(UiGold.r,UiGold.g,UiGold.b,.4f));DrawIcon(CitizenIcon[g],new Rect(tok.x+2,tok.y+2,19,19));}
   if(n>shown)GUI.Label(new Rect(box.x+5+(shown%cols)*25,box.y+22+(shown/cols)*25,23,23),"+"+(n-shown),cityTinyCentre);
   GUI.Label(new Rect(box.x,box.yMax-18,box.width,16),CitizenEffect[g],cityTinyCentre);
   if(box.Contains(m)&&cityDrag<0){
    SetCityTip(g==0?n+" citizens work the land; the city chooses their tiles by its focus unless you place them. Drag one onto a specialist group, or onto the land.":CitizenGroups[g]+": "+n+". Each gives "+CitizenEffect[g]+" but works no land. Drag one to another group, or onto the land to put it back to work.");
    if(e.type==EventType.MouseDown&&e.button==0&&n>0&&GUI.enabled&&world.result==""){cityDrag=g;e.Use();}}
  }
  y+=100;
  // Buildings.
  GUI.Label(new Rect(x,y,w-110,22),"Buildings",heading);
  if(world.modernRules&&GUI.Button(new Rect(x+w-100,y,100,22),resourcePanel?"Close":"Resources",button)){resourcePanel=!resourcePanel;moodPanel=false;moodScroll=Vector2.zero;}y+=26;
  var built=World.BuildingIds.Where(b=>b!=65&&b!=66&&world.HasBuilding(c,b)).ToList();var holy=World.FaithBuildingIds.Where(b=>b!=36&&world.HasBuilding(c,b)).ToList();
  if(built.Count+holy.Count==0)GUI.Label(new Rect(x,y,w,22),"None yet. Choose one from the Buildings list.",cityTiny);
  int across=Mathf.Max(1,(int)(w/36));int at=0;
  foreach(int b in built.Concat(holy)){var r=new Rect(x+(at%across)*36,y+(at/across)*36,32,32);if(r.yMax>area.yMax-4)break;Fill(r,CityCell);
   bool faith=World.FaithBuildingIds.Contains(b)&&!World.BuildingIds.Contains(b);if(faith)DrawIcon(PixelArt.IconFaith,new Rect(r.x+3,r.y+3,26,26));else DrawIcon(new Rect(r.x+3,r.y+3,26,26),12+Array.IndexOf(World.BuildingIds,b));
   if(r.Contains(m))SetCityTip(faith?World.FaithBuildingName(b):World.BuildName(b)+"\n"+World.BuildingEffect(b)+(world.nationalForces&&(b==30||b==40||b==32)?"\nNational arms output: +"+(b==30?ForcesRules.WorkshopArms:b==40?ForcesRules.FactoryArms:ForcesRules.BarracksArms)+" a turn from this building.":""));at++;}
 }
 string TotalTip(City c,int k,int total){
  if(k==4)return "Faith "+total+" a turn. "+(world.modernRules?FaithShareText(c):c.religion>=0?"Follows "+world.FaithName(c.religion):"No majority faith")+". Shrines, temples and holy sites add faith.";
  var parts=world.YieldBreakdown(c,k);var lines=new List<string>{new[]{"Food","Shields","Science","Gold"}[k]+": "+(k==0&&total>0?"+":"")+total+" a turn"};
  foreach(var p in parts)lines.Add("  "+p.Key+"   "+(p.Value>0?"+":"")+p.Value);
  if(k==1&&c.build>=0&&c.buildingProject>=0&&world.modernRules)lines.Add("Shared between the two lines: units "+world.LineShare(c,false)+", buildings "+world.LineShare(c,true)+".");
  if(k==0&&c.ration>0)lines.Add("Rations for the army: -"+c.ration+".");
  return string.Join("\n",lines.ToArray());
 }
 // The right column: both production lines, then the list to choose from.
 void DrawCityProduction(City c,Rect area){
  var e=Event.current;var m=e.mousePosition;GUI.Box(area,"");float x=area.x+10,w=area.width-20,y=area.y+8;
  GUI.Label(new Rect(x,y,w,24),"Production",heading);GUI.Label(new Rect(x,y,w,24),world.Yield(c)[1]+" shields a turn",cityRight);y+=28;
  y=DrawLineCard(c,false,new Rect(x,y,w,0));y+=6;y=DrawLineCard(c,true,new Rect(x,y,w,0));y+=8;
  float half=(w-6)/2;
  if(GUI.Button(new Rect(x,y,half,26),(cityBuildingTab?"":"● ")+"Units",button)){cityBuildingTab=false;cityPickScroll=Vector2.zero;}
  if(GUI.Button(new Rect(x+half+6,y,half,26),(cityBuildingTab?"● ":"")+"Buildings",button)){cityBuildingTab=true;cityPickScroll=Vector2.zero;}y+=30;
  if(!cityBuildingTab&&world.nationalForces){GUI.Label(new Rect(x,y,w-110,24),"Soldiers and warships are raised by the nation.",cityTiny);if(GUI.Button(new Rect(x+w-104,y,104,22),"Forces ▸",button)){cityScreen=false;detailsScreen="Military";militaryTab=0;GUIUtility.ExitGUI();}y+=26;}
  bool building=cityBuildingTab;int current=building?c.buildingProject:c.build;var queue=building?c.buildingQueue:c.queue;int output=Math.Max(1,world.LineShare(c,building));
  int[] options=(building?World.BuildingIds:Enumerable.Range(0,World.UnitNames.Length).Select(World.UnitBuild).ToArray()).Where(p=>world.TechAllows(c,p)&&!(world.nationalForces&&!building&&ForcesRules.Formation(World.BuildUnit(p)))).ToArray();
  int[] holy=building&&world.modernRules?World.FaithBuildingIds.Where(b=>world.FaithAllows(c,b)).ToArray():new int[0];
  const float Row=44;var list=new Rect(x-2,y,w+4,area.yMax-y-6);float inner=options.Length*Row+(holy.Length>0?28+holy.Length*Row:0);
  bool inList=list.Contains(m);
  cityPickScroll=GUI.BeginScrollView(list,cityPickScroll,new Rect(0,0,list.width-18,inner));var lm=Event.current.mousePosition;float rw=list.width-20;
  for(int i=0;i<options.Length;i++){
   int p=options[i];var r=new Rect(0,i*Row,rw,Row-3);string reason=world.BuildReason(c,p);bool ordered=building&&(current==p||queue.Contains(p));bool was=GUI.enabled;
   if(ordered&&e.type==EventType.MouseDown&&e.button==1&&r.Contains(lm)&&world.result==""){world.CancelProject(c,p);world.SettleBuilding(c);status="Cancelled "+world.BuildNameFor(c.owner,p)+".";e.Use();GUIUtility.ExitGUI();}
   string payWhy=building&&current<0?world.PayReason(c,p):"";if(reason==""&&payWhy!="")reason=payWhy;GUI.enabled=was&&reason==""&&world.result==""&&!ordered;
   if(GUI.Button(r,"",button)){bool ok=world.ChooseProject(c,p,current>=0);if(ok&&building)world.SettleBuilding(c);status=ok?(current>=0?world.BuildNameFor(c.owner,p)+" added to the "+(building?"buildings":"units")+" line.":"Now making "+world.BuildNameFor(c.owner,p)+"."):"That line is full (eight items).";GUIUtility.ExitGUI();}
   GUI.enabled=was;var keep=GUI.color;if(reason!=""||ordered)GUI.color=new Color(1,1,1,.55f);
   if(building)DrawIcon(new Rect(r.x+6,r.y+5,27,27),12+Array.IndexOf(World.BuildingIds,p));else GUI.DrawTexture(new Rect(r.x+4,r.y+3,31,31),people[World.BuildUnit(p)],ScaleMode.ScaleToFit,true);
   int cost=world.CostFor(c,p),t=Math.Max(1,(cost+output-1)/output);
   GUI.Label(new Rect(r.x+40,r.y+2,r.width-120,20),world.BuildNameFor(c.owner,p),new GUIStyle(body){wordWrap=false,clipping=TextClipping.Clip});
   GUI.Label(new Rect(r.x+40,r.y+20,r.width-44,19),ordered?"Ordered":reason!=""?reason:building?(world.buildingMaterials&&world.BuildingPriceText(p)!=""?world.BuildingPriceText(p)+" · ":"")+World.BuildingEffect(p):UnitLine(p),new GUIStyle(cityTiny){wordWrap=false,clipping=TextClipping.Clip});
   GUI.Label(new Rect(r.xMax-82,r.y+2,76,20),cost+" • "+t+"t",cityRight);GUI.color=keep;
   if(r.Contains(lm)&&inList){if(building)cityHoverProjectNext=p;SetCityTip(PickTip(c,p,building,reason,ordered,cost,t));}
  }
  if(holy.Length>0){float hy=options.Length*Row+2;GUI.Label(new Rect(4,hy,rw,24),"Religious — from "+world.nations[0].faith+" faith (+"+world.FaithPerTurn(0)+" a turn)",heading);hy+=26;
   for(int i=0;i<holy.Length;i++){int b=holy[i];var r=new Rect(0,hy+i*Row,rw,Row-3);string reason=world.FaithBuildReason(c,b);bool was=GUI.enabled;GUI.enabled=was&&reason==""&&world.result=="";
    if(GUI.Button(r,"",button)){status=world.BuildWithFaith(c,b)?World.FaithBuildingName(b)+" consecrated.":world.FaithBuildReason(c,b);GUIUtility.ExitGUI();}
    GUI.enabled=was;DrawIcon(PixelArt.IconFaith,new Rect(r.x+6,r.y+5,27,27));GUI.Label(new Rect(r.x+40,r.y+2,r.width-120,20),World.FaithBuildingName(b),new GUIStyle(body){wordWrap=false,clipping=TextClipping.Clip});
    GUI.Label(new Rect(r.xMax-82,r.y+2,76,20),world.FaithBuildingCost(b)+" faith",cityRight);GUI.Label(new Rect(r.x+40,r.y+20,r.width-44,19),reason!=""?reason:"Ready to consecrate",new GUIStyle(cityTiny){wordWrap=false,clipping=TextClipping.Clip});
    if(r.Contains(lm)&&inList)SetCityTip(World.FaithBuildingName(b)+" — "+world.FaithBuildingCost(b)+" faith.\n"+World.FaithBuildingTexts[Array.IndexOf(World.FaithBuildingIds,b)]+(reason!=""?"\n"+reason:""));}}
  GUI.EndScrollView();
 }
 string UnitLine(int p){int u=World.BuildUnit(p);return u<0?"":World.Power[u]>0?"Strength "+World.Power[u]:"Civilian";}
 string PickTip(City c,int p,bool building,string reason,bool ordered,int cost,int turns){
  var lines=new List<string>{world.BuildNameFor(c.owner,p)+" — "+cost+" shields, about "+turns+(turns==1?" turn":" turns")+" on this line"+(building&&world.BuildingGold(p)>0?" + "+world.BuildingGold(p)+" gold":"")+"."};
  if(building){lines.Add(World.BuildingEffect(p));if(world.buildingMaterials&&world.BuildingPriceText(p)!=""){var nat=world.nations[c.owner];lines.Add("Paid when it starts: "+world.BuildingPriceText(p)+" (you have "+nat.gold+" gold, "+nat.timber+" timber, "+nat.stone+" stone). Cancelling refunds it.");}{int armsThen=world.ArmsWith(c,p);if(armsThen>=0)lines.Add("National arms output: "+world.ArmsOutput(c.owner)+" → "+armsThen+" a turn.");}
   if(cityPreview!=null&&cityHoverProject==p){var d=cityPreview;var gain=new List<string>();string[] n={"food","shields","science","gold","faith"};for(int k=0;k<5;k++)if(d[k]!=0)gain.Add((d[k]>0?"+":"")+d[k]+" "+n[k]);
    lines.Add(gain.Count>0?"Here, now: "+string.Join(", ",gain.ToArray())+" a turn.":"No change to this city's totals today.");if(cityHighlight.Count>0)lines.Add("Improves the "+cityHighlight.Count+" lit tile"+(cityHighlight.Count==1?"":"s")+" on the land.");}}
  if(ordered)lines.Add("Already ordered. Right-click to cancel it.");else if(reason!="")lines.Add(reason);else lines.Add("Click to add it to the "+(building?"buildings":"units")+" line.");
  return string.Join("\n",lines.ToArray());
 }
 // One production line: the item in progress with its bar, turns and Buy, then up to two queued items.
 float DrawLineCard(City c,bool building,Rect card){
  var m=Event.current.mousePosition;{var q0=building?c.buildingQueue:c.queue;int cur0=building?c.buildingProject:c.build;card.height=cur0<0?62:72+Math.Max(1,Math.Min(2,q0.Count))*20+(q0.Count>2?16:0);}
  Fill(card,CityCell);Fill(new Rect(card.x,card.y,3,card.height),building?new Color(.55f,.62f,.85f):new Color(.85f,.55f,.35f));
  float x=card.x+10,w=card.width-16,y=card.y+4;int current=building?c.buildingProject:c.build,progress=building?c.buildingProduction:c.production;var queue=building?c.buildingQueue:c.queue;var turns=world.LineTurns(c,building);
  GUI.Label(new Rect(x,y,w,16),(building?"BUILDINGS":"UNITS")+"   +"+world.LineShare(c,building)+" shields a turn",new GUIStyle(cityTiny){normal={textColor=UiGold}});y+=18;
  if(current<0){GUI.Label(new Rect(x,y+6,w,36),"Nothing chosen. Pick one from the "+(building?"Buildings":"Units")+" list below.",cityTiny);return card.yMax;}
  if(building)DrawIcon(new Rect(x,y+2,30,30),12+Array.IndexOf(World.BuildingIds,current));else GUI.DrawTexture(new Rect(x-2,y,34,34),people[World.BuildUnit(current)],ScaleMode.ScaleToFit,true);
  int cost=world.CostFor(c,current),price=world.RushPrice(c,building);string reason=world.BuildReason(c,current);if(reason==""&&building&&world.buildingMaterials&&c.paidFor!=current)reason=world.PayReason(c,current);
  GUI.Label(new Rect(x+38,y,w-130,20),world.BuildNameFor(c.owner,current),new GUIStyle(body){wordWrap=false,clipping=TextClipping.Clip});
  DrawBar(new Rect(x+38,y+21,w-130,8),progress/(float)Math.Max(1,cost),building?new Color(.55f,.62f,.85f):new Color(.9f,.6f,.3f));
  GUI.Label(new Rect(x+38,y+29,w-130,16),reason!=""?"Paused: "+reason:progress+" / "+cost+"  •  "+(turns.Count>0?turns[0]:0)+(turns.Count>0&&turns[0]==1?" turn":" turns"),new GUIStyle(cityTiny){wordWrap=false,clipping=TextClipping.Clip});
  {var buy=new Rect(x+w-86,y+2,86,30);bool was=GUI.enabled;bool can=price>0&&world.gold>=price&&reason==""&&!World.IsProject(current)&&world.result=="";GUI.enabled=was&&can;
   if(GUI.Button(buy,"Buy "+price+"g",button)){status=world.Rush(c,building)?"Bought: "+world.BuildNameFor(c.owner,current)+" completes next turn.":"Cannot buy it now.";GUIUtility.ExitGUI();}GUI.enabled=was;
   if(buy.Contains(m))SetCityTip(World.IsProject(current)?"Projects cannot be bought.":price<=0?"Already paid for.":world.gold<price?"Buying costs "+price+" gold; you have "+world.gold+".":"Pay "+price+" gold to finish it next turn (double the price if nothing has been put into it yet).");}
  y+=48;
  int rows=Math.Min(2,queue.Count);
  for(int i=0;i<rows;i++){var r=new Rect(x,y+i*20,w,19);Fill(new Rect(r.x,r.yMax-1,r.width,1),new Color(1,1,1,.06f));
   GUI.Label(new Rect(r.x,r.y,r.width-90,19),(i+2)+". "+world.BuildNameFor(c.owner,queue[i]),new GUIStyle(cityTiny){wordWrap=false,clipping=TextClipping.Clip});
   GUI.Label(new Rect(r.xMax-140,r.y,80,19),(turns.Count>i+1?turns[i+1]:0)+" turns",new GUIStyle(cityTiny){alignment=TextAnchor.MiddleRight,wordWrap=false});
   bool was=GUI.enabled;GUI.enabled=was&&i>0;if(GlyphButton(new Rect(r.xMax-56,r.y,18,18),0)){world.MoveQueued(c,building,i,-1);GUIUtility.ExitGUI();}
   GUI.enabled=was&&i<queue.Count-1;if(GlyphButton(new Rect(r.xMax-37,r.y,18,18),1)){world.MoveQueued(c,building,i,1);GUIUtility.ExitGUI();}
   GUI.enabled=was;if(GlyphButton(new Rect(r.xMax-18,r.y,18,18),2)){world.RemoveQueued(c,building,i);world.SettleBuilding(c);status="Removed from the line.";GUIUtility.ExitGUI();}}
  if(queue.Count>2)GUI.Label(new Rect(x,y+40,w,16),"and "+(queue.Count-2)+" more: "+string.Join(", ",queue.Skip(2).Select(b=>world.BuildNameFor(c.owner,b)).ToArray()),new GUIStyle(cityTiny){wordWrap=false,clipping=TextClipping.Clip});
  else if(queue.Count==0)GUI.Label(new Rect(x,y,w,18),"Nothing queued after it. Clicking the list adds to this line.",cityTiny);
  return card.yMax;
 }
}
}
namespace ParadigmRealm {
public partial class RealmGame {
 // Small queue buttons with pixel glyphs (the interface font has no arrows or crosses): 0 up, 1 down, 2 remove.
 bool GlyphButton(Rect r,int kind){
  bool hit=GUI.Button(r,"",button);var ink=GUI.enabled?new Color(.95f,.86f,.6f):new Color(.5f,.5f,.52f);float cx=Mathf.Round(r.center.x),cy=Mathf.Round(r.center.y);
  if(kind<2)for(int i=0;i<4;i++){float row=kind==0?cy-2+i:cy+1-i;Fill(new Rect(cx-i-.5f,row,2*i+1,1),ink);}
  else for(int i=-3;i<=3;i++){Fill(new Rect(cx+i-.5f,cy+i-.5f,2,2),ink);Fill(new Rect(cx+i-.5f,cy-i-.5f,2,2),ink);}
  return hit;
 }
}
}
