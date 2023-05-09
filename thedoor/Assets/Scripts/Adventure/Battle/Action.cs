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
        public AttackPart MyAttackPart { get; private set; }

        /// <summary>
        /// 考慮行動順序後在時間軸上是第幾秒才會執行
        /// </summary>
        public virtual int NeedTimeBeforeAction { get { return NeedTime; } }

        public bool Done { get; protected set; }

        public List<StatusEffect> Effects { get; private set; }

        public RoleAction(string _name, Role _doer, int _time, List<StatusEffect> _effects, AttackPart _attackPart) {
            Name = _name;
            Doer = _doer;
            Time = _time;
            NeedTime = Time;
            Effects = _effects;
            MyAttackPart = _attackPart;
            Done = false;
        }


        /// <summary>
        /// 執行行動
        /// </summary>
        public virtual void DoAction() {

            WriteLog.LogColor(Doer.Name + "行動:" + Name, WriteLog.LogType.Battle);

            //執行效果
            foreach (var effect in Effects) {
                // 如果執行者或目標已經死亡就不執行效果
                if (effect.MyTarget.IsDead || effect.Doer.IsDead) continue;

                string targetEffectStr = "對 " + effect.MyTarget.Name + " 賦予效果:" + effect.MyType;

                float hitProb = 1;
                float addHitProb = 0;

                if (effect.MyTarget is EnemyRole) {
                    var eRole = ((EnemyRole)effect.MyTarget);
                    var partTurple = eRole.MyData.GetAttackPartTuple(MyAttackPart);
                    WriteLog.LogColorFormat(targetEffectStr + " 部位{0} 傷害率: {1} 命中率: {2}", WriteLog.LogType.Battle, MyAttackPart.ToString(), partTurple.Item1, partTurple.Item2);
                    hitProb = partTurple.Item2;
                } else {
                    WriteLog.LogColorFormat(targetEffectStr + " 部位{0} 傷害率: {1} 命中率: {2}", WriteLog.LogType.Battle, AttackPart.Body, 1, 1);
                }

                foreach (var doerEffect in effect.Doer.Effects.Values) {
                    addHitProb += doerEffect.GetAttackHitProbExtraValue();
                }
                foreach (var targetEffect in effect.MyTarget.Effects.Values) {
                    addHitProb += targetEffect.GetBeAttackedHitProbExtraValue();
                }
                hitProb += addHitProb;
                hitProb *= effect.Probability;//最後才乘上各效果的機率 也就是說即使因狀態讓某個部位的命中率達到100%還是會因為各效果的命中不一樣而導致命中率不是100%
                //Debug.LogError("最終命中率=" + hitProb);
                if (!Prob.GetResult(hitProb)) {//未命中
                    WriteLog.LogColor(Name + " 賦予效果:" + effect.MyType + "失敗", WriteLog.LogType.Battle);
                    //如果沒成功會跳Miss或Fail
                    if (effect.IsBuff)
                        DNPManager.Instance.Spawn(DNPManager.DPNType.Fail, 0, BattleUI.GetTargetRectTrans(effect.MyTarget), Vector2.zero);
                    else
                        DNPManager.Instance.Spawn(DNPManager.DPNType.Miss, 0, BattleUI.GetTargetRectTrans(effect.MyTarget), Vector2.zero);
                    continue;
                }


                //攻擊目標
                effect.AttackDealDmg();
                //精神傷害目標
                effect.AttackDealSanDmg();
                //恢復目標生命
                effect.RestoreHP();
                //恢復目標神智
                effect.RestoreSanP();
                //移除目標狀態效果
                effect.RemoveEffect();
                //賦予目標狀態效果
                effect.ApplyEffect();

            }
            OnDoActionDone();

        }

        protected virtual void OnDoActionDone() {
            Done = true;
            List<StatusEffect> effects = new List<StatusEffect>(Doer.Effects.Values);
            foreach (var effect in effects) {
                if (effect == null) continue;
                effect.ActionDoneRemoveEffect();
            }
        }





    }
}