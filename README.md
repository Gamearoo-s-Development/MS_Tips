# MS_Tips

BepInEx plugin for **Megastore Simulator** that adds **customer tipping** (extra checkout money) when payments complete.

## Features

- On each completed checkout, customers may give an extra random tip.
- Tip behavior is weighted by store condition signals:
	- stock availability
	- recent out-of-stock / long-wait / freshness complaints
	- recent positive greeting/success signals
- Low chance is still possible, but tips are guaranteed every so often with a no-tip streak safety setting.
- Default config values:
	- `MinTipAmountDollars = 0`
	- `MaxTipAmountDollars = 5`
	- `BaseTipChance = 0.12`
	- `ServiceScoreWeight = 0.60`
	- `NoTipStreakBonusPerCheckout = 0.03`
	- `MaxNoTipCheckouts = 8`
	- `VerboseLog = false`
	- `ShowTipToast = true`
	- `TipToastSeconds = 2.5`

## Prerequisites

- BepInEx installed in the game folder: [Tobey's BepInEx Pack for Megastore Simulator](https://www.nexusmods.com/megastoresimulator/mods/2)
	- `G:\SteamLibrary\steamapps\common\Megastore Simulator`
- .NET SDK installed (`dotnet` CLI).

## Build

From this repo folder (`MS_Tips`):

```powershell
dotnet build -c Release
```

If your game is in a different path, override the `GameDir` property:

```powershell
dotnet build -c Release -p:GameDir="D:\Games\Megastore Simulator"
```

## Install

Copy the built DLL to:

`G:\SteamLibrary\steamapps\common\Megastore Simulator\BepInEx\plugins\`

Expected DLL output path:

`MS_Tips\bin\Release\net472\MS_Tips.dll`

## Config

After first launch, edit:

`G:\SteamLibrary\steamapps\common\Megastore Simulator\BepInEx\config\com.gamearoo.megastore.randomtips.cfg`

Main settings:

- `MinTipAmountDollars` (default `0`)
- `MaxTipAmountDollars` (default `5`)
- `BaseTipChance` (default `0.12`)
- `ServiceScoreWeight` (default `0.60`)
- `NoTipStreakBonusPerCheckout` (default `0.03`)
- `MaxNoTipCheckouts` (default `8`) guarantees a tip after this many no-tip checkouts
- `VerboseLog` (default `false`)
- `ShowTipToast` (default `true`) shows on-screen `TIP RECEIVED +$X` when tip is added
- `TipToastSeconds` (default `2.5`) controls how long the on-screen tip message is visible

### What each config does

- `MinTipAmountDollars`
	- Lowest possible tip amount in dollars.
	- If set to `0`, a customer can still decide to leave no tip unless the streak guarantee forces one.

- `MaxTipAmountDollars`
	- Highest possible tip amount in dollars.
	- If this is lower than `MinTipAmountDollars`, the mod automatically uses `MinTipAmountDollars` for both.

- `BaseTipChance`
	- Starting tip chance every checkout before other bonuses/penalties are applied.
	- Range is `0.0` to `1.0` (for example, `0.12` = 12%).

- `ServiceScoreWeight`
	- How strongly store/customer conditions affect tip chance.
	- Higher values make stock/complaints/greeting signals matter more.

- `NoTipStreakBonusPerCheckout`
	- Extra chance added after each checkout that gives no tip.
	- Helps prevent long dry streaks while still keeping randomness.

- `MaxNoTipCheckouts`
	- Hard safety limit for no-tip streaks.
	- When reached, the next checkout is forced to give a tip (if max tip is above 0).

- `VerboseLog`
	- Writes detailed tip-roll info to `LogOutput.log` (score, chance, streak, etc.).
	- Useful for balancing and debugging.

- `ShowTipToast`
	- Shows/hides the in-game popup when a tip is awarded.
	- Popup includes current tip and running `Tips Total`.

- `TipToastSeconds`
	- How long the tip popup stays visible on screen.
	- Increase this if you want more time to notice tips.

