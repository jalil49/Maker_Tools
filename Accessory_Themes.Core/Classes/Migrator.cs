using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Accessory_Themes
{
    public static class Migrator
    {
        public static void MigrateV0(PluginData MyData, ref DataStruct data)
        {
            if (MyData.data.TryGetValue("Theme_Names", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<string>[]>((byte[])ByteData);
                if (temp == null || temp.All(x => x.Count == 0))
                {
                    return;
                }
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Count > 0)
                        temp[i].RemoveAt(0);
                    var themes = data.Coordinate[i].Themes;
                    foreach (var item in temp[i])
                    {
                        themes.Add(new ThemeData(item));
                    }
                }
            }

            if (MyData.data.TryGetValue("Theme_dic", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                for (int i = 0; i < temp.Length; i++)
                {
                    var themes = data.Coordinate[i].Themes;
                    foreach (var item in temp[i])
                    {
                        themes[item.Value].ThemedSlots.Add(item.Key);
                    }
                }
            }

            if (MyData.data.TryGetValue("Color_Theme_dic", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<Color[]>[]>((byte[])ByteData);
                for (int i = 0; i < temp.Length; i++)
                {
                    var list = temp[i];
                    if (list.Count > 0)
                        list.RemoveAt(0);

                    var themes = data.Coordinate[i].Themes;
                    for (int j = 0; j < list.Count; j++)
                    {
                        themes[j].Colors = list[j];
                    }
                }
            }

            if (MyData.data.TryGetValue("Relative_Theme_Bools", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<bool>[]>((byte[])ByteData);
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Count > 0)
                        temp[i].RemoveAt(0);
                    var themes = data.Coordinate[i].Themes;
                    for (int j = 0; j < temp[i].Count; j++)
                    {
                        themes[j].Isrelative = temp[i][j];
                    }
                }
            }

            if (MyData.data.TryGetValue("Relative_ACC_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int[]>>[]>((byte[])ByteData);
                for (int i = 0; i < temp.Length; i++)
                {
                    data.Coordinate[i].Relative_ACC_Dictionary = temp[i];
                }
            }
        }

        public static CoordinateData CoordinateMigrateV0(PluginData MyData)
        {
            var data = new CoordinateData();
            if (MyData.data.TryGetValue("Theme_Names", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<string>>((byte[])ByteData);
                if (temp == null || temp.Count == 0)
                {
                    return data;
                }

                if (temp.Count > 0)
                    temp.RemoveAt(0);
                var themes = data.Themes;
                foreach (var item in temp)
                {
                    themes.Add(new ThemeData(item));
                }
            }

            if (MyData.data.TryGetValue("Theme_dic", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                var themes = data.Themes;
                foreach (var item in temp)
                {
                    themes[item.Value].ThemedSlots.Add(item.Key);
                }
            }

            if (MyData.data.TryGetValue("Color_Theme_dic", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<Color[]>>((byte[])ByteData);
                if (temp.Count > 0)
                    temp.RemoveAt(0);
                var themes = data.Themes;
                for (int j = 0; j < temp.Count; j++)
                {
                    themes[j].Colors = temp[j];
                }
            }

            if (MyData.data.TryGetValue("Relative_Theme_Bools", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<bool>>((byte[])ByteData);
                if (temp.Count > 0)
                    temp.RemoveAt(0);
                var themes = data.Themes;
                for (int j = 0; j < temp.Count; j++)
                {
                    themes[j].Isrelative = temp[j];
                }
            }

            if (MyData.data.TryGetValue("Relative_ACC_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int[]>>>((byte[])ByteData);
                data.Relative_ACC_Dictionary = temp;

            }

            return data;
        }
    }
}
