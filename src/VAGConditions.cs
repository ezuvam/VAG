using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGCondition : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string ConditionType { get { return GetDataStr("ConditionType"); } set { SetDataStr("ConditionType", value); } }
        public string LeftValue { get { return GetDataStr("LeftValue"); } set { SetDataStr("LeftValue", value); } }
        public string RightValue { get { return GetDataStr("RightValue"); } set { SetDataStr("RightValue", value); } }

        private readonly VAGConditionCollection _collection;
        public VAGCondition(JSONClass initialData, VAGStore ownerStore, VAGConditionCollection collection) : base(initialData, ownerStore)
        {
            _collection = collection;
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
    }

    public class VAGConditionCollection : VAGCustomStorableCollection
    {
        public VAGConditionCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGCondition(initialData, Store, this); }

        public VAGCondition Add(string Name = "")
        {
            VAGCondition item = AddNewItem() as VAGCondition;
            item.Name = Name;
            return item;
        }

        public VAGCondition ByIndex(int index)
        {
            return childs[index] as VAGCondition;
        }
        public new VAGCondition ByName(string Name)
        {
            return base.ByName(Name) as VAGCondition;
        }

        public bool Evaluate()
        {
            bool val = true;

            for (int i = 0; i < Count; i++)
            {
                VAGCondition Condition = ByIndex(i);

                switch (Condition.ConditionType)
                {
                    case ">":
                        {
                            val = val && (Int32.Parse(Store.Handler.GetVariableValue(Condition.LeftValue)) > Int32.Parse(Store.Handler.GetVariableValue(Condition.RightValue)));
                            break;
                        }

                    case "<":
                        {
                            val = val && (Int32.Parse(Store.Handler.GetVariableValue(Condition.LeftValue)) < Int32.Parse(Store.Handler.GetVariableValue(Condition.RightValue)));
                            break;
                        }

                    case "<>":
                        {
                            val = val && (!Store.Handler.GetVariableValue(Condition.LeftValue).Equals(Store.Handler.GetVariableValue(Condition.RightValue)));
                            break;
                        }

                    case "=":
                        {
                            val = val && Store.Handler.GetVariableValue(Condition.LeftValue).Equals(Store.Handler.GetVariableValue(Condition.RightValue));
                            break;
                        }
                }
            }

            return val;
        }
    }
}