# Creating a new CupkekGames package

Reference for peeling a library out of a game project (or starting fresh) into a new sibling package under the `Cupkek-Games` GitHub org. Follow it end-to-end and the package shows up in the **Tools > CupkekGames > Package Manager** window with one-click install on consumer projects.

## 1. Create the GitHub repo

- Repo name: **`CupkekGames-<Name>`** (e.g. `CupkekGames-HeroManager`)
- Org: `Cupkek-Games`
- Default branch: `main`
- License: pick one and mirror it as `Third-Party Notices.md` in the repo root (every existing sibling does this).

## 2. Folder layout

The repo IS the Unity package. Don't nest a Unity project inside it.

```
CupkekGames-<Name>/
├─ package.json
├─ README.md
├─ AGENTS.md            ← optional, but recommended (AI agent instructions)
├─ Third-Party Notices.md
├─ Runtime/
│  ├─ CupkekGames.<Name>.asmdef
│  └─ <your runtime code>
├─ Editor/              ← optional
│  ├─ CupkekGames.<Name>.Editor.asmdef
│  └─ <your editor code>
├─ Documentation/       ← optional, longer-form .md
└─ Samples~/            ← optional, NOTE the tilde (see "Samples" below)
   └─ <SampleName>/
```

Some siblings (like `servicelocator`) wrap the package in an extra folder (`ServiceLocator/Runtime/...`) — this works but isn't required. New packages should put `Runtime/` and `Editor/` directly at the repo root for simplicity.

## 3. `package.json`

Minimum:

```json
{
  "name": "com.cupkekgames.<name>",
  "displayName": "CupkekGames <Name>",
  "version": "0.1.0",
  "author": {
    "name": "CupkekGames",
    "url": "https://www.docs.cupkek.games"
  },
  "unity": "6000.0",
  "documentationUrl": "https://docs.cupkek.games/",
  "description": "<one-line summary>",
  "keywords": ["CupkekGames", "<Domain>"],
  "dependencies": {
    "com.cupkekgames.core": "0.1.0"
  }
}
```

Notes:

- `name` is lowercase, dotted, prefixed `com.cupkekgames.`.
- `dependencies` should include `com.cupkekgames.core` unless the package genuinely needs nothing from core (rare). Add other CupkekGames siblings or Unity packages as required.
- For Asset-Store-distributed packages (only Luna today), also set `changelogUrl` and `repository`.

If you ship samples, add a `samples` array — see Luna's `package.json` for the prose-rich format.

## 4. Asmdefs

### Runtime asmdef

```json
{
  "name": "CupkekGames.<Name>",
  "references": [
    "GUID:38afbb2359e489e4fa79dc88f61428b5"
  ]
}
```

That GUID is `com.cupkekgames.core`'s Runtime asmdef. Reference any other CupkekGames packages you depend on by their GUIDs (look up the `.asmdef.meta` next to each `.asmdef`).

### Editor asmdef (optional)

```json
{
  "name": "CupkekGames.<Name>.Editor",
  "references": [
    "GUID:ab7612e7fe1d7e64ab54a584495caa06",
    "GUID:38afbb2359e489e4fa79dc88f61428b5",
    "GUID:<your runtime asmdef GUID>"
  ],
  "includePlatforms": ["Editor"]
}
```

`ab7612e7fe1d7e64ab54a584495caa06` is `CupkekGames.Core.Editor` — needed if you consume `EditorColorPalette.uss`, the package-manager registry, or any other shared editor tooling.

### Common GUIDs

| Asmdef | GUID |
|---|---|
| `CupkekGames.Core` (Runtime) | `38afbb2359e489e4fa79dc88f61428b5` |
| `CupkekGames.Core.Editor` | `ab7612e7fe1d7e64ab54a584495caa06` |
| `CupkekGames.Luna` (Runtime) | `8c5a58f4ceeaeff428a5333f02ab4313` |
| `CupkekGames.Luna.Editor` | `84651a3751eca9349aac36a66bba901b` |

If a GUID drifts, look it up in the relevant `.asmdef.meta` file.

## 5. Register in core

Add the new package to **`com.cupkekgames.core/Editor/CupkekGamesPackageRegistry.cs`** so it shows up in the Package Manager window:

```csharp
new Entry(
    "com.cupkekgames.<name>",
    "<DisplayName>",
    "https://github.com/Cupkek-Games/CupkekGames-<Name>.git",
    PackageTags.GameFull),   // or your own tag — see PackageTags
```

If the new package isn't part of GameFull, add a new tag constant in the `PackageTags` class (e.g. `public const string HeroManager = "HeroManager";`) and tag the entry with that. Bulk-install buttons filter by tag.

Order matters: leaf deps (packages others depend on) listed before consumers, so Unity resolves cleanly during bulk install.

## 6. Documentation page (optional)

If users will reach for the package directly, add a page in `luna-docs-next`:

- New file: `src/content/<name>.md` with frontmatter `title:` and an opening paragraph that names the package id (`com.cupkekgames.<name>`) and links to the [Package Manager window](/luna/architecture#package-manager-window).
- Add a sidebar entry in `src/lib/docs-menu.ts`.

## 7. Versioning + release

- Tag releases as `v<X.Y.Z>` (e.g. `v0.1.0`) on `main`.
- Consumers can pin a release by appending `#vX.Y.Z` to the Git URL — but the registry currently leaves it on `HEAD of main`.
- Bump `version` in `package.json` together with the tag.

## Samples

If the package ships samples, place them under `Samples~/<SampleName>/`. The tilde is **load-bearing**:

- With `Samples~`: Unity treats the folder as a sample and skips compiling it during normal package consumption. Users opt in via Package Manager → Samples → Import.
- Without the tilde (`Samples/`): Unity compiles it as part of the package. Used during local development inside this repo only — never ship like that.

If a sample references assets in another sibling package by absolute URL, those URLs only resolve at compile time when the target package's `Samples~/` is detilded. For embedded multi-package dev, detilde all sibling packages together.

## Extracting from a game project

If the new package starts as code inside a game project's `Assets/`:

1. Create the empty repo (steps 1–4 above).
2. Clone it into `Packages/com.cupkekgames.<name>/` of the game project.
3. **Move files with `git mv`** (or plain `mv` if untracked) — this preserves `.meta` GUIDs so existing prefab/scene references in the game keep resolving.
4. Update any namespaces that need it (e.g. `MyGame.Foo` → `CupkekGames.<Name>.Foo`).
5. Add the package's runtime asmdef ref to any game asmdefs that consume the moved code.
6. Verify the game still compiles + runs before pushing the new repo.

## Checklist

- [ ] GitHub repo `Cupkek-Games/CupkekGames-<Name>` exists, default branch `main`
- [ ] `package.json` with id, displayName, version, deps
- [ ] Runtime asmdef referencing core
- [ ] (If editor code) Editor asmdef referencing core editor
- [ ] README + AGENTS.md + Third-Party Notices
- [ ] Registered in `CupkekGamesPackageRegistry.Entries` with appropriate tag
- [ ] (If user-facing) Doc page in `luna-docs-next` + sidebar entry
- [ ] Tagged `v0.1.0` and pushed

## See also

- [`AGENTS.md`](../AGENTS.md) — core package's own structure, meta-file rules
- [`Editor/CupkekGamesPackageRegistry.cs`](../Editor/CupkekGamesPackageRegistry.cs) — where to register
- Luna's [`AGENTS.md`](../../com.cupkekgames.luna/AGENTS.md) — Asset Store distribution model + cross-package URL rules
