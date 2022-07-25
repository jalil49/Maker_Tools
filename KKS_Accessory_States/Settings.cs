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
            var pluginData = ExtendedSave.GetExtendedDataById(file, GUID);
            if(pluginData == null || pluginData.version != -1)
                return;

            if(pluginData.data.TryGetValue("TempMigration", out var byteArray) && byteArray != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, TempMigration>>((byte[])byteArray);

                foreach(var item in temp)
                {
                    var accessory = file.coordinate[item.Key].accessory;
                    accessory.SetExtendedDataById(GUID, item.Value.CoordinateData.Serialize());
                    var parts = accessory.parts;
                    foreach(var slotData in item.Value.SlotData)
                    {
                        if(slotData.Key >= parts.Length)
                            continue;

                        var part = parts[slotData.Key];
                        if(part.type == 120)
                            continue;

                        part.SetExtendedDataById(GUID, slotData.Value.Serialize());
                    }
                }
            }

            ExtendedSave.SetExtendedDataById(file, GUID, new PluginData() { version = Constants.SaveVersion });
        }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData, Dictionary<int, int?> coordinateMapping)
        {
            var attemptASS = false;
            if(!importedExtendedData.TryGetValue(GUID, out var Data) || Data == null)
                attemptASS = true;

            Dictionary<int, TempMigration> DataStore;

            if(!attemptASS)
            {
                var Coordinate = new Dictionary<int, CoordinateDataV1>();
                foreach(var item in coordinateMapping)
                {
                    Coordinate[item.Key] = new CoordinateDataV1();
                }

                if(Data.version >= 0 && Data.version < 2)
                {
                    ImportMigrator.StandardCharaMigrator(Data, out DataStore);
                }
                else
                {
                    return;
                }
            }
            else
            {
                if(!importedExtendedData.TryGetValue("madevil.kk.ass", out var ASSData) || ASSData == null)
                    return;

                var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                if(ASSData.version > 7)
                {
                    Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                    return;
                }
                else if(ASSData.version < 6)
                {
                    AccStateSync.Migration.ConvertCharaPluginData(ASSData, ref TriggerPropertyList, ref TriggerGroupList);
                }
                else
                {
                    if(ASSData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                    {
                        var _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                        if(_tempTriggerProperty?.Count > 0)
                            TriggerPropertyList.AddRange(_tempTriggerProperty);

                        if(ASSData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                        {
                            var _tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                            if(_tempTriggerGroup?.Count > 0)
                            {
                                foreach(var _group in _tempTriggerGroup)
                                {
                                    if(_group.GUID.IsNullOrEmpty())
                                        _group.GUID = Guid.NewGuid().ToString("D").ToUpper();
                                }

                                TriggerGroupList.AddRange(_tempTriggerGroup);
                            }
                        }
                    }
                }

                if(TriggerPropertyList == null)
                    TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if(TriggerGroupList == null)
                    TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                ImportMigrator.FullAssCardLoad(TriggerPropertyList, TriggerGroupList, out DataStore);
            }

            var transfer = new Dictionary<int, TempMigration>();

            foreach(var item in coordinateMapping)
            {
                if(!item.Value.HasValue || !DataStore.TryGetValue(item.Key, out var coord))
                    continue;
                transfer[item.Value.Value] = coord;
            }

            Data = importedExtendedData[GUID] = new PluginData() { version = -1 };
            Data.data["TempMigration"] = MessagePackSerializer.Serialize(transfer);
        }
    }
}