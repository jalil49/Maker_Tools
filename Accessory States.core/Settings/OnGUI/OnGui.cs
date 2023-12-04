using KKAPI.Maker;
using KKAPI.Studio;
using static Extensions.OnGUIExtensions;

namespace Accessory_States
{
    public partial class Settings
    {
        private bool _initialized;

        internal void Update()
        {
            if (SlotWindowHotKey.Value.IsDown())
            {
                MakerGUI.Instance?.ToggleSlotWindow();
#if Studio
                StudioGUI.Instance?.ToggleSlotWindow();
#endif
            }

            if (PreviewWindowHotKey.Value.IsDown())
            {
                MakerGUI.Instance?.TogglePreviewWindow();
#if Studio
                StudioGUI.Instance?.TogglePreviewWindow();
#endif
            }
        }

        public void OnGUI()
        {
            if (!_initialized)
            {
                _initialized = true;
                InitializeStyles();
#if Studio
                //disable OnGui and Update calls when not in maker for main game Initialize styles first, if in studio don't disable
                if (!StudioAPI.InsideStudio)
#endif
                    enabled = false;
            }

            if (MakerAPI.IsInterfaceVisible()) MakerGUI.Instance?.OnGUI();

            StudioGUI.Instance?.OnGUI();
        }
    }
}