using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// Screens for the 13 Nov 2026 diplomacy (DiplomacyRequests.cs, DiplomacyPeace.cs, DiplomacyMemory.cs): the Requests panel on the
// diplomacy screen's right side, the peace-terms rows of the deal panel, and the memory, commitments and explanation sections
// of the More page. Same frame, styles and hover notes as the rest of the screen.
public partial class RealmGame {
 bool requestsView;int reqResource,reqKind,reqTarget,reqGate,reqDuration=1,reqConsequence;string reqGold="0",reqAid="25";bool reqEndRivalry;
 int peaceAskCity,peaceGiveCity,peaceTruce;bool peaceWithdraw;
 static readonly int[] RequestDurations={5,10,20};
 // ---- Requests (right panel) ----
 void DrawRequestsPanel(int partner,Rect side,float y){
  float left=side.x+16,wide=side.width-32;bool enabled=GUI.enabled;var pn=world.nations[partner];
  Color navy=new Color(.1f,.16f,.3f),teal=new Color(.07f,.38f,.4f);
  if(DipButton(new Rect(left,y,wide,26),"◂ Back",navy))requestsView=false;y+=32;
  DipHeading(new Rect(left,y,wide,22),"Ask "+world.NationName(partner));y+=26;
  reqKind=Dropdown(new Rect(left,y,wide,26),"reqkind",reqKind,RequestKinds.Names);y+=30;
  GUI.Label(new Rect(left,y,wide,34),RequestKinds.Texts[reqKind],new GUIStyle(dipRow){fontSize=12,wordWrap=true});y+=36;
  var r=new DiplomaticRequest{from=0,to=partner,kind=reqKind,duration=RequestDurations[Mathf.Clamp(reqDuration,0,2)],gold=DealNumber(reqGold,2000),consequence=reqConsequence,endRivalry=reqEndRivalry};
  if(reqKind==RequestKinds.StopWar||reqKind==RequestKinds.JoinWar){
   var others=Enumerable.Range(1,world.civCount-1).Where(i=>i!=partner&&world.met[i]&&world.Survives(i)).ToArray();
   if(others.Length==0){GUI.Label(new Rect(left,y,wide,22),"You know no other civilisation.",new GUIStyle(dipRow){fontSize=13});y+=26;}
   else{reqTarget=Mathf.Clamp(reqTarget,0,others.Length-1);GUI.Label(new Rect(left,y+3,90,22),"Concerning",new GUIStyle(dipRow){fontSize=13});reqTarget=Dropdown(new Rect(left+92,y,wide-92,26),"reqtarget",reqTarget,others.Select(i=>world.NationName(i)).ToArray());r.target=others[reqTarget];y+=30;}}
  if(reqKind==RequestKinds.DefendGate){
   var gates=world.AllGates().Where(g=>g.owner==0).ToList();
   if(gates.Count==0){GUI.Label(new Rect(left,y,wide,22),"You hold no gateway.",new GUIStyle(dipRow){fontSize=13});y+=26;}
   else{reqGate=Mathf.Clamp(reqGate,0,gates.Count-1);GUI.Label(new Rect(left,y+3,90,22),"Gateway",new GUIStyle(dipRow){fontSize=13});reqGate=Dropdown(new Rect(left+92,y,wide-92,26),"reqgate",reqGate,gates.Select(g=>{var near=world.cities.Where(c=>c.owner==0).OrderBy(c=>world.Distance(c.x,c.y,g.x,g.y)).FirstOrDefault();return near==null?"("+g.x+", "+g.y+")":"near "+near.name;}).ToArray());var pick=gates[reqGate];r.gate=pick.y*world.Width+pick.x;y+=30;}}
  if(reqKind==RequestKinds.Resource){var shared=Enumerable.Range(RequestKinds.FirstShared,RequestKinds.LastShared-RequestKinds.FirstShared+1).ToArray();reqResource=Mathf.Clamp(reqResource,0,shared.Length-1);GUI.Label(new Rect(left,y+3,90,22),"Resource",new GUIStyle(dipRow){fontSize=13});reqResource=Dropdown(new Rect(left+92,y,wide-92,26),"reqres",reqResource,shared.Select(i=>World.ResourceNames[i]+(world.HasDomesticResource(partner,i)?"":" (they have none)")).ToArray());r.resource=shared[reqResource];y+=30;}
  if(reqKind==RequestKinds.Aid){GUI.Label(new Rect(left,y+3,90,22),"Gold asked",new GUIStyle(dipRow){fontSize=13});reqAid=Stepper(new Rect(left+92,y,wide-92,24),reqAid,25,2000);r.aid=DealNumber(reqAid,2000);y+=30;}
  else{GUI.Label(new Rect(left,y+3,90,22),"Term",new GUIStyle(dipRow){fontSize=13});reqDuration=Dropdown(new Rect(left+92,y,wide-92,26),"reqterm",reqDuration,new[]{"5 turns","10 turns","20 turns"});r.duration=RequestDurations[Mathf.Clamp(reqDuration,0,2)];y+=30;}
  GUI.Label(new Rect(left,y+3,90,22),"You pay",new GUIStyle(dipRow){fontSize=13});reqGold=Stepper(new Rect(left+92,y,wide-92,24),reqGold,25,2000);r.gold=DealNumber(reqGold,2000);y+=28;
  GUI.Label(new Rect(left,y,wide,18),"Paid only when it is done.",hudSmall);y+=20;
  GUI.Label(new Rect(left,y+3,90,22),"If not",new GUIStyle(dipRow){fontSize=13});reqConsequence=Dropdown(new Rect(left+92,y,wide-92,26),"reqthreat",reqConsequence,RequestConsequence.Names);r.consequence=reqConsequence;y+=30;
  if(world.Rival(0,partner)){reqEndRivalry=GUI.Toggle(new Rect(left,y,wide,22),reqEndRivalry," And you end your rivalry when it is done (reopens trade)");r.endRivalry=reqEndRivalry;y+=26;}else{reqEndRivalry=false;r.endRivalry=false;}
  string problem=world.RequestProblem(r);int score=problem==""?world.RequestScore(r,null):-1;
  var keep=GUI.color;GUI.color=problem!=""?new Color(.75f,.75f,.78f):score>=0?DipGood:DipBad;
  var verdict=new Rect(left,y,wide,38);GUI.Label(verdict,problem!=""?problem:score>=0?"They would agree (+"+score+").":"They would refuse ("+score+").",new GUIStyle(dipRow){fontSize=13,wordWrap=true});GUI.color=keep;
  if(problem=="")Tip(verdict,"Why they would answer so:",world.RequestLines(r),"A threat is weighed by your strength as they see it; repeating threats makes them worthless. Breaking an accepted request costs trust; an ally who abandons you is remembered.");y+=40;
  float half=(wide-8)/2;GUI.enabled=enabled&&problem=="";
  if(DipButton(new Rect(left,y,half,36),"SEND REQUEST",teal)){var sent=world.ProposeRequest(r);status=sent.answer;dipReply=(string.IsNullOrEmpty(pn.leader)?world.NationName(partner):pn.leader)+": \""+(sent.status==RequestStatus.Fulfilled||sent.status==RequestStatus.Active?"Very well. We will do it.":sent.answer.Contains("instead")?"We cannot do that. Take this gold instead.":"No.")+"\"";dipReplyPartner=partner;}
  if(DipButton(new Rect(left+half+8,y,half,36),"What would it take?",navy)){int need=world.IncentiveToAccept(r);if(need>0){reqGold=(r.gold+need).ToString();status="They would agree for "+(r.gold+need)+" gold on completion.";}else status=need==0?"They would already agree.":"No incentive you can afford would change their mind: "+string.Join("; ",world.RequestLines(r).Where(l=>l.Value<0).OrderBy(l=>l.Value).Take(2).Select(l=>l.Key).ToArray())+".";}
  GUI.enabled=enabled;y+=42;
  // What is open between you, newest first.
  var open=world.requests.Where(x=>(x.from==0&&x.to==partner||x.to==0&&x.from==partner)&&x.created>=world.turn-20).OrderByDescending(x=>x.id).Take(4).ToList();
  if(open.Count>0&&y<side.yMax-120){DipHeading(new Rect(left,y,wide,20),"Between you");y+=22;
   foreach(var x in open){if(y>side.yMax-80)break;var row=new Rect(left,y,wide,20);GUI.Label(row,(x.from==0?"You asked: ":"They asked: ")+RequestKinds.Names[x.kind]+" — "+RequestStatus.Names[x.status]+(x.status==RequestStatus.Active?" ("+(x.deadline-world.turn)+" turns left)":""),new GUIStyle(dipRow){fontSize=12});Tip(row,world.DescribeRequest(x),null,x.answer);y+=20;}}
 }
 // ---- Peace terms (inside the deal panel during war) ----
 float DrawPeaceTerms(int partner,Rect side,float y,DiplomaticOffer offer){
  float left=side.x+16,wide=side.width-32;
  var theirs=world.cities.Where(c=>c.owner==partner&&world.At(c.x,c.y).seen&&world.nations[partner].capital!=c.id).OrderByDescending(c=>c.pop).Take(8).ToList();
  var mine=world.cities.Where(c=>c.owner==0&&world.nations[0].capital!=c.id).OrderBy(c=>c.pop).Take(8).ToList();
  GUI.Label(new Rect(left,y+3,120,22),"They hand over",new GUIStyle(dipRow){fontSize=13});peaceAskCity=Mathf.Clamp(Dropdown(new Rect(left+124,y,wide-124,26),"peaceask",peaceAskCity,new[]{"No city"}.Concat(theirs.Select(c=>c.name+" (size "+c.pop+")")).ToArray()),0,theirs.Count);offer.askCity=peaceAskCity>0?theirs[peaceAskCity-1].id:-1;y+=30;
  GUI.Label(new Rect(left,y+3,120,22),"You hand over",new GUIStyle(dipRow){fontSize=13});peaceGiveCity=Mathf.Clamp(Dropdown(new Rect(left+124,y,wide-124,26),"peacegive",peaceGiveCity,new[]{"No city"}.Concat(mine.Select(c=>c.name+" (size "+c.pop+")")).ToArray()),0,mine.Count);offer.giveCity=peaceGiveCity>0?mine[peaceGiveCity-1].id:-1;y+=30;
  peaceWithdraw=GUI.Toggle(new Rect(left,y,wide/2,24),peaceWithdraw," They withdraw from your cities");offer.withdraw=peaceWithdraw;
  GUI.Label(new Rect(left+wide/2+4,y+2,60,22),"Truce",new GUIStyle(dipRow){fontSize=13});peaceTruce=Dropdown(new Rect(left+wide/2+64,y,wide/2-64,26),"peacetruce",peaceTruce,new[]{"10 turns","20 turns","30 turns"});offer.truce=new[]{0,20,30}[Mathf.Clamp(peaceTruce,0,2)];y+=30;
  GUI.Label(new Rect(left,y,wide,18),"Your leverage: "+world.Leverage(0,partner)+" (their exhaustion beside yours, and who looks stronger). Demands beyond it are refused.",new GUIStyle(hudSmall){wordWrap=true});y+=32;
  return y;}
 void ResetPeaceTerms(){peaceAskCity=peaceGiveCity=peaceTruce=0;peaceWithdraw=false;}
 // ---- More page sections ----
 void DrawRequestInbox(){
  var asks=world.requests.Where(r=>r.to==0&&r.status==RequestStatus.Pending).ToList();
  GUILayout.Label("Requests awaiting your answer",heading);if(asks.Count==0)GUILayout.Label("None.",small);
  foreach(var r in asks){GUILayout.Label(world.DescribeRequest(r),body);GUILayout.Label("Answer within "+Math.Max(1,RequestKinds.AnswerTurns-(world.turn-r.created))+" turns; no answer counts as a refusal. "+RequestKinds.Texts[r.kind],small);
   GUILayout.BeginHorizontal();if(Button("Accept request #"+r.id))status=world.RespondToRequest(r.id,true)?"You have given your word.":"No longer possible: "+r.answer;if(Button("Refuse #"+r.id))world.RespondToRequest(r.id,false);GUILayout.EndHorizontal();}
  GUILayout.Label("Your commitments and deadlines",heading);var all=world.CommitmentSummary(0);if(all.Count==0)GUILayout.Label("None in force.",small);foreach(var line in all)GUILayout.Label(line,small);}
 void DrawRelationshipMemory(int partner){
  GUILayout.Label("What "+world.NationName(partner)+" remembers",heading);
  var mem=world.MemoryLines(partner,0);if(mem.Count==0)GUILayout.Label("Nothing of note yet.",small);
  foreach(var m in mem)GUILayout.Label(m.text+": "+(m.value>0?"+":"")+m.value+(m.turnsLeft<0?" (kept up while your envoy stays)":" — fades in about "+m.turnsLeft+" turns if nothing new happens"),small);
  GUILayout.Label("How to improve relations",heading);foreach(var t in world.ImproveTips(partner,0))GUILayout.Label("• "+t,small);
  GUILayout.Label("How they see you (their reckoning)",heading);foreach(var l in world.AiExplanation(partner,0))GUILayout.Label(l,small);
  GUILayout.Label("Rival behaviour this game: "+AiProfiles.Names[Mathf.Clamp(world.aiProfile,0,1)]+". "+AiProfiles.Texts[Mathf.Clamp(world.aiProfile,0,1)],small);}
}
}
