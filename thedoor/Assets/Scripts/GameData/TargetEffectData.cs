using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public class TargetEffectData {

        public Target MyTarget { get; private set; }
        public EffectType EffectType { get; private set; }
        public float Probability { get; private set; }
        public int Value { get; private set; }

        /// <summary>
        /// 設定目標效果資料
        /// </summary>
        public TargetEffectData(Target _target, EffectType _type, float _probability, int _value) {
            MyTarget = _target;
            EffectType = _type;
            Value = _value;
            Probability = _probability;
        }
        public string Description {
            get {
                string description = "";
                switch (EffectType) {
                    case EffectType.Bleeding:
                        description = string.Format(StringData.GetUIString(EffectType.ToString()), Value);
                        break;
                    case EffectType.Dizzy:
                        description = string.Format(StringData.GetUIString(EffectType.ToString()), Value);
                        break;
                    default:
                        description = StringData.GetUIString(EffectType.ToString());
                        break;
                }
                return description;
            }
        }
    }
}