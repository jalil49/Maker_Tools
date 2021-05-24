using BepInEx.Logging;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MoreAccessoriesKOI;
using System.Collections.Generic;
using ToolBox;
using UniRx;

namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        List<ChaFileAccessory.PartsInfo> Accessorys_Parts = new List<ChaFileAccessory.PartsInfo>();
        public CharaEvent()
        {
            MakerAPI.MakerFinishedLoading += MakerAPI_MakerFinishedLoading;
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;
        }

        protected override void OnDestroy()
        {
            MakerAPI.MakerFinishedLoading -= MakerAPI_MakerFinishedLoading;
            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;
            base.OnDestroy();
        }

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
                Update_More_Accessories();
                Constants.Default_Dict();
                if (Slots_Location != null)
                {
                    Slot_Toggles.Clear();
                    UpdateSlots();
                }
            });
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            Update_More_Accessories(); Constants.Default_Dict();
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
    }
}
