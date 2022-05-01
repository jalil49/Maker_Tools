using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var Data = GetExtendedData();
            if (Data != null)
            {
                if (Data.version <= 2)
                {
                    Migrator.StandardCharaMigrator(ChaControl, Data);
                }
                else
                {
                    Settings.Logger.LogWarning("New version of plugin detected please update");
                }
            }

            CurrentCoordinate.Subscribe(X =>
            {
                ShowCustomGui = false;
                UpdateNowCoordinate();
                Update_Drop_boxes();
            });
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var Data = GetCoordinateExtendedData(coordinate);
            if (Data != null)
            {
            }
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {

        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            Update_Old_Parents();
        }

        private void UpdateNowCoordinate()
        {

        }
    }
}
