using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States.Migration.Version1
{
    [Serializable]
    [MessagePackObject]
    internal class SlotdataV1
    {
        [Key("_binding")]
        public int Binding { get; set; } = -1;

        [Key("_state")]
        public List<int[]> States { get; set; }

        [Key("_shoetype")]
        public byte Shoetype { get; set; } = 2;

        [Key("_parented")]
        public bool Parented;

        public SlotdataV1() { NullCheck(); }

        private void NullCheck()
        {
            if (States == null) States = new List<int[]>() { new int[] { 0, 3 } };
        }

        public BindingData ToBindingData(NameData nameData, int slot)
        {
            NullCheck();
            var bindingData = new BindingData() { NameData = nameData };
            var max = 0;

            foreach (var state in States)
            {
                if (state == null || state.Length != 2) continue;
                max = Math.Max(max, state[1]);
            }
            max++;

            for (var i = 0; i < max; i++)
            {
                var newState = new StateInfo() { Binding = this.Binding, Priority = 0, ShoeType = this.Shoetype, Slot = slot, State = i };
                newState.Show = States.Any(x => x[0] <= i && i <= x[1]);
                bindingData.States.Add(newState);
            }

            return bindingData;
        }
    }
}
