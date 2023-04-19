using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedRoleData : OwnedData {
        public int ID { get; private set; }
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
            ID = _data.TryGetValue("ID", out value) ? Convert.ToInt32(value) : default(int);
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


        public List<TalentData> GetTalentDatas() {
            if (Talents == null || Talents.Count == 0) return null;
            List<TalentData> talents = new List<TalentData>();
            for (int i = 0; i < Talents.Count; i++) {
                var talentData = TalentData.GetData(Talents[i]);
                if (talentData != null) {
                    talents.Add(talentData);
                }
            }
            if (talents.Count == 0) return null;
            return talents;
        }
        public List<TargetEffectData> GetEffectDatas() {
            if (Effects == null || Effects.Count == 0) return null;
            List<TargetEffectData> effects = new List<TargetEffectData>();
            foreach (var effectType in Effects.Keys) {
                var effectData = new TargetEffectData(Target.Myself, effectType, 1, Effects[effectType].ToArray());
                effects.Add(effectData);
            }
            return effects;
        }

    }
}
