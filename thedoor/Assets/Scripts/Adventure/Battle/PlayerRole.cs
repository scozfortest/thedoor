using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
namespace TheDoor.Main {
    public class PlayerRole : Role {

        public RoleData MyData { get; private set; }
        public override string Name { get { return MyData.Name; } }
        public override string Ref { get { return MyData.Ref; } }

        public override void AddHP(int _value) {
            base.AddHP(_value);
            if (_value <= 0)
                DNPManager.Instance.Spawn(DNPManager.DPNType.Dmg, _value, RoleStateUI.Instance.DNPTrans, Vector2.zero);
            else
                DNPManager.Instance.Spawn(DNPManager.DPNType.Restore, _value, RoleStateUI.Instance.DNPTrans, Vector2.zero);
            RoleStateUI.Instance.RefreshState();
        }




        public override void AddSanP(int _value) {
            if (IsDead) return;
            base.AddSanP(_value);
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
            //鏡頭演出
            CoroutineJob.Instance.StartNewAction(() => {
                CameraManager.ShakeCam(CameraManager.CamNames.Adventure, 0.5f, 1, 0.2f);
            }, 0.1f);
        }

        /// <summary>
        /// 承受神智攻擊
        /// </summary>
        public override void GetSanAttacked(int _dmg) {
            if (IsDead) return;
            base.GetSanAttacked(_dmg);

            AddSanP(-_dmg);

            //鏡頭演出
            CoroutineJob.Instance.StartNewAction(() => {
                CameraManager.ShakeCam(CameraManager.CamNames.Adventure, 0.5f, 1, 0.2f);
            }, 0.1f);
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
        }

    }
}