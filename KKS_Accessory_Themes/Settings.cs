using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;
using static Accessory_Themes.Migrator.Migrator;

namespace Accessory_Themes
{
    [BepInProcess("KoikatsuSunshine")]
    public partial class Settings : BaseUnityPlugin
    {
        private void GameUnique() { ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported; }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData, Dictionary<int, int?> coordinateMapping)
        {
            if (!importedExtendedData.TryGetValue(Guid, out var data) || data == null || data.version >= 2) return;

            var coordinate = new Dictionary<int, CoordinateDataV1>();

            switch (data.version)
            {
                case 1:
                    if (data.data.TryGetValue(Constants.CoordinateKey, out var byteData) && byteData != null)
                    {
                        StandardCharaMigrateV0(data, ref coordinate, coordinateMapping.Count);
                    }
                    break;
                case 2:
                    if (data.data.TryGetValue(Constants.CoordinateKey, out byteData) && byteData != null)
                    {
                        coordinate = CoordinateDataV1.DictDeserialize(byteData);
                    }
                    break;
            }

            var transfer = new Dictionary<int, CoordinateDataV1>();

            foreach (var item in coordinateMapping)
            {
                if (!coordinate.TryGetValue(item.Key, out var coord) || !item.Value.HasValue) continue;
                transfer[item.Value.Value] = coord;
            }

            data.data.Clear();
            data.data[Constants.CoordinateKey] = MessagePackSerializer.Serialize(transfer);
        }
    }
}
