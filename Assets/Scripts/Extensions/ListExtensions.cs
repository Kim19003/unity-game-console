using System.Collections.Generic;

namespace Assets.Scripts.Extensions
{
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
}
