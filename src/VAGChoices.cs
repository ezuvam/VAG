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
        public string DisabledButtonText { get { return GetDataStr("DisabledButtonText"); } set { SetDataStr("DisabledButtonText", value); } }
        
        public VAGActionsCollection Actions;
        public VAGDialogsCollection Dialogs;
        public VAGConditionCollection Conditions;
        public VAGChoice(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            Actions = new VAGActionsCollection(GetDataObject("Actions"), ownerStore);
            Dialogs = new VAGDialogsCollection(GetDataObject("Dialogs"), ownerStore);
            Conditions = new VAGConditionCollection(GetDataObject("Conditions"), ownerStore);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Actions.LoadFromJSON(GetDataObject("Actions"));
            Dialogs.LoadFromJSON(GetDataObject("Dialogs"));
            Conditions.LoadFromJSON(GetDataObject("Conditions"));
        }
        public override void Clear()
        {
            Actions.Clear();
            Dialogs.Clear();
            Conditions.Clear();
            base.Clear();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            Actions.BindToScene(Handler);
            Dialogs.BindToScene(Handler);
            Conditions.BindToScene(Handler);
        }

        public override void AddToDict(Dictionary<string, VAGCustomStorable> Dict, string AttrName)
        {
            base.AddToDict(Dict, AttrName);
            Actions.AddToDict(Dict, AttrName);
            Dialogs.AddToDict(Dict, AttrName);
            Conditions.AddToDict(Dict, AttrName);
        }

        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);
            if (Actions.Count > 0) { Handler.PlayObject(Actions); }
            if (Dialogs.Count > 0) { Handler.PlayObject(Dialogs); }
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

            handler.PlayObject(ByIndex(choosenIndex));
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

        public VAGChoicesUI ChoicesUI
        {
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