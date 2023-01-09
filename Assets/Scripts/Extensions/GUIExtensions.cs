using System;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    [Flags]
    public enum BorderDirection
    {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8,
        All = Top | Right | Bottom | Left
    }

    public static class GUIExtensions
    {
        /// <summary>
        /// Create a bordered box.
        /// </summary>
        /// <returns>Content part's Rect.</returns>
        public static Rect BorderedBox(Rect position, string text, GUIStyle style, float borderSize, GUIStyle borderStyle = null, BorderDirection borderDirection = BorderDirection.All)
        {
            if (borderStyle == null)
            {
                borderStyle = new GUIStyle() { normal = new GUIStyleState() { background = Texture2D.whiteTexture } };
            }

            Rect mainBoxRect = new Rect(
                borderDirection.HasFlag(BorderDirection.Left) ? position.x + borderSize : position.x,
                borderDirection.HasFlag(BorderDirection.Top) ? position.y + borderSize : position.y,
                borderDirection.HasFlag(BorderDirection.Left | BorderDirection.Right) ? position.width - borderSize * 2
                : borderDirection.HasFlag(BorderDirection.Left) || borderDirection.HasFlag(BorderDirection.Right) ? position.width - borderSize : position.width,
                borderDirection.HasFlag(BorderDirection.Top | BorderDirection.Bottom) ? position.height - borderSize * 2
                : borderDirection.HasFlag(BorderDirection.Top) || borderDirection.HasFlag(BorderDirection.Bottom) ? position.height - borderSize : position.height
                );

            // Main box
            GUI.Box(mainBoxRect, text, style);

            if (borderDirection.HasFlag(BorderDirection.Top))
            {
                // Top border
                GUI.Box(new Rect(position.x + borderSize, position.y, position.width - borderSize * 2, borderSize), string.Empty, borderStyle);
            }

            if (borderDirection.HasFlag(BorderDirection.Left))
            {
                // Left border
                GUI.Box(new Rect(position.x, position.y, borderSize, position.height), string.Empty, borderStyle);
            }

            if (borderDirection.HasFlag(BorderDirection.Bottom))
            {
                // Bottom border
                GUI.Box(new Rect(position.x + borderSize, position.y + position.height - borderSize, position.width - borderSize * 2, borderSize), string.Empty, borderStyle);
            }

            if (borderDirection.HasFlag(BorderDirection.Right))
            {
                // Right border
                GUI.Box(new Rect(position.width - borderSize, position.y, borderSize, position.height), string.Empty, borderStyle);
            }

            return mainBoxRect;
        }
    }
}
