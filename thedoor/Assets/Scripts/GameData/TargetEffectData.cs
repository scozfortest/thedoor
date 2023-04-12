using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public enum TargetEffectType {
        Health,//  恢復生命
        Sanity,//  恢復理智
        Stun,//    造成暈眩
        Bleed,//   造成流血
        Flee,//    逃離戰鬥
    }
    public class TargetEffectData {

        public TargetEffectType EffectType { get; private set; }
        string value;
        public float Value {
            get {
                return 1;
            }
        }
        public float Probability { get; private set; }

        public TargetEffectData(TargetEffectType _type, string _value, float _probability) {
            EffectType = _type;
            value = _value;
            Probability = _probability;
        }
    }
}