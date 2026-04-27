#if UNITY_EDITOR
using System.Linq;

namespace CupkekGames.Core.Editor
{
    /// <summary>Tag constants used by <see cref="CupkekGamesPackageRegistry.Entry.Tags"/>.</summary>
    public static class PackageTags
    {
        /// <summary>Required by the Luna GameFull sample.</summary>
        public const string GameFull = "GameFull";
    }

    public static class CupkekGamesPackageRegistry
    {
        public readonly struct Entry
        {
            public readonly string PackageId;
            public readonly string DisplayName;
            public readonly string GitUrl;
            public readonly string[] Tags;

            public Entry(string packageId, string displayName, string gitUrl, params string[] tags)
            {
                PackageId = packageId;
                DisplayName = displayName;
                GitUrl = gitUrl;
                Tags = tags ?? System.Array.Empty<string>();
            }

            public bool HasTag(string tag)
            {
                if (Tags == null) return false;
                for (int i = 0; i < Tags.Length; i++)
                    if (Tags[i] == tag) return true;
                return false;
            }
        }

        // core + input are omitted — Luna's package.json > dependencies installs them transitively.
        // Append "#vX.Y.Z" to a Git URL to pin a specific release (currently HEAD of main).
        // Order matters: leaf deps first, packages that depend on them after, so Unity resolves cleanly.
        public static readonly Entry[] Entries = new[]
        {
            new Entry("com.cupkekgames.servicelocator",  "ServiceLocator",  "https://github.com/Cupkek-Games/CupkekGames-ServiceLocator.git",  PackageTags.GameFull),
            new Entry("com.cupkekgames.data",            "Data",            "https://github.com/Cupkek-Games/CupkekGames-Data.git",            PackageTags.GameFull),
            new Entry("com.cupkekgames.gamesave",        "GameSave",        "https://github.com/Cupkek-Games/CupkekGames-GameSave.git",        PackageTags.GameFull),
            new Entry("com.cupkekgames.newtonsoft",      "Newtonsoft",      "https://github.com/Cupkek-Games/CupkekGames-Newtonsoft.git",      PackageTags.GameFull),
            new Entry("com.cupkekgames.rpgstats",        "RPGStats",        "https://github.com/Cupkek-Games/CupkekGames-RPGStats.git",        PackageTags.GameFull),
            new Entry("com.cupkekgames.inventory",       "Inventory",       "https://github.com/Cupkek-Games/CupkekGames-Inventory.git",       PackageTags.GameFull),
            new Entry("com.cupkekgames.addressables",    "Addressables",    "https://github.com/Cupkek-Games/CupkekGames-Addressables.git",    PackageTags.GameFull),
            new Entry("com.cupkekgames.scenemanagement", "SceneManagement", "https://github.com/Cupkek-Games/CupkekGames-SceneManagement.git", PackageTags.GameFull),
            new Entry("com.cupkekgames.sequencer",       "Sequencer",       "https://github.com/Cupkek-Games/CupkekGames-Sequencer.git",       PackageTags.GameFull),
            new Entry("com.cupkekgames.settings",        "Settings",        "https://github.com/Cupkek-Games/CupkekGames-Settings.git",        PackageTags.GameFull),
            new Entry("com.cupkekgames.ink",             "Ink",             "https://github.com/Cupkek-Games/CupkekGames-Ink.git",             PackageTags.GameFull),
        };

        /// <summary>Entries with the given tag, in registration order.</summary>
        public static Entry[] GetByTag(string tag)
            => Entries.Where(e => e.HasTag(tag)).ToArray();
    }
}
#endif
