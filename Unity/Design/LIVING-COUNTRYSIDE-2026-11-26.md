# The living countryside: first pass, the river valley (26 Nov 2026)

Owner brief: make the painted hex map feel alive, judged on one river valley before anything wider. Built in `AmbientLife.cs` (the gestures), `AmbientPaths.cs` (the painted roads and rivers as lines), `Valley.cs` (the demonstration), `AmbientChecks.cs`. No save version change: `World.valleyMill` loads as -1.

## Seeing it

- `ParadigmRealm.exe -valley` opens the valley; `-valley awaken` with its gateway stirring. It is seed 26035, dressed the same way every time. Zoom in fully.
- In any game: zoom to 64 or closer over your own land. Settings > **Living countryside** turns it off; so does `-noAmbient`.
- `-valleyShot <folder>` takes the pictures, runs the tests below and saves the frames of a clip.

## What was built

| Design | How it works | Where |
|---|---|---|
| The broken-silver current | Up to three tapered marks just downstream of a bridge pier, on the river's own centre line; they stretch, break in two and go. Only where the river's outflow is known (`HexRiverLine` is null otherwise) | Every bridged river tile in sight; also below the valley's mill |
| The travelling gust | One wind from the place on the map and the clock (`GustPhase`). Ripe wheat (field picture 0) answers in a narrow band; groves lean half a second later, in slices, trunks still; leaf tufts last | Every ripe field and grassland grove in sight |
| The miller's road | A cart from a worked field with a road (or the mill) to its city, at most 4 tiles, along the painted road (`HexRoadArm`); loaded out, 0.9 s at the town's edge, gone, then back empty | Your own cities, one cart each, two from size 8, six in all |
| The occupied hearth | Smoke from the roof peaks found in the settlement's own picture (`RoofPeaks`); 1 to 4 hearths by size, uneven gaps; a city founded in play sends its first wisp at once | Every settlement in sight |
| The wrong-way leaves | Two or three leaves drift into a shut gateway against the wind every 21 to 27 s | Every found, shut gateway in sight |
| The waking gateway | While your own people are opening it (`GateStudy.approach`): the reflection rises on water below it, 0.85 s later colour reaches the stone; inland the leaves come every 8 s | Gateways you are opening; rivals' work is never shown |

Sources of truth, all existing: `Worked`, `Disorder`, `occupation`, `RoadConnects`, `Bridged`, `RiverDown`, `Visible`, `gatesFound`, `GateOpen`, `StudyOf`. Nothing here writes to the game.

**Order and cover.** Wheat, marks, groves and smoke are drawn with their tile, so nearer roofs and trees cover them. A cart is drawn when its row of tiles is down, then its town (or the mill) is laid over it again. Units and plates are drawn after all of it. The plate of a gateway that stands over water is now above the arch, so it does not hide the reflection.

**Zoom.** 128 and 96: everything. 64: no leaf tufts, fewer stalks. 48: smoke only, one hearth a town. 32: nothing.

**Fog.** Only tiles in sight; carts need every tile of their road in sight.

**Hexagon maps only.** Old square saves are unchanged.

## The mill

The game has no mill. The valley's is decoration (`World.valleyMill`), but it works only while its city works that land and is not in disorder or occupied; then the wheel runs down and the cart stops. In ordinary games the cart comes from a worked farm with a road.

## Art (SpriteCook, 168 credits, 519 left; originals in `Design/SpriteCook-River-Valley-2026-11-26`, game files in `Resources/Visuals/Ambient`)

All finished art, made from a crop of the map as reference, cut to two picture pixels a map unit (units use the same) and powers of two.

| Set | Frames | Canvas | Pivot | Note |
|---|---|---|---|---|
| `mill` | 1 | 64 | axle (43.8, 49.3) | static wall; wheel separate |
| `mill-wheel-0..7` | 8 | 32 | centre (16, 16) | SpriteCook wheel turned 5.6 degrees a frame at the wall's angle by a script, so axle and rim cannot jitter; the ochre highlight stays put and paddles pass through it. The first wheel read as a ship's wheel at play size and was made again |
| `cart-loaded-0..3`, `cart-empty-0..3` | 4 + 4 | 32 | foot 89% down, middle | empty is an edit of loaded on the same canvas; the wheel is turned in place. Drawn facing right and mirrored for the left; the light on it then comes from the wrong side, which cannot be seen at play size |
| `wheat-0..7` | 8 | 32 | bottom middle | dark outline softened to straw, tips paled through the bend |
| `branch-0..5` | 6 | 16 | middle | |
| `leaf-0`, `leaf-1` | 2 | 8 | middle | |
| `smoke-0..7` | 8 | 64 | bottom middle | keeps soft alpha |

Interim, drawn in code rather than authored: the silver marks (`Stroke`), the reflection (the owner's open-gate picture, upside down on the water), the crowns' lean.

## Tested in a running build (`-valleyShot`, 1280 x 800)

Passed: every gesture seen; wheel turns while the land is worked; at 48 nothing but smoke, at 32 nothing; a cart whose road is cut fades and none replaces it until the road is back; with work stopped the carts go and the wheel runs down, and both start again; nothing moves on remembered land; ending a turn doubles no cart; saving and loading keeps the mill, doubles nothing and lights no hearth anew; a new city's first wisp within two seconds; the reflection comes before the colour. The cart's path was checked against the painted road from the frames of the clip.

Speed, map drawing only, same view with and without (this PC): at zoom 64 2.27 ms a frame without and 2.30 with; at 128 1.04 and 1.12 (an earlier run: 2.23 and 2.37; 1.05 and 1.23). Full engine checks and graphics checks pass in Unity.

Pictures: `Documentation/River-Valley-2026-11-26`: the same valley with and without at 64, 96 and 128, and `valley-clip.gif` (fixed camera, 23 s: the cart out loaded at 11 s, gone at 16, back empty at 19, the gust over both fields at 18 to 20).

## Not done, or to be judged by eye

- Hand play on a real game, and at other screen sizes.
- Wind on farmstead trees.
- Wakes at rocks (the map has no rocks in rivers) and at mill wheels outside the valley.
- Carts on square maps; carts between cities.

## Every other tile (owner request, 25 Nov 2026; `AmbientTerrain.cs`)

| Tile | What moves |
|---|---|
| Grassland, hills, savanna, dry steppe, tundra, river banks | Grass bends as the one gust passes and settles; butterflies on some meadows, a circling hawk over some hills and peaks, a tumbling bush on some steppe |
| Forest, jungle, arctic forest, savanna trees, mangrove, hill woods | The trees lean row by row; forest leaves fall, jungle mist rises, snow slips from arctic branches, small flocks cross |
| Desert, desolate wasteland | Sand or dust runs before the wind; now and then a dust devil |
| Ice sheet, glacier, tundra | Drifting snow and glints |
| Mountains | Snow streams off the peak; a cloud passes beneath it |
| Volcanic terrain | Smoke (the only mark kept at zoom 48), embers, a breathing glow |
| Ocean | Crests rise and fall back; glints; gulls here and there |
| Lake, coral reef, mangrove | Ripples, glints, cat's-paws under the gust, small fish on reefs |
| River | Light going down the stream on its own line (rivers without a known outflow: glints) |
| Bog | Bubbles that swell and burst, low mist, reeds that lean |
| Rocky coastline | Spray on the edges that meet water |
| Farms | All three field pictures answer the gust (wheat, green rows, pale rows) |
| Village camps | Thin smoke from the fire |

The marks are drawn in code; they are not authored art. Trees are the map's own pictures. Birds show only over land in sight. Test and pictures: `-lifeShot <folder>`.

Measured on this PC, map drawing only: the valley at zoom 64 2.28 ms a frame without any of it and 2.66 with, at 128 1.11 and 1.44; a worst case with every terrain in sight across the whole screen, 2.61 and 4.22 at 64 (632 marks), 0.97 and 1.90 at 128. Pictures and a clip: `Documentation/World-Page-And-Movement-2026-11-25`.
