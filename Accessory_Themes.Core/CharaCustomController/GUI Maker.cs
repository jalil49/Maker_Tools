using System;
using System.Collections.Generic;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Utilities;
using UnityEngine;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private static bool _showCustomGui;
        public static Dictionary<int, int> GuiStates = new Dictionary<int, int>();

        private static Vector2 _nameScrolling;
        private static Vector2 _stateScrolling;
        private static bool _mouseassigned;

        private static Rect _screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f),
            (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        private static bool _showdelete;

        private static GUIStyle _labelstyle;
        private static GUIStyle _buttonstyle;
        private static GUIStyle _fieldstyle;
        private static GUIStyle _togglestyle;

        internal void OnGUI()
        {
            if (!_showCustomGui || !AccessoriesApi.AccessoryCanvasVisible || !MakerAPI.IsInterfaceVisible()) return;

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

            IMGUIUtils.DrawSolidBox(_screenRect);
            _screenRect = GUILayout.Window(2902, _screenRect, CustomGui,
                $"Accessory Themes Gui: Slot {AccessoriesApi.SelectedMakerAccSlot + 1}");
        }

        private void CustomGui(int id)
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            var partinfo = AccessoriesApi.GetPartsInfo(slot);
            var valid = partinfo.type != 120;
            var themes = Themes;
            if (!ThemeDict.TryGetValue(slot, out var themeNum)) themeNum = -1;
            GUILayout.BeginVertical();
            {
                Topoptions();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Add Theme", _buttonstyle)) AddThemeValueToList(slot, false);
                        }
                        GUILayout.EndHorizontal();

                        _nameScrolling = GUILayout.BeginScrollView(_nameScrolling);
                        {
                            DrawParentNames(slot, valid, themeNum);
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        _stateScrolling = GUILayout.BeginScrollView(_stateScrolling);
                        {
                            if (themeNum >= 0)
                            {
                                var theme = themes[themeNum];
                                GUILayout.Label(theme.ThemeName, _labelstyle, GUILayout.ExpandWidth(true));
                                theme.IsRelative =
                                    GUILayout.Toggle(theme.IsRelative, "Theme is relative", _togglestyle);
                            }

                            Drawbulk();
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            _screenRect = IMGUIUtils.DragResizeEatWindow(2902, _screenRect);
        }

        private void Drawbulk()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Start", _labelstyle);
                Bulkrange[0] = GUILayout.TextField(Bulkrange[0], _fieldstyle);
                GUILayout.Label("Stop", _labelstyle);
                Bulkrange[1] = GUILayout.TextField(Bulkrange[1], _fieldstyle);
                if (GUILayout.Button("Bulk", _buttonstyle)) BulkProcess(_themesDropdownwrapper.GetSelectedValue() - 1);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawParentNames(int slot, bool valid, int bindedtheme)
        {
            var themelist = Themes;
            for (int themeNum = 0, n = themelist.Count; themeNum < n; themeNum++)
            {
                var theme = themelist[themeNum];
                var ispart = themeNum == bindedtheme;
                if (ispart)
                    GUILayout.BeginHorizontal(GUI.skin.box);
                else
                    GUILayout.BeginHorizontal();
                {
                    theme.ThemeName = GUILayout.TextField(theme.ThemeName, _fieldstyle);
                    if (valid && !ispart && GUILayout.Button("Bind", _buttonstyle, GUILayout.ExpandWidth(false)))
                    {
                        if (bindedtheme >= 0) themelist[bindedtheme].ThemedSlots.RemoveAt(slot);
                        ThemeDict[slot] = themeNum;
                        theme.ThemedSlots.Add(slot);
                    }

                    if (ispart && GUILayout.Button("Unbind", _buttonstyle, GUILayout.ExpandWidth(false)))
                    {
                        ThemeDict.Remove(slot);
                        theme.ThemedSlots.Remove(slot);
                    }

                    if (_showdelete && GUILayout.Button("Delete", _buttonstyle, GUILayout.ExpandWidth(false)))
                    {
                        themelist.RemoveAt(themeNum);
                        themeNum--;
                        n--;
                        PopulateThemeDict();
                    }
                }
                GUILayout.EndHorizontal();
            }
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
                _showdelete = GUILayout.Toggle(_showdelete, "Enable Delete", _togglestyle,
                    GUILayout.ExpandWidth(false));
                _tolerance = GUILayout.TextField(_tolerance, _fieldstyle);
                GUILayout.Label("Tolerance", _labelstyle);
                if (GUILayout.Button("Auto Generate Themes", _buttonstyle)) AutoTheme();
                _clearthemes = GUILayout.Toggle(_clearthemes, "Clear Themes", _togglestyle,
                    GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
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