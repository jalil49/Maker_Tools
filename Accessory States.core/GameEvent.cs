using HarmonyLib;
using KKAPI.MainGame;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


namespace Accessory_States
{
    public class GameEvent : GameCustomFunctionController
    {
        List<SaveData.Heroine> heroines;
        HSprite[] HSprites;

        readonly Dictionary<int, List<Button>> ButtonList = new Dictionary<int, List<Button>>();
        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            if (vr)
            {
#if KK
                HSprites = Traverse.Create(proc).Field("sprites").GetValue<HSprite[]>();
#elif KKS
                HSprites = new HSprite[] { Traverse.Create(proc).Field("sprite").GetValue<HSprite>() };
#endif
            }
            else
            {
                HSprites = new HSprite[] { ((HSceneProc)proc).sprite };
            }
            CharaEvent.Coordloaded += CharaEvent_coordloaded;
            heroines = hFlag.lstHeroine;
            for (var i = 0; i < heroines.Count; i++)
            {
                Settings.Logger.LogError($"heroine {i} refresh");
                var controller = heroines[i].chaCtrl.GetComponent<CharaEvent>();
                Buttonlogic(i);
            }
            base.OnStartH(proc, hFlag, vr);
        }

        private void CharaEvent_coordloaded(object sender, CoordinateLoadedEventARG e)
        {
            if (heroines == null)
            {
                return;
            }
            for (var i = 0; i < heroines.Count; i++)
            {
                if (heroines[i].chaCtrl.name == e.Character.name)
                {
                    Buttonlogic(i);
                    return;
                }
            }
        }
        protected override void OnEndH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            ButtonList.Clear();
            heroines = null;
            HSprites = null;
            base.OnEndH(proc, hFlag, vr);
        }

        private void Buttonlogic(int Female)
        {
            if (HSprites == null)
            {
                return;
            }
            var Harem = heroines.Count > 1;
            var Heroine_Ctrl = heroines[Female].chaCtrl;
            var controller = Heroine_Ctrl.GetComponent<CharaEvent>();
            if (!ButtonList.TryGetValue(Female, out var list))
            {
                list = new List<Button>();
            }
            list.Reverse();
            foreach (var item in list)
            {
                DeleteButton(Female, Harem, item);
            }

            ButtonList.Remove(Female);

            var nameDataList = controller.Names;
            var slotinfo = controller.SlotBindingData;
            var shoetype = Heroine_Ctrl.fileStatus.shoesType;
            foreach (var nameData in nameDataList)
            {
                if (nameData.Binding < Constants.ClothingLength) continue; // skip default clothing buttons.

                if (slotinfo.Any(x => x.Value.BindingExists(nameData.Binding, shoetype))) //hide binding in-case its outdoor/indoor specific
                {
                    foreach (var sprite in HSprites)
                    {
                        var button = Createbutton(Female, Harem, nameData.Name, nameData.Binding, sprite);
                        button.onClick.AddListener(delegate ()
                        {
                            nameData.IncrementCurrentState();
                            controller.RefreshSlots();
                            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
                        });
                    }
                }
            }

            foreach (var item in controller.ParentedNameDictionary.Keys)
            {
                foreach (var sprite in HSprites)
                {
                    var button = Createbutton(Female, Harem, item, 0, sprite);

                    button.onClick.AddListener(delegate ()
                    {
                        controller.ParentedNameDictionary[item] = !controller.ParentedNameDictionary[item];
                        controller.RefreshSlots();
                        Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
                    });
                }
            }
        }

        private void DeleteButton(int Female, bool Harem, Button Remove)
        {
            foreach (var Sprite in HSprites)
            {
                var hSceneSpriteCategory = Harem ? Sprite.lstMultipleFemaleDressButton[Female].accessoryAll : Sprite.categoryAccessoryAll;
                hSceneSpriteCategory.lstButton.Remove(Remove);
            }
            Destroy(Remove);
        }

        private Button Createbutton(int Female, bool Harem, string name, int kind, HSprite Sprite)
        {
            var hSceneSpriteCategory = Harem ? Sprite.lstMultipleFemaleDressButton[Female].accessoryAll : Sprite.categoryAccessoryAll;

            var parent = hSceneSpriteCategory.transform;

            var origin = Sprite.categoryAccessory.lstButton[0].transform;
            var copy = Instantiate(origin.transform, parent, false);
            copy.name = $"btn_{name}_{kind}";
            copy.GetComponentInChildren<TextMeshProUGUI>().text = name;
            var trans = copy.GetComponent<RectTransform>();
            trans.sizeDelta = new Vector2(115, trans.sizeDelta.y);

            var button = copy.GetComponentInChildren<Button>();
            button.onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
            button.onClick.RemoveAllListeners();
            button.onClick = new Button.ButtonClickedEvent();
            button.image.raycastTarget = true;
            if (!ButtonList.TryGetValue(Female, out var list))
            {
                list = new List<Button>();
                ButtonList.Add(Female, list);
            }
            list.Add(button);
            hSceneSpriteCategory.lstButton.Add(button);
            return button;
        }
    }
}
