# Statecraft: strong by staying small or by growing large (24 Nov 2026)

Owner request: let the player play between a large civilisation and a small one. Stage 1 of three is built. Every number is in `StatecraftRules` (`Statecraft.cs`) and is a first guess.

## Built (stage 1)

- **One national choice**, on Empire > Statecraft (`StatecraftUI.cs`), from Code of Laws:

| | Centralised | Balanced | Expansive |
|---|---|---|---|
| Output | +8% science, shields and gold for each city fewer than 8, at most +40% | as before | as before |
| Army | new soldiers start as Veterans | as before | 60% of citizens can serve in peacetime, not 50% |
| Settlers | 50% dearer | as before | 25% cheaper |
| Extra cities | as before | as before | a third less gold |
| Influence | +1 a turn for each pact, at most +3 | none | none |

- **Changing:** 5 turns to settle, in which neither way counts and every city has one citizen fewer content. A choice must stand for 15 turns.
- **Size and technology:** each city beyond the fourth makes technology 3% dearer, at most 45%, for every civilisation of the home world (`SizeTechPercent`). The peoples behind the gates keep pace by their own rule and are exempt.
- **Preview:** the screen shows your realm's figures under all three choices (`PreviewStatecraft`) before you choose.
- **Rivals** (`StatecraftWanted`): aggressors and settlers lean Expansive, builders with five cities or fewer Centralised, eight cities or more always Expansive. They change at most every 25 turns and not in wartime. A Centralised rival stops settling at five cities; an Expansive one wants three more than its difficulty gives it.
- **Saves:** switched on for new and loaded games (`EnableStatecraft`), off in `World.New`. No save version change: `Nation.statecraft`, `statecraftSince` and `statecraftTold` load as Balanced, never changed and not told.
- **Checks:** `StatecraftChecks`. `-hudShot` saves `hud-empire-tabs`, `hud-statecraft`, `hud-statecraft-ask`, `hud-statecraft-settling`.

## Measured

`Editor/StatecraftProbe.cs` (`-executeMethod ParadigmRealm.StatecraftProbe.Run`), 8 games of up to 250 turns on Medium, six rivals, half made small and Centralised, half large and Expansive:

| Turn 250, average | Cities | Citizens | Technologies | Science | Shields | Army strength | Gold held |
|---|---|---|---|---|---|---|---|
| Small, with Statecraft | 5.3 | 63 | 22.0 | 211 | 288 | 70 | 498 |
| Large, with Statecraft | 11.1 | 103 | 21.8 | 271 | 424 | 125 | 187 |
| Small, rules off | 5.4 | 66 | 22.3 | 180 | 270 | 79 | 582 |
| Large, rules off | 10.8 | 100 | 21.8 | 219 | 395 | 130 | 140 |

- At turn 200 the small realms made 130 science and 218 shields against 111 and 176 with the rules off; the large ones changed little.
- The most advanced civilisation at the end was small in 5 games and large in 3 (4 and 4 with the rules off).
- Technology is close in every case because `MinTurns` caps how fast one can be learned, so extra science late in the game is partly wasted. The large realms keep a clear lead in army.
- Only one game ended in a victory (religion), so this compares strength, not wins. Rivals play both sides; nobody has hand-played it.

## Not built

- **Stage 2:** army types (Elite, Standard, Levy) and mercenaries.
- **Stage 3:** autonomous cities and unions.

## Found on the way

End Turn could fail when a faith split (`AuthorityTurn` walked the list a schism adds to). Fixed.
