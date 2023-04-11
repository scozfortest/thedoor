using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func {
    public class DoLimitTimes {
        static Dictionary<string, int> DoCountDic = new Dictionary<string, int>();
        public static bool DoCheck(string _doType, int _limit) {
            if (DoCountDic.ContainsKey(_doType)) {
                if (DoCountDic[_doType] >= _limit) return false;
                else {
                    DoCountDic[_doType]++;
                    return true;
                }
            } else {
                DoCountDic[_doType] = 1;
                return true;
            }
        }
    }
}