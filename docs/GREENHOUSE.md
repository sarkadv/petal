# Petal — Greenhouse Screen

> The morning phase of the daily loop (GAMEPLAY.md §3–4). GAMEPLAY.md §4 has the rules; this doc describes the
> screen: what's on it, what each part shows and how it's used. The agreed look is
> [`reference/greenhouse_concept.svg`](reference/greenhouse_concept.svg).

---

## 1. Look

The inside of a small glass greenhouse seen from **straight above**, in warm morning light (GAMEPLAY.md §1.1):

- Glass walls with white frame posts, grass and flowers outside. The greenhouse is a long hall with a
  **five-sided bay** at the bottom end.
- A glass **roof** over it: a ridge with a raised glass strip and small brass knobs at each end, crossbars, and a
  roof that fans out over the bay. It overhangs the walls, with a gutter.
- Gravel floor with the roof frame's shadows and diagonal sunbeams, strips of soil with plants along both side
  walls, and a tiled **path** from the garden supplies cabinet down to the door.
- A few small pots and a soil sack in the corners of the bay.

Everything that never changes is one static SVG background. The things you interact with sit on top of it.

### Layout

The screen is laid out on a fixed **720×1280 stage** (the base viewport), centred. On taller or wider phones the
extra space around it is grass. From top to bottom:

| Where | What |
|---|---|
| Top, on the grass | Back button, day chalkboard, inventory button, money pill (§2) |
| Along the top wall | The tool shelf (§3) |
| Top of the path | The garden supplies cabinet (§5) |
| Either side of the path | The pots, 2 columns × 3 rows (§4) |
| In the bay at the bottom | The Open Shop sign over the door (§6) |

## 2. Top bar

- **Back** (round wooden button, top left): see §8.
- **Day** on a small chalkboard, in handwriting: *Day 3* / *Den 3*.
- **Inventory** (round wooden button with a wicker basket of flowers, left of the money): a *×N* badge shows how
  many flowers are in today's inventory. Tapping it opens the inventory (below).
- **Money** in a cream pill (top right): a gold coin and the amount.

### Inventory

Today's inventory of harvested flowers (GAMEPLAY.md §4.5) in a window over the greenhouse, titled *Inventory* /
*Inventář*. It looks like the seed choices (§3), but bigger and only for looking:

- A row per flower under the flower's name, in catalogue order, with a thin line between rows.
- An item per colour and quality: the flower in its colour, a gold star per quality level, the colour's name and
  the count in a corner badge. Within a flower, colours go in colour order (red, pink, white, yellow, orange,
  purple, blue, green, as in GAMEPLAY.md §4.4) and each colour from the lowest quality up (*Tulip*: red ★, red ★★,
  yellow ★).
- A row with more items than fit scrolls sideways; more rows than fit scroll down.
- When it's empty it says *You have no harvested flowers yet* / *Zatím nemáte žádné sklizené květiny*.
- Items can't be tapped. **Close**, tapping outside the window or Back closes it.

## 3. Tool shelf

A wooden shelf along the top wall holds five tools, left to right: **Seeds** (a tray of seed packets),
**Water** (watering can), **Fertilize** (sack), **Harvest** (wicker basket of flowers) and **Clear** (trowel).
Each has its name written under it.

- **Tap a tool** to pick it up. The selected tool has a warm glow behind it. The screen opens with **Water**
  selected.
- **Tap a pot** to use the selected tool on it (§4).
- **Seeds** and **Fertilize** always show a small **count badge**:
  - Seeds: a dot in the chosen seed's colour and how many of it are left. The packets on the tray are
    coloured to match. At *×0* the dot and the packets disappear, leaving an empty tray and just *×0*.
  - Fertilize: the chosen fertilizer's icon and how many are left. At *×0* the icon disappears, leaving just *×0*.
  - Until something is chosen, the tool takes the first seed or fertilizer in stock. When the chosen one runs
    out it stays chosen and shows *×0*; switching to another is up to the player (hold the tool).
- **Hold Seeds or Fertilize** (0.4 s) to choose which one it uses. A small popup opens under the shelf:
  - Seeds: a paper seed packet for each seed in stock, with the flower in its colour on it, the colour's name under
    it and the count in a corner badge. The packets are in a row per flower, under the flower's name (*Tulip*: red,
    yellow). A row with more colours than fit scrolls sideways and more rows than fit scroll down, each with a
    scrollbar; the popup opens with the current choice in view.
  - Fertilize: a row for each fertilizer in stock with its icon, its name and the count in a badge on the right.
  - Both have a thin line between rows.
  - The current choice is outlined, unless it has run out: only items in stock are listed. Tap one to pick it;
    tap anywhere else to close. Tapping another tool closes it and picks that tool up in the same tap, and holding
    Seeds or Fertilize opens its choices straight away; other taps outside the popup only close it.
  - With nothing in stock it says *You have no seeds* / *You have no fertilizer*.

## 4. Pots

Each plot is a round terracotta pot seen from above. The six starting pots sit in two columns either side of the
path; extra plots from upgrades (GAMEPLAY.md §7) add rows.

### What a pot shows

There's no text on a pot; everything is read from the picture:

| State | Picture |
|---|---|
| Empty | Plain soil |
| Seedling | Small sprouts |
| Growing | Leaves with green buds |
| Ready | Leaves with open flowers in the flower's colour |
| Withered | Drooping brown plants |
| Dead | Dried-up brown sprouts |
| Watered (any state) | Darker soil, until the night |
| Growth fertilizer | Soil tinted soft green, until the night |
| Bloom fertilizer | Soil tinted soft purple, until the night |

The fertilizer tint shows while the fertilizer is in the soil (GAMEPLAY.md §4.2): from fertilizing until the night,
also in an empty pot. It takes effect overnight on the plant in the pot then, which keeps the benefit after the tint is gone. Dry soil on a seedling or growing plant means it still needs water today.

### Using a pot

- **Tap** a pot to use the selected tool on it. It gives a small bounce when the action worked; nothing happens
  if it can't be done (e.g. planting into a pot that isn't empty).
- **Clearing a living plant** asks first (*Remove this plant?* with *Keep* / *Remove*); tapping outside the question
  or Back counts as *Keep*. Dead and withered plants are cleared straight away.
- **Hold** a pot (0.4 s) to see what's growing in it. A small popup opens diagonally off the pot's corner that
  faces the middle of the screen, overlapping that corner a little, with two lines:
  - the flower, with its colour if it comes in several (e.g. *Tulip (red)*), and
  - its state: *Ready in 2 d*, *Ready to harvest*, *Withered* or *Dead*.

  Empty pots show nothing. Letting go after a hold never also uses the tool on the pot. As with the tool popups,
  tapping a tool closes it and picks the tool up; tapping anywhere else only closes it.

## 5. Garden supplies

A small one-door wooden cabinet at the top of the path, its door ajar with seed packets showing inside and a rake leaning against its right side. **Tap it** to open the **Garden supplies**
window (*Kůlna*) over the greenhouse:

- The title and your money at the top, then two tabs, **Seeds** and **Fertilizers**. It opens on Seeds the first time, then on whichever tab was open when it was last closed.
- Seeds: one row per unlocked flower with its name, price (gold coin + amount) and grow time. Flowers that come in several colours have
  colour swatches to pick from, and the row shows the chosen colour and how many of those seeds you own.
- Fertilizers: Growth and Bloom, each with what it does, its price and how many you own.
- **Buy** adds one to your stock; it's greyed out when you can't afford it.
- **Close**, tapping outside the window or Back closes it.

## 6. Open Shop

A green sign over the door in the bottom of the bay. Tapping it ends the morning and opens the shop
(GAMEPLAY.md §5). Until the shop exists it ends the day: today's inventory wilts, the night passes and the next
morning starts.

## 7. Text and fonts

- Lora for everything, semibold on buttons and tool names; Caveat (handwriting) only on the day chalkboard.
- All player-visible text is in English and Czech (`localization/translations.csv`).

## 8. Back

Android's back button (or the back button in the top bar) closes the topmost thing that's open, in this order:
the *Remove this plant?* question, a popup, the inventory, the garden supplies window. Each of these is an `Overlay`; the order
is the order they're drawn in. With nothing open it returns to the main
menu, and **Play** continues the same game.

---

## Implementation

| File | What |
|---|---|
| `scenes/greenhouse/greenhouse.tscn`, `Greenhouse.cs` | The screen: stage, top bar, tools, holds, back |
| `scenes/greenhouse/tool_button.tscn`, `ToolButton.cs` | One tool on the shelf, with its count badge |
| `scenes/greenhouse/count_badge.tscn`, `CountBadge.cs` | The *×N* badge on the tools, inventory button, seed packets, inventory items and fertilizer rows |
| `scenes/greenhouse/plot.tscn`, `PlotView.cs` | One pot |
| `scenes/greenhouse/popovers/seed_popover.tscn`, `SeedPopover.cs`, `seed_packet.tscn`, `SeedPacket.cs` | The popup with the seeds to choose from |
| `scenes/greenhouse/item_row.tscn` | A row of items under a flower's name that scrolls sideways, used by the seed popup and the inventory |
| `core/ui/RowList.cs` | The list of rows with a thin line between them in the two choices popups and the inventory |
| `scenes/greenhouse/overlay_window.tscn` | Shared frame of the inventory and the garden supplies window: dim, card, title and **Close** |
| `scenes/greenhouse/inventory/inventory.tscn`, `Inventory.cs`, `inventory_item.tscn`, `InventoryItem.cs` | The inventory window |
| `scenes/greenhouse/popovers/fertilizer_popover.tscn`, `FertilizerPopover.cs`, `fertilizer_row.tscn` | The popup with the fertilizers to choose from |
| `scenes/greenhouse/popovers/ChoicesPopover.cs` | Shared base of the two above: the rows, the empty text, closing on a pick |
| `scenes/greenhouse/popovers/info_popover.tscn`, `InfoPopover.cs` | The popup with what is growing in a pot |
| `scenes/greenhouse/popovers/Popover.cs` | Base of all three popups: placing the popup on screen, and closing on taps outside it (a tap on a tool also picks the tool up) |
| `core/ui/Overlay.cs` | Base of the popups, the inventory, the garden supplies window and the question: closing on Back, and on taps outside for the window and the question |
| `scenes/greenhouse/seed_stall/seed_stall.tscn`, `SeedStall.cs`, `stall_row.tscn`, `StallRow.cs` | Garden supplies window |
| `scenes/greenhouse/art/` | Placeholder SVG art, cut from or drawn to match the reference |
| `core/game/Plot.cs`, `core/game/GameState.cs` | The rules from GAMEPLAY.md §4 |
| `data/game_data.tres` | Flowers and fertilizers (prices, icons, soil tints), starting values, stems per harvest, Bloom's chance |
| `ui/petal_theme.tres` | Styles for the chalkboard, pill, tools, popup, dim, tabs and sign |

## Open points

- **Teaching the gestures:** nothing on screen shows that tools and pots can be held. The tutorial
  (GAMEPLAY.md §10) should teach it.
- **Quality isn't visible:** stars were removed from the pots, so players can't see a harvest's quality before
  picking it. The pot popup could show it.
- **Notches:** the top bar sits close to the top of the 720×1280 stage. On taller phones there's grass above it,
  but a 16:9 phone with a notch may need the bar pushed down by the safe area.
- **Font licences:** the `OFL.txt` files in `ui/fonts/` must ship with the game.
