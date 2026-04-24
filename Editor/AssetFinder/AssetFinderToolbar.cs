#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// Configuration for the asset finder toolbar.
    /// </summary>
    public class AssetFinderToolbarConfig
    {
        /// <summary>
        /// The type of assets to find.
        /// </summary>
        public Type AssetType;

        /// <summary>
        /// Optional: The type for keys if adding to a key-value database.
        /// </summary>
        public Type KeyType;

        /// <summary>
        /// Custom function to extract key from an asset.
        /// </summary>
        public Func<Object, object> GetKeyFromAsset;

        /// <summary>
        /// Optional: Custom persistence key. If not set, uses AssetType name.
        /// </summary>
        public string PersistenceKey;
    }

    /// <summary>
    /// Reusable toolbar component for finding and adding assets to lists.
    /// Manages filters internally with automatic EditorPrefs persistence.
    /// </summary>
    public class AssetFinderToolbar : VisualElement
    {
        private const string PrefsKeyPrefix = "AssetFinderToolbar_";
        private const string FoldoutPrefsKeySuffix = "_Foldout";

        public event Action<List<Object>> OnAssetsFound;
        public event Action OnClear;

        private readonly AssetFinderToolbarConfig _config;
        private readonly string _persistenceKey;
        private readonly List<AssetFinderFilterConfig> _filters = new();
        private FoldoutSection _filterSection;

        public AssetFinderToolbar(AssetFinderToolbarConfig config)
        {
            _config = config;
            _persistenceKey = PrefsKeyPrefix + (config.PersistenceKey ?? config.AssetType?.FullName ?? "default");
            LoadFilters();
            BuildUI();
        }

        /// <summary>
        /// Snapshot of current filters for use with <see cref="AssetFinder.FindAssets{T}(System.Collections.Generic.List{AssetFinderFilterConfig})"/>.
        /// </summary>
        public List<AssetFinderFilterConfig> GetFiltersCopy() => new List<AssetFinderFilterConfig>(_filters);

        private void BuildUI()
        {
            style.marginTop = 8f;
            style.marginLeft = 4f;
            style.marginRight = 4f;
            style.paddingTop = 4f;
            style.borderTopWidth = 1f;
            style.borderTopColor = EditorUIElements.Colors.Border;

            _filterSection = new FoldoutSection(
                GetFiltersLabel(),
                _persistenceKey + FoldoutPrefsKeySuffix);

            // Header buttons (right side)
            var copyButton = EditorUIElements.CreateIconButton("C", null, CopyFilters);
            copyButton.tooltip = "Copy filters";
            _filterSection.HeaderRight.Add(copyButton);

            var pasteButton = EditorUIElements.CreateIconButton("P", null, PasteFilters);
            pasteButton.tooltip = "Paste filters";
            _filterSection.HeaderRight.Add(pasteButton);

            var addFilterButton = EditorUIElements.CreateIconButton("+", null, ShowAddFilterMenu);
            addFilterButton.tooltip = "Add filter";
            _filterSection.HeaderRight.Add(addFilterButton);

            var findButton = EditorUIElements.CreatePrimaryButton("Find", OnFindClicked);
            findButton.style.width = 50f;
            findButton.style.marginLeft = 4f;
            findButton.style.borderLeftWidth = 0;
            _filterSection.HeaderRight.Add(findButton);

            var clearButton = EditorUIElements.CreateIconButton("×", null, () => OnClear?.Invoke());
            clearButton.tooltip = "Clear list";
            clearButton.style.marginLeft = 4f;
            _filterSection.HeaderRight.Add(clearButton);

            Add(_filterSection);

            // Rebuild filter UI
            RebuildFilterList();
        }

        private string GetFiltersLabel()
        {
            if (_filters.Count == 0)
                return "Filters (global)";
            return $"Filters ({_filters.Count})";
        }

        private void UpdateFoldoutLabel()
        {
            _filterSection?.SetLabel(GetFiltersLabel());
        }

        private void RebuildFilterList()
        {
            _filterSection.Content.Clear();

            for (int i = 0; i < _filters.Count; i++)
            {
                int index = i; // Capture for closure
                var filterRow = CreateFilterRow(index);
                _filterSection.Content.Add(filterRow);
            }

            if (_filters.Count == 0)
            {
                var hint = EditorUIElements.CreateHintLabel("No filters (global search)");
                hint.style.marginBottom = 4f;
                _filterSection.Content.Add(hint);
            }

            UpdateFoldoutLabel();
        }

        private VisualElement CreateFilterRow(int index)
        {
            var filter = _filters[index];
            var row = EditorUIElements.CreateRow();
            row.style.marginBottom = 2f;
            row.style.backgroundColor = EditorUIElements.Colors.Base200;
            row.style.borderBottomLeftRadius = 3f;
            row.style.borderBottomRightRadius = 3f;
            row.style.borderTopLeftRadius = 3f;
            row.style.borderTopRightRadius = 3f;
            row.style.paddingLeft = 4f;
            row.style.paddingRight = 4f;
            row.style.paddingTop = 2f;
            row.style.paddingBottom = 2f;

            // Create UI based on filter type
            var filterContent = CreateFilterUI(filter);
            filterContent.style.flexGrow = 1;
            row.Add(filterContent);

            // Remove button
            var removeBtn = EditorUIElements.CreateIconButton("×", null, () => RemoveFilter(index));
            removeBtn.style.width = 20f;
            removeBtn.style.height = 18f;
            removeBtn.style.marginLeft = 4f;
            row.Add(removeBtn);

            return row;
        }

        private VisualElement CreateFilterUI(AssetFinderFilterConfig filter)
        {
            var container = EditorUIElements.CreateRow(Align.Center);

            switch (filter)
            {
                case NameFilterConfig nameFilter:
                    container.Add(CreateLabel("Name"));
                    var nameField = CreateTextField(nameFilter.NamePattern, v =>
                    {
                        nameFilter.NamePattern = v;
                        SaveFilters();
                    });
                    container.Add(nameField);
                    container.Add(CreateHint("(contains)"));
                    break;

                case PathFilterConfig pathFilter:
                    container.Add(CreateLabel("Path"));
                    var pathField = CreateTextField(pathFilter.PathPattern, v =>
                    {
                        pathFilter.PathPattern = v;
                        SaveFilters();
                    });
                    container.Add(pathField);
                    container.Add(CreateHint("(contains)"));
                    break;

                case ExcludeNameFilterConfig excludeFilter:
                    container.Add(CreateLabel("Exclude"));
                    var excludeField = CreateTextField(excludeFilter.ExcludePattern, v =>
                    {
                        excludeFilter.ExcludePattern = v;
                        SaveFilters();
                    });
                    container.Add(excludeField);
                    container.Add(CreateHint("(name excludes)"));
                    break;

                case LabelFilterConfig labelFilter:
                    container.Add(CreateLabel("Labels"));
                    var labelsContainer = CreateLabelsUI(labelFilter);
                    container.Add(labelsContainer);
                    break;

                case TypeFilterConfig typeFilter:
                    container.Add(CreateLabel("Type"));
                    var typePicker = new TypePickerField(_config.AssetType, null, false, true);
                    typePicker.style.flexGrow = 1;
                    typePicker.SetSelectedType(typeFilter.TypeFullName);
                    typePicker.OnTypeSelected += type =>
                    {
                        typeFilter.SetFilterType(type);
                        SaveFilters();
                    };
                    container.Add(typePicker);
                    break;
            }

            return container;
        }

        private Label CreateLabel(string text)
        {
            var label = new Label(text);
            label.style.width = 55;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            return label;
        }

        private TextField CreateTextField(string value, Action<string> onChange)
        {
            var field = new TextField();
            field.value = value ?? "";
            field.style.flexGrow = 1;
            field.RegisterValueChangedCallback(e => onChange(e.newValue));
            return field;
        }

        private Label CreateHint(string text)
        {
            var hint = new Label(text);
            hint.style.color = EditorUIElements.Colors.TextMuted;
            hint.style.fontSize = 10;
            hint.style.marginLeft = 4;
            return hint;
        }

        private VisualElement CreateLabelsUI(LabelFilterConfig labelFilter)
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;

            var badgesRow = EditorUIElements.CreateRow();
            badgesRow.style.flexWrap = Wrap.Wrap;

            // Input field
            var inputField = new TextField();
            inputField.style.width = 100;
            inputField.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Return)
                {
                    string label = inputField.value?.Trim();
                    if (!string.IsNullOrEmpty(label))
                    {
                        labelFilter.AddLabel(label);
                        SaveFilters();
                        RebuildFilterList();
                    }
                }
            });
            badgesRow.Add(inputField);

            // Existing labels as badges
            foreach (var label in labelFilter.Labels)
            {
                var badge = CreateLabelBadge(label, () =>
                {
                    labelFilter.RemoveLabel(label);
                    SaveFilters();
                    RebuildFilterList();
                });
                badgesRow.Add(badge);
            }

            // Hint if empty
            if (labelFilter.Labels.Length == 0)
            {
                var hint = CreateHint("(type + Enter)");
                badgesRow.Add(hint);
            }

            container.Add(badgesRow);
            return container;
        }

        private VisualElement CreateLabelBadge(string label, Action onRemove)
        {
            var badge = EditorUIElements.CreateRow(Align.Center);
            badge.style.backgroundColor = EditorUIElements.Colors.Primary;
            badge.style.paddingLeft = 6;
            badge.style.paddingRight = 2;
            badge.style.paddingTop = 1;
            badge.style.paddingBottom = 1;
            badge.style.marginLeft = 4;
            badge.style.borderBottomLeftRadius = 8;
            badge.style.borderBottomRightRadius = 8;
            badge.style.borderTopLeftRadius = 8;
            badge.style.borderTopRightRadius = 8;

            var labelText = new Label(label);
            labelText.style.color = new Color(1, 1, 1);
            labelText.style.fontSize = 10;
            badge.Add(labelText);

            var removeBtn = new Button(onRemove) { text = "×" };
            removeBtn.style.backgroundColor = Color.clear;
            removeBtn.style.borderBottomWidth = 0;
            removeBtn.style.borderTopWidth = 0;
            removeBtn.style.borderLeftWidth = 0;
            removeBtn.style.borderRightWidth = 0;
            removeBtn.style.color = new Color(1, 1, 1, 0.8f);
            removeBtn.style.fontSize = 10;
            removeBtn.style.paddingLeft = 2;
            removeBtn.style.paddingRight = 2;
            removeBtn.style.marginLeft = 2;
            badge.Add(removeBtn);

            return badge;
        }

        private void ShowAddFilterMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Name"), false, () => AddFilter<NameFilterConfig>());
            menu.AddItem(new GUIContent("Path"), false, () => AddFilter<PathFilterConfig>());
            menu.AddItem(new GUIContent("Exclude Name"), false, () => AddFilter<ExcludeNameFilterConfig>());
            menu.AddItem(new GUIContent("Labels"), false, () => AddFilter<LabelFilterConfig>());
            menu.AddItem(new GUIContent("Type"), false, () => AddFilter<TypeFilterConfig>());
            menu.ShowAsContext();
        }

        private void AddFilter<T>() where T : AssetFinderFilterConfig, new()
        {
            _filters.Add(new T());
            SaveFilters();
            RebuildFilterList();
        }

        private void RemoveFilter(int index)
        {
            if (index >= 0 && index < _filters.Count)
            {
                _filters.RemoveAt(index);
                SaveFilters();
                RebuildFilterList();
            }
        }

        private const string ClipboardPrefix = "AssetFinderFilters:";

        private void CopyFilters()
        {
            if (_filters.Count == 0)
            {
                Debug.Log("No filters to copy.");
                return;
            }

            var wrapper = new FiltersWrapper();
            foreach (var filter in _filters)
            {
                wrapper.Filters.Add(new FilterData
                {
                    Type = filter.GetType().Name,
                    Json = JsonUtility.ToJson(filter)
                });
            }

            EditorGUIUtility.systemCopyBuffer = ClipboardPrefix + JsonUtility.ToJson(wrapper);
            Debug.Log($"Copied {_filters.Count} filter(s) to clipboard.");
        }

        private void PasteFilters()
        {
            string clipboard = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clipboard) || !clipboard.StartsWith(ClipboardPrefix))
            {
                Debug.LogWarning("No AssetFinder filters found in clipboard.");
                return;
            }

            try
            {
                string json = clipboard.Substring(ClipboardPrefix.Length);
                var wrapper = JsonUtility.FromJson<FiltersWrapper>(json);
                if (wrapper?.Filters == null || wrapper.Filters.Count == 0)
                {
                    Debug.LogWarning("Clipboard contains empty filter data.");
                    return;
                }

                int added = 0;
                foreach (var data in wrapper.Filters)
                {
                    AssetFinderFilterConfig filter = data.Type switch
                    {
                        nameof(NameFilterConfig) => JsonUtility.FromJson<NameFilterConfig>(data.Json),
                        nameof(PathFilterConfig) => JsonUtility.FromJson<PathFilterConfig>(data.Json),
                        nameof(ExcludeNameFilterConfig) => JsonUtility.FromJson<ExcludeNameFilterConfig>(data.Json),
                        nameof(LabelFilterConfig) => JsonUtility.FromJson<LabelFilterConfig>(data.Json),
                        nameof(TypeFilterConfig) => JsonUtility.FromJson<TypeFilterConfig>(data.Json),
                        _ => null
                    };

                    if (filter != null)
                    {
                        _filters.Add(filter);
                        added++;
                    }
                }

                if (added > 0)
                {
                    SaveFilters();
                    RebuildFilterList();
                    Debug.Log($"Pasted {added} filter(s) from clipboard.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to paste filters: {e.Message}");
            }
        }

        private void OnFindClicked()
        {
            var filtered = RunProjectSearchWithFilters(_config.AssetType, _filters);
            if (filtered == null)
                return;
            OnAssetsFound?.Invoke(filtered);
        }

        /// <summary>EditorPrefs key used for filter JSON (matches toolbar instance).</summary>
        public static string GetFiltersPersistenceKey(AssetFinderToolbarConfig config)
        {
            return PrefsKeyPrefix + (config.PersistenceKey ?? config.AssetType?.FullName ?? "default");
        }

        /// <summary>Loads filter configs from EditorPrefs (same format as the UI toolbar).</summary>
        public static List<AssetFinderFilterConfig> LoadFiltersFromEditorPrefs(string fullPersistenceKey)
        {
            var list = new List<AssetFinderFilterConfig>();
            if (string.IsNullOrEmpty(fullPersistenceKey))
                return list;

            string json = EditorPrefs.GetString(fullPersistenceKey, "");
            if (string.IsNullOrEmpty(json))
                return list;

            try
            {
                var wrapper = JsonUtility.FromJson<FiltersWrapper>(json);
                if (wrapper?.Filters == null)
                    return list;

                foreach (var data in wrapper.Filters)
                {
                    AssetFinderFilterConfig filter = data.Type switch
                    {
                        nameof(NameFilterConfig) => JsonUtility.FromJson<NameFilterConfig>(data.Json),
                        nameof(PathFilterConfig) => JsonUtility.FromJson<PathFilterConfig>(data.Json),
                        nameof(ExcludeNameFilterConfig) => JsonUtility.FromJson<ExcludeNameFilterConfig>(data.Json),
                        nameof(LabelFilterConfig) => JsonUtility.FromJson<LabelFilterConfig>(data.Json),
                        nameof(TypeFilterConfig) => JsonUtility.FromJson<TypeFilterConfig>(data.Json),
                        _ => null
                    };

                    if (filter != null)
                        list.Add(filter);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load AssetFinderToolbar filters: {e.Message}");
            }

            return list;
        }

        /// <summary>
        /// Same search pipeline as the Find button. Returns null if the user cancels the empty-filter warning.
        /// </summary>
        public static List<Object> RunProjectSearchWithFilters(Type assetType,
            IReadOnlyList<AssetFinderFilterConfig> filters)
        {
            if (assetType == null)
            {
                Debug.LogWarning("AssetFinderToolbar: AssetType not configured.");
                return new List<Object>();
            }

            if (filters == null || filters.Count == 0)
            {
                if (!EditorUtility.DisplayDialog(
                        "No Filters",
                        "No filters are set. This will search the entire project and may find a large number of assets. Continue?",
                        "Find All", "Cancel"))
                {
                    return null;
                }
            }

            string nameFilter = null;
            string pathFilter = null;
            string excludeNameFilter = null;
            string[] labels = null;
            Type typeFilter = null;

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    switch (filter)
                    {
                        case NameFilterConfig nf:
                            nameFilter = nf.NamePattern;
                            break;
                        case PathFilterConfig pf:
                            pathFilter = pf.PathPattern;
                            break;
                        case ExcludeNameFilterConfig ef:
                            excludeNameFilter = ef.ExcludePattern;
                            break;
                        case LabelFilterConfig lf:
                            if (lf.Labels.Length > 0)
                                labels = lf.Labels;
                            break;
                        case TypeFilterConfig tf:
                            typeFilter = tf.GetFilterType();
                            break;
                    }
                }
            }

            var options = new AssetFinderOptions { Labels = labels };

            Debug.Log($"Searching entire project for {assetType.Name}...");
            var result = AssetFinder.FindAllAssets(assetType, options);

            if (!result.Success)
            {
                Debug.LogError($"AssetFinder error: {result.Error}");
                return new List<Object>();
            }

            var filtered = new List<Object>();
            foreach (var asset in result.Assets)
            {
                string path = AssetDatabase.GetAssetPath(asset);

                if (!string.IsNullOrEmpty(nameFilter))
                {
                    if (!asset.name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                if (!string.IsNullOrEmpty(pathFilter))
                {
                    if (!path.Contains(pathFilter, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                if (!string.IsNullOrEmpty(excludeNameFilter))
                {
                    if (asset.name.Contains(excludeNameFilter, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                if (typeFilter != null)
                {
                    if (!typeFilter.IsAssignableFrom(asset.GetType()))
                        continue;
                }

                filtered.Add(asset);
            }

            Debug.Log(
                $"Found {filtered.Count} assets of type {assetType.Name} (filtered from {result.FoundCount}).");
            return filtered;
        }

        #region Persistence

        [Serializable]
        private class FilterData
        {
            public string Type;
            public string Json;
        }

        [Serializable]
        private class FiltersWrapper
        {
            public List<FilterData> Filters = new();
        }

        private void LoadFilters()
        {
            _filters.Clear();
            string json = EditorPrefs.GetString(_persistenceKey, "");
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var wrapper = JsonUtility.FromJson<FiltersWrapper>(json);
                if (wrapper?.Filters == null) return;

                foreach (var data in wrapper.Filters)
                {
                    AssetFinderFilterConfig filter = data.Type switch
                    {
                        nameof(NameFilterConfig) => JsonUtility.FromJson<NameFilterConfig>(data.Json),
                        nameof(PathFilterConfig) => JsonUtility.FromJson<PathFilterConfig>(data.Json),
                        nameof(ExcludeNameFilterConfig) => JsonUtility.FromJson<ExcludeNameFilterConfig>(data.Json),
                        nameof(LabelFilterConfig) => JsonUtility.FromJson<LabelFilterConfig>(data.Json),
                        nameof(TypeFilterConfig) => JsonUtility.FromJson<TypeFilterConfig>(data.Json),
                        _ => null
                    };

                    if (filter != null)
                        _filters.Add(filter);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load AssetFinderToolbar filters: {e.Message}");
            }
        }

        private void SaveFilters()
        {
            var wrapper = new FiltersWrapper();
            foreach (var filter in _filters)
            {
                wrapper.Filters.Add(new FilterData
                {
                    Type = filter.GetType().Name,
                    Json = JsonUtility.ToJson(filter)
                });
            }

            string json = JsonUtility.ToJson(wrapper);
            EditorPrefs.SetString(_persistenceKey, json);
        }

        #endregion
    }
}
#endif
