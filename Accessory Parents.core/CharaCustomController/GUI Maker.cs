using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Utilities;
using UnityEngine;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private static bool _showCustomGui;

        private static Vector2 _accessorySlotsScrollPos;

        public static Dictionary<int, int> GuiStates = new Dictionary<int, int>();

        private static Vector2 _nameScrolling;

        private static Rect _screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f),
            (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        private static bool _showDelete;
        private static bool _showreplace;
        private static bool _mouseassigned;

        private static readonly int[] _selectedadjustment = { 2, 1, 0 };

        private static readonly string[] Moveadjustment = { "0.01", "0.1", "1.0", "10.0", "custom" };
        private static readonly string[] Rotationadjustment = { "0.1", "1.0", "5.0", "10.0", "custom" };
        private static readonly string[] Scaleadjustment = { "0.01", "0.1", "1.0", "custom" };
        private static readonly string[] Vectornames = { "x", "y", "z" };
        private static readonly string[] Customvalues = { "5", "2.5", "0.5" };

        private static GUIStyle _labelstyle;
        private static GUIStyle _buttonstyle;
        private static GUIStyle _fieldstyle;
        private static GUIStyle _togglestyle;

        private static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        internal void OnGUI()
        {
            if (!MakerAPI.IsInterfaceVisible() || !AccessoriesApi.AccessoryCanvasVisible) return;

            if (_labelstyle == null)
            {
                _labelstyle = new GUIStyle(GUI.skin.label);
                _buttonstyle = new GUIStyle(GUI.skin.button);
                _fieldstyle = new GUIStyle(GUI.skin.textField);
                _togglestyle = new GUIStyle(GUI.skin.toggle);
                _buttonstyle.hover.textColor = Color.red;
                _buttonstyle.onNormal.textColor = Color.red;
                SetFontSize(Screen.height / 108);
            }

            if (_showCustomGui)
                DrawCustomGUI();
        }

        private static void GUI_Toggle()
        {
            _showCustomGui = !_showCustomGui;
        }

        private void DrawCustomGUI()
        {
            IMGUIUtils.DrawSolidBox(_screenRect);
            _screenRect = GUILayout.Window(2901, _screenRect, CustomGui,
                $"Accessory Parents Gui: Slot {AccessoriesApi.SelectedMakerAccSlot + 1}");
        }

        private void CustomGui(int id)
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            var partinfo = AccessoriesApi.GetPartsInfo(slot);
            var valid = partinfo.type != 120;

            GUILayout.BeginVertical();
            {
                Topoptions();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("New Parent", _buttonstyle)) AddTheme();
                    if (GUILayout.Button("Update Dropbox", _buttonstyle, GUILayout.ExpandWidth(false)))
                        Update_Drop_boxes();
                    if (valid && GUILayout.Button("Save Position", _buttonstyle, GUILayout.ExpandWidth(false)))
                        Save_Relative_Data(slot);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        _nameScrolling = GUILayout.BeginScrollView(_nameScrolling, GUILayout.ExpandWidth(true));
                        {
                            DrawThemeNames(slot, valid);
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        _accessorySlotsScrollPos =
                            GUILayout.BeginScrollView(_accessorySlotsScrollPos, GUILayout.ExpandWidth(true));
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
            _screenRect = IMGUIUtils.DragResizeEatWindow(800, _screenRect);
            GUILayout.EndVertical();
        }

        private void Topoptions()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                DrawFontSize();
                if (Input.GetMouseButtonDown(0) && !_mouseassigned && _screenRect.Contains(Input.mousePosition))
                    StartCoroutine(DragEvent());
                if (GUILayout.Button("X", _buttonstyle, GUILayout.ExpandWidth(false))) _showCustomGui = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                _showDelete = GUILayout.Toggle(_showDelete, "Enable Delete", _togglestyle,
                    GUILayout.ExpandWidth(false));
                _recursiveStop = GUILayout.Toggle(_recursiveStop, "Recursive Stop", _togglestyle,
                    GUILayout.ExpandWidth(false));
                _retrospect = GUILayout.Toggle(_retrospect, "Retrospect", _togglestyle, GUILayout.ExpandWidth(false));
                _showreplace = GUILayout.Toggle(_showreplace, "Show Replace", _togglestyle,
                    GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawThemeNames(int slot, bool valid)
        {
            if (!RelatedNames.TryGetValue(slot, out var relation)) relation = new List<int>();
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            for (int i = 0, n = ParentGroups.Count; i < n; i++)
            {
                var item = ParentGroups[i];
                var parentof = item.ParentSlot == slot;
                var childof = item.childSlots.Contains(slot);
                var relatedto = relation.Contains(item.ParentSlot);
                if (parentof || childof)
                    GUILayout.BeginHorizontal(GUI.skin.box);
                else
                    GUILayout.BeginHorizontal();
                {
                    item.Name = GUILayout.TextField(item.Name, _fieldstyle);

                    if (!parentof)
                    {
                        if (valid && (_showreplace || item.ParentSlot == -1) && (!relatedto || childof) &&
                            GUILayout.Button("Make Parent", _buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            if (childof) item.childSlots.Remove(slot);
                            item.ParentSlot = slot;
                            UpdateRelations();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Remove Parent", _buttonstyle, GUILayout.ExpandWidth(false)))
                            item.ParentSlot = -1;
                    }

                    if (!childof)
                    {
                        if (valid && !parentof && !relatedto &&
                            GUILayout.Button("Make Child", _buttonstyle, GUILayout.ExpandWidth(false)))
                        {
                            item.childSlots.Add(slot);
                            MakeChild(slot, item.ParentSlot);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Unbind Child", _buttonstyle, GUILayout.ExpandWidth(false)))
                            item.childSlots.Remove(slot);
                    }

                    if (_showDelete && GUILayout.Button("Delete", _buttonstyle))
                    {
                        ParentGroups.RemoveAt(i);
                        n--;
                        i--;
                        if (n >= _parentDropDown.Value) _parentDropDown.SetValue(n - 1);
                        Update_Drop_boxes();
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void MoveAdjustment(int slot, Vector3 movement)
        {
            GUILayout.Label("Movement", _labelstyle);
            GUILayout.BeginHorizontal();
            {
                _selectedadjustment[0] = GUILayout.Toolbar(_selectedadjustment[0], Moveadjustment, _buttonstyle,
                    GUILayout.ExpandWidth(false));
                Customvalues[0] = GUILayout.TextField(Customvalues[0], _fieldstyle);
            }
            GUILayout.EndHorizontal();

            var adjustment = 0f;
            switch (_selectedadjustment[0])
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
                    float.TryParse(Customvalues[0], out adjustment);
                    break;
            }

            for (var vectortype = 0; vectortype < 3; vectortype++)
                AdjustmentTool(slot, 0, vectortype, Vectornames[vectortype], movement[vectortype], adjustment);
        }

        private void RotAdjustment(int slot, Vector3 rotation)
        {
            GUILayout.Label("Rotation", _labelstyle);
            GUILayout.BeginHorizontal();
            {
                _selectedadjustment[1] = GUILayout.Toolbar(_selectedadjustment[1], Rotationadjustment, _buttonstyle,
                    GUILayout.ExpandWidth(false));
                Customvalues[1] = GUILayout.TextField(Customvalues[1], _fieldstyle);
            }
            GUILayout.EndHorizontal();
            var adjustment = 0f;
            switch (_selectedadjustment[1])
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
                    float.TryParse(Customvalues[1], out adjustment);
                    break;
            }

            for (var vectortype = 0; vectortype < 3; vectortype++)
                AdjustmentTool(slot, 1, vectortype, Vectornames[vectortype], rotation[vectortype], adjustment);
        }

        private void ScaleAdjustment(int slot, Vector3 scale)
        {
            GUILayout.Label("Scale", _labelstyle);
            GUILayout.BeginHorizontal();
            {
                _selectedadjustment[2] = GUILayout.Toolbar(_selectedadjustment[2], Scaleadjustment, _buttonstyle,
                    GUILayout.ExpandWidth(false));
                Customvalues[2] = GUILayout.TextField(Customvalues[2], _fieldstyle);
            }
            GUILayout.EndHorizontal();

            var adjustment = 0f;

            switch (_selectedadjustment[2])
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
                    float.TryParse(Customvalues[2], out adjustment);
                    break;
            }

            for (var vectortype = 0; vectortype < 3; vectortype++)
                AdjustmentTool(slot, 2, vectortype, Vectornames[vectortype], scale[vectortype], adjustment);
        }

        private void AdjustmentTool(int slot, int typeindex, int vectortype, string label, float value,
            float adjustment)
        {
            var resultingvalue = 0f;
            var reset = false;
            var fullreset = false;
            var assigned = false;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(label, _labelstyle);

                if (GUILayout.Button("◀", _buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    resultingvalue = -adjustment;
                    assigned = true;
                }

                if (GUILayout.Button("▶", _buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    resultingvalue = adjustment;
                    assigned = true;
                }

                var original = value.ToString(CultureInfo.InvariantCulture);
                var result = GUILayout.TextField(original, _fieldstyle);
                if (original != result && float.TryParse(result, out var resultValue))
                {
                    resultingvalue = resultValue - value;
                    assigned = true;
                }

                if (GUILayout.Button("Reset", _buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    reset = true;
                    assigned = true;
                }

                if (GUILayout.Button("Recur Reset", _buttonstyle, GUILayout.ExpandWidth(false)))
                {
                    fullreset = true;
                    assigned = true;
                }

                if (assigned)
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
            GUILayout.EndHorizontal();
        }

        private void AddTheme(bool auto = false)
        {
            var count = ParentGroups.Count;
            var name = new CustomName("Parent Group " + count);
            if (auto) name.ParentSlot = AccessoriesApi.SelectedMakerAccSlot;
            ParentGroups.Add(name);
            Update_Drop_boxes();
            UpdateRelations();
        }

        private void UpdateRelations()
        {
            var dict = RelatedNames;
            var names = ParentGroups;
            var childdict = Child;

            dict.Clear();
            childdict.Clear();

            var n = names.Count;
            for (var i = 0; i < n; i++)
            {
                var item = names[i];
                if (!dict.TryGetValue(item.ParentSlot, out var itembindings))
                {
                    itembindings = new List<int>();
                    dict[item.ParentSlot] = itembindings;
                }

                TryChildListBySlot(item.ParentSlot, out var allchild, true);
                allchild = allchild.Distinct().ToList();

                foreach (var child in allchild)
                {
                    if (!dict.TryGetValue(child, out var childbindings))
                    {
                        childbindings = new List<int>();
                        dict[child] = childbindings;
                    }

                    itembindings.Add(child);
                    childbindings.Add(item.ParentSlot);
                    childdict[child] = item.ParentSlot;
                }
            }
        }

        private void UpdateRelations(int outfitNum)
        {
            if (!_parentData.TryGetValue(outfitNum, out var data))
            {
                data = new CoordinateData();
                _parentData[outfitNum] = data;
            }

            var names = data.parentGroups;
            var dict = data.RelatedNames;
            var childdict = data.Child;

            dict.Clear();
            childdict.Clear();

            var n = names.Count;
            for (var i = 0; i < n; i++)
            {
                var item = names[i];
                if (!dict.TryGetValue(item.ParentSlot, out var parentbindings)) parentbindings = new List<int>();
                TryChildListBySlot(item.ParentSlot, out var allchild, true);

                allchild = allchild.Distinct().ToList();

                foreach (var child in allchild)
                {
                    if (!dict.TryGetValue(child, out var childbindings)) childbindings = new List<int>();
                    parentbindings.Add(child);
                    childbindings.Add(item.ParentSlot);
                    childdict[child] = item.ParentSlot;
                    dict[child] = childbindings.Distinct().ToList();
                }

                dict[item.ParentSlot] = parentbindings.Distinct().ToList();
            }
        }

        private static void DrawFontSize()
        {
            if (GUILayout.Button("GUI-", _buttonstyle)) SetFontSize(Math.Max(_labelstyle.fontSize - 1, 5));
            if (GUILayout.Button("GUI+", _buttonstyle)) SetFontSize(1 + _labelstyle.fontSize);
        }

        private static void SetFontSize(int size)
        {
            _labelstyle.fontSize = size;
            _buttonstyle.fontSize = size;
            _fieldstyle.fontSize = size;
            _togglestyle.fontSize = size;
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