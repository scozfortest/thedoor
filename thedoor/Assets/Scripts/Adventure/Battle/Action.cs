using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class RoleAction {
        public Role Doer;
        public int Time { get; private set; }//消耗時間
        public List<StatusEffect> Effects { get; private set; }

        public RoleAction(Role _doer, int _time, List<StatusEffect> _effects) {
            Doer = _doer;
            Time = _time;
            Effects = _effects;
        }


        /// <summary>
        /// 執行行動
        /// </summary>
        public void DoAction() {

            //執行效果
            foreach (var effect in Effects) {
                // 如果執行者或目標已經死亡就不執行效果
                if (effect.MyTarget.IsDead || effect.Doer.IsDead) continue;

                //對目標進行攻擊
                int dmg = effect.Dmg();
                dmg += effect.Doer.GetExtraAttackDmg();
                effect.MyTarget.TackenDmgAttacked(dmg);

                //對目標進行神智攻擊
                int sanDmg = effect.SanDmg();
                sanDmg += effect.Doer.GetExtraAttackSanDmg();
                effect.MyTarget.TackenSanDmgAttacked(sanDmg);

                //對目標恢復生命
                int restore = effect.Restore();
                effect.MyTarget.AddHP(restore);

                //對目標恢復神智
                int restoreSanP = effect.SanRestore();
                effect.Doer.AddSanP(restoreSanP);

                //承受時間流逝傷害
                int timeDmg = effect.Doer.GetTimeDmgTaken(Time);
                effect.Doer.AddHP(-timeDmg);

                //承受時間流逝神智傷害
                int timeSanDmg = effect.Doer.GetTimeSanDmgTaken(Time);
                effect.Doer.AddSanP(-timeSanDmg);
            }

        }
    }
}