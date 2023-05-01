using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class RoleAction {
        public string Name { get; private set; }
        public Role Doer;
        public int Time { get; private set; }//消耗時間
        public List<StatusEffect> Effects { get; private set; }

        public RoleAction(string _name, Role _doer, int _time, List<StatusEffect> _effects) {
            Name = _name;
            Doer = _doer;
            Time = _time;
            Effects = _effects;
        }

        public void ModifyTime(int _value) {
            Time = _value;
        }

        /// <summary>
        /// 執行行動
        /// </summary>
        public virtual void DoAction() {
            WriteLog.Log(Doer.Name + "執行行動:" + Name);
            //執行效果
            foreach (var effect in Effects) {
                WriteLog.Log("對 " + effect.MyTarget.Name + " 賦予效果:" + effect.MyType);
                // 如果執行者或目標已經死亡就不執行效果
                if (effect.MyTarget.IsDead || effect.Doer.IsDead) continue;
                //對目標進行攻擊
                if (!Prob.GetResult(effect.Probability)) {
                    Debug.LogError("Miss");
                    continue;
                }
                int dmg = effect.Dmg();
                if (dmg != 0) {
                    effect.Doer.AddExtraDmg(ref dmg);
                    effect.MyTarget.GetAttacked(dmg);
                }

                //對目標進行神智攻擊
                if (effect.MyTarget is PlayerRole) {
                    int sanDmg = effect.SanDmg();
                    if (sanDmg != 0) {
                        effect.Doer.AddExtraSanDmg(ref sanDmg);
                        ((PlayerRole)effect.MyTarget).GetSanAttacked(sanDmg);
                    }
                }

                //對目標恢復生命
                int restore = effect.Restore();
                if (restore != 0) {
                    effect.MyTarget.AddHP(restore);
                }


                //對目標恢復神智
                if (effect.MyTarget is PlayerRole) {
                    int restoreSanP = effect.SanRestore();
                    if (restoreSanP != 0) {
                        ((PlayerRole)effect.Doer).AddSanP(restoreSanP);
                    }
                }


                //承受時間流逝傷害
                int timeDmg = 0;
                effect.Doer.AddTimePassDmg(ref timeDmg, Time);
                if (timeDmg != 0) {
                    effect.Doer.AddHP(-timeDmg);
                }


                //承受時間流逝神智傷害
                int timeSanDmg = 0;
                effect.Doer.AddTimePassSanDmg(ref timeSanDmg, Time);
                if (timeSanDmg != 0) {
                    if (effect.MyTarget is PlayerRole) {
                        ((PlayerRole)effect.Doer).AddSanP(-timeSanDmg);
                    }
                }

            }

        }
    }
}