#if !KKS
using Hook_Space;
using KKAPI.MainGame;
using System.Collections;
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
        HSceneProc hScene;

        readonly Dictionary<int, List<int>> ButtonList = new Dictionary<int, List<int>>();

        protected override void OnStartH(HSceneProc hSceneProc, bool freeH)
        {
            Hooks.HcoordChange += Hooks_HcoordChange;
            CharaEvent.Coordloaded += CharaEvent_coordloaded;
            hScene = hSceneProc;
            var heroines = hScene.dataH.lstFemale;
            for (int i = 0; i < heroines.Count; i++)
            {
                var ThisCharactersData = Constants.CharacterInfo.Find(x => heroines[i].chaCtrl.fileParam.personality == x.Personality && x.FullName == heroines[i].chaCtrl.fileParam.fullname && x.BirthDay == heroines[i].chaCtrl.fileParam.strBirthDay);
                if (ThisCharactersData != null)
                {
                    ThisCharactersData.Controller = heroines[i].chaCtrl.GetComponent<CharaEvent>();
                    ThisCharactersData.Update_Now_Coordinate();
                    ThisCharactersData.Controller.Refresh();
                }
                Buttonlogic(i, false);
            }
        }

        private void CharaEvent_coordloaded(object sender, CoordinateLoadedEventARG e)
        {
            var heroines = hScene.dataH.lstFemale;
            for (int i = 0; i < heroines.Count; i++)
            {
                if (heroines[i].chaCtrl.name == e.Character.name)
                {
                    Buttonlogic(i, true);
                    return;
                }
            }
        }

        protected override void OnEndH(HSceneProc hSceneProc, bool freeH)
        {
            Hooks.HcoordChange -= Hooks_HcoordChange;
            ButtonList.Clear();
            hScene = null;
        }

        private void Hooks_HcoordChange(object sender, OnClickCoordinateChange e)
        {
            Buttonlogic(e.Female, false, e.Coordinate);
        }

        private void Buttonlogic(int Female, bool Coordloaded, int Coordchange = -1)
        {
            bool Harem = hScene.dataH.lstFemale.Count > 1;
            var Heroine_Ctrl = hScene.dataH.lstFemale[Female].chaCtrl;
            var ThisCharactersData = Constants.CharacterInfo.Find(x => Heroine_Ctrl.fileParam.personality == x.Personality && x.FullName == Heroine_Ctrl.fileParam.fullname && x.BirthDay == Heroine_Ctrl.fileParam.strBirthDay);
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
                ThisCharactersData.Update_Now_Coordinate();
            }
            if (Coordchange > -1)
            {
                ThisCharactersData.Update_Now_Coordinate(Coordchange);
            }

            //Settings.Logger.LogWarning("create");

            foreach (var item in ThisCharactersData.Now_ACC_Name_Dictionary)
            {
                Createbutton(Female, Harem, item.Value, item.Key, ThisCharactersData, 0);
            }

            foreach (var item in ThisCharactersData.Now_Parented_Name_Dictionary.Distinct())
            {
                Createbutton(Female, Harem, item.Value, 0, ThisCharactersData, 1);
            }
        }

        private void DeleteButton(int Female, bool Harem, int Remove)
        {
            //Settings.Logger.LogWarning("deleting button");
            Button ToRemove;
            HSceneSpriteCategory hSceneSpriteCategory;
            if (Harem)
            {
                hSceneSpriteCategory = hScene.sprite.lstMultipleFemaleDressButton[Female].accessoryAll;
            }
            else
            {
                hSceneSpriteCategory = hScene.sprite.categoryAccessoryAll;
            }
            ToRemove = hSceneSpriteCategory.lstButton[Remove];
            Destroy(ToRemove.gameObject);
            hSceneSpriteCategory.lstButton.RemoveAt(Remove);

            //Settings.Logger.LogWarning("deleting button");
            //HSceneSpriteCategory hSceneSpriteCategory;
            //if (Harem)
            //{
            //    hSceneSpriteCategory = hScene.sprite.lstMultipleFemaleDressButton[Female].accessoryAll;
            //}
            //else
            //{
            //    hSceneSpriteCategory = hScene.sprite.categoryAccessoryAll;
            //}
            //Destroy(hSceneSpriteCategory.gameObject.transform.GetChild(Remove).gameObject);
            //hSceneSpriteCategory.lstButton.RemoveAt(Remove);

        }

        private void Createbutton(int Female, bool Harem, string name, int kind, Data CharacterData, int ButtonKind)
        {
            Transform parent;
            HSceneSpriteCategory hSceneSpriteCategory;
            if (Harem)
            {
                hSceneSpriteCategory = hScene.sprite.lstMultipleFemaleDressButton[Female].accessoryAll;
            }
            else
            {
                hSceneSpriteCategory = hScene.sprite.categoryAccessoryAll;
            }

            parent = hSceneSpriteCategory.transform;

            Transform origin = hScene.sprite.categoryAccessory.lstButton[0].transform;
            Transform copy = Instantiate(origin.transform, parent, false);
            copy.name = $"btn_{name}_{kind}";
            copy.GetComponentInChildren<TextMeshProUGUI>().text = name;
            var trans = copy.GetComponent<RectTransform>();
            trans.sizeDelta = new Vector2(115, trans.sizeDelta.y);

            Button button = copy.GetComponentInChildren<Button>();
            button.onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
            button.onClick.RemoveAllListeners();
            button.onClick = new Button.ButtonClickedEvent();
            button.image.raycastTarget = true;
            if (ButtonKind == 0)
            {
                int state = 0;
                var binded = CharacterData.Now_ACC_Binding_Dictionary.Where(x => x.Value == kind);
                int final = 0;
                foreach (var item in binded)
                {
                    final = Mathf.Max(final, CharacterData.Now_ACC_State_array[item.Key][1]);
                }
                final += 2;
                button.onClick.AddListener(delegate ()
                {
                    state = (state + 1) % (final);
                    //Settings.Logger.LogWarning($"name:{name}, kind: {kind}, State: {state}");
                    CharacterData.Controller.Custom_Groups(kind, state);
                    Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
                });
            }
            else
            {
                bool state = true;
                button.onClick.AddListener(delegate ()
                {
                    state = !state;
                    //Settings.Logger.LogWarning($"Setting {name} show to {state} state");
                    CharacterData.Controller.Parent_toggle(name, state);
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
}
#endif