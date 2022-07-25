﻿using Accessory_States.Classes.PresetStorage;
using Extensions.GUI_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Accessory_States.OnGUI
{
    /// <summary>
    /// Setup for Maker by Default to be adapted for others via override
    /// </summary>
    public abstract class BaseGUI
    {
        #region Windows
        protected readonly WindowGUI _slotWindow;//main SelectedSlot window
        protected readonly WindowGUI _groupGUI;//General NameData window, add, change, remove Namedata and other modifications
        protected readonly WindowGUI _presetWindow;
        protected readonly WindowGUI _addBinding;
        protected readonly WindowGUI _previewWindow;
        protected readonly WindowGUI _settingWindow;
        #endregion

        #region Settings
        protected ToolbarGUI _AssPreference;
        #endregion

        protected readonly ToolbarGUI ShoeTypeGUI;
        protected readonly ToolbarGUI PresetView;
        protected abstract int SelectedDropDown { get; set; }

        protected readonly Dictionary<NameData, NameDataControl> NameControls = new Dictionary<NameData, NameDataControl>();
        protected readonly Dictionary<StateInfo, StateInfoControl> StateControls = new Dictionary<StateInfo, StateInfoControl>();
        protected readonly List<PresetData> SinglePresetDatas = new List<PresetData>();
        protected readonly List<PresetFolder> presetFolders = new List<PresetFolder>();
        protected readonly Dictionary<PresetData, PresetContols> SingleControls = new Dictionary<PresetData, PresetContols>();
        protected readonly Dictionary<PresetFolder, PresetFolderContol> PresetFolderControls = new Dictionary<PresetFolder, PresetFolderContol>();
        protected readonly TextFieldGUI PresetFilter;

        protected abstract int SelectedSlot { get; set; }
        protected CharaEvent GetController => GetControl?.CharaEvent;
        protected abstract CharaEventControl GetControl { get; set; }
        protected abstract SlotData SelectedSlotData { get; }

        protected BaseGUI(string section)
        {
            SelectedDropDown = 0;

            #region Windows
            BepInEx.Configuration.ConfigFile config = Settings.Instance.Config;
            _slotWindow = new WindowGUI(config, section, "Slot", new Rect(Screen.width * 0.33f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), 1f, SlotWindowDraw, new GUIContent("Slot 0", "Modify data attached to this Accessory"), new ScrollGUI(SlotWindowScroll));

            _presetWindow = new WindowGUI(config, section, "Preset", new Rect(Screen.width * 0.33f, Screen.height * 0.3f, Screen.width * 0.14f, Screen.height * 0.2f), 1f, PresetWindowDraw, new GUIContent("Presets", "Apply, Create or Modify Presets"), new[] { new ScrollGUI(PresetFolderScroll), new ScrollGUI(PresetSingleScroll) });

            _groupGUI = new WindowGUI(config, section, "Group Data", new Rect(Screen.width * 0.475f, Screen.height * 0.3f, Screen.width * 0.128f, Screen.height * 0.2f), 1f, GroupWindowDraw, new GUIContent("Custom Group Data", "Modify Custom Groups"), new ScrollGUI(GroupWindowScroll));

            _settingWindow = new WindowGUI(config, section, "Settings", new Rect(Screen.width * 0.57f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), 1f, SettingWindowDraw, new GUIContent("Settings", "Modify Plugin Settings"), new ScrollGUI(SettingScroll));

            _addBinding = new WindowGUI(config, section, "Group Select", new Rect(Screen.width * 0.475f, Screen.height * 0.1f, Screen.width * 0.075f, Screen.height * 0.2f), 1f, SelectBindingWindowDraw, new GUIContent("Group Select", "Select a group to Show"), new ScrollGUI(BindingWindowScroll));

            _previewWindow = new WindowGUI(config, section, "Preview", new Rect(Screen.width * 0.80f, Screen.height * 0.2f, Screen.width * 0.076f, Screen.height * 0.75f), 0.6f, PreviewWindowDraw, new GUIContent("State Preview", "Adjust State Values"), new ScrollGUI(PreviewScroll));
            #endregion

            GUIContent[] shoeTypeGUIContext = new GUIContent[3];
#if KK
            shoeTypeGUIContext[0] = new GUIContent("Indoor", "Story/FreeH: Applies when inside buildings");
            shoeTypeGUIContext[1] = new GUIContent("Outdoor", "Story/FreeH: Applies when outside buildings");
            shoeTypeGUIContext[2] = new GUIContent("Both", "Applies regardless of Location");
#else
            shoeTypeGUIContext[0] = new GUIContent("Indoor", "Non-functioning: can attempt in the future Story/FreeH: Applies when inside buildings");
            shoeTypeGUIContext[1] = new GUIContent("Outdoor", "Non-functioning: can attempt in the future Story/FreeH: Applies when outside buildings");
            shoeTypeGUIContext[2] = new GUIContent("Both", "Functioning: Applies regardless of Location");
#endif

            GUIContent[] assContent = new GUIContent[]
            {
                new GUIContent("Indoor","Save Accessory State Sync format with Indoor Values if not both"),
                new GUIContent("Outdoor","Save Accessory State Sync format with Outdoor Values if not both")
            };

#if KK
            _AssPreference = new ToolbarGUI(0, assContent);
#else
            _AssPreference = new ToolbarGUI(1, assContent);
#endif

            GUIContent[] PresetContent = new GUIContent[]
            {
                new GUIContent("Folder","Show Folder Presets"),
                new GUIContent("Single","Show invididual Presets")
            };
            PresetView = new ToolbarGUI(0, PresetContent);

            DiskLoad();

            PresetFilter = new TextFieldGUI(new GUIContent("", "Search among presets"));
            ShoeTypeGUI = new ToolbarGUI(2, shoeTypeGUIContext)
            {
                OnValueChange = (oldValue, newValue) =>
                {
                    ChaControl control = GetController.ChaControl;
                    if(newValue < 2)
                    {
#if KK
                        control.fileStatus.shoesType = (byte)newValue;
#endif
                        control.SetClothesState(7, control.fileStatus.clothesState[7]);
                    }
                }
            };
        }

        public virtual void TogglePreviewWindow()
        {
            _previewWindow.ToggleShow();
        }

        public virtual void ToggleSlotWindow()
        {
            _slotWindow.ToggleShow();
        }

        protected virtual void ToggleGroupWindow()
        {
            _groupGUI.ToggleShow();
        }

        protected virtual void ToggleSettingsWindow()
        {
            _settingWindow.ToggleShow();
        }

        protected virtual void TogglePresetWindow()
        {
            _presetWindow.ToggleShow();
        }
        protected virtual void ToggleAddBindingWindow()
        {
            _addBinding.ToggleShow();
        }

        protected virtual WindowReturn SlotWindowDraw()
        {
            _slotWindow.SetWindowName("Slot " + (SelectedSlot + 1));
            SlotData slotData = SelectedSlotData;
            BindingData bData = GetSelectedBindingData(slotData);

            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if(Button("Preview", "Open preview window to modify states", false))
                {
                    TogglePreviewWindow();
                }

                if(Button("Settings", "Open Settings", false))
                    ToggleSettingsWindow();

                GL.Space(10);

                if(Button("X", "Close this window", false))
                    ToggleSlotWindow();
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                ShoeTypeGUI.Draw();
                if(Toggle(slotData.Parented, "Enable Hide by Parent", "Toggle to hide by parent group") ^ slotData.Parented)
                {
                    slotData.Parented = !slotData.Parented;
                    GetController.UpdateParentedDict();
                    GetController.SaveSlotData(SelectedSlot);
                }
            }

            GL.EndHorizontal();
#if !KK
            if(ShoeTypeGUI.Value != 2)
            {
#if KKS
                Label("KK Functionality, KKS only uses Outdoor ATM by default");
#else
                Label("KK Functionality, may attempt to reimplement the feature later");
#endif

                GL.BeginHorizontal();
                {
                    if(ShoeTypeGUI.Value == 0)
                    {
#if KKS
                        Label("Non-functioning: May be implemented in the future");
#else
                        Label("Unknown functionality");
#endif
                    }

                    if(ShoeTypeGUI.Value == 1)
                    {
#if KKS
                        Label("Default State: May implement Indoor in the future");
#else
                        Label("Unknown functionality");
#endif
                    }
                }

                GL.EndHorizontal();
            }
#endif
            GL.BeginHorizontal();
            {
                if(Button("Group Data", "Create and Modify Custom Groups"))
                    _groupGUI.ToggleShow();

                if(Button("Use Presets Bindings", "Create and Use Predefine Presets to apply common settings."))
                    _presetWindow.ToggleShow();
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                string dropDownName = "None";
                if(bData != null)
                {
                    dropDownName = bData.NameData.Name;
                }

                if(Button("<", "Previous binding for this accessory") && slotData.bindingDatas.Count > 0)
                    SelectedDropDown = SelectedDropDown == 0 ? slotData.bindingDatas.Count - 1 : SelectedDropDown - 1;
                if(Button(dropDownName, "Click to open window to apply binding groups"))
                    _addBinding.ToggleShow();
                if(Button(">", "Next binding for this accessory") && slotData.bindingDatas.Count > 0)
                    SelectedDropDown = SelectedDropDown == slotData.bindingDatas.Count - 1 ? 0 : SelectedDropDown + 1;
            }

            GL.EndHorizontal();

            if(bData == null)
                return new WindowReturn();

            if(!NameControls.TryGetValue(bData.NameData, out NameDataControl controls))
            {
                NameControls[bData.NameData] = controls = new NameDataControl(bData.NameData, GetControl);
            }

            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                controls.DrawStatePreview();
            }

            GL.EndHorizontal();
            return new WindowReturn();
        }

        protected virtual void SlotWindowScroll()
        {
            BindingData bData = GetSelectedBindingData(SelectedSlotData);
            if(bData == null)
                return;
            GL.BeginVertical(GUI.skin.box);
            for(int i = 0; i < bData.States.Count; i++)
            {
                StateInfo item = bData.States[i];
                if(!(ShoeTypeGUI.Value == 2 || item.ShoeType == ShoeTypeGUI.Value || item.ShoeType == 2))
                    continue;

                if(!StateControls.TryGetValue(item, out StateInfoControl controls))
                {
                    StateControls[item] = controls = new StateInfoControl(bData, item, SelectedSlot, GetControl);
                }

                if(bData.NameData.CurrentState == i)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                controls.Draw(ShoeTypeGUI.Value);
                GL.EndHorizontal();
            }

            GL.EndVertical();
        }

        protected virtual WindowReturn GroupWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if(Button("X", "Close this window", false))
                    ToggleGroupWindow();

                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    if(Button("Add New Group"))
                    {
                        List<NameData> names = GetController.NameDataList;
                        int max = names.Max(x => x.Binding) + 1;
                        names.Add(new NameData() { Binding = max, Name = "Group " + max });
                    }
                }
            }

            GL.EndHorizontal();
            return new WindowReturn();
        }

        protected virtual void GroupWindowScroll()
        {
            List<NameData> names = GetController.NameDataList;
            for(int i = 0; i < names.Count; i++)
            {
                if(names[i].Binding < Constants.ClothingLength)
                    continue;
                if(!NameControls.TryGetValue(names[i], out NameDataControl controls))
                {
                    NameControls[names[i]] = controls = new NameDataControl(names[i], GetControl);
                }

                GL.BeginVertical(GUI.skin.box);
                {
                    GL.BeginHorizontal(GUI.skin.box);
                    {
                        controls.DrawGroupRename(SelectedSlot);
                    }

                    GL.EndHorizontal();

                    controls.DrawGroupSetting();

                    controls.DrawStateRename(SelectedSlot);
                }

                GL.EndVertical();
            }
        }

        protected virtual WindowReturn PresetWindowDraw()
        {
            GL.BeginHorizontal();
            {
                PresetView.Draw();

                GL.FlexibleSpace();
                if(Button("Settings", "Open Settings", false))
                    ToggleSettingsWindow();
                GL.Space(10);
                if(Button("X", "Close this window", false))
                    TogglePresetWindow();
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                Label("Search:", "Filter lists by Search", false);
                PresetFilter.ActiveDraw();
            }

            GL.EndHorizontal();

            switch(PresetView.Value)
            {
                case 0:
                    PresetFolderDraw();
                    break;
                case 1:
                    PresetSingleDraw();
                    break;
                default:
                    break;
            }
            //TODO: Remember to make presets
            return new WindowReturn(PresetView.Value);
        }

        protected virtual void PresetFolderDraw()
        {
            GL.BeginHorizontal();
            {
                if(Button("New Preset Folder", "Create new folder group"))
                {
                    PresetFolder presetFolder = new PresetFolder()
                    {
                        Name = $"Slot {SelectedSlot} Folder Preset"
                    };
                    presetFolders.Add(presetFolder);
                }
            }

            GL.EndHorizontal();
        }

        protected virtual void PresetSingleDraw()
        {
            GL.BeginHorizontal();
            {
                if(Button("New Preset"))
                {
                    PresetData presetData = PresetData.ConvertSlotData(SelectedSlotData, SelectedSlot);
                    SinglePresetDatas.Add(presetData);
                }
            }

            GL.EndHorizontal();
        }

        protected virtual void PresetSingleScroll()
        {
            string filter = PresetFilter.GUIContent.text.Trim();
            bool validfilter = filter.Length > 0;
            for(int i = 0; i < SinglePresetDatas.Count; i++)
            {
                if(!SingleControls.TryGetValue(SinglePresetDatas[i], out PresetContols preset))
                {
                    preset = SingleControls[SinglePresetDatas[i]] = new PresetContols(SinglePresetDatas[i], SinglePresetDatas);
                }

                if(validfilter && preset.Filter(filter))
                    continue;

                GL.BeginVertical(GUI.skin.box);
                {
                    preset.Draw(GetController, SelectedSlot);
                }

                GL.EndVertical();
            }
        }

        protected virtual void PresetFolderScroll()
        {
            string filter = PresetFilter.GUIContent.text.Trim();
            bool validfilter = filter.Length > 0;

            for(int i = 0; i < presetFolders.Count; i++)
            {
                PresetFolder folder = presetFolders[i];
                if(!PresetFolderControls.TryGetValue(folder, out PresetFolderContol folderPreset))
                {
                    folderPreset = PresetFolderControls[folder] = new PresetFolderContol(folder, presetFolders);
                }

                if(validfilter && folderPreset.Filter(filter))
                    continue;

                GL.BeginVertical(GUI.skin.box);
                {
                    folderPreset.Draw(SelectedSlotData, SelectedSlot);
                    if(folderPreset.ShowContents)
                    {
                        List<PresetData> presets = folder.PresetDatas;
                        for(int j = 0; j < presets.Count; j++)
                        {
                            PresetData singlePreset = presets[j];
                            if(!SingleControls.TryGetValue(singlePreset, out PresetContols singlePresetControl))
                            {
                                singlePresetControl = new PresetContols(singlePreset, presets);
                                SingleControls[singlePreset] = singlePresetControl;
                            }

                            if(singlePreset.Filter(filter))
                                continue;
                            GL.BeginHorizontal(GUI.skin.box);
                            {
                                GL.Space(10);
                                GL.BeginVertical();
                                {
                                    singlePresetControl.Draw(GetController, SelectedSlot);
                                }

                                GL.EndVertical();
                            }

                            GL.EndHorizontal();
                        }
                    }
                    else
                    {
                        GL.Space(10);
                    }
                }

                GL.EndVertical();
            }
        }

        protected virtual WindowReturn SelectBindingWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if(Button("X", "Close this window", false))
                    ToggleAddBindingWindow();
            }

            GL.EndHorizontal();
            return new WindowReturn();
        }

        protected virtual void BindingWindowScroll()
        {
            SlotData slotData = SelectedSlotData;
            foreach(NameData item in GetController.NameDataList)
            {
                if(item.Binding < 0)
                    continue;
                int nameDataIndex = slotData.bindingDatas.FindIndex(x => x.NameData == item);
                if(SelectedDropDown == nameDataIndex)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                {
                    Label(item.Name);
                    if(nameDataIndex > -1)
                    {
                        if(SelectedDropDown != nameDataIndex && Button("Select", expandwidth: false))
                        {
                            SelectedDropDown = nameDataIndex;
                        }

                        if(Button("Remove", "Remove data associated with this group from accessory", false))
                        {
                            item.AssociatedSlots.Remove(SelectedSlot);
                            slotData.bindingDatas.RemoveAt(nameDataIndex);
                            GetController.SaveSlotData(SelectedSlot);
                            if(SelectedDropDown >= slotData.bindingDatas.Count || SelectedDropDown < 0)
                                SelectedDropDown = 0;
                        }
                    }
                    else
                    {
                        GL.FlexibleSpace();
                        if(Button("Add", "Add this binding type to accessory", expandwidth: false))
                        {
                            item.AssociatedSlots.Add(SelectedSlot);
                            slotData.bindingDatas.Add(new BindingData() { NameData = item, States = item.GetDefaultStates(SelectedSlot) });
                            GetController.SaveSlotData(SelectedSlot);
                        }
                    }
                }

                GL.EndHorizontal();
            }
        }

        protected virtual WindowReturn PreviewWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                GL.Space(10);
                if(Button("X", "Close this window", false))
                {
                    TogglePreviewWindow();
                }
            }

            GL.EndHorizontal();
            return new WindowReturn();
        }

        protected virtual void PreviewScroll()
        {
            List<NameData> nameDatas = GetController.NameDataList;
            List<BindingData> bindingDatas = SelectedSlotData.bindingDatas;

            foreach(NameData nameData in nameDatas)
            {
                if(!NameControls.TryGetValue(nameData, out NameDataControl controls))
                {
                    NameControls[nameData] = controls = new NameDataControl(nameData, GetControl);
                }

                if(nameData.Binding < 0)
                    continue;
                if(bindingDatas.Any(x => x.NameData == nameData))
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                controls.DrawStatePreview();
                GL.EndHorizontal();
            }

            string currentParent = GetController.PartsArray[SelectedSlot].parentKey;
            Dictionary<string, CharaEvent.ParentedData> dict = GetController.ParentedNameDictionary;
            for(int i = 0; i < dict.Count; i++)
            {
                KeyValuePair<string, CharaEvent.ParentedData> keyValue = dict.ElementAt(i);
                if(currentParent == keyValue.Key)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                {
                    Label(keyValue.Key, "Display accessory if parented toggle is also enabled", false);
                    if(Button(keyValue.Value.Show ? "Show" : "Hide", "If accessory should be shown or hidden with current state"))
                    {
                        keyValue.Value.Toggle();
                        GetController.RefreshSlots(keyValue.Value.AssociateSlots);
                    }
                }

                GL.EndHorizontal();
            }
        }

        protected virtual WindowReturn SettingWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                if(Button("G-", "Decrease GUI size", false))
                {
                    Settings.MakerFontSize.Value = Math.Max(0, Settings.MakerFontSize.Value - 1);
                    Settings.Logger.LogMessage(Settings.MakerFontSize.Value);
                    SetFontSize(Settings.MakerFontSize.Value);
                    WindowGUI.SaveEvent = false;
                }

                if(Button("G+", "Increase GUI size", false))
                {
                    Settings.MakerFontSize.Value += 1;
                    Settings.Logger.LogMessage(Settings.MakerFontSize.Value);
                    SetFontSize(Settings.MakerFontSize.Value);
                    WindowGUI.SaveEvent = false;
                }

                GL.Space(10);

                if(Button("X", "Close this window", false))
                    _settingWindow.ToggleShow();
            }

            GL.EndHorizontal();
            return new WindowReturn();
        }

        protected virtual void SettingScroll()
        {
            GL.BeginVertical(GUI.skin.box);
            {
                GL.BeginHorizontal();
                {
                    Label("Settings:", expandwidth: false);
                    WindowGUI.ManualSaveDraw();
                }

                GL.EndHorizontal();
                foreach(WindowGUI item in WindowGUI.windowGUIs)
                {
                    item.TransparencyDraw();
                }
            }

            GL.EndVertical();
            GL.Space(5);
            GL.BeginVertical(GUI.skin.box);
            {
                Label("Accessory State Sync:");

                Settings.ASS_SAVE.Value = Toggle(Settings.ASS_SAVE.Value, "Save Accessory State Sync Format", "Enable attempting to save using Madevil's format so it can maybe work with ASS");
                GL.BeginHorizontal();
                {
                    Label("On ASS Save: ShoeType ", "Chose which shoetype should be used when saving with Madevil's format");
                    _AssPreference.Draw();
                }

                GL.EndHorizontal();
            }

            GL.EndVertical();
            GL.Space(5);
            GL.BeginVertical(GUI.skin.box);
            {
                Label("Presets:");

                GL.BeginHorizontal();
                {
                    Label("Clear Loaded Presets", "", false);
                    GL.FlexibleSpace();
                    if(Button("Clear", "Empty Loaded Preset list", false))
                    {
                        SinglePresetDatas.Clear();
                        SingleControls.Clear();
                        presetFolders.Clear();
                        PresetFolderControls.Clear();
                    }
                }

                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    Label("Load Default Presets", "", false);
                    GL.FlexibleSpace();
                    if(Button("Load Defaults", "Recreate Hard Coded Presets", false))
                    {

                    }
                }

                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    Label("Load Presets From disk", "", false);
                    GL.FlexibleSpace();
                    if(Button("Disk Load", "Reload Presets from Disk", false))
                    {
                        DiskLoad();
                    }
                }

                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    Label("Delete Saved Presets", "", false);
                    GL.FlexibleSpace();
                    if(Button("Disk Delete", "Delete Preset Cache from disk", false))
                    {
                        Presets.DeleteCache();
                    }
                }

                GL.EndHorizontal();
            }

            GL.EndVertical();
        }

        protected void DiskLoad()
        {
            Presets.LoadAllPresets(out List<PresetData> singles, out List<PresetFolder> folders);
            foreach(PresetData preset in singles)
            {
                if(SinglePresetDatas.Any(x => x.FileName == preset.FileName))
                    continue;
                SinglePresetDatas.Add(preset);
            }

            foreach(PresetFolder preset in folders)
            {
                if(presetFolders.Any(x => x.FileName == preset.FileName))
                    continue;
                presetFolders.Add(preset);
            }
        }

        public abstract void OnGUI();

        protected BindingData GetSelectedBindingData(SlotData slotData)
        {
            if(SelectedDropDown >= slotData.bindingDatas.Count || SelectedDropDown < 0)
            {
                SelectedDropDown = 0;
            }

            if(slotData.bindingDatas.Count == 0)
                return null;

            return slotData.bindingDatas[SelectedDropDown];
        }
    }
}
