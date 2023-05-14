using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class SupplyData : MyJsonData, IItemJsonData {
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

        public string EffectDescription {
            get {
                string description = "";
                var effects = GetSupplyEffects();
                if (effects == null) return description;
                foreach (var e in effects) {
                    description += e.Description;
                }
                return description;
            }
        }
        public bool Exclusive { get; private set; }
        public Target MyTarget { get; private set; }

        public ItemType MyItemType { get; } = ItemType.Supply;
        public string Ref { get; set; }
        public bool Lock { get; private set; }
        public int Rank { get; private set; }
        public int ExtendHP { get; private set; }
        public int ExtendSanP { get; private set; }
        public int Usage { get; private set; }
        public int Time { get; private set; }
        HashSet<string> Tags;//道具分類
        HashSet<Timing> Timings;//使用時機
        public enum Timing {
            Battle,
            Rest,
        }


        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "Ref":
                        Ref = item[key].ToString();
                        break;
                    case "Tags":
                        Tags = TextManager.GetHashSetFromSplitStr(item[key].ToString(), ',');
                        break;
                    case "Target":
                        MyTarget = MyEnum.ParseEnum<Target>(item[key].ToString());
                        break;
                    case "Exclusive":
                        Exclusive = bool.Parse(item[key].ToString());
                        break;
                    case "Lock":
                        Lock = bool.Parse(item[key].ToString());
                        break;
                    case "Rank":
                        Rank = int.Parse(item[key].ToString());
                        break;
                    case "ExtendHP":
                        ExtendHP = int.Parse(item[key].ToString());
                        break;
                    case "ExtendSanP":
                        ExtendSanP = int.Parse(item[key].ToString());
                        break;
                    case "Timing":
                        Timings = TextManager.GetEnumHashSetFromSplitStr<Timing>(item[key].ToString(), ',');
                        break;
                    case "Usage":
                        Usage = int.Parse(item[key].ToString());
                        break;
                    case "Time":
                        Time = int.Parse(item[key].ToString());
                        break;
                    default:
                        WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
            if (Usage == 0) Usage = -1;//沒有填使用次數就是無限次數
        }
        public void GetIconSprite(Action<Sprite> _ac) {
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
        public PlayerAction GetAction(PlayerRole _doer, Role _target, AttackPart _attackPart) {
            var supplyEffectDatas = GetSupplyEffects();
            var statusEffects = new List<StatusEffect>();
            foreach (var supplyEffectData in supplyEffectDatas) {
                Role doer = _doer;
                Role target = (supplyEffectData.MyTarget == Target.Myself) ? _doer : _target;//效果目標
                var targetEffectDatas = supplyEffectData.MyEffects;
                if (targetEffectDatas == null || targetEffectDatas.Count == 0) continue;
                foreach (var effectData in targetEffectDatas) {
                    var effect = EffectFactory.Create(effectData.Probability, effectData.EffectType, (int)effectData.Value, _doer, target, _attackPart);
                    if (effect != null)
                        statusEffects.Add(effect);
                }
            }
            Role actionTarget = (MyTarget == Target.Myself) ? _doer : _target;//行動目標
            return new PlayerAction(Name, _doer, actionTarget, Time, statusEffects, _attackPart);
        }
        List<SupplyEffectData> GetSupplyEffects() {
            return SupplyEffectData.GetSupplyEffectDatas(ID);
        }
        public bool BelongToTiming(Timing _timing) {
            if (Timings == null) return false;
            return Timings.Contains(_timing);
        }
        public bool BelongToTag(string _tag) {
            if (Tags == null) return false;
            if (string.IsNullOrEmpty(_tag)) return true;
            return Tags.Contains(_tag);
        }
        /// <summary>
        /// 完全符合才會返回true
        /// </summary>
        public bool BelongToTags(HashSet<string> _tags) {
            if (Tags == null) return false;
            if (_tags == null) return true;
            foreach (var tag in _tags) {
                if (!Tags.Contains(tag)) return false;
            }
            return true;
        }
        /// <summary>
        /// 符合一項就會返回true
        /// </summary>
        public bool ContainTags(HashSet<string> _tags) {
            if (Tags == null) return false;
            if (_tags == null) return false;
            foreach (var tag in _tags) {
                if (Tags.Contains(tag)) return true;
            }
            return false;
        }

        public static SupplyData GetData(int _id) {
            return GameDictionary.GetJsonData<SupplyData>("Supply", _id);
        }
        public static List<SupplyData> GetRoleUnarmedDatas(RoleData _roleData) {
            List<SupplyData> datas = new List<SupplyData>();
            foreach (var id in _roleData.Unarmeds) {
                var data = GetData(id);
                if (data == null) continue;
                datas.Add(data);
            }
            return datas;
        }
        public static List<SupplyData> GetRndDatas(int _count, int _rank, HashSet<string> _exclusiveTags) {
            var supplyDic = GameDictionary.GetIntKeyJsonDic<SupplyData>("Supply");
            var supplyDatas = supplyDic.Values.ToList().FindAll(a => {
                if (a.Exclusive) return false;
                if (a.Rank != _rank) return false;
                if (_exclusiveTags != null && _exclusiveTags.Count != 0)
                    if (a.ContainTags(_exclusiveTags)) return false;
                return true;
            });
            return Prob.GetRandomTFromTList(supplyDatas, _count);
        }

        public static Dictionary<string, object> GetJsonDataDic(SupplyData _data) {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("UID", GamePlayer.Instance.GetNextUID("Supply"));
            dic.Add("OwnerUID", GamePlayer.Instance.Data.UID);
            dic.Add("CreateTime", GameManager.Instance.NowTime);

            dic.Add("ID", _data.ID);
            dic.Add("Usage", _data.Usage);
            dic.Add("OwnRoleUID", GamePlayer.Instance.Data.CurRoleUID);
            return dic;
        }


    }

}
