using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{

    public class VAGQuest : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }

        public VAGCollection ChildQuests;
        public VAGQuest(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            ChildQuests = new VAGCollection(GetDataObject("ChildQuests"), Store);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            ChildQuests.LoadFromJSON(GetDataObject("ChildQuests"));
        }

        public override void Clear()
        {
            base.Clear();
            ChildQuests.Clear();
        }
        public override void GameStateChanged(VAGHandler Handler)
        {
            base.GameStateChanged(Handler);
            ChildQuests.GameStateChanged(Handler);
        }

    }
    public class VAGCollection : VAGCustomStorableCollection
    {
        public VAGCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGQuest(initialData, Store); }

        public VAGQuest Add(string Name = "")
        {
            VAGQuest item = AddNewItem() as VAGQuest;
            item.Name = Name;
            return item;
        }

        public VAGQuest ByIndex(int index)
        {
            return childs[index] as VAGQuest;
        }

    }

}