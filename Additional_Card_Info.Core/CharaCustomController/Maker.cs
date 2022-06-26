using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static readonly MakerToggle[] CoordinateKeepToggles = new MakerToggle[Constants.ClothingTypesLength];
        static readonly MakerToggle[] PersonalKeepToggles = new MakerToggle[Constants.ClothingTypesLength];
        static readonly MakerRadioButtons[] PersonalityToggles = new MakerRadioButtons[Constants.PersonalityLength];
        static readonly MakerRadioButtons[] TraitToggles = new MakerRadioButtons[Constants.TraitsLength];
        static readonly MakerRadioButtons[] InterestToggles = new MakerRadioButtons[Constants.InterestLength];
        static readonly MakerToggle[] HeightToggles = new MakerToggle[Constants.HeightLength];
        static readonly MakerToggle[] BreastsizeToggles = new MakerToggle[Constants.BreastsizeLength];
        static MakerToggle Character_Cosplay_Ready;
        static MakerToggle MakeUpToggle;
        static MakerToggle Advanced;

        static MakerRadioButtons AccKeepRadio;
        static MakerRadioButtons CoordinateTypeRadio;
        static MakerRadioButtons CoordinateSubTypeRadio;
        static MakerRadioButtons ClubTypeRadio;
        static MakerRadioButtons HStateTypeRadio;
        static MakerRadioButtons GenderRadio;
        static MakerTextbox Creator;
        static MakerText CoordinateCreatorNames;
        static MakerTextbox Set_Name;
        static MakerTextbox Sub_Set_Name;

        static readonly Dictionary<string, MakerTextbox> AdvancedCharacterOutfitFolders = new Dictionary<string, MakerTextbox>();
        static MakerTextbox SimpleCharacterOutfitFolders;

        static CharaEvent GetController => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public int ListInfoResult { get; internal set; }
        public ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;

        static string Creatorname = Settings.CreatorName.Value;

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.SelectedMakerAccSlotChanged += AccessoriesApi_SelectedMakerAccSlotChanged;
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;

            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            GetController.UpdateSlots();
        }

        private static void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;

            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
        }

        private static void AccessoriesApi_SelectedMakerAccSlotChanged(object sender, AccessorySlotEventArgs e) => GetController.SelectedMakerAccSlotChanged(e.SlotIndex);

        private void SelectedMakerAccSlotChanged(int slot)
        {
            var acckeep = KeepState.NonHairKeep;
            if (SlotInfo.TryGetValue(slot, out var slotdata))
            {
                acckeep = slotdata.KeepState;
            }
            AccKeepRadio.SetValue((int)acckeep, false);
        }

        public static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;

            #region Personal Settings
            var category = new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings");

            e.AddSubCategory(category);

            //e.AddControl(new MakerText(null, category, owner));
            e.AddControl(new MakerText("Toggle when all Hair accessories and accessories you want to keep on this character are ready.", category, owner));
            e.AddControl(new MakerText("Example Mecha Chika who requires her arm and legs accessories", category, owner));

            e.AddControl(new MakerButton("Keep All Accessories", category, owner)).OnClick.AddListener(delegate ()
            {
                var Controller = GetController;
                AccKeepRadio.SetValue((int)KeepState.NonHairKeep, false);
                foreach (var item in Controller.SlotInfo)
                {
                    if (Controller.Parts[item.Key].type != 120)
                    {
                        item.Value.KeepState = KeepState.NonHairKeep;
                        Controller.SaveSlot(item.Key);
                    }
                }
            });

            Character_Cosplay_Ready = e.AddControl(new MakerToggle(category, "Cosplay Academy Ready", false, owner));
            Character_Cosplay_Ready.ValueChanged.Subscribe(value =>
            {
                GetController.CosplayReady = value;
            });

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Select data that shouldn't be overwritten by other mods.", category, owner));
            e.AddControl(new MakerText("Example Mecha Chika who doesn't work with socks/pantyhose and requires her shoes/glove", category, owner));
            #region Keep Toggles

            for (var i = 0; i < Constants.ClothingTypesLength; i++)
            {
                PersonalClothingKeepControls(i, e);
            }

            MakeUpToggle = e.AddControl(new MakerToggle(category, "Keep Makeup", owner));

            MakeUpToggle.ValueChanged.Subscribe(x =>
            {
                GetController.MakeUpKeep = x;
                GetController.SaveCoordinate();
            });

            e.AddControl(new MakerSeparator(category, owner));
            #endregion

            #region Folders
            var invalidpath = System.IO.Path.GetInvalidPathChars().Select(xval => xval.ToString());

            Advanced = e.AddControl(new MakerToggle(category, "Advanced", false, owner));
            Advanced.ValueChanged.Subscribe(x =>
            {
                GetController.AdvancedDirectory = x;
                for (int i = 0, n = AdvancedCharacterOutfitFolders.Count; i < n; i++)
                {
                    var foldertext = AdvancedCharacterOutfitFolders.ElementAt(i).Value;
                    if (foldertext == null || foldertext.IsDisposed)
                    {
                        continue;
                    }
                    foldertext.Visible.OnNext(x);
                }
            });
            e.AddControl(new MakerText("Custom Character Outfit Folders", category, owner));
            SimpleCharacterOutfitFolders = e.AddControl(new MakerTextbox(category, "Simplified All Folders", "", owner));

            SimpleCharacterOutfitFolders.ValueChanged.Subscribe(x =>
            {
                if (x.ContainsAny(invalidpath))
                {
                    SimpleCharacterOutfitFolders.SetValue("Invalid Path", false);
                    return;
                }
                GetController.SimpleFolderDirectory = x;
            });

            foreach (var item in Constants.AdditionalCoordinateReferences)
            {
                CustomOutfitFoldersControls(item, e);
            }
            #endregion

            #endregion

            #region Accessory Window Settings
            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2, "Accessory Settings");

            var KeepRadio = new MakerRadioButtons(category, owner, "Accessory Keep", new string[] { "Don't", "Not Hair", "Is Hair" });
            AccKeepRadio = MakerAPI.AddAccessoryWindowControl(KeepRadio, true);
            AccKeepRadio.ValueChanged.Subscribe(x => KeepStateChanged(x));
            #endregion

            #region Clothing Settings

            category = new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings");
            e.AddSubCategory(category);

            e.AddControl(new MakerText("Settings to be applied to coordinates", category, owner));

            e.AddControl(new MakerSeparator(category, owner));

            CoordinateCreatorNames = e.AddControl(new MakerText("", category, owner));

            Creator = new MakerTextbox(category, "Author", Settings.CreatorName.Value, owner);
            Set_Name = new MakerTextbox(category, "Set Name", "", owner);
            Sub_Set_Name = new MakerTextbox(category, "Sub Set Name", "", owner);
            e.AddControl(Creator).ValueChanged.Subscribe(x => { Creatorname = x; });
            e.AddControl(Set_Name).ValueChanged.Subscribe(x =>
            {
                if (x.ContainsAny(invalidpath))
                {
                    Set_Name.SetValue("Invalid character", false);
                    return;
                }

                GetController.SetNames = x;
                GetController.SaveCoordinate();
            });
            e.AddControl(Sub_Set_Name).ValueChanged.Subscribe(x =>
            {
                if (x.ContainsAny(invalidpath))
                {
                    Sub_Set_Name.SetValue("Invalid character", false);
                    return;
                }


                GetController.SubSetNames = x;
                GetController.SaveCoordinate();
            });


            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Coordinate Type information", category, owner));

            var SimpleCoord = new List<string>() { "Unassigned" };
            SimpleCoord.AddRange(Enum.GetNames(typeof(Constants.SimplifiedCoordinateTypes)));
            for (int i = 0, n = SimpleCoord.Count; i < n; i++)
            {
                SimpleCoord[i] = SimpleCoord[i].Replace('_', ' ');
            }
            CoordinateTypeRadio = new MakerRadioButtons(category, owner, "Coordinate Type", 0, SimpleCoord.ToArray())
            {
                ColumnCount = 2
            };
            e.AddControl(CoordinateTypeRadio).ValueChanged.Subscribe(x =>
            {
                GetController.CoordinateType = x;
                GetController.SaveCoordinate();
            });

            var buttons = new List<string> { "Unassigned", "General" };
            buttons.AddRange(Enum.GetNames(typeof(Constants.ClothingTypes)));
            buttons.AddRange(Enum.GetNames(typeof(Constants.AdditonalClothingTypes)));
            for (int i = 0, n = buttons.Count; i < n; i++)
            {
                buttons[i] = buttons[i].Replace('_', ' ');
            }
            e.AddControl(new MakerText(null, category, owner));
            CoordinateSubTypeRadio = new MakerRadioButtons(category, owner, "Sub Type", 0, buttons.ToArray())
            {
                ColumnCount = 3
            };
            e.AddControl(CoordinateSubTypeRadio).ValueChanged.Subscribe(x =>
            {
                GetController.CoordinateSubType = x;
                GetController.SaveCoordinate();
            });

            e.AddControl(new MakerText(null, category, owner));

            GenderRadio = new MakerRadioButtons(category, owner, "Gender", new string[] { "Female", "Both", "Male" });
            e.AddControl(GenderRadio).ValueChanged.Subscribe(x =>
            {
                GetController.GenderType = x;
                GetController.SaveCoordinate();
            });


            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("H State Restrictions", category, owner));
            var HstatesOptions = Enum.GetNames(typeof(Constants.HStates));
            for (var i = 0; i < HstatesOptions.Length; i++)
            {
                HstatesOptions[i] = HstatesOptions[i].Replace('_', ' ');
            }
            HStateTypeRadio = new MakerRadioButtons(category, owner, "H State", 0, HstatesOptions)
            {
                ColumnCount = 3
            };
            e.AddControl(HStateTypeRadio).ValueChanged.Subscribe(x =>
            {
                GetController.HstateType_Restriction = x;
                GetController.SaveCoordinate();
            });

            //e.AddControl(new MakerText(null, category, owner));

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Club Restrictions", category, owner));
            var ClubOptions = Enum.GetNames(typeof(Constants.Club));
            for (var i = 0; i < ClubOptions.Length; i++)
            {
                ClubOptions[i] = ClubOptions[i].Replace('_', ' ');
            }
            ClubTypeRadio = new MakerRadioButtons(category, owner, "Club", 0, ClubOptions)
            {
                ColumnCount = 3
            };

            e.AddControl(ClubTypeRadio).ValueChanged.Subscribe(x =>
            {
                GetController.ClubType_Restriction = x;
                GetController.SaveCoordinate();
            });

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Non-replaceable", category, owner));
            for (var ClothingInt = 0; ClothingInt < 9; ClothingInt++)
            {
                ClothingKeepControls(ClothingInt, e);
            }
            #endregion

            e.AddControl(new MakerSeparator(category, owner));

            e.AddControl(new MakerText("Breast Size Restrictions (exclusive)", category, owner));
            for (var i = 0; i < Constants.BreastsizeLength; i++)
            {
                BreastSizeRestrictionControls(i, e);
            }
            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Height Restrictions (exclusive)", category, owner));

            for (var i = 0; i < Constants.HeightLength; i++)
            {
                HeightRestrictionControls(i, e);
            }

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Interest Restrictions: Koi_Sun", category, owner));

            for (var i = 0; i < Constants.InterestLength; i++)
            {
                InterestsRestrictionControls(i, e);
            }

            #region Personality Restrictions
            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Personality Restrictions", category, owner));
            for (int PersonNum = 0, n = Enum.GetNames(typeof(Constants.Personality)).Length; PersonNum < n; PersonNum++)
            {
                PersonalityRestrictionControls(PersonNum, e);
            }

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Trait Restrictions", category, owner));
            for (int TraitNum = 0, n = Enum.GetNames(typeof(Constants.Traits)).Length; TraitNum < n; TraitNum++)
            {
                TraitRestrictionControls(TraitNum, e);
            }
            #endregion

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            AccKeepRadio.GroupingID = GroupingID;
        }

        #region Control Creation

        private static void CustomOutfitFoldersControls(string stringname, RegisterSubCategoriesEvent e)
        {
            var invalidpath = System.IO.Path.GetInvalidPathChars().Select(xval => xval.ToString());
            AdvancedCharacterOutfitFolders[stringname] = e.AddControl(new MakerTextbox(new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings"), stringname, "", Settings.Instance));
            AdvancedCharacterOutfitFolders[stringname].ValueChanged.Subscribe(x =>
            {
                if (x.ContainsAny(invalidpath))
                {
                    SimpleCharacterOutfitFolders.SetValue("Invalid Path", false);
                    return;
                }
                GetController.ReferenceADVDirectory[stringname] = x;
                GetController.SaveCard();
            });
            AdvancedCharacterOutfitFolders[stringname].Visible.OnNext(false);
        }

        private static void PersonalClothingKeepControls(int ClothingInt, RegisterSubCategoriesEvent e)
        {
            PersonalKeepToggles[ClothingInt] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings"), $"Keep {((Constants.ClothingTypes)ClothingInt).ToString().Replace('_', ' ')}", false, Settings.Instance));
            PersonalKeepToggles[ClothingInt].ValueChanged.Subscribe(x =>
            {
                GetController.PersonalClothingBools[ClothingInt] = x;
                GetController.SaveCard();
            });
        }

        private static void PersonalityRestrictionControls(int PersonalityNum, RegisterSubCategoriesEvent e)
        {
            PersonalityToggles[PersonalityNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Personality)PersonalityNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            PersonalityToggles[PersonalityNum].ValueChanged.Subscribe(x =>
            {
                GetController.PersonalityType_Restriction[PersonalityNum] = --x;
                GetController.SaveCoordinate();
            });
        }

        private static void TraitRestrictionControls(int TraitNum, RegisterSubCategoriesEvent e)
        {
            TraitToggles[TraitNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Traits)TraitNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            TraitToggles[TraitNum].ValueChanged.Subscribe(x =>
            {
                GetController.TraitType_Restriction[TraitNum] = --x;
                GetController.SaveCoordinate();
            });
        }

        private static void InterestsRestrictionControls(int IntrestNum, RegisterSubCategoriesEvent e)
        {
            InterestToggles[IntrestNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Interests)IntrestNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            InterestToggles[IntrestNum].ValueChanged.Subscribe(x =>
            {
                GetController.Interest_Restriction[IntrestNum] = --x;
                GetController.SaveCoordinate();
            });
        }

        private static void ClothingKeepControls(int ClothingInt, RegisterSubCategoriesEvent e)
        {
            CoordinateKeepToggles[ClothingInt] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"Keep {((Constants.ClothingTypes)ClothingInt).ToString().Replace('_', ' ')}", false, Settings.Instance));
            CoordinateKeepToggles[ClothingInt].ValueChanged.Subscribe(x =>
            {
                GetController.CoordinateSaveBools[ClothingInt] = x; /*Settings.Logger.LogWarning($"Chaging Outfitnum restriction {(Constants.ClothingTypes)ClothingInt}");*/
                GetController.SaveCoordinate();
            });
        }

        private static void BreastSizeRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            BreastsizeToggles[size] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"{((Constants.Breastsize)size)}", false, Settings.Instance));
            BreastsizeToggles[size].ValueChanged.Subscribe(x =>
            {
                GetController.Breastsize_Restriction[size] = x;
                GetController.SaveCoordinate();
            });
        }

        private static void HeightRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            HeightToggles[size] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"{((Constants.Height)size)}", false, Settings.Instance));
            HeightToggles[size].ValueChanged.Subscribe(x =>
            {
                GetController.Height_Restriction[size] = x;
                GetController.SaveCoordinate();
            });
        }

        #endregion

        private static void KeepStateChanged(int e)
        {
            var Controller = GetController;
            var slotinfo = Controller.SelectedSlotInfo;
            if (slotinfo != null)
            {
                slotinfo.KeepState = (KeepState)(e - 1);
            }
            Controller.SaveSlot();
        }

        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if (type == 120)
            {
                if (SlotInfo.TryGetValue(slotNo, out var slotInfo))
                {
                    slotInfo.Clear();
                    SaveSlot(slotNo);
                    return;
                }
                AccKeepRadio.SetValue((int)KeepState.DontKeep, false);
            }
        }

        private void UpdateSlots()
        {
            if (!MakerAPI.InsideMaker)
            {
                return;
            }

            for (var i = 0; i < PersonalClothingBools.Length; i++)
            {
                PersonalKeepToggles[i].SetValue(PersonalClothingBools[i], false);
            }

            SelectedMakerAccSlotChanged(AccessoriesApi.SelectedMakerAccSlot);

            for (var i = 0; i < CoordinateKeepToggles.Length; i++)
            {
                CoordinateKeepToggles[i].SetValue(CoordinateSaveBools[i], false);
            }

            for (var i = 0; i < PersonalityToggles.Length; i++)
            {
                PersonalityType_Restriction.TryGetValue(i, out var Value);
                PersonalityToggles[i].SetValue(++Value, false);
            }

            for (var i = 0; i < TraitToggles.Length; i++)
            {
                TraitType_Restriction.TryGetValue(i, out var Value);
                TraitToggles[i].SetValue(++Value, false);
            }

            for (var i = 0; i < HeightToggles.Length; i++)
            {
                HeightToggles[i].SetValue(Height_Restriction[i], false);
            }

            for (var i = 0; i < BreastsizeToggles.Length; i++)
            {
                BreastsizeToggles[i].SetValue(Breastsize_Restriction[i], false);
            }

            for (var i = 0; i < Constants.InterestLength; i++)
            {
                Interest_Restriction.TryGetValue(i, out var value);
                InterestToggles[i].SetValue(++value, false);
            }

            UpdateClothingNots();

            Character_Cosplay_Ready.SetValue(CosplayReady);

            var creators = "[None]";
            if (CreatorNames.Count > 0)
            {
                creators = "";
                foreach (var item in CreatorNames)
                {
                    creators += $"[{item}] > ";
                }
                creators = creators.Remove(creators.Length - 3);
            }

            CoordinateCreatorNames.Text = creators;

            Advanced.SetValue(AdvancedDirectory, false);

            SimpleCharacterOutfitFolders.SetValue(SimpleFolderDirectory, false);

            for (int i = 0, n = AdvancedCharacterOutfitFolders.Count; i < n; i++)
            {
                var textbox = AdvancedCharacterOutfitFolders.ElementAt(i);
                if (ReferenceADVDirectory.TryGetValue(textbox.Key, out var foldername))
                {
                    textbox.Value.SetValue(foldername, false);
                    continue;
                }
                textbox.Value.SetValue("", false);
            }

            MakeUpToggle.SetValue(MakeUpKeep, false);
            CoordinateTypeRadio.SetValue(CoordinateType, false);
            CoordinateSubTypeRadio.SetValue(CoordinateSubType, false);
            ClubTypeRadio.SetValue(ClubType_Restriction, false);
            HStateTypeRadio.SetValue(HstateType_Restriction, false);
            Set_Name.SetValue(SetNames, false);
        }

        #region external event
        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            GetController.LoadSlot(e.DestinationSlotIndex);
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var controller = GetController;
            if (e.CopyDestination != controller.CurrentCoordinate.Value) return;
            foreach (var index in e.CopiedSlotIndexes)
            {
                controller.LoadSlot(index);
                if (index == AccessoriesApi.SelectedMakerAccSlot)
                {
                    controller.SelectedMakerAccSlotChanged(index);
                }
            }
        }
        #endregion

        internal void UpdateClothingNots()
        {
            for (int i = 0, n = ClothNotData.Length; i < n; i++)
            {
                ClothNotData[i] = false;
            }

            //Top can hide either or both bra or bot(pants) example: idol dress covers both
            //coordinate is used to hide either NotBot or NotShorts
            ClothNotData[0] = InfoBaseGetInfoInt(ChaControl.infoClothes[0], ChaListDefine.KeyType.Coordinate) == 2;
            ClothNotData[1] = InfoBaseGetInfoInt(ChaControl.infoClothes[0], ChaListDefine.KeyType.NotBra) == 1;

            //Bra can hide shorts(panties)
            ClothNotData[2] = InfoBaseGetInfoInt(ChaControl.infoClothes[2], ChaListDefine.KeyType.Coordinate) == 2;

            if (MakerAPI.InsideMaker) SaveCoordinate();
        }

        private static int InfoBaseGetInfoInt(ListInfoBase listInfo, ChaListDefine.KeyType key)
        {
            if (listInfo != null && listInfo.dictInfo.TryGetValue((int)key, out var stringresult) && int.TryParse(stringresult, out var intresult))
            {
                return intresult;
            }
            return 0;
        }

        internal void MovIt(List<QueueItem> _) => UpdatePluginData();
    }
}
