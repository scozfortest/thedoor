using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class PlayerAction : RoleAction {
        public AttackPart MyAttackPart { get; private set; }
        public PlayerAction(string _name, PlayerRole _doer, int _time, List<StatusEffect> effects, AttackPart _attackPart)
                    : base(_name, _doer, _time, effects) {

            MyAttackPart = _attackPart;
        }
        protected override void DoDmg(StatusEffect _effect) {
            int dmg = _effect.Dmg();
            if (dmg != 0) {
                _effect.Doer.AddExtraDmg(ref dmg);
                if (_effect.MyTarget is EnemyRole) {
                    var eRole = ((EnemyRole)_effect.MyTarget);
                    var partTurple = eRole.MyData.GetAttackPartTuple(MyAttackPart);
                    WriteLog.LogColorFormat("攻擊{0} 傷害: {1} 命中: {2}", WriteLog.LogType.Battle, MyAttackPart.ToString(), partTurple.Item1, partTurple.Item2);
                    if (!Prob.GetResult(partTurple.Item2)) {//部位攻擊未命中
                        //如果沒成功會跳Miss
                        DNPManager.Instance.Spawn(DNPManager.DPNType.Miss, 0, BattleUI.GetTargetRectTrans(eRole), Vector2.zero);
                        WriteLog.LogColorFormat("攻擊{0}未命中", WriteLog.LogType.Battle, MyAttackPart.ToString());
                        return;
                    }
                    dmg = (int)(dmg * partTurple.Item1);
                }
                _effect.MyTarget.GetAttacked(dmg);
            }
        }
    }
}