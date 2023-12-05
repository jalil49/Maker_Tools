using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using UniRx;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (!MakerAPI.InsideMaker) return;

            CosplayReady = false;
            var maxkey = MaxKey;
            var coordlength = ChaFileControl.coordinate.Length;

            for (var i = 0; i < coordlength; i++)
                if (Data.CoordinateInfo.ContainsKey(i))
                    Data.ClearOutfit(i);
                else
                    Data.CreateOutfit(i);

            for (int i = coordlength, n = maxkey + 1; i <= n; i++) Data.RemoveOutfit(i);
            CardInfo.Clear();
            CurrentCoordinate.Subscribe(delegate(ChaFileDefine.CoordinateType value)
            {
                var coordinateNum = (int)value;
                if (!CoordinateInfo.ContainsKey(coordinateNum))
                    Data.CreateOutfit(coordinateNum);

                UpdateNowCoordinate();

                StartCoroutine(UpdateSlots());
            });

            var aciData = GetExtendedData();

            if (aciData != null)
            {
                if (aciData.version == 1)
                {
                    if (aciData.data.TryGetValue("CardInfo", out var byteData) && byteData != null)
                        CardInfo = MessagePackSerializer.Deserialize<CardInfo>((byte[])byteData);
                    if (aciData.data.TryGetValue("CoordinateInfo", out byteData) && byteData != null)
                        CoordinateInfo =
                            MessagePackSerializer.Deserialize<Dictionary<int, CoordinateInfo>>((byte[])byteData);
                }
                else if (aciData.version == 0)
                {
                    Migrator.MigrateV0(aciData, ref Data);
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
                var data = GetExtendedData();
                if (data != null) SetExtendedData(data);
                return;
            }

            var aciData = new PluginData
            {
                version = 1
            };
            Data.CleanUp();

            if (!_creatorName.IsNullOrEmpty() && MakerAPI.InsideMaker)
                for (int i = 0, n = ChaFileControl.coordinate.Length; i < n; i++)
                {
                    var creatorlist = CoordinateInfo[i].creatorNames;
                    if (creatorlist.Count == 0 || creatorlist.Last() != _creatorName) creatorlist.Add(_creatorName);
                }

            aciData.data.Add("CardInfo", MessagePackSerializer.Serialize(CardInfo));
            aciData.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(CoordinateInfo));
            SetExtendedData(aciData);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            var aciData = new PluginData
            {
                version = 1
            };
            UpdateClothingNots();
            var creatorlist = CreatorNames;

            if (_creatorName != "" && (creatorlist.Count == 0 || creatorlist.Last() != _creatorName))
                creatorlist.Add(_creatorName);
            aciData.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(NowCoordinateInfo));
            SetCoordinateExtendedData(coordinate, aciData);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            NowCoordinateInfo.Clear();
            NowRestrictionInfo.Clear();
            var aciData = GetCoordinateExtendedData(coordinate);
            if (aciData != null)
                switch (aciData.version)
                {
                    case 0:
                        NowCoordinateInfo = Migrator.CoordinateMigrateV0(aciData);
                        break;
                    case 1:
                        if (aciData.data.TryGetValue("CoordinateInfo", out var byteData) && byteData != null)
                            NowCoordinateInfo = MessagePackSerializer.Deserialize<CoordinateInfo>((byte[])byteData);
                        break;
                    default:
                        Settings.Logger.LogWarning("New version detected please update");
                        break;
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