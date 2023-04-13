using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public enum RequireType {
        Gold,
        Point,
        Supply,//需求擁有某道具
        UseSupply,//需求使用某道具1次
        ConsumeSupply,//需求消耗某道具(該道具會消失)
        SupplyTag,//需求擁有某道具類型
        UseSupplyTag,//需求使用某道具類型1次
        ConsumeSupplyTag,//需求消耗某道具類型(擁有的該類型道具全部都會消失)
    }
    public class RequireData {

        public RequireType Type { get; private set; }
        string value;
        public float Value {
            get {
                return 1;
            }
        }
        public float Probability { get; private set; }

        public RequireData(RequireType _type, string _value) {
            Type = _type;
            value = _value;
        }
    }
}