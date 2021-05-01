using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States
{
    public static partial class AccStateSync
    {
        [Serializable]
        [MessagePackObject]
        public class OutfitTriggerInfoV1
        {
            [Key("Index")]
            public int Index { get; set; }
            [Key("Parts")]
            public List<AccTriggerInfo> Parts { get; set; } = new List<AccTriggerInfo>();

            public OutfitTriggerInfoV1(int index) { Index = index; }
        }

        internal static OutfitTriggerInfo UpgradeOutfitTriggerInfoV1(OutfitTriggerInfoV1 OldOutfitTriggerInfo)
        {
            Settings.Logger.LogDebug($"[UpgradeOutfitTriggerInfoV1] Fired!!");
            OutfitTriggerInfo OutfitTriggerInfo = new OutfitTriggerInfo(OldOutfitTriggerInfo.Index);
            if (OldOutfitTriggerInfo.Parts.Count() > 0)
            {
                for (int j = 0; j < OldOutfitTriggerInfo.Parts.Count(); j++)
                {
                    AccTriggerInfo TriggerPart = OldOutfitTriggerInfo.Parts[j];
                    if (TriggerPart.Kind > -1)
                    {
                        OutfitTriggerInfo.Parts[j] = new AccTriggerInfo(j);
                        CopySlotTriggerInfo(TriggerPart, OutfitTriggerInfo.Parts[j]);
                    }
                }
            }
            return OutfitTriggerInfo;
        }

        internal static Dictionary<string, string> UpgradeVirtualGroupNamesV1(Dictionary<string, string> OldVirtualGroupNames)
        {
            Settings.Logger.LogDebug($"[UpgradeVirtualGroupNamesV1] Fired!!");
            Dictionary<string, string> VirtualGroupNames = new Dictionary<string, string>();
            if (OldVirtualGroupNames?.Count() > 0)
            {
                foreach (KeyValuePair<string, string> VirtualGroupName in OldVirtualGroupNames)
                    VirtualGroupNames[VirtualGroupName.Key] = VirtualGroupName.Value;
            }
            return VirtualGroupNames;
        }

        internal static Dictionary<string, VirtualGroupInfo> UpgradeVirtualGroupNamesV2(Dictionary<string, string> OldVirtualGroupNames)
        {
            Settings.Logger.LogDebug($"[UpgradeVirtualGroupNamesV2] Fired!!");
            Dictionary<string, VirtualGroupInfo> OutfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();
            if (OldVirtualGroupNames?.Count() > 0)
            {
                foreach (KeyValuePair<string, string> VirtualGroupName in OldVirtualGroupNames)
                {
                    if (VirtualGroupName.Key.StartsWith("custom_"))
                    {
                        string Group = VirtualGroupName.Key;
                        int Kind = int.Parse(Group.Replace("custom_", "")) + 9;
                        string Label = VirtualGroupName.Value;

                        OutfitVirtualGroupInfo[VirtualGroupName.Key] = new VirtualGroupInfo(Group, Kind, Label);
                    }
                }
            }
            return OutfitVirtualGroupInfo;
        }
    }
}
