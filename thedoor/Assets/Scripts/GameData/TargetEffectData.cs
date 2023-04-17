using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public enum TargetEffectType {
        HP,//  生命
        SanP,//  理智
        Stun,//    造成暈眩
        Bleed,//   造成流血
        Flee,//    逃離戰鬥
    }
    public class TargetEffectData {

        public Target MyTarget { get; private set; }
        public TargetEffectType EffectType { get; private set; }
        string value;
        public float Value {
            get {
                return 1;
            }
        }
        public float Probability { get; private set; }

        /// <summary>
        /// 設定目標效果資料
        /// </summary>
        /// <param name="_target">目標類型</param>
        /// <param name="_type">效果類型</param>
        /// <param name="_value">效果參數</param>
        /// <param name="_probability">觸發機率</param>
        public TargetEffectData(Target _target, TargetEffectType _type, string _value, float _probability) {
            MyTarget = _target;
            EffectType = _type;
            value = _value;
            Probability = _probability;
        }
    }
}