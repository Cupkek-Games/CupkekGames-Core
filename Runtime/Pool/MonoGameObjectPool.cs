using UnityEngine;

namespace CupkekGames.Core.Pool
{
  public class MonoGameObjectPool : MonoBehaviour
  {
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 20;
    [SerializeField] private bool prewarm = true;
    [SerializeField] private bool collectionCheck = true;

    private GameObjectPool _pool;
    public GameObjectPool Pool => _pool;

    private void Awake()
    {
      _pool = new GameObjectPool(_prefab, defaultCapacity, maxSize, prewarm, collectionCheck);
    }

    private void OnDestroy()
    {
      _pool.Pool.Dispose();
    }
  }
}