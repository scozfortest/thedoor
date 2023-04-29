using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheDoor.Main {
    public abstract class Role {
        public virtual string Name { get; }
        public virtual string Ref { get; }
        public int MaxHP { get; protected set; }
        public int MaxSanP { get; protected set; }
        private int curHP;
        public int CurHP {
            get { return curHP; }
            protected set { curHP = Mathf.Clamp(value, 0, MaxHP); }
        }
        public float HPRatio { get { return (float)CurHP / (float)MaxHP; } }
        private int curSanP;
        public int CurSanP {
            get { return curSanP; }
            protected set { curSanP = Mathf.Clamp(value, 0, MaxSanP); }
        }
        public float SanPRatio { get { return (float)CurSanP / (float)MaxSanP; } }

        public Dictionary<EffectType, StatusEffect> Effects { get; protected set; } = new Dictionary<EffectType, StatusEffect>();
        public bool IsDead { get { return (CurHP <= 0 || CurSanP <= 0); } }


        public void AddHP(int _value) {
            if (_value == 0) return;
            if (IsDead) return;

            CurHP += _value;

            if (IsDead) OnDeath();
        }

        public void AddSanP(int _value) {
            if (_value == 0) return;
            if (IsDead) return;

            CurSanP += _value;

            if (IsDead) OnDeath();
        }
        protected virtual void OnDeath() {
        }


        public int GetTimeDmgTaken(int _time) {
            int value = 0;
            foreach (var effect in Effects.Values) {
                value += effect.TimeDmgTaken(_time);
            }
            return value;
        }
        public int GetTimeSanDmgTaken(int _time) {
            int value = 0;
            foreach (var effect in Effects.Values) {
                value += effect.TimeSanDmgTaken(_time);
            }
            return value;
        }
        public int GetExtraAttackDmg() {
            int value = 0;
            foreach (var effect in Effects.Values) {
                value += effect.AttackExtraDamageDealt();
            }
            return value;
        }
        public int GetExtraAttackSanDmg() {
            int value = 0;
            foreach (var effect in Effects.Values) {
                value += effect.AttackExtraSanDamageDealt();
            }
            return value;
        }
        /// <summary>
        /// 承受攻擊
        /// </summary>
        public void TackenDmgAttacked(int _dmg) {
            if (_dmg == 0) return;
            if (IsDead) return;

            //執行效果
            foreach (var effect in Effects.Values) {
                _dmg += effect.BeAtteckedExtraDmgTaken();
                _dmg -= effect.BeAttackDamageReduction(_dmg);
            }
            AddHP(-_dmg);
            RemoveExpiredEffects();
        }
        /// <summary>
        /// 承受神智攻擊
        /// </summary>
        public void TackenSanDmgAttacked(int _dmg) {
            if (_dmg == 0) return;
            if (IsDead) return;

            //執行效果
            foreach (var effect in Effects.Values) {
                _dmg += effect.BeAtteckedExtraSanDmgTaken();
                _dmg -= effect.BeAttackSanDamageReduction(_dmg);
            }
            AddSanP(-_dmg);
            RemoveExpiredEffects();
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

            public Builder<T> SetMaxSanP(int _maxSanP) {
                instance.MaxSanP = _maxSanP;
                return this;
            }

            public Builder<T> SetCurSanP(int _curSanP) {
                instance.CurSanP = _curSanP;
                return this;
            }

            public T Build() {
                return instance;
            }
        }
    }
}