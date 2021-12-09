using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{

    public class VAGItem : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string AtomName { get { return GetDataStr("AtomName"); } set { SetDataStr("AtomName", value); } }
        public string Description { get { return GetDataStr("Description"); } set { SetDataStr("Description", value); } }

        public VAGFloatingChoicesCollection Choices;
        public VAGActionsCollection GrabActions;
        public VAGActionsCollection DropActions;
        private readonly VAGItemCollection _collection;
        private bool _isGrabbed = false;
        public VAGItem(JSONClass initialData, VAGStore ownerStore, VAGItemCollection collection) : base(initialData, ownerStore)
        {
            _collection = collection;
            Choices = new VAGFloatingChoicesCollection(GetDataObject("Choices"), ownerStore);
            GrabActions = new VAGActionsCollection(GetDataObject("GrabActions"), ownerStore);
            DropActions = new VAGActionsCollection(GetDataObject("DropActions"), ownerStore);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Choices.LoadFromJSON(GetDataObject("Choices"));
            GrabActions.LoadFromJSON(GetDataObject("GrabActions"));
            DropActions.LoadFromJSON(GetDataObject("DropActions"));
        }
        public override void Clear()
        {
            Atom atom = SuperController.singleton.GetAtomByUid(AtomName);

            if (Assigned(atom))
            {
                atom.mainController.onGrabStartHandlers -= DoOnAtomGrabStart;
            }

            Choices.Clear();
            GrabActions.Clear();
            DropActions.Clear();
            base.Clear();
        }
        public override void Start(VAGHandler handler)
        {
            base.Start(handler);
            ShowAtomUI();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            Choices.BindToScene(Handler);
            GrabActions.BindToScene(Handler);
            DropActions.BindToScene(Handler);

            Atom atom = SuperController.singleton.GetAtomByUid(AtomName);

            if (Assigned(atom))
            {
                atom.mainController.onGrabStartHandlers += DoOnAtomGrabStart;
                atom.mainController.onGrabEndHandlers += DoOnAtomGrabEnd;
            }
        }

        public void ShowAtomUI()
        {
            Atom atom = SuperController.singleton.GetAtomByUid(AtomName);

            if (Assigned(atom))
            {
                Choices.ChoicesUI.ActiveChoices = Choices;
                Choices.UIButton.SetParentTransform(atom.mainController.transform);
                Choices.UIButton.LookAtPlayer();
                Choices.UIButton.Visible = true;
            }
            else
            {
                SuperController.LogError($"VAGItem {Name}: Atom with name {AtomName} not found!");
            }
        }

        public void DoGrabItem()
        {
            if (!_isGrabbed)
            {
                _isGrabbed = true;

                ShowAtomUI();
                Store.Handler.PlayObject(GrabActions);
            }
        }

        public void DoDropItem()
        {
            if (_isGrabbed)
            {
                _isGrabbed = false;
                Store.Handler.PlayObject(DropActions);
            }
        }
        public void DoOnAtomGrabStart(FreeControllerV3 fcv3)
        {
            DoGrabItem();
            //SuperController.LogMessage($"VAGItem {Name}: Atom with name {AtomName} grabbed");     
        }

        public void DoOnAtomGrabEnd(FreeControllerV3 fcv3)
        {
            DoDropItem();
            //SuperController.LogMessage($"VAGItem {Name}: Atom with name {AtomName} dropped");     
        }

    }

    public class VAGItemCollection : VAGCustomStorableCollection
    {
        public VAGItemCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGItem(initialData, Store, this); }

        public VAGItem Add(string Name = "")
        {
            VAGItem item = AddNewItem() as VAGItem;
            item.Name = Name;
            return item;
        }

        public VAGItem ByIndex(int index)
        {
            return childs[index] as VAGItem;
        }
        public new VAGItem ByName(string Name)
        {
            return base.ByName(Name) as VAGItem;
        }
    }
}