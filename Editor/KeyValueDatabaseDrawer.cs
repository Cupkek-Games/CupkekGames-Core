#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
  // ===========================================================================
  // KeyValuePairDrawer
  // ---------------------------------------------------------------------------
  // Drawn when a KeyValuePair<,> is rendered standalone (outside a database).
  // KeyValueDatabaseDrawer below bypasses this and renders Key / Value directly
  // to gain finer control over the row layout.
  // ===========================================================================
  [CustomPropertyDrawer(typeof(KeyValuePair<,>), true)]
  public class KeyValuePairDrawer : PropertyDrawer
  {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      SerializedProperty keyProp = property.FindPropertyRelative("Key");
      SerializedProperty valueProp = property.FindPropertyRelative("Value");
      if (keyProp == null || valueProp == null)
        return EditorGUIUtility.singleLineHeight;

      float hk = EditorGUI.GetPropertyHeight(keyProp, GUIContent.none, true);
      float hv = EditorGUI.GetPropertyHeight(valueProp, GUIContent.none, true);
      return Mathf.Max(hk, hv);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);
      SerializedProperty keyProp = property.FindPropertyRelative("Key");
      SerializedProperty valueProp = property.FindPropertyRelative("Value");
      if (keyProp == null || valueProp == null)
      {
        EditorGUI.LabelField(position, label.text, "Malformed KeyValuePair");
        EditorGUI.EndProperty();
        return;
      }

      float rowH = Mathf.Max(
        EditorGUI.GetPropertyHeight(keyProp, GUIContent.none, true),
        EditorGUI.GetPropertyHeight(valueProp, GUIContent.none, true));

      Rect row = new Rect(position.x, position.y, position.width, rowH);
      Rect content = EditorGUI.PrefixLabel(row, label);
      float gap = 4f;
      float keyW = Mathf.Max(60f, (content.width - gap) * 0.35f);
      Rect keyRect = new Rect(content.x, content.y, keyW, rowH);
      Rect valueRect = new Rect(content.x + keyW + gap, content.y, content.width - keyW - gap, rowH);

      EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none, true);
      EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none, true);
      EditorGUI.EndProperty();
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
      var row = new VisualElement();
      row.style.flexDirection = FlexDirection.Row;
      row.style.alignItems = Align.Center;

      var keyProp = property.FindPropertyRelative("Key");
      var valueProp = property.FindPropertyRelative("Value");

      var keyField = new PropertyField(keyProp, "");
      keyField.style.flexGrow = 1f;
      keyField.style.flexBasis = 0;
      keyField.style.minWidth = 60;

      var valueField = new PropertyField(valueProp, "");
      valueField.style.flexGrow = 2f;
      valueField.style.flexBasis = 0;
      valueField.style.minWidth = 80;
      valueField.style.marginLeft = 4f;

      row.Add(keyField);
      row.Add(valueField);

      return row;
    }
  }

  // ===========================================================================
  // KeyValueDatabaseDrawer
  // ---------------------------------------------------------------------------
  // Modern property drawer for KeyValueDatabase<,>:
  //  - Search filter (any key type, converted to display string)
  //  - Sort A→Z toggle (view-only, doesn't mutate the underlying list)
  //  - Per-row duplicate highlight + summary alert
  //  - IMGUI: pagination for >25 visible rows
  //  - UI Toolkit: ListView virtualization (custom itemsSource of indices)
  //  - Empty / no-results states
  //  - Element count badge in header
  //  - Confirm-on-clear
  // ===========================================================================
  [CustomPropertyDrawer(typeof(KeyValueDatabase<,>), true)]
  public class KeyValueDatabaseDrawer : PropertyDrawer
  {
    // ----- Per-property view state ------------------------------------------
    private class ViewState
    {
      public string Search = "";
      public bool SortActive;
      public int Page;
      public readonly List<int> Visible = new List<int>();
      public readonly HashSet<int> Duplicates = new HashSet<int>();
      public int Version = -1; // recompute when != listProperty.arraySize
    }

    private const int ImguiPageSize = 25;
    private const float ContentPanelPadding = 6f;
    private const float ListViewMaxHeight = 420f;

    private static readonly Dictionary<string, ViewState> _states = new Dictionary<string, ViewState>();
    private static SearchField _imguiSearchField;
    private static SearchField ImguiSearchField => _imguiSearchField ?? (_imguiSearchField = new SearchField());

    private static ViewState GetState(SerializedProperty p)
    {
      int instanceId = p.serializedObject.targetObject != null
        ? p.serializedObject.targetObject.GetInstanceID()
        : 0;
      string key = instanceId + ":" + p.propertyPath;
      if (!_states.TryGetValue(key, out var s))
      {
        s = new ViewState();
        _states[key] = s;
      }
      return s;
    }

    private static void Invalidate(ViewState s) => s.Version = -1;

    private static void EnsureView(ViewState s, SerializedProperty listProperty)
    {
      if (s.Version != listProperty.arraySize)
        RecomputeView(s, listProperty);
    }

    private static void RecomputeView(ViewState s, SerializedProperty listProperty)
    {
      s.Visible.Clear();
      s.Duplicates.Clear();
      int n = listProperty.arraySize;
      string search = (s.Search ?? "").Trim();
      bool hasSearch = search.Length > 0;

      // Filter
      for (int i = 0; i < n; i++)
      {
        if (!hasSearch)
        {
          s.Visible.Add(i);
          continue;
        }
        var elem = listProperty.GetArrayElementAtIndex(i);
        var keyProp = elem.FindPropertyRelative("Key");
        if (keyProp == null) continue;
        string keyStr = SerializedPropertyToDisplayString(keyProp);
        if (keyStr.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
          s.Visible.Add(i);
      }

      // Sort (view-only, alphabetic by key display string)
      if (s.SortActive)
      {
        s.Visible.Sort((a, b) =>
        {
          string ka = SerializedPropertyToDisplayString(listProperty.GetArrayElementAtIndex(a).FindPropertyRelative("Key"));
          string kb = SerializedPropertyToDisplayString(listProperty.GetArrayElementAtIndex(b).FindPropertyRelative("Key"));
          return string.Compare(ka, kb, StringComparison.OrdinalIgnoreCase);
        });
      }

      // Duplicates across the entire list (not just visible)
      var seen = new Dictionary<object, int>();
      for (int i = 0; i < n; i++)
      {
        var keyProp = listProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Key");
        if (keyProp == null) continue;
        object kv = GetValueFromProperty(keyProp);
        if (kv == null) continue;
        if (seen.TryGetValue(kv, out int prev))
        {
          s.Duplicates.Add(i);
          s.Duplicates.Add(prev);
        }
        else seen[kv] = i;
      }

      s.Version = n;
    }

    private static float ComputeRowHeight(SerializedProperty elem)
    {
      float lineH = EditorGUIUtility.singleLineHeight;
      var keyP = elem.FindPropertyRelative("Key");
      var valP = elem.FindPropertyRelative("Value");
      float kh = keyP != null ? EditorGUI.GetPropertyHeight(keyP, GUIContent.none, true) : lineH;
      float vh = valP != null ? EditorGUI.GetPropertyHeight(valP, GUIContent.none, true) : lineH;
      return Mathf.Max(kh, vh);
    }

    // ===========================================================================
    // IMGUI styling helpers
    // ===========================================================================

    private static Color GetImguiPanelBackground()
    {
      return EditorGUIUtility.isProSkin
        ? EditorUIElements.Colors.Base100
        : new Color(0.83f, 0.83f, 0.83f, 1f);
    }

    private static Color GetImguiPanelBorder()
    {
      return EditorGUIUtility.isProSkin
        ? EditorUIElements.Colors.Border
        : new Color(0.55f, 0.55f, 0.55f, 1f);
    }

    private static Color GetImguiHeaderColor()
    {
      return EditorGUIUtility.isProSkin
        ? new Color(0.18f, 0.18f, 0.18f, 1f)
        : new Color(0.78f, 0.78f, 0.78f, 1f);
    }

    private static Color GetImguiRowStripeColor(int rowIndex)
    {
      if (EditorGUIUtility.isProSkin)
        return (rowIndex & 1) == 0 ? new Color(0.21f, 0.21f, 0.21f, 1f) : new Color(0.235f, 0.235f, 0.235f, 1f);
      return (rowIndex & 1) == 0 ? new Color(0.79f, 0.79f, 0.79f, 1f) : new Color(0.83f, 0.83f, 0.83f, 1f);
    }

    private static void DrawImguiBorderedPanel(Rect r, Color bg, Color border)
    {
      EditorGUI.DrawRect(r, bg);
      const float t = 1f;
      EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, t), border);
      EditorGUI.DrawRect(new Rect(r.x, r.yMax - t, r.width, t), border);
      EditorGUI.DrawRect(new Rect(r.x, r.y, t, r.height), border);
      EditorGUI.DrawRect(new Rect(r.xMax - t, r.y, t, r.height), border);
    }

    // ===========================================================================
    // IMGUI public API
    // ===========================================================================

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      return GetKeyValueDatabasePropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      DrawKeyValueDatabaseProperty(position, property, label);
    }

    /// <summary>IMGUI height for a serialized <see cref="KeyValueDatabase{TKey,TValue}"/> field.</summary>
    public static float GetKeyValueDatabasePropertyHeight(SerializedProperty property, GUIContent label)
    {
      var listProperty = property.FindPropertyRelative("_list");
      if (listProperty == null)
        return EditorGUIUtility.singleLineHeight;

      float lineH = EditorGUIUtility.singleLineHeight;
      float sp = EditorGUIUtility.standardVerticalSpacing;

      float h = lineH; // header row
      if (!property.isExpanded)
        return h;

      var s = GetState(property);
      EnsureView(s, listProperty);

      h += sp;
      h += ContentPanelPadding;       // top inner padding
      h += lineH + sp;                // toolbar row

      int n = s.Visible.Count;
      bool needsPagination = n > ImguiPageSize;
      int pageStart = 0, pageEnd = n;
      if (needsPagination)
      {
        int totalPages = Mathf.Max(1, (n + ImguiPageSize - 1) / ImguiPageSize);
        s.Page = Mathf.Clamp(s.Page, 0, totalPages - 1);
        pageStart = s.Page * ImguiPageSize;
        pageEnd = Mathf.Min(pageStart + ImguiPageSize, n);
        h += lineH + sp;              // pagination row
      }

      if (n == 0)
      {
        h += lineH + sp;              // empty / no-results state
      }
      else
      {
        for (int i = pageStart; i < pageEnd; i++)
        {
          var elem = listProperty.GetArrayElementAtIndex(s.Visible[i]);
          h += ComputeRowHeight(elem) + sp;
        }
      }

      if (s.Duplicates.Count > 0)
        h += lineH * 2f + sp;         // duplicate alert

      h += ContentPanelPadding;       // bottom inner padding
      return h;
    }

    /// <summary>IMGUI drawing for a serialized <see cref="KeyValueDatabase{TKey,TValue}"/> field.</summary>
    public static void DrawKeyValueDatabaseProperty(Rect position, SerializedProperty property, GUIContent label)
    {
      var listProperty = property.FindPropertyRelative("_list");
      EditorGUI.BeginProperty(position, label, property);
      if (listProperty == null)
      {
        EditorGUI.LabelField(position, label != null ? label.text : property.displayName, "Missing _list");
        EditorGUI.EndProperty();
        return;
      }

      float lineH = EditorGUIUtility.singleLineHeight;
      float sp = EditorGUIUtility.standardVerticalSpacing;
      float y = position.y;

      var s = GetState(property);
      EnsureView(s, listProperty);

      // -------- Header foldout --------
      string headerText = (label != null && !string.IsNullOrEmpty(label.text)) ? label.text : property.displayName;
      string headerWithCount = $"{headerText}  ({listProperty.arraySize})";
      Rect headerRect = new Rect(position.x, y, position.width, lineH);
      property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, headerWithCount, true);
      y += lineH + sp;

      if (!property.isExpanded)
      {
        EditorGUI.EndProperty();
        return;
      }

      // -------- Bordered panel for content --------
      float panelTop = y;
      float panelBottom = position.y + GetKeyValueDatabasePropertyHeight(property, label);
      Rect panelRect = new Rect(position.x, panelTop, position.width, panelBottom - panelTop);
      DrawImguiBorderedPanel(panelRect, GetImguiPanelBackground(), GetImguiPanelBorder());

      y += ContentPanelPadding;

      int prevIndent = EditorGUI.indentLevel;
      EditorGUI.indentLevel = 0;

      try
      {
        // -------- Toolbar: search | sort | + | × --------
        const float toolbarPad = 4f;
        const float btnW = 22f;
        const float sortW = 32f;
        const float btnGap = 2f;
        float toolsRight = panelRect.xMax - toolbarPad;
        float toolsLeft = panelRect.x + toolbarPad;
        float btnsTotal = sortW + btnGap + btnW + btnGap + btnW;
        float searchW = Mathf.Max(60f, (toolsRight - toolsLeft) - btnGap - btnsTotal);

        Rect searchRect = new Rect(toolsLeft, y, searchW, lineH);
        EditorGUI.BeginChangeCheck();
        string newSearch = ImguiSearchField.OnToolbarGUI(searchRect, s.Search ?? "");
        if (EditorGUI.EndChangeCheck())
        {
          s.Search = newSearch ?? "";
          s.Page = 0;
          Invalidate(s);
          EnsureView(s, listProperty);
        }

        float bx = toolsLeft + searchW + btnGap;

        // Sort toggle (view-only)
        Color prevBg = GUI.backgroundColor;
        if (s.SortActive) GUI.backgroundColor = new Color(0.55f, 0.75f, 1f, 1f);
        var sortContent = new GUIContent(s.SortActive ? "A↓" : "⇅", "Sort A→Z (view only — does not reorder underlying list)");
        if (GUI.Button(new Rect(bx, y, sortW, lineH), sortContent, EditorStyles.miniButton))
        {
          s.SortActive = !s.SortActive;
          Invalidate(s);
          EnsureView(s, listProperty);
        }
        GUI.backgroundColor = prevBg;
        bx += sortW + btnGap;

        // Add
        if (GUI.Button(new Rect(bx, y, btnW, lineH), new GUIContent("+", "Add new entry"), EditorStyles.miniButton))
        {
          listProperty.arraySize++;
          listProperty.serializedObject.ApplyModifiedProperties();
          // Clear search so the new (likely empty-key) entry is visible
          s.Search = "";
          Invalidate(s);
          EnsureView(s, listProperty);
          int totalPages = Mathf.Max(1, (s.Visible.Count + ImguiPageSize - 1) / ImguiPageSize);
          s.Page = totalPages - 1;
          GUI.FocusControl(null);
        }
        bx += btnW + btnGap;

        // Clear
        if (GUI.Button(new Rect(bx, y, btnW, lineH), new GUIContent("×", "Clear all entries"), EditorStyles.miniButton))
        {
          if (listProperty.arraySize > 0 && EditorUtility.DisplayDialog(
                "Clear all entries?",
                $"Remove all {listProperty.arraySize} entries from \"{property.displayName}\"?",
                "Clear", "Cancel"))
          {
            listProperty.ClearArray();
            listProperty.serializedObject.ApplyModifiedProperties();
            s.Page = 0;
            Invalidate(s);
            EnsureView(s, listProperty);
          }
        }

        y += lineH + sp;

        // -------- Pagination row --------
        int n = s.Visible.Count;
        bool needsPagination = n > ImguiPageSize;
        int pageStart = 0, pageEnd = n;
        if (needsPagination)
        {
          int totalPages = Mathf.Max(1, (n + ImguiPageSize - 1) / ImguiPageSize);
          s.Page = Mathf.Clamp(s.Page, 0, totalPages - 1);
          pageStart = s.Page * ImguiPageSize;
          pageEnd = Mathf.Min(pageStart + ImguiPageSize, n);

          float pagY = y;
          float pagPad = 4f;
          Rect labelLeftRect = new Rect(panelRect.x + pagPad, pagY, panelRect.width - 2 * pagPad, lineH);
          GUI.Label(labelLeftRect, $"Showing {pageStart + 1}–{pageEnd} of {n}", EditorStyles.miniLabel);

          float navW = 22f;
          float pageLabelW = 90f;
          float pagRight = panelRect.xMax - pagPad;
          Rect nextRect = new Rect(pagRight - navW, pagY, navW, lineH);
          Rect pageLabelRect = new Rect(nextRect.x - pageLabelW - 2f, pagY, pageLabelW, lineH);
          Rect prevRect = new Rect(pageLabelRect.x - navW - 2f, pagY, navW, lineH);

          using (new EditorGUI.DisabledScope(s.Page <= 0))
            if (GUI.Button(prevRect, new GUIContent("◀", "Previous page"), EditorStyles.miniButton)) s.Page--;

          GUI.Label(pageLabelRect, $"Page {s.Page + 1} / {totalPages}",
            new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter });

          using (new EditorGUI.DisabledScope(s.Page >= totalPages - 1))
            if (GUI.Button(nextRect, new GUIContent("▶", "Next page"), EditorStyles.miniButton)) s.Page++;

          y += lineH + sp;
        }

        // -------- Row list / empty state --------
        if (n == 0)
        {
          bool listEmpty = listProperty.arraySize == 0;
          string emptyMsg = listEmpty
            ? "No entries — click  +  to add one."
            : "No entries match the current search.";
          var emptyStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter };
          var muted = emptyStyle.normal.textColor;
          muted.a = 0.7f;
          emptyStyle.normal.textColor = muted;
          EditorGUI.LabelField(new Rect(panelRect.x, y, panelRect.width, lineH), emptyMsg, emptyStyle);
          y += lineH + sp;
        }
        else
        {
          bool reorderEnabled = (s.Search ?? "").Length == 0 && !s.SortActive;
          const float rowActionsW = 66f;
          const float rowSidePad = 4f;
          const float idxW = 26f;

          for (int i = pageStart; i < pageEnd; i++)
          {
            int actualIdx = s.Visible[i];
            var elem = listProperty.GetArrayElementAtIndex(actualIdx);
            var keyProp = elem.FindPropertyRelative("Key");
            var valueProp = elem.FindPropertyRelative("Value");
            float rowH = ComputeRowHeight(elem);

            // Row stripe
            Rect stripeRect = new Rect(panelRect.x + 1f, y - sp * 0.5f, panelRect.width - 2f, rowH + sp);
            EditorGUI.DrawRect(stripeRect, GetImguiRowStripeColor(i));

            bool isDuplicate = s.Duplicates.Contains(actualIdx);
            if (isDuplicate)
              EditorGUI.DrawRect(stripeRect, new Color(0.85f, 0.25f, 0.25f, 0.18f));

            // Index label
            var idxStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
            var idxColor = idxStyle.normal.textColor;
            idxColor.a = 0.55f;
            idxStyle.normal.textColor = idxColor;
            Rect idxRect = new Rect(panelRect.x + rowSidePad, y, idxW, lineH);
            GUI.Label(idxRect, actualIdx.ToString(), idxStyle);

            // Key / Value fields
            float fieldsX = idxRect.xMax + rowSidePad;
            float fieldsRight = panelRect.xMax - rowSidePad - rowActionsW - rowSidePad;
            float fieldsW = Mathf.Max(40f, fieldsRight - fieldsX);
            const float gap = 4f;
            float keyW = Mathf.Max(80f, (fieldsW - gap) * 0.4f);
            float valW = fieldsW - keyW - gap;

            float kh = keyProp != null ? EditorGUI.GetPropertyHeight(keyProp, GUIContent.none, true) : lineH;
            float vh = valueProp != null ? EditorGUI.GetPropertyHeight(valueProp, GUIContent.none, true) : lineH;
            Rect keyRect = new Rect(fieldsX, y, keyW, kh);
            Rect valRect = new Rect(fieldsX + keyW + gap, y, valW, vh);

            if (keyProp != null)
              EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none, true);
            if (valueProp != null)
              EditorGUI.PropertyField(valRect, valueProp, GUIContent.none, true);

            // Action buttons (top-aligned to the row)
            float ax = fieldsRight + rowSidePad;
            using (new EditorGUI.DisabledScope(!reorderEnabled || actualIdx <= 0))
            {
              if (GUI.Button(new Rect(ax, y, 20f, lineH), new GUIContent("↑", "Move up"), EditorStyles.miniButton))
              {
                listProperty.MoveArrayElement(actualIdx, actualIdx - 1);
                listProperty.serializedObject.ApplyModifiedProperties();
                Invalidate(s);
                EditorGUI.indentLevel = prevIndent;
                EditorGUI.EndProperty();
                return;
              }
            }
            using (new EditorGUI.DisabledScope(!reorderEnabled || actualIdx >= listProperty.arraySize - 1))
            {
              if (GUI.Button(new Rect(ax + 22f, y, 20f, lineH), new GUIContent("↓", "Move down"), EditorStyles.miniButton))
              {
                listProperty.MoveArrayElement(actualIdx, actualIdx + 1);
                listProperty.serializedObject.ApplyModifiedProperties();
                Invalidate(s);
                EditorGUI.indentLevel = prevIndent;
                EditorGUI.EndProperty();
                return;
              }
            }
            if (GUI.Button(new Rect(ax + 44f, y, 20f, lineH), new GUIContent("×", "Remove"), EditorStyles.miniButton))
            {
              listProperty.DeleteArrayElementAtIndex(actualIdx);
              listProperty.serializedObject.ApplyModifiedProperties();
              Invalidate(s);
              EditorGUI.indentLevel = prevIndent;
              EditorGUI.EndProperty();
              return;
            }

            y += rowH + sp;
          }
        }

        // -------- Duplicate alert --------
        if (s.Duplicates.Count > 0)
        {
          float warnH = lineH * 2f;
          Rect warnRect = new Rect(
            panelRect.x + ContentPanelPadding,
            y,
            Mathf.Max(0f, panelRect.width - 2f * ContentPanelPadding),
            warnH);
          EditorGUI.HelpBox(warnRect, $"Duplicate keys detected — {s.Duplicates.Count} affected entries highlighted.", MessageType.Error);
        }
      }
      finally
      {
        EditorGUI.indentLevel = prevIndent;
      }

      EditorGUI.EndProperty();
    }

    // ===========================================================================
    // UI Toolkit
    // ===========================================================================

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
      return CreateKeyValueDatabaseVisualElement(property);
    }

    /// <summary>UI Toolkit modern panel for any type that serializes a <c>_list</c> like <see cref="KeyValueDatabase{TKey,TValue}"/>.</summary>
    public static VisualElement CreateKeyValueDatabaseVisualElement(SerializedProperty property)
    {
      var listProperty = property.FindPropertyRelative("_list");
      if (listProperty == null)
        return new Label("Missing _list (KeyValueDatabase)");

      var state = GetState(property);
      EnsureView(state, listProperty);

      // -------- Outer card --------
      var card = EditorUIElements.CreateBox(EditorUIElements.Colors.Base200);
      card.style.paddingTop = 6f;
      card.style.paddingBottom = 6f;
      card.style.paddingLeft = 6f;
      card.style.paddingRight = 6f;
      card.style.marginTop = 4f;

      // -------- Foldout section header --------
      var section = new FoldoutSection(property.displayName, property);

      var countBadge = EditorUIElements.CreateBadge(listProperty.arraySize.ToString(), EditorUIElements.BadgeStyle.Neutral);
      countBadge.style.marginRight = 0f;
      section.HeaderRight.Add(countBadge);

      card.Add(section);

      // -------- Toolbar: [search] [sort] [+] [×] --------
      var toolbar = EditorUIElements.CreateRow(Align.Center);
      toolbar.style.marginTop = 6f;
      toolbar.style.marginBottom = 4f;

      var searchField = new ToolbarSearchField();
      searchField.style.flexGrow = 1f;
      searchField.style.flexShrink = 1f;
      searchField.style.minWidth = 80f;
      searchField.SetValueWithoutNotify(state.Search ?? "");
      toolbar.Add(searchField);

      var sortBtn = EditorUIElements.CreateIconButton("⇅", null, null);
      sortBtn.tooltip = "Sort A→Z (view only — does not reorder underlying list)";
      sortBtn.style.width = 32f;
      sortBtn.style.marginLeft = 4f;
      toolbar.Add(sortBtn);

      var addBtn = EditorUIElements.CreateIconButton("+", null, null);
      addBtn.tooltip = "Add new entry";
      addBtn.style.marginLeft = 4f;
      toolbar.Add(addBtn);

      var clearBtn = EditorUIElements.CreateIconButton("×", null, null);
      clearBtn.tooltip = "Clear all entries";
      clearBtn.style.marginLeft = 4f;
      toolbar.Add(clearBtn);

      section.Content.Add(toolbar);

      // -------- Empty / no-results state --------
      var emptyState = EditorUIElements.CreateHintLabel("No entries — click  +  to add one.");
      emptyState.style.unityTextAlign = TextAnchor.MiddleCenter;
      emptyState.style.paddingTop = 14f;
      emptyState.style.paddingBottom = 14f;
      emptyState.style.display = DisplayStyle.None;
      section.Content.Add(emptyState);

      // -------- ListView with custom itemsSource (List<int> of visible indices) --------
      var listView = new ListView();
      listView.itemsSource = state.Visible;
      listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
      listView.selectionType = SelectionType.None;
      listView.showAddRemoveFooter = false;
      listView.showBoundCollectionSize = false;
      listView.showFoldoutHeader = false;
      listView.reorderable = false;
      listView.style.maxHeight = ListViewMaxHeight;
      listView.style.minHeight = 40f;
      listView.style.flexShrink = 0f;
      listView.style.marginTop = 2f;
      section.Content.Add(listView);

      // -------- Duplicate-key alert --------
      var warning = EditorUIElements.CreateAlert("Duplicate keys detected — highlighted entries will be lost on save.", EditorUIElements.StatusType.Error);
      warning.style.marginTop = 6f;
      warning.style.borderTopColor = EditorColorPalette.Danger;
      warning.style.borderRightColor = EditorColorPalette.Danger;
      warning.style.borderBottomColor = EditorColorPalette.Danger;
      warning.style.borderLeftColor = EditorColorPalette.Danger;
      warning.style.display = DisplayStyle.None;
      section.Content.Add(warning);

      // -------- Refresh helper --------
      void Refresh()
      {
        EnsureView(state, listProperty);
        bool listEmpty = listProperty.arraySize == 0;
        bool noResults = !listEmpty && state.Visible.Count == 0;

        emptyState.text = listEmpty
          ? "No entries — click  +  to add one."
          : "No entries match the current search.";
        emptyState.style.display = (listEmpty || noResults) ? DisplayStyle.Flex : DisplayStyle.None;
        listView.style.display = (listEmpty || noResults) ? DisplayStyle.None : DisplayStyle.Flex;

        warning.style.display = state.Duplicates.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

        var badgeLabel = countBadge.Q<Label>();
        if (badgeLabel != null) badgeLabel.text = listProperty.arraySize.ToString();

        listView.itemsSource = state.Visible;
        listView.RefreshItems();
      }

      // -------- ListView make / bind --------
      listView.makeItem = () => new KvdRowElement();
      listView.bindItem = (el, viewIdx) =>
      {
        if (!(el is KvdRowElement row)) return;
        if (viewIdx < 0 || viewIdx >= state.Visible.Count) return;
        int actualIdx = state.Visible[viewIdx];
        if (actualIdx < 0 || actualIdx >= listProperty.arraySize) return;

        var elem = listProperty.GetArrayElementAtIndex(actualIdx);
        var keyProp = elem.FindPropertyRelative("Key");
        var valueProp = elem.FindPropertyRelative("Value");

        row.IndexLabel.text = actualIdx.ToString();
        row.DupDot.style.display = state.Duplicates.Contains(actualIdx) ? DisplayStyle.Flex : DisplayStyle.None;
        row.style.backgroundColor = state.Duplicates.Contains(actualIdx)
          ? new Color(0.85f, 0.25f, 0.25f, 0.18f)
          : Color.clear;

        if (keyProp != null) row.KeyField.BindProperty(keyProp);
        if (valueProp != null) row.ValueField.BindProperty(valueProp);

        bool reorderEnabled = (state.Search ?? "").Length == 0 && !state.SortActive;
        row.UpBtn.SetEnabled(reorderEnabled && actualIdx > 0);
        row.DownBtn.SetEnabled(reorderEnabled && actualIdx < listProperty.arraySize - 1);

        row.OnUp = () =>
        {
          if (actualIdx <= 0) return;
          listProperty.MoveArrayElement(actualIdx, actualIdx - 1);
          listProperty.serializedObject.ApplyModifiedProperties();
          Invalidate(state);
          Refresh();
        };
        row.OnDown = () =>
        {
          if (actualIdx >= listProperty.arraySize - 1) return;
          listProperty.MoveArrayElement(actualIdx, actualIdx + 1);
          listProperty.serializedObject.ApplyModifiedProperties();
          Invalidate(state);
          Refresh();
        };
        row.OnRemove = () =>
        {
          if (actualIdx < 0 || actualIdx >= listProperty.arraySize) return;
          listProperty.DeleteArrayElementAtIndex(actualIdx);
          listProperty.serializedObject.ApplyModifiedProperties();
          Invalidate(state);
          Refresh();
        };
      };

      // -------- Wire up controls --------
      searchField.RegisterValueChangedCallback(evt =>
      {
        state.Search = evt.newValue ?? "";
        Invalidate(state);
        Refresh();
      });

      void UpdateSortBtnLook()
      {
        sortBtn.text = state.SortActive ? "A↓" : "⇅";
        if (state.SortActive)
        {
          sortBtn.style.borderTopWidth = 1f;
          sortBtn.style.borderBottomWidth = 1f;
          sortBtn.style.borderLeftWidth = 1f;
          sortBtn.style.borderRightWidth = 1f;
          sortBtn.style.borderTopColor = EditorUIElements.Colors.Primary;
          sortBtn.style.borderBottomColor = EditorUIElements.Colors.Primary;
          sortBtn.style.borderLeftColor = EditorUIElements.Colors.Primary;
          sortBtn.style.borderRightColor = EditorUIElements.Colors.Primary;
        }
        else
        {
          sortBtn.style.borderTopWidth = 0f;
          sortBtn.style.borderBottomWidth = 0f;
          sortBtn.style.borderLeftWidth = 0f;
          sortBtn.style.borderRightWidth = 0f;
        }
      }
      UpdateSortBtnLook();

      sortBtn.clicked += () =>
      {
        state.SortActive = !state.SortActive;
        UpdateSortBtnLook();
        Invalidate(state);
        Refresh();
      };

      addBtn.clicked += () =>
      {
        listProperty.arraySize++;
        listProperty.serializedObject.ApplyModifiedProperties();
        // Clear search so the new entry is visible
        state.Search = "";
        searchField.SetValueWithoutNotify("");
        Invalidate(state);
        Refresh();
        if (state.Visible.Count > 0)
          listView.ScrollToItem(state.Visible.Count - 1);
      };

      clearBtn.clicked += () =>
      {
        if (listProperty.arraySize == 0) return;
        if (EditorUtility.DisplayDialog(
              "Clear all entries?",
              $"Remove all {listProperty.arraySize} entries from \"{property.displayName}\"?",
              "Clear", "Cancel"))
        {
          listProperty.ClearArray();
          listProperty.serializedObject.ApplyModifiedProperties();
          Invalidate(state);
          Refresh();
        }
      };

      // External-change tracker: keep view fresh when Undo/Redo or other code mutates the list.
      card.TrackPropertyValue(listProperty, _ =>
      {
        Invalidate(state);
        Refresh();
      });

      // Initial refresh
      Refresh();

      return card;
    }

    // ----- UI Toolkit row element ------------------------------------------
    private class KvdRowElement : VisualElement
    {
      public readonly Label IndexLabel;
      public readonly VisualElement DupDot;
      public readonly PropertyField KeyField;
      public readonly PropertyField ValueField;
      public readonly Button UpBtn;
      public readonly Button DownBtn;
      public readonly Button RemoveBtn;

      public Action OnUp;
      public Action OnDown;
      public Action OnRemove;

      public KvdRowElement()
      {
        style.flexDirection = FlexDirection.Row;
        style.alignItems = Align.Center;
        style.paddingTop = 3f;
        style.paddingBottom = 3f;
        style.paddingLeft = 4f;
        style.paddingRight = 4f;
        style.borderBottomWidth = 1f;
        style.borderBottomColor = new Color(1f, 1f, 1f, 0.04f);

        IndexLabel = new Label("0");
        IndexLabel.style.minWidth = 26f;
        IndexLabel.style.width = 26f;
        IndexLabel.style.fontSize = 10f;
        IndexLabel.style.color = EditorUIElements.Colors.TextMuted;
        IndexLabel.style.unityTextAlign = TextAnchor.MiddleRight;
        IndexLabel.style.marginRight = 4f;
        Add(IndexLabel);

        DupDot = EditorUIElements.CreateStatusDot(EditorUIElements.StatusType.Error, 6f);
        DupDot.style.marginRight = 4f;
        DupDot.tooltip = "Duplicate key";
        DupDot.style.display = DisplayStyle.None;
        Add(DupDot);

        KeyField = new PropertyField { label = "" };
        KeyField.style.flexGrow = 1f;
        KeyField.style.flexBasis = 0;
        KeyField.style.flexShrink = 1;
        KeyField.style.minWidth = 60f;
        Add(KeyField);

        ValueField = new PropertyField { label = "" };
        ValueField.style.flexGrow = 2f;
        ValueField.style.flexBasis = 0;
        ValueField.style.flexShrink = 1;
        ValueField.style.minWidth = 80f;
        ValueField.style.marginLeft = 4f;
        Add(ValueField);

        UpBtn = EditorUIElements.CreateIconButton("↑", null, () => OnUp?.Invoke());
        UpBtn.tooltip = "Move up";
        UpBtn.style.marginLeft = 4f;
        Add(UpBtn);

        DownBtn = EditorUIElements.CreateIconButton("↓", null, () => OnDown?.Invoke());
        DownBtn.tooltip = "Move down";
        DownBtn.style.marginLeft = 2f;
        Add(DownBtn);

        RemoveBtn = EditorUIElements.CreateIconButton("×", null, () => OnRemove?.Invoke());
        RemoveBtn.tooltip = "Remove";
        RemoveBtn.style.marginLeft = 4f;
        Add(RemoveBtn);
      }
    }

    // ===========================================================================
    // Property utilities
    // ===========================================================================

    private static string SerializedPropertyToDisplayString(SerializedProperty p)
    {
      if (p == null) return "";
      switch (p.propertyType)
      {
        case SerializedPropertyType.String:
          return p.stringValue ?? "";
        case SerializedPropertyType.Integer:
          return p.intValue.ToString();
        case SerializedPropertyType.Float:
          return p.floatValue.ToString();
        case SerializedPropertyType.Boolean:
          return p.boolValue.ToString();
        case SerializedPropertyType.Enum:
          return p.enumValueIndex >= 0 && p.enumValueIndex < p.enumDisplayNames.Length
            ? p.enumDisplayNames[p.enumValueIndex]
            : "";
        case SerializedPropertyType.ObjectReference:
          return p.objectReferenceValue != null ? p.objectReferenceValue.name : "";
        case SerializedPropertyType.Character:
          return ((char)p.intValue).ToString();
        default:
          var v = GetValueFromProperty(p);
          return v != null ? v.ToString() : "";
      }
    }

    private static object GetValueFromProperty(SerializedProperty property)
    {
      switch (property.propertyType)
      {
        case SerializedPropertyType.Integer:
          return property.intValue;
        case SerializedPropertyType.Float:
          return property.floatValue;
        case SerializedPropertyType.String:
          return property.stringValue;
        case SerializedPropertyType.Boolean:
          return property.boolValue;
        case SerializedPropertyType.Enum:
          return property.enumValueIndex;
        case SerializedPropertyType.ObjectReference:
          return property.objectReferenceValue;
        case SerializedPropertyType.Color:
          return property.colorValue;
        case SerializedPropertyType.LayerMask:
          return property.intValue;
        case SerializedPropertyType.Vector2:
          return property.vector2Value;
        case SerializedPropertyType.Vector3:
          return property.vector3Value;
        case SerializedPropertyType.Vector4:
          return property.vector4Value;
        case SerializedPropertyType.Rect:
          return property.rectValue;
        case SerializedPropertyType.ArraySize:
          return property.intValue;
        case SerializedPropertyType.Character:
          return property.intValue;
        case SerializedPropertyType.AnimationCurve:
          return property.animationCurveValue;
        case SerializedPropertyType.Bounds:
          return property.boundsValue;
        case SerializedPropertyType.Quaternion:
          return property.quaternionValue;
        case SerializedPropertyType.ExposedReference:
          return property.exposedReferenceValue;
        case SerializedPropertyType.FixedBufferSize:
          return property.fixedBufferSize;
        case SerializedPropertyType.Vector2Int:
          return property.vector2IntValue;
        case SerializedPropertyType.Vector3Int:
          return property.vector3IntValue;
        case SerializedPropertyType.RectInt:
          return property.rectIntValue;
        case SerializedPropertyType.BoundsInt:
          return property.boundsIntValue;
        case SerializedPropertyType.Hash128:
          return property.hash128Value;
        default:
          return null;
      }
    }
  }
}
#endif
