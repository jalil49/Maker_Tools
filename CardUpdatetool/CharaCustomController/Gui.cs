using ChaCustom;
using ExtensibleSaveFormat;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;

namespace CardUpdateTool
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static private Vector2 StateScrolling = new Vector2();
        static private Vector2 PathScrolling = new Vector2();
        static private Vector3 mousepos = new Vector3();
        static private bool mouseassigned = false;
        static private bool moveassigned = false;
        static Rect screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f), (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        static string CharacterPath = new System.IO.DirectoryInfo(UserData.Path).FullName + "chara";
        static string CoordinatePath = new System.IO.DirectoryInfo(UserData.Path).FullName + "coordinate";
        static bool UpdateCoordinates = false;
        static int Tabvalue;
        static readonly string[] Tabnames = new string[] { "Coordinates", "Characters" };

        static int InProgressCount = 0;
        static int ProgressTotal = 0;

        static bool ShowForce = false;
        static bool ShowFullyupdated = false;

        static bool ForceStop = false;

        static GUIStyle labelstyle;
        static GUIStyle buttonstyle;
        static GUIStyle fieldstyle;
        static GUIStyle togglestyle;

        internal void OnGUI()
        {
            if (!ShowGui || !MakerAPI.IsInterfaceVisible())
            {
                return;
            }

            if (labelstyle == null)
            {
                labelstyle = new GUIStyle(GUI.skin.label);
                buttonstyle = new GUIStyle(GUI.skin.button);
                fieldstyle = new GUIStyle(GUI.skin.textField);
                togglestyle = new GUIStyle(GUI.skin.toggle);
                SetFontSize(Screen.height / 108);
            }

            IMGUIUtils.DrawSolidBox(screenRect);
            GUILayout.Window(2903, screenRect, CustomGui, $"Card Update Tool GUI");
        }

        private void CustomGui(int id)
        {
            GUILayout.BeginVertical();
            {
                Topoptions();
                switch (Tabvalue)
                {
                    case 0:
                        DrawCoordinateWindow();
                        break;
                    case 1:
                        DrawCharacterWindow();
                        break;
                    default:
                        break;
                }
            }
            GUILayout.EndVertical();

            screenRect = IMGUIUtils.DragResizeEatWindow(2903, screenRect);
        }

        private void DrawCoordinateWindow()
        {
            if (UpdateInProgress)
            {
                GUILayout.Label($"In Progress {InProgressCount}/{ProgressTotal}", labelstyle);

                GUILayout.Label($"# Of outfits with outdated mods {CardInfo.Coord_OutMissMods[0]}", labelstyle);
                GUILayout.Label($"# Of outfits with missing mods {CardInfo.Coord_OutMissMods[1]}", labelstyle);
                return;
            }

            GUILayout.BeginHorizontal();
            {
                PathScrolling = GUILayout.BeginScrollView(PathScrolling, GUILayout.ExpandHeight(false));
                {
                    CoordinatePath = GUILayout.TextField(CoordinatePath, fieldstyle);
                }
                GUILayout.EndScrollView();

            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Update Outfit List", buttonstyle))
                {
                    OutfitCardCheck(CoordinatePath);
                }
                GUILayout.Label($"Card Count {OutfitList.Count}", labelstyle);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"# Of outfits with outdated mods {CardInfo.Coord_OutMissMods[0]}", labelstyle);

                if (CardInfo.Coord_OutMissMods[0] > 0 && GUILayout.Button("Update", buttonstyle))
                {
                    var cardlist = OutfitList.Where(x => x.OutdatedMods).ToList();
                    StartCoroutine(OutfitCardsUpdate(cardlist));
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"# Of outfits with missing mods {CardInfo.Coord_OutMissMods[1]}", labelstyle);

                if (CardInfo.Coord_OutMissMods[1] > 0 && GUILayout.Button("Move to Seperate Directory", buttonstyle))
                {
                    var cardlist = OutfitList.Where(x => x.MissingMods).ToArray();
                    for (int i = 0, n = cardlist.Length; i < n; i++)
                    {
                        var card = cardlist[i];
                        MoveToNewDirectory(CoordinatePath, card.Path, "/MissingMods/");
                        OutfitList.Remove(card);
                    }
                    UpdateOutmiss();
                }

                if (ShowForce && CardInfo.Coord_OutMissMods[1] > 0 && GUILayout.Button("Force Update", buttonstyle))
                {
                    var cardlist = OutfitList.Where(x => x.OutdatedMods || x.MissingMods).ToList();
                    StartCoroutine(OutfitCardsUpdate(cardlist));
                }
            }
            GUILayout.EndHorizontal();

            DrawOutfitUpdateOptions();
        }

        private void DrawCharacterWindow()
        {
            if (UpdateInProgress)
            {
                GUILayout.Label($"In Progress {InProgressCount}/{ProgressTotal}", labelstyle);

                GUILayout.Label($"# Of Characters with outdated mods {CardInfo.Chara_OutMissMods[0]}", labelstyle);
                GUILayout.Label($"# Of Characters with missing mods {CardInfo.Chara_OutMissMods[1]}", labelstyle);
                return;
            }

            GUILayout.BeginHorizontal();
            {
                PathScrolling = GUILayout.BeginScrollView(PathScrolling, GUILayout.ExpandHeight(false));
                {
                    CharacterPath = GUILayout.TextField(CharacterPath, fieldstyle);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Update Character List", buttonstyle))
                {
                    CharacterCardCheck(CharacterPath);
                }
                GUILayout.Label($"Card Count {CharacterList.Count}", labelstyle);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"# Of Characters with outdated mods {CardInfo.Chara_OutMissMods[0]}", labelstyle);

                if (CardInfo.Chara_OutMissMods[0] > 0 && GUILayout.Button("Update", buttonstyle))
                {
                    var cardlist = CharacterList.Where(x => x.OutdatedMods).ToList();

                    StartCoroutine(CharactersCardsUpdate(cardlist, false));
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"# Of Characters with missing mods {CardInfo.Chara_OutMissMods[1]}", labelstyle);

                if (CardInfo.Chara_OutMissMods[1] > 0 && GUILayout.Button("Move to Seperate Directory", buttonstyle))
                {
                    var cardlist = CharacterList.Where(x => x.MissingMods).ToArray();
                    for (int i = 0, n = cardlist.Length; i < n; i++)
                    {
                        var card = cardlist[i];
                        MoveToNewDirectory(CharacterPath, card.Path, "/MissingMods/");
                        CharacterList.Remove(card);
                    }
                    UpdateOutmiss();
                }

                if (ShowForce && CardInfo.Chara_OutMissMods[1] > 0 && GUILayout.Button("Force Update", buttonstyle))
                {
                    var cardlist = CharacterList.Where(x => x.OutdatedMods || x.MissingMods).ToList();
                    StartCoroutine(CharactersCardsUpdate(cardlist, false));
                }
            }
            GUILayout.EndHorizontal();

            DrawCharacterUpdateOptions();
        }

        private void UpdateOptionHeader()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"Updateable GUIDS", labelstyle, GUILayout.ExpandWidth(false));
                ShowFullyupdated = GUILayout.Toggle(ShowFullyupdated, "Show Fully Updated", togglestyle);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCharacterUpdateOptions()
        {
            UpdateOptionHeader();
            StateScrolling = GUILayout.BeginScrollView(StateScrolling, GUI.skin.box);
            {
                var maxversions = CardInfo.CurrentMaxVersion;
                for (int j = 0, jn = maxversions.Count; j < jn; j++)
                {
                    var Guid = maxversions.ElementAt(j);
                    if (!Guid.Value.Charavisible || !ShowFullyupdated && !ShowForce && !Guid.Value.AnyCharaOutdated)
                    {
                        continue;
                    }
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.TextField($"{Guid.Value.Name}: Ver.{Guid.Value.version}", labelstyle);

                        if (Guid.Value.AnyCharaOutdated && GUILayout.Button("Update", buttonstyle))
                        {
                            var cardlist = CharacterList.Where(x => UpdateByGuid(x, Guid.Key, false)).ToList();
                            StartCoroutine(CharactersCardsUpdate(cardlist, UpdateCoordinates));
                        }
                        if (ShowForce && GUILayout.Button("Update Missing", buttonstyle))
                        {
                            var cardlist = CharacterList.Where(x => UpdateByGuid(x, Guid.Key, true)).ToList();
                            StartCoroutine(CharactersCardsUpdate(cardlist, UpdateCoordinates));
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawOutfitUpdateOptions()
        {
            UpdateOptionHeader();
            StateScrolling = GUILayout.BeginScrollView(StateScrolling, GUI.skin.box);
            {
                var maxversions = CardInfo.CurrentMaxVersion;
                for (int i = 0, jn = maxversions.Count; i < jn; i++)
                {
                    var Guid = maxversions.ElementAt(i);
                    if (!Guid.Value.OutfitVisible || !ShowFullyupdated && !ShowForce && !Guid.Value.AnyOutfitOutdated)
                    {
                        continue;
                    }
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.TextField($"{Guid.Value.Name}: Ver.{Guid.Value.version}", labelstyle);

                        if (Guid.Value.AnyOutfitOutdated && GUILayout.Button("Update", buttonstyle))
                        {
                            var cardlist = OutfitList.Where(x => UpdateByGuid(x, Guid.Key, false)).ToList();
                            StartCoroutine(OutfitCardsUpdate(cardlist));
                        }
                        if (ShowForce && GUILayout.Button("Update Missing", buttonstyle))
                        {
                            var cardlist = OutfitList.Where(x => UpdateByGuid(x, Guid.Key, true)).ToList();
                            StartCoroutine(OutfitCardsUpdate(cardlist));
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }

        private void Topoptions()
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
                    toggle.SetValue(false);
                    ShowGui = false;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                ShowForce = GUILayout.Toggle(ShowForce, "Show Force", togglestyle, GUILayout.ExpandWidth(false));
                if (Tabvalue == 1 && !UpdateInProgress)
                {
                    UpdateCoordinates = GUILayout.Toggle(UpdateCoordinates, "Update Each Coordinate", togglestyle, GUILayout.ExpandWidth(false));
                }
                if (GUILayout.Button("Update Outdated/Missing/GUIDS", buttonstyle))
                {
                    UpdateOutmiss();
                }
                if (UpdateInProgress && GUILayout.Button("Force Stop", buttonstyle))
                {
                    ForceStop = true;
                }
            }
            GUILayout.EndHorizontal();
            if (!UpdateInProgress)
            {
                Tabvalue = GUILayout.Toolbar(Tabvalue, Tabnames, buttonstyle);
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
    }
}
