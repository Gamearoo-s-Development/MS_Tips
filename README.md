# MS_Tips

BepInEx plugin for **Megastore Simulator** that adds **customer tipping** (extra checkout money) when payments complete.

## Features

- On each completed checkout, customers may give an extra random tip.
- Tip behavior is weighted by store condition signals:
	- stock availability
	- recent out-of-stock / long-wait / freshness complaints
	- recent positive greeting/success signals
- Default config values:
	- `MinTipAmountDollars = 0`
	- `MaxTipAmountDollars = 5`

## Prerequisites

- BepInEx installed in the game folder:
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

`G:\SteamLibrary\steamapps\common\Megastore Simulator\BepInEx\config\com.gamea.megastore.randomtips.cfg`

Main settings:

- `MinTipAmountDollars` (default `0`)
- `MaxTipAmountDollars` (default `5`)
- `VerboseLog` (default `false`)
- `ShowTipToast` (default `true`) shows on-screen `TIP RECEIVED +$X` when tip is added
- `TipToastSeconds` (default `2.5`) controls how long the on-screen tip message is visible

