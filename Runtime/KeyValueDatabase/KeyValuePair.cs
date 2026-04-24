
using System;
using UnityEngine;

namespace CupkekGames.Core
{
    [Serializable]
    public struct KeyValuePair<TKey, TValue>
    {
        [SerializeField] public TKey Key;
        [SerializeField] public TValue Value;
    }
}