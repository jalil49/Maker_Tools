using System;
using System.Collections.Generic;
using MessagePack;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class BindingData : IMessagePackSerializationCallbackReceiver
    {
        public BindingData()
        {
            NameData = new NameData();
            States = new List<StateInfo>();
        }

        public NameData NameData { get; set; }
        public List<StateInfo> States { get; set; }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        public void Sort()
        {
            States.Sort((x, y) =>
            {
                var result = x.State.CompareTo(y.State);
                if (result != 0)
                    return result;

                result = x.Priority.CompareTo(y.Priority);
                if (result != 0)
                    return result;

                result = x.ShoeType.CompareTo(y.ShoeType);
                if (result != 0)
                    return result;

                return 0;
            });
        }

        public StateInfo GetStateInfo(int state, int shoe)
        {
            foreach (var item in States)
            {
                if (item.State != state)
                    continue;
                if (item.ShoeType == shoe || item.ShoeType == 2)
                    return item;
            }

            return null;
        }

        public int GetBinding()
        {
            if (States == null || States.Count == 0)
                return -1;
            return NameData.binding;
        }

        public void SetBinding()
        {
            foreach (var item in States) item.Binding = NameData.binding;
        }

        internal void SetSlot(int slot)
        {
            foreach (var item in States) item.Slot = slot;
        }

        private void NullCheck()
        {
            NameData = NameData ?? new NameData
                { binding = -1, Name = "Unknown", StateNames = new Dictionary<int, string>() };
            States = States ?? new List<StateInfo>();
        }
    }
}