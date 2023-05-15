using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    /// <summary>
    /// 物品類型
    /// </summary>
    public enum ItemType {
        Gold,
        Point,
        ItemGroup,
        Role,
        Supply,
        //有新增新的類型也要追加
    }
    /// <summary>
    /// 獲得物品類型
    /// </summary>
    public enum GainItemType {
        Gold,
        Point,
        Supply,
        SupplyRank,//Value填道具品階 填0就是全品階
    }
    //非獨立資料類道具，記錄在PlayerData-Item裡的道具都屬於非獨立資料類道具
    public enum NotUniqueItemTypes {

    }
    public class ItemData {
        public ItemType Type { get; private set; }
        public long Value { get; private set; }
        public string Name {
            get {
                string name = "NoName";
                switch (Type) {
                    case ItemType.Gold:
                    case ItemType.Point:
                        name = string.Format("{0}{1}", Value, StringData.GetUIString(Type.ToString()));
                        break;
                    default:
                        var itemJsonData = GameDictionary.GetItemJsonData(Type, (int)Value);
                        name = itemJsonData.Name;
                        break;
                }
                return name;
            }
        }
        public ItemData(ItemType _type, long _value) {
            Type = _type;
            Value = _value;
        }

        public static ItemData GetItemData(GainItemType _gainItemType, string _valueStr) {
            switch (_gainItemType) {
                case GainItemType.Gold:
                    return new ItemData(ItemType.Gold, int.Parse(_valueStr));
                case GainItemType.Point:
                    return new ItemData(ItemType.Point, int.Parse(_valueStr));
                case GainItemType.Supply:
                    return new ItemData(ItemType.Supply, int.Parse(_valueStr));
                case GainItemType.SupplyRank:
                    int rank = int.Parse(_valueStr);
                    SupplyData rndSupplyData = null;
                    if (rank != 0)
                        rndSupplyData = SupplyData.GetRndData(rank, null);
                    if (rndSupplyData != null) return new ItemData(ItemType.Supply, rndSupplyData.ID);
                    else return null;
                default:
                    WriteLog.LogErrorFormat("ItemData.GetItemData有尚未定義的GainItemType=" + _gainItemType);
                    return null;
            }
        }
    }
}
