using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine.Serialization;

namespace Accessory_States.Migration.Version1
{
    [Serializable]
    [MessagePackObject]
    public class CoordinateDataV1 : IMessagePackSerializationCallbackReceiver
    {
        // ReSharper disable once NotAccessedField.Global
        [FormerlySerializedAs("ForceClothNotUpdate")] [Key("_forceclothnotupdate")] public bool forceClothNotUpdate = true;

        [Key("_slotinfo")] public Dictionary<int, SlotdataV1> SlotData { get; set; } = new Dictionary<int, SlotdataV1>();

        [Key("_names")] public Dictionary<int, NameDataV1> Names { get; set; } = new Dictionary<int, NameDataV1>();

        [Key("_clothnotdata")] public bool[] ClothNotData { get; set; } = new[] { false, false, false };

        public void OnBeforeSerialize()
        {
            CleanUp();
        }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        public void CleanUp()
        {
            var removeList = SlotData.Where(x => x.Value.Binding == -1 && !x.Value.parented).Select(x => x.Key)
                .ToList();
            foreach (var item in removeList) SlotData.Remove(item);

            removeList = Names.Where(x => SlotData.All(y => y.Value.Binding != x.Key)).Select(x => x.Key).ToList();
            foreach (var item in removeList) Names.Remove(item);

            foreach (var item in Names)
            {
                var max = MaxState(item.Key);
                var stateNames = item.Value.Statenames;
                removeList = stateNames.Keys.Where(x => x > max).ToList();
                foreach (var key in removeList) stateNames.Remove(key);
            }
        }

        private void NullCheck()
        {
            SlotData = SlotData ?? new Dictionary<int, SlotdataV1>();
            Names = Names ?? new Dictionary<int, NameDataV1>();
            ClothNotData = ClothNotData ?? new[] { false, false, false };
            if (ClothNotData.Length > 3)
                ClothNotData = new[] { ClothNotData[0], ClothNotData[1], ClothNotData[2] };
        }

        private int MaxState(int binding)
        {
            if (binding < 9) return 3;

            var max = 0;
            var bindingList = SlotData.Values.Where(x => x.Binding == binding);
            foreach (var item in bindingList) item.States.ForEach(x => max = Math.Max(x[1], max));

            return max;
        }
    }
}