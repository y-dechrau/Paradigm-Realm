# Diplomacy, rebuilt after Stellaris (10 Nov 2026, save version 40)

Owner request: "re-work the diplomacy from the ground up. base it on a stellaris style system. tailor make it so that it simple but extremely effective."

This replaces the earlier treaty system (timed friendship, alliance, research pact, border promise, joint war, embassy, denounce, embargo, influence-bought envoys, incitement and coalitions). Code: `Diplomacy.cs` (rules), `DiplomacyDeals.cs` (proposals, reasons, turn, rivals' diplomacy, save upgrade), `DiplomacyDepth.cs` (leader character), `DiplomacyScreen.cs` and `DiplomacyUI.cs` (screen).

## The whole system in six ideas

Each idea is one number or one word that the screen shows.

| Idea | What it is | Where it comes from |
|---|---|---|
| Opinion | -100 to 100, a list of named reasons that add up | Deeds leave marks that fade; standing facts (agreements, rivalry, war, fear, agenda, faith) count while they last |
| Trust | 0 to 100; every 2 trust is 1 opinion | Grows 1 every second turn while agreements stand, up to the ceiling of the strongest one |
| Attitude | One word a leader holds toward another | Worked out from opinion, relative strength and fear |
| Agreements | Five standing pacts with no end date | Proposed; the answer and every reason for it are shown first |
| Rivals and claims | Public warnings that become a just cause for war | Naming a rival is free; a claim costs influence |
| War exhaustion and threat | How tired each side of a war is; how much the world fears a conqueror | Time, losses and conquest |

## Opinion

Marks left by deeds (`OpinionMods`), each fading toward nothing:

| Mark | Typical size | Fades |
|---|---|---|
| Past dealings | +3 for each agreement signed | 1 every third turn |
| Gifts | +10 for 25 gold | 1 every third turn |
| Grievances | -5 for others when a war has no cause, and smaller wrongs | 1 every second turn |
| Declared war on us | -20 with a just cause, -40 without | 1 every second turn |
| Took our cities | -10 for a claimed city, -20 for an unclaimed one | 1 every fourth turn |
| Settled against our borders | -3, or -8 between pact partners | 1 every second turn |
| Their envoy at our court | +1 a turn up to +30 | 1 a turn once the envoy leaves |
| Broke an agreement | -8, -15 (non-aggression) or -25 (defensive pact) | 1 every fourth turn |

A forgiving leader (Forgiveness 7 or more) lets wrongs fade twice as fast.

Standing reasons: trust (half its value); agreements (+5 borders, +10 non-aggression, +5 commerce, +5 research, +20 defence); at war -50; named a rival -30 each way; a claim on our city -10 each (at most -30); fear of their conquests (half of threat); fighting the same enemy +10; rival of our rival +15; fearing the same power +15; plus the earlier reasons that stay (old grudges, same government, difficulty, the leader's agenda, the ruler's traits, faith).

## Trust

| Strongest agreement | Trust ceiling |
|---|---|
| None, but your envoy is at their court | 20 |
| Open borders | 20 |
| Commercial pact or research agreement | 40 |
| Non-aggression pact | 50 |
| Defensive pact | 100 |

Above its ceiling trust falls by 1 a turn; at war it falls by 2 a turn. Friendship therefore has to be built in steps: borders first, then the pacts the growing trust makes possible.

## Attitudes

| Attitude | When | What it means for agreements |
|---|---|---|
| At war | At war | Only peace |
| Rival | They named you a rival | Nothing (-100) |
| Loyal | Defensive pact in force | +10 |
| Threatened | Threat 50 or more, or you are Overwhelming and opinion is under 10 | Non-aggression +25; defensive pact refused |
| Hostile | Opinion -50 or lower | Nothing (-100) |
| Domineering | You are Pathetic beside them, they are bold, opinion under 30 | -30 |
| Unfriendly | Opinion -20 or lower | -20 |
| Friendly | Opinion 50 or more | +10 |
| Cordial | Opinion 20 or more | 0 |
| Wary | They covet your land a little, or you are much stronger and not liked | -10 |
| Neutral | Otherwise | 0 |

Relative strength is what a leader can see: the soldiers it sees plus 4 for each city, beside its own (Pathetic under 50%, Inferior under 80%, Equivalent to 125%, Superior to 200%, Overwhelming above).

## Agreements

| Agreement | Effect | Reluctance | Needs |
|---|---|---|---|
| Open borders | Units may cross each other's land | +10 | |
| Non-aggression pact | No war between the two | -15 | |
| Commercial pact | Both gain 2 gold a turn, +1 for every two cities of the smaller partner, at most 8 | -20 | |
| Research agreement | Both gain +2 science, +1 for a partner who knows more | -25 | Writing |
| Defensive pact | Each joins a war declared on the other; includes open borders | -50 | Code of Laws |

A proposal is accepted when its reasons add up to 0 or more: reluctance + opinion + half of trust + attitude + (for the two pacts that keep the peace) +20 when both face the same danger and -60 when the leader has designs on your land + (defensive pact) your strength, -20 to +15 + the leader's posture + gold offered with it. Hovering a row on the screen lists them.

Agreements have no end date. One can be ended after 10 turns in force. Ending it is remembered as a broken word and costs trust; ending a non-aggression or defensive pact also means neither side can declare war for 10 turns.

Trade deals (gold now, gold by the turn, resources, for 10 to 30 turns) work as before and are judged by what the terms are worth to that leader.

## Envoys

1 envoy in the first age, 2 in the second and third, 3 from the fourth. An envoy at a court adds 1 to their opinion each turn up to +30, makes their fear of you fade twice as fast, and lets trust reach 20 without any agreement. Envoys cost nothing.

## Rivals

Naming a rival needs no agreements between the two. All trade between them stops, the namer gains 1 influence a turn (at most 2 rivals), both opinions fall by 30, and after 3 turns the namer has a just cause for war. A rivalry stands for at least 10 turns. Leaders who share a rival like each other more.

## Claims

A claim on a foreign city you have found costs influence: 20 + 2 for each point of city size (at most 60), +20 for a capital. Influence is 2 a turn, +1 for each rival named, and is spent on nothing else. At most 3 claims against one civilisation, and none against a pact partner. The owner is told. After 3 turns the claim is a just cause for war.

## War

War can be declared without a cause. The difference is what the world thinks:

| | Just cause | No cause |
|---|---|---|
| Victim's opinion | -20 | -40 |
| Victim's fear | +4 | +12 |
| Everyone else who knows you | nothing | fear +6, opinion -5 |

Just causes: a claim or a rivalry at least 3 turns old, a holy war (from the faith system), or fear of the target at 50 or more ("containing a threat"). Defensive partners of the victim join.

Taking a city adds fear (scaled by how much each leader hates warmongers): with a claim +10 for the victim and +3 for others (+4 if they have a city within 10 tiles); without one +24 and +12 (+10).

War exhaustion is kept for each side of each war: +1 a turn, +2 for each unit lost, +10 for each city lost, -5 for taking a city. Every 25 points of a civilisation's worst war costs each of its cities a content citizen (at most 4). Peace is judged by it: will to fight on -40, their exhaustion in full, minus half of yours, -30 if they believe they are winning, +30 if they fear they are losing, and at 100 they always accept. Peace leaves each side with what it holds and starts a 10-turn truce.

## How rivals use it

Every fifth turn each rival looks at each civilisation it knows:

- At war: after 6 turns it asks for peace once its exhaustion is 60, or 30 if it is also losing.
- If it covets the other's land (the existing ambition reckoning) or hates it (opinion -50): it ends its agreements when allowed, names the rival, claims the nearest city it knows when it has the influence, and declares war once it has a just cause and the notice or truce has run out. The player hears each step as a message.
- Otherwise: it ends a rivalry that no longer fits, then proposes the lowest agreement it does not yet hold and itself wants, or a resource trade.
- Envoys go first to a power that fears it, then to a leader nearly ready to sign.

Leaders that fear the same power gain +15 opinion of each other and +20 toward the pacts that keep the peace, so a conqueror draws defensive pacts against itself. This replaces coalitions.

## Older saves (versions 1 to 39)

Open borders, friendship (as non-aggression), alliance (as defensive pact with non-aggression) and research pacts still running become standing agreements. Old trust and grievances become marks on opinion, an embassy +5. An embargo or a denunciation under 15 turns old becomes a rivalry. War weariness becomes exhaustion of each war under way. Border promises, pending joint-war and border-promise proposals, embassies and coalitions are dropped.

## Measured (10 Nov 2026, .NET harness, not Unity play)

Three games on a Small hex map, 6 civilisations, modern rules, 300 turns, idle player who accepts proposals from leaders that do not dislike it:

| Seed | Wars | Without cause | Peaces | Cities taken | Rivalries | Claims | Most agreements at once |
|---|---|---|---|---|---|---|---|
| 701 | 6 | 0 | 4 | 3 | 6 | 11 | 6 (player conquered at turn 147) |
| 702 | 6 | 0 | 5 | 1 | 2 | 7 | 8 |
| 703 | 16 | 0 | 14 | 6 | 7 | 18 | 5 |

First wars at turns 58 to 89. Every rival war had a cause, so the player is always warned. To repeat: `dotnet EngineChecks.dll diplomacy 300`.

## Not built

- Federations, the galactic community and resolutions, subjects and vassals, espionage operations: Stellaris has them, this game does not.
- "Harm relations" envoys: left out to keep one clear use for an envoy.
- War goals that hand over cities at the peace table: peace always leaves each side with what it holds.
- Rivals do not yet ask a third party to join a war.
- Numbers are untuned and the system has not been played by hand.
