# CupkekGames Core

Shared foundation utilities used by all CupkekGames packages (LunaUI + the domain packages: data, rpgstats, inventory, addressables, sequencer, settings, ink). Not intended for direct consumer use — you get it transitively when you install Luna or any of its sibling packages.

## What's inside

**Runtime** (`CupkekGames.Core.asmdef`)
- `Singleton<T>` — MonoBehaviour singleton base
- `Fadeable` — canvas-group fade helpers
- `Pool/` + `GameObjectPool` — object pooling
- `KeyValueDatabase<K,V>` — serializable dictionary wrapper
- `Input/` — `InputDeviceManager`, `InputEscapeManager`, `EscapeAction`, `InputIconDatabaseSO`, `InputIconControlScheme`
- `AssetFinder`, `Attributes`, `EnumHelper`, `FolderReference`

**Editor** (`CupkekGames.Core.Editor.asmdef`)
- `EditorColorPalette`, `EditorImGuiDrawing`, `KeyValueDatabaseDrawer`, `MultiLineHeaderDrawer`, `SpritePreviewUtility`, `AssetFinder` editor UI

## Dependency graph

```
com.cupkekgames.core  ← this package (no deps)
       ↑
       ├── com.cupkekgames.luna
       ├── com.cupkekgames.data
       ├── com.cupkekgames.rpgstats
       ├── com.cupkekgames.inventory
       ├── com.cupkekgames.addressables
       ├── com.cupkekgames.sequencer
       ├── com.cupkekgames.settings
       └── com.cupkekgames.ink
```

See Luna's [Documentation/ARCHITECTURE.md](../com.cupkekgames.luna/Documentation/ARCHITECTURE.md) for the full dep graph.

## Installation

Embedded package — clone the repo into your project's `Packages/` folder alongside `com.cupkekgames.luna`.

## Related packages

- `com.cupkekgames.luna` — pure UI library (depends on this)
- `com.cupkekgames.data`, `rpgstats`, `inventory`, `addressables`, `sequencer`, `settings`, `ink` — domain packages built on this and Luna
