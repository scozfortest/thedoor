using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System;
using System.Linq;
using LitJson;

namespace TheDoor.Main {

    public partial class GamePlayer : MyPlayer {

        //在這裡讀寫玩家存在裝置的資料PlayerPrefs

        ////////////////////////////////////////////////PlayerSetting/////////////////////////////////////////////
        public bool PostProcessing { get; private set; } = false;//是否開啟後製效果(預設關閉)
        public bool Vibration { get; private set; } = true;//是否開啟震動效果(預設開啟)
        public void SetPostProcessing(bool _on) {
            PostProcessing = _on;
        }
        public void SetVibration(bool _on) {
            Vibration = _on;
        }
        void LoadPlayerSettingFromLoco() {
            string str = "";
            if (PlayerPrefs.HasKey(LocoData.PlayerSetting.ToString())) {
                str = PlayerPrefs.GetString(LocoData.PlayerSetting.ToString());
            } else {
                return;
            }
            JsonData item = JsonMapper.ToObject(str);
            foreach (string key in item.Keys) {
                switch (key) {
                    case "PostProcessing":
                        PostProcessing = bool.Parse(item[key].ToString());
                        break;
                    case "Vibration":
                        Vibration = bool.Parse(item[key].ToString());
                        break;
                    default:
                        Debug.LogWarning(string.Format("LoadPlayerSettingFromLoco有不明屬性:{0}", key));
                        break;
                }
            }
        }
        string GetPlayerSettingJsonStr() {
            Dictionary<string, object> setting = new Dictionary<string, object>();
            setting.Add("PostProcessing", PostProcessing);
            setting.Add("Vibration", Vibration);
            LocoPlayerSettingData data = new LocoPlayerSettingData(setting);
            JsonData jd = JsonMapper.ToJson(data);
            return jd.ToString();
        }
        public void SavePlayerSettingToLoco() {
            string str = GetPlayerSettingJsonStr();
            DebugLogger.LogFormat("<color=Orange>{0}</color>", str);
            PlayerPrefs.SetString(LocoData.PlayerSetting.ToString(), str);
            DebugLogger.LogFormat("<color=Orange>{0}</color>", "Save PlayerSetting To Loco");
        }



        ////////////////////////////////////////////////RedDot(紅點系統)/////////////////////////////////////////////
        HashSet<string> RedDot_ShopDataUIDs { get; set; } = new HashSet<string>();//已經觀看過的商城品項
        Dictionary<string, HashSet<int>> RedDot_ItemIDs = new Dictionary<string, HashSet<int>>();//已經觀看過的道具品項

        /// <summary>
        /// 將目前展示的商品都加到本機已觀看清單
        /// </summary>
        public void AddRedDot_ShopDataUIDs() {
            List<ShopData> shopDatas = GameData.GetShopDatas();
            if (shopDatas == null || shopDatas.Count == 0)
                return;
            for (int i = 0; i < shopDatas.Count; i++) {
                if (!shopDatas[i].IsReadyToSale) continue;
                AddRedDot_ShopDataUID(shopDatas[i].UID);
            }
        }
        void AddRedDot_ShopDataUID(string _uid) {
            if (!RedDot_ShopDataUIDs.Contains(_uid))
                RedDot_ShopDataUIDs.Add(_uid);
        }

        /// <summary>
        /// 將指定的道具ID加到本機已觀看清單
        /// </summary>
        public void AddRedDot_ItemID(NotUniqueItemTypes _type, int _id) {
            if (!RedDot_ItemIDs.ContainsKey(_type.ToString()))
                RedDot_ItemIDs.Add(_type.ToString(), new HashSet<int>());
            if (!RedDot_ItemIDs[_type.ToString()].Contains(_id)) {
                RedDot_ItemIDs[_type.ToString()].Add(_id);
            }

        }
        void LoadReDotFromLoco() {
            string str = "";
            if (PlayerPrefs.HasKey(LocoData.RedDot.ToString())) {
                str = PlayerPrefs.GetString(LocoData.RedDot.ToString());
            } else {
                return;
            }
            JsonData item = JsonMapper.ToObject(str);
            foreach (string key in item.Keys) {
                switch (key) {
                    case "RedDot_ShopDataUIDs":
                        RedDot_ShopDataUIDs.Clear();
                        for (int i = 0; i < item[key].Count; i++) {
                            if (item[key][i] != null)
                                RedDot_ShopDataUIDs.Add(item[key][i].ToString());
                        }
                        break;
                    case "Icon":
                        if (RedDot_ItemIDs.ContainsKey(key))
                            RedDot_ItemIDs[key].Clear();
                        else
                            RedDot_ItemIDs.Add(key, new HashSet<int>());

                        for (int i = 0; i < item[key].Count; i++) {
                            if (item[key][i] != null)
                                RedDot_ItemIDs[key].Add(int.Parse(item[key][i].ToString()));
                        }
                        break;
                    default:
                        Debug.LogWarning(string.Format("LoadReDotFromLoco有不明屬性:{0}", key));
                        break;
                }
            }
        }
        string GetRedDotJsonStr() {
            Dictionary<string, object> setting = new Dictionary<string, object>();
            setting.Add("RedDot_ShopDataUIDs", RedDot_ShopDataUIDs.ToArray());
            foreach (var key in RedDot_ItemIDs.Keys) {
                setting.Add(key, RedDot_ItemIDs[key].ToArray());
            }
            LocoRedDotData data = new LocoRedDotData(setting);
            JsonData jd = JsonMapper.ToJson(data);
            return jd.ToString();
        }
        public void SaveRedDotToLoco() {
            string str = GetRedDotJsonStr();
            DebugLogger.LogFormat("<color=Orange>{0}</color>", str);
            PlayerPrefs.SetString(LocoData.RedDot.ToString(), str);
            DebugLogger.LogFormat("<color=Orange>{0}</color>", "Save PlayerSetting To Loco");
        }
    }
}
