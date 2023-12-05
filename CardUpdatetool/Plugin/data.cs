using System.Collections.Generic;
using System.Linq;
using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using UnityEngine;

namespace CardUpdateTool
{
    public partial class CardUpdateTool
    {
        private static bool _showGui;

        private static readonly DataType CharacterData = new DataType();
        private static readonly DataType OutfitData = new DataType();

        private static bool _interceptsave;
        private static string _interceptpath;
        private static SidebarToggle _toggle;
        private static bool _updateInProgress;
        private static WaitForSeconds _waitFor01S = new WaitForSeconds(0.1f);

        private static List<CardInfo> CharacterList
        {
            get => CharacterData.Cardlist;
            set => CharacterData.Cardlist = value;
        }

        private static List<CardInfo> OutfitList
        {
            get => OutfitData.Cardlist;
            set => OutfitData.Cardlist = value;
        }

        private static List<string> GuidList
        {
            get => DataType.GuidList;
            set => DataType.GuidList = value;
        }

        private static Dictionary<string, VersionData> StaticMaxVersion
        {
            get => DataType.StaticMaxVersion;
            set => DataType.StaticMaxVersion = value;
        }

        private static bool Resolverinprogress => Hooks.Resolverinprogress;

        private static bool Waitforresolver
        {
            get => Hooks.Waitforresolver;
            set => Hooks.Waitforresolver = value;
        }

        private static bool BlackList
        {
            get => Hooks.Missing;
            set => Hooks.Missing = value;
        }

        private static bool NeedUpdate
        {
            get => Hooks.Outdated;
            set => Hooks.Outdated = value;
        }

        private static bool WaitForSave
        {
            get => Hooks.WaitForSave;
            set => Hooks.WaitForSave = value;
        }

        private static bool Migrateable
        {
            get => Hooks.Migrateable;
            set => Hooks.Migrateable = value;
        }

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
    }
}