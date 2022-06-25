using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        // TODO: Implement ASS support
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var insidermaker = currentGameMode == GameMode.Maker;

            GUI_int_state_copy_Dict.Clear();

            var chafile = (currentGameMode == GameMode.Maker) ? MakerAPI.LastLoadedChaFile : ChaFileControl;

            var Extended_Data = GetExtendedData();
            if (Extended_Data != null)
            {
                if (Extended_Data.version < 2)
                {
                    Migration.Migrator.StandardCharaMigrator(ChaControl, Extended_Data);
                }
            }

            CurrentCoordinate.Subscribe(x =>
            {
                GUI_int_state_copy_Dict.Clear();
                UpdatePluginData();
                StartCoroutine(ChangeOutfitCoroutine());
            });
        }

        // TODO: Implement ASS support
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            var States_Data = new PluginData() { version = Constants.SaveVersion };
            SetExtendedData(States_Data);

            if (!Settings.ASS_SAVE.Value || ASSExists)
            {
                return;
            }
        }

        // TODO: Implement ASS support
        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {

            if (!Settings.ASS_SAVE.Value || ASSExists)
            {
                return;
            }
        }

        // TODO: Implement ASS support
        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var Extended_Data = GetCoordinateExtendedData(coordinate);
            if (Extended_Data != null)
            {
                if (Extended_Data.version < 2)
                {
                    Migration.Migrator.StandardCoordMigrator(coordinate, Extended_Data);
                }
            }

            UpdatePluginData();
        }

        //private void SetClothesState_switch(int clothesKind, byte state)
        //{
        //    switch (clothesKind)
        //    {
        //        case 0://top
        //            SetClothesState_switch_Case(ClothNotData[0], ChaControl.notBot != ClothNotData[0], clothesKind, state, 1);//1 is bot
        //            SetClothesState_switch_Case(ClothNotData[1], ChaControl.notBra != ClothNotData[1], clothesKind, state, 2);//2 is bra; line added for clothingunlock
        //            break;
        //        case 1://bot
        //            SetClothesState_switch_Case(ClothNotData[0], ChaControl.notBot != ClothNotData[0], clothesKind, state, 0);//0 is top
        //            break;
        //        case 2://bra
        //            SetClothesState_switch_Case(ClothNotData[2], ChaControl.notShorts != ClothNotData[2], clothesKind, state, 3);//3 is underwear
        //            SetClothesState_switch_Case(ClothNotData[1], ChaControl.notBra != ClothNotData[1], clothesKind, state, 0);//line added for clothingunlock
        //            break;
        //        case 3://underwear
        //            SetClothesState_switch_Case(ClothNotData[2], ChaControl.notShorts != ClothNotData[2], clothesKind, state, 2);
        //            break;
        //        case 7://innershoes
        //            SetClothesState_switch_Case_2(state, 8);
        //            break;
        //        case 8://outershoes
        //            SetClothesState_switch_Case_2(state, 7);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private void SetClothesState_switch_Case(bool condition, bool clothingunlocked, int clothesKind, byte currentstate, int relatedcloth)
        //{
        //    if (condition)
        //    {
        //        var clothesState = ChaControl.fileStatus.clothesState;
        //        byte setrelatedclothset;
        //        var currentvisible = clothesState[clothesKind] != 3;
        //        var relatedvisible = clothesState[relatedcloth] != 3;
        //        if (!currentvisible && relatedvisible)
        //        {
        //            setrelatedclothset = 3;
        //        }
        //        else if (!relatedvisible && currentvisible)
        //        {
        //            setrelatedclothset = currentstate;
        //        }
        //        else
        //        {
        //            return;
        //        }
        //        if (clothingunlocked && !(StopMakerLoop && MakerAPI.InsideMaker))
        //        {
        //            ChaControl.SetClothesState(relatedcloth, setrelatedclothset);
        //        }

        //        ChangedOutfit(relatedcloth, setrelatedclothset);
        //    }
        //}

        //private void SetClothesState_switch_Case_2(byte state, int Clotheskind)
        //{
        //    if (state == 0)
        //    {
        //        if (ChaControl.IsClothesStateKind(Clotheskind))
        //        {
        //            ChangedOutfit(Clotheskind, state);
        //        }
        //    }
        //    else
        //    {
        //        ChangedOutfit(Clotheskind, state);
        //    }
        //}

        private IEnumerator<int> ChangeOutfitCoroutine()
        {
            yield return 0;
        }

        internal void AccessoryCategoryChange(int category, bool show)
        {
            switch (category)
            {
                case 0:
                    ShowMain = show;
                    break;
                case 1:
                    ShowSub = show;
                    break;
                default:
                    Settings.Logger.LogWarning($"Unknown Accessory Category [{category}] set to {show}");
                    break;
            }
        }
        // TODO: do a selective hide
        private void ChangeBindingSub(int hidesetting)
        {
            var coordinateaccessory = ChaFileControl.coordinate[(int)CurrentCoordinate.Value].accessory.parts;
            var nowcoodaccessory = ChaControl.nowCoordinate.accessory.parts;
            var slotslist = SlotBindingData.Where(x => x.Value.bindingDatas.Any(y => y.GetBinding() == Selectedkind)).Select(x => x.Key);
            foreach (var slot in slotslist)
            {
                coordinateaccessory[slot].hideCategory = nowcoodaccessory[slot].hideCategory = hidesetting;
            }
        }

        /// <summary>
        /// Iterrate over whole list to deal with potentially cascading effects of one state affecting another which affects another
        /// </summary>
        public void RefreshSlots()
        {
            foreach (var slotData in SlotBindingData)
            {
                if (slotData.Key >= PartsArray.Length)
                    return;

                var show = ShouldShow(slotData.Value);
                var partsInfo = PartsArray[slotData.Key];
                if (slotData.Value.Parented && ParentedNameDictionary.TryGetValue(partsInfo.parentKey, out var parentingState))
                {
                    show &= parentingState;
                }
                var isSub = partsInfo.hideCategory == 1;
                show &= !isSub && ShowMain || isSub && ShowSub; //respect the hide/show main and sub buttons
                ChaControl.SetAccessoryState(slotData.Key, show);
            }
        }

        /// <summary>
        /// Check bindings of slotdata, respect 
        /// 
        /// </summary>
        /// <param name="slotdata"></param>
        /// <returns></returns>
        public bool ShouldShow(SlotData slotdata)
        {
            var shoeType = ChaControl.fileStatus.shoesType;
            var result = true;
            var priority = 0;

            foreach (var item in slotdata.bindingDatas)
            {
                if (item.NameData == null) continue;

                var stateInfo = item.GetStateInfo(item.NameData.CurrentState, shoeType);
                if (stateInfo == null || stateInfo.Binding < 0 || priority < stateInfo.Priority) continue;


                if (stateInfo.Priority == priority)
                {
                    result &= stateInfo.Show;
                    continue;
                }

                priority = stateInfo.Priority;
                result = stateInfo.Show;
            }
            return result;
        }

        internal void SetClothesState(int clothesKind, byte state)
        {
            var nameData = Names.First(x => x.Binding == clothesKind);
            nameData.CurrentState = state;
            RefreshSlots();
        }
    }
}
