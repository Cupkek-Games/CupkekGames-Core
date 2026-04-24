#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// Options for filtering asset searches.
    /// </summary>
    public class AssetFinderOptions
    {
        /// <summary>
        /// Asset labels to filter by (all must match).
        /// </summary>
        public string[] Labels;
    }

    /// <summary>
    /// Result of an asset search operation.
    /// </summary>
    public class AssetFinderResult<T> where T : Object
    {
        public List<T> Assets = new();
        public int FoundCount => Assets.Count;
        public string Error;
        public bool Success => string.IsNullOrEmpty(Error);
    }

    /// <summary>
    /// Utility class for finding assets in the project.
    /// </summary>
    public static class AssetFinder
    {
        /// <summary>
        /// Find all assets of type T in the specified folder.
        /// </summary>
        public static AssetFinderResult<T> FindAssets<T>(string folderPath, AssetFinderOptions options = null) where T : Object
        {
            return FindAssets<T>(new[] { folderPath }, options);
        }

        /// <summary>
        /// Find all assets of type T in the specified folders.
        /// </summary>
        public static AssetFinderResult<T> FindAssets<T>(string[] folderPaths, AssetFinderOptions options = null) where T : Object
        {
            var result = new AssetFinderResult<T>();
            options ??= new AssetFinderOptions();

            try
            {
                // Build search filter
                string typeFilter = $"t:{typeof(T).Name}";
                string labelFilter = "";

                if (options.Labels != null && options.Labels.Length > 0)
                {
                    foreach (var label in options.Labels)
                    {
                        labelFilter += $" l:{label}";
                    }
                }

                string filter = typeFilter + labelFilter;

                // Find assets
                string[] guids = AssetDatabase.FindAssets(filter, folderPaths);

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                    if (asset != null)
                        result.Assets.Add(asset);
                }
            }
            catch (Exception e)
            {
                result.Error = e.Message;
            }

            return result;
        }

        /// <summary>
        /// Find all assets of a runtime type in the specified folder.
        /// </summary>
        public static AssetFinderResult<Object> FindAssets(Type type, string folderPath, AssetFinderOptions options = null)
        {
            return FindAssets(type, new[] { folderPath }, options);
        }

        /// <summary>
        /// Find all assets of a runtime type in the specified folders.
        /// </summary>
        public static AssetFinderResult<Object> FindAssets(Type type, string[] folderPaths, AssetFinderOptions options = null)
        {
            var result = new AssetFinderResult<Object>();
            options ??= new AssetFinderOptions();

            try
            {
                string typeFilter = $"t:{type.Name}";
                string labelFilter = "";

                if (options.Labels != null && options.Labels.Length > 0)
                {
                    foreach (var label in options.Labels)
                    {
                        labelFilter += $" l:{label}";
                    }
                }

                string filter = typeFilter + labelFilter;
                string[] guids = AssetDatabase.FindAssets(filter, folderPaths);

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Object asset = AssetDatabase.LoadAssetAtPath(path, type);

                    if (asset != null)
                        result.Assets.Add(asset);
                }
            }
            catch (Exception e)
            {
                result.Error = e.Message;
            }

            return result;
        }

        /// <summary>
        /// Find all assets of type T in the entire project (Assets + Packages).
        /// </summary>
        public static AssetFinderResult<T> FindAllAssets<T>(AssetFinderOptions options = null) where T : Object
        {
            // Pass null to search everywhere including Packages
            return FindAssets<T>((string[])null, options);
        }

        /// <summary>
        /// Find all assets of a runtime type in the entire project (Assets + Packages).
        /// </summary>
        public static AssetFinderResult<Object> FindAllAssets(Type type, AssetFinderOptions options = null)
        {
            // Pass null to search everywhere including Packages
            return FindAssets(type, (string[])null, options);
        }

        /// <summary>
        /// Find all assets of type T using the provided filter configs.
        /// </summary>
        public static List<T> FindAssets<T>(List<AssetFinderFilterConfig> filters) where T : Object
        {
            var result = new List<T>();

            // Build search parameters from filters
            var labels = new List<string>();
            string namePattern = null;
            string pathPattern = null;
            string excludeNamePattern = null;

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (filter is LabelFilterConfig labelFilter)
                    {
                        var filterLabels = labelFilter.Labels;
                        if (filterLabels != null)
                        {
                            labels.AddRange(filterLabels);
                        }
                    }
                    else if (filter is NameFilterConfig nameFilter)
                    {
                        namePattern = nameFilter.NamePattern;
                    }
                    else if (filter is PathFilterConfig pathFilter)
                    {
                        pathPattern = pathFilter.PathPattern;
                    }
                    else if (filter is ExcludeNameFilterConfig excludeFilter)
                    {
                        excludeNamePattern = excludeFilter.ExcludePattern;
                    }
                }
            }

            // Build search filter
            string typeFilter = $"t:{typeof(T).Name}";
            string labelPart = "";

            foreach (var label in labels)
            {
                labelPart += $" l:{label}";
            }

            string searchFilter = typeFilter + labelPart;

            // Search all assets
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset != null)
                {
                    // Apply name filter if specified
                    if (!string.IsNullOrEmpty(namePattern))
                    {
                        if (!asset.name.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    // Apply path filter if specified
                    if (!string.IsNullOrEmpty(pathPattern))
                    {
                        if (!path.Contains(pathPattern, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    // Apply exclude name filter if specified
                    if (!string.IsNullOrEmpty(excludeNamePattern))
                    {
                        if (asset.name.Contains(excludeNamePattern, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    result.Add(asset);
                }
            }

            return result;
        }

        /// <summary>
        /// Find all assets of a runtime type using the provided filter configs.
        /// </summary>
        public static List<Object> FindAssets(Type type, List<AssetFinderFilterConfig> filters)
        {
            var result = new List<Object>();

            // Build search parameters from filters
            var labels = new List<string>();
            string namePattern = null;
            string pathPattern = null;
            string excludeNamePattern = null;

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (filter is LabelFilterConfig labelFilter)
                    {
                        var filterLabels = labelFilter.Labels;
                        if (filterLabels != null)
                        {
                            labels.AddRange(filterLabels);
                        }
                    }
                    else if (filter is NameFilterConfig nameFilter)
                    {
                        namePattern = nameFilter.NamePattern;
                    }
                    else if (filter is PathFilterConfig pathFilter)
                    {
                        pathPattern = pathFilter.PathPattern;
                    }
                    else if (filter is ExcludeNameFilterConfig excludeFilter)
                    {
                        excludeNamePattern = excludeFilter.ExcludePattern;
                    }
                }
            }

            // Build search filter
            string typeFilter = $"t:{type.Name}";
            string labelPart = "";

            foreach (var label in labels)
            {
                labelPart += $" l:{label}";
            }

            string searchFilter = typeFilter + labelPart;

            // Search all assets
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath(path, type);

                if (asset != null)
                {
                    // Apply name filter if specified
                    if (!string.IsNullOrEmpty(namePattern))
                    {
                        if (!asset.name.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    // Apply path filter if specified
                    if (!string.IsNullOrEmpty(pathPattern))
                    {
                        if (!path.Contains(pathPattern, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    // Apply exclude name filter if specified
                    if (!string.IsNullOrEmpty(excludeNamePattern))
                    {
                        if (asset.name.Contains(excludeNamePattern, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    result.Add(asset);
                }
            }

            return result;
        }
    }
}
#endif
