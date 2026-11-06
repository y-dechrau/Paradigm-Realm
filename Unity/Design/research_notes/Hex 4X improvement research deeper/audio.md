# Sound and music for Paradigm Realm: licences, packs, composers, techniques, accessibility (deeper round)

Not legal advice. Everything marked "owner check" must be confirmed by the owner before audio is committed or shipped.

**Evidence labels.**
- **[read]**: I fetched the page. The fetch tool returns a model's extraction rather than raw text, so section numbers and wording need checking against the live page before anyone relies on them. Licence wording below is paraphrased on purpose. Read the linked text for the exact terms.
- **[snippet]**: seen only through a search result summary. The page itself was not opened.
- **[secondary]**: a blog, fan page, forum or article about the thing, not the thing itself.
- **[measured here]**: I searched this project's files.
- **[memory]**: my own knowledge, not checked in this round. Treat as a lead.

**Sites that refused automated reading.** I did not work around any of these.
- opengameart.org returned 403 on the FAQ and on track pages. Its licence text file on static.opengameart.org did open.
- musopen.org returned 403 on its FAQ.
- web.archive.org cannot be reached by the fetch tool.
- The Paradox forum showed a browser check, so the Stellaris dev diary was not read.

## Baseline: is the game silent?

### Takeaway
Yes, as far as text searches can show. No audio class appears in the scripts, and no listener or audio component appears in the scene files.

### Cited Findings
- Searching `Unity/Assets/Scripts` for `AudioSource|AudioClip|AudioMixer|AudioListener` gives 0 matches. [measured here]
- Searching `Unity/Assets/**/*.unity` for `AudioListener|AudioSource:|m_AudioClip` gives 0 matches. This closes the gap left by the first round, which did not inspect scenes. A scene saved in binary rather than text form would not show up in this search. [measured here]
- The first round ranked sound fourth, with strong evidence for the gap and weak evidence for the size of the effect. — `Unity/Design/reports/Hex 4X improvement research.md`, item 4 [measured here]

### Inferences
- Audio code will have to add an `AudioListener` itself, for example with `AddComponent` on the camera or on a persistent audio object. Without one, nothing is heard.

### Gaps
- Nothing has been run in Unity for this note.

## Licences: commercial use, attribution, and public GitHub repository

### Takeaway
Only CC0 and CC BY material fits this project's habit of pushing everything to a public repository. Sonniss, Pixabay and Asset Store audio can all be legal inside a finished game, but their terms bar or do not clearly allow loose files in a public repository, so they would have to stay out of the backup.

### Cited Findings

| Source | Commercial use | Credit needed | Files in a public repo | Evidence |
|---|---|---|---|---|
| **CC0 1.0** (legal text) | Yes, any purpose | No | Yes: the rights are waived, so nothing restricts it | [read](https://creativecommons.org/publicdomain/zero/1.0/legalcode) |
| **CC BY 4.0** (legal text) | Yes | Yes | Yes, if notices and credit travel with the files | [read](https://creativecommons.org/licenses/by/4.0/legalcode) |
| **Kenney** | Yes | No; the logo is reserved | Page does not address it; CC0 would allow it. Owner check: the licence file in each download | [read](https://kenney.nl/support) |
| **Incompetech (Kevin MacLeod)** | Yes, under CC BY | Yes, or buy a Standard Licence | FAQ is silent; CC BY's sharing right should cover it | [FAQ read](https://incompetech.com/music/royalty-free/faq.html), [licences read](https://incompetech.com/music/royalty-free/licenses/) |
| **Freesound** | CC0 and CC BY yes; CC BY-NC no | CC BY yes; CC0 no | Follows each sound's licence | [read](https://freesound.org/help/faq/) |
| **OpenGameArt** | Depends on each item's licence | Depends | Depends. CC0 and BY fine; BY-SA and GPL bring copyleft questions | FAQ blocked; [OGA-BY text read](https://static.opengameart.org/OGA-BY-3.0.txt); [FAQ snippet](https://opengameart.org/content/faq) |
| **Sonniss GDC bundles** | Yes, inside your project | No | **No, in practice** (see below) | [licence read](https://sonniss.com/gdc-bundle-license/), [page read](https://sonniss.com/gameaudiogdc/) |
| **Pixabay** | Yes | No | **Risky**: unmodified standalone files are barred | [summary read](https://pixabay.com/service/license-summary/), [terms read](https://pixabay.com/service/terms/) |
| **Musopen** | Per recording; sources conflict | Sources conflict | Per recording | Blocked; [snippet only](https://www.wikipedia.org/wiki/File:Alexander_Glazunov_-_chant_du_menestrel,_op._71.ogg) |
| **Free Music Archive** | Per track; never NC | Yes for CC licences with a BY term | Follows each track's licence | [read](https://freemusicarchive.org/License_Guide) |
| **Unity Asset Store** | Yes, embedded in a product | Not set by the EULA | **No, in my reading** (see below) | [read](https://unity.com/legal/as-terms) |

**CC0 details**
- The waiver covers all territories and any purpose, including commercial use. A fallback licence applies if the waiver fails somewhere. — [CC0 legal text](https://creativecommons.org/publicdomain/zero/1.0/legalcode) [read]
- Three limits remain: trademark and patent rights are not waived, the work comes with no warranty (including of title or non-infringement), and the person applying CC0 does not clear other people's rights. A false CC0 upload is therefore the user's risk. — [same](https://creativecommons.org/publicdomain/zero/1.0/legalcode) [read]

**CC BY 4.0 details**
- Section 3(a) requires keeping creator identification, any copyright notice, the licence and disclaimer notices, a link where practical, a note of any changes, and a statement of the licence. It can be met in any reasonable way for the medium, including a link to a page holding the details. — [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/legalcode) [read]
- Section 2(a)(5)(B) bars adding terms or technical measures that restrict recipients' licensed rights. — [same](https://creativecommons.org/licenses/by/4.0/legalcode) [read]
- CC 4.0 treats music synchronised with moving images as always producing an adaptation. — [memory; not extracted in this round, so check section 1(a)]. If that applies to a game, it makes any share-alike track a real risk to the licensing of the game.

**Incompetech**
- The FAQ gives CC BY 4.0 as the credit wording, says the credit must not be hidden, and says game credits usually go on a Credits screen in the settings menu. — [FAQ](https://incompetech.com/music/royalty-free/faq.html) [read]
- Tracks may be edited freely. The music remains under copyright. The FAQ says nothing on repositories or resale. — [FAQ](https://incompetech.com/music/royalty-free/faq.html) [read]
- A Standard Licence is the paid route for when no credit is wanted. No price appears on the licences page; a PDF is generated and paid for through PayPal. — [licences page](https://incompetech.com/music/royalty-free/licenses/) [read]
- That page names no CC version, and third-party listings disagree (3.0 versus 4.0) for at least one track. — [Foundry listing](https://foundryvtt.com/packages/minstrel-packs-music-01) [snippet, secondary]

**Freesound**
- Uploaders pick CC0, CC BY or CC BY-NC per sound. NC bars earning from the result. Attribution should give title, author, link and licence, and a long list may sit on a linked credits page. — [Freesound FAQ](https://freesound.org/help/faq/) [read]
- Older sounds may still carry the retired Sampling+ licence, which the FAQ likens to NC. Avoid these. — [same](https://freesound.org/help/faq/) [read]

**OpenGameArt**
- OGA-BY 3.0 requires intact notices, author credit, title and link where practical, a licence copy or link, labelling of changes, and no implied endorsement. No clause on technical measures appears in its text, and it allows distribution under CC BY 3.0. — [OGA-BY text](https://static.opengameart.org/OGA-BY-3.0.txt) [read]
- The site's FAQ describes BY-SA as requiring derivatives under the same licence, and users have said many uploaders do not notice the anti-DRM clause in the CC licences. — [FAQ snippet](https://opengameart.org/content/faq), [forum snippet](https://opengameart.org/comment/27449) [snippet]
- At least one fantasy pack, Contemplative Fantasy Vol. 2 (CC BY 4.0), has a comment saying the author admitted it is AI-generated. — [OGA page](https://opengameart.org/content/contemplative-fantasy-vol-2) [snippet, unverified]

**Sonniss**
- Licensor is Sonniss Ltd under English law. Grant: worldwide, royalty-free, unlimited projects, no credit, usable in games. — [licence](https://sonniss.com/gdc-bundle-license/) [read]
- Sounds may not be supplied to others as sound effects, in raw, edited or redesigned form, or in any pack, library, template or SDK. Sharing is allowed inside your own team. Others should download from Sonniss. AI training is barred without permission. — [same](https://sonniss.com/gdc-bundle-license/) [read]
- The licence does not mention repositories by name. — [same](https://sonniss.com/gdc-bundle-license/) [read]
- Bundles listed run from 2015 to 2024, about 207 GB added together, with no 2025 bundle and a separate 2026 page. — [Sonniss page](https://sonniss.com/gameaudiogdc/) [read]

**Pixabay**
- Free, no credit needed, commercial use allowed, adapting allowed. Selling or distributing a file on a standalone basis is barred, meaning with no creative effort applied, and this names audio files and stock platforms. Games and repositories are not mentioned. — [Terms section 5](https://pixabay.com/service/terms/) [read]
- Pixabay gives no warranty that consents or licences were obtained. — [Terms section 14](https://pixabay.com/service/terms/) [read]

**Musopen**
- Own FAQ blocked, so nothing here is first-hand. Commons file pages show public-domain-marked uploads with attribution requested, but quote an older FAQ against direct sale. Third-party pages list some tracks under NC or ND. — [Commons mirror](https://www.wikipedia.org/wiki/File:Alexander_Glazunov_-_chant_du_menestrel,_op._71.ogg), [listing](https://mytour.vn/en/blog/kinh-nghiem-hay/how-to-choose-filter-and-search-for-open-licensed-music-on-musopen-mytour.html) [snippet, secondary]

**Free Music Archive**
- Licences are set by each artist, and FMA itself cannot grant any. NC variants bar commercial use. ND variants bar placing a song in a derivative work, and BY-SA carries the licence over to derivatives. A retired FMA-Limited licence covers personal listening only. — [FMA guide](https://freemusicarchive.org/License_Guide) [read]
- A conflict: the guide says all CC licences require attribution, yet CC0 has no such condition. — [FMA guide](https://freemusicarchive.org/License_Guide) versus [CC0](https://creativecommons.org/publicdomain/zero/1.0/legalcode) [read both]. The legal text wins.

**Unity Asset Store**
- EULA 2.2.1: non-exclusive, perpetual licence to incorporate an asset in a product with substantial original content, where the asset is not a substantial part, and to distribute it embedded in that product. — [Terms, Appendix 1](https://unity.com/legal/as-terms) [read]
- 2.2.1.1 bars letting users transfer or distribute the assets, bars any other distribution or sublicensing, bars cost pooling, and restricts AI and machine-learning training. Contractors need their own licence (2.4). — [same](https://unity.com/legal/as-terms) [read]
- Public repositories are not named. — [same](https://unity.com/legal/as-terms) [read]
- Forum answers say store assets in a public repo are a breach, while private repos are accepted. This is community opinion, not Unity's ruling. — [Unity discussion](https://discussions.unity.com/t/what-kind-of-license-can-be-used-to-protect-assets-purchased-from-the-asset-store-in-a-public-repo/860104), [private repo thread](https://discussions.unity.com/t/using-git-for-paid-for-assets-if-set-the-repository-to-private/879286) [snippet, secondary]

**Wesnoth (a tempting source of fantasy music)**
- Assets are GPL v2+ or CC BY-SA 4.0 depending on date. — [Wesnoth wiki](https://wiki.wesnoth.org/Wesnoth:Copyrights) [read]

### Inferences
- **Project rule proposal: only CC0 and CC BY in `Unity/Assets`, nothing else.** The owner's standing rule is to push every change to the public repo. Sonniss, Pixabay and Asset Store files would have to be git-ignored, and then a fresh clone would build a game with missing sounds. Keeping them out avoids that.
- Sonniss in a public repo looks like supplying sounds to third parties. That is my reading, as the licence does not name repositories. The same goes for the Asset Store.
- Pixabay audio inside a built game is arguably fine. The same file sitting unmodified in a public repo looks like the standalone case the terms bar.
- Avoid BY-SA, GPL and all NC tracks, including Wesnoth's. Copyleft could reach at least the audio and possibly more, and untangling that is not worth it for 6-10 tracks.
- Avoid Musopen for now. The terms could not be read, and classical recordings are not the wanted sound.
- CC BY's technical-measures clause matters only if the game ships later with store DRM. Owner check at that point.
- Every CC0 claim is the uploader's word with no warranty, so keep evidence: page URL, author, licence and the date seen.

### Gaps
- Live text of OpenGameArt's FAQ and Musopen's terms could not be read. The owner should open both in a browser.
- Whether a compiled game counts as an adaptation or a collection under CC is a legal question I cannot settle.
- Incompetech's Standard Licence price is not on the pages read.
- Kenney's `License.txt` inside the zips was not read, because no audio or archives were downloaded.

## Which sounds, and which named packs

### Takeaway
Effects can be covered entirely from Kenney's CC0 packs. Music has CC0 candidates, but no pack I could confirm is clearly right for a 16-bit fantasy strategy game, so the owner must listen first. Keep CC BY Kevin MacLeod tracks as the dependable backup.

### Cited Findings

**Effects: confirmed CC0 on the pack page**
- [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds): 100 files, CC0, released 2020 [read]
- [Kenney Music Jingles](https://kenney.nl/assets/music-jingles): 85 files, CC0, released 2014 [read]
- [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds): 130 files, CC0 [read]
- [Kenney RPG Audio](https://kenney.nl/assets/rpg-audio): 50 files, CC0, tagged foley, footstep and weapon [read]
- [Kenney UI Audio](https://kenney.nl/assets/ui-audio) and [Digital Audio](https://kenney.nl/assets/digital-audio): listed in the [audio category](https://kenney.nl/assets/category:Audio), individual pages not opened [read for the list only]
- Formats are not stated on these pages. [read]

**Effects and music together**
- [Ninja Adventure Asset Pack](https://pixel-boy.itch.io/ninja-adventure-asset-pack): CC0, 37 music tracks, 100+ effects, credit optional, usable in commercial games. Ninja theme, so the fit for fantasy is unproven. [read]
- Juhani Junkala's 512 Sound Effects (8-bit style): the pack's info file says CC0. — [mirrored INFO.txt](https://git.uni-greifswald.de/GAIM/gaim/src/branch/main/assets/Sounds/collection1/INFO.txt), [OpenGameArt](https://opengameart.org/content/512-sound-effects-8-bit-style) [snippet]

**Music candidates**

| Track or pack | Licence | Suggested role | Evidence |
|---|---|---|---|
| [Once Upon a Time (loop), TAD](https://opengameart.org/) (find by title) | CC0; fantasy, orchestral, WAV and MP3 | Main menu | [snippet] |
| [Treasure Hunter](https://opengameart.org/content/treasure-hunter) | CC0; simple orchestral loop | Peace, early eras | [snippet] |
| [Chiptune Adventures, Juhani Junkala](https://opengameart.org/comment/70103) | CC0 | 16-bit flavour, if a chip sound is wanted | [snippet] |
| [Ninja Adventure music](https://pixel-boy.itch.io/ninja-adventure-asset-pack) (pick by ear) | CC0 | Peace or exploration | [read] |
| [Folk Round, Kevin MacLeod](https://incompetech.filmmusic.io/song/3770-folk-round) | CC BY | Peace, early eras | [snippet] |
| [Minstrel Guild, Kevin MacLeod](https://incompetech.filmmusic.io/song/4056-minstrel-guild) | CC BY; the composer describes it as town or tavern music | City screen or peace | [snippet] |
| Teller of the Tales, Kevin MacLeod | CC BY, version unconfirmed | Peace, middle eras | [snippet] |
| [Determined Pursuit, Emma_MA](https://opengameart.org/content/determined-pursuit-epic-orchestra-loop) | CC0; battle tag | War only | [snippet] |
| Boss Battle Music, SubspaceAudio (find by title) | CC0; orchestral, seamless loop | War only | [snippet] |
| [Alkakrab free medieval ambient pack](https://alkakrab.itch.io/free-fantasy-medieval-ambient-music-pack) | **No named licence** | Hold until confirmed | [read] |
| [Alkakrab Medieval Vol. 2](https://alkakrab.itch.io/free-medieval-soundtrack-no-copyright) | **Unclear**: page says free for commercial use, and the author answered yes to a commenter asking about CC0 | Hold until confirmed | [read] |

- Collections to browse by hand: [CC0 cinematic music](https://opengameart.org/content/cc0-cinematic-music) and the CC0 Fantasy Music & Sounds collection on OpenGameArt. [snippet]
- Alkakrab's 10-track pack has looped versions of all tracks in mp3, WAV and Ogg and says no AI was used. When asked for a licence name, the author said they would think about it. Vol. 2 points to a READ ME I could not see and has 354 MB and 485 MB downloads. — [pack 1](https://alkakrab.itch.io/free-fantasy-medieval-ambient-music-pack), [Vol. 2](https://alkakrab.itch.io/free-medieval-soundtrack-no-copyright) [read]
- Avoid Fantasy Journey (CC BY-ND, no derivatives, which also bars edits). — [itch.io](https://augustomazzoli.itch.io/fantasy-journey-music-pack) [snippet]

**Ambient:** I found no named pack I could confirm. Freesound's CC0 filter is the legitimate route, per sound. — [Freesound FAQ](https://freesound.org/help/faq/) [read for the licence rules only]

### Inferences

Suggested cue list, in build order, for the owner to audition:

| Cue | Where to look first | Note |
|---|---|---|
| Button click, tab, error | Kenney Interface Sounds | Three to four variants; quiet, because this is the sound heard most often |
| End turn | Kenney Interface Sounds or a soft bell from Freesound CC0 | Must fire on the click, before AI turns run (about 1.5 s late game) |
| New turn or your move | Kenney Music Jingles | Very short; useful after long AI turns |
| City founded | Kenney Music Jingles | A warm rising figure |
| Battle: hit, kill, loss | Kenney Impact Sounds, RPG Audio | Only for fights the player can see |
| Research complete | Kenney Music Jingles | Different in shape from city founded |
| Era change | Longer Music Jingles item, or Freesound CC0 fanfare | The largest stinger, with music dipped under it |
| First contact, war declared | Music Jingles | Pairs with existing leader screens |
| Ambient beds | Freesound CC0: wind, sea, forest, night | Quiet, chosen by view; later step |
| Music, 6-10 tracks | Table above | Peace tracks first |

- Six-track minimum: menu, three peace (early, middle, late), two war. Ten tracks would widen the peace lists. Six eras share these, grouped into three bands.
- Rough size: 3-minute Ogg at 160 kbps is about 3.6 MB, so ten tracks are about 36 MB in the public repo. My arithmetic, not measured.
- Music from one composer or one pack will sound more unified. Mixing chiptune with orchestral will likely sound wrong, so the owner should pick a palette first, chip or small acoustic ensemble, then fill it in.
- Kevin MacLeod's music is used very widely, so it may sound familiar to players. That is my opinion, not a finding.

### Gaps
- **No track here has been heard by me.** Fit, loop quality and loudness are all unknown, so listening is required.
- OpenGameArt pages were blocked, so its licences are snippet-level. Each must be opened in a browser and its licence recorded. Packs can also carry mixed licences per file.
- For OpenGameArt items without a direct link, the title search result pointed to node pages I could not open. Exact URLs need finding by hand.
- I did not research AI-generated music or commissioning rates.

## What composers and audio designers say

### Takeaway
The clearest long-play lessons are structural: Civ IV used era playlists, Civ V used peace and war playlists, and Civ VI grew arrangements by era. I found no composer talking directly about fatigue in long 4X sessions, so the advice on that is partly my inference.

### Cited Findings

**Baba Yetu and Civ IV**
- Tin says Soren Johnson, his college roommate and Civ IV's lead designer, phoned him about the opening animations and asked for an African-style vocal piece with drums. He spent a month on the theme. Tin ties it to the opening animations; the article does not call it a menu theme. — [Game Developer interview](https://www.gamedeveloper.com/game-platforms/interview-christopher-tin-from-i-civilization-iv-i-to-calling-all-dawns) [read]
- It was the first video game music to win a Grammy (2011). — [Mohawk Games](https://mohawkgames.com/?p=3435) [read]
- Civ IV had playlists for each era, centred on Western music from the medieval period on, and leader music was composed separately from the mostly licensed soundtrack. — [Square Enix Music Online interview](https://squareenixmusic.com/features/interviews/michaelcurran.shtml) [read; the licensed detail is from a search summary of the same page]

**Civ V (Knorr and Curran)**
- Civ V reorganised into War and Peace playlists for four world regions. War pieces are more aggressive or melancholy, and distort the melodies more than the peace pieces. — [same interview](https://squareenixmusic.com/features/interviews/michaelcurran.shtml) [read]
- Knorr varied orchestration so that repeated phrases stay interesting. — [same](https://squareenixmusic.com/features/interviews/michaelcurran.shtml) [read]
- Curran describes tiered combat sound: units of four or more play one looping group sound rather than many separate ones. Ambient sound changes by screen area, and music is levelled dynamically to sit with other audio. — [same](https://squareenixmusic.com/features/interviews/michaelcurran.shtml) [read]

**Civ VI and VII (Knorr and Rizzo)**
- Rizzo says Civ VI added instruments and changed style as time moved on, whereas Civ VII themes do not evolve, so its first age starts with a larger group. Knorr describes keeping similar mix settings to unify varied ensembles, and era-specific ambient themes for peace and war in Civ VII. — [Laced Records](https://www.lacedrecords.co/blogs/blog/how-the-civilization-vii-music-team-created-a-world-music-masterpiece) [read]
- A fan blog describes four variations per civilisation, moving from one or two instruments to small ensemble, full orchestra, then synthesisers, with the melody kept throughout. It credits the score to Knorr but does not attribute this description to him. — [LadiesGamers](https://ladiesgamers.com/paulas-game-ramblings-music-of-civilization-vi/) [read, secondary]

**Old World (Tin)**
- The score fuses traditional Arabic music with contemporary classical, with Arvo Part as an influence, and began with Tin contacting Middle Eastern musicians. It was Grammy-nominated. — [WSHU summary](https://www.wshu.org/podcast/music-respawn/2022-01-20/christopher-tins-soundtrack-for-old-world-is-a-global-project), [Mohawk](https://mohawkgames.com/?p=3435) [read, summaries]
- No page I read says how the music behaves in play. A Mohawk podcast with Tin and Johnson exists, but I did not hear it. — [Mohawk](https://mohawkgames.com/?p=3435) [read]

**Winifred Phillips**
- Her Dragon Front series argues music should support strategy players' focus. Part 1 is about music fitting the game's message, citing an advertising study and a film-score study. — [Part 1](https://gamedeveloper.com/audio/video-game-composers-can-make-you-smarter-the-music-of-dragon-front-part-1) [read]
- Part 2 says she kept all in-game music at moderate to slow tempo, with nothing energetic, citing a marketing study on slow tempo and alpha waves, a design-ideas study and a driving-simulator study. — [Part 2](https://gamedeveloper.com/audio/video-game-composers-can-make-you-smarter-the-music-of-dragon-front-part-2) [read]
- A caution: none of those three studies tested strategy games. A student paper in search results says alpha activity's role in cognition is unclear, which is weak but points the same way. — [Student paper](https://www.scribd.com/document/323899003/COGS402paper-odt) [snippet, weak]
- On repetition, she avoids staying in one key for too long, and suggests modulation, small tempo shifts and new motifs to keep loops fresh. Battle and calm music should share style, with stingers smoothing the change. — [GDC Showcase Q&A](https://www.gamedeveloper.com/audio/composer-interview-gdc-showcase-game-music-q-a) [read]

**Stellaris (Waldetoft)**
- He chose to avoid static, ambient-only music, wanting melody and a beat, with ambient space sound built as separate layers by sound design. A separate writer notes the score must survive being heard hundreds of times, without describing how. — [Dev Diary 27 snippet](https://forum.paradoxplaza.com/forum/threads/stellaris-dev-diary-27-music-sound.916337/page-3), [Nerd Stash](https://thenerdstash.com/?p=35768) [snippet; diary blocked]

### Inferences
- Choose calm, moderate-tempo pieces for most of play. Keep the driving "battle" tracks rare. This fits Phillips and Civ V's war playlists, though the evidence for any cognitive benefit is thin.
- Prefer tracks that change key or section, and prefer longer ones to short loops.
- Put silence between tracks, about 30-90 seconds. No composer I read recommends this directly. It is my judgement for long sessions, and easy to tune.
- Era progress can be shown by instrumentation, as in Civ VI, by choosing sparse tracks for early bands and fuller ones later, without any real-time layering.

### Gaps
- **I found no statement from any of these composers about fatigue in long sessions.** Please do not present one as sourced.
- I found no Knorr statement on real-time layers or stems. Civ VI's variations may be separate arrangements, so "layering" may be the wrong word for them.
- Phillips's book, *A Composer's Guide to Game Music*, was not read. The Tin and Knorr podcasts and the RNZ interview were not heard.

## Cheap adaptive techniques and Unity implementation

### Takeaway
Era-band playlists plus a peace or war switch give most of the benefit with two audio sources and no special music files. Stem layering needs matching stems that free packs rarely offer, so skip it for now.

### Cited Findings

**Import settings (Unity 6.3)**
- Decompress On Load suits small sounds, but Vorbis then uses about ten times more memory than compressed, and ADPCM about 3.5 times, so not for large files. — [AudioClip import docs](https://docs.unity.com/en-us/engine/6000.3/manual/audio/reference/class-audio-clip) [read]
- Compressed In Memory has slight CPU cost on the mixer thread. — [same](https://docs.unity.com/en-us/engine/6000.3/manual/audio/reference/class-audio-clip) [read]
- Streaming uses minimal memory with about 200 KB overhead per clip, decoded on a separate thread. — [same](https://docs.unity.com/en-us/engine/6000.3/manual/audio/reference/class-audio-clip) [read]
- ADPCM suits noisy, frequent sounds and is 3.5 times smaller than PCM. Vorbis or MP3 suits medium effects and music. Advice: start at high quality and lower until the loss is audible. — [same](https://docs.unity.com/en-us/engine/6000.3/manual/audio/reference/class-audio-clip) [read]
- Force To Mono reduces size. Load In Background is off by default and avoids main-thread stalls. — [same](https://docs.unity.com/en-us/engine/6000.3/manual/audio/reference/class-audio-clip) [read]

**Playback**
- `PlayOneShot` does not cancel sounds already playing and takes a volume multiplier; values above 1 may clip. — [docs](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/audiosource/playoneshot) [read]
- `PlayScheduled` runs on the `AudioSettings.dspTime` clock, independent of frame rate, and is Unity's preferred way to stitch clips, using two alternating sources. Schedule ahead by 100-200 ms; the sample schedules about 1 second ahead. — [docs](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/audiosource/playscheduled) [read]

**Audio Mixer**
- Mixers route sources through groups (buses). Snapshots save settings to transition between moods. Ducking lets one group reduce another. Web builds support this only partly. — [Mixer docs](https://docs.unity.com/en-us/engine/6000.3/manual/audio/mixer/mixer) [read]
- The pages read do not cover exposed parameters or `SetFloat`. — [usage page](https://docs.unity.com/en-us/engine/6000.3/manual/audio/mixer/usage) [read]. From memory: volume is set in decibels through an exposed parameter, so a slider needs a log conversion. Check [the AudioMixer reference](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/audio/audiomixer). [memory]

### Inferences

**Techniques by cost**

| Technique | Extra work | Precedent | Advice |
|---|---|---|---|
| Era-band playlists with no-repeat shuffle and silent gaps | Very small | Civ IV | Do first |
| Stingers on events, music dipped under them | Small | Civ V levelling, Phillips | Do first |
| Peace and war lists | Small | Civ V | Do second |
| Crossfade with two sources | Small | Unity sample | Do second |
| Ambient bed chosen by view | Small to Medium | Civ V, Stellaris | Later |
| Mixer snapshots | Small, but adds an asset | Unity docs | Optional |
| Real-time stem layers | Large, plus suitable stems | Possibly Civ VI | Skip |

**Project-specific warnings**
- **War trigger:** with up to 66 nations, "at war with someone" may be true almost always, so a plain test would play war music constantly. Use combat involving the player in the last few turns, or an enemy near a city, with hysteresis of several turns. Change at the track's end, or with a slow crossfade.
- **Battle spam:** rival turns can produce many events. Play only events the player can see, with a cooldown per cue and a cap on simultaneous sounds. Curran's group sound for large fights is the same idea. Late-game AI turns of about 1.5 s make this important.
- **Parallel worlds:** fights on a world not in view should stay silent, using `viewRealm`.

**Suggested shape** (not built, and unverified in Unity)
- One static class, for example `GameAudio`, with a persistent object holding an `AudioListener` if the camera lacks one, two music sources, one ambient source and a small pool for effects.
- Calls by named cue: `GameAudio.Play(Cue.EndTurn)`. This fits the IMGUI code, where `GUI.Button` results can trigger sound. A wrapper around common buttons would avoid hundreds of edits. I did not audit how buttons are drawn.
- Clips loaded from `Resources/Audio`, as art is already loaded from `Resources`.
- Settings in PlayerPrefs, as the tutorial setting is, so saves and save versions are unaffected.
- Start with plain volume multiplication and no mixer asset, which suits the code-first project. Add a mixer only if ducking by hand proves awkward.
- Import settings: effects ADPCM with Decompress On Load, Force To Mono where stereo adds nothing; music Vorbis with Streaming and Load In Background; jingles Vorbis, Compressed In Memory if longer.
- Silent harness runs: headless checks must not break when no audio device exists. Keep `GameAudio` out of engine logic and call it only from UI or event display code.

**Working rules to observe**
- Commit each audio file with its Unity-generated `.meta`.
- Add a credits file beside the audio and an in-game Credits screen. Log for each file: title, author, source URL, licence and version, date seen, changes made and the file's role.
- Update `IMPLEMENTATION-STATUS.md` and report honestly what was heard in Unity.

### Gaps
- Memory, CPU and startup cost are unmeasured for this project. The size figures above are estimates.
- Whether Unity's own docs recommend any particular shared design for one-shot pools was not checked.

## Accessibility

### Takeaway
Guidelines ask for separate volume controls and no essential information by sound alone. Add a mono toggle and distinct cues, and both major guidelines are largely met for a game with no speech. These are voluntary guidelines. I did not research any legal duty.

### Cited Findings
- Game Accessibility Guidelines, Hearing, Basic: separate volume or mute controls for effects, speech and music; no essential information by sound alone. — [full list](https://gameaccessibilityguidelines.com/full-list/) [read]
- Same, Intermediate: stereo/mono toggle; captions or visuals for significant background sounds; audio-carried supplementary information repeated visually. — [full list](https://gameaccessibilityguidelines.com/full-list/) [read]
- Same, Cognitive and Vision, Intermediate: sound and music choices for key events kept distinct, and separate volume controls also listed. — [full list](https://gameaccessibilityguidelines.com/full-list/) [read]
- Xbox guideline 105: music, voice-over, active effects and background effects should all be adjustable or mutable independently. A mono option sends all audio to both channels. Audio events longer than three seconds should be pausable. It also suggests lowering game audio when a screen reader speaks. — [XAG 105](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/105) [read]
- The same guideline cites Grounded's six sliders: master, effects, music, UI, dialogue and voice chat. It says removing nonessential audio also helps players who are easily distracted. — [XAG 105](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/105) [read]
- Xbox guideline 104: captions for important sounds not already shown visually; players in noisy rooms or with sound off also benefit. — [XAG 104](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/104) [read]

### Inferences
- **Settings to build from day one:** Master, Music, Effects, Interface, Ambient sliders; a mute-all; mono toggle; all remembered in PlayerPrefs. Optional: pause music when the window loses focus, and a "music gap" setting.
- With no voice acting, speech sliders and subtitles do not yet apply. If spoken leader lines arrive later, add a voice slider and subtitles at that point.
- **Silent play must remain complete.** Every cue needs a visual already on screen. War declared, research complete and era change should be checked against this rule; I did not audit the UI.
- Give each event its own melodic or rhythmic shape, not only a change in pitch.
- A test with the sound off should be part of the hand-played game already recommended in the first round.

### Gaps
- I did not research legal accessibility duties, such as the European Accessibility Act, or how they relate to games.
- I found no data on how many 4X players use these options.
- The mono route in Unity (likely `AudioSettings` speaker mode) was not checked against its docs. [memory]
- Owner checks overall: (1) read each licence on the live page and keep a dated record; (2) read `License.txt` or READ ME in every download; (3) open OpenGameArt and Musopen in a browser, as both blocked automated reading; (4) ask Alkakrab for a named licence in writing before use; (5) decide whether the repository stays public; (6) listen to everything and check the Credits screen meets CC BY before shipping.
