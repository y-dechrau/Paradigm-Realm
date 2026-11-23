# Negotiation, Beliefs & Traditions, and the Public Vow (23 Nov 2026)

Handover for the three owner briefs of 23 Nov 2026. Everything here is offline and deterministic (no random rolls: outcomes
depend only on the world state and the seed).

## 1. Diplomacy: negotiated agreements

### What is playable
- **Council of Realms**: four views (Seats, Contact list, Agreements, History). Seat markers use a symbol as well as a
  colour (number = proposals waiting, `?` = a request, hourglass = an agreement ending within 3 turns, `!` = a grievance or a
  broken promise) and explain themselves on hover.
- **Private audience** (Imperial Court) with a two-sided deal table, WE OFFER / WE ASK: gold at signing, gold a turn, a
  resource, passage through the giver's land, use of one known gateway. The whole agreement is written out in plain English
  before you propose it, and the ruler's likely answer is given in words with the one or two strongest reasons.
- **Counteroffers**: a ruler who would take a different price lays a counteroffer on the table with the changed term in
  gold. Accept, Amend (their terms are loaded into the table as your next proposal) or Decline.
- **Passage and gate-transit agreements** that really change movement: the border opens one way, for the stated turns;
  a blocked move says why ("their border is closed", "only the road to the gateway is open", "the agreement expired").
- **Execution** is revalidated at acceptance, applied once, saved, and expires when the stated turn begins; units still
  inside are escorted to the border. War, a lost gateway or an eliminated party end it with no blame.
- **Memory**: kept, cancelled-by-agreement and broken are recorded differently and change later answers. A kept word only
  counts after 10 turns in force (no farming short deals).
- **AI proposals**: rulers ask for passage themselves, with a reason and a due turn, on a 20-turn cooldown per pair.
- **Escape** closes the topmost diplomacy panel first.

### Try a first negotiation
1. Diplomacy, pick a neighbour, **Request Audience**, then **Trade Deal**.
2. Under WE ASK click **Passage through their land**. Read the summary and the answer in words.
3. **Propose**. They refuse and answer with a counteroffer naming a price.
4. **Accept**. Gold moves once; your units can now cross. Council > Agreements shows it with turns left;
   Council > History shows the refusal, the counteroffer and the signing.

### Where to tune
`DiplomacyTuning` in `DiplomacyAgreements.cs` (prices, caution by personality, trust weights, cooldowns, talks per turn).

### Limits
- Three proposals to a ruler each turn (pacts still one).
- Counteroffers change gold or length only; they never add new clause types.
- AI-to-AI passage deals are signed directly, without haggling.

## 2. Beliefs & Traditions

### Audit and migration map
| Old piece | Fate |
|---|---|
| Faith currency (`Nation.faith`), pantheons, founding, missionaries, priests, faith buildings | **Kept, one meaning**: the standing of the organised clergy of the historical religions. Nothing in the new system spends it. |
| `faithPressure` / `CityShares`, `FaithRecord`, the faith event queue, old Reform/Schism | Kept as they were for the historical religions (Organised Faiths button). |
| "How people live" (did not exist) | **New, authoritative**: `Lifeways.cs`. Adherence, conviction, institutions (practices) and state policy are four separate values. |
| Religion rail entry | Now opens **Beliefs & Traditions**; the old screen is one click away. |

The old religion rules were not deleted: their checks assert exact numbers and the save format depends on them. The new
system is the single authority for traditions, practices, commitments and vows, and is switched by `World.lifeways` (on for
every new and loaded game; off in the old checks).

### The First Granary (playable end to end)
Run the game with `-granary`, or play normally until a city's net food is negative and its store would empty in 3 turns.
1. End the turn twice: **The stores are failing**. Send food from another city (it loses exactly what is sent), buy grain,
   or let them endure.
2. Three turns later: **How shall it be remembered?** A religious observance (the Feast of the First Granary), a civic rule
   (the Granary Rule, a public reserve) or a local custom (the Shared Loaf).
3. The practice falls due every 12 turns. In a good year it is paid and kept on its own.
4. In a lean year it is put to you: keep it in full, ask the elders for a smaller observance (not twice running), or refuse.
   Refused twice, it lapses.
5. It appears in the chronicle, on the Beliefs & Traditions screen, in the city's Faith tooltip, and survives save and load.

### The rest
- **Four authored traditions**: Keeper of the Ford (deity), Houses of Remembrance (ancestral), School of the Open Hand
  (ethical philosophy), Society of the Measured Sky (natural philosophy). Each rises from terrain or conduct (river, scars or
  age, relief or trade, a library), or is founded deliberately for 40 gold where it has a root.
- **Spread** only along trade routes, connected roads and open borders, and the chronicle names the road it took.
- **Commitments** with triggers: leaving a city hungry, refusing a practice, breaking an agreement, failing to pay the
  observers at a discovery. The Open Hand makes rulers weigh your offers more kindly while held.
- **Reform** (one, end to end): a practice can be paid in coin or kept less often, once, at a cost in conviction.
- **Schism** (one, end to end): a commitment broken while a city holds firm divides that city; reconcile, let it keep its own
  house, or suppress it.
- **A voice beyond the gate** when the first gateway opens, read differently by each tradition.
- **Map lens**: minimap layer "Traditions".

### Where to tune
The constants at the foot of `Lifeways` in `Lifeways.cs`.

## 3. Engagement

1. **Main problem found**: the opening gives the player work (found, explore, build) but no question, and nothing the
   player says in the first 50 turns can later be held against them. Religion was a currency sink rather than a way of life.
2. **Three directions**: (a) home world: scarcity and relief stories; (b) religion and philosophy: practices and
   commitments; (c) realms: gates as dilemmas. **Chosen**: (b) built on (a), with one hook into (c) (the voice beyond the
   gate), because it gives consequence, counterplay and memory with the least new art.
3. **Signature mechanic: the public vow.** A time-bound promise about a named place ("no one in X will starve"), people
   ("no war on Y") or practice ("the feast of X will be kept"). It ends one of four ways: kept (conviction, trust,
   influence), broken (every ruler remembers), released (25 gold, no blame from rulers), or made impossible through no act
   of yours (no blame). Anti-farming: one at a time, 15 to 40 turns, an 8-turn wait between vows, and the same vow about the
   same place or people is worth half each time. Other rulers swear peace vows on the same terms and are bound by them.
4. **The opening question** is read from the land around the capital on turn 1 (river, thin soil, a nearby gateway, or good
   land) and can be answered as a public vow, a private aim, or set aside. Same seed, same question.
5. **To play it**: new game, any seed, end the first turn; or `-granary` (seed 2311) for the shortage story at once.
6. **Systems changed**: `World.TurnSteps` (one call), city content and yields (one term each), `DeclareWar`/`WarReason`,
   `BreakAgreement`, research completion. Saves: new lists default empty; older saves load and are valid (checked).

### Tests run
`RealmTools.Checks` passes, including `NegotiationChecks` and `LifewaysChecks` (the First Granary through every branch,
save/load, reform, schism, the four vow endings, the opening question on a repeated seed, and thirty real turns).
Screens checked for clipping: hud-deal-passage, hud-deal-counter, hud-council-list/agreements/history, hud-way-opening,
hud-way-shortage, hud-way-remember, hud-way-lean, hud-traditions.

### Not done or not verified
- No long played game (100+ turns) with traditions on has been watched; balance numbers are first guesses.
- No baseline-versus-revised opening comparison was played by a person; only the determinism of the question was checked.
- The city screen shows the way of life in the Faith tooltip only, not in its own panel.
- Rulers other than the player answer choices with the first option they can afford, and only swear peace vows.
- The voice beyond the gate is the player's only; traditions do not yet count toward any victory.

### Three playtest questions
1. When the feast fell due in a lean year, did the choice feel like a real cost, or was one answer obviously right?
2. Did you make a public vow you later regretted, and did you break it, buy release, or keep it at a cost?
3. Did the first question change what you did in the first thirty turns?
