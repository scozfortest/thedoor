using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedAdventureData : OwnedData {
        public string OwnRoleUID { get; private set; }
        public int CurDoor { get; private set; }
        public List<DoorData> DoorDatas = new List<DoorData>();

        public OwnedAdventureData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            OwnRoleUID = _data.TryGetValue("OwnRoleUID", out value) ? Convert.ToString(value) : default(string);
            CurDoor = _data.TryGetValue("CurDoor", out value) ? Convert.ToInt32(value) : default(int);

            //設定門資料
            DoorDatas.Clear();
            List<object> doorObjs = _data.TryGetValue("Doors", out value) ? value as List<object> : null;
            for (int i = 0; i < doorObjs.Count; i++) {
                Dictionary<string, object> doorDic = DictionaryExtension.ConvertToStringKeyDic(doorObjs[i]);
                string typeStr = doorDic.TryGetValue("DoorType", out value) ? Convert.ToString(value) : default(string);
                if (MyEnum.TryParseEnum(typeStr, out DoorType type)) {
                    Dictionary<string, object> values = doorDic.TryGetValue("Values", out value) ? DictionaryExtension.ConvertToStringKeyDic(value) : null;
                    var doorData = new DoorData(type, values);
                    DoorDatas.Add(doorData);
                }
            }
        }

    }
}
