# Timber, stone and contested sites (owner briefs, 26 Nov 2026)

Two briefs: a turn-by-turn timber and stone economy, and resource competition. Built in `MaterialEconomy.cs`,
`MaterialsUI.cs`, `SiteCompetition.cs`, `SiteUI.cs`; checked by `MaterialEconomyChecks` and `SiteChecks`; photographed by
`-hudShot <folder> -woodOnly`. No save version change: every new field loads empty or as zero.

## Before

- Timber and stone were national stockpiles. Every city gave 1 of each a turn; worked woodland gave 1 timber, worked hills
  1 stone, worked Timber and Stone resources 2 (+1 with a road or mine).
- They were spent on buildings only: the whole timber and stone price was taken when a building started, together with its
  gold. Without enough, the building waited at the front of its line. Cancelling refunded it.
- A building needed a sixth of its shield cost in materials, usually 3 to 10. Stocks therefore piled up, and the top bar showed
  only the stock and the gain.

## Now

### Drawn as the work goes
- Every project has a timber and stone total. Each turn it is charged `Owed(new progress) − already installed`, where
  `Owed(p) = total × p ÷ work`, rounded down. At completion `Owed` is exactly the total, so nothing is lost to rounding.
- A project needing both materials advances only as far as both allow.
- The gold for a building is still paid when it starts.
- The projects that draw materials are:
  - Buildings: a third of their shield cost, up from a sixth. The split between timber and stone is unchanged, except the
    Harbour, which is now a third timber and two thirds stone.
  - Boat 6, Galley 16, War Galley 20, Catapult 12 and Caravan 4 timber, when trained in a city.
  - A bridge (a road on a river tile): 20 timber.
  - A fort: 12 stone and 4 timber.
  - War galleys and catapults raised under Military > Forces do not draw timber yet.
- **Turn order:** the harvest is added first and can be used the same turn. Then every project is supplied, before any city
  works.
- **Supply order:** Urgent projects first, then the earliest started, then buildings, units and worker jobs, then by id. The
  order never depends on which city updates first.
- **Short of materials:** a project keeps its work and its installed materials, and waits. Shields its building line cannot
  use go to the unit line. Nothing completes until all its materials are installed. Finished work has no upkeep.
- **Cancelling:** cancelling or replacing a building or unit returns everything installed in it, once. Materials in a
  worker's unfinished bridge or fort stay on that tile until work resumes there. If a different work is begun there, they go
  back to the stockpile.
- **One calculation:** `Allocate` runs the turn, and `ForecastJobs` repeats it turn after turn for every preview, so a preview
  and the turn that follows agree.

### Where the player sees it
- **Top bar:** each figure shows the stock and next turn's net change (harvest less what work will draw). Clicking either
  figure opens the ledger:
  - Last turn: opening stock, produced (named by city and source, and by camp), used (named by project), what waited, and
    closing stock.
  - Then every project that draws materials, in supply order, with next turn's draw, when it finishes and an Urgent switch.
- **City production line:** what is installed, or "Waiting for N stone". Hovering gives the full line: installed, next draw,
  finish, and which project is supplied first.
- **Build list:** hovering an item shows what starting it would do, for example "Start Galley · 16 timber over 4 turns · work
  already under way still finishes on schedule", or which project it delays and by how much.
- **End Turn:** a project newly predicted to wait next turn is named once in the notes.

### Rivals
- Rivals do not start a building they cannot supply within 15 turns of income, after what they have already promised
  (`MaterialsInReach`).

### Older saves
- A building already paid for under the old rule counts as fully installed and draws nothing more.
- Saves keep installed materials, urgency, worker sites, camps and the last ledger.

## Contested sites

Every judgement is made from one civilisation's own knowledge, by four questions:

| Question | How it is answered |
|---|---|
| Do we know what is here? | The civilisation has seen the tile (`Nation.known`; for you, `seen`) |
| Do we know how to use it? | Timber needs Stone Tools, stone (hills or a Stone resource) needs Mining. Something we are building must need it now |
| Can we get it and bring it home? | A worker can set up a camp on unclaimed land, or on land of a people that lets us pass, within 6 steps of our own land. A river without Bridge Building counts as two steps, and another people's land without passage blocks the way |
| Is it worth it now? | The same forecast as the city lines, with the camp's output added once a worker could have it running, compared with no camp |

- **Camps:** a camp is worker job 6, three turns of work. It sends 2 timber or stone home each turn, +1 on a known resource,
  +1 with a road. It gives a right to work the site, not the land: borders do not change. On land closed to you, or with the
  way home cut, it delivers nothing until that changes.
- **What others notice:** a people that wanted the site, and can see the camp, thinks less of you (1 to 4 opinion).
- **The site panel:** left-click a woodland, hills or a timber or stone resource you have seen beyond your borders. The panel
  answers the questions in plain sentences. It names known claims: a camp, a border, a city within 4 tiles, workers close by.
  It never names what another people is building.
- **The choices:** each choice shows what it delivers and what it commits:
  - Set up a camp: which project moves from which turn to which, the first delivery, and the worker it ties up. A Send a
    Worker button.
  - Found a settlement nearby, if you have a Settler.
  - Buy a shipment from a people that can spare it, judged by the seller's own forecast. They need a city within 12 tiles of
    yours on the same land, or a commercial pact. Price 2 gold each, double when they are near their own limit.
  - If the way runs through another people's land, open borders with them.
  - Carry on with what you have.
- **Rivals:** a rival worker, short of materials by its own forecast, builds a camp where it saves the most, now and then.
  Rivals may have one camp, plus one for every three cities.

### Not built
- Gateway trade, such as copper for timber beyond a gate.
- Timber grades and finished components, such as ship timbers.
- Masonry bridges with their own advantage.
- Flood foundations, terraces and lined channels.
- Materials for units raised under Forces.
- Haggling over shipments in the diplomacy screen: the price is fixed.
- Disputes that escalate past opinion.

## The woodland scenario (`-woodland`)

Seed 2612, you and one neighbour, Dawn Age. Both know Stone Tools and Pottery; the neighbour also knows Bronze Working.

- **You:** your capital's Granary has its work done but no timber in store. Your Worker is farming. Your Scout stands beside an
  unclaimed woodland 4 to 5 tiles away.
- **The neighbour:** it is building a Workshop, also short of timber. Its Scout is beside the same woodland and its Worker is
  at home.

1. Click the woodland. The panel says, for example: "Would move Granary in Umberlee from turn 9 to turn 7. First timber in 5
   turns (2 a turn). Commits a busy Worker 1 turn away for 3 turns." The neighbour cannot sell you timber, because it needs
   its own.
2. Choose Send a Worker, or carry on with what you have, and play on. The ledger, opened by clicking the timber figure,
   names the camp as a producer once it is built, and the Granary finishes when the panel said.
3. Played without the camp, the neighbour's worker may set one up first, if its own forecast says the camp pays.
