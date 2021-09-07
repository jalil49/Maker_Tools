using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;

namespace Accessory_Parents
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
            if (!importedExtendedData.TryGetValue(GUID, out var Data) || Data == null) return;
            var Parent_Data = new Dictionary<int, CoordinateData>();

            if (Data.version == 1)
            {
                if (Data.data.TryGetValue("Coordinate_Data", out var ByteData) && ByteData != null)
                {
                    Parent_Data = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateData>>((byte[])ByteData);
                }
            }
            else if (Data.version == 0)
            {
                Migrator.MigrateV0(Data, ref Parent_Data);
            }
            else
            {
                Settings.Logger.LogWarning("New version of plugin detected please update");
            }

            var transfer = new Dictionary<int, CoordinateData>();

            foreach (var item in coordinateMapping)
            {
                if (!Parent_Data.TryGetValue(item.Key, out var coord) || !item.Value.HasValue) continue;
                transfer[item.Value.Value] = coord;

            }
            Data.data.Clear();
            Data.data["Coordinate_Data"] = MessagePackSerializer.Serialize(transfer);
        }
    }
}