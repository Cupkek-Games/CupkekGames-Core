using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CupkekGames.Core
{
  public abstract class KeyValueDatabaseMono<TKey, TValue> : MonoBehaviour, IKeyValueDatabase<TKey, TValue>
  {
    [SerializeField] private KeyValueDatabase<TKey, TValue> _pairs = new();

    public int Count => _pairs.Count;
    public Dictionary<TKey, TValue>.KeyCollection Keys => _pairs.Keys;
    public Dictionary<TKey, TValue>.ValueCollection Values => _pairs.Values;

    protected virtual void Awake()
    {
      _pairs.InitializeDictionary();
    }

    public bool EditorHasKey(TKey key)
    {
      return _pairs.EditorHasKey(key);
    }

    public bool EditorAdd(TKey key, TValue value)
    {
      return _pairs.EditorAdd(key, value);
    }

    public System.Collections.Generic.KeyValuePair<TKey, TValue> GetRandomPair()
    {
      return _pairs.GetRandomPair();
    }
    public void EditorClear()
    {
      _pairs.EditorClear();
    }
    public bool EditorHasDuplicateKeys()
    {
      return _pairs.EditorHasDuplicateKeys();
    }

    public void SetValue(TKey key, TValue value)
    {
      _pairs.SetValue(key, value);
    }

    public bool TryAdd(TKey key, TValue value)
    {
      return _pairs.TryAdd(key, value);
    }

    public bool TryRemove(TKey key)
    {
      return _pairs.TryRemove(key);
    }

    public bool TryUpdate(TKey key, TValue value)
    {
      return _pairs.TryUpdate(key, value);
    }

    public bool ContainsKey(TKey key)
    {
      return _pairs.ContainsKey(key);
    }

    public TValue GetValue(TKey key)
    {
      return _pairs.GetValue(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      return _pairs.TryGetValue(key, out value);
    }

    public List<TKey> GetKeys()
    {
      return _pairs.GetKeys();
    }

    public List<TValue> GetValues()
    {
      return _pairs.GetValues();
    }
  }
}