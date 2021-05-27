using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGChoice : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string ButtonText { get { return GetDataStr("ButtonText"); } set { SetDataStr("ButtonText", value); } }
        public VAGActionsCollection Actions;
        public VAGChoice(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            Actions = new VAGActionsCollection(GetDataObject("Actions"), ownerStore);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Actions.LoadFromJSON(GetDataObject("Actions"));
        }
        public override void Clear()
        {
            base.Clear();
            Actions.Clear();
        }

    }

    public class VAGChoicesCollection : VAGCustomStorableCollection
    {
        public VAGChoicesCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGChoice(initialData, Store); }

        public VAGChoice Add(string Name = "", string ButtonText = "")
        {
            VAGChoice item = AddNewItem() as VAGChoice;
            item.Name = Name;
            item.ButtonText = ButtonText;
            return item;
        }

        public VAGChoice ByIndex(int index)
        {
            return childs[index] as VAGChoice;
        }

        public override void Start(VAGHandler handler)
        {
            base.Start(handler);

            (GetPopUpButton(handler).PopUpWnd as VAGChoicesUI).ActiveChoices = this;
            GetPopUpButton(handler).ShowPopUp();
        }

        public virtual void ExecuteChoosen(VAGHandler handler, int choosenIndex)
        {
            GetPopUpButton(handler).PopUpWnd.Visible = false;
            ByIndex(choosenIndex).Actions.Execute(handler);
        }

        public virtual VAMUIWindowPopUpButton GetPopUpButton(VAGHandler handler)
        {
            return null;
        }
    }

    public class VAGDialogChoicesCollection : VAGChoicesCollection
    {
        public VAGDialogChoicesCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore) { }

        public override VAMUIWindowPopUpButton GetPopUpButton(VAGHandler handler)
        {
            return handler.MainMenuUI.btnChoicesUI;
        }

    }
    public class VAGFloatingChoicesCollection : VAGChoicesCollection
    {
        string ButtonCaption { get; set; }
        private VAMUIWindowPopUpButton _uiButton = null;
        public VAGFloatingChoicesCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
        }

        public VAMUIWindowPopUpButton UIButton
        {
            get
            {
                if (!Assigned(_uiButton))
                {
                    VAGChoicesUI choicesUI = new VAGChoicesUI(Store.Handler.OwnerPlugin);
                    _uiButton = new VAMUIWindowPopUpButton(null, choicesUI, "btnChoicePopUpUI", ButtonCaption, 80, 40);

                }
                return _uiButton;
            }
        }

        public VAGChoicesUI ChoicesUI {
            get
            {
                {
                    return UIButton.PopUpWnd as VAGChoicesUI;
                }
            }
        }

        public override VAMUIWindowPopUpButton GetPopUpButton(VAGHandler handler)
        {
            return UIButton;
        }

    }
}