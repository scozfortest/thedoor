using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public enum ItemType {
        Gold,
        Point,
        ItemGroup,
        Role,
        Supply,
        //有新增新的類型也要追加
    }
    //非獨立資料類道具，記錄在PlayerData-Item裡的道具都屬於非獨立資料類道具
    public enum NotUniqueItemTypes {
        Supply,
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
    }
}
