using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGTransition : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string Description { get { return GetDataStr("Description"); } set { SetDataStr("Description", value); } }
        public float Delay { get { return GetDataFloat("Delay"); } set { SetDataFloat("Delay", value); } }
        public bool StopAllDialogs { get { return GetDataBool("StopAllDialogs", true); } set { SetDataBool("StopAllDialogs", value); } }
        public bool Active { get; set; }
        public VAGAtomSettingsCollection Atoms;
        private VAGAtomSettingsCollection _atombackup = null;
        public VAGTransition(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            Atoms = new VAGAtomSettingsCollection(GetDataObject("Atoms"), ownerStore);
            Active = false;
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Atoms.LoadFromJSON(GetDataObject("Atoms"));
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            Atoms.BindToScene(Handler);
        }
        public override void Clear()
        {
            Atoms.Clear();
            base.Clear();
        }
        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);

            if (Active)
            {
                ExecuteStart(Handler);
            }
            else
            {
                ExecuteEnd(Handler);
            }

        }

        public override void Finish(VAGHandler Handler)
        {
            base.Finish(Handler);
        }

        public void ExecuteStart(VAGHandler Handler)
        {
            if (StopAllDialogs)
            {
                Handler.StopAllDialogs();
            }

            if (!Assigned(_atombackup))
            {
                SuperController.LogMessage($"AtomBackup already existing on transistion start!");
                _atombackup = null;
            }

            Handler.ActiveTransition = this;
            SuperController.singleton.pauseRender = true;
            SuperController.singleton.SetFreezeAnimation(true);

            _atombackup = new VAGAtomSettingsCollection(null, Store);
            _atombackup.LoadFromAtoms(Atoms);

            //SuperController.LogMessage($"AtomBackup is {_atombackup._data}");

            Atoms.ApplyToAtoms(Handler);

        }
        public void ExecuteEnd(VAGHandler Handler)
        {
            if (Assigned(_atombackup))
            {
                //SuperController.LogMessage($"AtomBackup for restore is {_atombackup._data}");

                _atombackup.ApplyToAtoms(Handler);
                _atombackup = null;

                SuperController.singleton.pauseRender = false;
                SuperController.singleton.SetFreezeAnimation(false);

                Handler.ActiveTransition = null;
            }
            else
            {
                SuperController.LogMessage($"NO AtomBackup exists!");
            }
        }

    }

    public class VAGTransitionsCollection : VAGCustomStorableCollection
    {
        public VAGTransitionsCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGTransition(initialData, Store); }

        public VAGTransition Add(string name = "", string description = "")
        {
            VAGTransition item = AddNewItem() as VAGTransition;
            item.Name = name;
            item.Description = description;
            return item;
        }

        public VAGTransition ByIndex(int index)
        {
            return childs[index] as VAGTransition;
        }
        public new VAGTransition ByName(string Name)
        {
            return base.ByName(Name) as VAGTransition;
        }

    }
}