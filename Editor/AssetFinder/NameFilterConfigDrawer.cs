#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// Property drawer for NameFilterConfig using UI Toolkit.
    /// </summary>
    [CustomPropertyDrawer(typeof(NameFilterConfig))]
    public class NameFilterConfigDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var namePatternProp = property.FindPropertyRelative("_namePattern");
            if (namePatternProp == null)
            {
                EditorGUI.LabelField(position, label.text, "Malformed NameFilterConfig");
                return;
            }

            EditorGUI.PropertyField(position, namePatternProp, new GUIContent("Name", "Partial name match."));
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var namePatternProp = property.FindPropertyRelative("_namePattern");

            var root = EditorUIElements.CreateRow();

            var label = EditorUIElements.CreateHeaderLabel("Name");
            label.style.width = 50f;
            root.Add(label);

            var textField = new TextField();
            textField.value = namePatternProp.stringValue;
            textField.style.flexGrow = 1f;
            textField.RegisterValueChangedCallback(evt =>
            {
                namePatternProp.stringValue = evt.newValue;
                namePatternProp.serializedObject.ApplyModifiedProperties();
            });
            root.Add(textField);

            // Hint
            var hint = EditorUIElements.CreateHintLabel("(partial match)");
            hint.style.marginLeft = 8f;
            root.Add(hint);

            return root;
        }
    }
}
#endif
