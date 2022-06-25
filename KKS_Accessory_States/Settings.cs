using Accessory_States.Migration;
using Accessory_States.Migration.Version1;
using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Accessory_States
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
            var attemptASS = false;
            var Coordinate = new Dictionary<int, CoordinateDataV1>();
            foreach (var item in coordinateMapping)
            {
                Coordinate[item.Key] = new CoordinateDataV1();
            }

            if (!importedExtendedData.TryGetValue(GUID, out var Data) || Data == null) attemptASS = true;

            if (!attemptASS)
            {
                if (Data.version == 1)
                {
                    if (Data.data.TryGetValue("CoordinateData", out var ByteData) && ByteData != null)
                    {
                        Coordinate = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateDataV1>>((byte[])ByteData);
                    }
                }
                else if (Data.version == 0)
                {
                    Migrator.CharaMigrateV0(Data, Coordinate, coordinateMapping.Count);
                }
                else
                {
                    Logger.LogWarning("New plugin version found on card please update");
                    return;
                }
            }
            else
            {
                //TODO: Reimplement ASS
                if (!importedExtendedData.TryGetValue("madevil.kk.ass", out var ASSData) || ASSData == null) return;

                var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                if (ASSData.version > 6)
                    Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                else if (ASSData.version < 6)
                {
                    AccStateSync.Migration.ConvertCharaPluginData(ASSData, ref TriggerPropertyList, ref TriggerGroupList);
                }
                else
                {
                    if (ASSData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                    {
                        var _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                        if (_tempTriggerProperty?.Count > 0)
                            TriggerPropertyList.AddRange(_tempTriggerProperty);

                        if (ASSData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                        {
                            var _tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                            if (_tempTriggerGroup?.Count > 0)
                            {
                                foreach (var _group in _tempTriggerGroup)
                                {
                                    if (_group.GUID.IsNullOrEmpty())
                                        _group.GUID = Guid.NewGuid().ToString("D").ToUpper();
                                }
                                TriggerGroupList.AddRange(_tempTriggerGroup);
                            }
                        }
                    }
                }

                if (TriggerPropertyList == null) TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if (TriggerGroupList == null) TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                foreach (var item in Coordinate)
                {
                    //item.Value.AccStateSyncConvert(TriggerPropertyList.Where(x => x.Coordinate == item.Key).ToList(), TriggerGroupList.Where(x => x.Coordinate == item.Key).ToList());
                }
            }

            var transfer = new Dictionary<int, CoordinateDataV1>();

            foreach (var item in coordinateMapping)
            {
                if (!Coordinate.TryGetValue(item.Key, out var coord) || !item.Value.HasValue) continue;
                transfer[item.Value.Value] = coord;
            }

            Data = importedExtendedData[GUID] = new PluginData() { version = 1 };
            Data.data["CoordinateData"] = MessagePackSerializer.Serialize(transfer);
        }
    }
}
