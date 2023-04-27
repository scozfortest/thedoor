using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class EnemyAction : RoleAction {
        public EnemyAction(EnemyRole _doer, int _time, List<StatusEffect> effects)
            : base(_doer, _time, effects) { }
    }
}