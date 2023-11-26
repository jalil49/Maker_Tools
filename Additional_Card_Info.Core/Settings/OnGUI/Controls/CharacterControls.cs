using System.Collections.Generic;
using Extensions.GUI_Classes;

namespace Additional_Card_Info.Controls
{
    public class CharacterControls
    {
        private static readonly Dictionary<string, string> AdvancedCharacterOutfitFolders =
            new Dictionary<string, string>();

        private readonly Dictionary<int, ToggleGUI> _personalKeepToggles = new Dictionary<int, ToggleGUI>();

        private ToggleGUI _advanced;
        private ToggleGUI _characterCosplayReady;
    }
}