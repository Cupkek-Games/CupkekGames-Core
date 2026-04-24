using UnityEngine;

namespace CupkekGames.Core
{
  public class FadeableScale : FadeableMono
  {
    private Transform _transform;
    private Vector3 _initialScale;
    [SerializeField] private bool _ignoreY;

    protected override void Awake()
    {
      base.Awake();

      _transform = GetComponent<Transform>();
      _initialScale = _transform.localScale;

      Fadeable.OnApply += Apply;
    }

    public void Apply()
    {
      float value = Fadeable.Value;

      if (_ignoreY)
      {
        Vector3 mul = new Vector3(value, 1, value);
        _transform.localScale = Vector3.Scale(_initialScale, mul);
      }
      else
      {
        _transform.localScale = _initialScale * value;
      }
    }
  }
}