# Opening a gateway as an expedition (10 Nov 2026, save version 41)

Built from the owner's proposal: finding a gateway should be exciting, and opening it should be a small expedition with a strategic choice at the end. Code: `GateExpedition.cs` (rules), `GateUI.cs` (panel).

## The steps

1. **Discover.** As before: a unit sees a sealed gateway and it is marked on the map.
2. **Investigate.** A Scout, Medic or Priest that ends 2 turns on the sealed gateway reads its seal. The Scout is available from turn 1, so the first gateway is within early reach. Reading the seal tells you where the gateway leads, what that world is like, what the seal asks for and how dangerous the far side probably is.
3. **Meet the requirement.** One main requirement from the design of the far side, plus a smaller activation cost from the design on this side.
4. **Choose an approach.** Each shows its exact cost and consequence before you commit.
5. **It opens** for everyone, in both directions, and the opener sees the land within 3 tiles of the far side.

One of your units must stand on the gateway to begin, and again when it opens.

## Requirements

Main requirement, from the world beyond:

| Far-side design | World | The seal asks for |
|---|---|---|
| Grove | Fae Wilds, Hive Expanse | 40 faith (restore its shrine) |
| Sun | Ashen Waste | 80 gold (a sunstone, bought from traders) |
| Obsidian | Infernal Depths | A source of Iron; its guardians are half as strong again |
| Rift | Plague Marshes | 30 faith and 30 gold (feed both halves) |
| Frost | Drowned Deep | Writing and 40 gold (read the inscription, bring fuel) |

Activation, from the design on the home side:

| Home-side design | Activation |
|---|---|
| Arcane | 20 influence (its custodians' leave) |
| Steam | Bronze Working and 30 gold (mend its works) |
| Sky | Know 8 technologies (charge its crystals) |
| Void | 20 faith (quiet what whispers in it) |

## Approaches

| Approach | Cost | Time | Consequence |
|---|---|---|---|
| Stabilise | The requirement plus 30 gold | 6 turns | None |
| Force open | The requirement | At once | The guardians attack the soldiers on the gateway |
| Negotiate access | Half the gold, faith and influence | 3 turns | The partner pays the other half of the gold and gains open borders with you; +5 opinion each way |

- **Guardians:** strength 4 + 4 for each level of danger (1 to 3) + 2 for each age of whoever wakes them. Each soldier on the gateway loses the same health, 1 to 9, fewer the stronger the garrison; a soldier with no more health than that falls. The panel states the loss and how many would fall before you choose. Forcing needs at least one soldier on the gateway.
- **Partners:** a civilisation you know, at peace with you and not a rival, whose opinion of you is 10 or more, that has a city and can pay its half.

## Added 11 Nov 2026

- **First gateway.** While no gateway has opened in the game, a gateway asks half of its gold, faith and influence and stabilises in 3 turns.
- **Rough danger.** The danger beyond is shown as two neighbouring levels, one of them true. Forcing shows guardian strength and the loss per soldier as a range; the result is never worse than the worst shown.
- **Sunstone.** A Sun seal's 80 gold is waived for a civilisation one of whose units explores a ruin within 6 tiles of the gate.
- **Shrine.** A Grove seal's 40 faith is waived for a civilisation with a Shrine in a city within 10 tiles of the gate.
- **Rift.** The opener empowers the living side (grassland within 2 tiles of the far gate) or the burning side (waste there, and the opener sees 5 tiles round instead of 3).
- **Closing.** A unit on an open gateway can close it: a full turn, the tile still held, and not while a unit of a civilisation at war with the closer stands beside either side. The gateway is then sealed for everyone; the closer still knows the seal.
- **Control.** Holding the gate's tile is holding the crossing, because no one can enter a tile another civilisation's units stand on without fighting.

## What stays from before

A world's key technology, the Gatekeeper's Key wonder and the priest's rite of unsealing still open gateways. Rivals mostly rely on those: a rival that happens to read a seal and can pay will stabilise it, but rivals do not send investigators on purpose and never force.

## Not built from the proposal

- Guardians as units on the map and a "temporary breach".
- Recovering an item from a nearby ruin, a shrine building or a guardian to bargain with: requirements are paid in gold, faith, influence, technology or a resource.
- The Rift choice of which side to empower.
- Contesting, capturing and closing a gateway, and access agreements for allies.
- Uncertainty in the danger level beyond the word "probably": the level shown is the true one.
- Not hand-played; the costs are first guesses.
