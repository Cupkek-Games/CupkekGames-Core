#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// IMGUI helpers for property drawers: draw direct children without calling <see cref="EditorGUI.PropertyField"/>
    /// on the same property the drawer is handling (avoids infinite recursion).
    /// </summary>
    public static class EditorImGuiDrawing
    {
        public static float GetChildPropertiesHeight(SerializedProperty parentProperty)
        {
            float sp = EditorGUIUtility.standardVerticalSpacing;
            float sum = 0f;
            SerializedProperty it = parentProperty.Copy();
            SerializedProperty end = parentProperty.GetEndProperty();
            if (!it.NextVisible(true))
                return 0f;
            do
            {
                if (SerializedProperty.EqualContents(it, end))
                    break;
                sum += EditorGUI.GetPropertyHeight(it, true) + sp;
            } while (it.NextVisible(false));

            return Mathf.Max(0f, sum - sp);
        }

        public static void DrawChildProperties(Rect position, SerializedProperty parentProperty, ref float y)
        {
            float sp = EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty it = parentProperty.Copy();
            SerializedProperty end = parentProperty.GetEndProperty();
            if (!it.NextVisible(true))
                return;
            do
            {
                if (SerializedProperty.EqualContents(it, end))
                    break;
                float h = EditorGUI.GetPropertyHeight(it, true);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), it, true);
                y += h + sp;
            } while (it.NextVisible(false));
        }

        /// <summary>Foldout header + child properties when expanded.</summary>
        public static float GetFoldoutWithChildrenHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float sp = EditorGUIUtility.standardVerticalSpacing;
            if (!property.isExpanded)
                return line;
            return line + sp + GetChildPropertiesHeight(property);
        }

        public static void DrawFoldoutWithChildren(Rect position, SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float sp = EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, line),
                property.isExpanded,
                label,
                true);
            float y = position.y + line + sp;
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawChildProperties(new Rect(position.x, y, position.width, position.height), property, ref y);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
