using System;
using ParadigmRealm;
class Run {
 static void Main(){DiplomacyChecks.Run();Console.WriteLine("PASS advanced diplomacy");FoundationChecks.Run();Console.WriteLine("PASS Foundation");ContentChecks.Run();Console.WriteLine("PASS Content");EmpireChecks.Run();Console.WriteLine("PASS Empire (including 3 x 50 turn campaigns)");StrategyChecks.Run();Console.WriteLine("PASS city strategy and terrain invariants");SettlementChecks.Run();Console.WriteLine("PASS settlement strategy, fog, AI colony and city bonuses");Measure();}
 // Turn time and save size with the parallel worlds and native peoples (a report, not a check).
 static void Measure(){foreach(int size in new[]{1,2,3}){var w=World.New(500+size,"Timing",0,1,1,0,size,1,4);var sw=System.Diagnostics.Stopwatch.StartNew();for(int t=0;t<10;t++)w.EndTurn();sw.Restart();for(int t=0;t<20;t++)w.EndTurn();double ms=sw.Elapsed.TotalMilliseconds/20;
  string json=System.Text.Json.JsonSerializer.Serialize(w,new System.Text.Json.JsonSerializerOptions{IncludeFields=true});Console.WriteLine("MEASURE "+World.MapSizeNames[size]+": "+w.civCount+" civilisations, "+w.tiles.Length+" tiles, "+ms.ToString("0")+" ms a turn at turn 30, save about "+(json.Length/1048576.0).ToString("0.0")+" MB");}}
}
