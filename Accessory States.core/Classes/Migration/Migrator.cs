﻿using System;
using System.Collections.Generic;
using System.Linq;
using Accessory_States.Migration.Version1;
using ExtensibleSaveFormat;
using MessagePack;

namespace Accessory_States.Migration
{
    public static class Migrator
    {
        public static void StandardCharaMigrator(ChaControl control, PluginData importeddata)
        {
            if (importeddata.version > 1)
                return;

            var dict = new Dictionary<int, CoordinateDataV1>();
            var coordlength = control.chaFile.coordinate.Length;

            if (importeddata.version == 0)
            {
                for (var i = 0; i < coordlength; i++) dict[i] = new CoordinateDataV1();

                CharaMigrateV0(importeddata, dict, coordlength);
            }

            if (importeddata.version == 1)
                if (importeddata.data.TryGetValue(Constants.CoordinateKey, out var byteData) && byteData != null)
                    dict = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateDataV1>>((byte[])byteData);

            for (var coordnum = 0; coordnum < coordlength; coordnum++)
            {
                if (!dict.TryGetValue(coordnum, out var dataV1))
                    continue;
                CoordinateProcess(control.chaFile.coordinate[coordnum], dataV1);
                if (coordnum == control.fileStatus.coordinateType) CoordinateProcess(control.nowCoordinate, dataV1);
            }

            ;

            ExtendedSave.SetExtendedDataById(control.chaFile, Settings.Guid, new PluginData { version = 2 });
        }

        public static void StandardCoordMigrator(ChaFileCoordinate file, PluginData importeddata)
        {
            if (importeddata.version > 1)
                return;

            var dict = new CoordinateDataV1();

            if (importeddata.version == 0) dict = CoordinateMigrateV0(importeddata);

            if (importeddata.version == 1)
                if (importeddata.data.TryGetValue(Constants.CoordinateKey, out var byteData) && byteData != null)
                    dict = MessagePackSerializer.Deserialize<CoordinateDataV1>((byte[])byteData);

            CoordinateProcess(file, dict);
        }

        private static void CoordinateProcess(ChaFileCoordinate file, CoordinateDataV1 dict)
        {
            var parts = file.accessory.parts;
            var names = Constants.GetNameDataList();

            foreach (var item in dict.Names)
            {
                if (names.Any(x => x.Name == item.Value.Name))
                    continue;
                names.Add(item.Value.ToNewNameData());
            }

            foreach (var item in dict.SlotData)
            {
                if (parts.Length <= item.Key)
                    continue;

                var slotdata = new SlotData
                {
                    parented = item.Value.parented
                };

                if (item.Value.Binding == 8) item.Value.Binding = 7;

                var nameData = names.FirstOrDefault(x => x.binding == item.Value.Binding);
                if (nameData != null) slotdata.bindingDatas.Add(item.Value.ToBindingData(nameData, item.Key));

                parts[item.Key].SetExtendedDataById(Settings.Guid, slotdata.Serialize());
            }

            var coord = new CoordinateData();

            file.accessory.SetExtendedDataById(Settings.Guid, coord.Serialize());

            ExtendedSave.SetExtendedDataById(file, Settings.Guid, new PluginData { version = 2 });
        }

        internal static void CharaMigrateV0(PluginData plugindata, Dictionary<int, CoordinateDataV1> data,
            int coordcount)
        {
            if (plugindata.data.TryGetValue("ACC_Binding_Dictionary", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length && i < coordcount; i++)
                {
                    var sub = temp[i];
                    var slotData = data[i].SlotData;

                    foreach (var element in sub)
                    {
                        int result;
                        if (element.Value < 9)
                            result = element.Value - 1;
                        else
                            result = element.Value;
                        slotData[element.Key] = new SlotdataV1 { Binding = result };
                    }
                }
            }

            if (plugindata.data.TryGetValue("ACC_State_array", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int[]>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length && i < coordcount; i++)
                {
                    var slotData = data[i].SlotData;

                    foreach (var pair in temp[i])
                        if (slotData.TryGetValue(pair.Key, out var slotdata))
                        {
                            var list = slotdata.States = new List<int[]> { pair.Value };
                            if (list[0][0] == 1) list[0][0] = 3;

                            if (list[0][1] == 1) list[0][1] = 3;
                        }
                }
            }

            if (plugindata.data.TryGetValue("ACC_Name_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, string>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length && i < coordcount; i++)
                    foreach (var item in temp[i])
                        data[i].Names[item.Key] = new NameDataV1 { Name = item.Value };
            }

            if (plugindata.data.TryGetValue("ACC_Parented_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, bool>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length && i < coordcount; i++)
                {
                    var slotData = data[i].SlotData;
                    foreach (var item in temp[i])
                    {
                        if (!slotData.TryGetValue(item.Key, out var slotdata))
                            slotdata = slotData[item.Key] = new SlotdataV1();

                        slotdata.parented = item.Value;
                    }
                }
            }
        }

        internal static CoordinateDataV1 CoordinateMigrateV0(PluginData plugindata)
        {
            var data = new CoordinateDataV1();
            if (plugindata.data.TryGetValue("ACC_Binding_Dictionary", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])byteData);

                var slotData = data.SlotData;

                foreach (var element in temp)
                {
                    int result;
                    if (element.Value < 9)
                        result = element.Value - 1;
                    else
                        result = element.Value;
                    temp[element.Key] = result;
                    slotData[element.Key] = new SlotdataV1 { Binding = element.Value };
                }
            }

            if (plugindata.data.TryGetValue("ACC_State_array", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int[]>>((byte[])byteData);
                var slotData = data.SlotData;
                foreach (var item in temp)
                    if (slotData.TryGetValue(item.Key, out var slotdata))
                        slotdata.States = new List<int[]> { item.Value };
            }

            if (plugindata.data.TryGetValue("ACC_Name_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])byteData);
                foreach (var item in temp) data.Names[item.Key] = new NameDataV1 { Name = item.Value };
            }

            if (plugindata.data.TryGetValue("ACC_Parented_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])byteData);
                var slotData = data.SlotData;
                foreach (var item in temp)
                {
                    if (!slotData.TryGetValue(item.Key, out var slotdata))
                        slotdata = slotData[item.Key] = new SlotdataV1();

                    slotdata.parented = item.Value;
                }
            }

            return data;
        }
    }

#if !KK
    public static class ImportMigrator
    {
        public static void StandardCharaMigrator(PluginData importeddata,
            out Dictionary<int, TempMigration> TempDictionary)
        {
            TempDictionary = new Dictionary<int, TempMigration>();
            if (importeddata.version > 1)
                return;

            var dict = new Dictionary<int, CoordinateDataV1>();

            if (importeddata.version == 0) CharaMigrateV0(importeddata, dict);

            if (importeddata.version == 1)
                if (importeddata.data.TryGetValue(Constants.CoordinateKey, out var ByteData) && ByteData != null)
                    dict = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateDataV1>>((byte[])ByteData);

            for (var i = 0; i < dict.Count; i++)
            {
                var keyValue = dict.ElementAt(i);
                TempDictionary[keyValue.Key] = CoordinateProcess(keyValue.Value);
            }

            ;
        }

        private static TempMigration CoordinateProcess(CoordinateDataV1 dict)
        {
            var tempMigration = new TempMigration
            {
                CoordinateData = new CoordinateData { clothingNotData = dict.ClothNotData }
            };

            var names = Constants.GetNameDataList();

            foreach (var item in dict.Names)
            {
                if (names.Any(x => x.Name == item.Value.Name))
                    continue;
                names.Add(item.Value.ToNewNameData());
            }

            foreach (var item in dict.SlotData)
            {
                var slotData = new SlotData
                {
                    parented = item.Value.parented
                };

                if (item.Value.Binding == 8) item.Value.Binding = 7;

                var nameData = names.FirstOrDefault(x => x.binding == item.Value.Binding);
                if (nameData != null) slotData.bindingDatas.Add(item.Value.ToBindingData(nameData, item.Key));

                tempMigration.SlotData[item.Key] = slotData;
            }

            return tempMigration;
        }

        internal static void CharaMigrateV0(PluginData pluginData, Dictionary<int, CoordinateDataV1> allCoordinatedata)
        {
            if (pluginData.data.TryGetValue("ACC_Binding_Dictionary", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var data = TryMigrateV0Data(allCoordinatedata, i);
                    var slotData = data.SlotData;

                    var sub = temp[i];
                    foreach (var element in sub)
                    {
                        int result;
                        if (element.Value < 9)
                            result = element.Value - 1;
                        else
                            result = element.Value;
                        slotData[element.Key] = new SlotdataV1 { Binding = result };
                    }
                }
            }

            if (pluginData.data.TryGetValue("ACC_State_array", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int[]>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var data = TryMigrateV0Data(allCoordinatedata, i);
                    var SlotData = data.SlotData;

                    foreach (var pair in temp[i])
                        if (SlotData.TryGetValue(pair.Key, out var slotdata))
                        {
                            var list = slotdata.States = new List<int[]> { pair.Value };
                            if (list[0][0] == 1) list[0][0] = 3;

                            if (list[0][1] == 1) list[0][1] = 3;
                        }
                }
            }

            if (pluginData.data.TryGetValue("ACC_Name_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, string>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var data = TryMigrateV0Data(allCoordinatedata, i);

                    foreach (var item in temp[i]) data.Names[item.Key] = new NameDataV1 { Name = item.Value };
                }
            }

            if (pluginData.data.TryGetValue("ACC_Parented_Dictionary", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, bool>[]>((byte[])byteData);
                for (var i = 0; i < temp.Length; i++)
                {
                    var data = TryMigrateV0Data(allCoordinatedata, i);
                    var SlotData = data.SlotData;
                    foreach (var item in temp[i])
                    {
                        if (!SlotData.TryGetValue(item.Key, out var slotdata))
                            slotdata = SlotData[item.Key] = new SlotdataV1();

                        slotdata.parented = item.Value;
                    }
                }
            }
        }

        private static CoordinateDataV1 TryMigrateV0Data(Dictionary<int, CoordinateDataV1> data, int num)
        {
            if (!data.TryGetValue(num, out var coordinateDataV1)) data[num] = coordinateDataV1 = new CoordinateDataV1();

            return coordinateDataV1;
        }

        public static void FullAssCardLoad(List<AccStateSync.TriggerProperty> triggers,
            List<AccStateSync.TriggerGroup> groups, out Dictionary<int, TempMigration> migrationDict)
        {
            migrationDict = new Dictionary<int, TempMigration>();
            var length = 0;
            if (groups.Count > 0) length = groups.Max(x => x.Kind);

            if (triggers.Count > 0) length = Math.Max(length, groups.Max(x => x.Kind));

            length++;
            for (var i = 0; i < length; i++)
            {
                var migration = migrationDict[i] = new TempMigration();
                ConvertAssCoordinate(triggers.FindAll(x => x.Coordinate == i), groups.FindAll(x => x.Coordinate == i),
                    migration);
            }
        }

        public static void ConvertAssCoordinate(List<AccStateSync.TriggerProperty> triggers,
            List<AccStateSync.TriggerGroup> groups, TempMigration migration)
        {
            var localNames = Constants.GetNameDataList();
            var max = localNames.Max(x => x.binding) + 1;
            //dictionary<slot, refkind, listoftriggers>
            var slotDict = new Dictionary<int, Dictionary<int, List<AccStateSync.TriggerProperty>>>();
            var groupRelation = new Dictionary<AccStateSync.TriggerGroup, NameData>();
            foreach (var item in groups)
            {
                var newNameData = item.ToNameData();
                var reference =
                    localNames.FirstOrDefault(x => x.binding == newNameData.binding || x.Equals(newNameData, true));
                if (reference != null) newNameData = reference;

                if (newNameData.binding >= Constants.ClothingLength)
                {
                    newNameData.binding = max;
                    max++;
                }

                groupRelation[item] = newNameData;
            }

            foreach (var item in triggers)
            {
                if (!slotDict.TryGetValue(item.Slot, out var subDict))
                    subDict = slotDict[item.Slot] = new Dictionary<int, List<AccStateSync.TriggerProperty>>();

                if (!subDict.TryGetValue(item.RefKind, out var subRefKindList))
                    slotDict[item.RefKind] = new Dictionary<int, List<AccStateSync.TriggerProperty>>();
            }

            foreach (var slotReference in slotDict)
            {
                var slotData = new SlotData();

                foreach (var bindingReference in slotReference.Value)
                {
                    var bindingData = new BindingData
                    {
                        NameData =
                            groupRelation[groups.First(x => x.Kind == bindingReference.Key)]
                    };
                    slotData.bindingDatas.Add(bindingData);
                    foreach (var state in bindingReference.Value) bindingData.States.Add(state.ToStateInfo());

                    bindingData.SetBinding();
                    bindingData.SetSlot(slotReference.Key);
                }

                migration.SlotData[slotReference.Key] = slotData;
            }

            migration.CoordinateData = new CoordinateData { clothingNotData = new[] { false, false, false } };
        }
    }

    [MessagePackObject(true)]
    public class TempMigration
    {
        public CoordinateData CoordinateData { get; set; }
        public Dictionary<int, SlotData> SlotData { get; set; } = new Dictionary<int, SlotData>();
    }
#endif
}