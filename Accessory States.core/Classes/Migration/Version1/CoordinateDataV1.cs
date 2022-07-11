using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States.Migration.Version1
{
    [Serializable]
    [MessagePackObject]
    public class CoordinateDataV1 : IMessagePackSerializationCallbackReceiver
    {
        [Key("_slotinfo")]
        public Dictionary<int, SlotdataV1> Slotinfo { get; set; }

        [Key("_names")]
        public Dictionary<int, NameDataV1> Names { get; set; }

        [Key("_clothnotdata")]
        public bool[] ClothNotData { get; set; }

        [Key("_forceclothnotupdate")]
        public bool ForceClothNotUpdate = true;

        public CoordinateDataV1()
        {
            Slotinfo = new Dictionary<int, SlotdataV1>();
            Names = new Dictionary<int, NameDataV1>();
            ClothNotData = new bool[3] { false, false, false };
            ForceClothNotUpdate = true;
        }

        public void CleanUp()
        {
            var removelist = Slotinfo.Where(x => x.Value.Binding == -1 && !x.Value.Parented).Select(x => x.Key).ToList();
            foreach (var item in removelist)
            {
                Slotinfo.Remove(item);
            }
            removelist = Names.Where(x => !Slotinfo.Any(y => y.Value.Binding == x.Key)).Select(x => x.Key).ToList();
            foreach (var item in removelist)
            {
                Names.Remove(item);
            }

            foreach (var item in Names)
            {
                var max = MaxState(item.Key);
                var statenames = item.Value.Statenames;
                removelist = statenames.Keys.Where(x => x > max).ToList();
                foreach (var key in removelist)
                {
                    statenames.Remove(key);
                }
            }
        }

        private void NullCheck()
        {
            Slotinfo = Slotinfo ?? new Dictionary<int, SlotdataV1>();
            Names = Names ?? new Dictionary<int, NameDataV1>();
            ClothNotData = ClothNotData ?? new bool[3] { false, false, false };
            if (ClothNotData.Length > 3)
            {
                ClothNotData = new bool[3] { ClothNotData[0], ClothNotData[1], ClothNotData[2] };
            }
        }

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

        public void OnBeforeSerialize() { CleanUp(); }

        public void OnAfterDeserialize() { NullCheck(); }
    }
}
