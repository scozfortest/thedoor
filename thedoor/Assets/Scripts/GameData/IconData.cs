using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class IconData : MyJsonData, IItemJsonData {
        public static string DataName { get; private set; }
        public string Name {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Name");
            }
        }
        public string TypeName {
            get {
                return StringData.GetUIString("Item_" + DataName);
            }
        }
        public ItemType MyItemType { get; } = ItemType.Icon;
        public string Ref { get; private set; }
        public int Rank { get; private set; }
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
                    case "Rank":
                        Rank = int.Parse(item[key].ToString());
                        break;
                    default:
                        DebugLogger.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
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
        public static void GetIconSprite(int _id, Action<Sprite> _ac) {

            var data = GameDictionary.GetJsonData<IconData>(DataName, _id);
            if (data == null) {
                _ac?.Invoke(null);
                return;
            }
            data.GetIconSprite(sprite => {
                _ac?.Invoke(sprite);
            });
        }


    }

}
