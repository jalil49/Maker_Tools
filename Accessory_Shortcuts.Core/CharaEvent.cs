using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MoreAccessoriesKOI;
using System.Collections;
using System.Collections.Generic;
using ToolBox;
using UniRx;

namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        List<ChaFileAccessory.PartsInfo> Accessorys_Parts = new List<ChaFileAccessory.PartsInfo>();

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        { }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (!MakerAPI.InsideMaker)
            {
                return;
            }
            CurrentCoordinate.Subscribe(x =>
            {
                StartCoroutine(Wait());
            });
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            StartCoroutine(Wait());
        }

        private void Update_More_Accessories()
        {
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            if (_accessoriesByChar.TryGetValue(ChaFileControl, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
            }
            Accessorys_Parts = data.nowAccessories;
        }

        private IEnumerator Wait()
        {
            yield return null;
            Update_More_Accessories();
            Constants.Default_Dict();
            if (Slots_Location != null)
            {
                Slot_Toggles.Clear();
                UpdateSlots();
            }
        }
    }
}
