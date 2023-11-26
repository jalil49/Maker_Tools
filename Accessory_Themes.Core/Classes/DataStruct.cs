using ExtensibleSaveFormat;
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
        public bool IsRelative { get; set; }
        public ThemeData Theme { get; set; }

        public PluginData Serialize()
        {
            if(Theme == null)
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
    public class ThemeData : IMessagePackSerializationCallbackReceiver, IEquatable<ThemeData>
    {
        public ThemeData(string themeName)
        {
            ThemeName = themeName;
            NullCheck();
        }

        public ThemeData(string themeName, ChaFileAccessory.PartsInfo part) : this(themeName)
        {
            BaseColors = new Color[part.color.Length];
            for(var i = 0; i < BaseColors.Length; i++)
            {
                BaseColors[i] = new Color(part.color[i].r, part.color[i].g, part.color[i].b, part.color[i].a);
            }
        }

        public ThemeData(string themeName, Color[] baseColors) : this(themeName)
        {
            BaseColors = baseColors ?? new Color[4] { Color.white, Color.white, Color.white, Color.white };
        }

        public ThemeData(string themeName, Color[] baseColors, string collision) : this(themeName, baseColors)
        {
            Collision = collision;
        }

        public string ThemeName { get; set; }

        public Color[] BaseColors { get; set; }

        public string Collision { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as ThemeData);
        }

        public bool Equals(ThemeData other)
        {
            return !(other is null) &&
                   ThemeName == other.ThemeName &&
                   EqualityComparer<Color[]>.Default.Equals(BaseColors, other.BaseColors) &&
                   Collision == other.Collision;
        }

        public override int GetHashCode()
        {
            var hashCode = -1385759252;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ThemeName);
            hashCode = hashCode * -1521134295 + EqualityComparer<Color[]>.Default.GetHashCode(BaseColors);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Collision);
            return hashCode;
        }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        public void OnBeforeSerialize()
        {
        }

        private void NullCheck()
        {
            ThemeName = ThemeName ?? string.Empty;

            Collision = Collision ?? Guid.NewGuid().ToString("N");

            BaseColors = BaseColors ?? new Color[4] { Color.white, Color.white, Color.white, Color.white };
        }
    }
}