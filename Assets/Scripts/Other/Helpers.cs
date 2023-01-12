using System;
using UnityEngine;

namespace Assets.Scripts.Other
{
    public enum UnityTimeMode
    {
        Time,
        UnscaledTime,
        TimeSinceLevelLoad,
        FixedTime,
        FixedUnscaledTime,
        DeltaTime,
        UnscaledDeltaTime,
        FixedDeltaTime,
        FixedUnscaledDeltaTime,
        RealtimeSinceStartup
    }

    public enum GetterResult
    {
        Successful,
        TargetNotFound
    }

    public enum SetterResult
    {
        Successful,
        ValueNotAllowed,
        TargetNotFound
    }

    public enum RichTextColor
    {
        Aqua,
        Black,
        Blue,
        Brown,
        Cyan,
        Darkblue,
        Fuchsia,
        Green,
        Grey,
        Lightblue,
        Lime,
        Magenta,
        Maroon,
        Navy,
        Olive,
        Orange,
        Purple,
        Red,
        Silver,
        Teal,
        White,
        Yellow,
    }

    public enum CustomColor
    {
        Black,
        LightGray
    }

    public static class Helpers
    {
        public static Color32 GetCustomColor(CustomColor customColor, byte alpha = 255)
        {
            switch (customColor)
            {
                case CustomColor.LightGray:
                    return new Color32(200, 200, 200, alpha);
                default:
                    return new Color32(0, 0, 0, alpha);
            }
        }

        public static Color GetRichTextColor(RichTextColor richTextColor)
        {
            switch (richTextColor)
            {
                case RichTextColor.Aqua:
                    return Color.cyan;
                case RichTextColor.Black:
                    return Color.black;
                case RichTextColor.Blue:
                    return Color.blue;
                case RichTextColor.Brown:
                    return new Color32(165, 42, 42, 255);
                case RichTextColor.Cyan:
                    return Color.cyan;
                case RichTextColor.Darkblue:
                    return new Color32(0, 0, 160, 255);
                case RichTextColor.Fuchsia:
                    return Color.magenta;
                case RichTextColor.Green:
                    return Color.green;
                case RichTextColor.Grey:
                    return Color.grey;
                case RichTextColor.Lightblue:
                    return new Color32(173, 216, 230, 255);
                case RichTextColor.Lime:
                    return new Color32(0, 255, 0, 255);
                case RichTextColor.Magenta:
                    return Color.magenta;
                case RichTextColor.Maroon:
                    return new Color32(128, 0, 0, 255);
                case RichTextColor.Navy:
                    return new Color32(0, 0, 128, 255);
                case RichTextColor.Olive:
                    return new Color32(128, 128, 0, 255);
                case RichTextColor.Orange:
                    return new Color32(255, 165, 0, 255);
                case RichTextColor.Purple:
                    return new Color32(128, 0, 128, 255);
                case RichTextColor.Red:
                    return Color.red;
                case RichTextColor.Silver:
                    return new Color32(192, 192, 192, 255);
                case RichTextColor.Teal:
                    return new Color32(0, 128, 128, 255);
                case RichTextColor.White:
                    return Color.white;
                case RichTextColor.Yellow:
                    return Color.yellow;
                default:
                    return Color.black;
            }
        }

        public static Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();

            return result;
        }

        public static float GetTimeMode(UnityTimeMode timeMode)
        {
            switch (timeMode)
            {
                case UnityTimeMode.Time:
                    return Time.time;
                case UnityTimeMode.UnscaledTime:
                    return Time.unscaledTime;
                case UnityTimeMode.TimeSinceLevelLoad:
                    return Time.timeSinceLevelLoad;
                case UnityTimeMode.FixedTime:
                    return Time.fixedTime;
                case UnityTimeMode.FixedUnscaledTime:
                    return Time.fixedUnscaledTime;
                case UnityTimeMode.DeltaTime:
                    return Time.deltaTime;
                case UnityTimeMode.UnscaledDeltaTime:
                    return Time.unscaledDeltaTime;
                case UnityTimeMode.FixedDeltaTime:
                    return Time.fixedDeltaTime;
                case UnityTimeMode.FixedUnscaledDeltaTime:
                    return Time.fixedUnscaledDeltaTime;
                case UnityTimeMode.RealtimeSinceStartup:
                    return Time.realtimeSinceStartup;
                default:
                    throw new ArgumentException("Time mode not found.");
            }
        }
    }
}
