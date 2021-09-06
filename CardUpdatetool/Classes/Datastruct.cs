using ExtensibleSaveFormat;
using System.Collections.Generic;
using System.Linq;

namespace CardUpdateTool
{

    public class DataType
    {
        public static Dictionary<string, VersionData> StaticMaxVersion = new Dictionary<string, VersionData>();

        public static List<string> GuidList = new List<string>();

        public List<CardInfo> Cardlist = new List<CardInfo>();

        public List<string> PrintOrder = new List<string>();

        internal void Clear()
        {
            Cardlist.Clear();
            PrintOrder.Clear();
        }

        public int OutDatedcount = 0;
        public int Missingcount = 0;
        public int Migratedcount = 0;

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

        public void CardInfoListConvert(List<string> stringlist)
        {
            Cardlist.Clear();
            foreach (var item in stringlist)
            {
                Cardlist.Add(new CardInfo(item));
            }
        }

        public void NullCheck()
        {
            foreach (var card in Cardlist.Where(x => x.CheckNull))
            {
                foreach (var item in card.NullList)
                {
                    if (StaticMaxVersion.TryGetValue(item, out var versionData))
                    {
                        card.Plugin_data[item] = versionData.version;
                        continue;
                    }
                    card.Plugin_data[item] = 0;
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

        public Dictionary<string, int> Plugin_data = new Dictionary<string, int>();

        public int lasterror = 0;

        public CardInfo(string _path)
        {
            Path = _path;
        }

        public void PopulateDictinary(Dictionary<string, PluginData> pluginData)
        {
            Plugin_data.Clear();
            foreach (var item in pluginData.Where(x => DataType.GuidList.Contains(x.Key)))
            {
                var guid = item.Key;

                if (!DataType.StaticMaxVersion.TryGetValue(guid, out var versionData))
                {
                    versionData = new VersionData(guid, 0);
                    DataType.StaticMaxVersion[guid] = versionData;
                }

                var ContainsNull = item.Value == null;

                if (ContainsNull && !NullList.Contains(item.Key))
                {
                    NullList.Add(item.Key);
                }

                var version = (ContainsNull) ? -1 : item.Value.version;

                Plugin_data[guid] = version;

                versionData.version = System.Math.Max(version, versionData.version);
            }
            if (NullList.Count > 0)
                CheckNull = true;
        }
    }

    public class VersionData
    {
        public string Name;
        public int version = 0;

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
        public bool AnyOutfitOutdated { get { return _anyoutfitoutdated; } }
        public int OutfitOutdatedCount
        {
            get { return _outfitoutdatedcount; }
            set
            {
                _outfitoutdatedcount = value;
                _anyoutfitoutdated = value != 0;
            }
        }
        #endregion;

        #endregion;

        public VersionData(string _name, int _ver)
        {
            Name = _name;
            version = _ver;
        }
    }
}
