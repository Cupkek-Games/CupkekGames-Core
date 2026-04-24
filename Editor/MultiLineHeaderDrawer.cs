#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    [CustomPropertyDrawer(typeof(MultiLineHeaderAttribute))]
    public class MultiLineHeaderDrawer : DecoratorDrawer
    {
        private static GUIStyle _style;

        private static GUIStyle Style
        {
            get
            {
                if (_style == null)
                {
                    _style = new GUIStyle(EditorStyles.helpBox)
                    {
                        wordWrap = true,
                        fontSize = 11,
                        padding = new RectOffset(10, 10, 5, 5)
                    };
                }
                return _style;
            }
        }

        public override float GetHeight()
        {
            MultiLineHeaderAttribute header = (MultiLineHeaderAttribute)attribute;
            float width = EditorGUIUtility.currentViewWidth > 0 ? EditorGUIUtility.currentViewWidth - 20 : 300;
            return Style.CalcHeight(new GUIContent(header.headerText), width) + 10;
        }

        public override void OnGUI(Rect position)
        {
            MultiLineHeaderAttribute header = (MultiLineHeaderAttribute)attribute;
            position.y += 5;
            position.height -= 10;
            EditorGUI.LabelField(position, header.headerText, Style);
        }

        public override VisualElement CreatePropertyGUI()
        {
            MultiLineHeaderAttribute header = (MultiLineHeaderAttribute)attribute;

            var label = new Label(header.headerText);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.backgroundColor = new Color(0f, 0f, 0f, 0.2f);
            label.style.borderTopColor = new Color(0f, 0f, 0f, 0.4f);
            label.style.borderBottomColor = new Color(0f, 0f, 0f, 0.4f);
            label.style.borderLeftColor = new Color(0f, 0f, 0f, 0.4f);
            label.style.borderRightColor = new Color(0f, 0f, 0f, 0.4f);
            label.style.borderTopWidth = 1f;
            label.style.borderBottomWidth = 1f;
            label.style.borderLeftWidth = 1f;
            label.style.borderRightWidth = 1f;
            label.style.borderTopLeftRadius = 4f;
            label.style.borderTopRightRadius = 4f;
            label.style.borderBottomLeftRadius = 4f;
            label.style.borderBottomRightRadius = 4f;
            label.style.paddingTop = 5f;
            label.style.paddingBottom = 5f;
            label.style.paddingLeft = 10f;
            label.style.paddingRight = 10f;
            label.style.marginTop = 5f;
            label.style.marginBottom = 5f;
            label.style.marginLeft = 5f;
            label.style.marginRight = 5f;

            return label;
        }
    }
}
#endif
