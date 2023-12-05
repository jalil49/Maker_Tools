using System;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;

namespace CardUpdateTool
{
    public class DataType
    {
        public static Dictionary<string, VersionData> StaticMaxVersion = new Dictionary<string, VersionData>();

        public static List<string> GuidList = new List<string>();

        public List<CardInfo> Cardlist = new List<CardInfo>();
        public int Migratedcount;
        public int Missingcount;

        public int OutDatedcount;

        public List<string> PrintOrder = new List<string>();

        internal void Clear()
        {
            Cardlist.Clear();
            PrintOrder.Clear();
        }

        public void Reset()
        {
            OutDatedcount = 0;
            Missingcount = 0;
            Migratedcount = 0;
        }

        public void UpdateOutMiss()
        {
            OutDatedcount = Cardlist.Count(x => x.OutdatedMods);
            Missingcount = Cardlist.Count(x => x.MissingMods);
            Migratedcount = Cardlist.Count(x => x.MigratedMods);
        }

        public void CardInfoListConvert(List<string> stringList)
        {
            Cardlist.Clear();
            foreach (var item in stringList) Cardlist.Add(new CardInfo(item));
        }

        public void NullCheck()
        {
            foreach (var card in Cardlist.Where(x => x.CheckNull))
            {
                foreach (var item in card.NullList)
                {
                    if (StaticMaxVersion.TryGetValue(item, out var versionData))
                    {
                        card.PluginData[item] = versionData.Version;
                        continue;
                    }

                    card.PluginData[item] = 0;
                }

                card.CheckNull = false;
            }
        }
    }

    public class CardInfo
    {
        public bool CheckNull;

        public int Lasterror = 0;

        public bool MigratedMods = false;
        public List<string> MissingList = new List<string>();

        public bool MissingMods = false;

        public readonly List<string> NullList = new List<string>();
        public List<string> OutdatedList = new List<string>();

        public bool OutdatedMods = false;

        public readonly Dictionary<string, int> PluginData = new Dictionary<string, int>();

        public CardInfo(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }

        public void PopulateDictinary(Dictionary<string, PluginData> pluginData)
        {
            PluginData.Clear();
            foreach (var item in pluginData.Where(x => DataType.GuidList.Contains(x.Key)))
            {
                var guid = item.Key;

                if (!DataType.StaticMaxVersion.TryGetValue(guid, out var versionData))
                {
                    versionData = new VersionData(guid, 0);
                    DataType.StaticMaxVersion[guid] = versionData;
                }

                var containsNull = item.Value == null;

                if (containsNull && !NullList.Contains(item.Key)) NullList.Add(item.Key);

                var version = containsNull ? -1 : item.Value.version;

                PluginData[guid] = version;

                versionData.Version = Math.Max(version, versionData.Version);
            }

            if (NullList.Count > 0)
                CheckNull = true;
        }
    }

    public class VersionData
    {
        public readonly string Name;
        public int Version;

        public VersionData(string name, int ver)
        {
            Name = name;
            Version = ver;
        }

        #region Chara stuff

        #region private

        private int _charaoutdatedcount;

        #endregion;

        #region public

        public int CharaUpToDate = 0;
        public bool CharaVisible = false;
        public bool AnyCharaOutdated { get; private set; }

        public int CharaOutdatedCount
        {
            get => _charaoutdatedcount;
            set
            {
                _charaoutdatedcount = value;
                AnyCharaOutdated = value != 0;
            }
        }

        #endregion;

        #endregion;

        #region Outfit stuff

        #region private

        private int _outfitoutdatedcount;

        #endregion;

        #region public

        public int OutfitsUpToDate = 0;
        public bool OutfitVisible = false;
        public bool AnyOutfitOutdated { get; private set; }

        public int OutfitOutdatedCount
        {
            get => _outfitoutdatedcount;
            set
            {
                _outfitoutdatedcount = value;
                AnyOutfitOutdated = value != 0;
            }
        }

        #endregion;

        #endregion;
    }
}