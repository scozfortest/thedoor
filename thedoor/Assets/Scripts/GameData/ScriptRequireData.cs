using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public enum ScriptRequireType {
        Gold,//需求消耗金幣
        Point,//需求消耗點數
        NeedSupplies,//需求擁有以下全部道具
        UseSupplies,//需求使用以下全部道具1次
        ConsumeSupplies,//需求消耗以下道具(以下道具都會消失)
        NeedSupplyTags,//需求擁有完全符合以下道具類型的道具
        UseSupplyTags,//需求使用完全符合以下道具類型的其中1個道具1次
        ConsumeSupplyTags,//需求耗盡完全符合以下道具類型的其中一個道具
    }
    public class ScriptRequireData {

        public ScriptRequireType MyType { get; private set; }
        public string Value { get; private set; }

        public ScriptRequireData(ScriptRequireType _type, string _value) {
            MyType = _type;
            Value = _value;
        }
        public bool MeetRequire() {
            var ownedSupplyDatas = GamePlayer.Instance.GetOwnedDatas<OwnedSupplyData>(ColEnum.Supply);

            switch (MyType) {
                case ScriptRequireType.Gold:
                case ScriptRequireType.Point:
                    long needCurrency = long.Parse(Value);
                    long playerCurrency = GamePlayer.Instance.Data.GetCurrency(MyEnum.ParseEnum<Currency>(MyType.ToString()));
                    return playerCurrency >= needCurrency;
                case ScriptRequireType.NeedSupplies:
                case ScriptRequireType.UseSupplies:
                case ScriptRequireType.ConsumeSupplies:
                    HashSet<int> ids = new HashSet<int>();
                    ids = TextManager.GetIntHashSetFromSplitStr(Value, ',');
                    if (ids.Count == 0) return true;
                    foreach (var supply in ownedSupplyDatas) {
                        if (ids.Contains(supply.ID))
                            ids.Remove(supply.ID);
                        if (ids.Count == 0) return true;
                    }
                    return false;
                case ScriptRequireType.NeedSupplyTags:
                case ScriptRequireType.UseSupplyTags:
                case ScriptRequireType.ConsumeSupplyTags:
                    HashSet<string> tags = new HashSet<string>();
                    tags = TextManager.StringSplitToStrHashSet(Value, ',');
                    if (tags.Count == 0) return true;
                    foreach (var supply in ownedSupplyDatas) {
                        var supplyData = SupplyData.GetData(supply.ID);
                        if (supplyData == null) continue;
                        if (supplyData.BelongToTags(tags)) return true;
                    }
                    return false;
                default:
                    WriteLog.LogError("尚未定義的ScriptRequireType :" + MyType);
                    return true;
            }
        }

        public string GetRequireStr() {
            string str = "";
            string content = "";
            string typeStr = StringData.GetUIString("ScriptRequireStr_" + MyType);

            switch (MyType) {
                case ScriptRequireType.Gold:
                    str = string.Format(typeStr, long.Parse(Value));
                    str = string.Format(StringData.GetUIString("ScriptRequireStr_Parentheses"), str);
                    return str;
                case ScriptRequireType.Point:
                    return string.Format(StringData.GetUIString("ScriptRequireStr_Point"), long.Parse(Value));
                case ScriptRequireType.NeedSupplies:
                case ScriptRequireType.UseSupplies:
                case ScriptRequireType.ConsumeSupplies:
                    HashSet<int> ids = new HashSet<int>();
                    ids = TextManager.GetIntHashSetFromSplitStr(Value, ',');
                    if (ids.Count == 0) return str;
                    foreach (var id in ids) {
                        if (content != "") content += StringData.GetUIString("ScriptRequireStr_Comma");
                        content += SupplyData.GetData(id).Name;
                    }
                    str = string.Format(typeStr, content);
                    str = string.Format(StringData.GetUIString("ScriptRequireStr_Parentheses"), str);
                    return str;
                case ScriptRequireType.NeedSupplyTags:
                case ScriptRequireType.UseSupplyTags:
                case ScriptRequireType.ConsumeSupplyTags:
                    HashSet<string> tags = new HashSet<string>();
                    tags = TextManager.StringSplitToStrHashSet(Value, ',');
                    if (tags.Count == 0) return str;
                    foreach (var tag in tags) {
                        if (content != "") content += StringData.GetUIString("ScriptRequireStr_Comma");
                        content += StringData.GetUIString("SupplyTag_" + tag);
                    }
                    str = string.Format(typeStr, content);
                    str = string.Format(StringData.GetUIString("ScriptRequireStr_Parentheses"), str);
                    return str;
                default:
                    WriteLog.LogError("尚未定義的ScriptRequireType :" + MyType);
                    return str;
            }
        }
    }
}