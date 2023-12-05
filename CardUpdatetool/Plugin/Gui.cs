using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KKAPI.Maker;
using KKAPI.Utilities;
using UnityEngine;

namespace CardUpdateTool
{
    public partial class CardUpdateTool
    {
        private static Vector2 _stateScrolling;
        private static Vector2 _pathScrolling;
        private static bool _mouseassigned;

        private static Rect _screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f),
            (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        private static string _characterPath = new DirectoryInfo(UserData.Path).FullName + "chara";
        private static string _coordinatePath = new DirectoryInfo(UserData.Path).FullName + "coordinate";
        private static bool _updateCoordinates;
        private static int _tabvalue;
        private static readonly string[] Tabnames = { "Coordinates", "Characters" };

        private static int _inProgressCount;
        private static int _progressTotal;

        private static bool _showForce;
        private static bool _showFullyupdated;

        private static bool _forceStop;
        private static bool _pause;
        private static bool _waiting;

        private static GUIStyle _labelstyle;
        private static GUIStyle _buttonstyle;
        private static GUIStyle _fieldstyle;
        private static GUIStyle _togglestyle;

        internal void OnGUI()
        {
            if (!_showGui || !MakerAPI.IsInterfaceVisible()) return;

            if (_labelstyle == null)
            {
                _labelstyle = new GUIStyle(GUI.skin.label);
                _buttonstyle = new GUIStyle(GUI.skin.button);

                _buttonstyle.onActive.textColor = Color.green;
                _buttonstyle.active.textColor = Color.magenta;
                _buttonstyle.hover.textColor = Color.red;
                _buttonstyle.onNormal.textColor = Color.red;

                _fieldstyle = new GUIStyle(GUI.skin.textField);
                _togglestyle = new GUIStyle(GUI.skin.toggle);
                SetFontSize(Screen.height / 108);
            }

            IMGUIUtils.DrawSolidBox(_screenRect);
            _screenRect = GUILayout.Window(2903, _screenRect, CustomGui, "Card Update Tool GUI");
        }

        private void CustomGui(int id)
        {
            GUILayout.BeginVertical();
            {
                Topoptions();
                switch (_tabvalue)
                {
                    case 0:
                        DrawCoordinateWindow();
                        break;
                    case 1:
                        DrawCharacterWindow();
                        break;
                }

                GUILayout.Label(GUI.tooltip, _labelstyle, GUILayout.ExpandHeight(false));
            }
            GUILayout.EndVertical();
            _screenRect = IMGUIUtils.DragResizeEatWindow(2903, _screenRect);
        }

        private void DrawCoordinateWindow()
        {
            var outDatedcount = OutfitData.OutDatedcount;
            var missingcount = OutfitData.Missingcount;
            var migrateCount = OutfitData.Migratedcount;

            if (_updateInProgress)
            {
                Label($"In Progress {_inProgressCount}/{_progressTotal}");

                if (migrateCount > 0)
                    Label($"# Of outfits with Migrated mods {migrateCount}");

                if (outDatedcount > 0)
                    Label($"# Of outfits with Outdated mods {outDatedcount}");

                if (missingcount > 0)
                    Label($"# Of outfits with Missing mods {missingcount}");
                return;
            }

            GUILayout.BeginHorizontal();
            {
                _pathScrolling = GUILayout.BeginScrollView(_pathScrolling, GUILayout.ExpandHeight(false));
                {
                    _coordinatePath = Field(_coordinatePath, true);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (Button("Update Outfit List", "Get cards from directory above")) OutfitCardCheck(_coordinatePath);
                Label($"Card Count {OutfitList.Count}");
            }
            GUILayout.EndHorizontal();

            if (migrateCount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of outfits with migrated mods {migrateCount}", true);

                    if (Button("Update", "Mods whose parts were migrated, no data loss"))
                    {
                        var cardlist = OutfitList.Where(x => x.MigratedMods).ToList();
                        StartCoroutine(OutfitCardsUpdate(cardlist));
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (outDatedcount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of outfits with outdated mods {outDatedcount}", true);

                    if (Button("Move to Separate Directory", "Move to OutdatedMods folder in above the directory"))
                    {
                        var csv = OpenCsv(_coordinatePath);
                        if (csv != null)
                        {
                            var cardlist = OutfitList.Where(x => x.OutdatedMods).ToArray();
                            foreach (var card in cardlist)
                            {
                                MoveToNewDirectory(_coordinatePath, card, "/OutdatedMods/", ref csv);
                                OutfitList.Remove(card);
                            }

                            UpdateOutMissing();
                            WriteCsv(_coordinatePath, csv);
                        }
                    }

                    if (_showForce && Button("Update"))
                    {
                        var cardlist = OutfitList.Where(x => x.OutdatedMods).ToList();
                        StartCoroutine(OutfitCardsUpdate(cardlist));
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (missingcount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of outfits with missing mods {missingcount}", true);

                    if (Button("Move to Separate Directory", "Move to MissingMods folder in above the directory"))
                    {
                        var csv = OpenCsv(_coordinatePath);
                        if (csv != null)
                        {
                            var cardlist = OutfitList.Where(x => x.MissingMods).ToArray();
                            foreach (var card in cardlist)
                            {
                                MoveToNewDirectory(_coordinatePath, card, "/MissingMods/", ref csv);
                                OutfitList.Remove(card);
                            }

                            WriteCsv(_coordinatePath, csv);
                            UpdateOutMissing();
                        }
                    }

                    if (_showForce && Button("Force Update",
                            "Force update, if a sideloader toggle is enabled missing content will be lost"))
                    {
                        var cardlist = OutfitList.Where(x => x.MigratedMods || x.OutdatedMods || x.MissingMods)
                            .ToList();
                        StartCoroutine(OutfitCardsUpdate(cardlist));
                    }
                }
                GUILayout.EndHorizontal();
            }

            DrawOutfitUpdateOptions();
        }

        private void DrawCharacterWindow()
        {
            var outDatedCount = CharacterData.OutDatedcount;
            var missingcount = CharacterData.Missingcount;
            var migrateCount = CharacterData.Migratedcount;

            if (_updateInProgress)
            {
                Label($"In Progress {_inProgressCount}/{_progressTotal}");

                if (migrateCount > 0)
                    Label($"# Of Characters with Migrated mods {migrateCount}");

                if (outDatedCount > 0)
                    Label($"# Of Characters with Outdated mods {outDatedCount}");

                if (missingcount > 0)
                    Label($"# Of Characters with Missing mods {missingcount}");
                return;
            }

            GUILayout.BeginHorizontal();
            {
                _pathScrolling = GUILayout.BeginScrollView(_pathScrolling, GUILayout.ExpandHeight(false));
                {
                    _characterPath = Field(_characterPath, true);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (Button("Update Character List", "Get cards from directory above"))
                    CharacterCardCheck(_characterPath);
                Label($"Card Count {CharacterList.Count}");
            }
            GUILayout.EndHorizontal();

            if (migrateCount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of Characters with Migrated mods {migrateCount}", true);

                    if (Button("Update", "Mods whose parts were migrated, no data loss"))
                    {
                        var cardlist = CharacterList.Where(x => x.MigratedMods).ToList();

                        StartCoroutine(CharactersCardsUpdate(cardlist, _updateCoordinates));
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (outDatedCount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of Characters with outdated mods {outDatedCount}", true);

                    if (Button("Move to Separate Directory", "Move to OutdatedMods folder in above the directory"))
                    {
                        var csv = OpenCsv(_characterPath);
                        if (csv != null)
                        {
                            var cardlist = CharacterList.Where(x => x.OutdatedMods).ToArray();
                            foreach (var card in cardlist)
                            {
                                MoveToNewDirectory(_characterPath, card, "/OutdatedMods/", ref csv);
                                CharacterList.Remove(card);
                            }

                            WriteCsv(_characterPath, csv);
                            UpdateOutMissing();
                        }
                    }

                    if (_showForce && Button("Force Update", "Force update, missing content will be lost"))
                    {
                        var cardList = CharacterList.Where(x => x.OutdatedMods).ToList();

                        StartCoroutine(CharactersCardsUpdate(cardList, _updateCoordinates));
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (missingcount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of Characters with missing mods {missingcount}", true);

                    if (Button("Move to Separate Directory", "Move to MissingMods folder in above the directory"))
                    {
                        var csv = OpenCsv(_characterPath);
                        if (csv != null)
                        {
                            var cardlist = CharacterList.Where(x => x.MissingMods).ToArray();
                            foreach (var card in cardlist)
                            {
                                MoveToNewDirectory(_characterPath, card, "/MissingMods/", ref csv);
                                CharacterList.Remove(card);
                            }

                            WriteCsv(_characterPath, csv);
                        }

                        UpdateOutMissing();
                    }

                    if (_showForce && Button("Force Update",
                            "Force update, if a sideloader toggle is enabled missing content will be lost"))
                    {
                        var cardlist = CharacterList.Where(x => x.OutdatedMods || x.MissingMods).ToList();
                        StartCoroutine(CharactersCardsUpdate(cardlist, _updateCoordinates));
                    }
                }
                GUILayout.EndHorizontal();
            }

            DrawCharacterUpdateOptions();
        }

        private static void UpdateOptionHeader()
        {
            GUILayout.BeginHorizontal();
            {
                Label("Updateable GUIDS");
                _showFullyupdated = Toggle(_showFullyupdated, "Show Fully Updated");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCharacterUpdateOptions()
        {
            UpdateOptionHeader();
            _stateScrolling = GUILayout.BeginScrollView(_stateScrolling, GUI.skin.box);
            {
                var maxversions = StaticMaxVersion;
                foreach (var guid in CharacterData.PrintOrder)
                {
                    var versionData = maxversions[guid];
                    if (!versionData.CharaVisible ||
                        (!_showFullyupdated && !_showForce && !versionData.AnyCharaOutdated)) continue;
                    GUILayout.BeginHorizontal();
                    {
                        Label(
                            $"{versionData.Name}: Ver.{versionData.Version} #{versionData.CharaUpToDate + versionData.CharaOutdatedCount}",
                            true);

                        if (versionData.AnyCharaOutdated && Button($"Update #{versionData.CharaOutdatedCount}",
                                "These cards have an outdated version, note if after updating the number remains the same you might have an outdated plugin installed"))
                        {
                            var cardlist = CharacterList.Where(x => UpdateByGuid(x, guid, false)).ToList();
                            StartCoroutine(CharactersCardsUpdate(cardlist, _updateCoordinates));
                        }

                        if (_showForce && Button($"Update Missing #{CharacterList.Count - versionData.CharaUpToDate}",
                                $"Update all cards that lack {versionData.Name} data"))
                        {
                            var cardlist = CharacterList.Where(x => UpdateByGuid(x, guid, true)).ToList();
                            StartCoroutine(CharactersCardsUpdate(cardlist, _updateCoordinates));
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
            _stateScrolling = GUILayout.BeginScrollView(_stateScrolling, GUI.skin.box);
            {
                var maxversions = StaticMaxVersion;
                foreach (var guid in OutfitData.PrintOrder)
                {
                    var versionData = maxversions[guid];
                    if (!versionData.OutfitVisible ||
                        (!_showFullyupdated && !_showForce && !versionData.AnyOutfitOutdated)) continue;
                    GUILayout.BeginHorizontal();
                    {
                        Label(
                            $"{versionData.Name}: Ver.{versionData.Version} #{versionData.OutfitsUpToDate + versionData.OutfitOutdatedCount}",
                            true);

                        if (versionData.AnyOutfitOutdated && Button($"Update #{versionData.OutfitOutdatedCount}",
                                "These cards have an outdated version, note if after updating the number remains the same you might have an outdated plugin installed"))
                        {
                            var cardlist = OutfitList.Where(x => UpdateByGuid(x, guid, false)).ToList();
                            StartCoroutine(OutfitCardsUpdate(cardlist));
                        }

                        if (_showForce && Button($"Update Missing #{OutfitList.Count - versionData.OutfitsUpToDate}",
                                $"Update all cards that lack {versionData.Name} data"))
                        {
                            var cardlist = OutfitList.Where(x => UpdateByGuid(x, guid, true)).ToList();
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
                if (Input.GetMouseButtonDown(0) && !_mouseassigned && _screenRect.Contains(Input.mousePosition))
                    StartCoroutine(DragEvent());

                if (Button("X"))
                {
                    _toggle.SetValue(false);
                    _showGui = false;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                _showForce = Toggle(_showForce, "Show Force", "Show extra update options");
                if (_tabvalue == 1 && !_updateInProgress)
                    _updateCoordinates = Toggle(_updateCoordinates, "Coordinates",
                        "Run through each coordinate after loading");


                _pause = Toggle(_pause, "Pause", "Pause before saving");

                if (_updateInProgress && _waiting && Button("Continue", "Save and Load the next card"))
                    _waiting = false;

                if (Button("Update Outdated/Missing/GUIDS", "Update several numbers", true)) UpdateOutMissing();

                if (_updateInProgress && Button("Force Stop")) _forceStop = true;
            }
            GUILayout.EndHorizontal();
            if (!_updateInProgress) _tabvalue = GUILayout.Toolbar(_tabvalue, Tabnames, _buttonstyle);
        }

        private static bool Toggle(bool toggle, string text, string tooltip = "", bool expandwidth = false)
        {
            return GUILayout.Toggle(toggle, new GUIContent(text, tooltip), _togglestyle,
                GUILayout.ExpandWidth(expandwidth));
        }

        private static bool Button(string text, string tooltip = "", bool expandwidth = false)
        {
            return GUILayout.Button(new GUIContent(text, tooltip), _buttonstyle, GUILayout.ExpandWidth(expandwidth));
        }

        private static void Label(string text, bool expandwidth = false)
        {
            GUILayout.Label(text, _labelstyle, GUILayout.ExpandWidth(expandwidth));
        }

        private static string Field(string text, bool expandwidth = false)
        {
            return GUILayout.TextField(text, _fieldstyle, GUILayout.ExpandWidth(expandwidth));
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

        private static void DrawFontSize()
        {
            if (Button("GUI-", "Decrease GUI Size")) SetFontSize(Math.Max(_labelstyle.fontSize - 1, 5));
            if (Button("GUI+", "Increase GUI Size")) SetFontSize(1 + _labelstyle.fontSize);
        }

        private static void SetFontSize(int size)
        {
            _labelstyle.fontSize = size;
            _buttonstyle.fontSize = size;
            _fieldstyle.fontSize = size;
            _togglestyle.fontSize = size;
        }
    }
}