using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public enum DoorType {
        Start,//起始
        Monster,//妖靈
        Rest,//休息
        Encounter,//遭遇事件
        Boss,//頭目
    }
    public class DoorData : IScozJsonConvertible {
        [ScozSerializable] public DoorType MyType { get; private set; }
        [ScozSerializable] public Dictionary<string, string> Values { get; private set; }


        /// <summary>
        /// 初始化門資料
        /// </summary>
        public DoorData(DoorType _type, Dictionary<string, string> _values) {
            MyType = _type;
            Values = _values;
        }

        public string Name {
            get {
                return StringData.GetUIString("DoorType_" + MyType.ToString());
            }
        }

        //public string Name {
        //    get {
        //        string name = "";
        //        switch (MyType) {
        //            case DoorType.:
        //                name = string.Format(StringData.GetUIString(MyType.ToString()), Values[0]);
        //                break;
        //            case DoorType.Stun:
        //                name = string.Format(StringData.GetUIString(MyType.ToString()), Values[0]);
        //                break;
        //            default:
        //                name = StringData.GetUIString(MyType.ToString());
        //                break;
        //        }
        //        return name;
        //    }
        //}
    }
}