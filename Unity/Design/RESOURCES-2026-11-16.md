# Map resources from the owner's pack (16 Nov 2026)

Source: `Paradigm_Realm_Map_Resource_Sprites_and_Claude_Prompt (2).zip`, which holds 122 catalogue entries, 122 draft 96 x 96 sprites,
a brief (`Documentation/CLAUDE_IMPLEMENTATION_PROMPT.md`) and reference atlases. The pack itself is not in the repository.

## Architecture (one page)

- **`Scripts/ResourceRegistry.cs`** is the single data registry. It holds one `Def` per resource: stable lower-case id,
  display name, kind (food, luxury or strategic), yields, strategic production %, the technology that reveals it, allowed
  terrains, placement weight, where it goes (Legacy / Land / Coast / Sea / Realm), how it is developed (farm or pasture, mine,
  sea), sprite scale, and art state (Approved / Draft / NeedsRedraw). `World.ResourceNames`, `ResourceFood/Shields/Trade`,
  `ResourceTech`, `IsSeaResource` and `CityResources`' `ResourceKind`/`StrategicPercent` are now views of the registry. Nothing
  else in the game needed to know about the new resources.
- **Saved numbers never change.** A tile saves its resource as a number (`Tile.resource`). Numbers 1-19 are the resources
  the game already had, with the same rules and art. The catalogue is appended from 20 in catalogue order, so old saves load
  unchanged. `ResourceRegistry.Number("copper")` maps ids to numbers.
- **No duplicates.** Wheat, iron, gold, horses, spices, fish, whales, pearls, crabs, oysters, sheep, cattle, pigs and camels
  already existed, so the pack's entries for them were not added again; the existing ones keep their approved art.
- **Placement: `Scripts/ResourcePlacement.cs`.** It runs in new worlds only, after the original placement and after the
  climate remap. It fills only tiles still empty, using its own random stream from the world seed, so the rest of world
  generation is untouched and repeatable. Each resource goes on its listed terrains, weighted by rarity. Coastal ones need
  water beside them; sea ones go on water beside land; dimensional ones go only in the parallel worlds (`PlaceRealmResources`,
  a late-game prize). One resource per tile. Older saves get no new deposits; nothing respawns when research completes.
- **Revealing.** It reuses the existing rule: a resource is drawn and counted only once its technology is known
  (`ResourceKnown`, already applied to drawing, tile info and the city screen). The pack's suggested technologies were mapped
  to the game's real ones (Animal Husbandry and Pottery to Agriculture, Mining to Bronze Working, Forestry to Stone Tools,
  Industrialisation to Industry, Dimensional Surveying to Mysticism, and so on).
- **Development.** This uses the existing worker jobs, with no new improvement types:
  - Minerals: a mine. A known mineral deposit now allows a mine off the hills (`JobReason`).
  - Crops and animals: a farm or pasture.
  - Woodland resources (timber, hardwood, bamboo, rubber and others): a road through the woods. The trees stay, standing in
    for a plantation or hunting track (`ResourceDeveloped`).
  - Sea: a Harbour or linked port, as before.

  Rival workers aim for these through the same rules (`ImproveAI`).
- **Yields and effects** follow the existing system (worked +1 of its kind, developed effects, variety bonus). Strategic
  minerals give production % like Iron. No new recipes, stockpiles or depletion were invented, as the brief asked.
- **Drawing.** `DrawResourceArt` in `RealmGame.cs` draws `Sprites/res-NN` in the tile's ground box at the registry scale. It
  keeps the existing draw order: trees in front cover it, and units and animals stand in front of it. Rendering is still
  batched IMGUI; nothing is added per tile. A catalogue resource with no picture is simply not drawn, never a wrong icon or an
  error.
- **Empire screen.** The resource list shows the original nineteen as before, and catalogue resources only once connected.

## Art review and clean-up

- **001-025** come from the pack's high-resolution isolated sheet. They are clean and used as supplied. Two are not used
  (wheat, and cattle/sheep/pigs where the game already has art).
- **026-122** were extracted from a labelled atlas. They carried atlas text, neighbouring objects and coloured halos baked
  into partly transparent pixels. Clean-up steps:
  1. Background removal in SpriteCook on the drafts, laid out on four grid sheets (4 credits).
  2. Locally, the removal's mask was combined with the original pixel colours, so no colours were repainted.
  3. Pixels that were only halo in the draft were faded out.
  4. Only the main object was kept; stray fragments and edge slivers were dropped.
  5. The object was centred on a bottom-centre anchor by whole pixels, and saved as a square `Sprites/res-NN` using the
     import settings of the existing resource art.
- Every result was reviewed by eye on contact sheets and in the game (`-resourceGallery`).
  - **Installed as draft (82 from the atlas, plus 25 clean):** acceptable at map scale. They are marked `Art.Draft` in the
    registry and shown in amber in the gallery.
  - **Needs redrawing, not placed in worlds (23):** listed below.

### Art QA checklist (pack file names)

Needs redrawing (registered, never placed, no picture installed):

| File | Problem |
|---|---|
| 032_elephants | Halo |
| 039_turtles | Neighbour clipped in |
| 041_cowrie_shells | Halo |
| 046_cotton | Halo |
| 047_flax | Halo |
| 048_wool | Halo |
| 049_furs | Broken shapes |
| 051_silk | Halo |
| 052_honey | Halo |
| 054_dyes | Halo |
| 055_lavender | Halo |
| 061_limestone | Atlas label text |
| 062_marble | Atlas label text |
| 065_kaolin | Atlas label text |
| 089_geothermal | Too faint |
| 093_phosphates | Atlas label text |
| 098_jade | Clipped |
| 099_lapis_lazuli | Clipped |
| 100_obsidian | Clipped |
| 101_amber | Clipped |
| 102_incense | Clipped |
| 113_living_timber | Opaque background |
| 121_worldroot_resin | Opaque background |

Installed but worth a better pass later:

- 034_fish, 036_clams, 104_ambergris: square-ish water or sand base.
- 043_hardwood, 042_timber: a little background between the logs.
- 086_oil: dark pool reads heavy at small size.
- 120_gravity_shards, 112_stariron: busy at small size.

Not used, because the game already has art: 001 wheat, 023 cattle, 024 sheep, 026 pigs, 027 horses, 028 camels, 019 spices,
034 fish (the game's own fish kept), 035 crabs, 037 oysters, 038 pearls, 040 whales, 077 gold, 072 iron.

## Tests and results (16 Nov 2026, Unity 6000.3.25f1, batch mode)

- `RealmTools.Checks`: **PASS** (all engine checks, Unity JSON round trip, legacy save migration). New
  `ContentChecks.ResourceCatalogueChecks` checks:
  - The registry has no problems: no duplicate ids or numbers, every placed resource has art and terrain.
  - Resources 1-19 keep their yields and kinds exactly.
  - Ids map to numbers.
  - Six seeds: every catalogue deposit is on one of its terrains, at sea, or in a parallel world as defined, and at least 25
    kinds appear.
  - Placement repeats from the seed.
  - Copper is hidden until Bronze Working.
  - A mine develops a mineral; a road develops timber without clearing it.
  - Saves accept every registry number and reject one beyond it.
- `GraphicsChecks.Run`: **PASS** (413 assets). The new part requires a square sprite of at least 48 px for every resource with
  art, and art for every resource that can be placed.
- Three existing checks were out of date and were corrected:
  - "one step for each rival": the turn has run in small pieces since 15 Nov.
  - "every resource from 6 up is a sea resource": no longer true.
  - The gateway war scenario in `DimensionChecks` laid its colony on a parallel world whose new deposits moved the site. That
    test world clears its dimensional deposits first.
- `-resourceGallery <folder>`: every resource at real map size on grassland, desert, mountain and sea ground, plus the map in
  play at two zooms. Copies are in `Documentation/Unity-Screenshots-2026-11-16-Resources`.
- Performance (`-frameProbe`, medium world, 80 turns, three timed End Turns):
  - Turn pieces all stay under 55 ms and frames between turns stay at 4-6 ms.
  - With the new resources there are repeatable single-frame hitches during End Turn: 269-287 ms and 429-431 ms, on the
    first and the autosave turn.
  - With the new placement switched off (`-noCatalogue`, a test switch) the first turn still hitches (340 ms) but the
    autosave turn does not (26 ms).
  - The hitches are not inside any timed turn piece. Garbage collection is the likely cause; the incremental collector is
    already on. **Open issue, not fixed.**
- Not done: hand-played games, save and reload through the menu by hand, and very large maps.

## Not built (deliberately)

- New improvement types (camps, quarries, plantations, wells): the existing farm, mine and road stand in.
- Separate pictures for an improved deposit.
- Manufacturing recipes, stockpiles, depletion, logistics and transdimensional trade.
- New trade deals for the new resources (deals still carry the first five, as before).
