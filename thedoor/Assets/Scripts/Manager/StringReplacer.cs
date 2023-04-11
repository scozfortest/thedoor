using TheDoor.Main;
using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {

    public class StringReplacer {

        public enum StringReplaceType {
            TalkerName,
        }
        public delegate void RefStringAction(ref string _str);
        /// <summary>
        /// 替代文字A為文字B清單
        /// </summary>
        static Dictionary<StringReplaceType, Dictionary<string, string>> ReplaceStrs;
        /// <summary>
        /// 將文字A透過方法替代為文字B清單
        /// </summary>
        static Dictionary<StringReplaceType, Dictionary<string, RefStringAction>> ReplaceStrActions;

        public static void Init() {
            ReplaceStrs = new Dictionary<StringReplaceType, Dictionary<string, string>>();
            ReplaceStrActions = new Dictionary<StringReplaceType, Dictionary<string, RefStringAction>>();
            InitTalkerNameReplaceStr();
        }
        static void InitTalkerNameReplaceStr() {
            ReplaceStrActions.Clear();
            ReplaceStrActions[StringReplaceType.TalkerName] = new Dictionary<string, RefStringAction>() {
                //{ "{PlayerName}",(ref string _str)=>{_str=_str.Replace("{PlayerName}", GamePlayer.Instance.Data.Name); } },//把{PlayerName}取代為玩家名稱
            };
        }
        /// <summary>
        /// 新增替代文字A為文字B清單
        /// </summary>
        public static void AddReplaceStrs(StringReplaceType _type, string _key, string _replacedStr) {
            if (ReplaceStrs != null) {
                DebugLogger.LogError("StringReplacer尚未初始化");
                return;
            }
            if (ReplaceStrs.ContainsKey(_type) && ReplaceStrs[_type] != null)
                ReplaceStrs[_type][_key] = _replacedStr;
            else
                ReplaceStrs[_type].Add(_key, _replacedStr);
            SameReplacedKeyCheck(_type);
        }
        public static void RemoveReplaceStr(StringReplaceType _type, string _key) {
            if (ReplaceStrs != null) {
                DebugLogger.LogError("StringReplacer尚未初始化");
                return;
            }
            ReplaceStrs[_type].Remove(_key);
        }
        /// <summary>
        /// 新增文字A透過方法替代為文字B清單
        /// </summary>
        public static void AddReplaceStrActions(StringReplaceType _type, string _key, RefStringAction _refStringAction) {
            if (ReplaceStrActions != null) {
                DebugLogger.LogError("StringReplacer尚未初始化");
                return;
            }
            if (ReplaceStrActions.ContainsKey(_type) && ReplaceStrActions[_type] != null)
                ReplaceStrActions[_type][_key] = _refStringAction;
            else
                ReplaceStrActions[_type].Add(_key, _refStringAction);
            SameReplacedKeyCheck(_type);
        }
        public static void RemoveReplaceStrAction(StringReplaceType _type, string _key) {
            if (ReplaceStrActions != null) {
                DebugLogger.LogError("StringReplacer尚未初始化");
                return;
            }
            ReplaceStrActions[_type].Remove(_key);
        }
        /// <summary>
        /// 檢查兩種取代類型的Key值是否重複，並跳錯誤訊息
        /// </summary>
        static void SameReplacedKeyCheck(StringReplaceType _type) {
            foreach (string key in ReplaceStrActions[_type].Keys) {
                if (ReplaceStrs[_type].ContainsKey(key)) {
                    DebugLogger.LogErrorFormat("StringReplacer Type:{0} Key{1}重複", _type, key);
                }
            }
        }
        public static string GetReplacedStr(StringReplaceType _type, string _str) {
            //Key值重複時會先以ReplaceStrActions為主
            if (ReplaceStrActions.ContainsKey(_type) && ReplaceStrActions[_type] != null) {
                foreach (string key in ReplaceStrActions[_type].Keys) {
                    ReplaceStrActions[_type][key]?.Invoke(ref _str);
                }
            }
            if (ReplaceStrs.ContainsKey(_type) && ReplaceStrs[_type] != null) {
                foreach (string key in ReplaceStrs[_type].Keys) {
                    _str = _str.Replace(key, ReplaceStrs[_type][key]);
                }
            }
            return _str;
        }
    }
}