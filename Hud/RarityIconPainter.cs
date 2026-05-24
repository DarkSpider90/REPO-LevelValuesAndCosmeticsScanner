using System.Collections.Generic;
using UnityEngine;

namespace LevelValuesAndCosmeticsScanner.Hud;

public static class RarityIconPainter
{
    private const int TextureSize = 64;
    private static readonly Dictionary<SemiFunc.Rarity, Texture2D> Textures = [];

    public static Texture2D GetIconTexture(SemiFunc.Rarity rarity)
    {
        if (Textures.TryGetValue(rarity, out var cached))
            return cached;

        var texture = DrawRarityIcon(GetPalette(rarity));
        texture.name = $"LevelValuesAndCosmeticsScanner_{rarity}_Icon";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Textures[rarity] = texture;
        return texture;
    }

    private static Texture2D DrawRarityIcon(IconPalette palette)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        var pixels = new Color[TextureSize * TextureSize];

        for (var y = 0; y < TextureSize; y++)
        {
            for (var x = 0; x < TextureSize; x++)
            {
                var px = x + 0.5f;
                var py = y + 0.5f;

                var outer = StrokeRoundedRect(px, py, 32f, 32f, 21f, 16f, 6.5f, 3.2f);
                var outerGlow = GlowRoundedRect(px, py, 32f, 32f, 21f, 16f, 6.5f, 12f);
                var inner = StrokeRoundedRect(px, py, 32f, 32f, 10f, 8.5f, 1.5f, 3f);
                var innerGlow = GlowRoundedRect(px, py, 32f, 32f, 10f, 8.5f, 1.5f, 6f);

                var color = Color.clear;
                color = Blend(color, palette.Glow, outerGlow * 0.28f);
                color = Blend(color, palette.Glow, innerGlow * 0.16f);
                color = Blend(color, palette.Main, outer * 0.96f);
                color = Blend(color, palette.Inner, inner * 0.88f);

                pixels[y * TextureSize + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static IconPalette GetPalette(SemiFunc.Rarity rarity)
    {
        return rarity switch
        {
            SemiFunc.Rarity.Common => new IconPalette(
                new Color(0.34f, 1f, 0.22f, 1f),
                new Color(0.62f, 1f, 0.46f, 1f),
                new Color(0.12f, 1f, 0.08f, 1f)
            ),
            SemiFunc.Rarity.Uncommon => new IconPalette(
                new Color(0.07f, 0.60f, 1f, 1f),
                new Color(0.26f, 0.82f, 1f, 1f),
                new Color(0.00f, 0.45f, 1f, 1f)
            ),
            SemiFunc.Rarity.Rare => new IconPalette(
                new Color(0.72f, 0.16f, 1f, 1f),
                new Color(0.94f, 0.36f, 1f, 1f),
                new Color(0.60f, 0.06f, 1f, 1f)
            ),
            SemiFunc.Rarity.UltraRare => new IconPalette(
                new Color(1f, 0.60f, 0.08f, 1f),
                new Color(1f, 0.82f, 0.22f, 1f),
                new Color(1f, 0.42f, 0.00f, 1f)
            ),
            _ => new IconPalette(Color.white, Color.white, Color.white)
        };
    }

    private static float StrokeRoundedRect(float x, float y, float centerX, float centerY, float halfWidth, float halfHeight, float radius, float thickness)
    {
        var distance = Mathf.Abs(RoundedRectDistance(x, y, centerX, centerY, halfWidth, halfHeight, radius));
        return Smooth01((thickness * 0.5f) + 1.1f, (thickness * 0.5f) - 0.6f, distance);
    }

    private static float GlowRoundedRect(float x, float y, float centerX, float centerY, float halfWidth, float halfHeight, float radius, float glowWidth)
    {
        var distance = Mathf.Abs(RoundedRectDistance(x, y, centerX, centerY, halfWidth, halfHeight, radius));
        var glow = 1f - Mathf.Clamp01(distance / glowWidth);
        return glow * glow;
    }

    private static float RoundedRectDistance(float x, float y, float centerX, float centerY, float halfWidth, float halfHeight, float radius)
    {
        var qx = Mathf.Abs(x - centerX) - halfWidth + radius;
        var qy = Mathf.Abs(y - centerY) - halfHeight + radius;
        var outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
        var inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
        return outside + inside - radius;
    }

    private static float Smooth01(float edge0, float edge1, float value)
    {
        var t = Mathf.Clamp01((value - edge0) / (edge1 - edge0));
        return t * t * (3f - 2f * t);
    }

    private static Color Blend(Color under, Color over, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        var outAlpha = alpha + under.a * (1f - alpha);
        if (outAlpha <= 0f)
            return Color.clear;

        return new Color(
            (over.r * alpha + under.r * under.a * (1f - alpha)) / outAlpha,
            (over.g * alpha + under.g * under.a * (1f - alpha)) / outAlpha,
            (over.b * alpha + under.b * under.a * (1f - alpha)) / outAlpha,
            outAlpha
        );
    }

    private readonly struct IconPalette
    {
        public readonly Color Main;
        public readonly Color Inner;
        public readonly Color Glow;

        public IconPalette(Color main, Color inner, Color glow)
        {
            Main = main;
            Inner = inner;
            Glow = glow;
        }
    }
}
