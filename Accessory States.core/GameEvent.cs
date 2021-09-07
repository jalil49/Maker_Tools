using HarmonyLib;
using KKAPI.MainGame;
using System;
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
                HSprites = Traverse.Create(proc).Field("sprites").GetValue<HSprite[]>();
            }
            else
            {
                HSprites = new HSprite[] { ((HSceneProc)proc).sprite };
            }
            Hooks.HcoordChange += Hooks_HcoordChange;
            CharaEvent.Coordloaded += CharaEvent_coordloaded;
            heroines = hFlag.lstHeroine;
            for (var i = 0; i < heroines.Count; i++)
            {
                var controller = heroines[i].chaCtrl.GetComponent<CharaEvent>();
                controller.Update_Now_Coordinate();
                controller.Refresh();
                Buttonlogic(i, false);
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
                    Buttonlogic(i, true);
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
            Hooks.HcoordChange -= Hooks_HcoordChange;
            ButtonList.Clear();
            heroines = null;
            HSprites = null;
            if (hFlag.isFreeH)
            {
                CharaEvent.FreeHHeroines = new List<SaveData.Heroine>();
            }
            base.OnEndH(proc, hFlag, vr);
        }

        private void Hooks_HcoordChange(object sender, OnClickCoordinateChange e)
        {
            Buttonlogic(e.Female, false, e.Coordinate);
        }

        private void Buttonlogic(int Female, bool Coordloaded, int Coordchange = -1)
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
            //Settings.Logger.LogWarning("Enter delete");
            foreach (var item in list)
            {
                DeleteButton(Female, Harem, item);
            }
            //Settings.Logger.LogWarning("clear");

            ButtonList.Remove(Female);

            if (!Coordloaded && Coordchange == -1)
            {
                controller.Update_Now_Coordinate();
            }
            if (Coordchange > -1)
            {
                controller.Update_Now_Coordinate(Coordchange);
            }
            var names = controller.NowCoordinate.Names;
            var slotinfo = controller.NowCoordinate.Slotinfo;
            //Settings.Logger.LogWarning("create");
            var shoetype = Heroine_Ctrl.fileStatus.shoesType;
            foreach (var item in names)
            {
                if (slotinfo.Count(x => x.Value.Binding == item.Key && (x.Value.Shoetype == shoetype || x.Value.Shoetype == 2)) > 0)
                    Createbutton(Female, Harem, item.Value.Name, item.Key, controller, 0);
            }

            foreach (var item in controller.Now_Parented_Name_Dictionary)
            {
                Createbutton(Female, Harem, item.Key, 0, controller, 1);
            }
        }

        private void DeleteButton(int Female, bool Harem, int Remove)
        {
            //Settings.Logger.LogWarning("deleting button");
            Button ToRemove;
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
                ToRemove = hSceneSpriteCategory.lstButton[Remove];
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
                    var state = 0;
                    var binded = Controller.NowCoordinate.Slotinfo.Where(x => x.Value.Binding == kind);
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
                        //Settings.Logger.LogWarning($"name:{name}, kind: {kind}, State: {state}");
                        Controller.Custom_Groups(kind, state);
                        Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
                    });
                }
                else
                {
                    var state = true;
                    button.onClick.AddListener(delegate ()
                    {
                        state = !state;
                        //Settings.Logger.LogWarning($"Setting {name} show to {state} state");
                        Controller.Parent_toggle(name, state);
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
