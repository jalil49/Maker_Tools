using KKAPI.Chara;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using static Accessory_States.AccStateSync;
using static ExtensibleSaveFormat.Extensions;
namespace Accessory_States
{
    partial class CharaEvent : CharaCustomFunctionController
    {
        private void ConvertCoordinateToAss(int coord, ChaFileCoordinate coordinate, ref List<TriggerProperty> triggers, ref List<TriggerGroup> groups)
        {
            var localNames = Constants.GetNameDataList();
            var slot = 0;
            foreach (var part in coordinate.accessory.parts)
            {
                GetSlotData(slot, part, ref localNames, coord, ref triggers);
                slot++;
            }

            foreach (var item in localNames)
            {
                groups.Add(new TriggerGroup(item, coord));
            }
        }

        private SlotData GetSlotData(int slot, ChaFileAccessory.PartsInfo part, ref List<NameData> localNames, int coord, ref List<TriggerProperty> triggers)
        {
            if (!part.TryGetExtendedDataById(Settings.GUID, out var extendedData) || extendedData == null || extendedData.version > 2 || !extendedData.data.TryGetValue(Constants.AccessoryKey, out var bytearray) || bytearray == null)
            {
                return null;
            }

            var slotdata = MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
            foreach (var item in slotdata.bindingDatas)
            {
                item.SetSlot(slot);
                var binding = item.GetBinding();

                if (binding < 0)
                {
                    if (item.NameData == null)
                        continue;
                    //re-value binding reference
                    var nameDataReference = localNames.FirstOrDefault(x => x.Equals(item.NameData));

                    if (nameDataReference == null)
                    {
                        localNames.Add(item.NameData);
                        item.NameData.Binding = Constants.ClothingLength + localNames.IndexOf(nameDataReference);
                    }
                    else
                    {
                        nameDataReference.MergeStatesWith(item.NameData);
                        item.NameData = nameDataReference;
                    }
                    foreach (var state in item.States)
                    {
                        triggers.Add(new TriggerProperty(state, coord, slot));
                    }
                    continue;
                }

                if (binding < Constants.ClothingLength)
                {
                    item.NameData = localNames.First(x => x.Binding == binding);
                }

                foreach (var state in item.States)
                {
                    if (state.ShoeType == AssShoePreference || state.ShoeType == 2)
                        triggers.Add(new TriggerProperty(state, coord, slot));
                }
            }
            return slotdata;
        }

        private void FullAssCardSave(ref List<TriggerProperty> triggers, ref List<TriggerGroup> groups)
        {
            var coord = 0;
            foreach (var coordinate in ChaControl.chaFile.coordinate)
            {
                ConvertCoordinateToAss(coord, coordinate, ref triggers, ref groups);
                coord++;
            }
        }

        private static void FullAssCardLoad(ChaFile chaFile, List<TriggerProperty> triggers, List<TriggerGroup> groups)
        {
            for (var i = 0; i < chaFile.coordinate.Length; i++)
            {
                ConvertAssCoordinate(chaFile.coordinate[i], triggers.FindAll(x => x.Coordinate == i), groups.FindAll(x => x.Coordinate == i));
            }
        }

        private static void ConvertAssCoordinate(ChaFileCoordinate coordinate, List<TriggerProperty> triggers, List<TriggerGroup> groups)
        {
            var localNames = Constants.GetNameDataList();
            var max = localNames.Max(x => x.Binding) + 1;
            //dictionary<slot, refkind, listoftriggers>
            var slotDict = new Dictionary<int, Dictionary<int, List<TriggerProperty>>>();
            var groupRelation = new Dictionary<TriggerGroup, NameData>();
            foreach (var item in groups)
            {
                var newNameData = item.ToNameData();
                var reference = localNames.FirstOrDefault(x => x.Binding == newNameData.Binding || x.Equals(newNameData));
                if (reference != null)
                {
                    newNameData = reference;
                }
                if (newNameData.Binding >= Constants.ClothingLength)
                {
                    newNameData.Binding = max;
                    max++;
                }
                groupRelation[item] = newNameData;
            }

            foreach (var item in triggers)
            {
                if (!slotDict.TryGetValue(item.Slot, out var subDict))
                {
                    subDict = slotDict[item.Slot] = new Dictionary<int, List<TriggerProperty>>();
                }

                if (!subDict.TryGetValue(item.RefKind, out var subRefKindList))
                {
                    slotDict[item.RefKind] = new Dictionary<int, List<TriggerProperty>>();
                }
            }

            var parts = coordinate.accessory.parts;
            foreach (var slotReference in slotDict)
            {
                var part = parts[slotReference.Key];

                if (part.TryGetExtendedDataById(Settings.GUID, out var pluginData) && pluginData != null)
                {
                    continue;
                }
                var slotData = new SlotData();

                foreach (var bindingReference in slotReference.Value)
                {
                    if (bindingReference.Key >= parts.Length)
                        continue;


                    var bindingData = new BindingData() { NameData = groupRelation[groups.First(x => x.Kind == bindingReference.Key)] };
                    slotData.bindingDatas.Add(bindingData);
                    foreach (var state in bindingReference.Value)
                    {
                        bindingData.States.Add(state.ToStateInfo());
                    }
                    bindingData.SetBinding();
                    bindingData.SetSlot(slotReference.Key);
                }

                part.SetExtendedDataById(Settings.GUID, slotData.Serialize());
            }
        }
    }
}