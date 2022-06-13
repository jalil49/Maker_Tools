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
        public CoordinateData() { NullCheck(); }

        public CoordinateData(CoordinateData _copy) => CopyData(_copy);

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
        }

        public string Print()
        {
            return "";
        }

        internal void NullCheck()
        {
            if (bindingDatas == null) bindingDatas = new List<BindingData>();
            //if (GroupName == null) GroupName = new NameData();
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
            return Name == other.Name && DefaultState == other.DefaultState;
        }
    }

    [Serializable]
    [MessagePackObject(true)]
    public class BindingData
    {
        public NameData NameData { get; set; }
        public Dictionary<int, StateInfo> States { get; set; }
    }

    [Serializable]
    [MessagePackObject(true)]
    public class StateInfo
    {
        public int priority;
        public bool show;
    }
}
