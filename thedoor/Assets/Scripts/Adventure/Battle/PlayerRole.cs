using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class PlayerRole : Role {

        public RoleData MyData { get; private set; }
        public override string Ref { get { return MyData.Ref; } }

        public void DoSupplyAction(SupplyData _supplyData, Role _target) {
            var action = _supplyData.GetAction(this, _target);
            if (action == null) return;
            action.DoAction();
        }

        protected override void OnDeath() {
            base.OnDeath();
            Debug.Log("玩家已死亡");
        }



        public class Builder : Role.Builder<PlayerRole> {
            public Builder SetData(RoleData _data) {
                instance.MyData = _data;
                return this;
            }
        }

    }
}