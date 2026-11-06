# Terrain art for a pixel-art hex map: second round, with measurements and proposed ramps

Labels on every claim: **read** = I opened the page or file, though through a summarising fetch tool, so wording is close but not guaranteed verbatim. **snippet** = search result text only. **secondary** = someone reporting on the primary source. **measured here** = computed by me this session. **mine** = my suggestion or judgement.

Method for "measured here": I compiled the project's own `BasePixel` (lines 121-239 of `Unity/Assets/Scripts/PixelArt.cs`) unchanged into a scratch harness, rendered 320 x 160 px of each terrain, and measured that and rectangles of `Unity/Documentation/Unity-Screenshots-2026-11-05-HexGrid` (1-map.png, 5-world.png). No project file except this note was changed. Nothing here has run in Unity.

Definitions: **orphan** = pixel with no same-coloured 4-neighbour. **Cluster** = 4-connected same-colour area. **N90** = how many colours cover 90% of pixels. **CR** = WCAG contrast (L1+.05)/(L2+.05) on sRGB relative luminance. **L*** = CIE lightness.

Limits you should know first:
- I used the whole 60-call budget. I generated a side-by-side picture of the proposal but **never looked at it**. Every number below says the proposal is calmer; none says it is prettier. Prototype metrics may also reflect over-flattening (see the acceptance bands).
- The picture is at `C:\Users\Matth\AppData\Local\Temp\claude\37cdc78b320e14b6\a3b6113e-5299-463a-9abe-c0038f5399dd\scratchpad\m\sheet.png`. It is in a temp folder, so copy it if wanted. Each row reads current, proposed, current remembered (full veil), proposed remembered (candidate A). Left block top to bottom: grass, forest, jungle, savanna, steppe, desert, mountain, rock, tundra, snow forest, ocean. Right block: reef, lake, bog, waste, ice, glacier, volcanic, mangrove. Its forest row uses the first forest ramp, not the revised one below.
- Not read for lack of budget: SLYNYRD Pixelblog 2 (Texture), 13 (Rocks), 43 (Top Down Tiles 2), 41/54 (Isometric), Pixel Parmesan's isometric article, any Paul Robertson interview.

## What do the measurements say about the current terrain?

### Takeaway
The first round's "limit each ground to a small colour set" is only half the problem. Most terrains already use 5-6 colours and still have about a third of their pixels as orphans. The fault is per-pixel jitter, not palette size. Alpha overlays then multiply the colours again on screen, so ramps alone will not survive to the display.

### Cited Findings
Generator output, no scenery or overlays — measured here, from [PixelArt.cs](../../../Assets/Scripts/PixelArt.cs):

| Terrain | Colours | N90 | Median lum | p95:p5 CR | Orphans | Mean cluster px | Adjacent pairs differing | Mean adjacent ΔL* |
|---|---|---|---|---|---|---|---|---|
| Grassland | 363 | 71 | .293 | 1.87 | 34% | 2.4 | 60% | 3.7 |
| Forest floor | 5 | 4 | .115 | 2.56 | 34% | 2.2 | 70% | 8.3 |
| Desert | 5 | 3 | .538 | 1.67 | 11% | 5.0 | 53% | 5.0 |
| Ocean / reef | 7 | 3 | .143 | 2.50 | 17% | 4.7 | 44% | 8.3 |
| Lake | 7 | 3 | .163 | 2.02 | 17% | 4.7 | 44% | 6.7 |
| Mountain base | 5 | 3 | .203 | 1.85 | 31% | 2.4 | 68% | 7.6 |
| Tundra | 6 | 3 | .344 | 2.93 | 34% | 2.3 | 69% | 11.0 |
| Bog | 6 | 4 | .087 | 1.80 | 29% | 2.6 | 62% | 5.5 |
| Wasteland | 10 | 4 | .145 | 2.81 | 36% | 2.1 | 71% | 9.6 |
| Jungle | 5 | 4 | .097 | 3.74 | 34% | 2.2 | 70% | 8.9 |
| Snow forest | 6 | 3 | .545 | 2.22 | 33% | 2.3 | 69% | 9.1 |
| Ice | 6 | 3 | .734 | 1.65 | 34% | 2.3 | 70% | 6.3 |
| Glacier | 6 | 3 | .601 | 2.11 | 34% | 2.2 | 70% | 8.9 |
| Savanna | 5 | 4 | .442 | 2.32 | 34% | 2.2 | 70% | 7.9 |
| Volcanic | 6 | 3 | .036 | 2.87 | 31% | 2.4 | 69% | 7.7 |
| Rocky coast | 8 | 6 | .269 | 3.30 | 29% | 2.4 | 67% | 10.0 |
| Mangrove | 5 | 4 | .125 | 2.53 | 34% | 2.2 | 70% | 7.2 |
| Steppe | 282 | 39 | .406 | 1.79 | 22% | 3.7 | 48% | 3.0 |

Same pictures on screen, hand-picked rectangles, scenery mostly excluded — measured here:

| Region (file, x,y,w,h) | Colours | N90 | Median lum | Orphans | Mean cluster |
|---|---|---|---|---|---|
| Visible sea (1-map, 620,275,200,30) | 189 | 46 | .129 | 19% | 3.6 |
| Visible grass (1-map, 560,470,50,25) | 363 | 238 | .293 | 47% | 1.8 |
| Savanna (1-map, 840,360,30,30) | 230 | 140 | .419 | 34% | 2.0 |
| Cloud bank (1-map, 300,120,180,120) | 735 | 185 | .026 | 18% | 3.4 |
| Remembered ocean (5-world, 930,420,170,130) | 5428 | 3218 | .029 | 49% | 1.5 |
| Remembered grass (5-world, 240,180,50,30) | 878 | 728 | .044 | 59% | 1.4 |
| Remembered mountains (5-world, 510,500,90,70) | 2830 | 2200 | .031 | 53% | 1.4 |
| Remembered forest (5-world, 720,480,100,40) | 2390 | 1990 | .024 | 56% | 1.4 |
| Whole remembered area (5-world, 160,130,330,200) | 17144 | 10544 | .038 | 61% | 1.3 |

Causes in the code — read, [PixelArt.cs](../../../Assets/Scripts/PixelArt.cs):
- Generic land: `val=q+(Bay(x,y)-.5)*.30`, with `q` holding 25% per-pixel hash and 30% noise at scale 1.8 (lines 220-221). So a Bayer term and a hash term both decide the colour of each pixel independently.
- Speckle: lines 224-229 add 4.5% lone ramp-end pixels. Line 235 adds 3% pure white pixels to tundra, snow and ice. Tundra's p95 of .775 against a median of .344 comes from this (inference from the two numbers).
- Grass and steppe reach 363 and 282 colours because of `MixC` tints (lines 202, 204, 215-217).
- Sea: lines 185-191 re-roll white marks with `Hsh(x+frame*3,...)` each frame.
- Coral reef (terrain 12) uses the same ramp as ocean: reef to ocean CR is 1.00 — measured here. The name comes from `World.TerrainNames` in World.cs.

Hue behaviour of current ramps, end to end — measured here:
- Well shifted: Grass 122° to 74°, Forest 141° to 98°, Jungle 153° to 113°.
- Nearly straight: Mountain 42° to 44°, Savanna 45° to 48°, Steppe 66° to 57°, Ice 204° to 192°, Glacier 208° to 200°, Desert 37° to 43°.
- SLYNYRD's method calls straight ramps flat ([Pixelblog 1, via first-round note](../Hex%204X%20improvement%20research/textures.md)).

Calibration against published palettes, colours from the Lospec JSON — read: [Apollo by AdamCYounis](https://lospec.com/palette-list/apollo.json), [Endesga 32 by ENDESGA](https://lospec.com/palette-list/endesga-32.json). Figures — measured here:
| Published ramp | Hue path | L* path | Step CR |
|---|---|---|---|
| Apollo greens | 166 131 105 90 78 68 | 19 32 49 63 77 85 | 1.58 1.84 1.64 1.53 1.25 |
| Apollo blues | 224 218 214 204 193 178 | 13 24 39 57 73 84 | 1.42 1.72 1.89 1.68 1.39 |
| Apollo sand | 348 7 22 26 30 39 | 22 36 55 65 76 86 | 1.66 1.96 1.39 1.41 1.33 |
| Endesga greens | 183 151 128 109 | 23 35 51 72 | 1.53 1.81 2.01 |

Those palettes step much harder (CR 1.4-2.0) than the ground colours proposed below (1.25-1.45). That fits a general-purpose palette, which has to cover whole sprites with few colours (mine).

### Inferences
- **Fix the jitter, not only the palette (mine).** Forest floor already has 5 colours and is as noisy as anything. A colour-limit-only change would leave 34% orphans.
- **Alpha overlays break palettes.** Remembered grass has 878 colours in a 1500-pixel patch. Candidates are the 128 x 128 soft cloud shadow sheet (`CloudShadows`, alpha 0-42), the mist puffs and the owner wash. Mine: step those alphas into 2-3 flat levels with hard edges, or the ramp work will not reach the screen.
- **Non-integer zoom is a likely extra cause, unverified.** `5-world.png` appears to be at a different zoom, and tiles use point filtering (`IsoRenderer.cs` line 31). Point-sampling at non-integer scale drops or doubles pixels, which would raise orphans. Test by measuring the same area at zoom 1, 2 and any fractional step. If confirmed, snap zoom to steps that keep pixels even.

### Gaps
- One frame of each screenshot, hand-picked rectangles, some scenery contamination. `MountainPixel`, `VolcanoPixel` and the hill relief were not in the harness, so the mountain row is the base ramp only.

## What ramps do I propose for each terrain? (my suggestions, not from any source)

### Takeaway
Five colours per terrain: three core tones that noise chooses between, plus one shadow and one highlight accent used only as short dashes. In greyscale, sea moves further from grass (CR 1.77 to 2.23), and reef and lake separate from ocean. Dark woodland stays close to sea and relies on hue.

### Cited Findings
All figures measured here, using the harness. Index 2 is the base tone. **All RGB values are my suggestions.**

| Terrain | 0 shadow accent | 1 dark | 2 base | 3 light | 4 highlight accent | L* | Step CR | Hue path |
|---|---|---|---|---|---|---|---|---|
| Grass, hill, river meadow | 52,104,68 | 74,134,62 | 98,156,66 | 126,176,76 | 170,202,98 | 40 51 59 67 77 | 1.49 1.33 1.29 1.38 | 138 110 99 90 78 |
| Forest floor (revised, hand-computed) | 34,68,58 | 46,88,58 | 62,110,60 | 82,128,66 | 112,150,76 | 26 34 42 49 58 | 1.32 1.36 1.29 1.36 | 162 137 118 105 91 |
| Jungle floor | 18,52,50 | 26,70,52 | 38,88,52 | 56,108,54 | 88,132,62 | 19 26 33 41 51 | 1.26 1.28 1.33 1.42 | 176 155 137 118 98 |
| Mangrove | 28,62,60 | 40,84,64 | 58,106,68 | 82,128,76 | 60,120,120 (wet) | 24 32 41 49 47 | 1.35 1.37 1.37 1.10 | 176 153 133 113 180 |
| Bog | 38,50,44 | 54,68,46 | 74,88,52 | 96,106,62 | 62,96,92 (wet) | 20 27 35 43 38 | 1.28 1.35 1.33 1.20 | 150 98 83 74 173 |
| Savanna | 140,116,62 | 172,146,68 | 198,172,82 | 220,196,106 | 238,218,140 | 50 61 71 79 87 | 1.48 1.36 1.29 1.24 | 42 45 47 47 48 |
| Steppe | 112,120,70 | 142,148,80 | 168,172,94 | 192,194,116 | 216,212,144 | 49 59 68 77 84 | 1.46 1.34 1.28 1.23 | 70 65 63 62 57 |
| Desert | 176,134,92 | 204,164,108 | 224,190,128 | 238,210,150 | 248,230,182 | 59 70 79 85 92 | 1.42 1.30 1.21 1.19 | 30 35 39 41 44 |
| Mountain base | 72,66,72 | 98,88,84 | 124,112,98 | 150,136,114 | 182,168,142 | 29 38 48 57 69 | 1.42 1.43 1.39 1.48 | 300 17 32 37 39 |
| Rock | 64,62,78 | 90,86,96 | 118,112,114 | 148,140,132 | 184,174,158 | 27 37 48 59 72 | 1.45 1.48 1.46 1.51 | 248 264 340 30 37 |
| Wasteland | 72,56,60 | 96,78,68 | 122,102,82 | 148,128,100 | 178,156,124 | 25 35 45 55 66 | 1.40 1.44 1.44 1.44 | 345 21 30 35 36 |
| Volcanic | 28,24,36 | 44,36,46 | 62,48,52 | 86,62,58 | 190,70,30 (ember) | 9 15 22 29 46 | 1.16 1.20 1.28 1.90 | 260 288 343 9 15 |
| Tundra | 92,108,104 | 118,134,118 | 146,160,136 | 176,186,162 | 214,222,206 | 44 54 64 74 87 | 1.43 1.40 1.37 1.46 | 165 120 95 85 90 |
| Snow forest | 116,138,160 | 146,168,184 | 178,198,208 | 208,222,228 | 238,244,246 | 57 68 79 88 96 | 1.45 1.40 1.28 1.24 | 210 205 200 198 195 |
| Ice | 146,182,214 | 174,204,226 | 202,224,238 | 226,240,246 | 248,252,252 | 73 81 88 94 99 | 1.27 1.23 1.17 1.13 | 208 205 203 198 180 |
| Glacier | 104,144,190 | 136,174,212 | 172,206,232 | 206,228,242 | 236,248,252 | 59 70 81 89 97 | 1.43 1.41 1.26 1.21 | 212 210 206 203 195 |
| Ocean | 18,50,116 | 22,66,140 | 28,84,164 | 44,112,186 | 120,180,216 | 23 29 36 47 71 | 1.26 1.30 1.45 2.25 | 220 218 215 211 203 |
| Coral reef / shallows | 26,92,160 | 34,116,180 | 48,140,194 | 84,170,206 | 170,222,232 | 39 47 55 66 85 | 1.37 1.34 1.42 1.79 | 210 206 202 198 190 |
| Lake | 24,74,100 | 32,96,120 | 44,116,138 | 66,142,156 | 126,190,190 | 29 38 45 55 73 | 1.36 1.32 1.40 1.79 | 201 196 194 189 180 |

Between terrains, median luminance CR. "First pass" is what the harness measured. "With revised forest" is my hand arithmetic, taking the forest median as .125, the luminance of its base tone. Everything else in the table is harness output.

| Pair | Current | First pass | With revised forest |
|---|---|---|---|
| Grass / ocean | 1.77 | 2.23 | 2.23 |
| Forest / ocean | 1.17 | **1.02** | 1.22 |
| Grass / forest | 2.08 | 2.27 | 1.82 |
| Jungle / forest | 1.12 | 1.11 | 1.39 |
| Bog / forest | 1.20 | 1.02 | 1.28 |
| Desert / grass | 1.72 | 1.86 | 1.86 |
| Savanna / grass | 1.43 | 1.48 | 1.48 |
| Steppe / grass | 1.33 | 1.37 | 1.37 |
| Steppe / savanna | 1.08 | 1.08 | 1.08 |
| Tundra / steppe | 1.16 | 1.15 | 1.15 |
| Lake / ocean | 1.10 | 1.39 | 1.39 |
| Reef / ocean | 1.00 | 1.98 | 1.98 |
| Snow forest / ice | 1.32 | 1.30 | 1.30 |
| Ice / glacier | 1.21 | 1.21 | 1.21 |

### Inferences
- **The first-pass forest ramp made forest and ocean tie at 1.02, so I raised it.** Use the revised row. The revised row was not re-run through the texture harness.
- **Known weakness: steppe, savanna and tundra stay close in value.** They rely on hue and saturation (S about 45%, 59%, 15%). A possible steppe change: 104,114,68 / 128,138,76 / 150,160,92 / 176,182,112 / 206,204,140. By hand that gives steppe/savanna about 1.26 and steppe/grass about 1.18, but tundra/steppe falls to about 1.02. Try it only if a greyscale check shows steppe and savanna merging.
- **Savanna, steppe and desert are still only lightly hue-shifted (mine).** Lower the first two steps towards orange-brown if they look flat. Grass and forest's first step exceeds SLYNYRD's upper limit of 20° at the shadow end (28° and 25°), which is acceptable for a rarely used accent but worth checking.
- **The ocean is darker than now** (median .093 against .143). That helps land stand out but may run against the owner's taste for bright "gallery" seas and his "reads dark" complaint. Middle ground if he objects: base 30,94,176.

### Gaps
- No source sets numbers for terrain-to-terrain value contrast. All targets are mine.
- Colourblind simulation not run.

## Does per-pixel noise conflict with cluster rules, and how do practitioners avoid it?

### Takeaway
Yes. Independent per-pixel decisions are what cluster guidance warns against. The fix that worked in my prototype keeps per-pixel evaluation but makes the decision per small brick and per low-frequency field, so single-pixel choices cannot occur. Blue noise is the wrong tool for ground colour and the right one for scattering.

### Cited Findings
- Pixels of the same colour should group into solid shapes, a lone pixel usually being meaningless. Avoid independent pixels that add nothing, except specular highlights or essential small details. Dithering is for tapering edges, not covering large areas, and random dithering is generally avoided as noise — read, [cure, Pixel Joint tutorial](https://pixeljoint.com/forum/forum_posts.asp?TID=11299).
- Banding: avoid neighbouring pixels lining up on the same x or y, as that exposes the grid — read, [same](https://pixeljoint.com/forum/forum_posts.asp?TID=11299).
- Bayer is cheap, deterministic and per-pixel, with visible structure. Blue noise has no strong clusters or voids, looks organic, tiles, and is as fast to apply, the generation being the expensive part (about a minute for 64 x 64). Error diffusion is sequential. Dithering in sRGB rather than linear looks too bright — read, [Surma, Ditherpunk](https://surma.dev/things/ditherpunk/).
- Obra Dinn's Bayer dithering flickered on camera moves, and a variant called blue noise held up better under scaling and video compression, with stability work involving remapping onto a sphere — secondary/snippet, [Alan Zucconi](https://www.alanzucconi.com/?p=10365), [Metafilter](https://www.Metafilter.com/170872/man-this-dithering-is-stable-as-shit-total-magic-going-on-here). Pope's own devlog refused automated reading (HTTP 403), so I did not read it.
- Procedural practitioners commonly snap coordinates to a coarser grid, threshold noise into fixed colours, and offer Bayer shading modes — snippet, [Godot Shaders terrain](https://godotshaders.com/shader/2d-procedural-terrain-can-be-pixelart/), [Easy Pixel Planet](https://godotshaders.com/shader/easy-pixel-planet/), [Pixel Planet Creator](https://plasmator-games.itch.io/pixel-planet-creator). These are hobby and tool pages, not authorities. I found no written account by a named procedural pixel artist of avoiding orphans.
- Saint11's tutorial index is readable text, but the lessons (Water, Rock, Sand, Tiles) are GIFs, so I could not read them — read, [saint11.art](https://saint11.art/blog/pixel-art-tutorials/).

Prototype result — measured here, in harness only, using the proposed ramps and the code below:

| Terrain | Orphans now → proposed | Mean cluster | Pairs differing | Mean ΔL* | N90 |
|---|---|---|---|---|---|
| Grass | 34% → 0% | 2.4 → 36 | 60% → 17% | 3.7 → 2.4 | 71 → 3 |
| Forest floor (first-pass ramp) | 34% → 0% | 2.2 → 50 | 70% → 15% | 8.3 → 1.5 | 4 → 3 |
| Desert | 11% → 0% | 5.0 → 38 | 53% → 21% | 5.0 → 2.0 | 3 → 3 |
| Tundra | 34% → 0% | 2.3 → 47 | 69% → 16% | 11.0 → 2.3 | 3 → 3 |
| Savanna | 34% → 0% | 2.2 → 37 | 70% → 17% | 7.9 → 2.4 | 4 → 3 |
| Steppe | 22% → 0% | 3.7 → 43 | 48% → 16% | 3.0 → 2.1 | 39 → 3 |
| Ocean | 17% → 0% | 4.7 → 110 | 44% → 6% | 8.3 → 0.6 | 3 → 2 |

### Inferences
- **Blue noise as a ground threshold would not help (mine).** Its whole property is avoiding clumps, so at low threshold levels it scatters isolated pixels. Use it, or jittered grids as below, for placing tufts and pebbles only.
- **Obra Dinn's flicker problem does not apply to the ground (mine).** `GroundPixel` works in world pixels (`gx,gy`), so patterns move with the camera. The risk here is zoom scaling instead.
- **Keep Bayer at biome borders (mine).** Line 1124's 3.6 px blend is the taper use that cure allows. Ban it from fills: remove `Bay` from lines 200, 213 and 221.
- **Likely too flat as prototyped.** Mean clusters of 36-110 px may lose the "painting" look the owner asked for on 31 Oct (comment at line 194). Suggested acceptance bands (mine) to tune by eye: orphans under 5% (accents excepted), N90 of 3-5, mean cluster 8-25 px on land, adjacent-pairs-differing 20-40%. To add life, raise the `mid` weight, narrow the "rest zone", or raise `Dn`.
- **Foreshortening:** `HexYScale` of 1.848 means ground circles should be about 0.54 as tall as wide (from `PixelArt.cs` line 42). Wesnoth's view is about 0.745 ([read, Mesilliac](https://wiki.wesnoth.org/Mesilliac%27s_Essay_on_Terrain_Perspective)). The noise scales below use about 0.58.

Prototype (C#, works with the existing `VNoise` and `Hsh`, replaces the land branch of `BasePixel`; `r` is the ramp, `w` is brick width and `dn` is accent density). Mine, not run in Unity:

```csharp
int s=t*131, by=y>>1, bx=(x+((by*5)&(w-1|3)))/w;           // 2 px tall bricks, staggered per row to avoid banding
double broad=VNoise(x/19.0,y/11.0,s+41), mid=VNoise(bx/2.0,by/1.5,s+43), j=Hsh(bx,by,s+47);
double v=.60*broad+.30*mid+.10*j;
int i=v<.40?1:v<.60?2:3; if(broad>.42&&broad<.56&&j<.85)i=2; // the rest zone: flat base tone
uint c=r[i];
int cy=y>>2, cx=(x+cy*3)>>3;                                 // jittered grid of short accent dashes
if(Hsh(cx,cy,s+51)<dn){
  int py=cy*4+(int)(Hsh(cx,cy,s+52)*3), px=cx*8-cy*3+(int)(Hsh(cx,cy,s+53)*5), len=2+(int)(Hsh(cx,cy,s+54)*3);
  if(y==py&&x>=px&&x<px+len)c=r[4]; else if(y==py+1&&x>=px&&x<px+len)c=r[0]; // lit top, shadow below
}
```

Parameters I tested, in terrain id order 0-20:
- `w={4,3,4,5,8,3,8,4,4,3,3,3,8,4,5,5,4,3,3,3,7}`
- `dn={.35,.2,.35,.15,0,.25,0,.35,.2,.25,.2,.2,0,.1,.08,.1,.3,.2,.25,.25,.3}`

Desert also mixes in the dune sine, using brick coordinates: `v=.6*v+.4*(.5+.5*Math.Sin((bx*w+by*2.8)*.20+broad*4))`.

A later cache or texture change would need no save bump, since this changes pictures only (mine).

### Gaps
- Interaction with hill shading (line 1118), which uses continuous `Shade` and `MixC` and will reintroduce colours. Mine: pick a ramp index offset instead. Mountains and volcano pixel functions likewise were not audited.
- Whether the flowers on grass (line 205) should stay: they are orphans by design. Cure's exceptions would allow them if rare.

## How do Wesnoth, Unciv and Freeciv build their terrain graphics?

### Takeaway
All three separate terrain into ordered layers of authored variants chosen by rules, and none computes colour per pixel. The useful ideas for a code-generated map are explicit layer order, a small number of weighted base variants, and desynchronised water.

### Cited Findings
Wesnoth (first 100,000 of 104,771 characters of the main file):
- Terrain rules are macro-driven (`NEW:BASE`, `NEW:OVERLAY`, `NEW:FOREST`, `NEW:TRANSITION`, `NEW:FLOODFILL`). Default layer is -500, and examples include flowers -500, rubble -1, bridges over chasms -80, lily pads -86. Weighted base tiles use `BASE_P`, for example grass at 20, 25, 25, and ice from 10 to 100. `PROB=` lists can be cumulative — read, [terrain-graphics.cfg](https://raw.githubusercontent.com/wesnoth/wesnoth/master/data/core/terrain-graphics.cfg).
- `NEW:BASE` assumes 72 x 72 images and 11 variants, at layer -1000 by default — read, [new-macros.cfg](https://raw.githubusercontent.com/wesnoth/wesnoth/master/data/core/terrain-graphics/new-macros.cfg).
- The wiki has no global terrain palette. The Art page points to "Choosing your palette" (quoted there as "Make shadows look interesting"), which I did not open — read, [Create Art](https://wiki.wesnoth.org/Art). The Mesilliac essay does not discuss lighting direction — read, [essay](https://wiki.wesnoth.org/Mesilliac%27s_Essay_on_Terrain_Perspective).
- Wesnoth's forums returned an anti-bot "Access Denied" page, so artists' discussion there went unread. I did not try to get round it.

Unciv:
- `TileSetConfig` holds `useColorAsBaseTerrain`, `fogOfWarColor` (default `ImageGetter.CHARCOAL`), `tileScale` and `ruleVariants` — read, [TileSetConfig.kt](https://raw.githubusercontent.com/yairm210/Unciv/master/core/src/com/unciv/models/tilesets/TileSetConfig.kt).

Freeciv:
- `hexemplio` is an isometric hex tileset at 126 x 64 with `hex_side = 16`. Its fixed layer list runs Background, Terrain1-2, Darkness, Terrain3, Water, Roads, Special1, Grid1, City1, Special2, Fog, Unit and so on. Terrain layers match "floor, coast, lake", then "water, land", then "hills, foliage" — read, [hexemplio.tilespec](https://raw.githubusercontent.com/freeciv/freeciv/main/data/hexemplio.tilespec).

### Inferences
- **Layer order (mine):** floor-type terrain first, then land, then raised land, matching Freeciv's split. This could give `WarpedHexTerrain` a tie rule, though it currently uses nearest-centre, so this is only relevant if edges look unbalanced.
- **Weighted patches rather than equal noise (mine):** Wesnoth's probabilities show flat variants dominating. `Dn` above and the rest zone are the code equivalent.
- **Tile proportion (measured from sources):** hexemplio's 126 x 64 is about 0.51, close to this game's 64 x 30 row step at 0.47. It is the nearest published hex analogue and worth viewing by hand. Its art was not fetched.

### Gaps
- I did not measure colour counts of Wesnoth, Unciv or Freeciv images. That would have meant saving files locally, and I had no budget or permission to do so. So I cannot say whether their tiles have fewer colours than the proposal.
- Unciv's default tileset images and its value of `CHARCOAL` were not read.

## What do shipped games show for explored-but-unseen land, and is there a published rationale?

### Takeaway
Shipped games either tint towards a dark colour and add a pattern, or change how the land is depicted. Only Civ VI has a stated rationale that I could find, and it is thematic, with no numbers. Measured here, desaturation with a lighter multiply meets the earlier targets far better than the present fog, and it needs no ramp changes.

### Cited Findings
- **Unciv:** when not viewable, terrain images lerp 60% towards `fogOfWarColor`, for base and overlay images alike. An extra crosshatch hexagon is drawn in white at alpha 0.2. Unexplored tiles get a separate opaque image — read, [TileLayerTerrain.kt](https://raw.githubusercontent.com/yairm210/Unciv/master/core/src/com/unciv/ui/components/tilegroups/layers/TileLayerTerrain.kt), [TileLayerOverlay.kt](https://raw.githubusercontent.com/yairm210/Unciv/master/core/src/com/unciv/ui/components/tilegroups/layers/TileLayerOverlay.kt).
- **Wesnoth:** fogged hexes still draw terrain, with a fog image on a `fog_shroud` layer and directional edge overlays facing neighbours. Overlays are hidden unless flagged `visible_in_fog`. Shrouded hexes skip terrain altogether — read, [display.cpp](https://raw.githubusercontent.com/wesnoth/wesnoth/master/src/display.cpp), with `fog_prefix="terrain/fog/fog"` and `shroud_prefix="terrain/void/void"` — read, [game_config.cfg](https://raw.githubusercontent.com/wesnoth/wesnoth/master/data/game_config.cfg).
- **Freeciv:** `fog_style` is Auto (code adds fog), Sprite (one sprite on top of fogged tiles), or Darkness. Encroaching darkness along unknown edges has its own styles. Hexemplio uses Auto and CardinalSingle, and its Fog layer lies below units — read, [README.graphics](https://raw.githubusercontent.com/freeciv/freeciv/main/doc/README.graphics), [hexemplio.tilespec](https://raw.githubusercontent.com/freeciv/freeciv/main/data/hexemplio.tilespec). What Auto does to colour was not read.
- **Civ VI:** art director Brian Busatti tied fog to an age-of-exploration theme, with old parchment maps and instruments. He said the system lets them "be very exact where we hide fog of war", and that "Civ V had the cloud, but it was kinda flat" — read, [wccftech interview](https://wccftech.com/civilization-vi-interview-with-sarah-darney-and-brian-busatti/).
- Civ VI unexplored land as "blank parchment", explored land in a "map style" rather than darker terrain, and producer Dennis Shirk linking readability when zoomed out — snippet only, from a search summary of [Twinfinite](https://twinfinite.net/2016/07/civilization-vi-devs-explain-the-games-art-style/) and [Digital Trends](https://digitaltrends.com/gaming/art-civilization-vi). When I opened one Digital Trends copy, the cartography description was the writer's rather than a staff quote — read, [Digital Trends copy](https://www.digitaltrends.com/?p=1005521). Treat those as weak.
- Players complained explored and unexplored were too similar in Civ VI — snippet, opinion, [CivFanatics](https://forums.civfanatics.com/threads/new-fog-of-war-makes-the-map-harder-to-use.600464/page-2). That mirrors this game's measured 1.04:1.

Why this game's fog is darker than its multiply suggests:
- `MistThin` puffs are drawn at 96 x 64 px with alpha .3 for every remembered tile (`IsoRenderer.cs` line 274), while tiles sit 64 px apart in 30 px rows. That is about 3.2 overlapping puffs per tile by area (my arithmetic). Fully covered, three layers would reach 1-(.7³) = 66% — puff transparency will lower this.
- Simulated multiply with one full veil layer gives grass .068 and ocean .041, while the screenshot measures .044 and .029. So one layer does not explain the screen, and stacking or cloud shadows probably do — measured here, cause inferred.
- `IsoGround(t,i,j,frame)` at line 224 receives the live frame regardless of visibility, so remembered sea appears to animate. I did not read that function's body.

Fog candidates, median luminance — measured here, generator only with no scenery. "Proposed" means proposed ramps, with the first-pass forest ramp. Candidate targets (remembered land to ocean ≥1.5, land to cloud ≥2) are the first round's, and mine:

| Candidate | Ramps | Grass/ocean | Forest/ocean | Mountain/ocean | Desert/ocean | Grass/cloud .026 | Forest/cloud .026 | Ocean/cloud .026 |
|---|---|---|---|---|---|---|---|---|
| Multiply, no veil | Current | 1.43 | 1.15 | 1.14 | 2.14 | 1.98 | 1.20 | 1.38 |
| Multiply, one full veil layer | Current | 1.30 | 1.08 | 1.12 | 1.76 | 1.56 | 1.11 | 1.20 |
| **F: desaturate .55, ×.80, cool tint** | Current | **1.62** | 1.14 | 1.37 | 2.89 | **2.72** | 1.47 | 1.67 |
| G: lerp 45% to 58,66,84 | Current | 1.42 | 1.08 | 1.19 | 2.06 | 2.66 | 1.74 | 1.88 |
| **A: desaturate .55, ×.80, cool tint** | Proposed | **1.93** | 1.01 (1.20 with revised forest, by hand) | 1.51 | 3.68 | **2.58** | 1.34 (1.60 revised) | 1.33 |
| D: lerp 45% to 58,66,84 | Proposed | 1.61 | 1.00 | 1.29 | 2.45 | 2.56 | 1.59 | 1.59 |
| B: desaturate .50, ×.72 | Proposed | 1.83 | 1.00 | 1.46 | 3.34 | 2.20 | 1.20 | 1.20 |

For reference, the screen today measures remembered grass to ocean about 1.2 and forest to ocean about 1.0.

Formula for A and F, in display values (project is gamma space): `y=.299r+.587g+.114b; c'=(y+(c-y)*.55)*.80*(tint .97,1.0,1.08)`. Examples by hand from the proposed ramps: grass 98,156,66 → 87,115,81; ocean 28,84,164 → 39,64,108; desert 224,190,128 → 163,153,136.

Cloud darkening: multiplying cloud display values by .7 gives luminance ×.46, or about .012 (my arithmetic). With A that gives ocean/cloud 1.64. With F, ocean/cloud 2.05.

### Inferences
- **Do F first (S to try, mine).** It reaches the grass/ocean and grass/cloud targets with no art changes. Forest stays short at about 1.14, because forest and sea are close in value everywhere, and hue has to carry it. That's why the transform keeps 55% saturation rather than going grey.
- **`GUI.color` can only multiply, so desaturation needs something else.** Options, mine, untested:
  1. A small unlit material with `Graphics.DrawTexture` during Repaint, for ground and scenery alike. This also covers supplied SpriteCook art. I did not verify this API this session, or how it behaves with IMGUI clipping.
  2. Baking a second "remembered" `PixelCanvas` per cached picture, which doubles cache for those tiles and needs readable textures for supplied art.
  3. A cheap approximation: flat hex overlay lerp (already used for the owner wash at line 234), as candidate G. It covers ground only, and sprites would stay on multiply.
- **Thin the veil.** Mine: alpha .10-.12, or draw a puff on every third tile, stepped hard-edged. It keeps the owner's "dark mist" (his request per the comment at line 271), and saves about two thirds of those draws in the second form.
- **Second signal.** XAG 103 asks for more than colour (first round). Options, mine: freeze remembered water and foam at frame 0, which also removes the texture churn; and an optional sparse hatch, as Unciv does at alpha .2. Try freezing first.
- **Visible versus remembered remains clear in value:** grass .268 to .146 is CR 1.62, ocean .093 to .051 is CR 1.42, and saturation drops too. Check that "brighter fog" doesn't make the player mistake remembered land for live land. Units are hidden there, so a wrong read has real cost.
- **Parchment-style remembered land** would be the Civ VI family: a large art change and an owner decision. I don't recommend it now, because the owner chose dark mist.

### Gaps
- No shipped game's numeric fog values beyond Unciv's were found, and Wesnoth's fog image was not measured. No published study of fog readability found.
- Old World, Humankind, Endless Legend and Age of Wonders 4 fog: nothing read. Civ V: only Busatti's remark.
- Candidates exclude scenery, owner wash and cloud shadows, so on-screen numbers will be lower.

## How should water be animated and kept calm at this scale?

### Takeaway
Current open sea changes 13-17% of its pixels every step, three times a loop, with a hitch at wrap-around, and foam is re-rolled every frame. Published advice points to slow, low-amplitude motion that doesn't compete for attention. A world-locked set of short ripple dashes changes under 2% of pixels per step in my prototype.

### Cited Findings
Current behaviour:
- `frame=(int)(Time.realtimeSinceStartup*2.5f)%3` gives 3 frames at 400 ms — read, `IsoRenderer.cs` line 187.
- Pixels changing per step, ocean: 13.2%, 13.1%, 16.6% (the last being wrap-around). Lake: 13.7%, 13.7%, 17.7% — measured here. The formula at line 185 uses `frame*3` with period 24, so frames 0-2 are not a closed loop.
- Foam: `Hsh((gx+frame)>>1,gy>>1,frame+7)` re-rolls with the frame as seed (line 1147), flipping 32.2% of foam-band pixels per step — measured here.

Other sources:
- Factorio's animated water aimed to resemble the existing look, avoid photorealism and keep GPU cost low. It is described as "Slowly moving" and doesn't try to grab attention. Players can turn it off, when it freezes and stays cached — read, [FFF-323](https://cf-www.factorio.com/blog/post/fff-323).
- Ripple method: start from a chain of ovals for a tileable sine wave, with wide ovals for shallow broad waves, and crests slightly steeper than dips. Surface sparkle is a 6-frame linear loop. River flow uses pulsing lines, which suit windings that don't fit a tile grid. Reflections want low ripple speed, width and height — read, [SLYNYRD Pixelblog 10](https://slynyrd.com/blog/2018/10/12/pixelblog-10-water-in-motion).
- Wesnoth water: `NEW:WATER_342_180` calls use `DURATION=125` and `RANDOM_START=125`. I could not confirm how those numbers map onto frames, and I did not find the macro's definition. Waves use `NEW:WAVES` with 13 frames at 200 ms (`-A[01~13]`, 2.6 s loop) and `random_start=no`, so all tiles share a phase, masked to hexes — read, [terrain-graphics.cfg](https://raw.githubusercontent.com/wesnoth/wesnoth/master/data/core/terrain-graphics.cfg), [new-macros.cfg](https://raw.githubusercontent.com/wesnoth/wesnoth/master/data/core/terrain-graphics/new-macros.cfg).
- Mark Ferrari's GDC 2016 talk covers colour cycling and palette shifting for background animation without frame-by-frame drawing — snippet, [GDC Vault](https://gdcvault.com/play/1023586/8-Bit-8-Bitish-Graphics). The Vault gave a login notice; the talk was not viewed.

Proposal, prototype results — measured here:
- Ocean pixels changing per step: 13-17% → 1.5-1.7%, even across all 8 steps with no wrap hitch. Lake 1.7%.
- Static ocean metrics: colours 7 → 5, N90 3 → 2, orphans 17% → 0%, mean ΔL* 8.3 → 0.6.
- Foam as a sine swell of the band edge: flips 32.2% → 7.1% of a 6 px band per step.

### Inferences
Water code (mine, not run in Unity). It replaces lines 181-192 for terrains 4, 6 and 12, with `r` the water ramp and `f` in 0..7:

```csharp
int by=y>>1, bx=(x+((by*5)&7))>>3;
double b=VNoise(x/26.0,y/11.0,s+61)+(Hsh(bx,by,s+62)-.5)*.10;
uint c=b<.42?r[1]:r[2];                                      // two calm flat tones
int cy=y/7, cx=(x+cy*5)/13;
if(Hsh(cx,cy,s+66)<.7){
  int pcx=cx*13-cy*5+3+(int)(Hsh(cx,cy,s+63)*7), py=cy*7+(int)(Hsh(cx,cy,s+64)*6);
  int l=Len[(f+(int)(Hsh(cx,cy,s+65)*8))%8], dx=Math.Abs(x-pcx)*2;  // Len={0,2,4,6,4,2,0,0}
  if(y==py&&dx<l)c=(l==6&&dx==0&&Hsh(cx,cy,s+67)<.4)?r[4]:r[3];     // dash grows and fades in place
  else if(y==py+1&&dx<l-2)c=r[0];                                   // trough below
}
```

Foam: `foam = shore + Math.Sin(2*Math.PI*f/8 + gx/7.0)*.8 + (Hsh(gx>>2,0,7)-.5)*.6 <= 2.3`. The band breathes rather than sparkles. Keep the shallows band of lines 1148 but snap it to the reef ramp, instead of `MixC` to 84,204,226, for fewer colours.

Other recommendations, mine:
- **Frames:** change `%3` to `%8` at the same 2.5 Hz, giving a 3.2 s loop close to Wesnoth's 2.6 s. Eight frames multiplies the per-tile cache if each frame is stored. So bake the sea once and draw dashes from one small shared tiling overlay per frame, or test `%4`. I did not read `IsoGround`'s cache, and this must be profiled.
- **Palette-cycling equivalent:** because dashes are world-locked, an index map plus cycling table in a shader could replace frames altogether, if the map moves to a shader later. Ferrari's talk is the reference.
- **Setting:** add an option to freeze water, as Factorio does (S).
- **Do not scroll:** keep dashes in place. Sliding textures need seam handling across 64 x 40 tiles, and the sine at line 185 already causes the wrap hitch.

### Gaps
- Calmness and looks are judged only by these numbers. A 1.6% change rate might look too quiet; the visual check is still to be done.
- No authority gave a frame count or change rate for strategy-map water at this size. Saint11's Water GIF and Ferrari's talk should be viewed by hand.

## What specific, testable changes follow?

### Takeaway
Do the fog transform first, then de-jitter ground and quieten water, then swap ramps. Each has a pass/fail number that can go into `GraphicsChecks`, and side-by-side screenshots are still the final judge.

### Cited Findings
All baselines are from the sections above — measured here. All targets are mine, not from any standard.

| # | Change | Where | Pass test | Job (mine) |
|---|---|---|---|---|
| 1 | Remembered desaturate .55 × .80 with cool tint (F) | `IsoRenderer.cs` lines 180, 222, 241 | Remembered grass/ocean ≥1.5, grass/cloud ≥2 on screenshots (now 1.2 and about 1.6) | S to try, M to finish |
| 2 | Thin and step the veil; darken cloud ×.7 | Line 274-275, `MistThin`, `MistBank` | Ocean/cloud ≥1.5 (now about 1.04); colours in remembered grass patch under 100 (now 878) | S |
| 3 | Freeze remembered water | Line 224, pass frame 0 when `!visible` | 0% pixel change on remembered sea between frames | S |
| 4 | Remove Bayer, hash and lone-pixel terms from land | `BasePixel` lines 199-237 | Orphans under 5% (now 11-36%); changed pairs 20-40% (now 48-71%); N90 ≤5 | M |
| 5 | Calm water code and looping frames | Lines 181-192, 187 | Pixel change under 3% per step, wrap step no higher than others; orphans under 5% | S-M |
| 6 | Stable foam | Line 1147 | Foam flips under 10% per step (now 32%) | S |
| 7 | New ramp table | Lines 152-170 | Table above within 1 unit; reef/ocean ≥1.5, lake/ocean ≥1.3, grass/ocean ≥2 | S |
| 8 | Reef gets its own ramp | `TerrainRamp[12]` | Follows from 7 | S |
| 9 | Step cloud shadow alpha into 2 levels | `CloudShadows`, line 1236 | Visible grass N90 ≤ about 20 on screen (now 238) | S |
| 10 | Measure zoom effect, snap if confirmed | Zoom code | Orphans at each zoom within 5 points of zoom 1 | S to test |

### Inferences
- Ordering reason: 1-3 address the owner's "reads dark" complaint and do not depend on the rest. Only 4 and 7 need the owner's eye on taste, and they risk the "painted" look he asked for, so compare side by side and keep the old code behind a switch until he chooses.
- The metrics in `metrics.cs` of the scratch harness could become a `GraphicsChecks` test in a future change. If so, CLAUDE.md's rule on updating harnesses and status docs applies. Picture changes need no save version change (mine).
- None of this uses SpriteCook credits. Supplied scenery sprites (`Resources/Sprites`) keep their own palettes, so an audit of scenery against the new ramps is a possible follow-up.

### Gaps
- Whether any of this looks better. That needs a Unity build, screenshots and the owner.
- Frame cost of the fog material and overlays, not profiled. First-round advice to measure before rebuilding still stands.
