using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using UniRx;

namespace Additional_Card_Info
{
    public partial class CharaEvent
    {
        private static readonly MakerToggle[] CoordinateKeepToggles = new MakerToggle[Constants.ClothingTypesLength];
        private static readonly MakerToggle[] PersonalKeepToggles = new MakerToggle[Constants.ClothingTypesLength];

        private static readonly MakerRadioButtons[] PersonalityToggles =
            new MakerRadioButtons[Constants.PersonalityLength];

        private static readonly MakerRadioButtons[] TraitToggles = new MakerRadioButtons[Constants.TraitsLength];
        private static readonly MakerRadioButtons[] InterestToggles = new MakerRadioButtons[Constants.InterestLength];
        private static readonly MakerToggle[] HeightToggles = new MakerToggle[Constants.HeightLength];
        private static readonly MakerToggle[] BreastSizeToggles = new MakerToggle[Constants.BreastsizeLength];

        private static MakerToggle _characterCosplayReady;
        private static MakerToggle _makeUpToggle;
        private static MakerToggle _advanced;

        private static AccessoryControlWrapper<MakerToggle, bool> _accKeepToggles;
        private static AccessoryControlWrapper<MakerToggle, bool> _hairKeepToggles;

        private static MakerRadioButtons _coordinateTypeRadio;
        private static MakerRadioButtons _coordinateSubTypeRadio;
        private static MakerRadioButtons _clubTypeRadio;
        private static MakerRadioButtons _hStateTypeRadio;
        private static MakerRadioButtons _genderRadio;
        private static MakerTextbox _creator;
        private static MakerText _coordinateCreatorNames;
        private static MakerTextbox _setName;
        private static MakerTextbox _subSetName;

        private static readonly Dictionary<string, MakerTextbox> AdvancedCharacterOutfitFolders =
            new Dictionary<string, MakerTextbox>();

        private static MakerTextbox _simpleCharacterOutfitFolders;

        private static string _creatorName = Settings.CreatorName.Value;

        private static CharaEvent GetController => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public int ListInfoResult { get; internal set; }
        public ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;

            MakerAPI.MakerExiting += MakerAPI_MakerExiting;

            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            GetController.StartCoroutine(GetController.UpdateSlots());
        }

        private static void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;

            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
        }

        public static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;

            #region Personal Settings

            var category = new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3,
                "Settings");
            e.AddSubCategory(category);

            //e.AddControl(new MakerText(null, category, owner));
            e.AddControl(new MakerText(
                "Toggle when all Hair accessories and accessories you want to keep on this character are ready.",
                category, owner));
            e.AddControl(
                new MakerText("Example Mecha Chika who requires her arm and legs accessories", category, owner));

            e.AddControl(new MakerButton("Keep All Accessories", category, owner)).OnClick.AddListener(delegate
            {
                var controller = GetController;
                var accKeep = controller.AccKeep;
                for (int i = 0, n = _accKeepToggles.Control.ControlObjects.Count(); i < n; i++)
                    if (AccessoriesApi.GetPartsInfo(i).type != 120)
                    {
                        _accKeepToggles.SetValue(i, true);
                        if (!accKeep.Contains(i)) accKeep.Add(i);
                    }
            });

            _characterCosplayReady = e.AddControl(new MakerToggle(category, "Cosplay Academy Ready", false, owner));
            _characterCosplayReady.ValueChanged.Subscribe(value => GetController.CosplayReady = value);

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Select data that shouldn't be overwritten by other mods.", category, owner));
            e.AddControl(new MakerText(
                "Example Mecha Chika who doesn't work with socks/pantyhose and requires her shoes/glove", category,
                owner));

            #region Keep Toggles

            for (var i = 0; i < Constants.ClothingTypesLength; i++) PersonalClothingKeepControls(i, e);

            _makeUpToggle = e.AddControl(new MakerToggle(category, "Keep Makeup", owner));

            _makeUpToggle.ValueChanged.Subscribe(x => { GetController.MakeUpKeep = x; });

            e.AddControl(new MakerSeparator(category, owner));

            #endregion

            #region Folders

            _advanced = e.AddControl(new MakerToggle(category, "Advanced", false, owner));
            _advanced.ValueChanged.Subscribe(x =>
            {
                GetController.AdvancedDirectory = x;
                for (int i = 0, n = AdvancedCharacterOutfitFolders.Count; i < n; i++)
                {
                    var folderText = AdvancedCharacterOutfitFolders.ElementAt(i).Value;
                    if (folderText == null || folderText.IsDisposed) continue;

                    folderText.Visible.OnNext(x);
                }
            });
            e.AddControl(new MakerText("Custom Character Outfit Folders", category, owner));
            _simpleCharacterOutfitFolders =
                e.AddControl(new MakerTextbox(category, "Simplified All Folders", "", owner));
            _simpleCharacterOutfitFolders.ValueChanged.Subscribe(x => { GetController.SimpleFolderDirectory = x; });
            for (int i = 0, n = Constants.SimplifiedCoordinateTypesLength; i < n; i++)
                CustomOutfitFoldersControls(i, e);

            AdvancedCharacterOutfitFolders["Underwear"] = e.AddControl(new MakerTextbox(
                new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings"),
                "Underwear", "", Settings.Instance));
            AdvancedCharacterOutfitFolders["Underwear"].ValueChanged.Subscribe(x =>
            {
                GetController.AdvancedFolderDirectory["Underwear"] = x;
            });
            AdvancedCharacterOutfitFolders["Underwear"].Visible.OnNext(false);

            #endregion

            #endregion

            #region Accessory Window Settings

            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2,
                "Accessory Settings");
            var keep = new MakerToggle(category, "Keep this Accessory", owner);
            _accKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(keep, true);
            _accKeepToggles.ValueChanged += AccKeep_ValueChanged;

            var hairKeep = new MakerToggle(category, "Is this a hair piece?", owner);
            _hairKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(hairKeep, true);
            _hairKeepToggles.ValueChanged += AccHairKeep_ValueChanged;

            #endregion

            #region Clothing Settings

            category = new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2,
                "Clothing Settings");
            e.AddSubCategory(category);

            e.AddControl(new MakerText("Settings to be applied to coordinates", category, owner));

            e.AddControl(new MakerSeparator(category, owner));

            _coordinateCreatorNames = e.AddControl(new MakerText("", category, owner));

            _creator = new MakerTextbox(category, "Author", Settings.CreatorName.Value, owner);
            _setName = new MakerTextbox(category, "Set Name", "", owner);
            _subSetName = new MakerTextbox(category, "Sub Set Name", "", owner);
            e.AddControl(_creator).ValueChanged.Subscribe(x => { _creatorName = x; });
            e.AddControl(_setName).ValueChanged.Subscribe(x => { GetController.SetNames = x; });
            e.AddControl(_subSetName).ValueChanged.Subscribe(x => { GetController.SubSetNames = x; });


            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Coordinate Type information", category, owner));

            var simpleCoord = new List<string> { "Unassigned" };
            simpleCoord.AddRange(Enum.GetNames(typeof(Constants.SimplifiedCoordinateTypes)));
            for (int i = 0, n = simpleCoord.Count; i < n; i++) simpleCoord[i] = simpleCoord[i].Replace('_', ' ');

            _coordinateTypeRadio = new MakerRadioButtons(category, owner, "Coordinate Type", 0, simpleCoord.ToArray())
            {
                ColumnCount = 2
            };
            e.AddControl(_coordinateTypeRadio).ValueChanged.Subscribe(x => { GetController.CoordinateType = x; });

            var buttons = new List<string> { "Unassigned", "General" };
            buttons.AddRange(Enum.GetNames(typeof(Constants.ClothingTypes)));
            buttons.AddRange(Enum.GetNames(typeof(Constants.AdditonalClothingTypes)));
            for (int i = 0, n = buttons.Count; i < n; i++) buttons[i] = buttons[i].Replace('_', ' ');

            e.AddControl(new MakerText(null, category, owner));
            _coordinateSubTypeRadio = new MakerRadioButtons(category, owner, "Sub Type", 0, buttons.ToArray())
            {
                ColumnCount = 3
            };
            e.AddControl(_coordinateSubTypeRadio).ValueChanged.Subscribe(x => { GetController.CoordinateSubType = x; });

            e.AddControl(new MakerText(null, category, owner));

            _genderRadio = new MakerRadioButtons(category, owner, "Gender", "Female", "Both", "Male");
            e.AddControl(_genderRadio).ValueChanged.Subscribe(x => { GetController.GenderType = x; });


            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("H State Restrictions", category, owner));
            var hStatesOptions = Enum.GetNames(typeof(Constants.HStates));
            for (var i = 0; i < hStatesOptions.Length; i++) hStatesOptions[i] = hStatesOptions[i].Replace('_', ' ');

            _hStateTypeRadio = new MakerRadioButtons(category, owner, "H State", 0, hStatesOptions)
            {
                ColumnCount = 3
            };
            e.AddControl(_hStateTypeRadio).ValueChanged.Subscribe(x => { GetController.HStateTypeRestriction = x; });

            //e.AddControl(new MakerText(null, category, owner));

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Club Restrictions", category, owner));
            var clubOptions = Enum.GetNames(typeof(Constants.Club));
            for (var i = 0; i < clubOptions.Length; i++) clubOptions[i] = clubOptions[i].Replace('_', ' ');

            _clubTypeRadio = new MakerRadioButtons(category, owner, "Club", 0, clubOptions)
            {
                ColumnCount = 3
            };

            e.AddControl(_clubTypeRadio).ValueChanged.Subscribe(x => { GetController.ClubTypeRestriction = x; });

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Non-replaceable", category, owner));
            for (var clothingInt = 0; clothingInt < 9; clothingInt++) ClothingKeepControls(clothingInt, e);

            #endregion

            e.AddControl(new MakerSeparator(category, owner));

            e.AddControl(new MakerText("Breast Size Restrictions (exclusive)", category, owner));
            for (var i = 0; i < Constants.BreastsizeLength; i++) BreastSizeRestrictionControls(i, e);

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Height Restrictions (exclusive)", category, owner));

            for (var i = 0; i < Constants.HeightLength; i++) HeightRestrictionControls(i, e);

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Interest Restrictions: Koi_Sun", category, owner));

            for (var i = 0; i < Constants.InterestLength; i++) InterestsRestrictionControls(i, e);

            #region Personality Restrictions

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Personality Restrictions", category, owner));
            for (int personNum = 0, n = Enum.GetNames(typeof(Constants.Personality)).Length; personNum < n; personNum++)
                PersonalityRestrictionControls(personNum, e);

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Trait Restrictions", category, owner));
            for (int traitNum = 0, n = Enum.GetNames(typeof(Constants.Traits)).Length; traitNum < n; traitNum++)
                TraitRestrictionControls(traitNum, e);

            #endregion

            var groupingID = "Maker_Tools_" + Settings.NamingID.Value;
            _accKeepToggles.Control.GroupingID = groupingID;
            _hairKeepToggles.Control.GroupingID = groupingID;
        }

        private static void AccHairKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            var controller = GetController;
            if (controller.HairAcc == null) return;

            if (e.NewValue)
                controller.HairAcc.Add(e.SlotIndex);
            else
                controller.HairAcc.Remove(e.SlotIndex);
        }

        private static void AccKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            var controller = GetController;
            if (controller.AccKeep == null) return;

            if (e.NewValue)
                controller.AccKeep.Add(e.SlotIndex);
            else
                controller.AccKeep.Remove(e.SlotIndex);
        }

        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if (type == 120)
            {
                _accKeepToggles.SetValue(slotNo, false, false);
                _hairKeepToggles.SetValue(slotNo, false, false);
                HairAcc.Remove(slotNo);
                AccKeep.Remove(slotNo);
            }
        }

        private IEnumerator UpdateSlots()
        {
            if (!MakerAPI.InsideMaker) yield break;

            do
            {
                yield return null;
            } while (!MakerAPI.InsideAndLoaded);

            for (var i = 0; i < PersonalClothingBools.Length; i++)
                PersonalKeepToggles[i].SetValue(PersonalClothingBools[i], false);

            for (int i = 0, n = Parts.Length; i < n; i++)
            {
                _accKeepToggles.SetValue(i, AccKeep.Contains(i), false);
                _hairKeepToggles.SetValue(i, HairAcc.Contains(i), false);
            }

            for (var i = 0; i < CoordinateKeepToggles.Length; i++)
                CoordinateKeepToggles[i].SetValue(CoordinateSaveBools[i], false);

            for (var i = 0; i < PersonalityToggles.Length; i++)
            {
                PersonalityTypeRestriction.TryGetValue(i, out var value);
                PersonalityToggles[i].SetValue(++value, false);
            }

            for (var i = 0; i < TraitToggles.Length; i++)
            {
                TraitTypeRestriction.TryGetValue(i, out var value);
                TraitToggles[i].SetValue(++value, false);
            }

            for (var i = 0; i < HeightToggles.Length; i++) HeightToggles[i].SetValue(HeightRestriction[i], false);

            for (var i = 0; i < BreastSizeToggles.Length; i++)
                BreastSizeToggles[i].SetValue(BreastsizeRestriction[i], false);

            for (var i = 0; i < Constants.InterestLength; i++)
            {
                InterestRestriction.TryGetValue(i, out var value);
                InterestToggles[i].SetValue(++value, false);
            }

            UpdateClothingNots();

            _characterCosplayReady.SetValue(CosplayReady);

            var creators = "[None]";
            if (CreatorNames.Count > 0)
            {
                creators = "";
                foreach (var item in CreatorNames) creators += $"[{item}] > ";

                creators = creators.Remove(creators.Length - 3);
            }

            _coordinateCreatorNames.Text = creators;


            _advanced.SetValue(AdvancedDirectory, false);

            _simpleCharacterOutfitFolders.SetValue(SimpleFolderDirectory, false);

            for (int i = 0, n = AdvancedCharacterOutfitFolders.Count; i < n; i++)
            {
                var textbox = AdvancedCharacterOutfitFolders.ElementAt(i);
                if (AdvancedFolderDirectory.TryGetValue(textbox.Key, out var foldername))
                {
                    textbox.Value.SetValue(foldername, false);
                    continue;
                }

                textbox.Value.SetValue("", false);
            }

            _makeUpToggle.SetValue(MakeUpKeep, false);
            _coordinateTypeRadio.SetValue(CoordinateType, false);
            _coordinateSubTypeRadio.SetValue(CoordinateSubType, false);
            _clubTypeRadio.SetValue(ClubTypeRestriction, false);
            _hStateTypeRadio.SetValue(HStateTypeRestriction, false);
            _setName.SetValue(SetNames, false);
        }

        internal void UpdateClothingNots()
        {
            if (!MakerAPI.InsideMaker) return;

            var clothingnot = ClothNotData;
            for (int i = 0, n = clothingnot.Length; i < n; i++) clothingnot[i] = false;

            var clothinfo = ChaControl.infoClothes;

            UpdateTopClothingNots(clothinfo[0], ref clothingnot);
            UpdateBraClothingNots(clothinfo[2], ref clothingnot);
        }

        private void UpdateTopClothingNots(ListInfoBase infoBase, ref bool[] clothingnot)
        {
            if (infoBase == null) return;

            Hooks.ClothingNotPatch.IsShortsCheck = false;
            var listInfoResult = Hooks.ClothingNotPatch.ListInfoResult;
            var key = ChaListDefine.KeyType.Coordinate;
            infoBase.GetInfo(key);
            var notbot = listInfoResult[key] == 2; //only in ChangeClothesTopAsync
            key = ChaListDefine.KeyType.NotBra;
            infoBase.GetInfo(key);
            var notbra = listInfoResult[key] == 1; //only in ChangeClothesTopAsync
            clothingnot[0] = clothingnot[0] || notbot;
            clothingnot[1] = clothingnot[1] || notbra;
        }

        private void UpdateBraClothingNots(ListInfoBase infoBase, ref bool[] clothingNot)
        {
            if (infoBase == null) return;

            Hooks.ClothingNotPatch.IsShortsCheck = true;

            var listInfoResult = Hooks.ClothingNotPatch.ListInfoResult;
            const ChaListDefine.KeyType key = ChaListDefine.KeyType.Coordinate;

            infoBase.GetInfo(key);

            var notShorts = listInfoResult[key] == 2; //only in ChangeClothesBraAsync
            clothingNot[2] = clothingNot[2] || notShorts;
        }

        #region Control Creation

        private static void CustomOutfitFoldersControls(int clothingInt, RegisterSubCategoriesEvent e)
        {
            var stringname = ((Constants.SimplifiedCoordinateTypes)clothingInt).ToString().Replace('_', ' ');
            AdvancedCharacterOutfitFolders[stringname] = e.AddControl(new MakerTextbox(
                new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings"),
                stringname, "", Settings.Instance));
            AdvancedCharacterOutfitFolders[stringname].ValueChanged.Subscribe(x =>
            {
                GetController.AdvancedFolderDirectory[stringname] = x;
            });
            AdvancedCharacterOutfitFolders[stringname].Visible.OnNext(false);
        }

        private static void PersonalClothingKeepControls(int clothingInt, RegisterSubCategoriesEvent e)
        {
            PersonalKeepToggles[clothingInt] = e.AddControl(new MakerToggle(
                new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings"),
                $"Keep {((Constants.ClothingTypes)clothingInt).ToString().Replace('_', ' ')}", false,
                Settings.Instance));
            PersonalKeepToggles[clothingInt].ValueChanged.Subscribe(x =>
            {
                GetController.PersonalClothingBools[clothingInt] = x;
            });
        }

        private static void PersonalityRestrictionControls(int personalityNum, RegisterSubCategoriesEvent e)
        {
            PersonalityToggles[personalityNum] = e.AddControl(new MakerRadioButtons(
                new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2,
                    "Clothing Settings"), Settings.Instance,
                ((Constants.Personality)personalityNum).ToString().Replace('_', ' '), 1, "Exclude", "Neutral",
                "Include"));
            PersonalityToggles[personalityNum].ValueChanged.Subscribe(x =>
            {
                GetController.PersonalityTypeRestriction[personalityNum] = --x;
            });
        }

        private static void TraitRestrictionControls(int traitNum, RegisterSubCategoriesEvent e)
        {
            TraitToggles[traitNum] = e.AddControl(new MakerRadioButtons(
                new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2,
                    "Clothing Settings"), Settings.Instance, ((Constants.Traits)traitNum).ToString().Replace('_', ' '),
                1, "Exclude", "Neutral", "Include"));
            TraitToggles[traitNum].ValueChanged.Subscribe(x => { GetController.TraitTypeRestriction[traitNum] = --x; });
        }

        private static void InterestsRestrictionControls(int interestNum, RegisterSubCategoriesEvent e)
        {
            InterestToggles[interestNum] = e.AddControl(new MakerRadioButtons(
                new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2,
                    "Clothing Settings"), Settings.Instance,
                ((Constants.Interests)interestNum).ToString().Replace('_', ' '), 1, "Exclude", "Neutral", "Include"));
            InterestToggles[interestNum].ValueChanged.Subscribe(x =>
            {
                GetController.InterestRestriction[interestNum] = --x;
            });
        }

        private static void ClothingKeepControls(int clothingInt, RegisterSubCategoriesEvent e)
        {
            CoordinateKeepToggles[clothingInt] = e.AddControl(new MakerToggle(
                new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2,
                    "Clothing Settings"), $"Keep {((Constants.ClothingTypes)clothingInt).ToString().Replace('_', ' ')}",
                false, Settings.Instance));
            CoordinateKeepToggles[clothingInt].ValueChanged.Subscribe(x =>
            {
                GetController.CoordinateSaveBools[clothingInt] =
                    x; /*Settings.Logger.LogWarning($"Changing Outfit num restriction {(Constants.ClothingTypes)ClothingInt}");*/
            });
        }

        private static void BreastSizeRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            BreastSizeToggles[size] = e.AddControl(new MakerToggle(
                new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2,
                    "Clothing Settings"), $"{(Constants.Breastsize)size}", false, Settings.Instance));
            BreastSizeToggles[size].ValueChanged.Subscribe(x => { GetController.BreastsizeRestriction[size] = x; });
        }

        private static void HeightRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            HeightToggles[size] = e.AddControl(new MakerToggle(
                new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2,
                    "Clothing Settings"), $"{(Constants.Height)size}", false, Settings.Instance));
            HeightToggles[size].ValueChanged.Subscribe(x => { GetController.HeightRestriction[size] = x; });
        }

        #endregion

        #region external event

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            GetController.AccessoriesTransfered(e);
        }

        private void AccessoriesTransfered(AccessoryTransferEventArgs e)
        {
            if (HairAcc.Contains(e.SourceSlotIndex))
                if (!HairAcc.Contains(e.DestinationSlotIndex))
                    HairAcc.Add(e.DestinationSlotIndex);

            if (AccKeep.Contains(e.SourceSlotIndex))
                if (!AccKeep.Contains(e.DestinationSlotIndex))
                    AccKeep.Add(e.DestinationSlotIndex);

            _hairKeepToggles.SetValue(e.DestinationSlotIndex, _hairKeepToggles.GetValue(e.SourceSlotIndex));
            _accKeepToggles.SetValue(e.DestinationSlotIndex, _accKeepToggles.GetValue(e.SourceSlotIndex));
        }

        private void AccessoriesCopied(AccessoryCopyEventArgs e)
        {
            var copiedSlots = e.CopiedSlotIndexes.ToArray();
            var source = CoordinateInfo[(int)e.CopySource];
            var dest = CoordinateInfo[(int)e.CopyDestination];

            for (var i = 0; i < copiedSlots.Length; i++)
            {
                if (source.accKeep.Contains(copiedSlots[i]))
                {
                    if (!dest.hairAcc.Contains(copiedSlots[i]))
                        dest.accKeep.Add(copiedSlots[i]);
                }
                else
                {
                    dest.accKeep.Remove(copiedSlots[i]);
                }

                //Settings.Logger.LogWarning($"HairKeep");
                if (source.hairAcc.Contains(copiedSlots[i]))
                {
                    if (!dest.hairAcc.Contains(copiedSlots[i]))
                        dest.hairAcc.Add(copiedSlots[i]);
                }
                else
                {
                    dest.hairAcc.Remove(copiedSlots[i]);
                }
            }
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            GetController.AccessoriesCopied(e);
        }

        internal void MovIt(List<QueueItem> queue)
        {
            var acckeep = AccKeep;
            var hairkeep = HairAcc;
            foreach (var item in queue)
            {
                if (acckeep.Contains(item.SrcSlot))
                {
                    acckeep.Add(item.DstSlot);
                    acckeep.Remove(item.SrcSlot);
                    _accKeepToggles.SetValue(item.DstSlot, true, false);
                    _accKeepToggles.SetValue(item.SrcSlot, false, false);
                }

                if (hairkeep.Contains(item.SrcSlot))
                {
                    hairkeep.Add(item.DstSlot);
                    hairkeep.Remove(item.SrcSlot);
                    _hairKeepToggles.SetValue(item.DstSlot, true, false);
                    _hairKeepToggles.SetValue(item.SrcSlot, false, false);
                }
            }
        }

        internal void RemoveOutfitEvent()
        {
            Data.RemoveOutfit(MaxKey);
        }

        internal void AddOutfitEvent()
        {
            for (var i = MaxKey; i < ChaFileControl.coordinate.Length; i++)
                Data.CreateOutfit(i);
        }

        #endregion
    }
}