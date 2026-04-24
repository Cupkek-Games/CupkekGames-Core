#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// Property drawer for PathFilterConfig using UI Toolkit.
    /// </summary>
    [CustomPropertyDrawer(typeof(PathFilterConfig))]
    public class PathFilterConfigDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pathPatternProp = property.FindPropertyRelative("_pathPattern");
            if (pathPatternProp == null)
            {
                EditorGUI.LabelField(position, label.text, "Malformed PathFilterConfig");
                return;
            }

            EditorGUI.PropertyField(position, pathPatternProp, new GUIContent("Path", "Path contains this substring."));
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var pathPatternProp = property.FindPropertyRelative("_pathPattern");

            var root = EditorUIElements.CreateRow();

            var label = EditorUIElements.CreateHeaderLabel("Path");
            label.style.width = 50f;
            root.Add(label);

            var textField = new TextField();
            textField.value = pathPatternProp.stringValue;
            textField.style.flexGrow = 1f;
            textField.RegisterValueChangedCallback(evt =>
            {
                pathPatternProp.stringValue = evt.newValue;
                pathPatternProp.serializedObject.ApplyModifiedProperties();
            });
            root.Add(textField);

            // Hint
            var hint = EditorUIElements.CreateHintLabel("(path contains)");
            hint.style.marginLeft = 8f;
            root.Add(hint);

            return root;
        }
    }
}
#endif
