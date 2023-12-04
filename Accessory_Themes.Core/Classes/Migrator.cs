using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Accessory_Themes.Migrator
{
    public static class Migrator
    {
        public static SlotData SlotDataMigrate(PluginData pluginData)
        {
            switch(pluginData.version)
            {
                case 2:
                    if(pluginData.data.TryGetValue(Constants.AccessoryKey, out var bytearray))
                    {
                        return SlotData.Deserialize(bytearray);
                    }

                    break;
                default:
                    break;
            }

            return null;
        }

        public static void StandardCharaMigrate(ChaControl control, PluginData aciData)
        {
            if(aciData.version == 2)
                return;

            if(aciData.version > 2)
            {
                PrintOutdated();
                return;
            }

            var oldcoordinatedata = new Dictionary<int, CoordinateDataV1>();

            if(aciData.version == 0)
            {
                StandardCharaMigrateV0(aciData, ref oldcoordinatedata, control.chaFile.coordinate.Length);
            }

            if(aciData.version == 1)
            {
                if(aciData.data.TryGetValue(Constants.CoordinateKey, out var byteData) && byteData != null)
                {
                    oldcoordinatedata = CoordinateDataV1.DictDeserialize(byteData);
                }
            }

            foreach(var item in oldcoordinatedata)
            {
                if(item.Key >= control.chaFile.coordinate.Length)
                {
                    continue;
                }

                CoordinateProcess(control.chaFile.coordinate[item.Key], item.Value);

                if(item.Key == control.fileStatus.coordinateType)
                {
                    var baseparts = control.chaFile.coordinate[item.Key].accessory.parts;
                    var nowparts = control.nowCoordinate.accessory.parts;
                    for(var i = 0; i < baseparts.Length; i++)
                    {
                        if(baseparts[i].TryGetExtendedDataById(Settings.Guid, out var basedata))
                        {
                            nowparts[i].SetExtendedDataById(Settings.Guid, basedata);
                        }
                    }
                }
            }

            ExtendedSave.SetExtendedDataById(control.chaFile, Settings.Guid, new PluginData() { version = Constants.MasterSaveVersion });
        }

        public static void StandardCharaMigrateV0(PluginData myData, ref Dictionary<int, CoordinateDataV1> coordinate, int limit)
        {
            for(var i = 0; i < limit; i++)
            {
                coordinate[i] = new CoordinateDataV1();
            }

            if(myData.data.TryGetValue("Theme_Names", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<string>[]>((byte[])byteData);
                if(temp == null || temp.All(x => x.Count == 0))
                {
                    return;
                }

                for(var i = 0; i < temp.Length && i < limit; i++)
                {
                    if(temp[i].Count > 0)
                        temp[i].RemoveAt(0);
                    var themes = coordinate[i].themes;
                    foreach(var item in temp[i])
                    {
                        themes.Add(new ThemeData(item));
                    }
                }
            }

            if(myData.data.TryGetValue("Theme_dic", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for(var i = 0; i < temp.Length && i < limit; i++)
                {
                    var themes = coordinate[i].themes;
                    foreach(var item in temp[i])
                    {
                        themes[item.Value] =item.Key;
                    }
                }
            }

            if(myData.data.TryGetValue("Relative_Theme_Bools", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<bool>[]>((byte[])byteData);
                for(var i = 0; i < temp.Length && i < limit; i++)
                {
                    if(temp[i].Count > 0)
                        temp[i].RemoveAt(0);
                    var themes = coordinate[i].themes;
                    for(var j = 0; j < temp[i].Count; j++)
                    {
                        themes[j].Isrelative = temp[i][j];
                    }
                }
            }
        }

        public static CoordinateDataV1 CoordinateMigrateV0(PluginData myData)
        {
            var data = new CoordinateDataV1();
            if(myData.data.TryGetValue("Theme_Names", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<string>>((byte[])byteData);
                if(temp == null || temp.Count == 0)
                {
                    return data;
                }

                if(temp.Count > 0)
                    temp.RemoveAt(0);
                var themes = data.themes;
                foreach(var item in temp)
                {
                    themes.Add(new ThemeData(item));
                }
            }

            if(myData.data.TryGetValue("Theme_dic", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])byteData);
                var themes = data.themes;
                foreach(var item in temp)
                {
                    themes[item.Value].ThemedSlots.Add(item.Key);
                }
            }

            if(myData.data.TryGetValue("Relative_Theme_Bools", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<bool>>((byte[])byteData);
                if(temp.Count > 0)
                    temp.RemoveAt(0);
                var themes = data.themes;
                for(var j = 0; j < temp.Count; j++)
                {
                    themes[j].Isrelative = temp[j];
                }
            }

            return data;
        }

        public static void StandardCoordMigrator(ChaFileCoordinate file, PluginData importeddata)
        {
            if(importeddata.version > 1)
                return;

            var dict = new CoordinateDataV1();

            if(importeddata.version == 0)
            {
                dict = CoordinateMigrateV0(importeddata);
            }

            if(importeddata.version == 1)
            {
                if(importeddata.data.TryGetValue(Constants.CoordinateKey, out var byteData) && byteData != null)
                {
                    dict = CoordinateDataV1.Deserialize(byteData);
                }
            }

            CoordinateProcess(file, dict);
        }

        private static void CoordinateProcess(ChaFileCoordinate file, CoordinateDataV1 dict)
        {
            var parts = file.accessory.parts;

            foreach(var theme in dict.themes)
            {
                foreach(var slot in theme.ThemedSlots)
                {
                    if(slot >= parts.Length)
                        continue;
                    var tempslot = new SlotData() { ThemeName = theme.ThemeName, IsRelative = theme.Isrelative };
                    parts[slot].SetExtendedDataById(Settings.Guid, tempslot.Serialize());
                }
            }

            ExtendedSave.SetExtendedDataById(file, Settings.Guid, new PluginData { version = Constants.MasterSaveVersion });
        }

        private static void PrintOutdated()
        {
            Settings.Logger.LogWarning("New version of Accessory Themes detected please update");
        }

        [Serializable]
        [MessagePackObject]
        public class CoordinateDataV1
        {
            [FormerlySerializedAs("Themes")] [Key("_themes")]
            public List<ThemeData> themes;

            public CoordinateDataV1() { NullCheck(); }

            public CoordinateDataV1(List<ThemeData> themes)
            {
                themes = themes.ToNewList();
                NullCheck();
            }

            private void NullCheck()
            {
                if(themes == null)
                    themes = new List<ThemeData>();
            }

            public static CoordinateDataV1 Deserialize(object bytearray)
            {
                return MessagePackSerializer.Deserialize<CoordinateDataV1>((byte[])bytearray);
            }

            public static Dictionary<int, CoordinateDataV1> DictDeserialize(object bytearray)
            {
                return MessagePackSerializer.Deserialize<Dictionary<int, CoordinateDataV1>>((byte[])bytearray);
            }
        }
    }

    [Serializable]
    [MessagePackObject(true)]
    public class SlotDataV1
    {
        public string ThemeName { get; set; } = string.Empty;
        public bool IsRelative { get; set; }

        public PluginData Serialize()
        {
            if(ThemeName.IsNullOrEmpty())
            {
                return null;
            }

            return new PluginData
            {
                version = Constants.AccessoryKeyVersion,
                data = new Dictionary<string, object>
                { [Constants.AccessoryKey] = MessagePackSerializer.Serialize(this) }
            };
        }

        public static SlotData Deserialize(object bytearray)
        {
            return MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
        }
    }

    [Serializable]
    [MessagePackObject]
    public class ThemeDataV1 : IMessagePackSerializationCallbackReceiver
    {
        public ThemeDataV1(string themename)
        {
            ThemeName = themename;
            NullCheck();
        }

        public ThemeDataV1(string themename, ChaFileAccessory.PartsInfo part)
        {
            ThemeName = themename;
            Colors = new Color[part.color.Length];
            for(var i = 0; i < Colors.Length; i++)
            {
                Colors[i] = new Color(part.color[i].r, part.color[i].g, part.color[i].b, part.color[i].a);
            }

            NullCheck();
        }

        public ThemeDataV1(string themename, Color[] colors)
        {
            ThemeName = themename;
            Colors = colors ?? new Color[4] { Color.white, Color.white, Color.white, Color.white };
            NullCheck();
        }

        [Key("_themename")]
        public string ThemeName { get; set; }

        [Key("_isrelative")]
        public bool Isrelative { get; set; }

        [IgnoreMember]
        public Color[] Colors { get; set; }

        [Key("_slots")]
        public List<int> ThemedSlots { get; set; }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        public void OnBeforeSerialize() { }

        private void NullCheck()
        {
            ThemeName = ThemeName ?? string.Empty;

            Colors = Colors ?? new Color[4] { Color.white, Color.white, Color.white, Color.white };

            ThemedSlots = ThemedSlots ?? new List<int>();
        }
    }
}
