#if UNITY_EDITOR
namespace CupkekGames.Core.Editor
{
    public static class CupkekGamesPackageRegistry
    {
        public readonly struct Entry
        {
            public readonly string PackageId;
            public readonly string DisplayName;
            public readonly string GitUrl;

            public Entry(string packageId, string displayName, string gitUrl)
            {
                PackageId = packageId;
                DisplayName = displayName;
                GitUrl = gitUrl;
            }
        }

        // core + input are omitted — Luna's package.json > dependencies installs them transitively.
        // Append "#vX.Y.Z" to pin a tag (currently HEAD of main).
        // Order matters: leaf deps first, packages that depend on them after, so Unity resolves cleanly.
        public static readonly Entry[] GameFullDependencies = new[]
        {
            new Entry("com.cupkekgames.servicelocator",  "ServiceLocator",  "https://github.com/Cupkek-Games/CupkekGames-ServiceLocator.git"),
            new Entry("com.cupkekgames.data",            "Data",            "https://github.com/Cupkek-Games/CupkekGames-Data.git"),
            new Entry("com.cupkekgames.gamesave",        "GameSave",        "https://github.com/Cupkek-Games/CupkekGames-GameSave.git"),
            new Entry("com.cupkekgames.newtonsoft",      "Newtonsoft",      "https://github.com/Cupkek-Games/CupkekGames-Newtonsoft.git"),
            new Entry("com.cupkekgames.rpgstats",        "RPGStats",        "https://github.com/Cupkek-Games/CupkekGames-RPGStats.git"),
            new Entry("com.cupkekgames.inventory",       "Inventory",       "https://github.com/Cupkek-Games/CupkekGames-Inventory.git"),
            new Entry("com.cupkekgames.addressables",    "Addressables",    "https://github.com/Cupkek-Games/CupkekGames-Addressables.git"),
            new Entry("com.cupkekgames.scenemanagement", "SceneManagement", "https://github.com/Cupkek-Games/CupkekGames-SceneManagement.git"),
            new Entry("com.cupkekgames.sequencer",       "Sequencer",       "https://github.com/Cupkek-Games/CupkekGames-Sequencer.git"),
            new Entry("com.cupkekgames.settings",        "Settings",        "https://github.com/Cupkek-Games/CupkekGames-Settings.git"),
            new Entry("com.cupkekgames.ink",             "Ink",             "https://github.com/Cupkek-Games/CupkekGames-Ink.git"),
        };
    }
}
#endif
