using KKAPI;
using KKAPI.Chara;
using UniRx;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var data = GetExtendedData();
            if (data != null)
            {
                if (data.version <= 2)
                    Migrator.StandardCharaMigrator(ChaControl, data);
                else
                    Settings.Logger.LogWarning("New version of plugin detected please update");
            }

            CurrentCoordinate.Subscribe(coordinateType =>
            {
                _showCustomGui = false;
                UpdateNowCoordinate();
                Update_Drop_boxes();
            });
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var data = GetCoordinateExtendedData(coordinate);
            if (data != null)
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