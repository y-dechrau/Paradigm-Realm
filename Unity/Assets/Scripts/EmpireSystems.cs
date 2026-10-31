using System;
using System.Linq;
using System.Collections.Generic;
namespace ParadigmRealm {
[Serializable] public class TradeRoute {public int origin,destination,originOwner,destinationOwner;}
public partial class World {
    public List<TradeRoute> tradeRoutes=new List<TradeRoute>();
    public static readonly string[] Governments={"Tribal rule","Monarchy","Republic","Democracy","Collectivism"};
    public static readonly string[] GovernmentTech={"","laws","philosophy","printing","industrialisation"};
    public static readonly string[] GovernmentEffects={"6 free unit upkeep; basic economy", "8 free unit upkeep; +1 content/city", "2 free unit upkeep; +2 gold and +2 science/city", "No free unit upkeep; +3 gold and +4 science/city; overseas wars cause unrest", "10 free unit upkeep; +3 production/city, -2 science"};
    public static bool IsWonder(int b){return b>=60&&b<=63;}
    public static bool IsProject(int b){return b>=70&&b<=72;}
    public bool OwnWonder(int owner,int id){return cities.Any(c=>c.owner==owner&&HasBuilding(c,id));}
    public bool AdjacentWater(City c){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){if(dx==0&&dy==0)continue;var t=Step(c.x,c.y,dx,dy);if(t!=null&&(t.terrain==4||t.terrain==6||t.terrain==12))return true;}return false;}
    public int GovernmentSupport(int owner){return new[]{6,8,2,0,10}[nations[owner].government];}
    public bool ChangeGovernment(int owner,int government){if(owner<0||owner>=civCount||government<0||government>=Governments.Length||result!="")return false;var n=nations[owner];if(n.government==government||n.revolution>0||(government>0&&!HasTech(owner,GovernmentTech[government])))return false;n.government=government;n.revolution=2;if(owner==0)Note("Revolution: two turns of reduced output before "+Governments[government]+".");return true;}
    public int ExtraHappiness(City c){var n=nations[c.owner];return c.entertainers*2+n.luxury*2+(n.government==1?1:0)+(HasBuilding(c,36)?2:0)+(HasBuilding(c,42)?3:0)+(OwnWonder(c.owner,61)?2:0)-(n.revolution>0?2:0)-(n.government==3&&units.Any(u=>u.owner==c.owner&&Power[u.type]>0&&At(u.x,u.y).owner>=0&&At(u.x,u.y).owner!=c.owner&&AtWar(c.owner,At(u.x,u.y).owner))?2:0);}
    public void NormaliseSpecialists(City c){while(c.scientists+c.merchants+c.entertainers>c.pop){if(c.scientists>0)c.scientists--;else if(c.merchants>0)c.merchants--;else c.entertainers--;}}
    public bool AdjustSpecialist(City c,int type,int delta){if(c==null||!cities.Contains(c)||result!=""||type<0||type>2||(delta!=1&&delta!=-1))return false;int value=type==0?c.scientists:type==1?c.merchants:c.entertainers;if(delta<0&&value==0||delta>0&&c.scientists+c.merchants+c.entertainers>=c.pop)return false;if(type==0)c.scientists+=delta;else if(type==1)c.merchants+=delta;else c.entertainers+=delta;return true;}
    public int GrowthLimit(City c){return HasBuilding(c,41)?20:HasBuilding(c,37)?12:FreshWater(c.x,c.y)?8:6;}
    void ApplyEmpireYields(City c,ref int food,ref int production,ref int science,ref int goldYield){var n=nations[c.owner];science+=c.scientists*3+(n.capital==c.id?DiplomaticScience(c.owner):0);goldYield+=c.merchants*3;
        // Stage 8: flat building bonuses as the design states (University +3); research pacts count once, at the capital.
        if(HasBuilding(c,38))science+=3;if(HasBuilding(c,40))production=production*3/2;
        if(OwnWonder(c.owner,60))science+=3;if(OwnWonder(c.owner,62))goldYield+=3;
        if(n.government==2){science+=2;goldYield+=2;}if(n.government==3){science+=4;goldYield+=3;}if(n.government==4){production+=3;science=Math.Max(1,science-2);}
        if(HasBuilding(c,39))goldYield+=3;
        if(n.revolution>0){production=Math.Max(1,production/2);science=Math.Max(1,science/2);goldYield/=2;}
    }
    public static string BuildingEffect(int b){switch(b){case 7:return "Retains food after growth";case 8:return "+2 science";case 9:return "+2 defence for units in the city";case 30:return "+3 production";case 31:return "+2 gold";case 32:return "New units start as Veterans; units in this city heal fully each turn";case 33:return "+2 content citizens";case 34:return "Reduces city upkeep by 2";case 35:return "+2 healing in city";case 36:return "+2 content citizens";case 37:return "Allows population up to 12";case 38:return "+3 science";case 39:return "+3 gold";case 40:return "+50% production before government bonuses";case 41:return "Allows population up to 20";case 42:return "+3 content citizens";case 43:return "+1 food per worked water tile";case 44:return "On the coast of the city's territory; while a road joins it to the city: +2 gold, +1 food on worked sea resources, builds Galleys and War Galleys";case 60:return "WORLD WONDER: +3 science in all your cities";case 61:return "WORLD WONDER: +2 content citizens in all your cities";case 62:return "WORLD WONDER: +3 gold in all your cities";case 63:return "WORLD WONDER: all new units start as Veterans";case 70:case 71:case 72:return "TRANSCENDENCE: stage "+(b-69)+" of 3; activates over "+ProjectActivationBase+" turns. Complete all three to win. Losing this city loses the project.";default:return "";}}
    public int RushPrice(City c,bool building){int b=building?c.buildingProject:c.build;if(b<0)return 0;int progress=building?c.buildingProduction:c.production;return Math.Max(0,CostFor(c,b)-progress)*2*(progress==0?2:1);}
    public bool Rush(City c,bool building){if(c==null||c.owner!=0||result!="")return false;int b=building?c.buildingProject:c.build,price=RushPrice(c,building);if(b<0||IsProject(b)||BuildReason(c,b)!=""||price<=0||gold<price)return false;gold-=price;nations[0].gold=gold;if(building)c.buildingProduction=CostFor(c,b);else c.production=CostFor(c,b);Note(c.name+": paid "+price+" gold; project ready next turn (unit space permitting).");return true;}
    void CancelWonderRivals(City winner,int id){foreach(var c in cities){c.buildingQueue.RemoveAll(b=>b==id);if(c==winner||c.buildingProject!=id)continue;nations[c.owner].gold+=c.buildingProduction/2;c.buildingProduction=0;c.buildingProject=c.buildingQueue.Count>0?c.buildingQueue[0]:-1;if(c.buildingQueue.Count>0)c.buildingQueue.RemoveAt(0);if(c.owner==0){gold=nations[0].gold;Note("A rival finished "+BuildName(id)+"; half our invested production returned as gold.");}}}
    public bool RouteActive(TradeRoute r){var a=cities.Find(c=>c.id==r.origin);var b=cities.Find(c=>c.id==r.destination);return a!=null&&b!=null&&a.owner==r.originOwner&&b.owner==r.destinationOwner&&!AtWar(a.owner,b.owner)&&!Embargoed(a.owner,b.owner);}
    public int TradeIncome(int owner){return tradeRoutes.Where(RouteActive).Sum(r=>(r.originOwner==owner?3:0)+(r.destinationOwner==owner?3:0));}
    public string TradeReason(Unit u,City c){if(u==null||c==null||u.type!=11||!units.Contains(u)||u.moves<=0)return "Caravan needs movement";var origin=cities.Find(a=>a.id==u.homeCity&&a.owner==u.owner);if(origin==null)return "No surviving home city";if(origin==c||Distance(origin.x,origin.y,c.x,c.y)<4)return "Choose another, more distant city";if(Distance(u.x,u.y,c.x,c.y)>1)return "Move beside the destination city";if(c.owner!=u.owner&&Embargoed(u.owner,c.owner))return "Embargo blocks foreign trade";if(c.owner!=u.owner&&(AtWar(u.owner,c.owner)||!CanEnter(u.owner,At(c.x,c.y))))return "Requires peaceful open borders";if(tradeRoutes.Any(r=>r.origin==origin.id&&r.destination==c.id||r.origin==c.id&&r.destination==origin.id))return "These cities already trade";if(tradeRoutes.Count(r=>r.origin==origin.id||r.destination==origin.id)>=3||tradeRoutes.Count(r=>r.origin==c.id||r.destination==c.id)>=3)return "A city already has three routes";return "";}
    public bool DeliverCaravan(Unit u,City c){if(result!=""||TradeReason(u,c)!="")return false;var origin=cities.Find(a=>a.id==u.homeCity);int reward=20+Distance(origin.x,origin.y,c.x,c.y)*2+(c.owner!=u.owner?20:0);tradeRoutes.Add(new TradeRoute{origin=origin.id,destination=c.id,originOwner=u.owner,destinationOwner=c.owner});if(u.owner==0){gold+=reward;nations[0].gold=gold;Note("Trade established: "+origin.name+" to "+c.name+". +"+reward+" gold; +3 gold/turn per endpoint while peaceful.");}else nations[u.owner].gold+=reward;units.Remove(u);return true;}
    public bool HelpWonder(Unit u,City c){if(result!=""||u==null||c==null||!units.Contains(u)||u.type!=11||u.owner!=c.owner||Distance(u.x,u.y,c.x,c.y)>1||u.moves<=0||!IsWonder(c.buildingProject)||BuildReason(c,c.buildingProject)!="")return false;c.buildingProduction+=40;units.Remove(u);if(c.owner==0)Note("Caravan supplied 40 production to "+BuildName(c.buildingProject)+".");return true;}
    void AICaravan(Unit u){var target=cities.Where(c=>TradeReason(u,c)=="").FirstOrDefault();if(target!=null){DeliverCaravan(u,target);return;}target=cities.Where(c=>c.owner==u.owner&&c.id!=u.homeCity).OrderBy(c=>Distance(u.x,u.y,c.x,c.y)).FirstOrDefault();if(target!=null)Order(u,target.x,target.y);}
    public bool GiftGold(int other){if(result!=""||other<1||other>=civCount||!met[other]||war[other]||gold<25)return false;gold-=25;nations[0].gold=gold;nations[other].gold+=25;nations[other].relations=Math.Min(100,nations[other].relations+10);ChangeTrust(other,0,10);Note("Gift to "+NationName(other)+": relations improved.");return true;}
    public int TechPrice(int id){return Math.Max(25,ResearchCost(id)/2);}
    public bool BuyTechnology(int other,int id){if(result!=""||other<1||other>=civCount||id<0||id>=TechnologyCatalog.Count||!met[other]||war[other]||!nations[other].tech[id]||!AvailableTech().Contains(id)||gold<TechPrice(id)||Opinion(other,0)<0||Embargoed(0,other))return false;gold-=TechPrice(id);nations[0].gold=gold;nations[other].gold+=TechPrice(id);tech[id]=true;nations[0].tech=tech;if(research==id){research=AvailableTech().OrderBy(t=>ResearchCost(t)).DefaultIfEmpty(-1).First();nations[0].research=research;science=research<0?0:nations[0].progress[research];}Note("Purchased "+TechNames[id]+" from "+NationName(other)+".");CheckVictory();return true;}
    public string CombatPreview(Unit attacker,int x,int y){
        if(attacker==null||Power[attacker.type]==0||!VisibleTo(attacker.owner,x,y)||Distance(attacker.x,attacker.y,x,y)!=1)return "";
        var enemy=BestDefender(attacker,x,y);if(enemy==null)return "";
        return ExchangeText(attacker,enemy,"Attack")+(AtWar(attacker.owner,enemy.owner)?"":" Declares war.");
    }
    // Exact outcome of one exchange, as the real attack will resolve it (combat is deterministic).
    public string ExchangeText(Unit attacker,Unit enemy,string kind){
        int dealt,taken;Exchange(attacker,enemy,out dealt,out taken);int a=Strength(attacker,enemy,false),d=Strength(enemy,attacker,true);
        return kind+" preview: "+UnitNames[attacker.type]+" "+a+" vs "+UnitNames[enemy.type]+" "+d+". You deal "+dealt+" (enemy health "+enemy.hp+" to "+Math.Max(0,enemy.hp-dealt)+(enemy.hp<=dealt?", destroyed":"")+"), you take "+taken+(taken==0?" (no retaliation against ranged attacks)":" (your health "+attacker.hp+" to "+Math.Max(0,attacker.hp-taken)+(attacker.hp<=taken?", destroyed":"")+")")+".";
    }
    public string RangedPreview(Unit attacker,Unit target){var e=target==null?null:BestDefender(attacker,target.x,target.y);return e==null?"":ExchangeText(attacker,e,"Ranged");}
    void SeedVillages(){var r=new Random(seed^317);foreach(var t in tiles)if(!Barren(t.terrain)&&cities.All(c=>Distance(c.x,c.y,t.x,t.y)>4)&&units.All(u=>Distance(u.x,u.y,t.x,t.y)>4)&&r.Next(60)==0)t.village=true;}
    // Tribal villages (owner request): the first visitor still gets a gift (gold, research, or healing and a victory); any
    // civilisation whose unit occupies the village gains the tribe's local knowledge (the land within 4 tiles is revealed)
    // and its allegiance. A village allied to a civilisation pays it tax every turn while the village lies inside that
    // civilisation's borders or within 4 tiles of them. Another civilisation can win the village over by occupying it.
    void VisitVillage(Unit u){var t=At(u.x,u.y);if(!t.village||t.villageOwner==u.owner)return;
        if(t.villageOwner<0){int reward=new Random(seed^u.x*73^u.y*997).Next(3);string message;if(reward==0){nations[u.owner].gold+=30;if(u.owner==0)gold+=30;message="30 gold";}else if(reward==1){int researchId=u.owner==0?research:nations[u.owner].research;if(researchId>=0){nations[u.owner].progress[researchId]+=20;if(u.owner==0)science+=20;}message="20 research points";}else{u.hp=10;Victory(u);message="full healing and a victory towards the next rank";}if(u.owner==0)Note("Tribal village: "+message+".");}
        int before=t.villageOwner;t.villageOwner=u.owner;
        foreach(var near in tiles)if(Distance(near.x,near.y,t.x,t.y)<=4)LearnTile(u.owner,near);
        if(u.owner==0)Note("The tribe shares its knowledge of the land around it and pledges allegiance"+(before>=0?" (won over from "+NationName(before)+")":"")+"; it pays "+VillageTax+" gold a turn while within 4 tiles of your borders.");
        else if(before==0)Note("A tribal village has been won over by "+NationName(u.owner)+".");}
    public const int VillageTax=2;
    public bool VillageInReach(int owner,Tile t){if(t.owner==owner)return true;for(int dy=-4;dy<=4;dy++)for(int dx=-4;dx<=4;dx++){var n=Step(t.x,t.y,dx,dy);if(n!=null&&n.owner==owner)return true;}return false;}
    public int VillageTribute(int owner){int sum=0;foreach(var t in tiles)if(t.village&&t.villageOwner==owner&&VillageInReach(owner,t))sum+=VillageTax;return sum;}
    // Scouts and every combat unit can explore on their own (owner request); exploring stops when an enemy comes near.
    public static bool CanExplore(int type){return type==3||Power[type]>0||IsNaval(type);}
    public bool AutoExplore(Unit u){if(u==null||u.owner!=0||!units.Contains(u)||!CanExplore(u.type)||result!="")return false;CancelActivity(u);u.exploring=true;ExploreStep(u);return true;}
    // Auto-explore: a breadth-first search over known, enterable land finds the nearest tile beside unexplored ground, and the
    // unit follows a proper route there, so it no longer gets stuck in dead ends (owner request).
    void ExploreStep(Unit u){
        for(int guard=0;guard<12&&u.exploring&&u.moves>0&&units.Contains(u);guard++){
            if(VisibleThreat(u)){u.exploring=false;if(u.owner==0)Note(UnitNames[u.type]+" stopped exploring: enemy nearby.");return;}
            // Nearest land tile beside undiscovered ground (owner request); one the unit cannot reach is skipped, not a reason to stop.
            var skip=new HashSet<int>();Tile target=null;List<Tile> path=null;
            for(int tries=0;tries<6;tries++){target=ExploreTarget(u,skip);if(target==null)break;path=Route(u,target.x,target.y);if(path.Count>0)break;skip.Add(target.y*Width+target.x);target=null;}
            if(target==null){u.exploring=false;if(u.owner==0)Note(UnitNames[u.type]+" has explored everything it can reach.");return;}
            var t=path[0];if(u.moves<StepCost(u,u.x,u.y,t))return;
            if(!Move(u,t.x,t.y))return;
        }
    }
    Tile ExploreTarget(Unit u,HashSet<int> skip=null){
        var known=nations[u.owner].known;var seen=new HashSet<int>{u.y*Width+u.x};var queue=new Queue<Tile>();queue.Enqueue(At(u.x,u.y));int budget=4000;
        while(queue.Count>0&&budget-->0){
            var t=queue.Dequeue();
            if(t.x!=u.x||t.y!=u.y){bool frontier=false;for(int dy=-1;dy<=1&&!frontier;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n!=null&&!known[n.y*Width+n.x]){frontier=true;break;}}
                if(frontier&&(skip==null||!skip.Contains(t.y*Width+t.x))&&!units.Any(a=>a.owner!=u.owner&&a.x==t.x&&a.y==t.y))return t;}
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){
                if(dx==0&&dy==0)continue;var n=Step(t.x,t.y,dx,dy);if(n==null)continue;int idx=n.y*Width+n.x;
                if(seen.Contains(idx)||!known[idx])continue;seen.Add(idx);
                int terrain=u.owner==0&&!Visible(n.x,n.y)?KnownTerrain(n):n.terrain;
                // Land explorers keep to land: water is never a target or a way through.
                if((IsNaval(u.type)?!Passable(u,n,terrain):Barren(terrain)||IsWater(terrain)||IsImpassable(terrain))||!CanEnter(u.owner,n))continue;
                queue.Enqueue(n);
            }
        }
        return null;
    }    bool VisibleThreat(Unit u){return units.Any(e=>AtWar(u.owner,e.owner)&&VisibleTo(u.owner,e.x,e.y)&&Distance(u.x,u.y,e.x,e.y)<=4);}
    void ProcessEmpireOrders(int owner){foreach(var u in units.Where(a=>a.owner==owner).ToList()){if(u.sentry&&VisibleThreat(u)){u.sentry=false;if(owner==0)Note(UnitNames[u.type]+" sentry spotted an enemy.");}if(u.exploring)ExploreStep(u);}}
    void EmpireAI(int owner){var n=nations[owner];AIChooseEdicts(owner);if(n.government==0&&HasTech(owner,"laws"))ChangeGovernment(owner,1);n.luxury=cities.Any(c=>c.owner==owner&&Happiness(c)<0)&&n.gold>20?1:0;foreach(var c in cities.Where(c=>c.owner==owner)){if(c.build<0&&HasTech(owner,"currency")&&cities.Count(a=>a.owner==owner)>1&&units.Count(u=>u.owner==owner&&u.type==11)==0&&tradeRoutes.Count(r=>r.originOwner==owner)<2)ChooseProject(c,UnitBuild(11),false);}}
    void UpgradeEmpireSave(){if(tradeRoutes==null)tradeRoutes=new List<TradeRoute>();if(cities!=null)foreach(var c in cities)if(c!=null)NormaliseSpecialists(c);}
    bool EmpireValid(){if(tradeRoutes==null||nations==null||cities==null)return false;if(nations.Any(n=>n!=null&&(n.government<0||n.government>=Governments.Length||n.revolution<0||n.revolution>2||n.luxury<0||n.luxury>2)))return false;if(cities.Any(c=>c!=null&&(c.scientists<0||c.merchants<0||c.entertainers<0||c.scientists+c.merchants+c.entertainers>c.pop)))return false;return tradeRoutes.All(r=>r!=null&&r.origin!=r.destination&&r.originOwner>=0&&r.originOwner<civCount&&r.destinationOwner>=0&&r.destinationOwner<civCount&&cities.Any(c=>c.id==r.origin)&&cities.Any(c=>c.id==r.destination));}
}
}
