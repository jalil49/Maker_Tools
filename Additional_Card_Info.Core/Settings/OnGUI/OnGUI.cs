using static Extensions.OnGUIExtensions;
namespace Additional_Card_Info
{
    public partial class Settings
    {
        private bool _intitalized;
        internal void Update()
        {

        }

        internal void OnGUI()
        {
            if (_intitalized)
            {
                InitializeStyles();
                this.enabled = false;
                _intitalized = true;
            }
        }
    }
}
