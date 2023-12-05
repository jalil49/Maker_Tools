using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Illusion.Game;
using KKAPI.MainGame;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if KK
using Heroine = SaveData.Heroine;
#elif KKS
using SaveData;
#endif

namespace Accessory_States
{
    public class GameEvent : GameCustomFunctionController
    {
        private readonly Dictionary<int, List<int>> _buttonList = new Dictionary<int, List<int>>();
        private List<Heroine> _heroines;
        private HSprite[] _hSprites;
#if KK
        protected override void OnStartH(BaseLoader proc, HFlag hFlag, bool vr)
#else
        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
#endif
        {
            if (vr)
                _hSprites = Traverse.Create(proc).Field("sprites").GetValue<HSprite[]>();
            else
                _hSprites = new[] { ((HSceneProc)proc).sprite };
            Hooks.HCoordinateChange += HooksHCoordinateChange;
            CharaEvent.CoordinateLoaded += CharaEventCoordinateLoaded;
            _heroines = hFlag.lstHeroine;
            for (var i = 0; i < _heroines.Count; i++)
            {
                var controller = _heroines[i].chaCtrl.GetComponent<CharaEvent>();
                controller.UpdateNowCoordinate();
                controller.Refresh();
                ButtonLogic(i, false);
            }

            base.OnStartH(proc, hFlag, vr);
        }

        private void CharaEventCoordinateLoaded(object sender, CoordinateLoadedEventArg e)
        {
            if (_heroines == null) return;
            for (var i = 0; i < _heroines.Count; i++)
                if (_heroines[i].chaCtrl.name == e.Character.name)
                {
                    ButtonLogic(i, true);
                    return;
                }
        }
#if KK
        protected override void OnEndH(BaseLoader proc, HFlag hFlag, bool vr)
#else
        protected override void OnEndH(MonoBehaviour proc, HFlag hFlag, bool vr)
#endif
        {
            Hooks.HCoordinateChange -= HooksHCoordinateChange;
            _buttonList.Clear();
            _heroines = null;
            _hSprites = null;
            if (hFlag.isFreeH) CharaEvent.FreeHHeroines = new List<Heroine>();
            base.OnEndH(proc, hFlag, vr);
        }

        private void HooksHCoordinateChange(object sender, OnClickCoordinateChange e)
        {
            ButtonLogic(e.Female, false, e.Coordinate);
        }

        private void ButtonLogic(int female, bool coordLoaded, int coordChange = -1)
        {
            if (_hSprites == null) return;
            var harem = _heroines.Count > 1;
            var heroineCtrl = _heroines[female].chaCtrl;
            var controller = heroineCtrl.GetComponent<CharaEvent>();
            if (!_buttonList.TryGetValue(female, out var list)) list = new List<int>();
            list.Reverse();
            //Settings.Logger.LogWarning("Enter delete");
            foreach (var item in list) DeleteButton(female, harem, item);
            //Settings.Logger.LogWarning("clear");

            _buttonList.Remove(female);

            if (!coordLoaded && coordChange == -1) controller.UpdateNowCoordinate();
            if (coordChange > -1) controller.UpdateNowCoordinate(coordChange);
            var names = controller.nowCoordinate.Names;
            var slotInfo = controller.nowCoordinate.SlotInfo;
            //Settings.Logger.LogWarning("create");
            var shoeType = heroineCtrl.fileStatus.shoesType;
            foreach (var item in names)
                if (slotInfo.Count(x =>
                        x.Value.Binding == item.Key && (x.Value.ShoeType == shoeType || x.Value.ShoeType == 2)) > 0)
                    CreateButton(female, harem, item.Value.Name, item.Key, controller, 0);

            foreach (var item in controller.NowParentedNameDictionary)
                CreateButton(female, harem, item.Key, 0, controller, 1);
        }

        private void DeleteButton(int female, bool harem, int remove)
        {
            //Settings.Logger.LogWarning("deleting button");
            foreach (var sprite in _hSprites)
            {
                var hSceneSpriteCategory = harem ? sprite.lstMultipleFemaleDressButton[female].accessoryAll : sprite.categoryAccessoryAll;
                var toRemove = hSceneSpriteCategory.lstButton[remove];
                Destroy(toRemove.gameObject);
                hSceneSpriteCategory.lstButton.RemoveAt(remove);
            }
        }

        private void CreateButton(int female, bool harem, string buttonName, int kind, CharaEvent controller,
            int buttonKind)
        {
            foreach (var sprite in _hSprites)
            {
                var hSceneSpriteCategory = harem ? sprite.lstMultipleFemaleDressButton[female].accessoryAll : sprite.categoryAccessoryAll;
                var parent = hSceneSpriteCategory.transform;

                var origin = sprite.categoryAccessory.lstButton[0].transform;
                var copy = Instantiate(origin.transform, parent, false);
                copy.name = $"btn_{buttonName}_{kind}";
                copy.GetComponentInChildren<TextMeshProUGUI>().text = buttonName;
                var trans = copy.GetComponent<RectTransform>();
                trans.sizeDelta = new Vector2(115, trans.sizeDelta.y);

                var button = copy.GetComponentInChildren<Button>();
                button.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
                button.onClick.RemoveAllListeners();
                button.onClick = new Button.ButtonClickedEvent();
                button.image.raycastTarget = true;
                if (buttonKind == 0)
                {
                    var state = 0;
                    var binded = controller.nowCoordinate.SlotInfo.Where(x => x.Value.Binding == kind);
                    var final = 0;
                    foreach (var item in binded)
                    foreach (var item2 in item.Value.States)
                        final = Mathf.Max(final, item2[1]);
                    final += 2;
                    button.onClick.AddListener(delegate
                    {
                        state = (state + 1) % final;
                        //Settings.Logger.LogWarning($"name:{name}, kind: {kind}, State: {state}");
                        controller.Custom_Groups(kind, state);
                        Utils.Sound.Play(SystemSE.sel);
                    });
                }
                else
                {
                    var state = true;
                    button.onClick.AddListener(delegate
                    {
                        state = !state;
                        //Settings.Logger.LogWarning($"Setting {name} show to {state} state");
                        controller.Parent_toggle(buttonName, state);
                        Utils.Sound.Play(SystemSE.sel);
                    });
                }

                if (!_buttonList.TryGetValue(female, out var list))
                {
                    list = new List<int>();
                    _buttonList.Add(female, list);
                }

                list.Add(hSceneSpriteCategory.lstButton.Count);
                hSceneSpriteCategory.lstButton.Add(button);
            }
        }

        protected override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            base.OnGameLoad(args);
        }
    }
}