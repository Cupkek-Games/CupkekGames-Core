using System.Collections.Generic;

namespace CupkekGames.Core
{
  public interface IKeyValueDatabase<TKey, TValue>
  {
    public bool EditorHasKey(TKey key);
    public bool EditorAdd(TKey key, TValue value);
    public bool EditorHasDuplicateKeys();
    public void EditorClear();

    // Runtime
    public int Count { get; }
    public Dictionary<TKey, TValue>.KeyCollection Keys { get; }
    public Dictionary<TKey, TValue>.ValueCollection Values { get; }
    public System.Collections.Generic.KeyValuePair<TKey, TValue> GetRandomPair();
    public void SetValue(TKey key, TValue value);
    public bool TryAdd(TKey key, TValue value);
    public bool TryRemove(TKey key);
    public bool TryUpdate(TKey key, TValue value);
    public bool ContainsKey(TKey key);
    public TValue GetValue(TKey key);
    public bool TryGetValue(TKey key, out TValue value);
    public List<TKey> GetKeys();
    public List<TValue> GetValues();
  }
}