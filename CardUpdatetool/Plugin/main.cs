using ExtensibleSaveFormat;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;

namespace CardUpdateTool
{
    public partial class CardUpdateTool
    {
        internal void Exiting()
        {
            CharacterData.Clear();
            OutfitData.Clear();
            GuidList.Clear();
            StaticMaxVersion.Clear();
            ShowGui = false;
        }

        internal void Starting()
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
            CharacterPath = Path.GetFullPath(CharacterPath);
            GetAllGuids();
        }

        internal void Register(object sender, RegisterSubCategoriesEvent e)
        {
            toggle = e.AddSidebarControl(new SidebarToggle("Card Update Tool", ShowGui, Instance));
            toggle.ValueChanged.Subscribe(X => ShowGui = X);
        }

        private void UpdateOutmiss()
        {
            CharacterData.UpdateOutMiss();
            OutfitData.UpdateOutMiss();
            MissingVersionCheck();
        }

        private static void UpdateVisibleGuids()
        {
            foreach (var item in Constants.ReadableGuid.Where(x => !StaticMaxVersion.ContainsKey(x.Key) && GuidList.Contains(x.Key)))
            {
                StaticMaxVersion[item.Key] = new VersionData(item.Value.Name, item.Value.KnownVersion) { CharaVisible = item.Value.Charavisible, OutfitVisible = item.Value.Outfitvisible };
            }
            foreach (var guid in StaticMaxVersion.Where(x => !x.Value.CharaVisible || !x.Value.OutfitVisible))
            {
                guid.Value.CharaVisible = guid.Value.CharaVisible || CharacterList.Any(x => x.Plugin_data.ContainsKey(guid.Key));
                guid.Value.OutfitVisible = guid.Value.OutfitVisible || OutfitList.Any(x => x.Plugin_data.ContainsKey(guid.Key));
            }
        }

        private static void GetAllGuids()
        {
            GuidList = CharacterApi.RegisteredHandlers.Select(x => x.ExtendedDataId).ToList();
            GuidList.Add("moreAccessories");
            UpdateVisibleGuids();
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
            if (!interceptsave || path == interceptpath)//just in case there are other attempts during save. 
            {
                return;
            }
            Logger.LogWarning($"Intercepted and deleted {path}");
            File.Delete(path);
        }

        private void MissingVersionCheck()
        {
            CharacterData.NullCheck();
            OutfitData.NullCheck();

            foreach (var maxdatainfo in StaticMaxVersion)
            {
                var charafilter = CharacterList.Where(x => !x.MissingMods && !x.OutdatedMods && x.Plugin_data.ContainsKey(maxdatainfo.Key));
                var outfitfilter = OutfitList.Where(x => !x.MissingMods && !x.OutdatedMods && x.Plugin_data.ContainsKey(maxdatainfo.Key));

                maxdatainfo.Value.CharaUpToDate = charafilter.Count(x => x.Plugin_data[maxdatainfo.Key] == maxdatainfo.Value.version);
                maxdatainfo.Value.CharaOutdatedCount = charafilter.Count(x => x.Plugin_data[maxdatainfo.Key] < maxdatainfo.Value.version);

                maxdatainfo.Value.OutfitsUpToDate = outfitfilter.Count(x => x.Plugin_data[maxdatainfo.Key] == maxdatainfo.Value.version);
                maxdatainfo.Value.OutfitOutdatedCount = outfitfilter.Count(x => x.Plugin_data[maxdatainfo.Key] < maxdatainfo.Value.version);
            }

            OutfitData.PrintOrder = StaticMaxVersion.OrderBy(x => !x.Value.AnyOutfitOutdated).ThenByDescending(x => x.Value.OutfitOutdatedCount).ThenBy(x => x.Value.Name).Select(x => x.Key).ToList();
            CharacterData.PrintOrder = StaticMaxVersion.OrderBy(x => !x.Value.AnyCharaOutdated).ThenByDescending(x => x.Value.CharaOutdatedCount).ThenBy(x => x.Value.Name).Select(x => x.Key).ToList();
        }

        private void Stopcommand()
        {
            UpdateVisibleGuids();
            UpdateOutmiss();
            UpdateInProgress = false;
            ForceStop = false;
            interceptsave = false;
        }

        private void CharacterCardCheck(string StartPath)
        {
            CharacterData.CardInfoListConvert(DirectoryFinder.Get_Cards_From_Path(StartPath));
            StartCoroutine(test());

            IEnumerator<int> test()
            {
                UpdateInProgress = true;
                var testcontrol = new ChaFile();
                InProgressCount = 0;
                ProgressTotal = CharacterList.Count;

                var csv = OpenCSV(StartPath);
                if (csv == null) ForceStop = true;

                for (; InProgressCount < ProgressTotal && !ForceStop; InProgressCount++)
                {
                    if (InProgressCount % 50 == 0)
                        yield return 0;

                    Migrateable = NeedUpdate = BlackList = false;

                    var card = CharacterList[InProgressCount];
                    Waitforresolver = true;

                    testcontrol.LoadFile(card.Path);
                    var yieldcount = -1;
                    while (Resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                    {
                        yield return 0;
                    }

                    if (testcontrol.lastLoadErrorCode != 0)
                    {
                        card.lasterror = testcontrol.lastLoadErrorCode;
                        MoveToNewDirectory(StartPath, card, "/BadCardData/", ref csv);
                        CharacterList.RemoveAt(InProgressCount);
                        InProgressCount--;
                        ProgressTotal--;
                    }
                    else
                        Dataloading(ref card, ExtendedSave.GetAllExtendedData(testcontrol));

                    if (NeedUpdate || BlackList)
                    {
                        AddToCSV(ref csv, card.Path, CSVLine(card, card.Path));
                    }

                    if (ForceStop)
                    {
                        var removecount = ProgressTotal - InProgressCount;
                        if (removecount > 0)
                            CharacterList.RemoveRange(InProgressCount, removecount);
                    }
                }
                if (csv != null) WriteCSV(StartPath, csv);
                Stopcommand();
            }
        }

        private void OutfitCardCheck(string StartPath)
        {
            OutfitData.CardInfoListConvert(DirectoryFinder.Get_Cards_From_Path(StartPath));

            StartCoroutine(test());
            IEnumerator<int> test()
            {
                UpdateInProgress = true;
                var testcontrol = new ChaFileCoordinate();
                InProgressCount = 0;
                ProgressTotal = OutfitList.Count;

                var csv = OpenCSV(StartPath);
                if (csv == null) ForceStop = true;

                for (; InProgressCount < ProgressTotal && !ForceStop; InProgressCount++)
                {
                    if (InProgressCount % 50 == 0)
                        yield return 0;

                    Migrateable = NeedUpdate = BlackList = false;

                    var card = OutfitList[InProgressCount];
                    Waitforresolver = true;

                    testcontrol.LoadFile(card.Path);
                    var yieldcount = -1;

                    while (Resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                    {
                        yield return 0;
                    }

                    if (testcontrol.lastLoadErrorCode != 0)
                    {
                        card.lasterror = testcontrol.lastLoadErrorCode;

                        MoveToNewDirectory(StartPath, card, "/BadCardData/", ref csv);
                        OutfitList.RemoveAt(InProgressCount);
                        InProgressCount--;
                        ProgressTotal--;
                        continue;
                    }

                    Dataloading(ref card, ExtendedSave.GetAllExtendedData(testcontrol));

                    if (NeedUpdate || BlackList)
                    {
                        AddToCSV(ref csv, card.Path, CSVLine(card, card.Path));
                    }

                    if (ForceStop)
                    {
                        var removecount = ProgressTotal - InProgressCount;
                        if (removecount > 0)
                            OutfitList.RemoveRange(InProgressCount, removecount);
                    }
                }
                if (csv != null) WriteCSV(StartPath, csv);
                Stopcommand();
            }
        }

        private IEnumerator CharactersCardsUpdate(List<CardInfo> cardInfos, bool coordinates)
        {
            var sing = MakerAPI.GetMakerBase();
            UpdateInProgress = true;
            var testcontrol = new ChaFile();

            InProgressCount = 0;
            ProgressTotal = cardInfos.Count();

            var csv = OpenCSV(CharacterPath);
            if (csv == null) ForceStop = true;

            for (; !ForceStop && InProgressCount < ProgressTotal; InProgressCount++)
            {
                var reference = cardInfos[InProgressCount];

                if (InProgressCount % 50 == 0)
                    yield return 0;

                var card = CharacterList.First(x => x == reference);

                Waitforresolver = true;
                ChaControl.chaFile.LoadFileLimited(card.Path, ChaControl.sex);
                ChaControl.ChangeCoordinateType(true);
                ChaControl.Reload();
                sing.updateCustomUI = true;

                var yieldcount = -1;

                while (Resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                {
                    yield return 0;
                }

                do yield return 0;
                while (ChaControl.animBody == null);

                yieldcount = 0;
                do
                {
                    yield return 0;
                } while (yieldcount++ < 10);

                if (coordinates)
                {
                    for (int j = 0, jn = ChaFileControl.coordinate.Length; j < jn; j++)
                    {
                        ChaControl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)j);
                        do yield return 0;
                        while (ChaControl.animBody == null);

                        yieldcount = 0;
                        do
                        {
                            yield return 0;
                        } while (yieldcount++ < 5);
                    }
                }

                if (Pause)
                {
                    Waiting = true;
                    while (Waiting) yield return 0;
                }

                WaitForSave = true;
                ChaFileControl.facePngData = MakerAPI.LastLoadedChaFile.facePngData;
                ChaFileControl.pngData = PngFile.LoadPngBytes(card.Path);
                ChaFileControl.SaveCharaFile(card.Path);

                while (WaitForSave)
                {
                    yield return 0;
                }

                Migrateable = NeedUpdate = BlackList = false;
                Waitforresolver = true;
                testcontrol.LoadFile(card.Path);
                yieldcount = -1;

                while (Resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                {
                    yield return 0;
                }

                Dataloading(ref card, ExtendedSave.GetAllExtendedData(testcontrol));

                if (NeedUpdate || BlackList)
                {
                    AddToCSV(ref csv, card.Path, CSVLine(card, card.Path));
                }
            }
            if (csv != null) WriteCSV(CharacterPath, csv);
            Stopcommand();
        }

        private IEnumerator OutfitCardsUpdate(List<CardInfo> cardInfos)
        {
            var sing = MakerAPI.GetMakerBase();

            UpdateInProgress = true;
            var testcontrol = new ChaFileCoordinate();
            InProgressCount = 0;
            ProgressTotal = cardInfos.Count();
            interceptsave = true;

            var csv = OpenCSV(CharacterPath);
            if (csv == null) ForceStop = true;

            for (; !ForceStop && InProgressCount < ProgressTotal; InProgressCount++)
            {
                if (InProgressCount % 50 == 0)
                    yield return 0;

                var reference = cardInfos[InProgressCount];
                var card = OutfitList.First(x => x == reference);

                Waitforresolver = true;
                var coordinate = ChaControl.chaFile.coordinate[CurrentCoordinate];
                var nowcoordinate = ChaControl.nowCoordinate;
                coordinate.LoadFile(card.Path);
                nowcoordinate.LoadFile(card.Path);
                ChaControl.ChangeCoordinateTypeAndReload();
                sing.updateCustomUI = true;

                var yieldcount = -1;

                while (Resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                {
                    yield return 0;
                }

                for (yieldcount = 0; yieldcount < 10; yieldcount++)
                {
                    yield return 0;
                }

                if (Pause)
                {
                    Waiting = true;
                    while (Waiting) yield return 0;
                }

                WaitForSave = true;
                interceptpath = Path.GetFullPath(card.Path);
                coordinate.pngData = PngFile.LoadPngBytes(interceptpath);
                File.Delete(interceptpath);//delete so recyclebin mod will move it; will just be replaced otherwise
                coordinate.SaveFile(interceptpath);

                while (WaitForSave)
                {
                    yield return 0;
                }

                Migrateable = NeedUpdate = BlackList = false;
                Waitforresolver = true;
                testcontrol.LoadFile(card.Path);
                yieldcount = -1;

                while (Resolverinprogress || (Waitforresolver && yieldcount++ < 10))
                {
                    yield return 0;
                }

                Dataloading(ref card, ExtendedSave.GetAllExtendedData(testcontrol));

                if (NeedUpdate || BlackList)
                {
                    AddToCSV(ref csv, card.Path, CSVLine(card, card.Path));
                }
            }
            if (csv != null) WriteCSV(CoordinatePath, csv);
            Stopcommand();
        }

        private void Dataloading(ref CardInfo card, Dictionary<string, PluginData> extendeddata)
        {
            card.MissingList = MissingList;
            card.OutdatedList = OutdatedList;

            card.MissingMods = BlackList;
            card.OutdatedMods = NeedUpdate && !BlackList;
            card.MigratedMods = Migrateable && !NeedUpdate && !BlackList;

            if (extendeddata != null)
            {
                card.PopulateDictinary(extendeddata);
            }
        }

        private bool UpdateByGuid(CardInfo card, string GUID, bool missingdata)
        {
            if (card.MissingMods || card.OutdatedMods)
            {
                return false;
            }
            var containsdata = card.Plugin_data.TryGetValue(GUID, out var version);

            var updateversion = missingdata && !containsdata || containsdata && version < StaticMaxVersion[GUID].version;

            return updateversion;
        }

        private List<string> OpenCSV(string path)
        {
            path = Path.GetFullPath(path + "/CardUpdateTool.csv");

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Logger.LogMessage("Directory Path doesn't exist");
                return null;
            }

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            try
            {
                var csvlist = File.ReadAllLines(path).ToList();
                if (csvlist.Count == 0)
                {
                    csvlist.Add("sep=;");
                    var header = "Current Path;";
                    header += "Folder;";
                    header += "Outdated count;";
                    header += "Outdated mods;";
                    header += "Missing count;";
                    header += "Missing mods;";
                    header += "Error Code;";
                    csvlist.Add(header);
                }

                return csvlist;
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("Sharing violation on path"))
                {
                    Logger.LogMessage("Read-only folder or File Open Error");
                    return null;
                }
                Logger.LogError(ex);
            }
            catch (System.Exception ex)
            {
                Logger.LogError(ex);
            }
            return null;
        }

        private void MoveToNewDirectory(string startpath, CardInfo card, string Dest, ref List<string> stringlist)
        {
            var source = card.Path;
            var newlocation = Path.GetFullPath(startpath + source.Replace(startpath, Dest));
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

            RemovefromCSV(ref stringlist, source);

            AddToCSV(ref stringlist, newlocation, CSVLine(card, newlocation));
        }

        private string CSVLine(CardInfo card, string fullpath)
        {
            var result = $"=HYPERLINK(\"{fullpath}\",\"{Path.GetFileName(fullpath)}\");";
            result += $"=HYPERLINK(\"{Path.GetDirectoryName(fullpath)}\");";
            result += $"{card.OutdatedList.Count} ;";
            if (card.OutdatedList.Count > 0)
            {
                foreach (var item in card.OutdatedList)
                {
                    result += $"{item}, ";
                }
                result = result.Remove(result.Length - 2);
            }
            result += " ;";

            result += $"{card.MissingList.Count} ;";
            if (card.MissingList.Count > 0)
            {
                foreach (var item in card.MissingList)
                {
                    result += $"{item}, ";
                }
                result = result.Remove(result.Length - 2);
            }
            result += " ;";

            result += $"{card.lasterror} ;";
            return result;
        }

        private void RemovefromCSV(ref List<string> stringlist, string originalpath)
        {
            var index = stringlist.FindIndex(x => x.Contains(originalpath));
            if (index < 0)
            {
                return;
            }
            stringlist.RemoveAt(index);
        }

        private void AddToCSV(ref List<string> stringlist, string path, string csvline)
        {
            var index = stringlist.FindIndex(x => x.Contains(path));
            if (index < 0)
            {
                stringlist.Add(csvline);
                return;
            }
            stringlist[index] = csvline;

        }

        private void WriteCSV(string path, List<string> csvlist)
        {
            path = Path.GetFullPath(path + "/CardUpdateTool.csv");
            if (!Directory.Exists(Path.GetDirectoryName(path)) || csvlist == null)
            {
                return;
            }
            File.WriteAllLines(path, csvlist.ToArray());
        }
    }
}