using BepInEx;

namespace Accessory_States
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    [BepInProcess("KoikatuVR")]
    [BepInProcess("Koikatsu Party VR")]
    [BepInProcess("CharaStudio")]
    public partial class Settings : BaseUnityPlugin
    {
        private void GameUnique() { }
    }
}
