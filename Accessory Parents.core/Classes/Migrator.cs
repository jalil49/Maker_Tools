using System;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine;

namespace Accessory_Parents
{
    internal class Migrator
    {
        public static void StandardCharaMigrator(ChaControl control, PluginData pluginData)
        {
            if (pluginData.version == 2)
            {
                return;
            }

            if (pluginData.version > 2)
            {
                return;
            }

            var CoordDict = new Dictionary<int, CoordinateDataV1>();
            switch (pluginData.version)
            {
                case 0:
                    MigrateV0(pluginData, ref CoordDict, control.chaFile.coordinate.Length);
                    break;
                case 1:
                    if (pluginData.data.TryGetValue("Coordinate_Data", out var ByteData) && ByteData != null)
                    {
                        CoordDict =
                            MessagePackSerializer.Deserialize<Dictionary<int, CoordinateDataV1>>((byte[])ByteData);
                    }

                    break;
            }

            foreach (var item in CoordDict)
            {
                if (item.Key >= control.chaFile.coordinate.Length) { }
            }
        }

        public static void MigrateV0(PluginData Data, ref Dictionary<int, CoordinateDataV1> data, int limit)
        {
            for (var i = 0; i < limit; i++)
            {
                data[i] = new CoordinateDataV1();
            }

            if (Data.data.TryGetValue("Parenting_Names", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<string, int>[]>((byte[])ByteData);

                for (var i = 0; i < temp.Length && i < limit; i++)
                {
                    var Convert = new List<CustomName>();
                    foreach (var item in temp[i])
                    {
                        Convert.Add(new CustomName(item.Key, item.Value));
                    }

                    data[i].ParentGroups = Convert;
                }
            }

            if (Data.data.TryGetValue("Parenting_Data", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>[]>((byte[])ByteData);
                for (var i = 0; i < temp.Length && i < limit; i++)
                {
                    foreach (var item in temp[i])
                    {
                        var namestruct = data[i].ParentGroups.First(x => item.Key == x.ParentSlot);
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
                for (var i = 0; i < temp.Length && i < limit; i++)
                {
                    var relative_data = data[i].RelativeData;
                    foreach (var item in temp[i])
                    {
                        var vecdata = item.Value;
                        relative_data[item.Key] = new Vector3[3] { vecdata[0, 0], vecdata[0, 1], vecdata[0, 2] };
                    }
                }
            }
        }

        public static CoordinateDataV1 CoordinateMigrateV0(PluginData PlugininData)
        {
            var data = new CoordinateDataV1();
            if (PlugininData.data.TryGetValue("Parenting_Names", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<string, int>>((byte[])ByteData);
                var Convert = new List<CustomName>();
                foreach (var item in temp)
                {
                    Convert.Add(new CustomName(item.Key, item.Value));
                }

                data.ParentGroups = Convert;
            }

            if (PlugininData.data.TryGetValue("Parenting_Data", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>>((byte[])ByteData);
                foreach (var item in temp)
                {
                    var namestruct = data.ParentGroups.First(x => item.Key == x.ParentSlot);
                    if (namestruct != null)
                    {
                        namestruct.ChildSlots = item.Value;
                    }
                }
            }

            if (PlugininData.data.TryGetValue("Relative_Data", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>>((byte[])ByteData);
                var relative_data = data.RelativeData;
                foreach (var item in temp)
                {
                    var vecdata = item.Value;
                    relative_data[item.Key] = new Vector3[3] { vecdata[0, 0], vecdata[0, 1], vecdata[0, 2] };
                }
            }

            return data;
        }

        [Serializable]
        [MessagePackObject]
        public class CoordinateDataV1 : IMessagePackSerializationCallbackReceiver
        {
            [Key("_theme_names")]
            public List<CustomName> ParentGroups;

            [IgnoreMember]
            internal Dictionary<int, int> Child;

            [IgnoreMember]
            internal Dictionary<int, string> OldParent;

            [IgnoreMember]
            internal Dictionary<int, List<int>> RelatedNames;

            [Key("_relative_data")]
            public Dictionary<int, Vector3[]> RelativeData;

            public CoordinateDataV1() => NullCheck();

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize() => NullCheck();

            public void CleanUp()
            {
                NullCheck();
                ParentGroups.RemoveAll(x => x.ParentSlot == -1 || x.ChildSlots.Count == 0);
                var relativeClean = RelativeData.Keys
                    .Where(x => !ParentGroups.Any(y => y.ChildSlots.Contains(x) || y.ParentSlot == x)).ToList();
                foreach (var item in relativeClean)
                {
                    RelativeData.Remove(item);
                }
            }

            public void Clear()
            {
                NullCheck();
                ParentGroups.Clear();
                RelativeData.Clear();
                Child.Clear();
                RelatedNames.Clear();
                OldParent.Clear();
            }

            private void NullCheck()
            {
                ParentGroups = ParentGroups ?? new List<CustomName>();

                RelativeData = RelativeData ?? new Dictionary<int, Vector3[]>();

                RelatedNames = RelatedNames ?? new Dictionary<int, List<int>>();

                Child = Child ?? new Dictionary<int, int>();

                OldParent = OldParent ?? new Dictionary<int, string>();
            }
        }

        [Serializable]
        [MessagePackObject]
        public class CustomName : IMessagePackSerializationCallbackReceiver
        {
            [Key("_childslots")]
            public List<int> ChildSlots;

            public CustomName(string name, int slot, List<int> childslots) : this(name, slot) =>
                ChildSlots = childslots ?? new List<int>();

            public CustomName(string name, int slot) : this(name) => ParentSlot = slot;

            public CustomName(string name) : this() => Name = name;

            private CustomName() => NullCheck();

            [Key("_name")]
            public string Name { get; set; }

            [Key("_slot")]
            public int ParentSlot { get; set; } = -1;

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize() => NullCheck();

            private void NullCheck()
            {
                Name = Name ?? string.Empty;

                ChildSlots = ChildSlots ?? new List<int>();
            }
        }
    }
}