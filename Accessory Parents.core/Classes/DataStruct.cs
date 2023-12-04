using System;
using System.Collections.Generic;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine.Serialization;

namespace Accessory_Parents
{
    [Serializable]
    [MessagePackObject(true)]
    public class CustomName
    {
        [FormerlySerializedAs("CoordinateCheck")] public string coordinateCheck;

        [FormerlySerializedAs("ChildSlots")] public List<int> childSlots;

        public CustomName(string name, int slot, List<int> childslots)
        {
            Name = name;
            ParentSlot = slot;
            childSlots = childslots ?? new List<int>();
            NullCheck();
        }

        public CustomName(string name, int slot)
        {
            Name = name;
            ParentSlot = slot;
            NullCheck();
        }

        public CustomName(string name)
        {
            Name = name;
            NullCheck();
        }

        public string Name { get; set; }

        public int ParentSlot { get; set; } = -1;

        private void NullCheck()
        {
            if (Name == null) Name = string.Empty;

            if (childSlots == null) childSlots = new List<int>();

            if (coordinateCheck.IsNullOrEmpty()) coordinateCheck = string.Empty;
        }

        public PluginData Serialize()
        {
            if (ParentSlot < 0 || childSlots.Count == 0) return null;

            return new PluginData
            {
                version = 2,
                data = new Dictionary<string, object> { ["AccessoryData"] = MessagePackSerializer.Serialize(this) }
            };
        }
    }
}