# Paradigm Realm — 16-bit core design (planned)

Status: **PLANNED. Nothing in this document is implemented yet.** See IMPLEMENTATION-STATUS.md for what exists. Date: 28 October 2026.

Goal: a classic 16-bit-style turn-based 4X with deep choices and little menu bloat. All maths uses integers. Where this document disagrees with MASTER-DESIGN.md, this document is the newer intent for the Earth game; reconcile before implementing.

## Decisions made with Matt

- Compress the technology tree to about 36 keystone technologies (6 eras of 6) instead of keeping 124 as separate cards. Existing technologies would need mapping or merging.
- Combat stays deterministic, with an exact preview before attacking. No random swing for now.
- Earth first. Realms, portals and the "Dimensional" era beyond a late Transcendence project are deferred until the Earth game works. Interpreting the answers: both "yes" replies were taken as the first option offered. Confirm if wrong.

## Phase 1 — pillars

### 1. Core loop
At most three decisions a turn: research, city builds, unit orders. Everything else is automated or a single key. Enter ends the turn. No screen more than two menus deep.
- Turn 1: found the capital, queue Warriors then Settlers, explore with the Scout, choose research.
- Turn 50: 3-5 cities, Workers on roads and farms, first contact, a Governor preset on each city.
- Turn 200: 8-12 cities, wonders, alliances or war, governments and tax, pushing towards one victory bar.

### 2. Map and tiles
Square grid, 8-direction movement, 16x16 sprites scaled up. Yields are Food / Shields / Trade.

| Terrain | F/S/T | Notes |
|---|---|---|
| Grassland | 2/0/0 | Farm +1F |
| Plains | 1/1/0 | Farm +1F |
| Forest | 1/2/0 | Slow, defence +1 |
| Hills | 1/0/0 | Mine +2S, defence +2 |
| Desert | 0/1/0 | Farm +1F with a river |
| Mountains | 0/1/0 | Mine +2S, very slow, defence +3 |
| Ocean | 1/0/2 | Harbour +1F later |

Rivers add +1T on land. Roads add +1T on grass, plains and desert. Moving between two road tiles always costs 1 movement point.

### 3. Cities
One screen, three panels. Left: a 5x5 tile ring; click a tile to assign or lock a citizen. Middle: Food/Shields/Trade totals, growth bar, happiness. Right: a build queue of up to three items with cost and turns left. A Governor switch (Balanced / Growth / Build / Science) sits at the bottom; manual locks override it. Surplus citizens become entertainers automatically, with one click to make them scientists or tax collectors.

### 4. Combat
Attack, Defence, Health 10, Moves. Damage dealt = max(1, 3 + own strength - enemy strength). Both sides deal damage at once, except that ranged attackers take no retaliation from melee units. Defence bonuses are added points: hills +2, forest +1, river +1, fortified +1, walls +2. Stack limit 3 per tile; only the best defender fights and only it dies if it loses.

| Unit | A / D / Moves | Cost | Special |
|---|---|---|---|
| Spearman | 1 / 2 / 1 | 20 | +2 Defence vs mounted |
| Horseman | 3 / 1 / 2 | 25 | Ignores forest slowdown |
| Archer | 2 / 1 / 1 | 20 | Range 2, no retaliation from melee |

Spearman beats Horseman, Horseman beats Archer, Archer beats Spearman.

### 5. Technology
36 technologies in 6 eras (Dawn, Classical, Medieval, Industrial, Modern, Dimensional). Each must unlock something visible. Knowing 4 of the 6 technologies in an era advances the era and offers a choice of 1 of 3 permanent edicts. Milestones: Bronze Working, Writing, Currency, Code of Laws, Engineering, Industry, Electricity, Computing.

### 6. Victory
- Conquest: hold every rival capital, or eliminate all rivals.
- Transcendence: a 3-stage project; the bar shows the stage and turns left.
- Concord: friendship or alliance with every surviving rival for 20 consecutive turns; a broken treaty resets the count.

## Phase 2 — balancing formulas (DRAFT, not yet approved)

Principle: flat integer bonuses instead of percentages, except the tax rate. Numbers below are starting points to tune in play.

### Growth
- Food needed to grow from size n to n+1 = 10 + 4n. Each citizen eats 2. The city centre tile is worked free.
- Negative food box: lose 1 citizen (minimum size 1) and reset the box to 0. A Granary keeps half the box (rounded down) after growth.
- Size caps follow the existing rules: 6, or 8 beside fresh water; 12 with an Aqueduct; 20 with a Sewer.

### Production and buying
- Shields per turn = worked tiles plus the centre tile, minimum 1. Surplus carries over.
- Costs: Warrior 10, Worker 20, Spearman 20, Archer 20, Horseman 25, Catapult 40, Settler 30 (and 1 population, city size 2 or more).
- Buildings come in tiers of 40, 60 and 80 shields. Wonders cost 100 + 50 x era.
- Buying costs 2 gold per remaining shield, doubled if no shields are in the item yet.

### Trade, science and gold
- Tax rate is set in steps of 10%, maximum 60% science early and 80% after an edict. Science = trade x rate / 100, rounded down. Gold = the rest, so nothing is lost.
- Buildings give flat bonuses (for example Library +2 science, Market +2 gold).

### Technology cost
Cost = 20 x e^2 + 5 x e x i, where e is the era (1-6) and i is the position in the era (1-6). Ranges: era 1 is 25-50, era 2 is 90-140, era 3 is 195-270, era 4 is 340-440, era 5 is 525-650, era 6 is 750-900. Cost falls by one fifth (rounded down) if two civilisations you have met know the technology.

### Upkeep
Each city supports 3 units free; each extra unit costs 1 gold per turn. Buildings in the 40 tier are free; 60 and 80 tier buildings cost 1 gold per turn. Wonders are free.

### Happiness
- Content workers = min(workers, 4 + happiness buildings + 2 x entertainers). Unhappy workers = workers - content workers.
- A city is in disorder (no production, no tax) if unhappy workers outnumber content workers.
- High tax removes one content slot; low tax adds one.

### Health and experience
Units heal 1 per turn in the open, 2 if fortified, 3 in a city; a Barracks heals fully. Two victories make a unit Veteran (+1 strength); five make it Elite (+2).

### Victory numbers
- Transcendence stages cost 300, 400 and 500 shields in a city of size 8 or more that knows Computing. Each stage then needs 10 turns to activate. Losing that city loses the project.
- Concord needs 20 consecutive turns.

### Pacing targets (standard game about 250 turns)
Era 1 complete around turn 40, era 2 around 80, era 3 around 125, era 4 around 170, era 5 around 210. Earliest Transcendence around turn 230.
