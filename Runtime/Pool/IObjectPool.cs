using System;

namespace CupkekGames.Core.Pool
{
  public interface IObjectPool<T>
  {
    // Events
    event Action<T> OnCreateEvent;
    event Action<T> OnTakeFromPoolEvent;
    event Action<T> OnReturnToPoolEvent;
    event Action<T> OnDestroyObjectEvent;

    // Methods
    T CreatePooledObject();

    void OnTakeFromPool(T Instance);

    void OnReturnToPool(T Instance);

    void OnDestroyObject(T Instance);
  }
}