using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class EnemyAction : RoleAction {
        public EnemyAction PreviousAction { get; private set; }

        /// <summary>
        /// 考慮行動順序後在時間軸上是第幾秒才會執行
        /// </summary>
        public override int NeedTimeBeforeAction {
            get {
                return GetTimeOnTimeline(0);
            }
        }

        /// <summary>
        /// 取得敵方此行動考慮行動順序後在時間軸上是第幾秒才會執行
        /// </summary>
        int GetTimeOnTimeline(int _previousActionTotalNeedTime) {
            if (PreviousAction == null) return NeedTime;
            else {
                _previousActionTotalNeedTime += NeedTime + PreviousAction.GetTimeOnTimeline(_previousActionTotalNeedTime);
                return _previousActionTotalNeedTime;
            }
        }

        public void AddNeedTime(int _value) {
            NeedTime += _value;
        }


        public EnemyAction(string _name, HashSet<string> _particles, EnemyRole _doer, Role _target, int _time, List<StatusEffect> effects, AttackPart _attackPart, EnemyAction _previousAction)
            : base(_name, _particles, _doer, _target, _time, effects, _attackPart) {
            PreviousAction = _previousAction;
        }

    }
}