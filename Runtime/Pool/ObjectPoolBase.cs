using System;
using UnityEngine;
using UnityEngine.Pool;

namespace CupkekGames.Core.Pool
{
  public abstract class ObjectPoolBase<T> : IObjectPool<T> where T : class
  {
    private bool _collectionCheck = true;
    private int _defaultCapacity;
    private int _maxSize;
    private ObjectPool<T> pool;

    // Events from interface
    public event Action<T> OnCreateEvent;
    public event Action<T> OnTakeFromPoolEvent;
    public event Action<T> OnReturnToPoolEvent;
    public event Action<T> OnDestroyObjectEvent;

    public ObjectPool<T> Pool => pool;

    public ObjectPoolBase(int defaultCapacity, int maxSize, bool collectionCheck = true)
    {
      _collectionCheck = collectionCheck;
      _defaultCapacity = defaultCapacity;
      _maxSize = maxSize;

      pool = new ObjectPool<T>(CreatePooledObjectInternal, OnTakeFromPoolInternal, OnReturnToPoolInternal, OnDestroyObjectInternal, _collectionCheck, _defaultCapacity, _maxSize);
    }

    public void Prewarm()
    {
      T[] instances = new T[_defaultCapacity];
      for (int i = 0; i < _defaultCapacity; i++)
      {
        instances[i] = Pool.Get();
      }
      for (int i = 0; i < _defaultCapacity; i++)
      {
        Pool.Release(instances[i]);
      }
    }

    private T CreatePooledObjectInternal()
    {
      T instance = CreatePooledObject();
      OnCreateEvent?.Invoke(instance);
      return instance;
    }

    private void OnTakeFromPoolInternal(T instance)
    {
      OnTakeFromPool(instance);
      OnTakeFromPoolEvent?.Invoke(instance);
    }

    private void OnReturnToPoolInternal(T instance)
    {
      OnReturnToPool(instance);
      OnReturnToPoolEvent?.Invoke(instance);
    }

    private void OnDestroyObjectInternal(T instance)
    {
      OnDestroyObject(instance);
      OnDestroyObjectEvent?.Invoke(instance);
    }

    public abstract T CreatePooledObject();

    public abstract void OnTakeFromPool(T Instance);

    public abstract void OnReturnToPool(T Instance);

    public abstract void OnDestroyObject(T Instance);
  }
}