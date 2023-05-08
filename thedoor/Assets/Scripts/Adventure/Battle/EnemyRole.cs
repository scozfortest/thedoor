using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class EnemyRole : Role {

        public MonsterData MyData { get; private set; }
        public override string Name { get { return MyData.Name; } }
        public override string Ref { get { return MyData.Ref; } }

        public List<EnemyAction> Actions { get; private set; } = new List<EnemyAction>();

        public int CurActionIndex { get; private set; }

        public EnemyAction AddNewAction() {
            var action = MyData.GetAction(this, BattleManager.PRole, GetLastAction());
            if (action == null) return null;
            Actions.Add(action);
            return action;
        }
        EnemyAction GetLastAction() {
            return Actions.Count > 0 ? Actions[Actions.Count - 1] : null;
        }
        public EnemyAction GetCurAction() {
            if (CurActionIndex >= Actions.Count) return null;
            return Actions[CurActionIndex];
        }
        public void AddActionIndex() {
            CurActionIndex++;
        }

        void ScheduleNewActions() {
            int needCount = GameSettingData.GetInt(GameSetting.Monster_ScheduleActionCount);
            int addCount = needCount - Actions.Count;
            for (int i = 0; i < addCount; i++) {
                var action = MyData.GetAction(this, BattleManager.PRole, GetLastAction());
                if (action == null) continue;
                Actions.Add(action);
            }
        }

        public override void AddHP(int _value) {
            base.AddHP(_value);
            if (_value <= 0)
                DNPManager.Instance.Spawn(DNPManager.DPNType.Dmg, _value, BattleUI.GetTargetRectTrans(this), Vector2.zero);
            else
                DNPManager.Instance.Spawn(DNPManager.DPNType.Restore, _value, BattleUI.GetTargetRectTrans(this), Vector2.zero);

            EnemyUI.Instance?.RefreshUI();
        }

        public override void ApplyEffect(StatusEffect _effect) {
            base.ApplyEffect(_effect);

            //如果是暈眩要改變第一個行動需求的時間 並 將更新時間軸UI
            GetCurAction().AddNeedTime(_effect.Stack);
            if (_effect.MyType == EffectType.Dizzy) {
                TimelineBattleUI.Instance.PassTime(-_effect.Stack, () => {
                });
            }

            if (MyEnum.TryParseEnum(_effect.MyType.ToString(), out DNPManager.DPNType _type)) {
                DNPManager.Instance.Spawn(_type, _effect.Stack, BattleUI.GetTargetRectTrans(this), Vector2.zero);
            }
        }



        protected override void OnDeath() {
            base.OnDeath();
        }
        public class Builder : Role.Builder<EnemyRole> {
            public Builder SetData(MonsterData _data) {
                WriteLog.LogColor("產生怪物:" + _data.Name, WriteLog.LogType.Battle);
                instance.MyData = _data;
                instance.CurActionIndex = 0;
                instance.ScheduleNewActions();
                return this;
            }
        }
    }
}