using BepInEx;
using static Extensions.OnGUIExtensions;

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
                InitializeStyles();
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
