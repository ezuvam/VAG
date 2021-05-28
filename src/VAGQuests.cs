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

        public VAGQuestsCollection Quests;
        public VAGQuest(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            Quests = new VAGQuestsCollection(GetDataObject("Quests"), Store);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Quests.LoadFromJSON(GetDataObject("Quests"));
        }
        public override void Clear()
        {            
            Quests.Clear();
            base.Clear();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            Quests.BindToScene(Handler);
        }
        public override void AddToDict(Dictionary<string, VAGCustomStorable> Dict, string AttrName)
        {
            base.AddToDict(Dict, AttrName);
            Quests.AddToDict(Dict, AttrName);
        }
        public override void GameStateChanged(VAGHandler Handler)
        {
            base.GameStateChanged(Handler);
            Quests.GameStateChanged(Handler);
        }

    }
    public class VAGQuestsCollection : VAGCustomStorableCollection
    {
        public VAGQuestsCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGQuest(initialData, Store); }

        public VAGQuest Add(string Name = "")
        {
            VAGQuest item = AddNewItem() as VAGQuest;
            item.Name = Name;
            return item;
        }

        public new VAGQuest ByName(string Name)
        {
            return base.ByName(Name) as VAGQuest;
        }

        public VAGQuest ByIndex(int index)
        {
            return childs[index] as VAGQuest;
        }

    }

}