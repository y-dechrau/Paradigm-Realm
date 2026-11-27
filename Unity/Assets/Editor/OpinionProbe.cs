using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (owner report 27 Nov 2026: two neighbours at +33 and +80 with the same smiling mask): prints, for every met home
// civilisation, its opinion of you with every reason, its trust, attitude and the memory marks behind them.
// Unity -batchmode -executeMethod ParadigmRealm.OpinionProbe.Run -probeSave <save.json> -probeOut <log>
public static class OpinionProbe {
 static string Arg(string[] a,string k,string d){int i=Array.IndexOf(a,k);return i>=0&&i+1<a.Length?a[i+1]:d;}
 public static void Run(){
  var args=Environment.GetCommandLineArgs();var sb=new StringBuilder();
  try{
   foreach(var path in Arg(args,"-probeSave","").Split('|')){
    var w=JsonUtility.FromJson<World>(File.ReadAllText(path));w.UpgradeSave();w.lifeways=true;w.EnableStatecraft();w.EnableMaterials();
    sb.AppendLine("=== "+Path.GetFileName(path)+": turn "+w.turn);
    for(int o=1;o<w.civCount;o++){if(w.IsNativeNation(o)||!w.met[o])continue;
     sb.AppendLine(w.NationName(o)+": opinion "+w.Opinion(o,0)+", trust "+w.Trust(o,0)+", attitude "+Attitudes.Names[w.Attitude(o,0)]);
     foreach(var l in w.OpinionLines(o,0))if(l.Value!=0)sb.AppendLine("   "+(l.Value>0?"+":"")+l.Value+"  "+l.Key);}
   }
  }catch(Exception e){sb.AppendLine("FAILED: "+e);}
  File.WriteAllText(Arg(args,"-probeOut","opinion.txt"),sb.ToString());EditorApplication.Exit(0);
 }
}
}
