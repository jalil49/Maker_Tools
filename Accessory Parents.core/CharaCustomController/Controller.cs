using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
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
            for (int i = 0; i < ChaFileControl.coordinate.Length; i++)
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
            PluginData data = new PluginData() { version = 1 };
            Current_Parent_Data.CleanUp();
            data.data.Add("Coordinate_Data", MessagePackSerializer.Serialize(Current_Parent_Data));
            SetCoordinateExtendedData(coordinate, data);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            Update_Old_Parents();
            PluginData data = new PluginData() { version = 1 };
            foreach (var item in Parent_Data)
            {
                item.Value.CleanUp();
            }
            data.data.Add("Coordinate_Data", MessagePackSerializer.Serialize(Parent_Data));
            SetExtendedData(data);
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
