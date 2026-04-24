#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// A reusable collapsible section with a header row (toggle + label + count + right-side slot)
    /// and a content container. Supports persistence via SerializedProperty.isExpanded or EditorPrefs.
    /// </summary>
    public class FoldoutSection : VisualElement
    {
        private readonly Label _arrow;
        private readonly Label _label;
        private readonly VisualElement _headerRight;
        private readonly VisualElement _content;
        private readonly string _editorPrefsKey;
        private readonly SerializedProperty _expandedProperty;
        private bool _expanded;

        /// <summary>Fired when the expanded state changes.</summary>
        public event Action<bool> OnExpandedChanged;

        /// <summary>The right-side slot in the header row. Add buttons or other elements here.</summary>
        public VisualElement HeaderRight => _headerRight;

        /// <summary>The collapsible content container. Add your content here.</summary>
        public VisualElement Content => _content;

        /// <summary>Whether the section is currently expanded.</summary>
        public bool Expanded
        {
            get => _expanded;
            set => SetExpanded(value);
        }

        /// <summary>
        /// Create a FoldoutSection with EditorPrefs persistence.
        /// </summary>
        public FoldoutSection(string label, string editorPrefsKey, bool defaultExpanded = false)
        {
            _editorPrefsKey = editorPrefsKey;
            _expanded = EditorPrefs.GetBool(editorPrefsKey, defaultExpanded);
            _arrow = CreateArrow();
            _label = CreateLabel(label);
            _headerRight = new VisualElement();
            _content = CreateContent();
            Build();
        }

        /// <summary>
        /// Create a FoldoutSection with SerializedProperty.isExpanded persistence.
        /// </summary>
        public FoldoutSection(string label, SerializedProperty property)
        {
            _expandedProperty = property;
            _expanded = property.isExpanded;
            _arrow = CreateArrow();
            _label = CreateLabel(label);
            _headerRight = new VisualElement();
            _content = CreateContent();
            Build();
        }

        /// <summary>Update the label text (e.g. to refresh a count).</summary>
        public void SetLabel(string text)
        {
            _label.text = text;
        }

        private void Build()
        {
            var headerRow = EditorUIElements.CreateRow(Align.Center, Justify.SpaceBetween);

            // Left side: clickable toggle
            var toggle = new Button(Toggle);
            toggle.style.flexDirection = FlexDirection.Row;
            toggle.style.alignItems = Align.Center;
            toggle.style.backgroundColor = Color.clear;
            toggle.style.borderLeftWidth = 0;
            toggle.style.borderRightWidth = 0;
            toggle.style.borderTopWidth = 0;
            toggle.style.borderBottomWidth = 0;
            toggle.style.paddingLeft = 0;
            toggle.style.paddingRight = 8;
            toggle.style.paddingTop = 2;
            toggle.style.paddingBottom = 2;
            toggle.style.marginLeft = 0;
            toggle.Add(_arrow);
            toggle.Add(_label);
            headerRow.Add(toggle);

            // Right side slot
            _headerRight.style.flexDirection = FlexDirection.Row;
            _headerRight.style.alignItems = Align.Center;
            _headerRight.style.justifyContent = Justify.FlexEnd;
            headerRow.Add(_headerRight);

            Add(headerRow);
            Add(_content);

            ApplyExpandedState();
        }

        private void Toggle()
        {
            SetExpanded(!_expanded);
        }

        private void SetExpanded(bool value)
        {
            _expanded = value;

            if (_editorPrefsKey != null)
                EditorPrefs.SetBool(_editorPrefsKey, value);

            if (_expandedProperty != null)
                _expandedProperty.isExpanded = value;

            ApplyExpandedState();
            OnExpandedChanged?.Invoke(value);
        }

        private void ApplyExpandedState()
        {
            _content.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;
            _arrow.text = _expanded ? "▼" : "▶";
        }

        private static Label CreateArrow()
        {
            var arrow = new Label("▶");
            arrow.style.fontSize = 10;
            arrow.style.width = 14;
            arrow.style.unityTextAlign = TextAnchor.MiddleCenter;
            return arrow;
        }

        private static Label CreateLabel(string text)
        {
            var label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            return label;
        }

        private static VisualElement CreateContent()
        {
            var content = new VisualElement();
            content.style.marginLeft = 20f;
            content.style.marginTop = 4f;
            return content;
        }
    }
}
#endif
