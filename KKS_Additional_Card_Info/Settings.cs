using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Additional_Card_Info
{
    [BepInProcess("KoikatsuSunshine")]
    public partial class Settings : BaseUnityPlugin
    {
        private void EventRegister()
        {
            ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported; ;
        }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData, Dictionary<int, int?> coordinateMapping)
        {
            importedExtendedData.TryGetValue(GUID, out var ACI_Data);
            if (ACI_Data != null)
            {
                Logger.LogDebug($"ACI Data version {ACI_Data.version} Found on import");

                var data = new DataStruct();
                var CardInfo = data.CardInfo;
                var CoordinateInfo = data.CoordinateInfo;
                if (ACI_Data.version == 1)
                {
                    if (ACI_Data.data.TryGetValue("CardInfo", out var ByteData) && ByteData != null)
                    {
                        CardInfo = MessagePackSerializer.Deserialize<Cardinfo>((byte[])ByteData);
                    }
                    if (ACI_Data.data.TryGetValue("CoordinateInfo", out ByteData) && ByteData != null)
                    {
                        CoordinateInfo = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateInfo>>((byte[])ByteData);
                    }
                }
                else if (ACI_Data.version == 0)
                {
                    Migrator.MigrateV0(ACI_Data, ref data);
                }
                else
                {
                    Logger.LogWarning("New version of plugin detected on import please update");
                }

                var keylist = CoordinateInfo.Keys.ToList();
                keylist.Remove(0);
                foreach (var item in keylist)
                {
                    CoordinateInfo.Remove(item);
                }
                ACI_Data.data.Clear();
                ACI_Data.data.Add("CardInfo", MessagePackSerializer.Serialize(CardInfo));
                ACI_Data.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(CoordinateInfo));
            }
        }
    }
}
