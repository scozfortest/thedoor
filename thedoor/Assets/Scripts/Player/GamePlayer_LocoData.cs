using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System;
using System.Linq;
using LitJson;
using SimpleJSON;

namespace TheDoor.Main {

    public partial class GamePlayer : MyPlayer {
        //在這裡讀寫玩家存在裝置的資料PlayerPrefs

        public void LoadDataFromLoco(LocoDataName _name) {
            string json = LocoDataManager.GetDataFromLoco(_name);
            JSONNode jsNode = JSONNode.Parse(json);

            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (var key in jsNode.Keys) {
                data[key] = jsNode[key].Value;
            }
            GamePlayer.Instance.SetMainPlayerData(data);

        }
    }
}
