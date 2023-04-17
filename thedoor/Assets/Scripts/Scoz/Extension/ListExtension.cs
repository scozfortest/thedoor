using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scoz.Func {
    public static class ListExtension {
        public static int GetFirstItemOrderIndex<T>(this List<T> _list, T _t) {
            for (int i = 0; i < _list.Count; i++) {
                if (_t.Equals(_list[i])) {
                    return i;
                }
            }
            WriteLog.LogError("傳入的item不再list中");
            return 0;
        }
        public static List<float> ToFloatList(this List<object> _objList) {
            List<float> floatList = _objList.OfType<IConvertible>()
                .Select(item => Convert.ToSingle(item)).ToList();
            return floatList;
        }
    }
}