#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    public class CupkekGamesPackageManagerWindow : EditorWindow
    {
        private const string FirstRunSeenKey = "CupkekGamesPackageManager_FirstRunSeen";

        private VisualElement _headerCount;
        private Button _installGameFullButton;
        private VisualElement _rowsContainer;
        private Label _errorLabel;

        private Dictionary<string, string> _installedPackages;

        [MenuItem("Tools/CupkekGames/Package Manager", false, 4)]
        public static CupkekGamesPackageManagerWindow ShowWindow()
        {
            CupkekGamesPackageManagerWindow wnd = GetWindow<CupkekGamesPackageManagerWindow>();
            wnd.titleContent = new GUIContent("CupkekGames Packages");
            wnd.minSize = new Vector2(540, 380);
            return wnd;
        }

        [InitializeOnLoadMethod]
        private static void AutoOpenIfPackagesMissing()
        {
            // Defer until editor is settled so Client.List succeeds and ProjectSettings load.
            EditorApplication.delayCall += () =>
            {
                if (EditorPrefs.GetBool(FirstRunSeenKey, false)) return;
                if (HasOpenInstances<CupkekGamesPackageManagerWindow>()) return;

                CupkekGamesPackageInstaller.GetInstalledPackages(installed =>
                {
                    int missing = 0;
                    foreach (CupkekGamesPackageRegistry.Entry e in CupkekGamesPackageRegistry.GetByTag(PackageTags.GameFull))
                    {
                        if (installed == null || !installed.ContainsKey(e.PackageId))
                        {
                            missing++;
                        }
                    }

                    if (missing > 0)
                    {
                        EditorPrefs.SetBool(FirstRunSeenKey, true);
                        ShowWindow();
                    }
                });
            };
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            StyleSheet palette = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Packages/com.cupkekgames.core/Editor/EditorColorPalette.uss");
            StyleSheet windowUss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Packages/com.cupkekgames.core/Editor/CupkekGamesPackageManagerWindow.uss");

            if (palette != null) root.styleSheets.Add(palette);
            if (windowUss != null) root.styleSheets.Add(windowUss);

            BuildHeader(root);
            BuildContent(root);

            Refresh();
        }

        private void BuildHeader(VisualElement root)
        {
            VisualElement header = new VisualElement();
            header.AddToClassList("pm-header");
            root.Add(header);

            Label title = new Label("CupkekGames Package Manager");
            title.AddToClassList("pm-header-title");
            header.Add(title);

            Label subtitle = new Label(
                "Sibling CupkekGames packages installed via Git URL. Required for the Luna GameFull sample. " +
                "core + input + luna come pre-bundled with Luna; the entries below complete the ecosystem.");
            subtitle.AddToClassList("pm-header-subtitle");
            header.Add(subtitle);
        }

        private void BuildContent(VisualElement root)
        {
            VisualElement content = new VisualElement();
            content.AddToClassList("pm-content");
            root.Add(content);

            // Toolbar row: count + Install GameFull Packages + Refresh
            VisualElement toolbar = new VisualElement();
            toolbar.AddToClassList("pm-toolbar");
            content.Add(toolbar);

            _headerCount = new VisualElement();
            _headerCount.AddToClassList("pm-toolbar-count");
            toolbar.Add(_headerCount);

            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1f;
            toolbar.Add(spacer);

            Button refreshBtn = new Button(Refresh);
            refreshBtn.text = "Refresh";
            refreshBtn.AddToClassList("pm-toolbar-btn");
            toolbar.Add(refreshBtn);

            _installGameFullButton = new Button(OnInstallGameFullPackages);
            _installGameFullButton.AddToClassList("pm-toolbar-btn");
            _installGameFullButton.AddToClassList("pm-toolbar-btn--primary");
            toolbar.Add(_installGameFullButton);

            // Scrollable rows
            ScrollView scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("pm-rows-scroll");
            content.Add(scroll);

            _rowsContainer = scroll.contentContainer;

            // Footer error
            _errorLabel = new Label();
            _errorLabel.AddToClassList("pm-error-label");
            _errorLabel.style.display = DisplayStyle.None;
            content.Add(_errorLabel);
        }

        private void Refresh()
        {
            if (_rowsContainer == null) return;

            _rowsContainer.Clear();
            Label loading = new Label("Detecting installed packages…");
            loading.AddToClassList("pm-loading-text");
            _rowsContainer.Add(loading);

            CupkekGamesPackageInstaller.GetInstalledPackages(installed =>
            {
                _installedPackages = installed;
                BuildRows();
                UpdateToolbar();
                UpdateErrorFooter();
            });
        }

        private void BuildRows()
        {
            _rowsContainer.Clear();

            CupkekGamesPackageRegistry.Entry[] entries = CupkekGamesPackageRegistry.GetByTag(PackageTags.GameFull);
            foreach (CupkekGamesPackageRegistry.Entry entry in entries)
            {
                _rowsContainer.Add(BuildRow(entry));
            }
        }

        private VisualElement BuildRow(CupkekGamesPackageRegistry.Entry entry)
        {
            bool isInstalled = _installedPackages != null && _installedPackages.ContainsKey(entry.PackageId);

            VisualElement row = new VisualElement();
            row.AddToClassList("pm-row");
            row.AddToClassList(isInstalled ? "pm-row--installed" : "pm-row--missing");

            // Status icon (not the badge — icon-sized)
            VisualElement icon = new VisualElement();
            icon.AddToClassList("pm-row-icon");
            icon.AddToClassList(isInstalled ? "pm-row-icon--installed" : "pm-row-icon--missing");
            row.Add(icon);

            // Display name
            Label displayName = new Label(entry.DisplayName);
            displayName.AddToClassList("pm-row-displayname");
            row.Add(displayName);

            // Package id (smaller, secondary)
            Label packageId = new Label(entry.PackageId);
            packageId.AddToClassList("pm-row-packageid");
            row.Add(packageId);

            // Spacer
            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1f;
            row.Add(spacer);

            if (isInstalled && _installedPackages.TryGetValue(entry.PackageId, out string version))
            {
                Label v = new Label("v" + version);
                v.AddToClassList("pm-row-version");
                row.Add(v);
            }
            else
            {
                string gitUrl = entry.GitUrl;
                Button install = new Button(() => OnInstallSingle(gitUrl));
                install.text = "Install";
                install.AddToClassList("pm-row-install-btn");
                install.tooltip = gitUrl;
                row.Add(install);
            }

            return row;
        }

        private void UpdateToolbar()
        {
            CupkekGamesPackageRegistry.Entry[] entries = CupkekGamesPackageRegistry.GetByTag(PackageTags.GameFull);
            int total = entries.Length;
            int installed = entries.Count(e =>
                _installedPackages != null && _installedPackages.ContainsKey(e.PackageId));
            int missing = total - installed;

            _headerCount.Clear();
            Label countLabel = new Label($"{installed}/{total} installed");
            countLabel.AddToClassList("pm-toolbar-count-text");
            countLabel.AddToClassList(missing == 0 ? "pm-toolbar-count-text--ok" : "pm-toolbar-count-text--missing");
            _headerCount.Add(countLabel);

            if (missing > 0)
            {
                _installGameFullButton.text = $"Install GameFull Packages ({missing})";
                _installGameFullButton.SetEnabled(true);
            }
            else
            {
                _installGameFullButton.text = "GameFull Packages Installed";
                _installGameFullButton.SetEnabled(false);
            }
        }

        private void UpdateErrorFooter()
        {
            string lastError = CupkekGamesPackageInstaller.LastError;
            if (string.IsNullOrEmpty(lastError))
            {
                _errorLabel.style.display = DisplayStyle.None;
                return;
            }
            _errorLabel.text = "Last install error: " + lastError;
            _errorLabel.style.display = DisplayStyle.Flex;
        }

        private void OnInstallSingle(string gitUrl)
        {
            CupkekGamesPackageInstaller.InstallByGitUrl(gitUrl, (ok, msg) =>
            {
                if (!ok)
                {
                    Debug.LogError($"[CupkekGames] Failed to install {gitUrl}: {msg}");
                }
                Refresh();
            });
        }

        private void OnInstallGameFullPackages()
        {
            CupkekGamesPackageRegistry.Entry[] entries = CupkekGamesPackageRegistry.GetByTag(PackageTags.GameFull);
            List<string> urls = entries
                .Where(e => _installedPackages == null || !_installedPackages.ContainsKey(e.PackageId))
                .Select(e => e.GitUrl)
                .ToList();
            if (urls.Count == 0) return;

            CupkekGamesPackageInstaller.InstallAllByGitUrls(urls, (ok, msg) =>
            {
                if (!ok)
                {
                    Debug.LogError($"[CupkekGames] Bulk install failed: {msg}");
                }
                Refresh();
            });
            // Single Client.AddAndRemove call → one manifest write, one domain reload after all packages resolve.
            // CreateGUI re-fires post-reload; the Refresh callback above is the same-session fallback if reload doesn't happen.
        }
    }
}
#endif
