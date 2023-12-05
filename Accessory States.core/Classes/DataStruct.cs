using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using MessagePack;
using UnityEngine.Serialization;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject]
    public class CoordinateData
    {
        // ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("ForceClothNotUpdate")] [Key("_forceclothnotupdate")]
        public bool forceClothNotUpdate = true;

        public CoordinateData()
        {
            NullCheck();
        }

        public CoordinateData(CoordinateData copy)
        {
            CopyData(copy);
        }

        // ReSharper disable once StringLiteralTypo
        [Key("_slotinfo")] public Dictionary<int, SlotData> SlotInfo { get; set; }

        [Key("_names")] public Dictionary<int, NameData> Names { get; set; }

        // ReSharper disable once StringLiteralTypo
        [Key("_clothnotdata")] public bool[] ClothNotData { get; set; }

        public void CleanUp()
        {
            var removeList = SlotInfo.Where(x => x.Value.Binding == -1 && !x.Value.parented).Select(x => x.Key)
                .ToList();
            foreach (var item in removeList) SlotInfo.Remove(item);
            removeList = Names.Where(x => SlotInfo.All(y => y.Value.Binding != x.Key)).Select(x => x.Key).ToList();
            foreach (var item in removeList) Names.Remove(item);

            foreach (var item in Names)
            {
                var max = MaxState(item.Key);
                var stateNames = item.Value.StateNames;
                removeList = stateNames.Keys.Where(x => x > max).ToList();
                foreach (var key in removeList) stateNames.Remove(key);
            }
        }

        public void Clear()
        {
            SlotInfo.Clear();
            Names.Clear();
            ClothNotData = new bool[3];
            forceClothNotUpdate = true;
        }

        private void NullCheck()
        {
            if (SlotInfo == null) SlotInfo = new Dictionary<int, SlotData>();
            if (Names == null) Names = new Dictionary<int, NameData>();
            if (ClothNotData == null) ClothNotData = new bool[3];
        }

        public void CopyData(CoordinateData copy)
        {
            CopyData(copy.SlotInfo, copy.Names, copy.ClothNotData, copy.forceClothNotUpdate);
        }

        public void CopyData(Dictionary<int, SlotData> copySlotInfo, Dictionary<int, NameData> copyNames,
            bool[] copyClothNotData, bool copyForceClothNotUpdate)
        {
            SlotInfo = copySlotInfo.ToNewDictionary();
            Names = copyNames.ToNewDictionary();
            ClothNotData = copyClothNotData.ToNewArray(3);
            forceClothNotUpdate = copyForceClothNotUpdate;
            NullCheck();
        }

        public void AccStateSyncConvert(int outfitNum, out List<AccStateSync.TriggerProperty> triggerProperty,
            out List<AccStateSync.TriggerGroup> triggerGroup)
        {
            triggerProperty = new List<AccStateSync.TriggerProperty>();
            triggerGroup = new List<AccStateSync.TriggerGroup>();

            foreach (var slotInfo in SlotInfo)
            {
                var statesList = slotInfo.Value.States;
                var binding = slotInfo.Value.Binding;
                int shoeType = slotInfo.Value.ShoeType;

                if (binding < 0) continue;

                var maxState = MaxState(binding) + 1;

                bool priorityCheck;
                int priority;
                bool visible;

                for (var state = 0; state < maxState; state++) //default conversion
                {
                    visible = ShowState(state, statesList);

                    priorityCheck = binding < 4 && state % 3 == 0;
                    priority = 1;

                    if (priorityCheck) priority = 2;

                    triggerProperty.Add(new AccStateSync.TriggerProperty(outfitNum, slotInfo.Key, binding, state,
                        visible, priority));
                }

                var shoeBinding = binding == 7 || binding == 8;

                if (shoeType != 2 &&
                    !shoeBinding) //bind to indoor or outdoor state only make false priority to not show
                {
                    var otherShoe = shoeType == 0 ? 8 : 7;
                    for (var state = 0; state < maxState; state++)
                        triggerProperty.Add(new AccStateSync.TriggerProperty(outfitNum, slotInfo.Key, otherShoe, state,
                            false, 3));
                }

                if (shoeBinding && shoeType == 2) //bind to a shoe, but other shoe needs same work
                {
                    var otherShoe = binding == 7 ? 8 : 7;
                    for (var state = 0; state < maxState; state++)
                    {
                        visible = ShowState(state, statesList);

                        priorityCheck = binding < 4 && state % 3 == 0;

                        priority = 1;

                        if (priorityCheck) priority = 2;

                        triggerProperty.Add(new AccStateSync.TriggerProperty(outfitNum, slotInfo.Key, otherShoe, state,
                            visible, priority));
                    }
                }

                if (binding > 3) continue;

                switch (binding) //using clothing not values to fill in space
                {
                    case 0:
                        if (ClothNotData[0])
                            triggerProperty.AddRange(ClothingNotConversion(outfitNum, slotInfo.Key, statesList, 1));
                        if (ClothNotData[1])
                            triggerProperty.AddRange(ClothingNotConversion(outfitNum, slotInfo.Key, statesList, 2));
                        break;
                    case 1:
                        if (ClothNotData[0])
                            triggerProperty.AddRange(ClothingNotConversion(outfitNum, slotInfo.Key, statesList, 0));
                        break;
                    case 2:
                        if (ClothNotData[2])
                            triggerProperty.AddRange(ClothingNotConversion(outfitNum, slotInfo.Key, statesList, 3));
                        if (ClothNotData[1])
                            triggerProperty.AddRange(ClothingNotConversion(outfitNum, slotInfo.Key, statesList, 0));
                        break;
                    case 3:
                        if (ClothNotData[2])
                            triggerProperty.AddRange(ClothingNotConversion(outfitNum, slotInfo.Key, statesList, 2));
                        break;
                }
            }

            foreach (var nameData in Names)
            {
                var states = nameData.Value.StateNames;
                for (int i = 0, n = MaxState(nameData.Key) + 1; i < n; i++)
                {
                    if (states.ContainsKey(i)) continue;
                    states[i] = $"State {i}";
                }

                triggerGroup.Add(new AccStateSync.TriggerGroup(outfitNum, nameData.Key, nameData.Value.Name)
                    { States = states.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value) });
            }
        }

        public void AccStateSyncConvert(List<AccStateSync.TriggerProperty> triggerProperty,
            List<AccStateSync.TriggerGroup> triggerGroup)
        {
            var trigListBySlot = new List<List<AccStateSync.TriggerProperty>>();

            foreach (var slot in triggerProperty.Distinct(x => x.Slot).Select(x => x.Slot))
                trigListBySlot.Add(triggerProperty.Where(x => slot == x.Slot).OrderBy(x => x.RefKind)
                    .ThenBy(x => x.RefState).ToList());
            for (int index = 0, trigDictCount = trigListBySlot.Count; index < trigDictCount; index++)
            {
                var list = trigListBySlot[index];

                var slot = list[0].Slot;
                var inner = list.Any(x => x.RefKind == 7 && x.Visible);
                var outer = list.Any(x => x.RefKind == 8 && x.Visible);
                var refKindList = list.Distinct(x => x.RefKind).Select(x => x.RefKind).ToList();
                var priorityDict = new Dictionary<int, int>();
                var stateDict = new Dictionary<int, List<int[]>>();

                var bothShoes = (inner && outer) || (!inner && !outer);

                byte shoeType;
                if (bothShoes)
                    shoeType = 2;
                else if (inner)
                    shoeType = 0;
                else
                    shoeType = 1;

                for (var shoe = 7; shoe <= 8; shoe++)
                    if (refKindList.Contains(shoe))
                        if (refKindList.Any(x => x < shoe))
                            refKindList.Remove(shoe);

                foreach (var kind in refKindList)
                {
                    var visibleList = list.Where(x => x.Visible && x.RefKind == kind).ToList();
                    if (visibleList.Count == 0) continue;
                    priorityDict[kind] = visibleList.Count;
                    var statesList = stateDict[kind] = new List<int[]>();
                    for (int i = 0, n = visibleList.Count; i < n; i++)
                    {
                        var j = i;
                        while (j + 1 < n && visibleList[j + 1].RefState == j + 1) j++;
                        statesList.Add(new[] { i, j });
                        i = j;
                    }
                }

                var selectedRefKind = priorityDict.OrderByDescending(x => x.Value).First().Key;

                SlotInfo[slot] = new SlotData(selectedRefKind, stateDict[selectedRefKind], shoeType, false);
            }

            foreach (var item in triggerGroup) Names[item.Kind] = new NameData(item.Label, item.States);
            NullCheck();
        }

        private List<AccStateSync.TriggerProperty> ClothingNotConversion(int outfitNum, int slot, List<int[]> stateList,
            int clothingKind)
        {
            var triggerProperty = new List<AccStateSync.TriggerProperty>();
            for (var state = 0; state < 4; state++)
            {
                var priorityCheck = state % 3 == 0;
                triggerProperty.Add(new AccStateSync.TriggerProperty(outfitNum, slot, clothingKind, state,
                    priorityCheck && ShowClothNotState(state, stateList), priorityCheck ? 2 : 0));
            }

            return triggerProperty;
        }

        private int MaxState(int binding)
        {
            if (binding < 9) return 3;
            var max = 0;
            var bindingList = SlotInfo.Values.Where(x => x.Binding == binding);
            foreach (var item in bindingList) item.States.ForEach(x => max = Math.Max(x[1], max));
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
    public class SlotData
    {
        [FormerlySerializedAs("Parented")] [Key("_parented")]
        public bool parented;

        public SlotData()
        {
            NullCheck();
        }

        public SlotData(SlotData slotData)
        {
            CopyData(slotData);
        }

        public SlotData(int binding, List<int[]> state, byte shoeType, bool parented)
        {
            CopyData(binding, state, shoeType, parented);
        }

        [Key("_binding")] public int Binding { get; set; } = -1;

        [Key("_state")] public List<int[]> States { get; set; }

        // ReSharper disable once StringLiteralTypo
        [Key("_shoetype")] public byte ShoeType { get; set; } = 2;

        public void CopyData(SlotData slotData)
        {
            CopyData(slotData.Binding, slotData.States, slotData.ShoeType, slotData.parented);
        }

        public void CopyData(int copyBinding, List<int[]> copyState, byte copyShoeType, bool copyParented)
        {
            Binding = copyBinding;
            States = copyState.ToNewList(new[] { 0, 3 });
            ShoeType = copyShoeType;
            parented = copyParented;
        }

        public string Print()
        {
            var print = $"Binding {Binding}\t ShoeType {ShoeType} \n States: ";
            foreach (var item in States) print += $" {item[0]} {item[1]} ";
            return print;
        }

        private void NullCheck()
        {
            if (States == null) States = new List<int[]> { new[] { 0, 3 } };
        }
    }

    [Serializable]
    [MessagePackObject]
    public class NameData
    {
        public NameData()
        {
            NullCheck();
        }

        public NameData(NameData slotData)
        {
            CopyData(slotData);
        }

        public NameData(string name, Dictionary<int, string> stateNames)
        {
            CopyData(name, stateNames);
        }

        [Key("_name")] public string Name { get; set; }

        // ReSharper disable once StringLiteralTypo
        [Key("_statenames")] public Dictionary<int, string> StateNames { get; set; }

        public void CopyData(NameData slotData)
        {
            CopyData(slotData.Name, slotData.StateNames);
        }

        public void CopyData(string name, Dictionary<int, string> stateNames)
        {
            Name = name;
            StateNames = stateNames.ToNewDictionary();
        }

        private void NullCheck()
        {
            if (StateNames == null) StateNames = new Dictionary<int, string>();
            if (Name == null) Name = "";
        }
    }
}