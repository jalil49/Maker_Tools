using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;

namespace Accessory_Themes
{
    [BepInProcess("KoikatsuSunshine")]
    public partial class Settings : BaseUnityPlugin
    {
        private void GameUnique() { ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported; }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData, Dictionary<int, int?> coordinateMapping)
        {
            if (!importedExtendedData.TryGetValue(GUID, out var Data) || Data == null || Data.version >= 2) return;

            var Coordinate = new Dictionary<int, Migrator.CoordinateDataV1>();

            switch (Data.version)
            {
                case 1:
                    if (Data.data.TryGetValue(Constants.CoordinateKey, out var ByteData) && ByteData != null)
                    {
                        Migrator.StandardCharaMigrateV0(Data, ref Coordinate, coordinateMapping.Count);
                    }
                    break;
                case 2:
                    if (Data.data.TryGetValue(Constants.CoordinateKey, out ByteData) && ByteData != null)
                    {
                        Coordinate = Migrator.CoordinateDataV1.DictDeserialize(ByteData);
                    }
                    break;
            }

            var transfer = new Dictionary<int, Migrator.CoordinateDataV1>();

            foreach (var item in coordinateMapping)
            {
                if (!Coordinate.TryGetValue(item.Key, out var coord) || !item.Value.HasValue) continue;
                transfer[item.Value.Value] = coord;
            }

            Data.data.Clear();
            Data.data[Constants.CoordinateKey] = MessagePackSerializer.Serialize(transfer);
        }
    }
}
