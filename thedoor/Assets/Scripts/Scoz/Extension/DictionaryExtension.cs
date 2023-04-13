using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func {
    public static class DictionaryExtension {
        /// <summary>
        /// 取得key值在Dic裡排序 0為第一個
        /// </summary>
        public static int GetKeyOrderInDic<T, U>(this SortedDictionary<T, U> _dic, T _key) {
            int order = 0;
            foreach (var t in _dic.Keys) {
                if (t.Equals(_key)) {
                    return order;
                }
                order++;
            }
            WriteLog.LogError("Key Not in Dictionary");
            return order;
        }

        public static Dictionary<string, object> ToStringKeyDic<T>(this Dictionary<T, object> _oldDic) where T : struct, IConvertible {
            Dictionary<string, object> resultDic = new Dictionary<string, object>();
            foreach (var data in _oldDic)
                resultDic.Add(data.Key.ToString(), data.Value);
            return resultDic;
        }
        public static Dictionary<string, object> ConvertToStringKeyDic(IDictionary _oldDic) {
            if (_oldDic == null)
                return null;
            Dictionary<string, object> resultDic = new Dictionary<string, object>();
            foreach (var key in _oldDic.Keys)
                resultDic.Add(key.ToString(), _oldDic[key]);
            return resultDic;
        }
        public static Dictionary<int, int> ConvertToIntKeyValueDic(IDictionary _oldDic) {
            if (_oldDic == null)
                return null;
            Dictionary<int, int> resultDic = new Dictionary<int, int>();
            foreach (var key in _oldDic.Keys)
                resultDic.Add(Convert.ToInt32(key), Convert.ToInt32(_oldDic[key]));
            return resultDic;
        }

        public static Dictionary<string, int> ConvertToStringKeyIntValueDic(IDictionary _oldDic) {
            if (_oldDic == null)
                return null;
            Dictionary<string, int> resultDic = new Dictionary<string, int>();
            foreach (var key in _oldDic.Keys)
                resultDic.Add(key.ToString(), Convert.ToInt32(_oldDic[key]));
            return resultDic;
        }
        /// <summary>
        /// 轉成Dictionary<int, Dictionary<string, object>>
        /// </summary>
        public static Dictionary<int, Dictionary<string, object>> ConvertToMutiDic1(IDictionary _oldDic) {
            if (_oldDic == null)
                return null;
            Dictionary<int, Dictionary<string, object>> resultDic = new Dictionary<int, Dictionary<string, object>>();
            foreach (var key in _oldDic.Keys) {
                IDictionary iDic = _oldDic[key] as IDictionary;
                if (iDic == null) continue;
                Dictionary<string, object> tmpValueDic = ConvertToStringKeyDic(iDic);
                resultDic.Add(Convert.ToInt32(key), tmpValueDic);
            }
            return resultDic;
        }

    }

}