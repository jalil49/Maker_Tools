using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;

namespace Accessory_States
{
    partial class CharaEvent : CharaCustomFunctionController
    {
        private Traverse ASS_Traverse;

        private bool ASS_Setup()
        {
            if (!ASS_Exists)
                return false;

            if (ASS_Traverse != null)
            {
                return true;
            }
            ASS_Traverse = Traverse.Create(ChaControl.GetComponent("AccStateSync.AccStateSync+AccStateSyncController, KK_AccStateSync"));
            return true;
        }

        private void DeleteGroup(int _kind)
        {
            if (!ASS_Setup())
            {
                return;
            }
            ASS_Traverse.Method("RemoveTriggerGroup", new object[] { (int)CurrentCoordinate.Value, _kind }).GetValue();
            RefreshCache();
        }

        private void RenameGroup(int _kind, string _label)
        {
            if (!ASS_Setup())
            {
                return;
            }

            ASS_Traverse.Method("RenameTriggerGroup", new object[] { _kind, _label }).GetValue();
            RefreshCache();
        }

        private void AddGroup(int _kind, string _label)
        {
            if (!ASS_Setup())
            {
                return;
            }
            Settings.Logger.LogWarning("adding group " + _label);
            DeleteGroup(_kind);
            ASS_Traverse.Method("RemoveTriggerGroupNewOrGetTriggerGroup", new object[] { (int)CurrentCoordinate.Value, _kind }).GetValue();
            RenameGroup(_kind, _label);
            RefreshCache();
        }

        private void RemoveTriggerSlot()
        {
            var _slot = AccessoriesApi.SelectedMakerAccSlot;
            ASS_Traverse.Method("RemoveSlotTriggerProperty", new object[] { (int)CurrentCoordinate.Value, _slot }).GetValue();
            RefreshCache();
        }

        private void RemoveTriggerByKind(int _refKind)
        {
            ASS_Traverse.Method("RemoveSlotTriggerProperty", new object[] { (int)CurrentCoordinate.Value, _refKind }).GetValue();
            RefreshCache();
        }

        private void ChangeTriggerProperty(int _refKind)
        {
            var _slot = AccessoriesApi.SelectedMakerAccSlot;

            var list = Slotinfo[_slot].States;
            var _coord = (int)CurrentCoordinate.Value;
            var single = 3 < _refKind && _refKind < 9;
            for (int _refState = 0, n = MaxState(_refKind) + 1; _refState < n; _refState++)
            {
                var test = ASS_Traverse.Method("NewOrGetTriggerProperty", new object[] { _coord, _slot, _refKind, _refState }).GetValue();
                Traverse.Create(test).Property("Visible").SetValue(ShowState(_refState, list));
            }
            RefreshCache();
        }

        private void RefreshCache()
        {
            ASS_Traverse.Method("RefreshCache").GetValue();
        }
    }
}
