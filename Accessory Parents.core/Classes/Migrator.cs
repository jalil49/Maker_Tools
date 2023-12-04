using System;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine;
using UnityEngine.Serialization;

namespace Accessory_Parents
{
    internal class Migrator
    {
        public static void StandardCharaMigrator(ChaControl control, PluginData pluginData)
        {
            if (pluginData.version == 2) return;

            if (pluginData.version > 2) return;

            var coordDict = new Dictionary<int, CoordinateDataV1>();
            switch (pluginData.version)
            {
                case 0:
                    MigrateV0(pluginData, ref coordDict, control.chaFile.coordinate.Length);
                    break;
                case 1:
                    if (pluginData.data.TryGetValue("Coordinate_Data", out var byteData) && byteData != null)
                        coordDict =
                            MessagePackSerializer.Deserialize<Dictionary<int, CoordinateDataV1>>((byte[])byteData);

                    break;
            }

            foreach (var item in coordDict)
                if (item.Key >= control.chaFile.coordinate.Length)
                {
                }
        }

        public static void MigrateV0(PluginData data, ref Dictionary<int, CoordinateDataV1> dictionary, int limit)
        {
            for (var i = 0; i < limit; i++) dictionary[i] = new CoordinateDataV1();

            if (data.data.TryGetValue("Parenting_Names", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<string, int>[]>((byte[])byteData);

                for (var i = 0; i < temp.Length && i < limit; i++)
                {
                    var convert = new List<CustomName>();
                    foreach (var item in temp[i]) convert.Add(new CustomName(item.Key, item.Value));

                    dictionary[i].parentGroups = convert;
                }
            }

            if (data.data.TryGetValue("Parenting_Data", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length && i < limit; i++)
                    foreach (var item in temp[i])
                    {
                        var namestruct = dictionary[i].parentGroups.First(x => item.Key == x.ParentSlot);
                        if (namestruct != null) namestruct.childSlots = item.Value;
                    }
            }

            if (data.data.TryGetValue("Relative_Data", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length && i < limit; i++)
                {
                    var relativeData = dictionary[i].RelativeData;
                    foreach (var item in temp[i])
                    {
                        var vecdata = item.Value;
                        relativeData[item.Key] = new Vector3[3] { vecdata[0, 0], vecdata[0, 1], vecdata[0, 2] };
                    }
                }
            }
        }

        public static CoordinateDataV1 CoordinateMigrateV0(PluginData plugininData)
        {
            var data = new CoordinateDataV1();
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
                    var namestruct = data.parentGroups.First(x => item.Key == x.ParentSlot);
                    if (namestruct != null) namestruct.childSlots = item.Value;
                }
            }

            if (plugininData.data.TryGetValue("Relative_Data", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>>((byte[])byteData);
                var relativeData = data.RelativeData;
                foreach (var item in temp)
                {
                    var vecdata = item.Value;
                    relativeData[item.Key] = new Vector3[3] { vecdata[0, 0], vecdata[0, 1], vecdata[0, 2] };
                }
            }

            return data;
        }

        [Serializable]
        [MessagePackObject]
        public class CoordinateDataV1 : IMessagePackSerializationCallbackReceiver
        {
            [FormerlySerializedAs("ParentGroups")] [Key("_theme_names")] public List<CustomName> parentGroups;

            [IgnoreMember] internal Dictionary<int, int> Child;

            [IgnoreMember] internal Dictionary<int, string> OldParent;

            [IgnoreMember] internal Dictionary<int, List<int>> RelatedNames;

            [Key("_relative_data")] public Dictionary<int, Vector3[]> RelativeData;

            public CoordinateDataV1()
            {
                NullCheck();
            }

            public void OnBeforeSerialize()
            {
            }

            public void OnAfterDeserialize()
            {
                NullCheck();
            }

            public void CleanUp()
            {
                NullCheck();
                parentGroups.RemoveAll(x => x.ParentSlot == -1 || x.childSlots.Count == 0);
                var relativeClean = RelativeData.Keys
                    .Where(x => !parentGroups.Any(y => y.childSlots.Contains(x) || y.ParentSlot == x)).ToList();
                foreach (var item in relativeClean) RelativeData.Remove(item);
            }

            public void Clear()
            {
                NullCheck();
                parentGroups.Clear();
                RelativeData.Clear();
                Child.Clear();
                RelatedNames.Clear();
                OldParent.Clear();
            }

            private void NullCheck()
            {
                parentGroups = parentGroups ?? new List<CustomName>();

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
            [FormerlySerializedAs("ChildSlots")] [Key("_childslots")] public List<int> childSlots;

            public CustomName(string name, int slot, List<int> childslots) : this(name, slot)
            {
                childSlots = childslots ?? new List<int>();
            }

            public CustomName(string name, int slot) : this(name)
            {
                ParentSlot = slot;
            }

            public CustomName(string name) : this()
            {
                Name = name;
            }

            private CustomName()
            {
                NullCheck();
            }

            [Key("_name")] public string Name { get; set; }

            [Key("_slot")] public int ParentSlot { get; set; } = -1;

            public void OnBeforeSerialize()
            {
            }

            public void OnAfterDeserialize()
            {
                NullCheck();
            }

            private void NullCheck()
            {
                Name = Name ?? string.Empty;

                childSlots = childSlots ?? new List<int>();
            }
        }
    }
}