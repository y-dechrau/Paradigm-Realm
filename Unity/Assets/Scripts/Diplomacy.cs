using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
public enum TreatyKind { Access, Friendship, Alliance, Research, Trade, Peace, BorderPromise, JointWar }
[Serializable] public class DiplomaticPair {
 public bool atWar; public int a,b,trustA,trustB,grievanceA,grievanceB,accessUntil,friendUntil,allianceUntil,researchUntil,peaceUntil,warStarted;
 public int promiseAUntil,promiseBUntil,denounceA=-1,denounceB=-1,embargoUntil,lastA=-10,lastB=-10;
 public bool contact,embassyA,embassyB;
 public List<string> events=new List<string>();
}
[Serializable] public class DiplomaticOffer {
 public int id,from,to,duration=15,created,expires,status,target=-1,giveGold,askGold,givePerTurn,askPerTurn,giveResource,askResource;
 public TreatyKind kind; public string answer="";
 public DiplomaticOffer Copy(){return (DiplomaticOffer)MemberwiseClone();}
}
[Serializable] public class DiplomaticContract {
 public int id,from,to,start,end,givePerTurn,askPerTurn,giveResource,askResource;
}
public partial class World {
 public List<DiplomaticPair> diplomacy=NewDiplomacy(3);
 public List<DiplomaticOffer> offers=new List<DiplomaticOffer>();
 public List<DiplomaticContract> contracts=new List<DiplomaticContract>();
 public int nextDiplomaticId=1; public int[] warWeariness=new int[3]; public bool rivalWar;
 // Pairs are stored in order (0,1),(0,2)...(1,2)... so Relation can find one by index.
 static List<DiplomaticPair> NewDiplomacy(int n){var list=new List<DiplomaticPair>();for(int a=0;a<n-1;a++)for(int b=a+1;b<n;b++)list.Add(new DiplomaticPair{a=a,b=b});return list;}
 public void MigrateDiplomacy(){diplomacy=NewDiplomacy(3);offers=new List<DiplomaticOffer>();contracts=new List<DiplomaticContract>();warWeariness=new int[3];nextDiplomaticId=1;rivalWar=true;foreach(var p in diplomacy){p.atWar=p.a!=0&&rivalWar;p.contact=p.a!=0||met[p.b];p.trustB=p.a==0?nations[p.b].relations:0;p.warStarted=AtWar(p.a,p.b)?turn:0;}}
 public DiplomaticPair Relation(int a,int b){int lo=Math.Min(a,b),hi=Math.Max(a,b);if(lo>=0&&hi<civCount&&lo!=hi&&diplomacy!=null&&diplomacy.Count==civCount*(civCount-1)/2){var q=diplomacy[lo*(2*civCount-lo-1)/2+(hi-lo-1)];if(q.a==lo&&q.b==hi)return q;}return diplomacy.Find(p=>p.a==lo&&p.b==hi);}
 public bool HasContact(int a,int b){return a>=0&&b>=0&&a<civCount&&b<civCount&&a!=b&&(a==0||b==0?met[a==0?b:a]:Relation(a,b).contact);}
 public bool Friends(int a,int b){var p=Relation(a,b);return p!=null&&(p.friendUntil>turn||p.allianceUntil>turn);}
 public bool Allied(int a,int b){var p=Relation(a,b);return p!=null&&p.allianceUntil>turn;}
 public bool Embargoed(int a,int b){var p=Relation(a,b);return p!=null&&p.embargoUntil>turn;}
 public bool HasAccess(int a,int b){var p=Relation(a,b);return p!=null&&!AtWar(a,b)&&(p.accessUntil>turn||Allied(a,b)||(a==0||b==0)&&openBorders[a==0?b:a]);}
 public int Grievances(int observer,int other){var p=Relation(observer,other);return observer==p.a?p.grievanceA:p.grievanceB;}
 public void ChangeTrust(int observer,int other,int amount){var p=Relation(observer,other);if(observer==p.a)p.trustA=Math.Max(-100,Math.Min(100,p.trustA+amount));else p.trustB=Math.Max(-100,Math.Min(100,p.trustB+amount));}
 void Grieve(int victim,int offender,int amount){var p=Relation(victim,offender);if(victim==p.a)p.grievanceA=Math.Min(200,p.grievanceA+amount);else p.grievanceB=Math.Min(200,p.grievanceB+amount);}
 public int Opinion(int observer,int other){var p=Relation(observer,other);int trust=observer==p.a?p.trustA:p.trustB;return Math.Max(-100,Math.Min(100,trust-Grievances(observer,other)/2+(nations[observer].government==nations[other].government?5:0)+(Friends(observer,other)?10:0)+(Allied(observer,other)?15:0)-(AtWar(observer,other)?25:0)-(Embargoed(observer,other)?10:0)));}
 public string RelationshipBreakdown(int observer,int other){var p=Relation(observer,other);return "Opinion "+Opinion(observer,other)+" | Trust "+(observer==p.a?p.trustA:p.trustB)+" | Grievances "+Grievances(observer,other)+". Same government +5; friendship +10; alliance +15; war -25; embargo -10; each 2 grievances -1.";}
 public string Personality(int owner){return owner==0?"Your decisions shape your reputation.":new[]{"Mercantile: values trade and reliable payments.","Scholarly: favours research cooperation.","Martial: respects strength and keeps its word to allies.","Builder: prefers peace and steady growth.","Expansionist: watches its neighbours closely."}[(owner-1)%5];}
 void DiplomacyEvent(int a,int b,string message){var p=Relation(a,b);p.events.Insert(0,"Turn "+turn+": "+message);if(p.events.Count>12)p.events.RemoveAt(12);if(a==0||b==0||met[a]&&met[b])Note(NationName(a)+" / "+NationName(b)+": "+message);}
 int Treasury(int owner){return owner==0?gold:nations[owner].gold;}
 void AddDiplomaticGold(int owner,int amount){nations[owner].gold=Treasury(owner)+amount;if(owner==0)gold=nations[0].gold;}
 public string WarReason(int a,int b){if(!HasContact(a,b))return "First contact required.";if(result!="")return "The game has ended.";if(AtWar(a,b))return "Already at war.";var p=Relation(a,b);if(p.peaceUntil>turn)return "Peace treaty protects this civilisation for "+(p.peaceUntil-turn)+" turns.";if(Friends(a,b))return "End friendship/alliance before declaring war; breaking it causes grievances.";return "";}
 public bool FormalWarAvailable(int a,int b){var p=Relation(a,b);int t=a==p.a?p.denounceA:p.denounceB;return t>=0&&turn-t>=3&&turn-t<15;}
 public bool DeclareWar(int a,int b,bool defensive=false){if(WarReason(a,b)!="")return false;var allies=Enumerable.Range(0,civCount).Where(c=>c!=a&&c!=b&&Allied(c,b)).ToArray();var p=Relation(a,b);if(a==0||b==0){war[a==0?b:a]=true;openBorders[a==0?b:a]=false;}else p.atWar=true;p.warStarted=turn;p.accessUntil=p.researchUntil=p.friendUntil=p.allianceUntil=0;p.promiseAUntil=p.promiseBUntil=0;contracts.RemoveAll(c=>SamePair(c.from,c.to,a,b));Grieve(b,a,defensive?0:FormalWarAvailable(a,b)?25:45);ChangeTrust(b,a,-25);if(a==0)nations[b].relations=Math.Max(-100,nations[b].relations-25);networks=null;DiplomacyEvent(a,b,(defensive?"Defensive":FormalWarAvailable(a,b)?"Formal":"Surprise")+" war declared.");
  if(!defensive)foreach(int c in allies){if(c==0||a==0)met[c==0?a:c]=true;else Relation(c,a).contact=true;if(WarReason(c,a)=="")DeclareWar(c,a,true);else if(!AtWar(c,a)){Grieve(b,c,15);DiplomacyEvent(c,b,"Defence blocked by another treaty; trust damaged.");ChangeTrust(b,c,-10);}}return true;}
 bool MakePeace(int a,int b){if(!HasContact(a,b)||!AtWar(a,b))return false;if(a==0||b==0)war[a==0?b:a]=false;else Relation(a,b).atWar=false;Relation(a,b).peaceUntil=turn+8;networks=null;DiplomacyEvent(a,b,"Peace agreed. Eight turns of protection.");return true;}
 public bool SetWar(int other,bool value){return value?DeclareWar(0,other):MakePeace(0,other);}
 public bool SetBorders(int other){if(!HasContact(0,other)||AtWar(0,other))return false;if(HasAccess(0,other))return EndTreaty(0,other,TreatyKind.Access);return Propose(new DiplomaticOffer{from=0,to=other,kind=TreatyKind.Access}).status==1;}
 public bool OfferPeace(int other){return Propose(new DiplomaticOffer{from=0,to=other,kind=TreatyKind.Peace}).status==1;}
 public bool SendEmbassy(int a,int b){if(!HasContact(a,b)||AtWar(a,b)||result!=""||Treasury(a)<25)return false;var p=Relation(a,b);if(a==p.a?p.embassyA:p.embassyB)return false;if(a==p.a)p.embassyA=true;else p.embassyB=true;AddDiplomaticGold(a,-25);ChangeTrust(b,a,8);DiplomacyEvent(a,b,"Embassy established for 25 gold; trust +8.");return true;}
 public bool Denounce(int a,int b){if(!HasContact(a,b)||AtWar(a,b)||Friends(a,b)||result!="")return false;var p=Relation(a,b);int t=a==p.a?p.denounceA:p.denounceB;if(t>=0&&turn-t<15)return false;if(a==p.a)p.denounceA=turn;else p.denounceB=turn;Grieve(b,a,8);DiplomacyEvent(a,b,"Denounced. Formal war available after three turns, for twelve turns.");return true;}
 public bool ImposeEmbargo(int a,int b){if(!HasContact(a,b)||Friends(a,b)||Embargoed(a,b)||result!="")return false;Relation(a,b).embargoUntil=turn+10;Relation(a,b).researchUntil=0;contracts.RemoveAll(c=>SamePair(c.from,c.to,a,b));Grieve(b,a,12);DiplomacyEvent(a,b,"Ten-turn embargo: contracts ended and caravan income suspended.");return true;}
 public bool EndTreaty(int a,int b,TreatyKind kind){if(!HasContact(a,b)||result!="")return false;var p=Relation(a,b);bool active=false;switch(kind){case TreatyKind.Access:if(Allied(a,b))return false;active=HasAccess(a,b);p.accessUntil=0;if(a==0||b==0)openBorders[a==0?b:a]=false;break;case TreatyKind.Friendship:case TreatyKind.Alliance:active=Friends(a,b);p.friendUntil=p.allianceUntil=p.researchUntil=0;break;case TreatyKind.Research:active=p.researchUntil>turn;p.researchUntil=0;break;case TreatyKind.Trade:active=contracts.RemoveAll(c=>SamePair(c.from,c.to,a,b))>0;break;}if(!active)return false;Grieve(b,a,kind==TreatyKind.Friendship||kind==TreatyKind.Alliance?25:8);ChangeTrust(b,a,-10);DiplomacyEvent(a,b,kind+" broken: trust -10 and grievances increased.");return true;}
 public int DiplomaticScience(int owner){return diplomacy.Count(p=>(p.a==owner||p.b==owner)&&p.researchUntil>turn&&!AtWar(p.a,p.b))*2;}
 public int WarUnhappiness(int owner){return Math.Min(4,warWeariness[owner]/6);}
 public string SettlementDiplomacyWarning(int owner,int x,int y){
  for(int other=0;other<civCount;other++){if(!HasContact(owner,other))continue;var p=Relation(owner,other);if((owner==p.a?p.promiseAUntil:p.promiseBUntil)<=turn)continue;
   if(cities.Any(c=>c.owner==other&&Distance(c.x,c.y,x,y)<=6&&(VisibleTo(owner,c.x,c.y)||owner==0&&At(c.x,c.y).seen&&At(c.x,c.y).rememberedOwner==other&&!string.IsNullOrEmpty(At(c.x,c.y).rememberedCity))))return "Warning: settling here risks breaking your border promise to "+NationName(other)+" (25 grievances, trust -15).";
  }return "";
 }
 void DiplomacyOnFound(City city){for(int other=0;other<civCount;other++){if(other==city.owner||!HasContact(city.owner,other))continue;if(!cities.Any(c=>c.owner==other&&Distance(c.x,c.y,city.x,city.y)<=6&&VisibleTo(other,city.x,city.y)))continue;var p=Relation(city.owner,other);bool promised=(city.owner==p.a?p.promiseAUntil:p.promiseBUntil)>turn;Grieve(other,city.owner,promised?25:6);if(promised){if(city.owner==p.a)p.promiseAUntil=0;else p.promiseBUntil=0;ChangeTrust(other,city.owner,-15);}DiplomacyEvent(city.owner,other,promised?"Border promise broken by nearby settlement: 25 grievances.":"Nearby settlement causes 6 grievances.");}}
 void DiplomacyOnCapture(int conqueror,int victim){Grieve(victim,conqueror,30);warWeariness[victim]=Math.Min(60,warWeariness[victim]+4);foreach(int other in Enumerable.Range(0,civCount).Where(o=>o!=victim&&o!=conqueror&&HasContact(o,conqueror)&&Friends(o,victim)))Grieve(other,conqueror,10);DiplomacyEvent(conqueror,victim,"City captured: 30 grievances and increased war weariness.");}
 static bool SamePair(int a,int b,int c,int d){return a==c&&b==d||a==d&&b==c;}
}
}
