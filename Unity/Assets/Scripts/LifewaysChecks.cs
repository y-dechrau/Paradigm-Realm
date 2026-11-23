using System;
using System.Linq;
namespace ParadigmRealm {
// The Beliefs & Traditions briefs' scenarios (23 Nov 2026): the First Granary from shortage to a refused feast, the four
// separate measures, emergence and spread with causes, commitments, one reform and one schism, and public vows that tell
// a breach from a release from an impossibility.
public static class LifewaysChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Lifeways: "+what);}
 static WayChoice Next(World w,int kind){return w.wayChoices.FirstOrDefault(e=>e.kind==kind);}
 static World Twin(World w){return UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));}
 public static void Run(){
  // 1. The First Granary: shortage, relief at a real cost, memory, a practice, a lean year, and the save.
  {var w=World.GranaryScenario();var cap=w.cities.First(c=>c.owner==0);var other=w.cities.FirstOrDefault(c=>c.owner==0&&c.id!=cap.id);
   Check(other!=null&&w.Yield(cap)[0]<0,"the scenario has a second city and a capital that cannot feed itself ("+w.Yield(cap)[0]+")");
   w.LifewaysTurn();w.turn++;w.LifewaysTurn();var e=Next(w,0);Check(e!=null&&e.city==cap.id,"a real shortage raises the choice");
   Check(w.WayOptionProblem(e,0)==""&&w.WayChoiceText(e).Contains(cap.name),"relief from the other city is offered ("+w.WayOptionProblem(e,0)+")");
   int had=other.food,capFood=cap.food;Check(w.WayResolve(e,0)&&other.food<had&&cap.food>capFood&&cap.food-capFood==had-other.food,"relief costs the giver what the receiver gains");
   Check(!w.WayResolve(e,0)&&other.food+cap.food==had+capFood,"it cannot be taken twice");
   for(int k=0;k<Lifeways.RememberAfter;k++){w.turn++;w.LifewaysTurn();}var how=Next(w,1);Check(how!=null,"the relief is remembered: a second choice follows");
   Check(w.WayOptions(how).Length==4,"observance, civic rule, custom or nothing");Check(w.WayResolve(how,0),"an observance can be chosen");
   var p=w.practices.FirstOrDefault(x=>x.city==cap.id);Check(p!=null&&p.kind==0&&p.name.Contains("First Granary")&&p.nextDue==w.turn+Lifeways.Period,"the Feast of the First Granary is kept from now on");
   Check(w.WayContent(cap)>=1&&w.WayLine(cap).Contains("First Granary"),"it shows in the city and pleases it");Check(w.wayChronicle.Any(s=>s.Contains("First Granary")),"and in the chronicle");
   var copy=Twin(w);Check(copy.Valid()&&copy.practices.Count==1&&copy.practices[0].nextDue==p.nextDue&&copy.WayOf(copy.cities.First(c=>c.id==cap.id)).reliefTurn==w.WayOf(cap).reliefTurn,"it survives saving and loading");
   // The feast falls due while the city is short again: an opportunity cost, with three answers.
   cap.food=0;w.turn=p.nextDue;w.LifewaysTurn();var due=Next(w,2);Check(due!=null&&due.a==p.id,"a lean year puts the feast to the ruler");
   int conv=w.WayOf(cap).conviction;var twin=Twin(w);var twin2=Twin(w);
   int gold=w.nations[0].gold;Check(w.WayResolve(due,0)&&w.nations[0].gold<gold&&p.kept==1&&w.WayOf(cap).conviction>conv,"keeping it in full costs the treasury and deepens conviction");
   {var tc=twin.cities.First(c=>c.id==cap.id);var tp=twin.practices[0];var td=twin.wayChoices.First(x=>x.kind==2);int g2=twin.nations[0].gold;Check(twin.WayResolve(td,1)&&tp.kept==1&&twin.nations[0].gold<g2&&twin.nations[0].gold>w.nations[0].gold&&twin.WayOf(tc).conviction<conv,"a smaller observance costs less, and a little conviction");
    tc.food=0;twin.turn=tp.nextDue;twin.LifewaysTurn();var again=twin.wayChoices.First(x=>x.kind==2);Check(twin.WayOptionProblem(again,1)!="","the elders do not bend twice running");}
   {var tc=twin2.cities.First(c=>c.id==cap.id);var tp=twin2.practices[0];var td=twin2.wayChoices.First(x=>x.kind==2);Check(twin2.WayResolve(td,2)&&tp.missed==1&&tp.status==0&&twin2.WayOf(tc).conviction<conv-10&&twin2.WayContent(tc)<w.WayContent(cap),"refusing it is felt");
    tc.food=0;twin2.turn=tp.nextDue;twin2.LifewaysTurn();twin2.WayResolve(twin2.wayChoices.First(x=>x.kind==2),2);Check(tp.status==1&&twin2.wayChronicle.Any(s=>s.Contains("lapsed")),"refused twice, it lapses");}
   // One reform, end to end.
   Check(w.ReformProblem(p.id,1)==""&&w.ReformPractice(p.id,1)==""&&p.form==1&&w.PracticeCostText(p).Contains("gold")&&w.ReformProblem(p.id,2)!="","a practice can be reformed once: the feast is paid in coin");}
  // 2. A civic rule seeds a philosophy, not a faith.
  {var w=World.GranaryScenario();var cap=w.cities.First(c=>c.owner==0);w.LifewaysTurn();w.turn++;w.LifewaysTurn();int gold=w.nations[0].gold;Check(w.WayResolve(Next(w,0),1)&&w.nations[0].gold<gold,"grain can be bought instead");for(int k=0;k<Lifeways.RememberAfter;k++){w.turn++;w.LifewaysTurn();}
   Check(w.WayResolve(Next(w,1),1)&&w.practices[0].kind==1&&w.WayOf(cap).adherence[Lifeways.OpenHand]>=30,"a civic rule seeds the Open Hand");}
  // 3. Emergence from the ground, a deliberate founding, spread along a road with its cause, and state policy.
  {var w=World.GranaryScenario();var cap=w.cities.First(c=>c.owner==0);var other=w.cities.First(c=>c.owner==0&&c.id!=cap.id);cap.library=true;
   for(int k=0;k<20;k++)w.WaysEmergeForChecks(cap);var l=w.WayOf(cap);Check(l.tradition>=0&&l.cause!=""&&w.wayChronicle.Any(s=>s.Contains("first honoured")),"a tradition rises from what the city is ("+l.cause+")");
   int t=l.tradition;int rootless=Enumerable.Range(0,4).FirstOrDefault(k=>!w.RootOf(other,k));if(!w.RootOf(other,rootless))Check(w.HouseProblem(0,other.id,rootless).Contains("root"),"a house needs a root");
   l.adherence[t]=80;l.conviction=60;int before=w.WayOf(other).adherence[t];w.WaysSpreadForChecks(cap);bool linked=w.Connected(cap)&&w.Connected(other)&&w.Distance(cap.x,cap.y,other.x,other.y)<=Lifeways.SpreadReach;
   Check(linked?w.WayOf(other).adherence[t]>before:w.WayOf(other).adherence[t]==before,"it travels only along a real connection");
   int content=w.WayContent(cap);Check(w.SetWayPolicy(0,t,2)==""&&w.WayContent(cap)<content,"discouraging a tradition displeases its followers");w.SetWayPolicy(0,t,0);
   w.WayOf(other).tradition=-1;Check(w.CommitmentProblem(0,t)!="","a commitment needs two following cities");w.WayOf(other).adherence[t]=70;w.WayOf(other).tradition=t;Check(w.TakeCommitment(0,t)=="","then it can be taken up");
   var copy=Twin(w);Check(copy.Valid()&&copy.NationWayOf(0).commitment[t]==1,"commitments are saved");}
  // 4. A commitment broken while a city holds firm divides it: one schism, end to end.
  {var w=World.GranaryScenario();var cap=w.cities.First(c=>c.owner==0);var other=w.cities.First(c=>c.owner==0&&c.id!=cap.id);
   foreach(var c in new[]{cap,other}){var l=w.WayOf(c);l.adherence[Lifeways.Ford]=70;l.tradition=Lifeways.Ford;l.conviction=70;}Check(w.TakeCommitment(0,Lifeways.Ford)=="","the commitment of the ford is taken");
   w.LifewaysTurn();w.turn++;w.LifewaysTurn();var e=Next(w,0);Check(e!=null&&w.WayResolve(e,2),"a hungry city is left to endure");
   Check(w.NationWayOf(0).commitment[Lifeways.Ford]==2&&w.wayChronicle.Any(s=>s.Contains("broke")),"which breaks the commitment");var split=Next(w,5);Check(split!=null,"and the firmest city turns on the realm");
   int city=split.city;Check(w.WayResolve(split,1)&&w.WayOf(w.cities.First(c=>c.id==city)).split,"it may be let go to keep its own house");
   Check(w.CommitmentProblem(0,Lifeways.Ford).Contains("renewed from"),"the broken commitment cannot be renewed at once");}
  // 5. Public vows: kept, broken, released and made impossible are four different endings.
  {Func<World> make=()=>{var x=World.GranaryScenario();x.met[1]=true;x.Relation(0,1).contact=true;return x;};
   var w=make();var cap=w.cities.First(c=>c.owner==0);Check(w.VowProblem(0,0,cap.id,-1,5)!=""&&w.VowProblem(0,1,-1,1,20)=="","a vow has a least length and a named people ("+w.VowProblem(0,1,-1,1,20)+")");
   Check(w.DeclareVow(0,1,-1,1,20)==""&&w.VowProblem(0,0,cap.id,-1,20)!="","one vow at a time");var v=w.vows[0];int trust=w.Trust(1,0),worth=w.VowWorth(v);
   w.turn=v.due;w.VowsTurnForChecks();Check(v.status==1&&w.Trust(1,0)>trust&&w.OpinionMod(1,0,OpinionMods.KeptWord)>0,"a kept vow is rewarded and remembered");
   Check(w.VowProblem(0,1,-1,1,20)!="","no vow straight after another");w.turn+=Lifeways.VowCooldown;Check(w.DeclareVow(0,1,-1,1,20)=="","then it may be sworn again");Check(w.VowWorth(w.vows[1])==worth/2,"but the same vow again is worth half ("+w.VowWorth(w.vows[1])+" of "+worth+")");
   var b=make();b.DeclareVow(0,1,-1,1,20);int t0=b.Trust(1,0);b.SetTrust(1,0,50);t0=50;b.WayWar(0,1);Check(b.vows[0].status==2&&b.Trust(1,0)<t0&&b.OpinionMod(1,0,OpinionMods.Broken)<0,"declaring war breaks it, and it is remembered as broken");
   var i=make();i.DeclareVow(0,1,-1,1,20);i.SetTrust(1,0,50);i.WayWar(1,0);Check(i.vows[0].status==4&&i.Trust(1,0)==50&&i.OpinionMod(1,0,OpinionMods.Broken)==0,"their attack makes it impossible, with no blame");
   var r=make();r.DeclareVow(0,1,-1,1,20);r.SetTrust(1,0,50);int g=r.nations[0].gold;Check(r.ReleaseVow(r.vows[0].id)==""&&r.vows[0].status==3&&r.nations[0].gold==g-Lifeways.ReleaseCost&&r.Trust(1,0)==50,"release costs gold, not trust");
   var h=make();var hc=h.cities.First(c=>c.owner==0);h.DeclareVow(0,0,hc.id,-1,20);hc.owner=1;h.VowsTurnForChecks();Check(h.vows[0].status==4,"a vow about a lost city becomes impossible");
   var f=make();f.DeclareVow(1,1,-1,0,20);Check(f.vows.Count==1&&f.VowForbidsWar(1,0)&&f.WarReason(1,0).Contains("vow"),"another ruler's vow binds that ruler");
   var copy=Twin(r);Check(copy.Valid()&&copy.vows.Count==1&&copy.vows[0].status==3,"vows are saved");}
  // 6. The first question comes from the land, and can be made a vow.
  {var w=World.New(2312,"Opening",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);w.lifeways=true;w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.LifewaysTurn();var q=Next(w,6);
   Check(q!=null&&q.a>=0&&q.a<4&&w.WayChoiceText(q).Contains("?"),"the opening asks a question read from the map");
   var w2=World.New(2312,"Opening",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);w2.lifeways=true;w2.Found(w2.units.First(u=>u.owner==0&&u.type==0));w2.LifewaysTurn();Check(Next(w2,6).a==q.a,"the same seed asks the same question");
   Check(w.WayResolve(q,0)&&w.vows.Count==1&&w.vows[0].kind==3,"it can be made a public vow");w.turn=w.vows[0].due;w.VowsTurnForChecks();Check(w.vows[0].status==1||w.vows[0].status==2,"which is kept or broken on its turn");}
  // 8. Whole turns with the system on: thirty turns of a real game stay valid, and the story arrives through End Turn alone.
  {var w=World.GranaryScenario();bool asked=false,kept=false;for(int k=0;k<30&&w.result=="";k++){foreach(int o in Enumerable.Range(1,w.civCount-1))w.Relation(0,o).peaceUntil=w.turn+10;while(w.NextWayChoice()!=null){var e=w.NextWayChoice();asked|=e.kind==0;int pick=0;while(pick<w.WayOptions(e).Length-1&&w.WayOptionProblem(e,pick)!="")pick++;if(!w.WayResolve(e,pick))break;}w.EndTurn();kept|=w.practices.Any(p=>p.kept>0);}
   Check(w.Valid(),"thirty turns with traditions stay valid");Check(asked&&w.practices.Count>0,"End Turn alone brings the shortage and the practice ("+w.practices.Count+")");Check(kept||w.wayChronicle.Count>2,"and the practice comes due");}
  // 7. A voice beyond the gate; older saves load; a game without the system is untouched.
  {var w=World.GranaryScenario();w.openedGates=new System.Collections.Generic.List<int>{1};w.LifewaysTurn();var e=Next(w,3);Check(e!=null&&w.WayChoiceText(e).Length>40,"an opened gateway brings a voice");int inf=w.nations[0].influence;Check(w.WayResolve(e,2)&&w.nations[0].influence==inf+5,"it can be answered");w.turn++;w.LifewaysTurn();Check(Next(w,3)==null,"once");
   var old=World.New(2313,"Old",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);old.ways=null;old.vows=null;old.practices=null;old.nationWays=null;old.wayChoices=null;old.wayChronicle=null;Check(old.Valid()&&old.NextWayChoice()==null,"a game from before traditions loads safely");
   old.Found(old.units.First(u=>u.owner==0&&u.type==0));old.LifewaysTurn();Check(old.wayChoices.Count==0&&old.WayContent(old.cities[0])==0,"and without the system nothing changes");}
 }
}
}
