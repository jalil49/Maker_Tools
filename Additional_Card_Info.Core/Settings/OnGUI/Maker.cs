using Additional_Card_Info.Controls;
using Extensions.GUI_Classes;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Additional_Card_Info
{
    public class Maker
    {
        private readonly ToolbarGUI _accessoryKeepType;

        private readonly WindowGUI _accessoryWindow;
        private readonly WindowGUI _characterWindow;
        private readonly WindowGUI _clothingWindow;
        private readonly WindowGUI _settingsWindow;

        private Maker()
        {
            var config = Settings.Instance.Config;
            _clothingWindow = new WindowGUI(config, "Clothing", "Clothing Window", Rect.zero, 1f, ClothingWindowDraw,
                GUIContent.none, new ScrollGUI(ClothingWindowScrollDraw));
            _accessoryWindow = new WindowGUI(config, "Accessory", "Accessory Window", Rect.zero, 1f,
                AccessoryWindowDraw,
                GUIContent.none, new ScrollGUI(AccessoryWindowScrollDraw));
            _settingsWindow = new WindowGUI(config, "Settings", "Settings Window", Rect.zero, 1f, SettingsWindowDraw,
                GUIContent.none, new ScrollGUI(SettingsWindowScrollDraw));
            _characterWindow = new WindowGUI(config, "Character", "Character Window", Rect.zero, 1f, SettingsWindowDraw,
                GUIContent.none, new ScrollGUI(CharacterWindowScrollDraw));

            _accessoryKeepType = new ToolbarGUI(0,
                new[]
                {
                    new GUIContent("Don't Keep", "Don't move this accessory if outfit override"),
                    new GUIContent("Accessory", "Move this accessory with outfit override"),
                    new GUIContent("Hair", "Move this accessory with outfit override, also treat as hair")
                });
        }

        private static string CoordinateCreatorNames { get; set; }

        private static string Creator
        {
            get => Settings.CreatorName.Value;
            set => Settings.CreatorName.Value = value;
        }

        private static List<string> CreatorNames
        {
            get => GetController.CreatorNames;
            set => GetController.CreatorNames = value;
        }

        private static string SimpleCharacterOutfitFolders
        {
            get => GetController.SimpleFolderDirectory;
            set => GetController.SimpleFolderDirectory = value;
        }

        private static string SetName
        {
            get => GetController.SetNames;
            set => GetController.SetNames = value;
        }

        private string SubsetName
        {
            get => GetController.SubSetNames;
            set => GetController.SubSetNames = value;
        }

        private static string CreatorName => Settings.CreatorName.Value;

        private Dictionary<SlotData, SlotDataControls> _slotDataControlsMap =
            new Dictionary<SlotData, SlotDataControls>();

        private SlotDataControls SelectedSlotDataControl
        {
            get
            {
                if(_slotDataControlsMap.TryGetValue(SelectedSlotData, out var controls))
                    return controls;
                return _slotDataControlsMap[SelectedSlotData] = new SlotDataControls(SelectedSlotData);

            }
        }
        private static CharaEvent GetController => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public int ListInfoResult { get; internal set; }

        private SlotData SelectedSlotData
        {
            get
            {
                if(GetController.SlotData.TryGetValue(SelectedSlot, out var slotData))
                    return slotData;
                return GetController.SlotData[SelectedSlot] = new SlotData();
            }
        }

        private static int SelectedSlot => AccessoriesApi.SelectedMakerAccSlot;
        public static Maker MakerInstance { get; private set; }

        private WindowReturn CharacterWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if(Button("X", "Close this window"))
                {
                    CharacterWindowToggle();
                }
            }

            GL.EndHorizontal();
            return new WindowReturn();
        }

        internal void CharacterWindowToggle()
        {
            _characterWindow.ToggleShow();
        }

        private WindowReturn SettingsWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if(Button("X", "Close this window"))
                {
                    SettingsWindowToggle();
                }
            }

            GL.EndHorizontal();
            return new WindowReturn();
        }

        private void CharacterWindowScrollDraw() { }

        internal void SettingsWindowToggle()
        {
            _settingsWindow.ToggleShow();
        }

        private void SettingsWindowScrollDraw() { }

        private WindowReturn AccessoryWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if(Button("X", "Close this window"))
                {
                    AccessoryWindowToggle();
                }
            }

            GL.EndHorizontal();
            return new WindowReturn();
        }

        private void AccessoryWindowScrollDraw()
        {

            GL.BeginHorizontal();
            {

            }

            GL.EndHorizontal();

        }

        internal void AccessoryWindowToggle()
        {
            _accessoryWindow.ToggleShow();
        }

        private WindowReturn ClothingWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if(Button("X", "Close this window"))
                {
                    ClothingWindowToggle();
                }
            }

            GL.EndHorizontal();
            return new WindowReturn();
        }

        private void ClothingWindowScrollDraw() { }

        internal void ClothingWindowToggle()
        {
            _clothingWindow.ToggleShow();
        }

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            MakerInstance = new Maker();
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            GetController.UpdatePluginData();
        }

        private static void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            MakerInstance = null;
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
            // e.AddControl(new MakerText(
            //     "Toggle when all Hair accessories and accessories you want to keep on this character are ready.",
            //     category, owner));
            // e.AddControl(
            //     new MakerText("Example Mecha Chika who requires her arm and legs accessories", category, owner));

            e.AddControl(new MakerButton("Character Window", category, owner)).OnClick
                .AddListener(MakerInstance.CharacterWindowToggle);

            // _characterCosplayReady = e.AddControl(new MakerToggle(category, "Cosplay Academy Ready", false, owner));
            // _characterCosplayReady.ValueChanged.Subscribe(value => { GetController.CosplayReady = value; });
            //
            // e.AddControl(new MakerSeparator(category, owner));
            // e.AddControl(new MakerText("Select data that shouldn't be overwritten by other mods.", category, owner));
            // e.AddControl(new MakerText(
            //     "Example Mecha Chika who doesn't work with socks/pantyhose and requires her shoes/glove", category,
            //     owner));

            #endregion

            #region Accessory Window Settings

            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2,
                "Accessory Settings");
            var accessoryButton = new MakerButton("Accessory Window", category, owner);
            MakerAPI.AddAccessoryWindowControl(accessoryButton, true).OnClick
                .AddListener(MakerInstance.AccessoryWindowToggle);

            #endregion

            #region Clothing Settings

            category = new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2,
                "Clothing Settings");

            e.AddSubCategory(category);
            MakerAPI.AddAccessoryWindowControl(new MakerButton("Clothing Window", category, owner), true).OnClick
                .AddListener(MakerInstance.AccessoryWindowToggle);

            #endregion

            var groupingID = $"Maker_Tools_{Settings.NamingID.Value}";
            accessoryButton.GroupingID = groupingID;
        }

        private void UpdateCreatorNames()
        {
            var creatorNames = GetController.CreatorNames;
            if(creatorNames.Count > 0)
            {
                var test = new StringBuilder();
                foreach(var item in creatorNames)
                {
                    test.Append($"[{item}] > ");
                }

                test.Remove(test.Length - 1, 3);
                CoordinateCreatorNames = test.ToString();
            }
            else
            {
                CoordinateCreatorNames = "[None]";
            }
        }

        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if(type != 120)
            {
                return;
            }

            if(!GetController.SlotData.TryGetValue(slotNo, out var slotData))
            {
                return;
            }

            slotData.Init();
            GetController.SaveSlot(slotNo);
        }

        private static int InfoBaseGetInfoInt(ListInfoBase listInfo, ChaListDefine.KeyType key)
        {
            if(listInfo != null && listInfo.dictInfo.TryGetValue((int)key, out var stringResult) &&
                int.TryParse(stringResult, out var intResult))
            {
                return intResult;
            }

            return 0;
        }

        internal void OnGui()
        {
            if(!MakerAPI.IsInterfaceVisible())
            {
                return;
            }

            if(AccessoriesApi.AccessoryCanvasVisible)
            {
                _accessoryWindow.Draw();
            }
        }

        #region external event

        internal void MovIt(List<QueueItem> _)
        {
            GetController.UpdatePluginData();
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            GetController.LoadSlot(e.DestinationSlotIndex);
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var controller = GetController;
            if(e.CopyDestination != controller.CurrentCoordinate.Value)
            {
                return;
            }

            foreach(var index in e.CopiedSlotIndexes)
            {
                controller.SlotData.Remove(index);
                controller.LoadSlot(index);
            }
        }

        #endregion
    }
}