using System;
using System.Linq;
using System.Text;
namespace ParadigmRealm {
// Early diplomacy rebalanced (owner brief, 27 Nov 2026): the same opening over 50 turns with idle contact, a fulfilled agreement
// and a broken border promise; two rival leaders left to themselves; saving and loading half-way gives no extra marks.
public static class OpinionBalanceChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Opinion balance: "+what);}
 public static string Report="";
 static void Turns(World w,int n){for(int k=0;k<n;k++)w.EndTurn();}
 public static void Run(){
  var sb=new StringBuilder();
  // Idle contact: met, at peace, nothing done.
  var idle=World.ValleyTalkScenario("plenty");idle.valleyIssues.Clear();Turns(idle,50);int idleOp=idle.Opinion(1,0),idleTrust=idle.Trust(1,0);
  sb.AppendLine("idle, turn "+idle.turn+": "+idle.RelationshipBreakdown(1,0));
  Check(Math.Abs(idleOp)<=20,"fifty turns of idle contact stay near neutral ("+idleOp+")");
  // A valuable fulfilled agreement: timber delivered for twelve turns.
  var kept=World.ValleyTalkScenario("lasting");var issue=kept.AskAboutValley(1);Check(issue!=null,"the lasting shortage opens a talk");kept.AgreeSupplyCounter(issue,3,12);Turns(kept,50);
  int keptOp=kept.Opinion(1,0),keptTrust=kept.Trust(1,0);sb.AppendLine("fulfilled agreement, turn "+kept.turn+": "+kept.RelationshipBreakdown(1,0)+" | "+string.Join(", ",kept.TrustHistory(1,0).ToArray()));
  Check(keptTrust>=idleTrust+OpinionTuning.FulfilledTrust/2,"a fulfilled agreement earns trust ("+keptTrust+" against idle "+idleTrust+")");
  Check(keptOp>idleOp,"and warmth ("+keptOp+" against idle "+idleOp+" | KEPT "+kept.RelationshipBreakdown(1,0)+" | IDLE "+idle.RelationshipBreakdown(1,0)+")");
  // A broken border promise: paid to leave the ford, then settled it.
  var broken=World.ValleyTalkScenario("farmland");var bi=broken.AskAboutValley(1);Check(bi!=null&&broken.AcceptValley(bi)=="","the farmland talk is accepted for payment");
  var site=broken.tiles[bi.site];var settler=broken.units.First(u=>u.owner==0&&u.type==0);settler.x=site.x;settler.y=site.y;settler.moves=1;broken.Found(settler);Turns(broken,50);
  int brokenOp=broken.Opinion(1,0),brokenTrust=broken.Trust(1,0);sb.AppendLine("broken promise, turn "+broken.turn+": "+broken.RelationshipBreakdown(1,0)+" | "+string.Join(", ",broken.TrustHistory(1,0).ToArray()));
  Check(brokenOp<idleOp-5,"a broken promise is felt fifty turns later ("+brokenOp+" against idle "+idleOp+")");Check(brokenTrust<=idleTrust,"and trust does not recover past an untested neighbour's ("+brokenTrust+")");
  Check(broken.TrustHistory(1,0).Any(s=>s.Contains("broken")),"the trust history names the broken promise");
  // +80 is reachable only through shared history, not routine contact: the routine maximum stays well short of it.
  int routine=OpinionTuning.Coexistence+OpinionTuning.Trade+OpinionTuning.Dealings+OpinionTuning.Gifts+OpinionTuning.Envoy+OpinionTuning.SameGovernment+OpinionTuning.Friendship;
  Check(routine<50,"routine contact alone cannot reach +50 ("+routine+")");
  // Rivals among themselves: three home civilisations, fifty turns, no runaway warmth between strangers.
  var ai=World.New(2711,"Three",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);ai.EnableMaterials();Turns(ai,50);
  for(int a=1;a<ai.civCount;a++)for(int b=1;b<ai.civCount;b++){if(a==b||!ai.HasContact(a,b))continue;int op=ai.Opinion(a,b);sb.AppendLine("rival "+a+" of rival "+b+": "+op+" ("+ai.RelationshipBreakdown(a,b)+")");Check(op<=60,"two rivals do not reach +60 without shared history ("+op+")");}
  // Saving and loading half-way neither repeats nor loses a mark.
  var s1=World.ValleyTalkScenario("lasting");var si=s1.AskAboutValley(1);s1.AgreeSupplyCounter(si,3,12);Turns(s1,20);
  var s2=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(s1));s2.RebalanceOpinion();
  Check(Enumerable.Range(0,OpinionMods.Count).All(k=>s1.OpinionMod(1,0,k)==s2.OpinionMod(1,0,k))&&s1.Trust(1,0)==s2.Trust(1,0),"a save and load keeps every mark and trust exactly");
  s2.RebalanceOpinion();Check(s2.Opinion(1,0)==s1.Opinion(1,0),"loading twice adds nothing");
  Turns(s1,10);Turns(s2,10);Check(Math.Abs(s1.Opinion(1,0)-s2.Opinion(1,0))<=2&&s1.valleyAccords.Sum(a=>a.delivered)==s2.valleyAccords.Sum(a=>a.delivered),"the loaded game goes on as the original (opinion "+s1.Opinion(1,0)+" / "+s2.Opinion(1,0)+")");
  Report=sb.ToString();UnityEngine.Debug.Log("OPINION BALANCE: "+Report);
 }
}
}
