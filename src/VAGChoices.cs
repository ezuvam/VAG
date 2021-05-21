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

        public override void Start(VAGHandler Handler) { 
            base.Start(Handler);           

            Handler.MainMenuUI.ChoicesUI.ActiveChoices = this;
            Handler.MainMenuUI.btnChoicesUI.ShowPopUp();
        }

        public void ExecuteChoosen(VAGHandler handler, int choosenIndex)
        {
            handler.MainMenuUI.ChoicesUI.Visible = false;
            ByIndex(choosenIndex).Actions.Execute(handler);
        }
    }
}