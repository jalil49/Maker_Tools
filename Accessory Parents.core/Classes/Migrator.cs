using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine;

namespace Accessory_Parents
{
    internal class Migrator
    {
        public static void MigrateV0(PluginData pluginData, ref Dictionary<int, CoordinateData> dataDict)
        {
            if (pluginData.data.TryGetValue("Parenting_Names", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<string, int>[]>((byte[])byteData);

                for (var i = 0; i < temp.Length; i++)
                    if (!dataDict.TryGetValue(i, out var _))
                        dataDict[i] = new CoordinateData();

                for (var i = 0; i < temp.Length; i++)
                {
                    var convert = new List<CustomName>();
                    foreach (var item in temp[i]) convert.Add(new CustomName(item.Key, item.Value));
                    dataDict[i].parentGroups = convert;
                }
            }

            if (pluginData.data.TryGetValue("Parenting_Data", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                    foreach (var item in temp[i])
                        dataDict[i].parentGroups.First(x => item.Key == x.ParentSlot).childSlots = item.Value;
            }

            if (pluginData.data.TryGetValue("Relative_Data", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var relativeData = dataDict[i].RelativeData;
                    foreach (var item in temp[i])
                    {
                        var vectorData = item.Value;
                        relativeData[item.Key] = new Vector3[]
                            { vectorData[0, 0], vectorData[0, 1], vectorData[0, 2] };
                    }
                }
            }
        }

        public static CoordinateData CoordinateMigrateV0(PluginData plugininData)
        {
            var data = new CoordinateData();
            if (plugininData.data.TryGetValue("Parenting_Names", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<string, int>>((byte[])byteData);
                var convert = new List<CustomName>();
                foreach (var item in temp) convert.Add(new CustomName(item.Key, item.Value));
                data.parentGroups = convert;
            }

            if (plugininData.data.TryGetValue("Parenting_Data", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>>((byte[])byteData);
                foreach (var item in temp)
                {
                    var nameStruct = data.parentGroups.First(x => item.Key == x.ParentSlot);
                    nameStruct.childSlots = item.Value;
                }
            }

            if (plugininData.data.TryGetValue("Relative_Data", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>>((byte[])byteData);
                var relativeData = data.RelativeData;
                foreach (var item in temp)
                {
                    var vectorData = item.Value;
                    relativeData[item.Key] = new[] { vectorData[0, 0], vectorData[0, 1], vectorData[0, 2] };
                }
            }

            return data;
        }
    }
}