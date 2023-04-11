
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using LitJson;
namespace Scoz.Func
{
    public static class ObjectExtension
    {
        public static Dictionary<string, object> ToDictionary(this JsonData _data)
        {
            if (_data == null)
            {
                DebugLogger.LogError("Jsondata為null不可轉為Dicitonary");
                return null;
            }
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (var key in _data.Keys)
            {
                dic.Add(key, _data[key]);
            }
            return dic;
        }
        public static T ToEnum<T>(this object _obj) where T : struct, IConvertible
        {
            if (Enum.TryParse(_obj.ToString(), out T t))
                return t;
            DebugLogger.LogErrorFormat("傳入字串:{0} 無法轉為 {1} Enum", _obj.ToString(), typeof(T));
            return default(T);
        }
        public static float ToFloat(this object _obj)
        {
            return (float)_obj;
        }
    }
}
