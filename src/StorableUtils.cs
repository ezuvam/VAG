#define DEBUG_MODE

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public delegate void VAGObjEventHandler(VAGObject sender, VAGHandler Handler);
    public delegate void VAGObjEvent(VAGObject sender);

    public class VAGObject
    {
        public VAGState States = new VAGState();
        public event VAGObjEventHandler OnPlayingFinish;
        public virtual void Start(VAGHandler Handler)
        {
            States.TimeToRun = 0;
            States.Stopped = false;
            States.Running = true;
        }

        public virtual void Stop(VAGHandler Handler)
        {
            States.TimeToRun = 0;
            States.Running = false;
            States.Stopped = true;
        }

        public virtual void Finish(VAGHandler Handler)
        {
            States.Running = false;

            if (OnPlayingFinish != null)
            {
                OnPlayingFinish(this, Handler);
            }
        }

    }

    public class VAGCustomStorable : VAGObject
    {
        public string UID
        {
            get
            {
                string s = GetDataStr("UID");

                if (string.IsNullOrEmpty(s))
                {
                    s = GetDataStr("Name");
                }

                return s;
            }
            set { SetDataStr("UID", value); }
        }
        public JSONClass _data;
        public event VAGObjEvent OnChanged;

        public bool Assigned(object o) { return !System.Object.ReferenceEquals(o, null); }
        public VAGCustomStorable(JSONClass initialData)
        {
            if (Assigned(initialData)) { _data = initialData; } else { _data = new JSONClass(); }
        }

        public void SetDataStr(string key, string value)
        {
            _data[key] = value;
        }
        public string GetDataStr(string key, string defval = "")
        {
            if (_data.AsObject.HasKey(key))
            {
                return _data[key];
            }
            else
            {
                return defval;
            }
        }

        public void SetDataInt(string key, int value)
        {
            _data[key].AsInt = value;

        }
        public int GetDataInt(string key, int defval = 0)
        {
            if (_data.AsObject.HasKey(key))
            {
                if (string.IsNullOrEmpty(_data[key].Value))
                {
                    return defval;
                }
                else
                {
                    return _data[key].AsInt;
                }
            }
            else
            {
                return defval;
            }
        }
        public void SetDataBool(string key, bool value)
        {
            _data[key].AsBool = value;
        }

        public bool GetDataBool(string key, bool defval = false)
        {
            if (_data.AsObject.HasKey(key))
            {
                if (string.IsNullOrEmpty(_data[key].Value))
                {
                    return defval;
                }
                else
                {
                    return _data[key].AsBool;
                }
            }
            else
            {
                return defval;
            }
        }
        public void SetDataFloat(string key, float value)
        {
            _data[key].AsFloat = value;

        }
        public float GetDataFloat(string key, float defval = 0)
        {
            if (_data.AsObject.HasKey(key))
            {
                if (string.IsNullOrEmpty(_data[key].Value))
                {
                    return defval;
                }
                else
                {
                    return _data[key].AsFloat;
                }
            }
            else
            {
                return defval;
            }
        }
        public JSONClass GetDataObject(string key)
        {
            if (_data.AsObject.HasKey(key))
            {
                return _data[key].AsObject;
            }
            else
            {
                JSONClass o = new JSONClass();
                _data.AsObject.Add(key, o);
                return o;
            }
        }

        public JSONArray GetDataArray(string key)
        {
            if (_data.AsObject.HasKey(key))
            {
                return _data[key].AsArray;
            }
            else
            {
                JSONArray a = new JSONArray();
                _data.AsObject.Add(key, a);
                return a;
            }
        }

        public virtual void AddToDict(Dictionary<string, VAGCustomStorable> Dict, string AttrName)
        {
            if (Assigned(_data))
            {
                if (_data.HasKey(AttrName))
                {
                    string key = GetDataStr(AttrName);

                    if (!string.IsNullOrEmpty(key))
                    {
                        if (!Dict.ContainsKey(key))
                        {
                            Dict.Add(key, this);
                        }
                        else
                        {
                            SuperController.LogError($"VAGItem with attribut {AttrName} value {key} already exists");
                        }
                    }
                }
            }
        }

        public virtual void Clear()
        {
            if (Assigned(_data))
            {
                for (int i = _data.Count - 1; i >= 0; i--)
                {
                    _data.Remove(i);
                }
            }
            else
            {
                _data = new JSONClass();
            }
        }

        public virtual void LoadFromJSON(JSONClass jsonData)
        {
            _data = jsonData;
        }

        public virtual JSONClass SaveToJSON()
        {
            return _data;
        }

        public virtual void Changed()
        {
            if (OnChanged != null)
            {
                OnChanged(this);
            }

        }

        public virtual void Assign(VAGCustomStorable source)
        {
            Clear();

            foreach (string key in source._data.AsObject.Keys)
            {
                _data.Add(key, source._data[key].Value);
            }

            LoadFromJSON(_data);
        }
        public virtual void BindToScene(VAGHandler Handler)
        {

        }

    }

    public class VAGCustomGameObject : VAGCustomStorable
    {
        // Handler.GameStates.GetGameState()
        public VAGStore Store { get { return _store; } }
        private readonly VAGStore _store;

        public VAGCustomGameObject(JSONClass initialData, VAGStore ownerStore) : base(initialData)
        {
            _store = ownerStore;
        }
        public JSONClass GameStates
        {
            get
            {
                return Store.GameStates.GetGameState(this);
            }
        }

        public virtual void GameStateChanged(VAGHandler Handler)
        {

        }

        public static JSONStorable FindStorableByNameFlex(Atom atom, string storableClassName)
        {
            List<string> names = atom.GetStorableIDs();

            string jsName = names.Find(s => s.Contains(storableClassName));
            if (!string.IsNullOrEmpty(jsName))
            {
                return atom.GetStorableByID(jsName);
            }
            else
            {
                return null;
            }
        }
        public static JSONStorable FindPlugin(Atom atom, string pluginClassName)
        {
            List<string> names = atom.GetStorableIDs();

            string pluginName = names.Find(s => s.StartsWith("plugin#") && s.Contains(pluginClassName));
            if (!string.IsNullOrEmpty(pluginName))
            {
                return atom.GetStorableByID(pluginName);
            }
            else
            {
                return null;
            }
        }
        public static void CallPluginMethod(Atom atom, string pluginClassName, string methodeName, object parameter)
        {
            JSONStorable plugin = FindPlugin(atom, pluginClassName);

            if (plugin != null)
            {
                plugin.BroadcastMessage(methodeName, parameter, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                SuperController.LogError($"Plugin {pluginClassName} not found on atom {atom.name}!");
            }

        }

    }

    public class VAGCustomStorableCollection : VAGCustomGameObject
    {
        private readonly string _storename;
        protected List<VAGCustomGameObject> childs = new List<VAGCustomGameObject>();
        protected Dictionary<string, VAGCustomStorable> NameDict;

        public VAGCustomStorableCollection(JSONClass initialData, string storename, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            _storename = storename;
        }
        protected virtual VAGCustomGameObject CreateNewItem(JSONClass initialData)
        {
            return new VAGCustomGameObject(initialData, Store);
        }

        public int Count { get { return childs.Count; } }

        public override void AddToDict(Dictionary<string, VAGCustomStorable> Dict, string AttrName)
        {
            base.AddToDict(Dict, AttrName);

            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].AddToDict(Dict, AttrName);
            }
        }

        protected void InvalidateNameDict()
        {
            if (!Assigned(NameDict))
            {
                NameDict = new Dictionary<string, VAGCustomStorable>();
                AddToDict(NameDict, "Name");
            }
        }


        public VAGCustomStorable ByName(string Name)
        {
            InvalidateNameDict();

            VAGCustomStorable item;
            if (NameDict.TryGetValue(Name, out item))
            {
                return item;
            }
            else
            {
                return null;
            }

        }

        public VAGCustomStorable ByAttrValue(string attrName, string attrValue)
        {
            for (int i = 0; i < childs.Count; i++)
            {
                if (childs[i]._data[attrName].Value.Equals(attrValue))
                {
                    return childs[i];
                }
            }
            return null;
        }

        public override void Clear()
        {
            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].Clear();
            }
            childs.Clear();

            base.Clear();

            NameDict = null;
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);

            if (Assigned(childs) & Assigned(_storename) & (_storename.Length > 0))
            {
                childs.Clear();

                if (Assigned(_data) & (Assigned(_storename)) & (_data.AsObject.HasKey(_storename)))
                {

                    JSONArray jsonQuests = _data[_storename].AsArray;

                    for (int i = 0; i < jsonQuests.Count; i++)
                    {
                        VAGCustomGameObject _questItem = CreateNewItem(null);
                        _questItem.LoadFromJSON(jsonQuests[i].AsObject);
                        childs.Add(_questItem);
                    }
                }

#if DEBUG_MODE
                //SuperController.LogMessage($"DEBUG: collection loaded {childs.Count} childs");
#endif
            }

        }
        protected VAGCustomGameObject AddNewItem()
        {
            VAGCustomGameObject item = CreateNewItem(null);
            GetDataArray(_storename).Add(item._data);

            childs.Add(item);

            return item;
        }

        public void FillItems(List<string> dest, string attrName = "Name")
        {
            for (int i = 0; i < childs.Count; i++)
            {
                dest.Add(childs[i]._data[attrName]);
            }
        }

        public override void GameStateChanged(VAGHandler Handler)
        {
            base.GameStateChanged(Handler);
            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].GameStateChanged(Handler);
            }
        }

        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].BindToScene(Handler);
            }
        }

    }

}