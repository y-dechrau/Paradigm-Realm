using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Three judgements per leader (owner brief, 27 Nov 2026), kept apart:
//   Opinion   warmth and willingness to compromise (Opinion, from memories and standing arrangements)
//   Trust     untested at first; raised by promises kept and help given, cut by promises broken (Trust, TrustCap)
//   Interest  what this leader stands to gain or lose from you now: trade, a common enemy, timber you supply, an open border
//             question, crowded land, fear. A friendly leader can still refuse to give up what it needs; a wary one can still
//             take payment up front.
// And one active issue for the portrait ("Cordial · Border concern").
public partial class World {
 public int Interest(int o,int t,List<string> why){
  int sum=0;Action<string,int> add=(s,v)=>{if(v==0)return;sum+=v;if(why!=null)why.Add((v>0?"+":"")+v+" "+s);};
  var p=Relation(o,t);if(p==null)return 0;
  if((p.pacts&(1<<Pacts.Commercial))!=0)add("trade worth keeping",10);
  if((p.pacts&(1<<Pacts.Defensive))!=0)add("our alliance",10);
  for(int x=0;x<civCount;x++){if(x==o||x==t)continue;if(AtWar(o,x)&&AtWar(t,x)){add("a common enemy",15);break;}}
  if(Accords().Any(a=>a.status==0&&a.receiver==o&&a.supplier==t))add("timber they supply",10);
  var issue=OpenIssue(o);if(issue!=null&&t==0)add(issue.motive==1?"land we both want":"an open border question",-15);
  {int close=cities.Count(c=>c.owner==t&&cities.Any(m=>m.owner==o&&Distance(m.x,m.y,c.x,c.y)<=6));if(close>0)add("crowded land between us",-Math.Min(15,5*close));}
  {int fear=Threat(o,t);if(fear>=20)add("fear of their strength",-fear/3);}
  if(AtWar(o,t))add("at war",-40);
  return Math.Max(-100,Math.Min(100,sum));
 }
 // The one thing on this leader's mind about you, if any, for the portrait.
 public string ActiveIssue(int o,int t){
  if(AtWar(o,t))return "At war";var issue=t==0?OpenIssue(o):null;if(issue!=null)return issue.motive==1?"Wants your land":issue.kind==1?"Needs timber":"Border concern";
  if(Accords().Any(a=>a.status==2&&(a.a==o||a.b==o)&&(a.a==t||a.b==t)&&a.keeper==t))return "Broken promise";
  if(t==0&&offers.Any(x=>x.from==o&&x.to==0&&x.status==0&&x.expires>turn))return "Has an offer";
  if(Rival(o,t))return "Rivalry";if(Threat(o,t)>=40)return "Fears you";
  if(Accords().Any(a=>a.status==0&&a.receiver==o&&a.supplier==t))return "Your customer";
  return "";
 }
 // Trust history in words: what was proved either way.
 public List<string> TrustHistory(int o,int t){
  var lines=new List<string>();var m=Mods(o,t);
  if(m[OpinionMods.KeptWord]>0)lines.Add("kept their word");if(m[OpinionMods.Aid]>0)lines.Add("helped in need");if(m[OpinionMods.Broken]<0)lines.Add("broke an agreement");
  if(supplyRecords!=null)foreach(var r in supplyRecords.Where(r=>r.receiver==o&&r.supplier==t)){if(r.delivered>0)lines.Add(r.delivered+" deliveries made");if(r.missed>0)lines.Add(r.missed+" missed");if(r.broken>0)lines.Add(r.broken+" promise"+(r.broken==1?"":"s")+" broken");}
  if(lines.Count==0)lines.Add("untested");return lines;
 }
 // The hover text for a portrait: attitude, the strongest reasons, trust and its history, interest.
 public string LeaderSummary(int o){
  var why=new List<string>();int interest=Interest(o,0,why);var reasons=OpinionLines(o,0).Where(l=>l.Value!=0).Take(4).Select(l=>(l.Value>0?"+":"")+l.Value+" "+l.Key).ToList();
  return NationName(o)+": "+Attitudes.Names[Attitude(o,0)]+", opinion "+Opinion(o,0)+(reasons.Count>0?" ("+string.Join("; ",reasons.ToArray())+")":"")+
   ". Trust "+Trust(o,0)+": "+string.Join(", ",TrustHistory(o,0).ToArray())+". Interest "+(interest>0?"+":"")+interest+(why.Count>0?" ("+string.Join("; ",why.ToArray())+")":"")+"."+(ActiveIssue(o,0)!=""?" On their mind: "+ActiveIssue(o,0)+".":"");
 }
}
}
namespace ParadigmRealm {
public partial class World {
 // Older saves carry the old, larger routine marks: on loading they are brought within the new caps once (envoy, gifts, trade).
 public void RebalanceOpinion(){if(diplomacy==null)return;foreach(var p in diplomacy){if(p==null)continue;foreach(var m in new[]{p.modsA,p.modsB}){if(m==null||m.Length<OpinionMods.Count)continue;
  m[OpinionMods.Envoy]=Math.Min(m[OpinionMods.Envoy],OpinionTuning.Envoy);m[OpinionMods.Gifts]=Math.Min(m[OpinionMods.Gifts],OpinionTuning.Gifts);m[OpinionMods.Dealings]=Math.Min(m[OpinionMods.Dealings],OpinionTuning.Dealings);}}}
}
}
