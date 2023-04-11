using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedMaJamMatchingInvite : OwnedData {
        public string InviterUID { get; private set; }
        public string RoomID { get; private set; }
        public string RoomUID { get; private set; }
        static HashSet<DateTime> AlreadyNotifyList = new HashSet<DateTime>();//已經有跳彈窗通知的會加入這個清單

        public OwnedMaJamMatchingInvite(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            InviterUID = _data.TryGetValue("InviterUID", out value) ? Convert.ToString(value) : default(string);
            RoomID = _data.TryGetValue("RoomID", out value) ? Convert.ToString(value) : default(string);
            RoomUID = _data.TryGetValue("RoomUID", out value) ? Convert.ToString(value) : default(string);
        }

        /// <summary>
        /// 是否需要通知
        /// </summary>
        public bool NeedNotify() {
            if (!AlreadyNotifyList.Contains(CreateTime)) {
                AlreadyNotifyList.Add(CreateTime);
                if ((GameManager.Instance.NowTime - CreateTime).TotalSeconds > 30)//超過30秒就不跳邀請通知了
                    return false;
                return true;
            }
            return false;
        }


    }
}
