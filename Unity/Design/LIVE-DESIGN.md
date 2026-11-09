# Paradigm Realms Live — design exploration

Status: **proposal, nothing built yet** (9 Nov 2026). Paradigm Realms Live began as a copy of Paradigm Realm 0.27. It stays a turn-based fantasy 4X on the same map, and brings in the systems that make Stellaris feel like Stellaris. The original game is unchanged.

## The idea in one paragraph

In Paradigm Realm you manage cities and tiles. In Live you manage **people**: every city is a home for pops, groups of people who belong to a species or people, hold a job, follow an ethic, and join factions that want things from you. The empire grows harder to hold as it spreads. Wars are fought **for** stated goals and end when one side is exhausted. The sealed worlds become a long chain of discovery that leads to a late-game crisis. A council of all peoples votes on laws that bind everyone.

## Systems, from the base up

### 1. Pops and jobs (the heart of the change)
- **Now:** a city has a size; citizens work tiles or become specialists.
- **Live:** a city holds pops. Buildings and developed tiles create **jobs**, and each pop fills one job:
  - farmers, miners, artisans, scholars, priests, soldiers and merchants
  - rulers, filled by the most educated pops
- Pops that find no job are **unemployed**: unhappy, and they drift to cities with work.
- **Strata:** rulers, specialists and workers have different needs (food, goods, comfort).
- **Species and peoples:** a captured or migrated pop keeps its people, traits and ethic. The 55 sealed peoples and 12 natives become real populations you can absorb, not just rivals.
- **Builds on:** specialists, buildings, the population figures in `Demography.cs`, `CityCare.cs`.

### 2. Ethics and factions
- Every pop leans to one of eight ethics: Militarist or Pacifist, Spiritualist or Materialist, Xenophile or Xenophobe, Egalitarian or Authoritarian.
- Pops sharing an ethic form a **faction** with demands, for example "be at war less", "build temples" or "do not enslave". A satisfied faction gives influence and stability; an angry one gives unrest and, at worst, the rebellions the living world already has.
- Your government's ethics decide which factions are easy to please.
- **Builds on:** governors, loyalty, grudges, rebellions (`LivingWorld.cs`), religion's factions and schisms.

### 3. Empire sprawl and stability
- Every city, pop and colony adds **sprawl**. Above your capacity, research and edicts cost more and stability falls.
- **Stability** per city replaces content and unhappy citizens as the main mood figure. High stability gives output bonuses; low stability brings strikes, then revolt.
- **Builds on:** the cost of a large empire, governors' corruption and the content calculation.

### 4. Influence as a resource
- **Influence** (it exists already for envoys) pays for new cities' claims, edicts, faction promises, diplomatic pacts and war goals. It is the brake on spamming cities and wars.

### 5. War goals, claims and war exhaustion
- **Claims:** before a war you claim enemy cities or lands (influence cost).
- **War goal:** you declare war for a goal (conquer claims, humiliate, liberate, convert faith, make tributary).
- **War exhaustion:** each side's score of losses, occupied land and turns at war. At 100% that side must accept a status quo peace. Battles and occupations move it.
- Peace gives exactly the claims you hold.
- **Builds on:** the new sieges and stacks, rival war logic (`RivalWar.cs`), grievances.

### 6. Traditions and ascension
- A second tree beside research, bought with **unity**, a new yield from monuments, temples and stability. Its themes are Expansion, Prosperity, Discovery, Supremacy, Harmony and Mysticism.
- Completing trees grants **ascension perks**: big, permanent identity choices.
- **Builds on:** edicts, the faith tree (doctrines), golden ages.

### 7. Discovery chains (anomalies and archaeology)
- Ruins, graveyards, sealed gateways and odd tiles become **sites**. A scout or scholar studies them over several turns, chapter by chapter, with choices and risks.
- The final chapters of the sealed-world chains reveal **who sealed the gates and why**, and set up the crisis.
- **Builds on:** ruins (now in five styles), sealed worlds and their histories, faith events.

### 8. The Council of Realms (galactic community)
- Once enough peoples have met, a council forms. Every few turns it votes on resolutions, for example a ban on burning cities, open trade, sanctions on one people, or protection of the sealed worlds.
- Votes are weighted by influence and diplomatic weight. A custodian role and an emperor path come late.

### 9. Late-game crisis
- Around the end of the Industrial age, if the gateways are open, something comes through: the hum beneath the Hive Expanse, the plague of the Marshes, or a fire from the Infernal Depths, chosen per game.
- It spreads, takes cities and cannot be bargained with. Every people must stand together or fall.

### 10. Time
- **Recommended:** keep turns. Stellaris's real-time-with-pause would mean rewriting the turn engine, the AI and the interface; it is effectively a different game.
- A middle way, if you want it later: a **"turn speed" autoplay**. Turns advance on a timer, with pause, so it feels live without changing the engine.

## A suggested order

| Phase | What | Why first |
|---|---|---|
| 1 | Pops and jobs, stability | Everything else hangs off pops |
| 2 | Ethics and factions, influence | Gives pops a voice |
| 3 | Sprawl, traditions and unity | The growth brakes and the second tree |
| 4 | War goals, claims, exhaustion | Uses the new sieges and stacks |
| 5 | Discovery chains | The story of the sealed worlds |
| 6 | Council of Realms | Needs factions and influence |
| 7 | Crisis | The finale |
| optional | Autoplay turn timer | The "live" feel |

## Questions for the owner
1. Should pops replace citizens completely (Stellaris style), or sit beside the current city size?
2. Should captured peoples keep their traits and ethic as pops, with different needs and jobs they are good at?
3. War exhaustion that forces peace: yes or no?
4. Do you want the autoplay turn timer to give the game a "live" feel?
5. Which crisis first: the hum of the Hive Expanse, the plague of the Marshes, or the fire of the Infernal Depths?
