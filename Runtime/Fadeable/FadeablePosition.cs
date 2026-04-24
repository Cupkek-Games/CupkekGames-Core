using UnityEngine;

namespace CupkekGames.Core
{
  public class FadeablePosition : FadeableMono
  {
    public Vector3 _offset = Vector3.zero;

    private Transform _transform;
    private Vector3 _startPos;

    protected override void Awake()
    {
      base.Awake();

      _transform = GetComponent<Transform>();
      _startPos = _transform.position;

      Fadeable.OnApply += Apply;
    }

    public void Apply()
    {
      Vector3 mul = _offset * Fadeable.Value;

      _transform.position = _startPos + mul;
    }
  }
}