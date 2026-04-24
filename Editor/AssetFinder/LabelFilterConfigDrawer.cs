#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// Property drawer for LabelFilterConfig.
    /// </summary>
    [CustomPropertyDrawer(typeof(LabelFilterConfig))]
    public class LabelFilterConfigDrawer : PropertyDrawer
    {
        /// <summary>Per-property input buffer for the "add label" text field.</summary>
        private static readonly System.Collections.Generic.Dictionary<string, string> s_inputBuffers = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var labelsProp = property.FindPropertyRelative("_labels");
            if (labelsProp == null)
                return EditorGUIUtility.singleLineHeight;

            float lineH = EditorGUIUtility.singleLineHeight;
            float sp = EditorGUIUtility.standardVerticalSpacing;
            // One row for existing badges, one row for the add-label input
            int badgeRows = Mathf.Max(labelsProp.arraySize, 0);
            return lineH + sp + (badgeRows > 0 ? lineH + sp : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelsProp = property.FindPropertyRelative("_labels");
            if (labelsProp == null)
            {
                EditorGUI.LabelField(position, label.text, "Malformed LabelFilterConfig");
                return;
            }

            float lineH = EditorGUIUtility.singleLineHeight;
            float sp = EditorGUIUtility.standardVerticalSpacing;
            float labelW = 55f;
            float x = position.x;
            float y = position.y;

            // --- Row 1: "Labels" label + existing badges with × buttons ---
            EditorGUI.LabelField(new Rect(x, y, labelW, lineH), "Labels", EditorStyles.boldLabel);

            float badgeX = x + labelW + 4f;
            float badgeMaxX = position.xMax;
            int removeIndex = -1;

            if (labelsProp.arraySize == 0)
            {
                EditorGUI.LabelField(new Rect(badgeX, y, badgeMaxX - badgeX, lineH),
                    "(none — type below + Enter)", EditorStyles.miniLabel);
            }
            else
            {
                for (int i = 0; i < labelsProp.arraySize; i++)
                {
                    string text = labelsProp.GetArrayElementAtIndex(i).stringValue;
                    float textW = EditorStyles.miniLabel.CalcSize(new GUIContent(text)).x + 6f;
                    float removeW = 18f;
                    float totalW = textW + removeW + 4f;

                    if (badgeX + totalW > badgeMaxX && i > 0)
                        break; // overflow — stop drawing rather than overlap

                    // Badge background
                    Rect badgeRect = new Rect(badgeX, y, textW, lineH);
                    EditorGUI.DrawRect(badgeRect, new Color(0.25f, 0.45f, 0.7f, 0.6f));
                    EditorGUI.LabelField(badgeRect, text, EditorStyles.miniLabel);

                    // × button
                    if (GUI.Button(new Rect(badgeX + textW + 1f, y, removeW, lineH), "×", EditorStyles.miniButton))
                        removeIndex = i;

                    badgeX += totalW;
                }
            }

            if (removeIndex >= 0)
            {
                labelsProp.DeleteArrayElementAtIndex(removeIndex);
                labelsProp.serializedObject.ApplyModifiedProperties();
            }

            y += lineH + sp;

            // --- Row 2: text input to add new label ---
            string bufferKey = property.propertyPath;
            if (!s_inputBuffers.TryGetValue(bufferKey, out string inputText))
                inputText = "";

            float addBtnW = 40f;
            Rect fieldRect = new Rect(x + labelW + 4f, y, position.width - labelW - 4f - addBtnW - 4f, lineH);
            Rect addBtnRect = new Rect(fieldRect.xMax + 4f, y, addBtnW, lineH);

            string controlName = $"LabelInput_{bufferKey}";
            GUI.SetNextControlName(controlName);
            inputText = EditorGUI.TextField(fieldRect, inputText);
            s_inputBuffers[bufferKey] = inputText;

            bool addClicked = GUI.Button(addBtnRect, "Add", EditorStyles.miniButton);
            bool enterPressed = Event.current.type == EventType.KeyDown
                && Event.current.keyCode == KeyCode.Return
                && GUI.GetNameOfFocusedControl() == controlName;

            if (addClicked || enterPressed)
            {
                string trimmed = inputText?.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    // Check for duplicates
                    bool exists = false;
                    for (int i = 0; i < labelsProp.arraySize; i++)
                    {
                        if (labelsProp.GetArrayElementAtIndex(i).stringValue == trimmed)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        labelsProp.InsertArrayElementAtIndex(labelsProp.arraySize);
                        labelsProp.GetArrayElementAtIndex(labelsProp.arraySize - 1).stringValue = trimmed;
                        labelsProp.serializedObject.ApplyModifiedProperties();
                    }

                    s_inputBuffers[bufferKey] = "";
                }
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var labelsProp = property.FindPropertyRelative("_labels");

            var root = EditorUIElements.CreateRow();
            root.style.flexWrap = Wrap.Wrap;

            // "Labels" label
            var labelsLabel = EditorUIElements.CreateHeaderLabel("Labels");
            labelsLabel.style.width = 50f;
            root.Add(labelsLabel);

            // Badges container (wrapping)
            var badgesContainer = EditorUIElements.CreateWrapRow();
            badgesContainer.style.flexGrow = 1;
            badgesContainer.style.marginRight = 4f;
            root.Add(badgesContainer);

            // Text field for adding new label
            var addField = new TextField();
            addField.style.width = 80f;
            addField.style.marginLeft = StyleKeyword.Auto;
            addField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == UnityEngine.KeyCode.Return || evt.keyCode == UnityEngine.KeyCode.KeypadEnter)
                {
                    var labelName = addField.value.Trim();
                    if (!string.IsNullOrEmpty(labelName))
                    {
                        // Check if already exists
                        bool exists = false;
                        for (int i = 0; i < labelsProp.arraySize; i++)
                        {
                            if (labelsProp.GetArrayElementAtIndex(i).stringValue == labelName)
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            labelsProp.InsertArrayElementAtIndex(labelsProp.arraySize);
                            labelsProp.GetArrayElementAtIndex(labelsProp.arraySize - 1).stringValue = labelName;
                            labelsProp.serializedObject.ApplyModifiedProperties();
                            RebuildBadges(badgesContainer, labelsProp);
                        }

                        addField.value = "";
                    }
                    evt.StopPropagation();
                }
            });
            root.Add(addField);

            // Build badges
            RebuildBadges(badgesContainer, labelsProp);

            return root;
        }

        private void RebuildBadges(VisualElement container, SerializedProperty labelsProp)
        {
            container.Clear();

            if (labelsProp.arraySize == 0)
            {
                container.Add(EditorUIElements.CreateHintLabel("(type label + Enter)"));
                return;
            }

            for (int i = 0; i < labelsProp.arraySize; i++)
            {
                int index = i; // Capture for closure
                string labelText = labelsProp.GetArrayElementAtIndex(i).stringValue;

                var badge = EditorUIElements.CreateRemovableBadge(labelText, () =>
                {
                    labelsProp.DeleteArrayElementAtIndex(index);
                    labelsProp.serializedObject.ApplyModifiedProperties();
                    RebuildBadges(container, labelsProp);
                }, EditorUIElements.BadgeStyle.Info);

                container.Add(badge);
            }
        }
    }
}
#endif
