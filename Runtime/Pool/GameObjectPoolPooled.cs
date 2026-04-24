using UnityEngine;

namespace CupkekGames.Core.Pool
{
  public class GameObjectPoolPooled : MonoBehaviour
  {
    private ObjectPoolBase<GameObject> _gameObjectPool;

    private bool _isReleased = true;

    private void OnDisable()
    {
      // Debug.Log($"[GameObjectPoolPooled] OnDisable for {gameObject.name}");
      ReleaseToPool();
    }

    private void OnDestroy()
    {
      // Debug.Log($"[GameObjectPoolPooled] OnDestroy for {gameObject.name}");
      ReleaseToPool();
    }

    private void ReleaseToPool()
    {
      // Debug.Log($"[GameObjectPoolPooled] ReleaseToPool for {gameObject.name}, isReleased: {_isReleased}");

      if (_isReleased)
      {
        // Debug.LogWarning($"[GameObjectPoolPooled] Attempted to release object {gameObject.name}, but it was already released");
        return;
      }

      // Debug.Log($"[GameObjectPoolPooled] Releasing {gameObject.name} to pool");

      if (_gameObjectPool != null)
      {
        // Debug.Log($"[GameObjectPoolPooled] Releasing {gameObject.name} to pool");
        try
        {
          _gameObjectPool.Pool.Release(gameObject);
          _gameObjectPool = null;
        }
        catch (System.InvalidOperationException e)
        {
          Debug.LogError($"[GameObjectPoolPooled] Failed to release {gameObject.name}: {e.Message}");
        }
        finally
        {
          _isReleased = true;
          // Debug.Log($"[GameObjectPoolPooled] finally for {gameObject.name}, isReleased: {_isReleased}");
        }
      }
      else
      {
        Debug.LogError($"[GameObjectPoolPooled] GameObjectPool is not set for {gameObject.name}");
      }
    }

    public void SetGameObjectPool(ObjectPoolBase<GameObject> gameObjectPool)
    {
      _gameObjectPool = gameObjectPool;
      _isReleased = false;
      // Debug.Log($"[GameObjectPoolPooled] SetGameObjectPool for {gameObject.name}, isReleased: {_isReleased}");
    }

    public void SetIsReleased(bool isReleased)
    {
      _isReleased = isReleased;
      // Debug.Log($"[GameObjectPoolPooled] SetIsReleased for {gameObject.name}, isReleased: {_isReleased}");
    }
  }
}