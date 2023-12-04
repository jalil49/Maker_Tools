using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardUpdateTool
{
    public partial class CardUpdateTool
    {
        private static bool _showGui = false;

        private static readonly DataType CharacterData = new DataType();
        private static readonly DataType OutfitData = new DataType();

        private static List<CardInfo> CharacterList { get { return CharacterData.CardList; } set { CharacterData.CardList = value; } }
        private static List<CardInfo> OutfitList { get { return OutfitData.CardList; } set { OutfitData.CardList = value; } }
        private static List<string> GuidList { get { return DataType.GuidList; } set { DataType.GuidList = value; } }

        private static Dictionary<string, VersionData> StaticMaxVersion { get { return DataType.StaticMaxVersion; } set { DataType.StaticMaxVersion = value; } }

        private static bool Resolverinprogress { get { return Hooks.Resolverinprogress; } }
        private static bool Waitforresolver { get { return Hooks.Waitforresolver; } set { Hooks.Waitforresolver = value; } }
        private static bool BlackList { get { return Hooks.Missing; } set { Hooks.Missing = value; } }
        private static bool NeedUpdate { get { return Hooks.Outdated; } set { Hooks.Outdated = value; } }
        private static bool WaitForSave { get { return Hooks.WaitForSave; } set { Hooks.WaitForSave = value; } }
        private static bool Migrateable { get { return Hooks.Migrateable; } set { Hooks.Migrateable = value; } }
        private static List<string> MissingList
        {
            get
            {
                var temp = Hooks.MissingList.ToList();
                Hooks.MissingList.Clear();
                return temp;
            }
        }
        private static List<string> OutdatedList
        {
            get
            {
                var temp = Hooks.OutdatedList.ToList();
                Hooks.OutdatedList.Clear();
                return temp;
            }
        }

        private static ChaControl ChaControl => MakerAPI.GetCharacterControl();
        private static int CurrentCoordinate => ChaControl.fileStatus.coordinateType;
        private static ChaFileControl ChaFileControl => ChaControl.chaFile;

        private static bool _interceptsave = false;
        private static string _interceptpath;
        private static SidebarToggle _toggle;
        private static bool _updateInProgress = false;
        private static WaitForSeconds _waitFor01S = new WaitForSeconds(0.1f);
    }
}
