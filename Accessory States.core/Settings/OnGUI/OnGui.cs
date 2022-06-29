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
        private bool _initialized = false;

        #region Maker
        internal static Maker _maker;
        #endregion

        #region Studio
        internal static Studio _studio;
        #endregion

        public void OnGUI()
        {
            if (!_initialized)
            {
                _initialized = true;
                OnGuiExtensions.InitializeStyles();
            }


            if (_maker != null)
            {
                if (KKAPI.Maker.MakerAPI.IsInterfaceVisible())
                    _maker.OnGUI();
            }

            if (_studio != null)
            {
                _studio.OnGUI();
            }
        }

        internal static void UpdateGUI(CharaEvent charaEvent)
        {
            if (_maker != null)
                _maker.ClearCoordinate();
        }
    }
}
