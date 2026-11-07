# Diplomacy research, 8 Nov 2026

The owner asked for research into how strategy games make diplomacy feel alive, and for the results to be added to the game. This note lists the sources, what each one taught, and where it now lives in the code. Everything listed is in `Unity/Assets/Scripts/DiplomacyDepth.cs` unless another file is named. It applies under modern rules, which covers new games and upgraded saves.

## Sources and what they gave

| Source | Lesson | In the game |
|---|---|---|
| Civilization VI diplomacy and agendas (Firaxis; fan wiki and guides) | Leaders have a fixed agenda and visible reasons for their opinion of you, so you can work out why a leader dislikes you. | Opinion is now a list of named reasons that add up exactly to the number (`OpinionLines`, `OpinionSum`). The diplomacy screen has a "Why … feels this way" panel and Discuss lists every reason. |
| Civilization VII dev diary "Diplomacy, Influence and Trade" | Influence is a diplomatic currency earned each turn and spent on actions. | Each civilisation earns influence: 1 a turn, +1 for each embassy it holds, and +1 for each friendship, up to 3. It spends it on **Send envoys** (20: trust +8, alarm −10, once every five turns) and **Incite others** (30: everyone who likes you grows 10 more alarmed at the target, and the target holds 10 grievances against you). |
| Crusader Kings III (Paradox; guides on traits, stress and dread) | Opinion comes from people as well as states: a ruler's traits and stress change how they judge others. | The ruler's traits from the living world shape opinion (`RulerOpinion`). For example, a paranoid ruler fears your army, an ambitious one envies your size, a loyal one stands by friends, an honest one despises broken promises, a pious one judges your faith, and a drunkard has moods. A ruler under strain is short-tempered. |
| Europa Universalis IV (Paradox; community notes on aggressive expansion and coalitions) | Expanding by force alarms everyone who knows you. The alarm fades slowly, and enough alarmed nations band together. | Capturing cities, surprise wars and crowding settlement add alarm, scaled by each leader's hatred of warmongers. Alarm fades by 1 a turn, or 2 with an embassy. Three or more alarmed civilisations that do not like the target form a coalition, whose members defend each other against it and are more eager to attack it. A member leaves when its alarm drops below 25. |
| Soren Johnson, Designer Notes on Old World diplomacy, and his GDC 2022 talk on Civilization III's bargaining table | A bargaining table where items trade value for value turns diplomacy into min-maxing; leaders should not act like vending machines. | Deals are judged by what the leader needs and what kind of leader it is (see below), not at a fixed price. |
| Utility theory in game AI (Dave Mark and Kevin Dill, GDC AI Summit talks; *Behavioral Mathematics for Game AI*) | Each option is scored by response curves that read the AI's own state. | `GoldNeed`: a leader low on gold or in debt values gold up to 2.5 times as much, and greed shifts this. `ResourceNeed`: a hungry leader values food resources double, one at war values iron and horses at 180%, and one with cities in disorder values luxuries more. The offer assessment names these needs. |
| Finite state machines vs behaviour trees for AI personality | A leader's behaviour switches between named states at set thresholds. | Each rival has one of four postures: Peaceful builder, Expansionist, Aggressor or Defensive (`UpdatePosture`). It enters a posture at one threshold and leaves at a lower one, so it does not flicker. A rival becomes Defensive when a coalition forms against it or it is losing a war, and Aggressor when war ambition is high (bold leaders sooner). Posture changes war ambition (+15 for an Aggressor, −10 for a Builder, −25 for a Defensive rival), soldier numbers (+2 when Aggressor or Defensive) and willingness to sign deals. It is shown on the screen and announced when it changes. |
| Modding: Civilization IV LeaderHeadInfos XML, Civilization V and Vox Populi leader flavours (CivFanatics) | Leaders are rows of 1–10 flavour values, shifted by up to 2 at random each game. | Five flavours per leader: Hates warmongers, Boldness, Loyalty, Greed and Forgiveness. They come from the leader's agenda in a table (`DiplomacyFlavours.ByAgenda`) plus a fixed roll of −2 to +2 for each game. They are listed under Discuss. |

## Not done yet

- Hidden agendas, discovered through embassies.
- Sanctions with lasting yield penalties.
- Event-driven diplomacy in the style of Old World.
- Behaviour trees.
- Loyalty is shown but drives nothing yet. Hates warmongers drives alarm, Boldness drives posture, Greed drives the value of gold, and Forgiveness (7 or more) makes grievances fade twice as fast in peace.

## Links

- https://civilization.fandom.com/wiki/Diplomacy_(Civ6)
- https://civilization.2k.com/civ-vii/game-guide/dev-diary/diplomacy-influence-trade/
- https://www.designer-notes.com/old-world-designer-notes-10-diplomacy
- https://www.pcgamer.com/soren-johnson-says-civ-3s-bargaining-table-was-a-big-mistake
- https://gdcvault.com/play/1012410/Improving-AI-Decision-Modeling-Through
- https://en.wikipedia.org/wiki/Utility_system
- https://forums.civfanatics.com/threads/vox-populi-diplomacy-guides.683886/latest
- https://forums.civfanatics.com/threads/civilizationv-the-guide-of-leader-flavors.401897
- https://modiki.civfanatics.com/index.php/Civ4LeaderHeadInfos
- https://steamcommunity.com/app/236850/discussions/0/3044983510175025873/ (Europa Universalis IV aggressive expansion, community)
- https://segmentnext.com/crusader-kings-3-traits-guide

Most of the mechanics notes come from fan wikis, forums and guides rather than official design documents, so the numbers here are our own tuning, not copies.
