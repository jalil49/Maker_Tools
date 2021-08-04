using ExtensibleSaveFormat;
using System.Collections.Generic;

namespace CardUpdateTool
{
    public class CardInfo
    {
        public static Dictionary<string, VersionData> CurrentMaxVersion = new Dictionary<string, VersionData>();

        public static readonly List<string> GuidList = new List<string>();

        public static int[] Coord_OutMissMods = new int[] { 0, 0 };

        public static int[] Chara_OutMissMods = new int[] { 0, 0 };

        public string Path { get; private set; }

        public bool MissingMods = false;
        public bool OutdatedMods = false;

        public Dictionary<string, int> Plugin_data = new Dictionary<string, int>();

        public CardInfo(string _path)
        {
            Path = _path;
        }

        public void PopulateDictinary(Dictionary<string, PluginData> pluginData)
        {
            Plugin_data.Clear();
            foreach (var item in pluginData)
            {
                if (!GuidList.Contains(item.Key))
                {
                    continue;
                }

                string guid = item.Key;

                if (!CurrentMaxVersion.TryGetValue(guid, out var versionData))
                {
                    versionData = new VersionData(guid, 0);
                    CurrentMaxVersion[guid] = versionData;
                }

                int version = (item.Value == null) ? versionData.version : item.Value.version;

                Plugin_data[guid] = version;

                versionData.version = System.Math.Max(version, versionData.version);
            }
        }

        public static List<CardInfo> CardInfoListConvert(List<string> stringlist)
        {
            List<CardInfo> cardlist = new List<CardInfo>();
            foreach (var item in stringlist)
            {
                cardlist.Add(new CardInfo(item));
            }
            return cardlist;
        }

        public static void Reset()
        {
            Coord_OutMissMods = new int[] { 0, 0 };
            Chara_OutMissMods = new int[] { 0, 0 };
        }

        public class VersionData
        {
            public string Name;
            public int version = 0;
            public bool Charavisible = false;
            public bool OutfitVisible = false;
            public bool AnyCharaOutdated = false;
            public bool AnyOutfitOutdated = false;
            public VersionData(string _name, int _ver)
            {
                Name = _name;
                version = _ver;
            }
        }
    }
}
