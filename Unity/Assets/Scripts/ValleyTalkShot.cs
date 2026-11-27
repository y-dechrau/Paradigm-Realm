using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (27 Nov 2026): "-hudShot <folder> -valleyOnly" plays the valley encounter in the built game at 1280 x 800:
// the neighbour's call answered by a real click, the talk, asking why, a counteroffer of timber, and the map ten turns later.
// The farmland case follows: asking why there gets the land, and timber is refused. PROBE lines say what was tested.
public partial class RealmGame {
 IEnumerator ValleyTalkShots(string folder){
  Directory.CreateDirectory(folder);tutorialOn=false;tutorialStep=-1;guide=false;
  foreach(string variant in new[]{"lasting","farmland"}){
   string scenario=Path.Combine(Application.temporaryCachePath,"valley-shot.json");File.WriteAllText(scenario,JsonUtility.ToJson(World.ValleyTalkScenario(variant)));LoadFile(scenario);activeSavePath=null;autoSaveFolder=folder;mainMenu=false;nameSetup=false;viewRealm=0;isoWidth=96;
   selected=-1;cityId=-1;int turns=0;
   while(world.OpenIssue(1)==null&&turns<6){EndTurn();while(turnRunning)yield return null;yield return Frames(3);if(world.researchPrompts!=null)world.researchPrompts.Clear();if(world.buildPrompts!=null)world.buildPrompts.Clear();if(world.wayChoices!=null)world.wayChoices.Clear();if(world.OpenIssue(1)==null)world.rivalCalls.Clear();else world.rivalCalls.RemoveAll(c=>!c.text.Contains("talk about"));turns++;}
   world.nations[0].faith=0;var issue=world.OpenIssue(1);Probe(variant+": the neighbour raises the ford after "+turns+" turns",issue!=null);if(issue==null)continue;
   Debug.Log("PROBE "+variant+" reasoning: "+world.ValleyReasoningText(1));
   var s=world.tiles[issue.site];FocusTile(s.x,s.y);yield return Frames(20);yield return Shot(folder,"valley-"+variant+"-call.bmp",20);
   // Answer the call with a real click.
   var call=CallRect;yield return ClickAt(call.x+64+60,call.y+66+12);yield return Frames(15);Probe(variant+": clicking Answer opens the talk",ValleyTalkOpen());
   yield return Shot(folder,"valley-"+variant+"-talk.bmp",20);
   valleyWhy=true;yield return Frames(10);Debug.Log("PROBE "+variant+" why: "+world.ValleyWhy(issue));yield return Shot(folder,"valley-"+variant+"-why.bmp",20);
   if(variant=="lasting"){
    counterPer=4;counterTurns=12;yield return Frames(10);yield return Shot(folder,"valley-lasting-counter.bmp",20);
    {var alt=world.BestOtherSite(1,issue.site,ValleyTuning.KeepRadius+3);Debug.Log("PROBE lasting: neighbour's best other site "+(alt==null?"none":world.Distance(alt.x,alt.y,s.x,s.y)+" tiles from the ford, score "+alt.score));}
    valleyReply=world.AgreeSupplyCounter(issue,counterPer,counterTurns);foreach(var u in world.units.Where(u=>u.owner==1&&u.type==0))Debug.Log("PROBE lasting: after agreeing, neighbour settler "+(u.hasOrder?"bound "+world.Distance(u.destinationX,u.destinationY,s.x,s.y)+" tiles from the ford":"has no order"));Debug.Log("PROBE lasting answer: "+valleyReply);Probe("lasting: the counteroffer is accepted",issue.status==4);
    yield return Frames(10);yield return Shot(folder,"valley-lasting-agreed.bmp",20);valleyTalk=-1;
    // Settle the ford yourself, then play on: where does the neighbour settle?
    var mine=world.units.First(u=>u.owner==0&&u.type==0);mine.x=s.x;mine.y=s.y;mine.moves=1;var town=world.Found(mine);Probe("lasting: you found a town at the ford with no promise broken",town!=null&&world.valleyAccords.All(a=>a.status!=2));
    for(int k=0;k<12;k++){world.nations[0].faith=0;EndTurn();while(turnRunning)yield return null;yield return Frames(2);ClosePrompts();world.rivalCalls.Clear();world.nations[0].faith=0;foreach(var u in world.units.Where(u=>u.owner==1&&u.type==0))Debug.Log("PROBE lasting turn "+world.turn+": neighbour settler at "+world.Distance(u.x,u.y,s.x,s.y)+" tiles from the ford"+(u.hasOrder?", bound "+world.Distance(u.destinationX,u.destinationY,s.x,s.y)+" tiles from it":", no order"));}
    var theirs=world.cities.Where(c=>c.owner==1).ToList();Debug.Log("PROBE lasting: neighbour towns "+string.Join("; ",theirs.Select(c=>c.name+" at "+world.Distance(c.x,c.y,s.x,s.y)+" tiles from the ford").ToArray()));
    var acc=world.valleyAccords.First(a=>a.supplier==0);Debug.Log("PROBE lasting: deliveries "+acc.delivered+", missed "+acc.missed+", status "+acc.status);
    Probe("lasting: no town of theirs at the ford",!theirs.Any(c=>world.Distance(c.x,c.y,s.x,s.y)<=ValleyTuning.KeepRadius));
    isoWidth=64;FocusTile(s.x,s.y);yield return Frames(30);yield return Shot(folder,"valley-lasting-12-turns-later.bmp",20);
   }else{
    int each;bool ok;string judged=world.JudgeSupplyCounter(issue,5,20,out each,out ok);Debug.Log("PROBE farmland counter: "+judged);Probe("farmland: timber does not buy off a leader who wants the land",!ok);
    counterPer=5;counterTurns=20;yield return Frames(10);yield return Shot(folder,"valley-farmland-counter.bmp",20);valleyTalk=-1;
   }
  }
  // Your own terms (27 Nov 2026): the composer with a place named, then a timber purchase; and a plain pact answered in the call box.
  {valleyTalk=-1;world.rivalCalls.Clear();var home=world.cities.First(c=>c.owner==0);var place=world.tiles.First(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&world.Distance(t.x,t.y,home.x,home.y)==4);
   OpenCompose(1);composeKind=0;composeSite=place.y*world.Width+place.x;composeGold=20;isoWidth=96;FocusTile(place.x,place.y);yield return Frames(20);Probe("compose: the composer is open",ComposeOpen());yield return Shot(folder,"valley-compose-place.bmp",20);
   composeKind=3;composeAmount=3;composeTurns=8;composeGold=12;yield return Frames(5);yield return Shot(folder,"valley-compose-timber.bmp",10);composeWith=-1;
   foreach(var o in world.offers.Where(o=>o.to==0&&o.status==0))o.status=2;
   var pact=new DiplomaticOffer{id=world.nextDiplomaticId++,from=1,to=0,kind=Pacts.Kind[Pacts.NonAggression],created=world.turn,expires=world.turn+5,status=0};world.offers.Add(pact);
   world.rivalCalls.Add(new RivalCall{from=1,offer=pact.id,text="offers a non-aggression pact."});yield return Frames(10);Probe("call: a plain pact is answered in the box",World.PlainPact(pact));yield return Shot(folder,"valley-call-pact.bmp",10);
   var call=CallRect;yield return ClickAt(call.x+64+40,call.y+66+12);yield return Frames(5);Probe("call: Accept agrees the pact ("+pact.status+")",pact.status==1&&world.Pact(0,1,Pacts.NonAggression));
   yield return Shot(folder,"valley-call-pact-accepted.bmp",10);}
  Application.Quit();
 }
}
}

