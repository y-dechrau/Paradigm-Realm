using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
namespace ParadigmRealm {
// Development aid (24 Nov 2026): plays whole games with no drawing in which half the rivals are made to stay small and Centralised
// (four cities) and half to grow large and Expansive, and compares them: cities, citizens, technologies, science, shields, soldiers
// and gold. Run: Unity -batchmode -executeMethod ParadigmRealm.StatecraftProbe.Run -probeOut <file> [-probeTurns 250] [-probeSeeds 6]
// [-probeMode 0 as built, 1 the two sizes with no Statecraft at all].
public static class StatecraftProbe {
 static string Arg(string[] a,string key,string fallback){int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
 public static void Checks(){try{NovemberRequestChecks.Run();StatecraftChecks.Run();UnityEngine.Debug.Log("STATECRAFT CHECKS PASS");}catch(Exception e){UnityEngine.Debug.Log("STATECRAFT CHECKS FAIL "+e);}EditorApplication.Exit(0);}
 public static void Run(){
  var args=Environment.GetCommandLineArgs();string outPath=Arg(args,"-probeOut","statecraft-probe.txt");int turns=int.Parse(Arg(args,"-probeTurns","250")),seeds=int.Parse(Arg(args,"-probeSeeds","6"));bool off=Arg(args,"-probeMode","0")=="1";
  var sb=new StringBuilder();var sums=new Dictionary<string,double[]>();int smallAhead=0,largeAhead=0;
  try{for(int s=0;s<seeds;s++){
    var w=World.New(5100+s*37,"Probe",0,1,1,0,2,2,7,hex:true,natives:false,level:DifficultyTable.Steward,wild:true,rations:true,animalShare:100);
    w.nationalForces=true;w.EnableMaterials();w.lifeways=true;w.barbarians=true;w.EnableStatecraft();w.statecraftForced=new Dictionary<int,int>();
    for(int o=1;o<w.civCount;o++)w.statecraftForced[o]=(o+s)%2==0?StatecraftRules.Centralised:StatecraftRules.Expansive;
    if(off)w.statecraftRulesOff=true;
    var founder=w.units.FirstOrDefault(u=>u.owner==0&&u.type==0);if(founder!=null)w.Found(founder);
    for(int t=1;t<=turns&&w.result=="";t++){
     for(int o=1;o<w.civCount;o++)w.Relation(0,o).peaceUntil=w.turn+10;
     if(w.research<0||!w.AvailableTech(0).Contains(w.research))w.SelectResearch(w.AvailableTech(0).DefaultIfEmpty(0).First());
     w.researchPrompts.Clear();w.buildPrompts.Clear();w.buildNotices.Clear();w.rivalCalls.Clear();w.newContacts.Clear();w.EndTurn();
     if(t%100==0||t==turns){foreach(int kind in new[]{1,2}){var those=Enumerable.Range(1,w.civCount-1).Where(o=>w.statecraftForced[o]==kind&&w.Survives(o)).ToList();if(those.Count==0)continue;
       var v=new double[]{those.Average(o=>w.cities.Count(c=>c.owner==o)),those.Average(o=>w.Citizens(o)),those.Average(o=>w.nations[o].tech.Count(x=>x)),those.Average(o=>w.SciencePerTurn(o)),those.Average(o=>w.cities.Where(c=>c.owner==o).Sum(c=>w.Yield(c)[1])),those.Average(o=>w.units.Where(u=>u.owner==o&&World.Power[u.type]>0).Sum(u=>World.Attack[u.type]+World.Defence[u.type]+2*u.promotion)),those.Average(o=>w.nations[o].gold),1,those.Count};
       string key="turn "+t+(kind==1?" small":" large");double[] sum;if(!sums.TryGetValue(key,out sum)){sum=new double[9];sums[key]=sum;}for(int k=0;k<9;k++)sum[k]+=v[k];
       sb.AppendLine("seed "+s+" "+key+" ("+those.Count+" realms): cities "+v[0].ToString("0.0")+", citizens "+v[1].ToString("0")+", technologies "+v[2].ToString("0.0")+", science "+v[3].ToString("0")+", shields "+v[4].ToString("0")+", army "+v[5].ToString("0")+", gold "+v[6].ToString("0"));
       if(t==turns&&kind==2){}}}
    }
    var best=Enumerable.Range(1,w.civCount-1).Where(o=>w.Survives(o)).OrderByDescending(o=>w.nations[o].tech.Count(x=>x)).ThenByDescending(o=>w.SciencePerTurn(o)).First();if(w.statecraftForced[best]==1)smallAhead++;else largeAhead++;
    sb.AppendLine("seed "+s+": most advanced at the end is a "+(w.statecraftForced[best]==1?"small":"large")+" realm; result '"+w.result+"'");File.WriteAllText(outPath,sb.ToString());
   }
   sb.AppendLine();sb.AppendLine(off?"AVERAGES with no Statecraft rules":"AVERAGES with Statecraft");foreach(var kv in sums){double g=kv.Value[7];sb.AppendLine(kv.Key+": cities "+(kv.Value[0]/g).ToString("0.0")+", citizens "+(kv.Value[1]/g).ToString("0")+", technologies "+(kv.Value[2]/g).ToString("0.0")+", science "+(kv.Value[3]/g).ToString("0")+", shields "+(kv.Value[4]/g).ToString("0")+", army "+(kv.Value[5]/g).ToString("0")+", gold "+(kv.Value[6]/g).ToString("0")+", realms alive "+(kv.Value[8]/g).ToString("0.0"));}
   sb.AppendLine("Most advanced civilisation at the end: small in "+smallAhead+" games, large in "+largeAhead+".");
  }catch(Exception e){sb.AppendLine("FAILED: "+e);}
  File.WriteAllText(outPath,sb.ToString());EditorApplication.Exit(0);
 }
}
}
