
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using LitJson;
namespace Scoz.Func {
    public static class ObjectExtension {
        public static Dictionary<string, object> ToDictionary(this JsonData _data) {
            if (_data == null) {
                WriteLog.LogError("Jsondata為null不可轉為Dicitonary");
                return null;
            }
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (var key in _data.Keys) {
                dic.Add(key, _data[key]);
            }
            return dic;
        }
        public static T ToEnum<T>(this object _obj) where T : struct, IConvertible {
            if (Enum.TryParse(_obj.ToString(), out T t))
                return t;
            WriteLog.LogErrorFormat("傳入字串:{0} 無法轉為 {1} Enum", _obj.ToString(), typeof(T));
            return default(T);
        }
        public static float ToFloat(this object _obj) {
            return (float)_obj;
        }
        public static List<int> ObjListToIntList(this object _obj) {
            if (_obj == null) return null;
            List<object> objs = _obj as List<object>;
            if (objs == null || objs.Count == 0) return null;
            List<int> list = new List<int>();
            foreach (var o in objs) {
                int num = Convert.ToInt32(o);
                list.Add(num);
            }
            return list;
        }
    }
}
