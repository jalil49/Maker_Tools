using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

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

        static AccessoryControlWrapper<MakerToggle, bool> AccKeepToggles;
        static AccessoryControlWrapper<MakerToggle, bool> HairKeepToggles;

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

            MakerAPI.MakerExiting += MakerAPI_MakerExiting;

            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            var Controller = GetController;
            Controller.StartCoroutine(Controller.UpdateSlots());
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
            var category = new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings");
            e.AddSubCategory(category);

            //e.AddControl(new MakerText(null, category, owner));
            e.AddControl(new MakerText("Toggle when all Hair accessories and accessories you want to keep on this character are ready.", category, owner));
            e.AddControl(new MakerText("Example Mecha Chika who requires her arm and legs accessories", category, owner));

            e.AddControl(new MakerButton("Keep All Accessories", category, owner)).OnClick.AddListener(delegate ()
            {
                var Controller = GetController;
                var ChaControl = Controller.ChaControl;
                var ACCKeep = Controller.AccKeep;
                for (int i = 0, n = AccKeepToggles.Control.ControlObjects.Count(); i < n; i++)
                {
                    if (AccessoriesApi.GetPartsInfo(i).type != 120)
                    {
                        AccKeepToggles.SetValue(i, true);
                        if (!ACCKeep.Contains(i))
                        {
                            ACCKeep.Add(i);
                        }
                    }
                }
            });

            Character_Cosplay_Ready = e.AddControl(new MakerToggle(category, "Cosplay Academy Ready", false, owner));
            Character_Cosplay_Ready.ValueChanged.Subscribe(value => GetController.CosplayReady = value);

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
            });

            e.AddControl(new MakerSeparator(category, owner));
            #endregion

            #region Folders
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
                GetController.SimpleFolderDirectory = x;
            });
            for (int i = 0, n = Constants.SimplifiedCoordinateTypesLength; i < n; i++)
            {
                CustomOutfitFoldersControls(i, e);
            }
            AdvancedCharacterOutfitFolders["Underwear"] = e.AddControl(new MakerTextbox(new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings"), "Underwear", "", Settings.Instance));
            AdvancedCharacterOutfitFolders["Underwear"].ValueChanged.Subscribe(x =>
            {
                GetController.AdvancedFolderDirectory["Underwear"] = x;
            });
            AdvancedCharacterOutfitFolders["Underwear"].Visible.OnNext(false);

            #endregion

            #endregion

            #region Accessory Window Settings
            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2, "Accessory Settings");
            var Keep = new MakerToggle(category, "Keep this Accessory", owner);
            AccKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(Keep, true);
            AccKeepToggles.ValueChanged += AccKeep_ValueChanged;

            var HairKeep = new MakerToggle(category, "Is this a hair piece?", owner);
            HairKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(HairKeep, true);
            HairKeepToggles.ValueChanged += AccHairKeep_ValueChanged;
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
            e.AddControl(Set_Name).ValueChanged.Subscribe(x => { GetController.SetNames = x; });
            e.AddControl(Sub_Set_Name).ValueChanged.Subscribe(x => { GetController.SubSetNames = x; });


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
            e.AddControl(CoordinateTypeRadio).ValueChanged.Subscribe(x => { GetController.CoordinateType = x; });

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
            });

            e.AddControl(new MakerText(null, category, owner));

            GenderRadio = new MakerRadioButtons(category, owner, "Gender", new string[] { "Female", "Both", "Male" });
            e.AddControl(GenderRadio).ValueChanged.Subscribe(x => { GetController.GenderType = x; });


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
            AccKeepToggles.Control.GroupingID = GroupingID;
            HairKeepToggles.Control.GroupingID = GroupingID;
        }

        #region Control Creation

        private static void CustomOutfitFoldersControls(int ClothingInt, RegisterSubCategoriesEvent e)
        {
            var stringname = ((Constants.SimplifiedCoordinateTypes)ClothingInt).ToString().Replace('_', ' ');
            AdvancedCharacterOutfitFolders[stringname] = e.AddControl(new MakerTextbox(new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings"), stringname, "", Settings.Instance));
            AdvancedCharacterOutfitFolders[stringname].ValueChanged.Subscribe(x =>
            {
                GetController.AdvancedFolderDirectory[stringname] = x;
            });
            AdvancedCharacterOutfitFolders[stringname].Visible.OnNext(false);
        }

        private static void PersonalClothingKeepControls(int ClothingInt, RegisterSubCategoriesEvent e)
        {
            PersonalKeepToggles[ClothingInt] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings"), $"Keep {((Constants.ClothingTypes)ClothingInt).ToString().Replace('_', ' ')}", false, Settings.Instance));
            PersonalKeepToggles[ClothingInt].ValueChanged.Subscribe(x =>
            {
                GetController.PersonalClothingBools[ClothingInt] = x;
            });
        }

        private static void PersonalityRestrictionControls(int PersonalityNum, RegisterSubCategoriesEvent e)
        {
            PersonalityToggles[PersonalityNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Personality)PersonalityNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            PersonalityToggles[PersonalityNum].ValueChanged.Subscribe(x =>
            {
                GetController.PersonalityType_Restriction[PersonalityNum] = --x;
            });
        }

        private static void TraitRestrictionControls(int TraitNum, RegisterSubCategoriesEvent e)
        {
            TraitToggles[TraitNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Traits)TraitNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            TraitToggles[TraitNum].ValueChanged.Subscribe(x =>
            {
                GetController.TraitType_Restriction[TraitNum] = --x;
            });
        }

        private static void InterestsRestrictionControls(int IntrestNum, RegisterSubCategoriesEvent e)
        {
            InterestToggles[IntrestNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Interests)IntrestNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            InterestToggles[IntrestNum].ValueChanged.Subscribe(x =>
            {
                GetController.Interest_Restriction[IntrestNum] = --x;
            });
        }

        private static void ClothingKeepControls(int ClothingInt, RegisterSubCategoriesEvent e)
        {
            CoordinateKeepToggles[ClothingInt] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"Keep {((Constants.ClothingTypes)ClothingInt).ToString().Replace('_', ' ')}", false, Settings.Instance));
            CoordinateKeepToggles[ClothingInt].ValueChanged.Subscribe(x =>
            {
                GetController.CoordinateSaveBools[ClothingInt] = x; /*Settings.Logger.LogWarning($"Chaging Outfitnum restriction {(Constants.ClothingTypes)ClothingInt}");*/
            });
        }

        private static void BreastSizeRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            BreastsizeToggles[size] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"{((Constants.Breastsize)size)}", false, Settings.Instance));
            BreastsizeToggles[size].ValueChanged.Subscribe(x =>
            {
                GetController.Breastsize_Restriction[size] = x;
            });
        }

        private static void HeightRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            HeightToggles[size] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"{((Constants.Height)size)}", false, Settings.Instance));
            HeightToggles[size].ValueChanged.Subscribe(x =>
            {
                GetController.Height_Restriction[size] = x;
            });
        }

        #endregion

        private static void AccHairKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            var Controller = GetController;
            if (Controller.HairAcc == null)
            {
                return;
            }
            if (e.NewValue)
            {
                Controller.HairAcc.Add(e.SlotIndex);
            }
            else
            {
                Controller.HairAcc.Remove(e.SlotIndex);
            }
        }

        private static void AccKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            var Controller = GetController;
            if (Controller.AccKeep == null)
            {
                return;
            }
            if (e.NewValue)
            {
                Controller.AccKeep.Add(e.SlotIndex);
            }
            else
            {
                Controller.AccKeep.Remove(e.SlotIndex);
            }
        }

        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if (type == 120)
            {
                AccKeepToggles.SetValue(slotNo, false, false);
                HairKeepToggles.SetValue(slotNo, false, false);
                HairAcc.Remove(slotNo);
                AccKeep.Remove(slotNo);
            }
        }

        private IEnumerator UpdateSlots()
        {
            if (!MakerAPI.InsideMaker)
            {
                yield break;
            }

            do
            {
                yield return null;
            }
            while (!MakerAPI.InsideAndLoaded);

            for (var i = 0; i < PersonalClothingBools.Length; i++)
            {
                PersonalKeepToggles[i].SetValue(PersonalClothingBools[i], false);
            }

            for (int i = 0, n = Parts.Length; i < n; i++)
            {
                AccKeepToggles.SetValue(i, AccKeep.Contains(i), false);
                HairKeepToggles.SetValue(i, HairAcc.Contains(i), false);
            }

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
                if (AdvancedFolderDirectory.TryGetValue(textbox.Key, out var foldername))
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
            GetController.AccessoriesTransfered(e);
        }

        private void AccessoriesTransfered(AccessoryTransferEventArgs e)
        {
            if (HairAcc.Contains(e.SourceSlotIndex))
            {
                if (!HairAcc.Contains(e.DestinationSlotIndex))
                    HairAcc.Add(e.DestinationSlotIndex);
            }
            if (AccKeep.Contains(e.SourceSlotIndex))
            {
                if (!AccKeep.Contains(e.DestinationSlotIndex))
                    AccKeep.Add(e.DestinationSlotIndex);
            }
            HairKeepToggles.SetValue(e.DestinationSlotIndex, HairKeepToggles.GetValue(e.SourceSlotIndex));
            AccKeepToggles.SetValue(e.DestinationSlotIndex, AccKeepToggles.GetValue(e.SourceSlotIndex));

        }

        private void AccessoriesCopied(AccessoryCopyEventArgs e)
        {
            var CopiedSlots = e.CopiedSlotIndexes.ToArray();
            var Source = CoordinateInfo[(int)e.CopySource];
            var Dest = CoordinateInfo[(int)e.CopyDestination];

            for (var i = 0; i < CopiedSlots.Length; i++)
            {
                if (Source.AccKeep.Contains(CopiedSlots[i]))
                {
                    if (!Dest.HairAcc.Contains(CopiedSlots[i]))
                        Dest.AccKeep.Add(CopiedSlots[i]);
                }
                else
                {
                    Dest.AccKeep.Remove(CopiedSlots[i]);
                }
                //Settings.Logger.LogWarning($"HairKeep");
                if (Source.HairAcc.Contains(CopiedSlots[i]))
                {
                    if (!Dest.HairAcc.Contains(CopiedSlots[i]))
                        Dest.HairAcc.Add(CopiedSlots[i]);
                }
                else
                {
                    Dest.HairAcc.Remove(CopiedSlots[i]);
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
                    AccKeepToggles.SetValue(item.DstSlot, true, false);
                    AccKeepToggles.SetValue(item.SrcSlot, false, false);
                }
                if (hairkeep.Contains(item.SrcSlot))
                {
                    hairkeep.Add(item.DstSlot);
                    hairkeep.Remove(item.SrcSlot);
                    HairKeepToggles.SetValue(item.DstSlot, true, false);
                    HairKeepToggles.SetValue(item.SrcSlot, false, false);
                }
            }
        }

        internal void RemoveOutfitEvent()
        {
            data.Removeoutfit(MaxKey);
        }

        internal void AddOutfitEvent()
        {
            for (var i = MaxKey; i < ChaFileControl.coordinate.Length; i++)
                data.Createoutfit(i);
        }

        #endregion

        internal void UpdateClothingNots()
        {
            if (!MakerAPI.InsideMaker)
            {
                return;
            }
            var clothingnot = ClothNotData;
            for (int i = 0, n = clothingnot.Length; i < n; i++)
            {
                clothingnot[i] = false;
            }

            var clothinfo = ChaControl.infoClothes;

            UpdateTopClothingNots(clothinfo[0], ref clothingnot);
            UpdateBraClothingNots(clothinfo[2], ref clothingnot);
        }

        private void UpdateTopClothingNots(ListInfoBase infoBase, ref bool[] clothingnot)
        {
            if (infoBase == null)
            {
                return;
            }
            Hooks.ClothingNotPatch.IsshortsCheck = false;
            var ListInfoResult = Hooks.ClothingNotPatch.ListInfoResult;
            var key = ChaListDefine.KeyType.Coordinate;
            infoBase.GetInfo(key);
            var notbot = ListInfoResult[key] == 2; //only in ChangeClothesTopAsync
            key = ChaListDefine.KeyType.NotBra;
            infoBase.GetInfo(key);
            var notbra = ListInfoResult[key] == 1; //only in ChangeClothesTopAsync
            clothingnot[0] = clothingnot[0] || notbot;
            clothingnot[1] = clothingnot[1] || notbra;
        }

        private void UpdateBraClothingNots(ListInfoBase infoBase, ref bool[] clothingnot)
        {
            if (infoBase == null)
            {
                return;
            }
            Hooks.ClothingNotPatch.IsshortsCheck = true;

            var ListInfoResult = Hooks.ClothingNotPatch.ListInfoResult;
            var key = ChaListDefine.KeyType.Coordinate;

            infoBase.GetInfo(key);

            var notShorts = ListInfoResult[key] == 2; //only in ChangeClothesBraAsync
            clothingnot[2] = clothingnot[2] || notShorts;
        }
    }
}
