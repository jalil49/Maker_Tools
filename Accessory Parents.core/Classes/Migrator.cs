using ExtensibleSaveFormat;
using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Accessory_Parents
{
    class Migrator
    {
        public static void StandardCharaMigrator(ChaControl control, PluginData pluginData)
        {
            if (pluginData.version == 2) return;
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
                        CoordDict = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateDataV1>>((byte[])ByteData);
                    }
                    break;
            }
            foreach (var item in CoordDict)
            {
                if (item.Key >= control.chaFile.coordinate.Length) continue;


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
                for (var i = 0; i < temp.Length && i < limit; i++)
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
                for (var i = 0; i < temp.Length && i < limit; i++)
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

        public static CoordinateDataV1 CoordinateMigrateV0(PluginData PlugininData)
        {
            var data = new CoordinateDataV1();
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

        [Serializable]
        [MessagePackObject]
        public class CoordinateDataV1
        {
            [Key("_theme_names")]
            public List<Custom_Name> Parent_Groups;

            [Key("_relative_data")]
            public Dictionary<int, Vector3[]> Relative_Data;

            [IgnoreMember]
            internal Dictionary<int, int> Child;

            [IgnoreMember]
            internal Dictionary<int, List<int>> RelatedNames;
            [IgnoreMember]
            internal Dictionary<int, string> Old_Parent;

            public CoordinateDataV1() { NullCheck(); }

            public CoordinateDataV1(CoordinateDataV1 _copy) => CopyData(_copy);

            public CoordinateDataV1(List<Custom_Name> _theme_names, Dictionary<int, Vector3[]> _relative_data)
            {
                Parent_Groups = _theme_names.ToNewList();
                Relative_Data = _relative_data.ToNewDictionary();
                NullCheck();
            }

            public void CleanUp()
            {
                NullCheck();
                Parent_Groups.RemoveAll(x => x.ParentSlot == -1 || x.ChildSlots.Count == 0);
                var relativeclean = Relative_Data.Keys.Where(x => !Parent_Groups.Any(y => y.ChildSlots.Contains(x) || y.ParentSlot == x)).ToList();
                foreach (var item in relativeclean)
                {
                    Relative_Data.Remove(item);
                }
            }

            public void Clear()
            {
                NullCheck();
                Parent_Groups.Clear();
                Relative_Data.Clear();
                Child.Clear();
                RelatedNames.Clear();
                Old_Parent.Clear();
            }

            private void NullCheck()
            {
                if (Parent_Groups == null) Parent_Groups = new List<Custom_Name>();
                if (Relative_Data == null) Relative_Data = new Dictionary<int, Vector3[]>();
                if (RelatedNames == null) RelatedNames = new Dictionary<int, List<int>>();
                if (Child == null) Child = new Dictionary<int, int>();
                if (Old_Parent == null) Old_Parent = new Dictionary<int, string>();
            }

            public void CopyData(CoordinateDataV1 _copy) => CopyData(_copy.Parent_Groups, _copy.Relative_Data, _copy.Child, _copy.RelatedNames, _copy.Old_Parent);

            public void CopyData(List<Custom_Name> _theme_names, Dictionary<int, Vector3[]> _relative_data, Dictionary<int, int> _child, Dictionary<int, List<int>> _related, Dictionary<int, string> _oldparent)
            {
                Parent_Groups = _theme_names.ToNewList();
                Relative_Data = _relative_data.ToNewDictionary();
                Child = _child.ToNewDictionary();
                RelatedNames = _related.ToNewDictionary();
                Old_Parent = _oldparent.ToNewDictionary(); ;
                NullCheck();
            }
        }

        [Serializable]
        [MessagePackObject]
        public class Custom_Name
        {
            [Key("_name")]
            public string Name { get; set; }

            [Key("_slot")]
            public int ParentSlot { get; set; } = -1;

            [Key("_childslots")]
            public List<int> ChildSlots;

            public Custom_Name(string _name, int _slot, List<int> _childslots)
            {
                Name = _name;
                ParentSlot = _slot;
                ChildSlots = _childslots.ToNewList();
                NullCheck();
            }

            public Custom_Name(string _name, int _slot)
            {
                Name = _name;
                ParentSlot = _slot;
                NullCheck();
            }

            public Custom_Name(string _name)
            {
                Name = _name;
                NullCheck();
            }
            private void NullCheck()
            {
                if (Name == null) Name = "";
                if (ChildSlots == null) ChildSlots = new List<int>();
            }
        }

    }
}
