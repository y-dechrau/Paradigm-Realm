# Diplomacy Stage 3 — implementation and verification

## Design references

Reviewed official Civilization VI manual excerpts online on 28 October 2026:

- https://cdn.steamstatic.com/steam/apps/289070/manuals/CIV_VI_25TH_ONLINE_MANUAL_ENG.pdf?t=1636653176 — diplomacy actions: friendship, alliance, defensive pacts, joint wars and requests about nearby settlement.
- https://downloads.2kgames.com/civilization/vi/manuals/eu/CIV_VI_25TH_ONLINE_MANUAL_ENG.pdf — timed borders, defensive pacts and research agreements.

These informed the concept of negotiated, time-limited commitments and escalation. Paradigm Realm uses its own numbers, three-civilisation simulation, resources and existing IMGUI interface. No external assets or source code were copied. The full PDF was not downloaded/read end to end.

## Player decisions and connections

| Mechanic | Decision and connected systems | Trade-off | AI and explanation |
| --- | --- | --- | --- |
| Open borders | Trade mobility and peaceful access | Gives reciprocal access; expires | AI evaluates opinion; terms and timers shown |
| Friendship | Commit to peace and grow trust | Must break commitment at a reputation cost before war | AI negotiates; +1 mutual trust each fifth turn while active |
| Defensive alliance | Protect a partner and gain access | Automatic defensive war may draw either side in; other treaties can block it | Both need Code of Laws and friendship; explicit preview and confirmation |
| Research pact | Spend treasury to speed research | 25 gold each; temporary +2 base science/city, affected by later city modifiers | Both need Writing and friendship; scholarly rival more receptive; AI prioritises these technologies when it has friends |
| Trade contract | Exchange treasury and resource access | Supply interruption and insolvency cancel it; no free income | Mercantile rival favours trade, penalises defaults more; shared offer validation, value preview and counteroffers |
| Peace | End war and recover happiness | Eight-turn protection; bribes/ongoing terms can cost money | AI considers duration, weariness, visible military pressure and deal value |
| Denunciation | Warn before a formal war | Creates grievances; three-turn delay | AI uses it before offensive war; declared timing visible |
| Embargo | Disrupt another economy | Also loses your bilateral trade and creates grievances | Shared core rules; user action and acceptance restrictions explained |
| Border promise | Avoid expansion friction | Restricts valuable city locations | AI filters known risky sites; player adviser warns, while manual founding remains possible |
| Grievances and weariness | Weigh expansion/war against reputation and happiness | Conquest can reduce production/science indirectly through unrest | Relationship breakdown, economy label and event log explain consequences |
| Joint war | Coordinate against a shared rival | Both participants declare war; contact and peace restrictions apply | Uses alliance and offer valuation; explicit target and confirmation |

## Implementation

- `Diplomacy.cs`: serializable pair/offer/contract records, trust, grievance, war, treaty and settlement consequences.
- `DiplomacyDeals.cs`: validation, valuation, proposals/counteroffers, acceptance, resource supply, payment settlement, treaty expiry, AI and save validation.
- `DiplomacyUI.cs`: contact-gated partner selection, inbox, editable terms, review/confirmation, treaty status and event history.
- Existing World, city yields, resource gates, treasury, AI research, settlement advice, caravan routes, technology benefits, war confirmation and save migration are integrated.
- Simulation code uses no Unity rendering dependency. No additional per-frame world scan beyond UI and existing yield queries was introduced for diplomacy; turn processing handles expiry and AI. The system has three pair records and a bounded recent event list. Finished offer history is pruned when proposals are submitted.
- Save version 7 migrates version 6 relationships using legacy war/contact/open-border fields; it preserves the old rivals' war. New worlds start the rivals at peace. Earlier migrations chain through version 6. New lists/IDs/timers are validated. Confirmation UI resets when switching worlds.
- Existing player war/access fields remain for backward compatibility. The legacy SetWar(false) engine helper remains for older internal checks; gameplay peace negotiations and AI use the offer path.

## Validation actually performed

Compiled all runtime C# files using .NET 8 / Roslyn with the supplied Unity API doubles. Compiled pure engine checks separately. Passed:

- DiplomacyChecks: legacy migration, contact gates, embassy/gifts, agreement acceptance and refusal, proposal cooldown, expiry, inbox and stale-term validation, trade counteroffers, conserved payments, five-turn settlement duration, insolvency, resource connection loss and non-exportable imports, tech gates, defensive alliances, protected truces, formal/surprise war, weariness recovery, research costs and city yields, caravan embargoes, promises and settlement advice, joint war, AI-to-AI deals and malformed-state rejection.
- Existing Foundation, Content, Empire, Strategy and Settlement checks, including three 50-turn campaigns and other turn simulations.
- DiplomacyUIChecks with API doubles: composer and inbox confirmation, pending count, version 7 full-state JSON round trip, loaded pending acceptance, multi-target declaration preflight, friendship blocking map attacks, and identical state in two 20-turn diplomatic campaigns with identical decisions.
- Existing UIProgram: input, production queues, research/city drawing calls, asset lookup, saves and invalid-load handling.
- Existing VisualRegression: JSON round trip and v6 migration, deterministic simulation, terrain cache, fog protection, no rendering mutation and 1,728 terrain dependency comparisons.
- Existing SettlementUIChecks: advice UI, manual founding, persistent movement and save/load.
- Byte comparison retained all 266 pre-existing artwork, scene and .meta files unchanged.

Test command output is in `Documentation/Diplomacy-Validation.txt`. No Unity Editor compilation, Play Mode, real JsonUtility run or visual screenshot verification was performed. The Editor engine check was extended to include active diplomacy data in its real JsonUtility test for local execution.

## Deliberate limitations

Three civilisations; no general dynamic participant count. Resource access is shared, not quantified inventory. Incoming gold cannot finance outgoing payments in the same payment settlement step. Border violations must be observed at founding; no retroactive investigation. Existing alliances are defensive commitments, without levels or specialised alliance categories. Military valuation uses visible forces, not omniscient army totals. AI initiates access/friendship/alliance/research/trade/peace proposals and denunciation/war; it can evaluate border promises and joint-war proposals but does not proactively generate those or embargoes. No congress, spies, diplomatic victory, vassals, city-state influence, city transfers, elaborate war objectives, multiplayer or ships. Balance is provisional until sustained human Unity playtesting.
