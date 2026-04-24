#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// Property drawer for ExcludeNameFilterConfig using UI Toolkit.
    /// </summary>
    [CustomPropertyDrawer(typeof(ExcludeNameFilterConfig))]
    public class ExcludeNameFilterConfigDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var excludePatternProp = property.FindPropertyRelative("_excludePattern");
            if (excludePatternProp == null)
            {
                EditorGUI.LabelField(position, label.text, "Malformed ExcludeNameFilterConfig");
                return;
            }

            EditorGUI.PropertyField(position, excludePatternProp,
                new GUIContent("Exclude", "Exclude assets whose name contains this substring."));
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var excludePatternProp = property.FindPropertyRelative("_excludePattern");

            var root = EditorUIElements.CreateRow();

            var label = EditorUIElements.CreateHeaderLabel("Exclude");
            label.style.width = 50f;
            root.Add(label);

            var textField = new TextField();
            textField.value = excludePatternProp.stringValue;
            textField.style.flexGrow = 1f;
            textField.RegisterValueChangedCallback(evt =>
            {
                excludePatternProp.stringValue = evt.newValue;
                excludePatternProp.serializedObject.ApplyModifiedProperties();
            });
            root.Add(textField);

            // Hint
            var hint = EditorUIElements.CreateHintLabel("(name excludes)");
            hint.style.marginLeft = 8f;
            root.Add(hint);

            return root;
        }
    }
}
#endif
