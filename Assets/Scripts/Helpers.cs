using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using TextEditor = UnityEngine.TextEditor;

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

public class TimedUnityAction
{
    public Action Action
    {
        get
        {
            return action;
        }
        set
        {
            if (value != null)
            {
                action = value;
            }
        }
    }
    Action action = null;

    public float Interval
    {
        get
        {
            return interval;
        }
        private set
        {
            if (value > 0)
            {
                interval = value;
            }
        }
    }
    private float interval = 0f;

    float nextActionTime = 0f;

    bool started = false;

    public TimedUnityAction()
    {

    }

    public TimedUnityAction(Action action, float interval)
    {
        if (action == null)
        {
            throw new ArgumentException("Action cannot be null.");
        }
        else if (interval <= 0)
        {
            throw new ArgumentException("Interval must be greater than 0.");
        }

        Action = action;
        Interval = interval;
    }

    /// <summary>
    /// Runs {Action} every {Interval} second. Call this in Unity's Update method.
    /// </summary>
    public void Run(float startDelay = 0f)
    {
        if (Action == null)
        {
            throw new ArgumentException("Action cannot be null.");
        }
        else if (Interval <= 0)
        {
            throw new ArgumentException("Interval must be greater than 0.");
        }
        else if (startDelay < 0)
        {
            throw new ArgumentException("Start delay must be greater than or equal to 0.");
        }

        if (Time.timeSinceLevelLoad > (startDelay > 0 && !started ? startDelay : nextActionTime))
        {
            Action();

            nextActionTime += Interval;

            started = true;
        }
    }

    /// <summary>
    /// Runs {action} every {interval} second. Call this in Unity's Update method.
    /// </summary>
    public void Run(Action action, float interval, float startDelay = 0f)
    {
        if (action == null)
        {
            throw new ArgumentException("Action cannot be null.");
        }
        else if (interval <= 0)
        {
            throw new ArgumentException("Interval must be greater than 0.");
        }
        else if (startDelay < 0)
        {
            throw new ArgumentException("Start delay must be greater than or equal to 0.");
        }

        if (Action == null)
        {
            Action = action;
        }
        else if (Action != action)
        {
            throw new ArgumentException("The action has already been initialized, and cannot be changed here. Use the SetAction method to change the action.");
        }

        if (Interval != interval)
        {
            Interval = interval;
        }

        if (Time.timeSinceLevelLoad > (startDelay > 0 && !started ? startDelay : nextActionTime))
        {
            Action();

            nextActionTime += Interval;

            started = true;
        }
    }

    public void SetAction(Action action)
    {
        Action = action;
    }

    public void SetInterval(float interval)
    {
        Interval = interval;
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

    public static int ContainsAmount(this string value, char item)
    {
        int times = 0;

        foreach (char c in value)
        {
            if (c == item)
            {
                times++;
            }
        }

        return times;
    }

    public static string RemovePartFromStart(this string text, string part)
    {
        if (text.StartsWith(part))
        {
            return text.Remove(0, part.Length);
        }

        return text;
    }

    public static string RemovePartFromEnd(this string text, string part)
    {
        if (text.EndsWith(part))
        {
            return text.Remove(text.Length - part.Length);
        }

        return text;
    }

    public static char GetCharAt(this string text, int index)
    {
        if (index > -1 && index < text.Length)
        {
            return text[index];
        }

        return '\0';
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

    public static string AddWrappersTo(this string text, int index, char wrapChar)
    {
        return text.Insert(index, $"{wrapChar}{wrapChar}");
    }

    public static string WrapWith(this string text, char wrapChar)
    {
        return $"{wrapChar}{text}{wrapChar}";
    }

    public static string WrapFirstFoundPart(this string text, string part, char wrapChar)
    {
        string newText = text;

        if (newText.Contains(part))
        {
            int partIndex = newText.IndexOf(part);
            newText = newText.Remove(partIndex, part.Length);
            string partWrapped = part.WrapWith(wrapChar);
            newText = newText.Insert(partIndex, partWrapped);
        }

        return newText;
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

    public static T GetClosestAt<T>(this HashSet<T> hashSet, ref int index, bool returnDefaultIfOutOfBounds = false)
    {
        if (hashSet.Count == 0)
        {
            return default;
        }

        if (index < 0)
        {
            if (returnDefaultIfOutOfBounds)
            {
                index = -1;

                return default;
            }

            index = 0;
        }
        else if (index >= hashSet.Count)
        {
            if (returnDefaultIfOutOfBounds)
            {
                index = hashSet.Count;

                return default;
            }

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
    public static T GetClosestAt<T>(this List<T> list, ref int index, bool returnDefaultIfOutOfBounds = false)
    {
        if (list.Count == 0)
        {
            return default;
        }

        if (index < 0)
        {
            if (returnDefaultIfOutOfBounds)
            {
                index = -1;

                return default;
            }

            index = 0;
        }
        else if (index >= list.Count)
        {
            if (returnDefaultIfOutOfBounds)
            {
                index = list.Count;

                return default;
            }

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
                return gameObject.transform.position.ToString("F3");
            case "rotation":
                return gameObject.transform.rotation.eulerAngles.ToString("F3");
            case "scale":
                return gameObject.transform.localScale.ToString("F3");
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