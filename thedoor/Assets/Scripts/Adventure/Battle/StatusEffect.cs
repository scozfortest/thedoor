using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public enum EffectType {
        HP,//  生命
        SanP,//  理智
        Dizzy,//    暈眩
        Poison,//中毒
        Insanity,//神智崩潰
        Bleeding,//   流血
        Fear,//   恐懼
        Evade,//迴避
        Calm,//冷靜
        Focus,//專注
        Horror,//恐怖

        Protection,//防護
        Fortitude,//堅毅
        Antidote,//解毒
        Recovery,//恢復理智
        Strong,//強壯
        Faith,//信仰

        Flee,//    戰鬥
    }
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
        public virtual int TimeModification(int _time) { return _time; }//時間調整
        public virtual HashSet<EffectType> RemoveStatusEffect() { return null; }//移除狀態

        public virtual List<StatusEffect> ApplyStatusEffect() { return null; }//賦予效果


        #endregion

        #region 行動完觸發
        public virtual HashSet<EffectType> RemoveStatusEffectWhenActionDone() { return null; }//行動完移除狀態

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


        #region 免疫狀態

        public virtual bool ImmuneStatusEffect(EffectType _type) { return false; }//免疫狀態

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
        { EffectType.Protection, (_prob,stack, doer, target) => new ProtectionEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Fortitude, (_prob,stack, doer, target) => new FortitudeEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Antidote, (_prob,stack, doer, target) => new AntidoteEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Recovery, (_prob,stack, doer, target) => new RecoveryEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Strong, (_prob,stack, doer, target) => new StrongEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Faith, (_prob,stack, doer, target) => new FaithEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
        { EffectType.Flee, (_prob,stack, doer, target) => new FleeEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).Build() },
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
    /// 消耗行動耗時追加層數
    /// </summary>
    public class DizzyEffect : StatusEffect {

        public class Builder : Builder<DizzyEffect> { }

        public DizzyEffect() {
            MyType = EffectType.Dizzy;
        }

        public override int TimeModification(int _time) {
            _time += Stack;
            return _time;
        }
        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
        }
        public override HashSet<EffectType> RemoveStatusEffectWhenActionDone() {
            return new HashSet<EffectType> { this.MyType };
        }
    }


    /// <summary>
    /// 中毒效果，在行動後受到行動消耗秒數的傷害並移除相應層數
    /// 每次腳色行動後受到行動消耗s的傷害並移除s層數 假設目前5層 此行動消耗3s 發動完此行動會受到3的傷害並移除3層剩下2層
    /// 如果目前2層 此行動消耗3s 發動完此行動只會受到2傷害 並剩下0層
    /// 戰鬥後不會移除效果 要休息,完成冒險,使用道具來解除
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

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
        }
    }
    /// <summary>
    /// 精神崩潰效果，行動後受到行動消耗秒數的神智傷害並移除相應層數
    /// 每次腳色行動後受到行動消耗s的神智傷害並移除s層數 假設目前5層 此行動消耗3s 發動完此行動會受到3的神智傷害並移除3層剩下2層
    /// 如果目前2層 此行動消耗3s 發動完此行動只會受到2傷害 並剩下0層
    /// 戰鬥後不會移除效果 要休息,完成冒險,使用道具來解除
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

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
        }
    }

    /// <summary>
    /// 流血效果，在受到攻擊時受到額外的層數傷害
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

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
        }
    }
    /// <summary>
    /// 恐懼效果，攻擊時增加層數的傷害
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

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
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

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
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

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
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

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
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
        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
        }
    }

    /// <summary>
    /// 受到的傷害降低層數 
    /// </summary>
    public class ProtectionEffect : StatusEffect {
        public class Builder : Builder<ProtectionEffect> { }
        public ProtectionEffect() {
            IsBuff = true;
            MyType = EffectType.Protection;
        }

        public override int BeAttackDamageReduction(int _dmg) {
            if (Stack <= 0) return 0;
            int value = Mathf.Min(Stack, _dmg);
            return value;
        }

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
        }
    }

    /// <summary>
    /// 受到的神智傷害降低層數
    /// </summary>
    public class FortitudeEffect : StatusEffect {
        public class Builder : Builder<FortitudeEffect> { }
        public FortitudeEffect() {
            IsBuff = true;
            MyType = EffectType.Fortitude;
        }

        public override int BeAttackSanDamageReduction(int _dmg) {
            if (Stack <= 0) return 0;
            int value = Mathf.Min(Stack, _dmg);
            return value;
        }

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
        }
    }

    /// <summary>
    /// 解除中毒狀態
    /// </summary>
    public class AntidoteEffect : StatusEffect {
        public class Builder : Builder<AntidoteEffect> { }
        public AntidoteEffect() {
            IsBuff = true;
            MyType = EffectType.Antidote;
        }

        public override HashSet<EffectType> RemoveStatusEffect() {
            SetStack(0);
            return new HashSet<EffectType> { EffectType.Poison };
        }

    }

    /// <summary>
    /// 解除精神崩潰狀態
    /// </summary>
    public class RecoveryEffect : StatusEffect {
        public class Builder : Builder<RecoveryEffect> { }
        public RecoveryEffect() {
            IsBuff = true;
            MyType = EffectType.Recovery;
        }

        public override HashSet<EffectType> RemoveStatusEffect() {
            SetStack(0);
            return new HashSet<EffectType> { EffectType.Insanity };
        }

    }


    /// <summary>
    /// 免疫流血與中毒狀態
    /// </summary>
    public class StrongEffect : StatusEffect {
        public class Builder : Builder<StrongEffect> { }
        public StrongEffect() {
            IsBuff = true;
            MyType = EffectType.Strong;
        }

        public override HashSet<EffectType> RemoveStatusEffect() {
            return new HashSet<EffectType> { EffectType.Bleeding, EffectType.Poison };
        }

        public override bool ImmuneStatusEffect(EffectType _type) {
            if (_type == EffectType.Bleeding || _type == EffectType.Poison)
                return true;
            return false;
        }

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
        }
    }

    /// <summary>
    /// 免疫精神崩潰與恐懼狀態
    /// </summary>
    public class FaithEffect : StatusEffect {
        public class Builder : Builder<FaithEffect> { }
        public FaithEffect() {
            IsBuff = true;
            MyType = EffectType.Faith;
        }
        public override HashSet<EffectType> RemoveStatusEffect() {
            return new HashSet<EffectType> { EffectType.Insanity, EffectType.Fear };
        }
        public override bool ImmuneStatusEffect(EffectType _type) {
            if (_type == EffectType.Insanity || _type == EffectType.Fear)
                return true;
            return false;
        }

        public override List<StatusEffect> ApplyStatusEffect() {
            return new List<StatusEffect> { this };
        }
    }

    /// <summary>
    /// 逃跑
    /// </summary>
    public class FleeEffect : StatusEffect {
        public class Builder : Builder<FleeEffect> { }
        public FleeEffect() {
            IsBuff = true;
            MyType = EffectType.Flee;
        }
    }



}