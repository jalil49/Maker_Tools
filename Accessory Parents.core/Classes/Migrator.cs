using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Accessory_Parents
{
    class Migrator
    {
        public static void MigrateV0(PluginData Data, ref Dictionary<int, CoordinateData> data)
        {
            if (Data.data.TryGetValue("Parenting_Names", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<string, int>[]>((byte[])ByteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var Convert = new List<Custom_Name>();
                    foreach (var item in temp[i])
                    {
                        Convert.Add(new Custom_Name(item.Key, item.Value));
                    }
                    data[i].Parent_Groups = Convert;
                }
            }
            if (Data.data.TryGetValue("Parenting_Data", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>[]>((byte[])ByteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    foreach (var item in temp[i])
                    {
                        var namestruct = data[i].Parent_Groups.First(x => item.Key == x.ParentSlot);
                        if (namestruct != null)
                        {
                            namestruct.ChildSlots = item.Value;
                        }
                    }
                }
            }
            if (Data.data.TryGetValue("Relative_Data", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>[]>((byte[])ByteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var relative_data = data[i].Relative_Data;
                    foreach (var item in temp[i])
                    {
                        var vecdata = item.Value;
                        relative_data[item.Key] = new Vector3[3] { vecdata[0, 0], vecdata[0, 1], vecdata[0, 2] };
                    }
                }
            }
        }

        public static CoordinateData CoordinateMigrateV0(PluginData PlugininData)
        {
            var data = new CoordinateData();
            if (PlugininData.data.TryGetValue("Parenting_Names", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<string, int>>((byte[])ByteData);
                var Convert = new List<Custom_Name>();
                foreach (var item in temp)
                {
                    Convert.Add(new Custom_Name(item.Key, item.Value));
                }
                data.Parent_Groups = Convert;
            }
            if (PlugininData.data.TryGetValue("Parenting_Data", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>>((byte[])ByteData);
                foreach (var item in temp)
                {
                    var namestruct = data.Parent_Groups.First(x => item.Key == x.ParentSlot);
                    if (namestruct != null)
                    {
                        namestruct.ChildSlots = item.Value;
                    }
                }
            }
            if (PlugininData.data.TryGetValue("Relative_Data", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>>((byte[])ByteData);
                var relative_data = data.Relative_Data;
                foreach (var item in temp)
                {
                    var vecdata = item.Value;
                    relative_data[item.Key] = new Vector3[3] { vecdata[0, 0], vecdata[0, 1], vecdata[0, 2] };
                }
            }
            return data;
        }
    }
}
