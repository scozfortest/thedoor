using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedRoleData : OwnedData {
        public int RoleID { get; private set; }
        public int CurHP { get; private set; }
        public int CurSanP { get; private set; }
        public List<string> Talents = new List<string>();
        public Dictionary<TargetEffectType, List<float>> Effects = new Dictionary<TargetEffectType, List<float>>();

        public OwnedRoleData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            RoleID = _data.TryGetValue("RoleID", out value) ? Convert.ToInt32(value) : default(int);
            CurHP = _data.TryGetValue("CurHP", out value) ? Convert.ToInt32(value) : default(int);
            CurSanP = _data.TryGetValue("CurSanP", out value) ? Convert.ToInt32(value) : default(int);
            //設定Talent清單
            List<object> talentObjs = _data.TryGetValue("Talent", out value) ? value as List<object> : null;
            Talents = talentObjs.OfType<string>().ToList();

            //設定狀態清單
            Dictionary<string, object> effectDic = _data.TryGetValue("Effect", out value) ? DictionaryExtension.ConvertToStringKeyDic(value) : null;
            foreach (var key in effectDic.Keys) {
                if (MyEnum.TryParseEnum(key, out TargetEffectType _type)) {
                    List<object> valueObjs = effectDic[key] as List<object>;
                    List<float> values = valueObjs.ToFloatList();
                    Effects.Add(_type, values);
                }
            }
        }

    }
}
