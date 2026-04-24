using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    public class EditorTabView : VisualElement
    {
        private static readonly Color ActiveBg = new(0.25f, 0.25f, 0.25f);
        private static readonly Color InactiveBg = new(0.15f, 0.15f, 0.15f);
        private static readonly Color ActiveText = Color.white;
        private static readonly Color InactiveText = new(0.6f, 0.6f, 0.6f);
        private static readonly Color Border = new(0.2f, 0.2f, 0.2f);

        private readonly VisualElement _tabBar;
        private readonly VisualElement _tabBarRight;
        private readonly VisualElement _content;
        private readonly List<(Button button, VisualElement content)> _tabs = new();
        private int _activeIndex = -1;

        public int ActiveIndex => _activeIndex;
        public event Action<int> OnTabChanged;

        public EditorTabView()
        {
            _tabBar = new VisualElement();
            _tabBar.style.flexDirection = FlexDirection.Row;
            _tabBar.style.alignItems = Align.Center;

            _tabBarRight = new VisualElement();
            _tabBarRight.style.flexDirection = FlexDirection.Row;
            _tabBarRight.style.alignItems = Align.Center;
            _tabBarRight.style.flexGrow = 3;
            _tabBarRight.style.justifyContent = Justify.FlexEnd;
            _tabBar.Add(_tabBarRight);

            hierarchy.Add(_tabBar);

            _content = new VisualElement();
            _content.style.borderLeftWidth = 2;
            _content.style.borderRightWidth = 2;
            _content.style.borderBottomWidth = 2;
            _content.style.borderLeftColor = Border;
            _content.style.borderRightColor = Border;
            _content.style.borderBottomColor = Border;
            _content.style.borderBottomLeftRadius = 4;
            _content.style.borderBottomRightRadius = 4;
            _content.style.backgroundColor = ActiveBg;
            _content.style.paddingTop = 4;
            _content.style.paddingBottom = 4;
            _content.style.paddingLeft = 16;
            _content.style.paddingRight = 4;
            hierarchy.Add(_content);
        }

        public VisualElement AddTab(string label, VisualElement tabContent = null)
        {
            int index = _tabs.Count;
            tabContent ??= new VisualElement();

            var btn = new Button(() => SetActive(index)) { text = label };
            btn.style.flexGrow = 1;
            btn.style.borderTopLeftRadius = 4;
            btn.style.borderTopRightRadius = 4;
            btn.style.borderBottomLeftRadius = 0;
            btn.style.borderBottomRightRadius = 0;
            btn.style.borderBottomWidth = 0;
            btn.style.borderLeftWidth = 2;
            btn.style.borderRightWidth = 2;
            btn.style.borderTopWidth = 2;
            btn.style.marginLeft = 0;
            btn.style.marginRight = 0;
            btn.style.marginBottom = 0;
            btn.style.paddingTop = 4;
            btn.style.paddingBottom = 4;
            _tabBar.Insert(_tabBar.IndexOf(_tabBarRight), btn);

            tabContent.style.display = DisplayStyle.None;
            _content.Add(tabContent);

            _tabs.Add((btn, tabContent));
            ApplyButtonState(btn, false);

            if (_tabs.Count == 1)
                SetActive(0);

            return tabContent;
        }

        public void SetActive(int index)
        {
            if (index < 0 || index >= _tabs.Count) return;

            if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
            {
                _tabs[_activeIndex].content.style.display = DisplayStyle.None;
                ApplyButtonState(_tabs[_activeIndex].button, false);
            }

            _activeIndex = index;
            _tabs[index].content.style.display = DisplayStyle.Flex;
            ApplyButtonState(_tabs[index].button, true);
            OnTabChanged?.Invoke(index);
        }

        public void AddToolbarRight(VisualElement element)
        {
            _tabBarRight.Add(element);
        }

        private static void ApplyButtonState(Button btn, bool active)
        {
            btn.style.backgroundColor = active ? ActiveBg : InactiveBg;
            btn.style.color = active ? ActiveText : InactiveText;
            btn.style.borderTopColor = active ? Border : Color.clear;
            btn.style.borderLeftColor = active ? Border : Color.clear;
            btn.style.borderRightColor = active ? Border : Color.clear;
        }
    }
}
