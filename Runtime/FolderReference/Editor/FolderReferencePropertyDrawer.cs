#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    [CustomPropertyDrawer(typeof(FolderReference))]
    public class FolderReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var guid = property.FindPropertyRelative("GUID");
            if (guid == null)
            {
                EditorGUI.LabelField(position, label.text, "Malformed FolderReference");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            Object folderObject = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid.stringValue));
            EditorGUI.BeginChangeCheck();
            Object picked = EditorGUI.ObjectField(position, label, folderObject, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (picked != null)
                {
                    string path = AssetDatabase.GetAssetPath(picked);
                    if (Directory.Exists(path))
                    {
                        guid.stringValue = AssetDatabase.AssetPathToGUID(path);
                        guid.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    guid.stringValue = "";
                    guid.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var guid = property.FindPropertyRelative("GUID");
            if (guid == null)
                return new Label($"{property.displayName}: Malformed FolderReference");

            var folderObject = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid.stringValue));

            var field = new ObjectField(property.displayName)
            {
                objectType = typeof(DefaultAsset),
                allowSceneObjects = false,
                value = folderObject
            };

            field.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null)
                {
                    string path = AssetDatabase.GetAssetPath(evt.newValue);
                    if (Directory.Exists(path))
                    {
                        guid.stringValue = AssetDatabase.AssetPathToGUID(path);
                        guid.serializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        // Not a folder — revert
                        field.SetValueWithoutNotify(evt.previousValue);
                    }
                }
                else
                {
                    guid.stringValue = "";
                    guid.serializedObject.ApplyModifiedProperties();
                }
            });

            return field;
        }
    }
}
#endif
