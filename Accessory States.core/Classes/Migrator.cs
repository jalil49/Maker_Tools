using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;

namespace Accessory_States
{
    class Migrator
    {
        public static void MigrateV0(PluginData plugindata, ref Data data)
        {

            if (plugindata.data.TryGetValue("ACC_Binding_Dictionary", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                for (int i = 0; i < temp.Length; i++)
                {
                    var sub = temp[i];
                    var slotinfo = data.Coordinate[i].Slotinfo;

                    foreach (var element in sub)
                    {
                        int result;
                        if (element.Value < 9)
                            result = element.Value - 1;
                        else
                            result = element.Value;
                        slotinfo[element.Key] = new Slotdata() { Binding = result };
                    }
                }
            }

            if (plugindata.data.TryGetValue("ACC_State_array", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int[]>[]>((byte[])ByteData);
                for (int i = 0; i < temp.Length; i++)
                {
                    var slotinfo = data.Coordinate[i].Slotinfo;

                    foreach (var pair in temp[i])
                    {
                        if (slotinfo.TryGetValue(pair.Key, out var slotdata))
                        {
                            var list = slotdata.States = new List<int[]> { pair.Value };
                            if (list[0][0] == 1)
                            {
                                list[0][0] = 3;
                            }
                            if (list[0][1] == 1)
                            {
                                list[0][1] = 3;
                            }
                        }
                    }
                }
            }

            if (plugindata.data.TryGetValue("ACC_Name_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, string>[]>((byte[])ByteData);
                for (int i = 0; i < temp.Length; i++)
                {
                    foreach (var item in temp[i])
                    {
                        data.Coordinate[i].Names[item.Key] = new NameData() { Name = item.Value };
                    }
                }
            }

            if (plugindata.data.TryGetValue("ACC_Parented_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, bool>[]>((byte[])ByteData);
                for (int i = 0; i < temp.Length; i++)
                {
                    var slotinfo = data.Coordinate[i].Slotinfo;
                    foreach (var item in temp[i])
                    {
                        if (!slotinfo.TryGetValue(item.Key, out var slotdata))
                        {
                            slotdata = slotinfo[item.Key] = new Slotdata();
                        }
                        slotdata.Parented = item.Value;
                    }
                }
            }
        }

        public static CoordinateData CoordinateMigrateV0(PluginData plugindata)
        {
            var data = new CoordinateData();
            if (plugindata.data.TryGetValue("ACC_Binding_Dictionary", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);

                var slotinfo = data.Slotinfo;

                foreach (var element in temp)
                {
                    int result;
                    if (element.Value < 9)
                        result = element.Value - 1;
                    else
                        result = element.Value;
                    temp[element.Key] = result;
                    slotinfo[element.Key] = new Slotdata() { Binding = element.Value };
                }
            }
            if (plugindata.data.TryGetValue("ACC_State_array", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int[]>>((byte[])ByteData);
                var slotinfo = data.Slotinfo;
                foreach (var item in temp)
                {
                    if (slotinfo.TryGetValue(item.Key, out var slotdata))
                    {
                        slotdata.States = new List<int[]> { item.Value };
                    }
                }
            }
            if (plugindata.data.TryGetValue("ACC_Name_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])ByteData);
                foreach (var item in temp)
                {
                    data.Names[item.Key] = new NameData() { Name = item.Value };
                }
            }
            if (plugindata.data.TryGetValue("ACC_Parented_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])ByteData);
                var slotinfo = data.Slotinfo;
                foreach (var item in temp)
                {
                    if (!slotinfo.TryGetValue(item.Key, out var slotdata))
                    {
                        slotdata = slotinfo[item.Key] = new Slotdata();
                    }
                    slotdata.Parented = item.Value;
                }
            }
            return data;
        }
    }
}
