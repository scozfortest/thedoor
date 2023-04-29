using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public abstract class StatusEffect {
        public EffectType MyType { get; protected set; }
        public int Stack { get; protected set; }
        public bool IsExpired { get { return Stack <= 0; } }
        public Role Doer { get; private set; }//執行者
        public Role MyTarget { get; private set; }//目標
        public float Probability { get; private set; }//命中機率，失敗會跳Miss
        public bool IsBuff { get; protected set; }//是否為Buff


        public void AddStack(int _stack) {
            Stack += _stack;
        }
        public void SetStack(int _stack) {
            Stack = _stack;
        }

        #region 行動時觸發

        public virtual int Dmg() { return 0; }//造成傷害值
        public virtual int SanDmg() { return 0; }//造成神智傷害值
        public virtual int AttackExtraDamageDealt() { return 0; }//攻擊造成額外傷害
        public virtual int AttackExtraSanDamageDealt() { return 0; }//攻擊造成額外神智傷害
        public virtual int Restore() { return 0; }//恢復生命
        public virtual int SanRestore() { return 0; }//恢復神智
        public virtual int TimeModification() { return 0; }//時間調整

        #endregion

        #region 受到攻擊觸發

        public virtual int BeAtteckedExtraDmgTaken() { return 0; }//受到攻擊受到傷害
        public virtual int BeAtteckedExtraSanDmgTaken() { return 0; }//受到攻擊受到神智傷害
        public virtual int BeAttackDamageReduction(int _dmg) { return 0; }//受到攻擊減少傷害
        public virtual int BeAttackSanDamageReduction(int _dmg) { return 0; }//受到攻擊減少神智傷害

        #endregion

        #region 時間流逝時觸發

        public virtual int TimeDmgTaken(int _actionTime) { return 0; }//經過時間受到傷害
        public virtual int TimeSanDmgTaken(int _actionTime) { return 0; }//經過時間受到神智傷害

        #endregion

        protected StatusEffect() {
        }
        public abstract class Builder<T> where T : StatusEffect, new() {
            protected T instance;

            public Builder() {
                instance = new T();
            }

            public Builder<T> SetStack(int _stack) {
                instance.Stack = _stack;
                return this;
            }
            public Builder<T> SetDoer(Role _role) {
                instance.Doer = _role;
                return this;
            }
            public Builder<T> SetTarget(Role _role) {
                instance.MyTarget = _role;
                return this;
            }
            public Builder<T> SetProb(float _prob) {
                instance.Probability = _prob;
                return this;
            }

            public T Build() {
                return instance;
            }
        }

    }

    public class EffectFactory {
        private delegate StatusEffect EffectCreator(float _prob, int stack, Role doer, Role target);
        private static Dictionary<EffectType, EffectCreator> effectCreators = new Dictionary<EffectType, EffectCreator>{
        { EffectType.HP, (_prob,stack, doer, target) => new HPEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.SanP, (_prob,stack, doer, target) => new SanPEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Dizzy, (_prob,stack, doer, target) => new DizzyEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Poison, (_prob,stack, doer, target) => new PoisonEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Insanity, (_prob,stack, doer, target) => new InsanityEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Bleeding, (_prob,stack, doer, target) => new BleedingEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Fear, (_prob,stack, doer, target) => new FearEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Evade, (_prob,stack, doer, target) => new EvadeEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Calm, (_prob,stack, doer, target) => new CalmEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Focus, (_prob,stack, doer, target) => new FocusEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Horror, (_prob,stack, doer, target) => new HorrorEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        // 有新增狀態效果要往下加
    };

        public static StatusEffect Create(float _prob, EffectType effectType, int stack, Role doer, Role target) {
            if (effectCreators.TryGetValue(effectType, out EffectCreator creator)) {
                return creator(_prob, stack, doer, target);
            }

            WriteLog.LogErrorFormat("EffectFactory: 未定義的效果字典", effectType);
            return null;
        }
    }

    /// <summary>
    /// 可以是傷害或恢復生命
    /// </summary>
    public class HPEffect : StatusEffect {
        public class Builder : Builder<HPEffect> { }
        public HPEffect() {
            MyType = EffectType.HP;
        }

        public override int Dmg() {
            if (Stack >= 0) return 0;
            int value = Stack;
            SetStack(0);
            return value;
        }
        public override int Restore() {
            if (Stack <= 0) return 0;
            int value = Stack;
            SetStack(0);
            return value;
        }
    }

    /// <summary>
    /// 可以是傷害或恢復神智
    /// </summary>
    public class SanPEffect : StatusEffect {
        public class Builder : Builder<SanPEffect> { }
        public SanPEffect() {
            MyType = EffectType.SanP;
        }

        public override int SanDmg() {
            if (Stack >= 0) return 0;
            int value = Stack;
            SetStack(0);
            return value;
        }
        public override int SanRestore() {
            if (Stack <= 0) return 0;
            int value = Stack;
            SetStack(0);
            return value;
        }
    }


    /// <summary>
    /// 暈眩效果，使行動消耗增加
    /// 遊戲是時間軸制 假設怪物下次攻擊分別是 咬(3s) 重擊(5s) 在時間軸上就會顯示3s時候進行咬 在8s時進行重擊 也就是玩家在距離
    ///  下次怪物攻擊前有3s的時間格可以用 如果玩家本來發動砍消耗2s 那玩家會剩下1s可以行動 假設玩家重了狀態"暈眩"1層結果會是 怪物在2s後進行咬 在7s秒後進行重擊
    ///  所以這時玩家發動砍消耗2s就直接換怪物進行咬 也就是說 暈眩就是使行動消耗增加的狀態
    /// </summary>
    public class DizzyEffect : StatusEffect {

        public class Builder : Builder<DizzyEffect> { }

        public DizzyEffect() {
            MyType = EffectType.Dizzy;
        }

        public override int TimeModification() {
            if (Stack <= 0) return 0;
            int value = Stack;
            SetStack(0);
            return value;
        }
    }


    /// <summary>
    /// 中毒效果，在行動後受到行動消耗秒數的傷害並移除相應層數
    /// 每次腳色行動後受到行動消耗s的傷害並移除s層數 假設目前5層 此行動消耗3s 發動完此行動會受到3的傷害並移除3層剩下2層
    /// 如果目前2層 此行動消耗3s 發動完此行動只會受到2傷害 並剩下0層
    /// </summary>
    public class PoisonEffect : StatusEffect {
        public class Builder : Builder<PoisonEffect> { }
        public PoisonEffect() {
            IsBuff = false;
            MyType = EffectType.Poison;
        }

        public override int TimeDmgTaken(int _time) {
            if (Stack <= 0) return 0;
            int value = Mathf.Min(Stack, _time);
            AddStack(-_time);
            return value;
        }
    }
    /// <summary>
    /// 出血效果，在受到攻擊時受到額外的層數傷害
    /// 每次腳色行動後受到行動消耗s的神智傷害並移除s層數 假設目前5層 此行動消耗3s 發動完此行動會受到3的神智傷害並移除3層剩下2層
    /// 如果目前2層 此行動消耗3s 發動完此行動只會受到2傷害 並剩下0層
    /// </summary>
    public class InsanityEffect : StatusEffect {
        public class Builder : Builder<InsanityEffect> { }
        public InsanityEffect() {
            IsBuff = false;
            MyType = EffectType.Insanity;
        }

        public override int TimeSanDmgTaken(int _time) {
            if (Stack <= 0) return 0;
            int value = Mathf.Min(Stack, _time);
            AddStack(-value);
            return value;
        }
    }

    /// <summary>
    /// 恐懼效果，在受到攻擊時受到額外的層數神智傷害
    /// 每次受到攻擊都會受到 層數的傷害 例如目前5層 每次受到攻擊都會受到5點傷害
    /// </summary>
    public class BleedingEffect : StatusEffect {
        public class Builder : Builder<BleedingEffect> { }
        public BleedingEffect() {
            IsBuff = false;
            MyType = EffectType.Bleeding;
        }

        public override int BeAtteckedExtraDmgTaken() {
            if (Stack <= 0) return 0;
            return Stack;
        }
    }
    /// <summary>
    /// 專注效果，攻擊時增加層數的傷害
    /// 每次受到攻擊都會受到 層數的心智傷害 例如目前5層 每次受到攻擊都會受到5點神智傷害
    /// </summary>
    public class FearEffect : StatusEffect {
        public class Builder : Builder<FearEffect> { }
        public FearEffect() {
            IsBuff = false;
            MyType = EffectType.Fear;
        }

        public override int BeAtteckedExtraSanDmgTaken() {
            if (Stack <= 0) return 0;
            return Stack;
        }
    }
    /// <summary>
    /// 專注效果，攻擊時增加層數的傷害
    /// 攻擊時增加層數的傷害
    /// </summary>
    public class FocusEffect : StatusEffect {
        public class Builder : Builder<FocusEffect> { }
        public FocusEffect() {
            IsBuff = false;
            MyType = EffectType.Focus;
        }

        public override int AttackExtraDamageDealt() {
            if (Stack <= 0) return 0;
            return Stack;
        }
    }
    /// <summary>
    /// 恐怖效果，攻擊時增加層數的傷害
    /// 攻擊時增加層數的傷害
    /// </summary>
    public class HorrorEffect : StatusEffect {
        public class Builder : Builder<HorrorEffect> { }
        public HorrorEffect() {
            IsBuff = true;
            MyType = EffectType.Horror;
        }

        public override int AttackExtraSanDamageDealt() {
            if (Stack <= 0) return 0;
            return Stack;
        }
    }
    /// <summary>
    /// 迴避效果，減少下次受到的傷害並移除相應層數
    /// 減少下次受到的傷害 如果有2層 下次受到攻擊時傷害減少2 並移除2層
    /// </summary>
    public class EvadeEffect : StatusEffect {
        public class Builder : Builder<EvadeEffect> { }
        public EvadeEffect() {
            IsBuff = true;
            MyType = EffectType.Evade;
        }

        public override int BeAttackDamageReduction(int _dmg) {
            if (Stack <= 0) return 0;
            int value = Mathf.Min(Stack, _dmg);
            AddStack(-value);
            return value;
        }
    }
    /// <summary>
    /// 冷靜效果，減少下次受到的神智傷害並移除相應層數
    /// 減少下次受到的神智傷害 如果有2層 下次受到攻擊時神智傷害減少2 並移除2層
    /// </summary>
    public class CalmEffect : StatusEffect {
        public class Builder : Builder<CalmEffect> { }
        public CalmEffect() {
            IsBuff = true;
            MyType = EffectType.Calm;
        }

        public override int BeAttackDamageReduction(int _dmg) {
            if (Stack <= 0) return 0;
            int value = Mathf.Min(Stack, _dmg);
            AddStack(-value);
            return value;
        }
    }



}