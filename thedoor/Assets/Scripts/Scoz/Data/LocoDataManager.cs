using TheDoor.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace Scoz.Func {

    public enum LocoDataName {
        PlayerSetting,//玩家設定
        Player,
        History,
        Role,
        Supply,
        Adventure,
    }
    public class LocoDataManager {
        public static void SaveDataToLoco(LocoDataName _name, string _json) {
            PlayerPrefs.SetString(_name.ToString(), _json);
            WriteLog.LogFormat("<color=Orange>SaveDataToLoco-{0}:   {1}</color>", _name, _json);
        }


        public static string GetDataFromLoco(LocoDataName _name) {
            string json = "";
            if (PlayerPrefs.HasKey(_name.ToString())) {
                json = PlayerPrefs.GetString(_name.ToString());
                WriteLog.LogFormat("<color=Orange>GetDataFromLoco-{0}:   {1}</color>", _name, json);
            } else {
                WriteLog.LogFormat("<color=Orange>No Loco Data-{0}</color>", _name, json);
            }
            return json;
        }
    }
}