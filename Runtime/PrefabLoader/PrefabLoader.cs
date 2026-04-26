using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using CupkekGames.Core;

namespace CupkekGames.Systems
{
  public abstract class PrefabLoader<TKey> : KeyValueDatabaseMono<TKey, GameObject>, IPrefabLoader<TKey, GameObject>
  {
    private Dictionary<TKey, List<GameObject>> _instances = new();
    [SerializeField] private FolderReference _searchFolder;

    // Event for unloading UI
    public event EventHandler<TKey> OnInstanceDestroyed;

    public List<GameObject> GetInstances(TKey key)
    {
      return _instances[key];
    }

    public GameObject Instantiate(TKey key)
    {
      if (!ContainsKey(key))
      {
        Debug.LogWarning("Key not found: " + key);

        return null;
      }

      if (_instances.ContainsKey(key)) return null;

      GameObject instance = Instantiate(GetValue(key).gameObject);

      AddReportDestroy(key, instance);

      List<GameObject> list;
      if (_instances.ContainsKey(key))
      {
        list = _instances[key];
      }
      else
      {
        list = new();
      }

      list.Add(instance);

      _instances[key] = list;

      return instance;
    }

    public void DestroyAllOf(TKey key)
    {
      if (_instances.ContainsKey(key))
      {
        List<GameObject> list = _instances[key];
        foreach (GameObject go in list)
        {
          Destroy(go);
        }
        _instances.Remove(key);

        OnInstanceDestroyed?.Invoke(this, key);
      }
    }

    public IEnumerator DestroyAllOfWithDelay(TKey key, float duration)
    {
      yield return new WaitForSeconds(duration);

      List<GameObject> list = _instances[key];
      foreach (GameObject go in list)
      {
        Destroy(go);
      }
    }

    public void AddReportDestroy(object key, GameObject instance)
    {
      if (!instance.TryGetComponent<PrefabLoaderReportDestroy>(out var report))
      {
        report = instance.AddComponent<PrefabLoaderReportDestroy>();
      }

      report.PrefabLoader = this;
      report.PrefabKey = key;
    }

    public void ReportDestroy(object keyObj, GameObject instance)
    {
      TKey key = (TKey)keyObj;
      if (_instances.ContainsKey(key))
      {
        List<GameObject> list = _instances[key];
        if (list.Remove(instance))
        {
          if (list.Count == 0)
          {
            _instances.Remove(key);
          }

          OnInstanceDestroyed?.Invoke(this, key);
        }
      }
    }
    public void DestroyAll()
    {
      foreach (var key in Keys)
      {
        DestroyAllOf(key);
      }
    }
  }
}