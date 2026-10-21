# Paradigm Realm — integrated design baseline

Version: Foundation Alpha 0.2 • 17 October 2026
Source: Matt and Gary's complete 36-topic design discussion.

This document incorporates the proposed design into the Unity project. It is a target specification, not a declaration that every feature is finished. See IMPLEMENTATION-STATUS.md for actual coverage. Values are initial balancing proposals. Earth comes first; additional dimensions follow. Preserve existing drag-to-move, persistent orders, first contact, custom names, road connections and tile tooltips.

## 1. Reality, fantasy or both
Historical foundations develop into speculative science and other realms. Early play covers survival, farming, settlement, government and warfare. Middle periods add trade, industry and international politics. Late development introduces computing, automation, unusual energy and dimensional research. Different civilisations create alternative histories rather than following predetermined historical outcomes. Decide explicitly whether extraordinary effects represent unfamiliar physics or genuine magic before creating realm content. Starting scope is grounded Earth with extensible world data.

## 2. Realms and portals
Each realm is a separate map with its own terrain, hazards, resources, settlement suitability, energy conditions and potentially inhabitants. Discovery, activation and stabilisation are separate milestones. Portals have ownership, operating energy costs and transport capacity; heavy equipment occupies more capacity. Probes or expeditions should scout destinations before settlement. Captured or closed portals interrupt reinforcements and trade without deleting stranded units. Self-sufficient colonies can survive isolation. Future units and cities need world identifiers and cross-world routing. No active realm simulation is claimed for this release.

## 3. Map types
Target terrain representation separates base terrain, elevation, features, resources, improvements and ownership. A forested iron-bearing hill should be representable without multiplying terrain categories. Continents, archipelagos, a large continent, inland seas and a handcrafted Earth are proposed. Seeds reproduce worlds. Starts need reliable food and production; rare strategic resources encourage interaction. Rivers, coasts and mountain passes should shape settlement. The current compact map guarantees land connections because ships are not yet implemented; it is not a final realistic geography generator.

## 4. Navigation
Keyboard/edge panning, mouse-wheel zoom, middle-drag panning, minimap navigation and next-idle-unit controls form the target. Clicking selects; dragging a unit previews and issues a persistent route. Preview costs, approximate arrival and war/transport/portal consequences. Escape cancels; notifications navigate to their source. City, military, treasury, research, diplomacy and resource screens support large empires. Remember camera positions per realm. Current navigation uses WASD/middle drag, scroll bars and zoom buttons; arrows issue movement.

## 5. City management
Cities track population, food surplus/storage, production, income, science, happiness, health, buildings, queues and territory. Citizens work eligible tiles or specialist slots. Food storage supports growth and buffers shortages; sustained starvation reduces population. Specialists ultimately include researchers, merchants, engineers and entertainers. Governors offer balanced, growth, production and research priorities, with manual locks preserved. Switching projects should eventually preserve project-specific progress without allowing production-transfer exploits. The alpha uses a bounded queue; explicit cancellation discards current progress.

## 6. Resources and construction
Food resources include grain, fish and livestock; construction materials include timber, stone and steel; strategic resources include horses, iron and oil; luxuries include spices, silk and gems; advanced materials support specialist and dimensional projects. Separate production capacity from physical materials. Begin with connected access; later introduce stockpiles selectively for fuel and advanced materials. A mine supplies access only when owned, improved and connected. Explain missing technology, facilities, materials and resource connections for every unavailable project. Never imply a displayed tile yield alone grants military access.

## 7. Finance and economy
Taxes, commerce, domestic/foreign trade, exports and diplomatic payments provide income. City administration, units, buildings and advanced infrastructure create expenses. Low/normal/high taxation trades revenue against happiness. Show national and eventually city-level balances, immediate purchase cost and recurring upkeep. Treasury reserves permit temporary deficits. Warn before bankruptcy; halt optional spending first, then reduce services or combat effectiveness, with later desertion only after clear warning. Early free unit support prevents the opening from collapsing financially. Loans and financial markets are deferred. Current connected domestic cities provide trade income; foreign resource sales do not yet exist.

## 8. Units
Roles: settlers, workers/engineers, scouts, infantry, counters, ranged, mounted/mobile, siege/artillery, naval combat, transport ships, aircraft, support, diplomats and spies. Every role needs a purpose, cost and weakness. Settlers are consumed founding cities. Workers remain after multi-turn jobs. Scouts gain sight and efficient rough-terrain travel. Infantry capture and hold territory; spearmen counter mounted troops. Archers cannot capture cities; siege breaches walls. Medics assist healing. Naval/air/espionage systems need dedicated rules before implementation. Current catalogue adds Spearman, Horseman, Catapult and Medic while preserving legacy unit identifiers.

## 9. Movement
Use movement allowances, explicit terrain costs and connected-road discounts. The implemented allowance scale is open terrain 2, forest/hills 3, roads 1; scouts pay 2 off-road. Mountains and oceans are impassable for the current land roster. Rivers, bridges and zones of control remain future work. Persistent routes stop at blocked paths or peaceful restrictions and never declare war automatically. New orders replace old ones. Route knowledge must not disclose hidden obstacles or enemies as current intelligence; the next actual step is checked before movement.

## 10. Stacking and transport
Test a cap of three military units plus one civilian/support unit on a tile. Grouped armies should move at the slowest member's speed; formation does not grant free movement. Defence selection must be visible and consistent. Siege may eventually affect multiple crowded units. Ships require capacity, loading/unloading costs and amphibious penalties. Heavy units can use extra slots. A sunk transport may lose passengers, with clear warning. Current production waits if the spawn tile is full. Naval transport and grouped orders are not implemented.

## 11. Buildings
Food, production, commerce, science, recruitment, defence, health, happiness and administration are distinct building functions. Granaries store food, libraries improve research, walls improve defence, workshops improve output, markets earn income, barracks train recruits, gardens improve happiness and courthouses reduce administration. Hospitals currently improve friendly-city healing. Terrain, technology and resource prerequisites should encourage specialisation. Ports, factories, power, mothballing and demolition come later. Costs and benefits must be visible. Do not add nominal buildings without functioning effects.

## 12. Infrastructure
Farms, mines, quarries, pastures, plantations, roads, rail, forts, watchtowers, airfields, depots and portal installations form the target roster. One main economic improvement can coexist with transport. Construction takes turns; terrain and technology eventually alter work speed. Clearing forests should trade continuing value for a one-off benefit. Pillaging interrupts output or connectivity; repairing restores it. The alpha has multi-turn roads, farms/pastures, mines and forts, with pillaging and rebuilding. Road segments align at tile boundaries at every supported zoom.

## 13. Borders
Cities claim nearby land and expand influence. Distinguish unclaimed, owned and contested land. Ordinary expansion cannot silently transfer cities. Military access, resource rights and intelligence sharing are separate concepts. Open borders permit peaceful travel; ending permission must allow withdrawal without enabling exploits. Capturing a city recalculates local control. Current influence expands with city age and resolves overlap by distance, then stable city id. Cultural pressure, contested-tile diplomacy and formal withdrawal timers remain targets.

## 14. Trade networks
Owned, improved resources must connect to usable networks. Roads come first; navigable rivers and ports follow. Networks respond to roads, pillaging, occupation, borders and agreements. A disconnected city should ultimately use local supplies while losing external access. Trade contracts specify commodity, payment and duration and cannot overcommit limited supplies. Route overlays should identify failures. Current strategic access uses the owned road network from the oldest remaining city; this is explicitly a simplified capital network, not the final local-network system.

## 15. Research tree
Branches span food, industry, military organisation, transport, government/commerce, medicine, energy/computing and dimensional science. Research one technology at a time. Save separate progress when switching. Dependencies connect related branches. Show cost, duration estimate, prerequisites and unlocks; eventually offer a path to distant targets. Every discovery needs practical value. Current tree has 12 technologies. Agriculture enables farms/granaries, Archery archers, Writing libraries, Bronze Working mines/spearmen/workshops/barracks, Animal Husbandry horsemen, Siegecraft catapults, Medicine medics/hospitals, Commerce markets, Administration courthouses, Engineering forts/walls/knights, Industry riflemen, Electricity improved science. Later eras need much more content.

## 16. Technology effects
Distinguish unlocks, automatic yield improvements, upgrade eligibility, new actions, resource discovery and new dependencies. Better farming can improve existing infrastructure; rail requires construction. Upgrades consume gold and resources in friendly territory. Older assets remain usable rather than disappearing on discovery. Buildings may upgrade or remain low-cost alternatives. Exact effects must be available in tooltips/reference text. Current implementation deliberately does not model power grids, fuel or hidden deposits.

## 17. Diplomacy
Unmet, peaceful and hostile states support first contact, war and peace. Add open borders first, then trade, non-aggression, alliances, defensive pacts, payments and intelligence agreements. Attitudes should reflect promises, aggression, pressure, trade and assistance. AI explains refusals. Confirmation precedes hostile action and discloses broken agreements. Peace ultimately needs enforceable truce rules. Current peace/open-border offers are accepted automatically; this is a prototype, not negotiated diplomacy. The two AI factions begin hostile to one another; player wars require declarations.

## 18. Capture
Capture transfers responsibility and creates temporary occupation: reduced output, unrest and possible building damage. Garrison, rebuilding, amenities and time should support integration. Later decisions include annexing, returning, liberating and restricted time-consuming razing with diplomatic consequences. Resource benefits require intact connections. Current capture reduces population, destroys walls, clears construction and imposes four turns of occupation. Only eligible direct-combat troops capture; ranged/siege units do not.

## 19. Victory
Conquest, science, diplomacy, realm projects and optional score victory are proposed. Scientific victory needs construction and protection, not a single technology click. Diplomatic victory requires an established voting system with safeguards against instant vote buying. Realm victory should expose progress and allow response. Setup should eventually enable/disable conditions and allow sandbox continuation. Current conquest requires no remaining rival cities or units; player defeat requires no own cities or units. Other victory types and continuation are deferred.

## 20. Happiness and stability
Local happiness covers food, taxes, safety, amenities, overcrowding, war and occupation. National stability covers administration, prolonged crises, occupied regions and reforms. Escalate through visible warnings, output penalties, protests and then revolt; avoid unexplained rebellion. Responses include improved supply, lower taxes, amenities, peace or autonomy. Current negative happiness reduces output and prevents growth; population, taxation, gardens, connected spices, occupation and unpaid upkeep contribute. National revolts, war weariness and autonomy are not simulated.

## 21. Civilisations and identity
Premade and custom civilisations share balanced rules. Target choices include names, adjective, citizen name, leader, colour, flag, city names, trait and visual style. Historical identities are thematic presets, not fixed destiny. Cosmetic names/flags remain editable; mechanical reform requires separate costs. Current custom name, city names, three symbolic flags and four fixed trait options are provided. Unique units, historical leader packs and imported flags remain future content.

## 22. Experience and upgrades
Reward meaningful combat and eventually role-appropriate exploration/support actions. Target promotion branches include terrain, sight, healing, defence and siege, with a few meaningful choices. Upgrades retain identity and experience and require technology, resources, gold and friendly facilities. Current combat earns experience; each rank consumes increasing experience and grants +1 strength, capped at three. Warrior -> Spearman -> Rifleman and Horseman -> Knight are explicit upgrade paths. Branching promotions and service histories are not yet present.

## 23. Setup and difficulty
Map type/size, rivals, speed, difficulty, resources, victories, starting era and seed form the target. Advanced options can cover wrapping, hazards and realms. Difficulty should improve AI decisions before relying on disclosed bonuses. Same movement, production and access rules apply. Warn about untested or cramped settings. Current setup supports two map presets, three research/construction speeds, trait and seed on a fixed 48x30 map with three civilisations and standard AI. No promise of 50-civilisation or enormous-map performance.

## 24. Time
A shared calendar slows from decades towards years. Technology determines capabilities; dates do not grant technology. Different civilisations can be at different research stages on the same date. Speed adjusts research/construction and eventually growth while movement is tested separately. Units represent continuing formations and do not die simply because centuries pass. Current schedule uses 40-year turns, then 10-year turns, then yearly turns; no year zero. Calendar schedule is independent of research.

## 25. Accessibility and information
Target adjustable text/interface scale, high contrast, icons plus words, colour-independent ownership, rebinding, reduced animation, concise tooltips and searchable reference. Every unavailable action should state why. Combat and finance previews expose modifiers and costs. Notifications have priorities and history. Short descriptions, generous spacing and consistent labels support readability. Current UI retains resolution scaling, text labels, tile tooltips, construction lock reasons and a chronicle. Full accessibility settings and adjustable fonts are still needed.

## 26. Balance
Test tall vs wide development, peaceful defence, counters, aggression, recovery, starting fairness and management burden. Evaluate construction, upkeep, supply, mobility and combat together. Whole campaigns are necessary; isolated unit tests cannot establish balance. AI simulations expose patterns but do not replace human play. Current numbers are provisional. Verify seed reproducibility and state invariants and use playtesting before adding many more units or increasing scale.

## 27. Art and aesthetics
Consistent 2D, 16-bit-inspired perspective and scale. Distinct terrain silhouettes, resources and improvements; readable stacks, ownership and health. Cities should eventually change by size/era. Close zoom focuses on detail, middle zoom on armies/geography, far zoom on borders/settlements. Brief skippable animation and separately adjustable audio come later. Current original procedural pixel sprites are placeholders, with ownership tint and symbolic flags; final artwork is not integrated or claimed.

## 28. Tutorial
Teach selection, movement, inspection, founding, construction, research, ending turns, resource improvement, contact and defence through a small forgiving scenario. Explain why actions matter. Introduce advanced lessons only when relevant, allow dismissal/reopening, and provide linked reference topics. Current help text and action errors are basic guidance; an interactive objective-driven tutorial remains future work.

## 29. Turn processing
Target a predictable per-civilisation sequence for income/upkeep, food/production/research, completions, healing/movement reset, events, safe queued movement and new orders. Advance the shared date after every civilisation acts. Distinguish required decisions from optional idle units; prevent double-processing and keep UI responsive during AI work. The alpha resolves AI factions, then advances the shared turn and prepares the next player turn. Re-entrant processing is guarded, but large-world asynchronous progress reporting remains future work.

## 30. Combat
Health and strength combine with terrain, fortification, counters, experience and later supply. Close combat damages both sides; ranged retaliation needs explicit eligibility. Show expected damage, modifiers, advance rules and diplomatic consequences before attacks. Modest bounded randomness is a possible future choice; current combat is deterministic for testability. Forest/hill defence, forts, walls, fortification, mounted counters, promotions and unpaid-upkeep penalties affect results. Ranged attacks have two-tile range and basic intervening forest/mountain checks; a complete geometric line-of-sight system and preview are deferred.

## 31. Exploration
Unexplored, remembered and currently visible are distinct information states. Previously observed cities and ownership should eventually store dated snapshots; moving enemies must disappear from view. Sight comes from units, cities and later watchtowers. Civilisations remain unnamed until contact or formal introduction. Current player discovery and tile memory preserve last-seen terrain, improvements, resources, ownership and city names; hidden enemy units are not drawn. Sophisticated AI belief state and shareable intelligence remain planned.

## 32. AI
Separate strategy, empire management and local actions. Account for geography, affordability, escorting, mixed armies, retreat and defence. Personalities bias sensible decisions rather than overriding circumstances. Use discovered information instead of omniscience. Current AI researches, pays upkeep, builds legal units/buildings, expands, improves land and attacks visible targets under the same engine rules. It is a simple heuristic AI with no sophisticated negotiations, strategic memory, escorts or coordinated campaigns. Shared terrain routing now uses explored information for obstacle decisions, but full fog-of-war fairness still needs work.

## 33. Save/load
Preserve worlds, discovery, cities, queues, health, experience, orders, jobs, technologies, balances, agreements, active turn and necessary random state. Provide manual and rotating autosaves, write via temporary replacement, keep a backup and validate on load. Version saves and migrate deliberately. Current v1/v2 migration adds v3 state while preserving old IDs, three rotating autosaves and backup files. Validity, JSON round-trip and migration checks exist; Unity JSON execution must be tested inside Unity.

## 34. Notifications/history
Urgent events, required decisions and routine information need different priorities. Click events to navigate and offer relevant actions. Group routine completions to avoid interrupting large empires. Record wars, captures, losses, shortages and discoveries. Current chronicle keeps 150 entries and shows recent messages. Interactive notifications, grouping and event-location storage are deferred.

## 35. Automation
Governors, worker queues, road-connection tasks, rally points, repeating production, auto-exploration and unit inventories reduce management. Respect manual locks and avoid destroying valuable improvements or entering dangerous areas without configured permission. Current city focus, bounded production queues and persistent movement exist. Workers perform one multi-turn task; job queues, rally points, group movement and advanced automation remain planned.

## 36. Performance and architecture
Measure pathfinding, AI, visibility, trade updates, rendering and saves on increasing world sizes. Cache connectivity and invalidate on relevant changes. Separate rules from graphics so engine checks do not need Unity rendering. Move definitions into editable data as content grows. Current engine is independent of Unity and network connectivity is cached; definitions are central arrays, not external mod data. Rendering culls offscreen tiles. Current pathfinding and several yield/visibility scans still use linear searches; no claim of large-world optimisation or benchmark readiness.
