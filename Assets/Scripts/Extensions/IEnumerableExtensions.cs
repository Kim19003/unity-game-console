using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;

namespace Assets.Scripts.Extensions
{
    public static class IEnumerableExtensions
    {
        private static readonly Random random = new Random();

        public static T GetRandomElement<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ElementAt(random.Next(0, enumerable.Count()));
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || enumerable.Count() == 0;
        }
    }
}
