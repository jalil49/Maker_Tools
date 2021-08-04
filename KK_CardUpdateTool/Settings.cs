using BepInEx;

namespace CardUpdateTool
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    [BepInProcess("KoikatuVR")]
    [BepInProcess("Koikatsu Party VR")]
    public partial class Settings : BaseUnityPlugin
    {
        private void EventRegister()
        {

        }
    }
}
