using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TheDoor.Main {
    public class MainPlayerData : PlayerData {

        string CurRoleUID;
        public OwnedRoleData CurRole {
            get {
                return GamePlayer.Instance.GetOwnedData<OwnedRoleData>(ColEnum.Role, CurRoleUID);
            }
        }
        public int TotalPurchase { get; private set; }
        public bool Ban { get; private set; } = false;
        public string DeviceUID { get; private set; }

        /// <summary>
        /// 上一場麻將結果，如果為null代表沒有上一場或上一場沒打完
        /// </summary>
        public LastMaJamGameResult MyLastMaJamGameResult { get; private set; } = null;
        public override void SetData(Dictionary<string, object> _data) {
            if (_data == null) {
                WriteLog.LogErrorFormat("設定MainPlayerData時傳入資料為Null");
                return;
            }
            base.SetData(_data);
            object value;

            //目前使用的腳色
            CurRoleUID = _data.TryGetValue("CurRoleUID", out value) ? Convert.ToString(value) : default(string);
            //總購買
            TotalPurchase = _data.TryGetValue("TotalPurchase", out value) ? Convert.ToInt32(value) : default(int);
            //是否被Ban
            Ban = false;
            Ban = _data.TryGetValue("Ban", out value) ? Convert.ToBoolean(value) : false;
            if (Ban) GameStateManager.Instance.InGameCheckCanPlayGame();
            //裝置UID
            DeviceUID = _data.TryGetValue("DeviceUID", out value) ? Convert.ToString(value) : default(string);
            if (GamePlayer.Instance.AlreadSetDeviceUID) {
                if (DeviceUID != SystemInfo.deviceUniqueIdentifier) {
                    PopupUI.ShowClickCancel(StringData.GetUIString("MultipleSignIn"), () => {
                        Application.Quit();
                    });
                }
            }

        }

        /// <summary>
        /// 設定目前使用的腳色UID
        /// </summary>
        /// <param name="_curRoleUID"></param>
        public void SetCurRole_Loco(string _curRoleUID) {
            CurRoleUID = _curRoleUID;
        }



    }
}