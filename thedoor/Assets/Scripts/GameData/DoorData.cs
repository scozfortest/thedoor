using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public enum DoorType {
        Monster,//妖靈
        Rest,//休息
        Encounter,//遭遇事件
    }
    public class DoorData {
        public DoorType MyType { get; private set; }
        object[] Values;


        /// <summary>
        /// 設定目標效果資料
        /// </summary>
        /// <param name="_type">門的類型</param>
        /// <param name="_values">門的參數陣列</param>
        public DoorData(DoorType _type, params object[] _values) {
            MyType = _type;
            if (_values != null && _values.Length > 0)
                Values = _values;
        }

        public string Name {
            get {
                return StringData.GetUIString(MyType.ToString());
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