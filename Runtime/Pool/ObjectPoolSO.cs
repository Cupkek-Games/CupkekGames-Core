using System;
using UnityEngine;

namespace CupkekGames.Core.Pool
{
  public abstract class ObjectPoolSO<T> : ScriptableObject, IObjectPool<T> where T : class
  {
    [SerializeField] private bool prewarm = true;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 20;
    [SerializeField] private UnityEngine.Pool.ObjectPool<T> pool;

    // Events from interface
    public event Action<T> OnCreateEvent;
    public event Action<T> OnTakeFromPoolEvent;
    public event Action<T> OnReturnToPoolEvent;
    public event Action<T> OnDestroyObjectEvent;

    public UnityEngine.Pool.ObjectPool<T> Pool => pool;

    public void Init()
    {
      pool = new UnityEngine.Pool.ObjectPool<T>(CreatePooledObjectInternal, OnTakeFromPoolInternal, OnReturnToPoolInternal, OnDestroyObjectInternal, collectionCheck, defaultCapacity, maxSize);

      if (prewarm)
      {
        Prewarm();

        // Debug.Log("Created ScriptableObject pool for " + typeof(T) + " with " + Pool.CountAll + " objects");
      }
    }

    public void Prewarm()
    {
      T[] instances = new T[defaultCapacity];
      for (int i = 0; i < defaultCapacity; i++)
      {
        instances[i] = Pool.Get();
      }
      for (int i = 0; i < defaultCapacity; i++)
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