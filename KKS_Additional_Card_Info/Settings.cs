using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;

namespace Additional_Card_Info
{
    [BepInProcess("KoikatsuSunshine")]
    public partial class Settings : BaseUnityPlugin
    {
        private void GameUnique()
        {
            ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported;
        }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData, Dictionary<int, int?> coordinateMapping)
        {
            if (!importedExtendedData.TryGetValue(GUID, out var ACI_Data) || ACI_Data == null || ACI_Data.version > 1) return;
            Logger.LogDebug($"ACI Data version {ACI_Data.version} Found on import");

            var CardInfo = new Migrator.CardInfoV1();
            var CoordinateInfo = new Dictionary<int, Migrator.CoordinateInfoV1>();
            if (ACI_Data.version == 1)
            {
                if (ACI_Data.data.TryGetValue("CardInfo", out var ByteData) && ByteData != null)
                {
                    CardInfo = MessagePackSerializer.Deserialize<Migrator.CardInfoV1>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("CoordinateInfo", out ByteData) && ByteData != null)
                {
                    CoordinateInfo = MessagePackSerializer.Deserialize<Dictionary<int, Migrator.CoordinateInfoV1>>((byte[])ByteData);
                }
            }
            else if (ACI_Data.version == 0)
            {
                Migrator.StandardCharaMigrateV0(ACI_Data, ref CoordinateInfo, ref CardInfo);
            }
            var transfer = new Dictionary<int, Migrator.CoordinateInfoV1>();

            foreach (var item in coordinateMapping)
            {
                if (!CoordinateInfo.TryGetValue(item.Key, out var info) || !item.Value.HasValue) continue;
                transfer[item.Value.Value] = info;
            }

            ACI_Data.data.Clear();
            ACI_Data.data.Add("CardInfo", MessagePackSerializer.Serialize(CardInfo));
            ACI_Data.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(CoordinateInfo));
        }
    }
}
