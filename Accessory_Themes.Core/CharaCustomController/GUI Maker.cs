using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static bool ShowCustomGui = false;
        static public Dictionary<int, int> Gui_states = new Dictionary<int, int>();

        static private Vector2 NameScrolling = new Vector2();
        static private Vector2 StateScrolling = new Vector2();
        static private bool mouseassigned = false;
        static Rect screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f), (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        static bool showdelete = false;

        static GUIStyle labelstyle;
        static GUIStyle buttonstyle;
        static GUIStyle fieldstyle;
        static GUIStyle togglestyle;

        internal void OnGUI()
        {
            if (!ShowCustomGui || !AccessoriesApi.AccessoryCanvasVisible || !MakerAPI.IsInterfaceVisible())
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

            IMGUIUtils.DrawSolidBox(screenRect);
            screenRect = GUILayout.Window(2902, screenRect, CustomGui, $"Accessory Themes Gui: Slot {AccessoriesApi.SelectedMakerAccSlot + 1}");
        }

        private void CustomGui(int id)
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            var partinfo = AccessoriesApi.GetPartsInfo(slot);
            var valid = partinfo.type != 120;
            var themes = Themes;
            if (!Theme_Dict.TryGetValue(slot, out var themenum))
            {
                themenum = -1;
            }
            GUILayout.BeginVertical();
            {
                Topoptions();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Add Theme", buttonstyle))
                            {
                                AddThemeValueToList(slot, false);
                            }
                        }
                        GUILayout.EndHorizontal();

                        NameScrolling = GUILayout.BeginScrollView(NameScrolling);
                        {
                            DrawParentNames(slot, valid, themenum);
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        StateScrolling = GUILayout.BeginScrollView(StateScrolling);
                        {
                            if (themenum >= 0)
                            {
                                var theme = themes[themenum];
                                GUILayout.Label(theme.ThemeName, labelstyle, GUILayout.ExpandWidth(true));
                                theme.Isrelative = GUILayout.Toggle(theme.Isrelative, "Theme is relative", togglestyle);
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

            screenRect = IMGUIUtils.DragResizeEatWindow(2902, screenRect);
        }

        private void Drawbulk()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Start", labelstyle);
                bulkrange[0] = GUILayout.TextField(bulkrange[0], fieldstyle);
                GUILayout.Label("Stop", labelstyle);
                bulkrange[1] = GUILayout.TextField(bulkrange[1], fieldstyle);
                if (GUILayout.Button("Bulk", buttonstyle))
                {
                    BulkProcess(ThemesDropdownwrapper.GetSelectedValue() - 1);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawParentNames(int slot, bool valid, int bindedtheme)
        {
            var themelist = Themes;
            for (int themenum = 0, n = themelist.Count; themenum < n; themenum++)
            {
                var theme = themelist[themenum];
                var ispart = themenum == bindedtheme;
                if (ispart)
                    GUILayout.BeginHorizontal(GUI.skin.box);
                else
                    GUILayout.BeginHorizontal();
                {
                    theme.ThemeName = GUILayout.TextField(theme.ThemeName, fieldstyle);
                    if (valid && !ispart && GUILayout.Button("Bind", buttonstyle, GUILayout.ExpandWidth(false)))
                    {
                        if (bindedtheme >= 0)
                        {
                            themelist[bindedtheme].ThemedSlots.RemoveAt(slot);
                        }
                        Theme_Dict[slot] = themenum;
                        theme.ThemedSlots.Add(slot);
                    }
                    if (ispart && GUILayout.Button("Unbind", buttonstyle, GUILayout.ExpandWidth(false)))
                    {
                        Theme_Dict.Remove(slot);
                        theme.ThemedSlots.Remove(slot);
                    }
                    if (showdelete && GUILayout.Button("Delete", buttonstyle, GUILayout.ExpandWidth(false)))
                    {
                        themelist.RemoveAt(themenum);
                        themenum--;
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
                Tolerance = GUILayout.TextField(Tolerance, fieldstyle);
                GUILayout.Label("Tolerance", labelstyle);
                if (GUILayout.Button("Auto Generate Themes", buttonstyle))
                {
                    AutoTheme();
                }
                Clearthemes = GUILayout.Toggle(Clearthemes, "Clear Themes", togglestyle, GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
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
    }
}
