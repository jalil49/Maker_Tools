using MessagePack;
using System;
using System.Collections.Generic;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class NameData : IMessagePackSerializationCallbackReceiver
    {
        public string Name { get; set; }

        public int DefaultState { get; set; }

        public Dictionary<int, string> StateNames { get; set; }

        /// <summary>
        /// Should be Recalculated for non-accessory groups
        /// </summary>
        public int Binding;

        /// <summary>
        /// Store for Studio Reference maybe EC
        /// otherwise should be returned to default
        /// </summary>
        public int CurrentState;

        public bool StopCollision;

        public string CollisionString;

        [IgnoreMember]
        public int StateLength => StateNames.Count;

        [IgnoreMember]
        public HashSet<int> AssociatedSlots = new HashSet<int>();

        public NameData()
        {
            Name = "Default Name";
            DefaultState = 0;
            CurrentState = 0;
            StateNames = new Dictionary<int, string>() { [0] = "State 0", [1] = "State 1" };
            StopCollision = true;
            CollisionString = Guid.NewGuid().ToString("D").ToUpper();
        }

        internal void NullCheck()
        {
            StateNames = StateNames ?? new Dictionary<int, string>();
            Name = Name ?? string.Empty;
            DefaultState = DefaultState < 0 ? 0 : DefaultState;
            CurrentState = CurrentState < 0 ? 0 : CurrentState;
            CollisionString = CollisionString ?? Guid.NewGuid().ToString("D").ToUpper();
        }

        public bool Equals(NameData other, bool collision)
        {
            if(collision)
            {
                if(this.StopCollision != other.StopCollision)
                    return false;

                if(this.StopCollision)
                {
                    return this.CollisionString.Equals(other.CollisionString) && Name.Equals(other.Name);
                }
            }

            return Name.Equals(other.Name);
        }

        public void MergeStatesWith(NameData other)
        {
            if(other == null)
                return;
            foreach(var item in other.StateNames)
            {
                if(StateNames.ContainsKey(item.Key))
                    continue;

                StateNames[item.Key] = item.Value;
            }
        }

        public string GetStateName(int state)
        {
            if(StateNames.TryGetValue(state, out var name))
                return name;
            return StateNames[state] = "State: " + state;
        }

        public List<StateInfo> GetDefaultStates(int slot)
        {
            var states = new List<StateInfo>();
            for(var i = 0; i < StateLength; i++)
            {
                states.Add(new StateInfo() { Binding = Binding, Slot = slot, State = i });
            }

            Settings.Logger.LogWarning($"GetDefaultStates count {states.Count} bind {Binding}");
            return states;
        }

        public int IncrementCurrentState()
        {
            if(++CurrentState >= StateLength)
                CurrentState = 0;
            return CurrentState;
        }

        public int DecrementCurrentState()
        {
            if(--CurrentState < 0)
                CurrentState = Math.Max(0, StateLength - 1);
            return CurrentState;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            NullCheck();
            if(!KKAPI.Studio.StudioAPI.InsideStudio) //Load Last State In Studio
                CurrentState = DefaultState;
        }
    }
}
