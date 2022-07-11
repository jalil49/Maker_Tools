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
#if Studio
        internal static Studio _studio;
#endif
        #endregion

        public void OnGUI()
        {
            if (!_initialized)
            {
                _initialized = true;
                InitializeStyles();
#if Studio
                if (!KKAPI.Studio.StudioAPI.InsideStudio)//disable OnGui and Update calls when not in maker for main game Initialize styles first, if in studio don't disable
#endif
                {
                    this.enabled = false;
                }
            }

            if (_maker != null)
            {
                if (KKAPI.Maker.MakerAPI.IsInterfaceVisible())
                    _maker.OnGUI();
            }

            if (_studio != null)
            {
                _studio.OnGui();
            }
        }

        internal static void UpdateGUI(CharaEvent charaEvent)
        {
            if (_maker != null)
                _maker.ClearCoordinate();

            if (_studio != null)
                _studio.ClearCoordinate(charaEvent);
        }

        internal void Update()
        {
            if (SlotWindowHotKey.Value.IsDown())
            {
                if (_maker != null)
                    _maker.ToggleSlotWindow();
#if Studio
                if (_studio != null)
                    _studio.ToggleSlotWindow();
#endif
            }

            if (PreviewWindowHotKey.Value.IsDown())
            {
                if (_maker != null)
                    _maker.TogglePreviewWindow();
#if Studio
                if (_studio != null)
                    _studio.TogglePreviewWindow();
#endif
            }
        }
    }
}

