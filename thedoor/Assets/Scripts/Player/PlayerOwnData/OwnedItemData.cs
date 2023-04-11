using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedItemData : OwnedData {

        Dictionary<NotUniqueItemTypes, Dictionary<int, int>> Datas = new Dictionary<NotUniqueItemTypes, Dictionary<int, int>>();


        public OwnedItemData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            Datas.Clear();

            //object value;
            foreach (var key in _data.Keys) {
                NotUniqueItemTypes type;
                if (!Enum.TryParse(key, out type))//key值無法轉為NotUniqueItemTypes就代表不是已定義的Item類型
                    continue;

                Dictionary<int, int> itemDic = DictionaryExtension.ConvertToIntKeyValueDic(_data[key] as IDictionary);
                if (itemDic == null)
                    continue;
                if (Datas.ContainsKey(type))
                    Datas[type] = itemDic;
                else
                    Datas.Add(type, itemDic);
            }
        }
        /// <summary>
        /// 取得TypeItemDic(玩家擁有非獨立資料類道具的字典，格式參考為:
        /// [表格ID]:[數量]
        /// </summary>
        public Dictionary<int, int> GetItemDic(NotUniqueItemTypes _type) {
            if (!Datas.ContainsKey(_type))
                return null;
            return Datas[_type];
        }
        /// <summary>
        /// 傳入道具類型與ID，取得玩家擁有此ID道具的數量
        /// </summary>
        public int GetItemCount(NotUniqueItemTypes _type, int _id) {
            var typeItemDic = GetItemDic(_type);
            if (typeItemDic != null && typeItemDic.ContainsKey(_id))
                return typeItemDic[_id];
            return 0;
        }

    }
}
