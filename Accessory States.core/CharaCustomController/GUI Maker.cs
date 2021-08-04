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

        static private readonly Dictionary<int, List<int[]>> GUI_int_state_copy_Dict = new Dictionary<int, List<int[]>>();

        static private readonly Dictionary<string, bool> GUI_Parent_Dict = new Dictionary<string, bool>();

        static private bool ShowToggleInterface = false;

        static private bool ShowCustomGui = false;

        static Rect _Custom_GroupsRect;

        static private Vector2 _accessorySlotsScrollPos = new Vector2();

        static public Dictionary<int, int> Gui_states = new Dictionary<int, int>();

        static private Vector2 NameScrolling = new Vector2();
        static private Vector2 StateScrolling = new Vector2();
        static private Vector3 mousepos = new Vector3();
        static private bool mouseassigned = false;
        static private bool moveassigned = false;
        static private bool autoscale = false;
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
        static string[] statenameassign = new string[] { "0", "state 0" };


        private static readonly string[] shoetypetext = new string[] { "Inner", "Outside", "Both" };
        private static readonly string[] TabText = new string[] { "Assign", "State Names" };

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

        private void Custom_groups_GUI_Toggle(int kind)
        {
            if (!Gui_states.TryGetValue(kind, out int state))
            {
                state = 0;
            }
            var slotinfo = Slotinfo.Values.Where(x => x.Binding == kind);

            int final = 0;
            foreach (var item in slotinfo)
            {
                foreach (var item2 in item.States)
                {
                    final = Math.Max(final, item2[1]);
                }
            }
            Gui_states[kind] = state = (state + 1) % (final + 2);

            Custom_Groups(kind, state);
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

        private void Update_Custom_GUI()
        {
            foreach (var Custom in Names)
            {
                if (!GUI_Custom_Dict.TryGetValue(Custom.Key, out var States))
                {
                    States = new int[] { 0, 2 };
                    GUI_Custom_Dict[Custom.Key] = States;
                }
                int max = 0;

                var list = Slotinfo.Values.Where(x => x.Binding == Custom.Key);
                foreach (var item in list)
                {
                    foreach (var item2 in item.States)
                    {
                        max = Math.Max(max, item2[1]);
                    }
                }
                States[1] = max + 2;
            }
        }

        private void DrawCustomGUI()
        {
            IMGUIUtils.DrawSolidBox(screenRect);
            GUILayout.Window(2900, screenRect, CustomGui, $"Accessory States Gui: Slot {AccessoriesApi.SelectedMakerAccSlot + 1}");
        }

        private void CustomGui(int id)
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            if (!Slotinfo.TryGetValue(slot, out var slotdata))
            {
                slotdata = new Slotdata(-1, new List<int[]>() { new int[] { 0, 3 } }, 2);
                Slotinfo[slot] = slotdata;
            }

            var Binding = slotdata.Binding;
            var stateinfo = slotdata.States;

            if (!Parented.TryGetValue(slot, out var isparented))
            {
                isparented = false;
                Parented[slot] = isparented;
            }

#pragma warning disable CS0612 // Type or member is obsolete
            bool valid = AccessoriesApi.GetPartsInfo(slot).type != 120;
#pragma warning restore CS0612 // Type or member is obsolete

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
                                ACC_Appearance_dropdown.SetValue(slot, -1);
                            }
                            if (!binded)
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
                                DrawStatesWindow(ref slotdata, isparented, valid, statelimits, slot);
                                break;
                            case 1:
                                DrawStatesNameWindow();
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

        private void DrawStatesNameWindow()
        {
            if (!Names.ContainsKey(Selectedkind))
            {
                return;
            }
            var test = Names[Selectedkind].Statenames;

            GUILayout.BeginHorizontal();
            {
                statenameassign[0] = GUILayout.TextField(statenameassign[0], fieldstyle);
                statenameassign[1] = GUILayout.TextField(statenameassign[1], fieldstyle);
                if (GUILayout.Button("Add State Name", buttonstyle))
                {
                    if (int.TryParse(statenameassign[0], out var key))
                    {
                        test[key] = statenameassign[1];
                    }
                }
            }
            GUILayout.EndHorizontal();


            StateScrolling = GUILayout.BeginScrollView(StateScrolling);
            {
                for (int i = 0, n = test.Count; i < n; i++)
                {
                    var key = test.ElementAt(i).Key;
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label($"State: {key}", labelstyle);
                        test[key] = GUILayout.TextField(test[key], fieldstyle);
                        if (GUILayout.Button("Delete", buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            test.Remove(key);
                            i--;
                            n--;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawStatesWindow(ref Slotdata slotdata, bool isparented, bool valid, int statelimits, int slot)
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
                    if (GUILayout.Toggle(isparented, "Parent Bind", togglestyle, GUILayout.ExpandWidth(false)) != isparented)
                    {
                        isparented = !isparented;
                        ACC_Is_Parented.SetSelectedValue(isparented, true);
                    }
                    GUILayout.Label("Shoe", labelstyle);
                    for (byte i = 0; i < 3; i++)
                    {
                        DrawShoeButton(ref slotdata, i);
                    }
                }
                else
                    GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();

            }
            GUILayout.EndHorizontal();
            if (binding > 0)
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

        private void DrawShoeButton(ref Slotdata slotdata, byte shoetype)
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
            var states = slotdata.States;
            var cloth = ChaControl.nowCoordinate.clothes.parts;
            foreach (var item in Constants.ConstantOutfitNames)
            {
                switch (item.Key)
                {
                    case -1:
                        continue;
                    case 1:
                        if (ChaControl.notBot || cloth[1].id == 0)
                            continue;
                        break;
                    case 2:
                        if (ChaControl.notBra || cloth[2].id == 0)
                            continue;
                        break;
                    case 3:
                        if (ChaControl.notShorts || cloth[3].id == 0)
                            continue;
                        break;
                    default:
                        if (cloth[item.Key].id == 0)
                            continue;
                        break;
                }

                GUILayout.BeginVertical();
                {
                    if (item.Key == currentbinding)
                        GUILayout.BeginHorizontal(GUI.skin.box);
                    else
                        GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(item.Value + $" {item.Key}", labelstyle);
                        if (valid)
                            DrawApplyBindingButton(ref states, currentbinding, item.Key, item.Key + 1, slot);

                        if (item.Key != currentbinding)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                            continue;
                        }
                        if (item.Key == currentbinding)
                        {
                            DrawCopyPasteButtons(ref states, currentbinding, slot);
                            DrawResetButton(ref states, limit, slot);
                        }
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
            var reserved = namedict.Values;
            var count = Constants.ConstantOutfitNames.Count;
            var states = slotinfo.States;

            for (int i = 0; i < reserved.Count; i++)
            {
                var element = namedict.ElementAt(i);

                GUILayout.BeginVertical();
                {
                    if (element.Key == currentbinding)
                        GUILayout.BeginHorizontal(GUI.skin.box);
                    else
                        GUILayout.BeginHorizontal();
                    {
                        var text = GUILayout.TextField(element.Value.Name, fieldstyle);
                        if (text != element.Value.Name)
                        {
                            namedict[element.Key].Name = text;
                            RenameGroup(element.Key, text);
                        }

                        if (valid)
                            DrawApplyBindingButton(ref states, currentbinding, element.Key, i + count, slot);

                        if (element.Key == currentbinding)
                        {
                            DrawCopyPasteButtons(ref states, currentbinding, slot);
                            DrawResetButton(ref states, (autoscale) ? limit : limit - 1, slot);
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

        private void DrawStartStopSlider(ref List<int[]> states, int count, int limit, int binding, int slot)
        {
            var stateref = states[count];
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
                    if (count == 0)
                        ACC_Appearance_state.SetValue(slot, round, true);
                    else
                        ChangeTriggerProperty(binding);
                }
                var text = GUILayout.TextField(round.ToString(), fieldstyle, GUILayout.ExpandWidth(false));
                if (int.TryParse(text, out int textvalue) && textvalue != round && textvalue <= stateref[1])
                {
                    stateref[0] = textvalue;
                    if (count == 0)
                        ACC_Appearance_state.SetValue(slot, textvalue, true);
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
                    if (count == 0)
                        ACC_Appearance_state2.SetValue(slot, round, true);
                    else
                        ChangeTriggerProperty(binding);
                }
                var text = GUILayout.TextField(round.ToString(), fieldstyle, GUILayout.ExpandWidth(false));
                if (int.TryParse(text, out int textvalue) && textvalue != round && textvalue >= stateref[0] && (autoscale || textvalue <= limit))
                {
                    stateref[1] = textvalue;
                    if (count == 0)
                        ACC_Appearance_state2.SetValue(slot, textvalue, true);
                    else
                        ChangeTriggerProperty(binding);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawApplyBindingButton(ref List<int[]> states, int currentbinding, int newbinding, int index, int slot)
        {
            if (newbinding != currentbinding && GUILayout.Button("Apply", buttonstyle, GUILayout.ExpandWidth(false)))
            {
                ACC_Appearance_dropdown.SetValue(slot, index, true);
                var max = StateLimitsCheck(newbinding);
                states.Clear();
                states.Add(new int[] { 0, max });
                ACC_Appearance_state.SetValue(slot, 0, true);
                ACC_Appearance_state2.SetValue(slot, max, true);
            }
        }

        private static void DrawCopyPasteButtons(ref List<int[]> states, int currentbinding, int slot)
        {
            if (GUILayout.Button("Copy", buttonstyle, GUILayout.ExpandWidth(false)))
            {
                GUI_int_state_copy_Dict[currentbinding] = new List<int[]>(states);
            }
            if (GUI_int_state_copy_Dict.TryGetValue(currentbinding, out var list) && GUILayout.Button("Paste", buttonstyle, GUILayout.ExpandWidth(false)))
            {
                states.Clear();
                foreach (var item in list)
                {
                    states.Add(new int[] { item[0], item[1] });
                }
                ACC_Appearance_state.SetValue(slot, list[0][0], true);
                ACC_Appearance_state2.SetValue(slot, list[0][1], true);
            }
        }

        private static void Topoptions()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                DrawFontSize();
                moveassigned = GUILayout.Toggle(moveassigned, "Move", togglestyle, GUILayout.ExpandWidth(false));
                if (moveassigned)
                //if (GUILayout.Button("Move") || mouseassigned && !Input.GetMouseButtonUp(0))
                {
                    var pos = Input.mousePosition;
                    if (!mouseassigned)
                    {
                        mousepos = new Vector3(pos.x, pos.y, pos.z);
                        mouseassigned = true;
                    }
                    var delta = pos - mousepos;
                    screenRect.x += delta.x;
                    screenRect.y -= delta.y;

                    mousepos = new Vector3(pos.x, pos.y, pos.z);
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        moveassigned = false;
                    }
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
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawResetButton(ref List<int[]> states, int limit, int slot)
        {
            if (GUILayout.Button("Reset", buttonstyle, GUILayout.ExpandWidth(false)))
            {
                states.Clear();
                states.Add(new int[] { 0, limit });
                ACC_Appearance_state.SetValue(slot, 0, true);
                ACC_Appearance_state2.SetValue(slot, limit, true);
            }
        }

        private static int StateLimitsCheck(int Binding, int currentmax)
        {
            if (Binding < 5)
            {
                return 3;
            }
            if (Binding <= Max_Defined_Key)
            {
                return 1;
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

        private static void DrawParentButton(string Parent)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            if (!GUI_Parent_Dict.TryGetValue(Parent, out bool isOn))
            {
                isOn = true;
            }
            if (GUILayout.Button($"{Parent}: {(isOn ? "On" : "Off")}"))
            {
                isOn = !isOn;
                Controller.Parent_toggle(Parent, isOn);
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
                return state.ToString();
            }
            if (binding > 3)
            {
                switch (state)
                {
                    case 0:
                        statename += " Dressed"; break;
                    case 1:
                        statename += " Undressed"; break;
                    default:
                        statename += " Error"; break;
                }
                return statename;
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
    }
}
