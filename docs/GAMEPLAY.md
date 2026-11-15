# Petal — Gameplay Loop Design

> Status: draft v0.3 · Engine: Godot 4.7 (Mobile renderer) · Target: touch-first, also playable with mouse · Art: 2D, Ghibli-inspired

You run a small florist shop. Every day you grow flowers in your greenhouse and turn them into bouquets for the people of your little town. The money you earn buys upgrades, new seeds and themes. There's no way to lose and no end goal: the game is meant to be played at your own pace, as long as you like.

---

## 1. Design Pillars

1. **Chill, always.** There are no running costs, no debt and no game over. Nothing in the greenhouse runs on a timer, and you can close the shop whenever you like.
2. **Plan ahead, sell today.** Flowers take days to grow but wilt one day after harvest. The fun is matching what you grow to who walks in.
3. **Creative bouquets, readable scoring.** Players get to express themselves, and the game always explains why a customer was happy or not.
4. **Collect and decorate, not chase numbers.** Long-term motivation comes from unlocking flowers, filling a flower journal and making the shop your own. There are no ratings or scores.

---

## 1.1 Art Direction

**2D, inspired by Studio Ghibli's look** (our own original art, not copied characters or scenes):

- Soft hand-painted backgrounds with watercolour textures and visible brush strokes.
- Warm, natural light: morning sun in the greenhouse, golden late-afternoon light in the shop, a cosy lamp-lit evening for the summary.
- Lush, detailed plants against simple, clean character shapes with gentle expressions.
- Small living details: dust in sunbeams, leaves swaying, a cat asleep on the windowsill, steam from a teacup on the desk.
- A palette of muted greens, creams and sky blues, with the flowers as the brightest, most saturated things on screen so the bouquets stand out.
- Customers are illustrated townsfolk drawn from a mix-and-match set of parts (faces, hair, clothes) so customers look varied.

---

## 2. Main Menu

| Button | Behaviour |
|---|---|
| **Play** | Opens a save-slot screen with 3 slots. An empty slot starts a new game; a filled slot shows *Day N · $money* and has **Continue** or **Delete**. |
| **Settings** | Language

The shop is always called **Petal**. A new game goes straight into a short tutorial day (see §10).

**Saving:** the game autosaves at the **start of each day** (after the upgrade shop). Quitting mid-day sends you back to that morning.

---

## 3. The Daily Loop

```
 ┌──────────────┐   ┌──────────────┐   ┌──────────────┐   ┌──────────────┐
 │  1. MORNING  │──▶│  2. SHOP     │──▶│  3. EVENING  │──▶│ 4. UPGRADES  │──┐
 │  Greenhouse  │   │  Customers   │   │  Closing &   │   │  Spend money │  │
 │  (untimed)   │   │  (clock runs)│   │  day summary │   │              │  │
 └──────────────┘   └──────────────┘   └──────────────┘   └──────────────┘  │
		▲                                                                    │
		└───────────────────────────── next day ─────────────────────────────┘
```

**Why players won't just skip days:** there's no punishment for it. A day with the shop closed is simply a day with no income, so waiting a week for an orchid means missing a week of earnings. On top of that, advance orders (§8) come with a day they're needed by, so it pays to keep the shop open.

---

## 4. Phase 1 — Morning: The Greenhouse

The greenhouse is a grid of **plots** (start: 6 plots, 2×3). No time limit; press **"Open Shop"** when you're done.

### 4.1 Plot states

```
EMPTY ──plant──▶ SEEDLING ──(days)──▶ GROWING ──(days)──▶ READY ──harvest──▶ EMPTY
					│                     │                   │
					└──── not watered ────┘                   └── not harvested for 2 days ──▶ WITHERED
							   │
							   ▼
							 DEAD
```

- **Not watered:** a SEEDLING or GROWING plant that isn't watered in the morning **dies** that night. The seed and any fertilizer used on it are lost.
- **Not harvested:** a READY plant can be harvested on the day it ripens or the day after. If it's still not picked after 2 days it **withers** and yields nothing.
- DEAD and WITHERED plots must be **cleared** before you can plant there again.

### 4.2 Actions

| Action | Cost | Effect |
|---|---|---|
| **Plant** | Seed price (varies) | Pick a seed from your seed stock and drop it in an empty plot. |
| **Water** | Free | Every SEEDLING or GROWING plant must be watered **each morning**, or it dies. |
| **Fertilize** | Fertilizer item (bought) | Choose one per planting: **Growth** (−1 day to maturity, min 1) or **Bloom** (+1 quality tier at harvest). |
| **Harvest** | Free | Moves the stems into today's inventory. |
| **Clear** | Free | Removes a dead, withered or unwanted plant. |

### 4.3 Seed & supply stall

A small stall in the corner of the greenhouse sells seeds and fertilizer. It always stocks **every seed you've unlocked**, in any amount, at fixed prices. For flowers that come in several colours, you **pick the colour** when buying (e.g. *Tulip seeds — red*). Each colour is a separate seed item in your stock. There's no random rotation, so you can plan exactly what to grow.

### 4.4 Flowers

Each flower has:

- **Colours:** the colours this flower comes in, chosen from red, pink, white, yellow, orange, purple, blue and green. Some flowers have only one colour (daisy, sunflower, lavender); others have several (tulip, rose).
- **Grow time:** in days
- **Yield:** stems per harvest
- **Base value:** what one stem is worth in a bouquet
- **Tags:** the meanings and moods customers ask for, e.g. `romantic`, `cheerful`, `sympathy`, `elegant`, `wild`, `celebration`
- **Quality:** ★ / ★★ / ★★★, decided at harvest by care (Bloom fertilizer, upgrades, harvesting on the day it ripens rather than the day after)

Starter catalogue (examples to balance later):

| Flower | Colours | Grow | Yield | Seed $ | Base $ | Tags | How to unlock |
|---|---|---|---|---|---|---|---|
| Daisy | white | 1 | 4 | 2 | 2 | cheerful, wild | start |
| Tulip | red, pink, yellow, purple | 2 | 3 | 4 | 4 | cheerful, romantic | start |
| Carnation | red, pink, white | 2 | 3 | 4 | 4 | celebration, sympathy | start |
| Fern / greenery | green | 1 | 5 | 1 | 1 | filler (fits everything) | start |
| Sunflower | yellow | 3 | 1 | 6 | 10 | cheerful, celebration | Seed catalog: Garden |
| Rose | red, pink, white | 3 | 2 | 8 | 9 | romantic, elegant | Seed catalog: Garden |
| Lily | white, orange, pink | 3 | 2 | 9 | 10 | sympathy, elegant | Seed catalog: Classic |
| Lavender | purple | 2 | 4 | 5 | 4 | calm, wild | Seed catalog: Wildflower |
| Peony | pink, white | 4 | 2 | 14 | 16 | romantic, celebration | Seed catalog: Classic |
| Orchid | purple, white | 5 | 1 | 25 | 35 | elegant, rare | Heater upgrade |
| Blue Rose | blue | 6 | 1 | 40 | 60 | romantic, rare | Heater + completing the Exotic journal set (§8) |

### 4.5 Inventory rule

Harvested stems go into **today's inventory** and wilt at closing. Seeds and fertilizer keep indefinitely. The trade-off: harvest only what you expect to sell, and leave the rest on the plant for tomorrow (READY plants wither after 2 days). The **cold display fridge** upgrade (§7) is the one exception.

---

## 5. Phase 2 — The Shop

Pressing **Open Shop** moves you to the shop scene: a front desk, the customer standing behind it, and your flower bucket (inventory) beside it.

### 5.1 Shop clock

- The shop is open **9:00–17:00**.
- The clock **only moves on actions:** a customer arriving takes 30 min, a finished bouquet 60 min), a decline 10 min. Nothing ticks while you think.
- **Close Shop** is always available. Closing early is fine when your bucket is empty.

### 5.2 Customer arrival

One customer at a time comes up to the desk. The customers spawn indefinitely, until the shop closes. Customers are randomly generated townsfolk. Each has:

- **Request:** what they want (see below)
- **Budget:** a rough figure the player can see as `$`, `$$` or `$$$`
- **Personality:** affects what they care about most (e.g. *Romantic* likes colour harmony, *Practical* cares about price, *Collector* wants rare flowers)

### 5.3 Request types

| Type | Example | How it's judged |
|---|---|---|
| **Specific** | "Five red roses, please." | Exact flowers and count. Easy to read, hard to bend. |
| **Partial** | "Something with tulips, mostly yellow." | Must include the named flower plus a colour mood. |
| **Vague (occasion)** | "My grandma just got out of hospital." | Matched to tags: `cheerful`, `calm`. |
| **Vague (mood)** | "Something wild, like a meadow." | Tags plus a variety bonus. |
| **Wildcard** | "Surprise me!" | Judged on harmony, quality and size only. Generous tips. |

The mix shifts with how many flowers you've unlocked. Early on most requests are Specific, which teaches the flowers; once your catalogue grows, more Vague and Collector requests show up. Customers only ever ask for flowers you've unlocked.

### 5.4 Accept or decline

- **Accept** → bouquet crafting (§5.5).
- **Decline** → the customer leaves politely. The only cost is 10 minutes of shop time.

The **Accept** button shows a hint when you can't fulfil a Specific request ("You have no roses"), so you don't have to dig through your inventory.

### 5.5 Bouquet crafting

A crafting screen with three steps. You can go back to any step before handing it over.

1. **Flowers:** drag stems from the bucket into the bouquet, up to the wrap's stem limit. The bouquet preview updates live.
2. **Wrap:** paper or cellophane (free basic brown paper; nicer wraps cost $ and add style).
3. **Decorations:** ribbon, card, glitter, baby's breath spray and so on. Each costs a little money and adds a small bonus to satisfaction. Some customers love certain decorations ("Ooh, a card!").

Then **Hand over**.

### 5.6 Satisfaction & payment

Satisfaction is scored 0–100 from weighted parts (the weights shift with personality):

| Component | What it checks |
|---|---|
| **Request match** | Right flowers and counts (Specific), or how well the tags fit (Vague). This is the biggest weight. |
| **Colour harmony** | Do the colours go together? (complementary, analogous, all one colour = good; clashing = bad) |
| **Quality** | Average star rating of the stems. |
| **Size vs budget** | Too skimpy for a `$$$` customer is bad; a huge bouquet for a `$` customer is wasted value (they won't pay more than their budget). |
| **Decorations** | Small bonus, larger for customers who like them. |

**Payment** = `min(bouquet value, budget cap) × satisfaction multiplier + tip`

| Satisfaction | Reaction | Multiplier | Tip |
|---|---|---|---|
| 90–100 | 😍 "It's perfect!" | ×1.3 | yes |
| 70–89 | 😊 | ×1.0 | small |
| 40–69 | 😐 | ×0.8 | — |
| 0–39 | 😕 | ×0.6 | — |

Even a poor bouquet still earns something, because a bad result shouldn't feel like a punishment. The customer always says **one line of feedback** about the strongest or weakest part ("Love the colours!", "I asked for roses…"), so players learn the system.

---

## 6. Phase 3 — Evening: Closing & Day Summary

When the clock reaches closing time or the player presses **Close Shop**:

1. Unused stems wilt and are thrown out (or go into the compost bin if you have one, §7). The summary lists them gently as "Leftover flowers", with no money value attached.
2. A short **day summary card** shows: customers served, money earned, and new journal entries.

Money can never go below zero: there are no bills, so the worst case is a day with less income.

---

## 7. Phase 4 — Upgrade Shop

Opens after the summary. It has four tabs, and every upgrade is a one-time purchase unless marked *tiered*.

**🏪 Shop**

| Upgrade | Effect |
|---|---|
| Premium wrap supplier | Unlocks better wraps and decorations |
| Shop bell | Shows the next customer's request type before they reach the desk |
| Cold display fridge (tiered) | Keeps up to 3 / 6 / 10 stems for **one extra day** |
| Order board | Customers can place bigger orders **days in advance** (§8) |

**🌱 Greenhouse**

| Upgrade | Effect |
|---|---|
| Extra plots (tiered) | +2 plots per tier |
| Sprinkler (tiered) | Waters a row automatically each morning |
| Rich soil | +1 base quality to all plots |
| Heater | Lets you grow `rare` flowers |
| Compost bin | Wilted stems at closing become fertilizer |

**📖 Seed catalogs**

| Catalog | Unlocks |
|---|---|
| Garden | Sunflower, Rose |
| Classic | Lily, Peony |
| Wildflower | Lavender, Cornflower, Poppy, Cosmos |
| Exotic (needs Heater) | Orchid, Protea, Bird of Paradise |

**🎨 Themes** (cosmetic, with a small optional flavour bonus)

| Theme | Look | Bonus |
|---|---|---|
| Cottage | Wood, warm light | Default |
| Parisian | Pastel, wrought iron | Elegant customers tip a bit more |
| Botanical | Jungle green | Wild requests appear more often |
| Modern | White, concrete | Collector customers appear more often |

Themes can be swapped for free once bought.

---

## 8. Long-Term Play

The game has no ending, so long-term motivation comes from **unlocking, collecting and decorating**, not from a score.

### 8.1 Advance orders (with the Order board upgrade)

Each morning there's a chance of a note pinned to the order board: *"Wedding in 4 days, I'll need 10 white roses."* These orders pay well and give players a reason to plant rare or slow flowers on purpose. If you miss the day, there's no penalty; the note just says "No worries, we found something else!" and you miss the bonus.

### 8.2 Flower journal

Every flower, colour variant and quality level you harvest gets a page in the journal, along with a hand-painted pressed-flower illustration and a short fact about its meaning. Completing a set of pages (e.g. all Classic flowers) gives a reward: a decoration, a theme piece or, for the Exotic set, Blue Rose seeds.

### 8.3 Bouquet album

You can save **favourite bouquets** to a photo album, and pin a few of them as framed pictures on the shop wall.

---

## 9. Economy Sanity Check (early game)

- Starting money: **$100**, plus a starter pack of 6 daisy, 2 red tulip, 2 yellow tulip and 2 fern seeds.
- Typical early bouquet: 5–7 stems ≈ $15–30. About 4 customers a day ≈ **$80/day**, minus a few $ for seeds.
- First catalog or upgrade (~$100–200) should be affordable within 2–3 days, so the early game has quick, frequent rewards.
- Later upgrades cost more ($500–2000) to stretch progression over weeks of play.
- Rare flowers pay off best through advance orders or Collector customers, so growing them is a planned project rather than a gamble.

All numbers are placeholders. Keep them in a data file (e.g. `flowers.json` / Godot `Resource`s) so balancing doesn't touch code.

---

## 10. Tutorial Day (Day 0)

1. The greenhouse has one READY row of daisies and tulips. The game teaches harvest → water → plant.
2. The shop has 3 scripted customers: one Specific, one Vague, and one Wildcard you're meant to decline, to show that declining is fine.
3. A short day, then the summary.
4. The upgrade shop is shown with one cheap upgrade affordable.

---

## 11. Parked Ideas (not in the first version)

- **Regulars:** named townsfolk with small storylines who come back over many days and give gifts.
- **Seasons:** a 28-day cycle that changes seed specials, holiday requests (Valentine's, Mother's Day) and seasonal themes.
