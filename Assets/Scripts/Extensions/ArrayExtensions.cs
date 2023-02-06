using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Extensions
{
    public static class ArrayExtensions
    {
        private static readonly Random random = new Random();

        public static T GetRandomElement<T>(this T[] array)
        {
            return array[random.Next(0, array.Length)];
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static string[] CombineSplittedWrappedParts(this string[] splitted, (string Opening, string Closing) openingAndClosingWrappers)
        {
            List<string> splittedAsList = new List<string>(splitted);

            List<int> removalIndexes = new List<int>();
            List<string> combinedTexts = new List<string>();

            List<string> combinedTextParts = new List<string>();
            for (int i = 0; i < splittedAsList.Count; i++)
            {
                if (splittedAsList[i].StartsWith(openingAndClosingWrappers.Opening))
                {
                    removalIndexes.Add(i);
                    combinedTextParts.Add(splittedAsList[i]);
                }
                else if (combinedTextParts.Count > 0)
                {
                    removalIndexes.Add(i);
                    combinedTextParts.Add(splittedAsList[i]);

                    if (splittedAsList[i].EndsWith(openingAndClosingWrappers.Closing))
                    {
                        combinedTexts.Add(string.Join(string.Empty, combinedTextParts));
                        combinedTextParts.Clear();
                    }
                }
            }

            if (removalIndexes.Count > 0)
            {
                foreach (int removalIndex in removalIndexes)
                {
                    for (int i = 0; i < splittedAsList.Count; i++)
                    {
                        if (i == removalIndex)
                        {
                            bool consecutiveEndsAtNext = false;

                            if (splittedAsList.Count <= (i + 1) || !removalIndexes.Contains(i + 1))
                            {
                                consecutiveEndsAtNext = true;
                            }

                            splittedAsList[i] = consecutiveEndsAtNext ? "{{{ADD_ME}}}" : "{{{REMOVE_ME}}}";
                        }
                    }
                }

                foreach (string combinedText in combinedTexts)
                {
                    for (int i = 0; i < splittedAsList.Count; i++)
                    {
                        if (splittedAsList[i] == "{{{ADD_ME}}}")
                        {
                            splittedAsList[i] = combinedText;
                            break;
                        }
                    }
                }

                splittedAsList.RemoveAll(s => s == "{{{REMOVE_ME}}}");
            }

            return splittedAsList.ToArray();
        }

        public static string[] CombineSplittedCommandWrappedParts(this string[] splitted, (char Opening, char Closing)[] openingAndClosingWrapChars)
        {
            List<(int Index, int Amount, string BuiltString)> indexesWithAmountsAndBuiltString = new List<(int Index, int Amount, string BuiltString)>();

            List<string> splittedAsList = new List<string>(splitted);

            foreach (var (Opening, Closing) in openingAndClosingWrapChars)
            {
                if (splittedAsList.Contains(Opening.ToString()) && splittedAsList.Contains(Closing.ToString())
                    || splittedAsList.Any(s => s.StartsWith(Opening.ToString()) && !s.EndsWith(Closing.ToString()))
                    && splittedAsList.Any(s => s.EndsWith(Closing.ToString())))
                {
                    bool containedStarter = false;
                    int startIndex = -1, amount = -1, fixedIndex = -1;
                    string builtString = string.Empty;

                    for (int i = 0; i < splittedAsList.Count; i++)
                    {
                        if (!containedStarter && splittedAsList[i] == Opening.ToString())
                        {
                            startIndex = i;
                            amount = 1;
                            builtString += Opening.ToString();
                            indexesWithAmountsAndBuiltString.Add((i, amount, builtString));
                            fixedIndex++;
                            containedStarter = true;
                        }
                        else if (containedStarter && splittedAsList[i] == Closing.ToString())
                        {
                            amount++;
                            builtString += " " + Closing.ToString();
                            indexesWithAmountsAndBuiltString[fixedIndex] = (startIndex, amount, builtString);
                            splittedAsList.RemoveRange(indexesWithAmountsAndBuiltString[fixedIndex].Index, indexesWithAmountsAndBuiltString[fixedIndex].Amount);
                            splittedAsList.Insert(indexesWithAmountsAndBuiltString[fixedIndex].Index, indexesWithAmountsAndBuiltString[fixedIndex].BuiltString);
                            break;
                        }
                        else if (containedStarter)
                        {
                            amount++;
                            builtString += $" \"{splittedAsList[i]}\"";
                            indexesWithAmountsAndBuiltString[fixedIndex] = (startIndex, amount, builtString);
                        }
                    }
                }
            }

            return indexesWithAmountsAndBuiltString.Count > 0 ? splittedAsList.ToArray() : splitted;
        }
    }
}
