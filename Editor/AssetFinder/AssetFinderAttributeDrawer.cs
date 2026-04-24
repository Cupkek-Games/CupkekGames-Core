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
    [CustomPropertyDrawer(typeof(AssetFinderAttribute))]
    public class AssetFinderAttributeDrawer : PropertyDrawer
    {
        /// <summary>Tracks foldout + filter state per property path across repaints.</summary>
        private static readonly Dictionary<string, ImguiFilterState> s_stateCache = new();

        private class ImguiFilterState
        {
            public bool Foldout;
            public List<AssetFinderFilterConfig> Filters;
            public string PersistenceKey;

            public ImguiFilterState(string persistenceKey)
            {
                PersistenceKey = persistenceKey;
                Filters = AssetFinderToolbar.LoadFiltersFromEditorPrefs(persistenceKey);
            }

            public void Save()
            {
                // Reuse the shared persistence format so UI Toolkit and IMGUI stay in sync
                var wrapper = new ImguiFiltersWrapper();
                foreach (var f in Filters)
                    wrapper.Filters.Add(new ImguiFilterData
                    {
                        Type = f.GetType().Name,
                        Json = JsonUtility.ToJson(f)
                    });
                EditorPrefs.SetString(PersistenceKey, JsonUtility.ToJson(wrapper));
            }

            [Serializable]
            private class ImguiFilterData { public string Type; public string Json; }
            [Serializable]
            private class ImguiFiltersWrapper { public List<ImguiFilterData> Filters = new(); }
        }

        private ImguiFilterState GetOrCreateState(AssetFinderToolbarConfig config)
        {
            string key = AssetFinderToolbar.GetFiltersPersistenceKey(config);
            if (!s_stateCache.TryGetValue(key, out var state))
            {
                state = new ImguiFilterState(key);
                s_stateCache[key] = state;
            }
            return state;
        }

        private const float WrapPadLeft = 18f;  // covers foldout arrows
        private const float WrapPadRight = 4f;
        private const float WrapPadTop = 6f;
        private const float WrapPadBottom = 6f;
        private const float WrapMarginTop = 4f;
        private const float SeparatorHeight = 1f;
        private const float SeparatorMargin = 4f; // vertical gap around separator

        private float ImguiToolbarHeight(ImguiFilterState state)
        {
            float sp = EditorGUIUtility.standardVerticalSpacing;
            float lineH = EditorGUIUtility.singleLineHeight;
            // Header row (foldout + buttons)
            float h = lineH + sp;
            // Filter rows when expanded
            if (state.Foldout)
            {
                int count = Mathf.Max(state.Filters.Count, 1); // at least 1 for hint
                h += count * (lineH + sp);
                // "Add filter" menu button row
                h += lineH + sp;
            }
            return h;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!TryGetCustomDrawerContext(property, out var fieldKind, out var config, out _))
                return EditorGUI.GetPropertyHeight(property, label, true);

            var state = GetOrCreateState(config);
            float toolbarH = ImguiToolbarHeight(state);
            float mainH = fieldKind == FieldKind.List
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : KeyValueDatabaseDrawer.GetKeyValueDatabasePropertyHeight(property, label);

            // main + separator + toolbar + wrapper padding + top margin
            return WrapMarginTop + WrapPadTop + mainH + SeparatorMargin + SeparatorHeight + SeparatorMargin + toolbarH + WrapPadBottom;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!TryGetCustomDrawerContext(property, out var fieldKind, out var toolbarConfig, out var keyType))
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var state = GetOrCreateState(toolbarConfig);

            float mainH = fieldKind == FieldKind.List
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : KeyValueDatabaseDrawer.GetKeyValueDatabasePropertyHeight(property, label);
            float toolbarH = ImguiToolbarHeight(state);

            // --- Wrapper background ---
            Color wrapBg = EditorGUIUtility.isProSkin
                ? EditorUIElements.Colors.Base300
                : new Color(0.78f, 0.78f, 0.78f, 1f);
            Color wrapBorder = EditorGUIUtility.isProSkin
                ? EditorUIElements.Colors.Border
                : new Color(0.55f, 0.55f, 0.55f, 1f);
            Color separatorColor = EditorGUIUtility.isProSkin
                ? new Color(0.3f, 0.3f, 0.3f, 1f)
                : new Color(0.6f, 0.6f, 0.6f, 1f);

            float innerH = WrapPadTop + mainH + SeparatorMargin + SeparatorHeight + SeparatorMargin + toolbarH + WrapPadBottom;
            Rect wrapRect = new Rect(
                position.x - WrapPadLeft,
                position.y + WrapMarginTop,
                position.width + WrapPadLeft + WrapPadRight,
                innerH);

            // Background + border
            EditorGUI.DrawRect(wrapRect, wrapBg);
            EditorGUI.DrawRect(new Rect(wrapRect.x, wrapRect.y, wrapRect.width, 1f), wrapBorder);
            EditorGUI.DrawRect(new Rect(wrapRect.x, wrapRect.yMax - 1f, wrapRect.width, 1f), wrapBorder);
            EditorGUI.DrawRect(new Rect(wrapRect.x, wrapRect.y, 1f, wrapRect.height), wrapBorder);
            EditorGUI.DrawRect(new Rect(wrapRect.xMax - 1f, wrapRect.y, 1f, wrapRect.height), wrapBorder);

            // --- Database / List ---
            float contentY = position.y + WrapMarginTop + WrapPadTop;
            Rect mainRect = new Rect(position.x, contentY, position.width, mainH);
            if (fieldKind == FieldKind.List)
                EditorGUI.PropertyField(mainRect, property, label, true);
            else
                KeyValueDatabaseDrawer.DrawKeyValueDatabaseProperty(mainRect, property, label);

            // --- Separator line between database and filters ---
            float sepY = contentY + mainH + SeparatorMargin;
            Rect sepRect = new Rect(wrapRect.x + 8f, sepY, wrapRect.width - 16f, SeparatorHeight);
            EditorGUI.DrawRect(sepRect, separatorColor);

            // --- Filter toolbar ---
            float toolbarY = sepY + SeparatorHeight + SeparatorMargin;
            Rect toolbarRect = new Rect(position.x, toolbarY, position.width, toolbarH);
            DrawImguiAssetFinderToolbar(toolbarRect, property, fieldKind, toolbarConfig, keyType, state);
        }

        /// <summary>
        /// When false, use default property UI (unknown field types, non-Object asset types, array elements).
        /// </summary>
        private bool TryGetCustomDrawerContext(SerializedProperty property, out FieldKind fieldKind,
            out AssetFinderToolbarConfig toolbarConfig, out Type keyTypeForDatabase)
        {
            toolbarConfig = null;
            keyTypeForDatabase = null;
            fieldKind = DetectFieldKind(fieldInfo.FieldType);

            if (property.propertyPath.Contains(".Array.data["))
                return false;

            if (fieldKind == FieldKind.Unknown)
                return false;

            var attr = (AssetFinderAttribute)attribute;
            var fieldType = fieldInfo.FieldType;
            var assetType = attr.AssetType;
            if (fieldKind == FieldKind.KeyValueDatabase)
            {
                var detectedType = GetValueTypeFromDatabase(fieldType);
                if (detectedType != null && typeof(Object).IsAssignableFrom(detectedType))
                    assetType = detectedType;
            }

            if (!typeof(Object).IsAssignableFrom(assetType))
                return false;

            var targetObject = property.serializedObject.targetObject;
            var persistenceKey = attr.PersistenceKey
                ?? $"{targetObject.GetType().Name}_{targetObject.GetInstanceID()}_{property.propertyPath}";

            toolbarConfig = new AssetFinderToolbarConfig
            {
                AssetType = assetType,
                KeyType = attr.KeyType,
                PersistenceKey = persistenceKey
            };

            keyTypeForDatabase = fieldKind == FieldKind.KeyValueDatabase
                ? (attr.KeyType ?? GetKeyTypeFromDatabase(fieldType))
                : null;

            return true;
        }

        private void DrawImguiAssetFinderToolbar(Rect rect, SerializedProperty property, FieldKind fieldKind,
            AssetFinderToolbarConfig toolbarConfig, Type keyType, ImguiFilterState state)
        {
            float lineH = EditorGUIUtility.singleLineHeight;
            float sp = EditorGUIUtility.standardVerticalSpacing;
            float y = rect.y;

            // --- Header row: foldout + Find + Clear ---
            string filterLabel = state.Filters.Count == 0
                ? "Filters (global)"
                : $"Filters ({state.Filters.Count})";

            float foldoutW = rect.width - 170f;
            state.Foldout = EditorGUI.Foldout(
                new Rect(rect.x, y, foldoutW, lineH),
                state.Foldout, filterLabel, true);

            float btnX = rect.x + foldoutW + 4f;
            if (GUI.Button(new Rect(btnX, y, 80f, lineH), "Find"))
            {
                var found = AssetFinderToolbar.RunProjectSearchWithFilters(toolbarConfig.AssetType, state.Filters);
                if (found != null)
                {
                    if (fieldKind == FieldKind.List)
                        AddToList(property, found);
                    else
                        AddToDatabase(property, keyType, found);
                }
            }

            if (GUI.Button(new Rect(btnX + 84f, y, 80f, lineH), "Clear"))
            {
                if (fieldKind == FieldKind.List)
                    ClearList(property);
                else
                    ClearDatabase(property);
            }

            y += lineH + sp;

            // --- Expanded filter rows ---
            if (!state.Foldout)
                return;

            if (state.Filters.Count == 0)
            {
                EditorGUI.LabelField(new Rect(rect.x + 16f, y, rect.width - 16f, lineH),
                    "No filters — will search entire project.", EditorStyles.miniLabel);
                y += lineH + sp;
            }
            else
            {
                int removeIndex = -1;
                for (int i = 0; i < state.Filters.Count; i++)
                {
                    float rowX = rect.x + 16f;
                    float rowW = rect.width - 16f - 22f;
                    var filter = state.Filters[i];

                    // Alternating row background
                    Color rowBg = EditorGUIUtility.isProSkin
                        ? ((i & 1) == 0 ? EditorUIElements.Colors.Base200 : EditorUIElements.Colors.Base300)
                        : ((i & 1) == 0 ? new Color(0.75f, 0.75f, 0.75f, 1f) : new Color(0.78f, 0.78f, 0.78f, 1f));
                    EditorGUI.DrawRect(new Rect(rowX - 4f, y - 1f, rowW + 26f + 4f, lineH + 2f), rowBg);

                    DrawImguiFilterRow(new Rect(rowX, y, rowW, lineH), filter, state);

                    // Remove button
                    if (GUI.Button(new Rect(rowX + rowW + 2f, y, 20f, lineH), "×"))
                        removeIndex = i;

                    y += lineH + sp;
                }

                if (removeIndex >= 0)
                {
                    state.Filters.RemoveAt(removeIndex);
                    state.Save();
                }
            }

            // --- Add filter dropdown ---
            if (EditorGUI.DropdownButton(
                    new Rect(rect.x + 16f, y, 110f, lineH),
                    new GUIContent("+ Add Filter"), FocusType.Passive))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Name"), false, () =>
                {
                    state.Filters.Add(new NameFilterConfig());
                    state.Save();
                });
                menu.AddItem(new GUIContent("Path"), false, () =>
                {
                    state.Filters.Add(new PathFilterConfig());
                    state.Save();
                });
                menu.AddItem(new GUIContent("Exclude Name"), false, () =>
                {
                    state.Filters.Add(new ExcludeNameFilterConfig());
                    state.Save();
                });
                menu.AddItem(new GUIContent("Labels"), false, () =>
                {
                    state.Filters.Add(new LabelFilterConfig());
                    state.Save();
                });
                menu.ShowAsContext();
            }
        }

        private void DrawImguiFilterRow(Rect rect, AssetFinderFilterConfig filter, ImguiFilterState state)
        {
            float labelW = 55f;
            float fieldX = rect.x + labelW + 4f;
            float fieldW = rect.width - labelW - 4f;

            switch (filter)
            {
                case NameFilterConfig nf:
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, labelW, rect.height), "Name",
                        EditorStyles.boldLabel);
                    string newName = EditorGUI.TextField(new Rect(fieldX, rect.y, fieldW, rect.height),
                        nf.NamePattern ?? "");
                    if (newName != (nf.NamePattern ?? ""))
                    {
                        nf.NamePattern = newName;
                        state.Save();
                    }
                    break;

                case PathFilterConfig pf:
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, labelW, rect.height), "Path",
                        EditorStyles.boldLabel);
                    string newPath = EditorGUI.TextField(new Rect(fieldX, rect.y, fieldW, rect.height),
                        pf.PathPattern ?? "");
                    if (newPath != (pf.PathPattern ?? ""))
                    {
                        pf.PathPattern = newPath;
                        state.Save();
                    }
                    break;

                case ExcludeNameFilterConfig ef:
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, labelW, rect.height), "Exclude",
                        EditorStyles.boldLabel);
                    string newExclude = EditorGUI.TextField(new Rect(fieldX, rect.y, fieldW, rect.height),
                        ef.ExcludePattern ?? "");
                    if (newExclude != (ef.ExcludePattern ?? ""))
                    {
                        ef.ExcludePattern = newExclude;
                        state.Save();
                    }
                    break;

                case LabelFilterConfig lf:
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, labelW, rect.height), "Labels",
                        EditorStyles.boldLabel);
                    string joined = string.Join(", ", lf.Labels);
                    string newJoined = EditorGUI.TextField(new Rect(fieldX, rect.y, fieldW, rect.height), joined);
                    if (newJoined != joined)
                    {
                        // Rebuild labels from comma-separated input
                        var newLabels = newJoined.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        // Clear and re-add
                        while (lf.Labels.Length > 0)
                            lf.RemoveLabel(lf.Labels[0]);
                        foreach (var l in newLabels)
                        {
                            string trimmed = l.Trim();
                            if (!string.IsNullOrEmpty(trimmed))
                                lf.AddLabel(trimmed);
                        }
                        state.Save();
                    }
                    break;
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!TryGetCustomDrawerContext(property, out var fieldKind, out var toolbarConfig, out var keyType))
                return null;

            // Wrapper container that visually groups the database list and filter toolbar
            var root = new VisualElement();
            root.style.backgroundColor = EditorUIElements.Colors.Base300;
            root.style.borderTopWidth = 1f;
            root.style.borderBottomWidth = 1f;
            root.style.borderLeftWidth = 1f;
            root.style.borderRightWidth = 1f;
            root.style.borderTopColor = EditorUIElements.Colors.Border;
            root.style.borderBottomColor = EditorUIElements.Colors.Border;
            root.style.borderLeftColor = EditorUIElements.Colors.Border;
            root.style.borderRightColor = EditorUIElements.Colors.Border;
            root.style.borderTopLeftRadius = 4f;
            root.style.borderTopRightRadius = 4f;
            root.style.borderBottomLeftRadius = 4f;
            root.style.borderBottomRightRadius = 4f;
            root.style.paddingTop = 4f;
            root.style.paddingBottom = 4f;
            root.style.paddingLeft = 4f;
            root.style.paddingRight = 4f;

            FoldoutListView foldoutListView = null;

            if (fieldKind == FieldKind.List)
            {
                foldoutListView = new FoldoutListView(property, property.displayName);
                root.Add(foldoutListView);
            }
            else
            {
                var listProp = property.FindPropertyRelative("_list");
                if (listProp != null)
                {
                    foldoutListView = new FoldoutListView(listProp, property.displayName);
                    foldoutListView.Section.Expanded = property.isExpanded;
                    foldoutListView.Section.OnExpandedChanged += expanded => property.isExpanded = expanded;
                    root.Add(foldoutListView);
                }
            }

            var toolbar = new AssetFinderToolbar(toolbarConfig);
            // Remove the toolbar's own top separator — the wrapper provides grouping
            toolbar.style.borderTopWidth = 0f;
            toolbar.style.marginTop = 4f;

            if (fieldKind == FieldKind.List)
            {
                toolbar.OnAssetsFound += assets =>
                {
                    AddToList(property, assets);
                    foldoutListView?.RefreshSize(property.arraySize);
                };
                toolbar.OnClear += () =>
                {
                    ClearList(property);
                    foldoutListView?.RefreshSize(0);
                };
            }
            else
            {
                var listPropForSync = property.FindPropertyRelative("_list");
                toolbar.OnAssetsFound += assets =>
                {
                    AddToDatabase(property, keyType, assets);
                    foldoutListView?.RefreshSize(listPropForSync?.arraySize ?? 0);
                };
                toolbar.OnClear += () =>
                {
                    ClearDatabase(property);
                    foldoutListView?.RefreshSize(0);
                };
            }

            root.Add(toolbar);
            return root;
        }

        private enum FieldKind { Unknown, List, KeyValueDatabase }

        private FieldKind DetectFieldKind(Type fieldType)
        {
            if (fieldType.IsGenericType)
            {
                var genericDef = fieldType.GetGenericTypeDefinition();
                if (genericDef == typeof(List<>))
                    return FieldKind.List;
                if (genericDef == typeof(KeyValueDatabase<,>))
                    return FieldKind.KeyValueDatabase;
            }
            return FieldKind.Unknown;
        }

        private Type GetKeyTypeFromDatabase(Type databaseType)
        {
            var genericArgs = databaseType.GetGenericArguments();
            return genericArgs.Length > 0 ? genericArgs[0] : typeof(string);
        }

        private Type GetValueTypeFromDatabase(Type databaseType)
        {
            var genericArgs = databaseType.GetGenericArguments();
            return genericArgs.Length > 1 ? genericArgs[1] : null;
        }

        #region List Operations

        private void AddToList(SerializedProperty property, List<Object> assets)
        {
            int addedCount = 0;
            foreach (var asset in assets)
            {
                // Check if already exists
                bool exists = false;
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).objectReferenceValue == asset)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    property.InsertArrayElementAtIndex(property.arraySize);
                    property.GetArrayElementAtIndex(property.arraySize - 1).objectReferenceValue = asset;
                    addedCount++;
                }
            }

            property.serializedObject.ApplyModifiedProperties();
            Debug.Log($"Added {addedCount} assets to list.");
        }

        private void ClearList(SerializedProperty property)
        {
            property.ClearArray();
            property.serializedObject.ApplyModifiedProperties();
            Debug.Log("Cleared list.");
        }

        #endregion

        #region KeyValueDatabase Operations

        private void AddToDatabase(SerializedProperty property, Type keyType, List<Object> assets)
        {
            var listProperty = property.FindPropertyRelative("_list");
            if (listProperty == null)
            {
                Debug.LogError("AssetFinder: Could not find _list property in KeyValueDatabase.");
                return;
            }

            int addedCount = 0;
            foreach (var asset in assets)
            {
                // Check if already exists
                bool exists = false;
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    var element = listProperty.GetArrayElementAtIndex(i);
                    var valueProp = element.FindPropertyRelative("Value");
                    if (valueProp != null && valueProp.objectReferenceValue == asset)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    // Add new entry
                    listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
                    var newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
                    var keyProp = newElement.FindPropertyRelative("Key");
                    var valueProp = newElement.FindPropertyRelative("Value");

                    // Set key based on type
                    if (keyType == typeof(string))
                    {
                        keyProp.stringValue = asset.name;
                    }
                    else if (keyType == typeof(int))
                    {
                        keyProp.intValue = listProperty.arraySize - 1;
                    }

                    valueProp.objectReferenceValue = asset;
                    addedCount++;
                }
            }

            property.serializedObject.ApplyModifiedProperties();
            Debug.Log($"Added {addedCount} assets to database.");
        }

        private void ClearDatabase(SerializedProperty property)
        {
            var listProperty = property.FindPropertyRelative("_list");
            if (listProperty != null)
            {
                listProperty.ClearArray();
                property.serializedObject.ApplyModifiedProperties();
                Debug.Log("Cleared database.");
            }
        }

        #endregion
    }
}
#endif
