using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
namespace ParadigmRealm {
// Freeze recorder (owner report 26 Nov 2026: the game stopped responding for a while; Windows logged a hang, not a crash, and it
// could not be repeated from the last autosave). A background thread watches a heartbeat the game sets every frame. If no frame
// has finished for FreezeSeconds, it writes what the game was last doing (screen, input event, turn step, rival decision) to the
// player log and to freeze-report.txt beside the saves, and when the game recovers, how long it was stuck. It never touches the
// game itself, so it cannot cause or worsen a freeze.
public static class FreezeWatch {
 public const double FreezeSeconds=4;
 // While the window is in the background Unity stops drawing on purpose; that is not a freeze (owner's report of 27 Nov 2026 was
 // full of such pauses, taken while screenshots were made), so the watch sleeps until the window has focus again.
 static volatile bool focused=true;public static void Focus(bool on){focused=on;Beat();}
 static long beat=Stopwatch.GetTimestamp();static volatile string where="starting",step="",decision="";static string folder;static Thread watcher;
 public static void Start(string saveFolder){if(watcher!=null)return;folder=saveFolder;watcher=new Thread(Watch){IsBackground=true,Name="FreezeWatch"};watcher.Start();}
 public static void Beat(){Interlocked.Exchange(ref beat,Stopwatch.GetTimestamp());}
 public static void Where(string w){where=w;}
 public static void Step(string s){step=s;}
 public static void Decision(string d){decision=d;}
 static void Watch(){
  bool frozen=false;long since=0;
  while(true){
   Thread.Sleep(500);if(!focused){Beat();continue;}long now=Stopwatch.GetTimestamp();double idle=(now-Interlocked.Read(ref beat))/(double)Stopwatch.Frequency;
   if(!frozen&&idle>=FreezeSeconds){frozen=true;since=Interlocked.Read(ref beat);Write("FREEZE: no frame for "+idle.ToString("0.0")+" s. Last: "+where+(step!=""?"; turn step "+step:"")+(decision!=""?"; decision "+decision:""));}
   else if(frozen&&idle<1){frozen=false;Write("FREEZE ended after "+((now-since)/(double)Stopwatch.Frequency).ToString("0.0")+" s. Last: "+where+(step!=""?"; turn step "+step:"")+(decision!=""?"; decision "+decision:""));}
  }
 }
 static void Write(string line){string stamped=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"  "+line;UnityEngine.Debug.Log(stamped);try{if(folder!=null)File.AppendAllText(Path.Combine(folder,"freeze-report.txt"),stamped+Environment.NewLine);}catch{}}
}
}
