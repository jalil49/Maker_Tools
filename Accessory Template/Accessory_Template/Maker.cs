using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        MakerDropdown Parent_DropDown;
        MakerText ParentText;
        MakerText ChildText;
        MakerTextbox textbox;
        MakerRadioButtons radio;
        MakerButton Modify_Button;
        MakerButton Replace_Button;
        MakerButton Save_Relative_Button;
        MakerButton Child_Button;

        private void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.AccessoriesCopied -= (s, es) => VisibiltyToggle();
            AccessoriesApi.AccessoryTransferred -= (s, es) => VisibiltyToggle();
            AccessoriesApi.MakerAccSlotAdded -= (s, es) => VisibiltyToggle();
            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, es) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading -= (s, es) => VisibiltyToggle();

            AccessoriesApi.AccessoryKindChanged -= AccessoriesApi_AccessoryKindChanged;
            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;

            Hooks.Slot_ACC_Change -= Hooks_Slot_ACC_Change;
        }

        private void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            AccessoriesApi.AccessoriesCopied += (s, es) => VisibiltyToggle();
            AccessoriesApi.AccessoryTransferred += (s, es) => VisibiltyToggle();
            AccessoriesApi.MakerAccSlotAdded += (s, es) => VisibiltyToggle();
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, es) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading += (s, es) => VisibiltyToggle();

            AccessoriesApi.AccessoryKindChanged += AccessoriesApi_AccessoryKindChanged;
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;

            Hooks.Slot_ACC_Change += Hooks_Slot_ACC_Change;
        }

        private void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            var category = new MakerCategory("", "");

            ParentText = MakerAPI.AddAccessoryWindowControl<MakerText>(new MakerText("Not Parent", category, owner));
            ChildText = MakerAPI.AddAccessoryWindowControl<MakerText>(new MakerText("Not Child", category, owner));

            var Dropdown = new MakerDropdown("Parent", new string[] { "None" }, category, 0, owner);
            Parent_DropDown = MakerAPI.AddAccessoryWindowControl<MakerDropdown>(Dropdown);
            //Parent_DropDown.ValueChanged += Parent_ValueChanged;

            textbox = MakerAPI.AddAccessoryWindowControl<MakerTextbox>(new MakerTextbox(category, "Name", "", owner));


            radio = MakerAPI.AddAccessoryWindowControl<MakerRadioButtons>(new MakerRadioButtons(category, owner, "Modify", 0, new string[] { "Add", "Remove", "Rename" }));
            radio.ValueChanged.Subscribe(x => RadioChanged(x));

            Modify_Button = MakerAPI.AddAccessoryWindowControl<MakerButton>(new MakerButton("Modify Parent", category, owner));
            Modify_Button.OnClick.AddListener(delegate ()
            {
                List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>(Parent_DropDown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options);
                int index = 0;
                switch (radio.Value)
                {
                    case 0:
                        if (options.Any(x => x.text == textbox.Value))
                        {
                            return;
                        }
                        options.Add(new TMP_Dropdown.OptionData(textbox.Value));
                        break;
                    case 1:
                        if (textbox.Value == "None")
                        {
                            return;
                        }
                        for (int i = 0; i < options.Count; i++)
                        {
                            if (options[i].text == textbox.Value)
                            {
                                index = i;
                                break;
                            }
                            else if (i == options.Count - 1)
                            {
                                return;
                            }
                        }
                        options.RemoveAt(index);
                        break;
                    case 2:
                        if (Parent_DropDown.Value == 0)
                        {
                            return;
                        }
                        foreach (var item in options)
                        {
                            if (item.text == textbox.Value)
                            {
                                return;
                            }
                        }
                        options[Parent_DropDown.Value].text = textbox.Value;
                        break;
                    default:
                        break;
                }

                foreach (var item in Parent_DropDown.ControlObjects)
                {
                    item.GetComponentInChildren<TMP_Dropdown>().options = options;
                }
                VisibiltyToggle();
            });
            Replace_Button = MakerAPI.AddAccessoryWindowControl<MakerButton>(new MakerButton("Replace Parent", category, owner));
            Replace_Button.OnClick.AddListener(delegate ()
                {
                    if (Dropdown.Value == 0)
                    {
                        return;
                    }
                    var options = Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
                    var Slot = AccessoriesApi.SelectedMakerAccSlot;
                    VisibiltyToggle();
                });
            Child_Button = MakerAPI.AddAccessoryWindowControl<MakerButton>(new MakerButton("Make Child", category, owner));
            Child_Button.OnClick.AddListener(delegate ()
            {
                var Slot = AccessoriesApi.SelectedMakerAccSlot;
                if (Dropdown.Value == 0)
                {
                    return;
                }
                var options = Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
            });
            Save_Relative_Button = MakerAPI.AddAccessoryWindowControl<MakerButton>(new MakerButton("Save Position", category, owner));

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            Parent_DropDown.GroupingID = GroupingID;
            ParentText.GroupingID = GroupingID;
            Modify_Button.GroupingID = GroupingID;
            Replace_Button.GroupingID = GroupingID;
            textbox.GroupingID = GroupingID;
            radio.GroupingID = GroupingID;
            Child_Button.GroupingID = GroupingID;
            Save_Relative_Button.GroupingID = GroupingID;
            ChildText.GroupingID = GroupingID;
        }

        private void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            Update_DropBox();
        }

        private void AccessoriesApi_SelectedMakerAccSlotChanged(object sender, AccessorySlotEventArgs e)
        {
            StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                Update_More_Accessories();
                VisibiltyToggle();
            }
        }

        private void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                Update_More_Accessories();
                VisibiltyToggle();
            }
        }

        private void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            VisibiltyToggle();
        }

        private void AccessoriesApi_AccessoryKindChanged(object sender, AccessorySlotEventArgs e)
        {
            VisibiltyToggle();
        }

        private void VisibiltyToggle()
        {
            if (!MakerAPI.InsideMaker)
                return;

            var accessory = MakerAPI.GetCharacterControl().GetAccessoryObject(AccessoriesApi.SelectedMakerAccSlot);
            if (accessory == null)
            {
                Parent_DropDown.Visible.OnNext(false);
                ParentText.Visible.OnNext(false);
                Modify_Button.Visible.OnNext(false);
                Replace_Button.Visible.OnNext(false);
                textbox.Visible.OnNext(false);
                radio.Visible.OnNext(false);
                Child_Button.Visible.OnNext(false);
                Save_Relative_Button.Visible.OnNext(false);
                ChildText.Visible.OnNext(false);
            }
            else
            {
                Parent_DropDown.Visible.OnNext(true);
                ParentText.Visible.OnNext(true);
                Modify_Button.Visible.OnNext(true);
                Replace_Button.Visible.OnNext(true);
                textbox.Visible.OnNext(true);
                radio.Visible.OnNext(true);
                Child_Button.Visible.OnNext(true);
                Save_Relative_Button.Visible.OnNext(true);
                ChildText.Visible.OnNext(true);
                //Update_Text(AccessoriesApi.SelectedMakerAccSlot);
            }
        }

        //private void Update_Text(int slot)
        //{
        //    string output = "Slot " + slot.ToString();
        //    var find = Custom_Names[CoordinateNum].Where(x => x.Value == slot).ToArray();
        //    if (find.Length > 0)
        //    {
        //        output += " is parent of ";
        //        for (int i = 0, n = find.Length; i < n; i++)
        //        {
        //            if (i < n - 2)
        //            {
        //                output += find[i].Key + ", ";
        //            }
        //            else if (i == n - 2)
        //            {
        //                output += find[i].Key + " and ";
        //            }
        //            else
        //            {
        //                output += find[i].Key;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        output += " is not Parent";
        //    }
        //    ParentText.ControlObjects.ElementAt(slot).GetComponentInChildren<TextMeshProUGUI>().text = output;
        //    string output2;
        //    if (Child[CoordinateNum].TryGetValue(slot, out int value))
        //    {
        //        output2 = "Child of " + Custom_Names[CoordinateNum].First(x => x.Value == value).Key;
        //    }
        //    else
        //    {
        //        output2 = "Is not a Child";
        //    }
        //    ChildText.ControlObjects.ElementAt(slot).GetComponentInChildren<TextMeshProUGUI>().text = output2;
        //}




        private void Hooks_Slot_ACC_Change(object sender, Slot_ACC_Change_ARG e)
        {
            if (e.Type == 120)
            {
            }
            else
            {
            }
            VisibiltyToggle();
        }

        private void RadioChanged(int value)
        {
            var ControlObjects = radio.ControlObjects.ToArray();
            foreach (var ControlObject in ControlObjects)
            {
                var Toggles = ControlObject.GetComponentsInChildren<Toggle>();
                for (int Index = 0, n2 = Toggles.Length; Index < n2; Index++)
                {
                    Toggles[Index].isOn = value == Index;
                }
            }
        }

        private void Update_DropBox()
        {
            StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                VisibiltyToggle();
            }
            if (Parent_DropDown.ControlObject == null)
            {
                return;
            }
            var ControlObjects = Parent_DropDown.ControlObjects;
            List<TMP_Dropdown.OptionData> Options = new List<TMP_Dropdown.OptionData>(Parent_DropDown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options);
            if (Options.Count > 1)
            {
                Options.RemoveRange(1, Options.Count - 1);
            }
            //foreach (var item in Custom_Names[CoordinateNum])
            //{
            //    Options.Add(new TMP_Dropdown.OptionData(item.Key));
            //}
            foreach (var item in ControlObjects)
            {
                item.GetComponentInChildren<TMP_Dropdown>().options = Options;
            }
        }
    }
}
