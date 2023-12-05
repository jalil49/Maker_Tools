using System.Collections.Generic;
using System.Linq;
using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;

namespace Accessory_States
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
            var attemptAss = false;
            var coordinate = new Dictionary<int, CoordinateData>();
            foreach (var item in coordinateMapping) coordinate[item.Key] = new CoordinateData();

            if (!importedExtendedData.TryGetValue(Guid, out var data) || data == null) attemptAss = true;

            if (!attemptAss)
            {
                if (data.version == 1)
                {
                    if (data.data.TryGetValue("CoordinateData", out var byteData) && byteData != null)
                        coordinate =
                            MessagePackSerializer.Deserialize<Dictionary<int, CoordinateData>>((byte[])byteData);
                }
                else if (data.version == 0)
                {
                    Migrator.MigrateV0(data, ref coordinate);
                }
                else
                {
                    Logger.LogWarning("New plugin version found on card please update");
                    return;
                }
            }
            else
            {
                if (!importedExtendedData.TryGetValue("madevil.kk.ass", out var assData) || assData == null) return;

                var triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var triggerGroupList = new List<AccStateSync.TriggerGroup>();

                if (assData.version > 6)
                {
                    Logger.LogWarning(
                        "New version of AccessoryStateSync found, accessory states needs update for compatibility");
                }
                else if (assData.version < 6)
                {
                    AccStateSync.Migration.ConvertCharaPluginData(assData, ref triggerPropertyList,
                        ref triggerGroupList);
                }
                else
                {
                    if (assData.data.TryGetValue("TriggerPropertyList", out var loadedTriggerProperty) &&
                        loadedTriggerProperty != null)
                    {
                        var tempTriggerProperty =
                            MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>(
                                (byte[])loadedTriggerProperty);
                        if (tempTriggerProperty?.Count > 0)
                            triggerPropertyList.AddRange(tempTriggerProperty);

                        if (assData.data.TryGetValue("TriggerGroupList", out var loadedTriggerGroup) &&
                            loadedTriggerGroup != null)
                        {
                            var tempTriggerGroup =
                                MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>(
                                    (byte[])loadedTriggerGroup);
                            if (tempTriggerGroup?.Count > 0)
                            {
                                foreach (var group in tempTriggerGroup)
                                    if (group.Guid.IsNullOrEmpty())
                                        group.Guid = System.Guid.NewGuid().ToString("D").ToUpper();
                                triggerGroupList.AddRange(tempTriggerGroup);
                            }
                        }
                    }
                }

                if (triggerPropertyList == null) triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if (triggerGroupList == null) triggerGroupList = new List<AccStateSync.TriggerGroup>();

                foreach (var item in coordinate)
                    item.Value.AccStateSyncConvert(triggerPropertyList.Where(x => x.Coordinate == item.Key).ToList(),
                        triggerGroupList.Where(x => x.Coordinate == item.Key).ToList());
            }

            var transfer = new Dictionary<int, CoordinateData>();

            foreach (var item in coordinateMapping)
            {
                if (!coordinate.TryGetValue(item.Key, out var coord) || !item.Value.HasValue) continue;
                transfer[item.Value.Value] = coord;
            }

            data = importedExtendedData[Guid] = new PluginData { version = 1 };
            data.data["CoordinateData"] = MessagePackSerializer.Serialize(transfer);
        }
    }
}