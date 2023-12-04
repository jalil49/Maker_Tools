using System.Collections.Generic;
using Extensions.GUI_Classes;
using static Additional_Card_Info.Constants;

namespace Additional_Card_Info.Controls
{
    public class OutfitControls
    {
        private readonly Dictionary<int, ToggleGUI<Breastsize>> _breastSizeToggles = new Dictionary<int, ToggleGUI<Breastsize>>();

        private readonly Dictionary<int, ToggleGUI<KeepState>> _coordinateKeepToggles = new Dictionary<int, ToggleGUI<KeepState>>();
        private readonly Dictionary<int, ToggleGUI<Height>> _heightToggles = new Dictionary<int, ToggleGUI<Height>>();

        private readonly Dictionary<int, ToolbarGUI> _interestToggles = new Dictionary<int, ToolbarGUI>();
        private readonly Dictionary<int, ToggleGUI<Personality>> _personalityToggles = new Dictionary<int, ToggleGUI<Personality>>();

        private Dictionary<int, ToolbarGUI> _traitToggles = new Dictionary<int, ToolbarGUI>();

        public void Setup()
        {
            for (var i = 0; i < BreastsizeLength; i++)
            {
                _breastSizeToggles[i] = new ToggleGUI<Breastsize>();
            }
        }
    }
}