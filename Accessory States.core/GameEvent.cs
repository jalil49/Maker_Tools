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

        readonly Dictionary<int, List<int>> ButtonList = new Dictionary<int, List<int>>();
#if KK
        protected override void OnStartH(BaseLoader proc, HFlag hFlag, bool vr)
#else
        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
#endif
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
                controller.Refresh();
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
#if KK
        protected override void OnEndH(BaseLoader proc, HFlag hFlag, bool vr)
#else
        protected override void OnEndH(MonoBehaviour proc, HFlag hFlag, bool vr)
#endif
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
                list = new List<int>();
            }
            list.Reverse();
            foreach (var item in list)
            {
                DeleteButton(Female, Harem, item);
            }

            ButtonList.Remove(Female);

            var names = controller.Names;
            var slotinfo = controller.SlotInfo;
            var shoetype = Heroine_Ctrl.fileStatus.shoesType;
            var i = 0;
            foreach (var item in names)
            {
                if (slotinfo.Count(x => x.Value.Binding == Constants.ClothingLength + i && (x.Value.ShoeType == shoetype || x.Value.ShoeType == 2)) > 0)
                {
                    Createbutton(Female, Harem, item.Name, Constants.ClothingLength + i, controller, 0);
                }
                i++;
            }

            foreach (var item in controller.ParentedNameDictionary)
            {
                Createbutton(Female, Harem, item.Key, 0, controller, 1);
            }
        }

        private void DeleteButton(int Female, bool Harem, int Remove)
        {
            HSceneSpriteCategory hSceneSpriteCategory;
            foreach (var Sprite in HSprites)
            {
                if (Harem)
                {
                    hSceneSpriteCategory = Sprite.lstMultipleFemaleDressButton[Female].accessoryAll;
                }
                else
                {
                    hSceneSpriteCategory = Sprite.categoryAccessoryAll;
                }
                var ToRemove = hSceneSpriteCategory.lstButton[Remove];
                Destroy(ToRemove.gameObject);
                hSceneSpriteCategory.lstButton.RemoveAt(Remove);
            }
        }

        private void Createbutton(int Female, bool Harem, string name, int kind, CharaEvent Controller, int ButtonKind)
        {
            Transform parent;
            HSceneSpriteCategory hSceneSpriteCategory;
            foreach (var Sprite in HSprites)
            {
                if (Harem)
                {
                    hSceneSpriteCategory = Sprite.lstMultipleFemaleDressButton[Female].accessoryAll;
                }
                else
                {
                    hSceneSpriteCategory = Sprite.categoryAccessoryAll;
                }

                parent = hSceneSpriteCategory.transform;

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
                if (ButtonKind == 0)
                {
                    var state = Controller.Names[kind - Constants.ClothingLength].DefaultState;
                    var binded = Controller.SlotInfo.Where(x => x.Value.Binding == kind);
                    var final = 0;
                    foreach (var item in binded)
                    {
                        foreach (var item2 in item.Value.States)
                        {
                            final = Mathf.Max(final, item2[1]);
                        }
                    }
                    final += 2;
                    button.onClick.AddListener(delegate ()
                    {
                        state = (state + 1) % (final);
                        Controller.CustomGroups(kind, state);
                        Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
                    });
                }
                else
                {
                    var state = true;
                    button.onClick.AddListener(delegate ()
                    {
                        state = !state;
                        Controller.ParentToggle(name, state);
                        Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
                    });
                }
                if (!ButtonList.TryGetValue(Female, out var list))
                {
                    list = new List<int>();
                    ButtonList.Add(Female, list);
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
