using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ExtensibleSaveFormat.Extensions;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static private bool ShowCustomGui = false;

        static private Vector2 _accessorySlotsScrollPos = new Vector2();

        static public Dictionary<int, int> Gui_states = new Dictionary<int, int>();

        static private Vector2 NameScrolling = new Vector2();
        static Rect screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f), (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        static bool showdelete = false;
        static bool showreplace = false;
        static bool mouseassigned = false;

        static readonly int[] selectedadjustment = new int[3] { 2, 1, 0 };

        static readonly string[] moveadjustment = new string[] { "0.01", "0.1", "1.0", "10.0", "custom" };
        static readonly string[] rotationadjustment = new string[] { "0.1", "1.0", "5.0", "10.0", "custom" };
        static readonly string[] scaleadjustment = new string[] { "0.01", "0.1", "1.0", "custom" };
        static readonly string[] vectornames = new string[] { "x", "y", "z" };
        static readonly string[] customvalues = new string[3] { "5", "2.5", "0.5" };

        static GUIStyle labelstyle;
        static GUIStyle buttonstyle;
        static GUIStyle fieldstyle;
        static GUIStyle togglestyle;

        static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        internal void OnGUI()
        {
            if (!MakerAPI.IsInterfaceVisible() || !AccessoriesApi.AccessoryCanvasVisible)
            {
                return;
            }

            if (labelstyle == null)
            {
                labelstyle = new GUIStyle(GUI.skin.label);
                buttonstyle = new GUIStyle(GUI.skin.button);
                fieldstyle = new GUIStyle(GUI.skin.textField);
                togglestyle = new GUIStyle(GUI.skin.toggle);
                buttonstyle.hover.textColor = Color.red;
                buttonstyle.onNormal.textColor = Color.red;
                SetFontSize(Screen.height / 108);
            }

            if (ShowCustomGui)
                DrawCustomGUI();
        }

        private static void GUI_Toggle()
        {
            ShowCustomGui = !ShowCustomGui;
        }

        private void DrawCustomGUI()
        {
            IMGUIUtils.DrawSolidBox(screenRect);
            screenRect = GUILayout.Window(2901, screenRect, CustomGui, $"Accessory Parents Gui: Slot {AccessoriesApi.SelectedMakerAccSlot + 1}");
        }

        private void CustomGui(int id)
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            var partinfo = Parts[slot];
            var valid = partinfo.type != 120;

            GUILayout.BeginVertical();
            {
                Topoptions();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("New Parent", buttonstyle))
                    {
                        AddTheme();
                    }
                    if (GUILayout.Button("Update Dropbox", buttonstyle, GUILayout.ExpandWidth(false)))
                    {
                        Update_Drop_boxes();
                    }
                    if (valid && GUILayout.Button("Save Position", buttonstyle, GUILayout.ExpandWidth(false)))
                    {
                        Save_Relative_Data(slot);
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        NameScrolling = GUILayout.BeginScrollView(NameScrolling, GUILayout.ExpandWidth(true));
                        {
                            DrawThemeNames(slot, valid);
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        _accessorySlotsScrollPos = GUILayout.BeginScrollView(_accessorySlotsScrollPos, GUILayout.ExpandWidth(true));
                        {
                            if (valid)
                            {
                                MoveAdjustment(slot, partinfo.addMove[0, 0]);
                                RotAdjustment(slot, partinfo.addMove[0, 1]);
                                ScaleAdjustment(slot, partinfo.addMove[0, 2]);
                            }
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            screenRect = IMGUIUtils.DragResizeEatWindow(800, screenRect);
            GUILayout.EndVertical();
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
                RecursiveStop = GUILayout.Toggle(RecursiveStop, "Recursive Stop", togglestyle, GUILayout.ExpandWidth(false));
                Retrospect = GUILayout.Toggle(Retrospect, "Retrospect", togglestyle, GUILayout.ExpandWidth(false));
                showreplace = GUILayout.Toggle(showreplace, "Show Replace", togglestyle, GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawThemeNames(int slot, bool valid)
        {
            if (!RelatedNames.TryGetValue(slot, out var relation))
            {
                relation = new List<int>();
            }
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            for (int i = 0, n = Parent_Groups.Count; i < n; i++)
            {
                var item = Parent_Groups[i];
                var parentof = item.ParentSlot == slot;
                var childof = item.ChildSlots.Contains(slot);
                var relatedto = relation.Contains(item.ParentSlot);
                if (parentof || childof)
                    GUILayout.BeginHorizontal(GUI.skin.box);
                else
                    GUILayout.BeginHorizontal();
                {
                    var tempname = GUILayout.TextField(item.Name, fieldstyle);
                    if (tempname != item.Name)
                    {
                        item.Name = tempname;
                        SetPartData(item.ParentSlot, item.Serialize());
                    }
                    if (!parentof)
                    {
                        if (valid && (showreplace || item.ParentSlot == -1) && (!relatedto || childof) && GUILayout.Button("Make Parent", buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            if (childof)
                            {
                                item.ChildSlots.Remove(slot);
                            }
                            SetPartData(item.ParentSlot, null);
                            item.ParentSlot = slot;
                            SetPartData(item.ParentSlot, item.Serialize());
                            UpdateRelations();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Remove Parent", buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            SetPartData(item.ParentSlot, null);
                            item.ParentSlot = -1;
                        }
                    }
                    if (!childof)
                    {
                        if (valid && !parentof && !relatedto && GUILayout.Button("Make Child", buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            item.ChildSlots.Add(slot);
                            MakeChild(slot, item.ParentSlot);
                            SetPartData(item.ParentSlot, item.Serialize());
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Unbind Child", buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            item.ChildSlots.Remove(slot);
                            SetPartData(item.ParentSlot, item.Serialize());
                        }
                    }
                    if (showdelete && GUILayout.Button("Delete", buttonstyle))
                    {
                        SetPartData(item.ParentSlot, null);
                        Parent_Groups.RemoveAt(i);
                        n--;
                        i--;
                        if (n >= Parent_DropDown.Value)
                        {
                            Parent_DropDown.SetValue(n - 1);
                        }
                        Update_Drop_boxes();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void MoveAdjustment(int slot, Vector3 movement)
        {
            GUILayout.Label("Movement", labelstyle);
            GUILayout.BeginHorizontal();
            {
                selectedadjustment[0] = GUILayout.Toolbar(selectedadjustment[0], moveadjustment, buttonstyle, GUILayout.ExpandWidth(false));
                customvalues[0] = GUILayout.TextField(customvalues[0], fieldstyle);
            }
            GUILayout.EndHorizontal();

            var adjustment = 0f;
            switch (selectedadjustment[0])
            {
                case 0:
                    adjustment = 0.01f;
                    break;
                case 1:
                    adjustment = 0.1f;
                    break;
                case 2:
                    adjustment = 1f;
                    break;
                case 3:
                    adjustment = 10f;
                    break;
                case 4:
                    float.TryParse(customvalues[0], out adjustment);
                    break;
                default:
                    break;
            }

            for (var vectortype = 0; vectortype < 3; vectortype++)
            {
                AdjustmentTool(slot, 0, vectortype, vectornames[vectortype], movement[vectortype], adjustment);
            }
        }

        private void RotAdjustment(int slot, Vector3 rotation)
        {
            GUILayout.Label("Rotation", labelstyle);
            GUILayout.BeginHorizontal();
            {
                selectedadjustment[1] = GUILayout.Toolbar(selectedadjustment[1], rotationadjustment, buttonstyle, GUILayout.ExpandWidth(false));
                customvalues[1] = GUILayout.TextField(customvalues[1], fieldstyle);
            }
            GUILayout.EndHorizontal();
            var adjustment = 0f;
            switch (selectedadjustment[1])
            {
                case 0:
                    adjustment = 0.1f;
                    break;
                case 1:
                    adjustment = 1f;
                    break;
                case 2:
                    adjustment = 5f;
                    break;
                case 3:
                    adjustment = 10f;
                    break;
                case 4:
                    float.TryParse(customvalues[1], out adjustment);
                    break;
            }
            for (var vectortype = 0; vectortype < 3; vectortype++)
            {
                AdjustmentTool(slot, 1, vectortype, vectornames[vectortype], rotation[vectortype], adjustment);
            }
        }

        private void ScaleAdjustment(int slot, Vector3 scale)
        {
            GUILayout.Label("Scale", labelstyle);
            GUILayout.BeginHorizontal();
            {
                selectedadjustment[2] = GUILayout.Toolbar(selectedadjustment[2], scaleadjustment, buttonstyle, GUILayout.ExpandWidth(false));
                customvalues[2] = GUILayout.TextField(customvalues[2], fieldstyle);
            }
            GUILayout.EndHorizontal();

            var adjustment = 0f;

            switch (selectedadjustment[2])
            {
                case 0:
                    adjustment = 0.01f;
                    break;
                case 1:
                    adjustment = 0.1f;
                    break;
                case 2:
                    adjustment = 1f;
                    break;
                case 3:
                    float.TryParse(customvalues[2], out adjustment);
                    break;
            }

            for (var vectortype = 0; vectortype < 3; vectortype++)
            {
                AdjustmentTool(slot, 2, vectortype, vectornames[vectortype], scale[vectortype], adjustment);
            }
        }

        private void AdjustmentTool(int slot, int typeindex, int vectortype, string label, float value, float adjustment)
        {
            var resultingvalue = 0f;
            var reset = false;
            var fullreset = false;
            var assigned = false;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(label, labelstyle);

                if (GUILayout.Button("◀", buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    resultingvalue = -adjustment;
                    assigned = true;
                }
                if (GUILayout.Button("▶", buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    resultingvalue = adjustment;
                    assigned = true;
                }
                var original = value.ToString();
                var result = GUILayout.TextField(original, fieldstyle);
                if (original != result && float.TryParse(result, out var resultvalue))
                {
                    resultingvalue = resultvalue - value;
                    assigned = true;
                }
                if (GUILayout.Button("Reset", buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    reset = true;
                    assigned = true;
                }
                if (GUILayout.Button("Recur Reset", buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    fullreset = true;
                    assigned = true;
                }
                if (assigned)
                {
                    switch (typeindex)
                    {
                        case 0:
                            ChangePosition(slot, resultingvalue, vectortype, reset, fullreset);
                            break;
                        case 1:
                            ChangeRotation(slot, resultingvalue, vectortype, reset, fullreset);
                            break;
                        case 2:
                            ChangeScale(slot, resultingvalue, vectortype, reset, fullreset);
                            break;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void AddTheme(bool auto = false)
        {
            var count = Parent_Groups.Count;
            var name = new Custom_Name("Parent Group " + count);
            if (auto)
            {
                name.ParentSlot = AccessoriesApi.SelectedMakerAccSlot;
            }
            Parent_Groups.Add(name);
            Update_Drop_boxes();
            UpdateRelations();
        }

        private void UpdateRelations()
        {
            RelatedNames.Clear();
            Child.Clear();

            var n = Parent_Groups.Count;
            for (var i = 0; i < n; i++)
            {
                var item = Parent_Groups[i];
                if (!RelatedNames.TryGetValue(item.ParentSlot, out var itembindings))
                {
                    itembindings = new List<int>();
                    RelatedNames[item.ParentSlot] = itembindings;
                }
                TryChildListBySlot(item.ParentSlot, out var allchild, true);
                allchild = allchild.Distinct().ToList();

                foreach (var child in allchild)
                {
                    if (!RelatedNames.TryGetValue(child, out var childbindings))
                    {
                        childbindings = new List<int>();
                        RelatedNames[child] = childbindings;
                    }
                    itembindings.Add(child);
                    childbindings.Add(item.ParentSlot);
                    Child[child] = item.ParentSlot;
                }
            }
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

        private void SetPartData(int slot, ExtensibleSaveFormat.PluginData plugin)
        {
            if (slot > 0 && slot < Parts.Length)
            {
                Parts[slot].SetExtendedDataById(Settings.GUID, plugin);
            }
        }
    }
}
