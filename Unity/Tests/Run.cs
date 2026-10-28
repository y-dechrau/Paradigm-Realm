using System;
using ParadigmRealm;
class Run {
 static void Main(){DiplomacyChecks.Run();Console.WriteLine("PASS advanced diplomacy");FoundationChecks.Run();Console.WriteLine("PASS Foundation");ContentChecks.Run();Console.WriteLine("PASS Content");EmpireChecks.Run();Console.WriteLine("PASS Empire (including 3 x 50 turn campaigns)");StrategyChecks.Run();Console.WriteLine("PASS city strategy and terrain invariants");SettlementChecks.Run();Console.WriteLine("PASS settlement strategy, fog, AI colony and city bonuses");}
}
