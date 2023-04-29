using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class PlayerAction : RoleAction {
        public AttackPart MyAttackPart;
        public PlayerAction(PlayerRole _doer, int _time, List<StatusEffect> effects, AttackPart _attackPart)
                    : base(_doer, _time, effects) {

            MyAttackPart = _attackPart;
        }
    }
}