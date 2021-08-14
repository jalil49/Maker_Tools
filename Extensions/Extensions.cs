using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class Extensions
    {
        public static List<T> ToNewList<T>(this List<T> value)
        {
            if (value != null)
            {
                return new List<T>(value);
            }
            return new List<T>();
        }

        public static List<T> ToNewList<T>(this List<T> value, T initialvalue)
        {
            if (value != null)
            {
                return new List<T>(value);
            }
            return new List<T>() { initialvalue };
        }

        public static Dictionary<T1, T2> ToNewDictionary<T1, T2>(this Dictionary<T1, T2> value)
        {
            if (value != null)
            {
                return new Dictionary<T1, T2>(value);
            }

            return new Dictionary<T1, T2>();
        }

        public static bool[] ToNewArray(this bool[] value, int size)
        {
            bool[] array = new bool[size];
            if (value != null)
            {
                for (int i = 0; i < size; i++)
                {
                    array[i] = value[i];
                }
                return array;
            }
            return array;
        }
        public static Color[] ToNewArray(this Color[] value, int size)
        {
            Color[] array = new Color[size];
            if (value != null)
            {
                for (int i = 0; i < size; i++)
                {
                    var color = value[i];
                    array[i] = new Color(color.r, color.g, color.b, color.a);
                }
                return array;
            }
            for (int i = 0; i < size; i++)
            {
                array[i] = new Color();
            }
            return array;
        }
    }
}
