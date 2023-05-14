using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
namespace TheDoor.Main {
    public partial class PlayerRole : Role {
        public OwnedRoleData MyOwnedRoleData;

        public override int MaxHP {
            get {
                int value = BaseHP;
                value += GetExtraHPBySupply();
                return value;
            }
        }
        int curHP;
        public override int CurHP {
            get { return curHP; }
            protected set { curHP = Mathf.Clamp(value, 0, MaxHP); }
        }

        public override int MaxSanP {
            get {
                int value = BaseSanP;
                value += GetExtraSanPBySupply();
                return value;
            }
        }
        int curSanP;
        public override int CurSanP {
            get { return curSanP; }
            protected set { curSanP = Mathf.Clamp(value, 0, MaxSanP); }
        }

        public RoleData MyData { get; private set; }
        public override string Name { get { return MyData.Name; } }
        public override string Ref { get { return MyData.Ref; } }
        public List<TalentData> Talents { get; private set; }


        public void UpdateToOwnedRoleData() {
            MyOwnedRoleData.UpdateOwnedRoleData(this);
        }

        public override void AddHP(int _value) {
            base.AddHP(_value);
            if (RoleStateUI.Instance != null) {
                if (_value <= 0) DNPManager.Instance.Spawn(DNPManager.DPNType.Dmg, _value, RoleStateUI.Instance.DNPTrans, Vector2.zero);
                else DNPManager.Instance.Spawn(DNPManager.DPNType.Restore, _value, RoleStateUI.Instance.DNPTrans, Vector2.zero);
            }

            RoleStateUI.Instance?.RefreshState();
        }




        public override void AddSanP(int _value) {
            if (IsDead) return;
            base.AddSanP(_value);
            CurSanP += _value;
            WriteLog.LogColor(Name + "SanP增加:" + _value, WriteLog.LogType.Battle);
            if (RoleStateUI.Instance != null) {
                if (_value <= 0) DNPManager.Instance.Spawn(DNPManager.DPNType.SanDmg, _value, BattleUI.GetTargetRectTrans(this), Vector2.zero);
                else DNPManager.Instance.Spawn(DNPManager.DPNType.SanRestore, _value, BattleUI.GetTargetRectTrans(this), Vector2.zero);
            }
            if (IsDead) OnDeath();
            RoleStateUI.Instance?.RefreshState();
        }
        public override void GetAttacked(int _dmg) {
            base.GetAttacked(_dmg);
            //特效
            GameObjSpawner.SpawnParticleObjByPath("screenBlood", AdventureUI.Instance?.transform);
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

            //特效
            GameObjSpawner.SpawnParticleObjByPath("screenBlood", AdventureUI.Instance?.transform);
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

            //跳文字
            if (MyEnum.TryParseEnum(_effect.MyType.ToString(), out DNPManager.DPNType _type)) {
                DNPManager.Instance.Spawn(_type, _effect.Stack, BattleUI.GetTargetRectTrans(this), Vector2.zero);
            }
            //刷新UI
            RoleStateUI.Instance.RefreshEffect();
        }




        public class Builder : Builder<PlayerRole> {
            public Builder SetData(OwnedRoleData _ownedRoleData, RoleData _data) {
                instance.MyData = _data;
                instance.MyOwnedRoleData = _ownedRoleData;
                List<TalentData> talents = new List<TalentData>();
                foreach (var talentID in _ownedRoleData.Talents) {
                    var talentData = TalentData.GetData(talentID);
                    talents.Add(talentData);
                }
                instance.Talents = talents;
                instance.BaseHP = _data.HP;
                instance.BaseSanP = _data.SanP;
                return this;
            }
            public Builder SetCurHP(int _curHP) {
                instance.CurHP = _curHP;
                return this;
            }
            public Builder SetCurSanP(int _curSanP) {
                instance.CurSanP = _curSanP;
                return this;
            }

        }

    }
}