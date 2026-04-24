using UnityEngine;


namespace CupkekGames.Core
{
  public enum FadeableAction
  {
    None,
    FadeIn,
    FadeOut,
    SetFadedIn,
    SetFadedOut
  }

  public abstract class FadeableMono : MonoBehaviour
  {
    [SerializeField] public Fadeable Fadeable;

    [SerializeField] private FadeableAction _onAwake = FadeableAction.None;
    [SerializeField] private FadeableAction _onEnable = FadeableAction.None;
    [SerializeField] private FadeableAction _onDisable = FadeableAction.None;
    [SerializeField] private FadeableAction _onDestroy = FadeableAction.None;

    protected virtual void Awake()
    {
      if (Fadeable == null)
      {
        Fadeable = new Fadeable(this);
      }
      else
      {
        Fadeable.Initialize(this);
      }
      ExecuteAction(_onAwake);
    }

    protected virtual void OnEnable()
    {
      ExecuteAction(_onEnable);
    }

    protected virtual void OnDisable()
    {
      ExecuteAction(_onDisable);
    }

    protected virtual void OnDestroy()
    {
      ExecuteAction(_onDestroy);
      Fadeable.Kill();
    }

    private void ExecuteAction(FadeableAction action)
    {
      switch (action)
      {
        case FadeableAction.FadeIn:
          Fadeable.FadeIn();
          break;
        case FadeableAction.FadeOut:
          Fadeable.FadeOut();
          break;
        case FadeableAction.SetFadedIn:
          Fadeable.SetFadedIn();
          break;
        case FadeableAction.SetFadedOut:
          Fadeable.SetFadedOut();
          break;
        case FadeableAction.None:
        default:
          break;
      }
    }
  }
}