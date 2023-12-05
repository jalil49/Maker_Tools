using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using UniRx;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (currentGameMode != GameMode.Maker) return;
            for (var i = 0; i < ChaFileControl.coordinate.Length; i++)
                if (_parentData.ContainsKey(i))
                    Clearoutfit(i);
                else
                    Createoutfit(i);
            for (int i = ChaFileControl.coordinate.Length, n = _parentData.Keys.Max() + 1; i < n; i++) Removeoutfit(i);
            CurrentCoordinate.Subscribe(x =>
            {
                _showCustomGui = false;
                var coordinateNum = (int)x;
                if (!_parentData.ContainsKey(coordinateNum)) Createoutfit(coordinateNum);
                UpdateNowCoordinate();
                Update_Drop_boxes();
            });
            _currentParentData.Clear();
            var data = GetExtendedData();
            if (data != null)
            {
                if (data.version == 1)
                {
                    if (data.data.TryGetValue("Coordinate_Data", out var byteData) && byteData != null)
                        _parentData =
                            MessagePackSerializer.Deserialize<Dictionary<int, CoordinateData>>((byte[])byteData);
                }
                else if (data.version == 0)
                {
                    Migrator.MigrateV0(data, ref _parentData);
                }
                else
                {
                    Settings.Logger.LogWarning("New version of plugin detected please update");
                }

                for (int outfitNum = 0, n = ChaFileControl.coordinate.Length; outfitNum < n; outfitNum++)
                    UpdateRelations(outfitNum);
            }
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (!MakerAPI.InsideMaker) return;

            _currentParentData.Clear();

            var data = GetCoordinateExtendedData(coordinate);
            if (data != null)
            {
                if (data.version == 1)
                {
                    if (data.data.TryGetValue("Coordinate_Data", out var dataBytes) && dataBytes != null)
                        _currentParentData = MessagePackSerializer.Deserialize<CoordinateData>((byte[])dataBytes);
                }
                else if (data.version == 0)
                {
                    _currentParentData = Migrator.CoordinateMigrateV0(data);
                }
                else
                {
                    Settings.Logger.LogWarning("New version of plugin detected please update");
                }
            }

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                var coordinateNum = (int)CurrentCoordinate.Value;
                _currentParentData = _parentData[coordinateNum];
                UpdateRelations(coordinateNum);
            }
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            var data = new PluginData { version = 1 };
            _currentParentData.CleanUp();
            var nulldata = ParentGroups.Count == 0;
            data.data.Add("Coordinate_Data", MessagePackSerializer.Serialize(_currentParentData));
            SetCoordinateExtendedData(coordinate, nulldata ? null : data);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (!MakerAPI.InsideMaker)
            {
                var data = GetExtendedData();
                if (data != null) SetExtendedData(data);
                return;
            }

            Update_Old_Parents();
            var pluginData = new PluginData { version = 1 };
            foreach (var item in _parentData) item.Value.CleanUp();
            var nullData = _parentData.All(x => x.Value.parentGroups.Count == 0);

            pluginData.data.Add("Coordinate_Data", MessagePackSerializer.Serialize(_parentData));
            SetExtendedData(nullData ? null : pluginData);
        }

        private void UpdateNowCoordinate()
        {
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                _currentParentData = _parentData[(int)CurrentCoordinate.Value];
                return;
            }

            _currentParentData = new CoordinateData(_parentData[(int)CurrentCoordinate.Value]);
        }

        private void Clearoutfit(int key)
        {
            if (!_parentData.ContainsKey(key))
                Createoutfit(key);
            _parentData[key].Clear();
        }

        private void Createoutfit(int key)
        {
            if (!_parentData.ContainsKey(key))
                _parentData[key] = new CoordinateData();
        }

        private void Moveoutfit(int dest, int src)
        {
            _parentData[dest].CopyData(_parentData[src]);
        }

        private void Removeoutfit(int key)
        {
            _parentData.Remove(key);
        }
    }
}