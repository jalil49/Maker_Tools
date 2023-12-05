using System.Collections.Generic;
using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;

namespace Accessory_Parents
{
    [BepInProcess("KoikatsuSunshine")]
    public partial class Settings
    {
        private void GameUnique()
        {
            ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported;
        }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData,
            Dictionary<int, int?> coordinateMapping)
        {
            if (!importedExtendedData.TryGetValue(Guid, out var data) || data == null) return;
            var parentData = new Dictionary<int, CoordinateData>();

            if (data.version == 1)
            {
                if (data.data.TryGetValue("Coordinate_Data", out var byteData) && byteData != null)
                    parentData = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateData>>((byte[])byteData);
            }
            else if (data.version == 0)
            {
                Migrator.MigrateV0(data, ref parentData);
            }
            else
            {
                Logger.LogWarning("New version of plugin detected please update");
            }

            var transfer = new Dictionary<int, CoordinateData>();

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