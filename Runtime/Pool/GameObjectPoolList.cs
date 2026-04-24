using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Core.Pool
{
  public class GameObjectPoolList : MonoBehaviour
  {
    private Dictionary<Guid, GameObjectPool> _poolDictionary = new();

    public Guid CreateNewPool(GameObject prefab, int defaultCapacity, int maxSize, bool prewarm = true, bool collectionCheck = true)
    {
      GameObjectPool pool = new GameObjectPool(prefab, defaultCapacity, maxSize, prewarm, collectionCheck);

      Guid id = Guid.NewGuid();

      _poolDictionary.Add(id, pool);

      return id;
    }

    public GameObjectPool GetPool(Guid id)
    {
      return _poolDictionary[id];
    }

    private void OnDestroy()
    {
      Dispose();
    }

    public void Dispose()
    {
      foreach (System.Collections.Generic.KeyValuePair<Guid, GameObjectPool> kvp in _poolDictionary)
      {
        kvp.Value.Pool.Dispose();
      }

      _poolDictionary.Clear();
    }
  }
}