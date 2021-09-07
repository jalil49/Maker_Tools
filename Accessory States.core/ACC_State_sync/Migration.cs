using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States
{
    public static partial class AccStateSync
    {
        public partial class Migration
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

            internal static OutfitTriggerInfo UpgradeOutfitTriggerInfoV1(OutfitTriggerInfoV1 _oldOutfitTriggerInfo)
            {
                var _outfitTriggerInfo = new OutfitTriggerInfo(_oldOutfitTriggerInfo.Index);
                if (_oldOutfitTriggerInfo.Parts.Count() > 0)
                {
                    for (var j = 0; j < _oldOutfitTriggerInfo.Parts.Count(); j++)
                    {
                        var Itrigger = _oldOutfitTriggerInfo.Parts[j];
                        if (Itrigger.Kind > -1)
                        {
                            _outfitTriggerInfo.Parts[j] = new AccTriggerInfo(j);
                            CopySlotTriggerInfo(Itrigger, _outfitTriggerInfo.Parts[j]);
                        }
                    }
                }
                return _outfitTriggerInfo;
            }

            [Serializable]
            [MessagePackObject]
            public class AccTriggerInfo
            {
                [Key("Slot")]
                public int Slot { get; set; }
                [Key("Kind")]
                public int Kind { get; set; } = -1;
                [Key("Group")]
                public string Group { get; set; } = "";
                [Key("State")]
                public List<bool> State { get; set; } = new List<bool>() { true, false, false, false };

                public AccTriggerInfo(int slot) { Slot = slot; }
            }

            [Serializable]
            [MessagePackObject]
            public class OutfitTriggerInfo
            {
                [Key("Index")]
                public int Index { get; set; }
                [Key("Parts")]
                public Dictionary<int, AccTriggerInfo> Parts { get; set; } = new Dictionary<int, AccTriggerInfo>();

                public OutfitTriggerInfo(int index) { Index = index; }
            }

            [Serializable]
            [MessagePackObject]
            public class VirtualGroupInfo
            {
                [Key("Kind")]
                public int Kind { get; set; }
                [Key("Group")]
                public string Group { get; set; }
                [Key("Label")]
                public string Label { get; set; }
                [Key("Secondary")]
                public bool Secondary { get; set; } = false;
                [Key("State")]
                public bool State { get; set; } = true;

                public VirtualGroupInfo(string group, int kind, string label = "")
                {
                    Group = group;
                    Kind = kind;
                    if (label.IsNullOrEmpty())
                    {
                        if (kind > 9)
                            label = group.Replace("custom_", "Custom ");
                        else if (kind == 9)
                        {
                            label = Group;
                            if (_accessoryParentNames.ContainsKey(Group))
                                label = _accessoryParentNames[Group];
                        }
                    }
                    Label = label;
                }
            }

            internal static Dictionary<string, string> UpgradeVirtualGroupNamesV1(Dictionary<string, string> _oldVirtualGroupNames)
            {
                var _outfitVirtualGroupInfo = new Dictionary<string, string>();
                if (_oldVirtualGroupNames?.Count() > 0)
                {
                    foreach (var _group in _oldVirtualGroupNames)
                        _outfitVirtualGroupInfo[_group.Key] = _group.Value;
                }
                return _outfitVirtualGroupInfo;
            }

            internal static Dictionary<string, VirtualGroupInfo> UpgradeVirtualGroupNamesV2(Dictionary<string, string> _oldVirtualGroupNames)
            {
                var _outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();
                if (_oldVirtualGroupNames?.Count() > 0)
                {
                    foreach (var _group in _oldVirtualGroupNames)
                    {
                        if (_group.Key.StartsWith("custom_"))
                            _outfitVirtualGroupInfo[_group.Key] = new VirtualGroupInfo(_group.Key, int.Parse(_group.Key.Replace("custom_", "")) + 9, _group.Value);
                    }
                }
                return _outfitVirtualGroupInfo;
            }

            internal static void ConvertCharaPluginData(PluginData _pluginData, ref List<TriggerProperty> _outputTriggerProperty, ref List<TriggerGroup> _outputTriggerGroup)
            {
                var _charaTriggerInfo = new Dictionary<int, OutfitTriggerInfo>();
                var _charaVirtualGroupInfo = new Dictionary<int, Dictionary<string, VirtualGroupInfo>>();

                _pluginData.data.TryGetValue("CharaTriggerInfo", out var _loadedCharaTriggerInfo);
                if (_loadedCharaTriggerInfo == null) return;

                if (_pluginData.version < 2)
                {
                    var _oldCharaTriggerInfo = MessagePackSerializer.Deserialize<List<OutfitTriggerInfoV1>>((byte[])_loadedCharaTriggerInfo);
                    for (var i = 0; i < 7; i++)
                        _charaTriggerInfo[i] = UpgradeOutfitTriggerInfoV1(_oldCharaTriggerInfo[i]);
                }
                else
                    _charaTriggerInfo = MessagePackSerializer.Deserialize<Dictionary<int, OutfitTriggerInfo>>((byte[])_loadedCharaTriggerInfo);

                if (_charaTriggerInfo == null) return;

                if (_pluginData.version < 5)
                {
                    if (_pluginData.data.TryGetValue("CharaVirtualGroupNames", out var _loadedCharaVirtualGroupNames) && _loadedCharaVirtualGroupNames != null)
                    {
                        if (_pluginData.version < 2)
                        {
                            var _oldCharaVirtualGroupNames = MessagePackSerializer.Deserialize<List<Dictionary<string, string>>>((byte[])_loadedCharaVirtualGroupNames);
                            if (_oldCharaVirtualGroupNames?.Count == 7)
                            {
                                for (var i = 0; i < 7; i++)
                                {
                                    var _outfitVirtualGroupNames = UpgradeVirtualGroupNamesV1(_oldCharaVirtualGroupNames[i]);
                                    _charaVirtualGroupInfo[i] = UpgradeVirtualGroupNamesV2(_outfitVirtualGroupNames);
                                }
                            }
                        }
                        else
                        {
                            var _charaVirtualGroupNames = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, string>>>((byte[])_loadedCharaVirtualGroupNames);
                            for (var i = 0; i < 7; i++)
                                _charaVirtualGroupInfo[i] = UpgradeVirtualGroupNamesV2(_charaVirtualGroupNames[i]);
                        }
                    }
                }
                else
                {
                    if (_pluginData.data.TryGetValue("CharaVirtualGroupInfo", out var _loadedCharaVirtualGroupInfo) && _loadedCharaVirtualGroupInfo != null)
                        _charaVirtualGroupInfo = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, VirtualGroupInfo>>>((byte[])_loadedCharaVirtualGroupInfo);
                }

                Migrate(_charaTriggerInfo, _charaVirtualGroupInfo, ref _outputTriggerProperty, ref _outputTriggerGroup);
            }

            internal static void ConvertOutfitPluginData(int _coordinate, PluginData _pluginData, ref List<TriggerProperty> _outputTriggerProperty, ref List<TriggerGroup> _outputTriggerGroup)
            {
                OutfitTriggerInfo _outfitTriggerInfo;
                var _outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();

                _pluginData.data.TryGetValue("OutfitTriggerInfo", out var _loadedOutfitTriggerInfo);
                if (_loadedOutfitTriggerInfo == null) return;

                if (_pluginData.version < 2)
                {
                    var _oldCharaTriggerInfo = MessagePackSerializer.Deserialize<OutfitTriggerInfoV1>((byte[])_loadedOutfitTriggerInfo);
                    _outfitTriggerInfo = UpgradeOutfitTriggerInfoV1(_oldCharaTriggerInfo);
                }
                else
                    _outfitTriggerInfo = MessagePackSerializer.Deserialize<OutfitTriggerInfo>((byte[])_loadedOutfitTriggerInfo);

                if (_outfitTriggerInfo == null) return;

                if (_pluginData.version < 5)
                {
                    if (_pluginData.data.TryGetValue("OutfitVirtualGroupNames", out var _loadedOutfitVirtualGroupNames) && _loadedOutfitVirtualGroupNames != null)
                    {
                        var _outfitVirtualGroupNames = MessagePackSerializer.Deserialize<Dictionary<string, string>>((byte[])_loadedOutfitVirtualGroupNames);
                        _outfitVirtualGroupInfo = UpgradeVirtualGroupNamesV2(_outfitVirtualGroupNames);
                    }
                }
                else
                {
                    if (_pluginData.data.TryGetValue("OutfitVirtualGroupInfo", out var _loadedOutfitVirtualGroupInfo) && _loadedOutfitVirtualGroupInfo != null)
                        _outfitVirtualGroupInfo = MessagePackSerializer.Deserialize<Dictionary<string, VirtualGroupInfo>>((byte[])_loadedOutfitVirtualGroupInfo);
                }

                Migrate(_coordinate, _outfitTriggerInfo, _outfitVirtualGroupInfo, ref _outputTriggerProperty, ref _outputTriggerGroup);
            }

            public static void Migrate(Dictionary<int, OutfitTriggerInfo> _charaTriggerInfo, Dictionary<int, Dictionary<string, VirtualGroupInfo>> _charaVirtualGroupInfo, ref List<TriggerProperty> _outputTriggerProperty, ref List<TriggerGroup> _outputTriggerGroup)
            {
                for (var _coordinate = 0; _coordinate < 7; _coordinate++)
                {
                    var _outfitTriggerInfo = _charaTriggerInfo.ContainsKey(_coordinate) ? _charaTriggerInfo[_coordinate] : new OutfitTriggerInfo(_coordinate);
                    Dictionary<string, VirtualGroupInfo> _outfitVirtualGroupInfo;
                    if (!_charaVirtualGroupInfo.ContainsKey(_coordinate) || _charaVirtualGroupInfo[_coordinate]?.Count == 0)
                        _outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();
                    else
                        _outfitVirtualGroupInfo = _charaVirtualGroupInfo[_coordinate];
                    Migrate(_coordinate, _outfitTriggerInfo, _outfitVirtualGroupInfo, ref _outputTriggerProperty, ref _outputTriggerGroup);
                }
            }

            public static void Migrate(int _coordinate, OutfitTriggerInfo _outfitTriggerInfo, Dictionary<string, VirtualGroupInfo> _outfitVirtualGroupInfo, ref List<TriggerProperty> _outputTriggerProperty, ref List<TriggerGroup> _outputTriggerGroup)
            {
                if (_outfitTriggerInfo == null) return;
                if (_outfitVirtualGroupInfo == null)
                    _outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();

                var _mapping = new Dictionary<string, int>();
                var _refBase = 9;

                var _parts = _outfitTriggerInfo.Parts.Values.OrderBy(x => x.Kind).ThenBy(x => x.Group).ThenBy(x => x.Slot).ToList();
                foreach (var _part in _parts)
                {
                    if (MathfEx.RangeEqualOn(0, _part.Kind, 8))
                    {
                        for (var i = 0; i < 4; i++)
                            _outputTriggerProperty.Add(new TriggerProperty(_coordinate, _part.Slot, _part.Kind, i, _part.State[i], 0));
                    }
                    else if (_part.Kind >= 9)
                    {
                        if (!_mapping.ContainsKey(_part.Group))
                        {
                            _mapping[_part.Group] = _refBase;
                            _refBase++;
                        }

                        _outputTriggerProperty.Add(new TriggerProperty(_coordinate, _part.Slot, _mapping[_part.Group], 0, _part.State[0], 0));
                        _outputTriggerProperty.Add(new TriggerProperty(_coordinate, _part.Slot, _mapping[_part.Group], 1, _part.State[3], 0));
                    }
                }

                foreach (var x in _mapping)
                {
                    if (!_outfitVirtualGroupInfo.ContainsKey(x.Key))
                    {
                        var _label = _accessoryParentNames.ContainsKey(x.Key) ? _accessoryParentNames[x.Key] : x.Key;
                        _outputTriggerGroup.Add(new TriggerGroup(_coordinate, x.Value, _label));
                    }
                    else
                    {
                        var _group = _outfitVirtualGroupInfo[x.Key];
                        _outputTriggerGroup.Add(new TriggerGroup(_coordinate, x.Value, _group.Label, (_group.State ? 0 : 1), 0, (_group.Secondary ? 1 : -1)));
                    }
                }
            }
            public static void CopySlotTriggerInfo(AccTriggerInfo CopySource, AccTriggerInfo CopyDestination)
            {
                CopyDestination.Slot = CopySource.Slot;
                CopyDestination.Kind = CopySource.Kind;
                CopyDestination.Group = CopySource.Group;
                CopyDestination.State = CopySource.State.ToList();
            }
        }
    }
}


