using Assets.Scripts.Other;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.Extensions
{
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

        public static void AddIfUniqueCommand(this HashSet<TimedCommandCallerCommand> hashSet, TimedCommandCallerCommand item)
        {
            if (!hashSet.Any(e => e.Command == item.Command))
            {
                hashSet.Add(item);
            }
        }
    }
}
