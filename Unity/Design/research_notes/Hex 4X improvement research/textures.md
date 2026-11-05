# Textures, terrain art and map readability for a pixel-art hex 4X

Scope note: effort tags are my own estimates for this codebase, not sourced: S = under half a day, M = 1-3 days, L = 1-2 weeks, XL = longer or architectural. Nothing here has been tried in Unity. The aim is technique, not copying any game's art.

## Which techniques do pixel artists and tile engineers recommend for terrain?

### Takeaway
The best-supported advice for this scale is about restraint: controlled hue-shifted ramps, one calm texture beside one busy one, few colours per texture, and little or no dithering used as texture. Autotile theory matters mainly for the supplied sprites, since ground pixels here are already blended procedurally.

### Cited Findings
Palette ramps and hue shifting
- SLYNYRD's method: 9 swatches per ramp, hue shift about +20 degrees per swatch (treated as an upper limit), usually warming as colours brighten. Ramps with no hue shift look flat and combine poorly with other hues — [SLYNYRD Pixelblog 1](https://www.slynyrd.com/blog/2018/1/10/pixelblog-1-color-palettes)
- Saturation peaks near the middle swatch and never reaches 0% or 100%. Avoid high saturation with high brightness, which looks harsh. Set the middle swatch first. Tune by eye because uniform increments often look wrong — [SLYNYRD Pixelblog 1](https://www.slynyrd.com/blog/2018/1/10/pixelblog-1-color-palettes)
- "Straight ramps" that change only brightness and saturation are named as a common beginner mistake — [GameDev.net, Color Palettes](https://gamedev.net/articles/visual-arts/color-palettes-r4964/?do=embed)
- Pixel Parmesan's framing: hue shift is not a separate technique but the result of light meeting local colour. A warm light gives relatively cool shadows, and a cool light gives relatively warm ones. Midtones are usually the most saturated, with highlights and core shadows less so — [Pixel Parmesan, Color Theory](https://pixelparmesan.com/blog/color-theory-for-pixel-artists-its-all-relative)
- Perceived brightness depends on value, saturation and hue together. Yellow reads brighter and blue darker. Each hue's saturation peaks at a different value: yellow high, red mid, blue low — [Pixel Parmesan, Color Theory](https://pixelparmesan.com/blog/color-theory-for-pixel-artists-its-all-relative)
- Limit the saturation and value range within a piece. One suggested weighting is a main colour over most of the image, an accent at roughly 10-20%, and a secondary for the rest. Greys appear as the complement of surrounding colours — [Pixel Parmesan, Color Theory](https://pixelparmesan.com/blog/color-theory-for-pixel-artists-its-all-relative)

Dithering
- "Extraneous dithering often creates visual noise or the unwanted illusion of texture." The author argues dithering was not designed as a texture tool — [Pixel Parmesan, Dithering](https://pixelparmesan.com/blog/dithering-for-pixel-artists)
- Dither patterns become more obvious when zoomed or displayed larger. Dithering softens edges and can break up forms. Some sprites are too small to hold it, and animated sprites are a poor fit — [Pixel Parmesan, Dithering](https://pixelparmesan.com/blog/dithering-for-pixel-artists)
- Transitional dithering works best in higher-resolution, painterly pieces, used sparingly. Larger contrast between the blended colours needs more steps and more resolution — [Pixel Parmesan, Dithering](https://pixelparmesan.com/blog/dithering-for-pixel-artists)

Tile texture and variation
- Pair a busy texture with a calmer one, because two busy textures side by side create visual noise. Use flat areas as negative space to rest the eye. Avoid strongly contrasting colours inside a texture pattern — [SLYNYRD Pixelblog 20](https://slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles)
- Keep visual weight even across a tile, since dominant areas expose the repeat. Do not tile one grass design over a large area; mix flat areas with patches of varying vegetation density. Use only a few colours per texture — [SLYNYRD Pixelblog 20](https://slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles)
- Repeated key shapes should not touch edge to edge, because they merge into noisy clumps. Corner contact is acceptable — [SLYNYRD Pixelblog 20](https://slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles)
- Poisson disc or jittered-grid placement is recommended over high-frequency noise for trees and rocks — [Red Blob Games, Terrain from Noise](https://www.redblobgames.com/maps/terrain-from-noise/)

Banding and clusters
- Banding is described as occurring when adjacent pixel segments of different colours align their endpoints along a shared axis. The suggested fix is to stagger value transitions rather than stack them on one diagonal. I saw this in search excerpts only, from community sources, and could not open the VG Resource wiki (HTTP 403) — [Pixel Joint forum thread](https://pixeljoint.com/Forum/printer_friendly_posts.asp?TID=11303); [VG Resource Dictionary](https://wiki.vg-resource.com/Dictionary)

Autotiling
- Marching squares (corner, two values) needs 16 tiles, or 6 with rotation and mirroring. Blob needs 47 of 256 possible edge-and-corner combinations, or 16 with symmetry. Hex is a valid cell type in the classification, but no hex counts are given — [Boris the Brave, Classification of Tilesets](https://www.boristhebrave.com/2021/11/14/classification-of-tilesets/)
- Blob is the practical subset because the full 256-tile set is generally too many to produce and use — [cr31 Wang tiles, Blob](http://www.cr31.co.uk/stagecast/wang/blob.html)

### Inferences
- A hex has six edges and no separate corner states to resolve, so a binary per-edge neighbour mask has 2^6 = 64 cases. Those reduce to 14 under rotation and 13 with mirroring (my arithmetic, necklace and bracelet counts). This is far cheaper than a square blob set, and it is the right count if any shoreline or transition sprites are ever authored. Because the sprites are foreshortened, rotation cannot simply be reused: north and south edges are visually different, so expect a real need nearer the 64 side unless the masks are generated in code.
- **Terrain ramps table (M).** Move each ground type to one authored 4-6 colour hue-shifted ramp and have noise pick an index, rather than free continuous colour. This gives the "few colours per texture" and cluster character cheaply. It is probably the largest visual gain per hour.
- **Calm the water (S).** The screenshots show the ocean as high-frequency speckle across a large share of the screen, while land is also textured. That is the "two busy textures" case. Lower sea noise amplitude away from coasts and keep detail at shallows and foam. Screenshot judgement, not measurement.
- **Quantised noise and flat patches (S-M).** Threshold noise into 3-4 steps with low-frequency patches of flat colour, so each ground type has rest areas.
- **Ban dither as ground texture (S).** Keep it, if at all, to the blended biome borders. It should also be judged at the highest zoom, since zoom makes patterns more obvious.
- **Blue-noise scenery placement (S-M).** Applies if trees and rocks still use noise thresholds. I did not audit the current placement code.

### Gaps
- Saint11's tutorials exist on [saint11.art](https://saint11.art/blog/pixel-art-tutorials/) as GIFs with no readable text, so I could not verify or quote them. The index filenames suggest Rock, Sand, Water, Tiles and Modular lessons are worth watching by hand.
- No source gave hard rules for 64 x 40 tiles. SLYNYRD calls tiles above 32 x 32 excessive for pixel art in his context, which suggests this game's generated tiles sit outside the size most of this advice was written for.
- Advice is mostly from individual artists and is craft opinion, not tested.

## How do hex-map games handle transitions, elevation and readability of resources and units?

### Takeaway
Wesnoth and Unciv publish their systems: layered terrain with directional overlay images chosen by neighbour rules. For the commercial 3D games I found little primary material, and I found no GDC talk on terrain art. The one direct art-director statement stresses shape and silhouette at zoomed-out distance.

### Cited Findings
Battle for Wesnoth
- Each terrain has a set of 72 x 72 variants with configured probabilities. Each terrain sits in a layer, and terrain in a higher layer draws its transition over the lower — [Wesnoth Tiles Tutorial](https://wiki.wesnoth.org/Tiles_Tutorial)
- Transitions come in 1, 2, 3 and 4-side forms across six directions, 24 images for a full set per pair, though few or no terrains use all of them. Transitions can vary by neighbouring terrain — [Wesnoth Tiles Tutorial](https://wiki.wesnoth.org/Tiles_Tutorial)
- Graphics are rule-driven: a rule is tested at every coordinate with relative tile patterns, a probability, flags that stop later rules re-using a tile, and `rotations` that generate six rotated copies. Images have a `layer` and a `base` pixel for top-to-bottom sorting — [Wesnoth TerrainGraphicsWML](https://wiki.wesnoth.org/TerrainGraphicsWML)
- Multi-hex tiles carry their own transitions, and flat terrain has to be designed to blend into neighbours — [Wesnoth Tiles Tutorial](https://wiki.wesnoth.org/Tiles_Tutorial)
- Art contributors have noted wider transitions can look nice but there is often no room, and that single-hex water can get obliterated. Forum opinion, seen in a search excerpt only — [Wesnoth forum, Tweaking terrain transitions](https://wesnoth.org/forum/viewtopic.php?p=141483)
- Standard hex art can be made by scaling a square texture to 54 x 36, cropping to 72 x 72 and trimming corners so it tiles — [Wesnoth, Turning Square Tiles into Hex](https://wiki.wesnoth.org/Turning_Square_Tiles_into_Hex)

Unciv
- Edge images are drawn when a tile borders another in a specific direction, named by origin filter, destination filter and neighbour direction — [Unciv docs, via search index](https://docsearch.algolia.com/mcp/docs/repo/yairm210/unciv)
- `ruleVariants` set which images a tile combination uses and in what order, such as base, back and front layers. Era variants use a suffix — [Unciv, Creating a custom tileset](https://yairm210.github.io/Unciv/Modders/Creating-a-custom-tileset)

Civilization VI
- Art director Brian Busatti: players zoom out to see the world, so things must be readable from that distance, and different silhouettes make things more readable. The article does not discuss terrain — [Game Developer](https://www.gamedeveloper.com/art/making-i-civilization-vi-i-both-playable-and-beautiful)

Endless Legend and rivers
- Three river approaches are named by a tutorial author: edges (Civ 5), cell to cell (Endless Legend), and water cells (Age of Wonders 3). This is secondhand, not a studio source — [Catlike Coding, Hex Map 6](https://catlikecoding.com/unity/tutorials/hex-map/part-6/)
- Amplitude's design document says a tile can have its own height, terrain type and contents including forests and rivers. Seen as a search excerpt — [Amplitude forum, EL GDD 3](https://community.amplitude-studios.com/amplitude-studios/endless-legend/forums/7-archives/threads/3589-el-gdd-3-the-world?page=4)

Readability overlays
- XAG 102 uses map examples: Forza Horizon 4 markers use a solid fill under a black outline, and For Honor's symbols use black then white outlines so they hold on both light and dark backgrounds — [XAG 102](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102)

### Inferences
- **Layer order for transitions (S-M).** This game already blends borders by noise. The useful thing to borrow is an explicit priority: which terrain "wins" and overlaps. A fixed order (for example water under sand under grass under forest floor) keeps edges consistent. I did not audit how `TerrainPainter` currently resolves this.
- **Elevation by silhouette (M).** With a level ground plane, the owner's choice, relief comes only from scenery. Each raised land feature needs a distinct outline when reduced to black and white. Hills as mounds, mountains as peaks and forest as canopy mass already differ in the screenshots, but in remembered land they merge (see the fourth question).
- **Resource tokens (S).** The current resource icons sit on small grey squares, which read clearly but look foreign to the art. A two-tone outline, as in the XAG examples, could replace the square while keeping the contrast. Worth comparing side by side; do not remove the plate without testing.
- **City plate clash (S).** In `5-world.png` the plate for Azurehold overlaps the unit sprite and is hard to read. Either the units or the plate should yield. Screenshot judgement from a single frame.
- **Optional yields or grid overlay (M).** Since hex outlines appear only at borders by the owner's choice, a held key that temporarily shows the grid and yields would give clarity without changing the default look. This is my suggestion and needs the owner's agreement.

### Gaps
- I found no reliable source on Old World's map art or Mohawk's reasoning. The search returned reviews on UI crowding and a [GDC listing](https://gdcvault.com/play/1028038/My-Elephant-in-the-Room) on design conventions, not art.
- I found no Firaxis or Amplitude primary source on terrain blending pipelines. Civ forum claims about tiles "spilling over" are player speculation, so I have not relied on them.
- Claims about Civ V and Civ VI's specific art direction beyond the Busatti quote are unverified here.

## What from Red Blob Games is relevant: generation quality, biomes, rivers on edges versus through tiles?

### Takeaway
Use elevation and moisture lookups, independent seeds and post-processing for global features, because noise alone gives samey worlds and no real rivers. Edge rivers look better and suit crossing penalties, but this game uses river tiles with boats, and changing that is an expensive decision with gameplay effects.

### Cited Findings
Noise and biomes
- Biomes should come from a lookup on elevation and moisture rather than elevation alone, which gives horizontal bands. Use separate seeds for each field. Thresholds must be tuned per project and noise library — [Red Blob, Terrain from Noise](https://www.redblobgames.com/maps/terrain-from-noise/)
- Stated limits: each location is computed independently, so there are no global constraints or global features such as rivers from peak to ocean, and every area tends to feel the same — [Red Blob, Terrain from Noise](https://www.redblobgames.com/maps/terrain-from-noise/)
- Ridged noise gives sharp ridges, and higher octaves can be weighted by lower-octave elevation so only mountains get fine detail. East-west wrap can use 3D noise on the cosine and sine of an angle — [Red Blob, Terrain from Noise](https://www.redblobgames.com/maps/terrain-from-noise/)
- Patel's polygon generator sets elevation by distance from coast, moisture by distance from fresh water, then redistributes for roughly equal wet and dry regions. Biomes come from an adapted Whittaker table, and he notes each game needs its own rules — [Patel, Polygonal Map Generation](http://theory.stanford.edu/~amitp/game-programming/polygon-map-generation/)

Rivers
- In that generator rivers flow along corner-to-corner edges, which he found looked much nicer than the centre graph, starting at mountain corners and going downhill to the ocean — [Patel, Polygonal Map Generation](http://theory.stanford.edu/~amitp/game-programming/polygon-map-generation/)
- Noisy edges hide cell structure and have "a large impact on the map appearance, especially for rivers and coastlines". Each edge is confined to a quadrilateral of two centres and two corners, so edges can never cross — [Patel, Polygonal Map Generation](http://theory.stanford.edu/~amitp/game-programming/polygon-map-generation/)
- A later experiment grows drainage trees first (source, bend and fork types) with noise biasing the search, and uses Strahler numbers so major rivers draw thicker. The author calls the elevation-fit part an experiment — [Red Blob, Procedural river growing](https://www.redblobgames.com/x/1723-procedural-river-growing/)
- Edge rivers suit meander and division of tiles, and Civ games traditionally charge movement and combat penalties to cross them. Centre rivers are simple and would enable boats. One author reports that Civ games lacked river boats because of this — [T-Machine](https://t-machine.org/index.php/2016/04/18/the-age-old-question-of-civ-games-roads-and-rivers-in-center-of-tiles-or-edges/)
- A Unity tutorial chose cell-to-cell rivers because cell edges were already used by slopes and cliffs — [Catlike Coding, Hex Map 6](https://catlikecoding.com/unity/tutorials/hex-map/part-6/)

Hex geometry
- Offset coordinates suit rectangular maps and wrap easily, but need separate neighbour lists for odd and even rows. Algorithms should convert to axial or cube. Separate x and y scale factors handle squashed hexes, with division on the way back — [Red Blob, Hexagons](https://www.redblobgames.com/grids/hexagons/)

### Inferences
- The codebase treats river as terrain id 7 with a `riverMask` passed to composition (`TerrainRendering.cs`, `PixelArt.cs`), so rivers run through tiles. Moving to edges would touch rules, pathfinding, saves and art: **XL**, and not justified on looks alone. I would not recommend it.
- **Noisy river and coast lines within the current model (M).** Patel's point about visual impact applies without any rule change: confine the wiggle to each tile so that neighbouring tiles still join at fixed edge midpoints. The owner already has meandering rivers, so this is a check, not necessarily new work.
- **River width by flow (M).** Count upstream tiles (Strahler or plain accumulation) and draw wider near the mouth. This reads well and helps tell river from stream. It needs a stored or derived flow value, which may be computable at load with no save change.
- **Regional feel (M-L).** Since the game has plate-tectonic mountains and a climate generator, this may already be covered. Red Blob's "every area feels the same" warning is the thing to check by viewing whole worlds, not single screens.

### Gaps
- I did not find a Red Blob article specifically comparing hex rivers on edges and through tiles. The evidence above comes from polygon maps and third parties.
- The Red Blob hexagon page's spacing formulas are in diagrams that did not extract, so none are quoted here. The existing grid is already working, so they are not needed.

## What are the colour and accessibility rules for telling apart terrain, owners and fog?

### Takeaway
No key information should rely on colour alone, and large elements should reach at least 3:1 against their background. Measured on the current screenshots, remembered land, remembered ocean and unexplored cloud all sit at nearly the same luminance, so the owner's "reads dark" complaint is a real loss of value separation, not only overall dimness.

### Cited Findings
Rules
- Large-scale text and visual elements: at least 3:1 against background. Standard-size elements with important information: 4.5:1. High-contrast mode: 7:1 for all UI elements. Against non-solid backgrounds, measure against the lowest-contrast area — [XAG 102](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102)
- Map elements are named in scope: "all maps that provide key information for gameplay, text symbols, and other elements" should have strong contrast. Outline colours should be configurable or strong against all backgrounds — [XAG 102](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102)
- Content expressed through colour needs at least one more signifier: shape, pattern, iconography or text. If colour is primary, offer presets or free choice of colour. Greyed-out state needs another method as well — [XAG 103](https://learn.microsoft.com/en-gb/gaming/accessibility/xbox-accessibility-guidelines/103)
- Simulators are for internal checks and do not replace testing with players who are colourblind. Color Oracle is listed as a tool — [XAG 103](https://learn.microsoft.com/en-gb/gaming/accessibility/xbox-accessibility-guidelines/103)
- Red or green difficulty is the most common, at around 8-10% of males, and makes both look brownish green. Some colours, most often red, look darker to colourblind players — [Game Accessibility Guidelines](https://gameaccessibilityguidelines.com/ensure-no-essential-information-is-conveyed-by-a-colour-alone/)
- Free colour choice for team colours is the most requested option. Whole-screen recolour filters are a blunt tool and can cause new clashes. For two teams, orange and blue works for the three most common types, and light blue against dark orange helps further. The guideline's position is that reducing reliance on colour should come first — [Game Accessibility Guidelines](https://gameaccessibilityguidelines.com/ensure-no-essential-information-is-conveyed-by-a-colour-alone/)
- Okabe-Ito palette: #E69F00, #56B4E9, #009E73, #F0E442, #0072B2, #D55E00, #CC79A7, #000000. These come from secondary listings; the original is the Okabe and Ito Color Universal Design page, which I did not open — [Concept Viz listing](https://conceptviz.app/ja/blog/okabe-ito-palette-hex-codes-complete-reference); [Rdocumentation, factoextra](https://rdrr.io/cran/factoextra/man/factoextra_palette.html)

Measured baseline for this game (my own measurement, sRGB relative luminance, medians of hand-picked rectangles in `Unity/Documentation/Unity-Screenshots-2026-11-05-HexGrid`; scenery pixels included, one frame each)

| Region | Median luminance |
| --- | --- |
| Visible grass (`1-map.png`) | 0.293 |
| Visible sea (`1-map.png`) | 0.137 |
| Remembered grass (`5-world.png`) | 0.050 |
| Remembered mountains (`5-world.png`) | 0.036 |
| Remembered ocean (`5-world.png`) | 0.028-0.030 |
| Remembered forest (`5-world.png`) | 0.027 |
| Unexplored cloud bank (`1-map.png`) | 0.027 |

Contrast ratios from those medians, using (L1+0.05)/(L2+0.05)
- Visible grass to visible sea: 1.8:1. Visible grass to remembered grass: 3.4:1.
- Remembered grass to remembered ocean: about 1.25:1. Remembered forest or mountains to remembered ocean: about 1.0-1.1:1.
- Remembered ocean to unexplored cloud: about 1.04:1.

Cause, from the code
- Remembered tiles are multiplied by `Remembered` (.58, .61, .68), then get a drifting `MistThin` cloud overlay at 30% alpha of a dark colour (`IsoRenderer.cs` lines 180, 222, 241, 274). The project uses gamma colour space (`ProjectSettings.asset`, `m_ActiveColorSpace: 0`), so the multiply acts on display values, and luminance falls much faster than the 0.6 factor suggests.

### Inferences
- **Fix remembered land first (S to try, M to tune). Likely the highest-value single change.** Do not rely on darkening alone. Fog state can be carried by desaturation and a slightly lifted, compressed value range, so that it stays lighter than the void and keeps land and sea apart. For example, convert toward a cool desaturated tone at roughly 50-60% of original saturation, multiply less, and remove or lighten the veil. Suggested targets to try, mine and not from a standard: remembered land to ocean at least 1.5:1, and remembered land to unexplored cloud at least 2:1. Cloud banks can also be darker or carry shape only, since XAG treats outlines and shape as valid signifiers.
- Remembered and visible should still differ by more than colour, per XAG 103. The drifting veil or a subtle pattern already gives a second signal, so a lighter veil may be better than removing it. Note a moving veil on many tiles is also a repeated draw cost (last question).
- **Greyscale and Color Oracle pass (S).** View every terrain pair, border and fog state in greyscale and in a simulator. This is cheap and catches most problems. The Game Accessibility Guidelines page itself gives no advice on greyscale, so that part is my suggestion.
- **Owner borders (M).** Terrain occupies green, blue, tan and orange, so owner colours will clash with some ground whatever is picked. A two-tone border (a light line beside a dark line) stays visible on both light and dark terrain, following the XAG outline examples. Add a player-chosen colour override and flags on plates as the non-colour signal; flags already exist.
- **Owner palette check (S-M).** With up to 16 civilisations plus natives, colours alone cannot separate everyone, and the Okabe-Ito set has only 8. Pair colour with flag or emblem at city plates and unit bases rather than seeking 16 safe hues. I did not audit the current civilisation colours.
- **High-contrast map mode (M-L).** XAG asks for 7:1 for UI elements in such a mode. A simple version raises border and token contrast and flattens ground texture. Optional, and worth doing after the above.
- 1.8:1 for visible grass against sea is acceptable to my eye because hue, texture and foam also separate them. XAG's 3:1 is best applied to gameplay signals (borders, tokens, selection) rather than every pair of terrains, as XAG exempts decorative content. That reading is mine.

### Gaps
- No source sets numeric contrast targets between terrain types or fog states. The targets above are starting points to tune by eye.
- The measurements come from one screenshot pair and hand-picked rectangles, not gameplay across many maps, and were not taken over other zoom levels.
- Colourblind simulation of the actual screenshots was not run.

## Is IMGUI texture drawing a bottleneck compared with a mesh or sprite atlas for thousands of tiles?

### Takeaway
Unity's documentation says IMGUI is not intended for normal in-game UI and may run several times per frame, and that each mesh by default needs its own draw call, with batching and instancing as the remedy. But I found no official figure for `GUI.DrawTexture` cost, so whether it is a bottleneck here must be measured in the Profiler, not assumed.

### Cited Findings
- "IMGUI is not generally intended to be used for normal in-game user interfaces that players might use and interact with." It is described as mainly a tool for programmers — [Unity 6000.3 Manual, IMGUI](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/ui-imgui/guiscripting-guide)
- "Your OnGUI implementation might be called several times per frame (one call per event)." — [Unity Scripting API, OnGUI](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/monobehaviour/ongui)
- By default each mesh needs its own draw call. Updating render state is the most expensive CPU step. Grouping objects that share render state lets Unity update state less often. Named methods are GPU instancing, the SRP Batcher and batching — [Unity 6000.3 Manual, Optimizing draw calls](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/reduce-draw-calls/optimizing-draw-calls)
- A Unity hex tutorial builds terrain per chunk, with separate terrain and river meshes per chunk, and Unity skips empty meshes — [Catlike Coding, Hex Map 6](https://catlikecoding.com/unity/tutorials/hex-map/part-6/)
- Community reports say OnGUI calls can far outnumber Update calls, with rough per-control draw-call figures. These are old and anecdotal — [Unity Discussions](https://discussions.unity.com/t/why-is-ongui-called-significantly-more-than-update-and-fixedupdate/83623); [Unity forum, 2018](https://forum.unity.com/threads/worst-performance-yet-of-ongui-in-2018-2-9-build-script-debugging.583240/)

Local facts
- `IsoRenderer.cs` and `RealmGame.cs` both test `Event.current.type==EventType.Repaint` at points (lines 165 and 253), so at least part of the map avoids non-repaint work. I did not confirm this covers every draw path.
- Each tile draw uses `GUI.DrawTexture`, with per-draw tint via `GUI.color`. Remembered and unexplored tiles add a cloud draw each (`IsoRenderer.cs` lines 219-275). CLAUDE.md records a shared picture cache with a per-frame paint budget, and that it is unmeasured. Turn times recorded there cover rules, not rendering.

### Inferences
- A rough visible count at the current scale: a 1336 x 740 map area over 64 x 30 pixel tiles is about 500 tiles, or roughly 1,000-1,500 textured quads with scenery and fog. Zoomed out, multiply by the square of the zoom factor. Those quads are mostly different textures (each composed tile has its own picture), which is the case batching handles worst. So the concern is plausible, mostly when zoomed out, but unproven.
- **Measure first (S).** Use the Profiler and Frame Debugger on a Massive map at minimum zoom, in a build, with fog on. Record CPU time in OnGUI, draw call count and texture upload time from the paint budget. Decide from that.
- **Atlas the tile pictures (M-L, if measured).** Composing tile pictures into a few shared pages and drawing with `GUI.DrawTextureWithTexCoords` keeps IMGUI. Whether this reduces draw calls under IMGUI is not stated by Unity, so test it before investing.
- **Map as chunked mesh (L-XL, if measured).** Bake ground into chunk textures or meshes per world region and draw them outside IMGUI in the camera pass, with the HUD staying in IMGUI. This is the documented, mainstream direction and would also help the fog: a per-tile state texture and a shader could replace thousands of tinted redraws. It is a major refactor of input, ordering of scenery and picking, so only do it with evidence. Scenery and units must still sort by row.
- **Fog cheaper meanwhile (S-M).** Merge the veil clouds into fewer, larger draws or use a single overlay. This also fits the remembered-land fix above.
- Art recommendations in this note do not depend on this question: ramps, water calming and fog colour all work with the current drawing.

### Gaps
- I found no Unity documentation giving a draw-call cost or batching behaviour for `GUI.DrawTexture`, or a recommended sprite limit. The linked "Choose a method for optimizing draw calls" page was not read.
- No profile of this game's rendering exists, so the visible-count estimate above is arithmetic only. I did not check the real viewport or zoom limits.
- I did not verify IMGUI's internal batching in Unity 6, and community figures may be out of date.
