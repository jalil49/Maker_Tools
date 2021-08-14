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
        static private readonly Dictionary<int, int[]> GUI_Custom_Dict = new Dictionary<int, int[]>();

        static private readonly Dictionary<int, Slotdata> GUI_int_state_copy_Dict = new Dictionary<int, Slotdata>();

        static private readonly Dictionary<string, bool> GUI_Parent_Dict = new Dictionary<string, bool>();

        static private bool ShowToggleInterface = false;

        static private bool ShowCustomGui = false;

        static Rect _Custom_GroupsRect;

        static private Vector2 _accessorySlotsScrollPos = new Vector2();

        static private Vector2 NameScrolling = new Vector2();
        static private Vector2 StateScrolling = new Vector2();
        static private bool mouseassigned = false;
        static private bool autoscale = true;
        static private int minilimit = 3;
        static Rect screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f), (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        static bool showdelete = false;

        static GUIStyle labelstyle;
        static GUIStyle buttonstyle;
        static GUIStyle fieldstyle;
        static GUIStyle togglestyle;
        static GUIStyle sliderstyle;
        static GUIStyle sliderthumbstyle;

        static int tabvalue = 0;
        static int Selectedkind = -1;
        static readonly string[] statenameassign = new string[] { "0", "state 0" };


        private static readonly string[] shoetypetext = new string[] { "Inside", "Outside", "Both" };
        private static readonly string[] TabText = new string[] { "Assign", "Settings" };

        private void OnGUI()
        {
            if (!(ShowToggleInterface || ShowCustomGui) || !MakerAPI.IsInterfaceVisible())
            {
                return;
            }

            if (labelstyle == null)
            {
                labelstyle = new GUIStyle(GUI.skin.label);
                buttonstyle = new GUIStyle(GUI.skin.button);
                fieldstyle = new GUIStyle(GUI.skin.textField);
                togglestyle = new GUIStyle(GUI.skin.toggle);
                sliderstyle = new GUIStyle(GUI.skin.horizontalSlider);
                sliderthumbstyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
                buttonstyle.hover.textColor = Color.red;
                buttonstyle.onNormal.textColor = Color.red;

                SetFontSize(Screen.height / 108);
            }

            if (ShowToggleInterface)
                DrawToggleButtons();
            if (ShowCustomGui && AccessoriesApi.AccessoryCanvasVisible)
                DrawCustomGUI();
        }

        private static void GUI_Toggle()
        {
            ShowCustomGui = !ShowCustomGui;
        }

        private static void SetupInterface()
        {
            float Height = 50f + Screen.height / 2;
            float Margin = 5f;
            float Width = 130f;

            var distanceFromRightEdge = Screen.width / 10f;
            var x = Screen.width - distanceFromRightEdge - Width - Margin;
            var windowRect = new Rect(x, Margin, Width, Height);

            _Custom_GroupsRect = windowRect;
            _Custom_GroupsRect.x += 7;
            _Custom_GroupsRect.width -= 7;
            _Custom_GroupsRect.y += Height + Margin;
            _Custom_GroupsRect.height = 300f;
        }

        private void Update_Toggles_GUI()
        {
            foreach (var Custom in Names)
            {
                if (!GUI_Custom_Dict.TryGetValue(Custom.Key, out var States))
                {
                    States = new int[] { 0, 2 };
                    GUI_Custom_Dict[Custom.Key] = States;
                }
                States[1] = MaxState(Custom.Key) + 2;
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
            if (!Slotinfo.TryGetValue(slot, out var slotdata))
            {
                slotdata = new Slotdata(-1, new List<int[]>() { new int[] { 0, 3 } }, 2, false);
                Slotinfo[slot] = slotdata;
            }

            var Binding = slotdata.Binding;
            var stateinfo = slotdata.States;

            bool valid = AccessoriesApi.GetPartsInfo(slot).type != 120;

            int statelimits = StateLimitsCheck(Binding, MaxState(stateinfo));
            bool binded = -1 < Binding;
            GUILayout.BeginVertical();
            {
                Topoptions();

                GUILayout.BeginHorizontal();
                {
                    //Groups
                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("New Custom", buttonstyle))
                            {
                                int max = 10;
                                while (Names.ContainsKey(max))
                                {
                                    max++;
                                }
                                string name = "Custom " + max;
                                Names[max] = new NameData() { Name = name };
                                AddGroup(max, name);
                                Update_Drop_boxes();
                            }

                            if (GUILayout.Button("Update Dropbox", buttonstyle, GUILayout.ExpandWidth(false)))
                            {
                                Update_Drop_boxes();
                            }
                            if (binded && GUILayout.Button("Un-Bind", buttonstyle))
                            {
                                slotdata.Binding = -1;
                                ACC_Appearance_dropdown.SetValue(slot, 0);
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
                        tabvalue = GUILayout.Toolbar(tabvalue, TabText, buttonstyle);
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
            if (!Names.ContainsKey(Selectedkind))
            {
                GUILayout.Label("Custom group not selected", labelstyle);
                return;
            }
            var statenamedict = Names[Selectedkind].Statenames;

            GUILayout.BeginHorizontal();
            {
                statenameassign[0] = GUILayout.TextField(statenameassign[0], fieldstyle);
                statenameassign[1] = GUILayout.TextField(statenameassign[1], fieldstyle);
                if (GUILayout.Button("Add State Name", buttonstyle))
                {
                    if (int.TryParse(statenameassign[0], out var key))
                    {
                        statenamedict[key] = statenameassign[1];
                    }
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
                        GUILayout.Label($"State: {key}", labelstyle);
                        statenamedict[key] = GUILayout.TextField(statenamedict[key], fieldstyle);
                        if (GUILayout.Button("Delete", buttonstyle, GUILayout.ExpandWidth(false)))
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

        private void DrawStatesWindow(ref Slotdata slotdata, bool valid, int statelimits, int slot)
        {
            var binding = slotdata.Binding;
            var stateinfo = slotdata.States;
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label((binding <= Max_Defined_Key) ? Constants.ConstantOutfitNames[binding] : Names[binding].Name, labelstyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                CustomLimit(binding);

                if (valid)
                {
                    slotdata.Parented = GUILayout.Toggle(slotdata.Parented, "Parent Bind", togglestyle, GUILayout.ExpandWidth(false));
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    {
                        GUILayout.Label("Shoe", labelstyle);
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
                if (GUILayout.Button("Add new state", buttonstyle))
                {
                    stateinfo.Add(new int[] { 0, (binding > Max_Defined_Key) ? statelimits - 1 : statelimits });
                }
                if (stateinfo.Count > 1 && GUILayout.Button("Remove last state", buttonstyle))
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
                GUILayout.Label("Unassigned", labelstyle, GUILayout.ExpandWidth(true));
            }
        }

        private void DrawShoeButton(int slot, ref Slotdata slotdata, byte shoetype)
        {
            if (shoetype != slotdata.Shoetype)
            {
                bool show = true;
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
                if (show && GUILayout.Button(shoetypetext[shoetype], buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    slotdata.Shoetype = shoetype;
                    if (shoetype < 2 && ChaControl.fileStatus.shoesType != shoetype)
                    {
                        show = false;
                    }
                    ChaControl.SetAccessoryState(slot, show);
                }
                return;
            }
            GUILayout.Label(shoetypetext[shoetype], labelstyle);
        }

        private static void CustomLimit(int binding)
        {
            if (binding > Max_Defined_Key)
            {
                GUILayout.Label("Limit: " + minilimit, labelstyle);
                if (GUILayout.Button("-1", buttonstyle))
                {
                    minilimit = Math.Max(1, --minilimit);
                }
                if (GUILayout.Button("+1", buttonstyle))
                {
                    minilimit++;
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
            }
        }

        private void DefinedGroups(ref Slotdata slotdata, int limit, int slot, bool valid)
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
                    bool comparison = notcloth;
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
                        GUILayout.Label(item.Value, labelstyle);
                        if (valid)
                            DrawApplyBindingButton(ref slotdata, currentbinding, clothnum, clothnum + 1, slot);

                        if (item.Key == currentbinding)
                        {
                            DrawCopyPasteButtons(ref slotdata, currentbinding, slot);
                            DrawResetButton(ref slotdata, limit, slot);
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

        private void CustomGroups(ref Slotdata slotinfo, int limit, int slot, bool valid)
        {
            var currentbinding = slotinfo.Binding;
            var namedict = Names;
            var count = Constants.ConstantOutfitNames.Count;

            for (int i = 0; i < namedict.Count; i++)
            {
                var element = namedict.ElementAt(i);

                GUILayout.BeginVertical();
                {
                    if (element.Key == currentbinding)
                        GUILayout.BeginHorizontal(GUI.skin.box);
                    else
                        GUILayout.BeginHorizontal();
                    {
                        var text = GUILayout.TextField(element.Value.Name, fieldstyle, GUILayout.ExpandWidth(false));
                        if (text != element.Value.Name)
                        {
                            namedict[element.Key].Name = text;
                            RenameGroup(element.Key, text);
                        }

                        if (valid)
                            DrawApplyBindingButton(ref slotinfo, currentbinding, element.Key, i + count, slot);

                        if (element.Key == currentbinding)
                        {
                            DrawCopyPasteButtons(ref slotinfo, currentbinding, slot);
                            DrawResetButton(ref slotinfo, (autoscale) ? limit : limit - 1, slot);
                        }

                        if (tabvalue == 1 && Selectedkind != element.Key && GUILayout.Button("Select", buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            Selectedkind = element.Key;
                        }

                        if (showdelete && GUILayout.Button("Delete", buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            namedict.Remove(element.Key);
                            DeleteGroup(element.Key);
                            var removelist = Slotinfo.Where(x => x.Value.Binding == element.Key).ToList();
                            for (int j = 0; j < removelist.Count; j++)
                            {
                                ACC_Appearance_dropdown.SetValue(removelist.Count, -1);
                                Slotinfo.Remove(removelist[i].Key);
                            }
                            GUI_int_state_copy_Dict.Remove(element.Key);
                            GUI_Custom_Dict.Remove(element.Key);

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
                GUILayout.Label($"Start: {StateDescription(binding, stateref[0])}", labelstyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Stop: {StateDescription(binding, stateref[1])}", labelstyle);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                int round = Mathf.RoundToInt(GUILayout.HorizontalSlider(stateref[0], 0, limit, sliderstyle, sliderthumbstyle, GUILayout.ExpandWidth(true)));
                if (round != stateref[0] && round <= stateref[1])
                {
                    stateref[0] = round;
                    if (index == 0)
                        ACC_Appearance_state.SetValue(slot, round, true);
                    else
                        ChangeTriggerProperty(binding);
                }
                var text = GUILayout.TextField(round.ToString(), fieldstyle, GUILayout.ExpandWidth(false));
                if (int.TryParse(text, out int textvalue) && textvalue != round && textvalue <= stateref[1])
                {
                    stateref[0] = textvalue;
                    if (index == 0)
                        ACC_Appearance_state.SetValue(slot, textvalue);
                    else
                        ChangeTriggerProperty(binding);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                int round = Mathf.RoundToInt(GUILayout.HorizontalSlider(stateref[1], 0, limit, sliderstyle, sliderthumbstyle, GUILayout.ExpandWidth(true)));
                if (round != stateref[1] && round >= stateref[0])
                {
                    stateref[1] = round;
                    if (index == 0)
                        ACC_Appearance_state2.SetValue(slot, round, true);
                    else
                    {
                        UpdateAccessoryshow(Slotinfo[slot], slot);
                        ChangeTriggerProperty(binding);
                        Update_Toggles_GUI();
                    }
                }
                var text = GUILayout.TextField(round.ToString(), fieldstyle, GUILayout.ExpandWidth(false));
                if (int.TryParse(text, out int textvalue) && textvalue != round && textvalue >= stateref[0] && (autoscale || textvalue <= limit))
                {
                    stateref[1] = textvalue;
                    if (index == 0)
                        ACC_Appearance_state2.SetValue(slot, textvalue);
                    else
                    {
                        UpdateAccessoryshow(Slotinfo[slot], slot);
                        ChangeTriggerProperty(binding);
                        Update_Toggles_GUI();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawApplyBindingButton(ref Slotdata slotinfo, int currentbinding, int newbinding, int index, int slot)
        {
            if (newbinding != currentbinding && GUILayout.Button("Apply", buttonstyle, GUILayout.ExpandWidth(false)))
            {
                var shoetype = slotinfo.Shoetype;
                if (newbinding == 7 && shoetype == 1) shoetype = 0;
                if (newbinding == 8 && shoetype == 0) shoetype = 1;
                slotinfo.Shoetype = shoetype;
                ACC_Appearance_dropdown.SetValue(slot, index, true);
                var max = StateLimitsCheck(newbinding);
                var states = slotinfo.States;
                states.Clear();
                states.Add(new int[] { 0, max });
                ACC_Appearance_state.SetValue(slot, 0, true);
                ACC_Appearance_state2.SetValue(slot, max, true);
                Update_Toggles_GUI();
            }
        }

        private static void DrawCopyPasteButtons(ref Slotdata currentslotdata, int currentbinding, int slot)
        {
            if (GUILayout.Button("Copy", buttonstyle, GUILayout.ExpandWidth(false)))
            {
                GUI_int_state_copy_Dict[currentbinding] = new Slotdata(currentslotdata);
            }
            if (GUI_int_state_copy_Dict.TryGetValue(currentbinding, out var list) && GUILayout.Button("Paste", buttonstyle, GUILayout.ExpandWidth(false)))
            {
                currentslotdata.Shoetype = list.Shoetype;
                currentslotdata.Parented = list.Parented;
                currentslotdata.States = list.States;
                var firststate = list.States[0];
                ACC_Appearance_state.SetValue(slot, firststate[0]);
                ACC_Appearance_state2.SetValue(slot, firststate[1]);
            }
        }

        private void Topoptions()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                DrawFontSize();
                if (Input.GetMouseButtonDown(0) && !mouseassigned && screenRect.Contains(Input.mousePosition))
                {
                    StartCoroutine(DragEvent());
                }
                if (GUILayout.Button("X", buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    ShowCustomGui = false;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                showdelete = GUILayout.Toggle(showdelete, "Enable Delete", togglestyle, GUILayout.ExpandWidth(false));
                autoscale = GUILayout.Toggle(autoscale, "Auto increase custom Limit", togglestyle, GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Refresh", buttonstyle))
                {
                    Refresh();
                }
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawResetButton(ref Slotdata slotinfo, int limit, int slot)
        {
            if (GUILayout.Button("Reset", buttonstyle, GUILayout.ExpandWidth(false)))
            {
                var states = slotinfo.States;
                states.Clear();
                states.Add(new int[] { 0, limit });
                ACC_Appearance_state.SetValue(slot, 0, true);
                ACC_Appearance_state2.SetValue(slot, limit, true);
            }
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
            if (!Slotinfo.TryGetValue(AccessoriesApi.SelectedMakerAccSlot, out var stateinfo))
            {
                return 3;
            }
            return StateLimitsCheck(Binding, MaxState(stateinfo.States));
        }

        private void DrawToggleButtons()
        {
            GUILayout.BeginArea(_Custom_GroupsRect);
            {
                GUILayout.BeginScrollView(_accessorySlotsScrollPos);
                {
                    GUILayout.BeginVertical();
                    {
                        //Custom Groups
                        foreach (var item in Names)
                        {
                            DrawCustomButton(item.Key, item.Value.Name);
                        }
                        //Parent
                        foreach (var item in ThisCharactersData.Now_Parented_Name_Dictionary.Keys)
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

        private void DrawCustomButton(int Kind, string Name)
        {
            if (!GUI_Custom_Dict.TryGetValue(Kind, out int[] State))
            {
                State = new int[] { 0, 2 };
                GUI_Custom_Dict[Kind] = State;
            }

            if (GUILayout.Button($"{Name}: {State[0]}"))
            {
                State[0] = (State[0] + 1) % State[1];
                Custom_Groups(Kind, State[0]);
            }
            GUILayout.Space(-5);
        }

        private void DrawParentButton(string Parent)
        {
            if (!GUI_Parent_Dict.TryGetValue(Parent, out bool isOn))
            {
                isOn = true;
            }
            if (GUILayout.Button($"{Parent}: {(isOn ? "On" : "Off")}"))
            {
                isOn = !isOn;
                Parent_toggle(Parent, isOn);
            }
            GUILayout.Space(-5);
            GUI_Parent_Dict[Parent] = isOn;
        }

        private string StateDescription(int binding, int state)
        {
            string statename = state.ToString();
            if (binding > Max_Defined_Key)
            {
                if (Names[binding].Statenames.TryGetValue(state, out statename))
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

        private static void DrawFontSize()
        {
            if (GUILayout.Button("GUI-", buttonstyle))
            {
                SetFontSize(Math.Max(labelstyle.fontSize - 1, 5));
            }
            if (GUILayout.Button("GUI+", buttonstyle))
            {
                SetFontSize(1 + labelstyle.fontSize);
            }
        }

        private static void SetFontSize(int size)
        {
            labelstyle.fontSize = size;
            buttonstyle.fontSize = size;
            fieldstyle.fontSize = size;
            togglestyle.fontSize = size;
            sliderstyle.fontSize = size;
            sliderthumbstyle.fontSize = size;
        }

        private IEnumerator<int> DragEvent()
        {
            var pos = Input.mousePosition;
            Vector2 mousepos = pos;
            mouseassigned = true;
            bool mousebuttonup = false;
            for (int i = 0; i < 20; i++)
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
    }
}
