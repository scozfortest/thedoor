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

        public EnemyAction AddNewAction() {
            var action = MyData.GetAction(this, BattleManager.PRole);
            if (action == null) return null;
            Actions.Add(action);
            return action;
        }

        void ScheduleNewActions() {
            int needCount = GameSettingData.GetInt(GameSetting.Monster_ScheduleActionCount);
            int addCount = needCount - Actions.Count;
            for (int i = 0; i < addCount; i++) {
                var action = MyData.GetAction(this, BattleManager.PRole);
                if (action == null) continue;
                Actions.Add(action);
            }
        }

        public override void AddHP(int _value) {
            base.AddHP(_value);
            if (_value <= 0) {
                DNPManager.Instance.Spawn(DNPManager.DPNType.Dmg, _value, EnemyUI.Instance.GetComponent<RectTransform>(), Vector2.zero);
            }
            EnemyUI.Instance?.RefreshUI();
        }



        protected override void OnDeath() {
            base.OnDeath();
        }
        public class Builder : Role.Builder<EnemyRole> {
            public Builder SetData(MonsterData _data) {
                WriteLog.LogColor("產生怪物:" + _data.Name, WriteLog.LogType.Battle);
                instance.MyData = _data;
                instance.ScheduleNewActions();
                return this;
            }
        }
    }
}