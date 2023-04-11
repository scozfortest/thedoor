using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {

    /// <summary>
    /// Firestore上的GameData-Setting集合中的設定
    /// </summary>
    public class FirestoreGameSetting {

        static Dictionary<GameDataDocEnum, Dictionary<string, object>> Datas { get; set; } = new Dictionary<GameDataDocEnum, Dictionary<string, object>>();
        public static List<TriggerEventData> TriggerEvents = new List<TriggerEventData>();
        public static List<string> RecordableEvents = new List<string>();

        public static void UpdateSetting(Dictionary<string, object> _data) {
            object value;
            string uid = _data.TryGetValue(OwnedEnum.UID.ToString(), out value) ? Convert.ToString(value) : default(string);
            GameDataDocEnum docEnum;
            if (MyEnum.TryParseEnum(uid, out docEnum)) {
                switch (docEnum) {
                    case GameDataDocEnum.TriggerEvent:
                        SetTriggerEventDatas(_data);
                        break;
                    default:
                        if (!Datas.ContainsKey(docEnum)) {
                            Datas.Add(docEnum, _data);
                        } else
                            Datas[docEnum] = _data;
                        break;
                }

                UpdateHandle(docEnum);
            }
        }
        /// <summary>
        /// 設定觸發事件
        /// </summary>
        static void SetTriggerEventDatas(Dictionary<string, object> _data) {
            TriggerEvents.Clear();
            RecordableEvents.Clear();
            List<object> objs = _data["RecordableEvents"] as List<object>;
            RecordableEvents = objs.ConvertAll(a => a.ToString());
            objs = _data["TriggerEvents"] as List<object>;
            if (objs == null) return;
            for (int i = 0; i < objs.Count; i++) {
                try {
                    Dictionary<string, object> triggerEventDic = DictionaryExtension.ConvertToStringKeyDic(objs[i] as IDictionary);
                    TriggerEvents.Add(new TriggerEventData(triggerEventDic));
                } catch (Exception _ex) {
                    DebugLogger.LogError("GameData-Setting>TriggerEvent資料錯誤: " + _ex);
                }
            }
            if (TriggerEvents != null || TriggerEvents.Count != 0)
                TriggerEvents = TriggerEvents.OrderByDescending(a => a.Priority).ToList();
        }

        /// <summary>
        /// 當DB資料更新後會根據不同資料有特別的處理放這裡
        /// </summary>
        static void UpdateHandle(GameDataDocEnum _docEnum) {
            switch (_docEnum) {
                case GameDataDocEnum.Version:
                    if (GamePlayer.Instance.Data != null) GameStateManager.Instance.InGameCheckCanPlayGame();//檢測是否可繼續遊戲
                    break;
                case GameDataDocEnum.ScheduledInGameNotification:
                    GameStateManager.Instance.InGameCheckScheduledInGameNotification();//檢測是否跳出遊戲內推播
                    break;
            }
        }

        public static bool GetBoolData(GameDataDocEnum _docName, string _key) {
            if (!Datas.ContainsKey(_docName)) {
                DebugLogger.LogErrorFormat("GameData-Setting無此Doc名稱: {0}", _docName);
                return false;
            }
            object value;
            bool returnValue = Datas[_docName].TryGetValue(_key, out value) ? Convert.ToBoolean(value) : false;
            return returnValue;
        }
        public static string GetStrData(GameDataDocEnum _docName, string _key) {
            if (!Datas.ContainsKey(_docName)) {
                DebugLogger.LogErrorFormat("GameData-Setting無此Doc名稱: {0}", _docName);
                return "";
            }
            object value;
            string returnValue = Datas[_docName].TryGetValue(_key, out value) ? Convert.ToString(value) : default(string);
            return returnValue;
        }
        public static int GetIntData(GameDataDocEnum _docName, string _key) {
            if (!Datas.ContainsKey(_docName)) {
                DebugLogger.LogErrorFormat("GameData-Setting無此Doc名稱: {0}", _docName);
                return default(int);
            }
            object value;
            int returnValue = Datas[_docName].TryGetValue(_key, out value) ? Convert.ToInt32(value) : default(int);
            return returnValue;
        }
        public static float GetFloatData(GameDataDocEnum _docName, string _key) {
            if (!Datas.ContainsKey(_docName)) {
                DebugLogger.LogErrorFormat("GameData-Setting無此Doc名稱: {0}", _docName);
                return default(float);
            }
            object value;
            float returnValue = Datas[_docName].TryGetValue(_key, out value) ? (float)Convert.ToDouble(value) : default(float);
            return returnValue;
        }
        public static DateTime GetDateTime(GameDataDocEnum _docName, string _key) {
            if (!Datas.ContainsKey(_docName)) {
                DebugLogger.LogErrorFormat("GameData-Setting無此Doc名稱: {0}", _docName);
                return default(DateTime);
            }
            object value;
            DateTime time = Datas[_docName].TryGetValue(_key, out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
            return time;
        }
        public static List<string> GetStrs(GameDataDocEnum _docName, string _key) {
            if (!Datas.ContainsKey(_docName)) {
                DebugLogger.LogErrorFormat("GameData-Setting無此Doc名稱: {0}", _docName);
                return null;
            }
            object value;
            List<object> objs = Datas[_docName].TryGetValue(_key, out value) ? value as List<object> : new List<object>();
            if (objs == null)
                return new List<string>();
            return objs.Cast<string>().ToList();
        }
        public static List<int> GetInts(GameDataDocEnum _docName, string _key) {
            if (!Datas.ContainsKey(_docName)) {
                DebugLogger.LogErrorFormat("GameData-Setting無此Doc名稱: {0}", _docName);
                return null;
            }
            object value;
            List<object> objs = Datas[_docName].TryGetValue(_key, out value) ? value as List<object> : null;
            if (objs == null)
                return null;
            return objs.ConvertAll(a => Convert.ToInt32(a)).ToList();
        }
        public static List<float> GetFloats(GameDataDocEnum _docName, string _key) {
            if (!Datas.ContainsKey(_docName)) {
                DebugLogger.LogErrorFormat("GameData-Setting無此Doc名稱: {0}", _docName);
                return null;
            }
            object value;
            List<object> objs = Datas[_docName].TryGetValue(_key, out value) ? value as List<object> : null;
            if (objs == null)
                return null;
            return objs.ConvertAll(a => (float)Convert.ToDouble(a)).ToList();
        }
        public static Dictionary<string, object> GetDicData(GameDataDocEnum _docName) {
            if (!Datas.ContainsKey(_docName)) {
                DebugLogger.LogErrorFormat("GameData-Setting無此Doc名稱: {0}", _docName);
                return null;
            }
            return Datas[_docName];
        }
    }
}
