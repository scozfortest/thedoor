using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheDoor.Main {
    public abstract class Role {
        public virtual string Name { get; }
        public virtual string Ref { get; }
        public int MaxHP { get; protected set; }
        private int curHP;
        public int CurHP {
            get { return curHP; }
            protected set { curHP = Mathf.Clamp(value, 0, MaxHP); }
        }
        public float HPRatio { get { return (float)CurHP / (float)MaxHP; } }

        public int MaxSanP { get; protected set; }
        private int curSanP;
        public int CurSanP {
            get { return curSanP; }
            protected set { curSanP = Mathf.Clamp(value, 0, MaxSanP); }
        }
        public float SanPRatio { get { return (float)CurSanP / (float)MaxSanP; } }


        public Dictionary<EffectType, StatusEffect> Effects { get; protected set; } = new Dictionary<EffectType, StatusEffect>();
        public bool IsDead { get { return (CurHP <= 0 || CurSanP <= 0); } }


        public virtual void AddHP(int _value) {
            WriteLog.LogColor(Name + "HP增加:" + _value, WriteLog.LogType.Battle);
            if (IsDead) return;
            CurHP += _value;
            if (IsDead) OnDeath();
        }

        public virtual void AddSanP(int _value) { }
        /// <summary>
        /// 承受神智攻擊
        /// </summary>
        public virtual void GetSanAttacked(int _dmg) { }

        protected virtual void OnDeath() {
            WriteLog.LogColor(Name + "死亡", WriteLog.LogType.Battle);
        }


        /// <summary>
        /// 承受攻擊
        /// </summary>
        public virtual void GetAttacked(int _dmg) {
            if (IsDead) return;
            AddHP(-_dmg);
        }



        /// <summary>
        /// 賦予效果
        /// </summary>
        public virtual void ApplyEffect(StatusEffect _effect) {
            if (_effect.IsExpired) return;
            // 檢查是否已經存在相同類型的狀態效果
            if (Effects.ContainsKey(_effect.MyType)) {
                // 如果存在，則更新層數
                Effects[_effect.MyType].AddStack(_effect.Stack);
            } else {
                // 如果不存在，則添加新效果
                Effects.Add(_effect.MyType, _effect);
            }
            _effect.BeAppliedEffectTrigger();
        }
        /// <summary>
        /// 移除狀態效果
        /// </summary>
        public void RemoveEffects(params EffectType[] _types) {
            if (_types == null || _types.Length == 0) return;
            foreach (var type in _types) {
                Effects.Remove(type);
            }
        }
        /// <summary>
        /// 移除過期的效果
        /// </summary>
        public void RemoveExpiredEffects() {
            var keysToRemove = Effects.Keys.Where(key => Effects[key].IsExpired).ToList();
            foreach (var key in keysToRemove) {
                Effects.Remove(key);
            }
        }
        public void DoTimePass(int _time) {
            List<StatusEffect> effects = new List<StatusEffect>(Effects.Values);
            foreach (var effect in effects) {
                if (effect == null) continue;
                effect.TimePassDmgTaken(_time);
                effect.TimePassSanDmgTaken(_time);
                effect.TimePassStackChange(_time);
            }
        }



        protected Role() {
        }

        public abstract class Builder<T> where T : Role, new() {
            protected T instance;

            public Builder() {
                instance = new T();
            }


            public Builder<T> SetMaxHP(int _maxHP) {
                instance.MaxHP = _maxHP;
                return this;
            }

            public Builder<T> SetCurHP(int _curHP) {
                instance.CurHP = _curHP;
                return this;
            }
            public Builder<T> SetCurSanP(int _curSanP) {
                instance.CurSanP = _curSanP;
                return this;
            }
            public Builder<T> SetMaxSanP(int _maxSanP) {
                instance.MaxSanP = _maxSanP;
                return this;
            }


            public T Build() {
                return instance;
            }
        }
    }
}