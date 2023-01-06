using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;

public enum ConsoleTextColor
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

public class Helpers
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

    public static Color GetConsoleTextColor(ConsoleTextColor consoleTextColor)
    {
        switch (consoleTextColor)
        {
            case ConsoleTextColor.Aqua:
                return Color.cyan;
            case ConsoleTextColor.Black:
                return Color.black;
            case ConsoleTextColor.Blue:
                return Color.blue;
            case ConsoleTextColor.Brown:
                return new Color32(165, 42, 42, 255);
            case ConsoleTextColor.Cyan:
                return Color.cyan;
            case ConsoleTextColor.Darkblue:
                return new Color32(0, 0, 160, 255);
            case ConsoleTextColor.Fuchsia:
                return Color.magenta;
            case ConsoleTextColor.Green:
                return Color.green;
            case ConsoleTextColor.Grey:
                return Color.grey;
            case ConsoleTextColor.Lightblue:
                return new Color32(173, 216, 230, 255);
            case ConsoleTextColor.Lime:
                return new Color32(0, 255, 0, 255);
            case ConsoleTextColor.Magenta:
                return Color.magenta;
            case ConsoleTextColor.Maroon:
                return new Color32(128, 0, 0, 255);
            case ConsoleTextColor.Navy:
                return new Color32(0, 0, 128, 255);
            case ConsoleTextColor.Olive:
                return new Color32(128, 128, 0, 255);
            case ConsoleTextColor.Orange:
                return new Color32(255, 165, 0, 255);
            case ConsoleTextColor.Purple:
                return new Color32(128, 0, 128, 255);
            case ConsoleTextColor.Red:
                return Color.red;
            case ConsoleTextColor.Silver:
                return new Color32(192, 192, 192, 255);
            case ConsoleTextColor.Teal:
                return new Color32(0, 128, 128, 255);
            case ConsoleTextColor.White:
                return Color.white;
            case ConsoleTextColor.Yellow:
                return Color.yellow;
            default:
                return Color.black;
        }
    }

    public static Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = color;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}

public static class StringExtensions
{
    public static string RemovePartFromStart(this string text, string part)
    {
        if (text.StartsWith(part))
        {
            return text.Remove(0, part.Length);
        }

        return text;
    }

    public static string Get(this string text)
    {
        return text;
    }

    public static string RemoveTags(this string text, string tag, char startTagChar = '<', char endTagChar = '>', char tagClosingMarkerChar = '/')
    {
        string startTag = startTagChar.ToString() + tag;
        string tagClosingMarker = startTagChar.ToString() + tagClosingMarkerChar.ToString() + tag + endTagChar.ToString();

        while (text.Contains(startTag))
        {
            int startIndex = text.IndexOf(startTag);
            int endIndex = text.IndexOf(endTagChar, startIndex);

            if (endIndex == -1)
            {
                break;
            }

            text = text.Remove(startIndex, endIndex - startIndex + 1);
        }

        while (text.Contains(tagClosingMarker))
        {
            int startIndex = text.IndexOf(tagClosingMarker);
            text = text.Remove(startIndex, tagClosingMarker.Length);
        }

        return text;
    }

    public static string[] SplitAllNonWrapped(this string text, char splitChar, char wrapChar)
    {
        List<string> list = new List<string>();
        string[] split = text.Split(wrapChar);
        for (int i = 0; i < split.Length; i++)
        {
            if (i % 2 == 0)
            {
                list.AddRange(split[i].Split(splitChar));
            }
            else
            {
                list.Add(split[i]);
            }
        }

        return list.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
    }
}

public static class TextEditorExtensions
{
    public static int GetCaretIndex(this TextEditor textEditor)
    {
        return textEditor.cursorIndex;
    }

    public static void SetCaretIndex(this TextEditor textEditor, int index)
    {
        textEditor.cursorIndex = index;
        textEditor.SelectNone();
    }
}

public static class HashSetExtensions
{
    public static T GetClosestAt<T>(this HashSet<T> hashSet, ref int index)
    {
        if (hashSet.Count == 0)
        {
            return default;
        }

        if (index < 0)
        {
            index = 0;
        }
        else if (index >= hashSet.Count)
        {
            index = hashSet.Count - 1;
        }

        int i = 0;
        foreach (T item in hashSet)
        {
            if (i == index)
            {
                return item;
            }

            i++;
        }

        return default;
    }
}

public static class ListExtensions
{
    public static T GetClosestAt<T>(this List<T> list, ref int index)
    {
        if (list.Count == 0)
        {
            return default;
        }

        if (index < 0)
        {
            index = 0;
        }
        else if (index >= list.Count)
        {
            index = list.Count - 1;
        }

        int i = 0;
        foreach (T item in list)
        {
            if (i == index)
            {
                return item;
            }

            i++;
        }

        return default;
    }
}