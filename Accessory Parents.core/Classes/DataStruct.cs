using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using MessagePack;
using UnityEngine;
using UnityEngine.Serialization;

namespace Accessory_Parents
{
    [Serializable]
    [MessagePackObject]
    public class CoordinateData
    {
        [FormerlySerializedAs("Parent_Groups")] [Key("_theme_names")]
        public List<CustomName> parentGroups;

        [IgnoreMember] internal Dictionary<int, int> Child;

        [IgnoreMember] internal Dictionary<int, string> OldParent;

        [IgnoreMember] internal Dictionary<int, List<int>> RelatedNames;

        [Key("_relative_data")] public Dictionary<int, Vector3[]> RelativeData;

        public CoordinateData()
        {
            NullCheck();
        }

        public CoordinateData(CoordinateData copy)
        {
            CopyData(copy);
        }

        public CoordinateData(List<CustomName> themeNames, Dictionary<int, Vector3[]> relativeData)
        {
            parentGroups = themeNames.ToNewList();
            RelativeData = relativeData.ToNewDictionary();
            NullCheck();
        }

        public void CleanUp()
        {
            NullCheck();
            parentGroups.RemoveAll(x => x.ParentSlot == -1 || x.childSlots.Count == 0);
            var relativeclean = RelativeData.Keys
                .Where(x => !parentGroups.Any(y => y.childSlots.Contains(x) || y.ParentSlot == x)).ToList();
            foreach (var item in relativeclean) RelativeData.Remove(item);
        }

        public void Clear()
        {
            NullCheck();
            parentGroups.Clear();
            RelativeData.Clear();
            Child.Clear();
            RelatedNames.Clear();
            OldParent.Clear();
        }

        private void NullCheck()
        {
            if (parentGroups == null) parentGroups = new List<CustomName>();
            if (RelativeData == null) RelativeData = new Dictionary<int, Vector3[]>();
            if (RelatedNames == null) RelatedNames = new Dictionary<int, List<int>>();
            if (Child == null) Child = new Dictionary<int, int>();
            if (OldParent == null) OldParent = new Dictionary<int, string>();
        }

        public void CopyData(CoordinateData copy)
        {
            CopyData(copy.parentGroups, copy.RelativeData, copy.Child, copy.RelatedNames, copy.OldParent);
        }

        public void CopyData(List<CustomName> themeNames, Dictionary<int, Vector3[]> relativeData,
            Dictionary<int, int> child, Dictionary<int, List<int>> related, Dictionary<int, string> oldParent)
        {
            parentGroups = themeNames.ToNewList();
            RelativeData = relativeData.ToNewDictionary();
            Child = child.ToNewDictionary();
            RelatedNames = related.ToNewDictionary();
            OldParent = oldParent.ToNewDictionary();
            NullCheck();
        }
    }

    [Serializable]
    [MessagePackObject]
    public class CustomName
    {
        // ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("ChildSlots")] [Key("_childslots")]
        public List<int> childSlots;

        public CustomName(string name, int slot, List<int> childSlots)
        {
            Name = name;
            ParentSlot = slot;
            this.childSlots = childSlots.ToNewList();
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

        [Key("_name")] public string Name { get; set; }

        [Key("_slot")] public int ParentSlot { get; set; } = -1;

        private void NullCheck()
        {
            if (Name == null) Name = "";
            if (childSlots == null) childSlots = new List<int>();
        }
    }
}