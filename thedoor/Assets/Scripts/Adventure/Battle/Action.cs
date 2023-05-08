using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TheDoor.Main {
    public class RoleAction {
        public string Name { get; private set; }
        public Role Doer;
        public int Time { get; protected set; }//該行動原本需消耗時間
        public int NeedTime { get; protected set; }

        /// <summary>
        /// 考慮行動順序後在時間軸上是第幾秒才會執行
        /// </summary>
        public virtual int NeedTimeBeforeAction { get { return NeedTime; } }

        public bool Done { get; protected set; }

        public List<StatusEffect> Effects { get; private set; }

        public RoleAction(string _name, Role _doer, int _time, List<StatusEffect> _effects) {
            Name = _name;
            Doer = _doer;
            Time = _time;
            NeedTime = Time;
            Effects = _effects;
            Done = false;
        }



        /// <summary>
        /// 傷害目標
        /// </summary>
        protected virtual void DoDmg(StatusEffect _effect) {
            int dmg = _effect.Dmg();
            if (dmg != 0) {
                _effect.Doer.AddExtraDmg(ref dmg);
                _effect.MyTarget.GetAttacked(dmg);
            }
        }
        /// <summary>
        /// 精神傷害目標
        /// </summary>
        protected virtual void DoSanDmg(StatusEffect _effect) {
            int sanDmg = _effect.SanDmg();
            if (sanDmg != 0) {
                _effect.Doer.AddExtraSanDmg(ref sanDmg);
                ((PlayerRole)_effect.MyTarget).GetSanAttacked(sanDmg);
            }
        }

        /// <summary>
        /// 生命影響
        /// </summary>
        protected virtual void RestoreHP(StatusEffect _effect) {
            int restore = _effect.Restore();
            if (restore != 0) {
                _effect.MyTarget.AddHP(restore);
            }
        }
        /// <summary>
        /// 神智影響
        /// </summary>
        protected virtual void RestoreSanP(StatusEffect _effect) {
            int restoreSanP = _effect.SanRestore();
            if (restoreSanP != 0) {
                ((PlayerRole)_effect.Doer).AddSanP(restoreSanP);
            }
        }

        /// <summary>
        /// 執行行動
        /// </summary>
        public virtual void DoAction() {

            WriteLog.LogColor(Doer.Name + "執行行動:" + Name, WriteLog.LogType.Battle);
            //執行效果
            foreach (var effect in Effects) {
                WriteLog.LogColor("對 " + effect.MyTarget.Name + " 賦予效果:" + effect.MyType, WriteLog.LogType.Battle);
                // 如果執行者或目標已經死亡就不執行效果
                if (effect.MyTarget.IsDead || effect.Doer.IsDead) continue;
                //對目標進行攻擊
                if (!Prob.GetResult(effect.Probability)) {
                    WriteLog.LogColor(Name + " 賦予效果:" + effect.MyType + "失敗", WriteLog.LogType.Battle);
                    //如果沒成功會跳Miss或Fail
                    if (effect.IsBuff)
                        DNPManager.Instance.Spawn(DNPManager.DPNType.Fail, 0, BattleUI.GetTargetRectTrans(effect.MyTarget), Vector2.zero);
                    else
                        DNPManager.Instance.Spawn(DNPManager.DPNType.Miss, 0, BattleUI.GetTargetRectTrans(effect.MyTarget), Vector2.zero);
                    continue;
                }


                //傷害目標
                DoDmg(effect);
                //精神傷害目標
                if (effect.MyTarget is PlayerRole)
                    DoSanDmg(effect);
                //對目標恢復生命
                RestoreHP(effect);
                //對目標恢復神智
                if (effect.MyTarget is PlayerRole)
                    RestoreSanP(effect);

                ////承受時間流逝傷害
                //int timeDmg = 0;
                //effect.Doer.AddTimePassDmg(ref timeDmg, Time);
                //if (timeDmg != 0) {
                //    effect.Doer.AddHP(-timeDmg);
                //}

                ////承受時間流逝神智傷害
                //int timeSanDmg = 0;
                //effect.Doer.AddTimePassSanDmg(ref timeSanDmg, Time);
                //if (timeSanDmg != 0) {
                //    if (effect.MyTarget is PlayerRole) {
                //        ((PlayerRole)effect.Doer).AddSanP(-timeSanDmg);
                //    }
                //}

                //移除狀態效果
                if (effect.RemoveStatusEffect() != null)
                    effect.MyTarget.RemoveEffects(effect.RemoveStatusEffect().ToArray());




                //賦予效果
                var applyEffects = effect.ApplyStatusEffect();
                if (applyEffects != null && applyEffects.Count > 0) {
                    foreach (var applyEffect in applyEffects) {
                        bool emmune = false;
                        foreach (var tEffect in effect.MyTarget.Effects.Values) {
                            if (tEffect.ImmuneStatusEffect(effect.MyType)) {
                                emmune = true;
                                break;
                            }
                        }
                        if (!emmune)
                            effect.MyTarget.ApplyEffect(applyEffect);
                    }
                }



                //逃跑效果 觸發後就不會觸發之後的效果
                if (effect.MyType == EffectType.Flee) {
                    if (effect.MyTarget.IsDead) {
                        BattleUI.Instance.Lose();
                        break;
                    }
                }
            }
            OnDoActionDone();

        }

        protected virtual void OnDoActionDone() {
            Done = true;
            List<EffectType> removeTypes = new List<EffectType>();
            foreach (var effect in Doer.Effects.Values) {
                if (effect.RemoveStatusEffectWhenActionDone() != null)
                    removeTypes.AddRange(effect.RemoveStatusEffectWhenActionDone());
            }
            Doer.RemoveEffects(removeTypes.ToArray());
        }





    }
}