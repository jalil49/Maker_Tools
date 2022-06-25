using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States.Migration.Version1
{
    [Serializable]
    [MessagePackObject]
    internal class CoordinateDataV1
    {
        [Key("_slotinfo")]
        public Dictionary<int, SlotdataV1> Slotinfo { get; set; }

        [Key("_names")]
        public Dictionary<int, NameDataV1> Names { get; set; }

        [Key("_clothnotdata")]
        public bool[] ClothNotData { get; set; }

        [Key("_forceclothnotupdate")]
        public bool ForceClothNotUpdate = true;

        private int MaxState(int binding)
        {
            if (binding < 9)
            {
                return 3;
            }
            var max = 0;
            var bindinglist = Slotinfo.Values.Where(x => x.Binding == binding);
            foreach (var item in bindinglist)
            {
                item.States.ForEach(x => max = Math.Max(x[1], max));
            }
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
}
