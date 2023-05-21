using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public enum EffectType {
        Attack,//  攻擊
        SanAttack,//  神智攻擊
        RestoreHP,//恢復生命
        RestoreSanP,//恢復神智
        Dizzy,//    暈眩
        Poison,//中毒
        Fear,//   恐懼
        Bleeding,//   流血
        Insanity,//崩潰
        Evade,//迴避
        Burst,//爆發
        Weak,//衰弱
        Horror,//恐怖
        Protection,//防護
        Fortitude,//堅毅
        Antidote,//解毒
        Recovery,//恢復理智
        Strong,//強壯
        Faith,//信仰
        Flee,// 逃離戰鬥
        Thorns,//尖刺
        Ethereal,//靈體化
        Blindness,//失明
        Focus,//專注

        BeastSeveredToe,//野獸的斷趾 每經過兩個房間回復x點生命
    }
    public abstract class StatusEffect {
        public EffectType MyType { get; protected set; }
        int stack;
        public int Stack {
            get { return stack; }
            set {
                if (value <= 0)
                    value = 0;
                stack = value;
            }
        }
        public int Value { get; protected set; }//特殊用參數
        public bool IsExpired { get { return Stack <= 0; } }
        public Role Doer { get; private set; }//執行者
        public Role MyTarget { get; private set; }//目標
        public float Probability { get; private set; }//成功機率，失敗會跳Miss
        public string Name {
            get {
                return StringData.GetString_static("EffectType" + "_" + MyType.ToString(), "Name");
            }
        }
        public string Description {
            get {
                return StringData.GetString_static("EffectType" + "_" + MyType.ToString(), "Description");
            }
        }

        /// <summary>
        /// 是否為Buff
        /// </summary>
        public bool IsBuff {
            get {
                switch (MyType) {
                    case EffectType.RestoreHP:
                    case EffectType.RestoreSanP:
                    case EffectType.Evade:
                    case EffectType.Burst:
                    case EffectType.Horror:
                    case EffectType.Protection:
                    case EffectType.Fortitude:
                    case EffectType.Antidote:
                    case EffectType.Recovery:
                    case EffectType.Strong:
                    case EffectType.Faith:
                    case EffectType.Thorns:
                    case EffectType.Ethereal:
                    case EffectType.Focus:
                    case EffectType.Flee:
                    case EffectType.BeastSeveredToe:
                        return true;
                    case EffectType.Attack:
                    case EffectType.SanAttack:
                    case EffectType.Dizzy:
                    case EffectType.Poison:
                    case EffectType.Fear:
                    case EffectType.Bleeding:
                    case EffectType.Insanity:
                    case EffectType.Weak:
                    case EffectType.Blindness:
                        return false;
                    default:
                        WriteLog.LogError("尚未定義IsBuff的狀態類型");
                        return true;
                }
            }
        }
        /// <summary>
        /// 是否會在戰鬥後移除
        /// </summary>
        public bool RemoveAffertBattle {
            get {
                switch (MyType) {
                    case EffectType.RestoreHP:
                    case EffectType.RestoreSanP:
                    case EffectType.Evade:
                    case EffectType.Burst:
                    case EffectType.Horror:
                    case EffectType.Protection:
                    case EffectType.Fortitude:
                    case EffectType.Thorns:
                    case EffectType.Ethereal:
                    case EffectType.Focus:
                    case EffectType.Flee:
                    case EffectType.Attack:
                    case EffectType.SanAttack:
                    case EffectType.Dizzy:
                    case EffectType.Bleeding:
                    case EffectType.Insanity:
                    case EffectType.Weak:
                    case EffectType.Blindness:
                    case EffectType.BeastSeveredToe:
                        return true;
                    case EffectType.Poison:
                    case EffectType.Fear:
                    case EffectType.Antidote:
                    case EffectType.Recovery:
                    case EffectType.Strong:
                    case EffectType.Faith:
                        return false;
                    default:
                        WriteLog.LogError("尚未定義RemoveAffertBattle的狀態類型");
                        return true;
                }
            }
        }
        public AttackPart MyAttackPart { get; protected set; }//攻擊部位


        public void AddStack(int _stack) {
            Stack += _stack;
            if (IsExpired)
                MyTarget.RemoveEffects(MyType);
        }

        #region 行動時觸發

        public virtual int AttackDealDmg() { return 0; }//攻擊造成傷害
        public virtual int AttackDealSanDmg() { return 0; }//攻擊造成神智傷害
        public virtual int GetAttackExtraValue(int _dmg) { return 0; }//攻擊造成傷害獲得額外數值
        public virtual int GetSanAttackExtraValue(int _dmg) { return 0; }//攻擊造成神智傷害獲得額外數值
        public virtual float GetAttackHitProbExtraValue() { return 0; }//任何效果(包括攻擊)的命中率獲得額外數值
        public virtual int RestoreHP() { return 0; }//恢復生命
        public virtual int RestoreSanP() { return 0; }//恢復神智
        public virtual int NeedTimeModification() { return 0; }//行動消耗時間調整
        public virtual HashSet<EffectType> RemoveEffect() { return null; }//移除狀態
        //賦予狀態
        public virtual bool ApplyEffect() {
            foreach (var targetOwnEffect in MyTarget.Effects.Values) {
                if (targetOwnEffect.ImmuneStatusEffect(MyType)) return false;
            }
            return true;
        }


        #endregion

        #region 行動完觸發
        public virtual void ActionDoneRemoveEffect() { }//行動完移除狀態

        #endregion

        #region 被賦予狀態時觸發
        public virtual void BeAppliedEffectTrigger() { }//被賦予狀態時觸發
        public virtual bool ImmuneStatusEffect(EffectType _type) { return false; }//免疫狀態
        #endregion


        #region 受到攻擊觸發

        public virtual int GetBeAttackedExtraValue(int _dmg) { return 0; }//受到攻擊獲得額外數值
        public virtual int GetBeSanAttackedExtraValue(int _dmg) { return 0; }//受到神智攻擊獲得額外數值
        public virtual float GetBeAttackedHitProbExtraValue() { return 0; }//受到任何效果(包括攻擊)的命中率獲得額外數值
        public virtual void BeAttackedTrigger(Role _attacker) { }//受到攻擊後 觸發效果
        public virtual void BeSanAttackedTrigger(Role _attacker) { }//受到神智攻擊後 觸發效果



        #endregion


        #region 時間流逝時觸發
        public virtual void TimePassStackChange(int _time) { }//經過時間層數變化
        public virtual int TimePassDmgTaken(int _time) { return 0; }//經過時間受到傷害
        public virtual int TimePassSanDmgTaken(int _time) { return 0; }//經過時間受到神智傷害

        #endregion

        #region 開門時觸發
        public virtual void OpenTheDoorTrigger() { }//打開門觸發

        #endregion


        /// <summary>
        /// 對玩家腳色賦予劇本效果
        /// </summary>
        public static void DoScriptEffectsToPlayerRole(PlayerRole _role, List<StatusEffect> _statusEffect) {
            foreach (var effect in _statusEffect) {
                switch (effect.MyType) {
                    case EffectType.Attack:
                        _role.AddHP(-effect.Stack);
                        break;
                    case EffectType.SanAttack:
                        _role.AddSanP(-effect.Stack);
                        break;
                    case EffectType.RestoreHP:
                        _role.AddHP(effect.Stack);
                        break;
                    case EffectType.RestoreSanP:
                        _role.AddSanP(effect.Stack);
                        break;
                    default:
                        WriteLog.LogError("DoScriptEffectsToPlayerRole賦予錯誤類型的效果給玩家腳色: " + effect.MyType);
                        break;
                }
            }
        }


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
            public Builder<T> SetAttackPart(AttackPart _attackPart) {
                instance.MyAttackPart = _attackPart;
                return this;
            }

            public T Build() {
                return instance;
            }
        }

    }

    public class EffectFactory {
        private delegate StatusEffect EffectCreator(float _prob, int stack, Role doer, Role target, AttackPart _attackPart);
        private static Dictionary<EffectType, EffectCreator> effectCreators = new Dictionary<EffectType, EffectCreator>{
        { EffectType.Attack, (_prob,stack, doer, target,_attackPart) => new AttackEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.SanAttack, (_prob,stack, doer, target,_attackPart) => new SanAttackEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.RestoreHP, (_prob,stack, doer, target,_attackPart) => new RestoreHPEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.RestoreSanP, (_prob,stack, doer, target,_attackPart) => new RestoreSanPEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Dizzy, (_prob,stack, doer, target,_attackPart) => new DizzyEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Poison, (_prob,stack, doer, target,_attackPart) => new PoisonEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Insanity, (_prob,stack, doer, target,_attackPart) => new InsanityEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Bleeding, (_prob,stack, doer, target,_attackPart) => new BleedingEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Fear, (_prob,stack, doer, target,_attackPart) => new FearEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Evade, (_prob,stack, doer, target,_attackPart) => new EvadeEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Burst, (_prob,stack, doer, target,_attackPart) => new BurstEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Weak, (_prob,stack, doer, target,_attackPart) => new WeakEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Horror, (_prob,stack, doer, target,_attackPart) => new HorrorEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Protection, (_prob,stack, doer, target,_attackPart) => new ProtectionEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Fortitude, (_prob,stack, doer, target,_attackPart) => new FortitudeEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Antidote, (_prob,stack, doer, target,_attackPart) => new AntidoteEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Recovery, (_prob,stack, doer, target,_attackPart) => new RecoveryEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Strong, (_prob,stack, doer, target,_attackPart) => new StrongEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Faith, (_prob,stack, doer, target,_attackPart) => new FaithEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Flee, (_prob,stack, doer, target,_attackPart) => new FleeEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Thorns, (_prob,stack, doer, target,_attackPart) => new ThornsEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Ethereal, (_prob,stack, doer, target,_attackPart) => new EtherealEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Blindness, (_prob,stack, doer, target,_attackPart) => new BlindnessEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.Focus, (_prob,stack, doer, target,_attackPart) => new FocusEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        { EffectType.BeastSeveredToe, (_prob,stack, doer, target,_attackPart) => new BeastSeveredToeEffect.Builder().SetProb(_prob).SetStack(stack).SetDoer(doer).SetTarget(target).SetAttackPart(_attackPart).Build() },
        // 有新增狀態效果要往下加
    };

        public static StatusEffect Create(float _prob, EffectType effectType, int stack, Role doer, Role target, AttackPart _attackPart) {
            if (effectCreators.TryGetValue(effectType, out EffectCreator creator)) {
                return creator(_prob, stack, doer, target, _attackPart);
            }

            WriteLog.LogErrorFormat("EffectFactory: 未定義的效果字典", effectType);
            return null;
        }
    }

    /// <summary>
    /// 攻擊造成傷害
    /// </summary>
    public class AttackEffect : StatusEffect {
        public class Builder : Builder<AttackEffect> { }
        public AttackEffect() {
            MyType = EffectType.Attack;
        }

        public override int AttackDealDmg() {
            if (IsExpired) return 0;
            int value = Stack;
            int attackExtraValue = 0;

            foreach (var effect in Doer.Effects.Values) {
                attackExtraValue += effect.GetAttackExtraValue(Stack);
            }
            foreach (var effect in MyTarget.Effects.Values) {
                attackExtraValue += effect.GetBeAttackedExtraValue(Stack);
                effect.BeAttackedTrigger(Doer);
            }
            if (Doer is PlayerRole)//玩家腳色天賦加成
                attackExtraValue += ((PlayerRole)Doer).GetAttackExtraValueByTalent(Stack);
            //目標是敵方才考慮攻擊部位影響
            if (MyTarget is EnemyRole) {
                var eRole = ((EnemyRole)MyTarget);
                var partTurple = eRole.MyData.GetAttackPartTuple(MyAttackPart);
                value = Mathf.RoundToInt((float)value * partTurple.Item1);
            }
            //Debug.LogError("value=" + value);
            //Debug.LogError("attackExtraValue=" + attackExtraValue);
            value += attackExtraValue;
            MyTarget.GetAttacked(value);

            return value;
        }
    }

    /// <summary>
    /// 攻擊造成神智傷害
    /// </summary>
    public class SanAttackEffect : StatusEffect {
        public class Builder : Builder<SanAttackEffect> { }
        public SanAttackEffect() {
            MyType = EffectType.SanAttack;
        }

        public override int AttackDealSanDmg() {
            if (IsExpired) return 0;
            int value = Stack;
            int attackExtraValue = 0;

            foreach (var effect in Doer.Effects.Values) {
                attackExtraValue += effect.GetSanAttackExtraValue(Stack);
            }
            foreach (var effect in MyTarget.Effects.Values) {
                attackExtraValue += effect.GetBeSanAttackedExtraValue(Stack);
                effect.BeSanAttackedTrigger(Doer);
            }
            //目標是敵方才考慮攻擊部位影響
            if (MyTarget is EnemyRole) {
                var eRole = ((EnemyRole)MyTarget);
                var partTurple = eRole.MyData.GetAttackPartTuple(MyAttackPart);
                value = Mathf.RoundToInt((float)value * partTurple.Item1);
            }
            value += attackExtraValue;
            MyTarget.GetSanAttacked(value);

            return value;
        }
    }

    /// <summary>
    /// 回復生命
    /// </summary>
    public class RestoreHPEffect : StatusEffect {
        public class Builder : Builder<RestoreHPEffect> { }
        public RestoreHPEffect() {
            MyType = EffectType.RestoreHP;
        }

        public override int RestoreHP() {
            if (IsExpired) return 0;
            int value = Stack;
            MyTarget.AddHP(value);
            return value;
        }
    }
    /// <summary>
    /// 回復神智
    /// </summary>
    public class RestoreSanPEffect : StatusEffect {
        public class Builder : Builder<RestoreSanPEffect> { }
        public RestoreSanPEffect() {
            MyType = EffectType.RestoreSanP;
        }

        public override int RestoreSanP() {
            if (IsExpired) return 0;
            int value = Stack;
            MyTarget.AddSanP(value);
            return value;
        }
    }


    /// <summary>
    /// 使行動消耗增加層數時間 行動後移除暈眩效果
    /// </summary>
    public class DizzyEffect : StatusEffect {

        public class Builder : Builder<DizzyEffect> { }

        public DizzyEffect() {
            MyType = EffectType.Dizzy;
        }

        public override int NeedTimeModification() {
            return Stack;
        }
        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
        public override void BeAppliedEffectTrigger() {

            //如果目標是敵方腳色 要改變第一個行動需求的時間 並 將更新時間軸UI
            if (MyTarget is EnemyRole) {
                var myTarget = ((EnemyRole)MyTarget);
                myTarget.GetCurAction().AddNeedTime(Stack);
                TimelineBattleUI.Instance.PassTime(-Stack, () => {
                });
            }
            //跳狀態文字
            if (MyEnum.TryParseEnum(MyType.ToString(), out DNPManager.DPNType _type)) {
                DNPManager.Instance.Spawn(_type, Stack, BattleUI.GetTargetRectTrans(MyTarget), Vector2.zero);
            }

        }
        public override void ActionDoneRemoveEffect() {
            MyTarget.RemoveEffects(MyType);
        }
    }


    /// <summary>
    /// 每次時間流逝都會受到層數傷害並減少層數 戰鬥後保留狀態
    /// </summary>
    public class PoisonEffect : StatusEffect {
        public class Builder : Builder<PoisonEffect> { }
        public PoisonEffect() {
            MyType = EffectType.Poison;
        }

        public override int TimePassDmgTaken(int _time) {
            if (IsExpired) return 0;
            int effectTime = Mathf.Min(Stack, _time);
            int value = MyMath.SumOfArithmeticSeries(Stack, effectTime);
            MyTarget.AddHP(-value);
            return value;
        }
        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }
    /// <summary>
    /// 每次時間流逝都會受到層數神智傷害並減少層數 戰鬥後保留狀態
    /// </summary>
    public class FearEffect : StatusEffect {
        public class Builder : Builder<FearEffect> { }
        public FearEffect() {
            MyType = EffectType.Fear;
        }

        public override int TimePassSanDmgTaken(int _time) {
            if (IsExpired) return 0;
            int effectTime = Mathf.Min(Stack, _time);
            int value = MyMath.SumOfArithmeticSeries(Stack, effectTime);
            MyTarget.AddSanP(-value);
            return value;
        }

        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }

    /// <summary>
    /// 每次時間流逝都會受到層數傷害
    /// </summary>
    public class BleedingEffect : StatusEffect {
        public class Builder : Builder<BleedingEffect> { }
        public BleedingEffect() {
            MyType = EffectType.Bleeding;
        }

        public override int TimePassDmgTaken(int _time) {
            int effectTime = Mathf.Min(_time, Stack);
            int value = effectTime * Stack;
            MyTarget.AddHP(-value);
            return value;
        }


        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }

    /// <summary>
    ///每次時間流逝都會受到層數的神智傷害
    /// </summary>
    public class InsanityEffect : StatusEffect {
        public class Builder : Builder<InsanityEffect> { }
        public InsanityEffect() {
            MyType = EffectType.Insanity;
        }

        public override int TimePassSanDmgTaken(int _time) {
            int effectTime = Mathf.Min(_time, Stack);
            int value = effectTime * Stack;
            MyTarget.AddSanP(-value);
            return value;
        }


        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }

    /// <summary>
    /// 增加50%攻擊迴避率 隨時間減少層數
    /// </summary>
    public class EvadeEffect : StatusEffect {
        public class Builder : Builder<EvadeEffect> { }
        public EvadeEffect() {
            MyType = EffectType.Evade;
        }

        public override float GetBeAttackedHitProbExtraValue() {
            if (IsExpired) return 0;
            return -0.5f;
        }

        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }

    /// <summary>
    /// 攻擊時增加一半傷害 隨時間減少層數
    /// </summary>
    public class BurstEffect : StatusEffect {
        public class Builder : Builder<BurstEffect> { }
        public BurstEffect() {
            MyType = EffectType.Burst;
        }

        public override int GetAttackExtraValue(int _dmg) {
            if (IsExpired) return 0;
            _dmg = Mathf.RoundToInt((float)_dmg * 0.5f);
            return _dmg;
        }

        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }
    /// <summary>
    /// 攻擊時減少50%傷害 隨時間減少層數
    /// </summary>
    public class WeakEffect : StatusEffect {
        public class Builder : Builder<WeakEffect> { }
        public WeakEffect() {
            MyType = EffectType.Weak;
        }

        public override int GetAttackExtraValue(int _dmg) {
            if (IsExpired) return 0;
            _dmg = -Mathf.RoundToInt((float)_dmg * 0.5f);
            return _dmg;
        }

        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }
    /// <summary>
    /// 神智攻擊時增加50%傷害 隨時間減少層數
    /// </summary>
    public class HorrorEffect : StatusEffect {
        public class Builder : Builder<HorrorEffect> { }
        public HorrorEffect() {
            MyType = EffectType.Horror;
        }

        public override int GetSanAttackExtraValue(int _dmg) {
            if (IsExpired) return 0;
            _dmg = Mathf.RoundToInt((float)_dmg * 0.5f);
            return _dmg;
        }

        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }
    /// <summary>
    /// 受到的傷害減少50% 隨時間減少層數
    /// </summary>
    public class ProtectionEffect : StatusEffect {
        public class Builder : Builder<ProtectionEffect> { }
        public ProtectionEffect() {
            MyType = EffectType.Protection;
        }

        public override int GetBeAttackedExtraValue(int _dmg) {
            if (IsExpired) return 0;
            _dmg = -Mathf.RoundToInt((float)_dmg * 0.5f);
            Debug.LogError("_dmg=" + _dmg);
            return _dmg;
        }

        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }

    /// <summary>
    /// 受到的神智傷害減少50% 隨時間減少層數
    /// </summary>
    public class FortitudeEffect : StatusEffect {
        public class Builder : Builder<FortitudeEffect> { }
        public FortitudeEffect() {
            MyType = EffectType.Fortitude;
        }

        public override int GetBeSanAttackedExtraValue(int _dmg) {
            if (IsExpired) return 0;
            _dmg = -Mathf.RoundToInt((float)_dmg * 0.5f);
            return _dmg;
        }

        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }

    /// <summary>
    /// 解毒
    /// 解除中毒狀態
    /// </summary>
    public class AntidoteEffect : StatusEffect {
        public class Builder : Builder<AntidoteEffect> { }
        public AntidoteEffect() {
            MyType = EffectType.Antidote;
        }

        public override HashSet<EffectType> RemoveEffect() {
            Doer.RemoveEffects(MyType);
            return new HashSet<EffectType> { EffectType.Poison };
        }
    }

    /// <summary>
    /// 恢復理智
    /// 解除恐懼狀態
    /// </summary>
    public class RecoveryEffect : StatusEffect {
        public class Builder : Builder<RecoveryEffect> { }
        public RecoveryEffect() {
            MyType = EffectType.Recovery;
        }

        public override HashSet<EffectType> RemoveEffect() {
            Doer.RemoveEffects(MyType);
            return new HashSet<EffectType> { EffectType.Fear };
        }

    }


    /// <summary>
    /// 強壯
    /// 免疫流血與中毒狀態
    /// </summary>
    public class StrongEffect : StatusEffect {
        public class Builder : Builder<StrongEffect> { }
        public StrongEffect() {
            MyType = EffectType.Strong;
        }

        public override HashSet<EffectType> RemoveEffect() {
            return new HashSet<EffectType> { EffectType.Bleeding, EffectType.Poison };
        }

        public override bool ImmuneStatusEffect(EffectType _type) {
            if (_type == EffectType.Bleeding || _type == EffectType.Poison)
                return true;
            return false;
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }

    /// <summary>
    /// 免疫精神崩潰與恐懼狀態
    /// </summary>
    public class FaithEffect : StatusEffect {
        public class Builder : Builder<FaithEffect> { }
        public FaithEffect() {
            MyType = EffectType.Faith;
        }
        public override HashSet<EffectType> RemoveEffect() {
            return new HashSet<EffectType> { EffectType.Insanity, EffectType.Fear };
        }
        public override bool ImmuneStatusEffect(EffectType _type) {
            if (_type == EffectType.Insanity || _type == EffectType.Fear)
                return true;
            return false;
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }

    /// <summary>
    /// 逃跑
    /// </summary>
    public class FleeEffect : StatusEffect {
        public class Builder : Builder<FleeEffect> { }
        public FleeEffect() {
            MyType = EffectType.Flee;
        }
        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            //待實作逃跑
            return true;
        }
    }

    /// <summary>
    /// 尖刺
    /// 每次受到攻擊會反彈層數的傷害給攻擊者
    /// </summary>
    public class ThornsEffect : StatusEffect {
        public class Builder : Builder<ThornsEffect> { }
        public ThornsEffect() {
            MyType = EffectType.Thorns;
        }
        public override void BeAttackedTrigger(Role _attacker) {
            if (_attacker.IsDead) return;
            _attacker.AddHP(-Stack);
        }
        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }


    /// <summary>
    /// 靈體化
    /// 免疫物理攻擊傷害 
    /// </summary>
    public class EtherealEffect : StatusEffect {
        public class Builder : Builder<EtherealEffect> { }
        public EtherealEffect() {
            MyType = EffectType.Ethereal;
        }

        public override int GetBeAttackedExtraValue(int _dmg) {
            if (IsExpired) return 0;
            return -_dmg;
        }
        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }



    /// <summary>
    /// 失明
    /// 攻擊命中率下降20%，隨時間減少層數
    /// </summary>
    public class BlindnessEffect : StatusEffect {
        public class Builder : Builder<BlindnessEffect> { }
        public BlindnessEffect() {
            MyType = EffectType.Blindness;
        }

        public override float GetAttackHitProbExtraValue() {
            if (IsExpired) return 0;
            return -0.2f;
        }
        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }


    /// <summary>
    /// 專注
    /// 攻擊命中率上升20%，隨時間減少層數
    /// </summary>
    public class FocusEffect : StatusEffect {
        public class Builder : Builder<FocusEffect> { }
        public FocusEffect() {
            MyType = EffectType.Focus;
        }

        public override float GetAttackHitProbExtraValue() {
            if (IsExpired) return 0;
            return 0.2f;
        }
        public override void TimePassStackChange(int _time) {
            AddStack(-_time);
        }

        public override bool ApplyEffect() {
            if (!base.ApplyEffect()) return false;
            MyTarget.ApplyEffect(this);
            return true;
        }
    }


    /// <summary>
    /// 專注
    /// 攻擊命中率上升20%，隨時間減少層數
    /// </summary>
    public class BeastSeveredToeEffect : StatusEffect {
        public class Builder : Builder<BeastSeveredToeEffect> { }
        public BeastSeveredToeEffect() {
            MyType = EffectType.BeastSeveredToe;
            Value = 0;
        }

        public override void OpenTheDoorTrigger() {
            base.OpenTheDoorTrigger();
            Value++;
            if (Value >= 2) {
                Doer.AddHP(Stack);
                Value = 0;
            }

        }
    }


}