# Faith, Belief and Religious Power (owner design, 8 Nov 2026)

The owner's design asks for religion to be something a civilisation develops, shapes, competes over and fights for. It names six elements: origins, beliefs, institutions, influence, power and evolution. This note records what was built from that design, where it lives, and what is still missing.

**Where it lives:**
- Engine: `Unity/Assets/Scripts/FaithWorld.cs`.
- Screen and event panel: `FaithUI.cs`.
- Built on the existing religion code: pantheons, 23 historical faiths, pressure between cities, missionaries, priests, the faith tree, and religious buildings bought with faith.

It applies under modern rules, meaning new games and upgraded saves (save version 36).

**Checked by:**
- `ContentChecks.FaithDesignChecks` in Unity.
- A Windows build with screenshots of all six tabs and an event panel.
- A 300-turn headless game: three faiths founded (turns 196–287), prophets, state churches, and institutions up to level 6. That run had no schism.

**Not hand-played.**

## Built

| Design section | In the game |
|---|---|
| 1–2 Origins | A faith's origin comes from what its people lived through. In order: famine scars or plague in the holy city, then a graveyard of plague dead nearby, then a fire mountain nearby, then five or more battle victories, then an ambitious ruler with three or more cities, then knowing Philosophy, then a mountain or lake nearby, and otherwise ancestor worship. The result is one of eight origins (`OriginFor`). Each origin gives the holy city a small bonus and suggests beliefs. Founding is still the player's decision. |
| 3 Beliefs | The founder picks a core belief, a social doctrine, an economic doctrine and an afterlife. There are 5 core beliefs (Polytheism, Monotheism, Philosophical, Animism, Ancestor worship). There are 10 social doctrines (Equality, Hierarchy, Charity, Asceticism, Scholarship, Militarism, Pacifism, Expansionism, Traditionalism, Tolerance). There are 6 economic doctrines (Tithes, Pilgrimage, Sacred trade, Monastic economy, Charitable giving, Religious taxation). There are 7 afterlives, which give identity only and have no direct effect. Every other choice has one clear effect, and some carry a trade-off: Taxation brings gold but lowers authority and feeds reform; Hierarchy brings production but costs content in large cities. Rivals choose from their agenda and the origin. |
| 4 Institutions | There are eight levels, from a sacred gathering to a supreme religious institution (`InstitutionLevel`). Each level helps the faith spread. Religious Authority (0–100) is kept separate from faith. A civilisation chooses an independent church, a state religion (authority capped at 80, +1 content where the state faith is followed) or a theocracy (+2 faith and −1 science in every city). |
| 5 Population | Every city holds a share of each faith plus the unaffiliated, shown on the city screen and the Expansion tab (`CityShares`). Conversion is gradual pressure. Pressure is helped by authority, institution level, Monotheism, Expansionism, Printing and trade agreements. It is resisted by an established majority, Traditionalism, Monotheism and religious policy. After Industry more people are unaffiliated. A conqueror's faith gains pressure in the city it takes. |
| 6 Missionaries and resistance | Missionaries reach 20 tiles with Sailing (12 before). Four policies: permit all, restrict foreign missionaries, ban foreign faiths, forced conversion. Banning or forcing breeds secret worship (−1 content) and persecution events. |
| 7 Holy places | Each faith has a holy city and up to two sacred sites chosen at founding: ancient ruins, fire mountains, mountains, lakes and rivers. Discoveries add more. Losing the holy city costs 25 authority and feeds reform, but the faith lives on. |
| 8 Religious diplomacy | Opinion reasons: "Shares our faith" +10, "Follows another god" −5 for Monotheists, "Tolerant of your faith" +5, "Persecutes our faithful" −15, "Protects our faithful" +5, "Condemned our faith" −15. Actions: fund clergy abroad, demand protection (refusal gives just cause for a holy war that does not alarm the world), religious treaty, condemn a heresy, call a council. |
| 9 Reformation and schism | Reformation changes doctrines for faith, at a cost in authority. A reform movement grows from wealth, size, low authority and a lost holy city. At its height the Great Schism makes the founder choose: a council, accept the reformers, suppress them, or let the faith divide. A branch takes one of eight branch slots, with its own name (for example "Reformed Buddhism") and doctrine, and the distant or foreign cities go with it. Faiths outlive their founders. |
| 10 Government | The church choice can be combined freely with any government. The screen describes how each government relates to religion. |
| 11 Technology | Philosophy enables reformation; Printing helps a faith spread; Sailing extends missionary reach; Industry increases the unaffiliated; Computing widens pressure from 10 to 15 tiles. |
| 12 Faith and authority | Faith is in the top bar. Each city stores its own faith for religious buildings, and the national pool pays for missions, councils, clergy and reforms. Authority is a separate measure. |
| 13 Religious victory | The faith of 50% of the world's people, of half the rival capitals, the holy city and two sacred sites, and the supreme institution, all held for 20 turns. This is a fourth bar on the Victory screen. |
| 14 Events | The Great Prophet, the Great Schism, the Sacred Discovery and Religious Persecution arise from the state of the world, each with four choices. Rivals decide their own. |
| 15 Interface | The Religion screen has six sections: Overview, Beliefs, Institutions, Expansion, Diplomacy and History. A decision panel opens on the map. |

## Not built yet

- Faiths emerging without the player founding them. Origins are chosen from events, but founding is still a click.
- Several of the design's suggested religious units: Preacher, Scholar, Pilgrim, Inquisitor, Religious diplomat, High priest and Prophet. The existing Priest and missionaries cover conversion.
- Migration and marriage as ways a faith spreads.
- A religious map overlay.
- A separate religious diplomacy screen; it is a tab instead.
- Religious alliances as their own treaty; ordinary alliances are used.
- Festivals, pilgrimage programmes and religious research as things to spend faith on.
