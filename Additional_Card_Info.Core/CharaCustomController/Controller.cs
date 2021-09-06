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
            if (!MakerAPI.InsideMaker)
            {
                return;
            }

            CosplayReady = false;
            var maxkey = MaxKey;
            var coordlength = ChaFileControl.coordinate.Length;

            for (var i = 0; i < coordlength; i++)
            {
                if (data.CoordinateInfo.ContainsKey(i))
                    data.Clearoutfit(i);
                else
                    data.Createoutfit(i);
            }

            for (int i = coordlength, n = maxkey + 1; i <= n; i++)
            {
                data.Removeoutfit(i);
            }
            CardInfo.Clear();
            CurrentCoordinate.Subscribe(delegate (ChaFileDefine.CoordinateType value)
            {
                var CoordinateNum = (int)value;
                if (!CoordinateInfo.ContainsKey(CoordinateNum))
                    data.Createoutfit(CoordinateNum);

                UpdateNowCoordinate();

                StartCoroutine(UpdateSlots());
            });

            var ACI_Data = GetExtendedData();

            if (ACI_Data != null)
            {
                if (ACI_Data.version == 1)
                {
                    if (ACI_Data.data.TryGetValue("CardInfo", out var ByteData) && ByteData != null)
                    {
                        CardInfo = MessagePackSerializer.Deserialize<Cardinfo>((byte[])ByteData);
                    }
                    if (ACI_Data.data.TryGetValue("CoordinateInfo", out ByteData) && ByteData != null)
                    {
                        CoordinateInfo = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateInfo>>((byte[])ByteData);
                    }
                }
                else if (ACI_Data.version == 0)
                {
                    Migrator.MigrateV0(ACI_Data, ref data);
                }
                else
                {
                    Settings.Logger.LogWarning("New plugin version found on card please update");
                }
            }

            UpdateNowCoordinate();
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

            var ACI_Data = new PluginData
            {
                version = 1
            };
            data.CleanUp();

            if (!Creatorname.IsNullOrEmpty() && MakerAPI.InsideMaker)
            {
                for (int i = 0, n = ChaFileControl.coordinate.Length; i < n; i++)
                {
                    var creatorlist = CoordinateInfo[i].CreatorNames;
                    if (creatorlist.Count == 0 || creatorlist.Last() != Creatorname)
                    {
                        creatorlist.Add(Creatorname);
                    }
                }
            }

            ACI_Data.data.Add("CardInfo", MessagePackSerializer.Serialize(CardInfo));
            ACI_Data.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(CoordinateInfo));
            SetExtendedData(ACI_Data);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            var ACI_Data = new PluginData
            {
                version = 1
            };
            UpdateClothingNots();
            var creatorlist = CreatorNames;

            if (Creatorname != "" && (creatorlist.Count == 0 || creatorlist.Last() != Creatorname))
            {
                creatorlist.Add(Creatorname);
            }
            ACI_Data.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(NowCoordinateInfo));
            SetCoordinateExtendedData(coordinate, ACI_Data);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            NowCoordinateInfo.Clear();
            NowRestrictionInfo.Clear();
            var ACI_Data = GetCoordinateExtendedData(coordinate);
            if (ACI_Data != null)
            {
                switch (ACI_Data.version)
                {
                    case 0:
                        NowCoordinateInfo = Migrator.CoordinateMigrateV0(ACI_Data);
                        break;
                    case 1:
                        if (ACI_Data.data.TryGetValue("CoordinateInfo", out var ByteData) && ByteData != null)
                        {
                            NowCoordinateInfo = MessagePackSerializer.Deserialize<CoordinateInfo>((byte[])ByteData);
                        }
                        break;
                    default:
                        Settings.Logger.LogWarning("New version detected please update");
                        break;
                }
            }
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                CoordinateInfo[(int)CurrentCoordinate.Value] = NowCoordinateInfo;
                StartCoroutine(UpdateSlots());
            }
        }

        private void UpdateNowCoordinate()
        {
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                NowCoordinateInfo = CoordinateInfo[(int)CurrentCoordinate.Value];
                return;
            }
            NowCoordinateInfo = new CoordinateInfo(CoordinateInfo[(int)CurrentCoordinate.Value]);
        }
    }
}
