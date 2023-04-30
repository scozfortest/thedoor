using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class EnemyRole : Role {

        public MonsterData MyData { get; private set; }
        public override string Name { get { return MyData.Name; } }
        public override string Ref { get { return MyData.Ref; } }

        public List<RoleAction> Actions { get; private set; } = new List<RoleAction>();


        public void ScheduleActions() {
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
            EnemyUI.GetInstance<EnemyUI>()?.RefreshUI();
        }



        protected override void OnDeath() {
            base.OnDeath();
            Debug.Log("敵人已死亡");
        }
        public class Builder : Role.Builder<EnemyRole> {
            public Builder SetData(MonsterData _data) {
                instance.MyData = _data;
                instance.ScheduleActions();
                return this;
            }
        }
    }
}