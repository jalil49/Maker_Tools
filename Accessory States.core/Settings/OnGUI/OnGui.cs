using Accessory_States.Classes.StudioGUI;
using BepInEx;
using Extensions;
using Extensions.GUI_Classes;
using GUIHelper;
using System;
using System.Collections.Generic;
using System.Text;
namespace Accessory_States
{
    public partial class Settings : BaseUnityPlugin
    {
        private static bool _initialized = false;

        #region Maker
        internal static MakerGUI _makerGUI;//main slot window
        internal static StatesGUI _statesGUI;//General NameData window, add, change, remove Namedata and other modifications
        internal static WindowGUI _statesGUI;//General NameData window, add, change, remove Namedata and other modifications
        internal static CharaEvent MakerCharaEvent => KKAPI.Maker.MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
        #endregion

        #region Studio
        internal static StudioGUI _studioGUI;
        #endregion

        public void OnGUI()
        {
            if (!_initialized)
            {
                _initialized = true;
                OnGuiExtensions.InitializeStyles();
            }

            if (_makerGUI != null && _makerGUI.Show)
            {
                var slot = KKAPI.Maker.AccessoriesApi.SelectedMakerAccSlot;
                var parts = MakerCharaEvent.PartsArray;
                if (slot >= parts.Length) return;

                _makerGUI.Draw();
                if (_statesGUI.Show) _statesGUI.Draw();
            }

            if (_studioGUI != null && _studioGUI.Show)
            {
                _studioGUI.Draw();
            }
        }
    }
}
