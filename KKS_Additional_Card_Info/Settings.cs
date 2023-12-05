using System.Collections.Generic;
using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;

namespace Additional_Card_Info
{
    [BepInProcess("KoikatsuSunshine")]
    public partial class Settings : BaseUnityPlugin
    {
        private void GameUnique()
        {
            ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported;
        }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData,
            Dictionary<int, int?> coordinateMapping)
        {
            if (!importedExtendedData.TryGetValue(Guid, out var aciData) || aciData == null) return;
            Logger.LogDebug($"ACI Data version {aciData.version} Found on import");

            var data = new DataStruct();
            var cardInfo = data.CardInfo;
            var coordinateInfo = data.CoordinateInfo;
            if (aciData.version == 1)
            {
                if (aciData.data.TryGetValue("CardInfo", out var byteData) && byteData != null)
                    cardInfo = MessagePackSerializer.Deserialize<CardInfo>((byte[])byteData);
                if (aciData.data.TryGetValue("CoordinateInfo", out byteData) && byteData != null)
                    coordinateInfo =
                        MessagePackSerializer.Deserialize<Dictionary<int, CoordinateInfo>>((byte[])byteData);
            }
            else if (aciData.version == 0)
            {
                Migrator.MigrateV0(aciData, ref data);
            }
            else
            {
                Logger.LogWarning("New version of plugin detected on import please update");
            }

            var transfer = new Dictionary<int, CoordinateInfo>();

            foreach (var item in coordinateMapping)
            {
                if (!data.CoordinateInfo.TryGetValue(item.Key, out var info) || !item.Value.HasValue) continue;
                transfer[item.Value.Value] = info;
            }

            aciData.data.Clear();
            aciData.data.Add("CardInfo", MessagePackSerializer.Serialize(cardInfo));
            aciData.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(coordinateInfo));
        }
    }
}