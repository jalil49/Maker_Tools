using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class CoordinateData
    {
        public List<NameData> Names { get; set; }
        public bool[] ClothNotData { get; set; }

        public bool ForceClothNotUpdate = true;

        public CoordinateData() { NullCheck(); }

        public CoordinateData(CoordinateData _copy) => CopyData(_copy);

        public CoordinateData(List<NameData> Names, bool[] ClothNotData, bool ForceClothNotUpdate) => CopyData(Names, ClothNotData, ForceClothNotUpdate);

        public void CleanUp(Dictionary<int, SlotData> slotinfo)
        {
            for (var i = Names.Count - 1; i > 0; i--)
            {
                if (slotinfo.Any(x => x.Value.GroupName == Names[i].Name))
                {
                    continue;
                }
                Names.RemoveAt(i);
            }
        }

        public void Clear()
        {
            Names.Clear();
            ClothNotData = new bool[3];
            ForceClothNotUpdate = true;
        }

        internal void NullCheck()
        {
            if (Names == null) Names = new List<NameData>();
            if (ClothNotData == null) ClothNotData = new bool[3];
        }

        public void CopyData(CoordinateData _copy) => CopyData(_copy.Names, _copy.ClothNotData, _copy.ForceClothNotUpdate);

        public void CopyData(List<NameData> _names, bool[] _clothnotdata, bool _forceclothnotupdate)
        {
            Names = _names.ToNewList();
            ClothNotData = _clothnotdata.ToNewArray(3);
            ForceClothNotUpdate = _forceclothnotupdate;
            NullCheck();
        }

        public byte[] Serialize() => MessagePackSerializer.Serialize(this);

        //public void Accstatesyncconvert(int outfitnum, out List<AccStateSync.TriggerProperty> TriggerProperty, out List<AccStateSync.TriggerGroup> TriggerGroup)
        //{
        //    TriggerProperty = new List<AccStateSync.TriggerProperty>();
        //    TriggerGroup = new List<AccStateSync.TriggerGroup>();

        //    foreach (var slotinfo in Slotinfo)
        //    {
        //        var statelist = slotinfo.Value.States;
        //        var binding = slotinfo.Value.Binding;
        //        int shoetype = slotinfo.Value.Shoetype;

        //        if (binding < 0)
        //        {
        //            continue;
        //        }

        //        var maxstate = MaxState(binding) + 1;

        //        bool prioritycheck;
        //        int priority;
        //        bool visible;

        //        for (var state = 0; state < maxstate; state++) //default conversion
        //        {
        //            visible = ShowState(state, statelist);

        //            prioritycheck = binding < 4 && state % 3 == 0;
        //            priority = 1;

        //            if (prioritycheck) priority = 2;

        //            TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, binding, state, visible, priority));
        //        }

        //        var shoebinding = binding == 7 || binding == 8;

        //        if (shoetype != 2 && !shoebinding) //binded to indoor or outdoor state only make false priorty to not show
        //        {
        //            var othershoe = (shoetype == 0) ? 8 : 7;
        //            for (var state = 0; state < maxstate; state++)
        //            {
        //                TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, othershoe, state, false, 3));
        //            }
        //        }

        //        if (shoebinding && shoetype == 2) //binded to a shoe, but other shoe needs same work
        //        {
        //            var othershoe = (binding == 7) ? 8 : 7;
        //            for (var state = 0; state < maxstate; state++)
        //            {
        //                visible = ShowState(state, statelist);

        //                prioritycheck = binding < 4 && state % 3 == 0;

        //                priority = 1;

        //                if (prioritycheck) priority = 2;

        //                TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, othershoe, state, visible, priority));
        //            }
        //        }

        //        if (binding > 3)
        //        {
        //            continue;
        //        }

        //        switch (binding)//using clothing nots to fill in space
        //        {
        //            case 0:
        //                if (ClothNotData[0])
        //                {
        //                    TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 1));
        //                }
        //                if (ClothNotData[1])
        //                {
        //                    TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 2));
        //                }
        //                break;
        //            case 1:
        //                if (ClothNotData[0])
        //                {
        //                    TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 0));
        //                }
        //                break;
        //            case 2:
        //                if (ClothNotData[2])
        //                {
        //                    TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 3));
        //                }
        //                if (ClothNotData[1])
        //                {
        //                    TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 0));
        //                }
        //                break;
        //            case 3:
        //                if (ClothNotData[2])
        //                {
        //                    TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 2));
        //                }
        //                break;
        //        }
        //    }

        //    foreach (var NameData in Names)
        //    {
        //        var states = NameData.Value.StateNames;
        //        for (int i = 0, n = MaxState(NameData.Key) + 1; i < n; i++)
        //        {
        //            if (states.ContainsKey(i))
        //            {
        //                continue;
        //            }
        //            states[i] = $"State {i}";
        //        }

        //        TriggerGroup.Add(new AccStateSync.TriggerGroup(outfitnum, NameData.Key, NameData.Value.Name) { States = states.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value) });
        //    }
        //}

        //public void Accstatesyncconvert(List<AccStateSync.TriggerProperty> TriggerProperty, List<AccStateSync.TriggerGroup> TriggerGroup)
        //{
        //    var triglistbyslot = new List<List<AccStateSync.TriggerProperty>>();

        //    foreach (var slot in TriggerProperty.Distinct(x => x.Slot).Select(x => x.Slot))
        //    {
        //        triglistbyslot.Add(TriggerProperty.Where(x => slot == x.Slot).OrderBy(x => x.RefKind).ThenBy(x => x.RefState).ToList());
        //    }
        //    for (int index = 0, trigdictcount = triglistbyslot.Count; index < trigdictcount; index++)
        //    {
        //        var list = triglistbyslot[index];

        //        var slot = list[0].Slot;
        //        var inner = list.Any(x => x.RefKind == 7 && x.Visible);
        //        var outer = list.Any(x => x.RefKind == 8 && x.Visible);
        //        var refkindlist = list.Distinct(x => x.RefKind).Select(x => x.RefKind).ToList();
        //        var PriorityDict = new Dictionary<int, int>();
        //        var statedict = new Dictionary<int, List<int[]>>();

        //        var bothshoes = inner && outer || !inner && !outer;

        //        byte shoetype;
        //        if (bothshoes)
        //        {
        //            shoetype = 2;
        //        }
        //        else if (inner)
        //        {
        //            shoetype = 0;
        //        }
        //        else
        //        {
        //            shoetype = 1;
        //        }

        //        for (var shoe = 7; shoe <= 8; shoe++)
        //        {
        //            if (refkindlist.Contains(shoe))
        //            {
        //                if (refkindlist.Any(x => x < shoe))
        //                {
        //                    refkindlist.Remove(shoe);
        //                }
        //            }
        //        }

        //        foreach (var kind in refkindlist)
        //        {
        //            var visiblelist = list.Where(x => x.Visible && x.RefKind == kind).ToList();
        //            if (visiblelist.Count == 0) continue;
        //            PriorityDict[kind] = visiblelist.Count;
        //            var statelist = statedict[kind] = new List<int[]>();
        //            for (int i = 0, n = visiblelist.Count; i < n; i++)
        //            {
        //                var j = i;
        //                while (j + 1 < n && visiblelist[j + 1].RefState == j + 1)
        //                {
        //                    j++;
        //                }
        //                statelist.Add(new int[2] { i, j });
        //                i = j;
        //            }
        //        }

        //        var selectedrefkind = PriorityDict.OrderByDescending(x => x.Value).First().Key;

        //        Slotinfo[slot] = new Slotdata(selectedrefkind, statedict[selectedrefkind], shoetype, false);
        //    }

        //    foreach (var item in TriggerGroup)
        //    {
        //        Names[item.Kind] = new NameData(item.Label, item.States);
        //    }
        //    NullCheck();
        //}

        //private List<AccStateSync.TriggerProperty> ClothingNotConversion(int outfitnum, int slot, List<int[]> statelist, int clothingkind)
        //{
        //    var TriggerProperty = new List<AccStateSync.TriggerProperty>();
        //    bool prioritycheck;
        //    for (var state = 0; state < 4; state++)
        //    {
        //        prioritycheck = state % 3 == 0;
        //        TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slot, clothingkind, state, (prioritycheck) ? ShowClothNotState(state, statelist) : false, (prioritycheck) ? 2 : 0));
        //    }
        //    return TriggerProperty;
        //}

        //private int MaxState(int binding)
        //{
        //    if (binding < 9)
        //    {
        //        return 3;
        //    }
        //    var max = 0;
        //    var bindinglist = Slotinfo.Values.Where(x => x.Binding == binding);
        //    foreach (var item in bindinglist)
        //    {
        //        item.States.ForEach(x => max = Math.Max(x[1], max));
        //    }
        //    return max;
        //}

        //private static bool ShowState(int state, List<int[]> list)
        //{
        //    return list.Any(x => x[0] <= state && state <= x[1]);
        //}

        //private static bool ShowClothNotState(int state, List<int[]> list)
        //{
        //    return list.Any(x => x[0] <= state && state <= x[1]);
        //}
    }

    [Serializable]
    [MessagePackObject(true)]
    public class SlotData
    {
        public string GroupName { get; set; }

        public int Binding { get; set; } = -1;

        public List<int[]> States { get; set; }

        public byte ShoeType { get; set; } = 2;

        public bool Parented;

        public SlotData() => NullCheck();

        public SlotData(SlotData slotdata) => CopyData(slotdata);

        public void CopyData(SlotData slotdata) => CopyData(slotdata.GroupName, slotdata.Binding, slotdata.States, slotdata.ShoeType, slotdata.Parented);

        public SlotData(string GroupName, int Binding, List<int[]> States, byte ShoeType, bool Parented) => CopyData(GroupName, Binding, States, ShoeType, Parented);

        public void CopyData(string _groupname, int _binding, List<int[]> _state, byte _shoetype, bool _parented)
        {
            GroupName = _groupname;
            Binding = _binding;
            States = _state.ToNewList(new int[] { 0, 3 });
            ShoeType = _shoetype;
            Parented = _parented;
        }

        public string Print()
        {
            var print = $"Binding {Binding}\t shoetype {ShoeType} \n States: ";
            foreach (var item in States)
            {
                print += $" {item[0]} {item[1]} ";
            }
            return print;
        }

        internal void NullCheck()
        {
            if (States == null) States = new List<int[]>() { new int[] { 0, 3 } };
            if (GroupName == null) GroupName = "";
        }

        public byte[] Serialize() => MessagePackSerializer.Serialize(this);
    }

    [Serializable]
    [MessagePackObject(true)]
    public class NameData
    {
        public string Name { get; set; }

        public int DefaultState { get; set; }

        public Dictionary<int, string> StateNames { get; set; }

        public NameData() { NullCheck(); }

        public NameData(NameData slotdata) => CopyData(slotdata);

        public void CopyData(NameData slotdata) => CopyData(slotdata.Name, slotdata.DefaultState, slotdata.StateNames);

        public NameData(string Name, int DefaultState, Dictionary<int, string> StateNames) => CopyData(Name, DefaultState, StateNames);

        public void CopyData(string _name, int _defaultstate, Dictionary<int, string> _statenames)
        {
            Name = _name;
            StateNames = _statenames.ToNewDictionary();
            DefaultState = _defaultstate;
        }

        private void NullCheck()
        {
            if (StateNames == null) StateNames = new Dictionary<int, string>();
            if (Name == null) Name = "";
        }
    }
}
