using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;
using System.Collections.Generic;

#if !KKS
using MoreAccessoriesKOI;
using ToolBox;
#endif

#if Parents
namespace Accessory_Parents
#elif States
namespace Accessory_States
#elif Themes
namespace Accessory_Themes
#elif ACI
namespace Additional_Card_Info
#elif Shortcuts
namespace Accessory_Shortcuts
#endif

{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        internal List<ChaFileAccessory.PartsInfo> Accessorys_Parts = new List<ChaFileAccessory.PartsInfo>();

#if !KKS
        private static readonly WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();

        private MoreAccessories.CharAdditionalData GetMoreAccessoriesData()
        {
            if (MakerAPI.InsideMaker)
            {
                return MoreAccessories._self._charaMakerData;
            }

            if (_accessoriesByChar.TryGetValue(ChaFileControl, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
            }
            return data;
        }
#endif
        internal void Update_More_Accessories()
        {
            Accessorys_Parts.Clear();
            Accessorys_Parts.AddRange(ChaControl.nowCoordinate.accessory.parts);
#if !KKS
            Accessorys_Parts.AddRange(GetMoreAccessoriesData().nowAccessories);
#endif
        }
    }
}
