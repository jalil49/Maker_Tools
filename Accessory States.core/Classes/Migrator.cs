using ExtensibleSaveFormat;
using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtensibleSaveFormat.Extensions;

namespace Accessory_States
{
    public static class Migrator
    {
        public static void StandardCharaMigrator(ChaControl control, PluginData importeddata)
        {
            if (importeddata.version > 1) return;

            var dict = new Dictionary<int, CoordinateDataV1>();
            var coordlength = control.chaFile.coordinate.Length;

            if (importeddata.version == 0)
            {
                for (var i = 0; i < coordlength; i++)
                {
                    dict[i] = new CoordinateDataV1();
                }

                CharaMigrateV0(importeddata, ref dict, coordlength);
            }

            if (importeddata.version == 1)
            {
                if (importeddata.data.TryGetValue(Constants.CoordinateKey, out var ByteData) && ByteData != null)
                {
                    dict = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateDataV1>>((byte[])ByteData);
                }
            }

            for (var coordnum = 0; coordnum < coordlength; coordnum++)
            {
                if (!dict.TryGetValue(coordnum, out var dataV1)) continue;
                CoordinateProcess(control.chaFile.coordinate[coordnum], dataV1);
                if (coordnum == control.fileStatus.coordinateType)
                {
                    CoordinateProcess(control.nowCoordinate, dataV1);
                }
            };

            ExtendedSave.SetExtendedDataById(control.chaFile, Settings.GUID, new PluginData() { version = 2 });
        }
        public static void StandardCoordMigrator(ChaFileCoordinate file, PluginData importeddata)
        {
            if (importeddata.version > 1) return;

            var dict = new CoordinateDataV1();

            if (importeddata.version == 0)
            {
                dict = CoordinateMigrateV0(importeddata);
            }

            if (importeddata.version == 1)
            {
                if (importeddata.data.TryGetValue(Constants.CoordinateKey, out var ByteData) && ByteData != null)
                {
                    dict = MessagePackSerializer.Deserialize<CoordinateDataV1>((byte[])ByteData);
                }
            }

            CoordinateProcess(file, dict);
        }

        private static void CoordinateProcess(ChaFileCoordinate file, CoordinateDataV1 dict)
        {
            var savedata = new PluginData() { version = 2 };

            var parts = file.accessory.parts;

            foreach (var item in dict.Slotinfo)
            {
                if (parts.Length <= item.Key) continue;

                var tempslot = new SlotData() { Parented = item.Value.Parented, ShoeType = item.Value.Shoetype, States = item.Value.States, Binding = item.Value.Binding };
                if (dict.Names.TryGetValue(item.Value.Binding, out var groupname))
                {
                    tempslot.GroupName = groupname.Name;
                }
                tempslot.NullCheck();
                savedata.data[Constants.AccessoryKey] = tempslot.Serialize();
                parts[item.Key].SetExtendedDataById(Settings.GUID, savedata);
            }
            savedata.data.Clear();

            var coord = new CoordinateData();
            foreach (var item in dict.Names.Values)
            {
                var tempname = new NameData() { Name = item.Name, StateNames = item.Statenames, DefaultState = 0 };
            }

            coord.NullCheck();
            savedata.data[Constants.CoordinateKey] = coord.Serialize();
            file.accessory.SetExtendedDataById(Settings.GUID, savedata);
            savedata.data.Clear();

            ExtendedSave.SetExtendedDataById(file, Settings.GUID, savedata);
        }

        internal static void CharaMigrateV0(PluginData plugindata, ref Dictionary<int, CoordinateDataV1> data, int coordcount)
        {
            if (plugindata.data.TryGetValue("ACC_Binding_Dictionary", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                for (var i = 0; i < temp.Length && i < coordcount; i++)
                {
                    var sub = temp[i];
                    var slotinfo = data[i].Slotinfo;

                    foreach (var element in sub)
                    {
                        int result;
                        if (element.Value < 9)
                            result = element.Value - 1;
                        else
                            result = element.Value;
                        slotinfo[element.Key] = new SlotdataV1() { Binding = result };
                    }
                }
            }

            if (plugindata.data.TryGetValue("ACC_State_array", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int[]>[]>((byte[])ByteData);
                for (var i = 0; i < temp.Length && i < coordcount; i++)
                {
                    var slotinfo = data[i].Slotinfo;

                    foreach (var pair in temp[i])
                    {
                        if (slotinfo.TryGetValue(pair.Key, out var slotdata))
                        {
                            var list = slotdata.States = new List<int[]> { pair.Value };
                            if (list[0][0] == 1)
                            {
                                list[0][0] = 3;
                            }
                            if (list[0][1] == 1)
                            {
                                list[0][1] = 3;
                            }
                        }
                    }
                }
            }

            if (plugindata.data.TryGetValue("ACC_Name_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, string>[]>((byte[])ByteData);
                for (var i = 0; i < temp.Length && i < coordcount; i++)
                {
                    foreach (var item in temp[i])
                    {
                        data[i].Names[item.Key] = new NameDataV1() { Name = item.Value };
                    }
                }
            }

            if (plugindata.data.TryGetValue("ACC_Parented_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, bool>[]>((byte[])ByteData);
                for (var i = 0; i < temp.Length && i < coordcount; i++)
                {
                    var slotinfo = data[i].Slotinfo;
                    foreach (var item in temp[i])
                    {
                        if (!slotinfo.TryGetValue(item.Key, out var slotdata))
                        {
                            slotdata = slotinfo[item.Key] = new SlotdataV1();
                        }
                        slotdata.Parented = item.Value;
                    }
                }
            }
        }

        internal static CoordinateDataV1 CoordinateMigrateV0(PluginData plugindata)
        {
            var data = new CoordinateDataV1();
            if (plugindata.data.TryGetValue("ACC_Binding_Dictionary", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);

                var slotinfo = data.Slotinfo;

                foreach (var element in temp)
                {
                    int result;
                    if (element.Value < 9)
                        result = element.Value - 1;
                    else
                        result = element.Value;
                    temp[element.Key] = result;
                    slotinfo[element.Key] = new SlotdataV1() { Binding = element.Value };
                }
            }
            if (plugindata.data.TryGetValue("ACC_State_array", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int[]>>((byte[])ByteData);
                var slotinfo = data.Slotinfo;
                foreach (var item in temp)
                {
                    if (slotinfo.TryGetValue(item.Key, out var slotdata))
                    {
                        slotdata.States = new List<int[]> { item.Value };
                    }
                }
            }
            if (plugindata.data.TryGetValue("ACC_Name_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])ByteData);
                foreach (var item in temp)
                {
                    data.Names[item.Key] = new NameDataV1() { Name = item.Value };
                }
            }
            if (plugindata.data.TryGetValue("ACC_Parented_Dictionary", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])ByteData);
                var slotinfo = data.Slotinfo;
                foreach (var item in temp)
                {
                    if (!slotinfo.TryGetValue(item.Key, out var slotdata))
                    {
                        slotdata = slotinfo[item.Key] = new SlotdataV1();
                    }
                    slotdata.Parented = item.Value;
                }
            }
            return data;
        }

        [Serializable]
        [MessagePackObject]
        internal class CoordinateDataV1
        {
            [Key("_slotinfo")]
            public Dictionary<int, SlotdataV1> Slotinfo { get; set; }

            [Key("_names")]
            public Dictionary<int, NameDataV1> Names { get; set; }

            [Key("_clothnotdata")]
            public bool[] ClothNotData { get; set; }

            [Key("_forceclothnotupdate")]
            public bool ForceClothNotUpdate = true;

            public CoordinateDataV1() { NullCheck(); }

            public CoordinateDataV1(CoordinateDataV1 _copy) => CopyData(_copy);

            public void CleanUp()
            {
                var removelist = Slotinfo.Where(x => x.Value.Binding == -1 && !x.Value.Parented).Select(x => x.Key).ToList();
                foreach (var item in removelist)
                {
                    Slotinfo.Remove(item);
                }
                removelist = Names.Where(x => !Slotinfo.Any(y => y.Value.Binding == x.Key)).Select(x => x.Key).ToList();
                foreach (var item in removelist)
                {
                    Names.Remove(item);
                }

                foreach (var item in Names)
                {
                    var max = MaxState(item.Key);
                    var statenames = item.Value.Statenames;
                    removelist = statenames.Keys.Where(x => x > max).ToList();
                    foreach (var key in removelist)
                    {
                        statenames.Remove(key);
                    }
                }
            }

            public void Clear()
            {
                Slotinfo.Clear();
                Names.Clear();
                ClothNotData = new bool[3];
                ForceClothNotUpdate = true;
            }

            private void NullCheck()
            {
                if (Slotinfo == null) Slotinfo = new Dictionary<int, SlotdataV1>();
                if (Names == null) Names = new Dictionary<int, NameDataV1>();
                if (ClothNotData == null) ClothNotData = new bool[3];
            }

            public void CopyData(CoordinateDataV1 _copy) => CopyData(_copy.Slotinfo, _copy.Names, _copy.ClothNotData, _copy.ForceClothNotUpdate);

            public void CopyData(Dictionary<int, SlotdataV1> _slotinfo, Dictionary<int, NameDataV1> _names, bool[] _clothnotdata, bool _forceclothnotupdate)
            {
                Slotinfo = _slotinfo.ToNewDictionary();
                Names = _names.ToNewDictionary();
                ClothNotData = _clothnotdata.ToNewArray(3);
                ForceClothNotUpdate = _forceclothnotupdate;
                NullCheck();
            }

            public void AccStateSyncConvert(int outfitnum, out List<AccStateSync.TriggerProperty> TriggerProperty, out List<AccStateSync.TriggerGroup> TriggerGroup)
            {
                TriggerProperty = new List<AccStateSync.TriggerProperty>();
                TriggerGroup = new List<AccStateSync.TriggerGroup>();

                foreach (var slotinfo in Slotinfo)
                {
                    var statelist = slotinfo.Value.States;
                    var binding = slotinfo.Value.Binding;
                    int shoetype = slotinfo.Value.Shoetype;

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

                        TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, binding, state, visible, priority));
                    }

                    var shoebinding = binding == 7 || binding == 8;

                    if (shoetype != 2 && !shoebinding) //binded to indoor or outdoor state only make false priorty to not show
                    {
                        var othershoe = (shoetype == 0) ? 8 : 7;
                        for (var state = 0; state < maxstate; state++)
                        {
                            TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, othershoe, state, false, 3));
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

                            TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, othershoe, state, visible, priority));
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
                                TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 1));
                            }
                            if (ClothNotData[1])
                            {
                                TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 2));
                            }
                            break;
                        case 1:
                            if (ClothNotData[0])
                            {
                                TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 0));
                            }
                            break;
                        case 2:
                            if (ClothNotData[2])
                            {
                                TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 3));
                            }
                            if (ClothNotData[1])
                            {
                                TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 0));
                            }
                            break;
                        case 3:
                            if (ClothNotData[2])
                            {
                                TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 2));
                            }
                            break;
                    }
                }

                foreach (var NameData in Names)
                {
                    var states = NameData.Value.Statenames;
                    for (int i = 0, n = MaxState(NameData.Key) + 1; i < n; i++)
                    {
                        if (states.ContainsKey(i))
                        {
                            continue;
                        }
                        states[i] = $"State {i}";
                    }

                    TriggerGroup.Add(new AccStateSync.TriggerGroup(outfitnum, NameData.Key, NameData.Value.Name) { States = states.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value) });
                }
            }

            public void AccStateSyncConvert(List<AccStateSync.TriggerProperty> TriggerProperty, List<AccStateSync.TriggerGroup> TriggerGroup)
            {
                var triglistbyslot = new List<List<AccStateSync.TriggerProperty>>();

                foreach (var slot in TriggerProperty.Distinct(x => x.Slot).Select(x => x.Slot))
                {
                    triglistbyslot.Add(TriggerProperty.Where(x => slot == x.Slot).OrderBy(x => x.RefKind).ThenBy(x => x.RefState).ToList());
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

                    Slotinfo[slot] = new SlotdataV1(selectedrefkind, statedict[selectedrefkind], shoetype, false);
                }

                foreach (var item in TriggerGroup)
                {
                    Names[item.Kind] = new NameDataV1(item.Label, item.States);
                }
                NullCheck();
            }

            private List<AccStateSync.TriggerProperty> ClothingNotConversion(int outfitnum, int slot, List<int[]> statelist, int clothingkind)
            {
                var TriggerProperty = new List<AccStateSync.TriggerProperty>();
                bool prioritycheck;
                for (var state = 0; state < 4; state++)
                {
                    prioritycheck = state % 3 == 0;
                    TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slot, clothingkind, state, (prioritycheck) ? ShowClothNotState(state, statelist) : false, (prioritycheck) ? 2 : 0));
                }
                return TriggerProperty;
            }

            private int MaxState(int binding)
            {
                if (binding < 9)
                {
                    return 3;
                }
                var max = 0;
                var bindinglist = Slotinfo.Values.Where(x => x.Binding == binding);
                foreach (var item in bindinglist)
                {
                    item.States.ForEach(x => max = Math.Max(x[1], max));
                }
                return max;
            }

            private static bool ShowState(int state, List<int[]> list)
            {
                return list.Any(x => x[0] <= state && state <= x[1]);
            }

            private static bool ShowClothNotState(int state, List<int[]> list)
            {
                return list.Any(x => x[0] <= state && state <= x[1]);
            }
        }

        [Serializable]
        [MessagePackObject]
        internal class SlotdataV1
        {
            [Key("_binding")]
            public int Binding { get; set; } = -1;

            [Key("_state")]
            public List<int[]> States { get; set; }

            [Key("_shoetype")]
            public byte Shoetype { get; set; } = 2;

            [Key("_parented")]
            public bool Parented;

            public SlotdataV1() { NullCheck(); }

            public SlotdataV1(SlotdataV1 slotdata) => CopyData(slotdata);

            public void CopyData(SlotdataV1 slotdata) => CopyData(slotdata.Binding, slotdata.States, slotdata.Shoetype, slotdata.Parented);

            public SlotdataV1(int _binding, List<int[]> _state, byte _shoetype, bool _parented) => CopyData(_binding, _state, _shoetype, _parented);

            public void CopyData(int _binding, List<int[]> _state, byte _shoetype, bool _parented)
            {
                Binding = _binding;
                States = _state.ToNewList(new int[] { 0, 3 });
                Shoetype = _shoetype;
                Parented = _parented;
                //if (States == null) States = new List<int[]>() {  };
            }

            public string Print()
            {
                var print = $"Binding {Binding}\t shoetype {Shoetype} \n States: ";
                foreach (var item in States)
                {
                    print += $" {item[0]} {item[1]} ";
                }
                return print;
            }

            private void NullCheck()
            {
                if (States == null) States = new List<int[]>() { new int[] { 0, 3 } };
            }
        }

        [Serializable]
        [MessagePackObject]
        internal class NameDataV1
        {
            [Key("_name")]
            public string Name { get; set; }

            [Key("_statenames")]
            public Dictionary<int, string> Statenames { get; set; }

            public NameDataV1() { NullCheck(); }

            public NameDataV1(NameDataV1 slotdata) => CopyData(slotdata);

            public void CopyData(NameDataV1 slotdata) => CopyData(slotdata.Name, slotdata.Statenames);

            public NameDataV1(string _name, Dictionary<int, string> _statenames) => CopyData(_name, _statenames);

            public void CopyData(string _name, Dictionary<int, string> _statenames)
            {
                Name = _name;
                Statenames = _statenames.ToNewDictionary();
            }

            private void NullCheck()
            {
                if (Statenames == null) Statenames = new Dictionary<int, string>();
                if (Name == null) Name = "";
            }
        }
    }
}
