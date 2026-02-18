# Megastore Checkout Tipping (BepInEx)

Give your customers a chance to leave gratuity after checkout in **Megastore Simulator**.

This mod adds random tip income on completed payments, scales chance by store conditions, and includes a streak safety system so tips still happen every so often.

---

## Features

- Adds extra tip money after `CASH_PAYMENT_DONE` and `POS_PAYMENT_DONE`
- Random tip amount with configurable min/max (default `$0` to `$5`)
- Tip chance affected by store/service conditions:
  - stock availability
  - recent complaints (out-of-stock, long wait, freshness concern)
  - positive service moments
- No-tip streak system:
  - increasing bonus chance after misses
  - guaranteed tip after a configurable max streak
- In-game tip popup:
  - `TIP RECEIVED +$X`
  - running `Tips Total` directly underneath
- Optional verbose logging for balancing/debugging

---

## Requirements

- **Megastore Simulator**
- **BepInEx 5.x** installed in the game folder

---

## Installation

1. Install BepInEx (5.x) for the game.
2. Build or download `MS_Tips.dll`.
3. Copy `MS_Tips.dll` to:

   `Megastore Simulator\BepInEx\plugins\`

4. Launch the game once to generate config.
5. Edit config at:

   `Megastore Simulator\BepInEx\config\com.gamearoo.megastore.randomtips.cfg`

---

## Configuration

### Core Tipping

- `MinTipAmountDollars` (default `0`)
  - Lowest possible tip value.

- `MaxTipAmountDollars` (default `5`)
  - Highest possible tip value.

- `BaseTipChance` (default `0.12`)
  - Base tip chance per checkout before modifiers.

- `ServiceScoreWeight` (default `0.60`)
  - How strongly service/store conditions affect tip chance.

### Anti-Dry-Streak Safety

- `NoTipStreakBonusPerCheckout` (default `0.03`)
  - Additional tip chance added for each no-tip checkout in a row.

- `MaxNoTipCheckouts` (default `8`)
  - Guarantees a tip when this many no-tip checkouts are reached.

### Display / Logging

- `ShowTipToast` (default `true`)
  - Shows in-game popup when tip is awarded.

- `TipToastSeconds` (default `2.5`)
  - Popup duration.

- `VerboseLog` (default `false`)
  - Writes detailed tip-roll info to `BepInEx\LogOutput.log`.

---

## Recommended Presets

### Casual (more frequent tips)

- `BaseTipChance = 0.20`
- `ServiceScoreWeight = 0.70`
- `NoTipStreakBonusPerCheckout = 0.05`
- `MaxNoTipCheckouts = 4`

### Balanced (default-like)

- `BaseTipChance = 0.12`
- `ServiceScoreWeight = 0.60`
- `NoTipStreakBonusPerCheckout = 0.03`
- `MaxNoTipCheckouts = 8`

### Harsh Economy (rarer tips)

- `BaseTipChance = 0.05`
- `ServiceScoreWeight = 0.45`
- `NoTipStreakBonusPerCheckout = 0.02`
- `MaxNoTipCheckouts = 12`

---

## Notes

- If your tip chance is very low, no-tip streaks are expected. The streak safety settings are designed to prevent extremely long dry runs.
- If you update from older versions, make sure your config file key names match the current mod version.

---

## Version

Current documented behavior: **v1.2.0**

---

## Credits

Created by **Gamearoo-s-Development**.
