using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var ACI_Data = GetExtendedData();

            if (ACI_Data != null)
            {
                if (ACI_Data.version <= Constants.MasterSaveVersion)
                {
                    CardInfo = Migrator.StandardCharaMigrate(ChaControl, ACI_Data);
                }
                else
                {
                    CardInfo.Clear();
                }
            }

            CurrentCoordinate.Subscribe(delegate (ChaFileDefine.CoordinateType value)
            {
                UpdatePluginData();

                UpdateSlots();
            });

            LoadCard();
            UpdatePluginData();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            SetExtendedData(CardInfo.Serialize());
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate) => UpdateClothingNots();

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            NowCoordinateInfo.Clear();
            var ACI_Data = GetCoordinateExtendedData(coordinate);
            if (ACI_Data != null)
            {
                Migrator.StandardCoordinateMigrate(coordinate, ACI_Data);
            }
        }

        private void UpdatePluginData()
        {
            LoadCoordinate();
            SlotInfo.Clear();
            for (var i = 0; i < Parts.Length; i++)
            {
                LoadSlot(i);
            }
            UpdateSlots();
        }
    }
}
