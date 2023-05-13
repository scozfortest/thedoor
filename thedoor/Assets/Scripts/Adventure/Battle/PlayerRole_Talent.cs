using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
namespace TheDoor.Main {
    public partial class PlayerRole {
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