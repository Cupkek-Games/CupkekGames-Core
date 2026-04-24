using UnityEngine;

namespace CupkekGames.Core.Pool
{
  public abstract class GameObjectPoolBase : ObjectPoolBase<GameObject>
  {
    public GameObjectPoolBase(int defaultCapacity, int maxSize, bool collectionCheck = true) : base(defaultCapacity, maxSize, collectionCheck)
    {
    }

    public virtual GameObject CreateObject()
    {
      return null;
    }

    public override GameObject CreatePooledObject()
    {
      GameObject instance = CreateObject();
      if (instance == null)
      {
        Debug.LogError($"Failed to create pooled object");
        return null;
      }

      instance.SetActive(false);

      if (instance.GetComponent<GameObjectPoolPooled>() == null)
      {
          GameObjectPoolPooled pooledObject = instance.AddComponent<GameObjectPoolPooled>();
          pooledObject.SetIsReleased(true);
      }

      return instance;
    }

    public override void OnTakeFromPool(GameObject instance)
    {
      if (instance == null)
      {
        return;
      }
      
      GameObjectPoolPooled pooledGameObject = instance.GetComponent<GameObjectPoolPooled>();
      pooledGameObject.SetGameObjectPool(this);
    }

    public override void OnReturnToPool(GameObject instance)
    {
      GameObjectPoolPooled pooledGameObject = instance.GetComponent<GameObjectPoolPooled>();
      pooledGameObject.SetIsReleased(true);
      
      instance.SetActive(false);
    }

    public override void OnDestroyObject(GameObject instance)
    {
      if (Application.isPlaying && instance != null)
      {
        GameObject.Destroy(instance);
      }
    }
  }
}