using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGChapter : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string Title { get { return GetDataStr("Title"); } set { SetDataStr("Title", value); } }
        public string Description { get { return GetDataStr("Description"); } set { SetDataStr("Description", value); } }
        public int Level { get { return GetDataInt("Level"); } set { SetDataInt("Level", value); } }

        public VAGChapter(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {

        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);

        }
        public override void Clear()
        {

            base.Clear();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);

        }
        public override void AddToDict(Dictionary<string, VAGCustomStorable> Dict, string AttrName)
        {
            base.AddToDict(Dict, AttrName);

        }

    }

    public class VAGChapterCollection : VAGCustomStorableCollection
    {
        public VAGChapterCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGChapter(initialData, Store); }

        public VAGChapter Add(string Name = "")
        {
            VAGChapter item = AddNewItem() as VAGChapter;
            item.Name = Name;
            return item;
        }

        public VAGChapter ByIndex(int index)
        {
            return childs[index] as VAGChapter;
        }
        public VAGChapter ByName(string Name)
        {
            return (VAGChapter)base.ByName(Name, typeof(VAGChapter));
        }
    }
}