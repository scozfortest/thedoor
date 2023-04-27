using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedRoleData : OwnedData {
        [ScozSerializable] public int ID { get; private set; }
        [ScozSerializable] public int CurHP { get; private set; }
        [ScozSerializable] public int CurSanP { get; private set; }
        [ScozSerializable] public List<string> Talents { get; private set; } = new List<string>();
        [ScozSerializable] public Dictionary<EffectType, List<float>> Effects { get; private set; } = new Dictionary<EffectType, List<float>>();

        public OwnedAdventureData MyAdventure {
            get {
                return GamePlayer.Instance.GetOwnedData<OwnedAdventureData>(ColEnum.Adventure, UID);
            }
        }

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
            Talents.Clear();
            List<object> talentObjs = _data.TryGetValue("Talent", out value) ? value as List<object> : null;
            if (talentObjs != null)
                Talents = talentObjs.OfType<string>().ToList();

            //設定狀態清單
            Effects.Clear();
            Dictionary<string, object> effectDic = _data.TryGetValue("Effect", out value) ? DicExtension.ConvertToStringKeyDic(value) : null;
            if (effectDic != null) {
                foreach (var key in effectDic.Keys) {
                    if (MyEnum.TryParseEnum(key, out EffectType _type)) {
                        List<object> valueObjs = effectDic[key] as List<object>;
                        List<float> values = valueObjs.ToFloatList();
                        Effects.Add(_type, values);
                    }
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

        public List<OwnedSupplyData> GetSupplyDatas() {
            List<OwnedSupplyData> supplyDatas = GamePlayer.Instance.GetOwnedDatas<OwnedSupplyData>(ColEnum.Supply);
            supplyDatas = supplyDatas.FindAll(a => a.OwnRoleUID == UID);
            return supplyDatas;
        }

    }
}
