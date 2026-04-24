using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    public class SearchableDropdown : EditorWindow
    {
        private string _searchText = "";
        private string _selectedKey;
        private List<DropdownItem> _items;
        private List<DropdownItem> _filteredItems;
        private Action<string> _onSelect;
        private ScrollView _scrollView;

        private static readonly Color HoverBg = new Color(0.3f, 0.5f, 0.8f, 0.3f);
        private static readonly Color GroupBg = new Color(0f, 0f, 0f, 0.15f);
        private static readonly Color SeparatorColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        public struct DropdownItem
        {
            public string Key;
            public string DisplayName;
            public string Group;
        }

        public static void Show(Rect activatorRect, List<DropdownItem> items, Action<string> onSelect, string selectedKey = null)
        {
            var window = CreateInstance<SearchableDropdown>();
            window._items = items;
            window._filteredItems = new List<DropdownItem>(items);
            window._onSelect = onSelect;
            window._selectedKey = selectedKey;

            float width = Mathf.Max(activatorRect.width, 220);
            float height = Mathf.Clamp(38 + items.Count * 24, 150, 350);
            window.position = new Rect(activatorRect.x, activatorRect.yMax, width, height);
            window.ShowPopup();
            window.Focus();
        }

        public static void Show(VisualElement anchor, List<DropdownItem> items, Action<string> onSelect, string selectedKey = null)
        {
            var worldBound = anchor.worldBound;
            var screenPos = GUIUtility.GUIToScreenPoint(new Vector2(worldBound.x, worldBound.yMax));
            var rect = new Rect(screenPos.x, screenPos.y, worldBound.width, 1);
            Show(rect, items, onSelect, selectedKey);
        }

        private void OnLostFocus() => Close();

        private void CreateGUI()
        {
            rootVisualElement.style.paddingTop = 4;
            rootVisualElement.style.paddingBottom = 4;
            rootVisualElement.style.paddingLeft = 4;
            rootVisualElement.style.paddingRight = 4;

            var searchField = new TextField();
            searchField.textEdition.placeholder = "Search...";
            searchField.style.marginBottom = 4;
            searchField.RegisterValueChangedCallback(evt =>
            {
                _searchText = evt.newValue;
                FilterItems();
                RebuildList();
            });
            rootVisualElement.Add(searchField);

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.style.flexGrow = 1;
            rootVisualElement.Add(_scrollView);

            RebuildList();

            searchField.schedule.Execute(() => searchField.Focus());
        }

        private void RebuildList()
        {
            _scrollView.Clear();
            string currentGroup = null;

            if (_filteredItems.Count == 0)
            {
                var empty = new Label("No results");
                empty.style.unityTextAlign = TextAnchor.MiddleCenter;
                empty.style.color = new Color(0.6f, 0.6f, 0.6f);
                empty.style.paddingTop = 8;
                empty.style.unityFontStyleAndWeight = FontStyle.Italic;
                _scrollView.Add(empty);
                return;
            }

            foreach (var item in _filteredItems)
            {
                if (item.Group != currentGroup)
                {
                    currentGroup = item.Group;
                    if (!string.IsNullOrEmpty(currentGroup))
                    {
                        if (_scrollView.childCount > 0)
                        {
                            var sep = new VisualElement();
                            sep.style.height = 1;
                            sep.style.backgroundColor = SeparatorColor;
                            sep.style.marginTop = 4;
                            sep.style.marginBottom = 2;
                            _scrollView.Add(sep);
                        }

                        var groupLabel = new Label(currentGroup);
                        groupLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                        groupLabel.style.fontSize = 11;
                        groupLabel.style.paddingTop = 4;
                        groupLabel.style.paddingBottom = 2;
                        groupLabel.style.paddingLeft = 6;
                        groupLabel.style.backgroundColor = GroupBg;
                        groupLabel.style.borderTopLeftRadius = 3;
                        groupLabel.style.borderTopRightRadius = 3;
                        groupLabel.style.borderBottomLeftRadius = 3;
                        groupLabel.style.borderBottomRightRadius = 3;
                        _scrollView.Add(groupLabel);
                    }
                }

                bool isSelected = item.Key == _selectedKey;
                var capturedKey = item.Key;

                var btn = new Button(() =>
                {
                    _onSelect?.Invoke(capturedKey);
                    Close();
                })
                {
                    text = (isSelected ? "\u2713 " : "    ") + item.DisplayName
                };
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.borderTopWidth = 0;
                btn.style.borderBottomWidth = 0;
                btn.style.borderLeftWidth = 0;
                btn.style.borderRightWidth = 0;
                btn.style.backgroundColor = Color.clear;
                btn.style.paddingLeft = 6;
                btn.style.paddingTop = 2;
                btn.style.paddingBottom = 2;
                btn.style.borderTopLeftRadius = 3;
                btn.style.borderTopRightRadius = 3;
                btn.style.borderBottomLeftRadius = 3;
                btn.style.borderBottomRightRadius = 3;
                btn.style.marginTop = 1;
                btn.style.marginBottom = 1;

                if (isSelected)
                    btn.style.unityFontStyleAndWeight = FontStyle.Bold;

                btn.RegisterCallback<MouseEnterEvent>(_ => btn.style.backgroundColor = HoverBg);
                btn.RegisterCallback<MouseLeaveEvent>(_ => btn.style.backgroundColor = Color.clear);

                _scrollView.Add(btn);
            }
        }

        private void FilterItems()
        {
            if (string.IsNullOrEmpty(_searchText))
                _filteredItems = new List<DropdownItem>(_items);
            else
                _filteredItems = _items.FindAll(i =>
                    i.DisplayName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0
                    || (!string.IsNullOrEmpty(i.Group) && i.Group.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0));
        }
    }
}
