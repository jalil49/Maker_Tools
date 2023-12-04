using Additional_Card_Info.Classes.Migration;
using KKAPI;
using KKAPI.Chara;
using UniRx;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var aciData = GetExtendedData();

            if (aciData != null)
            {
                if (aciData.version <= Constants.MasterSaveVersion)
                {
                    cardData = Migrator.StandardCharaMigrate(ChaControl, aciData);
                }
                else
                {
                    cardData.Clear();
                }
            }

            CurrentCoordinate.Subscribe(delegate { UpdatePluginData(); });

            LoadCard();
            UpdatePluginData();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            SetExtendedData(cardData.Serialize());
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate) { }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            nowCoordinateInfo.Clear();
            var aciData = GetCoordinateExtendedData(coordinate);
            if (aciData != null)
            {
                Migrator.StandardCoordinateMigrate(coordinate, aciData);
            }
        }

        internal void UpdatePluginData()
        {
            LoadCoordinate();
            SlotData.Clear();
            for (var i = 0; i < Parts.Length; i++)
            {
                LoadSlot(i);
            }

            UpdateSlots();
        }

        public void Slot_ACC_Change(int slotNo, int type)
        {
            throw new System.NotImplementedException();
        }
    }
}