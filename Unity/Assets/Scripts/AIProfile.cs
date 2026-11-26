using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace ParadigmRealm {
// Development aid (20 Nov 2026): when RealmProbe switches it on, the time each kind of rival unit decision takes is added up by
// name. Off (null) in play, when P just runs the decision.
public partial class World {
 public static Dictionary<string,double> decisionTimes;
 static void AddProfile(string k,long t0){double ms=(Stopwatch.GetTimestamp()-t0)*1000.0/Stopwatch.Frequency;double had;decisionTimes.TryGetValue(k,out had);decisionTimes[k]=had+ms;}
 bool P(string k,Func<bool> f){FreezeWatch.Decision(k);if(decisionTimes==null)return f();long t0=Stopwatch.GetTimestamp();bool r=f();AddProfile(k,t0);return r;}
 void P(string k,Action f){FreezeWatch.Decision(k);if(decisionTimes==null){f();return;}long t0=Stopwatch.GetTimestamp();f();AddProfile(k,t0);}
}
}
