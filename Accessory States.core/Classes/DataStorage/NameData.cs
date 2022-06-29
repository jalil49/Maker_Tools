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
        public int StateLength => StateNames.Count;

        [IgnoreMember]
        public int Binding = 0;

        /// <summary>
        /// Store for Studio Reference maybe EC
        /// otherwise should be returned to default
        /// </summary>
        public int CurrentState = 0;

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
            if (StateNames == null)
                StateNames = new Dictionary<int, string>();
            if (Name == null)
                Name = "";
        }

        public bool Equals(NameData other)
        {
            return Name == other.Name;
        }

        public void MergeStatesWith(NameData other)
        {
            if (other == null)
                return;
            foreach (var item in other.StateNames)
            {
                if (this.StateNames.ContainsKey(item.Key))
                    continue;

                this.StateNames[item.Key] = item.Value;
            }
        }

        public string GetStateName(int state)
        {
            if (StateNames.TryGetValue(state, out var name))
                return name;
            return StateNames[state] = "State: " + state;
        }

        public List<StateInfo> GetDefaultStates(int slot)
        {
            var states = new List<StateInfo>();
            for (var i = 0; i < StateLength; i++)
            {
                states.Add(new StateInfo() { Binding = Binding, Slot = slot, State = i });
            }
            return states;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Settings.Logger.LogWarning("Namedata Deserialized");
            if (!KKAPI.Studio.StudioAPI.InsideStudio)
                CurrentState = DefaultState;
        }

        public int IncrementCurrentState()
        {
            if (++CurrentState >= StateLength)
                CurrentState = 0;
            return CurrentState;
        }
        public int DecrementCurrentState()
        {
            if (--CurrentState < 0)
                CurrentState = StateLength - 1;
            return CurrentState;
        }
    }
}
