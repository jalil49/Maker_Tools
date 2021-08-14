using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using System.Collections;
using UniRx;

namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        { }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (!MakerAPI.InsideMaker)
            {
                return;
            }
            StartCoroutine(Wait());
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
            if (Slot_Toggles.Count > 0 && !(Accessorys_Parts.Count < AccessoriesApi.SelectedMakerAccSlot))
                Slot_Toggles[Slot_Toggles.Count - 1].isOn = true;
        }
    }
}
