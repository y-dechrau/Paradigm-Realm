# Paradigm Realm — Diplomacy, Stage 3

## Play this version

1. Extract the ZIP into a **new folder** so your current project stays available.
2. In Unity Hub, choose **Add project from disk** and select the extracted folder containing `Assets`, `Packages` and `ProjectSettings`.
3. Use the project's Unity version, **6000.3.25f1**. Open `Assets/Scenes/Earth.unity` and press Play.
4. Meet a rival, then open **Diplomacy** in the top bar. A number beside it means an offer needs your response.
5. Select an agreement, set terms, choose **Review proposal**, then confirm. Incoming offers also require review and confirmation.

Run **Paradigm Realm > Run Engine Checks** in Unity before continuing an important save. Keep a separate copy of your save files. Version 6 saves migrate to version 7; do not load a newly saved version 7 game in the older build.

## What changes while playing

- Rivals can negotiate with each other. New games no longer force the two rivals into permanent war. Older saves retain their existing wars.
- Open borders, friendship, defensive alliances and research pacts have durations. Peace protects both sides for eight turns.
- Negotiate upfront gold, gold per turn and connected resource access. AI can refuse terms or return a priced trade counteroffer. Incoming proposals expire after three turns.
- Writing enables research pacts between friends: 25 gold each and +2 base science per city. Code of Laws enables defensive alliances between friends. Both participants need the technology.
- Alliances grant access and automatic defence. You can be drawn into a war when your ally is attacked. Existing truces or friendships can prevent that defence and harm relations with the ally.
- Joint war proposals commit both participants against the third civilisation, subject to contact and treaty restrictions.
- Embassies and gifts improve trust. Denunciation allows formal war after three turns, until the warning expires after fifteen turns. Surprise war causes more grievances.
- Captures, pillaging, nearby settlement and broken agreements create grievances. They decay during peace. Friendship gradually builds trust.
- Border promises affect settlement recommendations. Known violations are warned about; the AI avoids known promise-breaking sites. Observed violations damage trust.
- Embargoes suspend caravan income, block technology purchases and new foreign caravan deliveries, and end economic contracts. Research cooperation is also ended.
- Long wars build weariness, reducing happiness by up to four per city. Peace recovers two weariness each turn.
- Diplomacy displays acceptance reasons, exact terms, relationship factors, treaty timers and recent events. Economy shows incoming and outgoing treaty payments.

## Important rules

Resource trade shares access to the supplier's domestic, improved, capital-connected resource. It is not a stockpile or an exclusive export: the supplier keeps access. Imported resources cannot be re-exported. Losing the supply removes the benefit immediately and ends the contract at the next payment check.

Gold-per-turn payments are settled before ordinary turn income. You need treasury reserves to cover outgoing payments. Incoming treaty payments are not used to finance circular credit in the same settlement step. A payment default cancels the affected contract and creates grievances. A pair can have one recurring contract; end it before replacing it. One proposal per direction per turn prevents proposal spam.

Peace lasts eight turns; a recurring payment attached to peace uses the selected trade duration. Breaking a trade contract does not remove the truce. Friendship/alliance must be ended before a declaration of war, with the displayed reputation cost. Ending friendship also ends its research pact. Expired access does not teleport troops: they can withdraw into territory they may enter.

A border promise concerns cities founded within six tiles of the other side's cities. Violations are recorded when the founding is observed. Settlement advice uses known cities, not hidden information. Neither rival has perfect strategic foresight.

## Verification and scope

The pure C# engine suites and supplementary Unity API-double suites passed. They cover negotiation, contracts, diplomacy AI, migration, full-state JSON round trips, map war protection and existing gameplay regressions. All 266 existing artwork, scene and metadata files were preserved byte-for-byte.

**Unity Editor and Play Mode were not available here.** API doubles do not prove Unity serialization or visual layout. The Editor menu includes a real Unity JSON round-trip check for you to run.

This is an implemented bilateral diplomacy expansion for the existing three-civilisation game. It does not yet include a world congress, diplomatic victory, espionage, city-states, vassalage, city transfers, multiplayer negotiation or naval systems. War objectives are limited to war/peace and joint target commitments; there is no separate war-score/demand system. Personality and military judgment remain deliberately simple. Long-term balance needs playtesting.

See `Design/DIPLOMACY-STAGE-3.md` for decisions, sources and test detail.
