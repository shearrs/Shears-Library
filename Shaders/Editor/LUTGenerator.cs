using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shears.Shaders.Editor
{
    public static class LUTGenerator
    {
        const int SIZE = 32;

        [MenuItem("Assets/Create/Shears Library/Create 3D Texture From Palette")]
        private static void GenerateLUT()
        {
            if (!TryLoadPalette(out var palette, out var path))
                return;

            var lutTexture = new Texture3D(SIZE, SIZE, SIZE, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };

            var colors = new Color32[SIZE * SIZE * SIZE];
            float recipSize = 1.0f / (SIZE - 1.0f);
            var palettePixels = palette.GetPixels32();

            for (int z = 0; z < SIZE; z++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    for (int x = 0; x < SIZE; x++)
                    {
                        var uv = new Color32(
                            (byte)Mathf.RoundToInt(x * recipSize * 255),
                            (byte)Mathf.RoundToInt(y * recipSize * 255),
                            (byte)Mathf.RoundToInt(z * recipSize * 255),
                            255
                        );
                        var closestColor = GetClosestColor(uv, palettePixels);

                        colors[x + (SIZE * y) + (SIZE * SIZE * z)] = closestColor;
                    }
                }
            }

            lutTexture.SetPixels32(colors);
            lutTexture.Apply();

            AssetDatabase.CreateAsset(lutTexture, $"{path}/{palette.name} LUT.asset");
            AssetDatabase.SaveAssets();
        }

        private static bool TryLoadPalette(out Texture2D texture, out string path)
        {
            texture = null;
            path = string.Empty;

            var selection = Selection.activeObject;

            if (selection is not Texture2D)
                return false;

            path = AssetDatabase.GetAssetPath(selection);
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (texture == null)
                return false;

            path = Path.GetDirectoryName(path);

            return true;
        }

        private static Color32 GetClosestColor(Color32 uv, Color32[] palettePixels)
        {
            var closestColor = Color.clear;
            float minDistance = float.MaxValue;

            for (int i = 0; i < palettePixels.Length; i++)
            {
                Color linearColor = palettePixels[i];
                Color32 color = linearColor.linear;

                float rDiff = uv.r - color.r;
                float gDiff = uv.g - color.g;
                float bDiff = uv.b - color.b;

                var distance =
                    rDiff * rDiff * 0.299f + gDiff * gDiff * 0.587f + bDiff * bDiff * 0.114f;

                if (distance < minDistance)
                {
                    closestColor = color;
                    minDistance = distance;
                }
            }

            return closestColor;
        }
    }
}
