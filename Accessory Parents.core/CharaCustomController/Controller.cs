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
            if (currentGameMode != GameMode.Maker)
            {
                return;
            }
            for (var i = 0; i < ChaFileControl.coordinate.Length; i++)
            {
                if (Parent_Data.ContainsKey(i))
                    Clearoutfit(i);
                else
                    Createoutfit(i);
            }
            for (int i = ChaFileControl.coordinate.Length, n = Parent_Data.Keys.Max() + 1; i < n; i++)
            {
                Removeoutfit(i);
            }
            CurrentCoordinate.Subscribe(X =>
            {
                ShowCustomGui = false;
                var CoordinateNum = (int)X;
                if (!Parent_Data.ContainsKey(CoordinateNum))
                {
                    Createoutfit(CoordinateNum);
                };
                UpdateNowCoordinate();
                Update_Drop_boxes();
            });
            Current_Parent_Data.Clear();
            var Data = GetExtendedData();
            if (Data != null)
            {
                if (Data.version == 1)
                {
                    if (Data.data.TryGetValue("Coordinate_Data", out var ByteData) && ByteData != null)
                    {
                        Parent_Data = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateData>>((byte[])ByteData);
                    }
                }
                else if (Data.version == 0)
                {
                    Migrator.MigrateV0(Data, ref Parent_Data);
                }
                else
                {
                    Settings.Logger.LogWarning("New version of plugin detected please update");
                }
                for (int outfitnum = 0, n = ChaFileControl.coordinate.Length; outfitnum < n; outfitnum++)
                {
                    UpdateRelations(outfitnum);
                }
            }
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (!MakerAPI.InsideMaker)
            {
                return;
            }

            Current_Parent_Data.Clear();

            var Data = GetCoordinateExtendedData(coordinate);
            if (Data != null)
            {
                if (Data.version == 1)
                {
                    if (Data.data.TryGetValue("Coordinate_Data", out var DataBytes) && DataBytes != null)
                    {
                        Current_Parent_Data = MessagePackSerializer.Deserialize<CoordinateData>((byte[])DataBytes);
                    }
                }
                else if (Data.version == 0)
                {
                    Current_Parent_Data = Migrator.CoordinateMigrateV0(Data);
                }
                else
                {
                    Settings.Logger.LogWarning("New version of plugin detected please update");
                }
            }
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                var CoordinateNum = (int)CurrentCoordinate.Value;
                Current_Parent_Data = Parent_Data[CoordinateNum];
                UpdateRelations(CoordinateNum);
            }
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            var data = new PluginData() { version = 1 };
            Current_Parent_Data.CleanUp();
            var nulldata = Parent_Groups.Count == 0;
            data.data.Add("Coordinate_Data", MessagePackSerializer.Serialize(Current_Parent_Data));
            SetCoordinateExtendedData(coordinate, (nulldata) ? null : data);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (!MakerAPI.InsideMaker)
            {
                var Data = GetExtendedData();
                if (Data != null)
                {
                    SetExtendedData(Data);
                }
                return;
            }

            Update_Old_Parents();
            var data = new PluginData() { version = 1 };
            foreach (var item in Parent_Data)
            {
                item.Value.CleanUp();
            }
            var nulldata = Parent_Data.All(x => x.Value.Parent_Groups.Count == 0);

            data.data.Add("Coordinate_Data", MessagePackSerializer.Serialize(Parent_Data));
            SetExtendedData((nulldata) ? null : data);
        }

        private void UpdateNowCoordinate()
        {
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                Current_Parent_Data = Parent_Data[(int)CurrentCoordinate.Value];
                return;
            }
            Current_Parent_Data = new CoordinateData(Parent_Data[(int)CurrentCoordinate.Value]);
        }

        private void Clearoutfit(int key)
        {
            if (!Parent_Data.ContainsKey(key))
                Createoutfit(key);
            Parent_Data[key].Clear();
        }

        private void Createoutfit(int key)
        {
            if (!Parent_Data.ContainsKey(key))
                Parent_Data[key] = new CoordinateData();
        }

        private void Moveoutfit(int dest, int src)
        {
            Parent_Data[dest].CopyData(Parent_Data[src]);
        }

        private void Removeoutfit(int key)
        {
            Parent_Data.Remove(key);
        }
    }
}
