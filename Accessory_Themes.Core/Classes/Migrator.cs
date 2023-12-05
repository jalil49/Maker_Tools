using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine;

namespace Accessory_Themes
{
    public static class Migrator
    {
        public static void MigrateV0(PluginData myData, ref DataStruct data)
        {
            if (myData.data.TryGetValue("Theme_Names", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<string>[]>((byte[])byteData);
                if (temp == null || temp.All(x => x.Count == 0)) return;
                for (var i = 0; i < temp.Length; i++)
                    if (!data.Coordinate.TryGetValue(i, out var _))
                        data.Coordinate[i] = new CoordinateData();
                for (var i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Count > 0)
                        temp[i].RemoveAt(0);
                    var themes = data.Coordinate[i].themes;
                    foreach (var item in temp[i]) themes.Add(new ThemeData(item));
                }
            }

            if (myData.data.TryGetValue("Theme_dic", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var themes = data.Coordinate[i].themes;
                    foreach (var item in temp[i]) themes[item.Value].ThemedSlots.Add(item.Key);
                }
            }

            if (myData.data.TryGetValue("Color_Theme_dic", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<Color[]>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var list = temp[i];
                    if (list.Count > 0)
                        list.RemoveAt(0);

                    var themes = data.Coordinate[i].themes;
                    for (var j = 0; j < list.Count; j++) themes[j].Colors = list[j];
                }
            }

            if (myData.data.TryGetValue("Relative_Theme_Bools", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<bool>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Count > 0)
                        temp[i].RemoveAt(0);
                    var themes = data.Coordinate[i].themes;
                    for (var j = 0; j < temp[i].Count; j++) themes[j].IsRelative = temp[i][j];
                }
            }

            if (myData.data.TryGetValue("Relative_ACC_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int[]>>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++) data.Coordinate[i].RelativeAccDictionary = temp[i];
            }
        }

        public static CoordinateData CoordinateMigrateV0(PluginData myData)
        {
            var data = new CoordinateData();
            if (myData.data.TryGetValue("Theme_Names", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<string>>((byte[])byteData);
                if (temp == null || temp.Count == 0) return data;

                if (temp.Count > 0)
                    temp.RemoveAt(0);
                var themes = data.themes;
                foreach (var item in temp) themes.Add(new ThemeData(item));
            }

            if (myData.data.TryGetValue("Theme_dic", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])byteData);
                var themes = data.themes;
                foreach (var item in temp) themes[item.Value].ThemedSlots.Add(item.Key);
            }

            if (myData.data.TryGetValue("Color_Theme_dic", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<Color[]>>((byte[])byteData);
                if (temp.Count > 0)
                    temp.RemoveAt(0);
                var themes = data.themes;
                for (var j = 0; j < temp.Count; j++) themes[j].Colors = temp[j];
            }

            if (myData.data.TryGetValue("Relative_Theme_Bools", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<bool>>((byte[])byteData);
                if (temp.Count > 0)
                    temp.RemoveAt(0);
                var themes = data.themes;
                for (var j = 0; j < temp.Count; j++) themes[j].IsRelative = temp[j];
            }

            if (myData.data.TryGetValue("Relative_ACC_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int[]>>>((byte[])byteData);
                data.RelativeAccDictionary = temp;
            }

            return data;
        }
    }
}