using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CupkekGames.Core
{
    /// <summary>
    /// Base class for serializable asset finder filter configurations.
    /// Use [SerializeReference] to store polymorphic filter lists.
    /// </summary>
    [Serializable]
    public abstract class AssetFinderFilterConfig
    {
        /// <summary>
        /// Display name for this filter type.
        /// </summary>
        public abstract string FilterName { get; }
    }

    /// <summary>
    /// Filter by asset name (case-insensitive contains).
    /// </summary>
    [Serializable]
    public class NameFilterConfig : AssetFinderFilterConfig
    {
        public override string FilterName => "Name";

        [SerializeField] private string _namePattern;

        public string NamePattern
        {
            get => _namePattern;
            set => _namePattern = value;
        }
    }

    /// <summary>
    /// Filter by Unity asset labels.
    /// </summary>
    [Serializable]
    public class LabelFilterConfig : AssetFinderFilterConfig
    {
        public override string FilterName => "Labels";

        [SerializeField] private string[] _labels = Array.Empty<string>();

        public string[] Labels => _labels ?? Array.Empty<string>();

        public void AddLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return;
            var list = new List<string>(_labels ?? Array.Empty<string>());
            if (!list.Contains(label))
            {
                list.Add(label);
                _labels = list.ToArray();
            }
        }

        public void RemoveLabel(string label)
        {
            if (_labels == null) return;
            _labels = _labels.Where(l => l != label).ToArray();
        }
    }

    /// <summary>
    /// Filter by asset path (folder or path contains).
    /// </summary>
    [Serializable]
    public class PathFilterConfig : AssetFinderFilterConfig
    {
        public override string FilterName => "Path";

        [SerializeField] private string _pathPattern;

        public string PathPattern
        {
            get => _pathPattern;
            set => _pathPattern = value;
        }
    }

    /// <summary>
    /// Exclude assets by name pattern.
    /// </summary>
    [Serializable]
    public class ExcludeNameFilterConfig : AssetFinderFilterConfig
    {
        public override string FilterName => "Exclude Name";

        [SerializeField] private string _excludePattern;

        public string ExcludePattern
        {
            get => _excludePattern;
            set => _excludePattern = value;
        }
    }

    /// <summary>
    /// Filter by type (class or interface).
    /// Stores the full type name for serialization.
    /// </summary>
    [Serializable]
    public class TypeFilterConfig : AssetFinderFilterConfig
    {
        public override string FilterName => "Type";

        [SerializeField] private string _typeFullName;

        /// <summary>
        /// The full name of the type to filter by.
        /// </summary>
        public string TypeFullName
        {
            get => _typeFullName;
            set => _typeFullName = value;
        }

        /// <summary>
        /// Gets the Type object from the stored full name.
        /// Returns null if type not found or not set.
        /// </summary>
        public Type GetFilterType()
        {
            if (string.IsNullOrEmpty(_typeFullName))
                return null;

            // Try direct lookup
            var type = Type.GetType(_typeFullName);
            if (type != null)
                return type;

            // Search all assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(_typeFullName);
                if (type != null)
                    return type;
            }

            return null;
        }

        /// <summary>
        /// Sets the filter type.
        /// </summary>
        public void SetFilterType(Type type)
        {
            _typeFullName = type?.FullName;
        }
    }
}
