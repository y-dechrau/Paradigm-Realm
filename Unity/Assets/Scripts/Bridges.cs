namespace ParadigmRealm {
// Bridges after Civilization IV (owner request, 24 Nov 2026; the rule is Construction's bridge building there).
//  - Roads may be laid up to and on a river from The Wheel onwards; nothing stops at the bank any more.
//  - Until a civilisation knows Bridge Building (technology m_arches, Classical Age, after Mathematics) its units get no road speed
//    stepping onto a river: they ford it at the ordinary cost of the land. Roads that follow a river from one river tile to the
//    next keep to the bank and are not slowed.
//  - Learning Bridge Building bridges every road the civilisation uses at once; no worker has to come back.
//  - As in Civilization IV it is the unit's own civilisation that must know how: an enemy without it fords your river.
//  - A bridge takes nothing from the defence a river gives (+1 to whoever stands on it), and roads join cities and resources across
//    a river with or without one.
// On the map a road without bridges stops at each bank and stepping stones cross the water (PixelArt.PaintRoad, ford).
// Before save version 45 Engineering brought bridges, so a civilisation that had it keeps them.
public partial class World {
 public const string BridgeTech="m_arches",FordText="Units ford it slowly until you learn Bridge Building.",BridgeDescription="Every road that crosses a river is bridged: units keep their road speed instead of fording the water. A bridge costs as much to keep as two tiles of road.";
 static int bridgeTechIndex=-1;
 public static int BridgeTechIndex{get{if(bridgeTechIndex<0)bridgeTechIndex=TechnologyCatalog.Index(BridgeTech);return bridgeTechIndex;}}
 public bool HasBridges(int owner){if(owner<0||nations==null||owner>=nations.Length||nations[owner]==null)return false;var known=nations[owner].tech;int i=BridgeTechIndex;return known!=null&&i>=0&&i<known.Length&&known[i];}
 // A road on a river tile is bridged when the people whose land it crosses (you, on unclaimed land) can build bridges.
 public bool Bridged(Tile t){return t!=null&&HasBridges(t.owner>=0?t.owner:0);}
 // Stepping onto a river from dry land along a road, with no bridge to carry it.
 bool FordsRiver(int owner,Tile from,Tile to){return to.terrain==7&&from.terrain!=7&&!HasBridges(owner);}
 void UpgradeBridges45(){
  if(nations==null)return;int eng=TechnologyCatalog.Index("engineering"),b=BridgeTechIndex;
  foreach(var n in nations)if(n!=null&&n.tech!=null&&n.tech.Length>b&&n.tech[eng])n.tech[b]=true;
  if(tech!=null&&tech.Length>b&&tech[eng])tech[b]=true;}
}
}
