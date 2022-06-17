using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class CoordinateData
    {
        public CoordinateData() { NullCheck(); }

        public CoordinateData(CoordinateData _copy) => CopyData(_copy);


        public void Clear()
        {
        }

        internal void NullCheck()
        {
        }

        public void CopyData(CoordinateData _copy) => CopyData();

        public void CopyData()
        {
            NullCheck();
        }

        public byte[] Serialize() => MessagePackSerializer.Serialize(this);
    }

    [Serializable]
    [MessagePackObject(true)]
    public class SlotData
    {
        public List<BindingData> bindingDatas;
        public byte ShoeType { get; set; } = 2;

        public bool Parented;

        public SlotData() => NullCheck();

        public SlotData(SlotData slotdata) => CopyData(slotdata);

        public void CopyData(SlotData slotdata) => CopyData(slotdata.bindingDatas, slotdata.ShoeType, slotdata.Parented);

        public SlotData(List<BindingData> bindingDatas, byte ShoeType, bool Parented) => CopyData(bindingDatas, ShoeType, Parented);

        public void CopyData(List<BindingData> bindingDatas, byte _shoetype, bool _parented)
        {
            this.bindingDatas = bindingDatas;
            ShoeType = _shoetype;
            Parented = _parented;
            NullCheck();
        }

        public string Print()
        {
            return "";
        }

        internal void NullCheck()
        {
            if (bindingDatas == null) bindingDatas = new List<BindingData>();
        }

        public bool ShouldSave()
        {
            if (Parented) return true;
            if (ShoeType != 2) return true;
            if (bindingDatas != null && bindingDatas.Count > 0) return true;
            return false;
        }

        public int MaxStateByBinding(int binding)
        {
            var slotdata = bindingDatas.FirstOrDefault(x => x.Binding == binding);
            if (slotdata == null || slotdata.States.Count == 0) return 0;
            return slotdata.States.Max(x => x.Key) + 1;
        }

        public List<BindingData> GetByBinding(int binding)
        {
            var result = new List<BindingData>();
            foreach (var item in bindingDatas)
            {
                if (item.Binding == binding) result.Add(item);
            }
            return result;
        }

        public bool ShouldShow(int binding, int state, byte[] clothesState, Dictionary<int, int> CustomStateDict)
        {

            var copyDict = new Dictionary<int, int>(CustomStateDict);
            for (var i = 0; i < clothesState.Length; i++)
            {
                copyDict[i] = clothesState[i];
            }

#if KK || KKS
            copyDict[clothesState.Length] = copyDict[clothesState.Length - 1];
#endif
            foreach (var item in bindingDatas)
            {
                if (item.Binding != binding) continue;
                if (item.NameData == null) continue;
                if (!item.States.TryGetValue(state, out var stateInfo)) continue;
                foreach (var restr in stateInfo.restrictions)
                {
                    if (!copyDict.TryGetValue(restr.binding, out var currentState) && currentState != restr.state) continue;

                }
            }
            return true;
        }

        public byte[] Serialize() => MessagePackSerializer.Serialize(this);
    }

    [Serializable]
    [MessagePackObject(true)]
    public class NameData : IEquatable<NameData>
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

        public bool Equals(NameData other)
        {
            return Name == other.Name;
        }

        public void MergeStatesWith(NameData other)
        {
            if (other == null) return;
            foreach (var item in other.StateNames)
            {
                if (this.StateNames.ContainsKey(item.Key)) continue;

                this.StateNames[item.Key] = item.Value;
            }
        }
    }

    [Serializable]
    [MessagePackObject(true)]
    public class BindingData
    {
        // public int Binding { get; set; }
        public NameData NameData { get; set; }
        public List<StateInfo> States { get; set; }

        public BindingData()
        {
            Shoes = 0;
            NameData = new NameData();
            States = new List<StateInfo>();
        }

        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (Binding >= Constants.ClothingLength)
                Binding = -1;

            States.Sort((x, y) => x.State.CompareTo(y.State));
        }

        public void Sort()
        {
            States.Sort((x, y) =>
            {

                var result = x.Binding.CompareTo(y.Binding);
                if (result != 0)
                    return result;

                result = x.State.CompareTo(y.State);
                if (result != 0)
                    return result;

                result = x.ShoeType.CompareTo(y.ShoeType);
                if (result != 0)
                    return result;

                return 0;
            });
        }
    }

    [Serializable]
    [MessagePackObject(true)]
    public class StateInfo
    {
        public int Binding { get; set; }
        public int State { get; set; }
        public int Priority { get; set; }
        public byte ShoeType { get; set; }
        public bool Show { get; set; }

        public StateInfo()
        {
            Binding = -1;
            State = -1;
            Priority = 0;
            ShoeType = 2;
            Show = true;
        }

        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {

            if (Binding >= Constants.ClothingLength)
                Binding = -1;
        }
    }
}
