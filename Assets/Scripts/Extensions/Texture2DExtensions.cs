using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class Texture2DExtensions
    {
        public static void FillWithColor(this Texture2D texture2D, Color color)
        {
            Color[] colors = new Color[texture2D.width * texture2D.height];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            texture2D.SetPixels(colors);
            texture2D.Apply();
        }

        public static void CreateBorders(this Texture2D texture2D, int borderWidth, Color borderColor)
        {
            Color[] pixels = texture2D.GetPixels();

            for (int x = 0; x < texture2D.width; x++)
            {
                for (int y = 0; y < texture2D.height; y++)
                {
                    if (x < borderWidth || x >= texture2D.width - borderWidth || y < borderWidth || y >= texture2D.height - borderWidth)
                    {
                        pixels[y * texture2D.width + x] = borderColor;
                    }
                }
            }

            texture2D.SetPixels(pixels);
            texture2D.Apply();
        }
    }
}
