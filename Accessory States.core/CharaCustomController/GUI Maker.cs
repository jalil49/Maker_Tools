using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;


namespace Accessory_States
{
    partial class CharaEvent : CharaCustomFunctionController
    {
        internal readonly Dictionary<int, int[]> GUI_Custom_Dict = new Dictionary<int, int[]>();

        static readonly Dictionary<int, SlotData> GUI_int_state_copy_Dict = new Dictionary<int, SlotData>();

        internal readonly Dictionary<string, bool> GUI_Parent_Dict = new Dictionary<string, bool>();

        private static bool ShowToggleInterface = false;

        private static bool ShowCustomGui = false;

        static Rect _Custom_GroupsRect;

        private static Vector2 _accessorySlotsScrollPos = new Vector2();

        private static Vector2 NameScrolling = new Vector2();
        private static Vector2 StateScrolling = new Vector2();
        private static bool mouseassigned = false;
        private static bool autoscale = true;
        private static int minilimit = 3;
        private Rect screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f), (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        static bool showdelete = false;

        static int tabvalue = 0;
        static int Selectedkind = -1;
        static readonly string[] statenameassign = new string[] { "0", "state 0" };
        static string defaultstatetext = "0";
        private static readonly string[] shoetypetext = new string[] { "Inside", "Outside", "Both" };
        private static readonly string[] TabText = new string[] { "Assign", "Settings" };

        private void OnGUI()
        {
            if (MakerAPI.IsInterfaceVisible())
            {
                if (ShowToggleInterface)
                    DrawToggleButtons();
                if (ShowCustomGui && AccessoriesApi.AccessoryCanvasVisible)
                    DrawCustomGUI();
            }
        }

        private static void GUIToggle()
        {
            ShowCustomGui = !ShowCustomGui;
        }

        private static void SetupInterface()
        {
            var Height = 50f + Screen.height / 2;
            var Margin = 5f;
            var Width = 130f;

            var distanceFromRightEdge = Screen.width / 10f;
            var x = Screen.width - distanceFromRightEdge - Width - Margin;
            var windowRect = new Rect(x, Margin, Width, Height);

            _Custom_GroupsRect = windowRect;
            _Custom_GroupsRect.x += 7;
            _Custom_GroupsRect.width -= 7;
            _Custom_GroupsRect.y += Height + Margin;
            _Custom_GroupsRect.height = 300f;
        }

        private void UpdateTogglesGUI()
        {
            var i = Constants.ClothingLength;
            foreach (var custom in Names)
            {
                if (!GUI_Custom_Dict.TryGetValue(i, out var States))
                {
                    GUI_Custom_Dict[i] = States = new int[] { 0, 2 };
                }
                States[1] = MaxState(i) + 2;
                i++;
            }
        }

        private void DrawCustomGUI()
        {
            IMGUIUtils.DrawSolidBox(screenRect);
            screenRect = GUILayout.Window(2900, screenRect, CustomGui, $"Accessory States Gui: Slot {AccessoriesApi.SelectedMakerAccSlot + 1}");
        }

        private void CustomGui(int id)
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            if (!SlotInfo.TryGetValue(slot, out var slotdata))
            {
                SlotInfo[slot] = slotdata = new SlotData("", -1, new List<int[]>() { new int[] { 0, 3 } }, 2, false);
            }

            var Binding = slotdata.Binding;
            var stateinfo = slotdata.States;

            var valid = AccessoriesApi.GetPartsInfo(slot).type != 120;

            var statelimits = StateLimitsCheck(Binding, MaxState(stateinfo));
            var binded = -1 < Binding;
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
                            if (Button("New Custom", true))
                            {
                                var max = 10 + Names.Count;
                                var name = "Custom " + max;
                                Names.Add(new NameData() { Name = name });
                                //AddGroup(max, name);
                                UpdateMakerDropboxes();
                            }

                            if (Button("Update Dropbox", false))
                            {
                                UpdateMakerDropboxes();
                            }
                            if (binded && Button("Un-Bind"))
                            {
                                slotdata.Binding = -1;
                                GroupSelect.SetValue(slot, 0);
                            }
                            else
                            {
                                GUILayout.FlexibleSpace();
                            }
                        }
                        GUILayout.EndHorizontal();

                        NameScrolling = GUILayout.BeginScrollView(NameScrolling);
                        {
                            DefinedGroups(ref slotdata, statelimits, slot, valid);
                            CustomGroups(ref slotdata, statelimits, slot, valid);
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        tabvalue = GUILayout.Toolbar(tabvalue, TabText);
                        switch (tabvalue)
                        {
                            case 0:
                                DrawStatesWindow(ref slotdata, valid, statelimits, slot);
                                break;
                            case 1:
                                DrawSettingsWindow();
                                break;
                            default:
                                break;
                        }
                    }
                    GUILayout.EndVertical();

                    screenRect = IMGUIUtils.DragResizeEatWindow(800, screenRect);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawSettingsWindow()
        {
            if (Selectedkind == -1)
            {
                Label("Please select a group");
                return;
            }
            GUILayout.BeginHorizontal();
            {
                Label("Change all category");
                if (Button("Main"))
                {
                    ChangeBindingSub(0);
                }
                if (Button("Sub"))
                {
                    ChangeBindingSub(1);
                }
            }
            GUILayout.EndHorizontal();

            DrawStatesNameWindow();
        }

        private void DrawStatesNameWindow()
        {
            if (Names.Count <= Selectedkind - Constants.ClothingLength)
            {
                Label("Custom group not selected");
                return;
            }
            var statenamedict = Names[Selectedkind - Constants.ClothingLength].StateNames;

            GUILayout.BeginHorizontal();
            {
                var currentstate = Names[Selectedkind - Constants.ClothingLength].DefaultState;
                Label($"Default state {currentstate}: {StateDescription(Selectedkind, currentstate)}", false);
                defaultstatetext = TextField(defaultstatetext);
                if (int.TryParse(defaultstatetext, out var newdefault) && newdefault > -1 && newdefault != currentstate && newdefault <= GUI_Custom_Dict[Selectedkind][1] && Button("Apply"))
                {
                    Names[Selectedkind - Constants.ClothingLength].DefaultState = newdefault;
                    SaveCoordinateData();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                statenameassign[0] = TextField(statenameassign[0], true);
                statenameassign[1] = TextField(statenameassign[1], true);
                if (int.TryParse(statenameassign[0], out var key) && Button("Add State Name"))
                {
                    statenamedict[key] = statenameassign[1];
                }
            }
            GUILayout.EndHorizontal();


            StateScrolling = GUILayout.BeginScrollView(StateScrolling);
            {
                for (int i = 0, n = statenamedict.Count; i < n; i++)
                {
                    var key = statenamedict.ElementAt(i).Key;
                    GUILayout.BeginHorizontal();
                    {
                        Label($"State: {key}");
                        statenamedict[key] = TextField(statenamedict[key], true);
                        if (Button("Delete", false))
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

        private void DrawStatesWindow(ref SlotData slotdata, bool valid, int statelimits, int slot)
        {
            var binding = slotdata.Binding;
            var stateinfo = slotdata.States;
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.FlexibleSpace();
                Label((binding < Constants.ClothingLength) ? Constants.ConstantOutfitNames[binding] : Names[binding - Constants.ClothingLength].Name, false);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                CustomLimit(binding);

                if (valid)
                {
                    slotdata.Parented = Toggle(slotdata.Parented, "Parent Bind");
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    {
                        Label("Shoe");
                        for (byte i = 0; i < 3; i++)
                        {
                            DrawShoeButton(slot, ref slotdata, i);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                else
                    GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();

            }
            GUILayout.EndHorizontal();
            if (binding > -1)
            {
                StateScrolling = GUILayout.BeginScrollView(StateScrolling);
                if (Button("Add new state"))
                {
                    stateinfo.Add(new int[] { 0, (binding >= Constants.ClothingLength) ? statelimits - 1 : statelimits });
                }
                if (stateinfo.Count > 1 && Button("Remove last state"))
                {
                    stateinfo.RemoveAt(stateinfo.Count - 1);
                }
                for (int i = 0, n = stateinfo.Count; i < n; i++)
                {
                    DrawStartStopSlider(ref stateinfo, i, statelimits, binding, slot);
                }
                GUILayout.EndScrollView();
            }
            else
            {
                Label("Unassigned", true);
            }
        }

        private void DrawShoeButton(int slot, ref SlotData slotdata, byte shoetype)
        {
            if (shoetype != slotdata.ShoeType)
            {
                var show = true;
                switch (slotdata.Binding)
                {
                    case 7:
                        if (shoetype == 1)
                        {
                            show = false;
                        }
                        break;
                    case 8:
                        if (shoetype == 0)
                        {
                            show = false;
                        }
                        break;
                    default:
                        break;
                }
                if (show && Button(shoetypetext[shoetype], false))
                {
                    slotdata.ShoeType = shoetype;
                    if (shoetype < 2 && ChaControl.fileStatus.shoesType != shoetype)
                    {
                        show = false;
                    }
                    ChaControl.SetAccessoryState(slot, show);
                    SaveSlotData(slot, slotdata);
                }
                return;
            }
            Label(shoetypetext[shoetype], true);
        }

        private static void CustomLimit(int binding)
        {
            if (binding >= Constants.ClothingLength)
            {
                Label("Limit: " + minilimit);
                if (Button("-1"))
                {
                    minilimit = Math.Max(1, --minilimit);
                }
                if (Button("+1"))
                {
                    minilimit++;
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
            }
        }

        private void DefinedGroups(ref SlotData slotdata, int limit, int slot, bool valid)
        {
            var currentbinding = slotdata.Binding;
            var cloth = ChaControl.nowCoordinate.clothes.parts;
            foreach (var item in Constants.ConstantOutfitNames)
            {
                var clothnum = item.Key;
                if (clothnum < 0 || cloth[clothnum].id == 0)
                {
                    continue;
                }
                else if (0 < clothnum && clothnum < 4)
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

                    if (notcloth && notcloth == comparison)
                    {
                        continue;
                    }
                }

                GUILayout.BeginVertical();
                {
                    if (clothnum == currentbinding)
                        GUILayout.BeginHorizontal(GUI.skin.box);
                    else
                        GUILayout.BeginHorizontal();
                    {
                        Label(item.Value);
                        if (valid)
                            DrawApplyBindingButton(ref slotdata, currentbinding, clothnum, clothnum + 1, slot);

                        if (item.Key == currentbinding)
                        {
                            DrawCopyPasteButtons(ref slotdata, currentbinding, slot);
                            DrawResetButton(ref slotdata, limit, slot);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        private void CustomGroups(ref SlotData slotinfo, int limit, int slot, bool valid)
        {
            var currentbinding = slotinfo.Binding;
            var count = Constants.ClothingLength;

            for (var i = 0; i < Names.Count; i++)
            {
                var element = Names[i];
                var namebinding = i + Constants.ClothingLength;
                GUILayout.BeginVertical();
                {
                    if (namebinding == currentbinding)
                        GUILayout.BeginHorizontal(GUI.skin.box);
                    else
                        GUILayout.BeginHorizontal();
                    {
                        var text = TextField(element.Name, true);
                        if (text != element.Name)
                        {
                            Names[i].Name = text;
                            foreach (var item in SlotInfo.Where(x => x.Value.Binding == namebinding))
                            {
                                item.Value.GroupName = text;
                                SaveSlotData(item.Key);
                            }
                        }

                        if (valid)
                            DrawApplyBindingButton(ref slotinfo, currentbinding, i + Constants.ClothingLength, i + count, slot);

                        if (namebinding == currentbinding)
                        {
                            DrawCopyPasteButtons(ref slotinfo, currentbinding, slot);
                            DrawResetButton(ref slotinfo, (autoscale) ? limit : limit - 1, slot);
                        }

                        if (tabvalue == 1 && Selectedkind != namebinding && Button("Select", false))
                        {
                            Selectedkind = namebinding;
                        }

                        if (showdelete && Button("Delete", false))
                        {
                            Names.Remove(element);
                            //DeleteGroup(element.Key);
                            var removelist = SlotInfo.Where(x => x.Value.Binding == namebinding).ToList();
                            for (var j = 0; j < removelist.Count; j++)
                            {
                                GroupSelect.SetValue(removelist.Count, -1);
                                SlotInfo.Remove(removelist[i].Key);
                            }
                            GUI_int_state_copy_Dict.Remove(i);
                            GUI_Custom_Dict.Remove(i);

                            i--;
                            UpdateMakerDropboxes();
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
                Label($"Start: {StateDescription(binding, stateref[0])}", true);
                GUILayout.FlexibleSpace();
                Label($"Stop: {StateDescription(binding, stateref[1])}", false);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                var round = HorizontalSlider(stateref[0], 0, limit, true);
                if (round != stateref[0] && round <= stateref[1])
                {
                    stateref[0] = round;
                    if (index == 0)
                        StartState.SetValue(slot, round, true);
                    //else
                    //    ChangeTriggerProperty(binding);
                    SaveSlotData(slot);
                }
                var text = TextField(round.ToString(), false);
                if (int.TryParse(text, out var textvalue) && textvalue != round && textvalue <= stateref[1])
                {
                    stateref[0] = textvalue;
                    if (index == 0)
                        StartState.SetValue(slot, textvalue);
                    //else
                    //    ChangeTriggerProperty(binding);
                    SaveSlotData(slot);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                var round = HorizontalSlider(stateref[1], 0, limit, true);
                if (round != stateref[1] && round >= stateref[0])
                {
                    stateref[1] = round;
                    if (index == 0)
                        EndState.SetValue(slot, round, true);
                    else
                    {
                        UpdateAccessoryshow(SlotInfo[slot], slot);
                        //ChangeTriggerProperty(binding);
                        UpdateTogglesGUI();
                    }
                    SaveSlotData(slot);
                }
                var text = TextField(round.ToString(), false);
                if (int.TryParse(text, out var textvalue) && textvalue != round && textvalue >= stateref[0] && (autoscale || textvalue <= limit))
                {
                    stateref[1] = textvalue;
                    if (index == 0)
                        EndState.SetValue(slot, textvalue);
                    else
                    {
                        UpdateAccessoryshow(SlotInfo[slot], slot);
                        //ChangeTriggerProperty(binding);
                        UpdateTogglesGUI();
                    }
                    SaveSlotData(slot);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawApplyBindingButton(ref SlotData slotinfo, int currentbinding, int newbinding, int index, int slot)
        {
            if (newbinding != currentbinding && Button("Apply", false))
            {
                var shoetype = slotinfo.ShoeType;
                if (newbinding == 7 && shoetype == 1) shoetype = 0;
                if (newbinding == 8 && shoetype == 0) shoetype = 1;
                slotinfo.ShoeType = shoetype;
                GroupSelect.SetValue(slot, index, true);
                var max = StateLimitsCheck(newbinding);
                var states = slotinfo.States;
                states.Clear();
                states.Add(new int[] { 0, max });
                StartState.SetValue(slot, 0, true);
                EndState.SetValue(slot, max, true);
                UpdateTogglesGUI();
                SaveSlotData(slot);
            }
        }

        private static void DrawCopyPasteButtons(ref SlotData currentslotdata, int currentbinding, int slot)
        {
            if (Button("Copy", false))
            {
                GUI_int_state_copy_Dict[currentbinding] = new SlotData(currentslotdata);
            }
            if (GUI_int_state_copy_Dict.TryGetValue(currentbinding, out var list) && Button("Paste", false))
            {
                currentslotdata.ShoeType = list.ShoeType;
                currentslotdata.Parented = list.Parented;
                currentslotdata.States = list.States;
                var firststate = list.States[0];
                StartState.SetValue(slot, firststate[0]);
                EndState.SetValue(slot, firststate[1]);
                ControllerGet.SaveSlotData(slot);
            }
        }

        private void Topoptions()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                Settings.DrawFontSize();
                if (Input.GetMouseButtonDown(0) && !mouseassigned && screenRect.Contains(Input.mousePosition))
                {
                    StartCoroutine(DragEvent());
                }
                if (Button("X", false))
                {
                    ShowCustomGui = false;
                }
            }
            GUILayout.EndHorizontal();
        }

        public void MakerSuboptions()
        {
            GUILayout.BeginHorizontal();
            {
                showdelete = Toggle(showdelete, "Enable Delete", false);
                autoscale = Toggle(autoscale, "Auto increase custom Limit", false);
                if (Button("Refresh"))
                {
                    Refresh();
                }
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawResetButton(ref SlotData slotinfo, int limit, int slot)
        {
            if (Button("Reset", false))
            {
                var states = slotinfo.States;
                states.Clear();
                states.Add(new int[] { 0, limit });
                StartState.SetValue(slot, 0, true);
                EndState.SetValue(slot, limit, true);
                ControllerGet.SaveSlotData(slot);
            }
        }


        #region ClothingStateMenu LookALike
        private void DrawToggleButtons()
        {
            GUILayout.BeginArea(_Custom_GroupsRect);
            {
                GUILayout.BeginScrollView(_accessorySlotsScrollPos);
                {
                    GUILayout.BeginVertical();
                    {
                        //Custom Groups
                        var i = Constants.ClothingLength;
                        foreach (var item in Names)
                        {
                            DrawCustomButton(i, item.Name);
                            i++;
                        }
                        //Parent
                        foreach (var item in ParentedNameDictionary.Keys)
                        {
                            DrawParentButton(item);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        private void DrawCustomButton(int kind, string name)
        {
            if (!GUI_Custom_Dict.TryGetValue(kind, out var State))
            {
                GUI_Custom_Dict[kind] = State = new int[] { 0, 2 };
            }

            if (Button($"{name}: {State[0]}"))
            {
                State[0] = (State[0] + 1) % State[1];
                CustomGroups(kind, State[0]);
            }
            GUILayout.Space(-5);
        }

        private void DrawParentButton(string Parent)
        {
            if (!GUI_Parent_Dict.TryGetValue(Parent, out var isOn))
            {
                isOn = true;
            }
            if (Button($"{Parent}: {(isOn ? "On" : "Off")}"))
            {
                isOn = !isOn;
                ParentToggle(Parent, isOn);
            }
            GUILayout.Space(-5);
            GUI_Parent_Dict[Parent] = isOn;
        }
        #endregion

        internal string StateDescription(int binding, int state)
        {
            var statename = state.ToString();

            if (binding >= Constants.ClothingLength && binding < Constants.ClothingLength + Names.Count)
            {
                var index = binding - Constants.ClothingLength;
                if (index >= Names.Count)
                {
                    return "Error";
                }
                if (Names[index].StateNames.TryGetValue(state, out statename))
                {
                    return statename;
                }
                return $"state {state}";
            }

            switch (state)
            {
                case 0:
                    statename += " Dressed"; break;
                case 1:
                    statename += " Half 1"; break;
                case 2:
                    statename += " Half 2"; break;
                case 3:
                    statename += " Undressed"; break;
                default:
                    statename += " Error";
                    break;
            }
            return statename;
        }

        private static int StateLimitsCheck(int Binding, int currentmax)
        {
            if (Binding < 9)
            {
                return 3;
            }
            if (autoscale)
            {
                return Math.Max(1, currentmax + 1);
            }
            return Math.Max(minilimit, currentmax);
        }
        private int StateLimitsCheck(int Binding)
        {
            if (!SlotInfo.TryGetValue(AccessoriesApi.SelectedMakerAccSlot, out var stateinfo))
            {
                return 3;
            }
            return StateLimitsCheck(Binding, MaxState(stateinfo.States));
        }

        private IEnumerator<int> DragEvent()
        {
            var pos = Input.mousePosition;
            Vector2 mousepos = pos;
            mouseassigned = true;
            var mousebuttonup = false;
            for (var i = 0; i < 20; i++)
            {
                mousebuttonup = Input.GetMouseButtonUp(0);
                yield return 0;
            }
            while (!mousebuttonup)
            {
                mousebuttonup = Input.GetMouseButtonUp(0);
                screenRect.position += (Vector2)pos - mousepos;
                mousepos = pos;
                yield return 0;
            }
            yield return 0;
            mouseassigned = false;
        }

        private static bool Toggle(bool value, string text, bool expandwidth = true)
        {
            return GUILayout.Toggle(value, text, Settings.ToggleStyle, GUILayout.ExpandWidth(expandwidth));
        }
        private static bool Button(string text, bool expandwidth = true)
        {
            return GUILayout.Button(text, Settings.ButtonStyle, GUILayout.ExpandWidth(expandwidth));
        }
        private static string TextField(string text, bool expandwidth = true)
        {
            return GUILayout.TextField(text, Settings.FieldStyle, GUILayout.ExpandWidth(expandwidth));
        }
        private static int HorizontalSlider(int value, int start, int stop, bool expandwidth = true)
        {
            return Mathf.RoundToInt(GUILayout.HorizontalSlider(value, start, stop, Settings.SliderStyle, Settings.SliderThumbStyle, GUILayout.ExpandWidth(expandwidth)));
        }
        private static void Label(string text, bool expandwidth = true)
        {
            GUILayout.Label(text, Settings.LabelStyle, GUILayout.ExpandWidth(expandwidth));
        }
    }
}
