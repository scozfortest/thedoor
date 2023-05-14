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
        [ScozSerializable] public Dictionary<EffectType, int> Effects { get; private set; } = new Dictionary<EffectType, int>();

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
            List<object> talentObjs = _data.TryGetValue("Talents", out value) ? value as List<object> : null;
            if (talentObjs != null)
                Talents = talentObjs.OfType<string>().ToList();
            //設定狀態清單
            Effects.Clear();
            Dictionary<string, object> effectDic = _data.TryGetValue("Effect", out value) ? DicExtension.ConvertToStringKeyDic(value) : null;
            if (effectDic != null) {
                foreach (var key in effectDic.Keys) {
                    if (MyEnum.TryParseEnum(key, out EffectType _type)) {
                        int typeValue = Convert.ToInt32(effectDic[key]);
                        Effects.Add(_type, typeValue);
                    }
                }
            }

        }
        public void UpdateOwnedRoleData(PlayerRole _pRole) {
            CurHP = _pRole.CurHP;
            CurSanP = _pRole.CurSanP;
            List<string> ids = new List<string>();
            foreach (var data in _pRole.Talents) {
                ids.Add(data.ID);
            }
            Talents = ids;
            Effects.Clear();
            foreach (var key in _pRole.Effects.Keys) {
                if (_pRole.Effects[key].Stack <= 0) continue;
                Effects.Add(key, _pRole.Effects[key].Stack);
            }

        }

        public List<TalentData> GetTalentDatas() {
            if (Talents == null || Talents.Count == 0) return null;
            List<TalentData> talents = new List<TalentData>();
            for (int i = 0; i < Talents.Count; i++) {
                var talentData = TalentData.GetData(Talents[i]);
                if (talentData == null) continue;
                talents.Add(talentData);
            }
            if (talents.Count == 0) return null;
            return talents;
        }
        public List<TargetEffectData> GetEffectDatas() {
            if (Effects == null || Effects.Count == 0) return null;
            List<TargetEffectData> effects = new List<TargetEffectData>();
            foreach (var effectType in Effects.Keys) {
                var effectData = new TargetEffectData(Target.Myself, effectType, 1, Effects[effectType]);
                effects.Add(effectData);
            }
            return effects;
        }

        public List<OwnedSupplyData> GetSupplyDatas(HashSet<string> _exclusiveTags, params SupplyData.Timing[] _timings) {
            List<OwnedSupplyData> supplyDatas = GamePlayer.Instance.GetOwnedDatas<OwnedSupplyData>(ColEnum.Supply);
            supplyDatas = supplyDatas.FindAll(a => {
                if (a.OwnRoleUID != UID)
                    return false;
                var supplyData = SupplyData.GetData(a.ID);
                for (int i = 0; i < _timings.Length; i++) {
                    if (!supplyData.BelongToTiming(_timings[i]))
                        return false;
                }
                if (supplyData.ContainTags(_exclusiveTags))
                    return false;
                return true;
            });

            return supplyDatas;
        }
        List<OwnedSupplyData> GetUnarmedSupplyDatas() {
            List<OwnedSupplyData> supplyDatas = new List<OwnedSupplyData>();
            var roleData = RoleData.GetData(ID);
            foreach (var id in roleData.Unarmeds) {
                var supplyData = SupplyData.GetData(id);
                if (supplyData == null) continue;
                Dictionary<string, object> supplyDataDic = new Dictionary<string, object>();
                supplyDataDic.Add("UID", GamePlayer.Instance.GetNextUID("Supply"));
                supplyDataDic.Add("OwnerUID", GamePlayer.Instance.Data.UID);
                supplyDataDic.Add("CreateTime", GameManager.Instance.NowTime);

                supplyDataDic.Add("ID", id);
                supplyDataDic.Add("Usage", -1);
                supplyDataDic.Add("OwnRoleUID", UID);
                var ownedSupplyData = new OwnedSupplyData(supplyDataDic);
                supplyDatas.Add(ownedSupplyData);
            }
            return supplyDatas;
        }

    }
}
