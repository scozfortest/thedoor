using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedAdventureData : OwnedData {
        [ScozSerializable] public string OwnRoleUID { get; private set; }
        [ScozSerializable] public int CurDoorIndex { get; private set; }
        [ScozSerializable] public List<DoorData> Doors { get; private set; } = new List<DoorData>();

        public DoorData CurDoor { get { return Doors[CurDoorIndex]; } }

        public OwnedAdventureData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            OwnRoleUID = _data.TryGetValue("OwnRoleUID", out value) ? Convert.ToString(value) : default(string);
            CurDoorIndex = _data.TryGetValue("CurDoorIndex", out value) ? Convert.ToInt32(value) : default(int);

            //設定門資料
            Doors.Clear();
            List<object> doorObjs = _data.TryGetValue("Doors", out value) ? value as List<object> : null;
            if (doorObjs != null) {
                for (int i = 0; i < doorObjs.Count; i++) {
                    Dictionary<string, object> doorDic = DicExtension.ConvertToStringKeyDic(doorObjs[i]);
                    if (doorDic == null) continue;
                    string typeStr = doorDic.TryGetValue("DoorType", out value) ? Convert.ToString(value) : default(string);
                    if (MyEnum.TryParseEnum(typeStr, out DoorType type)) {
                        Dictionary<string, object> values = doorDic.TryGetValue("Values", out value) ? DicExtension.ConvertToStringKeyDic(value) : null;
                        var doorData = new DoorData(type, values);
                        Doors.Add(doorData);
                    }
                }
            }
        }
        public void NextDoor() {
            CurDoorIndex++;
        }


    }
}
