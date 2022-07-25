using ExtensibleSaveFormat;
using MessagePack;


namespace Additional_Card_Info
{
    [MessagePackObject(true)]
    public class SlotData
    {
        public KeepState KeepState;
        public bool HideAccessoryButton;
        public bool ConstantlyShown;

        public SlotData()
        {
            KeepState = KeepState.DontKeep;
            HideAccessoryButton = false;
            ConstantlyShown = false;
        }

        public void Clear()
        {
            KeepState = KeepState.DontKeep;
            HideAccessoryButton = false;
            ConstantlyShown = false;
        }

        public PluginData Serialize()
        {
            var pluginData = new PluginData() { version = Constants.AccessoryKeyVersion };
            pluginData.data[Constants.AccessoryKey] = MessagePackSerializer.Serialize(this);
            return pluginData;
        }

        public static SlotData Deserialize(object bytearray) => MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
    }

    public enum KeepState
    {
        DontKeep = -1,
        NonHairKeep = 0,
        HairKeep = 1
    }
}
