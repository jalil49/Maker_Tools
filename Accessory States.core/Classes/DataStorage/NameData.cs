using System;
using System.Collections.Generic;
using KKAPI.Studio;
using MessagePack;
using UnityEngine.Serialization;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class NameData : IMessagePackSerializationCallbackReceiver
    {
        /// <summary>
        ///     Should be Recalculated for non-accessory groups
        /// </summary>
        [FormerlySerializedAs("Binding")] public int binding;

        /// <summary>
        ///     Store for Studio Reference maybe EC
        ///     otherwise should be returned to default
        /// </summary>
        [FormerlySerializedAs("CurrentState")] public int currentState;

        [FormerlySerializedAs("StopCollision")] public bool stopCollision;

        [FormerlySerializedAs("CollisionString")] public string collisionString;

        [IgnoreMember] public HashSet<int> AssociatedSlots = new HashSet<int>();

        public NameData()
        {
            Name = "Default Name";
            DefaultState = 0;
            currentState = 0;
            StateNames = new Dictionary<int, string> { [0] = "State 0", [1] = "State 1" };
            stopCollision = true;
            collisionString = Guid.NewGuid().ToString("D").ToUpper();
        }

        public string Name { get; set; }

        public int DefaultState { get; set; }

        public Dictionary<int, string> StateNames { get; set; }

        [IgnoreMember] public int StateLength => StateNames.Count;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            NullCheck();
            if (!StudioAPI.InsideStudio) //Load Last State In Studio
                currentState = DefaultState;
        }

        internal void NullCheck()
        {
            StateNames = StateNames ?? new Dictionary<int, string>();
            Name = Name ?? string.Empty;
            DefaultState = DefaultState < 0 ? 0 : DefaultState;
            currentState = currentState < 0 ? 0 : currentState;
            collisionString = collisionString ?? Guid.NewGuid().ToString("D").ToUpper();
        }

        public bool Equals(NameData other, bool collision)
        {
            if (collision)
            {
                if (stopCollision != other.stopCollision)
                    return false;

                if (stopCollision) return collisionString.Equals(other.collisionString) && Name.Equals(other.Name);
            }

            return Name.Equals(other.Name);
        }

        public void MergeStatesWith(NameData other)
        {
            if (other == null)
                return;
            foreach (var item in other.StateNames)
            {
                if (StateNames.ContainsKey(item.Key))
                    continue;

                StateNames[item.Key] = item.Value;
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
                states.Add(new StateInfo { Binding = binding, Slot = slot, State = i });

            Settings.Logger.LogWarning($"GetDefaultStates count {states.Count} bind {binding}");
            return states;
        }

        public int IncrementCurrentState()
        {
            if (++currentState >= StateLength)
                currentState = 0;
            return currentState;
        }

        public int DecrementCurrentState()
        {
            if (--currentState < 0)
                currentState = Math.Max(0, StateLength - 1);
            return currentState;
        }
    }
}