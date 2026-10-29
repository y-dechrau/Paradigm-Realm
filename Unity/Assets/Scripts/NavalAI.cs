using System;
using System.Collections.Generic;
using System.Linq;

namespace ParadigmRealm
{
    // Naval AI for rival civilisations (owner request). It uses only what the rival knows, like the rest of the AI.
    // - Research: Sailing and Engineering are wanted once a city's borders reach the sea.
    // - Ports: coastal-reach cities build one (two port cities per civilisation at most); workers lay the road that links it.
    // - Boats: one per civilisation explores the seas.
    // - War Galleys: a port city keeps one at war (two at most per port), and builds one when enemy ships are seen; they hunt
    //   enemy ships within 8 tiles and otherwise guard their port.
    // - Galleys ferry settlers: when a settler has no site it can reach on foot, the civilisation builds a Galley (one at a
    //   time); the galley picks the settler up from the shore, sails to a good coastal site it has seen on other land and puts
    //   the settler ashore, where the usual settlement planning founds the city.
    public partial class World
    {
        [NonSerialized] HashSet<int> aiFerryWanted;
        HashSet<int> FerryWanted{get{if(aiFerryWanted==null)aiFerryWanted=new HashSet<int>();return aiFerryWanted;}}
        bool AtSea(Unit u){var t=At(u.x,u.y);return t!=null&&IsWater(t.terrain);}
        bool PortReach(int owner){return cities.Any(c=>c.owner==owner&&PortSite(c)!=null);}

        // A port for coastal-reach cities, at most two per civilisation.
        bool AIWantsPort(City c){return PortSite(c)!=null&&!HasBuilding(c,PortBuilding)&&cities.Count(a=>a.owner==c.owner&&HasBuilding(a,PortBuilding))<2;}

        // Road tiles still needed between each unlinked port and its city: a straight walk from the city towards the port.
        HashSet<int> PortRoadNeeds(int owner)
        {
            var needs=new HashSet<int>();
            foreach(var c in cities.Where(a=>a.owner==owner&&!a.portLinked))
            {
                var port=PortTile(c);if(port==null||port.x==c.x&&port.y==c.y)continue;
                int x=c.x,y=c.y;
                for(int step=0;step<6;step++)
                {
                    int dx=Math.Sign(DeltaX(x,port.x)),dy=Math.Sign(port.y-y);x+=dx;y+=dy;var t=At(x,y);if(t==null)break;x=t.x;y=t.y;
                    if(x==port.x&&y==port.y)break;
                    if(!t.road&&!Barren(t.terrain))needs.Add(t.y*Width+t.x);
                }
            }
            return needs;
        }

        // What a rival city should launch, or -1.
        internal int AINavalBuild(City c)
        {
            int owner=c.owner;var own=units.Where(a=>a.owner==owner).ToList();
            // Count vessels already being built in the civilisation's other cities as well as those afloat.
            Func<int,int> fleet=type=>own.Count(a=>a.type==type)+cities.Count(a=>a.owner==owner&&a!=c&&a.build==UnitBuild(type));
            if(BuildReason(c,UnitBuild(BoatUnit))==""&&fleet(BoatUnit)==0&&nations[owner].known.Any(k=>!k))return UnitBuild(BoatUnit);
            if(!c.portLinked)return -1;
            if(FerryWanted.Contains(owner)&&fleet(GalleyUnit)==0&&BuildReason(c,UnitBuild(GalleyUnit))=="")return UnitBuild(GalleyUnit);
            // An invasion galley when an enemy city lies overseas (a second one if a ferry is also wanted).
            if(InvasionTarget(owner)!=null&&fleet(GalleyUnit)<(FerryWanted.Contains(owner)?2:1)&&BuildReason(c,UnitBuild(GalleyUnit))=="")return UnitBuild(GalleyUnit);
            bool atWar=Enumerable.Range(0,civCount).Any(o=>o!=owner&&AtWar(owner,o));
            bool enemyShips=units.Any(a=>IsNaval(a.type)&&a.owner!=owner&&AtWar(owner,a.owner)&&VisibleTo(owner,a.x,a.y));
            int guards=fleet(WarGalleyUnit),ports=cities.Count(a=>a.owner==owner&&a.portLinked);
            if((atWar&&guards<ports||enemyShips&&guards<ports*2)&&BuildReason(c,UnitBuild(WarGalleyUnit))=="")return UnitBuild(WarGalleyUnit);
            return -1;
        }

        void NavalUnitAI(Unit u)
        {
            if(u.moves<=0)return;
            if(u.type==BoatUnit){if(!u.exploring)AutoExplore(u);return;}
            if(u.type==WarGalleyUnit)
            {
                var prey=units.Where(a=>IsNaval(a.type)&&AtWar(u.owner,a.owner)&&VisibleTo(u.owner,a.x,a.y)&&Distance(u.x,u.y,a.x,a.y)<=8).OrderBy(a=>Distance(u.x,u.y,a.x,a.y)).FirstOrDefault();
                if(prey!=null){if(Distance(u.x,u.y,prey.x,prey.y)==1)Move(u,prey.x,prey.y);else Order(u,prey.x,prey.y);return;}
                var home=cities.Where(c=>c.owner==u.owner&&c.portLinked).OrderBy(c=>Distance(u.x,u.y,c.x,c.y)).FirstOrDefault();
                var port=home==null?null:PortTile(home);
                if(port!=null&&Distance(u.x,u.y,port.x,port.y)>2){var berth=LaunchTile(home,u.type);if(berth!=null)Order(u,berth.x,berth.y);}
                return;
            }
            if(u.type==GalleyUnit)FerryAI(u);
        }

        // Galley: carry a stranded settler to a coastal site on other land.
        void FerryAI(Unit g)
        {
            var troops=Cargo(g).Where(a=>a.type!=0).ToList();
            if(troops.Count>0||!FerryWanted.Contains(g.owner)&&!Cargo(g).Any(a=>a.type==0)&&InvasionTarget(g.owner)!=null){InvasionAI(g,troops);return;}
            var aboard=Cargo(g).FirstOrDefault(a=>a.type==0);
            if(aboard!=null)
            {
                var site=FerrySite(g.owner,g.x,g.y,aboard);
                if(site==null){FerryWanted.Remove(g.owner);return;}
                if(Distance(g.x,g.y,site.x,site.y)<=1){aboard.moves=Math.Max(aboard.moves,1);if(Move(aboard,site.x,site.y))FerryWanted.Remove(g.owner);return;}
                var water=AdjacentWater(site,g);if(water!=null)Order(g,water.x,water.y);return;
            }
            // Fetch the nearest settler that has nowhere to go on foot.
            var waiting=units.Where(a=>a.owner==g.owner&&a.type==0&&!AtSea(a)&&!a.hasOrder&&FerryWanted.Contains(g.owner)).OrderBy(a=>Distance(a.x,a.y,g.x,g.y)).FirstOrDefault();
            if(waiting==null)return;
            if(Distance(waiting.x,waiting.y,g.x,g.y)<=1)return; // the settler steps aboard on its own turn
            var shore=AdjacentWater(At(waiting.x,waiting.y),g);if(shore!=null)Order(g,shore.x,shore.y);
        }
        // A sailable tile beside a land tile, reachable by this ship.
        Tile AdjacentWater(Tile land,Unit ship)
        {
            Tile best=null;int bestLength=int.MaxValue;
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
            {
                var n=At(land.x+dx,land.y+dy);if(n==null||!Sailable(n.terrain)||!Passable(ship,n,n.terrain))continue;
                if(n.x==ship.x&&n.y==ship.y)return n;
                var path=Route(ship,n.x,n.y);if(path.Count>0&&path.Count<bestLength){bestLength=path.Count;best=n;}
            }
            return best;
        }
        // The nearest known coastal site within 15 tiles with room for a city.
        Tile FerrySite(int owner,int x,int y,Unit settler)
        {
            var candidates=new List<Tile>();
            for(int dy=-15;dy<=15;dy++)for(int dx=-15;dx<=15;dx++)
            {
                var t=At(x+dx,y+dy);if(t==null||IsWater(t.terrain)||!nations[owner].known[t.y*Width+t.x]||!NextToSea(t.x,t.y))continue;
                // Land seen before (not necessarily in sight now), legal to settle, with room around it; the settler plans properly once ashore.
                if(SiteReason(owner,t.x,t.y,false)!=""||SettlementThreat(owner,t.x,t.y))continue;
                int room=0;for(int ry=-2;ry<=2;ry++)for(int rx=-2;rx<=2;rx++){var k=At(t.x+rx,t.y+ry);if(k!=null&&!IsWater(k.terrain)&&nations[owner].known[k.y*Width+k.x])room++;}
                if(room>=6)candidates.Add(t);
            }
            return candidates.OrderBy(t=>Distance(x,y,t.x,t.y)).ThenBy(t=>t.y*Width+t.x).FirstOrDefault();
        }
        // ---------- invasions by sea ----------
        // A rival at war with a civilisation whose cities it cannot reach over land builds a Galley at a connected port.
        // Spare troops (not a city's last defender) march to the galley and board; once two or three are aboard (or no
        // more are coming) it sails to the enemy coast, puts them ashore beside the target city (or straight into it when
        // they can attack from the deck), and goes home for the next wave. Ashore, the usual AI attacks what it sees.
        // Landmasses ignore polar ice: a crossing over the pole is possible but no way to move an army.
        [NonSerialized] int[] landmass;[NonSerialized] int landmassTurn=-1;
        int Landmass(int x,int y)
        {
            if(landmass==null||landmass.Length!=tiles.Length||landmassTurn!=turn)
            {
                landmass=new int[tiles.Length];int next=1;var queue=new Queue<int>();
                for(int i=0;i<tiles.Length;i++)
                {
                    if(landmass[i]!=0||Barren(tiles[i].terrain))continue;landmass[i]=next;queue.Enqueue(i);
                    while(queue.Count>0){int k=queue.Dequeue();var t=tiles[k];for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=At(t.x+dx,t.y+dy);if(n==null)continue;int j=n.y*Width+n.x;if(landmass[j]==0&&!Barren(n.terrain)){landmass[j]=next;queue.Enqueue(j);}}}
                    next++;
                }
                landmassTurn=turn;
            }
            var at=At(x,y);return at==null?0:landmass[at.y*Width+at.x];
        }
        // The overseas enemy city to strike: at war, its tile known, on land none of this civilisation's cities stand on;
        // the one nearest a connected port.
        internal City InvasionTarget(int owner)
        {
            var home=new HashSet<int>(cities.Where(c=>c.owner==owner).Select(c=>Landmass(c.x,c.y)));
            var ports=cities.Where(c=>c.owner==owner&&c.portLinked).ToList();if(ports.Count==0)return null;
            return cities.Where(c=>c.owner!=owner&&AtWar(owner,c.owner)&&nations[owner].known[c.y*Width+c.x]&&!home.Contains(Landmass(c.x,c.y)))
                .OrderBy(c=>ports.Min(p=>Distance(p.x,p.y,c.x,c.y))).ThenBy(c=>c.id).FirstOrDefault();
        }
        // A land tile beside the target city where troops can come ashore.
        Tile LandingTile(int owner,City target,Unit ship)
        {
            Tile best=null;int bestLength=int.MaxValue;
            for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++)
            {
                var t=At(target.x+dx,target.y+dy);if(t==null||IsWater(t.terrain)||IsImpassable(t.terrain)||!NextToSea(t.x,t.y))continue;
                if(t.x==target.x&&t.y==target.y)continue;if(units.Any(a=>a.owner!=owner&&a.x==t.x&&a.y==t.y))continue;
                var water=AdjacentWater(t,ship);if(water==null)continue;int length=(water.x==ship.x&&water.y==ship.y?0:Route(ship,water.x,water.y).Count)*10+Distance(t.x,t.y,target.x,target.y);
                if(length<bestLength){bestLength=length;best=t;}
            }
            return best;
        }
        bool SpareTroop(Unit u){if(Power[u.type]==0||IsNaval(u.type))return false;bool garrison=cities.Any(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y)&&!units.Any(a=>a.id!=u.id&&a.owner==u.owner&&Power[a.type]>0&&!IsNaval(a.type)&&a.x==u.x&&a.y==u.y);return !garrison;}
        Unit InvasionGalley(int owner){return units.Where(g=>g.owner==owner&&g.type==GalleyUnit&&!Cargo(g).Any(a=>a.type==0)).OrderBy(g=>g.id).FirstOrDefault();}
        // Called for each rival land unit with nothing to fight on its own landmass: march to the invasion galley and board.
        bool JoinInvasion(Unit u)
        {
            if(!SpareTroop(u)||AtSea(u)||InvasionTarget(u.owner)==null)return false;
            if(units.Any(a=>AtWar(u.owner,a.owner)&&!IsNaval(a.type)&&VisibleTo(u.owner,a.x,a.y)&&Landmass(a.x,a.y)==Landmass(u.x,u.y)))return false; // fight at home first
            var g=InvasionGalley(u.owner);if(g==null||Cargo(g).Count>=CargoSpace(g.type)||!AtSea(g))return false;
            if(Distance(u.x,u.y,g.x,g.y)==1){Move(u,g.x,g.y);return true;}
            Tile shore=null;int best=int.MaxValue;
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=At(g.x+dx,g.y+dy);if(n==null||IsWater(n.terrain)||IsImpassable(n.terrain)||Landmass(n.x,n.y)!=Landmass(u.x,u.y))continue;int d=Distance(u.x,u.y,n.x,n.y);if(d<best){best=d;shore=n;}}
            if(shore==null)return false;Order(u,shore.x,shore.y);if(Distance(u.x,u.y,g.x,g.y)==1&&u.moves>0)Move(u,g.x,g.y);return true;
        }
        // Galley with troops (or waiting for them).
        void InvasionAI(Unit g,List<Unit> troops)
        {
            var target=InvasionTarget(g.owner);
            if(target==null){var home=cities.Where(c=>c.owner==g.owner&&c.portLinked).OrderBy(c=>Distance(g.x,g.y,c.x,c.y)).FirstOrDefault();if(home!=null&&troops.Count>0){var berth=AdjacentWater(PortTile(home),g);if(berth!=null&&!(berth.x==g.x&&berth.y==g.y))Order(g,berth.x,berth.y);else foreach(var a in troops)GoAshore(a);}return;}
            if(troops.Count==0)
            {
                // Wait at the home port for troops.
                var home=cities.Where(c=>c.owner==g.owner&&c.portLinked).OrderBy(c=>Distance(c.x,c.y,target.x,target.y)).FirstOrDefault();var port=home==null?null:PortTile(home);
                if(port!=null&&Distance(g.x,g.y,port.x,port.y)>1){var berth=AdjacentWater(port,g);if(berth!=null)Order(g,berth.x,berth.y);}
                return;
            }
            bool moreComing=units.Any(a=>a.owner==g.owner&&SpareTroop(a)&&!AtSea(a)&&Distance(a.x,a.y,g.x,g.y)<=5&&Landmass(a.x,a.y)!=Landmass(target.x,target.y));
            if(troops.Count<Math.Min(2,CargoSpace(g.type))&&moreComing&&Distance(g.x,g.y,target.x,target.y)>3)return;
            var landing=LandingTile(g.owner,target,g);if(landing==null)return;
            if(Distance(g.x,g.y,landing.x,landing.y)>1){var water=AdjacentWater(landing,g);if(water!=null)Order(g,water.x,water.y);}
            if(Distance(g.x,g.y,landing.x,landing.y)<=1)
            {
                foreach(var a in troops)
                {
                    if(a.moves<=0)continue;
                    if(Distance(g.x,g.y,target.x,target.y)==1&&Power[a.type]>0&&!IsRanged(a.type)&&Move(a,target.x,target.y))continue;
                    Move(a,landing.x,landing.y);
                }
            }
        }
        // A land unit at sea that is not being ferried steps onto the nearest land it can reach.
        void GoAshore(Unit u){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=At(u.x+dx,u.y+dy);if(n!=null&&!IsWater(n.terrain)&&Move(u,n.x,n.y))return;}}
        // Settler with no reachable site: ask for a ferry, and step aboard a galley alongside.
        bool SettlerAwaitsFerry(Unit u)
        {
            if(AtSea(u))return true; // the galley decides where it lands
            FerryWanted.Add(u.owner);
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
            {
                var n=At(u.x+dx,u.y+dy);if(n==null||!Sailable(n.terrain))continue;
                var ship=units.FirstOrDefault(s=>s.owner==u.owner&&s.type==GalleyUnit&&s.x==n.x&&s.y==n.y);
                if(ship!=null&&Cargo(ship).Count<CargoSpace(ship.type)&&Move(u,n.x,n.y))return true;
            }
            return false;
        }
    }
}
