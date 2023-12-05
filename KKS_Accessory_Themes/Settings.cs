using System.Collections.Generic;
using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;

namespace Accessory_Themes
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
            if (!importedExtendedData.TryGetValue(Guid, out var pluginData) || pluginData == null) return;
            var dataStruct = new DataStruct();
            if (pluginData.version == 1)
            {
                if (pluginData.data.TryGetValue("CoordinateData", out var byteData) && byteData != null)
                    dataStruct.Coordinate =
                        MessagePackSerializer.Deserialize<Dictionary<int, CoordinateData>>((byte[])byteData);
            }
            else if (pluginData.version == 0)
            {
                Migrator.MigrateV0(pluginData, ref dataStruct);
            }
            else
            {
                Logger.LogWarning("New plugin version found on card please update");
            }

            var transfer = new Dictionary<int, CoordinateData>();

            foreach (var item in coordinateMapping)
            {
                if (!dataStruct.Coordinate.TryGetValue(item.Key, out var coord) || !item.Value.HasValue) continue;
                transfer[item.Value.Value] = coord;
            }

            pluginData.data.Clear();
            pluginData.data["CoordinateData"] = MessagePackSerializer.Serialize(transfer);
        }
    }
}