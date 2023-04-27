using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class EnemyRole : Role {

        public MonsterData MyData { get; private set; }
        public override string Ref { get { return MyData.Ref; } }

        public List<RoleAction> Actions { get; private set; } = new List<RoleAction>();

        public void ScheduleActions() {
            int needCount = GameSettingData.GetInt(GameSetting.Monster_ScheduleActionCount);
            int addCount = needCount - Actions.Count;
            for (int i = 0; i < addCount; i++) {
                RoleAction ac = GetAction(BattleManager.PRole);
                if (ac == null) continue;
                Actions.Add(ac);
            }
        }

        RoleAction GetAction(Role _target) {
            var action = MyData.GetAction(this, _target);
            return action;
        }


        protected override void OnDeath() {
            base.OnDeath();
            Debug.Log("敵人已死亡");
        }
        public class Builder : Role.Builder<EnemyRole> {
            public Builder SetData(MonsterData _data) {
                instance.MyData = _data;
                return this;
            }
        }
    }
}