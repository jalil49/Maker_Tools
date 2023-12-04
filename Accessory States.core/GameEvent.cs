using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Illusion.Game;
using KKAPI.MainGame;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Accessory_States
{
    public class GameEvent : GameCustomFunctionController
    {
        private readonly Dictionary<int, List<Button>> _buttonList = new Dictionary<int, List<Button>>();
        private List<SaveData.Heroine> _heroines;
        private HSprite[] _hSprites;

        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            if(vr)
            {
#if KK
                _hSprites = Traverse.Create(proc).Field("sprites").GetValue<HSprite[]>();
#elif KKS
                _hSprites = new[] { Traverse.Create(proc).Field("sprite").GetValue<HSprite>() };
#endif
            }
            else
            {
                _hSprites = new[] { ((HSceneProc)proc).sprite };
            }

            CharaEvent.Coordloaded += CharaEvent_coordloaded;
            _heroines = hFlag.lstHeroine;
            for (var i = 0; i < _heroines.Count; i++)
            {
                Settings.Logger.LogError($"heroine {i} refresh");
                _heroines[i].chaCtrl.GetComponent<CharaEvent>();
                Buttonlogic(i);
            }

            base.OnStartH(proc, hFlag, vr);
        }

        private void CharaEvent_coordloaded(object sender, CoordinateLoadedEventArg e)
        {
            if (_heroines == null) return;

            for (var i = 0; i < _heroines.Count; i++)
                if (_heroines[i].chaCtrl.name == e.Character.name)
                {
                    Buttonlogic(i);
                    return;
                }
        }

        protected override void OnEndH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            _buttonList.Clear();
            _heroines = null;
            _hSprites = null;
            base.OnEndH(proc, hFlag, vr);
        }

        private void Buttonlogic(int female)
        {
            if (_hSprites == null) return;

            var harem = _heroines.Count > 1;
            var heroineCtrl = _heroines[female].chaCtrl;
            var controller = heroineCtrl.GetComponent<CharaEvent>();
            if (!_buttonList.TryGetValue(female, out var list)) list = new List<Button>();

            list.Reverse();
            foreach (var item in list) DeleteButton(female, harem, item);

            _buttonList.Remove(female);

            var nameDataList = controller.NameDataList;
            var slotData = controller.SlotBindingData;
            var shoeType = heroineCtrl.fileStatus.shoesType;
            foreach (var nameData in nameDataList)
            {
                if (nameData.binding < Constants.ClothingLength)
                    continue; // skip default clothing buttons.

                if (slotData.Any(x =>
                        x.Value.BindingExists(nameData.binding,
                            shoeType))) //hide binding in-case its outdoor/indoor specific
                    foreach (var sprite in _hSprites)
                    {
                        var button = CreateButton(female, harem, nameData.Name, nameData.binding, sprite);
                        button.onClick.AddListener(delegate
                        {
                            nameData.IncrementCurrentState();
                            controller.RefreshSlots(nameData.AssociatedSlots);
                            Utils.Sound.Play(SystemSE.sel);
                        });
                    }
            }

            foreach (var item in controller.ParentedNameDictionary)
            foreach (var sprite in _hSprites)
            {
                var button = CreateButton(female, harem, item.Key, 0, sprite);

                button.onClick.AddListener(delegate
                {
                    item.Value.Toggle();
                    controller.RefreshSlots(item.Value.AssociateSlots);
                    Utils.Sound.Play(SystemSE.sel);
                });
            }
        }

        private void DeleteButton(int female, bool harem, Button remove)
        {
            foreach (var sprite in _hSprites)
            {
                var hSceneSpriteCategory =
                    harem ? sprite.lstMultipleFemaleDressButton[female].accessoryAll : sprite.categoryAccessoryAll;
                hSceneSpriteCategory.lstButton.Remove(remove);
            }

            Destroy(remove);
        }

        private Button CreateButton(int female, bool harem, string buttonName, int kind, HSprite sprite)
        {
            var hSceneSpriteCategory =
                harem ? sprite.lstMultipleFemaleDressButton[female].accessoryAll : sprite.categoryAccessoryAll;

            var parent = hSceneSpriteCategory.transform;

            var origin = sprite.categoryAccessory.lstButton[0].transform;
            var copy = Instantiate(origin.transform, parent, false);
            copy.name = $"btn_{buttonName}_{kind.ToString()}";
            copy.GetComponentInChildren<TextMeshProUGUI>().text = buttonName;
            var trans = copy.GetComponent<RectTransform>();
            trans.sizeDelta = new Vector2(115, trans.sizeDelta.y);

            var button = copy.GetComponentInChildren<Button>();
            button.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
            button.onClick.RemoveAllListeners();
            button.onClick = new Button.ButtonClickedEvent();
            button.image.raycastTarget = true;
            if (!_buttonList.TryGetValue(female, out var list))
            {
                list = new List<Button>();
                _buttonList.Add(female, list);
            }

            list.Add(button);
            hSceneSpriteCategory.lstButton.Add(button);
            return button;
        }
    }
}