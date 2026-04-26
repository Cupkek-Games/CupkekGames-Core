#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Systems.Editor
{
    public abstract class PrefabLoaderEditor<TKey, TValue> : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // Create a container for the UI elements
            VisualElement container = new VisualElement();

            // Default inspector elements
            InspectorElement.FillDefaultInspector(container, serializedObject, this);

            // Reference to the target script
            PrefabLoader<TKey> prefabLoader = (PrefabLoader<TKey>)target;

            // Label with word wrapping
            Label searchLabel = new Label(
                $"Search for Prefabs containing {typeof(TValue).Name} in folder and add them to the list.\n" +
                "This usually takes less than 5 seconds, but may vary with project size." +
                "You may need to click elsewhere and then click this object again to refresh it."
            );
            searchLabel.style.whiteSpace = WhiteSpace.Normal;  // Enable word wrapping
            container.Add(searchLabel);

            // Add the "Find All Prefabs In Folder" button
            Button findButton = new Button(() =>
            {
                SerializedProperty folder = serializedObject.FindProperty("_searchFolder");
                string guid = folder.FindPropertyRelative("GUID").stringValue;
                string searchFolder = AssetDatabase.GUIDToAssetPath(guid);

                Debug.Log($"Searching folder {searchFolder}...");

                FindPrefabs(prefabLoader, searchFolder);
            })
            {
                text = "Find All Prefabs In Folder"
            };
            findButton.style.flexGrow = 1;
            container.Add(findButton);

            // Add the "Clear" button
            Button clearButton = new Button(() =>
            {
                prefabLoader.EditorClear();
                EditorUtility.SetDirty(prefabLoader);
                serializedObject.ApplyModifiedProperties();
                Debug.Log("Cleared prefab loader.");
            })
            {
                text = "Clear"
            };
            clearButton.style.flexGrow = 1;
            container.Add(clearButton);

            return container;
        }


        private void FindPrefabs(PrefabLoader<TKey> prefabLoader, string searchFolder)
        {
            List<GameObject> prefabsWithScript = new List<GameObject>();

            // Get all prefab paths in the project
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { searchFolder });

            foreach (string prefabPath in prefabPaths)
            {
                string fullPath = AssetDatabase.GUIDToAssetPath(prefabPath);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);

                if (prefab.GetComponent<TValue>() != null)
                {
                    prefabsWithScript.Add(prefab);
                }
            }

            if (prefabsWithScript.Count > 0)
            {
                Debug.Log($"Found {prefabsWithScript.Count} prefabs with the script {typeof(TValue).Name}:");
                foreach (GameObject prefab in prefabsWithScript)
                {
                    prefabLoader.EditorAdd(GetKeyFromFileName(prefab.name), prefab);
                }

                EditorUtility.SetDirty(prefabLoader);
                Repaint();
            }
            else
            {
                Debug.Log($"No prefabs found with the script {typeof(TValue).Name}.");
            }
        }
        public abstract TKey GetKeyFromFileName(string name);
    }
}
#endif