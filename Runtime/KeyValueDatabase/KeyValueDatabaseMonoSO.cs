using UnityEngine;

namespace CupkekGames.Core
{
  /// <summary>
  /// Base class for MonoBehaviour databases that store ScriptableObject values.
  /// The KeyValueDatabase field is handled by the PropertyDrawer automatically.
  /// </summary>
  public abstract class KeyValueDatabaseMonoSO<TKey, TValue> : KeyValueDatabaseMono<TKey, TValue> where TValue : ScriptableObject
  {
  }
}