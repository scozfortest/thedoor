using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
namespace TheDoor.Main {
    public partial class PlayerRole {




        /// <summary>
        /// 獲得時道具附加狀態
        /// </summary>
        public void GainSupplyPassiveEffects(List<SupplyData> _datas) {
            foreach (var supplyData in _datas) {
                if (supplyData.PassiveEffect == null || StatusEffect.OnlyInBattle(supplyData.PassiveEffect.EffectType)) continue;
                StatusEffect se = EffectFactory.Create(1, supplyData.PassiveEffect.EffectType, supplyData.PassiveEffect.Value, this, this, AttackPart.Body);
                ApplyEffect(se);
            }
        }


        /// <summary>
        /// 獲得道具增加生命/新智最大值的時候也會同時增加目前值
        /// </summary>
        public void AddSupplyExtendAttribute(List<SupplyData> _datas) {
            int addHP = 0;
            int addSanP = 0;
            foreach (var supplyData in _datas) {
                addHP += supplyData.ExtendHP;
                addSanP += supplyData.ExtendSanP;
            }
            if (addHP > 0)
                AddHP(addHP);
            if (addSanP > 0)
                AddSanP(addSanP);
        }



        /// <summary>
        /// 生命獲得道具加成
        /// </summary>
        public int GetExtraHPBySupply() {
            int value = 0;
            foreach (var supply in MyOwnedRoleData.GetSupplyDatas(null)) {
                var supplyData = SupplyData.GetData(supply.ID);
                value += supplyData.ExtendHP;
            }
            return value;
        }
        /// <summary>
        /// 神智獲得道具加成
        /// </summary>
        public int GetExtraSanPBySupply() {
            int value = 0;
            foreach (var supply in MyOwnedRoleData.GetSupplyDatas(null)) {
                var supplyData = SupplyData.GetData(supply.ID);
                value += supplyData.ExtendSanP;
            }
            return value;
        }

        /// <summary>
        /// 攻擊獲得天賦加成
        /// </summary>
        public int GetAttackExtraValueByTalent(int _dmg) {
            int value = 0;
            foreach (var id in MyOwnedRoleData.Talents) {
                var talentData = TalentData.GetData(id);
                value += talentData.GetAttackExtraValue(_dmg);
            }
            return value;
        }
        /// <summary>
        /// 休息室可供選擇的道具天賦加成
        /// </summary>
        public int GetSearchExtraSupplyByTalent() {
            int value = 0;
            foreach (var id in MyOwnedRoleData.Talents) {
                var talentData = TalentData.GetData(id);
                value += talentData.GetSearchExtraSupply();
            }
            return value;
        }
        /// <summary>
        /// 休息室恢復生命天賦加成
        /// </summary>
        public int GetRestExtraHPRestoreByTalent() {
            int value = 0;
            foreach (var id in MyOwnedRoleData.Talents) {
                var talentData = TalentData.GetData(id);
                value += talentData.GetRestExtraHPRestore(this);
            }
            return value;
        }
        /// <summary>
        /// 休息室恢復心智天賦加成
        /// </summary>
        public int GetRestExtraSanPRestoreByTalent() {
            int value = 0;
            foreach (var id in MyOwnedRoleData.Talents) {
                var talentData = TalentData.GetData(id);
                value += talentData.GetRestExtraSanPRestore(this);
            }
            return value;
        }


    }
}