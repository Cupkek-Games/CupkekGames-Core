using UnityEngine;

namespace CupkekGames.Core
{
    public enum EasingType
    {
        Linear,
        SineIn,
        SineOut,
        SineInOut,
        QuadIn,
        QuadOut,
        QuadInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut
    }
    public static class EasingTypeExtensions
    {
        public static float Apply(this EasingType type, float t)
        {
            switch (type)
            {
                case EasingType.QuadIn:
                    return t * t; // Quadratic ease-in
                case EasingType.QuadOut:
                    return t * (2 - t); // Quadratic ease-out
                case EasingType.QuadInOut:
                    return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t; // Quadratic ease-in-out
                case EasingType.ExpoIn:
                    return t == 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f)); // Exponential ease-in
                case EasingType.ExpoOut:
                    return t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t); // Exponential ease-out
                case EasingType.ExpoInOut:
                    if (t == 0f) return 0f;
                    if (t == 1f) return 1f;
                    return t < 0.5f
                        ? 0.5f * Mathf.Pow(2f, 20f * t - 10f) // First half: exponential ease-in
                        : 1f - 0.5f * Mathf.Pow(2f, -20f * t + 10f); // Second half: exponential ease-out
                case EasingType.SineIn:
                    return 1f - Mathf.Cos(t * Mathf.PI * 0.5f); // Sine ease-in
                case EasingType.SineOut:
                    return Mathf.Sin(t * Mathf.PI * 0.5f); // Sine ease-out
                case EasingType.SineInOut:
                    return 0.5f * (1f - Mathf.Cos(Mathf.PI * t)); // Sine ease-in-out
                default:
                    return t; // Linear
            }
        }
    }
}