using KKAPI.Studio;
using KKAPI.Studio.UI;
using KKAPI.Utilities;
using Studio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Accessory_States
{
    public partial class Settings
    {
        internal static bool ShowStudioGui;

        private Rect screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f), (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));
        static private Vector2 NameScrolling = new Vector2();

        internal static GUIStyle labelstyle;
        internal static GUIStyle buttonstyle;
        internal static GUIStyle fieldstyle;
        internal static GUIStyle togglestyle;
        internal static GUIStyle sliderstyle;
        internal static GUIStyle sliderthumbstyle;
        private bool mouseassigned;
        private readonly List<CharaEvent> StudioList = new List<CharaEvent>();
        private void CreateStudioControls()
        {
            var currentStateCategory = StudioAPI.GetOrCreateCurrentStateCategory("Acc. States");
            var button = new CurrentStateCategorySwitch("GUI Toggle", delegate (OCIChar chara)
            {
                return ShowStudioGui;
            });
            UniRx.ObservableExtensions.Subscribe(button.Value, delegate (bool value)
            {
                StudioList.Clear();
                foreach (var Controller in StudioAPI.GetSelectedControllers<CharaEvent>())
                {
                    ShowStudioGui = value;
                    StudioList.Add(Controller);
                }
            });

            var test = currentStateCategory.AddControl(button);
        }


        internal void OnGUI()
        {
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

            if (!ShowStudioGui) return;

            DrawStudioGUI();
        }

        internal static void DrawFontSize()
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

        internal static void SetFontSize(int size)
        {
            labelstyle.fontSize = size;
            buttonstyle.fontSize = size;
            fieldstyle.fontSize = size;
            togglestyle.fontSize = size;
            sliderstyle.fontSize = size;
            sliderthumbstyle.fontSize = size;
        }

        private void DrawStudioGUI()
        {
            IMGUIUtils.DrawSolidBox(screenRect);
            screenRect = GUILayout.Window(2901, screenRect, StudioGUI, "Accessory States GUI");
        }

        private void StudioGUI(int id)
        {
            Topoptions();
            NameScrolling = GUILayout.BeginScrollView(NameScrolling);
            GUILayout.BeginVertical();
            {
                foreach (var controller in StudioList)
                {
                    var Names = controller.Names;
                    var Now_Parented_Name_Dictionary = controller.Now_Parented_Name_Dictionary;
                    var GUI_Custom_Dict = controller.GUI_Custom_Dict;
                    var GUI_Parent_Dict = controller.GUI_Parent_Dict;
                    if (Names.Count == 0 && Now_Parented_Name_Dictionary.Count == 0)
                    {
                        GUILayout.Label($"No custom data found for {controller.ChaFileControl.parameter.fullname}", labelstyle);
                        continue;
                    }
                    GUILayout.Label($"Character: {controller.ChaFileControl.parameter.fullname}", labelstyle);
                    foreach (var item in Names)
                    {
                        if (!GUI_Custom_Dict.TryGetValue(item.Key, out var array))
                        {
                            GUI_Custom_Dict[item.Key] = array = new int[] { 0, controller.MaxState(item.Key) };
                        }
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(item.Value.Name, labelstyle);
                            GUILayout.Label(controller.StateDescription(item.Key, array[0]), labelstyle);
                        }
                        GUILayout.EndHorizontal();
                        var round = Mathf.RoundToInt(GUILayout.HorizontalSlider(array[0], 0, array[1], sliderstyle, sliderthumbstyle));
                        if (round != array[0])
                        {
                            controller.Custom_Groups(item.Key, round);
                            array[0] = round;
                        }
                    }
                    foreach (var item in Now_Parented_Name_Dictionary)
                    {
                        if (!GUI_Parent_Dict.TryGetValue(item.Key, out var show))
                        {
                            GUI_Parent_Dict[item.Key] = show = true;
                        }

                        if (GUILayout.Button($"{item.Key}: {OnOff(show)}", buttonstyle))
                        {
                            controller.Parent_toggle(item.Key, !show);
                            GUI_Parent_Dict[item.Key] = !show;
                        }
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            screenRect = IMGUIUtils.DragResizeEatWindow(800, screenRect);

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
                    ShowStudioGui = false;
                }
            }
            GUILayout.EndHorizontal();
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

        private string OnOff(bool show)
        {
            if (show)
            {
                return "On";
            }
            return "Off";
        }
    }
}
