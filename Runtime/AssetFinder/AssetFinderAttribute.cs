using System;
using UnityEngine;

namespace CupkekGames.Core
{
    /// <summary>
    /// Adds an asset finder toolbar to List or KeyValueDatabase fields.
    /// The toolbar allows searching and batch-adding ScriptableObject assets.
    /// </summary>
    /// <example>
    /// [AssetFinder(typeof(ItemSO))]
    /// [SerializeField] private List&lt;ItemSO&gt; _items;
    /// 
    /// [AssetFinder(typeof(ItemSO))]
    /// [SerializeField] private KeyValueDatabase&lt;string, ItemSO&gt; _database;
    /// </example>
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetFinderAttribute : PropertyAttribute
    {
        /// <summary>
        /// The ScriptableObject type to search for.
        /// </summary>
        public Type AssetType { get; }

        /// <summary>
        /// Optional custom key for filter persistence.
        /// If not set, auto-generated from the field path.
        /// </summary>
        public string PersistenceKey { get; set; }

        /// <summary>
        /// For KeyValueDatabase: the key type for auto-generating keys.
        /// </summary>
        public Type KeyType { get; set; }

        public AssetFinderAttribute(Type assetType)
        {
            AssetType = assetType;
        }
    }
}
