# CupkekGames Core

Shared foundation utilities used by **LunaUI** (`com.cupkekgames.luna`) and **CupkekGames Systems** (`com.cupkekgames.systems`). Not intended for direct consumer use — you get it transitively when you install Luna or Systems.

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
       └── com.cupkekgames.systems → depends on luna
```

## Installation

Embedded package — clone the repo into your project's `Packages/` folder alongside `com.cupkekgames.luna`.

## Related packages

- `com.cupkekgames.luna` — pure UI library (depends on this)
- `com.cupkekgames.systems` — game systems (depends on this + luna)
