using BepInEx;

namespace Accessory_States
{
    [BepInPlugin(GUID, "Accessory States", Version)]
    [BepInProcess("KoikatsuSunshineTrial")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class Settings : BaseUnityPlugin
    { }
}
