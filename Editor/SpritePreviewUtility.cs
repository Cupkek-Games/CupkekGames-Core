using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CupkekGames.Core.Editor
{
    public static class SpritePreviewUtility
    {
        private static readonly Dictionary<int, Texture2D> _cache = new();

        public static Texture2D GetPreview(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return null;

            // Use cached async preview if available (best quality)
            var preview = AssetPreview.GetAssetPreview(sprite);
            if (preview != null) return preview;

            // Synchronous fallback: blit sprite rect from GPU
            int id = sprite.GetInstanceID();
            if (_cache.TryGetValue(id, out var cached) && cached != null)
                return cached;

            var tex = BlitSpriteRect(sprite);
            if (tex != null)
            {
                tex.hideFlags = HideFlags.HideAndDontSave;
                _cache[id] = tex;
            }
            return tex;
        }

        private static Texture2D BlitSpriteRect(Sprite sprite, int maxSize = 64)
        {
            var rect = sprite.textureRect;
            int w = Mathf.RoundToInt(rect.width);
            int h = Mathf.RoundToInt(rect.height);
            if (w <= 0 || h <= 0) return null;

            if (w > maxSize || h > maxSize)
            {
                float ratio = maxSize / (float)Mathf.Max(w, h);
                w = Mathf.Max(1, Mathf.RoundToInt(w * ratio));
                h = Mathf.Max(1, Mathf.RoundToInt(h * ratio));
            }

            var src = sprite.texture;
            var scale = new Vector2(rect.width / src.width, rect.height / src.height);
            var offset = new Vector2(rect.x / src.width, rect.y / src.height);

            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
            var prevRT = RenderTexture.active;

            Graphics.Blit(src, rt, scale, offset);

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            RenderTexture.active = prevRT;
            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }
    }
}
