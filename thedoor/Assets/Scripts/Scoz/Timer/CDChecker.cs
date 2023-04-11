using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func {

    public class CDChecker : MonoBehaviour {
        static Dictionary<string, DateTime> ColdDownDic = new Dictionary<string, DateTime>();
        /// <summary>
        /// 傳入ID與CD秒數，回傳是否已經CD結束，如果是就反回true並更新時間，還在CD就回傳false
        /// </summary>
        public static bool DoneCD(string _id, float _secs) {
            if (ColdDownDic.ContainsKey(_id)) {
                if (GameManager.Instance.NowTime >= ColdDownDic[_id].AddSeconds(_secs)) {
                    ColdDownDic[_id] = GameManager.Instance.NowTime;
                    return true;
                } else {

                    return false;
                }
            }
            ColdDownDic[_id] = GameManager.Instance.NowTime;
            return true;
        }
        public static bool DoneCD(int _id, float _secs) {
            return DoneCD(_id.ToString(), _secs);
        }
        /// <summary>
        /// 取得CD剩餘秒數
        /// </summary>
        public static float GetRemainSecs(string _id, float _secs) {
            if (ColdDownDic.ContainsKey(_id)) {
                if (GameManager.Instance.NowTime >= ColdDownDic[_id].AddSeconds(_secs)) {
                    return 0;
                } else {
                    TimeSpan span = ColdDownDic[_id].AddSeconds(_secs) - GameManager.Instance.NowTime;
                    return (float)span.TotalSeconds;
                }
            }
            return 0;
        }
    }
}