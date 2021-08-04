using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;

namespace CardUpdateTool
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private static bool ShowGui = false;
        private static List<CardInfo> CharacterList = new List<CardInfo>();
        private static List<CardInfo> OutfitList = new List<CardInfo>();

        internal static bool resolverinprogress = false;
        internal static bool Waitforresolver = true;
        internal static bool BlackList = false;
        internal static bool NeedUpdate = false;
        internal static bool WaitForSave = false;

        private static bool interceptsave = false;
        private static string interceptpath;
        private static SidebarToggle toggle;
        private static bool UpdateInProgress = false;

        internal static void Exiting()
        {
            CharacterList.Clear();
            OutfitList.Clear();
            CardInfo.GuidList.Clear();
            CardInfo.CurrentMaxVersion.Clear();
            ShowGui = false;
        }

        internal static void Starting()
        {
            CharacterPath = new DirectoryInfo(UserData.Path).FullName + "chara";
            switch (MakerAPI.GetMakerSex())
            {
                case 0:
                    CharacterPath += @"\male";
                    break;
                case 1:
                    CharacterPath += @"\female";
                    break;
            }
            GetAllGuids();
        }

        internal static void Register(object sender, RegisterSubCategoriesEvent e)
        {
            toggle = e.AddSidebarControl(new SidebarToggle("Card Update Tool", ShowGui, Settings.Instance));
            toggle.ValueChanged.Subscribe(X => ShowGui = X);
        }

        private void CharacterCardCheck(string StartPath)
        {
            CharacterList = CardInfo.CardInfoListConvert(DirectoryFinder.Get_Cards_From_Path(StartPath));
            StartCoroutine(test());

            IEnumerator<int> test()
            {
                UpdateInProgress = true;
                var testcontrol = new ChaFile();
                InProgressCount = 0;
                ProgressTotal = CharacterList.Count;
                for (; InProgressCount < ProgressTotal; InProgressCount++)
                {
                    if (ForceStop)
                    {
                        CharacterList.RemoveRange(InProgressCount - 1, ProgressTotal - InProgressCount + 1);
                        UpdateVisibleGuids();
                        UpdateOutmiss();
                        UpdateInProgress = false;
                        ForceStop = false;
                        yield break;
                    }

                    if (InProgressCount % 50 == 0)
                        yield return 0;

                    NeedUpdate = BlackList = false;

                    var card = CharacterList[InProgressCount];
                    Waitforresolver = true;

                    testcontrol.LoadFile(card.Path);
                    int yieldcount = -1;
                    while (resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                    {
                        yield return 0;
                    }
                    if (testcontrol.lastLoadErrorCode != 0)
                    {
                        MoveToNewDirectory(StartPath, card.Path, "/BadCardData/");
                        CharacterList.RemoveAt(InProgressCount);
                        InProgressCount--;
                        ProgressTotal--;
                        continue;
                    }

                    card.MissingMods = BlackList;

                    card.OutdatedMods = NeedUpdate && !BlackList;

                    var extendedData = ExtendedSave.GetAllExtendedData(testcontrol);
                    if (extendedData != null)
                    {
                        card.PopulateDictinary(extendedData);
                    }
                }
                UpdateVisibleGuids();
                UpdateOutmiss();
                UpdateInProgress = false;
            }
        }

        private void OutfitCardCheck(string StartPath)
        {
            OutfitList = CardInfo.CardInfoListConvert(DirectoryFinder.Get_Cards_From_Path(StartPath));

            StartCoroutine(test());
            IEnumerator<int> test()
            {
                UpdateInProgress = true;
                var testcontrol = new ChaFileCoordinate();
                InProgressCount = 0;
                ProgressTotal = OutfitList.Count;
                for (; InProgressCount < ProgressTotal; InProgressCount++)
                {
                    if (ForceStop)
                    {
                        OutfitList.RemoveRange(InProgressCount - 1, ProgressTotal - InProgressCount + 1);
                        UpdateVisibleGuids();
                        UpdateOutmiss();
                        UpdateInProgress = false;
                        ForceStop = false;
                        yield break;
                    }

                    if (InProgressCount % 50 == 0)
                        yield return 0;

                    NeedUpdate = BlackList = false;

                    var card = OutfitList[InProgressCount];
                    Waitforresolver = true;

                    testcontrol.LoadFile(card.Path);
                    int yieldcount = -1;

                    while (resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                    {
                        yield return 0;
                    }
                    if (testcontrol.lastLoadErrorCode != 0)
                    {
                        MoveToNewDirectory(StartPath, card.Path, "/BadCardData/");
                        OutfitList.RemoveAt(InProgressCount);
                        InProgressCount--;
                        ProgressTotal--;
                        continue;
                    }

                    card.MissingMods = BlackList;

                    card.OutdatedMods = NeedUpdate && !BlackList;

                    var extendedData = ExtendedSave.GetAllExtendedData(testcontrol);
                    if (extendedData != null)
                    {
                        card.PopulateDictinary(extendedData);
                    }
                }
                UpdateVisibleGuids();
                UpdateOutmiss();
                UpdateInProgress = false;
            }
        }

        private void MoveToNewDirectory(string startpath, string source, string Dest)
        {
            var newlocation = startpath + source.Replace(startpath, Dest);
            var DirectoryName = Path.GetDirectoryName(newlocation);
            if (!Directory.Exists(DirectoryName))
            {
                Directory.CreateDirectory(DirectoryName);
            }
            if (File.Exists(newlocation))
            {
                File.Delete(newlocation);
            }
            File.Move(source, newlocation);
        }

        private void UpdateOutmiss()
        {
            CardInfo.Chara_OutMissMods[0] = CharacterList.Count(x => x.OutdatedMods);
            CardInfo.Chara_OutMissMods[1] = CharacterList.Count(x => x.MissingMods);

            CardInfo.Coord_OutMissMods[0] = OutfitList.Count(x => x.OutdatedMods);
            CardInfo.Coord_OutMissMods[1] = OutfitList.Count(x => x.MissingMods);
            MissingVersionCheck();
        }

        private static void UpdateVisibleGuids()
        {
            foreach (var item in Constants.ReadableGuid)
            {
                if (!CardInfo.CurrentMaxVersion.ContainsKey(item.Key) && CardInfo.GuidList.Contains(item.Key))
                {
                    CardInfo.CurrentMaxVersion[item.Key] = new CardInfo.VersionData(item.Value.Name, item.Value.KnownVersion) { Charavisible = item.Value.Charavisible, OutfitVisible = item.Value.Outfitvisible };
                }
            }
            foreach (var guid in CardInfo.CurrentMaxVersion)
            {
                guid.Value.Charavisible = guid.Value.Charavisible || CharacterList.Any(x => x.Plugin_data.ContainsKey(guid.Key));
                guid.Value.OutfitVisible = guid.Value.OutfitVisible || OutfitList.Any(x => x.Plugin_data.ContainsKey(guid.Key));
            }
        }

        private static void GetAllGuids()
        {
            var GuidList = CardInfo.GuidList;
            foreach (var item in CharacterApi.RegisteredHandlers)
            {
                GuidList.Add(item.ExtendedDataId);
            }
            UpdateVisibleGuids();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        internal static string InterceptSave(string path)
        {
            if (interceptsave)
            {
                return interceptpath;
            }
            return path;
        }

        internal static void DeleteIntercept(string path)
        {
            WaitForSave = false;
            if (!interceptsave || path == interceptpath)
            {
                return;
            }
            Settings.Logger.LogWarning($"Intercepted and deleted {path}");
            File.Delete(path);
        }

        private static void MissingVersionCheck()
        {
            foreach (var maxdatainfo in CardInfo.CurrentMaxVersion)
            {
                maxdatainfo.Value.AnyCharaOutdated = CharacterList.Any(x => !x.MissingMods && x.Plugin_data.ContainsKey(maxdatainfo.Key) && x.Plugin_data[maxdatainfo.Key] < maxdatainfo.Value.version);
                maxdatainfo.Value.AnyOutfitOutdated = OutfitList.Any(x => !x.MissingMods && x.Plugin_data.ContainsKey(maxdatainfo.Key) && x.Plugin_data[maxdatainfo.Key] < maxdatainfo.Value.version);
            }

            switch (Tabvalue)
            {
                case 0:
                    CardInfo.CurrentMaxVersion = CardInfo.CurrentMaxVersion.OrderBy(x => !x.Value.AnyOutfitOutdated).ThenBy(x => x.Value.Name).ToDictionary(x => x.Key, x => x.Value);
                    break;

                case 1:
                    CardInfo.CurrentMaxVersion = CardInfo.CurrentMaxVersion.OrderBy(x => !x.Value.AnyCharaOutdated).ThenBy(x => x.Value.Name).ToDictionary(x => x.Key, x => x.Value);
                    break;
            }
        }

        private IEnumerator CharactersCardsUpdate(List<CardInfo> cardInfos, bool coordinates)
        {
            var sing = MakerAPI.GetMakerBase();
            UpdateInProgress = true;
            var testcontrol = new ChaFile();

            InProgressCount = 0;
            ProgressTotal = cardInfos.Count();

            for (; ; InProgressCount++)
            {
                NeedUpdate = BlackList = false;
                if (ForceStop || InProgressCount >= ProgressTotal)
                {
                    UpdateOutmiss();
                    UpdateVisibleGuids();
                    UpdateInProgress = false;
                    ForceStop = false;
                    yield break;
                }

                var reference = cardInfos[InProgressCount];

                if (InProgressCount % 50 == 0)
                    yield return 0;

                var card = CharacterList.First(x => x == reference);

                Waitforresolver = true;
                ChaControl.chaFile.LoadFileLimited(card.Path, ChaControl.sex);
                ChaControl.ChangeCoordinateType(true);
                ChaControl.Reload();
                sing.updateCustomUI = true;

                int yieldcount = -1;

                while (resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                {
                    yield return 0;
                }

                do yield return new WaitForEndOfFrame();
                while (ChaControl.animBody == null);

                yieldcount = 0;
                do
                {
                    yield return new WaitForEndOfFrame();
                } while (yieldcount++ < 5);

                if (coordinates)
                {
                    for (int j = 0, jn = ChaFileControl.coordinate.Length; j < jn; j++)
                    {
                        ChaControl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)j);
                        do yield return new WaitForEndOfFrame();
                        while (ChaControl.animBody == null);

                        yieldcount = 0;
                        do
                        {
                            yield return new WaitForEndOfFrame();
                        } while (yieldcount++ < 5);
                    }
                }

                WaitForSave = true;
                ChaFileControl.facePngData = MakerAPI.LastLoadedChaFile.facePngData;
                ChaFileControl.pngData = PngFile.LoadPngBytes(card.Path);
                ChaFileControl.SaveCharaFile(card.Path);

                while (WaitForSave)
                {
                    yield return 0;
                }

                NeedUpdate = BlackList = false;
                Waitforresolver = true;
                testcontrol.LoadFile(card.Path);
                yieldcount = -1;

                while (resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                {
                    yield return 0;
                }


                card.MissingMods = BlackList;

                card.OutdatedMods = NeedUpdate && !BlackList;

                var extendedData = ExtendedSave.GetAllExtendedData(testcontrol);
                if (extendedData != null)
                {
                    card.PopulateDictinary(extendedData);
                }
            }
        }

        private IEnumerator OutfitCardsUpdate(List<CardInfo> cardInfos)
        {
            var sing = MakerAPI.GetMakerBase();
            UpdateInProgress = true;
            var testcontrol = new ChaFileCoordinate();
            InProgressCount = 0;
            ProgressTotal = cardInfos.Count();
            interceptsave = true;
            for (; ; InProgressCount++)
            {
                if (ForceStop || InProgressCount >= ProgressTotal)
                {
                    UpdateOutmiss();
                    UpdateVisibleGuids();
                    UpdateInProgress = false;
                    ForceStop = false;
                    interceptsave = false;
                    yield break;
                }

                if (InProgressCount % 50 == 0)
                    yield return 0;

                var reference = cardInfos[InProgressCount];
                var card = OutfitList.First(x => x == reference);

                Waitforresolver = true;
                var coordinate = ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value];
                var nowcoordinate = ChaControl.nowCoordinate;
                coordinate.LoadFile(card.Path);
                nowcoordinate.LoadFile(card.Path);
                ChaControl.ChangeCoordinateTypeAndReload();
                sing.updateCustomUI = true;

                int yieldcount = -1;

                while (resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                {
                    yield return 0;
                }

                for (yieldcount = 0; yieldcount < 10; yieldcount++)
                {
                    yield return 0;
                }

                WaitForSave = true;
                interceptpath = Path.GetFullPath(card.Path);
                coordinate.pngData = PngFile.LoadPngBytes(interceptpath);
                coordinate.SaveFile(interceptpath);

                while (WaitForSave)
                {
                    yield return 0;
                }

                NeedUpdate = BlackList = false;
                Waitforresolver = true;
                testcontrol.LoadFile(card.Path);
                yieldcount = -1;

                while (resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                {
                    yield return 0;
                }

                card.MissingMods = BlackList;

                card.OutdatedMods = NeedUpdate && !BlackList;

                var extendedData = ExtendedSave.GetAllExtendedData(testcontrol);
                if (extendedData != null)
                {
                    card.PopulateDictinary(extendedData);
                }
            }
        }

        private bool UpdateByGuid(CardInfo card, string GUID, bool missingdata)
        {
            bool missing = card.MissingMods;
            bool containsdata = card.Plugin_data.TryGetValue(GUID, out int version);

            bool updateversion = missingdata && !containsdata || containsdata && version < CardInfo.CurrentMaxVersion[GUID].version;

            return !missing && updateversion;
        }
    }
}
