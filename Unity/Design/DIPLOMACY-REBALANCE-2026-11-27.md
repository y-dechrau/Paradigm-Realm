# Early diplomacy rebalanced, and the owner's other requests of 27 Nov 2026

No save version change. Older saves are brought within the new caps once on loading (`World.RebalanceOpinion`, called from `LoadFile` and after `UpgradeDiplomacy40`).

## Why two neighbours stood at +34 and +83

Measured on the owner's turn-50 autosave with `Editor/OpinionProbe.cs`:

| Aurelian Republic, +83 | | Zarevan Crown, +34 | |
|---|---|---|---|
| Envoy sent | +30 | Two pacts | +15 |
| Three pacts | +20 | Difficulty | +10 |
| Difficulty (labelled "Wary of you", wrongly) | +10 | Turned us down | -8 |
| Past dealings | +7 | Trust (counted again) | +5 |
| Trust (counted again) | +5 | Same government | +5 |
| Same government | +5 | Ruler moods | +4 |
| Long friendship | +3 | Long friendship | +3 |
| Ruler mood | +3 | | |

The causes: a free envoy alone was worth +30; each standing pact added up to +10 with no cap for being at peace; the difficulty bonus was counted in full and named backwards; trust was added to opinion as well as kept on its own; and "past dealings" (up to +35) and gifts (up to +20) piled up from routine exchanges. Nothing in that list needed shared history, so +80 came from routine contact.

## What changed

- **Caps on routine warmth** (`OpinionTuning.cs`): living at peace (non-aggression, borders, research and gate pacts together) at most +5; trade +4; past dealings at most +8; gifts at most +6; an envoy +6; an alliance +6; same government +2; long friendship at most +10 over ten turns (scaled by game speed). Difficulty adds half its old amount and is named truly ("Well disposed to you (easier difficulty)" / "Wary of you (harder difficulty)"). Trust is no longer part of opinion.
- **+80 only through history:** the routine maximum is 41 (checked). Above that needs memories: promises kept, help given, common enemies, delivered agreements (each delivery raises trust; a fulfilled supply accord adds trust 10 and "kept their word" up to +8).
- **Memories scaled by impact and speed:** fade rates per kind (`OpinionMods.FadeEvery`), stretched on Long games and shortened on Quick (`SpeedPercent`); a broken promise is felt for much longer than a declined offer.
- **Three judgements kept apart** (`DiplomacyJudgement.cs`): Opinion (warmth), Trust (untested at first; raised by kept promises and help, cut by broken ones; `TrustHistory`), and Interest (what the leader stands to gain or lose from you now: trade, a common enemy, timber you supply, land you both want, crowded borders, fear). A friendly leader can still refuse to give up land it needs.
- **The portrait** in the top right shows the attitude word and one active issue ("Cordial · Border concern", `ActiveIssue`); hovering shows the strongest reasons, trust with its history and the leader's interest (`LeaderSummary`).
- **One playable settlement dispute:** the valley talks of the same day (`Design/VALLEY-TALKS-2026-11-27.md`); refusals and obstacles are spoken in the leader's voice.

## Measured (OpinionBalanceChecks, 50 turns from the same opening)

| Case | Opinion | Trust | Main reasons |
|---|---|---|---|
| Idle contact | -12 | 0 | turned us down -17 (ignored offers), honest ruler +3, same government +2 |
| A valuable agreement fulfilled (timber, 12 deliveries) | -7 | 15 | kept their word, 12 deliveries made |
| A border promise broken (paid to stay off the ford, then settled it) | -19 | 0 | broke an agreement, named in the trust history |

Also checked: two rivals left to themselves for 50 turns stay at or under +60; saving and loading half-way keeps every mark and trust exactly, loading twice adds nothing, and the loaded game goes on as the original.

How to repeat: `RealmTools.Checks` (runs `OpinionBalanceChecks`); `Unity -batchmode -executeMethod ParadigmRealm.OpinionProbe.Run -probeSave <save.json> -probeOut <file>` for a real save; `-valleyTalk <plenty|short|lasting|alternative|farmland>` and `-devReasoning` in the game; `-hudShot <folder> -valleyOnly` photographs the talks, your own proposal and the quick pact answer.

## The other requests of the same message

- **The leader's call made sense of the place:** "King Aldric wants to talk about the ford 6 tiles north of Northwind." Places are named from your side (`PlaceName`, `CompassWord`, nearest of your cities within 14 tiles); the call box wraps its sentence onto two lines and shrinks rather than cuts it.
- **Quick answer to a plain pact:** when a leader offers only a pact (no gold, resources, cities, passage or gates asked or given; `World.PlainPact`) the call box has Accept, Decline, Details and Later.
- **Your own land and timber proposals** (`ValleyProposals.cs`, composer in `ValleyTalksUI.cs`): ask them to leave a place unsettled, promise to leave one yourself, sell timber now, or buy timber each turn; "Choose on map" names any tile; the leader answers live from its own stake in the place (`PlaceStake`: a settler bound there, the site's worth over its next best, timber it would end a shortage with). Open from Court > Land and timber, or "Your own terms" in a talk.
- **Left-clicking woodland outside your borders** no longer opens the site panel.
- **Rivals never play your civilisation** (`AssignRivalCivilisations`).
- **Barbarian raiders walk** (`RaiderMotion.cs`): they glide tile to tile like units with a six-frame walk made in SpriteCook from the raider's own picture (`Resources/Visuals/Movement/raider-move-00..05`; 20 credits, 49 left) and face the way they walk. `-hudShot <folder> -raiderOnly`.
- **New Game pages after the owner's two pictures** (`SetupWorldUI.cs`, `SetupCivUI.cs`): each picture with its changing words cleared is the page (`Resources/UI/Setup/setup2-world`, `setup2-civ`); buttons, rail, knob and tick boxes are cut from it (`setup2-*`); everything else is live. `-hudShot <folder> -setupOnly` presses every control on both pages.
- **Military > Forces after the owner's picture** (`MilitaryScreenUI.cs`): the camp painting, four panels, the service line with Mobilise…, Formations (with the soldiers' painting), Raise and Programme. Same rules and reasons as before (`ForcesUI.cs` keeps the shared pieces). `-hudShot <folder> -forcesOnly`.
- **Freeze reports while the game was in the background:** the freeze recorder counted time while the window was not focused (Windows stops painting it), so the owner's report was full of false alarms; it now pauses while the window is unfocused (`FreezeWatch.Focus`).

Not hand-played. Every number above is a first tuning.
