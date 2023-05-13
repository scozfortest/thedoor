using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class PlayerAction : RoleAction {

        /// <summary>
        /// 考慮狀態效果後 需要幾秒執行此行動
        /// </summary>
        public override int NeedTimeBeforeAction {
            get {
                int value = NeedTime;
                foreach (var effect in Doer.Effects.Values) {
                    value += effect.NeedTimeModification();
                }
                return value;
            }
        }

        public PlayerAction(string _name, PlayerRole _doer, Role _target, int _time, List<StatusEffect> effects, AttackPart _attackPart)
            : base(_name, _doer, _target, _time, effects, _attackPart) {
        }

    }
}