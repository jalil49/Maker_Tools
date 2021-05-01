using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        readonly MakerToggle[] CoordinateKeepToggles = new MakerToggle[9];

        MakerToggle IsUnderwear;
        AccessoryControlWrapper<MakerToggle, bool> AccKeepToggles;
        AccessoryControlWrapper<MakerToggle, bool> HairKeepToggles;

        private void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, e2) => VisibiltyToggle();

            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
            Hooks.Slot_ACC_Change += Hooks_Slot_ACC_Change;
        }

        private void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.MakerAccSlotAdded -= AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, e2) => VisibiltyToggle();

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
            Hooks.Slot_ACC_Change -= Hooks_Slot_ACC_Change;
        }

        private void Hooks_Slot_ACC_Change(object sender, Slot_ACC_Change_ARG e)
        {
            VisibiltyToggle();
        }

        private void MakerAPI_MakerFinishedLoading(object sender, System.EventArgs e)
        {
            VisibiltyToggle();
        }

        private void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            #region Personal Settings
            MakerCategory category = new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings");

            e.AddSubCategory(category);

            e.AddControl(new MakerText("Toggle when all Hair accessories and accessories you want to keep on this character are ready.\nExample Mecha Chika who requires her arm and legs accessories", category, owner));
            e.AddControl(new MakerSeparator(category, owner));

            e.AddControl(new MakerToggle(category, "Cosplay Academy Ready", false, owner)).BindToFunctionController<CharaEvent, bool>(
                 (controller) => Character_Cosplay_Ready,
                 (controller, value) => Character_Cosplay_Ready = value);

            e.AddControl(new MakerText("Select data that shouldn't be overwritten by other mods.\nExample Mecha Chika who doesn't work with socks/pantyhose and requires her shoes/glove", category, owner));

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
            CoordinateKeepToggles[0] = new MakerToggle(category, $"Keep Top", owner);
            CoordinateKeepToggles[1] = new MakerToggle(category, $"Keep Bottom", owner);
            CoordinateKeepToggles[2] = new MakerToggle(category, $"Keep Bra", owner);
            CoordinateKeepToggles[3] = new MakerToggle(category, $"Keep Underwear", owner);
            CoordinateKeepToggles[4] = new MakerToggle(category, $"Keep Gloves", owner);
            CoordinateKeepToggles[5] = new MakerToggle(category, $"Keep Pantyhose", owner);
            CoordinateKeepToggles[6] = new MakerToggle(category, $"Keep Socks", owner);
            CoordinateKeepToggles[7] = new MakerToggle(category, $"Keep Indoor Shoes", owner);
            CoordinateKeepToggles[8] = new MakerToggle(category, $"Keep Outdoor Shoes", owner);
            e.AddControl(CoordinateKeepToggles[0]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][0] = x);
            e.AddControl(CoordinateKeepToggles[1]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][1] = x);
            e.AddControl(CoordinateKeepToggles[2]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][2] = x);
            e.AddControl(CoordinateKeepToggles[3]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][3] = x);
            e.AddControl(CoordinateKeepToggles[4]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][4] = x);
            e.AddControl(CoordinateKeepToggles[5]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][5] = x);
            e.AddControl(CoordinateKeepToggles[6]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][6] = x);
            e.AddControl(CoordinateKeepToggles[7]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][7] = x);
            e.AddControl(CoordinateKeepToggles[8]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][8] = x);
            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Underwear settings", category, owner));

            IsUnderwear = new MakerToggle(category, "Is underwear coordinate", owner);
            e.AddControl(IsUnderwear).ValueChanged.Subscribe(x => underwearbool = x);

            #endregion


            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            AccKeepToggles.Control.GroupingID = GroupingID;
            HairKeepToggles.Control.GroupingID = GroupingID;
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
