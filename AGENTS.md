# CupkekGames Core — AI Agent Instructions

## Package Overview

**CupkekGames Core** (`com.cupkekgames.core`) is the shared foundation layer for CupkekGames packages (LunaUI and CupkekGames Systems). Pure utility code — no UI, no game systems, no external Unity package dependencies.

Both `com.cupkekgames.luna` and `com.cupkekgames.systems` depend on this. This package depends on nothing above Unity's core modules.

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
    EditorColorPalette.cs
    EditorImGuiDrawing.cs
    EditorUI/
    KeyValueDatabaseDrawer.cs
    MultiLineHeaderDrawer.cs
    SpritePreviewUtility.cs
    UI/
```

## Coding Conventions

- **Namespace**: `CupkekGames.Core` (Runtime) and `CupkekGames.Core.Editor` (Editor)
- **Asmdefs**: GUID references, not name references
- **No external dependencies**: keep it that way. If you find yourself wanting to add a dep on Luna or Systems, the type you're writing belongs there, not here.
- **Strict typing**: all C# is strictly typed

## What NOT to add here

- UI code → goes in `com.cupkekgames.luna`
- Game systems (save, inventory, dialogue, etc.) → goes in `com.cupkekgames.systems`
- Anything that depends on `UnityEngine.UIElements` beyond bare-minimum utility → Luna
- Anything game-specific (inventory items, stats, dialogue) → Systems

## Related packages

- `com.cupkekgames.luna` — UI library that depends on this
- `com.cupkekgames.systems` — game systems that depend on both this and Luna

Multi-repo dev: embed all three repos in the consumer project's `Packages/` folder. See the Luna package's `Documentation/V2_STEP1_5.md` for full architecture rationale.
