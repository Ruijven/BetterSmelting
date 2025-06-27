# Better Smelting

A Valheim mod that enhances the smelting experience with configurable ore and fuel capacities, smelting speed, and blast furnace functionality.

## Features

- **Configurable Ore Capacity**: Increase the maximum amount of ore that can be loaded into Smelters and Blast Furnaces
- **Configurable Fuel Capacity**: Increase the maximum amount of coal/wood that can be loaded as fuel
- **Adjustable Smelting Speed**: Speed up or slow down the smelting process with a multiplier
- **Blast Furnace All Ores**: Optionally allow all ore types to be smelted in the Blast Furnace
- **Server-Synced Configuration**: All settings are synchronized between server and clients

## Installation

1. Install [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)
2. Download the latest release of BetterSmelting
3. Extract the contents of the zip file into your Valheim `BepInEx/plugins` folder

## Configuration

All configuration can be found in `BepInEx/config/ruijven.BetterSmelting.cfg`. You can edit this file directly or use the in-game configuration manager.

### Smelter Settings
- **MaxOre**: Maximum amount of ore that can be loaded into a Smelter (default: 10, range: 10-100)
- **MaxFuel**: Maximum amount of fuel that can be loaded into a Smelter (default: 20, range: 20-100)

### Blast Furnace Settings
- **MaxOre**: Maximum amount of ore that can be loaded into a Blast Furnace (default: 10, range: 10-100)
- **MaxFuel**: Maximum amount of fuel that can be loaded into a Blast Furnace (default: 20, range: 20-100)

### Features
- **AllOresInBlastFurnace**: If enabled, all ore types can be smelted in the Blast Furnace (default: true)
- **SmeltingSpeedMultiplier**: Multiplier for smelting speed (higher values = faster smelting) (default: 1.0, range: 0.1-10.0)

## Server Installation

For dedicated servers, place the mod in the same `BepInEx/plugins` folder as you would for a client. The configuration will be synchronized to all connected clients.

## Compatibility

- Compatible with most mods that don't modify the smelting system
- Server-side configuration is enforced on all clients

## Support

For support, bug reports, or feature requests, please visit our Discord:

[![Discord](https://img.shields.io/discord/your-invite-code?logo=discord)](https://discord.gg/qGf5FdqhVC)

## Changelog

### v1.0.0
- Initial release

## License

This mod is licensed under the MIT License.