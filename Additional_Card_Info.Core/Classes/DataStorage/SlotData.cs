using System;
using ExtensibleSaveFormat;
using MessagePack;

namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject(true)]
    public class SlotData
    {
        public bool Keep;
        public KeepState KeepState;
        public bool Recolor;
        public bool HideAccessoryButton;
        public bool ConstantlyShown;

        public SlotData() => Init();

        public void Init()
        {
            KeepState = KeepState.DontKeep;
            HideAccessoryButton = false;
            ConstantlyShown = false;
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