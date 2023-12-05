using System;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using MessagePack;

namespace Accessory_States
{
    public static partial class AccStateSync
    {
        public static class Migration
        {
            private static OutfitTriggerInfo UpgradeOutfitTriggerInfoV1(OutfitTriggerInfoV1 oldOutfitTriggerInfo)
            {
                var outfitTriggerInfo = new OutfitTriggerInfo(oldOutfitTriggerInfo.Index);
                if (oldOutfitTriggerInfo.Parts.Any())
                    for (var j = 0; j < oldOutfitTriggerInfo.Parts.Count(); j++)
                    {
                        var itrigger = oldOutfitTriggerInfo.Parts[j];
                        if (itrigger.Kind > -1)
                        {
                            outfitTriggerInfo.Parts[j] = new AccTriggerInfo(j);
                            CopySlotTriggerInfo(itrigger, outfitTriggerInfo.Parts[j]);
                        }
                    }

                return outfitTriggerInfo;
            }

            internal static Dictionary<string, string> UpgradeVirtualGroupNamesV1(
                Dictionary<string, string> oldVirtualGroupNames)
            {
                var outfitVirtualGroupInfo = new Dictionary<string, string>();
                if (oldVirtualGroupNames?.Count() > 0)
                    foreach (var group in oldVirtualGroupNames)
                        outfitVirtualGroupInfo[group.Key] = group.Value;

                return outfitVirtualGroupInfo;
            }

            internal static Dictionary<string, VirtualGroupInfo> UpgradeVirtualGroupNamesV2(
                Dictionary<string, string> oldVirtualGroupNames)
            {
                var outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();
                if (oldVirtualGroupNames?.Count() > 0)
                    foreach (var group in oldVirtualGroupNames)
                        if (group.Key.StartsWith("custom_"))
                            outfitVirtualGroupInfo[group.Key] = new VirtualGroupInfo(group.Key,
                                int.Parse(group.Key.Replace("custom_", "")) + 9, group.Value);

                return outfitVirtualGroupInfo;
            }

            internal static void ConvertCharaPluginData(PluginData pluginData,
                ref List<TriggerProperty> outputTriggerProperty, ref List<TriggerGroup> outputTriggerGroup)
            {
                var charaTriggerInfo = new Dictionary<int, OutfitTriggerInfo>();
                var charaVirtualGroupInfo = new Dictionary<int, Dictionary<string, VirtualGroupInfo>>();

                pluginData.data.TryGetValue("CharaTriggerInfo", out var loadedCharaTriggerInfo);
                if (loadedCharaTriggerInfo == null) return;

                if (pluginData.version < 2)
                {
                    var oldCharaTriggerInfo =
                        MessagePackSerializer.Deserialize<List<OutfitTriggerInfoV1>>((byte[])loadedCharaTriggerInfo);
                    for (var i = 0; i < 7; i++)
                        charaTriggerInfo[i] = UpgradeOutfitTriggerInfoV1(oldCharaTriggerInfo[i]);
                }
                else
                {
                    charaTriggerInfo =
                        MessagePackSerializer.Deserialize<Dictionary<int, OutfitTriggerInfo>>(
                            (byte[])loadedCharaTriggerInfo);
                }

                if (charaTriggerInfo == null) return;

                if (pluginData.version < 5)
                {
                    if (pluginData.data.TryGetValue("CharaVirtualGroupNames", out var loadedCharaVirtualGroupNames) &&
                        loadedCharaVirtualGroupNames != null)
                    {
                        if (pluginData.version < 2)
                        {
                            var oldCharaVirtualGroupNames =
                                MessagePackSerializer.Deserialize<List<Dictionary<string, string>>>(
                                    (byte[])loadedCharaVirtualGroupNames);
                            if (oldCharaVirtualGroupNames?.Count == 7)
                                for (var i = 0; i < 7; i++)
                                {
                                    var outfitVirtualGroupNames =
                                        UpgradeVirtualGroupNamesV1(oldCharaVirtualGroupNames[i]);
                                    charaVirtualGroupInfo[i] = UpgradeVirtualGroupNamesV2(outfitVirtualGroupNames);
                                }
                        }
                        else
                        {
                            var charaVirtualGroupNames =
                                MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, string>>>(
                                    (byte[])loadedCharaVirtualGroupNames);
                            for (var i = 0; i < 7; i++)
                                charaVirtualGroupInfo[i] = UpgradeVirtualGroupNamesV2(charaVirtualGroupNames[i]);
                        }
                    }
                }
                else
                {
                    if (pluginData.data.TryGetValue("CharaVirtualGroupInfo", out var loadedCharaVirtualGroupInfo) &&
                        loadedCharaVirtualGroupInfo != null)
                        charaVirtualGroupInfo =
                            MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, VirtualGroupInfo>>>(
                                (byte[])loadedCharaVirtualGroupInfo);
                }

                Migrate(charaTriggerInfo, charaVirtualGroupInfo, ref outputTriggerProperty, ref outputTriggerGroup);
            }

            internal static void ConvertOutfitPluginData(int coordinate, PluginData pluginData,
                ref List<TriggerProperty> outputTriggerProperty, ref List<TriggerGroup> outputTriggerGroup)
            {
                OutfitTriggerInfo outfitTriggerInfo;
                var outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();

                pluginData.data.TryGetValue("OutfitTriggerInfo", out var loadedOutfitTriggerInfo);
                if (loadedOutfitTriggerInfo == null) return;

                if (pluginData.version < 2)
                {
                    var oldCharaTriggerInfo =
                        MessagePackSerializer.Deserialize<OutfitTriggerInfoV1>((byte[])loadedOutfitTriggerInfo);
                    outfitTriggerInfo = UpgradeOutfitTriggerInfoV1(oldCharaTriggerInfo);
                }
                else
                {
                    outfitTriggerInfo =
                        MessagePackSerializer.Deserialize<OutfitTriggerInfo>((byte[])loadedOutfitTriggerInfo);
                }

                if (outfitTriggerInfo == null) return;

                if (pluginData.version < 5)
                {
                    if (pluginData.data.TryGetValue("OutfitVirtualGroupNames", out var loadedOutfitVirtualGroupNames) &&
                        loadedOutfitVirtualGroupNames != null)
                    {
                        var outfitVirtualGroupNames =
                            MessagePackSerializer.Deserialize<Dictionary<string, string>>(
                                (byte[])loadedOutfitVirtualGroupNames);
                        outfitVirtualGroupInfo = UpgradeVirtualGroupNamesV2(outfitVirtualGroupNames);
                    }
                }
                else
                {
                    if (pluginData.data.TryGetValue("OutfitVirtualGroupInfo", out var loadedOutfitVirtualGroupInfo) &&
                        loadedOutfitVirtualGroupInfo != null)
                        outfitVirtualGroupInfo =
                            MessagePackSerializer.Deserialize<Dictionary<string, VirtualGroupInfo>>(
                                (byte[])loadedOutfitVirtualGroupInfo);
                }

                Migrate(coordinate, outfitTriggerInfo, outfitVirtualGroupInfo, ref outputTriggerProperty,
                    ref outputTriggerGroup);
            }

            public static void Migrate(Dictionary<int, OutfitTriggerInfo> charaTriggerInfo,
                Dictionary<int, Dictionary<string, VirtualGroupInfo>> charaVirtualGroupInfo,
                ref List<TriggerProperty> outputTriggerProperty, ref List<TriggerGroup> outputTriggerGroup)
            {
                for (var coordinate = 0; coordinate < 7; coordinate++)
                {
                    var outfitTriggerInfo = charaTriggerInfo.TryGetValue(coordinate, out var value)
                        ? value
                        : new OutfitTriggerInfo(coordinate);
                    Dictionary<string, VirtualGroupInfo> outfitVirtualGroupInfo;
                    if (!charaVirtualGroupInfo.ContainsKey(coordinate) || charaVirtualGroupInfo[coordinate]?.Count == 0)
                        outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();
                    else
                        outfitVirtualGroupInfo = charaVirtualGroupInfo[coordinate];
                    Migrate(coordinate, outfitTriggerInfo, outfitVirtualGroupInfo, ref outputTriggerProperty,
                        ref outputTriggerGroup);
                }
            }

            public static void Migrate(int coordinate, OutfitTriggerInfo outfitTriggerInfo,
                Dictionary<string, VirtualGroupInfo> outfitVirtualGroupInfo,
                ref List<TriggerProperty> outputTriggerProperty, ref List<TriggerGroup> outputTriggerGroup)
            {
                if (outfitTriggerInfo == null) return;
                if (outfitVirtualGroupInfo == null)
                    outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();

                var mapping = new Dictionary<string, int>();
                var refBase = 9;

                var parts = outfitTriggerInfo.Parts.Values.OrderBy(x => x.Kind).ThenBy(x => x.Group).ThenBy(x => x.Slot)
                    .ToList();
                foreach (var part in parts)
                    if (MathfEx.RangeEqualOn(0, part.Kind, 8))
                    {
                        for (var i = 0; i < 4; i++)
                            outputTriggerProperty.Add(new TriggerProperty(coordinate, part.Slot, part.Kind, i,
                                part.State[i], 0));
                    }
                    else if (part.Kind >= 9)
                    {
                        if (!mapping.ContainsKey(part.Group))
                        {
                            mapping[part.Group] = refBase;
                            refBase++;
                        }

                        outputTriggerProperty.Add(new TriggerProperty(coordinate, part.Slot, mapping[part.Group], 0,
                            part.State[0], 0));
                        outputTriggerProperty.Add(new TriggerProperty(coordinate, part.Slot, mapping[part.Group], 1,
                            part.State[3], 0));
                    }

                foreach (var x in mapping)
                    if (!outfitVirtualGroupInfo.ContainsKey(x.Key))
                    {
                        var label = AccessoryParentNames.TryGetValue(x.Key, out var name) ? name : x.Key;
                        outputTriggerGroup.Add(new TriggerGroup(coordinate, x.Value, label));
                    }
                    else
                    {
                        var group = outfitVirtualGroupInfo[x.Key];
                        outputTriggerGroup.Add(new TriggerGroup(coordinate, x.Value, group.Label, group.State ? 0 : 1,
                            0, group.Secondary ? 1 : -1));
                    }
            }

            public static void CopySlotTriggerInfo(AccTriggerInfo copySource, AccTriggerInfo copyDestination)
            {
                copyDestination.Slot = copySource.Slot;
                copyDestination.Kind = copySource.Kind;
                copyDestination.Group = copySource.Group;
                copyDestination.State = copySource.State.ToList();
            }

            [Serializable]
            [MessagePackObject]
            public class OutfitTriggerInfoV1
            {
                public OutfitTriggerInfoV1(int index)
                {
                    Index = index;
                }

                [Key("Index")] public int Index { get; set; }
                [Key("Parts")] public List<AccTriggerInfo> Parts { get; set; } = new List<AccTriggerInfo>();
            }

            [Serializable]
            [MessagePackObject]
            public class AccTriggerInfo
            {
                public AccTriggerInfo(int slot)
                {
                    Slot = slot;
                }

                [Key("Slot")] public int Slot { get; set; }
                [Key("Kind")] public int Kind { get; set; } = -1;
                [Key("Group")] public string Group { get; set; } = "";
                [Key("State")] public List<bool> State { get; set; } = new List<bool> { true, false, false, false };
            }

            [Serializable]
            [MessagePackObject]
            public class OutfitTriggerInfo
            {
                public OutfitTriggerInfo(int index)
                {
                    Index = index;
                }

                [Key("Index")] public int Index { get; set; }

                [Key("Parts")]
                public Dictionary<int, AccTriggerInfo> Parts { get; set; } = new Dictionary<int, AccTriggerInfo>();
            }

            [Serializable]
            [MessagePackObject]
            public class VirtualGroupInfo
            {
                public VirtualGroupInfo(string group, int kind, string label = "")
                {
                    Group = group;
                    Kind = kind;
                    if (label.IsNullOrEmpty())
                    {
                        if (kind > 9)
                        {
                            label = group.Replace("custom_", "Custom ");
                        }
                        else if (kind == 9)
                        {
                            label = Group;
                            if (AccessoryParentNames.TryGetValue(Group, out var name))
                                label = name;
                        }
                    }

                    Label = label;
                }

                [Key("Kind")] public int Kind { get; set; }
                [Key("Group")] public string Group { get; set; }
                [Key("Label")] public string Label { get; set; }
                [Key("Secondary")] public bool Secondary { get; set; } = false;
                [Key("State")] public bool State { get; set; } = true;
            }
        }
    }
}