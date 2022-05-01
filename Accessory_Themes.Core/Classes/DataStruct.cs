using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Accessory_Themes
{
    [Serializable]
    [MessagePackObject(true)]
    public class SlotData
    {
        public string ThemeName { get; set; } = "";
        public bool IsRelative { get; set; }

        public ExtensibleSaveFormat.PluginData Serialize()
        {
            if (ThemeName.IsNullOrEmpty())
            {
                return null;
            }
            return new ExtensibleSaveFormat.PluginData()
            {
                version = Constants.AccessoryKeyVersion,
                data = new Dictionary<string, object>() { [Constants.AccessoryKey] = MessagePackSerializer.Serialize(this) }
            };
        }
        public static SlotData Deserialize(object bytearray) => MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
    }


    [Serializable]
    [MessagePackObject]
    public class ThemeData
    {
        [Key("_themename")]
        public string ThemeName { get; set; }

        [Key("_isrelative")]
        public bool Isrelative { get; set; }

        [IgnoreMember]
        public Color[] Colors { get; set; }

        [Key("_slots")]
        public List<int> ThemedSlots { get; set; }

        public ThemeData(string _themename)
        {
            ThemeName = _themename;
            NullCheck();
        }

        public ThemeData(string _themename, ChaFileAccessory.PartsInfo part)
        {
            ThemeName = _themename;
            Colors = new Color[part.color.Length];
            for (var i = 0; i < Colors.Length; i++)
            {
                Colors[i] = new Color(part.color[i].r, part.color[i].g, part.color[i].b, part.color[i].a);
            }
            NullCheck();
        }

        public ThemeData(ThemeData _copy, bool partial)
        {
            ThemeName = _copy.ThemeName;
            Isrelative = _copy.Isrelative;
            Colors = _copy.Colors.ToNewArray(4);
            if (!partial) ThemedSlots = _copy.ThemedSlots.ToNewList();
            NullCheck();
        }

        public ThemeData(string _themename, Color[] _colors)
        {
            ThemeName = _themename;
            Colors = _colors.ToNewArray(4);
            NullCheck();
        }

        public ThemeData(string _themename, bool _isrelative, Color[] _colors, List<int> _slots) => CopyData(_themename, _isrelative, _colors, _slots);

        public ThemeData(ThemeData _copy) => CopyData(_copy.ThemeName, _copy.Isrelative, _copy.Colors, _copy.ThemedSlots);

        public void CopyData(string _themename, bool _isrelative, Color[] _colors, List<int> _slots)
        {
            ThemeName = _themename;
            Isrelative = _isrelative;
            ThemedSlots = _slots.ToNewList();
            Colors = _colors.ToNewArray(4);
            NullCheck();
        }

        private void NullCheck()
        {
            if (ThemeName == null) ThemeName = "";
            if (Colors == null) Colors = new Color[4];
            for (var i = 0; i < Colors.Length; i++)
            {
                if (Colors[i] == null)
                {
                    Colors[i] = new Color();
                }
            }
            if (ThemedSlots == null) ThemedSlots = new List<int>();
        }
    }
}
