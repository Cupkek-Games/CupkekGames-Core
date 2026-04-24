using UnityEngine;

namespace CupkekGames.Core
{
  public class FadeableColor : FadeableMono
  {
    public Color _colorStart;
    public Color _colorEnd;

    public Color GetColor()
    {
      return Color.Lerp(_colorStart, _colorEnd, Fadeable.Value);
    }
  }
}