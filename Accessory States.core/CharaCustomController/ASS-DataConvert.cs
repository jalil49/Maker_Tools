using ExtensibleSaveFormat;
using KKAPI.Chara;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtensibleSaveFormat.Extensions;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        /*
        //Save/Load ASS Data
        private void ASSCharaSave()
        {
            var _pluginData = new PluginData() { version = 6 };

            AccStateSyncConvert(out var TriggerPropertyList, out var TriggerGroup);

            _pluginData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(TriggerPropertyList));
            _pluginData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(TriggerGroup));
            _pluginData.data.Add("ExternalManipulation", MessagePackSerializer.Serialize("Accessory_States"));

            ExtendedSave.SetExtendedDataById(ChaFileControl, "madevil.kk.ass", (TriggerPropertyList.Count == 0) ? null : _pluginData);
        }

        private void ASSCharaLoad(PluginData pluginData)
        {
            var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
            var TriggerGroupList = new List<AccStateSync.TriggerGroup>();

            if (pluginData.version > 6)
                Settings.Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
            else if (pluginData.version < 6)
            {
                AccStateSync.Migration.ConvertCharaPluginData(pluginData, ref TriggerPropertyList, ref TriggerGroupList);
            }
            else
            {
                if (pluginData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                {
                    var _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                    if (_tempTriggerProperty?.Count > 0)
                        TriggerPropertyList.AddRange(_tempTriggerProperty);

                    if (pluginData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
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

            AccStateSyncConvert(TriggerPropertyList, TriggerGroupList);
        }

        private void ASSCoordSave(ChaFileCoordinate coordinate)
        {
            var _pluginData = new PluginData() { version = 6 };
            var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
            var TriggerGroup = new List<AccStateSync.TriggerGroup>();

            AccStateSyncConvertCoordProcess(coordinate, ref TriggerPropertyList, ref TriggerGroup, -1);

            _pluginData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(TriggerPropertyList));
            _pluginData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(TriggerGroup));
            _pluginData.data.Add("ExternalManipulation", MessagePackSerializer.Serialize("Accessory_States"));

            ExtendedSave.SetExtendedDataById(coordinate, "madevil.kk.ass", (TriggerPropertyList.Count == 0) ? null : _pluginData);
        }

        private void ASSCoordLoad(ChaFileCoordinate coordinate, PluginData pluginData)
        {
            var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
            var TriggerGroupList = new List<AccStateSync.TriggerGroup>();
            if (pluginData.version > 6)
            {
                Settings.Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
            }
            else if (pluginData.version < 6)
            {
                AccStateSync.Migration.ConvertOutfitPluginData((int)CurrentCoordinate.Value, pluginData, ref TriggerPropertyList, ref TriggerGroupList);
            }
            else
            {
                if (pluginData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                {
                    TriggerPropertyList = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);

                    if (pluginData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                    {
                        TriggerGroupList = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                    }
                }
            }

            if (TriggerPropertyList == null) TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
            if (TriggerGroupList == null) TriggerGroupList = new List<AccStateSync.TriggerGroup>();

            AccStateSyncConvertCoordProcess(coordinate, TriggerPropertyList, TriggerGroupList);
        }
        //convert from ASS
        private void AccStateSyncConvert(List<AccStateSync.TriggerProperty> TriggerPropertyList, List<AccStateSync.TriggerGroup> TriggerGroupList)
        {
            for (var coordnum = 0; coordnum < chafile.coordinate.Length; coordnum++)
            {
                var triggers = TriggerPropertyList.Where(x => x.Coordinate == coordnum);
                var groups = TriggerGroupList.Where(x => x.Coordinate == coordnum);
                AccStateSyncConvertCoordProcess(chafile.coordinate[coordnum], triggers, groups);
                if (coordnum == (int)CurrentCoordinate.Value)
                {
                    if (chafile.coordinate[coordnum].accessory.TryGetExtendedDataById(Settings.GUID, out var pluginData))
                    {
                        NowCoordinate.accessory.SetExtendedDataById(Settings.GUID, pluginData);
                    }
                    var slot = 0;
                    foreach (var item in chafile.coordinate[coordnum].accessory.parts)
                    {
                        if (item.TryGetExtendedDataById(Settings.GUID, out pluginData))
                        {
                            PartsArray[slot].SetExtendedDataById(Settings.GUID, pluginData);
                        }
                        slot++;
                    }
                }
            }
        }

        private void AccStateSyncConvertCoordProcess(ChaFileCoordinate file, IEnumerable<AccStateSync.TriggerProperty> triggers, IEnumerable<AccStateSync.TriggerGroup> groups)
        {
            var coordinatedata = new CoordinateData();
            foreach (var item in groups)
            {
                coordinatedata.Names.Add(new NameData() { StateNames = item.States, DefaultState = item.State, Name = item.Label });
            }

            var triglistbyslot = new List<List<AccStateSync.TriggerProperty>>();
            var slotdict = new Dictionary<int, SlotData>();

            foreach (var slot in triggers.Distinct(x => x.Slot).Select(x => x.Slot))
            {
                triglistbyslot.Add(triggers.Where(x => slot == x.Slot).OrderBy(x => x.RefKind).ThenBy(x => x.RefState).ToList());
            }
            for (int index = 0, trigdictcount = triglistbyslot.Count; index < trigdictcount; index++)
            {
                var list = triglistbyslot[index];

                var slot = list[0].Slot;
                var inner = list.Any(x => x.RefKind == 7 && x.Visible);
                var outer = list.Any(x => x.RefKind == 8 && x.Visible);
                var refkindlist = list.Distinct(x => x.RefKind).Select(x => x.RefKind).ToList();
                var PriorityDict = new Dictionary<int, int>();
                var statedict = new Dictionary<int, List<int[]>>();

                var bothshoes = inner && outer || !inner && !outer;

                byte shoetype;
                if (bothshoes)
                {
                    shoetype = 2;
                }
                else if (inner)
                {
                    shoetype = 0;
                }
                else
                {
                    shoetype = 1;
                }

                for (var shoe = 7; shoe <= 8; shoe++)
                {
                    if (refkindlist.Contains(shoe))
                    {
                        if (refkindlist.Any(x => x < shoe))
                        {
                            refkindlist.Remove(shoe);
                        }
                    }
                }

                foreach (var kind in refkindlist)
                {
                    var visiblelist = list.Where(x => x.Visible && x.RefKind == kind).ToList();
                    if (visiblelist.Count == 0) continue;
                    PriorityDict[kind] = visiblelist.Count;
                    var statelist = statedict[kind] = new List<int[]>();
                    for (int i = 0, n = visiblelist.Count; i < n; i++)
                    {
                        var j = i;
                        while (j + 1 < n && visiblelist[j + 1].RefState == j + 1)
                        {
                            j++;
                        }
                        statelist.Add(new int[2] { i, j });
                        i = j;
                    }
                }

                var selectedrefkind = PriorityDict.OrderByDescending(x => x.Value).First().Key;

                var name = "";
                var kindgroup = groups.FirstOrDefault(x => selectedrefkind == x.Kind);
                if (kindgroup != null)
                {
                    name = kindgroup.Label;
                }
                var slotdata = new SlotData() { Binding = selectedrefkind, States = statedict[selectedrefkind], ShoeType = shoetype, Parented = false, GroupName = name };
                file.accessory.parts[slot].SetExtendedDataById(Settings.GUID, slotdata.Serialize());
                slotdict[slot] = slotdata;
            }

            file.accessory.SetExtendedDataById(Settings.GUID, coordinatedata.Serialize(slotdict));
        }

        //convert to ASS
        private void AccStateSyncConvert(out List<AccStateSync.TriggerProperty> TriggerPropertyList, out List<AccStateSync.TriggerGroup> TriggerGroupList)
        {
            TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
            TriggerGroupList = new List<AccStateSync.TriggerGroup>();

            for (var coordnum = 0; coordnum < chafile.coordinate.Length; coordnum++)
            {
                AccStateSyncConvertCoordProcess(chafile.coordinate[coordnum], ref TriggerPropertyList, ref TriggerGroupList, coordnum);
            }
        }

        private void AccStateSyncConvertCoordProcess(ChaFileCoordinate file, ref List<AccStateSync.TriggerProperty> TriggerProperty, ref List<AccStateSync.TriggerGroup> TriggerGroup, int outfitnum)
        {
            var coordinatedata = new CoordinateData();
            if (file.accessory.TryGetExtendedDataById(Settings.GUID, out var pluginData))
            {
                if (pluginData.version == 2)
                {
                    if (pluginData.data.TryGetValue(Constants.CoordinateKey, out var bytearray) && bytearray != null)
                    {
                        coordinatedata = CoordinateData.Deserialize(bytearray);
                    }
                }
                else
                {
                    Settings.Logger.LogMessage("New version of Accessory_States Detected, Please Update");
                    return;
                }
            }
            var partsarray = file.accessory.parts;
            var slotinfo = new Dictionary<int, SlotData>();

            foreach (var slotdata in slotinfo)
            {
                var statelist = slotdata.Value.States;
                var binding = slotdata.Value.Binding;
                int shoetype = slotdata.Value.ShoeType;

                if (binding < 0)
                {
                    continue;
                }

                var maxstate = MaxState(binding) + 1;

                bool prioritycheck;
                int priority;
                bool visible;

                for (var state = 0; state < maxstate; state++) //default conversion
                {
                    visible = ShowState(state, statelist);

                    prioritycheck = binding < 4 && state % 3 == 0;
                    priority = 1;

                    if (prioritycheck) priority = 2;

                    TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotdata.Key, binding, state, visible, priority));
                }

                var shoebinding = binding == 7 || binding == 8;

                if (shoetype != 2 && !shoebinding) //binded to indoor or outdoor state only make false priorty to not show
                {
                    var othershoe = (shoetype == 0) ? 8 : 7;
                    for (var state = 0; state < maxstate; state++)
                    {
                        TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotdata.Key, othershoe, state, false, 3));
                    }
                }

                if (shoebinding && shoetype == 2) //binded to a shoe, but other shoe needs same work
                {
                    var othershoe = (binding == 7) ? 8 : 7;
                    for (var state = 0; state < maxstate; state++)
                    {
                        visible = ShowState(state, statelist);

                        prioritycheck = binding < 4 && state % 3 == 0;

                        priority = 1;

                        if (prioritycheck) priority = 2;

                        TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotdata.Key, othershoe, state, visible, priority));
                    }
                }

                if (binding > 3)
                {
                    continue;
                }

                switch (binding)//using clothing nots to fill in space
                {
                    case 0:
                        if (ClothNotData[0])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotdata.Key, statelist, 1));
                        }
                        if (ClothNotData[1])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotdata.Key, statelist, 2));
                        }
                        break;
                    case 1:
                        if (ClothNotData[0])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotdata.Key, statelist, 0));
                        }
                        break;
                    case 2:
                        if (ClothNotData[2])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotdata.Key, statelist, 3));
                        }
                        if (ClothNotData[1])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotdata.Key, statelist, 0));
                        }
                        break;
                    case 3:
                        if (ClothNotData[2])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotdata.Key, statelist, 2));
                        }
                        break;
                }
            }

            var namecount = Constants.ClothingLength;
            foreach (var NameData in coordinatedata.Names)
            {
                var states = NameData.StateNames;
                for (int i = 0, n = MaxState(namecount, slotinfo) + 1; i < n; i++)
                {
                    if (states.ContainsKey(i))
                    {
                        continue;
                    }
                    states[i] = $"State {i}";
                }

                TriggerGroup.Add(new AccStateSync.TriggerGroup(outfitnum, namecount, NameData.Name) { States = states.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value), Startup = NameData.DefaultState });
                namecount++;
            }
        }

        private List<AccStateSync.TriggerProperty> ClothingNotConversion(int outfitnum, int slot, List<int[]> statelist, int clothingkind)
        {
            var TriggerProperty = new List<AccStateSync.TriggerProperty>();
            bool prioritycheck;
            for (var state = 0; state < 4; state++)
            {
                prioritycheck = state % 3 == 0;
                TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slot, clothingkind, state, (prioritycheck) && ShowClothNotState(state, statelist), (prioritycheck) ? 2 : 0));
            }
            return TriggerProperty;
        }

        private static bool ShowClothNotState(int state, List<int[]> list)
        {
            return list.Any(x => x[0] <= state && state <= x[1]);
        }

        private int MaxState(int binding, Dictionary<int, SlotData> slotinfo)
        {
            if (binding < 9)
            {
                return 3;
            }
            var max = 1;
            foreach (var item in slotinfo.Values.Where(x => x.Binding == binding))
            {
                item.States.ForEach(x => max = Math.Max(x[1], max));
            }
            return max;
        }
        */
    }
}
