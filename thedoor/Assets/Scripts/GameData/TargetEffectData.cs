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
        float[] Values;
        public float GetValue(int _index) {
            if (_index < Values.Length) return Values[_index];
            return 0;
        }
        public bool IsBuff {
            get {
                switch (EffectType) {
                    case EffectType.HP:
                    case EffectType.SanP:
                        return Values[0] > 0;
                    case EffectType.Dizzy:
                    case EffectType.Bleeding:
                        return Values[0] < 0;
                    default:
                        return true;
                }
            }
        }

        /// <summary>
        /// 設定目標效果資料
        /// </summary>
        /// <param name="_target">目標類型</param>
        /// <param name="_type">效果類型</param>
        /// <param name="_probability">觸發機率</param>
        /// <param name="_values">效果參數陣列</param>
        public TargetEffectData(Target _target, EffectType _type, float _probability, params float[] _values) {
            MyTarget = _target;
            EffectType = _type;
            if (_values != null && _values.Length > 0)
                Values = _values;
            Probability = _probability;
        }
        public string Description {
            get {
                string description = "";
                switch (EffectType) {
                    case EffectType.Bleeding:
                        description = string.Format(StringData.GetUIString(EffectType.ToString()), Values[0]);
                        break;
                    case EffectType.Dizzy:
                        description = string.Format(StringData.GetUIString(EffectType.ToString()), Values[0]);
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