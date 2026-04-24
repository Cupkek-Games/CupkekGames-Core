#if UNITY_EDITOR
using UnityEngine;

namespace CupkekGames.Core.Editor
{
  /// <summary>
  /// Shared editor colors for IMGUI / UI Toolkit inline styling in C#.
  /// Keep aligned with EditorColorPalette.uss.
  /// </summary>
  public static class EditorColorPalette
  {
    public static readonly Color SurfaceWeak = new(1f, 1f, 1f, 0.035f);
    public static readonly Color SurfaceMedium = new(1f, 1f, 1f, 0.06f);
    public static readonly Color BorderSoft = new(1f, 1f, 1f, 0.085f);
    public static readonly Color BorderMedium = new(1f, 1f, 1f, 0.12f);

    public static readonly Color TextPrimary = new(0.925f, 0.945f, 0.98f, 0.96f);
    public static readonly Color TextSecondary = new(0.875f, 0.90f, 0.94f, 0.90f);
    public static readonly Color TextMuted = new(0.77f, 0.80f, 0.86f, 0.76f);

    public static readonly Color Success = new(0.286f, 0.80f, 0.357f, 1f);
    public static readonly Color Warning = new(0.85f, 0.75f, 0.3f, 1f);
    public static readonly Color Danger = new(0.86f, 0.39f, 0.39f, 1f);
  }
}
#endif
