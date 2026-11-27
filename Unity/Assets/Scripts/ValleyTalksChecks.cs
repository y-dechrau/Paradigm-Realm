using System;
using System.Linq;
using System.Text;
namespace ParadigmRealm {
// The valley encounter (owner brief, 27 Nov 2026): the same map and plans, only the neighbour's timber access changed, must give
// different decisions from the neighbour's own reckoning; a counteroffer must change where it settles; agreements and memories
// survive saving; turns and reloads never repeat offers, payments or deliveries; a broken promise is remembered.
public static class ValleyTalksChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Valley talks: "+what);}
 public static string Report="";
 public static void Run(){
  var sb=new StringBuilder();
  ValleyNeed Need(World w){var n=w.AssessValley(1);sb.AppendLine(w.ValleyReasoningText(1));return n;}
  // 1. Availability changes behaviour.
  var plenty=Need(World.ValleyTalkScenario("plenty"));Check(plenty.decision=="silent"&&plenty.kind!=2,"with plenty of timber it says nothing about timber ("+plenty.reason+")");
  var shortW=World.ValleyTalkScenario("short");var shortN=Need(shortW);Check(shortN.decision=="approach"&&shortN.kind==1&&shortN.amount>0&&shortN.amount<=ValleyTuning.SmallShort,"a small shortage brings a modest purchase ("+shortN.decision+", "+shortN.reason+")");
  var lastW=World.ValleyTalkScenario("lasting");var lastN=Need(lastW);Check(lastN.decision=="approach"&&lastN.kind==2&&lastN.motive==0,"a lasting shortage with no other source asks for the ford ("+lastN.decision+", "+lastN.reason+")");
  Check(lastN.offerGold>shortN.offerGold,"and pays more for it than for a small purchase ("+lastN.offerGold+" against "+shortN.offerGold+")");
  var altN=Need(World.ValleyTalkScenario("alternative"));Check(altN.decision=="alternative"||altN.decision=="approach"&&altN.offerGold<lastN.offerGold,"a wood of its own near home makes the ford worth little ("+altN.decision+", "+altN.reason+")");
  var farmN=Need(World.ValleyTalkScenario("farmland"));Check(farmN.decision=="approach"&&farmN.kind==2&&farmN.motive==1,"with timber to spare, fertile land still makes the ford worth asking for ("+farmN.decision+", "+farmN.reason+")");
  Report=sb.ToString();UnityEngine.Debug.Log("VALLEY REASONING: "+Report);
  // 2. It raises the matter in play, once; asking why gives grounded reasons.
  var w=lastW;for(int k=0;k<ValleyTuning.RaiseEvery&&w.OpenIssue(1)==null;k++)w.EndTurn();var issue=w.OpenIssue(1);
  Check(issue!=null&&issue.kind==2&&issue.opening.Contains("settlers"),"the neighbour raises the ford in play, naming your settlers ("+(issue==null?"none":issue.opening)+")");
  string why=w.ValleyWhy(issue);Check(why.StartsWith("The timber"),"asked why, it speaks of the timber ("+why+" | "+w.ValleyReasoningText(1)+" | turn "+w.turn+")");
  {var rep=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));int before=rep.Issues2().Count;rep.EndTurn();rep.EndTurn();Check(rep.Issues2().Count==before,"ending turns does not repeat the approach");}
  // 3. A counteroffer changes its settlement decision: you supply timber, it settles elsewhere.
  int each;bool ok;string answer=w.JudgeSupplyCounter(issue,1,3,out each,out ok);Check(!ok,"too little, too short is refused ("+answer+")");
  answer=w.JudgeSupplyCounter(issue,3,12,out each,out ok);Check(ok,"enough timber in time is accepted ("+answer+")");
  w.AgreeSupplyCounter(issue,3,12);var site=w.tiles[issue.site];
  Check(w.KeepPromiseWarning(1,site.x,site.y)!=""&&w.KeepPromiseWarning(0,site.x,site.y)=="","the neighbour now keeps clear of the ford; you are free to settle it");
  Check(!w.units.Any(u=>u.owner==1&&u.type==0&&u.hasOrder&&w.Distance(u.destinationX,u.destinationY,site.x,site.y)<=ValleyTuning.KeepRadius),"its settler no longer heads for the ford");
  int myT=w.nations[0].timber,theirT=w.nations[1].timber;
  for(int k=0;k<12;k++)w.EndTurn();
  {var ac0=w.valleyAccords.First(a=>a.supplier==0);Check(!w.cities.Any(c=>c.owner==1&&w.Distance(c.x,c.y,site.x,site.y)<=ValleyTuning.KeepRadius),"twelve turns on, no town of theirs stands at the ford (accord status "+ac0.status+", delivered "+ac0.delivered+", missed "+ac0.missed+"; towns "+string.Join(", ",w.cities.Where(c=>c.owner==1).Select(c=>c.name+" "+w.Distance(c.x,c.y,site.x,site.y)+" founded "+c.age).ToArray())+"; events "+string.Join(" | ",w.Relation(0,1).events.Take(4).ToArray())+"; warnings at their towns: "+string.Join(" ; ",w.cities.Where(c=>c.owner==1).Select(c=>c.name+": [" +w.SettlementDiplomacyWarning(1,c.x,c.y)+"] accord site "+(ac0.site==issue.site)+" keeper "+ac0.keeper+" radius "+ac0.radius).ToArray())+")");}
  var acc=w.valleyAccords.First(a=>a.supplier==0);Check(acc.delivered+acc.missed<=12&&acc.delivered>0,"deliveries ran once a turn ("+acc.delivered+" delivered, "+acc.missed+" missed)");
  // 4. Saving and loading keeps agreements and memories, and does not deliver twice.
  var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));var ca=copy.valleyAccords.First(a=>a.id==acc.id);
  Check(ca.delivered==acc.delivered&&ca.status==acc.status&&copy.supplyRecords.Count==w.supplyRecords.Count&&copy.valleyIssues.Count==w.valleyIssues.Count,"agreements, issues and the delivery record are saved");
  int t0=copy.nations[0].timber;copy.EndTurn();var cb=copy.valleyAccords.First(a=>a.id==acc.id);Check(cb.delivered+cb.missed<=ca.delivered+ca.missed+1,"a reloaded game delivers at most once more per turn");
  // 5. A broken promise is remembered with the place and the payment.
  var f=World.ValleyTalkScenario("farmland");for(int k=0;k<ValleyTuning.RaiseEvery&&f.OpenIssue(1)==null;k++)f.EndTurn();var fi=f.OpenIssue(1);Check(fi!=null&&fi.motive==1,"the farmland case raises the ford for its land");
  Check(f.ValleyWhy(fi).StartsWith("The land"),"asked why, it speaks of the land");int g=f.JudgeSupplyCounter(fi,5,20,out each,out ok).Length;Check(!ok,"timber does not buy off a leader who wants farmland");
  int gold0=f.gold;Check(f.AcceptValley(fi)==""&&f.gold>gold0,"you can accept the payment and promise");var fs=f.tiles[fi.site];Check(f.KeepPromiseWarning(0,fs.x,fs.y)!="","your settlers are warned of the promise at the ford");
  var settler=f.units.FirstOrDefault(u=>u.owner==0&&u.type==0);settler.x=fs.x;settler.y=fs.y;settler.moves=1;int op0=f.OpinionMod(1,0,OpinionMods.Broken);var town=f.Found(settler);
  Check(town!=null&&f.valleyAccords.Any(a=>a.status==2)&&f.OpinionMod(1,0,OpinionMods.Broken)<op0&&f.supplyRecords.Any(r=>r.receiver==1&&r.broken>0),"settling the ford anyway breaks the promise and they remember it");
  Check(f.Relation(0,1).events.Any(e=>e.Contains("broke the agreement over")&&e.Contains("gold")),"the record names the place and the payment");
  // 6. A withdrawn offer explains what changed.
  var s=World.ValleyTalkScenario("short");var si=s.AskAboutValley(1);Check(si!=null&&si.kind==1,"asked from the Court, the small shortage becomes a purchase");
  s.nations[1].timber+=60;s.EndTurn();Check(si.status==3&&si.why.Contains("no longer lack timber"),"given timber elsewhere, it withdraws and says why ("+si.why+")");
  // 7. Your own proposals, judged from their stake.
  {var lp=World.ValleyTalkScenario("lasting");var ford=lp.valleyIssues.Count;int fordCell=lp.AssessValley(1).site;
   string a=lp.JudgeProposal(1,0,fordCell,0,0,0,out ok);Check(!ok,"asking them to leave the ford they need, for nothing, is refused ("+a+")");
   lp.JudgeProposal(1,0,fordCell,0,0,400,out ok);Check(ok,"for enough gold they agree");lp.gold=500;lp.nations[0].gold=500;lp.MakeProposal(1,0,fordCell,0,0,400);var ft=lp.tiles[fordCell];
   Check(lp.KeepPromiseWarning(1,ft.x,ft.y)!=""&&!lp.units.Any(u=>u.owner==1&&u.type==0&&u.hasOrder&&lp.Distance(u.destinationX,u.destinationY,ft.x,ft.y)<=ValleyTuning.KeepRadius),"then they keep clear of it and their settler turns away");
   var far=lp.tiles.First(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&t.owner<0&&lp.Distance(t.x,t.y,ft.x,ft.y)>=12);string b=lp.JudgeProposal(1,0,far.y*lp.Width+far.x,0,0,0,out ok);Check(ok,"a place they have no plans for costs nothing ("+b+")");
   // A painted area (27 Nov 2026): the promise covers exactly the tiles marked, no ring around them.
   {var area=lp.KeepTiles(far.y*lp.Width+far.x,1).Where(t=>!PixelArt.IsWater(t.terrain)&&t.owner<0).Take(4).Select(t=>t.y*lp.Width+t.x).ToList();lp.JudgeProposal(1,0,area,0,0,0,out ok);Check(ok&&area.Count>=2,"a painted area is judged like a place");
    lp.MakeProposal(1,0,area,0,0,0);var pa=lp.valleyAccords.Last();var outside=lp.tiles.First(t=>t.realm==0&&!area.Contains(t.y*lp.Width+t.x)&&lp.Distance(t.x,t.y,far.x,far.y)==2);
    Check(pa.area.Count==area.Count&&area.All(c=>lp.AccordCovers(pa,lp.tiles[c].x,lp.tiles[c].y))&&!lp.AccordCovers(pa,outside.x,outside.y),"the promise covers the painted tiles and nothing beyond them");
    var round=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(lp));Check(round.valleyAccords.Last().area.Count==area.Count,"the painted area survives saving and loading");
    Check(lp.valleyAccords.Where(a=>a!=pa&&a.site>=0).All(a=>a.area==null||a.area.Count==0||a.area.Count>0),"older promises keep their ring");}
   var sp=World.ValleyTalkScenario("short");string c=sp.JudgeProposal(1,2,-1,4,0,6,out ok);Check(ok,"a neighbour short of timber buys yours ("+c+")");
   var pp=World.ValleyTalkScenario("plenty");string d=pp.JudgeProposal(1,2,-1,4,0,6,out ok);Check(!ok&&d.Contains("enough"),"one with plenty does not ("+d+")");
   string e=pp.JudgeProposal(1,3,-1,2,5,12,out ok);Check(ok,"one with plenty sells you timber each turn ("+e+")");}
  Check(w.Valid()&&copy.Valid()&&f.Valid()&&s.Valid(),"every game stays valid");
 }
}
public partial class World{public System.Collections.Generic.List<ValleyIssue> Issues2(){return Issues();}}
}
