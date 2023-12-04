using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Accessory_Parents
{
    [BepInProcess("KoikatsuSunshine")]
    public partial class Settings
    {
        private void GameUnique()
        {
            ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported;
        }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData, Dictionary<int, int?> coordinateMapping)
        {
            if (!importedExtendedData.TryGetValue(Guid, out var data) || data == null || data.version > 1) return;
            var parentData = new Dictionary<int, Migrator.CoordinateDataV1>();

            switch (data.version)
            {
                case 1:
                {
                    if (data.data.TryGetValue("Coordinate_Data", out var byteData) && byteData != null)
                    {
                        parentData = MessagePackSerializer.Deserialize<Dictionary<int, Migrator.CoordinateDataV1>>((byte[])byteData);
                    }

                    break;
                }
                case 0:
                    Migrator.MigrateV0(data, ref parentData, coordinateMapping.Count);
                    break;
                default:
                    Logger.LogWarning("New version of plugin detected please update");
                    break;
            }

            var transfer = new Dictionary<int, Migrator.CoordinateDataV1>();

            foreach (var item in coordinateMapping)
            {
                if (!parentData.TryGetValue(item.Key, out var coord) || !item.Value.HasValue) continue;
                transfer[item.Value.Value] = coord;

            }
            data.data.Clear();
            data.data["Coordinate_Data"] = MessagePackSerializer.Serialize(transfer);
        }
    }
}