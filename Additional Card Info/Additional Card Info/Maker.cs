using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections.Generic;
using UniRx;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        readonly MakerToggle[] CoordinateKeepToggles = new MakerToggle[Constants.ClothingTypesLength];
        readonly MakerRadioButtons[] PersonalityToggles = new MakerRadioButtons[Constants.PersonalityLength];
        readonly MakerRadioButtons[] TraitToggles = new MakerRadioButtons[Constants.TraitsLength];
        readonly MakerToggle[] HeightToggles = new MakerToggle[Constants.HeightLength];
        readonly MakerToggle[] BreastsizeToggles = new MakerToggle[Constants.BreastsizeLength];

        AccessoryControlWrapper<MakerToggle, bool> AccKeepToggles;
        AccessoryControlWrapper<MakerToggle, bool> HairKeepToggles;

        MakerRadioButtons CoordinateTypeRadio;
        MakerRadioButtons CoordinateSubTypeRadio;
        MakerRadioButtons ClubTypeRadio;
        MakerRadioButtons HStateTypeRadio;

        MakerTextbox Creator;
        MakerTextbox Set_Name;

        private void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;

            MakerAPI.MakerExiting += MakerAPI_MakerExiting;

            MakerAPI.ReloadCustomInterface += (s, e2) => StartCoroutine(UpdateSlots());
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, e2) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading += (s, e2) =>
            {
                VisibiltyToggle();
            };
            Hooks.Slot_ACC_Change += (s, e2) => VisibiltyToggle();
        }

        private void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.MakerAccSlotAdded -= AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;


            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;

            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, e2) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading -= (s, e2) => VisibiltyToggle();
            MakerAPI.ReloadCustomInterface -= (s, e2) => StartCoroutine(UpdateSlots());
            Hooks.Slot_ACC_Change -= (s, e2) => VisibiltyToggle();
        }

        private void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;

            #region Personal Settings
            MakerCategory category = new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings");

            e.AddSubCategory(category);

            e.AddControl(new MakerText("Toggle when all Hair accessories and accessories you want to keep on this character are ready.\nExample Mecha Chika who requires her arm and legs accessories", category, owner));
            e.AddControl(new MakerText(null, category, owner));
            e.AddControl(new MakerSeparator(category, owner));

            e.AddControl(new MakerToggle(category, "Cosplay Academy Ready", false, owner)).BindToFunctionController<CharaEvent, bool>(
                 (controller) => Character_Cosplay_Ready,
                 (controller, value) => Character_Cosplay_Ready = value);

            e.AddControl(new MakerText("Select data that shouldn't be overwritten by other mods.\nExample Mecha Chika who doesn't work with socks/pantyhose and requires her shoes/glove", category, owner));
            e.AddControl(new MakerText(null, category, owner));

            #region Keep Toggles

            var TopKeep = e.AddControl(new MakerToggle(category, "Keep Top", owner));

            TopKeep.BindToFunctionController<CharaEvent, bool>(
                (controller) => PersonalClothingBools[0],
                (controller, value) => PersonalClothingBools[0] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Bottom", owner));

            TopKeep.BindToFunctionController<CharaEvent, bool>(
                (controller) => PersonalClothingBools[1],
                (controller, value) => PersonalClothingBools[1] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Bra", owner));

            TopKeep.BindToFunctionController<CharaEvent, bool>(
                (controller) => PersonalClothingBools[2],
                (controller, value) => PersonalClothingBools[2] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Underwear", owner));

            TopKeep.BindToFunctionController<CharaEvent, bool>(
                (controller) => PersonalClothingBools[3],
                (controller, value) => PersonalClothingBools[3] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Gloves", owner));

            TopKeep.BindToFunctionController<CharaEvent, bool>(
                (controller) => PersonalClothingBools[4],
                (controller, value) => PersonalClothingBools[4] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Pantyhose", owner));

            TopKeep.BindToFunctionController<CharaEvent, bool>(
                (controller) => PersonalClothingBools[5],
                (controller, value) => PersonalClothingBools[5] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep socks", owner));

            TopKeep.BindToFunctionController<CharaEvent, bool>(
                (controller) => PersonalClothingBools[6],
                (controller, value) => PersonalClothingBools[6] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Indoor Shoes", owner));

            TopKeep.BindToFunctionController<CharaEvent, bool>(
                (controller) => PersonalClothingBools[7],
                (controller, value) => PersonalClothingBools[7] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Outdoor Shoes", owner));
            TopKeep.BindToFunctionController<CharaEvent, bool>(
                (controller) => PersonalClothingBools[8],
                (controller, value) => PersonalClothingBools[8] = value);

            #endregion

            #endregion

            #region Accessory Window Settings
            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2, "Accessory Settings");
            var Keep = new MakerToggle(category, "Keep this Accessory", owner);
            AccKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(Keep);
            AccKeepToggles.ValueChanged += AccKeep_ValueChanged;

            var HairKeep = new MakerToggle(category, "Is this a hair piece?", owner);
            HairKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(HairKeep);
            HairKeepToggles.ValueChanged += AccHairKeep_ValueChanged;
            #endregion

            #region Clothing Settings

            category = new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings");
            e.AddSubCategory(category);

            e.AddControl(new MakerText("Settings to be applied when saving coordinates\nThis isn't saved to Character Cards", category, owner));

            e.AddControl(new MakerSeparator(category, owner));

            Creator = new MakerTextbox(category, "Author", Settings.CreatorName.Value, owner);
            Set_Name = new MakerTextbox(category, "Set Name", "", owner);

            e.AddControl(Creator).ValueChanged.Subscribe(x => CreatorNames[CoordinateNum] = x);
            e.AddControl(Set_Name).ValueChanged.Subscribe(x => SetNames[CoordinateNum] = x);


            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Coordinate Type information", category, owner));
            CoordinateTypeRadio = new MakerRadioButtons(category, owner, "Coordinate Type", 0, Enum.GetNames(typeof(Constants.SimplifiedCoordinateTypes)))
            {
                Rows = 2
            };
            e.AddControl(CoordinateTypeRadio).ValueChanged.Subscribe(x => CoordinateType[CoordinateNum] = x);
            e.AddControl(new MakerText(null, category, owner));

            List<string> buttons = new List<string> { "General" };
            buttons.AddRange(Enum.GetNames(typeof(Constants.ClothingTypes)));
            buttons.AddRange(Enum.GetNames(typeof(Constants.AdditonalClothingTypes)));
            for (int i = 0, n = buttons.Count; i < n; i++)
            {
                buttons[i] = buttons[i].Replace('_', ' ');
            }
            CoordinateSubTypeRadio = new MakerRadioButtons(category, owner, "Sub Type", 0, buttons.ToArray())
            {
                Rows = 5
            };
            Settings.Logger.LogWarning($"Subtype {buttons.Count / 4 + (buttons.Count % 4) % 2}");
            e.AddControl(CoordinateSubTypeRadio).ValueChanged.Subscribe(x =>
            {
                CoordinateSubType[CoordinateNum] = x;
                Settings.Logger.LogWarning($"Setting coordiante sub to {buttons[x]}");
            });
            for (int i = 0; i < 3; i++)
            {
                e.AddControl(new MakerText(null, category, owner));
            }

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("H State Restrictions", category, owner));
            string[] HstatesOptions = Enum.GetNames(typeof(Constants.HStates));
            for (int i = 0; i < HstatesOptions.Length; i++)
            {
                HstatesOptions[i] = HstatesOptions[i].Replace('_', ' ');
            }
            HStateTypeRadio = new MakerRadioButtons(category, owner, "H State", 0, HstatesOptions)
            {
                Rows = 2
            };
            e.AddControl(HStateTypeRadio).ValueChanged.Subscribe(x => HstateType_Restriction[CoordinateNum] = x);

            e.AddControl(new MakerText(null, category, owner));

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Club Restrictions", category, owner));
            string[] ClubOptions = Enum.GetNames(typeof(Constants.Club));
            for (int i = 0; i < ClubOptions.Length; i++)
            {
                ClubOptions[i] = ClubOptions[i].Replace('_', ' ');
            }
            ClubTypeRadio = new MakerRadioButtons(category, owner, "Club", 0, ClubOptions)
            {
                Rows = 4
            };

            e.AddControl(ClubTypeRadio).ValueChanged.Subscribe(x => ClubType_Restriction[CoordinateNum] = x - 1);
            for (int i = 0; i < 2; i++)
            {
                e.AddControl(new MakerText(null, category, owner));
            }
            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Non-replaceable", category, owner));
            for (int ClothingInt = 0; ClothingInt < 9; ClothingInt++)
            {
                ClothingKeepControls(ClothingInt, e);
            }
            #endregion

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Breast Size Restrictions (exclusive)", category, owner));
            for (int i = 0; i < Constants.BreastsizeLength; i++)
            {
                BreastSizeRestrictionControls(i, e);
            }
            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Height Restrictions (exclusive)", category, owner));

            for (int i = 0; i < Constants.HeightLength; i++)
            {
                HeightRestrictionControls(i, e);
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

        private void PersonalityRestrictionControls(int PersonalityNum, RegisterSubCategoriesEvent e)
        {
            //PersonalityToggles[PersonalityNum] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), ((Constants.Personality)PersonalityNum).ToString().Replace('_', ' '), false, Settings.Instance));
            PersonalityToggles[PersonalityNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Personality)PersonalityNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            PersonalityToggles[PersonalityNum].ValueChanged.Subscribe(x =>
            {
                Settings.Logger.LogWarning($"Changing Outfitnum restriction {(Constants.Personality)PersonalityNum}");
                if (x == 1)
                {
                    PersonalityType_Restriction[CoordinateNum].Remove(PersonalityNum);
                    return;
                }
                PersonalityType_Restriction[CoordinateNum][PersonalityNum] = x;
            });
        }

        private void TraitRestrictionControls(int TraitNum, RegisterSubCategoriesEvent e)
        {
            TraitToggles[TraitNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Traits)TraitNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            TraitToggles[TraitNum].ValueChanged.Subscribe(x =>
            {
                Settings.Logger.LogWarning($"Changing Outfitnum restriction {(Constants.Traits)TraitNum}");
                if (x == 1)
                {
                    PersonalityType_Restriction[CoordinateNum].Remove(TraitNum);
                    return;
                }
                PersonalityType_Restriction[CoordinateNum][TraitNum] = x;
            });
        }

        private void ClothingKeepControls(int ClothingInt, RegisterSubCategoriesEvent e)
        {
            CoordinateKeepToggles[ClothingInt] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"Keep {((Constants.ClothingTypes)ClothingInt).ToString().Replace('_', ' ')}", false, Settings.Instance));
            CoordinateKeepToggles[ClothingInt].ValueChanged.Subscribe(x => { CoordinateSaveBools[CoordinateNum][ClothingInt] = x; Settings.Logger.LogWarning($"Chaging Outfitnum restriction {(Constants.ClothingTypes)ClothingInt}"); });
        }

        private void BreastSizeRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            BreastsizeToggles[size] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"{((Constants.Breastsize)size)}", false, Settings.Instance));
            BreastsizeToggles[size].ValueChanged.Subscribe(x => { Breastsize_Restriction[CoordinateNum][size] = x; Settings.Logger.LogWarning($"Chaging Outfitnum restriction {(Constants.Breastsize)size}"); });
        }

        private void HeightRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            HeightToggles[size] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"{((Constants.Height)size)}", false, Settings.Instance));
            HeightToggles[size].ValueChanged.Subscribe(x => { Height_Restriction[CoordinateNum][size] = x; Settings.Logger.LogWarning($"Chaging Outfitnum restriction {(Constants.Height)size}"); });
        }

        private void AccHairKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            if (HairAcc == null)
            {
                return;
            }
            if (e.NewValue)
            {
                HairAcc[CoordinateNum].Add(e.SlotIndex);
            }
            else
            {
                HairAcc[CoordinateNum].Remove(e.SlotIndex);
            }
        }

        private void AccKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            if (AccKeep == null)
            {
                return;
            }
            if (e.NewValue)
            {
                AccKeep[CoordinateNum].Add(e.SlotIndex);
            }
            else
            {
                AccKeep[CoordinateNum].Remove(e.SlotIndex);
            }
        }

        private void VisibiltyToggle()
        {
            if (!MakerAPI.InsideMaker)
                return;

            var accessory = MakerAPI.GetCharacterControl().GetAccessoryObject(AccessoriesApi.SelectedMakerAccSlot);
            if (accessory == null)
            {

                AccKeepToggles.Control.Visible.OnNext(false);
                HairKeepToggles.Control.Visible.OnNext(false);
            }
            else
            {
                HairKeepToggles.Control.Visible.OnNext(true);
                AccKeepToggles.Control.Visible.OnNext(true);
            }
        }

        private void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {

        }
    }
}
