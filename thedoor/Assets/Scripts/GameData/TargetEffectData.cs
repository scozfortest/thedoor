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
        public float Probability { get; private set; }
        float[] Values;

        /// <summary>
        /// 設定目標效果資料
        /// </summary>
        /// <param name="_target">目標類型</param>
        /// <param name="_type">效果類型</param>
        /// <param name="_probability">觸發機率</param>
        /// <param name="_values">效果參數陣列</param>
        public TargetEffectData(Target _target, TargetEffectType _type, float _probability, params float[] _values) {
            MyTarget = _target;
            EffectType = _type;
            if (_values != null && _values.Length > 0)
                Values = _values;
            Probability = _probability;
        }
    }
}