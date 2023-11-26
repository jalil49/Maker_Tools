using System;
using System.Collections.Generic;
using ExtensibleSaveFormat;
using MessagePack;

namespace Accessory_Parents
{
    [Serializable]
    [MessagePackObject(true)]
    public class Custom_Name
    {
        public string CoordinateCheck;

        public List<int> ChildSlots;

        public Custom_Name(string _name, int _slot, List<int> _childslots)
        {
            Name = _name;
            ParentSlot = _slot;
            ChildSlots = _childslots ?? new List<int>();
            NullCheck();
        }

        public Custom_Name(string _name, int _slot)
        {
            Name = _name;
            ParentSlot = _slot;
            NullCheck();
        }

        public Custom_Name(string name)
        {
            Name = name;
            NullCheck();
        }

        public string Name { get; set; }

        public int ParentSlot { get; set; } = -1;

        private void NullCheck()
        {
            if (Name == null)
            {
                Name = string.Empty;
            }

            if (ChildSlots == null)
            {
                ChildSlots = new List<int>();
            }

            if (CoordinateCheck.IsNullOrEmpty())
            {
                CoordinateCheck = string.Empty;
            }
        }

        public PluginData Serialize()
        {
            if (ParentSlot < 0 || ChildSlots.Count == 0)
            {
                return null;
            }

            return new PluginData
            {
                version = 2,
                data = new Dictionary<string, object> { ["AccessoryData"] = MessagePackSerializer.Serialize(this) }
            };
        }
    }
}