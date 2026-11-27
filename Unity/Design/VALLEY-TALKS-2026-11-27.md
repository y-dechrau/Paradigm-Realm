# Valley talks: early diplomacy over a place (owner brief, 27 Nov 2026)

Built in `ValleyTalks.cs` (rules), `ValleyTalksUI.cs` (the talk), `ValleyTalksScenario.cs` (`-valleyTalk <variant>`) and
`ValleyTalksChecks.cs`. Photographed with `-hudShot <folder> -valleyOnly` (`ValleyTalkShot.cs`). No save version change: the new
lists `valleyIssues`, `valleyAccords` and `supplyRecords` load empty.

## How a neighbour decides (`AssessValley`, every 3 turns per home rival you have met)
From its own knowledge only (`Nation.known`, units in its sight, woodland it has seen on your land):
1. **Use:** the timber its work under way and queued would install (forecast with unlimited timber, so slow shields count),
   or the building it would choose next. No use: nothing to say.
2. **Shortfall:** the worst gap at any turn: timber that work would have used by then, against its store, income and promised
   deliveries by then. It checks two spans: 12 turns (`Horizon`) and 24 (`LongHorizon`).
3. **Alternative:** its best woodland camp it can reach and use (the earlier site rules), away from the contested place.
4. **Gain:** how many turns sooner its work finishes with the place (a camp there), against the alternative. For land: how much
   better the place is as a town than its next best site.

## What it then does
| Decision | When |
|---|---|
| silent | no use, or no gap at any turn; or a small gap and no timber of yours seen to buy |
| buy timber | gap at most 8 over both spans, and it has seen your woods |
| alternative (quiet) | a lasting gap, but its own reachable wood serves as well |
| ask you to leave the place | a lasting gap, the place is its only practical source, and it has seen your people there |
| ask for the land | the place beats its next best site by 20 points or more, whatever the timber |
| quiet claim | short, has a place in mind, has seen no one else there |

**Counteroffers:** it judges yours by three tests: do the deliveries cover the gap within 12 turns, does it trust you, and how
long do they last. Trust combines its trust in you, +2 per delivery you have made, -5 per one you missed, -15 per broken
promise, and its Loyalty.

## Agreements
- **A promise to leave a place unsettled** covers every tile within 3 tiles of it (`KeepRadius`; towns must be 4 apart). It is
  shown in the Found prompt and read by every settler, yours and the AI's (`SettlementDiplomacyWarning`).
- **A supply agreement** delivers timber each turn through the stockpiles, and the neighbour pays per delivery. It counts
  deliveries made and missed; missing 2 breaks it. A neighbour that accepts your supply turns its settler to its next best site
  and keeps off the place for 48 turns after the last delivery.
- **Breaking a promise** costs opinion and trust in proportion to what was paid and the leader's Forgiveness. It is recorded
  with the place and the payment.
- **Repeats:** the same matter is not raised again for 15 turns, or after it is settled, unless the gap has more than doubled.
  An open offer is reassessed each turn and withdrawn with the reason when the need goes.

## Reading a leader's mind (developers)
Run the game with `-devReasoning`: the Court's full page shows each leader's reckoning (`ValleyReasoningText`), e.g.
"silent — enough timber: needs 21 within 12 turns and expects 100". The five variants' reckonings are in
`Documentation/Valley-Talks-2026-11-27/reasoning-five-variants.txt`.

## Playing it
Start the game with `-valleyTalk lasting` (or `plenty`, `short`, `alternative`, `farmland`). You are Riverfold; King Aldric of
the Azure Crown is the neighbour.

1. End the turn. Aldric calls: "Your settlers are near the southern ford. Do you mean to stay?…"
2. Press Answer, then "Land or timber?". In the lasting game he answers that it is the timber, with the turn and the gap; in
   the farmland game he answers that it is the land.
3. Choose 4 timber a turn for 12 turns and press "Propose these terms". He accepts, turns his settler away and founds Kingsport
   5 tiles from the ford two turns later.
4. Settle the ford with your own Settler. Over the next turns the notes show each delivery; after 12 he remembers them all.

## Not built
- Grain or other goods as payment (gold and timber only).
- Haggling over the price in the talk.
- Disputes that turn into war.
- Talks between two rivals.
- Your own outlying camps entering the neighbour's view of your supply (it counts only woodland it can see).
