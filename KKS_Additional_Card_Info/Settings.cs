using BepInEx;
using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Additional_Card_Info
{
    [BepInProcess("KoikatsuSunshineTrial")]
    public partial class Settings : BaseUnityPlugin
    {
        private void EventRegister()
        {
            ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported;
        }
        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData)
        {
            importedExtendedData.TryGetValue(GUID, out var ACI_Data);
            if (ACI_Data != null)
            {
                Logger.LogDebug($"ACI Data version {ACI_Data.version} Found on import");

                var data = new DataStruct();

                if (ACI_Data.version == 1)
                {
                }
                else if (ACI_Data.version == 0)
                {

                    importedExtendedData[GUID] = ACI_Data;
                }
                else
                {
                    Logger.LogWarning("New version of plugin detected on import please update");
                }
            }
        }
    }
}
