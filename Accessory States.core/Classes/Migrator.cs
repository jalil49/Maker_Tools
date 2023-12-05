using System.Collections.Generic;
using ExtensibleSaveFormat;
using MessagePack;

namespace Accessory_States
{
    internal class Migrator
    {
        public static void MigrateV0(PluginData plugindata, ref Dictionary<int, CoordinateData> data)
        {
            if (plugindata.data.TryGetValue("ACC_Binding_Dictionary", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                    if (!data.TryGetValue(i, out var _))
                        data[i] = new CoordinateData();
                for (var i = 0; i < temp.Length; i++)
                {
                    var sub = temp[i];
                    var slotInfo = data[i].SlotInfo;

                    foreach (var element in sub)
                    {
                        int result;
                        if (element.Value < 9)
                            result = element.Value - 1;
                        else
                            result = element.Value;
                        slotInfo[element.Key] = new SlotData { Binding = result };
                    }
                }
            }

            if (plugindata.data.TryGetValue("ACC_State_array", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int[]>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var slotInfo = data[i].SlotInfo;

                    foreach (var pair in temp[i])
                        if (slotInfo.TryGetValue(pair.Key, out var slotdata))
                        {
                            var list = slotdata.States = new List<int[]> { pair.Value };
                            if (list[0][0] == 1) list[0][0] = 3;
                            if (list[0][1] == 1) list[0][1] = 3;
                        }
                }
            }

            if (plugindata.data.TryGetValue("ACC_Name_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, string>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                    foreach (var item in temp[i])
                        data[i].Names[item.Key] = new NameData { Name = item.Value };
            }

            if (plugindata.data.TryGetValue("ACC_Parented_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, bool>[]>((byte[])byteData);

                for (var i = 0; i < temp.Length; i++)
                    if (!data.TryGetValue(i, out var _))
                        data[i] = new CoordinateData();

                for (var i = 0; i < temp.Length; i++)
                {
                    var slotInfo = data[i].SlotInfo;
                    foreach (var item in temp[i])
                    {
                        if (!slotInfo.TryGetValue(item.Key, out var slotdata))
                            slotdata = slotInfo[item.Key] = new SlotData();
                        slotdata.parented = item.Value;
                    }
                }
            }
        }

        public static CoordinateData CoordinateMigrateV0(PluginData plugindata)
        {
            var data = new CoordinateData();
            if (plugindata.data.TryGetValue("ACC_Binding_Dictionary", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])byteData);

                var slotInfo = data.SlotInfo;

                foreach (var element in temp)
                {
                    int result;
                    if (element.Value < 9)
                        result = element.Value - 1;
                    else
                        result = element.Value;
                    temp[element.Key] = result;
                    slotInfo[element.Key] = new SlotData { Binding = element.Value };
                }
            }

            if (plugindata.data.TryGetValue("ACC_State_array", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int[]>>((byte[])byteData);
                var slotInfo = data.SlotInfo;
                foreach (var item in temp)
                    if (slotInfo.TryGetValue(item.Key, out var slotdata))
                        slotdata.States = new List<int[]> { item.Value };
            }

            if (plugindata.data.TryGetValue("ACC_Name_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])byteData);
                foreach (var item in temp) data.Names[item.Key] = new NameData { Name = item.Value };
            }

            if (plugindata.data.TryGetValue("ACC_Parented_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])byteData);
                var slotInfo = data.SlotInfo;
                foreach (var item in temp)
                {
                    if (!slotInfo.TryGetValue(item.Key, out var slotdata))
                        slotdata = slotInfo[item.Key] = new SlotData();
                    slotdata.parented = item.Value;
                }
            }

            return data;
        }
    }
}