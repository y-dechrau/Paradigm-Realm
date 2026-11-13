using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// The player's research queue (owner request, 13 Nov 2026, with the Chronicle of Discovery screen): technologies to research
// after the current one, in order. "Add technology" queues the chosen technology after any of its prerequisites not yet known
// or queued. When a technology is discovered the first queued one that can be researched is taken up; with nothing queued the
// cheapest available is chosen, as before. No save version change: older saves load with an empty queue.
public partial class World {
 public List<int> researchQueue=new List<int>();
 List<int> Queue(){if(researchQueue==null)researchQueue=new List<int>();researchQueue.RemoveAll(t=>t<0||t>=TechnologyCatalog.Count||nations[0].tech[t]||!TechnologyCatalog.Active[t]);return researchQueue;}
 public IList<int> ResearchQueue(){return Queue().AsReadOnly();}
 public string QueueReason(int t){if(t<0||t>=TechnologyCatalog.Count||!TechnologyCatalog.Active[t])return "Unknown technology.";if(nations[0].tech[t])return "Already discovered.";if(research==t)return "Being researched now.";if(Queue().Contains(t))return "Already in the queue.";return "";}
 // Adds t and, before it, every prerequisite still missing, in an order that can be researched. Returns how many were added.
 public int QueueTechnology(int t){
  if(QueueReason(t)!="")return 0;var q=Queue();int before=q.Count;var seen=new HashSet<int>();
  Action<int> add=null;add=x=>{if(!seen.Add(x)||nations[0].tech[x]||x==research||q.Contains(x))return;foreach(int p in Requirements[x])add(p);q.Add(x);};
  add(t);return q.Count-before;}
 public bool Unqueue(int t){return Queue().Remove(t);}
 // Called when the player's research completes: the first queued technology that can be researched now, else the cheapest.
 int NextPlayerResearch(){
  var q=Queue();var open=AvailableTech(0);
  foreach(int t in q)if(open.Contains(t)){q.Remove(t);return t;}
  return open.OrderBy(t=>ResearchCost(t,0)).DefaultIfEmpty(-1).First();}
}
}
