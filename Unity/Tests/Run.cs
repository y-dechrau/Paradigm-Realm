using System;
using System.Linq;
using ParadigmRealm;
class Run {
 static void Main(string[] args){if(args.Length>0&&args[0]=="diplomacy"){DiplomacyReport(args.Length>1?int.Parse(args[1]):300);return;}DiplomacyChecks.Run();Console.WriteLine("PASS advanced diplomacy");SaveRoundTrip();Console.WriteLine("PASS save round trip with war, reparations and commitments in force");FoundationChecks.Run();Console.WriteLine("PASS Foundation");ContentChecks.Run();Console.WriteLine("PASS Content");EmpireChecks.Run();Console.WriteLine("PASS Empire (including 3 x 50 turn campaigns)");StrategyChecks.Run();Console.WriteLine("PASS city strategy and terrain invariants");SettlementChecks.Run();Console.WriteLine("PASS settlement strategy, fog, AI colony and city bonuses");Measure();}
 // A save written and read back (System.Text.Json here; Unity's JsonUtility round trip is in RealmTools.Checks) during a war,
 // with reparations and requests in force: everything diplomatic survives and the game keeps running.
 static void SaveRoundTrip(){
  var w=World.New(640,natives:false,rations:true);w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.met[1]=w.met[2]=true;w.Relation(1,2).contact=true;foreach(var n in w.nations)n.gold=500;w.gold=500;
  w.DeclareWar(1,0);w.turn+=8;w.AddExhaustion(1,0,80);var peace=w.Propose(new DiplomaticOffer{from=0,to=1,kind=TreatyKind.Peace,duration=10,askPerTurn=2,withdraw=true});
  w.DeclareWar(2,0);var opts=new System.Text.Json.JsonSerializerOptions{IncludeFields=true};
  var back=System.Text.Json.JsonSerializer.Deserialize<World>(System.Text.Json.JsonSerializer.Serialize(w,opts),opts);
  if(peace.status!=1||back==null||!back.AtWar(0,2)||back.AtWar(0,1)||back.contracts.Count!=w.contracts.Count||back.requests.Count!=w.requests.Count||back.requests.Any(r=>r.status!=w.requests.First(x=>x.id==r.id).status)||back.Relation(0,2).lastDeclarer!=2)throw new Exception("Save round trip: diplomacy changed on the way through");
  for(int i=0;i<5;i++)back.EndTurn();if(!back.Valid())throw new Exception("Save round trip: the loaded game is not valid after 5 turns");}
 // Turn time and save size with the parallel worlds and native peoples (a report, not a check).
 static void Measure(){foreach(int size in new[]{1,2,3}){var w=World.New(500+size,"Timing",0,1,1,0,size,1,4);var sw=System.Diagnostics.Stopwatch.StartNew();for(int t=0;t<10;t++)w.EndTurn();sw.Restart();for(int t=0;t<20;t++)w.EndTurn();double ms=sw.Elapsed.TotalMilliseconds/20;
  string json=System.Text.Json.JsonSerializer.Serialize(w,new System.Text.Json.JsonSerializerOptions{IncludeFields=true});Console.WriteLine("MEASURE "+World.MapSizeNames[size]+": "+w.civCount+" civilisations, "+w.tiles.Length+" tiles, "+ms.ToString("0")+" ms a turn at turn 30, save about "+(json.Length/1048576.0).ToString("0.0")+" MB");}}
 // How the rebuilt diplomacy behaves over whole rival games (a report, not a check): run with "diplomacy [turns]".
 static void DiplomacyReport(int turns){
  foreach(int seed in new[]{701,702,703}){var w=World.New(seed,"Talks",0,1,1,0,1,2,6,hex:true,natives:false,rations:true);w.Found(w.units.First(u=>u.owner==0&&u.type==0));
   int wars=0,peaces=0,warTurns=0,maxPacts=0,rivalries=0,claimsMade=0,captures=0,unjust=0;var atWar=new System.Collections.Generic.HashSet<int>();var rival=new System.Collections.Generic.HashSet<int>();int lastClaims=0;var owners=w.cities.ToDictionary(c=>c.id,c=>c.owner);string firstWar="";
   var termed=new System.Collections.Generic.HashSet<int>();var reqSeen=new System.Collections.Generic.Dictionary<int,DiplomaticRequest>();
   for(int t=0;t<turns&&w.result=="";t++){w.EndTurn();foreach(var o in w.offers.Where(o=>o.status==0&&o.to==0).ToArray())w.RespondToOffer(o.id,w.Opinion(0,o.from)>=0);
    foreach(var r in w.requests.Where(r=>r.status==RequestStatus.Pending&&r.to==0).ToArray())w.RespondToRequest(r.id,w.Opinion(0,r.from)>=0&&r.kind!=RequestKinds.JoinWar);
    foreach(var r in w.requests)reqSeen[r.id]=r;foreach(var o in w.offers)if(o.kind==TreatyKind.Peace&&o.status==1&&(o.askPerTurn+o.givePerTurn+o.askGold+o.giveGold>0||o.withdraw||o.askCity>=0||o.giveCity>=0))termed.Add(o.id);
    int pacts=0;foreach(var p in w.diplomacy){int key=p.a*100+p.b;bool war=w.AtWar(p.a,p.b);if(war){warTurns++;if(atWar.Add(key)){wars++;if(firstWar=="")firstWar="turn "+w.turn;if(p.events.Any(e=>e.Contains("without cause")&&e.StartsWith("Turn "+w.turn+":")))unjust++;}}else if(atWar.Remove(key))peaces++;
     for(int k=0;k<Pacts.Count;k++)if((p.pacts&(1<<k))!=0)pacts++;foreach(int side in new[]{0,1}){int rk=key*2+side;bool r=side==0?p.rivalA>=0:p.rivalB>=0;if(r){if(rival.Add(rk))rivalries++;}else rival.Remove(rk);}}
    maxPacts=Math.Max(maxPacts,pacts);if(w.claims.Count>lastClaims)claimsMade+=w.claims.Count-lastClaims;lastClaims=w.claims.Count;
    foreach(var c in w.cities){int was;if(owners.TryGetValue(c.id,out was)&&was!=c.owner)captures++;owners[c.id]=c.owner;}
    if(w.turn%100==0){var counts=new int[Pacts.Count];foreach(var p in w.diplomacy)for(int k=0;k<Pacts.Count;k++)if((p.pacts&(1<<k))!=0)counts[k]++;var att=new int[Attitudes.Names.Length];for(int a=1;a<w.civCount;a++)for(int b=0;b<w.civCount;b++)if(a!=b&&w.HasContact(a,b))att[w.Attitude(a,b)]++;
     Console.WriteLine("  seed "+seed+" turn "+w.turn+": pacts "+string.Join("/",counts)+" (borders/NAP/commerce/research/defence); attitudes "+string.Join(", ",Enumerable.Range(0,att.Length).Where(i=>att[i]>0).Select(i=>Attitudes.Names[i]+" "+att[i]))+"; cities "+string.Join(",",Enumerable.Range(0,w.civCount).Select(o=>w.cities.Count(c=>c.owner==o)))+"; player opinion of rivals "+string.Join(",",Enumerable.Range(1,w.civCount-1).Select(o=>w.Opinion(o,0))));}}
   Console.WriteLine("DIPLOMACY seed "+seed+": "+w.turn+" turns, "+(w.result==""?"no result":w.result)+"; wars "+wars+" ("+unjust+" without cause, first "+firstWar+"), peaces "+peaces+", pair-turns at war "+warTurns+", cities taken "+captures+", rivalries "+rivalries+", claims "+claimsMade+", most pacts at once "+maxPacts+", valid "+w.Valid());
   Console.WriteLine("  requests: "+string.Join(", ",Enumerable.Range(0,RequestKinds.Count).Select(k=>RequestKinds.Names[k]+" "+reqSeen.Values.Count(r=>r.kind==k)+" ("+string.Join("/",Enumerable.Range(0,6).Select(s=>reqSeen.Values.Count(r=>r.kind==k&&r.status==s)))+")"))+" [pending/active/fulfilled/broken/refused/cancelled]; peaces with terms "+termed.Count+"; rival-to-rival requests "+reqSeen.Values.Count(r=>r.from!=0&&r.to!=0));}}}
