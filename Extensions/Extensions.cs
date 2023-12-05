using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Extensions
{
    public static class Extensions
    {
        [NotNull]
        public static List<T> ToNewList<T>(this List<T> value)
        {
            if (value != null) return new List<T>(value);

            return new List<T>();
        }

        [NotNull]
        public static List<T> ToNewList<T>(this List<T> value, T initialValue)
        {
            if (value != null) return new List<T>(value);

            return new List<T> { initialValue };
        }

        [NotNull]
        public static List<T> ToNewList<T>(this List<T> value, params T[] initialValue)
        {
            if (value != null) return new List<T>(value);

            return initialValue.ToList();
        }

        [NotNull]
        public static Dictionary<T1, T2> ToNewDictionary<T1, T2>(this Dictionary<T1, T2> value)
        {
            if (value != null) return new Dictionary<T1, T2>(value);

            return new Dictionary<T1, T2>();
        }

        [NotNull]
        public static bool[] ToNewArray(this bool[] value, int size)
        {
            var array = new bool[size];
            if (value != null)
            {
                for (var i = 0; i < size; i++) array[i] = value[i];

                return array;
            }

            return array;
        }

        [NotNull]
        public static Color[] ToNewArray(this Color[] value, int size)
        {
            var array = new Color[size];
            if (value != null)
            {
                for (var i = 0; i < size; i++)
                {
                    var color = value[i];
                    array[i] = new Color(color.r, color.g, color.b, color.a);
                }

                return array;
            }

            for (var i = 0; i < size; i++) array[i] = new Color();

            return array;
        }
    }
}