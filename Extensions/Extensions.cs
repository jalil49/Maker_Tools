using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class Extensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
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
            var array = new bool[size];
            if (value != null)
            {
                for (var i = 0; i < size; i++)
                {
                    array[i] = value[i];
                }
                return array;
            }
            return array;
        }
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
            for (var i = 0; i < size; i++)
            {
                array[i] = new Color();
            }
            return array;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chaControl"></param>
        /// <param name="select">ListInfoBase to check</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ClothingUnlocker(this ChaControl chaControl, int select, ChaListDefine.KeyType value)
        {
            var lists = chaControl.infoClothes;
            if (select >= lists.Length)
                return false;
            var listInfoBase = lists[select];
            if (listInfoBase == null)
                return false;
            if (!(listInfoBase.dictInfo.TryGetValue((int)value, out var stringValue) && int.TryParse(stringValue, out var intValue)))
                return false;

            if (intValue == listInfoBase.GetInfoInt(value))
                return false;

            return true;
        }
    }
}
