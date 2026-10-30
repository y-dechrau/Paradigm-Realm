using System;
using System.Collections.Generic;
using System.Linq;

namespace ParadigmRealm
{
    // Boats, ships and ports (owner request, save version 18).
    // - Boat (Sailing): built by a city beside open water or a lake; keeps to water next to land; carries 1 land unit.
    // - Port (Engineering): needs the city's borders to reach the sea. It stands on the city itself when the city is on the
    //   coast, otherwise on the nearest coastal land tile of the city's territory (within 3 tiles).
    // - A port away from the city only works while a road joins it to the city and no enemy unit stands on that road or on
    //   the port. Cut off, it gives nothing; mended, everything returns.
    // - A working port: +2 gold a turn, +1 food on worked sea-resource tiles, and Galleys (carry 3) and War Galleys (fight
    //   ships) can be built; they are launched beside the port.
    // - Land units board a ship with room by moving onto its tile, travel with it, and step off onto land. Units left at sea
    //   with no ship under them (their ship sunk) are lost.
    public partial class World
    {
        public const int PortBuilding=44,BoatUnit=12,GalleyUnit=13,WarGalleyUnit=14;
        public static bool IsNaval(int type){return type>=BoatUnit&&type<=WarGalleyUnit;}
        public static bool Sailable(int t){return t==4||t==6;}
        public static int CargoSpace(int type){return type==BoatUnit?1:type==GalleyUnit?3:0;}
        bool NextToLand(Tile t){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n!=null&&!IsWater(n.terrain))return true;}return false;}
        static bool IsWater(int t){return t==4||t==6||t==12;}
        bool NextToSea(int x,int y){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){if(dx==0&&dy==0)continue;var n=Step(x,y,dx,dy);if(n!=null&&Sailable(n.terrain))return true;}return false;}
        public List<Unit> Cargo(Unit ship){var t=At(ship.x,ship.y);if(t==null||!IsWater(t.terrain))return new List<Unit>();return units.Where(a=>a.owner==ship.owner&&!IsNaval(a.type)&&a.x==ship.x&&a.y==ship.y).ToList();}
        public Unit TransportAt(int owner,int x,int y){return units.FirstOrDefault(s=>s.owner==owner&&CargoSpace(s.type)>0&&s.x==x&&s.y==y&&Cargo(s).Count<CargoSpace(s.type));}
        // Whether unit u may stand on tile t (terrain as the unit's owner knows it); other civilisations' units are checked elsewhere.
        public bool Passable(Unit u,Tile t,int terrain)
        {
            if(IsNaval(u.type))
            {
                if(Sailable(terrain))return u.type!=BoatUnit||NextToLand(t);
                var harbour=cities.Find(c=>c.x==t.x&&c.y==t.y);return harbour!=null&&harbour.owner==u.owner&&NextToSea(t.x,t.y);
            }
            if(!IsImpassable(terrain))return true;
            if(!Sailable(terrain))return false;
            var ship=units.FirstOrDefault(s=>s.owner==u.owner&&CargoSpace(s.type)>0&&s.x==t.x&&s.y==t.y);
            return ship!=null&&(u.x==t.x&&u.y==t.y||Cargo(ship).Count<CargoSpace(ship.type));
        }
        // Land units left at sea without a ship are lost.
        void DrownStranded(){units.RemoveAll(a=>!IsNaval(a.type)&&IsWater(At(a.x,a.y).terrain)&&!units.Any(s=>s.owner==a.owner&&CargoSpace(s.type)>0&&s.x==a.x&&s.y==a.y));}
        // ---------- ports ----------
        public Tile PortSite(City c)
        {
            if(NextToSea(c.x,c.y))return At(c.x,c.y);
            Tile best=null;int bestDistance=99;
            for(int dy=-3;dy<=3;dy++)for(int dx=-3;dx<=3;dx++)
            {
                var t=Step(c.x,c.y,dx,dy);if(t==null||t.owner!=c.owner||IsWater(t.terrain)||Barren(t.terrain)||!NextToSea(t.x,t.y))continue;
                if(cities.Any(o=>o.x==t.x&&o.y==t.y))continue;int d=Distance(c.x,c.y,t.x,t.y);if(d<bestDistance){bestDistance=d;best=t;}
            }
            return best;
        }
        public Tile PortTile(City c)
        {
            if(!HasBuilding(c,PortBuilding))return null;
            if(c.portX<0){var site=PortSite(c);if(site==null)return null;c.portX=site.x;c.portY=site.y;}
            return At(c.portX,c.portY);
        }
        bool EnemyOn(int owner,Tile t){return units.Any(a=>a.x==t.x&&a.y==t.y&&a.owner!=owner&&AtWar(owner,a.owner));}
        // Road link from the city to its port: road tiles only, none held by an enemy unit, within 4 tiles of the city.
        public bool PortRoadLinked(City c)
        {
            var port=PortTile(c);if(port==null)return false;if(port.x==c.x&&port.y==c.y)return true;
            if(EnemyOn(c.owner,port))return false;
            var seen=new HashSet<int>{c.y*Width+c.x};var queue=new Queue<Tile>();queue.Enqueue(At(c.x,c.y));
            while(queue.Count>0)
            {
                var t=queue.Dequeue();
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                {
                    var n=Step(t.x,t.y,dx,dy);if(n==null||!seen.Add(n.y*Width+n.x))continue;
                    if(n.x==port.x&&n.y==port.y)return true;
                    if(!n.road||EnemyOn(c.owner,n)||Distance(c.x,c.y,n.x,n.y)>4)continue;
                    queue.Enqueue(n);
                }
            }
            return false;
        }
        // Refreshed after every move, every turn and on loading, so the link is never stale; the owner hears when it changes.
        public void UpdatePortLinks()
        {
            foreach(var c in cities)
            {
                bool linked=HasBuilding(c,PortBuilding)&&PortRoadLinked(c);
                if(linked!=c.portLinked&&HasBuilding(c,PortBuilding)&&c.owner==0)Note(linked?"The port of "+c.name+" is connected again.":"The port of "+c.name+" is cut off: its road is broken or held by the enemy.");
                c.portLinked=linked;
            }
        }
        public int PortIncome(City c){return c.portLinked?2:0;}
        public string NavalReason(City c,int u)
        {
            if(u==BoatUnit)return NextToSea(c.x,c.y)?"":"Requires a city beside the sea or a lake";
            if(!HasBuilding(c,PortBuilding))return "Requires a Port";
            if(!c.portLinked)return "Port cut off: rebuild the road or drive the enemy off it";
            return LaunchTile(c,u)==null?"No free water beside the port":"";
        }
        // Where a new vessel goes into the water: beside the city for boats, beside the port for ships.
        public Tile LaunchTile(City c,int type)
        {
            var from=type==BoatUnit?At(c.x,c.y):PortTile(c);if(from==null)return null;var probe=new Unit{owner=c.owner,type=type,id=-1,x=from.x,y=from.y};
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
            {
                var n=Step(from.x,from.y,dx,dy);if(n==null||!Sailable(n.terrain)||!Passable(probe,n,n.terrain))continue;
                if(units.Any(a=>a.owner!=c.owner&&a.x==n.x&&a.y==n.y))continue;
                if(units.Count(a=>a.owner==c.owner&&IsNaval(a.type)&&a.x==n.x&&a.y==n.y)>=3)continue;
                return n;
            }
            return null;
        }
        // A ship carries its cargo with it from one stretch of water to the next.
        void CarryCargo(List<Unit> cargo,int x,int y){foreach(var a in cargo){a.x=x;a.y=y;a.fortified=false;}}
        public bool PortValid(){return cities.All(c=>(c.portX==-1&&c.portY==-1)||At(c.portX,c.portY)!=null);}
    }
}
