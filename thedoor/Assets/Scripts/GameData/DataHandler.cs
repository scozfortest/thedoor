using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class DataHandler {
        /// <summary>
        /// 將server回傳的獲得道具清單轉為字典
        /// 回傳字典Key值["ReturnGainItems"]是實際獲得的道具清單
        /// 回傳字典Key值["ReplaceGainItems"]被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單)
        /// </summary>
        public static Dictionary<string, List<ItemData>> ConvertDataObjToReturnItemDic(object _dataObj) {
            Dictionary<string, List<ItemData>> dic = new Dictionary<string, List<ItemData>>();
            try {
                if (_dataObj == null) {
                    dic.Add("ReturnGainItems", null);
                    dic.Add("ReplaceGainItems", null);
                    return dic;
                }

                IDictionary tmpDic = _dataObj as IDictionary;
                List<ItemData> returnGainItems = ConvertDataObjToItemList(tmpDic["ReturnGainItems"]);
                List<ItemData> replaceGainItems = ConvertDataObjToItemList(tmpDic["ReplaceGainItems"]);

                if (returnGainItems != null && returnGainItems.Count > 0)
                    dic.Add("ReturnGainItems", returnGainItems);
                else
                    dic.Add("ReturnGainItems", null);

                if (replaceGainItems != null && replaceGainItems.Count > 0)
                    dic.Add("ReplaceGainItems", replaceGainItems);
                else
                    dic.Add("ReplaceGainItems", null);
            } catch (Exception _e) {
                DebugLogger.LogError("ConvertDataObjToReturnItemDic時發生錯誤");
                DebugLogger.LogError(_e);
            }


            return dic;
        }
        static List<ItemData> ConvertDataObjToItemList(object _dataObj) {
            List<ItemData> items = new List<ItemData>();
            List<object> objList = _dataObj as List<object>;
            if (objList == null)
                return items;
            for (int i = 0; i < objList.Count; i++) {
                var data = DictionaryExtension.ConvertToStringKeyDic(objList[i] as IDictionary);
                if (!data.ContainsKey("ItemType") || !data.ContainsKey("ItemValue"))
                    continue;
                ItemType itemType;
                if (!MyEnum.TryParseEnum(data["ItemType"].ToString(), out itemType))
                    continue;
                ItemData itemData = new ItemData(itemType, Convert.ToInt64(data["ItemValue"]));
                items.Add(itemData);
            }
            /*
            for (int i = 0; i < items.Count; i++) {
                DebugLogger.LogError(items[i].Type + " : " + items[i].Value);
            }
            */
            return items;
        }
    }
}