using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGMood : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string Plugin { get { return GetDataStr("Plugin"); } set { SetDataStr("Plugin", value); } }
        public string Method { get { return GetDataStr("Method"); } set { SetDataStr("Method", value); } }
        public string MethodParam { get { return GetDataStr("MethodParam"); } set { SetDataStr("MethodParam", value); } }

        public VAGActionsCollection StartActions;
        public VAGActionsCollection EndActions;
        private readonly VAGMoodCollection _collection;
        public VAGMood(JSONClass initialData, VAGStore ownerStore, VAGMoodCollection collection) : base(initialData, ownerStore)
        {
            _collection = collection;
            StartActions = new VAGActionsCollection(GetDataObject("StartActions"), ownerStore);
            EndActions = new VAGActionsCollection(GetDataObject("EndActions"), ownerStore);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            StartActions.LoadFromJSON(GetDataObject("StartActions"));
            EndActions.LoadFromJSON(GetDataObject("EndActions"));
        }
        public override void Clear()
        {            
            StartActions.Clear();
            EndActions.Clear();
            base.Clear();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            StartActions.BindToScene(Handler);
            EndActions.BindToScene(Handler);
        }
        public void ApplyToPerson(Atom personAtom)
        {
            CallPluginMethod(personAtom, string.IsNullOrEmpty(Plugin) ? _collection.Plugin : Plugin,
                string.IsNullOrEmpty(Method) ? _collection.Method : Method,
                MethodParam);
        }

    }

    public class VAGMoodCollection : VAGCustomStorableCollection
    {
        public string Plugin { get { return GetDataStr("Plugin"); } set { SetDataStr("Plugin", value); } }
        public string Method { get { return GetDataStr("Method"); } set { SetDataStr("Method", value); } }

        public VAGMoodCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGMood(initialData, Store, this); }

        public VAGMood Add(string Name = "")
        {
            VAGMood item = AddNewItem() as VAGMood;
            item.Name = Name;
            return item;
        }

        public VAGMood ByIndex(int index)
        {
            return childs[index] as VAGMood;
        }
        public new VAGMood ByName(string Name)
        {
            return base.ByName(Name) as VAGMood;
        }
    }
}