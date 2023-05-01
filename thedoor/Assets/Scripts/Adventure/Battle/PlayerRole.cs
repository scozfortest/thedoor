using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class PlayerRole : Role {

        public RoleData MyData { get; private set; }
        public override string Name { get { return MyData.Name; } }
        public override string Ref { get { return MyData.Ref; } }
        public int MaxSanP { get; protected set; }
        private int curSanP;
        public int CurSanP {
            get { return curSanP; }
            protected set { curSanP = Mathf.Clamp(value, 0, MaxSanP); }
        }
        public float SanPRatio { get { return (float)CurSanP / (float)MaxSanP; } }
        public override bool IsDead { get { return (CurHP <= 0 || CurSanP <= 0); } }

        public override void AddHP(int _value) {
            Debug.LogError("prole get dmg=" + _value);
            base.AddHP(_value);
            RoleStateUI.Instance.RefreshState();
        }

        public void AddSanP(int _value) {
            if (IsDead) return;
            CurSanP += _value;
            if (IsDead) OnDeath();
            RoleStateUI.Instance.RefreshState();
        }
        /// <summary>
        /// 承受神智攻擊
        /// </summary>
        public void GetSanAttacked(int _dmg) {
            if (_dmg == 0) return;
            if (IsDead) return;

            //執行效果
            foreach (var effect in Effects.Values) {
                _dmg -= effect.BeAtteckedExtraSanDmgTaken();
                _dmg += effect.BeAttackSanDamageReduction(_dmg);
            }
            if (_dmg > 0) _dmg = 0;
            AddSanP(_dmg);
            RemoveExpiredEffects();
        }
        protected override void OnDeath() {
            base.OnDeath();
            Debug.Log("玩家已死亡");
        }



        public class Builder : Builder<PlayerRole> {
            public Builder SetData(RoleData _data) {
                instance.MyData = _data;
                return this;
            }
            public Builder SetCurSanP(int _curSanP) {
                instance.CurSanP = _curSanP;
                return this;
            }
            public Builder SetMaxSanP(int _maxSanP) {
                instance.MaxSanP = _maxSanP;
                return this;
            }
        }

    }
}