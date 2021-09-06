using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace CardUpdateTool
{
    public partial class CardUpdateTool
    {
        static private Vector2 StateScrolling = new Vector2();
        static private Vector2 PathScrolling = new Vector2();
        static private bool mouseassigned = false;
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
        static bool Pause = false;
        static bool Waiting = false;

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

                buttonstyle.onActive.textColor = Color.green;
                buttonstyle.active.textColor = Color.magenta;
                buttonstyle.hover.textColor = Color.red;
                buttonstyle.onNormal.textColor = Color.red;

                fieldstyle = new GUIStyle(GUI.skin.textField);
                togglestyle = new GUIStyle(GUI.skin.toggle);
                SetFontSize(Screen.height / 108);
            }

            IMGUIUtils.DrawSolidBox(screenRect);
            screenRect = GUILayout.Window(2903, screenRect, CustomGui, $"Card Update Tool GUI");
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
                GUILayout.Label(GUI.tooltip, labelstyle, GUILayout.ExpandHeight(false));
            }
            GUILayout.EndVertical();
            screenRect = IMGUIUtils.DragResizeEatWindow(2903, screenRect);
        }

        private void DrawCoordinateWindow()
        {
            var OutDatedcount = OutfitData.OutDatedcount;
            var Missingcount = OutfitData.Missingcount;
            var MigrateCount = OutfitData.Migratedcount;

            if (UpdateInProgress)
            {
                Label($"In Progress {InProgressCount}/{ProgressTotal}");

                if (MigrateCount > 0)
                    Label($"# Of outfits with Migrated mods {MigrateCount}");

                if (OutDatedcount > 0)
                    Label($"# Of outfits with Outdated mods {OutDatedcount}");

                if (Missingcount > 0)
                    Label($"# Of outfits with Missing mods {Missingcount}");
                return;
            }

            GUILayout.BeginHorizontal();
            {
                PathScrolling = GUILayout.BeginScrollView(PathScrolling, GUILayout.ExpandHeight(false));
                {
                    CoordinatePath = Field(CoordinatePath, true);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (Button("Update Outfit List", "Get cards from directory above"))
                {
                    OutfitCardCheck(CoordinatePath);
                }
                Label($"Card Count {OutfitList.Count}");
            }
            GUILayout.EndHorizontal();

            if (MigrateCount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of outfits with migrated mods {MigrateCount}", true);

                    if (Button("Update", "Mods whose parts were migrated, no data loss"))
                    {
                        var cardlist = OutfitList.Where(x => x.MigratedMods).ToList();
                        StartCoroutine(OutfitCardsUpdate(cardlist));
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (OutDatedcount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of outfits with outdated mods {OutDatedcount}", true);

                    if (Button("Move to Seperate Directory", "Move to OutdatedMods folder in above the directory"))
                    {
                        var csv = OpenCSV(CoordinatePath);
                        if (csv != null)
                        {
                            var cardlist = OutfitList.Where(x => x.OutdatedMods).ToArray();
                            foreach (var card in cardlist)
                            {
                                MoveToNewDirectory(CoordinatePath, card, "/OutdatedMods/", ref csv);
                                OutfitList.Remove(card);
                            }
                            UpdateOutmiss();
                            WriteCSV(CoordinatePath, csv);
                        }
                    }

                    if (ShowForce && Button("Update"))
                    {
                        var cardlist = OutfitList.Where(x => x.OutdatedMods).ToList();
                        StartCoroutine(OutfitCardsUpdate(cardlist));
                    }


                }
                GUILayout.EndHorizontal();
            }

            if (Missingcount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of outfits with missing mods {Missingcount}", true);

                    if (Button("Move to Seperate Directory", "Move to MissingMods folder in above the directory"))
                    {
                        var csv = OpenCSV(CoordinatePath);
                        if (csv != null)
                        {
                            var cardlist = OutfitList.Where(x => x.MissingMods).ToArray();
                            foreach (var card in cardlist)
                            {
                                MoveToNewDirectory(CoordinatePath, card, "/MissingMods/", ref csv);
                                OutfitList.Remove(card);
                            }
                            WriteCSV(CoordinatePath, csv);
                            UpdateOutmiss();
                        }
                    }

                    if (ShowForce && Button("Force Update", "Force update, if a sideloader toggle is enabled missing content will be lost"))
                    {
                        var cardlist = OutfitList.Where(x => x.MigratedMods || x.OutdatedMods || x.MissingMods).ToList();
                        StartCoroutine(OutfitCardsUpdate(cardlist));
                    }
                }
                GUILayout.EndHorizontal();
            }

            DrawOutfitUpdateOptions();
        }

        private void DrawCharacterWindow()
        {
            var OutDatedcount = CharacterData.OutDatedcount;
            var Missingcount = CharacterData.Missingcount;
            var MigrateCount = CharacterData.Migratedcount;

            if (UpdateInProgress)
            {
                Label($"In Progress {InProgressCount}/{ProgressTotal}");

                if (MigrateCount > 0)
                    Label($"# Of Characters with Migrated mods {MigrateCount}");

                if (OutDatedcount > 0)
                    Label($"# Of Characters with Outdated mods {OutDatedcount}");

                if (Missingcount > 0)
                    Label($"# Of Characters with Missing mods {Missingcount}");
                return;
            }

            GUILayout.BeginHorizontal();
            {
                PathScrolling = GUILayout.BeginScrollView(PathScrolling, GUILayout.ExpandHeight(false));
                {
                    CharacterPath = Field(CharacterPath, true);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (Button("Update Character List", "Get cards from directory above"))
                {
                    CharacterCardCheck(CharacterPath);
                }
                Label($"Card Count {CharacterList.Count}");
            }
            GUILayout.EndHorizontal();

            if (MigrateCount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of Characters with Migrated mods {MigrateCount}", true);

                    if (MigrateCount > 0 && Button("Update", "Mods whose parts were migrated, no data loss"))
                    {
                        var cardlist = CharacterList.Where(x => x.MigratedMods).ToList();

                        StartCoroutine(CharactersCardsUpdate(cardlist, UpdateCoordinates));
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (OutDatedcount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of Characters with outdated mods {OutDatedcount}", true);

                    if (Button("Move to Seperate Directory", "Move to OutdatedMods folder in above the directory"))
                    {
                        var csv = OpenCSV(CharacterPath);
                        if (csv != null)
                        {
                            var cardlist = CharacterList.Where(x => x.OutdatedMods).ToArray();
                            foreach (var card in cardlist)
                            {
                                MoveToNewDirectory(CharacterPath, card, "/OutdatedMods/", ref csv);
                                CharacterList.Remove(card);
                            }
                            WriteCSV(CharacterPath, csv);
                            UpdateOutmiss();
                        }
                    }

                    if (ShowForce && OutDatedcount > 0 && Button("Force Update", "Force update, missing content will be lost"))
                    {
                        var cardlist = CharacterList.Where(x => x.OutdatedMods).ToList();

                        StartCoroutine(CharactersCardsUpdate(cardlist, UpdateCoordinates));
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (Missingcount > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    Label($"# Of Characters with missing mods {Missingcount}", true);

                    if (Button("Move to Seperate Directory", "Move to MissingMods folder in above the directory"))
                    {
                        var csv = OpenCSV(CharacterPath);
                        if (csv != null)
                        {
                            var cardlist = CharacterList.Where(x => x.MissingMods).ToArray();
                            foreach (var card in cardlist)
                            {
                                MoveToNewDirectory(CharacterPath, card, "/MissingMods/", ref csv);
                                CharacterList.Remove(card);
                            }
                            WriteCSV(CharacterPath, csv);
                        }
                        UpdateOutmiss();
                    }

                    if (ShowForce && Missingcount > 0 && Button("Force Update", "Force update, if a sideloader toggle is enabled missing content will be lost"))
                    {
                        var cardlist = CharacterList.Where(x => x.OutdatedMods || x.MissingMods).ToList();
                        StartCoroutine(CharactersCardsUpdate(cardlist, UpdateCoordinates));
                    }
                }
                GUILayout.EndHorizontal();
            }

            DrawCharacterUpdateOptions();
        }

        private void UpdateOptionHeader()
        {
            GUILayout.BeginHorizontal();
            {
                Label($"Updateable GUIDS");
                ShowFullyupdated = Toggle(ShowFullyupdated, "Show Fully Updated");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCharacterUpdateOptions()
        {
            UpdateOptionHeader();
            StateScrolling = GUILayout.BeginScrollView(StateScrolling, GUI.skin.box);
            {
                var maxversions = StaticMaxVersion;
                foreach (var guid in CharacterData.PrintOrder)
                {
                    var versionData = maxversions[guid];
                    if (!versionData.CharaVisible || !ShowFullyupdated && !ShowForce && !versionData.AnyCharaOutdated)
                    {
                        continue;
                    }
                    GUILayout.BeginHorizontal();
                    {
                        Label($"{versionData.Name}: Ver.{versionData.version} #{versionData.CharaUpToDate + versionData.CharaOutdatedCount}", true);

                        if (versionData.AnyCharaOutdated && Button($"Update #{versionData.CharaOutdatedCount}", "These cards have an outdated version, note if after updating the number remains the same you might have an outdated plugin installed"))
                        {
                            var cardlist = CharacterList.Where(x => UpdateByGuid(x, guid, false)).ToList();
                            StartCoroutine(CharactersCardsUpdate(cardlist, UpdateCoordinates));
                        }
                        if (ShowForce && Button($"Update Missing #{CharacterList.Count - versionData.CharaUpToDate}", $"Update all cards that lack {versionData.Name} data"))
                        {
                            var cardlist = CharacterList.Where(x => UpdateByGuid(x, guid, true)).ToList();
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
                var maxversions = StaticMaxVersion;
                foreach (var guid in OutfitData.PrintOrder)
                {
                    var versionData = maxversions[guid];
                    if (!versionData.OutfitVisible || !ShowFullyupdated && !ShowForce && !versionData.AnyOutfitOutdated)
                    {
                        continue;
                    }
                    GUILayout.BeginHorizontal();
                    {
                        Label($"{versionData.Name}: Ver.{versionData.version} #{versionData.OutfitsUpToDate + versionData.OutfitOutdatedCount}", true);

                        if (versionData.AnyOutfitOutdated && Button($"Update #{versionData.OutfitOutdatedCount}", "These cards have an outdated version, note if after updating the number remains the same you might have an outdated plugin installed"))
                        {
                            var cardlist = OutfitList.Where(x => UpdateByGuid(x, guid, false)).ToList();
                            StartCoroutine(OutfitCardsUpdate(cardlist));
                        }
                        if (ShowForce && Button($"Update Missing #{OutfitList.Count - versionData.OutfitsUpToDate}", $"Update all cards that lack {versionData.Name} data"))
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
                if (Input.GetMouseButtonDown(0) && !mouseassigned && screenRect.Contains(Input.mousePosition))
                {
                    StartCoroutine(DragEvent());
                }

                if (Button("X"))
                {
                    toggle.SetValue(false);
                    ShowGui = false;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                ShowForce = Toggle(ShowForce, "Show Force", "Show extra update options");
                if (Tabvalue == 1 && !UpdateInProgress)
                {
                    UpdateCoordinates = Toggle(UpdateCoordinates, "Coordinates", "Run through each coordinate after loading");
                }


                Pause = Toggle(Pause, "Pause", "Pause before saving");

                if (UpdateInProgress && Waiting && Button("Continue", "Save and Load the next card"))
                {
                    Waiting = false;
                }

                if (Button("Update Outdated/Missing/GUIDS", "Update several numbers", true))
                {
                    UpdateOutmiss();
                }

                if (UpdateInProgress && Button("Force Stop"))
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

        private static bool Toggle(bool toggle, string text, string tooltip = "", bool expandwidth = false)
        {
            return GUILayout.Toggle(toggle, new GUIContent(text, tooltip), togglestyle, GUILayout.ExpandWidth(expandwidth));
        }

        private static bool Button(string text, string tooltip = "", bool expandwidth = false)
        {
            return GUILayout.Button(new GUIContent(text, tooltip), buttonstyle, GUILayout.ExpandWidth(expandwidth));
        }

        private static void Label(string text, bool expandwidth = false)
        {
            GUILayout.Label(text, labelstyle, GUILayout.ExpandWidth(expandwidth));
        }

        private static string Field(string text, bool expandwidth = false)
        {
            return GUILayout.TextField(text, fieldstyle, GUILayout.ExpandWidth(expandwidth));
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

        private static void DrawFontSize()
        {
            if (Button("GUI-", "Decrease GUI Size"))
            {
                SetFontSize(Math.Max(labelstyle.fontSize - 1, 5));
            }
            if (Button("GUI+", "Increase GUI Size"))
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
