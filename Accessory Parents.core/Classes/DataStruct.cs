using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Accessory_Parents
{
    [Serializable]
    [MessagePackObject]
    class CoordinateData
    {
        [Key("_theme_names")]
        public List<Custom_Name> Parent_Groups = new List<Custom_Name>();

        [Key("_relative_data")]
        public Dictionary<int, Vector3[]> Relative_Data = new Dictionary<int, Vector3[]>();

        [IgnoreMember]
        internal Dictionary<int, int> Child = new Dictionary<int, int>();

        [IgnoreMember]
        internal Dictionary<int, List<int>> RelatedNames = new Dictionary<int, List<int>>();

        [IgnoreMember]
        internal Dictionary<int, string> Old_Parent = new Dictionary<int, string>();

        public CoordinateData() { }

        public CoordinateData(CoordinateData _copy) => CopyData(_copy);

        public CoordinateData(List<Custom_Name> _theme_names, Dictionary<int, Vector3[]> _relative_data)
        {
            Parent_Groups = _theme_names;
            Relative_Data = _relative_data;
        }

        public void CleanUp()
        {
            NullCheck();
            Parent_Groups.RemoveAll(x => x.ParentSlot == -1 || x.ChildSlots.Count == 0);
            var relativeclean = Relative_Data.Keys.Where(x => !Parent_Groups.Any(y => y.ChildSlots.Contains(x) || y.ParentSlot == x)).ToList();
            foreach (var item in relativeclean)
            {
                Relative_Data.Remove(item);
            }
        }

        public void Clear()
        {
            NullCheck();
            Parent_Groups.Clear();
            Relative_Data.Clear();
            Child.Clear();
            RelatedNames.Clear();
            Old_Parent.Clear();
        }

        private void NullCheck()
        {
            if (Parent_Groups == null) Parent_Groups = new List<Custom_Name>();
            if (Relative_Data == null) Relative_Data = new Dictionary<int, Vector3[]>();
            if (RelatedNames == null) RelatedNames = new Dictionary<int, List<int>>();
            if (Child == null) Child = new Dictionary<int, int>();
            if (Old_Parent == null) Old_Parent = new Dictionary<int, string>();
        }

        public void CopyData(CoordinateData _copy) => CopyData(_copy.Parent_Groups, _copy.Relative_Data, _copy.Child, _copy.RelatedNames, _copy.Old_Parent);

        public void CopyData(List<Custom_Name> _theme_names, Dictionary<int, Vector3[]> _relative_data, Dictionary<int, int> _child, Dictionary<int, List<int>> _related, Dictionary<int, string> _oldparent)
        {
            Parent_Groups = new List<Custom_Name>(_theme_names);
            Relative_Data = new Dictionary<int, Vector3[]>(_relative_data);
            Child = new Dictionary<int, int>(_child);
            RelatedNames = new Dictionary<int, List<int>>(_related);
            Old_Parent = new Dictionary<int, string>(_oldparent);
        }
    }

    [Serializable]
    [MessagePackObject]
    class Custom_Name
    {
        [Key("_name")]
        public string Name { get; set; }

        [Key("_slot")]
        public int ParentSlot { get; set; } = -1;

        [Key("_childslots")]
        public List<int> ChildSlots = new List<int>();

        public Custom_Name(string _name, int _slot, List<int> _childslots)
        {
            Name = _name;
            ParentSlot = _slot;
            ChildSlots = _childslots;
        }

        public Custom_Name(string _name, int _slot)
        {
            Name = _name;
            ParentSlot = _slot;
        }

        public Custom_Name(string _name)
        {
            Name = _name;
        }
    }
}
