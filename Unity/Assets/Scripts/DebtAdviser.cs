using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// The treasurer (owner request, 27 Nov 2026: "when a civilisation runs into debt an adviser should suggest what the player needs
// to do to get out of debt or reduce it"). Each suggestion is worked out on your empire as it stands, with what it would change a
// turn, best first: the Empire priorities tried for real and put back, soldiers abroad (they cost double), the costliest soldier,
// luxury spending, agreements that cost gold, cut-off cities, the costliest building, roads. The adviser opens at the start of a
// turn when the treasury is empty or will be after the coming turn, and again whenever the debt deepens; Empire > Overview shows it
// too (DebtAdvice).
public partial class World {
 public sealed class Advice{public string text,action;public int gain;}
 // Gold the treasury will hold after the next turn at today's income.
 public int GoldNextTurn(int owner){return nations[owner].gold+BudgetDetail(owner).Net;}
 public bool NeedsTreasurer(int owner){return nations[owner].arrears>0||BudgetDetail(owner).Net<0&&GoldNextTurn(owner)<0;}
 public List<Advice> DebtAdvice(int owner){
  var list=new List<Advice>();var n=nations[owner];var b=BudgetDetail(owner);int now=b.Net;
  Func<int,int,int> tryRates=(sci,mil)=>{int s=n.scienceRate,m=n.militaryRate,tax=n.tax;n.scienceRate=sci;n.militaryRate=mil;int net=BudgetDetail(owner).Net;n.scienceRate=s;n.militaryRate=m;n.tax=tax;return net-now;};
  if(n.militaryRate>0){int g=tryRates(n.scienceRate,0);if(g>0)list.Add(new Advice{text="Set the military priority to 0% (Empire > Overview): the trade it turns into arms comes back as gold.",gain=g,action="empire"});}
  if(n.scienceRate>=20){int to=n.scienceRate-20,g=tryRates(to,Math.Min(n.militaryRate,100-to));if(g>0)list.Add(new Advice{text="Lower science from "+n.scienceRate+"% to "+to+"% (Empire > Overview): research slows, the treasury fills.",gain=g,action="empire"});}
  var soldiers=units.Where(u=>u.owner==owner&&Power[u.type]>0).ToList();
  // Soldiers: each saving is found by trying it (moving them home, or leaving one out) and putting everything back.
  var capital=cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).FirstOrDefault();
  {var abroad=soldiers.Where(u=>!AtHome(u)).ToList();if(abroad.Count>0&&capital!=null){var was=abroad.Select(u=>new[]{u.x,u.y}).ToList();foreach(var u in abroad){u.x=capital.x;u.y=capital.y;}int g=BudgetDetail(owner).Net-now;for(int k=0;k<abroad.Count;k++){abroad[k].x=was[k][0];abroad[k].y=was[k][1];}
   if(g>0)list.Add(new Advice{text="Bring "+abroad.Count+" soldier"+(abroad.Count==1?"":"s")+" home: units outside your borders cost twice as much.",gain=g,action="military"});}}
  {Unit dear=null;int best=0;foreach(var u in soldiers){int at=units.IndexOf(u);units.RemoveAt(at);int g=BudgetDetail(owner).Net-now;units.Insert(at,u);if(g>best){best=g;dear=u;}}
   if(dear!=null)list.Add(new Advice{text="Disband the "+UnitName(dear)+" (Military > Forces), or any soldier you can spare.",gain=best,action="military"});}
  if(b.luxury>0)list.Add(new Advice{text="Stop luxury spending: it costs "+n.luxury+" gold in each city.",gain=b.luxury,action="empire"});
  if(b.treatyCost>0)list.Add(new Advice{text="Your agreements cost "+b.treatyCost+" gold a turn; end the ones that no longer pay for themselves (Diplomacy).",gain=b.treatyCost,action="diplomacy"});
  if(b.cutOff>0)list.Add(new Advice{text="Join your cut-off cities to the capital by road: each costs "+CutOffGold+" gold a turn while it is cut off.",gain=b.cutOff,action=""});
  if(b.forceShortfall>0)list.Add(new Advice{text="You have more soldiers than your service capacity: disband or grow your cities to stop paying "+b.forceShortfall+" a turn.",gain=b.forceShortfall,action="military"});
  {City at=null;int worst=-1;foreach(var c in cities.Where(c=>c.owner==owner))foreach(int bd in c.buildings){if(IsWonder(bd)||IsProject(bd))continue;if(worst<0||BuildingUpkeep(bd)>BuildingUpkeep(worst)){worst=bd;at=c;}}
   if(at!=null&&BuildingUpkeep(worst)>=2)list.Add(new Advice{text="The "+BuildName(worst)+" in "+at.name+" is your costliest building ("+BuildingUpkeep(worst)+" gold a turn); build no more like it until the treasury recovers.",gain=0,action=""});}
  if(b.roads>=3)list.Add(new Advice{text="Roads cost "+b.roads+" gold a turn: lay no new ones for now.",gain=0,action=""});
  list.Add(new Advice{text="Raise income: a Marketplace, tax collectors in large cities (city screen), trade routes and a commercial pact with a neighbour all add gold.",gain=0,action=""});
  return list.OrderByDescending(a=>a.gain).ToList();
 }
}
public partial class RealmGame {
 bool debtAdviserOpen;int debtShownArrears=-1,debtShownTurn=-99;
 bool DebtAdviserOpen(){return debtAdviserOpen&&world!=null&&world.result=="";}
 // Called when your turn begins (GuidePromptsUI, after End Turn).
 void CheckTreasurer(){
  if(world==null||world.result!=""||!world.modernRules)return;var n=world.nations[0];
  if(!world.NeedsTreasurer(0)){debtShownArrears=-1;return;}
  if(n.arrears>debtShownArrears||world.turn-debtShownTurn>=10){debtAdviserOpen=true;debtShownArrears=n.arrears;debtShownTurn=world.turn;}
 }
 void DrawDebtAdviser(){
  GUI.enabled=true;DiplomacyStyles();HudStyles();var n=world.nations[0];var b=world.BudgetDetail(0);
  Fill(FullCanvas,new Color(0,0,0,.45f));var box=new Rect(290,120,700,520);DipPanel(box,.98f);
  GUI.Label(new Rect(box.x,box.y+14,box.width,34),"YOUR TREASURER",dipTitle);
  var text=new GUIStyle(dipRow){fontSize=15,wordWrap=true,alignment=TextAnchor.UpperLeft};float x=box.x+34,w=box.width-68,y=box.y+60;
  string state=n.arrears>0?world.DebtState(0):"The treasury will be empty after this turn: "+n.gold+" gold, "+b.Net+" a turn.";
  {var st=new GUIStyle(text){fontSize=16,normal={textColor=new Color(1f,.62f,.45f)}};float h=st.CalcHeight(new GUIContent(state),w);GUI.Label(new Rect(x,y,w,h),state,st);y+=h+6;}
  {string sums="Income "+b.Income+", spending "+b.Expense+" (soldiers "+b.army+", buildings "+b.buildingUpkeep+", cities "+b.cityUpkeep+(b.roads>0?", roads "+b.roads:"")+(b.treatyCost>0?", agreements "+b.treatyCost:"")+"): "+(b.Net>=0?"+":"")+b.Net+" a turn.";float h=text.CalcHeight(new GUIContent(sums),w);GUI.Label(new Rect(x,y,w,h),sums,text);y+=h+10;}
  Fill(new Rect(x,y,w,1),new Color(.86f,.7f,.39f,.6f));y+=10;
  GUI.Label(new Rect(x,y,w,22),"What would help most:",new GUIStyle(text){normal={textColor=new Color(.86f,.7f,.39f)}});y+=26;
  float bottom=box.yMax-70;
  foreach(var a in world.DebtAdvice(0)){string line="• "+a.text+(a.gain>0?"  (+"+a.gain+" gold a turn)":"");var st=new GUIStyle(text);float h=st.CalcHeight(new GUIContent(line),w);if(y+h>bottom)break;GUI.Label(new Rect(x,y,w,h),line,st);y+=h+6;}
  Color navy=new Color(.1f,.16f,.3f),teal=new Color(.07f,.38f,.4f);
  if(DipButton(new Rect(box.x+34,box.yMax-56,200,38),"Empire priorities",teal)){debtAdviserOpen=false;empireTab=0;OpenDetails("Economy");}
  if(DipButton(new Rect(box.x+250,box.yMax-56,200,38),"Military",navy)){debtAdviserOpen=false;militaryTab=0;OpenDetails("Military");}
  if(DipButton(new Rect(box.xMax-34-200,box.yMax-56,200,38),"Close",navy)){debtAdviserOpen=false;GUIUtility.ExitGUI();}
 }
}
}
