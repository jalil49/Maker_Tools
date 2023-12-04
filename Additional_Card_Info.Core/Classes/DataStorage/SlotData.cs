using System;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine.Serialization;

namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject(true)]
    public class SlotData
    {
        [FormerlySerializedAs("Keep")] public bool keep;
        [FormerlySerializedAs("KeepState")] public KeepState keepState;
        [FormerlySerializedAs("Recolor")] public bool recolor;
        [FormerlySerializedAs("HideAccessoryButton")] public bool hideAccessoryButton;
        [FormerlySerializedAs("ConstantlyShown")] public bool constantlyShown;

        public SlotData() => Init();

        public void Init()
        {
            keepState = KeepState.DontKeep;
            hideAccessoryButton = false;
            constantlyShown = false;
        }

        public PluginData Serialize() =>
            new PluginData
            {
                version = Constants.AccessoryKeyVersion,
                data =
                {
                    [Constants.AccessoryKey] = MessagePackSerializer.Serialize(this)
                }
            };

        public static SlotData Deserialize(object bytearray) =>
            MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
    }
}