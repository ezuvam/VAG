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
        public string ChainType { get { return GetDataStr("ChainType"); } set { SetDataStr("ChainType", value); } }

        public VAGConditionCollection Conditions;       
        public VAGCondition(JSONClass initialData, VAGStore ownerStore, VAGConditionCollection collection) : base(initialData, ownerStore)
        {            
            Conditions = new VAGConditionCollection(GetDataObject("Conditions"), ownerStore);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Conditions.LoadFromJSON(GetDataObject("Conditions"));
        }
        public override void Clear()
        {
            base.Clear();
            Conditions.Clear();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            Conditions.BindToScene(Handler);
        }

        public bool Evaluate()
        {
            bool val = true;

            switch (ConditionType)
            {
                case ">":
                    {
                        val = (Int32.Parse(Store.Handler.GetVariableValue(LeftValue)) > Int32.Parse(Store.Handler.GetVariableValue(RightValue)));
                        break;
                    }

                case "<":
                    {
                        val = (Int32.Parse(Store.Handler.GetVariableValue(LeftValue)) < Int32.Parse(Store.Handler.GetVariableValue(RightValue)));
                        break;
                    }

                case "<>":
                    {
                        val = (!Store.Handler.GetVariableValue(LeftValue).Equals(Store.Handler.GetVariableValue(RightValue)));
                        break;
                    }

                case "=":
                    {
                        val = Store.Handler.GetVariableValue(LeftValue).Equals(Store.Handler.GetVariableValue(RightValue));
                        break;
                    }
            }

            switch (ChainType)
            {
                case "and":
                    {
                        val = val && Conditions.Evaluate();
                        break;
                    }

                case "or":
                    {
                        val = val || Conditions.Evaluate();
                        break;
                    }

                case "andnot":
                    {
                        val = val && !Conditions.Evaluate();
                        break;
                    }

                case "ornot":
                    {
                        val = val || !Conditions.Evaluate();
                        break;
                    }                                      
            }

            return val;
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

                if (i == 0)
                {
                    val = Condition.Evaluate();
                }
                else
                {
                    switch (Condition.ChainType)
                    {
                        case "and":
                            {
                                val = val && Condition.Evaluate();
                                break;
                            }

                        case "or":
                            {
                                val = val || Condition.Evaluate();
                                break;
                            }

                        case "andnot":
                            {
                                val = val && !Condition.Evaluate();
                                break;
                            }

                        case "ornot":
                            {
                                val = val || !Condition.Evaluate();
                                break;
                            }
                    }
                }

            }

            return val;
        }
    }
}