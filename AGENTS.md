# CupkekGames Core — AI Agent Instructions

## Package Overview

**CupkekGames Core** (`com.cupkekgames.core`) is the shared foundation layer for all CupkekGames packages (LunaUI + 7 domain packages: data, rpgstats, inventory, addressables, sequencer, settings, ink). Pure utility code — no UI, no game systems, no external Unity package dependencies.

Every other CupkekGames package depends on this; this package depends on nothing above Unity's core modules.

## Critical: Do not hand-edit Unity serialized assets or `.meta` files

Applies to Cursor/agents and automated edits:

- **Do not** edit or create `.meta`, `.prefab`, `.unity`, `.asset`, `.controller`, `.anim`, or other Unity YAML serialized assets. Apply any inspector-visible changes via the Unity Editor.
- `.meta` GUIDs must be preserved across moves — use `git mv` (or plain `mv` when working untracked) so Unity's GUID-based references keep resolving.

## Package Structure

```
com.cupkekgames.core/
  package.json
  README.md
  AGENTS.md
  Runtime/                         ← CupkekGames.Core.asmdef
    AssetFinder/
    Attributes/
    EnumHelper.cs
    Fadeable/
    FolderReference/
    Input/                         ← InputDeviceManager, InputEscape*, IconDatabaseSO
    KeyValueDatabase/
    Pool/
    Singleton.cs
  Editor/                          ← CupkekGames.Core.Editor.asmdef
    AssetFinder/                   ← editor UI for AssetFinder
    CupkekGamesPackageInstaller.cs ← Client.AddAndRemove wrapper
    CupkekGamesPackageManagerWindow.cs / .uss ← Tools > CupkekGames > Package Manager
    CupkekGamesPackageRegistry.cs  ← sibling Git-URL list + tags
    EditorColorPalette.cs
    EditorColorPalette.uss         ← shared --editor-* tokens for every CupkekGames editor window
    EditorImGuiDrawing.cs
    EditorUI/
    KeyValueDatabaseDrawer.cs
    MultiLineHeaderDrawer.cs
    SpritePreviewUtility.cs
    UI/
  Documentation/
    CREATING_A_PACKAGE.md          ← recipe for new sibling packages
```

## Adding a new sibling package

See [Documentation/CREATING_A_PACKAGE.md](Documentation/CREATING_A_PACKAGE.md). Covers repo setup, `package.json`, asmdef GUIDs, registering in `CupkekGamesPackageRegistry.Entries`, samples (`Samples~/`), and extracting from a game project.

## Coding Conventions

- **Namespace**: `CupkekGames.Core` (Runtime) and `CupkekGames.Core.Editor` (Editor)
- **Asmdefs**: GUID references, not name references
- **No external dependencies**: keep it that way. If you find yourself wanting to add a dep on Luna or Systems, the type you're writing belongs there, not here.
- **Strict typing**: all C# is strictly typed

## What NOT to add here

- UI code → `com.cupkekgames.luna`
- Persistence / IData / save → `com.cupkekgames.data`
- Inventory / stats → `com.cupkekgames.inventory`, `com.cupkekgames.rpgstats`
- Asset loading → `com.cupkekgames.addressables`
- Scene / boot flow → `com.cupkekgames.sequencer`
- Settings panel → `com.cupkekgames.settings`
- Ink narrative → `com.cupkekgames.ink`
- Anything that depends on `UnityEngine.UIElements` → Luna or a Luna-dep package, never core

## Related packages

- `com.cupkekgames.luna` — UI library that depends on this
- `com.cupkekgames.data`, `rpgstats`, `inventory`, `addressables`, `sequencer`, `settings`, `ink` — domain packages depending on this (and most also on Luna)

Multi-repo dev: embed all 9 packages in the consumer project's `Packages/` folder. See Luna's `Documentation/ARCHITECTURE.md` for the canonical layout.
