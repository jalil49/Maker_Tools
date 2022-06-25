using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class NameData
    {
        public string Name { get; set; }

        public int DefaultState { get; set; }

        public Dictionary<int, string> StateNames { get; set; }

        [IgnoreMember]
        public int StateLength = 0;

        [IgnoreMember]
        public int Binding = 0;

        [IgnoreMember]
        public int CurrentState = 0;

        private int _modStateLength { get { return StateLength + 2; } }

        public NameData() { NullCheck(); }

        public void CopyData(NameData slotdata) => CopyData(slotdata.Name, slotdata.DefaultState, slotdata.StateNames);

        public void CopyData(string _name, int _defaultstate, Dictionary<int, string> _statenames)
        {
            Name = _name;
            StateNames = _statenames.ToNewDictionary();
            DefaultState = _defaultstate;
        }

        internal void NullCheck()
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

        public string GetStateName(int state)
        {
            if (StateNames.TryGetValue(state, out var name)) return name;
            return "State: " + state;
        }

        public int SetMaxStateLength(int state)
        {
            StateLength = Math.Max(state, StateLength);
            return StateLength;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            CurrentState = DefaultState;
        }

        public int IncrementCurrentState()
        {
            CurrentState = ++CurrentState % _modStateLength;
            return CurrentState;
        }
    }
}
