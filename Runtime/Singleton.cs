using UnityEngine;

namespace CupkekGames.Core
{
  // Singleton pattern that ensures only one instance of the class exists and provides global access.
  public abstract class Singleton<T> : MonoBehaviour where T : Component
  {
    #region Fields

    /// <summary>
    /// The static instance.
    /// </summary>
    private static T _instance;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static T Instance => _instance;

    #endregion

    #region Methods

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    protected virtual void Awake()
    {
      if (_instance == null)
      {
        _instance = this as T;

        DontDestroyOnLoad(gameObject);
      }
      else
      {
        Destroy(gameObject); // Destroy duplicate instances
      }
    }

    #endregion
  }
}
