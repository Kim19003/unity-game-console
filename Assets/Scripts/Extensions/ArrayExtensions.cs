using System;

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
    }
}
