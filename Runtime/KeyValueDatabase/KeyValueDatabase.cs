using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CupkekGames.Core
{
  [Serializable]
  public class KeyValueDatabase<TKey, TValue> : IKeyValueDatabase<TKey, TValue>, ISerializationCallbackReceiver
  {
    [NonSerialized] private Dictionary<TKey, TValue> _dictionary;
    [SerializeField] private List<KeyValuePair<TKey, TValue>> _list = new();
    public List<KeyValuePair<TKey, TValue>> EditorList => _list;

    public KeyValueDatabase()
    {
      InitializeDictionary();
    }

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
      InitializeDictionary();
    }

    public void InitializeDictionary()
    {
      _dictionary = new Dictionary<TKey, TValue>();
      foreach (var pair in _list)
      {
        _dictionary.TryAdd(pair.Key, pair.Value);
      }
    }

    // Editor
    public bool EditorHasKey(TKey key)
    {
      foreach (var pair in _list)
      {
        if (pair.Key.Equals(key))
        {
          return true;
        }
      }

      return false;
    }

    public bool EditorAdd(TKey key, TValue value)
    {
      if (EditorHasKey(key))
      {
        return false;
      }

      _list.Add(new KeyValuePair<TKey, TValue>
      {
        Key = key,
        Value = value
      });

      return true;
    }

    public bool EditorHasDuplicateKeys()
    {
      HashSet<TKey> keys = new HashSet<TKey>();

      foreach (var pair in _list)
      {
        // If the key is already in the set, it's a duplicate
        if (!keys.Add(pair.Key))
        {
          return true; // Duplicate found
        }
      }

      return false; // No duplicates
    }

    public void EditorClear()
    {
      _list.Clear();
    }

    public void EditorCopy(List<KeyValuePair<TKey, TValue>> list)
    {
      _list = new List<KeyValuePair<TKey, TValue>>(list);
    }

    // Runtime
    public int Count => _dictionary.Count;
    public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;
    public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;
    public System.Collections.Generic.KeyValuePair<TKey, TValue> GetRandomPair()
    {
      return _dictionary.ElementAt(UnityEngine.Random.Range(0, _dictionary.Count));
    }
    public void SetValue(TKey key, TValue value)
    {
      if (!TryUpdate(key, value))
      {
        TryAdd(key, value);
      }
    }
    public bool TryAdd(TKey key, TValue value)
    {
      if (_dictionary.ContainsKey(key))
      {
        return false;
      }

      _dictionary.Add(key, value);
      _list.Add(new KeyValuePair<TKey, TValue> { Key = key, Value = value });
      return true;
    }
    public bool TryRemove(TKey key)
    {
      if (!_dictionary.Remove(key))
      {
        return false;
      }

      _list.RemoveAll(pair => pair.Key.Equals(key));
      return true;
    }
    public bool TryUpdate(TKey key, TValue value)
    {
      if (!_dictionary.ContainsKey(key))
      {
        return false;
      }

      _dictionary[key] = value;
      int index = _list.FindIndex(pair => pair.Key.Equals(key));
      if (index >= 0)
      {
        _list[index] = new KeyValuePair<TKey, TValue> { Key = key, Value = value };
      }
      return true;
    }
    public bool ContainsKey(TKey key)
    {
      return _dictionary.ContainsKey(key);
    }
    public TValue GetValue(TKey key)
    {
      return _dictionary.TryGetValue(key, out var value) ? value : default;
    }
    public bool TryGetValue(TKey key, out TValue value)
    {
      return _dictionary.TryGetValue(key, out value);
    }
    public List<TKey> GetKeys()
    {
      return _dictionary.Keys.ToList();
    }
    public List<TValue> GetValues()
    {
      return _dictionary.Values.ToList();
    }
  }
}