# Bridges, bargains with barbarians, reputation and the Chronicle (24 Nov 2026, save version 45)

Owner requests of 24 Nov 2026. Every number below is a first guess and has not been tuned by play.

## Bridge Building (`Bridges.cs`)

- A technology in the Classical Age, after Mathematics (`m_arches`, the retired Arches; icon `UI/Tech/tech-m_arches`, SpriteCook). 101 technologies in all. It keeps Arches' +4% production.
- As in Civilization IV, where Construction brings bridges:
  - Roads may be laid up to and on a river from The Wheel onwards. The old rule (no road on a river before Engineering) is gone.
  - Until a civilisation knows Bridge Building, its units get no road speed stepping onto a river from dry land (`FordsRiver`, in `StepCost`). A road that follows a river from one river tile to the next keeps to the bank and is not slowed.
  - Learning it bridges every road at once; no worker has to come back.
  - It is the moving unit's own civilisation that must know how.
  - A bridge takes nothing from a river's +1 defence, and roads join cities across a river with or without one.
- Upkeep: a bridge counts as two road tiles, a ford as one.
- Drawing: without bridges the road stops at each bank and stepping stones cross the water (`PaintRoad`, `ford`).
- Saves before version 45: a civilisation that knows Engineering gets Bridge Building (`UpgradeBridges45`).

## Bargains with barbarians (`BarbarianDeals.cs`, `BarbarianUI.cs`)

- Left-click a camp you have found, or one of its raiders in sight, to open the camp panel. A bargain binds that camp's raiders.
- **Pay them off:** 30 gold + 20 per age, 15 turns. They leave the payer's units, cities and land alone.
- **Hire them:** 80 gold + 40 per age + 2 per tile to the target's nearest city, 15 turns. They march on a civilisation you have met (within 30 tiles, over land), with two more raiders and a raid every 4 turns. They leave the employer alone and the employer takes half of what they plunder.
- Like a privateer they carry no flag: nobody learns who sent them, so it costs no peace and no reputation.
- One employer at a time. Anyone else may buy a hired camp off at double the price; if the victim does, the contract ends.
- Each payment makes that camp 25% dearer (at most 12 steps). Attacking the camp or its raiders ends your own bargain.
- Rival home civilisations do both (`BargainAI`): they pay off raiders on their own land, and hire a camp against someone they are at war with or have named a rival.

## Reputation (`OpinionMods.Declined`, "Turned us down")

- Declining an offer: -3 opinion. Refusing a request: -5. Declining a counteroffer: -1; choosing Amend costs nothing.
- Letting an offer or request lapse counts as turning it down. An offer that could no longer be carried out costs nothing.
- The mark fades 1 point every 4 turns and never passes -20. The buttons show the cost.

## The Chronicle (`ChronicleRules.cs`)

- A line is written only if the player can see it happen or it happens to a civilisation the player has met (`NoteEvent`). Tightened: gateways opening and closing, colonies falling, city walls breached, the Chronicle of beliefs, the list of religious history.
- `World.Note` never names a civilisation not yet met: it reads "an unknown people" (`ConcealUnmet`). Lines already in an older save are left as they are.

## Look

- A farm on a river tile is now the same painted field as every other farm, cut back from the water (`PixelArt.RiverField`). It used to be drawn in code (`FarmQuilt`), which is why it looked like another art style.
- The Scout takes guard with a SpriteCook clip made from its own picture (`Visuals/Combat/unit-03-guard-00..07`).
- The nine medallions along the bottom are gone; the tile card sits in their place (y 740).
