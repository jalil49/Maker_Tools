using System.Collections.Generic;
using Additional_Card_Info.Classes.Migration;
using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;

namespace Additional_Card_Info
{
    [BepInProcess("KoikatsuSunshine")]
    public partial class Settings
    {
        private void GameUnique()
        {
            ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported;
        }

        private static void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData,
                                                           Dictionary<int, int?> coordinateMapping)
        {
            if (!importedExtendedData.TryGetValue(GUID, out var aciData) || aciData == null || aciData.version > 1)
            {
                return;
            }

            Logger.LogDebug($"ACI Data version {aciData.version.ToString()} Found on import");

            var cardInfo = new Migrator.CardInfoV1();
            var coordinateInfo = new Dictionary<int, Migrator.CoordinateInfoV1>();
            switch (aciData.version)
            {
                case 1:
                {
                    if (aciData.data.TryGetValue("CardInfo", out var byteData) && byteData != null)
                    {
                        cardInfo = MessagePackSerializer.Deserialize<Migrator.CardInfoV1>((byte[])byteData);
                    }

                    if (aciData.data.TryGetValue("CoordinateInfo", out byteData) && byteData != null)
                    {
                        coordinateInfo =
                            MessagePackSerializer.Deserialize<Dictionary<int, Migrator.CoordinateInfoV1>>(
                                (byte[])byteData);
                    }

                    break;
                }
                case 0:
                    Migrator.StandardCharaMigrateV0(aciData, ref coordinateInfo, ref cardInfo);
                    break;
            }

            var transfer = new Dictionary<int, Migrator.CoordinateInfoV1>();

            foreach (var item in coordinateMapping)
            {
                if (!coordinateInfo.TryGetValue(item.Key, out var info) || !item.Value.HasValue)
                {
                    continue;
                }

                transfer[item.Value.Value] = info;
            }

            aciData.data.Clear();
            aciData.data.Add("CardInfo", MessagePackSerializer.Serialize(cardInfo));
            aciData.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(transfer));
        }
    }
}