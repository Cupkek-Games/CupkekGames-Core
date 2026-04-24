using UnityEngine;

namespace CupkekGames.Core.Pool
{
  public class GameObjectPool : GameObjectPoolBase
  {
    protected GameObject _prefab;
    public GameObjectPool(GameObject prefab, int defaultCapacity, int maxSize, bool prewarm = true, bool collectionCheck = true) : base(defaultCapacity, maxSize, collectionCheck)
    {
      _prefab = prefab;

      if (prewarm)
      {
        Prewarm();

        // Debug.Log("Created GameObject pool for " + typeof(GameObject) + " with " + Pool.CountAll + " objects");
      }
    }

    public override GameObject CreateObject()
    {
      return GameObject.Instantiate(_prefab);
    }
  }
}