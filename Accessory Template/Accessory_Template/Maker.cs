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


namespace Template_Accessories
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static MakerDropdown Parent_DropDown;
        static MakerText ParentText;
        static MakerText ChildText;
        static MakerTextbox textbox;
        static MakerRadioButtons radio;
        static MakerButton Modify_Button;
        static MakerButton Replace_Button;
        static MakerButton Save_Relative_Button;
        static MakerButton Child_Button;

        private static void MakerAPI_MakerExiting(object sender, EventArgs e)
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

        private static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
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

        private static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            var category = new MakerCategory("", "");

            ParentText = MakerAPI.AddAccessoryWindowControl<MakerText>(new MakerText("Not Parent", category, owner));
            ChildText = MakerAPI.AddAccessoryWindowControl<MakerText>(new MakerText("Not Child", category, owner));

            var Dropdown = new MakerDropdown("Parent", new string[] { "None" }, category, 0, owner);
            Parent_DropDown = MakerAPI.AddAccessoryWindowControl<MakerDropdown>(Dropdown);

            textbox = MakerAPI.AddAccessoryWindowControl<MakerTextbox>(new MakerTextbox(category, "Name", "", owner));

            radio = new MakerRadioButtons(category, owner, "Modify", 0, new string[] { "Add", "Remove", "Rename" });

            MakerAPI.AddAccessoryWindowControl<MakerRadioButtons>(radio);

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

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            Update_DropBox();
        }

        private static void AccessoriesApi_SelectedMakerAccSlotChanged(object sender, AccessorySlotEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            Controller.StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                Controller.Update_More_Accessories();
                VisibiltyToggle();
            }
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            Controller.StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                Controller.Update_More_Accessories();
                VisibiltyToggle();
            }
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            VisibiltyToggle();
        }

        private static void AccessoriesApi_AccessoryKindChanged(object sender, AccessorySlotEventArgs e)
        {
            VisibiltyToggle();
        }

        private static void VisibiltyToggle()
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
            }
        }

        private static void Hooks_Slot_ACC_Change(object sender, Slot_ACC_Change_ARG e)
        {
            if (e.Type == 120)
            {
            }
            else
            {
            }
            VisibiltyToggle();
        }

        private static void Update_DropBox()
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            Controller.StartCoroutine(Wait());
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
