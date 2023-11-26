using System.Collections.Generic;
using Extensions.GUI_Classes;
using static Additional_Card_Info.Constants;

namespace Additional_Card_Info.Controls
{
    public class OutfitControls
    {
        private readonly Dictionary<int, ToggleGUI> _breastSizeToggles = new Dictionary<int, ToggleGUI>();

        private readonly Dictionary<int, ToggleGUI> _coordinateKeepToggles = new Dictionary<int, ToggleGUI>();
        private readonly Dictionary<int, ToggleGUI> _heightToggles = new Dictionary<int, ToggleGUI>();

        private readonly Dictionary<int, ToolbarGUI> _interestToggles = new Dictionary<int, ToolbarGUI>();
        private readonly Dictionary<int, ToggleGUI> _personalityToggles = new Dictionary<int, ToggleGUI>();

        private Dictionary<int, ToolbarGUI> _traitToggles = new Dictionary<int, ToolbarGUI>();

        public void Setup()
        {
            for (var i = 0; i < BreastsizeLength; i++)
            {
                _breastSizeToggles[i] = new ToggleGUI();
            }
        }
    }
}