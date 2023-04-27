using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public enum MonsterType {
        Normal,
        Boss,
    }
    public class MonsterData : MyJsonData {
        public static string DataName { get; set; }
        public string Name {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Name");
            }
        }
        public string Description {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Description");
            }
        }
        public int Rank { get; private set; }
        public string Ref { get; private set; }
        public MonsterType MyMonsterType { get; private set; }
        public string[] Weakness { get; private set; }

        public int HP { get; private set; }
        public float HeadDmg { get; private set; }
        public float HeadProb { get; private set; }
        public float BodyDmg { get; private set; }
        public float BodyProb { get; private set; }
        public float LimbsDmg { get; private set; }
        public float LimbsProb { get; private set; }

        public static Dictionary<MonsterType, List<MonsterData>> MonsterTypeDic = new Dictionary<MonsterType, List<MonsterData>>();

        public static void ClearStaticDic() {
            MonsterTypeDic.Clear();
        }

        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "Rank":
                        Rank = int.Parse(item[key].ToString());
                        break;
                    case "Ref":
                        Ref = item[key].ToString();
                        break;
                    case "MonsterType":
                        MyMonsterType = MyEnum.ParseEnum<MonsterType>(item[key].ToString());
                        break;
                    case "Weakness":
                        Weakness = item[key].ToString().Split(',');
                        break;
                    case "HP":
                        HP = int.Parse(item[key].ToString());
                        break;
                    case "HeadDmg":
                        HeadDmg = float.Parse(item[key].ToString());
                        break;
                    case "HeadProb":
                        HeadProb = float.Parse(item[key].ToString());
                        break;
                    case "BodyDmg":
                        BodyDmg = float.Parse(item[key].ToString());
                        break;
                    case "BodyProb":
                        BodyProb = float.Parse(item[key].ToString());
                        break;
                    case "LimbsDmg":
                        LimbsDmg = float.Parse(item[key].ToString());
                        break;
                    case "LimbsProb":
                        LimbsProb = float.Parse(item[key].ToString());
                        break;
                    default:
                        WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
            if (MonsterTypeDic.ContainsKey(MyMonsterType))
                MonsterTypeDic[MyMonsterType].Add(this);
            else
                MonsterTypeDic.Add(MyMonsterType, new List<MonsterData>() { this });
        }
        public static MonsterData GetData(int _id) {
            return GameDictionary.GetJsonData<MonsterData>(DataName, _id);
        }
        public RoleAction GetAction(Role _doer, Role _target) {
            var mActionDatas = MonsterActionData.GetMonsterActionDatas(ID);
            var statusEffects = new List<StatusEffect>();
            int time = 0;
            RoleAction action = null;
            for (int i = 0; i < 10; i++) {//跑10次還是沒有產生行動才回傳null
                foreach (var mActionData in mActionDatas) {
                    if (!Prob.GetResult(mActionData.Probability)) continue;
                    Role doer = _doer;
                    var targetEffectDatas = mActionData.MyEffects;
                    if (targetEffectDatas == null || targetEffectDatas.Count == 0) continue;
                    foreach (var effectData in targetEffectDatas) {
                        if (!Prob.GetResult(effectData.Probability)) continue;
                        Role target = (effectData.MyTarget == Target.Myself) ? _doer : _target;
                        var effect = EffectFactory.Create(effectData.EffectType, (int)effectData.GetValue(0), _doer, target);
                        if (effect != null)
                            statusEffects.Add(effect);
                    }
                }
                if (time == 0 || statusEffects == null || statusEffects.Count <= 0) continue;
                if (action != null) break;
            }
            return action;
        }

        public static MonsterData GetRndMonsterData(MonsterType _type) {
            return Prob.GetRandomTFromTList(MonsterTypeDic[_type]);
        }
        public static void GetSprite(int _id, Action<Sprite> _ac) {

            var data = GameDictionary.GetJsonData<MonsterData>(DataName, _id);
            if (data == null) {
                _ac?.Invoke(null);
                return;
            }
            data.GetSprite(sprite => {
                _ac?.Invoke(sprite);
            });
        }
        public void GetSprite(Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(Ref)) {
                _ac?.Invoke(null);
                return;
            }
            AddressablesLoader.GetSpriteAtlas(DataName, atlas => {
                if (atlas != null) {
                    Sprite sprite = atlas.GetSprite(Ref);
                    _ac?.Invoke(sprite);
                }
            });
        }


        public bool WeakTo(params string[] _tags) {
            if (Weakness == null) return false;
            if (_tags == null) return false;
            for (int i = 0; i < _tags.Length; i++) {
                return Weakness.Contains(_tags[i]);
            }
            return false;
        }

        /// <summary>
        /// 取得符合多少標籤
        /// </summary>
        /// <param name="_tags">傳入SupplyData的Tag</param>
        /// <returns>符合標籤的數量</returns>
        public int WeakMatchs(params string[] _tags) {
            if (Weakness == null) return 0;
            if (_tags == null) return 0;
            int matchs = 0;
            for (int i = 0; i < _tags.Length; i++) {
                if (Weakness.Contains(_tags[i])) matchs++;
            }
            return matchs;
        }




    }

}
