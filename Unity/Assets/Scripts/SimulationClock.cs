using System;
using System.Linq;
namespace ParadigmRealm {
// Stage 1 of Paradigm Realms Live (LIVE-ARCHITECTURE-SPEC.md §4): one central simulation clock.
//  - World.day counts days since the start; 30 days make a month, and one month is one of the old turns, so every turn-based rule
//    and balance figure keeps its meaning. The monthly work is the existing turn pipeline (TurnSteps).
//  - Daily: your units walk. Each unit still has its whole month's movement, but earns it day by day (MaxMoves x days / 30), so
//    it covers the same ground in a month as a turn used to, step by step across the days. Orders, exploring, road routes and
//    auto develop all walk daily. Rivals still plan and move at the month (Stage 6 moves armies daily).
//  - Weekly (days 1, 8, 16, 23 of a month) and yearly hooks are reserved for later stages.
//  - liveClock is switched on by the Live interface for every game it runs; checks and old code paths see the old turn rules.
public partial class World {
 public const int DaysPerMonth=30;
 public int day;public bool liveClock;
 public static readonly string[] MonthNames={"January","February","March","April","May","June","July","August","September","October","November","December"};
 public static readonly string[] SeasonNames={"Winter","Spring","Summer","Autumn"};
 public int DayOfMonth{get{return day%DaysPerMonth+1;}}
 public int MonthOfYear{get{return (day/DaysPerMonth)%12;}}
 public string Season{get{int m=MonthOfYear;return SeasonNames[m<2||m==11?0:m<5?1:m<8?2:3];}}
 public string DateText(){return Season+", "+DayOfMonth+" "+MonthNames[MonthOfYear]+" "+Calendar();}
 public string ShortDate(){return DayOfMonth+" "+MonthNames[MonthOfYear].Substring(0,3)+" "+Calendar();}
 public static bool WeekDay(int dayOfMonth){return dayOfMonth==1||dayOfMonth==8||dayOfMonth==16||dayOfMonth==23;}
 // One day. Returns true when this day ended a month: the caller then runs the monthly work (TurnSteps), which advances turn.
 public bool TickDay(){
  if(result!="")return false;
  DailyMovement();
  if(WeekDay(DayOfMonth))TickWeek();
  day++;
  return day%DaysPerMonth==0;
 }
 void TickWeek(){}
 // Movement a unit may have used by the end of `elapsed` days of this month.
 int AllowedBy(Unit u,int elapsed){int max=MaxMoves(u);return max*Math.Max(0,Math.Min(DaysPerMonth,elapsed))/DaysPerMonth;}
 // Runs act with the unit limited to what it has earned so far this month; the rest is held back and given back afterwards.
 public void WithAllowance(Unit u,int elapsed,Action act){
  if(u==null)return;int max=MaxMoves(u),reserve=Math.Max(0,max-AllowedBy(u,elapsed));if(u.moves<=reserve)return;
  u.moves-=reserve;try{act();}finally{if(units.Contains(u))u.moves+=reserve;}
 }
 void DailyMovement(){
  if(!liveClock)return;int elapsed=day%DaysPerMonth+1;
  foreach(var u in units.Where(a=>a.owner==0).ToList()){
   if(!units.Contains(u)||result!="")continue;
   if(u.hasOrder)WithAllowance(u,elapsed,()=>ContinueOrder(u));
   else if(u.exploring)WithAllowance(u,elapsed,()=>ExploreStep(u));
   else if(u.roadOrder&&u.job==0)WithAllowance(u,elapsed,()=>RoadStep(u));
   else if(u.autoWork&&u.job==0&&u.flee==0)WithAllowance(u,elapsed,()=>ImproveAI(u));
  }
 }
 // An order given between days walks only as far as the unit has earned so far this month.
 internal void ContinueNow(Unit u){if(liveClock&&u.owner==0)WithAllowance(u,day%DaysPerMonth,()=>ContinueOrder(u));else ContinueOrder(u);}
 // The whole rest of the month at once (checks, and the old End Turn path): every remaining day, then the monthly work.
 public void AdvanceMonth(){if(liveClock){int guard=0;while(guard++<DaysPerMonth&&!TickDay()){}}foreach(int step in TurnSteps()){}}
}
}
