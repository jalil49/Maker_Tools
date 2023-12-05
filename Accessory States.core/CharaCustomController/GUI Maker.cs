using System;
using System.Collections.Generic;
using System.Linq;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Utilities;
using UnityEngine;

namespace Accessory_States
{
    partial class CharaEvent : CharaCustomFunctionController
    {
        private static readonly Dictionary<int, SlotData> GUIINTStateCopyDict = new Dictionary<int, SlotData>();

        private static bool _showToggleInterface;

        private static bool _showCustomGui;

        private static Rect _customGroupsRect;

        private static readonly Vector2 _accessorySlotsScrollPos = new Vector2();

        private static Vector2 _nameScrolling;
        private static Vector2 _stateScrolling;
        private static bool _mouseassigned;
        private static bool _autoscale = true;
        private static int _minilimit = 3;

        private static bool _showdelete;

        private static int _tabvalue;
        private static int _selectedkind = -1;
        private static readonly string[] Statenameassign = { "0", "state 0" };

        private static readonly string[] Shoetypetext = { "Inside", "Outside", "Both" };
        private static readonly string[] TabText = { "Assign", "Settings" };
        internal readonly Dictionary<int, int[]> GUICustomDict = new Dictionary<int, int[]>();

        internal readonly Dictionary<string, bool> GUIParentDict = new Dictionary<string, bool>();

        private Rect _screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f),
            (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        private static GUIStyle Labelstyle => Settings.Labelstyle;
        private static GUIStyle Buttonstyle => Settings.Buttonstyle;
        private static GUIStyle Fieldstyle => Settings.Fieldstyle;
        private static GUIStyle Togglestyle => Settings.Togglestyle;
        private static GUIStyle Sliderstyle => Settings.Sliderstyle;
        private static GUIStyle Sliderthumbstyle => Settings.Sliderthumbstyle;

        private void OnGUI()
        {
            if (MakerAPI.IsInterfaceVisible())
            {
                if (_showToggleInterface)
                    DrawToggleButtons();
                if (_showCustomGui && AccessoriesApi.AccessoryCanvasVisible)
                    DrawCustomGUI();
            }
        }

        private static void GUI_Toggle()
        {
            _showCustomGui = !_showCustomGui;
        }

        private static void SetupInterface()
        {
            var height = 50f + Screen.height / 2f;
            var margin = 5f;
            var width = 130f;

            var distanceFromRightEdge = Screen.width / 10f;
            var x = Screen.width - distanceFromRightEdge - width - margin;
            var windowRect = new Rect(x, margin, width, height);

            _customGroupsRect = windowRect;
            _customGroupsRect.x += 7;
            _customGroupsRect.width -= 7;
            _customGroupsRect.y += height + margin;
            _customGroupsRect.height = 300f;
        }

        private void Update_Toggles_GUI()
        {
            foreach (var custom in Names)
            {
                if (!GUICustomDict.TryGetValue(custom.Key, out var states))
                {
                    states = new[] { 0, 2 };
                    GUICustomDict[custom.Key] = states;
                }

                states[1] = MaxState(custom.Key) + 2;
            }
        }

        private void DrawCustomGUI()
        {
            IMGUIUtils.DrawSolidBox(_screenRect);
            _screenRect = GUILayout.Window(2900, _screenRect, CustomGui,
                $"Accessory States Gui: Slot {AccessoriesApi.SelectedMakerAccSlot + 1}");
        }

        private void CustomGui(int id)
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            if (!SlotInfo.TryGetValue(slot, out var slotdata))
            {
                slotdata = new SlotData(-1, new List<int[]> { new[] { 0, 3 } }, 2, false);
                SlotInfo[slot] = slotdata;
            }

            var binding = slotdata.Binding;
            var stateinfo = slotdata.States;

            var valid = AccessoriesApi.GetPartsInfo(slot).type != 120;

            var statelimits = StateLimitsCheck(binding, MaxState(stateinfo));
            var binded = -1 < binding;
            GUILayout.BeginVertical();
            {
                Topoptions();
                MakerSuboptions();
                GUILayout.BeginHorizontal();
                {
                    //Groups
                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("New Custom", Buttonstyle))
                            {
                                var max = 10;
                                while (Names.ContainsKey(max)) max++;
                                var name = "Custom " + max;
                                Names[max] = new NameData { Name = name };
                                AddGroup(max, name);
                                Update_Drop_boxes();
                            }

                            if (GUILayout.Button("Update Dropbox", Buttonstyle, GUILayout.ExpandWidth(false)))
                                Update_Drop_boxes();
                            if (binded && GUILayout.Button("Un-Bind", Buttonstyle))
                            {
                                slotdata.Binding = -1;
                                _accAppearanceDropdown.SetValue(slot, 0);
                            }
                            else
                            {
                                GUILayout.FlexibleSpace();
                            }
                        }
                        GUILayout.EndHorizontal();

                        _nameScrolling = GUILayout.BeginScrollView(_nameScrolling);
                        {
                            DefinedGroups(ref slotdata, statelimits, slot, valid);
                            CustomGroups(ref slotdata, statelimits, slot, valid);
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        _tabvalue = GUILayout.Toolbar(_tabvalue, TabText, Buttonstyle);
                        switch (_tabvalue)
                        {
                            case 0:
                                DrawStatesWindow(ref slotdata, valid, statelimits, slot);
                                break;
                            case 1:
                                DrawSettingsWindow();
                                break;
                        }
                    }
                    GUILayout.EndVertical();

                    _screenRect = IMGUIUtils.DragResizeEatWindow(800, _screenRect);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawSettingsWindow()
        {
            //if (Selectedkind == -1)
            //{
            //    GUILayout.Label("Please select a group", labelstyle);
            //    return;
            //}
            //GUILayout.BeginHorizontal();
            //{
            //    GUILayout.Label("Change all subcategory", labelstyle);
            //    if (GUILayout.Button("Show", buttonstyle))
            //    {
            //        ChangeBindingSub(0);
            //    }
            //    if (GUILayout.Button("Hide", buttonstyle))
            //    {
            //        ChangeBindingSub(1);
            //    }
            //}
            //GUILayout.EndHorizontal();

            DrawStatesNameWindow();
        }

        private void DrawStatesNameWindow()
        {
            if (!Names.ContainsKey(_selectedkind))
            {
                GUILayout.Label("Custom group not selected", Labelstyle);
                return;
            }

            var statenamedict = Names[_selectedkind].StateNames;

            GUILayout.BeginHorizontal();
            {
                Statenameassign[0] = GUILayout.TextField(Statenameassign[0], Fieldstyle);
                Statenameassign[1] = GUILayout.TextField(Statenameassign[1], Fieldstyle);
                if (GUILayout.Button("Add State Name", Buttonstyle))
                    if (int.TryParse(Statenameassign[0], out var key))
                        statenamedict[key] = Statenameassign[1];
            }
            GUILayout.EndHorizontal();


            _stateScrolling = GUILayout.BeginScrollView(_stateScrolling);
            {
                for (int i = 0, n = statenamedict.Count; i < n; i++)
                {
                    var key = statenamedict.ElementAt(i).Key;
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label($"State: {key}", Labelstyle);
                        statenamedict[key] = GUILayout.TextField(statenamedict[key], Fieldstyle);
                        if (GUILayout.Button("Delete", Buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            statenamedict.Remove(key);
                            i--;
                            n--;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawStatesWindow(ref SlotData slotData, bool valid, int statelimits, int slot)
        {
            var binding = slotData.Binding;
            var stateinfo = slotData.States;
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(binding <= MaxDefinedKey ? Constants.ConstantOutfitNames[binding] : Names[binding].Name,
                    Labelstyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                CustomLimit(binding);

                if (valid)
                {
                    slotData.parented = GUILayout.Toggle(slotData.parented, "Parent Bind", Togglestyle,
                        GUILayout.ExpandWidth(false));
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    {
                        GUILayout.Label("Shoe", Labelstyle);
                        for (byte i = 0; i < 3; i++) DrawShoeButton(slot, ref slotData, i);
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.FlexibleSpace();
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            if (binding > -1)
            {
                _stateScrolling = GUILayout.BeginScrollView(_stateScrolling);
                if (GUILayout.Button("Add new state", Buttonstyle))
                    stateinfo.Add(new[] { 0, binding > MaxDefinedKey ? statelimits - 1 : statelimits });
                if (stateinfo.Count > 1 && GUILayout.Button("Remove last state", Buttonstyle))
                    stateinfo.RemoveAt(stateinfo.Count - 1);
                for (int i = 0, n = stateinfo.Count; i < n; i++)
                    DrawStartStopSlider(ref stateinfo, i, statelimits, binding, slot);
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("Unassigned", Labelstyle, GUILayout.ExpandWidth(true));
            }
        }

        private void DrawShoeButton(int slot, ref SlotData slotData, byte shoeType)
        {
            if (shoeType != slotData.ShoeType)
            {
                var show = true;
                switch (slotData.Binding)
                {
                    case 7:
                        if (shoeType == 1) show = false;
                        break;
                    case 8:
                        if (shoeType == 0) show = false;
                        break;
                }

                if (show && GUILayout.Button(Shoetypetext[shoeType], Buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    slotData.ShoeType = shoeType;
                    if (shoeType < 2 && ChaControl.fileStatus.shoesType != shoeType) show = false;
                    ChaControl.SetAccessoryState(slot, show);
                }

                return;
            }

            GUILayout.Label(Shoetypetext[shoeType], Labelstyle);
        }

        private static void CustomLimit(int binding)
        {
            if (binding > MaxDefinedKey)
            {
                GUILayout.Label("Limit: " + _minilimit, Labelstyle);
                if (GUILayout.Button("-1", Buttonstyle)) _minilimit = Math.Max(1, --_minilimit);
                if (GUILayout.Button("+1", Buttonstyle)) _minilimit++;
            }
            else
            {
                GUILayout.FlexibleSpace();
            }
        }

        private void DefinedGroups(ref SlotData slotData, int limit, int slot, bool valid)
        {
            var currentbinding = slotData.Binding;
            var cloth = ChaControl.nowCoordinate.clothes.parts;
            foreach (var item in Constants.ConstantOutfitNames)
            {
                var clothnum = item.Key;
                if (clothnum < 0 || cloth[clothnum].id == 0) continue;

                if (0 < clothnum && clothnum < 4)
                {
                    var notcloth = ClothNotData[clothnum - 1];
                    var comparison = notcloth;
                    switch (clothnum)
                    {
                        case 1:
                            comparison = ChaControl.notBot;
                            break;
                        case 2:
                            comparison = ChaControl.notBra;
                            break;
                        case 3:
                            comparison = ChaControl.notShorts;
                            break;
                    }

                    if (notcloth && notcloth == comparison) continue;
                }

                GUILayout.BeginVertical();
                {
                    if (clothnum == currentbinding)
                        GUILayout.BeginHorizontal(GUI.skin.box);
                    else
                        GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(item.Value, Labelstyle);
                        if (valid)
                            DrawApplyBindingButton(ref slotData, currentbinding, clothnum, clothnum + 1, slot);

                        if (item.Key == currentbinding)
                        {
                            DrawCopyPasteButtons(ref slotData, currentbinding, slot);
                            DrawResetButton(ref slotData, limit, slot);
                        }

                        //if (tabvalue == 1 && Selectedkind != clothnum && GUILayout.Button("Select", buttonstyle, GUILayout.ExpandWidth(false)))
                        //{
                        //    Selectedkind = clothnum;
                        //}
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        private void CustomGroups(ref SlotData slotInfo, int limit, int slot, bool valid)
        {
            var currentbinding = slotInfo.Binding;
            var namedict = Names;
            var count = Constants.ConstantOutfitNames.Count;

            for (var i = 0; i < namedict.Count; i++)
            {
                var element = namedict.ElementAt(i);

                GUILayout.BeginVertical();
                {
                    if (element.Key == currentbinding)
                        GUILayout.BeginHorizontal(GUI.skin.box);
                    else
                        GUILayout.BeginHorizontal();
                    {
                        var text = GUILayout.TextField(element.Value.Name, Fieldstyle, GUILayout.ExpandWidth(false));
                        if (text != element.Value.Name)
                        {
                            namedict[element.Key].Name = text;
                            RenameGroup(element.Key, text);
                        }

                        if (valid)
                            DrawApplyBindingButton(ref slotInfo, currentbinding, element.Key, i + count, slot);

                        if (element.Key == currentbinding)
                        {
                            DrawCopyPasteButtons(ref slotInfo, currentbinding, slot);
                            DrawResetButton(ref slotInfo, _autoscale ? limit : limit - 1, slot);
                        }

                        if (_tabvalue == 1 && _selectedkind != element.Key &&
                            GUILayout.Button("Select", Buttonstyle, GUILayout.ExpandWidth(false)))
                            _selectedkind = element.Key;

                        if (_showdelete && GUILayout.Button("Delete", Buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            namedict.Remove(element.Key);
                            DeleteGroup(element.Key);
                            var removelist = SlotInfo.Where(x => x.Value.Binding == element.Key).ToList();
                            for (var j = 0; j < removelist.Count; j++)
                            {
                                _accAppearanceDropdown.SetValue(removelist.Count, -1);
                                SlotInfo.Remove(removelist[i].Key);
                            }

                            GUIINTStateCopyDict.Remove(element.Key);
                            GUICustomDict.Remove(element.Key);

                            i--;
                            Update_Drop_boxes();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        private void DrawStartStopSlider(ref List<int[]> states, int index, int limit, int binding, int slot)
        {
            var stateref = states[index];
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"Start: {StateDescription(binding, stateref[0])}", Labelstyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Stop: {StateDescription(binding, stateref[1])}", Labelstyle);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                var round = Mathf.RoundToInt(GUILayout.HorizontalSlider(stateref[0], 0, limit, Sliderstyle,
                    Sliderthumbstyle, GUILayout.ExpandWidth(true)));
                if (round != stateref[0] && round <= stateref[1])
                {
                    stateref[0] = round;
                    if (index == 0)
                        _accAppearanceState.SetValue(slot, round, true);
                    else
                        ChangeTriggerProperty(binding);
                }

                var text = GUILayout.TextField(round.ToString(), Fieldstyle, GUILayout.ExpandWidth(false));
                if (int.TryParse(text, out var textvalue) && textvalue != round && textvalue <= stateref[1])
                {
                    stateref[0] = textvalue;
                    if (index == 0)
                        _accAppearanceState.SetValue(slot, textvalue);
                    else
                        ChangeTriggerProperty(binding);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                var round = Mathf.RoundToInt(GUILayout.HorizontalSlider(stateref[1], 0, limit, Sliderstyle,
                    Sliderthumbstyle, GUILayout.ExpandWidth(true)));
                if (round != stateref[1] && round >= stateref[0])
                {
                    stateref[1] = round;
                    if (index == 0)
                    {
                        _accAppearanceState2.SetValue(slot, round, true);
                    }
                    else
                    {
                        UpdateAccessoryshow(SlotInfo[slot], slot);
                        ChangeTriggerProperty(binding);
                        Update_Toggles_GUI();
                    }
                }

                var text = GUILayout.TextField(round.ToString(), Fieldstyle, GUILayout.ExpandWidth(false));
                if (int.TryParse(text, out var textvalue) && textvalue != round && textvalue >= stateref[0] &&
                    (_autoscale || textvalue <= limit))
                {
                    stateref[1] = textvalue;
                    if (index == 0)
                    {
                        _accAppearanceState2.SetValue(slot, textvalue);
                    }
                    else
                    {
                        UpdateAccessoryshow(SlotInfo[slot], slot);
                        ChangeTriggerProperty(binding);
                        Update_Toggles_GUI();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawApplyBindingButton(ref SlotData slotInfo, int currentbinding, int newbinding, int index,
            int slot)
        {
            if (newbinding != currentbinding && GUILayout.Button("Apply", Buttonstyle, GUILayout.ExpandWidth(false)))
            {
                var shoeType = slotInfo.ShoeType;
                if (newbinding == 7 && shoeType == 1) shoeType = 0;
                if (newbinding == 8 && shoeType == 0) shoeType = 1;
                slotInfo.ShoeType = shoeType;
                _accAppearanceDropdown.SetValue(slot, index, true);
                var max = StateLimitsCheck(newbinding);
                var states = slotInfo.States;
                states.Clear();
                states.Add(new[] { 0, max });
                _accAppearanceState.SetValue(slot, 0, true);
                _accAppearanceState2.SetValue(slot, max, true);
                Update_Toggles_GUI();
            }
        }

        private static void DrawCopyPasteButtons(ref SlotData currentslotdata, int currentbinding, int slot)
        {
            if (GUILayout.Button("Copy", Buttonstyle, GUILayout.ExpandWidth(false)))
                GUIINTStateCopyDict[currentbinding] = new SlotData(currentslotdata);
            if (GUIINTStateCopyDict.TryGetValue(currentbinding, out var list) &&
                GUILayout.Button("Paste", Buttonstyle, GUILayout.ExpandWidth(false)))
            {
                currentslotdata.ShoeType = list.ShoeType;
                currentslotdata.parented = list.parented;
                currentslotdata.States = list.States;
                var firststate = list.States[0];
                _accAppearanceState.SetValue(slot, firststate[0]);
                _accAppearanceState2.SetValue(slot, firststate[1]);
            }
        }

        private void Topoptions()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                Settings.DrawFontSize();
                if (Input.GetMouseButtonDown(0) && !_mouseassigned && _screenRect.Contains(Input.mousePosition))
                    StartCoroutine(DragEvent());
                if (GUILayout.Button("X", Buttonstyle, GUILayout.ExpandWidth(false))) _showCustomGui = false;
            }
            GUILayout.EndHorizontal();
        }

        public void MakerSuboptions()
        {
            GUILayout.BeginHorizontal();
            {
                _showdelete = GUILayout.Toggle(_showdelete, "Enable Delete", Togglestyle, GUILayout.ExpandWidth(false));
                _autoscale = GUILayout.Toggle(_autoscale, "Auto increase custom Limit", Togglestyle,
                    GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Refresh", Buttonstyle)) Refresh();
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawResetButton(ref SlotData slotInfo, int limit, int slot)
        {
            if (GUILayout.Button("Reset", Buttonstyle, GUILayout.ExpandWidth(false)))
            {
                var states = slotInfo.States;
                states.Clear();
                states.Add(new[] { 0, limit });
                _accAppearanceState.SetValue(slot, 0, true);
                _accAppearanceState2.SetValue(slot, limit, true);
            }
        }

        private static int StateLimitsCheck(int binding, int currentmax)
        {
            if (binding < 9) return 3;
            if (_autoscale) return Math.Max(1, currentmax + 1);
            return Math.Max(_minilimit, currentmax);
        }

        private int StateLimitsCheck(int binding)
        {
            if (!SlotInfo.TryGetValue(AccessoriesApi.SelectedMakerAccSlot, out var stateinfo)) return 3;
            return StateLimitsCheck(binding, MaxState(stateinfo.States));
        }

        private void DrawToggleButtons()
        {
            GUILayout.BeginArea(_customGroupsRect);
            {
                GUILayout.BeginScrollView(_accessorySlotsScrollPos);
                {
                    GUILayout.BeginVertical();
                    {
                        //Custom Groups
                        foreach (var item in Names) DrawCustomButton(item.Key, item.Value.Name);
                        //Parent
                        foreach (var item in NowParentedNameDictionary.Keys) DrawParentButton(item);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        private void DrawCustomButton(int kind, string name)
        {
            if (!GUICustomDict.TryGetValue(kind, out var state))
            {
                state = new[] { 0, 2 };
                GUICustomDict[kind] = state;
            }

            if (GUILayout.Button($"{name}: {state[0]}"))
            {
                state[0] = (state[0] + 1) % state[1];
                Custom_Groups(kind, state[0]);
            }

            GUILayout.Space(-5);
        }

        private void DrawParentButton(string parent)
        {
            if (!GUIParentDict.TryGetValue(parent, out var isOn)) isOn = true;
            if (GUILayout.Button($"{parent}: {(isOn ? "On" : "Off")}"))
            {
                isOn = !isOn;
                Parent_toggle(parent, isOn);
            }

            GUILayout.Space(-5);
            GUIParentDict[parent] = isOn;
        }

        internal string StateDescription(int binding, int state)
        {
            var statename = state.ToString();
            if (binding > MaxDefinedKey)
            {
                if (Names[binding].StateNames.TryGetValue(state, out statename)) return statename;
                return $"state {state}";
            }

            switch (state)
            {
                case 0:
                    statename += " Dressed";
                    break;
                case 1:
                    statename += " Half 1";
                    break;
                case 2:
                    statename += " Half 2";
                    break;
                case 3:
                    statename += " Undressed";
                    break;
                default:
                    statename += " Error";
                    break;
            }

            return statename;
        }

        private IEnumerator<int> DragEvent()
        {
            var pos = Input.mousePosition;
            Vector2 mousepos = pos;
            _mouseassigned = true;
            var mousebuttonup = false;
            for (var i = 0; i < 20; i++)
            {
                mousebuttonup = Input.GetMouseButtonUp(0);
                yield return 0;
            }

            while (!mousebuttonup)
            {
                mousebuttonup = Input.GetMouseButtonUp(0);
                _screenRect.position += (Vector2)pos - mousepos;
                mousepos = pos;
                yield return 0;
            }

            yield return 0;
            _mouseassigned = false;
        }
    }
}