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


        public Dictionary<EffectType, StatusEffect> Effects { get; protected set; } = new Dictionary<EffectType, StatusEffect>();
        public virtual bool IsDead { get { return (CurHP <= 0); } }


        public virtual void AddHP(int _value) {
            WriteLog.LogColor(Name + "HP增加:" + _value, WriteLog.LogType.Battle);
            if (IsDead) return;

            CurHP += _value;
            if (IsDead) OnDeath();
        }


        protected virtual void OnDeath() {
            WriteLog.LogColor(Name + "死亡", WriteLog.LogType.Battle);
        }


        public void AddTimePassDmg(ref int _value, int _time) {
            foreach (var effect in Effects.Values) {
                _value += effect.TimeDmgTaken(_time);
            }
        }
        public void AddTimePassSanDmg(ref int _value, int _time) {
            foreach (var effect in Effects.Values) {
                _value += effect.TimeSanDmgTaken(_time);
            }
        }
        public void AddExtraDmg(ref int _value) {
            foreach (var effect in Effects.Values) {
                _value += effect.AttackExtraDamageDealt();
            }
        }
        public void AddExtraSanDmg(ref int _value) {
            foreach (var effect in Effects.Values) {
                _value += effect.AttackExtraSanDamageDealt();
            }
        }
        /// <summary>
        /// 承受攻擊
        /// </summary>
        public void GetAttacked(int _dmg) {
            if (_dmg == 0) return;
            if (IsDead) return;

            //執行效果
            foreach (var effect in Effects.Values) {
                _dmg -= effect.BeAtteckedExtraDmgTaken();
                _dmg += effect.BeAttackDamageReduction(_dmg);
            }
            if (_dmg > 0) _dmg = 0;
            AddHP(_dmg);
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



            public T Build() {
                return instance;
            }
        }
    }
}