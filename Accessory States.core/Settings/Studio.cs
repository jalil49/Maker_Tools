using ExtensibleSaveFormat;
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
        private static Vector2 NameScrolling = new Vector2();

        internal static GUIStyle LabelStyle;
        internal static GUIStyle ButtonStyle;
        internal static GUIStyle FieldStyle;
        internal static GUIStyle ToggleStyle;
        internal static GUIStyle SliderStyle;
        internal static GUIStyle SliderThumbStyle;
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

            currentStateCategory.AddControl(button);

            ExtendedSave.SceneBeingLoaded += ExtendedSave_SceneBeingLoaded;
        }

        private void ExtendedSave_SceneBeingLoaded(string path)
        {
            StartCoroutine(Wait());
            IEnumerator<int> Wait()
            {
                CharaEvent.DisableRefresh = true;
                for (var i = 0; i < 10; i++)
                {
                    yield return 0;
                }
                CharaEvent.DisableRefresh = false;
            }
        }

        internal void OnGUI()
        {
            if (LabelStyle == null)
            {
                LabelStyle = new GUIStyle(GUI.skin.label);
                ButtonStyle = new GUIStyle(GUI.skin.button);
                FieldStyle = new GUIStyle(GUI.skin.textField);
                ToggleStyle = new GUIStyle(GUI.skin.toggle);
                SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
                SliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
                ButtonStyle.hover.textColor = Color.red;
                ButtonStyle.onNormal.textColor = Color.red;

                SetFontSize(Screen.height / 108);
            }

            if (!ShowStudioGui) return;

            DrawStudioGUI();
        }

        internal static void DrawFontSize()
        {
            if (GUILayout.Button("GUI-", ButtonStyle))
            {
                SetFontSize(Math.Max(LabelStyle.fontSize - 1, 5));
            }
            if (GUILayout.Button("GUI+", ButtonStyle))
            {
                SetFontSize(1 + LabelStyle.fontSize);
            }
        }

        internal static void SetFontSize(int size)
        {
            LabelStyle.fontSize = size;
            ButtonStyle.fontSize = size;
            FieldStyle.fontSize = size;
            ToggleStyle.fontSize = size;
            SliderStyle.fontSize = size;
            SliderThumbStyle.fontSize = size;
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
                    var Now_Parented_Name_Dictionary = controller.ParentedNameDictionary;
                    var GUI_Custom_Dict = controller.GUI_Custom_Dict;
                    var GUI_Parent_Dict = controller.GUI_Parent_Dict;
                    if (Names.Count == 0 && Now_Parented_Name_Dictionary.Count == 0)
                    {
                        GUILayout.Label($"No custom data found for {controller.ChaFileControl.parameter.fullname}", LabelStyle);
                        continue;
                    }
                    GUILayout.Label($"Character: {controller.ChaFileControl.parameter.fullname}", LabelStyle);
                    var nameint = 0;
                    foreach (var item in Names)
                    {
                        if (!GUI_Custom_Dict.TryGetValue(nameint, out var array))
                        {
                            GUI_Custom_Dict[nameint] = array = new int[] { 0, controller.MaxState(nameint) };
                        }
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(item.Name, LabelStyle);
                            GUILayout.Label(controller.StateDescription(nameint, array[0]), LabelStyle);
                        }
                        GUILayout.EndHorizontal();
                        var round = Mathf.RoundToInt(GUILayout.HorizontalSlider(array[0], 0, array[1], SliderStyle, SliderThumbStyle));
                        if (round != array[0])
                        {
                            controller.CustomGroups(nameint, round);
                            array[0] = round;
                        }
                        nameint++;
                    }
                    foreach (var item in Now_Parented_Name_Dictionary)
                    {
                        if (!GUI_Parent_Dict.TryGetValue(item.Key, out var show))
                        {
                            GUI_Parent_Dict[item.Key] = show = true;
                        }

                        if (GUILayout.Button($"{item.Key}: {OnOff(show)}", ButtonStyle))
                        {
                            controller.ParentToggle(item.Key, !show);
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
                if (GUILayout.Button("X", ButtonStyle, GUILayout.ExpandWidth(false)))
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
