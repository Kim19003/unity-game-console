using Assets.Scripts.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
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

        public static int ContainsAmountOf(this string value, char @char)
        {
            int times = 0;

            foreach (char c in value)
            {
                if (c == @char)
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

        public static string AsBold(this string text)
        {
            return $"<b>{text}</b>";
        }

        public static string AsItalic(this string text)
        {
            return $"<i>{text}</i>";
        }

        public static string AsColor(this string text, RichTextColor richTextColor)
        {
            return $"<color={richTextColor.ToString().ToLower()}>{text}</color>";
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

        public static string AddWrappersTo(this string text, int index, string firstWrapper, string secondWrapper)
        {
            return text.Insert(index, $"{firstWrapper}{secondWrapper}");
        }

        public static string WrapWith(this string text, string firstWrapper, string secondWrapper)
        {
            return $"{firstWrapper}{text}{secondWrapper}";
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

        public static string[] SplitAllNonWrapped(this string text, char splitChar, char[] wrapChars)
        {
            List<string> list = new List<string>();
            string[] split = text.Split(wrapChars);
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

        public static char TryGetWrapperAt(this string text, int index, char wrapChar)
        {
            char foundChar = text.GetCharAt(index);

            if (foundChar == wrapChar)
            {
                return foundChar;
            }

            return '\0';
        }

        public static char TryGetWrapperAt(this string text, int index, char[] wrapChars)
        {
            char foundChar = text.GetCharAt(index);

            if (wrapChars.Contains(foundChar))
            {
                return foundChar;
            }

            return '\0';
        }

        public static string[] GetArguments(this string commandAsWhole, string commandId, char[] wrapChars)
        {
            string[] commandSplitted = commandAsWhole.SplitAllNonWrapped(' ', wrapChars);

            if (commandSplitted.Length > 0 && commandSplitted[0] == commandId)
            {
                return commandSplitted.Skip(1).ToArray();
            }

            return Array.Empty<string>();
        }
    }
}
