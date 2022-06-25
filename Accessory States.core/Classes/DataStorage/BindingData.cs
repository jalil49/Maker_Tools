using MessagePack;
using System;
using System.Collections.Generic;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class BindingData
    {
        public NameData NameData { get; set; }
        public List<StateInfo> States { get; set; }

        public BindingData()
        {
            NameData = new NameData();
            States = new List<StateInfo>();
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
                if (item.State != state) continue;
                if (item.ShoeType == shoe || item.ShoeType == 2) return item;
            }
            return null;
        }

        public int GetBinding()
        {
            if (States == null || States.Count == 0) return -1;
            return States[0].Binding;
        }

        public void SetBinding()
        {
            foreach (var item in States)
            {
                item.Binding = NameData.Binding;
                if (NameData != null) NameData.SetMaxStateLength(item.State);
            }
        }

        internal void SetSlot(int slot)
        {
            foreach (var item in States)
            {
                item.Slot = slot;
            }
        }
    }
}
