using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class PlayerRole : Role {

        public RoleData MyData { get; private set; }
        public override string Name { get { return MyData.Name; } }
        public override string Ref { get { return MyData.Ref; } }


        protected override void OnDeath() {
            base.OnDeath();
            Debug.Log("玩家已死亡");
        }



        public class Builder : Builder<PlayerRole> {
            public Builder SetData(RoleData _data) {
                instance.MyData = _data;
                return this;
            }
        }

    }
}