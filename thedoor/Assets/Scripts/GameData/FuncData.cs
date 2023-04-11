using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public enum FuncType {
        TestFunc,
    }
    public class FuncData {
        public FuncType Type { get; private set; }
        public int Value { get; private set; }
        public FuncData(FuncType _type, int _value) {
            Type = _type;
            Value = _value;
        }
    }
}
