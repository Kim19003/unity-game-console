using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEditorInternal;
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

public class Convert
{
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="OverflowException"></exception>
    public static int ToInt(string value)
    {
        value = value.Trim();
        return int.Parse(value);
    }

    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="OverflowException"></exception>
    public static float ToFloat(string value)
    {
        value = value.Trim();
        value = value.Replace(".", ",");
        value = value.Replace("f", string.Empty);
        return float.Parse(value);
    }

    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    public static bool ToBool(string value)
    {
        value = value.Trim();
        return bool.Parse(value);
    }

    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    public static char ToChar(string value)
    {
        value = value.Trim();
        return char.Parse(value);
    }

    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="OverflowException"></exception>
    public static Vector2 ToVector2(string value)
    {
        value = value.Replace("(", string.Empty);
        value = value.Replace(")", string.Empty);
        string[] values = value.Split(',');
        return new Vector2(ToFloat(values[0]), ToFloat(values[1]));
    }

    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="OverflowException"></exception>
    public static Vector3 ToVector3(string value)
    {
        value = value.Replace("(", string.Empty);
        value = value.Replace(")", string.Empty);
        string[] values = value.Split(',');
        return new Vector3(ToFloat(values[0]), ToFloat(values[1]), ToFloat(values[2]));
    }
}

public static class StringExtensions
{
    public static bool ContainsAll(this string value, params string[] items)
    {
        if (items.Length > 0)
        {
            int equalItems = 0;

            foreach (string item in items)
            {
                if (value.Contains(item))
                {
                    equalItems++;
                }
            }

            if (equalItems == items.Length)
            {
                return true;
            }
        }

        return false;
    }

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

    public static string WrapWithTags(this string text, string tag, char tagStartChar = '<', char tagEndChar = '>', char tagClosingMarkerChar = '/')
    {
        string tagEndingPart = tag.Contains("=") ? tag.Substring(0, tag.IndexOf("=")) : tag;
        return $"{tagStartChar}{tag}{tagEndChar}{text}{tagStartChar}{tagClosingMarkerChar}{tagEndingPart}{tagEndChar}";
    }

    public static string WrapAlreadyWrappedPartsWithTags(this string text, string tag, char wrappedPartsStartChar = '<', char wrappedPartsEndChar = '>',
        char tagStartChar = '<', char tagEndChar = '>', char tagClosingMarkerChar = '/')
    {
        if (text.ContainsAll(wrappedPartsStartChar.ToString(), wrappedPartsEndChar.ToString()))
        {
            string newText = string.Empty;

            foreach (char c in text)
            {
                string addition = c.ToString();

                if (c == wrappedPartsStartChar)
                {
                    addition = $"{tagStartChar}{tag}{tagEndChar}{addition}";
                }
                else if (c == wrappedPartsEndChar)
                {
                    string tagEndingPart = tag.Contains("=") ? tag.Substring(0, tag.IndexOf("=")) : tag;

                    addition = $"{addition}{tagStartChar}{tagClosingMarkerChar}{tagEndingPart}{tagEndChar}";
                }

                newText += addition;
            }

            return newText;
        }

        return text;
    }

    public static string RemoveTags(this string text, string tag, char tagStartChar = '<', char tagEndChar = '>', char tagClosingMarkerChar = '/')
    {
        string startTag = tagStartChar.ToString() + tag;
        string tagClosingMarker = tagStartChar.ToString() + tagClosingMarkerChar.ToString() + tag + tagEndChar.ToString();

        while (text.Contains(startTag))
        {
            int startIndex = text.IndexOf(startTag);
            int endIndex = text.IndexOf(tagEndChar, startIndex);

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

    public static string GetFirstWord(this string text)
    {
        string textTrimmed = text.Trim();

        if (textTrimmed.Contains(" "))
        {
            return textTrimmed.Substring(0, textTrimmed.IndexOf(" "));
        }

        return text;
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
    public static void ClearAndFill<T>(this HashSet<T> hashSet, T[] items)
    {
        hashSet.Clear();
        
        foreach (T item in items)
        {
            hashSet.Add(item);
        }
    }

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

public static class GameObjectExtensions
{
    public static readonly string[] AttributeNames = new string[]
    {
        "activate",
        "name",
        "tag",
        "layer",
        "position",
        "rotation",
        "scale",
        "activeInHierarchy",
        "activeSelf",
        "isStatic",
        "hideFlags"
    };

    public static void SetAttributeValue(this GameObject gameObject, string attributeName, string attributeValue)
    {
        switch (attributeName)
        {
            case "activate":
                gameObject.SetActive(Convert.ToBool(attributeValue));
                break;
            case "name":
                gameObject.name = attributeValue;
                break;
            case "tag":
                gameObject.tag = attributeValue;
                break;
            case "layer":
                gameObject.layer = Convert.ToInt(attributeValue);
                break;
            case "position":
                gameObject.transform.position = Convert.ToVector3(attributeValue);
                break;
            case "rotation":
                gameObject.transform.rotation = Quaternion.Euler(Convert.ToVector3(attributeValue));
                break;
            case "scale":
                gameObject.transform.localScale = Convert.ToVector3(attributeValue);
                break;
            case "activeInHierarchy":
                gameObject.SetActive(Convert.ToBool(attributeValue));
                break;
            case "activeSelf":
                gameObject.SetActive(Convert.ToBool(attributeValue));
                break;
            case "isStatic":
                gameObject.isStatic = Convert.ToBool(attributeValue);
                break;
            case "hideFlags":
                gameObject.hideFlags = (HideFlags)Convert.ToInt(attributeValue);
                break;
            default:
                throw new ArgumentException($"Attribute '{attributeName}' not found");
        }
    }

    public static string GetAttributeValue(this GameObject gameObject, string attributeName)
    {
        switch (attributeName)
        {
            case "activate":
                return gameObject.activeSelf.ToString();
            case "name":
                return gameObject.name;
            case "tag":
                return gameObject.tag;
            case "layer":
                return gameObject.layer.ToString();
            case "position":
                return gameObject.transform.position.ToString();
            case "rotation":
                return gameObject.transform.rotation.eulerAngles.ToString();
            case "scale":
                return gameObject.transform.localScale.ToString();
            case "activeInHierarchy":
                return gameObject.activeInHierarchy.ToString();
            case "activeSelf":
                return gameObject.activeSelf.ToString();
            case "isStatic":
                return gameObject.isStatic.ToString();
            case "hideFlags":
                return ((int)gameObject.hideFlags).ToString();
            default:
                throw new ArgumentException($"Attribute '{attributeName}' not found");
        }
    }
}