using System;
using System.Collections.Generic;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using KKAPI.Utilities;
using UnityEngine;
using ObservableExtensions = UniRx.ObservableExtensions;

namespace Accessory_States
{
    public partial class Settings
    {
        internal static bool ShowStudioGui;
        private static Vector2 _nameScrolling;

        internal static GUIStyle Labelstyle;
        internal static GUIStyle Buttonstyle;
        internal static GUIStyle Fieldstyle;
        internal static GUIStyle Togglestyle;
        internal static GUIStyle Sliderstyle;
        internal static GUIStyle Sliderthumbstyle;
        private readonly List<CharaEvent> _studioList = new List<CharaEvent>();
        private bool _mouseassigned;

        private Rect _screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f),
            (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));


        internal void OnGUI()
        {
            if (Labelstyle == null)
            {
                Labelstyle = new GUIStyle(GUI.skin.label);
                Buttonstyle = new GUIStyle(GUI.skin.button);
                Fieldstyle = new GUIStyle(GUI.skin.textField);
                Togglestyle = new GUIStyle(GUI.skin.toggle);
                Sliderstyle = new GUIStyle(GUI.skin.horizontalSlider);
                Sliderthumbstyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
                Buttonstyle.hover.textColor = Color.red;
                Buttonstyle.onNormal.textColor = Color.red;

                SetFontSize(Screen.height / 108);
            }

            if (!ShowStudioGui) return;

            DrawStudioGUI();
        }

        private void CreateStudioControls()
        {
            var currentStateCategory = StudioAPI.GetOrCreateCurrentStateCategory("Acc. States");
            var button = new CurrentStateCategorySwitch("GUI Toggle", delegate { return ShowStudioGui; });
            ObservableExtensions.Subscribe(button.Value, delegate(bool value)
            {
                _studioList.Clear();
                foreach (var controller in StudioAPI.GetSelectedControllers<CharaEvent>())
                {
                    ShowStudioGui = value;
                    _studioList.Add(controller);
                }
            });

            var test = currentStateCategory.AddControl(button);
        }

        internal static void DrawFontSize()
        {
            if (GUILayout.Button("GUI-", Buttonstyle)) SetFontSize(Math.Max(Labelstyle.fontSize - 1, 5));
            if (GUILayout.Button("GUI+", Buttonstyle)) SetFontSize(1 + Labelstyle.fontSize);
        }

        internal static void SetFontSize(int size)
        {
            Labelstyle.fontSize = size;
            Buttonstyle.fontSize = size;
            Fieldstyle.fontSize = size;
            Togglestyle.fontSize = size;
            Sliderstyle.fontSize = size;
            Sliderthumbstyle.fontSize = size;
        }

        private void DrawStudioGUI()
        {
            IMGUIUtils.DrawSolidBox(_screenRect);
            _screenRect = GUILayout.Window(2901, _screenRect, StudioGUI, "Accessory States GUI");
        }

        private void StudioGUI(int id)
        {
            Topoptions();
            _nameScrolling = GUILayout.BeginScrollView(_nameScrolling);
            GUILayout.BeginVertical();
            {
                foreach (var controller in _studioList)
                {
                    var names = controller.Names;
                    var nowParentedNameDictionary = controller.NowParentedNameDictionary;
                    var guiCustomDict = controller.GUICustomDict;
                    var guiParentDict = controller.GUIParentDict;
                    if (names.Count == 0 && nowParentedNameDictionary.Count == 0)
                    {
                        GUILayout.Label($"No custom data found for {controller.ChaFileControl.parameter.fullname}",
                            Labelstyle);
                        continue;
                    }

                    GUILayout.Label($"Character: {controller.ChaFileControl.parameter.fullname}", Labelstyle);
                    foreach (var item in names)
                    {
                        if (!guiCustomDict.TryGetValue(item.Key, out var array))
                            guiCustomDict[item.Key] = array = new[] { 0, controller.MaxState(item.Key) };
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(item.Value.Name, Labelstyle);
                            GUILayout.Label(controller.StateDescription(item.Key, array[0]), Labelstyle);
                        }
                        GUILayout.EndHorizontal();
                        var round = Mathf.RoundToInt(GUILayout.HorizontalSlider(array[0], 0, array[1], Sliderstyle,
                            Sliderthumbstyle));
                        if (round != array[0])
                        {
                            controller.Custom_Groups(item.Key, round);
                            array[0] = round;
                        }
                    }

                    foreach (var item in nowParentedNameDictionary)
                    {
                        if (!guiParentDict.TryGetValue(item.Key, out var show)) guiParentDict[item.Key] = show = true;

                        if (GUILayout.Button($"{item.Key}: {OnOff(show)}", Buttonstyle))
                        {
                            controller.Parent_toggle(item.Key, !show);
                            guiParentDict[item.Key] = !show;
                        }
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            _screenRect = IMGUIUtils.DragResizeEatWindow(800, _screenRect);
        }

        private void Topoptions()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                DrawFontSize();
                if (Input.GetMouseButtonDown(0) && !_mouseassigned && _screenRect.Contains(Input.mousePosition))
                    StartCoroutine(DragEvent());
                if (GUILayout.Button("X", Buttonstyle, GUILayout.ExpandWidth(false))) ShowStudioGui = false;
            }
            GUILayout.EndHorizontal();
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

        private string OnOff(bool show)
        {
            if (show) return "On";
            return "Off";
        }
    }
}