using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;
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
                        triggers.Add(new TriggerProperty(state, coord));
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
                        triggers.Add(new TriggerProperty(state, coord));
                }
            }
            return slotdata;
        }

        private void AssCardSave(ref List<TriggerProperty> triggers, ref List<TriggerGroup> groups)
        {
            var coord = 0;
            foreach (var coordinate in ChaControl.chaFile.coordinate)
            {
                ConvertCoordinateToAss(coord, coordinate, ref triggers, ref groups);
                coord++;
            }
        }
    }
}