using BepInEx;

namespace Accessory_Parents
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    [BepInProcess("KoikatuVR")]
    [BepInProcess("Koikatsu Party VR")]
    public partial class Settings : BaseUnityPlugin
    {
        private void GameUnique()
        {
        }
    }
}