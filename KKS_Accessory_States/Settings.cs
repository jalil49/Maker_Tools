using Accessory_States.Migration;
using Accessory_States.Migration.Version1;
using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using static ExtensibleSaveFormat.Extensions;

namespace Accessory_States
{
    [BepInProcess("KoikatsuSunshine")]
    public partial class Settings : BaseUnityPlugin
    {
        private void GameUnique()
        {
            ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported;
            ExtendedSave.CardBeingSaved += ExtendedSave_CardBeingSaved;
        }

        private void ExtendedSave_CardBeingSaved(ChaFile file)
        {
            var pluginData = ExtendedSave.GetExtendedDataById(file, Guid);
            if(pluginData == null || pluginData.version != -1)
                return;

            if(pluginData.data.TryGetValue("TempMigration", out var byteArray) && byteArray != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, TempMigration>>((byte[])byteArray);

                foreach(var item in temp)
                {
                    var accessory = file.coordinate[item.Key].accessory;
                    accessory.SetExtendedDataById(Guid, item.Value.CoordinateData.Serialize());
                    var parts = accessory.parts;
                    foreach(var slotData in item.Value.SlotData)
                    {
                        if(slotData.Key >= parts.Length)
                            continue;

                        var part = parts[slotData.Key];
                        if(part.type == 120)
                            continue;

                        part.SetExtendedDataById(Guid, slotData.Value.Serialize());
                    }
                }
            }

            ExtendedSave.SetExtendedDataById(file, Guid, new PluginData() { version = Constants.SaveVersion });
        }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData, Dictionary<int, int?> coordinateMapping)
        {
            var attemptAss = false;
            if(!importedExtendedData.TryGetValue(Guid, out var data) || data == null)
                attemptAss = true;

            Dictionary<int, TempMigration> dataStore;

            if(!attemptAss)
            {
                var coordinate = new Dictionary<int, CoordinateDataV1>();
                foreach(var item in coordinateMapping)
                {
                    coordinate[item.Key] = new CoordinateDataV1();
                }

                if(data.version >= 0 && data.version < 2)
                {
                    ImportMigrator.StandardCharaMigrator(data, out dataStore);
                }
                else
                {
                    return;
                }
            }
            else
            {
                if(!importedExtendedData.TryGetValue("madevil.kk.ass", out var assData) || assData == null)
                    return;

                var triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var triggerGroupList = new List<AccStateSync.TriggerGroup>();

                if(assData.version > 7)
                {
                    Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                    return;
                }
                else if(assData.version < 6)
                {
                    AccStateSync.Migration.ConvertCharaPluginData(assData, ref triggerPropertyList, ref triggerGroupList);
                }
                else
                {
                    if(assData.data.TryGetValue("TriggerPropertyList", out var loadedTriggerProperty) && loadedTriggerProperty != null)
                    {
                        var tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])loadedTriggerProperty);
                        if(tempTriggerProperty?.Count > 0)
                            triggerPropertyList.AddRange(tempTriggerProperty);

                        if(assData.data.TryGetValue("TriggerGroupList", out var loadedTriggerGroup) && loadedTriggerGroup != null)
                        {
                            var tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])loadedTriggerGroup);
                            if(tempTriggerGroup?.Count > 0)
                            {
                                foreach(var group in tempTriggerGroup)
                                {
                                    if(group.Guid.IsNullOrEmpty())
                                        group.Guid = System.Guid.NewGuid().ToString("D").ToUpper();
                                }

                                triggerGroupList.AddRange(tempTriggerGroup);
                            }
                        }
                    }
                }

                if(triggerPropertyList == null)
                    triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if(triggerGroupList == null)
                    triggerGroupList = new List<AccStateSync.TriggerGroup>();

                ImportMigrator.FullAssCardLoad(triggerPropertyList, triggerGroupList, out dataStore);
            }

            var transfer = new Dictionary<int, TempMigration>();

            foreach(var item in coordinateMapping)
            {
                if(!item.Value.HasValue || !dataStore.TryGetValue(item.Key, out var coord))
                    continue;
                transfer[item.Value.Value] = coord;
            }

            data = importedExtendedData[Guid] = new PluginData() { version = -1 };
            data.data["TempMigration"] = MessagePackSerializer.Serialize(transfer);
        }
    }
}