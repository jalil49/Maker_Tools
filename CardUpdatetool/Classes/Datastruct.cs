using ExtensibleSaveFormat;
using System.Collections.Generic;
using System.Linq;
using Illusion;

namespace CardUpdateTool
{

    public class DataType
    {
        public static Dictionary<string, VersionData> StaticMaxVersion = new Dictionary<string, VersionData>();

        public static List<string> GuidList = new List<string>();

        public List<CardInfo> CardList = new List<CardInfo>();

        public List<string> PrintOrder = new List<string>();

        internal void Clear()
        {
            Utils.Enum<>.EnumerateParameter
            CardList.Clear();
            PrintOrder.Clear();
        }

        public int OutDatedCount = 0;
        public int MissingCount = 0;
        public int MigratedCount = 0;

        public void Reset()
        {
            OutDatedCount = 0;
            MissingCount = 0;
            MigratedCount = 0;
        }

        public void UpdateOutMiss()
        {
            OutDatedCount = CardList.Count(x => x.OutdatedMods);
            MissingCount = CardList.Count(x => x.MissingMods);
            MigratedCount = CardList.Count(x => x.MigratedMods);
        }

        public void CardInfoListConvert(List<string> stringList)
        {
            CardList.Clear();
            foreach (var item in stringList)
            {
                CardList.Add(new CardInfo(item));
            }
        }

        public void NullCheck()
        {
            foreach (var card in CardList.Where(x => x.CheckNull))
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
        public string Path { get; private set; }

        public bool MissingMods = false;
        public List<string> MissingList = new List<string>();

        public bool OutdatedMods = false;
        public List<string> OutdatedList = new List<string>();

        public bool MigratedMods = false;

        public bool CheckNull = false;

        public List<string> NullList = new List<string>();

        public Dictionary<string, int> PluginData = new Dictionary<string, int>();

        public int Lasterror = 0;

        public CardInfo(string path)
        {
            Path = path;
        }

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

                if (containsNull && !NullList.Contains(item.Key))
                {
                    NullList.Add(item.Key);
                }

                var version = (containsNull) ? -1 : item.Value.version;

                PluginData[guid] = version;

                versionData.Version = System.Math.Max(version, versionData.Version);
            }
            if (NullList.Count > 0)
                CheckNull = true;
        }
    }

    public class VersionData
    {
        public string Name;
        public int Version = 0;

        #region Chara stuff

        #region private
        private bool _anycharaoutdated = false;
        private int _charaoutdatedcount = 0;
        #endregion;

        #region public
        public int CharaUpToDate = 0;
        public bool CharaVisible = false;
        public bool AnyCharaOutdated { get { return _anycharaoutdated; } }
        public int CharaOutdatedCount
        {
            get { return _charaoutdatedcount; }
            set
            {
                _charaoutdatedcount = value;
                _anycharaoutdated = value != 0;
            }
        }
        #endregion;

        #endregion;

        #region Outfit stuff

        #region private
        private bool _anyoutfitoutdated = false;
        private int _outfitoutdatedcount = 0;
        #endregion;

        #region public
        public int OutfitsUpToDate = 0;
        public bool OutfitVisible = false;
        public bool AnyOutfitOutdated => _anyoutfitoutdated;

        public int OutfitOutdatedCount
        {
            get => _outfitoutdatedcount;
            set
            {
                _outfitoutdatedcount = value;
                _anyoutfitoutdated = value != 0;
            }
        }
        #endregion;

        #endregion;

        public VersionData(string name, int ver)
        {
            Name = name;
            Version = ver;
        }
    }
}
