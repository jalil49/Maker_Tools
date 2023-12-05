using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;

namespace Accessory_States
{
    partial class CharaEvent : CharaCustomFunctionController
    {
        private Traverse _assTraverse;

        private bool ASS_Setup()
        {
            if (!AssExists)
                return false;

            if (_assTraverse != null) return true;
            _assTraverse =
                Traverse.Create(
                    ChaControl.GetComponent("AccStateSync.AccStateSync+AccStateSyncController, KK_AccStateSync"));
            return true;
        }

        private void DeleteGroup(int kind)
        {
            if (!ASS_Setup()) return;
            _assTraverse.Method("RemoveTriggerGroup", (int)CurrentCoordinate.Value, kind).GetValue();
            RefreshCache();
        }

        private void RenameGroup(int kind, string label)
        {
            if (!ASS_Setup()) return;

            _assTraverse.Method("RenameTriggerGroup", kind, label).GetValue();
            RefreshCache();
        }

        private void AddGroup(int kind, string label)
        {
            if (!ASS_Setup()) return;
            Settings.Logger.LogWarning("adding group " + label);
            DeleteGroup(kind);
            _assTraverse.Method("RemoveTriggerGroupNewOrGetTriggerGroup", (int)CurrentCoordinate.Value, kind)
                .GetValue();
            RenameGroup(kind, label);
            RefreshCache();
        }

        private void RemoveTriggerSlot()
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            _assTraverse.Method("RemoveSlotTriggerProperty", (int)CurrentCoordinate.Value, slot).GetValue();
            RefreshCache();
        }

        private void RemoveTriggerByKind(int refKind)
        {
            _assTraverse.Method("RemoveSlotTriggerProperty", (int)CurrentCoordinate.Value, refKind).GetValue();
            RefreshCache();
        }

        private void ChangeTriggerProperty(int refKind)
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;

            var list = SlotInfo[slot].States;
            var coord = (int)CurrentCoordinate.Value;
            var single = 3 < refKind && refKind < 9;
            for (int refState = 0, n = MaxState(refKind) + 1; refState < n; refState++)
            {
                var test = _assTraverse.Method("NewOrGetTriggerProperty", coord, slot, refKind, refState).GetValue();
                Traverse.Create(test).Property("Visible").SetValue(ShowState(refState, list));
            }

            RefreshCache();
        }

        private void RefreshCache()
        {
            _assTraverse.Method("RefreshCache").GetValue();
        }
    }
}