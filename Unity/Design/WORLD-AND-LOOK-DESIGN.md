# Paradigm Realm — spherical world, realistic biomes and new look (design, planned)

Status: **Parts A and B (sphere topology, climate generator) are written in source but NOT yet tested in Unity; parts C and D (procedural art, diamond view, globe) are now written in source (29 Oct 2026) but NOT yet compiled or run in Unity.** Date: 28 October 2026. Extends CORE-DESIGN-16BIT.md and IMPLEMENTATION-PLAN-16BIT.md. Gameplay maths stays integer-only; world generation may use floating point internally because it runs once per new game and is seeded.

## Owner requests (28 Oct 2026)
1. Improve the terrain textures significantly.
2. Make the world spherical.
3. Make the progression from one biome to the next more realistic.
4. Use the main-menu artwork ("The Art of Paradigm Realm" gallery) as inspiration for the look of the playable game.

## What the gallery art looks like (observed)
47 images, 640 x 640. The map pictures (for example the coastal kingdom, enchanted forest, frozen kingdom and crystal realm) share these traits:
- A raised three-quarter view, with mountains, cliffs, towers and trees standing up from the ground.
- Dense, layered woodland: dark conifers and rounder broadleaf trees in clumps, with autumn colour accents.
- Rock-faced coastlines with white foam, small rocky islets, piers and ships.
- Rivers that run continuously, drop over waterfalls at cliff edges, and pass under stone bridges.
- Snow-capped mountains with strong light and shadow; ice floes at the poles.
- Farmland in coloured patches with windmills; roads as pale dirt paths; towns with blue or red roofs.
- A limited, saturated palette with hue-shifted shading and a consistent top-left light.
- Territory borders drawn as a glowing outline.

The current in-game terrain is flat 256 x 256 supplied textures composited per tile with soft blends (see Documentation/Terrain-Comparison.png). It has no height, no shorelines with foam, no cliffs and no tall scenery on the terrain itself.

## 1. Spherical world
Keep the square gameplay grid, but give it sphere topology.
- **Shape:** equirectangular grid. Columns wrap east to west. Leaving the top or bottom row re-enters the same row half a world away (x + width / 2), which is the true topology of a sphere. Latitude comes from the row number.
- **Everything distance-based must wrap:** movement, pathfinding, vision and fog, territory, settlement checks, AI scans, the atlas, and the camera (scrolling east never hits an edge).
- **Poles:** ice sheet and glacier rows, passable but slow and unproductive, so polar crossings are a real but costly shortcut.
- **Aspect ratio:** a sphere maps best to 2:1. Size presets would change to 2:1 shapes. Proposal: Tiny 48x24, Small 72x36, Medium 96x48, Large 144x72, Huge 192x96, Massive 320x160. The existing 48x30 shape stays as a flat legacy map so old saves keep loading (a saved world shape flag, save version bump).
- **Generation without seams:** sample 3D noise at the point on a unit sphere that matches each tile's latitude and longitude, so there is no seam at the wrap line and no stretching at the poles.
- **Globe view:** the Known World screen can show an orthographic globe, drawn on the CPU from the known map into one small texture and rotated with the mouse. Cheap, and it makes the sphere obvious.

## 2. Realistic biome progression
Replace the current pattern-and-latitude generator with a simple climate model, then classify tiles from it.
1. **Elevation:** continents from 3D noise; mountain chains from ridged noise along a few plate lines; sea level set per world.
2. **Temperature (integer band 0-7):** warm at the equator and cold at the poles (from latitude), lowered with height, softened near the sea.
3. **Moisture (integer band 0-7):** wind bands as on Earth (trade winds under 30 degrees, westerlies 30 to 60, polar easterlies); air gains moisture over ocean and loses it as rain on windward slopes, leaving dry rain shadows behind mountains; a wet belt at the equator and dry belts near 25 to 35 degrees (where the great deserts sit).
4. **Smooth the two climate fields** before classifying so neighbouring tiles differ by at most one band, except across coasts and mountain ranges. This is what removes abrupt jumps.
5. **Classify** with a Whittaker-style table onto the existing 21 terrain types: ice, tundra, arctic forest, temperate forest, grassland, dry steppe, desert, savanna, jungle, bog, coastal and mangrove tiles, with hills, mountains and volcanic terrain from elevation and hot spots.
6. **Rivers** start in high wet ground, run downhill to the sea or a lake, and leave green strips through dry land (as the Nile does). Lakes form in basins.
7. **Expected ordering:** north to south over land you would see ice, tundra, taiga, temperate forest, grassland or steppe, then desert, then savanna and jungle at the equator, with the pattern shifted by mountains and ocean currents.

## 3. Terrain art overhaul
Keep the same 16-bit rules (limited palette per biome, consistent top-left light, dark outlines) and add what the gallery has:
- **Transitions:** proper edge autotiling for every biome pair, replacing soft blends.
- **Height:** hills and mountains drawn as raised pieces that overlap the tile to their north; cliff faces where land meets water; trees as clustered sprites on forest tiles.
- **Water:** animated two or three frame shimmer, white foam along shores, rocky islets, ice floes at the poles.
- **Rivers:** continuous channels with waterfalls at elevation drops and bridge pieces where roads cross.
- **Scenery and settlements:** field patterns on farms, wind and smoke animation on cities, glowing territory outlines.
- **Honest limit:** Claude cannot paint hand-drawn art. Claude can write a generator that produces consistent pixel-art tiles from palette ramps, dithering and noise, and a loading pipeline for art supplied by the owner (the existing TEXTURE-MANIFEST.json is the starting point). The generator would look cleaner and more coherent than today but not as rich as the gallery pictures.

## 4. Order of work (proposed)
A. Sphere topology and 2:1 map sizes (save version bump; touches movement, vision, fog, camera, atlas).
B. Climate-based world generator and rivers.
C. Terrain art overhaul (autotiles, height, water, scenery).
D. Globe view.
Then the earlier plan resumes: more than three civilisations, rules alignment, combat, technology, victory, interface.

## Decisions (owner, 28 Oct 2026)
1. Art source: **procedural** pixel-art generator.
2. Viewing angle: originally the **three-quarter / diamond (isometric) view**. **Changed 29 Oct 2026:** the owner asked for the map to run horizontally, not diagonally, and then (same day) chose to keep the 3D look: land raised above water with bank faces, in horizontal rows. Later the same evening the owner asked again for no step at all between land and sea and for far more natural blending; the current renderer is level everywhere, with domain-warped organic borders (see IMPLEMENTATION-STATUS.md). The renderer is now a horizontal three-quarter view (64 x 48 tiles in level rows, land 8 px above water, raised scenery). See IMPLEMENTATION-STATUS.md.
3. World shape: **true sphere with pole crossing**. 2:1 map shapes were proposed and assumed; the owner has not objected.

## Status by part
- A. Sphere topology (save version 9, `mapShape` 0 = flat, 1 = sphere): written in source. `At`, `Distance`, `Move`, fog, the settlement scan and the ranged-attack midpoint wrap east-west and cross the poles. Version 8 saves load as flat maps. Sphere sizes: Tiny 48x24, Small 72x36, Medium 96x48, Large 144x72, Huge 192x96, Massive 320x160. The New Game screen has a Flat / Sphere picker, defaulting to Sphere. The current square renderer does NOT yet scroll seamlessly across the wrap; that comes with the new renderer.
- B. Climate generator (`WorldClimate.cs`): written in source. 3D noise on a unit sphere, land set to about 38 percent of tiles, temperature and moisture bands 0-7 (latitude, altitude, wind belts, distance from sea, rain shadows), smoothed, Whittaker table onto the 21 terrain types, coasts, mangrove, coral, ice caps, lakes in lowlands, rivers running downhill, and all three start sites placed on the largest landmass so land conquest is possible. The algorithm was tuned in a throw-away Python prototype (biome bands, smoothness and land share looked right); the C# was not compiled or run.
- C. Terrain art overhaul and renderer: implemented (horizontal level view with organic blending; see IMPLEMENTATION-STATUS.md).
- D. Globe view: implemented on the Known World screen, and (save version 17) as the furthest map zoom on a sphere once Mysticism/astronomy is known.
- Poles (save version 17): ice sheet and glacier are passable at a whole move per step with 1 hit point lost per turn; they cannot be settled or improved.
- Biome neighbours (save version 17): a Whittaker-class pass after generation removes hot-beside-cold and dry-beside-wet contacts on every land type.

## Remaining design questions
1. How tall should hills, mountains and trees be in the diamond view, and how large should one diamond tile be on screen (a first guess is 64x32 pixels)?
2. Should the flat option stay in the New Game screen once the new renderer exists?
