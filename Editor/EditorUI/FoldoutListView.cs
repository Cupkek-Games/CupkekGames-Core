#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// A reusable foldout containing a ListView with an inline editable size field.
    /// Composes FoldoutSection for the collapsible header.
    /// </summary>
    public class FoldoutListView : VisualElement
    {
        public ListView ListView { get; }
        public IntegerField SizeField { get; }
        public FoldoutSection Section { get; }

        public FoldoutListView(SerializedProperty arrayProperty, string label = null, float maxHeight = 400f)
        {
            Section = new FoldoutSection(label ?? arrayProperty.displayName, arrayProperty);

            ListView = new ListView();

            // Editable size field next to foldout label
            SizeField = new IntegerField { value = arrayProperty.arraySize };
            SizeField.style.width = 50;
            SizeField.style.marginLeft = 4f;
            SizeField.style.minWidth = 40;

            // Remove inner padding so the number text isn't clipped on the left
            var textInput = SizeField.Q(className: "unity-text-element");
            if (textInput != null)
                textInput.style.marginLeft = 2f;
            SizeField.RegisterValueChangedCallback(evt =>
            {
                int newSize = Mathf.Max(0, evt.newValue);
                arrayProperty.arraySize = newSize;
                arrayProperty.serializedObject.ApplyModifiedProperties();
                ListView.schedule.Execute(() => ListView.RefreshItems());
            });

            Section.HeaderRight.Add(SizeField);

            // Clear all button
            var clearButton = EditorUIElements.CreateIconButton("×", null, () =>
            {
                arrayProperty.arraySize = 0;
                arrayProperty.serializedObject.ApplyModifiedProperties();
                SizeField.SetValueWithoutNotify(0);
                ListView.schedule.Execute(() => ListView.RefreshItems());
            });
            clearButton.tooltip = "Clear all";
            Section.HeaderRight.Add(clearButton);

            // Configure ListView
            ListView.showBoundCollectionSize = false;
            ListView.showAddRemoveFooter = true;
            ListView.showFoldoutHeader = false;
            ListView.reorderable = true;
            ListView.reorderMode = ListViewReorderMode.Animated;
            ListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            ListView.BindProperty(arrayProperty);

            if (maxHeight > 0)
                ListView.style.maxHeight = maxHeight;

            // Keep size field in sync
            ListView.itemsAdded += _ => SizeField.SetValueWithoutNotify(arrayProperty.arraySize);
            ListView.itemsRemoved += _ => SizeField.SetValueWithoutNotify(arrayProperty.arraySize);

            Section.Content.Add(ListView);
            Add(Section);
        }

        /// <summary>
        /// Updates the size field to reflect the current array size.
        /// Call after externally modifying the bound property.
        /// </summary>
        public void RefreshSize(int size)
        {
            SizeField.SetValueWithoutNotify(size);
            ListView.schedule.Execute(() => ListView.RefreshItems());
        }
    }
}
#endif
