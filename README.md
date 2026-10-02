# MacroLink for Minecraft

MacroLink connects Macro Deck 3 to the MacroLink client-side Minecraft mod. The mod streams
player snapshots over a local WebSocket connection; this plugin exposes those values as Macro Deck
variables and provides a live Minecraft XP progress widget.

The plugin reconnects automatically when Minecraft is not running and after its connection settings
are changed. It connects to `127.0.0.1:25599` by default.

## Features

- Connection status, health, armor, hunger, coordinates, dimension, biome, game ticks, air supply,
  XP level, and target name/type as Macro Deck variables.
- A **Minecraft XP Progress** widget with a live green progress bar and percentage.
- A configurable WebSocket host and port.

The `mc-game-time` value is the game's tick counter, not the in-game time-of-day clock. Target values
are available when the companion mod includes target information in its snapshots.

## Requirements

- Macro Deck 3.
- Minecraft with Fabric and the companion MacroLink mod installed.
- .NET SDK 10.0 to build the plugin from source.

The companion mod is maintained separately:
[Minecraft MacroLink mod](https://github.com/tabsik30/Minecraft-Macro-link-Mod). Install the mod
and its required Fabric dependencies, then start Minecraft before using the plugin. This mod is a
required dependency: the plugin cannot receive Minecraft data without it running.

## Use the plugin

1. Install MacroLink for Minecraft in Macro Deck 3 using its Creator Portal listing.
2. Install the companion Fabric mod in Minecraft and start a world.
3. In Macro Deck, open the plugin's configuration and set the WebSocket host and port if they differ
   from `127.0.0.1` and `25599`.
4. Use the plugin's variables in buttons, conditions, and other Macro Deck flows, or add
   **Minecraft XP Progress** to a deck.

The variable identifiers and types are:

| Variable | Type | Value |
| --- | --- | --- |
| `mc-connected` | Boolean | Whether the plugin is connected to the mod |
| `mc-health` | Text | Current health |
| `mc-max-health` | Text | Maximum health |
| `mc-armor` | Text | Armor |
| `mc-hunger` | Text | Hunger |
| `mc-x`, `mc-y`, `mc-z` | Text | Player coordinates |
| `mc-dimension` | Text | Dimension, without the `minecraft:` prefix |
| `mc-biome` | Text | Biome, without the `minecraft:` prefix |
| `mc-game-time` | Text | Game tick counter |
| `mc-air`, `mc-max-air` | Text | Current and maximum air supply |
| `mc-xp-level` | Text | XP level |
| `mc-xp-progress` | Numeric | XP bar progress as a rounded percentage from 0 to 100 |
| `mc-target-name` | Text | Target name, or an empty value when there is no target |
| `mc-target-type` | Text | Target type, or `none` when there is no target |

## Build and test

From the repository root:

```bash
dotnet build
dotnet test
```

The Macro Deck packages use the `3.0.0-beta.14` SDK baseline, configured centrally in
[`Directory.Packages.props`](Directory.Packages.props). The application is currently packaged for
Windows x64.

## Build a plugin package

Install the Macro Deck plugin CLI, then build the Windows x64 plugin package. The build configuration
in `src/MacroLink/macrodeck-build.json` supplies the self-contained publish settings.

```bash
dotnet tool install --global MacroDeck.Plugin.Cli --prerelease
macrodeck-plugin build --source src/MacroLink --output publish
macrodeck-plugin inspect --artifact publish/com.tabsik12.minecraft-macrolink-1.0.0.macroDeckPlugin
```

## Release

The GitHub Actions release workflow runs when a GitHub Release is published. It builds and uploads
the plugin to the Macro Deck Creator Portal build library. The uploaded build must then be selected
and released from the Creator Portal.

## License

MIT. See [LICENSE](LICENSE).
