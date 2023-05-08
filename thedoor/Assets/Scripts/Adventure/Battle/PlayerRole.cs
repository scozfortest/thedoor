using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
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
            base.AddHP(_value);
            if (_value <= 0)
                DNPManager.Instance.Spawn(DNPManager.DPNType.Dmg, _value, RoleStateUI.Instance.DNPTrans, Vector2.zero);
            else
                DNPManager.Instance.Spawn(DNPManager.DPNType.Restore, _value, RoleStateUI.Instance.DNPTrans, Vector2.zero);
            RoleStateUI.Instance.RefreshState();
        }




        public void AddSanP(int _value) {
            if (IsDead) return;
            CurSanP += _value;
            WriteLog.LogColor(Name + "SanP增加:" + _value, WriteLog.LogType.Battle);
            if (_value <= 0)
                DNPManager.Instance.Spawn(DNPManager.DPNType.SanDmg, _value, BattleUI.GetTargetRectTrans(this), Vector2.zero);
            else
                DNPManager.Instance.Spawn(DNPManager.DPNType.SanRestore, _value, BattleUI.GetTargetRectTrans(this), Vector2.zero);
            if (IsDead) OnDeath();
            RoleStateUI.Instance.RefreshState();
        }
        public override void GetAttacked(int _dmg) {
            base.GetAttacked(_dmg);
            CoroutineJob.Instance.StartNewAction(() => {
                CameraManager.ShakeCam(CameraManager.CamNames.Adventure, 0.5f, 1, 0.2f);
            }, 0.2f);
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
            CoroutineJob.Instance.StartNewAction(() => {
                CameraManager.ShakeCam(CameraManager.CamNames.Adventure, 0.5f, 1, 0.2f);
            }, 0.2f);
        }
        protected override void OnDeath() {
            base.OnDeath();
        }
        public override void ApplyEffect(StatusEffect _effect) {
            base.ApplyEffect(_effect);

            if (MyEnum.TryParseEnum(_effect.MyType.ToString(), out DNPManager.DPNType _type)) {
                DNPManager.Instance.Spawn(_type, _effect.Stack, BattleUI.GetTargetRectTrans(this), Vector2.zero);
            }

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