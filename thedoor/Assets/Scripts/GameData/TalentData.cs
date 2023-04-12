using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public enum TalentType {
        Investigation,
        HideAndSeek,
    }
    public class TalentData {

        public TalentType Type { get; private set; }
        string value;
        public float Value {
            get {
                return 1;
            }
        }
        public float Probability { get; private set; }

        public TalentData(TalentType _type, string _value) {
            Type = _type;
            value = _value;
        }
    }
}