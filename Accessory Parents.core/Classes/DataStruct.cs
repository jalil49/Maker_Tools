using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Accessory_Parents
{
    [Serializable]
    [MessagePackObject(true)]
    public class Custom_Name
    {
        public string CoordinateCheck;

        public string Name { get; set; }

        public int ParentSlot { get; set; } = -1;

        public List<int> ChildSlots;

        public Custom_Name(string _name, int _slot, List<int> _childslots)
        {
            Name = _name;
            ParentSlot = _slot;
            ChildSlots = _childslots.ToNewList();
            NullCheck();
        }

        public Custom_Name(string _name, int _slot)
        {
            Name = _name;
            ParentSlot = _slot;
            NullCheck();
        }

        public Custom_Name(string _name)
        {
            Name = _name;
            NullCheck();
        }
        private void NullCheck()
        {
            if (Name == null) Name = "";
            if (ChildSlots == null) ChildSlots = new List<int>();
            if (CoordinateCheck.IsNullOrEmpty()) CoordinateCheck = "";
        }

        public ExtensibleSaveFormat.PluginData Serialize()
        {
            if (ParentSlot < 0 || ChildSlots.Count == 0)
            {
                return null;
            }
            return new ExtensibleSaveFormat.PluginData() { version = 2, data = new Dictionary<string, object>() { ["AccessoryData"] = MessagePackSerializer.Serialize(this) } };
        }
    }
}
