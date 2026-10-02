# MacroLink plugin

This project contains the Macro Deck 3 plugin for MacroLink. It connects to the companion
Minecraft Fabric mod over WebSocket and exposes the incoming player snapshot through Macro Deck
variables. It also provides the **Minecraft XP Progress** widget.

## Required Minecraft mod

This plugin requires the MacroLink Fabric mod to be installed and running in Minecraft. Download
the mod from the [Minecraft MacroLink mod GitHub repository](https://github.com/tabsik30/Minecraft-Macro-link-Mod).
The plugin does not include the mod and cannot receive game data without it.

## Connection

The plugin connects to `ws://127.0.0.1:25599` by default and retries after disconnection. Start
Minecraft with the companion mod, then configure a different host or port through the integration's
configuration flow if needed.

## Capabilities

- Variables for connection status, health, armor, hunger, position, dimension, biome, game ticks,
  air supply, XP level, XP progress, and target information.
- A live XP progress widget with a green progress bar.

The game-time variable is the game's tick counter, not the time-of-day clock. Target values depend
on the companion mod including target data in its WebSocket snapshots.

See the repository [README](../../README.md) for setup, usage, and variable identifiers.
