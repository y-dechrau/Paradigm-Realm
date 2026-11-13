# Diplomacy and strategy, after 4X player feedback (13 Nov 2026, save version 43)

Owner brief: make diplomacy change outcomes. Relationships should have history, alliances should give real help, peace should take more than one shape, gates should create opportunities and disputes, and the late game should stay enjoyable.

This document covers four things:

- what was found in the project;
- what was built in this pass;
- how to use it and how the rivals use it;
- what was tested, and what is left, with the next step.

It extends the 10 Nov rebuild (`DIPLOMACY-STELLARIS-2026-11-10.md`), which stays the base: opinion as named marks, trust with ceilings, attitudes, standing pacts, envoys, rivals, claims, just causes and war exhaustion.

## 1. Inspection: what already existed

| Brief item | State before this pass | Where |
|---|---|---|
| Opinion with named reasons, fading marks | Existed | `Diplomacy.cs` `OpinionSum`, `OpinionMods` |
| Trust separate from opinion | Existed (ceiling set by the strongest pact) | `Trust`, `TrustCap` |
| Threat (fear) | Existed (from conquest and war without cause) | `Threat`, `AddThreat` |
| Interests | Partly: leader agendas, need-weighted deals, ambition | `DiplomacyDepth.cs`, `RivalWar.Ambition` |
| Grievances | Partly: marks, but no expiry shown | `OpinionMods` |
| Standing pacts | Five, with notice and a minimum term | `Pacts` |
| Trade deals | Gold, gold by the turn, resources; payment default handled | `DiplomacyDeals.cs` |
| Peace | White peace only | `MakePeace` |
| Requests, guarantees, ally help | Missing; defensive partners joined wars automatically | none |
| Gate agreements | Missing; passage only through open borders | none |
| AI explanation | Partly: acceptance reasons shown on hover | `AcceptanceLines` |
| Behaviour profiles | Missing; difficulty only | `Difficulty.cs` |
| Resources affecting strategy | Strategic units, luxuries, and per-city effects added earlier the same day | `CityResources.cs` |
| Victory routes | Conquest, Transcendence, Concord, religion | `Victory.cs` |

The simulation is turn based, so every duration below is in turns. Engine: Unity 6000.3.25f1 (`ProjectSettings/ProjectVersion.txt`), C#, IMGUI screens. No new packages were added.

## 2. Built in this pass

### Relationship memory (`DiplomacyMemory.cs`)

**New marks.** Four marks on opinion, each with its own fading pace (`OpinionMods.FadeEvery`, turns per point):

| Mark | Typical size | Fades |
|---|---|---|
| Helped us | +10 (aid, joining a war, guarding a gateway) | every 6 turns |
| Kept their word | +10, or +15 for help | every 6 turns |
| Abandoned us | -10 for refusing an ally attacked, -15 for breaking a promise to one | every 4 turns |
| Attacked those we protect | -20 (a defensive partner, or a peace this leader brokered) | every 3 turns |

**Gifts.** Gifts add at most +20 in all, and each is worth less than the last. Gold cannot wash out a betrayal: in the checks, six gifts do not undo an ended defensive pact.

**Inertia.**
- A long friendship adds +1 for every 8 turns of the oldest agreement, up to +15.
- A partner with "Kept their word" or "Helped us" is trusted 10 points beyond its agreements' ceiling.
- Betrayal is lasting but recoverable: an ended defensive pact (-25) is forgotten after about 100 turns.

**What the screens show.**
- The opinion panel shows "fades in ~N turns" beside each remembered mark.
- Diplomacy > More lists what the leader remembers, with the turns until each mark fades.
- It also lists ways to improve relations, and "how they see you" (their designs on your land, posture, what they would sign now, what they hold you to, and peace odds).

### Peace with terms (`DiplomacyPeace.cs`)

**Terms.** A peace offer can carry:

- gold now;
- reparations (gold by the turn, as a contract);
- resources;
- a city handed over each way (never a capital);
- a withdrawal pledge: the recipient keeps soldiers 4 tiles from your cities for the truce, as a Withdraw commitment;
- a truce of 10, 20 or 30 turns.

**Leverage.** `Leverage(a,b)` is the enemy's war exhaustion minus yours, ±15 for visible strength.
- An undefeated enemy (their exhaustion under 50 and your leverage under 30) will not cede a city.
- Demands worth more than leverage x 4 + their exhaustion x 2 are refused as "beyond what you have won".

**Interruption and breach.**
- Reparations that stop because the payer is bankrupt are a broken word (the existing default rule).
- A resource deal that ends because its road was cut ends with no blame.

**Rivals offer terms too.**
- With the upper hand they ask reparations and a withdrawal.
- Losing badly, they offer reparations and a long truce.
- Otherwise they offer a plain peace (`AIPeaceOffer`).

**Annexation still costs.** Taking cities by force still frightens everyone (`DiplomacyOnCapture`), whoever started the war.

### Requests and guarantees (`DiplomacyRequests.cs`)

Five kinds. Each has a deadline or term, a payment made only on completion (the incentive), an optional threat (the consequence), and an "end our rivalry" sweetener:

| Request | Done when | Broken when |
|---|---|---|
| Stop a war (with a named third party) | They make peace; the peace becomes a truce for the request's term | Still at war at the deadline |
| Withdraw troops | No soldier within 3 tiles of your cities for the whole term | A soldier remains after a 2-turn grace |
| Join a war (against your enemy) | They declare war (as defence of an ally) | They have not by the deadline |
| Defend a gateway (yours) | A soldier within 2 tiles on at least half the turns | Left unguarded |
| Send aid | The gold is paid at once | — |

**How the recipient decides.** It weighs named reasons (`RequestLines`) and agrees at 0 or more. The reasons are:

- the cost of the task and whether it is feasible: distance (24 tiles), relative strength, a soldier to spare, gold;
- opinion and trust;
- any pact, and whether you were the one attacked;
- the incentive;
- the threat, credible only if you are at least Equivalent in strength; threats repeated within 10 turns count for nothing;
- its posture.

**When an ally can't do it.** An ally that cannot fight where asked sends up to 60 gold instead and says why.

**Outcomes.**
- *Fulfilled:* the incentive is paid, the asker records "Kept their word" (and "Helped us" for help), and trust rises by 8.
- *Broken:* -15 opinion and trust; "Abandoned us" between allies; then the consequence.
- *Refused:* the consequence only if credible. "Remember" is a -5 grievance; "Rivalry" names you a rival, which is not war.
- *Cancelled with no blame:* a party falls, the gateway changes hands, or the war ends anyway.

**Anti-exploit rules.**
- One open request of a kind per pair.
- A 10-turn cooldown after a refusal.
- At most 3 commitments per pair.
- Payment only on completion, so payments can't be farmed.
- No join-war against a pact partner.
- No join-war if they are already at war with that enemy.

**How rivals use requests** (`RequestsAI`, every fifth turn, the same rules as the player):

1. At war and not winning, a rival asks friends, or anyone with its own quarrel with the enemy, to join, or friends for aid.
2. With an enemy within 4 tiles of its gateway, it asks an ally to stand guard.
3. When someone attacks its friend, it tells the attacker to stop, threatening rivalry if strong enough.
4. When soldiers crowd its cities in peacetime, it asks them to withdraw.

**How rivals carry out what they agree to.**
- Peace and war are applied at once.
- Withdrawing and guarding gateways are unit duties (`DutyErrand`, run before other unit errands).

**Breaking a defensive pact** abandons any defence commitments between the two.

### Gate access (`Pacts.GateAccess`, the sixth agreement)

**Effect.** Units may cross land within 2 tiles of the partner's gateways and pass through them. The rest of the partner's land stays closed. This is implemented in `CanEnter`, so it affects real movement and pathfinding.

**Terms.** At least one side must hold a gateway. Trust ceiling 30, reluctance -5.

**How leaders weigh it** (`GateInterest`):

| Reason | Value |
|---|---|
| Passage to a gateway the leader lacks | +20 |
| Passage to more gateways | +5 |
| Wary of your troops at their gateways (fear 30 or more) | -20 |
| You already hold colonies beyond the gates | -5 |

### Rival behaviour profile (`AiProfiles`, New Game screen)

**World simulation** (the default, and all older saves): rivals pursue survival, prosperity, agendas and grudges.

**Competitive:** rivals also add +25 or more to their designs on any civilisation halfway to a win (`VictoryThreat`: Transcendence stage, a faith's progress, or rival capitals held), the player included.

The profile changes motives only. Difficulty bonuses stay on their own setting and description.

### Interface

**Diplomacy screen**
- "Requests and guarantees" opens a panel: kind, concerning (civilisation or gateway), term, payment on completion, threat, and the end-rivalry option.
- The panel shows the answer with every reason on hover, a "What would it take?" button (the least incentive that turns the answer), and the requests between you.
- "Peace with terms" now has city drop-downs, a withdrawal toggle, the truce length and your leverage.
- "What would it take?" on the deal panel adds the least gold now that turns a refusal. Only gold is added, so it cannot be gamed one unit at a time.

**More page**
- The inbox lists requests as well as proposals, with Accept/Refuse.
- "Your commitments and deadlines" summarises requests, trade deals and truces across all partners, soonest first.
- Memory, ways to improve, and how they see you, as above.

**Alerts**
- A rival call when a request arrives or a promise to you is broken.
- A note when a commitment of yours has 2 turns left.

## 3. Save compatibility

`World.SaveVersion` is 43. `UpgradeDiplomacy43` grows each pair's agreement and mark arrays, creates the request list, and clears `lastDeclarer` (unknown in old wars). All earlier versions upgrade through the existing chain.

New fields:

| Field | Purpose |
|---|---|
| `World.requests` | Requests and commitments |
| `World.aiProfile` | Rival behaviour profile |
| `DiplomaticPair.lastDeclarer` | Who declared the current or last war |
| `DiplomaticOffer.giveCity`, `askCity`, `truce`, `withdraw` | Peace terms |

`TreatyKind.GateAccess` is numbered after `Retired`, so stored numbers of older kinds keep their meaning.

## 4. Tests and scenarios (the .NET harness; Unity runs the same checks through `RealmTools.Checks`)

`DiplomacyStrategyChecks.Run` (called from `DiplomacyChecks.Run`) shows the following. All of them pass.

| Brief scenario | Shown by | Result |
|---|---|---|
| 1. Defensive war ends with reparations, not annexation | Rival attacks; with its exhaustion at 70, it accepts 3 gold a turn for 10 turns, a withdrawal pledge and a 20-turn truce; no city changes hands; reparations are paid. A capital can never be demanded; 400 gold plus 20 a turn is refused as beyond what was won | Pass |
| 2. A long-standing ally gives feasible help | A 50-turn defensive partner sends aid when asked. Asked to join a war, it either joins or, if it cannot, sends gold instead with its reasons | Pass |
| — Helping an ally at a gateway raises trust | The player accepts an ally's request to guard its gateway, guards it 6 turns, is paid, gains "Kept their word" and the higher trust ceiling. Leaving it unguarded the next time breaks the word and records "Abandoned us" | Pass |
| 3. A request protects a weaker nation | The player asks an attacker to stop, offering 50 gold and threatening rivalry. The attacker weighs it (+24: tired of the war, the incentive, fear of the player) and makes peace; the peace becomes a 15-turn truce. The same request cannot be sent again at once | Pass |
| 4. Betrayal persists but can be overcome | Ending a 40-turn defensive pact: -25 "Broke an agreement", remembered for 100 turns; six gifts cannot make up for it; after 101 turns it is gone | Pass |
| 5. A scarce resource by more than one route | Iron bought for 6 gold a turn (3 was refused as too little), or taken with the city that holds it at the peace table, then joined by road | Pass |
| 6. A compact empire stays competitive | Not shown. The 13 Nov city resource rules help tall cities, but nothing measures it | Not done |
| 7. Gate access changes movement | Without the pact, the gateway and land near it are closed; with it, the gateway is reachable but land 4 tiles away stays closed and open borders stay off | Pass |
| 8. Realm activity depends on home investment | Already so (colonies are supplied and settled from home cities; `DimensionChecks`); not changed here | Existing |
| 9. Late-game administration takes fewer actions | Only the commitments summary and deadline alerts; no production or unit-order changes | Not done |
| 10. A satisfying ending without total conquest | Not changed (Transcendence, Concord and religion already exist) | Not done |

Other checks:
- The competitive profile turns rivals on a player two Transcendence stages in; world simulation does not.
- An attacked rival calls on its ally.
- A version 42 save upgrades to 43.
- A save written and read back during a war, with reparations and a withdrawal in force, keeps every request and contract, and plays 5 more turns valid. This uses System.Text.Json in the harness; Unity's JsonUtility round trip is in `RealmTools.Checks`.

**Rival games** (`dotnet EngineChecks.dll diplomacy 300`): three Small hex games, 6 civilisations, 300 turns, an idle player who accepts what friendly leaders propose and ask (except joining wars).

| Seed | Wars | Peaces | Peaces with terms | Requests made (granted or kept) | Rival to rival | End |
|---|---|---|---|---|---|---|
| 701 | 9 | 6 | 0 | 17: aid 8 (7 sent), withdraw 5, join war 3, stop war 1 | 7 | Player conquered, turn 191 |
| 702 | 6 | 5 | 2 | 3 withdraw (1 in force, 1 broken) | 3 | No result at 300 |
| 703 | 7 | 6 | 1 | stop war 1 (kept), withdraw 1 | 2 | A rival's faith won, turn 192 |

**Interpretation.**
- Rivals use the new tools, but how much depends on the game: alliances are few (at most 4 pacts at once), and most requests come from wars.
- No gateway-defence request appeared, because these games do not turn on dimension rules.
- Three games are not a balance study.

**Turn time** at turn 30 was 31, 36 and 79 ms (Small, Medium, Large), against 31, 42 and 75 ms before this pass.

**Not run here.**
- Unity compile, Play Mode and screenshots: Unity is not installed in this environment.
- In the stand-in compile of the interface, the new screen file's only errors are two `GUI.Toggle` calls. `GUI.Toggle` is real Unity IMGUI; the stand-in library lacks it, as it lacks APIs used by eight other files.

## 5. Known limitations

- Requests are judged once, when made, against the state at that moment. A rival does not re-plan if its situation changes before the deadline. It simply keeps or breaks its word.
- Withdrawal zones are around cities, not drawn borders or demilitarised tiles. Tile-level zones need a reliable territory model for contested land.
- Allied help is gold, war and gateway guards. Troop gifting or leasing is not built: ownership, upkeep and return would need new unit rules.
- Information is not filtered by espionage level. The player sees every reason a leader weighs (the brief's development view and the player view are the same page).
- Gate access does not distinguish civilian from military passage, and there is no "keep this gate closed" treaty.
- AI ally requests to the player for gateway guards only happen in games with dimension rules (new games from the menu).

## 6. Deferred work, dependencies and the next step

| Stage | What is left | Depends on |
|---|---|---|
| 5. Geography and resources | Blockades cutting resource deals, gate-borne trade routes, AI response to shortages through requests ("send Iron") | Trade routes as map objects; a resource-request kind |
| 6. Civilisations and tall play | Trait effects on diplomacy (e.g. mercantile peoples value commerce), measured compact-empire tests | A small set of trait hooks in `PactScore` and `RequestScore` |
| 7. Late-game administration | Repeatable production queues, city specialisation presets, grouped orders, multi-unit upgrades, notification filters | City screen and unit picker changes |
| 7. Pacing and endings | A diplomatic or coalition victory tied to standing pacts and kept words (Concord is the nearest), gate stabilisation as an objective | Victory screen; a measure of influence |
| 8. Balance | Numbers above are first guesses: request weights, leverage limits, gift cap, fade rates | Hand-played games |

**Done (13 Nov, later):** "Share a resource" is a sixth request kind (`RequestKinds.Resource`, `DiplomaticRequest.resource`; Wheat, Iron, Gold, Horses or Spices). On acceptance it runs as a trade-deal contract for the term, so a lost mine or road ends it with no blame. Rivals that know Iron Working or Horseback Riding but lack the resource ask a neighbour who has it, paying on delivery. A refusal in the last 20 turns raises the asker's `ResourceNeed` for it to 160, so it pays more in later deals. Checks in `DiplomacyStrategyChecks`.

**Earlier next step (now built):** add a "Send a resource" request kind (a timed resource delivery that uses the trade-deal contract). Have rivals that lack Iron or Horses for their next unit ask for it, and let a refusal feed `ResourceNeed`. That joins requests to the 13 Nov resource rules and starts stage 5.
